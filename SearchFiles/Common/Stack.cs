using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SearchFiles.Common
{
    public interface IStackMgr
    {
        void Push(SearchOptions options);
        SearchOptions Pop();
        SearchOptions Peek();
        int Count { get;  }
        void LoadStack();
        void Clear();
        void ChangeTab(string newTabid);
    }

    public class StackMgr : IStackMgr
    {
        Stack<SearchOptions> _Stack = null;
        protected const string FILE_PREFIX = "eCodified-FileSearch-";
        protected const string FILE_EXT = ".txt";
        protected string STACK_FILE_NAME = "";
        protected string m_stackName = "";
        protected string m_tabid = "";

        public StackMgr(string stackName, string tabid)
        {
            try
            {
                _Stack = new Stack<SearchOptions>(50);
                m_stackName = stackName;
                m_tabid = tabid;
                STACK_FILE_NAME = FILE_PREFIX + m_stackName + "-" + m_tabid + FILE_EXT;
            }
            catch (Exception ex) { Debug.WriteLine("\nStackMgr ctor: " + ex.ToString()); }
        }

        public void ChangeTab(string newTabid)
        {
            try
            {
                SaveStack();
                _Stack.Clear(); // in-memory only

                m_tabid = newTabid;

                STACK_FILE_NAME = FILE_PREFIX + m_stackName + "-" + m_tabid + FILE_EXT;
                LoadStack();
            }
            catch (Exception ex) { Debug.WriteLine("\nStackMgr.ChangeTab " + ex.ToString()); }
        }

        public void Push(SearchOptions options)
        {
            try {
                 if (options == null)
                    return;

                _Stack.Push(options);
                SaveStack();
                Debug.WriteLine(STACK_FILE_NAME + ": Push: " + options.ToString());
            }
            catch (Exception ex) { Debug.WriteLine("\nStackMgr.Push " + ex.ToString()); }
            finally { /*Debug.WriteLine("Stack.Push finally block");*/ }
        }

        public SearchOptions Pop()
        {
            try {
                if (_Stack.Count <= 0) {
                    Debug.Assert(false);
                    return null;
                }

                SearchOptions options = _Stack.Pop();
                SaveStack();
                Debug.WriteLine(STACK_FILE_NAME + ": Pop:  " + options.ToString());
                return options;
            }
            catch (Exception ex) { Debug.WriteLine("\nStackMgr.Pop " + ex.ToString()); }
            return new SearchOptions();
        }

        public SearchOptions Peek()
        {
            try {
                if (_Stack.Count <= 0)
                    return null;

                return _Stack.Peek();
            }
            catch (Exception ex) { Debug.WriteLine("\nStackMgr.Peek " + ex.ToString()); }
            return new SearchOptions();
        }

        public int Count
        {
            get { return _Stack.Count; }
        }

        public void LoadStack() // TODO IMPL
        {
            //try
            //{
            //    _Stack.Clear();
            //
            //    using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            //    {
            //        if (!isf.FileExists(STACK_FILE_NAME))
            //        {
            //            using (var fs = new IsolatedStorageFileStream(STACK_FILE_NAME, FileMode.Create, isf))
            //            {
            //            }
            //            if (!isf.FileExists(STACK_FILE_NAME))
            //                Debug.WriteLine("\n" + MainPage.DBG_HD + "ERROR: StackMgr.LoadStack did not create the stack file! Stack file does NOT exist: " + STACK_FILE_NAME);
            //            else
            //                Debug.WriteLine("\n" + MainPage.DBG_HD + "OK: StackMgr.LoadStack did create the stack file: " + STACK_FILE_NAME);
            //            return;
            //        }
            //
            //        // load the stack
            //        using (var fs = new IsolatedStorageFileStream(STACK_FILE_NAME, FileMode.Open, isf))
            //        {
            //            StreamReader reader = new StreamReader(fs);
            //            string url = null;
            //            while ((url = reader.ReadLine()) != null)
            //            {
            //                if (url.Trim().Length > 0)
            //                    _Stack.Push(url.Trim()); // this reverses the order of entries in the file (see also SaveStack)
            //            }
            //            reader.Close();
            //        };
            //    }
            //}
            //catch (Exception ex) { Debug.WriteLine("\nStackMgr.LoadStack" + ex.ToString()); }
            //finally { /*Debug.WriteLine("StackMgr.LoadStack finally block");*/ }
        }

        public void Clear()
        {
            try {
                _Stack.Clear();
                SaveStack();
            }
            catch (Exception ex) { Debug.WriteLine("\nStackMgr.Clear " + ex.ToString()); }
            finally { /*Debug.WriteLine("StackMgr.Clear finally block");*/ }
        }

        protected const int MAX_TO_SAVE = 20;

        protected void SaveStack() // TODO IMPL
        {
            //try
            //{
            //    // reverse the stack order (see also LoadStack) and only save up to MAX_TO_SAVE elements
            //    var array = _Stack.ToArray();
            //    var revStack = new Stack<string>(MAX_TO_SAVE);
            //    for (int i = 0; i < array.Length && i < MAX_TO_SAVE; ++i)
            //    {
            //        if (array[i] != null)
            //            revStack.Push(array[i].Trim());
            //    }
            //
            //    using (var isf = IsolatedStorageFile.GetUserStoreForApplication()) // gets a reference to the Local Folder
            //    {
            //        // save the stack
            //        using (var fs = new IsolatedStorageFileStream(STACK_FILE_NAME, FileMode.Create, isf))
            //        {
            //            var writer = new StreamWriter(fs);
            //            if (revStack.Count <= 0) // empty stack should also be saved
            //            {
            //                writer.Write("\n");
            //            }
            //            else
            //            {
            //                while (revStack.Count > 0) // saving non-empty stack
            //                {
            //                    string item = revStack.Pop();
            //                    if (item != null)
            //                        writer.Write(item.Trim() + "\n");
            //                }
            //            }
            //            writer.Close();
            //        };
            //        if (!isf.FileExists(STACK_FILE_NAME))
            //            Debug.WriteLine("\nERROR: StackMgr.SaveStack did not create/update the stack file! Stack file does NOT exist: " + STACK_FILE_NAME);
            //        //else
            //        //    Debug.WriteLine("\nOK: StackMgr.SaveStack did create/update the stack file: " + STACK_FILE_NAME);
            //    }
            //}
            //catch (Exception ex) { Debug.WriteLine("\n" + ex.ToString()); }
            //finally { /*Debug.WriteLine("StackMgr.SaveStack finally block");*/ }
        }
    }
}
