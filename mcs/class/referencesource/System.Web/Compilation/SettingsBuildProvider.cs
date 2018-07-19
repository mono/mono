 //------------------------------------------------------------------------------
// <copyright file="SettingsBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// Settings files are cut per DevDiv 32258
#if SETTINGS_FILE_SUPPORT

namespace System.Web.Compilation {

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
#if !FEATURE_PAL
using System.Configuration.Design;
#endif
using System.Xml;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Hosting;
using System.Web.Util;
using Util=System.Web.UI.Util;

internal class SettingsBuildProvider: BuildProvider {

    public override void GenerateCode(AssemblyBuilder assemblyBuilder)  {
#if !FEATURE_PAL // FEATURE_PAL does not support System.Configuration.Design
        CodeCompileUnit codeCompileUnit = new CodeCompileUnit();

        // Process the .settings file and generate a CodeCompileUnit from it
        using (Stream stream = VirtualPathProvider.OpenFile(VirtualPath)) {
            using (TextReader reader = new StreamReader(stream)) {
                SettingsSingleFileGenerator.Generate(
                    reader, codeCompileUnit, assemblyBuilder.CodeDomProvider, TypeAttributes.Public);
            }
        }

        // Add the CodeCompileUnit to the compilation
        assemblyBuilder.AddCodeCompileUnit(this, codeCompileUnit);
#else   // !FEATURE_PAL 
        throw new NotImplementedException("System.Configuration.Design - ROTORTODO");
#endif  // !FEATURE_PAL 
    }
}

}

#endif
