using LipiDex2.LibraryGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex2.SpectrumSearcher
{
    public class SampleSpectrum 
    {
        public double precursor;                             //Precursor mz for MSn
        public double retention;                             //Retention time
        public string polarity;                              //MSn polarity
        public string file;                                  //file
        public int msnOrder;                            //Whether MS1, MS2, MS3, etc. 
        public List<Transition> transitionArray;      //List of all (mz, int) peaks. Transitions can also refer to specific fatty acyl losses or other neutral losses
        public List<Identification> idArray;                 //A spectrum can have multiple plausible IDs, store them here
        public List<double> allMatchedMassesArray;           //all Transition mzs that match to a known fragment   [Shouldn't this be in the Identification object?]
        double maxIntensity;                          //Highest intensity in spectrum
        double maxIntensityMass;                      //mz of highest intensity in spectrum
        int spectrumNumber = 0;                       //the scan number from the rawfile
        PeakPurity peakPurity = null;                 //the spectrum's peak purity

        public SampleSpectrum(
            double precursor, string polarity, 
            string file, double retention, int spectrumNumber,
            int msnOrder
            )
            
        {
            this.precursor = precursor;  //For some reason, Paul rounds to 3 decimal places
            this.polarity = polarity;
            this.file = file;
            this.msnOrder = msnOrder;
            this.transitionArray = new List<Transition>();
            this.idArray = new List<Identification>();
            allMatchedMassesArray = new List<double>();
            maxIntensity = 0.0;
            maxIntensityMass = 0.0;
            this.retention = Math.Round(precursor, 3);
            this.spectrumNumber = spectrumNumber;
        }

        public double CalcDotProduct(
            List<Transition> libArray,
            double mzTol,
            bool reverse,
            double massWeight,
            double intWeight
            )
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

            transitionArray.Sort();
            libArray.Sort();

            int i = 0, j = 0;

            //Both indexers must be less than their respective array size (duh)
            while (i < libArray.Count() && j < transitionArray.Count())
            {
                massDiff = transitionArray[j].mass - libArray[i].mass;
                if (Math.Abs(massDiff) > mzTol)
                {
                    if (libArray[i].mass < transitionArray[j].mass)
                    {
                        libIntensities.Add(libArray[i].intensity);
                        sampleIntensities.Add(0.0);
                        sampleMasses.Add(0.0);
                        libMasses.Add(libArray[i++].mass);  //Note increment i++                   
                    }
                    else if (libArray[i].mass > transitionArray[j].mass)
                    {
                        sampleIntensities.Add(transitionArray[j].mass);
                        libIntensities.Add(0.0);
                        sampleMasses.Add(transitionArray[j++].mass);
                        libMasses.Add(0.0);
                    }
                }
                else
                {
                    sampleMasses.Add(transitionArray[j].mass);
                    libMasses.Add(libArray[i].mass);
                    libIntensities.Add(libArray[i++].intensity);
                    sampleIntensities.Add(transitionArray[j++].intensity);
                }
            }
            // LipiDex1 comment: "Print remaining elements of the larger array" 
            while (i < libArray.Count())
            {
                libIntensities.Add(libArray[i].intensity);
                sampleIntensities.Add(0.0);
                sampleMasses.Add(0.0);
                libMasses.Add(libArray[i++].mass);
            }
            while (j < transitionArray.Count())
            {
                sampleIntensities.Add(transitionArray[j].intensity);
                libIntensities.Add(0.0);
                sampleMasses.Add(transitionArray[j++].mass);
                libMasses.Add(0.0);
            }

            //Iterate through unique masses
            for (int k = 0; k < libMasses.Count(); k++)
            {
                //If only in sample
                if (libIntensities[k] == 0.0)
                {
                    if (!reverse) sampleSum += Math.Pow(massWeight * sampleMasses[k] * Math.Pow(sampleIntensities[k] / 2.0, intWeight), 2);
                }
                //If only in library
                else if (sampleIntensities[k] == 0.0)
                {
                    if (!reverse) libSum += Math.Pow(massWeight * libMasses[k] * Math.Pow(libIntensities[k], intWeight), 2);
                }
                //If in both
                else
                {
                    libSum += Math.Pow(Math.Pow(libMasses[k], massWeight) * Math.Pow(libIntensities[k], intWeight), 2);
                    sampleSum += Math.Pow(Math.Pow(sampleMasses[k], massWeight) * Math.Pow(sampleIntensities[k], intWeight), 2);
                    numerSum += Math.Pow(sampleMasses[k], massWeight) * Math.Pow(sampleIntensities[k], intWeight) * Math.Pow(libMasses[k], massWeight) * Math.Pow(libIntensities[k], intWeight);
                }
            }

            //Calculate dot product
            if (numerSum > 0.0) result = 1000.0 * Math.Pow(numerSum, 2) / (sampleSum * libSum);

            return result;
        }

        public void CalcPurityAll(List<LibraryGenerator.FattyAcid> fattyAcidsDB, double mzTol)
        {
            idArray.Sort();
            List<LibrarySpectrum> ls = new List<LibrarySpectrum>();
            if (idArray.Count() > 0)
            {
                if (!idArray[0].librarySpectrum.name.Equals(""))
                {

                }
            }
        }

        public void ScaleIntensities()
        {
            throw new NotImplementedException();
        }

        public void AddID(LibrarySpectrum ls, double dotProduct,
            double reverseDotProduct, double mzTol)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public void AddFrag(double mass, double intensity)
        {
            transitionArray.Add(new Transition(mass, intensity, transitionType: null));
            if (intensity > maxIntensity)
            {
                maxIntensity = intensity;
                maxIntensityMass = mass;
            }
        }



    }
}
