//
// Microsoft.Web.Services.Addressing.Action.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman
//

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public class Action : AttributedUriString, IXmlElement
	{


		public Action (string uri) : base (uri)
		{
		}

		public Action (XmlElement element) : base ()
		{
			LoadXml (element);
		}

		public static implicit operator Action(string obj)
		{

			return new Action (obj);
		
		}

		public static implicit operator string(Action obj)
		{

			if(obj == null) {
				return null;
			}
			return obj.Value;
		
		}

		public XmlElement GetXml (XmlDocument document)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}

			XmlElement element = document.CreateElement ("wsa",
			                                             "Action",
					                             "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			
			GetXmlUri (document, element);
			
			return element;
			
		}

		public void LoadXml (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			if(element.LocalName != "Action" || element.NamespaceURI != "http://schemas.xmlsoap.org/ws/2003/03/addressing") {
				throw new ArgumentException ("Invalid Element Supplied");
			}

			LoadXmlUri (element);
		}		

	}
}
