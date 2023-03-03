using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex2.PeakFinder
{
    public class Sample
    {
        public string file;        // filepath TODO: make into Path() object that contains the location of the .raw file 
        public string filename;    // the full filename
        public string filepath;    // the filepath as given in the CD StudyInformation table
        
        public int cdFileID;    // Compound Discoverer File ID
        public string sampleID;
        public string studyFileID;

        public bool ms2Counted;        // true iff MS2 associated  TODO: MSn object?
        public int polarityFileNumber; // File number of joining separate polarity files
        public string mergedName;
        public string polarity;       // polarity of file if separate polarities collected
        public double peakCapacity;

        // Iterable Variables
        public int lipids;  // Number of associated lipids
        public int compoundsFound; // Num. compounds found by Compound Discoverer
        public double maxFeatureRT; // Maximum retention time

        // Avg. Variables
        public double ppmError;
        public double ms2RTDev; // TODO: MSn object
        public double avgPurity;
        public double avgFWHM;

        // Avg. variable arrays
        public List<double> ppmErrorArray;
        public List<double> ms2RTDevArray; // TODO: MSn object
        public List<int> avgPurityArray;
        public List<double> avgFWHMArray;
        public List<RTDeviation> featureRTDevArray;

        public Sample(string filename, string filepath, int cdFileID, string sampleID, string studyFileID,
            int polarityFileNumber, bool separatePolarities)
        {
            this.filename = filename;
            this.cdFileID = cdFileID;
            this.filepath = filepath;
            this.sampleID = sampleID;
            this.studyFileID = studyFileID;
            maxFeatureRT = 0.0;
            ms2Counted = false; // TODO: MSn object
             
             
        }

        // Loess fit retention time and rt deviations to backwards-calculate RT correction across files
        // Hopefully not needed because we have direct access to RT corrections from .cdResults file
        public void LoessFit() { }

        //public double FindRTOffset(double rt) { }

        //public double GetCorrectedRT(double rt) { }

        // Sets maximum retention time for sample
        public void CheckMaxRT(double rt)
        {
            if (rt > maxFeatureRT) maxFeatureRT = rt;
        }

        public void GetCorrectedRT(double retention)
        {

        }

        // We can calculate average of an array using a library
        // public void CalculateAverages() { }
        // We can calculate average of an array using a library
        // public double CalcAvgFromIntArray(List<int> array) { }
        // CalcAvgFromDoubleArray()

        // Public Array.Add() methods. Hopefully we can avoid
        //public void AddLipid() { lipids++; }
        //public void AddCompound() { compoundsFound++; }
        //public void AddPPMError(double error) { ppmErrorArray.Add(error);  }
        //public void AddRTDev(double dev) { ms2RTDevArray.Add(dev);  }
        //public void AddPurity(int purity) { avgPurityArray.Add(purity); }
        //public void AddFWHM(double FWHM) { avgFWHMArray.Add(FWHM); }

        // Returns string with various pieces of info for writing to results CSVs.
        // We should include each entry in its own column
        //public string ToString() { return result; }


    }
}
