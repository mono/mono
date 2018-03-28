#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

#endregion

namespace System.Workflow.Activities
{
    [Serializable]
    sealed internal class DirectoryRedirect : IDirectoryOperation
    {
        private String m_getPropertyName;
        private String m_searchPropertyName;
        private bool m_recursive;

        public DirectoryRedirect(String getPropertyName, String searchPropertyName)
            : this(getPropertyName, searchPropertyName, false)
        {
        }

        public DirectoryRedirect(String getPropertyName, String searchPropertyName, bool recursive)
        {
            if (getPropertyName == null)
                throw new ArgumentNullException("getPropertyName");
            if (searchPropertyName == null)
                throw new ArgumentNullException("searchPropertyName");

            this.m_getPropertyName = getPropertyName;
            this.m_searchPropertyName = searchPropertyName;
            this.m_recursive = recursive;
        }

        public void GetResult(DirectoryEntry rootEntry, DirectoryEntry currentEntry, List<DirectoryEntry> response)
        {
            if (rootEntry == null)
                throw new ArgumentNullException("rootEntry");
            if (currentEntry == null)
                throw new ArgumentNullException("currentEntry");
            if (response == null)
                throw new ArgumentNullException("response");

            if (!this.m_recursive)
            {
                using (DirectorySearcher searcher = CreateSearcher(rootEntry, currentEntry))
                {
                    foreach (SearchResult result in searcher.FindAll())
                    {
                        response.Add(result.GetDirectoryEntry());
                    }
                }
            }
            else
            {
                Dictionary<Guid, DirectoryEntry> dResponse = new Dictionary<Guid, DirectoryEntry>();
                Stack<DirectoryEntry> stack = new Stack<DirectoryEntry>();
                stack.Push(currentEntry);

                while (stack.Count != 0)
                {
                    DirectoryEntry currentTop = stack.Pop();
                    using (DirectorySearcher searcher = CreateSearcher(rootEntry, currentTop))
                    {
                        foreach (SearchResult result in searcher.FindAll())
                        {
                            DirectoryEntry newEntry = result.GetDirectoryEntry();
                            if (!dResponse.ContainsKey(newEntry.Guid))
                                dResponse.Add(newEntry.Guid, newEntry);
                            stack.Push(newEntry);
                        }
                    }
                }

                response.AddRange(dResponse.Values);
            }
        }

        private DirectorySearcher CreateSearcher(DirectoryEntry rootEntry, DirectoryEntry currentEntry)
        {
            DirectorySearcher searcher = new DirectorySearcher(rootEntry);

            PropertyValueCollection values = currentEntry.Properties[this.m_getPropertyName];
            System.Diagnostics.Debug.Assert(values.Count == 1);

            searcher.Filter = "(" + this.m_searchPropertyName + "=" + values[0] + ")";

            return searcher;
        }
    }
}
