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
		XmlMapping _typeMap;

		public XmlSerializationWriterInterpreter(XmlMapping typeMap)
		{
			_typeMap = typeMap;
		}

		protected override void InitCallbacks ()
		{
		}

		internal override void WriteObject (object ob)
		{
			WriteStartDocument ();

			if (_typeMap is XmlTypeMapping)
			{
				XmlTypeMapping mp = (XmlTypeMapping) _typeMap;
				WriteObject (mp, ob, mp.ElementName, mp.Namespace, true, false);
			}
			else if (ob is object[])
				WriteMessage ((XmlMembersMapping)_typeMap, (object[]) ob);
			else
				throw CreateUnknownTypeException (ob);
		}

		void WriteObject (XmlTypeMapping typeMap, object ob, string element, string namesp, bool isNullable, bool needType)
		{
			if (ob == null)
			{
				if (isNullable) WriteNullTagLiteral(element, namesp);
				return;
			}

			XmlTypeMapping map = typeMap.GetRealTypeMap (ob.GetType().FullName);
			if (map == null)
			{
				WriteTypedPrimitive (element, namesp, ob, true);
			}
			else 
			{
				if (map != typeMap) needType = true;
				switch (map.TypeData.SchemaType)
				{
					case SchemaTypes.Class: WriteObjectElement (map, ob, element, namesp, needType); break;
					case SchemaTypes.Array: WriteListElement (map, ob, element, namesp, needType); break;
					case SchemaTypes.XmlNode: WriteXmlNodeElement (map, ob, element, namesp, needType); break;
					case SchemaTypes.Primitive: WriteTypedPrimitive (element, namesp, ob, needType); break;
					case SchemaTypes.Enum: WriteEnumElement (map, ob, element, namesp, needType); break;
				}
			}
		}

		void WriteMessage (XmlMembersMapping membersMap, object[] parameters)
		{
			if (membersMap.HasWrapperElement) {
				WriteStartDocument();
				// TopLevelElement();
				WriteStartElement(membersMap.ElementName, membersMap.Namespace);
			}
			
			WriteMembers ((ClassMap)membersMap.ObjectMap, parameters, true);

			if (membersMap.HasWrapperElement)
				WriteEndElement();
		}

		void WriteObjectElement (XmlTypeMapping typeMap, object ob, string element, string namesp, bool needType)
		{
			WriteStartElement(element, namesp, ob);
			if (needType) 
				WriteXsiType(typeMap.XmlType, typeMap.Namespace);

			ClassMap map = (ClassMap)typeMap.ObjectMap;
			WriteMembers (map, ob, false);
			WriteEndElement (ob);
		}

		void WriteMembers (ClassMap map, object ob, bool isValueList)
		{
			// Write attributes

			ICollection attributes = map.AttributeMembers;
			if (attributes != null)
			{
				foreach (XmlTypeMapMemberAttribute attr in attributes) {
					if (MemberHasValue (attr, ob, isValueList))
						WriteAttribute(attr.AttributeName, attr.Namespace, XmlCustomFormatter.ToXmlString (GetMemberValue (attr, ob, isValueList)));
				}
			}

			XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
			if (anyAttrMember != null && MemberHasValue (anyAttrMember, ob, isValueList))
			{
				ICollection extraAtts = (ICollection) GetMemberValue (anyAttrMember, ob, isValueList);
				if (extraAtts != null) 
				{
					foreach (XmlAttribute attr in extraAtts)
						WriteAttribute(attr.LocalName, attr.NamespaceURI, attr.Value);
				}
			}

			// Write elements

			ICollection members = map.ElementMembers;
			if (members != null)
			{
				foreach (XmlTypeMapMemberElement member in members)
				{
					if (!MemberHasValue (member, ob, isValueList)) continue;
					object memberValue = GetMemberValue (member, ob, isValueList);
					Type memType = member.GetType();

					if (memType == typeof(XmlTypeMapMemberList))
					{
						if (memberValue != null) 
						{
							XmlTypeMapMemberList mm = (XmlTypeMapMemberList)member;
							WriteStartElement(mm.ElementName, mm.Namespace, memberValue);
							WriteListContent (member.TypeData, (ListMap) mm.ListTypeMapping.ObjectMap, memberValue);
							WriteEndElement (memberValue);
						}
					}
					else if (memType == typeof(XmlTypeMapMemberFlatList))
					{
						if (memberValue != null)
							WriteListContent (member.TypeData, ((XmlTypeMapMemberFlatList)member).ListMap, memberValue);
					}
					else if (memType == typeof(XmlTypeMapMemberAnyElement))
					{
						if (memberValue != null)
							WriteAnyElementContent ((XmlTypeMapMemberAnyElement)member, memberValue);
					}
					else if (memType == typeof(XmlTypeMapMemberAnyElement))
					{
						if (memberValue != null)
							WriteAnyElementContent ((XmlTypeMapMemberAnyElement)member, memberValue);
					}
					else if (memType == typeof(XmlTypeMapMemberAnyAttribute))
					{
						// Ignore
					}
					else if (memType == typeof(XmlTypeMapMemberElement))
					{
						XmlTypeMapElementInfo elem = member.FindElement (ob, memberValue);
						WriteMemberElement (elem, memberValue);
					}
					else
						throw new InvalidOperationException ("Unknown member type");
				}
			}
		}

		object GetMemberValue (XmlTypeMapMember member, object ob, bool isValueList)
		{
			if (isValueList) return ((object[])ob)[member.Index];
			else return member.GetValue (ob);
		}

		bool MemberHasValue (XmlTypeMapMember member, object ob, bool isValueList)
		{
			if (isValueList) return member.Index < ((object[])ob).Length;
			else return true;
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
			else if (elem.TypeData.SchemaType == SchemaTypes.Enum)
				WriteElementString(elem.ElementName, elem.Namespace, GetEnumXmlValue (elem.MappedType, memberValue));
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

		void WriteAnyElementContent (XmlTypeMapMemberAnyElement member, object memberValue)
		{
			if (member.TypeData.Type == typeof (XmlElement)) {
				memberValue = new object[] { memberValue };
			}

			Array elems = (Array) memberValue;
			foreach (XmlNode elem in elems)
			{
				if (elem is XmlElement) 
				{
					if (member.IsElementDefined (elem.Name, elem.NamespaceURI))
						WriteElementLiteral(elem, "", "", false, true);
					else
						throw CreateUnknownAnyElementException (elem.Name, elem.NamespaceURI);
				}
				else
					CreateUnknownTypeException (elem);
			}
		}

		void WriteXmlNodeElement (XmlTypeMapping typeMap, object ob, string element, string namesp, bool needType)
		{
			WriteElementLiteral((XmlNode)ob, "", "", true, false);
		}

		void WriteEnumElement (XmlTypeMapping typeMap, object ob, string element, string namesp, bool needType)
		{
			Writer.WriteStartElement(element, namesp);
			if (needType) WriteXsiType(typeMap.XmlType, typeMap.Namespace);
			Writer.WriteString (GetEnumXmlValue (typeMap, ob));
			Writer.WriteEndElement();
		}

		string GetEnumXmlValue (XmlTypeMapping typeMap, object ob)
		{
			EnumMap map = (EnumMap)typeMap.ObjectMap;
			return map.GetXmlName (ob.ToString ());
		}
	}
}
