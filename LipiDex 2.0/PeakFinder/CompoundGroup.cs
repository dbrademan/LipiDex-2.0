using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using csgoslin;

namespace LipiDex2.PeakFinder
{
    internal class CompoundGroup
    {
        public int compoundGroupID;
        //string id;
        public LipidParser lipid;
        public double mw;
        public double maxArea;
        public int numAdducts;
        public int MSnStatus;
        public int MSDepth;
        public int isChecked;  // Maybe bool? DB Browser shows this as int

        public string referenceIon;
        public double quantIon;  // MassOverCharge column
        public string quantPolarity;
        public double retention;
        public string sumID;
        public double purity;
        public List<FileResult> fileResults;
        //public List<Compound> compounds;
        public List<Lipid> lipidCandidates;
        public List<string> sumIDs;
        public List<int> sumIDsCount;
        public List<double> quantIonArray;
        //public LipidCandidate identification = null;
        public Lipid finalLipidID = null;
        public bool keep;
        public bool ms2Sampled;
        public bool displaySumID;
        public bool plasmenylEtherConflict;
        public double avgFWHM;
        public bool noCoelutingPeaks;
        public int maxIsotopeNumber;
        public int featuresAdded;
        public bool positiveFeature;
        public bool negativeFeature;
        public List<PurityResult> summedPurities;
        public string filterReason;

        public List<SimilarCompound> similarCompoundList = new List<SimilarCompound>();
        
        //Constructor
        public CompoundGroup(
            int compoundGroupID, 
            double mw, 
            double quantIon,
            double retention, 
            double maxArea,
            List<FileResult> fileResults,
            int MSnStatus,
            int MSDepth,
            int isChecked,
            string referenceIon,

            // The remaining items are random pieces from the SQLite DB that might be interesting
            string cdID
            )
        {
            
            this.compoundGroupID = compoundGroupID;
            this.mw = mw;
            this.quantIon = quantIon;
            this.retention = retention;
            this.maxArea = maxArea;
            this.fileResults = fileResults; 
            this.MSnStatus = MSnStatus;
            this.MSDepth = MSDepth; 
            this.isChecked = isChecked; 
            this.referenceIon = referenceIon;   

            //compounds = new List<Compound>();
            summedPurities = new List<PurityResult>();
            sumIDs = new List<string>();
            sumIDsCount = new List<int>();
            lipidCandidates = new List<Lipid>();
            quantIonArray = new List<double>();
            sumID = "";
            keep = true;
            plasmenylEtherConflict = false;
            maxIsotopeNumber = 0;
            avgFWHM = 0.0;
            ms2Sampled = false;
            featuresAdded = 0;
            displaySumID = false;
            filterReason = "";
            
        }

            


        

    }
}
