// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaDocumentation.
	/// </summary>
	public class XmlSchemaDocumentation : XmlSchemaObject
	{
		private string language;
		private XmlNode[] markup;
		private string source;

		public XmlSchemaDocumentation()
		{
		}

		[XmlAnyElement]
		[XmlText]
		public XmlNode[] Markup 
		{
			get{ return  markup; }
			set{ markup = value; }
		}
		
		[System.Xml.Serialization.XmlAttribute("source")]
		public string Source 
		{
			get{ return  source; } 
			set{ source = value; }
		}

		[System.Xml.Serialization.XmlAttribute("xml:lang")]
		public string Language 
		{
			get{ return  language; }
			set{ language = value; }
		}

		//<documentation
		//  source = anyURI
		//  xml:lang = language>
		//  Content: ({any})*
		//</documentation>
		internal static XmlSchemaDocumentation Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaDocumentation doc = new XmlSchemaDocumentation();

			reader.MoveToElement();
			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != "documentation")
			{
				error(h,"Should not happen :1: XmlSchemaDocumentation.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			doc.LineNumber = reader.LineNumber;
			doc.LinePosition = reader.LinePosition;
			doc.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "source")
				{
					doc.source = reader.Value;
				}
				else if(reader.Name == "xml:lang")
				{
					doc.language = reader.Value;
				}
				else
				{
					error(h,reader.Name + " is not a valid attribute for documentation",null);
				}
			}

			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return doc;

			//Content {any}*
			//FIXME: How to handle {any}* content
			while(reader.Read())
			{
				if(reader.NodeType == XmlNodeType.EndElement && reader.NamespaceURI == XmlSchema.Namespace && 
					reader.LocalName == "documentation")
					break;
			}

			return doc;
		}
	}
}
