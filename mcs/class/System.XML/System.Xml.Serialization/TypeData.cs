//
// System.Xml.Serialization.TypeData
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//  Lluis Sanchez Gual (lluis@ximian.com)
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
using System.Globalization;
using System.Reflection;
using System.Xml.Schema;

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
		TypeData listItemTypeData;
		TypeData listTypeData;
		TypeData mappedType;
		XmlSchemaPatternFacet facet;
		bool hasPublicConstructor = true;

		public TypeData (Type type, string elementName, bool isPrimitive) :
			this(type, elementName, isPrimitive, null, null) {}

		public TypeData (Type type, string elementName, bool isPrimitive, TypeData mappedType, XmlSchemaPatternFacet facet)
		{
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
				else if (typeof (System.Xml.XmlNode).IsAssignableFrom (type))
					sType = SchemaTypes.XmlNode;
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

				if (SchemaType != SchemaTypes.Array)
					throw new InvalidOperationException (Type.FullName + " is not a collection");
				else if (type.IsArray) 
					listItemType = type.GetElementType ();
				else if (typeof(ICollection).IsAssignableFrom (type))
				{
					if (typeof (IDictionary).IsAssignableFrom (type))
						throw new NotSupportedException (string.Format (CultureInfo.InvariantCulture,
							"The type {0} is not supported because it implements" +
							" IDictionary.", type.FullName));

					PropertyInfo prop = GetIndexerProperty (type);
					if (prop == null) 
						throw new InvalidOperationException ("You must implement a default accessor on " + type.FullName + " because it inherits from ICollection");

					listItemType = prop.PropertyType;

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
	}
}
