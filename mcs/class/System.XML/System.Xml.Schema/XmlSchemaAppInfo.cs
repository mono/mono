// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAppInfo.
	/// </summary>
	public class XmlSchemaAppInfo : XmlSchemaObject
	{
		private XmlNode[] markup;
		private string source;

		public XmlSchemaAppInfo()
		{
		}

		[System.Xml.Serialization.XmlAttribute("source")]
		public string Source 
		{
			get{ return  source; } 
			set{ source = value; }
		}

		[XmlAnyElement]
		[XmlText]
		public XmlNode[] Markup 
		{
			get{ return  markup; }
			set{ markup = value; }
		}

		//<appinfo
		//  source = anyURI>
		//  Content: ({any})*
		//</appinfo>
		internal static XmlSchemaAppInfo Read(XmlSchemaReader reader, ValidationEventHandler h, out bool skip)
		{
			skip = false;
			XmlSchemaAppInfo appinfo = new XmlSchemaAppInfo();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != "appinfo")
			{
				error(h,"Should not happen :1: XmlSchemaAppInfo.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			appinfo.LineNumber = reader.LineNumber;
			appinfo.LinePosition = reader.LinePosition;
			appinfo.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "source")
				{
					appinfo.source = reader.Value;
				}
				else
				{
					error(h,reader.Name + " is not a valid attribute for appinfo",null);
				}
			}

			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return appinfo;

			//Content {any}*
			//FIXME: This is a pure Quick Hack; There must be a another method;
			XmlDocument xmldoc = new XmlDocument();
			xmldoc.AppendChild(xmldoc.ReadNode(reader));
			XmlNode root = xmldoc.FirstChild;
			if(root != null && root.ChildNodes != null)
			{
				appinfo.Markup = new XmlNode[root.ChildNodes.Count];
				for(int i=0;i<root.ChildNodes.Count;i++)
				{
					appinfo.Markup[i] = root.ChildNodes[i];
				}
			}
			if(reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement)
				skip = true;
			return appinfo;
		}
	}
}
