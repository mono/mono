//
// System.Security.SecurityManager.cs
//
// Authors:
//	Nick Drochak(ndrochak@gol.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Nick Drochak
// Portions (C) 2004 Motus Technologies Inc. (http://www.motus.com)
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

#if MOBILE

using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Policy;

namespace System.Security {

	// Must match MonoDeclSecurityActions in /mono/metadata/reflection.h
	internal struct RuntimeDeclSecurityActions {
		public RuntimeDeclSecurityEntry cas;
		public RuntimeDeclSecurityEntry noncas;
		public RuntimeDeclSecurityEntry choice;
	}

	[ComVisible (true)]
	public static class SecurityManager {

		// properties

#if NET_4_0
		[Obsolete]
#endif
		public static bool CheckExecutionRights {
			get { return false; }
			set { ; }
		}

		[Obsolete ("The security manager cannot be turned off on MS runtime")]
		public static bool SecurityEnabled {
			get { return false; }
			set { ; }
		}
		
		internal static bool HasElevatedPermissions {
			get { return true; }
		}

		internal static bool CheckElevatedPermissions ()
		{
				return true;
		}

		internal static void EnsureElevatedPermissions ()
		{
		}
		
		// methods

		[StrongNameIdentityPermission (SecurityAction.LinkDemand, PublicKey = "0x00000000000000000400000000000000")]
		public static void GetZoneAndOrigin (out ArrayList zone, out ArrayList origin) 
		{
			zone = new ArrayList ();
			origin = new ArrayList ();
		}

#if NET_4_0
		[Obsolete]
#endif
		public static bool IsGranted (IPermission perm)
		{
			return true;
		}

#if NET_4_0
		[Obsolete]
#endif
		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
		public static PolicyLevel LoadPolicyLevelFromFile (string path, PolicyLevelType type)
		{
			throw new NotSupportedException ();
		}

#if NET_4_0
		[Obsolete]
#endif
		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
		public static PolicyLevel LoadPolicyLevelFromString (string str, PolicyLevelType type)
		{
			throw new NotSupportedException ();
		}

#if NET_4_0
		[Obsolete]
#endif
		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
		public static IEnumerator PolicyHierarchy ()
		{
			throw new NotSupportedException ();
		}

#if NET_4_0
		[Obsolete]
#endif
		public static PermissionSet ResolvePolicy (Evidence evidence)
		{
			throw new NotSupportedException ();
		}

#if NET_4_0
		[Obsolete]
#endif
		public static PermissionSet ResolvePolicy (Evidence[] evidences)
		{
			throw new NotSupportedException ();
		}

#if NET_4_0
		[Obsolete]
#endif
		public static PermissionSet ResolveSystemPolicy (Evidence evidence)
		{
			throw new NotSupportedException ();
		}

#if NET_4_0
		[Obsolete]
#endif
		public static PermissionSet ResolvePolicy (Evidence evidence, PermissionSet reqdPset, PermissionSet optPset, PermissionSet denyPset, out PermissionSet denied)
		{
			throw new NotSupportedException ();
		}

#if NET_4_0
		[Obsolete]
#endif
		public static IEnumerator ResolvePolicyGroups (Evidence evidence)
		{
			throw new NotSupportedException ();
		}

#if NET_4_0
		[Obsolete]
#endif
		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
		public static void SavePolicy () 
		{
			throw new NotSupportedException ();
		}

#if NET_4_0
		[Obsolete]
#endif
		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
		public static void SavePolicyLevel (PolicyLevel level) 
		{
			throw new NotSupportedException ();
		}

		// private/internal stuff

		internal static bool ResolvePolicyLevel (ref PermissionSet ps, PolicyLevel pl, Evidence evidence)
		{
			throw new NotSupportedException ();
		}

		internal static void ResolveIdentityPermissions (PermissionSet ps, Evidence evidence)
		{
			throw new NotSupportedException ();
		}

		internal static PolicyLevel ResolvingPolicyLevel {
			get { return null; }
			set { ; }
		}

		internal static IPermission CheckPermissionSet (Assembly a, PermissionSet ps, bool noncas)
		{
			return null;
		}

		internal static IPermission CheckPermissionSet (AppDomain ad, PermissionSet ps)
		{
			return null;
		}

		internal static PermissionSet Decode (IntPtr permissions, int length)
		{
			throw new NotSupportedException ();
		}

		internal static PermissionSet Decode (byte[] encodedPermissions)
		{
			throw new NotSupportedException ();
		}

		internal static void ReflectedLinkDemandInvoke (MethodBase mb)
		{
			throw new NotSupportedException ();
		}

		internal static bool ReflectedLinkDemandQuery (MethodBase mb)
		{
			throw new NotSupportedException ();
		}

#if NET_4_0
		public static PermissionSet GetStandardSandbox (Evidence evidence)
		{
			if (evidence == null)
				throw new ArgumentNullException ("evidence");

			throw new NotImplementedException ();
		}

		public static bool CurrentThreadRequiresSecurityContextCapture ()
		{
			throw new NotImplementedException ();
		}
#endif
	}
}

#endif