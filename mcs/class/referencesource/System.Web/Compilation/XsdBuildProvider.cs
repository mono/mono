//------------------------------------------------------------------------------
// <copyright file="XsdBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

using System;
using System.IO;
using System.Data;
using System.Data.Design;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Hosting;
using System.Web.Configuration;
using System.Collections;

using Util=System.Web.UI.Util;
#if !FEATURE_PAL // FEATURE_PAL does not support System.Data.Design
using TypedDataSetGenerator=System.Data.Design.TypedDataSetGenerator;
#endif // !FEATURE_PAL 

[BuildProviderAppliesTo(BuildProviderAppliesTo.Code)]
internal class XsdBuildProvider: BuildProvider {

    [SuppressMessage("Microsoft.Security", "MSEC1207:UseXmlReaderForLoad", Justification = "Developer-controlled .xsd files in application directory are implicitly trusted by ASP.Net.")]
    public override void GenerateCode(AssemblyBuilder assemblyBuilder)  {
#if !FEATURE_PAL // FEATURE_PAL does not support System.Data.Design
        // Get the namespace that we will use
        string ns = Util.GetNamespaceFromVirtualPath(VirtualPathObject);

        // We need to use XmlDocument to parse the xsd file is order to open it with the
        // correct encoding (VSWhidbey 566286)
        XmlDocument doc = new XmlDocument();
        using (Stream stream = OpenStream()) {
            doc.Load(stream);
        }
        String content = doc.OuterXml;

        // Generate a CodeCompileUnit from the dataset
        CodeCompileUnit codeCompileUnit = new CodeCompileUnit();

        CodeNamespace codeNamespace = new CodeNamespace(ns);
        codeCompileUnit.Namespaces.Add(codeNamespace);

        // Devdiv 18365, Dev10 bug 444516 
        // Call a different Generate method if compiler version is v3.5 or above
        bool isVer35OrAbove = CompilationUtil.IsCompilerVersion35OrAbove(assemblyBuilder.CodeDomProvider.GetType());

        if (isVer35OrAbove) {
            TypedDataSetGenerator.GenerateOption generateOptions = TypedDataSetGenerator.GenerateOption.None;
            generateOptions |= TypedDataSetGenerator.GenerateOption.HierarchicalUpdate;
            generateOptions |= TypedDataSetGenerator.GenerateOption.LinqOverTypedDatasets;
            Hashtable customDBProviders = null;
            TypedDataSetGenerator.Generate(content, codeCompileUnit, codeNamespace, assemblyBuilder.CodeDomProvider, customDBProviders, generateOptions);
        }
        else {
            TypedDataSetGenerator.Generate(content, codeCompileUnit, codeNamespace, assemblyBuilder.CodeDomProvider);
        }

        // Add all the assembly references needed by the generated code
        if (TypedDataSetGenerator.ReferencedAssemblies != null) {
            var isVer35 = CompilationUtil.IsCompilerVersion35(assemblyBuilder.CodeDomProvider.GetType());
            foreach (Assembly a in TypedDataSetGenerator.ReferencedAssemblies) {
                
                if (isVer35) {
                    var aName = a.GetName();
                    if (aName.Name == "System.Data.DataSetExtensions") {
                        // Dev10 Bug 861688 - We need to specify v3.5 version so that the build system knows to use the v3.5 version
                        // because the loaded assembly here is always v4.0
                        aName.Version = new Version(3, 5, 0, 0);
                        CompilationSection.RecordAssembly(aName.FullName, a);
                    }
                }
                assemblyBuilder.AddAssemblyReference(a);
            }
        }
        

        // Add the CodeCompileUnit to the compilation
        assemblyBuilder.AddCodeCompileUnit(this, codeCompileUnit);
#else // !FEATURE_PAL 
        throw new NotImplementedException("System.Data.Design - ROTORTODO");
#endif // !FEATURE_PAL 
    }
}
}
