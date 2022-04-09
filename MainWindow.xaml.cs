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

using Microsoft.Win32;

using System.Diagnostics;
using System.IO;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Path = System.IO.Path;
using PdfSharp.Drawing.Layout;

namespace AnnexMarker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string file_path;      //Path to PDF being annexed
        string file_dir;       //Path to directory of PDF file
        string file_title;     //Title to add to header of file and name of new file
        string file_footer;    //Copyright information to add to footer of file.

        XStringFormat title_placement;
        XStringFormat footer_placement;


        public MainWindow()
        {
            InitializeComponent();


            titleRBCenter.IsChecked = true;  //Initialize title placement radio button at center.
            title_placement = XStringFormats.TopCenter;
            footerRBLeft.IsChecked  = true; //Initialize footer placement radio button at left.
            footer_placement = XStringFormats.BottomLeft;
        }

        private void Browse(object sender, RoutedEventArgs e)
        {
            //Open file dialog using context information
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Pdf Files | *.pdf";
            if (openFileDialog.ShowDialog() == true)
            {
                path.Text = openFileDialog.FileName;
            }
        }

        private void Submit(object sender, RoutedEventArgs e)
        {
            ResetTextBoxes(); //Remove error borders from past submissions.

            path.Text = path.Text.Replace(@"\", "/"); //Convert path into proper form if using backslashes.

            //Run file creation script
            if (path.Text.Length <= path.MaxLength && File.Exists(path.Text) && Path.GetExtension(path.Text) == ".pdf"/*&& file is real and file is pdf*/)
            {
                file_path = path.Text.Replace(@"\", "/");
                file_dir = Path.GetDirectoryName(path.Text);

                if (title.Text.Length <= title.MaxLength && title.Text.Length>0)
                {
                    file_title = title.Text;
                    if(footer.Text.Length <= footer.MaxLength)
                    {
                        
                        file_footer = footer.Text;
                        CreateAnnex();
                    }
                    else
                    {
                        errorBox.Text = "Invalid Footer. Cannot Exceed: " + (footer.MaxLength).ToString() + " characters.";
                        footer.BorderBrush = Brushes.Red;
                    }
                }
                else
                {
                    if (title.Text.Length > title.MaxLength)
                    {
                        errorBox.Text = "Invalid Title. Cannot Exceed: " + (title.MaxLength).ToString() + " characters.";
                    }
                    else if(title.Text.Length <= 0)
                    {
                        errorBox.Text = "Invalid Title. Title cannot be blank.";
                    }
                    title.BorderBrush = Brushes.Red;
                }
            }
            else
            {
                errorBox.Text = "Invalid File Path. File path must exist and be PDF.";
                path.BorderBrush = Brushes.Red;
            }
        }

        public void CreateAnnex()
        {
            //Open document to be stamped.
            PdfDocument document = PdfReader.Open(file_path, PdfDocumentOpenMode.Modify);

            //Set fonts and colors
            XBrush brush = XBrushes.Black;
            XFont tFont = new XFont("Arial", 18, XFontStyle.Regular);
            XFont fFont = new XFont("Arial", 12, XFontStyle.Regular);

            int pageNumber = 0;

            foreach (PdfPage page in document.Pages)
            {

                XGraphics gfx = XGraphics.FromPdfPage(page);
                XTextFormatter tf = new XTextFormatter(gfx);

                XRect rectangle = new XRect(0, 0, page.Width, page.Height);

                pageNumber++;
                if (pageNumber == 1)
                {
                    gfx.DrawString(file_title, tFont, brush, rectangle, title_placement);
                }


                XRect footerRect = new XRect(0, 0, page.Width, page.Height);
                gfx.DrawString(file_footer, fFont, brush, footerRect, footer_placement);
            }

            string fileName = file_dir + "/" + file_title + ".pdf";

            if (!File.Exists(fileName) && fileName.Length<255)
            {
                document.Save(fileName);
                Process.Start(fileName);
            }
            else
            {
                //File with same name already exists. Are you sure you want to replace it? (Y/N)
                errorBox.Text = "ERROR: A file with the name: " + file_title + ".pdf already exists in " + file_dir;
            }
        }

        //Set placement of Title based on radio buttons
        private void TitleCheck(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            string placement = (string) rb.Content;

            char placeChar = placement[0];

            switch (placeChar)
            {
                case 'L': title_placement = XStringFormats.TopLeft;
                    break;
                case 'C': title_placement = XStringFormats.TopCenter;
                    break;
                case 'R': title_placement = XStringFormats.TopRight;
                    break;
                default:  title_placement = XStringFormats.TopLeft;
                    break;
            }
        }

        //Set placement of Footer based on radio buttons
        private void FooterCheck(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            string placement = (string)rb.Content;

            char placeChar = placement[0];

            switch (placeChar)
            {
                case 'L': footer_placement = XStringFormats.BottomLeft;
                    break;
                case 'C': footer_placement = XStringFormats.BottomCenter;
                    break;
                case 'R': footer_placement = XStringFormats.BottomRight;
                    break;
                default:  footer_placement = XStringFormats.BottomLeft;
                    break;
            }
        }

        private void ResetTextBoxes()
        {
            Brush brush = Brushes.DarkGray;
            path.BorderBrush    = brush;
            title.BorderBrush   = brush;
            footer.BorderBrush  = brush;

            errorBox.Text = "";
        }

    }
}
