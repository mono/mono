// Types.cs
// (C) Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Collections;
using System.Reflection;

namespace Mono.ILASM {

	public class Types {

		// maps default types to their library equivalents
		private static Hashtable defaultTypes;
		private static readonly object dummy;
		private Hashtable userTypes;

		static Types ()
		{
			dummy = new Object ();

			defaultTypes = new Hashtable ();
			Hashtable t = defaultTypes;

			t ["object"]  = Type.GetType ("System.Object");
			t ["string"]  = Type.GetType ("System.String");
			t ["char"]    = Type.GetType ("System.Char");
			t ["void"]    = Type.GetType ("System.Void");
			t ["bool"]    = Type.GetType ("System.Boolean");
			t ["int8"]    = Type.GetType ("System.Byte");
			t ["int16"]   = Type.GetType ("System.Int16");
			t ["int32"]   = Type.GetType ("System.Int32");
			t ["int64"]   = Type.GetType ("System.Int64");
			t ["float32"] = Type.GetType ("System.Single");
			t ["float64"] = Type.GetType ("System.Double");
			t ["uint8"]   = Type.GetType ("System.SByte");
			t ["uint16"]  = Type.GetType ("System.UInt16");
			t ["uint32"]  = Type.GetType ("System.UInt32");
			t ["uint64"]  = Type.GetType ("System.UInt64");
		}


		/// <summary>
		/// </summary>
		public Types ()
		{
		}


		/// <summary>
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public Type Lookup (string typeName)
		{
			Type res = defaultTypes [typeName] as Type;
			return res;
		}


		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		public void Add (string name, Type type)
		{
			if (defaultTypes.Contains (name)) return;

			if (userTypes == null) userTypes = new Hashtable ();
			userTypes [name] = (type != null) ? type : dummy;
		}


		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		public void Add (string name){
			Add (name, null);
		}

	}
}

