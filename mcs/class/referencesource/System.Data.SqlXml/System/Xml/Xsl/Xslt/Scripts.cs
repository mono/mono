//------------------------------------------------------------------------------
// <copyright file="Scripts.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <spec>http://devdiv/Documents/Whidbey/CLR/CurrentSpecs/BCL/CodeDom%20Activation.doc</spec>
//------------------------------------------------------------------------------

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using System.Threading;
using System.Xml.Xsl.IlGen;
using System.Xml.Xsl.Runtime;
using System.Runtime.Versioning;

namespace System.Xml.Xsl.Xslt {
    using Res = System.Xml.Utils.Res;

    internal class ScriptClass {
        public string               ns;
        public CompilerInfo         compilerInfo;
        public StringCollection     refAssemblies;
        public StringCollection     nsImports;
        public CodeTypeDeclaration  typeDecl;
        public bool                 refAssembliesByHref;

        public Dictionary<string, string> scriptUris;

        // These two fields are used to report a compile error when its position is outside
        // of all user code snippets in the generated temporary file
        public string               endUri;
        public Location             endLoc;

        public ScriptClass(string ns, CompilerInfo compilerInfo) {
            this.ns             = ns;
            this.compilerInfo   = compilerInfo;
            this.refAssemblies  = new StringCollection();
            this.nsImports      = new StringCollection();
            this.typeDecl       = new CodeTypeDeclaration(GenerateUniqueClassName());
            this.refAssembliesByHref = false;
            this.scriptUris     = new Dictionary<string, string>(
#if !FEATURE_CASE_SENSITIVE_FILESYSTEM            
                StringComparer.OrdinalIgnoreCase
#endif
            );
        }

        private static long scriptClassCounter = 0;

        private static string GenerateUniqueClassName() {
            return "Script" + Interlocked.Increment(ref scriptClassCounter);
        }

        public void AddScriptBlock(string source, string uriString, int lineNumber, Location end) {
            CodeSnippetTypeMember scriptSnippet = new CodeSnippetTypeMember(source);
            string fileName = SourceLineInfo.GetFileName(uriString);
            if (lineNumber > 0) {
                scriptSnippet.LinePragma = new CodeLinePragma(fileName, lineNumber);
                scriptUris[fileName] = uriString;
            }
            typeDecl.Members.Add(scriptSnippet);

            this.endUri = uriString;
            this.endLoc = end;
        }

        public ISourceLineInfo EndLineInfo {
            get {
                return new SourceLineInfo(this.endUri, this.endLoc, this.endLoc);
            }
        }
    }

    internal class Scripts {
        private const string ScriptClassesNamespace = "System.Xml.Xsl.CompiledQuery";

        private Compiler                  compiler;
        private List<ScriptClass>         scriptClasses = new List<ScriptClass>();
        private Dictionary<string, Type>  nsToType      = new Dictionary<string, Type>();
        private XmlExtensionFunctionTable extFuncs      = new XmlExtensionFunctionTable();

        public Scripts(Compiler compiler) {
            this.compiler = compiler;
        }

        public Dictionary<string, Type> ScriptClasses {
            get { return nsToType; }
        }

        public XmlExtensionFunction ResolveFunction(string name, string ns, int numArgs, IErrorHelper errorHelper) {
            Type type;
            if (nsToType.TryGetValue(ns, out type)) {
                try {
                    return extFuncs.Bind(name, ns, numArgs, type, XmlQueryRuntime.EarlyBoundFlags);
                }
                catch (XslTransformException e) {
                    errorHelper.ReportError(e.Message);
                }
            }
            return null;
        }

        public ScriptClass GetScriptClass(string ns, string language, IErrorHelper errorHelper) {
            CompilerInfo compilerInfo;
            try {
                compilerInfo = CodeDomProvider.GetCompilerInfo(language);
                Debug.Assert(compilerInfo != null);
            }
            catch (ConfigurationException) {
                // There is no CodeDom provider defined for this language
                errorHelper.ReportError(/*[XT_010]*/Res.Xslt_ScriptInvalidLanguage, language);
                return null;
            }

            foreach (ScriptClass scriptClass in scriptClasses) {
                if (ns == scriptClass.ns) {
                    // Use object comparison because CompilerInfo.Equals may throw
                    if (compilerInfo != scriptClass.compilerInfo) {
                        errorHelper.ReportError(/*[XT_011]*/Res.Xslt_ScriptMixedLanguages, ns);
                        return null;
                    }
                    return scriptClass;
                }
            }

            ScriptClass newScriptClass = new ScriptClass(ns, compilerInfo);
            newScriptClass.typeDecl.TypeAttributes = TypeAttributes.Public;
            scriptClasses.Add(newScriptClass);
            return newScriptClass;
        }

        //------------------------------------------------
        // Compilation
        //------------------------------------------------

        public void CompileScripts() {
            List<ScriptClass> scriptsForLang = new List<ScriptClass>();

            for (int i = 0; i < scriptClasses.Count; i++) {
                // If the script is already compiled, skip it
                if (scriptClasses[i] == null)
                    continue;

                // Group together scripts with the same CompilerInfo
                CompilerInfo compilerInfo = scriptClasses[i].compilerInfo;
                scriptsForLang.Clear();

                for (int j = i; j < scriptClasses.Count; j++) {
                    // Use object comparison because CompilerInfo.Equals may throw
                    if (scriptClasses[j] != null && scriptClasses[j].compilerInfo == compilerInfo) {
                        scriptsForLang.Add(scriptClasses[j]);
                        scriptClasses[j] = null;
                    }
                }

                Assembly assembly = CompileAssembly(scriptsForLang);

                if (assembly != null) {
                    foreach (ScriptClass script in scriptsForLang) {
                        Type clrType = assembly.GetType(ScriptClassesNamespace + Type.Delimiter + script.typeDecl.Name);
                        if (clrType != null) {
                            nsToType.Add(script.ns, clrType);
                        }
                    }
                }
            }
        }

        // Namespaces we always import when compiling
        private static readonly string[] defaultNamespaces = new string[] {
            "System",
            "System.Collections",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Xml",
            "System.Xml.Xsl",
            "System.Xml.XPath",
        };

        // SxS: This method does not take any resource name and does not expose any resources to the caller.
        // It's OK to suppress the SxS warning.
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        private Assembly CompileAssembly(List<ScriptClass> scriptsForLang) {
            TempFileCollection      allTempFiles  = compiler.CompilerResults.TempFiles;
            CompilerErrorCollection allErrors     = compiler.CompilerResults.Errors;
            ScriptClass             lastScript    = scriptsForLang[scriptsForLang.Count - 1];
            CodeDomProvider         provider;
            bool                    isVB          = false;

            try {
                provider = lastScript.compilerInfo.CreateProvider();
            }
            catch (ConfigurationException e) {
                // The CodeDom provider type could not be located, or some error in machine.config
                allErrors.Add(compiler.CreateError(lastScript.EndLineInfo, /*[XT_041]*/Res.Xslt_ScriptCompileException, e.Message));
                return null;
            }

#if !FEATURE_PAL // visualbasic
            isVB = provider is Microsoft.VisualBasic.VBCodeProvider;
#endif // !FEATURE_PAL

            CodeCompileUnit[] codeUnits = new CodeCompileUnit[scriptsForLang.Count];
            CompilerParameters compilParams = lastScript.compilerInfo.CreateDefaultCompilerParameters();

            // 


            compilParams.ReferencedAssemblies.Add(typeof(System.Xml.Res).Assembly.Location);
            compilParams.ReferencedAssemblies.Add("System.dll");
            if (isVB) {
                compilParams.ReferencedAssemblies.Add("Microsoft.VisualBasic.dll");
            }

            bool refAssembliesByHref = false;

            for (int idx = 0; idx < scriptsForLang.Count; idx++) {
                ScriptClass script = scriptsForLang[idx];
                CodeNamespace scriptNs = new CodeNamespace(ScriptClassesNamespace);

                // Add imported namespaces
                foreach (string ns in defaultNamespaces) {
                    scriptNs.Imports.Add(new CodeNamespaceImport(ns));
                }
                if (isVB) {
                    scriptNs.Imports.Add(new CodeNamespaceImport("Microsoft.VisualBasic"));
                }
                foreach (string ns in script.nsImports) {
                    scriptNs.Imports.Add(new CodeNamespaceImport(ns));
                }

                scriptNs.Types.Add(script.typeDecl);

                CodeCompileUnit unit = new CodeCompileUnit(); {
                    unit.Namespaces.Add(scriptNs);

                    if (isVB) {
                        // This settings have sense for Visual Basic only. In future releases we may allow to specify
                        // them explicitly in the msxsl:script element.
                        unit.UserData["AllowLateBound"]             = true;   // Allow variables to be declared untyped
                        unit.UserData["RequireVariableDeclaration"] = false;  // Allow variables to be undeclared
                    }

                    // Put SecurityTransparentAttribute and SecurityRulesAttribute on the first CodeCompileUnit only
                    if (idx == 0) {
                        unit.AssemblyCustomAttributes.Add(new CodeAttributeDeclaration("System.Security.SecurityTransparentAttribute"));

                        // We want the assemblies generated for scripts to stick to the old security model
                        unit.AssemblyCustomAttributes.Add(
                            new CodeAttributeDeclaration(
                                new CodeTypeReference(typeof(System.Security.SecurityRulesAttribute)),
                                new CodeAttributeArgument(
                                    new CodeFieldReferenceExpression(
                                        new CodeTypeReferenceExpression(typeof(System.Security.SecurityRuleSet)), "Level1"))));
                    }
                }

                codeUnits[idx] = unit;
                foreach (string name in script.refAssemblies) {
                    compilParams.ReferencedAssemblies.Add(name);
                }

                refAssembliesByHref |= script.refAssembliesByHref;
            }

            XsltSettings settings                 = compiler.Settings;
            compilParams.WarningLevel             = settings.WarningLevel >= 0 ? settings.WarningLevel : compilParams.WarningLevel;
            compilParams.TreatWarningsAsErrors    = settings.TreatWarningsAsErrors;
            compilParams.IncludeDebugInformation  = compiler.IsDebug;

            string asmPath = compiler.ScriptAssemblyPath;
            if (asmPath != null && scriptsForLang.Count < scriptClasses.Count) {
                asmPath = Path.ChangeExtension(asmPath, "." + GetLanguageName(lastScript.compilerInfo) + Path.GetExtension(asmPath));
            }
            compilParams.OutputAssembly           = asmPath;

            string tempDir = (settings.TempFiles != null) ? settings.TempFiles.TempDir : null;
            compilParams.TempFiles                = new TempFileCollection(tempDir);

            // We need only .dll and .pdb, but there is no way to specify that
            bool keepFiles = (compiler.IsDebug && asmPath == null);
        #if DEBUG 
            keepFiles = keepFiles || XmlILTrace.IsEnabled;
        #endif
            keepFiles = keepFiles && !settings.CheckOnly;


            compilParams.TempFiles.KeepFiles = keepFiles;

            // If GenerateInMemory == true, then CodeDom loads the compiled assembly using Assembly.Load(byte[])
            // instead of Assembly.Load(AssemblyName).  That means the assembly will be loaded in the anonymous
            // context (http://blogs.msdn.com/[....]/archive/2003/05/29/57143.aspx), and its dependencies can only
            // be loaded from the Load context or using AssemblyResolve event.  However we want to use the LoadFrom
            // context to preload all dependencies specified by <ms:assembly href="uri-reference"/>, so we turn off
            // GenerateInMemory here.
            compilParams.GenerateInMemory = (asmPath == null && !compiler.IsDebug && !refAssembliesByHref) || settings.CheckOnly;

            CompilerResults results;

            try {
                results = provider.CompileAssemblyFromDom(compilParams, codeUnits);
            }
            catch (ExternalException e) {
                // Compiler might have created temporary files
                results = new CompilerResults(compilParams.TempFiles);
                results.Errors.Add(compiler.CreateError(lastScript.EndLineInfo, /*[XT_041]*/Res.Xslt_ScriptCompileException, e.Message));
            }

            if (!settings.CheckOnly) {
                foreach (string fileName in results.TempFiles) {
                    allTempFiles.AddFile(fileName, allTempFiles.KeepFiles);
                }
            }

            foreach (CompilerError error in results.Errors) {
                FixErrorPosition(error, scriptsForLang);
                compiler.AddModule(error.FileName);
            }

            allErrors.AddRange(results.Errors);
            return results.Errors.HasErrors ? null : results.CompiledAssembly;
        }

        private int assemblyCounter = 0;

        private string GetLanguageName(CompilerInfo compilerInfo) {
            Regex alphaNumeric = new Regex("^[0-9a-zA-Z]+$"); 
            foreach (string name in compilerInfo.GetLanguages()) {
                if (alphaNumeric.IsMatch(name))
                    return name;
            }
            return "script" + (++assemblyCounter).ToString(CultureInfo.InvariantCulture);
        }

        // The position of a compile error may be outside of all user code snippets (for example, in case of
        // unclosed '{'). In that case filename would be the name of the temporary file, and not the name
        // of the stylesheet file. Exposing the path of the temporary file is considered to be a security issue,
        // so here we check that filename is amongst user files.
        private static void FixErrorPosition(CompilerError error, List<ScriptClass> scriptsForLang) {
            string fileName = error.FileName;
            string uri;

            foreach (ScriptClass script in scriptsForLang) {
                // We assume that CodeDom provider returns absolute paths (VSWhidbey 289665).
                // Note that casing may be different.
                if (script.scriptUris.TryGetValue(fileName, out uri)) {
                    // The error position is within one of user stylesheets, its URI may be reported
                    error.FileName = uri;
                    return;
                }
            }

            // Error is outside user code snippeets, we should hide filename for security reasons.
            // Return filename and position of the end of the last script block for the given class.
            int idx, scriptNumber;
            ScriptClass errScript = scriptsForLang[scriptsForLang.Count - 1];

            // Normally temporary source files are named according to the scheme "<random name>.<script number>.
            // <language extension>". Try to extract the middle part to find the relevant script class. In case
            // of a non-standard CodeDomProvider, use the last script class.
            fileName = Path.GetFileNameWithoutExtension(fileName);
            if ((idx = fileName.LastIndexOf('.')) >= 0)
                if (int.TryParse(fileName.Substring(idx + 1), NumberStyles.None, NumberFormatInfo.InvariantInfo, out scriptNumber))
                    if ((uint)scriptNumber < scriptsForLang.Count) {
                        errScript = scriptsForLang[scriptNumber];
                    }

            error.FileName  = errScript.endUri;
            error.Line      = errScript.endLoc.Line;
            error.Column    = errScript.endLoc.Pos;
        }
    }
}
