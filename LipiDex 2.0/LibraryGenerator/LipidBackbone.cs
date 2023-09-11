using CSMSL.Chemistry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LipiDex_2._0.LibraryGenerator
{
    public class LipidBackbone : LipidMoiety
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
        #region Lipid Backbone Properties - Data Grid Display

        public string numberMoieties { get; set; }                      // Number of possible lipid moieties attached

        #endregion

        // these variables are the used store final verisons of the Lipid Backbone Object
        #region  Lipid Backbone Properties - Private

        private int _numberMoieties;                                    // Type of fatty acid
        private List<LipidMoiety> _attachedMoieties;                    // attached lipid moieties for abstracted class building

        #endregion

        public LipidBackbone(string name, string formula, string numMoieties)
        {
            ValidateBackboneName(name, -1);
            ValidateBackboneFormula(formula, -1);
            ValidateNumberOfMoieties(numMoieties, -1);
            this._attachedMoieties = new List<LipidMoiety>(_numberMoieties);
        }

        /// <summary>
        /// Takes in a edited backbone name and makes sure it's not null or empty. Saves result to internal variable if valid. 
        /// </summary>
        /// <returns>
        /// true if the backbone name is valid and parsable. Returns false otherwise.
        /// </returns>
        public bool ValidateBackboneName(string textToValidate, int rowNumber)
        {
            return ValidateMoietyName(textToValidate, rowNumber);
        }

        /// <summary>
        /// Takes in a edited backbone formula. Saves result to internal variable if it's a valid ChemicalFormula. 
        /// </summary>
        /// <returns>
        /// true if the backbone formula is valid. Returns false otherwise.
        /// </returns>
        public bool ValidateBackboneFormula(string textToValidate, int rowNumber)
        {
            return ValidateMoietyFormula(textToValidate, rowNumber);
        }

        /// <summary>
        /// Takes in a edited number of moieties. Saves result to internal variable if it's a valid integer. 
        /// </summary>
        /// <returns>
        /// true if the moiety number is valid. Returns false otherwise.
        /// </returns>
        public bool ValidateNumberOfMoieties(string textToValidate, int rowNumber)
        {
            try
            {
                if (IsInteger(textToValidate))
                {
                    this._numberMoieties = Math.Abs(Convert.ToInt32(textToValidate));
                    this.numberMoieties = this._numberMoieties.ToString();
                    return true;
                }
                else
                {
                    throw new ArgumentException(string.Format("Number of attached moieties to this lipid backbone could not be evaluated. Please make sure number of moieties is an integer."));
                }
            }
            catch (ArgumentException e)
            {
                var messageBoxQuery = string.Format("Backbone parsing error in table column 4, row {0}.\n\nError Message:\n{1}", rowNumber + 1, e.Message);
                var messageBoxShortPrompt = string.Format("Number of Moieties Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                return false;
            }
        }

        public int GetNumberOfMoieties()
        {
            return this._numberMoieties;
        }


        /// <summary>
        /// Takes the intermediate variables from the supplied LipidBackbone object and double checks that all values are valid. If parsing fails, this method returns false.
        /// </summary>
        /// 
        public bool IsValid(int rowNumber)
        {
            if (this.ValidateBackboneName(this.name, rowNumber) && this.ValidateBackboneFormula(this.formula, rowNumber)
                && this.ValidateNumberOfMoieties(this.numberMoieties, rowNumber))
            {
                return true;
            }
            return false;
        }

        /// <summary>
		/// Stringify relevant backbone properties to save to the library csv file.
		/// </summary>
		/// <returns>
		/// Return a stringified representation of this LipidBackbone object
		/// </returns>
		public string SaveString()
        {
            return string.Format("\"{0}\",\"{1}\",\"{2}\"", this._name, this._formula.ToString(), this._numberMoieties);
        }
    }
}
