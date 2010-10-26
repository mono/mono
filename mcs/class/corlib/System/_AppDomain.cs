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
#if !DISABLE_SECURITY
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Principal;
#endif
using System.Reflection;
#if !MICRO_LIB
using System.Reflection.Emit;
#endif
using System.Globalization;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;

namespace System
{
#if NET_2_0
	[ComVisible (true)]
#endif
	[CLSCompliant (false)]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[Guid ("05F696DC-2B29-3663-AD8B-C4389CF2A713")]
	public interface _AppDomain
	{
		string BaseDirectory {get; }
		string DynamicDirectory {get; }
		#if !DISABLE_SECURITY
		Evidence Evidence {get; }
		#endif
		string FriendlyName {get; }
		string RelativeSearchPath {get; }
		bool ShadowCopyFiles {get; }

#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
#endif
		void AppendPrivatePath (string path);

#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
#endif
		void ClearPrivatePath ();

#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
#endif
		void ClearShadowCopyPath ();

		ObjectHandle CreateInstance (string assemblyName, string typeName);
		ObjectHandle CreateInstance (string assemblyName, string typeName, object[] activationAttributes);
		#if !DISABLE_SECURITY
		ObjectHandle CreateInstance (string assemblyName, string typeName, bool ignoreCase,
			BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture,
			object[] activationAttributes, Evidence securityAttributes);
		#else
		ObjectHandle CreateInstance (string assemblyName, string typeName, bool ignoreCase,
			BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture,
			object[] activationAttributes, object securityAttributes);

		#endif

		ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName);
		ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, object[] activationAttributes);
		#if !DISABLE_SECURITY
		ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, bool ignoreCase,
			BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture,
			object[] activationAttributes, Evidence securityAttributes);
		#else
		ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, bool ignoreCase,
			BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture,
			object[] activationAttributes, object securityAttributes);
		#endif

#if !MICRO_LIB
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
		#if !DISABLE_SECURITY
		int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity);
		int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity, string[] args);
		#else
		int ExecuteAssembly (string assemblyFile, object assemblySecurity);
		int ExecuteAssembly (string assemblyFile, object assemblySecurity, string[] args);
		#endif

		Assembly[] GetAssemblies ();
		object GetData (string name);
		int GetHashCode();

#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
#endif
		object GetLifetimeService ();

		Type GetType ();

#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
#endif
		object InitializeLifetimeService ();

		Assembly Load (AssemblyName assemblyRef);
		Assembly Load (byte[] rawAssembly);
		Assembly Load (string assemblyString);
		#if !DISABLE_SECURITY
		Assembly Load (AssemblyName assemblyRef, Evidence assemblySecurity);
		#else
		Assembly Load (AssemblyName assemblyRef, object assemblySecurity);
		#endif
		Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore);
		#if !DISABLE_SECURITY
		Assembly Load (string assemblyString, Evidence assemblySecurity);
		Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence);
		#else
		Assembly Load (string assemblyString, object assemblySecurity);
		Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore, object securityEvidence);
		#endif

		#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		void SetAppDomainPolicy (PolicyLevel domainPolicy);
		#endif

		#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		#endif
		void SetCachePath (string s);

		#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		#endif
		void SetData (string name, object data);

		#if !DISABLE_SECURITY
		void SetPrincipalPolicy (PrincipalPolicy policy);
		#endif

		#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		#endif
		void SetShadowCopyPath (string s);

		#if !DISABLE_SECURITY
		void SetThreadPrincipal (IPrincipal principal);
		#endif
		string ToString ();

#if BOOTSTRAP_WITH_OLDLIB
		// older MCS/corlib returns:
		// _AppDomain.cs(138) error CS0592: Attribute 'SecurityPermission' is not valid on this declaration type.
		// It is valid on 'assembly' 'class' 'constructor' 'method' 'struct'  declarations only.
		event AssemblyLoadEventHandler AssemblyLoad;
		event ResolveEventHandler AssemblyResolve;
		event EventHandler DomainUnload;
		event EventHandler ProcessExit;
		event ResolveEventHandler ResourceResolve;
		event ResolveEventHandler TypeResolve;
		event UnhandledExceptionEventHandler UnhandledException;
#else
#if !DISABLE_SECURITY
		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
#endif
		event AssemblyLoadEventHandler AssemblyLoad;

#if !DISABLE_SECURITY
		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
#endif
		event ResolveEventHandler AssemblyResolve;

#if !DISABLE_SECURITY
		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
#endif
		event EventHandler DomainUnload;

#if !DISABLE_SECURITY
		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
#endif
		event EventHandler ProcessExit;

#if !DISABLE_SECURITY
		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
#endif
		event ResolveEventHandler ResourceResolve;

#if !DISABLE_SECURITY
		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
#endif
		event ResolveEventHandler TypeResolve;

#if !DISABLE_SECURITY
		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
#endif
		event UnhandledExceptionEventHandler UnhandledException;
#endif

#if NET_1_1
		void GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);

		void GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo);

		void GetTypeInfoCount (out uint pcTInfo);

		void Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
#endif
	}
}
