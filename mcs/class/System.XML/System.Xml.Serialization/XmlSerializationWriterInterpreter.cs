//
// XmlSerializationWriterInterpreter.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Xml.Serialization
{
	internal class XmlSerializationWriterInterpreter: XmlSerializationWriter
	{
		XmlTypeMapping _typeMap;

		public XmlSerializationWriterInterpreter(XmlTypeMapping typeMap)
		{
			_typeMap = typeMap;
		}

		protected override void InitCallbacks ()
		{
		}

		internal override void WriteObject (object ob)
		{
			WriteObject (_typeMap, ob, _typeMap.ElementName, _typeMap.Namespace, true, false);
		}

		void WriteObject (XmlTypeMapping typeMap, object ob, string element, string namesp, bool isNullable, bool needType)
		{
			if (ob == null)
			{
				if (isNullable) WriteNullTagLiteral(element, namesp);
				return;
			}

			XmlTypeMapping map = typeMap.GetRealTypeMap (ob.GetType().FullName);
			if (map != typeMap) needType = true;

			switch (map.TypeData.SchemaType)
			{
				case SchemaTypes.Class: WriteObjectElement (map, ob, element, namesp, needType); break;
				case SchemaTypes.Array: WriteListElement (map, ob, element, namesp, needType); break;
				case SchemaTypes.XmlNode: WriteXmlNodeElement (map, ob, element, namesp, needType); break;
			}
		}

		void WriteObjectElement (XmlTypeMapping typeMap, object ob, string element, string namesp, bool needType)
		{
			WriteStartElement(element, namesp, ob);
			if (needType) 
				WriteXsiType(typeMap.XmlType, typeMap.Namespace);

			ClassMap map = (ClassMap)typeMap.ObjectMap;
			ICollection attributes = map.AttributeMembers;
			if (attributes != null)
			{
				foreach (XmlTypeMapMemberAttribute attr in attributes)
					WriteAttribute(attr.AttributeName, attr.Namespace, XmlCustomFormatter.ToXmlString (attr.GetValue(ob)));
			}

			ICollection members = map.ElementMembers;
			if (members != null)
			{
				foreach (XmlTypeMapMemberElement member in members)
				{
					object memberValue = member.GetValue (ob);
					if (member is XmlTypeMapMemberList)
					{
						if (memberValue != null) {
							XmlTypeMapMemberList mm = (XmlTypeMapMemberList)member;
							WriteStartElement(mm.ElementName, mm.Namespace, memberValue);
							WriteListContent (member.TypeData, (ListMap) mm.ListTypeMapping.ObjectMap, memberValue);
							WriteEndElement (memberValue);
						}
					}
					else if (member is XmlTypeMapMemberFlatList)
					{
						if (memberValue != null)
							WriteListContent (member.TypeData, ((XmlTypeMapMemberFlatList)member).ListMap, memberValue);
					}
					else
					{
						XmlTypeMapElementInfo elem = member.FindElement (ob, memberValue);
						WriteMemberElement (elem, memberValue);
					}
				}
			}
			WriteEndElement (ob);
		}

		void WriteMemberElement (XmlTypeMapElementInfo elem, object memberValue)
		{
			if (elem.IsPrimitive)
			{
				if (elem.TypeData.SchemaType == SchemaTypes.XmlNode)
					WriteElementLiteral(((XmlNode)memberValue), elem.ElementName, elem.Namespace, elem.IsNullable, false);
				else if (elem.IsNullable)
					WriteNullableStringLiteral (elem.ElementName, elem.Namespace, XmlCustomFormatter.ToXmlString (memberValue));
				else 
					WriteElementString (elem.ElementName, elem.Namespace, XmlCustomFormatter.ToXmlString (memberValue));
			}
			else
				WriteObject (elem.MappedType, memberValue, elem.ElementName, elem.Namespace, elem.IsNullable, false);
		}

		void WriteListElement (XmlTypeMapping typeMap, object ob, string element, string namesp, bool needType)
		{
			WriteStartElement(element, namesp, ob);
			if (needType) 
				WriteXsiType(typeMap.XmlType, typeMap.Namespace);

			WriteListContent (typeMap.TypeData, (ListMap) typeMap.ObjectMap, ob);
			WriteEndElement (ob);
		}

		void WriteListContent (TypeData listType, ListMap map, object ob)
		{
			if (listType.Type.IsArray)
			{
				Array array = (Array)ob;
				for (int n=0; n<array.Length; n++)
				{
					object item = array.GetValue (n);
					XmlTypeMapElementInfo info = map.FindElement (item);
					if (info != null) WriteMemberElement (info, item);
					else if (item != null) throw CreateUnknownTypeException (item);
				}
			}
			else if (ob is ICollection)
			{
				int count = (int) listType.Type.GetProperty ("Count").GetValue(ob,null);
				PropertyInfo itemProp = listType.Type.GetProperty ("Item");
				object[] index = new object[1];
				for (int n=0; n<count; n++)
				{
					index[0] = n;
					object item = itemProp.GetValue (ob, index);
					XmlTypeMapElementInfo info = map.FindElement (item);
					if (info != null) WriteMemberElement (info, item);
					else if (item != null) throw CreateUnknownTypeException (item);
				}
			}
			else if (ob is IEnumerable)
			{
				IEnumerable e = (IEnumerable)ob;
				foreach (object item in e)
				{
					XmlTypeMapElementInfo info = map.FindElement (item);
					if (info != null) WriteMemberElement (info, item);
					else if (item != null) throw CreateUnknownTypeException (item);
				}
			}
			else
				throw new Exception ("Unsupported collection type");
		}

		void WriteXmlNodeElement (XmlTypeMapping typeMap, object ob, string element, string namesp, bool needType)
		{
			WriteElementLiteral((XmlNode)ob, "", "", true, true);
		}
	}
}
