//
// XmlSchemaValidator.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2004 Novell Inc,
//

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

//
// LAMESPEC:
//	- There is no assurance that xsi:type precedes to any other attributes,
//	  or xsi:type is not handled.
//	- There is no SourceUri provision.
//

#if NET_2_0

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using Mono.Xml.Schema;

using QName = System.Xml.XmlQualifiedName;
using Form = System.Xml.Schema.XmlSchemaForm;
using Use = System.Xml.Schema.XmlSchemaUse;
using ContentType = System.Xml.Schema.XmlSchemaContentType;
using Validity = System.Xml.Schema.XmlSchemaValidity;
using ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags;
using ContentProc = System.Xml.Schema.XmlSchemaContentProcessing;
using SOMList = System.Xml.Schema.XmlSchemaObjectCollection;
using SOMObject = System.Xml.Schema.XmlSchemaObject;
using XsElement = System.Xml.Schema.XmlSchemaElement;
using XsAttribute = System.Xml.Schema.XmlSchemaAttribute;
using AttrGroup = System.Xml.Schema.XmlSchemaAttributeGroup;
using AttrGroupRef = System.Xml.Schema.XmlSchemaAttributeGroupRef;
using XsDatatype = System.Xml.Schema.XmlSchemaDatatype;
using SimpleType = System.Xml.Schema.XmlSchemaSimpleType;
using ComplexType = System.Xml.Schema.XmlSchemaComplexType;
using SimpleModel = System.Xml.Schema.XmlSchemaSimpleContent;
using SimpleExt = System.Xml.Schema.XmlSchemaSimpleContentExtension;
using SimpleRst = System.Xml.Schema.XmlSchemaSimpleContentRestriction;
using ComplexModel = System.Xml.Schema.XmlSchemaComplexContent;
using ComplexExt = System.Xml.Schema.XmlSchemaComplexContentExtension;
using ComplexRst = System.Xml.Schema.XmlSchemaComplexContentRestriction;
using SimpleTypeRest = System.Xml.Schema.XmlSchemaSimpleTypeRestriction;
using SimpleTypeList = System.Xml.Schema.XmlSchemaSimpleTypeList;
using SimpleTypeUnion = System.Xml.Schema.XmlSchemaSimpleTypeUnion;
using SchemaFacet = System.Xml.Schema.XmlSchemaFacet;
using LengthFacet = System.Xml.Schema.XmlSchemaLengthFacet;
using MinLengthFacet = System.Xml.Schema.XmlSchemaMinLengthFacet;
using Particle = System.Xml.Schema.XmlSchemaParticle;
using Sequence = System.Xml.Schema.XmlSchemaSequence;
using Choice = System.Xml.Schema.XmlSchemaChoice;
using ValException = System.Xml.Schema.XmlSchemaValidationException;


namespace System.Xml.Schema
{
	public sealed class XmlSchemaValidator
	{
		enum Transition {
			None,
			Content,
			StartTag,
			Finished
		}

		static readonly XsAttribute [] emptyAttributeArray =
			new XsAttribute [0];

		public XmlSchemaValidator (
			XmlNameTable nameTable,
			XmlSchemaSet schemas,
			IXmlNamespaceResolver nsResolver,
			ValidationFlags options)
		{
			this.nameTable = nameTable;
			this.schemas = schemas;
			this.nsResolver = nsResolver;
			this.options = options;
		}

		#region Fields

		// XmlReader/XPathNavigator themselves
		object nominalEventSender;
		IXmlLineInfo lineInfo;
		IXmlNamespaceResolver nsResolver;
		Uri sourceUri;

		// These fields will be from XmlReaderSettings or 
		// XPathNavigator.CheckValidity(). BTW, I think we could
		// implement XPathNavigator.CheckValidity() with
		// XsdValidatingReader.
		XmlNameTable nameTable;
		XmlSchemaSet schemas;
		XmlResolver xmlResolver = new XmlUrlResolver ();

		// "partialValidationType". but not sure how it will be used.
		SOMObject startType;

		// It is perhaps from XmlReaderSettings, but XPathNavigator
		// does not have it.
		ValidationFlags options;

		// Validation state
		Transition transition;
		XsdParticleStateManager state;

		ArrayList occuredAtts = new ArrayList ();
		XsAttribute [] defaultAttributes = emptyAttributeArray;
		ArrayList defaultAttributesCache = new ArrayList ();

#region ID Constraints
		XsdIDManager idManager = new XsdIDManager ();
#endregion

#region Key Constraints
		ArrayList keyTables = new ArrayList ();
		ArrayList currentKeyFieldConsumers = new ArrayList ();
		ArrayList tmpKeyrefPool;
#endregion
		ArrayList elementQNameStack = new ArrayList ();

		StringBuilder storedCharacters = new StringBuilder ();
		bool shouldValidateCharacters;

		int depth;
		int xsiNilDepth = -1;
		int skipValidationDepth = -1;

		// LAMESPEC: XmlValueGetter is bogus by design because there
		// is no way to get associated schema type for current value.
		// Here XmlSchemaValidatingReader needs "current type"
		// information to validate attribute values.
		internal XmlSchemaDatatype CurrentAttributeType;

		XmlSchemaInfo current_info;

		#endregion

		#region Public properties

		// Settable Properties

		// IMHO It should just be an event that fires another event.
		public event ValidationEventHandler ValidationEventHandler;

		public object ValidationEventSender {
			get { return nominalEventSender; }
			set { nominalEventSender = value; }
		}

		// (kinda) Construction Properties

		public IXmlLineInfo LineInfoProvider {
			get { return lineInfo; }
			set { lineInfo = value; }
		}

		public XmlResolver XmlResolver {
			set { xmlResolver = value; }
		}

		public Uri SourceUri {
			get { return sourceUri; }
			set { sourceUri = value; }
		}
		#endregion

		#region Private properties

		private string BaseUri {
			get { return sourceUri != null ? sourceUri.AbsoluteUri : String.Empty; }
		}

		private XsdValidationContext Context {
			get { return state.Context; }
		}

		private bool IgnoreWarnings {
			get { return (options & ValidationFlags
				.ReportValidationWarnings) == 0; }
		}

		private bool IgnoreIdentity {
			get { return (options & ValidationFlags
				.ProcessIdentityConstraints) == 0; }
		}

		#endregion

		#region Public methods

		// State Monitor

		public XmlSchemaAttribute [] GetExpectedAttributes ()
		{
			ComplexType cType = Context.ActualType as ComplexType;
			if (cType == null)
				return emptyAttributeArray;
			ArrayList al = new ArrayList ();
			foreach (DictionaryEntry entry in cType.AttributeUses)
				if (!occuredAtts.Contains ((QName) entry.Key))
					al.Add (entry.Value);
			return (XsAttribute [])
				al.ToArray (typeof (XsAttribute));
		}

		private void CollectAtomicParticles (XmlSchemaParticle p,
			ArrayList al)
		{
			if (p is XmlSchemaGroupBase) {
				foreach (XmlSchemaParticle c in 
					((XmlSchemaGroupBase) p).Items)
					CollectAtomicParticles (c, al);
			}
			else
				al.Add (p);
		}

		[MonoTODO] // Need some tests.
		// Its behavior is not obvious. For example, it does not
		// contain groups (xs:sequence/xs:choice/xs:all). Since it
		// might contain xs:any, it could not be of type element[].
		public XmlSchemaParticle [] GetExpectedParticles ()
		{
			ArrayList al = new ArrayList ();
			Context.State.GetExpectedParticles (al);
			ArrayList ret = new ArrayList ();

			foreach (XmlSchemaParticle p in al)
				CollectAtomicParticles (p, ret);

			return (XmlSchemaParticle []) ret.ToArray (
				typeof (XmlSchemaParticle));
		}

		public void GetUnspecifiedDefaultAttributes (ArrayList defaultAttributeList)
		{
			if (defaultAttributeList == null)
				throw new ArgumentNullException ("defaultAttributeList");

			if (transition != Transition.StartTag)
				throw new InvalidOperationException ("Method 'GetUnsoecifiedDefaultAttributes' works only when the validator state is inside a start tag.");
			foreach (XmlSchemaAttribute attr
				in GetExpectedAttributes ())
				if (attr.ValidatedDefaultValue != null || attr.ValidatedFixedValue != null)
					defaultAttributeList.Add (attr);

			defaultAttributeList.AddRange (defaultAttributes);
		}

		// State Controller

		public void AddSchema (XmlSchema schema)
		{
			if (schema == null)
				throw new ArgumentNullException ("schema");
			schemas.Add (schema);
			schemas.Compile ();
		}

		public void Initialize ()
		{
			transition = Transition.Content;
			state = new XsdParticleStateManager ();
			if (!schemas.IsCompiled)
				schemas.Compile ();
		}

		public void Initialize (SOMObject partialValidationType)
		{
			if (partialValidationType == null)
				throw new ArgumentNullException ("partialValidationType");
			this.startType = partialValidationType;
			Initialize ();
		}

		// It must be called at the end of the validation (to check
		// identity constraints etc.).
		public void EndValidation ()
		{
			CheckState (Transition.Content);
			transition = Transition.Finished;

			if (schemas.Count == 0)
				return;

			if (depth > 0)
				throw new InvalidOperationException (String.Format ("There are {0} open element(s). ValidateEndElement() must be called for each open element.", depth));

			// 3.3.4 ElementLocallyValidElement 7 = Root Valid.
			if (!IgnoreIdentity &&
				idManager.HasMissingIDReferences ())
				HandleError ("There are missing ID references: " + idManager.GetMissingIDString ());
		}

		// I guess it is for validation error recovery
		[MonoTODO] // FIXME: Find out how XmlSchemaInfo is used.
		public void SkipToEndElement (XmlSchemaInfo info)
		{
			CheckState (Transition.Content);
			if (schemas.Count == 0)
				return;
			state.PopContext ();
		}

		public object ValidateAttribute (
			string localName,
			string ns,
			string attributeValue,
			XmlSchemaInfo info)
		{
			if (attributeValue == null)
				throw new ArgumentNullException ("attributeValue");
			return ValidateAttribute (localName, ns, delegate () { return attributeValue; }, info);
		}

		// I guess this weird XmlValueGetter is for such case that
		// value might not be required (and thus it improves 
		// performance in some cases. Doh).

		// The return value is typed primitive, if possible.
		// AttDeriv
		public object ValidateAttribute (
			string localName,
			string ns,
			XmlValueGetter attributeValue,
			XmlSchemaInfo info)
		{
			if (localName == null)
				throw new ArgumentNullException ("localName");
			if (ns == null)
				throw new ArgumentNullException ("ns");
			if (attributeValue == null)
				throw new ArgumentNullException ("attributeValue");

			CheckState (Transition.StartTag);

			QName qname = new QName (localName, ns);
			if (occuredAtts.Contains (qname))
				throw new InvalidOperationException (String.Format ("Attribute '{0}' has already been validated in the same element.", qname));
			occuredAtts.Add (qname);

			if (ns == XmlNamespaceManager.XmlnsXmlns)
				return null;

			if (schemas.Count == 0)
				return null;

			if (Context.Element != null && Context.XsiType == null) {

				// 3.3.4 Element Locally Valid (Type) - attribute
				if (Context.ActualType is ComplexType)
					return AssessAttributeElementLocallyValidType (localName, ns, attributeValue, info);
				else
					HandleError ("Current simple type cannot accept attributes other than schema instance namespace.");
			}
			return null;
		}

		// StartTagOpenDeriv
		public void ValidateElement (
			string localName,
			string ns,
			XmlSchemaInfo info)
		{
			ValidateElement (localName, ns, info, null, null, null, null);
		}

		public void ValidateElement (
			string localName,
			string ns,
			XmlSchemaInfo info,
			string xsiType,
			string xsiNil,
			string schemaLocation,
			string noNsSchemaLocation)
		{
			if (localName == null)
				throw new ArgumentNullException ("localName");
			if (ns == null)
				throw new ArgumentNullException ("ns");
			SetCurrentInfo (info);
			try {

			CheckState (Transition.Content);
			transition = Transition.StartTag;

			if (schemaLocation != null)
				HandleSchemaLocation (schemaLocation);
			if (noNsSchemaLocation != null)
				HandleNoNSSchemaLocation (noNsSchemaLocation);

			elementQNameStack.Add (new XmlQualifiedName (localName, ns));

			if (schemas.Count == 0)
				return;

#region ID Constraints
			if (!IgnoreIdentity)
				idManager.OnStartElement ();
#endregion
			defaultAttributes = emptyAttributeArray;

			// If there is no schema information, then no validation is performed.
			if (skipValidationDepth < 0 || depth <= skipValidationDepth) {
				if (shouldValidateCharacters)
					ValidateEndSimpleContent (null);

				AssessOpenStartElementSchemaValidity (localName, ns);
			}

			if (xsiNil != null)
				HandleXsiNil (xsiNil, info);
			if (xsiType != null)
				HandleXsiType (xsiType);

			if (xsiNilDepth < depth)
				shouldValidateCharacters = true;

			if (info != null) {
				info.IsNil = xsiNilDepth >= 0;
				info.SchemaElement = Context.Element;
				info.SchemaType = Context.ActualSchemaType;
				info.SchemaAttribute = null;
				info.IsDefault = false;
				info.MemberType = null;
				// FIXME: supply Validity (really useful?)
			}

			} finally {
				current_info = null;
			}
		}

		public object ValidateEndElement (XmlSchemaInfo info)
		{
			return ValidateEndElement (info, null);
		}

		// The return value is typed primitive, if supplied.
		// Parameter 'var' seems to be converted into the type
		// represented by current simple content type. (try passing
		// some kind of object to this method to check the behavior.)
		// EndTagDeriv
		[MonoTODO] // FIXME: Handle 'var' parameter.
		public object ValidateEndElement (XmlSchemaInfo info,
			object var)
		{
			SetCurrentInfo (info);
			try {

			// If it is going to validate an empty element, then
			// first validate end of attributes.
			if (transition == Transition.StartTag) {
				current_info = null;
				ValidateEndOfAttributes (info);
			}

			CheckState (Transition.Content);

			elementQNameStack.RemoveAt (elementQNameStack.Count - 1);

			if (schemas.Count == 0)
				return null;
			if (depth == 0)
				throw new InvalidOperationException ("There was no corresponding call to 'ValidateElement' method.");

			depth--;

			object ret = null;
			if (depth == skipValidationDepth)
				skipValidationDepth = -1;
			else if (skipValidationDepth < 0 || depth <= skipValidationDepth)
				ret = AssessEndElementSchemaValidity (info);
			return ret;

			} finally {
				current_info = null;
			}
		}

		// StartTagCloseDeriv
		// FIXME: fill validity inside this invocation.
		public void ValidateEndOfAttributes (XmlSchemaInfo info)
		{
			try {
				SetCurrentInfo (info);

				CheckState (Transition.StartTag);
				transition = Transition.Content;
				if (schemas.Count == 0)
					return;

				if (skipValidationDepth < 0 || depth <= skipValidationDepth)
					AssessCloseStartElementSchemaValidity (info);
				depth++;
			} finally {
				current_info = null;
				occuredAtts.Clear ();
			}
		}

		// LAMESPEC: It should also receive XmlSchemaInfo so that
		// a validator application can receive simple type or
		// or content type validation errors.
		public void ValidateText (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			ValidateText (delegate () { return value; });
		}

		// TextDeriv ... without text. Maybe typed check is done by
		// ValidateAtomicValue().
		public void ValidateText (XmlValueGetter getter)
		{
			if (getter == null)
				throw new ArgumentNullException ("getter");

			CheckState (Transition.Content);
			if (schemas.Count == 0)
				return;

			if (skipValidationDepth >= 0 && depth > skipValidationDepth)
				return;

			ComplexType ct = Context.ActualType as ComplexType;
			if (ct != null) {
				switch (ct.ContentType) {
				case XmlSchemaContentType.Empty:
					HandleError ("Not allowed character content was found.");
					break;
				case XmlSchemaContentType.ElementOnly:
					string s = storedCharacters.ToString ();
					if (s.Length > 0 && !XmlChar.IsWhitespace (s))
						HandleError ("Not allowed character content was found.");
					break;
				}
			}

			ValidateCharacters (getter);
		}

		public void ValidateWhitespace (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			ValidateWhitespace (delegate () { return value; });
		}

		// TextDeriv. It should do the same as ValidateText() in our actual implementation (whitespaces are conditioned).
		public void ValidateWhitespace (XmlValueGetter getter)
		{
			ValidateText (getter);
		}

		#endregion

		#region Error handling

		private void HandleError (string message)
		{
			HandleError (message, null, false);
		}

		private void HandleError (
			string message, Exception innerException)
		{
			HandleError (message, innerException, false);
		}

		private void HandleError (string message,
			Exception innerException, bool isWarning)
		{
			if (current_info != null)
				current_info.Validity = XmlSchemaValidity.Invalid;
			if (isWarning && IgnoreWarnings)
				return;

			ValException vex = new ValException (
				message, nominalEventSender, BaseUri,
				null, innerException);
			HandleError (vex, isWarning);
		}

		private void HandleError (ValException exception)
		{
			HandleError (exception, false);
		}

		private void HandleError (ValException exception, bool isWarning)
		{
			if (current_info != null)
				current_info.Validity = XmlSchemaValidity.Invalid;

			if (isWarning && IgnoreWarnings)
				return;

			if (ValidationEventHandler == null)
				throw exception;

			ValidationEventArgs e = new ValidationEventArgs (
				exception,
				exception.Message,
				isWarning ? XmlSeverityType.Warning :
					XmlSeverityType.Error);
			ValidationEventHandler (nominalEventSender, e);
		}

		#endregion

		// call this at entry point of every public method.
		private void SetCurrentInfo (XmlSchemaInfo info)
		{
			if (current_info != null)
				throw new InvalidOperationException ("Not allowed concurrent call to validation method");
			current_info = info;
			if (info != null && info.Validity == XmlSchemaValidity.NotKnown)
				current_info.Validity = XmlSchemaValidity.Valid;
		}

		private void CheckState (Transition expected)
		{
			if (transition != expected) {
				if (transition == Transition.None)
					throw new InvalidOperationException ("Initialize() must be called before processing validation.");
				else
					throw new InvalidOperationException (
						String.Format ("Unexpected attempt to validate state transition from {0} to {1}.",
							transition,
							expected));
			}
		}

		private XsElement FindElement (string name, string ns)
		{
			return (XsElement) schemas.GlobalElements [new XmlQualifiedName (name, ns)];
		}

		private XmlSchemaType FindType (XmlQualifiedName qname)
		{
			return (XmlSchemaType) schemas.GlobalTypes [qname];
		}

		#region Type Validation

		private void ValidateStartElementParticle (
			string localName, string ns)
		{
			if (Context.State == null)
				return;
			Context.XsiType = null;
			state.CurrentElement = null;
			Context.EvaluateStartElement (localName,
				ns);
			if (Context.IsInvalid)
				HandleError ("Invalid start element: " + ns + ":" + localName);

			Context.PushCurrentElement (state.CurrentElement);
		}

		private void AssessOpenStartElementSchemaValidity (
			string localName, string ns)
		{
			// If the reader is inside xsi:nil (and failed
			// on validation), then simply skip its content.
			if (xsiNilDepth >= 0 && xsiNilDepth < depth)
				HandleError ("Element item appeared, while current element context is nil.");

			ValidateStartElementParticle (localName, ns);

			// Create Validation Root, if not exist.
			// [Schema Validity Assessment (Element) 1.1]
			if (Context.Element == null) {
				state.CurrentElement = FindElement (localName, ns);
				Context.PushCurrentElement (state.CurrentElement);
			}

#region Key Constraints
			if (!IgnoreIdentity) {
				ValidateKeySelectors ();
				ValidateKeyFields (false, xsiNilDepth == depth,
					Context.ActualType, null, null, null);
			}
#endregion
		}

		private void AssessCloseStartElementSchemaValidity (XmlSchemaInfo info)
		{
			if (Context.XsiType != null)
				AssessCloseStartElementLocallyValidType (info);
			else if (Context.Element != null) {
				// element locally valid is checked only when
				// xsi:type does not exist.
				AssessElementLocallyValidElement ();
				if (Context.Element.ElementType != null)
					AssessCloseStartElementLocallyValidType (info);
			}

			if (Context.Element == null) {
				switch (state.ProcessContents) {
				case ContentProc.Skip:
					break;
				case ContentProc.Lax:
					break;
				default:
					QName current = (QName) elementQNameStack [elementQNameStack.Count - 1];
					if (Context.XsiType == null &&
						(schemas.Contains (current.Namespace) ||
						!schemas.MissedSubComponents (current.Namespace)))
						HandleError ("Element declaration for " + current + " is missing.");
					break;
				}
			}

			// Proceed to the next depth.

			state.PushContext ();

			XsdValidationState next = null;
			if (state.ProcessContents == ContentProc.Skip)
				skipValidationDepth = depth;
			else {
				// create child particle state.
				ComplexType xsComplexType = Context.ActualType as ComplexType;
				if (xsComplexType != null)
					next = state.Create (xsComplexType.ValidatableParticle);
				else if (state.ProcessContents == ContentProc.Lax)
					next = state.Create (XmlSchemaAny.AnyTypeContent);
				else
					next = state.Create (XmlSchemaParticle.Empty);
			}
			Context.State = next;
		}

		// It must be invoked after xsi:nil turned out not to be in
		// this element.
		private void AssessElementLocallyValidElement ()
		{
			XsElement element = Context.Element;
			XmlQualifiedName qname = (XmlQualifiedName) elementQNameStack [elementQNameStack.Count - 1];
			// 1.
			if (element == null)
				HandleError ("Element declaration is required for " + qname);
			// 2.
			if (element.ActualIsAbstract)
				HandleError ("Abstract element declaration was specified for " + qname);
			// 3. is checked inside ValidateAttribute().
		}

		// 3.3.4 Element Locally Valid (Type)
		private void AssessCloseStartElementLocallyValidType (XmlSchemaInfo info)
		{
			object schemaType = Context.ActualType;
			if (schemaType == null) {	// 1.
				HandleError ("Schema type does not exist.");
				return;
			}
			ComplexType cType = schemaType as ComplexType;
			SimpleType sType = schemaType as SimpleType;
			if (sType != null) {
				// 3.1.1.
				// Attributes are checked in ValidateAttribute().
			} else if (cType != null) {
				// 3.2. Also, 2. is checked there.
				AssessCloseStartElementLocallyValidComplexType (cType, info);
			}
		}

		// 3.4.4 Element Locally Valid (Complex Type)
		// FIXME: use SchemaInfo for somewhere (? it is passed to ValidateEndOfAttributes() for some reason)
		private void AssessCloseStartElementLocallyValidComplexType (ComplexType cType, XmlSchemaInfo info)
		{
			// 1.
			if (cType.IsAbstract) {
				HandleError ("Target complex type is abstract.");
				return;
			}

			// 2 (xsi:nil and content prohibition)
			// See AssessStartElementSchemaValidity() and ValidateCharacters()
			// 3. attribute uses and  5. wild IDs are handled at
			// ValidateAttribute(), except for default/fixed values.

			// Collect default attributes.
			// 4.
			foreach (XsAttribute attr in GetExpectedAttributes ()) {
				if (attr.ValidatedUse == XmlSchemaUse.Required && 
					attr.ValidatedFixedValue == null)
					HandleError ("Required attribute " + attr.QualifiedName + " was not found.");
				else if (attr.ValidatedDefaultValue != null || attr.ValidatedFixedValue != null)
					defaultAttributesCache.Add (attr);
			}
			if (defaultAttributesCache.Count == 0)
				defaultAttributes = emptyAttributeArray;
			else
				defaultAttributes = (XsAttribute []) 
					defaultAttributesCache.ToArray (
						typeof (XsAttribute));
			defaultAttributesCache.Clear ();
			// 5. wild IDs was already checked at ValidateAttribute().

			// 3. - handle default attributes
#region ID Constraints
			if (!IgnoreIdentity) {
				foreach (XsAttribute a in defaultAttributes) {
					var atype = a.AttributeType as XmlSchemaDatatype ?? a.AttributeSchemaType.Datatype;
					object avalue = a.ValidatedFixedValue ?? a.ValidatedDefaultValue;
					string error = idManager.AssessEachAttributeIdentityConstraint (atype, avalue, ((QName) elementQNameStack [elementQNameStack.Count - 1]).Name);
					if (error != null)
						HandleError (error);
				}
			}
#endregion

#region Key Constraints
			if (!IgnoreIdentity)
				foreach (XsAttribute a in defaultAttributes)
					ValidateKeyFieldsAttribute (a, a.ValidatedFixedValue ?? a.ValidatedDefaultValue);
#endregion
		}

		private object AssessAttributeElementLocallyValidType (string localName, string ns, XmlValueGetter getter, XmlSchemaInfo info)
		{
			ComplexType cType = Context.ActualType as ComplexType;
			XmlQualifiedName qname = new XmlQualifiedName (localName, ns);
			// including 3.10.4 Item Valid (Wildcard)
			XmlSchemaObject attMatch = XmlSchemaUtil.FindAttributeDeclaration (ns, schemas, cType, qname);
			if (attMatch == null)
				HandleError ("Attribute declaration was not found for " + qname);
			XsAttribute attdecl = attMatch as XsAttribute;
			if (attdecl != null) {
				AssessAttributeLocallyValidUse (attdecl);
				return AssessAttributeLocallyValid (attdecl, info, getter);
			} // otherwise anyAttribute or null.
			return null;
		}

		// 3.2.4 Attribute Locally Valid and 3.4.4
		private object AssessAttributeLocallyValid (XsAttribute attr, XmlSchemaInfo info, XmlValueGetter getter)
		{
			// 2. - 4.
			if (attr.AttributeType == null)
				HandleError ("Attribute type is missing for " + attr.QualifiedName);
			XsDatatype dt = attr.AttributeType as XsDatatype;
			if (dt == null)
				dt = ((SimpleType) attr.AttributeType).Datatype;

			object parsedValue = null;

			// It is a bit heavy process, so let's omit as long as possible ;-)
			if (dt != SimpleType.AnySimpleType || attr.ValidatedFixedValue != null) {
				try {
					CurrentAttributeType = dt;
					parsedValue = getter ();
				} catch (Exception ex) { // It is inevitable and bad manner.
					HandleError (String.Format ("Attribute value is invalid against its data type {0}", dt != null ? dt.TokenizedType : default (XmlTokenizedType)), ex);
				}

				// check part of 3.14.4 StringValid
				SimpleType st = attr.AttributeType as SimpleType;
				if (st != null)
					ValidateRestrictedSimpleTypeValue (st, ref dt, new XmlAtomicValue (parsedValue, attr.AttributeSchemaType).Value);

				if (attr.ValidatedFixedValue != null) {
					if (!XmlSchemaUtil.AreSchemaDatatypeEqual (attr.AttributeSchemaType, attr.ValidatedFixedTypedValue, attr.AttributeSchemaType, parsedValue))
						HandleError (String.Format ("The value of the attribute {0} does not match with its fixed value '{1}' in the space of type {2}", attr.QualifiedName, attr.ValidatedFixedValue, dt));
					parsedValue = attr.ValidatedFixedTypedValue;
				}
			}

#region ID Constraints
			if (!IgnoreIdentity) {
				string error = idManager.AssessEachAttributeIdentityConstraint (dt, parsedValue, ((QName) elementQNameStack [elementQNameStack.Count - 1]).Name);
				if (error != null)
					HandleError (error);
			}
#endregion

#region Key Constraints
			if (!IgnoreIdentity)
				ValidateKeyFieldsAttribute (attr, parsedValue);
#endregion

			return parsedValue;
		}

		private void AssessAttributeLocallyValidUse (XsAttribute attr)
		{
			// This is extra check than spec 3.5.4
			if (attr.ValidatedUse == XmlSchemaUse.Prohibited)
				HandleError ("Attribute " + attr.QualifiedName + " is prohibited in this context.");
		}

		private object AssessEndElementSchemaValidity (
			XmlSchemaInfo info)
		{
			object ret = ValidateEndSimpleContent (info);

			ValidateEndElementParticle ();	// validate against childrens' state.

			// 3.3.4 Assess ElementLocallyValidElement 5: value constraints.
			// 3.3.4 Assess ElementLocallyValidType 3.1.3. = StringValid(3.14.4)
			// => ValidateEndSimpleContent ().

#region Key Constraints
			if (!IgnoreIdentity)
				ValidateEndElementKeyConstraints ();
#endregion

			// Reset xsi:nil, if required.
			if (xsiNilDepth == depth)
				xsiNilDepth = -1;
			return ret;
		}

		private void ValidateEndElementParticle ()
		{
			if (Context.State != null) {
				if (!Context.EvaluateEndElement ()) {
					HandleError ("Invalid end element. There are still required content items.");
				}
			}
			Context.PopCurrentElement ();
			state.PopContext ();
			Context.XsiType = null; // FIXME: this is hack. should be stacked as well as element.
		}

		// Utility for missing validation completion related to child items.
		private void ValidateCharacters (XmlValueGetter getter)
		{
			if (xsiNilDepth >= 0 && xsiNilDepth < depth)
				HandleError ("Element item appeared, while current element context is nil.");

			if (shouldValidateCharacters) {
				CurrentAttributeType = null;
				storedCharacters.Append (getter ());
			}
		}


		// Utility for missing validation completion related to child items.
		private object ValidateEndSimpleContent (XmlSchemaInfo info)
		{
			object ret = null;
			if (shouldValidateCharacters)
				ret = ValidateEndSimpleContentCore (info);
			shouldValidateCharacters = false;
			storedCharacters.Length = 0;
			return ret;
		}

		private object ValidateEndSimpleContentCore (XmlSchemaInfo info)
		{
			if (Context.ActualType == null)
				return null;

			string value = storedCharacters.ToString ();
			object ret = null;

			if (value.Length == 0) {
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
						if (value.Length > 0 && !XmlChar.IsWhitespace (value))
							HandleError ("Character content not allowed in an elementOnly model.");
						break;
					case XmlSchemaContentType.Empty:
						if (value.Length > 0)
							HandleError ("Character content not allowed in an empty model.");
						break;
					}
				}
			}
			if (dt != null) {
				// 3.3.4 Element Locally Valid (Element) :: 5.2.2.2. Fixed value constraints
				if (Context.Element != null && Context.Element.ValidatedFixedValue != null)
					if (value != Context.Element.ValidatedFixedValue)
						HandleError ("Fixed value constraint was not satisfied.");
				ret = AssessStringValid (st, dt, value);
			}

#region Key Constraints
			if (!IgnoreIdentity)
				ValidateSimpleContentIdentity (dt, value);
#endregion

			shouldValidateCharacters = false;

			if (info != null) {
				info.IsNil = xsiNilDepth >= 0;
				info.SchemaElement = null;
				info.SchemaType = Context.ActualType as XmlSchemaType;
				if (info.SchemaType == null)
					info.SchemaType = XmlSchemaType.GetBuiltInSimpleType (dt.TypeCode);
				info.SchemaAttribute = null;
				info.IsDefault = false; // FIXME: might be true
				info.MemberType = null; // FIXME: check
				// FIXME: supply Validity (really useful?)
			}

			return ret;
		}

		// 3.14.4 String Valid 
		private object AssessStringValid (SimpleType st,
			XsDatatype dt, string value)
		{
			XsDatatype validatedDatatype = dt;
			object ret = null;
			if (st != null) {
				string normalized = validatedDatatype.Normalize (value);
				string [] values;
				XsDatatype itemDatatype;
				SimpleType itemSimpleType;
				switch (st.DerivedBy) {
				case XmlSchemaDerivationMethod.List:
					SimpleTypeList listContent = st.Content as SimpleTypeList;
					values = normalized.Split (XmlChar.WhitespaceChars);
					// LAMESPEC: Types of each element in
					// the returned list might be 
					// inconsistent, so basically returning 
					// value does not make sense without 
					// explicit runtime type information 
					// for base primitive type.
					object [] retValues = new object [values.Length];
					itemDatatype = listContent.ValidatedListItemType as XsDatatype;
					itemSimpleType = listContent.ValidatedListItemType as SimpleType;
					for (int vi = 0; vi < values.Length; vi++) {
						string each = values [vi];
						if (each == String.Empty)
							continue;
						// validate against ValidatedItemType
						if (itemDatatype != null) {
							try {
								retValues [vi] = itemDatatype.ParseValue (each, nameTable, nsResolver);
							} catch (Exception ex) { // It is inevitable and bad manner.
								HandleError ("List type value contains one or more invalid values.", ex);
								break;
							}
						}
						else
							AssessStringValid (itemSimpleType, itemSimpleType.Datatype, each);
					}
					ret = retValues;
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
									ret = itemDatatype.ParseValue (each, nameTable, nsResolver);
								} catch (Exception) { // It is inevitable and bad manner.
									continue;
								}
							}
							else {
								try {
									ret = AssessStringValid (itemSimpleType, itemSimpleType.Datatype, each);
								} catch (ValException) {
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
							 ret = AssessStringValid (baseType, dt, value);
						}
						if (!str.ValidateValueWithFacets (value, nameTable, nsResolver)) {
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
					ret = validatedDatatype.ParseValue (value, nameTable, nsResolver);
				} catch (Exception ex) { // It is inevitable and bad manner.
					HandleError (String.Format ("Invalidly typed data was specified."), ex);
				}
			}
			return ret;
		}

		private void ValidateRestrictedSimpleTypeValue (SimpleType st, ref XsDatatype dt, string normalized)
		{
			{
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
								itemDatatype.ParseValue (each, nameTable, nsResolver);
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
									itemDatatype.ParseValue (each, nameTable, nsResolver);
								} catch (Exception) { // FIXME: (wishlist) better exception handling ;-(
									continue;
								}
							}
							else {
								try {
									AssessStringValid (itemSimpleType, itemSimpleType.Datatype, each);
								} catch (ValException) {
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
						if (!str.ValidateValueWithFacets (normalized, nameTable, nsResolver)) {
							HandleError ("Specified value was invalid against the facets.");
							break;
						}
					}
					dt = st.Datatype;
					break;
				}
			}
		}

		#endregion

		#region Key Constraints Validation
		private XsdKeyTable CreateNewKeyTable (XmlSchemaIdentityConstraint ident)
		{
			XsdKeyTable seq = new XsdKeyTable (ident);
			seq.StartDepth = depth;
			this.keyTables.Add (seq);
			return seq;
		}

		// 3.11.4 Identity Constraint Satisfied
		private void ValidateKeySelectors ()
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
				if (seq.SelectorMatches (this.elementQNameStack, depth) != null) {
					// creates and registers new entry.
					XsdKeyEntry entry = new XsdKeyEntry (seq, depth, lineInfo);
					seq.Entries.Add (entry);
				}
			}
		}

		private void ValidateKeyFieldsAttribute (XsAttribute attr, object value)
		{
			ValidateKeyFields (true, false, attr.AttributeType, attr.QualifiedName.Name, attr.QualifiedName.Namespace, value);
		}

		private void ValidateKeyFields (bool isAttr, bool isNil, object schemaType, string attrName, string attrNs, object value)
		{
			// (c) Evaluate field paths.
			for (int i = 0; i < keyTables.Count; i++) {
				XsdKeyTable seq  = (XsdKeyTable) keyTables [i];
				// If possible, create new field entry candidates.
				for (int j = 0; j < seq.Entries.Count; j++) {
					CurrentAttributeType = null;
					try {
						seq.Entries [j].ProcessMatch (
							isAttr,
							elementQNameStack,
							nominalEventSender,
							nameTable,
							BaseUri,
							schemaType,
							nsResolver,
							lineInfo,
							isAttr ? depth + 1 : depth,
							attrName,
							attrNs,
							value,
							isNil, 
							currentKeyFieldConsumers);
					} catch (ValException ex) {
						HandleError (ex);
					}
				}
			}
		}

		private void ValidateEndElementKeyConstraints ()
		{
			// Reset Identity constraints.
			for (int i = 0; i < keyTables.Count; i++) {
				XsdKeyTable seq = this.keyTables [i] as XsdKeyTable;
				if (seq.StartDepth == depth) {
					ValidateEndKeyConstraint (seq);
				} else {
					for (int k = 0; k < seq.Entries.Count; k++) {
						XsdKeyEntry entry = seq.Entries [k] as XsdKeyEntry;
						// Remove finished (maybe key not found) entries.
						if (entry.StartDepth == depth) {
							if (entry.KeyFound)
								seq.FinishedEntries.Add (entry);
							else if (seq.SourceSchemaIdentity is XmlSchemaKey)
								HandleError ("Key sequence is missing.");
							seq.Entries.RemoveAt (k);
							k--;
						}
						// Pop validated key depth to find two or more fields.
						else {
							for (int j = 0; j < entry.KeyFields.Count; j++) {
								XsdKeyEntryField kf = entry.KeyFields [j];
								if (!kf.FieldFound && kf.FieldFoundDepth == depth) {
									kf.FieldFoundDepth = 0;
									kf.FieldFoundPath = null;
								}
							}
						}
					}
				}
			}
			for (int i = 0; i < keyTables.Count; i++) {
				XsdKeyTable seq = this.keyTables [i] as XsdKeyTable;
				if (seq.StartDepth == depth) {
					keyTables.RemoveAt (i);
					i--;
				}
			}
		}

		private void ValidateEndKeyConstraint (XsdKeyTable seq)
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

		private void ValidateSimpleContentIdentity (
			XmlSchemaDatatype dt, string value)
		{
			// Identity field value
			if (currentKeyFieldConsumers != null) {
				while (this.currentKeyFieldConsumers.Count > 0) {
					XsdKeyEntryField field = this.currentKeyFieldConsumers [0] as XsdKeyEntryField;
					if (field.Identity != null)
						HandleError ("Two or more identical field was found. Former value is '" + field.Identity + "' .");
					object identity = null; // This means empty value
					if (dt != null) {
						try {
							identity = dt.ParseValue (value, nameTable, nsResolver);
						} catch (Exception ex) { // It is inevitable and bad manner.
							HandleError ("Identity value is invalid against its data type " + dt.TokenizedType, ex);
						}
					}
					if (identity == null)
						identity = value;

					if (!field.SetIdentityField (identity, depth == xsiNilDepth, dt as XsdAnySimpleType, depth, lineInfo))
						HandleError ("Two or more identical key value was found: '" + value + "' .");
					this.currentKeyFieldConsumers.RemoveAt (0);
				}
			}
		}
		#endregion

		#region xsi:type
		private object GetXsiType (string name)
		{
			object xsiType = null;
			XmlQualifiedName typeQName =
				XmlQualifiedName.Parse (name, nsResolver, true);
			if (typeQName == ComplexType.AnyTypeName)
				xsiType = ComplexType.AnyType;
			else if (XmlSchemaUtil.IsBuiltInDatatypeName (typeQName))
				xsiType = XsDatatype.FromName (typeQName);
			else
				xsiType = FindType (typeQName);
			return xsiType;
		}

		private void HandleXsiType (string typename)
		{
			XsElement element = Context.Element;
			object xsiType = GetXsiType (typename);
			if (xsiType == null) {
				HandleError ("The instance type was not found: " + typename);
				return;
			}
			XmlSchemaType xsiSchemaType = xsiType as XmlSchemaType;
			if (xsiSchemaType != null && Context.Element != null) {
				XmlSchemaType elemBaseType = element.ElementType as XmlSchemaType;
				if (elemBaseType != null && (xsiSchemaType.DerivedBy & elemBaseType.FinalResolved) != 0)
					HandleError ("The instance type is prohibited by the type of the context element.");
				if (elemBaseType != xsiType && (xsiSchemaType.DerivedBy & element.BlockResolved) != 0)
					HandleError ("The instance type is prohibited by the context element.");
			}
			ComplexType xsiComplexType = xsiType as ComplexType;
			if (xsiComplexType != null && xsiComplexType.IsAbstract)
				HandleError ("The instance type is abstract: " + typename);
			else {
				// If current schema type exists, then this xsi:type must be
				// valid extension of that type. See 1.2.1.2.4.
				if (element != null) {
					AssessLocalTypeDerivationOK (xsiType, element.ElementType, element.BlockResolved);
				}
				// See also ValidateEndOfAttributes().
				Context.XsiType = xsiType;
			}
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
				} catch (ValException ex) {
					HandleError (ex);
				}
			else {
				SimpleType xsiSimpleType = xsiType as SimpleType;
				if (xsiSimpleType != null) {
					try {
						xsiSimpleType.ValidateTypeDerivationOK (baseType, null, null, true);
					} catch (ValException ex) {
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
		#endregion

		private void HandleXsiNil (string value, XmlSchemaInfo info)
		{
			XsElement element = Context.Element;
			if (!element.ActualIsNillable) {
				HandleError (String.Format ("Current element '{0}' is not nillable and thus does not allow occurence of 'nil' attribute.", Context.Element.QualifiedName));
				return;
			}
			value = value.Trim (XmlChar.WhitespaceChars);
			// 3.2.
			// Note that 3.2.1 xsi:nil constraints are to be 
			// validated in AssessElementSchemaValidity() and 
			// ValidateCharacters().
			if (value == "true") {
				if (element.ValidatedFixedValue != null)
					HandleError ("Schema instance nil was specified, where the element declaration for " + element.QualifiedName + "has fixed value constraints.");
				xsiNilDepth = depth;
				if (info != null)
					info.IsNil = true;
			}
		}

		#region External schema resolution

		private XmlSchema ReadExternalSchema (string uri)
		{
			Uri absUri = new Uri (SourceUri, uri.Trim (XmlChar.WhitespaceChars));
			XmlTextReader xtr = null;
			try {
				xtr = new XmlTextReader (absUri.ToString (),
					(Stream) xmlResolver.GetEntity (
						absUri, null, typeof (Stream)),
					nameTable);
				return XmlSchema.Read (
					xtr, ValidationEventHandler);
			} finally {
				if (xtr != null)
					xtr.Close ();
			}
		}

		private void HandleSchemaLocation (string schemaLocation)
		{
			if (xmlResolver == null)
				return;
			XmlSchema schema = null;
			bool schemaAdded = false;
			string [] tmp = null;
			try {
				schemaLocation = XmlSchemaType.GetBuiltInSimpleType (XmlTypeCode.Token).Datatype.ParseValue (schemaLocation, null, null) as string;
				tmp = schemaLocation.Split (XmlChar.WhitespaceChars);
			} catch (Exception ex) {
				HandleError ("Invalid schemaLocation attribute format.", ex, true);
				tmp = new string [0];
			}
			if (tmp.Length % 2 != 0)
				HandleError ("Invalid schemaLocation attribute format.");
			for (int i = 0; i < tmp.Length; i += 2) {
				try {
					schema = ReadExternalSchema (tmp [i + 1]);
				} catch (Exception ex) { // It is inevitable and bad manner.
					HandleError ("Could not resolve schema location URI: " + schemaLocation, ex, true);
					continue;
				}
				if (schema.TargetNamespace == null)
					schema.TargetNamespace = tmp [i];
				else if (schema.TargetNamespace != tmp [i])
					HandleError ("Specified schema has different target namespace.");

				if (schema != null) {
					if (!schemas.Contains (schema.TargetNamespace)) {
						schemaAdded = true;
						schemas.Add (schema);
					}
				}
			}
			if (schemaAdded)
				schemas.Compile ();
		}

		private void HandleNoNSSchemaLocation (string noNsSchemaLocation)
		{
			if (xmlResolver == null)
				return;
			XmlSchema schema = null;
			bool schemaAdded = false;

			try {
				schema = ReadExternalSchema (noNsSchemaLocation);
			} catch (Exception ex) { // It is inevitable and bad manner.
				HandleError ("Could not resolve schema location URI: " + noNsSchemaLocation, ex, true);
			}
			if (schema != null && schema.TargetNamespace != null)
				HandleError ("Specified schema has different target namespace.");

			if (schema != null) {
				if (!schemas.Contains (schema.TargetNamespace)) {
					schemaAdded = true;
					schemas.Add (schema);
				}
			}
			if (schemaAdded)
				schemas.Compile ();
		}

		#endregion
	}
}

#endif
