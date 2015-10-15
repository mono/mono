//------------------------------------------------------------------------------
// <copyright file="BaseResourcesBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

using System;
using System.Resources;
using System.Resources.Tools;
using System.Reflection;
using System.Globalization;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.Util;
using Util=System.Web.UI.Util;

/// Base class for BuildProviders that generate resources
[BuildProviderAppliesTo(BuildProviderAppliesTo.Resources)]
internal abstract class BaseResourcesBuildProvider : BuildProvider {

    internal const string DefaultResourcesNamespace = "Resources";

    // The generated namespace and type name
    private string _ns;
    private string _typeName;
    
    private string _cultureName;
    private bool _dontGenerateStronglyTypedClass;

    internal void DontGenerateStronglyTypedClass() {
        _dontGenerateStronglyTypedClass = true;
    }

    public override void GenerateCode(AssemblyBuilder assemblyBuilder) {

        _cultureName = GetCultureName();

        if (!_dontGenerateStronglyTypedClass) {
            // Get the namespace and type name that we will use
            _ns = Util.GetNamespaceAndTypeNameFromVirtualPath(VirtualPathObject,
                (_cultureName == null) ? 1 : 2 /*chunksToIgnore*/, out _typeName);

            // Always prepend the namespace with Resources.
            if (_ns.Length == 0)
                _ns = DefaultResourcesNamespace;
            else
                _ns = DefaultResourcesNamespace + "." + _ns;
        }

        // Get an input stream for our virtual path, and get a resource reader from it
        using (Stream inputStream = OpenStream()) {
            IResourceReader reader = GetResourceReader(inputStream);

            try {
                GenerateResourceFile(assemblyBuilder, reader);
            }
            catch (ArgumentException e) {
                // If the inner exception is Xml, throw that instead, as it contains more
                // useful info
                if (e.InnerException != null &&
                    (e.InnerException is XmlException || e.InnerException is XmlSchemaException)) {
                    throw e.InnerException;
                }

                // Otherwise, so just rethrow
                throw;
            }

            // Skip the code part for satellite assemblies, or if dontGenerate is set
            if (_cultureName == null && !_dontGenerateStronglyTypedClass)
                GenerateStronglyTypedClass(assemblyBuilder, reader);
        }
    }

    protected abstract IResourceReader GetResourceReader(Stream inputStream);

    private void GenerateResourceFile(AssemblyBuilder assemblyBuilder, IResourceReader reader) {

        // Get the name of the generated .resource file
        string resourceFileName;
        if (_ns == null) {
            // In the case where we don't generate code, just name the resource file
            // after the virtual file
            resourceFileName = UrlPath.GetFileNameWithoutExtension(VirtualPath) + ".resources";
        }
        else if (_cultureName == null) {
            // Name the resource file after the generated class, since that's what the
            // generated class expects
            resourceFileName = _ns + "." + _typeName + ".resources";
        }
        else {
            // If it's a non-default resource, include the culture in the name
            resourceFileName = _ns + "." + _typeName + "." + _cultureName + ".resources";
        }

        // Make it lower case, since GetManifestResourceStream (which we use later on) is
        // case sensitive
        resourceFileName = resourceFileName.ToLower(CultureInfo.InvariantCulture);

        Stream outputStream = null;

        try {
            try {
                try {
                }
                finally {
                    // Put the assignment in a finally block to avoid a ThreadAbortException from
                    // causing the created stream to not get assigned and become leaked (Dev10 
                    outputStream = assemblyBuilder.CreateEmbeddedResource(this, resourceFileName);
                }
            }
            catch (ArgumentException) {
                // This throws an ArgumentException if the resource file name was already added.
                // Catch the situation, and give a better error message (VSWhidbey 87110)

                throw new HttpException(SR.GetString(SR.Duplicate_Resource_File, VirtualPath));
            }

            // Create an output stream from the .resource file
            using (outputStream) {
                using (ResourceWriter writer = new ResourceWriter(outputStream)) {
                    // Enable resource writer to be target-aware
                    writer.TypeNameConverter = System.Web.UI.TargetFrameworkUtil.TypeNameConverter;

                    // Copy the resources
                    foreach (DictionaryEntry de in reader) {
                        writer.AddResource((string)de.Key, de.Value);
                    }
                }
            }
        }
        finally {
            // Always close the stream to avoid a ThreadAbortException from causing the stream
            // to be leaked (Dev10 
            if (outputStream != null) {
                outputStream.Close();
            }
        }
    }

    private void GenerateStronglyTypedClass(AssemblyBuilder assemblyBuilder, IResourceReader reader) {

        // Copy the resources into an IDictionary
        IDictionary resourceList;
        using (reader) {
            resourceList = GetResourceList(reader);
        }

        // Generate a strongly typed class from the resources
        CodeDomProvider provider = assemblyBuilder.CodeDomProvider;
        string[] unmatchable;
        CodeCompileUnit ccu = StronglyTypedResourceBuilder.Create(
            resourceList, _typeName, _ns,
            provider, false /*internalClass*/, out unmatchable);

        // Ignore the unmatchable items.  We just won't generate code for them,
        // but they'll still be usable via the ResourceManager (VSWhidbey 248226)

// We decided to cut support for My.Resources (VSWhidbey 358088)
#if OLD
        // generate a My.Resources.* override (VSWhidbey 251554)
        CodeNamespace ns = new CodeNamespace();
        ns.Name = "My." + _ns;

        CodeTypeDeclaration type = new CodeTypeDeclaration();
        type.Name = _typeName;
        CodeTypeReference baseType = new CodeTypeReference(_ns + "." + _typeName);

        // Need to use a global reference to avoid a conflict, since the classes have the same name
        baseType.Options = CodeTypeReferenceOptions.GlobalReference;
        type.BaseTypes.Add(baseType);

        ns.Types.Add(type);
        ccu.Namespaces.Add(ns);
#endif

        // Add the code compile unit to the compilation
        assemblyBuilder.AddCodeCompileUnit(this, ccu);
    }

    private IDictionary GetResourceList(IResourceReader reader) {

        // Read the resources into a dictionary.
        IDictionary resourceList = new Hashtable(StringComparer.OrdinalIgnoreCase);
        foreach(DictionaryEntry de in reader)
            resourceList.Add(de.Key, de.Value);

        return resourceList;
    }
}

}
