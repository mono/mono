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
    public class DiscoveryMessageSequence11 : IXmlSerializable
    {
        DiscoveryMessageSequence discoveryMessageSequence;

        DiscoveryMessageSequence11()
        {
            this.discoveryMessageSequence = new DiscoveryMessageSequence();
        }

        DiscoveryMessageSequence11(DiscoveryMessageSequence discoveryMessageSequence)
        {
            this.discoveryMessageSequence = discoveryMessageSequence;
        }

        public static DiscoveryMessageSequence11 FromDiscoveryMessageSequence(DiscoveryMessageSequence discoveryMessageSequence)
        {
            if (discoveryMessageSequence == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryMessageSequence");
            }
            return new DiscoveryMessageSequence11(discoveryMessageSequence);
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaSet");
            }

            return SchemaUtility.EnsureAppSequenceSchema(DiscoveryVersion.WSDiscovery11, schemaSet);
        }

        public DiscoveryMessageSequence ToDiscoveryMessageSequence()
        {
            return this.discoveryMessageSequence;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        [Fx.Tag.InheritThrows(From = "ReadFrom", FromDeclaringType = typeof(DiscoveryMessageSequence))]
        public void ReadXml(XmlReader reader)
        {
            this.discoveryMessageSequence.ReadFrom(reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            this.discoveryMessageSequence.WriteTo(writer);
        }
    }
}
