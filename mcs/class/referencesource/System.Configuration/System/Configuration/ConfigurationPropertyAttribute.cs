//------------------------------------------------------------------------------
// <copyright file="ConfigurationPropertyAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration.Internal;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Xml;
using System.Globalization;
using System.ComponentModel;
using System.Security;
using System.Text;

namespace System.Configuration {

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConfigurationPropertyAttribute : Attribute {
// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal static readonly String DefaultCollectionPropertyName = "";
#pragma warning restore 0414

        private String _Name;
        private object _DefaultValue = ConfigurationElement.s_nullPropertyValue;
        private ConfigurationPropertyOptions _Flags = ConfigurationPropertyOptions.None;

        public ConfigurationPropertyAttribute(String name) {
            _Name = name;
        }

        public String Name {
            get { 
                return _Name; 
            }
        }

        public object DefaultValue {
            get { 
                return _DefaultValue; 
            }
            set { 
                _DefaultValue = value; 
            }
        }

        public ConfigurationPropertyOptions Options {
            get { 
                return _Flags; 
            }
            set { 
                _Flags = value; 
            }
        }

        public bool IsDefaultCollection {
            get {
                return ((Options & ConfigurationPropertyOptions.IsDefaultCollection) != 0);
            }
            set {
                if (value == true) {
                    Options |= ConfigurationPropertyOptions.IsDefaultCollection;
                }
                else
                    Options &= ~ConfigurationPropertyOptions.IsDefaultCollection;
            }
        }

        public bool IsRequired {
            get {
                return ((Options & ConfigurationPropertyOptions.IsRequired) != 0);
            }
            set {
                if (value == true) {
                    Options |= ConfigurationPropertyOptions.IsRequired;
                }
                else {
                    Options &= ~ConfigurationPropertyOptions.IsRequired;
                }
            }
        }

        public bool IsKey {
            get {
                return ((Options & ConfigurationPropertyOptions.IsKey) != 0);
            }
            set {
                if (value == true) {
                    Options |= ConfigurationPropertyOptions.IsKey;
                }
                else {
                    Options &= ~ConfigurationPropertyOptions.IsKey;
                }
            }
        }
    }
}
