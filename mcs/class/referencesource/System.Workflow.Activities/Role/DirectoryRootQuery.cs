#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

#endregion

namespace System.Workflow.Activities
{
    [Serializable]
    sealed internal class DirectoryRootQuery : IDirectoryOperation
    {
        private String m_name;
        private String m_value;
        private DirectoryQueryOperation m_operation;

        public DirectoryRootQuery(String name, String value, DirectoryQueryOperation operation)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (value == null)
                throw new ArgumentNullException("value");

            this.m_name = name;
            this.m_value = value;
            this.m_operation = operation;
        }

        public void GetResult(DirectoryEntry rootEntry, DirectoryEntry currentEntry, List<DirectoryEntry> response)
        {
            if (rootEntry == null)
                throw new ArgumentNullException("rootEntry");
            if (currentEntry == null)
                throw new ArgumentNullException("currentEntry");
            if (response == null)
                throw new ArgumentNullException("response");

            using (DirectorySearcher searcher = new DirectorySearcher(rootEntry))
            {
                String strStart = "(";
                String strOperation = "";
                String strEnd = ")";

                switch (this.m_operation)
                {
                    case DirectoryQueryOperation.Equal:
                        strOperation = "=";
                        break;

                    case DirectoryQueryOperation.NotEqual:
                        strStart = "(!(";
                        strOperation = "=";
                        strEnd = "))";
                        break;

                    default:
                        System.Diagnostics.Debug.Assert(false);
                        break;
                }

                searcher.Filter = strStart + this.m_name + strOperation + this.m_value + strEnd;

                foreach (SearchResult result in searcher.FindAll())
                {
                    response.Add(result.GetDirectoryEntry());
                }
            }
        }
    }

}
