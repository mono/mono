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
				case SchemaTypes.Array: return ReadListElement (typeMap, isNullable);
				case SchemaTypes.XmlNode: return ReadXmlNodeElement (typeMap, isNullable);
				default: throw new Exception ("Unsupported map type");
			}
		}

		object ReadClassInstance (XmlTypeMapping typeMap, bool isNullable, bool checkType)
		{
			if (isNullable && ReadNull()) return null;

            if (checkType) 
			{
                System.Xml.XmlQualifiedName t = GetXsiType();
				if (t != null) {
					typeMap = typeMap.GetRealElementMap (t.Name, t.Namespace);
					if (typeMap == null)
						throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)t);
				}
            }

			object ob = Activator.CreateInstance (typeMap.TypeData.Type);

			ClassMap map = (ClassMap) typeMap.ObjectMap;

			while (Reader.MoveToNextAttribute()) 
			{
				XmlTypeMapMemberAttribute member = map.GetAttribute (Reader.LocalName, Reader.NamespaceURI);
				if (member != null)
					member.SetValue (ob, XmlCustomFormatter.FromXmlString (member.TypeData.FullTypeName, Reader.Value));
				else if (!IsXmlnsAttribute(Reader.Name)) 
					UnknownNode(ob);
			}

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

			if (map.FlatLists != null) {
				indexes = new int[map.FlatLists.Count];
				flatLists = new object[map.FlatLists.Count];
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
							info.Member.SetValue (ob, ReadListElement (info.MappedType, info.IsNullable));
							readFlag[info.Member.Index] = true;
						}
						else if (info.Member.GetType() == typeof (XmlTypeMapMemberFlatList))
						{
							XmlTypeMapMemberFlatList mem = (XmlTypeMapMemberFlatList)info.Member;
							AddListValue (mem.TypeData.Type, ref flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex]++, ReadObjectElement (info));
						}
						else if (info.Member.GetType() == typeof(XmlTypeMapMemberElement))
						{
							info.Member.SetValue (ob, ReadObjectElement (info));
							readFlag[info.Member.Index] = true;
						}
						else
							throw new Exception ("Unknown member type");
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
				foreach (XmlTypeMapMemberFlatList mem in map.FlatLists)
				{
					Object list = flatLists[mem.FlatArrayIndex];
					if (mem.TypeData.Type.IsArray)
						list = ShrinkArray ((Array)list, indexes[mem.FlatArrayIndex], mem.TypeData.Type.GetElementType(), true);
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
					return XmlCustomFormatter.FromXmlString (elem.TypeData.FullTypeName, ReadNullableString ());
				else 
					return XmlCustomFormatter.FromXmlString (elem.TypeData.FullTypeName, Reader.ReadElementString ());
			}
			else if (elem.MappedType.TypeData.SchemaType == SchemaTypes.Array)
				return ReadListElement (elem.MappedType, elem.IsNullable);
			else
				return ReadObject (elem.MappedType, elem.IsNullable, true);
		}

		object ReadListElement (XmlTypeMapping typeMap, bool isNullable)
		{
			Type listType = typeMap.TypeData.Type;
			ListMap listMap = (ListMap)typeMap.ObjectMap;

			if (ReadNull()) return null;

			object list = CreateList (listType);

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
						AddListValue (listType, ref list, index++, ReadObjectElement (elemInfo));
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

		void AddListValue (Type listType, ref object list, int index, object value)
		{
			if (listType.IsArray)
			{
				list = EnsureArrayIndex ((Array)list, index, listType.GetElementType());
				((Array)list).SetValue (value, index);
			}
			else	// Must be IEnumerable
			{
				if (list == null) list = Activator.CreateInstance (listType);
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
	}
}
