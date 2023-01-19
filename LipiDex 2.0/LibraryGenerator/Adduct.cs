using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using CSMSL.Chemistry;

namespace LipiDex_2._0.LibraryGenerator
{
	public class Adduct : LipidMoiety
    {
        // This set of properties are used as intermediate placeholders during editing of the data grid.
        #region Adduct Properties - Data Grid Display

		public bool loss { get; set; }                    //True iff adduct is a neutral loss from an intact parent
		public string polarity { get; set; }                //polarity
		public string charge { get; set; }                  //charge state of adduct

        #endregion

        // these variables are the used store internal verisons of the Fatty Acid Object
        #region Adduct Properties - Private

		public bool _loss;									//True iff adduct is a neutral loss from an intact parent
		public string _polarity;							//polarity
		public int _charge;                                 //charge state of adduct

        #endregion

        /// <summary>
        /// Takes in string versions of an adduct's name, chemical formula, if it is a neutral loss ("true"/"false"), polarity ("+"/"-"), and charge
        /// </summary>
        public Adduct(string name, string formula, string loss, string polarity, string charge)
		{
            ValidateAdductName(name, -1);
            ValidateAdductFormula(name, -1);

			if (loss.Equals("TRUE") || loss.Equals("True") || loss.Equals("true"))
            {
				this._loss = true;
			}
			else if (loss.Equals("FALSE") || loss.Equals("False") || loss.Equals("false"))
			{
				this._loss = false;
			}
			else
            {
				throw new ArgumentException(string.Format("Neutral loss value \"{0}\" could not be evaluated to boolean. Please make sure to use \"true\" or \"false\" only.", loss));
			}

			this._polarity = polarity;
			this._charge = Convert.ToInt32(charge);

			// if we made it this far, the library entry parsed correctly.
			// set DataGrid-facing properties
			this.name = this._name;
			this.formula = this._formula.ToString();
			this.loss = this._loss;
			this.polarity = this._polarity;
			this.charge = this._charge.ToString();
	    }

		/// <summary>
		/// Retrieves the adduct's private polarity property
		/// </summary>
		/// <returns>
		/// Return the adduct's polarity as a string ('+'/'-')
		/// </returns>
		public string GetPolarity()
		{
			return this._polarity;
		}

		/// <summary>
		/// Retrieves the adduct's private charge property
		/// </summary>
		/// <returns>
		/// Return the adduct's charge as a string ('+'/'-')
		/// </returns>
		public int GetCharge()
		{
			return this._charge;
		}

		/// <summary>
		/// Retrieves the adduct's private name property
		/// </summary>
		/// <returns>
		/// Return the adduct's name as a string ('+'/'-')
		/// </returns>
		public string GetName()
		{
			return this._name;
		}

		/// <summary>
		/// Stringify relevant adduct properties to save to the library csv file.
		/// </summary>
		/// <returns>
		/// Return a stringified representation of this Adduct object
		/// </returns>
		public string SaveString()
		{
			string result = "";

			result += this._name + ",";
			result += this._formula.ToString() + ",";
			if (this._loss)
			{
				result += "true" + ",";
			}
			else
			{
				result += "false" + ",";
			}
			result += this._polarity + ",";
			result += this._charge;

			return result;
		}

        /// <summary>
        /// Takes the intermediate variables from the supplied Adduct object and double checks that all values are valid. If parsing fails, this method returns false.
        /// </summary>
        public bool IsValid(int rowNumber)
        {
            if (this.ValidateAdductName(this.name, rowNumber) && this.ValidateAdductFormula(this.formula, rowNumber) 
                && this.ValidateAdductPolarity(this.polarity, rowNumber) && this.ValidateAdductCharge(this.charge.ToString(), rowNumber)) 
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Takes in a edited adduct name and makes sure it's not null or empty. Saves result to internal variable if valid. 
        /// </summary>
        /// <returns>
        /// true if the adduct name is valid and parsable. Returns false otherwise.
        /// </returns>
        public bool ValidateAdductName(string textToValidate, int rowNumber)
        {
            return ValidateMoietyName(textToValidate, rowNumber);
        }

        /// <summary>
        /// Takes in a edited adduct formula. Saves result to internal variable if it's a valid ChemicalFormula. 
        /// </summary>
        /// <returns>
        /// true if the adduct formula is valid. Returns false otherwise.
        /// </returns>
        public bool ValidateAdductFormula(string textToValidate, int rowNumber)
        {
            return ValidateMoietyFormula(textToValidate, rowNumber);
        }

        /// <summary>
        /// Takes in a edited adduct polarity. Saves result to internal variable if it's "+" or "-". 
        /// </summary>
        /// <returns>
        /// true if the adduct polarity is "+" or "-". Returns false otherwise.
        /// </returns>
        public bool ValidateAdductPolarity(string textToValidate, int rowNumber)
        {
            try
            {
                if (textToValidate.Equals("+") || textToValidate.Equals("-"))
                {
                    this._polarity = textToValidate;
                    this.polarity = this._polarity;
                    return true;
                }
                else
                {
                    throw new ArgumentException(string.Format("Adduct polarity could not be evaluated. Please make sure to use \"+\" or \"-\" only."));
                }
            }
            catch (ArgumentException e)
            {
                var messageBoxQuery = string.Format("Adduct parsing error in table column 4, row {0}.\n\nError Message:\n{1}", rowNumber + 1, e.Message);
                var messageBoxShortPrompt = string.Format("Adduct Loss Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                return false;
            }
        }

        /// <summary>
        /// Takes in a edited adduct charge. Saves result to internal variable if it's a valid charge. 
        /// </summary>
        /// <returns>
        /// true if the adduct charge is valid. Returns false otherwise.
        /// </returns>
        public bool ValidateAdductCharge(string textToValidate, int rowNumber)
        {
            try
            {
                if (IsInteger(textToValidate))
                {
                    this._charge = Math.Abs(Convert.ToInt32(textToValidate));
                    this.charge = this._charge.ToString();
                    return true;
                }
                else
                {
                    throw new ArgumentException(string.Format("Adduct charge could not be evaluated. Please make sure charge is an integer."));
                }
            }
            catch (ArgumentException e)
            {
                var messageBoxQuery = string.Format("Adduct parsing error in table column 4, row {0}.\n\nError Message:\n{1}", rowNumber + 1, e.Message);
                var messageBoxShortPrompt = string.Format("Adduct Loss Parsing Error!");
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                return false;
            }
        }
    }
}
