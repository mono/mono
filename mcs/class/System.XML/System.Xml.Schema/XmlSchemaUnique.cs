// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaUnique.
	/// </summary>
	public class XmlSchemaUnique : XmlSchemaIdentityConstraint
	{
		private static string xmlname = "unique";

		public XmlSchemaUnique()
		{
		}
		/// <remarks>
		/// 1. name must be present
		/// 2. selector and field must be present
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			return base.Compile(h,schema);
		}
		
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			return errorCount;
		}

		/*
		internal new void error(ValidationEventHandler handle, string message)
		{
			errorCount++;
			ValidationHandler.RaiseValidationError(handle, this, message);
		}
		*/
		//<unique 
		//  id = ID 
		//  name = NCName 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (selector, field+))
		//</unique>
		internal static XmlSchemaUnique Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaUnique unique = new XmlSchemaUnique();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaUnique.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			unique.LineNumber = reader.LineNumber;
			unique.LinePosition = reader.LinePosition;
			unique.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					unique.Id = reader.Value;
				}
				else if(reader.Name == "name")
				{
					unique.Name = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for unique",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,unique);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return unique;

			//  Content: annotation?, selector, field+
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaUnion.Read, name="+reader.Name,null);
					break;
				}
				if(reader.LocalName == "annotation" && unique.Annotation == null )//Only one annotation
				{
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						unique.Annotation = annotation;
					continue;
				}
				if( reader.LocalName == "selector")
				{
					XmlSchemaXPath selector = XmlSchemaXPath.Read(reader,h,"selector");
					if(selector != null)
						unique.Selector = selector;
					continue;
				}
				if(reader.LocalName == "field")
				{	
					XmlSchemaXPath field = XmlSchemaXPath.Read(reader,h,"field");
					if(field != null)
						unique.Fields.Add(field);
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return unique;
		}
	}
}
