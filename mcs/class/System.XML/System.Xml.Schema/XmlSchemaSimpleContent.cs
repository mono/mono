// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleContent.
	/// </summary>
	public class XmlSchemaSimpleContent : XmlSchemaContentModel
	{
		private XmlSchemaContent content;
		private static string xmlname = "simpleContent";
		public XmlSchemaSimpleContent()
		{
		}

		[XmlElement("restriction",typeof(XmlSchemaSimpleContentRestriction),Namespace=XmlSchema.Namespace)]
		[XmlElement("extension",typeof(XmlSchemaSimpleContentExtension),Namespace=XmlSchema.Namespace)]
		public override XmlSchemaContent Content 
		{
			get{ return  content; } 
			set{ content = value; }
		}

		///<remarks>
		/// 1. Content must be present and one of restriction or extention
		///</remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			if(Content == null)
			{
				error(h, "Content must be present in a simpleContent");
			}
			else
			{
				if(Content is XmlSchemaSimpleContentRestriction)
				{
					XmlSchemaSimpleContentRestriction xscr = (XmlSchemaSimpleContentRestriction) Content;
					errorCount += xscr.Compile(h, schema);
				}
				else if(Content is XmlSchemaSimpleContentExtension)
				{
					XmlSchemaSimpleContentExtension xsce = (XmlSchemaSimpleContentExtension) Content;
					errorCount += xsce.Compile(h, schema);
				}
				else
					error(h,"simpleContent can't have any value other than restriction or extention");
			}
			
			XmlSchemaUtil.CompileID(Id,this, schema.IDCollection,h);
			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}
		//<simpleContent 
		//  id = ID 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (restriction | extension))
		//</simpleContent>
		internal static XmlSchemaSimpleContent Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaSimpleContent simple = new XmlSchemaSimpleContent();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaComplexContent.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			simple.LineNumber = reader.LineNumber;
			simple.LinePosition = reader.LinePosition;
			simple.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					simple.Id = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for simpleContent",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,simple);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return simple;
			//Content: (annotation?, (restriction | extension))
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaSimpleContent.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						simple.Annotation = annotation;
					continue;
				}
				if(level <=2)
				{
					if(reader.LocalName == "restriction")
					{
						level = 3;
						XmlSchemaSimpleContentRestriction restriction = XmlSchemaSimpleContentRestriction.Read(reader,h);
						if(restriction != null)
							simple.content = restriction;
						continue;
					}
					if(reader.LocalName == "extension")
					{
						level = 3;
						XmlSchemaSimpleContentExtension extension = XmlSchemaSimpleContentExtension.Read(reader,h);
						if(extension != null)
							simple.content = extension;
						continue;
					}
				}
				reader.RaiseInvalidElementError();
			}
			return simple;
		}

	}
}
