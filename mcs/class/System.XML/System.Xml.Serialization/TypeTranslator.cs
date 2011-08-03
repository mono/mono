//
// System.Xml.Serialization.TypeTranslator
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Erik LeBel (eriklebel@yahoo.ca)
//  Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Erik Lebel
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
using System.Collections;
using System.Globalization;
using System.Xml.Schema;

namespace System.Xml.Serialization
{
	internal class TypeTranslator
	{
		static Hashtable nameCache;
		static Hashtable primitiveTypes;
		static Hashtable primitiveArrayTypes;
		static Hashtable nullableTypes;

#if TARGET_JVM
		static readonly object AppDomain_TypeTranslatorCacheLock = new object ();
		const string AppDomain_nameCacheName = "System.Xml.Serialization.TypeTranslator.nameCache";
		const string AppDomain_nullableTypesName = "System.Xml.Serialization.TypeTranslator.nullableTypes";
		
		static Hashtable AppDomain_nameCache {
			get { return GetAppDomainCache (AppDomain_nameCacheName); }
		}

		static Hashtable AppDomain_nullableTypes {
			get { return GetAppDomainCache (AppDomain_nullableTypesName); }
		}

		static Hashtable GetAppDomainCache(string name) {
			Hashtable res = (Hashtable) AppDomain.CurrentDomain.GetData (name);

			if (res == null) {
				lock (AppDomain_TypeTranslatorCacheLock) {
					res = (Hashtable) AppDomain.CurrentDomain.GetData (name);
					if (res == null) {
						res = Hashtable.Synchronized (new Hashtable ());
						AppDomain.CurrentDomain.SetData (name, res);
					}
				}
			}

			return res;
		}
#endif

		static TypeTranslator ()
		{
			nameCache = new Hashtable ();
			primitiveArrayTypes = Hashtable.Synchronized (new Hashtable ());

#if !TARGET_JVM
			nameCache = Hashtable.Synchronized (nameCache);
#endif
			// XSD Types with direct map to CLR types

			nameCache.Add (typeof (bool), new TypeData (typeof (bool), "boolean", true));
			nameCache.Add (typeof (short), new TypeData (typeof (short), "short", true));
			nameCache.Add (typeof (ushort), new TypeData (typeof (ushort), "unsignedShort", true));
			nameCache.Add (typeof (int), new TypeData (typeof (int), "int", true));
			nameCache.Add (typeof (uint), new TypeData (typeof (uint), "unsignedInt", true));
			nameCache.Add (typeof (long), new TypeData (typeof (long), "long", true));
			nameCache.Add (typeof (ulong), new TypeData (typeof (ulong), "unsignedLong", true));
			nameCache.Add (typeof (float), new TypeData (typeof (float), "float", true));
			nameCache.Add (typeof (double), new TypeData (typeof (double), "double", true));
			nameCache.Add (typeof (DateTime), new TypeData (typeof (DateTime), "dateTime", true));	// TODO: timeInstant, Xml date, xml time
			nameCache.Add (typeof (decimal), new TypeData (typeof (decimal), "decimal", true));
			nameCache.Add (typeof (XmlQualifiedName), new TypeData (typeof (XmlQualifiedName), "QName", true));
			nameCache.Add (typeof (string), new TypeData (typeof (string), "string", true));
#if !MOONLIGHT
			XmlSchemaPatternFacet guidFacet = new XmlSchemaPatternFacet();
			guidFacet.Value = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
			nameCache.Add (typeof (Guid), new TypeData (typeof (Guid), "guid", true, (TypeData)nameCache[typeof (string)], guidFacet));
#endif
			nameCache.Add (typeof (byte), new TypeData (typeof (byte), "unsignedByte", true));
			nameCache.Add (typeof (sbyte), new TypeData (typeof (sbyte), "byte", true));
			nameCache.Add (typeof (char), new TypeData (typeof (char), "char", true, (TypeData)nameCache[typeof (ushort)], null));
			nameCache.Add (typeof (object), new TypeData (typeof (object), "anyType", false));
			nameCache.Add (typeof (byte[]), new TypeData (typeof (byte[]), "base64Binary", true));
#if !MOONLIGHT
			nameCache.Add (typeof (XmlNode), new TypeData (typeof (XmlNode), "XmlNode", false));
			nameCache.Add (typeof (XmlElement), new TypeData (typeof (XmlElement), "XmlElement", false));
#endif

			primitiveTypes = new Hashtable();
			ICollection types = nameCache.Values;
			foreach (TypeData td in types)
				primitiveTypes.Add (td.XmlType, td);

			// Additional XSD types

			primitiveTypes.Add ("date", new TypeData (typeof (DateTime), "date", true));	// TODO: timeInstant
			primitiveTypes.Add ("time", new TypeData (typeof (DateTime), "time", true));
			primitiveTypes.Add ("timePeriod", new TypeData (typeof (DateTime), "timePeriod", true));
			primitiveTypes.Add ("gDay", new TypeData (typeof (string), "gDay", true));
			primitiveTypes.Add ("gMonthDay", new TypeData (typeof (string), "gMonthDay", true));
			primitiveTypes.Add ("gYear", new TypeData (typeof (string), "gYear", true));
			primitiveTypes.Add ("gYearMonth", new TypeData (typeof (string), "gYearMonth", true));
			primitiveTypes.Add ("month", new TypeData (typeof (DateTime), "month", true));
			primitiveTypes.Add ("NMTOKEN", new TypeData (typeof (string), "NMTOKEN", true));
			primitiveTypes.Add ("NMTOKENS", new TypeData (typeof (string), "NMTOKENS", true));
			primitiveTypes.Add ("Name", new TypeData (typeof (string), "Name", true));
			primitiveTypes.Add ("NCName", new TypeData (typeof (string), "NCName", true));
			primitiveTypes.Add ("language", new TypeData (typeof (string), "language", true));
			primitiveTypes.Add ("integer", new TypeData (typeof (string), "integer", true));
			primitiveTypes.Add ("positiveInteger", new TypeData (typeof (string), "positiveInteger", true));
			primitiveTypes.Add ("nonPositiveInteger", new TypeData (typeof (string), "nonPositiveInteger", true));
			primitiveTypes.Add ("negativeInteger", new TypeData (typeof (string), "negativeInteger", true));
			primitiveTypes.Add ("nonNegativeInteger", new TypeData (typeof (string), "nonNegativeInteger", true));
			primitiveTypes.Add ("ENTITIES", new TypeData (typeof (string), "ENTITIES", true));
			primitiveTypes.Add ("ENTITY", new TypeData (typeof (string), "ENTITY", true));
			primitiveTypes.Add ("hexBinary", new TypeData (typeof (byte[]), "hexBinary", true));
			primitiveTypes.Add ("ID", new TypeData (typeof (string), "ID", true));
			primitiveTypes.Add ("IDREF", new TypeData (typeof (string), "IDREF", true));
			primitiveTypes.Add ("IDREFS", new TypeData (typeof (string), "IDREFS", true));
			primitiveTypes.Add ("NOTATION", new TypeData (typeof (string), "NOTATION", true));
			primitiveTypes.Add ("token", new TypeData (typeof (string), "token", true));
			primitiveTypes.Add ("normalizedString", new TypeData (typeof (string), "normalizedString", true));
			primitiveTypes.Add ("anyURI", new TypeData (typeof (string), "anyURI", true));
			primitiveTypes.Add ("base64", new TypeData (typeof (byte[]), "base64", true));
			primitiveTypes.Add ("duration", new TypeData (typeof (string), "duration", true));

#if NET_2_0
			nullableTypes = Hashtable.Synchronized(new Hashtable ());
			foreach (DictionaryEntry de in primitiveTypes) {
				TypeData td = (TypeData) de.Value;
				TypeData ntd = new TypeData (td.Type, td.XmlType, true);
				ntd.IsNullable = true;
				nullableTypes.Add (de.Key, ntd);
			}
#endif
		}

		public static TypeData GetTypeData (Type type)
		{
			return GetTypeData (type, null);
		}

		public static TypeData GetTypeData (Type runtimeType, string xmlDataType)
		{
			Type type = runtimeType;
			bool nullableOverride = false;
#if NET_2_0
			// Nullable<T> is serialized as T
			if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (Nullable<>)) {
				nullableOverride = true;
				type = type.GetGenericArguments () [0];

				TypeData pt = GetTypeData (type); // beware this recursive call btw ...
				if (pt != null) {
						TypeData tt = (TypeData) nullableTypes [pt.XmlType];
#if TARGET_JVM
						if (tt == null)
							tt = (TypeData) AppDomain_nullableTypes [pt.XmlType];
#endif
						if (tt == null) {
							tt = new TypeData (type, pt.XmlType, false);
							tt.IsNullable = true;
#if TARGET_JVM
							AppDomain_nullableTypes [pt.XmlType] = tt;
#else
							nullableTypes [pt.XmlType] = tt;
#endif
						}
						return tt;
				}
			}
#endif

			if ((xmlDataType != null) && (xmlDataType.Length != 0)) {
				// If the type is an array, xmlDataType specifies the type for the array elements,
				// not for the whole array. The exception is base64Binary, since it is a byte[],
				// that's why the following check is needed.
				TypeData at = GetPrimitiveTypeData (xmlDataType);
				if (type.IsArray && type != at.Type) {
						TypeData tt = (TypeData) primitiveArrayTypes [xmlDataType];
						if (tt != null)
							return tt;
						if (at.Type == type.GetElementType ()) {
							tt = new TypeData (type, GetArrayName (at.XmlType), false);
							primitiveArrayTypes [xmlDataType] = tt;
							return tt;
						}
						else
							throw new InvalidOperationException ("Cannot convert values of type '" + type.GetElementType () + "' to '" + xmlDataType + "'");
				}
				return at;
			}

				TypeData typeData = nameCache[runtimeType] as TypeData;
				if (typeData != null) return typeData;

#if TARGET_JVM
				Hashtable dynamicCache = AppDomain_nameCache;
				typeData = dynamicCache[runtimeType] as TypeData;
				if (typeData != null) return typeData;
#endif

				string name;
				if (type.IsArray) {
					string sufix = GetTypeData (type.GetElementType ()).XmlType;
					name = GetArrayName (sufix);
				}
#if NET_2_0
				else if (type.IsGenericType && !type.IsGenericTypeDefinition) {
					name = XmlConvert.EncodeLocalName (type.Name.Substring (0, type.Name.IndexOf ('`'))) + "Of";
					foreach (Type garg in type.GetGenericArguments ())
						name += garg.IsArray || garg.IsGenericType ?
							GetTypeData (garg).XmlType :
							CodeIdentifier.MakePascal (XmlConvert.EncodeLocalName (garg.Name));
				}
#endif
				else 
					name = XmlConvert.EncodeLocalName (type.Name);

				typeData = new TypeData (type, name, false);
				if (nullableOverride)
					typeData.IsNullable = true;
#if TARGET_JVM
				dynamicCache[runtimeType] = typeData;
#else
				nameCache[runtimeType] = typeData;
#endif
				return typeData;
		}

		public static bool IsPrimitive (Type type)
		{
			return GetTypeData (type).SchemaType == SchemaTypes.Primitive;
		}

		public static TypeData GetPrimitiveTypeData (string typeName)
		{
			return GetPrimitiveTypeData (typeName, false);
		}

		public static TypeData GetPrimitiveTypeData (string typeName, bool nullable)
		{
			TypeData td = (TypeData) primitiveTypes [typeName];
			if (td != null && !td.Type.IsValueType)
				return td;
			// for 1.x profile, 'nullableTypes' is null
			Hashtable table = nullable && nullableTypes != null ? nullableTypes : primitiveTypes;
			td = (TypeData) table [typeName];
			if (td == null) throw new NotSupportedException ("Data type '" + typeName + "' not supported");
			return td;
		}

		public static TypeData FindPrimitiveTypeData (string typeName)
		{
			return (TypeData) primitiveTypes[typeName];
		}

		public static TypeData GetDefaultPrimitiveTypeData (TypeData primType)
		{
			// Returns the TypeData that is mapped by default to the clr type
			// that primType represents
			
			if (primType.SchemaType == SchemaTypes.Primitive)
			{
				TypeData newPrim = GetTypeData (primType.Type, null);
				if (newPrim != primType) return newPrim;
			}
			return primType;
		}

		public static bool IsDefaultPrimitiveTpeData (TypeData primType)
		{
			return GetDefaultPrimitiveTypeData (primType) == primType;
		}

		public static TypeData CreateCustomType (string typeName, string fullTypeName, string xmlType, SchemaTypes schemaType, TypeData listItemTypeData)
		{
			TypeData td = new TypeData (typeName, fullTypeName, xmlType, schemaType, listItemTypeData);
			return td;
		}

		public static string GetArrayName (string elemName)
		{
			return "ArrayOf" + Char.ToUpper (elemName [0], CultureInfo.InvariantCulture) + elemName.Substring (1);
		}
		
		public static string GetArrayName (string elemName, int dimensions)
		{
			string aname = GetArrayName (elemName);
			for ( ; dimensions > 1; dimensions--)
				aname = "ArrayOf" + aname;
			return aname;
		}
		
		public static void ParseArrayType (string arrayType, out string type, out string ns, out string dimensions)
		{
			int i = arrayType.LastIndexOf (":");
			if (i == -1) ns = "";
			else ns = arrayType.Substring (0,i);
			
			int j = arrayType.IndexOf ("[", i+1);
			if (j == -1) throw new InvalidOperationException ("Cannot parse WSDL array type: " + arrayType);
			type = arrayType.Substring (i+1, j-i-1);
			dimensions = arrayType.Substring (j);
		}
	}
}
