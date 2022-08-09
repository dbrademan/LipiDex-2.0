using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.LibraryGenerator
{
    internal class TransitionDefinition
    {
		internal double mass;                    //Mass of fragment if applicable
		internal double relativeIntensity;       //Relative intensity scaled to 1000
		internal string formula;                 //Elemental formula
		public string displayName;      //Display name for Jtree display
		internal bool isFormula;              //Boolean if a formula was supplied
		internal int charge;                 //Charge on transition
		internal string massFormula;             //String to store supplied mass/formula string
		internal string type;                    //String to store supplied type
		internal TransitionType typeObject;      //Transition type object					

		public TransitionDefinition(string massFormula, double relIntensity, string displayName, string type, int charge, TransitionType typeObject)
		{
			//Initialize blank variables
			this.mass = -1.0;
			this.formula = "";
			this.type = type;
			this.charge = 1;
			this.typeObject = typeObject;

			//Initialize paramaterized variables
			this.relativeIntensity = relIntensity;
			this.displayName = ParseDisplayName(displayName);
			this.charge = charge;

			//Parse massFormula field
			ParseMassFormula(massFormula);

			//Update mass and formula field if applicable
			UpdateMassFormula();
		}

		public new string GetType()
		{
			return this.type;
		}

		public TransitionType GetTypeObject()
		{
			return this.typeObject;
		}

		public void UpdateMassFormula()
		{
			//If a formula
			if (this.isFormula)
			{
				this.mass = Utilities.CalculateMassFromFormula(this.massFormula);
				this.formula = this.massFormula;

			}
			//If not a formula
			else
			{
				this.mass = Convert.ToDouble(this.massFormula);
			}
		}

		//Check mass formula for correct declaration
		public void ParseMassFormula(string massFormula)
		{
			bool formula = false;

			for (int i = 0; i < Utilities.elements.Count; i++)
			{
				if (massFormula.Contains(Utilities.elements[i]))
				{
					formula = true;
				}
			}

			if (massFormula.Equals("-"))
			{
				formula = true;
			}

			//Check mass validity
			if (!formula)
			{
				try
				{
					this.isFormula = false;
					double massDouble = Convert.ToDouble(massFormula);
					this.massFormula = massFormula;
				}
				catch (Exception e)
				{
					//CustomError e1 = new CustomError(massFormula + " is not a valid mass", null);
				}
			}
			//Check formula validity
			else
			{
				try
				{
					this.isFormula = true;

					if (Utilities.ValidElementalFormula(massFormula))
                    {
						this.massFormula = massFormula;
						this.formula = massFormula;
                    }
					else
                    {
						throw new CustomException(massFormula + " is not a valid elemental formula", new ApplicationException(massFormula + " is not a valid elemental formula"));
					}
				}
				catch (Exception e)
				{
					throw new CustomException(massFormula + " is not a valid elemental formula", e);
					//CustomError e1 = new CustomError(massFormula + " is not a valid elemental formula", null);
				}
			}

		}

		public void UpdateValues(double relInt, string massFormula, string type, string charge)
		{
			try
			{
				//Update charge
				this.charge = Convert.ToInt32(charge);

				//Parse massFormula field
				ParseMassFormula(massFormula);

				//Update relative intensity
				this.relativeIntensity = relInt;

				//Update type
				this.type = type;

				//Reparse display name
				this.displayName = ParseDisplayName(massFormula + "," + relInt + "," + charge + "," + type);

				//Update mass and formula field if applicable
				UpdateMassFormula();
			}
			catch (Exception e)
			{
				throw new CustomException("Error updating entry. Please check formatting", new ApplicationException("Error updating entry. Please check formatting"));
				//CustomError ce = new CustomError("Error updating entry.  Please check formatting", null);
			}
}

	//Format transition for display in tree
	public string ParseDisplayName(string name)
	{
		string result = "";
		string[] split;

		if (name.Contains(","))
		{
			split = name.Split(',');

			if (split[0].Contains("."))
			{
				split[0] = Convert.ToString(Math.Round(Convert.ToDouble(split[0]) * 10000.0) / 10000.0);
			}

			result += string.Format("{0}", split[0]).PadRight(20);
			result += string.Format("{0}", Math.Round(relativeIntensity)).PadRight(5);
			result += string.Format("{0}", charge).PadRight(3);
			result += split[3];
		}
		else
		{
			split = name.Split(new string[] { " +" }, StringSplitOptions.None);

			if (split[0].contains(".")) split[0] = String.valueOf(Math.round(Double.valueOf(split[0]) * 10000.0) / 10000.0);
			result += String.format("%1$-" + 20 + "s", split[0]);
			result += String.format("%1$-" + 5 + "s", Math.round(relativeIntensity));
			result += String.format("%1$-" + 3 + "s", charge);
			result += split[3];
		}
		return result;
	}

	//Adds elemental formula to transition
	public void addFormula(String formula)
	{
		this.formula = formula;
	}

	//Returns elemental formula
	public String getFormula()
	{
		return formula;
	}

	//Returns mass as double
	public Double getMass()
	{
		return mass;
	}

	//Returns relative intensity
	public Double getRelativeIntensity()
	{
		return relativeIntensity;
	}

	//Calculate elemental formula for transitions based on precursor formula
	public void calculateElementalComposition(String precursorFormula)
	{
		addFormula(annotateMassWithMZTolerance(mass, precursorFormula));

		if (!formula.equals(""))
			mass = calculateMassFromFormula(formula);
	}

	//Returns string representation of transition
	public String toString()
	{
		String result = "";
		result = massFormula + "," + relativeIntensity + "," + charge + "," + type;
		return result;
	}

	//Comparator for sorting by intensity
	public int compareTo(Transition t2)
	{
		if (t2.getIntensity() > relativeIntensity) return 1;
		else if (t2.getIntensity() < relativeIntensity) return -1;
		return 0;
	}
}
}
