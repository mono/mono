// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaXPath.
	/// </summary>
	public class XmlSchemaXPath : XmlSchemaAnnotated
	{
		private string xpath;

		public XmlSchemaXPath()
		{
		}
		[DefaultValue(null)]
		[System.Xml.Serialization.XmlAttribute("xpath")]
		public string XPath 
		{
			get{ return  xpath; } 
			set{ xpath = value; }
		}

		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			XmlSchemaUtil.CompileID(Id, this, schema.IDCollection, h);
			this.CompilationId = schema.CompilationId;
			return errorCount;
		}

		//<selector 
		//  id = ID 
		//  xpath = a subset of XPath expression, see below 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</selector>
		internal static XmlSchemaXPath Read(XmlSchemaReader reader, ValidationEventHandler h,string name)
		{
			XmlSchemaXPath path = new XmlSchemaXPath();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != name)
			{
				error(h,"Should not happen :1: XmlSchemaComplexContentRestriction.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			path.LineNumber = reader.LineNumber;
			path.LinePosition = reader.LinePosition;
			path.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					path.Id = reader.Value;
				}
				else if(reader.Name == "xpath")
				{
					path.xpath = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+name,null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,path);
				}
			}

			reader.MoveToElement();	
			if(reader.IsEmptyElement)
				return path;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != name)
						error(h,"Should not happen :2: XmlSchemaXPath.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						path.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return path;
		}

	}
}