// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Mono.Xml.Schema;
using System.Globalization;

#if NET_2_0
using NSResolver = System.Xml.IXmlNamespaceResolver;
#else
using NSResolver = System.Xml.XmlNamespaceManager;
#endif

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
		const string xmlname = "restriction";
		private string [] enumarationFacetValues;
		private string [] patternFacetValues;
		private Regex [] rexPatterns;
		private decimal lengthFacet;
		private decimal maxLengthFacet;
		private decimal minLengthFacet;
		private decimal fractionDigitsFacet;
		private decimal totalDigitsFacet;
		private object maxInclusiveFacet ;
		private object maxExclusiveFacet ;
		private object minInclusiveFacet ;
		private object minExclusiveFacet ;
		private XmlSchemaFacet.Facet fixedFacets = XmlSchemaFacet.Facet.None; 
		private static NumberStyles lengthStyle = NumberStyles.Integer;


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

		[XmlElement("simpleType", Type=typeof (XmlSchemaSimpleType))]
		public XmlSchemaSimpleType BaseType 
		{
			get{ return  baseType; } 
			set{ baseType = value; }
		}

		[XmlElement("minExclusive",typeof(XmlSchemaMinExclusiveFacet))]
		[XmlElement("minInclusive",typeof(XmlSchemaMinInclusiveFacet))] 
		[XmlElement("maxExclusive",typeof(XmlSchemaMaxExclusiveFacet))]
		[XmlElement("maxInclusive",typeof(XmlSchemaMaxInclusiveFacet))]
		[XmlElement("totalDigits",typeof(XmlSchemaTotalDigitsFacet))]
		[XmlElement("fractionDigits",typeof(XmlSchemaFractionDigitsFacet))]
		[XmlElement("length",typeof(XmlSchemaLengthFacet))]
		[XmlElement("minLength",typeof(XmlSchemaMinLengthFacet))]
		[XmlElement("maxLength",typeof(XmlSchemaMaxLengthFacet))]
		[XmlElement("enumeration",typeof(XmlSchemaEnumerationFacet))]
		[XmlElement("whiteSpace",typeof(XmlSchemaWhiteSpaceFacet))]
		[XmlElement("pattern",typeof(XmlSchemaPatternFacet))]
		public XmlSchemaObjectCollection Facets 
		{
			get{ return facets; }
		}

		internal override void SetParent (XmlSchemaObject parent)
		{
			base.SetParent (parent);
			if (BaseType != null)
				BaseType.SetParent (this);
			foreach (XmlSchemaObject obj in Facets)
				obj.SetParent (this);
		}

		/// <remarks>
		/// 1. One of base or simpletype must be present but not both
		/// 2. id must be a valid ID
		/// 3. base must be a valid QName *NO CHECK REQUIRED*
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (CompilationId == schema.CompilationId)
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

			for (int i = 0; i < this.Facets.Count; i++)
				if (! (Facets [i] is XmlSchemaFacet))
					error (h, "Only XmlSchemaFacet objects are allowed for Facets property");
			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		/** Checks if this facet is valid on this restriction. Does not check that it has
			* not been fixed in the baseType. That is done elsewhere.
			*/
		
		private static readonly XmlSchemaFacet.Facet listFacets =
						 XmlSchemaFacet.Facet.length | XmlSchemaFacet.Facet.minLength |
						 XmlSchemaFacet.Facet.maxLength | XmlSchemaFacet.Facet.pattern | 
						 XmlSchemaFacet.Facet.enumeration | XmlSchemaFacet.Facet.whiteSpace; 
 
		
		private bool IsAllowedFacet(XmlSchemaFacet xsf) {
		/* Must be called after this.ValidateActualType, as it uses actualBaseSchemaType */

			XsdAnySimpleType ast = ActualBaseSchemaType as XsdAnySimpleType;
			if (ast != null) {
				// Based directly on an xsd type 
				return ast.AllowsFacet(xsf);
			}
			else {
			 XmlSchemaSimpleTypeContent st = ((XmlSchemaSimpleType)ActualBaseSchemaType).Content as XmlSchemaSimpleTypeContent;
			 if (st != null) {
				 XmlSchemaSimpleTypeRestriction str = st as XmlSchemaSimpleTypeRestriction;
				 if (str != null && str != this) {
					 return str.IsAllowedFacet(xsf);
				 }
				 XmlSchemaSimpleTypeList stl = st as XmlSchemaSimpleTypeList;
				 if (stl != null) {
					 return ((xsf.ThisFacet & listFacets) != 0);
				 }

				 XmlSchemaSimpleTypeUnion stu = st as XmlSchemaSimpleTypeUnion;
				 if (stu != null) {
					 return (xsf is XmlSchemaPatternFacet ||
									 xsf is XmlSchemaEnumerationFacet);
				 }
				 
			 }
			 else {
				 // TODO: Should this be either a XmlSchemaSimpleType or XmlSchemaDatatype ?
				 // If so report error
			 }
			}
			// Not sure it could ever get here
			return false;
		}
		
		
	
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			
			if (IsValidated (schema.ValidationId))
				return errorCount;

			this.ValidateActualType (h, schema);

			
			lengthFacet = maxLengthFacet = minLengthFacet = fractionDigitsFacet = totalDigitsFacet = -1;
			
			XmlSchemaSimpleTypeRestriction baseSTR = null; 

			if (ActualBaseSchemaType is XmlSchemaSimpleType) {
				XmlSchemaSimpleTypeContent st = ((XmlSchemaSimpleType)ActualBaseSchemaType).Content as XmlSchemaSimpleTypeContent;
				baseSTR = st as XmlSchemaSimpleTypeRestriction;
			}
			
				
			if (baseSTR != null) {
				fixedFacets = baseSTR.fixedFacets;
				lengthFacet = baseSTR.lengthFacet;
				maxLengthFacet = baseSTR.maxLengthFacet;
				minLengthFacet = baseSTR.minLengthFacet;
				fractionDigitsFacet = baseSTR.fractionDigitsFacet;
				totalDigitsFacet = baseSTR.totalDigitsFacet;
				maxInclusiveFacet = baseSTR.maxInclusiveFacet;
				maxExclusiveFacet = baseSTR.maxExclusiveFacet;
				minInclusiveFacet = baseSTR.minInclusiveFacet;
				minExclusiveFacet = baseSTR.minExclusiveFacet;
			}
			
			enumarationFacetValues = patternFacetValues = null;
			rexPatterns = null;
			
			XmlSchemaFacet.Facet facetsDefined = XmlSchemaFacet.Facet.None; 

			ArrayList enums = null;
			ArrayList patterns = null;
			for (int i = 0; i < facets.Count; i++) {

				XmlSchemaFacet facet = facets[i] as XmlSchemaFacet;
				if (facet != null) {
					if (!IsAllowedFacet(facet)) {
						facet.error(h, facet.ThisFacet +" is not a valid facet for this type");
						continue;
					}
				}
				else {
					// Other than XmlSchemaFacet; already reported at Compile()
					continue;
				}

				
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
				
				// Put this test here, as pattern and enumeration 
				// can occur multiple times.
				if ( (facetsDefined & facet.ThisFacet) !=0) {
					facet.error (h, "This is a duplicate '" + facet.ThisFacet + "' facet.");
					continue;
				}
				else {
					facetsDefined |= facet.ThisFacet; 
				}

				
				

				
				if (facet is XmlSchemaLengthFacet) {
					checkLengthFacet((XmlSchemaLengthFacet)facet, facetsDefined, h);
				}
				else if (facet is XmlSchemaMaxLengthFacet) {
					checkMaxLengthFacet((XmlSchemaMaxLengthFacet)facet, facetsDefined, h);
				}
				else if (facet is XmlSchemaMinLengthFacet) {
					checkMinLengthFacet((XmlSchemaMinLengthFacet)facet, facetsDefined, h);
				}
				
				else if (facet is XmlSchemaMinInclusiveFacet) {
					checkMinMaxFacet((XmlSchemaMinInclusiveFacet)facet, ref minInclusiveFacet, h);
				}
				else if (facet is XmlSchemaMaxInclusiveFacet) {
					checkMinMaxFacet((XmlSchemaMaxInclusiveFacet)facet, ref maxInclusiveFacet, h);
				}
				else if (facet is XmlSchemaMinExclusiveFacet) {
					checkMinMaxFacet((XmlSchemaMinExclusiveFacet)facet, ref minExclusiveFacet, h);
				}
				else if (facet is XmlSchemaMaxExclusiveFacet) {
					checkMinMaxFacet((XmlSchemaMaxExclusiveFacet)facet, ref maxExclusiveFacet, h);
				}
				else if (facet is XmlSchemaFractionDigitsFacet) {
					checkFractionDigitsFacet((XmlSchemaFractionDigitsFacet)facet, h);
				}
				else if (facet is XmlSchemaTotalDigitsFacet) {
					checkTotalDigitsFacet((XmlSchemaTotalDigitsFacet)facet, h);
				}

				if (facet.IsFixed) {
					fixedFacets |= facet.ThisFacet;
				}
				
			 
			}
			if (enums != null)
				this.enumarationFacetValues = enums.ToArray (typeof (string)) as string [];
			if (patterns != null) {
				this.patternFacetValues = patterns.ToArray (typeof (string)) as string [];
				this.rexPatterns = new Regex [patterns.Count];
				for (int i = 0; i < patternFacetValues.Length; i++) {
					try {
						string src = patternFacetValues [i];
						StringBuilder sb = null;
						int start = 0;
						for (int c = 0; c < src.Length; c++) {
							if (src [c] == '\\' && src.Length > i + 1) {
								string subst = null;
								switch (src [c + 1]) {
								case 'i':
									subst = "[\\p{L}_]";
									break;
								case 'I':
									subst = "[^\\p{L}_]";
									break;
								case 'c':
									subst = "[\\p{L}\\p{N}_\\.\\-:]";
									break;
								case 'C':
									subst = "[^\\p{L}\\p{N}_\\.\\-:]";
									break;
								}
								if (subst != null) {
									if (sb == null)
										sb = new StringBuilder ();
									sb.Append (src, start, c - start);
									sb.Append (subst);
									start = c + 2;
								}
							}
						}
						if (sb != null) {
							sb.Append (src, start, src.Length - start);
							src = sb.ToString ();
						}
//						src = src.Replace ("\\i", "[\\p{L}_]").Replace ("\\I", "[^\\p{L}_]").Replace ("\\c", "[\\p{L}\\p{N}_\\.\\-:]").Replace ("\\C", "[^\\p{L}\\p{N}_\\.\\-:]");
						Regex rex = new Regex ("^" + src + "$");
						rexPatterns [i] = rex;
					} catch (Exception ex) {
						error (h, "Invalid regular expression pattern was specified.", ex);
					}
				}
				
				
			
			}

			ValidationId = schema.ValidationId;
		 /* 
				Console.WriteLine("Facets:\n defined\t{10}\n fixed\t{0}\n length\t{1}\n maxLen\t{2}\n minLen\t{3}\n " +
													"frac\t{4}\n tot\t{5}\n maxI\t{6}\n maxE\t{7}\n minI\t{8}\n minE\t{9}\n", 
						fixedFacets , 
						lengthFacet, 
						maxLengthFacet ,
						minLengthFacet ,
						fractionDigitsFacet ,
						totalDigitsFacet ,
						maxInclusiveFacet ,
						maxExclusiveFacet ,
						minInclusiveFacet ,
						minExclusiveFacet , 
						facetsDefined);
*/
			return errorCount;
		}


		internal void ValidateActualType (ValidationEventHandler h, XmlSchema schema)
		{
			GetActualType (h, schema, true);
		}

		internal object GetActualType (ValidationEventHandler h, XmlSchema schema, bool validate)
		{
			object actualBaseSchemaType = null;

			XmlSchemaSimpleType type = baseType;
			if (type == null)
				type = schema.FindSchemaType (baseTypeName) as XmlSchemaSimpleType;
			if (type != null) {
				if (validate)
					errorCount += type.Validate (h, schema);
				actualBaseSchemaType = type;
			} else if (baseTypeName == XmlSchemaComplexType.AnyTypeName) {
				actualBaseSchemaType = XmlSchemaSimpleType.AnySimpleType;
			} else if (baseTypeName.Namespace == XmlSchema.Namespace ||
				baseTypeName.Namespace == XmlSchema.XdtNamespace) {
				actualBaseSchemaType = XmlSchemaDatatype.FromName (baseTypeName);
				if (actualBaseSchemaType == null)
					if (validate)
						error (h, "Invalid schema type name was specified: " + baseTypeName);
			}
			// otherwise, it might be missing sub components.
			else if (!schema.IsNamespaceAbsent (baseTypeName.Namespace))
				if (validate)
					error (h, "Referenced base schema type " + baseTypeName + " was not found in the corresponding schema.");

			return actualBaseSchemaType;
		}

		
		private void checkTotalDigitsFacet (XmlSchemaTotalDigitsFacet totf, 
																				ValidationEventHandler h) {
			if (totf != null) {
			/* totalDigits is the maximum number of digits in values of datatypes
			 * derived from decimal. The value of totalDigits must be a
			 * positiveInteger. */
				try {
					decimal newTotalDigits = decimal.Parse (totf.Value.Trim (), lengthStyle, CultureInfo.InvariantCulture);
					if (newTotalDigits <= 0) 
						totf.error(h, String.Format(CultureInfo.InvariantCulture, "The value '{0}' is an invalid totalDigits value", newTotalDigits));
					// Valid restriction
					if ((totalDigitsFacet > 0) && (newTotalDigits > totalDigitsFacet)) {
						totf.error(h, String.Format(CultureInfo.InvariantCulture, "The value '{0}' is not a valid restriction of the base totalDigits facet '{1}'", newTotalDigits, totalDigitsFacet));
					}
					totalDigitsFacet = newTotalDigits;
				}
				catch (FormatException ) {
					totf.error(h, String.Format("The value '{0}' is an invalid totalDigits facet specification", totf.Value.Trim () ));
				}
			}
		}

		
		private void checkFractionDigitsFacet (XmlSchemaFractionDigitsFacet fracf, 
																					 ValidationEventHandler h) {

			if (fracf != null) {
				try {
					decimal newFractionDigits = decimal.Parse (fracf.Value.Trim (), lengthStyle, CultureInfo.InvariantCulture);
					if (newFractionDigits< 0) 
						fracf.error(h, String.Format(CultureInfo.InvariantCulture, "The value '{0}' is an invalid fractionDigits value", newFractionDigits));
					
					if ((fractionDigitsFacet >= 0) && (newFractionDigits > fractionDigitsFacet)) {
						fracf.error(h, String.Format(CultureInfo.InvariantCulture, "The value '{0}' is not a valid restriction of the base fractionDigits facet '{1}'", newFractionDigits, fractionDigitsFacet));
					}
					fractionDigitsFacet = newFractionDigits;
				}
				catch (FormatException ) {
					fracf.error(h, String.Format("The value '{0}' is an invalid fractionDigits facet specification", fracf.Value.Trim () ));
				}
			}

		}
 
		
		private void checkMinMaxFacet(XmlSchemaFacet facet, 
																		ref object baseFacet,
																		ValidationEventHandler h) { 
// Is it a valid instance of the base type.
		 object newValue = ValidateValueWithDatatype(facet.Value);
		 if (newValue != null) {
// Is the base fixed - if so is it the same
			 if (((fixedFacets & facet.ThisFacet) != 0)  && (baseFacet != null)){
				 XsdAnySimpleType dt = getDatatype();
				 if (dt.Compare (newValue, baseFacet) != XsdOrdering.Equal) {
					 facet.error (h, 
							 String.Format(CultureInfo.InvariantCulture, "{0} is not the same as fixed parent {1} facet.", 
										 facet.Value, facet.ThisFacet));
				 }
			 }
			 baseFacet = newValue;
		 }
		 else {
			 facet.error(h, 
					 String.Format("The value '{0}' is not valid against the base type.", facet.Value));
		 }
		}
		
		

		private void checkLengthFacet(XmlSchemaLengthFacet lf,	
																	XmlSchemaFacet.Facet facetsDefined, 
																	ValidationEventHandler h) {
				if (lf != null) {
					try {
					if ((facetsDefined & (XmlSchemaFacet.Facet.minLength | XmlSchemaFacet.Facet.maxLength)) != 0)  
							lf.error(h, "It is an error for both length and minLength or maxLength to be present.");
						else {
							lengthFacet = decimal.Parse (lf.Value.Trim (), lengthStyle, CultureInfo.InvariantCulture);
						/* TODO: Check that it is between inherited max/min lengths */
							if (lengthFacet < 0) 
								lf.error(h, "The value '" + lengthFacet + "' is an invalid length");
						}
				} catch (FormatException) { // FIXME: better catch ;-(
						lf.error (h, "The value '" + lf.Value + "' is an invalid length facet specification");
					}
				}
		}

		private void checkMaxLengthFacet(XmlSchemaMaxLengthFacet maxlf, 
																		 XmlSchemaFacet.Facet facetsDefined,
																		 ValidationEventHandler h) {
				if (maxlf != null) {
					try {
					if ((facetsDefined & XmlSchemaFacet.Facet.length) != 0) 
							maxlf.error(h, "It is an error for both length and minLength or maxLength to be present.");
						else {
						decimal newMaxLengthFacet = decimal.Parse (maxlf.Value.Trim (), lengthStyle, CultureInfo.InvariantCulture);
						
						if (((fixedFacets & XmlSchemaFacet.Facet.maxLength)!=0) && (newMaxLengthFacet != maxLengthFacet)) 
							maxlf.error(h, String.Format(CultureInfo.InvariantCulture, "The value '{0}' is not the same as the fixed value '{1}' on the base type", maxlf.Value.Trim (), maxLengthFacet));
						if ((maxLengthFacet >0) && (newMaxLengthFacet > maxLengthFacet)) 
							maxlf.error(h, String.Format(CultureInfo.InvariantCulture, "The value '{0}' is not a valid restriction of the value '{1}' on the base maxLength facet", maxlf.Value.Trim (), maxLengthFacet));
						else
							maxLengthFacet = newMaxLengthFacet;
							if (maxLengthFacet < 0) 
								maxlf.error(h, "The value '" + maxLengthFacet + "' is an invalid maxLength");
							if (minLengthFacet >=0 && minLengthFacet > maxLengthFacet)
								maxlf.error(h, "minLength is greater than maxLength.");
						}

				} catch (FormatException) { 
						maxlf.error (h, "The value '" + maxlf.Value+ "' is an invalid maxLength facet specification");
					}
				}
		}

		private void checkMinLengthFacet(XmlSchemaMinLengthFacet minlf, 
																		 XmlSchemaFacet.Facet facetsDefined,
																		 ValidationEventHandler h) {
				if (minlf != null) {
					try {
						if (lengthFacet >=0) 
							minlf.error(h, "It is an error for both length and minLength or maxLength to be present.");
						else {
						decimal newMinLengthFacet = decimal.Parse (minlf.Value.Trim (), lengthStyle, CultureInfo.InvariantCulture);
						
						if (((fixedFacets & XmlSchemaFacet.Facet.minLength)!=0) && (newMinLengthFacet != minLengthFacet)) 
							minlf.error(h, String.Format(CultureInfo.InvariantCulture, "The value '{0}' is not the same as the fixed value '{1}' on the base type", minlf.Value.Trim (), minLengthFacet));
						if (newMinLengthFacet < minLengthFacet) 
							minlf.error(h, String.Format(CultureInfo.InvariantCulture, "The value '{0}' is not a valid restriction of the value '{1}' on the base minLength facet", minlf.Value.Trim (), minLengthFacet));
						else
							minLengthFacet = newMinLengthFacet;
							if (minLengthFacet < 0) 
								minlf.error(h, "The value '" + minLengthFacet + "' is an invalid minLength");
							if (maxLengthFacet >=0 && minLengthFacet > maxLengthFacet)
								minlf.error(h, "minLength is greater than maxLength.");
						}
				} catch (FormatException) {
						minlf.error (h, "The value '" + minlf.Value + "' is an invalid minLength facet specification");
					}
				}
			}


		private XsdAnySimpleType getDatatype() {
			XsdAnySimpleType ast = ActualBaseSchemaType as XsdAnySimpleType;
			if (ast != null) {
				// Based directly on an xsd type 
				return ast;
			}
			XmlSchemaSimpleTypeContent st = ((XmlSchemaSimpleType)ActualBaseSchemaType).Content as XmlSchemaSimpleTypeContent;
			
			if (st is XmlSchemaSimpleTypeRestriction) {
				return ((XmlSchemaSimpleTypeRestriction)st).getDatatype();
			}
			else if ((st is XmlSchemaSimpleTypeList) ||
							 (st is XmlSchemaSimpleTypeUnion)) {
				return null;
			}
			return null;
		}

		
		private object ValidateValueWithDatatype(string value) {
			XsdAnySimpleType dt = getDatatype();
			object ret = null;
			//		Console.WriteLine("DT: " + dt);
			if (dt != null) {
					try {
					/* I think we can parse null here, as the types 
					 * that use the nametable and nsmgr are ones that 
					 * we don't need to parse here.
					 */ 
					ret = dt.ParseValue (value, null, null);
				//	Console.WriteLine("Ret: " + ret);
					// If we are based on something with facets, check that we are valid
					if (ActualBaseSchemaType is XmlSchemaSimpleType) {
						XmlSchemaSimpleTypeContent st = ((XmlSchemaSimpleType) ActualBaseSchemaType).Content as XmlSchemaSimpleTypeContent;
						if (st is XmlSchemaSimpleTypeRestriction) {
							if (((XmlSchemaSimpleTypeRestriction)st).ValidateValueWithFacets(value, null, null)) {
								return ret;
							} else {
								return null;
					}
				}
			}

				} catch (Exception) {
					return null;
				}
			}
			return ret;
		}

		internal bool ValidateValueWithFacets (string value, XmlNameTable nt, NSResolver nsmgr)
		{
			/*
			 * FIXME: Shouldn't this be recursing more? What if this is a 
			 * restriction of a restriction of a list type?
			 */
			XmlSchemaSimpleType baseST = this.ActualBaseSchemaType as XmlSchemaSimpleType;
			XmlSchemaSimpleTypeList listType = baseST != null ? baseST.Content as XmlSchemaSimpleTypeList : null;

			// numeric
			if (listType != null)
				return ValidateListValueWithFacets (value, nt, nsmgr);
			else
				return ValidateNonListValueWithFacets (value, nt, nsmgr);
		}

		private bool ValidateListValueWithFacets (string value, XmlNameTable nt, NSResolver nsmgr)
		{
			try {
				return ValidateListValueWithFacetsCore (value, nt, nsmgr);
			} catch (Exception) { // this is for datatype ParseValue()
				return false;
			}
		}

		private bool ValidateListValueWithFacetsCore (string value, XmlNameTable nt, NSResolver nsmgr)
		{
			string [] list = ((XsdAnySimpleType) XmlSchemaDatatype.FromName ("anySimpleType", XmlSchema.Namespace)).ParseListValue (value, nt);

			// pattern
			if (this.patternFacetValues != null) {
				for (int l = 0; l < list.Length; l++) {
					for (int i = 0; i < this.patternFacetValues.Length; i++)
						if (rexPatterns [i] != null && !rexPatterns [i].IsMatch (list [l]))
							return false;
				}
			}

			bool enumMatched = false;
#if true
			// enumeration - lexical space comparison
			//
			// I'm not sure if allowing literally-equivalent values
			// without parsing (because it will cause trouble when
			// you try to get TypedValue anyways), but at least it
			// avoids extraneous validation errors ...
			if (this.enumarationFacetValues != null) {
				for (int l = 0; l < list.Length; l++) {
					for (int i = 0; i < this.enumarationFacetValues.Length; i++) {
						if (list [l] == this.enumarationFacetValues [i]) {
							enumMatched = true;
							break;
						}
					}
				}
			}
#endif
			// enumeration - value space comparison
			if (!enumMatched && this.enumarationFacetValues != null) {
				for (int l = 0; l < list.Length; l++) {

					XsdAnySimpleType dt = getDatatype ();
					if (dt == null)
						dt = (XsdAnySimpleType) XmlSchemaDatatype.FromName ("anySimpleType", XmlSchema.Namespace);
					object v = dt.ParseValue (list [l], nt, nsmgr);

					for (int i = 0; i < this.enumarationFacetValues.Length; i++) {
						if (XmlSchemaUtil.AreSchemaDatatypeEqual (dt, v, dt, dt.ParseValue (this.enumarationFacetValues [i], nt, nsmgr))) {
							enumMatched = true;
							break;
						}
					}
					if (!enumMatched)
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

		private bool ValidateNonListValueWithFacets (string value, XmlNameTable nt, NSResolver nsmgr)
		{
			try {
				return ValidateNonListValueWithFacetsCore (value, nt, nsmgr);
			} catch (Exception) { // this is for datatype ParseValue()
				return false;
			}
		}

		private bool ValidateNonListValueWithFacetsCore (string value, XmlNameTable nt, NSResolver nsmgr)
		{
			// pattern
			// Patterns are the only facets that need to be checked on this
			// type and its base types. We should probably separate them, then
			// do base-type pattern validation.
			if (this.patternFacetValues != null) {
				bool matched = false;
				for (int i = 0; i < this.patternFacetValues.Length; i++)
					if (rexPatterns [i] != null && rexPatterns [i].IsMatch (value)) {
						matched = true;
				                break;
					}
				if (!matched)
					return false;
			}

			XsdAnySimpleType dt = getDatatype ();

			bool enumMatched = false;

#if true
			// enumeration - lexical space comparison
			//
			// I'm not sure if allowing literally-equivalent values
			// without parsing (because it will cause trouble when
			// you try to get TypedValue anyways), but at least it
			// avoids extraneous validation errors ...
			if (this.enumarationFacetValues != null) {
				for (int i = 0; i < this.enumarationFacetValues.Length; i++) {
					if (value == this.enumarationFacetValues [i]) {
						enumMatched = true;
						break;
					}
				}
			}
#endif
			// enumeration - value space comparison
			if (!enumMatched && this.enumarationFacetValues != null) {
				XsdAnySimpleType edt = dt;
				if (edt == null)
					edt = (XsdAnySimpleType) XmlSchemaDatatype.FromName ("anySimpleType", XmlSchema.Namespace);
				object v = edt.ParseValue (value, nt, nsmgr);
				for (int i = 0; i < this.enumarationFacetValues.Length; i++) {
					if (XmlSchemaUtil.AreSchemaDatatypeEqual (edt, v, edt, edt.ParseValue (this.enumarationFacetValues [i], nt, nsmgr))) {
						enumMatched = true;
						break;
					}
				}
				if (!enumMatched)
					return false;
			}
			
			// Need to skip length tests for 
			// types derived from QName or NOTATION
			// see errata: E2-36 Clarification
			
			if (! ( (dt is XsdQName) || (dt is XsdNotation))) {  
        // Length potentially slower now, so only calculate if needed			
				if (! ((lengthFacet == -1) && (maxLengthFacet == -1) && (minLengthFacet == -1))) {
											
					// numeric
					// : length
				
					int length = dt.Length(value);
					
					if (lengthFacet >= 0 && length != lengthFacet)
							return false;
					// : maxLength
					if (maxLengthFacet >= 0 && length > maxLengthFacet)
							return false;
					// : minLength
					if (minLengthFacet >= 0 && length < minLengthFacet)
							return false;

				}
		  }
				
			if ((totalDigitsFacet >=0) || (fractionDigitsFacet >=0)) {
				String newValue = value.Trim(new Char [] { '+', '-', '0', '.' });
				int fractionDigits = 0;
				int totalDigits = newValue.Length;
				int point = newValue.IndexOf(".");
				if (point != -1) {
					totalDigits -= 1;
					fractionDigits = newValue.Length - point -1; 
				} 
				if ((totalDigitsFacet >=0) && (totalDigits > totalDigitsFacet)) 
					return false;
				if ((fractionDigitsFacet >=0) && (fractionDigits > fractionDigitsFacet)) 
					return false;
			}
			
			if ((maxInclusiveFacet != null) ||
					(maxExclusiveFacet != null) ||
					(minInclusiveFacet != null) ||
					(minExclusiveFacet != null)) { 
				if (dt != null) {
					object parsed;
					try {
						parsed = dt.ParseValue (value, nt, null);
					} catch (OverflowException ) {
						/* This appears to be what .NET does */
						return false ;
					} catch (FormatException ) {
						/* This appears to be what .NET does */
						return false ;
					}
					
					if (maxInclusiveFacet != null) {
						XsdOrdering result = dt.Compare (parsed, maxInclusiveFacet);
						if ((result != XsdOrdering.LessThan) &&
								(result != XsdOrdering.Equal)) 
							return false;
					}
					if (maxExclusiveFacet != null) {
					
						XsdOrdering result = dt.Compare (parsed, maxExclusiveFacet);
						if (result != XsdOrdering.LessThan) 
							return false;
					}
					if (minInclusiveFacet != null) {
						XsdOrdering result = dt.Compare (parsed, minInclusiveFacet);
						if ((result != XsdOrdering.GreaterThan) &&
								(result != XsdOrdering.Equal)) 
							return false;
					}
					if (minExclusiveFacet != null) {
						XsdOrdering result = dt.Compare (parsed, minExclusiveFacet);
						if (result != XsdOrdering.GreaterThan) 
							return false;
					}

				}
			}

			// all passed
			return true;
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
