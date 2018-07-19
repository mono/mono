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
    public class ResolveCriteriaApril2005 : IXmlSerializable
    {
        ResolveCriteria resolveCriteria;

        ResolveCriteriaApril2005()        
        {
            this.resolveCriteria = new ResolveCriteria();
        }

        ResolveCriteriaApril2005(ResolveCriteria resolveCriteria)
        {
            this.resolveCriteria = resolveCriteria;
        }

        public static ResolveCriteriaApril2005 FromResolveCriteria(ResolveCriteria resolveCriteria)
        {
            if (resolveCriteria == null)
            {
                throw FxTrace.Exception.ArgumentNull("resolveCriteria");
            }
            return new ResolveCriteriaApril2005(resolveCriteria);
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaSet");
            }

            return SchemaUtility.EnsureResolveSchema(DiscoveryVersion.WSDiscoveryApril2005, schemaSet);
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
            this.resolveCriteria.ReadFrom(DiscoveryVersion.WSDiscoveryApril2005, reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            this.resolveCriteria.WriteTo(DiscoveryVersion.WSDiscoveryApril2005, writer);
        }        
    }
}
