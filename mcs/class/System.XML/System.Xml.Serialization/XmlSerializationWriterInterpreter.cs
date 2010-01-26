//
// XmlSerializationWriterInterpreter.cs: 
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
using System.Text;
using System.Collections;
using System.Reflection;
using System.Xml.Schema;

namespace System.Xml.Serialization
{
	internal class XmlSerializationWriterInterpreter: XmlSerializationWriter
	{
		XmlMapping _typeMap;
		SerializationFormat _format;
		const string xmlNamespace = "http://www.w3.org/2000/xmlns/";

		public XmlSerializationWriterInterpreter (XmlMapping typeMap)
		{
			_typeMap = typeMap;
			_format = typeMap.Format;
		}

		protected override void InitCallbacks ()
		{
			ArrayList maps = _typeMap.RelatedMaps;
			if (maps != null)
			{
				foreach (XmlTypeMapping map in maps)  {
					CallbackInfo info = new CallbackInfo (this, map);
					if (map.TypeData.SchemaType == SchemaTypes.Enum) AddWriteCallback(map.TypeData.Type, map.XmlType, map.Namespace, new XmlSerializationWriteCallback (info.WriteEnum));
					else AddWriteCallback(map.TypeData.Type, map.XmlType, map.Namespace, new XmlSerializationWriteCallback (info.WriteObject));
				}
			}
		}

		public void WriteRoot (object ob)
		{
			WriteStartDocument ();

			if (_typeMap is XmlTypeMapping)
			{
				XmlTypeMapping mp = (XmlTypeMapping) _typeMap;
				if (mp.TypeData.SchemaType == SchemaTypes.Class || mp.TypeData.SchemaType == SchemaTypes.Array) 
					TopLevelElement ();

				if (_format == SerializationFormat.Literal)
					WriteObject (mp, ob, mp.ElementName, mp.Namespace, true, false, true);
				else
					WritePotentiallyReferencingElement (mp.ElementName, mp.Namespace, ob, mp.TypeData.Type, true, false);
			}
			else if (ob is object[])
				WriteMessage ((XmlMembersMapping)_typeMap, (object[]) ob);
			else
				throw CreateUnknownTypeException (ob);

			WriteReferencedElements ();
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

		protected virtual void WriteObject (XmlTypeMapping typeMap, object ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable) 
				{
					if (_format == SerializationFormat.Literal) WriteNullTagLiteral(element, namesp);
					else WriteNullTagEncoded (element, namesp);
				}
				return;
			}

			if (ob is XmlNode)
			{
				if (_format == SerializationFormat.Literal) WriteElementLiteral((XmlNode)ob, "", "", true, false);
				else WriteElementEncoded((XmlNode)ob, "", "", true, false);
				return;
			}

			if (typeMap.TypeData.SchemaType == SchemaTypes.XmlSerializable)
			{
				WriteSerializable ((IXmlSerializable)ob, element, namesp, isNullable);
				return;
			}

			XmlTypeMapping map = typeMap.GetRealTypeMap (ob.GetType());

			if (map == null) 
			{
				// bug #81539
				if (ob.GetType ().IsArray && typeof (XmlNode).IsAssignableFrom (ob.GetType ().GetElementType ())) {
					Writer.WriteStartElement (element, namesp);
					foreach (XmlNode node in (IEnumerable) ob)
						node.WriteTo (Writer);
					Writer.WriteEndElement ();
				}
				else
					WriteTypedPrimitive (element, namesp, ob, true);
				return;
			}

			if (writeWrappingElem)
			{
				if (map != typeMap || _format == SerializationFormat.Encoded) needType = true;
				WriteStartElement (element, namesp, ob);
			}

			if (needType) 
				WriteXsiType(map.XmlType, map.XmlTypeNamespace);

			switch (map.TypeData.SchemaType)
			{
				case SchemaTypes.Class: WriteObjectElement (map, ob, element, namesp); break;
				case SchemaTypes.Array: WriteListElement (map, ob, element, namesp); break;
				case SchemaTypes.Primitive: WritePrimitiveElement (map, ob, element, namesp); break;
				case SchemaTypes.Enum: WriteEnumElement (map, ob, element, namesp); break;
			}

			if (writeWrappingElem)
				WriteEndElement (ob);
		}

		protected virtual void WriteMessage (XmlMembersMapping membersMap, object[] parameters)
		{
			if (membersMap.HasWrapperElement) {
				TopLevelElement ();
				WriteStartElement(membersMap.ElementName, membersMap.Namespace, (_format == SerializationFormat.Encoded));

				if (Writer.LookupPrefix (XmlSchema.Namespace) == null)
					WriteAttribute ("xmlns","xsd",XmlSchema.Namespace,XmlSchema.Namespace);
	
				if (Writer.LookupPrefix (XmlSchema.InstanceNamespace) == null)
					WriteAttribute ("xmlns","xsi",XmlSchema.InstanceNamespace,XmlSchema.InstanceNamespace);
			}
			
			WriteMembers ((ClassMap)membersMap.ObjectMap, parameters, true);

			if (membersMap.HasWrapperElement)
				WriteEndElement();
		}

		protected virtual void WriteObjectElement (XmlTypeMapping typeMap, object ob, string element, string namesp)
		{
			ClassMap map = (ClassMap)typeMap.ObjectMap;
			if (map.NamespaceDeclarations != null)
				WriteNamespaceDeclarations ((XmlSerializerNamespaces) map.NamespaceDeclarations.GetValue (ob));
			
			WriteObjectElementAttributes (typeMap, ob);
			WriteObjectElementElements (typeMap, ob);
		}
		
		protected virtual void WriteObjectElementAttributes (XmlTypeMapping typeMap, object ob)
		{
			ClassMap map = (ClassMap)typeMap.ObjectMap;
			WriteAttributeMembers (map, ob, false);
		}

		protected virtual void WriteObjectElementElements (XmlTypeMapping typeMap, object ob)
		{
			ClassMap map = (ClassMap)typeMap.ObjectMap;
			WriteElementMembers (map, ob, false);
		}

		void WriteMembers (ClassMap map, object ob, bool isValueList)
		{
			WriteAttributeMembers (map, ob, isValueList);
			WriteElementMembers (map, ob, isValueList);
		}
		
		void WriteAttributeMembers (ClassMap map, object ob, bool isValueList)
		{
			// Write attributes

			XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
			if (anyAttrMember != null && MemberHasValue (anyAttrMember, ob, isValueList))
			{
				ICollection extraAtts = (ICollection) GetMemberValue (anyAttrMember, ob, isValueList);
				if (extraAtts != null) 
				{
					foreach (XmlAttribute attr in extraAtts)
						if (attr.NamespaceURI != xmlNamespace)
							WriteXmlAttribute (attr, ob);
				}
			}

			ICollection attributes = map.AttributeMembers;
			if (attributes != null)
			{
				foreach (XmlTypeMapMemberAttribute attr in attributes) {
					if (MemberHasValue (attr, ob, isValueList))
						WriteAttribute (attr.AttributeName, attr.Namespace, GetStringValue (attr.MappedType, attr.TypeData, GetMemberValue (attr, ob, isValueList)));
				}
			}
		}

		void WriteElementMembers (ClassMap map, object ob, bool isValueList)
		{
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
						WriteMemberElement ((XmlTypeMapElementInfo) member.ElementInfo[0], memberValue);
					}
					else if (memType == typeof(XmlTypeMapMemberFlatList))
					{
						if (memberValue != null)
							WriteListContent (ob, member.TypeData, ((XmlTypeMapMemberFlatList)member).ListMap, memberValue, null);
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
			if (isValueList) return ((object[])ob)[member.GlobalIndex];
			else return member.GetValue (ob);
		}

		bool MemberHasValue (XmlTypeMapMember member, object ob, bool isValueList)
		{
			if (isValueList) {
				return member.GlobalIndex < ((object[])ob).Length;
			}
			else if (member.DefaultValue != System.DBNull.Value) {
				object val = GetMemberValue (member, ob, isValueList);
				if (val == null && member.DefaultValue == null) return false;
				if (val != null && val.GetType().IsEnum)
				{
					if (val.Equals (member.DefaultValue)) return false;
					Type t = Enum.GetUnderlyingType(val.GetType());
					val = Convert.ChangeType (val, t);
				}
				if (val != null && val.Equals (member.DefaultValue)) return false;
			}
			else if (member.IsOptionalValueType)
				return member.GetValueSpecified (ob);

			return true;
		}

		void WriteMemberElement (XmlTypeMapElementInfo elem, object memberValue)
		{
			switch (elem.TypeData.SchemaType)
			{
				case SchemaTypes.XmlNode:
					string elemName = elem.WrappedElement ? elem.ElementName : "";
					if (_format == SerializationFormat.Literal) WriteElementLiteral(((XmlNode)memberValue), elemName, elem.Namespace, elem.IsNullable, false);
					else WriteElementEncoded(((XmlNode)memberValue), elemName, elem.Namespace, elem.IsNullable, false);
					break;

				case SchemaTypes.Enum:
				case SchemaTypes.Primitive:
					if (_format == SerializationFormat.Literal) 
						WritePrimitiveValueLiteral (memberValue, elem.ElementName, elem.Namespace, elem.MappedType, elem.TypeData, elem.WrappedElement, elem.IsNullable);
					else 
						WritePrimitiveValueEncoded (memberValue, elem.ElementName, elem.Namespace, new XmlQualifiedName (elem.DataTypeName, elem.DataTypeNamespace), elem.MappedType, elem.TypeData, elem.WrappedElement, elem.IsNullable);
					break;

				case SchemaTypes.Array:
					if (memberValue == null) {
						if (!elem.IsNullable) return;
						if (_format == SerializationFormat.Literal) WriteNullTagLiteral (elem.ElementName, elem.Namespace);
						else WriteNullTagEncoded (elem.ElementName, elem.Namespace);
					}
					else if (elem.MappedType.MultiReferenceType) 
						WriteReferencingElement (elem.ElementName, elem.Namespace, memberValue, elem.IsNullable);
					else {
						WriteStartElement(elem.ElementName, elem.Namespace, memberValue);
						WriteListContent (null, elem.TypeData, (ListMap) elem.MappedType.ObjectMap, memberValue, null);
						WriteEndElement (memberValue);
					}
					break;

				case SchemaTypes.Class:
					if (elem.MappedType.MultiReferenceType)	{
						if (elem.MappedType.TypeData.Type == typeof(object))
							WritePotentiallyReferencingElement (elem.ElementName, elem.Namespace, memberValue, null, false, elem.IsNullable);
						else
							WriteReferencingElement (elem.ElementName, elem.Namespace, memberValue, elem.IsNullable);
					}
					else WriteObject (elem.MappedType, memberValue, elem.ElementName, elem.Namespace, elem.IsNullable, false, true);
					break;

				case SchemaTypes.XmlSerializable:
					// bug #419973
					if (!elem.MappedType.TypeData.Type.IsInstanceOfType (memberValue))
						memberValue = ImplicitConvert (memberValue, elem.MappedType.TypeData.Type);
					WriteSerializable ((IXmlSerializable) memberValue, elem.ElementName, elem.Namespace, elem.IsNullable);
					break;

				default:
					throw new NotSupportedException ("Invalid value type");
			}
		}

		object ImplicitConvert (object obj, Type type)
		{
			if (obj == null)
				return null;
			for (Type t = type; t != typeof (object); t = t.BaseType) {
				MethodInfo mi = t.GetMethod ("op_Implicit", new Type [] {t});
				if (mi != null && mi.ReturnType.IsAssignableFrom (obj.GetType ()))
					return mi.Invoke (null, new object [] {obj});
			}

			for (Type t = obj.GetType (); t != typeof (object); t = t.BaseType) {
				MethodInfo mi = t.GetMethod ("op_Implicit", new Type [] {t});
				if (mi != null && mi.ReturnType == type)
					return mi.Invoke (null, new object [] {obj});
			}
			return obj;
		}

		void WritePrimitiveValueLiteral (object memberValue, string name, string ns, XmlTypeMapping mappedType, TypeData typeData, bool wrapped, bool isNullable)
		{
			if (!wrapped) {
				WriteValue (GetStringValue (mappedType, typeData, memberValue));
			}
			else if (isNullable) {
				if (typeData.Type == typeof(XmlQualifiedName)) WriteNullableQualifiedNameLiteral (name, ns, (XmlQualifiedName)memberValue);
				else WriteNullableStringLiteral (name, ns, GetStringValue (mappedType, typeData, memberValue));
			}
			else {
				if (typeData.Type == typeof(XmlQualifiedName)) WriteElementQualifiedName (name, ns, (XmlQualifiedName)memberValue);
				else WriteElementString (name, ns, GetStringValue (mappedType, typeData, memberValue));
			}
		}

		void WritePrimitiveValueEncoded (object memberValue, string name, string ns, XmlQualifiedName xsiType, XmlTypeMapping mappedType, TypeData typeData, bool wrapped, bool isNullable)
		{
			if (!wrapped) {
				WriteValue (GetStringValue (mappedType, typeData, memberValue));
			}
			else if (isNullable) {
				if (typeData.Type == typeof(XmlQualifiedName)) WriteNullableQualifiedNameEncoded (name, ns, (XmlQualifiedName)memberValue, xsiType);
				else WriteNullableStringEncoded (name, ns, GetStringValue (mappedType, typeData, memberValue), xsiType);
			}
			else {
				if (typeData.Type == typeof(XmlQualifiedName)) WriteElementQualifiedName (name, ns, (XmlQualifiedName)memberValue, xsiType);
				else WriteElementString (name, ns, GetStringValue (mappedType, typeData, memberValue), xsiType);
			}
		}

		protected virtual void WriteListElement (XmlTypeMapping typeMap, object ob, string element, string namesp)
		{
			if (_format == SerializationFormat.Encoded)
			{
				string n, ns;
				int itemCount = GetListCount (typeMap.TypeData, ob);
				((ListMap) typeMap.ObjectMap).GetArrayType (itemCount, out n, out ns);
				string arrayType = (ns != string.Empty) ? FromXmlQualifiedName (new XmlQualifiedName(n,ns)) : n;
				WriteAttribute ("arrayType", XmlSerializer.EncodingNamespace, arrayType);
			}
			WriteListContent (null, typeMap.TypeData, (ListMap) typeMap.ObjectMap, ob, null);
		}

		void WriteListContent (object container, TypeData listType, ListMap map, object ob, StringBuilder targetString)
		{
			if (listType.Type.IsArray)
			{
				Array array = (Array)ob;
				for (int n=0; n<array.Length; n++)
				{
					object item = array.GetValue (n);
					XmlTypeMapElementInfo info = map.FindElement (container, n, item);
					if (info != null && targetString == null) WriteMemberElement (info, item);
					else if (info != null && targetString != null) targetString.Append (GetStringValue (info.MappedType, info.TypeData, item)).Append (" ");
					else if (item != null) throw CreateUnknownTypeException (item);
				}
			}
			else if (ob is ICollection)
			{
				int count = (int) ob.GetType().GetProperty ("Count").GetValue(ob,null);
				PropertyInfo itemProp = TypeData.GetIndexerProperty (listType.Type);
				object[] index = new object[1];
				for (int n=0; n<count; n++)
				{
					index[0] = n;
					object item = itemProp.GetValue (ob, index);
					XmlTypeMapElementInfo info = map.FindElement (container, n, item);
					if (info != null && targetString == null) WriteMemberElement (info, item);
					else if (info != null && targetString != null) targetString.Append (GetStringValue (info.MappedType, info.TypeData, item)).Append (" ");
					else if (item != null) throw CreateUnknownTypeException (item);
				}
			}
			else if (ob is IEnumerable)
			{
				IEnumerable e = (IEnumerable)ob;
				foreach (object item in e)
				{
					XmlTypeMapElementInfo info = map.FindElement (container, -1, item);
					if (info != null && targetString == null) WriteMemberElement (info, item);
					else if (info != null && targetString != null) targetString.Append (GetStringValue (info.MappedType, info.TypeData, item)).Append (" ");
					else if (item != null) throw CreateUnknownTypeException (item);
				}
			}
			else
				throw new Exception ("Unsupported collection type");
		}

		int GetListCount (TypeData listType, object ob)
		{
			if (listType.Type.IsArray)
				return ((Array)ob).Length;
			else
				return (int) listType.Type.GetProperty ("Count").GetValue(ob,null);
		}

		void WriteAnyElementContent (XmlTypeMapMemberAnyElement member, object memberValue)
		{
			if (member.TypeData.Type == typeof (XmlElement)) {
				memberValue = new object[] { memberValue };
			}

			Array elems = (Array) memberValue;
			foreach (var elem_ in elems)
			{
				XmlNode elem = elem_ as XmlNode;
				if (elem == null)
					throw new InvalidOperationException (String.Format ("XmlAnyElementAttribute can only be applied to members of type XmlElement, XmlElement[] or XmlNode[]. The target object is {0}", elem_ != null ? elem_.GetType () : null));
				if (elem is XmlElement) 
				{
					if (member.IsElementDefined (elem.Name, elem.NamespaceURI))
					{
						if (_format == SerializationFormat.Literal) WriteElementLiteral (elem, "", "", false, true);
						else WriteElementEncoded (elem, "", "", false, true);
					}
					else
						throw CreateUnknownAnyElementException (elem.Name, elem.NamespaceURI);
				}
				else
					elem.WriteTo (Writer);
			}
		}

		protected virtual void WritePrimitiveElement (XmlTypeMapping typeMap, object ob, string element, string namesp)
		{
			Writer.WriteString (GetStringValue (typeMap, typeMap.TypeData, ob));
		}

		protected virtual void WriteEnumElement (XmlTypeMapping typeMap, object ob, string element, string namesp)
		{
			Writer.WriteString (GetEnumXmlValue (typeMap, ob));
		}

		string GetStringValue (XmlTypeMapping typeMap, TypeData type, object value)
		{
			if (type.SchemaType == SchemaTypes.Array) {
				if (value == null) return null;
				StringBuilder sb = new StringBuilder ();
				WriteListContent (null, typeMap.TypeData, (ListMap)typeMap.ObjectMap, value, sb);
				return sb.ToString ().Trim ();
			}
			else if (type.SchemaType == SchemaTypes.Enum)
				return GetEnumXmlValue (typeMap, value);
			else if (type.Type == typeof (XmlQualifiedName))
				return FromXmlQualifiedName ((XmlQualifiedName)value);
			else if (value == null)
				return null;
			else
				return XmlCustomFormatter.ToXmlString (type, value);
		}

		string GetEnumXmlValue (XmlTypeMapping typeMap, object ob)
		{
			if (ob == null)
				return null;
			EnumMap map = (EnumMap)typeMap.ObjectMap;
			return map.GetXmlName (typeMap.TypeFullName, ob);
		}

		class CallbackInfo
		{
			XmlSerializationWriterInterpreter _swi;
			XmlTypeMapping _typeMap;

			public CallbackInfo (XmlSerializationWriterInterpreter swi, XmlTypeMapping typeMap)
			{
				_swi = swi;
				_typeMap = typeMap;
			}

			internal void WriteObject (object ob)
			{
				_swi.WriteObject (_typeMap, ob, _typeMap.ElementName, _typeMap.Namespace, false, false, false);
			}

			internal void WriteEnum (object ob)
			{
				_swi.WriteObject (_typeMap, ob, _typeMap.ElementName, _typeMap.Namespace, false, true, false);
			}
		}

	}
}
