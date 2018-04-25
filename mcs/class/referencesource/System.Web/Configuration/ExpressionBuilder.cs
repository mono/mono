//------------------------------------------------------------------------------
// <copyright file="ExpressionBuilder.cs" company="Microsoft">
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

    public sealed class ExpressionBuilder : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propExpressionPrefix =
            new ConfigurationProperty("expressionPrefix",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propType =
            new ConfigurationProperty("type",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsTypeStringTransformationRequired);

        static ExpressionBuilder() {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propExpressionPrefix);
            _properties.Add(_propType);
        }

        internal ExpressionBuilder() {
        }

        public ExpressionBuilder(string expressionPrefix, string theType) {
            ExpressionPrefix = expressionPrefix;
            Type = theType;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("expressionPrefix", IsRequired = true, IsKey = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string ExpressionPrefix {
            get {
                return (string)base[_propExpressionPrefix];
            }
            set {
                base[_propExpressionPrefix] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string Type {
            get {
                return (string)base[_propType];
            }
            set {
                base[_propType] = value;
            }
        }

        internal Type TypeInternal {
            get {
                return CompilationUtil.LoadTypeWithChecks(Type,
                    typeof(System.Web.Compilation.ExpressionBuilder), null, this, "type");
            }
        }
    }
}
