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
    public class FindCriteriaApril2005 : IXmlSerializable
    {
        FindCriteria findCriteria;

        FindCriteriaApril2005()
        {
            this.findCriteria = new FindCriteria();
        }

        FindCriteriaApril2005(FindCriteria findCriteria)
        {
            this.findCriteria = findCriteria;
        }

        public static FindCriteriaApril2005 FromFindCriteria(FindCriteria findCriteria)
        {
            if (findCriteria == null)
            {
                throw FxTrace.Exception.ArgumentNull("findCriteria");
            }

            return new FindCriteriaApril2005(findCriteria);
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaSet");
            }

            return SchemaUtility.EnsureProbeSchema(DiscoveryVersion.WSDiscoveryApril2005, schemaSet);            
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
            this.findCriteria.ReadFrom(DiscoveryVersion.WSDiscoveryApril2005, reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            this.findCriteria.WriteTo(DiscoveryVersion.WSDiscoveryApril2005, writer);
        }        
    }
}
