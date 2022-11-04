using System;
using System.ComponentModel;
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

		// This set of properties are used as intermediate placeholders during editing of the data grid.
		public bool isDirty;
		public string name { get; set; }
		public string formula { get; set; }
		public string type { get; set; }
		public bool enabled { get; set; }

		// these variables are the used store final verisons of the Fatty Acid Object
		public string _name { get; private set; }                // Abbreviated name
		public ChemicalFormula _formula { get; private set; }    // Elemental formula
		public string _fattyAcidCategory { get; private set; }   // Type of fatty acid
		public bool _enabled { get; private set; }               // True iff the fatty acid will be used for library generation

		// these variables are algorthimically calculated when public object properties are set
		public double mass { get; set; }							//Mass for sorting purpose
		public int carbonNumber { get; set; }                    //Number of carbons in FA chain
		public int doubleBondNumber { get; set; }                //Number of double bonds in FA chain
		public bool polyUnsaturatedFattyAcid { get; set; }       //True iff fatty acids is a polyunsaturated fatty acid

		// Constructor will set final versions of fatty acids directly.
		public FattyAcid(string name, string type, string formula, string enabled)
		{
			//Initialize class variables
			this.isDirty = true;
			this._name = name;
			this._fattyAcidCategory = type;

			try
			{
				this._formula = new ChemicalFormula(formula);
			}
			catch (Exception e)
			{
				throw new ArgumentException(string.Format("Parsing error for chemical formula \"{0}\". The exact error follows:\n{1}", name, e.Message));
			}
			
			if (enabled.Equals("true") || enabled.Equals("True") || enabled.Equals("TRUE"))
			{
				this._enabled = true;
			}
			else if (enabled.Equals("false") || enabled.Equals("False") || enabled.Equals("FALSE"))
            {
				this._enabled = false;
            }
			else
            {
				throw new ArgumentException(string.Format("Parsing error for fatty acid \"{0}\". Only 'true' or 'false' are accepted values for the `Enabled` column.", name));
            }

			//Parse fatty acid name for carbon and db number calculation
			ParseFattyAcid();

			// finally, set the temporary variables (which are actually displayed in the data grid
			this.name = _name;
			this.formula = _formula.ToString();
			this.type = _fattyAcidCategory;
			this.enabled = _enabled;
			this.mass = this._formula.MonoisotopicMass;

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

			this.isDirty = false;
		}

		// Constructor to create a fatty acid from the intermediate variables of another fatty acid
		public FattyAcid(FattyAcid otherFattyAcid)
        {
			//Initialize class variables
			this.isDirty = true;
			this._name = otherFattyAcid.name;
			this._formula = new ChemicalFormula(otherFattyAcid.formula);
			this._fattyAcidCategory = otherFattyAcid.type;
			this._enabled = otherFattyAcid.enabled;

			//Parse fatty acid name for carbon and db number calculation
			ParseFattyAcid();

			// finally, set the temporary variables (which are actually displayed in the data grid
			this.name = _name;
			this.formula = _formula.ToString();
			this.type = _fattyAcidCategory;
			this.enabled = _enabled;
			this.mass = this._formula.MonoisotopicMass;

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

			this.isDirty = false;
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
			return this._formula;
		}

		/// <summary>
		/// Get name of a fatty acid
		/// </summary>
		/// <returns>
		/// (string) Fatty acid name (as defined in the library)
		/// </returns>
		public string GetName()
		{
			return this._name;
		}

		/// <summary>
		/// Get the mass of a fatty acid
		/// </summary>
		/// <returns>
		/// (double) Fatty acid mass (as defined from the fatty acid's chemical formula)
		/// </returns>
		public double GetMass()
		{
			return this._formula.MonoisotopicMass;
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
			if (!otherFattyAcid._fattyAcidCategory.Equals(this._fattyAcidCategory))
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

			result += this._name + ",";
			result += this._fattyAcidCategory + ",";
			result += this._formula.ToString() + ",";

			if (this._enabled)
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
		private void ParseFattyAcid()
		{
			List<string> splitName = this._name.Split(':').ToList();

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
			return this._name;
		}

		// in short:
		// take the intermediate variables from the FattyAcid object
		// try to create
		/// <summary>
		/// Takes the intermediate variables from the supplied FattyAcid object and tries to parse them into a new fattyAcid object. If parsing fails, this method returns false.
		/// </summary>
		public static bool ValidateFattyAcid(FattyAcid dirtyFattyAcid, out Exception thrownError)
        {
			thrownError = null;
			try
            {
				new FattyAcid(dirtyFattyAcid);
            }
			catch (Exception e)
            {
				thrownError = e;
				return false;
            }

			return true;
		}

		public static bool ValidateFattyAcidField(FattyAcid dirtyFattyAcid, int editedColumnIndex, out Exception thrownError)
        {
			switch (editedColumnIndex)
            {
				case 0:
					break;

				case 1:
					break;

				case 2:
					break;
				
				case 3:
					break;

				default:
					break;
            }

			thrownError = null;

			return false;
        }
	}
}
