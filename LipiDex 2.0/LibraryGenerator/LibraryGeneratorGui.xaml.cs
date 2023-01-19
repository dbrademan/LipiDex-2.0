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
using System.IO;
using LipiDex_2._0.LibraryGenerator;

namespace LipiDex_2._0
{
    /// <summary>
    /// Interaction logic for LibraryGeneratorGui.xaml
    /// </summary>
    public partial class LibraryGeneratorGui : Window
    {
        public LibraryGeneratorGui()
        {
            InitializeComponent();
            LoadExistingLibraries();
            LibraryName_Textbox.Text = String.Format("New Library {0}", DateTime.Now.ToString("yyyyMMdd"));
        }

        /// <summary>
        /// Read in currently existing libraries
        /// </summary>
        private void LoadExistingLibraries()
        {
            // remove all library entries
            LipidexLibraries_ListBox.Items.Clear();

            var libraryDirectory = System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", "LipidexLibraries");
            var existingLibraries = Directory.GetDirectories(libraryDirectory);

            foreach (var library in existingLibraries)
            {
                LipidexLibraries_ListBox.Items.Add(Path.GetFileName(library));
            }
        }

        // access new library name in textbox.
        // make new library directory in ./resources/lipidexLibraries/
        // copy template libraries into new directory.
        private void CreateNewLib_Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LibraryName_Textbox.Text))
            {
                var messageBoxQuery = "Please enter a new library name to create a new library.";
                var messageBoxShortPrompt = "Library creation error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return;
            }

            var indexOfFirstDisallowedChar = LibraryName_Textbox.Text.IndexOfAny(Path.GetInvalidFileNameChars());
            if (indexOfFirstDisallowedChar != -1)
            {
                var messageBoxQuery = string.Format("The character `{0}` is not allowed in the library name. Please remove any non-typical file name characters.", LibraryName_Textbox.Text[indexOfFirstDisallowedChar]);
                var messageBoxShortPrompt = "Library creation error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return;
            }

            // check if library already exists.
            var workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "LipidexLibraries");

            if (Directory.Exists(Path.Combine(workingDirectory, LibraryName_Textbox.Text)))
            {
                var messageBoxQuery = "This library already exists. Please either enter a unique library name, or edit the existing library";
                var messageBoxShortPrompt = "Library creation error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return;
            }

            // create new library
            DirectoryInfo libraryDirectory;
            try
            {
                libraryDirectory = Directory.CreateDirectory(Path.Combine(workingDirectory, LibraryName_Textbox.Text));
            }
            catch (Exception exception)
            {
                var messageBoxQuery = exception.Message;
                var messageBoxShortPrompt = "Library-specific directory creation error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;
                
                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return;
            }

            // try to copy over template library files
            try
            {
                var templateDirectory = Path.GetFullPath(Path.Combine(workingDirectory, "..", "LibraryTemplates"));

                // copy adduct table
                var templateFile = Path.Combine(templateDirectory, "template_Adducts.csv");
                var targetFile = Path.Combine(libraryDirectory.FullName, "Adducts.csv");
                File.Copy(templateFile, targetFile);

                // copy fatty acid table
                templateFile = Path.Combine(templateDirectory, "template_FattyAcids.csv");
                targetFile = Path.Combine(libraryDirectory.FullName, "FattyAcids.csv");
                File.Copy(templateFile, targetFile);

                // copy lipid classes table
                templateFile = Path.Combine(templateDirectory, "template_Lipid_Classes.csv");
                targetFile = Path.Combine(libraryDirectory.FullName, "Lipid_Classes.csv");
                File.Copy(templateFile, targetFile);

                // copy ms2 templates table
                templateFile = Path.Combine(templateDirectory, "template_MS2_Templates.csv");
                targetFile = Path.Combine(libraryDirectory.FullName, "MS2_Templates.csv");
                File.Copy(templateFile, targetFile);
            }
            catch (Exception exception)
            {
                var messageBoxQuery = exception.Message;
                var messageBoxShortPrompt = "Error Transferring LipiDex Library Templates!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return;
            }

            // reload libraries
            LoadExistingLibraries();
        }

        private void DeleteLib_Button_Click(object sender, RoutedEventArgs e)
        {
            if (LipidexLibraries_ListBox.SelectedItem != null)
            {
                var messageBoxQuery = string.Format("Permanently delete library `{0}` and all its associated files? This is irreversible.", LibraryName_Textbox.Text);
                var messageBoxShortPrompt = "Just Making Sure...";
                var messageBoxButtonOptions = MessageBoxButton.YesNo;
                var messageBoxImage = MessageBoxImage.Exclamation;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var pathToDelete = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "LipidexLibraries", LipidexLibraries_ListBox.SelectedItem.ToString());
                    Directory.Delete(pathToDelete, true);
                }

                // reload libraries
                LoadExistingLibraries();
                return;
            }
            else
            {
                var messageBoxQuery = "No Library Selected!";
                var messageBoxShortPrompt = "Please Select A Library To Delete (if you really want to)!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return;
                
            }
        }

        private void ChooseLib_Button_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = LipidexLibraries_ListBox.SelectedItem;
            
            if (LipidexLibraries_ListBox.SelectedItem == null)
            {
                var messageBoxQuery = "No Library Selected";
                var messageBoxShortPrompt = "Please Select A Library To Open!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return;
            }
            else
            {
                // procedurally construct library path from current directory
                var libraryPath = Path.Combine(
                    Directory.GetCurrentDirectory(), 
                    "Resources", 
                    "LipidexLibraries", 
                    LipidexLibraries_ListBox.SelectedItem.ToString()
                );

                // check to make sure library exists and it contains all necessary template files to load.
                // it should always exists, but I've seen weirder things happen before...
                try
                {
                    if (LibraryExists(libraryPath))
                    {
                        LibraryEditor libraryEditorInstance = new LibraryEditor(libraryPath);

                        libraryEditorInstance.Show();

                        // the library selector window staying open feels a little clunky.
                        // close it after the library editor window opens.
                        this.Close();
                    }
                }
                catch (IOException exception)
                {
                    var messageBoxQuery = "Error in loading lipid library...";
                    var messageBoxShortPrompt = exception.Message;
                    var messageBoxButtonOptions = MessageBoxButton.OK;
                    var messageBoxImage = MessageBoxImage.Error;

                    var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                    return;
                }
            }
        }

        /// <summary>
        /// Checks to make sure library exists and contains all necessary template files to load.
        /// </summary>
        /// <param name="libraryPath"></param>
        /// <returns>
        /// True if library directory and all files exist. False if any dependencies are missing
        /// </returns>
        private bool LibraryExists(string libraryPath)
        {
            // check if the library folder exists
            if (!Directory.Exists(libraryPath))
            {
                throw new DirectoryNotFoundException(string.Format("The folder for the selected library could not be found. Missing file path:\n{0}", libraryPath));
            }

            // check if each library csv file exists
            //
            if (!File.Exists(Path.Combine(libraryPath, "Adducts.csv")))
            {
                throw new FileNotFoundException(string.Format("The lipid adduct template file for the selected library could not be found.\nDirectory:{0}\nMissing file: Adducts.csv", libraryPath));
            }
            // check if each library csv file exists
            if (!File.Exists(Path.Combine(libraryPath, "FattyAcids.csv")))
            {
                throw new FileNotFoundException(string.Format("The lipid fatty acid template file for the selected library could not be found.\nDirectory:{0}\nMissing file: FattyAcids.csv", libraryPath));
            }
            // check if each library csv file exists
            if (!File.Exists(Path.Combine(libraryPath, "Lipid_Classes.csv")))
            {
                throw new FileNotFoundException(string.Format("The lipid class template file for the selected library could not be found.\nDirectory:{0}\nMissing file: Lipid_Classes.csv", libraryPath));
            }
            // check if each library csv file exists
            if (!File.Exists(Path.Combine(libraryPath, "MS2_Templates.csv")))
            {
                throw new FileNotFoundException(string.Format("The lipid fragmentation template file for the selected library could not be found.\nDirectory:{0}\nMissing file: MS2_Templates.csv", libraryPath));
            }

            return true;
        }

        private void FocusNewLibraryName(object sender, RoutedEventArgs e)
        {
            if (this.LibraryName_Textbox.Text.Equals("New Library Name..."))
            {
                this.LibraryName_Textbox.Text = "";
            }
        }

        private void LoseFocusNewLibraryName(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.LibraryName_Textbox.Text))
            {
                this.LibraryName_Textbox.Text = "New Library Name...";
            }
        }
    }
}
