// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAll.
	/// </summary>
	public class XmlSchemaChoice : XmlSchemaGroupBase
	{
		private XmlSchemaObjectCollection items;
		private static string xmlname = "choice";

		public XmlSchemaChoice()
		{
			items = new XmlSchemaObjectCollection();
		}

		[XmlElement("element",typeof(XmlSchemaElement),Namespace=XmlSchema.Namespace)]
		[XmlElement("group",typeof(XmlSchemaGroupRef),Namespace=XmlSchema.Namespace)]
		[XmlElement("choice",typeof(XmlSchemaChoice),Namespace=XmlSchema.Namespace)]
		[XmlElement("sequence",typeof(XmlSchemaSequence),Namespace=XmlSchema.Namespace)]
		[XmlElement("any",typeof(XmlSchemaAny),Namespace=XmlSchema.Namespace)]
		public override XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}

		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			//FIXME: Should we reset the values
			if(MinOccurs > MaxOccurs)
				error(h,"minOccurs must be less than or equal to maxOccurs");

			XmlSchemaUtil.CompileID(Id, this, schema.IDCollection, h);

			foreach(XmlSchemaObject obj in Items)
			{
				if(obj is XmlSchemaElement)
				{
					errorCount += ((XmlSchemaElement)obj).Compile(h, schema);
				}
				else if(obj is XmlSchemaGroupRef)
				{
					errorCount += ((XmlSchemaGroupRef)obj).Compile(h,schema);
				}
				else if(obj is XmlSchemaChoice)
				{
					errorCount += ((XmlSchemaChoice)obj).Compile(h,schema);
				}
				else if(obj is XmlSchemaSequence)
				{
					errorCount += ((XmlSchemaSequence)obj).Compile(h,schema);
				}
				else if(obj is XmlSchemaAny)
				{
					errorCount += ((XmlSchemaAny)obj).Compile(h,schema);
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
		//<choice
		//  id = ID
		//  maxOccurs =  (nonNegativeInteger | unbounded)  : 1
		//  minOccurs = nonNegativeInteger : 1
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (element | group | choice | sequence | any)*)
		//</choice>
		internal static XmlSchemaChoice Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaChoice choice = new XmlSchemaChoice();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaChoice.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			choice.LineNumber = reader.LineNumber;
			choice.LinePosition = reader.LinePosition;
			choice.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					choice.Id = reader.Value;
				}
				else if(reader.Name == "maxOccurs")
				{
					try
					{
						choice.MaxOccursString = reader.Value;
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
						choice.MinOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for minOccurs",e);
					}
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for choice",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,choice);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return choice;

			//  Content: (annotation?, (element | group | choice | sequence | any)*)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaChoice.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						choice.Annotation = annotation;
					continue;
				}
				if(level <=2)
				{
					if(reader.LocalName == "element")
					{
						level = 2;
						XmlSchemaElement element = XmlSchemaElement.Read(reader,h);
						if(element != null)
							choice.items.Add(element);
						continue;
					}
					if(reader.LocalName == "group")
					{
						level = 2;
						XmlSchemaGroupRef group = XmlSchemaGroupRef.Read(reader,h);
						if(group != null)
							choice.items.Add(group);
						continue;
					}
					if(reader.LocalName == "choice")
					{
						level = 2;
						XmlSchemaChoice ch = XmlSchemaChoice.Read(reader,h);
						if(ch != null)
							choice.items.Add(ch);
						continue;
					}
					if(reader.LocalName == "sequence")
					{
						level = 2;
						XmlSchemaSequence sequence = XmlSchemaSequence.Read(reader,h);
						if(sequence != null)
							choice.items.Add(sequence);
						continue;
					}
					if(reader.LocalName == "any")
					{
						level = 2;
						XmlSchemaAny any = XmlSchemaAny.Read(reader,h);
						if(any != null)
							choice.items.Add(any);
						continue;
					}
				}
				reader.RaiseInvalidElementError();
			}
			return choice;
		}
	}
}
