using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.LibraryGenerator
{
	public class Lipid
	{
		public List<FattyAcid> fattyAcids;    //array of FAs
		public LipidClass lipidClass;                  //Class
		public Adduct adduct;                      //Adduct
		public string formula;                     //Formula
		public double mass;                        //monoisotopic mass
		public string polarity;                    //Polarity of lipid
		public string name;                        //canonical name
		public MSn generatedMS2;                   //generated MS2
		public List<string> uniqueTypes;      //Array of fa types this lipid can identify, used in LF

		//Constructor
		public Lipid(List<FattyAcid> fattyAcids, LipidClass lipidClass, Adduct adduct)
		{
			this.fattyAcids = fattyAcids;
			this.lipidClass = lipidClass;
			this.adduct = adduct;
			this.polarity = adduct.polarity;
			GenerateName();
			CalculateFormula();
		}

		//Return mass
		public double GetMass()
		{
			return this.mass;
		}

		//Return adduct
		public Adduct GetAdduct()
		{
			return this.adduct;
		}

		//Return generated MS2
		public MSn GetGeneratedMS2()
		{
			return this.generatedMS2;
		}

		//Return lipidClass object
		public LipidClass GetLipidClass()
		{
			return this.lipidClass;
		}

		//Return arrayList of fatty acids in lipid
		public List<FattyAcid> GetFattyAcids()
		{
			return this.fattyAcids;
		}

		//Add entry to unique FA type array
		public void AddUniqueFAType(string fattyAcidType)
		{
			if (this.uniqueTypes == null)
			{
				this.uniqueTypes = new List<string>();
			}

			this.uniqueTypes.Add(fattyAcidType);
		}

		//Get formula
		public string GetFormula()
		{
			return this.formula;
		}

		//Get canonical name
		public string GetName()
		{
			return this.name;
		}

		//Return adduct name as string
		public string GetAdductName()
		{
			return this.adduct.GetName();
		}

		//Adds a generator ms2 object
		public void AddGeneratedMS2(MSn ms2)
		{
			this.generatedMS2 = ms2;
		}

		//Sort FA Array
		public void SortFattyAcids()
		{
			var fattyAcidComparer = new FattyAcidComparer();
			this.fattyAcids.Sort(fattyAcidComparer);
		}

		//Calculate Elemental Composition
		public void CalculateFormula()
		{
			string tempFormula = "";

			//add in backbone + headgroup
			tempFormula = this.lipidClass.GetFormula();

			//add in FAs
			for (int i = 0; i < this.fattyAcids.Count; i++)
			{
				tempFormula = Utilities.MergeFormulas(tempFormula, this.fattyAcids[i].GetFormula());
			}

			//add in adduct
			tempFormula = Utilities.MergeFormulas(tempFormula, this.adduct.GetFormula());

			this.formula = tempFormula;

			CalculateMass();
		}

		public override string ToString()
		{
			return this.GetName();
		}

		//Calculate Monoisotopic mass
		public void CalculateMass()
		{
			this.mass = Utilities.CalculateMassFromFormula(this.formula) / this.adduct.charge;
		}

		//Generate canonical name
		public void GenerateName()
		{
			//Add class name
			this.name = this.lipidClass.GetAbbreviation() + " ";

			//Add fatty acids
			for (int i = 0; i < this.fattyAcids.Count; i++)
			{
				this.name = this.name + this.fattyAcids[i].GetName();
				if ((i + 1) < this.fattyAcids.Count)
				{
					this.name = this.name + "_";
				}
			}

			this.name = this.name + " " + this.adduct.GetName();
		}

		//Generatre msp entry for library generation
		public string GenerateMSPResult()
		{
			string result = "";
			bool optimalPolarity = false;

			if (this.lipidClass.optimalPolarity.Contains(this.polarity))
			{
				optimalPolarity = true;
			}

			//Name field
			result += "Name: " + this.name + ";\n";

			//MW field
			result += "MW: " + Utilities.RoundToFourDecimals(this.mass) + "\n";

			//Precursor MZ field
			result += "PRECURSORMZ: " + Utilities.RoundToFourDecimals(this.mass) + "\n";

			//Comment Field
			result += "Comment: " + "Name=" + this.name
					+ " Mass=" + Utilities.RoundToFourDecimals(this.mass)
					+ " Formula=" + this.formula
					+ " OptimalPolarity=" + optimalPolarity
					+ " Type=LipiDex" + "\n";

			//NumPeaks Field
			result += "Num Peaks: " + this.generatedMS2.GetTransitions().Count + "\n";

			//MS2 array
			for (int i = 0; i < this.generatedMS2.GetTransitions().Count; i++)
			{
				result += Utilities.RoundToFourDecimals(generatedMS2.GetTransitions()[i].GetMass()) + " " +
						Math.Round(generatedMS2.GetTransitions()[i].GetIntensity())
						+ " \"" + generatedMS2.GetTransitions()[i].GetType() + "\"\n";
			}

			return result;
		}
	}
}
