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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSMSL;
using CSMSL.IO.Thermo;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;

namespace LipiDex_2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread backgroundThread = new Thread(new ThreadStart(ReadDataFromRawFile));
            backgroundThread.Start();
        }

        private void LibraryGenerator_MouseEnter(object sender, RoutedEventArgs e)
        {
            LibraryGeneratorLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources","Icons","libraryGeneratorActive.png"));
        }

        private void LibraryGenerator_MouseLeave(object sender, RoutedEventArgs e)
        {
            LibraryGeneratorLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "libraryGenerator.png"));
        }

        private void LibraryGenerator_Click(object sender, RoutedEventArgs e)
        {
            LibraryGeneratorGui libraryGeneratorGuiInstance = new LibraryGeneratorGui();

            libraryGeneratorGuiInstance.Show();
        }

        private void LibraryForge_MouseEnter(object sender, RoutedEventArgs e)
        {
            LibraryForgeLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "libraryForgeActive.png"));
        }

        private void LibraryForge_MouseLeave(object sender, RoutedEventArgs e)
        {
            LibraryForgeLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "libraryForge.png"));
        }

        private void LibraryForge_Click(object sender, RoutedEventArgs e)
        {
            LibraryForgeGui libraryForgeGuiInstance = new LibraryForgeGui();

            libraryForgeGuiInstance.Show();
        }

        private void SpectrumGenerator_MouseEnter(object sender, RoutedEventArgs e)
        {
            SpectrumGeneratorLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "spectrumGeneratorActive.png"));
        }

        private void SpectrumGenerator_MouseLeave(object sender, RoutedEventArgs e)
        {
            SpectrumGeneratorLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "spectrumGenerator.png"));
        }

        private void SpectrumGenerator_Click(object sender, RoutedEventArgs e)
        {
            SpectrumGeneratorGui spectrumGeneratorGuiInstance = new SpectrumGeneratorGui();

            spectrumGeneratorGuiInstance.Show();
        }

        private void SpectrumSearcher_MouseEnter(object sender, RoutedEventArgs e)
        {
            SpectrumSearcherLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "spectrumSearcherActive.png"));
        }

        private void SpectrumSearcher_MouseLeave(object sender, RoutedEventArgs e)
        {
            SpectrumSearcherLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "spectrumSearcher.png"));
        }

        private void SpectrumSearcher_Click(object sender, RoutedEventArgs e)
        {
            SpectrumSearcherGui spectrumSearcherGuiInstance = new SpectrumSearcherGui();

            spectrumSearcherGuiInstance.Show();
        }

        private void PeakFinder_MouseEnter(object sender, RoutedEventArgs e)
        {
            PeakFinderLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "peakFinderActive.png"));
        }

        private void PeakFinder_MouseLeave(object sender, RoutedEventArgs e)
        {
            PeakFinderLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "peakFinder.png"));
        }

        private void PeakFinder_Click(object sender, RoutedEventArgs e)
        {
            PeakFinderGui peakFinderGuiInstance = new PeakFinderGui();

            peakFinderGuiInstance.Show();
        }

        private void DataCleaner_MouseEnter(object sender, RoutedEventArgs e)
        {
            ResultCleanerLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "resultCleanerBroomOutlineActive.png"));
        }

        private void DataCleaner_MouseLeave(object sender, RoutedEventArgs e)
        {
            ResultCleanerLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "resultCleanerBroomOutline.png"));
        }

        private void DataCleaner_Click(object sender, RoutedEventArgs e)
        {
            DataCleanerGui dataCleanerGuiInstance = new DataCleanerGui();

            dataCleanerGuiInstance.Show();
        }

        private void CoonLogo_MouseEnter(object sender, RoutedEventArgs e)
        {
            CoonLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "coonLogoActive.png"));
        }

        private void CoonLogo_MouseLeave(object sender, RoutedEventArgs e)
        {
            CoonLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "coonLogo.png"));
        }

        private void CoonLogo_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://coonlabs.com/",
                UseShellExecute = true
            });
        }

        private void LipidexLogo_MouseEnter(object sender, RoutedEventArgs e)
        {
            LipidexLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "lipidexLogoActive.png"));
        }

        private void LipidexLogo_MouseLeave(object sender, RoutedEventArgs e)
        {
            LipidexLogo.Source = UpdateImageEvent(System.IO.Path.Combine("Resources", "Icons", "lipidexLogo.png"));
        }

        private void LipidexLogo_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/coongroup/LipiDex/",
                UseShellExecute = true
            });
        }

        private BitmapImage UpdateImageEvent(string path, UriKind relativeOrAbsolute = UriKind.Relative)
        {
            Uri imgUri = new Uri(path, relativeOrAbsolute);
            return new BitmapImage(imgUri);
        }

        private void ReadDataFromRawFile()
        {
            /*
            //var rawfile = new ThermoRawFile(@"B:\Intelligent Lipid Acquisition\20220620_RevisitUltimateSplashLibrary\20210322_KAO_ultimateSplash_30RF_3500V_25_10_5_rep1.raw");
            var rawfile = new ThermoRawFile(@"B:\Intelligent Lipid Acquisition\20220502_DRB_FinalRtlsDataset\Raw Files\Blanks\20220501_DRB_Blank_ITMS_Rep1.raw");
            rawfile.Open();
            
            TextBox_rawfileName.Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBox_rawfileName.Text = rawfile.Name;
            }));

            for (var i = rawfile.FirstSpectrumNumber + 1; i <= rawfile.LastSpectrumNumber; i++)
            {
                var scan = rawfile.GetSpectrum(i);

                TextBox_scanProperties.Dispatcher.BeginInvoke(new Action(() =>
                {
                    TextBox_scanProperties.Text = string.Format("Scan Num: {0}, First Mass: {1}, Last Mass: {2}", i, scan.FirstMZ, scan.LastMZ);
                }));

                //Thread.Sleep(500);

            }
            */
        }
        
        private void MainWindow_CloseOtherWindows(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var messageBoxQuery = "Closing the main window of LipiDex.\n\nDo you want to close all other open windows as well?";
            var messageBoxShortPrompt = "Closing LipiDex";
            var messageBoxButtonOptions = MessageBoxButton.YesNoCancel;
            var messageBoxImage = MessageBoxImage.Question;
            
            var messageBoxResult = MessageBox.Show(messageBoxQuery, messageBoxShortPrompt, messageBoxButtonOptions, messageBoxImage);
            
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
            else if (messageBoxResult == MessageBoxResult.No)
            {
                Application.Current.Windows[Application.Current.Windows.Count - 1].Close();
            } 
            else
            {
                e.Cancel = true;
            }
        }
    }
}
