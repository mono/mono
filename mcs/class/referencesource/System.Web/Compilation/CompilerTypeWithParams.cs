//------------------------------------------------------------------------------
// <copyright file="CompilerType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.Security.Permissions;
using System.IO;
using System.Collections;
using System.Globalization;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Hosting;
using System.Web.Util;
using System.Web.UI;
using System.Web.Configuration;
    using System.Diagnostics.CodeAnalysis;

/*
 * This class describes a CodeDom compiler, along with the parameters that it uses.
 * The reason we need this class is that if two files both use the same language,
 * but ask for different command line options (e.g. debug vs retail), we will not
 * be able to compile them together.  So effectively, we need to treat them as
 * different languages.
 */
public sealed class CompilerType {

    private Type _codeDomProviderType;
    public Type CodeDomProviderType { get { return _codeDomProviderType; } }

    private CompilerParameters _compilParams;
    public CompilerParameters CompilerParameters { get { return _compilParams; } }

    internal CompilerType(Type codeDomProviderType, CompilerParameters compilParams) {

        Debug.Assert(codeDomProviderType != null);
        _codeDomProviderType = codeDomProviderType;

        if (compilParams == null)
            _compilParams = new CompilerParameters();
        else
            _compilParams = compilParams;
    }


    internal CompilerType Clone() {
        // Clone the CompilerParameters to make sure the original is untouched
        return new CompilerType(_codeDomProviderType, CloneCompilerParameters());
    }

    private CompilerParameters CloneCompilerParameters() {

        CompilerParameters copy = new CompilerParameters();
        copy.IncludeDebugInformation = _compilParams.IncludeDebugInformation;
        copy.TreatWarningsAsErrors = _compilParams.TreatWarningsAsErrors;
        copy.WarningLevel = _compilParams.WarningLevel;
        copy.CompilerOptions = _compilParams.CompilerOptions;

        return copy;
    }

    [SuppressMessage("Microsoft.Usage", "CA2303:FlagTypeGetHashCode", Justification = "This is used on codeDomProviderTypes which are not com types.")]
    public override int GetHashCode()
    {
        return _codeDomProviderType.GetHashCode();
    }

    public override bool Equals(Object o) {
        CompilerType other = o as CompilerType;
        if (o == null)
            return false;

        return _codeDomProviderType == other._codeDomProviderType &&
            _compilParams.WarningLevel == other._compilParams.WarningLevel &&
            _compilParams.IncludeDebugInformation == other._compilParams.IncludeDebugInformation &&
            _compilParams.CompilerOptions == other._compilParams.CompilerOptions;
    }

    internal AssemblyBuilder CreateAssemblyBuilder(CompilationSection compConfig,
        ICollection referencedAssemblies) {

        return CreateAssemblyBuilder(compConfig, referencedAssemblies,
            null /*generatedFilesDir*/, null /*outputAssemblyName*/);
    }

    internal AssemblyBuilder CreateAssemblyBuilder(CompilationSection compConfig,
        ICollection referencedAssemblies, string generatedFilesDir, string outputAssemblyName) {

        // Create a special AssemblyBuilder when we're only supposed to generate
        // source files but not compile them (for ClientBuildManager.GetCodeDirectoryInformation)
        if (generatedFilesDir != null) {
            return new CbmCodeGeneratorBuildProviderHost(compConfig,
                referencedAssemblies, this, generatedFilesDir, outputAssemblyName);
        }

        return new AssemblyBuilder(compConfig, referencedAssemblies, this, outputAssemblyName);
    }

    private static CompilerType GetDefaultCompilerTypeWithParams(
        CompilationSection compConfig, VirtualPath configPath) {

        // By default, use C# when no provider is asking for a specific language
        return CompilationUtil.GetCSharpCompilerInfo(compConfig, configPath);
    }

    internal static AssemblyBuilder GetDefaultAssemblyBuilder(CompilationSection compConfig,
        ICollection referencedAssemblies, VirtualPath configPath, string outputAssemblyName) {

        return GetDefaultAssemblyBuilder(compConfig, referencedAssemblies,
            configPath, null /*generatedFilesDir*/, outputAssemblyName);
    }

    internal static AssemblyBuilder GetDefaultAssemblyBuilder(CompilationSection compConfig,
        ICollection referencedAssemblies, VirtualPath configPath,
        string generatedFilesDir, string outputAssemblyName) {

        CompilerType ctwp = GetDefaultCompilerTypeWithParams(compConfig, configPath);
        return ctwp.CreateAssemblyBuilder(compConfig, referencedAssemblies,
            generatedFilesDir, outputAssemblyName);
    }
}

}
