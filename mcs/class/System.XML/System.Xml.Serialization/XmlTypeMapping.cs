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
		bool multiReferenceType = false;

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

		internal bool MultiReferenceType
		{
			get { return multiReferenceType; }
			set { multiReferenceType = value; }
		}

		internal XmlTypeMapping GetRealTypeMap (string objectFullTypeName)
		{
			// Returns the map for a subtype of this map's type

			if (TypeFullName == objectFullTypeName) return this;
			foreach (XmlTypeMapping map in _derivedTypes)
				if (map.TypeFullName == objectFullTypeName) return map;

			return null;
		}

		internal XmlTypeMapping GetRealElementMap (string name, string ens)
		{
			if (xmlType == name && ns == ens) return this;
			foreach (XmlTypeMapping map in _derivedTypes)
				if (map.xmlType == name && map.ns == ens) return map;
			return null;
		}
	}

	// Mapping info for classes and structs

	internal class ClassMap: ObjectMap
	{
		Hashtable _elements;
		ArrayList _elementMembers;
		Hashtable _attributeMembers;
		ArrayList _flatLists;
		XmlTypeMapMemberAnyElement _defaultAnyElement;
		XmlTypeMapMemberAnyAttribute _defaultAnyAttribute;
		XmlTypeMapMemberNamespaces _namespaceDeclarations;
		XmlTypeMapMember _xmlTextCollector;

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
				RegisterFlatList ((XmlTypeMapMemberFlatList)member);
			}
			else if (member is XmlTypeMapMemberAnyElement)
			{
				XmlTypeMapMemberAnyElement mem = (XmlTypeMapMemberAnyElement) member;
				if (mem.IsDefaultAny) _defaultAnyElement = mem;
				if (mem.TypeData.IsListType) RegisterFlatList (mem);
			}
			else if (member is XmlTypeMapMemberAnyAttribute)
			{
				_defaultAnyAttribute = (XmlTypeMapMemberAnyAttribute) member;
				return;
			}
			else if (member is XmlTypeMapMemberNamespaces)
			{
				_namespaceDeclarations = (XmlTypeMapMemberNamespaces) member;
				return;
			}

			if (member is XmlTypeMapMemberElement && ((XmlTypeMapMemberElement)member).IsXmlTextCollector)
			{
				if (_xmlTextCollector != null) throw new InvalidOperationException ("XmlTextAttribute can only be applied once in a class");
				_xmlTextCollector = member;
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

		void RegisterFlatList (XmlTypeMapMemberExpandable member)
		{
			if (_flatLists == null) _flatLists = new ArrayList ();
			member.FlatArrayIndex = _flatLists.Count;
			_flatLists.Add (member);
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

		public XmlTypeMapMemberAnyElement DefaultAnyElementMember
		{
			get { return _defaultAnyElement; }
		}

		public XmlTypeMapMemberAnyAttribute DefaultAnyAttributeMember
		{
			get { return _defaultAnyAttribute; }
		}

		public XmlTypeMapMemberNamespaces NamespaceDeclarations
		{
			get { return _namespaceDeclarations; }
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

		public XmlTypeMapMember XmlTextCollector
		{
			get { return _xmlTextCollector; }
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

		public void GetArrayType (int itemCount, out string localName, out string ns)
		{
			string arrayDim;
			if (itemCount != -1) arrayDim = "[" + itemCount + "]";
			else arrayDim = "[]";

			XmlTypeMapElementInfo info = (XmlTypeMapElementInfo) _itemInfo[0];
			if (info.TypeData.SchemaType == SchemaTypes.Array)
			{
				string nm;
				((ListMap)info.MappedType.ObjectMap).GetArrayType (-1, out nm, out ns);
				localName = nm + arrayDim;
			}
			else 
			{
				if (info.MappedType != null)
				{
					localName = info.MappedType.XmlType + arrayDim;
					ns = info.MappedType.Namespace;
				}
				else 
				{
					localName = info.TypeData.XmlType + arrayDim;
					ns = info.DataTypeNamespace;
				}
			}
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

	internal class EnumMap: ObjectMap
	{
		EnumMapMember[] _members;
		bool _isFlags;

		public class EnumMapMember
		{
			string _xmlName;
			string _enumName;

			public EnumMapMember (string xmlName, string enumName)
			{
				_xmlName = xmlName;
				_enumName = enumName;
			}

			public string XmlName
			{
				get { return _xmlName; }
			}

			public string EnumName
			{
				get { return _enumName; }
			}
		}

		public EnumMap (EnumMapMember[] members, bool isFlags)
		{
			_members = members;
			_isFlags = isFlags;
		}

		public EnumMapMember[] Members
		{
			get { return _members; }
		}

		public string GetXmlName (object enumValue)
		{
			string enumName = enumValue.ToString();

			if (_isFlags && enumName.IndexOf (',') != -1)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				string[] enumNames = enumValue.ToString().Split (',');
				foreach (string name in enumNames)
				{
					foreach (EnumMapMember mem in _members)
						if (mem.EnumName == name.Trim()) {
							sb.Append (mem.XmlName).Append (' ');
							break;
						}
				}
				sb.Remove (sb.Length-1, 1);
				return sb.ToString ();
			}

			foreach (EnumMapMember mem in _members)
				if (mem.EnumName == enumName) return mem.XmlName;
			
			return Convert.ToInt64(enumValue).ToString();
		}

		public string GetEnumName (string xmlName)
		{
			if (_isFlags && xmlName.Trim().IndexOf (' ') != -1)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				string[] enumNames = xmlName.ToString().Split (' ');
				foreach (string name in enumNames)
				{
					if (name == string.Empty) continue;
					string foundEnumValue = null;
					foreach (EnumMapMember mem in _members)
						if (mem.XmlName == name) { foundEnumValue = mem.EnumName; break; }

					if (foundEnumValue != null) sb.Append (foundEnumValue).Append (','); 
					else throw new InvalidOperationException ("Invalid enum value '" + name + "'");
				}
				sb.Remove (sb.Length-1, 1);
				return sb.ToString ();
			}

			foreach (EnumMapMember mem in _members)
				if (mem.XmlName == xmlName) return mem.EnumName;
				
			try {
				Int64.Parse (xmlName);
				return xmlName;
			}
			catch {
				throw new InvalidOperationException ("Invalid enumeration value: " + xmlName);
			}
		}
	}
}
