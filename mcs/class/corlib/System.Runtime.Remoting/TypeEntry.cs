//
// System.Runtime.Remoting.TypeEntry.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;

namespace System.Runtime.Remoting {

	public class TypeEntry
	{
		string assembly_name;
		string type_name;
		
		protected TypeEntry ()
		{
		}

		public string AssemblyName {
			get { return assembly_name; }
			set { assembly_name = value; }
		}

		public string TypeName {
			get { return type_name; }
			set { type_name = value; }
		}
	}
}
