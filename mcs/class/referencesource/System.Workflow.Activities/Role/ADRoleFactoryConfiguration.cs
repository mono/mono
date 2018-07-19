#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.Configuration;

#endregion

namespace System.Workflow.Activities.Configuration
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    sealed public class ActiveDirectoryRoleFactoryConfiguration : ConfigurationSection
    {
        private const String _RootPath = "RootPath";
        private const String _Manager = "Manager";
        private const String _DistinguishedName = "DistiguishedName";
        private const String _DirectReports = "DirectReports";
        private const String _Group = "Group";
        private const String _Member = "Member";

        [ConfigurationProperty(_RootPath, DefaultValue = "")]
        public string RootPath
        {
            get { return (string)base[_RootPath]; }
            set { base[_RootPath] = value; }
        }

        [ConfigurationProperty(_Manager, DefaultValue = "manager")]
        public String Manager
        {
            get { return (string)base[_Manager]; }
            set { base[_Manager] = value; }
        }

        [ConfigurationProperty(_DistinguishedName, DefaultValue = "distinguishedName")]
        public String DistinguishedName
        {
            get { return (string)base[_DistinguishedName]; }
            set { base[_DistinguishedName] = value; }
        }

        [ConfigurationProperty(_DirectReports, DefaultValue = "directReports")]
        public String DirectReports
        {
            get { return (string)base[_DirectReports]; }
            set { base[_DirectReports] = value; }
        }

        [ConfigurationProperty(_Group, DefaultValue = "group")]
        public String Group
        {
            get { return (string)base[_Group]; }
            set { base[_Group] = value; }
        }

        [ConfigurationProperty(_Member, DefaultValue = "member")]
        public String Member
        {
            get { return (string)base[_Member]; }
            set { base[_Member] = value; }
        }
    }
}
