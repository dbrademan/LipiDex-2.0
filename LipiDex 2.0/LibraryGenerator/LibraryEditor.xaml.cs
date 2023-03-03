using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using CSMSL.Chemistry;

namespace LipiDex2.LibraryGenerator
{
    /// <summary>
    /// Interaction logic for LibraryEditor.xaml
    /// </summary>
    public partial class LibraryEditor : Window
    {
        public List<FattyAcid> DataGridBinding_FattyAcids = new List<FattyAcid>();
        public List<Adduct> DataGridBinding_Adducts = new List<Adduct>();
        public List<Backbone> DataGridBinding_Backbones = new List<Backbone>();
        public List<LipidClass> DataGridBinding_LipidClasses = new List<LipidClass>();
        
        public LibraryEditor()
        {
            InitializeComponent();
        }

        public LibraryEditor(string libraryPath)
        {
            InitializeComponent();
            LoadFattyAcids(libraryPath);
            LoadLipidAdducts(libraryPath);
            //LoadLipidBackbones

            LoadLipidClasses(libraryPath);
            //LoadFragmentationRules();
            //LoadLibraryGeneration();
            DataContext = this;
        }

        private void LoadFattyAcids(string libraryBasePath)
        {
            var fattyAcidPath = System.IO.Path.Combine(libraryBasePath, "FattyAcids.csv");

            try
            {
                var reader = new CsvReader(new StreamReader(fattyAcidPath), true);
                this.DataGridBinding_FattyAcids = new List<FattyAcid>();

                while (reader.ReadNextRecord())
                {
                    string name = reader["Name"];
                    string type = reader["Base"];
                    string chemicalFormulaString = reader["Formula"];
                    string enabled = reader["Enabled"];

                    this.DataGridBinding_FattyAcids.Add(new FattyAcid(name, type, chemicalFormulaString, enabled)); 
                }

                // try refreshing data grid bindings. Don't know why this doesn't work...
                DataGrid_FattyAcids.ItemsSource = this.DataGridBinding_FattyAcids;
            }
            catch (Exception e)
            {
                var messageBoxQuery = e.Message;
                var messageBoxShortPrompt = "Fatty Acid Template Loading Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        private void LoadLipidAdducts(string libraryBasePath)
        {
            var adductPath = System.IO.Path.Combine(libraryBasePath, "Adducts.csv");

            try
            {
                var reader = new CsvReader(new StreamReader(adductPath), true);
                this.DataGridBinding_Adducts = new List<Adduct>();

                while (reader.ReadNextRecord())
                {
                    string name = reader["Name"];
                    string chemicalFormulaString = reader["Formula"];
                    string isNeutralLoss = reader["Loss"];
                    string polarity = reader["Polarity"];
                    string charge = reader["Charge"];

                    this.DataGridBinding_Adducts.Add(new Adduct(name, chemicalFormulaString, isNeutralLoss, polarity, charge));
                }
            }
            catch (Exception e)
            {
                var messageBoxQuery = e.Message;
                var messageBoxShortPrompt = "Adduct Template Loading Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        private void LoadLipidClasses(string libraryBasePath)
        {
            var lipidClassPath = System.IO.Path.Combine(libraryBasePath, "Lipid_Classes.csv");

            try
            {
                var reader = new CsvReader(new StreamReader(lipidClassPath), true);
                var lipidClasses = new List<LipidClass>();

                while (reader.ReadNextRecord())
                {
                    string className = reader["Name"];
                    string classAbbreviation = reader["Abbreviation"];
                    ChemicalFormula headGroup = new ChemicalFormula(reader["HeadGroup"]);
                    List<string> Adducts = reader["Adducts"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    bool isSterol = BackboneBooleanConverter(reader["Sterol"]);
                    bool isGlycerol = BackboneBooleanConverter(reader["Glycerol"]);
                    bool isSphingoid = BackboneBooleanConverter(reader["Sphingoid"]);
                    string parsedBackboneString = reader["Backbone"];

                    ChemicalFormula backboneFormula = null;

                    if (string.IsNullOrWhiteSpace(parsedBackboneString))
                    {
                        backboneFormula = GetBackboneFormula(isSterol, isGlycerol, isSphingoid);
                    }
                    else
                    {
                        backboneFormula = GetBackboneFormula(isSterol, isGlycerol, isSphingoid, parsedBackboneString);
                    }

                    int numFattyAcids = Convert.ToInt32(reader["numFattyAcids"]);
                    string optimalPolarity = reader["OptimalPolarity"];
                    List<string> fattyAcids = new List<string>(numFattyAcids);

                    for (var i = 1; i < numFattyAcids + 1; i++)
                    {
                        var fattyAcidColumnHeader = "FA" + i;
                        fattyAcids.Add(reader[fattyAcidColumnHeader]);
                    }

                    //lipidClasses.Add(new LipidClass(className, classAbbreviation, headGroup, ))
                }
            }
            catch (Exception e)
            {
                var messageBoxQuery = e.Message;
                var messageBoxShortPrompt = "Lipid Class Loading Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        private ChemicalFormula GetBackboneFormula(bool isSterol, bool isGlycerol, bool isSphingoid, string nonStandardBackbone = null)
        {
            if (isSterol)
            {
                return new ChemicalFormula();
            }
            else if (isGlycerol)
            {
                return new ChemicalFormula();
            } 
            else if (isSphingoid)
            {
                return new ChemicalFormula();
            }
            else if (nonStandardBackbone != null)
            {
                return GetBackboneFormula(nonStandardBackbone);
            }
            else
            {
                throw new ArgumentException("No lipid backbone supplied. Check the library templates and try again.");
            }
        }

        private ChemicalFormula GetBackboneFormula(string nonStandardBackbone)
        {
            return new ChemicalFormula(nonStandardBackbone);
        }

        private bool BackboneBooleanConverter(string booleanString)
        {
            if (booleanString.Equals("TRUE"))
            {
                return true;
            }
            else if (booleanString.Equals("FALSE"))
            {
                return false;
            }
            else
            {
                throw new ArgumentException("Boolean evaluation of Lipid_Classes.csv library column {0} failed. Check the csv and make sure formatting is correct.");
            }
        }
    }
}
