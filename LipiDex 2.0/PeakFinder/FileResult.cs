using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.PeakFinder
{
    /* 
    Contains all the data for one file, for one compound group 
    
    The double values will be populated from the blob columns in .cdResults

    The total number of FileResult objects will be (num. Compound groups) x (num. raw files analyzed) 
    */
    public class FileResult
    {
        // File metadata
        Sample file;            // associated file object
        string mergedFileName;  // for separate polarity runs
        string polarity;
        bool merged;

        // Quant data
        double area;
        double pqfFwhm2Base;
        double pqfJaggedness;
        double pqfModality;
        double pqfZigZag;
        double peakRating;
        double gapStatus;
        //double backupGapStatus;  // Unusual column in cdResults
        double gapFillStatus;
        
        public FileResult(
            Sample file, double area, double pqfFwhm2Base, double pqfJaggedness, double pqfModality, 
            double pqfZigZag, double peakRating, double gapStatus)
        {
            this.file = file;
            this.area = area;
            this.pqfFwhm2Base = pqfFwhm2Base;
            this.pqfJaggedness = pqfJaggedness;
            this.pqfModality = pqfModality;
            this.pqfZigZag = pqfZigZag;
            this.peakRating = peakRating;
            this.gapStatus = gapStatus;
            //this.backupGapStatus = backupGapStatus;
            //this.gapFillStatus = gapFillStatus;

            mergedFileName = "";
            merged = false;

        }

        public void AddMergedFileName(string name)
        {
            this.mergedFileName = name;
        }

        public void SetPolarity(string polarity)
        {
            this.polarity = polarity;
        }

        




    }
}
