using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{

    [XmlSchemaProvider(null, IsAny = true)]
    public class TestElement : IXmlSerializable
    {
        public string Value { get; set; }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("dummy", Value);
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Value = reader.ReadElementString("dummy");
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }
    }


    [TestFixture]
    public class Bug11916Test
    {
        [Test]
        public void TestIsAnyTrueDataContractSerializer()
        {
            TestElement element = new TestElement();
            element.Value = "bar";

            StringBuilder stringBuilder = new StringBuilder ();

            DataContractSerializer ser = new DataContractSerializer (typeof (TestElement));

            using (var xw = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (new StringWriter (stringBuilder))))
            {
                ser.WriteObject(xw, element);
            }

            string actualXml   = stringBuilder.ToString ();
            string expectedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><dummy>bar</dummy>";

            Assert.AreEqual (expectedXml, actualXml, "#1 IsAny=true DataContractSerializer");
        }
    }
}
