using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using LipiDex2.LibraryGenerator;
using CSMSL;
using CSMSL.IO;
using CSMSL.Spectral;
using CSMSL.IO.Thermo;
//using static System.Net.WebRequestMethods;
using System.Windows.Media.Media3D;

namespace LipiDex2.SpectrumSearcher
{
    public class SpectrumSearcher
    {
        public List<string> rawFiles;
        public List<string> mgfFiles;
        public List<string> mzxmlFiles;
        public List<string> lipidLibs;  // LD1 used java File object
        static List<FattyAcid> fattyAcidsDB;
        public List<LibrarySpectrum> librarySpectra;
        public double ms1Tol;
        public double ms2Tol;
        public double massBinSize;
        public int maxResults;
        //ProgressBar
        public List<SampleSpectrum> sampleMS2Spectra;
        public int[] countBin;
        public int[] addedBin;
        public LibrarySpectrum[][] massBin;
        public double maxMass;
        public double minMass;
        public double intWeight = 1.2;
        public double massWeight = 0.9;
        public double minMS2Mass = 60.0;
        public static List<TransitionType> transitionTypes;
        public static List<string> transitionTypeStrings = new List<string>();
        public long timeStart;
        public long timeStop;
        int numSpectra;
        static int progressInt = 0;


        public SpectrumSearcher(
            List<string> lipidLibs,
            List<string> rawFiles,
            List<string> mgfFiles,
            List<string> mzxmlFile,
            double ms1Tol,
            double ms2Tol,
            int maxResults,
            double minMassCutoff
            )
        {
            this.minMS2Mass = minMassCutoff;
            this.lipidLibs = lipidLibs;
            //SpectrumSearcher.progressbar = ProgressBar;
            this.ms1Tol = ms1Tol;
            this.ms2Tol = ms2Tol;
            this.rawFiles = rawFiles;
            this.mgfFiles = mgfFiles;
            this.mzxmlFiles = mzxmlFile;
            massBinSize = ms1Tol / 10.0;
            minMass = 999.0;
            maxMass = 0.0;
            librarySpectra = new List<LibrarySpectrum>();
            fattyAcidsDB = new List<FattyAcid>();
            sampleMS2Spectra = new List<SampleSpectrum>();
            this.maxResults = maxResults;
            numSpectra = 0;
        }

        public void RunSpectraSearch(List<string> lipidLibs, double mzTol) 
        {
            /* 
             * Processing steps
             * 0. Read fatty acids, adducts, lipid classes and lipid categories
             * 1. Read each rawfile using new CSMSL
             *      2. For each spectrum:
             *             Run library matching with custom weighted cosine similarity as implemented by Paul
             *      3. Calculate spectral purity 
             *      4. 
             */

            this.lipidLibs = lipidLibs;
            //CheckFileStatus();  //Verify no result files are currently open --> We can do this with .NET functions
            ReadFattyAcids(@"src\backup\FattyAcids.csv");

            try
            {
                //ReadMSP();
            }
            catch (Exception e)
            {
                //CustomErrorsModes ce = new CustomError("Error loading library .msp file", e);
                Console.WriteLine("Error loading msp");  //TODO: make custom error
            }

            //Sort arrays by ascending mass
            librarySpectra.Sort();
            // UpdateProgress

            BinMasses();  //Bin MSP LibrarySpectra
            // UpdateProgress

            //Read .raw file directly using CSMSL
            foreach (string file in rawFiles)
            {
                timeStart = System.Diagnostics.Stopwatch.GetTimestamp();
                numSpectra = 0;
                ReadRawfile(file);
            }

            
            foreach (string file in mgfFiles)
            {
                timeStart = System.Diagnostics.Stopwatch.GetTimestamp();
                numSpectra = 0;

                ReadMGF(file);

                //Iterate through spectra and match
                foreach (SampleSpectrum spectrum in sampleMS2Spectra) 
                {
                    MatchLibrarySpectra(spectrum, massBinSize, ms1Tol, ms2Tol);
                }

                //Calculate purity values
                foreach (SampleSpectrum spectrum in sampleMS2Spectra)
                {
                    spectrum.CalcPurityAll(fattyAcidsDB, mzTol);
                }

                //Write output files for MGF files
                try
                {
                    //WriteResults(sampleMS2Spectra, file, maxResults);
                    string y = "";
                }
                catch (IOException e)
                {
                    //CustomError ce = new CustomError("Please close " + mgfFiles.get(i) + " and re-search the data", null);
                    Console.WriteLine("close mgf files");
                }

                //Clear sample spectra
                sampleMS2Spectra = new List<SampleSpectrum>();
                //UpdateProgress

            }

            // Now parse mzxml files 
            MZXMLParser parser = new MZXMLParser();

            foreach (var mzxmlFile in mzxmlFiles)
            {
                timeStart = System.Diagnostics.Stopwatch.GetTimestamp();
                numSpectra = 0;

                parser.ReadFile(mzxmlFile);

                // Create SampleSpectrum objects
                foreach (var sampleSpectrum in parser.sampleSpecArray)
                {
                    sampleMS2Spectra.Add(sampleSpectrum);
                }

                //Iterate through spectra and match
                foreach (var spectrum in sampleMS2Spectra)
                {
                    MatchLibrarySpectra(spectrum, massBinSize, ms1Tol, ms2Tol);
                }

                //Calculate purity values
                foreach (var spectrum in sampleMS2Spectra)
                {
                    spectrum.CalcPurityAll(fattyAcidsDB, mzTol);
                }
                //Write outputs for mzxml
                //WriteResults(sampleMS2Spectra, mzxmlFile, maxResults);

                //Clear sample spectra
                sampleMS2Spectra = new List<SampleSpectrum>();
                
                //UpdateProgress
            }


        }

        public void MatchLibrarySpectra(SampleSpectrum spectrum, double massBinSize, double ms1Tol, double ms2Tol)
        {
            Double dotProd = 0.0;
            Double reverseDotProd = 0.0;

            if (minMass < 9998.0 && spectrum.precursor < maxMass && spectrum.precursor > minMass)
            {
                //Scale MS2s to maximum peak in spectra (0-1000) and remove peaks below .5%
                spectrum.ScaleIntensities();

                // Find range of mass bins which need to be searched
                int minIndex = FindBinIndex(FindMinMassRange(spectrum.precursor, ms1Tol), massBinSize, minMass);
                int maxIndex = FindBinIndex(FindMaxMassRange(spectrum.precursor, ms1Tol), massBinSize, minMass);

                if (minIndex < 0) minIndex = 0;
                if (maxIndex > (massBin.Length - 1)) maxIndex = massBin.Length - 1;

                //Iterate through this mass bin range
                for (int i = minIndex; i <= maxIndex; i++)
                {
                    //If the bin contains library spectra
                    if (countBin[i] > 0)
                    {
                        //For all spectra which are in the same mass bin
                        for (int j = 0; j < addedBin[i]; j++)
                        {
                            if (Math.Abs(massBin[i][j].precursor - spectrum.precursor) < ms1Tol)
                            {
                                //Calculate the dot product (spectral similarity) between the two spectra 
                                dotProd = spectrum.CalcDotProduct(massBin[i][j].transitionArray, ms2Tol, false, massWeight, intWeight);
                                reverseDotProd = spectrum.CalcDotProduct(massBin[i][j].transitionArray, ms2Tol, true, massWeight, intWeight);

                                if (dotProd > 1)
                                {
                                    //Add identification to array
                                    spectrum.AddID(massBin[i][j], dotProd, reverseDotProd, ms2Tol);
                                }
                            }
                        }
                    }
                }

                //Sort by dot product
                spectrum.idArray.Sort();
            }
        }

        private object FindMaxMassRange(double precursor, double ms1Tol)
        {
            throw new NotImplementedException();
        }

        private object FindMinMassRange(double precursor, double ms1Tol)
        {
            throw new NotImplementedException();
        }

        private int FindBinIndex(object value, double massBinSize, double minMass)
        {
            throw new NotImplementedException();
        }

        //private void ReadMGF(string file)
        //{
        //    String line = "";                   //String for reading in .mgf
        //    String polarity = "";               //Polarity of ms2
        //    Double precursor = 0.0;             //Precursor mass
        //    SampleSpectrum specTemp = null;     //Stores ms2 to be added
        //    String[] split;                     //String array for parsing transitions
        //    Double retention = 0.0;             //Retention time in minutes

        //    BufferedReader reader = new BufferedReader(new FileReader(filename));
        //    File file = new File(filename);

        //    //read line if not empty
        //    while ((line = reader.readLine()) != null)
        //    {
        //        if (!line.startsWith("#"))
        //        {
        //            if (line.contains("PEPMASS="))
        //            {

        //                if (line.contains(" ")) precursor = Double.valueOf(line.substring(line.indexOf("=") + 1, line.lastIndexOf(" ")));
        //                else precursor = Double.valueOf(line.substring(line.indexOf("=") + 1));
        //            }
        //            else if (line.contains("RTINSECONDS"))
        //            {
        //                if (line.contains(" ")) retention = (Double.valueOf(line.substring(line.indexOf("=") + 1, line.lastIndexOf(" ")))) / 60.0;
        //                else retention = Double.valueOf(line.substring(line.indexOf("=") + 1)) / 60.0;
        //            }
        //            else if (line.contains("CHARGE"))
        //            {
        //                //peakStart = true;

        //                if (line.contains("-")) polarity = "-";
        //                else polarity = "+";
        //            }
        //            else if (line.contains("END IONS"))
        //            {
        //                numSpectra++;

        //                if (specTemp != null) sampleMS2Spectra.add(specTemp);

        //                specTemp = null;
        //                polarity = "+";
        //                precursor = 0.0;
        //                retention = 0.0;
        //            }
        //            else if (line.contains(".") && !line.contains("PEPMASS") && !line.contains("CHARGE") && !line.contains("TITLE"))
        //            {
        //                split = line.split(" ");

        //                if (specTemp == null)
        //                {
        //                    specTemp = new SampleSpectrum(precursor, polarity, file.getName(), retention, numSpectra);
        //                }

        //                if ((precursor - Double.valueOf(split[0])) > 1.5
        //                        && Double.valueOf(split[0]) > minMS2Mass
        //                        && Double.valueOf(split[1]) > 1.0)
        //                {
        //                    specTemp.addFrag(Double.valueOf(split[0]), Double.valueOf(split[1]));
        //                }
        //            }
        //        }
        //    }
        //    reader.close();
        //}

        private void BinMasses()
        {
            throw new NotImplementedException();
        }

        //Method to add spectrum to proper array and update mass ranges
        public void AddSpectrum(LibrarySpectrum spectrum)
        {
            librarySpectra.Add(spectrum);
            if (spectrum.precursor > maxMass) maxMass = spectrum.precursor;
            if (spectrum.precursor < minMass) minMass = spectrum.precursor;
            spectrum.ScaleIntensities();
        }

        public void ReadRawfile(string file)
        {
            using (ThermoRawFile rawFile = new ThermoRawFile(file))
            {
                rawFile.Open();
                for (int i = rawFile.FirstSpectrumNumber; i <= rawFile.LastSpectrumNumber; i++)
                {
                    //Get scans that are not MS1s
                    if (rawFile.GetMsnOrder(i) > 1)
                    {
                        ThermoSpectrum rawSpectrum = rawFile.GetSpectrum(i);

                        double precursor = rawFile.GetPrecursorMz(i);
                        int msnOrder = rawFile.GetMsnOrder(i);
                        string polarity = rawFile.GetPolarity(i).ToString();
                        if      (polarity.Equals("Positive")) polarity = "+";
                        else if (polarity.Equals("Negative")) polarity = "-";
                        else throw new Exception("Spectrum polarity is not positive or negative");

                        double rt = rawFile.GetRetentionTime(i);

                        SampleSpectrum sampleSpectrum = new SampleSpectrum(
                            precursor: precursor, 
                            polarity: polarity, 
                            file: file, 
                            retention: rt, 
                            spectrumNumber: i,
                            msnOrder: msnOrder
                            );

                        foreach (ThermoMzPeak peak in rawSpectrum)
                        {
                            sampleSpectrum.AddFrag(mass: peak.MZ, intensity: peak.Intensity);
                        }

                        sampleMS2Spectra.Add(sampleSpectrum);

                    }
                    else continue;

                }

            }
        }

        private void WriteResults(List<SampleSpectrum> sampleMS2Spectra, string file, int maxResults, string outDir)
        {
            string resultFileName = @"D:\Lipidex2\tests\SS_test_output.csv";

            // print writer

            foreach (SampleSpectrum spectrum in sampleMS2Spectra)
            {
                if (spectrum.idArray.Count() > 0 && spectrum.idArray[0].dotProduct > 1)
                {

                }
            }
            
        }

        private void ReadFattyAcids(string filename)
        {
            string line = null;
            string[] split = null;
            string name;
            string type;
            string enabled;
            string formula;
            List<string> faTypes = new List<string>();

            //Create file buffer  TODO: figure out file buffers?
            //File file = new File(filename);
            //BufferedReader reader = new BufferedReader(new Filereader(file));

            // clear FA DB
            fattyAcidsDB = new List<FattyAcid>();

            //while (reader.ReadLine())
            //{

            //}

        }

        private static TransitionType getTransitionType(String s)
        {
            for (int i = 0; i < transitionTypes.Count(); i++)
            {
                if (transitionTypes[i].name.Equals(s)) return transitionTypes[i];
            }
            return null;
        }

        private void CheckFileStatus() 
        {
            foreach (var mgfFile in mgfFiles)
            {
                string resultFileName = mgfFile.Substring(0, mgfFiles.LastIndexOf(".")) + "_Results.csv";
            }
        }

        //        private void ReadMSP()
        //        {
        //            string line = "";                       //Line for reading
        //            string fragType = "";                   //Fragment type
        //            string polarity = "";                   //Polarity of entry
        //            Double precursor = 0.0;                 //Precursor mass
        //            string name = "";                       //Lipid name
        //            LibrarySpectrum entryTemp = null;       //Temp library entry
        //            bool peakStart = false;              //Boolean if peak list beginning
        //            bool isLipidex = false;              //Boolean if spectra was generated using LipiDex
        //            bool optimalPolarity = false;        //Boolean if optimal polarity for class
        //            string[] split;                         //String array for parsing transitions

        //            //for (int i = 0; i < lipidLibs.Count; i++)
        //            foreach (string lib in lipidLibs)
        //            {
        //                //updateProgress((int)(Double.valueOf(i + 1) / Double.valueOf(lipidLibs.size()) * 100.0), "% - Reading Libraries", true);

        //                //BufferedReader reader = new BufferedReader(new FileReader(lipidLibs.get(i)));
        //                if (File.Exists(lib))
        //                {
        //                    using (StreamReader reader = new StreamReader(lib))
        //                    {
        //                        while ((line = reader.ReadLine()) != null)
        //                        {
        //                            //read in retention time
        //                            if (line.Contains("Name:") || line.Contains("NAME:"))
        //                            {
        //                                //Add entry
        //                                if (precursor > 0.0)
        //                                {
        //                                    AddSpectrum(entryTemp);
        //                                }

        //                                //Erase entry
        //                                polarity = "";
        //                                precursor = 0.0;
        //                                name = "";
        //                                fragType = "";
        //                                peakStart = false;
        //                                isLipidex = false;
        //                                optimalPolarity = false;

        //                                if (line.Contains("]+")) polarity = "+";
        //                                else polarity = "-";

        //                                name = line.Substring(line.IndexOf(":") + 1);
        //                            }

        //                            //read in optimal polarity
        //                            if (line.Contains("OptimalPolarity=true"))
        //                            {
        //                                optimalPolarity = true;
        //                            }

        //                            if (line.Contains("LipiDex")) isLipidex = true;

        //                            if (line.Contains("Num Peaks:"))
        //                            {
        //                                peakStart = true;
        //                                entryTemp = new LibrarySpectrum(precursor, polarity, name, lipidLibs.get(i).getName(), isLipidex, optimalPolarity);
        //                            }

        //                            if (peakStart && line.contains(".") && !line.contains("Num"))
        //                            {
        //                                if (line.contains("	"))
        //                                    split = line.split("	");
        //                                else
        //                                    split = line.split(" ");

        //                                if (isLipidex)
        //                                    fragType = line.substring(line.indexOf("\"") + 1, line.lastIndexOf("\""));

        //                                if (precursor - Double.valueOf(split[0]) > 2.0)
        //                                    entryTemp.addFrag(Double.valueOf(split[0]), Double.valueOf(split[1]), fragType, getTransitionType(fragType));

        //                            }

        //                            if (line.contains("PRECURSORMZ:"))
        //                            {
        //                                precursor = Double.valueOf(line.substring(line.lastIndexOf(" ") + 1));
        //                            }
        //                        }
        //                    }

        //                }




        //                addSpectrum(entryTemp);
        //                reader.close();
        //            }

        //        // Create array of bins to store objects, bins are indexed by precursor mass
        //        public void CreateBins(int arraySize)
        //        {
        //            //Check if any spectra from each polarity have been created
        //            if (librarySpectra.Count() > 0)
        //            {
        //                countBin = new int[arraySize];
        //                addedBin = new int[arraySize];

        //                for (int i = 0; i < arraySize; i++)
        //                {
        //                    countBin[i] = 0;
        //                    addedBin[i] = 0;
        //                }
        //                //Create array to store spectra objects
        //                massBin = new LibrarySpectrum[arraySize][];
        //            }
        //        }

        //        //Find correct bin number for precursor mass
        //        public int FindBinIndex(double precursor, double binSize, double minMass)
        //        {
        //            return (int)((precursor - minMass) / binSize);
        //        }

        //        //Calculate array size based on mass range
        //        public int CalculateArraySize(double binSize, double minMass, double maxMass)
        //        {
        //            return (int)((maxMass - minMass) / binSize) + 1;
        //        }

        //        //Bin precursor masses for efficient searching
        //        private void BinMasses()
        //        {
        //            CreateBins(CalculateArraySize(massBinSize, minMass, maxMass));
        //            //Check if spectra from either polarity have been created
        //            if (librarySpectra.Count() > 0)
        //            {
        //                //Populate count array to correctly initialize array size for positive lib spectra 
        //                foreach (LibrarySpectrum spectrum in librarySpectra)
        //                {
        //                    countBin[FindBinIndex(spectrum.precursor, massBinSize, minMass)]++;
        //                }
        //                //UpdateProgress()
        //                //Use count bin to initialize new arrays to place positive spectra into hash table
        //                for (int i=0; i < countBin.Length; i++)
        //                {
        //                    massBin[i] = new LibrarySpectrum[countBin[i]];
        //                }
        //                //UpdateProgress()

        //                //Populate spectrum arrays for positive spectra
        //                foreach (LibrarySpectrum spectrum in librarySpectra)
        //                {
        //                    massBin[FindBinIndex(spectrum.precursor, massBinSize, minMass)]
        //                           [addedBin[FindBinIndex(spectrum.precursor, massBinSize, minMass)]] = spectrum;
        //                    addedBin[FindBinIndex(spectrum.precursor, massBinSize, minMass)]++;
        //                }
        //                //UpdateProgress()
        //            }
        //        }

        //        //Search all  experimental mass spectra against libraries
        //        public void MatchLibrarySpectra(
        //                SampleSpectrum ms2,
        //                double massBinSize,
        //                double ms1Tol,
        //                double ms2Tol)
        //        {
        //            double dotProd = 0.0;
        //            double reverseDotProd = 0.0;

        //            if (minMass < 9998.0 && ms2.precursor < maxMass && ms2.precursor > minMass)
        //            {
        //                //Scale MS2s to maximum peak in spectra (0-1000) and remove peaks below .5%
        //                ms2.ScaleIntensities();

        //                // Find range of mass bins which need to be searched
        //                int minIndex = FindBinIndex(FindMinMassRange(ms2.precursor, ms1Tol), massBinSize, minMass);
        //                int maxIndex = FindBinIndex(FindMaxMassRange(ms2.precursor, ms1Tol), massBinSize, minMass);

        //                if (minIndex < 0) minIndex = 0;
        //                if (maxIndex > (massBin.Length - 1)) maxIndex = massBin.Length - 1;

        //                //Iterate through this mass bin range
        //                for (int i = minIndex; i <= maxIndex; i++)
        //                {
        //                    //If the bin contains library spectra
        //                    if (countBin[i] > 0)
        //                    {
        //                        //For all spectra which are in the same mass bin
        //                        for (int j = 0; j < addedBin[i]; j++)
        //                        {
        //                            if (Math.Abs(massBin[i][j].precursor - ms2.precursor) < ms1Tol)
        //                            {
        //                                //Calculate the dot product (spectral similarity) between the two spectra 
        //                                dotProd = ms2.CalcDotProduct(massBin[i][j].transitionArray, ms2Tol, false, massWeight, intWeight);
        //                                reverseDotProd = ms2.CalcDotProduct(massBin[i][j].transitionArray, ms2Tol, true, massWeight, intWeight);

        //                                if (dotProd > 1)
        //                                {
        //                                    //Add identification to array
        //                                    ms2.AddID(massBin[i][j], dotProd, reverseDotProd, ms2Tol);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }

        //                //Sort by dot product
        //                ms2.idArray.Sort();
        //            }
        //        }

        //        private void ReadMGF(string file)
        //        {
        //            throw new NotImplementedException();
        //        }

        //        public void AddSpectrum(LibrarySpectrum spectrum)
        //        {
        //            librarySpectra.Add(spectrum);
        //            if (spectrum.precursor > maxMass) maxMass = spectrum.precursor;
        //            if (spectrum.precursor < minMass) minMass = spectrum.precursor;
        //            spectrum.ScaleIntensities();
        //        }

        //        //Return transition type object corresponding to provided string
        //        private static TransitionType GetTransitionType(string s)
        //        {
        //            for (int i = 0; i < transitionTypes.Count; i++)
        //            {
        //                if (transitionTypes[i].name.Equals(s)) return transitionTypes[i];
        //            }
        //            return null;
        //        }

        //        public double FindMinMassRange(double mass, double mzTol)
        //        {
        //            return (mass - mzTol);
        //        }
        //        public double FindMaxMassRange(double mass, double mzTol)
        //        {
        //            return (mass + mzTol);
        //        }




    }
}
