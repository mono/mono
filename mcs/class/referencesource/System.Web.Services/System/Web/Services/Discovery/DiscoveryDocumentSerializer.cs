namespace System.Web.Services.Discovery {
internal class DiscoveryDocumentSerializationWriter : System.Xml.Serialization.XmlSerializationWriter {
        

        public void Write10_discovery(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteNullTagLiteral(@"discovery", @"http://schemas.xmlsoap.org/disco/");
                return;
            }
            TopLevelElement();
            Write9_DiscoveryDocument(@"discovery", @"http://schemas.xmlsoap.org/disco/", ((global::System.Web.Services.Discovery.DiscoveryDocument)o), true, false);
        }

        void Write9_DiscoveryDocument(string n, string ns, global::System.Web.Services.Discovery.DiscoveryDocument o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Discovery.DiscoveryDocument)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"DiscoveryDocument", @"http://schemas.xmlsoap.org/disco/");
            {
                global::System.Collections.IList a = (global::System.Collections.IList)o.@References;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Object ai = (global::System.Object)a[ia];
                        {
                            if (ai is global::System.Web.Services.Discovery.SchemaReference) {
                                Write7_SchemaReference(@"schemaRef", @"http://schemas.xmlsoap.org/disco/schema/", ((global::System.Web.Services.Discovery.SchemaReference)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Discovery.ContractReference) {
                                Write5_ContractReference(@"contractRef", @"http://schemas.xmlsoap.org/disco/scl/", ((global::System.Web.Services.Discovery.ContractReference)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Discovery.DiscoveryDocumentReference) {
                                Write3_DiscoveryDocumentReference(@"discoveryRef", @"http://schemas.xmlsoap.org/disco/", ((global::System.Web.Services.Discovery.DiscoveryDocumentReference)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Discovery.SoapBinding) {
                                Write8_SoapBinding(@"soap", @"http://schemas.xmlsoap.org/disco/soap/", ((global::System.Web.Services.Discovery.SoapBinding)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write8_SoapBinding(string n, string ns, global::System.Web.Services.Discovery.SoapBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Discovery.SoapBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"SoapBinding", @"http://schemas.xmlsoap.org/disco/soap/");
            WriteAttribute(@"address", @"", ((global::System.String)o.@Address));
            WriteAttribute(@"binding", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Binding)));
            WriteEndElement(o);
        }

        void Write3_DiscoveryDocumentReference(string n, string ns, global::System.Web.Services.Discovery.DiscoveryDocumentReference o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Discovery.DiscoveryDocumentReference)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"DiscoveryDocumentReference", @"http://schemas.xmlsoap.org/disco/");
            WriteAttribute(@"ref", @"", ((global::System.String)o.@Ref));
            WriteEndElement(o);
        }

        void Write5_ContractReference(string n, string ns, global::System.Web.Services.Discovery.ContractReference o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Discovery.ContractReference)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"ContractReference", @"http://schemas.xmlsoap.org/disco/scl/");
            WriteAttribute(@"ref", @"", ((global::System.String)o.@Ref));
            WriteAttribute(@"docRef", @"", ((global::System.String)o.@DocRef));
            WriteEndElement(o);
        }

        void Write7_SchemaReference(string n, string ns, global::System.Web.Services.Discovery.SchemaReference o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Discovery.SchemaReference)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"SchemaReference", @"http://schemas.xmlsoap.org/disco/schema/");
            WriteAttribute(@"ref", @"", ((global::System.String)o.@Ref));
            WriteAttribute(@"targetNamespace", @"", ((global::System.String)o.@TargetNamespace));
            WriteEndElement(o);
        }

        protected override void InitCallbacks() {
        }
    }
    internal class DiscoveryDocumentSerializationReader : System.Xml.Serialization.XmlSerializationReader {
        

        public object Read10_discovery() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id1_discovery && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o = Read9_DiscoveryDocument(true, true);
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null, @"http://schemas.xmlsoap.org/disco/:discovery");
            }
            return (object)o;
        }

        global::System.Web.Services.Discovery.DiscoveryDocument Read9_DiscoveryDocument(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id3_DiscoveryDocument && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Discovery.DiscoveryDocument o;
            o = new global::System.Web.Services.Discovery.DiscoveryDocument();
            global::System.Collections.IList a_0 = (global::System.Collections.IList)o.@References;
            bool[] paramsRead = new bool[1];
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
            int whileIterations0 = 0;
            int readerCount0 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (((object) Reader.LocalName == (object)id4_discoveryRef && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_0) == null) Reader.Skip(); else a_0.Add(Read3_DiscoveryDocumentReference(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id5_contractRef && (object) Reader.NamespaceURI == (object)id6_Item)) {
                        if ((object)(a_0) == null) Reader.Skip(); else a_0.Add(Read5_ContractReference(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id7_schemaRef && (object) Reader.NamespaceURI == (object)id8_Item)) {
                        if ((object)(a_0) == null) Reader.Skip(); else a_0.Add(Read7_SchemaReference(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id9_soap && (object) Reader.NamespaceURI == (object)id10_Item)) {
                        if ((object)(a_0) == null) Reader.Skip(); else a_0.Add(Read8_SoapBinding(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://schemas.xmlsoap.org/disco/:discoveryRef, http://schemas.xmlsoap.org/disco/scl/:contractRef, http://schemas.xmlsoap.org/disco/schema/:schemaRef, http://schemas.xmlsoap.org/disco/soap/:soap");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/disco/:discoveryRef, http://schemas.xmlsoap.org/disco/scl/:contractRef, http://schemas.xmlsoap.org/disco/schema/:schemaRef, http://schemas.xmlsoap.org/disco/soap/:soap");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations0, ref readerCount0);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Discovery.SoapBinding Read8_SoapBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id11_SoapBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id10_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Discovery.SoapBinding o;
            o = new global::System.Web.Services.Discovery.SoapBinding();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id12_address && (object) Reader.NamespaceURI == (object)id13_Item)) {
                    o.@Address = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id14_binding && (object) Reader.NamespaceURI == (object)id13_Item)) {
                    o.@Binding = ToXmlQualifiedName(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":address, :binding");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations1 = 0;
            int readerCount1 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations1, ref readerCount1);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Discovery.SchemaReference Read7_SchemaReference(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id15_SchemaReference && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id8_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Discovery.SchemaReference o;
            o = new global::System.Web.Services.Discovery.SchemaReference();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id16_ref && (object) Reader.NamespaceURI == (object)id13_Item)) {
                    o.@Ref = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id17_targetNamespace && (object) Reader.NamespaceURI == (object)id13_Item)) {
                    o.@TargetNamespace = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":ref, :targetNamespace");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations2 = 0;
            int readerCount2 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations2, ref readerCount2);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Discovery.ContractReference Read5_ContractReference(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id18_ContractReference && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id6_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Discovery.ContractReference o;
            o = new global::System.Web.Services.Discovery.ContractReference();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id16_ref && (object) Reader.NamespaceURI == (object)id13_Item)) {
                    o.@Ref = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id19_docRef && (object) Reader.NamespaceURI == (object)id13_Item)) {
                    o.@DocRef = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":ref, :docRef");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations3 = 0;
            int readerCount3 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations3, ref readerCount3);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Discovery.DiscoveryDocumentReference Read3_DiscoveryDocumentReference(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id20_DiscoveryDocumentReference && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Discovery.DiscoveryDocumentReference o;
            o = new global::System.Web.Services.Discovery.DiscoveryDocumentReference();
            bool[] paramsRead = new bool[1];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id16_ref && (object) Reader.NamespaceURI == (object)id13_Item)) {
                    o.@Ref = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":ref");
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

        protected override void InitCallbacks() {
        }

        string id1_discovery;
        string id4_discoveryRef;
        string id19_docRef;
        string id8_Item;
        string id14_binding;
        string id20_DiscoveryDocumentReference;
        string id17_targetNamespace;
        string id5_contractRef;
        string id10_Item;
        string id13_Item;
        string id7_schemaRef;
        string id3_DiscoveryDocument;
        string id9_soap;
        string id12_address;
        string id16_ref;
        string id11_SoapBinding;
        string id18_ContractReference;
        string id2_Item;
        string id15_SchemaReference;
        string id6_Item;

        protected override void InitIDs() {
            id1_discovery = Reader.NameTable.Add(@"discovery");
            id4_discoveryRef = Reader.NameTable.Add(@"discoveryRef");
            id19_docRef = Reader.NameTable.Add(@"docRef");
            id8_Item = Reader.NameTable.Add(@"http://schemas.xmlsoap.org/disco/schema/");
            id14_binding = Reader.NameTable.Add(@"binding");
            id20_DiscoveryDocumentReference = Reader.NameTable.Add(@"DiscoveryDocumentReference");
            id17_targetNamespace = Reader.NameTable.Add(@"targetNamespace");
            id5_contractRef = Reader.NameTable.Add(@"contractRef");
            id10_Item = Reader.NameTable.Add(@"http://schemas.xmlsoap.org/disco/soap/");
            id13_Item = Reader.NameTable.Add(@"");
            id7_schemaRef = Reader.NameTable.Add(@"schemaRef");
            id3_DiscoveryDocument = Reader.NameTable.Add(@"DiscoveryDocument");
            id9_soap = Reader.NameTable.Add(@"soap");
            id12_address = Reader.NameTable.Add(@"address");
            id16_ref = Reader.NameTable.Add(@"ref");
            id11_SoapBinding = Reader.NameTable.Add(@"SoapBinding");
            id18_ContractReference = Reader.NameTable.Add(@"ContractReference");
            id2_Item = Reader.NameTable.Add(@"http://schemas.xmlsoap.org/disco/");
            id15_SchemaReference = Reader.NameTable.Add(@"SchemaReference");
            id6_Item = Reader.NameTable.Add(@"http://schemas.xmlsoap.org/disco/scl/");
        }
    }
}
