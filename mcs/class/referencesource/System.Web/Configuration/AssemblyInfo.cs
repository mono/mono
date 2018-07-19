//------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Microsoft">
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

    public sealed class AssemblyInfo : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propAssembly =
            new ConfigurationProperty("assembly",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsAssemblyStringTransformationRequired);

        private Assembly[] _assembly;
        private CompilationSection _compilationSection;

        internal void SetCompilationReference(CompilationSection compSection) {
            _compilationSection = compSection;
        }

        static AssemblyInfo() {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propAssembly);
        }


        internal AssemblyInfo() {
        }

        public AssemblyInfo(string assemblyName) {
            Assembly = assemblyName;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("assembly", IsRequired = true, IsKey = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string Assembly {
            get {
                return (string)base[_propAssembly];
            }
            set {
                base[_propAssembly] = value;
            }
        }

        internal Assembly[] AssemblyInternal {
            get {
                Debug.Trace("AssemblyInternal", "Loading assembly: " + Assembly);
                if (_assembly == null) {
                    Debug.Assert(_compilationSection != null);
                    _assembly = _compilationSection.LoadAssembly(this);
                }
                return _assembly;
            }
            set {
                Debug.Trace("AssemblyInternal", "Set assembly: " + Assembly);
                _assembly = value;
            }
        }
    }
}
