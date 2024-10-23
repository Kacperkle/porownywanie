using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DiffViewerApp
{
    public partial class MainWindow : Window
    {
        private string fileOrFolder1Content = string.Empty;
        private string fileOrFolder2Content = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLoadFileOrFolder1_Click(object sender, RoutedEventArgs e)
        {
            string path = LoadFileOrFolder();
            if (Directory.Exists(path))
            {
                txtFileOrFolder1.Text = path;
                LoadDirectoryContent(path, rtbFileOrFolder1);
            }
            else if (File.Exists(path))
            {
                txtFileOrFolder1.Text = path;
                fileOrFolder1Content = File.ReadAllText(path);
                LoadFileContent(fileOrFolder1Content, rtbFileOrFolder1);
            }
        }

        private void btnLoadFileOrFolder2_Click(object sender, RoutedEventArgs e)
        {
            string path = LoadFileOrFolder();
            if (Directory.Exists(path))
            {
                txtFileOrFolder2.Text = path;
                LoadDirectoryContent(path, rtbFileOrFolder2);
            }
            else if (File.Exists(path))
            {
                txtFileOrFolder2.Text = path;
                fileOrFolder2Content = File.ReadAllText(path);
                LoadFileContent(fileOrFolder2Content, rtbFileOrFolder2);
            }
        }

        private string LoadFileOrFolder()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select File or Folder",
                Filter = "All files (*.*)|*.*",
                Multiselect = false
            };

            bool? result = dialog.ShowDialog();
            return result == true ? dialog.FileName : string.Empty;
        }

        private void LoadDirectoryContent(string directoryPath, RichTextBox richTextBox)
        {
            richTextBox.Document.Blocks.Clear();
            var files = Directory.GetFiles(directoryPath);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var paragraph = new Paragraph(new Run(fileName));
                richTextBox.Document.Blocks.Add(paragraph);
            }
        }

        private void LoadFileContent(string content, RichTextBox richTextBox)
        {
            richTextBox.Document.Blocks.Clear();
            var paragraph = new Paragraph(new Run(content));
            richTextBox.Document.Blocks.Add(paragraph);
        }

        private void btnCompareFiles_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(fileOrFolder1Content) || string.IsNullOrEmpty(fileOrFolder2Content))
            {
                MessageBox.Show("Please load both files before comparing.");
                return;
            }
            CompareFiles(fileOrFolder1Content, fileOrFolder2Content);
        }

        private void CompareFiles(string file1Content, string file2Content)
        {
            var diffBuilder = new InlineDiffBuilder(new Differ());
            var diffResult = diffBuilder.BuildDiffModel(file1Content, file2Content);

            ShowDifferencesWithContext(diffResult, 3);
        }

        private void ShowDifferencesWithContext(DiffPaneModel diffResult, int contextLines)
        {
            rtbFileOrFolder1.Document.Blocks.Clear();
            rtbFileOrFolder2.Document.Blocks.Clear();

            for (int i = 0; i < diffResult.Lines.Count; i++)
            {
                var line = diffResult.Lines[i];
                if (line.Type == ChangeType.Inserted || line.Type == ChangeType.Deleted || line.Type == ChangeType.Modified)
                {  
                    for (int j = Math.Max(0, i - contextLines); j < i; j++)
                    {
                        AddLine(diffResult.Lines[j], rtbFileOrFolder1, rtbFileOrFolder2);
                    }
                    AddLine(line, rtbFileOrFolder1, rtbFileOrFolder2);
                    for (int j = i + 1; j <= Math.Min(i + contextLines, diffResult.Lines.Count - 1); j++)
                    {
                        AddLine(diffResult.Lines[j], rtbFileOrFolder1, rtbFileOrFolder2);
                    }
                    i += contextLines;
                }
            }
        }

        private void AddLine(DiffPiece line, RichTextBox rtb1, RichTextBox rtb2)
        {
            if (line.Type == ChangeType.Inserted)
            {
                var paragraphFile2 = new Paragraph(new Run(line.Text) { Background = System.Windows.Media.Brushes.LightGreen });
                rtb2.Document.Blocks.Add(paragraphFile2);
            }
            else if (line.Type == ChangeType.Deleted)
            {
                var paragraphFile1 = new Paragraph(new Run(line.Text) { Background = System.Windows.Media.Brushes.LightCoral });
                rtb1.Document.Blocks.Add(paragraphFile1);
            }
            else if (line.Type == ChangeType.Unchanged)
            {
                var paragraphFile1 = new Paragraph(new Run(line.Text));
                var paragraphFile2 = new Paragraph(new Run(line.Text));
                rtb1.Document.Blocks.Add(paragraphFile1);
                rtb2.Document.Blocks.Add(paragraphFile2);
            }
        }

        private void btnCompareDirectories_Click(object sender, RoutedEventArgs e)
        {
            string folder1 = SelectFolder();
            string folder2 = SelectFolder();

            if (string.IsNullOrEmpty(folder1) || string.IsNullOrEmpty(folder2))
            {
                MessageBox.Show("Both folders must be selected.");
                return;
            }

            CompareDirectories(folder1, folder2);
        }

        private string SelectFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();

            return result == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : string.Empty;
        }

        private void CompareDirectories(string folder1, string folder2)
        {
            var files1 = Directory.GetFiles(folder1);
            var files2 = Directory.GetFiles(folder2);
            var commonFiles = files1.Select(Path.GetFileName).Intersect(files2.Select(Path.GetFileName));

            rtbFileOrFolder1.Document.Blocks.Clear();
            rtbFileOrFolder2.Document.Blocks.Clear();

            foreach (var fileName in commonFiles)
            {
                var file1 = Path.Combine(folder1, fileName);
                var file2 = Path.Combine(folder2, fileName);

                var content1 = File.ReadAllText(file1);
                var content2 = File.ReadAllText(file2);

                var diffBuilder = new InlineDiffBuilder(new Differ());
                var diffResult = diffBuilder.BuildDiffModel(content1, content2);
                ShowDifferencesWithContext(diffResult, 3);
            }
            var onlyInFolder1 = files1.Select(Path.GetFileName).Except(files2.Select(Path.GetFileName));
            var onlyInFolder2 = files2.Select(Path.GetFileName).Except(files1.Select(Path.GetFileName));

            foreach (var fileName in onlyInFolder1)
            {
                var paragraph = new Paragraph(new Run($"File only in {folder1}: {fileName}") { Background = System.Windows.Media.Brushes.Yellow });
                rtbFileOrFolder1.Document.Blocks.Add(paragraph);
            }

            foreach (var fileName in onlyInFolder2)
            {
                var paragraph = new Paragraph(new Run($"File only in {folder2}: {fileName}") { Background = System.Windows.Media.Brushes.Yellow });
                rtbFileOrFolder2.Document.Blocks.Add(paragraph);
            }
        }
    }
}
