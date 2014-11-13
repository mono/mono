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
    public class DiscoveryMessageSequenceApril2005 : IXmlSerializable
    {
        DiscoveryMessageSequence discoveryMessageSequence;

        DiscoveryMessageSequenceApril2005()
        {
            this.discoveryMessageSequence = new DiscoveryMessageSequence();
        }

        DiscoveryMessageSequenceApril2005(DiscoveryMessageSequence discoveryMessageSequence)
        {
            this.discoveryMessageSequence = discoveryMessageSequence;
        }

        public static DiscoveryMessageSequenceApril2005 FromDiscoveryMessageSequence(DiscoveryMessageSequence discoveryMessageSequence)
        {
            if (discoveryMessageSequence == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryMessageSequence");
            }
            return new DiscoveryMessageSequenceApril2005(discoveryMessageSequence);
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaSet");
            }

            return SchemaUtility.EnsureAppSequenceSchema(DiscoveryVersion.WSDiscoveryApril2005, schemaSet);
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
