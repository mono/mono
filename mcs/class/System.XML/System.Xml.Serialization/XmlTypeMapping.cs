//
// XmlTypeMapping.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002 John Donagher
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

using System.Xml;
using System;
using System.Collections;
using System.Globalization;

namespace System.Xml.Serialization
{
	public class XmlTypeMapping : XmlMapping
	{
		private string elementName;
		private string ns;
		private string xmlType;
		private string xmlTypeNamespace;
		TypeData type;
		XmlTypeMapping baseMap;
		bool multiReferenceType = false;
		bool isSimpleType;
		string documentation;
		bool includeInSchema;
		bool isNullable = true;

		ArrayList _derivedTypes = new ArrayList();

		internal XmlTypeMapping(string elementName, string ns, TypeData typeData, string xmlType, string xmlTypeNamespace)
		{
			this.elementName = elementName;
			this.ns = ns;
			this.type = typeData;
			this.xmlType = xmlType;
			this.xmlTypeNamespace = xmlTypeNamespace;
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

		internal string XmlTypeNamespace
		{
			get { return xmlTypeNamespace; }
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

		internal XmlTypeMapping BaseMap
		{
			get { return baseMap; }
			set { baseMap = value; }
		}

		internal bool IsSimpleType
		{
			get { return isSimpleType; }
			set { isSimpleType = value; }
		}

		internal string Documentation
		{
			set { documentation = value; }
			get { return documentation; }
		}

		internal bool IncludeInSchema
		{
			get { return includeInSchema; }
			set { includeInSchema = value; }
		}
		
		internal bool IsNullable
		{
			get { return isNullable; }
			set { isNullable = value; }
		}

		internal XmlTypeMapping GetRealTypeMap (string objectFullTypeName)
		{
			// Returns the map for a subtype of this map's type

			objectFullTypeName = objectFullTypeName.Replace ('+','.');
			if (TypeFullName == objectFullTypeName) return this;
			for (int n=0; n<_derivedTypes.Count; n++) {
				XmlTypeMapping map = (XmlTypeMapping) _derivedTypes[n];
				if (map.TypeFullName == objectFullTypeName) return map;
			}
			
			return null;
		}

		internal XmlTypeMapping GetRealElementMap (string name, string ens)
		{
			if (xmlType == name && xmlTypeNamespace == ens) return this;
			foreach (XmlTypeMapping map in _derivedTypes)
				if (map.xmlType == name && map.xmlTypeNamespace == ens) return map;
			
			return null;
		}
		
		internal void UpdateRoot (XmlQualifiedName qname)
		{
			if (qname != null) {
				this.elementName = qname.Name;
				this.ns = qname.Namespace;
			}
		}
	}

	// Mapping info for classes and structs

	internal class ClassMap: ObjectMap
	{
		Hashtable _elements = new Hashtable ();
		ArrayList _elementMembers;
		Hashtable _attributeMembers;
		XmlTypeMapMemberAttribute[] _attributeMembersArray;
		XmlTypeMapElementInfo[] _elementsByIndex;
		ArrayList _flatLists;
		ArrayList _allMembers = new ArrayList ();
		ArrayList _membersWithDefault;
		XmlTypeMapMemberAnyElement _defaultAnyElement;
		XmlTypeMapMemberAnyAttribute _defaultAnyAttribute;
		XmlTypeMapMemberNamespaces _namespaceDeclarations;
		XmlTypeMapMember _xmlTextCollector;
		XmlTypeMapMember _returnMember;
		bool _ignoreMemberNamespace;
		bool _canBeSimpleType = true;

		public void AddMember (XmlTypeMapMember member)
		{
			_allMembers.Add (member);
			
			if (!(member.DefaultValue is System.DBNull)) {
				if (_membersWithDefault == null) _membersWithDefault = new ArrayList ();
				_membersWithDefault.Add (member);
			}
			
			if (member.IsReturnValue)
				_returnMember = member;
			
			if (member is XmlTypeMapMemberAttribute)
			{
				XmlTypeMapMemberAttribute atm = (XmlTypeMapMemberAttribute)member;
				if (_attributeMembers == null) _attributeMembers = new Hashtable();
				string key = BuildKey (atm.AttributeName, atm.Namespace);
				if (_attributeMembers.ContainsKey (key))
					throw new InvalidOperationException ("The XML attribute named '" + atm.AttributeName + "' from namespace '" + atm.Namespace + "' already present in the current scope. Use XML attributes to specify another XML name or namespace for the attribute.");
				member.Index = _attributeMembers.Count;
				_attributeMembers.Add (key, member);
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
				string key = BuildKey (elem.ElementName, elem.Namespace);
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
			return (XmlTypeMapMemberAttribute)_attributeMembers [BuildKey(name,ns)];
		}

		public XmlTypeMapElementInfo GetElement (string name, string ns)
		{
			if (_elements == null) return null;
			return (XmlTypeMapElementInfo)_elements [BuildKey(name,ns)];
		}
		
		public XmlTypeMapElementInfo GetElement (int index)
		{
			if (_elements == null) return null;
			
			if (_elementsByIndex == null)
			{
				_elementsByIndex = new XmlTypeMapElementInfo [_elementMembers.Count];
				foreach (XmlTypeMapMemberElement mem in _elementMembers)
				{
					if (mem.ElementInfo.Count != 1) 
						throw new InvalidOperationException ("Read by order only possible for encoded/bare format");
						
					_elementsByIndex [mem.Index] = (XmlTypeMapElementInfo) mem.ElementInfo [0];
				}
			}
			
			return _elementsByIndex [index];
		}
		
		private string BuildKey (string name, string ns)
		{
			if (_ignoreMemberNamespace) return name;
			else return name + " / " + ns;
		}
		
		public ICollection AllElementInfos
		{
			get { return _elements.Values; }
		}
		
		
		public bool IgnoreMemberNamespace
		{
			get { return _ignoreMemberNamespace; }
			set { _ignoreMemberNamespace = value; }
		}

		public XmlTypeMapMember FindMember (string name)
		{
			for (int n=0; n<_allMembers.Count; n++)
				if (((XmlTypeMapMember)_allMembers[n]).Name == name) return (XmlTypeMapMember)_allMembers[n];
			return null;
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
			get 
			{
				if (_attributeMembers == null) return null;
				if (_attributeMembersArray != null) return _attributeMembersArray;
				
				_attributeMembersArray = new XmlTypeMapMemberAttribute[_attributeMembers.Count];
				foreach (XmlTypeMapMemberAttribute mem in _attributeMembers.Values)
					_attributeMembersArray [mem.Index] = mem;
				return _attributeMembersArray;
			}
		}

		public ICollection ElementMembers
		{
			get { return _elementMembers; }
		}

		public ArrayList AllMembers
		{
			get { return _allMembers; }
		}

		public ArrayList FlatLists
		{
			get { return _flatLists; }
		}
		
		public ArrayList MembersWithDefault
		{
			get { return _membersWithDefault; }
		}

		public XmlTypeMapMember XmlTextCollector
		{
			get { return _xmlTextCollector; }
		}
		
		public XmlTypeMapMember ReturnMember
		{
			get { return _returnMember; }
		}

		public XmlQualifiedName SimpleContentBaseType
		{
			get
			{
				if (!_canBeSimpleType || _elementMembers == null || _elementMembers.Count != 1) return null;
				XmlTypeMapMemberElement member = (XmlTypeMapMemberElement) _elementMembers[0];
				if (member.ElementInfo.Count != 1) return null;
				XmlTypeMapElementInfo einfo = (XmlTypeMapElementInfo) member.ElementInfo[0];
				if (!einfo.IsTextElement) return null;
				if (member.TypeData.SchemaType == SchemaTypes.Primitive || member.TypeData.SchemaType == SchemaTypes.Enum)
					return new XmlQualifiedName (einfo.TypeData.XmlType, einfo.DataTypeNamespace);
				return null;
			}
		}
		
		public void SetCanBeSimpleType (bool can)
		{
			_canBeSimpleType = can;
		}

		public bool HasSimpleContent
		{
			get
			{
				return SimpleContentBaseType != null;
			}
		}

	}

	// Mapping info for arrays and lists

	internal class ListMap: ObjectMap
	{
		XmlTypeMapElementInfoList _itemInfo;
		bool _gotNestedMapping;
		XmlTypeMapping _nestedArrayMapping;

		public bool IsMultiArray
		{
			get
			{
				return (NestedArrayMapping != null);
			}
		}

		public XmlTypeMapping NestedArrayMapping
		{
			get
			{
				if (_gotNestedMapping) return _nestedArrayMapping;
				_gotNestedMapping = true;

				_nestedArrayMapping = ((XmlTypeMapElementInfo)_itemInfo[0]).MappedType;

				if (_nestedArrayMapping == null) return null;
				
				if (_nestedArrayMapping.TypeData.SchemaType != SchemaTypes.Array) {
					_nestedArrayMapping = null; return null;
				}

				foreach (XmlTypeMapElementInfo elem in _itemInfo)
					if (elem.MappedType != _nestedArrayMapping) {
						_nestedArrayMapping = null;
						return null;
					}

				return _nestedArrayMapping;
			}
		}

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
		
		public XmlTypeMapElementInfo FindTextElement ()
		{
			foreach (XmlTypeMapElementInfo elem in _itemInfo)
				if (elem.IsTextElement) return elem;
			return null;
		}
		
		public string GetSchemaArrayName ()
		{
			XmlTypeMapElementInfo einfo = (XmlTypeMapElementInfo) _itemInfo[0];
			if (einfo.MappedType != null) return TypeTranslator.GetArrayName (einfo.MappedType.XmlType);
			else return TypeTranslator.GetArrayName (einfo.TypeData.XmlType);
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
			string _documentation;

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

			public string Documentation
			{
				get { return _documentation; }
				set { _documentation = value; }
			}
		}

		public EnumMap (EnumMapMember[] members, bool isFlags)
		{
			_members = members;
			_isFlags = isFlags;
		}
		
		public bool IsFlags
		{
			get { return _isFlags; }
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
					string tname = name.Trim();
					foreach (EnumMapMember mem in _members)
						if (mem.EnumName == tname) {
							sb.Append (mem.XmlName).Append (' ');
							break;
						}
				}
				sb.Remove (sb.Length-1, 1);
				return sb.ToString ();
			}

			foreach (EnumMapMember mem in _members)
				if (mem.EnumName == enumName) return mem.XmlName;
			
			return Convert.ToInt64(enumValue).ToString(CultureInfo.InvariantCulture);
		}

		public string GetEnumName (string xmlName)
		{
			if (_isFlags && xmlName.Length == 0) 
				return "0";
			
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
					else return null;
				}
				sb.Remove (sb.Length-1, 1);
				return sb.ToString ();
			}

			foreach (EnumMapMember mem in _members)
				if (mem.XmlName == xmlName) return mem.EnumName;
				
			return null;
		}
	}
}
