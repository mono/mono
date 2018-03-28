//
// XmlSerializationReaderInterpreter.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
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
using System.Reflection;
using System.Collections;

namespace System.Xml.Serialization
{
	internal class XmlSerializationReaderInterpreter: XmlSerializationReader
	{
		XmlMapping _typeMap;
		SerializationFormat _format;
		static readonly XmlQualifiedName AnyType = new XmlQualifiedName("anyType", System.Xml.Schema.XmlSchema.Namespace);
		static readonly object [] empty_array = new object [0];

		public XmlSerializationReaderInterpreter(XmlMapping typeMap)
		{
			_typeMap = typeMap;
			_format = typeMap.Format;
		}

		protected override void InitCallbacks ()
		{
			ArrayList maps = _typeMap.RelatedMaps;
			if (maps != null)
			{
				foreach (XmlTypeMapping map in maps)  
				{
					if (map.TypeData.SchemaType == SchemaTypes.Class || map.TypeData.SchemaType == SchemaTypes.Enum)
					{
						ReaderCallbackInfo info = new ReaderCallbackInfo (this, map);
						AddReadCallback (map.XmlType, map.Namespace, map.TypeData.Type, new XmlSerializationReadCallback (info.ReadObject));
					}
				}
			}
		}

		protected override void InitIDs ()
		{
		}

		protected XmlTypeMapping GetTypeMap (Type type)
		{
			ArrayList maps = _typeMap.RelatedMaps;
			if (maps != null)
			{
				foreach (XmlTypeMapping map in maps)
					if (map.TypeData.Type == type) return map;
			}
			throw new InvalidOperationException ("Type " + type + " not mapped");
		}

		public object ReadRoot ()
		{
			Reader.MoveToContent();
			if (_typeMap is XmlTypeMapping)
			{
				if (_format == SerializationFormat.Literal)
					return ReadRoot ((XmlTypeMapping)_typeMap);
				else
					return ReadEncodedObject ((XmlTypeMapping)_typeMap);
			}
			else
				return ReadMessage ((XmlMembersMapping)_typeMap);
		}

		object ReadEncodedObject (XmlTypeMapping typeMap)
		{
			object ob = null;
			Reader.MoveToContent();
			if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
			{
				if (Reader.LocalName == typeMap.ElementName && Reader.NamespaceURI == typeMap.Namespace)
					ob = ReadReferencedElement();
				else 
					throw CreateUnknownNodeException();
			}
			else 
				UnknownNode(null);

			ReadReferencedElements();
			return ob;
		}

		protected virtual object ReadMessage (XmlMembersMapping typeMap)
		{
			object[] parameters = new object[typeMap.Count];

			if (typeMap.HasWrapperElement)
			{
				// bug #79988: out parameters need to be initialized if they 
				// are value types
				ArrayList members = ((ClassMap) typeMap.ObjectMap).AllMembers;
				for (int n = 0; n < members.Count; n++) {
					XmlTypeMapMember mem = (XmlTypeMapMember) members [n];
					if (!mem.IsReturnValue && mem.TypeData.IsValueType)
						SetMemberValueFromAttr (mem, parameters, CreateInstance (
							mem.TypeData.Type), true);
				}

				if (_format == SerializationFormat.Encoded)
				{
					while (Reader.NodeType == System.Xml.XmlNodeType.Element)
					{
						string root = Reader.GetAttribute ("root", XmlSerializer.EncodingNamespace);
						if (root == null || System.Xml.XmlConvert.ToBoolean(root)) break;
						ReadReferencedElement ();
						Reader.MoveToContent ();
					}
				}

				while (Reader.NodeType != System.Xml.XmlNodeType.EndElement &&
				       // it could be an empty root element
				       Reader.ReadState == ReadState.Interactive)
				{
					if (Reader.IsStartElement(typeMap.ElementName, typeMap.Namespace) 
					    || _format == SerializationFormat.Encoded)  
					{
						ReadAttributeMembers ((ClassMap)typeMap.ObjectMap, parameters, true);
						if (Reader.IsEmptyElement) {
							Reader.Skip();
							Reader.MoveToContent();
							continue;
						}
						Reader.ReadStartElement();
						ReadMembers ((ClassMap)typeMap.ObjectMap, parameters, true, false);
						ReadEndElement();
						break;
					}
					else 
						UnknownNode(null);

					Reader.MoveToContent();
				}
			}
			else
				ReadMembers ((ClassMap)typeMap.ObjectMap, parameters, true, _format == SerializationFormat.Encoded);

			if (_format == SerializationFormat.Encoded)
				ReadReferencedElements();

			return parameters;
		}

		object ReadRoot (XmlTypeMapping rootMap)
		{
			if (rootMap.TypeData.SchemaType == SchemaTypes.XmlNode)
			{
				return ReadXmlNodeElement (rootMap, true);
			}
			else
			{
				if (!rootMap.IsAny && (Reader.LocalName != rootMap.ElementName || Reader.NamespaceURI != rootMap.Namespace))
					throw CreateUnknownNodeException();
				
				return ReadObject (rootMap, rootMap.IsNullable, true);
			}
		}		


		protected virtual object ReadObject (XmlTypeMapping typeMap, bool isNullable, bool checkType)
		{
			switch (typeMap.TypeData.SchemaType)
			{
				case SchemaTypes.Class: return ReadClassInstance (typeMap, isNullable, checkType);
				case SchemaTypes.Array: return ReadListElement (typeMap, isNullable, null, true);
				case SchemaTypes.XmlNode: return ReadXmlNodeElement (typeMap, isNullable);
				case SchemaTypes.Primitive: return ReadPrimitiveElement (typeMap, isNullable);
				case SchemaTypes.Enum: return ReadEnumElement (typeMap, isNullable);
				case SchemaTypes.XmlSerializable: return ReadXmlSerializableElement (typeMap, isNullable);
				default: throw new Exception ("Unsupported map type");
			}
		}

		protected virtual object ReadClassInstance (XmlTypeMapping typeMap, bool isNullable, bool checkType)
		{
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
                System.Xml.XmlQualifiedName t = GetXsiType();
				if (t != null) 
				{
					XmlTypeMapping realMap = typeMap.GetRealElementMap (t.Name, t.Namespace);
					if (realMap == null) {
						if (typeMap.TypeData.Type == typeof(object))
							return ReadTypedPrimitive (t);
						else
							throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)t);
					}
					if (realMap != typeMap)
						return ReadObject (realMap, false, false);
				}
				else if (typeMap.TypeData.Type == typeof(object))
					return ReadTypedPrimitive (AnyType);
            }

			object ob = CreateInstance (typeMap.TypeData.Type, true);

			Reader.MoveToElement();
			bool isEmpty = Reader.IsEmptyElement;
			ReadClassInstanceMembers (typeMap, ob);

			if (isEmpty) Reader.Skip();
			else ReadEndElement();

			return ob;
		}

		protected virtual void ReadClassInstanceMembers (XmlTypeMapping typeMap, object ob)
		{
			ReadMembers ((ClassMap) typeMap.ObjectMap, ob, false, false);
		}

		void ReadAttributeMembers (ClassMap map, object ob, bool isValueList)
		{
			XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
			int anyAttributeIndex = 0;
			object anyAttributeArray = null;

			while (Reader.MoveToNextAttribute())
			{
				XmlTypeMapMemberAttribute member = map.GetAttribute (Reader.LocalName, Reader.NamespaceURI);

				if (member != null) 
				{
					SetMemberValue (member, ob, GetValueFromXmlString (Reader.Value, member.TypeData, member.MappedType), isValueList);
				}
				else if (IsXmlnsAttribute(Reader.Name)) 
				{
					// If the map has NamespaceDeclarations,
					// then store this xmlns to the given member.
					// If the instance doesn't exist, then create.
					if (map.NamespaceDeclarations != null) {
						XmlSerializerNamespaces nss = this.GetMemberValue (map.NamespaceDeclarations, ob, isValueList) as XmlSerializerNamespaces;
						if (nss == null) {
							nss = new XmlSerializerNamespaces ();
							SetMemberValue (map.NamespaceDeclarations, ob, nss, isValueList);
						}
						if (Reader.Prefix == "xmlns")
							nss.Add (Reader.LocalName, Reader.Value);
						else
							nss.Add ("", Reader.Value);
					}
				}	
				else if (anyAttrMember != null) 
				{
					XmlAttribute attr = (XmlAttribute) Document.ReadNode(Reader);
					ParseWsdlArrayType (attr);
					AddListValue (anyAttrMember.TypeData, ref anyAttributeArray, anyAttributeIndex++, attr, true);
				}
				else
					ProcessUnknownAttribute(ob);
			}

			if (anyAttrMember != null)
			{
				anyAttributeArray = ShrinkArray ((Array)anyAttributeArray, anyAttributeIndex, anyAttrMember.TypeData.Type.GetElementType(), true);
				SetMemberValue (anyAttrMember, ob, anyAttributeArray, isValueList);
			}
			Reader.MoveToElement ();
		}

		void ReadMembers (ClassMap map, object ob, bool isValueList, bool readBySoapOrder)
		{
			// Reads attributes
			ReadAttributeMembers (map, ob, isValueList);

			if (!isValueList)
			{
				Reader.MoveToElement();
				if (Reader.IsEmptyElement) { 
					SetListMembersDefaults (map, ob, isValueList);
					return;
				}

				Reader.ReadStartElement();
			}

			// Reads elements

			bool[] readFlag = new bool[(map.ElementMembers != null) ? map.ElementMembers.Count : 0];

			bool hasAnyReturnMember = (isValueList && _format == SerializationFormat.Encoded && map.ReturnMember != null);
			
			Reader.MoveToContent();

			int[] indexes = null;
			object[] flatLists = null;
			object[] flatListsChoices = null;
			Fixup fixup = null;
			int ind = -1;
			int maxInd;

			if (readBySoapOrder) {
				if (map.ElementMembers != null) maxInd = map.ElementMembers.Count;
				else maxInd = -1;
			}
			else
				maxInd = int.MaxValue;

			if (map.FlatLists != null) 
			{
				indexes = new int[map.FlatLists.Count];
				flatLists = new object[map.FlatLists.Count];
				foreach (XmlTypeMapMemberExpandable mem in map.FlatLists) {
					if (IsReadOnly (mem, mem.TypeData, ob, isValueList))
						flatLists [mem.FlatArrayIndex] = mem.GetValue (ob);
					else if (mem.TypeData.Type.IsArray) {
						flatLists [mem.FlatArrayIndex] = InitializeList (mem.TypeData);
					}
					else {
						object list = mem.GetValue (ob);
						if (list == null) {
							list = InitializeList (mem.TypeData);
							SetMemberValue (mem, ob, list, isValueList);
						}
						flatLists [mem.FlatArrayIndex] = list;
					}
						
					if (mem.ChoiceMember != null) {
						if (flatListsChoices == null)
							flatListsChoices = new object [map.FlatLists.Count];
						flatListsChoices [mem.FlatArrayIndex] = InitializeList (mem.ChoiceTypeData);
					}
				}
			}
			
			if (_format == SerializationFormat.Encoded && map.ElementMembers != null)
			{
				FixupCallbackInfo info = new FixupCallbackInfo (this, map, isValueList);
				fixup = new Fixup(ob, new XmlSerializationFixupCallback(info.FixupMembers), map.ElementMembers.Count);
				AddFixup (fixup);
			}

			XmlTypeMapMember previousMember = null;
			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && (ind < maxInd - 1))
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					XmlTypeMapElementInfo info;
					
					if (readBySoapOrder) {
						info = map.GetElement (Reader.LocalName, Reader.NamespaceURI, ind);
					}
					else if (hasAnyReturnMember) {
						info = (XmlTypeMapElementInfo) ((XmlTypeMapMemberElement)map.ReturnMember).ElementInfo[0];
						hasAnyReturnMember = false;
					}
					else {
						if (map.IsOrderDependentMap) {
							info = map.GetElement (Reader.LocalName, Reader.NamespaceURI, ind);
						}
						else
							info = map.GetElement (Reader.LocalName, Reader.NamespaceURI);
					}

					if (info != null && !readFlag[info.Member.Index] )
					{
						if (info.Member != previousMember)
						{
							ind = info.ExplicitOrder + 1;
							// If the member is a flat list don't increase the index, since the next element may
							// be another item of the list. This is a fix for Xamarin bug #9193.
							if (info.Member is XmlTypeMapMemberFlatList)
								ind--;
							previousMember = info.Member;
						}

						if (info.Member.GetType() == typeof (XmlTypeMapMemberList))
						{
							if (_format == SerializationFormat.Encoded && info.MultiReferenceType)
							{
								object list = ReadReferencingElement (out fixup.Ids[info.Member.Index]);
								if (fixup.Ids[info.Member.Index] == null)	// Already read
								{
									if (IsReadOnly (info.Member, info.TypeData, ob, isValueList)) throw CreateReadOnlyCollectionException (info.TypeData.FullTypeName);
									else SetMemberValue (info.Member, ob, list, isValueList);
								}
								else if (!info.MappedType.TypeData.Type.IsArray)
								{
									if (IsReadOnly (info.Member, info.TypeData, ob, isValueList)) 
										list = GetMemberValue (info.Member, ob, isValueList);
									else { 
										list = CreateList (info.MappedType.TypeData.Type);
										SetMemberValue (info.Member, ob, list, isValueList);
									}
									AddFixup (new CollectionFixup (list, new XmlSerializationCollectionFixupCallback (FillList), fixup.Ids[info.Member.Index]));
									fixup.Ids[info.Member.Index] = null;	// The member already has the value, no further fix needed.
								}
							}
							else
							{
								if (IsReadOnly (info.Member, info.TypeData, ob, isValueList)) {
									ReadListElement (info.MappedType, info.IsNullable, GetMemberValue (info.Member, ob, isValueList), false);
								} else if (info.MappedType.TypeData.Type.IsArray) {
									object list = ReadListElement (info.MappedType, info.IsNullable, null, true);
									if (list != null || info.IsNullable)
										SetMemberValue (info.Member, ob, list, isValueList);
								} else {
									// If the member already has a list, reuse that list. No need to create a new one. 
									object list = GetMemberValue (info.Member, ob, isValueList);
									if (list == null) {
										list = CreateList (info.MappedType.TypeData.Type);
										SetMemberValue (info.Member, ob, list, isValueList);
									}
									ReadListElement (info.MappedType, info.IsNullable, list, true);
								}
							}
							readFlag[info.Member.Index] = true;
						}
						else if (info.Member.GetType() == typeof (XmlTypeMapMemberFlatList))
						{
							XmlTypeMapMemberFlatList mem = (XmlTypeMapMemberFlatList)info.Member;
							AddListValue (mem.TypeData, ref flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex]++, ReadObjectElement (info), !IsReadOnly (info.Member, info.TypeData, ob, isValueList));
							if (mem.ChoiceMember != null) {
								AddListValue (mem.ChoiceTypeData, ref flatListsChoices [mem.FlatArrayIndex], indexes[mem.FlatArrayIndex]-1, info.ChoiceValue, true);
							}
						}
						else if (info.Member.GetType() == typeof (XmlTypeMapMemberAnyElement))
						{
							XmlTypeMapMemberAnyElement mem = (XmlTypeMapMemberAnyElement)info.Member;
							if (mem.TypeData.IsListType) AddListValue (mem.TypeData, ref flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex]++, ReadXmlNode (mem.TypeData.ListItemTypeData, false), true);
							else SetMemberValue (mem, ob, ReadXmlNode (mem.TypeData, false), isValueList);
						}
						else if (info.Member.GetType() == typeof(XmlTypeMapMemberElement))
						{
							object val;
							readFlag[info.Member.Index] = true;
							if (_format == SerializationFormat.Encoded)
							{
								if (info.Member.TypeData.SchemaType != SchemaTypes.Primitive)
									val = ReadReferencingElement (out fixup.Ids[info.Member.Index]);
								else
									val = ReadReferencingElement (info.Member.TypeData.XmlType, System.Xml.Schema.XmlSchema.Namespace, out fixup.Ids[info.Member.Index]);
									
								if (info.MultiReferenceType) {
									if (fixup.Ids[info.Member.Index] == null)	// already read
										SetMemberValue (info.Member, ob, val, isValueList);
								}
								else if (val != null)
									SetMemberValue (info.Member, ob, val, isValueList);
							}
							else {
								SetMemberValue (info.Member, ob, ReadObjectElement (info), isValueList);
								if (info.ChoiceValue != null) {
									XmlTypeMapMemberElement imem = (XmlTypeMapMemberElement) info.Member;
									imem.SetChoice (ob, info.ChoiceValue);
								}
							}
						}
						else
							throw new InvalidOperationException ("Unknown member type");
					}
					else if (map.DefaultAnyElementMember != null)
					{
						XmlTypeMapMemberAnyElement mem = map.DefaultAnyElementMember;
						if (mem.TypeData.IsListType) AddListValue (mem.TypeData, ref flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex]++, ReadXmlNode (mem.TypeData.ListItemTypeData, false), true);
						else SetMemberValue (mem, ob, ReadXmlNode (mem.TypeData, false), isValueList);
					}
					else 
						ProcessUnknownElement(ob);
				}
				else if ((Reader.NodeType == System.Xml.XmlNodeType.Text || Reader.NodeType == System.Xml.XmlNodeType.CDATA) && map.XmlTextCollector != null)
				{
					if (map.XmlTextCollector is XmlTypeMapMemberExpandable)
					{
						XmlTypeMapMemberExpandable mem = (XmlTypeMapMemberExpandable)map.XmlTextCollector;
						XmlTypeMapMemberFlatList flatl = mem as XmlTypeMapMemberFlatList;
						TypeData itype = (flatl == null) ? mem.TypeData.ListItemTypeData : flatl.ListMap.FindTextElement().TypeData;

						object val = (itype.Type == typeof (string)) ? (object) Reader.ReadString() : (object) ReadXmlNode (itype, false);
						AddListValue (mem.TypeData, ref flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex]++, val, true);
					}
					else
					{
						XmlTypeMapMemberElement mem = (XmlTypeMapMemberElement) map.XmlTextCollector;
						XmlTypeMapElementInfo info = (XmlTypeMapElementInfo) mem.ElementInfo [0];
						if (info.TypeData.Type == typeof (string))
							SetMemberValue (mem, ob, Reader.ReadString (), isValueList);
						else
							SetMemberValue (mem, ob, GetValueFromXmlString (Reader.ReadString(), info.TypeData, info.MappedType), isValueList);
					}
				}
				else 
					UnknownNode(ob);
				Reader.MoveToContent();
			}

			if (flatLists != null)
			{
				foreach (XmlTypeMapMemberExpandable mem in map.FlatLists)
				{
					Object list = flatLists[mem.FlatArrayIndex];
					if (mem.TypeData.Type.IsArray)
						list = ShrinkArray ((Array)list, indexes[mem.FlatArrayIndex], mem.TypeData.Type.GetElementType(), true);
					if (!IsReadOnly (mem, mem.TypeData, ob, isValueList) && mem.TypeData.Type.IsArray)
						SetMemberValue (mem, ob, list, isValueList);
				}
			}

			if (flatListsChoices != null)
			{
				foreach (XmlTypeMapMemberExpandable mem in map.FlatLists)
				{
					Object list = flatListsChoices[mem.FlatArrayIndex];
					if (list == null) continue;
					list = ShrinkArray ((Array)list, indexes[mem.FlatArrayIndex], mem.ChoiceTypeData.Type.GetElementType(), true);
					XmlTypeMapMember.SetValue (ob, mem.ChoiceMember, list);
				}
			}
			SetListMembersDefaults (map, ob, isValueList);
		}
		
		void SetListMembersDefaults (ClassMap map, object ob, bool isValueList)
		{
			if (map.ListMembers != null)
			{
				ArrayList members = map.ListMembers;
				for (int n=0; n<members.Count; n++) {
					XmlTypeMapMember mem = (XmlTypeMapMember) members[n];
					if (IsReadOnly (mem, mem.TypeData, ob, isValueList))
						continue;
					if (GetMemberValue (mem, ob, isValueList) == null)
						SetMemberValue (mem, ob, InitializeList (mem.TypeData), isValueList);
				}
			}
		}

		internal void FixupMembers (ClassMap map, object obfixup, bool isValueList)
		{
			Fixup fixup = (Fixup)obfixup;
			ICollection members = map.ElementMembers;
			string[] ids = fixup.Ids;
			foreach (XmlTypeMapMember member in members)
			{
				if (ids[member.Index] != null)
					SetMemberValue (member, fixup.Source, GetTarget(ids[member.Index]), isValueList);
			}
		}
		
		protected virtual void ProcessUnknownAttribute (object target)
		{
			UnknownNode (target);
		}
		
		protected virtual void ProcessUnknownElement (object target)
		{
			UnknownNode (target);
		}

		bool IsReadOnly (XmlTypeMapMember member, TypeData memType, object ob, bool isValueList)
		{
			if (isValueList) return !memType.HasPublicConstructor;
			else return member.IsReadOnly (ob.GetType()) || !memType.HasPublicConstructor;
		}

		void SetMemberValue (XmlTypeMapMember member, object ob, object value, bool isValueList)
		{
			var memberType = member.TypeData.Type;
			if (value != null && !value.GetType().IsAssignableFrom (memberType))
				value = XmlSerializationWriterInterpreter.ImplicitConvert (value, memberType);

			if (isValueList)
				((object[])ob)[member.GlobalIndex] = value;
			else
				member.SetValue (ob, value);
			if (member.IsOptionalValueType)
				member.SetValueSpecified (ob, true); 
		}

		void SetMemberValueFromAttr (XmlTypeMapMember member, object ob, object value, bool isValueList)
		{
			// Enumeration values specified in custom attributes are stored as integer
			// values if the custom attribute property is of type object. So, it is
			// necessary to convert to the enum type before asigning the value to the field.
			
			if (member.TypeData.Type.IsEnum)
				value = Enum.ToObject (member.TypeData.Type, value);
			SetMemberValue (member, ob, value, isValueList);
		}

		object GetMemberValue (XmlTypeMapMember member, object ob, bool isValueList)
		{
			if (isValueList) return ((object[])ob)[member.GlobalIndex];
			else return member.GetValue (ob);
		}

		object ReadObjectElement (XmlTypeMapElementInfo elem)
		{
			switch (elem.TypeData.SchemaType)
			{
				case SchemaTypes.XmlNode:
					return ReadXmlNode (elem.TypeData, true);

				case SchemaTypes.Primitive:
				case SchemaTypes.Enum:
					return ReadPrimitiveValue (elem);

				case SchemaTypes.Array:
					return ReadListElement (elem.MappedType, elem.IsNullable, null, true);

				case SchemaTypes.Class:
					return ReadObject (elem.MappedType, elem.IsNullable, true);

				case SchemaTypes.XmlSerializable:
					object ob = CreateInstance (elem.TypeData.Type, true);
					return ReadSerializable ((IXmlSerializable)ob);

				default:
					throw new NotSupportedException ("Invalid value type");
			}
		}

		object ReadPrimitiveValue (XmlTypeMapElementInfo elem)
		{
			if (elem.TypeData.Type == typeof (XmlQualifiedName)) {
				if (elem.IsNullable) return ReadNullableQualifiedName ();
				else return ReadElementQualifiedName ();
			}
			else if (elem.IsNullable)
				return GetValueFromXmlString (ReadNullableString (), elem.TypeData, elem.MappedType);
			else
				return GetValueFromXmlString (Reader.ReadElementString (), elem.TypeData, elem.MappedType);
		}
		
		object GetValueFromXmlString (string value, TypeData typeData, XmlTypeMapping typeMap)
		{
			if (typeData.SchemaType == SchemaTypes.Array)
				return ReadListString (typeMap, value);
			else if (typeData.SchemaType == SchemaTypes.Enum)
				return GetEnumValue (typeMap, value);
			else if (typeData.Type == typeof (XmlQualifiedName))
				return ToXmlQualifiedName (value);
			else 
				return XmlCustomFormatter.FromXmlString (typeData, value);
		}

		object ReadListElement (XmlTypeMapping typeMap, bool isNullable, object list, bool canCreateInstance)
		{
			Type listType = typeMap.TypeData.Type;
			ListMap listMap = (ListMap)typeMap.ObjectMap;

			if (listType.IsArray && ReadNull()) return null;

			if (list == null) {
				if (canCreateInstance && typeMap.TypeData.HasPublicConstructor) list = CreateList (listType);
				else throw CreateReadOnlyCollectionException (typeMap.TypeFullName);
			}

			if (Reader.IsEmptyElement) {
				Reader.Skip();
				if (listType.IsArray)
					list = ShrinkArray ((Array)list, 0, listType.GetElementType(), false);
				return list;
			}

			int index = 0;
			Reader.ReadStartElement();
			Reader.MoveToContent();

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					XmlTypeMapElementInfo elemInfo = listMap.FindElement (Reader.LocalName, Reader.NamespaceURI);
					if (elemInfo != null)
						AddListValue (typeMap.TypeData, ref list, index++, ReadObjectElement (elemInfo), false);
					else
						UnknownNode(null);
				}
				else 
					UnknownNode(null);

				Reader.MoveToContent();
			}
			ReadEndElement();

			if (listType.IsArray)
				list = ShrinkArray ((Array)list, index, listType.GetElementType(), false);

			return list;
		}

		object ReadListString (XmlTypeMapping typeMap, string values)
		{
			Type listType = typeMap.TypeData.Type;
			ListMap listMap = (ListMap)typeMap.ObjectMap;
			values = values.Trim ();

			if (values == string.Empty)
			{
				return Array.CreateInstance (listType.GetElementType(), 0);
			}

			string[] valueArray = values.Split (' ');
			Array list = Array.CreateInstance (listType.GetElementType(), valueArray.Length);

			XmlTypeMapElementInfo info = (XmlTypeMapElementInfo)listMap.ItemInfo[0];

			for (int index = 0; index < valueArray.Length; index++)
				list.SetValue (GetValueFromXmlString (valueArray[index], info.TypeData, info.MappedType), index);

			return list;
		}

		void AddListValue (TypeData listType, ref object list, int index, object value, bool canCreateInstance)
		{
			Type type = listType.Type;
			if (type.IsArray)
			{
				list = EnsureArrayIndex ((Array)list, index, type.GetElementType ());
				listType.ConvertForAssignment (ref value);
				((Array)list).SetValue (value, index);
			}
			else	// Must be IEnumerable
			{
				if (list == null) {
					if (canCreateInstance) list = CreateInstance (type, true);
					else throw CreateReadOnlyCollectionException (type.FullName);
				}

				MethodInfo mi = type.GetMethod ("Add", new Type[] {listType.ListItemType} );
				mi.Invoke (list, new object[] { value });
			}
		}

		static object CreateInstance (Type type, bool nonPublic)
		{
			return Activator.CreateInstance (type, nonPublic);
		}

		object CreateInstance (Type type)
		{
			return Activator.CreateInstance (type, empty_array);
		}

		object CreateList (Type listType)
		{
			if (listType.IsArray)
				return EnsureArrayIndex (null, 0, listType.GetElementType());
			else
				return CreateInstance (listType, true);
		}
		
		object InitializeList (TypeData listType)
		{
			if (listType.Type.IsArray)
				return null;
			else
				return CreateInstance (listType.Type, true);
		}

		void FillList (object list, object items)
		{
			CopyEnumerableList (items, list);
		}

		void CopyEnumerableList (object source, object dest)
		{
			if (dest == null) throw CreateReadOnlyCollectionException (source.GetType().FullName);

			object[] param = new object[1];
			MethodInfo mi = dest.GetType().GetMethod ("Add");
			foreach (object ob in (IEnumerable)source)
			{
				param[0] = ob;
				mi.Invoke (dest, param);
			}
		}

		object ReadXmlNodeElement (XmlTypeMapping typeMap, bool isNullable)
		{
			return ReadXmlNode (typeMap.TypeData, false);
		}
		
		object ReadXmlNode (TypeData type, bool wrapped)
		{
			if (type.Type == typeof (XmlDocument))
				return ReadXmlDocument (wrapped);
			else
				return ReadXmlNode (wrapped);
		}

		object ReadPrimitiveElement (XmlTypeMapping typeMap, bool isNullable)
		{
			XmlQualifiedName t = GetXsiType();
			if (t == null) t = new XmlQualifiedName (typeMap.XmlType, typeMap.Namespace);
			return ReadTypedPrimitive (t);
		}

		object ReadEnumElement (XmlTypeMapping typeMap, bool isNullable)
		{
			Reader.ReadStartElement ();
			object o = GetEnumValue (typeMap, Reader.ReadString());
			ReadEndElement ();
			return o;
		}

		object GetEnumValue (XmlTypeMapping typeMap, string val)
		{
			if (val == null)
				return null;
			EnumMap map = (EnumMap) typeMap.ObjectMap;
			string ev = map.GetEnumName (typeMap.TypeFullName, val);
			if (ev == null) throw CreateUnknownConstantException (val, typeMap.TypeData.Type);
			return Enum.Parse (typeMap.TypeData.Type, ev, false);
		}

		object ReadXmlSerializableElement (XmlTypeMapping typeMap, bool isNullable)
		{
			Reader.MoveToContent ();
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (typeMap.IsAny || (Reader.LocalName == typeMap.ElementName && Reader.NamespaceURI == typeMap.Namespace))
				{
					object ob = CreateInstance (typeMap.TypeData.Type, true);
					return ReadSerializable ((IXmlSerializable)ob);
				}
				else
					throw CreateUnknownNodeException ();
			}
			else
			{
				UnknownNode (null);
				return null;
			}
		}

		class FixupCallbackInfo
		{
			XmlSerializationReaderInterpreter _sri;
			ClassMap _map;
			bool _isValueList;

			public FixupCallbackInfo (XmlSerializationReaderInterpreter sri, ClassMap map, bool isValueList)
			{
				_sri = sri;
				_map = map;
				_isValueList = isValueList;
			}

			public void FixupMembers (object fixup)
			{
				_sri.FixupMembers (_map, fixup, _isValueList);
			}
		}

		class ReaderCallbackInfo
		{
			XmlSerializationReaderInterpreter _sri;
			XmlTypeMapping _typeMap;

			public ReaderCallbackInfo (XmlSerializationReaderInterpreter sri, XmlTypeMapping typeMap)
			{
				_sri = sri;
				_typeMap = typeMap;
			}

			internal object ReadObject ()
			{
				return _sri.ReadObject (_typeMap, true, true);
			}
		}
	}
}
