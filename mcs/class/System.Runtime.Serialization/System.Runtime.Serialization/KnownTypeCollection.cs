//
// KnownTypeCollection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
#if NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

using QName = System.Xml.XmlQualifiedName;
using System.Xml.Serialization;

namespace System.Runtime.Serialization
{
/*
	XmlFormatter implementation design inference:

	type definitions:
	- No XML Schema types are directly used. There are some maps from
	  xs:blahType to ms:blahType where the namespaceURI for prefix "ms" is
	  "http://schemas.microsoft.com/2003/10/Serialization/" .

	serializable types:
	- An object being serialized 1) must be of type System.Object, or
	  2) must be null, or 3) must have either a [DataContract] attribute
	  or a [Serializable] attribute to be serializable.
	- When the object is either of type System.Object or null, then the
	  XML type is "anyType".
	- When the object is [Serializable], then the runtime-serialization
	  compatible object graph is written.
	- Otherwise the serialization is based on contract attributes.
	  ([Serializable] takes precedence).

	type derivation:
	- For type A to be serializable, the base type B of A must be
	  serializable.
	- If a type which is [Serializable] and whose base type has a
	  [DataContract], then for base type members [DataContract] is taken.
	- It is vice versa i.e. if the base type is [Serializable] and the
	  derived type has a [DataContract], then [Serializable] takes place
	  for base members.

	known type collection:
	- It internally manages mapping store keyed by contract QNames.
	  KnownTypeCollection.Add() checks if the same QName contract already
	  exists (and raises InvalidOperationException if required).

*/

	internal static class TypeExtensions
	{
#if !NET_4_5
		public static T GetCustomAttribute<T> (this MemberInfo type, bool inherit)
		{
			var arr = type.GetCustomAttributes (typeof (T), inherit);
			return arr != null && arr.Length == 1 ? (T) arr [0] : default (T);
		}
#endif
		public static IEnumerable<Type> GetInterfacesOrSelfInterface (this Type type)
		{
			if (type.IsInterface)
				yield return type;
			foreach (var t in type.GetInterfaces ())
				yield return t;
		}

		public static bool ImplementsInterface (this Type type, Type iface)
		{
			foreach (var t in type.GetInterfacesOrSelfInterface ()) {
				if (t == iface)
					return true;
			}

			var baseType = type.BaseType;
			if (baseType != null)
				return baseType.ImplementsInterface (iface);
			
			return false;
		}
	}

	internal sealed class KnownTypeCollection : Collection<Type>
	{
		internal const string MSSimpleNamespace =
			"http://schemas.microsoft.com/2003/10/Serialization/";
		internal const string MSArraysNamespace =
			"http://schemas.microsoft.com/2003/10/Serialization/Arrays";
		internal const string DefaultClrNamespaceBase =
			"http://schemas.datacontract.org/2004/07/";
		internal const string DefaultClrNamespaceSystem =
			"http://schemas.datacontract.org/2004/07/System";


		static QName any_type, bool_type,
			byte_type, date_type, decimal_type, double_type,
			float_type, string_type,
			short_type, int_type, long_type,
			ubyte_type, ushort_type, uint_type, ulong_type,
			// non-TypeCode
			any_uri_type, base64_type, duration_type, qname_type,
			// custom in ms nsURI schema
			char_type, guid_type,
			// not in ms nsURI schema
			dbnull_type, date_time_offset_type;

		// XmlSchemaType.GetBuiltInPrimitiveType() does not exist in moonlight, so I had to explicitly add them. And now that we have it, it does not make much sense to use #if MOONLIGHT ... #endif for XmlSchemaType anymore :-(
		static Dictionary<string,Type> xs_predefined_types = new Dictionary<string,Type> ();

		static KnownTypeCollection ()
		{
			string s = MSSimpleNamespace;
			any_type = new QName ("anyType", s);
			any_uri_type = new QName ("anyURI", s);
			bool_type = new QName ("boolean", s);
			base64_type = new QName ("base64Binary", s);
			date_type = new QName ("dateTime", s);
			duration_type = new QName ("duration", s);
			qname_type = new QName ("QName", s);
			decimal_type = new QName ("decimal", s);
			double_type = new QName ("double", s);
			float_type = new QName ("float", s);
			byte_type = new QName ("byte", s);
			short_type = new QName ("short", s);
			int_type = new QName ("int", s);
			long_type = new QName ("long", s);
			ubyte_type = new QName ("unsignedByte", s);
			ushort_type = new QName ("unsignedShort", s);
			uint_type = new QName ("unsignedInt", s);
			ulong_type = new QName ("unsignedLong", s);
			string_type = new QName ("string", s);
			guid_type = new QName ("guid", s);
			char_type = new QName ("char", s);

			dbnull_type = new QName ("DBNull", DefaultClrNamespaceBase + "System");
			date_time_offset_type = new QName ("DateTimeOffset", DefaultClrNamespaceBase + "System");

			xs_predefined_types.Add ("string", typeof (string));
			xs_predefined_types.Add ("boolean", typeof (bool));
			xs_predefined_types.Add ("float", typeof (float));
			xs_predefined_types.Add ("double", typeof (double));
			xs_predefined_types.Add ("decimal", typeof (decimal));
			xs_predefined_types.Add ("duration", typeof (TimeSpan));
			xs_predefined_types.Add ("dateTime", typeof (DateTime));
			xs_predefined_types.Add ("date", typeof (DateTime));
			xs_predefined_types.Add ("time", typeof (DateTime));
			xs_predefined_types.Add ("gYearMonth", typeof (DateTime));
			xs_predefined_types.Add ("gYear", typeof (DateTime));
			xs_predefined_types.Add ("gMonthDay", typeof (DateTime));
			xs_predefined_types.Add ("gDay", typeof (DateTime));
			xs_predefined_types.Add ("gMonth", typeof (DateTime));
			xs_predefined_types.Add ("hexBinary", typeof (byte []));
			xs_predefined_types.Add ("base64Binary", typeof (byte []));
			xs_predefined_types.Add ("anyURI", typeof (Uri));
			xs_predefined_types.Add ("QName", typeof (QName));
			xs_predefined_types.Add ("NOTATION", typeof (string));

			xs_predefined_types.Add ("normalizedString", typeof (string));
			xs_predefined_types.Add ("token", typeof (string));
			xs_predefined_types.Add ("language", typeof (string));
			xs_predefined_types.Add ("IDREFS", typeof (string []));
			xs_predefined_types.Add ("ENTITIES", typeof (string []));
			xs_predefined_types.Add ("NMTOKEN", typeof (string));
			xs_predefined_types.Add ("NMTOKENS", typeof (string []));
			xs_predefined_types.Add ("Name", typeof (string));
			xs_predefined_types.Add ("NCName", typeof (string));
			xs_predefined_types.Add ("ID", typeof (string));
			xs_predefined_types.Add ("IDREF", typeof (string));
			xs_predefined_types.Add ("ENTITY", typeof (string));

			xs_predefined_types.Add ("integer", typeof (decimal));
			xs_predefined_types.Add ("nonPositiveInteger", typeof (int));
			xs_predefined_types.Add ("negativeInteger", typeof (int));
			xs_predefined_types.Add ("long", typeof (long));
			xs_predefined_types.Add ("int", typeof (int));
			xs_predefined_types.Add ("short", typeof (short));
			xs_predefined_types.Add ("byte", typeof (sbyte));
			xs_predefined_types.Add ("nonNegativeInteger", typeof (decimal));
			xs_predefined_types.Add ("unsignedLong", typeof (ulong));
			xs_predefined_types.Add ("unsignedInt", typeof (uint));
			xs_predefined_types.Add ("unsignedShort", typeof (ushort));
			xs_predefined_types.Add ("unsignedByte", typeof (byte));
			xs_predefined_types.Add ("positiveInteger", typeof (decimal));

			xs_predefined_types.Add ("anyType", typeof (object));
		}

		// FIXME: find out how QName and guid are processed

		internal QName GetXmlName (Type type)
		{
			SerializationMap map = FindUserMap (type);
			if (map != null)
				return map.XmlName;
			return GetPredefinedTypeName (type);
		}

		internal static QName GetPredefinedTypeName (Type type)
		{
			QName name = GetPrimitiveTypeName (type);
			if (name != QName.Empty)
				return name;
			if (type == typeof (DBNull))
				return dbnull_type;
			return QName.Empty;
		}

		internal static QName GetPrimitiveTypeName (Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (Nullable<>))
				return GetPrimitiveTypeName (type.GetGenericArguments () [0]);

			if (type.IsEnum)
				return QName.Empty;

			switch (Type.GetTypeCode (type)) {
			case TypeCode.Object: // other than System.Object
			case TypeCode.DBNull: // it is natively mapped, but not in ms serialization namespace.
			case TypeCode.Empty:
			default:
				if (type == typeof (object))
					return any_type;
				if (type == typeof (Guid))
					return guid_type;
				if (type == typeof (TimeSpan))
					return duration_type;
				if (type == typeof (byte []))
					return base64_type;
				if (type == typeof (Uri))
					return any_uri_type;
				if (type == typeof (DateTimeOffset))
					return date_time_offset_type;
				return QName.Empty;
			case TypeCode.Boolean:
				return bool_type;
			case TypeCode.Byte:
				return ubyte_type;
			case TypeCode.Char:
				return char_type;
			case TypeCode.DateTime:
				return date_type;
			case TypeCode.Decimal:
				return decimal_type;
			case TypeCode.Double:
				return double_type;
			case TypeCode.Int16:
				return short_type;
			case TypeCode.Int32:
				return int_type;
			case TypeCode.Int64:
				return long_type;
			case TypeCode.SByte:
				return byte_type;
			case TypeCode.Single:
				return float_type;
			case TypeCode.String:
				return string_type;
			case TypeCode.UInt16:
				return ushort_type;
			case TypeCode.UInt32:
				return uint_type;
			case TypeCode.UInt64:
				return ulong_type;
			}
		}

		internal static string PredefinedTypeObjectToString (object obj)
		{
			Type type = obj.GetType ();
			switch (Type.GetTypeCode (type)) {
			case TypeCode.Object: // other than System.Object
			case TypeCode.Empty:
			default:
				if (type == typeof (object))
					return String.Empty;
				if (type == typeof (Guid))
					return XmlConvert.ToString ((Guid) obj);
				if (type == typeof (TimeSpan))
					return XmlConvert.ToString ((TimeSpan) obj);
				if (type == typeof (byte []))
					return Convert.ToBase64String ((byte []) obj);
				if (type == typeof (Uri))
					return ((Uri) obj).ToString ();
				throw new Exception ("Internal error: missing predefined type serialization for type " + type.FullName);
			case TypeCode.DBNull: // predefined, but not primitive
				return String.Empty;
			case TypeCode.Boolean:
				return XmlConvert.ToString ((bool) obj);
			case TypeCode.Byte:
				return XmlConvert.ToString ((int)((byte) obj));
			case TypeCode.Char:
				return XmlConvert.ToString ((uint) (char) obj);
			case TypeCode.DateTime:
				return XmlConvert.ToString ((DateTime) obj, XmlDateTimeSerializationMode.RoundtripKind);
			case TypeCode.Decimal:
				return XmlConvert.ToString ((decimal) obj);
			case TypeCode.Double:
				return XmlConvert.ToString ((double) obj);
			case TypeCode.Int16:
				return XmlConvert.ToString ((short) obj);
			case TypeCode.Int32:
				return XmlConvert.ToString ((int) obj);
			case TypeCode.Int64:
				return XmlConvert.ToString ((long) obj);
			case TypeCode.SByte:
				return XmlConvert.ToString ((sbyte) obj);
			case TypeCode.Single:
				return XmlConvert.ToString ((float) obj);
			case TypeCode.String:
				return (string) obj;
			case TypeCode.UInt16:
				return XmlConvert.ToString ((int) (ushort) obj);
			case TypeCode.UInt32:
				return XmlConvert.ToString ((uint) obj);
			case TypeCode.UInt64:
				return XmlConvert.ToString ((ulong) obj);
			}
		}

		internal static Type GetPrimitiveTypeFromName (QName name)
		{
			switch (name.Namespace) {
			case DefaultClrNamespaceSystem:
				switch (name.Name) {
				case "DBNull":
					return typeof (DBNull);
				case "DateTimeOffset":
					return typeof (DateTimeOffset);
				}
				break;
			case XmlSchema.Namespace:
				return xs_predefined_types.FirstOrDefault (p => p.Key == name.Name).Value;
			case MSSimpleNamespace:
				switch (name.Name) {
				case "anyURI":
					return typeof (Uri);
				case "boolean":
					return typeof (bool);
				case "base64Binary":
					return typeof (byte []);
				case "dateTime":
					return typeof (DateTime);
				case "duration":
					return typeof (TimeSpan);
				case "QName":
					return typeof (QName);
				case "decimal":
					return typeof (decimal);
				case "double":
					return typeof (double);
				case "float":
					return typeof (float);
				case "byte":
					return typeof (sbyte);
				case "short":
					return typeof (short);
				case "int":
					return typeof (int);
				case "long":
					return typeof (long);
				case "unsignedByte":
					return typeof (byte);
				case "unsignedShort":
					return typeof (ushort);
				case "unsignedInt":
					return typeof (uint);
				case "unsignedLong":
					return typeof (ulong);
				case "string":
					return typeof (string);
				case "anyType":
					return typeof (object);
				case "guid":
					return typeof (Guid);
				case "char":
					return typeof (char);
				}
				break;
			}
			return null;
		}


		internal static object PredefinedTypeStringToObject (string s,
			string name, XmlReader reader)
		{
			switch (name) {
			case "anyURI":
				return new Uri(s,UriKind.RelativeOrAbsolute);
			case "boolean":
				return XmlConvert.ToBoolean (s);
			case "base64Binary":
				return Convert.FromBase64String (s);
			case "dateTime":
				return XmlConvert.ToDateTime (s, XmlDateTimeSerializationMode.RoundtripKind);
			case "duration":
				return XmlConvert.ToTimeSpan (s);
			case "QName":
				int idx = s.IndexOf (':');
				string l = idx < 0 ? s : s.Substring (idx + 1);
				return idx < 0 ? new QName (l) :
					new QName (l, reader.LookupNamespace (
						s.Substring (0, idx)));
			case "decimal":
				return XmlConvert.ToDecimal (s);
			case "double":
				return XmlConvert.ToDouble (s);
			case "float":
				return XmlConvert.ToSingle (s);
			case "byte":
				return XmlConvert.ToSByte (s);
			case "short":
				return XmlConvert.ToInt16 (s);
			case "int":
				return XmlConvert.ToInt32 (s);
			case "long":
				return XmlConvert.ToInt64 (s);
			case "unsignedByte":
				return XmlConvert.ToByte (s);
			case "unsignedShort":
				return XmlConvert.ToUInt16 (s);
			case "unsignedInt":
				return XmlConvert.ToUInt32 (s);
			case "unsignedLong":
				return XmlConvert.ToUInt64 (s);
			case "string":
				return s;
			case "guid":
				return XmlConvert.ToGuid (s);
			case "anyType":
				return s;
			case "char":
				return (char) XmlConvert.ToUInt32 (s);
			default:
				throw new Exception ("Unanticipated primitive type: " + name);
			}
		}

		List<SerializationMap> contracts = new List<SerializationMap> ();

		public KnownTypeCollection ()
		{
		}

		protected override void ClearItems ()
		{
			base.Clear ();
		}

		protected override void InsertItem (int index, Type type)
		{
			if (ShouldNotRegister (type))
				return;
			if (!Contains (type)) {
				TryRegister (type);
				base.InsertItem (index, type);
			}
		}

		// FIXME: it could remove other types' dependencies.
		protected override void RemoveItem (int index)
		{
			lock (this)
				DoRemoveItem (index);
		}

		void DoRemoveItem (int index)
		{
			Type t = base [index];
			List<SerializationMap> l = new List<SerializationMap> ();
			foreach (SerializationMap m in contracts) {
				if (m.RuntimeType == t)
					l.Add (m);
			}
			foreach (SerializationMap m in l) {
				contracts.Remove (m);
				base.RemoveItem (index);
			}
		}

		protected override void SetItem (int index, Type type)
		{
			if (ShouldNotRegister (type))
				return;

			// Since this collection is not assured to be ordered, it ignores the whole Set operation if the type already exists.
			if (Contains (type))
				return;

			if (index != Count)
				RemoveItem (index);
			if (TryRegister (type))
				base.InsertItem (index - 1, type);
		}

		internal SerializationMap FindUserMap (Type type)
		{
			lock (this) {
				for (int i = 0; i < contracts.Count; i++)
					if (type == contracts [i].RuntimeType)
						return contracts [i];
				return null;
			}
		}

		internal SerializationMap FindUserMap (QName qname)
		{
			lock (this)
				return contracts.FirstOrDefault (c => c.XmlName == qname);
		}

		internal SerializationMap FindUserMap (QName qname, Type type)
		{
			lock (this)
				return contracts.FirstOrDefault (c => c.XmlName == qname && c.RuntimeType == type);
		}

		internal Type GetSerializedType (Type type)
		{
			if (IsPrimitiveNotEnum (type))
				return type;
			Type element = GetCollectionElementType (type);
			if (element == null)
				return type;
			QName name = GetQName (type);
			var map = FindUserMap (name, type);
			if (map != null)
				return map.RuntimeType;
			return type;
		}

		internal QName GetQName (Type type)
		{
			SerializationMap map = FindUserMap (type);
			if (map != null)
				// already mapped.
				return map.XmlName;
			return GetStaticQName (type);
		}

		public static QName GetStaticQName (Type type)
		{
			if (IsPrimitiveNotEnum (type))
				return GetPrimitiveTypeName (type);

			if (type.IsEnum)
				return GetEnumQName (type);

			QName qname = GetContractQName (type);
			if (qname != null)
				return qname;

			if (type.GetInterface ("System.Xml.Serialization.IXmlSerializable") != null)
				//FIXME: Reusing GetSerializableQName here, since we just
				//need name of the type..
				return GetSerializableQName (type);

			qname = GetCollectionContractQName (type);
			if (qname != null)
				return qname;

			Type element = GetCollectionElementType (type);
			if (element != null) {
				if (type.IsInterface || IsCustomCollectionType (type, element))
					return GetCollectionQName (element);
			}

			if (GetAttribute<SerializableAttribute> (type) != null)
				return GetSerializableQName (type);

			// default type map - still uses GetContractQName().
			return GetContractQName (type, null, null);
		}

		internal static QName GetContractQName (Type type)
		{
			var a = GetAttribute<DataContractAttribute> (type);
			return a == null ? null : GetContractQName (type, a.Name, a.Namespace);
		}

		static QName GetCollectionContractQName (Type type)
		{
			var a = GetAttribute<CollectionDataContractAttribute> (type);
			return a == null ? null : GetContractQName (type, a.Name, a.Namespace);
		}

		static QName GetContractQName (Type type, string name, string ns)
		{
			if (name == null)
				name = GetDefaultName (type);
			else if (type.IsGenericType) {
				var args = type.GetGenericArguments ();
				for (int i = 0; i < args.Length; i++)
					name = name.Replace ("{" + i + "}", GetStaticQName (args [i]).Name);
			}

			if (ns == null)
				ns = GetDefaultNamespace (type);
			return new QName (name, ns);
		}

		static QName GetEnumQName (Type type)
		{
			string name = null, ns = null;

			if (!type.IsEnum)
				return null;

			var dca = GetAttribute<DataContractAttribute> (type);

			if (dca != null) {
				ns = dca.Namespace;
				name = dca.Name;
			}

			if (ns == null)
				ns = GetDefaultNamespace (type);

			if (name == null)
				name = type.Namespace == null ? type.Name : type.FullName.Substring (type.Namespace.Length + 1).Replace ('+', '.');

			return new QName (name, ns);
		}

		internal static string GetDefaultName (Type type)
		{
			// FIXME: there could be decent ways to get
			// the same result...
			string name = type.Namespace == null || type.Namespace.Length == 0 ? type.Name : type.FullName.Substring (type.Namespace.Length + 1).Replace ('+', '.');
			if (type.IsGenericType) {
				name = name.Substring (0, name.IndexOf ('`')) + "Of";
				foreach (var t in type.GetGenericArguments ())
					name += t.Name; // FIXME: check namespaces too
			}
			return name;
		}

		internal static string GetDefaultNamespace (Type type)
		{
			foreach (ContractNamespaceAttribute a in type.Assembly.GetCustomAttributes (typeof (ContractNamespaceAttribute), true))
				if (a.ClrNamespace == type.Namespace)
					return a.ContractNamespace;
			return DefaultClrNamespaceBase + type.Namespace;
		}

		static QName GetCollectionQName (Type element)
		{
			QName eqname = GetStaticQName (element);

			string ns = eqname.Namespace;
			if (eqname.Namespace == MSSimpleNamespace)
				//Arrays of Primitive types
				ns = MSArraysNamespace;

			return new QName (
				"ArrayOf" + XmlConvert.EncodeLocalName (eqname.Name),
				ns);
		}

		static QName GetSerializableQName (Type type)
		{
			// First, check XmlSchemaProviderAttribute and try GetSchema() to see if it returns a schema in the expected format.
			var xpa = type.GetCustomAttribute<XmlSchemaProviderAttribute> (true);
			if (xpa != null) {
				if (xpa.IsAny)
					return XmlQualifiedName.Empty;
				var mi = type.GetMethod (xpa.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				if (mi != null) {
					try {
						var xss = new XmlSchemaSet ();
						return (XmlQualifiedName) mi.Invoke (null, new object [] {xss});
					} catch {
						// ignore.
					}
				}
			}

			string xmlName = type.Name;
			if (type.IsGenericType) {
				xmlName = xmlName.Substring (0, xmlName.IndexOf ('`')) + "Of";
				foreach (var t in type.GetGenericArguments ())
					xmlName += GetStaticQName (t).Name; // FIXME: check namespaces too
			}
			string xmlNamespace = GetDefaultNamespace (type);
			var x = GetAttribute<XmlRootAttribute> (type);
			if (x != null) {
				xmlName = x.ElementName;
				xmlNamespace = x.Namespace;
			}
			return new QName (XmlConvert.EncodeLocalName (xmlName), xmlNamespace);
		}

		static bool IsPrimitiveNotEnum (Type type)
		{
			if (type.IsEnum)
				return false;
			if (Type.GetTypeCode (type) != TypeCode.Object) // explicitly primitive
				return true;
			if (type == typeof (Guid) || type == typeof (object) || type == typeof(TimeSpan) || type == typeof(byte[]) || type == typeof(Uri) || type == typeof(DateTimeOffset)) // special primitives
				return true;
			// DOM nodes
			if (type == typeof (XmlElement) || type == typeof (XmlNode []))
				return true;
			// nullable
			if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (Nullable<>))
				return IsPrimitiveNotEnum (type.GetGenericArguments () [0]);
			return false;
		}

		bool ShouldNotRegister (Type type)
		{
			return IsPrimitiveNotEnum (type);
		}

		internal bool TryRegister (Type type)
		{
			lock (this) {
				return DoTryRegister (type);
			}
		}

		bool DoTryRegister (Type type)
		{
			// exclude predefined maps
			if (ShouldNotRegister (type))
				return false;

			if (FindUserMap (type) != null)
				return false;

			if (RegisterEnum (type) != null)
				return true;

			if (RegisterDictionary (type) != null)
				return true;

			if (RegisterCollectionContract (type) != null)
				return true;

			if (RegisterContract (type) != null)
				return true;

			if (RegisterIXmlSerializable (type) != null)
				return true;

			if (RegisterCollection (type) != null)
				return true;

			if (GetAttribute<SerializableAttribute> (type) != null) {
				RegisterSerializable (type);
				return true;
			}

			RegisterDefaultTypeMap (type);
			return true;
		}

		static Type GetCollectionElementType (Type type)
		{
			if (type.IsArray)
				return type.GetElementType ();
			var ifaces = type.GetInterfacesOrSelfInterface ();
			foreach (Type i in ifaces)
				if (i.IsGenericType && i.GetGenericTypeDefinition ().Equals (typeof (IEnumerable<>)))
					return i.GetGenericArguments () [0];
			foreach (Type i in ifaces)
				if (i == typeof (IEnumerable))
					return typeof (object);
			return null;
		}

		internal static T GetAttribute<T> (ICustomAttributeProvider ap) where T : Attribute
		{
			object [] atts = ap.GetCustomAttributes (typeof (T), false);
			return atts.Length == 0 ? null : (T) atts [0];
		}

		private CollectionContractTypeMap RegisterCollectionContract (Type type)
		{
			var cdca = GetAttribute<CollectionDataContractAttribute> (type);
			if (cdca == null)
				return null;

			Type element = GetCollectionElementType (type);
			if (element == null)
				throw new InvalidDataContractException (String.Format ("Type '{0}' is marked as collection contract, but it is not a collection", type));
			if (type.GetMethod ("Add", new Type[] { element }) == null)
				throw new InvalidDataContractException (String.Format ("Type '{0}' is marked as collection contract, but missing a public \"Add\" method", type));

			TryRegister (element); // must be registered before the name conflict check.

			QName qname = GetCollectionContractQName (type);
			CheckStandardQName (qname);
			var map = FindUserMap (qname, type);
			if (map != null) {
				var cmap = map as CollectionContractTypeMap;
				if (cmap == null) // The runtime type may still differ (between array and other IList; see bug #670560)
					throw new InvalidOperationException (String.Format ("Failed to add type {0} to known type collection. There already is a registered type for XML name {1}", type, qname));
			}

			var ret = new CollectionContractTypeMap (type, cdca, element, qname, this);
			contracts.Add (ret);
			return ret;
		}

		private CollectionTypeMap RegisterCollection (Type type)
		{
			Type element = GetCollectionElementType (type);
			if (element == null)
				return null;

			TryRegister (element);

			/*
			 * To qualify as a custom collection type, a type must have
			 * a public parameterless constructor and an "Add" method
			 * with the correct parameter type in addition to implementing
			 * one of the collection interfaces.
			 * 
			 */

			if (!type.IsArray && type.IsClass && !IsCustomCollectionType (type, element))
				return null;

			QName qname = GetCollectionQName (element);

			var map = FindUserMap (qname, element);
			if (map != null) {
				var cmap = map as CollectionTypeMap;
				if (cmap == null) // The runtime type may still differ (between array and other IList; see bug #670560)
					throw new InvalidOperationException (String.Format ("Failed to add type {0} to known type collection. There already is a registered type for XML name {1}", type, qname));
				return cmap;
			}

			CollectionTypeMap ret =
				new CollectionTypeMap (type, element, qname, this);
			contracts.Add (ret);
			return ret;
		}

		static bool IsCustomCollectionType (Type type, Type elementType)
		{
			if (!type.IsClass)
				return false;
			if (type.GetConstructor (new Type [0]) == null)
				return false;
			if (type.GetMethod ("Add", new Type[] { elementType }) == null)
				return false;

			return true;
		}

		internal static bool IsInterchangeableCollectionType (Type contractType, Type graphType,
		                                                      out QName collectionQName)
		{
			collectionQName = null;
			if (GetAttribute<CollectionDataContractAttribute> (contractType) != null)
				return false;

			var type = contractType;
			if (type.IsGenericType)
				type = type.GetGenericTypeDefinition ();

			var elementType = GetCollectionElementType (contractType);
			if (elementType == null)
				return false;
			
			if (contractType.IsArray) {
				if (!graphType.IsArray || !elementType.Equals (graphType.GetElementType ()))
					throw new InvalidCastException (String.Format ("Type '{0}' cannot be converted into '{1}'.", graphType.GetElementType (), elementType));
			} else if (!contractType.IsInterface) {
				if (GetAttribute<SerializableAttribute> (contractType) == null)
					return false;

				var graphElementType = GetCollectionElementType (graphType);
				if (elementType != graphElementType)
					return false;

				if (!IsCustomCollectionType (contractType, elementType))
					return false;
			} else if (type.Equals (typeof (IEnumerable)) || type.Equals (typeof (IList)) ||
			           type.Equals (typeof (ICollection))) {
				if (!graphType.ImplementsInterface (contractType))
					return false;
			} else if (type.Equals (typeof (IEnumerable<>)) || type.Equals (typeof (IList<>)) ||
			           type.Equals (typeof (ICollection<>))) {
				var graphElementType = GetCollectionElementType (graphType);
				if (graphElementType != elementType)
					throw new InvalidCastException (String.Format (
						"Cannot convert type '{0}' into '{1}'.", graphType, contractType));

				if (!graphType.ImplementsInterface (contractType))
					return false;
			} else {
				return false;
			}

			collectionQName = GetCollectionQName (elementType);
			return true;
		}

		static bool ImplementsInterface (Type type, Type iface)
		{
			foreach (var i in type.GetInterfacesOrSelfInterface ())
				if (iface == i)
					return true;
					
			return false;
		}


		static bool TypeImplementsIEnumerable (Type type)
		{
			foreach (var iface in type.GetInterfacesOrSelfInterface ())
				if (iface == typeof (IEnumerable) || (iface.IsGenericType && iface.GetGenericTypeDefinition () == typeof (IEnumerable<>)))
					return true;
			
			return false;
		}

		static bool TypeImplementsIDictionary (Type type)
		{
			foreach (var iface in type.GetInterfacesOrSelfInterface ())
				if (iface == typeof (IDictionary) || (iface.IsGenericType && iface.GetGenericTypeDefinition () == typeof (IDictionary<,>)))
					return true;

			return false;
		}

		// it also supports contract-based dictionary.
		private DictionaryTypeMap RegisterDictionary (Type type)
		{
			if (!TypeImplementsIDictionary (type))
				return null;

			var cdca = GetAttribute<CollectionDataContractAttribute> (type);

			DictionaryTypeMap ret =
				new DictionaryTypeMap (type, cdca, this);

			TryRegister (ret.KeyType);
			TryRegister (ret.ValueType);

			var map = FindUserMap (ret.XmlName, type);
			if (map != null) {
				var dmap = map as DictionaryTypeMap;
				if (dmap == null) // The runtime type may still differ (between array and other IList; see bug #670560)
					throw new InvalidOperationException (String.Format ("Failed to add type {0} to known type collection. There already is a registered type for XML name {1}", type, ret.XmlName));
			}
			contracts.Add (ret);

			return ret;
		}

		private SerializationMap RegisterSerializable (Type type)
		{
			QName qname = GetSerializableQName (type);

			if (FindUserMap (qname, type) != null)
				throw new InvalidOperationException (String.Format ("There is already a registered type for XML name {0}", qname));

			SharedTypeMap ret = new SharedTypeMap (type, qname, this);
			contracts.Add (ret);
			ret.Initialize ();
			return ret;
		}

		private SerializationMap RegisterIXmlSerializable (Type type)
		{
			if (type.GetInterface ("System.Xml.Serialization.IXmlSerializable") == null)
				return null;

			QName qname = GetSerializableQName (type);

			if (!QName.Empty.Equals (qname) && FindUserMap (qname, type) != null)
				throw new InvalidOperationException (String.Format ("There is already a registered type for XML name {0}", qname));

			XmlSerializableMap ret = new XmlSerializableMap (type, qname, this);
			contracts.Add (ret);

			return ret;
		}

		void CheckStandardQName (QName qname)
		{
			switch (qname.Namespace) {
			case XmlSchema.Namespace:
			case XmlSchema.InstanceNamespace:
			case MSSimpleNamespace:
			case MSArraysNamespace:
				throw new InvalidOperationException (String.Format ("Namespace {0} is reserved and cannot be used for user serialization", qname.Namespace));
			}

		}

		private SharedContractMap RegisterContract (Type type)
		{
			QName qname = GetContractQName (type);
			if (qname == null)
				return null;
			CheckStandardQName (qname);
			if (FindUserMap (qname, type) != null)
				throw new InvalidOperationException (String.Format ("There is already a registered type for XML name {0}", qname));

			SharedContractMap ret = new SharedContractMap (type, qname, this);
			contracts.Add (ret);
			ret.Initialize ();

			if (type.BaseType != typeof (object)) {
				TryRegister (type.BaseType);
				if (!FindUserMap (type.BaseType).IsContractAllowedType)
					throw new InvalidDataContractException (String.Format ("To be serializable by data contract, type '{0}' cannot inherit from non-contract and non-Serializable type '{1}'", type, type.BaseType));
			}

			object [] attrs = type.GetCustomAttributes (typeof (KnownTypeAttribute), true);
			for (int i = 0; i < attrs.Length; i++) {
				KnownTypeAttribute kt = (KnownTypeAttribute) attrs [i];
				foreach (var t in kt.GetTypes (type))
					TryRegister (t);
			}

			return ret;
		}

		DefaultTypeMap RegisterDefaultTypeMap (Type type)
		{
			DefaultTypeMap ret = new DefaultTypeMap (type, this);
			contracts.Add (ret);
			ret.Initialize ();
			return ret;
		}

		private EnumMap RegisterEnum (Type type)
		{
			QName qname = GetEnumQName (type);
			if (qname == null)
				return null;

			if (FindUserMap (qname, type) != null)
				throw new InvalidOperationException (String.Format ("There is already a registered type for XML name {0}", qname));

			EnumMap ret =
				new EnumMap (type, qname, this);
			contracts.Add (ret);
			return ret;
		}
	}
}
#endif
