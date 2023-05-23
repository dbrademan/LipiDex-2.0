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
            LoadPolymericHeadgroups(libraryPath);
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
            Adduct selectedItem = (Adduct)DataGrid_Adducts.SelectedItem;

            if (selectedItem != null)
            {
                DataGridBinding_Adducts.Remove(selectedItem);
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
            FattyAcid selectedItem = (FattyAcid)DataGrid_FattyAcids.SelectedItem;

            if (selectedItem != null)
            {
                DataGridBinding_FattyAcids.Remove(selectedItem);
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
            LipidBackbone selectedItem = (LipidBackbone)DataGrid_Backbones.SelectedItem;

            if (selectedItem != null)
            {
                DataGridBinding_Backbones.Remove(selectedItem);
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

        #endregion

        #region Polymeric Headgroup Tab Controls

        /// <summary>
        /// Stores two-way bound PolymericHeadgroup objects to the Library Generator DataGrid - Polymeric Headgroups tab.
        /// </summary>
        public ObservableCollection<PolymericHeadgroup> DataGridBinding_PolyHeadgroups = new ObservableCollection<PolymericHeadgroup>();

        // <summary>
        /// Loads all lipid backbones from the specified library (libraryBasePath).
        /// </summary>
        private void LoadPolymericHeadgroups(string libraryBasePath)
        {
            var polyHeadgroupPath = System.IO.Path.Combine(libraryBasePath, "Poly_Headgroups.csv");

            try
            {
                var reader = new CsvReader(new StreamReader(polyHeadgroupPath), true);
                this.DataGridBinding_PolyHeadgroups = new ObservableCollection<PolymericHeadgroup>();

                while (reader.ReadNextRecord())
                {
                    string name = reader["Name"];
                    bool isPeptide = TextToBoolConverter(reader["IsPeptide"]);
                    bool isGlycan = TextToBoolConverter(reader["IsGlycan"]);
                    string sequence = reader["Sequence"];
                    string otherFormula = reader["OtherFormula"];

                    this.DataGridBinding_PolyHeadgroups.Add(new PolymericHeadgroup(name, isPeptide, isGlycan, sequence, otherFormula));
                }

                DataGrid_PolyHeadgroups.ItemsSource = this.DataGridBinding_PolyHeadgroups;
            }
            catch (Exception e)
            {
                var messageBoxQuery = e.Message;
                var messageBoxShortPrompt = "Lipid Polymeric Headgroup Template Loading Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        private bool TextToBoolConverter(string textToConvert) 
        {
            if (textToConvert.ToLower().Equals("true"))
            {
                return true;
            }
            else if (textToConvert.ToLower().Equals("false"))
            {
                return false;
            }
            else
            {
                throw new ApplicationException("This polymeric headgroup column ain't a bool. try again.");
            }
        }

        /// <summary>
        /// Add a new polymeric headgroup to the polymeric headgroup grid and bind a matching object to the polymeric headgroup ObservableCollection&lt;PolymericHeadgroup&gt;
        /// <br/>
        /// Adds brief color highlight when row is added
        /// </summary>
        private void Button_PolyHeadgroups_Add_Click(object sender, RoutedEventArgs e)
        {
            var newPolyHeadgroupName = "Example Peptide";
            var isPeptide = true;
            var isGlycan = false;
            var sequence = "TESTPEPTIDEK";
            var formulaModifier = "C1H2N3O4";

            var newPolyHeadgroupObject = new PolymericHeadgroup(newPolyHeadgroupName, isPeptide, isGlycan, sequence, formulaModifier);
            DataGridBinding_PolyHeadgroups.Add(newPolyHeadgroupObject);

            // force update of data grid? 
            DataGrid_PolyHeadgroups.UpdateLayout();

            // programatically select newly added adduct row
            DataGrid_PolyHeadgroups.SelectedItems.Clear();

            var newRowIndex = DataGridBinding_PolyHeadgroups.Count - 1;
            var newSelectedRow = DataGrid_PolyHeadgroups.Items[newRowIndex];
            DataGrid_PolyHeadgroups.ScrollIntoView(newSelectedRow);

            // mark a color animation transition for added row
            DataGridRow row = (DataGridRow)DataGrid_PolyHeadgroups.ItemContainerGenerator.ContainerFromIndex(newRowIndex);
            Color startColor = LibraryEditor.addedRowColor;
            Color endColor = Colors.White;

            ColorAnimation ca = new ColorAnimation(endColor, new Duration(TimeSpan.FromSeconds(0.5)));
            row.Background = new SolidColorBrush(startColor);
            row.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }

        /// <summary>
        /// Remove an polymeric headgroup from the polymeric headgroup data grid and the corresponding object from the ObservableCollection&lt;PolymericHeadgroup&gt;
        /// </summary>
        private void Button_PolyHeadgroups_Remove_Click(object sender, RoutedEventArgs e)
        {
           PolymericHeadgroup selectedItem = (PolymericHeadgroup)DataGrid_PolyHeadgroups.SelectedItem;

            if (selectedItem != null)
            {
                DataGridBinding_PolyHeadgroups.Remove(selectedItem);
            }
        }

        /// <summary>
        /// Saves all polymeric headgroups in the polymeric headgroup data grid to the text-based LipiDex library .csv file.
        /// </summary>
        private void Button_PolyHeadgroups_SavePolyHeadgroups_Click(object sender, RoutedEventArgs e)
        {
            // default to false so we don't try writing out an empty library
            var validLibrary = false;

            for (var i = 0; i < DataGridBinding_PolyHeadgroups.Count; i++)
            {
                var polyHeadgroup = DataGridBinding_PolyHeadgroups[i];

                // if there is an invalid polymericHeadgroup, let validation message box be thrown and set all valid to false.
                if (polyHeadgroup.IsValid(i))
                {
                    validLibrary = true;
                }
                else
                {
                    // bad polymeric headgroup
                    // break iteration and send popup alert
                    validLibrary = false;
                    break;
                }
            }

            if (validLibrary)
            {
                try
                {
                    SavePolymericHeadgroupLibrary(libraryPath);

                    var messageBoxQuery = string.Format("Library \"{0}\" has been successfully saved.\n\nOther open windows referencing \"{0}\" will not reflect these changes and should be reloaded!", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
                    var messageBoxShortPrompt = "Library Saved Successfully!";
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Asterisk;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                    // just in case there's weird shenanigans in my code, reload the lipid adducts from the file to make sure there are no
                    // weird artifacts left over from the data grid
                    LoadPolymericHeadgroups(libraryPath);
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
                var messageBoxQuery = string.Format("Formatting error(s) detected with polymeric headgroup entries in library \"{0}\". They must be corrected before this library can be saved.", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
                var messageBoxShortPrompt = "Library Formatting Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var results = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        /// <summary>
        /// Effectively discards all changes since the library was last saved. Clears the ObservableCollection&lt;PolymericHeadgroup&gt; and repopulates it from the text-based LipiDex Libraries
        /// </summary>
        private void Button_PolyHeadgroups_ReloadOldPolyHeadgroups_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxQuery = string.Format("Reload library \"{0}\" from last-saved version? Any unsaved edits will be lost.", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
            var messageBoxShortPrompt = "Reloading last saved version of library!";
            var messageBoxButtonOptions = MessageBoxButton.YesNoCancel;
            var messageBoxImage = MessageBoxImage.Question;

            var results = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

            if (results.Equals(MessageBoxResult.Yes))
            {
                LoadPolymericHeadgroups(this.libraryPath);
            }
        }

        /// <summary>
        /// TODO!!!
        /// - Saves all changes made to the polymeric headgroup library stored in the [base]/Resources/LipidexLibraries directory if all changes are valid.
        /// <br/>
        /// - If an error occurs during save, rolls back library changes to original
        /// <br/>
        /// - Clears the ObservableCollection&lt;PolymericHeadgroup&gt; and repopulates it from the text-based LipiDex libraries.
        /// </summary>
        private void SavePolymericHeadgroupLibrary(string libraryBasePath)
        {
            var lipidClassLibraryPath = System.IO.Path.Combine(libraryBasePath, "Polymeric_Headgroups.csv");
            var backupLipidClassLibraryPath = System.IO.Path.Combine(libraryBasePath, "Polymeric_Headgroups_tmpBackup.csv");

            try
            {
                /*
                // first, back up old version of the adduct library
                File.Copy(lipidClassLibraryPath, backupLipidClassLibraryPath);

                // overwrite old library version
                var writer = new StreamWriter(lipidClassLibraryPath);

                // write txt file headers
                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", "Abbreviation", "Full Name", "Head Group",
                    "Adducts", "Backbone", "Optimal Polarity", "Number of Moieties", "Moiety 1", "Moiety 2", "Moiety 3", "Moiety 4"));

                // write out each adduct
                foreach (var lipidClass in DataGridBinding_LipidClasses)
                {
                    writer.WriteLine(lipidClass.SaveString());
                }

                writer.Close();
                writer.Dispose();

                // successfully wrote library
                // delete backup copy of old library.
                File.Delete(backupLipidClassLibraryPath);
                */
            }
            catch (Exception e)
            {
                /*
                // something about updating the new library failed.
                // restore old library
                File.Copy(backupLipidClassLibraryPath, lipidClassLibraryPath, true);

                // delete backup copy
                File.Delete(backupLipidClassLibraryPath);

                // throw exception back to upper level
                throw new InvalidOperationException(e.Message);
                */
            }
        }
        #endregion

        /// <summary>
        /// Event which generates row numbers in data grid using an object's index in the ObservableCollection&lt;PolyHeadgroup&gt;.
        /// </summary>
        private void DataGrid_PolyHeadgroups_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row;

            // show row number
            row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>
        /// Validates a cell which just finished editing.
        /// </summary>
        private void DataGrid_PolyHeadgroups_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // validate the individual edited cell
            switch (e.Column.DisplayIndex)
            {
                // PolyHeadgroup name
                case 0:
                    var index = e.Row.GetIndex();
                    var editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_PolyHeadgroups[index].ValidatePolyHeadgroupName(editedTextBox.Text, index);
                    break;

                // PolyHeadgroup peptide type edited
                case 1:
                    index = e.Row.GetIndex();
                    var peptideBoolean = ((CheckBox)e.EditingElement).IsChecked ?? false;
                    DataGridBinding_PolyHeadgroups[index].ValidatePolyHeadgroupType(peptideBoolean, DataGridBinding_PolyHeadgroups[index].isGlycan, index);
                    break;

                // PolyHeadgroup glycan type edited
                case 2:
                    index = e.Row.GetIndex();
                    var glycanBoolean = ((CheckBox)e.EditingElement).IsChecked ?? false;
                    DataGridBinding_PolyHeadgroups[index].ValidatePolyHeadgroupType(DataGridBinding_PolyHeadgroups[index].isPeptide, glycanBoolean, index);
                    break;

                //  PolyHeadgroup sequence
                case 3:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_PolyHeadgroups[index].ValidatePolyHeadgroupSequence(editedTextBox.Text, index);
                    break;

                case 4:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_PolyHeadgroups[index].ValidateExtraFormulaBalancer(editedTextBox.Text, index);
                    break;
            }
        }

        private void DataGrid_PolyHeadgroups_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {

        }


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
                DataGridBinding_LipidClasses = new ObservableCollection<LipidClass>();

                while (reader.ReadNextRecord())
                {
                    string classAbbreviation = reader["Abbreviation"];
                    string className = reader["Full Name"];
                    string headGroup = reader["Head Group"];
                    string delimitedAdducts = reader["Adducts"];
                    string backboneClassifierString = reader["Backbone"];
                    string optimalPolarityString = reader["Optimal Polarity"];
                    string numberOfMoieties = reader["Number of Moieties"];
                    string moiety1 = reader["Moiety 1"];
                    string moiety2 = reader["Moiety 2"];
                    string moiety3 = reader["Moiety 3"];
                    string moiety4 = reader["Moiety 4"];

                    if (classAbbreviation.Equals("TG"))
                    {
                        var t = "";
                    }
                    var lipidClass = new LipidClass(className, classAbbreviation, headGroup, delimitedAdducts, backboneClassifierString, optimalPolarityString, numberOfMoieties, moiety1, moiety2, moiety3, moiety4, this);
                    DataGridBinding_LipidClasses.Add(lipidClass);
                }

                DataGrid_LipidClasses.ItemsSource = this.DataGridBinding_LipidClasses;
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
        /// Add a new lipid class to the lipid class grid and bind a matching object to the adduct ObservableCollection&lt;LipidClass&gt;<br/>Adds brief color highlight when row is added
        /// </summary>
        private void Button_LipidClasses_Add_Click(object sender, RoutedEventArgs e)
        {
            var newClassAbbreviation = "Example-GPL";
            var newClassFullName = "Example Glycerophospholipid";
            var newClassHeadgroup = "C1H2N3O4";
            var newClassDelimitedAdducts = "[M+H]+";
            var newClassBackboneClassifier = "Glycerol";
            var newClassOptimalPolarity = "+";
            var newClassNumberOfMoieties = "2";
            var newClassMoiety1 = "Alkyl";
            var newClassMoiety2 = "Alkyl";
            var newClassMoiety3 = "";
            var newClassMoiety4 = "";
            var newlipidClassObject = new LipidClass(newClassFullName, newClassAbbreviation, newClassHeadgroup, newClassDelimitedAdducts, newClassBackboneClassifier, 
                newClassOptimalPolarity, newClassNumberOfMoieties, newClassMoiety1, newClassMoiety2, newClassMoiety3, newClassMoiety4, this);
            DataGridBinding_LipidClasses.Add(newlipidClassObject);

            // force update of data grid? 
            DataGrid_LipidClasses.UpdateLayout();

            // programatically select newly added adduct row
            DataGrid_LipidClasses.SelectedItems.Clear();

            var newRowIndex = DataGridBinding_LipidClasses.Count - 1;
            var newSelectedRow = DataGrid_LipidClasses.Items[newRowIndex];
            DataGrid_LipidClasses.ScrollIntoView(newSelectedRow);

            // mark a color animation transition for added row
            DataGridRow row = (DataGridRow)DataGrid_LipidClasses.ItemContainerGenerator.ContainerFromIndex(newRowIndex);
            Color startColor = LibraryEditor.addedRowColor;
            Color endColor = Colors.White;

            ColorAnimation ca = new ColorAnimation(endColor, new Duration(TimeSpan.FromSeconds(0.5)));
            row.Background = new SolidColorBrush(startColor);
            row.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }

        /// <summary>
        /// Remove a lipid class from the lipid class data grid and the corresponding object from the ObservableCollection&lt;LipidClass&gt;
        /// </summary>
        private void Button_LipidClasses_Remove_Click(object sender, RoutedEventArgs e)
        {
            LipidClass selectedItem = (LipidClass)DataGrid_LipidClasses.SelectedItem;

            if (selectedItem != null)
            {
                DataGridBinding_LipidClasses.Remove(selectedItem);
            }
        }

        /// <summary>
        /// Saves all lipid class in the lipid class data grid to the text-based LipiDex library .csv file.
        /// </summary>
        private void Button_LipidClasses_SaveLipidClasses_Click(object sender, RoutedEventArgs e)
        {
            // default to false so we don't try writing out an empty library
            var validLibrary = false;

            for (var i = 0; i < DataGridBinding_LipidClasses.Count; i++)
            {
                var lipidClass = DataGridBinding_LipidClasses[i];

                // if there is an invalid lipidClass, let validation message box be thrown and set all valid to false.
                if (lipidClass.IsValid(this))
                {
                    validLibrary = true;
                }
                else
                {
                    // bad lipid class
                    // break iteration and send popup alert
                    validLibrary = false;
                    break;
                }
            }

            if (validLibrary)
            {
                try
                {
                    SaveLipidClassLibrary(libraryPath);

                    var messageBoxQuery = string.Format("Library \"{0}\" has been successfully saved.\n\nOther open windows referencing \"{0}\" will not reflect these changes and should be reloaded!", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
                    var messageBoxShortPrompt = "Library Saved Successfully!";
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Asterisk;

                    MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                    // just in case there's weird shenanigans in my code, reload the lipid adducts from the file to make sure there are no
                    // weird artifacts left over from the data grid
                    LoadLipidClasses(libraryPath);
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
                var messageBoxQuery = string.Format("Formatting error(s) detected with lipid class entries in library \"{0}\". They must be corrected before this library can be saved.", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
                var messageBoxShortPrompt = "Library Formatting Error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var results = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            }
        }

        /// <summary>
        /// - Saves all changes made to the lipid class library stored in the [base]/Resources/LipidexLibraries directory if all changes are valid.
        /// <br/>
        /// - If an error occurs during save, rolls back library changes to original
        /// <br/>
        /// - Clears the ObservableCollection&lt;LipidClass&gt; and repopulates it from the text-based LipiDex libraries.
        /// </summary>
        private void SaveLipidClassLibrary(string libraryBasePath)
        {
            var lipidClassLibraryPath = System.IO.Path.Combine(libraryBasePath, "Lipid_Classes.csv");
            var backupLipidClassLibraryPath = System.IO.Path.Combine(libraryBasePath, "Lipid_Classes_tmpBackup.csv");

            try
            {
                // first, back up old version of the adduct library
                File.Copy(lipidClassLibraryPath, backupLipidClassLibraryPath);

                // overwrite old library version
                var writer = new StreamWriter(lipidClassLibraryPath);

                // write txt file headers
                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", "Abbreviation","Full Name","Head Group",
                    "Adducts","Backbone","Optimal Polarity","Number of Moieties","Moiety 1","Moiety 2","Moiety 3","Moiety 4"));

                // write out each adduct
                foreach (var lipidClass in DataGridBinding_LipidClasses)
                {
                    writer.WriteLine(lipidClass.SaveString());
                }

                writer.Close();
                writer.Dispose();

                // successfully wrote library
                // delete backup copy of old library.
                File.Delete(backupLipidClassLibraryPath);
            }
            catch (Exception e)
            {
                // something about updating the new library failed.
                // restore old library
                File.Copy(backupLipidClassLibraryPath, lipidClassLibraryPath, true);

                // delete backup copy
                File.Delete(backupLipidClassLibraryPath);

                // throw exception back to upper level
                throw new InvalidOperationException(e.Message);
            }
        }

        /// <summary>
        /// Effectively discards all changes since the library was last saved. Clears the ObservableCollection&lt;LipidClass&gt; and repopulates it from the text-based LipiDex Libraries
        /// </summary>
        private void Button_LipidClasses_ReloadOldLipidClasses_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxQuery = string.Format("Reload library \"{0}\" from last-saved version? Any unsaved edits will be lost.", System.IO.Path.GetFileNameWithoutExtension(libraryPath));
            var messageBoxShortPrompt = "Reloading last saved version of library!";
            var messageBoxButtonOptions = MessageBoxButton.YesNoCancel;
            var messageBoxImage = MessageBoxImage.Question;

            var results = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

            if (results.Equals(MessageBoxResult.Yes))
            {
                LoadLipidClasses(this.libraryPath);
            }
        }

        /// <summary>
        /// Event which generates row numbers in data grid using an object's index in the ObservableCollection&lt;LipidClass&gt;.
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
                // lipid class abbreviation
                case 0:
                    var index = e.Row.GetIndex();
                    var editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_LipidClasses[index].ValidateLipidClassAbbreviation(editedTextBox.Text, index);
                    break;

                // lipid class full name
                case 1:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_LipidClasses[index].ValidateFullLipidClassName(editedTextBox.Text, e.Row.GetIndex());
                    break;

                // lipid class headgroup
                case 2:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_LipidClasses[index].ValidateLipidHeadgroup(editedTextBox.Text, e.Row.GetIndex());
                    break;

                // lipid class adducts
                case 3:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_LipidClasses[index].ValidateLipidAdductsClassifier(editedTextBox.Text, e.Row.GetIndex(), this);
                    break;

                // lipid class backbone
                case 4:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_LipidClasses[index].ValidateLipidBackboneClassifier(editedTextBox.Text, e.Row.GetIndex(), this);
                    break;

                // lipid class optimal polarity
                case 5:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_LipidClasses[index].ValidateOptimalPolarity(editedTextBox.Text, e.Row.GetIndex());
                    break;

                case 6:
                    index = e.Row.GetIndex();
                    editedTextBox= (TextBox)e.EditingElement;
                    DataGridBinding_LipidClasses[index].ValidateNumberOfMoieties(editedTextBox.Text, e.Row.GetIndex());
                    break;

                case 7: case 8: case 9: case 10:
                    index = e.Row.GetIndex();
                    editedTextBox = (TextBox)e.EditingElement;
                    DataGridBinding_LipidClasses[index].ValidateSingleChangedMoiety(editedTextBox.Text, e.Column.DisplayIndex, e.Row.GetIndex());
                    break;
            }
        }

        private void DataGrid_LipidClasses_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {

        }
        #endregion
    }
}
