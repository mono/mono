//-----------------------------------------------------------------------------
// <copyright file="SvcFileMapSerializer.cs" company="Microsoft">
//   Copyright (C) Microsoft Corporation. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------------

//  This file is generated. DO NOT MODIFY IT BY HAND.
// Please read HowToUpdateSerializer.txt in the parent directory to see how to update it.


namespace System.Web.Compilation.WCFModel.SvcMapFileXmlSerializer {

    internal class XmlSerializationWriterSvcMapFileImpl : System.Xml.Serialization.XmlSerializationWriter {

        public void Write16_ReferenceGroup(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteNullTagLiteral(@"ReferenceGroup", @"urn:schemas-microsoft-com:xml-wcfservicemap");
                return;
            }
            TopLevelElement();
            Write15_SvcMapFileImpl(@"ReferenceGroup", @"urn:schemas-microsoft-com:xml-wcfservicemap", ((global::System.Web.Compilation.WCFModel.SvcMapFileImpl)o), true, false);
        }

        void Write15_SvcMapFileImpl(string n, string ns, global::System.Web.Compilation.WCFModel.SvcMapFileImpl o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Compilation.WCFModel.SvcMapFileImpl)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"SvcMapFileImpl", @"urn:schemas-microsoft-com:xml-wcfservicemap");
            WriteAttribute(@"ID", @"", ((global::System.String)o.@ID));
            Write9_ClientOptions(@"ClientOptions", @"urn:schemas-microsoft-com:xml-wcfservicemap", ((global::System.Web.Compilation.WCFModel.ClientOptions)o.@ClientOptions), false, false);
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource>)o.@MetadataSourceList);
                if (a != null){
                    WriteStartElement(@"MetadataSources", @"urn:schemas-microsoft-com:xml-wcfservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write10_MetadataSource(@"MetadataSource", @"urn:schemas-microsoft-com:xml-wcfservicemap", ((global::System.Web.Compilation.WCFModel.MetadataSource)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile>)o.@MetadataList);
                if (a != null){
                    WriteStartElement(@"Metadata", @"urn:schemas-microsoft-com:xml-wcfservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write13_MetadataFile(@"MetadataFile", @"urn:schemas-microsoft-com:xml-wcfservicemap", ((global::System.Web.Compilation.WCFModel.MetadataFile)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile>)o.@Extensions);
                if (a != null){
                    WriteStartElement(@"Extensions", @"urn:schemas-microsoft-com:xml-wcfservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write14_ExtensionFile(@"ExtensionFile", @"urn:schemas-microsoft-com:xml-wcfservicemap", ((global::System.Web.Compilation.WCFModel.ExtensionFile)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            WriteEndElement(o);
        }

        void Write14_ExtensionFile(string n, string ns, global::System.Web.Compilation.WCFModel.ExtensionFile o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Compilation.WCFModel.ExtensionFile)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"ExtensionFile", @"urn:schemas-microsoft-com:xml-wcfservicemap");
            WriteAttribute(@"FileName", @"", ((global::System.String)o.@FileName));
            WriteAttribute(@"Name", @"", ((global::System.String)o.@Name));
            WriteEndElement(o);
        }

        void Write13_MetadataFile(string n, string ns, global::System.Web.Compilation.WCFModel.MetadataFile o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Compilation.WCFModel.MetadataFile)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"MetadataFile", @"urn:schemas-microsoft-com:xml-wcfservicemap");
            WriteAttribute(@"FileName", @"", ((global::System.String)o.@FileName));
            WriteAttribute(@"MetadataType", @"", Write12_MetadataType(((global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType)o.@FileType)));
            WriteAttribute(@"ID", @"", ((global::System.String)o.@ID));
            if (o.@IgnoreSpecified) {
                WriteAttribute(@"Ignore", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Ignore)));
            }
            if (o.@IsMergeResultSpecified) {
                WriteAttribute(@"IsMergeResult", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsMergeResult)));
            }
            if (o.@SourceIdSpecified) {
                WriteAttribute(@"SourceId", @"", System.Xml.XmlConvert.ToString((global::System.Int32)((global::System.Int32)o.@SourceId)));
            }
            WriteAttribute(@"SourceUrl", @"", ((global::System.String)o.@SourceUrl));
            if (o.@IgnoreSpecified) {
            }
            if (o.@IsMergeResultSpecified) {
            }
            if (o.@SourceIdSpecified) {
            }
            WriteEndElement(o);
        }

        string Write12_MetadataType(global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType v) {
            string s = null;
            switch (v) {
                case global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Unknown: s = @"Unknown"; break;
                case global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Disco: s = @"Disco"; break;
                case global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Wsdl: s = @"Wsdl"; break;
                case global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Schema: s = @"Schema"; break;
                case global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Policy: s = @"Policy"; break;
                case global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Xml: s = @"Xml"; break;
                case global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Edmx: s = @"Edmx"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"System.Web.Compilation.WCFModel.MetadataFile.MetadataType");
            }
            return s;
        }

        void Write10_MetadataSource(string n, string ns, global::System.Web.Compilation.WCFModel.MetadataSource o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Compilation.WCFModel.MetadataSource)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"MetadataSource", @"urn:schemas-microsoft-com:xml-wcfservicemap");
            WriteAttribute(@"Address", @"", ((global::System.String)o.@Address));
            WriteAttribute(@"Protocol", @"", ((global::System.String)o.@Protocol));
            WriteAttribute(@"SourceId", @"", System.Xml.XmlConvert.ToString((global::System.Int32)((global::System.Int32)o.@SourceId)));
            WriteEndElement(o);
        }

        void Write9_ClientOptions(string n, string ns, global::System.Web.Compilation.WCFModel.ClientOptions o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Compilation.WCFModel.ClientOptions)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"ClientOptions", @"urn:schemas-microsoft-com:xml-wcfservicemap");
            WriteElementStringRaw(@"GenerateAsynchronousMethods", @"urn:schemas-microsoft-com:xml-wcfservicemap", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@GenerateAsynchronousMethods)));
            if (o.@GenerateTaskBasedAsynchronousMethodSpecified) {
                WriteElementStringRaw(@"GenerateTaskBasedAsynchronousMethod", @"urn:schemas-microsoft-com:xml-wcfservicemap", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@GenerateTaskBasedAsynchronousMethod)));
            }
            WriteElementStringRaw(@"EnableDataBinding", @"urn:schemas-microsoft-com:xml-wcfservicemap", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@EnableDataBinding)));
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType>)o.@ExcludedTypeList);
                if (a != null){
                    WriteStartElement(@"ExcludedTypes", @"urn:schemas-microsoft-com:xml-wcfservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write2_ReferencedType(@"ExcludedType", @"urn:schemas-microsoft-com:xml-wcfservicemap", ((global::System.Web.Compilation.WCFModel.ReferencedType)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            WriteElementStringRaw(@"ImportXmlTypes", @"urn:schemas-microsoft-com:xml-wcfservicemap", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@ImportXmlTypes)));
            WriteElementStringRaw(@"GenerateInternalTypes", @"urn:schemas-microsoft-com:xml-wcfservicemap", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@GenerateInternalTypes)));
            WriteElementStringRaw(@"GenerateMessageContracts", @"urn:schemas-microsoft-com:xml-wcfservicemap", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@GenerateMessageContracts)));
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.NamespaceMapping> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.NamespaceMapping>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.NamespaceMapping>)o.@NamespaceMappingList);
                if (a != null){
                    WriteStartElement(@"NamespaceMappings", @"urn:schemas-microsoft-com:xml-wcfservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write3_NamespaceMapping(@"NamespaceMapping", @"urn:schemas-microsoft-com:xml-wcfservicemap", ((global::System.Web.Compilation.WCFModel.NamespaceMapping)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedCollectionType> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedCollectionType>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedCollectionType>)o.@CollectionMappingList);
                if (a != null){
                    WriteStartElement(@"CollectionMappings", @"urn:schemas-microsoft-com:xml-wcfservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write5_ReferencedCollectionType(@"CollectionMapping", @"urn:schemas-microsoft-com:xml-wcfservicemap", ((global::System.Web.Compilation.WCFModel.ReferencedCollectionType)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            WriteElementStringRaw(@"GenerateSerializableTypes", @"urn:schemas-microsoft-com:xml-wcfservicemap", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@GenerateSerializableTypes)));
            WriteElementString(@"Serializer", @"urn:schemas-microsoft-com:xml-wcfservicemap", Write6_ProxySerializerType(((global::System.Web.Compilation.WCFModel.ClientOptions.ProxySerializerType)o.@Serializer)));
            if (o.@UseSerializerForFaultsSpecified) {
                WriteElementStringRaw(@"UseSerializerForFaults", @"urn:schemas-microsoft-com:xml-wcfservicemap", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@UseSerializerForFaults)));
            }
            if (o.@WrappedSpecified) {
                WriteElementStringRaw(@"Wrapped", @"urn:schemas-microsoft-com:xml-wcfservicemap", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Wrapped)));
            }
            WriteElementStringRaw(@"ReferenceAllAssemblies", @"urn:schemas-microsoft-com:xml-wcfservicemap", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@ReferenceAllAssemblies)));
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedAssembly> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedAssembly>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedAssembly>)o.@ReferencedAssemblyList);
                if (a != null){
                    WriteStartElement(@"ReferencedAssemblies", @"urn:schemas-microsoft-com:xml-wcfservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write7_ReferencedAssembly(@"ReferencedAssembly", @"urn:schemas-microsoft-com:xml-wcfservicemap", ((global::System.Web.Compilation.WCFModel.ReferencedAssembly)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType>)o.@ReferencedDataContractTypeList);
                if (a != null){
                    WriteStartElement(@"ReferencedDataContractTypes", @"urn:schemas-microsoft-com:xml-wcfservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write2_ReferencedType(@"ReferencedDataContractType", @"urn:schemas-microsoft-com:xml-wcfservicemap", ((global::System.Web.Compilation.WCFModel.ReferencedType)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ContractMapping> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ContractMapping>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ContractMapping>)o.@ServiceContractMappingList);
                if (a != null){
                    WriteStartElement(@"ServiceContractMappings", @"urn:schemas-microsoft-com:xml-wcfservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write8_ContractMapping(@"ServiceContractMapping", @"urn:schemas-microsoft-com:xml-wcfservicemap", ((global::System.Web.Compilation.WCFModel.ContractMapping)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            WriteEndElement(o);
        }

        void Write8_ContractMapping(string n, string ns, global::System.Web.Compilation.WCFModel.ContractMapping o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Compilation.WCFModel.ContractMapping)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"ContractMapping", @"urn:schemas-microsoft-com:xml-wcfservicemap");
            WriteAttribute(@"Name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"TargetNamespace", @"", ((global::System.String)o.@TargetNamespace));
            WriteAttribute(@"TypeName", @"", ((global::System.String)o.@TypeName));
            WriteEndElement(o);
        }

        void Write2_ReferencedType(string n, string ns, global::System.Web.Compilation.WCFModel.ReferencedType o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Compilation.WCFModel.ReferencedType)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"ReferencedType", @"urn:schemas-microsoft-com:xml-wcfservicemap");
            WriteAttribute(@"TypeName", @"", ((global::System.String)o.@TypeName));
            WriteEndElement(o);
        }

        void Write7_ReferencedAssembly(string n, string ns, global::System.Web.Compilation.WCFModel.ReferencedAssembly o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Compilation.WCFModel.ReferencedAssembly)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"ReferencedAssembly", @"urn:schemas-microsoft-com:xml-wcfservicemap");
            WriteAttribute(@"AssemblyName", @"", ((global::System.String)o.@AssemblyName));
            WriteEndElement(o);
        }

        string Write6_ProxySerializerType(global::System.Web.Compilation.WCFModel.ClientOptions.ProxySerializerType v) {
            string s = null;
            switch (v) {
                case global::System.Web.Compilation.WCFModel.ClientOptions.ProxySerializerType.@Auto: s = @"Auto"; break;
                case global::System.Web.Compilation.WCFModel.ClientOptions.ProxySerializerType.@DataContractSerializer: s = @"DataContractSerializer"; break;
                case global::System.Web.Compilation.WCFModel.ClientOptions.ProxySerializerType.@XmlSerializer: s = @"XmlSerializer"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"System.Web.Compilation.WCFModel.ClientOptions.ProxySerializerType");
            }
            return s;
        }

        void Write5_ReferencedCollectionType(string n, string ns, global::System.Web.Compilation.WCFModel.ReferencedCollectionType o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Compilation.WCFModel.ReferencedCollectionType)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"ReferencedCollectionType", @"urn:schemas-microsoft-com:xml-wcfservicemap");
            WriteAttribute(@"TypeName", @"", ((global::System.String)o.@TypeName));
            WriteAttribute(@"Category", @"", Write4_CollectionCategory(((global::System.Web.Compilation.WCFModel.ReferencedCollectionType.CollectionCategory)o.@Category)));
            WriteEndElement(o);
        }

        string Write4_CollectionCategory(global::System.Web.Compilation.WCFModel.ReferencedCollectionType.CollectionCategory v) {
            string s = null;
            switch (v) {
                case global::System.Web.Compilation.WCFModel.ReferencedCollectionType.CollectionCategory.@Unknown: s = @"Unknown"; break;
                case global::System.Web.Compilation.WCFModel.ReferencedCollectionType.CollectionCategory.@List: s = @"List"; break;
                case global::System.Web.Compilation.WCFModel.ReferencedCollectionType.CollectionCategory.@Dictionary: s = @"Dictionary"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"System.Web.Compilation.WCFModel.ReferencedCollectionType.CollectionCategory");
            }
            return s;
        }

        void Write3_NamespaceMapping(string n, string ns, global::System.Web.Compilation.WCFModel.NamespaceMapping o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Compilation.WCFModel.NamespaceMapping)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"NamespaceMapping", @"urn:schemas-microsoft-com:xml-wcfservicemap");
            WriteAttribute(@"TargetNamespace", @"", ((global::System.String)o.@TargetNamespace));
            WriteAttribute(@"ClrNamespace", @"", ((global::System.String)o.@ClrNamespace));
            WriteEndElement(o);
        }

        protected override void InitCallbacks() {
        }
    }

    internal class XmlSerializationReaderSvcMapFileImpl : System.Xml.Serialization.XmlSerializationReader {

        public object Read16_ReferenceGroup() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id1_ReferenceGroup && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o = Read15_SvcMapFileImpl(true, true);
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:ReferenceGroup");
            }
            return (object)o;
        }

        global::System.Web.Compilation.WCFModel.SvcMapFileImpl Read15_SvcMapFileImpl(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id3_SvcMapFileImpl && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.SvcMapFileImpl o;
            o = new global::System.Web.Compilation.WCFModel.SvcMapFileImpl();
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource> a_1 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource>)o.@MetadataSourceList;
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile> a_2 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile>)o.@MetadataList;
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile> a_3 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile>)o.@Extensions;
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[4] && ((object) Reader.LocalName == (object)id4_ID && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@ID = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":ID");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            int state = 0;
            Reader.MoveToContent();
            int whileIterations0 = 0;
            int readerCount0 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    switch (state) {
                    case 0:
                        if (((object) Reader.LocalName == (object)id6_ClientOptions && (object) Reader.NamespaceURI == (object)id2_Item)) {
                            o.@ClientOptions = Read9_ClientOptions(false, true);
                        }
                        state = 1;
                        break;
                    case 1:
                        if (((object) Reader.LocalName == (object)id7_MetadataSources && (object) Reader.NamespaceURI == (object)id2_Item)) {
                            if (!ReadNull()) {
                                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource> a_1_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource>)o.@MetadataSourceList;
                                if (((object)(a_1_0) == null) || (Reader.IsEmptyElement)) {
                                    Reader.Skip();
                                }
                                else {
                                    Reader.ReadStartElement();
                                    Reader.MoveToContent();
                                    int whileIterations1 = 0;
                                    int readerCount1 = ReaderCount;
                                    while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                        if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                            if (((object) Reader.LocalName == (object)id8_MetadataSource && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                                if ((object)(a_1_0) == null) Reader.Skip(); else a_1_0.Add(Read10_MetadataSource(true, true));
                                            }
                                            else {
                                                UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:MetadataSource");
                                            }
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:MetadataSource");
                                        }
                                        Reader.MoveToContent();
                                        CheckReaderCount(ref whileIterations1, ref readerCount1);
                                    }
                                ReadEndElement();
                                }
                            }
                        }
                        else {
                            state = 2;
                        }
                        break;
                    case 2:
                        if (((object) Reader.LocalName == (object)id9_Metadata && (object) Reader.NamespaceURI == (object)id2_Item)) {
                            if (!ReadNull()) {
                                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile> a_2_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile>)o.@MetadataList;
                                if (((object)(a_2_0) == null) || (Reader.IsEmptyElement)) {
                                    Reader.Skip();
                                }
                                else {
                                    Reader.ReadStartElement();
                                    Reader.MoveToContent();
                                    int whileIterations2 = 0;
                                    int readerCount2 = ReaderCount;
                                    while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                        if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                            if (((object) Reader.LocalName == (object)id10_MetadataFile && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                                if ((object)(a_2_0) == null) Reader.Skip(); else a_2_0.Add(Read13_MetadataFile(true, true));
                                            }
                                            else {
                                                UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:MetadataFile");
                                            }
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:MetadataFile");
                                        }
                                        Reader.MoveToContent();
                                        CheckReaderCount(ref whileIterations2, ref readerCount2);
                                    }
                                ReadEndElement();
                                }
                            }
                        }
                        else {
                            state = 3;
                        }
                        break;
                    case 3:
                        if (((object) Reader.LocalName == (object)id11_Extensions && (object) Reader.NamespaceURI == (object)id2_Item)) {
                            if (!ReadNull()) {
                                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile> a_3_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile>)o.@Extensions;
                                if (((object)(a_3_0) == null) || (Reader.IsEmptyElement)) {
                                    Reader.Skip();
                                }
                                else {
                                    Reader.ReadStartElement();
                                    Reader.MoveToContent();
                                    int whileIterations3 = 0;
                                    int readerCount3 = ReaderCount;
                                    while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                        if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                            if (((object) Reader.LocalName == (object)id12_ExtensionFile && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                                if ((object)(a_3_0) == null) Reader.Skip(); else a_3_0.Add(Read14_ExtensionFile(true, true));
                                            }
                                            else {
                                                UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:ExtensionFile");
                                            }
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:ExtensionFile");
                                        }
                                        Reader.MoveToContent();
                                        CheckReaderCount(ref whileIterations3, ref readerCount3);
                                    }
                                ReadEndElement();
                                }
                            }
                        }
                        else {
                            state = 4;
                        }
                        break;
                    default:
                        UnknownNode((object)o, null);
                        break;
                    }
                }
                else {
                    UnknownNode((object)o, null);
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations0, ref readerCount0);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Compilation.WCFModel.ExtensionFile Read14_ExtensionFile(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id12_ExtensionFile && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.ExtensionFile o;
            o = new global::System.Web.Compilation.WCFModel.ExtensionFile();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id13_FileName && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@FileName = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id14_Name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":FileName, :Name");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations4 = 0;
            int readerCount4 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations4, ref readerCount4);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Compilation.WCFModel.MetadataFile Read13_MetadataFile(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id10_MetadataFile && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.MetadataFile o;
            o = new global::System.Web.Compilation.WCFModel.MetadataFile();
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id13_FileName && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@FileName = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id15_MetadataType && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@FileType = Read12_MetadataType(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id4_ID && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@ID = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id16_Ignore && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Ignore = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    o.@IgnoreSpecified = true;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id17_IsMergeResult && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsMergeResult = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    o.@IsMergeResultSpecified = true;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id18_SourceId && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SourceId = System.Xml.XmlConvert.ToInt32(Reader.Value);
                    o.@SourceIdSpecified = true;
                    paramsRead[5] = true;
                }
                else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id19_SourceUrl && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SourceUrl = Reader.Value;
                    paramsRead[6] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":FileName, :MetadataType, :ID, :Ignore, :IsMergeResult, :SourceId, :SourceUrl");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations5 = 0;
            int readerCount5 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations5, ref readerCount5);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType Read12_MetadataType(string s) {
            switch (s) {
                case @"Unknown": return global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Unknown;
                case @"Disco": return global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Disco;
                case @"Wsdl": return global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Wsdl;
                case @"Schema": return global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Schema;
                case @"Policy": return global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Policy;
                case @"Xml": return global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Xml;
                case @"Edmx": return global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType.@Edmx;
                default: throw CreateUnknownConstantException(s, typeof(global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType));
            }
        }

        global::System.Web.Compilation.WCFModel.MetadataSource Read10_MetadataSource(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id8_MetadataSource && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.MetadataSource o;
            o = new global::System.Web.Compilation.WCFModel.MetadataSource();
            bool[] paramsRead = new bool[3];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id20_Address && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Address = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id21_Protocol && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Protocol = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id18_SourceId && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SourceId = System.Xml.XmlConvert.ToInt32(Reader.Value);
                    paramsRead[2] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":Address, :Protocol, :SourceId");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations6 = 0;
            int readerCount6 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations6, ref readerCount6);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Compilation.WCFModel.ClientOptions Read9_ClientOptions(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id6_ClientOptions && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.ClientOptions o;
            o = new global::System.Web.Compilation.WCFModel.ClientOptions();
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType> a_3 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType>)o.@ExcludedTypeList;
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.NamespaceMapping> a_7 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.NamespaceMapping>)o.@NamespaceMappingList;
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedCollectionType> a_8 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedCollectionType>)o.@CollectionMappingList;
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedAssembly> a_14 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedAssembly>)o.@ReferencedAssemblyList;
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType> a_15 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType>)o.@ReferencedDataContractTypeList;
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ContractMapping> a_16 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ContractMapping>)o.@ServiceContractMappingList;
            bool[] paramsRead = new bool[17];
            while (Reader.MoveToNextAttribute()) {
                if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations7 = 0;
            int readerCount7 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_GenerateAsynchronousMethods && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        {
                            o.@GenerateAsynchronousMethods = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                        }
                        paramsRead[0] = true;
                    }
                    else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id23_Item && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        {
                            o.@GenerateTaskBasedAsynchronousMethod = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                        }
                        paramsRead[1] = true;
                    }
                    else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id24_EnableDataBinding && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        {
                            o.@EnableDataBinding = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                        }
                        paramsRead[2] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id25_ExcludedTypes && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if (!ReadNull()) {
                            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType> a_3_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType>)o.@ExcludedTypeList;
                            if (((object)(a_3_0) == null) || (Reader.IsEmptyElement)) {
                                Reader.Skip();
                            }
                            else {
                                Reader.ReadStartElement();
                                Reader.MoveToContent();
                                int whileIterations8 = 0;
                                int readerCount8 = ReaderCount;
                                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                    if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                        if (((object) Reader.LocalName == (object)id26_ExcludedType && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                            if ((object)(a_3_0) == null) Reader.Skip(); else a_3_0.Add(Read2_ReferencedType(true, true));
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:ExcludedType");
                                        }
                                    }
                                    else {
                                        UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:ExcludedType");
                                    }
                                    Reader.MoveToContent();
                                    CheckReaderCount(ref whileIterations8, ref readerCount8);
                                }
                            ReadEndElement();
                            }
                        }
                    }
                    else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id27_ImportXmlTypes && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        {
                            o.@ImportXmlTypes = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                        }
                        paramsRead[4] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id28_GenerateInternalTypes && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        {
                            o.@GenerateInternalTypes = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                        }
                        paramsRead[5] = true;
                    }
                    else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id29_GenerateMessageContracts && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        {
                            o.@GenerateMessageContracts = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                        }
                        paramsRead[6] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id30_NamespaceMappings && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if (!ReadNull()) {
                            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.NamespaceMapping> a_7_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.NamespaceMapping>)o.@NamespaceMappingList;
                            if (((object)(a_7_0) == null) || (Reader.IsEmptyElement)) {
                                Reader.Skip();
                            }
                            else {
                                Reader.ReadStartElement();
                                Reader.MoveToContent();
                                int whileIterations9 = 0;
                                int readerCount9 = ReaderCount;
                                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                    if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                        if (((object) Reader.LocalName == (object)id31_NamespaceMapping && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                            if ((object)(a_7_0) == null) Reader.Skip(); else a_7_0.Add(Read3_NamespaceMapping(true, true));
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:NamespaceMapping");
                                        }
                                    }
                                    else {
                                        UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:NamespaceMapping");
                                    }
                                    Reader.MoveToContent();
                                    CheckReaderCount(ref whileIterations9, ref readerCount9);
                                }
                            ReadEndElement();
                            }
                        }
                    }
                    else if (((object) Reader.LocalName == (object)id32_CollectionMappings && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if (!ReadNull()) {
                            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedCollectionType> a_8_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedCollectionType>)o.@CollectionMappingList;
                            if (((object)(a_8_0) == null) || (Reader.IsEmptyElement)) {
                                Reader.Skip();
                            }
                            else {
                                Reader.ReadStartElement();
                                Reader.MoveToContent();
                                int whileIterations10 = 0;
                                int readerCount10 = ReaderCount;
                                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                    if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                        if (((object) Reader.LocalName == (object)id33_CollectionMapping && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                            if ((object)(a_8_0) == null) Reader.Skip(); else a_8_0.Add(Read5_ReferencedCollectionType(true, true));
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:CollectionMapping");
                                        }
                                    }
                                    else {
                                        UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:CollectionMapping");
                                    }
                                    Reader.MoveToContent();
                                    CheckReaderCount(ref whileIterations10, ref readerCount10);
                                }
                            ReadEndElement();
                            }
                        }
                    }
                    else if (!paramsRead[9] && ((object) Reader.LocalName == (object)id34_GenerateSerializableTypes && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        {
                            o.@GenerateSerializableTypes = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                        }
                        paramsRead[9] = true;
                    }
                    else if (!paramsRead[10] && ((object) Reader.LocalName == (object)id35_Serializer && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        {
                            o.@Serializer = Read6_ProxySerializerType(Reader.ReadElementString());
                        }
                        paramsRead[10] = true;
                    }
                    else if (!paramsRead[11] && ((object) Reader.LocalName == (object)id36_UseSerializerForFaults && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        {
                            o.@UseSerializerForFaults = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                        }
                        paramsRead[11] = true;
                    }
                    else if (!paramsRead[12] && ((object) Reader.LocalName == (object)id37_Wrapped && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        {
                            o.@Wrapped = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                        }
                        paramsRead[12] = true;
                    }
                    else if (!paramsRead[13] && ((object) Reader.LocalName == (object)id38_ReferenceAllAssemblies && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        {
                            o.@ReferenceAllAssemblies = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                        }
                        paramsRead[13] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id39_ReferencedAssemblies && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if (!ReadNull()) {
                            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedAssembly> a_14_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedAssembly>)o.@ReferencedAssemblyList;
                            if (((object)(a_14_0) == null) || (Reader.IsEmptyElement)) {
                                Reader.Skip();
                            }
                            else {
                                Reader.ReadStartElement();
                                Reader.MoveToContent();
                                int whileIterations11 = 0;
                                int readerCount11 = ReaderCount;
                                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                    if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                        if (((object) Reader.LocalName == (object)id40_ReferencedAssembly && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                            if ((object)(a_14_0) == null) Reader.Skip(); else a_14_0.Add(Read7_ReferencedAssembly(true, true));
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:ReferencedAssembly");
                                        }
                                    }
                                    else {
                                        UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:ReferencedAssembly");
                                    }
                                    Reader.MoveToContent();
                                    CheckReaderCount(ref whileIterations11, ref readerCount11);
                                }
                            ReadEndElement();
                            }
                        }
                    }
                    else if (((object) Reader.LocalName == (object)id41_ReferencedDataContractTypes && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if (!ReadNull()) {
                            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType> a_15_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ReferencedType>)o.@ReferencedDataContractTypeList;
                            if (((object)(a_15_0) == null) || (Reader.IsEmptyElement)) {
                                Reader.Skip();
                            }
                            else {
                                Reader.ReadStartElement();
                                Reader.MoveToContent();
                                int whileIterations12 = 0;
                                int readerCount12 = ReaderCount;
                                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                    if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                        if (((object) Reader.LocalName == (object)id42_ReferencedDataContractType && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                            if ((object)(a_15_0) == null) Reader.Skip(); else a_15_0.Add(Read2_ReferencedType(true, true));
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:ReferencedDataContractType");
                                        }
                                    }
                                    else {
                                        UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:ReferencedDataContractType");
                                    }
                                    Reader.MoveToContent();
                                    CheckReaderCount(ref whileIterations12, ref readerCount12);
                                }
                            ReadEndElement();
                            }
                        }
                    }
                    else if (((object) Reader.LocalName == (object)id43_ServiceContractMappings && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if (!ReadNull()) {
                            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ContractMapping> a_16_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ContractMapping>)o.@ServiceContractMappingList;
                            if (((object)(a_16_0) == null) || (Reader.IsEmptyElement)) {
                                Reader.Skip();
                            }
                            else {
                                Reader.ReadStartElement();
                                Reader.MoveToContent();
                                int whileIterations13 = 0;
                                int readerCount13 = ReaderCount;
                                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                    if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                        if (((object) Reader.LocalName == (object)id44_ServiceContractMapping && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                            if ((object)(a_16_0) == null) Reader.Skip(); else a_16_0.Add(Read8_ContractMapping(true, true));
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:ServiceContractMapping");
                                        }
                                    }
                                    else {
                                        UnknownNode(null, @"urn:schemas-microsoft-com:xml-wcfservicemap:ServiceContractMapping");
                                    }
                                    Reader.MoveToContent();
                                    CheckReaderCount(ref whileIterations13, ref readerCount13);
                                }
                            ReadEndElement();
                            }
                        }
                    }
                    else {
                        UnknownNode((object)o, @"urn:schemas-microsoft-com:xml-wcfservicemap:GenerateAsynchronousMethods, urn:schemas-microsoft-com:xml-wcfservicemap:GenerateTaskBasedAsynchronousMethod, urn:schemas-microsoft-com:xml-wcfservicemap:EnableDataBinding, urn:schemas-microsoft-com:xml-wcfservicemap:ExcludedTypes, urn:schemas-microsoft-com:xml-wcfservicemap:ImportXmlTypes, urn:schemas-microsoft-com:xml-wcfservicemap:GenerateInternalTypes, urn:schemas-microsoft-com:xml-wcfservicemap:GenerateMessageContracts, urn:schemas-microsoft-com:xml-wcfservicemap:NamespaceMappings, urn:schemas-microsoft-com:xml-wcfservicemap:CollectionMappings, urn:schemas-microsoft-com:xml-wcfservicemap:GenerateSerializableTypes, urn:schemas-microsoft-com:xml-wcfservicemap:Serializer, urn:schemas-microsoft-com:xml-wcfservicemap:UseSerializerForFaults, urn:schemas-microsoft-com:xml-wcfservicemap:Wrapped, urn:schemas-microsoft-com:xml-wcfservicemap:ReferenceAllAssemblies, urn:schemas-microsoft-com:xml-wcfservicemap:ReferencedAssemblies, urn:schemas-microsoft-com:xml-wcfservicemap:ReferencedDataContractTypes, urn:schemas-microsoft-com:xml-wcfservicemap:ServiceContractMappings");
                    }
                }
                else {
                    UnknownNode((object)o, @"urn:schemas-microsoft-com:xml-wcfservicemap:GenerateAsynchronousMethods, urn:schemas-microsoft-com:xml-wcfservicemap:GenerateTaskBasedAsynchronousMethod, urn:schemas-microsoft-com:xml-wcfservicemap:EnableDataBinding, urn:schemas-microsoft-com:xml-wcfservicemap:ExcludedTypes, urn:schemas-microsoft-com:xml-wcfservicemap:ImportXmlTypes, urn:schemas-microsoft-com:xml-wcfservicemap:GenerateInternalTypes, urn:schemas-microsoft-com:xml-wcfservicemap:GenerateMessageContracts, urn:schemas-microsoft-com:xml-wcfservicemap:NamespaceMappings, urn:schemas-microsoft-com:xml-wcfservicemap:CollectionMappings, urn:schemas-microsoft-com:xml-wcfservicemap:GenerateSerializableTypes, urn:schemas-microsoft-com:xml-wcfservicemap:Serializer, urn:schemas-microsoft-com:xml-wcfservicemap:UseSerializerForFaults, urn:schemas-microsoft-com:xml-wcfservicemap:Wrapped, urn:schemas-microsoft-com:xml-wcfservicemap:ReferenceAllAssemblies, urn:schemas-microsoft-com:xml-wcfservicemap:ReferencedAssemblies, urn:schemas-microsoft-com:xml-wcfservicemap:ReferencedDataContractTypes, urn:schemas-microsoft-com:xml-wcfservicemap:ServiceContractMappings");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations7, ref readerCount7);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Compilation.WCFModel.ContractMapping Read8_ContractMapping(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id45_ContractMapping && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.ContractMapping o;
            o = new global::System.Web.Compilation.WCFModel.ContractMapping();
            bool[] paramsRead = new bool[3];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id14_Name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id46_TargetNamespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@TargetNamespace = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id47_TypeName && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@TypeName = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":Name, :TargetNamespace, :TypeName");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations14 = 0;
            int readerCount14 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations14, ref readerCount14);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Compilation.WCFModel.ReferencedType Read2_ReferencedType(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id48_ReferencedType && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.ReferencedType o;
            o = new global::System.Web.Compilation.WCFModel.ReferencedType();
            bool[] paramsRead = new bool[1];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id47_TypeName && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@TypeName = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":TypeName");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations15 = 0;
            int readerCount15 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations15, ref readerCount15);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Compilation.WCFModel.ReferencedAssembly Read7_ReferencedAssembly(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id40_ReferencedAssembly && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.ReferencedAssembly o;
            o = new global::System.Web.Compilation.WCFModel.ReferencedAssembly();
            bool[] paramsRead = new bool[1];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id49_AssemblyName && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@AssemblyName = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":AssemblyName");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations16 = 0;
            int readerCount16 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations16, ref readerCount16);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Compilation.WCFModel.ClientOptions.ProxySerializerType Read6_ProxySerializerType(string s) {
            switch (s) {
                case @"Auto": return global::System.Web.Compilation.WCFModel.ClientOptions.ProxySerializerType.@Auto;
                case @"DataContractSerializer": return global::System.Web.Compilation.WCFModel.ClientOptions.ProxySerializerType.@DataContractSerializer;
                case @"XmlSerializer": return global::System.Web.Compilation.WCFModel.ClientOptions.ProxySerializerType.@XmlSerializer;
                default: throw CreateUnknownConstantException(s, typeof(global::System.Web.Compilation.WCFModel.ClientOptions.ProxySerializerType));
            }
        }

        global::System.Web.Compilation.WCFModel.ReferencedCollectionType Read5_ReferencedCollectionType(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id50_ReferencedCollectionType && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.ReferencedCollectionType o;
            o = new global::System.Web.Compilation.WCFModel.ReferencedCollectionType();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id47_TypeName && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@TypeName = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id51_Category && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Category = Read4_CollectionCategory(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":TypeName, :Category");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations17 = 0;
            int readerCount17 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations17, ref readerCount17);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Compilation.WCFModel.ReferencedCollectionType.CollectionCategory Read4_CollectionCategory(string s) {
            switch (s) {
                case @"Unknown": return global::System.Web.Compilation.WCFModel.ReferencedCollectionType.CollectionCategory.@Unknown;
                case @"List": return global::System.Web.Compilation.WCFModel.ReferencedCollectionType.CollectionCategory.@List;
                case @"Dictionary": return global::System.Web.Compilation.WCFModel.ReferencedCollectionType.CollectionCategory.@Dictionary;
                default: throw CreateUnknownConstantException(s, typeof(global::System.Web.Compilation.WCFModel.ReferencedCollectionType.CollectionCategory));
            }
        }

        global::System.Web.Compilation.WCFModel.NamespaceMapping Read3_NamespaceMapping(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id31_NamespaceMapping && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.NamespaceMapping o;
            o = new global::System.Web.Compilation.WCFModel.NamespaceMapping();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id46_TargetNamespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@TargetNamespace = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id52_ClrNamespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@ClrNamespace = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":TargetNamespace, :ClrNamespace");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations18 = 0;
            int readerCount18 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations18, ref readerCount18);
            }
            ReadEndElement();
            return o;
        }

        protected override void InitCallbacks() {
        }

        string id47_TypeName;
        string id1_ReferenceGroup;
        string id49_AssemblyName;
        string id38_ReferenceAllAssemblies;
        string id46_TargetNamespace;
        string id29_GenerateMessageContracts;
        string id28_GenerateInternalTypes;
        string id13_FileName;
        string id3_SvcMapFileImpl;
        string id7_MetadataSources;
        string id50_ReferencedCollectionType;
        string id8_MetadataSource;
        string id25_ExcludedTypes;
        string id10_MetadataFile;
        string id45_ContractMapping;
        string id15_MetadataType;
        string id34_GenerateSerializableTypes;
        string id31_NamespaceMapping;
        string id42_ReferencedDataContractType;
        string id16_Ignore;
        string id36_UseSerializerForFaults;
        string id52_ClrNamespace;
        string id4_ID;
        string id17_IsMergeResult;
        string id40_ReferencedAssembly;
        string id48_ReferencedType;
        string id22_GenerateAsynchronousMethods;
        string id2_Item;
        string id12_ExtensionFile;
        string id32_CollectionMappings;
        string id23_Item;
        string id39_ReferencedAssemblies;
        string id35_Serializer;
        string id21_Protocol;
        string id44_ServiceContractMapping;
        string id14_Name;
        string id19_SourceUrl;
        string id51_Category;
        string id5_Item;
        string id30_NamespaceMappings;
        string id9_Metadata;
        string id24_EnableDataBinding;
        string id27_ImportXmlTypes;
        string id18_SourceId;
        string id20_Address;
        string id11_Extensions;
        string id33_CollectionMapping;
        string id26_ExcludedType;
        string id43_ServiceContractMappings;
        string id37_Wrapped;
        string id41_ReferencedDataContractTypes;
        string id6_ClientOptions;

        protected override void InitIDs() {
            id47_TypeName = Reader.NameTable.Add(@"TypeName");
            id1_ReferenceGroup = Reader.NameTable.Add(@"ReferenceGroup");
            id49_AssemblyName = Reader.NameTable.Add(@"AssemblyName");
            id38_ReferenceAllAssemblies = Reader.NameTable.Add(@"ReferenceAllAssemblies");
            id46_TargetNamespace = Reader.NameTable.Add(@"TargetNamespace");
            id29_GenerateMessageContracts = Reader.NameTable.Add(@"GenerateMessageContracts");
            id28_GenerateInternalTypes = Reader.NameTable.Add(@"GenerateInternalTypes");
            id13_FileName = Reader.NameTable.Add(@"FileName");
            id3_SvcMapFileImpl = Reader.NameTable.Add(@"SvcMapFileImpl");
            id7_MetadataSources = Reader.NameTable.Add(@"MetadataSources");
            id50_ReferencedCollectionType = Reader.NameTable.Add(@"ReferencedCollectionType");
            id8_MetadataSource = Reader.NameTable.Add(@"MetadataSource");
            id25_ExcludedTypes = Reader.NameTable.Add(@"ExcludedTypes");
            id10_MetadataFile = Reader.NameTable.Add(@"MetadataFile");
            id45_ContractMapping = Reader.NameTable.Add(@"ContractMapping");
            id15_MetadataType = Reader.NameTable.Add(@"MetadataType");
            id34_GenerateSerializableTypes = Reader.NameTable.Add(@"GenerateSerializableTypes");
            id31_NamespaceMapping = Reader.NameTable.Add(@"NamespaceMapping");
            id42_ReferencedDataContractType = Reader.NameTable.Add(@"ReferencedDataContractType");
            id16_Ignore = Reader.NameTable.Add(@"Ignore");
            id36_UseSerializerForFaults = Reader.NameTable.Add(@"UseSerializerForFaults");
            id52_ClrNamespace = Reader.NameTable.Add(@"ClrNamespace");
            id4_ID = Reader.NameTable.Add(@"ID");
            id17_IsMergeResult = Reader.NameTable.Add(@"IsMergeResult");
            id40_ReferencedAssembly = Reader.NameTable.Add(@"ReferencedAssembly");
            id48_ReferencedType = Reader.NameTable.Add(@"ReferencedType");
            id22_GenerateAsynchronousMethods = Reader.NameTable.Add(@"GenerateAsynchronousMethods");
            id2_Item = Reader.NameTable.Add(@"urn:schemas-microsoft-com:xml-wcfservicemap");
            id12_ExtensionFile = Reader.NameTable.Add(@"ExtensionFile");
            id32_CollectionMappings = Reader.NameTable.Add(@"CollectionMappings");
            id23_Item = Reader.NameTable.Add(@"GenerateTaskBasedAsynchronousMethod");
            id39_ReferencedAssemblies = Reader.NameTable.Add(@"ReferencedAssemblies");
            id35_Serializer = Reader.NameTable.Add(@"Serializer");
            id21_Protocol = Reader.NameTable.Add(@"Protocol");
            id44_ServiceContractMapping = Reader.NameTable.Add(@"ServiceContractMapping");
            id14_Name = Reader.NameTable.Add(@"Name");
            id19_SourceUrl = Reader.NameTable.Add(@"SourceUrl");
            id51_Category = Reader.NameTable.Add(@"Category");
            id5_Item = Reader.NameTable.Add(@"");
            id30_NamespaceMappings = Reader.NameTable.Add(@"NamespaceMappings");
            id9_Metadata = Reader.NameTable.Add(@"Metadata");
            id24_EnableDataBinding = Reader.NameTable.Add(@"EnableDataBinding");
            id27_ImportXmlTypes = Reader.NameTable.Add(@"ImportXmlTypes");
            id18_SourceId = Reader.NameTable.Add(@"SourceId");
            id20_Address = Reader.NameTable.Add(@"Address");
            id11_Extensions = Reader.NameTable.Add(@"Extensions");
            id33_CollectionMapping = Reader.NameTable.Add(@"CollectionMapping");
            id26_ExcludedType = Reader.NameTable.Add(@"ExcludedType");
            id43_ServiceContractMappings = Reader.NameTable.Add(@"ServiceContractMappings");
            id37_Wrapped = Reader.NameTable.Add(@"Wrapped");
            id41_ReferencedDataContractTypes = Reader.NameTable.Add(@"ReferencedDataContractTypes");
            id6_ClientOptions = Reader.NameTable.Add(@"ClientOptions");
        }
    }

    internal abstract class XmlSerializer1 : System.Xml.Serialization.XmlSerializer {
        protected override System.Xml.Serialization.XmlSerializationReader CreateReader() {
            return new XmlSerializationReaderSvcMapFileImpl();
        }
        protected override System.Xml.Serialization.XmlSerializationWriter CreateWriter() {
            return new XmlSerializationWriterSvcMapFileImpl();
        }
    }

    internal sealed class SvcMapFileImplSerializer : XmlSerializer1 {

        public override System.Boolean CanDeserialize(System.Xml.XmlReader xmlReader) {
            return xmlReader.IsStartElement(@"ReferenceGroup", @"urn:schemas-microsoft-com:xml-wcfservicemap");
        }

        protected override void Serialize(object objectToSerialize, System.Xml.Serialization.XmlSerializationWriter writer) {
            ((XmlSerializationWriterSvcMapFileImpl)writer).Write16_ReferenceGroup(objectToSerialize);
        }

        protected override object Deserialize(System.Xml.Serialization.XmlSerializationReader reader) {
            return ((XmlSerializationReaderSvcMapFileImpl)reader).Read16_ReferenceGroup();
        }
    }

    internal class XmlSerializerContract : global::System.Xml.Serialization.XmlSerializerImplementation {
        public override global::System.Xml.Serialization.XmlSerializationReader Reader { get { return new XmlSerializationReaderSvcMapFileImpl(); } }
        public override global::System.Xml.Serialization.XmlSerializationWriter Writer { get { return new XmlSerializationWriterSvcMapFileImpl(); } }
        System.Collections.Hashtable readMethods = null;
        public override System.Collections.Hashtable ReadMethods {
            get {
                if (readMethods == null) {
                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                    _tmp[@"System.Web.Compilation.WCFModel.SvcMapFileImpl:urn:schemas-microsoft-com:xml-wcfservicemap:ReferenceGroup:True:"] = @"Read16_ReferenceGroup";
                    if (readMethods == null) readMethods = _tmp;
                }
                return readMethods;
            }
        }
        System.Collections.Hashtable writeMethods = null;
        public override System.Collections.Hashtable WriteMethods {
            get {
                if (writeMethods == null) {
                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                    _tmp[@"System.Web.Compilation.WCFModel.SvcMapFileImpl:urn:schemas-microsoft-com:xml-wcfservicemap:ReferenceGroup:True:"] = @"Write16_ReferenceGroup";
                    if (writeMethods == null) writeMethods = _tmp;
                }
                return writeMethods;
            }
        }
        System.Collections.Hashtable typedSerializers = null;
        public override System.Collections.Hashtable TypedSerializers {
            get {
                if (typedSerializers == null) {
                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                    _tmp.Add(@"System.Web.Compilation.WCFModel.SvcMapFileImpl:urn:schemas-microsoft-com:xml-wcfservicemap:ReferenceGroup:True:", new SvcMapFileImplSerializer());
                    if (typedSerializers == null) typedSerializers = _tmp;
                }
                return typedSerializers;
            }
        }
        public override System.Boolean CanSerialize(System.Type type) {
            if (type == typeof(global::System.Web.Compilation.WCFModel.SvcMapFileImpl)) return true;
            return false;
        }
        public override System.Xml.Serialization.XmlSerializer GetSerializer(System.Type type) {
            if (type == typeof(global::System.Web.Compilation.WCFModel.SvcMapFileImpl)) return new SvcMapFileImplSerializer();
            return null;
        }
    }
}
