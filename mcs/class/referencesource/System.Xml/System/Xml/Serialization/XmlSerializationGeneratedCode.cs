//------------------------------------------------------------------------------
// <copyright file="XmlSerializationGeneratedCode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;
    using System.IO;
    using System.Collections;
    using System.ComponentModel;
    using System.Threading;
    using System.Reflection;
    using System.Security;
    using System.Globalization;
   
    /// <include file='doc\XmlSerializationGeneratedCode.uex' path='docs/doc[@for="XmlSerializationGeneratedCode"]/*' />
    ///<internalonly/>
    public abstract class XmlSerializationGeneratedCode {
        TempAssembly tempAssembly;
        int threadCode;
        ResolveEventHandler assemblyResolver;

        internal void Init(TempAssembly tempAssembly) {
            this.tempAssembly = tempAssembly;
            // only hook the assembly resolver if we have something to help us do the resolution
            if (tempAssembly != null && tempAssembly.NeedAssembyResolve) {
                // we save the threadcode to make sure we don't handle any resolve events for any other threads
                threadCode = Thread.CurrentThread.GetHashCode();
                assemblyResolver = new ResolveEventHandler(OnAssemblyResolve);
                AppDomain.CurrentDomain.AssemblyResolve += assemblyResolver;
            }
        }

        // this method must be called at the end of serialization
        internal void Dispose() {
            if (assemblyResolver != null)
                AppDomain.CurrentDomain.AssemblyResolve -= assemblyResolver;
            assemblyResolver = null;
        }

        internal Assembly OnAssemblyResolve(object sender, ResolveEventArgs args) {
            if (tempAssembly != null && Thread.CurrentThread.GetHashCode() == threadCode)
                return tempAssembly.GetReferencedAssembly(args.Name);
            return null;
        }
    }

    internal class XmlSerializationCodeGen {
        IndentedWriter writer;
        int nextMethodNumber = 0;
        Hashtable methodNames = new Hashtable();
        ReflectionAwareCodeGen raCodeGen;
        TypeScope[] scopes;
        TypeDesc stringTypeDesc = null;
        TypeDesc qnameTypeDesc = null;
        string access;
        string className;
        TypeMapping[] referencedMethods;
        int references = 0;
        Hashtable generatedMethods = new Hashtable();

        internal XmlSerializationCodeGen(IndentedWriter writer, TypeScope[] scopes, string access, string className) {
            this.writer = writer;
            this.scopes = scopes;
            if (scopes.Length > 0) {
                stringTypeDesc = scopes[0].GetTypeDesc(typeof(string));
                qnameTypeDesc = scopes[0].GetTypeDesc(typeof(XmlQualifiedName));
            }
            this.raCodeGen = new ReflectionAwareCodeGen(writer);
            this.className = className;
            this.access = access;
        }

        internal IndentedWriter Writer { get { return writer; } }
        internal int NextMethodNumber { get { return nextMethodNumber; } set { nextMethodNumber = value; } }
        internal ReflectionAwareCodeGen RaCodeGen { get { return raCodeGen; } }
        internal TypeDesc StringTypeDesc { get { return stringTypeDesc; } }
        internal TypeDesc QnameTypeDesc { get { return qnameTypeDesc; } }
        internal string ClassName { get { return className; } }
        internal string Access { get { return access; } }
        internal TypeScope[] Scopes { get { return scopes; } }
        internal Hashtable MethodNames { get { return methodNames; } }
        internal Hashtable GeneratedMethods { get { return generatedMethods; } }

        internal virtual void GenerateMethod(TypeMapping mapping){}

        internal void GenerateReferencedMethods() {
            while(references > 0) {
                TypeMapping mapping = referencedMethods[--references];
                GenerateMethod(mapping);
            }
        }

        internal string ReferenceMapping(TypeMapping mapping) {
            if (!mapping.IsSoap) {
                if (generatedMethods[mapping] == null) {
                    referencedMethods = EnsureArrayIndex(referencedMethods, references);
                    referencedMethods[references++] = mapping;
                }
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

        internal void WriteQuotedCSharpString(string value) {
            raCodeGen.WriteQuotedCSharpString(value);
        }

        internal void GenerateHashtableGetBegin(string privateName, string publicName) {
            writer.Write(typeof(Hashtable).FullName);
            writer.Write(" ");
            writer.Write(privateName);
            writer.WriteLine(" = null;");
            writer.Write("public override ");
            writer.Write(typeof(Hashtable).FullName);

            writer.Write(" ");
            writer.Write(publicName);
            writer.WriteLine(" {");
            writer.Indent++;

            writer.WriteLine("get {");
            writer.Indent++;

            writer.Write("if (");
            writer.Write(privateName);
            writer.WriteLine(" == null) {");
            writer.Indent++;

            writer.Write(typeof(Hashtable).FullName);
            writer.Write(" _tmp = new ");
            writer.Write(typeof(Hashtable).FullName);
            writer.WriteLine("();");

        }

        internal void GenerateHashtableGetEnd(string privateName) {
            writer.Write("if (");
            writer.Write(privateName);
            writer.Write(" == null) ");
            writer.Write(privateName);
            writer.WriteLine(" = _tmp;");
            writer.Indent--;
            writer.WriteLine("}");

            writer.Write("return ");
            writer.Write(privateName);
            writer.WriteLine(";");
            writer.Indent--;
            writer.WriteLine("}");

            writer.Indent--;
            writer.WriteLine("}");
        }
        internal void GeneratePublicMethods(string privateName, string publicName, string[] methods, XmlMapping[] xmlMappings) {
            GenerateHashtableGetBegin(privateName, publicName);
            if (methods != null && methods.Length != 0 && xmlMappings != null && xmlMappings.Length == methods.Length) {
                for (int i = 0; i < methods.Length; i++) {
                    if (methods[i] == null)
                        continue;
                    writer.Write("_tmp[");
                    WriteQuotedCSharpString(xmlMappings[i].Key);
                    writer.Write("] = ");
                    WriteQuotedCSharpString(methods[i]);
                    writer.WriteLine(";");
                }
            }
            GenerateHashtableGetEnd(privateName);
        }

        internal void GenerateSupportedTypes(Type[] types) {
            writer.Write("public override ");
            writer.Write(typeof(bool).FullName);
            writer.Write(" CanSerialize(");
            writer.Write(typeof(Type).FullName);
            writer.WriteLine(" type) {");
            writer.Indent++;
            Hashtable uniqueTypes = new Hashtable();
            for (int i = 0; i < types.Length; i++) {
                Type type = types[i];

                if (type == null)
                    continue;
                if (!type.IsPublic && !type.IsNestedPublic)
                    continue;
                if (uniqueTypes[type] != null)
                    continue;
                if (DynamicAssemblies.IsTypeDynamic(type))
                    continue;
                if (type.IsGenericType || type.ContainsGenericParameters && DynamicAssemblies.IsTypeDynamic(type.GetGenericArguments()))
                    continue;
                uniqueTypes[type] = type;
                writer.Write("if (type == typeof(");
                writer.Write(CodeIdentifier.GetCSharpName(type));
                writer.WriteLine(")) return true;");
            }
            writer.WriteLine("return false;");
            writer.Indent--;
            writer.WriteLine("}");
        }

        internal string GenerateBaseSerializer(string baseSerializer, string readerClass, string writerClass, CodeIdentifiers classes) {
            baseSerializer = CodeIdentifier.MakeValid(baseSerializer);
            baseSerializer = classes.AddUnique(baseSerializer, baseSerializer);

            writer.WriteLine();
            writer.Write("public abstract class ");
            writer.Write(CodeIdentifier.GetCSharpName(baseSerializer));
            writer.Write(" : ");
            writer.Write(typeof(XmlSerializer).FullName);
            writer.WriteLine(" {");
            writer.Indent++;

            writer.Write("protected override ");
            writer.Write(typeof(XmlSerializationReader).FullName);
            writer.WriteLine(" CreateReader() {");
            writer.Indent++;
            writer.Write("return new ");
            writer.Write(readerClass);
            writer.WriteLine("();");
            writer.Indent--;
            writer.WriteLine("}");

            writer.Write("protected override ");
            writer.Write(typeof(XmlSerializationWriter).FullName);
            writer.WriteLine(" CreateWriter() {");
            writer.Indent++;
            writer.Write("return new ");
            writer.Write(writerClass);
            writer.WriteLine("();");
            writer.Indent--;
            writer.WriteLine("}");

            writer.Indent--;
            writer.WriteLine("}");

            return baseSerializer;
        }

        internal string GenerateTypedSerializer(string readMethod, string writeMethod, XmlMapping mapping, CodeIdentifiers classes, string baseSerializer, string readerClass, string writerClass) {
            string serializerName = CodeIdentifier.MakeValid(Accessor.UnescapeName(mapping.Accessor.Mapping.TypeDesc.Name));
            serializerName = classes.AddUnique(serializerName + "Serializer", mapping);

            writer.WriteLine();
            writer.Write("public sealed class ");
            writer.Write(CodeIdentifier.GetCSharpName(serializerName));
            writer.Write(" : ");
            writer.Write(baseSerializer);
            writer.WriteLine(" {");
            writer.Indent++;

            writer.WriteLine();
            writer.Write("public override ");
            writer.Write(typeof(bool).FullName);
            writer.Write(" CanDeserialize(");
            writer.Write(typeof(XmlReader).FullName);
            writer.WriteLine(" xmlReader) {");
            writer.Indent++;

            if (mapping.Accessor.Any) {
                writer.WriteLine("return true;");
            }
            else {
                writer.Write("return xmlReader.IsStartElement(");
                WriteQuotedCSharpString(mapping.Accessor.Name);
                writer.Write(", ");
                WriteQuotedCSharpString(mapping.Accessor.Namespace);
                writer.WriteLine(");");
            }
            writer.Indent--;
            writer.WriteLine("}");

            if (writeMethod != null) {
                writer.WriteLine();
                writer.Write("protected override void Serialize(object objectToSerialize, ");
                writer.Write(typeof(XmlSerializationWriter).FullName);
                writer.WriteLine(" writer) {");
                writer.Indent++;
                writer.Write("((");
                writer.Write(writerClass);
                writer.Write(")writer).");
                writer.Write(writeMethod);
                writer.Write("(");
                if (mapping is XmlMembersMapping) {
                    writer.Write("(object[])");
                }
                writer.WriteLine("objectToSerialize);");
                writer.Indent--;
                writer.WriteLine("}");
            }
            if (readMethod != null) {
                writer.WriteLine();
                writer.Write("protected override object Deserialize(");
                writer.Write(typeof(XmlSerializationReader).FullName);
                writer.WriteLine(" reader) {");
                writer.Indent++;
                writer.Write("return ((");
                writer.Write(readerClass);
                writer.Write(")reader).");
                writer.Write(readMethod);
                writer.WriteLine("();");
                writer.Indent--;
                writer.WriteLine("}");
            }
            writer.Indent--;
            writer.WriteLine("}");

            return serializerName;
        }

        void GenerateTypedSerializers(Hashtable serializers) {
            string privateName = "typedSerializers";
            GenerateHashtableGetBegin(privateName, "TypedSerializers");

            foreach (string key in serializers.Keys) {
                writer.Write("_tmp.Add(");
                WriteQuotedCSharpString(key);
                writer.Write(", new ");
                writer.Write((string)serializers[key]);
                writer.WriteLine("());");
            }
            GenerateHashtableGetEnd("typedSerializers");
        }

        //GenerateGetSerializer(serializers, xmlMappings);
        void GenerateGetSerializer(Hashtable serializers, XmlMapping[] xmlMappings) {
            writer.Write("public override ");
            writer.Write(typeof(XmlSerializer).FullName);
            writer.Write(" GetSerializer(");
            writer.Write(typeof(Type).FullName);
            writer.WriteLine(" type) {");
            writer.Indent++;

            for (int i = 0; i < xmlMappings.Length; i++) {
                if (xmlMappings[i] is XmlTypeMapping) {
                    Type type = xmlMappings[i].Accessor.Mapping.TypeDesc.Type;
                    if (type == null)
                        continue;
                    if (!type.IsPublic && !type.IsNestedPublic)
                        continue;
                    if (DynamicAssemblies.IsTypeDynamic(type))
                        continue;
                    if (type.IsGenericType || type.ContainsGenericParameters && DynamicAssemblies.IsTypeDynamic(type.GetGenericArguments()))
                        continue;
                    writer.Write("if (type == typeof(");
                    writer.Write(CodeIdentifier.GetCSharpName(type));
                    writer.Write(")) return new ");
                    writer.Write((string)serializers[xmlMappings[i].Key]);
                    writer.WriteLine("();");
                }
            }
            writer.WriteLine("return null;");
            writer.Indent--;
            writer.WriteLine("}");
        }

        internal void GenerateSerializerContract(string className, XmlMapping[] xmlMappings, Type[] types, string readerType, string[] readMethods, string writerType, string[] writerMethods, Hashtable serializers) {
            writer.WriteLine();
            writer.Write("public class XmlSerializerContract : global::");
            writer.Write(typeof(XmlSerializerImplementation).FullName);
            writer.WriteLine(" {");
            writer.Indent++;

            writer.Write("public override global::");
            writer.Write(typeof(XmlSerializationReader).FullName);
            writer.Write(" Reader { get { return new ");
            writer.Write(readerType);
            writer.WriteLine("(); } }");

            writer.Write("public override global::");
            writer.Write(typeof(XmlSerializationWriter).FullName);
            writer.Write(" Writer { get { return new ");
            writer.Write(writerType);
            writer.WriteLine("(); } }");

            GeneratePublicMethods("readMethods", "ReadMethods", readMethods, xmlMappings);
            GeneratePublicMethods("writeMethods", "WriteMethods", writerMethods, xmlMappings);
            GenerateTypedSerializers(serializers);
            GenerateSupportedTypes(types);
            GenerateGetSerializer(serializers, xmlMappings);

            writer.Indent--;
            writer.WriteLine("}");

        }

        internal static bool IsWildcard(SpecialMapping mapping) {
            if (mapping is SerializableMapping)
                return ((SerializableMapping)mapping).IsAny;
            return mapping.TypeDesc.CanBeElementValue;
        }
    }
}
