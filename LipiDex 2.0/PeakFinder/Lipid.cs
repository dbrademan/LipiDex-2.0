using CSMSL.Analysis.ExperimentalDesign;
using LipiDex_2._0.LibraryGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.PeakFinder
{
    public class Lipid  // Inherit from Comparable<Lipid> 
    {
        public double retention;
        public double correctedRetention;
        public double precursor;
        public Sample sample;
        public double dotProduct;
        public double revDotProduct;
        public double gaussianScore;
        public string lipidString;
        public string lipidClass;
        public string lipidName;
        public string sumLipidName;
        public string polarity;
        public string adduct;
        public int charge;
        public MSn ms2 = null;
        public int purity = 0;
        public bool preferredPolarity;
        public double ppmError;
        public bool keep;
        public bool isFattyAcidLipid;
        public bool isLipiDex;
        public List<string> fattyAcids;
        public List<double> fragmentMasses;
        public List<PurityResult> purityArray;

        public Lipid(
            double RT,
            double precursor,
            //double sample,
            double dotProduct,
            double revDotProduct,
            string lipidString,
            double libPrecursor,
            int purity,
            bool preferredPolarity,
            bool isLipiDex,
            List<PurityResult> purityArray,
            List<double> fragmentMasses
            )
        {
            fattyAcids = new List<string>();
            this.fragmentMasses = fragmentMasses;
            this.retention = retention;
            this.precursor = precursor;
            //this.sample = sample;
            this.dotProduct = dotProduct;
            this.revDotProduct = revDotProduct;
            this.lipidString = lipidString;
            this.purity = purity;
            this.preferredPolarity = preferredPolarity;
            this.isLipiDex = isLipiDex;

            ppmError = ((precursor - libPrecursor) / libPrecursor) * 1000000.0;
            keep = true;
            gaussianScore = 0.0;

            //this.correctedRetention = sample.GetCorrectedRT(retention);
            this.purityArray = purityArray;

            // Change sample stats
            //sample.AddPPMError(ppmError);

            // Parse class, adduct, FAs
            //Utils.LipidParser(this.lipidString).ParseString();

            // Parse polarity
            if (this.adduct.Contains("]+"))
            {
                polarity = "+";
            }
            else
            {
                polarity = "-";
            }

            lipidName = ToString();
            // Parse charge
            if (this.adduct.Contains("2-")) { charge = 2; }
            else { charge = 1; }



        }

        public int CompareTo(Lipid l)
        {
            if (!preferredPolarity && l.preferredPolarity) return 1;
            else if (preferredPolarity && !preferredPolarity) return -1;

            // If same polarity, sort by Gaussian score
            else
            {
                if (gaussianScore > l.gaussianScore) return -1;
                else return 1;
            }

        }



    }
}
