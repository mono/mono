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
    public class ResolveCriteria11 : IXmlSerializable
    {
        ResolveCriteria resolveCriteria;

        ResolveCriteria11()
        {
            this.resolveCriteria = new ResolveCriteria();
        }

        ResolveCriteria11(ResolveCriteria resolveCriteria)
        {
            this.resolveCriteria = resolveCriteria;
        }

        public static ResolveCriteria11 FromResolveCriteria(ResolveCriteria resolveCriteria)
        {
            if (resolveCriteria == null)
            {
                throw FxTrace.Exception.ArgumentNull("resolveCriteria");
            }
            return new ResolveCriteria11(resolveCriteria);
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaSet");
            }

            return SchemaUtility.EnsureResolveSchema(DiscoveryVersion.WSDiscovery11, schemaSet);
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
            this.resolveCriteria.ReadFrom(DiscoveryVersion.WSDiscovery11, reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            this.resolveCriteria.WriteTo(DiscoveryVersion.WSDiscovery11, writer);
        }
    }
}
