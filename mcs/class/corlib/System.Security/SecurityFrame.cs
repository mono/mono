//
// System.Security.SecurityFrame.cs
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Permissions;

namespace System.Security {

	// Must match MonoDeclSecurityEntry in /mono/metadata/reflection.h
	internal struct RuntimeDeclSecurityEntry {
		public IntPtr blob;
		public int size;
	}

	// Must match MonoSecurityFrame in /mono/mini/declsec.h
	internal class RuntimeSecurityFrame {
		public MethodInfo method;
		public RuntimeDeclSecurityEntry assert;
		public RuntimeDeclSecurityEntry deny;
		public RuntimeDeclSecurityEntry permitonly;
	}

	internal struct SecurityFrame {

		private MethodInfo _method;
		private PermissionSet _assert;
		private PermissionSet _deny;
		private PermissionSet _permitonly;

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static RuntimeSecurityFrame _GetSecurityFrame (int skip);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static Array _GetSecurityStack (int skip);

		internal SecurityFrame (RuntimeSecurityFrame frame)
		{
			_method = null;
			_assert = null;
			_deny = null;
			_permitonly = null;
			InitFromRuntimeFrame (frame);
		}

		internal SecurityFrame (int skip)
		{
			_method = null;
			_assert = null;
			_deny = null;
			_permitonly = null;

			InitFromRuntimeFrame (_GetSecurityFrame (skip + 2));

			// TODO - add the imperative informations into the frame
		}

		// Note: SecurityManager.Decode implements a cache - so not every call
		// ends up making an icall
		internal void InitFromRuntimeFrame (RuntimeSecurityFrame frame)
		{
			_method = frame.method;

			if (frame.assert.size > 0) {
				_assert = SecurityManager.Decode (frame.assert.blob, frame.assert.size);
			}
			if (frame.deny.size > 0) {
				_deny = SecurityManager.Decode (frame.deny.blob, frame.deny.size);
			}
			if (frame.permitonly.size > 0) {
				_permitonly = SecurityManager.Decode (frame.permitonly.blob, frame.permitonly.size);
			}
		}

		public Assembly Assembly {
			get { return _method.ReflectedType.Assembly; }
		}

		public MethodInfo Method {
			get { return _method; }
		}

		public PermissionSet Assert {
			get { return _assert; }
		}

		public PermissionSet Deny {
			get { return _deny; }
		}

		public PermissionSet PermitOnly {
			get { return _permitonly; }
		}

		public bool HasStackModifiers {
			get { return ((_assert != null) || (_deny != null) || (_permitonly != null)); }
		}

		public bool Equals (SecurityFrame sf)
		{
			if (Assembly.ToString () != sf.Assembly.ToString ())
				return false;
			if (Method.ToString () != sf.Method.ToString ())
				return false;

			if ((_assert != null) && !_assert.Equals (sf.Assert))
				return false;
			if ((_deny != null) && !_deny.Equals (sf.Deny))
				return false;
			if ((_permitonly != null) && !_permitonly.Equals (sf.PermitOnly))
				return false;

			return true;
		}

		static public ArrayList GetStack (int skipFrames)
		{
			Array stack = _GetSecurityStack (skipFrames+2);
			ArrayList al = new ArrayList (stack.Length);
			foreach (RuntimeSecurityFrame frame in stack) {
				al.Add (new SecurityFrame (frame));
			}
			return al;
		}
	}
}
