// 
// System.Xml.Serialization.ReflectionHelper 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
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
			if (!_schemaTypes.ContainsKey (mapKey))
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
		
		public static void CheckSerializableType (Type type, bool allowPrivateConstructors)
		{
			if (type.IsArray) return;
			
			if (!allowPrivateConstructors && type.GetConstructor (Type.EmptyTypes) == null && !type.IsAbstract && !type.IsValueType)
				throw new InvalidOperationException (type.FullName + " cannot be serialized because it does not have a default public constructor");
				
			if (type.IsInterface)
				throw new InvalidOperationException (type.FullName + " cannot be serialized because it is an interface");
				
			Type t = type;
			do {
				if (!t.IsPublic && !t.IsNestedPublic)
					throw new InvalidOperationException (type.FullName + " is inaccessible due to its protection level. Only public types can be processed");
				t = t.DeclaringType;
			}
			while (t != null);
		}
		
		public static string BuildMapKey (Type type)
		{
			return type.FullName + "::";
		}
		
		public static string BuildMapKey (MethodInfo method, string tag)
		{
			string res = method.DeclaringType.FullName + ":" + method.ReturnType.FullName + " " + method.Name + "(";
			
			ParameterInfo[] pars = method.GetParameters ();
			
			for (int n=0; n<pars.Length; n++)
			{
				if (n > 0) res += ", ";
				res += pars[n].ParameterType.FullName;
			}
			res += ")";
			
			if (tag != null)
				res += ":" + tag;
				
			return res;
		}
	}
}
