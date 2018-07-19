//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.Runtime;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlSchemaProvider("GetSchema")]
    [Fx.Tag.XamlVisible(false)]
    public class EndpointDiscoveryMetadataApril2005 : IXmlSerializable
    {
        EndpointDiscoveryMetadata endpointDiscoveryMetadata;

        EndpointDiscoveryMetadataApril2005()
        {
            this.endpointDiscoveryMetadata = new EndpointDiscoveryMetadata();
        }

        EndpointDiscoveryMetadataApril2005(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            this.endpointDiscoveryMetadata = endpointDiscoveryMetadata;
        }

        public static EndpointDiscoveryMetadataApril2005 FromEndpointDiscoveryMetadata(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            if (endpointDiscoveryMetadata == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpointDiscoveryMetadata");
            }

            return new EndpointDiscoveryMetadataApril2005(endpointDiscoveryMetadata);
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaSet");
            }

            return SchemaUtility.EnsureProbeMatchSchema(DiscoveryVersion.WSDiscoveryApril2005, schemaSet);
        }

        public EndpointDiscoveryMetadata ToEndpointDiscoveryMetadata()
        {
            return this.endpointDiscoveryMetadata;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        [Fx.Tag.InheritThrows(From = "ReadFrom", FromDeclaringType = typeof(EndpointDiscoveryMetadata))]
        public void ReadXml(XmlReader reader)
        {
            this.endpointDiscoveryMetadata.ReadFrom(DiscoveryVersion.WSDiscoveryApril2005, reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            this.endpointDiscoveryMetadata.WriteTo(DiscoveryVersion.WSDiscoveryApril2005, writer);
        }        
    }
}
