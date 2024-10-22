using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DiffViewerApp
{
    public partial class MainWindow : Window
    {
        private string file1Content = string.Empty;
        private string file2Content = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLoadFile1_Click(object sender, RoutedEventArgs e)
        {
            string fileName = LoadFile(out file1Content);
            txtFile1.Text = fileName;  // Wyświetlamy nazwę pliku
        }

        private void btnLoadFile2_Click(object sender, RoutedEventArgs e)
        {
            string fileName = LoadFile(out file2Content);
            txtFile2.Text = fileName;  // Wyświetlamy nazwę pliku
        }

        private string LoadFile(out string content)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                content = File.ReadAllText(openFileDialog.FileName);
                return openFileDialog.FileName; // Zwracamy nazwę pliku
            }
            content = string.Empty;
            return string.Empty;
        }

        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(file1Content) || string.IsNullOrEmpty(file2Content))
            {
                MessageBox.Show("Please load both files before comparing.");
                return;
            }

            CompareFiles(file1Content, file2Content);
        }

        private void CompareFiles(string file1Content, string file2Content)
        {
            var diffBuilder = new InlineDiffBuilder(new Differ());
            var diffResult = diffBuilder.BuildDiffModel(file1Content, file2Content);

            // Czyścimy RichTextBoxy przed dodaniem nowej zawartości
            rtbFile1.Document.Blocks.Clear();
            rtbFile2.Document.Blocks.Clear();

            foreach (var line in diffResult.Lines)
            {
                if (line.Type == ChangeType.Inserted)
                {
                    // Linia dodana w pliku 2, wyświetlamy ją tylko w pliku 2
                    var paragraphFile2 = new Paragraph(new Run(line.Text) { Background = Brushes.LightGreen });
                    rtbFile2.Document.Blocks.Add(paragraphFile2);
                }
                else if (line.Type == ChangeType.Deleted)
                {
                    // Linia usunięta z pliku 1, wyświetlamy ją tylko w pliku 1
                    var paragraphFile1 = new Paragraph(new Run(line.Text) { Background = Brushes.LightCoral });
                    rtbFile1.Document.Blocks.Add(paragraphFile1);
                }
                else if (line.Type == ChangeType.Unchanged)
                {
                    // Niezmieniona linia, wyświetlamy ją w obu plikach
                    var paragraphFile1 = new Paragraph(new Run(line.Text));
                    var paragraphFile2 = new Paragraph(new Run(line.Text));

                    rtbFile1.Document.Blocks.Add(paragraphFile1);
                    rtbFile2.Document.Blocks.Add(paragraphFile2);
                }
            }
        }
    }
}
