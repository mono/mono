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

namespace System.Xml.Serialization
{
	internal class TypeData
	{
		Type type;
		string elementName;
		SchemaTypes sType;

		public TypeData (Type type, string elementName, bool isPrimitive)
		{
			this.type = type;
			this.elementName = elementName;

			if (isPrimitive)
				sType = SchemaTypes.Primitive;
			else
			{
				if (type.IsEnum)
					sType = SchemaTypes.Enum;
				/*else if (type == typeof (System.Data.DataSet))
					sType = SchemaTypes.DataSet;*/
				else if (typeof (System.Xml.XmlNode).IsAssignableFrom (type))
					sType = SchemaTypes.XmlNode;
				else if (type.IsArray || type.GetInterface ("IEnumerable") != null || type.GetInterface ("ICollection") != null)
					sType = SchemaTypes.Array;
				else
					sType = SchemaTypes.Class;
			}
		}

		public string ElementName
		{
			get {
				return elementName;
			}
		}
				
		public string TypeName
		{
			get {
				return type.Name;
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
				return type.FullName;
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
					      SchemaType == SchemaTypes.Enum ); 
			}
		}

		public Type ListItemType
		{
			get
			{
				if (SchemaType != SchemaTypes.Array)
					throw new InvalidOperationException (Type.FullName + " is not a collection");
				else if (type.IsArray) 
					return type.GetElementType ();
				else
					return type.GetMethod ("Add").GetParameters()[0].ParameterType;
			}
		}
	}
}

