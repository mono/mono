//
// Mono.Xml.Schema.XsdValidatingReader.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
//#define IGNORE_KEYS

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
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Mono.Xml;

using ContentProc = System.Xml.Schema.XmlSchemaContentProcessing;
using XsElement = System.Xml.Schema.XmlSchemaElement;
using XsAttribute = System.Xml.Schema.XmlSchemaAttribute;
using ComplexType = System.Xml.Schema.XmlSchemaComplexType;
using SimpleType = System.Xml.Schema.XmlSchemaSimpleType;
using SimpleTypeRest = System.Xml.Schema.XmlSchemaSimpleTypeRestriction;
using SimpleTypeList = System.Xml.Schema.XmlSchemaSimpleTypeList;
using SimpleTypeUnion = System.Xml.Schema.XmlSchemaSimpleTypeUnion;
using XsDatatype = System.Xml.Schema.XmlSchemaDatatype;

namespace Mono.Xml.Schema
{
	internal class XsdValidatingReader : XmlReader, IXmlLineInfo, IHasXmlSchemaInfo, IHasXmlParserContext, IXmlNamespaceResolver
	{
		static readonly XsAttribute [] emptyAttributeArray =
			new XsAttribute [0];

		XmlReader reader;
		XmlResolver resolver;
		IHasXmlSchemaInfo sourceReaderSchemaInfo;
		IXmlLineInfo readerLineInfo;
		ValidationType validationType;
		XmlSchemaSet schemas = new XmlSchemaSet ();
		bool namespaces = true;

		bool checkIdentity = true;
		Hashtable idList = new Hashtable ();
		ArrayList missingIDReferences;
		string thisElementId;

#if !IGNORE_KEYS
		ArrayList keyTables = new ArrayList ();
		ArrayList currentKeyFieldConsumers;
		ArrayList tmpKeyrefPool;
		ArrayList elementQNameStack = new ArrayList ();
#endif

		XsdParticleStateManager state = new XsdParticleStateManager ();

		int skipValidationDepth = -1;
		int xsiNilDepth = -1;
		StringBuilder storedCharacters = new StringBuilder ();
		bool shouldValidateCharacters;

		XsAttribute [] defaultAttributes = emptyAttributeArray;
		int currentDefaultAttribute = -1;
		ArrayList defaultAttributesCache = new ArrayList ();
		bool defaultAttributeConsumed;
		XmlQualifiedName attrQName;

#region .ctor
		public XsdValidatingReader (XmlReader reader)
		{
			this.reader = reader;
			readerLineInfo = reader as IXmlLineInfo;
			sourceReaderSchemaInfo = reader as IHasXmlSchemaInfo;
		}
#endregion

		public ValidationEventHandler ValidationEventHandler;

		// Private Properties
		private XmlQualifiedName CurrentQName {
			get {
				if (attrQName == null)
					attrQName = new XmlQualifiedName (LocalName, NamespaceURI);
				return attrQName;
			}
		}

		private XsdValidationContext Context {
			get { return state.Context; }
		}

#if !IGNORE_KEYS
		internal ArrayList CurrentKeyFieldConsumers {
			get {
				if (currentKeyFieldConsumers == null)
					currentKeyFieldConsumers = new ArrayList ();
				return currentKeyFieldConsumers;
			}
		}
#endif

		private ArrayList MissingIDReferences {
			get {
				if (missingIDReferences == null)
					missingIDReferences = new ArrayList ();
				return missingIDReferences;
			}
		}

		// Public Non-overrides

		public int XsiNilDepth {
			get { return xsiNilDepth; }
		}

		public bool Namespaces {
			get { return namespaces; }
			set { namespaces = value; }
		}

		public XmlReader Reader {
			get { return reader; }
		}

		// This is required to resolve xsi:schemaLocation
		public XmlResolver XmlResolver {
			set {
				resolver = value;
			}
		}

		// This should be changed before the first Read() call.
		public XmlSchemaSet Schemas {
			get { return schemas; }
			set {
				if (ReadState != ReadState.Initial)
					throw new InvalidOperationException ("Schemas must be set before the first call to Read().");
				schemas = value;
			}
		}

		public object SchemaType {
			get {
				if (ReadState != ReadState.Interactive)
					return null;

				switch (NodeType) {
				case XmlNodeType.Element:
					if (Context.ActualType != null)
						return Context.ActualType;
					else
						return SourceReaderSchemaType;
				case XmlNodeType.Attribute:
					ComplexType ct = Context.ActualType as ComplexType;
					if (ct != null) {
						XsAttribute attdef = ct.AttributeUses [CurrentQName] as XsAttribute;
						if (attdef != null)
							return attdef.AttributeType;
					}
					return SourceReaderSchemaType;
				default:
					return SourceReaderSchemaType;
				}
			}
		}

		private object SourceReaderSchemaType {
			get { return this.sourceReaderSchemaInfo != null ? sourceReaderSchemaInfo.SchemaType : null; }
		}

		public ValidationType ValidationType {
			get { return validationType; }
			set {
				if (ReadState != ReadState.Initial)
					throw new InvalidOperationException ("ValidationType must be set before reading.");
				validationType = value;
			}
		}

		IDictionary IXmlNamespaceResolver.GetNamespacesInScope (XmlNamespaceScope scope)
		{
			IXmlNamespaceResolver resolver = reader as IXmlNamespaceResolver;
			if (resolver == null)
				throw new NotSupportedException ("The input XmlReader does not implement IXmlNamespaceResolver and thus this validating reader cannot collect in-scope namespaces.");
			return resolver.GetNamespacesInScope (scope);
		}

		string IXmlNamespaceResolver.LookupPrefix (string ns)
		{
			return ((IXmlNamespaceResolver) this).LookupPrefix (ns, false);
		}

		string IXmlNamespaceResolver.LookupPrefix (string ns, bool atomizedNames)
		{
			IXmlNamespaceResolver resolver = reader as IXmlNamespaceResolver;
			if (resolver == null)
				throw new NotSupportedException ("The input XmlReader does not implement IXmlNamespaceResolver and thus this validating reader cannot execute namespace prefix lookup.");
			return resolver.LookupPrefix (ns, atomizedNames);
		}

		// It is used only for independent XmlReader use, not for XmlValidatingReader.
#if NET_2_0
		public override object ReadTypedValue ()
#else
		public object ReadTypedValue ()
#endif
		{
			XsDatatype dt = SchemaType as XsDatatype;
			SimpleType st = SchemaType as SimpleType;
			if (st != null)
				dt = st.Datatype;
			if (dt == null)
				return null;

			switch (NodeType) {
			case XmlNodeType.Element:
				if (IsEmptyElement)
					return null;

				storedCharacters.Length = 0;
				bool loop = true;
				do {
					Read ();
					switch (NodeType) {
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
						storedCharacters.Append (Value);
						break;
					case XmlNodeType.Comment:
						break;
					default:
						loop = false;
						break;
					}
				} while (loop && !EOF && ReadState == ReadState.Interactive);
				return dt.ParseValue (storedCharacters.ToString (), NameTable, ParserContext.NamespaceManager);
			case XmlNodeType.Attribute:
				return dt.ParseValue (Value, NameTable, ParserContext.NamespaceManager);
			}
			return null;
		}

		// Public Overrided Properties

		public override int AttributeCount {
			get {
				return reader.AttributeCount + defaultAttributes.Length;
			}
		}

		public override string BaseURI {
			get { return reader.BaseURI; }
		}

		// If this class is used to implement XmlValidatingReader,
		// it should be left to DTDValidatingReader. In other cases,
		// it depends on the reader's ability.
		public override bool CanResolveEntity {
			get { return reader.CanResolveEntity; }
		}

		public override int Depth {
			get {
				if (currentDefaultAttribute < 0)
					return reader.Depth;
				if (this.defaultAttributeConsumed)
					return reader.Depth + 2;
				return reader.Depth + 1;
			}
		}

		public override bool EOF {
			get { return reader.EOF; }
		}

		public override bool HasValue {
			get {
				if (currentDefaultAttribute < 0)
					return reader.HasValue;
				return true;
			}
		}

		public override bool IsDefault {
			get {
				if (currentDefaultAttribute < 0)
					return reader.IsDefault;
				return true;
			}
		}

		public override bool IsEmptyElement {
			get {
				if (currentDefaultAttribute < 0)
					return reader.IsEmptyElement;
				return false;
			}
		}

		public override string this [int i] {
			get { return GetAttribute (i); }
		}

		public override string this [string name] {
			get { return GetAttribute (name); }
		}

		public override string this [string localName, string ns] {
			get { return GetAttribute (localName, ns); }
		}

		int IXmlLineInfo.LineNumber {
			get { return readerLineInfo != null ? readerLineInfo.LineNumber : 0; }
		}

		int IXmlLineInfo.LinePosition {
			get { return readerLineInfo != null ? readerLineInfo.LinePosition : 0; }
		}

		public override string LocalName {
			get {
				if (currentDefaultAttribute < 0)
					return reader.LocalName;
				if (defaultAttributeConsumed)
					return String.Empty;
				return defaultAttributes [currentDefaultAttribute].QualifiedName.Name;
			}
		}

		public override string Name {
			get {
				if (currentDefaultAttribute < 0)
					return reader.Name;
				if (defaultAttributeConsumed)
					return String.Empty;

				XmlQualifiedName qname = defaultAttributes [currentDefaultAttribute].QualifiedName;
				string prefix = Prefix;
				if (prefix == String.Empty)
					return qname.Name;
				else
					return String.Concat (prefix, ":", qname.Name);
			}
		}

		public override string NamespaceURI {
			get {
				if (currentDefaultAttribute < 0)
					return reader.NamespaceURI;
				if (defaultAttributeConsumed)
					return String.Empty;
				return defaultAttributes [currentDefaultAttribute].QualifiedName.Namespace;
			}
		}

		public override XmlNameTable NameTable {
			get { return reader.NameTable; }
		}

		public override XmlNodeType NodeType {
			get {
				if (currentDefaultAttribute < 0)
					return reader.NodeType;
				if (defaultAttributeConsumed)
					return XmlNodeType.Text;
				return XmlNodeType.Attribute;
			}
		}

		public XmlParserContext ParserContext {
			get { return XmlSchemaUtil.GetParserContext (reader); }
		}

		public override string Prefix {
			get {
				if (currentDefaultAttribute < 0)
					return reader.Prefix;
				if (defaultAttributeConsumed)
					return String.Empty;
				XmlQualifiedName qname = defaultAttributes [currentDefaultAttribute].QualifiedName;
				string prefix = this.ParserContext.NamespaceManager.LookupPrefix (qname.Namespace, false);
				if (prefix == null)
					return String.Empty;
				else
					return prefix;
			}
		}

		public override char QuoteChar {
			get { return reader.QuoteChar; }
		}

		public override ReadState ReadState {
			get { return reader.ReadState; }
		}

		public override string Value {
			get {
				if (currentDefaultAttribute < 0)
					return reader.Value;
				string value = defaultAttributes [currentDefaultAttribute].ValidatedDefaultValue;
				if (value == null)
					value = defaultAttributes [currentDefaultAttribute].ValidatedFixedValue;
				return value;
			}
		}

		public override string XmlLang {
			get {
				string xmlLang = reader.XmlLang;
				if (xmlLang != null)
					return xmlLang;
				int idx = this.FindDefaultAttribute ("lang", XmlNamespaceManager.XmlnsXml);
				if (idx < 0)
					return null;
				xmlLang = defaultAttributes [idx].ValidatedDefaultValue;
				if (xmlLang == null)
					xmlLang = defaultAttributes [idx].ValidatedFixedValue;
				return xmlLang;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				XmlSpace space = reader.XmlSpace;
				if (space != XmlSpace.None)
					return space;
				int idx = this.FindDefaultAttribute ("space", XmlNamespaceManager.XmlnsXml);
				if (idx < 0)
					return XmlSpace.None;
				string spaceSpec = defaultAttributes [idx].ValidatedDefaultValue;
				if (spaceSpec == null)
					spaceSpec = defaultAttributes [idx].ValidatedFixedValue;
				return (XmlSpace) Enum.Parse (typeof (XmlSpace), spaceSpec, false);
			}
		}

		// Private Methods

		private void HandleError (string error)
		{
			HandleError (error, null);
		}

		private void HandleError (string error, Exception innerException)
		{
			HandleError (error, innerException, false);
		}

		private void HandleError (string error, Exception innerException, bool isWarning)
		{
			if (ValidationType == ValidationType.None)	// extra quick check
				return;

			XmlSchemaException schemaException = new XmlSchemaException (error, 
					this, this.BaseURI, null, innerException);
			HandleError (schemaException, isWarning);
		}

		private void HandleError (XmlSchemaException schemaException)
		{
			HandleError (schemaException, false);
		}

		private void HandleError (XmlSchemaException schemaException, bool isWarning)
		{
			if (ValidationType == ValidationType.None)
				return;

			ValidationEventArgs e = new ValidationEventArgs (schemaException,
				schemaException.Message, isWarning ? XmlSeverityType.Warning : XmlSeverityType.Error);

			if (ValidationEventHandler != null)
				ValidationEventHandler (this, e);

			else if (e.Severity == XmlSeverityType.Error)
				throw e.Exception;
		}

		private XsElement FindElement (string name, string ns)
		{
			return (XsElement) schemas.GlobalElements [new XmlQualifiedName (name, ns)];
		}

		private XmlSchemaType FindType (XmlQualifiedName qname)
		{
			return (XmlSchemaType) schemas.GlobalTypes [qname];
		}

		private void ValidateStartElementParticle ()
		{
			if (Context.State == null)
				return;
			Context.XsiType = null;
			state.CurrentElement = null;
			Context.EvaluateStartElement (reader.LocalName,
				reader.NamespaceURI);
			if (Context.State == XsdValidationState.Invalid)
				HandleError ("Invalid start element: " + reader.NamespaceURI + ":" + reader.LocalName);

			Context.SetElement (state.CurrentElement);
		}

		private void ValidateEndElementParticle ()
		{
			if (Context.State != null) {
				if (!Context.EvaluateEndElement ()) {
					HandleError ("Invalid end element: " + reader.Name);
				}
			}
			state.PopContext ();
		}

		// Utility for missing validation completion related to child items.
		private void ValidateCharacters ()
		{
			if (xsiNilDepth >= 0 && xsiNilDepth < reader.Depth)
				HandleError ("Element item appeared, while current element context is nil.");

			storedCharacters.Append (reader.Value);
		}

		// Utility for missing validation completion related to child items.
		private void ValidateEndCharacters ()
		{
			if (Context.ActualType == null)
				return;

			string value = storedCharacters.ToString ();

			if (storedCharacters.Length == 0) {
				// 3.3.4 Element Locally Valid (Element) 5.1.2
				if (Context.Element != null) {
					if (Context.Element.ValidatedDefaultValue != null)
						value = Context.Element.ValidatedDefaultValue;
				}					
			}

			XsDatatype dt = Context.ActualType as XsDatatype;
			SimpleType st = Context.ActualType as SimpleType;
			if (dt == null) {
				if (st != null) {
					dt = st.Datatype;
				} else {
					ComplexType ct = Context.ActualType as ComplexType;
					dt = ct.Datatype;
					switch (ct.ContentType) {
					case XmlSchemaContentType.ElementOnly:
					case XmlSchemaContentType.Empty:
						if (storedCharacters.Length > 0)
							HandleError ("Character content not allowed.");
						break;
					}
				}
			}
			if (dt != null) {
				// 3.3.4 Element Locally Valid (Element) :: 5.2.2.2. Fixed value constraints
				if (Context.Element != null && Context.Element.ValidatedFixedValue != null)
					if (value != Context.Element.ValidatedFixedValue)
						HandleError ("Fixed value constraint was not satisfied.");
				AssessStringValid (st, dt, value);
			}

#if !IGNORE_KEYS
			// Identity field value
			if (currentKeyFieldConsumers != null) {
				while (this.currentKeyFieldConsumers.Count > 0) {
					XsdKeyEntryField field = this.currentKeyFieldConsumers [0] as XsdKeyEntryField;
					if (field.Identity != null)
						HandleError ("Two or more identical field was found. Former value is '" + field.Identity + "' .");
					object identity = null; // This means empty value
					if (dt != null) {
						try {
							identity = dt.ParseValue (value, NameTable, ParserContext.NamespaceManager);
						} catch (Exception ex) { // FIXME: (wishlist) This is bad manner ;-(
							HandleError ("Identity value is invalid against its data type " + dt.TokenizedType, ex);
						}
					}
					if (identity == null)
						identity = value;

					if (!field.SetIdentityField (identity, reader.Depth == xsiNilDepth, dt as XsdAnySimpleType, this))
						HandleError ("Two or more identical key value was found: '" + value + "' .");
					this.currentKeyFieldConsumers.RemoveAt (0);
				}
			}
#endif

			shouldValidateCharacters = false;
		}

		// 3.14.4 String Valid 
		private void AssessStringValid (SimpleType st,
			XsDatatype dt, string value)
		{
			XsDatatype validatedDatatype = dt;
			if (st != null) {
				string normalized = validatedDatatype.Normalize (value);
				string [] values;
				XsDatatype itemDatatype;
				SimpleType itemSimpleType;
				switch (st.DerivedBy) {
				case XmlSchemaDerivationMethod.List:
					SimpleTypeList listContent = st.Content as SimpleTypeList;
					values = normalized.Split (XmlChar.WhitespaceChars);
					itemDatatype = listContent.ValidatedListItemType as XsDatatype;
					itemSimpleType = listContent.ValidatedListItemType as SimpleType;
					for (int vi = 0; vi < values.Length; vi++) {
						string each = values [vi];
						if (each == String.Empty)
							continue;
						// validate against ValidatedItemType
						if (itemDatatype != null) {
							try {
								itemDatatype.ParseValue (each, NameTable, ParserContext.NamespaceManager);
							} catch (Exception ex) { // FIXME: (wishlist) better exception handling ;-(
								HandleError ("List type value contains one or more invalid values.", ex);
								break;
							}
						}
						else
							AssessStringValid (itemSimpleType, itemSimpleType.Datatype, each);
					}
					break;
				case XmlSchemaDerivationMethod.Union:
					SimpleTypeUnion union = st.Content as SimpleTypeUnion;
					{
						string each = normalized;
						// validate against ValidatedItemType
						bool passed = false;
						foreach (object eachType in union.ValidatedTypes) {
							itemDatatype = eachType as XsDatatype;
							itemSimpleType = eachType as SimpleType;
							if (itemDatatype != null) {
								try {
									itemDatatype.ParseValue (each, NameTable, ParserContext.NamespaceManager);
								} catch (Exception) { // FIXME: (wishlist) better exception handling ;-(
									continue;
								}
							}
							else {
								try {
									AssessStringValid (itemSimpleType, itemSimpleType.Datatype, each);
								} catch (XmlSchemaException) {
									continue;
								}
							}
							passed = true;
							break;
						}
						if (!passed) {
							HandleError ("Union type value contains one or more invalid values.");
							break;
						}
					}
					break;
				case XmlSchemaDerivationMethod.Restriction:
					SimpleTypeRest str = st.Content as SimpleTypeRest;
					// facet validation
					if (str != null) {
						/* Don't forget to validate against inherited type's facets 
						 * Could we simplify this by assuming that the basetype will also
						 * be restriction?
						 * */
						 // mmm, will check later.
						SimpleType baseType = st.BaseXmlSchemaType as SimpleType;
						if (baseType != null) {
							 AssessStringValid(baseType, dt, normalized);
						}
						if (!str.ValidateValueWithFacets (normalized, NameTable)) {
							HandleError ("Specified value was invalid against the facets.");
							break;
						}
					}
					validatedDatatype = st.Datatype;
					break;
				}
			}
			if (validatedDatatype != null) {
				try {
					validatedDatatype.ParseValue (value, NameTable, ParserContext.NamespaceManager);
				} catch (Exception ex) {	// FIXME: (wishlist) It is bad manner ;-(
					HandleError ("Invalidly typed data was specified.", ex);
				}
			}
		}

		private object GetXsiType (string name)
		{
			object xsiType = null;
			XmlQualifiedName typeQName =
				XmlQualifiedName.Parse (name, this);
			if (typeQName == ComplexType.AnyTypeName)
				xsiType = ComplexType.AnyType;
			else if (XmlSchemaUtil.IsBuiltInDatatypeName (typeQName))
				xsiType = XsDatatype.FromName (typeQName);
			else
				xsiType = FindType (typeQName);
			return xsiType;
		}

		// It is common to ElementLocallyValid::4 and SchemaValidityAssessment::1.2.1.2.4
		private void AssessLocalTypeDerivationOK (object xsiType, object baseType, XmlSchemaDerivationMethod flag)
		{
			XmlSchemaType xsiSchemaType = xsiType as XmlSchemaType;
			ComplexType baseComplexType = baseType as ComplexType;
			ComplexType xsiComplexType = xsiSchemaType as ComplexType;
			if (xsiType != baseType) {
				// Extracted (not extraneous) check for 3.4.6 TypeDerivationOK.
				if (baseComplexType != null)
					flag |= baseComplexType.BlockResolved;
				if (flag == XmlSchemaDerivationMethod.All) {
					HandleError ("Prohibited element type substitution.");
					return;
				} else if (xsiSchemaType != null && (flag & xsiSchemaType.DerivedBy) != 0) {
					HandleError ("Prohibited element type substitution.");
					return;
				}
			}

			if (xsiComplexType != null)
				try {
					xsiComplexType.ValidateTypeDerivationOK (baseType, null, null);
				} catch (XmlSchemaException ex) {
//					HandleError ("Locally specified schema complex type derivation failed. " + ex.Message, ex);
					HandleError (ex);
				}
			else {
				SimpleType xsiSimpleType = xsiType as SimpleType;
				if (xsiSimpleType != null) {
					try {
						xsiSimpleType.ValidateTypeDerivationOK (baseType, null, null, true);
					} catch (XmlSchemaException ex) {
//						HandleError ("Locally specified schema simple type derivation failed. " + ex.Message, ex);
						HandleError (ex);
					}
				}
				else if (xsiType is XsDatatype) {
					// do nothing
				}
				else
					HandleError ("Primitive data type cannot be derived type using xsi:type specification.");
			}
		}

		// Section 3.3.4 of the spec.
		private void AssessStartElementSchemaValidity ()
		{
			// If the reader is inside xsi:nil (and failed
			// on validation), then simply skip its content.
			if (xsiNilDepth >= 0 && xsiNilDepth < reader.Depth)
				HandleError ("Element item appeared, while current element context is nil.");

			ValidateStartElementParticle ();

			string xsiNilValue = reader.GetAttribute ("nil", XmlSchema.InstanceNamespace);
			if (xsiNilValue != null)
				xsiNilValue = xsiNilValue.Trim (XmlChar.WhitespaceChars);
			bool isXsiNil = xsiNilValue == "true";
			if (isXsiNil && this.xsiNilDepth < 0)
				xsiNilDepth = reader.Depth;

			// [Schema Validity Assessment (Element) 1.2]
			// Evaluate "local type definition" from xsi:type.
			// (See spec 3.3.4 Schema Validity Assessment (Element) 1.2.1.2.3.
			// Note that Schema Validity Assessment(Element) 1.2 takes
			// precedence than 1.1 of that.

			string xsiTypeName = reader.GetAttribute ("type", XmlSchema.InstanceNamespace);
			if (xsiTypeName != null) {
				xsiTypeName = xsiTypeName.Trim (XmlChar.WhitespaceChars);
				object xsiType = GetXsiType (xsiTypeName);
				if (xsiType == null)
					HandleError ("The instance type was not found: " + xsiTypeName + " .");
				else {
					XmlSchemaType xsiSchemaType = xsiType as XmlSchemaType;
					if (xsiSchemaType != null && this.Context.Element != null) {
						XmlSchemaType elemBaseType = Context.Element.ElementType as XmlSchemaType;
						if (elemBaseType != null && (xsiSchemaType.DerivedBy & elemBaseType.FinalResolved) != 0)
							HandleError ("The instance type is prohibited by the type of the context element.");
						if (elemBaseType != xsiType && (xsiSchemaType.DerivedBy & this.Context.Element.BlockResolved) != 0)
							HandleError ("The instance type is prohibited by the context element.");
					}
					ComplexType xsiComplexType = xsiType as ComplexType;
					if (xsiComplexType != null && xsiComplexType.IsAbstract)
						HandleError ("The instance type is abstract: " + xsiTypeName + " .");
					else {
						// If current schema type exists, then this xsi:type must be
						// valid extension of that type. See 1.2.1.2.4.
						if (Context.Element != null) {
							AssessLocalTypeDerivationOK (xsiType, Context.Element.ElementType, Context.Element.BlockResolved);
						}
						AssessStartElementLocallyValidType (xsiType);	// 1.2.2:
						Context.XsiType = xsiType;
					}
				}
			}

			// Create Validation Root, if not exist.
			// [Schema Validity Assessment (Element) 1.1]
			if (Context.Element == null) {
				state.CurrentElement = FindElement (reader.LocalName, reader.NamespaceURI);
				Context.SetElement (state.CurrentElement);
			}
			if (Context.Element != null) {
				if (xsiTypeName == null) {
					AssessElementLocallyValidElement (Context.Element, xsiNilValue);	// 1.1.2
				}
			} else {
				switch (state.ProcessContents) {
				case ContentProc.Skip:
					break;
				case ContentProc.Lax:
					break;
				default:
					if (xsiTypeName == null &&
						(schemas.Contains (reader.NamespaceURI) ||
						!schemas.MissedSubComponents (reader.NamespaceURI)))
						HandleError ("Element declaration for " + reader.LocalName + " is missing.");
					break;
				}
			}

			state.PushContext ();

			XsdValidationState next = null;
			if (state.ProcessContents == ContentProc.Skip)
				skipValidationDepth = reader.Depth;
			else {
				// create child particle state.
				ComplexType xsComplexType = SchemaType as ComplexType;
				if (xsComplexType != null)
					next = state.Create (xsComplexType.ValidatableParticle);
				else if (state.ProcessContents == ContentProc.Lax)
					next = state.Create (XmlSchemaAny.AnyTypeContent);
				else
					next = state.Create (XmlSchemaParticle.Empty);
			}
			Context.State = next;

#if !IGNORE_KEYS
			AssessStartIdentityConstraints ();
#endif

		}

		// 3.3.4 Element Locally Valid (Element)
		private void AssessElementLocallyValidElement (XsElement element, string xsiNilValue)
		{
			XmlQualifiedName qname = new XmlQualifiedName (reader.LocalName, reader.NamespaceURI);
			// 1.
			if (element == null)
				HandleError ("Element declaration is required for " + qname);
			// 2.
			if (element.ActualIsAbstract)
				HandleError ("Abstract element declaration was specified for " + qname);
			// 3.1.
			if (!element.ActualIsNillable && xsiNilValue != null)
				HandleError ("This element declaration is not nillable: " + qname);
			// 3.2.
			// Note that 3.2.1 xsi:nil constraints are to be validated in
			else if (xsiNilValue == "true") {
				// AssessElementSchemaValidity() and ValidateCharacters()

				if (element.ValidatedFixedValue != null)
					HandleError ("Schema instance nil was specified, where the element declaration for " + qname + "has fixed value constraints.");
			}
			// 4.
			string xsiType = reader.GetAttribute ("type", XmlSchema.InstanceNamespace);
			if (xsiType != null) {
				Context.XsiType = GetXsiType (xsiType);
				AssessLocalTypeDerivationOK (Context.XsiType, element.ElementType, element.BlockResolved);
			}
			else
				Context.XsiType = null;

			// 5 Not all things cannot be assessed here.
			// It is common to 5.1 and 5.2
			if (element.ElementType != null)
				AssessStartElementLocallyValidType (SchemaType);

			// 6. should be out from here.
			// See invokation of AssessStartIdentityConstraints().

			// 7 is going to be validated in Read() (in case of xmlreader's EOF).
		}

		// 3.3.4 Element Locally Valid (Type)
		private void AssessStartElementLocallyValidType (object schemaType)
		{
			if (schemaType == null) {	// 1.
				HandleError ("Schema type does not exist.");
				return;
			}
			ComplexType cType = schemaType as ComplexType;
			SimpleType sType = schemaType as SimpleType;
			if (sType != null) {
				// 3.1.1.
				while (reader.MoveToNextAttribute ()) {
					if (reader.NamespaceURI == XmlNamespaceManager.XmlnsXmlns)
						continue;
					if (reader.NamespaceURI != XmlSchema.InstanceNamespace)
						HandleError ("Current simple type cannot accept attributes other than schema instance namespace.");
					switch (reader.LocalName) {
					case "type":
					case "nil":
					case "schemaLocation":
					case "noNamespaceSchemaLocation":
						break;
					default:
						HandleError ("Unknown schema instance namespace attribute: " + reader.LocalName);
						break;
					}
				}
				reader.MoveToElement ();
				// 3.1.2 and 3.1.3 cannot be assessed here.
			} else if (cType != null) {
				if (cType.IsAbstract) {	// 2.
					HandleError ("Target complex type is abstract.");
					return;
				}
				// 3.2
				AssessElementLocallyValidComplexType (cType);
			}
		}

		// 3.4.4 Element Locally Valid (Complex Type)
		private void AssessElementLocallyValidComplexType (ComplexType cType)
		{
			// 1.
			if (cType.IsAbstract)
				HandleError ("Target complex type is abstract.");

			// 2 (xsi:nil and content prohibition)
			// See AssessStartElementSchemaValidity() and ValidateCharacters()

			// 3. attribute uses and 
			// 5. wild IDs
			if (reader.MoveToFirstAttribute ()) {
				do {
					switch (reader.NamespaceURI) {
					case"http://www.w3.org/2000/xmlns/":
					case XmlSchema.InstanceNamespace:
						continue;
					}
					XmlQualifiedName qname = new XmlQualifiedName (reader.LocalName, reader.NamespaceURI);
					XmlSchemaObject attMatch = FindAttributeDeclaration (cType, qname);
					if (attMatch == null)
						HandleError ("Attribute declaration was not found for " + qname);
					XsAttribute attdecl = attMatch as XsAttribute;
					if (attdecl != null) {
						AssessAttributeLocallyValidUse (attdecl);
						AssessAttributeLocallyValid (attdecl);
					} // otherwise anyAttribute or null.
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}

			// Collect default attributes.
			// 4.
			foreach (DictionaryEntry entry in cType.AttributeUses) {
				XsAttribute attr = (XsAttribute) entry.Value;
				if (reader [attr.QualifiedName.Name, attr.QualifiedName.Namespace] == null) {
					if (attr.ValidatedUse == XmlSchemaUse.Required && 
						attr.ValidatedFixedValue == null)
						HandleError ("Required attribute " + attr.QualifiedName + " was not found.");
					else if (attr.ValidatedDefaultValue != null || attr.ValidatedFixedValue != null)
						defaultAttributesCache.Add (attr);
				}
			}
			if (defaultAttributesCache.Count == 0)
				defaultAttributes = emptyAttributeArray;
			else
				defaultAttributes = (XsAttribute []) 
					defaultAttributesCache.ToArray (
						typeof (XsAttribute));
			defaultAttributesCache.Clear ();
			// 5. wild IDs was already checked above.
		}

		// Spec 3.10.4 Item Valid (Wildcard)
		private static bool AttributeWildcardItemValid (XmlSchemaAnyAttribute anyAttr, XmlQualifiedName qname, string ns)
		{
			if (anyAttr.HasValueAny)
				return true;
			if (anyAttr.HasValueOther && (anyAttr.TargetNamespace == "" || ns != anyAttr.TargetNamespace))
				return true;
			if (anyAttr.HasValueTargetNamespace && ns == anyAttr.TargetNamespace)
				return true;
			if (anyAttr.HasValueLocal && ns == "")
				return true;
			for (int i = 0; i < anyAttr.ResolvedNamespaces.Count; i++)
				if (anyAttr.ResolvedNamespaces [i] == ns)
					return true;
			return false;
		}

		private XmlSchemaObject FindAttributeDeclaration (
			ComplexType cType,
			XmlQualifiedName qname)
		{
			XmlSchemaObject result = cType.AttributeUses [qname];
			if (result != null)
				return result;
			if (cType.AttributeWildcard == null)
				return null;

			if (!AttributeWildcardItemValid (cType.AttributeWildcard, qname, reader.NamespaceURI))
				return null;

			if (cType.AttributeWildcard.ResolvedProcessContents == ContentProc.Skip)
				return cType.AttributeWildcard;
			XsAttribute attr = schemas.GlobalAttributes [qname] as XsAttribute;
			if (attr != null)
				return attr;
			if (cType.AttributeWildcard.ResolvedProcessContents == ContentProc.Lax)
				return cType.AttributeWildcard;
			else
				return null;
		}

		// 3.2.4 Attribute Locally Valid and 3.4.4
		private void AssessAttributeLocallyValid (XsAttribute attr)
		{
			// 2. - 4.
			if (attr.AttributeType == null)
				HandleError ("Attribute type is missing for " + attr.QualifiedName);
			XsDatatype dt = attr.AttributeType as XsDatatype;
			if (dt == null)
				dt = ((SimpleType) attr.AttributeType).Datatype;
			// It is a bit heavy process, so let's omit as long as possible ;-)
			if (dt != SimpleType.AnySimpleType || attr.ValidatedFixedValue != null) {
				string normalized = dt.Normalize (reader.Value);
				object parsedValue = null;
				try {
					parsedValue = dt.ParseValue (normalized, reader.NameTable, this.ParserContext.NamespaceManager);
				} catch (Exception ex) { // FIXME: (wishlist) It is bad manner ;-(
					HandleError ("Attribute value is invalid against its data type " + dt.TokenizedType, ex);
				}
				if (attr.ValidatedFixedValue != null && attr.ValidatedFixedValue != normalized) {
					HandleError ("The value of the attribute " + attr.QualifiedName + " does not match with its fixed value.");
					parsedValue = dt.ParseValue (attr.ValidatedFixedValue, reader.NameTable, this.ParserContext.NamespaceManager);
				}
				if (this.checkIdentity)
					AssessEachAttributeIdentityConstraint (dt, parsedValue);
			}
		}

		// 3.4.4-5 wild IDs
		private void AssessEachAttributeIdentityConstraint (
			XsDatatype dt, object parsedValue)
		{
			// Validate identity constraints.
			string str = parsedValue as string;
			switch (dt.TokenizedType) {
			case XmlTokenizedType.ID:
				if (thisElementId != null)
					HandleError ("ID type attribute was already assigned in the containing element.");
				thisElementId = str;
				if (idList.Contains (str))
					HandleError ("Duplicate ID value was found.");
				else
					idList.Add (str, str);
				if (MissingIDReferences.Contains (str))
					MissingIDReferences.Remove (str);
				break;
			case XmlTokenizedType.IDREF:
				if (!idList.Contains (str))
					MissingIDReferences.Add (str);
				break;
			case XmlTokenizedType.IDREFS:
				string [] idrefs = (string []) parsedValue;
				for (int i = 0; i < idrefs.Length; i++) {
					string id = idrefs [i];
					if (!idList.Contains (id))
						MissingIDReferences.Add (id);
				}
				break;
			}
		}

		private void AssessAttributeLocallyValidUse (XsAttribute attr)
		{
			// This is extra check than spec 3.5.4
			if (attr.ValidatedUse == XmlSchemaUse.Prohibited)
				HandleError ("Attribute " + attr.QualifiedName + " is prohibited in this context.");
		}

		private void AssessEndElementSchemaValidity ()
		{
			ValidateEndElementParticle ();	// validate against childrens' state.

			if (shouldValidateCharacters) {
				ValidateEndCharacters ();
				shouldValidateCharacters = false;
			}

#if !IGNORE_KEYS
			// 3.3.4 Assess ElementLocallyValidElement 5: value constraints.
			// 3.3.4 Assess ElementLocallyValidType 3.1.3. = StringValid(3.14.4)
			// => ValidateEndCharacters().

			// Reset Identity constraints.
			for (int i = 0; i < keyTables.Count; i++) {
				XsdKeyTable keyTable = this.keyTables [i] as XsdKeyTable;
				if (keyTable.StartDepth == reader.Depth) {
					EndIdentityValidation (keyTable);
				} else {
					for (int k = 0; k < keyTable.Entries.Count; k++) {
						XsdKeyEntry entry = keyTable.Entries [k] as XsdKeyEntry;
						// Remove finished (maybe key not found) entries.
						if (entry.StartDepth == reader.Depth) {
							if (entry.KeyFound)
								keyTable.FinishedEntries.Add (entry);
							else if (entry.KeySequence.SourceSchemaIdentity is XmlSchemaKey)
								HandleError ("Key sequence is missing.");
							keyTable.Entries.RemoveAt (k);
							k--;
						}
						// Pop validated key depth to find two or more fields.
						else {
							for (int j = 0; j < entry.KeyFields.Count; j++) {
								XsdKeyEntryField kf = entry.KeyFields [j];
								if (!kf.FieldFound && kf.FieldFoundDepth == reader.Depth) {
									kf.FieldFoundDepth = 0;
									kf.FieldFoundPath = null;
								}
							}
						}
					}
				}
			}
			for (int i = 0; i < keyTables.Count; i++) {
				XsdKeyTable keyseq = this.keyTables [i] as XsdKeyTable;
				if (keyseq.StartDepth == reader.Depth) {
					keyTables.RemoveAt (i);
					i--;
				}
			}
#endif

			// Reset xsi:nil, if required.
			if (xsiNilDepth == reader.Depth)
				xsiNilDepth = -1;
		}

#if !IGNORE_KEYS
		// 3.11.4 Identity Constraint Satisfied
		private void AssessStartIdentityConstraints ()
		{
			if (tmpKeyrefPool != null)
				tmpKeyrefPool.Clear ();
			if (Context.Element != null && Context.Element.Constraints.Count > 0) {
				// (a) Create new key sequences, if required.
				for (int i = 0; i < Context.Element.Constraints.Count; i++) {
					XmlSchemaIdentityConstraint ident = (XmlSchemaIdentityConstraint) Context.Element.Constraints [i];
					XsdKeyTable seq = CreateNewKeyTable (ident);
					if (ident is XmlSchemaKeyref) {
						if (tmpKeyrefPool == null)
							tmpKeyrefPool = new ArrayList ();
						tmpKeyrefPool.Add (seq);
					}
				}
			}

			// (b) Evaluate current key sequences.
			for (int i = 0; i < keyTables.Count; i++) {
				XsdKeyTable seq  = (XsdKeyTable) keyTables [i];
				if (seq.SelectorMatches (this.elementQNameStack, reader) != null) {
					// creates and registers new entry.
					XsdKeyEntry entry = new XsdKeyEntry (seq, reader);
					seq.Entries.Add (entry);
				}
			}

			// (c) Evaluate field paths.
			for (int i = 0; i < keyTables.Count; i++) {
				XsdKeyTable seq  = (XsdKeyTable) keyTables [i];
				// If possible, create new field entry candidates.
				for (int j = 0; j < seq.Entries.Count; j++) {
					XsdKeyEntry entry = seq.Entries [j] as XsdKeyEntry;
					try {
						entry.FieldMatches (this.elementQNameStack, this);
					} catch (Exception ex) { // FIXME: (wishlist) It is bad manner ;-(
						HandleError ("Identity field value is invalid against its data type.", ex);
					}
				}
			}
		}

		private XsdKeyTable CreateNewKeyTable (XmlSchemaIdentityConstraint ident)
		{
			XsdKeyTable seq = new XsdKeyTable (ident, this);
			seq.StartDepth = reader.Depth;
			this.keyTables.Add (seq);
			return seq;
		}

		private void EndIdentityValidation (XsdKeyTable seq)
		{
			ArrayList errors = new ArrayList ();
			for (int i = 0; i < seq.Entries.Count; i++) {
				XsdKeyEntry entry = (XsdKeyEntry) seq.Entries [i];
				if (entry.KeyFound)
					continue;
				if (seq.SourceSchemaIdentity is XmlSchemaKey)
					errors.Add ("line " + entry.SelectorLineNumber + "position " + entry.SelectorLinePosition);
			}
			if (errors.Count > 0)
				HandleError ("Invalid identity constraints were found. Key was not found. "
					+ String.Join (", ", errors.ToArray (typeof (string)) as string []));

			errors.Clear ();
			// Find reference target
			XmlSchemaKeyref xsdKeyref = seq.SourceSchemaIdentity as XmlSchemaKeyref;
			if (xsdKeyref != null) {
				for (int i = this.keyTables.Count - 1; i >= 0; i--) {
					XsdKeyTable target = this.keyTables [i] as XsdKeyTable;
					if (target.SourceSchemaIdentity == xsdKeyref.Target) {
						seq.ReferencedKey = target;
						for (int j = 0; j < seq.FinishedEntries.Count; j++) {
							XsdKeyEntry entry = (XsdKeyEntry) seq.FinishedEntries [j];
							for (int k = 0; k < target.FinishedEntries.Count; k++) {
								XsdKeyEntry targetEntry = (XsdKeyEntry) target.FinishedEntries [k];
								if (entry.CompareIdentity (targetEntry)) {
									entry.KeyRefFound = true;
									break;
								}
							}
						}
					}
				}
				if (seq.ReferencedKey == null)
					HandleError ("Target key was not found.");
				for (int i = 0; i < seq.FinishedEntries.Count; i++) {
					XsdKeyEntry entry = (XsdKeyEntry) seq.FinishedEntries [i];
					if (!entry.KeyRefFound)
						errors.Add (" line " + entry.SelectorLineNumber + ", position " + entry.SelectorLinePosition);
				}
				if (errors.Count > 0)
					HandleError ("Invalid identity constraints were found. Referenced key was not found: "
						+ String.Join (" / ", errors.ToArray (typeof (string)) as string []));
			}
		}
#endif

		// Overrided Methods

		public override void Close ()
		{
			reader.Close ();
		}

		public override string GetAttribute (int i)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.GetAttribute (i);
			}

			if (reader.AttributeCount > i)
				reader.GetAttribute (i);
			int defIdx = i - reader.AttributeCount;
			if (i < AttributeCount)
				return defaultAttributes [defIdx].DefaultValue;

			throw new ArgumentOutOfRangeException ("i", i, "Specified attribute index is out of range.");
		}

		public override string GetAttribute (string name)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.GetAttribute (name);
			}

			string value = reader.GetAttribute (name);
			if (value != null)
				return value;

			XmlQualifiedName qname = SplitQName (name);
			return GetDefaultAttribute (qname.Name, qname.Namespace);
		}

		private XmlQualifiedName SplitQName (string name)
		{
			if (!XmlChar.IsName (name))
				throw new ArgumentException ("Invalid name was specified.", "name");

			Exception ex = null;
			XmlQualifiedName qname = XmlSchemaUtil.ToQName (reader, name, out ex);
			if (ex != null)
				return XmlQualifiedName.Empty;
			else
				return qname;
		}

		public override string GetAttribute (string localName, string ns)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.GetAttribute (localName, ns);
			}

			string value = reader.GetAttribute (localName, ns);
			if (value != null)
				return value;

			return GetDefaultAttribute (localName, ns);
		}

		private string GetDefaultAttribute (string localName, string ns)
		{
			int idx = this.FindDefaultAttribute (localName, ns);
			if (idx < 0)
				return null;
			string value = defaultAttributes [idx].ValidatedDefaultValue;
			if (value == null)
				value = defaultAttributes [idx].ValidatedFixedValue;
			return value;
		}

		private int FindDefaultAttribute (string localName, string ns)
		{
			for (int i = 0; i < this.defaultAttributes.Length; i++) {
				XsAttribute attr = defaultAttributes [i];
				if (attr.QualifiedName.Name == localName &&
					(ns == null || attr.QualifiedName.Namespace == ns))
					return i;
			}
			return -1;
		}

		bool IXmlLineInfo.HasLineInfo ()
		{
			return readerLineInfo != null && readerLineInfo.HasLineInfo ();
		}

		public override string LookupNamespace (string prefix)
		{
			return reader.LookupNamespace (prefix);
		}

		string IXmlNamespaceResolver.LookupNamespace (string prefix, bool atomizedNames)
		{
			IXmlNamespaceResolver res = reader as IXmlNamespaceResolver;
			if (res != null)
				return res.LookupNamespace (prefix, atomizedNames);
			else
				return reader.LookupNamespace (prefix);
		}

		public override void MoveToAttribute (int i)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				reader.MoveToAttribute (i);
				return;
			}

			attrQName = null;
			if (i < reader.AttributeCount) {
				reader.MoveToAttribute (i);
				this.currentDefaultAttribute = -1;
				this.defaultAttributeConsumed = false;
			}

			if (i < AttributeCount) {
				this.currentDefaultAttribute = i - reader.AttributeCount;
				this.defaultAttributeConsumed = false;
			}
			else
				throw new ArgumentOutOfRangeException ("i", i, "Attribute index is out of range.");
		}

		public override bool MoveToAttribute (string name)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.MoveToAttribute (name);
			}

			attrQName = null;
			bool b = reader.MoveToAttribute (name);
			if (b) {
				this.currentDefaultAttribute = -1;
				this.defaultAttributeConsumed = false;
				return true;
			}

			return MoveToDefaultAttribute (name, null);
		}

		public override bool MoveToAttribute (string localName, string ns)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.MoveToAttribute (localName, ns);
			}

			attrQName = null;
			bool b = reader.MoveToAttribute (localName, ns);
			if (b) {
				this.currentDefaultAttribute = -1;
				this.defaultAttributeConsumed = false;
				return true;
			}

			return MoveToDefaultAttribute (localName, ns);
		}

		private bool MoveToDefaultAttribute (string localName, string ns)
		{
			int idx = this.FindDefaultAttribute (localName, ns);
			if (idx < 0)
				return false;
			currentDefaultAttribute = idx;
			defaultAttributeConsumed = false;
			return true;
		}

		public override bool MoveToElement ()
		{
			currentDefaultAttribute = -1;
			defaultAttributeConsumed = false;
			attrQName = null;
			return reader.MoveToElement ();
		}

		public override bool MoveToFirstAttribute ()
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.MoveToFirstAttribute ();
			}

			attrQName = null;
			if (reader.AttributeCount > 0) {
				bool b = reader.MoveToFirstAttribute ();
				if (b) {
					currentDefaultAttribute = -1;
					defaultAttributeConsumed = false;
				}
				return b;
			}

			if (this.defaultAttributes.Length > 0) {
				currentDefaultAttribute = 0;
				defaultAttributeConsumed = false;
				return true;
			}
			else
				return false;
		}

		public override bool MoveToNextAttribute ()
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.MoveToNextAttribute ();
			}

			attrQName = null;
			if (currentDefaultAttribute >= 0) {
				if (defaultAttributes.Length == currentDefaultAttribute + 1)
					return false;
				currentDefaultAttribute++;
				defaultAttributeConsumed = false;
				return true;
			}

			bool b = reader.MoveToNextAttribute ();
			if (b) {
				currentDefaultAttribute = -1;
				defaultAttributeConsumed = false;
				return true;
			}

			if (defaultAttributes.Length > 0) {
				currentDefaultAttribute = 0;
				defaultAttributeConsumed = false;
				return true;
			}
			else
				return false;
		}

		private void ExamineAdditionalSchema ()
		{
			XmlSchema schema = null;
			string schemaLocation = reader.GetAttribute ("schemaLocation", XmlSchema.InstanceNamespace);
			bool schemaAdded = false;
			if (schemaLocation != null) {
				string [] tmp = null;
				try {
					schemaLocation = XsDatatype.FromName ("token", XmlSchema.Namespace).Normalize (schemaLocation);
					tmp = schemaLocation.Split (XmlChar.WhitespaceChars);
				} catch (Exception ex) {
					HandleError ("Invalid schemaLocation attribute format.", ex, true);
					tmp = new string [0];
				}
				if (tmp.Length % 2 != 0)
					HandleError ("Invalid schemaLocation attribute format.");
				for (int i = 0; i < tmp.Length; i += 2) {
					Uri absUri = null;
					XmlTextReader xtr = null;
					try {
						absUri = new Uri ((this.BaseURI != "" ? new Uri (BaseURI) : null), tmp [i + 1]);
						xtr = new XmlTextReader (absUri.ToString (), NameTable);
						schema = XmlSchema.Read (xtr, null);
					} catch (Exception) { // FIXME: (wishlist) It is bad manner ;-(
						HandleError ("Could not resolve schema location URI: " + absUri, null, true);
						continue;
					} finally {
						if (xtr != null)
							xtr.Close ();
					}
					if (schema.TargetNamespace == null)
						schema.TargetNamespace = tmp [i];
					else if (schema.TargetNamespace != tmp [i])
						HandleError ("Specified schema has different target namespace.");
				}
			}
			if (schema != null) {
				if (!schemas.Contains (schema.TargetNamespace)) {
					schemaAdded = true;
					schemas.Add (schema);
				}
			}
			schema = null;
			string noNsSchemaLocation = reader.GetAttribute ("noNamespaceSchemaLocation", XmlSchema.InstanceNamespace);
			if (noNsSchemaLocation != null) {
				Uri absUri = null;
				XmlTextReader xtr = null;
				try {
					absUri = new Uri ((this.BaseURI != "" ? new Uri (BaseURI) : null), noNsSchemaLocation);
					xtr = new XmlTextReader (absUri.ToString (), NameTable);
					schema = XmlSchema.Read (xtr, null);
				} catch (Exception) { // FIXME: (wishlist) It is bad manner ;-(
					HandleError ("Could not resolve schema location URI: " + absUri, null, true);
				} finally {
					if (xtr != null)
						xtr.Close ();
				}
				if (schema != null && schema.TargetNamespace != null)
					HandleError ("Specified schema has different target namespace.");
			}
			if (schema != null) {
				if (!schemas.Contains (schema.TargetNamespace)) {
					schemaAdded = true;
					schemas.Add (schema);
				}
			}
			// FIXME: should call Reprocess()?
			if (schemaAdded)
				schemas.Compile ();
		}

		private bool HasMissingIDReferences ()
		{
			return missingIDReferences != null
				&& missingIDReferences.Count > 0;
		}

		public override bool Read ()
		{
			currentDefaultAttribute = -1;
			defaultAttributeConsumed = false;
			attrQName = null;
			if (this.checkIdentity)
				thisElementId = null;
			defaultAttributes = emptyAttributeArray;

			bool result = reader.Read ();
			// 3.3.4 ElementLocallyValidElement 7 = Root Valid.
			if (!result && this.checkIdentity &&
				HasMissingIDReferences ())
				HandleError ("There are missing ID references: " +
					String.Join (" ",
					this.missingIDReferences.ToArray (typeof (string)) as string []));

			// FIXME: schemaLocation could be specified 
			// at any Depth.
			if (reader.Depth == 0 &&
				reader.NodeType == XmlNodeType.Element)
				ExamineAdditionalSchema ();
			if (schemas.Count == 0)
				return result;
			if (!schemas.IsCompiled)
				schemas.Compile ();

			switch (reader.NodeType) {
			case XmlNodeType.Element:
#if !IGNORE_KEYS
				this.elementQNameStack.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI));
#endif

				// If there is no schema information, then no validation is performed.
				if (skipValidationDepth < 0 || reader.Depth <= skipValidationDepth) {
					if (shouldValidateCharacters) {
						ValidateEndCharacters ();
						shouldValidateCharacters = false;
					}
					AssessStartElementSchemaValidity ();
					storedCharacters.Length = 0;
				}

				if (reader.IsEmptyElement)
					goto case XmlNodeType.EndElement;
				else
					shouldValidateCharacters = true;
				break;
			case XmlNodeType.EndElement:
				if (reader.Depth == skipValidationDepth)
					skipValidationDepth = -1;
				else if (skipValidationDepth < 0 || reader.Depth <= skipValidationDepth)
					AssessEndElementSchemaValidity ();

				storedCharacters.Length = 0;
				elementQNameStack.RemoveAt (elementQNameStack.Count - 1);
				break;

			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Text:
				ComplexType ct = Context.ActualType as ComplexType;
				if (ct != null && storedCharacters.Length > 0) {
					switch (ct.ContentType) {
					case XmlSchemaContentType.ElementOnly:
					case XmlSchemaContentType.Empty:
						HandleError ("Not allowed character content was found.");
						break;
					}
				}

				ValidateCharacters ();
				break;
			}

			return result;
		}

		public override bool ReadAttributeValue ()
		{
			if (currentDefaultAttribute < 0)
				return reader.ReadAttributeValue ();

			if (this.defaultAttributeConsumed)
				return false;

			defaultAttributeConsumed = true;
			return true;
		}

#if NET_1_0
		public override string ReadInnerXml ()
		{
			// MS.NET 1.0 has a serious bug here. It skips validation.
			return reader.ReadInnerXml ();
		}

		public override string ReadOuterXml ()
		{
			// MS.NET 1.0 has a serious bug here. It skips validation.
			return reader.ReadOuterXml ();
		}
#endif

		// XmlReader.ReadString() should call derived this.Read().
		public override string ReadString ()
		{
#if NET_1_0
			return reader.ReadString ();
#else
			return base.ReadString ();
#endif
		}

		// This class itself does not have this feature.
		public override void ResolveEntity ()
		{
			reader.ResolveEntity ();
		}
	}

	internal class XsdValidationContext
	{
		public XsdValidationContext ()
		{
		}

		// Some of them might be missing (See the spec section 5.3, and also 3.3.4).
		public XsElement Element;
		public XsdValidationState State;
		public object XsiType; // xsi:type

		// Note that it represents current element's type.
		public object ActualType {
			get {
				if (XsiType != null)
					return XsiType;
				else
					return Element != null ? Element.ElementType : null;
			}
		}

		public object Clone ()
		{
			return MemberwiseClone ();
		}

		public void EvaluateStartElement (
			string localName, string ns)
		{
			State = State.EvaluateStartElement (localName, ns);
		}

		public bool EvaluateEndElement ()
		{
			return State.EvaluateEndElement ();
		}

		public void SetElement (XsElement element)
		{
			Element = element;
		}
	}
}
