#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.DirectoryServices;

#endregion

namespace System.Workflow.Activities
{
    [Serializable]
    sealed internal class DirectoryGroupQuery : IDirectoryOperation
    {
        public DirectoryGroupQuery()
        {
        }

        public void GetResult(DirectoryEntry rootEntry, DirectoryEntry currentEntry, List<DirectoryEntry> response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            Stack<DirectoryEntry> entries = new Stack<DirectoryEntry>();
            entries.Push(currentEntry);

            while (entries.Count != 0)
            {
                DirectoryEntry entry = entries.Pop();

                bool isGroup = false;
                if (Contains(entry.Properties.PropertyNames, "objectClass"))
                {
                    foreach (String value in entry.Properties["objectClass"])
                    {
                        if (String.Compare(value, ActiveDirectoryRoleFactory.Configuration.Group, StringComparison.Ordinal) == 0)
                        {
                            isGroup = true;
                            break;
                        }
                    }

                    if (isGroup)
                    {
                        if (Contains(entry.Properties.PropertyNames, ActiveDirectoryRoleFactory.Configuration.Member))
                        {
                            foreach (String propValue in entry.Properties[ActiveDirectoryRoleFactory.Configuration.Member])
                            {
                                entries.Push(new DirectoryEntry(BuildUri(propValue)));
                            }
                        }
                    }
                    else
                    {
                        response.Add(entry);
                    }
                }
            }
        }

        private static bool Contains(ICollection propertyNames, String testPropertyName)
        {
            foreach (String propertyName in propertyNames)
            {
                if (String.Compare(propertyName, testPropertyName, StringComparison.Ordinal) == 0)
                    return true;
            }
            return false;
        }

        private static String BuildUri(String propValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("LDAP://");
            for (int i = 0; i < propValue.Length; ++i)
            {
                if (propValue[i] == '/')
                    sb.Append("\\/");
                else
                    sb.Append(propValue[i]);
            }
            return sb.ToString();
        }
    }

}
