// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaKey.
	/// </summary>
	public class XmlSchemaKey : XmlSchemaIdentityConstraint
	{
		private static string xmlname = "key";

		public XmlSchemaKey()
		{
		}
		/// <remarks>
		/// 1. name must be present
		/// 2. selector and field must be present
		/// </remarks>
		[MonoTODO]
		internal new int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			return base.Compile(h, schema);
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		internal new void error(ValidationEventHandler handle, string message)
		{
			errorCount++;
			ValidationHandler.RaiseValidationError(handle, this, message);
		}

		//<key 
		//  id = ID 
		//  name = NCName 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (selector, field+))
		//</key>
		internal static XmlSchemaKey Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaKey key = new XmlSchemaKey();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaKey.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			key.LineNumber = reader.LineNumber;
			key.LinePosition = reader.LinePosition;
			key.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					key.Id = reader.Value;
				}
				else if(reader.Name == "name")
				{
					key.Name = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for key",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,key);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return key;

			//  Content: annotation?, selector, field+
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaKey.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						key.Annotation = annotation;
					continue;
				}
				if(level <= 2 && reader.LocalName == "selector")
				{
					level = 3;
					XmlSchemaXPath selector = XmlSchemaXPath.Read(reader,h,"selector");
					if(selector != null)
						key.Selector = selector;
					continue;
				}
				if(level <= 3 && reader.LocalName == "field")
				{
					level = 3;
					if(key.Selector == null)
						error(h,"selector must be defined before field declarations",null);
					XmlSchemaXPath field = XmlSchemaXPath.Read(reader,h,"field");
					if(field != null)
						key.Fields.Add(field);
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return key;
		}
	}
}
