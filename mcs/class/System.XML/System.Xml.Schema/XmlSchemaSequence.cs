// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSequence.
	/// </summary>
	public class XmlSchemaSequence : XmlSchemaGroupBase
	{
		private XmlSchemaObjectCollection items;
		private static string xmlname = "sequence";

		public XmlSchemaSequence()
		{
			items = new XmlSchemaObjectCollection();
		}

		[XmlElement("element",typeof(XmlSchemaElement),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("group",typeof(XmlSchemaGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("choice",typeof(XmlSchemaChoice),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("sequence",typeof(XmlSchemaSequence),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("any",typeof(XmlSchemaAny),Namespace="http://www.w3.org/2001/XMLSchema")]
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
					errorCount += ((XmlSchemaElement)obj).Compile(h,schema);
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
		//<sequence
		//  id = ID
		//  maxOccurs =  (nonNegativeInteger | unbounded)  : 1
		//  minOccurs = nonNegativeInteger : 1
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (element | group | choice | sequence | any)*)
		//</sequence>
		internal static XmlSchemaSequence Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaSequence sequence = new XmlSchemaSequence();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaSequence.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			sequence.LineNumber = reader.LineNumber;
			sequence.LinePosition = reader.LinePosition;
			sequence.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					sequence.Id = reader.Value;
				}
				else if(reader.Name == "maxOccurs")
				{
					try
					{
						sequence.MaxOccursString = reader.Value;
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
						sequence.MinOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for minOccurs",e);
					}
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for sequence",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,sequence);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return sequence;

			//  Content: (annotation?, (element | group | choice | sequence | any)*)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaSequence.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						sequence.Annotation = annotation;
					continue;
				}
				if(level <=2)
				{
					if(reader.LocalName == "element")
					{
						level = 2;
						XmlSchemaElement element = XmlSchemaElement.Read(reader,h);
						if(element != null)
							sequence.items.Add(element);
						continue;
					}
					if(reader.LocalName == "group")
					{
						level = 2;
						XmlSchemaGroupRef group = XmlSchemaGroupRef.Read(reader,h);
						if(group != null)
							sequence.items.Add(group);
						continue;
					}
					if(reader.LocalName == "choice")
					{
						level = 2;
						XmlSchemaChoice choice = XmlSchemaChoice.Read(reader,h);
						if(choice != null)
							sequence.items.Add(choice);
						continue;
					}
					if(reader.LocalName == "sequence")
					{
						level = 2;
						XmlSchemaSequence seq = XmlSchemaSequence.Read(reader,h);
						if(seq != null)
							sequence.items.Add(seq);
						continue;
					}
					if(reader.LocalName == "any")
					{
						level = 2;
						XmlSchemaAny any = XmlSchemaAny.Read(reader,h);
						if(any != null)
							sequence.items.Add(any);
						continue;
					}
				}
				reader.RaiseInvalidElementError();
			}
			return sequence;
		}
	}
}
