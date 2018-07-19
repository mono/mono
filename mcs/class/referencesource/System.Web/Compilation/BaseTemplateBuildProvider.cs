//------------------------------------------------------------------------------
// <copyright file="BaseTemplateBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Util;
using System.Web.UI;

internal abstract class BaseTemplateBuildProvider: InternalBuildProvider {

    private TemplateParser _parser;
    internal TemplateParser Parser { get { return _parser; } }

    internal override IAssemblyDependencyParser AssemblyDependencyParser {
        get { return _parser; }
    }

    private string _instantiatableFullTypeName;
    private string _intermediateFullTypeName;

    protected abstract TemplateParser CreateParser();

    internal abstract BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(TemplateParser parser);

    protected internal override CodeCompileUnit GetCodeCompileUnit(out IDictionary linePragmasTable) {
        Debug.Assert(_parser != null);

        // Return the provider type and compiler params
        Type codeDomProviderType = _parser.CompilerType.CodeDomProviderType;

        // Create a code generator for the language
        CodeDomProvider codeDomProvider = CompilationUtil.CreateCodeDomProviderNonPublic(
            codeDomProviderType);

        // Create a designer mode codedom tree for the page
        BaseCodeDomTreeGenerator treeGenerator = CreateCodeDomTreeGenerator(_parser);
        treeGenerator.SetDesignerMode();
        CodeCompileUnit ccu = treeGenerator.GetCodeDomTree(codeDomProvider,
            new StringResourceBuilder(), VirtualPathObject);
        linePragmasTable = treeGenerator.LinePragmasTable;

// This code is used to see the full generated code in the debugger.  Just uncomment and look at
// generatedCode in the debugger.  Don't check in with this code uncommented!
#if TESTCODE
        Stream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.Unicode);
        codeDomProvider.GenerateCodeFromCompileUnit(ccu, writer, null /*CodeGeneratorOptions*/);
        writer.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        TextReader reader = new StreamReader(stream);
        string generatedCode = reader.ReadToEnd();
#endif

        return ccu;
    }

    public override CompilerType CodeCompilerType {
        get {
            Debug.Assert(_parser == null);

            _parser = CreateParser();

            if (IgnoreParseErrors)
                _parser.IgnoreParseErrors = true;

            if (IgnoreControlProperties)
                _parser.IgnoreControlProperties = true;

            if (!ThrowOnFirstParseError)
                _parser.ThrowOnFirstParseError = false;

            _parser.Parse(ReferencedAssemblies, VirtualPathObject);

            // If the page is non-compiled, don't ask for a language
            if (!Parser.RequiresCompilation)
                return null;

            return _parser.CompilerType;
        }
    }

    internal override ICollection GetCompileWithDependencies() {

        // If there is a code besides file, return it

        if (_parser.CodeFileVirtualPath == null)
            return null;

        // no-compile pages should not have any compile with dependencies
        Debug.Assert(Parser.RequiresCompilation);

        return new SingleObjectCollection(_parser.CodeFileVirtualPath);
    }

    public override void GenerateCode(AssemblyBuilder assemblyBuilder) {

        // Don't generate any code for no-compile pages
        if (!Parser.RequiresCompilation)
            return;

        BaseCodeDomTreeGenerator treeGenerator = CreateCodeDomTreeGenerator(_parser);

        CodeCompileUnit ccu = treeGenerator.GetCodeDomTree(assemblyBuilder.CodeDomProvider,
            assemblyBuilder.StringResourceBuilder, VirtualPathObject);

        if (ccu != null) {
            // Add all the assemblies
            if (_parser.AssemblyDependencies != null) {
                foreach (Assembly assembly in _parser.AssemblyDependencies) {
                    assemblyBuilder.AddAssemblyReference(assembly, ccu);
                }
            }

            assemblyBuilder.AddCodeCompileUnit(this, ccu);
        }

        // Get the name of the generated type that can be instantiated.  It may be null
        // in updatable compilation scenarios.
        _instantiatableFullTypeName = treeGenerator.GetInstantiatableFullTypeName();

        // tell the assembly builder to generate a fast factory for this type
        if (_instantiatableFullTypeName != null)
            assemblyBuilder.GenerateTypeFactory(_instantiatableFullTypeName);

        _intermediateFullTypeName = treeGenerator.GetIntermediateFullTypeName();
    }

    public override Type GetGeneratedType(CompilerResults results) {
        return GetGeneratedType(results, useDelayLoadTypeIfEnabled: false);
    }

    internal Type GetGeneratedType(CompilerResults results, bool useDelayLoadTypeIfEnabled) {

        // No Type is generated for no-compile pages
        if (!Parser.RequiresCompilation)
            return null;

        // Figure out the Type that needs to be persisted
        string typeName;

        if (_instantiatableFullTypeName == null) {

            if (Parser.CodeFileVirtualPath != null) {
                // Updatable precomp of a code separation page: use the intermediate type
                typeName = _intermediateFullTypeName;
            }
            else {
                // Updatable precomp of a single page: use the base type, since nothing got compiled
                return Parser.BaseType;
            }
        }
        else {
            typeName = _instantiatableFullTypeName;
        }

        Debug.Assert(typeName != null);

        Type generatedType;
        if (useDelayLoadTypeIfEnabled && DelayLoadType.Enabled) {
            string assemblyFilename = Path.GetFileName(results.PathToAssembly);
            string assemblyName = Util.GetAssemblyNameFromFileName(assemblyFilename);
            generatedType = new DelayLoadType(assemblyName, typeName);
        }
        else {
            generatedType = results.CompiledAssembly.GetType(typeName);
        }

        // It should always extend the required base type
        // Note: removing this assert as advanced ControlBuilder scenarios allow changing the base type
        //Debug.Assert(Parser.BaseType.IsAssignableFrom(generatedType));

        return generatedType;
    }

    internal override BuildResultCompiledType CreateBuildResult(Type t) {
        return new BuildResultCompiledTemplateType(t);
    }

    public override ICollection VirtualPathDependencies {
        get {
            return _parser.SourceDependencies;
        }
    }

    internal override ICollection GetGeneratedTypeNames() {
        if (_parser.GeneratedClassName == null && _parser.BaseTypeName == null) {
            return null;
        }

        ArrayList collection = new ArrayList();
        if (_parser.GeneratedClassName != null) {
            collection.Add(_parser.GeneratedClassName);
        }

        if (_parser.BaseTypeName != null) {
            collection.Add(Util.MakeFullTypeName(_parser.BaseTypeNamespace, _parser.BaseTypeName));
        }

        return collection;
    }
}

}
