//
// XmlTypeMapping.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002 John Donagher
//

using System.Xml;
using System;
using System.Collections;

namespace System.Xml.Serialization
{
	public class XmlTypeMapping : XmlMapping
	{
		private string elementName;
		private string ns;
		private string xmlType;
		TypeData type;
		ObjectMap map;

		ArrayList _derivedTypes = new ArrayList();

		internal XmlTypeMapping(string elementName, string ns, TypeData typeData, string xmlType)
		{
			this.elementName = elementName;
			this.ns = ns;
			this.type = typeData;
			this.xmlType = xmlType;
		}

		public string ElementName
		{
			get { return elementName; }
		}

		public string Namespace
		{
			get { return ns; }
		}

		public string TypeFullName
		{
			get { return type.FullTypeName; }
		}

		public string TypeName
		{
			get { return type.TypeName; }
		}

		internal TypeData TypeData
		{
			get { return type; }
		}

		internal string XmlType
		{
			get { return xmlType; }
		}

		internal ArrayList DerivedTypes
		{
			get { return _derivedTypes; }
			set { _derivedTypes = value; }
		}

		internal XmlTypeMapping GetRealTypeMap (string objectFullTypeName)
		{
			// Returns the map for a subtype of this map's type

			if (TypeFullName == objectFullTypeName) return this;
			foreach (XmlTypeMapping map in _derivedTypes)
				if (map.TypeFullName == objectFullTypeName) return map;

			throw new InvalidOperationException ("Invalid type: " + objectFullTypeName);
		}

		internal XmlTypeMapping GetRealElementMap (string name, string ens)
		{
			if (xmlType == name && ns == ens) return this;
			foreach (XmlTypeMapping map in _derivedTypes)
				if (map.xmlType == name && map.ns == ens) return map;
			return null;
		}

		internal ObjectMap ObjectMap
		{
			get { return map; }
			set { map = value; }
		}
	}

	internal class ObjectMap
	{
	}


	// Mapping info for classes and structs

	internal class ClassMap: ObjectMap
	{
		Hashtable _elements;
		ArrayList _elementMembers;
		Hashtable _attributeMembers;
		ArrayList _flatLists;

		public void AddMember (XmlTypeMapMember member)
		{
			if (member is XmlTypeMapMemberAttribute)
			{
				XmlTypeMapMemberAttribute atm = (XmlTypeMapMemberAttribute)member;
				if (_attributeMembers == null) _attributeMembers = new Hashtable();
				_attributeMembers.Add (atm.AttributeName + "/" + atm.Namespace, member);
				return;
			}
			else if (member is XmlTypeMapMemberFlatList)
			{
				if (_flatLists == null) _flatLists = new ArrayList ();
				((XmlTypeMapMemberFlatList)member).FlatArrayIndex = _flatLists.Count;
				_flatLists.Add (member);
			}

			if (_elementMembers == null) {
				_elementMembers = new ArrayList();
				_elements = new Hashtable();
			}

			member.Index = _elementMembers.Count;
			_elementMembers.Add (member);

			ICollection elemsInfo = ((XmlTypeMapMemberElement)member).ElementInfo;
			foreach (XmlTypeMapElementInfo elem in elemsInfo)
			{
				string key = elem.ElementName+"/"+elem.Namespace;
				if (_elements.ContainsKey (key)) 
					throw new InvalidOperationException ("The XML element named '" + elem.ElementName + "' from namespace '" + elem.Namespace + "' already present in the current scope. Use XML attributes to specify another XML name or namespace for the element.");
				_elements.Add (key, elem);
			}
		}

		public XmlTypeMapMemberAttribute GetAttribute (string name, string ns)
		{
			if (_attributeMembers == null) return null;
			return (XmlTypeMapMemberAttribute)_attributeMembers[name + "/" + ns];
		}

		public XmlTypeMapElementInfo GetElement (string name, string ns)
		{
			if (_elements == null) return null;
			return (XmlTypeMapElementInfo)_elements[name + "/" + ns];
		}

		public ICollection AttributeMembers
		{
			get { return (_attributeMembers != null) ? _attributeMembers.Values : null; }
		}

		public ICollection ElementMembers
		{
			get { return _elementMembers; }
		}

		public ICollection FlatLists
		{
			get { return _flatLists; }
		}
	}

	// Mapping info for arrays and lists

	internal class ListMap: ObjectMap
	{
		XmlTypeMapElementInfoList _itemInfo;

		public XmlTypeMapElementInfoList ItemInfo
		{
			get { return _itemInfo; }
			set { _itemInfo = value; }
		}

		public XmlTypeMapElementInfo FindElement (object memberValue)
		{
			if (_itemInfo.Count == 1) 
				return (XmlTypeMapElementInfo) _itemInfo[0];
			else
			{
				if (memberValue == null) return null;
				Type type = memberValue.GetType();
				foreach (XmlTypeMapElementInfo elem in _itemInfo)
					if (elem.TypeData.Type == type) return elem;
			}
			return null;
		}	

		public XmlTypeMapElementInfo FindElement (string elementName, string ns)
		{
			foreach (XmlTypeMapElementInfo elem in _itemInfo)
				if (elem.ElementName == elementName && elem.Namespace == ns) return elem;
			return null;
		}

		public override bool Equals (object other)
		{
			ListMap lmap = other as ListMap;
			if (lmap == null) return false;

			if (_itemInfo.Count != lmap._itemInfo.Count) return false;
			for (int n=0; n<_itemInfo.Count; n++)
				if (!_itemInfo[n].Equals (lmap._itemInfo[n])) return false;
			return true;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}
}
