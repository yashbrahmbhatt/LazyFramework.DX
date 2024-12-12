using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace LazyFramework.DX.Services.Athena
{
    public partial class FilePicker : UserControl
    {
        public static readonly DependencyProperty SelectedFilePathProperty =
            DependencyProperty.Register("SelectedFilePath", typeof(string), typeof(FilePicker), new PropertyMetadata(string.Empty));

        public string SelectedFilePath
        {
            get => (string)GetValue(SelectedFilePathProperty);
            set => SetValue(SelectedFilePathProperty, value);
        }

        public FilePicker()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePath = openFileDialog.FileName;
            }
        }
    }
}
