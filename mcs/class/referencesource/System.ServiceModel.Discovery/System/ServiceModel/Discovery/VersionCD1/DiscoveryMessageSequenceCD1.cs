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
    public class DiscoveryMessageSequenceCD1 : IXmlSerializable
    {
        DiscoveryMessageSequence discoveryMessageSequence;

        DiscoveryMessageSequenceCD1()
        {
            this.discoveryMessageSequence = new DiscoveryMessageSequence();
        }

        DiscoveryMessageSequenceCD1(DiscoveryMessageSequence discoveryMessageSequence)
        {
            this.discoveryMessageSequence = discoveryMessageSequence;
        }

        public static DiscoveryMessageSequenceCD1 FromDiscoveryMessageSequence(DiscoveryMessageSequence discoveryMessageSequence)
        {
            if (discoveryMessageSequence == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryMessageSequence");
            }
            return new DiscoveryMessageSequenceCD1(discoveryMessageSequence);
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaSet");
            }

            return SchemaUtility.EnsureAppSequenceSchema(DiscoveryVersion.WSDiscoveryCD1, schemaSet);
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
