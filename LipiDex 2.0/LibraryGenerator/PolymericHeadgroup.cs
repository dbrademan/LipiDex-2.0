using CSMSL.Chemistry;
using CSMSL.Glycomics;
using CSMSL.Proteomics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LipiDex_2._0.LibraryGenerator
{
    public class PolymericHeadgroup : LipidMoiety
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
        #region PolymericHeadgroup Properties - Data Grid Display

        public bool isPeptide { get; set; }                                 // parses the headgroup as a peptide if this property is set to true

        public bool isGlycan { get; set; }                                  // parses the headgroup as a glycan if this property is set to true

        public string sequence { get; set; }                                // these monomers will be parsed into individual units

        public string otherFormula { get; set; }                            // a final modification field 

        public string calculatedHeadgroupFormula { get; set; }      // readonly cell that represents the calculated formula from the headgroup sequence and modification shifts

        #endregion

        // This set of properties are used as intermediate placeholders during editing of the data grid.
        #region PolymericHeadgroup Properties - Private

        private bool _isPeptide;                                            // parses the headgroup as a peptide if this property is set to true

        private bool _isGlycan;                                             // parses the headgroup as a glycan if this property is set to true

        private string _sequence;                                           // these monomers will be parsed into individual units

        private Peptide _peptide;                                           // stores internal peptide object iff _isPeptide is true AND the sequence is a parsable peptide.

        private Glycan _glycan;                                             // stores internal peptide object iff _isGlycan is true AND the sequence is a parsable glycan.

        private ChemicalFormula _peptideOrGlycanFormula;                    // a quick-accession field which stores the formula of the polymeric headgroup

        private ChemicalFormula _otherFormula;                              // a final modification field. summarizes all 

        private ChemicalFormula _calculatedHeadgroupFormula;                // readonly cell that represents the calculated formula from the headgroup sequence and modification shifts


        #endregion

        public PolymericHeadgroup(string name, bool isPeptide, bool isGlycan, string sequence, string otherFormula)
        {
            ValidatePolyHeadgroupName(name, -1);
            ValidatePolyHeadgroupType(isPeptide, isGlycan, -1);
            ValidatePolyHeadgroupSequence(sequence, -1);
            ValidateExtraFormulaBalancer(otherFormula, -1);
            CalculateTotalFormula();
        }

        /// <summary>
        /// Takes in a edited polymeric headgroup identifier and makes sure it's not null or empty. Saves result to internal variable if valid. 
        /// </summary>
        /// <returns>
        /// true if the polymeric headgroup name is valid and parsable. Returns false otherwise.
        /// </returns>
        public bool ValidatePolyHeadgroupName(string textToValidate, int rowNumber)
        {
            return ValidateMoietyName(textToValidate, rowNumber);
        }

        /// <summary>
        /// Takes in two edited polymeric headgroup type booleans and checks to make sure both aren't true or false. Saves result to internal variable if valid. Should always be valid
        /// </summary>
        /// <returns>
        /// true if the polymeric headgroup type is valid and parsable. Returns false otherwise. It should never return false, but I've been surpised before.
        /// </returns>
        public bool ValidatePolyHeadgroupType(bool boolToValidate1, bool boolToValidate2, int rowNumber)
        {
            // if both 
            if (boolToValidate1 && boolToValidate2)
            {
                return false;
            }
            else if (!boolToValidate1 && !boolToValidate2)
            {
                return false;
            }
            else
            {
                // force sequence back to an empty string
                this.sequence = string.Empty;
                this._sequence = string.Empty;
                this.isPeptide = boolToValidate1;
                this.isGlycan = !boolToValidate1;
                this._isPeptide = this.isPeptide;
                this._isGlycan = this.isGlycan;
                return true;
            }
        }

        /// <summary>
        /// Takes in the state of the peptide boolean checkbox and sets in the internal properties & types appropriately to true/false. Should always be valid
        /// </summary>
        /// <returns>
        /// true if the polymeric headgroup type is valid and parsable. Returns false otherwise. It should never return false, but I've been surpised before.
        /// </returns>
        public bool ValidatePolyHeadgroupType_Peptide(bool peptideCheckboxIsChecked, int rowNumber)
        {
            if (peptideCheckboxIsChecked)
            {
                if (this._isGlycan)
                {
                    this._isPeptide = true;
                    this._isGlycan = false;
                    this.isPeptide = true;
                    this.isGlycan = false;

                    ValidatePolyHeadgroupSequence("PEPTIDEK", -1);

                    this.otherFormula = "H-2O-1";
                    this._otherFormula = new ChemicalFormula("H-2O-1");
                    CalculateTotalFormula();
                    return true;
                }
            }
            else
            {
                if (this._isPeptide)
                {
                    this._isGlycan = true;
                    this._isPeptide = false;
                    this.isGlycan = true;
                    this.isPeptide = false;

                    ValidatePolyHeadgroupSequence("HexNAc-HexNAc", -1);

                    this.otherFormula = "C-2H-4O-2";
                    this._otherFormula = new ChemicalFormula("C-2H-4O-2");
                    CalculateTotalFormula();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Takes in the state of the glycan boolean checkbox and sets in the internal properties & types appropriately to true/false. Should always be valid
        /// </summary>
        /// <returns>
        /// true if the polymeric headgroup type is valid and parsable. Returns false otherwise. Should never return false, but I've been surprised before
        /// </returns>
        public bool ValidatePolyHeadgroupType_Glycan(bool glycanCheckboxIsChecked, int rowNumber)
        {
            if (glycanCheckboxIsChecked)
            {
                if (this._isPeptide)
                {
                    this._isGlycan = true;
                    this._isPeptide = false;
                    this.isGlycan = true;
                    this.isPeptide = false;

                    ValidatePolyHeadgroupSequence("HexNAc-HexNAc", -1);

                    this.otherFormula = "C-2H-4O-2";
                    this._otherFormula = new ChemicalFormula("C-2H-4O-2");
                    CalculateTotalFormula();
                    return true;
                }
            }
            else
            {
                if (this._isGlycan)
                {
                    this._isGlycan = false;
                    this._isPeptide = true;
                    this.isGlycan = false;
                    this.isPeptide = true;

                    ValidatePolyHeadgroupSequence("TESTPEPTIDEK", -1);

                    this.otherFormula = "H-2O-1";
                    this._otherFormula = new ChemicalFormula("H-2O-1");

                    CalculateTotalFormula();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Takes in an edited polymeric headgroup sequence and checks to see if it's a respective parsable glycan or peptide. Saves result to internal variable if valid.
        /// </summary>
        /// <returns>
        /// true if the polymeric headgroup type sequence is valid for what is selected and parsable. Returns false otherwise. Should never return false, but I've been surprised before
        /// </returns>
        public bool ValidatePolyHeadgroupSequence(string textToValidate, int rowNumber)
        {
            if (this._isPeptide)
            {
                try
                {
                    this._peptide = new Peptide(textToValidate);
                    this._sequence = this._peptide.SequenceWithModifications;
                    this.sequence = this._sequence;
                    this._peptideOrGlycanFormula = this._peptide.GetChemicalFormula();
                    CalculateTotalFormula();
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            else if (this._isGlycan)
            {
                try
                {
                    this._glycan = new Glycan(textToValidate);
                    this._sequence = this._glycan.Sequence;
                    this.sequence = this._sequence;
                    this._peptideOrGlycanFormula = this._glycan.GetChemicalFormula();
                    CalculateTotalFormula();
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Takes in a edited formula. Saves result to internal variable if it's a valid ChemicalFormula. This method is necessary since we technically have several ChemicalFormula objects in this class. 
        /// </summary>
        /// <returns>
        /// true if the edited formula is valid. Returns false otherwise.
        /// </returns>
        public bool ValidateExtraFormulaBalancer(string textToValidate, int rowNumber)
        {
            try
            {
                // if chemical formula is just whitespace or empty, return the default empty ChemicalFormula object
                if (string.IsNullOrWhiteSpace(textToValidate))
                {
                    this._otherFormula = ChemicalFormula.Empty;
                    this.otherFormula = this._otherFormula.ToString();
                    CalculateTotalFormula();
                    return true;
                }
                else if (ChemicalFormula.IsValidChemicalFormula(textToValidate))
                {
                    this._otherFormula = new ChemicalFormula(textToValidate);
                    this.otherFormula = this._otherFormula.ToString();
                    CalculateTotalFormula();
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
        /// Calculates this row's total ChemicalFormula. 
        /// </summary>
        private void CalculateTotalFormula()
        {
            this._calculatedHeadgroupFormula = new ChemicalFormula();
            this._calculatedHeadgroupFormula.Add(this._peptideOrGlycanFormula);
            this._calculatedHeadgroupFormula.Add(this._otherFormula);
            this.calculatedHeadgroupFormula = this._calculatedHeadgroupFormula;
        }

        /// <summary>
        /// Takes the intermediate variables from the supplied PolymericHeadgroup object and double checks that all values are valid. If parsing fails, this method returns false.
        /// </summary>
        /// 
        public bool IsValid(int rowNumber)
        {
            if (this.ValidatePolyHeadgroupName(this.name, rowNumber) && this.ValidatePolyHeadgroupSequence(this.sequence, rowNumber) && this.ValidateExtraFormulaBalancer(this.otherFormula, rowNumber))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Writes out all properties of this object to a stringified version for library templating
        /// </summary>
        public string SaveString()
        {
            var sequence = "";
            if (this._isPeptide)
            {
                sequence = this._peptide.SequenceWithModifications;
            }
            else
            {
                sequence = this._glycan.Sequence;
            }


            var returnString = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"",
                this._name,
                this._isPeptide,
                this._isGlycan,
                sequence,
                this._otherFormula.ToString()
                );
            return returnString;
        }
    }
}