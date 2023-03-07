using System;
using System.Collections.Generic;
using LipiDex_2._0.LibraryGenerator;
using MathNet.Numerics.Statistics;

namespace LipiDex_2._0.SpectrumSearcher;

public class PeakPurity
{
	public List<Transition> Transitions = new();
	public List<LibrarySpectrum> Lipids = new();
	public List<double> Intensities = new();
	public List<double> MatchedMasses = new();
	public List<int> Purities = new();
	public List<double> ComboIntensities = new();
	public List<double> TopMasses = new();
	public List<double> IntensityCorrectionArray = new();
	public double IntensityCorrection = 0.0;
	public double MzTol = 0.01;
	public double MinPurity = 5.0;
	public int CarbonNumber = 0;
	public int UnsatNumber = 0;
	public int NumFattyAcids = 0;
	public int TotalFattyAcids = 0;
	public string ChainGreatest = "";

	public int CalcPurity(string lipidName, 
		string sumLipidName, 
		List<Transition> transitions, 
		double precursor, 
		string polarity,
		string lipidClass, 
		string adduct, 
		List<FattyAcid> faDB, 
		LibrarySpectrum ls, 
		List<LibrarySpectrum> isobaricIDs, 
		double mzTol)
	{
		double purity = 0.0;
		int purityInt = 0;
		double sumInt = 0.0;
		Transitions = transitions;
		MzTol = mzTol;

		// Count number of Fatty acids
		for (int i=0; i < lipidName.Length; i++ ) 
		{
			if( lipidName[i] == ':' )  
				TotalFattyAcids++;
		}

		//Calculate purity for first match
		purity = getPurityIntensity(ls, faDB, true);

		//if (purity == 0.0) return 0;
		if (purity > 0.0 ) 
		{
			Intensities.Add(purity);
			Lipids.Add(ls);

			//Add matches mass for in-source fragment screening
			for (int j=0; j < ls.TransitionArray.Count; j++)
			{
				MatchedMasses.Add(ls.TransitionArray[j].mass);
			}
		}

		//Calculate purity intensity for all other isobaric ids
		for (int i=0; i < isobaricIDs.Count; i++)
		{
			purity = getPurityIntensity(isobaricIDs[i], faDB, false);
			if (purity > 0.0 && isUniqueLipid(isobaricIDs[i]))
			{
				Intensities.Add(purity);
				Lipids.Add(isobaricIDs[i]);
				// If purity > 10, add matches mass for in-source fragment screening
				if (purity > MinPurity)
				{
					for (int j=0; j<isobaricIDs[i].TransitionArray.Count; j++)
					{
						MatchedMasses.Add(isobaricIDs[i].TransitionArray[j].mass);
					}
				}
			}
		}

		//Calculate purity percentage
		for (int i=0; i < Intensities.Count; i++)
		{
			sumInt += Intensities[i];
		}

		if (Intensities.Count < 1) return 0;
		
		purityInt = (int)Math.Round((Intensities[0] / sumInt) * 100);

		// Calculate purity percentage for all species
		for (int i=0; i < Intensities.Count; i++)
		{
			Purities.Add((int)Math.Round((Intensities[i]/sumInt)*100));
		}

		return purityInt;
	}

	// Returns true if lipid not contained in purity array
	private bool isUniqueLipid(LibrarySpectrum l)
	{
		for (int i = 0; i < Lipids.Count; i++)
		{
			if (Lipids[i].Name.Contains(l.Name)) return false;
		}
			
		return true;
	}

	private List<FattyAcid> parseFattyAcids(LibrarySpectrum ls, List<FattyAcid> faDB, string fattyAcidType)
	{
		List<FattyAcid> lipidFAs = new List<FattyAcid>();

		//Remove class name
		string faString = ls.Name
			.Substring(ls.Name.IndexOf(" ") + 1,ls.Name.LastIndexOf(" "));

		//Split name into FAs
		string[] split = faString.Split('_');

		//Iterate and find alkyl matches
		for (int i=0; i<split.Length; i++)
		{
			//Iterate through fadb
			for (int j=0; j<faDB.Count; j++)
			{
				//Add to temp array
				if (faDB[j].name.Equals(split[i]) && faDB[j].type.Equals(fattyAcidType))
				{
					lipidFAs.Add(faDB[j]);
				}
			}
		}

		return lipidFAs;
	}

	// Returns the intensity of each moiety-based fragment
	private List<double> findFAIntensities (LibrarySpectrum ls, List<FattyAcid> faDB, string transitionType, double minInt, bool topMatch)
	{
		double mass = 0.0;
		double intensity = 0.0;
		int massIndex = 0;
		double correctionIntensity;
		List<double> matchedIntensities = new List<double>();
		int faCount = 0;
		
		//For all library transitions
		for (int i=0; i<ls.TransitionArray.Count; i++)
		{
			//Add matching entries to libmasses array
			if (ls.TransitionArray[i].type.Equals(transitionType))
			{
				mass = ls.TransitionArray[i].mass;

				//Divide intensity by number of times fa occurs
				intensity = findTransitionMatch(mass);

				//Add if found
				if (intensity>minInt)
				{
					//Apply intensity correction
					if (getIndexOf(TopMasses, mass)>-1)
					{
						intensity = intensity -  IntensityCorrectionArray[getIndexOf(TopMasses, mass)];
						if (intensity<0.0) intensity = 0.0;
					}

					//Divide by number of occurences if less than 3 fatty acids
					//intensity = intensity/numOccurences(ls.transitionArray[i].FattyAcid,faDB);

					for (int j=0; j<numOccurences(ls.TransitionArray[i].FattyAcid, faDB); j++)
					{
						matchedIntensities.Add(intensity);
						faCount ++;
					}
				}
			}
		}

		if (faCount != faDB.Count) return null;

		//If all found and top hit, add intensities and masses to top arrays
		for (int i=0; i<ls.TransitionArray.Count; i++)
		{
			//index of mass in topMasses array
			massIndex = getIndexOf(TopMasses,ls.TransitionArray[i].mass);

			//Correction is smallest of matched intensities
			matchedIntensities.Sort();
			correctionIntensity = matchedIntensities.Median();
			

			//If unique and correct transition type, add to array
			if (!TopMasses.Contains(ls.TransitionArray[i].mass) 
					&& ls.TransitionArray[i].type.Equals(transitionType))
			{
				TopMasses.Add(ls.TransitionArray[i].mass);
				IntensityCorrectionArray.Add(correctionIntensity);
			}
			else if (ls.TransitionArray[i].type.Equals(transitionType))
			{
				//Add correction intensity to array
				double newCorrection = IntensityCorrectionArray[massIndex] + correctionIntensity;

				//If greater than actual spectral peak, change to original spectrum intensity
				if (newCorrection > findTransitionMatch(ls.TransitionArray[i].mass))
					newCorrection = findTransitionMatch(ls.TransitionArray[i].mass);

				//Change correction
				IntensityCorrectionArray[massIndex] = newCorrection;
			}
		}

		return matchedIntensities;
	}

	private int numOccurences(string fattyAcid, List<FattyAcid> faDB)
	{
		int count = 0;

		for (int i=0; i<faDB.Count; i++)
		{
			if (faDB[i].name.Equals(fattyAcid)) count ++;
		}
		return count;
	}

	private double getPurityIntensity(LibrarySpectrum ls, List<FattyAcid> faDB, bool topMatch)
	{
		string transitionType = "";
		double maxFragType = 0;
		List<FattyAcid> lipidFAs = new List<FattyAcid>();
		List<double> matchedIntensities = new List<double>();

		if (!ls.IsLipiDex) return 0.0;

		//Iterate through transitions to find most intense fatty acid fragment type
		try
		{
			for (int i=0; i<ls.TransitionArray.Count; i++)
			{
				if (!ls.TransitionArray[i].type.Contains("_Fragment") && !ls.TransitionArray[i].type.Contains("_Neutral Loss")
						&& !ls.TransitionArray[i].type.Contains("DG Fragment") && !ls.TransitionArray[i].type.Contains("PUFA"))
				{
					if (ls.TransitionArray[i].intensity>maxFragType)
					{
						maxFragType = ls.TransitionArray[i].intensity;
						transitionType = ls.TransitionArray[i].type;
					}
				}
			}
		}
		catch (Exception e)
		{
			return 0.0;
		}

		if (transitionType.Equals("")) return 0.0;

		//Parse out moiety
		if (transitionType.Contains(" Fragment"))
			lipidFAs = parseFattyAcids(ls, faDB, transitionType
				.Substring(transitionType.IndexOf("_")+1)
				.Replace(" Fragment",""));
		
		else if (transitionType.Contains(" Neutral Loss"))
			lipidFAs = parseFattyAcids(ls, faDB, transitionType
				.Substring(transitionType.IndexOf("_")+1)
				.Replace(" Neutral Loss",""));

		//Find intensity for all fatty acid peaks
		matchedIntensities = findFAIntensities(ls, lipidFAs, transitionType, maxFragType*.05, topMatch);

		//Check if all fragments were found
		if (matchedIntensities == null) return 0.0;
		
		//For 2 or less fatty acids, return median
		if (matchedIntensities.Count<3 || topMatch)
			return matchedIntensities.Median();

		else
			return matchedIntensities[matchedIntensities.Count-1];
	}

	/*
	 * If the putative mass is found within the ms2 mass tolerance, 
	 * add to FA and intensity array.  The number of times added corresponds
	 * to the number of possible fa sights available for purity analysis
	 */
	private double findTransitionMatch(double mass)
	{
		for (int i=0; i<Transitions.Count; i++)
		{
			if (Math.Abs(Transitions[i].mass-mass)<MzTol)
			{
				return Transitions[i].intensity;
			}
		}

		return 0.0;
	}
	

	//Calculate appropriate median spectral intensity
	// private double GetMedianChainIntensity(List<double> intTemp)
	// {
	// 	if (intTemp.Count > 0)
	// 	{
	// 		int middle;
	//
	// 		//Sort intensity array
	// 		intTemp.Sort();
	//
	// 		//Calculate median, if no middle point, move to the lower intensity
	// 		middle = intTemp.Count/2;
	//
	// 		if (intTemp.Count%2 == 1) 
	// 		{
	// 			return intTemp[middle];
	// 		} 
	// 		else 
	// 		{
	// 			return (intTemp[middle] + intTemp[middle-1]) / 2.0;
	// 		}
	// 	}
	// 	else
	// 	{
	// 		return 0.0;
	// 	}
	// }
	
	//Returns the n value of an array
	
	
	private double getNChainIntensity(List<double> intTemp, int num)
	{
		//Sort intensity array
		intTemp.Sort();

		//Return third
		return intTemp[intTemp.Count-num];
	}

	//Returns the lowest value above zero
	private double getMinIntensityAboveZero(List<double> intTemp)
	{
		//Sort intensity array
		intTemp.Sort();

		for (int i=0; i<intTemp.Count; i++)
		{
			if (intTemp[i]>0.001) return intTemp[i];
		}

		return 0.0;
	}

	//Returns index of double value
	private int getIndexOf(List<double> array, double num)
	{
		for (int i=0; i<array.Count; i++)
		{
			if (Math.Abs(array[i]-num)<0.000001)
			{
				return i;
			}
		}
		return -1;
	}
	
	// public static void skipPermutation(int[] limits, int[] counters)
	// {
	// 	//Set first counter to maximum
	// 	counters[0] = limits[0];
	// }
	
	//Returns the n value of an array, if not above threshold return n+1
	// private double getNChainIntensityAboveMin(List<double> intTemp, int num, double min)
	// {
	// 	//Sort intensity array
	// 	intTemp.Sort();
	//
	// 	if (intTemp.Count-num > min) 
	// 		return intTemp[intTemp.Count - num];
	// 	else
	// 		return intTemp[intTemp.Count-num+1];
	// }
}