//
// Mono.Xml.DTDObjectModel
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Mono.Xml.Schema;

namespace Mono.Xml
{
	public class DTDObjectModel
	{
		DTDElementDeclarationCollection elementDecls;
		DTDAttListDeclarationCollection attListDecls;
		DTDEntityDeclarationCollection entityDecls;
		DTDNotationDeclarationCollection notationDecls;

		public DTDObjectModel ()
		{
			elementDecls = new DTDElementDeclarationCollection (this);
			attListDecls = new DTDAttListDeclarationCollection (this);
			entityDecls = new DTDEntityDeclarationCollection (this);
			notationDecls = new DTDNotationDeclarationCollection (this);
			factory = new DTDAutomataFactory (this);
		}

		public string Name;
		
		public string PublicId;
		
		public string SystemId;
		
		public string InternalSubset;

		public bool InternalSubsetHasPEReference;
		
		public string ResolveEntity (string name)
		{
			DTDEntityDeclaration decl = EntityDecls [name] 
				as DTDEntityDeclaration;
			return decl.EntityValue;
		}

		private DTDAutomataFactory factory;
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

		public DTDNotationDeclarationCollection NotationDecls {
			get { return notationDecls; }
		}

		DTDElementAutomata rootAutomata;
		public DTDAutomata RootAutomata {
			get {
				if (rootAutomata == null)
					rootAutomata = new DTDElementAutomata (this, this.Name);
				return rootAutomata;
			}
		}

		DTDEmptyAutomata emptyAutomata;
		public DTDEmptyAutomata Empty {
			get {
				if (emptyAutomata == null)
					emptyAutomata = new DTDEmptyAutomata (this);
				return emptyAutomata;
			}
		}

		DTDAnyAutomata anyAutomata;
		public DTDAnyAutomata Any {
			get {
				if (anyAutomata == null)
					anyAutomata = new DTDAnyAutomata (this);
				return anyAutomata;
			}
		}

		DTDInvalidAutomata invalidAutomata;
		public DTDInvalidAutomata Invalid {
			get {
				if (invalidAutomata == null)
					invalidAutomata = new DTDInvalidAutomata (this);
				return invalidAutomata;
			}
		}
	}

	public class DTDElementDeclarationCollection
	{
		Hashtable elementDecls = new Hashtable ();
		DTDObjectModel root;

		public DTDElementDeclarationCollection (DTDObjectModel root)
		{
			this.root = root;
		}

		public DTDElementDeclaration this [string name] {
			get { return elementDecls [name] as DTDElementDeclaration; }
		}

		public void Add (string name, DTDElementDeclaration decl)
		{
			if (elementDecls [name] != null)
				throw new InvalidOperationException (String.Format (
					"Element declaration for {0} was already added.",
					name));
			decl.SetRoot (root);
			elementDecls.Add (name, decl);
		}

		public ICollection Keys {
			get { return elementDecls.Keys; }
		}

		public ICollection Values {
			get { return elementDecls.Values; }
		}
	}

	public class DTDAttListDeclarationCollection
	{
		Hashtable attListDecls = new Hashtable ();
		DTDObjectModel root;

		public DTDAttListDeclarationCollection (DTDObjectModel root)
		{
			this.root = root;
		}

		public DTDAttListDeclaration this [string name] {
			get { return attListDecls [name] as DTDAttListDeclaration; }
		}

		public void Add (string name, DTDAttListDeclaration decl)
		{
			DTDAttListDeclaration existing = this [name];
			if (existing != null) {
				// It should be valid and 
				// has effect of additive declaration.
				foreach (DTDAttributeDefinition def in decl.Definitions)
					if (decl.Get (def.Name) == null)
						existing.Add (def);
			} else {
				decl.SetRoot (root);
				attListDecls.Add (name, decl);
			}
		}

		public ICollection Keys {
			get { return attListDecls.Keys; }
		}

		public ICollection Values {
			get { return attListDecls.Values; }
		}
	}

	public class DTDEntityDeclarationCollection
	{
		Hashtable entityDecls = new Hashtable ();
		DTDObjectModel root;

		public DTDEntityDeclarationCollection (DTDObjectModel root)
		{
			this.root = root;
		}

		public DTDEntityDeclaration this [string name] {
			get { return entityDecls [name] as DTDEntityDeclaration; }
		}

		public void Add (string name, DTDEntityDeclaration decl)
		{
			if (entityDecls [name] != null)
				throw new InvalidOperationException (String.Format (
					"Entity declaration for {0} was already added.",
					name));
			decl.SetRoot (root);
			entityDecls.Add (name, decl);
		}

		public ICollection Keys {
			get { return entityDecls.Keys; }
		}

		public ICollection Values {
			get { return entityDecls.Values; }
		}
	}

	public class DTDNotationDeclarationCollection
	{
		Hashtable notationDecls = new Hashtable ();
		DTDObjectModel root;

		public DTDNotationDeclarationCollection (DTDObjectModel root)
		{
			this.root = root;
		}

		public DTDNotationDeclaration this [string name] {
			get { return notationDecls [name] as DTDNotationDeclaration; }
		}

		public void Add (string name, DTDNotationDeclaration decl)
		{
			if (notationDecls [name] != null)
				throw new InvalidOperationException (String.Format (
					"Notation declaration for {0} was already added.",
					name));
			decl.SetRoot (root);
			notationDecls.Add (name, decl);
		}

		public ICollection Keys {
			get { return notationDecls.Keys; }
		}

		public ICollection Values {
			get { return notationDecls.Values; }
		}
	}

	public class DTDContentModel
	{
		private DTDObjectModel root;
		DTDAutomata compiledAutomata;

		private string ownerElementName;
		public string ElementName;
		public DTDContentOrderType OrderType = DTDContentOrderType.None;
		public DTDContentModelCollection ChildModels 
			= new DTDContentModelCollection ();
		public DTDOccurence Occurence = DTDOccurence.One;

		internal DTDContentModel (DTDObjectModel root, string ownerElementName)
		{
			this.root = root;
			this.ownerElementName = ownerElementName;
		}

		public DTDElementDeclaration ElementDecl {
			get {
			      return root.ElementDecls [ownerElementName];
			}
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

	public class DTDContentModelCollection
	{
		ArrayList contentModel = new ArrayList ();

		public DTDContentModelCollection ()
		{
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

	public abstract class DTDNode
	{
		private DTDObjectModel root;

		internal void SetRoot (DTDObjectModel root)
		{
			this.root = root;
		}

		protected DTDObjectModel Root {
			get { return root; }
		}
	}

	public class DTDElementDeclaration : DTDNode // : ICloneable
	{
		public string Name;
		public bool IsEmpty;
		public bool IsAny;
		public bool IsMixedContent;
		public DTDContentModel contentModel;
		DTDObjectModel root;

		internal DTDElementDeclaration (DTDObjectModel root)
		{
			this.root = root;
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

//		public object Clone ()
//		{
//			return this.MemberwiseClone ();
//		}
	}

	public class DTDAttributeDefinition : DTDNode// : ICloneable
	{
		public string Name;
		public XmlSchemaDatatype Datatype;
		// entity reference inside enumerated values are not allowed,
		// but on the other hand, they are allowed inside default value.
		// Then I decided to use string ArrayList for enumerated values,
		// and unresolved string value for DefaultValue.
		public ArrayList EnumeratedAttributeDeclaration = new ArrayList ();
		public string UnresolvedDefaultValue = null;
		public ArrayList EnumeratedNotations = new ArrayList();
		public DTDAttributeOccurenceType OccurenceType = DTDAttributeOccurenceType.None;
		private string resolvedDefaultValue;
		private string resolvedNormalizedDefaultValue;

		internal DTDAttributeDefinition () {}

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
					object o = (string) Datatype.ParseValue (ComputeDefaultValue (), null, null);
					resolvedNormalizedDefaultValue = 
						(o is string []) ? 
						String.Join (" ", (string []) o) :
						o.ToString ();
				}
				return resolvedNormalizedDefaultValue;
			}
		}

		private string ComputeDefaultValue ()
		{
			if (UnresolvedDefaultValue == null)
				return null;

			StringBuilder sb = new StringBuilder ();
			int pos = 0;
			int next = 0;
			while ((next = this.UnresolvedDefaultValue.IndexOf ('&', pos)) >= 0) {
				sb.Append (this.UnresolvedDefaultValue.Substring (pos, next - 1));
				int semicolon = this.UnresolvedDefaultValue.IndexOf (';', next);
				string name = this.UnresolvedDefaultValue.Substring (pos + 1, semicolon - 1);
				sb.Append (Root.ResolveEntity (name));
			}
			sb.Append (this.UnresolvedDefaultValue.Substring (pos));
			// strip quote chars
			string ret = sb.ToString (1, sb.Length - 2);
			sb.Length = 0;
			return ret;
		}

		public char QuoteChar {
			get {
				return UnresolvedDefaultValue.Length > 0 ?
					this.UnresolvedDefaultValue [0] :
					'"';
			}
		}

//		public object Clone ()
//		{
//			return this.MemberwiseClone ();
//		}
	}

	public class DTDAttListDeclaration : DTDNode // : ICloneable
	{
		public string Name;

		internal DTDAttListDeclaration () {}
		private Hashtable attributeOrders = new Hashtable ();
		private ArrayList attributes = new ArrayList ();

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

		public ICollection Definitions {
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

//		public object Clone ()
//		{
//			return this.MemberwiseClone ();
//		}
	}

	public class DTDEntityDeclaration : DTDNode
	{
		public string Name;
		public string PublicId;
		public string SystemId;
		public string NotationName;
		// FIXME: should have more complex value than simple string
		public string EntityValue;
		public bool IsInternalSubset;

		internal DTDEntityDeclaration () {}
	}

	public class DTDNotationDeclaration : DTDNode
	{
		public string Name;
		public string LocalName;
		public string Prefix;
		public string PublicId;
		public string SystemId;

		internal DTDNotationDeclaration () {}
	}

	public class DTDParameterEntityDeclaration : DTDNode
	{
		public string Name;
		public string PublicId;
		public string SystemId;
		public string BaseURI;
		public string Value;
	}

	public enum DTDContentOrderType
	{
		None,
		Seq,
		Or
	}

	public enum DTDAttributeOccurenceType
	{
		None,
		Required,
		Optional,
		Fixed
	}

	public enum DTDOccurence
	{
		One,
		Optional,
		ZeroOrMore,
		OneOrMore
	}
}
