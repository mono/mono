//
// System.Xml.Serialization.TypeData
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//  Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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

		public TypeData (Type type, string elementName, bool isPrimitive)
		{
			this.type = type;
			this.elementName = elementName;
			this.typeName = type.Name;
			this.fullTypeName = type.FullName;

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
		}

		public TypeData (string typeName, string fullTypeName, string xmlType, SchemaTypes schemaType, TypeData listItemTypeData)
		{
			this.elementName = xmlType;
			this.typeName = typeName;
			this.fullTypeName = fullTypeName;
			this.listItemTypeData = listItemTypeData;
			this.sType = schemaType;
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
//				return type.FullName.Replace ('+', '.');
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
						  SchemaType == SchemaTypes.XmlSerializable ); 
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
				if (type == null) throw new InvalidOperationException ("Property ListItemType is not supported for custom types");

				if (listItemType != null) return listItemType;

				if (SchemaType != SchemaTypes.Array)
					throw new InvalidOperationException (Type.FullName + " is not a collection");
				else if (type.IsArray) 
					listItemType = type.GetElementType ();
				else if (typeof(ICollection).IsAssignableFrom (type))
				{
					PropertyInfo prop = GetIndexerProperty (type);
					if (prop == null) throw new InvalidOperationException ("You must implement a default accessor on " + type.FullName + " because it inherits from ICollection");
					return prop.PropertyType;
				}
				else
					return type.GetMethod ("Add").GetParameters()[0].ParameterType;

				return listItemType;
			}
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

