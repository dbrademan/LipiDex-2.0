using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.LibraryGenerator
{
    public class ConsensusLipidClass
    {
		public LipidClass lipidClass;                          //Associated lipidclass
		public Adduct adduct;                              //Associated adduct class
		public string name;                                //composite name
		public List<Transition> fattyAcylBasedTransitions;   //Array of all fatty acid transitions
		public List<Lipid> possibleLipids;            //Array list of all possible lipids in class

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
