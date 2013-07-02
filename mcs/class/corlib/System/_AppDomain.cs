//
// System.AppDomain.cs
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
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

using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Principal;
using System.Reflection;
#if !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif
using System.Globalization;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;

namespace System
{
	[ComVisible (true)]
	[CLSCompliant (false)]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[Guid ("05F696DC-2B29-3663-AD8B-C4389CF2A713")]
	public interface _AppDomain
	{
		string BaseDirectory {get; }
		string DynamicDirectory {get; }
		Evidence Evidence {get; }
		string FriendlyName {get; }
		string RelativeSearchPath {get; }
		bool ShadowCopyFiles {get; }

		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		void AppendPrivatePath (string path);

		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		void ClearPrivatePath ();

		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		void ClearShadowCopyPath ();

		ObjectHandle CreateInstance (string assemblyName, string typeName);
		ObjectHandle CreateInstance (string assemblyName, string typeName, object[] activationAttributes);
		ObjectHandle CreateInstance (string assemblyName, string typeName, bool ignoreCase,
			BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture,
			object[] activationAttributes, Evidence securityAttributes);

		ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName);
		ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, object[] activationAttributes);
		ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, bool ignoreCase,
			BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture,
			object[] activationAttributes, Evidence securityAttributes);

#if !FULL_AOT_RUNTIME
		AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access);
		AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, Evidence evidence);
		AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir);
		AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence);
		AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access,
			PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions);
		AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access,
			Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions,
			PermissionSet refusedPermissions);
		AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access,
			string dir, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions);
		AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access,
			string dir, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions,
			PermissionSet refusedPermissions);
		AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir,
			Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions,
			PermissionSet refusedPermissions, bool isSynchronized);
#endif

		void DoCallBack (CrossAppDomainDelegate theDelegate);
		bool Equals (object other);

		int ExecuteAssembly (string assemblyFile);
		int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity);
		int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity, string[] args);

		Assembly[] GetAssemblies ();
		object GetData (string name);
		int GetHashCode();

#if !NET_4_0
		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
#endif
		object GetLifetimeService ();

		Type GetType ();

		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
		object InitializeLifetimeService ();

		Assembly Load (AssemblyName assemblyRef);
		Assembly Load (byte[] rawAssembly);
		Assembly Load (string assemblyString);
		Assembly Load (AssemblyName assemblyRef, Evidence assemblySecurity);
		Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore);
		Assembly Load (string assemblyString, Evidence assemblySecurity);
		Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence);

		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		void SetAppDomainPolicy (PolicyLevel domainPolicy);

		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		void SetCachePath (string s);

		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		void SetData (string name, object data);

		void SetPrincipalPolicy (PrincipalPolicy policy);

		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		void SetShadowCopyPath (string s);

		void SetThreadPrincipal (IPrincipal principal);

		string ToString ();

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		event AssemblyLoadEventHandler AssemblyLoad;

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		event ResolveEventHandler AssemblyResolve;

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		event EventHandler DomainUnload;

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		event EventHandler ProcessExit;

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		event ResolveEventHandler ResourceResolve;

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		event ResolveEventHandler TypeResolve;

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		event UnhandledExceptionEventHandler UnhandledException;

#if !NET_2_1
		void GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);

		void GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo);

		void GetTypeInfoCount (out uint pcTInfo);

		void Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
#endif
	}
}
