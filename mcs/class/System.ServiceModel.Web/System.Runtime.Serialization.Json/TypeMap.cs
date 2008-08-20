//
// TypeMap.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace System.Runtime.Serialization.Json
{
	class TypeMap
	{
		static bool IsInvalidNCName (string name)
		{
			if (name == null || name.Length == 0)
				return true;
			try {
				XmlConvert.VerifyNCName (name);
			} catch (XmlException) {
				return true;
			}
			return false;
		}

		public static TypeMap CreateTypeMap (Type type)
		{
			object [] atts = type.GetCustomAttributes (typeof (DataContractAttribute), true);
			if (atts.Length == 1)
				return CreateTypeMap (type, (DataContractAttribute) atts [0]);

			atts = type.GetCustomAttributes (typeof (SerializableAttribute), false);
			if (atts.Length == 1)
				return CreateTypeMap (type, null);

			return null;
		}

		static TypeMap CreateTypeMap (Type type, DataContractAttribute dca)
		{
			if (dca != null && dca.Name != null && IsInvalidNCName (dca.Name))
				throw new InvalidDataContractException (String.Format ("DataContractAttribute for type '{0}' has an invalid name", type));

			List<TypeMapMember> members = new List<TypeMapMember> ();

			foreach (FieldInfo fi in type.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				if (dca != null) {
					object [] atts = fi.GetCustomAttributes (typeof (DataMemberAttribute), true);
					if (atts.Length == 0)
						continue;
					DataMemberAttribute dma = (DataMemberAttribute) atts [0];
					members.Add (new TypeMapField (fi, dma));
				} else {
					if (fi.GetCustomAttributes (typeof (NonSerializedAttribute), false).Length > 0)
						continue;
					members.Add (new TypeMapField (fi, null));
				}
			}

			if (dca != null) {
				foreach (PropertyInfo pi in type.GetProperties (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
					if (pi.GetIndexParameters ().Length > 0)
						continue;
					if (!pi.CanRead || !pi.CanWrite)
						throw new InvalidDataContractException (String.Format ("Property {0} must have both getter and setter", pi));
					object [] atts = pi.GetCustomAttributes (typeof (DataMemberAttribute), true);
					if (atts.Length == 0)
						continue;
					DataMemberAttribute dma = (DataMemberAttribute) atts [0];
					members.Add (new TypeMapProperty (pi, dma));
				}
			}

			members.Sort (delegate (TypeMapMember m1, TypeMapMember m2) { return m1.Order - m2.Order; });
			return new TypeMap (type, dca == null ? null : dca.Name, members.ToArray ());
		}

		Type type;
		string element;
		TypeMapMember [] members;

		public TypeMap (Type type, string element, TypeMapMember [] orderedMembers)
		{
			this.type = type;
			this.element = element;
			this.members = orderedMembers;
		}

		public void Serialize (JsonSerializationWriter outputter, object graph)
		{
			foreach (TypeMapMember member in members) {
				object memberObj = member.GetMemberOf (graph);
				// FIXME: consider EmitDefaultValue
				outputter.Writer.WriteStartElement (member.Name);
				outputter.WriteObjectContent (memberObj, false, false);
				outputter.Writer.WriteEndElement ();
			}
		}

		public object Deserialize (JsonSerializationReader jsr)
		{
			XmlReader reader = jsr.Reader;

			object ret = Activator.CreateInstance (type, true);
			Dictionary<TypeMapMember,bool> filled = new Dictionary<TypeMapMember,bool> ();

			reader.ReadStartElement ();
			for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
				bool consumed = false;
				for (int i = 0; i < members.Length; i++) {
					TypeMapMember mm = members [i];
					if (mm.Name == reader.LocalName && reader.NamespaceURI == String.Empty) {
						if (filled.ContainsKey (mm))
							throw new SerializationException (String.Format ("Object content '{0}' for '{1}' already appeared in the reader", reader.LocalName, type));
						mm.SetMemberValue (ret, jsr.ReadObject (mm.Type));
						filled [mm] = true;
						consumed = true;
						break;
					}
				}
				if (!consumed)
					reader.Skip ();
			}
			reader.ReadEndElement ();
			return ret;
		}
	}

	abstract class TypeMapMember
	{
		MemberInfo mi;
		DataMemberAttribute dma;

		protected TypeMapMember (MemberInfo mi, DataMemberAttribute dma)
		{
			this.mi = mi;
			this.dma = dma;
		}

		public string Name {
			get { return dma == null ? mi.Name : dma.Name ?? mi.Name; }
		}

		// FIXME: Fill 3.5 member in s.r.serialization.
//		public bool EmitDefaultValue {
//			get { return dma != null && dma.EmitDefaultValue; }
//		}

		public bool IsRequired {
			get { return dma != null && dma.IsRequired; }
		}

		public int Order {
			get { return dma != null ? dma.Order : -1; }
		}

		public abstract Type Type { get; }

		public abstract object GetMemberOf (object owner);

		public abstract void SetMemberValue (object owner, object value);
	}

	class TypeMapField : TypeMapMember
	{
		FieldInfo field;

		public TypeMapField (FieldInfo fi, DataMemberAttribute dma)
			: base (fi, dma)
		{
			this.field = fi;
		}

		public override Type Type {
			get { return field.FieldType; }
		}

		public override object GetMemberOf (object owner)
		{
			return field.GetValue (owner);
		}

		public override void SetMemberValue (object owner, object value)
		{
			field.SetValue (owner, value);
		}
	}

	class TypeMapProperty : TypeMapMember
	{
		PropertyInfo property;

		public TypeMapProperty (PropertyInfo pi, DataMemberAttribute dma)
			: base (pi, dma)
		{
			this.property = pi;
		}

		public override Type Type {
			get { return property.PropertyType; }
		}

		public override object GetMemberOf (object owner)
		{
			return property.GetValue (owner, null);
		}

		public override void SetMemberValue (object owner, object value)
		{
			property.SetValue (owner, value, null);
		}
	}
}
