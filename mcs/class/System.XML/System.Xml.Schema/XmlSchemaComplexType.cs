// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaComplexType.
	/// </summary>
	public class XmlSchemaComplexType : XmlSchemaType
	{
		private XmlSchemaAnyAttribute anyAttribute;
		private XmlSchemaObjectCollection attributes;
		private XmlSchemaObjectTable attributeUses;
		private XmlSchemaAnyAttribute attributeWildcard;
		private XmlSchemaDerivationMethod block;
		private XmlSchemaDerivationMethod blockResolved;
		private XmlSchemaContentModel contentModel;
		private XmlSchemaContentType contentType;
		private XmlSchemaParticle contentTypeParticle;
		private bool isAbstract;
		private bool isMixed;
		private XmlSchemaParticle particle;
		
		internal bool istoplevel = false;
		private static string xmlname = "complexType";

		public XmlSchemaComplexType()
		{
			attributes = new XmlSchemaObjectCollection();
			block = XmlSchemaDerivationMethod.None;
			attributeUses = new XmlSchemaObjectTable();
		}

		#region Attributes

		[DefaultValue(XmlSchemaDerivationMethod.None)]
		[System.Xml.Serialization.XmlAttribute("block")]
		public XmlSchemaDerivationMethod Block
		{
			get{ return  block; }
			set{ block = value; }
		}
		[DefaultValue(false)]
		[System.Xml.Serialization.XmlAttribute("abstract")]
		public bool IsAbstract 
		{
			get{ return  isAbstract; }
			set{ isAbstract = value; }
		}
		[DefaultValue(false)]
		[System.Xml.Serialization.XmlAttribute("mixed")]
		public override bool IsMixed
		{
			get{ return  isMixed; }
			set{ isMixed = value; }
		}
		
		#endregion
		
		#region Elements
		[XmlElement("anyAttribute",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaAnyAttribute AnyAttribute 
		{
			get{ return  anyAttribute; }
			set{ anyAttribute = value; }
		}

		[XmlElement("attribute",typeof(XmlSchemaAttribute),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Attributes 
		{
			get{ return attributes; }
		}
		
		[XmlElement("simpleContent",typeof(XmlSchemaSimpleContent),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("complexContent",typeof(XmlSchemaComplexContent),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaContentModel ContentModel 
		{
			get{ return  contentModel; } 
			set{ contentModel = value; }
		}

		//LAMESPEC: The default value for particle in Schema is of Type EmptyParticle (internal?)
		[XmlElement("group",typeof(XmlSchemaGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("all",typeof(XmlSchemaAll),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("choice",typeof(XmlSchemaChoice),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("sequence",typeof(XmlSchemaSequence),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaParticle Particle 
		{
			get{ return  particle; } 
			set{ particle = value; }
		}

		#endregion

		#region XmlIgnore
		[XmlIgnore]
		public XmlSchemaObjectTable AttributeUses 
		{
			get{ return attributeUses; }
		}
		[XmlIgnore]
		public XmlSchemaAnyAttribute AttributeWildcard 
		{
			get{ return attributeWildcard; }
		}
		[XmlIgnore]
		public XmlSchemaDerivationMethod BlockResolved 
		{
			get{ return blockResolved; }
		}
		[XmlIgnore]
		public XmlSchemaContentType ContentType 
		{
			get{ return contentType; }
		}
		[XmlIgnore]
		public XmlSchemaParticle ContentTypeParticle 
		{
			get{ return contentTypeParticle; }
		}
		#endregion

		/// <remarks>
		/// 1. If ContentModel is present, neither particle nor Attributes nor AnyAttribute can be present.
		/// 2. If particle is present, 
		/// a. For a topLevelComplexType
		///		1. name must be present and type NCName
		///		2. if block is #all, blockdefault is #all, else List of (extension | restriction)
		///		3. if final is #all, finaldefault is #all, else List of (extension | restriction)
		///	b. For a local Complex type 
		///		1. abstract must be false
		///		2. Name must be absent
		///		3. final must be absent
		///		4. block must be absent
		///		
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			if(istoplevel)
			{
				if(this.Name == null || this.Name == string.Empty)
					error(h,"name must be present in a top level complex type");
				else if(!XmlSchemaUtil.CheckNCName(Name))
					error(h,"name must be a NCName");
				else
					this.qName = new XmlQualifiedName(Name,info.TargetNamespace);
				
				if(Block != XmlSchemaDerivationMethod.None)
				{
					if(Block == XmlSchemaDerivationMethod.All)
					{
						blockResolved = XmlSchemaDerivationMethod.All;
					}
					else
					{
						//TODO: Check what all is not allowed
						blockResolved = Block & (XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Restriction);
					}
				}
				else
				{
					if(info.BlockDefault == XmlSchemaDerivationMethod.All)
					{
						blockResolved = XmlSchemaDerivationMethod.All;
					}
					else
						blockResolved = info.BlockDefault & (XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Restriction);
				}
				if(Final != XmlSchemaDerivationMethod.None)
				{
					if(Final == XmlSchemaDerivationMethod.All)
					{
						finalResolved = XmlSchemaDerivationMethod.All;
					}
					else
					{
						//TODO: Check what all is not allowed
						finalResolved = Final & (XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Restriction);
					}
				}
				else
				{
					if(info.FinalDefault == XmlSchemaDerivationMethod.All)
					{
						finalResolved = XmlSchemaDerivationMethod.All;
					}
					else
					{
						finalResolved = info.BlockDefault & (XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Restriction);
					}
				}
			}
			else // Not Top Level
			{
				if(isAbstract)
					error(h,"abstract must be false in a local complex type");
				if(Name != null)
					error(h,"name must be absent in a local complex type");
				if(Final != XmlSchemaDerivationMethod.None)
					error(h,"final must be absent in a local complex type");
				if(block != XmlSchemaDerivationMethod.None)
					error(h,"block must be absent in a local complex type");
			}

			if(ContentModel != null)
			{
				if(anyAttribute != null || Attributes.Count != 0 || Particle != null)
					error(h,"attributes, particles or anyattribute is not allowed if ContentModel is present");

				if(ContentModel is XmlSchemaSimpleContent)
				{
					XmlSchemaSimpleContent smodel = (XmlSchemaSimpleContent)ContentModel;
					errorCount += smodel.Compile(h,info);
				}
				else if(ContentModel is XmlSchemaComplexContent)
				{
					XmlSchemaComplexContent cmodel = (XmlSchemaComplexContent)ContentModel;
					errorCount += cmodel.Compile(h,info);
				}
			}
			else
			{
				if(Particle is XmlSchemaGroupRef)
				{
					XmlSchemaGroupRef xsgr = (XmlSchemaGroupRef)Particle;
					errorCount += xsgr.Compile(h,info);
				}
				else if(Particle is XmlSchemaAll)
				{
					XmlSchemaAll xsa = (XmlSchemaAll)Particle;
					errorCount += xsa.Compile(h,info);
				}
				else if(Particle is XmlSchemaChoice)
				{
					XmlSchemaChoice xsc = (XmlSchemaChoice)Particle;
					errorCount += xsc.Compile(h,info);
				}
				else if(Particle is XmlSchemaSequence)
				{
					XmlSchemaSequence xss = (XmlSchemaSequence)Particle;
					errorCount += xss.Compile(h,info);
				}

				if(this.anyAttribute != null)
				{
					AnyAttribute.Compile(h,info);
				}
				foreach(XmlSchemaObject obj in Attributes)
				{
					if(obj is XmlSchemaAttribute)
					{
						XmlSchemaAttribute attr = (XmlSchemaAttribute) obj;
						errorCount += attr.Compile(h,info);
					}
					else if(obj is XmlSchemaAttributeGroupRef)
					{
						XmlSchemaAttributeGroupRef atgrp = (XmlSchemaAttributeGroupRef) obj;
						errorCount += atgrp.Compile(h,info);
					}
					else
						error(h,obj.GetType() +" is not valid in this place::ComplexType");
				}
			}

			XmlSchemaUtil.CompileID(Id, this, info.IDCollection, h);
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		//<complexType
		//  abstract = boolean : false
		//  block = (#all | List of (extension | restriction)) 
		//  final = (#all | List of (extension | restriction)) 
		//  id = ID
		//  mixed = boolean : false
		//  name = NCName
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (simpleContent | complexContent | ((group | all | choice | sequence)?, ((attribute | attributeGroup)*, anyAttribute?))))
		//</complexType>
		internal static XmlSchemaComplexType Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaComplexType ctype = new XmlSchemaComplexType();
			reader.MoveToElement();
			Exception innerex;

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaComplexType.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			ctype.LineNumber = reader.LineNumber;
			ctype.LinePosition = reader.LinePosition;
			ctype.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "abstract")
				{
					ctype.IsAbstract = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h,reader.Value + " is invalid value for abstract",innerex);
				}
				else if(reader.Name == "block")
				{
					ctype.block = XmlSchemaUtil.ReadDerivationAttribute(reader,out innerex, "block");
					if(innerex != null)
						warn(h,"some invalid values for block attribute were found",innerex);
				}
				else if(reader.Name == "final")
				{
					ctype.Final = XmlSchemaUtil.ReadDerivationAttribute(reader,out innerex, "block");
					if(innerex != null)
						warn(h,"some invalid values for final attribute were found",innerex);
				}
				else if(reader.Name == "id")
				{
					ctype.Id = reader.Value;
				}
				else if(reader.Name == "mixed")
				{
					ctype.isMixed = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h,reader.Value + " is invalid value for mixed",innerex);
				}
				else if(reader.Name == "name")
				{
					ctype.Name = reader.Value;
				}
				else if(reader.NamespaceURI == "" || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for complexType",null);
				}
				else
				{
					if(reader.Prefix == "xmlns")
						ctype.Namespaces.Add(reader.LocalName, reader.Value);
					else if(reader.Name == "xmlns")
						ctype.Namespaces.Add("",reader.Value);
					//TODO: Add to Unhandled attributes
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return ctype;

			//Content: 1. annotation?, 
			//		   2. simpleContent | 2. complexContent | 
			//			(3.(group | all | choice | sequence)?, (4.(attribute | attributeGroup)*, 5.anyAttribute?)))
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaComplexType.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						ctype.Annotation = annotation;
					continue;
				}
				if(level <=2)
				{
					if(reader.LocalName == "simpleContent")
					{
						level = 6;
						XmlSchemaSimpleContent simple = XmlSchemaSimpleContent.Read(reader,h);
						if(simple != null)
							ctype.ContentModel = simple;
						continue;
					}
					if(reader.LocalName == "complexContent")
					{
						level = 6;
						XmlSchemaComplexContent complex = XmlSchemaComplexContent.Read(reader,h);
						if(complex != null)
							ctype.contentModel = complex;
						continue;
					}
				}
				if(level <= 3)
				{
					if(reader.LocalName == "group")
					{
						level = 4;
						XmlSchemaGroupRef group = XmlSchemaGroupRef.Read(reader,h);
						if(group != null)
							ctype.particle = group;
						continue;
					}
					if(reader.LocalName == "all")
					{
						level = 4;
						XmlSchemaAll all = XmlSchemaAll.Read(reader,h);
						if(all != null)
							ctype.particle = all;
						continue;
					}
					if(reader.LocalName == "choice")
					{
						level = 4;
						XmlSchemaChoice choice = XmlSchemaChoice.Read(reader,h);
						if(choice != null)
							ctype.particle = choice;
						continue;
					}
					if(reader.LocalName == "sequence")
					{
						level = 4;
						XmlSchemaSequence sequence = XmlSchemaSequence.Read(reader,h);
						if(sequence != null)
							ctype.particle = sequence;
						continue;
					}
				}
				if(level <= 4)
				{
					if(reader.LocalName == "attribute")
					{
						level = 4;
						XmlSchemaAttribute attr = XmlSchemaAttribute.Read(reader,h);
						if(attr != null)
							ctype.Attributes.Add(attr);
						continue;
					}
					if(reader.LocalName == "attributeGroup")
					{
						level = 4;
						XmlSchemaAttributeGroupRef attr = XmlSchemaAttributeGroupRef.Read(reader,h);
						if(attr != null)
							ctype.attributes.Add(attr);
						continue;
					}
				}
				if(level <= 5 && reader.LocalName == "anyAttribute")
				{
					level = 6;
					XmlSchemaAnyAttribute anyattr = XmlSchemaAnyAttribute.Read(reader,h);
					if(anyattr != null)
						ctype.AnyAttribute = anyattr;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return ctype;
		}
	}
}
