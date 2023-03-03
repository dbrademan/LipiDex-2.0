using LipiDex2.LibraryGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace LipiDex2.SpectrumSearcher
{
    public class PeakPurity
    {
        List<Transition> transitions = new List<Transition>();
        List<LibrarySpectrum> lipids = new List<LibrarySpectrum>();
        List<double> intensities = new List<double>();
        List<double> matchedMasses = new List<double>();
        List<int> purities = new List<int>();
        List<double> comboIntensities = new List<double>();
        List<double> topMasses = new List<double>();
        List<double> intensityCorrectionArray = new List<double>();
        double intensityCorrection = 0.0;
        double mzTol = 0.01;
        double minPurity = 5.0;
        int carbonNumber = 0;
        int unsatNumber = 0;
        int numFattyAcids = 0;
        int totalFattyAcids = 0;
        string chainGreatest = "";

        public int CalcPurity(
            string lipidName, 
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
            this.transitions = transitions;
            this.mzTol = mzTol;

            //Count number of glycerol substitutions
            for (int i = 0; i < lipidName.Length; i++)
            {
                if (lipidName[i] == ':')
                    totalFattyAcids++;
            }

            //Calculate purity for first match
            purity = getPurityIntensity(ls, faDB, true);

            //if (purity == 0.0) return 0;
            if (purity > 0.0)
            {
                intensities.Add(purity);
                lipids.Add(ls);

                //Add matches mass for in-source fragment screening
                for (int j = 0; j < ls.transitionArray.Count(); j++)
                {
                    matchedMasses.Add(ls.transitionArray[j].mass);
                }
            }

            //Calculate purity intensity for all other isobaric ids
            for (int i = 0; i < isobaricIDs.Count(); i++)
            {
                purity = getPurityIntensity(isobaricIDs[i], faDB, false);
                if (purity > 0.0 && isUniqueLipid(isobaricIDs[i]))
                {
                    intensities.Add(purity);
                    lipids.Add(isobaricIDs[i]);
                    //If purity>10 add matches mass for in-source fragment screening
                    if (purity > minPurity)
                    {
                        for (int j = 0; j < isobaricIDs[i].transitionArray.Count(); j++)
                        {
                            matchedMasses.Add(isobaricIDs[i].transitionArray[j].mass);
                        }
                    }
                }
            }

            //Calculate purity percentage
            for (int i = 0; i < intensities.Count(); i++)
            {
                sumInt += intensities[i];
            }

            if (intensities.Count() < 1) return 0;

            purityInt = (int)Math.Round((intensities[0] / sumInt) * 100);

            //Calculate purity percentage for all species
            for (int i = 0; i < intensities.Count(); i++)
            {
                purities.Add((int)Math.Round((intensities[i] / sumInt) * 100));
            }

            return purityInt;
        }

        //Returns true iff lipid not contained in purity array
        private bool isUniqueLipid(LibrarySpectrum l)
        {
            for (int i = 0; i < lipids.Count(); i++)
                if (lipids[i].name.Contains(l.name)) return false;
            return true;
        }

        private List<FattyAcid> parseFattyAcids(LibrarySpectrum ls, List<FattyAcid> faDB, string fattyAcidType)
        {
            List<FattyAcid> lipidFAs = new List<FattyAcid>();

            //Remove class name
            string faString = ls.name.Substring(ls.name.IndexOf(" ") + 1, ls.name.LastIndexOf(" "));

            //Split name into FAs
            string[] split = faString.Split('_');

            //Iterate and find alkyl matches
            for (int i = 0; i < split.Count(); i++)
            {
                //Iterate through fadb
                for (int j = 0; j < faDB.Count(); j++)
                {
                    //Add to temp array
                    if (faDB[j].name.Equals(split[i]) && faDB[j].type.equals(fattyAcidType))
                    {
                        lipidFAs.Add(faDB[j]);
                    }
                }
            }

            return lipidFAs;
        }

        //Returns the intensity of each moiety-based fragment
        private List<double> findFAIntensities(
            LibrarySpectrum ls, 
            List<FattyAcid> faDB,
            string transitionType, 
            double minInt, 
            bool topMatch)
        {
            double mass = 0.0;
            double intensity = 0.0;
            int massIndex = 0;
            double correctionIntensity;
            List<double> matchedIntensities = new List<double>();
            int faCount = 0;


            //For all library transitions
            for (int i = 0; i < ls.transitionArray.Count(); i++)
            {
                //Add matching entries to libmasses array
                if (ls.transitionArray[i].type.Equals(transitionType))
                {
                    mass = ls.transitionArray[i].mass;

                    //Divide intensity by number of times fa occurs
                    intensity = findTransitionMatch(mass);

                    //Add if found
                    if (intensity > minInt)
                    {
                        //Apply intensity correction
                        if (getIndexOf(topMasses, mass) > -1)
                        {
                            intensity = intensity - intensityCorrectionArray[getIndexOf(topMasses, mass)];
                            if (intensity < 0.0) intensity = 0.0;
                        }

                        //Divide by number of occurences if less than 3 fatty acids
                        //intensity = intensity/numOccurences(ls.transitionArray.get(i).fattyAcid,faDB);

                        for (int j = 0; j < NumOccurences(ls.transitionArray[i].fattyAcid, faDB); j++)
                        {
                            matchedIntensities.Add(intensity);
                            faCount++;
                        }
                    }
                }
            }

            if (faCount != faDB.size()) return null;

            //If all found and top hit, add intensities and masses to top arrays
            for (int i = 0; i < ls.transitionArray.size(); i++)
            {
                //index of mass in topMasses array
                massIndex = getIndexOf(topMasses, ls.transitionArray.get(i).mass.doubleValue());

                //Correction is smallest of matched intensities
                Collections.sort(matchedIntensities);
                correctionIntensity = GetMedianChainIntensity(matchedIntensities);

                //If unique and correct transition type, add to array
                if (!topMasses.contains(ls.transitionArray.get(i).mass.doubleValue())
                        && ls.transitionArray.get(i).type.equals(transitionType))
                {
                    topMasses.add(ls.transitionArray.get(i).mass.doubleValue());
                    intensityCorrectionArray.add(correctionIntensity);
                }
                else if (ls.transitionArray.get(i).type.equals(transitionType))
                {
                    //Add correction intensity to array
                    double newCorrection = intensityCorrectionArray.get(massIndex) + correctionIntensity;

                    //If greater than actual spectral peak, change to original spectrum intensity
                    if (newCorrection > findTransitionMatch(ls.transitionArray.get(i).mass))
                        newCorrection = findTransitionMatch(ls.transitionArray.get(i).mass);

                    //Change correction
                    intensityCorrectionArray.set(massIndex, newCorrection);
                }
            }

            return matchedIntensities;
        }

        private int NumOccurences(string fattyAcid, List<FattyAcid> faDB)
        {
            int count = 0;

            foreach (FattyAcid fa in faDB) // (int i = 0; i < faDB.size(); i++)
            {
                if (fa.name.Equals(fattyAcid)) count++;
            }
            return count;
        }

        private double getPurityIntensity(LibrarySpectrum ls, List<FattyAcid> faDB, bool topMatch)
        {
            string transitionType = "";
            double maxFragType = 0;
            List<FattyAcid> lipidFAs = new List<FattyAcid>();
            List<double> matchedIntensities = new List<double>();

            if (!ls.isLipidex) return 0.0;

            //Iterate through transitions to find most intense fatty acid fragment type
            try
            {
                for (int i = 0; i < ls.transitionArray.Count(); i++)
                {
                    if (!ls.transitionArray[i].type.Contains("_Fragment") && !ls.transitionArray[i].type.Contains("_Neutral Loss")
                            && !ls.transitionArray[i].type.Contains("DG Fragment") && !ls.transitionArray[i].type.Contains("PUFA"))
                    {
                        if (ls.transitionArray[i].intensity > maxFragType)
                        {
                            maxFragType = ls.transitionArray[i].intensity;
                            transitionType = ls.transitionArray[i].type;
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
                lipidFAs = parseFattyAcids(ls, faDB, transitionType.Substring(transitionType.IndexOf("_") + 1).replace(" Fragment", ""));
            else if (transitionType.Contains(" Neutral Loss"))
                lipidFAs = parseFattyAcids(ls, faDB, transitionType.Substring(transitionType.IndexOf("_") + 1).replace(" Neutral Loss", ""));

            //Find intensity for all fatty acid peaks
            matchedIntensities = findFAIntensities(ls, lipidFAs, transitionType, maxFragType * .05, topMatch);

            //Check if all fragments were found
            if (matchedIntensities == null) return 0.0;

            //For 2 or less fatty acids, return median
            if (matchedIntensities.size() < 3 || topMatch)
                return GetMedianChainIntensity(matchedIntensities);

            else
                return matchedIntensities.get(matchedIntensities.size() - 1);
        }

        /*
         * If the putative mass is found within the ms2 mass tolerance, 
         * add to FA and intensity array.  The number of times added corresponds
         * to the number of possible fa sights available for purity analysis
         */
        private double findTransitionMatch(double mass)
        {
            foreach (Transition t in transitions)
            {
                if (Math.Abs(t.mass - mass) < mzTol)
                {
                    return t.intensity;
                }
            }
            return 0.0;
        }


        public static void skipPermutation(int[] limits, int[] counters)
        {
            //Set first counter to maximum
            counters[0] = limits[0];
        }

        //Calculate appropriate median spectral intensity
        private double GetMedianChainIntensity(List<double> intTemp)
        {
            return Statistics.Median(intTemp);
            //if (intTemp.Count() > 0)
            //{
            //    int middle;

            //    //Sort intensity array
            //    intTemp.Sort();

            //    //Calculate median, if no middle point, move to the lower intensity
            //    middle = intTemp.Count() / 2;

            //    if (intTemp.Count() % 2 == 1)
            //    {
            //        return intTemp[middle];
            //    }
            //    else
            //    {
            //        return ((intTemp[middle] + intTemp[(middle - 1)]) / 2.0);
            //    }
            //}
            //else
            //{
            //    return 0.0;
            //}
        }
        //Returns the n value of an array
        private double getNChainIntensity(List<double> intTemp, int num)
        {
            //Sort intensity array
            intTemp.Sort();

            //Return third
            return intTemp[(intTemp.Count() - num)];
        }

        //Returns the n value of an array, if not above threshold return n+1
        private double getNChainIntensityAboveMin(List<double> intTemp, int num, double min)
        {
            //Sort intensity array
            intTemp.Sort();

            if (intTemp.Count() - num > min)
                return intTemp[(intTemp.Count() - num)];
            else
                return intTemp[(intTemp.Count() - num + 1)];
        }

        //Returns the lowest value above zero
        private double getMinIntensityAboveZero(List<double> intTemp)
        {
            //Sort intensity array
            intTemp.Sort();

            for (int i = 0; i < intTemp.Count(); i++)
            {
                if (intTemp[i] > 0.001) return intTemp[i];
            }

            return 0.0;
        }

        //Returns index of double value
        private int getIndexOf(List<double> array, double num)
        {
            for (int i = 0; i < array.Count(); i++)
            {
                if (Math.Abs(array[i] - num) < 0.000001)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}