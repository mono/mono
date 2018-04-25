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
    public class ResolveCriteriaCD1 : IXmlSerializable
    {
        ResolveCriteria resolveCriteria;

        ResolveCriteriaCD1()
        {
            this.resolveCriteria = new ResolveCriteria();
        }

        ResolveCriteriaCD1(ResolveCriteria resolveCriteria)
        {
            this.resolveCriteria = resolveCriteria;
        }

        public static ResolveCriteriaCD1 FromResolveCriteria(ResolveCriteria resolveCriteria)
        {
            if (resolveCriteria == null)
            {
                throw FxTrace.Exception.ArgumentNull("resolveCriteria");
            }
            return new ResolveCriteriaCD1(resolveCriteria);
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaSet");
            }

            return SchemaUtility.EnsureResolveSchema(DiscoveryVersion.WSDiscoveryCD1, schemaSet);
        }

        public ResolveCriteria ToResolveCriteria()
        {
            return this.resolveCriteria;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        [Fx.Tag.InheritThrows(From = "ReadFrom", FromDeclaringType = typeof(ResolveCriteria))]
        public void ReadXml(XmlReader reader)
        {
            this.resolveCriteria.ReadFrom(DiscoveryVersion.WSDiscoveryCD1, reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            this.resolveCriteria.WriteTo(DiscoveryVersion.WSDiscoveryCD1, writer);
        }
    }
}
