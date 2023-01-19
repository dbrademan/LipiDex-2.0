using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows;
using CSMSL.Chemistry;

namespace LipiDex_2._0.LibraryGenerator
{
    public class FattyAcid : LipidMoiety
    {
		public static FattyAcidComparer FattyAcidComparer = new FattyAcidComparer();

        // Inherited properties from LipidMoiety
        #region Inherited LipidMoiety Properties & Methods

			#region Properties

			// public string name { get; set; }                                  // Abbreviated moiety name for data grid
			// public string formula { get; set; }                               // Moiety elemental formula for data grid
			// protected string _name;                                           // Abbreviated name
			// protected ChemicalFormula _formula;                               // Elemental formula

			#endregion

			#region Methods

			// protected string GetFormulaString()

			// protected ChemicalFormula GetChemicalFormula()

			// protected bool ValidateMoietyName(string textToValidate, int rowNumber)

			// protected bool ValidateMoietyFormula(string textToValidate, int rowNumber)

			// protected bool IsInteger(string textToValidate)

			#endregion

        #endregion

        // This set of properties are used as intermediate placeholders during editing of the data grid.
        #region Fatty Acid Properties - Data Grid Display

        public string type { get; set; }								// Type of fatty acid
		public bool enabled { get; set; }								// True iff the fatty acid will be used for library generation

		#endregion

		// these variables are the used store final verisons of the Fatty Acid Object
		#region Fatty Acid Properties - Private

		private string _fattyAcidCategory;								// Type of fatty acid
		private bool _enabled;											// True iff the fatty acid will be used for library generation

		#endregion

		// these variables are algorthimically calculated when public object properties are set
		#region Fatty Acid Properties - Dependant on private variables

		public double mass { get; private set; }						//Mass for sorting purpose
		public int carbonNumber { get; private set; }                   //Number of carbons in FA chain
		public int doubleBondNumber { get; private set; }               //Number of double bonds in FA chain
		public bool polyUnsaturatedFattyAcid { get; private set; }      //True iff fatty acids is a polyunsaturated fatty acid

        #endregion

        /// <summary>
        /// Takes in string versions of an adduct's name, fatty acid type (categorical), chemical formula, and if it's enabled ("true"/"false")
        /// </summary>
        public FattyAcid(string name, string type, string formula, string enabled)
		{
			//Initialize class variables
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

			// finally, set the temporary variables (which are actually displayed in the data grid)
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
		}

		// Constructor to create a fatty acid from the intermediate variables of another fatty acid
		public FattyAcid(FattyAcid otherFattyAcid)
        {
			//Initialize class variables
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
				result += "true";
			}
			else 
			{ 
				result += "false"; 
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

		/// <summary>
		/// Takes the intermediate variables from the supplied FattyAcid object and tries to parse them into a new fattyAcid object. If parsing fails, this method returns false.
		/// </summary>
		public bool IsValid(int rowNumber)
        {
			if (this.ValidateFattyAcidName(this.name, rowNumber) && this.ValidateFattyAcidType(this.type, rowNumber) &&	this.ValidateFattyAcidFormula(this.formula, rowNumber))
            {
				return true;
            }
			return false;
		}

		/// <summary>
		/// Takes in a edited fatty acid name and makes sure it's parsable by LipiDex's architecture. Uses the FattyAcid object constructer logic. Saves result to internal variable if valid. 
		/// </summary>
		/// <returns>
		/// true if the fatty acid name is valid and parsable. Returns false otherwise.
		/// </returns>
		public bool ValidateFattyAcidName(string textToValidate, int rowNumber)
        {
			try
            {
				List<string> splitName = textToValidate.Split(':').ToList();

				if (splitName.Count != 2)
                {
					throw new NotImplementedException("LipiDex can only parse fatty acids with one colon {\':\'} character.");
                }
				// Remove all letter characters
				for (int i = 0; i < splitName.Count; i++)
				{
					splitName[i] = Regex.Replace(splitName[i], "[^\\d.]", "");
					splitName[i] = splitName[i].Replace("-", "");
				}

				// Find carbon number
				var tempCarbonNumber = Convert.ToInt32(splitName[0]);

				// Find double bond number
				var doubleBondNumber = Convert.ToInt32(splitName[1]);

				//Decide whether fatty acid is a PUFA
				var unsaturationString = name.Split(':')[1];

				// I can't imagine a FA with +10 unsaturations, but just in case, build out logic....
				var doubleBondEquivalents = -1;
				var polyUnsaturatedFattyAcid = false;

				try
				{
					doubleBondEquivalents = Convert.ToInt32(unsaturationString);
				}
				catch (FormatException e)
				{
					throw new ArgumentException(string.Format("Fatty_acids.csv parsing error for fatty acid \"{0}\" in row {1}. Cannot parse DBEs from fatty acid name. Make sure there are only numbers after the \":\" character.", name, rowNumber + 1));
				}

				if (doubleBondEquivalents > 1)
				{
					polyUnsaturatedFattyAcid = true;
				}

				// if we made it this far, name should be valid. Save properties to object.
				this._name = textToValidate;
				this.name = textToValidate;
				this.polyUnsaturatedFattyAcid = polyUnsaturatedFattyAcid;
				this.carbonNumber = tempCarbonNumber;

				return true;
			}
			catch (Exception e)
            {
				var messageBoxQuery = string.Format("Fatty acid parsing error in table column 1, row {0}.\n\n " +
					"The exact error message is as follows:\n{1}\n\n" +
					"Fatty acid name should take the format:\n" +
                    "\"[TEXT][CarbonNumber][TEXT]:[TEXT][Unsaturation][TEXT]\"\n" +
                    "(e.g. d18:1).\n" +
                    "[TEXT] segments are optional.\n\n", rowNumber + 1, e.Message);
				var messageBoxShortPrompt = string.Format("Fatty Acid Parsing Error!");
				var messageBoxButtonOptions = MessageBoxButton.OK;
				var messageBoxImage = MessageBoxImage.Error;

				MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

				return false;
			}
			
        }

		/// <summary>
		/// Takes in a edited fatty acid type. Saves result to internal variable if it's not blank. The fatty acid type is somewhat freeform. 
		/// </summary>
		/// <returns>
		/// true if the fatty acid type is not blank. Returns false otherwise.
		/// </returns>
		public bool ValidateFattyAcidType(string textToValidate, int rowNumber)
		{
			if (string.IsNullOrEmpty(textToValidate))
            {
				var messageBoxQuery = string.Format("Fatty acid parsing error in table column 2, row {0}. Fatty acid type cannot be blank.", rowNumber + 1);
				var messageBoxShortPrompt = string.Format("Fatty Acid Parsing Error!");
				var messageBoxButtonOptions = MessageBoxButton.OK;
				var messageBoxImage = MessageBoxImage.Error;

				MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
				return false;
			}
			else
            {
				this._fattyAcidCategory = textToValidate;
				this.type = this._fattyAcidCategory;
				return true;
            }
		}

		/// <summary>
		/// Takes in a edited fatty acid formula. Saves result to internal variable if it's a valid ChemicalFormula. 
		/// </summary>
		/// <returns>
		/// true if the fatty acid formula is valid. Returns false otherwise.
		/// </returns>
		public bool ValidateFattyAcidFormula(string textToValidate, int rowNumber)
		{
            try
            {
                // if chemical formula is just whitespace or empty, return the default empty ChemicalFormula object
                if (string.IsNullOrWhiteSpace(textToValidate))
                {
                    this._formula = ChemicalFormula.Empty;
                    this.formula = this._formula.ToString();
                    return true;
                }
                else if (ChemicalFormula.IsValidChemicalFormula(textToValidate))
				{
					this._formula = new ChemicalFormula(textToValidate);
					this.formula = this._formula.ToString();
					return true;
				}
				else
				{
					var messageBoxQuery = string.Format("Fatty acid parsing error in table column 3, row {0}. Chemical formula \"{1}\" is not valid.", rowNumber + 1, textToValidate);
					var messageBoxShortPrompt = string.Format("Fatty Acid Parsing Error!");
					var messageBoxButtonOptions = MessageBoxButton.OK;
					var messageBoxImage = MessageBoxImage.Error;

					MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
					return false;
				}
            }
            catch (ArgumentException e)
            {
                var messageBoxQuery = string.Format("Fatty acid parsing error in table column 2, row {0}. Chemical formula \"{1}\" is not valid.\n\nSpecific error:\n\n{2}", rowNumber + 1, textToValidate, e.Message);
                var messageBoxShortPrompt = string.Format("Fatty Acid Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                return false;
            }
        }
	}
}
