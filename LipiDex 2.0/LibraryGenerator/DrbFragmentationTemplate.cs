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

namespace LipiDex_2._0.LibraryGenerator
{
    internal class DrbFragmentationTemplate
    {

        public LipidClass lipidClass;




        public string LipidClass_AdductCombo { get; set; }

        // always set
        public ObservableCollection<TestTreeViewObject> Children { get; set; }


        // only set if lower level
        public ChemicalFormula formulaShift { get; set; }   //ChemicalFormula fragment shift
        public double massShift { get; set; }               //Mass shift if formula not defined
        public double intensity { get; set; }               //Relative intensity of fragment, scaled to 999
        public string type { get; set; }                 //Type of transition



        public DrbFragmentationTemplate(string name)
        {
            this.LipidClass_AdductCombo = name;
            this.Children = new ObservableCollection<TestTreeViewObject>();

            this.formulaShift = new ChemicalFormula();
            this.massShift = -1;
            this.intensity = -1;
            this.type = null;
        }

        //Constructor
        public DrbFragmentationTemplate(double mass, double intensity, string type)
        {
            this.LipidClass_AdductCombo = null;
            this.Children = new ObservableCollection<TestTreeViewObject>();

            this.formulaShift = new ChemicalFormula();
            this.massShift = mass;
            this.intensity = intensity;
            this.type = type;
        }

        public DrbFragmentationTemplate(string formulaShiftString, int intensity, string type)
        {
            this.LipidClass_AdductCombo = null;
            this.Children = new ObservableCollection<TestTreeViewObject>();

            this.formulaShift = new ChemicalFormula(formulaShiftString);
            this.massShift = formulaShift.MonoisotopicMass;
            this.intensity = intensity;
            this.type = type;
        }

        public DrbFragmentationTemplate(ChemicalFormula formulaShift, double intensity, string type)
        {
            this.LipidClass_AdductCombo = null;
            this.Children = new ObservableCollection<TestTreeViewObject>();

            this.formulaShift = formulaShift;
            this.massShift = formulaShift.MonoisotopicMass;
            this.intensity = intensity;
            this.type = type;
        }

        //Returns mass
        public double GetMass()
        {
            return this.massShift;
        }

        //Returns type
        public new string GetType()
        {
            return this.type;
        }

        //Returns intensity
        public double GetIntensity()
        {
            return this.intensity;
        }

        public ChemicalFormula GetFormulaShift()
        {
            return this.formulaShift;
        }

        //Return string representation of transition
        public override string ToString()
        {
            return string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\"", this.formulaShift.ToString(), this.massShift, this.intensity, this.type);
        }

        public static ObservableCollection<TestTreeViewObject> GenerateSampleData()
        {
            var exampleTreeViewObjects = new ObservableCollection<TestTreeViewObject>();

            TestTreeViewObject object1 = new TestTreeViewObject("PC [M+H]+");
            object1.Children.Add(new TestTreeViewObject("Choline Headgroup", 100, "1      | Phosphocholine Headgroup                  | H2PO4C2H4N(CH3)3              | 999       | 1      | Fragment                          | Species Level              | HCD  | 2   |        | "));
            object1.Children.Add(new TestTreeViewObject("CH3CH2CH3", 150, "An Important Fragment"));
            object1.Children.Add(new TestTreeViewObject(184.079, 999, "Choline Headgroup"));

            exampleTreeViewObjects.Add(object1);

            TestTreeViewObject object2 = new TestTreeViewObject("Lipid Template 2");
            object2.Children.Add(new TestTreeViewObject("O-1", 420, "Fragment 2"));
            object2.Children.Add(new TestTreeViewObject("C-2H-9O-4N-1P-1", 900, "An Important Fragment"));
            object2.Children.Add(new TestTreeViewObject(184.079, 22, "Choline Headgroup"));

            exampleTreeViewObjects.Add(object2);

            return exampleTreeViewObjects;
        }
    }
}
