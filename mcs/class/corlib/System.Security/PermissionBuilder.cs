//
// System.Security.PermissionBuilder.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Security.Permissions;

namespace System.Security {

#if NET_2_0
	internal static class PermissionBuilder {
#else
	internal class PermissionBuilder {
#endif
		private static object[] psNone = new object [1] { PermissionState.None };

		// can be used to create an empty or an unrestricted permission from any valid type
		static public IPermission Create (string fullname, PermissionState state)
		{
			if (fullname == null)
				throw new ArgumentNullException ("fullname");

			SecurityElement se = new SecurityElement ("IPermission");
			se.AddAttribute ("class", fullname);
			se.AddAttribute ("version", "1");
			if (state == PermissionState.Unrestricted)
				se.AddAttribute ("Unrestricted", "true");

			return CreatePermission (fullname, se);
		}

		static public IPermission Create (SecurityElement se)
		{
			if (se == null)
				throw new ArgumentNullException ("se");

			string className = se.Attribute ("class");
			if ((className == null) || (className.Length == 0))
				throw new ArgumentException ("class");

			return CreatePermission (className, se);
		}

		// to use in case where the "class" attribute isn't a fully qualified class name
		static public IPermission Create (string fullname, SecurityElement se)
		{
			if (fullname == null)
				throw new ArgumentNullException ("fullname");
			if (se == null)
				throw new ArgumentNullException ("se");

			return CreatePermission (fullname, se);
		}

		static public IPermission Create (Type type)
		{
			// note: unification is handled in lower levels
			// http://blogs.msdn.com/shawnfa/archive/2004/08/05/209320.aspx
			return (IPermission) Activator.CreateInstance (type, psNone);
		}

		// internal stuff

		internal static IPermission CreatePermission (string fullname, SecurityElement se)
		{
			Type classType = Type.GetType (fullname);
			if (classType == null) {
				string msg = Locale.GetText ("Can't create an instance of permission class {0}.");
#if NET_2_0
				throw new TypeLoadException (String.Format (msg, fullname));
#else
				throw new ArgumentException (String.Format (msg, fullname));
#endif
			}
			#if !DISABLE_SECURITY
			IPermission p = Create (classType);
			p.FromXml (se);
			return p;
			#else
			return null;
			#endif
		}
	}
}
