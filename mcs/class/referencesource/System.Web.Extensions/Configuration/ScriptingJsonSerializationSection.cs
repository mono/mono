//------------------------------------------------------------------------------
// <copyright file="ScriptingJsonSerializationSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Script.Serialization;

    public sealed class ScriptingJsonSerializationSection : ConfigurationSection {
        private static readonly ConfigurationProperty _propConverters =
            new ConfigurationProperty("converters",
                                    typeof(ConvertersCollection),
                                    null,
                                    ConfigurationPropertyOptions.IsDefaultCollection);


        private static readonly ConfigurationProperty _propRecursionLimitLimit =
            new ConfigurationProperty("recursionLimit",
                                        typeof(int),
                                        100,
                                        null,
                                        new IntegerValidator(1, int.MaxValue),
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propMaxJsonLength =
            new ConfigurationProperty("maxJsonLength",
                                        typeof(int),
                                        102400,
                                        null,
                                        new IntegerValidator(1, int.MaxValue),
                                        ConfigurationPropertyOptions.None);

        private static ConfigurationPropertyCollection _properties = BuildProperties();

        private static ConfigurationPropertyCollection BuildProperties() {
            ConfigurationPropertyCollection props = new ConfigurationPropertyCollection();
            props.Add(_propConverters);
            props.Add(_propRecursionLimitLimit);
            props.Add(_propMaxJsonLength);
            return props;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("converters", IsKey = true, DefaultValue = "")]
        public ConvertersCollection Converters {
            get {
                return (ConvertersCollection)base[_propConverters];
            }
        }

        [ConfigurationProperty("recursionLimit", DefaultValue = 100)]
        public int RecursionLimit {
            get {
                return (int)base[_propRecursionLimitLimit];
            }
            set {
                base[_propRecursionLimitLimit] = value;
            }
        }

        [ConfigurationProperty("maxJsonLength", DefaultValue = 102400)]
        public int MaxJsonLength {
            get {
                return (int)base[_propMaxJsonLength];
            }
            set {
                base[_propMaxJsonLength] = value;
            }
        }

        internal class ApplicationSettings {
            private int _recusionLimit;
            private int _maxJsonLimit;
            private JavaScriptConverter[] _converters;

            internal ApplicationSettings() {
#pragma warning disable 0436
                ScriptingJsonSerializationSection section = (ScriptingJsonSerializationSection)
                    WebConfigurationManager.GetSection("system.web.extensions/scripting/webServices/jsonSerialization");
#pragma warning restore 0436

                if (section != null) {
                    _recusionLimit = section.RecursionLimit;
                    _maxJsonLimit = section.MaxJsonLength;
                    _converters = section.Converters.CreateConverters();
                }
                else {
                    _recusionLimit = (int)_propRecursionLimitLimit.DefaultValue;
                    _maxJsonLimit = (int)_propMaxJsonLength.DefaultValue;
                    _converters = new JavaScriptConverter[0];
                }
            }

            internal int RecursionLimit {
                get {
                    return _recusionLimit;
                }
            }

            internal int MaxJsonLimit {
                get {
                    return _maxJsonLimit;
                }
            }

            internal JavaScriptConverter[] Converters {
                get {
                    return _converters;
                }
            }

        }

    }
}
