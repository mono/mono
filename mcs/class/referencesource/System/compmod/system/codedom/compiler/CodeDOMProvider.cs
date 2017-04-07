//------------------------------------------------------------------------------
// <copyright file="CodeDOMProvider.cs" company="Microsoft">
// 
// <OWNER>Microsoft</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.IO;
    using System.Security.Permissions;
    using System.Configuration;
    using System.Collections;
    using System.Runtime.InteropServices;

    [
    ToolboxItem(false)
    ]
    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    [ComVisible(true)]
    public abstract class CodeDomProvider : Component {

        [ComVisible(false)]
        public static CodeDomProvider CreateProvider(String language, System.Collections.Generic.IDictionary<String, String> providerOptions) {
            CompilerInfo compilerInfo = GetCompilerInfo(language);
            return compilerInfo.CreateProvider(providerOptions);
        }

        [ComVisible(false)]
        public static CodeDomProvider CreateProvider(String language) {
            CompilerInfo compilerInfo = GetCompilerInfo(language);
            return compilerInfo.CreateProvider();
        }

        [ComVisible(false)]
        public static String GetLanguageFromExtension(String extension) {
            CompilerInfo compilerInfo = (CompilerInfo)GetCompilerInfoForExtensionNoThrow(extension);
            if (compilerInfo == null)
                throw new ConfigurationErrorsException(SR.GetString(SR.CodeDomProvider_NotDefined));
            return compilerInfo._compilerLanguages[0]; // Return the first language name. There has to be one.
        }

        [ComVisible(false)]
        public static bool IsDefinedLanguage(String language) {
            return (GetCompilerInfoForLanguageNoThrow(language) != null);
        }

        [ComVisible(false)]
        public static bool IsDefinedExtension(String extension) {
            return (GetCompilerInfoForExtensionNoThrow(extension) != null);
        }


        [ComVisible(false)]
        public static CompilerInfo GetCompilerInfo(String language) {
            CompilerInfo compilerInfo = GetCompilerInfoForLanguageNoThrow(language);
            if (compilerInfo == null)
                throw new ConfigurationErrorsException(SR.GetString(SR.CodeDomProvider_NotDefined));
            return compilerInfo;
        }


        // Do argument validation but don't throw if there's no compiler defined for a language.
        private static CompilerInfo GetCompilerInfoForLanguageNoThrow(String language) {
            if (language == null)
                throw new ArgumentNullException("language");

            CompilerInfo compilerInfo = (CompilerInfo)Config._compilerLanguages[language.Trim()];
            return compilerInfo;
        }

        // Do argument validation but don't throw if there's no compiler defined for a language.
        private static CompilerInfo GetCompilerInfoForExtensionNoThrow(String extension) {
            if (extension == null)
                throw new ArgumentNullException("extension");

            CompilerInfo compilerInfo = (CompilerInfo)Config._compilerExtensions[extension.Trim()];
            return compilerInfo;
        }

        [ComVisible(false)]
        public static CompilerInfo[] GetAllCompilerInfo() {
            ArrayList values = Config._allCompilerInfo;
            CompilerInfo[] compilerInfos = (CompilerInfo[])values.ToArray(typeof(CompilerInfo));
            return compilerInfos;
        }

        // Don't cache the configuration since things are different for asp.net scenarios.
        private static CodeDomCompilationConfiguration Config {
            get {
                CodeDomCompilationConfiguration _configuration= (CodeDomCompilationConfiguration)PrivilegedConfigurationManager.GetSection(CodeDomCompilationConfiguration.sectionName);
                if (_configuration == null) {
                    return CodeDomCompilationConfiguration.Default;
                }
                return _configuration;
            }
        }
    
        /// <devdoc>
        ///    <para>Retrieves the default extension to use when saving files using this code dom provider.</para>
        /// </devdoc>
        public virtual string FileExtension {
            get {
                return string.Empty;
            }
        }

        /// <devdoc>
        ///    <para>Returns flags representing language variations.</para>
        /// </devdoc>
        public virtual LanguageOptions LanguageOptions {
            get {
                return LanguageOptions.None;
            }
        }
        
        [Obsolete("Callers should not use the ICodeGenerator interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
        public abstract ICodeGenerator CreateGenerator();

#pragma warning disable 618
        public virtual ICodeGenerator CreateGenerator(TextWriter output) {
            return CreateGenerator();
        }

        public virtual ICodeGenerator CreateGenerator(string fileName) {
            return CreateGenerator();
        }
#pragma warning restore 618

        [Obsolete("Callers should not use the ICodeCompiler interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
        public abstract ICodeCompiler CreateCompiler();
        
        [Obsolete("Callers should not use the ICodeParser interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
        public virtual ICodeParser CreateParser() {
            return null;
        }
        
        /// <devdoc>
        ///     This method allows a code dom provider implementation to provide a different type converter
        ///     for a given data type.  At design time, a designer may pass data types through this
        ///     method to see if the code dom provider wants to provide an additional converter.  
        ///     As typical way this would be used is if the language this code dom provider implements
        ///     does not support all of the values of MemberAttributes enumeration, or if the language
        ///     uses different names (Protected instead of Family, for example).  The default 
        ///     implementation just calls TypeDescriptor.GetConverter for the given type.
        /// </devdoc>
        public virtual TypeConverter GetConverter(Type type) {
            return TypeDescriptor.GetConverter(type);
        }
        
        /// <devdoc>
        ///    <para>
        ///       Creates an assembly based on options, with the information from the compile units
        ///    </para>
        /// </devdoc>
        public virtual CompilerResults CompileAssemblyFromDom(CompilerParameters options, params CodeCompileUnit[] compilationUnits) {
            return CreateCompilerHelper().CompileAssemblyFromDomBatch(options, compilationUnits);
        }

        /// <devdoc>
        ///    <para>
        ///       Creates an assembly based on options, with the contents of the files
        ///    </para>
        /// </devdoc>
        public virtual CompilerResults CompileAssemblyFromFile(CompilerParameters options, params string[] fileNames) {
            return CreateCompilerHelper().CompileAssemblyFromFileBatch(options, fileNames);
        }

        /// <devdoc>
        ///    <para>
        ///       Creates an assembly based on options, with the information from sources
        ///    </para>
        /// </devdoc>
        public virtual CompilerResults CompileAssemblyFromSource(CompilerParameters options, params string[] sources) {
            return CreateCompilerHelper().CompileAssemblyFromSourceBatch(options, sources);
        }
                
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether
        ///       the specified value is a valid identifier for this language.
        ///    </para>
        /// </devdoc>
        public virtual bool IsValidIdentifier(string value) {
            return CreateGeneratorHelper().IsValidIdentifier(value);
        }

        public virtual string CreateEscapedIdentifier(string value) {
            return CreateGeneratorHelper().CreateEscapedIdentifier(value);
        }

        public virtual string CreateValidIdentifier(string value) {
            return CreateGeneratorHelper().CreateValidIdentifier(value);            
        }

        public virtual string GetTypeOutput(CodeTypeReference type) {
            return CreateGeneratorHelper().GetTypeOutput(type);
        }

        public virtual bool Supports(GeneratorSupport generatorSupport) {
            return CreateGeneratorHelper().Supports(generatorSupport);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code from the specified expression and
        ///       outputs it to the specified textwriter.
        ///    </para>
        /// </devdoc>
        public virtual void GenerateCodeFromExpression(CodeExpression expression, TextWriter writer, CodeGeneratorOptions options) {
            CreateGeneratorHelper().GenerateCodeFromExpression(expression, writer, options);
        }

        /// <devdoc>
        ///    <para>
        ///       Outputs the language specific representaion of the CodeDom tree
        ///       refered to by statement, into writer.
        ///    </para>
        /// </devdoc>
        public virtual void GenerateCodeFromStatement(CodeStatement statement, TextWriter writer, CodeGeneratorOptions options) {
            CreateGeneratorHelper().GenerateCodeFromStatement(statement, writer, options);
        }

        /// <devdoc>
        ///    <para>
        ///       Outputs the language specific representaion of the CodeDom tree
        ///       refered to by codeNamespace, into writer.
        ///    </para>
        /// </devdoc>
        public virtual void GenerateCodeFromNamespace(CodeNamespace codeNamespace, TextWriter writer, CodeGeneratorOptions options) {
            CreateGeneratorHelper().GenerateCodeFromNamespace(codeNamespace, writer, options);
        }

        /// <devdoc>
        ///    <para>
        ///       Outputs the language specific representaion of the CodeDom tree
        ///       refered to by compileUnit, into writer.
        ///    </para>
        /// </devdoc>
        public virtual void GenerateCodeFromCompileUnit(CodeCompileUnit compileUnit, TextWriter writer, CodeGeneratorOptions options) {
            CreateGeneratorHelper().GenerateCodeFromCompileUnit(compileUnit, writer, options);
        }

        /// <devdoc>
        ///    <para>
        ///       Outputs the language specific representaion of the CodeDom tree
        ///       refered to by codeType, into writer.
        ///    </para>
        /// </devdoc>
        public virtual void GenerateCodeFromType(CodeTypeDeclaration codeType, TextWriter writer, CodeGeneratorOptions options) {
            CreateGeneratorHelper().GenerateCodeFromType(codeType, writer, options);
        }

        public virtual void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options) {
            throw new NotImplementedException(SR.GetString(SR.NotSupported_CodeDomAPI));
        }
        
        /// <devdoc>
        ///    <para>
        ///       Compiles the given text stream into a CodeCompile unit.  
        ///    </para>
        /// </devdoc>
        public virtual CodeCompileUnit Parse(TextReader codeStream) {
            return CreateParserHelper().Parse(codeStream);
        }
        
#pragma warning disable 618
        private ICodeCompiler CreateCompilerHelper() {
            ICodeCompiler compiler = CreateCompiler();
            if (compiler == null) {
                throw new NotImplementedException(SR.GetString(SR.NotSupported_CodeDomAPI));
            }
            return compiler;
        }

        private ICodeGenerator CreateGeneratorHelper() {
            ICodeGenerator generator = CreateGenerator();
            if (generator == null) {
                throw new NotImplementedException(SR.GetString(SR.NotSupported_CodeDomAPI));
            }
            return generator;
        }
        
        private ICodeParser CreateParserHelper() {
            ICodeParser parser = CreateParser();
            if (parser == null) {
                throw new NotImplementedException(SR.GetString(SR.NotSupported_CodeDomAPI));
            }
            return parser;
        }        
#pragma warning restore 618

        /// <summary>
        /// This method returns a reference to mscorlib.dll or System.Runtime.dll that coresponds to
        /// the version of the framework referenced by ReferencedAssemblies property if it appears
        /// that these references point to a multi-targeting pack.  VBCodeProvider and CSharpCodeProvider
        /// use this method to provide a value for CoreAssemblyFileName if it was not set so the default
        /// mscorlib.dll is not used in cases where it looks like the developer intended to do multi-targeting.
        /// 
        /// The huristic here is as follows:
        /// If there is a reference that contains "\Reference Assemblies\Microsoft\Framework\<SkuName>\v<Version>"
        /// and for each reference of the above form, they all share the same set of directories starting with
        /// Reference Assemblies, then the probable core assembly is mscorlib.dll in that directory.
        /// Otherwise, we do not have a probable core assembly.
        /// 
        /// Note that we do no validation to ensure SkuName or Version are actually valid sku names or versions.
        /// The version component must start with a v but otherwise can be in any arbitrary form.
        /// </summary>
        internal static bool TryGetProbableCoreAssemblyFilePath(CompilerParameters parameters, out string coreAssemblyFilePath) {
            string multiTargetingPackRoot = null;
            char[] pathSeperators = new char[] { Path.DirectorySeparatorChar };

            // Valid paths look like "...\Reference Assemblies\Microsoft\Framework\<SkuName>\v<Version>\..."
            string referenceAssemblyFolderPrefix = Path.Combine("Reference Assemblies", "Microsoft", "Framework");
         
            foreach (string s in parameters.ReferencedAssemblies) {

                if (Path.GetFileName(s).Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase)) {
                    // They already have their own mscorlib.dll, so let's not add another one.
                    coreAssemblyFilePath = string.Empty;
                    return false;
                }

                if (s.IndexOf(referenceAssemblyFolderPrefix, StringComparison.OrdinalIgnoreCase) >= 0) {

                    String[] dirs = s.Split(pathSeperators, StringSplitOptions.RemoveEmptyEntries);
                    
                    for (int i = 0; i < dirs.Length - 5; i++) {
                    
                        if (String.Equals(dirs[i], "Reference Assemblies", StringComparison.OrdinalIgnoreCase)) {
                            // Here i+5 is the index of the thing after the vX.XXX folder (if one exists) and i+4 should look like a version.
                            // (i.e. start with a v).
                            if (dirs[i + 4].StartsWith("v", StringComparison.OrdinalIgnoreCase)) {                            
                                if (multiTargetingPackRoot != null) {
                                    if (!String.Equals(multiTargetingPackRoot, Path.GetDirectoryName(s), StringComparison.OrdinalIgnoreCase)) {
                                        // We found one reference to one targeting pack and one referece to another.  Bail out.
                                        coreAssemblyFilePath = string.Empty;
                                        return false;
                                    }
                                } else {
                                    multiTargetingPackRoot = Path.GetDirectoryName(s);
                                }
                            } 
                        }
                    }
                }
            }

            if (multiTargetingPackRoot != null) {
                coreAssemblyFilePath = Path.Combine(multiTargetingPackRoot, "mscorlib.dll");
                return true;
            }

            coreAssemblyFilePath = string.Empty;
            return false;
        }
    }
}
