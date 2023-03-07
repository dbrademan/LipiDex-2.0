using CSMSL;
using LipiDex_2._0.SpectrumSearcher;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Windows.Documents;
using System.Windows.Shapes;
using CsvHelper;
using Path = System.IO.Path;

namespace LipiDex_2._0.SpectrumSearcher
{
    public class MZXMLParser
    {
        public List<SampleSpectrum> sampleSpecArray = new List<SampleSpectrum>();
		private List<MZXMLScan> scanList = new List<MZXMLScan>();
		
        public void readFile(string filepath) 
		{

			scanList.Clear();
			sampleSpecArray.Clear();
		
			MZXMLScan scan;             // Temp scan object
            string line = "";           // String holding currently read line
			int scanNum = 0;            // Number of scan;
			bool centroided = false;    // Boolean if data is centroided
			int msLevel = -1;           // Intever of ms level
            string polarity = "";       // polarity of scan
			double retentionTime = 0.0; // retention time of scan in seconds
            double basePeakMZ = 0.0;    // Mass to charge of most intense peak
			int precision = 0;          // precision used for mz array
            string byteOrder = "";      // byte order for mzArray
            double precursor = 0.0;     // Precursor mass if MS2
            string mzArray = "";        // Encoded mz array as string

			// BufferedReader reader = new BufferedReader(new FileReader(filepath));
			// File file = new File(filepath);
			// string filename = file.getName();

			var filename = Path.GetFileName(filepath);

			using var reader = new StreamReader(filepath);
			//read line if not empty
			while ((line = reader.ReadLine()) != null)
			{
				if (line.Contains("scan num"))
				{
					scanNum = Convert.ToInt32(line.Substring(line.IndexOf("=") + 2, line.LastIndexOf("\"")));
				}

				if (line.Contains("centroided"))
				{
					if (line.Contains("1")) centroided = true;
					else centroided = false;
				}

				if (line.Contains("msLevel"))
				{
					msLevel = Convert.ToInt32(line.Substring(line.IndexOf("=") + 2, line.LastIndexOf("\"")));
				}

				if (line.Contains("polarity"))
					polarity =
						line.Substring(line.IndexOf("=") + 2, line.LastIndexOf("\""));

				if (line.Contains("retentionTime"))
					retentionTime =
						Convert.ToDouble(line.Substring(line.IndexOf("PT") + 2, line.LastIndexOf("S\""))) / 60.0;

				if (line.Contains("basePeakMz"))
					basePeakMZ =
						Convert.ToDouble(line.Substring(line.IndexOf("=") + 2, line.LastIndexOf("\"")));

				if (line.Contains("precision"))
					precision =
						Convert.ToInt32(line.Substring(line.IndexOf("=") + 2, line.LastIndexOf("\"")));

				if (line.Contains("byteOrder"))
					byteOrder =
						line.Substring(line.IndexOf("=") + 2, line.LastIndexOf("\""));

				if (line.Contains("<precursorMz") && msLevel > 1)
					precursor =
						Convert.ToDouble(line.Substring(line.IndexOf(">") + 1, line.IndexOf("</precursorMz>")));

				if (line.Contains("m/z-int"))
				{
					if (line.Contains("==</peaks>"))
						mzArray = line.Substring(line.IndexOf(">") + 1, line.IndexOf("==</peaks>"));
					else if (line.Contains("=</peaks>"))
						mzArray = line.Substring(line.IndexOf(">") + 1, line.IndexOf("=</peaks>"));
					else if (line.Contains("</peaks>"))
						mzArray = line.Substring(line.IndexOf(">") + 1, line.IndexOf("</peaks>"));
				}

				if (line.Contains("</scan>") && msLevel > 1)
				{
					//Create new scan object
					scan = new MZXMLScan(scanNum, centroided, msLevel, polarity,
						retentionTime, basePeakMZ, precision, byteOrder,
						filename, precursor, mzArray);

					//Add to scan list
					scanList.Add(scan);
					sampleSpecArray.Add(scan.ConvertToSampleSpectrum());
				}
			}
		}
    }
}