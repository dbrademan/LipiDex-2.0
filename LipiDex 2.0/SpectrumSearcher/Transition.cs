// FULLY IMPLEMENTED LIPIDEX 1.0 METHODS

using System;
using System.Collections.Generic;

namespace LipiDex_2._0.SpectrumSearcher;

/// <summary>
/// Stores one MS peak parsed from MSP library.
/// In LipiDex libraries, each peak has a comment string inside quotes ("comment").
/// Comment contains chemical Formula, Transition Type, and fatty acids.
///
/// </summary>
public class Transition : IComparable<Transition>
{
    public double mass;					// Mass of fragment
    public double intensity;			// Intensity of fragment

    public string Comment;              // Text contained inside double quotes "" 

    public string Formula;				// Elemental Formula of fragment
    public string type;					// Type of fragment
    public TransitionType typeObject;	// Transition type object
    public string FattyAcid = "";		// String for associated fatty acid

    public Transition(double mass, double intensity, string comment)
    {
        this.mass = mass;
        this.intensity = intensity;
        Comment = comment;
    }

    public void ParseComment(Dictionary<string, TransitionType> transitionTypes)
    {
        var commentSplit = Comment.Split(' ');

        Formula = commentSplit[0];
        // if (Formula == "-") Formula = "";
        var transitionTypeText = commentSplit[1];
        var fattyAcid = commentSplit[2];
                
        // Remove the [ ] brackets surrounding the fatty acids in string.
        //     Note that the fatty acid string does not store multiple fatty acids inside the brackets (as in cardiolipins)
        FattyAcid = fattyAcid.Substring(1, fattyAcid.IndexOf(']'));  

        typeObject = transitionTypes[transitionTypeText];
        
        type = transitionTypeText;
    }
	    
    // Compares transitions by mass
    public int CompareTo(Transition other)
    {
        if (mass > other.mass) return 1;
        if (mass < other.mass) return -1;
        return 0;
    }
	    
    public string ToString()
    {
        return mass + " " + intensity;
    }
}