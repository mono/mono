//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlRoot(MetadataStrings.MetadataExchangeStrings.Metadata, Namespace = MetadataStrings.MetadataExchangeStrings.Namespace)]
    public class MetadataSet : IXmlSerializable
    {
        Collection<MetadataSection> sections = new Collection<MetadataSection>();
        Collection<XmlAttribute> attributes = new Collection<XmlAttribute>();

        internal ServiceMetadataExtension.WriteFilter WriteFilter;

        public MetadataSet()
        {
        }

        public MetadataSet(IEnumerable<MetadataSection> sections)
            : this()
        {
            if (sections != null)
                foreach (MetadataSection section in sections)
                    this.sections.Add(section);
        }

        [XmlElement(MetadataStrings.MetadataExchangeStrings.MetadataSection, Namespace = MetadataStrings.MetadataExchangeStrings.Namespace)]
        public Collection<MetadataSection> MetadataSections
        {
            get { return this.sections; }
        }

        [XmlAnyAttribute]
        public Collection<XmlAttribute> Attributes
        {
            get { return attributes; }
        }

        //Reader should write the <Metadata> element
        public void WriteTo(XmlWriter writer)
        {
            WriteMetadataSet(writer, true);
        }

        //Reader is on the <Metadata> element
        public static MetadataSet ReadFrom(XmlReader reader)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");

            MetadataSetSerializer xs = new MetadataSetSerializer();
            return (MetadataSet)xs.Deserialize(reader);
        }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        //Reader in on the <Metadata> element
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");

            MetadataSetSerializer xs = new MetadataSetSerializer();
            xs.ProcessOuterElement = false;

            MetadataSet metadataSet = (MetadataSet)xs.Deserialize(reader);

            this.sections = metadataSet.MetadataSections;
            this.attributes = metadataSet.Attributes;
        }

        //Reader has just written the <Metadata> element can still write attribs here
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            WriteMetadataSet(writer, false);
        } 

        void WriteMetadataSet(XmlWriter writer, bool processOuterElement)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");

            if (this.WriteFilter != null)
            {
                ServiceMetadataExtension.WriteFilter filter = this.WriteFilter.CloneWriteFilter();
                filter.Writer = writer;
                writer = filter;
            }
            MetadataSetSerializer xs = new MetadataSetSerializer();
            xs.ProcessOuterElement = processOuterElement;

            xs.Serialize(writer, this);
        }

    }

#pragma warning disable

    /* The Following code is a generated XmlSerializer.  It was created by:
     *      (*) Removing the IXmlSerializable from MetadataSet
     *      (*) Changing typeof(WsdlNS.ServiceDescription) and typeof(XsdNS.XmlSchema) to typeof(string) and typeof(int) on the [XmlElement] attribute on 
     *          MetadataSection.Metadata
     *      (*) running "sgen /a:System.ServiceModel.dll /t:System.ServiceModel.Description.MetadataSet /k" to generate the code 
     *      (*) Revert the above changes.
     * 
     * and then doing the following to fix it up:
     * 
     *      (*) Change the classes from public to internal
     *      (*) Add ProcessOuterElement to MetadataSetSerializer, XmlSerializationReaderMetadataSet, and XmlSerializationWriterMetadataSet
                       private bool processOuterElement = true;
     
                       public bool ProcessOuterElement
                       {
                           get { return processOuterElement; }
                           set { processOuterElement = value; }
                       }
     *      (*) Set XmlSerializationWriterMetadataSet.ProcessOuterElement with MetadataSetSerializer.ProcessOuterElement
     *          in MetadataSetSerializer.Serialize 
     *          ((XmlSerializationWriterMetadataSet)writer).ProcessOuterElement = this.processOuterElement;
     * 
     *      (*) Set XmlSerializationReaderMetadataSet.ProcessOuterElement with MetadataSetSerializer.ProcessOuterElement
     *          in MetadataSetSerializer.Deserialize 
     *          ((XmlSerializationReaderMetadataSet)reader).ProcessOuterElement = this.processOuterElement;
     *      (*) wrap anything in XmlSerializationWriterMetadataSet.Write*_Metadata or 
     *          XmlSerializationWriterMetadataSet.Write*_MetadataSet that outputs the outer
     *          element with "if(processOuterElement) { ... }"
     *      (*) Add "!processOuterElement ||" to checks for name and namespace of the outer element
     *          in XmlSerializationReaderMetadataSet.Read*_Metadata and XmlSerializationReaderMetadataSet.Read*_MetadataSet.
     *      (*) In XmlSerializationReaderMetadataSet.Read*_MetadataSection change the if clause writing the XmlSchema from
     *          
     *          o.@Metadata = Reader.ReadElementString();
     *          to
                o.@Metadata = System.Xml.Schema.XmlSchema.Read(this.Reader, null);
                if (this.Reader.NodeType == XmlNodeType.EndElement)
                    ReadEndElement();
     * 
     * 
     *      (*) In XmlSerializationWriterMetadataSet Write*_MetadataSection change
     *
     *          else if (o.@Metadata is global::System.Int32) {
     *              WriteElementString(@"schema", @"http://www.w3.org/2001/XMLSchema", ((global::System.Int32)o.@Metadata));
     *          }
     *          to
     * 
                else if (o.@Metadata is global::System.Xml.Schema.XmlSchema)
                {
                    ((global::System.Xml.Schema.XmlSchema)o.@Metadata).Write(this.Writer);
                }       
     * 
     *      (*) In XmlSerializationReaderMetadataSet.Read*_MetadataSection change 
     *          
     *          o.@Metadata = Reader.ReadElementString();
     *          to
     *          o.@Metadata = System.Web.Services.Description.ServiceDescription.Read(this.Reader);
     * 
     * 
     *      (*) In XmlSerializationWriterMetadataSet Write*_MetadataSection change
     *
     *          if (o.@Metadata is global::System.String) {
     *              WriteElementString(@"definitions", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.String)o.@Metadata));
     *          }
     *          to
     * 
                if (o.@Metadata is global::System.Web.Services.Description.ServiceDescription) {
                    ((global::System.Web.Services.Description.ServiceDescription)o.@Metadata).Write(this.Writer);
                }         
     * 
     *      (*) In XmlSerializationWriterMetadataSet Write*_MetadataSet add 
     *
                XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
                xmlSerializerNamespaces.Add(MetadataStrings.MetadataExchangeStrings.Prefix, MetadataStrings.MetadataExchangeStrings.Namespace);
                WriteNamespaceDeclarations(xmlSerializerNamespaces);
     *          
     *          immediately before 'if (needType) WriteXsiType(@"MetadataSet", @"http://schemas.xmlsoap.org/ws/2004/09/mex");'
     * 
     *      (*) In XmlSerializationWriterMetadataSet Write*_MetadataSection replace  
     *          WriteStartElement(n, ns, o, false, null);
     *          with
     * 
                XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
                xmlSerializerNamespaces.Add(string.Empty, string.Empty);

                WriteStartElement(n, ns, o, true, xmlSerializerNamespaces);
     *          
     *      (*) In XmlSerializationWriterMetadataSet Write*_XmlSchema replace              
     *          WriteStartElement(n, ns, o, false, o.@Namespaces);
     *          with 
     *          WriteStartElement(n, ns, o, true, o.@Namespaces);
     * 
     *       (*) Make sure you keep the #pragmas surrounding this block.
     * 
     *      (*) Make sure to replace all exception throw with standard throw using DiagnosticUtility.ExceptionUtility.ThrowHelperError;
     *          change:
     *
     *          throw CreateUnknownTypeException(*);
     *          to
     *          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnknownTypeException(*));
     *          
     *          throw CreateUnknownNodeException();
     *          to
     *          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnknownNodeException());
     * 
     *          throw CreateInvalidAnyTypeException(elem);
     *          to
     *          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateInvalidAnyTypeException(elem));
     * 
     *          throw CreateInvalidEnumValueException(*);
     *          to
     *          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateInvalidEnumValueException(*));
     * 
     *          throw CreateUnknownConstantException(*);
     *          to
     *          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnknownConstantException(*));
     *
     */

    internal class XmlSerializationWriterMetadataSet : System.Xml.Serialization.XmlSerializationWriter
    {
        bool processOuterElement = true;
        public bool ProcessOuterElement
        {
            get { return processOuterElement; }
            set { processOuterElement = value; }
        }

        public void Write68_Metadata(object o)
        {
            if (processOuterElement)
            {
                WriteStartDocument();
                if (o == null)
                {
                    WriteNullTagLiteral(@"Metadata", @"http://schemas.xmlsoap.org/ws/2004/09/mex");
                    return;
                }
                TopLevelElement();
            }
            Write67_MetadataSet(@"Metadata", @"http://schemas.xmlsoap.org/ws/2004/09/mex", ((global::System.ServiceModel.Description.MetadataSet)o), true, false);
        }

        void Write67_MetadataSet(string n, string ns, global::System.ServiceModel.Description.MetadataSet o, bool isNullable, bool needType)
        {
            if (processOuterElement)
            {
                if ((object)o == null)
                {
                    if (isNullable) WriteNullTagLiteral(n, ns);
                    return;
                }
            }
            if (!needType)
            {
                System.Type t = o.GetType();
                if (t == typeof(global::System.ServiceModel.Description.MetadataSet))
                {
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnknownTypeException(o));
                }
            }
            if (processOuterElement)
            {
                WriteStartElement(n, ns, o, false, null);
            }

            XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
            xmlSerializerNamespaces.Add(MetadataStrings.MetadataExchangeStrings.Prefix, MetadataStrings.MetadataExchangeStrings.Namespace);
            WriteNamespaceDeclarations(xmlSerializerNamespaces);

            if (needType) WriteXsiType(@"MetadataSet", @"http://schemas.xmlsoap.org/ws/2004/09/mex");
            {
                global::System.Collections.ObjectModel.Collection<global::System.Xml.XmlAttribute> a = (global::System.Collections.ObjectModel.Collection<global::System.Xml.XmlAttribute>)o.@Attributes;
                if (a != null)
                {
                    for (int i = 0; i < ((System.Collections.ICollection)a).Count; i++)
                    {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            {
                global::System.Collections.ObjectModel.Collection<global::System.ServiceModel.Description.MetadataSection> a = (global::System.Collections.ObjectModel.Collection<global::System.ServiceModel.Description.MetadataSection>)o.@MetadataSections;
                if (a != null)
                {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++)
                    {
                        Write66_MetadataSection(@"MetadataSection", @"http://schemas.xmlsoap.org/ws/2004/09/mex", ((global::System.ServiceModel.Description.MetadataSection)a[ia]), false, false);
                    }
                }
            }
            if (processOuterElement)
            {
                WriteEndElement(o);
            }
        }

        void Write66_MetadataSection(string n, string ns, global::System.ServiceModel.Description.MetadataSection o, bool isNullable, bool needType)
        {
            if ((object)o == null)
            {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType)
            {
                System.Type t = o.GetType();
                if (t == typeof(global::System.ServiceModel.Description.MetadataSection))
                {
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnknownTypeException(o));
                }
            }


            XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
            xmlSerializerNamespaces.Add(string.Empty, string.Empty);

            WriteStartElement(n, ns, o, true, xmlSerializerNamespaces);
            if (needType) WriteXsiType(@"MetadataSection", @"http://schemas.xmlsoap.org/ws/2004/09/mex");
            {
                global::System.Collections.ObjectModel.Collection<global::System.Xml.XmlAttribute> a = (global::System.Collections.ObjectModel.Collection<global::System.Xml.XmlAttribute>)o.@Attributes;
                if (a != null)
                {
                    for (int i = 0; i < ((System.Collections.ICollection)a).Count; i++)
                    {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"Dialect", @"", ((global::System.String)o.@Dialect));
            WriteAttribute(@"Identifier", @"", ((global::System.String)o.@Identifier));
            {
                if (o.@Metadata is global::System.Web.Services.Description.ServiceDescription)
                {
                    ((global::System.Web.Services.Description.ServiceDescription)o.@Metadata).Write(this.Writer);
                }
                else if (o.@Metadata is global::System.Xml.Schema.XmlSchema)
                {
                    ((global::System.Xml.Schema.XmlSchema)o.@Metadata).Write(this.Writer);
                }
                else if (o.@Metadata is global::System.ServiceModel.Description.MetadataSet)
                {
                    Write67_MetadataSet(@"Metadata", @"http://schemas.xmlsoap.org/ws/2004/09/mex", ((global::System.ServiceModel.Description.MetadataSet)o.@Metadata), false, false);
                }
                else if (o.@Metadata is global::System.ServiceModel.Description.MetadataLocation)
                {
                    Write65_MetadataLocation(@"Location", @"http://schemas.xmlsoap.org/ws/2004/09/mex", ((global::System.ServiceModel.Description.MetadataLocation)o.@Metadata), false, false);
                }
                else if (o.@Metadata is global::System.ServiceModel.Description.MetadataReference)
                {
                    WriteSerializable((System.Xml.Serialization.IXmlSerializable)((global::System.ServiceModel.Description.MetadataReference)o.@Metadata), @"MetadataReference", @"http://schemas.xmlsoap.org/ws/2004/09/mex", false, true);
                }
                else if (o.@Metadata is System.Xml.XmlElement)
                {
                    System.Xml.XmlElement elem = (System.Xml.XmlElement)o.@Metadata;
                    if ((elem) is System.Xml.XmlNode || elem == null)
                    {
                        WriteElementLiteral((System.Xml.XmlNode)elem, @"", null, false, true);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateInvalidAnyTypeException(elem));
                    }
                }
                else
                {
                    if (o.@Metadata != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnknownTypeException(o.@Metadata));
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write65_MetadataLocation(string n, string ns, global::System.ServiceModel.Description.MetadataLocation o, bool isNullable, bool needType)
        {
            if ((object)o == null)
            {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType)
            {
                System.Type t = o.GetType();
                if (t == typeof(global::System.ServiceModel.Description.MetadataLocation))
                {
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnknownTypeException(o));
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"MetadataLocation", @"http://schemas.xmlsoap.org/ws/2004/09/mex");
            {
                WriteValue(((global::System.String)o.@Location));
            }
            WriteEndElement(o);
        }

        protected override void InitCallbacks()
        {
        }
    }

    internal class XmlSerializationReaderMetadataSet : System.Xml.Serialization.XmlSerializationReader
    {
        bool processOuterElement = true;
        public bool ProcessOuterElement
        {
            get { return processOuterElement; }
            set { processOuterElement = value; }
        }

        public object Read68_Metadata()
        {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element)
            {
                if (!processOuterElement || (((object)Reader.LocalName == (object)id1_Metadata && (object)Reader.NamespaceURI == (object)id2_Item)))
                {
                    o = Read67_MetadataSet(true, true);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnknownNodeException());
                }
            }
            else
            {
                UnknownNode(null, @"http://schemas.xmlsoap.org/ws/2004/09/mex:Metadata");
            }
            return (object)o;
        }

        global::System.ServiceModel.Description.MetadataSet Read67_MetadataSet(bool isNullable, bool checkType)
        {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType)
            {
                if (!processOuterElement || (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)id3_MetadataSet && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)))
                {
                }
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType));
            }
            if (isNull) return null;
            global::System.ServiceModel.Description.MetadataSet o;
            o = new global::System.ServiceModel.Description.MetadataSet();
            global::System.Collections.ObjectModel.Collection<global::System.ServiceModel.Description.MetadataSection> a_0 = (global::System.Collections.ObjectModel.Collection<global::System.ServiceModel.Description.MetadataSection>)o.@MetadataSections;
            global::System.Collections.ObjectModel.Collection<global::System.Xml.XmlAttribute> a_1 = (global::System.Collections.ObjectModel.Collection<global::System.Xml.XmlAttribute>)o.@Attributes;
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute())
            {
                if (!IsXmlnsAttribute(Reader.Name))
                {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute)Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1.Add(attr);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
            {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations0 = 0;
            int readerCount0 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
            {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (((object)Reader.LocalName == (object)id4_MetadataSection && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        if ((object)(a_0) == null) Reader.Skip(); else a_0.Add(Read66_MetadataSection(false, true));
                    }
                    else
                    {
                        UnknownNode((object)o, @"http://schemas.xmlsoap.org/ws/2004/09/mex:MetadataSection");
                    }
                }
                else
                {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/ws/2004/09/mex:MetadataSection");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations0, ref readerCount0);
            }
            ReadEndElement();
            return o;
        }

        global::System.ServiceModel.Description.MetadataSection Read66_MetadataSection(bool isNullable, bool checkType)
        {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) 
            {
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)id4_MetadataSection && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item))
                {
                }
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType));
            }
            if (isNull) return null;
            global::System.ServiceModel.Description.MetadataSection o;
            o = new global::System.ServiceModel.Description.MetadataSection();
            global::System.Collections.ObjectModel.Collection<global::System.Xml.XmlAttribute> a_0 = (global::System.Collections.ObjectModel.Collection<global::System.Xml.XmlAttribute>)o.@Attributes;
            bool[] paramsRead = new bool[4];
            while (Reader.MoveToNextAttribute())
            {
                if (!paramsRead[1] && ((object)Reader.LocalName == (object)id5_Dialect && (object)Reader.NamespaceURI == (object)id6_Item))
                {
                    o.@Dialect = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object)Reader.LocalName == (object)id7_Identifier && (object)Reader.NamespaceURI == (object)id6_Item))
                {
                    o.@Identifier = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name))
                {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute)Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_0.Add(attr);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
            {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations1 = 0;
            int readerCount1 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
            {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (!paramsRead[3] && ((object)Reader.LocalName == (object)id1_Metadata && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        o.@Metadata = Read67_MetadataSet(false, true);
                        paramsRead[3] = true;
                    }
                    else if (!paramsRead[3] && ((object)Reader.LocalName == (object)id8_schema && (object)Reader.NamespaceURI == (object)id9_Item))
                    {
                        o.@Metadata = System.Xml.Schema.XmlSchema.Read(this.Reader, null);
                        if (this.Reader.NodeType == XmlNodeType.EndElement)
                            ReadEndElement();
                        paramsRead[3] = true;
                    }
                    else if (!paramsRead[3] && ((object)Reader.LocalName == (object)id10_definitions && (object)Reader.NamespaceURI == (object)id11_Item))
                    {
                        {
                            o.@Metadata = System.Web.Services.Description.ServiceDescription.Read(this.Reader);
                        }
                        paramsRead[3] = true;
                    }
                    else if (!paramsRead[3] && ((object)Reader.LocalName == (object)id12_MetadataReference && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        o.@Metadata = (global::System.ServiceModel.Description.MetadataReference)ReadSerializable((System.Xml.Serialization.IXmlSerializable)System.Activator.CreateInstance(typeof(global::System.ServiceModel.Description.MetadataReference), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.CreateInstance | System.Reflection.BindingFlags.NonPublic, null, new object[0], null));
                        paramsRead[3] = true;
                    }
                    else if (!paramsRead[3] && ((object)Reader.LocalName == (object)id13_Location && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        o.@Metadata = Read65_MetadataLocation(false, true);
                        paramsRead[3] = true;
                    }
                    else
                    {
                        o.@Metadata = (global::System.Xml.XmlElement)ReadXmlNode(false);
                    }
                }
                else
                {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/ws/2004/09/mex:Metadata, http://www.w3.org/2001/XMLSchema:schema, http://schemas.xmlsoap.org/wsdl/:definitions, http://schemas.xmlsoap.org/ws/2004/09/mex:MetadataReference, http://schemas.xmlsoap.org/ws/2004/09/mex:Location");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations1, ref readerCount1);
            }
            ReadEndElement();
            return o;
        }

        global::System.ServiceModel.Description.MetadataLocation Read65_MetadataLocation(bool isNullable, bool checkType)
        {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType)
            {
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)id14_MetadataLocation && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item))
                {
                }
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType));
            }
            if (isNull) return null;
            global::System.ServiceModel.Description.MetadataLocation o;
            o = new global::System.ServiceModel.Description.MetadataLocation();
            bool[] paramsRead = new bool[1];
            while (Reader.MoveToNextAttribute())
            {
                if (!IsXmlnsAttribute(Reader.Name))
                {
                    UnknownNode((object)o);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
            {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations2 = 0;
            int readerCount2 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
            {
                string tmp = null;
                if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    UnknownNode((object)o, @"");
                }
                else if (Reader.NodeType == System.Xml.XmlNodeType.Text ||
                Reader.NodeType == System.Xml.XmlNodeType.CDATA ||
                Reader.NodeType == System.Xml.XmlNodeType.Whitespace ||
                Reader.NodeType == System.Xml.XmlNodeType.SignificantWhitespace)
                {
                    tmp = ReadString(tmp, false);
                    o.@Location = tmp;
                }
                else
                {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations2, ref readerCount2);
            }
            ReadEndElement();
            return o;
        }

        protected override void InitCallbacks()
        {
        }

        string id60_documentation;
        string id22_targetNamespace;
        string id10_definitions;
        string id65_lang;
        string id31_attribute;
        string id47_ref;
        string id4_MetadataSection;
        string id54_refer;
        string id83_union;
        string id127_Item;
        string id53_XmlSchemaKeyref;
        string id27_import;
        string id75_all;
        string id128_XmlSchemaSimpleContent;
        string id139_XmlSchemaInclude;
        string id78_namespace;
        string id18_attributeFormDefault;
        string id100_XmlSchemaFractionDigitsFacet;
        string id32_attributeGroup;
        string id64_XmlSchemaDocumentation;
        string id93_maxLength;
        string id49_type;
        string id86_XmlSchemaSimpleTypeRestriction;
        string id96_length;
        string id104_XmlSchemaLengthFacet;
        string id17_XmlSchema;
        string id134_public;
        string id77_XmlSchemaAnyAttribute;
        string id24_id;
        string id71_simpleContent;
        string id51_key;
        string id67_XmlSchemaKey;
        string id80_XmlSchemaAttribute;
        string id126_Item;
        string id23_version;
        string id121_XmlSchemaGroupRef;
        string id90_maxInclusive;
        string id116_memberTypes;
        string id20_finalDefault;
        string id120_any;
        string id112_XmlSchemaMaxExclusiveFacet;
        string id15_EndpointReference;
        string id45_name;
        string id122_XmlSchemaSequence;
        string id73_sequence;
        string id82_XmlSchemaSimpleType;
        string id48_substitutionGroup;
        string id111_XmlSchemaMinInclusiveFacet;
        string id7_Identifier;
        string id113_XmlSchemaSimpleTypeList;
        string id41_default;
        string id125_extension;
        string id16_Item;
        string id1000_Item;
        string id124_XmlSchemaComplexContent;
        string id72_complexContent;
        string id11_Item;
        string id25_include;
        string id34_simpleType;
        string id91_minExclusive;
        string id94_pattern;
        string id2_Item;
        string id95_enumeration;
        string id114_itemType;
        string id115_XmlSchemaSimpleTypeUnion;
        string id59_XmlSchemaAnnotation;
        string id28_notation;
        string id84_list;
        string id39_abstract;
        string id103_XmlSchemaWhiteSpaceFacet;
        string id110_XmlSchemaMaxInclusiveFacet;
        string id55_selector;
        string id43_fixed;
        string id57_XmlSchemaXPath;
        string id118_XmlSchemaAll;
        string id56_field;
        string id119_XmlSchemaChoice;
        string id123_XmlSchemaAny;
        string id132_XmlSchemaGroup;
        string id35_element;
        string id129_Item;
        string id30_annotation;
        string id44_form;
        string id21_elementFormDefault;
        string id98_totalDigits;
        string id88_maxExclusive;
        string id42_final;
        string id46_nillable;
        string id9_Item;
        string id61_appinfo;
        string id38_maxOccurs;
        string id70_mixed;
        string id87_base;
        string id13_Location;
        string id12_MetadataReference;
        string id97_whiteSpace;
        string id29_group;
        string id92_minLength;
        string id99_fractionDigits;
        string id137_schemaLocation;
        string id26_redefine;
        string id101_value;
        string id63_source;
        string id89_minInclusive;
        string id133_XmlSchemaNotation;
        string id52_keyref;
        string id33_complexType;
        string id135_system;
        string id50_unique;
        string id74_choice;
        string id66_Item;
        string id105_XmlSchemaEnumerationFacet;
        string id107_XmlSchemaMaxLengthFacet;
        string id36_XmlSchemaElement;
        string id106_XmlSchemaPatternFacet;
        string id37_minOccurs;
        string id130_Item;
        string id68_XmlSchemaUnique;
        string id131_XmlSchemaAttributeGroup;
        string id40_block;
        string id81_use;
        string id85_restriction;
        string id1_Metadata;
        string id69_XmlSchemaComplexType;
        string id117_XmlSchemaAttributeGroupRef;
        string id138_XmlSchemaRedefine;
        string id6_Item;
        string id102_XmlSchemaTotalDigitsFacet;
        string id58_xpath;
        string id5_Dialect;
        string id14_MetadataLocation;
        string id3_MetadataSet;
        string id79_processContents;
        string id76_anyAttribute;
        string id19_blockDefault;
        string id136_XmlSchemaImport;
        string id109_XmlSchemaMinExclusiveFacet;
        string id108_XmlSchemaMinLengthFacet;
        string id8_schema;
        string id62_XmlSchemaAppInfo;

        protected override void InitIDs()
        {
            id60_documentation = Reader.NameTable.Add(@"documentation");
            id22_targetNamespace = Reader.NameTable.Add(@"targetNamespace");
            id10_definitions = Reader.NameTable.Add(@"definitions");
            id65_lang = Reader.NameTable.Add(@"lang");
            id31_attribute = Reader.NameTable.Add(@"attribute");
            id47_ref = Reader.NameTable.Add(@"ref");
            id4_MetadataSection = Reader.NameTable.Add(@"MetadataSection");
            id54_refer = Reader.NameTable.Add(@"refer");
            id83_union = Reader.NameTable.Add(@"union");
            id127_Item = Reader.NameTable.Add(@"XmlSchemaComplexContentRestriction");
            id53_XmlSchemaKeyref = Reader.NameTable.Add(@"XmlSchemaKeyref");
            id27_import = Reader.NameTable.Add(@"import");
            id75_all = Reader.NameTable.Add(@"all");
            id128_XmlSchemaSimpleContent = Reader.NameTable.Add(@"XmlSchemaSimpleContent");
            id139_XmlSchemaInclude = Reader.NameTable.Add(@"XmlSchemaInclude");
            id78_namespace = Reader.NameTable.Add(@"namespace");
            id18_attributeFormDefault = Reader.NameTable.Add(@"attributeFormDefault");
            id100_XmlSchemaFractionDigitsFacet = Reader.NameTable.Add(@"XmlSchemaFractionDigitsFacet");
            id32_attributeGroup = Reader.NameTable.Add(@"attributeGroup");
            id64_XmlSchemaDocumentation = Reader.NameTable.Add(@"XmlSchemaDocumentation");
            id93_maxLength = Reader.NameTable.Add(@"maxLength");
            id49_type = Reader.NameTable.Add(@"type");
            id86_XmlSchemaSimpleTypeRestriction = Reader.NameTable.Add(@"XmlSchemaSimpleTypeRestriction");
            id96_length = Reader.NameTable.Add(@"length");
            id104_XmlSchemaLengthFacet = Reader.NameTable.Add(@"XmlSchemaLengthFacet");
            id17_XmlSchema = Reader.NameTable.Add(@"XmlSchema");
            id134_public = Reader.NameTable.Add(@"public");
            id77_XmlSchemaAnyAttribute = Reader.NameTable.Add(@"XmlSchemaAnyAttribute");
            id24_id = Reader.NameTable.Add(@"id");
            id71_simpleContent = Reader.NameTable.Add(@"simpleContent");
            id51_key = Reader.NameTable.Add(@"key");
            id67_XmlSchemaKey = Reader.NameTable.Add(@"XmlSchemaKey");
            id80_XmlSchemaAttribute = Reader.NameTable.Add(@"XmlSchemaAttribute");
            id126_Item = Reader.NameTable.Add(@"XmlSchemaComplexContentExtension");
            id23_version = Reader.NameTable.Add(@"version");
            id121_XmlSchemaGroupRef = Reader.NameTable.Add(@"XmlSchemaGroupRef");
            id90_maxInclusive = Reader.NameTable.Add(@"maxInclusive");
            id116_memberTypes = Reader.NameTable.Add(@"memberTypes");
            id20_finalDefault = Reader.NameTable.Add(@"finalDefault");
            id120_any = Reader.NameTable.Add(@"any");
            id112_XmlSchemaMaxExclusiveFacet = Reader.NameTable.Add(@"XmlSchemaMaxExclusiveFacet");
            id15_EndpointReference = Reader.NameTable.Add(@"EndpointReference");
            id45_name = Reader.NameTable.Add(@"name");
            id122_XmlSchemaSequence = Reader.NameTable.Add(@"XmlSchemaSequence");
            id73_sequence = Reader.NameTable.Add(@"sequence");
            id82_XmlSchemaSimpleType = Reader.NameTable.Add(@"XmlSchemaSimpleType");
            id48_substitutionGroup = Reader.NameTable.Add(@"substitutionGroup");
            id111_XmlSchemaMinInclusiveFacet = Reader.NameTable.Add(@"XmlSchemaMinInclusiveFacet");
            id7_Identifier = Reader.NameTable.Add(@"Identifier");
            id113_XmlSchemaSimpleTypeList = Reader.NameTable.Add(@"XmlSchemaSimpleTypeList");
            id41_default = Reader.NameTable.Add(@"default");
            id125_extension = Reader.NameTable.Add(@"extension");
            id16_Item = Reader.NameTable.Add(@"http://schemas.xmlsoap.org/ws/2004/08/addressing");
            id1000_Item = Reader.NameTable.Add(@"http://www.w3.org/2005/08/addressing");
            id124_XmlSchemaComplexContent = Reader.NameTable.Add(@"XmlSchemaComplexContent");
            id72_complexContent = Reader.NameTable.Add(@"complexContent");
            id11_Item = Reader.NameTable.Add(@"http://schemas.xmlsoap.org/wsdl/");
            id25_include = Reader.NameTable.Add(@"include");
            id34_simpleType = Reader.NameTable.Add(@"simpleType");
            id91_minExclusive = Reader.NameTable.Add(@"minExclusive");
            id94_pattern = Reader.NameTable.Add(@"pattern");
            id2_Item = Reader.NameTable.Add(@"http://schemas.xmlsoap.org/ws/2004/09/mex");
            id95_enumeration = Reader.NameTable.Add(@"enumeration");
            id114_itemType = Reader.NameTable.Add(@"itemType");
            id115_XmlSchemaSimpleTypeUnion = Reader.NameTable.Add(@"XmlSchemaSimpleTypeUnion");
            id59_XmlSchemaAnnotation = Reader.NameTable.Add(@"XmlSchemaAnnotation");
            id28_notation = Reader.NameTable.Add(@"notation");
            id84_list = Reader.NameTable.Add(@"list");
            id39_abstract = Reader.NameTable.Add(@"abstract");
            id103_XmlSchemaWhiteSpaceFacet = Reader.NameTable.Add(@"XmlSchemaWhiteSpaceFacet");
            id110_XmlSchemaMaxInclusiveFacet = Reader.NameTable.Add(@"XmlSchemaMaxInclusiveFacet");
            id55_selector = Reader.NameTable.Add(@"selector");
            id43_fixed = Reader.NameTable.Add(@"fixed");
            id57_XmlSchemaXPath = Reader.NameTable.Add(@"XmlSchemaXPath");
            id118_XmlSchemaAll = Reader.NameTable.Add(@"XmlSchemaAll");
            id56_field = Reader.NameTable.Add(@"field");
            id119_XmlSchemaChoice = Reader.NameTable.Add(@"XmlSchemaChoice");
            id123_XmlSchemaAny = Reader.NameTable.Add(@"XmlSchemaAny");
            id132_XmlSchemaGroup = Reader.NameTable.Add(@"XmlSchemaGroup");
            id35_element = Reader.NameTable.Add(@"element");
            id129_Item = Reader.NameTable.Add(@"XmlSchemaSimpleContentExtension");
            id30_annotation = Reader.NameTable.Add(@"annotation");
            id44_form = Reader.NameTable.Add(@"form");
            id21_elementFormDefault = Reader.NameTable.Add(@"elementFormDefault");
            id98_totalDigits = Reader.NameTable.Add(@"totalDigits");
            id88_maxExclusive = Reader.NameTable.Add(@"maxExclusive");
            id42_final = Reader.NameTable.Add(@"final");
            id46_nillable = Reader.NameTable.Add(@"nillable");
            id9_Item = Reader.NameTable.Add(@"http://www.w3.org/2001/XMLSchema");
            id61_appinfo = Reader.NameTable.Add(@"appinfo");
            id38_maxOccurs = Reader.NameTable.Add(@"maxOccurs");
            id70_mixed = Reader.NameTable.Add(@"mixed");
            id87_base = Reader.NameTable.Add(@"base");
            id13_Location = Reader.NameTable.Add(@"Location");
            id12_MetadataReference = Reader.NameTable.Add(@"MetadataReference");
            id97_whiteSpace = Reader.NameTable.Add(@"whiteSpace");
            id29_group = Reader.NameTable.Add(@"group");
            id92_minLength = Reader.NameTable.Add(@"minLength");
            id99_fractionDigits = Reader.NameTable.Add(@"fractionDigits");
            id137_schemaLocation = Reader.NameTable.Add(@"schemaLocation");
            id26_redefine = Reader.NameTable.Add(@"redefine");
            id101_value = Reader.NameTable.Add(@"value");
            id63_source = Reader.NameTable.Add(@"source");
            id89_minInclusive = Reader.NameTable.Add(@"minInclusive");
            id133_XmlSchemaNotation = Reader.NameTable.Add(@"XmlSchemaNotation");
            id52_keyref = Reader.NameTable.Add(@"keyref");
            id33_complexType = Reader.NameTable.Add(@"complexType");
            id135_system = Reader.NameTable.Add(@"system");
            id50_unique = Reader.NameTable.Add(@"unique");
            id74_choice = Reader.NameTable.Add(@"choice");
            id66_Item = Reader.NameTable.Add(@"http://www.w3.org/XML/1998/namespace");
            id105_XmlSchemaEnumerationFacet = Reader.NameTable.Add(@"XmlSchemaEnumerationFacet");
            id107_XmlSchemaMaxLengthFacet = Reader.NameTable.Add(@"XmlSchemaMaxLengthFacet");
            id36_XmlSchemaElement = Reader.NameTable.Add(@"XmlSchemaElement");
            id106_XmlSchemaPatternFacet = Reader.NameTable.Add(@"XmlSchemaPatternFacet");
            id37_minOccurs = Reader.NameTable.Add(@"minOccurs");
            id130_Item = Reader.NameTable.Add(@"XmlSchemaSimpleContentRestriction");
            id68_XmlSchemaUnique = Reader.NameTable.Add(@"XmlSchemaUnique");
            id131_XmlSchemaAttributeGroup = Reader.NameTable.Add(@"XmlSchemaAttributeGroup");
            id40_block = Reader.NameTable.Add(@"block");
            id81_use = Reader.NameTable.Add(@"use");
            id85_restriction = Reader.NameTable.Add(@"restriction");
            id1_Metadata = Reader.NameTable.Add(@"Metadata");
            id69_XmlSchemaComplexType = Reader.NameTable.Add(@"XmlSchemaComplexType");
            id117_XmlSchemaAttributeGroupRef = Reader.NameTable.Add(@"XmlSchemaAttributeGroupRef");
            id138_XmlSchemaRedefine = Reader.NameTable.Add(@"XmlSchemaRedefine");
            id6_Item = Reader.NameTable.Add(@"");
            id102_XmlSchemaTotalDigitsFacet = Reader.NameTable.Add(@"XmlSchemaTotalDigitsFacet");
            id58_xpath = Reader.NameTable.Add(@"xpath");
            id5_Dialect = Reader.NameTable.Add(@"Dialect");
            id14_MetadataLocation = Reader.NameTable.Add(@"MetadataLocation");
            id3_MetadataSet = Reader.NameTable.Add(@"MetadataSet");
            id79_processContents = Reader.NameTable.Add(@"processContents");
            id76_anyAttribute = Reader.NameTable.Add(@"anyAttribute");
            id19_blockDefault = Reader.NameTable.Add(@"blockDefault");
            id136_XmlSchemaImport = Reader.NameTable.Add(@"XmlSchemaImport");
            id109_XmlSchemaMinExclusiveFacet = Reader.NameTable.Add(@"XmlSchemaMinExclusiveFacet");
            id108_XmlSchemaMinLengthFacet = Reader.NameTable.Add(@"XmlSchemaMinLengthFacet");
            id8_schema = Reader.NameTable.Add(@"schema");
            id62_XmlSchemaAppInfo = Reader.NameTable.Add(@"XmlSchemaAppInfo");
        }
    }

    internal abstract class XmlSerializer1 : System.Xml.Serialization.XmlSerializer
    {
        protected override System.Xml.Serialization.XmlSerializationReader CreateReader()
        {
            return new XmlSerializationReaderMetadataSet();
        }
        protected override System.Xml.Serialization.XmlSerializationWriter CreateWriter()
        {
            return new XmlSerializationWriterMetadataSet();
        }
    }

    internal sealed class MetadataSetSerializer : XmlSerializer1
    {
        bool processOuterElement = true;
        public bool ProcessOuterElement
        {
            get { return processOuterElement; }
            set { processOuterElement = value; }
        }

        public override System.Boolean CanDeserialize(System.Xml.XmlReader xmlReader)
        {
            return xmlReader.IsStartElement(@"Metadata", @"http://schemas.xmlsoap.org/ws/2004/09/mex");
        }

        protected override void Serialize(object objectToSerialize, System.Xml.Serialization.XmlSerializationWriter writer)
        {
            ((XmlSerializationWriterMetadataSet)writer).ProcessOuterElement = this.processOuterElement;
            ((XmlSerializationWriterMetadataSet)writer).Write68_Metadata(objectToSerialize);
        }

        protected override object Deserialize(System.Xml.Serialization.XmlSerializationReader reader)
        {
            ((XmlSerializationReaderMetadataSet)reader).ProcessOuterElement = this.processOuterElement;
            return ((XmlSerializationReaderMetadataSet)reader).Read68_Metadata();
        }
    }

    internal class XmlSerializerContract : global::System.Xml.Serialization.XmlSerializerImplementation
    {
        public override global::System.Xml.Serialization.XmlSerializationReader Reader { get { return new XmlSerializationReaderMetadataSet(); } }
        public override global::System.Xml.Serialization.XmlSerializationWriter Writer { get { return new XmlSerializationWriterMetadataSet(); } }
        System.Collections.Hashtable readMethods = null;
        public override System.Collections.Hashtable ReadMethods
        {
            get
            {
                if (readMethods == null)
                {
                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                    _tmp[@"System.ServiceModel.Description.MetadataSet:http://schemas.xmlsoap.org/ws/2004/09/mex:Metadata:True:"] = @"Read68_Metadata";
                    if (readMethods == null) readMethods = _tmp;
                }
                return readMethods;
            }
        }
        System.Collections.Hashtable writeMethods = null;
        public override System.Collections.Hashtable WriteMethods
        {
            get
            {
                if (writeMethods == null)
                {
                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                    _tmp[@"System.ServiceModel.Description.MetadataSet:http://schemas.xmlsoap.org/ws/2004/09/mex:Metadata:True:"] = @"Write68_Metadata";
                    if (writeMethods == null) writeMethods = _tmp;
                }
                return writeMethods;
            }
        }
        System.Collections.Hashtable typedSerializers = null;
        public override System.Collections.Hashtable TypedSerializers
        {
            get
            {
                if (typedSerializers == null)
                {
                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                    _tmp.Add(@"System.ServiceModel.Description.MetadataSet:http://schemas.xmlsoap.org/ws/2004/09/mex:Metadata:True:", new MetadataSetSerializer());
                    if (typedSerializers == null) typedSerializers = _tmp;
                }
                return typedSerializers;
            }
        }
        public override System.Boolean CanSerialize(System.Type type)
        {
            if (type == typeof(global::System.ServiceModel.Description.MetadataSet)) return true;
            return false;
        }
        public override System.Xml.Serialization.XmlSerializer GetSerializer(System.Type type)
        {
            if (type == typeof(global::System.ServiceModel.Description.MetadataSet)) return new MetadataSetSerializer();
            return null;
        }
    }

    // end generated code
#pragma warning restore
}
