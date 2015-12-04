//------------------------------------------------------------------------------
// <copyright file="XmlILModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="false">[....]</owner>
//------------------------------------------------------------------------------

using System.Collections;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using System.Xml.Xsl.Runtime;
using System.Runtime.Versioning;

namespace System.Xml.Xsl.IlGen {
    using DebuggingModes = DebuggableAttribute.DebuggingModes;

    internal enum XmlILMethodAttributes {
        None = 0,
        NonUser = 1,    // Non-user method which should debugger should step through
        Raw = 2,        // Raw method which should not add an implicit first argument of type XmlQueryRuntime
    }

    internal class XmlILModule {
        public static readonly PermissionSet CreateModulePermissionSet;     // Permission set that contains permissions required for generating module
        private static long AssemblyId;                                     // Unique identifier used to ensure that assembly names are unique within AppDomain
        private static ModuleBuilder LREModule;                             // Module used to emit dynamic lightweight-reflection-emit (LRE) methods

        private TypeBuilder typeBldr;
        private Hashtable methods, urlToSymWriter;
        private string modFile;
        private bool persistAsm, useLRE, emitSymbols;

        private static readonly Guid LanguageGuid = new Guid(0x462d4a3e, 0xb257, 0x4aee, 0x97, 0xcd, 0x59, 0x18, 0xc7, 0x53, 0x17, 0x58);
        private static readonly Guid VendorGuid = new Guid(0x994b45c4, 0xe6e9, 0x11d2, 0x90, 0x3f, 0x00, 0xc0, 0x4f, 0xa3, 0x02, 0xa1);
        private const string RuntimeName = "{" + XmlReservedNs.NsXslDebug + "}" + "runtime";

        static XmlILModule() {
            AssemblyName asmName;
            AssemblyBuilder asmBldr;

            CreateModulePermissionSet = new PermissionSet(PermissionState.None);
            // CreateDelegate demands MemberAccess permission
            CreateModulePermissionSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
            // DynamicMethod constructor demands ControlEvidence permissions. 
            // Emitting symbols in DefineDynamicModule (to allow to debug the stylesheet) requires UnmanagedCode permission. 
            CreateModulePermissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.UnmanagedCode));

            AssemblyId = 0;

            // 1. LRE assembly only needs to execute
            // 2. No temp files need be created
            // 3. Never allow assembly to Assert permissions
            asmName = CreateAssemblyName();
            asmBldr = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);

            try {
                CreateModulePermissionSet.Assert();

                // Add custom attribute to assembly marking it as security transparent so that Assert will not be allowed
                // and link demands will be converted to full demands.
                asmBldr.SetCustomAttribute(new CustomAttributeBuilder(XmlILConstructors.Transparent, new object[] {}));

                // Store LREModule once.  If multiple threads are doing this, then some threads might get different
                // modules.  This is OK, since it's not mandatory to share, just preferable.
                LREModule = asmBldr.DefineDynamicModule("System.Xml.Xsl.CompiledQuery", false);
            }
            finally {
                CodeAccessPermission.RevertAssert();
            }
        }

        public XmlILModule(TypeBuilder typeBldr) {
            this.typeBldr = typeBldr;

            this.emitSymbols = ((ModuleBuilder) this.typeBldr.Module).GetSymWriter() != null;
            this.useLRE = false;
            this.persistAsm = false;

            // Index all methods added to this module by unique name
            this.methods = new Hashtable();

            if (this.emitSymbols) {
                // Create mapping from source document to symbol writer
                this.urlToSymWriter = new Hashtable();
            }
        }

        public bool EmitSymbols {
            get {
                return this.emitSymbols;
            }
        }

        // SxS note: AssemblyBuilder.DefineDynamicModule() below may be using name which is not SxS safe. 
        // This file is written only for internal tracing/debugging purposes. In retail builds persistAsm 
        // will be always false and the file should never be written. As a result it's fine just to supress 
        // the the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        public XmlILModule(bool useLRE, bool emitSymbols) {
            AssemblyName asmName;
            AssemblyBuilder asmBldr;
            ModuleBuilder modBldr;
            Debug.Assert(!(useLRE && emitSymbols));

            this.useLRE = useLRE;
            this.emitSymbols = emitSymbols;
            this.persistAsm = false;

            // Index all methods added to this module by unique name
            this.methods = new Hashtable();

            if (!useLRE) {
                // 1. If assembly needs to support debugging, then it must be saved and re-loaded (rule of CLR)
                // 2. Get path of temp directory, where assembly will be saved
                // 3. Never allow assembly to Assert permissions
                asmName = CreateAssemblyName();

            #if DEBUG
                if (XmlILTrace.IsEnabled) {
                    this.modFile = "System.Xml.Xsl.CompiledQuery";
                    this.persistAsm = true;
                }
            #endif

                asmBldr = AppDomain.CurrentDomain.DefineDynamicAssembly(
                            asmName,
                            this.persistAsm ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.Run);

                // Add custom attribute to assembly marking it as security transparent so that Assert will not be allowed
                // and link demands will be converted to full demands.
                asmBldr.SetCustomAttribute(new CustomAttributeBuilder(XmlILConstructors.Transparent, new object[] { }));

                if (emitSymbols) {
                    // Create mapping from source document to symbol writer
                    this.urlToSymWriter = new Hashtable();

                    // Add DebuggableAttribute to assembly so that debugging is a better experience
                    DebuggingModes debuggingModes = DebuggingModes.Default | DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggingModes.DisableOptimizations;
                    asmBldr.SetCustomAttribute(new CustomAttributeBuilder(XmlILConstructors.Debuggable, new object[] {debuggingModes}));
                }

                // Create ModuleBuilder
                if (this.persistAsm)
                    modBldr = asmBldr.DefineDynamicModule("System.Xml.Xsl.CompiledQuery", this.modFile + ".dll", emitSymbols);
                else
                    modBldr = asmBldr.DefineDynamicModule("System.Xml.Xsl.CompiledQuery", emitSymbols);

                this.typeBldr = modBldr.DefineType("System.Xml.Xsl.CompiledQuery.Query", TypeAttributes.Public);
            }
        }

        /// <summary>
        /// Define a method in this module with the specified name and parameters.
        /// </summary>
        public MethodInfo DefineMethod(string name, Type returnType, Type[] paramTypes, string[] paramNames, XmlILMethodAttributes xmlAttrs) {
            MethodInfo methResult;
            int uniqueId = 1;
            string nameOrig = name;
            Type[] paramTypesNew;
            bool isRaw = (xmlAttrs & XmlILMethodAttributes.Raw) != 0;

            // Ensure that name is unique
            while (this.methods[name] != null) {
                // Add unique id to end of name in order to make it unique within this module
                uniqueId++;
                name = nameOrig + " (" + uniqueId + ")";
            }

            if (!isRaw) {
                // XmlQueryRuntime is always 0th parameter
                paramTypesNew = new Type[paramTypes.Length + 1];
                paramTypesNew[0] = typeof(XmlQueryRuntime);
                Array.Copy(paramTypes, 0, paramTypesNew, 1, paramTypes.Length);
                paramTypes = paramTypesNew;
            }

            if (!this.useLRE) {
                MethodBuilder methBldr;

                methBldr = this.typeBldr.DefineMethod(
                            name,
                            MethodAttributes.Private | MethodAttributes.Static, 
                            returnType,
                            paramTypes);

                if (emitSymbols && (xmlAttrs & XmlILMethodAttributes.NonUser) != 0) {
                    // Add DebuggerStepThroughAttribute and DebuggerNonUserCodeAttribute to non-user methods so that debugging is a better experience
                    methBldr.SetCustomAttribute(new CustomAttributeBuilder(XmlILConstructors.StepThrough, new object[] {}));
                    methBldr.SetCustomAttribute(new CustomAttributeBuilder(XmlILConstructors.NonUserCode, new object[] {}));
                }

                if (!isRaw)
                    methBldr.DefineParameter(1, ParameterAttributes.None, RuntimeName);

                for (int i = 0; i < paramNames.Length; i++) {
                    if (paramNames[i] != null && paramNames[i].Length != 0)
                        methBldr.DefineParameter(i + (isRaw ? 1 : 2), ParameterAttributes.None, paramNames[i]);
                }

                methResult = methBldr;
            }
            else {
                DynamicMethod methDyn = new DynamicMethod(name, returnType, paramTypes, LREModule);
                methDyn.InitLocals = true;

                if (!isRaw)
                    methDyn.DefineParameter(1, ParameterAttributes.None, RuntimeName);

                for (int i = 0; i < paramNames.Length; i++) {
                    if (paramNames[i] != null && paramNames[i].Length != 0)
                        methDyn.DefineParameter(i + (isRaw ? 1 : 2), ParameterAttributes.None, paramNames[i]);
                }

                methResult = methDyn;
            }

            // Index method by name
            this.methods[name] = methResult;
            return methResult;
        }

        /// <summary>
        /// Get an XmlILGenerator that can be used to generate the body of the specified method.
        /// </summary>
        public static ILGenerator DefineMethodBody(MethodBase methInfo) {
            DynamicMethod methDyn = methInfo as DynamicMethod;
            if (methDyn != null)
                return methDyn.GetILGenerator();

            MethodBuilder methBldr = methInfo as MethodBuilder;
            if (methBldr != null)
                return methBldr.GetILGenerator();

            return ((ConstructorBuilder) methInfo).GetILGenerator();
        }

        /// <summary>
        /// Find a MethodInfo of the specified name and return it.  Return null if no such method exists.
        /// </summary>
        public MethodInfo FindMethod(string name) {
            return (MethodInfo) this.methods[name];
        }

        /// <summary>
        /// Define ginitialized data field with the specified name and value.
        /// </summary>
        public FieldInfo DefineInitializedData(string name, byte[] data) {
            Debug.Assert(!this.useLRE, "Cannot create initialized data for an LRE module");
            return this.typeBldr.DefineInitializedData(name, data, FieldAttributes.Private | FieldAttributes.Static);
        }

        /// <summary>
        /// Define private static field with the specified name and value.
        /// </summary>
        public FieldInfo DefineField(string fieldName, Type type) {
            Debug.Assert(!this.useLRE, "Cannot create field for an LRE module");
            return this.typeBldr.DefineField(fieldName, type, FieldAttributes.Private | FieldAttributes.Static);
        }

        /// <summary>
        /// Define static constructor for this type.
        /// </summary>
        public ConstructorInfo DefineTypeInitializer() {
            Debug.Assert(!this.useLRE, "Cannot create type initializer for an LRE module");
            return this.typeBldr.DefineTypeInitializer();
        }

        /// <summary>
        /// Add the file name of a document containing source code for this module and return a symbol writer.
        /// </summary>
        public ISymbolDocumentWriter AddSourceDocument(string fileName) {
            ISymbolDocumentWriter symDoc;
            Debug.Assert(this.emitSymbols, "Cannot add source information to a module that doesn't allow symbols.");

            symDoc = this.urlToSymWriter[fileName] as ISymbolDocumentWriter;
            if (symDoc == null) {
                symDoc = ((ModuleBuilder) this.typeBldr.Module).DefineDocument(fileName, LanguageGuid, VendorGuid, Guid.Empty);
                this.urlToSymWriter.Add(fileName, symDoc);
            }

            return symDoc;
        }

        /// <summary>
        /// Once all methods have been defined, CreateModule must be called in order to "bake" the methods within
        /// this module.
        /// </summary>
        // SxS note: AssemblyBuilder.Save() below is using name which is not SxS safe. This file is written only for 
        // internal tracing/debugging purposes. In retail builds persistAsm will be always false and the file should 
        // never be written. As a result it's fine just to supress the the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        public void BakeMethods() {
            Type typBaked;
            Hashtable methodsBaked;

            if (!this.useLRE) {
                typBaked = this.typeBldr.CreateType();

                if (this.persistAsm) {
                    // Persist the assembly to disk
                    ((AssemblyBuilder) this.typeBldr.Module.Assembly).Save(this.modFile + ".dll");
                }

                // Replace all MethodInfos in this.methods
                methodsBaked = new Hashtable(this.methods.Count);
                foreach (string methName in this.methods.Keys) {
                    methodsBaked[methName] = typBaked.GetMethod(methName, BindingFlags.NonPublic | BindingFlags.Static);
                }
                this.methods = methodsBaked;

                // Release TypeBuilder and symbol writer resources
                this.typeBldr = null;
                this.urlToSymWriter = null;
            }
        }

        /// <summary>
        /// Wrap a delegate around a MethodInfo of the specified name and type and return it.
        /// </summary>
        public Delegate CreateDelegate(string name, Type typDelegate) {
            if (!this.useLRE)
                return Delegate.CreateDelegate(typDelegate, (MethodInfo) this.methods[name]);

            return ((DynamicMethod) this.methods[name]).CreateDelegate(typDelegate);
        }

        /// <summary>
        /// Define unique assembly name (within AppDomain).
        /// </summary>
        private static AssemblyName CreateAssemblyName() {
            AssemblyName name;

            System.Threading.Interlocked.Increment(ref AssemblyId);
            name = new AssemblyName();
            name.Name = "System.Xml.Xsl.CompiledQuery." + AssemblyId;

            return name;
        }
    }
}
