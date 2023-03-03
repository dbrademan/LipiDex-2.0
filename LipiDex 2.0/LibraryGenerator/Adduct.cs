using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSMSL.Chemistry;

namespace LipiDex2.LibraryGenerator
{
	public class Adduct
    {
		public ChemicalFormula formula;     //Elemental Composition
		public string name;        //Name of adduct
		public bool loss;       //True iff adduct is a NL
		public string polarity;    //polarity
		public int charge;         //charge state of adduct

		//Constructor
		public Adduct(string name, string formula, string loss, string polarity, string charge)
		{
			this.name = name;
			this.formula = new ChemicalFormula(formula);

			if (loss.Equals("TRUE") || loss.Equals("True") || loss.Equals("true"))
            {
				this.loss = true;
			}
			else if (loss.Equals("FALSE") || loss.Equals("False") || loss.Equals("false"))
			{
				this.loss = false;
			}
			else
            {
				this.loss = false;
				var t = "";
            }

			this.polarity = polarity;
			this.charge = Convert.ToInt32(charge);
		}

		//Returns polarity
		public string GetPolarity()
		{
			return polarity;
		}

		//Returns charge
		public int GetCharge()
		{
			return this.charge;
		}

		//Returns name
		public string GetName()
		{
			return this.name;
		}


		//Returns chemical formula
		public string GetFormula()
		{
			return this.formula;
		}

		//Returns string array representation of adduct for table
		public List<string> GetTableArray()
		{
			List<string> result = new List<string>();

			result.Add(this.name);
			result.Add(this.formula);
			result.Add(this.loss.ToString());
			result.Add(this.polarity);
			result.Add(this.charge.ToString());

			return result;
		}

		//Returns string representation of adduct
		public override string ToString()
		{
			string result = "";

			result += name + ",";
			result += formula + ",";
			if (loss)
			{
				result += "TRUE" + ",";
			}
			else
			{
				result += "FALSE" + ",";
			}
			result += polarity + ",";
			result += charge + ",";

			return result;
		}
	}
}
