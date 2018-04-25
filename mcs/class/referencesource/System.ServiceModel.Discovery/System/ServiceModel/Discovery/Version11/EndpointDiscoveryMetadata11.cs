//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.Runtime;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlSchemaProvider("GetSchema")]
    [Fx.Tag.XamlVisible(false)]
    public class EndpointDiscoveryMetadata11 : IXmlSerializable
    {
        EndpointDiscoveryMetadata endpointDiscoveryMetadata;

        EndpointDiscoveryMetadata11()
        {
            endpointDiscoveryMetadata = new EndpointDiscoveryMetadata();
        }

        EndpointDiscoveryMetadata11(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            this.endpointDiscoveryMetadata = endpointDiscoveryMetadata;
        }

        public static EndpointDiscoveryMetadata11 FromEndpointDiscoveryMetadata(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            if (endpointDiscoveryMetadata == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpointDiscoveryMetadata");
            }

            return new EndpointDiscoveryMetadata11(endpointDiscoveryMetadata);
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaSet");
            }

            return SchemaUtility.EnsureProbeMatchSchema(DiscoveryVersion.WSDiscovery11, schemaSet);
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
            this.endpointDiscoveryMetadata.ReadFrom(DiscoveryVersion.WSDiscovery11, reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            this.endpointDiscoveryMetadata.WriteTo(DiscoveryVersion.WSDiscovery11, writer);
        }
    }
}
