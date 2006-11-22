// Web service test for WSDL document:
// http://localhost:8080/TestBinding4.asmx?wsdl

using System;
using NUnit.Framework;
using TestBinding4Tests.Soap;
using System.Xml;

namespace Localhost.TestBinding4Tests
{
	[TestFixture]
	public class TestBinding4Test: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			GetData data = new GetData ();
			
			XmlDocument doc = new XmlDocument ();
			XmlElement elem1 = doc.CreateElement ("one");
			elem1.SetAttribute ("someAtt","someValue");
			XmlElement elem2 = doc.CreateElement ("two");
			elem2.SetAttribute ("someAtt","someValue");
			XmlNode[] nodes = new XmlNode[] {elem1,elem2};
			
			nodes = data.GetTabList (nodes);
			Check ("GetTabList", nodes);
			
			nodes = data.GetTabStruct ("hello", nodes);
			Check ("GetTabStruct", nodes);
			
			Assert.IsTrue (data.RebuildTabList (true), "RebuildTabList 1");
			Assert.IsTrue (!data.RebuildTabList (false), "RebuildTabList 2");
			
			Assert.IsTrue (data.RebuildTabStruct ("a", true), "RebuildTabStruct 1");
			Assert.IsTrue (!data.RebuildTabStruct ("b", false), "RebuildTabStruct 2");
		}
		
		void Check (string met, XmlNode[] nodes)
		{
			Assert.IsNotNull (nodes, met + " #1");
			Assert.AreEqual (2, nodes.Length, met + " #2");
			Assert.AreEqual ("<one someAtt=\"someValue\" xmlns=\"\" />", nodes[0].OuterXml, met + " #3");
			Assert.AreEqual ("<two someAtt=\"someValue\" xmlns=\"\" />", nodes[1].OuterXml, met + " #4");
		}
	}
}
