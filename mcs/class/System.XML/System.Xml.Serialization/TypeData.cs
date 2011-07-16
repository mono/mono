//
// System.Xml.Serialization.TypeData
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//  Lluis Sanchez Gual (lluis@ximian.com)
//  Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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
#if NET_2_0
using System.Collections.Generic;
#endif
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml.Schema;

#if MOONLIGHT
using XmlSchemaPatternFacet = System.Object;
#endif

namespace System.Xml.Serialization
{
	internal class TypeData
	{
		Type type;
		string elementName;
		SchemaTypes sType;
		Type listItemType;
		string typeName;
		string fullTypeName;
		string csharpName;
		string csharpFullName;
		TypeData listItemTypeData;
		TypeData listTypeData;
		TypeData mappedType;
		XmlSchemaPatternFacet facet;
		bool hasPublicConstructor = true;
		bool nullableOverride;

		public TypeData (Type type, string elementName, bool isPrimitive) :
			this(type, elementName, isPrimitive, null, null) {}

		public TypeData (Type type, string elementName, bool isPrimitive, TypeData mappedType, XmlSchemaPatternFacet facet)
		{
#if NET_2_0
			if (type.IsGenericTypeDefinition)
				throw new InvalidOperationException ("Generic type definition cannot be used in serialization. Only specific generic types can be used.");
#endif
			this.mappedType = mappedType;
			this.facet = facet;
			this.type = type;
			this.typeName = type.Name;
			this.fullTypeName = type.FullName.Replace ('+', '.');

			if (isPrimitive)
				sType = SchemaTypes.Primitive;
			else
			{
				if (type.IsEnum)
					sType = SchemaTypes.Enum;
				else if (typeof(IXmlSerializable).IsAssignableFrom (type))
					sType = SchemaTypes.XmlSerializable;
#if !MOONLIGHT
				else if (typeof (System.Xml.XmlNode).IsAssignableFrom (type))
					sType = SchemaTypes.XmlNode;
#endif
				else if (type.IsArray || typeof(IEnumerable).IsAssignableFrom (type))
					sType = SchemaTypes.Array;
				else
					sType = SchemaTypes.Class;
			}
			
			if (IsListType)
				this.elementName = TypeTranslator.GetArrayName (ListItemTypeData.XmlType);
			else
				this.elementName = elementName;

			if (sType == SchemaTypes.Array || sType == SchemaTypes.Class) {
				hasPublicConstructor = !type.IsInterface && (type.IsArray || type.GetConstructor (Type.EmptyTypes) != null || type.IsAbstract || type.IsValueType);
			}
		}

		internal TypeData (string typeName, string fullTypeName, string xmlType, SchemaTypes schemaType, TypeData listItemTypeData)
		{
			this.elementName = xmlType;
			this.typeName = typeName;
			this.fullTypeName = fullTypeName.Replace ('+', '.');
			this.listItemTypeData = listItemTypeData;
			this.sType = schemaType;
			this.hasPublicConstructor = true;
		}

		public string TypeName
		{
			get {
				return typeName;
			}
		}
				
		public string XmlType
		{
			get {
				return elementName;
			}
		}
				
		public Type Type
		{
			get {
				return type;
			}
		}
				
		public string FullTypeName
		{
			get {
				return fullTypeName;
			}
		}

		public string CSharpName
		{
			get {
				if (csharpName == null)
					csharpName = (Type == null) ? TypeName : ToCSharpName (Type, false);
				return csharpName;
			}
		}

		public string CSharpFullName
		{
			get {
				if (csharpFullName == null)
					csharpFullName = (Type == null) ? TypeName : ToCSharpName (Type, true);
				return csharpFullName;
			}
		}

		// static Microsoft.CSharp.CSharpCodeProvider csprovider =
		//	new Microsoft.CSharp.CSharpCodeProvider ();

		public static string ToCSharpName (Type type, bool full)
		{
			// return csprovider.GetTypeOutput (new System.CodeDom.CodeTypeReference (type));
			StringBuilder sb = new StringBuilder ();
			
			if (type.IsArray) {
				sb.Append (ToCSharpName (type.GetElementType (), full));
				sb.Append ('[');
				int rank = type.GetArrayRank ();
				for (int i = 1; i < rank; i++)
					sb.Append (',');
				sb.Append (']');
				return sb.ToString ();
			}
#if NET_2_0
			// Generic nested types return the complete list of type arguments,
			// including type arguments for the declaring class. This requires
			// some special handling
			if (type.IsGenericType && !type.IsGenericTypeDefinition) {
				Type[] args = type.GetGenericArguments ();
				int nt = args.Length - 1;
				Type pt = type;
				// Loop throguh the declaring class chain, consuming type arguments for every
				// generic class in the chain
				while (pt != null) {
					if (sb.Length > 0)
						sb.Insert (0, '.');
					int i = pt.Name.IndexOf ('`');
					if (i != -1) {
						int na = nt - int.Parse (pt.Name.Substring (i+1));
						sb.Insert (0,'>');
						for (;nt > na; nt--) {
							sb.Insert (0, ToCSharpName (args[nt],full));
							if (nt - 1 != na)
								sb.Insert (0, ',');
						}
						sb.Insert (0,'<');
						sb.Insert (0, pt.Name.Substring (0, i));
					} else
						sb.Insert (0, pt.Name);
					pt = pt.DeclaringType;
				}
				if (full && type.Namespace.Length > 0)
					sb.Insert (0, type.Namespace + ".");
				return sb.ToString ();
			}
#endif
			if (type.DeclaringType != null) {
				sb.Append (ToCSharpName (type.DeclaringType, full)).Append ('.');
				sb.Append (type.Name);
			}
			else {
				if (full && type.Namespace.Length > 0)
					sb.Append (type.Namespace).Append ('.');
				sb.Append (type.Name);
			}
			return sb.ToString ();
		}
		
		static bool IsKeyword (string name)
		{
			if (keywordsTable == null) {
				Hashtable t = new Hashtable ();
				foreach (string s in keywords)
					t [s] = s;
				keywordsTable = t;
			}
			return keywordsTable.Contains (name);
		}

		public SchemaTypes SchemaType
		{
			get {
				return sType;
			}
		}

		public bool IsListType
		{
			get { return SchemaType == SchemaTypes.Array; }
		}

		public bool IsComplexType
		{
			get 
			{ 
				return (SchemaType == SchemaTypes.Class || 
					      SchemaType == SchemaTypes.Array ||
					      SchemaType == SchemaTypes.Enum ||
					      SchemaType == SchemaTypes.XmlNode ||
						  SchemaType == SchemaTypes.XmlSerializable ||
						  !IsXsdType); 
			}
		}

		public bool IsValueType
		{
			get
			{
				if (type != null) return type.IsValueType;
				else return (sType == SchemaTypes.Primitive || sType == SchemaTypes.Enum);
			}
		}

		public bool NullableOverride
		{
			get { return nullableOverride; }
		}

		public bool IsNullable
		{
			get
			{
				if (nullableOverride)
					return true;
#if NET_2_0
				return !IsValueType ||
					(type != null &&
					 type.IsGenericType &&
					 type.GetGenericTypeDefinition () == typeof (Nullable<>));
#else
				return !IsValueType;
#endif
			}

			set
			{
				nullableOverride = value;
			}
		}

		public TypeData ListItemTypeData
		{
			get
			{
				if (listItemTypeData == null && type != null)
					listItemTypeData = TypeTranslator.GetTypeData (ListItemType);
				return listItemTypeData;
			}
		}
		
		public Type ListItemType
		{
			get
			{
				if (type == null) 
					throw new InvalidOperationException ("Property ListItemType is not supported for custom types");

				if (listItemType != null) return listItemType;

				Type genericArgument = null;

				if (SchemaType != SchemaTypes.Array)
					throw new InvalidOperationException (Type.FullName + " is not a collection");
				else if (type.IsArray) 
					listItemType = type.GetElementType ();
#if NET_2_0
				else if (typeof (ICollection).IsAssignableFrom (type) || (genericArgument = GetGenericListItemType (type)) != null)
#else
				else if (typeof (ICollection).IsAssignableFrom (type))
#endif
				{
					if (typeof (IDictionary).IsAssignableFrom (type))
						throw new NotSupportedException (string.Format (CultureInfo.InvariantCulture,
							"The type {0} is not supported because it implements" +
							" IDictionary.", type.FullName));

					if (genericArgument != null)
						listItemType = genericArgument;
					else {
						PropertyInfo prop = GetIndexerProperty (type);
						if (prop == null) 
							throw new InvalidOperationException ("You must implement a default accessor on " + type.FullName + " because it inherits from ICollection");
						listItemType = prop.PropertyType;
					}

					MethodInfo addMethod = type.GetMethod ("Add", new Type[] { listItemType });
					if (addMethod == null)
						throw CreateMissingAddMethodException (type, "ICollection",
							listItemType);
				}
				else // at this point, we must be dealing with IEnumerable implementation
				{
					MethodInfo met = type.GetMethod ("GetEnumerator", Type.EmptyTypes);
					if (met == null) { 
						// get private implemenation
						met = type.GetMethod ("System.Collections.IEnumerable.GetEnumerator",
							BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
							null, Type.EmptyTypes, null);
					}
					// determine ListItemType using IEnumerator.Current property
					PropertyInfo prop = met.ReturnType.GetProperty ("Current");
					if (prop == null)
						listItemType = typeof (object);
					else
						listItemType = prop.PropertyType;

					MethodInfo addMethod = type.GetMethod ("Add", new Type[] { listItemType });
					if (addMethod == null)
						throw CreateMissingAddMethodException (type, "IEnumerable",
							listItemType);
				}

				return listItemType;
			}
		}

		public TypeData ListTypeData
		{
			get
			{
				if (listTypeData != null) return listTypeData;
				
				listTypeData = new TypeData (TypeName + "[]",
					FullTypeName + "[]",
					TypeTranslator.GetArrayName(XmlType),
					SchemaTypes.Array, this);

				return listTypeData;
			}
		}

		public bool IsXsdType {
			get { return mappedType == null; }
		}

		public TypeData MappedType {
			get {
				return mappedType != null ? mappedType : this;
			}
		}

		public XmlSchemaPatternFacet XmlSchemaPatternFacet {
			get {
				return facet;
			}
		}
		
		public bool HasPublicConstructor
		{
			get { return hasPublicConstructor; }
		}


		public static PropertyInfo GetIndexerProperty (Type collectionType)
		{
			PropertyInfo[] props = collectionType.GetProperties (BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo prop in props)
			{
				ParameterInfo[] pi = prop.GetIndexParameters ();
				if (pi != null && pi.Length == 1 && pi[0].ParameterType == typeof(int))
					return prop;
			}
			return null;
		}

		private static InvalidOperationException CreateMissingAddMethodException (Type type, string inheritFrom, Type argumentType) {
			return new InvalidOperationException (string.Format(CultureInfo.InvariantCulture,
				"To be XML serializable, types which inherit from {0} must have " +
				"an implementation of Add({1}) at all levels of their inheritance " +
				"hierarchy. {2} does not implement Add({1}).", inheritFrom, 
				argumentType.FullName, type.FullName));
		}

		private static Hashtable keywordsTable;
		private static string[] keywords = new string[] {
			"abstract","event","new","struct","as","explicit","null","switch","base","extern",
			"this","false","operator","throw","break","finally","out","true",
			"fixed","override","try","case","params","typeof","catch","for",
			"private","foreach","protected","checked","goto","public",
			"unchecked","class","if","readonly","unsafe","const","implicit","ref",
			"continue","in","return","using","virtual","default",
			"interface","sealed","volatile","delegate","internal","do","is",
			"sizeof","while","lock","stackalloc","else","static","enum",
			"namespace",
			"object","bool","byte","float","uint","char","ulong","ushort",
			"decimal","int","sbyte","short","double","long","string","void",
#if NET_2_0
			"partial", "yield", "where"
#endif
		};

#if NET_2_0
		internal static Type GetGenericListItemType (Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (IEnumerable<>)) {
				Type [] gatypes = type.GetGenericArguments ();
				if (type.GetMethod ("Add", gatypes) != null)
					return gatypes [0];
			}
			Type t = null;
			foreach (Type i in type.GetInterfaces ())
				if ((t = GetGenericListItemType (i)) != null)
					return t;
			return null;
		}
#endif
	}
}
