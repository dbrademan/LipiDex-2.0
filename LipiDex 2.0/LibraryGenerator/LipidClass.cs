using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using CSMSL.Chemistry;

namespace LipiDex_2._0.LibraryGenerator
{
    public class LipidClass : LipidMoiety
    {
        // Inherited properties from LipidMoiety
        #region Inherited LipidMoiety Properties & Methods

        #region Properties

        // public string name { get; set; }                                  // Abbreviated moiety name for data grid
        // public string formula { get; set; }                               // Moiety elemental formula for data grid
        // protected string _name;                                           // Abbreviated name
        // protected ChemicalFormula _formula;                               // Elemental formula

        #endregion

        #region Methods

        // protected string GetName()

        // protected string GetFormulaString()

        // protected ChemicalFormula GetChemicalFormula()

        // protected bool ValidateMoietyName(string textToValidate, int rowNumber)

        // protected bool ValidateMoietyFormula(string textToValidate, int rowNumber)

        // protected bool IsInteger(string textToValidate)

        #endregion

        #endregion

        // This set of properties are used as intermediate placeholders during editing of the data grid.
        #region Lipid Class Properties - Data Grid Display

        public string fullClassName { get; set; }                       //Abbreviated class name
        public string headGroupFormula { get; set; }   					//Elemental formula of head group
        public string adducts { get; set; }                             //Array of adduct objects allowed for each class
        public string classBackbone { get; set; }                       //Lipid backbone.
		public string numberOfMoieties { get; set; }					//Number of bound moieties to the listed backbone
        public string optimalPolarity { get; set; }                     //Polarities that generate structural information

        #endregion

        #region Lipid Class Properties - Private Properties
        private string _fullClassName;									//Abbreviated class name
        private ChemicalFormula _headGroupFormula;						//Elemental formula of head group
        private List<Adduct> _adducts;                                  //Array of adduct objects allowed for each class
        private LipidBackbone _classBackbone;                           //Lipid backbone.	
        private string _optimalPolarity;                                //Polarities that generate structural information
        private List<List<LipidMoiety>> _possibleLipidMoieties;			//Array of possible bound moieties
        private ChemicalFormula _neutralFormula;                        //Elemental formula of entire lipid class without adduct

        #endregion

        #region TODO - Lipid Class Properties - Fragmentation Template-Specific Properties
        /*
		public ChemicalFormula headGroupFormula;                        //Elemental formula of head group
		public List<Adduct> adducts;									//Array of adduct objects allowed for each class
		public bool sterol;												//true iff backbone of lipid is sterol
		public bool glycerol;											//true iff backbone of lipid is glycerol
		public bool sphingoid;											//true iff sphingoid base
		public ChemicalFormula backboneFormula;							//Elemental formula of backbone							
		public int numberOfFattyAcids;									//number of allowed fatty acids
		public string optimalPolarity;									//Fragment informative polarity
		public List<List<FattyAcid>> possibleFattyAcids;				//Array of possible fatty acids
		public ChemicalFormula formula;                                 //Elemental formula of entire lipid class without adduct
		*/
        #endregion

        /// <summary>
        /// Constructor - Takes in string variables scraped from the LipidClass.csv template file and forms and intermediate LipidClass object for
		/// organizing lipid classes in the GUI grid.
		/// <br/>
		/// <br/>
		/// Some validation methods in this class behave differently than the other lipid moieties. If the linker variables don't validate, report errors in a log box.
		/// Does not immediately populate the following fields:
		/// - _neutralFormula
		/// ...
        /// </summary>
        public LipidClass(string fullClassName, string classAbbreviation, string headgroupString,
				string delimitedAdducts, string backboneClassifier, string optimalPolarity, LibraryEditor libraryEditorInstance)
		{
            //Instantiate class variables
            ValidateFullLipidClassName(fullClassName, -1);
			ValidateLipidClassAbbreviation(classAbbreviation, -1);
            ValidateLipidHeadgroup(headgroupString, -1);
            ValidateLipidAdductsClassifier(delimitedAdducts, -1, libraryEditorInstance);
            ValidateLipidBackboneClassifier(backboneClassifier, -1, libraryEditorInstance);
			ValidateOptimalPolarity(optimalPolarity, -1);
            /*
			
			ValidateOptimal
			this.numberOfFattyAcids = numberOfFattyAcids;
			this.optimalPolarity = optimalPolarity;
			this.fattyAcidTypes = fattyAcidTypes;
			this.possibleFattyAcids = new List<List<FattyAcid>>();

			//Calculate elemental formula
			CalculateFormula();
			*/
        }

        /// <summary>
        /// Takes in a edited full lipid class name and makes sure it's not null or empty. Saves result to internal variable if valid. 
        /// </summary>
        /// <returns>
        /// true if the full lipid class name is valid and parsable. Returns false otherwise.
        /// </returns>
        public bool ValidateFullLipidClassName(string textToValidate, int rowNumber)
		{
            try
            {
                if (string.IsNullOrWhiteSpace(textToValidate))
                {
                    throw new FormatException("Full lipid class name cannot be whitespace or empty. Please provide a full lipid class name.");
                }

                this._fullClassName = textToValidate;
                this.fullClassName = this._fullClassName;
                return true;
            }
            catch (Exception e)
            {
                var messageBoxQuery = string.Format("Full lipid class name parsing error in table column 1, row {0}.\n\n " +
                    "The exact error message is as follows:\n{1}\n\n", rowNumber + 1, e.Message);
                var messageBoxShortPrompt = string.Format("Full Lipid Class Name Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return false;
            }
        }

        /// <summary>
        /// Takes in a edited lipid class abbreviation and makes sure it's not null or empty. Saves result to internal variable if valid. 
        /// </summary>
        /// <returns>
        /// true if the lipid class abbreviation is valid and parsable. Returns false otherwise.
        /// </returns>
        public bool ValidateLipidClassAbbreviation(string textToValidate, int rowNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textToValidate))
                {
                    throw new FormatException("Lipid class abbreviation cannot be whitespace or empty. Please provide a lipid class abbreviation.");
                }

                this._name = textToValidate;
                this.name = this._name;
                return true;
            }
            catch (Exception e)
            {
                var messageBoxQuery = string.Format("Lipid class abbreviation parsing error in table column 2, row {0}.\n\n " +
                    "The exact error message is as follows:\n{1}\n\n", rowNumber + 1, e.Message);
                var messageBoxShortPrompt = string.Format("Lipid Class Abbreviation Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return false;
            }
        }

        /// <summary>
        /// Takes in an edited string representation of the lipid headgroup's chemical formula. Saves result to '_headgroup' and 'headgroup' internal variable if it's a valid ChemicalFormula. 
        /// </summary>
        /// <returns>
        /// true if the headgroup formula is valid. Returns false otherwise.
        /// </returns>
        public bool ValidateLipidHeadgroup(string textToValidate, int rowNumber)
        {
            try
            {
                // if chemical formula is just whitespace or empty, return the default empty ChemicalFormula object
                if (string.IsNullOrWhiteSpace(textToValidate))
                {
                    this._headGroupFormula = ChemicalFormula.Empty;
                    this.headGroupFormula = this._headGroupFormula.ToString();
                    return true;
                }
                else if (ChemicalFormula.IsValidChemicalFormula(textToValidate))
                {
                    this._headGroupFormula = new ChemicalFormula(textToValidate);
                    this.headGroupFormula = this._headGroupFormula.ToString();
                    return true;
                }
                else
                {
                    var messageBoxQuery = string.Format("Lipid headgroup parsing error in table column 3, row {0}. Chemical formula \"{1}\" is not valid.", rowNumber + 1, textToValidate);
                    var messageBoxShortPrompt = string.Format("Lipid Headgroup Parsing Error!");
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                    return false;
                }
            }
            catch (ArgumentException e)
            {
                var messageBoxQuery = string.Format("Lipid headgroup parsing error in table column 3, row {0}. Chemical formula \"{1}\" is not valid.\n\nSpecific error:\n{2}", rowNumber + 1, textToValidate, e.Message);
                var messageBoxShortPrompt = string.Format("Lipid Headgroup Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                return false;
            }
        }

        /// <summary>
        /// Takes in a semicolon-delimited set of adducts. Cross-reference existing adducts in the adduct table and throw errors if these adducts don't exist yet. 
        /// </summary>
        /// <returns>
        /// true if the all provided adducts are valid. Returns false otherwise and throws a popup.
        /// </returns>
        public bool ValidateLipidAdductsClassifier(string textToValidate, int rowNumber, LibraryEditor libraryEditorInstance)
        {
            try
            {
                // if chemical formula is just whitespace or empty, return the default empty ChemicalFormula object
                if (string.IsNullOrWhiteSpace(textToValidate))
                {
                    var messageBoxQuery = string.Format("Lipid adduct parsing error in table column 4, row {0}. Adduct string cannot be empty and must relate to previously defined adducts.", rowNumber + 1, textToValidate);
                    var messageBoxShortPrompt = string.Format("Lipid Adduct(s) Parsing Error!");
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                    return false;
                }
                else
                {
					// there's some sort of text in the adduct table.
					// try to parse it
					var splitAdducts = textToValidate.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

					foreach (var potentialAdductString in splitAdducts)
					{
						CrossLinkAdductString(potentialAdductString, libraryEditorInstance);
					}

					// made it this far, all adducts have been linked to the lipid class.
					return true;
                }

                /*
				var messageBoxQuery = string.Format("Lipid adduct parsing error in table column 3, row {0}. Adduct string \"{1}\" has not been parsed correctly.", rowNumber + 1, textToValidate);
                var messageBoxShortPrompt = string.Format("Lipid Adduct(s) Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;
				*/

            }
            catch (ArgumentException e)
            {
                var messageBoxQuery = string.Format("Lipid adduct parsing error in table column 4, row {0}. Adduct string \"{1}\" has not been parsed correctly.\n\nSpecific error:\n{2}", rowNumber + 1, textToValidate, e.Message);
                var messageBoxShortPrompt = string.Format("Lipid Adduct Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                return false;
            }
        }

        /// <summary>
        /// Takes in an edited backbone classifier. Cross-reference existing lipid backbones in the backbone table and throw errors if this backbone doesn't exist yet. 
        /// </summary>
        /// <returns>
        /// true if the provided backbone is valid. Returns false otherwise and throws a popup.
        /// </returns>
        public bool ValidateLipidBackboneClassifier(string textToValidate, int rowNumber, LibraryEditor libraryEditorInstance)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textToValidate))
                {
                    var messageBoxQuery = string.Format("Lipid backbone parsing error in table column 5, row {0}. Backbone classifier string cannot be empty and must relate to previously defined backbone.", rowNumber + 1, textToValidate);
                    var messageBoxShortPrompt = string.Format("Lipid Backbone Parsing Error!");
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                    return false;
                }
                else
                {
                    // there's some sort of text in the backbone table.
					// cross-link it
                    CrossLinkBackboneString(textToValidate, libraryEditorInstance);

                    // made it this far, the backbone has successfully been linked to the lipid class.
                    return true;
                }
            }
            catch (ArgumentException e)
            {
                var messageBoxQuery = string.Format("Lipid backbone parsing error in table column 5, row {0}. Backbone string \"{1}\" has not been parsed correctly.\n\nSpecific error:\n{2}", rowNumber + 1, textToValidate, e.Message);
                var messageBoxShortPrompt = string.Format("Lipid Backbone Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                return false;
            }
        }

        /// <summary>
        /// Takes in an edited optimal polarity classifier. The following options are valid (ignore brackets):
        /// <br/>
        /// <br/>&#x9;[""] - empty string, never will be ID'd at <i>molecular species level</i>
        /// <br/>&#x9;["+"] - ID'd at <i>molecular species level</i> only in <b>positive mode</b>
        /// <br/>&#x9;["-"] - ID'd at <i>molecular species level</i> only in <b>negative mode</b>
        /// <br/>&#x9;["+-"] - Can be ID'd at <i>molecular species level</i> in <b>both polarities</b>
        /// <br/>&#x9;["-+"] - Can be ID'd at <i>molecular species level</i> in <b>both polarities</b>
        /// <br/>&#x9;["-/+"] - Can be ID'd at <i>molecular species level</i> in <b>both polarities</b>
        /// <br/>&#x9;["+-"] - Can be ID'd at <i>molecular species level</i> in <b>both polarities</b>
        /// </summary>
        /// <returns>
        /// true if the provided optimal polarity is valid. Returns false otherwise and throws a popup.
        /// </returns>
		public bool ValidateOptimalPolarity(string textToValidate, int rowNumber)
		{
            try
            {
                if (string.IsNullOrWhiteSpace(textToValidate))
                {
                    this._optimalPolarity = "";
                    this.optimalPolarity = this._optimalPolarity;
                    return true;
                }
                else if (textToValidate.Equals("+"))
                {
                    this._optimalPolarity = "";
                    this.optimalPolarity = this._optimalPolarity;
                    return true;
                }
                else if (textToValidate.Equals("-"))
                {
                    this._optimalPolarity = "";
                    this.optimalPolarity = this._optimalPolarity;
                    return true;
                }
                else if (textToValidate.Equals("+-") || textToValidate.Equals("-+") || textToValidate.Equals("+/-") || textToValidate.Equals("-/+"))
                {
                    this._optimalPolarity = "";
                    this.optimalPolarity = this._optimalPolarity;
                    return true;
                }
                else
                {
                    // throw error
                    return false;
                }

                
            }
            catch (Exception e)
            {
                var messageBoxQuery = string.Format("Optimal polarity parsing error in table column x, row {0}.\n\n " +
                    "The exact error message is as follows:\n{1}\n\n", rowNumber + 1, e.Message);
                var messageBoxShortPrompt = string.Format("Full Lipid Class Name Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return false;
            }
        }

        /// <summary>
        /// Takes in a string representation of a single lipid adduct. Cross-links adducts from the ObservableCollection&lt;Adduct&gt;
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when a provided adduct string fails to cross-map to a lipid adduct.</exception>
        /// 
        private void CrossLinkAdductString(string potentialAdductString, LibraryEditor libraryEditorInstance)
        {
            List<Adduct> matchingAdducts = libraryEditorInstance.DataGridBinding_Adducts.Where(adduct => adduct.GetName().Equals(potentialAdductString)).ToList();

            if (matchingAdducts.Count == 0)
            {
                throw new ArgumentException(string.Format("No adducts found which match to the provided adduct \"{0}\"." +
                    "\n\nPlease define and save this adduct in the adduct tab before adding it to this class", potentialAdductString));
            }
            else if (matchingAdducts.Count == 1)
            {
                this._adducts.Add(matchingAdducts[0]);
            }
            else
            {
                throw new ArgumentException(string.Format("Duplicate adducts found matching to the name \"{0}\"." +
                    "\n\nPlease resolve this discrepency in the adduct tab by removing or renaming duplicates.", potentialAdductString));
            }
        }

        /// <summary>
        /// Takes in a string representation of a single lipid backbone. Cross-links the backbone from the ObservableCollection&lt;LipidBackbone&gt;
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when a provided backbone string fails to cross-map to a lipid backbone.</exception>
        /// 
        private void CrossLinkBackboneString(string potentialBackboneString, LibraryEditor libraryEditorInstance)
        {
            List<LipidBackbone> matchingBackbones = libraryEditorInstance.DataGridBinding_Backbones.Where(backbone => backbone.GetName().Equals(potentialBackboneString)).ToList();

            if (matchingBackbones.Count == 0)
            {
                throw new ArgumentException(string.Format("No backbones found which match to the provided backbone \"{0}\"." +
                    "\n\nPlease define and save this backbone in the backbone tab before adding it to this class", potentialBackboneString));
            }
            else if (matchingBackbones.Count == 1)
            {
                this._classBackbone = matchingBackbones[0];
            }
            else
            {
                throw new ArgumentException(string.Format("Duplicate backbones found matching to the name \"{0}\"." +
                    "\n\nPlease resolve this discrepency in the backbone tab by removing or renaming duplicates.", potentialBackboneString));
            }
        }

        /// <summary>
        /// Retrives the abbreviation for this lipid class. 
        /// </summary>
        /// <returns>
        /// The shorthand representation of this lipid class as a string.
        /// </returns>
        public string GetAbbreviation()
        {
            return this.GetName();
        }

		public new ChemicalFormula GetChemicalFormula()
		{
			var returnFormula = new ChemicalFormula();

			// add backbone ChemicalFormula
			returnFormula.Add(this._classBackbone.GetChemicalFormula());

			// add headgroup formula
			returnFormula.Add(this._headGroupFormula);

            // don't add adduct formulas here.
			// This needs to be done on the `lipid` side
            /*
			foreach (var adduct in this._adducts)
			{

			}
			*/

            // don't add the moity formula's here. Apparently that needs to be done on the `lipid` side
            /*
			foreach (var moiety in this._possibleLipidMoieties)
			{

			}
			*/

            // add adduct formula
            return returnFormula;

		}

		public List<List<LipidMoiety>> GetPossibleMoieties()
		{
			return this._possibleLipidMoieties;
		}

        //Returns string array representation of class for table display
        /*
        public List<string> GetTableArray()
		{
			string adductString = "";

			List<string> result = new List<string>();

			//Name
			result[0] = this.className;

			//Abbreviation
			result[1] = this.classAbbreviation;

			//Head Group
			result[2] = this.headGroup;

			//Adducts
			for (int i = 0; i < this.adducts.Count; i++)
			{
				adductString += this.adducts[i].name;
				if (i < this.adducts.Count - 1)
				{
					adductString += ";";
				}
			}
			result[3] = adductString;

			//Backbone
			//this should eventually be updated to allow users to define their own backbone structure
			if (this.glycerol)
			{
				result[4] = "Glycerol";
			}
			else if (this.sterol)
			{
				result[4] = "Sterol";
			}
			else if (this.sphingoid)
			{
				result[4] = "Sphingoid";
			}
			else
			{
				result[4] = this.backboneFormula;
			}

			//Num Fatty Acids
			result[5] = this.numberOfFattyAcids.ToString();

			//Optimal Polarity
			result[6] = this.optimalPolarity;

			//Add in fatty acid types
			result[7] = this.fattyAcidTypes[0];

			if (this.numberOfFattyAcids >= 2)
			{
				result[8] = this.fattyAcidTypes[1];
			}
			else 
			{ 
				result[8] = "-"; 
			}
			if (this.numberOfFattyAcids >= 3)
			{
				result[9] = this.fattyAcidTypes[2];
			}
			else
			{
				result[9] = "-";
			}
			if (this.numberOfFattyAcids >= 4)
			{
				result[10] = this.fattyAcidTypes[3];
			}
			else 
			{ 
				result[10] = "-"; 
			}

			return result;
		}

		

		//Return array of all fatty acid types
		public List<string> GetFattyAcidTypes()
		{
			return this.fattyAcidTypes;
		}

		//Returns the number of fatty acids allowed for class
		public int GetNumberOfFattyAcids()
		{
			return this.numberOfFattyAcids;
		}

		//Returns headgroup elemental formula
		public string GetHeadGroup()
		{
			return this.headGroup;
		}

		//Returns true iff a particular fatty acid is in possible fatty acids
		public bool IsValidFattyAcid(string fattyAcid)
		{
			for (int i = 0; i < this.possibleFattyAcids.Count; i++)
			{
				var possibleFattyAcidSubset = this.possibleFattyAcids[i];

				for (int j = 0; j < possibleFattyAcidSubset.Count; j++)
				{
					var possibleFattyAcid = possibleFattyAcidSubset[j];

					if (possibleFattyAcid.ToString().Equals(fattyAcid))
                    {
						return true;
					}	
				}
			}

			return false;
		}

		//Returns true iff a particular fatty acid is in possible fa
		public FattyAcid GetMatchingFattyAcid(string fattyAcid)
		{
			for (int i = 0; i < this.possibleFattyAcids.Count; i++)
			{
				var possibleFattyAcidSubset = this.possibleFattyAcids[i];

				for (int j = 0; j < possibleFattyAcidSubset.Count; j++)
				{
					var possibleFattyAcid = possibleFattyAcidSubset[j];
					
					if (possibleFattyAcid.ToString().Equals(fattyAcid))
                    {
						return possibleFattyAcid;
					}
				}
			}

			return null;
		}

		//Returns the number of fatty acids of each type allowed for class
		public int GetCountOfFattyAcidType(string type)
		{
			int count = 0;

			for (int i = 0; i < this.fattyAcidTypes.Count; i++)
			{
				if (this.fattyAcidTypes[i].Equals(type))
                {
					count++;
				}
			}
			return count;
		}

		//Populate 2d FA array with all possible fatty acids
		public void PopulateFattyAcids(List<FattyAcid> allFattyAcids)
		{
			this.possibleFattyAcids.Clear();
			List<FattyAcid> fattyAcidArray_Temporary;
			List<FattyAcid> fattyAcidArray;

			//If lipid class is a CL, remove fatty acids types
			//This should be refactored for flexability. Seems hacky...
			if (this.classAbbreviation.Contains("CL") || this.className.Contains("ardiol"))
			{
				fattyAcidArray = new List<FattyAcid>();

				string allowedFattyAcids = "_14:0_14:1_15:1_15:0_16:0_16:1_17:1_18:0_18:1_18:2_18:3_20:0_20:1_20:2_20:3_20:5_20:4_22:0_22:6_";

				for (int i = 0; i < allFattyAcids.Count; i++)
				{
					if (allowedFattyAcids.Contains("_" + allFattyAcids[i].name + "_"))
                    {

                    }
						fattyAcidArray.Add(allFattyAcids[i]);
				}
			}
			else
            {
				fattyAcidArray = allFattyAcids;
			}
				
			//Iterate through possible fatty acid slots
			for (int i = 0; i < this.fattyAcidTypes.Count; i++)
			{
				//Reset temp holder
				fattyAcidArray_Temporary = new List<FattyAcid>();

				//Iterate through
				for (int j = 0; j < fattyAcidArray.Count; j++)
				{
					//Check if matching fatty acid is found.  If so, add to temp array
					if (fattyAcidArray[j].type.Equals(fattyAcidTypes[i]) && fattyAcidArray[j].enabled)
					{
						fattyAcidArray.Add(fattyAcidArray[j]);
					}
						
				}
				//Add populated temp array to faTemp
				possibleFattyAcids.Add(fattyAcidArray_Temporary);
			}
		}

		//Calculate elemental composition of lipid class
		public void CalculateFormula()
		{
			formula = this.headGroup;

			// again, need to update the backbone logic to make it more flexible
			if (glycerol)
            {
				var backboneFormula = new ChemicalFormula("C3H5");
				formula = Utilities.MergeFormulas(formula, backboneFormula);
			}
			else if (sphingoid)
            {
				var backboneFormula = new ChemicalFormula("C3H2");
				formula = Utilities.MergeFormulas(formula, backboneFormula);
			}
			else if (sterol)
            {
				var backboneFormula = new ChemicalFormula("C27H45");
				formula = Utilities.MergeFormulas(formula, backboneFormula);
			}
			else
            {
				formula = Utilities.MergeFormulas(formula, this.backboneFormula);
			}
				
		}

		//Returns string representation of class
		public override string ToString()
		{
			string result = "";

			result += this.className + ",";
			result += this.classAbbreviation + ",";
			result += this.headGroup + ",";

			for (int i = 0; i < adducts.Count; i++)
			{
				result += adducts[i].name;

				if (i < adducts.Count - 1) result += ";";
				else result += ",";
			}

			if (sterol)
			{
				result += "TRUE,";
			}
			else 
			{ 
				result += "FALSE,"; 
			}

			if (glycerol) 
			{ 
				result += "TRUE,";
			}
			else 
			{ 
				result += "FALSE,";
			}

			if (sphingoid) 
			{ 
				result += "TRUE,";
			}
			else 
			{ 
				result += "FALSE,"; 
			}

			result += this.backboneFormula + ",";

			result += this.numberOfFattyAcids + ",";
			result += this.optimalPolarity + ",";

			for (int i = 0; i < fattyAcidTypes.Count; i++)
			{
				result += fattyAcidTypes[i];
				if (i < 3) result += ",";
			}

			for (int i = fattyAcidTypes.Count; i < 4; i++)
			{
				result += "none";
				if (i < 3) result += ",";
			}

			return result;
		}
		*/
    }
}
