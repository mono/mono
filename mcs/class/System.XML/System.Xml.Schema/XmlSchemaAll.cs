// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAll.
	/// </summary>
	public class XmlSchemaAll : XmlSchemaGroupBase
	{
		private XmlSchemaObjectCollection items;
		private static string xmlname = "all";

		public XmlSchemaAll()
		{
			items = new XmlSchemaObjectCollection();
		}

		[XmlElement("element",typeof(XmlSchemaElement),Namespace=XmlSchema.Namespace)]
		public override XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}

		/// <remarks>
		/// 1. MaxOccurs must be one. (default is also one)
		/// 2. MinOccurs must be zero or one.
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			//FIXME: Should we reset the values on error??
			if(MaxOccurs != Decimal.One)
				error(h,"maxOccurs must be 1");
			if(MinOccurs != Decimal.One && MinOccurs != Decimal.Zero)
				error(h,"minOccurs must be 0 or 1");

			XmlSchemaUtil.CompileID(Id, this, schema.IDCollection, h);

			foreach(XmlSchemaObject obj in Items)
			{
				if(obj is XmlSchemaElement)
				{
					XmlSchemaElement elem = (XmlSchemaElement)obj;
					if(elem.MaxOccurs != Decimal.One && elem.MaxOccurs != Decimal.Zero)
					{
						elem.error(h,"The {max occurs} of all the elements of 'all' must be 0 or 1. ");
					}
					errorCount += elem.Compile(h, schema);
				}
				else
				{
					error(h,"XmlSchemaAll can only contain Items of type Element");
				}
			}

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}

		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}
		//<all
		//  id = ID
		//  maxOccurs = 1 : 1
		//  minOccurs = (0 | 1) : 1
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, element*)
		//</all>
		internal static XmlSchemaAll Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaAll all = new XmlSchemaAll();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaAll.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}
			
			all.LineNumber = reader.LineNumber;
			all.LinePosition = reader.LinePosition;
			all.SourceUri = reader.BaseURI;

			//Read Attributes
			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					all.Id = reader.Value;
				}
				else if(reader.Name == "maxOccurs")
				{
					try
					{
						all.MaxOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for maxOccurs",e);
					}
				}
				else if(reader.Name == "minOccurs")
				{
					try
					{
						all.MinOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for minOccurs",e);
					}
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for all",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,all);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return all;

			//Content: (annotation?, element*)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaAll.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						all.Annotation = annotation;
					continue;
				}
				if(level <=2 && reader.LocalName == "element")
				{
					level = 2;
					XmlSchemaElement element = XmlSchemaElement.Read(reader,h);
					if(element != null)
						all.items.Add(element);
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return all;
		}
	}
}
