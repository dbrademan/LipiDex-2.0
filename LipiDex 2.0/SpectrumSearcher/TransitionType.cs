// FULLY IMPLEMENTED LIPIDEX 1.0 METHODS

using System.Collections.Generic;
using LipiDex_2._0.LibraryGenerator;

namespace LipiDex_2._0.SpectrumSearcher;

public class TransitionType 
{
	public string name;				//Name of transition type
	public string fattyAcidType;	//Type of fatty acid
	public bool isFattyAcid;		//True iff the transition type involves fatty acid moieties
	public bool isNeutralLoss;		//True iff the transition type is a loss from a precursor
	public int numFattyAcids;		//Number of fatty acids involved in transition

	//Constructor
	public TransitionType(string name, string fattyAcidType, bool isFattyAcid, bool isNeutralLoss, int numFattyAcids)
	{
		this.name = name;
		this.fattyAcidType = fattyAcidType;
		this.isFattyAcid = isFattyAcid;
		this.isNeutralLoss = isNeutralLoss;
		this.numFattyAcids = numFattyAcids;
	}

	//Method to return mass based on rule
	public double calculateMass(List<FattyAcid> faArray, Lipid l, double mass, int charge, string polarity) 
	{
		double result = 0.0;

		//If moiety-based fragment
		if (name.Contains(" DG Fragment"))
		{
			double fragSum = 0.0;

			//Sum moiety masses
			for (int i=0; i<faArray.Count; i++)
			{
				fragSum += faArray[i].mass;
			}

			//(Electrons + moiety mass + Formula mass) / charge
			result = addElectrons((fragSum+mass), charge, polarity) / charge;
		}
		//If moiety-based fragment
		else if (!isNeutralLoss && isFattyAcid)
		{
			double fragSum = 0.0;

			//Sum moiety masses
			for (int i=0; i<faArray.Count; i++)
			{
				fragSum += faArray[i].mass;
			}

			//(Electrons + moiety mass + Formula mass) / charge
			result = addElectrons((fragSum+mass),charge, polarity)/charge;
		}
		//If static fragment
		else if (!isNeutralLoss && !isFattyAcid)
		{
			//(Electrons + Formula mass) / charge
			result = addElectrons(mass,charge, polarity)/charge;
		}
		//If moiety-based neutral loss
		else if (isNeutralLoss && isFattyAcid)
		{
			//(Electrons + (precursor - moiety + Formula mass)) / charge
			result = addElectrons((l.mass - faArray[0].mass + mass), charge, polarity) / charge;
		}
		//If static neutral loss
		else if (isNeutralLoss && !isFattyAcid)
		{
			//(Electrons + (precursor + Formula mass)) / charge
			result = addElectrons((l.mass + mass), charge, polarity) / charge;
		}

		return result;
	}

	//Returns true iff the fatty acid types and numbers are valid
	private bool isValid(List<FattyAcid> faArray)
	{
		//Validate number
		if (faArray.Count != numFattyAcids) return false;

		//Validate type for PUFAs
		if (faArray.Count > 0 && fattyAcidType.Contains("PUFA"))
		{
			for (int i=0; i<faArray.Count; i++)
			{
				if (!faArray[i].pufa) return false;
			}
		}
		//Validate type for all other moieties
		else
		{
			for (int i=0; i < faArray.Count; i++)
			{
				if (!faArray[i].type.Equals(fattyAcidType)) return false;
			}
		}

		return true;
	}

	//Add electron mass to fragment based on charge and polarity
	public double addElectrons(double mass, int charge, string polarity)
	{
		//If adding electrons
		if (polarity.Equals("+"))
			return mass - charge * Utilities.MASSOFELECTRON;
		//If removing electrons
		else
			return mass + charge * Utilities.MASSOFELECTRON;
	}

	//Return string representation of object
	public string ToString()
	{
		return name;
	}

}
