using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;

namespace ShreddersModInstaller
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string shreddersPath;

        public MainWindow()
        {
            InitializeComponent();

            getSettings();
        }

        void getSettings()
        {
            // get install dir
            shreddersPath = Properties.Settings.Default.shreddersPath;
            txtShreddersInstall.Text = shreddersPath;

        }

        void setSettings(string setting)
        {
            switch (setting)
            {
                case "install":
                    Properties.Settings.Default.shreddersPath = shreddersPath;
                    break;
                default:
                    MessageBox.Show("No setting to save");
                    break;
            }

            Properties.Settings.Default.Save();
        }

        void extractMod(string zipPath)
        {
            string extractPath = shreddersPath;

            // Normalizes the path.
            extractPath = Path.GetFullPath(extractPath);

            // Ensures that the last character on the extraction path
            // is the directory separator char.
            // Without this, a malicious zip file could try to traverse outside of the expected
            // extraction path.
            if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                extractPath += Path.DirectorySeparatorChar;

            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase))
                    {
                        // Gets the full path to ensure that relative segments are removed.
                        string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                        // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                        // are case-insensitive.
                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                            entry.ExtractToFile(destinationPath);
                    }
                }
            }
        }

        private void BtnSelectZip_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Shredders Mods (*.shmod;*.zip)|*.shmod;*.zip";
            if (openFileDialog.ShowDialog() == true)
            {
                txtZipPath.Text = openFileDialog.FileName;
            }
        }

        private void BtnShreddersPath_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    shreddersPath = dialog.SelectedPath;

                    // if is install dir, set setting ; if not, gib error
                    if (shreddersPath.EndsWith("Shredders"))
                    {
                        txtShreddersInstall.Text = dialog.SelectedPath;

                        setSettings("install");
                    }
                    else
                    {
                        MessageBox.Show("Wrong Folder. Must be the Shredders main directory.");
                    }
                }
            }
        }

        private void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            extractMod(txtZipPath.Text);
        }
    }
}
