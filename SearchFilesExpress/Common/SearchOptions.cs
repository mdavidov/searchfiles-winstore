using System;
using System.Diagnostics;
using Windows.Storage;

namespace SearchFiles.Common
{
    public enum WordMatchKind
    {
        AllWords,
        AnyWord,
        ExactPhrase
    }

    public class SearchOptions : IComparable<SearchOptions>, IEquatable<SearchOptions>
    {
        public string SearchWords  { get; set; }
        public string TopFolderToken { get; set; }
        public string TopFolderPath { get; set; }
        public string TopFolderName { get; set; }
        public string FileNamePatterns { get; set; }
        public bool CaseSensitive { get; set; }
        public bool WholeWords { get; set; }
        public bool UseRegex { get; set; }
        public WordMatchKind WordMatch { get; set; }
        public bool AllDepths { get; set; }
        public int MaxDepth { get; set; }

        public const string ResultsIdKey = "ResultsIdKey";
        public const string SearchWordsKey = "SearchWordsKey";
        public const string TopFolderTokenKey = "FolderTokenKey";
        public const string TopFolderPathKey = "TopFolderPathKey";
        public const string TopFolderNameKey = "TopFolderNameKey";
        public const string FileNamePatternsKey = "FileNamePatternsKey";
        public const string CaseSensitiveKey = "CaseSensitiveKey";
        public const string WholeWordsKey = "WholeWordsKey";
        public const string UseRegexKey = "UseRegexKey";
        public const string WordMatchKey = "WordMatchKey";
        public const string AllDepthsKey = "AllDepthsKey";
        public const string MaxDepthKey = "MaxDepthKey";
        public const string AllFoldersCountKey = "AllFoldersCountKey";
        public const string AllFilesCountKey = "AllFilesCountKey";
        public const string IncompleteKey = "IncompleteKey";

        //public ulong SizeMin { get; set; }
        //public ulong SizeMax { get; set; }
        //public DateTimeOffset DateModified { get; set; }
        //public DateTimeOffset DateCreated { get; set; }
        //public DateTimeOffset DateAccessed { get; set; }

        #region Results Vars
        // Results
        public int ResultsId { get; set; }
        public ulong AllFoldersCount { get; set; }
        public ulong AllFilesCount { get; set; }
        public bool Incomplete { get; set; }
        #endregion

        public SearchOptions()
        {
            ResultsId = -1;
            SearchWords = "";
            TopFolderToken = "";
            TopFolderPath = "";
            TopFolderName = "";
            FileNamePatterns = "";
            CaseSensitive = false;
            WholeWords = false;
            UseRegex = false;
            WordMatch = WordMatchKind.AllWords;
            AllDepths = true;
            MaxDepth = 0;
            AllFoldersCount = 0;
            AllFilesCount = 0;
            Incomplete = false;
        }

        public SearchOptions(int resultsId, string searchWords, string topFolderToken, string topFolderPath, string topFolderName, string fileNamePatterns,
                             bool caseSensitive, bool wholeWords, bool useRegex, WordMatchKind wordMatch, bool allDepths, int maxDepth,
                             ulong allFoldersCount, ulong allFilesCount, bool incomplete)
        {
            ResultsId = resultsId;
            SearchWords = searchWords;
            TopFolderToken = topFolderToken;
            TopFolderPath = topFolderPath;
            TopFolderName = topFolderName;
            FileNamePatterns = fileNamePatterns;
            CaseSensitive = caseSensitive;
            WholeWords = wholeWords;
            UseRegex = useRegex;
            WordMatch = wordMatch;
            AllDepths = allDepths;
            MaxDepth = maxDepth;
            AllFoldersCount = allFoldersCount;
            AllFilesCount = allFilesCount;
            Incomplete = incomplete;
        }

        public void SaveInSettings(ApplicationDataContainer localSettings)
        {
            try
            {
                if (localSettings == null) return;

                localSettings.Values[ResultsIdKey]          = ResultsId;
                localSettings.Values[SearchWordsKey]        = SearchWords;
                localSettings.Values[TopFolderTokenKey]     = TopFolderToken;
                localSettings.Values[TopFolderPathKey]      = TopFolderPath;
                localSettings.Values[TopFolderNameKey]      = TopFolderName;
                localSettings.Values[FileNamePatternsKey]   = FileNamePatterns;
                localSettings.Values[CaseSensitiveKey]      = CaseSensitive;
                localSettings.Values[WholeWordsKey]         = WholeWords;
                localSettings.Values[UseRegexKey]           = UseRegex;
                localSettings.Values[WordMatchKey]          = (int) WordMatch;
                localSettings.Values[AllDepthsKey]          = AllDepths;
                localSettings.Values[MaxDepthKey]           = MaxDepth;
                localSettings.Values[AllFoldersCountKey]    = AllFoldersCount;
                localSettings.Values[AllFilesCountKey]      = AllFilesCount;
                localSettings.Values[IncompleteKey]         = Incomplete;
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public void RestoreFromSettings(ApplicationDataContainer localSettings)
        {
            try
            {
                if (localSettings == null) return;

                if (localSettings.Values.ContainsKey(ResultsIdKey))
                    ResultsId           = (int) localSettings.Values[ResultsIdKey];
                if (localSettings.Values.ContainsKey(SearchWordsKey))
                    SearchWords         = (string) localSettings.Values[SearchWordsKey];
                if (localSettings.Values.ContainsKey(TopFolderTokenKey))
                    TopFolderToken      = (string) localSettings.Values[TopFolderTokenKey];
                if (localSettings.Values.ContainsKey(TopFolderPathKey))
                    TopFolderPath       = (string) localSettings.Values[TopFolderPathKey];
                if (localSettings.Values.ContainsKey(TopFolderNameKey))
                    TopFolderName = (string)localSettings.Values[TopFolderNameKey];
                if (localSettings.Values.ContainsKey(FileNamePatternsKey))
                    FileNamePatterns    = (string) localSettings.Values[FileNamePatternsKey];
                if (localSettings.Values.ContainsKey(CaseSensitiveKey))
                    CaseSensitive       = (bool) localSettings.Values[CaseSensitiveKey];
                if (localSettings.Values.ContainsKey(WholeWordsKey))
                    WholeWords          = (bool)localSettings.Values[WholeWordsKey];
                if (localSettings.Values.ContainsKey(UseRegexKey))
                    UseRegex            = (bool)localSettings.Values[UseRegexKey];
                if (localSettings.Values.ContainsKey(WordMatchKey))
                    WordMatch           = (WordMatchKind) ((int) localSettings.Values[WordMatchKey]);
                if (localSettings.Values.ContainsKey(AllDepthsKey))
                    AllDepths           = (bool) localSettings.Values[AllDepthsKey];
                if (localSettings.Values.ContainsKey(MaxDepthKey))
                    MaxDepth            = (int) localSettings.Values[MaxDepthKey];
                if (localSettings.Values.ContainsKey(AllFoldersCountKey))
                    AllFoldersCount     = 0; // NOTE always save 0 //(ulong)localSettings.Values[AllFoldersCountKey];
                if (localSettings.Values.ContainsKey(AllFilesCountKey))
                    AllFilesCount       = 0; // NOTE always save 0 //(ulong)localSettings.Values[AllFilesCountKey];
                if (localSettings.Values.ContainsKey(IncompleteKey))
                    Incomplete          = false; // NOTE always save false //(bool)localSettings.Values[IncompleteKey];
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }
        
        private static string sep = "|:";

        public string WordMatchToString()
        {
            if (WordMatch == WordMatchKind.AllWords)
                return "AllWords";
            else if (WordMatch == WordMatchKind.AnyWord)
                return "AnyWord";
            else if (WordMatch == WordMatchKind.ExactPhrase)
                return "ExactPhrase";
            else
            {
                Debug.Assert(false);
                return "AllWords";
            }
        }
        
        public override string ToString()
        {
            return sep
                + ResultsId.ToString() + sep
                + SearchWords + sep
                + TopFolderToken + sep
                + TopFolderPath + sep
                + TopFolderName + sep
                + FileNamePatterns + sep
                + CaseSensitive.ToString() + sep
                + WholeWords.ToString() + sep
                + UseRegex.ToString() + sep
                + WordMatchToString() + sep
                + AllDepths.ToString() + sep
                + MaxDepth.ToString() + sep
                + AllFoldersCount.ToString() + sep
                + AllFilesCount.ToString() + sep
                + Incomplete.ToString();
        }

        public int CompareTo(SearchOptions other)
        {
            if (other == null)
                return 1;
            else if (this.Equals(other))
                return 0;
            else
            {
                return (!string.IsNullOrEmpty(this.TopFolderPath) || !string.IsNullOrEmpty(other.TopFolderPath)) ?
                    string.Compare(this.TopFolderPath, other.TopFolderPath, StringComparison.OrdinalIgnoreCase) :
                    string.Compare(this.TopFolderName, other.TopFolderName, StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool FullEquals(SearchOptions other)
        {
            if (other == null)
                return false;
            else
            {
                return (!string.IsNullOrEmpty(this.TopFolderPath) || !string.IsNullOrEmpty(other.TopFolderPath)) ?
                    (string.Compare(this.TopFolderPath, other.TopFolderPath, StringComparison.OrdinalIgnoreCase) == 0 && this.Equals(other)) :
                    (string.Compare(this.TopFolderName, other.TopFolderName, StringComparison.OrdinalIgnoreCase) == 0 && this.Equals(other));
            }
        }

        public bool Equals(SearchOptions other)
        {
            if (other == null)
                return false;

            StringComparison stringComp = this.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            return (string.Compare(this.SearchWords, other.SearchWords, stringComp) == 0 &&
                    string.Compare(this.FileNamePatterns, other.FileNamePatterns, StringComparison.OrdinalIgnoreCase) == 0 && // file names are case insensitive on Windows
                    this.AdvancedEquals(other));
        }

        public bool AdvancedEquals(SearchOptions other)
        {
            if (other == null)
                return false;

            bool isDepthEqual = true;
            if (this.AllDepths != other.AllDepths || (!this.AllDepths && this.MaxDepth != other.MaxDepth))
                isDepthEqual = false;

            return (this.CaseSensitive == other.CaseSensitive &&
                    this.WholeWords == other.WholeWords &&
                    this.UseRegex == other.UseRegex &&
                    this.WordMatch == other.WordMatch &&
                    isDepthEqual);
        }

        //public static bool operator ==(SearchOptions left, SearchOptions right)
        //{
        //    if ((object)left == null && (object)right == null)
        //        return true;
        //    else if ((object)left != null && (object)right != null)
        //        return left.Equals(right);
        //    else
        //        return false;
        //}

        //public static bool operator !=(SearchOptions left, SearchOptions right)
        //{
        //    return !(left == right);
        //}
    }
}
