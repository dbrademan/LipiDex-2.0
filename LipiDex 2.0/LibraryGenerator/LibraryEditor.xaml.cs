using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace LipiDex_2._0.LibraryGenerator
{
    /// <summary>
    /// Interaction logic for LibraryEditor.xaml
    /// </summary>
    public partial class LibraryEditor : Window
    {
        public ObservableCollection<FattyAcid> DataGridBinding_FattyAcids = new ObservableCollection<FattyAcid>();
        public ObservableCollection<Adduct> DataGridBinding_Adducts = new ObservableCollection<Adduct>();
        public ObservableCollection<Backbone> DataGridBinding_Backbones = new ObservableCollection<Backbone>();
        public ObservableCollection<LipidClass> DataGridBinding_LipidClasses = new ObservableCollection<LipidClass>();

        //lock object for synchronization
        private static object _syncLock = new object();

        public LibraryEditor(string libraryPath)
        {
            InitializeComponent();

            LoadFattyAcids(libraryPath);
            LoadLipidAdducts(libraryPath);
            //LoadLipidBackbones

            LoadLipidClasses(libraryPath);
            //LoadFragmentationRules();
            //LoadLibraryGeneration();

            // Enable cross-thread access to all datagrids.
            // Needed to modulate entries during execution with current implementation
            BindingOperations.EnableCollectionSynchronization(DataGridBinding_FattyAcids, _syncLock);

            DataContext = this;
        }

        private void LoadFattyAcids(string libraryBasePath)
        {
            var fattyAcidPath = System.IO.Path.Combine(libraryBasePath, "FattyAcids.csv");

            try
            {
                var reader = new CsvReader(new StreamReader(fattyAcidPath), true);
                this.DataGridBinding_FattyAcids = new ObservableCollection<FattyAcid>();

                while (reader.ReadNextRecord())
                {
                    string name = reader["Name"];
                    string type = reader["Base"];
                    string chemicalFormulaString = reader["Formula"];
                    string enabled = reader["Enabled"];

                    this.DataGridBinding_FattyAcids.Add(new FattyAcid(name, type, chemicalFormulaString, enabled)); 
                }

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
                this.DataGridBinding_Adducts = new ObservableCollection<Adduct>();

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

        private void DataGrid_FattyAcids_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        private void Button_FattyAcids_Add_Click(object sender, RoutedEventArgs e)
        {
            var newFattyAcidName = "NewFa-0:0";
            var newFattyAcidFormula = "C1H2N3O4";
            var newFattyAcidType = "FA_Type";
            var newFattyAcidEnabled = "False";

            var newFattyAcidObject = new FattyAcid(newFattyAcidName, newFattyAcidType, newFattyAcidFormula, newFattyAcidEnabled);
            DataGridBinding_FattyAcids.Add(newFattyAcidObject);
            DataGrid_FattyAcids.SelectedItem = newFattyAcidEnabled;
        }

        private void Button_FattyAcids_Remove_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = DataGrid_FattyAcids.SelectedIndex;

            DataGridBinding_FattyAcids.RemoveAt(selectedRow);
        }

        private void Button_FattyAcids_EnableAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var fattyAcid in DataGridBinding_FattyAcids)
            {
                fattyAcid.enabled = true;
            }

            DataGrid_FattyAcids.ItemsSource = new List<FattyAcid>();
            DataGrid_FattyAcids.ItemsSource = DataGridBinding_FattyAcids;
        }

        private void Button_FattyAcids_DisableAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (FattyAcid fattyAcid in DataGrid_FattyAcids.Items)
            {
                fattyAcid.enabled = false;
                
            }
            DataGrid_FattyAcids.ItemsSource = new List<FattyAcid>();
            DataGrid_FattyAcids.ItemsSource = DataGridBinding_FattyAcids;
        }

        private void Button_FattyAcids_SaveFattyAcids_Click(object sender, RoutedEventArgs e)
        {
            // first, if any table entries are considered dirty, they should be validated and committed to objects
            CleanDataGrid();
        }

        private void CleanDataGrid()
        {

            //ObservableCollection.
        }

        private void Button_FattyAcids_ReloadOldFattyAcids_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
