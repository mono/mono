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
		
		private int errorCount;
		internal bool istoplevel = false;

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
		//But since Particle is a PSVI 
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
				else
				{
					if(!XmlSchemaUtil.CheckNCName(Name))
						error(h,"name must be a NCName");
					else
						this.qName = new XmlQualifiedName(info.targetNS, Name);
				}
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
					if(info.blockDefault == XmlSchemaDerivationMethod.All)
					{
						blockResolved = XmlSchemaDerivationMethod.All;
					}
					else
						blockResolved = info.blockDefault & (XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Restriction);
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
					if(info.finalDefault == XmlSchemaDerivationMethod.All)
					{
						finalResolved = XmlSchemaDerivationMethod.All;
					}
					else
					{
						finalResolved = info.blockDefault & (XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Restriction);
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
						attr.Compile(h,info);
					}
					else if(obj is XmlSchemaAttributeGroup)
					{
						XmlSchemaAttributeGroup atgrp = (XmlSchemaAttributeGroup) obj;
						atgrp.Compile(h,info);
					}
					else
						error(h,"object is not valid in this place");
				}
			
			}
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		internal void error(ValidationEventHandler handle,string message)
		{
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
