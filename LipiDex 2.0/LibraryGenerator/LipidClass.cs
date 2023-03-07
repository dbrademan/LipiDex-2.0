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
        public string optimalPolarity { get; set; }                     //Polarities that generate structural information
        public string numberOfMoieties { get; set; }                    //Number of bound moieties to the listed backbone
        public string moiety1 { get; set; }                             //Lipid moiety at the sn1 position
        public string moiety2 { get; set; }                             //Lipid moiety at the sn2 position
        public string moiety3 { get; set; }                             //Lipid moiety at the sn3 position
        public string moiety4 { get; set; }                             //Lipid moiety at the sn4 position

        #endregion

        #region Lipid Class Properties - Private Properties
        private string _fullClassName;									//Abbreviated class name
        private ChemicalFormula _headGroupFormula;						//Elemental formula of head group
        private List<Adduct> _adducts;                                  //Array of adduct objects allowed for each class
        private LipidBackbone _classBackbone;                           //Lipid backbone.	
        private string _optimalPolarity;                                //Polarities that generate structural information
        private int _numberOfMoieties;                                  //Number of moities attached to this lipid class.
        private List<string> _boundMoietyClassifiers;                   //the string identifiers for the bound moieties
        private List<List<LipidMoiety>> _possibleLipidMoieties;			//Array of possible bound moieties
        private ChemicalFormula _neutralFormula;                        //Elemental formula of entire lipid class without adduct or attached moieties

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
		/// Some validation methods in this class behave differently than the other lipid moieties. If the linker variables don't validate, report errors.
        /// </summary>
        public LipidClass(string fullClassName, string classAbbreviation, string headgroupString,
				string delimitedAdducts, string backboneClassifier, string optimalPolarity, string numberOfMoieties, string moiety1, string moiety2,
                string moiety3, string moiety4, LibraryEditor libraryEditorInstance)
		{
            //Instantiate class variables
            ValidateFullLipidClassName(fullClassName, -1);
			ValidateLipidClassAbbreviation(classAbbreviation, -1);
            ValidateLipidHeadgroup(headgroupString, -1);
            ValidateLipidAdductsClassifier(delimitedAdducts, -1, libraryEditorInstance);
            ValidateLipidBackboneClassifier(backboneClassifier, -1, libraryEditorInstance);
			ValidateOptimalPolarity(optimalPolarity, -1);
            ValidateNumberOfMoieties(numberOfMoieties, -1);
            ValidateAllClassMoieties(moiety1, moiety2, moiety3, moiety4, -1);
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
                    this._adducts = new List<Adduct>();
                    
                    var splitAdducts = textToValidate.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var validAdductStrings = new List<string>();

					foreach (var potentialAdductString in splitAdducts)
					{
						CrossLinkAdductString(potentialAdductString, libraryEditorInstance);
					}

                    // made it this far, all adducts have been linked to the lipid class.
                    this.adducts = textToValidate;
					return true;
                }
            }
            catch (ApplicationException e)
            {
                this.adducts = textToValidate;

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
                    this.classBackbone = this._classBackbone.GetName();
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
        /// <br/>[""] - empty string, never will be ID'd at <i>molecular species level</i>
        /// <br/>["+"] - ID'd at <i>molecular species level</i> only in <b>positive mode</b>
        /// <br/>["-"] - ID'd at <i>molecular species level</i> only in <b>negative mode</b>
        /// <br/>["+-"] - Can be ID'd at <i>molecular species level</i> in <b>both polarities</b>
        /// <br/>["-+"] - Can be ID'd at <i>molecular species level</i> in <b>both polarities</b>
        /// <br/>["-/+"] - Can be ID'd at <i>molecular species level</i> in <b>both polarities</b>
        /// <br/>["+/-"] - Can be ID'd at <i>molecular species level</i> in <b>both polarities</b>
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
                    this._optimalPolarity = "+";
                    this.optimalPolarity = this._optimalPolarity;
                    return true;
                }
                else if (textToValidate.Equals("-"))
                {
                    this._optimalPolarity = "-";
                    this.optimalPolarity = this._optimalPolarity;
                    return true;
                }
                else if (textToValidate.Equals("+-") || textToValidate.Equals("-+") || textToValidate.Equals("+/-") || textToValidate.Equals("-/+"))
                {
                    this._optimalPolarity = "+/-";
                    this.optimalPolarity = this._optimalPolarity;
                    return true;
                }
                else
                {
                    // throw error
                    throw new FormatException("Optimal polarity value should be one of the following options:" +
                        "\n[\"\"] - empty string, never will be ID'd at molecular species level" +
                        "\n[\"+\"] - ID'd at molecular species level only in positive mode" +
                        "\n[\"-\"] - ID'd at molecular species level only in negative mode" +
                        "\n[\"+-\"] - Can be ID'd at molecular species level in both polarities" +
                        "\n[\"-+\"] - Can be ID'd at molecular species level in both polarities" +
                        "\n[\"-/+\"] - Can be ID'd at molecular species level in both polarities" +
                        "\n[\"+/-\"] - Can be ID'd at molecular species level in both polarities");
                }

                
            }
            catch (Exception e)
            {
                var messageBoxQuery = string.Format("Optimal polarity parsing error in table column 6, row {0}.\n\n " +
                    "The exact error message is as follows:\n{1}\n\n", rowNumber + 1, e.Message);
                var messageBoxShortPrompt = string.Format("Optimal Polarity Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return false;
            }
        }

        /// <summary>
        /// Called when loading lipid classes from template or saving classes to template. Checks to see if the number of provided moiety classifiers matches the class Number of Moieties
        /// </summary>
        /// <returns>
        /// true if the count of the provided moiety classifiers match the number of class moieties. Returns false otherwise and throws a popup.
        /// </returns>
        public bool ValidateAllClassMoieties(string moiety1, string moiety2, string moiety3, string moiety4, int rowNumber)
        {
            this._boundMoietyClassifiers= new List<string>();

            // store all moieties in a list for easy manipulation
            var moietiesToCheck = new List<string>
            {
                moiety1,
                moiety2,
                moiety3,
                moiety4
            };

            try
            {
                // check to see that number of non-empty moieties is equal to or less than the number of moieties property
                var numProvidedMoieties = 0;

                foreach (var moiety in moietiesToCheck)
                {
                    if (!string.IsNullOrWhiteSpace(moiety))
                    {
                        numProvidedMoieties++;
                    }
                }

                // check to make sure that Num Moieties == number provided moiety classifiers
                if (numProvidedMoieties > this._numberOfMoieties)
                {
                    throw new ApplicationException("\"Error on lipid class import/export!\n\n\"Number of Moieties\" column is smaller than the number of moiety classifiers provided in columns 8-11. These numbers should be equal before saving data.");
                }
                else if (numProvidedMoieties < this._numberOfMoieties)
                {
                    throw new ApplicationException("Error on lipid class import/export!\n\nThe number of saved moiety classifiers is less than the \"Number of Moieties\" column. These numbers should have already been equal.");
                }

                // Made it this far. Classifiers are likely okay.
                // Specific classifier validation will be done when defining fragmentation rules.
                this._boundMoietyClassifiers = moietiesToCheck;
                this.moiety1 = moiety1;
                this.moiety2 = moiety2;
                this.moiety3 = moiety3;
                this.moiety4 = moiety4;
                return true;
            }
            catch (ApplicationException e) 
            { 
                var messageBoxQuery = string.Format("Error parsing lipid class moiety categorical values in table columns 8-11, row {0}.\n\n " +
                    "The exact error message is as follows:\n{1}\n\n", rowNumber + 1, e.Message);
                var messageBoxShortPrompt = string.Format("Lipid Class Moiety 1-4 Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage); 
                return false;
            }
        }

        /// <summary>
        /// Called when editing a single lipid class moiety. Checks to see if the number of provided moiety classifiers matches the class Number of Moieties
        /// </summary>
        /// <returns>
        /// true if the count of the provided moiety classifiers match the number of class moieties. Returns false otherwise and throws a popup.
        /// </returns>
        public bool ValidateSingleChangedMoiety(string moietyToValidate, int columnNumber, int rowNumber)
        {
            var changedMoietyIndex = columnNumber - 6;

            // reset the boundMoietyClassifiers to empty list.
            this._boundMoietyClassifiers = new List<string>();

            // store all moieties in a list for easy manipulation
            List<string> moietiesToCheck = null;

            try
            {
                switch (changedMoietyIndex)
                {
                    case 1:
                        moietiesToCheck = new List<string> { moietyToValidate, this.moiety2, this.moiety3, this.moiety4 };
                        break;
                    case 2:
                        moietiesToCheck = new List<string> { this.moiety1, moietyToValidate, this.moiety3, this.moiety4 };
                        break;
                    case 3:
                        moietiesToCheck = new List<string> { this.moiety1, this.moiety2, moietyToValidate, this.moiety4 };
                        break;
                    case 4:
                        moietiesToCheck = new List<string> { this.moiety1, this.moiety2, this.moiety3, moietyToValidate };
                        break;
                }

                // check to see that number of non-empty moieties is equal to or less than the number of moieties property
                var numProvidedMoieties = 0;

                foreach (var moiety in moietiesToCheck)
                {
                    if (!string.IsNullOrWhiteSpace(moiety))
                    {
                        numProvidedMoieties++;
                    }
                }

                // check to make sure that Num Moieties == number provided moiety classifiers
                if (numProvidedMoieties > this._numberOfMoieties)
                {
                    throw new ApplicationException("Moiety added in column {0}, row {1} has exceeded the value in \"Num Moieties\" column. This discrepency must be fixed before saving is possible.");
                }

                // Made it this far. Classifiers are likely okay.
                // Specific classifier validation will be done when defining fragmentation rules.
                this._boundMoietyClassifiers = moietiesToCheck;                
                this.moiety1 = moietiesToCheck[0];
                this.moiety2 = moietiesToCheck[1];
                this.moiety3 = moietiesToCheck[2];
                this.moiety4 = moietiesToCheck[3];
                return true;
            }
            catch (ApplicationException e)
            {
                var messageBoxQuery = string.Format("Error parsing lipid class moiety categorical values in table column {2}, row {0}.\n\n " +
                    "The exact error message is as follows:\n{1}\n\n", rowNumber + 1, e.Message, columnNumber);
                var messageBoxShortPrompt = string.Format("Error Adding New Lipid Moiety!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                return false;
            }
            
        }

        /// <summary>
        /// Takes in a string representation of a single lipid adduct. Cross-links adducts from the ObservableCollection&lt;Adduct&gt;
        /// </summary>
        /// <exception cref="ApplicationException">Thrown when a provided adduct string fails to cross-map to a lipid adduct.</exception>
        /// 
        private void CrossLinkAdductString(string potentialAdductString, LibraryEditor libraryEditorInstance)
        {
            List<Adduct> matchingAdducts = libraryEditorInstance.DataGridBinding_Adducts.Where(adduct => adduct.GetName().Equals(potentialAdductString)).ToList();

            if (matchingAdducts.Count == 0)
            {
                throw new ApplicationException(string.Format("No adducts found which match to the provided adduct \"{0}\"." +
                    "\n\nPlease define and save this adduct in the adduct tab before adding it to this class", potentialAdductString));
            }
            else if (matchingAdducts.Count == 1)
            {
                this._adducts.Add(matchingAdducts[0]);
            }
            else
            {
                throw new ApplicationException(string.Format("Duplicate adducts found matching to the name \"{0}\"." +
                    "\n\nPlease resolve this discrepency in the adduct tab by removing or renaming duplicates.", potentialAdductString));
            }
        }

        /// <summary>
        /// Takes in a string representation of a single lipid backbone. Cross-links the backbone from the ObservableCollection&lt;LipidBackbone&gt;
        /// </summary>
        /// <exception cref="ApplicationException">Thrown when a provided backbone string fails to cross-map to a lipid backbone.</exception>
        /// 
        private void CrossLinkBackboneString(string potentialBackboneString, LibraryEditor libraryEditorInstance)
        {
            List<LipidBackbone> matchingBackbones = libraryEditorInstance.DataGridBinding_Backbones.Where(backbone => backbone.GetName().Equals(potentialBackboneString)).ToList();

            if (matchingBackbones.Count == 0)
            {
                throw new ApplicationException(string.Format("No backbones found which match to the provided backbone \"{0}\"." +
                    "\n\nPlease define and save this backbone in the backbone tab before adding it to this class", potentialBackboneString));
            }
            else if (matchingBackbones.Count == 1)
            {
                this._classBackbone = matchingBackbones[0];
            }
            else
            {
                throw new ApplicationException(string.Format("Duplicate backbones found matching to the name \"{0}\"." +
                    "\n\nPlease resolve this discrepency in the backbone tab by removing or renaming duplicates.", potentialBackboneString));
            }
        }

        /// <summary>
        /// Takes in a edited string representing the number of moieties. Saves result to internal variable if it's a valid integer and it's less than the maximum allowed moieties for the backbone. 
        /// </summary>
        /// <returns>
        /// true if the number of moieties is valid. Returns false otherwise.
        /// </returns>
        public bool ValidateNumberOfMoieties(string textToValidate, int rowNumber)
        {
            try
            {
                if (IsInteger(textToValidate))
                {
                    var potentialNumberOfMoieties = Math.Abs(Convert.ToInt32(textToValidate));

                    // make sure number of moieties is not greater than the maximum permissable number of moieties on the backbone.
                    // This check will likely help down the line if we decide to do more advanced structural determination
                    if (potentialNumberOfMoieties > this._classBackbone.GetNumberOfMoieties())
                    {
                        this._numberOfMoieties = this._classBackbone.GetNumberOfMoieties();
                        this.numberOfMoieties = this._numberOfMoieties.ToString();
                        return true;

                        var messageBoxQuery = string.Format("Number of moieties for the lipid class {0} is greater than specified lipid backbone {1} allows.\n\n" +
                            "\"Number of Moieties\" column has been set to the maximum number of moieties allowed by the backbone\n\nMore information:\n\nBackbone \"{1}\" allows {2} moieties" +
                            "\nNumber of moieties provided: {2}", potentialNumberOfMoieties, this._classBackbone.GetName(), this._classBackbone.GetNumberOfMoieties());
                        var messageBoxShortPrompt = string.Format("Warning - Too many moieties!");
                        var messageBoxButtonOptions = MessageBoxButton.OK;
                        var messageBoxImage = MessageBoxImage.Warning;

                        MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                    }
                    else
                    {
                        this._numberOfMoieties = Math.Abs(Convert.ToInt32(textToValidate));
                        this.numberOfMoieties = this._numberOfMoieties.ToString();
                        return true;
                    }
                    
                }
                else
                {
                    throw new ArgumentException(string.Format("Number of moieties attached to this lipid class could not be evaluated. Please make sure the number of moieties is an integer."));
                }
            }
            catch (ArgumentException e)
            {
                var messageBoxQuery = string.Format("Number of moieties parsing error in table column 7, row {0}.\n\nError Message:\n{1}", rowNumber + 1, e.Message);
                var messageBoxShortPrompt = string.Format("Number of Moieties Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                return false;
            }
        }

        /// <summary>
        /// TODO: Binds relevant moities (fatty acids or other lipid classes) to the lipid class. If no matching moiety is found, open a popup and let the user know. Mark the lipid class as unbound
        /// </summary>
        /// <exception cref="ApplicationException">Thrown when a provided backbone string fails to cross-map to a lipid backbone.</exception>
        /// 
        public void BindMoieties(ObservableCollection<FattyAcid> DataGridBinding_FattyAcids, ObservableCollection<LipidClass> DataGridBinding_LipidClasses)
        {

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

        public bool IsValid(LibraryEditor libraryEditorInstance)
        {

            // do a bunch of validation methods on the properties stored in the data grid.
            if (ValidateFullLipidClassName(this.fullClassName, -1) && ValidateLipidClassAbbreviation(this.name, -1) && ValidateLipidHeadgroup(this.headGroupFormula, -1)
                && ValidateLipidAdductsClassifier(this.adducts, -1, libraryEditorInstance) && ValidateLipidBackboneClassifier(this.classBackbone, -1, libraryEditorInstance)
                && ValidateOptimalPolarity(this.optimalPolarity, -1) && ValidateNumberOfMoieties(this.numberOfMoieties, -1) && ValidateAllClassMoieties(this.moiety1,
                this.moiety2, this.moiety3, this.moiety4, -1)) 
            {
                return true;
            }
            return false;
        }

		public new ChemicalFormula GetChemicalFormula()
		{
			var returnFormula = new ChemicalFormula();

			// add backbone ChemicalFormula
			returnFormula.Add(this._classBackbone.GetChemicalFormula());

			// add headgroup formula
			returnFormula.Add(this._headGroupFormula);

            // add adduct formula
            return returnFormula;

		}

        /// <summary>
        /// Creates all possible combinations of moieties for this lipid when creating templates. 
        /// </summary>
        /// <returns>
        /// The shorthand representation of this lipid class as a string.
        /// </returns>
		public List<List<LipidMoiety>> GetPossibleMoieties()
		{
			return this._possibleLipidMoieties;
		}

        public string SaveString()
        {
            var adductStrings = new List<string>();

            foreach (var adduct in this._adducts)
            {
                adductStrings.Add(adduct.GetName());
            }
            var savableAdductString = string.Join(";", adductStrings);
            var backboneString = this._classBackbone.GetName();

            var returnString = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", 
                this._name, 
                this._fullClassName, 
                this.headGroupFormula.ToString(),
                savableAdductString,
                backboneString,
                this._optimalPolarity, 
                this._numberOfMoieties, 
                this._boundMoietyClassifiers[0],
                this._boundMoietyClassifiers[1], 
                this._boundMoietyClassifiers[2], 
                this._boundMoietyClassifiers[3]
                );
            return returnString;
        }

        /*
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
