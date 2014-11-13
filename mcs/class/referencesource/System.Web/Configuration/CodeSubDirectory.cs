//------------------------------------------------------------------------------
// <copyright file="CodeSubDirectory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Compilation;
    using System.Reflection;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.CodeDom.Compiler;
    using System.Web.Util;
    using System.ComponentModel;
    using System.Security.Permissions;

    public sealed class CodeSubDirectory : ConfigurationElement {
        private const string dirNameAttribName = "directoryName";

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propDirectoryName =
            new ConfigurationProperty(dirNameAttribName,
                                        typeof(string),
                                        null,
                                        StdValidatorsAndConverters.WhiteSpaceTrimStringConverter,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

        static CodeSubDirectory() {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propDirectoryName);
        }


        internal CodeSubDirectory() {
        }

        public CodeSubDirectory(string directoryName) {
            DirectoryName = directoryName;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty(dirNameAttribName, IsRequired = true, IsKey = true, DefaultValue = "")]
        [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        public string DirectoryName {
            get {
                return (string)base[_propDirectoryName];
            }
            set {
                base[_propDirectoryName] = value;
            }
        }

        // The assembly is named after the directory
        internal string AssemblyName { get { return DirectoryName; } }

        // Validate the element for runtime use
        internal void DoRuntimeValidation() {
            string directoryName = DirectoryName;

            // If the app is precompiled, don't attempt further validation, sine the directory
            // will not actually exist (VSWhidbey 394333)
            if (BuildManager.IsPrecompiledApp) {
                return;
            }

            // Make sure it's just a valid simple directory name
            if (!Util.IsValidFileName(directoryName)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_CodeSubDirectory, directoryName),
                    ElementInformation.Properties[dirNameAttribName].Source, ElementInformation.Properties[dirNameAttribName].LineNumber);
            }

            VirtualPath codeVirtualSubDir = HttpRuntime.CodeDirectoryVirtualPath.SimpleCombineWithDir(directoryName);

            // Make sure the specified directory exists
            if (!VirtualPathProvider.DirectoryExistsNoThrow(codeVirtualSubDir)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_CodeSubDirectory_Not_Exist, codeVirtualSubDir),
                    ElementInformation.Properties[dirNameAttribName].Source, ElementInformation.Properties[dirNameAttribName].LineNumber);
            }

            // Look at the actual physical dir to get its name canonicalized (VSWhidbey 288568)
            string physicalDir = codeVirtualSubDir.MapPathInternal();
            FindFileData ffd;
            FindFileData.FindFile(physicalDir, out ffd);

            // If the name was not canonical, reject it
            if (!StringUtil.EqualsIgnoreCase(directoryName, ffd.FileNameLong)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_CodeSubDirectory, directoryName),
                    ElementInformation.Properties[dirNameAttribName].Source, ElementInformation.Properties[dirNameAttribName].LineNumber);
            }

            if (BuildManager.IsReservedAssemblyName(directoryName)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Reserved_AssemblyName, directoryName),
                    ElementInformation.Properties[dirNameAttribName].Source, ElementInformation.Properties[dirNameAttribName].LineNumber);
            }
        }
    }
}
