//------------------------------------------------------------------------------
// <copyright file="SimpleHandlerBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Configuration;
using System.Web.Util;
using System.Web.UI;

[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
internal abstract class SimpleHandlerBuildProvider: InternalBuildProvider {
    private SimpleWebHandlerParser _parser;

    internal override IAssemblyDependencyParser AssemblyDependencyParser {
        get { return _parser; }
    }

    protected abstract SimpleWebHandlerParser CreateParser();

    public override CompilerType CodeCompilerType {
        get {
            Debug.Assert(_parser == null);

            _parser = CreateParser();
            _parser.SetBuildProvider(this);
            _parser.IgnoreParseErrors = IgnoreParseErrors;

            _parser.Parse(ReferencedAssemblies);

            return _parser.CompilerType;
        }
    }

    protected internal override CodeCompileUnit GetCodeCompileUnit(out IDictionary linePragmasTable) {
        Debug.Assert(_parser != null);

        CodeCompileUnit ccu = _parser.GetCodeModel();
        linePragmasTable = _parser.GetLinePragmasTable();

        return ccu;
    }

    public override void GenerateCode(AssemblyBuilder assemblyBuilder) {

        CodeCompileUnit codeCompileUnit = _parser.GetCodeModel();

        // Bail if we have nothing we need to compile
        if (codeCompileUnit == null)
            return;

        assemblyBuilder.AddCodeCompileUnit(this, codeCompileUnit);

        // Add all the assemblies
        if (_parser.AssemblyDependencies != null) {
            foreach (Assembly assembly in _parser.AssemblyDependencies) {
                assemblyBuilder.AddAssemblyReference(assembly, codeCompileUnit);
            }
        }

        // NOTE: we can't actually generate the fast factory because it would give
        // a really bad error if the user specifies a classname which doesn't match
        // the actual class they define.  A bit unfortunate, but not that big a deal...

        // tell the host to generate a fast factory for this type (if any)
        //string generatedTypeName = _parser.GeneratedTypeName;
        //if (generatedTypeName != null)
        //    assemblyBuilder.GenerateTypeFactory(generatedTypeName);
    }

    public override Type GetGeneratedType(CompilerResults results) {

        Type t;

        if (_parser.HasInlineCode) {

            // This is the case where the asmx/ashx has code in the file, and it
            // has been compiled.

            Debug.Assert(results != null);

            t = _parser.GetTypeToCache(results.CompiledAssembly);
        }
        else {

            // This is the case where the asmx/ashx has no code and is simply
            // pointing to an existing assembly.  Set the UsesExistingAssembly
            // flag accordingly.

            t = _parser.GetTypeToCache(null);
        }

        return t;
    }

    public override ICollection VirtualPathDependencies {
        get {
            return _parser.SourceDependencies;
        }
    }

    internal CompilerType GetDefaultCompilerTypeForLanguageInternal(string language) {
        return GetDefaultCompilerTypeForLanguage(language);
    }

    internal CompilerType GetDefaultCompilerTypeInternal() {
        return GetDefaultCompilerType();
    }

    internal TextReader OpenReaderInternal() {
        return OpenReader();
    }

    internal override ICollection GetGeneratedTypeNames() {
        // Note that _parser.TypeName does not necessarily point to the type defined in the handler file,
        // it could be any type that can be referenced at runtime, App_Code for example.
        return new SingleObjectCollection(_parser.TypeName);
    }
}

internal class WebServiceBuildProvider: SimpleHandlerBuildProvider {
    protected override SimpleWebHandlerParser CreateParser() {
        return new WebServiceParser(VirtualPath);
    }
}

internal class WebHandlerBuildProvider: SimpleHandlerBuildProvider {
    protected override SimpleWebHandlerParser CreateParser() {
        return new WebHandlerParser(VirtualPath);
    }
}

}
