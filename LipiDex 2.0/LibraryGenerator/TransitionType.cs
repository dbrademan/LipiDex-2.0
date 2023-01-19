using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.LibraryGenerator
{
    public class TransitionType
    {
		public string name;             //Name of transition type
		public string fattyAcidType;    //Type of fatty acid
		public bool isFattyAcid;     //True iff the transition type involves fatty acid moieties
		public bool isNeutralLoss;   //True iff the transition type is a loss from a precursor
		public int numberOfFattyAcids;       //Number of fatty acids involved in transition

		//Constructor
		public TransitionType(string name, string fattyAcidType, bool isFattyAcid, bool isNeutralLoss, int numFattyAcids)
		{
			this.name = name;
			this.fattyAcidType = fattyAcidType;
			this.isFattyAcid = isFattyAcid;
			this.isNeutralLoss = isNeutralLoss;
			this.numberOfFattyAcids = numFattyAcids;
		}

		//Method to return mass based on rule
		public double CalculateMass(List<FattyAcid> fattyAcidArray, Lipid lipid, double mass, int charge, string polarity)
		{
			double result = 0.0;

			//Validate format
			if (!IsValid(fattyAcidArray))
			{
				throw new CustomException ("Invalid transition definition supplied", null);
			}

			//If moiety-based fragment
			else if (name.Contains(" DG Fragment"))
			{
				double fragSum = 0.0;

				//Sum moiety masses
				for (int i=0; i< fattyAcidArray.Count; i++)
				{
					fragSum += fattyAcidArray[i].mass;
				}

				//(Electrons + moiety mass + formula mass) / charge
				result = AddElectrons((fragSum+mass),charge, polarity)/charge;
			}
			//If moiety-based fragment
			else if (!isNeutralLoss && isFattyAcid)
			{
				double fragSum = 0.0;

				//Sum moiety masses
				for (int i = 0; i < fattyAcidArray.Count; i++)
				{
					fragSum += fattyAcidArray[i].mass;
				}

				//(Electrons + moiety mass + formula mass) / charge
				result = AddElectrons((fragSum + mass), charge, polarity) / charge;
			}
			//If static fragment
			else if (!this.isNeutralLoss && !this.isFattyAcid)
			{
				//(Electrons + formula mass) / charge
				result = AddElectrons(mass, charge, polarity) / charge;
			}
			//If moeity-based neutral loss
			else if (this.isNeutralLoss && this.isFattyAcid)
			{
				//(Electrons + (precursor - moiety + formula mass)) / charge
				result = AddElectrons((lipid.mass - fattyAcidArray[0].mass + mass), charge, polarity) / charge;
			}
			//If static neutral loss
			else if (this.isNeutralLoss && !this.isFattyAcid)
			{
				//(Electrons + (precursor + formula mass)) / charge
				result = AddElectrons((lipid.mass + mass), charge, polarity) / charge;
			}

			return result;
		}

		//Returns true iff the fatty acid types and numbers are valid
		private bool IsValid(List<FattyAcid> faArray)
		{
			//Validate number
			if (faArray.Count != this.numberOfFattyAcids)
			{
				return false;
			}

			//Validate type for PUFAs
			if (faArray.Count > 0 && fattyAcidType.Contains("PUFA"))
			{
				for (int i = 0; i < faArray.Count; i++)
				{
					if (!faArray[i].polyUnsaturatedFattyAcid)
					{
						return false;
					}
				}
			}
			//Validate type for all other moieties
			else
			{
				for (int i = 0; i < faArray.Count; i++)
				{
					if (!faArray[i].type.Equals(fattyAcidType))
					{
						return false;
					}
				}
			}

			return true;
		}

		//Add electron mass to fragment based on charge and polarity
		public double AddElectrons(double mass, int charge, string polarity)
		{
			//If adding electrons
			if (polarity.Equals("+"))
            {
				return mass - charge * Utilities.MASSOFELECTRON;
			}
			//If removing electrons
			else
            {
				return mass + charge * Utilities.MASSOFELECTRON;
			}
		}

		//Return string representation of object
		public override string ToString()
		{
			return this.name;
		}
    }
}
