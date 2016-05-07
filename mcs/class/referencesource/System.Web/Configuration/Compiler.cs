//------------------------------------------------------------------------------
// <copyright file="Compiler.cs" company="Microsoft">
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

    // CompilerCollection
    public sealed class Compiler : ConfigurationElement {
        private const string compilerOptionsAttribName = "compilerOptions";

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propLanguage =
            new ConfigurationProperty("language", typeof(string), String.Empty, ConfigurationPropertyOptions.None );
        private static readonly ConfigurationProperty _propExtension =
            new ConfigurationProperty("extension", typeof(string), String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propType =
            new ConfigurationProperty("type", typeof(string), String.Empty, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsTypeStringTransformationRequired);
        private static readonly ConfigurationProperty _propWarningLevel =
            new ConfigurationProperty("warningLevel",
                                        typeof(int),
                                        0,
                                        null,
                                        new IntegerValidator(0, 4),
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCompilerOptions =
            new ConfigurationProperty(compilerOptionsAttribName, typeof(string), String.Empty, ConfigurationPropertyOptions.None);

        private CompilerType _compilerType;

        static Compiler() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propLanguage);
            _properties.Add(_propExtension);
            _properties.Add(_propType);
            _properties.Add(_propWarningLevel);
            _properties.Add(_propCompilerOptions);
        }

        internal Compiler() {
        }

        public Compiler(String compilerOptions, String extension, String language, String type, int warningLevel)
            : this() {
            base[_propCompilerOptions] = compilerOptions;
            base[_propExtension] = extension;
            base[_propLanguage] = language;
            base[_propType] = type;
            base[_propWarningLevel] = warningLevel;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("language", DefaultValue = "", IsRequired = true, IsKey = true)]
        public string Language {
            get {
                return (string)base[_propLanguage];
            }
            // Remove to satisfy defect number 178343
            //set
            //{
            //    base[_propLanguage] = value;
            //}
        }

        [ConfigurationProperty("extension", DefaultValue = "")]
        public string Extension {
            get {
                return (string)base[_propExtension];
            }
            // Remove to satisfy defect number 178343
            //set
            //{
            //    base[_propExtension] = value;
            //}
        }

        [ConfigurationProperty("type", IsRequired = true, DefaultValue = "")]
        public string Type {
            get {
                return (string)base[_propType];
            }
            // Remove to satisfy defect number 178343
            //set
            //{
            //    base[_propType] = value;
            //}
        }

        internal CompilerType CompilerTypeInternal {
            get {
                if (_compilerType == null) {
                    lock (this) {
                        if (_compilerType == null) {
                            Type codeDomProviderType = CompilationUtil.LoadTypeWithChecks(
                            Type, typeof(CodeDomProvider), null, this, "type");

                            System.CodeDom.Compiler.CompilerParameters compilParams = new CompilerParameters();
                            compilParams.WarningLevel = WarningLevel;

                            // Need to be false if the warning level is 0
                            compilParams.TreatWarningsAsErrors = (WarningLevel > 0);

                            // Only allow the use of compilerOptions when we have UnmanagedCode access (ASURT 73678)
                            string compilerOptions = CompilerOptions;

                            // Only allow the use of compilerOptions when we have UnmanagedCode access (ASURT 73678)
                            CompilationUtil.CheckCompilerOptionsAllowed(compilerOptions, true /*config*/,
                                ElementInformation.Properties[compilerOptionsAttribName].Source,
                                ElementInformation.Properties[compilerOptionsAttribName].LineNumber);

                            compilParams.CompilerOptions = compilerOptions;

                            _compilerType = new CompilerType(codeDomProviderType, compilParams);
                        }
                    }
                }

                return _compilerType;
            }
        }

        [ConfigurationProperty("warningLevel", DefaultValue = 0)]
        [IntegerValidator(MinValue = 0, MaxValue = 4)]
        public int WarningLevel {
            get {
                return (int)base[_propWarningLevel];
            }
            // Remove to satisfy defect number 178343
            //set
            //{
            //    base[_propWarningLevel] = value;
            //}
        }

        [ConfigurationProperty(compilerOptionsAttribName, DefaultValue = "")]
        public string CompilerOptions {
            get {
                return (string)base[_propCompilerOptions];
            }
            // Remove to satisfy defect number 178343
            //set
            //{
            //    base[_propCompilerOptions] = value;
            //}
        }

    } // class Compiler
}
