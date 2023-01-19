using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CSMSL;
using CSMSL.Chemistry;

namespace LipiDex_2._0.LibraryGenerator
{
    //Class with parameters and utilities used by all classes
	//All the formula math seems really hacky. Should convert to CSMSL at some point...
	public static class Utilities 
	{
		//User-supplied parameters
		public static double ANNOTATIONPPMTOL = 5.0;										//Maximum ppm diff for annotation
		public static double DOTPRODUCTPPMTOL = 10.0;										//Minimum ppm diff for dot product calculation
		public static double MINANNOTATIONMZTOL = 0.01;										//Minimum mz different for annotionat
		public static List<string> elements = new List<string>() {"C","H","O","P","N","S","Na"};	
		public static List<double> masses = new List<double>()										//Accurate masses for all elements
		{
			1.007825035,2.014101779,7.016003,11.0093055,12.0,
			13.00335483,14.003074,15.00010897,15.99491463,17.9991603,18.99840322,
			22.9897677,23.9850423,30.973762,31.9720707,34.96885272,38.9637074,39.9625906,
			51.9405098,54.9380471,55.9349393,57.9353462,58.9331976,62.9295989,63.9291448,
			74.9215942,78.9183361,79.9165196,97.9054073,105.903478,106.905092,113.903357,
			126.904473,196.966543,201.970617
		};												

		public static List<string> elementsArray = new List<string>()						//All allowed elements
		{
			"H","Xa","Li","B","C","Xb","N","Xc","O","Xd","F","Na","Mg","P","S","Cl","K","Ca",
			"Cr","Mn","Fe","Ni","Co","Cu","Zn","As","Br","Se","Mo","Pd","Ag","Cd","I","Au","Hg"
		};	
		
		public static List<List<string>> heavyElementArray = new List<List<string>>()		//Symbols for heavy elements
		{	
			new List<string>(){"(2H)","Xa"},
			new List<string>(){"(13C)","Xb"},
			new List<string>(){"(15N)","Xc"},
			new List<string>(){"(18O)","Xd"}
		};					
		
		public static double MASSOFELECTRON = 0.00054858026;								//Mass of an electron

		//Calculate the mass difference in ppm
		public static double CalculatePPMDifference(double mass1, double mass2)
		{
			double result = 0.0;

			var mzDifference = mass1 - mass2;

			result = 1000000 * (Math.Abs(mzDifference) / (Math.Abs(mass2)));
			
			return result;
		}

		//Removes heavy elements from formula for later calculations
		public static string RemoveHeavyElements(string formula)
		{
			string result = formula;

			for (int i = 0; i < heavyElementArray.Count; i++)
			{
				if (formula.Contains(heavyElementArray[i][0]))
				{
					result = result.Replace(heavyElementArray[i][0], heavyElementArray[i][1]);
					result = result.Replace("\\)", "");
					result = result.Replace("\\(", "");
				}
			}

			return result;
		}

		//Round a number to 4 decimal placed
		public static double RoundToFourDecimals(double input)
		{
			return Math.Round(input, 4);
		}

		//Returns true iff a string is a valid elemental formula
		public static bool IsFormula(string formulaString)
		{
			string newFormula = RemoveHeavyElements(formulaString);

			if (formulaString.Equals("-"))
			{
				return true;
			}
		
			for (int i = 0; i < elementsArray.Count; i++)
			{
				if (newFormula.Contains(elementsArray[i]))
				{
					return true;
				}
			}

			return false;
		}

		//Returns the putative elementa formula of a mass based on user-prodvided constrains
		public static string AnnotateMassWithMZTolerance(double fragmentMass, string formula, double minimumMzTolerance, double minimumRingDoubleBondEquivalents)
		{
			List<string> elements = FormulaToElementArray(formula);
			List<int> limits = FormulaToCountArray(formula);
			List<int> counters = new List<int>(elements.Count);
			double massTemporary = 0.0;
			string formulaMinimumPPM = "";
			double mzDifferenceTemporary = 0.0;
			double minimumMz = minimumMzTolerance;

			while (true) {

				//If mass is greater than frag mass, stop iteration of element
				massTemporary =  CalculateMassFromFormula(ArrayToFormula(elements, counters));

				if (massTemporary > fragmentMass + 1)
				{
					SkipPermutation(limits, counters);
				}

				//Check if formula minimizes ppm error
				mzDifferenceTemporary = Math.Abs(fragmentMass- massTemporary);

				if (mzDifferenceTemporary < minimumMz && CalculateRingDoubleBondEquivalents(ArrayToFormula(elements, counters)) >= minimumRingDoubleBondEquivalents)
				{
					minimumMz = mzDifferenceTemporary;
					formulaMinimumPPM = ArrayToFormula(elements, counters);
				}

				// Advance permutation
				if (!NextPermutation(limits, counters))
				{
					break;
				}
			}

			return formulaMinimumPPM;
		}

		//Check if a formula is a valid elemental formula
		public static bool ValidElementalFormula(string input)
		{
			bool result = true;
			List<int> intArray;
			List<string> elementArray;

			//Possible elements
			List<bool> validArray;

			try
			{
				if (input.Equals("-"))
				{
					return true;
				}

				if (!input.Equals(""))
				{

					string newFormula = RemoveHeavyElements(input);

					//parse out integers
					intArray = FormulaToCountArray(newFormula);

					//Parse out elements
					elementArray = FormulaToElementArray(newFormula);
					validArray = new List<bool>(elementArray.Count);

					//Iterate through elements
					for (int i = 0; i < intArray.Count; i++)
					{
						validArray[i] = false;

						//For each, find MI mass and add
						for (int j = 0; j < elementsArray.Count; j++)
						{
							//If element match, add to final result
							if (elementsArray[j].Equals(elementArray[i]))
							{
								validArray[i] = true;
							}
						}

						if (!validArray[i])
						{
							result = false;
						}
					}


					if ((elementArray.Count != intArray.Count) || elementArray.Count < 1)
					{
						result = false;
					}
				}
				else
				{
					return false;
				}

				return result;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		//Calculate the monoisotopic mass from a formula, 
		public static double CalculateMassFromFormula(string input)
		{
			double result = 0.0; //mass
			List<int> intArray;
			List<string> elementArray;

			if (input.Equals("-"))
			{
				return 0.0;
			}
		
			//parse out integers
			intArray = FormulaToCountArray(input);

			//Parse out elements
			elementArray = FormulaToElementArray(input);

			//Iterate through elements
			for (int i = 0; i < elementArray.Count; i++)
			{
				//For each, find MI mass and add
				for (int j = 0; j < elementsArray.Count; j++)
				{
					//If element match, add to final result
					if (elementsArray[j].Equals(elementArray[i]))
					{
						result = result + masses[j] * Convert.ToDouble(intArray[i]);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Takes two chemical formulas and sums them together. This is a CSMSL adaption from PDH's original implementation
		/// </summary>
		/// <returns>
		/// Return the two chemical formulas combined together.
		/// </returns>
		public static ChemicalFormula MergeFormulas(ChemicalFormula formula1, ChemicalFormula formula2)
        {
			var returnFormula = new ChemicalFormula();
			
			returnFormula.Add(formula1);
			returnFormula.Add(formula2);

			return returnFormula;
        }

		/// <summary>
		/// Takes two chemical formulas and sums them together. This overload uses strings instead of ChemicalFormula objects to aid in refactoring PDH's original implementation
		/// </summary>
		/// <returns>
		/// Return the two chemical formulas combined together.
		/// </returns>
		public static ChemicalFormula MergeFormulas(string formula1, string formula2)
        {
			var returnFormula = new ChemicalFormula();

			returnFormula.Add(new ChemicalFormula(formula1));
			returnFormula.Add(new ChemicalFormula(formula2));

			return returnFormula;
		}

		//Add two elemental compositions together
		// Paul's original implementation of MergeFormulas
		/*
		public static string MergeFormulas(string formula1, string formula2)
		{
			string result = "";

			string merged = formula1+formula2;

			List<int> intArray; //numbers for formula
			List<string> elementArray; //elements for formula

			//parse out integers
			intArray = FormulaToCountArray(merged);

			//Parse out elements
			elementArray = FormulaToElementArray(merged);

			//iterate through element array and merge results
			for (int i = 0; i < elementArray.Count; i++)
			{
				//if not a blank look for duplicate
				if (!elementArray[i].Equals("X"))
				{
					//iterate through looking for another entry
					for (int j = i+1; j < elementArray.Count; j++)
					{
						//if match found, merge results
						if (elementArray[i].Equals(elementArray[j]))
						{
							intArray[i] = Convert.ToInt32(intArray[i]) + Convert.ToInt32(intArray[j]);
							intArray[j] = 0;
							elementArray[j] = "X";
						}
					}
				}
			}

			//Iterate through and produce final string
			for (int i = 0; i < elementArray.Count; i++)
			{
				if (!elementArray[i].Equals("X"))
				{
					result = result + elementArray[i]+intArray[i];
				}
			}

			return result;
		}
		*/

		//Converts a formula to an array of all contained elements
		public static List<string> FormulaToElementArray(string formula)
		{
			List<string> split;
			string formulaTemp = RemoveHeavyElements(formula);

			split = Regex.Split(formulaTemp, "(?<!^)(?=[A-Z])").ToList();

			return split;
		}

		//Returns an integer array of the count of all elements in formula
		public static List<int> FormulaToCountArray(string formula)
		{
			string temporaryString = RemoveHeavyElements(formula);
			List<string> elementArray = FormulaToElementArray(temporaryString);

			//Remove elements from formula and replace with comma
			for (int i = 0; i < elementArray.Count; i++)
			{
				temporaryString = temporaryString.Replace(elementArray[i], ", ");

				var tempVariable = temporaryString.IndexOf(elementArray[i]) + elementArray[i].Length;

				temporaryString = temporaryString.Substring(0, temporaryString.IndexOf(elementArray[i])) + ", " + 
									temporaryString.Substring(temporaryString.IndexOf(elementArray[i]) + elementArray[i].Length);		
			}

			//Split into array
			List<string> temp = temporaryString.Split(',').ToList();
			List<int> result = new List<int>(elementArray.Count);

			if (temp.Count == 0)
			{
				//Add counts to array
				for (int i = 0; i < result.Count-1; i++)
				{
						result[i] = 1;
				}
			}

			else
			{
				//Add counts to array
				for (int i=0; i < temp.Count-1; i++)
				{
					if (!temp[i+1].Equals(" "))
                    {
						result[i] = Convert.ToInt32(temp[i + 1].Substring(1));
					}
					else
                    {
						result[i] = 1;
					}
				}
			}

			//Trim array
			if (temp.Count < elementArray.Count + 1)
			{
				result[result.Count - 1] = 1;
			}

			return result;
		}

		//Annotate mass with putatuve elements formula within mass tolerance
		public static string AnnotateMassWithMZTolerance(double fragMass, string formula)
		{
			List<string> elements = FormulaToElementArray(formula);
			List<int> limits = FormulaToCountArray(formula);
			List<int> counters = new List<int>(elements.Count);
			double massTemp = 0.0;
			string formulaMinPPM = "";
			double mzDiffTemp = 0.0;
			double minMZ = MINANNOTATIONMZTOL;

			while (true) {

				//If mass is greater than frag mass, stop iteration of element
				massTemp = CalculateMassFromFormula(ArrayToFormula(elements, counters));

				if (massTemp > fragMass + 1)
				{
					SkipPermutation(limits, counters);
				}

				//Check if formula minimizes ppm error
				mzDiffTemp = Math.Abs(fragMass - massTemp);

				if (mzDiffTemp < minMZ)
				{
					minMZ = mzDiffTemp;
					formulaMinPPM = ArrayToFormula(elements, counters);
				}


				// Advance permutation
				if (!NextPermutation(limits, counters)) {
					break;
				}
			}

			return formulaMinPPM;
		}

		//Annotate mass
		public static string AnnotateMass(double fragMass, string formula)
		{
			List<string> elements = FormulaToElementArray(formula);
			List<int> limits = FormulaToCountArray(formula);
			List<int> counters = new List<int>(elements.Count);
			double massTemp = 0.0;
			string formulaMinPPM = "";
			double ppmTemp = 0.0;
			double minPPM = ANNOTATIONPPMTOL;
		
			while (true) {

				//If mass is greater than frag mass, stop iteration of element
				massTemp =  CalculateMassFromFormula(ArrayToFormula(elements, counters));

				if (massTemp > fragMass+1)
				{
					SkipPermutation(limits, counters);
				}

				//Check if formula minimizes ppm error
				ppmTemp = PpmDifference(fragMass, massTemp);

				if (ppmTemp < minPPM)
				{
					minPPM = ppmTemp;
					formulaMinPPM = ArrayToFormula(elements, counters);
				}


				// Advance permutation
				if (!NextPermutation(limits, counters)) break;
			}
			return formulaMinPPM;
		}

		public static string AnnotateMassWithExceptions(double fragMass, string formula, List<string> forbiddenElements)
		{
			List<string> elements = FormulaToElementArray(formula);
			List<int> limits = FormulaToCountArray(formula);
			List<int> counters = new List<int>(elements.Count);
			double massTemp = 0.0;
			string formulaMinPPM = "";
			string formulaTemp = "";
			double ppmTemp = 0.0;
			double minPPM = DOTPRODUCTPPMTOL;
			bool containsForbidden = false;
		
			while (true) {

				containsForbidden = false;

				//If mass is greater than frag mass, stop iteration of element
				massTemp =  CalculateMassFromFormula(ArrayToFormula(elements, counters));
				if (massTemp > fragMass+1)
				{
					SkipPermutation(limits, counters);
				}

				//Check if formula minimizes ppm error
				ppmTemp = PpmDifference(fragMass, massTemp);
				formulaTemp = ArrayToFormula(elements, counters);


				//Check for forbidden Elements
				for (int i = 0; i < elements.Count; i++)
				{
					for (int j = 0; j < forbiddenElements.Count; j++)
					{
						if (counters[i] > 0 && elements[i].Equals(forbiddenElements[j]))
						{
							containsForbidden = true;
						}
					}
				}

				if (ppmTemp < minPPM && !containsForbidden)
				{
					minPPM = ppmTemp;
					formulaMinPPM = formulaTemp;
				}


				// Advance permutation
				if (!NextPermutation(limits, counters))
				{
					break;
				}
			}
			return formulaMinPPM;
		}

		//Method to convert element array string to formula string
		public static string ElementArrayToString (string input)
		{
			string result = ""; //result string
			List<string> elements= new List<string>(); //array of all unique elements
			List<int> counts = new List<int>(); //array of element counts
			string temp;

			//iterate through string
			for (int i = 0; i < input.Length; i++)
			{
				//if a unique character add the count and element
				if (!elements.Contains(Convert.ToString(input[i])))
				{
					temp = input;
					elements.Add(Convert.ToString(input[i]));
					counts.Add(temp.Length - temp.Replace(Convert.ToString(input[i]), "").Length);
				}
			}

			//Create result string
			for (int i = 0; i < elements.Count; i++)
			{
				result = result + elements[i];
				result = result + counts[i];
			}

			return result;
		}

		//Method to calculate mass difference in ppm
		public static double PpmDifference(double massA, double massB)
		{
			double ppmResult = 0.0;

			ppmResult = Math.Abs((Math.Abs(massA) - Math.Abs(massB))/Math.Abs(massB))*1000000;

			return ppmResult;
		}

		//Method to advance array of counters for generation of all permutations
		public static bool NextPermutation(List<int> limits, List<int> counters)
		{
			int c = 0; // the current counter
			counters[c]++; // increment the first counter
			while (counters[c] > limits[c]) // if counter c overflows
			{
				counters[c] = 0; // reset counter c
				c++; // increment the current counter
				if (c >= limits.Count) {
					return false; // if we run out of counters, we're done
				}
				counters[c]++;
			}
			return true;
		}

		//Method to skip counters
		public static void SkipPermutation(List<int> limits, List<int> counters)
		{
			//Set first counter to maximum
			counters[0] = limits[0];
		}
	
		//Method to calculate the ring double bond equivalent
		public static double CalculateRingDoubleBondEquivalents(string formula)
		{
			double result = 1.0;
		
			List<string> elements = FormulaToElementArray(formula);
			List<int> count = FormulaToCountArray(formula);

			for (int i=0; i<elements.Count; i++)
			{
				if (elements[i].Equals("H")) result -= count[i]/2.0;
				else if (elements[i].Equals("N")) result += count[i]/2.0;
				else if (elements[i].Equals("C")) result += count[i];
				else if (elements[i].Equals("P")) result += count[i]/2.0;
			}
		
			return result;
		}

		//Converts arrays of elements and counts to formula
		public static string ArrayToFormula(List<string> elements, List<int> counts)
		{
			string result = "";

			for (int i = 0; i < elements.Count; i++)
			{
				if (counts[i] > 0)
				{
					result = result+elements[i];
					result = result+counts[i]; 
				}
			}

			return result;
		}

		//Invert Elemental Formula
		public static string InvertFormula(string formula)
		{
			string result = "";
			List<int> intArray = FormulaToCountArray(formula);
			List<string> elementArray = FormulaToElementArray(formula);
		
			//Iterate through elements
			for (int i=0; i<intArray.Count; i++)
			{
				intArray[i] = intArray[i]*-1;
			}

			result = ArrayToFormula(elementArray, intArray);

			return result;
		}

		//Decides if mass could come from a fatty acid fragment or neutral loss
		public static string FindFattyAcidFragment(double mass, FattyAcid fattyAcid)
		{
			string fattyAcidFormula = fattyAcid.GetFormula();
			string annotatedFormula = "";

			//if mass is at least 0.75 mass of total fatty acid
			if ((mass/fattyAcid.GetMass())>.75 && mass<fattyAcid.GetMass()+20.0)
			{
				//Attempt to annotate fragments as fatty acid, include +2h
				annotatedFormula = AnnotateMass(mass, MergeFormulas(fattyAcidFormula,"H2"));
			}

			return annotatedFormula;
		}
	}

}
