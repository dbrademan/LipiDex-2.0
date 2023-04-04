using CSMSL;
using LipiDex_2._0.LibraryGenerator;
// using LipiDex_2._0.PeakFinder;
using LipiDex_2._0.SpectrumSearcher;
using NUnit.Framework;
using System;

namespace LipiDex_2._0.SpectrumSearcher;

public class MZXMLScan 
{

    double precursor;       //Precursor mass
    string file;            //Filename
    int scanNum;            //Number of scan
    bool centroided;     //Boolean if data is centroided
    int msLevel;            //Intever of ms level
    string polarity;        //polarity of scan
    double retentionTime;   //retention time of scan in seconds
    double basePeakMZ;      //Mass to charge of most intense peak
    int precision;          //precision used for mz array
    string byteOrder;       //byte order for mzArray
    string mzArray;			//Encoded array

    public MZXMLScan(int scanNum, bool centroided, int msLevel, string polarity,
            double retentionTime, double basePeakMZ, int precision, string byteOrder,
            string file, double precursor, string mzArray)
    {
        //Initialize variables
        this.scanNum = scanNum;
        this.centroided = centroided;
        this.msLevel = msLevel;
        this.polarity = polarity;
        this.retentionTime = retentionTime;
        this.basePeakMZ = basePeakMZ;
        this.precision = precision;
        this.byteOrder = byteOrder;
        this.precursor = precursor;
        this.file = file;
        this.mzArray = mzArray;

        //Verify that byteOrder is correct
        if (!byteOrder.ToLower().Equals(("network")))
            throw new Exception("mzXML compression incorrect");
    }

    //Parse m/z array using byte buffer
    public void ParseMZArray(SampleSpectrum spec)
    {
        throw new NotImplementedException();
        // double[] values;
        // byte[] decoded = Base64.getDecoder().decode(mzArray);
        //       ByteBuffer byteBuffer = ByteBuffer.wrap(decoded);
        //       byteBuffer.order(ByteOrder.BIG_ENDIAN);
        //       
        //       values = new double[byteBuffer.asDoubleBuffer().capacity()];
        // byteBuffer.asDoubleBuffer().get(values);
        //
        // if (values.Length % 2 > 0)
        //  throw new Exception("Different number of m/z and intensity values encountered in peak list.");
        //
        // for (int peakIndex = 0; peakIndex<values.Length - 1; peakIndex += 2)
        // {
        //  double mz = values[peakIndex];
        //           double intensity = values[peakIndex + 1];
        //           spec.AddFrag(mz, intensity);
        // }
    }

    //Convert MZXML file to a common sampleSpectrum type
    public SampleSpectrum ConvertToSampleSpectrum()
    {
        //Create new sample spectrum object
        SampleSpectrum spec = new SampleSpectrum(precursor, polarity, file, retentionTime, scanNum
            // msnOrder: this.msLevel
            );

    //Parse MZ array
    ParseMZArray(spec);

    return spec;
    }
}
