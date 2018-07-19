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
    public class FindCriteria11 : IXmlSerializable
    {
        FindCriteria findCriteria;

        FindCriteria11()
        {
            this.findCriteria = new FindCriteria();
        }

        FindCriteria11(FindCriteria findCriteria)
        {
            this.findCriteria = findCriteria;
        }

        public static FindCriteria11 FromFindCriteria(FindCriteria findCriteria)
        {
            if (findCriteria == null)
            {
                throw FxTrace.Exception.ArgumentNull("findCriteria");
            }

            return new FindCriteria11(findCriteria);
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaSet");
            }

            return SchemaUtility.EnsureProbeSchema(DiscoveryVersion.WSDiscovery11, schemaSet);                        
        }

        public FindCriteria ToFindCriteria()
        {
            return this.findCriteria;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        [Fx.Tag.InheritThrows(From = "ReadFrom", FromDeclaringType = typeof(FindCriteria))]
        public void ReadXml(XmlReader reader)
        {
            this.findCriteria.ReadFrom(DiscoveryVersion.WSDiscovery11, reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            this.findCriteria.WriteTo(DiscoveryVersion.WSDiscovery11, writer);
        }
    }
}
