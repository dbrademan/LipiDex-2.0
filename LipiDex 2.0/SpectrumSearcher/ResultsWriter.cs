using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace LipiDex_2._0.SpectrumSearcher;

public static class ResultsWriter
{
    public static void WriteResults(List<SampleSpectrum> sampleSpectra, 
        string filepath = @"D:\LipidQC\LipidQC\LipidQC.test\test_output\SS_test.csv")
    {
        // Get subset of Sample Spectra that have a library match after filtering and purity calculations 
        var matches = sampleSpectra
            .Where(x => 
                x.IdentificationsList.Count > 0 &&
                x.IdentificationsList[0].DotProduct > 1);    // DotProduct > 1 is fine because dot product is scaled from 0 to 999
        
        using (var writer = new StreamWriter(filepath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<SpectrumSearcherResultsClassMapper>();
            csv.WriteRecords(matches);
        }
    }
}

/// <summary>
/// Maps properties in SampleSpectrum object to columns in Results CSV.
/// the .Name() method gives the column name to write to. 
/// </summary>
public sealed class SpectrumSearcherResultsClassMapper: ClassMap<SampleSpectrum>
{
    public SpectrumSearcherResultsClassMapper()
    {
        Map(m => m.spectraNumber).Name("scan_number");
        Map(m => m.retention).Name("retention_time_mins");
        // Map(m => m.Rank).Name("rank");  // Ranking of the match (from an option to select more than one ID in SS) 
        Map(m => m.IdentificationsList[0].LibrarySpectrum.Name).Name("identification");     // Name and adduct, e.g.  LysoPC 20:5 [M+H]+;
        Map(m => m.precursor).Name("precursor_mz");
        Map(m => m.libraryPrecursorMz).Name("library_precursor_mz");
        Map(m => m.ppmError).Name("ppm_error");
        Map(m => m.DotProduct).Name("dot_product");
        Map(m => m.ReverseDotProduct).Name("reverse_dot_product");
        Map(m => m.Purity).Name("purity");
        Map(m => m.SpectralComponents).Name("spectral_components");  // e.g.  PC 16:1_16:0 [M+Ac-H]-(62) / PC 14:0_18:1 [M+Ac-H]-(38)
        
        Map(m => m.IsOptimalPolarity).Name("optimal_polarity");
        Map(m => m.IsLipiDex).Name("from_lipidex_library");
        Map(m => m.LibrarySource).Name("library");
        Map(m => m.RawFileSource).Name("rawfile");
        
        Map(m => m.LibraryFragments).Name("matched_library_fragments");
    }
}