// FULLY IMPLEMENTED LIPIDEX 1 METHODS

using System;
using System.Collections.Generic;

namespace LipiDex_2._0.SpectrumSearcher;

public class SampleSpectrum  
{
	public double precursor;										//Precursor of MS2
	public double retention;										//Retention time
	public string polarity;										//Polarity of MS2
	public string file;											//.mgf file
	public List<Transition> transitionArray;					//Array of all transitions
	public List<Identification> IdentificationsList;						//Array of all identifications
	public List<double> allMatchedMassesArray;				//Array of all identifications
	public double maxIntensity;									//Intensity of most intense fragment
	public double maxIntensityMass;								//Mass of most intense fragment
	public int spectraNumber = 0;								//Spectra number
	public PeakPurity peakPurity = null;							//Object for storing peak purity information

	//Constructor
	public SampleSpectrum(
			double precursor, string polarity, string file,
			double retention, int spectraNumber)
	{
		//Initialize variables
		this.precursor = Math.Round(precursor * 1000.0) / 1000.0;
		this.polarity = polarity;
		this.file = file;
		this.transitionArray = new List<Transition>();
		this.IdentificationsList = new List<Identification>();
		allMatchedMassesArray = new List<double>();
		maxIntensity = 0.0;
		maxIntensityMass = 0.0;	
		this.retention = Math.Round(retention * 1000.0) / 1000.0;
		this.spectraNumber = spectraNumber;
	}
	
	/// <summary>
	/// FROM LIPIDEX PAPER:
	/// 
	/// Accurate quantification of co-fragmentation of isobaric lipids and total spectral purity
	/// follows the logic flow detailed in Figure S6. For a given MS/MS spectrum, all spectral
	/// matches are ranked according to their dot-product score and added to the scan queue (SQ).
	/// Next, fragmentation template entry for each candidate lipid species is queried to find
	/// the fatty acid-identifying fragment type which is assigned the highest relative intensity.
	/// The experimental spectrum is then searched for fragments which correspond to this rule type.
	/// If all of the predicted highest intensity chain-identifying fragments are not found,
	/// the candidate identification is discarded. If all predicted highest intensity
	/// chain-identifying fragments are found, the relative intensity of matched fragments
	/// is corrected using any matching m/z values found in the correction list (CL).
	/// The corrected m/z values are then assigned the median intensity of the matched
	/// fatty acid-identifying fragments and added to the CL. Depending on the number of
	/// fatty acids for the identified lipid, either the median or maximum matched intensity
	/// is added to the intensity list (IL). This intensity value is used to quantify the
	/// relative abundance of the lipid in the MS/MS spectrum. This process is repeated until
	/// no candidate spectra remain.
	/// All intensities found in the IL are then scaled to the sum of all matched species’ intensities.
	///
	///
	/// Figure S6. Spectral Deconvolution Logic Flow, Related to Figure 1
	/// The Scan Queue (SQ) is first populated with the spectral matching results and is rank-ordered
	/// 	by the dot-product score. MS/MS peaks corresponding to the most intense fatty acid-identifying
	/// 	fragment type are found and subtracted from the spectrum using the correction factor found in
	/// the Correction List (CL). Depending on the number of fatty acid moieties, a specific matched
	/// fragment (either the maximum or median intensity chain identifying fragment depending on the
	/// 	lipid) is added to the Intensity List (IL) and used to calculate the relative abundance of the
	/// 	species in the spectrum. This process is repeated until all candidate spectral matches are
	/// 	processed and a spectral purity value is calculated.
	///
	///
	/// 
	/// </summary>
	/// <param name="faDB"></param>
	/// <param name="mzTol"></param>
	public void CalculateSpectralPurity(List<FattyAcid> faDB, double mzTol)
	{
		var librarySpectra = new List<LibrarySpectrum>();
		
		if (IdentificationsList.Count == 0) return;
		
		IdentificationsList.Sort();

		if (IdentificationsList[0].LibrarySpectrum.Name == "") return; // I think that all Library spectra are guaranteed to have a name, therefore this is redundant
		
		// ORIGINAL LIPIDEX COMMENT: Add all library spectra to temp array
		// I think that the librarySpectra list is the "Scan Queue" object mentioned in the flow chart
		for (int i = 1; i < IdentificationsList.Count; i++)
		{
			if (IdentificationsList[i].LibrarySpectrum.Polarity == IdentificationsList[0].LibrarySpectrum.Polarity &&  // If the Library polarities are identical
			    IdentificationsList[i].LibrarySpectrum.PrecursorMz == IdentificationsList[0].LibrarySpectrum.PrecursorMz) // If Library Precursor MZs are identical
				librarySpectra.Add(IdentificationsList[i].LibrarySpectrum);
		}

		//Calculate purity
		peakPurity = IdentificationsList[0].CalcPurityAll(faDB, librarySpectra, transitionArray, mzTol);
	}
	
	// public void BJACalculateSpectralPurity()

	//Calculate dot product between to sample spectra
	public double CalculateDotProduct(List<MzIntensityComment> libArray, double mzTol,
								 bool reverse, double massWeight, double intWeight)
	{
		double result = 0.0;
		double numerSum = 0.0;
		double libSum = 0.0;
		double sampleSum = 0.0;
		double massDiff = -1.0;
		List<double> libMasses = new List<double>();
		List<double> sampleMasses = new List<double>();
		List<double> libIntensities = new List<double>();
		List<double> sampleIntensities = new List<double>();	

		// Algorithm is dependent on arrays being sorted
		transitionArray.Sort();
		libArray.Sort();
		
		int i = 0, j = 0;

		// Iterate through each sample and library peak list, stopping at the shorter one
		while (i < libArray.Count && j < transitionArray.Count)
		{
			massDiff = transitionArray[j].mass - libArray[i].Mz;

			if (Math.Abs(massDiff) > mzTol)
			{		
				if (libArray[i].Mz < transitionArray[j].mass)
				{
					libIntensities.Add(libArray[i].Intensity);
					sampleIntensities.Add(0.0);
					sampleMasses.Add(0.0);
					libMasses.Add(libArray[i++].Mz);
				}
				else if (libArray[i].Mz > transitionArray[j].mass)
				{
					sampleIntensities.Add(transitionArray[j].intensity);
					libIntensities.Add(0.0);
					sampleMasses.Add(transitionArray[j++].mass);
					libMasses.Add(0.0);
				}
			}
			else
			{
				sampleMasses.Add(transitionArray[j].mass);
				libMasses.Add(libArray[i].Mz);
				libIntensities.Add(libArray[i++].Intensity);
				sampleIntensities.Add(transitionArray[j++].intensity);
			}
		}

		// Add remaining elements of the array with more peaks 
		while (i < libArray.Count)
		{
			libIntensities.Add(libArray[i].Intensity);
			sampleIntensities.Add(0.0);
			sampleMasses.Add(0.0);
			libMasses.Add(libArray[i++].Mz);
		}
		while (j < transitionArray.Count)
		{
			sampleIntensities.Add(transitionArray[j].intensity);
			libIntensities.Add(0.0);
			sampleMasses.Add(transitionArray[j++].mass);
			libMasses.Add(0.0);
		}

		//Iterate through unique masses
		for (int k=0; k<libMasses.Count; k++)
		{
			//If only in sample
			if (libIntensities[k] == 0.0)
			{
				if (!reverse) sampleSum += Math.Pow(massWeight*sampleMasses[k]*Math.Pow(sampleIntensities[k]/2.0,intWeight), 2);
			}
			//If only in lib
			else if (sampleIntensities[k] == 0.0)
			{
				if (!reverse) libSum += Math.Pow(massWeight*libMasses[k]*Math.Pow(libIntensities[k],intWeight), 2);
			}
			//If in both
			else
			{
				libSum += Math.Pow(Math.Pow(libMasses[k], massWeight)*Math.Pow(libIntensities[k],intWeight), 2);
				sampleSum += Math.Pow(Math.Pow(sampleMasses[k], massWeight)*Math.Pow(sampleIntensities[k],intWeight), 2);
				numerSum += Math.Pow(sampleMasses[k], massWeight)*Math.Pow(sampleIntensities[k],intWeight)*Math.Pow(libMasses[k], massWeight)*Math.Pow(libIntensities[k],intWeight);
			}
		}

		//Calculate dot product
		if (numerSum > 0.0) result = 1000.0 * Math.Pow(numerSum, 2)/(sampleSum*libSum);
		
		return result;
	}

	//Method to add fragments to proper arrays
	public void AddPeak(double mass, double intensity)
	{
		//Add mass and intensity to arrays
		transitionArray.Add(new Transition(mass, intensity, null));

		//Update max intensity and mass
		if (intensity > maxIntensity)
		{
			maxIntensity = intensity;
			maxIntensityMass = mass;
		}
	}

	//A method to scale intensities to max. intensity on scale of 0-999
	public void scaleIntensities()
	{
		for (int i=0; i<transitionArray.Count; i++)
		{
			transitionArray[i].intensity = (transitionArray[i].intensity/maxIntensity)*999;

			if (transitionArray[i].intensity<5)
			{
				transitionArray.RemoveAt(i);
				i--;
			}
		}
	}

	//Method to add id to array
	public void AddId(LibrarySpectrum librarySpectrum, 
		double dotProduct, double reverseDotProduct, double mzTol)
	{
		Identification id = new Identification(
			librarySpectrum, 
			((precursor - librarySpectrum.PrecursorMz) / librarySpectrum.PrecursorMz) * 1000000,
			dotProduct, reverseDotProduct);
		IdentificationsList.Add(id);
	}

	//Calculates the mass difference in ppm
	public  double CalculatePpmDifference(double massA, double massB)
	{
		double ppmResult = 0.0;
		ppmResult = ((massA - massB) / massB) * 1000000;
		return ppmResult;
	}

	//A method to check if a mass array contains a certain mass within mass tolerance
	public bool checkFrag(double mass, double massTol)
	{
		for (int i=0; i<transitionArray.Count; i++)
		{
			if (Math.Abs(transitionArray[i].mass - mass) < massTol) return true;
		}

		return false;
	}

	//Returns string representation of array
	// public string toString(int maxResults)
	// {
	// 	string result = "";
	//
	// 	//Sort results by dot product
	// 	IdentificationsList.Sort();
	//
	// 	//Add in transitions up to max number of results
	// 	for (int i=0; i<IdentificationsList.Count && i<maxResults; i++)
	// 	{
	// 		result += spectraNumber+","
	// 				+retention+","+(i+1)+","
	// 				+IdentificationsList[i].LibrarySpectrumOld.Name+","
	// 				+precursor+","
	// 				+IdentificationsList[i].LibrarySpectrumOld.PrecursorMz+","
	// 				+IdentificationsList[i].DeltaMass+","
	// 				+Math.Round(IdentificationsList[i].DotProduct)+","
	// 				+Math.Round(IdentificationsList[i].ReverseDotProduct)+","
	// 				+IdentificationsList[i].Purity+",";
	//
	// 		if (peakPurity != null)
	// 		{
	// 			for (int j=0; j<peakPurity.Lipids.Count; j++)
	// 			{
	// 				result+= peakPurity.Lipids[j].Name.Replace(";", "")+"("+peakPurity.Purities[j]+")";
	// 				if (j!=peakPurity.Lipids.Count-1) result += " / ";
	// 			}
	//
	// 			result += ",";
	// 		}
	// 		else result+= ",";
	//
	// 		result += IdentificationsList[i].LibrarySpectrumOld.IsOptimalPolarity+","
	// 				+IdentificationsList[i].LibrarySpectrumOld.IsLipiDex+","
	// 				+IdentificationsList[i].LibrarySpectrumOld.Library+",";
	//
	// 		//If purity array has been made add in all matched masses
	// 		if (IdentificationsList[i].Purity>1 && peakPurity != null)
	// 		{
	// 			for (int j=0; j<peakPurity.MatchedMasses.Count; j++)
	// 			{
	// 				if (!result.Contains(peakPurity.MatchedMasses[j].ToString()))  // Java code was String.valueOf() instead of ToString
	// 					result+=peakPurity.MatchedMasses[j]+" | ";
	// 			}
	// 		}
	//
	// 		//Else, add in best ID masses
	// 		else if (IdentificationsList[i].Purity<1)
	// 		{
	// 			for (int j=0; j<IdentificationsList[i].LibrarySpectrumOld.MzIntensityCommentList.Count; j++)
	// 			{
	// 				result+=IdentificationsList[i].LibrarySpectrumOld.MzIntensityCommentList[j].Mz+" | ";
	// 			}
	// 		}
	//
	// 		if (IdentificationsList.Count > 1 && (i < (IdentificationsList.Count-1) && i < (maxResults-1))) result += "\n";
	// 	}
	// 	return result;
	// }
}