// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaComplexContent.
	/// </summary>
	public class XmlSchemaComplexContent : XmlSchemaContentModel
	{
		private XmlSchemaContent content;
		private bool isMixed;
		private static string xmlname = "complexContent";

		public XmlSchemaComplexContent()
		{}

		[System.Xml.Serialization.XmlAttribute("mixed")]
		public bool IsMixed 
		{
			get{ return  isMixed; } 
			set{ isMixed = value; }
		}

		[XmlElement("restriction",typeof(XmlSchemaComplexContentRestriction),Namespace=XmlSchema.Namespace)]
		[XmlElement("extension",typeof(XmlSchemaComplexContentExtension),Namespace=XmlSchema.Namespace)]
		public override XmlSchemaContent Content 
		{
			get{ return  content; } 
			set{ content = value; }
		}

		/// <remarks>
		/// 1. Content must be present
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			if(Content == null)
			{
				error(h, "Content must be present in a complexContent");
			}
			else
			{
				if(Content is XmlSchemaComplexContentRestriction)
				{
					XmlSchemaComplexContentRestriction xscr = (XmlSchemaComplexContentRestriction) Content;
					errorCount += xscr.Compile(h, schema);
				}
				else if(Content is XmlSchemaComplexContentExtension)
				{
					XmlSchemaComplexContentExtension xsce = (XmlSchemaComplexContentExtension) Content;
					errorCount += xsce.Compile(h, schema);
				}
				else
					error(h,"complexContent can't have any value other than restriction or extention");
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
		//<complexContent
		//  id = ID
		//  mixed = boolean
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (restriction | extension))
		//</complexContent>
		internal static XmlSchemaComplexContent Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaComplexContent complex = new XmlSchemaComplexContent();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaComplexContent.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			complex.LineNumber = reader.LineNumber;
			complex.LinePosition = reader.LinePosition;
			complex.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					complex.Id = reader.Value;
				}
				else if(reader.Name == "mixed")
				{
					Exception innerex;
					complex.isMixed = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h,reader.Value + " is an invalid value for mixed",innerex);
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for complexContent",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,complex);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return complex;
			//Content: (annotation?, (restriction | extension))
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaComplexContent.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						complex.Annotation = annotation;
					continue;
				}
				if(level <=2)
				{
					if(reader.LocalName == "restriction")
					{
						level = 3;
						XmlSchemaComplexContentRestriction restriction = XmlSchemaComplexContentRestriction.Read(reader,h);
						if(restriction != null)
							complex.content = restriction;
						continue;
					}
					if(reader.LocalName == "extension")
					{
						level = 3;
						XmlSchemaComplexContentExtension extension = XmlSchemaComplexContentExtension.Read(reader,h);
						if(extension != null)
							complex.content = extension;
						continue;
					}
				}
				reader.RaiseInvalidElementError();
			}
			return complex;
		}
	}
}
