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
using System.Xml;

namespace Mono.Xml
{

	public class DTDObjectModel
	{
		public DTDObjectModel ()
		{
		}

		internal Hashtable ElementDecls = new Hashtable ();
		internal Hashtable AttListDecls = new Hashtable ();
		internal Hashtable EntityDecls = new Hashtable ();
		internal Hashtable NotationDecls = new Hashtable ();

		public string BaseURI {
			// XmlStreamParser.BaseURI
			get { return baseURI; }
		}

		public string Name;
		
		public string PublicId;
		
		public string SystemId;
		
		public string InternalSubset;
		
		string baseURI;
	}

	public enum DTDContentOrderType
	{
		None,
		Seq,
		Or
	}

	public enum DTDAttributeType
	{
		None,
		CData,
		Id,
		IdRef,
		IdRefs,
		Entity,
		Entities,
		NmToken,
		NmTokens,
		Notation
	}

	public enum DTDAttributeOccurenceType
	{
		None,
		Required,
		Optional,
		Fixed
	}

	public class DTDContentModel
	{
		public string ElementName;
		public DTDContentOrderType OrderType = DTDContentOrderType.None;
		public ArrayList ChildModels = new ArrayList ();
		public decimal MinOccurs = 1;
		public decimal MaxOccurs = 1;

		internal DTDContentModel () {}
	}

	public class DTDElementDeclaration : ICloneable
	{
		public string Name;
		public bool IsEmpty;
		public bool IsAny;
		public bool IsMixedContent;
		public DTDContentModel ContentModel = new DTDContentModel ();

		internal DTDElementDeclaration () {}

		public object Clone ()
		{
			return this.MemberwiseClone ();
		}
	}

	public class DTDAttributeDefinition : ICloneable
	{
		public string Name;
		public DTDAttributeType AttributeType = DTDAttributeType.None;
		// entity reference inside enumerated values are not allowed,
		// but on the other hand, they are allowed inside default value.
		// Then I decided to use string ArrayList for enumerated values,
		// and unresolved string value for DefaultValue.
		public ArrayList EnumeratedAttributeDeclaration = new ArrayList ();
		public string UnresolvedDefaultValue = null;
		public ArrayList EnumeratedNotations = new ArrayList();
		public DTDAttributeOccurenceType OccurenceType = DTDAttributeOccurenceType.None;

		internal DTDAttributeDefinition () {}

		public object Clone ()
		{
			return this.MemberwiseClone ();
		}
	}

	public class DTDAttListDeclaration : ICloneable
	{
		public string Name;
		public Hashtable AttributeDefinitions = new Hashtable ();

		internal DTDAttListDeclaration () {}

		public object Clone ()
		{
			return this.MemberwiseClone ();
		}
	}

	public class DTDEntityDeclaration
	{
		public string Name;
		public string PublicId;
		public string SystemId;
		public string NotationName;
		// FIXME: should have more complex value than simple string
		public string EntityValue;

		internal DTDEntityDeclaration () {}
	}

	public class DTDNotationDeclaration
	{
		public string Name;
		public string LocalName;
		public string Prefix;
		public string PublicId;
		public string SystemId;

		internal DTDNotationDeclaration () {}
	}

	public class DTDParameterEntityDeclaration
	{
		public string Name;
		public string PublicId;
		public string SystemId;
		public string BaseURI;
		public string Value;
	}
}
