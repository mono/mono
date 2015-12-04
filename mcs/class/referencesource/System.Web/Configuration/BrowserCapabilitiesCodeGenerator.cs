//------------------------------------------------------------------------------
// <copyright file="BrowserCapabilitiesCodeGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Configuration;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
#if !FEATURE_PAL
    using System.ServiceProcess;
#endif // !FEATURE_PAL
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;
    using System.Xml.Schema;

    using Microsoft.Build.Utilities;
    using Microsoft.CSharp;

    [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
    [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
    public class BrowserCapabilitiesCodeGenerator {
        private static readonly string _browsersDirectory;
        private static readonly string _publicKeyTokenFile;

        private static object _staticLock = new object();

        private BrowserTree _browserTree;
        private BrowserTree _defaultTree;
        private BrowserDefinitionCollection _browserDefinitionCollection;

        internal const string browserCapsVariable = "browserCaps";
        internal const string IgnoreApplicationBrowserVariableName = "ignoreApplicationBrowsers";
        private const string _factoryTypeName = "BrowserCapabilitiesFactory";
        private const string _headerDictionaryVarName = "_headerDictionary";
        private const string _disableOptimizedCacheKeyMethodName = "DisableOptimizedCacheKey";
        private const string _matchedHeadersMethodName = "PopulateMatchedHeaders";
        private const string _browserElementsMethodName = "PopulateBrowserElements";
        private const string _dictionaryRefName = "dictionary";
        private const string _regexWorkerRefName = "regexWorker";
        private const string _headersRefName = "headers";
        private const string _resultVarName = "result";
        private const string _processRegexMethod = "ProcessRegex";
        private static readonly string _strongNameKeyFileName = browserCapsVariable + ".snk";
        private static readonly string _publicKeyTokenFileName = browserCapsVariable + ".token";
        private static bool _publicKeyTokenLoaded;
        private static string _publicKeyToken;

        private CodeVariableReferenceExpression _dictionaryRefExpr = new CodeVariableReferenceExpression(_dictionaryRefName);
        private CodeVariableReferenceExpression _regexWorkerRefExpr = new CodeVariableReferenceExpression(_regexWorkerRefName);
        private CodeVariableReferenceExpression _headersRefExpr = new CodeVariableReferenceExpression(_headersRefName);
        private CodeVariableReferenceExpression _browserCapsRefExpr = new CodeVariableReferenceExpression(browserCapsVariable);

        private ArrayList _browserFileList;
        
        private ArrayList _customBrowserFileLists;
        private ArrayList _customTreeList;
        private ArrayList _customTreeNames;
        private ArrayList _customBrowserDefinitionCollections;

        private CaseInsensitiveStringSet _headers;

        static BrowserCapabilitiesCodeGenerator() {
#if !PLATFORM_UNIX // File system paths must account for UNIX
            _browsersDirectory = HttpRuntime.ClrInstallDirectoryInternal + "\\config\\browsers";
            _publicKeyTokenFile = _browsersDirectory + "\\" + _publicKeyTokenFileName;
#else // !PLATFORM_UNIX 
            _browsersDirectory = HttpRuntime.ClrInstallDirectoryInternal + "/config/browsers";
            _publicKeyTokenFile = _browsersDirectory + "/" + _publicKeyTokenFileName;

#endif // !PLATFORM_UNIX 
        }

        public BrowserCapabilitiesCodeGenerator() {
            _headers = new CaseInsensitiveStringSet();
        }

        internal BrowserTree BrowserTree {
            get {
                return _browserTree;
            }
        }

        internal BrowserTree DefaultTree {
            get {
                return _defaultTree;
            }
        }

        internal ArrayList CustomTreeList {
            get {
                return _customTreeList;
            }
        }

        internal ArrayList CustomTreeNames {
            get {
                return _customTreeNames;
            }
        }

        internal static string BrowserCapAssemblyPublicKeyToken {
            get {
                if (_publicKeyTokenLoaded) {
                    return _publicKeyToken;
                }

                lock (_staticLock) {
                    if (_publicKeyTokenLoaded) {
                        return _publicKeyToken;
                    }

                    string publicKeyTokenFile;
                    if (MultiTargetingUtil.IsTargetFramework40OrAbove) {
                        publicKeyTokenFile = _publicKeyTokenFile;
                    }
                    else {
                        // If we are targeting pre-4.0, we should be using version 2.0 of the assembly
                        // ASP.BrowserCapsFactory, so we need to read the token file from the 2.0 path.
                        // (Dev10 bug 795509)
                        string subPath = @"config\browsers\" + _publicKeyTokenFileName;
                        publicKeyTokenFile = ToolLocationHelper.GetPathToDotNetFrameworkFile(subPath, TargetDotNetFrameworkVersion.Version20);
                    }
                    _publicKeyToken = LoadPublicKeyTokenFromFile(publicKeyTokenFile);
                    _publicKeyTokenLoaded = true;

                    return _publicKeyToken;
                }
            }
        }

        internal virtual bool GenerateOverrides { get { return true; } }

        internal virtual string TypeName {
            get {
                return _factoryTypeName;
            }
        }

        internal void AddFile(string filePath) {
            if (_browserFileList == null)
                _browserFileList = new ArrayList();

            _browserFileList.Add(filePath);
        }

        internal void AddCustomFile(string filePath) {
            if (_customBrowserFileLists == null) {
                _customBrowserFileLists = new ArrayList();
            }
            
            _customBrowserFileLists.Add(filePath);
        }

        //parse the config info and create BrowserTree
        //then generate code for, compile, and gac the object
        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public virtual void Create() {
            DirectoryInfo browserDirInfo = new DirectoryInfo(_browsersDirectory);
            //get all the browser files and put them in the "tree"
            FileInfo[] browserFiles = browserDirInfo.GetFiles("*.browser");
            
            if (browserFiles == null || browserFiles.Length == 0) {
                return;
            }

            foreach(FileInfo browserFile in browserFiles) {
                AddFile(browserFile.FullName);
            }

            // First parse the browser files.
            ProcessBrowserFiles();

            // Then parse custom browser files.
            ProcessCustomBrowserFiles();

            // Uninstall previously installed generated assembly.
            Uninstall();

            //generate the source code, compile it, and gac it
            GenerateAssembly();

            // Restart w3svc service
            RestartW3SVCIfNecessary();
        }

        internal bool UninstallInternal() {
            // Remove existing strong name public token file
            if (File.Exists(_publicKeyTokenFile)) {
                File.Delete(_publicKeyTokenFile);
            }

            // Removing existing copy from GAC
            GacUtil gacutil = new GacUtil();
            bool assemblyRemoved = gacutil.GacUnInstall("ASP.BrowserCapsFactory, Version=" + ThisAssembly.Version + ", Culture=neutral");
            if (!assemblyRemoved) {
                return false;
            }

            return true;
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public bool Uninstall() {
            // Restart w3svc service
            RestartW3SVCIfNecessary();

            if (!UninstallInternal()) {
                return false;
            }

            // Restart w3svc service again so applications get a fresh copy.
            RestartW3SVCIfNecessary();

            return true;
        }

        private void RestartW3SVCIfNecessary() {
#if !FEATURE_PAL
            try {
                // Dev10 bug 734918
                // We should not fail when the w3svc service is not installed.
                ServiceController[] services = ServiceController.GetServices();
                ServiceController controller = services.SingleOrDefault(s => String.Equals(s.ServiceName, "W3SVC", StringComparison.OrdinalIgnoreCase)); 
                if (controller == null) {
                    return;
                }

                ServiceControllerStatus status = controller.Status;

                // Stop the service if it's not currently stopped or pending.
                if (!status.Equals(ServiceControllerStatus.Stopped) &&
                    !status.Equals(ServiceControllerStatus.StopPending) &&
                    !status.Equals(ServiceControllerStatus.StartPending)) {
                    controller.Stop();

                    // Give it 5 minutes to stop
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 5, 0));
                    controller.Start();

                    // If the service was paused, pause it.
                    if (status.Equals(ServiceControllerStatus.Paused) || status.Equals(ServiceControllerStatus.PausePending)) {
                        controller.Pause();
                    }
                }
            }
            catch (Exception ex) {
                throw new InvalidOperationException(SR.GetString(SR.Browser_W3SVC_Failure_Helper_Text, ex));
            }
#endif // !FEATURE_PAL
        }

        internal void ProcessBrowserFiles() {
            ProcessBrowserFiles(false, String.Empty);
        }

        private string NoPathFileName(string fullPath) {
            int lastSlash = fullPath.LastIndexOf("\\", StringComparison.Ordinal);
            if(lastSlash > -1) {
                return fullPath.Substring(lastSlash + 1);
            }
            return fullPath;
        }

        internal virtual void ProcessBrowserNode(XmlNode node, BrowserTree browserTree) {

            BrowserDefinition browserInfo = null;

            if (node.Name == "gateway") {
                browserInfo = new GatewayDefinition(node);
            }
            else if (node.Name == "browser") {
                browserInfo = new BrowserDefinition(node);
            }
            else {
                Debug.Assert(node.Name == "defaultBrowser");
                browserInfo = new BrowserDefinition(node, true);
            }

            BrowserDefinition oldNode = (BrowserDefinition)browserTree[browserInfo.Name];

            if (oldNode != null) {
                if (browserInfo.IsRefID) {
                    oldNode.MergeWithDefinition(browserInfo);
                }
                else {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Duplicate_browser_id, browserInfo.ID), node);
                }
            }
            else {
                browserTree[browserInfo.Name] = browserInfo;
            }
        }

        private void NormalizeAndValidateTree(BrowserTree browserTree, bool isDefaultBrowser) {
            NormalizeAndValidateTree(browserTree, isDefaultBrowser, false);
        }

        private void NormalizeAndValidateTree(BrowserTree browserTree, bool isDefaultBrowser, bool isCustomBrowser) {
            //normalize the tree
            foreach (DictionaryEntry entry in browserTree) {
                BrowserDefinition bd = (BrowserDefinition)entry.Value;
                string parentName = bd.ParentName;
                BrowserDefinition parentBrowser = null;

                if (IsRootNode(bd.Name)) {
                    continue;
                }

                if (parentName.Length > 0) {
                    parentBrowser = (BrowserDefinition)browserTree[parentName];
                }

                if (parentBrowser != null) {
                    if (bd.IsRefID) {
                        if (bd is GatewayDefinition) {
                            parentBrowser.RefGateways.Add(bd);
                        }
                        else {
                            parentBrowser.RefBrowsers.Add(bd);
                        }
                    }
                    else if (bd is GatewayDefinition) {
                        parentBrowser.Gateways.Add(bd);
                    }
                    else {
                        parentBrowser.Browsers.Add(bd);
                    }
                }
                else {
                    if (isCustomBrowser) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Browser_parentID_Not_Found, bd.ParentID), bd.XmlNode);
                    }
                    else {
                        HandleUnRecognizedParentElement(bd, isDefaultBrowser);
                    }
                }
            }

            //validate the tree
            //loop check
            foreach (DictionaryEntry entry in browserTree) {
                BrowserDefinition bd = (BrowserDefinition)entry.Value;
                Hashtable loopCheck = new Hashtable();
                BrowserDefinition currentBrowser = bd;
                string currentId = currentBrowser.Name;
                while (!IsRootNode(currentId)) {
                    if (loopCheck[currentId] != null) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Browser_Circular_Reference, currentId), currentBrowser.XmlNode);
                    }
                    loopCheck[currentId] = currentId;
                    currentBrowser = (BrowserDefinition)browserTree[currentBrowser.ParentName];
                    //in app-level, parent can exist in machine level
                    if (currentBrowser == null) {
                        break;
                    }

                    currentId = currentBrowser.Name;
                }
            }
        }

        private void SetCustomTreeRoots(BrowserTree browserTree, int index) {
            foreach (DictionaryEntry entry in browserTree) {
                BrowserDefinition bd = (BrowserDefinition)entry.Value;
                if (bd.ParentName == null) {
                    _customTreeNames[index] = bd.Name;
                    break;
                }
            }
        }      

        // Now that we support adding custom browser hierarchies, root nodes other than Default are permitted.
        private bool IsRootNode(string nodeName) {
            if (String.Compare(nodeName, "Default", StringComparison.OrdinalIgnoreCase) == 0)
                return true;

            foreach (string treeRootName in _customTreeNames) {
                if (String.Compare(nodeName, treeRootName, StringComparison.OrdinalIgnoreCase) == 0) {
                    return true;
                }
            }

            return false;
        }

        protected void ProcessBrowserFiles(bool useVirtualPath, string virtualDir) {
            _browserTree = new BrowserTree();
            _defaultTree = new BrowserTree();
            _customTreeNames = new ArrayList();

            if (_browserFileList == null) {
                _browserFileList = new ArrayList();
            }

            _browserFileList.Sort();
//#if OPTIMIZE_FOR_DESKTOP_BROWSER
            string mozillaFile = null;
            string ieFile = null;
            string operaFile = null;

            // DevDivBugs 180962
            // IE, Mozilla and Opera are first-class browsers. Their User-Agent profiles need to be compared to the UA profile
            // of the HTTP request before other browsers. We put them to the head of the list so that the generated browser capabilities 
            // code will try to match them before other browsers.
            foreach (String filePath in _browserFileList) {
                if (filePath.EndsWith("ie.browser", StringComparison.OrdinalIgnoreCase)) {
                    ieFile = filePath;
                }
                else if (filePath.EndsWith("mozilla.browser", StringComparison.OrdinalIgnoreCase)) {
                    mozillaFile = filePath;
                }
                else if (filePath.EndsWith("opera.browser", StringComparison.OrdinalIgnoreCase)) {
                    operaFile = filePath;
                    break;
                }
            }

            if (ieFile != null) {
                _browserFileList.Remove(ieFile);
                _browserFileList.Insert(0, ieFile);
            }

            if (mozillaFile != null) {
                _browserFileList.Remove(mozillaFile);
                _browserFileList.Insert(1, mozillaFile);
            }

            if (operaFile != null) {
                _browserFileList.Remove(operaFile);
                _browserFileList.Insert(2, operaFile);
            }
//#endif
            foreach (string fileName in _browserFileList) {
                XmlDocument doc = new ConfigXmlDocument();
                try {
                    doc.Load(fileName);

                    XmlNode rootNode = doc.DocumentElement;
                    if(rootNode.Name != "browsers") {
                        if(useVirtualPath) {
                            throw new HttpParseException(SR.GetString(SR.Invalid_browser_root), null /*innerException*/, virtualDir + "/" + NoPathFileName(fileName), null /*sourceCode*/, 1);
                        }
                        else {
                            throw new HttpParseException(SR.GetString(SR.Invalid_browser_root), null /*innerException*/, fileName, null /*sourceCode*/, 1);
                        }
                    }

                    foreach (XmlNode node in rootNode.ChildNodes) {
                        if (node.NodeType != XmlNodeType.Element)
                            continue;
                        if (node.Name == "browser" || node.Name == "gateway") { 
                            ProcessBrowserNode(node, _browserTree);
                        }
                        else if (node.Name == "defaultBrowser") {
                            ProcessBrowserNode(node, _defaultTree);
                        }
                        else {
                            HandlerBase.ThrowUnrecognizedElement(node);
                        }
                    }
                }
                catch (XmlException e) {
                    if(useVirtualPath) {
                        throw new HttpParseException(e.Message, null /*innerException*/, virtualDir + "/" + NoPathFileName(fileName), null /*sourceCode*/, e.LineNumber);
                    }
                    else {
                        throw new HttpParseException(e.Message, null /*innerException*/, fileName, null /*sourceCode*/, e.LineNumber);
                    }
                }
                catch (XmlSchemaException e) {
                    if(useVirtualPath) {
                        throw new HttpParseException(e.Message, null /*innerException*/, virtualDir + "/" + NoPathFileName(fileName), null /*sourceCode*/, e.LineNumber);
                    }
                    else {
                        throw new HttpParseException(e.Message, null /*innerException*/, fileName, null /*sourceCode*/, e.LineNumber);
                    }
                }
            }
            NormalizeAndValidateTree(_browserTree, false);
            NormalizeAndValidateTree(_defaultTree, true);

            BrowserDefinition defaultBrowser = (BrowserDefinition)_browserTree["Default"];

            if (defaultBrowser != null) {
                AddBrowserToCollectionRecursive(defaultBrowser, 0);
            }
        }

        internal void ProcessCustomBrowserFiles() {
            ProcessCustomBrowserFiles(false, String.Empty);
        }

        internal void ProcessCustomBrowserFiles(bool useVirtualPath, string virtualDir) {
            //get all custom browser files and put them in the "tree"
            DirectoryInfo browserDirInfo = null;
            DirectoryInfo[] browserSubDirectories = null;
            DirectoryInfo[] allBrowserSubDirectories = null;
            ArrayList customBrowserFileNames;
            _customTreeList = new ArrayList();
            _customBrowserFileLists = new ArrayList();
            _customBrowserDefinitionCollections = new ArrayList();

            /* Machine Level Custom Browsers */
            if (useVirtualPath == false) {
                browserDirInfo = new DirectoryInfo(_browsersDirectory);
            }
            /* Application Level Custom Browsers */
            else {
                browserDirInfo = new DirectoryInfo(HostingEnvironment.MapPathInternal(virtualDir));
            }

            allBrowserSubDirectories = browserDirInfo.GetDirectories();
            
            int j = 0;
            int length = allBrowserSubDirectories.Length;
            browserSubDirectories = new DirectoryInfo[length];
            for (int i = 0; i < length; i++) {
                if ((allBrowserSubDirectories[i].Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
                    browserSubDirectories[j] = allBrowserSubDirectories[i];
                    j++;
                }
            }
            Array.Resize(ref browserSubDirectories, j);

            for (int i = 0; i < browserSubDirectories.Length; i++) {
                /* Recursively Into Subdirectories */
                FileInfo[] browserFiles = GetFilesNotHidden(browserSubDirectories[i], browserDirInfo);

                if (browserFiles == null || browserFiles.Length == 0) {
                    continue;
                }
                BrowserTree customTree = new BrowserTree();
                _customTreeList.Add(customTree);
                _customTreeNames.Add(browserSubDirectories[i].Name);
                customBrowserFileNames = new ArrayList();

                foreach (FileInfo browserFile in browserFiles) {
                    customBrowserFileNames.Add(browserFile.FullName);
                }
                _customBrowserFileLists.Add(customBrowserFileNames);
            }
            for (int i = 0; i < _customBrowserFileLists.Count; i++) {
                ArrayList fileNames = (ArrayList)_customBrowserFileLists[i];
                foreach (string fileName in fileNames) {
                    XmlDocument doc = new ConfigXmlDocument();
                    try {
                        doc.Load(fileName);
 
                        XmlNode rootNode = doc.DocumentElement;
                        if (rootNode.Name != "browsers") {
                            if (useVirtualPath) {
                                throw new HttpParseException(SR.GetString(SR.Invalid_browser_root), null /*innerException*/, virtualDir + "/" + NoPathFileName(fileName), null /*sourceCode*/, 1);
                            }
                            else {
                                throw new HttpParseException(SR.GetString(SR.Invalid_browser_root), null /*innerException*/, fileName, null /*sourceCode*/, 1);
                            }
                        }
                        foreach (XmlNode node in rootNode.ChildNodes) {
                            if (node.NodeType != XmlNodeType.Element) {
                                continue;
                            }
                            if (node.Name == "browser" || node.Name == "gateway") {
                                ProcessBrowserNode(node, (BrowserTree)_customTreeList[i]);
                            }
                            else {
                                HandlerBase.ThrowUnrecognizedElement(node);
                            }
                        }
                    }
                    catch (XmlException e) {
                        if (useVirtualPath) {
                            throw new HttpParseException(e.Message, null /*innerException*/, virtualDir + "/" + NoPathFileName(fileName), null /*sourceCode*/, e.LineNumber);
                        }
                        else {
                            throw new HttpParseException(e.Message, null /*innerException*/, fileName, null /*sourceCode*/, e.LineNumber);
                        }
                    }
                    catch (XmlSchemaException e) {
                        if (useVirtualPath) {
                            throw new HttpParseException(e.Message, null /*innerException*/, virtualDir + "/" + NoPathFileName(fileName), null /*sourceCode*/, e.LineNumber);
                        }
                        else {
                            throw new HttpParseException(e.Message, null /*innerException*/, fileName, null /*sourceCode*/, e.LineNumber);
                        }
                    }
                }
                SetCustomTreeRoots((BrowserTree)_customTreeList[i], i);
                NormalizeAndValidateTree((BrowserTree)_customTreeList[i], false, true);
                _customBrowserDefinitionCollections.Add(new BrowserDefinitionCollection());
                AddCustomBrowserToCollectionRecursive((BrowserDefinition)(((BrowserTree)_customTreeList[i])[_customTreeNames[i]]), 0, i);
            }
        }

        internal void AddCustomBrowserToCollectionRecursive(BrowserDefinition bd, int depth, int index) {
            if(_customBrowserDefinitionCollections[index] == null) {
                _customBrowserDefinitionCollections[index] = new BrowserDefinitionCollection();
            }
            bd.Depth = depth;
            bd.IsDeviceNode = true;
            ((BrowserDefinitionCollection)_customBrowserDefinitionCollections[index]).Add(bd);

            foreach (BrowserDefinition childBrowser in bd.Browsers) {
                AddCustomBrowserToCollectionRecursive(childBrowser, depth + 1, index);
            }
        }

        internal void AddBrowserToCollectionRecursive(BrowserDefinition bd, int depth) {
            if (_browserDefinitionCollection == null) {
                _browserDefinitionCollection = new BrowserDefinitionCollection();
            }

            bd.Depth = depth;
            bd.IsDeviceNode = true;
            _browserDefinitionCollection.Add(bd);

            foreach(BrowserDefinition childBrowser in bd.Browsers) {
                AddBrowserToCollectionRecursive(childBrowser, depth + 1);
            }
        }

        internal virtual void HandleUnRecognizedParentElement(BrowserDefinition bd, bool isDefault) {
            throw new ConfigurationErrorsException(SR.GetString(SR.Browser_parentID_Not_Found, bd.ParentID), bd.XmlNode);
        }
        
        private static FileInfo[] GetFilesNotHidden(DirectoryInfo rootDirectory, DirectoryInfo browserDirInfo) {
            ArrayList fileList = new ArrayList();
            FileInfo[] files;
            DirectoryInfo[] subDirectories = rootDirectory.GetDirectories("*", SearchOption.AllDirectories);
            
            files = rootDirectory.GetFiles("*.browser", SearchOption.TopDirectoryOnly);
            fileList.AddRange(files);
            for (int i = 0; i < subDirectories.Length; i++) {
                if ((HasHiddenParent(subDirectories[i], browserDirInfo) == false)) {
                    files = subDirectories[i].GetFiles("*.browser", SearchOption.TopDirectoryOnly);
                    fileList.AddRange(files);
                }
            }
            return ((FileInfo [])fileList.ToArray(typeof(FileInfo)));
        } 
        
        private static bool HasHiddenParent(DirectoryInfo directory, DirectoryInfo browserDirInfo) {
            while(!String.Equals(directory.Parent.Name, browserDirInfo.Name)) {
                if ((directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) {
                    return true;
                }
                directory = directory.Parent;
            }
            return false;
        }

        //generate the code from the parsed BrowserDefinitionTree
        //compile it, and install it in the gac
        private void GenerateAssembly() {
            Debug.Assert(_browserTree != null);
            BrowserDefinition root = (BrowserDefinition)_browserTree["Default"];
            BrowserDefinition defaultRoot = (BrowserDefinition)_defaultTree["Default"];
            ArrayList customTreeRoots = new ArrayList();
            for (int i = 0; i < _customTreeNames.Count; i++) {
                customTreeRoots.Add((BrowserDefinition)(((BrowserTree)_customTreeList[i])[_customTreeNames[i]]));
            }

            //create a CodeCompileUnit
            //add a CodeNamespace object to the CodeCompileUnit
            //add a CodeTypeDeclaration to the CodeNamespace
            //add all the members of the type/class to the CodeTypeDeclaration
            //use a CodeGenerator to generate code from the CodeCompileUnit
            //a CodeDomProvider can provide a CodeGenerator

            //translate the BrowserDefinition tree to code
            CSharpCodeProvider cscp = new CSharpCodeProvider();

            // namespace System.Web.BrowserCapsFactory
            CodeCompileUnit ccu = new CodeCompileUnit();

            //add strong-name key pair for
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(
                "System.Reflection.AssemblyKeyFile",
                new CodeAttributeArgument[] {
                    new CodeAttributeArgument(new CodePrimitiveExpression(_strongNameKeyFileName))});

            CodeAttributeDeclaration aptca = new CodeAttributeDeclaration(
                "System.Security.AllowPartiallyTrustedCallers");
            ccu.AssemblyCustomAttributes.Add(aptca);

            ccu.AssemblyCustomAttributes.Add(declaration);
            //add version number for it so it can distinguished in future versions
            declaration = new CodeAttributeDeclaration(
                "System.Reflection.AssemblyVersion",
                new CodeAttributeArgument[] {
                    new CodeAttributeArgument(new CodePrimitiveExpression(ThisAssembly.Version))});
            ccu.AssemblyCustomAttributes.Add(declaration);

            CodeNamespace cnamespace = new CodeNamespace("ASP");
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

            CodeTypeDeclaration factoryType = new CodeTypeDeclaration("BrowserCapabilitiesFactory");
            factoryType.Attributes = MemberAttributes.Private;
            factoryType.IsClass = true;
            factoryType.Name = TypeName;
            factoryType.BaseTypes.Add(new CodeTypeReference("System.Web.Configuration.BrowserCapabilitiesFactoryBase"));
            cnamespace.Types.Add(factoryType);

            //GEN: protected override object ConfigureBrowserCapabilities(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            method.ReturnType = new CodeTypeReference(typeof(void));
            method.Name = "ConfigureBrowserCapabilities";

            CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression(typeof(NameValueCollection), _headersRefName);
            method.Parameters.Add(cpde);
            cpde = new CodeParameterDeclarationExpression(typeof(HttpBrowserCapabilities), browserCapsVariable);
            method.Parameters.Add(cpde);
            factoryType.Members.Add(method);

            GenerateSingleProcessCall(root, method);
            
            for (int i = 0; i < customTreeRoots.Count; i++) {
                GenerateSingleProcessCall((BrowserDefinition)customTreeRoots[i], method);
            }

            //GEN: if(this.IsBrowserUnknown(browserCaps) == false) return;            
            CodeConditionStatement istatement = new CodeConditionStatement();

            CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "IsBrowserUnknown");
            cmie.Parameters.Add(_browserCapsRefExpr);
            istatement.Condition = new CodeBinaryOperatorExpression(cmie, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
            istatement.TrueStatements.Add(new CodeMethodReturnStatement());
            method.Statements.Add(istatement);

            if(defaultRoot != null) {
                GenerateSingleProcessCall(defaultRoot, method, "Default");
            }

            for (int i = 0; i < customTreeRoots.Count; i++) {
                foreach (DictionaryEntry entry in (BrowserTree)_customTreeList[i]) {
                    BrowserDefinition bd = entry.Value as BrowserDefinition;
                    Debug.Assert(bd != null);
                    GenerateProcessMethod(bd, factoryType);
                }
            }

            //GenerateCallsToProcessMethods(root, method);
            foreach (DictionaryEntry entry in _browserTree) {
                BrowserDefinition bd = entry.Value as BrowserDefinition;
                Debug.Assert(bd != null);
                GenerateProcessMethod(bd, factoryType);
            }

            foreach (DictionaryEntry entry in _defaultTree) {
                BrowserDefinition bd = entry.Value as BrowserDefinition;
                Debug.Assert(bd != null);
                GenerateProcessMethod(bd, factoryType, "Default");
            }

            GenerateOverrideMatchedHeaders(factoryType);
            GenerateOverrideBrowserElements(factoryType);

            //TODO: don't actually generate the code, just compile it in memory
            TextWriter twriter = new StreamWriter(new FileStream(_browsersDirectory + "\\BrowserCapsFactory.cs", FileMode.Create));
            try {
                cscp.GenerateCodeFromCompileUnit(ccu, twriter, null);
            }
            finally {
                if(twriter != null)
                    twriter.Close();
            }

            CompilationSection compConfig = MTConfigUtil.GetCompilationAppConfig();

            bool debug = compConfig.Debug;

#if !PLATFORM_UNIX // File system paths must account for UNIX
            string strongNameFile = _browsersDirectory + "\\" + _strongNameKeyFileName;
#else // !PLATFORM_UNIX
            string strongNameFile = _browsersDirectory + "/" + _strongNameKeyFileName;
#endif // !PLATFORM_UNIX

            // Generate strong name file
            StrongNameUtility.GenerateStrongNameFile(strongNameFile);

            //TODO: do not use interim file:  CompileAssemblyFromDom instead
            string[] referencedAssemblies = new string[2] { "System.dll", "System.Web.dll" };
            CompilerParameters compilerParameters = new CompilerParameters(referencedAssemblies, "ASP.BrowserCapsFactory", debug /* includeDebugInformation */ );
            compilerParameters.GenerateInMemory = false;
            compilerParameters.OutputAssembly = _browsersDirectory + "\\ASP.BrowserCapsFactory.dll";
            CompilerResults results = null;

            try {
                results = cscp.CompileAssemblyFromFile(compilerParameters, _browsersDirectory + "\\BrowserCapsFactory.cs");
            }
            finally {
                if (File.Exists(strongNameFile)) {
                    File.Delete(strongNameFile);
                }
            }

            if (results.NativeCompilerReturnValue != 0 || results.Errors.HasErrors) {
                foreach (CompilerError error in results.Errors) {
                    if (!error.IsWarning) {
                        throw new HttpCompileException(error.ErrorText);
                    }
                }

                throw new HttpCompileException(SR.GetString(SR.Browser_compile_error));
            }

            Assembly resultAssembly = results.CompiledAssembly;

            GacUtil gacutil = new GacUtil();
            gacutil.GacInstall(resultAssembly.Location);

            SavePublicKeyTokenFile(_publicKeyTokenFile, resultAssembly.GetName().GetPublicKeyToken());
        }

        private void SavePublicKeyTokenFile(string filename, byte[] publicKeyToken) {
            using (FileStream pktStream = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
                using (StreamWriter pktWriter = new StreamWriter(pktStream)) {
                    foreach (byte b in publicKeyToken) {
                        pktWriter.Write("{0:X2}", b);
                    }
                }
            }
        }

        private static string LoadPublicKeyTokenFromFile(string filename) {
            IStackWalk fileReadAccess = InternalSecurityPermissions.FileReadAccess(filename);
            Debug.Assert(fileReadAccess != null);
            fileReadAccess.Assert();
            if (!File.Exists(filename)) {
                return null;
            }

            try {
                using (FileStream pktStream = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
                    using (StreamReader pktReader = new StreamReader(pktStream)) {
                        return pktReader.ReadLine();
                    }
                }
            }
            catch (IOException) {
                if (HttpRuntime.HasFilePermission(filename)) {
                    throw;
                }

                // Don't throw exception if we don't have permission to the file.
                return null;
            }
            finally {
                CodeAccessPermission.RevertAssert();
            }
        }

        internal void GenerateOverrideBrowserElements(CodeTypeDeclaration typeDeclaration) {

            // Don't generate the property if there's nothing to override.
            if (_browserDefinitionCollection == null) {
                return;
            }

            // GEN:
            // protected override void PopulateBrowserElements(IDictionary dictionary) {
            //     dictionary["Default"] = new Triplet(null, "default description", 0 /*depth_of_node */);
            //     dictionary["Up"] = new Triplet("Default", "up Description", 1 /*depth_of_node */);
            // }
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = _browserElementsMethodName;
            method.Attributes = MemberAttributes.Override | MemberAttributes.Family;
            method.ReturnType = new CodeTypeReference(typeof(void));
            CodeParameterDeclarationExpression parameter =
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(IDictionary)), _dictionaryRefName);

            method.Parameters.Add(parameter);
            typeDeclaration.Members.Add(method);

            CodeMethodReferenceExpression baseMethod = new CodeMethodReferenceExpression(new CodeBaseReferenceExpression(), _browserElementsMethodName);
            CodeMethodInvokeExpression baseInvoke = new CodeMethodInvokeExpression(baseMethod, new CodeExpression[] { _dictionaryRefExpr });
            method.Statements.Add(baseInvoke);

            foreach(BrowserDefinition bd in _browserDefinitionCollection) {
                if (!bd.IsDeviceNode)
                    continue;

                Debug.Assert(!(bd is GatewayDefinition));

                CodeAssignStatement cas = new CodeAssignStatement();
                cas.Left = new CodeIndexerExpression(_dictionaryRefExpr, new CodeExpression[] {
                                                                             new CodePrimitiveExpression(bd.ID)
                                                                          });
                cas.Right = new CodeObjectCreateExpression(typeof(Triplet), 
                    new CodeExpression[] {
                        new CodePrimitiveExpression(bd.ParentName),
                        new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(String)), "Empty"),
                        new CodePrimitiveExpression(bd.Depth)});

                method.Statements.Add(cas);                
            }

            for (int i = 0; i < _customTreeNames.Count; i++) {
                foreach (BrowserDefinition bd in (BrowserDefinitionCollection)_customBrowserDefinitionCollections[i]) {
                    if (!bd.IsDeviceNode)
                        continue;

                    Debug.Assert(!(bd is GatewayDefinition));

                    CodeAssignStatement cas = new CodeAssignStatement();
                    cas.Left = new CodeIndexerExpression(_dictionaryRefExpr, new CodeExpression[] {
                                                                             new CodePrimitiveExpression(bd.ID)
                                                                           });
                    cas.Right = new CodeObjectCreateExpression(typeof(Triplet),
                        new CodeExpression[] {
                        new CodePrimitiveExpression(bd.ParentName),
                        new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(String)), "Empty"),
                        new CodePrimitiveExpression(bd.Depth)});

                    method.Statements.Add(cas);
                }
            }
        }

        internal void GenerateOverrideMatchedHeaders(CodeTypeDeclaration typeDeclaration) {
            // GEN:
            // protected override void PopulateMatchedHeaders(IDictionary dictionary) {
            //     base.PopulateMatchedHeaders(dictionary);
            //
            //     dictionary["header0"] = null;
            //     dictionary["header1"] = null;
            // }
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = _matchedHeadersMethodName;
            method.Attributes = MemberAttributes.Override | MemberAttributes.Family;
            method.ReturnType = new CodeTypeReference(typeof(void));
            CodeParameterDeclarationExpression parameter =
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(IDictionary)), _dictionaryRefName);

            method.Parameters.Add(parameter);
            typeDeclaration.Members.Add(method);

            CodeMethodReferenceExpression baseMethod = new CodeMethodReferenceExpression(new CodeBaseReferenceExpression(), _matchedHeadersMethodName);
            CodeMethodInvokeExpression baseInvoke = new CodeMethodInvokeExpression(baseMethod, new CodeExpression[] { _dictionaryRefExpr });
            method.Statements.Add(baseInvoke);

            foreach(String header in _headers) {
                CodeAssignStatement cas = new CodeAssignStatement();
                cas.Left = new CodeIndexerExpression(_dictionaryRefExpr, new CodeExpression[] {
                                                                             new CodePrimitiveExpression(header)
                                                                          });
                cas.Right = new CodePrimitiveExpression(null);

                method.Statements.Add(cas);
            }
        }

        internal void GenerateProcessMethod(BrowserDefinition bd, CodeTypeDeclaration ctd) {
            GenerateProcessMethod(bd, ctd, String.Empty);
        }

        //generate the xxxProcess method for an individual BrowserDefinition
        internal void GenerateProcessMethod(BrowserDefinition bd, CodeTypeDeclaration ctd, string prefix) {
            //GEN: internal bool XxxProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
            CodeMemberMethod cmm = new CodeMemberMethod();
            cmm.Name = prefix + bd.Name + "Process";
            cmm.ReturnType = new CodeTypeReference(typeof(bool));
            cmm.Attributes = MemberAttributes.Private;
            CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression(typeof(NameValueCollection), _headersRefName);
            cmm.Parameters.Add(cpde);
            cpde = new CodeParameterDeclarationExpression(typeof(HttpBrowserCapabilities), browserCapsVariable);
            cmm.Parameters.Add(cpde);

            bool regexWorkerGenerated = false;

            GenerateIdentificationCode(bd, cmm, ref regexWorkerGenerated);
            GenerateCapturesCode(bd, cmm, ref regexWorkerGenerated);
            GenerateSetCapabilitiesCode(bd, cmm, ref regexWorkerGenerated);
            GenerateSetAdaptersCode(bd, cmm);

            // Only add the browser node to the browser collection if it represents a device.
            if (bd.IsDeviceNode) {
                Debug.Assert(!(bd is GatewayDefinition));

                //GEN: browserCaps.AddBrowser("xxx");
                CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(browserCapsVariable), "AddBrowser");
                cmie.Parameters.Add(new CodePrimitiveExpression(bd.ID));
                cmm.Statements.Add(cmie);
            }

            // Generate ref gateway elements
            foreach (BrowserDefinition b in bd.RefGateways) {
                AddComment("ref gateways, parent=" + bd.ID, cmm);
                GenerateSingleProcessCall(b, cmm);
            }

            if ((GenerateOverrides) && (prefix.Length == 0)) {
                //Gen: protected virtual void XxxProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps) ;
                string methodName = prefix + bd.Name + "ProcessGateways";
                GenerateChildProcessMethod(methodName, ctd, false);

                //Gen: XxxProcessGateways(headers,  browserCaps) ;
                GenerateChildProcessInvokeExpression(methodName, cmm, false);
            }

            foreach(BrowserDefinition b in bd.Gateways) {
                AddComment("gateway, parent=" + bd.ID, cmm);
                GenerateSingleProcessCall(b, cmm);
            }

            if (GenerateOverrides) {
                //GEN: bool ignoreApplicationBrowsers = true | false; //bd.Browsers.Count != 0
                CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement(typeof(bool),
                    IgnoreApplicationBrowserVariableName, new CodePrimitiveExpression(bd.Browsers.Count != 0));
                cmm.Statements.Add(cvds);
            }

            if (bd.Browsers.Count > 0) {
                CodeStatementCollection statements = cmm.Statements;
                AddComment("browser, parent=" + bd.ID, cmm);
                foreach (BrowserDefinition b in bd.Browsers) {
                    statements = GenerateTrackedSingleProcessCall(statements, b, cmm, prefix);
                }

                if (GenerateOverrides) {
                    //GEN: ignoreApplicationBrowsers = false;
                    CodeAssignStatement codeAssignStmt = new CodeAssignStatement();
                    codeAssignStmt.Left = new CodeVariableReferenceExpression(IgnoreApplicationBrowserVariableName);
                    codeAssignStmt.Right = new CodePrimitiveExpression(false);
                    statements.Add(codeAssignStmt);
                }
            }

            // Generate ref browser 
            foreach (BrowserDefinition b in bd.RefBrowsers) {
                AddComment("ref browsers, parent=" + bd.ID, cmm);
                if (b.IsDefaultBrowser) {
                    GenerateSingleProcessCall(b, cmm, "Default");
                }
                else {
                    GenerateSingleProcessCall(b, cmm);
                }
            }

            if (GenerateOverrides) {
                //Gen: protected virtual void XxxProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps) ;
                string methodName = prefix + bd.Name + "ProcessBrowsers";
                GenerateChildProcessMethod(methodName, ctd, true);

                //Gen: XxxProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
                GenerateChildProcessInvokeExpression(methodName, cmm, true);
            }

            //GEN: return true;
            CodeMethodReturnStatement cmrs = new CodeMethodReturnStatement(new CodePrimitiveExpression(true));
            cmm.Statements.Add(cmrs);

            ctd.Members.Add(cmm);
        }

        private void GenerateChildProcessInvokeExpression(string methodName, CodeMemberMethod cmm, bool generateTracker) {
            //Gen: XxxProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps) ;
            CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), methodName);

            if (generateTracker) {
                expr.Parameters.Add(new CodeVariableReferenceExpression(IgnoreApplicationBrowserVariableName));
            }
            expr.Parameters.Add(new CodeVariableReferenceExpression(_headersRefName));
            expr.Parameters.Add(new CodeVariableReferenceExpression(browserCapsVariable));

            cmm.Statements.Add(expr);
        }

        private void GenerateChildProcessMethod(string methodName, CodeTypeDeclaration ctd, bool generateTracker) {
            //Gen: protected virtual void XxxProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps) ;
            CodeMemberMethod cmm= new CodeMemberMethod();
            cmm.Name = methodName;
            cmm.ReturnType = new CodeTypeReference(typeof(void));
            cmm.Attributes = MemberAttributes.Family;
            CodeParameterDeclarationExpression cpde = null;

            if (generateTracker) {
                cpde = new CodeParameterDeclarationExpression(typeof(bool), IgnoreApplicationBrowserVariableName);
                cmm.Parameters.Add(cpde);
            }

            cpde = new CodeParameterDeclarationExpression(typeof(NameValueCollection), _headersRefName);
            cmm.Parameters.Add(cpde);
            cpde = new CodeParameterDeclarationExpression(typeof(HttpBrowserCapabilities), browserCapsVariable);
            cmm.Parameters.Add(cpde);

            ctd.Members.Add(cmm);
        }

        private void GenerateRegexWorkerIfNecessary(CodeMemberMethod cmm, ref bool regexWorkerGenerated) {
            if (regexWorkerGenerated) {
                return;
            }

            regexWorkerGenerated = true;

            //GEN: RegexWorker regexWorker;
            cmm.Statements.Add(new CodeVariableDeclarationStatement("RegexWorker", _regexWorkerRefName));

            //GEN: regexWorker = new RegexWorker(browserCaps);
            cmm.Statements.Add(new CodeAssignStatement(_regexWorkerRefExpr, new CodeObjectCreateExpression("RegexWorker", _browserCapsRefExpr)));
        }

        private void ReturnIfHeaderValueEmpty(CodeMemberMethod cmm, CodeVariableReferenceExpression varExpr) {
            //  GEN: if(String.IsNullOrEmpty(varExpr)) {
            //  GEN:     return false;
            //  GEN: }
            CodeConditionStatement emptyCheckStmt = new CodeConditionStatement();
            CodeMethodReferenceExpression emptyCheckMethod = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(String)), "IsNullOrEmpty");
            CodeMethodInvokeExpression emptyCheckExpr = new CodeMethodInvokeExpression(emptyCheckMethod, varExpr);

            emptyCheckStmt.Condition = emptyCheckExpr;
            emptyCheckStmt.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
            cmm.Statements.Add(emptyCheckStmt);
        }

        //generate part of the xxxProcess method for handling determining if the requesting
        //browser meets the regexes for this browser
        private void GenerateIdentificationCode(BrowserDefinition bd, CodeMemberMethod cmm, ref bool regexWorkerGenerated) {

            //GEN: IDictionary dictionary;
            cmm.Statements.Add(new CodeVariableDeclarationStatement(typeof(IDictionary), _dictionaryRefName));

            //GEN: dictionary = browserCaps.Capabilities;
            CodeAssignStatement assign = new CodeAssignStatement(
                _dictionaryRefExpr,
                new CodePropertyReferenceExpression(_browserCapsRefExpr, "Capabilities")
                );
            cmm.Statements.Add(assign);

            bool disableOptimizedKey = false;
            CodeVariableReferenceExpression result = null;
            CodeVariableReferenceExpression headerValue = null;

            if(bd.IdHeaderChecks.Count > 0) {
                AddComment("Identification: check header matches", cmm);
                for (int i = 0; i < bd.IdHeaderChecks.Count; i++) {
                    string matchedString = ((CheckPair)bd.IdHeaderChecks[i]).MatchString;

                    // Skip matching ".*"
                    if (matchedString.Equals(".*")) {
                        continue;
                    }

                    if (headerValue == null) {
                        headerValue = GenerateVarReference(cmm, typeof(string), "headerValue");
                    }

                    CodeAssignStatement valueAssignment = new CodeAssignStatement();
                    cmm.Statements.Add(valueAssignment);
                    valueAssignment.Left = headerValue;

                    if (((CheckPair)bd.IdHeaderChecks[i]).Header.Equals("User-Agent")) {
                        _headers.Add(String.Empty);

                        // GEN: headerValue = ((string)(browserCaps[String.Empty]));
                        valueAssignment.Right = new CodeCastExpression(typeof(string),
                                                new CodeIndexerExpression(
                                                    new CodeVariableReferenceExpression(browserCapsVariable),
                                                    new CodeExpression[] { 
                                                        new CodePropertyReferenceExpression(
                                                        new CodeTypeReferenceExpression(typeof(String)), "Empty") }));
                    }
                    else {
                        string header = ((CheckPair)bd.IdHeaderChecks[i]).Header;
                        _headers.Add(header);

                        //GEN: headerValue = ((String)headers["xxx"]);
                        valueAssignment.Right = new CodeCastExpression(typeof(string),
                                                   new CodeIndexerExpression(
                                                       _headersRefExpr,
                                                       new CodeExpression[] { new CodePrimitiveExpression(header) }
                                                       )
                                                   );

                        disableOptimizedKey = true;
                    }

                    // Don't need to use Regex if matching . only.
                    if (matchedString.Equals(".")) {

                        // Simply return if the header exists.
                        ReturnIfHeaderValueEmpty(cmm, headerValue);

                        continue;
                    }

                    if (result == null) {
                        result = GenerateVarReference(cmm, typeof(bool), _resultVarName);
                    }

                    GenerateRegexWorkerIfNecessary(cmm, ref regexWorkerGenerated);
                    CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression(_regexWorkerRefExpr, _processRegexMethod);

                    cmie.Parameters.Add(headerValue);
                    cmie.Parameters.Add(new CodePrimitiveExpression(matchedString));

                    //GEN: result = regexWorker.ProcessRegex(headerValue, {matchedString});
                    assign = new CodeAssignStatement();
                    assign.Left = result;
                    assign.Right = cmie;
                    cmm.Statements.Add(assign);

                    //GEN: if(result == false) {
                    //GEN:     return false;
                    //GEN: }
                    CodeConditionStatement istatement = new CodeConditionStatement();
                    if(((CheckPair)bd.IdHeaderChecks[i]).NonMatch) {
                        istatement.Condition = new CodeBinaryOperatorExpression(result, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(true));
                    }
                    else {
                        istatement.Condition = new CodeBinaryOperatorExpression(result, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
                    }
                    istatement.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
                    cmm.Statements.Add(istatement);
                }
            }

            if (bd.IdCapabilityChecks.Count > 0) {
                AddComment("Identification: check capability matches", cmm);
                for (int i = 0; i < bd.IdCapabilityChecks.Count; i++) {
                    string matchedString = ((CheckPair)bd.IdCapabilityChecks[i]).MatchString;

                    // Skip matching ".*"
                    if (matchedString.Equals(".*")) {
                        continue;
                    }

                    if (headerValue == null) {
                        headerValue = GenerateVarReference(cmm, typeof(string), "headerValue");
                    }

                    CodeAssignStatement valueAssignment = new CodeAssignStatement();
                    cmm.Statements.Add(valueAssignment);
                    valueAssignment.Left = headerValue;
                    valueAssignment.Right = (new CodeCastExpression(typeof(string),
                                                               new CodeIndexerExpression(
                                                                   _dictionaryRefExpr,
                                                                   new CodeExpression[] {
                                                                       new CodePrimitiveExpression(((CheckPair)bd.IdCapabilityChecks[i]).Header)
                                                                   }
                                                                   )
                                                               ));

                    // Don't need to use Regex if matching . only.
                    if (matchedString.Equals(".")) {
                        continue;
                    } 

                    if (result == null) {
                        result = GenerateVarReference(cmm, typeof(bool), _resultVarName);
                    }

                    GenerateRegexWorkerIfNecessary(cmm, ref regexWorkerGenerated);
                    //GEN: result = regexWorker.ProcessRegex((string)dictionary["xxxCapability"], "xxxRegexString");
                    CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression(_regexWorkerRefExpr, _processRegexMethod);

                    cmie.Parameters.Add(headerValue);
                    cmie.Parameters.Add(new CodePrimitiveExpression(matchedString));
                    assign = new CodeAssignStatement();
                    assign.Left = result;
                    assign.Right = cmie;
                    cmm.Statements.Add(assign);

                    //GEN: if(result == false) {
                    //GEN:      return false;
                    //GEN: }
                    CodeConditionStatement istatement = new CodeConditionStatement();
                    if (((CheckPair)bd.IdCapabilityChecks[i]).NonMatch) {
                        istatement.Condition = new CodeBinaryOperatorExpression(result, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(true));
                    }
                    else {
                        istatement.Condition = new CodeBinaryOperatorExpression(result, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
                    }
                    istatement.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
                    cmm.Statements.Add(istatement);
                }
            }

            //GEN: browserCaps.DisableOptimizedCacheKey();
            if (disableOptimizedKey) {
                CodeMethodInvokeExpression cme = new CodeMethodInvokeExpression(_browserCapsRefExpr, _disableOptimizedCacheKeyMethodName);
                cmm.Statements.Add(cme);
            }
        }

        private CodeVariableReferenceExpression GenerateVarReference(CodeMemberMethod cmm, Type varType, string varName) {
            //GEN: {varType} {varName};
            cmm.Statements.Add(new CodeVariableDeclarationStatement(varType, varName));
            return new CodeVariableReferenceExpression(varName);
        }

        //generate part of the xxxProcess method for running and storing the capture regexes
        private void GenerateCapturesCode(BrowserDefinition bd, CodeMemberMethod cmm, ref bool regexWorkerGenerated) {
            if ((bd.CaptureHeaderChecks.Count == 0) && (bd.CaptureCapabilityChecks.Count == 0)) {
                return;
            }

            if(bd.CaptureHeaderChecks.Count > 0) {
                AddComment("Capture: header values", cmm);
                for(int i = 0; i < bd.CaptureHeaderChecks.Count; i++) {

                    string matchedString = ((CheckPair)bd.CaptureHeaderChecks[i]).MatchString;
                    if (matchedString.Equals(".*")) {
                        continue;
                    }

                    GenerateRegexWorkerIfNecessary(cmm, ref regexWorkerGenerated);
                    CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression(_regexWorkerRefExpr, _processRegexMethod);

                    if (((CheckPair)bd.CaptureHeaderChecks[i]).Header.Equals("User-Agent")) {
                        _headers.Add(String.Empty);
                        cmie.Parameters.Add(new CodeCastExpression(typeof(string),
                            new CodeIndexerExpression(new CodeVariableReferenceExpression(browserCapsVariable), new CodeExpression[] { 
                                new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(String)), "Empty") })));
                    }
                    else {
                        string header = ((CheckPair)bd.CaptureHeaderChecks[i]).Header;
                        _headers.Add(header);

                        //GEN: regexWorker.ProcessRegex((string)headers["xxx"], "xxxRegexString");
                        cmie.Parameters.Add(
                            new CodeCastExpression(typeof(string),
                                                   new CodeIndexerExpression(
                                                       _headersRefExpr,
                                                       new CodeExpression[] { new CodePrimitiveExpression(header) }
                                                       )
                                                   )
                            );
                    }

                    cmie.Parameters.Add(new CodePrimitiveExpression(matchedString));
                    cmm.Statements.Add(cmie);
                }
            }

            if (bd.CaptureCapabilityChecks.Count > 0) {
                AddComment("Capture: capability values", cmm);
                for(int i = 0; i < bd.CaptureCapabilityChecks.Count; i++) {

                    string matchedString = ((CheckPair)bd.CaptureCapabilityChecks[i]).MatchString;
                    if (matchedString.Equals(".*")) {
                        continue;
                    }

                    GenerateRegexWorkerIfNecessary(cmm, ref regexWorkerGenerated);
                    //GEN: regexWorker.ProcessRegex((string)dictionary["xxxCapability"], "xxxRegexString");
                    CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression(_regexWorkerRefExpr, _processRegexMethod);
                    cmie.Parameters.Add(
                        new CodeCastExpression(typeof(string),
                                               new CodeIndexerExpression(
                                                   _dictionaryRefExpr,
                                                   new CodeExpression[] { new CodePrimitiveExpression(((CheckPair)bd.CaptureCapabilityChecks[i]).Header) }
                                                   )
                                               )
                        );

                    cmie.Parameters.Add(new CodePrimitiveExpression(matchedString));
                    cmm.Statements.Add(cmie);
                }
            }
        }

        //generate part of the xxxProcess method for assigning capability values
        private void GenerateSetCapabilitiesCode(BrowserDefinition bd, CodeMemberMethod cmm, ref bool regexWorkerGenerated) {
            //GEN: browserCaps[aaa] = "bbb";
            //GEN: browserCaps[xxx] = "yyy";
            NameValueCollection nvc = bd.Capabilities;
            CodeAssignStatement assign;

            AddComment("Capabilities: set capabilities", cmm);
            foreach (string s in nvc.Keys) {
                string capsString = nvc[s];
                //GEN: dictionary["xxx"] = regexWorker["xxx"];
                assign = new CodeAssignStatement();
                assign.Left = new CodeIndexerExpression(
                    _dictionaryRefExpr,
                    new CodeExpression[] { new CodePrimitiveExpression(s) } );

                CodePrimitiveExpression capabilityExpr = new CodePrimitiveExpression(capsString);
                if (RegexWorker.RefPat.Match(capsString).Success) {

                    GenerateRegexWorkerIfNecessary(cmm, ref regexWorkerGenerated);
                    assign.Right = new CodeIndexerExpression(
                        _regexWorkerRefExpr,
                        new CodeExpression[] {capabilityExpr});
                }
                else {
                    assign.Right = capabilityExpr;
                }

                cmm.Statements.Add(assign);
            }
        }

        //generate part of the xxxProcess method for setting specific adapters for this browser
        internal void GenerateSetAdaptersCode(BrowserDefinition bd, CodeMemberMethod cmm) {
            //GEN: browserCaps.Adapters[xxxControl] = yyyAdapter;
            foreach (DictionaryEntry entry in bd.Adapters) {
                string controlString = (string)entry.Key;
                string adapterString = (string)entry.Value;
                CodePropertyReferenceExpression cpre = new CodePropertyReferenceExpression(_browserCapsRefExpr, "Adapters");
                CodeIndexerExpression indexerExpression = new CodeIndexerExpression(
                    cpre,
                    new CodeExpression[] { new CodePrimitiveExpression(controlString) }
                    );
                CodeAssignStatement assignAdapter = new CodeAssignStatement();
                assignAdapter.Left = indexerExpression;
                assignAdapter.Right = new CodePrimitiveExpression(adapterString);
                cmm.Statements.Add(assignAdapter);
            }

            //GEN: browser.HtmlTextWriter = xxxHtmlTextWriter;
            if(bd.HtmlTextWriterString != null) {
                CodeAssignStatement assignHtmlTextWriter = new CodeAssignStatement();
                assignHtmlTextWriter.Left = new CodePropertyReferenceExpression(_browserCapsRefExpr, "HtmlTextWriter");
                assignHtmlTextWriter.Right = new CodePrimitiveExpression(bd.HtmlTextWriterString);
                cmm.Statements.Add(assignHtmlTextWriter);
            }
            return;
        }

        internal void AddComment(string comment, CodeMemberMethod cmm) {
            cmm.Statements.Add(new CodeCommentStatement(comment));
        }

        internal CodeStatementCollection GenerateTrackedSingleProcessCall(CodeStatementCollection stmts, BrowserDefinition bd, CodeMemberMethod cmm) {
            return GenerateTrackedSingleProcessCall(stmts, bd, cmm, String.Empty);
        }

        internal CodeStatementCollection GenerateTrackedSingleProcessCall(CodeStatementCollection stmts, BrowserDefinition bd, CodeMemberMethod cmm, string prefix) {
            //GEN:  if (xProcess(headers, browserCaps)) {
            //      }
            //      else {
            //          ...
            //      }
            CodeMethodInvokeExpression xProcess = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), prefix + bd.Name + "Process");
            xProcess.Parameters.Add(new CodeVariableReferenceExpression(_headersRefName));
            xProcess.Parameters.Add(new CodeVariableReferenceExpression(browserCapsVariable));

            CodeConditionStatement conditionStmt = new CodeConditionStatement();
            conditionStmt.Condition = xProcess;

            stmts.Add(conditionStmt);

            return conditionStmt.FalseStatements;
        }

        internal void GenerateSingleProcessCall(BrowserDefinition bd, CodeMemberMethod cmm) {
            GenerateSingleProcessCall(bd, cmm, String.Empty);
        }

        //generate code to call the xxxProcess for a given browser
        //and store the result in a local variable
        internal void GenerateSingleProcessCall(BrowserDefinition bd, CodeMemberMethod cmm, string prefix) {
            //GEN: xProcess(headers, browserCaps);
            CodeMethodInvokeExpression xProcess = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), prefix + bd.Name + "Process");
            xProcess.Parameters.Add(new CodeVariableReferenceExpression(_headersRefName));
            xProcess.Parameters.Add(new CodeVariableReferenceExpression(browserCapsVariable));
            cmm.Statements.Add(xProcess);
        }
    }
}
