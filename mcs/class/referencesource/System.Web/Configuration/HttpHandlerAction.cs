//------------------------------------------------------------------------------
// <copyright file="HttpHandlerAction.cs" company="Microsoft">
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
    using System.Web.Util;
    using System.Web.Compilation;
    using System.Globalization;
    using System.Security.Permissions;

    public sealed class HttpHandlerAction : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propPath =
            new ConfigurationProperty("path",
                                      typeof(string),
                                      null,
                                      null,
                                      StdValidatorsAndConverters.NonEmptyStringValidator,
                                      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propVerb =
            new ConfigurationProperty("verb",
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
        private static readonly ConfigurationProperty _propValidate =
            new ConfigurationProperty("validate",
                                      typeof(bool),
                                      true,
                                      ConfigurationPropertyOptions.None);

        private Wildcard _requestType;
        private WildcardUrl _path;
        private Type _type;
        private string typeCache = null;

        static HttpHandlerAction() {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propPath);
            _properties.Add(_propVerb);
            _properties.Add(_propType);
            _properties.Add(_propValidate);
        }

        public HttpHandlerAction(String path, String type, String verb)
            : this(path, type, verb, true) {
        }

        public HttpHandlerAction(String path, String type, String verb, bool validate) {
            Path = path;
            Type = type;
            Verb = verb;
            Validate = validate;
        }

        internal HttpHandlerAction() {
        }

        internal string Key {
            get {
                return "verb=" + Verb + " | path=" + Path;
            }
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("path", IsRequired = true, IsKey = true)]
        public string Path {
            get {
                return (string)base[_propPath];
            }
            set {
                base[_propPath] = value;
            }
        }

        [ConfigurationProperty("verb", IsRequired = true, IsKey = true)]
        public string Verb {
            get {
                return (string)base[_propVerb];
            }
            set {
                base[_propVerb] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type {
            get {
                if (typeCache == null)
                    typeCache = (string)base[_propType];
                return typeCache;
            }
            set {
                base[_propType] = value;
                typeCache = value;
            }
        }

        internal Type TypeInternal {
            get {
                return _type;
            }
        }

        [ConfigurationProperty("validate", DefaultValue = true)]
        public bool Validate {
            get {
                return (bool)base[_propValidate];
            }
            set {
                base[_propValidate] = value;
            }
        }

        // Dev10 732000: In a homogenous AppDomain, it is necessary to assert FileIoPermission to load types outside
        // the AppDomain's grant set.
        [FileIOPermission(SecurityAction.Assert, AllFiles = FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery)]
        internal void InitValidateInternal() {
            string verb = Verb;

            // Remove all spaces from verbs before wildcard parsing.
            //   - We don't want get in "POST, GET" to be parsed into " GET".
            verb = verb.Replace(" ", String.Empty); // replace all " " with String.Empty in requestType

            _requestType = new Wildcard(verb, false);    // case-sensitive wildcard
            _path = new WildcardUrl(Path, true);         // case-insensitive URL wildcard

            // if validate="false" is marked on a handler, then the type isn't created until a request
            // is actually made that requires the handler. This (1) allows us to list handlers that
            // aren't present without throwing errors at init time and (2) speeds up init by avoiding
            // loading types until they are needed.

            if (!Validate) {
                _type = null;
            }
            else {
                _type = ConfigUtil.GetType(Type, "type", this);

                if (!ConfigUtil.IsTypeHandlerOrFactory(_type)) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Type_not_factory_or_handler, Type),
                        ElementInformation.Source, ElementInformation.LineNumber);
                }
            }
        }

        internal bool IsMatch(String verb, VirtualPath path) {
            return (_path.IsSuffix(path.VirtualPathString) && _requestType.IsMatch(verb));
        }

        internal Object Create() {
            // HACKHACK: for now, let uncreatable types through and error later (for .soap factory)
            // This design should change - developers will want to know immediately
            // when they misspell a type

            if (_type == null) {
                Type t = ConfigUtil.GetType(Type, "type", this);

                // throw for bad types in deferred case
                if (!ConfigUtil.IsTypeHandlerOrFactory(t)) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Type_not_factory_or_handler, Type),
                        ElementInformation.Source, ElementInformation.LineNumber);
                }

                _type = t;
            }

            return HttpRuntime.CreateNonPublicInstance(_type);
        }
    }
}

