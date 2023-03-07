using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using csgoslin;
using CSMSL.Chemistry;
using LipiDex_2._0.SpectrumSearcher;

namespace LipiDex_2._0.SpectrumSearcher;

public static class MspFileParser
{
    /// <summary>
    /// Reads and parses MSP files. Saves library entries into list. 
    /// This class was derived from MS-DIAL 5 source code. 
    /// </summary>
    /// <param name="databaseFilepath"></param>
    /// <param name="parser"></param>
    /// <param name="transitionTypes"></param>
    /// <returns></returns>
    public static List<LibrarySpectrum> MspFileReader(string databaseFilepath, 
        LipidParser parser, 
        Dictionary<string, TransitionType> transitionTypes)
    {
        var mspEntries = new List<LibrarySpectrum>();
        var mspEntry = new LibrarySpectrum();
        
        string line;
        int entryCounter = 0;
        int peakListCounter = 0;

        using (StreamReader sr = new StreamReader(databaseFilepath, Encoding.ASCII))
        {
            float rt = 0, preMz = 0, ri = 0, intensity = 0;

            while (sr.Peek() > -1)
            {
                line = sr.ReadLine();
                if (Regex.IsMatch(line, "^NAME:.*", RegexOptions.IgnoreCase))
                {
                    mspEntry.Id = entryCounter;
                    mspEntry.Name = getMspFieldValue(line);

                    while (sr.Peek() > -1)
                    {
                        line = sr.ReadLine();
                        if (line == string.Empty || String.IsNullOrWhiteSpace(line)) 
                            break;
                        
                        if (Regex.IsMatch(line, "PRECURSORMZ:.*", RegexOptions.IgnoreCase) ||
                            Regex.IsMatch(line, "precursor_m/z:.*", RegexOptions.IgnoreCase))
                        {
                            var fieldString = getMspFieldValue(line);
                            if (float.TryParse(fieldString, out preMz)) 
                                mspEntry.PrecursorMz = preMz;
                        }
                        
                        if (Regex.IsMatch(line, "COMMENT.*:.*", RegexOptions.IgnoreCase))
                        {
                            mspEntry.Comment = getMspFieldValue(line);
                        }
                        
                        if (Regex.IsMatch(line, "Num Peaks:.*", RegexOptions.IgnoreCase))
                        {
                            var peakNum = 0;
                            mspEntry.TransitionArray = ReadSpectrum(sr, line, transitionTypes, out peakNum);
                            mspEntry.PeakNumber = mspEntry.MzIntensityCommentList.Count;
                        }
                        if (Regex.IsMatch(line, "FORMULA:.*", RegexOptions.IgnoreCase))
                        {
                            mspEntry.Formula = getMspFieldValue(line);
                            if (mspEntry.Formula != null && mspEntry.Formula != string.Empty)
                                mspEntry.ChemicalFormula = new ChemicalFormula(mspEntry.Formula);
                        }
                        if (Regex.IsMatch(line, "IONMODE:.*", RegexOptions.IgnoreCase) ||
                            Regex.IsMatch(line, "ion_mode:.*", RegexOptions.IgnoreCase) ||
                            Regex.IsMatch(line, "Ionization:.*", RegexOptions.IgnoreCase))
                        {
                            var ionmodeString = getMspFieldValue(line);
                            if (ionmodeString == "Negative" || ionmodeString.Contains("N"))
                                mspEntry.Polarity = "-";
                            else 
                                mspEntry.Polarity = "+";
                        }
                        if (Regex.IsMatch(line, "COMPOUNDCLASS:.*", RegexOptions.IgnoreCase))
                        {
                            mspEntry.CompoundClass = getMspFieldValue(line);
                        }
                        // else if (Regex.IsMatch(wkstr, "Ontology:.*", RegexOptions.IgnoreCase))
                        // {
                        //     mspField.Ontology = getMspFieldValue(wkstr);
                        //     continue;
                        // }
                        if (Regex.IsMatch(line, "COLLISIONENERGY:.*", RegexOptions.IgnoreCase))
                        {
                            var collisionenergy = getMspFieldValue(line);
                            if (float.TryParse(collisionenergy, out rt))
                                mspEntry.CollisionEnergy = collisionenergy;
                        }
                        if (Regex.IsMatch(line, "QuantMass:.*", RegexOptions.IgnoreCase))
                        {
                            float quantmass;
                            var fieldString = getMspFieldValue(line);
                            if (float.TryParse(fieldString, out quantmass))
                                mspEntry.QuantMass = quantmass;
                            else
                                mspEntry.QuantMass = -1;
                        }
                        if (Regex.IsMatch(line, "PRECURSORTYPE:.*", RegexOptions.IgnoreCase) ||
                            Regex.IsMatch(line, "Precursor_type:.*", RegexOptions.IgnoreCase))
                        {
                            var fieldString = getMspFieldValue(line);
                            mspEntry.AdductIon =
                                fieldString; // mspField.AdductIon = AdductIonStringParser.GetAdductIonBean(fieldString);
                        }
                        if (Regex.IsMatch(line, "ISLIPIDEX:.*", RegexOptions.IgnoreCase))
                        {
                            var fieldString = getMspFieldValue(line);
                            if (fieldString.ToLowerInvariant().Contains("true"))
                                mspEntry.IsLipiDex = true;
                            mspEntry.IsLipiDex = false;
                        }

                        if (Regex.IsMatch(line, "OPTIMALPOLARITY:.*", RegexOptions.IgnoreCase))
                        {
                            var fieldString = getMspFieldValue(line);
                            if (fieldString.ToLowerInvariant().Contains("true"))
                                mspEntry.IsOptimalPolarity = true;
                            mspEntry.IsOptimalPolarity = false;
                        }

                        if (Regex.IsMatch(line, "MSNORDER:.*", RegexOptions.IgnoreCase))
                        {
                            var fieldString = getMspFieldValue(line);
                            int msOrder;
                            if (int.TryParse(fieldString, out msOrder))
                            {
                                mspEntry.MSnOrder = msOrder;
                            } 
                        }
                        
                        #region Extra MSP field parsers from MS-DIAL 5
                        // else if (Regex.IsMatch(wkstr, "RETENTIONTIME:.*", RegexOptions.IgnoreCase) ||
                        //          Regex.IsMatch(wkstr, "Retention_time:.*", RegexOptions.IgnoreCase))
                        // {
                        //     var rtString = getMspFieldValue(wkstr);
                        //     if (float.TryParse(rtString, out rt)) mspField.RetentionTime = rt;
                        //     else mspField.RetentionTime = -1;
                        //     continue;
                        // }
                        // else if (Regex.IsMatch(wkstr, "RT:.*", RegexOptions.IgnoreCase))
                        // {
                        //     var rtString = getMspFieldValue(wkstr);
                        //     if (float.TryParse(rtString, out rt)) mspField.RetentionTime = rt;
                        //     else mspField.RetentionTime = -1;
                        //     continue;
                        // }
                        // else if (Regex.IsMatch(wkstr, "RETENTIONINDEX:.*", RegexOptions.IgnoreCase) ||
                        //          Regex.IsMatch(wkstr, "Retention_index:.*", RegexOptions.IgnoreCase))
                        // {
                        //     var rtString = getMspFieldValue(wkstr);
                        //     if (float.TryParse(rtString, out ri)) mspField.RetentionIndex = ri;
                        //     else mspField.RetentionIndex = -1;
                        //     continue;
                        // }
                        // else if (Regex.IsMatch(wkstr, "RI:.*", RegexOptions.IgnoreCase))
                        // {
                        //     var rtString = getMspFieldValue(wkstr);
                        //     if (float.TryParse(rtString, out ri)) mspField.RetentionIndex = ri;
                        //     else mspField.RetentionIndex = -1;
                        //     continue;
                        // }
                        
                        // else if (Regex.IsMatch(wkstr, "CollisionCrossSection:.*", RegexOptions.IgnoreCase) ||
                        //          Regex.IsMatch(wkstr, "CCS:.*", RegexOptions.IgnoreCase))
                        // {
                        //     var fieldString = getMspFieldValue(wkstr);
                        //     if (float.TryParse(fieldString, out preMz)) mspField.CollisionCrossSection = preMz;
                        //     continue;
                        // }
                        // else if (Regex.IsMatch(wkstr, "Links:.*", RegexOptions.IgnoreCase))
                        // {
                        //     mspField.Links = getMspFieldValue(wkstr);
                        //     continue;
                        // }
                        // else if (Regex.IsMatch(wkstr, "Intensity:.*", RegexOptions.IgnoreCase))
                        // {
                        //     var fieldString = getMspFieldValue(wkstr);
                        //     if (float.TryParse(fieldString, out intensity)) mspField.Intensity = intensity;
                        //     else mspField.Intensity = -1;
                        //     continue;
                        // }
                        // else if (Regex.IsMatch(wkstr, "SCANNUMBER:.*", RegexOptions.IgnoreCase))
                        // {
                        //     var fieldString = getMspFieldValue(wkstr);
                        //     mspField.Comment += fieldString;
                        //     continue;
                        // }
                        // else if (Regex.IsMatch(wkstr, "INSTRUMENTTYPE:.*", RegexOptions.IgnoreCase))
                        // {
                        //     mspField.InstrumentType = getMspFieldValue(wkstr);
                        //     continue;
                        // }
                        // else if (Regex.IsMatch(wkstr, "INSTRUMENT:.*", RegexOptions.IgnoreCase))
                        // {
                        //     mspField.Instrument = getMspFieldValue(wkstr);
                        //     continue;
                        // }
                        // else if (Regex.IsMatch(wkstr, "SMILES:.*", RegexOptions.IgnoreCase))
                        // {
                        //     mspField.Smiles = getMspFieldValue(wkstr);
                        //     continue;
                        // }
                        // else if (Regex.IsMatch(wkstr, "InChIKey:.*", RegexOptions.IgnoreCase))
                        // {
                        //     mspField.InchiKey = getMspFieldValue(wkstr);
                        //     continue;
                        // }
                        #endregion
                        
                    }
                    mspEntry.Library = Path.GetFileNameWithoutExtension(databaseFilepath); // Add Library source 

                    mspEntries.Add(mspEntry);
                    mspEntry = new LibrarySpectrum();
                    entryCounter++;
                }
            }
        }

        return mspEntries;
    }

    private static string getMspFieldValue(string wkstr)
    {
        return wkstr.Substring(wkstr.Split(':')[0].Length + 1).Trim();
    }
    
    public static List<Transition> ReadSpectrum(
        StreamReader sr, string numPeakField, 
        Dictionary<string, TransitionType> transitionTypes, 
        out int peaknum)
    {
        peaknum = 0;
        var mspPeaks = new List<Transition>();

        if (int.TryParse(numPeakField.Split(':')[1].Trim(), out peaknum))
        {
            if (peaknum == 0)
            {
                return mspPeaks;
            }

            var pairCount = 0;
            var mspPeak = new MzIntensityComment();

            while (pairCount < peaknum)
            {
                var line = sr.ReadLine();
                
                if (string.IsNullOrWhiteSpace(line)) { break; }
                
                var split = line.Split(' ');
                var mz = double.Parse(split[0]);
                var intensity = double.Parse(split[1]);
                
                // Pattern  "([^\"]*)"  matches content inside two quotes.
                //      See: https://stackoverflow.com/questions/171480/regex-grabbing-values-between-quotation-marks
                var comment = Regex.Match(line, "([^\"]*)").Value;
                var transition = new Transition(mz, intensity, comment);
                transition.ParseComment(transitionTypes);
                mspPeaks.Add(transition);
            }
        }
        
        mspPeaks.Sort();
        
        return mspPeaks;
    }
}





