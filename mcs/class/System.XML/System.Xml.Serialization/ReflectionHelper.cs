// 
// System.Xml.Serialization.ReflectionHelper 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
//

using System.Reflection;
using System.Collections;

namespace System.Xml.Serialization
{
	internal class ReflectionHelper
	{
		Hashtable _clrTypes = new Hashtable ();
		Hashtable _schemaTypes = new Hashtable ();

		public void RegisterSchemaType (XmlTypeMapping map, string xmlType, string ns)
		{
			string mapKey = xmlType + "/" + ns;
			if (!_schemaTypes.ContainsKey (xmlType))
				_schemaTypes.Add (mapKey, map);
		}

		public XmlTypeMapping GetRegisteredSchemaType (string xmlType, string ns)
		{
			string mapKey = xmlType + "/" + ns;
			return _schemaTypes[mapKey] as XmlTypeMapping;
		}

		public void RegisterClrType (XmlTypeMapping map, Type type, string ns)
		{
			if (type == typeof(object)) ns = "";
			string mapKey = type.FullName + "/" + ns;
			if (!_clrTypes.ContainsKey (mapKey))
				_clrTypes.Add (mapKey, map);
		}

		public XmlTypeMapping GetRegisteredClrType (Type type, string ns)
		{
			if (type == typeof(object)) ns = "";
			string mapKey = type.FullName + "/" + ns;
			return _clrTypes[mapKey] as XmlTypeMapping;
		}	

		public Exception CreateError (XmlTypeMapping map, string message)
		{
			return new InvalidOperationException ("There was an error reflecting '" + map.TypeFullName + "': " + message);
		}
	}
}
