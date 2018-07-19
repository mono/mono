//------------------------------------------------------------------------------
// <copyright file="CompositeScriptReference.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Handlers;
    using System.Web.Resources;
    using System.Web.Util;

    [
    DefaultProperty("Path"),
    TypeConverter(typeof(EmptyStringExpandableObjectConverter))
    ]
    public class CompositeScriptReference : ScriptReferenceBase {
        private ScriptReferenceCollection _scripts;

        [
        ResourceDescription("CompositeScriptReference_Scripts"),
        Category("Behavior"),
        Editor("System.Web.UI.Design.CollectionEditorBase, " +
            AssemblyRef.SystemWebExtensionsDesign, typeof(UITypeEditor)),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        NotifyParentProperty(true),
        MergableProperty(false),
        ]
        public ScriptReferenceCollection Scripts {
            get {
                if (_scripts == null) {
                    _scripts = new ScriptReferenceCollection();
                }
                return _scripts;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1055", Justification = "Consistent with other URL properties in ASP.NET.")]
        protected internal override string GetUrl(ScriptManager scriptManager, bool zip) {
            bool isDebuggingEnabled = !scriptManager.DeploymentSectionRetail &&
                    ((ScriptMode == ScriptMode.Debug) ||
                    (((ScriptMode == ScriptMode.Inherit) || (ScriptMode == ScriptMode.Auto)) &&
                    (scriptManager.IsDebuggingEnabled)));
            if (!String.IsNullOrEmpty(Path)) {
                string path = Path;
                if (isDebuggingEnabled) {
                    path = GetDebugPath(path);
                }
                if (scriptManager.EnableScriptLocalization &&
                    (ResourceUICultures != null) && (ResourceUICultures.Length != 0)) {

                    CultureInfo currentCulture = CultureInfo.CurrentUICulture;
                    string cultureName = null;
                    bool found = false;
                    while (!currentCulture.Equals(CultureInfo.InvariantCulture)) {
                        cultureName = currentCulture.ToString();
                        foreach (string uiCulture in ResourceUICultures) {
                            if (String.Equals(cultureName, uiCulture.Trim(), StringComparison.OrdinalIgnoreCase)) {
                                found = true;
                                break;
                            }
                        }
                        if (found) break;
                        currentCulture = currentCulture.Parent;
                    }
                    if (found) {
                        path = (path.Substring(0, path.Length - 2) + cultureName + ".js");
                    }
                }

                // ResolveClientUrl is appropriate here because the path is consumed by the page it was declared within
                return ClientUrlResolver.ResolveClientUrl(path);
            }
            List<Tuple<Assembly, List<Tuple<string, CultureInfo>>>> resources =
                new List<Tuple<Assembly, List<Tuple<string, CultureInfo>>>>();
            Tuple<Assembly, List<Tuple<string, CultureInfo>>> resourceList = null;
            foreach (ScriptReference reference in Scripts) {
                if ((scriptManager.AjaxFrameworkMode == AjaxFrameworkMode.Explicit) &&
                    reference.IsAjaxFrameworkScript(scriptManager) &&
                    reference.EffectiveResourceName.StartsWith("MicrosoftAjax.", StringComparison.Ordinal)) {
                    continue;
                }
                bool hasPath = !String.IsNullOrEmpty(reference.EffectivePath);
#pragma warning disable 618
                // ScriptPath is obsolete but still functional
                bool hasScriptPath = (!String.IsNullOrEmpty(scriptManager.ScriptPath) && !reference.IgnoreScriptPath);
#pragma warning restore 618
                // cacheAssembly will be null if ScriptPath is set, but we still need the resource assembly in that case
                Assembly resourceAssembly = null;
                string resourceName = null;
                Assembly cacheAssembly = null;
                ScriptMode effectiveScriptModeForReference = reference.EffectiveScriptMode;
                bool isDebuggingEnabledForReference =
                    (effectiveScriptModeForReference == ScriptMode.Inherit) ?
                    isDebuggingEnabled :
                    (effectiveScriptModeForReference == ScriptMode.Debug); 
                if (!hasPath) {
                    resourceAssembly = reference.GetAssembly(scriptManager);
                    resourceName = reference.EffectiveResourceName;
                    reference.DetermineResourceNameAndAssembly(scriptManager, isDebuggingEnabledForReference,
                        ref resourceName, ref resourceAssembly);
                    if ((resourceAssembly != scriptManager.AjaxFrameworkAssembly) &&
                        (resourceAssembly != AssemblyCache.SystemWebExtensions) &&
                        AssemblyCache.IsAjaxFrameworkAssembly(resourceAssembly)) {
                        // if it is coming from an assembly that is not the current ajax script assembly, make sure the assembly
                        // is not meant to be an ajax script assembly.
                        // it isnt an AjaxFrameworkScript but it might be from an assembly that is meant to
                        // be an ajax script assembly, in which case we should throw an error.
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentUICulture,
                            AtlasWeb.ScriptReference_ResourceRequiresAjaxAssembly, resourceName, resourceAssembly));
                    }
                    if (!hasScriptPath) {
                        // The resource requested in the composite url will only contain the assembly name if it
                        // will ultimately come from the assembly -- if ScriptPath is set, it doesn't.
                        // We do still need to know the resource assembly in that case though, hence the separate
                        // assembly variables.
                        cacheAssembly = resourceAssembly;
                    }
                }

                CultureInfo culture = reference.DetermineCulture(scriptManager);
                if ((resourceList == null) || (resourceList.Item1 != cacheAssembly)) {
                    resourceList = new Tuple<Assembly, List<Tuple<string, CultureInfo>>>(
                    cacheAssembly, new List<Tuple<string, CultureInfo>>());
                    resources.Add(resourceList);
                }
                if (hasPath || hasScriptPath) {
                    if (hasPath) {
                        if (String.IsNullOrEmpty(reference.Path)) {
                            // the Path is coming from a script mapping, so its debug path applies
                            resourceName = reference.GetPath(scriptManager, reference.EffectivePath, reference.ScriptInfo.DebugPath,
                                isDebuggingEnabledForReference);
                        }
                        else {
                            // path explicitly set, even if a mapping has a DebugPath it does not apply
                            resourceName = reference.GetPath(scriptManager, reference.Path, null,
                                isDebuggingEnabledForReference);
                        }
                    }
                    else {
#pragma warning disable 618
                        // ScriptPath is obsolete but still functional
                        resourceName = ScriptReference.GetScriptPath(resourceName, resourceAssembly,
                            culture, scriptManager.ScriptPath);
#pragma warning restore 618
                    }

                    // ResolveClientUrl not appropriate here because the handler that will serve the response is not
                    // in the same directory as the page that is generating the url. Instead, an absolute url is needed
                    // as with ResolveUrl(). However, ResolveUrl() would prepend the entire application root name. For
                    // example, ~/foo.js would be /TheApplicationRoot/foo.js. If there are many path based scripts the
                    // app root would be repeated many times, which for deep apps or long named apps could cause the url
                    // to reach the maximum 2048 characters very quickly. So, the path is combined with the control's
                    // AppRelativeTemplateSourceDirectory manually, so that ~/foo.js remains ~/foo.js, and foo/bar.js
                    // becomes ~/templatesource/foo/bar.js. Absolute paths can remain as is. The ScriptResourceHandler will
                    // resolve the ~/ with the app root using VirtualPathUtility.ToAbsolute().
                    if (UrlPath.IsRelativeUrl(resourceName) && !UrlPath.IsAppRelativePath(resourceName)) {
                        resourceName = UrlPath.Combine(ClientUrlResolver.AppRelativeTemplateSourceDirectory, resourceName);
                    }
                }
                resourceList.Item2.Add(new Tuple<string, CultureInfo>(resourceName, culture));
            }
            return ScriptResourceHandler.GetScriptResourceUrl(resources, zip);
        }

        [Obsolete("Use IsAjaxFrameworkScript(ScriptManager)")]
        protected internal override bool IsFromSystemWebExtensions() {
            foreach (ScriptReference script in Scripts) {
                if (script.EffectiveAssembly == AssemblyCache.SystemWebExtensions) {
                    return true;
                }
            }
            return false;
        }

        protected internal override bool IsAjaxFrameworkScript(ScriptManager scriptManager) {
            foreach (ScriptReference script in Scripts) {
                if (script.IsAjaxFrameworkScript(scriptManager)) {
                    return true;
                }
            }
            return false;
        }
    }
}
