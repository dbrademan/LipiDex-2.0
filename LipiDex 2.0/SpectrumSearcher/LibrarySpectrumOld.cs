// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.RegularExpressions;
// using System.Windows.Documents;
//
// using CSMSL.IO.Thermo;
// using CSMSL.Spectral;
// using static System.TimeZoneInfo;
//
//
// namespace LipiDex_2._0.SpectrumSearcher
// {
//     public class LibrarySpectrumOld
//     {
//         public double precursor;
//         public string polarity;
//         public string name;
//         public string lipidClass;
//         public string adduct;
//         public string sumID;
//         public List<Transition> transitionArray;
//         public double maxIntensity;
//         public double maxIntensityMass;
//         public string library;
//         public bool isLipidex;
//         public bool optimalPolarity;
//
//         public LibrarySpectrumOld(
//             double precursor, string polarity,
//             string library, bool isLipidex, bool optimalPolarity)
//             
//         {
//             this.precursor = precursor;
//             this.polarity = polarity;  
//             this.name = name.Substring(1);
//             transitionArray = new List<Transition>();
//             maxIntensity = 0.0;
//             maxIntensityMass = 0.0;
//             this.library = library;
//             this.isLipidex = isLipidex;
//             if (isLipidex) this.sumID = GetSumID(this.name);
//         }
//
//         public override string ToString()
//         {
//             string result = precursor + "," + polarity;
//             return result;
//         }
//
//         private string GetSumID(string id)
//         {
//             String result = "";
//             int numDB = 0;
//             int numC = 0;
//             String lipidClass;
//             String faString;
//             String toAddToFA = "";
//             String[] faSplit;
//             int j;
//
//             lipidClass = id.Substring(0, id.IndexOf(" "));
//
//             //Remove class and adduct
//             faString = id.Substring(id.IndexOf(" ") + 1, id.LastIndexOf(" "));
//             //TODO:
//             /*
//             //Parse plasmenyl
//             if (faString.contains("P-"))
//             {
//                 faString = faString.replace("P-", "");
//                 toAddToFA = "P-";
//             }
//             //Parse ether
//             else if (faString.contains("O-"))
//             {
//                 faString = faString.replace("O-", "");
//                 toAddToFA = "O-";
//             }
//             //Parse sphingoid
//             else if (faString.contains("d"))
//             {
//                 faString = faString.replace("d", "");
//                 toAddToFA = "d";
//             }
//             //Parse methyl fatty acids
//             else if (faString.contains("m"))
//             {
//                 faString = faString.replace("m", "");
//                 toAddToFA = "m";
//             }
//              */
//
//
//             //Split string based on fatty acids
//             faSplit = faString.Split('_');
//
//             for (int i = 0; i < faSplit.Length; i++)
//             {
//                 //if lipid string does not contain "-"
//                 if (faSplit[i].Contains("-"))
//                 {
//                     j = faSplit[i].LastIndexOf("-") + 1;
//
//                     //Find integer for first number in string
//                     Regex pattern = Regex("^\\D*(\\d)");
//                     Regex matcher = Regex.Match(faSplit[i].Substring(j), "^\\D*(\\d)");
//                     matcher.find();
//                     j = j + matcher.start(1);
//
//                 }
//                 else
//                 {
//                     //Find integer for first number in string
//                     Pattern pattern = Regex.compile("^\\D*(\\d)");
//                     Matcher matcher = pattern.matcher(faSplit[i]);
//                     matcher.find();
//                     j = matcher.start(1);
//                 }
//
//
//                 //Remove modifier and add in later
//                 if (j > 0)
//                 {
//                     toAddToFA += faSplit[i].Substring(0, j);
//                     faSplit[i] = faSplit[i].Replace(faSplit[i].Substring(0, j), "");
//                 }
//
//                 numC += Convert.ToInt32(faSplit[i].Substring(0, faSplit[i].IndexOf(":")));
//                 numDB += Convert.ToInt32(faSplit[i].Substring(faSplit[i].IndexOf(":") + 1));
//             }
//
//             result = lipidClass + " " + toAddToFA + numC + ":" + numDB;
//
//             return result;
//         }
//
//         private void ParseName()
//         {
//             String[] split;
//
//             //Split name
//             split = name.Split(' ');
//             this.lipidClass = split[0];
//
//             //Capitalize lipid Class
//             this.lipidClass = this.lipidClass.Substring(0, 1).ToUpper() + this.lipidClass.Substring(1);
//
//             this.adduct = split[2];
//             this.adduct = this.adduct.Replace(";", "");
//         }
//
//         //A method to scale intensities to max. intensity on scale of 0-999
//         public void ScaleIntensities()
//         {
//             for (int i = 0; i < transitionArray.Count; i++)
//             {
//                 transitionArray[i].intensity = (transitionArray[i].intensity / maxIntensity) * 999;
//
//                 if (transitionArray[i].intensity < 5)
//                 {
//                     transitionArray.RemoveAt(i);
//                     i--;
//                 }
//             }
//         }
//
//         public void AddFrag(double mass, double intensity, string type, TransitionType transitionType)
//
//         {
// 		    Transition t = new Transition(mass, intensity, transitionType);
// 		    if (!type.Equals("")) t.AddType(type);
// 		    transitionArray.Add(t);
// 		    transitionArray.Sort();
//
// 		    if (intensity>maxIntensity)
// 		    {
// 			    maxIntensity = intensity;
// 			    maxIntensityMass = mass;
// 		    }
//         }
//
//
// public int CompareTo(LibrarySpectrumOld target)
//         {
//             throw new NotImplementedException();
//         }
//
//         // CalculatePPMDiff() function -- use Utilities
//
//         // In LipidexSpectrum class
//         //public void AddFrag(double mass, double intensity,
//         //    string type, TransitionType transitionType)
//             
//         //{
//         //    throw new NotImplementedException();
//         //}
//
//         // Unused functions in LipiDex1: CheckFrag(), AddName()
//
//         
//     }
// }