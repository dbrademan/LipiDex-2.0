using CSMSL.Chemistry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LipiDex_2._0.LibraryGenerator
{
    /// <summary>
    /// LipidMoiety is an abstraction of the "different chunks" of a lipid.
    /// <br/>
    /// <br/>
    /// Each chunk of a lipid will have these shared properties:
    /// <br/>
    /// - A name, label, or classifier
    /// <br/>
    /// - A chemical formula
    /// </summary>
    public abstract class LipidMoiety
    {

        public static LipidMoietyComparer LipidMoietyComparer = new LipidMoietyComparer();

        // This set of properties are used as intermediate placeholders during editing of the data grid.
        #region Lipid Moiety Properties - Data Grid Display

        public string name { get; set; }                                // Abbreviated moiety name
        public string formula { get; set; }                             // Moiety elemental formula

        #endregion

        // these variables are the used store final verisons of the Lipid Backbone Object
        #region  Lipid Moiety Properties - Private
        
        protected string _name;                                           // Abbreviated name
        protected ChemicalFormula _formula;                               // Elemental formula

        #endregion

        /// <summary>
		/// Retrieves a string representation of the moiety's elemental formula
		/// </summary>
		/// <returns>
		/// Return the moiety's chemical formula as a string ('+'/'-')
		/// </returns>
		public string GetFormulaString()
        {
            return this._formula.ToString();
        }

        /// <summary>
        /// Retrieves a CSMSL.Chemistry.ChemicalFormula representation of the moiety's elemental formula
        /// </summary>
        /// <returns>
        /// Return the moiety's chemical formula as a ChemicalFormula.
        /// </returns>
        public ChemicalFormula GetChemicalFormula()
        {
            return this._formula;
        }

        /// <summary>
        /// Retrieves the moiety's private name property
        /// </summary>
        /// <returns>
        /// Return the moiety's name as a string ('+'/'-')
        /// </returns>
        public string GetName()
        {
            return this._name;
        }

        /// <summary>
        /// Takes in a edited moiety name and makes sure it's not null or empty. Saves result to internal variable if valid. 
        /// </summary>
        /// <returns>
        /// true if the moiety name is valid and parsable. Returns false otherwise.
        /// </returns>
        protected bool ValidateMoietyName(string textToValidate, int rowNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textToValidate))
                {
                    throw new FormatException("Moiety name cannot be whitespace or empty. Please provide an moiety name.");
                }

                this._name = textToValidate;
                this.name = this._name;
                return true;
            }
            catch (Exception e)
            {
                var messageBoxQuery = string.Format("Moiety parsing error in table column 1, row {0}.\n\n " +
                    "The exact error message is as follows:\n{1}\n\n", rowNumber + 1, e.Message);
                var messageBoxShortPrompt = string.Format("Moiety Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return false;
            }
        }

        /// <summary>
        /// Takes in a edited moiety formula of type &lt;T&gt;. Saves result to internal variable if it's a valid ChemicalFormula. 
        /// </summary>
        /// <returns>
        /// true if the moiety formula is valid. Returns false otherwise.
        /// </returns>
        protected bool ValidateMoietyFormula(string textToValidate, int rowNumber)
        {
            try
            {
                // if chemical formula is just whitespace or empty, return the default empty ChemicalFormula object
                if (string.IsNullOrWhiteSpace(textToValidate))
                {
                    this._formula = new ChemicalFormula();
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
                    var messageBoxQuery = string.Format("Moiety parsing error in table column 2, row {0}. Chemical formula \"{1}\" is not valid.", rowNumber + 1, textToValidate);
                    var messageBoxShortPrompt = string.Format("Moiety Parsing Error!");
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                    return false;
                }
            }
            catch (ArgumentException e)
            {
                var messageBoxQuery = string.Format("Moiety parsing error in table column 2, row {0}. Chemical formula \"{1}\" is not valid.\n\nSpecific error:\n{2}", rowNumber + 1, textToValidate, e.Message);
                var messageBoxShortPrompt = string.Format("Moiety Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                return false;
            }
        }

        /// <summary>
        /// Checks to see if string value is an integer and valid charge. 
        /// </summary>
        /// <returns>
        /// true if the adduct charge is a valid integer. Returns false otherwise.
        /// </returns>
        protected bool IsInteger(string textToValidate)
        {
            if (int.TryParse(textToValidate, out int i))
            {
                return true;
            }

            return false;
        }
    }
}
