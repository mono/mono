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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
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

			if (IsPrimitiveType (type))
				return null;

			return CreateDefaultTypeMap (type);
		}

		static bool IsPrimitiveType (Type type)
		{
			if (type.IsEnum)
				return true;
			if (Type.GetTypeCode (type) != TypeCode.Object)
				return true; // FIXME: it is likely hacky
			return false;
		}

		static TypeMap CreateDefaultTypeMap (Type type)
		{
			var l = new List<TypeMapMember> ();
			foreach (var fi in type.GetFields ())
				if (!fi.IsStatic)
					l.Add (new TypeMapField (fi, null));
			foreach (var pi in type.GetProperties ())
				if (pi.CanRead && pi.CanWrite && !pi.GetGetMethod ().IsStatic && pi.GetIndexParameters ().Length == 0)
					l.Add (new TypeMapProperty (pi, null));
			l.Sort ((x, y) => x.Order != y.Order ? x.Order - y.Order : String.Compare (x.Name, y.Name, StringComparison.Ordinal));
			return new TypeMap (type, null, l.ToArray ());
		}

		internal static bool IsDictionary (Type type)
		{
			if (type.GetInterface ("System.Collections.IDictionary", false) != null)
				return true;
			if (type.GetInterface ("System.Collections.Generic.IDictionary`2", false) != null)
				return true;
			return false;
		}

		internal static bool IsCollection (Type type)
		{
			if (IsPrimitiveType (type) || IsDictionary (type))
				return false;
			if (type.GetInterface ("System.Collections.IEnumerable", false) != null)
				return true;
			return false;
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
					if (fi.GetCustomAttributes (typeof (IgnoreDataMemberAttribute), false).Length > 0)
						continue;
					members.Add (new TypeMapField (fi, null));
				}
			}

			if (dca != null) {
				foreach (PropertyInfo pi in type.GetProperties (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
					object [] atts = pi.GetCustomAttributes (typeof (DataMemberAttribute), true);
					if (atts.Length == 0)
						continue;
					if (pi.GetIndexParameters ().Length > 0)
						continue;
					if (IsCollection (pi.PropertyType)) {
						if (!pi.CanRead)
							throw new InvalidDataContractException (String.Format ("Property {0} must have a getter", pi));
					}
					else if (!pi.CanRead || !pi.CanWrite)
						throw new InvalidDataContractException (String.Format ("Non-collection property {0} must have both getter and setter", pi));
					DataMemberAttribute dma = (DataMemberAttribute) atts [0];
					members.Add (new TypeMapProperty (pi, dma));
				}
			}

			members.Sort (delegate (TypeMapMember m1, TypeMapMember m2) { return m1.Order != m2.Order ? m1.Order - m2.Order : String.CompareOrdinal (m1.Name, m2.Name); });
			return new TypeMap (type, dca == null ? null : dca.Name, members.ToArray ());
		}

		Type type;
		string element;
		TypeMapMember [] members;

		static readonly Type [] deser_methods_args = new Type [] { typeof (StreamingContext) };
		const BindingFlags binding_flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		public TypeMap (Type type, string element, TypeMapMember [] orderedMembers)
		{
			this.type = type;
			this.element = element;
			this.members = orderedMembers;

			foreach (var mi in type.GetMethods (binding_flags)) {
				if (mi.GetCustomAttributes (typeof (OnDeserializingAttribute), false).Length > 0)
					OnDeserializing = mi;
				else if (mi.GetCustomAttributes (typeof (OnDeserializedAttribute), false).Length > 0)
					OnDeserialized = mi;
				else if (mi.GetCustomAttributes (typeof (OnSerializingAttribute), false).Length > 0)
					OnSerializing = mi;
				else if (mi.GetCustomAttributes (typeof (OnSerializedAttribute), false).Length > 0)
					OnSerialized = mi;
			}
		}

		public MethodInfo OnDeserializing { get; set; }
		public MethodInfo OnDeserialized { get; set; }
		public MethodInfo OnSerializing { get; set; }
		public MethodInfo OnSerialized { get; set; }

		public virtual void Serialize (JsonSerializationWriter outputter, object graph, string type)
		{
			if (OnSerializing != null)
				OnSerializing.Invoke (graph, new object [] {new StreamingContext (StreamingContextStates.All)});

			outputter.Writer.WriteAttributeString ("type", type);
			foreach (TypeMapMember member in members) {
				object memberObj = member.GetMemberOf (graph);
				// FIXME: consider EmitDefaultValue
				outputter.Writer.WriteStartElement (member.Name);
				outputter.WriteObjectContent (memberObj, false, false);
				outputter.Writer.WriteEndElement ();
			}

			if (OnSerialized != null)
				OnSerialized.Invoke (graph, new object [] {new StreamingContext (StreamingContextStates.All)});
		}

		internal static object CreateInstance (Type type)
		{
			if (TypeMap.IsDictionary (type)) {
				if (type.IsGenericType)
					return Activator.CreateInstance (typeof (Dictionary<,>).MakeGenericType (type.GetGenericArguments ()));
				else
					return new Hashtable ();
			} else if (TypeMap.IsCollection (type)) {
				if (type.IsGenericType)
					return Activator.CreateInstance (typeof (List<>).MakeGenericType (type.GetGenericArguments ()));
				else
					return new ArrayList ();
			}
			else
				return FormatterServices.GetUninitializedObject (type);
		}

		public virtual object Deserialize (JsonSerializationReader jsr)
		{
			XmlReader reader = jsr.Reader;
			bool isNull = reader.GetAttribute ("type") == "null";

			object ret = isNull ? null : CreateInstance (type);
			if (ret != null && OnDeserializing != null)
				OnDeserializing.Invoke (ret, new object [] {new StreamingContext (StreamingContextStates.All)});
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
			if (ret != null && OnDeserialized != null)
				OnDeserialized.Invoke (ret, new object [] {new StreamingContext (StreamingContextStates.All)});
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

		public bool EmitDefaultValue {
			get { return dma != null && dma.EmitDefaultValue; }
		}

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
