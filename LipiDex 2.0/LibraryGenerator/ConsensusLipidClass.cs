using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.LibraryGenerator
{
    internal class ConsensusLipidClass
    {
		internal LipidClass lipidClass;                          //Associated lipidclass
		internal Adduct adduct;                              //Associated adduct class
		internal string name;                                //composite name
		internal List<Transition> fattyAcylBasedTransitions;   //Array of all fatty acid transitions
		internal List<Lipid> possibleLipids;            //Array list of all possible lipids in class

		//Constructor
		public ConsensusLipidClass(LipidClass lipidClass, Adduct adduct)
		{
			this.lipidClass = lipidClass;
			this.adduct = adduct;
			this.name = lipidClass.GetAbbreviation() + " " + adduct.GetName();
			this.fattyAcylBasedTransitions = new List<Transition>();
		}

		//Returns class name
		public string GetName()
		{
			return this.name;
		}

		//Returns string representation of lipid class + adduct
		public override string ToString()
		{
			string result = "";

			result = result + this.lipidClass.GetAbbreviation() + " " + adduct.GetName() + "\n";

			return result;
		}
	}
}
