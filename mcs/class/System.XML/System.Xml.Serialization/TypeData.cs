//
// System.Xml.Serialization.TypeData
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

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
				sType = SchemaTypes.NotSet;
		}

		private SchemaTypes GetSchemaType ()
		{
			if (type.IsEnum)
				return SchemaTypes.Enum;
			else if (type.IsArray)
				return SchemaTypes.Array;
			else if (type == typeof (System.Data.DataSet))
				return SchemaTypes.DataSet;
			else if (type == typeof (System.Xml.XmlNode))
				return SchemaTypes.XmlNode;
			return SchemaTypes.Class;
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
				return type.FullName.Replace ('+', '.');
			}
		}

		public SchemaTypes SchemaType
		{
			get {
				if (sType == SchemaTypes.NotSet)
					sType = GetSchemaType ();

				return sType;
			}
		}
	}
}

