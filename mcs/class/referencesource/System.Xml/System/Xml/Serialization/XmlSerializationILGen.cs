//------------------------------------------------------------------------------
// <copyright file="XmlSerializationILGen.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Text.RegularExpressions;

    internal class XmlSerializationILGen {
        int nextMethodNumber = 0;
        Hashtable methodNames = new Hashtable();
        // Lookup name->created Method
        Dictionary<string, MethodBuilderInfo> methodBuilders = new Dictionary<string, MethodBuilderInfo>();
        // Lookup name->created Type
        internal Dictionary<string, Type> CreatedTypes = new Dictionary<string, Type>();
        // Lookup name->class Member
        internal Dictionary<string, MemberInfo> memberInfos = new Dictionary<string, MemberInfo>();
        ReflectionAwareILGen raCodeGen;
        TypeScope[] scopes;
        TypeDesc stringTypeDesc = null;
        TypeDesc qnameTypeDesc = null;
        string className;
        TypeMapping[] referencedMethods;
        int references = 0;
        Hashtable generatedMethods = new Hashtable();
        ModuleBuilder moduleBuilder;
        TypeAttributes typeAttributes;
        protected TypeBuilder typeBuilder;
        protected CodeGenerator ilg;

        internal XmlSerializationILGen(TypeScope[] scopes, string access, string className) {
            this.scopes = scopes;
            if (scopes.Length > 0) {
                stringTypeDesc = scopes[0].GetTypeDesc(typeof(string));
                qnameTypeDesc = scopes[0].GetTypeDesc(typeof(XmlQualifiedName));
            }
            this.raCodeGen = new ReflectionAwareILGen();
            this.className = className;
            System.Diagnostics.Debug.Assert(access == "public");
            this.typeAttributes = TypeAttributes.Public;
        }

        internal int NextMethodNumber { get { return nextMethodNumber; } set { nextMethodNumber = value; } }
        internal ReflectionAwareILGen RaCodeGen { get { return raCodeGen; } }
        internal TypeDesc StringTypeDesc { get { return stringTypeDesc; } }
        internal TypeDesc QnameTypeDesc { get { return qnameTypeDesc; } }
        internal string ClassName { get { return className; } }
        internal TypeScope[] Scopes { get { return scopes; } }
        internal Hashtable MethodNames { get { return methodNames; } }
        internal Hashtable GeneratedMethods { get { return generatedMethods; } }

        internal ModuleBuilder ModuleBuilder {
            get { System.Diagnostics.Debug.Assert(moduleBuilder != null); return moduleBuilder; }
            set { System.Diagnostics.Debug.Assert(moduleBuilder == null && value != null); moduleBuilder = value; }
        }
        internal TypeAttributes TypeAttributes { get { return typeAttributes; } }

        static Dictionary<string, Regex> regexs = new Dictionary<string, Regex>();
        static internal Regex NewRegex(string pattern) {
            Regex regex;
            lock (regexs) {
                if (!regexs.TryGetValue(pattern, out regex)) {
                    regex = new Regex(pattern);
                    regexs.Add(pattern, regex);
                }
            }
            return regex;
        }

        internal MethodBuilder EnsureMethodBuilder(TypeBuilder typeBuilder, string methodName,
            MethodAttributes attributes, Type returnType, Type[] parameterTypes) {
            MethodBuilderInfo methodBuilderInfo;
            if (!methodBuilders.TryGetValue(methodName, out methodBuilderInfo)) {
                MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                    methodName,
                    attributes,
                    returnType,
                    parameterTypes);
                methodBuilderInfo = new MethodBuilderInfo(methodBuilder, parameterTypes);
                methodBuilders.Add(methodName, methodBuilderInfo);
            }
#if DEBUG
            else {
                methodBuilderInfo.Validate(returnType, parameterTypes, attributes);

            }
#endif
            return methodBuilderInfo.MethodBuilder;
        }

        internal MethodBuilderInfo GetMethodBuilder(string methodName) {
            System.Diagnostics.Debug.Assert(methodBuilders.ContainsKey(methodName));
            return methodBuilders[methodName];
        }
        internal virtual void GenerateMethod(TypeMapping mapping) { }

        internal void GenerateReferencedMethods() {
            while (references > 0) {
                TypeMapping mapping = referencedMethods[--references];
                GenerateMethod(mapping);
            }
        }

        internal string ReferenceMapping(TypeMapping mapping) {
            if (generatedMethods[mapping] == null) {
                referencedMethods = EnsureArrayIndex(referencedMethods, references);
                referencedMethods[references++] = mapping;
            }
            return (string)methodNames[mapping];
        }

        TypeMapping[] EnsureArrayIndex(TypeMapping[] a, int index) {
            if (a == null) return new TypeMapping[32];
            if (index < a.Length) return a;
            TypeMapping[] b = new TypeMapping[a.Length + 32];
            Array.Copy(a, b, index);
            return b;
        }

        internal FieldBuilder GenerateHashtableGetBegin(string privateName, string publicName, TypeBuilder serializerContractTypeBuilder) {
            FieldBuilder fieldBuilder = serializerContractTypeBuilder.DefineField(
                privateName,
                typeof(Hashtable),
                FieldAttributes.Private
                );
            ilg = new CodeGenerator(serializerContractTypeBuilder);
            PropertyBuilder propertyBuilder = serializerContractTypeBuilder.DefineProperty(
                publicName,
                PropertyAttributes.None,
                CallingConventions.HasThis,
                typeof(Hashtable),
                null, null, null, null, null);

            ilg.BeginMethod(
                typeof(Hashtable),
                "get_" + publicName,
                CodeGenerator.EmptyTypeArray,
                CodeGenerator.EmptyStringArray,
                CodeGenerator.PublicOverrideMethodAttributes | MethodAttributes.SpecialName);
            propertyBuilder.SetGetMethod(ilg.MethodBuilder);

            ilg.Ldarg(0);
            ilg.LoadMember(fieldBuilder);
            ilg.Load(null);
            // this 'if' ends in GenerateHashtableGetEnd
            ilg.If(Cmp.EqualTo);

            ConstructorInfo Hashtable_ctor = typeof(Hashtable).GetConstructor(
                CodeGenerator.InstanceBindingFlags,
                null,
                CodeGenerator.EmptyTypeArray,
                null
                );
            LocalBuilder _tmpLoc = ilg.DeclareLocal(typeof(Hashtable), "_tmp");
            ilg.New(Hashtable_ctor);
            ilg.Stloc(_tmpLoc);

            return fieldBuilder;
        }

        internal void GenerateHashtableGetEnd(FieldBuilder fieldBuilder) {
            ilg.Ldarg(0);
            ilg.LoadMember(fieldBuilder);
            ilg.Load(null);
            ilg.If(Cmp.EqualTo);
            {
                ilg.Ldarg(0);
                ilg.Ldloc(typeof(Hashtable), "_tmp");
                ilg.StoreMember(fieldBuilder);
            }
            ilg.EndIf();
            // 'endif' from GenerateHashtableGetBegin
            ilg.EndIf();

            ilg.Ldarg(0);
            ilg.LoadMember(fieldBuilder);
            ilg.GotoMethodEnd();

            ilg.EndMethod();
        }
        internal FieldBuilder GeneratePublicMethods(string privateName, string publicName, string[] methods, XmlMapping[] xmlMappings, TypeBuilder serializerContractTypeBuilder) {
            FieldBuilder fieldBuilder = GenerateHashtableGetBegin(privateName, publicName, serializerContractTypeBuilder);
            if (methods != null && methods.Length != 0 && xmlMappings != null && xmlMappings.Length == methods.Length) {
                MethodInfo Hashtable_set_Item = typeof(Hashtable).GetMethod(
                    "set_Item",
                    CodeGenerator.InstanceBindingFlags,
                    null,
                    new Type[] { typeof(Object), typeof(Object) },
                    null
                    );
                for (int i = 0; i < methods.Length; i++) {
                    if (methods[i] == null)
                        continue;
                    ilg.Ldloc(typeof(Hashtable), "_tmp");
                    ilg.Ldstr(xmlMappings[i].Key);
                    ilg.Ldstr(methods[i]);
                    ilg.Call(Hashtable_set_Item);
                }
            }
            GenerateHashtableGetEnd(fieldBuilder);
            return fieldBuilder;
        }

        internal void GenerateSupportedTypes(Type[] types, TypeBuilder serializerContractTypeBuilder) {
            ilg = new CodeGenerator(serializerContractTypeBuilder);
            ilg.BeginMethod(
                typeof(bool),
                "CanSerialize",
                new Type[] { typeof(Type) },
                new string[] { "type" },
                CodeGenerator.PublicOverrideMethodAttributes);
            Hashtable uniqueTypes = new Hashtable();
            for (int i = 0; i < types.Length; i++) {
                Type type = types[i];

                if (type == null)
                    continue;
                if (!type.IsPublic && !type.IsNestedPublic)
                    continue;
                if (uniqueTypes[type] != null)
                    continue;
                // DDB172141: Wrong generated CS for serializer of List<string> type
                if (type.IsGenericType || type.ContainsGenericParameters)
                    continue;
                uniqueTypes[type] = type;
                ilg.Ldarg("type");
                ilg.Ldc(type);
                ilg.If(Cmp.EqualTo);
                {
                    ilg.Ldc(true);
                    ilg.GotoMethodEnd();
                }
                ilg.EndIf();
            }
            ilg.Ldc(false);
            ilg.GotoMethodEnd();
            ilg.EndMethod();
        }

        internal string GenerateBaseSerializer(string baseSerializer, string readerClass, string writerClass, CodeIdentifiers classes) {
            baseSerializer = CodeIdentifier.MakeValid(baseSerializer);
            baseSerializer = classes.AddUnique(baseSerializer, baseSerializer);

            TypeBuilder baseSerializerTypeBuilder = CodeGenerator.CreateTypeBuilder(
                this.moduleBuilder,
                CodeIdentifier.GetCSharpName(baseSerializer),
                TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.BeforeFieldInit,
                typeof(XmlSerializer),
                CodeGenerator.EmptyTypeArray);

            ConstructorInfo readerCtor = CreatedTypes[readerClass].GetConstructor(
               CodeGenerator.InstanceBindingFlags,
               null,
               CodeGenerator.EmptyTypeArray,
               null
               );
            ilg = new CodeGenerator(baseSerializerTypeBuilder);
            ilg.BeginMethod(typeof(XmlSerializationReader),
                "CreateReader",
                CodeGenerator.EmptyTypeArray,
                CodeGenerator.EmptyStringArray,
                CodeGenerator.ProtectedOverrideMethodAttributes);
            ilg.New(readerCtor);
            ilg.EndMethod();

            ConstructorInfo writerCtor = CreatedTypes[writerClass].GetConstructor(
               CodeGenerator.InstanceBindingFlags,
               null,
               CodeGenerator.EmptyTypeArray,
               null
               );
            ilg.BeginMethod(typeof(XmlSerializationWriter),
                "CreateWriter",
                CodeGenerator.EmptyTypeArray,
                CodeGenerator.EmptyStringArray,
                CodeGenerator.ProtectedOverrideMethodAttributes);
            ilg.New(writerCtor);
            ilg.EndMethod();

            baseSerializerTypeBuilder.DefineDefaultConstructor(
                MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            Type baseSerializerType = baseSerializerTypeBuilder.CreateType();
            CreatedTypes.Add(baseSerializerType.Name, baseSerializerType);

            return baseSerializer;
        }

        internal string GenerateTypedSerializer(string readMethod, string writeMethod, XmlMapping mapping, CodeIdentifiers classes, string baseSerializer, string readerClass, string writerClass) {
            string serializerName = CodeIdentifier.MakeValid(Accessor.UnescapeName(mapping.Accessor.Mapping.TypeDesc.Name));
            serializerName = classes.AddUnique(serializerName + "Serializer", mapping);

            TypeBuilder typedSerializerTypeBuilder = CodeGenerator.CreateTypeBuilder(
                this.moduleBuilder,
                CodeIdentifier.GetCSharpName(serializerName),
                TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                CreatedTypes[baseSerializer],
                CodeGenerator.EmptyTypeArray
                );

            ilg = new CodeGenerator(typedSerializerTypeBuilder);
            ilg.BeginMethod(
                typeof(Boolean),
                "CanDeserialize",
                new Type[] { typeof(XmlReader) },
                new string[] { "xmlReader" },
                CodeGenerator.PublicOverrideMethodAttributes
            );

            if (mapping.Accessor.Any) {
                ilg.Ldc(true);
                ilg.Stloc(ilg.ReturnLocal);
                ilg.Br(ilg.ReturnLabel);
            }
            else {
                MethodInfo XmlReader_IsStartElement = typeof(XmlReader).GetMethod(
                     "IsStartElement",
                     CodeGenerator.InstanceBindingFlags,
                     null,
                     new Type[] { typeof(String), typeof(String) },
                     null
                     );
                ilg.Ldarg(ilg.GetArg("xmlReader"));
                ilg.Ldstr(mapping.Accessor.Name);
                ilg.Ldstr(mapping.Accessor.Namespace);
                ilg.Call(XmlReader_IsStartElement);
                ilg.Stloc(ilg.ReturnLocal);
                ilg.Br(ilg.ReturnLabel);
            }
            ilg.MarkLabel(ilg.ReturnLabel);
            ilg.Ldloc(ilg.ReturnLocal);
            ilg.EndMethod();

            if (writeMethod != null) {
                ilg = new CodeGenerator(typedSerializerTypeBuilder);
                ilg.BeginMethod(
                    typeof(void),
                    "Serialize",
                    new Type[] { typeof(object), typeof(XmlSerializationWriter) },
                    new string[] { "objectToSerialize", "writer" },
                    CodeGenerator.ProtectedOverrideMethodAttributes);
                MethodInfo writerType_writeMethod = CreatedTypes[writerClass].GetMethod(
                    writeMethod,
                    CodeGenerator.InstanceBindingFlags,
                    null,
                    new Type[] { (mapping is XmlMembersMapping) ? typeof(object[]) : typeof(object) },
                    null
                    );
                ilg.Ldarg("writer");
                ilg.Castclass(CreatedTypes[writerClass]);
                ilg.Ldarg("objectToSerialize");
                if (mapping is XmlMembersMapping) {
                    ilg.ConvertValue(typeof(object), typeof(object[]));
                }
                ilg.Call(writerType_writeMethod);
                ilg.EndMethod();
            }
            if (readMethod != null) {
                ilg = new CodeGenerator(typedSerializerTypeBuilder);
                ilg.BeginMethod(
                    typeof(object),
                    "Deserialize",
                    new Type[] { typeof(XmlSerializationReader) },
                    new string[] { "reader" },
                    CodeGenerator.ProtectedOverrideMethodAttributes);
                MethodInfo readerType_readMethod = CreatedTypes[readerClass].GetMethod(
                    readMethod,
                    CodeGenerator.InstanceBindingFlags,
                    null,
                    CodeGenerator.EmptyTypeArray,
                    null
                    );
                ilg.Ldarg("reader");
                ilg.Castclass(CreatedTypes[readerClass]);
                ilg.Call(readerType_readMethod);
                ilg.EndMethod();
            }
            typedSerializerTypeBuilder.DefineDefaultConstructor(CodeGenerator.PublicMethodAttributes);
            Type typedSerializerType = typedSerializerTypeBuilder.CreateType();
            CreatedTypes.Add(typedSerializerType.Name, typedSerializerType);

            return typedSerializerType.Name;
        }

        FieldBuilder GenerateTypedSerializers(Hashtable serializers, TypeBuilder serializerContractTypeBuilder) {
            string privateName = "typedSerializers";
            FieldBuilder fieldBuilder = GenerateHashtableGetBegin(privateName, "TypedSerializers", serializerContractTypeBuilder);
            MethodInfo Hashtable_Add = typeof(Hashtable).GetMethod(
                "Add",
                CodeGenerator.InstanceBindingFlags,
                null,
                new Type[] { typeof(Object), typeof(Object) },
                null
                );

            foreach (string key in serializers.Keys) {
                ConstructorInfo ctor = CreatedTypes[(string)serializers[key]].GetConstructor(
                    CodeGenerator.InstanceBindingFlags,
                    null,
                    CodeGenerator.EmptyTypeArray,
                    null
                    );
                ilg.Ldloc(typeof(Hashtable), "_tmp");
                ilg.Ldstr(key);
                ilg.New(ctor);
                ilg.Call(Hashtable_Add);
            }
            GenerateHashtableGetEnd(fieldBuilder);
            return fieldBuilder;
        }

        //GenerateGetSerializer(serializers, xmlMappings);
        void GenerateGetSerializer(Hashtable serializers, XmlMapping[] xmlMappings, TypeBuilder serializerContractTypeBuilder) {
            ilg = new CodeGenerator(serializerContractTypeBuilder);
            ilg.BeginMethod(
                typeof(XmlSerializer),
                "GetSerializer",
                new Type[] { typeof(Type) },
                new string[] { "type" },
                CodeGenerator.PublicOverrideMethodAttributes);

            for (int i = 0; i < xmlMappings.Length; i++) {
                if (xmlMappings[i] is XmlTypeMapping) {
                    Type type = xmlMappings[i].Accessor.Mapping.TypeDesc.Type;
                    if (type == null)
                        continue;
                    if (!type.IsPublic && !type.IsNestedPublic)
                        continue;
                    // DDB172141: Wrong generated CS for serializer of List<string> type
                    if (type.IsGenericType || type.ContainsGenericParameters)
                        continue;
                    ilg.Ldarg("type");
                    ilg.Ldc(type);
                    ilg.If(Cmp.EqualTo);
                    {
                        ConstructorInfo ctor = CreatedTypes[(string)serializers[xmlMappings[i].Key]].GetConstructor(
                            CodeGenerator.InstanceBindingFlags,
                            null,
                            CodeGenerator.EmptyTypeArray,
                            null
                            );
                        ilg.New(ctor);
                        ilg.Stloc(ilg.ReturnLocal);
                        ilg.Br(ilg.ReturnLabel);
                    }
                    ilg.EndIf();
                }
            }
            ilg.Load(null);
            ilg.Stloc(ilg.ReturnLocal);
            ilg.Br(ilg.ReturnLabel);
            ilg.MarkLabel(ilg.ReturnLabel);
            ilg.Ldloc(ilg.ReturnLocal);
            ilg.EndMethod();
        }

        internal void GenerateSerializerContract(string className, XmlMapping[] xmlMappings, Type[] types, string readerType, string[] readMethods, string writerType, string[] writerMethods, Hashtable serializers) {
            TypeBuilder serializerContractTypeBuilder = CodeGenerator.CreateTypeBuilder(
                this.moduleBuilder,
                "XmlSerializerContract",
                TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
                typeof(XmlSerializerImplementation),
                CodeGenerator.EmptyTypeArray
                );

            ilg = new CodeGenerator(serializerContractTypeBuilder);
            PropertyBuilder propertyBuilder = serializerContractTypeBuilder.DefineProperty(
                "Reader",
                PropertyAttributes.None,
                CallingConventions.HasThis,
                typeof(XmlSerializationReader),
                null, null, null, null, null);
            ilg.BeginMethod(
                typeof(XmlSerializationReader),
                "get_Reader",
                CodeGenerator.EmptyTypeArray,
                CodeGenerator.EmptyStringArray,
                CodeGenerator.PublicOverrideMethodAttributes | MethodAttributes.SpecialName);
            propertyBuilder.SetGetMethod(ilg.MethodBuilder);
            ConstructorInfo ctor = CreatedTypes[readerType].GetConstructor(
                CodeGenerator.InstanceBindingFlags,
                null,
                CodeGenerator.EmptyTypeArray,
                null
                );
            ilg.New(ctor);
            ilg.EndMethod();

            ilg = new CodeGenerator(serializerContractTypeBuilder);
            propertyBuilder = serializerContractTypeBuilder.DefineProperty(
                "Writer",
                PropertyAttributes.None,
                CallingConventions.HasThis,
                typeof(XmlSerializationWriter),
                null, null, null, null, null);
            ilg.BeginMethod(
                typeof(XmlSerializationWriter),
                "get_Writer",
                CodeGenerator.EmptyTypeArray,
                CodeGenerator.EmptyStringArray,
                CodeGenerator.PublicOverrideMethodAttributes | MethodAttributes.SpecialName);
            propertyBuilder.SetGetMethod(ilg.MethodBuilder);
            ctor = CreatedTypes[writerType].GetConstructor(
                CodeGenerator.InstanceBindingFlags,
                null,
                CodeGenerator.EmptyTypeArray,
                null
                );
            ilg.New(ctor);
            ilg.EndMethod();

            FieldBuilder readMethodsField = GeneratePublicMethods("readMethods", "ReadMethods", readMethods, xmlMappings, serializerContractTypeBuilder);
            FieldBuilder writeMethodsField = GeneratePublicMethods("writeMethods", "WriteMethods", writerMethods, xmlMappings, serializerContractTypeBuilder);
            FieldBuilder typedSerializersField = GenerateTypedSerializers(serializers, serializerContractTypeBuilder);
            GenerateSupportedTypes(types, serializerContractTypeBuilder);
            GenerateGetSerializer(serializers, xmlMappings, serializerContractTypeBuilder);

            // Default ctor
            ConstructorInfo baseCtor = typeof(XmlSerializerImplementation).GetConstructor(
                CodeGenerator.InstanceBindingFlags,
                null,
                CodeGenerator.EmptyTypeArray,
                null
                );
            ilg = new CodeGenerator(serializerContractTypeBuilder);
            ilg.BeginMethod(
                typeof(void),
                ".ctor",
                CodeGenerator.EmptyTypeArray,
                CodeGenerator.EmptyStringArray,
                CodeGenerator.PublicMethodAttributes | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName
                );
            ilg.Ldarg(0);
            ilg.Load(null);
            ilg.StoreMember(readMethodsField);
            ilg.Ldarg(0);
            ilg.Load(null);
            ilg.StoreMember(writeMethodsField);
            ilg.Ldarg(0);
            ilg.Load(null);
            ilg.StoreMember(typedSerializersField);
            ilg.Ldarg(0);
            ilg.Call(baseCtor);
            ilg.EndMethod();
            // Instantiate type
            Type serializerContractType = serializerContractTypeBuilder.CreateType();
            CreatedTypes.Add(serializerContractType.Name, serializerContractType);

        }

        internal static bool IsWildcard(SpecialMapping mapping) {
            if (mapping is SerializableMapping)
                return ((SerializableMapping)mapping).IsAny;
            return mapping.TypeDesc.CanBeElementValue;
        }
        internal void ILGenLoad(string source) {
            ILGenLoad(source, null);
        }
        internal void ILGenLoad(string source, Type type) {
            if (source.StartsWith("o.@", StringComparison.Ordinal)) {
                System.Diagnostics.Debug.Assert(memberInfos.ContainsKey(source.Substring(3)));
                MemberInfo memInfo = memberInfos[source.Substring(3)];
                ilg.LoadMember(ilg.GetVariable("o"), memInfo);
                if (type != null) {
                    Type memType = (memInfo.MemberType == MemberTypes.Field) ? ((FieldInfo)memInfo).FieldType : ((PropertyInfo)memInfo).PropertyType;
                    ilg.ConvertValue(memType, type);
                }
            }
            else {
                SourceInfo info = new SourceInfo(source, null, null, null, ilg);
                info.Load(type);
            }
        }
    }
}
