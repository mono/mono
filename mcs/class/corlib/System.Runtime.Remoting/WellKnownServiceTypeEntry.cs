//
// System.Runtime.Remoting.WellKnownServiceTypeEntry.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;
using System.Reflection;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting {

	public class WellKnownServiceTypeEntry : TypeEntry
	{
		Type obj_type;
		string obj_uri;
		WellKnownObjectMode obj_mode;
		
		public WellKnownServiceTypeEntry (Type type, string objectUri, WellKnownObjectMode mode)			
		{
			AssemblyName = type.Assembly.FullName;
			TypeName = type.FullName;
			obj_type = type;
			obj_uri = objectUri;
			obj_mode = mode;
		}

		public WellKnownServiceTypeEntry (string typeName, string assemblyName,
						  string objectUri, WellKnownObjectMode mode)			
		{
			AssemblyName = assemblyName;
			TypeName = typeName;
			Assembly a = Assembly.Load (assemblyName);
			obj_type = a.GetType (typeName);
			obj_uri = objectUri;
			obj_mode = mode;
		}

		public IContextAttribute [] ContextAttributes {
			get { return null; }
			set { } // This is not implemented in the MS runtime yet.
		}

		public WellKnownObjectMode Mode {
			get { return obj_mode; }
		}

		public Type ObjectType {
			get { return obj_type; }
		}

		public string ObjectUri {
			get { return obj_uri; }
		}

		[MonoTODO]
		public override string ToString ()
		{
			return TypeName + AssemblyName + ObjectUri;
		}
	}
}
