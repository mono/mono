// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Mono.Xml.Schema;

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
		private static string xmlname = "restriction";
		private string [] enumarationFacetValues;
		private string [] patternFacetValues;
		private Regex [] rexPatterns;
		private decimal lengthFacet;
		private decimal maxLengthFacet;
		private decimal minLengthFacet;
		private decimal fractionDigitsFacet;
		private decimal totalDigitsFacet;

		public XmlSchemaSimpleTypeRestriction()
		{
			baseTypeName = XmlQualifiedName.Empty;
			facets = new XmlSchemaObjectCollection();
		}

		[System.Xml.Serialization.XmlAttribute("base")]
		public XmlQualifiedName BaseTypeName 
		{
			get{ return  baseTypeName; } 
			set{ baseTypeName = value; }
		}

		[XmlElement("simpleType",Namespace=XmlSchema.Namespace)]
		public XmlSchemaSimpleType BaseType 
		{
			get{ return  baseType; } 
			set{ baseType = value; }
		}

		[XmlElement("minExclusive",typeof(XmlSchemaMinExclusiveFacet),Namespace=XmlSchema.Namespace)]
		[XmlElement("minInclusive",typeof(XmlSchemaMinInclusiveFacet),Namespace=XmlSchema.Namespace)] 
		[XmlElement("maxExclusive",typeof(XmlSchemaMaxExclusiveFacet),Namespace=XmlSchema.Namespace)]
		[XmlElement("maxInclusive",typeof(XmlSchemaMaxInclusiveFacet),Namespace=XmlSchema.Namespace)]
		[XmlElement("totalDigits",typeof(XmlSchemaTotalDigitsFacet),Namespace=XmlSchema.Namespace)]
		[XmlElement("fractionDigits",typeof(XmlSchemaFractionDigitsFacet),Namespace=XmlSchema.Namespace)]
		[XmlElement("length",typeof(XmlSchemaLengthFacet),Namespace=XmlSchema.Namespace)]
		[XmlElement("minLength",typeof(XmlSchemaMinLengthFacet),Namespace=XmlSchema.Namespace)]
		[XmlElement("maxLength",typeof(XmlSchemaMaxLengthFacet),Namespace=XmlSchema.Namespace)]
		[XmlElement("enumeration",typeof(XmlSchemaEnumerationFacet),Namespace=XmlSchema.Namespace)]
		[XmlElement("whiteSpace",typeof(XmlSchemaWhiteSpaceFacet),Namespace=XmlSchema.Namespace)]
		[XmlElement("pattern",typeof(XmlSchemaPatternFacet),Namespace=XmlSchema.Namespace)]
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
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			errorCount = 0;

			if(this.baseType != null && !this.BaseTypeName.IsEmpty)
				error(h, "both base and simpletype can't be set");
			if(this.baseType == null && this.BaseTypeName.IsEmpty)
				error(h, "one of basetype or simpletype must be present");
			if(this.baseType != null)
			{
				errorCount += this.baseType.Compile(h,schema);
			}
			if(!XmlSchemaUtil.CheckQName(BaseTypeName))
				error(h,"BaseTypeName must be a XmlQualifiedName");

			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		[MonoTODO]
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.ValidationId))
				return errorCount;

			this.ValidateActualType (baseType, baseTypeName, h, schema);

			enumarationFacetValues = patternFacetValues = null;
			rexPatterns = null;
			lengthFacet = maxLengthFacet = minLengthFacet = fractionDigitsFacet = totalDigitsFacet = -1;

			ArrayList enums = null;
			ArrayList patterns = null;
			for (int i = 0; i < facets.Count; i++) {
				XmlSchemaEnumerationFacet ef = facets [i] as XmlSchemaEnumerationFacet;
				if (ef != null) {
					if (enums == null)
						enums = new ArrayList ();
					enums.Add (ef.Value);
					continue;
				}
				XmlSchemaPatternFacet pf = facets [i] as XmlSchemaPatternFacet;
				if (pf != null) {
					if (patterns == null)
						patterns = new ArrayList ();
					patterns.Add (pf.Value);
					continue;
				}
				XmlSchemaLengthFacet lf = facets [i] as XmlSchemaLengthFacet;
				if (lf != null) {
					try {
						if (lengthFacet >= 0)
							lf.error (h, "There already length facet exists.");
						else
							lengthFacet = decimal.Parse (lf.Value.Trim ());
					} catch (Exception) { // FIXME: better catch ;-(
						lf.error (h, "Invalid length facet specifidation");
					}
					continue;
				}
				XmlSchemaMaxLengthFacet maxlf = facets [i] as XmlSchemaMaxLengthFacet;
				if (maxlf != null) {
					try {
						if (maxLengthFacet >= 0)
							maxlf.error (h, "There already maxLength facet exists.");
						else
							maxLengthFacet = decimal.Parse (maxlf.Value.Trim ());
					} catch (Exception) { // FIXME: better catch ;-(
						maxlf.error (h, "Invalid maxLength facet specifidation");
					}
					continue;
				}
				XmlSchemaMinLengthFacet minlf = facets [i] as XmlSchemaMinLengthFacet;
				if (minlf != null) {
					try {
						if (minLengthFacet >= 0)
							minlf.error (h, "There already minLength facet exists.");
						else
							minLengthFacet = decimal.Parse (minlf.Value.Trim ());
					} catch (Exception) { // FIXME: better catch ;-(
						minlf.error (h, "Invalid minLength facet specifidation");
					}
					continue;
				}
			}
			if (enums != null)
				this.enumarationFacetValues = enums.ToArray (typeof (string)) as string [];
			if (patterns != null) {
				this.patternFacetValues = patterns.ToArray (typeof (string)) as string [];
				this.rexPatterns = new Regex [patterns.Count];
				for (int i = 0; i < patternFacetValues.Length; i++) {
					try {
						Regex rex = new Regex (patternFacetValues [i]);
						rexPatterns [i] = rex;
					} catch (Exception ex) {
						error (h, "Invalid regular expression pattern was specified.", ex);
					}
				}
			}

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		internal bool ValidateValueWithFacets (string value, XmlNameTable nt)
		{
			XmlSchemaSimpleType baseST = this.ActualBaseSchemaType as XmlSchemaSimpleType;
			XmlSchemaSimpleTypeList listType = baseST != null ? baseST.Content as XmlSchemaSimpleTypeList : null;
			// numeric
			if (listType != null)
				return ValidateListValueWithFacets (value, nt);
			else
				return ValidateNonListValueWithFacets (value, nt);
		}

		private bool ValidateListValueWithFacets (string value, XmlNameTable nt)
		{
			string [] list = ((XsdAnySimpleType) XmlSchemaDatatype.FromName ("anySimpleType")).ParseListValue (value, nt);

			// pattern
			if (this.patternFacetValues != null) {
				for (int l = 0; l < list.Length; l++) {
					for (int i = 0; i < this.patternFacetValues.Length; i++)
						if (rexPatterns [i] != null && !rexPatterns [i].IsMatch (list [l]))
							return false;
				}
			}
			// enumeration
			if (this.enumarationFacetValues != null) {
				for (int l = 0; l < list.Length; l++) {
					bool matched = false;
					for (int i = 0; i < this.enumarationFacetValues.Length; i++) {
						if (list [l] == this.enumarationFacetValues [i]) {
							matched = true;
							break;
						}
					}
					if (!matched)
						return false;
				}
			}

			// numeric
			// : length
			if (lengthFacet >= 0 && list.Length != lengthFacet)
					return false;
			// : maxLength
			if (maxLengthFacet >= 0 && list.Length > maxLengthFacet)
					return false;
			// : minLength
			if (minLengthFacet >= 0 && list.Length < minLengthFacet)
					return false;
			return true;
		}

		private bool ValidateNonListValueWithFacets (string value, XmlNameTable nt)
		{
			// pattern
			if (this.patternFacetValues != null) {
				for (int i = 0; i < this.patternFacetValues.Length; i++)
					if (rexPatterns [i] != null && !rexPatterns [i].IsMatch (value))
						return false;
			}
			// enumeration
			if (this.enumarationFacetValues != null) {
				bool matched = false;
				for (int i = 0; i < this.enumarationFacetValues.Length; i++) {
					if (value == this.enumarationFacetValues [i]) {
						matched = true;
						break;
					}
				}
				if (!matched)
					return false;
			}
			// numeric
			// : length
			if (lengthFacet >= 0 && value.Length != lengthFacet)
					return false;
			// : maxLength
			if (maxLengthFacet >= 0 && value.Length > maxLengthFacet)
					return false;
			// : minLength
			if (minLengthFacet >= 0 && value.Length < minLengthFacet)
					return false;

			// TODO: fractionDigits and totalDigits

			// all passed
			return true;
		}

		internal override string Normalize (string s, XmlNameTable nt, XmlNamespaceManager nsmgr)
		{
			return base.Normalize (s, nt, nsmgr);
		}


		//<restriction 
		//  base = QName 
		//  id = ID 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (simpleType?, (minExclusive | minInclusive | maxExclusive | maxInclusive | totalDigits | fractionDigits | length | minLength | maxLength | enumeration | whiteSpace | pattern)*))
		//</restriction>
		internal static XmlSchemaSimpleTypeRestriction Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaSimpleTypeRestriction.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			restriction.LineNumber = reader.LineNumber;
			restriction.LinePosition = reader.LinePosition;
			restriction.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					restriction.Id = reader.Value;
				}
				else if(reader.Name == "base")
				{
					Exception innerex;
					restriction.baseTypeName = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for base attribute",innerex);
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for restriction",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,restriction);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return restriction;

			//  Content: annotation?, simpleType?, (minExclusive |. .. | pattern)*
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaSimpleTypeRestriction.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						restriction.Annotation = annotation;
					continue;
				}
				if(level <= 2 && reader.LocalName == "simpleType")
				{
					level = 3;
					XmlSchemaSimpleType stype = XmlSchemaSimpleType.Read(reader,h);
					if(stype != null)
						restriction.baseType = stype;
					continue;
				}
				if(level <= 3)
				{
					if(reader.LocalName == "minExclusive")
					{
						level = 3;
						XmlSchemaMinExclusiveFacet minex = XmlSchemaMinExclusiveFacet.Read(reader,h);
						if(minex != null)
							restriction.facets.Add(minex);
						continue;
					}
					else if(reader.LocalName == "minInclusive")
					{
						level = 3;
						XmlSchemaMinInclusiveFacet mini = XmlSchemaMinInclusiveFacet.Read(reader,h);
						if(mini != null)
							restriction.facets.Add(mini);
						continue;
					}
					else if(reader.LocalName == "maxExclusive")
					{
						level = 3;
						XmlSchemaMaxExclusiveFacet maxex = XmlSchemaMaxExclusiveFacet.Read(reader,h);
						if(maxex != null)
							restriction.facets.Add(maxex);
						continue;
					}
					else if(reader.LocalName == "maxInclusive")
					{
						level = 3;
						XmlSchemaMaxInclusiveFacet maxi = XmlSchemaMaxInclusiveFacet.Read(reader,h);
						if(maxi != null)
							restriction.facets.Add(maxi);
						continue;
					}
					else if(reader.LocalName == "totalDigits")
					{
						level = 3;
						XmlSchemaTotalDigitsFacet total = XmlSchemaTotalDigitsFacet.Read(reader,h);
						if(total != null)
							restriction.facets.Add(total);
						continue;
					}
					else if(reader.LocalName == "fractionDigits")
					{
						level = 3;
						XmlSchemaFractionDigitsFacet fraction = XmlSchemaFractionDigitsFacet.Read(reader,h);
						if(fraction != null)
							restriction.facets.Add(fraction);
						continue;
					}
					else if(reader.LocalName == "length")
					{
						level = 3;
						XmlSchemaLengthFacet length = XmlSchemaLengthFacet.Read(reader,h);
						if(length != null)
							restriction.facets.Add(length);
						continue;
					}
					else if(reader.LocalName == "minLength")
					{
						level = 3;
						XmlSchemaMinLengthFacet minlen = XmlSchemaMinLengthFacet.Read(reader,h);
						if(minlen != null)
							restriction.facets.Add(minlen);
						continue;
					}
					else if(reader.LocalName == "maxLength")
					{
						level = 3;
						XmlSchemaMaxLengthFacet maxlen = XmlSchemaMaxLengthFacet.Read(reader,h);
						if(maxlen != null)
							restriction.facets.Add(maxlen);
						continue;
					}
					else if(reader.LocalName == "enumeration")
					{
						level = 3;
						XmlSchemaEnumerationFacet enumeration = XmlSchemaEnumerationFacet.Read(reader,h);
						if(enumeration != null)
							restriction.facets.Add(enumeration);
						continue;
					}
					else if(reader.LocalName == "whiteSpace")
					{
						level = 3;
						XmlSchemaWhiteSpaceFacet ws = XmlSchemaWhiteSpaceFacet.Read(reader,h);
						if(ws != null)
							restriction.facets.Add(ws);
						continue;
					}
					else if(reader.LocalName == "pattern")
					{
						level = 3;
						XmlSchemaPatternFacet pattern = XmlSchemaPatternFacet.Read(reader,h);
						if(pattern != null)
							restriction.facets.Add(pattern);
						continue;
					}
				}
				reader.RaiseInvalidElementError();
			}
			return restriction;
		}
	}
}
