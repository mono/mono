//
// System.Runtime.Remoting.ActivatedServiceTypeEntry.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting {

	public class ActivatedServiceTypeEntry : TypeEntry
	{
		Type obj_type;
		
		public ActivatedServiceTypeEntry (Type type)			
		{
			AssemblyName = type.Assembly.FullName;
			TypeName = type.FullName;
			obj_type = type;
		}

		public ActivatedServiceTypeEntry (string typeName, string assemblyName)
		{
			AssemblyName = assemblyName;
			TypeName = typeName;
			Assembly a = Assembly.Load (assemblyName);
			obj_type = a.GetType (typeName);
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
			return AssemblyName + TypeName;
		}
	}
}
