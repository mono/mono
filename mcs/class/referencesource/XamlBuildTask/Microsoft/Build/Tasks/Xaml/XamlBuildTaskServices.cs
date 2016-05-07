//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Build.Utilities;
    using System.Reflection;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.CodeDom;
    using System.ComponentModel;
    using System.CodeDom.Compiler;
    using System.Linq;
    using Microsoft.Build.Framework;
    using XamlBuildTask;

    internal static class XamlBuildTaskServices
    {

        internal const string ClrNamespaceUriNamespacePart = "clr-namespace:";
        internal const string ClrNamespaceUriAssemblyPart = "assembly=";
        internal const string XamlExtension = ".xaml";


        //internal static XName SchemaTypeName = XamlSchemaTypeResolver.Default.GetTypeReference(typeof(SchemaType)).Name;
        const string UnknownExceptionErrorCode = "XC1000";
        // Update this value if any changes are made to it in System.Xaml\KnownStrings.cs
        const string serializerReferenceNamePrefix = "__ReferenceID";

        internal static string SerializerReferenceNamePrefix
        { get { return serializerReferenceNamePrefix; } }

        static string _private = String.Empty;
        internal static string PrivateModifier
        { get { return _private; } }

        static string _public = String.Empty;
        internal static string PublicModifier
        { get { return _public; } }

        static string _internal = String.Empty;
        internal static string InternalModifier
        { get { return _internal; } }

        static string _protected = String.Empty;
        internal static string ProtectedModifier
        { get { return _protected; } }

        static string _protectedInternal = String.Empty;
        internal static string ProtectedInternalModifier
        { get { return _protectedInternal; } }

        static string _protectedAndInternal = String.Empty;
        internal static string ProtectedAndInternalModifier
        { get { return _protectedAndInternal; } }

        static string _publicClass = String.Empty;
        internal static string PublicClassModifier
        { get { return _publicClass; } }

        static string _internalClass = String.Empty;
        internal static string InternalClassModifier
        { get { return _internalClass; } }

        static string _fileNotLoaded = String.Empty;
        internal static string FileNotLoaded
        {
            get { return _fileNotLoaded; }
        }

        internal static void PopulateModifiers(CodeDomProvider codeDomProvider)
        {
            TypeConverter memberAttributesConverter = codeDomProvider.GetConverter(typeof(MemberAttributes));
            if (memberAttributesConverter != null)
            {
                if (memberAttributesConverter.CanConvertTo(typeof(string)))
                {
                    try
                    {
                        _private = memberAttributesConverter.ConvertToInvariantString(MemberAttributes.Private).ToUpperInvariant();
                        _public = memberAttributesConverter.ConvertToInvariantString(MemberAttributes.Public).ToUpperInvariant();
                        _protected = memberAttributesConverter.ConvertToInvariantString(MemberAttributes.Family).ToUpperInvariant();
                        _internal = memberAttributesConverter.ConvertToInvariantString(MemberAttributes.Assembly).ToUpperInvariant();
                        _protectedInternal = memberAttributesConverter.ConvertToInvariantString(MemberAttributes.FamilyOrAssembly).ToUpperInvariant();
                        _protectedAndInternal = memberAttributesConverter.ConvertToInvariantString(MemberAttributes.FamilyAndAssembly).ToUpperInvariant();
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
            }

            TypeConverter typeAttributesConverter = codeDomProvider.GetConverter(typeof(TypeAttributes));
            if (typeAttributesConverter != null)
            {
                if (typeAttributesConverter.CanConvertTo(typeof(string)))
                {
                    try
                    {
                        _internalClass = typeAttributesConverter.ConvertToInvariantString(TypeAttributes.NotPublic).ToUpperInvariant();
                        _publicClass = typeAttributesConverter.ConvertToInvariantString(TypeAttributes.Public).ToUpperInvariant();
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
            }
        }

        internal static bool IsPublic(string classModifier)
        {
            if (!string.IsNullOrEmpty(classModifier))
            {
                if (string.Equals(classModifier, InternalClassModifier, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                else if (string.Equals(classModifier, PublicClassModifier, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    throw FxTrace.Exception.AsError(
                        new InvalidOperationException(SR.ClassModifierNotSupported(classModifier)));
                }
            }
            return true;
        }

        internal static MemberVisibility GetMemberVisibility(string memberModifier)
        {
            if (!string.IsNullOrEmpty(memberModifier))
            {
                if (string.Equals(memberModifier, XamlBuildTaskServices.PrivateModifier, StringComparison.OrdinalIgnoreCase))
                {
                    return MemberVisibility.Private;
                }
                else if (string.Equals(memberModifier, XamlBuildTaskServices.PublicModifier, StringComparison.OrdinalIgnoreCase))
                {
                    return MemberVisibility.Public;
                }
                else if (string.Equals(memberModifier, XamlBuildTaskServices.ProtectedModifier, StringComparison.OrdinalIgnoreCase))
                {
                    return MemberVisibility.Family;
                }
                else if (string.Equals(memberModifier, XamlBuildTaskServices.InternalModifier, StringComparison.OrdinalIgnoreCase))
                {
                    return MemberVisibility.Assembly;
                }
                else if (string.Equals(memberModifier, XamlBuildTaskServices.ProtectedInternalModifier, StringComparison.OrdinalIgnoreCase))
                {
                    return MemberVisibility.FamilyOrAssembly;
                }
                else if (string.Equals(memberModifier, XamlBuildTaskServices.ProtectedAndInternalModifier, StringComparison.OrdinalIgnoreCase))
                {
                    return MemberVisibility.FamilyAndAssembly;
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.FieldModifierNotSupported(memberModifier)));
                }
            }
            // Public is only the default modifier for properties, not for fields.
            // But we explicitly set the default modifier for fields (in ClassImporter), so if the
            // modifier is null or empty, it must be a property, and so we return public.
            // This is consistent with Dev10.
            return MemberVisibility.Public;
        }

        public static Assembly ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string[] parts = args.Name.Split(',');
            foreach (var asm in AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies())
            {
                AssemblyName assemblyName = asm.GetName();

                if (assemblyName.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase))
                {
                    return asm;
                }
            }

            // The fact that the file can not be found in the referenced assembly list means that 
            // CLR will throw a FileLoadException once this event handler returns.
            // Since the FileLoadException's FileName property is set to null (due to the fact there is no path),
            // we are storing it in a static variable so that we can print a pretty error message later on.

            _fileNotLoaded = args.Name;

            return null;
        }

        public static AppDomain CreateAppDomain(string friendlyName, string buildTaskPath)
        {
            if (buildTaskPath == null)
            {
                throw FxTrace.Exception.AsError(new LoggableException(new InvalidOperationException(SR.BuildTaskPathMustNotBeNull)));
            }

            // Enable shadow copying in the Designer appdomain, so that we can continue to rebuild
            // projects in the solution even if designer has loaded the assemblies that were referenced in current project
            AppDomainSetup appDomainSetup = new AppDomainSetup();
            appDomainSetup.ShadowCopyFiles = "true";
            appDomainSetup.ApplicationBase = buildTaskPath;
            appDomainSetup.LoaderOptimization = LoaderOptimization.MultiDomainHost;

            // Create appdomain with fulltrust.
            return AppDomain.CreateDomain(
                friendlyName,
                AppDomain.CurrentDomain.Evidence,
                appDomainSetup,
                new NamedPermissionSet("FullTrust"));
        }

        internal static IList<Assembly> Load(IList<ITaskItem> referenceAssemblies, bool isDesignTime)
        {
            List<string> systemReferences = new List<string>();
            List<string> nonSystemReferences = new List<string>();

            CategorizeReferenceAssemblies(referenceAssemblies, out systemReferences, out nonSystemReferences);

            IList<Assembly> assemblies = new List<Assembly>();
            foreach (string item in systemReferences)
            {
                assemblies.Add(Load(item));
            }

            foreach (string item in nonSystemReferences)
            {
                try
                {
                    assemblies.Add(Load(item));
                }
                catch (FileNotFoundException)
                {
                    // file not found on P2P references is allowed.
                    // The design time build can run before the DLL's are present
                    if (!isDesignTime)
                    {
                        throw;
                    }
                }
            }

            bool mscorlibFound = false;

            foreach (Assembly asm in assemblies)
            {
                // here we want to check if the assembly is mscorlib.dll.
                // for the current codebase, this check would have worked:
                // if (asm == typeof(Object).Assembly), but we 
                // prefer a check that will continue to work when LMR is used
                if (asm.GetReferencedAssemblies().Length == 0)
                {
                    mscorlibFound = true;
                }
            }

            if (!mscorlibFound)
            {
                assemblies.Add(typeof(Object).Assembly);
            }

            return assemblies;
        }

        [SuppressMessage(FxCop.Category.Reliability, FxCop.Rule.AvoidCallingProblematicMethods,
            Justification = "Using LoadFile to avoid loading through Fusion and load the exact assembly the developer specified")]
        internal static Assembly Load(string reference)
        {
            if (reference.EndsWith("mscorlib.dll", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(Object).Assembly;
            }
            string fullPath = Path.GetFullPath(reference);
            try
            {
                return Assembly.ReflectionOnlyLoadFrom(fullPath);
            }
            catch (FileNotFoundException e)
            {
                if (e.FileName == null)
                {
                    throw FxTrace.Exception.AsError((new FileNotFoundException(e.Message, fullPath)));
                }
                else
                    throw;
            }
        }

        internal static void LogException(TaskLoggingHelper buildLogger, string message)
        {
            LogException(buildLogger, message, null, 0, 0);
        }

        internal static void LogException(TaskLoggingHelper buildLogger, string message, string fileName, int lineNumber, int linePosition)
        {
            string errorCode, logMessage;
            ExtractErrorCodeAndMessage(buildLogger, message, out errorCode, out logMessage);
            buildLogger.LogError(null, errorCode, null, fileName, lineNumber, linePosition, 0, 0, logMessage, null);
        }

        static void ExtractErrorCodeAndMessage(TaskLoggingHelper buildLogger, string message, out string errorCode, out string logMessage)
        {
            errorCode = buildLogger.ExtractMessageCode(message, out logMessage);
            if (string.IsNullOrEmpty(errorCode))
            {
                errorCode = UnknownExceptionErrorCode;
                logMessage = SR.UnknownBuildError(message);
            }
        }

        internal static bool IsClrNamespaceUri(string nsName, out int nsIndex, out int assemblyIndex)
        {
            if (nsName.StartsWith(ClrNamespaceUriNamespacePart, StringComparison.Ordinal))
            {
                int semicolonIndex = nsName.IndexOf(';');
                if (semicolonIndex == -1 || nsName.Trim().EndsWith(";", StringComparison.Ordinal))
                {
                    nsIndex = ClrNamespaceUriNamespacePart.Length;
                    assemblyIndex = -1;
                    return true;
                }
                else
                {
                    int equalsIndex = nsName.IndexOf('=', semicolonIndex);
                    if (equalsIndex != -1)
                    {
                        int start = ClrNamespaceUriNamespacePart.Length;
                        int assemblyStart = semicolonIndex + 1;
                        if (equalsIndex - semicolonIndex == ClrNamespaceUriAssemblyPart.Length)
                        {
                            for (int i = 0; i < ClrNamespaceUriAssemblyPart.Length; i++)
                            {
                                if (nsName[assemblyStart + i] != ClrNamespaceUriAssemblyPart[i])
                                {
                                    nsIndex = -1;
                                    assemblyIndex = -1;
                                    return false;
                                }
                            }

                            nsIndex = start;
                            assemblyIndex = assemblyStart + ClrNamespaceUriAssemblyPart.Length;
                            return true;
                        }
                    }
                }
            }
            nsIndex = -1;
            assemblyIndex = -1;
            return false;
        }

        internal static string UpdateClrNamespaceUriWithLocalAssembly(string @namespace, string localAssemblyName)
        {
            return UpdateClrNamespaceUriWithLocalAssembly(@namespace, localAssemblyName, null);
        }

        internal static string UpdateClrNamespaceUriWithLocalAssembly(string @namespace, string localAssemblyName, string realAssemblyName)
        {
            int nsIndex, assemblyIndex;
            if (IsClrNamespaceUri(@namespace, out nsIndex, out assemblyIndex))
            {
                // If assembly portion of namespace does not exist, assume that this is part of the local assembly
                if (assemblyIndex == -1)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0};{1}{2}",
                        @namespace.TrimEnd(' ', ';'), XamlBuildTaskServices.ClrNamespaceUriAssemblyPart, localAssemblyName);
                }
                else if (!string.IsNullOrEmpty(realAssemblyName) &&
                    localAssemblyName != realAssemblyName &&
                    @namespace.Substring(assemblyIndex) == realAssemblyName)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}{1}",
                        @namespace.Substring(0, assemblyIndex), localAssemblyName);
                }
            }
            return @namespace;
        }

        internal static string GetFullTypeName(XamlType xamlType)
        {
            string typeName = GetFullTypeNameWithoutNamespace(xamlType);
            typeName = xamlType.PreferredXamlNamespace + ":" + typeName;
            return typeName;
        }

        private static string GetFullTypeNameWithoutNamespace(XamlType xamlType)
        {
            string typeName = string.Empty;
            if (xamlType != null)
            {
                typeName = xamlType.Name;
                bool firstTypeArg = true;
                if (xamlType.TypeArguments != null && xamlType.TypeArguments.Count > 0)
                {
                    typeName += "(";
                    foreach (XamlType typeArg in xamlType.TypeArguments)
                    {
                        if (!firstTypeArg)
                        {
                            typeName += ",";
                        }
                        else
                        {
                            firstTypeArg = false;
                        }
                        typeName += typeArg.Name;
                    }
                    typeName += ")";
                }
            }
            return typeName;
        }

        internal static XamlType GetXamlTypeFromString(string typeName, NamespaceTable namespaceTable, XamlSchemaContext xsc)
        {
            XamlTypeName xamlTypeName = XamlTypeName.Parse(typeName, namespaceTable);
            XamlType xamlType = xsc.GetXamlType(xamlTypeName);
            if (xamlType == null)
            {
                xamlType = GetXamlTypeFromXamlTypeName(xamlTypeName, namespaceTable, xsc);
            }
            return xamlType;
        }

        static XamlType GetXamlTypeFromXamlTypeName(XamlTypeName xamlTypeName, NamespaceTable namespaceTable, XamlSchemaContext xsc)
        {
            IList<XamlType> typeArgs = null;
            if (xamlTypeName.TypeArguments.Count > 0)
            {
                typeArgs = new List<XamlType>();
                foreach (var typeArg in xamlTypeName.TypeArguments)
                {
                    typeArgs.Add(GetXamlTypeFromXamlTypeName(typeArg, namespaceTable, xsc));
                }
            }
            return new XamlType(xamlTypeName.Namespace, xamlTypeName.Name, typeArgs, xsc);
        }      

        internal static bool TryGetClrTypeName(XamlType xamlType, string rootNamespace, out string clrTypeName)
        {
            bool isLocal;   
            return TryGetClrTypeName(xamlType, rootNamespace, out clrTypeName, out isLocal);
        }

        internal static bool TryGetClrTypeName(XamlType xamlType, string rootNamespace, out string clrTypeName, out bool isLocal)
        {
            if (!xamlType.IsUnknown)
            {
                isLocal = false;
                clrTypeName = xamlType.UnderlyingType != null ? xamlType.UnderlyingType.FullName : null;
                return (clrTypeName != null);
            }
            else
            {
                isLocal = true;
                return TryGetClrTypeNameFromLocalType(xamlType, rootNamespace, out clrTypeName);
            }
        }

        internal static bool TryExtractClrNs(string @namespace, out string clrNs)
        {
            int nsIndex, assemblyIndex;
            if (XamlBuildTaskServices.IsClrNamespaceUri(@namespace, out nsIndex, out assemblyIndex))
            {
                clrNs = (assemblyIndex == -1)
                    ? @namespace.Substring(nsIndex).TrimEnd(' ', ';')
                    : @namespace.Substring(
                        nsIndex,
                        assemblyIndex - XamlBuildTaskServices.ClrNamespaceUriAssemblyPart.Length - nsIndex - 1).TrimEnd(' ', ';');
                return true;
            }
            else
            {
                clrNs = null;
                return false;
            }
        }

        static bool TryGetClrTypeNameFromLocalType(XamlType xamlType, string rootNamespace, out string clrTypeName)
        {
            //
            // This means that either we have a type in the base hierarchy or type arguments
            // that is invalid or it's in the local (current) project.
            string @namespace = xamlType.PreferredXamlNamespace;
            string name = xamlType.Name;
            string clrNs;

            if (@namespace != null && TryExtractClrNs(@namespace, out clrNs))
            {
                if (!String.IsNullOrEmpty(rootNamespace) && 
                    !String.IsNullOrEmpty(clrNs) &&
                    clrNs.StartsWith(rootNamespace, StringComparison.OrdinalIgnoreCase))
                {
                    if (clrNs.Length > rootNamespace.Length)
                    {
                        clrNs = clrNs.Substring(rootNamespace.Length + 1);
                    }
                    else
                    {
                        clrNs = string.Empty;
                    }
                } 
                if (string.IsNullOrEmpty(clrNs))
                {
                    clrTypeName = name;
                }
                else
                {
                    clrTypeName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", clrNs, name);
                }
                return true;
            }
            else
            {
                // This could be an open generic with local type-args. Or it could be a known type arg
                // that we didn't resolve earlier because it was an arg to a local open generic.
                // Either way we should try to re-resolve it here.
                string qualifiedName = name;
                if (xamlType.TypeArguments != null && xamlType.TypeArguments.Count > 0)
                {
                    qualifiedName = name + "`" + xamlType.TypeArguments.Count;
                }
                XamlType resolvedType = xamlType.SchemaContext.GetXamlType(new XamlTypeName(@namespace, qualifiedName));
                if (resolvedType != null && resolvedType.UnderlyingType != null)
                {
                    clrTypeName = resolvedType.UnderlyingType.FullName;
                    return true;
                }
            }
            clrTypeName = null;
            return false;
        }

        // Returns true if type namespace is clr-namespace and populates assemblyName
        // Else returns false with assemblyName null;
        internal static bool GetTypeNameInAssemblyOrNamespace(XamlType type, string localAssemblyName, string realAssemblyName, 
            out string typeName, out string assemblyName, out string ns)
        {
            int assemblyIndex, nsIndex;
            string typeNs = type.PreferredXamlNamespace;
            typeName = GetFullTypeNameWithoutNamespace(type);
            if (IsClrNamespaceUri(typeNs, out nsIndex, out assemblyIndex))
            {
                assemblyName = assemblyIndex > -1 ? typeNs.Substring(assemblyIndex) : String.Empty;
                if ((!string.IsNullOrEmpty(localAssemblyName) && assemblyName.Contains(localAssemblyName)) || string.IsNullOrEmpty(assemblyName))
                {
                    assemblyName = realAssemblyName;
                }
                int nsLength = typeNs.IndexOf(';') - nsIndex;
                if (nsLength > 0)
                {
                    ns = typeNs.Substring(nsIndex, nsLength);
                }
                else
                {
                    ns = typeNs.Substring(nsIndex);
                }
                return true;
            }
            else
            {
                assemblyName = null;
                ns = typeNs;
                return false;
            }
        }

        internal static string GetTypeName(XamlType type, string localAssemblyName, string realAssemblyName)
        {
            string typeName, assemblyName, ns;
            if (GetTypeNameInAssemblyOrNamespace(type, localAssemblyName, realAssemblyName, out typeName, out assemblyName, out ns))
            {
                return ns + "." + typeName;
            }
            else
            {
                return ns + ":" + typeName;
            }
        }

        // Returns false if the type is known else returns true.
        internal static bool GetUnresolvedLeafTypeArg(XamlType type, ref IList<XamlType> unresolvedLeafTypeList)
        {
            if (unresolvedLeafTypeList != null && type != null && type.IsUnknown)
            {
                if (!type.IsGeneric)
                {
                    unresolvedLeafTypeList.Add(type);
                }
                else
                {
                    bool hasUnknownChildren = false;
                    foreach (XamlType typeArg in type.TypeArguments)
                    {
                        hasUnknownChildren |= GetUnresolvedLeafTypeArg(typeArg, ref unresolvedLeafTypeList);
                    }
                    if (!hasUnknownChildren)
                    {
                        unresolvedLeafTypeList.Add(type);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        [SuppressMessage(FxCop.Category.Reliability, FxCop.Rule.AvoidCallingProblematicMethods,
            Justification = "Using LoadFrom to avoid loading through Fusion and load from the exact path specified")]
        internal static IEnumerable<T> GetXamlBuildTaskExtensions<T>(IList<Tuple<string, string, string>> extensionNames, TaskLoggingHelper logger, string currentProjectDirectory) where T : class
        {            
            List<T> extensionsLoaded = new List<T>();

            if (extensionNames == null)
            {
                return extensionsLoaded;
            }

            foreach (Tuple<string, string, string> extensionEntry in extensionNames)
            {
                Assembly assembly = null;
                string assemblyName = extensionEntry.Item2;
                string assemblyFile = extensionEntry.Item3;
                if (!string.IsNullOrEmpty(assemblyName))
                {
                    try
                    {
                        // try to load using the assembly name
                        assembly = Assembly.Load(assemblyName);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;                            
                        }
                        logger.LogWarning(SR.UnresolvedExtensionAssembly(assemblyName));
                    }
                }
                else
                {
                    try 
                    {
                        if (Path.IsPathRooted(assemblyFile))
                        {
                            // if the path is absolute, we just load from the given location
                            assembly = Assembly.LoadFrom(assemblyFile);
                        }
                        else
                        {
                            // if the path is relative, we load from 
                            // current project folder + provided relative path
                            assembly = Assembly.LoadFrom(Path.Combine(currentProjectDirectory, assemblyFile));
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        logger.LogWarning(SR.UnresolvedExtensionAssembly(assemblyFile));
                    }
                }               

                if (assembly == null)
                {
                    continue;
                }

                Type extensionType = assembly.GetType(extensionEntry.Item1);

                if (extensionType == null || extensionType.GetInterface(typeof(T).FullName) == null)
                {
                    string assemblylocationInfo = assemblyFile != "" ? assemblyFile : assemblyName;
                    logger.LogWarning(SR.UnresolvedExtension(extensionEntry.Item1, assemblylocationInfo));
                    continue;
                }
                
                T extension;
                try
                {
                    extension = Activator.CreateInstance(extensionType) as T;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    logger.LogWarning(SR.ExceptionThrownDuringConstruction(extensionEntry.Item1,
                        e.GetType().ToString(),
                        e.Message,
                        e.InnerException != null ? e.InnerException.GetType().ToString() : "null",
                        e.InnerException != null ? e.InnerException.Message : "null"));

                    continue;
                }

                if (extension != null)
                {
                    extensionsLoaded.Add(extension);                    
                }                
            }
            return extensionsLoaded;
        }

        internal static IList<Tuple<string, string, string>> GetXamlBuildTaskExtensionNames(ITaskItem[] xamlBuildTypeGenerationExtensionsNames)
        {
            List<Tuple<string, string, string>> extensionNames = new List<Tuple<string, string, string>>();
            if (xamlBuildTypeGenerationExtensionsNames != null)
            {
                string assemblyFile;
                string assemblyName;
                foreach (ITaskItem taskItem in xamlBuildTypeGenerationExtensionsNames)
                {
                    assemblyFile = taskItem.GetMetadata("AssemblyFile");
                    assemblyName = taskItem.GetMetadata("AssemblyName");
                    if (assemblyName != "" && assemblyFile != "")
                    {
                        throw FxTrace.Exception.AsError(new LoggableException(SR.BothAssemblyNameAndFileSpecified));
                    }
                    if (assemblyName == "" && assemblyFile == "")
                    {
                        throw FxTrace.Exception.AsError(new LoggableException(SR.AssemblyNameOrFileNotSpecified));
                    }
                    
                    extensionNames.Add(new Tuple<string, string, string>(taskItem.ItemSpec, assemblyName, assemblyFile));
                }
            }
            return extensionNames;
        }

        internal static IList<string> GetReferences(IList<ITaskItem> referenceAssemblies)
        {
            IList<string> references = new List<string>();
            foreach (var reference in referenceAssemblies)
            {
                references.Add(reference.ItemSpec);
            }

            return references;
        }

        private static void CategorizeReferenceAssemblies(IList<ITaskItem> referenceAssemblies, out List<string> systemItems, out List<string> nonSystemItems)
        {
            List<string> systemList = new List<string>();
            List<string> nonSystemList = new List<string>();
            foreach (ITaskItem item in referenceAssemblies)
            {
                string resolvedFrom = item.GetMetadata("ResolvedFrom");
                string isSystemReference = item.GetMetadata("IsSystemReference");
                string asmName = Path.GetFileName(item.ItemSpec);

                bool isMsCorLib = asmName.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase);
                bool isManagedSystemMetadata = !String.IsNullOrEmpty(isSystemReference)
                    && isSystemReference.Equals("True", StringComparison.OrdinalIgnoreCase);
                bool isNativeSystemMetadata = resolvedFrom != null
                    && (resolvedFrom == "GetSDKReferenceFiles" || resolvedFrom == "{TargetFrameworkDirectory}");

                if (isManagedSystemMetadata || isNativeSystemMetadata || isMsCorLib)
                {
                    systemList.Add(item.ItemSpec);
                }
                else
                {
                    nonSystemList.Add(item.ItemSpec);
                }
            }

            systemItems = systemList;
            nonSystemItems = nonSystemList;
        }
    }


}
