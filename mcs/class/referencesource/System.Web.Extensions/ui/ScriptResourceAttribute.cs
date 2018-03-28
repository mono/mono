//------------------------------------------------------------------------------
// <copyright file="ScriptResourceAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Handlers;
    using System.Web.Resources;
    using System.Web.Script.Serialization;
    using System.Web.Util;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ScriptResourceAttribute : Attribute {
        private string _scriptName;
        private string _stringResourceName;
        private string _stringResourceClientTypeName;
        private static readonly Regex _webResourceRegEx = new Regex(
            @"<%\s*=\s*(?<resourceType>WebResource|ScriptResource)\(""(?<resourceName>[^""]*)""\)\s*%>",
            RegexOptions.Singleline | RegexOptions.Multiline);

        public ScriptResourceAttribute(string scriptName)
            : this(scriptName, null, null) {
        }

        [SuppressMessage("Microsoft.Naming","CA1720:IdentifiersShouldNotContainTypeNames", MessageId="string", Justification="Refers to 'string resource', not string the type.")]
        public ScriptResourceAttribute(string scriptName, string stringResourceName, string stringResourceClientTypeName) {
            if (String.IsNullOrEmpty(scriptName)) {
                throw new ArgumentException(AtlasWeb.Common_NullOrEmpty, "scriptName");
            }
            _scriptName = scriptName;
            _stringResourceName = stringResourceName;
            _stringResourceClientTypeName = stringResourceClientTypeName;
        }

        public string ScriptName {
            get {
                return _scriptName;
            }
        }

        [Obsolete("This property is obsolete. Use StringResourceName instead.")]
        public string ScriptResourceName {
            get {
                return StringResourceName;
            }
        }

        public string StringResourceClientTypeName {
            get {
                return _stringResourceClientTypeName;
            }
        }

        public string StringResourceName {
            get {
                return _stringResourceName;
            }
        }

        [Obsolete("This property is obsolete. Use StringResourceClientTypeName instead.")]
        public string TypeName {
            get {
                return StringResourceClientTypeName;
            }
        }

        private static void AddResources(Dictionary<String, String> resources,
            ResourceManager resourceManager, ResourceSet neutralSet) {
            foreach (DictionaryEntry res in neutralSet) {
                string key = (string)res.Key;
                string value = resourceManager.GetObject(key) as string;
                if (value != null) {
                    resources[key] = value;
                }
            }
        }

        private static Dictionary<String, String> CombineResources(
            ResourceManager resourceManager, ResourceSet neutralSet,
            ResourceManager releaseResourceManager, ResourceSet releaseNeutralSet) {
            Dictionary<String, String> resources = new Dictionary<String, String>(StringComparer.Ordinal);
            // add release resources first
            AddResources(resources, releaseResourceManager, releaseNeutralSet);
            // then debug, overwriting any existing release resources
            AddResources(resources, resourceManager, neutralSet);
            return resources;
        }

        private static void CopyScriptToStringBuilderWithSubstitution(
            string content, Assembly assembly, bool zip, StringBuilder output) {

            // Looking for something of the form: WebResource("resourcename")
            MatchCollection matches = _webResourceRegEx.Matches(content);
            int startIndex = 0;
            foreach (Match match in matches) {
                output.Append(content.Substring(startIndex, match.Index - startIndex));

                Group group = match.Groups["resourceName"];
                string embeddedResourceName = group.Value;
                bool isScriptResource = String.Equals(
                    match.Groups["resourceType"].Value, "ScriptResource", StringComparison.Ordinal);
                try {
                    if (isScriptResource) {
                        output.Append(ScriptResourceHandler.GetScriptResourceUrl(
                            assembly, embeddedResourceName, CultureInfo.CurrentUICulture, zip));
                    }
                    else {
                        output.Append(AssemblyResourceLoader.GetWebResourceUrlInternal(
                            assembly, embeddedResourceName, htmlEncoded: false, forSubstitution: true, scriptManager: null));
                    }
                }
                catch (HttpException e) {
                    throw new HttpException(String.Format(CultureInfo.CurrentCulture,
                        AtlasWeb.ScriptResourceHandler_UnknownResource,
                        embeddedResourceName), e);
                }

                startIndex = match.Index + match.Length;
            }

            output.Append(content.Substring(startIndex, content.Length - startIndex));
        }

        internal static ResourceManager GetResourceManager(string resourceName, Assembly assembly) {
            if (String.IsNullOrEmpty(resourceName)) {
                return null;
            }
            return new ResourceManager(GetResourceName(resourceName), assembly);
        }

        private static string GetResourceName(string rawResourceName) {
            if (rawResourceName.EndsWith(".resources", StringComparison.OrdinalIgnoreCase)) {
                return rawResourceName.Substring(0, rawResourceName.Length - 10);
            }
            return rawResourceName;
        }

        internal static string GetScriptFromWebResourceInternal(
            Assembly assembly, string resourceName, CultureInfo culture, 
            bool zip, out string contentType) {

            ScriptResourceInfo resourceInfo = ScriptResourceInfo.GetInstance(assembly, resourceName);
            ScriptResourceInfo releaseResourceInfo = null;
            if (resourceName.EndsWith(".debug.js", StringComparison.OrdinalIgnoreCase)) {
                // This is a debug script, we'll need to merge the debug resource
                // with the release one.
                string releaseResourceName = resourceName.Substring(0, resourceName.Length - 9) + ".js";
                releaseResourceInfo = ScriptResourceInfo.GetInstance(assembly, releaseResourceName);
            }
            if ((resourceInfo == ScriptResourceInfo.Empty) &&
                ((releaseResourceInfo == null) || (releaseResourceInfo == ScriptResourceInfo.Empty))) {

                throw new HttpException(AtlasWeb.ScriptResourceHandler_InvalidRequest);
            }

            ResourceManager resourceManager = null;
            ResourceSet neutralSet = null;
            ResourceManager releaseResourceManager = null;
            ResourceSet releaseNeutralSet = null;
            CultureInfo previousCulture = Thread.CurrentThread.CurrentUICulture;

            try {
                Thread.CurrentThread.CurrentUICulture = culture;

                if (!String.IsNullOrEmpty(resourceInfo.ScriptResourceName)) {
                    resourceManager = GetResourceManager(resourceInfo.ScriptResourceName, assembly);
                    // The following may throw MissingManifestResourceException
                    neutralSet = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);
                }
                if ((releaseResourceInfo != null) &&
                    !String.IsNullOrEmpty(releaseResourceInfo.ScriptResourceName)) {

                    releaseResourceManager = GetResourceManager(releaseResourceInfo.ScriptResourceName, assembly);
                    releaseNeutralSet = releaseResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);
                }
                if ((releaseResourceInfo != null) &&
                    !String.IsNullOrEmpty(releaseResourceInfo.ScriptResourceName) &&
                    !String.IsNullOrEmpty(resourceInfo.ScriptResourceName) &&
                    (releaseResourceInfo.TypeName != resourceInfo.TypeName)) {
                    throw new HttpException(String.Format(
                        CultureInfo.CurrentCulture,
                        AtlasWeb.ScriptResourceHandler_TypeNameMismatch,
                        releaseResourceInfo.ScriptResourceName));
                }

                StringBuilder builder = new StringBuilder();
                WriteScript(assembly,
                    resourceInfo, releaseResourceInfo,
                    resourceManager, neutralSet,
                    releaseResourceManager, releaseNeutralSet,
                    zip, builder);
                contentType = resourceInfo.ContentType;
                return builder.ToString();
            }
            finally {
                Thread.CurrentThread.CurrentUICulture = previousCulture;

                if (releaseNeutralSet != null) {
                    releaseNeutralSet.Dispose();
                }
                if (neutralSet != null) {
                    neutralSet.Dispose();
                }
            }
        }

        private static void RegisterNamespace(StringBuilder builder, string typeName, bool isDebug) {
            int lastDot = typeName.LastIndexOf('.');
            if (lastDot != -1) {
                builder.Append("Type.registerNamespace('");
                builder.Append(typeName.Substring(0, lastDot));
                builder.Append("');");
                if (isDebug) builder.AppendLine();
            }
        }

        private static void WriteResource(StringBuilder builder,
            Dictionary<String, String> resources,
            bool isDebug) {
            bool first = true;
            foreach (KeyValuePair<String,String> res in resources) {
                if (first) {
                    first = false;
                }
                else {
                    builder.Append(',');
                }
                if (isDebug) {
                    builder.AppendLine();
                }
                builder.Append('"');
                builder.Append(HttpUtility.JavaScriptStringEncode(res.Key));
                builder.Append("\":\"");
                builder.Append(HttpUtility.JavaScriptStringEncode(res.Value));
                builder.Append('"');
            }
        }

        private static void WriteResource(
            StringBuilder builder,
            ResourceManager resourceManager,
            ResourceSet neutralSet,
            bool isDebug) {

            bool first = true;
            foreach (DictionaryEntry res in neutralSet) {
                string key = (string)res.Key;
                string value = resourceManager.GetObject(key) as string;
                if (value != null) {
                    if (first) {
                        first = false;
                    }
                    else {
                        builder.Append(',');
                    }
                    if (isDebug) builder.AppendLine();
                    builder.Append('"');
                    builder.Append(HttpUtility.JavaScriptStringEncode(key));
                    builder.Append("\":\"");
                    builder.Append(HttpUtility.JavaScriptStringEncode(value));
                    builder.Append('"');
                }
            }
        }

        private static void WriteResourceToStringBuilder(
            ScriptResourceInfo resourceInfo, 
            ScriptResourceInfo releaseResourceInfo, 
            ResourceManager resourceManager, 
            ResourceSet neutralSet, 
            ResourceManager releaseResourceManager, 
            ResourceSet releaseNeutralSet, 
            StringBuilder builder) {

            if ((resourceManager != null) || (releaseResourceManager != null)) {
                string typeName = resourceInfo.TypeName;
                if (String.IsNullOrEmpty(typeName)) {
                    typeName = releaseResourceInfo.TypeName;
                }
                WriteResources(builder, typeName, resourceManager, neutralSet,
                    releaseResourceManager, releaseNeutralSet, resourceInfo.IsDebug);
            }
        }

        private static void WriteResources(StringBuilder builder, string typeName,
            ResourceManager resourceManager, ResourceSet neutralSet,
            ResourceManager releaseResourceManager, ResourceSet releaseNeutralSet,
            bool isDebug) {

            // DevDiv Bugs 131109: Resources and notification should go on a new line even in release mode
            // because main script may not end in a semi-colon or may end in a javascript comment.
            builder.AppendLine();
            RegisterNamespace(builder, typeName, isDebug);
            builder.Append(typeName);
            builder.Append("={");
            if ((resourceManager != null) && (releaseResourceManager != null)) {
                WriteResource(builder, CombineResources(resourceManager, neutralSet, releaseResourceManager, releaseNeutralSet), isDebug);
            }
            else {
                if (resourceManager != null) {
                    WriteResource(builder, resourceManager, neutralSet, isDebug);
                }
                else if (releaseResourceManager != null) {
                    WriteResource(builder, releaseResourceManager, releaseNeutralSet, isDebug);
                }
            }
            if (isDebug) {
                builder.AppendLine();
                builder.AppendLine("};");
            }
            else{
                builder.Append("};");
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification="Violation is no longer relevant due to 4.0 CAS model")]
        [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
        [SecuritySafeCritical]
        private static void WriteScript(Assembly assembly,
            ScriptResourceInfo resourceInfo, ScriptResourceInfo releaseResourceInfo,
            ResourceManager resourceManager, ResourceSet neutralSet,
            ResourceManager releaseResourceManager, ResourceSet releaseNeutralSet,
            bool zip, StringBuilder output) {

            using (StreamReader reader = new StreamReader(
                assembly.GetManifestResourceStream(resourceInfo.ScriptName), true)) {

                if (resourceInfo.IsDebug) {
                    // Output version information
                    AssemblyName assemblyName = assembly.GetName();
                    output.AppendLine("// Name:        " + resourceInfo.ScriptName);
                    output.AppendLine("// Assembly:    " + assemblyName.Name);
                    output.AppendLine("// Version:     " + assemblyName.Version.ToString());
                    output.AppendLine("// FileVersion: " + AssemblyUtil.GetAssemblyFileVersion(assembly));
                }
                if (resourceInfo.PerformSubstitution) {
                    CopyScriptToStringBuilderWithSubstitution(
                        reader.ReadToEnd(), assembly, zip, output);
                }
                else {
                    output.Append(reader.ReadToEnd());
                }
                WriteResourceToStringBuilder(resourceInfo, releaseResourceInfo, 
                    resourceManager, neutralSet, 
                    releaseResourceManager, releaseNeutralSet, 
                    output);
            }
        }
    }
}
