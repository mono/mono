// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// refers to the named group
	/// </summary>
	public class XmlSchemaGroup : XmlSchemaAnnotated
	{
		private string name;
		private XmlSchemaGroupBase particle;
		private XmlQualifiedName qualifiedName;
		
		private static string xmlname = "group";

		public XmlSchemaGroup()
		{
		}

		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return  name; } 
			set{ name = value; }
		}

		[XmlElement("all",typeof(XmlSchemaAll),Namespace=XmlSchema.Namespace)]
		[XmlElement("choice",typeof(XmlSchemaChoice),Namespace=XmlSchema.Namespace)]
		[XmlElement("sequence",typeof(XmlSchemaSequence),Namespace=XmlSchema.Namespace)]
		public XmlSchemaGroupBase Particle
		{
			get{ return  particle; }
			set{ particle = value; }
		}

		[XmlIgnore]
		internal XmlQualifiedName QualifiedName 
		{
			get{ return qualifiedName;}
		}

		// 1. name must be present
		// 2. MinOccurs & MaxOccurs of the Particle must be absent
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			if(Name == null)
				error(h,"Required attribute name must be present");
			else if(!XmlSchemaUtil.CheckNCName(this.name)) 
				error(h,"attribute name must be NCName");
			else
				qualifiedName = new XmlQualifiedName(Name,schema.TargetNamespace);

			if(Particle == null)
			{
				error(h,"Particle is required");
			}
			else 
			{
				if(Particle.MaxOccursString != null)
					Particle.error(h,"MaxOccurs must not be present when the Particle is a child of Group");
				if(Particle.MinOccursString != null)
					Particle.error(h,"MinOccurs must not be present when the Particle is a child of Group");
			
				if(Particle is XmlSchemaChoice)
				{
					errorCount += ((XmlSchemaChoice)Particle).Compile(h,schema);
				}
				else if(Particle is XmlSchemaSequence)
				{
					errorCount += ((XmlSchemaSequence)Particle).Compile(h,schema);
				}
				else if(Particle is XmlSchemaAll)
				{
					errorCount += ((XmlSchemaAll)Particle).Compile(h,schema);
				}
				else
				{
					error(h,"only all,choice or sequence are allowed");
				}
			}
			
			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		//From the Errata
		//<group 
		//  id = ID
		//  name = NCName
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (all | choice | sequence)?)
		//</group>
		internal static XmlSchemaGroup Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaGroup group = new XmlSchemaGroup();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaGroup.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			group.LineNumber = reader.LineNumber;
			group.LinePosition = reader.LinePosition;
			group.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					group.Id = reader.Value;
				}
				else if(reader.Name == "name")
				{
					group.name = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for group",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,group);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return group;

//			 Content: (annotation?, (all | choice | sequence)?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaGroup.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						group.Annotation = annotation;
					continue;
				}
				if(level <= 2)
				{
					if(reader.LocalName == "all")
					{
						level = 3;
						XmlSchemaAll all = XmlSchemaAll.Read(reader,h);
						if(all != null)
							group.Particle = all;
						continue;
					}
					if(reader.LocalName == "choice")
					{
						level = 3;
						XmlSchemaChoice choice = XmlSchemaChoice.Read(reader,h);
						if(choice != null)
							group.Particle = choice;
						continue;
					}
					if(reader.LocalName == "sequence")
					{
						level = 3;
						XmlSchemaSequence sequence = XmlSchemaSequence.Read(reader,h);
						if(sequence != null)
							group.Particle = sequence;
						continue;
					}
				}
				reader.RaiseInvalidElementError();
			}
			return group;
		}
	}
}
