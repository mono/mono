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
    [XmlRoot(AddressingStrings.EndpointReference, Namespace = Addressing200408Strings.Namespace)]
    public class EndpointAddressAugust2004 : IXmlSerializable
    {
        static XmlQualifiedName eprType;

        EndpointAddress address;

        // for IXmlSerializable
        EndpointAddressAugust2004()
        {
            this.address = null;
        }

        EndpointAddressAugust2004(EndpointAddress address)
        {
            this.address = address;
        }

        public static EndpointAddressAugust2004 FromEndpointAddress(EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            return new EndpointAddressAugust2004(address);
        }

        public EndpointAddress ToEndpointAddress()
        {
            return this.address;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            this.address = EndpointAddress.ReadFrom(AddressingVersion.WSAddressingAugust2004, XmlDictionaryReader.CreateDictionaryReader(reader));
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            this.address.WriteContentsTo(AddressingVersion.WSAddressingAugust2004, XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        static XmlQualifiedName EprType
        {
            get
            {
                if (eprType == null)
                    eprType = new XmlQualifiedName(AddressingStrings.EndpointReferenceType, Addressing200408Strings.Namespace);
                return eprType;
            }
        }

        static XmlSchema GetEprSchema()
        {
            using (XmlTextReader reader = new XmlTextReader(new StringReader(Schema)) { DtdProcessing = DtdProcessing.Prohibit })
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
            ICollection schemas = xmlSchemaSet.Schemas(Addressing200408Strings.Namespace);
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
@"<xs:schema targetNamespace=""http://schemas.xmlsoap.org/ws/2004/08/addressing"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:wsa=""http://schemas.xmlsoap.org/ws/2004/08/addressing"" elementFormDefault=""qualified"" blockDefault=""#all"">
  <!-- //////////////////// WS-Addressing //////////////////// -->
  <!-- Endpoint reference -->
  <xs:element name=""EndpointReference"" type=""wsa:EndpointReferenceType""/>
  <xs:complexType name=""EndpointReferenceType"">
    <xs:sequence>
      <xs:element name=""Address"" type=""wsa:AttributedURI""/>
      <xs:element name=""ReferenceProperties"" type=""wsa:ReferencePropertiesType"" minOccurs=""0""/>
      <xs:element name=""ReferenceParameters"" type=""wsa:ReferenceParametersType"" minOccurs=""0""/>
      <xs:element name=""PortType"" type=""wsa:AttributedQName"" minOccurs=""0""/>
      <xs:element name=""ServiceName"" type=""wsa:ServiceNameType"" minOccurs=""0""/>
      <xs:any namespace=""##other"" processContents=""lax"" minOccurs=""0"" maxOccurs=""unbounded"">
        <xs:annotation>
          <xs:documentation>
					 If ""Policy"" elements from namespace ""http://schemas.xmlsoap.org/ws/2002/12/policy#policy"" are used, they must appear first (before any extensibility elements).
					</xs:documentation>
        </xs:annotation>
      </xs:any>
    </xs:sequence>
    <xs:anyAttribute namespace=""##other"" processContents=""lax""/>
  </xs:complexType>
  <xs:complexType name=""ReferencePropertiesType"">
    <xs:sequence>
      <xs:any processContents=""lax"" minOccurs=""0"" maxOccurs=""unbounded""/>
    </xs:sequence>
  </xs:complexType>
  <xs:complexType name=""ReferenceParametersType"">
    <xs:sequence>
      <xs:any processContents=""lax"" minOccurs=""0"" maxOccurs=""unbounded""/>
    </xs:sequence>
  </xs:complexType>
  <xs:complexType name=""ServiceNameType"">
    <xs:simpleContent>
      <xs:extension base=""xs:QName"">
        <xs:attribute name=""PortName"" type=""xs:NCName""/>
        <xs:anyAttribute namespace=""##other"" processContents=""lax""/>
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>
  <!-- Message information header blocks -->
  <xs:element name=""MessageID"" type=""wsa:AttributedURI""/>
  <xs:element name=""RelatesTo"" type=""wsa:Relationship""/>
  <xs:element name=""To"" type=""wsa:AttributedURI""/>
  <xs:element name=""Action"" type=""wsa:AttributedURI""/>
  <xs:element name=""From"" type=""wsa:EndpointReferenceType""/>
  <xs:element name=""ReplyTo"" type=""wsa:EndpointReferenceType""/>
  <xs:element name=""FaultTo"" type=""wsa:EndpointReferenceType""/>
  <xs:complexType name=""Relationship"">
    <xs:simpleContent>
      <xs:extension base=""xs:anyURI"">
        <xs:attribute name=""RelationshipType"" type=""xs:QName"" use=""optional""/>
        <xs:anyAttribute namespace=""##other"" processContents=""lax""/>
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>
  <xs:simpleType name=""RelationshipTypeValues"">
    <xs:restriction base=""xs:QName"">
      <xs:enumeration value=""wsa:Reply""/>
    </xs:restriction>
  </xs:simpleType>
  <xs:element name=""ReplyAfter"" type=""wsa:ReplyAfterType""/>
  <xs:complexType name=""ReplyAfterType"">
    <xs:simpleContent>
      <xs:extension base=""xs:nonNegativeInteger"">
        <xs:anyAttribute namespace=""##other""/>
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>
  <xs:simpleType name=""FaultSubcodeValues"">
    <xs:restriction base=""xs:QName"">
      <xs:enumeration value=""wsa:InvalidMessageInformationHeader""/>
      <xs:enumeration value=""wsa:MessageInformationHeaderRequired""/>
      <xs:enumeration value=""wsa:DestinationUnreachable""/>
      <xs:enumeration value=""wsa:ActionNotSupported""/>
      <xs:enumeration value=""wsa:EndpointUnavailable""/>
    </xs:restriction>
  </xs:simpleType>
  <xs:attribute name=""Action"" type=""xs:anyURI""/>
  <!-- Common declarations and definitions -->
  <xs:complexType name=""AttributedQName"">
    <xs:simpleContent>
      <xs:extension base=""xs:QName"">
        <xs:anyAttribute namespace=""##other"" processContents=""lax""/>
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>
  <xs:complexType name=""AttributedURI"">
    <xs:simpleContent>
      <xs:extension base=""xs:anyURI"">
        <xs:anyAttribute namespace=""##other"" processContents=""lax""/>
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>
</xs:schema>";
    }
}
