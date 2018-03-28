//------------------------------------------------------------------------------
// <copyright file="WebReferencesBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

using System;
using System.Globalization;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Net;
using System.Xml.Serialization;
#if !FEATURE_PAL
using System.Web.Services.Description;
using System.Web.Services.Discovery;
#endif // !FEATURE_PAL
using System.Web.Hosting;
using System.Web.UI;
using System.Web.Util;
using Util=System.Web.UI.Util;

internal class WebReferencesBuildProvider: BuildProvider {

    private VirtualDirectory _vdir;

    private const string IndigoWebRefProviderTypeName = "System.Web.Compilation.WCFBuildProvider";
    private static Type s_indigoWebRefProviderType;
    private static bool s_triedToGetWebRefType;

    internal WebReferencesBuildProvider(VirtualDirectory vdir) {
        _vdir = vdir;
    }

    public override void GenerateCode(AssemblyBuilder assemblyBuilder)  {

        // Only attempt to get the Indigo provider once
        if (!s_triedToGetWebRefType) {
            s_indigoWebRefProviderType = BuildManager.GetType(IndigoWebRefProviderTypeName, false /*throwOnError*/);
            s_triedToGetWebRefType = true;
        }

        // If we have an Indigo provider, instantiate it and forward the GenerateCode call to it
        if (s_indigoWebRefProviderType != null) {
            BuildProvider buildProvider = (BuildProvider)HttpRuntime.CreateNonPublicInstance(s_indigoWebRefProviderType);
            buildProvider.SetVirtualPath(VirtualPathObject);
            buildProvider.GenerateCode(assemblyBuilder);
        }

        // e.g "/MyApp/Application_WebReferences"
        VirtualPath rootWebRefDirVirtualPath = HttpRuntime.WebRefDirectoryVirtualPath;

        // e.g "/MyApp/Application_WebReferences/Foo/Bar"
        string currentWebRefDirVirtualPath = _vdir.VirtualPath;

        Debug.Assert(StringUtil.StringStartsWithIgnoreCase(
            currentWebRefDirVirtualPath, rootWebRefDirVirtualPath.VirtualPathString));

        string ns;

        if (rootWebRefDirVirtualPath.VirtualPathString.Length == currentWebRefDirVirtualPath.Length) {
            // If it's the root WebReferences dir, use the empty namespace
            ns = String.Empty;
        }
        else {
            // e.g. "Foo/Bar"
            Debug.Assert(rootWebRefDirVirtualPath.HasTrailingSlash);
            currentWebRefDirVirtualPath = UrlPath.RemoveSlashFromPathIfNeeded(currentWebRefDirVirtualPath);
            currentWebRefDirVirtualPath = currentWebRefDirVirtualPath.Substring(
                rootWebRefDirVirtualPath.VirtualPathString.Length);

            // Split it into chunks separated by '/'
            string[] chunks = currentWebRefDirVirtualPath.Split('/');

            // Turn all the relevant chunks into valid namespace chunks
            for (int i=0; i<chunks.Length; i++) {
                chunks[i] = Util.MakeValidTypeNameFromString(chunks[i]);
            }

            // Put the relevant chunks back together to form the namespace
            ns = String.Join(".", chunks);
        }
#if !FEATURE_PAL // FEATURE_PAL does not support System.Web.Services

        CodeNamespace codeNamespace = new CodeNamespace(ns);

        // for each discomap file, read all references and add them to the WebReferenceCollection
        WebReferenceCollection webs = new WebReferenceCollection();

        bool hasDiscomap = false;

        // Go through all the discomap in the directory
        foreach (VirtualFile child in _vdir.Files) {

            string extension = UrlPath.GetExtension(child.VirtualPath);
            extension = extension.ToLower(CultureInfo.InvariantCulture);

            if (extension == ".discomap") {
                // NOTE: the WebReferences code requires physical path, so this feature
                // cannot work with a non-file based VirtualPathProvider
                string physicalPath = HostingEnvironment.MapPath(child.VirtualPath);

                DiscoveryClientProtocol client = new DiscoveryClientProtocol();
                client.AllowAutoRedirect = true;
                client.Credentials = CredentialCache.DefaultCredentials;

                client.ReadAll(physicalPath);

                WebReference webRefTemp = new WebReference(client.Documents, codeNamespace);

                // 

                string fileName = System.IO.Path.ChangeExtension(UrlPath.GetFileName(child.VirtualPath), null);
                string appSetttingUrlKey = ns + "." + fileName;

                WebReference web = new WebReference(client.Documents, codeNamespace, webRefTemp.ProtocolName, appSetttingUrlKey, null);

                webs.Add(web);

                hasDiscomap = true;
            }
        }

        // If we didn't find any discomap files, we have nothing to generate
        if (!hasDiscomap)
            return;

        CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
        codeCompileUnit.Namespaces.Add(codeNamespace);

        //public static StringCollection GenerateWebReferences(WebReferenceCollection webReferences, CodeDomProvider codeProvider, CodeCompileUnit codeCompileUnit, WebReferenceOptions options) {
        WebReferenceOptions options = new WebReferenceOptions();
        options.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateNewAsync | CodeGenerationOptions.GenerateOldAsync;
        options.Style = ServiceDescriptionImportStyle.Client;
        options.Verbose = true;
        StringCollection shareWarnings = ServiceDescriptionImporter.GenerateWebReferences(webs, assemblyBuilder.CodeDomProvider, codeCompileUnit, options);
        // Add the CodeCompileUnit to the compilation
        assemblyBuilder.AddCodeCompileUnit(this, codeCompileUnit);
#else // !FEATURE_PAL
        return;
#endif // !FEATURE_PAL
    }
}

}
