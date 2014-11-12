//------------------------------------------------------------------------------
// <copyright file="Compilation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System.Configuration;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Collections;
    using System.IO;
    using System;
    using System.Text;
    using System.Xml;
    using System.Threading;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Xml.Serialization.Configuration;
    using System.Diagnostics;
    using System.CodeDom.Compiler;
    using System.Globalization;
    using System.Runtime.Versioning;
    using System.Diagnostics.CodeAnalysis;

    internal class TempAssembly {
        internal const string GeneratedAssemblyNamespace = "Microsoft.Xml.Serialization.GeneratedAssembly";
        Assembly assembly;
        bool pregeneratedAssmbly = false;
        XmlSerializerImplementation contract = null;
        Hashtable writerMethods;
        Hashtable readerMethods;
        TempMethodDictionary methods;
        static object[] emptyObjectArray = new object[0];
        Hashtable assemblies = new Hashtable();
        static volatile FileIOPermission fileIOPermission;

        internal class TempMethod {
            internal MethodInfo writeMethod;
            internal MethodInfo readMethod;
            internal string name;
            internal string ns;
            internal bool isSoap;
            internal string methodKey;
        }

        private TempAssembly() {
        }

        internal TempAssembly(XmlMapping[] xmlMappings, Type[] types, string defaultNamespace, string location, Evidence evidence) {
            bool containsSoapMapping = false;
            for (int i = 0; i < xmlMappings.Length; i++) {
                xmlMappings[i].CheckShallow();
                if (xmlMappings[i].IsSoap) {
                    containsSoapMapping = true;
                }
            }

            // We will make best effort to use RefEmit for assembly generation
            bool fallbackToCSharpAssemblyGeneration = false;

            if (!containsSoapMapping && !TempAssembly.UseLegacySerializerGeneration) {
                try {
                    assembly = GenerateRefEmitAssembly(xmlMappings, types, defaultNamespace, evidence);
                }
                // Only catch and handle known failures with RefEmit
                catch (CodeGeneratorConversionException) {
                    fallbackToCSharpAssemblyGeneration = true;
                }
                // Add other known exceptions here...
                //
            }
            else {
                fallbackToCSharpAssemblyGeneration = true;
            }
            
            if (fallbackToCSharpAssemblyGeneration) {
                assembly = GenerateAssembly(xmlMappings, types, defaultNamespace, evidence, XmlSerializerCompilerParameters.Create(location), null, assemblies);
            }

#if DEBUG
            // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
            if (assembly == null) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorDetails, "Failed to generate XmlSerializer assembly, but did not throw"));
#endif
            InitAssemblyMethods(xmlMappings);
        }

        internal TempAssembly(XmlMapping[] xmlMappings, Assembly assembly, XmlSerializerImplementation contract) {
            this.assembly = assembly;
            InitAssemblyMethods(xmlMappings);
            this.contract = contract;
            pregeneratedAssmbly = true;
        }

        internal static bool UseLegacySerializerGeneration {
            get {
                if (AppSettings.UseLegacySerializerGeneration.HasValue) {
                    // AppSetting will always win if specified
                    return (bool) AppSettings.UseLegacySerializerGeneration; 
                }
                else {
                    XmlSerializerSection configSection = ConfigurationManager.GetSection(ConfigurationStrings.XmlSerializerSectionPath) as XmlSerializerSection;
                    return configSection == null ? false : configSection.UseLegacySerializerGeneration;
                }
            }
        }

        internal TempAssembly(XmlSerializerImplementation contract) {
            this.contract = contract;
            pregeneratedAssmbly = true;
        }

        internal XmlSerializerImplementation Contract {
            get {
                if (contract == null) {
                    contract = (XmlSerializerImplementation)Activator.CreateInstance(GetTypeFromAssembly(this.assembly, "XmlSerializerContract"));
                }
                return contract;
            }
        }

        internal void InitAssemblyMethods(XmlMapping[] xmlMappings) {
            methods = new TempMethodDictionary();
            for (int i = 0; i < xmlMappings.Length; i++) {
                TempMethod method = new TempMethod();
                method.isSoap = xmlMappings[i].IsSoap;
                method.methodKey = xmlMappings[i].Key;
                XmlTypeMapping xmlTypeMapping = xmlMappings[i] as XmlTypeMapping;
                if (xmlTypeMapping != null) {
                    method.name = xmlTypeMapping.ElementName;
                    method.ns = xmlTypeMapping.Namespace;
                }
                methods.Add(xmlMappings[i].Key, method);
            }
        }

        /// <devdoc>
        ///    <para>
        ///    Attempts to load pre-generated serialization assembly.
        ///    First check for the [XmlSerializerAssembly] attribute
        ///    </para>
        /// </devdoc>
        // SxS: This method does not take any resource name and does not expose any resources to the caller.
        // It's OK to suppress the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        internal static Assembly LoadGeneratedAssembly(Type type, string defaultNamespace, out XmlSerializerImplementation contract) {
            Assembly serializer = null;
            contract = null;
            string serializerName = null;

            // Packaged apps do not support loading generated serializers.
            if (Microsoft.Win32.UnsafeNativeMethods.IsPackagedProcess.Value) {
                return null;
            }

            bool logEnabled = DiagnosticsSwitches.PregenEventLog.Enabled;

            // check to see if we loading explicit pre-generated assembly
            object[] attrs = type.GetCustomAttributes(typeof(XmlSerializerAssemblyAttribute), false);
            if (attrs.Length == 0) {
                // Guess serializer name: if parent assembly signed use strong name 
                AssemblyName name = GetName(type.Assembly, true);
                serializerName = Compiler.GetTempAssemblyName(name, defaultNamespace);
                // use strong name 
                name.Name = serializerName;
                name.CodeBase = null;
                name.CultureInfo = CultureInfo.InvariantCulture;
                try {
                    serializer = Assembly.Load(name);
                }
                catch (Exception e) {
                    if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                        throw;
                    }
                    if (logEnabled) {
                        Log(e.Message, EventLogEntryType.Information);
                    }
                    byte[] token = name.GetPublicKeyToken();
                    if (token != null && token.Length > 0) {
                        // the parent assembly was signed, so do not try to LoadWithPartialName
                        return null;
                    }
#pragma warning disable 618
                    serializer = Assembly.LoadWithPartialName(serializerName, null);
#pragma warning restore 618
                }
                if (serializer == null) {
#if !FEATURE_PAL // EventLog
                    if (logEnabled) {
                        Log(Res.GetString(Res.XmlPregenCannotLoad, serializerName), EventLogEntryType.Information);
                    }
#endif //!FEATURE_PAL // EventLog
                    return null;
                }
                if (!IsSerializerVersionMatch(serializer, type, defaultNamespace, null)) {
#if !FEATURE_PAL // EventLog
                    if (logEnabled)
                        Log(Res.GetString(Res.XmlSerializerExpiredDetails, serializerName, type.FullName), EventLogEntryType.Error);
#endif //!FEATURE_PAL // EventLog
                    return null;
                }
            }
            else {
                XmlSerializerAssemblyAttribute assemblyAttribute = (XmlSerializerAssemblyAttribute)attrs[0];
                if (assemblyAttribute.AssemblyName != null && assemblyAttribute.CodeBase != null)
                    throw new InvalidOperationException(Res.GetString(Res.XmlPregenInvalidXmlSerializerAssemblyAttribute, "AssemblyName", "CodeBase"));

                // found XmlSerializerAssemblyAttribute attribute, it should have all needed information to load the pre-generated serializer
                if (assemblyAttribute.AssemblyName != null) {
                    serializerName = assemblyAttribute.AssemblyName;
#pragma warning disable 618
                    serializer = Assembly.LoadWithPartialName(serializerName, null);
#pragma warning restore 618
                }
                else if (assemblyAttribute.CodeBase != null && assemblyAttribute.CodeBase.Length > 0) {
                    serializerName = assemblyAttribute.CodeBase;
                    serializer = Assembly.LoadFrom(serializerName);
                }
                else {
                    serializerName = type.Assembly.FullName;
                    serializer = type.Assembly;
                }
                if (serializer == null) {
                    throw new FileNotFoundException(null, serializerName);
                }
            }
            Type contractType = GetTypeFromAssembly(serializer, "XmlSerializerContract");
            contract = (XmlSerializerImplementation)Activator.CreateInstance(contractType);
            if (contract.CanSerialize(type))
                return serializer;

#if !FEATURE_PAL // EventLog
            if (logEnabled)
                Log(Res.GetString(Res.XmlSerializerExpiredDetails, serializerName, type.FullName), EventLogEntryType.Error);
#endif //!FEATURE_PAL // EventLog
            return null;
        }

#if !FEATURE_PAL // EventLog
        static void Log(string message, EventLogEntryType type) {
            new EventLogPermission(PermissionState.Unrestricted).Assert();
            EventLog.WriteEntry("XmlSerializer", message, type);
        }
#endif //!FEATURE_PAL // EventLog

        static AssemblyName GetName(Assembly assembly, bool copyName) {
            PermissionSet perms = new PermissionSet(PermissionState.None);
            perms.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            perms.Assert();
            return assembly.GetName(copyName);
        }


        static bool IsSerializerVersionMatch(Assembly serializer, Type type, string defaultNamespace, string location) {
            if (serializer == null)
                return false;
            object[] attrs = serializer.GetCustomAttributes(typeof(XmlSerializerVersionAttribute), false);
            if (attrs.Length != 1)
                return false;

            XmlSerializerVersionAttribute assemblyInfo = (XmlSerializerVersionAttribute)attrs[0];
            // we found out dated pre-generate assembly
            // 
            if (assemblyInfo.ParentAssemblyId == GenerateAssemblyId(type) && assemblyInfo.Namespace == defaultNamespace)
                return true;
            return false;
        }

        // SxS: This method does not take any resource name and does not expose any resources to the caller.
        // It's OK to suppress the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        static string GenerateAssemblyId(Type type) {
            Module[] modules = type.Assembly.GetModules();
            ArrayList list = new ArrayList();
            for (int i = 0; i < modules.Length; i++) {
                list.Add(modules[i].ModuleVersionId.ToString());
            }
            list.Sort();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++) {
                sb.Append(list[i].ToString());
                sb.Append(",");
            }
            return sb.ToString();
        }

        internal static Assembly GenerateAssembly(XmlMapping[] xmlMappings, Type[] types, string defaultNamespace, Evidence evidence, XmlSerializerCompilerParameters parameters, Assembly assembly, Hashtable assemblies) {
            FileIOPermission.Assert();
            Compiler compiler = new Compiler();
            try {
                Hashtable scopeTable = new Hashtable();
                foreach (XmlMapping mapping in xmlMappings)
                    scopeTable[mapping.Scope] = mapping;
                TypeScope[] scopes = new TypeScope[scopeTable.Keys.Count];
                scopeTable.Keys.CopyTo(scopes, 0);

                assemblies.Clear();
                Hashtable importedTypes = new Hashtable();
                foreach (TypeScope scope in scopes) {
                    foreach (Type t in scope.Types) {
                        compiler.AddImport(t, importedTypes);
                        Assembly a = t.Assembly;
                        string name = a.FullName;
                        if (assemblies[name] != null)
                            continue;
                        if (!a.GlobalAssemblyCache) {
                            assemblies[name] = a;
                        }
                    }
                }
                for (int i = 0; i < types.Length; i++) {
                    compiler.AddImport(types[i], importedTypes);
                }
                compiler.AddImport(typeof(object).Assembly);
                compiler.AddImport(typeof(XmlSerializer).Assembly);

                IndentedWriter writer = new IndentedWriter(compiler.Source, false);

                writer.WriteLine("#if _DYNAMIC_XMLSERIALIZER_COMPILATION");
                writer.WriteLine("[assembly:System.Security.AllowPartiallyTrustedCallers()]");
                writer.WriteLine("[assembly:System.Security.SecurityTransparent()]");
                writer.WriteLine("[assembly:System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]");
                writer.WriteLine("#endif");
                // Add AssemblyVersion attribute to match parent accembly version
                if (types != null && types.Length > 0 && types[0] != null) {
                    writer.WriteLine("[assembly:System.Reflection.AssemblyVersionAttribute(\"" + types[0].Assembly.GetName().Version.ToString() + "\")]");
                }
                if (assembly != null && types.Length > 0) {
                    for (int i = 0; i < types.Length; i++) {
                        Type type = types[i];
                        if (type == null)
                            continue;
                        if (DynamicAssemblies.IsTypeDynamic(type)) {
                            throw new InvalidOperationException(Res.GetString(Res.XmlPregenTypeDynamic, types[i].FullName));
                        }
                    }
                    writer.Write("[assembly:");
                    writer.Write(typeof(XmlSerializerVersionAttribute).FullName);
                    writer.Write("(");
                    writer.Write("ParentAssemblyId=");
                    ReflectionAwareCodeGen.WriteQuotedCSharpString(writer, GenerateAssemblyId(types[0]));
                    writer.Write(", Version=");
                    ReflectionAwareCodeGen.WriteQuotedCSharpString(writer, ThisAssembly.Version);
                    if (defaultNamespace != null) {
                        writer.Write(", Namespace=");
                        ReflectionAwareCodeGen.WriteQuotedCSharpString(writer, defaultNamespace);
                    }
                    writer.WriteLine(")]");
                }
                CodeIdentifiers classes = new CodeIdentifiers();
                classes.AddUnique("XmlSerializationWriter", "XmlSerializationWriter");
                classes.AddUnique("XmlSerializationReader", "XmlSerializationReader");
                string suffix = null;
                if (types != null && types.Length == 1 && types[0] != null) {
                    suffix = CodeIdentifier.MakeValid(types[0].Name);
                    if (types[0].IsArray) {
                        suffix += "Array";
                    }
                }

                writer.WriteLine("namespace " + GeneratedAssemblyNamespace + " {");
                writer.Indent++;

                writer.WriteLine();

                string writerClass = "XmlSerializationWriter" + suffix;
                writerClass = classes.AddUnique(writerClass, writerClass);
                XmlSerializationWriterCodeGen writerCodeGen = new XmlSerializationWriterCodeGen(writer, scopes, "public", writerClass);

                writerCodeGen.GenerateBegin();
                string[] writeMethodNames = new string[xmlMappings.Length];

                for (int i = 0; i < xmlMappings.Length; i++) {
                    writeMethodNames[i] = writerCodeGen.GenerateElement(xmlMappings[i]);
                }
                writerCodeGen.GenerateEnd();

                writer.WriteLine();

                string readerClass = "XmlSerializationReader" + suffix;
                readerClass = classes.AddUnique(readerClass, readerClass);
                XmlSerializationReaderCodeGen readerCodeGen = new XmlSerializationReaderCodeGen(writer, scopes, "public", readerClass);

                readerCodeGen.GenerateBegin();
                string[] readMethodNames = new string[xmlMappings.Length];
                for (int i = 0; i < xmlMappings.Length; i++) {
                    readMethodNames[i] = readerCodeGen.GenerateElement(xmlMappings[i]);
                }
                readerCodeGen.GenerateEnd(readMethodNames, xmlMappings, types);

                string baseSerializer = readerCodeGen.GenerateBaseSerializer("XmlSerializer1", readerClass, writerClass, classes);
                Hashtable serializers = new Hashtable();
                for (int i = 0; i < xmlMappings.Length; i++) {
                    if (serializers[xmlMappings[i].Key] == null) {
                        serializers[xmlMappings[i].Key] = readerCodeGen.GenerateTypedSerializer(readMethodNames[i], writeMethodNames[i], xmlMappings[i], classes, baseSerializer, readerClass, writerClass);
                    }
                }
                readerCodeGen.GenerateSerializerContract("XmlSerializerContract", xmlMappings, types, readerClass, readMethodNames, writerClass, writeMethodNames, serializers);
                writer.Indent--;
                writer.WriteLine("}");

                return compiler.Compile(assembly, defaultNamespace, parameters, evidence);
            }
            finally {
                compiler.Close();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification="It is safe because the serialization assembly is generated by the framework code, not by the user.")]
        internal static Assembly GenerateRefEmitAssembly(XmlMapping[] xmlMappings, Type[] types, string defaultNamespace, Evidence evidence) {
            Hashtable scopeTable = new Hashtable();
            foreach (XmlMapping mapping in xmlMappings)
                scopeTable[mapping.Scope] = mapping;
            TypeScope[] scopes = new TypeScope[scopeTable.Keys.Count];
            scopeTable.Keys.CopyTo(scopes, 0);

            string assemblyName = "Microsoft.GeneratedCode";
            AssemblyBuilder assemblyBuilder = CodeGenerator.CreateAssemblyBuilder(AppDomain.CurrentDomain, assemblyName);
            ConstructorInfo SecurityTransparentAttribute_ctor = typeof(SecurityTransparentAttribute).GetConstructor(
                CodeGenerator.InstanceBindingFlags,
                null,
                CodeGenerator.EmptyTypeArray,
                null
                );
            assemblyBuilder.SetCustomAttribute(new CustomAttributeBuilder(SecurityTransparentAttribute_ctor, new Object[0]));
            ConstructorInfo AllowPartiallyTrustedCallersAttribute_ctor = typeof(AllowPartiallyTrustedCallersAttribute).GetConstructor(
                CodeGenerator.InstanceBindingFlags,
                null,
                CodeGenerator.EmptyTypeArray,
                null
                );
            assemblyBuilder.SetCustomAttribute(new CustomAttributeBuilder(AllowPartiallyTrustedCallersAttribute_ctor, new Object[0]));
            ConstructorInfo SecurityRulesAttribute_ctor = typeof(SecurityRulesAttribute).GetConstructor(
                CodeGenerator.InstanceBindingFlags,
                null,
                new Type[] { typeof(SecurityRuleSet) },
                null
                );
            assemblyBuilder.SetCustomAttribute(new CustomAttributeBuilder(SecurityRulesAttribute_ctor, new Object[] { SecurityRuleSet.Level1 }));
            // Add AssemblyVersion attribute to match parent accembly version
            if (types != null && types.Length > 0 && types[0] != null) {

                ConstructorInfo AssemblyVersionAttribute_ctor = typeof(AssemblyVersionAttribute).GetConstructor(
                    CodeGenerator.InstanceBindingFlags,
                    null,
                    new Type[] { typeof(String) },
                    null
                    );
                FileIOPermission.Assert();
                string assemblyVersion = types[0].Assembly.GetName().Version.ToString();
                FileIOPermission.RevertAssert();
                assemblyBuilder.SetCustomAttribute(new CustomAttributeBuilder(AssemblyVersionAttribute_ctor, new Object[] { assemblyVersion }));
            }
            CodeIdentifiers classes = new CodeIdentifiers();
            classes.AddUnique("XmlSerializationWriter", "XmlSerializationWriter");
            classes.AddUnique("XmlSerializationReader", "XmlSerializationReader");
            string suffix = null;
            if (types != null && types.Length == 1 && types[0] != null) {
                suffix = CodeIdentifier.MakeValid(types[0].Name);
                if (types[0].IsArray) {
                    suffix += "Array";
                }
            }

            ModuleBuilder moduleBuilder = CodeGenerator.CreateModuleBuilder(assemblyBuilder, assemblyName);

            string writerClass = "XmlSerializationWriter" + suffix;
            writerClass = classes.AddUnique(writerClass, writerClass);
            XmlSerializationWriterILGen writerCodeGen = new XmlSerializationWriterILGen(scopes, "public", writerClass);
            writerCodeGen.ModuleBuilder = moduleBuilder;

            writerCodeGen.GenerateBegin();
            string[] writeMethodNames = new string[xmlMappings.Length];

            for (int i = 0; i < xmlMappings.Length; i++) {
                writeMethodNames[i] = writerCodeGen.GenerateElement(xmlMappings[i]);
            }
            Type writerType = writerCodeGen.GenerateEnd();

            string readerClass = "XmlSerializationReader" + suffix;
            readerClass = classes.AddUnique(readerClass, readerClass);
            XmlSerializationReaderILGen readerCodeGen = new XmlSerializationReaderILGen(scopes, "public", readerClass);

            readerCodeGen.ModuleBuilder = moduleBuilder;
            readerCodeGen.CreatedTypes.Add(writerType.Name, writerType);

            readerCodeGen.GenerateBegin();
            string[] readMethodNames = new string[xmlMappings.Length];
            for (int i = 0; i < xmlMappings.Length; i++) {
                readMethodNames[i] = readerCodeGen.GenerateElement(xmlMappings[i]);
            }
            readerCodeGen.GenerateEnd(readMethodNames, xmlMappings, types);

            string baseSerializer = readerCodeGen.GenerateBaseSerializer("XmlSerializer1", readerClass, writerClass, classes);
            Hashtable serializers = new Hashtable();
            for (int i = 0; i < xmlMappings.Length; i++) {
                if (serializers[xmlMappings[i].Key] == null) {
                    serializers[xmlMappings[i].Key] = readerCodeGen.GenerateTypedSerializer(readMethodNames[i], writeMethodNames[i], xmlMappings[i], classes, baseSerializer, readerClass, writerClass);
                }
            }
            readerCodeGen.GenerateSerializerContract("XmlSerializerContract", xmlMappings, types, readerClass, readMethodNames, writerClass, writeMethodNames, serializers);

            if (DiagnosticsSwitches.KeepTempFiles.Enabled) {
                FileIOPermission.Assert();
                assemblyBuilder.Save(assemblyName + ".dll");
            }
            return writerType.Assembly;
        }

        // SxS: This method does not take any resource name and does not expose any resources to the caller.
        // It's OK to suppress the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        static MethodInfo GetMethodFromType(Type type, string methodName, Assembly assembly) {
            MethodInfo method = type.GetMethod(methodName);
            if (method != null)
                return method;

            MissingMethodException missingMethod = new MissingMethodException(type.FullName, methodName);
            if (assembly != null) {
                throw new InvalidOperationException(Res.GetString(Res.XmlSerializerExpired, assembly.FullName, assembly.CodeBase), missingMethod);
            }
            throw missingMethod;
        }

        internal static Type GetTypeFromAssembly(Assembly assembly, string typeName) {
            typeName = GeneratedAssemblyNamespace + "." + typeName;
            Type type = assembly.GetType(typeName);
            if (type == null) throw new InvalidOperationException(Res.GetString(Res.XmlMissingType, typeName, assembly.FullName));
            return type;
        }

        internal bool CanRead(XmlMapping mapping, XmlReader xmlReader) {
            if (mapping == null)
                return false;

            if (mapping.Accessor.Any) {
                return true;
            }
            TempMethod method = methods[mapping.Key];
            return xmlReader.IsStartElement(method.name, method.ns);
        }

        string ValidateEncodingStyle(string encodingStyle, string methodKey) {
            if (encodingStyle != null && encodingStyle.Length > 0) {
                if (methods[methodKey].isSoap) {
                    if (encodingStyle != Soap.Encoding && encodingStyle != Soap12.Encoding) {
                        throw new InvalidOperationException(Res.GetString(Res.XmlInvalidEncoding3, encodingStyle, Soap.Encoding, Soap12.Encoding));
                    }
                }
                else {
                    throw new InvalidOperationException(Res.GetString(Res.XmlInvalidEncodingNotEncoded1, encodingStyle));
                }
            }
            else {
                if (methods[methodKey].isSoap) {
                    encodingStyle = Soap.Encoding;
                }
            }
            return encodingStyle;
        }

        internal static FileIOPermission FileIOPermission {
            get {
                if (fileIOPermission == null)
                    fileIOPermission = new FileIOPermission(PermissionState.Unrestricted);
                return fileIOPermission;
            }
        }

        internal object InvokeReader(XmlMapping mapping, XmlReader xmlReader, XmlDeserializationEvents events, string encodingStyle) {
            XmlSerializationReader reader = null;
            try {
                encodingStyle = ValidateEncodingStyle(encodingStyle, mapping.Key);
                reader = Contract.Reader;
                reader.Init(xmlReader, events, encodingStyle, this);
                if (methods[mapping.Key].readMethod == null) {
                    if (readerMethods == null) {
                        readerMethods = Contract.ReadMethods;
                    }
                    string methodName = (string)readerMethods[mapping.Key];
                    if (methodName == null) {
                        throw new InvalidOperationException(Res.GetString(Res.XmlNotSerializable, mapping.Accessor.Name));
                    }
                    methods[mapping.Key].readMethod = GetMethodFromType(reader.GetType(), methodName, pregeneratedAssmbly ? this.assembly : null);
                }
                return methods[mapping.Key].readMethod.Invoke(reader, emptyObjectArray);
            }
            catch (SecurityException e) {
                throw new InvalidOperationException(Res.GetString(Res.XmlNoPartialTrust), e);
            }
            finally {
                if (reader != null)
                    reader.Dispose();
            }
        }

        internal void InvokeWriter(XmlMapping mapping, XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces, string encodingStyle, string id) {
            XmlSerializationWriter writer = null;
            try {
                encodingStyle = ValidateEncodingStyle(encodingStyle, mapping.Key);
                writer = Contract.Writer;
                writer.Init(xmlWriter, namespaces, encodingStyle, id, this);
                if (methods[mapping.Key].writeMethod == null) {
                    if (writerMethods == null) {
                        writerMethods = Contract.WriteMethods;
                    }
                    string methodName = (string)writerMethods[mapping.Key];
                    if (methodName == null) {
                        throw new InvalidOperationException(Res.GetString(Res.XmlNotSerializable, mapping.Accessor.Name));
                    }
                    methods[mapping.Key].writeMethod = GetMethodFromType(writer.GetType(), methodName, pregeneratedAssmbly ? assembly : null);
                }
                methods[mapping.Key].writeMethod.Invoke(writer, new object[] { o });
            }
            catch (SecurityException e) {
                throw new InvalidOperationException(Res.GetString(Res.XmlNoPartialTrust), e);
            }
            finally {
                if (writer != null)
                    writer.Dispose();
            }
        }

        internal Assembly GetReferencedAssembly(string name) {
            return assemblies != null && name != null ? (Assembly)assemblies[name] : null;
        }

        internal bool NeedAssembyResolve {
            get { return assemblies != null && assemblies.Count > 0; }
        }

        internal sealed class TempMethodDictionary : DictionaryBase {
            internal TempMethod this[string key] {
                get {
                    return (TempMethod)Dictionary[key];
                }
            }
            internal void Add(string key, TempMethod value) {
                Dictionary.Add(key, value);
            }
        }
    }

    sealed class XmlSerializerCompilerParameters {
        bool needTempDirAccess;
        CompilerParameters parameters;
        XmlSerializerCompilerParameters(CompilerParameters parameters, bool needTempDirAccess) {
            this.needTempDirAccess = needTempDirAccess;
            this.parameters = parameters;
        }

        internal bool IsNeedTempDirAccess { get { return this.needTempDirAccess; } }
        internal CompilerParameters CodeDomParameters { get { return this.parameters; } }

        internal static XmlSerializerCompilerParameters Create(string location) {
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;

            if (string.IsNullOrEmpty(location)) {
                XmlSerializerSection configSection = ConfigurationManager.GetSection(ConfigurationStrings.XmlSerializerSectionPath) as XmlSerializerSection;
                location = configSection == null ? location : configSection.TempFilesLocation;
                // Trim leading and trailing white spaces (VSWhidbey 229873)
                if (!string.IsNullOrEmpty(location)) {
                    location = location.Trim();
                }
            }
            parameters.TempFiles = new TempFileCollection(location);
            return new XmlSerializerCompilerParameters(parameters, string.IsNullOrEmpty(location));
        }

        internal static XmlSerializerCompilerParameters Create(CompilerParameters parameters, bool needTempDirAccess) {
            return new XmlSerializerCompilerParameters(parameters, needTempDirAccess);
        }
    }


    class TempAssemblyCacheKey {
        string ns;
        object type;

        internal TempAssemblyCacheKey(string ns, object type) {
            this.type = type;
            this.ns = ns;
        }

        public override bool Equals(object o) {
            TempAssemblyCacheKey key = o as TempAssemblyCacheKey;
            if (key == null) return false;
            return (key.type == this.type && key.ns == this.ns);
        }

        public override int GetHashCode() {
            return ((ns != null ? ns.GetHashCode() : 0) ^ (type != null ? type.GetHashCode() : 0));
        }
    }

    internal class TempAssemblyCache {
        Hashtable cache = new Hashtable();

        internal TempAssembly this[string ns, object o] {
            get { return (TempAssembly)cache[new TempAssemblyCacheKey(ns, o)]; }
        }

        internal void Add(string ns, object o, TempAssembly assembly) {
            TempAssemblyCacheKey key = new TempAssemblyCacheKey(ns, o);
            lock (this) {
                if (cache[key] == assembly) return;
                Hashtable clone = new Hashtable();
                foreach (object k in cache.Keys) {
                    clone.Add(k, cache[k]);
                }
                cache = clone;
                cache[key] = assembly;
            }
        }
    }
}

