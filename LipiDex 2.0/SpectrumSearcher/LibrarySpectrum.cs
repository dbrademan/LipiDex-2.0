// This class was derived from MS-DIAL source code. 

using System;
using System.Collections.Generic;
using csgoslin;
using CSMSL.Chemistry;

namespace LipiDex_2._0.SpectrumSearcher;

public class LibrarySpectrum : IComparable<LibrarySpectrum>
{
    public int Id { get; set; }
    public int BinId { get; set; }
    public int PeakNumber { get; set; }
    public string? Name { get; set; }
    public string Formula { get; set; }
    public ChemicalFormula ChemicalFormula { get; set; }
    public double PrecursorMz { get; set; } = 0;
    public string CompoundClass { get; set; }
    public string AdductIon { get; set; }
    public float RetentionTime { get; set; }
    public string? Polarity { get; set; }
    public string Comment { get; set; }
    public List<MzIntensityComment> MzIntensityCommentList { get; set; }

    public List<Transition> TransitionArray { get; set; } = new();
    
    public string Instrument { get; set; }
    public string CollisionEnergy { get; set; }
    public float QuantMass { get; set; }
    public bool IsLipiDex { get; set; } = false;
    public bool IsOptimalPolarity { get; set; } = false;

    public int MSnOrder { get; set; } = 2;    // Default MS order = 2 is acceptable 
    
    public string? Library { get; set; }

    private LipidAdduct GoslinLipid;
    public string SumId;

    /// <summary>
    /// It's possible to parse an MSP entry and not have all the essential parts, therefore check with this method.
    /// Every lipid library entry should have at least: PrecursorMz, Polarity, Name, Library source
    /// Then parse the name with CsGoslin.
    /// </summary>
    public void ValidateEntry(LipidParser parser)
    {
        if (PrecursorMz == 0 || Polarity == null || Name == null || Library == null)
            throw new Exception($"Msp Library entry {Name} missing required information");

        try
        {
            GoslinLipid = parser.parse(Name);
            SumId = GoslinLipid.get_lipid_string(LipidLevel.SPECIES);
        }
        catch (LipidException)
        {
            SumId = Name;
        }
    }

    public int CompareTo(LibrarySpectrum other)
    {
        if (other.PrecursorMz > PrecursorMz) return 1;
        if (other.PrecursorMz < PrecursorMz) return -1;
        return 0;
    }
}
