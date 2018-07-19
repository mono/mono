//------------------------------------------------------------------------------
// <copyright file="ServiceReference.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.Resources;
    using System.Web.Script.Services;

    [
    DefaultProperty("Path")
    ]
    public class ServiceReference {
        private string _path;
        private bool _inlineScript;
        // needed at design time to remember what control owns this service reference (SM or SMP)
        internal Control _containingControl;

        public ServiceReference() {
        }

        public ServiceReference(string path) {
            // do not use the virtual Path property setter here as it would violate Microsft.Usage:DoNotCallOverridableMethodsInConstructors
            // A derived class is not likely to use this constructor anyway -- if they do, and they rely on an overridden Path property,
            // they could call the property directly rather than use this constructor.
            _path = path;
        }

        [
        ResourceDescription("ServiceReference_InlineScript"),
        DefaultValue(false),
        Category("Behavior")
        ]
        public virtual bool InlineScript {
            get {
                return _inlineScript;
            }
            set {
                _inlineScript = value;
            }
        }

        [
        ResourceDescription("ServiceReference_Path"),
        DefaultValue(""),
        Category("Behavior"),
        UrlProperty()
        ]
        public virtual string Path {
            get {
                return _path ?? String.Empty;
            }
            set {
                _path = value;
            }
        }

        protected internal virtual string GetProxyScript(ScriptManager scriptManager, Control containingControl) {
            string serviceUrl = GetServiceUrl(containingControl, false);
            try {
                serviceUrl = VirtualPathUtility.Combine(containingControl.Context.Request.FilePath, serviceUrl);
            }
            catch {
                throw new ArgumentException(
                    String.Format(CultureInfo.InvariantCulture, AtlasWeb.WebService_InvalidInlineVirtualPath, serviceUrl));
            }
            return WebServiceClientProxyGenerator.GetInlineClientProxyScript(serviceUrl,
                containingControl.Context,
                scriptManager.IsDebuggingEnabled);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification="Cannot change to URI for compatibility, and yet must also provide this extensibility point.")]
        protected internal virtual string GetProxyUrl(ScriptManager scriptManager, Control containingControl) {
            return GetServiceUrl(containingControl, true) +
                ((scriptManager.DesignMode || scriptManager.IsDebuggingEnabled) ?
                RestHandlerFactory.ClientDebugProxyRequestPathInfo :
                RestHandlerFactory.ClientProxyRequestPathInfo);
        }

        private string GetServiceUrl(Control containingControl, bool encodeSpaces) {
            string path = Path;
            if (String.IsNullOrEmpty(path)) {
                throw new InvalidOperationException(AtlasWeb.ServiceReference_PathCannotBeEmpty);
            }
            if (encodeSpaces) {
                path = containingControl.ResolveClientUrl(path);
            }
            else {
                path = containingControl.ResolveUrl(path);
            }
            return path;
        }

        internal void Register(Control containingControl, ScriptManager scriptManager) {
            if (InlineScript) {
                if (!scriptManager.IsRestMethodCall) {
                    string script = GetProxyScript(scriptManager, containingControl);
                    if (!String.IsNullOrEmpty(script)) {
                        scriptManager.RegisterClientScriptBlockInternal(scriptManager, typeof(ScriptManager), script, script, true);
                    }
                }
            }
            else {
                string url = GetProxyUrl(scriptManager, containingControl);
                if (!String.IsNullOrEmpty(url)) {
                    scriptManager.RegisterClientScriptIncludeInternal(scriptManager, typeof(ScriptManager), url, url);
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override string ToString() {
            if (!String.IsNullOrEmpty(Path)) {
                return Path;
            }
            else {
                return GetType().Name;
            }
        }
    }
}
