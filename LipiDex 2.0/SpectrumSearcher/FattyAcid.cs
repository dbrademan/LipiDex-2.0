using System;
using System.Linq;
using CSMSL.Chemistry;

namespace LipiDex_2._0.SpectrumSearcher;

public class FattyAcid : IComparable<FattyAcid> 
{
	public string formula; 				//Elemental Formula
	public string name; 				//Abbreviated name
	public double mass; 				//Mass for sorting purpose
	public int carbonNumber;		//Number of carbons in FA chain
	public int dbNumber;			//Number of double bonds in FA chain
	public bool pufa = false;		//True iff fatty acids is a polyunsaturated fatty acid
	public bool enabled = false;		//True iff the fatty acid will be used for library generation
	public string type;					//Type of fatty acid
	
	public FattyAcid(string name, string type, string formula, string enabled)
	{
		this.name = name;

		var chemicalFormula = new ChemicalFormula(formula);
		mass = chemicalFormula.MonoisotopicMass;
		
		this.formula = formula;
		this.type = type;	
		this.enabled = enabled.Equals("true");

		//Decide whether fatty acid is a PUFA
		var numUnsats = int.Parse(name.Substring(name.IndexOf(":") + 1));
		if (numUnsats > 1)
			pufa = true;

		//Parse fatty acid name for carbon and db number calculation
		parseFA();
	}

	// Return elemental Formula
	public string getFormula()
	{
		return formula;
	}
	
	 // Return name
	 public string getName()
	 {
	 	return name;
	 }

	 // Return FA mass
	 public double getMass()
	 {
	 	return mass;
	 }

	// Sort Fatty Acid by type and molecular weight
	public int CompareTo(FattyAcid other)
	{
		if (other.type != type)
		{
			if (char.IsLetter(other.name.ElementAt(0)) && !char.IsLetter(name.ElementAt(0))) 
				return 1;
			else if (!char.IsLetter(other.name.ElementAt(0)) && char.IsLetter(name.ElementAt(0))) 
				return -1;
			else
			{
				if (mass > other.getMass()) return 1;
				if (mass < other.getMass()) return -1;
				else return 0;
			}
		}
		else
		{
			if (mass > other.getMass()) return 1;
			if (mass < other.getMass()) return -1;
			else return 0;
		}
		//else return (type.compareTo(other.type));
	}

	// Writes Fatty Acid to string for text writing
	public string SaveString()
	{
		string result = "";

		result += name+",";
		result += type+",";
		result += formula+",";

		if (enabled) result += true;
		else result += false;

		return result;
	}
	// Parse carbon number and double bond number from FA name
	public void parseFA()
	{
		string[] split = name.Split(':');

		//remove all letter characters
		for (int i=0; i<split.Length; i++)
		{
			split[i] = split[i].Replace("[^\\d.]", "");
			split[i] = split[i].Replace("-", "");
		}


		//Find carbon number
		carbonNumber = int.Parse(split[0]);

		//Find db number
		dbNumber = int.Parse(split[1]);
	}

	//Returns FA name
	public string ToString()
	{
		return name;
	}

}



