//
// Mono.Xml.DTDObjectModel
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
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
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
#if NET_2_0
using System.Collections.Generic;
#endif
#if NET_2_1
using XmlSchemaException = System.Xml.XmlException;
#else
using Mono.Xml.Schema;
#endif

#if NET_2_0
using XmlTextReaderImpl = Mono.Xml2.XmlTextReader;
#else
using XmlTextReaderImpl = System.Xml.XmlTextReader;
#endif

namespace Mono.Xml
{
	internal class DTDObjectModel
	{
		// This specifies the max number of dependent external entities
		// per a DTD can consume. A malicious external document server
		// might send users' document processing server a large number
		// of external entities.
		public const int AllowedExternalEntitiesMax = 256;

		DTDAutomataFactory factory;
		DTDElementAutomata rootAutomata;
		DTDEmptyAutomata emptyAutomata;
		DTDAnyAutomata anyAutomata;
		DTDInvalidAutomata invalidAutomata;

		DTDElementDeclarationCollection elementDecls;
		DTDAttListDeclarationCollection attListDecls;
		DTDParameterEntityDeclarationCollection peDecls;
		DTDEntityDeclarationCollection entityDecls;
		DTDNotationDeclarationCollection notationDecls;
		ArrayList validationErrors;
		XmlResolver resolver;
		XmlNameTable nameTable;

		Hashtable externalResources;

		string baseURI;
		string name;
		string publicId;
		string systemId;
		string intSubset;
		bool intSubsetHasPERef;
		bool isStandalone;
		int lineNumber;
		int linePosition;

		public DTDObjectModel (XmlNameTable nameTable)
		{
			this.nameTable = nameTable;
			elementDecls = new DTDElementDeclarationCollection (this);
			attListDecls = new DTDAttListDeclarationCollection (this);
			entityDecls = new DTDEntityDeclarationCollection (this);
			peDecls = new DTDParameterEntityDeclarationCollection (this);
			notationDecls = new DTDNotationDeclarationCollection (this);
			factory = new DTDAutomataFactory (this);
			validationErrors = new ArrayList ();
			externalResources = new Hashtable ();
		}

		public string BaseURI {
			get { return baseURI; }
			set { baseURI = value; }
		}

		public bool IsStandalone {
			get { return isStandalone; }
			set { isStandalone = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public XmlNameTable NameTable {
			get { return nameTable; }
		}
		
		public string PublicId {
			get { return publicId; }
			set { publicId = value; }
		}

		public string SystemId {
			get { return systemId; }
			set { systemId = value; }
		}
		
		public string InternalSubset {
			get { return intSubset; }
			set { intSubset = value; }
		}

		public bool InternalSubsetHasPEReference {
			get { return intSubsetHasPERef; }
			set { intSubsetHasPERef = value; }
		}

		public int LineNumber {
			get { return lineNumber; }
			set { lineNumber = value; }
		}

		public int LinePosition {
			get { return linePosition; }
			set { linePosition = value; }
		}

#if !NET_2_1
		internal XmlSchema CreateXsdSchema ()
		{
			XmlSchema s = new XmlSchema ();
			s.SourceUri = BaseURI;
			s.LineNumber = LineNumber;
			s.LinePosition = LinePosition;
			foreach (DTDElementDeclaration el in ElementDecls.Values)
				s.Items.Add (el.CreateXsdElement ());
			return s;
		}
#endif

		public string ResolveEntity (string name)
		{
			DTDEntityDeclaration decl = EntityDecls [name] 
				as DTDEntityDeclaration;
			if (decl == null) {
#if NET_2_1
				AddError (new XmlSchemaException (String.Format ("Required entity was not found: {0}", name), null, this.LineNumber, this.LinePosition));
#else
				AddError (new XmlSchemaException ("Required entity was not found.",
					this.LineNumber, this.LinePosition, null, this.BaseURI, null));
#endif
				return " ";
			}
			else
				return decl.EntityValue;
		}

		internal XmlResolver Resolver {
			get { return resolver; }
		}

		public XmlResolver XmlResolver {
			set { resolver = value; }
		}

		internal Hashtable ExternalResources {
			get { return externalResources; }
		}

		public DTDAutomataFactory Factory {
			get { return factory; }
		}

		public DTDElementDeclaration RootElement {
			get { return ElementDecls [Name]; }
		}

		public DTDElementDeclarationCollection ElementDecls {
			get { return elementDecls; }
		}

		public DTDAttListDeclarationCollection AttListDecls {
			get { return attListDecls; }
		}

		public DTDEntityDeclarationCollection EntityDecls {
			get { return entityDecls; }
		}

		public DTDParameterEntityDeclarationCollection PEDecls {
			get { return peDecls; }
		}

		public DTDNotationDeclarationCollection NotationDecls {
			get { return notationDecls; }
		}

		public DTDAutomata RootAutomata {
			get {
				if (rootAutomata == null)
					rootAutomata = new DTDElementAutomata (this, this.Name);
				return rootAutomata;
			}
		}

		public DTDEmptyAutomata Empty {
			get {
				if (emptyAutomata == null)
					emptyAutomata = new DTDEmptyAutomata (this);
				return emptyAutomata;
			}
		}

		public DTDAnyAutomata Any {
			get {
				if (anyAutomata == null)
					anyAutomata = new DTDAnyAutomata (this);
				return anyAutomata;
			}
		}

		public DTDInvalidAutomata Invalid {
			get {
				if (invalidAutomata == null)
					invalidAutomata = new DTDInvalidAutomata (this);
				return invalidAutomata;
			}
		}

		public XmlSchemaException [] Errors {
			get { return validationErrors.ToArray (typeof (XmlSchemaException)) as XmlSchemaException []; }
		}

		public void AddError (XmlSchemaException ex)
		{
			validationErrors.Add (ex);
		}

		internal string GenerateEntityAttributeText (string entityName)
		{
			DTDEntityDeclaration entity = EntityDecls [entityName] as DTDEntityDeclaration;
			if (entity == null)
				return null;
			return entity.EntityValue;
		}

		internal XmlTextReaderImpl GenerateEntityContentReader (string entityName, XmlParserContext context)
		{
			DTDEntityDeclaration entity = EntityDecls [entityName] as DTDEntityDeclaration;
			if (entity == null)
				return null;

			if (entity.SystemId != null) {
				Uri baseUri = entity.BaseURI == String.Empty ? null : new Uri (entity.BaseURI);
				Stream stream = resolver.GetEntity (resolver.ResolveUri (baseUri, entity.SystemId), null, typeof (Stream)) as Stream;
				return new XmlTextReaderImpl (stream, XmlNodeType.Element, context);
			}
			else
				return new XmlTextReaderImpl (entity.EntityValue, XmlNodeType.Element, context);
		}
	}

#if NET_2_0
	class DictionaryBase : List<KeyValuePair<string,DTDNode>>
	{
		public IEnumerable<DTDNode> Values {
			get {
				foreach (KeyValuePair<string,DTDNode> p in this)
					yield return p.Value;
			}
		}
	}
#endif
	internal class DTDCollectionBase : DictionaryBase
	{
		DTDObjectModel root;

		protected DTDCollectionBase (DTDObjectModel root)
		{
			this.root = root;
		}

		protected DTDObjectModel Root {
			get { return root; }
		}

#if NET_2_0
		public DictionaryBase InnerHashtable {
			get { return this; }
		}

		protected void BaseAdd (string name, DTDNode value)
		{
			base.Add (new KeyValuePair<string,DTDNode> (name, value));
		}

		public bool Contains (string key)
		{
			foreach (KeyValuePair<string,DTDNode> p in this)
				if (p.Key == key)
					return true;
			return false;
		}

		protected object BaseGet (string name)
		{
			foreach (KeyValuePair<string,DTDNode> p in this)
				if (p.Key == name)
					return p.Value;
			return null;
		}
#else
		public ICollection Keys {
			get { return InnerHashtable.Keys; }
		}

		public ICollection Values {
			get { return InnerHashtable.Values; }
		}

		protected void BaseAdd (string name, object value)
		{
			InnerHashtable.Add (name, value);
		}

		public bool Contains (string key)
		{
			return InnerHashtable.Contains (key);
		}

		protected object BaseGet (string name)
		{
			return InnerHashtable [name];
		}
#endif
	}

	internal class DTDElementDeclarationCollection : DTDCollectionBase
	{

		public DTDElementDeclarationCollection (DTDObjectModel root) : base (root) {}

		public DTDElementDeclaration this [string name] {
			get { return Get (name); }
		}

		public DTDElementDeclaration Get (string name)
		{
			return BaseGet (name) as DTDElementDeclaration;
		}

		public void Add (string name, DTDElementDeclaration decl)
		{
			if (Contains (name)) {
				Root.AddError (new XmlSchemaException (String.Format (
					"Element declaration for {0} was already added.",
					name), null));
				return;
			}
			decl.SetRoot (Root);
			BaseAdd (name, decl);
		}
	}

	internal class DTDAttListDeclarationCollection : DTDCollectionBase
	{
		public DTDAttListDeclarationCollection (DTDObjectModel root) : base (root) {}

		public DTDAttListDeclaration this [string name] {
			get { return BaseGet (name) as DTDAttListDeclaration; }
		}

		public void Add (string name, DTDAttListDeclaration decl)
		{
			DTDAttListDeclaration existing = this [name];
			if (existing != null) {
				// It is valid, that is additive declaration.
				foreach (DTDAttributeDefinition def in decl.Definitions)
					if (decl.Get (def.Name) == null)
						existing.Add (def);
			} else {
				decl.SetRoot (Root);
				BaseAdd (name, decl);
			}
		}
	}

	internal class DTDEntityDeclarationCollection : DTDCollectionBase
	{
		public DTDEntityDeclarationCollection (DTDObjectModel root) : base (root) {}

		public DTDEntityDeclaration this [string name] {
			get { return BaseGet (name) as DTDEntityDeclaration; }
		}

		public void Add (string name, DTDEntityDeclaration decl)
		{
			if (Contains (name))
				throw new InvalidOperationException (String.Format (
					"Entity declaration for {0} was already added.",
					name));
			decl.SetRoot (Root);
			BaseAdd (name, decl);
		}
	}

	internal class DTDNotationDeclarationCollection : DTDCollectionBase
	{
		public DTDNotationDeclarationCollection (DTDObjectModel root) : base (root) {}

		public DTDNotationDeclaration this [string name] {
			get { return BaseGet (name) as DTDNotationDeclaration; }
		}

		public void Add (string name, DTDNotationDeclaration decl)
		{
			if (Contains (name))
				throw new InvalidOperationException (String.Format (
					"Notation declaration for {0} was already added.",
					name));
			decl.SetRoot (Root);
			BaseAdd (name, decl);
		}
	}

	// This class contains either ElementName or ChildModels.
	internal class DTDContentModel : DTDNode
	{
		DTDObjectModel root;
		DTDAutomata compiledAutomata;

		string ownerElementName;
		string elementName;
		DTDContentOrderType orderType = DTDContentOrderType.None;
		DTDContentModelCollection childModels = new DTDContentModelCollection ();
		DTDOccurence occurence = DTDOccurence.One;

		internal DTDContentModel (DTDObjectModel root, string ownerElementName)
		{
			this.root = root;
			this.ownerElementName = ownerElementName;
		}

		public DTDContentModelCollection ChildModels {
			get { return childModels; }
			set { childModels = value; }
		}

		public DTDElementDeclaration ElementDecl {
			get { return root.ElementDecls [ownerElementName]; }
		}

		public string ElementName {
			get { return elementName; }
			set { elementName = value; }
		}

		public DTDOccurence Occurence {
			get { return occurence; }
			set { occurence = value; }
		}

		public DTDContentOrderType OrderType {
			get { return orderType; }
			set { orderType = value; }
		}

		public DTDAutomata GetAutomata ()
		{
			if (compiledAutomata == null)
				Compile ();
			return compiledAutomata;
		}

		public DTDAutomata Compile ()
		{
			compiledAutomata = CompileInternal ();
			return compiledAutomata;
		}

#if !NET_2_1
		internal XmlSchemaParticle CreateXsdParticle ()
		{
			XmlSchemaParticle p = CreateXsdParticleCore ();
			if (p == null)
				return null;

			switch (Occurence) {
			case DTDOccurence.Optional:
				p.MinOccurs = 0;
				break;
			case DTDOccurence.OneOrMore:
				p.MaxOccursString = "unbounded";
				break;
			case DTDOccurence.ZeroOrMore:
				p.MinOccurs = 0;
				p.MaxOccursString = "unbounded";
				break;
			}
			return p;
		}

		XmlSchemaParticle CreateXsdParticleCore ()
		{
			XmlSchemaParticle p = null;
			if (ElementName != null) {
				XmlSchemaElement el = new XmlSchemaElement ();
				SetLineInfo (el);
				el.RefName = new XmlQualifiedName (ElementName);
				return el;
			}
			else if (ChildModels.Count == 0)
				return null;
			else {
				XmlSchemaGroupBase gb = 
					(OrderType == DTDContentOrderType.Seq) ?
						(XmlSchemaGroupBase)
						new XmlSchemaSequence () :
						new XmlSchemaChoice ();
				SetLineInfo (gb);
				foreach (DTDContentModel cm in ChildModels.Items) {
					XmlSchemaParticle c = cm.CreateXsdParticle ();
					if (c != null)
						gb.Items.Add (c);
				}
				p = gb;
			}
			return p;
		}
#endif

		private DTDAutomata CompileInternal ()
		{
			if (ElementDecl.IsAny)
				return root.Any;
			if (ElementDecl.IsEmpty)
				return root.Empty;

			DTDAutomata basis = GetBasicContentAutomata ();
			switch (Occurence) {
			case DTDOccurence.One:
				return basis;
			case DTDOccurence.Optional:
				return Choice (root.Empty, basis);
			case DTDOccurence.OneOrMore:
				return new DTDOneOrMoreAutomata (root, basis);
			case DTDOccurence.ZeroOrMore:
				return Choice (root.Empty, new DTDOneOrMoreAutomata (root, basis));
			}
			throw new InvalidOperationException ();
		}

		private DTDAutomata GetBasicContentAutomata ()
		{
			if (ElementName != null)
				return new DTDElementAutomata (root, ElementName);
			switch (ChildModels.Count) {
			case 0:
				return root.Empty;
			case 1:
				return ChildModels [0].GetAutomata ();
			}

			DTDAutomata current = null;
			int childCount = ChildModels.Count;
			switch (OrderType) {
			case DTDContentOrderType.Seq:
				current = Sequence (
					ChildModels [childCount - 2].GetAutomata (),
					ChildModels [childCount - 1].GetAutomata ());
				for (int i = childCount - 2; i > 0; i--)
					current = Sequence (
						ChildModels [i - 1].GetAutomata (), current);
				return current;
			case DTDContentOrderType.Or:
				current = Choice (
					ChildModels [childCount - 2].GetAutomata (),
					ChildModels [childCount - 1].GetAutomata ());
				for (int i = childCount - 2; i > 0; i--)
					current = Choice (
						ChildModels [i - 1].GetAutomata (), current);
				return current;
			default:
				throw new InvalidOperationException ("Invalid pattern specification");
			}
		}

		private DTDAutomata Sequence (DTDAutomata l, DTDAutomata r)
		{
			return root.Factory.Sequence (l, r);
		}

		private DTDAutomata Choice (DTDAutomata l, DTDAutomata r)
		{
			return l.MakeChoice (r);
		}
	}

	internal class DTDContentModelCollection
	{
		ArrayList contentModel = new ArrayList ();

		public DTDContentModelCollection ()
		{
		}

		public IList Items {
			get { return contentModel; }
		}

		public DTDContentModel this [int i] {
			get { return contentModel [i] as DTDContentModel; }
		}

		public int Count {
			get { return contentModel.Count; }
		}

		public void Add (DTDContentModel model)
		{
			contentModel.Add (model);
		}
	}

	internal abstract class DTDNode : IXmlLineInfo
	{
		DTDObjectModel root;
		bool isInternalSubset;
		string baseURI;
		int lineNumber;
		int linePosition;

		public virtual string BaseURI {
			get { return baseURI; }
			set { baseURI = value; }
		}

		public bool IsInternalSubset {
			get { return isInternalSubset; }
			set { isInternalSubset = value; }
		}

		public int LineNumber {
			get { return lineNumber; }
			set { lineNumber = value; }
		}

		public int LinePosition {
			get { return linePosition; }
			set { linePosition = value; }
		}

		public bool HasLineInfo ()
		{
			return lineNumber != 0;
		}

		internal void SetRoot (DTDObjectModel root)
		{
			this.root = root;
			if (baseURI == null)
				this.BaseURI = root.BaseURI;
		}

		protected DTDObjectModel Root {
			get { return root; }
		}

		internal XmlException NotWFError (string message)
		{
			return new XmlException (this as IXmlLineInfo, BaseURI, message);
		}

#if !NET_2_1
		public void SetLineInfo (XmlSchemaObject obj)
		{
			obj.SourceUri = BaseURI;
			obj.LineNumber = LineNumber;
			obj.LinePosition = LinePosition;
		}
#endif
	}

	internal class DTDElementDeclaration : DTDNode
	{
		DTDObjectModel root;
		DTDContentModel contentModel;
		string name;
		bool isEmpty;
		bool isAny;
		bool isMixedContent;

		internal DTDElementDeclaration (DTDObjectModel root)
		{
			this.root = root;
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}
		public bool IsEmpty {
			get { return isEmpty; }
			set { isEmpty = value; }
		}

		public bool IsAny {
			get { return isAny; }
			set { isAny = value; }
		}

		public bool IsMixedContent {
			get { return isMixedContent; }
			set { isMixedContent = value; }
		}

		public DTDContentModel ContentModel {
			get {
				if (contentModel == null)
					contentModel = new DTDContentModel (root, Name);
				return contentModel;
			}
		}

		public DTDAttListDeclaration Attributes {
			get {
				return Root.AttListDecls [Name];
			}
		}

#if !NET_2_1
		internal XmlSchemaElement CreateXsdElement ()
		{
			XmlSchemaElement el = new XmlSchemaElement ();
			SetLineInfo (el);
			el.Name = Name;

			XmlSchemaComplexType ct = new XmlSchemaComplexType ();
			el.SchemaType = ct;
			if (Attributes != null) {
				SetLineInfo (ct);
				foreach (DTDAttributeDefinition a in
					Attributes.Definitions)
					ct.Attributes.Add (a.CreateXsdAttribute ());
			}
			if (IsEmpty)
				; // nothing to do
			else if (IsAny) {
				XmlSchemaAny any = new XmlSchemaAny ();
				any.MinOccurs = 0;
				any.MaxOccursString = "unbounded";
				ct.Particle = any;
			}
			else {
				if (IsMixedContent)
					ct.IsMixed = true;
				ct.Particle = ContentModel.CreateXsdParticle ();
			}

			/*
			if (IsEmpty) {
				el.SchemaType = new XmlSchemaComplexType ();
				SetLineInfo (el.SchemaType);
			}
			else if (IsAny)
				el.SchemaTypeName = new XmlQualifiedName (
					"anyType", XmlSchema.Namespace);
			else {
				XmlSchemaComplexType ct = new XmlSchemaComplexType ();
				SetLineInfo (ct);
				if (Attributes != null)
					foreach (DTDAttributeDefinition a in
						Attributes.Definitions)
						ct.Attributes.Add (a.CreateXsdAttribute ());
				if (IsMixedContent)
					ct.IsMixed = true;
				ct.Particle = ContentModel.CreateXsdParticle ();
				el.SchemaType = ct;
			}
			*/
			return el;
		}
#endif
	}

	internal class DTDAttributeDefinition : DTDNode
	{
		string name;
		XmlSchemaDatatype datatype;
		ArrayList enumeratedLiterals;
		string unresolvedDefault;
		ArrayList enumeratedNotations;
		DTDAttributeOccurenceType occurenceType = DTDAttributeOccurenceType.None;
		string resolvedDefaultValue;
		string resolvedNormalizedDefaultValue;

		internal DTDAttributeDefinition (DTDObjectModel root)
		{
			this.SetRoot (root);
		}

		public string Name {
			get { return name; }
			set { name =value; }
		}

		public XmlSchemaDatatype Datatype {
			get { return datatype; }
			set { datatype = value; }
		}

		public DTDAttributeOccurenceType OccurenceType {
			get { return this.occurenceType; }
			set { this.occurenceType = value; }
		}

		// entity reference inside enumerated values are not allowed,
		// but on the other hand, they are allowed inside default value.
		// Then I decided to use string ArrayList for enumerated values,
		// and unresolved string value for DefaultValue.
		public ArrayList EnumeratedAttributeDeclaration {
			get {
				if (enumeratedLiterals == null)
					enumeratedLiterals = new ArrayList ();
				return this.enumeratedLiterals;
			}
		}

		public ArrayList EnumeratedNotations {
			get {
				if (enumeratedNotations == null)
					enumeratedNotations = new ArrayList ();
				return this.enumeratedNotations;
			}
		}

		public string DefaultValue {
			get {
				if (resolvedDefaultValue == null)
					resolvedDefaultValue = ComputeDefaultValue ();
				return resolvedDefaultValue;
			}
		}

		public string NormalizedDefaultValue {
			get {
				if (resolvedNormalizedDefaultValue == null) {
					string s = ComputeDefaultValue ();
					try {
						object o = Datatype.ParseValue (s, null, null);
						resolvedNormalizedDefaultValue = 
							(o is string []) ? 
							String.Join (" ", (string []) o) :
							o is IFormattable ? ((IFormattable) o).ToString (null, CultureInfo.InvariantCulture) : o.ToString ();
					} catch (Exception) {
						// This is for non-error-reporting reader
						resolvedNormalizedDefaultValue = Datatype.Normalize (s);
					}
				}
				return resolvedNormalizedDefaultValue;
			}
		}

		public string UnresolvedDefaultValue {
			get { return this.unresolvedDefault; }
			set { this.unresolvedDefault = value; }
		}

		public char QuoteChar {
			get {
				return UnresolvedDefaultValue.Length > 0 ?
					this.UnresolvedDefaultValue [0] :
					'"';
			}
		}

#if !NET_2_1
		internal XmlSchemaAttribute CreateXsdAttribute ()
		{
			XmlSchemaAttribute a = new XmlSchemaAttribute ();
			SetLineInfo (a);
			a.Name = Name;
			a.DefaultValue = resolvedNormalizedDefaultValue;
			if (OccurenceType != DTDAttributeOccurenceType.Required)
				a.Use = XmlSchemaUse.Optional;

			XmlQualifiedName qname = XmlQualifiedName.Empty;
			ArrayList enumeration = null;
			if (enumeratedNotations != null && enumeratedNotations.Count > 0) {
				qname = new XmlQualifiedName ("NOTATION", XmlSchema.Namespace);
				enumeration = enumeratedNotations;
			}
			else if (enumeratedLiterals != null)
				enumeration = enumeratedLiterals;
			else {
				switch (Datatype.TokenizedType) {
				case XmlTokenizedType.ID:
					qname = new XmlQualifiedName ("ID", XmlSchema.Namespace); break;
				case XmlTokenizedType.IDREF:
					qname = new XmlQualifiedName ("IDREF", XmlSchema.Namespace); break;
				case XmlTokenizedType.IDREFS:
					qname = new XmlQualifiedName ("IDREFS", XmlSchema.Namespace); break;
				case XmlTokenizedType.ENTITY:
					qname = new XmlQualifiedName ("ENTITY", XmlSchema.Namespace); break;
				case XmlTokenizedType.ENTITIES:
					qname = new XmlQualifiedName ("ENTITIES", XmlSchema.Namespace); break;
				case XmlTokenizedType.NMTOKEN:
					qname = new XmlQualifiedName ("NMTOKEN", XmlSchema.Namespace); break;
				case XmlTokenizedType.NMTOKENS:
					qname = new XmlQualifiedName ("NMTOKENS", XmlSchema.Namespace); break;
				case XmlTokenizedType.NOTATION:
					qname = new XmlQualifiedName ("NOTATION", XmlSchema.Namespace); break;
				}
			}

			if (enumeration != null) {
				XmlSchemaSimpleType st = new XmlSchemaSimpleType ();
				SetLineInfo (st);
				XmlSchemaSimpleTypeRestriction r =
					new XmlSchemaSimpleTypeRestriction ();
				SetLineInfo (r);
				r.BaseTypeName = qname;
				if (enumeratedNotations != null) {
					foreach (string name in enumeratedNotations) {
						XmlSchemaEnumerationFacet f =
							new XmlSchemaEnumerationFacet ();
						SetLineInfo (f);
						r.Facets.Add (f);
						f.Value = name;
					}
				}
				st.Content = r;
			}
			else if (qname != XmlQualifiedName.Empty)
				a.SchemaTypeName = qname;
			return a;
		}
#endif

		internal string ComputeDefaultValue ()
		{
			if (UnresolvedDefaultValue == null)
				return null;

			StringBuilder sb = new StringBuilder ();
			int pos = 0;
			int next = 0;
			string value = this.UnresolvedDefaultValue;
			while ((next = value.IndexOf ('&', pos)) >= 0) {
				int semicolon = value.IndexOf (';', next);
				if (value [next + 1] == '#') {
					// character reference.
					char c = value [next + 2];
					NumberStyles style = NumberStyles.Integer;
					string spec;
					if (c == 'x' || c == 'X') {
						spec = value.Substring (next + 3, semicolon - next - 3);
						style |= NumberStyles.HexNumber;
					}
					else
						spec = value.Substring (next + 2, semicolon - next - 2);
					sb.Append ((char) int.Parse (spec, style, CultureInfo.InvariantCulture));
				} else {
					sb.Append (value.Substring (pos, next - 1));
					string name = value.Substring (next + 1, semicolon - 2);
					int predefined = XmlChar.GetPredefinedEntity (name);
					if (predefined >= 0)
						sb.Append (predefined);
					else
						sb.Append (Root.ResolveEntity (name));
				}
				pos = semicolon + 1;
			}
			sb.Append (value.Substring (pos));
			// strip quote chars
			string ret = sb.ToString (1, sb.Length - 2);
			sb.Length = 0;
			return ret;
		}

	}

	internal class DTDAttListDeclaration : DTDNode
	{
		string name;
		Hashtable attributeOrders = new Hashtable ();
		ArrayList attributes = new ArrayList ();

		internal DTDAttListDeclaration (DTDObjectModel root)
		{
			SetRoot (root);
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public DTDAttributeDefinition this [int i] {
			get { return Get (i); }
		}

		public DTDAttributeDefinition this [string name] {
			get { return Get (name); }
		}

		public DTDAttributeDefinition Get (int i)
		{
			return attributes [i] as DTDAttributeDefinition;
		}

		public DTDAttributeDefinition Get (string name)
		{
			object o = attributeOrders [name];
			if (o != null)
				return attributes [(int) o] as DTDAttributeDefinition;
			else
				return null;
		}

		public IList Definitions {
			get { return attributes; }
		}

		public void Add (DTDAttributeDefinition def)
		{
			if (attributeOrders [def.Name] != null)
				throw new InvalidOperationException (String.Format (
					"Attribute definition for {0} was already added at element {1}.",
					def.Name, this.Name));
			def.SetRoot (Root);
			attributeOrders.Add (def.Name, attributes.Count);
			attributes.Add (def);
		}

		public int Count {
			get { return attributeOrders.Count; }
		}
	}

	internal class DTDEntityBase : DTDNode
	{
		string name;
		string publicId;
		string systemId;
		string literalValue;
		string replacementText;
		string uriString;
		Uri absUri;
		bool isInvalid;
//		Exception loadException;
		bool loadFailed;
		XmlResolver resolver;

		protected DTDEntityBase (DTDObjectModel root)
		{
			SetRoot (root);
		}

		internal bool IsInvalid {
			get { return isInvalid; }
			set { isInvalid = value; }
		}

		public bool LoadFailed {
			get { return loadFailed; }
			set { loadFailed = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string PublicId {
			get { return publicId; }
			set { publicId = value; }
		}

		public string SystemId {
			get { return systemId; }
			set { systemId = value; }
		}

		public string LiteralEntityValue {
			get { return literalValue; }
			set { literalValue = value; }
		}

		public string ReplacementText {
			get { return replacementText; }
			set { replacementText = value; }
		}

		public XmlResolver XmlResolver {
			set { resolver = value; }
		}

		public string ActualUri {
			get {
				if (uriString == null) {
					if (resolver == null || SystemId == null || SystemId.Length == 0)
						uriString = BaseURI;
					else {
						Uri baseUri = null;
						try {
							if (BaseURI != null && BaseURI.Length > 0)
								baseUri = new Uri (BaseURI);
						} catch (UriFormatException) {
						}

						absUri = resolver.ResolveUri (baseUri, SystemId);
						uriString = absUri != null ? absUri.ToString () : String.Empty;
					}
				}
				return uriString;
			}
		}

		public void Resolve ()
		{
			if (ActualUri == String.Empty) {
				LoadFailed = true;
				LiteralEntityValue = String.Empty;
				return;
			}

			if (Root.ExternalResources.ContainsKey (ActualUri))
				LiteralEntityValue = (string) Root.ExternalResources [ActualUri];
			Stream s = null;
			try {
				s = resolver.GetEntity (absUri, null, typeof (Stream)) as Stream;
				XmlTextReaderImpl xtr = new XmlTextReaderImpl (ActualUri, s, Root.NameTable);
				// Don't skip Text declaration here. LiteralEntityValue contains it. See spec 4.5
				LiteralEntityValue = xtr.GetRemainder ().ReadToEnd ();

				Root.ExternalResources.Add (ActualUri, LiteralEntityValue);
				if (Root.ExternalResources.Count > DTDObjectModel.AllowedExternalEntitiesMax)
					throw new InvalidOperationException ("The total amount of external entities exceeded the allowed number.");

			} catch (Exception) {
//				loadException = ex;
				LiteralEntityValue = String.Empty;
				LoadFailed = true;
//				throw NotWFError ("Cannot resolve external entity. URI is " + ActualUri + " .");
			} finally {
				if (s != null)
					s.Close ();
			}
		}
	}

	internal class DTDEntityDeclaration : DTDEntityBase
	{
		string entityValue;
		string notationName;

		ArrayList ReferencingEntities = new ArrayList ();

		bool scanned;
		bool recursed;
		bool hasExternalReference;

		internal DTDEntityDeclaration (DTDObjectModel root) : base (root)
		{
		}

		public string NotationName {
			get { return notationName; }
			set { notationName = value; }
		}

		public bool HasExternalReference {
			get {
				if (!scanned)
					ScanEntityValue (new ArrayList ());
				return hasExternalReference;
			}
		}

		public string EntityValue {
			get {
				if (this.IsInvalid)
					return String.Empty;

				if (PublicId == null && SystemId == null && LiteralEntityValue == null)
					return String.Empty;

				if (entityValue == null) {
					if (NotationName != null)
						entityValue = "";
					else if (SystemId == null || SystemId == String.Empty) {
						entityValue = ReplacementText;
						if (entityValue == null)
							entityValue = String.Empty;
					} else {
						entityValue = ReplacementText;
					}
					// Check illegal recursion.
					ScanEntityValue (new ArrayList ());
				}
				return entityValue;
			}
		}

		// It returns whether the entity contains references to external entities.
		public void ScanEntityValue (ArrayList refs)
		{
			// To modify this code, beware nesting between this and EntityValue.
			string value = EntityValue;
			if (this.SystemId != null)
				hasExternalReference = true;

			if (recursed)
				throw NotWFError ("Entity recursion was found.");
			recursed = true;

			if (scanned) {
				foreach (string referenced in refs)
					if (this.ReferencingEntities.Contains (referenced))
						throw NotWFError (String.Format (
							"Nested entity was found between {0} and {1}",
							referenced, Name));
				recursed = false;
				return;
			}

			int len = value.Length;
			int start = 0;
			for (int i=0; i<len; i++) {
				switch (value [i]) {
				case '&':
					start = i+1;
					break;
				case ';':
					if (start == 0)
						break;
					string name = value.Substring (start, i - start);
					if (name.Length == 0)
						throw NotWFError ("Entity reference name is missing.");
					if (name [0] == '#')
						break;	// character reference
					if (XmlChar.GetPredefinedEntity (name) >= 0)
						break;	// predefined reference

					this.ReferencingEntities.Add (name);
					DTDEntityDeclaration decl = Root.EntityDecls [name];
					if (decl != null) {
						if (decl.SystemId != null)
							hasExternalReference = true;
						refs.Add (Name);
						decl.ScanEntityValue (refs);
						foreach (string str in decl.ReferencingEntities)
							ReferencingEntities.Add (str);
						refs.Remove (Name);
						value = value.Remove (start - 1, name.Length + 2);
						value = value.Insert (start - 1, decl.EntityValue);
						i -= name.Length + 1; // not +2, because of immediate i++ .
						len = value.Length;
					}
					start = 0;
					break;
				}
			}
			if (start != 0)
#if NET_2_1
				Root.AddError (new XmlSchemaException (this, this.BaseURI, "Invalid reference character '&' is specified."));
#else
				Root.AddError (new XmlSchemaException ("Invalid reference character '&' is specified.",
					this.LineNumber, this.LinePosition, null, this.BaseURI, null));
#endif
			scanned = true;
			recursed = false;
		}
	}

	internal class DTDNotationDeclaration : DTDNode
	{
		string name;
		string localName;
		string prefix;
		string publicId;
		string systemId;

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string PublicId {
			get { return publicId; }
			set { publicId = value; }
		}

		public string SystemId {
			get { return systemId; }
			set { systemId = value; }
		}

		public string LocalName {
			get { return localName; }
			set { localName = value; }
		}

		public string Prefix {
			get { return prefix; }
			set { prefix = value; }
		}

		internal DTDNotationDeclaration (DTDObjectModel root)
		{
			SetRoot (root);
		}
	}

	internal class DTDParameterEntityDeclarationCollection
	{
		Hashtable peDecls = new Hashtable ();
		DTDObjectModel root;

		public DTDParameterEntityDeclarationCollection (DTDObjectModel root)
		{
			this.root = root;
		}

		public DTDParameterEntityDeclaration this [string name] {
			get { return peDecls [name] as DTDParameterEntityDeclaration; }
		}

		public void Add (string name, DTDParameterEntityDeclaration decl)
		{
			// PEDecl can be overriden.
			if (peDecls [name] != null)
				return;
			decl.SetRoot (root);
			peDecls.Add (name, decl);
		}

		public ICollection Keys {
			get { return peDecls.Keys; }
		}

		public ICollection Values {
			get { return peDecls.Values; }
		}
	}

	internal class DTDParameterEntityDeclaration : DTDEntityBase
	{
		internal DTDParameterEntityDeclaration (DTDObjectModel root) : base (root)
		{
		}
	}

	internal enum DTDContentOrderType
	{
		None,
		Seq,
		Or
	}

	internal enum DTDAttributeOccurenceType
	{
		None,
		Required,
		Optional,
		Fixed
	}

	internal enum DTDOccurence
	{
		One,
		Optional,
		ZeroOrMore,
		OneOrMore
	}
}
