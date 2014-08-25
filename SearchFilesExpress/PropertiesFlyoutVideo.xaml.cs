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

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace SearchFiles
{
    public sealed partial class PropertiesFlyoutVideo : SettingsFlyout, IFileProperties
    {
        public TextBox VideoTitle{ get { return videoTitle; } }
        public TextBox Duration { get { return duration; } }
        public TextBox Dimensions { get { return dimensions; } }
        public TextBox Directors { get { return directors; } }
        public TextBox Writers { get { return writers; } }
        public TextBox Producers { get { return producers; } }
        public TextBox Publisher { get { return publisher; } }
        public TextBox Year{ get { return year; } }
        public TextBox Rating{ get { return rating; } }
        public TextBox Bitrate{ get { return bitrate; } }
        public TextBox Keywords{ get { return keywords; } }
        public TextBox Latitude{ get { return latitude; } }
        public TextBox Longitude{ get { return longitude; } }

        public void HideVideoTitle() { videoTitle.Visibility = Visibility.Collapsed; }
        public void HideDuration() { duration.Visibility = Visibility.Collapsed; }
        public void HideDimensions() { dimensions.Visibility = Visibility.Collapsed; }
        public void HideDirectors() { directors.Visibility = Visibility.Collapsed; }
        public void HideWriters() { writers.Visibility = Visibility.Collapsed; }
        public void HideProducers() { producers.Visibility = Visibility.Collapsed; }
        public void HidePublisher() { publisher.Visibility = Visibility.Collapsed; }
        public void HideYear() { year.Visibility = Visibility.Collapsed; }
        public void HideRating() { rating.Visibility = Visibility.Collapsed; }
        public void HideBitrate()   { bitrate.Visibility = Visibility.Collapsed; }
        public void HideKeywords()  { keywords.Visibility = Visibility.Collapsed; }
        public void HideLatitude()  { latitude.Visibility = Visibility.Collapsed; }
        public void HideLongitude() { longitude.Visibility = Visibility.Collapsed; }

        public PropertiesFlyoutVideo()
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
