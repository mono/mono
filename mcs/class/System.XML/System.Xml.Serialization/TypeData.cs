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
using System.Reflection;

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
		bool hasPublicConstructor = true;

		public TypeData (Type type, string elementName, bool isPrimitive)
		{
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
				hasPublicConstructor = (type.IsArray || type.GetConstructor (Type.EmptyTypes) != null || type.IsAbstract || type.IsValueType);
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
						  SchemaType == SchemaTypes.XmlSerializable ); 
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
					PropertyInfo prop = GetIndexerProperty (type);
					if (prop == null) 
						throw new InvalidOperationException ("You must implement a default accessor on " + type.FullName + " because it inherits from ICollection");
						
					return prop.PropertyType;
				}
				else
				{
					MethodInfo met = type.GetMethod ("Add");
					if (met == null)
						throw new InvalidOperationException ("The collection " + type.FullName + " must implement an Add method");

					ParameterInfo[] pars = met.GetParameters();
					if (pars.Length != 1)
						throw new InvalidOperationException ("The Add method of the collection " + type.FullName + " must have only one parameter");
					
					return pars[0].ParameterType;
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
	}
}
