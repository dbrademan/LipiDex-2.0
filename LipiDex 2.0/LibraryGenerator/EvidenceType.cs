using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.LibraryGenerator
{
    /// <summary>
    /// Easy way to delineate possible levels of structural information a specific fragment ion could provide. Feel free to add more options as necessary. Just follow the bit-shifting pattern.
    /// </summary>
    internal enum EvidenceType
    {
        /// <summary>
        /// This fragment doesn't provide any species diagnostic information. Maybe it's too common to be considered, always present across many species, etc. 
        /// </summary>
        NonSpecific = 0,
        /// <summary>
        /// This fragment is indicative of lipid class.
        /// </summary>
        SpeciesLevel = 1 << 0,
        /// <summary>
        /// This fragment is indicative of fatty acid composition, but not position on the lipid backbone.
        /// </summary>
        MolecularSpeciesLevel = 1 << 1,
        /// <summary>
        /// This fragment is indicative of fatty acid composition AND position on the lipid backbone.
        /// </summary>
        SnPositionalIsomer = 1 << 2,
        /// <summary>
        /// This fragment is indicative of double bond position on a fatty acid chain.
        /// </summary>
        DoubleBondPosition = 1 << 3,
        /// <summary>
        /// This fragment is indicative of double bond orientation (cis,trans) on a fatty acid chain.
        /// </summary>
        DoubleBondOrientation = 1 << 4,
        /// <summary>
        /// Other categories. Keep adding more as appropriate. Just follow the bit-shifting pattern.
        /// </summary>
        Other = 1 << 5 
    }
}
