//
// XmlSerializationReaderInterpreter.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Collections;

namespace System.Xml.Serialization
{
	public class XmlSerializationReaderInterpreter: XmlSerializationReader
	{
		XmlTypeMapping _typeMap;

		public XmlSerializationReaderInterpreter(XmlTypeMapping typeMap)
		{
			_typeMap = typeMap;
		}

		protected override void InitCallbacks ()
		{
		}

		protected override void InitIDs ()
		{
		}

		internal override object ReadObject ()
		{
			Reader.MoveToContent();
			return ReadObject (_typeMap, true, true);
		}

		object ReadObject (XmlTypeMapping typeMap, bool isNullable, bool checkType)
		{
			switch (typeMap.TypeData.SchemaType)
			{
				case SchemaTypes.Class: return ReadClassInstance (typeMap, isNullable, checkType);
				case SchemaTypes.Array: return ReadListElement (typeMap, isNullable, null, true);
				case SchemaTypes.XmlNode: return ReadXmlNodeElement (typeMap, isNullable);
				case SchemaTypes.Primitive: return ReadPrimitiveElement (typeMap, isNullable);
				case SchemaTypes.Enum: return ReadEnumElement (typeMap, isNullable);
				default: throw new Exception ("Unsupported map type");
			}
		}

		object ReadClassInstance (XmlTypeMapping typeMap, bool isNullable, bool checkType)
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
            }

			object ob = Activator.CreateInstance (typeMap.TypeData.Type);
			Type obType = typeMap.TypeData.Type;
			ClassMap map = (ClassMap) typeMap.ObjectMap;

			// Reads attributes

			XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
			int anyAttributeIndex = 0;
			object anyAttributeArray = null;

			while (Reader.MoveToNextAttribute())
			{
				XmlTypeMapMemberAttribute member = map.GetAttribute (Reader.LocalName, Reader.NamespaceURI);

				if (member != null) {
					member.SetValue (ob, XmlCustomFormatter.FromXmlString (member.TypeData.Type, Reader.Value));
				}
				else if (IsXmlnsAttribute(Reader.Name)) {
					// Ignore
				}	
				else if (anyAttrMember != null) {
					AddListValue (anyAttrMember.TypeData.Type, ref anyAttributeArray, anyAttributeIndex++, Document.ReadNode(Reader), true);
				}
				else
					UnknownNode(ob);
			}

			if (anyAttrMember != null)
			{
				anyAttributeArray = ShrinkArray ((Array)anyAttributeArray, anyAttributeIndex, anyAttrMember.TypeData.Type.GetElementType(), true);
				anyAttrMember.SetValue (ob, anyAttributeArray);
			}

			// Reads elements

			Reader.MoveToElement();
			if (Reader.IsEmptyElement) 
			{
				Reader.Skip();
				return ob;
			}

			bool[] readFlag = new bool[map.ElementMembers.Count];

			Reader.ReadStartElement();
			Reader.MoveToContent();

			int[] indexes = null;
			object[] flatLists = null;

			if (map.FlatLists != null) 
			{
				indexes = new int[map.FlatLists.Count];
				flatLists = new object[map.FlatLists.Count];
				foreach (XmlTypeMapMemberExpandable mem in map.FlatLists)
					if (mem.IsReadOnly (obType)) flatLists[mem.FlatArrayIndex] = mem.GetValue (ob);
			}

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					XmlTypeMapElementInfo info = map.GetElement (Reader.LocalName, Reader.NamespaceURI);
					if (info != null && !readFlag[info.Member.Index] )
					{
						if (info.Member.GetType() == typeof (XmlTypeMapMemberList))
						{
							if (info.Member.IsReadOnly (obType)) ReadListElement (info.MappedType, info.IsNullable, info.Member.GetValue (ob), false);
							else info.Member.SetValue (ob, ReadListElement (info.MappedType, info.IsNullable, null, true));
							readFlag[info.Member.Index] = true;
						}
						else if (info.Member.GetType() == typeof (XmlTypeMapMemberFlatList))
						{
							XmlTypeMapMemberFlatList mem = (XmlTypeMapMemberFlatList)info.Member;
							AddListValue (mem.TypeData.Type, ref flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex]++, ReadObjectElement (info), !info.Member.IsReadOnly (obType));
						}
						else if (info.Member.GetType() == typeof (XmlTypeMapMemberAnyElement))
						{
							XmlTypeMapMemberAnyElement mem = (XmlTypeMapMemberAnyElement)info.Member;
							if (mem.TypeData.IsListType) AddListValue (mem.TypeData.Type, ref flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex]++, ReadXmlNode (false), true);
							else mem.SetValue (ob, ReadXmlNode (false));
						}
						else if (info.Member.GetType() == typeof(XmlTypeMapMemberElement))
						{
							info.Member.SetValue (ob, ReadObjectElement (info));
							readFlag[info.Member.Index] = true;
						}
						else
							throw new InvalidOperationException ("Unknown member type");
					}
					else if (map.DefaultAnyElementMember != null)
					{
						XmlTypeMapMemberAnyElement mem = map.DefaultAnyElementMember;
						if (mem.TypeData.IsListType) AddListValue (mem.TypeData.Type, ref flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex]++, ReadXmlNode (false), true);
						else mem.SetValue (ob, ReadXmlNode (false));
					}
					else 
						UnknownNode(ob);
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
					if (!mem.IsReadOnly (obType))
						mem.SetValue (ob, list);
				}
			}

			ReadEndElement();
			return ob;
		}

		object ReadObjectElement (XmlTypeMapElementInfo elem)
		{
			if (elem.IsPrimitive)
			{
				if (elem.TypeData.SchemaType == SchemaTypes.XmlNode)
					return ReadXmlNode (true);
				else if (elem.IsNullable) 
					return XmlCustomFormatter.FromXmlString (elem.TypeData.Type, ReadNullableString ());
				else 
					return XmlCustomFormatter.FromXmlString (elem.TypeData.Type, Reader.ReadElementString ());
			}
			else if (elem.MappedType.TypeData.SchemaType == SchemaTypes.Array) {
				return ReadListElement (elem.MappedType, elem.IsNullable, null, true);
			}
			else if (elem.MappedType.TypeData.SchemaType == SchemaTypes.Enum) {
				return GetEnumValue (elem.MappedType, Reader.ReadElementString());
			}
			else
				return ReadObject (elem.MappedType, elem.IsNullable, true);
		}

		object ReadListElement (XmlTypeMapping typeMap, bool isNullable, object list, bool canCreateInstance)
		{
			Type listType = typeMap.TypeData.Type;
			ListMap listMap = (ListMap)typeMap.ObjectMap;

			if (ReadNull()) return null;

			if (list == null) {
				if (canCreateInstance) list = CreateList (listType);
				else throw CreateReadOnlyCollectionException (typeMap.TypeFullName);
			}	

			if (Reader.IsEmptyElement) {
				Reader.Skip();
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
						AddListValue (listType, ref list, index++, ReadObjectElement (elemInfo), false);
					else 
						UnknownNode(null);
				}
				else 
					UnknownNode(null);

				Reader.MoveToContent();
			}
			ReadEndElement();

			if (listType.IsArray)
				list = ShrinkArray ((Array)list, index, listType.GetElementType(), isNullable);

			return list;
		}

		void AddListValue (Type listType, ref object list, int index, object value, bool canCreateInstance)
		{
			if (listType.IsArray)
			{
				list = EnsureArrayIndex ((Array)list, index, listType.GetElementType());
				((Array)list).SetValue (value, index);
			}
			else	// Must be IEnumerable
			{
				if (list == null) {
					if (canCreateInstance) list = Activator.CreateInstance (listType);
					else throw CreateReadOnlyCollectionException (listType.FullName);
				}

				MethodInfo mi = listType.GetMethod ("Add");
				mi.Invoke (list, new object[] { value });
			}
		}

		object CreateList (Type listType)
		{
			if (listType.IsArray)
				return EnsureArrayIndex (null, 0, listType.GetElementType());
			else
				return Activator.CreateInstance (listType);
		}

		object ReadXmlNodeElement (XmlTypeMapping typeMap, bool isNullable)
		{
			return ReadXmlNode (false);
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
			Reader.ReadEndElement ();
			return o;
		}

		object GetEnumValue (XmlTypeMapping typeMap, string val)
		{
			EnumMap map = (EnumMap) typeMap.ObjectMap;
			return Enum.Parse (typeMap.TypeData.Type, map.GetEnumName (val));
		}
	}
}
