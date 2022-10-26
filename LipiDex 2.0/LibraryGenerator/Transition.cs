using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.LibraryGenerator
{
	public class Transition
	{
		public double mass;        //Mass of fragment
		public double intensity;   //Relative intensity of fragment, scaled to 999
		public string type;        //Type of transition

		//Constructor
		public Transition(double mass, double intensity, string type)
		{
			this.mass = mass;
			this.intensity = intensity;
			this.type = type;
		}

		//Returns mass
		public double GetMass()
		{
			return this.mass;
		}

		//Returns type
		public new string GetType()
		{
			return this.type;
		}

		//Returns intensity
		public double GetIntensity()
		{
			return this.intensity;
		}

		//Return string representation of transition
		public override string ToString()
		{
			string result = "";
			result = this.mass + " " + this.intensity + " \"" + this.type + "\"";
			return result;
		}
	}

	public class TransitionComparer : Comparer<Transition>
	{
		public override int Compare(Transition thisTransition, Transition otherTransition)
		{
			if (otherTransition.GetIntensity() > thisTransition.GetIntensity())
			{
				return 1;
			}
			else if (otherTransition.GetIntensity() < thisTransition.GetIntensity())
			{
				return -1;
			}
			else 
			{
				return 0;
			}
			
		}
	}
}
