using CSMSL.Chemistry;
using CSMSL.Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data.FilterEnums;

namespace LipiDex_2._0.LibraryGenerator
{
    internal class DrbFragmentationRule
    {
        /// <summary>
        /// Identifier used to link related MSⁿ transitions together if using serial activations. Procedurally generated.
        /// </summary>
        public int id;
        /// <summary>
        /// Describes the absolute mass shift of a fragmentation event or neutral loss. Additionally, this can be used as a mass balancer to properly describe fragmentation patterns.
        /// </summary>
        public double massBalancer;
        /// <summary>
        /// Describes the absolute ChemicalFormula shift of a fragmentation event or neutral loss.
        /// </summary>
        public ChemicalFormula formulaBalancer;
        /// <summary>
        /// The relative intensity of a fragment ion (0-1000).
        /// </summary>
        public double intensity;
        /// <summary>
        /// Charge of the fragment ion.
        /// </summary>
        public int charge;
        /// <summary>
        /// Fragmentation template type. Examples: Fragment, Neutral Loss, Fatty Acid, Other Variable Motifs
        /// </summary>
        public string transitionType;
        /// <summary>
        /// Boolean which determines charge localization, on the defined fragment or not.
        /// </summary>
        public bool isFragment;
        /// <summary>
        /// A descriptive category which suggests what structural information this fragment ion can provide. Likely won't be fully leveraged yet, but in principle this could be used in the future.
        /// </summary>
        public EvidenceType evidenceType;
        /// <summary>
        /// A descriptive category which describes the activation type used to fragment the lipid precursor. Uses CSMSL.Proteomics activation types. Can be used to filter spectra in Sectrum Searcher
        /// </summary>
        public ActivationType activationType;
        /// <summary>
        /// A descriptive category which describes the MSn order required to produce this transition. Can be used to filter spectra in Sectrum Searcher
        /// </summary>
        public int msnOrder;
        /// <summary>
        /// A linker variable that can be used to construct MSn trees in Spectrum Searcher and Peak Finder. Likely won't be fully leveraged yet, but in principle this could be used in the future.
        /// </summary>
        public int parentTransition;

    }
}
