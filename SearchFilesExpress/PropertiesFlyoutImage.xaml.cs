using SearchFiles.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SearchFiles
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PropertiesFlyoutImage : SettingsFlyout, IFileProperties
    {
        public TextBox DateTaken { get { return dateTaken; } }
        public TextBox Dimensions { get { return dimensions; } }
        public TextBox Latitude { get { return latitude; } }
        public TextBox Longitude { get { return longitude; } }
        public TextBox ImgTitle { get { return imgTitle; } }
        public TextBox Camera { get { return camera; } }

        public PropertiesFlyoutImage()
        {
            this.InitializeComponent();
        }

        public string FileName { set { fileName.Text = value; } }
        public string FileType { set { fileType.Text = value; } }
        public string FileSize { set { fileSize.Text = value; } }
        public string FileLocation { set { fileLocation.Text = value; } }
        public string ContainingFolder { set { containingFolder.Text = value; } }
        public string SelectedFolder { set { selectedFolder.Text = value; } }
        public string AbsolutePath { set { absolutePath.Text = value; } }
        public ImageSource FileImage { set { fileImage.Source = value; } }

        public void HideFileName() { fileName.Visibility = Visibility.Collapsed; }
        public void HideFileType() { fileType.Visibility = Visibility.Collapsed; }
        public void HideFileSize() { fileSize.Visibility = Visibility.Collapsed; }
        public void HideFileLocation() { fileLocation.Visibility = Visibility.Collapsed; }
        public void HideContainingFolder() { containingFolder.Visibility = Visibility.Collapsed; }
        public void HideSelectedFolder() { selectedFolder.Visibility = Visibility.Collapsed; }
        public void HideAbsolutePath() { absolutePath.Visibility = Visibility.Collapsed; }
        public void HideFileImage() { fileImage.Visibility = Visibility.Collapsed; }
    }
}
