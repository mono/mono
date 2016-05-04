//------------------------------------------------------------------------------
// <copyright file="UriSection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Configuration
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.IO;
    
    /// <summary>
    /// Summary description for UriSection.
    /// </summary>
    public sealed class UriSection : ConfigurationSection
    {
        private static readonly ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        private static readonly ConfigurationProperty idn = new ConfigurationProperty(CommonConfigurationStrings.Idn, 
            typeof(IdnElement), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty iriParsing = new ConfigurationProperty(
            CommonConfigurationStrings.IriParsing, typeof(IriParsingElement), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty schemeSettings =
            new ConfigurationProperty(CommonConfigurationStrings.SchemeSettings,
            typeof(SchemeSettingElementCollection), null, ConfigurationPropertyOptions.None);

        static UriSection()
        {
            properties.Add(idn);
            properties.Add(iriParsing);
            properties.Add(schemeSettings);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Idn", Justification = "changing this would be a breaking change because the API has been present since v3.5")]
        [ConfigurationProperty(CommonConfigurationStrings.Idn)]
        public IdnElement Idn{
            get {
                return (IdnElement)this[idn]; 
            }
        }

        [ConfigurationProperty(CommonConfigurationStrings.IriParsing)]
        public IriParsingElement IriParsing
        {
            get {
                return (IriParsingElement)this[iriParsing];
            }
        }

        [ConfigurationProperty(CommonConfigurationStrings.SchemeSettings)]
        public SchemeSettingElementCollection SchemeSettings
        {
            get {
                return (SchemeSettingElementCollection)this[schemeSettings];
            }
        }
        
        protected override ConfigurationPropertyCollection Properties 
        {
            get {
                return properties;
            }
        }
    }

    internal sealed class UriSectionInternal
    {
        private static readonly object classSyncObject = new object();
        private UriIdnScope idnScope;
        private bool iriParsing;
        private Dictionary<string, SchemeSettingInternal> schemeSettings;

        private UriSectionInternal()
        {
            this.schemeSettings = new Dictionary<string, SchemeSettingInternal>();
        }

        private UriSectionInternal(UriSection section)
            : this()
        {
            this.idnScope = section.Idn.Enabled;
            this.iriParsing = section.IriParsing.Enabled;

            if (section.SchemeSettings != null) {
                SchemeSettingInternal schemeSetting;
                foreach (SchemeSettingElement element in section.SchemeSettings)
                {
                    schemeSetting = new SchemeSettingInternal(element.Name, element.GenericUriParserOptions);
                    this.schemeSettings.Add(schemeSetting.Name, schemeSetting);
    	        }
            }
        }

        private UriSectionInternal(UriIdnScope idnScope, bool iriParsing,
            IEnumerable<SchemeSettingInternal> schemeSettings)
            : this()
        {
            this.idnScope = idnScope;
            this.iriParsing = iriParsing;

            if (schemeSettings != null) {
                foreach (SchemeSettingInternal schemeSetting in schemeSettings) {
                    this.schemeSettings.Add(schemeSetting.Name, schemeSetting);
                }
            }
        }

        internal UriIdnScope IdnScope
        {
            get { return idnScope; }
        }

        internal bool IriParsing
        {
            get { return iriParsing; }
        }

        internal SchemeSettingInternal GetSchemeSetting(string scheme)
        {
            SchemeSettingInternal result;
            if (schemeSettings.TryGetValue(scheme.ToLowerInvariant(), out result)) {
                return result;
            }
            else {
                return null;
            }
        }

        // This method originally just used System.Configuration to get the new-to-Orcas Uri config section.  
        // Unfortunately that created a circular dependency on System.Config that ultimately could cause
        // ConfigurationExceptions that didn't used to happen in Whidbey and thus break existing deployed 
        // applications.
        //
        // Now this method will determine if it is running in a client application or a web scenario (ASP.NET).   
        // If in a web scenario this code must still use System.Configuration to read config as the web scenario 
        // has a hierarchy of config files that only System.Configuration can practically discover and parse.  
        // In a client scenario this code will now use System.Xml.XmlReader to parse machine and app config files.
        //
        // The default output of this method if it encounters invalid or non-existent Uri config is the original 
        // Whidbey settings (no IDN support, no IRI support, and escaped dots and slashes are unescaped).
        //
        [SuppressMessage("Microsoft.Security","CA2106:SecureAsserts", Justification="Must Assert unrestricted FileIOPermission to get the app config path")]
        internal static UriSectionInternal GetSection()
        {
            lock (classSyncObject) {

                string appConfigFilePath = null;

                // Must Assert unrestricted FileIOPermission to get the app config path.
                new FileIOPermission(PermissionState.Unrestricted).Assert();
                try {
                    appConfigFilePath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                }
                finally {
                    FileIOPermission.RevertAssert();
                }

                if (IsWebConfig(appConfigFilePath)) {
                    // This is a web scenario. It is safe and *necessary* to use System.Configuration.
                    return LoadUsingSystemConfiguration();
                }
                else {
                    // This is a client application scenario. It is not safe to use System.Config for app 
                    // compat reasons. 
                    return LoadUsingCustomParser(appConfigFilePath);
                }
            }
        }

        private static UriSectionInternal LoadUsingSystemConfiguration()
        {
            try {
                UriSection section = PrivilegedConfigurationManager.GetSection(
                    CommonConfigurationStrings.UriSectionName) as UriSection;

                if (section == null) {
                    return null;
                }

                return new UriSectionInternal(section);
            }
            catch (ConfigurationException) {
                // Simply ---- any ConfigurationException.
                // Throwing it would potentially break applications.
                // Uri did not read config in previous releases.
                return null;
            }
        }

        [SuppressMessage("Microsoft.Security","CA2106:SecureAsserts", Justification="Must Assert unrestricted FileIOPermission to get the machine config path")]
        private static UriSectionInternal LoadUsingCustomParser(string appConfigFilePath)
        {
            // Already have the application config file path in scope.
            // Get the path of the machine config file.
            string runtimeDir = null;

            // Must Assert unrestricted FileIOPermission to get the machine config path.
            new FileIOPermission(PermissionState.Unrestricted).Assert();
            try {
                runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
            }
            finally {
                FileIOPermission.RevertAssert();
            }
            string machineConfigFilePath = Path.Combine(Path.Combine(runtimeDir, "Config"), "machine.config");

            UriSectionData machineSettings = UriSectionReader.Read(machineConfigFilePath);
            // pass machineSettings to ctor: appSettings will use the values of machineSettings as init values.
            UriSectionData appSettings = UriSectionReader.Read(appConfigFilePath, machineSettings);

            UriSectionData resultSectionData = null;
            if (appSettings != null) {
                resultSectionData = appSettings;
            }
            else if (machineSettings != null) {
                resultSectionData = machineSettings;
            }

            if (resultSectionData != null) {
                UriIdnScope idnScope = resultSectionData.IdnScope ?? IdnElement.EnabledDefaultValue;
                bool iriParsing = resultSectionData.IriParsing ?? IriParsingElement.EnabledDefaultValue;
                IEnumerable<SchemeSettingInternal> schemeSettings = 
                    resultSectionData.SchemeSettings.Values as IEnumerable<SchemeSettingInternal>;
    
                return new UriSectionInternal(idnScope, iriParsing, schemeSettings);
            }

            return null;
        }

        private static bool IsWebConfig(string appConfigFile)
        {
            // Determine if we are in a Web config scenario.

            // Existence of string object associated with .appVPath tells
            // us that this is an ASP.Net web scenario.
            string appVPath = AppDomain.CurrentDomain.GetData(".appVPath") as string;
            if (appVPath != null) {
                return true;
            }

            // If application config file path not null
            // and begins with http:// or https://
            // then this is the No-Touch web deployment scenario.
            if (appConfigFile != null) {
                if (appConfigFile.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    appConfigFile.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {

                    return true;
                }
            }
            return false;
        }
    }
}

