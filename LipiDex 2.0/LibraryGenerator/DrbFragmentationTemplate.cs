using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CSMSL;
using CSMSL.Chemistry;
using CSMSL.Proteomics;
using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data.FilterEnums;

namespace LipiDex_2._0.LibraryGenerator
{
    public class DrbFragmentationTemplate
    {

        private LipidClass lipidClass;
        private Adduct adduct;

        public string LipidClass_AdductCombo { get; set; }
        
        public ObservableCollection<DrbFragmentationRule> fragmentationRules;

        public DrbFragmentationTemplate(LipidClass lipidClass, Adduct adduct)
        {
            this.lipidClass = lipidClass;
            this.adduct = adduct;
            this.fragmentationRules = new ObservableCollection<DrbFragmentationRule>();
            this.LipidClass_AdductCombo = string.Format("{0} {1}", this.lipidClass.name, this.adduct.name);
        }

        public EvidenceType ParseEvidenceType(string evidenceString)
        {
            if (evidenceString.Equals("Nonspecific"))
            {
                return EvidenceType.NonSpecific;
            }
            else if (evidenceString.Equals("Species Level"))
            {
                return EvidenceType.SpeciesLevel;
            }
            else if (evidenceString.Equals("Molecular Species Level"))
            {
                return EvidenceType.MolecularSpeciesLevel;
            }
            else if (evidenceString.Equals("Sn Positional Isomer"))
            {
                return EvidenceType.SnPositionalIsomer;
            }
            else if (evidenceString.Equals("Double Bond Position"))
            {
                return EvidenceType.DoubleBondPosition;
            }
            else if (evidenceString.Equals("Double Bond Orientation"))
            {
                return EvidenceType.DoubleBondOrientation;
            }
            else if (evidenceString.Equals("Other"))
            {
                return EvidenceType.Other;
            }
            else
            {
                throw new ArgumentException("`" + evidenceString + "` is not a known EvidenceType currently. You'll need to add parsing logic to the codebase.");
            }
        }

        public ActivationType ParseActivationType(string activationString)
        {
            if (activationString.Equals("HCD"))
            {
                return ActivationType.HigherEnergyCollisionalDissociation;
            }
            else if (activationString.Equals("CID"))
            {
                return ActivationType.CollisionInducedDissociation;
            }
            else if (activationString.Equals("UVPD"))
            {
                return ActivationType.UltraVioletPhotoDissociation;
            }
            else
            {
                throw new ArgumentException("`" + activationString + "` is not a known EvidenceType currently. You'll need to add parsing logic to the codebase.");

            }
        }
    }
}
