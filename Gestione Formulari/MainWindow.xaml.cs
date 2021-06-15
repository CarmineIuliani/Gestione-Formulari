using Gestione_Formulari.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gestione_Formulari
{    public partial class MainWindow : Window
    {
        private List<MyFile> filesList = new List<MyFile>();
        private string startPath = "";
        private string destPath1 = "";
        private string destPath2 = "";

        public MainWindow()
        {
            InitializeComponent();
            //Ripristino le impostazioni salvate alla chiusura precedente
            Settings s = Settings.Default;
            startPath = s.LastPathStart;
            txtStartPath.Text = startPath;
            CaricaLista(startPath);
            destPath1 = s.LastPathDest1;
            txtDest1Path.Text = destPath1;
            destPath2 = s.LastPathDest2;
            txtDest2Path.Text = destPath2;
            Width = s.FormWidth;
            Height = s.FormHeight;
        }
        private void CaricaLista(string path)
        {
            if (path == "") return;
            filesList.Clear();
            lwStart.ItemsSource = null;
            foreach (string f in System.IO.Directory.GetFiles(path))
            {
                MyFile m = new MyFile();
                m.StartFileName = f.Replace(path, "");
                int indexDot = f.IndexOf(".");
                m.Estensione = f.Substring(indexDot, f.Length - indexDot);
                filesList.Add(m);
            }
            lwStart.ItemsSource = filesList;

        }
        private void btnStartPath_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if(result == System.Windows.Forms.DialogResult.OK)
                {
                    startPath = dialog.SelectedPath + @"\";
                    txtStartPath.Text = startPath;
                    CaricaLista(startPath);
                }
            }
        }
        private void btnDestPath_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            TextBox txt;

            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = true;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    if (b.Name.Contains("1"))
                    {
                        txt = txtDest1Path;
                        destPath1 = dialog.SelectedPath + @"\";
                    }
                    else
                    {
                        txt = txtDest2Path;
                        destPath2 = dialog.SelectedPath + @"\";
                    }
                    txt.Text = dialog.SelectedPath;
                }
            }
        }
        private void lwStart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MyFile f = lwStart.SelectedItem as MyFile;
            Process.Start(startPath + f.StartFileName);
        }
        private void btnSalva_Click(object sender, RoutedEventArgs e)
        {
            int numFiles = 0;
            foreach(MyFile f in filesList)
            {
                if (f.DestFileName != "")
                {
                    numFiles++;
                    string start = startPath + f.StartFileName;
                    string dest = "";

                    if (destPath1 != "")
                    {
                        dest = destPath1 + f.DestFileName + f.Estensione;
                        if (!System.IO.File.Exists(dest))
                            System.IO.File.Copy(start, dest);
                    }
                    if (destPath2 != "")
                    {
                        dest = destPath2 = f.DestFileName + f.Estensione;
                        if (!System.IO.File.Exists(dest))
                            System.IO.File.Copy(start, dest);
                    }
                    bool check = true;
                    while (check)
                    {
                        try
                        {
                            System.IO.File.Delete(start);
                            check = false;
                        }
                        catch(Exception ex)
                        {
                            MessageBoxResult res;
                            res = MessageBox.Show("Errore durante la cancellazione del file.\n\nVerificare che non sia aperto in un'altra applicazione e premere OK...\n\n"+ex.Message, "ERRORE", MessageBoxButton.OKCancel);
                            check = res == MessageBoxResult.OK;
                        }
                    }
                }
            }
            MessageBox.Show(string.Format("Sono stati copiati {0} file", numFiles.ToString()));
            CaricaLista(startPath);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Salvo le impostazioni correnti
            Settings s = Settings.Default;
            s.LastPathStart = startPath;
            s.LastPathDest1 = destPath1;
            s.LastPathDest2 = destPath2;
            s.FormHeight = Height;
            s.FormWidth = Width;
            Settings.Default.Save();
        }
    }
    public class MyFile
    {
        public string StartFileName { get; set; } = "";
        public string DestFileName { get; set; } = "";
        public string Estensione { get; set; } = "";
    }
}