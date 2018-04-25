//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.Runtime;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlSchemaProvider("GetSchema")]
    [Fx.Tag.XamlVisible(false)]
    public class EndpointDiscoveryMetadataCD1 : IXmlSerializable
    {
        EndpointDiscoveryMetadata endpointDiscoveryMetadata;

        EndpointDiscoveryMetadataCD1()
        {
            endpointDiscoveryMetadata = new EndpointDiscoveryMetadata();
        }

        EndpointDiscoveryMetadataCD1(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            this.endpointDiscoveryMetadata = endpointDiscoveryMetadata;
        }

        public static EndpointDiscoveryMetadataCD1 FromEndpointDiscoveryMetadata(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            if (endpointDiscoveryMetadata == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpointDiscoveryMetadata");
            }

            return new EndpointDiscoveryMetadataCD1(endpointDiscoveryMetadata);
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaSet");
            }

            return SchemaUtility.EnsureProbeMatchSchema(DiscoveryVersion.WSDiscoveryCD1, schemaSet);
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
            this.endpointDiscoveryMetadata.ReadFrom(DiscoveryVersion.WSDiscoveryCD1, reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            this.endpointDiscoveryMetadata.WriteTo(DiscoveryVersion.WSDiscoveryCD1, writer);
        }
    }
}
