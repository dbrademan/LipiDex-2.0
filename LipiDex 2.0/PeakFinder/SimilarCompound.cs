using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static LipiDex2.Utilities.Utilities;

namespace LipiDex2.PeakFinder
{
    public class SimilarCompound
    {
        public int similarityID;
        public double actualMassShift;      //MassShift
        //public double predictedMassShift;   //TransformationMass
        //public double massShiftPPMError;    
        //public string transformationString; //Best guess of transformation type (e.g. oxidation)
        //public string compositionChange;    //Calculated molecular formula of mass shift
        //public double MSnScore;             //Unknown number
        //public double forwardCoverage;      //Unknown number
        //public double reverseCoverage;      //Unknown number
        //public int forwardMatches;          //Num. forward matches to other compounds
        //public int reverseMatches;          //Num. reverse
        public int compoundGroupID1; 
        public int compoundGroupID2;
        

        public SimilarCompound(
            int similarityID,
            double actualMassShift,
            //double predictedMassShift,
            //string transformationString,
            //string compositionChange,
            //double MSnScore,
            //double forwardCoverage,
            //double reverseCoverage,
            //int forwardMatches,
            //int reverseMatches,
            int compoundGroupID1,
            int compoundGroupID2
            )
        {
            this.similarityID = similarityID;
            this.actualMassShift = actualMassShift;
            //this.predictedMassShift = predictedMassShift;
            //this.massShiftPPMError = PPMError(actualMassShift, predictedMassShift);
            //this.transformationString = transformationString;
            //this.compositionChange = compositionChange;
            //this.MSnScore = MSnScore;
            //this.forwardCoverage = forwardCoverage;
            //this.reverseCoverage = reverseCoverage;
            //this.forwardMatches = forwardMatches;
            //this.reverseMatches = reverseMatches;
            this.compoundGroupID1 = compoundGroupID1;
            this.compoundGroupID2 = compoundGroupID2;
        }
    }
}
