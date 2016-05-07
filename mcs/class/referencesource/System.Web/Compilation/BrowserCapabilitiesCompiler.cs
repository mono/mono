//------------------------------------------------------------------------------
// <copyright file="BrowserCapabilitiesCompiler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.CodeDom.Compiler;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Web.UI;
    using System.Xml;

    static class BrowserCapabilitiesCompiler {

        internal static readonly VirtualPath AppBrowsersVirtualDir =
            HttpRuntime.AppDomainAppVirtualPathObject.SimpleCombineWithDir(HttpRuntime.BrowsersDirectoryName);

        private const string browerCapabilitiesTypeName = "BrowserCapabilities";
        private const string browerCapabilitiesCacheKey = "__browserCapabilitiesCompiler";

        private static Type _browserCapabilitiesFactoryBaseType;

        private static BrowserCapabilitiesFactoryBase _browserCapabilitiesFactoryBaseInstance;
        private static object _lockObject = new object();
        internal static Assembly AspBrowserCapsFactoryAssembly { get; set; }

        static BrowserCapabilitiesCompiler() {
            Assembly assembly = null;
            String publicKeyToken = BrowserCapabilitiesCodeGenerator.BrowserCapAssemblyPublicKeyToken;

            // If the token file cannot be found, do not load the assembly.
            if (publicKeyToken != null) {
                try {
                    string version;

                    // If we are targeting previous versions, try loading the 2.0 version of ASP.BrowserCapsFactory
                    // (Dev10 bug 795509)
                    if (MultiTargetingUtil.IsTargetFramework40OrAbove) {
                        version = ThisAssembly.Version;
                    } else {
                        version = "2.0.0.0";
                    }
                    assembly = Assembly.Load("ASP.BrowserCapsFactory, Version=" + version + ", Culture=neutral, PublicKeyToken=" + publicKeyToken);
                    AspBrowserCapsFactoryAssembly = assembly;
                }
                catch (FileNotFoundException) {
                }
            }

            // fallback when assembly cannot be found.
            if((assembly == null) || (!(assembly.GlobalAssemblyCache))) {
                _browserCapabilitiesFactoryBaseType = typeof(System.Web.Configuration.BrowserCapabilitiesFactory);
            }
            else {
                _browserCapabilitiesFactoryBaseType = assembly.GetType("ASP.BrowserCapabilitiesFactory", true);
            }
        }

        internal static BrowserCapabilitiesFactoryBase BrowserCapabilitiesFactory {
            get {
                if(_browserCapabilitiesFactoryBaseInstance != null) {
                    return _browserCapabilitiesFactoryBaseInstance;
                }
                Type t = GetBrowserCapabilitiesType();
                lock(_lockObject) {
                    if(_browserCapabilitiesFactoryBaseInstance == null) {
                        if (t != null) {
                            _browserCapabilitiesFactoryBaseInstance =
                                (BrowserCapabilitiesFactoryBase)Activator.CreateInstance(t);
                        }
                    }
                }
                return _browserCapabilitiesFactoryBaseInstance;
            }
        }

        internal static Type GetBrowserCapabilitiesFactoryBaseType() {
            return _browserCapabilitiesFactoryBaseType;
        }

        internal static Type GetBrowserCapabilitiesType() {

            //Need to assert here to check directories and files
            InternalSecurityPermissions.Unrestricted.Assert();

            BuildResult result = null;

            try {
                // Try the cache first, and if it's not there, compile it
                result = BuildManager.GetBuildResultFromCache(browerCapabilitiesCacheKey);
                if (result == null) {
                    DateTime utcStart = DateTime.UtcNow;

                    VirtualDirectory directory = AppBrowsersVirtualDir.GetDirectory();

                    // first try if app browser dir exists
                    string physicalDir = HostingEnvironment.MapPathInternal(AppBrowsersVirtualDir);

                    /* DevDivBugs 173531
                     * For the App_Browsers scenario, we need to cache the generated browser caps processing 
                     * code. We need to add path dependency on all files so that changes to them will 
                     * invalidate the cache entry and cause recompilation. */
                    if (directory != null && Directory.Exists(physicalDir)) {
                        ArrayList browserFileList = new ArrayList();
                        ArrayList browserFileDependenciesList = new ArrayList();
                        bool hasCustomCaps = AddBrowserFilesToList(directory, browserFileList, false);
                        if (hasCustomCaps) {
                            AddBrowserFilesToList(directory, browserFileDependenciesList, true);
                        }
                        else {
                            browserFileDependenciesList = browserFileList;
                        }

                        if (browserFileDependenciesList.Count > 0) {
                            ApplicationBrowserCapabilitiesBuildProvider buildProvider = new ApplicationBrowserCapabilitiesBuildProvider();
                            foreach (string virtualPath in browserFileList) {
                                buildProvider.AddFile(virtualPath);
                            }
                            
                            BuildProvidersCompiler bpc = new BuildProvidersCompiler(null /*configPath*/,
                                BuildManager.GenerateRandomAssemblyName(BuildManager.AppBrowserCapAssemblyNamePrefix));
                            
                            bpc.SetBuildProviders(new SingleObjectCollection(buildProvider));
                            CompilerResults results = bpc.PerformBuild();
                            Assembly assembly = results.CompiledAssembly;
                            // Get the type we want from the assembly
                            Type t = assembly.GetType(
                                BaseCodeDomTreeGenerator.defaultNamespace + "." + ApplicationBrowserCapabilitiesCodeGenerator.FactoryTypeName);
                            // Cache it for next time
                            result = new BuildResultCompiledType(t);
                            result.VirtualPath = AppBrowsersVirtualDir;
                            result.AddVirtualPathDependencies(browserFileDependenciesList);

                            BuildManager.CacheBuildResult(browerCapabilitiesCacheKey, result, utcStart);
                        }
                    }
                }
            }
            finally {
                CodeAccessPermission.RevertAssert();
            }

            // Simply return the global factory type.
            if (result == null)
                return _browserCapabilitiesFactoryBaseType;

            // Return the compiled type
            return ((BuildResultCompiledType)result).ResultType;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification="Warning was suppressed for previous version of API (different overload) in FxCop project file.")]
        private static bool AddBrowserFilesToList(
            VirtualDirectory directory, IList list, bool doRecurse) {
            bool hasCustomCaps = false;

            foreach (VirtualFileBase fileBase in directory.Children) {

                // Recursive into subdirectories.
                if (fileBase.IsDirectory) {
                    if (doRecurse) {
                        AddBrowserFilesToList((VirtualDirectory)fileBase, list, true);
                    }
                    hasCustomCaps = true;
                    continue;
                }

                string extension = Path.GetExtension(fileBase.Name);
                if (StringUtil.EqualsIgnoreCase(extension, ".browser")) {
                    list.Add(fileBase.VirtualPath);
                }
            }
            return hasCustomCaps;
        }
    }

    internal class ApplicationBrowserCapabilitiesCodeGenerator : BrowserCapabilitiesCodeGenerator {
        internal const string FactoryTypeName = "ApplicationBrowserCapabilitiesFactory";
        private OrderedDictionary _browserOverrides;
        private OrderedDictionary _defaultBrowserOverrides;
        private BrowserCapabilitiesFactoryBase _baseInstance;
        private BuildProvider _buildProvider;

        internal ApplicationBrowserCapabilitiesCodeGenerator(BuildProvider buildProvider) {
            _browserOverrides = new OrderedDictionary();
            _defaultBrowserOverrides = new OrderedDictionary();
            _buildProvider = buildProvider;
        }

        internal override bool GenerateOverrides { get { return false; } }

        internal override string TypeName {
            get {
                return FactoryTypeName;
            }
        }

        public override void Create() {
            throw new NotSupportedException();
        }

        private static void AddStringToHashtable(OrderedDictionary table, object key, String content, bool before) {
            ArrayList list = (ArrayList)table[key];
            if (list == null) {
                list = new ArrayList(1);
                table[key] = list;
            }

            if (before) {
                list.Insert(0, content);
            }
            else {
                list.Add(content);
            }
        }

        private static string GetFirstItemFromKey(OrderedDictionary table, object key) {
            ArrayList list = (ArrayList)table[key];

            if (list != null && list.Count > 0) {
                return list[0] as String;
            }

            return null;
        }

        [SuppressMessage("Microsoft.Usage", "CA2303:FlagTypeGetHashCode", Justification = "BrowserDefinition is Internal type - okay")]
        internal override void HandleUnRecognizedParentElement(BrowserDefinition bd, bool isDefault)
        {
            // Use the valid type name so we can find the corresponding parent node.
            String parentName = bd.ParentName;
            int hashKey = bd.GetType().GetHashCode() ^ parentName.GetHashCode();

            // Add the refID in front of the list so they gets to be called first.
            if (isDefault) {
                AddStringToHashtable(_defaultBrowserOverrides, hashKey, bd.Name, bd.IsRefID);
            } else {
                AddStringToHashtable(_browserOverrides, hashKey, bd.Name, bd.IsRefID);
            }
        }

        //generate the code from the parsed BrowserDefinitionTree
        //compile it, and install it in the gac
        internal void GenerateCode(AssemblyBuilder assemblyBuilder) {
            
            ProcessBrowserFiles(true /*useVirtualPath*/, BrowserCapabilitiesCompiler.AppBrowsersVirtualDir.VirtualPathString);
            ProcessCustomBrowserFiles(true /*useVirtualPath*/, BrowserCapabilitiesCompiler.AppBrowsersVirtualDir.VirtualPathString);
            
            CodeCompileUnit ccu = new CodeCompileUnit();

            Debug.Assert(BrowserTree != null);
            ArrayList customTreeRoots = new ArrayList();
            for (int i = 0; i < CustomTreeNames.Count; i++) {
                customTreeRoots.Add((BrowserDefinition)(((BrowserTree)CustomTreeList[i])[CustomTreeNames[i]]));
            }

            // namespace ASP
            CodeNamespace cnamespace = new CodeNamespace(BaseCodeDomTreeGenerator.defaultNamespace);
            //GEN: using System;
            cnamespace.Imports.Add(new CodeNamespaceImport("System"));
            //GEN: using System.Web;
            cnamespace.Imports.Add(new CodeNamespaceImport("System.Web"));
            //GEN: using System.Web.Configuration;
            cnamespace.Imports.Add(new CodeNamespaceImport("System.Web.Configuration"));
            //GEN: using System.Reflection;
            cnamespace.Imports.Add(new CodeNamespaceImport("System.Reflection"));
            //GEN: class BrowserCapabilitiesFactory
            ccu.Namespaces.Add(cnamespace);

            Type baseType = BrowserCapabilitiesCompiler.GetBrowserCapabilitiesFactoryBaseType();

            CodeTypeDeclaration factoryType = new CodeTypeDeclaration();
            factoryType.Attributes = MemberAttributes.Private;
            factoryType.IsClass = true;
            factoryType.Name = TypeName;
            factoryType.BaseTypes.Add(new CodeTypeReference(baseType));
            cnamespace.Types.Add(factoryType);

            BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic;

            BrowserDefinition bd = null;
            //GEN: protected override object ConfigureBrowserCapabilities(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            method.ReturnType = new CodeTypeReference(typeof(void));
            method.Name = "ConfigureCustomCapabilities";
            CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression(typeof(NameValueCollection), "headers");
            method.Parameters.Add(cpde);
            cpde = new CodeParameterDeclarationExpression(typeof(HttpBrowserCapabilities), "browserCaps");
            method.Parameters.Add(cpde);
            factoryType.Members.Add(method);

            for (int i = 0; i < customTreeRoots.Count; i++) {
                GenerateSingleProcessCall((BrowserDefinition)customTreeRoots[i], method);
            }
            
            foreach (DictionaryEntry entry in _browserOverrides) {
                object key = entry.Key;
                BrowserDefinition firstBrowserDefinition = (BrowserDefinition)BrowserTree[GetFirstItemFromKey(_browserOverrides, key)];
                
                string parentName = firstBrowserDefinition.ParentName;
                
                //



                if ((!TargetFrameworkUtil.HasMethod(baseType, parentName + "ProcessBrowsers", flags)) ||
                    (!TargetFrameworkUtil.HasMethod(baseType, parentName + "ProcessGateways", flags))) {
                    String parentID = firstBrowserDefinition.ParentID;

                    if (firstBrowserDefinition != null) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Browser_parentID_Not_Found, parentID), firstBrowserDefinition.XmlNode);
                    } else {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Browser_parentID_Not_Found, parentID));
                    }
                }
               
                bool isBrowserDefinition = true;
                if (firstBrowserDefinition is GatewayDefinition) {
                    isBrowserDefinition = false;
                }

                //GenerateMethodsToOverrideBrowsers
                //Gen: protected override void Xxx_ProcessChildBrowsers(bool ignoreApplicationBrowsers, MNameValueCollection headers, HttpBrowserCapabilities browserCaps) ;

                string methodName = parentName + (isBrowserDefinition ? "ProcessBrowsers" : "ProcessGateways");
                CodeMemberMethod cmm = new CodeMemberMethod();
                cmm.Name = methodName;
                cmm.ReturnType = new CodeTypeReference(typeof(void));
                cmm.Attributes = MemberAttributes.Family | MemberAttributes.Override;

                if (isBrowserDefinition) {
                    cpde = new CodeParameterDeclarationExpression(typeof(bool), BrowserCapabilitiesCodeGenerator.IgnoreApplicationBrowserVariableName);
                    cmm.Parameters.Add(cpde);
                }
                cpde = new CodeParameterDeclarationExpression(typeof(NameValueCollection), "headers");
                cmm.Parameters.Add(cpde);
                cpde = new CodeParameterDeclarationExpression(typeof(HttpBrowserCapabilities), browserCapsVariable);
                cmm.Parameters.Add(cpde);

                factoryType.Members.Add(cmm);

                ArrayList overrides = (ArrayList)_browserOverrides[key];
                CodeStatementCollection statements = cmm.Statements;

                bool ignoreApplicationBrowsersVarRefGenerated = false;
                
                foreach (string browserID in overrides) {
                    bd = (BrowserDefinition)BrowserTree[browserID];
                    if (bd is GatewayDefinition || bd.IsRefID) {
                        GenerateSingleProcessCall(bd, cmm);
                    }
                    else {
                        if (!ignoreApplicationBrowsersVarRefGenerated) {
                            Debug.Assert(isBrowserDefinition);

                            // Gen: if (ignoreApplicationBrowsers) {
                            //      }
                            //      else {
                            //          ...
                            //      }
                            CodeConditionStatement istatement = new CodeConditionStatement();

                            istatement.Condition = new CodeVariableReferenceExpression(BrowserCapabilitiesCodeGenerator.IgnoreApplicationBrowserVariableName);

                            cmm.Statements.Add(istatement);
                            statements = istatement.FalseStatements;

                            ignoreApplicationBrowsersVarRefGenerated = true;
                        }
                        statements = GenerateTrackedSingleProcessCall(statements, bd, cmm);
                        if (_baseInstance == null) {
                            // If we are targeting 4.0 or using the ASP.BrowserCapsFactory assembly generated by
                            // aspnet_regbrowsers.exe, we can simply just instantiate the type.
                            // If not, then we need to use the type BrowserCapabilitiesFactory35 that contains code
                            // from the 2.0 version of BrowserCapabilitiesFactory. This is because "baseType" is the 4.0 type
                            // that contains the new 4.0 definitions.
                            // (Dev10 bug 795509)
                            if (MultiTargetingUtil.IsTargetFramework40OrAbove || 
                                baseType.Assembly == BrowserCapabilitiesCompiler.AspBrowserCapsFactoryAssembly) {
                                _baseInstance = (BrowserCapabilitiesFactoryBase)Activator.CreateInstance(baseType);
                            }
                            else {
                                _baseInstance = new BrowserCapabilitiesFactory35();
                            }
                        }
                        int parentDepth = (int)((Triplet)_baseInstance.InternalGetBrowserElements()[parentName]).Third;
                        AddBrowserToCollectionRecursive(bd, parentDepth + 1);
                    }
                }

            }

            foreach (DictionaryEntry entry in _defaultBrowserOverrides) {
                object key = entry.Key;

                BrowserDefinition firstDefaultBrowserDefinition = (BrowserDefinition)DefaultTree[GetFirstItemFromKey(_defaultBrowserOverrides, key)];
                string parentName = firstDefaultBrowserDefinition.ParentName;

                if (baseType.GetMethod("Default" + parentName + "ProcessBrowsers", flags) == null) {
                    String parentID = firstDefaultBrowserDefinition.ParentID;
                    if (firstDefaultBrowserDefinition != null) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.DefaultBrowser_parentID_Not_Found, parentID), firstDefaultBrowserDefinition.XmlNode);
                    }
                }

                string methodName = "Default" + parentName + "ProcessBrowsers";
                CodeMemberMethod cmm = new CodeMemberMethod();
                cmm.Name = methodName;
                cmm.ReturnType = new CodeTypeReference(typeof(void));
                cmm.Attributes = MemberAttributes.Family | MemberAttributes.Override;
                cpde = new CodeParameterDeclarationExpression(typeof(bool), BrowserCapabilitiesCodeGenerator.IgnoreApplicationBrowserVariableName);
                cmm.Parameters.Add(cpde);
                cpde = new CodeParameterDeclarationExpression(typeof(NameValueCollection), "headers");
                cmm.Parameters.Add(cpde);
                cpde = new CodeParameterDeclarationExpression(typeof(HttpBrowserCapabilities), browserCapsVariable);
                cmm.Parameters.Add(cpde);
                factoryType.Members.Add(cmm);

                ArrayList overrides = (ArrayList)_defaultBrowserOverrides[key];

                CodeConditionStatement istatement = new CodeConditionStatement();
                istatement.Condition = new CodeVariableReferenceExpression(BrowserCapabilitiesCodeGenerator.IgnoreApplicationBrowserVariableName);

                cmm.Statements.Add(istatement);
                CodeStatementCollection statements = istatement.FalseStatements;

                foreach(string browserID in overrides) {
                    bd = (BrowserDefinition)DefaultTree[browserID];
                    Debug.Assert(!(bd is GatewayDefinition));

                    if(bd.IsRefID) {
                        GenerateSingleProcessCall(bd, cmm, "Default");
                    }
                    else {
                        statements = GenerateTrackedSingleProcessCall(statements, bd, cmm, "Default");
                    }
                }
            }

            // Generate process method for the browser elements
            foreach (DictionaryEntry entry in BrowserTree) {
                bd = entry.Value as BrowserDefinition;
                Debug.Assert(bd != null);
                GenerateProcessMethod(bd, factoryType);
            }

            for (int i = 0; i < customTreeRoots.Count; i++) {
                foreach (DictionaryEntry entry in (BrowserTree)CustomTreeList[i]) {
                    bd = entry.Value as BrowserDefinition;
                    Debug.Assert(bd != null);
                    GenerateProcessMethod(bd, factoryType);
                }
            }

            // Generate process method for the default browser elements
            foreach (DictionaryEntry entry in DefaultTree) {
                bd = entry.Value as BrowserDefinition;
                Debug.Assert(bd != null);
                GenerateProcessMethod(bd, factoryType, "Default");
            }
            GenerateOverrideMatchedHeaders(factoryType);
            GenerateOverrideBrowserElements(factoryType);

            Assembly assembly = BrowserCapabilitiesCompiler.GetBrowserCapabilitiesFactoryBaseType().Assembly;
            assemblyBuilder.AddAssemblyReference(assembly, ccu);
            assemblyBuilder.AddCodeCompileUnit(_buildProvider, ccu);
        }

        internal override void ProcessBrowserNode(XmlNode node, BrowserTree browserTree) {
            if (node.Name == "defaultBrowser") {
                throw new ConfigurationErrorsException(SR.GetString(SR.Browser_Not_Allowed_InAppLevel, node.Name), node);
            }

            base.ProcessBrowserNode(node, browserTree);
        }
    }

    internal class ApplicationBrowserCapabilitiesBuildProvider : BuildProvider {

        private ApplicationBrowserCapabilitiesCodeGenerator _codeGenerator;

        internal ApplicationBrowserCapabilitiesBuildProvider() {
            _codeGenerator = new ApplicationBrowserCapabilitiesCodeGenerator(this);
        }

        internal void AddFile(string virtualPath) {
            String filePath = HostingEnvironment.MapPathInternal(virtualPath);
            _codeGenerator.AddFile(filePath);
        }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder) {
            _codeGenerator.GenerateCode(assemblyBuilder);
        }
    }
}
