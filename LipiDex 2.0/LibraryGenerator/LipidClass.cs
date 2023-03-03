using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSMSL.Chemistry;

namespace LipiDex2.LibraryGenerator
{
    public class LipidClass
    {
		public string className;                               //Full Class Name
		public string classAbbreviation;                          //Abbreviated class name
		public ChemicalFormula headGroup;                               //elemental formula of head group
		public List<Adduct> adducts;                                //Array of adduct objects allowed for each class
		public bool sterol;                                 //true iff backbone of lipid is sterol
		public bool glycerol;                               //true iff backbone of lipid is glycerol
		public bool sphingoid;                              //true iff sphingoid base
		public ChemicalFormula backboneFormula;                         //Elemental formula of backbone							
		public int numberOfFattyAcids;                              //number of allowed fatty acids
		public string optimalPolarity;                         //Fragment informative polarity
		public List<List<FattyAcid>> possibleFattyAcids;  //Array of possible fatty acids
		public string formula;                                 //Elemental formula of lipid class - fatty acids - adduct
		public List<string> fattyAcidTypes;               //Arraylist of all possible fatty acid classes for class

		//Constructor
		public LipidClass(string className, string classAbbreviation, ChemicalFormula headGroup,
				List<Adduct> adducts, bool sterol, bool glycerol, bool sphingoid, ChemicalFormula backboneFormula,
				int numberOfFattyAcids, string optimalPolarity, List<string> fattyAcidTypes)
		{
			//Instantiate class variables
			this.className = className;
			this.classAbbreviation = classAbbreviation;
			this.headGroup = headGroup;
			this.backboneFormula = backboneFormula;
			this.adducts = adducts;
			this.sterol = sterol;
			this.glycerol = glycerol;
			this.sphingoid = sphingoid;
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
					if (fattyAcidArray[j].fattyAcidCategory.Equals(fattyAcidTypes[i]) && fattyAcidArray[j].enabled)
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
				formula = Utilities.MergeFormulas(formula, "C3H5");
			}
			else if (sphingoid)
            {
				formula = Utilities.MergeFormulas(formula, "C3H2");
			}
			else if (sterol)
            {
				formula = Utilities.MergeFormulas(formula, "C27H45");
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
