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
        
        public ObservableCollection<Backbone> DataGridBinding_Backbones = new ObservableCollection<Backbone>();
        public ObservableCollection<LipidClass> DataGridBinding_LipidClasses = new ObservableCollection<LipidClass>();
        private string libraryPath;

        public LibraryEditor(string libraryPath)
        {
            this.libraryPath = libraryPath;
            InitializeComponent();

            LoadFattyAcids(libraryPath);
            LoadLipidAdducts(libraryPath);
            LoadLipidClasses(libraryPath);
            //LoadLipidBackbones();
            //LoadFragmentationRules();
            //LoadLibraryGeneration();

            //DataContext = this;
        }

        #region Lipid Class Tab Controls

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

        #endregion


        #region Adduct Tab Controls

        public ObservableCollection<Adduct> DataGridBinding_Adducts = new ObservableCollection<Adduct>();

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

        #endregion

        // Completed 2022-11-21 DRB
        #region Fatty Acid Tab Controls

        // 
        /// <summary>
        /// Two-way binding between Library Editor's displayed fatty acid dataGrid and fatty acid observable collection
        /// </summary>
        public ObservableCollection<FattyAcid> DataGridBinding_FattyAcids = new ObservableCollection<FattyAcid>();

        /// <summary>
        /// Load fatty acids from the text-based LipiDex libraries stored in the [base]/Resources/LipidexLibraries directory
        /// </summary>
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

        /// <summary>
        /// Add a new fatty acid to the fatty acid data grid and bind a matching object to the fatty acid ObservableCollection&lt;FattyAcid&gt;
        /// </summary>
        private void Button_FattyAcids_Add_Click(object sender, RoutedEventArgs e)
        {
            var newFattyAcidName = "NewFa-0:0";
            var newFattyAcidFormula = "C1H2N3O4";
            var newFattyAcidType = "FA_Type";
            var newFattyAcidEnabled = "False";

            var newFattyAcidObject = new FattyAcid(newFattyAcidName, newFattyAcidType, newFattyAcidFormula, newFattyAcidEnabled);
            DataGridBinding_FattyAcids.Add(newFattyAcidObject);

            // force update of data grid? 
            DataGrid_FattyAcids.UpdateLayout();

            // programatically select newly added fatty acid row
            DataGrid_FattyAcids.SelectedItems.Clear();

            var newRowIndex = DataGridBinding_FattyAcids.Count - 1;
            var newSelectedRow = DataGrid_FattyAcids.Items[newRowIndex];
            DataGrid_FattyAcids.ScrollIntoView(newSelectedRow);
        }

        /// <summary>
        /// Remove a fatty acid from the fatty acid data grid and the corresponding object from the ObservableCollection&lt;FattyAcid&gt;
        /// </summary>
        private void Button_FattyAcids_Remove_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = DataGrid_FattyAcids.SelectedIndex;

            if (selectedRow != -1)
            {
                DataGridBinding_FattyAcids.RemoveAt(selectedRow);
            }
        }

        /// <summary>
        /// Enables all fatty acids in the fatty acid data grid.
        /// </summary>
        private void Button_FattyAcids_EnableAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var fattyAcid in DataGridBinding_FattyAcids)
            {
                fattyAcid.enabled = true;
            }

            DataGrid_FattyAcids.ItemsSource = new List<FattyAcid>();
            DataGrid_FattyAcids.ItemsSource = DataGridBinding_FattyAcids;
        }

        /// <summary>
        /// Disables all fatty acids in the fatty acid data grid.
        /// </summary>
        private void Button_FattyAcids_DisableAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (FattyAcid fattyAcid in DataGrid_FattyAcids.Items)
            {
                fattyAcid.enabled = false;
                
            }
            DataGrid_FattyAcids.ItemsSource = new List<FattyAcid>();
            DataGrid_FattyAcids.ItemsSource = DataGridBinding_FattyAcids;
        }

        /// <summary>
        /// Saves all fatty acids in the fatty acid data grid to the text-based LipiDex Libraries.
        /// </summary>
        private void Button_FattyAcids_SaveFattyAcids_Click(object sender, RoutedEventArgs e)
        {
            // default to false so we don't try writing out an empty library
            var validLibrary = false;
            for (var i = 0; i < DataGridBinding_FattyAcids.Count; i++)
            {
                var fattyAcid = DataGridBinding_FattyAcids[i];

                // if there is an invalid fatty acid, let validation message box be thrown and set all valid to false.
                // this will throw an invalid library for every single 
                if (fattyAcid.IsValid(i))
                {
                    validLibrary = true;
                }
                else
                {
                    validLibrary = false;
                }                
            }

            if (validLibrary)
            {
                SaveFattyAcidLibrary(libraryPath);

                var messageBoxQuery = string.Format("Library \"{0}\" has been successfully saved.\n\nOther open windows referencing \"{0}\" will not reflect these changes and should be reloaded!", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
                var messageBoxShortPrompt = "Library Saved Successfully!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Asterisk;

                MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                // just in case there's weird shenanigans in my code, reload the fatty acids from the file to make sure there are no
                // weird artifacts left over from the data grid
                LoadFattyAcids(libraryPath);
            }
            else
            {
                var messageBoxQuery = string.Format("Formatting error(s) detected with fatty acid entries in library \"{0}\". They must be corrected before this library can be saved.", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
                var messageBoxShortPrompt = "Library Save Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var results = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        /// <summary>
        /// Effectively discards all changes since the library was last saved. Clears the ObservableCollection&lt;FattyAcid&gt; and repopulates it from the text-based LipiDex Libraries
        /// </summary>
        private void Button_FattyAcids_ReloadOldFattyAcids_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxQuery = string.Format("Reload library \"{0}\" from last-saved version? Any unsaved edits will be lost.", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
            var messageBoxShortPrompt = "Reloading last saved version of library!";
            var messageBoxButtonOptions = MessageBoxButton.YesNoCancel;
            var messageBoxImage = MessageBoxImage.Question;

            var results = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

            if (results.Equals(MessageBoxResult.Yes))
            {
                LoadFattyAcids(this.libraryPath);
            }
            
        }

        /// <summary>
        /// NOT USED: Event fired when user clicks into the data grid to editing a fatty acid property.
        /// </summary>
        private void DataGrid_FattyAcid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            //DataGridBinding_FattyAcids[DataGrid_FattyAcids.SelectedIndex].isDirty = true;
        }

        /// <summary>
        /// Event fired when user clicks off a data grid after editing a fatty acid property. Validates column edits and rolls back changes if edits are invalid
        /// </summary>
        private void DataGrid_FattyAcids_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {            
            switch (e.Column.DisplayIndex)
            {
                // fatty acid name
                case 0:
                    var index = e.Row.GetIndex();
                    var editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_FattyAcids[index].ValidateFattyAcidName(editedTextBox.Text, index);
                    break;

                // fatty acid type
                case 1:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_FattyAcids[index].ValidateFattyAcidType(editedTextBox.Text, e.Row.GetIndex());
                    break;

                // fatty acid formula
                case 2:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    // if the formula is fine, update the internal object's chemicalFormula
                    // need to do this as the internal ChemicalFormula Obj will not automatically update from text
                    DataGridBinding_FattyAcids[index].ValidateFattyAcidFormula(editedTextBox.Text, e.Row.GetIndex());

                    break;

                // fatty acid enabled/disabled
                case 3:
                    break;
                    // don't need a check for enabled/disabled since it's boolean. It will always be valid.
            }            
        }

        /// <summary>
        /// Event which generates row numbers in data grid using an object's index in the ObservableCollection&lt;FattyAcid&gt;
        /// </summary>
        private void DataGrid_FattyAcids_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // show row number
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>
        /// Save fatty acids from the text-based LipiDex libraries stored in the [base]/Resources/LipidexLibraries directory
        /// </summary>
        private void SaveFattyAcidLibrary(string libraryBasePath)
        {
            var fattyAcidPath = System.IO.Path.Combine(libraryBasePath, "FattyAcids.csv");

            try
            {
                var writer = new StreamWriter(fattyAcidPath);

                // write txt file headers
                writer.WriteLine(string.Format("{0},{1},{2},{3}", "Name", "Base", "Formula", "Enabled"));
                foreach (var fattyAcid in DataGridBinding_FattyAcids)
                {
                    writer.WriteLine(fattyAcid.SaveString());
                }

                writer.Close();
                writer.Dispose();
            }
            catch (Exception e)
            {
                var messageBoxQuery = e.Message;
                var messageBoxShortPrompt = "Fatty Acid Template Saving Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        #endregion 

        #region Backbone Tab Controls

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

        #endregion
    }
}
