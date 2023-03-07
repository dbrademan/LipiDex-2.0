using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.LibraryGenerator
{
    /// <summary>
	/// Custom Comparer for Lipid Moieties
	/// </summary>
    public class LipidMoietyComparer : Comparer<LipidMoiety>
    {
        public override int Compare(LipidMoiety thisMoiety, LipidMoiety otherMoiety)
        {
            if (thisMoiety.GetType().Equals(otherMoiety.GetType()))
            {
                if (char.IsLetter(otherMoiety.GetName()[0]) && !char.IsLetter(thisMoiety.GetName()[0]))
                {
                    return 1;
                }
                else if (!char.IsLetter(otherMoiety.GetName()[0]) && char.IsLetter(thisMoiety.GetName()[0]))
                {
                    return -1;
                }
                else
                {
                    if (thisMoiety.GetChemicalFormula().MonoisotopicMass > otherMoiety.GetChemicalFormula().MonoisotopicMass)
                    {
                        return 1;
                    }
                    else if (thisMoiety.GetChemicalFormula().MonoisotopicMass < otherMoiety.GetChemicalFormula().MonoisotopicMass)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            else
            {
                if (thisMoiety.GetChemicalFormula().MonoisotopicMass > otherMoiety.GetChemicalFormula().MonoisotopicMass)
                {
                    return 1;
                }
                else if (thisMoiety.GetChemicalFormula().MonoisotopicMass < otherMoiety.GetChemicalFormula().MonoisotopicMass)
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
}
