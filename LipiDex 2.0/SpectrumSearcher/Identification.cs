using csgoslin;
using System;
using System.Windows.Documents;
using System.Collections.Generic;

using LipiDex2.LibraryGenerator;
using System.Diagnostics;

namespace LipiDex2.SpectrumSearcher
{
    public class Identification
    {
        public LibrarySpectrum librarySpectrum;
        public double deltaMass;
        public double dotProduct;
        public double reverseDotProduct;
        public int purity;

        public Identification(LibrarySpectrum ls, double deltaMass, double dotProduct, double reverseDotProduct)
        {
            this.librarySpectrum = ls;
            this.deltaMass = Math.Round(deltaMass, 5);
            this.dotProduct = dotProduct;
            this.reverseDotProduct = reverseDotProduct;
            purity = 0;
        }
        //Calculate purity between different classes
        public PeakPurity CalculatePurityAll(
            List<LibraryGenerator.FattyAcid> faDB, 
            List<LibrarySpectrum> lipidDB,
            List<Transition> ms2, 
            double mzTol)
        {
            List<LibrarySpectrum> isobaricIDs = new List<LibrarySpectrum>();
            PeakPurity calculator = new PeakPurity();
            //Find isobaric IDs
            foreach (LibrarySpectrum libSpectrum in lipidDB)
            {
                if (Math.Abs(librarySpectrum.precursor - libSpectrum.precursor) < mzTol)
                {
                    isobaricIDs.Add(libSpectrum);
                }
            }
            //Verify the spectrum is a LipiDex ID  
            if (librarySpectrum.isLipidex)
            {
                if (librarySpectrum.optimalPolarity)
                {
                    this.purity = calculator.CalcPurity(librarySpectrum.name,
                        librarySpectrum.sumID,
                        ms2,
                        librarySpectrum.precursor,
                        librarySpectrum.polarity,
                        librarySpectrum.lipidClass,
                        librarySpectrum.adduct,
                        faDB,
                        librarySpectrum,
                        isobaricIDs,
                        mzTol);
                }
                else this.purity = 0;
            }
            return calculator;
        }

        //Compare IDs based on dot products
        //TODO: implement comparable
        public int CompareTo(Identification i)
        {
            if (dotProduct > i.dotProduct) return -1;
            else if (dotProduct < i.dotProduct) return 1;
            else return 0;
        }

        public string ToString()
        {
            string result = librarySpectrum.name + " " + dotProduct;
            return result;
        }


    }
}