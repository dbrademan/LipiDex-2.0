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
using System.IO;

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

            var indexOfFirstDisallowedChar = LibraryName_Textbox.Text.IndexOfAny(System.IO.Path.GetInvalidFileNameChars());
            if (indexOfFirstDisallowedChar != -1)
            {
                var messageBoxQuery = string.Format("The character `{0}` is not allowed in the library name. Please remove any non-typical file name characters.", LibraryName_Textbox.Text[indexOfFirstDisallowedChar]);
                var messageBoxShortPrompt = "Library creation error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return;
            }

            if (Directory.Exists(LibraryName_Textbox.Text))
            {
                var messageBoxQuery = "This library already exists. Please either enter a unique library name, or edit the existing library";
                var messageBoxShortPrompt = "Library creation error!";
                var messageBoxButtonOptions = MessageBoxButton.OK;
                var messageBoxImage = MessageBoxImage.Error;

                var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);

                return;
            }

            // try to create new library folder
            var workingDirectory = string.Format(@"{0}\Resources\LipidexLibraries\", Directory.GetCurrentDirectory());
            DirectoryInfo libraryDirectory;

            try
            {
                libraryDirectory = Directory.CreateDirectory(workingDirectory);
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
                var templateDirectory = workingDirectory + @"templates\";

                // copy adduct table
                var templateFile = templateDirectory + "template_Adducts.csv";
                var targetFile = libraryDirectory.FullName + "Adducts.csv";
                File.Copy(templateDirectory, targetFile);

                // copy fatty acid table
                templateFile = templateDirectory + "template_FattyAcids.csv";
                targetFile = libraryDirectory.FullName + "FattyAcids.csv";
                File.Copy(templateDirectory, targetFile);

                // copy lipid classes table
                templateFile = templateDirectory + "template_Lipid_Classes.csv";
                targetFile = libraryDirectory.FullName + "Lipid_Classes.csv";
                File.Copy(templateDirectory, targetFile);

                // copy ms2 templates table
                templateFile = templateDirectory + "template_MS2_Templates.csv";
                targetFile = libraryDirectory.FullName + "MS2_Templates.csv";
                File.Copy(templateDirectory, targetFile);
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

        }

        private void LoadExistingLibraries()
        {

        }

        private void DeleteLib_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChooseLib_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FocusNewLibraryName(object sender, RoutedEventArgs e)
        {
            if (this.LibraryName_Textbox.Text.Equals("New Library Name..."));
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
