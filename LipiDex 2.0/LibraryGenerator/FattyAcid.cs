using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CSMSL.Chemistry;

namespace LipiDex_2._0.LibraryGenerator
{
    public class FattyAcid
    {
		public static FattyAcidComparer FattyAcidComparer = new FattyAcidComparer();
		public ChemicalFormula formula { get; set; }              //Elemental formula
		public string name { get; set; }                 //Abbreviated name
		public double mass;              //Mass for sorting purpose
		public int carbonNumber;        //Number of carbons in FA chain
		public int doubleBondNumber;            //Number of double bonds in FA chain
		public bool polyUnsaturatedFattyAcid = false;        //True iff fatty acids is a polyunsaturated fatty acid
		public bool enabled { get; set; }     //True iff the fatty acid will be used for library generation
		public string fattyAcidCategory { get; set; }                 //Type of fatty acid


		//Constructor
		public FattyAcid(string name, string type, string formula, string enabled)
		{
			//Initialize class variables
			this.name = name;
			this.formula = new ChemicalFormula(formula);
			this.mass = this.formula.MonoisotopicMass;
			this.fattyAcidCategory = type;

			if (enabled.Equals("true"))
			{
				this.enabled = true;
			}
			else if (enabled.Equals("false"))
            {
				this.enabled = false;
            }
			else
            {
				throw new ArgumentException(string.Format("Fatty_acids.csv parsing error for fatty acid \"{0}\". Only 'true' or 'false' are accepted values for the `Enabled` column.", name));
            }

			//Decide whether fatty acid is a PUFA
			var unsaturationString = name.Split(':')[1];

			// I can't imagine a FA with +10 unsaturations, but just in case, build out logic....
			var doubleBondEquivalents = -1;

			try
            {
				doubleBondEquivalents = Convert.ToInt32(unsaturationString);
			}
			catch (FormatException e)
            {
				throw new ArgumentException(string.Format("Fatty_acids.csv parsing error for fatty acid \"{0}\". Cannot parse DBE from fatty acid name. Make sure there are only numbers after the \":\" character.", name));
            }

			if (doubleBondEquivalents > 1)
            {
				this.polyUnsaturatedFattyAcid = true;
            }

			//Parse fatty acid name for carbon and db number calculation
			ParseFattyAcid();
		}

		/// <summary>
		/// Return elemental formula of fatty acid
		/// </summary>
		/// <returns>
		/// Fatty acid chemical formula as a string
		/// </returns>
		/// <remarks>
		/// Should refactor all formulae options to use CSMSL ChemicalFormula Object
		/// </remarks>
		public string GetFormula()
		{
			return this.formula;
		}

		/// <summary>
		/// Get name of a fatty acid
		/// </summary>
		/// <returns>
		/// (string) Fatty acid name (as defined in the library)
		/// </returns>
		public string GetName()
		{
			return this.name;
		}

		/// <summary>
		/// Get the mass of a fatty acid
		/// </summary>
		/// <returns>
		/// (double) Fatty acid mass (as defined from the fatty acid's chemical formula)
		/// </returns>
		public double GetMass()
		{
			return mass;
		}

		/// <summary>
		/// Comparator for sorting fatty acids by type and molecular weight
		/// </summary>
		/// <returns>
		/// (int) 
		/// 1 if this fatty acid should be sorted first.
		/// -1 if the other should be sorted first. 
		/// 0 if the fatty acids are equal.
		/// </returns>
		public int CompareTo(FattyAcid otherFattyAcid)
		{
			if (!otherFattyAcid.fattyAcidCategory.Equals(this.fattyAcidCategory))
			{
				if (char.IsLetter(otherFattyAcid.name[0]) && !char.IsLetter(this.name[0]))
                {
					return 1;
				}
				else if (!char.IsLetter(otherFattyAcid.name[0]) && char.IsLetter(this.name[0]))
                {
					return -1;
				}
				else
				{
					if (this.mass > otherFattyAcid.GetMass()) 
					{
						return 1;
					}
						
					else if (mass < otherFattyAcid.GetMass())
					{
						return -1;
					}
					else 
					{
						return 0;
					}
				}
			}
			else
			{
				if (this.mass > otherFattyAcid.GetMass())
				{
					return 1;
				}
				else if (this.mass < otherFattyAcid.GetMass())
				{
					return -1;
				}
				else
				{
					return 0;
				}
			}
		}

		/// <summary>
		/// Returns string representation of FA for txt file generation
		/// </summary>
		/// <returns>
		/// (string) 
		/// string representation of a fatty acid for text file generation
		/// </returns>
		public string SaveString()
		{
			string result = "";

			result += this.name + ",";
			result += this.fattyAcidCategory + ",";
			result += this.formula + ",";

			if (enabled)
			{
				result += true;
			}
			else 
			{ 
				result += false; 
			}

			return result;
		}

		/// <summary>
		/// Parses carbon number and double bond number from the name of a fatty acid.
		/// Saves the carbon and double bond numbers to object properties.
		/// </summary>
		public void ParseFattyAcid()
		{
			List<string> splitName = this.name.Split(':').ToList();

			// Remove all letter characters
			for (int i = 0; i < splitName.Count; i++)
			{
				splitName[i] = Regex.Replace(splitName[i], "[^\\d.]", "");
				splitName[i] = splitName[i].Replace("-", "");
			}

			// Find carbon number
			this.carbonNumber = Convert.ToInt32(splitName[0]);

			// Find double bond number
			this.doubleBondNumber = Convert.ToInt32(splitName[1]);
		}

		/// <summary>
		/// Returns fatty acid name
		/// Overrides default ToString() method.
		/// </summary>
		public override string ToString()
		{
			return this.name;
		}

		// format 
		public List<string> GetTableArray()
        {
			List<string> result = new List<string>();

			result.Add(this.name);
			result.Add(this.fattyAcidCategory);
			result.Add(this.formula);
			result.Add(this.enabled.ToString());

			return result;
		}
	}

	/// <summary>
	/// Custom Comparer for Fatty Acids
	/// </summary>
	public class FattyAcidComparer : Comparer<FattyAcid>
	{
		// Compares by Length, Height, and Width.
		public override int Compare(FattyAcid thisFattyAcid, FattyAcid otherFattyAcid)
		{
			if (!otherFattyAcid.fattyAcidCategory.Equals(thisFattyAcid.fattyAcidCategory))
			{
				if (char.IsLetter(otherFattyAcid.name[0]) && !char.IsLetter(thisFattyAcid.name[0]))
				{
					return 1;
				}
				else if (!char.IsLetter(otherFattyAcid.name[0]) && char.IsLetter(thisFattyAcid.name[0]))
				{
					return -1;
				}
				else
				{
					if (thisFattyAcid.mass > otherFattyAcid.GetMass())
					{
						return 1;
					}

					else if (thisFattyAcid.mass < otherFattyAcid.GetMass())
					{
						return -1;
					}
					else
					{
						return 0;
					}
				}
			}
			else
			{
				if (thisFattyAcid.mass > otherFattyAcid.GetMass())
				{
					return 1;
				}
				else if (thisFattyAcid.mass < otherFattyAcid.GetMass())
				{
					return -1;
				}
				else
				{
					return 0;
				}
			}
		}
	}
}
