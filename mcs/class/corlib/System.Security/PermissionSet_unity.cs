//
// System.Security.PermissionSet.cs
//
// Authors:
//	Nick Drochak(ndrochak@gol.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Nick Drochak
// Portions (C) 2003, 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading;

namespace System.Security {

	[Serializable]
	public class PermissionSet {

		public PermissionSet ()
		{
		}
		
		internal PermissionSet (string xml)
		{
		}

		public PermissionSet (PermissionState state)
		{
		}

		public PermissionSet (PermissionSet permSet)
		{
		}

		public IPermission AddPermission (IPermission perm)
		{
			return perm;
		}
		
		public virtual void Assert ()
		{
		}
		
		public virtual PermissionSet Copy ()
		{
			return new PermissionSet (this);
		}
		
		public virtual void Demand ()
		{
		}
		
		public virtual void PermitOnly ()
		{
		}
		
		public virtual IPermission GetPermission (Type permClass)
		{
			return null;
		}
		
		public virtual PermissionSet Intersect (PermissionSet other)
		{
			return other;
		}
		
		public virtual void Deny ()
		{
		}
		
		public virtual void FromXml (SecurityElement et)
		{
		}
		
		public virtual void CopyTo (Array array, int index)
		{
		}
		
		public virtual SecurityElement ToXml ()
		{
			return null;
		}
		
		public virtual bool IsSubsetOf (PermissionSet target)
		{
			return true;
		}
		
		internal void SetReadOnly (bool value)
		{
		}
		
		public bool IsUnrestricted ()
		{
			return true;
		}

		public PermissionSet Union (PermissionSet other)
		{
			return new PermissionSet ();
		}
		
		public virtual IEnumerator GetEnumerator()
		{
			yield break;
		}
		
		internal PolicyLevel Resolver
		{
			get; set;
		}
		
		internal bool DeclarativeSecurity
		{
			get; set;
		}
		
		public virtual bool IsEmpty ()
		{
			return true;
		}
		
		internal static PermissionSet CreateFromBinaryFormat (byte[] data)
		{
			return new PermissionSet ();
		}
	}
}
