//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Text;
    using System.IO;

    [XmlSchemaProvider("GetSchema")]
    [XmlRoot(AddressingStrings.EndpointReference, Namespace = Addressing10Strings.Namespace)]
    public class EndpointAddress10 : IXmlSerializable
    {
        static XmlQualifiedName eprType;

        EndpointAddress address;

        // for IXmlSerializable
        EndpointAddress10()
        {
            this.address = null;
        }

        EndpointAddress10(EndpointAddress address)
        {
            this.address = address;
        }

        public static EndpointAddress10 FromEndpointAddress(EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            return new EndpointAddress10(address);
        }

        public EndpointAddress ToEndpointAddress()
        {
            return this.address;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            this.address = EndpointAddress.ReadFrom(AddressingVersion.WSAddressing10, XmlDictionaryReader.CreateDictionaryReader(reader));
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            this.address.WriteContentsTo(AddressingVersion.WSAddressing10, XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        static XmlQualifiedName EprType
        {
            get
            {
                if (eprType == null)
                    eprType = new XmlQualifiedName(AddressingStrings.EndpointReferenceType, Addressing10Strings.Namespace);
                return eprType;
            }
        }

        static XmlSchema GetEprSchema()
        {
            using (XmlTextReader reader = new XmlTextReader(new StringReader(Schema)))
            {
                return XmlSchema.Read(reader, null);
            }

        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet xmlSchemaSet)
        {
            if (xmlSchemaSet == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlSchemaSet");
            XmlQualifiedName eprType = EprType;
            XmlSchema eprSchema = GetEprSchema();
            ICollection schemas = xmlSchemaSet.Schemas(Addressing10Strings.Namespace);
            if (schemas == null || schemas.Count == 0)
                xmlSchemaSet.Add(eprSchema);
            else
            {
                XmlSchema schemaToAdd = null;
                foreach (XmlSchema xmlSchema in schemas)
                {
                    if (xmlSchema.SchemaTypes.Contains(eprType))
                    {
                        schemaToAdd = null;
                        break;
                    }
                    else
                        schemaToAdd = xmlSchema;
                }
                if (schemaToAdd != null)
                {
                    foreach (XmlQualifiedName prefixNsPair in eprSchema.Namespaces.ToArray())
                        schemaToAdd.Namespaces.Add(prefixNsPair.Name, prefixNsPair.Namespace);
                    foreach (XmlSchemaObject schemaObject in eprSchema.Items)
                        schemaToAdd.Items.Add(schemaObject);
                    xmlSchemaSet.Reprocess(schemaToAdd);
                }
            }
            return eprType;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        const string Schema =
@"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:wsa='http://www.w3.org/2005/08/addressing' targetNamespace='http://www.w3.org/2005/08/addressing' blockDefault='#all' elementFormDefault='qualified' finalDefault='' attributeFormDefault='unqualified'>
    
    <!-- Constructs from the WS-Addressing Core -->

    <xs:element name='EndpointReference' type='wsa:EndpointReferenceType'/>
    <xs:complexType name='EndpointReferenceType' mixed='false'>
        <xs:sequence>
            <xs:element name='Address' type='wsa:AttributedURIType'/>
            <xs:element name='ReferenceParameters' type='wsa:ReferenceParametersType' minOccurs='0'/>
            <xs:element ref='wsa:Metadata' minOccurs='0'/>
            <xs:any namespace='##other' processContents='lax' minOccurs='0' maxOccurs='unbounded'/>
        </xs:sequence>
        <xs:anyAttribute namespace='##other' processContents='lax'/>
    </xs:complexType>
    
    <xs:complexType name='ReferenceParametersType' mixed='false'>
        <xs:sequence>
            <xs:any namespace='##any' processContents='lax' minOccurs='0' maxOccurs='unbounded'/>
        </xs:sequence>
        <xs:anyAttribute namespace='##other' processContents='lax'/>
    </xs:complexType>
    
    <xs:element name='Metadata' type='wsa:MetadataType'/>
    <xs:complexType name='MetadataType' mixed='false'>
        <xs:sequence>
            <xs:any namespace='##any' processContents='lax' minOccurs='0' maxOccurs='unbounded'/>
        </xs:sequence>
        <xs:anyAttribute namespace='##other' processContents='lax'/>
    </xs:complexType>
    
    <xs:element name='MessageID' type='wsa:AttributedURIType'/>
    <xs:element name='RelatesTo' type='wsa:RelatesToType'/>
    <xs:complexType name='RelatesToType' mixed='false'>
        <xs:simpleContent>
            <xs:extension base='xs:anyURI'>
                <xs:attribute name='RelationshipType' type='wsa:RelationshipTypeOpenEnum' use='optional' default='http://www.w3.org/2005/08/addressing/reply'/>
                <xs:anyAttribute namespace='##other' processContents='lax'/>
            </xs:extension>
        </xs:simpleContent>
    </xs:complexType>
    
    <xs:simpleType name='RelationshipTypeOpenEnum'>
        <xs:union memberTypes='wsa:RelationshipType xs:anyURI'/>
    </xs:simpleType>
    
    <xs:simpleType name='RelationshipType'>
        <xs:restriction base='xs:anyURI'>
            <xs:enumeration value='http://www.w3.org/2005/08/addressing/reply'/>
        </xs:restriction>
    </xs:simpleType>
    
    <xs:element name='ReplyTo' type='wsa:EndpointReferenceType'/>
    <xs:element name='From' type='wsa:EndpointReferenceType'/>
    <xs:element name='FaultTo' type='wsa:EndpointReferenceType'/>
    <xs:element name='To' type='wsa:AttributedURIType'/>
    <xs:element name='Action' type='wsa:AttributedURIType'/>

    <xs:complexType name='AttributedURIType' mixed='false'>
        <xs:simpleContent>
            <xs:extension base='xs:anyURI'>
                <xs:anyAttribute namespace='##other' processContents='lax'/>
            </xs:extension>
        </xs:simpleContent>
    </xs:complexType>
    
    <!-- Constructs from the WS-Addressing SOAP binding -->

    <xs:attribute name='IsReferenceParameter' type='xs:boolean'/>
    
    <xs:simpleType name='FaultCodesOpenEnumType'>
        <xs:union memberTypes='wsa:FaultCodesType xs:QName'/>
    </xs:simpleType>
    
    <xs:simpleType name='FaultCodesType'>
        <xs:restriction base='xs:QName'>
            <xs:enumeration value='wsa:InvalidAddressingHeader'/>
            <xs:enumeration value='wsa:InvalidAddress'/>
            <xs:enumeration value='wsa:InvalidEPR'/>
            <xs:enumeration value='wsa:InvalidCardinality'/>
            <xs:enumeration value='wsa:MissingAddressInEPR'/>
            <xs:enumeration value='wsa:DuplicateMessageID'/>
            <xs:enumeration value='wsa:ActionMismatch'/>
            <xs:enumeration value='wsa:MessageAddressingHeaderRequired'/>
            <xs:enumeration value='wsa:DestinationUnreachable'/>
            <xs:enumeration value='wsa:ActionNotSupported'/>
            <xs:enumeration value='wsa:EndpointUnavailable'/>
        </xs:restriction>
    </xs:simpleType>
    
    <xs:element name='RetryAfter' type='wsa:AttributedUnsignedLongType'/>
    <xs:complexType name='AttributedUnsignedLongType' mixed='false'>
        <xs:simpleContent>
            <xs:extension base='xs:unsignedLong'>
                <xs:anyAttribute namespace='##other' processContents='lax'/>
            </xs:extension>
        </xs:simpleContent>
    </xs:complexType>
    
    <xs:element name='ProblemHeaderQName' type='wsa:AttributedQNameType'/>
    <xs:complexType name='AttributedQNameType' mixed='false'>
        <xs:simpleContent>
            <xs:extension base='xs:QName'>
                <xs:anyAttribute namespace='##other' processContents='lax'/>
            </xs:extension>
        </xs:simpleContent>
    </xs:complexType>
    
    <xs:element name='ProblemHeader' type='wsa:AttributedAnyType'/>
    <xs:complexType name='AttributedAnyType' mixed='false'>
        <xs:sequence>
            <xs:any namespace='##any' processContents='lax' minOccurs='1' maxOccurs='1'/>
        </xs:sequence>
        <xs:anyAttribute namespace='##other' processContents='lax'/>
    </xs:complexType>
    
    <xs:element name='ProblemIRI' type='wsa:AttributedURIType'/>
    
    <xs:element name='ProblemAction' type='wsa:ProblemActionType'/>
    <xs:complexType name='ProblemActionType' mixed='false'>
        <xs:sequence>
            <xs:element ref='wsa:Action' minOccurs='0'/>
            <xs:element name='SoapAction' minOccurs='0' type='xs:anyURI'/>
        </xs:sequence>
        <xs:anyAttribute namespace='##other' processContents='lax'/>
    </xs:complexType>
    
</xs:schema>";
    }
}
