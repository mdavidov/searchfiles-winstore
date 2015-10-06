using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SearchFiles.Common
{
    public interface IFileProperties
    {
        string FileName { set; }
        string FileType { set; }
        string FileSize { set; }
        string FileLocation { set; }
        string ContainingFolder { set; }
        string SelectedFolder { set; }
        string AbsolutePath { set; }
        ImageSource FileImage { set; }

        void HideFileName();
        void HideFileType();
        void HideFileSize();
        void HideFileLocation();
        void HideContainingFolder();
        void HideSelectedFolder();
        void HideAbsolutePath();
        void HideFileImage();
    }

    public class Util
    {
        public static double bKILO = 1024;
        public static double bMEGA = bKILO * bKILO;
        public static double bGIGA = bKILO * bMEGA;
        public static double bTERA = bKILO * bGIGA;
        public static double bPETA = bKILO * bTERA;

        public static string SizeToString(double number, string unit)
        {
            try
            {
                string humanReadable = "";

                if (Math.Abs(number) < 0.01)
                {
                    humanReadable = "0 " + unit;
                    return humanReadable;
                }

                double nbr = number;
                string unt = unit;

                if (number >= bPETA)
                {
                    nbr /= bPETA;
                    unt = "P" + unit;
                }
                else if (number >= bTERA)
                {
                    nbr /= bTERA;
                    unt = "T" + unit;
                }
                else if (number >= bGIGA)
                {
                    nbr /= bGIGA;
                    unt = "G" + unit;
                }
                else if (number >= bMEGA)
                {
                    nbr /= bMEGA;
                    unt = "M" + unit;
                }
                else if (number >= bKILO)
                {
                    nbr /= bKILO;
                    unt = "K" + unit;
                }

                if (unt.StartsWith(unit))
                {
                    humanReadable = string.Format("{0:N0} {1}", nbr, unt);
                }
                else if (nbr >= 100)
                {
                    humanReadable = string.Format("{0:N0} {1}", nbr, unt);
                }
                else if (nbr >= 10)
                {
                    humanReadable = string.Format("{0:N1} {1}", nbr, unt);
                }
                else
                {
                    humanReadable = string.Format("{0:N2} {1}", nbr, unt);
                }
                return humanReadable;
            }
            catch (Exception) { return ""; }
        }

        public static double dKILO = 1000;
        public static double dMEGA = dKILO * dKILO;
        public static double dGIGA = dKILO * dMEGA;
        public static double dTERA = dKILO * dGIGA;
        public static double dPETA = dKILO * dTERA;

        public static string NumberToString(double number, string unit)
        {
            try
            {
                string humanReadable = "";

                if (Math.Abs(number) < 0.01)
                {
                    humanReadable = "0 " + unit;
                    return humanReadable;
                }

                double nbr = number;
                string unt = unit;

                if (number >= dPETA)
                {
                    nbr /= dPETA;
                    unt = "P" + unit;
                }
                else if (number >= dTERA)
                {
                    nbr /= dTERA;
                    unt = "T" + unit;
                }
                else if (number >= dGIGA)
                {
                    nbr /= dGIGA;
                    unt = "G" + unit;
                }
                else if (number >= dMEGA)
                {
                    nbr /= dMEGA;
                    unt = "M" + unit;
                }
                else if (number >= dKILO)
                {
                    nbr /= dKILO;
                    unt = "K" + unit;
                }

                if (unt.StartsWith(unit))
                {
                    humanReadable = string.Format("{0:N0} {1}", nbr, unt);
                }
                else if (nbr >= 100)
                {
                    humanReadable = string.Format("{0:N1} {1}", nbr, unt);
                }
                else if (nbr >= 10)
                {
                    humanReadable = string.Format("{0:N2} {1}", nbr, unt);
                }
                else
                {
                    humanReadable = string.Format("{0:N3} {1}", nbr, unt);
                }
                return humanReadable;
            }
            catch (Exception) { return ""; }
        }

        public static async Task<bool> FileExists(StorageFile file)
        {
            try
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    return fileStream != null;
                }
            }
            catch (FileNotFoundException) { return false; }
            catch (Exception ex) { ItemsPage.ProcessException(ex, "Util.FileExists: "); return false; }
        }

        public static bool IsEmpty(IBuffer b)
        {
            return (b == null || b.Length <= 0);
        }

        public static Image GetImage(StorageItemThumbnail thumbnail)
        {
            try
            {
                if (thumbnail == null) return null;

                BitmapImage bmpImage = new BitmapImage();
                bmpImage.SetSource(thumbnail);
                Image image = new Image();
                image.Source = bmpImage;
                return image;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return null; }
        }

        public static async Task DelTempFiles()
        {
            try
            {
                StorageFolder folder = ApplicationData.Current.TemporaryFolder;
                if (folder == null) return;
                var files = await folder.GetFilesAsync();
                if (files == null) return;
                foreach (StorageFile file in files)
                    await file.DeleteAsync();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public static string LatOrLong_ToString(double? latOrLong)
        {
            string latOrLongStr = "";
            try
            {
                if (latOrLong.HasValue)
                {
                    double val = latOrLong.Value;
                    double latDeg = Math.Floor(val);
                    latOrLongStr += latDeg.ToString() + "deg ";
                    double latMin = Math.Floor((Math.Abs(val) - latDeg) * 60);
                    latOrLongStr += latMin.ToString() + "' ";
                    latOrLongStr += ((Math.Abs(val) - latDeg - latMin / 60) * 3600).ToString() + "\" ";
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return latOrLongStr;
        }

        public async static Task FillFileProperties(IFileProperties fileProps, StorageFile file, StorageFolder topFolder, string fileSubPath, BasicProperties basicProps)
        {
            try
            {
                fileProps.FileName = file.Name;
                fileProps.AbsolutePath = file.Path;

                fileProps.SelectedFolder = Util.FolderPath(topFolder);

                if (!string.IsNullOrEmpty(fileSubPath))
                    fileProps.ContainingFolder = fileSubPath;
                else
                    fileProps.ContainingFolder = ""; //fileProps.HideContainingFolder();

                string ftype = GetFileTypeText(file, false);
                if (!string.IsNullOrEmpty(ftype))
                    fileProps.FileType = ftype;
                else
                    fileProps.HideFileType();

                if (basicProps != null)
                    fileProps.FileSize = Util.SizeToString(basicProps.Size, "B");
                else
                    fileProps.HideFileSize();

                if (!string.IsNullOrEmpty(file.Provider.DisplayName))
                {
                    string flocation = file.Provider.DisplayName;
                    if (!string.IsNullOrEmpty(file.Provider.Id) && file.Provider.DisplayName.IndexOf(file.Provider.Id, StringComparison.OrdinalIgnoreCase) <= -1)
                        flocation += " (" + file.Provider.Id.Substring(0, 1).ToUpper() + file.Provider.Id.Substring(1) + ") ";
                    fileProps.FileLocation = flocation;
                }
                else if (!string.IsNullOrEmpty(file.Provider.Id))
                    fileProps.FileLocation = file.Provider.Id;
                else
                    fileProps.HideFileLocation();

                StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.DocumentsView, 260, ThumbnailOptions.None);
                Image image = Util.GetImage(thumbnail);
                if (image != null)
                    fileProps.FileImage = image.Source;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public async static Task FillAudioProperties(PropertiesFlyoutAudio flyout, MusicProperties musicProps, StorageFile file)
        {
            try
            {
                if (!string.IsNullOrEmpty(musicProps.AlbumArtist))
                    flyout.Artist.Text = musicProps.AlbumArtist;
                else if (!string.IsNullOrEmpty(musicProps.Artist))
                    flyout.Artist.Text = musicProps.Artist;
                else
                {
                    string authors = await GetDocAuthors(file);
                    if (!string.IsNullOrEmpty(authors))
                        flyout.Artist.Text = authors;
                    else
                        flyout.HideArtist();
                }

                if (!string.IsNullOrEmpty(musicProps.Album))
                    flyout.Album.Text = musicProps.Album;
                else
                    flyout.HideAlbum();
                
                if (!string.IsNullOrEmpty(musicProps.Title))
                {
                    string track = "";
                    if (musicProps.TrackNumber > 0)
                        track += musicProps.TrackNumber.ToString("D02") + " ";
                    if (!string.IsNullOrEmpty(musicProps.Subtitle))
                        flyout.TrackTitle.Text = track + musicProps.Title + " - " + musicProps.Subtitle;
                    else
                        flyout.TrackTitle.Text = track + musicProps.Title;
                }
                else
                {
                    string docTitle = await GetDocTitle(file);
                    if (!string.IsNullOrEmpty(docTitle))
                        flyout.TrackTitle.Text = docTitle;
                    else
                        flyout.HideTrackTitle();
                }

                if (musicProps.Duration.TotalSeconds > 0)
                    flyout.Duration.Text = TimeSpan_ToString(musicProps.Duration);
                else
                    flyout.HideDuration();

                if (musicProps.Composers.Count >= 1)
                    flyout.Composers.Text += NameCreditsStr("", musicProps.Composers);
                else
                    flyout.HideComposers();

                if (musicProps.Conductors.Count >= 1)
                    flyout.Conductors.Text += NameCreditsStr("", musicProps.Conductors);
                else
                    flyout.HideConductors();

                if (musicProps.Writers.Count >= 1)
                    flyout.Writers.Text = NameCreditsStr("", musicProps.Writers);
                else
                    flyout.HideWriters();

                if (musicProps.Producers.Count >= 1)
                    flyout.Producers.Text = NameCreditsStr("", musicProps.Producers);
                else
                    flyout.HideProducers();

                if (!string.IsNullOrEmpty(musicProps.Publisher))
                    flyout.Publisher.Text = musicProps.Publisher;
                else
                    flyout.HidePublisher();

                if (musicProps.Genre.Count >= 1)
                    flyout.Genres.Text = NameCreditsStr("", musicProps.Genre);
                else
                    flyout.HideGenres();

                if (musicProps.Year > 0)
                    flyout.Year.Text = musicProps.Year.ToString();
                else
                    flyout.HideYear();

                if (musicProps.Rating > 0)
                {
                    uint starRating = (musicProps.Rating == 0) ? 0 : (uint)Math.Round((double)musicProps.Rating / 25.0) + 1;
                    flyout.Rating.Text = starRating.ToString() + "*";
                }
                else
                    flyout.HideRating();

                if (musicProps.Bitrate > 0)
                    flyout.Bitrate.Text = Util.NumberToString(musicProps.Bitrate, "bps");
                else
                    flyout.HideBitrate();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public static void FillImageProperties(PropertiesFlyoutImage flyout, ImageProperties imageProps, StorageFile file, BasicProperties basicProps)
        {
            try
            {
                if (imageProps.DateTaken.Year > 1700)
                    flyout.DateTaken.Text = DateTime_ToString(imageProps.DateTaken, EDateTimeFormat.G);
                else if (basicProps != null)
                    flyout.DateTaken.Text = DateTime_ToString(basicProps.DateModified, EDateTimeFormat.G);
                else
                    flyout.DateTaken.Visibility = Visibility.Collapsed;

                if (imageProps.Width > 0 && imageProps.Height > 0)
                    flyout.Dimensions.Text = imageProps.Width.ToString() + " x " + imageProps.Height.ToString() + " pixels";
                else
                    flyout.Dimensions.Visibility = Visibility.Collapsed;

                // IMPORTANT: Need GeoCoordinate class from System.Device.Location namespace (but not avail on WinRT);
                // Not suitable: Windows.Devices.Geolocation namespace;
                // GeoCoordinate geoCoord = new GeoCoordinate();
                if (imageProps.Latitude.HasValue)
                    flyout.Latitude.Text = Util.LatOrLong_ToString(imageProps.Latitude);
                else
                    flyout.Latitude.Visibility = Visibility.Collapsed;
                if (imageProps.Longitude.HasValue)
                    flyout.Longitude.Text = Util.LatOrLong_ToString(imageProps.Longitude);
                else
                    flyout.Longitude.Visibility = Visibility.Collapsed;

                if (!string.IsNullOrEmpty(imageProps.Title))
                    flyout.ImgTitle.Text = imageProps.Title;
                else
                    flyout.ImgTitle.Visibility = Visibility.Collapsed;

                if (!string.IsNullOrEmpty(imageProps.CameraManufacturer))
                {
                    flyout.Camera.Text = imageProps.CameraManufacturer;
                    if (!string.IsNullOrEmpty(imageProps.CameraModel))
                        flyout.Camera.Text += " " + imageProps.CameraModel;
                }
                else
                    flyout.Camera.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public static void FillVideoProperties(PropertiesFlyoutVideo flyout, VideoProperties videoProps)
        {
            try
            {
                if (!string.IsNullOrEmpty(videoProps.Title))
                    flyout.VideoTitle.Text = videoProps.Title;
                else if (!string.IsNullOrEmpty(videoProps.Subtitle))
                    flyout.VideoTitle.Text = videoProps.Subtitle;
                else
                    flyout.HideVideoTitle();

                if (videoProps.Duration.TotalSeconds > 0)
                    flyout.Duration.Text = TimeSpan_ToString(videoProps.Duration);
                else
                    flyout.HideDuration();

                if (videoProps.Directors.Count >= 1)
                    flyout.Directors.Text += NameCreditsStr("", videoProps.Directors);
                else
                    flyout.HideDirectors();

                if (videoProps.Writers.Count >= 1)
                    flyout.Writers.Text = NameCreditsStr("", videoProps.Writers);
                else
                    flyout.HideWriters();

                if (videoProps.Producers.Count >= 1)
                    flyout.Producers.Text = NameCreditsStr("", videoProps.Producers);
                else
                    flyout.HideProducers();

                if (!string.IsNullOrEmpty(videoProps.Publisher))
                    flyout.Publisher.Text = videoProps.Publisher;
                else
                    flyout.HidePublisher();

                if (videoProps.Width > 0 && videoProps.Height > 0)
                    flyout.Dimensions.Text = videoProps.Width.ToString() + " x " + videoProps.Height.ToString() + " pixels";
                else
                    flyout.HideDimensions();

                if (videoProps.Year > 0)
                    flyout.Year.Text = videoProps.Year.ToString();
                else
                    flyout.HideYear();

                if (videoProps.Rating > 0)
                {
                    uint starRating = (videoProps.Rating == 0) ? 0 : (uint)Math.Round((double)videoProps.Rating / 25.0) + 1;
                    flyout.Rating.Text = starRating.ToString() + "*";
                }
                else
                    flyout.HideRating();

                if (videoProps.Bitrate > 0)
                    flyout.Bitrate.Text = Util.NumberToString(videoProps.Bitrate, "bps");
                else
                    flyout.HideBitrate();

                if (videoProps.Keywords.Count >= 1)
                    flyout.Keywords.Text = NameCreditsStr("", videoProps.Keywords);
                else
                    flyout.HideKeywords();

                // IMPORTANT: Need GeoCoordinate class from System.Device.Location namespace (but not avail on WinRT);
                // Not suitable: Windows.Devices.Geolocation namespace;
                // GeoCoordinate geoCoord = new GeoCoordinate();
                if (videoProps.Latitude.HasValue)
                    flyout.Latitude.Text = Util.LatOrLong_ToString(videoProps.Latitude);
                else
                    flyout.HideLatitude();
                if (videoProps.Longitude.HasValue)
                    flyout.Longitude.Text = Util.LatOrLong_ToString(videoProps.Longitude);
                else
                    flyout.HideLongitude();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public async static Task FillGeneralProperties(PropertiesFlyoutGeneral flyout, StorageFile file, BasicProperties basicProps)
        {
            try
            {
                if (!string.IsNullOrEmpty(file.DisplayType))
                    flyout.SetPropsKindTitle(file.DisplayType);

                DocumentProperties doc = await file.Properties.GetDocumentPropertiesAsync();
                if (doc != null)
                {
                    if (doc.Author != null && doc.Author.Count > 0)
                        flyout.Authors.Text = NameCreditsStr("", doc.Author);
                    else
                        flyout.HideAuthors();

                    if (!string.IsNullOrEmpty(doc.Title))
                        flyout.DocumentTitle.Text = doc.Title;
                    else
                        flyout.HideDocumentTitle();

                    if (!string.IsNullOrEmpty(doc.Comment))
                        flyout.Comments.Text = doc.Comment.Substring(0, Math.Min(200, doc.Comment.Length));
                    else
                        flyout.HideComments();

                    if (doc.Keywords != null && doc.Keywords.Count > 0)
                        flyout.Keywords.Text = NameCreditsStr("", doc.Keywords);
                    else
                        flyout.HideKeywords();
                }

                if (!string.IsNullOrEmpty(file.ContentType))
                    flyout.ContentType.Text = file.ContentType;
                else
                    flyout.HideContentType();

                if (basicProps != null && basicProps.DateModified.Year > 1700)
                    flyout.DateModified.Text = DateTime_ToString(basicProps.DateModified, EDateTimeFormat.G);
                else
                    flyout.HideDateModified();

                if (file.DateCreated.Year > 1700)
                    flyout.DateCreated.Text = DateTime_ToString(file.DateCreated, EDateTimeFormat.G);
                else
                    flyout.HideDateCreated();

                FileAttributes attr = file.Attributes;
                if ((uint)attr > 0)
                    flyout.Attributes.Text = attr.ToString();
                else
                    flyout.HideAttributes();

                //if (!string.IsNullOrEmpty(file.FolderRelativeId))
                //    flyout.FolderRelativeId.Text = file.FolderRelativeId;
                //else
                flyout.HideFolderRelativeId();

                //DisplayName
                //IsAvailable
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public static async Task<SettingsFlyout> CreatePropertiesFlyout(StorageFile file, StorageFolder topFolder, string fileSubPath)
        {
            if (file == null) return null;

            SettingsFlyout flyout = null;
            try
            {
                BasicProperties basicProps = null;
                try { basicProps = await file.GetBasicPropertiesAsync(); }
                catch (Exception ex) { Debug.WriteLine(ex.ToString()); }

                if (file.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                {
                    var flyoutImg = new PropertiesFlyoutImage();
                    flyout = flyoutImg;
                    ImageProperties imageProps = await file.Properties.GetImagePropertiesAsync();
                    if (imageProps != null)
                        FillImageProperties(flyoutImg, imageProps, file, basicProps);
                }
                else if (file.ContentType.ToLower().StartsWith("audio"))
                {
                    var flyoutAud = new PropertiesFlyoutAudio();
                    flyout = flyoutAud;
                    MusicProperties musicProps = await file.Properties.GetMusicPropertiesAsync();
                    if (musicProps != null)
                        await FillAudioProperties(flyoutAud, musicProps, file);
                }
                else if (file.ContentType.ToLower().StartsWith("video"))
                {
                    var flyoutVdo = new PropertiesFlyoutVideo();
                    flyout = flyoutVdo;
                    VideoProperties videoProps = await file.Properties.GetVideoPropertiesAsync();
                    if (videoProps != null)
                        FillVideoProperties(flyoutVdo, videoProps);
                }
                else
                {
                    var flyoutGen = new PropertiesFlyoutGeneral();
                    flyout = flyoutGen;
                    await FillGeneralProperties(flyoutGen, file, basicProps);
                }

                Debug.Assert(flyout != null, "Flyout object must exist.");
                if (flyout != null)
                    await FillFileProperties((IFileProperties)flyout, file, topFolder, fileSubPath, basicProps);
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return flyout;
        }

        //public static async Task<string> GetPropertiesText(StorageFile file)
        //{
        //    string txt = "";
        //    try
        //    {
        //        BasicProperties props = null;
        //        try { props = await file.GetBasicPropertiesAsync(); } catch (Exception ex) { Debug.WriteLine(ex.ToString()); }

        //        if (file.ContentType.ToLower().StartsWith("image"))
        //        {
        //            ImageProperties img = await file.Properties.GetImagePropertiesAsync();
        //            if (img != null)
        //            {
        //                txt += "IMAGE ";

        //                if (img.DateTaken.Year > 1700)
        //                    txt += "  Taken: " + DateTime_ToString(img.DateTaken, EDateTimeFormat.G);
        //                else if (props != null)
        //                    txt += "  Date: " + DateTime_ToString(props.DateModified, EDateTimeFormat.G);

        //                if (img.Width > 0 && img.Height > 0)
        //                    txt += "  Dimensions: " + img.Width.ToString() + " x " + img.Height.ToString() + " pixels";

        //                txt += "Type: " + GetFileTypeText(file, true);
        //                if (props != null)
        //                    txt += "  Size: " + Util.SizeToString(props.Size, "B");

        //                // IMPORTANT: Need GeoCoordinate class from System.Device.Location namespace (but not avail on WinRT);
        //                // Not suitable: Windows.Devices.Geolocation namespace;
        //                // GeoCoordinate geoCoord = new GeoCoordinate();
        //                if (img.Latitude.HasValue)
        //                    txt += "  Latitude: " + Util.LatOrLong_ToString(img.Latitude);
        //                if (img.Longitude.HasValue)
        //                    txt += "  Longitude: " + Util.LatOrLong_ToString(img.Longitude);

        //                if (!string.IsNullOrEmpty(img.Title))
        //                    txt += "  Title: " + img.Title;
        //                if (!string.IsNullOrEmpty(img.CameraManufacturer))
        //                {
        //                    txt += "  Camera: " + img.CameraManufacturer;
        //                    if (!string.IsNullOrEmpty(img.CameraModel))
        //                        txt += " " + img.CameraModel;
        //                }
        //            }
        //        }
        //        else if (file.ContentType.ToLower().StartsWith("audio"))
        //        {
        //            MusicProperties aud = await file.Properties.GetMusicPropertiesAsync();
        //            if (aud != null)
        //            {
        //                txt += "AUDIO ";
        //                if (!string.IsNullOrEmpty(aud.AlbumArtist))
        //                    txt += "  Artist: " + aud.AlbumArtist;
        //                else if (!string.IsNullOrEmpty(aud.Artist))
        //                    txt += "  Artist: " + aud.Artist;
        //                else
        //                    txt += await GetDocAuthors(file);
                        
        //                if (!string.IsNullOrEmpty(aud.Album))
        //                    txt += "  Album: " + aud.Album;
        //                else if (!string.IsNullOrEmpty(aud.Title))
        //                {
        //                    if (aud.TrackNumber > 0)
        //                        txt += "  Track: " + aud.TrackNumber.ToString();
        //                    txt += "  Title: " + aud.Title;
        //                }
        //                else
        //                    txt += await GetDocTitle(file);
                        
        //                if (aud.Duration.TotalSeconds > 0)
        //                    txt += "  Duration: " + TimeSpan_ToString(aud.Duration);

        //                txt += "Type: " + GetFileTypeText(file, true);
        //                if (props != null)
        //                    txt += "  Size: " + Util.SizeToString(props.Size, "B");

        //                if (aud.Writers.Count >= 1)
        //                    txt += NameCreditsStr("Writer", aud.Writers);
        //                if (aud.Producers.Count >= 1)
        //                    txt += NameCreditsStr("Producer", aud.Producers);
        //                if (aud.Composers.Count >= 1)
        //                    txt += NameCreditsStr("Composer", aud.Composers);
        //                if (aud.Conductors.Count >= 1)
        //                    txt += NameCreditsStr("Conductor", aud.Conductors);
        //                if (!string.IsNullOrEmpty(aud.Publisher))
        //                    txt += "  Publisher: " + aud.Publisher;
        //                if (aud.Genre.Count >= 1)
        //                    txt += NameCreditsStr("Genre", aud.Genre);
                        
        //                if (aud.Year > 0)
        //                    txt += "  Year: " + aud.Year.ToString();
        //                if (aud.Rating > 0)
        //                {
        //                    uint starRating = (aud.Rating == 0) ? 0 : (uint)Math.Round((double)aud.Rating / 25.0) + 1;
        //                    txt += "  Rating: " + starRating.ToString() + "*";
        //                }
        //                if (aud.Bitrate > 0)
        //                    txt += "  Bitrate: " + Util.NumberToString(aud.Bitrate, "bps");
        //            }
        //        }
        //        else if (file.ContentType.ToLower().StartsWith("video"))
        //        {
        //            VideoProperties vid = await file.Properties.GetVideoPropertiesAsync();
        //            if (vid != null)
        //            {
        //                txt += "VIDEO ";
        //                if (!string.IsNullOrEmpty(vid.Title))
        //                    txt += "  Title: " + vid.Title;
        //                else if (!string.IsNullOrEmpty(vid.Subtitle))
        //                    txt += "  " + vid.Subtitle;

        //                if (vid.Duration.TotalSeconds > 0)
        //                    txt += "  Duration: " + TimeSpan_ToString(vid.Duration);

        //                if (vid.Width > 0 && vid.Height > 0)
        //                    txt += "  Dimensions: " + vid.Width.ToString() + " x " + vid.Height.ToString() + " pixels";

        //                txt += "Type: " + GetFileTypeText(file, true);
        //                if (props != null)
        //                    txt += "  Size: " + Util.SizeToString(props.Size, "B");

        //                if (vid.Directors.Count >= 1)
        //                    txt += NameCreditsStr("Director", vid.Directors);
        //                if (!string.IsNullOrEmpty(vid.Publisher))
        //                    txt += "  Publisher: " + vid.Publisher;
        //                if (vid.Writers.Count >= 1)
        //                    txt += NameCreditsStr("Writer", vid.Writers);
        //                if (vid.Producers.Count >= 1)
        //                    txt += NameCreditsStr("Producer", vid.Producers);

        //                if (vid.Year > 0)
        //                    txt += "  Year: " + vid.Year.ToString();
        //                if (vid.Rating > 0)
        //                {
        //                    uint starRating = (vid.Rating == 0) ? 0 : (uint)Math.Round((double)vid.Rating / 25.0) + 1;
        //                    txt += "  Rating: " + starRating.ToString() + "*";
        //                }
        //                if (vid.Bitrate > 0)
        //                    txt += "  Bitrate: " + Util.NumberToString(vid.Bitrate, "bps");

        //                if (vid.Keywords.Count >= 1)
        //                    txt += NameCreditsStr("Keyword", vid.Keywords);
        //                if (vid.Latitude.HasValue)
        //                    txt += "  Latitude: " + Util.LatOrLong_ToString(vid.Latitude);
        //                if (vid.Longitude.HasValue)
        //                    txt += "  Longitude: " + Util.LatOrLong_ToString(vid.Longitude);
        //            }
        //        }
        //        else
        //        {
        //            txt += "Type: " + GetFileTypeText(file, false);

        //            if (!string.IsNullOrEmpty(file.ContentType))
        //                txt += "  Content Type: " + file.ContentType;

        //            if (props != null)
        //            {
        //                txt += "  Size: " + Util.SizeToString(props.Size, "B");
        //                txt += "  Date Modified: " + DateTime_ToString(props.DateModified, EDateTimeFormat.G);
        //            }
        //            txt += "  Date Created: "  + DateTime_ToString(file.DateCreated, EDateTimeFormat.G);

        //            txt += await GetDocProperties(file);
        //            FileAttributes attr = file.Attributes;
        //            if ((int)attr > 0)
        //                txt += "  Attributes: [" + attr.ToString() + "]";

        //            //if (!string.IsNullOrEmpty(file.FolderRelativeId))
        //            //    txt += "  Folder Relative Id: " + file.FolderRelativeId;
        //            //DisplayName
        //            //IsAvailable
        //        }

        //        if (!string.IsNullOrEmpty(file.Provider.DisplayName))
        //        {
        //            txt += "  Location: " + file.Provider.DisplayName;
        //            if (!string.IsNullOrEmpty(file.Provider.Id))
        //                txt += "-" + file.Provider.Id.Substring(0, 1).ToUpper() + file.Provider.Id.Substring(1);
        //            //txt += ">";
        //        }
                
        //        txt += "  File Name: " + file.Name;
        //    }
        //    catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        //    return txt;
        //}

        public static async Task<string> GetDocAuthors(StorageFile file)
        {
            string txt = "";
            try
            {
                DocumentProperties doc = await file.Properties.GetDocumentPropertiesAsync();
                if (doc != null && doc.Author != null && doc.Author.Count > 0)
                    txt += NameCreditsStr("Author", doc.Author);
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return txt;
        }

        public static async Task<string> GetDocTitle(StorageFile file)
        {
            string txt = "";
            try
            {
                DocumentProperties doc = await file.Properties.GetDocumentPropertiesAsync();
                if (doc != null && !String.IsNullOrEmpty(doc.Title))
                    txt += "  Title: " + doc.Title;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return txt;
        }

        public static string DocTitle(DocumentProperties doc)
        {
            string txt = "";
            if (doc != null && !String.IsNullOrEmpty(doc.Title))
                txt += "  Title: " + doc.Title;
            return txt;
        }

        public static async Task<string> GetDocProperties(StorageFile file)
        {
            string txt = "";
            try
            {
                DocumentProperties doc = await file.Properties.GetDocumentPropertiesAsync();
                if (doc != null)
                {
                    if (doc.Author != null && doc.Author.Count > 0)
                        txt += "  " + NameCreditsStr("Author", doc.Author);
                    txt += DocTitle(doc);
                    if (!String.IsNullOrEmpty(doc.Comment))
                        txt += "  Comment: [" + doc.Comment.Substring(0, Math.Min(80, doc.Comment.Length)) + "]";
                    if (doc.Keywords != null && doc.Keywords.Count > 0)
                        txt += NameCreditsStr("Keyword", doc.Keywords);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return txt;
        }

        public static string GetFileTypeText(StorageFile file, bool indent)
        {
            string txt = indent ? "  " : "";
            try
            {
                if (file != null)
                {
                    if (!string.IsNullOrEmpty(file.FileType))
                        txt += file.FileType.ToUpper();
                    if (!string.IsNullOrEmpty(file.DisplayType))
                    {
                        if (!string.IsNullOrWhiteSpace(txt))
                            txt += " - ";
                        txt += file.DisplayType;
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return txt;
        }

        public static string NameCreditsStr(string nameRole, IList<string> names)
        {
            string txt = "";
            if (nameRole != null && names != null && names.Count >= 1)
            {
                if (!string.IsNullOrEmpty(nameRole))
                {
                    txt += "  " + nameRole;
                    txt += (names.Count == 1) ? ": " : "s: ";
                }
                for (int i = 0; i < names.Count; ++i)
                {
                    txt += names[i];
                    if (i < (names.Count - 1))
                        txt += "; ";
                }
            }
            return txt;
        }

        public static bool IsWordChar(char ch)
        {
            return (char.IsLetterOrDigit(ch) || ch == '_'); //&& !char.IsSeparator(ch);
        }

        public static string FolderPath(StorageFolder folder)
        {
            const string virtFolder = "Virtual Folder";
            try
            {
                if (folder == null)
                    return virtFolder;

                if (!string.IsNullOrEmpty(folder.Path))
                    return folder.Path; // this is actually a proper (non-empty) path

                if (string.IsNullOrEmpty(folder.Name))
                {
                    if (!string.IsNullOrEmpty(folder.Provider.DisplayName))
                        return folder.Provider.DisplayName;
                    else
                        return !string.IsNullOrEmpty(folder.Provider.Id) ? folder.Provider.Id : virtFolder;
                }

                string path = folder.Name;
                if (folder.FolderRelativeId.StartsWith("0\\"))
                    path = "\\\\" + folder.Name;
                else if (folder.Provider.DisplayName.IndexOf("Network", StringComparison.OrdinalIgnoreCase) >= 0)
                    path = "Network\\" + folder.Name;
                else if (IsFolderALibraryByName(folder))
                    path = "Libraries\\" + folder.Name;
                else
                    path = "Homegroup\\" + folder.Name;
                //else if (folder.Provider.DisplayName.IndexOf("OneDrive", StringComparison.OrdinalIgnoreCase) >= 0)
                //    path = "OneDrive\\" + folder.Name;

                return path;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return virtFolder;
        }

        public static bool IsNetworkFile(StorageFile file)
        {
            return file.Path.StartsWith("\\\\");
        }

        public static bool IsSkyDriveFile(StorageFile file)
        {
            if (file.Provider.DisplayName.ToLower().Contains("onedrive") || file.Provider.Id.ToLower().Contains("skydrive"))
                return true;
            String pathLower = file.Path.ToLower();
            return pathLower.Contains("\\onedrive\\") || pathLower.Contains("\\skydrive\\") ||
                   pathLower.Contains("microsoftonedrive") || pathLower.Contains("microsoftskydrive");
        }

        public static bool IsSkyDriveFolder(StorageFolder folder)
        {
            if (folder.Provider.DisplayName.ToLower().Contains("onedrive") || folder.Provider.Id.ToLower().Contains("skydrive"))
                return true;
            String pathLower = folder.Path.ToLower();
            return pathLower.Contains("\\onedrive\\") || pathLower.Contains("\\skydrive\\") ||
                   pathLower.Contains("microsoftonedrive") || pathLower.Contains("microsoftskydrive");
        }

        public static bool IsPublicPath(string path)
        {
            return (path.IndexOf("\\users\\public", StringComparison.OrdinalIgnoreCase) > 0);
        }

        public static bool IsFileOnAddedLibrary(StorageFolder topFolder, StorageFile file)
        {
            return string.IsNullOrEmpty(topFolder.Path) && (IsSkyDriveFile(file) || IsPublicPath(file.Path));
        }

        public static bool IsFolderOnAddedLibrary(StorageFolder topFolder, StorageFolder folder)
        {
            return string.IsNullOrEmpty(topFolder.Path) && (IsSkyDriveFolder(folder) || IsPublicPath(folder.Path));
        }

        public static bool IsFileOnUsersLocal(StorageFolder topFolder, string userFirstName, StorageFile file)
        {
            try
            {
                if (topFolder == null || file == null || string.IsNullOrEmpty(userFirstName) || string.IsNullOrEmpty(file.Path)) return false;

                if (!string.IsNullOrEmpty(topFolder.Path)) return true; // yes, true

                string usersLocalPath = GetUsersLocalPath(userFirstName, topFolder.Name);
                return file.Path.IndexOf(usersLocalPath, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return true; }
        }

        public static bool IsFolderOnUsersLocal(StorageFolder topFolder, string userFirstName, StorageFolder folder)
        {
            try
            {
                if (topFolder == null || folder == null || string.IsNullOrEmpty(userFirstName) || string.IsNullOrEmpty(folder.Path)) return false;

                if (!string.IsNullOrEmpty(topFolder.Path)) return true; // yes, true

                string usersLocalPath = GetUsersLocalPath(userFirstName, topFolder.Name);
                return folder.Path.IndexOf(usersLocalPath, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return true; }
        }

        public static string GetUsersLocalPath(string userFirstName, string folderName)
        {
            return "\\Users\\" + userFirstName + "\\" + folderName;
        }

        //public async static Task<string> GetUsersLocalPath(string folderName)
        //{
        //    try
        //    {
        //        string firstName = await UserInformation.GetFirstNameAsync();
        //        return "\\Users\\" + firstName + "\\" + folderName;
        //    }
        //    catch (Exception ex) { Debug.WriteLine(ex.ToString()); return ""; }
        //}

        public static StorageFolder GetActualFolder(StorageFolder topFolder)
        {
            try
            {
                if (topFolder == null) return null;

                if (!string.IsNullOrEmpty(topFolder.Path))
                    return null;

                if (topFolder.Name.IndexOf("Camera", StringComparison.OrdinalIgnoreCase) == 0)
                    return KnownFolders.CameraRoll;
                else if (topFolder.Name.IndexOf("Documents", StringComparison.OrdinalIgnoreCase) == 0)
                    return KnownFolders.DocumentsLibrary;
                else if (topFolder.Name.IndexOf("Home", StringComparison.OrdinalIgnoreCase) == 0)
                    return KnownFolders.HomeGroup;
                else if (topFolder.Name.IndexOf("Media", StringComparison.OrdinalIgnoreCase) == 0)
                    return KnownFolders.MediaServerDevices;
                else if (topFolder.Name.IndexOf("Music", StringComparison.OrdinalIgnoreCase) == 0)
                    return KnownFolders.MusicLibrary;
                else if (topFolder.Name.IndexOf("Pictures", StringComparison.OrdinalIgnoreCase) == 0)
                    return KnownFolders.PicturesLibrary;
                else if (topFolder.Name.IndexOf("Playlists", StringComparison.OrdinalIgnoreCase) == 0)
                    return KnownFolders.Playlists;
                else if (topFolder.Name.IndexOf("RemovableDevices", StringComparison.OrdinalIgnoreCase) == 0)
                    return KnownFolders.RemovableDevices;
                else if (topFolder.Name.IndexOf("SavedPictures", StringComparison.OrdinalIgnoreCase) == 0)
                    return KnownFolders.SavedPictures;
                else if (topFolder.Name.IndexOf("Videos", StringComparison.OrdinalIgnoreCase) == 0)
                    return KnownFolders.VideosLibrary;

                return null;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return null;
        }

        public static bool IsFolderALibraryByName(StorageFolder topFolder)
        {
            try
            {
                if (topFolder == null) return false;

                if (!string.IsNullOrEmpty(topFolder.Path))
                    return false;

                if (topFolder.Name.IndexOf("Camera", StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
                else if (topFolder.Name.IndexOf("Documents", StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
                else if (topFolder.Name.IndexOf("Media", StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
                else if (topFolder.Name.IndexOf("Music", StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
                else if (topFolder.Name.IndexOf("Pictures", StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
                else if (topFolder.Name.IndexOf("Playlists", StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
                else if (topFolder.Name.IndexOf("Removable", StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
                else if (topFolder.Name.IndexOf("SavedPic", StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
                else if (topFolder.Name.IndexOf("Videos", StringComparison.OrdinalIgnoreCase) == 0)
                    return true;

                return false;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return false;
        }

        public static string GetInstallDrive()
        {
            try
            {
                Windows.ApplicationModel.Package package = Windows.ApplicationModel.Package.Current;
                Windows.Storage.StorageFolder installFolder = package.InstalledLocation;
                return installFolder.Path.Substring(0, 2);
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return ""; }
        }
        
        public static bool StringList_IsEmpty(List<string> stringList)
        {
            try
            {
                if (stringList == null || stringList.Count <= 0)
                    return true;

                foreach (var str in stringList)
                {
                    if (!string.IsNullOrEmpty(str))
                        return false;
                }
                return true;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return false;
        }

        public static string TimeSpan_ToString(TimeSpan timeSpan)
        {
            string timeStr = "";
            try
            {
                if (timeSpan.TotalHours >= 1.0)
                {
                    timeStr = timeSpan.Hours.ToString("D01") + " hr " +
                              timeSpan.Minutes.ToString("D02") + " min " +
                              timeSpan.Seconds.ToString("D02") + " sec";
                }
                else if (timeSpan.TotalMinutes >= 1.0)
                {
                    timeStr = timeSpan.Minutes.ToString("D01") + " min " +
                              timeSpan.Seconds.ToString("D02") + " sec";
                }
                else
                {
                    timeStr = timeSpan.Seconds.ToString("D01") + " sec";
                }
                return timeStr;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return timeStr;
        }

        public enum EDateTimeFormat
        {
            s,
            G,
            custom,
        }

        public static string DateTime_ToString(DateTimeOffset dt, EDateTimeFormat format)
        {
            string timeStr = "";
            try
            {
                if (format == EDateTimeFormat.s)
                {
                    timeStr = dt.ToString("s");
                }
                else if (format == EDateTimeFormat.G)
                {
                    timeStr = dt.ToString("G");
                }
                else if (format == EDateTimeFormat.custom)
                {
                    DateTime localdt = dt.LocalDateTime;
                    timeStr = localdt.Year.ToString("D04") + "-" + localdt.Month.ToString("D02") + "-" + localdt.Day.ToString("D02");
                    timeStr += " ";
                    timeStr += localdt.TimeOfDay.ToString("c");
                }
                return timeStr;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return timeStr;
        }
    };
}
