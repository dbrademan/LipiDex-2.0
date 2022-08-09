using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.LibraryGenerator
{
	internal class MS2
	{
		double precursor;                   //precursor sampled for ms2
		string polarity;					//Polarity of ms2
		int charge;                         //Charge of samples precursor
		double maxIntensity;                //Intensity of most intense fragment
		double sn;                          //signal to noise
		List<Transition> transitions;  //ArrayList of all transitions

		//Constructor
		public MS2(double precursor, string polarity, int charge)
		{
			this.precursor = precursor;
			this.polarity = polarity;
			this.charge = charge;
			this.transitions = new List<Transition>();
			this.sn = 0.0;
		}

		//Return arraylist of all transitions
		public List<Transition> GetTransitions()
		{
			return this.transitions;
		}

		//Add transition to array
		public void AddTransition(Transition t)
		{
			this.transitions.Add(t);
		}

		//Returns string representation of ms2
		public override string ToString()
		{
			return this.transitions.ToString();
		}
	}
}
