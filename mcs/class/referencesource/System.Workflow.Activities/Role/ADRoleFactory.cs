#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.Security.Principal;
using System.Configuration;
using System.Workflow.Runtime.Configuration;
using System.Workflow.Activities.Configuration;

#endregion

namespace System.Workflow.Activities
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public static class ActiveDirectoryRoleFactory
    {
        private static DirectoryGroupQuery s_directoryGroupQuery = new DirectoryGroupQuery();
        private static String s_configurationSectionName = "System.Workflow.Runtime.Hosting.ADRoleFactory";
        private static ActiveDirectoryRoleFactoryConfiguration s_configuration;
        private static DirectoryEntry s_rootEntry;

        static ActiveDirectoryRoleFactory()
        {
            s_configuration = (ActiveDirectoryRoleFactoryConfiguration)ConfigurationManager.GetSection(s_configurationSectionName);
            if (s_configuration == null)
                s_configuration = new ActiveDirectoryRoleFactoryConfiguration();
        }


        public static ActiveDirectoryRole CreateFromAlias(String alias)
        {
            if (alias == null)
                throw new ArgumentNullException("alias");

            ActiveDirectoryRole role = new ActiveDirectoryRole(GetRootEntry(), new DirectoryRootQuery("sAMAccountName", alias, DirectoryQueryOperation.Equal));
            role.Operations.Add(s_directoryGroupQuery);
            ValidateRole(role);
            return role;
        }

        public static ActiveDirectoryRole CreateFromSecurityIdentifier(SecurityIdentifier sid)
        {
            if (sid == null)
                throw new ArgumentNullException("sid");

            ActiveDirectoryRole role = new ActiveDirectoryRole(GetRootEntry(), new DirectoryRootQuery("objectSID", sid.ToString(), DirectoryQueryOperation.Equal));
            role.Operations.Add(s_directoryGroupQuery);
            ValidateRole(role);
            return role;
        }

        public static ActiveDirectoryRole CreateFromEmailAddress(String emailAddress)
        {
            if (emailAddress == null)
                throw new ArgumentNullException("emailAddress");

            ActiveDirectoryRole role = new ActiveDirectoryRole(GetRootEntry(), new DirectoryRootQuery("mail", emailAddress, DirectoryQueryOperation.Equal));
            role.Operations.Add(s_directoryGroupQuery);
            ValidateRole(role);
            return role;
        }

        private static DirectoryEntry GetRootEntry()
        {
            if (s_rootEntry == null)
            {
                if (s_configuration == null ||
                    s_configuration.RootPath == null ||
                    s_configuration.RootPath.Length == 0)
                {
                    s_rootEntry = new DirectoryEntry();
                }
                else
                {
                    s_rootEntry = new DirectoryEntry(s_configuration.RootPath);
                }
            }

            return s_rootEntry;
        }

        public static ActiveDirectoryRoleFactoryConfiguration Configuration
        {
            get
            {
                return s_configuration;
            }
        }

        private static void ValidateRole(ActiveDirectoryRole adRole)
        {
            if (adRole.GetEntries().Count == 0)
                throw new ArgumentException(SR.GetString(SR.Error_NoMatchingActiveDirectoryEntry));
        }

    }
}
