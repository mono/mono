// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;


namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleTypeRestriction.
	/// </summary>
	public class XmlSchemaSimpleTypeRestriction : XmlSchemaSimpleTypeContent
	{
		private XmlSchemaSimpleType baseType;
		private XmlQualifiedName baseTypeName;
		private XmlSchemaObjectCollection facets;
		private int errorCount;

		public XmlSchemaSimpleTypeRestriction()
		{
			baseTypeName = XmlQualifiedName.Empty;
			facets = new XmlSchemaObjectCollection();
		}

		[XmlElement("simpleType",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaSimpleType BaseType 
		{
			get{ return  baseType; } 
			set{ baseType = value; }
		}

		[System.Xml.Serialization.XmlAttribute("base")]
		public XmlQualifiedName BaseTypeName 
		{
			get{ return  baseTypeName; } 
			set{ baseTypeName = value; }
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

		/// <remarks>
		/// 1. One of base or simpletype must be present but not both
		/// 2. id must be a valid ID
		/// 3. base must be a valid QName *NO CHECK REQUIRED*
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			errorCount = 0;

			if(this.baseType != null && !this.BaseTypeName.IsEmpty)
				error(h, "both base and simpletype can't be set");
			if(this.baseType == null && this.BaseTypeName.IsEmpty)
				error(h, "one of basetype or simpletype must be present");
			if(this.baseType != null)
			{
				errorCount += this.baseType.Compile(h,info);
			}

			if(this.Id != null && !XmlSchemaUtil.CheckID(this.Id))
				error(h,"id must be a valid ID");

			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		internal void error(ValidationEventHandler handle,string message)
		{
			this.errorCount++;
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
