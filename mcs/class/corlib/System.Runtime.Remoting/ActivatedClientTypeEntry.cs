//
// System.Runtime.Remoting.ActivatedClientTypeEntry.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting {

	public class ActivatedClientTypeEntry : TypeEntry
	{
		string applicationUrl;
		Type obj_type;
		
		public ActivatedClientTypeEntry (Type type, string appUrl)
		{
			AssemblyName = type.Assembly.FullName;
			TypeName = type.FullName;
			applicationUrl = appUrl;
			obj_type = type;
		}

		public ActivatedClientTypeEntry (string typeName, string assemblyName, string appUrl)
		{
			AssemblyName = assemblyName;
			TypeName = typeName;
			applicationUrl = appUrl;
		}

		public string ApplicationUrl {
			get { return applicationUrl; }
		}

		public IContextAttribute [] ContextAttributes {
			get { return null; }
			set { } // This is not implemented in the MS runtime yet.
		}

		public Type ObjectType {
			get { return obj_type; }
		}

		public override string ToString ()
		{
			return TypeName + AssemblyName + ApplicationUrl;
		}
	}
}
