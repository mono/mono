using System;
using System.Xml;
using NUnit.Framework;
using Microsoft.Web.Services.Xml;
using Microsoft.Web.Services.Addressing;

namespace Microsoft.Web.Services.Addressing.Tests
{
	[TestFixture]
	public class EndpointReferenceTests
	{
		
		[Test]
		public void CreateEndpointReferenceFromAddress ()
		{
			EndpointReference e = new EndpointReference( new Address ( new Uri ("soap.tcp://127.0.0.1") ) );
			
			Assert.IsNotNull (e.Address.Value);
		}
		
		[Test]
		public void CreateEndpointReferenceFromUri ()
		{
			EndpointReference e = new EndpointReference( new Uri ("soap.tcp://127.0.0.1") );
			
			Assert.IsNotNull (e.Address.Value);
		}
		
		[Test]
		public void MinimalXmlToEndpointReference ()
		{
			XmlDocument doc = new XmlDocument ();
			XmlElement element = doc.CreateElement ("wsa", "EndpointReference", "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			
			Address a = new Address ( new Uri ("soap.tcp://127.0.0.1") );
			
			element.AppendChild (a.GetXml (doc));
			
			EndpointReference e = new EndpointReference (element);
		}
		
		[Test]
		public void EndpointReferenceToMinimalXml ()
		{
			EndpointReference e = new EndpointReference ( new Uri ("soap.tcp://127.0.0.1/") );
			XmlElement e2 = e.GetXml (new XmlDocument ());
			
			Assert.IsTrue (e2.FirstChild.InnerText == "soap.tcp://127.0.0.1/");
		}
		
		[Test]
		public void EndpointReferenceToXmlWithPortType ()
		{
			EndpointReference e = new EndpointReference ( new Uri ("soap.tcp://127.0.0.1/") );
			e.PortType = new PortType (new QualifiedName ("a","s","http://schemas.xmlsoap.org/ws/2003/03/addressing"));
			
			XmlElement element = e.GetXml (new XmlDocument ());
			
			Assert.IsTrue (element.FirstChild.NextSibling.InnerText == "a:s");
		}
		
		[Test]
		public void EndpointReferenceToXmlWithProperties ()
		{
			EndpointReference e = new EndpointReference (new Uri ("soap.tcp://127.0.0.1/") );
			e.ReferenceProperties = new ReferenceProperties ();
			
			XmlElement element = e.GetXml (new XmlDocument ());
			
			Assert.IsTrue (element.FirstChild.NextSibling.LocalName == "ReferenceProperties");
		}
		
		[Test]
		public void EndpointReferenceToXmlWithServiceName ()
		{
			EndpointReference e = new EndpointReference (new Uri ("soap.tcp://127.0.0.1/") );
			e.ServiceName = new ServiceName( new QualifiedName ("a", "s", "http://schemas.xmlsoap.org/ws/2003/03/addressing"));
			e.ServiceName.PortName = "test";
			
			XmlElement element = e.GetXml (new XmlDocument ());
			
			Assert.IsTrue (element.FirstChild.NextSibling.Attributes["wsa:PortName"].InnerText == "test");
		}
		
		[Test]
		public void RoundTripFromXml ()
		{
			XmlDocument doc = new XmlDocument ();
			XmlElement element = doc.CreateElement ("wsa", "EndpointReference", "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			
			Address a = new Address ( new Uri ("soap.tcp://127.0.0.1") );
			
			element.AppendChild (a.GetXml (doc));
			
			PortType p = new PortType (new QualifiedName ("wsa", "test", "http://schemas.xmlsoap.org/ws/2003/03/addressing"));
			
			element.AppendChild (p.GetXml (doc));
			
			EndpointReference e = new EndpointReference (element);
			
			XmlElement e2 = e.GetXml (new XmlDocument ());
			
			Assert.IsNotNull (element);
			Assert.IsNotNull (e2);
			
			Assert.IsTrue (element.OuterXml == e2.OuterXml);
		}
		
		[Test]
		public void RoundTripFromEndpointReference ()
		{
			EndpointReference e = new EndpointReference ( new Uri ("soap.tcp://127.0.0.1/") );
			XmlElement e2 = e.GetXml (new XmlDocument ());
			
			EndpointReference er = new EndpointReference (e2);
			
			Assert.IsTrue (e.Address.Value.AbsoluteUri == er.Address.Value.AbsoluteUri);
		}
		
		//Not going to bother testing the implicit operators, they work fine.
		
	}
}
