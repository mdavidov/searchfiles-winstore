using SearchFiles.Common;
using SearchFiles.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

namespace SearchFiles.Data
{
    /// <summary>
    /// Base class for <see cref="FileInfoDataItem"/> and <see cref="FileInfoDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class FileInfoDataCommon : SearchFiles.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");
        public static string SUB_HEAD = "{SF}\\";
        public static int SUB_IDX = SUB_HEAD.Length;

        public FileInfoDataCommon(StorageFile file, String uniqueId, Image image)
        {
            _file = file;
            _uniqueId = uniqueId;
            _image = image;

            _fileName = "";
            _subPath = SUB_HEAD;
            _size = "";
            _dateModified = "";
            _extension = "";
            _contentType = "";
            _description = "";
        }

        public static async Task<FileInfoDataItem> Create(StorageFile file, String uniqueId, Image image, FileInfoDataGroup group)
        {
            try
            {
                var obj = new FileInfoDataItem(file, uniqueId, image, group);
                if (file != null)
                {
                    obj.FileName = file.Name;

                    if (!string.IsNullOrEmpty(ItemsPage.Current.TopFolder.Path))
                    {
                        int topFolderLen = ItemsPage.Current.TopFolder.Path.Length;
                        int lastSepIdx = file.Path.LastIndexOf('\\');
                        Debug.Assert(lastSepIdx >= 0 && lastSepIdx < file.Path.Length && lastSepIdx >= topFolderLen);
                        if (lastSepIdx >= 0 && lastSepIdx < file.Path.Length && lastSepIdx > topFolderLen)
                            obj.SubPath += file.Path.Substring(topFolderLen + 1, lastSepIdx - topFolderLen - 1);
                    }
                    else
                    {
                        int idx = !string.IsNullOrEmpty(ItemsPage.Current.TopFolder.Name) ?
                                  file.Path.IndexOf(ItemsPage.Current.TopFolder.Name, StringComparison.OrdinalIgnoreCase) : -1;
                        if (idx >= 0)
                        {
                            int idxSubPath = idx + ItemsPage.Current.TopFolder.Name.Length + 1;
                            int idxFileName = Math.Max(file.Path.LastIndexOf("\\"), 0);
                            if (idxFileName > idxSubPath)
                                obj.SubPath += file.Path.Substring(idxSubPath, idxFileName - idxSubPath);
                        }
                        else
                        {
                            idx = file.Path.LastIndexOf("\\");
                            if (idx > 0)
                                obj.SubPath = file.Path.Substring(0, idx);
                            else
                                obj.SubPath = file.Path;
                        }
                    }

                    BasicProperties props = await file.GetBasicPropertiesAsync();
                    if (props != null)
                    {
                        obj.Size = Util.SizeToString(props.Size, "B");
                        ItemsPage.Current.IncrTotalBytes(props.Size);
                    }

                    obj.DateModified = Util.DateTime_ToString(props.DateModified, Util.EDateTimeFormat.G);
                    obj.Extension = file.FileType;
                    obj.ContentType = file.ContentType;

#if DEBUG
                    {
                        string ext = (file.FileType == String.Empty) ? "EMPTY" : file.FileType;
                        string contyp = (file.ContentType == String.Empty) ? "EMPTY" : file.ContentType;

                        if (!ItemsPage.Current._ExtContyp.ContainsKey(ext))
                        {
                            //Debug.WriteLine(ext + " - " + contyp);
                            ItemsPage.Current._ExtContyp.Add(ext, contyp);
                        }
                        //else if (file.FileType == String.Empty)
                        //    Debug.WriteLine(ext + " - " + contyp);

                        //if (!ItemsPage.Current._ContypExt.ContainsKey(contType))
                        //{
                        //    Debug.WriteLine(contType + " - " + ext);
                        //    ItemsPage.Current._ContypExt.Add(contType, ext);
                        //}
                        //else if (file.ContentType == String.Empty)
                        //    Debug.WriteLine(contType + " - " + ext);
                    }
#endif
                }
                return obj;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return null; }
        }

        private StorageFile _file = null;
        public  StorageFile File
        {
            get { return _file; }
            set { _file = value; }
        }

        //private StorageFolder _folder = null;
        //public StorageFolder Folder
        //{
        //    get { return _folder; }
        //    set { _folder = value; }
        //}

        private Image _image;
        public Image Image
        {
            get { return _image; }
            set { _image = value; }
        }
        
        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _fileName = string.Empty;
        public string FileName
        {
            get { return this._fileName; }
            set { this.SetProperty(ref this._fileName, value); }
        }

        private string _subPath = string.Empty;
        public string SubPath
        {
            get { return this._subPath; }
            set { this.SetProperty(ref this._subPath, value); }
        }

        private string _size = string.Empty;
        public string Size
        {
            get { return this._size; }
            set { this.SetProperty(ref this._size, value); }
        }

        private string _dateModified = string.Empty;
        public string DateModified
        {
            get { return this._dateModified; }
            set { this.SetProperty(ref this._dateModified, value); }
        }

        private string _extension = string.Empty;
        public string Extension
        {
            get { return this._extension; }
            set { this.SetProperty(ref this._extension, value); }
        }

        private string _contentType = string.Empty;
        public string ContentType
        {
            get { return this._contentType; }
            set { this.SetProperty(ref this._contentType, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        //private ImageSource _image = null;
        //private String _imagePath = null;
        public ImageSource ImageSource
        {
            get
            {
                //if (this._image == null && this._imagePath != null)
                //{
                //    this._image = new BitmapImage(new Uri(FileInfoDataCommon._baseUri, this._imagePath));
                //}
                //return this._image;

                if (this._image != null) 
                    return this._image.Source;
                else
                    return new BitmapImage();
            }

            set
            {
                //this._imagePath = null;
                //this.SetProperty(ref this._image, value);
                //this.SetProperty(ref this._image.Source, value);

                if (this._image != null)
                    this._image.Source = value;
            }
        }

        public void SetImage(Image image)
        {
            //this._image = null;
            //this._imagePath = path;
            this._image = image;
            this.OnPropertyChanged("ImageSource");
        }

        public override string ToString()
        {
            return this.FileName;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class FileInfoDataItem : FileInfoDataCommon
    {
        public FileInfoDataItem(StorageFile file, String uniqueId, Image image, FileInfoDataGroup group)
            : base(file, uniqueId, image)
        {
            this._content = ""; //content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private FileInfoDataGroup _group;
        public FileInfoDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class FileInfoDataGroup : FileInfoDataCommon
    {
        public FileInfoDataGroup(String uniqueId, String title, String subtitle, Image image, String description)
            : base(null, uniqueId, image)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<FileInfoDataItem> _items = new ObservableCollection<FileInfoDataItem>();
        public ObservableCollection<FileInfoDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<FileInfoDataItem> _topItem = new ObservableCollection<FileInfoDataItem>();
        public ObservableCollection<FileInfoDataItem> TopItems
        {
            get { return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// FileInfoDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class FileInfoDataSource
    {
        private static FileInfoDataSource _dataSource = new FileInfoDataSource();
        public static FileInfoDataSource Object
        {
            get { return _dataSource; }
        }
        
        //public static FileInfoDataSource _dataSource = new FileInfoDataSource();

        private ObservableCollection<FileInfoDataGroup> _allGroups = new ObservableCollection<FileInfoDataGroup>();
        public ObservableCollection<FileInfoDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<FileInfoDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _dataSource.AllGroups;
        }

        public static FileInfoDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _dataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static FileInfoDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _dataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private FileInfoDataGroup _successGroup = null;
        public FileInfoDataGroup SuccessGroup
        {
            get { return _successGroup; }
        }

        private FileInfoDataGroup _errorGroup = null;
        public FileInfoDataGroup ErrorGroup
        {
            get { return _errorGroup; }
        }

        public FileInfoDataSource()
        {
            //String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
            //            "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat.");
 
            _successGroup = new FileInfoDataGroup("SuccessGroup",
                    "SUCCESSFULL ",
                    "Successfully Shredded Files ",
                    null, //"Assets/DarkGray.png",
                    "List of successfully shredded files.");
            this.AllGroups.Add(_successGroup);

            //var item = new FileInfoDataItem("FILE NAME",
            //                                "FILE NAME",
            //                                "",
            //                                null,
            //                                "",
            //                                "FULL PATH",
            //                                _successGroup);
            //_successGroup.Items.Add(item);

            _errorGroup = new FileInfoDataGroup("ErrorGroup",
                    "FAILED",
                    "Not-Shredded Files ",
                    null, // imageOrig
                    "List of files that were not shredded, or were only partially shredded. ");
            this.AllGroups.Add(_errorGroup);
        }

        //public void AddSuccessItem(StorageFile file, string fileName, string fullPath)
        //{
        //    try
        //    {
        //        if (_successGroup == null) {
        //            return;
        //        }
        //        var item = new FileInfoDataItem(file,
        //                                        fileName,
        //                                        fileName,
        //                                        "",
        //                                        null,
        //                                        "",
        //                                        fullPath,
        //                                        _successGroup);
        //        _successGroup.Items.Add(item);
        //    }
        //    catch (Exception) { }
        //}
    }
}
