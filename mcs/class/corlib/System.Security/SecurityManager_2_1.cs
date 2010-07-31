//
// System.Security.SecurityManager.cs
//
// Authors:
//	Nick Drochak(ndrochak@gol.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Nick Drochak
// Portions (C) 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005, 2009-2010 Novell, Inc (http://www.novell.com)
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

#if MOONLIGHT

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;

namespace System.Security {

	// Must match MonoDeclSecurityActions in /mono/metadata/reflection.h
	internal struct RuntimeDeclSecurityActions {
		public RuntimeDeclSecurityEntry cas;
		public RuntimeDeclSecurityEntry noncas;
		public RuntimeDeclSecurityEntry choice;
	}

	internal static class SecurityManager {

		static SecurityManager ()
		{
			// if the security manager (coreclr) is not active then the application has elevated permissions
			HasElevatedPermissions = !SecurityEnabled;
		}

		// note: this let us differentiate between running in the browser (w/CoreCLR) and 
		// running on the desktop (e.g. smcs compiling stuff)
		extern public static bool SecurityEnabled {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		internal static bool HasElevatedPermissions {
			get; set;
		}

		extern static bool RequiresElevatedPermissions {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		internal static bool CheckElevatedPermissions ()
		{
			if (HasElevatedPermissions)
				return true;

			return !RequiresElevatedPermissions;
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		internal static void EnsureElevatedPermissions ()
		{
			// shortcut (to avoid the stack walk) if we are running with elevated trust
			if (HasElevatedPermissions)
				return;

			if (RequiresElevatedPermissions)
				throw new SecurityException ("This operation requires elevated permissions");
		}

		internal static IPermission CheckPermissionSet (Assembly a, PermissionSet ps, bool noncas)
		{
			return null;
		}

		internal static IPermission CheckPermissionSet (AppDomain ad, PermissionSet ps)
		{
			return null;
		}

		internal static PermissionSet Decode (byte[] encodedPermissions)
		{
			return null;
		}

		internal static PermissionSet Decode (IntPtr permissions, int length)
		{
			return null;
		}

		public static bool IsGranted (IPermission perm)
		{
			return false;
		}

		public static PermissionSet ResolvePolicy (Evidence evidence)
		{
			return null;
		}

		public static PermissionSet ResolvePolicy (Evidence evidence, PermissionSet reqdPset, PermissionSet optPset, PermissionSet denyPset, out PermissionSet denied)
		{
			denied = null;
			return null;
		}

		internal static bool ResolvePolicyLevel (ref PermissionSet ps, PolicyLevel pl, Evidence evidence)
		{
			return false;;
		}

		internal static PolicyLevel ResolvingPolicyLevel {
			get { return null; }
		}

		internal static void ReflectedLinkDemandInvoke (MethodBase mb)
		{
		}

		// called by the runtime when CoreCLR is enabled

		private static void ThrowException (Exception ex)
		{
			throw ex;
		}

		// internal - get called by the class loader

		// Called when
		// - class inheritance
		// - method overrides
		private unsafe static bool InheritanceDemand (AppDomain ad, Assembly a, RuntimeDeclSecurityActions *actions)
		{
			return false;
		}

		private static void InheritanceDemandSecurityException (int securityViolation, Assembly a, Type t, MethodInfo method)
		{
		}

		// internal - get called at JIT time

		private static void DemandUnmanaged ()
		{
		}

		// internal - get called by JIT generated code

		private static void InternalDemand (IntPtr permissions, int length)
		{
		}

		private static void InternalDemandChoice (IntPtr permissions, int length)
		{
		}

		private unsafe static bool LinkDemand (Assembly a, RuntimeDeclSecurityActions *klass, RuntimeDeclSecurityActions *method)
		{
			return false;
		}

		private static bool LinkDemandUnmanaged (Assembly a)
		{
			return false;
		}

		private static bool LinkDemandFullTrust (Assembly a)
		{
			return false;
		}

		private static void LinkDemandSecurityException (int securityViolation, IntPtr methodHandle)
		{
		}
	}
}

#endif

