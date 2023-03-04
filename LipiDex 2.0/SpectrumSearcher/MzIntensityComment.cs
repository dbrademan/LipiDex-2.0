// This class derived from MS-DIAL source code. 

using System;
using System.Collections.Generic;
using System.Linq;


namespace LipiDex_2._0.SpectrumSearcher;

/// <summary>
/// ---DEPRECATED---
///
/// Use SpectrumSearcher/Transition class
/// 
/// Stores one MS peak parsed from MSP library.
/// Stores MZ and intensity.
/// The Comment field is NOT the Comment line in the MSP entry,
///     rather it's the stuff in quotes after the MZ and intensity values. 
/// </summary>
public class MzIntensityComment : IComparable<MzIntensityComment>
{
    public double Mz { get; set; }
    public double Intensity { get; set; }
    public string Comment { get; set; }             
    public string? FragmentFormula { get; set; }
    public string Fragment { get; set; }
    
    public TransitionType transitionType { get; set; }
    public List<string> FattyAcids { get; set; } = new();

    public void ParseComment()
    {
        var commentSplit = Comment.Split('_');
        // Examples of  comment string:
        //      "C3H6O2_Alkyl Fragment_[12:0]"
        //      "C-12H-23O-10N-1_Neutral Loss_[]"
        // 3 parts to Paul's LipiDex comments:
        //    1. Chemical Formula (sometimes is a "-" character to indicate not given)
        //          Sometimes the Formula uses the '-' character to separate each Atom
        //    2. "XYZ Fragment"
        //    3. [FattyAcid1, FattyAcid2]  e.g. [20:2, 18:1]. Can also be empty brackets []
        var formula = commentSplit[0];
        if (formula == "-")
        {
            FragmentFormula = null;
        }
        else
        {
            FragmentFormula = formula;
        }
        
        Fragment = commentSplit[1];

        var faSplit = commentSplit[2]
            .Replace("[", "")
            .Replace("]", "")
            .Split(',');
        if (faSplit.Length == 1 && faSplit[0] == string.Empty)
        {
            return;
        }
        else
        {
            FattyAcids = faSplit.ToList();
        }
    }

    public int CompareTo(MzIntensityComment other)
    {
        if (other.Mz > Mz) return 1;
        if (other.Mz < Mz) return -1;
        return 0;
    }
    
}