using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.SpectrumSearcher
{
    public class SampleSpectrum
    {
        double precursor;
        double retention;
        string polarity;
        string file;
        public List<Transition> transitionArray;
        List<Identification> idArray;
        List<double> allMatchedMassesArray;
        double maxIntensity;
        double maxIntensityMass;
        int spectrumNumber;
        PeakPurity peakPurity = null;

        public SampleSpectrum(double precursor, string polarity, string file, double retention, int spectrumNumber)
        {
            this.precursor = precursor;  //For some reason, Paul does Math.round(precursor * 1000) / 1000 to round to 3 decimal places
            this.polarity = polarity;
            this.file = file;
            this.transitionArray = new List<Transition>();

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




            return result;
        }

    }
}
