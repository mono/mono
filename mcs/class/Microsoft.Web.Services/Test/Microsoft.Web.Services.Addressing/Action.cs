using System;
using NUnit.Framework;
using System.Xml;
using Microsoft.Web.Services;

namespace Microsoft.Web.Services.Addressing.Tests
{
	
	[TestFixture]
	public class ActionTest
	{
		
		[Test]
		public void CreateAction ()
		{
			Action a = new Action("urn:action:test");
			Assert.IsTrue (a.Value == "urn:action:test");
		}
		
		[Test]
		public void ActionToXml ()
		{
			Action a = new Action("urn:action:test");
			XmlElement element = a.GetXml(new XmlDocument());
			Assert.IsTrue (element.OuterXml.Length != 0);
		}
		
		[Test]
		public void XmlToAction ()
		{
			XmlDocument document = new XmlDocument ();
			
			XmlElement element = document.CreateElement("wsa", "Action", "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			element.InnerText = "urn:action:test";
			
			Action a = new Action (element);
			
			Assert.IsTrue (a.Value == "urn:action:test");
		}
		
		[Test]
		public void RoundTripFromAction ()
		{
			Action a = new Action ("urn:action:test");
			XmlElement element = a.GetXml(new XmlDocument());
			
			Action b = new Action (element);
			
			Assert.IsTrue (b.Value == "urn:action:test");
		}
		
		[Test]
		public void RoundTripFromXml ()
		{
			XmlDocument document = new XmlDocument ();
			
			XmlElement element = document.CreateElement("wsa", "Action", "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			element.InnerText = "urn:action:test";
			
			Action a = new Action (element);
			
			XmlElement element2 = a.GetXml(new XmlDocument ());
			
			Assert.IsTrue (element.OuterXml == element2.OuterXml);
			
		}
		
		[Test]
		public void ImplicitString ()
		{
			Action a = new Action ("urn:action:test");
			
			Assert.IsTrue ("urn:action:test" == a);
		}
		
		[Test]
		public void ImplicitAction ()
		{
			Action a = "urn:action:test";
			
			Assert.IsTrue ("urn:action:test" == a);
			
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void InvalidElementExceptionTest ()
		{
			XmlDocument doc = new XmlDocument ();
			
			XmlElement el = doc.CreateElement("b", "a", "d");
			
			Action a = new Action (el);
		}
		
	}
	
	
}
