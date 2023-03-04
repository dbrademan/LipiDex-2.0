using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using csgoslin;
using CSMSL;
using CSMSL.IO.Thermo;

namespace LipiDex_2._0.SpectrumSearcher;

public class SpectrumSearcher
{
	private List<string> mgfFiles; // List of mgf files 
	private List<string> mzxmlFiles; // List of mzxml files 
	private List<string> rawFiles; // Filepaths of mass spec rawfiles
	private List<string> lipidLibs; // Filepaths of spectral libraries

	private List<LibrarySpectrum> librarySpectra = new(); // Library spectra
	private List<SampleSpectrum> sampleMs2Spectra = new(); // List of experimental MSn spectra

	public static List<FattyAcid> fattyAcidsDB = new(); // Fatty acid objects read from csv file
	public static Dictionary<string, TransitionType> transitionTypes; // List of all class+adduct combos from active library
	public static List<string> transitionTypeStrings = new(); // List of all types of ms2 transitions possible

	private double ms1Tol; // MS1 search tolerance in Th
	private double ms2Tol; // MS2 search tolerance in Th
	private int maxResults; // Maximum number of search results returned

	// MassBin objects are deprecated with custom precursorMZ binary search logic 
	// private int[] countBin;								// Array which holds the number of spectra in each bin
	// private int[] addedBin; 								// Array storing the number of spectra added
	// private LibrarySpectrumOld[][] massBin; 				// 2D array to store spectra objects
	// private double maxMass = 999.0;						// Highest precursor mass from positive spectra
	// private double minMass = 0.0;						// Highest precursor mass from positive spectra
	// private double massBinSize;							// Size of each mass bin

	public double IntensityCosineWeighting = 1.2; // Weight for dot product calculations
	public double MassCosineWeighting = 0.9; // Weight for dot product calculations
	private double MinMs2Mass = 60.0; // Minimum mass for spectral searching

	public int numSpectra; // Total count of spectra

	public LipidParser
		GoslinLipidParser =
			new(); // Parser for lipid names. NOTE: ONLY CREATE ONE INSTANCE OF THIS OBJECT. INSTANTIATING THIS OBJECT IS VERY TIME CONSUMING

	private double MinLibraryPrecursorMz; // Minimum precursor MZ in all library spectra
	private double MaxLibraryPrecursorMz; // Max precursor MZ in all library spectra

	// Constructor
	public SpectrumSearcher(List<string> lipidLibs,
		List<string> mgfFiles,
		List<string> mzxmlFiles,
		double ms1Tol,
		double ms2Tol,
		int maxResults,
		double minMassCutoff)
	{
		this.MinMs2Mass = minMassCutoff;
		this.lipidLibs = lipidLibs;
		this.ms1Tol = ms1Tol;
		this.ms2Tol = ms2Tol;
		this.mgfFiles = mgfFiles;
		this.mzxmlFiles = mzxmlFiles;
		this.maxResults = maxResults;
		numSpectra = 0;
	}

	// Entry point for Spectrum Searcher logic
	public void RunSpectraSearch(double mzTol)
	{
		// Read fatty acids
		ReadFattyAcids("src\\backup\\FattyAcids.csv");

		//Read in MSP Files
		try
		{
			foreach (var filepath in lipidLibs)
			{
				librarySpectra.AddRange(
					MspFileParser.MspFileReader(filepath, GoslinLipidParser, transitionTypes));
			}


			foreach (var librarySpectrum in librarySpectra)
			{
				librarySpectrum.ValidateEntry(GoslinLipidParser);
			}
		}
		catch (Exception e)
		{
			throw new Exception("Error loading library .msp", e);
		}

		// Sort arrays by precursor MZ. Necessary for binary search.
		librarySpectra.Sort();

		MinLibraryPrecursorMz = librarySpectra.First().PrecursorMz; // If you ain't first...
		MaxLibraryPrecursorMz = librarySpectra.Last().PrecursorMz; // ... You're last

		// Iterate through all sample spectra in all raw files.
		//     Find precursor mz matches, and get cosine score
		foreach (var rawfilepath in rawFiles)
		{
			var rawfile = new ThermoRawFile(rawfilepath);

			for (int i = rawfile.FirstSpectrumNumber; i > rawfile.LastSpectrumNumber; i++)
			{
				int MsnOrder = rawfile.GetMsnOrder(i);
				if (MsnOrder < 2)
				{
					continue;
				}

				double rt = rawfile.GetRetentionTime(i);
				string polarity = rawfile.GetPolarity(i) == Polarity.Positive ? "+" : "-";
				// For precursor MZ, do we always want the isolated MS1 mz, no matter if we are MS2, or MS3? 
				double precursorMz = rawfile.GetPrecursorMz(i, MsnOrder);
				var sampleSpectrum = new SampleSpectrum(precursorMz, polarity, rawfilepath, rt, i);

				var peakList = rawfile
					.GetSpectrum(i)
					.FilterByMZ(minMZ: MinMs2Mass, maxMZ: 9999999999.9);
				var basePeakIntensity = peakList.GetBasePeakIntensity();
				peakList
					.FilterByIntensity(minIntensity: basePeakIntensity *
					                                 0.005); // Removes the lowest 0.5% intensity peaks. Same as LipiDex 1.

				foreach (var peak in peakList)
				{
					sampleSpectrum.AddPeak(peak.MZ, peak.Intensity);
				}

				sampleSpectrum.scaleIntensities();


				// Find library spectra that match the precursorMz, polarity and MsnOrder
				var librarySpectralMatches = LibraryBinarySearch(precursorMz, MsnOrder, polarity);

				foreach (var librarySpectrum in librarySpectralMatches)
				{
					// Calculate Forward Dot Product
					var forwardDotProduct = sampleSpectrum.CalculateDotProduct(
						librarySpectrum.MzIntensityCommentList, ms2Tol, false,
						MassCosineWeighting, IntensityCosineWeighting);
					// Calculate Reverse Dot Product
					var reverseDotProduct = sampleSpectrum.CalculateDotProduct(
						librarySpectrum.MzIntensityCommentList, ms2Tol, true,
						MassCosineWeighting, IntensityCosineWeighting);

					if (forwardDotProduct > 1) // Dot products are scaled from 0-999, so >1 is a low bar 
					{
						sampleSpectrum.AddId(librarySpectrum, forwardDotProduct, reverseDotProduct, ms2Tol);
					}

				}

				sampleMs2Spectra.Add(sampleSpectrum);
			}
		}

		// Calculate spectral purity using the Spectral Deconvolution Workflow (LipiDex Cell paper, Fig. S6) 
		foreach (var sampleSpectrum in sampleMs2Spectra)
		{
			sampleSpectrum.CalculateSpectralPurity(fattyAcidsDB, mzTol: mzTol);
		}

		// Write Spectrum Search results to CSV
		ResultsWriter.WriteResults(sampleMs2Spectra);

	}

	/// <summary>
	/// Returns list of MspEntries that match precursorMz within tolerance and have
	/// correct MSnOrder and polarity.
	/// Returns empty list if no spectra found.
	///
	/// Does not perform cosine scoring. 
	/// </summary>
	/// <param name="searchMz"></param>
	/// <param name="MsnOrder"></param>
	/// <param name="polarity"></param>
	/// <returns></returns>
	private List<LibrarySpectrum> LibraryBinarySearch(double searchMz, int MsnOrder, string polarity)
	{
		var matches = new List<LibrarySpectrum>();

		double highMz = searchMz + ms1Tol;
		double lowMz = searchMz - ms1Tol;

		// return if the searchMz is outside of the range of the library
		if (highMz < MinLibraryPrecursorMz || lowMz > MaxLibraryPrecursorMz)
		{
			return matches;
		}

		var subset = librarySpectra
			.Where(x => x.Polarity == polarity && x.MSnOrder == MsnOrder)
			.ToList();

		double left = 0;
		double right = subset.Count - 1;
		int indexFoundMatch;

		while (left <= right)
		{
			int middle = (int)Math.Floor((left + right) / 2);
			double precursorMz = subset[middle].PrecursorMz;

			if (lowMz < precursorMz && precursorMz < highMz) // True if a library spectrum within tolerance is found
			{
				indexFoundMatch = middle;
				matches.Add(subset[indexFoundMatch]);

				// Once a precursorMZ within the range is found, search upward and downward in the list
				//    to exhaustively find all the other library entries in the MZ range

				// upward and downwardIndex are used to search the library from the first found match.
				//     upwardIndex looks upward in the list until precursorMz is out of range. 
				int upwardIndex = indexFoundMatch + 1;
				int downwardIndex = indexFoundMatch - 1;

				while (upwardIndex < subset.Count && subset[upwardIndex].PrecursorMz < highMz)
				{
					matches.Add(subset[upwardIndex]);
					upwardIndex += 1;
				}

				while (downwardIndex >= 0 && subset[downwardIndex].PrecursorMz > lowMz)
				{
					matches.Add(subset[downwardIndex]);
					downwardIndex -= 1;
				}

				return matches;
			}

			if (subset[middle].PrecursorMz < searchMz)
			{
				left = middle + 1;
			}
			else if (subset[middle].PrecursorMz > searchMz)
			{
				right = middle - 1;
			}
		}

		return matches; // If it gets to here, then no matches were found 
	}

	/// <summary>
	/// LipiDex 1 included a "FattyAcids.csv" file that gave:
	///		1. Fatty acid name (e.g. 18:2, O-16:0, P-18:1, d16:1)
	///		2. Base (one of Plasmanyl, Plasmenyl, Sphingoid, Alkyl)
	///		3. Formula (e.g. C13H27O1)
	///		4. Enabled (true or false flag whether to use)
	///
	/// This method parses that file. 
	/// </summary>
	/// <param name="filename"></param>
	private static void ReadFattyAcids(string filename)
	{
		string line = null;
		string[] split = null;
		string name;
		string type;
		string enabled;
		string formula;
		HashSet<string> uniqueFaTypes = new();

		using var reader = new StreamReader(filename);
		//Clear FA DB
		fattyAcidsDB = new List<FattyAcid>();

		
		while ((line = reader.ReadLine()) != null)
		{
			if (line.Contains("Name")) continue; // Skip the header 

			split = line.Split(',');

			name = split[0];
			type = split[1];
			formula = split[2];
			enabled = split[3];

			fattyAcidsDB.Add(new FattyAcid(name, type, formula, enabled));

			uniqueFaTypes.Add(type);
		}

		//Create transition type objects
		transitionTypes = CreateTransitionTypes(uniqueFaTypes.ToList());

		//Populate string array
		foreach (var t in transitionTypes)
		{
			transitionTypeStrings.Add(t.ToString());
		}
	}

	//Returns array of all transition type definition objects for given fatty acids
	private static Dictionary<string, TransitionType> CreateTransitionTypes(List<string> typeArray)
	{
		var definitions = new Dictionary<string, TransitionType>();

		// Create static fragment class
		definitions["Fragment"] = new TransitionType("Fragment", null, false, false, 0);

		// Create static neutral loss class
		definitions["Neutral Loss"] = new TransitionType("Neutral Loss", null, false, true, 0);

		// For all fatty acid type strings
		for (int i = 0; i < typeArray.Count; i++)
		{
			// Create moiety fragment class
			var fragmentName = typeArray[i] + " Fragment";
			definitions[fragmentName] = new TransitionType(fragmentName, typeArray[i], true, false, 1);

			// Create moiety neutral loss class
			var neutralLossName = typeArray[i] + " Neutral Loss";
			definitions[neutralLossName] = new TransitionType(neutralLossName, typeArray[i], true, true, 1);
		}

		// Create cardiolipin DG fragment class
		definitions["Cardiolipin DG Fragment"] = new TransitionType("Cardiolipin DG Fragment", "Alkyl", true, false, 2);

		// Create PUFA fragment class
		definitions["PUFA Fragment"] = new TransitionType("PUFA Fragment", "PUFA", true, false, 1);

		// Create PUFA neutral loss class
		definitions["PUFA Neutral Loss"] = new TransitionType("PUFA Neutral Loss", "PUFA", true, true, 1);

		return definitions;
	}

	// // Read and parse .MSP libraries 
	// public void ReadMsp() 
	// {
	// 	foreach (var filepath in lipidLibs)
	// 	{
	// 		var mspReader = MspFileParser.MspFileReader(filepath, GoslinLipidParser);
	//
	// 		#region LipiDex1 MSP parsing logic
	//
	// 		// StreamReader reader = new StreamReader(filepath);
	// 		// while ((line = reader.ReadLine()) != null)
	// 		// {
	// 		// 	//read in retention time
	// 		// 	if (line.Contains("Name:") || line.Contains("NAME:"))
	// 		// 	{
	// 		// 		//Add entry
	// 		// 		if (precursor>0.0) 
	// 		// 			addSpectrum(entryTemp);
	// 		//
	// 		// 		//Erase entry
	// 		// 		polarity = "";
	// 		// 		precursor = 0.0;
	// 		// 		name = "";
	// 		// 		fragType = "";
	// 		// 		peakStart = false;
	// 		// 		isLipidex = false;
	// 		// 		optimalPolarity = false;
	// 		//
	// 		// 		if (line.Contains("]+")) polarity = "+";
	// 		// 		else polarity = "-";
	// 		//
	// 		// 		name = line.Substring(line.IndexOf(":")+1);
	// 		// 	}
	// 		//
	// 		// 	//read in optimal polarity
	// 		// 	if (line.Contains("OptimalPolarity=true"))
	// 		// 	{
	// 		// 		optimalPolarity = true;
	// 		// 	}
	// 		//
	// 		// 	if (line.Contains("LipiDex")) isLipidex = true;
	// 		//
	// 		// 	if (line.Contains("Num Peaks:"))
	// 		// 	{
	// 		// 		peakStart = true;
	// 		// 		entryTemp = new LibrarySpectrumOld(
	// 		// 			precursor, polarity, name, lipidLibs[i].getName(), 
	// 		// 			isLipidex, optimalPolarity, GoslinLipidParser);
	// 		// 	}
	// 		//
	// 		// 	if (peakStart && line.Contains(".") && !line.Contains("Num"))
	// 		// 	{
	// 		// 		if (line.Contains("	")) 
	// 		// 			split = line.Split("	");
	// 		// 		else 
	// 		// 			split = line.Split(" ");
	// 		//
	// 		// 		if (isLipidex)
	// 		// 			fragType = line.Substring(line.IndexOf("\"")+1,line.LastIndexOf("\""));
	// 		//
	// 		// 		if (precursor-Double.Parse(split[0])>2.0)
	// 		// 			entryTemp.AddPeak(Double.Parse(split[0]), Double.Parse(split[1]), fragType, GetTransitionType(fragType));
	// 		//
	// 		// 	}
	// 		//
	// 		// 	if (line.Contains("PRECURSORMZ:"))
	// 		// 	{
	// 		// 		precursor = Double.Parse(line.Substring(line.LastIndexOf(" ")+1));
	// 		// 	}
	// 		// }
	// 		//
	// 		// addSpectrum(entryTemp);
	// 		// reader.close();
	//
	// 		#endregion
	// 	}
	// }

	// Return transition type object corresponding to provided string
	// private static TransitionType GetTransitionType(string s)
	// {
	// 	for (int i = 0; i < transitionTypes.Count; i++)
	// 	{
	// 		if (transitionTypes[i].name.Equals(s)) 
	// 			return transitionTypes[i];
	// 	}
	// 	return null;
	// }

	//Find the minimum value of a range needed to search based on a precursor mass
	public double FindMinMassRange(double mass, double mzTol)
	{
		return (mass - mzTol);
	}

	//Find the maximum value of a range needed to search based on a precursor mass
	public double FindMaxMassRange(double mass, double mzTol)
	{
		return (mass + mzTol);
	}

	//Calculate the difference between two masses in ppm
	public double CalcPPMDiff(double mass1, double mass2)
	{
		return (Math.Abs(mass1 - mass2) / (mass2)) * 1000000;
	}

	//Find the minimum value of a range needed to search based on a precursor mass
	public double FindMinPPMRange(double mass, double ppm)
	{
		return (mass - ((ppm / 1000000.0) * mass));
	}

	//Find the maximum value of a range needed to search based on a precursor mass
	public double FindMaxPPMRange(double mass, double ppm)
	{
		return (mass + ((ppm / 1000000.0) * mass));
	}





	#region Parse MGF
	//Parse MGF file
	// public void readMGF(string filename)  // throws IOException
	// {
	// 	string line = "";					//String for reading in .mgf
	// 	string polarity = "";				//Polarity of ms2
	// 	double precursor = 0.0;				//Precursor mass
	// 	SampleSpectrum specTemp = null;		//Stores ms2 to be added
	// 	string[] split;						//String array for parsing transitions
	// 	double retention = 0.0;				//Retention time in minutes
	//
	// 	BufferedReader reader = new BufferedReader(new FileReader(filename));
	// 	File file = new File(filename);
	//
	// 	//read line if not empty
	// 	while ((line = reader.readLine()) != null)
	// 	{
	// 		if (!line.startsWith("#"))
	// 		{
	// 			if (line.Contains("PEPMASS="))
	// 			{
	//
	// 				if (line.Contains(" ")) precursor = Double.Parse(line.Substring(line.IndexOf("=")+1,line.LastIndexOf(" ")));
	// 				else precursor = Double.Parse(line.Substring(line.IndexOf("=")+1));
	// 			}
	// 			else if (line.Contains("RTINSECONDS"))
	// 			{
	// 				if (line.Contains(" ")) retention = (Double.Parse(line.Substring(line.IndexOf("=")+1,line.LastIndexOf(" "))))/60.0;
	// 				else retention = Double.Parse(line.Substring(line.IndexOf("=")+1))/60.0;
	// 			}
	// 			else if (line.Contains("CHARGE"))
	// 			{
	// 				//peakStart = true;
	//
	// 				if (line.Contains("-")) polarity = "-";
	// 				else polarity = "+";
	// 			}
	// 			else if (line.Contains("END IONS"))
	// 			{
	// 				numSpectra++;
	//
	// 				if (specTemp!= null) sampleMS2Spectra.Add(specTemp);
	//
	// 				specTemp = null;
	// 				polarity = "+";
	// 				precursor = 0.0;
	// 				retention = 0.0;
	// 			}
	// 			else if (line.Contains(".") && !line.Contains("PEPMASS") && !line.Contains("CHARGE")  && !line.Contains("TITLE"))
	// 			{
	// 				split = line.Split(" ");
	//
	// 				if (specTemp == null)
	// 				{
	// 					specTemp = new SampleSpectrum(precursor,polarity,file.getName(),retention,numSpectra);
	// 				}
	//
	// 				if ((precursor-Double.Parse(split[0]))>1.5 
	// 						&& Double.Parse(split[0])>minMS2Mass 
	// 						&& Double.Parse(split[1])>1.0)
	// 				{
	// 					specTemp.AddPeak(Double.Parse(split[0]), Double.Parse(split[1]));
	// 				}
	// 			}
	// 		}
	// 	}
	// 	reader.close();
	// }

	#endregion

	// Bypass the LipiDex 1 mass binning and use a custom Binary Search method instead
	// binMasses();  // Bin MSP LibrarySpectra
	//
	//Read in all mgf files
	// for (int i=0; i<mgfFiles.Count; i++)
	// {
	// 	// timeStart = System.nanoTime();
	// 	numSpectra = 0;
	//
	// 	//Read in spectra and search
	// 	readMGF(mgfFiles[i]);
	//
	// 	//Iterate through spectra and match
	// 	for (int j=0; j<sampleMS2Spectra.Count; j++)
	// 	{
	// 		matchLibrarySpectra(sampleMS2Spectra[j], massBinSize, ms1Tol, ms2Tol);
	// 	}
	//
	// 	//Calculate purity values
	// 	for (int j=0; j<sampleMS2Spectra.Count; j++)
	// 	{			
	// 		sampleMS2Spectra[j].CalculateSpectralPurity(fattyAcidsDB, mzTol);
	// 	}
	//
	// 	//Write ouput files for mgf
	// 	try
	// 	{
	// 		writeResults(sampleMS2Spectra,mgfFiles[i],maxResults);
	// 	}
	// 	catch (IOException e)
	// 	{
	// 		var ce = new ErrorMessageBox($"Please close {mgfFiles[i]} and re-search the data");
	// 	}
	//
	// 	//Clear sample spectra
	// 	sampleMS2Spectra = new List<SampleSpectrum>();
	//
	// }
	//
	//Create mzxmlparser
	// MZXMLParser parser = new MZXMLParser();
	//
	//Read in mzxml files
	// for (int i=0; i<mzxmlFiles.Count; i++)
	// {
	// 	timeStart = Stopwatch.GetTimestamp();
	// 	numSpectra = 0;
	//
	// 	//Parse MZXML
	// 	parser.readFile(mzxmlFiles[i]);
	//
	// 	//Create SampleSpectrum objects
	// 	for (int j=0; j<parser.sampleSpecArray.Count; j++)
	// 	{
	// 		sampleMS2Spectra.Add(parser.sampleSpecArray[j]);
	// 	}
	//
	// 	//Iterate through spectra and match
	// 	for (int j=0; j<sampleMS2Spectra.Count; j++)
	// 	{
	// 		matchLibrarySpectra(sampleMS2Spectra[j], massBinSize, ms1Tol, ms2Tol);				
	// 	}
	//
	// 	//Calculate purity values
	// 	for (int j=0; j<sampleMS2Spectra.Count; j++)
	// 	{			
	// 		sampleMS2Spectra[j].CalculateSpectralPurity(fattyAcidsDB, mzTol);
	// 	}
	//
	// 	//Write ouput files for mzxml
	// 	writeResults(sampleMS2Spectra,mzxmlFiles[i],maxResults);
	//
	// 	//Clear sample spectra
	// 	sampleMS2Spectra = new List<SampleSpectrum>();
	//
	// 	updateProgress((int)( i+1 /mgfFiles.Count*100.0 ),"% - Searching Spectra",true);
	// }
	// updateProgress(100,"% - Completed",true);

	// public void writeResults(List<SampleSpectrum> spectra, string mgfFilename, int maxResults) // throws FileNotFoundException
	// {
	// 	//write results to folder where .mgf file is
	// 	File mgfFile = new File(mgfFilename);
	// 	string parentFolder = mgfFile.getParent();
	// 	string resultFileName = mgfFile.getName().Substring(0,mgfFile.getName().LastIndexOf("."))+"_Results.csv";
	//
	// 	//Check if results exists and if not create results folder in mgf file folder
	// 	File resultsDir = new File(parentFolder);
	// 	if (!resultsDir.exists()) resultsDir.mkdir();
	//
	// 	PrintWriter pw = new PrintWriter(resultsDir.ToString()+"\\"+resultFileName);
	//
	// 	pw.println("MS2 ID,Retention Time (min),Rank,Identification,"
	// 			+ "Precursor Mass,Library Mass,Delta m/z,Dot Product,Reverse Dot Product,Purity,Spectral Components,Optimal Polarity,LipiDex Spectrum,Library,Potential Fragments");
	//
	// 	for (int i=0; i<spectra.Count; i++)
	// 	{
	// 		if (spectra[i].IdentificationsList.Count > 0)
	// 		{ 
	// 			if(spectra[i].IdentificationsList[0].DotProduct>1)
	// 			{
	// 				pw.println(spectra[i].toString(maxResults));
	// 			}
	// 		}
	// 	}
	//
	// 	pw.close();
	// }


	// Create array of bins in which to store objects
	// Bins are indexed by precursor mass
	// public  void createBins(int arraySize)
	// {
	// 	//Check if any spectra from each polarity has been created
	// 	if (librarySpectra.Count>0)
	// 	{
	// 		countBin = new int[arraySize]; //Array to store number of positive spectra stored in each bin
	// 		addedBin = new int[arraySize]; //Array to store number of positive spectra stored in each bin
	//
	// 		//Fill all positive bins with zeroes
	// 		for (int i=0; i<countBin.Length; i++)
	// 		{
	// 			countBin[i] = 0;
	// 			addedBin[i] = 0;
	// 		}
	//
	// 		//Create the array to store the actual spectra objects
	// 		massBin = new LibrarySpectrumOld[arraySize][];
	// 	}
	// }

	//Convert a precursor mass to the correct bin number
	// public  int findBinIndex(double precursor, double binSize, double minMass)
	// {
	// 	return (int)((precursor-minMass)/binSize);
	// }
	//
	// //Calculate the correct array size based on a mass range
	// public int calcArraySize(double binSize, double minMass, double maxMass)
	// {
	// 	return (int)((maxMass - minMass)/binSize)+1;
	// }
	//
	// //A method to bin all precursor masses
	// public  void binMasses()
	// {
	// 	//Create arrays
	// 	createBins(calcArraySize(massBinSize,minMass, maxMass));
	//
	// 	//Check if any spectra from each polarity has been created
	// 	if (librarySpectra.Count>0)
	// 	{
	// 		//Populate count array to correctly initialize array size for positive library spectra
	// 		for (int i=0; i<librarySpectra.Count; i++)
	// 		{
	// 			countBin[findBinIndex(librarySpectra[i].PrecursorMz, massBinSize,minMass)] ++;
	// 		}
	// 		updateProgress(17,"% - Creating Composite Library",true);
	//
	// 		//Use count bin to initialize new arrays to place positive spectra into hash table
	// 		for (int i=0; i<countBin.Length; i++)
	// 		{
	// 			massBin[i] = new LibrarySpectrumOld[countBin[i]];
	// 		}
	// 		updateProgress(33,"% - Creating Composite Library",true);
	//
	//
	// 		//Populate spectrum arrays for positive spectra
	// 		for (int i=0; i<librarySpectra.Count; i++)
	// 		{
	// 			massBin[findBinIndex(librarySpectra[i].PrecursorMz, massBinSize,minMass)]
	// 					[addedBin[findBinIndex(librarySpectra[i].PrecursorMz, massBinSize,minMass)]] = librarySpectra[i];
	// 			addedBin[findBinIndex(librarySpectra[i].PrecursorMz,massBinSize,minMass)]++;
	// 		}
	// 		updateProgress(50,"% - Creating Composite Library",true);
	// 	}
	// }


	//Search all  experimental mass spectra against libraries
	// public void matchLibrarySpectra(SampleSpectrum ms2, double massBinSize, double ms1Tol, double ms2Tol)
	// {
	// 	double dotProd = 0.0;
	// 	double reverseDotProd = 0.0;
	//
	// 	if (minMass < 9998.0 && ms2.precursor<maxMass && ms2.precursor>minMass)
	// 	{
	// 		//Scale MS2s to maximum peak in spectra (0-1000) and remove peaks below .5%
	// 		ms2.scaleIntensities();
	//
	// 		// Find range of mass bins which need to be searched
	// 		int minIndex = findBinIndex(FindMinMassRange(ms2.precursor,ms1Tol),massBinSize,minMass);
	// 		int maxIndex = findBinIndex(FindMaxMassRange(ms2.precursor,ms1Tol),massBinSize,minMass);
	//
	// 		if (minIndex<0) minIndex = 0;
	// 		if (maxIndex>(massBin.Length-1)) maxIndex = massBin.Length-1;
	//
	// 		//Iterate through this mass bin range
	// 		for (int i=minIndex; i<=maxIndex; i++)
	// 		{
	// 			//If the bin contains library spectra
	// 			if (countBin[i]>0)
	// 			{
	// 				//For all spectra which are in the same mass bin
	// 				for (int j=0; j<addedBin[i]; j++)
	// 				{
	// 					if (Math.Abs(massBin[i][j].Precursor-ms2.precursor)<ms1Tol)
	// 					{
	// 						//Calculate the dot product (spectral similarity) between the two spectra 
	// 						dotProd = ms2.CalculateDotProduct(massBin[i][j].TransitionArray,ms2Tol,false, MassCosineWeighting, IntensityCosineWeighting);
	// 						reverseDotProd = ms2.CalculateDotProduct(massBin[i][j].TransitionArray,ms2Tol,true, MassCosineWeighting, IntensityCosineWeighting);
	//
	// 						if (dotProd>1)
	// 						{
	// 							//Add identification to array
	// 							ms2.AddId(massBin[i][j], dotProd, reverseDotProd,ms2Tol);
	// 						}
	// 					}
	// 				}
	// 			}
	// 		}
	//
	// 		//Sort by dot product
	// 		ms2.IdentificationsList.Sort();
	// 	}
	// }

	// public void addSpectrum(LibrarySpectrumOld spectrum)
	// {
	// 	librarySpectra.Add(spectrum);
	// 	if (spectrum.Precursor > maxMass) maxMass = spectrum.Precursor;
	// 	if (spectrum.Precursor < minMass) minMass = spectrum.Precursor;
	// 	spectrum.ScaleIntensities();
	// }

}