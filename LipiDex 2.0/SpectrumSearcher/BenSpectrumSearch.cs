using System.Collections.Generic;
using System.IO;

namespace LipiDex2.SpectrumSearcher
{
    public class BenSpectrumSearch
    {
        public List<string> rawFiles;
        public double ms1Tol;
        
        public BenSpectrumSearch(
            List<string> rawFiles,
            double ms1Tol)
        {
            this.rawFiles = rawFiles;
            this.ms1Tol = ms1Tol
            
        }

        public void RunSpectraSearch()


    }
}