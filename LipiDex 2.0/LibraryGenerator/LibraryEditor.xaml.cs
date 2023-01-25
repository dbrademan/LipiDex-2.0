using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private string libraryPath;
        private static Color addedRowColor = (Color)ColorConverter.ConvertFromString("#9debdf");

        public LibraryEditor(string libraryPath)
        {
            this.libraryPath = libraryPath;
            InitializeComponent();

            // check to see if library is old format.
            // convert it if it is
            // TODO - CONVERT LIBRARY TEMPLATE FILES TO NEW FORMAT IF NECESSARY
            LoadFattyAcids(libraryPath);
            LoadLipidAdducts(libraryPath);
            LoadLipidBackbones(libraryPath);
            LoadLipidClasses(libraryPath);
            //
            //LoadFragmentationRules();
            //LoadLibraryGeneration();

            //DataContext = this;
        }

        // Completed 2022-01-19 DRB
        #region Adduct Tab Controls

        /// <summary>
        /// Stores two-way bound Adduct objects to the Library Generator DataGrid - Adduct tab.
        /// </summary>
        public ObservableCollection<Adduct> DataGridBinding_Adducts = new ObservableCollection<Adduct>();

        /// <summary>
		/// Loads all lipid adducts from the specified library (libraryBasePath).
		/// </summary>
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

                DataGrid_Adducts.ItemsSource = this.DataGridBinding_Adducts;
            }
            catch (Exception e)
            {
                var messageBoxQuery = e.Message;
                var messageBoxShortPrompt = "Lipid Adduct Template Loading Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        /// <summary>
        /// Add a new lipid adduct to the lipid adduct grid and bind a matching object to the adduct ObservableCollection&lt;Adduct&gt;<br/>Adds brief color highlight when row is added
        /// </summary>
        private void Button_Adducts_Add_Click(object sender, RoutedEventArgs e)
        {
            var newAdductName = "NewLipidAdduct";
            var newAdductFormula = "C1H2N3O4";
            var newAdductIsLoss = "false";
            var newAdductPolarity = "+";
            var newAdductCharge = "1";

            var newAdductObject = new Adduct(newAdductName, newAdductFormula, newAdductIsLoss, newAdductPolarity, newAdductCharge);
            DataGridBinding_Adducts.Add(newAdductObject);

            // force update of data grid? 
            DataGrid_Adducts.UpdateLayout();

            // programatically select newly added adduct row
            DataGrid_Adducts.SelectedItems.Clear();

            var newRowIndex = DataGridBinding_Adducts.Count - 1;
            var newSelectedRow = DataGrid_Adducts.Items[newRowIndex];
            DataGrid_Adducts.ScrollIntoView(newSelectedRow);

            // mark a color animation transition for added row
            DataGridRow row = (DataGridRow)DataGrid_Adducts.ItemContainerGenerator.ContainerFromIndex(newRowIndex);
            Color startColor = LibraryEditor.addedRowColor;
            Color endColor = Colors.White;

            ColorAnimation ca = new ColorAnimation(endColor, new Duration(TimeSpan.FromSeconds(0.5)));
            row.Background = new SolidColorBrush(startColor);
            row.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }

        /// <summary>
        /// Remove an adduct from the adduct data grid and the corresponding object from the ObservableCollection&lt;adduct&gt;
        /// </summary>
        private void Button_Adducts_Remove_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = DataGrid_Adducts.SelectedIndex;

            if (selectedRow != -1)
            {
                DataGridBinding_Adducts.RemoveAt(selectedRow);
            }
        }

        /// <summary>
        /// Saves all adducts in the adduct data grid to the text-based LipiDex library .csv file.
        /// </summary>
        private void Button_Adducts_SaveAdducts_Click(object sender, RoutedEventArgs e)
        {
            // default to false so we don't try writing out an empty library
            var validLibrary = false;

            for (var i = 0; i < DataGridBinding_Adducts.Count; i++)
            {
                var adduct = DataGridBinding_Adducts[i];

                // if there is an invalid adduct, let validation message box be thrown and set all valid to false.
                // this will break on 
                if (adduct.IsValid(i))
                {
                    validLibrary = true;
                }
                else
                {
                    // bad adduct
                    // break iteration and send popup alert
                    validLibrary = false;
                    break;
                }
            }

            if (validLibrary)
            {
                try
                {
                    SaveAdductLibrary(libraryPath);

                    var messageBoxQuery = string.Format("Library \"{0}\" has been successfully saved.\n\nOther open windows referencing \"{0}\" will not reflect these changes and should be reloaded!", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
                    var messageBoxShortPrompt = "Library Saved Successfully!";
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Asterisk;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                    // just in case there's weird shenanigans in my code, reload the lipid adducts from the file to make sure there are no
                    // weird artifacts left over from the data grid
                    LoadLipidAdducts(libraryPath);
                }
                catch (InvalidOperationException exception)
                {
                    var messageBoxQuery = string.Format("Unexpected error while saving changes to library \"{0}\".All edits have been rolled back!\n\nError Message:\n{1}", System.IO.Path.GetFileNameWithoutExtension(libraryPath), exception.Message);
                    var messageBoxShortPrompt = "Error - Changes Not Saved!";
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                }
            }
            else
            {
                var messageBoxQuery = string.Format("Formatting error(s) detected with adduct entries in library \"{0}\". They must be corrected before this library can be saved.", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
                var messageBoxShortPrompt = "Library Formatting Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var results = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        /// <summary>
        /// - Saves all changes made to the lipid adduct library stored in the [base]/Resources/LipidexLibraries directory if all changes are valid.
        /// <br/>
        /// - If an error occurs during save, rolls back library changes to original
        /// <br/>
        /// - Clears the ObservableCollection&lt;Adduct&gt; and repopulates it from the text-based LipiDex libraries.
        /// </summary>
        private void SaveAdductLibrary(string libraryBasePath)
        {
            var adductLibraryPath = System.IO.Path.Combine(libraryBasePath, "Adducts.csv");
            var backupAdductLibraryPath = System.IO.Path.Combine(libraryBasePath, "Adducts_tmpBackup.csv");
            
            try
            {
                // first, back up old version of the adduct library
                File.Copy(adductLibraryPath, backupAdductLibraryPath);

                // overwrite old library version
                var writer = new StreamWriter(adductLibraryPath);

                // write txt file headers
                writer.WriteLine(string.Format("{0},{1},{2},{3},{4}", "Name", "Formula", "Loss", "Polarity", "Charge"));

                // write out each adduct
                foreach (var adduct in DataGridBinding_Adducts)
                {
                    writer.WriteLine(adduct.SaveString());
                }

                writer.Close();
                writer.Dispose();

                // successfully wrote library
                // delete backup copy of old library.
                File.Delete(backupAdductLibraryPath);
            }
            catch (Exception e)
            {
                // something about updating the new library failed.
                // restore old library
                File.Copy(backupAdductLibraryPath, adductLibraryPath, true);

                // delete backup copy
                File.Delete(backupAdductLibraryPath);

                // throw exception back to upper level
                throw new InvalidOperationException(e.Message);
            }
        }

        /// <summary>
        /// Effectively discards all changes since the library was last saved. Clears the ObservableCollection&lt;Adduct&gt; and repopulates it from the text-based LipiDex Libraries
        /// </summary>
        private void Button_Adducts_ReloadOldAdducts_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxQuery = string.Format("Reload library \"{0}\" from last-saved version? Any unsaved edits will be lost.", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
            var messageBoxShortPrompt = "Reloading last saved version of library!";
            var messageBoxButtonOptions = MessageBoxButton.YesNoCancel;
            var messageBoxImage = MessageBoxImage.Question;

            var results = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

            if (results.Equals(MessageBoxResult.Yes))
            {
                LoadLipidAdducts(this.libraryPath);
            }

        }

        /// <summary>
        /// Event which generates row numbers in data grid using an object's index in the ObservableCollection&lt;Adduct&gt;.
        /// </summary>
        private void DataGrid_Adducts_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row;

            // show row number
            row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>
        /// Validates a cell which just finished editing.
        /// </summary>
        private void DataGrid_Adducts_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // validate the individual edited cell
            switch (e.Column.DisplayIndex)
            {
                // adduct name
                case 0:
                    var index = e.Row.GetIndex();
                    var editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_Adducts[index].ValidateAdductName(editedTextBox.Text, index);
                    break;

                // adduct formula
                case 1:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_Adducts[index].ValidateAdductFormula(editedTextBox.Text, e.Row.GetIndex());
                    break;

                // adduct loss
                case 2:
                    break;
                    // don't need a check for enabled/disabled since it's boolean. It will always be valid.

                // adduct polarity
                case 3:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_Adducts[index].ValidateAdductPolarity(editedTextBox.Text, e.Row.GetIndex());
                    break;
                // don't need a check for enabled/disabled since it's boolean. It will always be valid.

                // adduct charge
                case 4:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    
                    if (DataGridBinding_Adducts[index].ValidateAdductCharge(editedTextBox.Text, e.Row.GetIndex()))
                    {
                        editedTextBox.Text = DataGridBinding_Adducts[index].charge;
                    }
                    
                    break;
            }
        }

        private void DataGrid_Adducts_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {

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
        /// Add a new fatty acid to the fatty acid data grid and bind a matching object to the fatty acid ObservableCollection&lt;FattyAcid&gt;<br/>Adds brief color highlight when row is added
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

            // mark a color animation transition for added row
            DataGridRow row = (DataGridRow)DataGrid_FattyAcids.ItemContainerGenerator.ContainerFromIndex(newRowIndex);
            Color startColor = LibraryEditor.addedRowColor;
            Color endColor = Colors.White;

            ColorAnimation ca = new ColorAnimation(endColor, new Duration(TimeSpan.FromSeconds(0.5)));
            row.Background = new SolidColorBrush(startColor);
            row.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
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
                    // bad fatty acid
                    // break iteration and send popup alert
                    validLibrary = false;
                    break;
                }                
            }
            
            if (validLibrary)
            {
                try
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
                catch (InvalidOperationException exception)
                {
                    var messageBoxQuery = string.Format("Unexpected error while saving changes to library \"{0}\".All edits have been rolled back!\n\nError Message:\n{1}", System.IO.Path.GetFileNameWithoutExtension(libraryPath), exception.Message);
                    var messageBoxShortPrompt = "Error - Changes Not Saved!";
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                }
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
        /// Event which generates row numbers in data grid using an object's index in the ObservableCollection&lt;FattyAcid&gt;. Also add color highlight when row is added
        /// </summary>
        private void DataGrid_FattyAcids_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row;

            // show row number
            row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>
        /// - Saves all changes made to the fatty acid library stored in the [base]/Resources/LipidexLibraries directory if all changes are valid.
        /// <br/>
        /// - If an error occurs during save, rolls back library changes to original
        /// <br/>
        /// - Clears the ObservableCollection&lt;FattyAcid&gt; and repopulates it from the text-based LipiDex libraries.
        /// </summary>
        private void SaveFattyAcidLibrary(string libraryBasePath)
        {
            var fattyAcidPath = System.IO.Path.Combine(libraryBasePath, "FattyAcids.csv");
            var backupFattyAcidPath = System.IO.Path.Combine(libraryBasePath, "FattyAcids_tmpBackup.csv");

            try
            {
                // first, back up old version of the adduct library
                File.Copy(fattyAcidPath, backupFattyAcidPath);

                // overwrite old library version
                var writer = new StreamWriter(fattyAcidPath);

                
                // write txt file headers
                writer.WriteLine(string.Format("{0},{1},{2},{3}", "Name", "Base", "Formula", "Enabled"));
                foreach (var fattyAcid in DataGridBinding_FattyAcids)
                {
                    writer.WriteLine(fattyAcid.SaveString());
                }

                writer.Close();
                writer.Dispose();

                // successfully wrote library
                // delete backup copy of old library.
                File.Delete(backupFattyAcidPath);
            }
            catch (Exception e)
            {
                // something about updating the new library failed.
                // restore old library
                File.Copy(backupFattyAcidPath, backupFattyAcidPath, true);

                // delete backup copy
                File.Delete(backupFattyAcidPath);

                // throw exception back to upper level
                throw new InvalidOperationException(e.Message);
            }
        }

        #endregion 

        // Completed 2022-01-19 DRB
        #region Backbone Tab Controls

        /// <summary>
        /// Stores two-way bound LipidBackbone objects to the Library Generator DataGrid - Backbone tab.
        /// </summary>
        public ObservableCollection<LipidBackbone> DataGridBinding_Backbones = new ObservableCollection<LipidBackbone>();

        /// <summary>
		/// Loads all lipid backbones from the specified library (libraryBasePath).
		/// </summary>
        private void LoadLipidBackbones(string libraryBasePath)
        {
            var lipidBackbonePath = System.IO.Path.Combine(libraryBasePath, "Backbones.csv");

            try
            {
                var reader = new CsvReader(new StreamReader(lipidBackbonePath), true);
                this.DataGridBinding_Backbones = new ObservableCollection<LipidBackbone>();

                while (reader.ReadNextRecord())
                {
                    string name = reader["Name"];
                    string chemicalFormulaString = reader["Formula"];
                    string numberOfMoieties = reader["Number of Moieties"];

                    this.DataGridBinding_Backbones.Add(new LipidBackbone(name, chemicalFormulaString, numberOfMoieties));
                }

                DataGrid_Backbones.ItemsSource = this.DataGridBinding_Backbones;
            }
            catch (Exception e)
            {
                var messageBoxQuery = e.Message;
                var messageBoxShortPrompt = "Lipid Backbone Template Loading Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        /// <summary>
        /// Add a new lipid backbone to the lipid backbone grid and bind a matching object to the backbone ObservableCollection&lt;LipidBackbone&gt;
        /// <br/>
        /// Adds brief color highlight when row is added
        /// </summary>
        private void Button_Backbones_Add_Click(object sender, RoutedEventArgs e)
        {
            var newBackboneName = "NewLipidBackbone";
            var newBackboneFormula = "C1H2N3O4";
            var newBackboneNumberOfMoieties = "2";

            var newBackboneObject = new LipidBackbone(newBackboneName, newBackboneFormula, newBackboneNumberOfMoieties);
            DataGridBinding_Backbones.Add(newBackboneObject);

            // force update of data grid? 
            DataGrid_Backbones.UpdateLayout();

            // programatically select newly added adduct row
            DataGrid_Backbones.SelectedItems.Clear();

            var newRowIndex = DataGridBinding_Backbones.Count - 1;
            var newSelectedRow = DataGrid_Backbones.Items[newRowIndex];
            DataGrid_Backbones.ScrollIntoView(newSelectedRow);

            // mark a color animation transition for added row
            DataGridRow row = (DataGridRow)DataGrid_Backbones.ItemContainerGenerator.ContainerFromIndex(newRowIndex);
            Color startColor = LibraryEditor.addedRowColor;
            Color endColor = Colors.White;

            ColorAnimation ca = new ColorAnimation(endColor, new Duration(TimeSpan.FromSeconds(0.5)));
            row.Background = new SolidColorBrush(startColor);
            row.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }

        /// <summary>
        /// Remove an backbone from the backbone data grid and the corresponding object from the ObservableCollection&lt;LipidBackbone&gt;
        /// </summary>
        private void Button_Backbones_Remove_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = DataGrid_Backbones.SelectedIndex;

            if (selectedRow != -1)
            {
                DataGridBinding_Backbones.RemoveAt(selectedRow);
            }
        }

        /// <summary>
        /// Saves all backbones in the backbone data grid to the text-based LipiDex library .csv file.
        /// </summary>
        private void Button_Backbones_SaveBackbones_Click(object sender, RoutedEventArgs e)
        {
            // default to false so we don't try writing out an empty library
            var validLibrary = false;

            for (var i = 0; i < DataGridBinding_Backbones.Count; i++)
            {
                var backbone = DataGridBinding_Backbones[i];

                // if there is an invalid adduct, let validation message box be thrown and set all valid to false.
                // this will break on 
                if (backbone.IsValid(i))
                {
                    validLibrary = true;
                }
                else
                {
                    // bad adduct
                    // break iteration and send popup alert
                    validLibrary = false;
                    break;
                }
            }

            if (validLibrary)
            {
                try
                {
                    SaveBackboneLibrary(libraryPath);

                    var messageBoxQuery = string.Format("Library \"{0}\" has been successfully saved.\n\nOther open windows referencing \"{0}\" will not reflect these changes and should be reloaded!", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
                    var messageBoxShortPrompt = "Library Saved Successfully!";
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Asterisk;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                    // just in case there's weird shenanigans in my code, reload the lipid backbones from the file to make sure there are no
                    // weird artifacts left over from the data grid
                    LoadLipidBackbones(libraryPath);
                }
                catch (InvalidOperationException exception)
                {
                    var messageBoxQuery = string.Format("Unexpected error while saving changes to library \"{0}\".All edits have been rolled back!\n\nError Message:\n{1}", System.IO.Path.GetFileNameWithoutExtension(libraryPath), exception.Message);
                    var messageBoxShortPrompt = "Error - Changes Not Saved!";
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                }
            }
            else
            {
                var messageBoxQuery = string.Format("Formatting error(s) detected with lipid backbone entries in library \"{0}\". They must be corrected before this library can be saved.", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
                var messageBoxShortPrompt = "Library Formatting Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var results = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        /// <summary>
        /// - Saves all changes made to the lipid backbone library stored in the [base]/Resources/LipidexLibraries directory if all changes are valid.
        /// <br/>
        /// - If an error occurs during save, rolls back library changes to original
        /// <br/>
        /// - Clears the ObservableCollection&lt;LipidBackbone&gt; and repopulates it from the text-based LipiDex libraries.
        /// </summary>
        private void SaveBackboneLibrary(string libraryBasePath)
        {
            var backboneLibraryPath = System.IO.Path.Combine(libraryBasePath, "Backbones.csv");
            var backupBackboneLibraryPath = System.IO.Path.Combine(libraryBasePath, "Backbones_tmpBackup.csv");

            try
            {
                // first, back up old version of the adduct library
                File.Copy(backboneLibraryPath, backupBackboneLibraryPath);

                // overwrite old library version
                var writer = new StreamWriter(backboneLibraryPath);

                // write txt file headers
                writer.WriteLine(string.Format("{0},{1},{2}", "Name", "Formula", "Number of Moieties"));

                // write out each adduct
                foreach (var backbone in DataGridBinding_Backbones)
                {
                    writer.WriteLine(backbone.SaveString());
                }

                writer.Close();
                writer.Dispose();

                // successfully wrote library
                // delete backup copy of old library.
                File.Delete(backupBackboneLibraryPath);
            }
            catch (Exception e)
            {
                // something about updating the new library failed.
                // restore old library
                File.Copy(backupBackboneLibraryPath, backboneLibraryPath, true);

                // delete backup copy
                File.Delete(backupBackboneLibraryPath);

                // throw exception back to upper level
                throw new InvalidOperationException(e.Message);
            }
        }

        // <summary>
        /// Effectively discards all changes since the library was last saved. Clears the ObservableCollection&lt;LipidBackbone&gt; and repopulates it from the text-based LipiDex Libraries
        /// </summary>
        private void Button_Backbones_ReloadOldBackbones_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxQuery = string.Format("Reload library \"{0}\" from last-saved version? Any unsaved edits will be lost.", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
            var messageBoxShortPrompt = "Reloading last saved version of library!";
            var messageBoxButtonOptions = MessageBoxButton.YesNoCancel;
            var messageBoxImage = MessageBoxImage.Question;

            var results = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

            if (results.Equals(MessageBoxResult.Yes))
            {
                LoadLipidBackbones(this.libraryPath);
            }

        }

        /// <summary>
        /// Event which generates row numbers in data grid using an object's index in the ObservableCollection&lt;LipidBackbones&gt;.
        /// </summary>
        private void DataGrid_Backbones_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row;

            // show row number
            row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>
        /// Validates a cell which just finished editing.
        /// </summary>
        private void DataGrid_Backbones_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // validate the individual edited cell
            switch (e.Column.DisplayIndex)
            {
                // backbone name
                case 0:
                    var index = e.Row.GetIndex();
                    var editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_Backbones[index].ValidateBackboneName(editedTextBox.Text, index);
                    break;

                // backbone formula
                case 1:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_Backbones[index].ValidateBackboneFormula(editedTextBox.Text, e.Row.GetIndex());
                    break;

                // backbone number of moieties
                case 2:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_Backbones[index].ValidateNumberOfMoieties(editedTextBox.Text, e.Row.GetIndex());
                    break;
            }
        }

        private void DataGrid_Backbones_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {

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

        #endregion

        #region Lipid Class Tab Controls

        /// <summary>
        /// Stores two-way bound LipidClass objects to the Library Generator DataGrid - Lipid Class tab.
        /// </summary>
        public ObservableCollection<LipidClass> DataGridBinding_LipidClasses = new ObservableCollection<LipidClass>();

        /// <summary>
        /// Loads all lipid classes from the lipid class template file.
        /// </summary>
        private void LoadLipidClasses(string libraryBasePath)
        {
            var lipidClassPath = System.IO.Path.Combine(libraryBasePath, "Lipid_Classes.csv");

            try
            {
                var reader = new CsvReader(new StreamReader(lipidClassPath), true);
                var lipidClasses = new List<LipidClass>();

                while (reader.ReadNextRecord())
                {
                    string classAbbreviation = reader["Abbreviation"];
                    string className = reader["Full Name"];
                    string headGroup = reader["Head Group"];
                    string delimitedAdducts = reader["Adducts"];
                    string backboneString = reader["Backbone"];
                    string optimalPolarityString = reader["Optimal Polarity"];
                    string numberOfMotifs = reader["Number of Moieties"];

                    var lipidClass = new LipidClass(className, classAbbreviation, headGroup, delimitedAdducts, backboneString, optimalPolarityString, this);
                    lipidClass.AttachLipidMoieties(reader);

                    lipidClasses.Add();

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

                    //
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

        /// <summary>
        /// Add a new lipid adduct to the lipid adduct grid and bind a matching object to the adduct ObservableCollection&lt;Adduct&gt;<br/>Adds brief color highlight when row is added
        /// </summary>
        private void Button_Adducts_Add_Click(object sender, RoutedEventArgs e)
        {
            var newAdductName = "NewLipidAdduct";
            var newAdductFormula = "C1H2N3O4";
            var newAdductIsLoss = "false";
            var newAdductPolarity = "+";
            var newAdductCharge = "1";

            var newAdductObject = new Adduct(newAdductName, newAdductFormula, newAdductIsLoss, newAdductPolarity, newAdductCharge);
            DataGridBinding_Adducts.Add(newAdductObject);

            // force update of data grid? 
            DataGrid_Adducts.UpdateLayout();

            // programatically select newly added adduct row
            DataGrid_Adducts.SelectedItems.Clear();

            var newRowIndex = DataGridBinding_Adducts.Count - 1;
            var newSelectedRow = DataGrid_Adducts.Items[newRowIndex];
            DataGrid_Adducts.ScrollIntoView(newSelectedRow);

            // mark a color animation transition for added row
            DataGridRow row = (DataGridRow)DataGrid_Adducts.ItemContainerGenerator.ContainerFromIndex(newRowIndex);
            Color startColor = LibraryEditor.addedRowColor;
            Color endColor = Colors.White;

            ColorAnimation ca = new ColorAnimation(endColor, new Duration(TimeSpan.FromSeconds(0.5)));
            row.Background = new SolidColorBrush(startColor);
            row.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }

        /// <summary>
        /// Remove an adduct from the adduct data grid and the corresponding object from the ObservableCollection&lt;adduct&gt;
        /// </summary>
        private void Button_Adducts_Remove_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = DataGrid_Adducts.SelectedIndex;

            if (selectedRow != -1)
            {
                DataGridBinding_Adducts.RemoveAt(selectedRow);
            }
        }

        /// <summary>
        /// Saves all adducts in the adduct data grid to the text-based LipiDex library .csv file.
        /// </summary>
        private void Button_Adducts_SaveAdducts_Click(object sender, RoutedEventArgs e)
        {
            // default to false so we don't try writing out an empty library
            var validLibrary = false;

            for (var i = 0; i < DataGridBinding_Adducts.Count; i++)
            {
                var adduct = DataGridBinding_Adducts[i];

                // if there is an invalid adduct, let validation message box be thrown and set all valid to false.
                // this will break on 
                if (adduct.IsValid(i))
                {
                    validLibrary = true;
                }
                else
                {
                    // bad adduct
                    // break iteration and send popup alert
                    validLibrary = false;
                    break;
                }
            }

            if (validLibrary)
            {
                try
                {
                    SaveAdductLibrary(libraryPath);

                    var messageBoxQuery = string.Format("Library \"{0}\" has been successfully saved.\n\nOther open windows referencing \"{0}\" will not reflect these changes and should be reloaded!", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
                    var messageBoxShortPrompt = "Library Saved Successfully!";
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Asterisk;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                    // just in case there's weird shenanigans in my code, reload the lipid adducts from the file to make sure there are no
                    // weird artifacts left over from the data grid
                    LoadLipidAdducts(libraryPath);
                }
                catch (InvalidOperationException exception)
                {
                    var messageBoxQuery = string.Format("Unexpected error while saving changes to library \"{0}\".All edits have been rolled back!\n\nError Message:\n{1}", System.IO.Path.GetFileNameWithoutExtension(libraryPath), exception.Message);
                    var messageBoxShortPrompt = "Error - Changes Not Saved!";
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
                }
            }
            else
            {
                var messageBoxQuery = string.Format("Formatting error(s) detected with adduct entries in library \"{0}\". They must be corrected before this library can be saved.", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
                var messageBoxShortPrompt = "Library Formatting Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var results = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        /// <summary>
        /// - Saves all changes made to the lipid adduct library stored in the [base]/Resources/LipidexLibraries directory if all changes are valid.
        /// <br/>
        /// - If an error occurs during save, rolls back library changes to original
        /// <br/>
        /// - Clears the ObservableCollection&lt;Adduct&gt; and repopulates it from the text-based LipiDex libraries.
        /// </summary>
        private void SaveAdductLibrary(string libraryBasePath)
        {
            var adductLibraryPath = System.IO.Path.Combine(libraryBasePath, "Adducts.csv");
            var backupAdductLibraryPath = System.IO.Path.Combine(libraryBasePath, "Adducts_tmpBackup.csv");

            try
            {
                // first, back up old version of the adduct library
                File.Copy(adductLibraryPath, backupAdductLibraryPath);

                // overwrite old library version
                var writer = new StreamWriter(adductLibraryPath);

                // write txt file headers
                writer.WriteLine(string.Format("{0},{1},{2},{3},{4}", "Name", "Formula", "Loss", "Polarity", "Charge"));

                // write out each adduct
                foreach (var adduct in DataGridBinding_Adducts)
                {
                    writer.WriteLine(adduct.SaveString());
                }

                writer.Close();
                writer.Dispose();

                // successfully wrote library
                // delete backup copy of old library.
                File.Delete(backupAdductLibraryPath);
            }
            catch (Exception e)
            {
                // something about updating the new library failed.
                // restore old library
                File.Copy(backupAdductLibraryPath, adductLibraryPath, true);

                // delete backup copy
                File.Delete(backupAdductLibraryPath);

                // throw exception back to upper level
                throw new InvalidOperationException(e.Message);
            }
        }

        /// <summary>
        /// Effectively discards all changes since the library was last saved. Clears the ObservableCollection&lt;Adduct&gt; and repopulates it from the text-based LipiDex Libraries
        /// </summary>
        private void Button_Adducts_ReloadOldAdducts_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxQuery = string.Format("Reload library \"{0}\" from last-saved version? Any unsaved edits will be lost.", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
            var messageBoxShortPrompt = "Reloading last saved version of library!";
            var messageBoxButtonOptions = MessageBoxButton.YesNoCancel;
            var messageBoxImage = MessageBoxImage.Question;

            var results = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

            if (results.Equals(MessageBoxResult.Yes))
            {
                LoadLipidAdducts(this.libraryPath);
            }

        }



        /// <summary>
        /// Event which generates row numbers in data grid using an object's index in the ObservableCollection&lt;Adduct&gt;.
        /// </summary>
        private void DataGrid_LipidClasses_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row;

            // show row number
            row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>
        /// Validates a cell which just finished editing.
        /// </summary>
        private void DataGrid_LipidClasses_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // validate the individual edited cell
            switch (e.Column.DisplayIndex)
            {
                // adduct name
                case 0:
                    var index = e.Row.GetIndex();
                    var editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_LipidClasses[index].valid(editedTextBox.Text, index);
                    break;

                // adduct formula
                case 1:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_Adducts[index].ValidateAdductFormula(editedTextBox.Text, e.Row.GetIndex());
                    break;

                // adduct loss
                case 2:
                    break;
                // don't need a check for enabled/disabled since it's boolean. It will always be valid.

                // adduct polarity
                case 3:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_Adducts[index].ValidateAdductPolarity(editedTextBox.Text, e.Row.GetIndex());
                    break;
                // don't need a check for enabled/disabled since it's boolean. It will always be valid.

                // adduct charge
                case 4:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;

                    if (DataGridBinding_Adducts[index].ValidateAdductCharge(editedTextBox.Text, e.Row.GetIndex()))
                    {
                        editedTextBox.Text = DataGridBinding_Adducts[index].charge;
                    }

                    break;
            }
        }

        private void DataGrid_LipidClasses_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {

        }
        #endregion
    }
}
