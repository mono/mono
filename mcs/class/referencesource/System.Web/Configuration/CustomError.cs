//------------------------------------------------------------------------------
// <copyright file="CustomError.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Globalization;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Security.Permissions;

    // class CustomErrorsSection

    public sealed class CustomError : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propStatusCode =
            new ConfigurationProperty("statusCode",
                                        typeof(int),
                                        null,
                                        null,
                                        new IntegerValidator(100, 999),
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propRedirect =
            new ConfigurationProperty("redirect",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired);

        static CustomError() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propStatusCode);
            _properties.Add(_propRedirect);
        }

        internal CustomError() {
        }

        public CustomError(int statusCode, string redirect)
            : this() {
            StatusCode = statusCode;
            Redirect = redirect;
        }

        // I believe these can be removed
        public override bool Equals(object customError) {
            CustomError o = customError as CustomError;

            return (o != null && o.StatusCode == StatusCode && o.Redirect == Redirect);
        }

        public override int GetHashCode() {
            return HashCodeCombiner.CombineHashCodes(StatusCode, Redirect.GetHashCode());
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("statusCode", IsRequired = true, IsKey = true)]
        [IntegerValidator(MinValue = 100, MaxValue = 999)]
        public int StatusCode {
            get {
                return (int)base[_propStatusCode];
            }
            set {
                base[_propStatusCode] = value;
            }
        }

        [ConfigurationProperty("redirect", IsRequired = true)]
        [StringValidator(MinLength = 1)]
        public string Redirect {
            get {
                return (string)base[_propRedirect];
            }
            set {
                base[_propRedirect] = value;
            }
        }
    } // class CustomError
}
