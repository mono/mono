//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Xml;
    using System.Xml.Schema;

    static class SchemaUtility
    {
        public static XmlQualifiedName EnsureProbeMatchSchema(DiscoveryVersion discoveryVersion, XmlSchemaSet schemaSet)
        {
            Fx.Assert(schemaSet != null, "The schemaSet must be non null.");
            Fx.Assert(discoveryVersion != null, "The discoveryVersion must be non null.");

            // ensure that EPR is added to the schema.
            if (discoveryVersion == DiscoveryVersion.WSDiscoveryApril2005 || discoveryVersion == DiscoveryVersion.WSDiscoveryCD1)
            {
                EndpointAddressAugust2004.GetSchema(schemaSet);
            }
            else if (discoveryVersion == DiscoveryVersion.WSDiscovery11)
            {
                EndpointAddress10.GetSchema(schemaSet);
            }
            else
            {
                Fx.Assert("The discoveryVersion is not supported.");
            }

            // do not add/find Probe related schema items
            SchemaTypes typesFound = SchemaTypes.ProbeType | SchemaTypes.ResolveType;
            SchemaElements elementsFound = SchemaElements.None;         

            XmlSchema discoverySchema = null;
            ICollection discoverySchemas = schemaSet.Schemas(discoveryVersion.Namespace);
            if ((discoverySchemas == null) || (discoverySchemas.Count == 0))
            {
                discoverySchema = CreateSchema(discoveryVersion);
                AddImport(discoverySchema, discoveryVersion.Implementation.WsaNamespace);
                schemaSet.Add(discoverySchema);
            }
            else
            {                
                foreach (XmlSchema schema in discoverySchemas)
                {
                    discoverySchema = schema;
                    if (schema.SchemaTypes.Contains(discoveryVersion.Implementation.QualifiedNames.ProbeMatchType))
                    {
                        typesFound |= SchemaTypes.ProbeMatchType;
                        break;
                    }

                    LocateSchemaTypes(discoveryVersion, schema, ref typesFound);
                    LocateSchemaElements(discoveryVersion, schema, ref elementsFound);
                }
            }

            if ((typesFound & SchemaTypes.ProbeMatchType) != SchemaTypes.ProbeMatchType)
            {
                AddSchemaTypes(discoveryVersion, typesFound, discoverySchema);
                AddElements(discoveryVersion, elementsFound, discoverySchema);
                schemaSet.Reprocess(discoverySchema);
            }

            return discoveryVersion.Implementation.QualifiedNames.ProbeMatchType;
        }

        public static XmlQualifiedName EnsureProbeSchema(DiscoveryVersion discoveryVersion, XmlSchemaSet schemaSet)
        {
            Fx.Assert(schemaSet != null, "The schemaSet must be non null.");
            Fx.Assert(discoveryVersion != null, "The discoveryVersion must be non null.");

            // do not find/add ProbeMatch related schema items
            SchemaTypes typesFound = SchemaTypes.ProbeMatchType | SchemaTypes.ResolveType;
            SchemaElements elementsFound = SchemaElements.XAddrs | SchemaElements.MetadataVersion;         

            XmlSchema discoverySchema = null;
            ICollection discoverySchemas = schemaSet.Schemas(discoveryVersion.Namespace);
            if ((discoverySchemas == null) || (discoverySchemas.Count == 0))
            {
                discoverySchema = CreateSchema(discoveryVersion);
                schemaSet.Add(discoverySchema);
            }
            else
            {
                foreach (XmlSchema schema in discoverySchemas)
                {
                    discoverySchema = schema;
                    if (schema.SchemaTypes.Contains(discoveryVersion.Implementation.QualifiedNames.ProbeType))
                    {
                        typesFound |= SchemaTypes.ProbeType;
                        break;
                    }

                    LocateSchemaTypes(discoveryVersion, schema, ref typesFound);
                    LocateSchemaElements(discoveryVersion, schema, ref elementsFound);
                }
            }

            if ((typesFound & SchemaTypes.ProbeType) != SchemaTypes.ProbeType)
            {
                AddSchemaTypes(discoveryVersion, typesFound, discoverySchema);
                AddElements(discoveryVersion, elementsFound, discoverySchema);
                schemaSet.Reprocess(discoverySchema);
            }

            return discoveryVersion.Implementation.QualifiedNames.ProbeType;
        }

        public static XmlQualifiedName EnsureResolveSchema(DiscoveryVersion discoveryVersion, XmlSchemaSet schemaSet)
        {
            Fx.Assert(schemaSet != null, "The schemaSet must be non null.");
            Fx.Assert(discoveryVersion != null, "The discoveryVersion must be non null.");

            SchemaTypes typesFound = SchemaTypes.ProbeType |
                SchemaTypes.ProbeMatchType | 
                SchemaTypes.QNameListType | 
                SchemaTypes.ScopesType | 
                SchemaTypes.UriListType;

            // ensure that EPR is added to the schema.
            if (discoveryVersion == DiscoveryVersion.WSDiscoveryApril2005 || discoveryVersion == DiscoveryVersion.WSDiscoveryCD1)
            {
                EndpointAddressAugust2004.GetSchema(schemaSet);
            }
            else if (discoveryVersion == DiscoveryVersion.WSDiscovery11)
            {
                EndpointAddress10.GetSchema(schemaSet);
            }
            else
            {
                Fx.Assert("The discoveryVersion is not supported.");
            }

            XmlSchema discoverySchema = null;
            ICollection discoverySchemas = schemaSet.Schemas(discoveryVersion.Namespace);
            if ((discoverySchemas == null) || (discoverySchemas.Count == 0))
            {
                discoverySchema = CreateSchema(discoveryVersion);
                AddImport(discoverySchema, discoveryVersion.Implementation.WsaNamespace);
                schemaSet.Add(discoverySchema);
            }
            else
            {
                foreach (XmlSchema schema in discoverySchemas)
                {
                    discoverySchema = schema;
                    if (schema.SchemaTypes.Contains(discoveryVersion.Implementation.QualifiedNames.ResolveType))
                    {
                        typesFound |= SchemaTypes.ResolveType;
                        break;
                    }                                        
                }
            }

            if ((typesFound & SchemaTypes.ResolveType) != SchemaTypes.ResolveType)
            {
                AddSchemaTypes(discoveryVersion, typesFound, discoverySchema);                
                schemaSet.Reprocess(discoverySchema);
            }

            return discoveryVersion.Implementation.QualifiedNames.ResolveType;
        }

        public static XmlQualifiedName EnsureAppSequenceSchema(DiscoveryVersion discoveryVersion, XmlSchemaSet schemaSet)
        {
            Fx.Assert(schemaSet != null, "The schemaSet must be non null.");
            Fx.Assert(discoveryVersion != null, "The discoveryVersion must be non null.");

            bool add = true;
            XmlSchema discoverySchema = null;
            ICollection discoverySchemas = schemaSet.Schemas(discoveryVersion.Namespace);
            if ((discoverySchemas == null) || (discoverySchemas.Count == 0))
            {
                discoverySchema = CreateSchema(discoveryVersion);
                schemaSet.Add(discoverySchema);
            }
            else
            {
                foreach (XmlSchema schema in discoverySchemas)
                {
                    discoverySchema = schema;
                    if (schema.SchemaTypes.Contains(discoveryVersion.Implementation.QualifiedNames.AppSequenceType))
                    {
                        add = false;
                        break;
                    }
                }
            }
            if (add)
            {
                AddAppSequenceType(discoveryVersion, discoverySchema);
                schemaSet.Reprocess(discoverySchema);
            }
            return discoveryVersion.Implementation.QualifiedNames.AppSequenceType;
        }

        static void AddElements(DiscoveryVersion discoveryVersion, SchemaElements elementsFound, XmlSchema discoverySchema)
        {
            if ((elementsFound & SchemaElements.Types) == 0)
            {
                AddTypesElement(discoveryVersion, discoverySchema);
            }
            if ((elementsFound & SchemaElements.Scopes) == 0)
            {
                AddScopesElement(discoveryVersion, discoverySchema);
            }
            if ((elementsFound & SchemaElements.XAddrs) == 0)
            {
                AddXAddrsElement(discoveryVersion, discoverySchema);
            }
            if ((elementsFound & SchemaElements.MetadataVersion) == 0)
            {
                AddMetadataVersionSchemaElement(discoveryVersion, discoverySchema);
            }
        }

        static void AddAppSequenceType(DiscoveryVersion discoveryVersion, XmlSchema schema)
        {
            //<xs:complexType name="AppSequenceType" >
            XmlSchemaComplexType appSequenceType = new XmlSchemaComplexType();
            appSequenceType.Name = ProtocolStrings.SchemaNames.AppSequenceType;

            // <xs:complexContent>
            XmlSchemaComplexContent complexContent = new XmlSchemaComplexContent();
            appSequenceType.ContentModel = complexContent;

            // <xs:restriction base="xs:anyType" >
            XmlSchemaComplexContentRestriction contentRestriction = new XmlSchemaComplexContentRestriction();
            complexContent.Content = contentRestriction;
            contentRestriction.BaseTypeName = discoveryVersion.Implementation.QualifiedNames.AnyType;

            // <xs:attribute name="InstanceId" type="xs:unsignedInt" use="required" />
            XmlSchemaAttribute instanceId = new XmlSchemaAttribute();
            instanceId.Name = ProtocolStrings.SchemaNames.AppSequenceInstanceId;
            instanceId.SchemaTypeName = discoveryVersion.Implementation.QualifiedNames.UnsignedIntType;
            instanceId.Use = XmlSchemaUse.Required;

            // <xs:attribute name="SequenceId" type="xs:anyURI" />
            XmlSchemaAttribute sequenceId = new XmlSchemaAttribute();
            sequenceId.Name = ProtocolStrings.SchemaNames.AppSequenceSequenceId;
            sequenceId.SchemaTypeName = discoveryVersion.Implementation.QualifiedNames.AnyUriType;

            // <xs:attribute name="MessageNumber" type="xs:unsignedInt" use="required" />
            XmlSchemaAttribute messageNumber = new XmlSchemaAttribute();
            messageNumber.Name = ProtocolStrings.SchemaNames.AppSequenceMessageNumber;
            messageNumber.SchemaTypeName = discoveryVersion.Implementation.QualifiedNames.UnsignedIntType;
            messageNumber.Use = XmlSchemaUse.Required;

            // <xs:anyAttribute namespace="##other" processContents="lax" />
            XmlSchemaAnyAttribute anyAttribue = new XmlSchemaAnyAttribute();
            anyAttribue.Namespace = "##other";
            anyAttribue.ProcessContents = XmlSchemaContentProcessing.Lax;

            contentRestriction.Attributes.Add(instanceId);
            contentRestriction.Attributes.Add(sequenceId);
            contentRestriction.Attributes.Add(messageNumber);
            contentRestriction.AnyAttribute = anyAttribue;

            schema.Items.Add(appSequenceType);
        }

        static void AddImport(XmlSchema schema, string importNamespace)
        {
            XmlSchemaImport importElement = new XmlSchemaImport();
            importElement.Namespace = importNamespace;
            schema.Includes.Add(importElement);
        }

        static void AddMetadataVersionSchemaElement(DiscoveryVersion discoveryVersion, XmlSchema schema)
        {
            // <xs:element name="MetadataVersion" type="xs:unsignedInt" />
            XmlSchemaElement metadataVersionElement = new XmlSchemaElement();
            metadataVersionElement.Name = ProtocolStrings.SchemaNames.MetadataVersionElement;
            metadataVersionElement.SchemaTypeName = discoveryVersion.Implementation.QualifiedNames.UnsignedIntType;

            schema.Items.Add(metadataVersionElement);
        }

        static void AddResolveType(DiscoveryVersion discoveryVersion, XmlSchema schema)
        {
            //<xs:complexType name="ResolveType" >
            XmlSchemaComplexType resolveType = new XmlSchemaComplexType();
            resolveType.Name = ProtocolStrings.SchemaNames.ResolveType;

            //   <xs:sequence>
            XmlSchemaSequence resolveSequence = new XmlSchemaSequence();

            //     <xs:element ref="wsa:EndpointReference" />
            XmlSchemaElement eprElement = new XmlSchemaElement();
            eprElement.RefName = discoveryVersion.Implementation.QualifiedNames.EprElement;

            //     <xs:any minOccurs="0" maxOccurs="unbounded" namespace="##other" processContents="lax" />
            XmlSchemaAny any = new XmlSchemaAny();
            any.Namespace = "##other";
            any.ProcessContents = XmlSchemaContentProcessing.Lax;
            any.MinOccurs = 0;
            any.MaxOccurs = decimal.MaxValue;

            resolveSequence.Items.Add(eprElement);
            resolveSequence.Items.Add(any);

            //   <xs:anyAttribute namespace="##other" processContents="lax" />
            XmlSchemaAnyAttribute anyAttribue = new XmlSchemaAnyAttribute();
            anyAttribue.Namespace = "##other";
            anyAttribue.ProcessContents = XmlSchemaContentProcessing.Lax;

            // </xs:complexType>
            resolveType.Particle = resolveSequence;
            resolveType.AnyAttribute = anyAttribue;

            schema.Items.Add(resolveType);
        }

        static void AddProbeMatchType(DiscoveryVersion discoveryVersion, XmlSchema schema)
        {
            // <xs:complexType name="ProbeMatchType">
            XmlSchemaComplexType probeMatchType = new XmlSchemaComplexType();
            probeMatchType.Name = ProtocolStrings.SchemaNames.ProbeMatchType;

            //   <xs:sequence>
            XmlSchemaSequence probeMatcheSequence = new XmlSchemaSequence();

            //     <xs:element ref="wsa:EndpointReference" />
            XmlSchemaElement eprElement = new XmlSchemaElement();
            eprElement.RefName = discoveryVersion.Implementation.QualifiedNames.EprElement;

            //     <xs:element minOccurs="0" ref="tns:Types" />
            XmlSchemaElement typesElement = new XmlSchemaElement();
            typesElement.RefName = discoveryVersion.Implementation.QualifiedNames.TypesElement;
            typesElement.MinOccurs = 0;

            //     <xs:element minOccurs="0" ref="tns:Scopes" />
            XmlSchemaElement scopesElement = new XmlSchemaElement();
            scopesElement.RefName = discoveryVersion.Implementation.QualifiedNames.ScopesElement;
            scopesElement.MinOccurs = 0;

            //     <xs:element minOccurs="0" ref="tns:XAddrs" />
            XmlSchemaElement xAddrsElement = new XmlSchemaElement();
            xAddrsElement.RefName = discoveryVersion.Implementation.QualifiedNames.XAddrsElement;
            xAddrsElement.MinOccurs = 0;

            //     <xs:element ref="tns:MetadataVersion" /> -- allowing minOccurs=0 because the same type is used for Bye messages
            XmlSchemaElement metadataVersionElement = new XmlSchemaElement();
            metadataVersionElement.RefName = discoveryVersion.Implementation.QualifiedNames.MetadataVersionElement;
            metadataVersionElement.MinOccurs = 0;

            //     <xs:any minOccurs="0" maxOccurs="unbounded" namespace="##other" processContents="lax" />
            XmlSchemaAny any = new XmlSchemaAny();
            any.Namespace = "##other";
            any.ProcessContents = XmlSchemaContentProcessing.Lax;
            any.MinOccurs = 0;
            any.MaxOccurs = decimal.MaxValue;

            //   </xs:sequence>
            probeMatcheSequence.Items.Add(eprElement);
            probeMatcheSequence.Items.Add(typesElement);
            probeMatcheSequence.Items.Add(scopesElement);
            probeMatcheSequence.Items.Add(xAddrsElement);
            probeMatcheSequence.Items.Add(metadataVersionElement);
            probeMatcheSequence.Items.Add(any);

            //   <xs:anyAttribute namespace="##other" processContents="lax" />
            XmlSchemaAnyAttribute anyAttribue = new XmlSchemaAnyAttribute();
            anyAttribue.Namespace = "##other";
            anyAttribue.ProcessContents = XmlSchemaContentProcessing.Lax;

            // </xs:complexType>
            probeMatchType.Particle = probeMatcheSequence;
            probeMatchType.AnyAttribute = anyAttribue;

            schema.Items.Add(probeMatchType);
        }

        static void AddProbeType(DiscoveryVersion discoveryVersion, XmlSchema schema)
        {
            // <xs:complexType name="ProbeType">
            XmlSchemaComplexType probeType = new XmlSchemaComplexType();
            probeType.Name = ProtocolStrings.SchemaNames.ProbeType;

            //   <xs:sequence>
            XmlSchemaSequence probeTypeSequence = new XmlSchemaSequence();

            //     <xs:element ref="tns:Types" minOccurs="0" />
            XmlSchemaElement typesElement = new XmlSchemaElement();
            typesElement.RefName = discoveryVersion.Implementation.QualifiedNames.TypesElement;
            typesElement.MinOccurs = 0;

            //     <xs:element ref="tns:Scopes" minOccurs="0" />
            XmlSchemaElement scopesElement = new XmlSchemaElement();
            scopesElement.RefName = discoveryVersion.Implementation.QualifiedNames.ScopesElement;
            scopesElement.MinOccurs = 0;

            //     <xs:any namespace="##other" processContents="lax" minOccurs="0" maxOccurs="unbounded" />
            XmlSchemaAny any = new XmlSchemaAny();
            any.Namespace = "##other";
            any.ProcessContents = XmlSchemaContentProcessing.Lax;
            any.MinOccurs = 0;
            any.MaxOccurs = decimal.MaxValue;

            //   </xs:sequence>
            probeTypeSequence.Items.Add(typesElement);
            probeTypeSequence.Items.Add(scopesElement);
            probeTypeSequence.Items.Add(any);

            //   <xs:anyAttribute namespace="##other" processContents="lax" />
            XmlSchemaAnyAttribute anyAttribue = new XmlSchemaAnyAttribute();
            anyAttribue.Namespace = "##other";
            anyAttribue.ProcessContents = XmlSchemaContentProcessing.Lax;

            // </xs:complexType>
            probeType.Particle = probeTypeSequence;
            probeType.AnyAttribute = anyAttribue;

            schema.Items.Add(probeType);
        }

        static void AddQNameListType(DiscoveryVersion discoveryVersion, XmlSchema schema)
        {
            // <xs:simpleType name="QNameListType">
            XmlSchemaSimpleType qNameListType = new XmlSchemaSimpleType();
            qNameListType.Name = ProtocolStrings.SchemaNames.QNameListType;

            // <xs:list itemType="xs:QName" />
            XmlSchemaSimpleTypeList qNameListTypeContent = new XmlSchemaSimpleTypeList();
            qNameListTypeContent.ItemTypeName = discoveryVersion.Implementation.QualifiedNames.QNameType;

            // </xs:simpleType>
            qNameListType.Content = qNameListTypeContent;

            schema.Items.Add(qNameListType);
        }

        static void AddSchemaTypes(DiscoveryVersion discoveryVersion, SchemaTypes typesFound, XmlSchema discoverySchema)
        {
            if ((typesFound & SchemaTypes.ProbeMatchType) == 0)
            {
                AddProbeMatchType(discoveryVersion, discoverySchema);
            }
            if ((typesFound & SchemaTypes.ProbeType) == 0)
            {
                AddProbeType(discoveryVersion, discoverySchema);
            }
            if ((typesFound & SchemaTypes.ResolveType) == 0)
            {
                AddResolveType(discoveryVersion, discoverySchema);
            }
            if ((typesFound & SchemaTypes.QNameListType) == 0)
            {
                AddQNameListType(discoveryVersion, discoverySchema);
            }
            if ((typesFound & SchemaTypes.ScopesType) == 0)
            {
                AddScopesType(discoveryVersion, discoverySchema);
            }
            if ((typesFound & SchemaTypes.UriListType) == 0)
            {
                AddUriListType(discoveryVersion, discoverySchema);
            }
        }

        static void AddScopesElement(DiscoveryVersion discoveryVersion, XmlSchema schema)
        {
            // <xs:element name="Scopes" type="tns:ScopesType" />
            XmlSchemaElement scopesElement = new XmlSchemaElement();
            scopesElement.Name = ProtocolStrings.SchemaNames.ScopesElement;
            scopesElement.SchemaTypeName = discoveryVersion.Implementation.QualifiedNames.ScopesType;

            schema.Items.Add(scopesElement);
        }

        static void AddScopesType(DiscoveryVersion discoveryVersion, XmlSchema schema)
        {
            // <xs:complexType name="ScopesType">
            XmlSchemaComplexType scopesType = new XmlSchemaComplexType();
            scopesType.Name = ProtocolStrings.SchemaNames.ScopesType;

            //    <xs:simpleContent>
            XmlSchemaSimpleContent scopesTypeContent = new XmlSchemaSimpleContent();

            //       <xs:extension base="tns:UriListType">
            XmlSchemaSimpleContentExtension scopesTypeContentExtension = new XmlSchemaSimpleContentExtension();
            scopesTypeContentExtension.BaseTypeName = discoveryVersion.Implementation.QualifiedNames.UriListType;

            //          <xs:attribute name="MatchBy" type="xs:anyURI" />    
            XmlSchemaAttribute matchBy = new XmlSchemaAttribute();
            matchBy.Name = ProtocolStrings.SchemaNames.MatchByAttribute;
            matchBy.SchemaTypeName = discoveryVersion.Implementation.QualifiedNames.AnyUriType;

            //          <xs:anyAttribute namespace="##other" processContents="lax" />
            XmlSchemaAnyAttribute anyAttribute = new XmlSchemaAnyAttribute();
            anyAttribute.Namespace = "##other";
            anyAttribute.ProcessContents = XmlSchemaContentProcessing.Lax;

            //       </xs:extension>
            scopesTypeContentExtension.Attributes.Add(matchBy);
            scopesTypeContentExtension.AnyAttribute = anyAttribute;

            //    </xs:simpleContent>
            scopesTypeContent.Content = scopesTypeContentExtension;

            // <xs:complexType name="ScopesType">
            scopesType.ContentModel = scopesTypeContent;

            schema.Items.Add(scopesType);
        }

        static void AddTypesElement(DiscoveryVersion discoveryVersion, XmlSchema schema)
        {
            // <xs:element name="Types" type="tns:QNameListType" />
            XmlSchemaElement typesElement = new XmlSchemaElement();
            typesElement.Name = ProtocolStrings.SchemaNames.TypesElement;
            typesElement.SchemaTypeName = discoveryVersion.Implementation.QualifiedNames.QNameListType;

            schema.Items.Add(typesElement);
        }

        static void AddUriListType(DiscoveryVersion discoveryVersion, XmlSchema schema)
        {
            // <xs:simpleType name="UriListType">
            XmlSchemaSimpleType uriListType = new XmlSchemaSimpleType();
            uriListType.Name = ProtocolStrings.SchemaNames.UriListType;

            // <xs:list itemType="xs:anyURI" /> 
            XmlSchemaSimpleTypeList uriListTypeContent = new XmlSchemaSimpleTypeList();
            uriListTypeContent.ItemTypeName = discoveryVersion.Implementation.QualifiedNames.AnyUriType;

            // </xs:simpleType>
            uriListType.Content = uriListTypeContent;

            schema.Items.Add(uriListType);
        }

        static void AddXAddrsElement(DiscoveryVersion discoveryVersion, XmlSchema schema)
        {
            // <xs:element name="XAddrs" type="tns:UriListType" />
            XmlSchemaElement xAddrsElement = new XmlSchemaElement();
            xAddrsElement.Name = ProtocolStrings.SchemaNames.XAddrsElement;
            xAddrsElement.SchemaTypeName = discoveryVersion.Implementation.QualifiedNames.UriListType;

            schema.Items.Add(xAddrsElement);
        }

        static XmlSchema CreateSchema(DiscoveryVersion discoveryVersion)
        {
            XmlSchema schema = new XmlSchema();
            schema.TargetNamespace = discoveryVersion.Namespace;
            schema.Namespaces.Add("tns", discoveryVersion.Namespace);
            schema.ElementFormDefault = XmlSchemaForm.Qualified;
            schema.BlockDefault = XmlSchemaDerivationMethod.All;

            return schema;
        }

        static void LocateSchemaElements(DiscoveryVersion discoveryVersion, XmlSchema schema, ref SchemaElements elementsFound)
        {
            if (((elementsFound & SchemaElements.Types) != SchemaElements.Types) &&
                (schema.Elements.Contains(discoveryVersion.Implementation.QualifiedNames.TypesElement)))
            {
                elementsFound |= SchemaElements.Types;
            }
            if (((elementsFound & SchemaElements.Scopes) != SchemaElements.Scopes) &&
                (schema.Elements.Contains(discoveryVersion.Implementation.QualifiedNames.ScopesElement)))
            {
                elementsFound |= SchemaElements.Scopes;
            }
            if (((elementsFound & SchemaElements.XAddrs) != SchemaElements.XAddrs) &&
                (schema.Elements.Contains(discoveryVersion.Implementation.QualifiedNames.XAddrsElement)))
            {
                elementsFound |= SchemaElements.XAddrs;
            }
            if (((elementsFound & SchemaElements.MetadataVersion) != SchemaElements.MetadataVersion) &&
                (schema.Elements.Contains(discoveryVersion.Implementation.QualifiedNames.MetadataVersionElement)))
            {
                elementsFound |= SchemaElements.MetadataVersion;
            }
        }

        static void LocateSchemaTypes(DiscoveryVersion discoveryVersion, XmlSchema schema, ref SchemaTypes typesFound)
        {
            if (((typesFound & SchemaTypes.QNameListType) != SchemaTypes.QNameListType) &&
                (schema.SchemaTypes.Contains(discoveryVersion.Implementation.QualifiedNames.QNameListType)))
            {
                typesFound |= SchemaTypes.QNameListType;
            }
            if (((typesFound & SchemaTypes.UriListType) != SchemaTypes.UriListType) &&
                (schema.SchemaTypes.Contains(discoveryVersion.Implementation.QualifiedNames.UriListType)))
            {
                typesFound |= SchemaTypes.UriListType;
            }
            if (((typesFound & SchemaTypes.ScopesType) != SchemaTypes.ScopesType) &&
                (schema.SchemaTypes.Contains(discoveryVersion.Implementation.QualifiedNames.ScopesType)))
            {
                typesFound |= SchemaTypes.ScopesType;
            }
        }

        [Flags]
        enum SchemaTypes
        {
            None = 0,
            QNameListType = 1,            
            UriListType = 2,
            ScopesType = 4,
            ProbeType = 8,            
            ProbeMatchType = 16,                        
            ResolveType = 32
        }

        [Flags]
        enum SchemaElements
        {
            None = 0,
            Scopes = 1,
            Types = 2,            
            XAddrs = 4,
            MetadataVersion = 8
        }
    }
}
