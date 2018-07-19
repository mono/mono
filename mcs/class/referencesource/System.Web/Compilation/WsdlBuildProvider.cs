//------------------------------------------------------------------------------
// <copyright file="WsdlBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

using System;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Serialization;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Util;
using Util=System.Web.UI.Util;

[BuildProviderAppliesTo(BuildProviderAppliesTo.Code)]
internal class WsdlBuildProvider: BuildProvider {

    public override void GenerateCode(AssemblyBuilder assemblyBuilder)  {

        // Get the namespace that we will use
        string ns = Util.GetNamespaceFromVirtualPath(VirtualPathObject);

        ServiceDescription sd;

        // Load the wsdl file
        using (Stream stream = VirtualPathObject.OpenFile()) {
            try {
                sd = ServiceDescription.Read(stream);
            }
            catch (InvalidOperationException e) {
                // It can throw an InvalidOperationException, with the relevant
                // XmlException as the inner exception.  If so, throw that instead.
                XmlException xmlException = e.InnerException as XmlException;
                if (xmlException != null)
                    throw xmlException;
                throw;
            }
        }

        ServiceDescriptionImporter importer = new ServiceDescriptionImporter();

#if !FEATURE_PAL
        importer.CodeGenerator = assemblyBuilder.CodeDomProvider;

        importer.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties |
            CodeGenerationOptions.GenerateNewAsync | CodeGenerationOptions.GenerateOldAsync;
#endif // !FEATURE_PAL
        importer.ServiceDescriptions.Add(sd);

        CodeCompileUnit codeCompileUnit = new CodeCompileUnit();

        CodeNamespace codeNamespace = new CodeNamespace(ns);
        codeCompileUnit.Namespaces.Add(codeNamespace);

        // Create the code compile unit
        importer.Import(codeNamespace, codeCompileUnit);

        // Add the CodeCompileUnit to the compilation
        assemblyBuilder.AddCodeCompileUnit(this, codeCompileUnit);
    }
}

}
