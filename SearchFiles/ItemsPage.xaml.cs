using SearchFiles.Common;
using SearchFiles.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.UserProfile;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// + rel 2: up btn
// + rel 2: search within results
// + rel 2: size search option
// + rel 2: date search option - dates: mod, creation, access
// + rel 2: use new Win 8.1 Button.Flyout INSTEAD OF MsgDlgs !
// + rel 2: use new SearchResultsPage ?
// + rel 2: search history
// + rel 2: add settings flyout
// + rel 2: save results to file

namespace SearchFiles
{
    public sealed partial class ItemsPage : LayoutAwarePage
    {
        #region Licensing Vars
        private static bool TEST_MODE = false;
        private bool IsAppSrcCodePro { get { return false; } } // this is the express edition
        private LicenseChangedEventHandler m_licenseChangeHandler = null;
        private bool m_IsActiveLic = true;
        private bool m_IsTrialLic = false;
        #endregion

        #region Vars
        public const string APP_NAME = "eCodified Search.Files";
        private StorageFolder m_TopFolder = null;
        public StorageFolder TopFolder { get { return m_TopFolder; } }
        public static ItemsPage Current = null;

        public Dictionary<string, String> _ExtContyp = new Dictionary<string, string>();
        public Dictionary<string, String> _ContypExt = new Dictionary<string, string>();

        private Int32 MaxSubLevel = 1000; // default/initial value only
        private bool m_Cancelled = false;
        public bool Cancelled { get { return m_Cancelled; } }
        private bool m_Processing = false;
        private bool m_ResultsStopped = false;
        public  bool ResultsStopped { get { return m_ResultsStopped; } }
        private FileInfoDataSource m_fileInfoDataSource = null;
        private string m_LastFolderToken = "";
        private FileInfoDataItem m_LastFileInfoItem = null;
        private int m_ResultsId = 0;
        UInt64 m_AllFilesCount = 0;
        UInt64 m_AllFoldersCount = 0;
        UInt64 m_MatchBytes = 0;

        public Int32 ResultsCount { get { return (m_FileInfoViewGrid != null) ? m_FileInfoViewGrid.Items.Count : 0; } }

        private IStackMgr m_BackStack = null;
        private IStackMgr m_FowdStack = null;
        private bool m_BackInProgress = false;
        private bool m_FowdInProgress = false;
        private SearchOptions m_CurrSearchOpts = null;

        UInt32 m_BufferSize = 20 * 1024 * 1024; // init / default value only

        ApplicationDataContainer m_LocalSettings = null;
        private bool m_IsInitNav = true;

        //System.Threading.Timer m_SingleTapTimer = null;
        //DateTime m_SingleTapLastTime = DateTime.Now;
        private bool m_SingleTapNogo = true;

        private eCodified_SystemInfo? systemInfo = null;
        private static int NBR_TILES_HOLD = 1;

        private ListView m_FileInfoViewGrid = null;
        private List<ListView> m_FileViewObjList = null;
        private int m_FileViewIdx = -1;
        private int m_LastFileViewIdx = 0;

        public const string ResetSearchOptsKey = "ResetSearchOptsKey";
        private bool m_ResetSearchOpts = true;
        public const string ResetSearchOptsQuestKey = "ResetSearchOptsQuestKey";
        private bool m_ResetSearchOptsQuest = true;

        private const bool SearchAddedLibs = true;
        private bool SearchAddedLibsWarn = true;
        public const string SearchAddedLibsWarnKey = "SearchAddedLibsWarnKey";

        private string m_UserFirstName = "";
        private string m_SystemDrive = "";

        private bool m_FolderChanged = false;

        #endregion

        public ItemsPage()
        {
            try
            {
                this.InitializeComponent();
                Current = this;

                m_BackStack = new StackMgr("BackStack", "01");
                m_FowdStack = new StackMgr("ForwardStack", "01");

                m_fileInfoDataSource = new FileInfoDataSource();
                var group = FileInfoDataSource.GetGroup("SuccessGroup");
                this.DefaultViewModel["Group"] = group;
                this.DefaultViewModel["Items"] = group.Items;
                //this.itemsViewSource.View.MoveCurrentToFirst();
                //m_FileInfoViewItemSource.Source = m_fileInfoDataSource;

                this.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(ItemsPage_KeyDown), true);
                m_StopBtn2.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(ItemsPage_KeyDown), true);
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs ev)
        {
            try
            {
                base.OnNavigatedTo(ev);

                if (m_IsInitNav)
                {
                    m_IsInitNav = false;

                    if (m_MatchAllWordsRbtn != null)
                        m_MatchAllWordsRbtn.IsChecked = true; // very initial value only

                    await RefreshLicenseInfo();
                    await RestoreSettings();

                    //SearchOptions defaultOpts = new SearchOptions();
                    bool showAdvanced = false; //!m_CurrSearchOpts.AdvancedEquals(defaultOpts);

                    m_FolderBar.Visibility = Visibility.Visible;
                    m_MoreOptionsBtn.Visibility = showAdvanced ? Visibility.Collapsed : Visibility.Visible;
                    m_LessOptionsBtn.Visibility = showAdvanced ? Visibility.Visible : Visibility.Collapsed;
                    m_OptionsGrid.Visibility = showAdvanced ? Visibility.Visible : Visibility.Collapsed;
                    m_FileCountBar.Visibility = Visibility.Collapsed;
                    m_FoundFilesSeparator.Visibility = Visibility.Collapsed;
                    m_FoundFilesDisplay.Visibility = Visibility.Collapsed;
                    MaxSubLevel = GetMaxSubLevel();

                    //SearchWordsTbx_TextChanged(null, null);
                    //NamesFilterTbx_TextChanged(null, null);

                    EnableControls();
                    systemInfo = new eCodified_SystemInfo();
                    InitFileViewObjList();

                    if (m_TopFolder == null)
                    {
                        await WarnUserFirstRunFolder();
                        await PickFolder(true);
                    }
                    else
                    {
                        if (IsTrial() || !IsPro())
                            await AskAndBuyAppAsync();
                    }
                    m_BuyAppBtn.Visibility = (IsTrial() || !IsPro()) ? Visibility.Visible : Visibility.Collapsed;

                    m_UserFirstName = await UserInformation.GetFirstNameAsync();
                    m_SystemDrive = Util.GetInstallDrive();

                    if (IsSavingResults()) // <- must be called after InitFileViewObjList()
                        await Util.DelTempFiles();
                }
                else if (IsTrialExpired() || !IsPro())
                    await AskAndBuyAppAsync();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs ev)
        {
            try
            {
                base.OnNavigatedFrom(ev);
                m_CurrSearchOpts = CreateCurrSearchOpts(); // <- update search opts from the UI
                SaveSettings();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void InitFileViewObjList()
        {
            try
            {
                m_FileViewObjList = new List<ListView>();
                m_FileViewObjList.Add(m_FileInfoViewGrid00);
                m_FileViewObjList.Add(m_FileInfoViewGrid01);
                m_FileViewObjList.Add(m_FileInfoViewGrid02);
                m_FileViewObjList.Add(m_FileInfoViewGrid03);
                m_FileViewObjList.Add(m_FileInfoViewGrid04);
                m_FileViewObjList.Add(m_FileInfoViewGrid05);
                m_FileViewObjList.Add(m_FileInfoViewGrid06);
                m_FileViewObjList.Add(m_FileInfoViewGrid07);
                m_FileViewObjList.Add(m_FileInfoViewGrid08);
                m_FileViewObjList.Add(m_FileInfoViewGrid09);
                m_FileViewObjList.Add(m_FileInfoViewGrid10);
                m_FileViewObjList.Add(m_FileInfoViewGrid11);
                FileViewIdxFowd();
                SetFileViewObj();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void SetFileViewObj()
        {
            try
            {
                if (m_FileViewIdx < 0 || m_FileViewIdx >= m_FileViewObjList.Count)
                {
                    Debug.Assert(false, "ERROR: Invalid file-view-obj-list index: " + m_FileViewIdx.ToString());
                    return;
                }
                for (int i = 0; i < m_FileViewObjList.Count; ++i)
                {
                    if (i != m_FileViewIdx)
                        m_FileViewObjList[i].Visibility = Visibility.Collapsed;
                }
                m_FileInfoViewGrid = m_FileViewObjList[m_FileViewIdx];
                m_FileInfoViewGrid.Visibility = Visibility.Visible;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public bool IsNavInstant()
        {
            try
            {
                return (m_FileViewObjList != null && m_FileViewObjList.Count >= 2);
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return false; }
        }
        
        private void FileViewIdxBack()
        {
            try
            {
                --m_FileViewIdx;
                if (m_FileViewIdx < 0)
                    m_FileViewIdx = m_FileViewObjList.Count - 1;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void FileViewIdxFowd()
        {
            try
            {
                ++m_FileViewIdx;
                if (m_FileViewIdx == m_FileViewObjList.Count || m_FileViewIdx < 0)
                    m_FileViewIdx = 0;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void SetNbrTilesToHold()
        {
            try
            {
                if (systemInfo == null)
                    return;
                if (systemInfo.IsHighPerf())
                {
                    if (m_SearchWordsTbx.Text == string.Empty)
                        NBR_TILES_HOLD = (m_NamesFilterTbx.Text == string.Empty) ? 10 : 5;
                    else if (m_NamesFilterTbx.Text == string.Empty)
                        NBR_TILES_HOLD = 3;
                    else
                        NBR_TILES_HOLD = 1;
                }
                else if (systemInfo.IsMediumPerf())
                {
                    if (m_SearchWordsTbx.Text == string.Empty)
                        NBR_TILES_HOLD = (m_NamesFilterTbx.Text == string.Empty) ? 4 : 2;
                    else
                        NBR_TILES_HOLD = 1;
                }
                else if (systemInfo.IsLowPerf())
                {
                    if (m_SearchWordsTbx.Text == string.Empty)
                        NBR_TILES_HOLD = (m_NamesFilterTbx.Text == string.Empty) ? 3 : 2;
                    else
                        NBR_TILES_HOLD = 1;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public static bool IsEmpty(IBuffer b)
        {
            return (b == null || b.Length <= 0);
        }

        private async Task RestoreSettings()
        {
            try
            {
                if (m_CurrSearchOpts == null)
                    m_CurrSearchOpts = CreateCurrSearchOpts();

                m_LocalSettings = ApplicationData.Current.LocalSettings;
                if (m_LocalSettings == null)
                {
                    await ApplySearchOptions(m_CurrSearchOpts);
                    return;
                }
                m_CurrSearchOpts.RestoreFromSettings(m_LocalSettings);
                await ApplySearchOptions(m_CurrSearchOpts);

                if (m_LocalSettings.Values.ContainsKey(ResetSearchOptsKey))
                    m_ResetSearchOpts      = (bool)m_LocalSettings.Values[ResetSearchOptsKey];
                if (m_LocalSettings.Values.ContainsKey(ResetSearchOptsQuestKey))
                    m_ResetSearchOptsQuest = (bool)m_LocalSettings.Values[ResetSearchOptsQuestKey];
                if (m_LocalSettings.Values.ContainsKey(SearchAddedLibsWarnKey))
                    SearchAddedLibsWarn    = (bool)m_LocalSettings.Values[SearchAddedLibsWarnKey];
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public void SaveSettings()
        {
            try
            {
                m_LocalSettings = ApplicationData.Current.LocalSettings;
                if (m_LocalSettings == null)
                    return;

                if (m_CurrSearchOpts == null)
                    m_CurrSearchOpts = CreateCurrSearchOpts();

                m_CurrSearchOpts.SaveInSettings(m_LocalSettings);

                m_LocalSettings.Values[ResetSearchOptsKey]      = m_ResetSearchOpts;
                m_LocalSettings.Values[ResetSearchOptsQuestKey] = m_ResetSearchOptsQuest;
                m_LocalSettings.Values[SearchAddedLibsWarnKey]  = SearchAddedLibsWarn;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void SaveFolderToken(string folderToken)
        {
            try
            {
                if (m_LocalSettings == null)
                    return;

                m_LocalSettings.Values[SearchOptions.TopFolderTokenKey] = folderToken;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private string GetFolderToken()
        {
            try
            {
                if (m_LocalSettings == null || !m_LocalSettings.Values.ContainsKey(SearchOptions.TopFolderTokenKey))
                    return "";

                return (string)m_LocalSettings.Values[SearchOptions.TopFolderTokenKey];
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return ""; }
        }

        public bool SearchAfterFolderChanged() // TODO get it from settings
        {
            return false;
        }

        private async Task PickFolder(bool searchAllowed)
        {
            try
            {
                StorageFolder prevTopFolder = m_TopFolder;

                var openPicker = new FolderPicker();
                openPicker.ViewMode = PickerViewMode.List;
                openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                openPicker.FileTypeFilter.Add("*");
                openPicker.CommitButtonText = "Select Folder";

                StorageFolder folder = await openPicker.PickSingleFolderAsync();
                if (folder != null)
                    m_TopFolder = folder;

                if (m_TopFolder != null)
                {
                    try
                    {
                        m_LastFolderToken = StorageApplicationPermissions.FutureAccessList.Add(m_TopFolder);
                        SaveFolderToken(m_LastFolderToken);
                    }
                    catch (Exception ex) { Debug.WriteLine(ex.ToString()); }

                    if (prevTopFolder == null || m_TopFolder.Path != prevTopFolder.Path || m_TopFolder.Name != prevTopFolder.Name)
                        await OnFolderChanged(searchAllowed); // must be done after FutureAccessList.Add
                }
                return;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public UInt32 GetBufferSize(StorageFolder folder)
        {
            try
            {
                if (string.IsNullOrEmpty(m_TopFolder.Path) || m_TopFolder.Path.Length < 3)
                    return 20 * 1024 * 1024;

                string pathHead = m_TopFolder.Path.Substring(0, 3).ToLower();
                char pathCh0 = pathHead[0];
                char pathCh1 = pathHead[1];
                char pathCh2 = pathHead[2];

                // TODO write a GetDriveType function using .NET classes; eg. is it local, network, removable, external?
                // even better, write GetDriveSpeedProps; eg. read a block of data from the drive and measure its performance
                Debug.Assert((pathCh0 == '\\' && pathCh1 == '\\') || (pathCh1 == ':' && pathCh2 == '\\'));

                if ((pathCh0 == '\\' && pathCh1 == '\\') || pathCh0 >= 'o') // network file?
                    return 200 * 1024;
                else if (pathCh0 == 'c')
                    return 40 * 1024 * 1024;
                else
                    return 10 * 1024 * 1024;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return 200 * 1024 * 1024; }
        }

        public bool CaseSensitive()
        {
            return m_CaseSensitiveChk.IsChecked == true;
        }

        public bool WholeWords()
        {
            return m_WholeWordsChk.IsChecked == true;
        }

        public bool UseRegex()
        {
            return m_UseRegexChk.IsChecked == true;
        }

        /// <summary>
        ///  To be used by client code.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="wordList"></param>
        /// <returns></returns>
        public async Task<bool> FileContainsAllWords(StorageFile file, List<String> wordList)
        {
            try
            {
                foreach (var word in wordList)
                {
                    var wl = new List<String>();
                    wl.Add(word);
                    if (!await FileContainsAnyWord(file, wl))
                        return false;
                    if (m_Cancelled)
                        return false;
                }
                return true;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return false; }
        }

        /// <summary>
        ///  To be used by client code.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="wordList"></param>
        /// <returns></returns>
        public async Task<bool> FileContainsAnyWord(StorageFile file, List<String> wordList)
        {
            try
            {
                if (Util.StringList_IsEmpty(wordList))
                    return true;

                using (IInputStream stream = await file.OpenSequentialReadAsync())
                {
                    if (stream == null)
                        return false;

                    byte[] byteArr = new byte[m_BufferSize];
                    IBuffer readBuf = byteArr.AsBuffer();

                    using (var reader = new StreamReader(readBuf.AsStream()))
                    {
                        if (reader == null)
                            return false;

                        BasicProperties props = await file.GetBasicPropertiesAsync();

                        while (!m_Cancelled)
                        {
                            IBuffer retBuf = await stream.ReadAsync(readBuf, readBuf.Length, InputStreamOptions.None);
                            if (IsEmpty(retBuf))
                                return false;

                            int blockLen = (props != null && props.Size < (ulong)retBuf.Length) ? (int)props.Size : (int)retBuf.Length;
                            char[] blockArr = new char[blockLen];

                            int nbrRead = await reader.ReadBlockAsync(blockArr, 0, blockLen);  //reader.ReadToEnd(); //reader.ReadLine();
                            if (nbrRead > 0)
                            {
                                string blockStr = new string(blockArr);
                                Debug.Assert(blockLen == blockStr.Length);

                                if (!string.IsNullOrEmpty(blockStr))
                                {
                                    if (StringContainsAnyWord(blockStr, wordList, CaseSensitive()))
                                        return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return false; }
        }

        public bool StringContainsAnyWord(String theString, List<String> wordList, bool caseSensitive)
        {
            try
            {
                StringComparison stringComp = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                RegexOptions regexOpts = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

                foreach (String word in wordList)
                {
                    if (m_Cancelled)
                        return false;

                    if (string.IsNullOrEmpty(word))
                        continue;

                    if (UseRegex())
                    {
                        if (Regex.Match(theString, word, regexOpts).Success)
                            return true;
                    }
                    else
                    {
                        if (WholeWords())
                        {
                            if (StringContainsWholeWord(theString, word, stringComp))
                                return true;
                        }
                        else
                        {
                            int firstIdx = theString.IndexOf(word, stringComp);
                            if (firstIdx > -1)
                                return true;
                        }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return false;
        }

        public bool StringContainsWholeWord(string theString, string word, StringComparison stringComp)
        {
            try
            {
                int startIdx = 0;
                int limitIdx = theString.Length - word.Length;
                if (limitIdx < 0)
                    return false;
                else if (limitIdx == 0)
                    return (theString.IndexOf(word, startIdx, stringComp) > -1);
                else
                {
                    for (int i = 0; i < limitIdx; ++i)
                    {
                        int firstIdx = theString.IndexOf(word, startIdx, stringComp);
                        if (firstIdx <= -1)
                            return false;
                        char leftCh = (firstIdx == 0) ? ' ' : theString[firstIdx - 1];
                        char rightCh = ((firstIdx + word.Length) >= theString.Length) ? ' ' : theString[firstIdx + word.Length];

                        if (!Util.IsWordChar(leftCh) && !Util.IsWordChar(rightCh))
                            return true;

                        startIdx = firstIdx + word.Length;
                    }
                    return false;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return false;
        }

        public bool FileNameMatchesPatterns(StorageFile file, List<NamePatternList> namePatternLists)
        {
            try
            {
                if (namePatternLists.Count <= 0)
                    return true;

                if (UseRegex())
                {
                    if (namePatternLists[0].Count <= 0)
                        return true;

                    return Regex.Match(file.Name, namePatternLists[0][0].pat, RegexOptions.IgnoreCase).Success;
                }
                else
                {
                    foreach (var namePatterns in namePatternLists)
                    {
                        if (StringContainsAllPatterns(file.Name, namePatterns))
                            return true;
                    }
                    return false;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return true;
        }

        public void IncrTotalBytes(ulong nbytes)
        {
            m_MatchBytes += nbytes;
        }

        public async Task<bool> ProcessFileIfMatching(StorageFile file, List<string> searchWords, List<NamePatternList> namePatternLists)
        {
            try
            {
                if (FileNameMatchesPatterns(file, namePatternLists))
                {
                    bool match = (searchWords.Count <= 0);
                    if (!match)
                    {
                        match = (m_MatchAllWordsRbtn.IsChecked == true) ?
                                 await FileContainsAllWords(file, searchWords) : await FileContainsAnyWord(file, searchWords);
                    }
                    if (match)
                    {
                        bool res = await AddFileInfoTile(file);
                        //string text = indent + file.Name;
                        //Debug.WriteLine(text);
                        return true;
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return false;
        }

        private async Task SearchRecursive(StorageFolder folder, List<string> searchWords, List<NamePatternList> namePatternLists, int level)
        {
            try
            {
                ++m_AllFoldersCount;
                if (m_Cancelled)
                    return;

                IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();
                IReadOnlyList<StorageFolder> subFolderList = await folder.GetFoldersAsync();

                //indent += "    ";
                foreach (StorageFile file in fileList)
                {
                    if (SearchAddedLibs) // || Util.IsFileOnUsersLocal(m_TopFolder, m_UserFirstName, file))
                    {
                        await ProcessFileIfMatching(file, searchWords, namePatternLists);
                        ++m_AllFilesCount;
                        m_AllCountNbr.Text = " / " + m_AllFilesCount.ToString();
                    }
                    if (m_Cancelled)
                        return;
                }

                if (subFolderList.Count <= 0) // any sub-folders?
                    return;

                if (m_AllLevelsChk.IsChecked == true || level < MaxSubLevel)
                {
                    // sub-folder level is not limited or limit is not reached yet: recursion continues
                    ++level;
                    foreach (StorageFolder subFolder in subFolderList)
                    {
                        if (SearchAddedLibs) // || Util.IsFolderOnUsersLocal(m_TopFolder, m_UserFirstName, subFolder))
                        {
                            ++m_AllFoldersCount;
                            await SearchRecursive(subFolder, searchWords, namePatternLists, level);
                        }
                        if (m_Cancelled)
                            return;
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public /*async Task*/ void SearchFiles_OtherImpl(string textToFind)
        {
            // this impl uses Windows.Storage.Search
            //try
            //{
            //    if (m_TopFolder == null)
            //        return;
            //
            //    List<string> searchWords = new List<string>();
            //    GetSearchWords(m_SearchWordsTbx.Text, out searchWords);
            //
            //    List<string> namePatterns = GetSimpleNamePatterns(m_NamesFilterTbx.Text);
            //
            //    // Search deep (but not recursive) using .NET QueryOptions and StorageFileQueryResult
            //    var opts = new QueryOptions(CommonFileQuery.OrderByName, namePatterns);
            //    StorageFileQueryResult query = m_TopFolder.CreateFileQueryWithOptions(opts);
            //    IReadOnlyList<StorageFile> fileList = await query.GetFilesAsync();
            //    Debug.WriteLine("");
            //    foreach (StorageFile file in fileList)
            //    {
            //        Debug.WriteLine(file.Path);
            //        if (m_cancelled)
            //            return;
            //    }
            //    Debug.WriteLine("");
            //}
            //catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public bool IsSearchTextOk(string searchText)
        {
            try
            {
                string testStr = "this is just a test string";
                if (UseRegex())
                {
                    try
                    {
                        Regex.Match(testStr, searchText);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                    //try
                    //{
                    //    testStr.IndexOf(searchText);
                    //    return true;
                    //}
                    //catch (Exception)
                    //{
                    //    return false;
                    //}
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            return true;
        }

        private void GetSearchWords(string searchText, out List<string> searchWords)
        {
            searchWords = new List<string>();
            try
            {
                if (string.IsNullOrEmpty(searchText))
                    return;

                if (m_MatchCompletePhraseRbtn.IsChecked == true || UseRegex())
                {
                    searchWords.Add(searchText);
                    return;
                }

                char[] seps = new char[] { ' ' };
                string[] tempWords = searchText.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                searchWords = new List<string>();
                foreach (string word in tempWords)
                {
                    if (!string.IsNullOrEmpty(word))
                        searchWords.Add(word.Trim());
                }
                return;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public void SetSearchWordBox(String text)
        {
            try
            {
                m_SearchWordsTbx.Text = text;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private bool IsFileViewIdxOk()
        {
            return !IsNavInstant() || m_FileViewIdx <= m_LastFileViewIdx || (m_FileViewIdx - m_LastFileViewIdx) >= 2;
        }

        private void EnableBackAndForward()
        {
            try
            {
                m_BackBtn.IsEnabled = !m_Processing && m_BackStack.Count >= 1 && IsFileViewIdxOk();
                m_FowdBtn.IsEnabled = !m_Processing && m_FowdStack.Count >= 1;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void EnableUpBtn()
        {
            try
            {
                m_UpBtn.IsEnabled = !m_Processing && m_TopFolder != null && (string.IsNullOrEmpty(m_TopFolder.Path) || m_TopFolder.Path.Length > 3);
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void EnableWordMatchControls()
        {
            try
            {
                m_UseRegexChk.IsEnabled = !m_Processing;
                m_CaseSensitiveChk.IsEnabled = !m_Processing;
                m_WholeWordsChk.IsEnabled = !m_Processing && !UseRegex();
                m_MatchAnyWordRbtn.IsEnabled = !m_Processing && !UseRegex();
                m_MatchAllWordsRbtn.IsEnabled = !m_Processing && !UseRegex();
                m_MatchCompletePhraseRbtn.IsEnabled = !m_Processing && !UseRegex();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void EnableControls()
        {
            try
            {
                ShowAdverts(IsTrial() || !IsPro());
                SetAppTitle();
                EnableBackAndForward();
                EnableUpBtn();

                m_SearchWordsTbx.IsEnabled = !m_Processing;
                m_SearchBtn.IsEnabled = !m_Processing;

                m_FolderChangeBtn.IsEnabled = !m_Processing;
                m_MoreOptionsBtn.IsEnabled = !m_Processing;
                m_LessOptionsBtn.IsEnabled = !m_Processing;

                //m_OptionsGrid.IsHitTestVisible = !m_ProcInProgress;
                m_NamesFilterTbx.IsEnabled = !m_Processing;
                EnableWordMatchControls();
                m_AllLevelsChk.IsEnabled = !m_Processing;
                m_LevelNbrTbox.IsEnabled = !m_Processing && (m_AllLevelsChk.IsChecked == false);

                m_BuyAppBtn.IsEnabled = true; //!m_Processing;
                //m_ResultsPageBtn.IsEnabled = !m_Processing;

                m_StopBtn2.IsEnabled = m_Processing;
                //m_StopBtn2.Visibility = m_ProcInProgress ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void ShowAdverts(bool show)
        {
            try
            {
                //m_advert1.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        //private static int MAX_TILES = 2 * 1000 * 1000;
        private static int MAX_ICONS = 20 * 1000;

        public async Task<bool> AddFileInfoTile(StorageFile file)
        {
            try
            {
                if (m_FileInfoViewGrid == null) return false;

                //if (Util.IsFileOnAddedLibrary(m_TopFolder, file))
                //    return false;

                //if (m_FileInfoViewGrid.Items.Count > MAX_TILES)
                //    m_FileInfoViewGrid.Items.RemoveAt(0);

                Image image = null;
                if (m_FileInfoViewGrid.Items.Count <= MAX_ICONS)
                {
                    StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.DocumentsView, 30, ThumbnailOptions.None);
                    image = Util.GetImage(thumbnail);
                }

                m_LastFileInfoItem = await FileInfoDataItem.Create(file, file.Path, image, FileInfoDataSource.Object.SuccessGroup);

                m_FileInfoViewGrid.Items.Add(m_LastFileInfoItem);
                if (NBR_TILES_HOLD <= 1 || (m_FileInfoViewGrid.Items.Count % NBR_TILES_HOLD) == 0)
                {
                    m_FileInfoViewGrid.UpdateLayout();
                    m_FileInfoViewGrid.ScrollIntoView(m_LastFileInfoItem);

                    m_MatchCountNbr.Text = m_FileInfoViewGrid.Items.Count.ToString();
                }

                return true;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); return false; }
        }

        public async Task LaunchFile(StorageFile file)
        {
            try
            {
                if (file == null) return;

                bool res = await Launcher.LaunchFileAsync(file);
                if (!res)
                    await InformUser("The app was unable to open or run the selected file: " + file.Name, "Unable To Launch File");
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
        }

        public async Task LaunchFileWith(StorageFile file, FrameworkElement frameworkElem)
        {
            try
            {
                if (file == null) return;

                var options = new LauncherOptions();
                options.DisplayApplicationPicker = true;
                options.UI.InvocationPoint = GetOpenWithPoint(frameworkElem);
                options.UI.PreferredPlacement = Placement.Below;

                bool res = await Launcher.LaunchFileAsync(file, options);
                if (!res)
                    await InformUser("The system was not able to open or run the selected file: " + file.Name, "Unable To Launch File");
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
        }

        private /*async*/ void FileInfoView_ItemClick(object sender, ItemClickEventArgs ev)
        {
            //try
            //{
            //    if (m_Processing || ev == null)
            //        return;
            //    FrameworkElement fwElem = (FrameworkElement)ev.OriginalSource;
            //    fwElem.DataContext = ev.ClickedItem;
            //    m_SingleTapTimer = new System.Threading.Timer(SingleTapTimerCallback, fwElem, 400, Timeout.Infinite); // single shot timer
            //    TimeSpan singleTapSpan = DateTime.Now - m_SingleTapLastTime;
            //    if (singleTapSpan.TotalMilliseconds > 400)
            //       m_SingleTapNogo = false;
            //    m_SingleTapLastTime = DateTime.Now;
            //}
            //catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
        }

        public async void SingleTapTimerCallback(object stateInfo)
        {
            try
            {
                if (m_SingleTapNogo)
                    return;
                m_SingleTapNogo = true;
                if (stateInfo == null)
                    return;
                FrameworkElement fwElem = (FrameworkElement)stateInfo;
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    #pragma warning disable 4014
                        PopupContextMenu(fwElem);
                    #pragma warning restore 4014
                });
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
        }

        private async void FileInfoView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs ev)
        {
            try
            {
                m_SingleTapNogo = true;
                if (m_Processing || ev == null)
                    return;
                FrameworkElement frameworkElem = (FrameworkElement)ev.OriginalSource;
                FileInfoDataItem fileInfoItem = (FileInfoDataItem)frameworkElem.DataContext;
                StorageFile file = (fileInfoItem != null) ? fileInfoItem.File : null;
                await LaunchFile(file);
                ev.Handled = true;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
        }

        private /*async*/ void FileInfoView_Holding(object sender, HoldingRoutedEventArgs ev)
        {
            //try
            //{
            //    m_SingleTapNogo = true;
            //    if (m_Processing || ev == null)
            //        return;
            //    //ListView listView = (ListView)sender;
            //    FrameworkElement frameworkElem = (FrameworkElement)ev.OriginalSource;
            //    await PopupContextMenu(frameworkElem);
            //    ev.Handled = true;
            //}
            //catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
        }

        private async void FileInfoView_RightTapped(object sender, RightTappedRoutedEventArgs ev)
        {
            try
            {
                m_SingleTapNogo = true;
                if (m_Processing || ev == null)
                    return;
                //ListView listView = (ListView)sender;
                FrameworkElement frameworkElem = (FrameworkElement)ev.OriginalSource;
                await PopupContextMenu(frameworkElem);
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
        }

        private async Task PopupContextMenu(FrameworkElement frameworkElem)
        {
            try
            {
                FileInfoDataItem fileInfoItem = (FileInfoDataItem)frameworkElem.DataContext;
                StorageFile file = (fileInfoItem != null) ? fileInfoItem.File : null;
                if (file == null)
                    return;

                PopupMenu menu = new PopupMenu();
                menu.Commands.Add(new UICommand("Open", null, 1));
                menu.Commands.Add(new UICommand("Open With...", null, 2));
                //menu.Commands.Add(new UICommand("Open Folder", null, 3));
                menu.Commands.Add(new UICommandSeparator());
                menu.Commands.Add(new UICommand("Copy Folder Path", null, 3));
                menu.Commands.Add(new UICommand("Copy Full Path", null, 4));
                menu.Commands.Add(new UICommand("Properties", null, 5));
                //menu.Commands.Add(new UICommand("Delete...", null, 6));

                var cmd = await menu.ShowForSelectionAsync(GetElementRect(frameworkElem), Placement.Right);
                if (cmd != null)
                {
                    switch ((int)cmd.Id)
                    {
                        case 1: // Open
                            await LaunchFile(file);
                            break;
                        case 2: // Open With...
                            await LaunchFileWith(file, frameworkElem);
                            break;
                        //case 3: // Open Folder
                        //{
                        //    StorageFolder folder = await file.GetParentAsync();
                        //    Uri uri = new Uri("file:" + folder.Path, UriKind.Absolute); // "ms-appx://"
                        //    if (folder != null) {
                        //        bool res = await Launcher.LaunchUriAsync(uri);
                        //        Debug.WriteLine("Launcher.LaunchUriAsync() result: " + res.ToString() + ";  Uri: " + uri.AbsolutePath);
                        //    }
                        //    break;
                        //}
                        case 3: // Copy Folder Path
                        {
                            StorageFolder folder = await file.GetParentAsync();
                            if (folder != null) {
                                var dataPackage = new DataPackage();
                                dataPackage.SetText(folder.Path);
                                Clipboard.SetContent(dataPackage);
                            }
                            break;
                        }
                        case 4: // Copy Full Path
                        {
                            var dataPackage = new DataPackage();
                            dataPackage.SetText(file.Path);
                            Clipboard.SetContent(dataPackage);
                            break;
                        }
                        case 5: // Properties
                        {
                            SettingsFlyout flyout = await Util.CreatePropertiesFlyout(file, m_TopFolder, fileInfoItem.SubPath);
                            if (flyout != null)
                                flyout.Show();
                            break;
                        }
                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
                else
                    Debug.WriteLine("Context menu dismissed");
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); Debug.Assert(false); }
        }

        private Point GetOpenWithPoint(FrameworkElement el)
        {
            GeneralTransform btnTransform = el.TransformToVisual(null);
            Point desiredPoint = btnTransform.TransformPoint(new Point());
            desiredPoint.Y = desiredPoint.Y + el.ActualHeight;
            return desiredPoint;
        }

        private Rect GetElementRect(FrameworkElement elem)
        {
            GeneralTransform elemTransform = elem.TransformToVisual(null);
            Rect rect = elemTransform.TransformBounds(new Rect());
            return rect;
        }

        private async Task OnFolderChanged(bool searchAllowed)
        {
            try
            {
                if (m_TopFolder != null)
                    m_FolderUriTbx.Text = Util.FolderPath(m_TopFolder);

                if (searchAllowed && SearchAfterFolderChanged())
                    await PerformSearch(m_SearchWordsTbx.Text);
                else
                {
                    FileViewIdxFowd();
                    SetFileViewObj();
                    m_LastFileViewIdx = m_FileViewIdx;
                    m_BackStack.Push(m_CurrSearchOpts);
                    m_FowdStack.Clear();
                    ClearControls();
                    m_CurrSearchOpts = CreateCurrSearchOpts();
                    await ApplySearchOptions(m_CurrSearchOpts);
                    EnableControls();
                    SaveSettings();
                    m_FolderChanged = true;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }
        
        private async void FolderChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await PickFolder(true);
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private async void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            await PerformSearch(m_SearchWordsTbx.Text);
        }

        public SearchOptions CreateCurrSearchOpts()
        {
            WordMatchKind wordMatch = WordMatchKind.AllWords;
            if (m_MatchAnyWordRbtn.IsChecked == true)
                wordMatch = WordMatchKind.AnyWord;
            else if (m_MatchAllWordsRbtn.IsChecked == true)
                wordMatch = WordMatchKind.AllWords;
            else if (m_MatchCompletePhraseRbtn.IsChecked == true)
                wordMatch = WordMatchKind.ExactPhrase;

            string fileNames = UseRegex() ? m_NamesFilterTbx.Text : Str.RemoveChars(m_NamesFilterTbx.Text, Str.badNameChars);

            return new SearchOptions(m_ResultsId, m_SearchWordsTbx.Text, m_LastFolderToken, (m_TopFolder != null) ? m_TopFolder.Path : "",
                                     (m_TopFolder != null) ? m_TopFolder.Name : "", fileNames,
                                     CaseSensitive(), WholeWords(), UseRegex(),
                                     wordMatch, m_AllLevelsChk.IsChecked == true, GetMaxSubLevel(),
                                     m_AllFoldersCount, m_AllFilesCount, m_Cancelled);
        }

        public async Task ApplySearchOptions(SearchOptions options)
        {
            try
            {
                if (options == null) return;

                m_CurrSearchOpts = options;

                m_ResultsId = options.ResultsId;
                m_SearchWordsTbx.Text = options.SearchWords;
                m_LastFolderToken = options.TopFolderToken;
                if (!string.IsNullOrEmpty(m_LastFolderToken))
                {
                    try
                    {
                        if (StorageApplicationPermissions.FutureAccessList.ContainsItem(m_LastFolderToken))
                            m_TopFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(m_LastFolderToken);
                        else
                            { Debug.Assert(false); }
                    }
                    catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
                }
                if (m_TopFolder != null)
                    m_FolderUriTbx.Text = Util.FolderPath(m_TopFolder);
                else
                    m_FolderUriTbx.Text = !string.IsNullOrEmpty(options.TopFolderPath) ? options.TopFolderPath : options.TopFolderName;
                m_NamesFilterTbx.Text = options.FileNamePatterns;

                ApplyAdvancedOptions(options);
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public void ApplyAdvancedOptions(SearchOptions options)
        {
            try
            {
                m_CaseSensitiveChk.IsChecked = options.CaseSensitive;
                m_WholeWordsChk.IsChecked = options.WholeWords;
                m_UseRegexChk.IsChecked = options.UseRegex;

                if (options.WordMatch == WordMatchKind.AllWords)
                    m_MatchAllWordsRbtn.IsChecked = true;
                else if (options.WordMatch == WordMatchKind.AnyWord)
                    m_MatchAnyWordRbtn.IsChecked = true;
                else if (options.WordMatch == WordMatchKind.ExactPhrase)
                    m_MatchCompletePhraseRbtn.IsChecked = true;

                m_AllLevelsChk.IsChecked = options.AllDepths;
                m_LevelNbrTbox.Text = options.MaxDepth.ToString();

                ApplyResults(options);

                EnableControls();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public void ApplyResults(SearchOptions options)
        {
            try
            {
                if (options.AllFoldersCount > 0)
                {
                    m_AllFoldersCount = options.AllFoldersCount;
                    m_AllFilesCount = options.AllFilesCount;
                    m_Cancelled = options.Incomplete;
                }
                else
                {
                    m_AllFoldersCount = 0;
                    m_AllFilesCount = 0;
                    m_Cancelled = false;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }
        
        public void ClearControls()
        {
            try
            {
                if (m_FileInfoViewGrid != null)
                {
                    m_FileInfoViewGrid.Items.Clear();
                    m_FileInfoViewGrid.Visibility = Visibility.Collapsed;
                    m_FileInfoViewGrid.UpdateLayout();
                }

                m_FileCountBar.Visibility = Visibility.Collapsed;
                m_FoundFilesSeparator.Visibility = Visibility.Collapsed;
                m_FoundFilesDisplay.Visibility = Visibility.Collapsed;
                m_AllFilesCount = 0;
                m_AllFoldersCount = 0;
                m_MatchBytes = 0;
                _ExtContyp.Clear();
                _ContypExt.Clear();

                SetFileCountsVisibility(false);

                this.UpdateLayout();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public bool IsSavingResults()
        {
            return !IsNavInstant();
        }

        public void SetProcessingState(bool processing)
        {
            try
            {
                if (processing)
                {
                    m_Processing = true;
                    m_ResultsStopped = false;
                    m_ProgressRing.IsActive = true;
                    //m_ProgressBar.IsIndeterminate = true;
                    EnableControls();
                    m_FoundFilesSeparator.Visibility = Visibility.Visible;
                    m_FoundFilesDisplay.Visibility = Visibility.Visible;
                    m_FileInfoViewGrid.Visibility = Visibility.Visible;
                    SetFileCountsVisibility(processing);
                    m_Cancelled = false;
                }
                else
                {
                    m_Processing = false;
                    m_ResultsStopped = false;
                    m_ProgressRing.IsActive = false;
                    //m_ProgressBar.IsIndeterminate = false;
                    EnableControls();
                    m_FoundFilesSeparator.Visibility = (m_FileInfoViewGrid.Items.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
                    m_FoundFilesDisplay.Visibility = (m_FileInfoViewGrid.Items.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
                    m_FileInfoViewGrid.Visibility  = (m_FileInfoViewGrid.Items.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
                    m_FileInfoViewGrid.UpdateLayout();

                    if (!m_BackInProgress && !m_FowdInProgress)
                    {
                        m_CurrSearchOpts.AllFoldersCount = m_AllFoldersCount;
                        m_CurrSearchOpts.AllFilesCount = m_AllFilesCount;
                        m_CurrSearchOpts.Incomplete = m_Cancelled;
                    }

                    SetFileCountsVisibility(processing);
                    m_Cancelled = false; // must be done after SetFileCountsVisibility
                }
                this.UpdateLayout();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public void SetFileCountsVisibility(bool processing)
        {
            m_BuyAppBtn.Visibility = (IsTrial() || !IsPro()) ? Visibility.Visible : Visibility.Collapsed;
            //m_ResultsPageBtn.Visibility = (m_FileInfoViewGrid != null && m_FileInfoViewGrid.Items.Count > 0) ? Visibility.Visible : Visibility.Collapsed;

            if (m_AllFoldersCount > 0 || processing)
            {
                m_FileCountBar.Visibility  = Visibility.Visible;
                m_MatchCountLbl.Visibility = Visibility.Visible;
                m_MatchCountNbr.Visibility = Visibility.Visible;
                m_AllCountNbr.Visibility   = Visibility.Visible;

                m_MatchCountNbr.Text = m_FileInfoViewGrid.Items.Count.ToString();
                m_AllCountNbr.Text = " / " + m_AllFilesCount.ToString();
                if (m_Cancelled)
                    m_AllCountNbr.Text += "  not complete";
            }
            else
            {
                m_FileCountBar.Visibility  = Visibility.Collapsed;
                m_MatchCountLbl.Visibility = Visibility.Collapsed;
                m_MatchCountNbr.Visibility = Visibility.Collapsed;
                m_AllCountNbr.Visibility   = Visibility.Collapsed;

                m_MatchCountNbr.Text = "";
                m_AllCountNbr.Text = "";
            }
        }

        public async Task WarnUserFirstRunFolder()
        {
            try
            {
                await InformUser("No search folder has been selected yet - this seems to be the first run of the app. " +
                                 "In the next Folder Picker dialog please select the first search folder for file searches. " +
                                 "Note: The search folder is always saved by the app, and last used search folder is automatically restored on app start-up. ",
                                 "Search Folder to be Selected");
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private async Task CheckSearchAddedLibs()
        {
            try
            {
                if (string.IsNullOrEmpty(m_TopFolder.Path) && SearchAddedLibsWarn)
                {
                    string folderPath = Util.FolderPath(m_TopFolder);
                    if (!folderPath.StartsWith("Libraries"))
                        return;

                    MessageDialog msgDlg = new MessageDialog("As you selected a library under the Libraries virtual / composite folder, " +
                                                             "the selected library will be a combination of multiple libraries. It will contain, for example: " +
                                                             "your local, OneDrive/SkyDrive and public " + m_TopFolder.Name + " libraries. " +
                                                             "TIP: To select only the local library, in the Folder Picker first select This PC " +
                                                             "at the top left (not Libraries) and then select the library / folder you want to search. :-) " +
                                                             "Now however, matching files from all included " + m_TopFolder.Name + " libraries will be shown in the search results. ",
                                                             "Composite Library");
                    msgDlg.Commands.Add(new UICommand("OK", null, 0));
                    msgDlg.Commands.Add(new UICommand("OK, don't warn me again", null, 1));
                    //msgDlg.Commands.Add(new UICommand("Yes, search all folders", null, 0));
                    //msgDlg.Commands.Add(new UICommand("No, search local folder", null, 1));
                    msgDlg.DefaultCommandIndex = 0;

                    IUICommand cmdChosen = await msgDlg.ShowAsync();
                    //m_SearchAddedLibs = cmdChosen.Id.Equals(0) ? true : false;
                    if (cmdChosen.Id.Equals(1))
                    {
                        SearchAddedLibsWarn = false;
                        SaveSettings();
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public async Task PerformSearch(string searchText)
        {
            try
            {
                if (m_Processing)
                {
                    await CancelProcessingAsync();
                    if (m_Processing)
                        return;
                }

                if (m_TopFolder == null)
                {
                    await WarnUserFirstRunFolder();
                    await PickFolder(false);
                    if (m_TopFolder == null) // did user click cancel in the folder picker?
                        return;
                }

                String nameFilters = m_NamesFilterTbx.Text;

                if (UseRegex())
                {
                    if (!IsSearchTextOk(searchText))
                    {
                        await InformUser("The regular expression provided in the Search Words box is invalid: " + searchText, "Invalid Regular Expression");
                        return;
                    }
                    if (!IsSearchTextOk(nameFilters))
                    {
                        await InformUser("The regular expression provided in the File Names box is invalid: " + nameFilters, "Invalid Regular Expression");
                        return;
                    }
                }
                else
                {
                    if (Str.ContainsAnyBadNameChar(nameFilters))
                    {
                        string text = "The following characters (in the File Names box) are invalid when matching file names: " +
                                      Str.BadCharsToString() + "  Because they are not allowed and cannot appear in file names. " +
                                      "Note: Asterisk (*) can be used as a wild card character in space separated file name patterns. " +
                                      "When Regular Expressions are used, these characters may be allowed if the entered regular expression is valid. " +
                                      "So please change the invalid File Names characters or turn on Regular Expressions. ";
                        await InformUser(text, "Unsupported File Name Characters");
                        nameFilters = Str.RemoveChars(m_NamesFilterTbx.Text, Str.badNameChars);
                        return;
                    }
                }

                SearchOptions newSearchOpts = CreateCurrSearchOpts();
                //if (m_CurrSearchOpts.FullEquals(newSearchOpts))
                //{
                //    try
                //    {
                //        MessageDialog msgDlg = new MessageDialog("You are requesting a file search in the same folder with the same search options. " +
                //                                                 "Do you want to repeat the search and refresh the results?",
                //                                                 "Repeat the Same Search?");
                //        msgDlg.Commands.Add(new UICommand("No", null, 0));
                //        msgDlg.Commands.Add(new UICommand("Yes", null, 1));
                //        msgDlg.DefaultCommandIndex = 1;
                //        IUICommand cmdChosen = await msgDlg.ShowAsync();
                //        if (!cmdChosen.Id.Equals(1))
                //            return;
                //    }
                //    catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
                //}

                await CheckSearchAddedLibs();

                ++m_ResultsId;
                SetNbrTilesToHold();
                SaveSettings();

                m_BufferSize = GetBufferSize(m_TopFolder);

                List<string> searchWords = new List<string>();
                GetSearchWords(searchText, out searchWords);

                List<NamePatternList> namePatternLists = null;
                GetNamePatterns(nameFilters, out namePatternLists, UseRegex());

                if (!m_BackInProgress && !m_FowdInProgress)
                {
                    if (m_AllFoldersCount > 0 && !m_CurrSearchOpts.FullEquals(newSearchOpts))
                    {
                        if (!m_FolderChanged)
                        {
                            FileViewIdxFowd();
                            SetFileViewObj();
                            m_LastFileViewIdx = m_FileViewIdx;
                            m_BackStack.Push(m_CurrSearchOpts);
                            m_FowdStack.Clear();
                        }
                    }
                    ClearControls();
                    m_FolderChanged = false;
                    m_CurrSearchOpts = newSearchOpts;
                }

                SetProcessingState(true);

                //if (string.IsNullOrEmpty(m_TopFolder.Path))
                //    m_TopFolder = Util.GetActualFolder(m_TopFolder);

                await SearchRecursive(m_TopFolder, searchWords, namePatternLists, 0);

                m_FileInfoViewGrid.UpdateLayout();
                m_FileInfoViewGrid.ScrollIntoView(m_LastFileInfoItem);

                if (IsPro() && IsSavingResults())
                {
                    m_ResultsStopped = false;
                    await SearchResults.SaveResults(m_ResultsId, m_FileInfoViewGrid.Items, m_LastFolderToken, m_TopFolder.Path, m_TopFolder.Name);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            SetProcessingState(false);
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(m_Processing);
            m_Cancelled = true;
            m_ResultsStopped = true;
        }

        public Int32 GetMaxSubLevel()
        {
            try
            {
                Int32 level = 1000;
                bool ok = Int32.TryParse(m_LevelNbrTbox.Text, out level);
                return ok ? level : 1000;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return -1; }
        }
        
        private void LevelNbrTbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MaxSubLevel = GetMaxSubLevel();
            SaveSettings();
        }

        private async void AllLevelsChk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsPro())
                    await AskAndBuyAppAsync();
                if (!IsPro())
                    m_AllLevelsChk.IsChecked = true;

                m_LevelNbrTbox.IsEnabled = (m_AllLevelsChk.IsChecked == false);
                MaxSubLevel = GetMaxSubLevel();
                SaveSettings();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void MoreOptionsBtn_Click(object sender, RoutedEventArgs e)
        {
            m_MoreOptionsBtn.Visibility = Visibility.Collapsed;
            m_LessOptionsBtn.Visibility = Visibility.Visible;
            m_OptionsGrid.Visibility = Visibility.Visible;
            m_FileCountBar.Visibility = Visibility.Visible;
            SetFileCountsVisibility(m_Processing);
        }

        private void LessOptionsBtn_Click(object sender, RoutedEventArgs e)
        {
            //SearchOptions defaultOpts = new SearchOptions();
            //if (!m_CurrSearchOpts.AdvancedEquals(defaultOpts) && m_ResetSearchOptsQuest)
            //{
            //    MessageDialog msgDlg = new MessageDialog("Advanced search options with non-default values will be hidden (and potentially forgotten :-). " +
            //                                             "Do you want to reset the advanced search options to their default values?",
            //                                             "Reset Advanced Options?");
            //    msgDlg.Commands.Add(new UICommand("No", null, 0));
            //    msgDlg.Commands.Add(new UICommand("Yes", null, 1));
            //    msgDlg.Commands.Add(new UICommand("Yes and don't ask again", null, 2));
            //    msgDlg.DefaultCommandIndex = 2;
            //
            //    IUICommand cmdChosen = await msgDlg.ShowAsync();
            //    m_ResetSearchOpts = (cmdChosen.Id.Equals(1) || cmdChosen.Id.Equals(2)) ? true : false;
            //    if (cmdChosen.Id.Equals(2))
            //        m_ResetSearchOptsQuest = false;
            //}
            //
            //if (m_ResetSearchOpts)
            //    ApplyAdvancedOptions(defaultOpts);

            m_MoreOptionsBtn.Visibility = Visibility.Visible;
            m_LessOptionsBtn.Visibility = Visibility.Collapsed;
            m_OptionsGrid.Visibility = Visibility.Collapsed;
            SetFileCountsVisibility(m_Processing);
        }

        public async Task<bool> CancelProcessingAsync()
        {
            try
            {
                m_Cancelled = true; // cancel processing
                for (int i = 0; m_Processing && i < 100; ++i) // wait til cancelled or limit reached
                    await Task.Delay(TimeSpan.FromMilliseconds(40));

                //if (m_Processing) Debug.WriteLine("###: WARNING: Processing NOT cancelled. ");
                //else Debug.WriteLine("###: INFO: Processing CANCELLED successfully. ");
                Debug.Assert(!m_Processing);
                return !m_Processing;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return false; }
        }

        private async void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (m_BackStack.Count <= 0)
                    return;

                if (m_Processing)
                {
                    await CancelProcessingAsync();
                    if (m_Processing)
                        return;
                }

                m_FowdStack.Push(m_CurrSearchOpts);
                SearchOptions newOpts = m_BackStack.Pop();

                m_BackInProgress = true;
                EnableBackAndForward();
                await ApplySearchOptions(newOpts);

                if (IsNavInstant())
                {
                    FileViewIdxBack();
                    SetFileViewObj();
                    SetProcessingState(true);
                }
                else
                {
                    ClearControls();
                    SetProcessingState(true);
                    if (IsSavingResults())
                        await SearchResults.RestoreResults(newOpts.ResultsId);
                    else
                        await PerformSearch(m_SearchWordsTbx.Text);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            SetProcessingState(false);
            m_BackInProgress = false;
        }

        private async void FowdBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (m_FowdStack.Count <= 0)
                    return;

                if (m_Processing)
                {
                    await CancelProcessingAsync();
                    if (m_Processing)
                        return;
                }

                m_BackStack.Push(m_CurrSearchOpts);
                SearchOptions newOpts = m_FowdStack.Pop();

                m_FowdInProgress = true;
                EnableBackAndForward();
                await ApplySearchOptions(newOpts);

                if (IsNavInstant())
                {
                    FileViewIdxFowd();
                    SetFileViewObj();
                    SetProcessingState(true);
                }
                else
                {
                    ClearControls();
                    SetProcessingState(true);
                    if (IsSavingResults())
                        await SearchResults.RestoreResults(newOpts.ResultsId);
                    else
                        await PerformSearch(m_SearchWordsTbx.Text);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            SetProcessingState(false);
            m_FowdInProgress = false;
        }

        private async void UpBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsPro())
                    await AskAndBuyAppAsync();
                if (!IsPro())
                    return;

                if (m_TopFolder == null) return;

                StorageFolder upFolder = await m_TopFolder.GetParentAsync();
                if (upFolder == null)
                {
                    await InformUser("This folder has not been accessed before (either directly or by its parent/ancestor) using the Folder Picker. " +
                                      "In the next Folder Picker dialog please select a folder. TIP: If you select the top folder of a drive (e.g. C:\\) " +
                                      "you will then have easy access to all folders, sub-folders and files on that drive.",
                                      "First Time Folder Access");
                    await PickFolder(true);
                }
                else
                {
                    m_TopFolder = upFolder;
                    await OnFolderChanged(true);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void ClearResBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }
        
        private async void UseRegexChk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsPro())
                    await AskAndBuyAppAsync();
                if (!IsPro())
                    m_UseRegexChk.IsChecked = false;

                if (UseRegex())
                {
                    m_WholeWordsChk.IsChecked = false;
                    m_MatchAllWordsRbtn.IsChecked = true;
                }
                EnableWordMatchControls();
                SaveSettings();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private async void CaseSensitiveChk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsPro())
                    await AskAndBuyAppAsync();
                if (!IsPro())
                    m_CaseSensitiveChk.IsChecked = false;
                SaveSettings();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private async void WholeWordsChk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsPro())
                    await AskAndBuyAppAsync();
                if (!IsPro())
                    m_WholeWordsChk.IsChecked = false;
                SaveSettings();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private async void MatchAnyWordRbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsPro())
                    await AskAndBuyAppAsync();
                if (!IsPro())
                    m_MatchAllWordsRbtn.IsChecked = true;
                SaveSettings();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private async void MatchAllWordsRbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsPro())
                    await AskAndBuyAppAsync();
                if (!IsPro())
                    m_MatchAllWordsRbtn.IsChecked = true;
                SaveSettings();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private async void MatchCompletePhraseRbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsPro())
                    await AskAndBuyAppAsync();
                if (!IsPro())
                    m_MatchAllWordsRbtn.IsChecked = true;
                SaveSettings();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void SearchWordsTbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            //SetPlaceholderProps(m_SearchWordsTbx);
        }

        private void NamesFilterTbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            //SetPlaceholderProps(m_NamesFilterTbx);
        }

        private void NoOpBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var helpFo = new HelpFlyout();
                helpFo.Show();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private async void BuyAppBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await BuyAppAsync();
                //if (IsAppSrcCodePro)
                //{
                //    if (IsTrialExpired() || !m_IsActiveLic)
                //        Application.Current.Exit();
                //}
            }
            catch (Exception ex) { ProcessException(ex); }
        }

        private void ResultsPageBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Frame.Navigate(typeof(SearchResultsPage), this);
            }
            catch (Exception ex) { ProcessException(ex); }
        }

        public void SettingsCharm_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            try
            {
                var about = new SettingsCommand("about", "About", handler => new AboutFlyout().Show());
                args.Request.ApplicationCommands.Add(about);

                var help = new SettingsCommand("help", "Help", handler => new HelpFlyout().Show());
                args.Request.ApplicationCommands.Add(help);

                var privacy = new SettingsCommand("privacypolicy", "Privacy Policy", handler => new PrivacyFlyout().Show());
                args.Request.ApplicationCommands.Add(privacy);
            }
            catch (Exception ex) { ProcessException(ex); }
        }

        private void SetPlaceholderProps(TextBox textBox)
        {
            try
            {
                // WARNING: IT DOES NOT WORK PROPERLY, THE FONT IS CHANGED LATE / ERRATICALLY !!!
                if (string.IsNullOrEmpty(m_SearchWordsTbx.Text))
                {
                    textBox.Opacity = 0.8;
                    textBox.FontWeight = FontWeights.Light;
                }
                else
                {
                    textBox.Opacity = 1.0;
                    textBox.FontWeight = FontWeights.SemiBold;
                }
            }
            catch (Exception ex) { ProcessException(ex); }
        }

        public async Task<bool> ConfirmCancelAsync()
        {
            try
            {
                MessageDialog msgDlg = new MessageDialog("Please confirm that you want to stop the search.", "Confirm Stop");
                msgDlg.Commands.Add(new UICommand("Continue", null, 0));
                msgDlg.Commands.Add(new UICommand("Stop", null, 1));
                msgDlg.DefaultCommandIndex = 1;

                // popup to user
                var cmdChosen = await msgDlg.ShowAsync();

                return cmdChosen.Id.Equals(1);
            }
            catch (Exception ex) { ProcessException(ex); }
            return false;
        }

        private async void ItemsPage_KeyDown(object sender, KeyRoutedEventArgs ev)
        {
            try
            {
                // NOTE: Do NOT await in this method, and do set ev.Handled = true.
                // How to check for Ctrl or Shift: ctrl or alt.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down)

                var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                var alt  = Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu); // alt key is called menu !!!

                if (ev.Key == VirtualKey.Escape)
                {
                    if (m_Processing)
                    {
                        bool firm = await ConfirmCancelAsync();
                        if (firm)
                        {
                            ev.Handled = true;
                            StopBtn_Click(null, null);
                        }
                    }
                }
                else if (ev.Key == VirtualKey.Enter)
                {
                    if (!m_Processing)
                    {
                        ev.Handled = true;
                        #pragma warning disable 4014
                            PerformSearch(m_SearchWordsTbx.Text); // do NOT await
                        #pragma warning restore 4014
                    }
                }
                else if ((ev.Key == VirtualKey.Back && ctrl.HasFlag(CoreVirtualKeyStates.Down)) || // backspace key is called back !!!
                         (ev.Key == VirtualKey.Left &&  alt.HasFlag(CoreVirtualKeyStates.Down)))
                {
                    ev.Handled = true;
                    if (m_BackBtn.IsEnabled)
                        BackBtn_Click(sender, null); // do NOT await
                }
                else if (ev.Key == VirtualKey.Right && alt.HasFlag(CoreVirtualKeyStates.Down))
                {
                    ev.Handled = true;
                    if (m_FowdBtn.IsEnabled)
                        FowdBtn_Click(sender, null); // do NOT await
                }
                else if (ev.Key == VirtualKey.Up && alt.HasFlag(CoreVirtualKeyStates.Down))
                {
                    ev.Handled = true;
                    if (m_UpBtn.IsEnabled)
                        UpBtn_Click(sender, null); // do NOT await
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public static void ProcessException(Exception ex)
        {
            ProcessException(ex, "A processing error occurred. ");
        }

        public static void ProcessException(Exception ex, string introText)
        {
            try
            {
                Debug.WriteLine("###");
                Debug.WriteLine(introText + " " + ex.ToString());
            }
            catch (Exception) { }
        }

        public async Task InformUser(string text, string dlgTitle)
        {
            try
            {
                MessageDialog msgDlg = new MessageDialog(text, dlgTitle);
                msgDlg.Commands.Add(new UICommand("OK", null, 0));
                msgDlg.DefaultCommandIndex = 0;

                // popup to user
                await msgDlg.ShowAsync();
            }
            catch (Exception ex) { ProcessException(ex); }
        }

        #region Licensing Code ########################################################################################

        private LicenseInformation GetLicInfo()
        {
            return TEST_MODE ? CurrentAppSimulator.LicenseInformation : CurrentApp.LicenseInformation;
        }

        private async Task ReloadTestModeLicense()
        {
            try
            {
                if (!TEST_MODE)
                    return;

                StorageFolder installFolder = Package.Current.InstalledLocation;
                StorageFolder proxyDataFolder = await installFolder.GetFolderAsync("Data");
                StorageFile proxyFile = await proxyDataFolder.GetFileAsync("in-app-purchase.xml");
                await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
            }
            catch (Exception ex) { ProcessException(ex); }
        }

        public async Task RefreshLicenseInfo()
        {
            try
            {
                if (TEST_MODE)
                    await ReloadTestModeLicense();

                if (m_licenseChangeHandler == null)
                {
                    m_licenseChangeHandler = new LicenseChangedEventHandler(LicenseChangedRefresh);
                    GetLicInfo().LicenseChanged += m_licenseChangeHandler;
                }
                LicenseInformation licInfo = GetLicInfo();
                if (licInfo == null)
                    return;
                m_IsActiveLic = licInfo.IsActive;
                m_IsTrialLic = licInfo.IsTrial;
            }
            catch (Exception ex) { ProcessException(ex); }
        }

        private async void LicenseChangedRefresh()
        {
            //m_LicChanged = true;
            try
            {
                await RefreshLicenseInfo();
                // IMPORTANT: Must NOT touch/use any UI elements in this fun! Eg. must not call: EnableControls();
            }
            catch (Exception ex) { ProcessException(ex); }
        }

        // http://msdn.microsoft.com/en-us/library/windows/apps/windows.applicationmodel.store.licenseinformation(v=win.10).aspx
        // MSDN: For trial versions of an app, IsActive will return true so long as the trial hasn’t expired.
        // During the trial period the IsTrial returns true; returning false when the customer upgrades to the full version of the app.
        public bool IsFullLicense()
        {
            if (IsAppSrcCodePro)
                return m_IsActiveLic && !m_IsTrialLic;
            else
                return IsPro();
        }

        public bool IsPro()
        {
            return IsAppSrcCodePro ? true : IsProFromExpress();
        }

        public bool IsProFromExpress()
        {
            try
            {
                if (IsAppSrcCodePro) return false;

                LicenseInformation licInfo = GetLicInfo();
                ProductLicense productLic = (licInfo != null) ? licInfo.ProductLicenses["ExpressToPro_PRID"] : null;
                return (productLic != null && productLic.IsActive);
            }
            catch (Exception ex) { ProcessException(ex); return true; }
        }

        public bool IsTrial()
        {
            return IsAppSrcCodePro && m_IsTrialLic;
        }

        public bool IsTrialExpired()
        {
            return  IsAppSrcCodePro && m_IsTrialLic && DateTime.Now >= GetLicInfo().ExpirationDate;
        }

        public void SetAppTitle()
        {
            try
            {
                if (IsAppSrcCodePro)
                {
                    if (IsTrial())
                        m_TitleTblk.Text = APP_NAME + " Pro (Trial)";
                    else
                        m_TitleTblk.Text = APP_NAME + " Pro";
                }
                else if (IsProFromExpress())
                    m_TitleTblk.Text = APP_NAME + " Express + Pro";
                else
                    m_TitleTblk.Text = APP_NAME + " Express";
            }
            catch (Exception ex) { ProcessException(ex); }
        }

        public string GetAppTitle()
        {
            return (m_TitleTblk != null) ? m_TitleTblk.Text : "";
        }
        
        public async Task<string> GetPriceTextAsync()
        {
            try
            {
                ListingInformation listing = await CurrentApp.LoadListingInformationAsync();
                string text = " Current price is only " + listing.FormattedPrice + ". ";
                //Debug.WriteLine(APP_LOG_PREFIX + text);
                return text;
            }
            catch (Exception ex) { ProcessException(ex); return ""; }
        }

        public string GetTrialRemainText()
        {
            try
            {
                if (!IsTrial())
                    return " ";

                if (IsTrialExpired())
                    return "Your trial has expired. ";

                TimeSpan timeRemain = GetLicInfo().ExpirationDate - DateTime.Now;
                string text = "Your trial will expire in about ";

                if (GetLicInfo().ExpirationDate.Year <= 0 || GetLicInfo().ExpirationDate.Year >= 9999)
                    text = "Your trial will not expire. ";
                else if (timeRemain.TotalDays >= 365.0)
                    text += string.Format("{0} {1}. ", IntToIntStr((int)timeRemain.TotalDays / 365, "year"), IntToIntStr((int)timeRemain.TotalDays % 365, "day"));
                else if (timeRemain.TotalDays >= 7.0)
                    text += string.Format("{0}. ", DblToIntStr(timeRemain.TotalDays, "day"));
                else if (timeRemain.TotalHours >= 24.0)
                    text += string.Format("{0} {1}. ", IntToIntStr(timeRemain.Days, "day"), IntToIntStr(timeRemain.Hours, "hour"));
                else if (timeRemain.TotalMinutes >= 60.0)
                    text += string.Format("{0} {1}. ", IntToIntStr(timeRemain.Hours, "hour"), IntToIntStr(timeRemain.Minutes, "minute"));
                else
                    text += string.Format("{0} {1}. ", IntToIntStr(timeRemain.Minutes, "minute"), IntToIntStr(timeRemain.Seconds, "second"));

                Debug.WriteLine(text);
                return text;
            }
            catch (Exception ex) { ProcessException(ex); return ""; }
        }

        public string DblToIntStr(double dblValue, string unitStr)
        {
            int intValue = (int)(dblValue);  //(int)(dblValue + 0.5);
            return IntToIntStr(intValue, unitStr);
        }

        public string IntToIntStr(int intValue, string unitStr)
        {
            //if (intValue == 0)
            //    return "";
            //else
            return string.Format("{0} {1}", intValue, (intValue == 1) ? unitStr : (unitStr + "s"));
        }

        public async Task BuyAppAsync()
        {
            try
            {
                Exception exc = null;
                try
                {
                    if (IsAppSrcCodePro)
                    {
                        if (TEST_MODE)
                            await CurrentAppSimulator.RequestAppPurchaseAsync(false);
                        else
                            await CurrentApp.RequestAppPurchaseAsync(false); // BUY APP
                    }
                    else
                    {
                        if (TEST_MODE)
                            await CurrentAppSimulator.RequestProductPurchaseAsync("ExpressToPro_PRID");
                        else
                            await CurrentApp.RequestProductPurchaseAsync("ExpressToPro_PRID"); // BUY PRODUCT
                    }
                }
                catch (Exception ex)
                {
                    exc = ex;
                }
                await RefreshLicenseInfo();
                EnableControls();

                string trialOrExpress = IsAppSrcCodePro ? "the trial license of the Pro edition. "+ GetTrialRemainText()  :  "the Express edition. ";
                //string inTrial = IsAppSrcCodePro ? " during the trial." : ".";

                if (IsFullLicense())
                {
                    m_BuyAppBtn.Visibility = Visibility.Collapsed;
                    await InformUser("Thank you for buying the app. You successfully bought the full permanent license of the " + APP_NAME + " Pro edition. :-) ", // +
                                     //"(Also, no ads will be displayed any more :-) ",
                                     "Purchase Successful - Thanks");
                }
                else
                {
                    m_BuyAppBtn.Visibility = Visibility.Visible;
                    string notPerformed = "The purchase of the full permanent license of " + APP_NAME + " Pro has not been performed. ";
                    if (IsAppSrcCodePro)
                    {
                        if (IsTrial() && m_IsActiveLic)
                            notPerformed += "You still have " + trialOrExpress; //+ " Ads will still be displayed" + inTrial;
                        else if (IsTrialExpired())
                            notPerformed += "Your trial has expired. ";
                    }
                    else
                        notPerformed += "You still have the free Express edition of the app. ";
                    await InformUser(notPerformed, "Purchase Not Performed");
                }

                return;
            }
            catch (Exception ex) { ProcessException(ex); }
            await RefreshLicenseInfo();
            EnableControls();
        }

        public async Task AskAndBuyAppAsync()
        {
            try
            {
                await RefreshLicenseInfo();
                if (IsFullLicense())
                    return;

                string title = IsAppSrcCodePro ? "Buy " + APP_NAME + " Permanent License?" : "Buy " + APP_NAME + " Pro?";
                string text = "";
                if (IsAppSrcCodePro)
                    text = "Thanks for trying the Pro edition of this app. " + GetTrialRemainText() + /*"Ads will be displayed during the trial. " +*/
                           "Do you want to buy the app now and get the full permanent license? "; // (and remove the ads)
                else
                    text = "This is the free Express edition of the app with limited features. However, the Pro edition provides all features: " +
                            "1. Instant previous results with go Back and Forward; " +
                            "2. Advanced search options (e.g. Whole Words match, Max Sub-folder Depth, etc.); " +
                            "3. Advanced search using Regular Expressions. " +
                            "Do you want to buy the Pro edition of " + APP_NAME + " now and get the full functionality? ";

                string noBtn = "No Thanks";
                if (IsAppSrcCodePro)
                {
                    if (IsTrialExpired() || !m_IsActiveLic)
                    {
                        title = "Buy App or Exit";
                        text = "Sorry, your trial has expired. :-( To continue using the app you must buy it. ";
                        noBtn = "Exit";
                    }
                    // takes a long time: text += await GetPriceTextAsync();
                }

                MessageDialog msgDlg = new MessageDialog(text, title);
                msgDlg.Commands.Add(new UICommand("Buy Pro", null, 0));
                msgDlg.Commands.Add(new UICommand(noBtn, null, 1));
                msgDlg.DefaultCommandIndex = 0;
                var cmdChosen = await msgDlg.ShowAsync(); // popup to user
                if (cmdChosen.Id.Equals(0))
                    await BuyAppAsync();

                if (IsAppSrcCodePro)
                {
                    if (IsTrialExpired() || !m_IsActiveLic)
                        Application.Current.Exit();
                }
            }
            catch (Exception ex) { ProcessException(ex); }
        }

        #endregion ####################################################################################################

        #region Search Helpers ########################################################################################

        public class Str
        {
            public static String RemoveChar(String str, Char ch)
            {
                try
                {
                    char[] seps = new char[] { ch };
                    string[] strs = str.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                    string outStr = "";
                    foreach (var s in strs)
                        outStr += s;
                    return outStr;
                }
                catch (Exception ex) { Debug.WriteLine(ex.ToString()); return ""; }
            }

            public static String RemoveChars(String str, Char[] chars)
            {
                try
                {
                    string[] strs = str.Split(chars, StringSplitOptions.RemoveEmptyEntries);
                    string outStr = "";
                    foreach (var s in strs)
                        outStr += s;
                    return outStr;
                }
                catch (Exception ex) { Debug.WriteLine(ex.ToString()); return ""; }
            }

            public static int CountChar(String str, Char ch)
            {
                try
                {
                    if (String.IsNullOrEmpty(str))
                        return 0;
                    int cnt = 0;
                    for (int i = 0; i < str.Length; ++i)
                    {
                        if (str[i] == ch)
                            ++cnt;
                    }
                    return cnt;
                }
                catch (Exception ex) { Debug.WriteLine(ex.ToString()); return 0; }
            }

            //public static char[] badNameChars = { '\\', '/', ':', '?', '"',  '<', '>', '|', '*' }; // includes *
            public static char[]   badNameChars = { '\\', '/', ':', '?', '"',  '<', '>', '|' };
            public static string[] badNameStrs  = { "\\", "/", ":", "?", "\"", "<", ">", "|" };
            //public static string badNameStrsText = "back slash (\\)  " +
            //                                        "forward slash (/)  " +
            //                                        "colon (:)  " +
            //                                        "question mark (?)  " +
            //                                        "double quote (\")  " +
            //                                        "less than (<)  " +
            //                                        "greater than (>)  " +
            //                                        "vertical pipe (|)";

            public static bool ContainsAnyBadNameChar(String str)
            {
                try
                {
                    if (String.IsNullOrEmpty(str))
                        return false;

                    string[] results = str.Split(badNameChars, StringSplitOptions.RemoveEmptyEntries);
                    Debug.Assert(results.Length >= 0);
                    if (results.Length <= 0)
                        return true;

                    return (results.Length >= 2 || results[0] != str);
                }
                catch (Exception ex) { Debug.WriteLine(ex.ToString()); return false; }
            }

            public static String BadCharsToString()
            {
                try
                {
                    String str = "";
                    for (int i = 0; i < badNameStrs.Length; ++i)
                        str += badNameStrs[i] + " ";
                    return str;
                }
                catch (Exception ex) { Debug.WriteLine(ex.ToString()); return ""; }
            }
        }

        public enum MatchKind
        {
            StartsWith,
            Contains,
            EndsWith,
            Equals
        }

        public class NamePattern
        {
            public NamePattern(string pattern, MatchKind matchKind)
            {
                this.pat = pattern;
                this.kind = matchKind;
            }
            public string pat;
            public MatchKind kind;
        }

        public class NamePatternList : List<NamePattern>
        {
            public NamePatternList()
                : base()
            {
            }
            public NamePatternList(Int32 capacity)
                : base(capacity)
            {
            }
        }

        public static char cSTAR = '*';
        public static string sSTAR = "*";
        public static char[] seps1 = new char[] { ' ' };
        public static char[] seps2 = new char[] { cSTAR };

        public static void GetNamePatterns(String rawNamePatters, out List<NamePatternList> namePatternLists, bool useRegex)
        {
            namePatternLists = new List<NamePatternList>();
            if (String.IsNullOrEmpty(rawNamePatters))
                return;
            try
            {
                if (useRegex)
                {
                    var namePatterns = new NamePatternList();
                    namePatternLists.Add(namePatterns);
                    namePatterns.Add(new NamePattern(rawNamePatters, MatchKind.Contains));
                    return;
                }

                string[] tempPatterns = rawNamePatters.Split(seps1, StringSplitOptions.RemoveEmptyEntries);
                foreach (var tp in tempPatterns)
                {
                    string tempPat = tp.Trim();
                    if (string.IsNullOrEmpty(tempPat) || tempPat.Equals(";"))
                        continue;

                    var namePatterns = new NamePatternList();
                    namePatternLists.Add(namePatterns);

                    string[] procPatterns = tempPat.Split(seps2, StringSplitOptions.RemoveEmptyEntries);

                    if (Str.CountChar(tempPat, cSTAR) >= 3)
                    {
                        foreach (var procPat in procPatterns)
                            namePatterns.Add(new NamePattern(procPat, MatchKind.Contains));
                    }
                    else if (Str.CountChar(tempPat, cSTAR) == 2)
                    {
                        if (tempPat.StartsWith(sSTAR))
                        {
                            if (tempPat.EndsWith(sSTAR))
                            {
                                string procPat = Str.RemoveChar(tempPat, cSTAR); //tempPat.Substring(1, tempPat.Length - 2);
                                namePatterns.Add(new NamePattern(procPat, MatchKind.Contains));
                            }
                            else
                            {
                                namePatterns.Add(new NamePattern(procPatterns[0], MatchKind.Contains));
                                namePatterns.Add(new NamePattern(procPatterns[1], MatchKind.EndsWith));
                            }
                        }
                        else if (tempPat.EndsWith(sSTAR))
                        {
                            namePatterns.Add(new NamePattern(procPatterns[0], MatchKind.StartsWith));
                            namePatterns.Add(new NamePattern(procPatterns[1], MatchKind.Contains));
                        }
                        else
                        {
                            Debug.Assert(!tempPat.StartsWith(sSTAR) && !tempPat.EndsWith(sSTAR));
                            Debug.Assert(procPatterns.Length == 2 || procPatterns.Length == 3);
                            if (procPatterns.Length == 2)
                            {
                                namePatterns.Add(new NamePattern(procPatterns[0], MatchKind.StartsWith));
                                namePatterns.Add(new NamePattern(procPatterns[1], MatchKind.EndsWith));
                            }
                            else if (procPatterns.Length == 3)
                            {
                                namePatterns.Add(new NamePattern(procPatterns[0], MatchKind.StartsWith));
                                namePatterns.Add(new NamePattern(procPatterns[1], MatchKind.Contains));
                                namePatterns.Add(new NamePattern(procPatterns[2], MatchKind.EndsWith));
                            }
                            else // just in case (should never happen :-)
                            {
                                foreach (var procPat in procPatterns)
                                    namePatterns.Add(new NamePattern(procPat, MatchKind.Contains));
                            }
                        }
                    }
                    else if (Str.CountChar(tempPat, cSTAR) == 1)
                    {
                        if (tempPat.StartsWith(sSTAR))
                        {
                            string procPat = Str.RemoveChar(tempPat, cSTAR); //tempPat.Substring(1, tempPat.Length - 1);
                            namePatterns.Add(new NamePattern(procPat, MatchKind.EndsWith));
                        }
                        else if (tempPat.EndsWith(sSTAR))
                        {
                            string procPat = Str.RemoveChar(tempPat, cSTAR); //tempPat.Substring(0, tempPat.Length - 1);
                            namePatterns.Add(new NamePattern(procPat, MatchKind.StartsWith));
                        }
                        else
                        {
                            Debug.Assert(procPatterns.Length == 2);
                            if (procPatterns.Length == 2)
                            {
                                namePatterns.Add(new NamePattern(procPatterns[0], MatchKind.StartsWith));
                                namePatterns.Add(new NamePattern(procPatterns[1], MatchKind.EndsWith));
                            }
                            else // just in case (should never happen :-)
                            {
                                foreach (var procPat in procPatterns)
                                    namePatterns.Add(new NamePattern(procPat, MatchKind.Contains));
                            }
                        }
                    }
                    else
                    {
                        Debug.Assert(!tempPat.Contains(sSTAR));
                        if (!tempPat.Contains(sSTAR))
                            namePatterns.Add(new NamePattern(tempPat, MatchKind.Contains)); // or MatchKind.Equals
                        else // just in case (should never happen :-)
                        {
                            foreach (var procPat in procPatterns)
                                namePatterns.Add(new NamePattern(procPat, MatchKind.Contains));
                        }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private bool StringContainsAnyPattern(String theString, List<NamePattern> namePatterns)
        {
            String stringLower = theString.ToLower();
            foreach (NamePattern pattern in namePatterns)
            {
                if (m_Cancelled)
                    return false;

                if (pattern.kind == MatchKind.StartsWith)
                {
                    if (stringLower.StartsWith(pattern.pat.ToLower()))
                        return true;
                }
                else if (pattern.kind == MatchKind.EndsWith)
                {
                    if (stringLower.EndsWith(pattern.pat.ToLower()))
                        return true;
                }
                //else if (pattern.kind == MatchKind.Equals)
                //{
                //    if (stringLower.Equals(pattern.pat.ToLower()))
                //        return true;
                //}
                else
                {
                    if (stringLower.Contains(pattern.pat.ToLower()))
                        return true;
                }
            }
            return false;
        }

        private bool StringContainsAllPatterns(String theString, List<NamePattern> namePatterns)
        {
            String stringLower = theString.ToLower();
            foreach (NamePattern pattern in namePatterns)
            {
                if (m_Cancelled)
                    return false;

                if (pattern.kind == MatchKind.StartsWith)
                {
                    if (!stringLower.StartsWith(pattern.pat.ToLower()))
                        return false;
                }
                else if (pattern.kind == MatchKind.EndsWith)
                {
                    if (!stringLower.EndsWith(pattern.pat.ToLower()))
                        return false;
                }
                //else if (pattern.kind == MatchKind.Equals)
                //{
                //    if (!stringLower.Equals(pattern.pat.ToLower()))
                //        return false;
                //}
                else
                {
                    if (!stringLower.Contains(pattern.pat.ToLower()))
                        return false;
                }
            }
            return true;
        }

        //public bool HasBinExt(StorageFile file)
        //{
        //    try
        //    {
        //        String ext = file.FileType.ToLower();
        //        if (ext.StartsWith(".exe") || ext.StartsWith(".dll") || ext.StartsWith(".com") || ext.StartsWith(".lib") ||
        //            ext.StartsWith(".obj") || ext.StartsWith(".pdb") || ext.StartsWith(".idb") || ext.StartsWith(".bin") ||
        //            ext.StartsWith(".txt") || ext.StartsWith(".dat") || ext.StartsWith(".xap") || ext.StartsWith(".appxupload"))
        //            return true;
        //        return false;
        //    }
        //    catch (Exception ex) { Debug.WriteLine(ex.ToString()); return false; }
        //}

        //public bool HasTextExt(StorageFile file)
        //{
        //    try
        //    {
        //        String ext = file.FileType.ToLower();
        //        if (ext.StartsWith(".c") || ext.StartsWith(".h") || ext.StartsWith(".cpp") || ext.StartsWith(".hpp") ||
        //            ext.StartsWith(".cxx") || ext.StartsWith(".hxx") || ext.StartsWith(".cc") || ext.StartsWith(".hh") ||
        //            ext.StartsWith(".txt") || ext.StartsWith(".text") || ext.StartsWith(".bat"))
        //            return true;
        //        return false;
        //    }
        //    catch (Exception ex) { Debug.WriteLine(ex.ToString()); return false; }
        //}

        //public bool IsPlainTextFile(StorageFile file)
        //{
        //    try
        //    {
        //        String contType = file.ContentType.ToLower();
        //        if (!IsEmpty(contType))
        //        {
        //            return contType.StartsWith("text") || (contType.StartsWith("application/") &&
        //                                                   (contType.Contains("xml") || contType.Contains("xaml") || contType.Contains("script") || contType.Contains("json")));
        //        }
        //        else
        //            return true; //HasTextExt(file);
        //    }
        //    catch (Exception ex) { Debug.WriteLine(ex.ToString()); return false;  }
        //}

        #endregion ####################################################################################################
    }
}
