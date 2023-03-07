using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.LibraryGenerator
{
	/// <summary>
	/// Custom Comparer for Fatty Acids
	/// </summary>
	public class FattyAcidComparer : Comparer<FattyAcid>
	{
		public override int Compare(FattyAcid thisFattyAcid, FattyAcid otherFattyAcid)
		{
			if (!otherFattyAcid.type.Equals(thisFattyAcid.type))
			{
				if (char.IsLetter(otherFattyAcid.name[0]) && !char.IsLetter(thisFattyAcid.name[0]))
				{
					return 1;
				}
				else if (!char.IsLetter(otherFattyAcid.name[0]) && char.IsLetter(thisFattyAcid.name[0]))
				{
					return -1;
				}
				else
				{
					if (thisFattyAcid.mass > otherFattyAcid.GetMass())
					{
						return 1;
					}

					else if (thisFattyAcid.mass < otherFattyAcid.GetMass())
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
				if (thisFattyAcid.mass > otherFattyAcid.GetMass())
				{
					return 1;
				}
				else if (thisFattyAcid.mass < otherFattyAcid.GetMass())
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
