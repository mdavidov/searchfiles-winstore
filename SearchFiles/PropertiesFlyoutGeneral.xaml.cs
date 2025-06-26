using SearchFiles.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SearchFiles
{
    public sealed partial class PropertiesFlyoutGeneral : SettingsFlyout, IFileProperties
    {
        public TextBox ContentType { get { return contentType; } }
        public TextBox DocumentTitle { get { return documentTitle; } }
        public TextBox Authors { get { return authors; } }
        public TextBox Comments { get { return comments; } }
        public TextBox Keywords { get { return keywords; } }
        public TextBox DateModified { get { return dateModified; } }
        public TextBox DateCreated { get { return dateCreated; } }
        public TextBox Attributes { get { return attributes; } }
        public TextBox FolderRelativeId { get { return folderRelativeId;  } }

        public void HideContentType() { contentType.Visibility = Visibility.Collapsed; }
        public void HideDocumentTitle() { documentTitle.Visibility = Visibility.Collapsed; }
        public void HideAuthors() { authors.Visibility = Visibility.Collapsed; }
        public void HideComments() { comments.Visibility = Visibility.Collapsed; }
        public void HideKeywords() { keywords.Visibility = Visibility.Collapsed; }
        public void HideDateModified() { dateModified.Visibility = Visibility.Collapsed; }
        public void HideDateCreated() { dateCreated.Visibility = Visibility.Collapsed; }
        public void HideAttributes() { attributes.Visibility = Visibility.Collapsed; }
        public void HideFolderRelativeId() { folderRelativeId.Visibility = Visibility.Collapsed; }

        public void SetPropsKindTitle(string propsKindTitle_) { this.propsKindTitle.Text = propsKindTitle_; }

        public PropertiesFlyoutGeneral()
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
