// Web service test for WSDL document:
// http://localhost:8080/TestBinding4.asmx?wsdl

using System;
using NUnit.Framework;
using TestBinding4Tests.Soap;
using TestBinding4Tests.HttpGet;
using TestBinding4Tests.HttpPost;
using System.Xml;

namespace TestBinding4Tests
{
	[TestFixture]
	public class TestBinding4Test: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			TestBinding4Tests.Soap.GetData data = new TestBinding4Tests.Soap.GetData ();
			
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
			
			Assert ("RebuildTabList 1", data.RebuildTabList (true));
			Assert ("RebuildTabList 2", !data.RebuildTabList (false));
			
			Assert ("RebuildTabStruct 1", data.RebuildTabStruct ("a", true));
			Assert ("RebuildTabStruct 2", !data.RebuildTabStruct ("b", false));
		}
		
		void Check (string met, XmlNode[] nodes)
		{
			AssertNotNull (met + " #1", nodes);
			AssertEquals (met + " #2", 2, nodes.Length);
			AssertEquals (met + " #3", "<one someAtt=\"someValue\" xmlns=\"\" />", nodes[0].OuterXml);
			AssertEquals (met + " #4", "<two someAtt=\"someValue\" xmlns=\"\" />", nodes[1].OuterXml);
		}
	}
}
