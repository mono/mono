// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleContentRestriction.
	/// </summary>
	public class XmlSchemaSimpleContentRestriction : XmlSchemaContent
	{
		
		private XmlSchemaAnyAttribute any;
		private XmlSchemaObjectCollection attributes;
		private XmlSchemaSimpleType baseType;
		private XmlQualifiedName baseTypeName;
		private XmlSchemaObjectCollection facets;
		private int errorCount=0;

		public XmlSchemaSimpleContentRestriction()
		{
			baseTypeName = XmlQualifiedName.Empty;
			attributes	 = new XmlSchemaObjectCollection();
			facets		 = new XmlSchemaObjectCollection();
		}

		[System.Xml.Serialization.XmlAttribute("base")]
		public XmlQualifiedName BaseTypeName 
		{
			get{ return  baseTypeName; }
			set{ baseTypeName = value; }
		}

		[XmlElement("anyAttribute",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaAnyAttribute AnyAttribute 
		{
			get{ return  any; }
			set{ any = value; }
		}

		[XmlElement("attribute",typeof(XmlSchemaAttribute),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Attributes 
		{
			get{ return attributes; }
		}

		[XmlElement("simpleType",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaSimpleType BaseType 
		{ 
			get{ return  baseType; } 
			set{ baseType = value; } 
		}
		
		
		[XmlElement("minExclusive",typeof(XmlSchemaMinExclusiveFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("minInclusive",typeof(XmlSchemaMinInclusiveFacet),Namespace="http://www.w3.org/2001/XMLSchema")] 
		[XmlElement("maxExclusive",typeof(XmlSchemaMaxExclusiveFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("maxInclusive",typeof(XmlSchemaMaxInclusiveFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("totalDigits",typeof(XmlSchemaTotalDigitsFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("fractionDigits",typeof(XmlSchemaFractionDigitsFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("length",typeof(XmlSchemaLengthFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("minLength",typeof(XmlSchemaMinLengthFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("maxLength",typeof(XmlSchemaMaxLengthFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("enumeration",typeof(XmlSchemaEnumerationFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("whiteSpace",typeof(XmlSchemaWhiteSpaceFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("pattern",typeof(XmlSchemaPatternFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Facets 
		{ 
			get{ return facets; } 
		}
		///<remarks>
		/// 1. Base must be present and a QName
		///</remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			if(BaseTypeName == null || BaseTypeName.IsEmpty)
			{
				error(h, "base must be present and a QName");
			}

			if(BaseType != null)
			{
				errorCount += BaseType.Compile(h,info);
			}

			if(this.AnyAttribute != null)
			{
				errorCount += AnyAttribute.Compile(h,info);
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
					error(h,"object is not valid in this place");
			}
			
			//TODO: Compile Facets: Looks like they are a part of datatypes. So we'll do them with the datatypes

			if(this.Id != null && !XmlSchemaUtil.CheckID(Id))
				error(h, "id must be a valid ID");
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		internal void error(ValidationEventHandler handle,string message)
		{
			errorCount++;
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
