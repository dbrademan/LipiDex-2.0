using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.LibraryGenerator
{
    internal class PolymericHeadgroup : LipidMoiety
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

        #endregion


    }
}
