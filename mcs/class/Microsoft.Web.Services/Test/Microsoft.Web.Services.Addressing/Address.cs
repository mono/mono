using System;
using System.Xml;
using NUnit.Framework;
using Microsoft.Web.Services.Addressing;

namespace Microsoft.Web.Services.Addressing.Tests
{
	
	[TestFixture]
	public class AddressTests
	{
		
		[Test]
		public void CreateAddress ()
		{
			Address a = new Address (new Uri("soap.tcp://127.0.0.1/"));
			Assert.IsNotNull (a);
		}
		
		[Test]
		public void AddressToXml ()
		{
			Address a = new Address (new Uri("soap.tcp://127.0.0.1/"));
			XmlElement e = a.GetXml (new XmlDocument ());
			
			Assert.IsTrue (e.OuterXml.Length != 0);
		}
		
		[Test]
		public void XmlToAddress ()
		{
			XmlDocument doc = new XmlDocument ();
			XmlElement e = doc.CreateElement ("wsa", "Address", "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			
			e.InnerText = "soap.tcp://127.0.0.1/";
			
			Address a = new Address (e);
			
			Assert.IsTrue (a.Value.AbsoluteUri == e.InnerText);
		}
		
		[Test]
		public void RoundTripFromAddress ()
		{
			Address a = new Address (new Uri("soap.tcp://127.0.0.1"));
			
			XmlElement e = a.GetXml (new XmlDocument ());
			
			Address b = new Address (e);
			
			Assert.IsTrue (a.Value.AbsoluteUri == b.Value.AbsoluteUri);
		}
		
		[Test]
		public void RoundTripFromXml ()
		{
			XmlDocument doc = new XmlDocument ();
			XmlElement e = doc.CreateElement ("wsa", "Address", "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			
			e.InnerText = "soap.tcp://127.0.0.1/";
			
			Address a = new Address (e);
			
			XmlElement e2 = a.GetXml (new XmlDocument ());
			
			Assert.IsTrue (e.OuterXml == e2.OuterXml);
		}
		
		[Test]
		public void ImplicitUri ()
		{
			Uri u = new Uri ("soap.tcp://127.0.0.1/");
			
			Address a = new Address(u);
			
			Assert.IsTrue (u.AbsoluteUri == ((Uri)a).AbsoluteUri);
		}
		
		[Test]
		public void ImplicitAddress ()
		{
			Uri u = new Uri ("soap.tcp://127.0.0.1");
			
			Address a = new Address (u);
			
			Assert.AreEqual (u, (Uri)a);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void InvalidArgumentException()
		{
			XmlDocument document = new XmlDocument ();
			XmlElement e = document.CreateElement ("b", "a", "d");
			
			Address a = new Address (e);
		}
		
	}
	
}
