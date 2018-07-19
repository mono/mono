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
    public class FindCriteriaCD1 : IXmlSerializable
    {
        FindCriteria findCriteria;

        FindCriteriaCD1()
        {
            this.findCriteria = new FindCriteria();
        }

        FindCriteriaCD1(FindCriteria findCriteria)
        {
            this.findCriteria = findCriteria;
        }

        public static FindCriteriaCD1 FromFindCriteria(FindCriteria findCriteria)
        {
            if (findCriteria == null)
            {
                throw FxTrace.Exception.ArgumentNull("findCriteria");
            }

            return new FindCriteriaCD1(findCriteria);
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaSet");
            }

            return SchemaUtility.EnsureProbeSchema(DiscoveryVersion.WSDiscoveryCD1, schemaSet);
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
            this.findCriteria.ReadFrom(DiscoveryVersion.WSDiscoveryCD1, reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            this.findCriteria.WriteTo(DiscoveryVersion.WSDiscoveryCD1, writer);
        }
    }
}
