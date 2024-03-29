﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LipiDex_2._0.LibraryGenerator
{
	public class MSnTemplate
	{
		public static MSnTemplateComparer FattyAcidComparer = new MSnTemplateComparer();
		public ConsensusLipidClass lipidClass;                 //Lipid class
		public List<TransitionDefinition> transitions;    //Arraylist for transition definitions
		public List<FattyAcid> possibleFattyAcids;        //Array of all theoretically possible fatty acids
		public List<Lipid> theoreticalLipids;             //Array of all theoretical lipids

		//Constructor
		public MSnTemplate(ConsensusLipidClass lipidClass, List<TransitionDefinition> transitions)
		{
			this.lipidClass = lipidClass;
			this.transitions = transitions;
			this.possibleFattyAcids = new List<FattyAcid>();
			this.theoreticalLipids = new List<Lipid>();
		}

		//Add fatty acid to possible array
		public void AddFattyAcid(FattyAcid fattyAcid)
		{
			this.possibleFattyAcids.Add(fattyAcid);
		}

		//Returns transition array
		public List<TransitionDefinition> GetTransitions()
		{
			return this.transitions;
		}

		//Generate theoretical spectra from fragmentation rules
		public void GenerateInSilicoMS2(List<TransitionType> transitionTypes)
		{
			//Iterate  through all lipids
			for (int i = 0; i < this.theoreticalLipids.Count; i++)
			{
				this.theoreticalLipids[i].AddGeneratedMS2(GenerateMS2(theoreticalLipids[i], transitionTypes));
			}
		}

		//Add a theoretical lipid to array
		public void AddTheoreticalLipid(Lipid lipid)
		{
			this.theoreticalLipids.Add(lipid);
		}

		//Clear theoretical lipid array
		public void ClearTheoreticalLipids()
		{
			this.theoreticalLipids = new List<Lipid>();
		}

		//Generate all possible lipids for lipid class based on active fatty acids
		public void GenerateLipids()
		{
			List<List<LipidMoiety>> faArray = this.lipidClass.lipidClass.GetPossibleMoieties();
			List<Lipid> result = new List<Lipid>();
			List<LipidMoiety> faTemp = new List<LipidMoiety>();
			List<LipidMoiety> cLTemp1 = new List<LipidMoiety>();
			List<LipidMoiety> cLTemp2 = new List<LipidMoiety>();
			List<string> shadowArray = new List<string>();
			string fattyAcidString;
			Lipid lipidTemp;

			List<int> limits = new List<int>(faArray.Count);
			List<int> counters = new List<int>(faArray.Count);

			//Populate limits
			for (int i = 0; i < counters.Count; i++)
			{
				limits[i] = faArray[i].Count - 1;
			}

			while (true)
			{
				faTemp = new List<LipidMoiety>();
				cLTemp1 = new List<LipidMoiety>();
				cLTemp2 = new List<LipidMoiety>(); ;
				fattyAcidString = "";

				//Populate faTemp array
				for (int i = 0; i < counters.Count; i++)
				{
					faTemp.Add(faArray[i][counters[i]]);
				}

				//Parse cardiolipins 
				if (lipidClass.lipidClass.GetAbbreviation().Equals("CL"))
				{
					//Create 2x2 fatty acid array for CL
					cLTemp1.Add(faTemp[0]);
					cLTemp1.Add(faTemp[1]);
					cLTemp2.Add(faTemp[2]);
					cLTemp2.Add(faTemp[3]);

					//Sort arrays
					cLTemp1.Sort(LipidMoiety.LipidMoietyComparer);
					cLTemp2.Sort(LipidMoiety.LipidMoietyComparer);

					//Create string representing faArray
					fattyAcidString += cLTemp1[0] + "_";
					fattyAcidString += cLTemp1[1] + "_";
					fattyAcidString += cLTemp2[0] + "_";
					fattyAcidString += cLTemp2[1] + "_";

					//Load into faTemp
					faTemp = new List<LipidMoiety>();
					faTemp.Add(cLTemp1[0]);
					faTemp.Add(cLTemp1[1]);
					faTemp.Add(cLTemp2[0]);
					faTemp.Add(cLTemp2[1]);

					//Check if string is unique in shadow array
					if (!shadowArray.Contains(fattyAcidString))
					{
						//Add lipid
						lipidTemp = new Lipid(faTemp, this.lipidClass.lipidClass, this.lipidClass.adduct);
						lipidTemp.GenerateName();
						result.Add(lipidTemp);
						shadowArray.Add(fattyAcidString);
						shadowArray.Add(cLTemp2[0] + "_" +
								cLTemp2[1] + "_" +
								cLTemp1[0] + "_" +
								cLTemp1[1] + "_");
					}
				}
				else
				{
					//Sort fatty acid array
					faTemp.Sort(LipidMoiety.LipidMoietyComparer);

					//Create string representing FAArray
					for (int i = 0; i < faTemp.Count; i++)
					{
						fattyAcidString += faTemp[i] + "_";
					}

					//Check if string is unique in shadow array
					if (!shadowArray.Contains(fattyAcidString))
					{
						//Add lipid
						lipidTemp = new Lipid(faTemp, this.lipidClass.lipidClass, this.lipidClass.adduct);
						lipidTemp.GenerateName();
						result.Add(lipidTemp);
						shadowArray.Add(fattyAcidString);
					}
				}

				// Advance permutation
				if (!Utilities.NextPermutation(limits, counters)) break;
			}

			this.theoreticalLipids = result;
		}

		//Add electron mass to fragment based on charge and polarity
		public double AddElectrons(double mass, int charge, string polarity)
		{
			double result = 0.0;

			//If adding electrons
			if (polarity.Equals("+"))
			{
				result = mass - charge * Utilities.MASSOFELECTRON;
			}
			//If removing electrons
			else
			{
				result = mass + charge * Utilities.MASSOFELECTRON;
			}

			return result;
		}

		//Add transition to ms2 if mass is unique
		public void AddIfUnique(MSn ms2, Transition transition)
		{
			bool massMatchFound = false;

			if (transition != null)
			{
				//Iterate through looking for identical mass
				for (int i = 0; i < ms2.GetTransitions().Count; i++)
				{
					if (Math.Abs(ms2.GetTransitions()[i].GetMass() - transition.mass) < 0.0001)
					{
						//Keep more intense
						if (ms2.GetTransitions()[i].GetIntensity() < transition.intensity)
						{

							ms2.GetTransitions().RemoveAt(i--);
						}
						else
						{
							massMatchFound = true;
						}

						break;
					}
				}

				//If none found, add to ms2
				if (!massMatchFound)
				{
					ms2.AddTransition(transition);
				}
			}
		}

		//Returns ms2 object based on lipid
		public MSn GenerateMS2(Lipid lipid, List<TransitionType> transitionTypes)
		{
			int faCounter = 0;
			List<LipidMoiety> faArray;

			//Find theoretical precursor mass (mass/charge)
			double precursor = AddElectrons(lipid.GetMass(),lipid.adduct.GetCharge(),lipid.polarity);

			//Generate MS2
			MSn result = new MSn(precursor, lipid.GetAdduct().GetPolarity(), lipid.GetAdduct().GetCharge());

			//For all possible fatty acids
			for (int j = 0; j < lipid.fattyAcids.Count; j++)
			{
				//Add in all fragments to MS2 array
				for (int i = 0; i < transitions.Count; i++)
				{
					faArray = new List<LipidMoiety>();

					String type = transitions[i].type;

					//Generate cardiolipin dg transitions
					if (type.Contains("Cardiolipin DG Fragment"))
					{
						if (faCounter == 0 || faCounter == 2)
						{
							faArray.Add(lipid.fattyAcids[faCounter]);
							faArray.Add(lipid.fattyAcids[faCounter + 1]);
							AddIfUnique(result, ParseTransition(transitions[i], lipid, faArray));
						}
					}
					//For all other transitions
					else
					{
						if (transitions[i].typeObject.isFattyAcid)
						{
							faArray.Add(lipid.fattyAcids[faCounter]);

							//For PUFA transitions
							if (transitions[i].typeObject.name.Contains("PUFA"))
							{
								if (((FattyAcid)faArray[0]).polyUnsaturatedFattyAcid)
								{
									AddIfUnique(result, ParseTransition(transitions[i], lipid, faArray));
								}
							}
							else
							{
								if (transitions[i].typeObject.fattyAcidType.Equals(((FattyAcid)faArray[0]).type))
								{
									AddIfUnique(result, ParseTransition(transitions[i], lipid, faArray));
								}
							}
						}
						else
						{
							AddIfUnique(result, ParseTransition(transitions[i], lipid, faArray));
						}	
					}
				}
				faCounter++;
			}

			return result;
		}

		//Returns transition object based on definition and lipid
		public Transition ParseTransition(TransitionDefinition td, Lipid lipid, List<LipidMoiety> faArray)
		{
			Console.WriteLine(td);
			double mass;

			if (td.isFormula)
            {
				mass = td.typeObject.CalculateMass(faArray, lipid, Utilities.CalculateMassFromFormula(td.formula), lipid.adduct.GetCharge(), lipid.adduct.polarity);
			}	
			else
			{
				Console.WriteLine(lipidClass.name);
				mass = td.typeObject.CalculateMass(faArray, lipid, td.mass, lipid.adduct.GetCharge(), lipid.adduct.polarity);
			}

			return new Transition(mass, td.relativeIntensity, td.massFormula + "_" + td.typeObject.name + "_" + faArray);
		}

		//Return string representation of ms2 template
		public override string ToString()
		{
			string result = "";

			//Print Name
			result += lipidClass.GetName() + "\n";

			//Print Transition Masses and intensities
			for (int i = 0; i < transitions.Count; i++)
			{
				result += transitions[i] + "\n";
			}

			return result;
		}
	}

	public class MSnTemplateComparer : Comparer<MSnTemplate>
	{
		// Compares by Length, Height, and Width.
		public override int Compare(MSnTemplate thisTemplate, MSnTemplate otherTemplate)
		{
			if (otherTemplate.lipidClass == null)
			{
				return -1;
			}
			else if (thisTemplate.lipidClass == null)
			{
				return 1;
			}
			else
            {
				return otherTemplate.lipidClass.name.CompareTo(thisTemplate.lipidClass.name);
			}
		}
	}
}
