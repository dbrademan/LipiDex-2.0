using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public string classAbbreviation { get; set; }                              //Abbreviated class name
        public string headGroupFormula { get; set; }   						//Elemental formula of head group
        public string adducts { get; set; }                                    //Array of adduct objects allowed for each class
        public string classBackbone { get; set; }                           //Lipid backbone.	
        public string optimalPolarity { get; set; }                              //Polarities that generate structural information

        #endregion

        #region Lipid Class Properties - Private Properties
        private string _classAbbreviation;                              //Abbreviated class name
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
        /// Constructor - Takes in string variables scraped from the LipidClass.csv template file and forms and intermediate LipidClass object for organizing lipid classes in the GUI grid.
		/// <br/>
		/// <br/>
		/// Some validation methods in this class behave differently than the other lipid moieties. If the linker variables don't validate, report errors in a log box.
		/// Does not immediately populate the following fields:
		/// - _neutralFormula
		/// ...
        /// </summary>
        public LipidClass(string className, string classAbbreviation, string headgroupString,
				string delimitedAdducts, string backboneClassifier, string optimalPolarity, string moiety1 = "", string moiety2 = "", string moiety3 = "", string moiety4 = "")
		{
			//Instantiate class variables
			ValidateLipidClassName(className, -1);
			ValidateLipidClassAbbreviation(classAbbreviation, -1);
			ValidateHeadGroup(headgroupString, -1);
			ValidateBackboneClassifier(backboneClassifier, -1);
            ValidateLipidClassAdducts(delimitedAdducts, -1);
            ValidateBackboneFormula(backboneClassifier, -1);
			
			ValidateOptimal
			this.numberOfFattyAcids = numberOfFattyAcids;
			this.optimalPolarity = optimalPolarity;
			this.fattyAcidTypes = fattyAcidTypes;
			this.possibleFattyAcids = new List<List<FattyAcid>>();

			//Calculate elemental formula
			CalculateFormula();
		}

		//Returns string array representation of class for table display
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

		//Return class abbreviation as string
		public string GetAbbreviation()
		{
			return this.classAbbreviation;
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

		//Returns elemental formula
		public string GetFormula()
		{
			return this.formula;
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
	}
}
