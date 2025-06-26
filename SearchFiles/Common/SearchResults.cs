using SearchFiles.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace SearchFiles.Common
{
    class FolderFileId
    {
        public string TopFolderToken { get; set; }
        public string TopFolderPath { get; set; }
        public string SubPath { get; set; }
        public string FileName { get; set; }

        public FolderFileId()
        {
            TopFolderToken = "";
            TopFolderPath = "";
            SubPath = "";
            FileName = "";
        }

        public FolderFileId(string topFolderToken, string topFolderPath, string subPath, string fileName)
        {
            TopFolderToken = topFolderToken;
            TopFolderPath  = topFolderPath;
            SubPath = subPath;
            FileName = fileName;
        }

        public static async Task<StorageFolder> CreateFolder(string topFolderToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(topFolderToken) && StorageApplicationPermissions.FutureAccessList.ContainsItem(topFolderToken))
                    return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(topFolderToken);
                else
                    return null;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return null; }
        }

        //public async Task<StorageFolder> CreateFolder()
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(TopFolderToken) && StorageApplicationPermissions.FutureAccessList.ContainsItem(TopFolderToken))
        //            return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(TopFolderToken);
        //        else
        //            return null;
        //    }
        //    catch (Exception ex) { Debug.WriteLine(ex.ToString()); return null; }
        //}

        //public async Task<StorageFile> CreateFile()
        //{
        //    try
        //    {
        //        StorageFolder folder = await CreateFolder();
        //        if (folder != null)
        //        {
        //            StorageFile file = await folder.GetFileAsync(SubPath + "\\" + FileName);
        //            return file;
        //        }
        //        else
        //            return null;
        //    }
        //    catch (Exception ex) { Debug.WriteLine(ex.ToString()); return null; }
        //}

        //private static string sep = "|:";

        //public override string ToString()
        //{
        //    return sep
        //        + TopFolderToken + sep
        //        + TopFolderPath + sep
        //        + SubPath + sep
        //        + FileName + sep;
        //}
    }

    class SearchResults
    {
        private static Dictionary<int, string> resIdFileNameDict = new Dictionary<int,string>();

        //private static string sep = "|:";
        private static string bs = "\\";
        private static string nl = "\r\n";
        private static string LIB_FOLDER_NAME_PREF = "LIBRARY_NAME_TOP_FOLDER: ";

        public static async Task SaveResults(int resultsId, ItemCollection itemCollection, string topFolderToken, string topFolderPath, string topFolderName)
        {
            try
            {
                DateTimeOffset dto = DateTimeOffset.Now;
                // IMPORTANT: Must use "s" format here!
                string fileName = "eCodified.SearchFiles." + dto.ToString("s") + "." + dto.Millisecond.ToString("D03") + ".txt";
                fileName = ItemsPage.Str.RemoveChar(fileName, ':');
                if (!resIdFileNameDict.ContainsKey(resultsId))
                    resIdFileNameDict.Add(resultsId, fileName);

                StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
                StorageFile file = await tempFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                int cnt = 0;
                bool isLibFolder = string.IsNullOrEmpty(topFolderPath);

                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (var dataWriter = new DataWriter(stream))
                    {
                        dataWriter.WriteString(topFolderToken + nl);
                        if (isLibFolder)
                            dataWriter.WriteString(LIB_FOLDER_NAME_PREF + topFolderName + bs + nl);
                        else
                            dataWriter.WriteString(topFolderPath + bs + nl);

                        foreach (FileInfoDataItem fileItem in itemCollection)
                        {
                            try
                            {
                                if (ItemsPage.Current.ResultsStopped)
                                    break;

                                if (string.IsNullOrEmpty(fileItem.SubPath) || fileItem.SubPath == FileInfoDataCommon.SUB_HEAD)
                                    dataWriter.WriteString(fileItem.FileName + nl);
                                else if (isLibFolder)
                                    dataWriter.WriteString(fileItem.File.Path + nl);
                                else if (fileItem.SubPath.Length > FileInfoDataCommon.SUB_HEAD.Length)
                                    dataWriter.WriteString(fileItem.SubPath.Substring(FileInfoDataCommon.SUB_IDX) + bs + fileItem.FileName + nl);
                                else
                                    dataWriter.WriteString(fileItem.SubPath + bs + fileItem.FileName + nl);
                                ++cnt;
                                if ((cnt % 5000) == 0)
                                {
                                    await dataWriter.StoreAsync();
                                    await dataWriter.FlushAsync();
                                }
                            }
                            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
                        }
                        await dataWriter.StoreAsync(); // IMPORTANT
                        await dataWriter.FlushAsync(); // nice to have
                    }
                }
                Debug.WriteLine("SaveResults: id: " + resultsId.ToString() + "; file: " + file.Name + "; count: " + cnt.ToString());
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public static async Task RestoreResults(int resultsId)
        {
            try
            {
                if (!resIdFileNameDict.ContainsKey(resultsId))
                    return;

                string fileName = resIdFileNameDict[resultsId];
                if (string.IsNullOrEmpty(fileName))
                    return;

                StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
                StorageFile file = await tempFolder.GetFileAsync(fileName);
                if (file == null)
                    return;

                string text = await FileIO.ReadTextAsync(file); // !!! read the whole file

                string[] seps = new string[] { nl };
                string[] lines = text.Split(seps, StringSplitOptions.RemoveEmptyEntries);

                string topFolderToken = lines[0];
                string topFolderPath = lines[1];
                bool isLibFolder = topFolderPath.StartsWith(LIB_FOLDER_NAME_PREF);
                string bsLibName = isLibFolder ? ("\\" + topFolderPath.Substring(LIB_FOLDER_NAME_PREF.Length)) : "";

                StorageFolder topFolder = await FolderFileId.CreateFolder(topFolderToken);
                if (topFolder == null)
                    return;

                for (int i = 2; i < lines.Length; ++i)
                {
                    try
                    {
                        if (ItemsPage.Current.ResultsStopped)
                            return;

                        if (!string.IsNullOrEmpty(lines[i]))
                        {
                            string subPath = lines[i];
                            if (isLibFolder)
                            {
                                int idx = subPath.IndexOf(bsLibName);
                                if (idx > -1)
                                    subPath = subPath.Substring(idx + bsLibName.Length);
                            }
                            var ifile = await topFolder.GetFileAsync(subPath);
                            await ItemsPage.Current.AddFileInfoTile(ifile);
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
                }
                Debug.WriteLine("RestoreRes: id: " + resultsId.ToString() + "; file: " + file.Name + "; count: " + ItemsPage.Current.ResultsCount.ToString());
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

    }
}
