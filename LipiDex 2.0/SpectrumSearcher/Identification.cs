// LIPIDEX 1.0 METHODS ARE FULLY IMPLEMENTED

using System;
using System.Collections.Generic;
using LipiDex_2._0.LibraryGenerator;

namespace LipiDex_2._0.SpectrumSearcher;

public class Identification : IComparable<Identification>
{
	public LibrarySpectrum LibrarySpectrum;	    // LibrarySpectrumObject from array
	public double DeltaMass;					// Mass error between library and sample spectra in ppm
	public double DotProduct;					// Dot product between library and sample spectra
	public double ReverseDotProduct;			// Reverse dot product between library and sample spectra
	public int Purity;							// Spectral purity

	//Constructor
	public Identification(LibrarySpectrum ls, double deltaMass, 
			double dotProduct, double reverseDotProduct)
	{
		this.LibrarySpectrum = ls;
		this.DeltaMass = Math.Round(deltaMass * 10000.0) / 10000.0;
		this.DotProduct = dotProduct;
		this.ReverseDotProduct = reverseDotProduct;
		Purity = 0;
	}
	
	//calcPurity for between different classes
	public PeakPurity CalcPurityAll(List<FattyAcid> faDB,
									List<LibrarySpectrum> lipidDB,
									List<Transition> ms2,
									double mzTol)
	{
		List<LibrarySpectrum> isobaricIDs = new List<LibrarySpectrum>();
		PeakPurity peakPurity = new PeakPurity();
		
		//Find isobaric ids
		foreach (var dbSpectrum in lipidDB)
		{
			//If isobaric, add to array
			if (Math.Abs(LibrarySpectrum.PrecursorMz - dbSpectrum.PrecursorMz) < mzTol)
				isobaricIDs.Add(dbSpectrum);
		}
		
		//Verify the spectrum is a lipidex ID
		if (LibrarySpectrum.IsLipiDex)
		{
			if (LibrarySpectrum.IsOptimalPolarity) 
				Purity = peakPurity.CalcPurity(LibrarySpectrum.Name,
						LibrarySpectrum.SumId,
						ms2,
						LibrarySpectrum.PrecursorMz,
						LibrarySpectrum.Polarity,
						LibrarySpectrum.CompoundClass,
						LibrarySpectrum.AdductIon,
						faDB,
						LibrarySpectrum,
						isobaricIDs,
						mzTol);
			else Purity = 0;
		}
		return peakPurity;
	}

	//Compares identifications based on dot product
	public int CompareTo(Identification i)
	{
		if (DotProduct>i.DotProduct) return -1;
		else if (DotProduct<i.DotProduct) return 1;
		else return 0;
	}

	//Returns string representation of ID
	public string ToString()
	{
		return LibrarySpectrum.Name + " " + DotProduct;
	}
}