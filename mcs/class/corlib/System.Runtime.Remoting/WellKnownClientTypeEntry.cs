//
// System.Runtime.Remoting.WellKnownClientTypeEntry.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;
using System.Reflection;

namespace System.Runtime.Remoting {

	public class WellKnownClientTypeEntry : TypeEntry
	{
		Type obj_type;
		string obj_url;
		string app_url = null;
		
		public WellKnownClientTypeEntry (Type type, string objectUrl )
		{
			AssemblyName = type.Assembly.FullName;
			TypeName = type.FullName;
			obj_type = type;
			obj_url = objectUrl;
		}

		public WellKnownClientTypeEntry (string typeName, string assemblyName, string objectUrl)
		{
			AssemblyName = assemblyName;
			TypeName = typeName;
			Assembly a = Assembly.Load (assemblyName);
			obj_type = a.GetType (typeName);
		}

		public string ApplicationUrl {
			get { return app_url; }
			set { app_url = value; }
		}

		public Type ObjectType {
			get { return obj_type; }
		}

		public string ObjectUrl {
			get { return obj_url; }
			set { obj_url = value; }
		}

		public override string ToString ()
		{
			if (ApplicationUrl != null)
				return TypeName + AssemblyName + ObjectUrl + ApplicationUrl;
			else
				return TypeName + AssemblyName + ObjectUrl;
		}
	}
}
