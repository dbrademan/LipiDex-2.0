using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using csgoslin; 

namespace LipiDex_2._0.Utils
{
    public class LipidParser
    {
        /* 
        Constructor will read in lipid as string and parse out all parts of the lipid:
        - Lipid class
        - Lipid superclass according to LipidGenie (Glycero, Phospho, Sphingo, Sterol, Fatty acyl)
        - FAs list
        - isSumComp
        - Number of FA carbons
        - Number of FA unsaturations
        - Headgroup mass
        - FA masses

        Maybe it will have getters/setters for each attribute, or can access directly
        
        */ 
        public string lipidString;
        public string faString;
        public string[] split;
        public bool isFattyAcidLipid;     //Defined according to Paul's original definition
        public string lipidClass;
        public string lipidSuperclass;
        public string lipidSubclass;      //LipidMaps may have certain subclasses
        public int numFAs;                //Number of fatty acids in this class
        public List<List<int>> FAs;
        public bool isSumComp;            //For classes with 2 or more FAs, true if only sum composition known
        public List<int> sumComp;         //sumComp representation of FAs, List(numFACarbons, numFAUnsat)
                                          //    Should this be null if not sum comp, or always populated? 
                                          //    Should sumComp be populated for Free Fatty Acids?
        public int numFACarbons;          //Total number of carbons in all fatty acyls
        public int numFAUnsat;            //Total number of unsaturations in all fatty acyls
                                          //    How is this defined for sphingomyelin backbones, for example?
        public int numOx;                 //Total number of oxidations according to most recent LipidMaps nomenclature
        public string prefixThing;        //For example, sphingomyelins can have d37:1 or t18:0_16:2, d and t are the prefixThing
                                          //    This may be unncessary with new LipidMaps definitions
        public bool isUnsaturated;        //Whether FAs have any unsaturations
                                          //    Do sphingomyelins with backbone unsaturation count? 
        public bool isHeavyLabeled;       //For heavy-labeled standards

        public LipidParser(string lipidString, bool isHeavyLabeled = false)
        {
            this.lipidString = lipidString;

            split = lipidString.Split(' ');

            //Under nomenclature rules, the lipid class headgroup is always split by a space
            lipidClass = split[0];

            //FAs can be split by _ or / depending on sn-position isomer knowledge
            faString = split[1];
            //faSplit = split[1].Split('_').Split('/');  // TODO: split within each 



        }

        

        //public List<string> ParseString()
        //{
        //    return this.lipidString;
        //}

        
        public enum NumFAInClass
        {
            CL = 4,

            TG = 3,
            DG = 2,
            MG = 1,

            PA = 2,
            PC = 2,
            PE = 2,
            PG = 2,
            PI = 2,
            PS = 2,

            LPA = 1,
            LPC = 1,
            LPE = 1,
            LPG = 1,
            LPI = 1,
            LPS = 1,

            SM = 2,
        }
    }

}

