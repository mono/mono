//
// System.Runtime.Remoting.WellKnownClientTypeEntry.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Reflection;

namespace System.Runtime.Remoting {

#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
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
			obj_url = objectUrl;
			AssemblyName = assemblyName;
			TypeName = typeName;
			Assembly a = Assembly.Load (assemblyName);
			obj_type = a.GetType (typeName);
			if (obj_type == null) 
				throw new RemotingException ("Type not found: " + typeName + ", " + assemblyName);
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
