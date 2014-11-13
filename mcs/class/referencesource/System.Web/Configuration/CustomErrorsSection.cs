//------------------------------------------------------------------------------
// <copyright file="CustomErrorsSection.cs" company="Microsoft">
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

    /* From Machine.Config
    <!--
        customErrors Attributes:
          mode="[On|Off|RemoteOnly]"
            On: Always display custom errors
            Off: Always display ASP.NET error pages
            RemoteOnly: Display custom errors to remote clients and ASP.NET errors to localhost
          redirectMode="[ResponseRedirect|ResponseRewrite]"
            ResponseRedirect: Use redirection to display the custom error page, the URL changes
            ResponseRewrite: Send the custom error page without changing the URL
          defaultRedirect="url" - Url to redirect client to when an error occurs
        -->
        <customErrors mode="RemoteOnly" />
    */
    public sealed class CustomErrorsSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propAllowNestedErrors =
            new ConfigurationProperty("allowNestedErrors",
                                        typeof(bool),
                                        false /* defaultValue */,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propDefaultRedirect =
            new ConfigurationProperty("defaultRedirect",
                                        typeof(string), 
                                        null, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propRedirectMode =
            new ConfigurationProperty("redirectMode",
                                        typeof(CustomErrorsRedirectMode),
                                        CustomErrorsRedirectMode.ResponseRedirect,
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propMode =
            new ConfigurationProperty("mode", 
                                        typeof(CustomErrorsMode), 
                                        CustomErrorsMode.RemoteOnly, 
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propErrors =
            new ConfigurationProperty(null, 
                                        typeof(CustomErrorCollection), 
                                        null, 
                                        ConfigurationPropertyOptions.IsDefaultCollection);

        private string basepath = null;
        private string _DefaultAbsolutePath = null;
        private static CustomErrorsSection _default = null;

        static CustomErrorsSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propAllowNestedErrors);
            _properties.Add(_propDefaultRedirect);
            _properties.Add(_propRedirectMode);
            _properties.Add(_propMode);
            _properties.Add(_propErrors);
        }

        public CustomErrorsSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("allowNestedErrors", DefaultValue = false)]
        public bool AllowNestedErrors {
            get {
                return (bool)base[_propAllowNestedErrors];
            }
            set {
                base[_propAllowNestedErrors] = value;
            }
        }

        [ConfigurationProperty("defaultRedirect")]
        public string DefaultRedirect {
            get {
                return (string)base[_propDefaultRedirect];
            }
            set {
                base[_propDefaultRedirect] = value;
            }
        }

        [ConfigurationProperty("redirectMode", DefaultValue = CustomErrorsRedirectMode.ResponseRedirect)]
        public CustomErrorsRedirectMode RedirectMode {
            get {
                return (CustomErrorsRedirectMode)base[_propRedirectMode];
            }
            set {
                base[_propRedirectMode] = value;
            }
        }

        [ConfigurationProperty("mode", DefaultValue = CustomErrorsMode.RemoteOnly)]
        public CustomErrorsMode Mode {
            get {
                return (CustomErrorsMode)base[_propMode];
            }
            set {
                base[_propMode] = value;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public CustomErrorCollection Errors {
            get {
                return (CustomErrorCollection)base[_propErrors];
            }
        }

        internal String DefaultAbsolutePath {
            get {
                if (_DefaultAbsolutePath == null) {
                    _DefaultAbsolutePath = GetAbsoluteRedirect(DefaultRedirect, basepath);
                }

                return _DefaultAbsolutePath;
            }
        }

        internal String GetRedirectString(int code) {
            String r = null;

            if (Errors != null) {
                CustomError ce = Errors[(string)code.ToString(CultureInfo.InvariantCulture)];
                if (ce != null)
                    r = GetAbsoluteRedirect(ce.Redirect, basepath);
            }

            if (r == null) {
                r = DefaultAbsolutePath;
            }

            return r;
        }

        protected override void Reset(ConfigurationElement parentElement) {
            base.Reset(parentElement);
            CustomErrorsSection parent = parentElement as CustomErrorsSection;
            if (parent != null) {
                basepath = parent.basepath;
            }
        }

        protected override void DeserializeSection(XmlReader reader) {
            WebContext context;

            base.DeserializeSection(reader);

            // Determine Web Context
            context = EvaluationContext.HostingContext as WebContext;

            if (context != null) {
                basepath = UrlPath.AppendSlashToPathIfNeeded(context.Path);
            }
        }
        //
        // helper to create absolute redirect
        //
        internal static String GetAbsoluteRedirect(String path, String basePath) {
            if (path != null && UrlPath.IsRelativeUrl(path)) {
                if (String.IsNullOrEmpty(basePath))
                    basePath = "/";

                path = UrlPath.Combine(basePath, path);
            }

            return path;
        }

        internal static CustomErrorsSection GetSettings(HttpContext context) {
            return GetSettings(context, false);
        }

        internal static CustomErrorsSection GetSettings(HttpContext context, bool canThrow) {
            CustomErrorsSection ce = null;
            RuntimeConfig runtimeConfig = null;
            if (canThrow) {
                runtimeConfig = RuntimeConfig.GetConfig(context);
                if (runtimeConfig != null) {
                    ce = runtimeConfig.CustomErrors;
                }
            }
            else {
                runtimeConfig = RuntimeConfig.GetLKGConfig(context);
                if (runtimeConfig != null) {
                    ce = runtimeConfig.CustomErrors;
                }
                if (ce == null) {
                    if (_default == null) {
                        _default = new CustomErrorsSection();
                    }

                    ce = _default;
                }
            }

            return ce;
        }

        internal bool CustomErrorsEnabled(HttpRequest request) {
            // This could throw if the config file is malformed, but we don't want
            // to throw from here, as it would mess up error handling
            try {
                // Always turn of custom errors in retail deployment mode (DevDiv 36396)
                if (DeploymentSection.RetailInternal)
                    return true;
            }
            catch { }

            switch (Mode) {
                case CustomErrorsMode.Off:
                    return false;

                case CustomErrorsMode.On:
                    return true;

                case CustomErrorsMode.RemoteOnly:
                    return (!request.IsLocal);

                default:
                    return false;
            }
        }
    }
}
