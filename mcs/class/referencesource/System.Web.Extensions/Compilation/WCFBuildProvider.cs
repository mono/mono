//------------------------------------------------------------------------------
// <copyright file="WCFBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Compilation
{

    using System;
    using System.Globalization;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Xml.Serialization;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Diagnostics;
    using System.Threading;
    using System.Text;
    using System.Web.Compilation.WCFModel;
    using System.Collections.Generic;
    using System.Web.Configuration;
    using System.Web.Resources;
    using System.Security;
    using System.Security.Permissions;
    using System.Data.Services.Design;
    using System.IO;
    using System.Xml;
    using System.Reflection;


    /// <summary>
    /// A build provider for WCF service references in ASP.NET projects.
    ///   (Note: the ASMX version is called WebReferencesBuildProvider).
    /// Due to compatibility requirements, as few changes as possible were made
    ///   to System.Web.dll, which contains the build manager and WebReferencesBuildProvider.
    ///   WebReferencesBuildProvider will call into us for the App_WebReferences folder and
    ///   each of its subdirectories (recursively) if it finds our assembly and type on the
    ///   machine.
    /// Note: for a normal BuildProvider, the input file is represented by the protected VirtualPath
    ///   property of the base.  But for this build provider (same as for WebReferencesBuildProvider, the
    ///   asmx web references build provider), the input is an entire directory, which is still represented
    ///   by the protected VirtualPath property.
    /// TO DEBUG: The easiest way to debug is to debug the aspnet_compiler.exe command-line tool
    ///   while it compiles a website with a .svcmap file in it.
    ///   As an example, you could debug aspnet_compiler.exe with this command-line to debug
    ///   one of the suites:
    ///   
    ///     /v MiddleService -p {path}\ddsuites\src\vs\vb\IndigoTools\BuildProvider\WCFBuildProvider1\WebSite\MiddleService -c c:\temp\output
    ///     
    ///   Important: it will only call the build provider if the sources in the website have changed or if you delete
    ///   the deleted output folder's contents.
    ///   
    /// Data services (Astoria): in order to support Astoria "data services" we added code
    /// to scan for "datasvcmap" files in addition to the existing "svcmap" files. For data services
    /// we call into the Astoria code-gen library to do the work instead of the regular indigo path.
    ///     
    /// </summary>
    [
    SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased",
                    Justification = "Too late to change in Orcas--the WCFBuildProvider is hard-coded in ASP.NET " +
                                   "build manager for app_webreferences folder. Build manager is part of redbits.")
    ]
    // Transparency
    [SecurityCritical]
    public class WCFBuildProvider : BuildProvider
    {
        internal const string WebRefDirectoryName = "App_WebReferences";
        internal const string SvcMapExtension = ".svcmap";
        internal const string DataSvcMapExtension = ".datasvcmap";
        private const string TOOL_CONFIG_ITEM_NAME = "Reference.config";

        /// <summary>
        /// version number for 3.5 framework
        /// </summary>
        private const int FRAMEWORK_VERSION_35 = 0x30005;

        /// <summary>
        /// Search through the folder represented by base.VirtualPath for .svcmap and .datasvcmap files.  
        /// If any .svcmap/.datasvcmap files are found, then generate proxy code for them into the 
        /// specified assemblyBuilder.
        /// </summary>
        /// <param name="assemblyBuilder">Where to generate the proxy code</param>
        /// <remarks>
        /// When this routine is called, it is expected that the protected VirtualPath property has
        ///   been set to the folder to scan for .svcmap/.datasvcmap files.
        /// </remarks>
        [SecuritySafeCritical]
        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {

            // Go through all the svcmap files in the directory
            VirtualDirectory vdir = GetVirtualDirectory(VirtualPath);
            foreach (VirtualFile child in vdir.Files)
            {
                string extension = IO.Path.GetExtension(child.VirtualPath);
                if (extension.Equals(SvcMapExtension, StringComparison.OrdinalIgnoreCase))
                {
                    // .svcmap file found

                    // NOTE: the WebReferences code requires a physical path, so this feature
                    // cannot work with a non-file based VirtualPathProvider
                    string physicalPath = HostingEnvironment.MapPath(child.VirtualPath);

                    CodeCompileUnit codeUnit = GenerateCodeFromServiceMapFile(physicalPath);

                    // Add the CodeCompileUnit to the compilation
                    assemblyBuilder.AddCodeCompileUnit(this, codeUnit);
                }
                else if (extension.Equals(DataSvcMapExtension, StringComparison.OrdinalIgnoreCase))
                {
                    // In .NET FX 3.5, the ADO.NET Data Service build provider was included as part of the
                    // WCF build provider. In .NET FX 4.0, it is a separate build provider. However, under certain
                    // circumstances (i.e. design time/Visual Studio), we may call the 4.0 version of the build provider 
                    // when we actually want the web site to target .NET FX 3.5, and in that case, we have to emulate the 
                    // old behavior. 
                    if (BuildManager.TargetFramework.Version.Major < 4)
                    {

                        // NOTE: the WebReferences code requires a physical path, so this feature
                        // cannot work with a non-file based VirtualPathProvider
                        string physicalPath = HostingEnvironment.MapPath(child.VirtualPath);

                        GenerateCodeFromDataServiceMapFile(physicalPath, assemblyBuilder);
                    }
                }
            }
        }

        /// <summary>
        /// Generate code for one .datasvcmap file
        /// </summary>
        /// <param name="mapFilePath">The physical path to the data service map file</param>
        private void GenerateCodeFromDataServiceMapFile(string mapFilePath, AssemblyBuilder assemblyBuilder)
        {
            try
            {
                assemblyBuilder.AddAssemblyReference(typeof(System.Data.Services.Client.DataServiceContext).Assembly);

                DataSvcMapFileLoader loader = new DataSvcMapFileLoader(mapFilePath);
                DataSvcMapFile mapFile = loader.LoadMapFile() as DataSvcMapFile;

                if (mapFile.MetadataList[0].ErrorInLoading != null)
                {
                    throw mapFile.MetadataList[0].ErrorInLoading;
                }

                string edmxContent = mapFile.MetadataList[0].Content;

                System.Data.Services.Design.EntityClassGenerator generator = new System.Data.Services.Design.EntityClassGenerator(LanguageOption.GenerateCSharpCode);

                // the EntityClassGenerator works on streams/writers, does not return a CodeDom
                // object, so we use CreateCodeFile instead of compile units.
                using (TextWriter writer = assemblyBuilder.CreateCodeFile(this))
                {

                    // Note: currently GenerateCode never actually returns values
                    // for the error case (even though it returns an IList of error
                    // objects). Instead it throws on error. This may need some tweaking 
                    // later on.
#if DEBUG
                object errors = 
#endif
                    generator.GenerateCode(
                        XmlReader.Create(new StringReader(edmxContent)),
                        writer,
                        GetGeneratedNamespace());

#if DEBUG
                Debug.Assert(
                    errors == null ||
                    !(errors is ICollection) ||
                    ((ICollection)errors).Count == 0,
                    "Errors reported through the return value. Expected an exception");
#endif
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                errorMessage = String.Format(CultureInfo.CurrentCulture, "{0}: {1}", IO.Path.GetFileName(mapFilePath), errorMessage);
                throw new InvalidOperationException(errorMessage, ex);
            }
        }

        /// <summary>
        /// Generate code for one .svcmap file
        /// </summary>
        /// <param name="mapFilePath">the path to the service map file</param>
        /// <return></return>
        /// <remarks></remarks>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Violation is no longer relevant due to 4.0 CAS model")]
        private CodeCompileUnit GenerateCodeFromServiceMapFile(string mapFilePath)
        {
            try
            {
                string generatedNamespace = GetGeneratedNamespace();
                SvcMapFileLoader loader = new SvcMapFileLoader(mapFilePath);
                SvcMapFile mapFile = loader.LoadMapFile() as SvcMapFile;

                HandleProxyGenerationErrors(mapFile.LoadErrors);

                // We always use C# for the generated proxy
                CodeDomProvider provider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("c#");

                //Note: with the current implementation of the generator, it does all of its
                //  work in the constructor.  This may change in the future.
                VSWCFServiceContractGenerator generator = VSWCFServiceContractGenerator.GenerateCodeAndConfiguration(
                    mapFile,
                    GetToolConfig(mapFile, mapFilePath),
                    provider,
                    generatedNamespace,
                    null, //targetConfiguration
                    null, //configurationNamespace
                    new ImportExtensionServiceProvider(),
                    new TypeResolver(),
                    FRAMEWORK_VERSION_35,
                    typeof (System.Data.Design.TypedDataSetSchemaImporterExtensionFx35) //Always we are above framework version 3.5
                );

                // Determine what "name" to display to users for the service if there are any exceptions
                // If generatedNamespace is empty, then we display the name of the .svcmap file.
                string referenceDisplayName = String.IsNullOrEmpty(generatedNamespace) ?
                    System.IO.Path.GetFileName(mapFilePath) : generatedNamespace;
                
                VerifyGeneratedCodeAndHandleErrors(referenceDisplayName, mapFile, generator.TargetCompileUnit, generator.ImportErrors, generator.ProxyGenerationErrors);
#if DEBUG
#if false
                IO.TextWriter writer = new IO.StringWriter();
                CodeGeneratorOptions options = new CodeGeneratorOptions();
                options.BlankLinesBetweenMembers=true;
                provider.GenerateCodeFromCompileUnit(generator.TargetCompileUnit, writer, options);
                Debug.WriteLine("Generated proxy code:\r\n" + writer.ToString());
#endif
#endif
                return generator.TargetCompileUnit;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                errorMessage = String.Format(CultureInfo.CurrentCulture, "{0}: {1}", IO.Path.GetFileName(mapFilePath), errorMessage);
                throw new InvalidOperationException(errorMessage, ex);
            }
        }

        /// <summary>
        /// If there are errors passed in, handle them by throwing an appropriate error
        /// </summary>
        /// <param name="errors">IEnumerable for easier unit test accessors</param>
        private static void HandleProxyGenerationErrors(System.Collections.IEnumerable /*<ProxyGenerationError>*/ errors)
        {
            foreach (ProxyGenerationError generationError in errors)
            {
                // NOTE: the ASP.Net framework does not handle an error list, so we only give them the first error message
                // all warning messages are ignored today
                //
                // We treat all error messages from WsdlImport and ProxyGenerator as warning messages
                //   The reason is that many of them are ignorable and doesn't block generating useful code.
                if (!generationError.IsWarning && 
                        generationError.ErrorGeneratorState != WCFModel.ProxyGenerationError.GeneratorState.GenerateCode)
                {
                    throw new InvalidOperationException(ConvertToBuildProviderErrorMessage(generationError));
                }
            }
        }

        /// <summary>
        /// Merge error message strings together
        /// </summary>
        /// <param name="errors"></param>
        /// <param name="collectedMessages"></param>
        /// <return></return>
        /// <remarks></remarks>
        private static void CollectErrorMessages(System.Collections.IEnumerable errors, StringBuilder collectedMessages)
        {
            foreach (ProxyGenerationError generationError in errors)
            {
                if (!generationError.IsWarning)
                {
                    if (collectedMessages.Length > 0)
                    {
                        collectedMessages.Append(Environment.NewLine);
                    }
                    collectedMessages.Append(ConvertToBuildProviderErrorMessage(generationError));
                }
            }
        }

        /// <summary>
        /// Format an error reported by the code generator to message string reported to the user.
        /// </summary>
        /// <param name="generationError"></param>
        /// <return></return>
        /// <remarks></remarks>
        private static string ConvertToBuildProviderErrorMessage(ProxyGenerationError generationError)
        {
            string errorMessage = generationError.Message;
            if (!String.IsNullOrEmpty(generationError.MetadataFile))
            {
                if (generationError.LineNumber < 0)
                {
                    errorMessage = String.Format(CultureInfo.CurrentCulture, "'{0}': {1}", generationError.MetadataFile, errorMessage);
                }
                else if (generationError.LinePosition < 0)
                {
                    errorMessage = String.Format(CultureInfo.CurrentCulture, "'{0}' ({1}): {2}", generationError.MetadataFile, generationError.LineNumber, errorMessage);
                }
                else
                {
                    errorMessage = String.Format(CultureInfo.CurrentCulture, "'{0}' ({1},{2}): {3}", generationError.MetadataFile, generationError.LineNumber, generationError.LinePosition, errorMessage);
                }
            }
            return errorMessage;
        }

        /// <summary>
        /// Check the result from the code generator.
        /// By default we treat all error messages from WsdlImporter as warnings,
        /// and they will be ignored if valid code has been generated.
        /// We may hit other errors when we parse the metadata files.
        /// Those errors (which are usually because of a bad file) will not be ignored, because the user can fix them.
        /// If the WsdlImporter hasn't generated any code as we expect, we have to consider some of the error messages are fatal.
        /// We collect those messages and report to the user.
        /// </summary>
        /// <param name="referenceDisplayName">The name of the generated reference</param>
        /// <param name="mapFile">Original Map File</param>
        /// <param name="generatedCode">generated code compile unit</param>
        /// <param name="importErrors"></param>
        /// <param name="generatorErrors"></param>
        /// <remarks></remarks>
        private static void VerifyGeneratedCodeAndHandleErrors(
                                            string referenceDisplayName,
                                            SvcMapFile mapFile,
                                            CodeCompileUnit generatedCode,
                                            System.Collections.IEnumerable importErrors,
                                            System.Collections.IEnumerable generatorErrors)
        {
            // Check and report fatal error first...
            HandleProxyGenerationErrors(importErrors);
            HandleProxyGenerationErrors(generatorErrors);

            // if there is no fatal error, we expect valid type generated from the process
            //   unless there is no metadata files, or there is a service contract type sharing
            if (mapFile.MetadataList.Count > 0 && mapFile.ClientOptions.ServiceContractMappingList.Count == 0)
            {
                if (!IsAnyTypeGenerated(generatedCode))
                {
                    StringBuilder collectedMessages = new StringBuilder();

                    // merge error messages
                    CollectErrorMessages(importErrors, collectedMessages);
                    CollectErrorMessages(generatorErrors, collectedMessages);

                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WCFModelStrings.ReferenceGroup_FailedToGenerateCode, referenceDisplayName, collectedMessages.ToString()));
                }
            }
        }

        /// <summary>
        /// Check whether we have generated any types
        /// </summary>
        /// <param name="CompileUnit"></param>
        /// <return></return>
        /// <remarks></remarks>
        private static bool IsAnyTypeGenerated(CodeCompileUnit compileUnit)
        {
            if (compileUnit != null)
            {
                foreach (System.CodeDom.CodeNamespace codeNamespace in compileUnit.Namespaces)
                {
                    if (codeNamespace.Types.Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///  Retrieve a VirtualDirectory for the given virtual path
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        private VirtualDirectory GetVirtualDirectory(string virtualPath)
        {
            return HostingEnvironment.VirtualPathProvider.GetDirectory(VirtualPath);
        }

        /// <summary>
        ///  Caculate our namespace for current VirtualPath...
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        private string GetGeneratedNamespace()
        {
            // First, determine the namespace to use for code generation.  This is based on the
            //   relative path of the reference from its base App_WebReferences directory

            // ... Get the virtual path to the App_WebReferences folder, e.g "/MyApp/App_WebReferences"
            string rootWebRefDirVirtualPath = GetWebRefDirectoryVirtualPath();

            // ... Get the folder's directory path, e.g "/MyApp/Application_WebReferences/Foo/Bar",
            //     where we'll look for .svcmap files
            string currentSubfolderUnderWebReferences = this.VirtualPath;
            if (currentSubfolderUnderWebReferences == null)
            {
                Debug.Fail("Shouldn't be given a null virtual path");
                throw new InvalidOperationException();
            }

            return CalculateGeneratedNamespace(rootWebRefDirVirtualPath, currentSubfolderUnderWebReferences);
        }

        /// <summary>
        /// Determine the namespace to use for the proxy generation
        /// </summary>
        /// <param name="webReferencesRootVirtualPath">The path to the App_WebReferences folder</param>
        /// <param name="virtualPath">The path to the current folder</param>
        /// <returns></returns>
        private static string CalculateGeneratedNamespace(string webReferencesRootVirtualPath, string virtualPath)
        {
            // ... Ensure both folders have trailing slashes
            webReferencesRootVirtualPath = VirtualPathUtility.AppendTrailingSlash(webReferencesRootVirtualPath);
            virtualPath = VirtualPathUtility.AppendTrailingSlash(virtualPath);

            Debug.Assert(virtualPath.StartsWith(webReferencesRootVirtualPath, StringComparison.OrdinalIgnoreCase),
                "We expected to be inside the App_WebReferences folder");

            // ... Determine the namespace to use, based on the directory structure where the .svcmap file
            //     is found.
            if (webReferencesRootVirtualPath.Length == virtualPath.Length)
            {
                Debug.Assert(string.Equals(webReferencesRootVirtualPath, virtualPath, StringComparison.OrdinalIgnoreCase),
                    "We expected to be in the App_WebReferences directory");

                // If it's the root WebReferences dir, use the empty namespace
                return String.Empty;
            }
            else
            {
                // We're in a subdirectory of App_WebReferences.
                // Get the directory's relative path from App_WebReferences, e.g. "Foo/Bar"

                virtualPath = VirtualPathUtility.RemoveTrailingSlash(virtualPath).Substring(webReferencesRootVirtualPath.Length);

                // Split it into chunks separated by '/'
                string[] chunks = virtualPath.Split('/');

                // Turn all the relevant chunks into valid namespace chunks
                for (int i = 0; i < chunks.Length; i++)
                {
                    chunks[i] = MakeValidTypeNameFromString(chunks[i]);
                }

                // Put the relevant chunks back together to form the namespace
                return String.Join(".", chunks);
            }
        }

        /// <summary>
        /// Returns the app domain's application virtual path [from HttpRuntime.AppDomainAppVPath].
        /// Includes trailing slash, e.g. "/MyApp/"
        /// </summary>
        private static string GetAppDomainAppVirtualPath()
        {
            string appVirtualPath = HttpRuntime.AppDomainAppVirtualPath;
            if (appVirtualPath == null)
            {
                Debug.Fail("Shouldn't get a null app virtual path from the app domain");
                throw new InvalidOperationException();
            }

            return VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.ToAbsolute(appVirtualPath));
        }

        /// <summary>
        /// Gets the virtual path to the application's App_WebReferences directory, e.g. "/MyApp/App_WebReferences/"
        /// </summary>
        private static string GetWebRefDirectoryVirtualPath()
        {
            return VirtualPathUtility.Combine(GetAppDomainAppVirtualPath(), WebRefDirectoryName + @"\");
        }

        /// <summary>
        /// Return a valid type name from a string by changing any character
        ///   that's not a letter or a digit to an '_'.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal static string MakeValidTypeNameFromString(string typeName)
        {
            if (String.IsNullOrEmpty(typeName))
                throw new ArgumentNullException("typeName");

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < typeName.Length; i++)
            {
                // Make sure it doesn't start with a digit (ASURT 31134)
                if (i == 0 && Char.IsDigit(typeName[0]))
                    sb.Append('_');

                if (Char.IsLetterOrDigit(typeName[i]))
                    sb.Append(typeName[i]);
                else
                    sb.Append('_');
            }

            // Identifier can't be a single underscore character
            string validTypeName = sb.ToString();
            if (validTypeName.Equals("_", StringComparison.Ordinal))
            {
                validTypeName = "__";
            }

            return validTypeName;
        }

        /// <summary>
        /// Get the appropriate tool configuration for this service reference.
        /// 
        /// If a reference.config file is present, the configuration object returned 
        /// will be the merged view of:
        /// 
        ///    Machine Config
        ///       ReferenceConfig
        ///       
        /// If not reference.config file is present, the configuration object returned
        /// will be a merged view of:
        ///     
        ///     Machine.config
        ///         web.config in application's physical path...
        ///         
        /// </summary>
        /// <param name="mapFile">SvcMapFile representing the service</param>
        /// <returns></returns>
        private System.Configuration.Configuration GetToolConfig(SvcMapFile mapFile, string mapFilePath)
        {
            string toolConfigFile = null;

            if (mapFile != null && mapFilePath != null)
            {
                foreach (ExtensionFile extensionFile in mapFile.Extensions)
                {
                    if (String.Equals(extensionFile.Name, TOOL_CONFIG_ITEM_NAME, StringComparison.Ordinal))
                    {
                        toolConfigFile = extensionFile.FileName;
                    }
                }
            }

            System.Web.Configuration.WebConfigurationFileMap fileMap;
            fileMap = new System.Web.Configuration.WebConfigurationFileMap();

            System.Web.Configuration.VirtualDirectoryMapping mapping;
            if (toolConfigFile != null)
            {
                //
                // If we've got a specific tool configuration to use, we better load that...
                //
                mapping = new System.Web.Configuration.VirtualDirectoryMapping(System.IO.Path.GetDirectoryName(mapFilePath), true, toolConfigFile);
            }
            else
            {
                //
                // Otherwise we fall back to the default web.config file...
                //
                mapping = new System.Web.Configuration.VirtualDirectoryMapping(HostingEnvironment.ApplicationPhysicalPath, true);
            }
            fileMap.VirtualDirectories.Add("/", mapping);

            return System.Web.Configuration.WebConfigurationManager.OpenMappedWebConfiguration(fileMap, "/", System.Web.Hosting.HostingEnvironment.SiteName);

        }


        /// <summary>
        /// Helper class to implement type resolution for the generator
        /// </summary>
        private class TypeResolver : IContractGeneratorReferenceTypeLoader
        {
            private System.Reflection.Assembly[] _referencedAssemblies;

            private IEnumerable<System.Reflection.Assembly> ReferencedAssemblies
            {
                get
                {
                    if (_referencedAssemblies == null)
                    {
                        System.Collections.ICollection referencedAssemblyCollection = BuildManager.GetReferencedAssemblies();
                        _referencedAssemblies = new System.Reflection.Assembly[referencedAssemblyCollection.Count];
                        referencedAssemblyCollection.CopyTo(_referencedAssemblies, 0);
                    }
                    return _referencedAssemblies;
                }
            }

            [SecuritySafeCritical]
            System.Type IContractGeneratorReferenceTypeLoader.LoadType(string typeName)
            {
                // If the type can't be resolved, we need an exception thrown (thus the true argument)
                //   so it can be reported as a build error.
                return BuildManager.GetType(typeName, true);
            }

            [SecuritySafeCritical]
            System.Reflection.Assembly IContractGeneratorReferenceTypeLoader.LoadAssembly(string assemblyName)
            {
                System.Reflection.AssemblyName assemblyToLookFor = new System.Reflection.AssemblyName(assemblyName);

                foreach (System.Reflection.Assembly assembly in ReferencedAssemblies)
                {
                    if (System.Reflection.AssemblyName.ReferenceMatchesDefinition(assemblyToLookFor, assembly.GetName()))
                    {
                        return assembly;
                    }
                }

                throw new System.IO.FileNotFoundException(String.Format(CultureInfo.CurrentCulture, WCFModelStrings.ReferenceGroup_FailedToLoadAssembly, assemblyName));
            }

            [SecuritySafeCritical]
            void IContractGeneratorReferenceTypeLoader.LoadAllAssemblies(out IEnumerable<System.Reflection.Assembly> loadedAssemblies, out IEnumerable<Exception> loadingErrors)
            {
                loadedAssemblies = ReferencedAssemblies;
                loadingErrors = new System.Exception[] {};   
            }

        }

        [SuppressMessage("Microsoft.Usage", "CA2302:FlagServiceProviders", Justification = "IServiceProvider implementation does not return anything.")]
        private class ImportExtensionServiceProvider : IServiceProvider
        {

            #region IServiceProvider Members

            [SecuritySafeCritical]
            public object GetService(Type serviceType)
            {
                // We don't currently provide any services to import extensions in the build provider context
                return null;
            }

            #endregion
        }

    }

}

