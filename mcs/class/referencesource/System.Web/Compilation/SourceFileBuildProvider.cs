//------------------------------------------------------------------------------
// <copyright file="SourceFileBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.IO;
using System.Collections;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Hosting;
using System.Web.Util;
using System.Web.UI;

internal sealed class SourceFileBuildProvider: InternalBuildProvider {

    private CodeSnippetCompileUnit _snippetCompileUnit;
    private BuildProvider _owningBuildProvider;

    public override CompilerType CodeCompilerType {
        get {
            return CompilationUtil.GetCompilerInfoFromVirtualPath(VirtualPathObject);
        }
    }

    private void EnsureCodeCompileUnit() {
        if (_snippetCompileUnit == null) {
            // Read the contents of the file
            string sourceString = Util.StringFromVirtualPath(VirtualPathObject);
            _snippetCompileUnit = new CodeSnippetCompileUnit(sourceString);
            _snippetCompileUnit.LinePragma = BaseCodeDomTreeGenerator.CreateCodeLinePragmaHelper(
                VirtualPath, 1);
        }
    }

    public override void GenerateCode(AssemblyBuilder assemblyBuilder) {
        EnsureCodeCompileUnit();
        assemblyBuilder.AddCodeCompileUnit(this, _snippetCompileUnit);
    }

    protected internal override CodeCompileUnit GetCodeCompileUnit(out IDictionary linePragmasTable) {
        EnsureCodeCompileUnit();
        linePragmasTable = new Hashtable();
        linePragmasTable[1] = _snippetCompileUnit.LinePragma;

        return _snippetCompileUnit;
    }

    // The owning build provider in case this course file is a partial compile-with code besides
    internal BuildProvider OwningBuildProvider {
        get { return _owningBuildProvider; }
        set { _owningBuildProvider = value; }
    }
}

}
