//------------------------------------------------------------------------------
// <copyright file="PersonalizationStateQuery.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Web.Util;

    [Serializable]
    public sealed class PersonalizationStateQuery {
        private static readonly Dictionary<String, Type> _knownPropertyTypeMappings;

        private HybridDictionary _data;

        static PersonalizationStateQuery() {
            _knownPropertyTypeMappings = new Dictionary<String, Type>(StringComparer.OrdinalIgnoreCase);
            _knownPropertyTypeMappings["PathToMatch"] = typeof(string);
            _knownPropertyTypeMappings["UserInactiveSinceDate"] = typeof(DateTime);
            _knownPropertyTypeMappings["UsernameToMatch"] = typeof(string);
        }

        public PersonalizationStateQuery() {
            _data = new HybridDictionary(true);

            // VSWhidbey 357097: UserInactiveSinceDate needs to have a default value returned for the indexer property
            _data["UserInactiveSinceDate"] = PersonalizationAdministration.DefaultInactiveSinceDate;
        }

        public string PathToMatch {
            get {
                return (string) this["PathToMatch"];
            }
            set {
                if (value != null) {
                    value = value.Trim();
                }
                _data["PathToMatch"] = value;
            }
        }

        public DateTime UserInactiveSinceDate {
            get {
                object o = this["UserInactiveSinceDate"];
                Debug.Assert(o != null, "Should always have a default value!");
                return (DateTime) o;
            }
            set {
                _data["UserInactiveSinceDate"] = value;
            }
        }

        public string UsernameToMatch {
            get {
                return (string) this["UsernameToMatch"];
            }
            set {
                if (value != null) {
                    value = value.Trim();
                }
                _data["UsernameToMatch"] = value;
            }
        }

        public object this[string queryKey] {
            get {
                queryKey = StringUtil.CheckAndTrimString(queryKey, "queryKey");
                return _data[queryKey];
            }
            set {
                queryKey = StringUtil.CheckAndTrimString(queryKey, "queryKey");

                // VSWhidbey 436311: We need to check the value types for known properties
                if (_knownPropertyTypeMappings.ContainsKey(queryKey)) {
                    Type valueType = _knownPropertyTypeMappings[queryKey];
                    Debug.Assert(valueType != null);
                    if ((value == null && valueType.IsValueType) ||
                         (value != null && !valueType.IsAssignableFrom(value.GetType()))) {
                        throw new ArgumentException(
                            SR.GetString(SR.PersonalizationStateQuery_IncorrectValueType,
                                         queryKey, valueType.FullName));
                    }
                }

                _data[queryKey] = value;
            }
        }
    }
}

