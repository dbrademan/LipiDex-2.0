using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.LibraryGenerator
{
    internal class Adduct
    {
		internal string formula;     //Elemental Composition
		internal string name;        //Name of adduct
		internal bool loss;       //True iff adduct is a NL
		internal string polarity;    //polarity
		internal int charge;         //charge state of adduct

		//Constructor
		internal Adduct(string name, string formula, bool loss, string polarity, int charge)
		{
			this.name = name;
			this.formula = formula;
			this.loss = loss;
			this.polarity = polarity;
			this.charge = charge;
		}

		//Returns polarity
		internal string GetPolarity()
		{
			return polarity;
		}

		//Returns charge
		internal int GetCharge()
		{
			return this.charge;
		}

		//Returns name
		internal string GetName()
		{
			return this.name;
		}


		//Returns chemical formula
		internal string GetFormula()
		{
			return this.formula;
		}

		//Returns string array representation of adduct for table
		internal List<string> GetTableArray()
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
			if (loss) result += "TRUE" + ",";
			else result += "FALSE" + ",";
			result += polarity + ",";
			result += charge + ",";

			return result;
		}
	}
}
