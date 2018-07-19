//-----------------------------------------------------------------------------
// <copyright file="SvcFileMapSerializer.cs" company="Microsoft">
//   Copyright (C) Microsoft Corporation. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------------

//  This file is generated. DO NOT MODIFY IT BY HAND.
// Please read HowToUpdateSerializer.txt in the parent directory to see how to update it.


namespace System.Web.Compilation.WCFModel.DataSvcMapFileXmlSerializer {

    internal class XmlSerializationWriterDataSvcMapFileImpl : System.Xml.Serialization.XmlSerializationWriter {

        public void Write9_ReferenceGroup(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteNullTagLiteral(@"ReferenceGroup", @"urn:schemas-microsoft-com:xml-dataservicemap");
                return;
            }
            TopLevelElement();
            Write8_DataSvcMapFileImpl(@"ReferenceGroup", @"urn:schemas-microsoft-com:xml-dataservicemap", ((global::System.Web.Compilation.WCFModel.DataSvcMapFileImpl)o), true, false);
        }

        void Write8_DataSvcMapFileImpl(string n, string ns, global::System.Web.Compilation.WCFModel.DataSvcMapFileImpl o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Compilation.WCFModel.DataSvcMapFileImpl)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"DataSvcMapFileImpl", @"urn:schemas-microsoft-com:xml-dataservicemap");
            WriteAttribute(@"ID", @"", ((global::System.String)o.@ID));
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource>)o.@MetadataSourceList);
                if (a != null){
                    WriteStartElement(@"MetadataSources", @"urn:schemas-microsoft-com:xml-dataservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write2_MetadataSource(@"MetadataSource", @"urn:schemas-microsoft-com:xml-dataservicemap", ((global::System.Web.Compilation.WCFModel.MetadataSource)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile>)o.@MetadataList);
                if (a != null){
                    WriteStartElement(@"Metadata", @"urn:schemas-microsoft-com:xml-dataservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write5_MetadataFile(@"MetadataFile", @"urn:schemas-microsoft-com:xml-dataservicemap", ((global::System.Web.Compilation.WCFModel.MetadataFile)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile>)o.@Extensions);
                if (a != null){
                    WriteStartElement(@"Extensions", @"urn:schemas-microsoft-com:xml-dataservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write6_ExtensionFile(@"ExtensionFile", @"urn:schemas-microsoft-com:xml-dataservicemap", ((global::System.Web.Compilation.WCFModel.ExtensionFile)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            {
                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.Parameter> a = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.Parameter>)((global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.Parameter>)o.@Parameters);
                if (a != null){
                    WriteStartElement(@"Parameters", @"urn:schemas-microsoft-com:xml-dataservicemap", null, false);
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write7_Parameter(@"Parameter", @"urn:schemas-microsoft-com:xml-dataservicemap", ((global::System.Web.Compilation.WCFModel.Parameter)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            WriteEndElement(o);
        }

        void Write7_Parameter(string n, string ns, global::System.Web.Compilation.WCFModel.Parameter o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Compilation.WCFModel.Parameter)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"Parameter", @"urn:schemas-microsoft-com:xml-dataservicemap");
            WriteAttribute(@"Name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"Value", @"", ((global::System.String)o.@Value));
            WriteEndElement(o);
        }

        void Write6_ExtensionFile(string n, string ns, global::System.Web.Compilation.WCFModel.ExtensionFile o, bool isNullable, bool needType) {
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
            if (needType) WriteXsiType(@"ExtensionFile", @"urn:schemas-microsoft-com:xml-dataservicemap");
            WriteAttribute(@"FileName", @"", ((global::System.String)o.@FileName));
            WriteAttribute(@"Name", @"", ((global::System.String)o.@Name));
            WriteEndElement(o);
        }

        void Write5_MetadataFile(string n, string ns, global::System.Web.Compilation.WCFModel.MetadataFile o, bool isNullable, bool needType) {
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
            if (needType) WriteXsiType(@"MetadataFile", @"urn:schemas-microsoft-com:xml-dataservicemap");
            WriteAttribute(@"FileName", @"", ((global::System.String)o.@FileName));
            WriteAttribute(@"MetadataType", @"", Write4_MetadataType(((global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType)o.@FileType)));
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

        string Write4_MetadataType(global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType v) {
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

        void Write2_MetadataSource(string n, string ns, global::System.Web.Compilation.WCFModel.MetadataSource o, bool isNullable, bool needType) {
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
            if (needType) WriteXsiType(@"MetadataSource", @"urn:schemas-microsoft-com:xml-dataservicemap");
            WriteAttribute(@"Address", @"", ((global::System.String)o.@Address));
            WriteAttribute(@"Protocol", @"", ((global::System.String)o.@Protocol));
            WriteAttribute(@"SourceId", @"", System.Xml.XmlConvert.ToString((global::System.Int32)((global::System.Int32)o.@SourceId)));
            WriteEndElement(o);
        }

        protected override void InitCallbacks() {
        }
    }

    internal class XmlSerializationReaderDataSvcMapFileImpl : System.Xml.Serialization.XmlSerializationReader {

        public object Read9_ReferenceGroup() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id1_ReferenceGroup && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o = Read8_DataSvcMapFileImpl(true, true);
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null, @"urn:schemas-microsoft-com:xml-dataservicemap:ReferenceGroup");
            }
            return (object)o;
        }

        global::System.Web.Compilation.WCFModel.DataSvcMapFileImpl Read8_DataSvcMapFileImpl(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id3_DataSvcMapFileImpl && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.DataSvcMapFileImpl o;
            o = new global::System.Web.Compilation.WCFModel.DataSvcMapFileImpl();
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource> a_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource>)o.@MetadataSourceList;
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile> a_1 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile>)o.@MetadataList;
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile> a_2 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile>)o.@Extensions;
            global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.Parameter> a_3 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.Parameter>)o.@Parameters;
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
                        if (((object) Reader.LocalName == (object)id6_MetadataSources && (object) Reader.NamespaceURI == (object)id2_Item)) {
                            if (!ReadNull()) {
                                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource> a_0_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataSource>)o.@MetadataSourceList;
                                if (((object)(a_0_0) == null) || (Reader.IsEmptyElement)) {
                                    Reader.Skip();
                                }
                                else {
                                    Reader.ReadStartElement();
                                    Reader.MoveToContent();
                                    int whileIterations1 = 0;
                                    int readerCount1 = ReaderCount;
                                    while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                        if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                            if (((object) Reader.LocalName == (object)id7_MetadataSource && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                                if ((object)(a_0_0) == null) Reader.Skip(); else a_0_0.Add(Read2_MetadataSource(true, true));
                                            }
                                            else {
                                                UnknownNode(null, @"urn:schemas-microsoft-com:xml-dataservicemap:MetadataSource");
                                            }
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-dataservicemap:MetadataSource");
                                        }
                                        Reader.MoveToContent();
                                        CheckReaderCount(ref whileIterations1, ref readerCount1);
                                    }
                                ReadEndElement();
                                }
                            }
                        }
                        else {
                            state = 1;
                        }
                        break;
                    case 1:
                        if (((object) Reader.LocalName == (object)id8_Metadata && (object) Reader.NamespaceURI == (object)id2_Item)) {
                            if (!ReadNull()) {
                                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile> a_1_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.MetadataFile>)o.@MetadataList;
                                if (((object)(a_1_0) == null) || (Reader.IsEmptyElement)) {
                                    Reader.Skip();
                                }
                                else {
                                    Reader.ReadStartElement();
                                    Reader.MoveToContent();
                                    int whileIterations2 = 0;
                                    int readerCount2 = ReaderCount;
                                    while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                        if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                            if (((object) Reader.LocalName == (object)id9_MetadataFile && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                                if ((object)(a_1_0) == null) Reader.Skip(); else a_1_0.Add(Read5_MetadataFile(true, true));
                                            }
                                            else {
                                                UnknownNode(null, @"urn:schemas-microsoft-com:xml-dataservicemap:MetadataFile");
                                            }
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-dataservicemap:MetadataFile");
                                        }
                                        Reader.MoveToContent();
                                        CheckReaderCount(ref whileIterations2, ref readerCount2);
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
                        if (((object) Reader.LocalName == (object)id10_Extensions && (object) Reader.NamespaceURI == (object)id2_Item)) {
                            if (!ReadNull()) {
                                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile> a_2_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.ExtensionFile>)o.@Extensions;
                                if (((object)(a_2_0) == null) || (Reader.IsEmptyElement)) {
                                    Reader.Skip();
                                }
                                else {
                                    Reader.ReadStartElement();
                                    Reader.MoveToContent();
                                    int whileIterations3 = 0;
                                    int readerCount3 = ReaderCount;
                                    while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                        if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                            if (((object) Reader.LocalName == (object)id11_ExtensionFile && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                                if ((object)(a_2_0) == null) Reader.Skip(); else a_2_0.Add(Read6_ExtensionFile(true, true));
                                            }
                                            else {
                                                UnknownNode(null, @"urn:schemas-microsoft-com:xml-dataservicemap:ExtensionFile");
                                            }
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-dataservicemap:ExtensionFile");
                                        }
                                        Reader.MoveToContent();
                                        CheckReaderCount(ref whileIterations3, ref readerCount3);
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
                        if (((object) Reader.LocalName == (object)id12_Parameters && (object) Reader.NamespaceURI == (object)id2_Item)) {
                            if (!ReadNull()) {
                                global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.Parameter> a_3_0 = (global::System.Collections.Generic.List<global::System.Web.Compilation.WCFModel.Parameter>)o.@Parameters;
                                if (((object)(a_3_0) == null) || (Reader.IsEmptyElement)) {
                                    Reader.Skip();
                                }
                                else {
                                    Reader.ReadStartElement();
                                    Reader.MoveToContent();
                                    int whileIterations4 = 0;
                                    int readerCount4 = ReaderCount;
                                    while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                        if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                            if (((object) Reader.LocalName == (object)id13_Parameter && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                                if ((object)(a_3_0) == null) Reader.Skip(); else a_3_0.Add(Read7_Parameter(true, true));
                                            }
                                            else {
                                                UnknownNode(null, @"urn:schemas-microsoft-com:xml-dataservicemap:Parameter");
                                            }
                                        }
                                        else {
                                            UnknownNode(null, @"urn:schemas-microsoft-com:xml-dataservicemap:Parameter");
                                        }
                                        Reader.MoveToContent();
                                        CheckReaderCount(ref whileIterations4, ref readerCount4);
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

        global::System.Web.Compilation.WCFModel.Parameter Read7_Parameter(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id13_Parameter && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.Parameter o;
            o = new global::System.Web.Compilation.WCFModel.Parameter();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id14_Name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id15_Value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":Name, :Value");
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

        global::System.Web.Compilation.WCFModel.ExtensionFile Read6_ExtensionFile(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id11_ExtensionFile && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.ExtensionFile o;
            o = new global::System.Web.Compilation.WCFModel.ExtensionFile();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id16_FileName && (object) Reader.NamespaceURI == (object)id5_Item)) {
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

        global::System.Web.Compilation.WCFModel.MetadataFile Read5_MetadataFile(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id9_MetadataFile && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.MetadataFile o;
            o = new global::System.Web.Compilation.WCFModel.MetadataFile();
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id16_FileName && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@FileName = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id17_MetadataType && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@FileType = Read4_MetadataType(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id4_ID && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@ID = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id18_Ignore && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Ignore = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    o.@IgnoreSpecified = true;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id19_IsMergeResult && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsMergeResult = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    o.@IsMergeResultSpecified = true;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id20_SourceId && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SourceId = System.Xml.XmlConvert.ToInt32(Reader.Value);
                    o.@SourceIdSpecified = true;
                    paramsRead[5] = true;
                }
                else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id21_SourceUrl && (object) Reader.NamespaceURI == (object)id5_Item)) {
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
            int whileIterations7 = 0;
            int readerCount7 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations7, ref readerCount7);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Compilation.WCFModel.MetadataFile.MetadataType Read4_MetadataType(string s) {
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

        global::System.Web.Compilation.WCFModel.MetadataSource Read2_MetadataSource(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id7_MetadataSource && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Compilation.WCFModel.MetadataSource o;
            o = new global::System.Web.Compilation.WCFModel.MetadataSource();
            bool[] paramsRead = new bool[3];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_Address && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Address = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id23_Protocol && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Protocol = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id20_SourceId && (object) Reader.NamespaceURI == (object)id5_Item)) {
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
            int whileIterations8 = 0;
            int readerCount8 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations8, ref readerCount8);
            }
            ReadEndElement();
            return o;
        }

        protected override void InitCallbacks() {
        }

        string id5_Item;
        string id4_ID;
        string id18_Ignore;
        string id21_SourceUrl;
        string id20_SourceId;
        string id14_Name;
        string id2_Item;
        string id11_ExtensionFile;
        string id12_Parameters;
        string id1_ReferenceGroup;
        string id16_FileName;
        string id6_MetadataSources;
        string id17_MetadataType;
        string id13_Parameter;
        string id15_Value;
        string id23_Protocol;
        string id3_DataSvcMapFileImpl;
        string id8_Metadata;
        string id9_MetadataFile;
        string id19_IsMergeResult;
        string id7_MetadataSource;
        string id10_Extensions;
        string id22_Address;

        protected override void InitIDs() {
            id5_Item = Reader.NameTable.Add(@"");
            id4_ID = Reader.NameTable.Add(@"ID");
            id18_Ignore = Reader.NameTable.Add(@"Ignore");
            id21_SourceUrl = Reader.NameTable.Add(@"SourceUrl");
            id20_SourceId = Reader.NameTable.Add(@"SourceId");
            id14_Name = Reader.NameTable.Add(@"Name");
            id2_Item = Reader.NameTable.Add(@"urn:schemas-microsoft-com:xml-dataservicemap");
            id11_ExtensionFile = Reader.NameTable.Add(@"ExtensionFile");
            id12_Parameters = Reader.NameTable.Add(@"Parameters");
            id1_ReferenceGroup = Reader.NameTable.Add(@"ReferenceGroup");
            id16_FileName = Reader.NameTable.Add(@"FileName");
            id6_MetadataSources = Reader.NameTable.Add(@"MetadataSources");
            id17_MetadataType = Reader.NameTable.Add(@"MetadataType");
            id13_Parameter = Reader.NameTable.Add(@"Parameter");
            id15_Value = Reader.NameTable.Add(@"Value");
            id23_Protocol = Reader.NameTable.Add(@"Protocol");
            id3_DataSvcMapFileImpl = Reader.NameTable.Add(@"DataSvcMapFileImpl");
            id8_Metadata = Reader.NameTable.Add(@"Metadata");
            id9_MetadataFile = Reader.NameTable.Add(@"MetadataFile");
            id19_IsMergeResult = Reader.NameTable.Add(@"IsMergeResult");
            id7_MetadataSource = Reader.NameTable.Add(@"MetadataSource");
            id10_Extensions = Reader.NameTable.Add(@"Extensions");
            id22_Address = Reader.NameTable.Add(@"Address");
        }
    }

    internal abstract class XmlSerializer1 : System.Xml.Serialization.XmlSerializer {
        protected override System.Xml.Serialization.XmlSerializationReader CreateReader() {
            return new XmlSerializationReaderDataSvcMapFileImpl();
        }
        protected override System.Xml.Serialization.XmlSerializationWriter CreateWriter() {
            return new XmlSerializationWriterDataSvcMapFileImpl();
        }
    }

    internal sealed class DataSvcMapFileImplSerializer : XmlSerializer1 {

        public override System.Boolean CanDeserialize(System.Xml.XmlReader xmlReader) {
            return xmlReader.IsStartElement(@"ReferenceGroup", @"urn:schemas-microsoft-com:xml-dataservicemap");
        }

        protected override void Serialize(object objectToSerialize, System.Xml.Serialization.XmlSerializationWriter writer) {
            ((XmlSerializationWriterDataSvcMapFileImpl)writer).Write9_ReferenceGroup(objectToSerialize);
        }

        protected override object Deserialize(System.Xml.Serialization.XmlSerializationReader reader) {
            return ((XmlSerializationReaderDataSvcMapFileImpl)reader).Read9_ReferenceGroup();
        }
    }

    internal class XmlSerializerContract : global::System.Xml.Serialization.XmlSerializerImplementation {
        public override global::System.Xml.Serialization.XmlSerializationReader Reader { get { return new XmlSerializationReaderDataSvcMapFileImpl(); } }
        public override global::System.Xml.Serialization.XmlSerializationWriter Writer { get { return new XmlSerializationWriterDataSvcMapFileImpl(); } }
        System.Collections.Hashtable readMethods = null;
        public override System.Collections.Hashtable ReadMethods {
            get {
                if (readMethods == null) {
                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                    _tmp[@"System.Web.Compilation.WCFModel.DataSvcMapFileImpl:urn:schemas-microsoft-com:xml-dataservicemap:ReferenceGroup:True:"] = @"Read9_ReferenceGroup";
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
                    _tmp[@"System.Web.Compilation.WCFModel.DataSvcMapFileImpl:urn:schemas-microsoft-com:xml-dataservicemap:ReferenceGroup:True:"] = @"Write9_ReferenceGroup";
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
                    _tmp.Add(@"System.Web.Compilation.WCFModel.DataSvcMapFileImpl:urn:schemas-microsoft-com:xml-dataservicemap:ReferenceGroup:True:", new DataSvcMapFileImplSerializer());
                    if (typedSerializers == null) typedSerializers = _tmp;
                }
                return typedSerializers;
            }
        }
        public override System.Boolean CanSerialize(System.Type type) {
            if (type == typeof(global::System.Web.Compilation.WCFModel.DataSvcMapFileImpl)) return true;
            return false;
        }
        public override System.Xml.Serialization.XmlSerializer GetSerializer(System.Type type) {
            if (type == typeof(global::System.Web.Compilation.WCFModel.DataSvcMapFileImpl)) return new DataSvcMapFileImplSerializer();
            return null;
        }
    }
}
