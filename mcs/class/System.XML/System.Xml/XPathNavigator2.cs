//
// XPathNavigator2.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C)2003 Atsushi Enomoto
//
#if NET_1_2

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Security.Policy;
//using System.Xml.Query;
using System.Xml.Schema;
using System.Xml.XPath;
//using Mono.Xml.XPath2;
//using MS.Internal.Xml;

namespace System.Xml
{
	public abstract class XPathNavigator2 
		: ICloneable, ICustomTypeDescriptor, IXmlDataEvidence,
		IXmlNamespaceResolver, IXPathNavigator//,
//		MS.Internal.Xml.IXmlInfoItem
	{
		protected XPathNavigator2 ()
		{
		}

		public abstract string BaseUri { get; }

		public virtual Evidence [] Evidences {
			 get { throw new NotImplementedException (); }
		}

		public bool HasAttributes {
			get {
				if (ItemType != XmlInfoItemType.Element)
					return false;
				if (MoveToFirstAttribute () == null)
					return false;
				MoveToParent ();
				return true;
			}
		}

		public bool HasChildren {
			get {
				if (MoveToFirstChild () == null)
					return false;
				MoveToParent ();
				return true;
			}
		}
		
		public string InnerXml {
			 get { throw new NotImplementedException (); }
		}
		
		public abstract XmlInfoItemType ItemType { get; }
		
		public abstract string LocalName { get; }
		
		public abstract string Name { get; }
		
		public abstract string Namespace { get; }
		
		public abstract XmlNameTable NameTable { get; }

		public string OuterXml {
			 get { throw new NotImplementedException (); }
		}
		
		public abstract string Prefix { get; } 
		
//		public MS.Internal.Xml.IXmlSchemaInfo SchemaInfo {
//			 get { throw new NotImplementedException (); }
//		}

		public virtual object Schemas {
			 get { throw new NotImplementedException (); }
		}
		
		public virtual Type StorageType { 
			 get { throw new NotImplementedException (); }
		}
		
		public virtual object UnderlyingObject { 
			 get { throw new NotImplementedException (); }
		}
		
//		public virtual IXmlType XmlType { 
//			 get { throw new NotImplementedException (); }
//		}

		public virtual bool CheckValidity (XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		public abstract XPathNavigator2 Clone ();

		IXPathNavigator IXPathNavigator.Clone ()
		{
			return this.Clone ();
		}

		public abstract XmlNodeOrder ComparePosition (IXPathNavigator other);

//		public IXmlInfosetReader CopyToReader ()
//		{
//			throw new NotImplementedException ();
//		}

		// IMHO it should be virtual, even if Microsoft has excellent implementation.
		public string GetAttribute (string localName, string namespaceName)
		{
			string value = null;
			XmlInfoItemType cacheItemType = ItemType;
			string currentName = LocalName;
			string currentNs = Namespace;
			IXPathNavigator tmp = MoveToAttribute (localName, namespaceName);
			if (tmp != null) {
				value = ReadStringValue ();
				MoveToParent ();
				switch (cacheItemType) {
				case XmlInfoItemType.Attribute:
					MoveToAttribute (currentName, currentNs);
					break;
				case XmlInfoItemType.Namespace:
					MoveToNamespace (currentNs);
					break;
				}
			}
			return value;
		}

		public virtual StringDictionary GetNamespacesInScope (XmlNamespaceScope scope)
		{
			StringDictionary dict = new StringDictionary ();
			XPathNavigator2 nav = Clone ();
			IXPathNavigator ns = nav.MoveToFirstNamespace (scope);
			if (ns != null) {
				do {
					dict.Add (nav.LocalName, nav.Namespace);
					ns = nav.MoveToNextNamespace (scope);
				} while (ns != null);
			}
			return dict;
		}

		/*private*/ object ICloneable.Clone()
		{
			return Clone ();
		}

		/*private*/ AttributeCollection ICustomTypeDescriptor.GetAttributes ()
		{
			throw new NotImplementedException ();
		}

		/*private*/ string ICustomTypeDescriptor.GetClassName()
		{
			throw new NotImplementedException ();
		}

		/*private*/ string ICustomTypeDescriptor.GetComponentName()
		{
			throw new NotImplementedException ();
		}

		/*private*/ TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			throw new NotImplementedException ();
		}

		/*private*/ EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			throw new NotImplementedException ();
		}

		/*private*/ PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			throw new NotImplementedException ();
		}

		/*private*/ object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			throw new NotImplementedException ();
		}

		/*private*/ EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			throw new NotImplementedException ();
		}

		/*private*/ EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute[] attrs)
		{
			throw new NotImplementedException ();
		}

		/*private*/ PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			throw new NotImplementedException ();
		}

		/*private*/ PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute [] attrs)
		{
			throw new NotImplementedException ();
		}

		/*private*/ object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			throw new NotImplementedException ();
		}

		public virtual bool IsDescendant (IXPathNavigator other)
		{
			IXPathNavigator nav = other as XPathNavigator2;
			if (nav == null)
				throw new ArgumentException ();

			nav = nav.Clone ();
			do {
				if (IsSamePosition (nav))
					return true;
				nav = nav.MoveToParent ();
			} while (nav != null);

			return false;
		}

		public virtual bool IsSamePosition (IXPathNavigator other)
		{
			return (ComparePosition (other) == XmlNodeOrder.Same);
		}
/*
		MS.Internal.Xml.IXmlInfosetReader IXmlInfoItem.CopyToReader ()
		{
			throw new NotImplementedException ();
		}

		IXPathNavigator IXPathNavigator.Clone ()
		{
			return Clone ();
		}
*/
		public virtual string LookupNamespace (string prefix)
		{
			return LookupNamespace (prefix, false);
		}

		public virtual string LookupNamespace (string prefix, bool atomizedNames)
		{
			return LookupNamespaceNode (prefix, atomizedNames, true);
		}
		
		public virtual string LookupPrefix (string ns)  
		{
			return LookupPrefix (ns, false);
		}

		public virtual string LookupPrefix (string ns, bool atomizedNames)
		{
			return LookupNamespaceNode (ns, atomizedNames, false);
		}

		private string LookupNamespaceNode (string str, bool atomizedNames, bool isPrefix)
		{
			string value = null;
			XmlInfoItemType cacheItemType = ItemType;
			string currentName = LocalName;
			string currentNs = Namespace;
			IXPathNavigator tmp = MoveToFirstNamespace (XmlNamespaceScope.All);
			if (tmp == null)
				return null;
			do {
				if (atomizedNames) {
					if (isPrefix) {
						if (Object.ReferenceEquals (LocalName, str)) {
							value = Namespace;
							break;
						}
					} else {
						if (Object.ReferenceEquals (Namespace, str)) {
							value = LocalName;
							break;
						}
					}
				} else {
					if (isPrefix) {
						if (LocalName == str) {
							value = Namespace;
							break;
						}
					} else {
						if (Namespace == str) {
							value = Name;
							break;
						}
					}
				}
				tmp = MoveToNextNamespace (XmlNamespaceScope.All);
			} while (tmp != null);
			MoveToParent ();

			switch (cacheItemType) {
			case XmlInfoItemType.Attribute:
				MoveToAttribute (currentName, currentNs);
				break;
			case XmlInfoItemType.Namespace:
				MoveToNamespace (currentNs);
				break;
			}

			return value;
		}

		public abstract IXPathNavigator MoveTo (IXPathNavigator other);
		// In the meantime, we should wait System.Data.SqlXml.dll became public.
//		public abstract XPathNavigator2 MoveTo (XmlCommand command, XmlQueryArgumentList argList);
		public abstract XPathNavigator2 MoveTo (string query);
		public abstract XPathNavigator2 MoveTo (string query, IXmlNamespaceResolver namespaceResolver);
		public abstract XPathNavigator2 MoveTo (string query, IXmlNamespaceResolver namespaceResolver, XmlQueryDialect dialect);

		public virtual XPathNavigator2 MoveToAttribute (string localName, string namespaceName)
		{
			return MoveToAttribute (localName, namespaceName, false) as XPathNavigator2;
		}

		public virtual IXPathNavigator MoveToAttribute (string localName, string namespaceName, bool atomizedNames)
		{
			XmlInfoItemType cacheItemType = ItemType;
			string currentName = LocalName;
			string currentNs = Namespace;
			IXPathNavigator tmp = MoveToFirstAttribute ();
			if (tmp == null)
				return null;
			do {
				if (atomizedNames) {
					if (Object.ReferenceEquals (LocalName, localName) && Object.ReferenceEquals (Namespace, namespaceName))
						return this;
				} else {
					if (LocalName == localName && Namespace == namespaceName)
						return this;
				}
				tmp = MoveToNextAttribute ();
			} while (tmp != null);
			MoveToParent ();

			switch (cacheItemType) {
			case XmlInfoItemType.Attribute:
				MoveToAttribute (currentName, currentNs);
				break;
			case XmlInfoItemType.Namespace:
				MoveToNamespace (currentNs);
				break;
			}

			return null;
		}

		public virtual IXPathNavigator MoveToChild (string localName, string namespaceName, bool atomizedNames)
		{
			return MoveToChild (localName, namespaceName, atomizedNames, XmlInfoItemType.Document, false);
		}
		
		public virtual IXPathNavigator MoveToChild (XmlInfoItemType type)
		{
			return MoveToChild (null, null, false, type, true);
		}

		public virtual XPathNavigator2 MoveToChild (string localName, string namespaceName)
		{
			return MoveToChild (localName, namespaceName, false) as XPathNavigator2;
		}

		private IXPathNavigator MoveToChild (string localName, string namespaceName, bool atomizedNames, XmlInfoItemType type, bool byType)
		{
			XmlInfoItemType cacheItemType = ItemType;
			string currentName = LocalName;
			string currentNs = Namespace;
			IXPathNavigator tmp = MoveToFirstChild ();
			if (tmp == null)
				return null;

			do {
				if (byType) {
					if (type == ItemType)
						return this;
				} else if (atomizedNames) {
					if (Object.ReferenceEquals (LocalName, localName) && Object.ReferenceEquals (Namespace, namespaceName))
						return this;
				} else {
					if (LocalName == localName && Namespace == namespaceName)
						return this;
				}
				tmp = MoveToNextSibling ();
			} while (tmp != null);
			MoveToParent ();

			switch (cacheItemType) {
			case XmlInfoItemType.Attribute:
				MoveToAttribute (currentName, currentNs);
				break;
			case XmlInfoItemType.Namespace:
				MoveToNamespace (currentNs);
				break;
			}

			return null;
		}

		public virtual IXPathNavigator MoveToDescendantOf (IXPathNavigator root, string localName, string namespaceName, bool atomizedNames)
		{
			throw new NotImplementedException ();
		}

		public virtual IXPathNavigator MoveToDescendantOf (IXPathNavigator root, XmlInfoItemType type)  
		{
			throw new NotImplementedException ();
		}

		public abstract IXPathNavigator MoveToFirstAttribute ();

		public abstract IXPathNavigator MoveToFirstChild ();

		public abstract IXPathNavigator MoveToFirstNamespace (XmlNamespaceScope scope);

		public abstract IXPathNavigator MoveToFirstValue ();


		public virtual IXPathNavigator MoveToId (string id)
		{
			throw new NotImplementedException ();
		}

		public XPathNavigator2 MoveToNamespace (string prefix)  
		{
			return MoveToNamespace (prefix, false);
		}

		public XPathNavigator2 MoveToNamespace (string prefix, bool atomizedNames)  
		{
			throw new NotImplementedException ();
		}

		public abstract IXPathNavigator MoveToNextAttribute ();

		public abstract IXPathNavigator MoveToNextNamespace (XmlNamespaceScope scope);

		public abstract IXPathNavigator MoveToNextSibling ();

		public abstract IXPathNavigator MoveToNextValue ();

		public abstract IXPathNavigator MoveToParent ();

		public virtual IXPathNavigator MoveToPreviousSibling ()
		{
			IXPathNavigator backup = Clone ();
			IXPathNavigator tmp = MoveToParent ();
			if (tmp == null)
				return null;
			tmp = MoveToFirstChild ();
			do {
				if (tmp.IsSamePosition (backup))
					return this;
				tmp = MoveToNextSibling ();
			} while (tmp != null);

			return null;
		}

		public virtual IXPathNavigator MoveToRoot ()
		{
			IXPathNavigator tmp = MoveToParent ();
			while (tmp != null)
				MoveToParent ();
			return this;
		}

		public virtual XPathNavigator2 MoveToSibling (string localName, string namespaceName)
		{
			return (XPathNavigator2) MoveToSibling (localName, namespaceName, false);
		}

		public virtual IXPathNavigator MoveToSibling (string localName, string namespaceName, bool atomizedNames)
		{
			return MoveToSibling (localName, namespaceName, atomizedNames, XmlInfoItemType.Document, false);
		}
		
		public virtual IXPathNavigator MoveToSibling (XmlInfoItemType type)
		{
			return MoveToSibling (null, null, false, type, true);
		}

		private IXPathNavigator MoveToSibling (string localName, string namespaceName, bool atomizedNames, XmlInfoItemType type, bool byType)
		{
			XmlInfoItemType cacheItemType = ItemType;
			string currentName = LocalName;
			string currentNs = Namespace;
			IXPathNavigator tmp = MoveToNextSibling ();
			if (tmp == null)
				return null;

			int count = 0;
			while (tmp != null) {
				count++;
				if (byType) {
					if (type == ItemType)
						return this;
				} else if (atomizedNames) {
					if (Object.ReferenceEquals (LocalName, localName) && Object.ReferenceEquals (Namespace, namespaceName))
						return this;
				} else {
					if (LocalName == localName && Namespace == namespaceName)
						return this;
				}
				tmp = MoveToNextSibling ();
			}
			for (int i = 0; i < count; i++)
				MoveToPreviousSibling ();
			return null;
		}

		public virtual bool ReadboolValue ()
		{
			throw new NotImplementedException ();
		}

		public virtual byte ReadByteValue ()
		{
			throw new NotImplementedException ();
		}

		public virtual char ReadCharValue ()
		{
			throw new NotImplementedException ();
		}

		public virtual DateTime ReadDateTimeValue ()
		{
			throw new NotImplementedException ();
		}

		public virtual decimal ReadDecimalValue ()
		{
			throw new NotImplementedException ();
		}

		public virtual double ReadDoubleValue ()
		{
			throw new NotImplementedException ();
		}

		public virtual short ReadInt16Value ()
		{
			throw new NotImplementedException ();
		}

		public virtual int ReadintValue ()
		{
			throw new NotImplementedException ();
		}

		public virtual long ReadInt64Value ()
		{
			throw new NotImplementedException ();
		}

		public virtual float ReadSingleValue ()
		{
			throw new NotImplementedException ();
		}

		public virtual string ReadStringValue ()
		{
			throw new NotImplementedException ();
		}

		public virtual object ReadValue ()  
		{
			throw new NotImplementedException ();
		}

		public virtual object ReadValue (Type type)  
		{
			throw new NotImplementedException ();
		}

		// TODO
		// In the meantime, we should wait System.Data.SqlXml.dll became public.
//		public IEnumerable Select (XmlCommand command, XmlQueryArgumentList argList)  
//		{
//			throw new NotImplementedException ();
//		}

		public IEnumerable Select (string query)  
		{
			return Select (query, null);
		}

		public IEnumerable Select (string query, IXmlNamespaceResolver namespaceResolver)  
		{
			// TODO: check the true default dialect
			return Select (query, namespaceResolver, XmlQueryDialect.XPath1);
		}

		// TODO
		public IEnumerable Select (string query, IXmlNamespaceResolver namespaceResolver, XmlQueryDialect dialect)
		{
			throw new NotImplementedException ();
		}

		// TODO
		// In the meantime, we should wait System.Data.SqlXml.dll became public.
//		public object SelectSingleValue (XmlCommand command, XmlQueryArgumentList argList)
//		{
//			throw new NotImplementedException ();
//		}
  
		public object SelectSingleValue (string query)  
		{
			return SelectSingleValue (query, null);
		}

		public object SelectSingleValue (string query, IXmlNamespaceResolver namespaceResolver)  
		{
			return SelectSingleValue (query, namespaceResolver, XmlQueryDialect.XPath1);
		}

		// TODO
		public object SelectSingleValue (string query, IXmlNamespaceResolver namespaceResolver, XmlQueryDialect dialect)  
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
