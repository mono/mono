//------------------------------------------------------------------------------
// <copyright file="AssemblyBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Security.Cryptography;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;
    using System.Xml.Schema;

/*
 * This class is used to handle a single compilation using a CodeDom compiler.
 * It is instantiated via CompilerType.CreateAssemblyBuilder.
 */
public class AssemblyBuilder {

    private CompilationSection _compConfig;

    // CodeChecksumPragma.ChecksumAlgorithmId takes this GUID to represent a SHA1 hash of the file contents
    // See: http://msdn.microsoft.com/en-us/library/system.codedom.codechecksumpragma.checksumalgorithmid.aspx
    private static readonly Guid s_codeChecksumSha1Id = new Guid(0xff1816ec, 0xaa5e, 0x4d10, 0x87, 0xf7, 0x6f, 0x49, 0x63, 0x83, 0x34, 0x60);

    // List of BuildProviders involved in this compilation
    // The key is either a virtual path, or the BuildProvider itself if
    // it doesn't give us a virtual path.
    // The value is the BuildProvider.
    private Hashtable _buildProviders = new Hashtable(StringComparer.OrdinalIgnoreCase);
        
    internal ICollection BuildProviders {
        get { return _buildProviders.Values; }
    }

    // List of physical source files to be compiled
    private StringSet _sourceFiles = new StringSet();

    // CodeCompileUnit to hold various top level things we need to generate
    private CodeCompileUnit _miscCodeCompileUnit;

    // List of physical embedded resource files to be compiled
    private StringSet _embeddedResourceFiles;

    // The set of assemblies that we will be linked with
    private AssemblySet _initialReferencedAssemblies;

    // The additional set of assemblies that we will be linked with, and that are
    // requested by various BuildProviders.  We need to keep them separate to avoid
    // having BuildProviders see assemblies that were requested by earlier providers,
    // which would lead to unpredictable behavior (since the order is arbitrary)
    private AssemblySet _additionalReferencedAssemblies;

    internal CodeDomProvider _codeProvider;

    private Hashtable _buildProviderToSourceFileMap;

    // The type of CodeDom compiler (i.e. language, flags)
    private CompilerType _compilerType;

    internal Type CodeDomProviderType {
        get { return _compilerType.CodeDomProviderType; }
    }

    // Used to generate fast Type factories
    private ObjectFactoryCodeDomTreeGenerator _objectFactoryGenerator;

    private StringResourceBuilder _stringResourceBuilder;
    internal StringResourceBuilder StringResourceBuilder {
        get {
            if (_stringResourceBuilder == null)
                _stringResourceBuilder = new StringResourceBuilder();

            return _stringResourceBuilder;
        }
    }

    // Used to create temporary source files
    private TempFileCollection _tempFiles = new TempFileCollection(HttpRuntime.CodegenDirInternal);
    private int _fileCount;

    private string _cultureName;
    internal string CultureName {
        get { return _cultureName; }
        set { _cultureName = value; }
    }

    private string _outputAssemblyName;
    private string OutputAssemblyName {
        get {
            if (_outputAssemblyName == null) {
                // If we don't have the assembly name, we should never have a culture
                Debug.Assert(CultureName == null);

                // If the assembly name was not specified, use a generated one based on the TempFileCollection.
                // But prefix it with a fixed token, to make it easier to recognize the assembly (DevDiv 36625)
                string basePath = _tempFiles.BasePath;
                string baseFileName = Path.GetFileName(basePath);
                _outputAssemblyName = BuildManager.WebAssemblyNamePrefix + baseFileName;
            }

            return _outputAssemblyName; 
        }
    }

    private int _maxBatchSize;
    private long _maxBatchGeneratedFileSize;
    private long _totalFileLength;

    private CaseInsensitiveStringSet _registeredTypeNames;
    internal bool ContainsTypeNames(ICollection typeNames) {
        if (_registeredTypeNames != null && typeNames != null) {
            foreach (String typeName in typeNames) {
                if (_registeredTypeNames.Contains(typeName)) {
                    return true;
                }
            }
        }

        return false;
    }

    internal void AddTypeNames(ICollection typeNames) {
        if (typeNames == null) {
            return;
        }

        if (_registeredTypeNames == null) {
            _registeredTypeNames = new CaseInsensitiveStringSet();
        }

        _registeredTypeNames.AddCollection(typeNames);
    }

    internal AssemblyBuilder(CompilationSection compConfig,
        ICollection referencedAssemblies, CompilerType compilerType, string outputAssemblyName) {

        _compConfig = compConfig;

        _outputAssemblyName = outputAssemblyName;

        // Clone the referenced assemblies
        _initialReferencedAssemblies = AssemblySet.Create(referencedAssemblies);

        // We need to clone it to avoid modifying the original (VSWhidbey 338935)
        _compilerType = compilerType.Clone();

        if (BuildManager.PrecompilingWithDebugInfo) {
            // If the precompile flag indicates force debug, always compile as debug
            _compilerType.CompilerParameters.IncludeDebugInformation = true;
        }
        else if (BuildManager.PrecompilingForDeployment) {
            // If we're precompiling the app, never compile in debug mode (VSWhidbey 178377)
            _compilerType.CompilerParameters.IncludeDebugInformation = false;
        }
        else if (DeploymentSection.RetailInternal) {
            // If we're in retail deployment mode, always turn off debug (DevDiv 36396)
            _compilerType.CompilerParameters.IncludeDebugInformation = false;
        }
        else if (_compConfig.AssemblyPostProcessorTypeInternal != null) {
            // If an IAssemblyPostProcessor is registered always compile as debug
            _compilerType.CompilerParameters.IncludeDebugInformation = true;
        }

        // 
        _tempFiles.KeepFiles = _compilerType.CompilerParameters.IncludeDebugInformation;

        _codeProvider = CompilationUtil.CreateCodeDomProviderNonPublic(
            _compilerType.CodeDomProviderType);

        _maxBatchSize = _compConfig.MaxBatchSize;
        _maxBatchGeneratedFileSize = _compConfig.MaxBatchGeneratedFileSize * 1024;
    }

    // Beginning of public contract


    /// <devdoc>
    ///     Adds an assembly that will be referenced during compilation.
    /// </devdoc>
    public void AddAssemblyReference(Assembly a) {
        if (_additionalReferencedAssemblies == null)
            _additionalReferencedAssemblies = new AssemblySet();
        _additionalReferencedAssemblies.Add(a);
    }

    /// <devdoc>
    /// Adds an assembly that will be referenced during compilation. Also adds the
    /// assembly the the ReferencedAssemblies list in the CodeCompileUnit.
    /// </devdoc>
    internal void AddAssemblyReference(Assembly a, CodeCompileUnit ccu) {
        AddAssemblyReference(a);
        Util.AddAssemblyToStringCollection(a, ccu.ReferencedAssemblies);
    }

    /// <devdoc>
    ///     Creates a new source file that will be added to the compilation. See the public overload
    ///     method for detail.
    /// </devdoc>
    internal virtual TextWriter CreateCodeFile(BuildProvider buildProvider, out string filename) {

        string generatedFilePath = GetTempFilePhysicalPathWithAssert(_codeProvider.FileExtension);
        filename = generatedFilePath;

        if (buildProvider != null) {
            if (_buildProviderToSourceFileMap == null)
                _buildProviderToSourceFileMap = new Hashtable();
            _buildProviderToSourceFileMap[buildProvider] = generatedFilePath;
            buildProvider.SetContributedCode();
        }

        _sourceFiles.Add(generatedFilePath);

        return CreateCodeFileWithAssert(generatedFilePath);
    }

    [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
    private StreamWriter CreateCodeFileWithAssert(string generatedFilePath) {
        Stream temp = new FileStream(generatedFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        return new StreamWriter(temp, Encoding.UTF8);
    }


    /// <devdoc>
    ///     Creates a new source file that will be added to the compilation.  The build provider
    ///     can write source code to this file using the returned TextWriter.
    ///     The build provider should close the TextWriter when it is done writing to it.
    ///     The build provider should pass itself as a parameter to this method.
    /// </devdoc>
    public TextWriter CreateCodeFile(BuildProvider buildProvider) {
        // Ignore the unused filename param.
        string filename;

        return CreateCodeFile(buildProvider, out filename);
    }


    // Indicates whether the assemblyBuilder has reached its capacity limit.
    internal bool IsBatchFull {
        get {
            return (_sourceFiles.Count >= _maxBatchSize) ||
                (_totalFileLength >= _maxBatchGeneratedFileSize);
        }
    }


    /// <devdoc>
    ///     Adds a CodeCompileUnit to the compilation.  This is typically used as an
    ///     alternative to CreateSourceFile, by providers who are CodeDOM aware.
    ///     The build provider should pass itself as a parameter to this method.
    /// </devdoc>
    public void AddCodeCompileUnit(BuildProvider buildProvider, CodeCompileUnit compileUnit) {

        // Add a checksum pragma to the compile unit if appropriate
        AddChecksumPragma(buildProvider, compileUnit);

        // Add all the referenced assemblies to the CodeCompileUnit in case the CodeDom
        // provider needs them for code generation
        Util.AddAssembliesToStringCollection(_initialReferencedAssemblies, compileUnit.ReferencedAssemblies);

        // Merge the _additionalReferencedAssemblies from individul build providers
        Util.AddAssembliesToStringCollection(_additionalReferencedAssemblies, compileUnit.ReferencedAssemblies);

        String filename;

        // Revert impersonation when generating source code in the codegen dir (VSWhidbey 176576)
        using (new ProcessImpersonationContext()) {
            TextWriter writer = CreateCodeFile(buildProvider, out filename);

            try {
                _codeProvider.GenerateCodeFromCompileUnit(compileUnit, writer, null /*CodeGeneratorOptions*/);
            }
            finally {
                writer.Flush();
                writer.Close();
            }
        }

        if (filename != null) {
            _totalFileLength += GetFileLengthWithAssert(filename);
        }
    }

    // Assert to be able to get the length of the file in the CodeGen dir
    [FileIOPermission(SecurityAction.Assert, AllFiles = FileIOPermissionAccess.Read)]
    private long GetFileLengthWithAssert(string filename) {
        FileInfo info = new FileInfo(filename);
        return info.Length;
    }

    /// <devdoc>
    ///     Tell the host about a type that is being generated.  This allows the host
    ///     To generate a fast object factory for it.
    /// </devdoc>
    public void GenerateTypeFactory(string typeName) {

        // Create the object factory generator on demand
        if (_objectFactoryGenerator == null) {
            _objectFactoryGenerator = new ObjectFactoryCodeDomTreeGenerator(OutputAssemblyName);
        }

        // Add a method to fast create this type
        _objectFactoryGenerator.AddFactoryMethod(typeName);
    }

    /// <devdoc>
    ///     Creates a new resource that will be added to the compilation.  The build provider
    ///     can write to it using the returned Stream.
    ///     The build provider should close the Stream when it is done writing to it.
    ///     The build provider should pass itself as a parameter to this method.
    /// </devdoc>
    public Stream CreateEmbeddedResource(BuildProvider buildProvider, string name) {

        // Make sure it's just a valid simple file name
        if (!Util.IsValidFileName(name)) {
            throw new ArgumentException(null, name);
        }

        string resourceDir = BuildManager.CodegenResourceDir;
        string resourceFile = Path.Combine(resourceDir, name);
        CreateTempResourceDirectoryIfNecessary();

        _tempFiles.AddFile(resourceFile, _tempFiles.KeepFiles);

        if (_embeddedResourceFiles == null)
            _embeddedResourceFiles = new StringSet();

        _embeddedResourceFiles.Add(resourceFile);

        // Assert to be able to create the file in the temp dir
        InternalSecurityPermissions.FileWriteAccess(resourceDir).Assert();

        return File.OpenWrite(resourceFile);
    }

    [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
    private void CreateTempResourceDirectoryIfNecessary() {
        // Create the temp resource directory if needed
        string resourceDir = BuildManager.CodegenResourceDir;
        if (!FileUtil.DirectoryExists(resourceDir)) {
            Directory.CreateDirectory(resourceDir);
        }
    }

    /// <devdoc>
    ///     Returns a CodeDomProvider that the build provider can use to generate a CodeCompileUnit.
    /// </devdoc>
    public CodeDomProvider CodeDomProvider { 
        get { return _codeProvider; }
    }

    private string _tempFilePhysicalPathPrefix;
    private string TempFilePhysicalPathPrefix {
        get {
            if (_tempFilePhysicalPathPrefix == null) {
                _tempFilePhysicalPathPrefix = Path.Combine(_tempFiles.TempDir, OutputAssemblyName) + ".";

                // Append the culture name to avoid naming conflicts
                if (CultureName != null) {
                    _tempFilePhysicalPathPrefix += CultureName + "_";
                }
            }

            return _tempFilePhysicalPathPrefix;
        }
    }

    /// <devdoc>
    ///     Returns the physical path to a temporary file that the build provider
    ///     can use for intermediate results.  Note that the file is not actually
    ///     created.  It is up to the build provider to do this.
    ///     The temp file's extension is passed in by the build provider.
    ///     The file is automatically deleted after the compilation, so the
    ///     build provider does not need to explicitly delete it.
    /// </devdoc>
    public string GetTempFilePhysicalPath(string extension) {

        // Do the right thing depending on whether the extension include the starting '.'
        string tempPath;
        if (!String.IsNullOrEmpty(extension) && extension[0] == '.') {
            tempPath = TempFilePhysicalPathPrefix + ((_fileCount++) + extension);
        }
        else {
            tempPath = TempFilePhysicalPathPrefix + ((_fileCount++) + "." + extension);
        }

        _tempFiles.AddFile(tempPath, _tempFiles.KeepFiles);

        InternalSecurityPermissions.PathDiscovery(tempPath).Demand();

        return tempPath;
    }

    // End of public contract

    [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
    internal string GetTempFilePhysicalPathWithAssert(string extension) {
        return GetTempFilePhysicalPath(extension);
    }

    private void AddCompileWithBuildProvider(VirtualPath virtualPath, BuildProvider owningBuildProvider) {

        BuildProvider buildProvider = BuildManager.CreateBuildProvider(virtualPath,
            _compConfig, _initialReferencedAssemblies, true /*failIfUnknown*/);

        // Since it's referenced via compileWith, it doesn't need its own build result
        buildProvider.SetNoBuildResult();

        // If it's a CompileWith provider, remember the main provider
        SourceFileBuildProvider sourceBuildProvider = buildProvider as SourceFileBuildProvider;
        if (sourceBuildProvider != null)
            sourceBuildProvider.OwningBuildProvider = owningBuildProvider;

        AddBuildProvider(buildProvider);
    }

    internal virtual void AddBuildProvider(BuildProvider buildProvider) {

        // By default, use the build provider itself as the key
        object hashtableKey = buildProvider;

        bool isFolderLevel = false;

        // If the buildProvider is a folderLevel build provider, use the build provider itself
        // so that multiple build providers can work on the same path.
        if (_compConfig.FolderLevelBuildProviders != null) {
            Type t = buildProvider.GetType();
            isFolderLevel = _compConfig.FolderLevelBuildProviders.IsFolderLevelBuildProvider(t);
        }

        // Keep track of the build provider's virtual path, if any
        if (buildProvider.VirtualPath != null && !isFolderLevel) {

            // It has a virtual path, so use that as the key
            hashtableKey = buildProvider.VirtualPath;

            // If we already had it, ignore it.  This can happen when there is a user control
            // with a code beside in App_Code (VSWhidbey 481426)
            if (_buildProviders.ContainsKey(hashtableKey))
                return;
        }

        _buildProviders[hashtableKey] = buildProvider;

        // Ask the provider to generate the code
        // If it throws an Xml exception, extra the relevant info and turn it
        // into our own ParseException
        try {
            buildProvider.GenerateCode(this);
        }
        catch (XmlException e) {
            throw new HttpParseException(e.Message, null /*innerException*/,
                buildProvider.VirtualPath, null /*sourceCode*/, e.LineNumber);
        }
        catch (XmlSchemaException e) {
            throw new HttpParseException(e.Message, null /*innerException*/,
                buildProvider.VirtualPath, null /*sourceCode*/, e.LineNumber);
        }
        catch (Exception e) {
            throw new HttpParseException(e.Message, e,
                buildProvider.VirtualPath, null /*sourceCode*/, 1);
        }

        // Handle any 'compileWith' dependencies, i.e. files that must be compiled
        // within the same assembly as the current file
        InternalBuildProvider internalBuildProvider = buildProvider as InternalBuildProvider;
        if (internalBuildProvider != null) {
            ICollection compileWith = internalBuildProvider.GetCompileWithDependencies();
            if (compileWith != null) {
                foreach (VirtualPath virtualPath in compileWith) {

                    // If we already have it, ignore it
                    if (_buildProviders.ContainsKey(virtualPath.VirtualPathString))
                        continue;

                    // Add the compileWith dependency to our compilation
                    AddCompileWithBuildProvider(virtualPath, internalBuildProvider);
                }
            }
        }

    }

    private void AddAssemblyCultureAttribute() {

        if (CultureName == null) return;

        CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(
            new CodeTypeReference(typeof(System.Reflection.AssemblyCultureAttribute)),
            new CodeAttributeArgument[] {
                    new CodeAttributeArgument(new CodePrimitiveExpression(CultureName))});

        AddAssemblyAttribute(declaration);
    }


    private void AddAspNetGeneratedCodeAttribute() {

        CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(
            new CodeTypeReference(typeof(GeneratedCodeAttribute)));
        declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression("ASP.NET")));
        declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(VersionInfo.SystemWebVersion)));

        AddAssemblyAttribute(declaration);
    }

    private void AddAllowPartiallyTrustedCallersAttribute() {
        if (BuildManager.CompileWithAllowPartiallyTrustedCallersAttribute) {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(
                new CodeTypeReference(typeof(AllowPartiallyTrustedCallersAttribute)));

            AddAssemblyAttribute(declaration);
        }
    }

    private void AddAssemblyKeyFileAttribute() {
        if (!String.IsNullOrEmpty(BuildManager.StrongNameKeyFile)) {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(
                new CodeTypeReference(typeof(AssemblyKeyFileAttribute)), 
                new CodeAttributeArgument(new CodePrimitiveExpression(BuildManager.StrongNameKeyFile)));

            AddAssemblyAttribute(declaration);
        }
    }

    private void AddAssemblyKeyContainerAttribute() {
        if (!String.IsNullOrEmpty(BuildManager.StrongNameKeyContainer)) {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(
                new CodeTypeReference(typeof(AssemblyKeyNameAttribute)),
                new CodeAttributeArgument(new CodePrimitiveExpression(BuildManager.StrongNameKeyContainer)));

            AddAssemblyAttribute(declaration);
        }
    }

    private void AddAssemblyDelaySignAttribute() {
        if (BuildManager.CompileWithDelaySignAttribute) {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(
                new CodeTypeReference(typeof(AssemblyDelaySignAttribute)),
                new CodeAttributeArgument(new CodePrimitiveExpression(true)));

            AddAssemblyAttribute(declaration);
        }
    }

    private void AddSecurityRulesAttribute() {
        // Skip applying the attribute if targeting 2.0/3.5, since the attribute
        // is only available in 4.0 and above.
        if (MultiTargetingUtil.IsTargetFramework20 || MultiTargetingUtil.IsTargetFramework35) {
            return;
        }

        TrustSection trustSection = RuntimeConfig.GetAppConfig().Trust;
        CodeAttributeDeclaration declaration;
        Type attrType = typeof(SecurityRulesAttribute);
        Type enumType = typeof(SecurityRuleSet);
        if (trustSection.LegacyCasModel) {
            SecurityRuleSet set = SecurityRuleSet.Level1;
            string fieldName = Enum.GetName(enumType, set);
            CodeFieldReferenceExpression field = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(enumType), fieldName);
            declaration = new CodeAttributeDeclaration(new CodeTypeReference(attrType), new CodeAttributeArgument(field));
            AddAssemblyAttribute(declaration);
        }
        else {
            SecurityRuleSet set = SecurityRuleSet.Level2;
            string fieldName = Enum.GetName(enumType, set);
            CodeFieldReferenceExpression field = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(enumType), fieldName);
            declaration = new CodeAttributeDeclaration(new CodeTypeReference(attrType), new CodeAttributeArgument(field));
            AddAssemblyAttribute(declaration);
        }
    }
    
    private void AddTargetFrameworkAttribute() {
        if (MultiTargetingUtil.TargetFrameworkVersion.Major >= 4) {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(
                new CodeTypeReference(typeof(System.Runtime.Versioning.TargetFrameworkAttribute)),
                new CodeAttributeArgument(new CodePrimitiveExpression(BuildManager.TargetFramework.FullName)));

            AddAssemblyAttribute(declaration);
        }
    }

    // Add an assembly level attribute to the assembly
    private void AddAssemblyAttribute(CodeAttributeDeclaration declaration) {

        if (_miscCodeCompileUnit == null)
            _miscCodeCompileUnit = new CodeCompileUnit();

        _miscCodeCompileUnit.AssemblyCustomAttributes.Add(declaration);
    }

    private void GenerateMiscCodeCompileUnit() {

        // If there aren't any, return
        if (_miscCodeCompileUnit == null)
            return;

        AddCodeCompileUnit(null /*buildProvider*/, _miscCodeCompileUnit);
    }

    // Add a checksum pragma.  This is used for improved debugging experience.
    private void AddChecksumPragma(BuildProvider buildProvider, CodeCompileUnit compileUnit) {

        // If we can't get a virtual path, do nothing
        if (buildProvider == null || buildProvider.VirtualPath == null)
            return;

        // Only do this if we're compiling in debug mode
        if (!_compilerType.CompilerParameters.IncludeDebugInformation)
            return;

        string physicalPath = HostingEnvironment.MapPathInternal(buildProvider.VirtualPath);

        // Only do this is the file physically exists, which it would not in the
        // case of a non-file based VirtualPathProvider.  In such case, there is
        // no point in putting the pragma, since the debugger could not locate
        // the file anyway.
        if (!File.Exists(physicalPath))
            return;

        CodeChecksumPragma pragma = new CodeChecksumPragma() {
            ChecksumAlgorithmId = s_codeChecksumSha1Id
        };

        if (_compConfig.UrlLinePragmas) {
            pragma.FileName = ErrorFormatter.MakeHttpLinePragma(buildProvider.VirtualPathObject.VirtualPathString);
        }
        else {
            pragma.FileName = physicalPath;
        }

        // Generate a SHA1 hash from the contents of the file

        // The VS debugger uses a cryptographic hash of the file being debugged so that it doesn't accidentally
        // display to the user the wrong version of the file. This is merely a convenience feature for debugging
        // purposes and is not security-related in any way. Since VS only supports MD5 and SHA1 hashes, we just
        // use SHA1 and suppress the [Obsolete] warning.
#pragma warning disable 618
        using (Stream stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            using (SHA1 hashAlgorithm = CryptoAlgorithms.CreateSHA1()) {
                pragma.ChecksumData = hashAlgorithm.ComputeHash(stream);
            }
        }
#pragma warning restore 618

        // Add the pragma to the CodeCompileUnit
        compileUnit.StartDirectives.Add(pragma);
    }

    internal CompilerParameters GetCompilerParameters() {
        CompilerParameters compilParams = _compilerType.CompilerParameters;

        string dir = _tempFiles.TempDir;

        // If a culture is set, modify the assembly name and location based on it
        if (CultureName != null) {
            dir = Path.Combine(dir, CultureName);
            Directory.CreateDirectory(dir);
            compilParams.OutputAssembly = Path.Combine(dir, OutputAssemblyName + ".resources.dll");
        }
        else {
            compilParams.OutputAssembly = Path.Combine(dir, OutputAssemblyName + ".dll");
        }

        // If such file already exist, try to delete or rename it
        if (File.Exists(compilParams.OutputAssembly))
            Util.RemoveOrRenameFile(compilParams.OutputAssembly);

        compilParams.TempFiles = _tempFiles;

        // Create the string resource file (shared by all the pages we're compiling)
        if (_stringResourceBuilder != null && _stringResourceBuilder.HasStrings) {
            string resFileName = _tempFiles.AddExtension("res");
            _stringResourceBuilder.CreateResourceFile(resFileName);
            compilParams.Win32Resource = resFileName;
        }

        // Add all the embedded resources to the compilParams
        if (_embeddedResourceFiles != null) {
            foreach (string aname in _embeddedResourceFiles)
                compilParams.EmbeddedResources.Add(aname);
        }

        // Merge the two sets of assemblies
        if (_additionalReferencedAssemblies != null) {
            foreach (Assembly assembly in _additionalReferencedAssemblies) {
                _initialReferencedAssemblies.Add(assembly);
            }
        }

        // Add all the referenced assemblies to the compilParams
        Util.AddAssembliesToStringCollection(_initialReferencedAssemblies, compilParams.ReferencedAssemblies);

        // Make any fix up adjustments to the CompilerParameters to work around some issues
        FixUpCompilerParameters(_compConfig, _compilerType.CodeDomProviderType, compilParams);

        return compilParams;
    }

    static string s_vbImportsString;

    private static void AddVBGlobalNamespaceImports(CompilerParameters compilParams) {
        // Put together the VB import string on demand
        if (s_vbImportsString == null) {
            PagesSection pagesConfig = MTConfigUtil.GetPagesAppConfig();
            if (pagesConfig.Namespaces == null) {
                s_vbImportsString = String.Empty;
            }
            else {
                StringBuilder sb = new StringBuilder();
                sb.Append("/imports:");

                bool nextItemNeedsComma = false;

                // Auto-import Microsoft.VisualBasic is needed
                if (pagesConfig.Namespaces.AutoImportVBNamespace) {
                    sb.Append("Microsoft.VisualBasic");
                    nextItemNeedsComma = true;
                }

                // Add all the namespaces from the config <namespaces> section
                foreach (NamespaceInfo entry in pagesConfig.Namespaces) {

                    // If there was a previous entry, we need a comma separator
                    if (nextItemNeedsComma)
                        sb.Append(',');

                    sb.Append(entry.Namespace);

                    nextItemNeedsComma = true;
                }

                s_vbImportsString = sb.ToString();
            }
        }

        // Prepend it to the compilerOptions
        if (s_vbImportsString.Length > 0) {
            if (compilParams.CompilerOptions == null)
                compilParams.CompilerOptions = s_vbImportsString;
            else
                compilParams.CompilerOptions = s_vbImportsString + " " + compilParams.CompilerOptions;
        }
    }

    // Command line string for My.* support
    private const string MySupport = @"/define:_MYTYPE=\""Web\""";

    private static void AddVBMyFlags(CompilerParameters compilParams) {

        // Prepend it to the compilerOptions
        if (compilParams.CompilerOptions == null)
            compilParams.CompilerOptions = MySupport;
        else
            compilParams.CompilerOptions = MySupport + " " + compilParams.CompilerOptions;
    }


    [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Warnings was about use of CompilerParameters.CoreAssemblyFileName which was not set by user supplied string - so okay.")]
    internal static void FixUpCompilerParameters(CompilationSection compilationSection, Type codeDomProviderType, CompilerParameters compilParams) {
        // The mscorlib reference is special cased, and needs to be passed via the CoreAssemblyFileName property.
        if (BuildManagerHost.InClientBuildManager && !MultiTargetingUtil.IsTargetFramework20 && !MultiTargetingUtil.IsTargetFramework35) {
            string coreAssemblyFile;
            AssemblyResolver.GetPathToReferenceAssembly(typeof(string).Assembly, out coreAssemblyFile);
            compilParams.CoreAssemblyFileName = coreAssemblyFile;
        }

        // DevDiv 404267: If the developer enabled 'warnings as errors', we should disable [Obsolete] warnings. This helps
        // prevent in-place framework updates from breaking runtime compilation of pages. We only respect this attribute
        // when not in the CBM, as CBM is a design-time feature instead of a runtime feature, and the developer probably
        // wants to be notified of all errors at design time.
        bool disableObsoleteWarnings = !BuildManagerHost.InClientBuildManager && compilationSection.DisableObsoleteWarnings;

        // If C#, remove the warning that complains about variables that start with "__"
        // Also ignore warning that complains about assemblyKeyName and delaysign
        // Also ignore warning about assuming assembly versions matching (CS1701, DevDiv 137847, warning about System.Web.Extensions v1.0 matching v3.5)
        if (codeDomProviderType == typeof(Microsoft.CSharp.CSharpCodeProvider)) {
            List<string> noWarnStrings = new List<string>(5);
            noWarnStrings.AddRange(new string[] { "1659", "1699", "1701" });

            if (disableObsoleteWarnings) {
                noWarnStrings.Add("612"); // [Obsolete] without message
                noWarnStrings.Add("618"); // [Obsolete("with message")]
            }

            CodeDomUtility.PrependCompilerOption(compilParams, "/nowarn:" + String.Join(";", noWarnStrings));
        }
        else if (codeDomProviderType == typeof(Microsoft.VisualBasic.VBCodeProvider)) {
            List<string> noWarnStrings = new List<string>(3);

            // If VB, add all the imported namespaces on the command line (DevDiv 21499).
            // This is VB only because other languages don't support global command line
            // namespace imports.
            AddVBGlobalNamespaceImports(compilParams);

            // Add any command line flags needed to support the My.* feature
            AddVBMyFlags(compilParams);

            // Ignore vb warning that complains about assemblyKeyName (Dev10 662544)
            // but only for target 3.5 and above (715329)
            if (MultiTargetingUtil.TargetFrameworkVersion >= MultiTargetingUtil.Version35) {
                noWarnStrings.Add("41008");
            }

            if (disableObsoleteWarnings) {
                noWarnStrings.Add("40000"); // [Obsolete("with message")]
                noWarnStrings.Add("40008"); // [Obsolete] without message
            }

            if (noWarnStrings.Count > 0) {
                CodeDomUtility.PrependCompilerOption(compilParams, "/nowarn:" + String.Join(",", noWarnStrings));
            }
        }

        ProcessProviderOptions(codeDomProviderType, compilParams);
        FixTreatWarningsAsErrors(codeDomProviderType, compilParams);

        // Add CodeAnalysis symbol if required by client.
        if (BuildManager.PrecompilingWithCodeAnalysisSymbol) {
            CodeDomUtility.PrependCompilerOption(compilParams, "/define:CODE_ANALYSIS");
        }
    }

    // DevDiv 114316
    // CodeDom sets TreatWarningAsErrors to true whenever warningLevel is non-zero.
    // To get warnings only, the workaround is to use /warnaserror- in CompilerOptions.
    // However this does not work in some cases, as TreatWarningAsErrors set to true still emits
    // /warnaserror+.
    // So, whenever the user wants /warnaserror[+|-|numberlist], we explicitly set TreatWarningsAsErrors to false,
    // so that the /warnaserror+ is not emitted, and the user can specify exactly what is desired.
    internal static void FixTreatWarningsAsErrors(Type codeDomProviderType, CompilerParameters compilParams) {
        // Only do so for C# and VB.
        if (codeDomProviderType != typeof(Microsoft.CSharp.CSharpCodeProvider) && 
            codeDomProviderType != typeof(Microsoft.VisualBasic.VBCodeProvider))
            return;

        if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(compilParams.CompilerOptions, "/warnaserror", CompareOptions.IgnoreCase) >= 0)
            compilParams.TreatWarningsAsErrors = false;
    }


    // Check for OptionInfer and WarnAsError. This is the workaround as use of compilerOptions is not allowed in partial trust.
    // Devdiv 130325
    // 
    private static void ProcessProviderOptions(Type codeDomProviderType, CompilerParameters compilParams) {
        IDictionary<string, string> providerOptions = CompilationUtil.GetProviderOptions(codeDomProviderType);
        if (providerOptions == null) return;

        // For C# and VB, check for WarnAsError
        if (codeDomProviderType == typeof(Microsoft.VisualBasic.VBCodeProvider) ||
            codeDomProviderType == typeof(Microsoft.CSharp.CSharpCodeProvider))
            ProcessBooleanProviderOption("WarnAsError", "/warnaserror+", "/warnaserror-", providerOptions, compilParams);

        // Only process OptionInfer for v3.5 compiler (or above)
        if (codeDomProviderType == null || !CompilationUtil.IsCompilerVersion35OrAbove(codeDomProviderType))
            return;

        // For VB, check for OptionInfer
        if (codeDomProviderType == typeof(Microsoft.VisualBasic.VBCodeProvider))
            ProcessBooleanProviderOption("OptionInfer", "/optionInfer+", "/optionInfer-", providerOptions, compilParams);

    }

    private static void ProcessBooleanProviderOption(string providerOptionName, string trueCompilerOption, string falseCompilerOption,
        IDictionary<string, string> providerOptions, CompilerParameters compilParams) {

        if (providerOptions == null || compilParams == null) return;
        Debug.Assert(providerOptionName != null, "providerOptionName should not be null");
        Debug.Assert(trueCompilerOption != null, "trueCompilerOption should not be null");
        Debug.Assert(falseCompilerOption != null, "falseCompilerOption should not be null");

        string providerOptionValue = null;
        if (!providerOptions.TryGetValue(providerOptionName, out providerOptionValue)) return;
        if (string.IsNullOrEmpty(providerOptionValue))
            throw new System.Configuration.ConfigurationErrorsException(SR.GetString(SR.Property_NullOrEmpty, CompilationUtil.CodeDomProviderOptionPath + providerOptionName));

        bool value;

        if (Boolean.TryParse(providerOptionValue, out value)) {
            // If the value is boolean, insert the compiler options
            if (value)
                CodeDomUtility.AppendCompilerOption(compilParams, trueCompilerOption);
            else
                CodeDomUtility.AppendCompilerOption(compilParams, falseCompilerOption);
        }
        else {
            // If the value is not boolean, throw an exception
            throw new System.Configuration.ConfigurationErrorsException(SR.GetString(SR.Value_must_be_boolean, CompilationUtil.CodeDomProviderOptionPath + providerOptionName));
        }
    }


    internal CompilerResults Compile() {

        // First, check if there is something to compile
        if (_sourceFiles.Count == 0 && _embeddedResourceFiles == null)
            return null;

        // if we have some fast object factories to generate, get the CodeCompileUnit
        if (_objectFactoryGenerator != null) {
            _miscCodeCompileUnit = _objectFactoryGenerator.CodeCompileUnit;
        }

        // Add a culture attribute if needed
        AddAssemblyCultureAttribute();

        // Add a ComVisible(false) attribute (VSWhidbey 436453)
        // Actually, don't do it to avoid breaking migrated apps (VSWhidbey 446788)
        //AddComVisibleAttribute();

        // Add an AspNetGeneratedCode attribute to help fxcop ignore some violations (VSWhidbey 437581)
        AddAspNetGeneratedCodeAttribute();

        // Add an AllowPartiallyTrustedCallers attribute to make strong-name assemblies. (Devdiv 39696)
        AddAllowPartiallyTrustedCallersAttribute();

        AddAssemblyDelaySignAttribute();

        AddAssemblyKeyFileAttribute();

        AddAssemblyKeyContainerAttribute();

        AddSecurityRulesAttribute();
        
        AddTargetFrameworkAttribute();

        // Generate a source file for the misc top level items if needed
        GenerateMiscCodeCompileUnit();

        CompilerParameters compilParams = GetCompilerParameters();

        string[] files = new string[_sourceFiles.Count];
        _sourceFiles.CopyTo(files, 0);

        // Increment compilation counter
        PerfCounters.IncrementCounter(AppPerfCounter.COMPILATIONS);

        // Raise Web Event
        WebBaseEvent.RaiseSystemEvent(this, WebEventCodes.ApplicationCompilationStart);
        
        HttpContext context = HttpContext.Current;

        if (context != null) {
            if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_COMPILE_ENTER, context.WorkerRequest);
        }

        CompilerResults results = null;

        try {
            try {
                // Revert impersonation when compiling source code in the codegen dir (VSWhidbey 176576)
                using (new ProcessImpersonationContext()) {
                    results = _codeProvider.CompileAssemblyFromFile(compilParams, files);
                }
            }
            finally {
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure) && context != null) {

                    string fileNames = null;
                    if (_buildProviders.Count < 20) {
                        IDictionaryEnumerator e = _buildProviders.GetEnumerator();
                        while(e.MoveNext()) {
                            if (fileNames != null)
                                fileNames += ",";
                            fileNames += e.Key;
                        }
                    }
                    else {
                        fileNames = String.Format(CultureInfo.InstalledUICulture, SR.Resources.GetString(SR.Etw_Batch_Compilation, CultureInfo.InstalledUICulture), new object[1] {_buildProviders.Count});
                    }

                    string status;
                    if (results != null && (results.NativeCompilerReturnValue != 0 || results.Errors.HasErrors))
                        status = SR.Resources.GetString(SR.Etw_Failure, CultureInfo.InstalledUICulture);
                    else
                        status = SR.Resources.GetString(SR.Etw_Success, CultureInfo.InstalledUICulture);

                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_COMPILE_LEAVE, context.WorkerRequest, fileNames, status);
                }
            }
        }
        catch { throw; }    // Prevent Exception Filter Security Issue (ASURT 122835)

        // If an IAssemblyPostProcessor is registered, call it
        Type postProcessorType = _compConfig.AssemblyPostProcessorTypeInternal;
        if (postProcessorType != null) {
            using (IAssemblyPostProcessor postProcessor = (IAssemblyPostProcessor) HttpRuntime.FastCreatePublicInstance(postProcessorType)) {
                postProcessor.PostProcessAssembly(results.PathToAssembly);
            }
        }

        // Raise Web Event
        WebBaseEvent.RaiseSystemEvent(this, WebEventCodes.ApplicationCompilationEnd);

        if (results != null) {

            // Invalidate an invalid assembly to trigger recompilation
            InvalidateInvalidAssembly(results, compilParams);

            // Fix up with line pragmas to account for the http case, and for some special conditions
            FixUpLinePragmas(results);

            if (results.Errors.HasErrors) {
                // Give all the BuildProviders a chance to look at the compile errors, and possibly tweak them
                foreach (BuildProvider buildProvider in BuildProviders) {
                    buildProvider.ProcessCompileErrors(results);
                }
            }

            // If there is a CBM callback, inform it of the errors/warnings
            if (BuildManager.CBMCallback != null) {
                foreach (CompilerError error in results.Errors) {
                    BuildManager.CBMCallback.ReportCompilerError(error);
                }
            }

            // If there are errors, increment the relevant perf counters and throw
            if (results.NativeCompilerReturnValue != 0 || results.Errors.HasErrors) {

                // Increment the compilation error and total error counters
                PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_COMPILING);
                PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);

                throw new HttpCompileException(results, GetErrorSourceFileContents(results));
            }
        }

        return results;
    }

    private void InvalidateInvalidAssembly(CompilerResults results, CompilerParameters compilParams) {
        // VSWhidbey 610291
        // If target assembly gets locked, we invalidate the assembly, so that it does
        // not get used, and the next compilation will use a new assembly name.
        // CS0016 is the error code for "Could not write to output file 'file' -- 'reason'"

        if (results == null || !results.Errors.HasErrors)
            return;

        foreach (CompilerError error in results.Errors) {
            if (error.IsWarning) continue;

            if (StringUtil.EqualsIgnoreCase(error.ErrorNumber, "CS0016")){

                // Also invalidate the base assembly if this is a localized resource assembly 
                if (CultureName != null) {
                    string dir = _tempFiles.TempDir;
                    string baseAssemblyFile = Path.Combine(dir, OutputAssemblyName + ".dll");
                    DiskBuildResultCache.TryDeleteFile(new FileInfo(baseAssemblyFile));
                }

                // Invalidate the target assembly
                DiskBuildResultCache.TryDeleteFile(compilParams.OutputAssembly);
            }
        }
    }

    /*
     * Fix up all the source files in the errors in case they are HTTP (VS compiler scenario).
     * Also, fix the error in case the base class was incorrect in the code beside model
     */
    private void FixUpLinePragmas(CompilerResults results) {

        CompilerError badBaseClassError = null;

        // Go through the errors backwards so we can delete them as needed
        for (int i=results.Errors.Count-1; i>=0; i--) {
            CompilerError error = results.Errors[i];

            string physicalPath = ErrorFormatter.ResolveHttpFileName(error.FileName);

            // Only replace it by the physical path if it actually exists, which may not
            // be the case when using a VirtualPathProvider
            if (File.Exists(physicalPath)) {
                error.FileName = physicalPath;

                // If it is our special marker line number, remember it and remove it.
                // We place the marker at two places: 1) before setting AppRelativeVirtualPath in the constructor,
                // and 2) before the method FrameworkInitialize.
                // For the generated method FrameworkInitialize, the method comes one line after 
                // the marker, due to an additional line taken by the DebuggerNonUserCodeAttribute. (DevDiv 175681)
                if (error.Line == TemplateControlCodeDomTreeGenerator.badBaseClassLineMarker ||
                    (error.Line == TemplateControlCodeDomTreeGenerator.badBaseClassLineMarker + 1 &&
                     error.ErrorText != null && error.ErrorText.IndexOf("FrameworkInitialize", StringComparison.OrdinalIgnoreCase) >= 0)) {
                    badBaseClassError = error;
                    results.Errors.RemoveAt(i);
                }
                else if (error.Line > TemplateControlCodeDomTreeGenerator.badBaseClassLineMarker &&
                    error.Line < TemplateControlCodeDomTreeGenerator.badBaseClassLineMarker + 50) {

                    // Also, if within range of it, remove it altogether
                    results.Errors.RemoveAt(i);
                }
            }
        }

        // If we found our special marker error, we're most likely in a situation where
        // the class in the code beside file doesn't match the 'inherits' in the aspx/ascx,
        // or is missing the based type (or has the wrong base type).  In that case, change
        // the error message to make the problem explicit to the user (VSWhidbey 376977/468830)
        if (badBaseClassError != null) {

            // Read the content of the code beside file
            string codeFileContent = Util.StringFromFile(badBaseClassError.FileName);

            // Search for the partial class declaration within the file.  We do this by searching for
            // the string "partial class" in case insensitive way.  This is far from fool proof, but
            // it covers the common VB and C# cases, and the fallback when not found is reasonable.
            int classOffset = CultureInfo.InvariantCulture.CompareInfo.IndexOf(codeFileContent,
                "partial class", CompareOptions.IgnoreCase);

            if (classOffset >= 0) {
                // We found it, so figure out the line number from it
                badBaseClassError.Line = Util.LineCount(codeFileContent, 0, classOffset) + 1;
            }
            else {
                // Otherwise, just use 1.  It won't point to the right line, but at least the error
                // message is helpful
                badBaseClassError.Line = 1;
            }

            // Change the error message to make the situation clear to the user
            badBaseClassError.ErrorText = SR.GetString(SR.Bad_Base_Class_In_Code_File);
            badBaseClassError.ErrorNumber = "ASPNET";

            // Insert the error at the begining of the collection, since we display the first error.
            results.Errors.Insert(0, badBaseClassError);
        }
    }

    /*
     * Attempt to find the generated source file that has the error, and return
     * its contents as a string (for error reproting purposes).
     * Note that when debug is false, we set tempFiles.KeepFiles to false, and
     * all the sources will be gone by the time we get here.  I filed VSWhidbey 103673,
     * to get a solution to this from BCL.
     */
    private string GetErrorSourceFileContents(CompilerResults results) {

        if (!results.Errors.HasErrors)
            return null;

        // Get the physical path of the file that has the error. Note that this could be
        // either the path to a high level file (e.g. aspx) if pragmas are in play,
        // or the path to a generated file if there are no pragmas
        string linePragma = results.Errors[0].FileName;

        // Attempt to locate the correct build provider
        BuildProvider buildProvider = GetBuildProviderFromLinePragma(linePragma);

        if (buildProvider != null) {
            // Return the generated file for this build provider
            return GetGeneratedSourceFromBuildProvider(buildProvider);
        }

        // If we didn't find it, then we're probably in the no pragma case, in
        // which case linePragma itself is the generated file
        return Util.StringFromFileIfExists(linePragma);
    }

    internal string GetGeneratedSourceFromBuildProvider(BuildProvider buildProvider) {

        // Return the generated file content for this build provider
        string generatedFilePath = (string) _buildProviderToSourceFileMap[buildProvider];
        return Util.StringFromFileIfExists(generatedFilePath);
    }

    internal BuildProvider GetBuildProviderFromLinePragma(string linePragma) {
        BuildProvider buildProvider = GetBuildProviderFromLinePragmaInternal(linePragma);

        // If it's a CompileWith provider, return the main provider instead
        SourceFileBuildProvider sourceBuildProvider = buildProvider as SourceFileBuildProvider;
        if (sourceBuildProvider != null)
            buildProvider = sourceBuildProvider.OwningBuildProvider;

        return buildProvider;
    }

    private BuildProvider GetBuildProviderFromLinePragmaInternal(string linePragma) {

        // If we didn't keep track of any generated files, we can't do much
        if (_buildProviderToSourceFileMap == null)
            return null;

        // Check if it's an http line pragma, from which we can get a VirtualPath
        string virtualPath = ErrorFormatter.GetVirtualPathFromHttpLinePragma(linePragma);

        // First, look for the pragma case
        foreach (BuildProvider buildProvider in BuildProviders) {

            // If the build provider can't give us a virtual path, skip it
            if (buildProvider.VirtualPath == null)
                continue;

            // If we got a virtual path, use it to locate the correct BuildProvider
            if (virtualPath != null) {
                if (StringUtil.EqualsIgnoreCase(virtualPath, buildProvider.VirtualPath)) {
                    return buildProvider;
                }

                continue;
            }

            // Otherwise, work with the physical path

            string physicalPath = HostingEnvironment.MapPathInternal(buildProvider.VirtualPath);

            if (StringUtil.EqualsIgnoreCase(linePragma, physicalPath)) {
                return buildProvider;
            }
        }

        return null;
    }
}

/*
 * This class is used intead of AssemblyBuilder when handling
 * ClientBuildManager.GetCodeDirectoryInformation
 * It is instantiated via CompilerType.CreateAssemblyBuilder.
 */
internal class CbmCodeGeneratorBuildProviderHost: AssemblyBuilder {

    private string _generatedFilesDir;

    internal CbmCodeGeneratorBuildProviderHost(CompilationSection compConfig,
        ICollection referencedAssemblies, CompilerType compilerType,
        string generatedFilesDir, string outputAssemblyName)
        : base(compConfig, referencedAssemblies, compilerType, outputAssemblyName) {

        // Wipe out any existing directory, and recreate it
        // This is where we will put generated source files
        if (Directory.Exists(generatedFilesDir)) {

            // Delete all the files in the directory
            foreach (FileData fileData in FileEnumerator.Create(generatedFilesDir)) {

                // It should only contain files
                Debug.Assert(!fileData.IsDirectory);
                if (fileData.IsDirectory) continue;

                Debug.Trace("CbmCodeGeneratorBuildProviderHost", "Deleting " + fileData.FullName);
                File.Delete(fileData.FullName);
            }
            
        }

        // Create it to make sure it exists
        Directory.CreateDirectory(generatedFilesDir);

        _generatedFilesDir = generatedFilesDir;
    }

    internal override TextWriter CreateCodeFile(BuildProvider buildProvider, out string filename) {

        // use GetCacheKeyFromVirtualPath to get a file name that looks like
        // the original file, but is guaranteed unique across different virtual dirs.
        string generatedCodeFile = BuildManager.GetCacheKeyFromVirtualPath(
            buildProvider.VirtualPathObject);

        generatedCodeFile = Path.Combine(_generatedFilesDir, generatedCodeFile);
        generatedCodeFile = FileUtil.TruncatePathIfNeeded(generatedCodeFile, 10 /*length of extension */);

        generatedCodeFile = generatedCodeFile + "." + _codeProvider.FileExtension;
        filename = generatedCodeFile;

        BuildManager.GenerateFileTable[buildProvider.VirtualPathObject.VirtualPathStringNoTrailingSlash] = generatedCodeFile;

        Debug.Trace("CbmCodeGeneratorBuildProviderHost", "Generating " + generatedCodeFile);

        Stream temp = new FileStream(generatedCodeFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        return new StreamWriter(temp, Encoding.UTF8);
    }

    internal override void AddBuildProvider(BuildProvider buildProvider) {

        // Skip source files, since their code generation is an identity transform
        if (buildProvider is SourceFileBuildProvider)
            return;

        base.AddBuildProvider(buildProvider);
    }
}

}
