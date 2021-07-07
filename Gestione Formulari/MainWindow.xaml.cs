using Gestione_Formulari.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using iText.Kernel.Pdf;
using iText.Pdfocr;
using iText.Pdfocr.Tesseract4;
using System.Reflection;
using Microsoft.Win32;
using iText.Kernel;
using System.IO;

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
                m.DestFileName = OttieniNomeFile(f);
                int indexDot = f.IndexOf(".");
                m.Estensione = f.Substring(indexDot, f.Length - indexDot);
                filesList.Add(m);
            }
            lwStart.ItemsSource = filesList;
        }
        private CopyResult CopiaFile(string start, string dest)
        {
            if (System.IO.File.Exists(dest))
            {
                MessageBoxResult r = MessageBox.Show("Il file "+ dest +" esiste già.\n\nProseguo con gli altri file?", "ATTENZIONE!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                switch (r)
                {
                    case MessageBoxResult.OK:
                        {
                            return CopyResult.Continue;
                        }
                    case MessageBoxResult.Cancel:
                        {
                            return CopyResult.Cancel;
                        }
                }
            }
            try
            {
                System.IO.File.Copy(start, dest);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Si è verificato un errore durante la copia in " + dest + "\n\nNon posso proseguire.\n\n" + ex.Message, "ERRORE!", MessageBoxButton.OK, MessageBoxImage.Error);
                return CopyResult.Cancel;
            }
            return CopyResult.OK;
        }
        private string OttieniNomeFile(string path)
        {
            string res = "";
            bool success = false;
            List<System.IO.FileInfo> image = new List<System.IO.FileInfo>();
            image.Add(new System.IO.FileInfo(path));
            Tesseract4OcrEngineProperties properties = new Tesseract4OcrEngineProperties();
            //string exePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            while (success)
            {
                try
                {
                    properties.SetPathToTessData(new System.IO.FileInfo(Settings.Default.TessDataPath));
                    success = true;
                }
                catch(Exception ex)
                {
                    OpenFileDialog fd = new OpenFileDialog();
                    fd.Title = "Scegli il file TessData";
                    fd.Filter = "File TessData|*.traineddata";
                    fd.Multiselect = false;
                    fd.ShowDialog();
                    if (fd.FileName != "")
                        Settings.Default.TessDataPath = fd.FileName;
                }
            }
            Tesseract4LibOcrEngine engine = new Tesseract4LibOcrEngine(properties);
            
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(path));

            int numberOfPdfObject = pdfDoc.GetNumberOfPdfObjects();
            for (int i = 1; i <= numberOfPdfObject; i++)
            {
                PdfObject obj = pdfDoc.GetPdfObject(i);
                if (obj != null && obj.IsStream())
                {
                    //todo investiga qui....
                    //obj.GetObjectType();
                    byte[] b = ((PdfStream)obj).GetBytes();
                    if(b!=null)
                    {
                        using (FileStream file = new FileStream(string.Format(@"C:\Users\Carmine\Desktop\Monia\new{0}.jpeg",i), FileMode.Create))
                        {
                            file.Write(b, 0, b.Length);
                        }
                    }
                }
            }

            pdfDoc.Close();


            return res;
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
            List<MyFile> toRemove = new List<MyFile>();

            foreach (MyFile f in filesList)
            {
                if (f.DestFileName != "")
                {
                    string start = startPath + f.StartFileName;
                    string dest1 = "";
                    string dest2 = "";
                    CopyResult copyResult;

                    //Provo ad effettuare la prima copia
                    if (destPath1 != "")
                    {
                        dest1 = destPath1 + f.DestFileName + f.Estensione;
                        copyResult = CopiaFile(start, dest1);
                        if(copyResult == CopyResult.Continue)
                            continue;
                        if(copyResult == CopyResult.Cancel)
                            break;
                    }
                    //Provo ad effettuare la seconda copia
                    if (destPath2 != "")
                    {
                        dest2 = destPath2 + f.DestFileName + f.Estensione;
                        copyResult = CopiaFile(start, dest2);
                        if (copyResult != CopyResult.OK)
                        {
                            //Se la seconda copia non va a buon fine, elimino il file della prima copia
                            System.IO.File.Delete(dest1);
                        }
                        if (copyResult == CopyResult.Continue)
                            continue;
                        if (copyResult == CopyResult.Cancel)
                            break;
                    }
                    //Cancello il file di origine
                    bool check = true;
                    while (check)
                    {
                        try
                        {
                            //Cancello il file dalla cartella di partenza
                            System.IO.File.Delete(start);
                            //Aggiungo il file alla lista di quelli da cancellare
                            toRemove.Add(f);
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
                numFiles++;
            }
            MessageBox.Show(string.Format("Sono stati copiati {0} file", numFiles.ToString()));
            //CaricaLista(startPath);
            //Rimuovo dalla lista principale quelli che ho effettivamente copiato
            foreach (MyFile removing in toRemove)
                filesList.Remove(removing);
            lwStart.ItemsSource = null;
            lwStart.ItemsSource = filesList;
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
    public enum CopyResult
    {
        OK, Continue, Cancel
    }
}