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
using System.Xml.Schema;
using System.Reflection;

namespace System.Xml.Serialization
{
	public class XmlTypeMapping : XmlMapping
	{
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
		: base (elementName, ns)
		{
			this.type = typeData;
			this.xmlType = xmlType;
			this.xmlTypeNamespace = xmlTypeNamespace;
		}

#if !NET_2_0
		public string ElementName
		{
			get { return _elementName; }
		}

		public string Namespace
		{
			get { return _namespace; }
		}
#endif

		public string TypeFullName
		{
			get { return type.FullTypeName; }
		}

		public string TypeName
		{
			get { return type.TypeName; }
		}

#if NET_2_0
		public string XsdTypeName
		{
			get { return XmlType; }
		}

		public string XsdTypeNamespace
		{
			get { return XmlTypeNamespace; }
		}
#endif

		internal TypeData TypeData
		{
			get { return type; }
		}

		internal string XmlType
		{
			get { return xmlType; }
			set { xmlType = value; }
		}

		internal string XmlTypeNamespace
		{
			get { return xmlTypeNamespace; }
			set { xmlTypeNamespace = value; }
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

		internal XmlTypeMapping GetRealTypeMap (Type objectType)
		{
			if (TypeData.SchemaType == SchemaTypes.Enum)
				return this;

			// Returns the map for a subtype of this map's type
			if (TypeData.Type == objectType) return this;
			for (int n=0; n<_derivedTypes.Count; n++) {
				XmlTypeMapping map = (XmlTypeMapping) _derivedTypes[n];
				if (map.TypeData.Type == objectType) return map;
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
				this._elementName = qname.Name;
				this._namespace = qname.Namespace;
			}
		}
	}

 
	// Mapping info for XmlSerializable
	internal class XmlSerializableMapping : XmlTypeMapping
	{
		XmlSchema _schema;
#if NET_2_0
		XmlSchemaComplexType _schemaType;
		XmlQualifiedName _schemaTypeName;
#endif

		internal XmlSerializableMapping(XmlRootAttribute root, string elementName, string ns, TypeData typeData, string xmlType, string xmlTypeNamespace)
			: base(elementName, ns, typeData, xmlType, xmlTypeNamespace)
		{
#if NET_2_0
			XmlSchemaProviderAttribute schemaProvider = (XmlSchemaProviderAttribute) Attribute.GetCustomAttribute (typeData.Type, typeof (XmlSchemaProviderAttribute));

			if (schemaProvider != null) {
				string method = schemaProvider.MethodName;
				MethodInfo mi = typeData.Type.GetMethod (method, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				if (mi == null)
					throw new InvalidOperationException (String.Format ("Type '{0}' must implement public static method '{1}'", typeData.Type, method));
				if (!typeof (XmlQualifiedName).IsAssignableFrom (mi.ReturnType) &&
				    // LAMESPEC: it is undocumented. (We don't have to tell users about it in the error message.)
				    // Also do not add such a silly compatibility test to assert that it does not raise an error.
				    !typeof (XmlSchemaComplexType).IsAssignableFrom (mi.ReturnType))
					throw new InvalidOperationException (String.Format ("Method '{0}' indicated by XmlSchemaProviderAttribute must have its return type as XmlQualifiedName", method));
				XmlSchemaSet xs = new XmlSchemaSet ();
				object retVal = mi.Invoke (null, new object [] { xs });
				_schemaTypeName = XmlQualifiedName.Empty;
				if (retVal == null)
					return;

				if (retVal is XmlSchemaComplexType) {
					_schemaType = (XmlSchemaComplexType) retVal;
					if (!_schemaType.QualifiedName.IsEmpty)
						_schemaTypeName = _schemaType.QualifiedName;
					else
						_schemaTypeName = new XmlQualifiedName (xmlType, xmlTypeNamespace);
				}
				else if (retVal is XmlQualifiedName) {
					_schemaTypeName = (XmlQualifiedName)retVal;
				}
				else
					throw new InvalidOperationException (
						String.Format ("Method {0}.{1}() specified by XmlSchemaProviderAttribute has invalid signature: return type must be compatible with System.Xml.XmlQualifiedName.", typeData.Type.Name, method));

				// defaultNamespace at XmlReflectionImporter takes precedence for Namespace, but not for XsdTypeNamespace.
				UpdateRoot (new XmlQualifiedName (root != null ? root.ElementName : _schemaTypeName.Name, root != null ? root.Namespace : Namespace ?? _schemaTypeName.Namespace));
				XmlTypeNamespace = _schemaTypeName.Namespace;
				XmlType = _schemaTypeName.Name;

				if (!_schemaTypeName.IsEmpty && xs.Count > 0) {
					XmlSchema [] schemas = new XmlSchema [xs.Count];
					xs.CopyTo (schemas, 0);
					_schema = schemas [0];
				}

				return;
			}
#endif
			IXmlSerializable serializable = (IXmlSerializable)Activator.CreateInstance (typeData.Type, true);
#if NET_2_0
			try {
				_schema = serializable.GetSchema();
			} catch (Exception) {
				// LAMESPEC: .NET has a bad exception catch and swallows it silently.
			}
#else
			_schema = serializable.GetSchema();
#endif
			if (_schema != null) 
			{
				if (_schema.Id == null || _schema.Id.Length == 0) 
					throw new InvalidOperationException("Schema Id is missing. The schema returned from " + typeData.Type.FullName + ".GetSchema() must have an Id.");
			}
		}

		internal XmlSchema Schema
		{
			get { return _schema; }
		}

#if NET_2_0
		internal XmlSchemaType SchemaType {
			get { return _schemaType; }
		}

		internal XmlQualifiedName SchemaTypeName {
			get { return _schemaTypeName; }
		}
#endif
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
		ArrayList _listMembers;
		XmlTypeMapMemberAnyElement _defaultAnyElement;
		XmlTypeMapMemberAnyAttribute _defaultAnyAttribute;
		XmlTypeMapMemberNamespaces _namespaceDeclarations;
		XmlTypeMapMember _xmlTextCollector;
		XmlTypeMapMember _returnMember;
		bool _ignoreMemberNamespace;
		bool _canBeSimpleType = true;

		public void AddMember (XmlTypeMapMember member)
		{
			member.GlobalIndex = _allMembers.Count;
			_allMembers.Add (member);
			
			if (!(member.DefaultValue is System.DBNull) && member.DefaultValue != null) {
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
					throw new InvalidOperationException ("The XML attribute named '" + atm.AttributeName + "' from namespace '" + atm.Namespace + "' is already present in the current scope. Use XML attributes to specify another XML name or namespace for the attribute.");
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
					throw new InvalidOperationException ("The XML element named '" + elem.ElementName + "' from namespace '" + elem.Namespace + "' is already present in the current scope. Use XML attributes to specify another XML name or namespace for the element.");
				_elements.Add (key, elem);
			}
			
			if (member.TypeData.IsListType && member.TypeData.Type != null && !member.TypeData.Type.IsArray) {
				if (_listMembers == null) _listMembers = new ArrayList ();
				_listMembers.Add (member);
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
		
		public ArrayList ListMembers
		{
			get { return _listMembers; }
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
		string _choiceMember;

		public bool IsMultiArray
		{
			get
			{
				return (NestedArrayMapping != null);
			}
		}

		public string ChoiceMember
		{
			get { return _choiceMember; }
			set { _choiceMember = value; }
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

		public XmlTypeMapElementInfo FindElement (object ob, int index, object memberValue)
		{
			if (_itemInfo.Count == 1) 
				return (XmlTypeMapElementInfo) _itemInfo[0];
			else if (_choiceMember != null && index != -1)
			{
				Array values = (Array) XmlTypeMapMember.GetValue (ob, _choiceMember);
				if (values == null || index >= values.Length)
					throw new InvalidOperationException ("Invalid or missing choice enum value in member '" + _choiceMember + "'.");
				object val = values.GetValue (index);
				foreach (XmlTypeMapElementInfo elem in _itemInfo)
					if (elem.ChoiceValue != null && elem.ChoiceValue.Equals (val))
						return elem;
			}
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
		readonly EnumMapMember[] _members;
		readonly bool _isFlags;
		readonly string[] _enumNames = null;
		readonly string[] _xmlNames = null;
		readonly long[] _values = null;

		public class EnumMapMember
		{
			readonly string _xmlName;
			readonly string _enumName;
			readonly long _value;
			string _documentation;

 			public EnumMapMember (string xmlName, string enumName)
				: this (xmlName, enumName, 0)
 			{
			}

			public EnumMapMember (string xmlName, string enumName, long value)
			{
				_xmlName = xmlName;
				_enumName = enumName;
				_value = value;
			}

			public string XmlName
			{
				get { return _xmlName; }
			}

			public string EnumName
			{
				get { return _enumName; }
			}

			public long Value
			{
				get { return _value; }
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

			_enumNames = new string[_members.Length];
			_xmlNames = new string[_members.Length];
			_values = new long[_members.Length];

			for (int i = 0; i < _members.Length; i++) {
				EnumMapMember mem = _members[i];
				_enumNames[i] = mem.EnumName;
				_xmlNames[i] = mem.XmlName;
				_values[i] = mem.Value;
			}
		}
		
		public bool IsFlags
		{
			get { return _isFlags; }
		}

		public EnumMapMember[] Members
		{
			get { return _members; }
		}

		public string[] EnumNames
		{
			get {
				return _enumNames;
			}
		}

		public string[] XmlNames
		{
			get {
				return _xmlNames;
			}
		}

		public long[] Values
		{
			get {
				return _values;
			}
		}

		public string GetXmlName (string typeName, object enumValue)
		{
			if (enumValue is string) {
				throw new InvalidCastException ();
			}

			long value = 0;

			try {
				value = ((IConvertible) enumValue).ToInt64 (CultureInfo.CurrentCulture);
			} catch (FormatException) {
				throw new InvalidCastException ();
			}

			for (int i = 0; i < Values.Length; i++) {
				if (Values[i] == value)
					return XmlNames[i];
			}

			if (IsFlags && value == 0)
				return string.Empty;

			string xmlName = string.Empty;
			if (IsFlags) {
#if NET_2_0
				xmlName = XmlCustomFormatter.FromEnum (value, XmlNames, Values, typeName);
#else
				xmlName = XmlCustomFormatter.FromEnum (value, XmlNames, Values);
#endif
			}

			if (xmlName.Length == 0) {
#if NET_2_0
				throw new InvalidOperationException (string.Format(CultureInfo.CurrentCulture,
					"'{0}' is not a valid value for {1}.", value, typeName));
#else
				return value.ToString (CultureInfo.InvariantCulture);
#endif
			}
			return xmlName;
		}

		public string GetEnumName (string typeName, string xmlName)
		{
			if (_isFlags) {
				xmlName = xmlName.Trim ();
				if (xmlName.Length == 0)
					return "0";

				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				string[] enumNames = xmlName.Split (null);
				foreach (string name in enumNames) {
					if (name == string.Empty) continue;
					string foundEnumValue = null;
					for (int i = 0; i < XmlNames.Length; i++)
						if (XmlNames[i] == name) {
							foundEnumValue = EnumNames[i];
							break;
						}

					if (foundEnumValue != null) {
						if (sb.Length > 0)
							sb.Append (',');
						sb.Append (foundEnumValue);
					} else {
						throw new InvalidOperationException (string.Format (CultureInfo.CurrentCulture,
							"'{0}' is not a valid value for {1}.", name, typeName));
					}
				}
				return sb.ToString ();
			}

			foreach (EnumMapMember mem in _members)
				if (mem.XmlName == xmlName) return mem.EnumName;
				
			return null;
		}
	}
}
