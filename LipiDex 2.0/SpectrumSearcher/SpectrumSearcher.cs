using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.SpectrumSearcher
{
    internal class SpectrumSearcher
    {


        public SpectrumSearcher()
        {

        }

        public void RunSpectraSearch()
        {
            /* 
             * Steps
             * 0. Read fatty acids, adducts, lipid classes and lipid categories
             * 1. Read each rawfile using new CSMSL
             *      2. For each spectrum:
             *             Run library matching with custom weighted cosine similarity as implemented by Paul
             *      3. Calculate spectral purity 
             *      4. 
             * 
             * 
             * 
             * 
             * 
             */ 
        }

        public void MatchLibrarySpectra(
            SampleSpectrum ms2,
            double massBinSize,
            double ms1Tol,
            double ms2Tol
            )
        {

        }


    }
}
