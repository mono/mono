//
// System._AppDomain
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//

using System.Security;
using System.Security.Policy;
using System.Security.Principal;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.Remoting;

namespace System
{

[CLSCompliant(false)]
public interface _AppDomain {

	string BaseDirectory {get; }
	string DynamicDirectory {get; }
	Evidence Evidence {get; }
	string FriendlyName {get; }
	string RelativeSearchPath {get; }
	bool ShadowCopyFiles {get; }

	void AppendPrivatePath (string path);
	void ClearPrivatePath ();
	void ClearShadowCopyPath ();

	ObjectHandle CreateInstance (string assemblyName, string typeName);
	ObjectHandle CreateInstance (
		string assemblyName,
		string typeName,
		object[] activationAttributes);
	ObjectHandle CreateInstance (
		string assemblyName,
		string typeName,
		bool ignoreCase,
		BindingFlags bindingAttr,
		Binder binder,
		object[] args,
		CultureInfo culture,
		object[] activationAttributes,
		Evidence securityAttribtutes);

	ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName);
	ObjectHandle CreateInstanceFrom (
		string assemblyName, string typeName,
		object[] activationAttributes);
	ObjectHandle CreateInstanceFrom (string assemblyName,
		string typeName,
		bool ignoreCase,
		BindingFlags bindingAttr,
		Binder binder,
		object[] args,
		CultureInfo culture,
		object[] activationAttributes,
		Evidence securityAttribtutes);

	AssemblyBuilder DefineDynamicAssembly (
		AssemblyName name,
		AssemblyBuilderAccess access);
	AssemblyBuilder DefineDynamicAssembly (
		AssemblyName name,
		AssemblyBuilderAccess access,
		Evidence evidence);
	AssemblyBuilder DefineDynamicAssembly (
		AssemblyName name,
		AssemblyBuilderAccess access, string dir);
	AssemblyBuilder DefineDynamicAssembly (
		AssemblyName name,
		AssemblyBuilderAccess access,
		string dir,
		Evidence evidence);
	AssemblyBuilder DefineDynamicAssembly (
		AssemblyName name,
		AssemblyBuilderAccess access,
		PermissionSet requiredPermissions,
		PermissionSet optionalPermissions,
		PermissionSet refusedPersmissions);
	AssemblyBuilder DefineDynamicAssembly (
		AssemblyName name,
		AssemblyBuilderAccess access,
		Evidence evidence,
		PermissionSet requiredPermissions,
		PermissionSet optionalPermissions,
		PermissionSet refusedPersmissions);
	AssemblyBuilder DefineDynamicAssembly (
		AssemblyName name,
		AssemblyBuilderAccess access,
		string dir,
		PermissionSet requiredPermissions,
		PermissionSet optionalPermissions,
		PermissionSet refusedPersmissions);
	AssemblyBuilder DefineDynamicAssembly (
		AssemblyName name,
		AssemblyBuilderAccess access,
		string dir,
		Evidence evidence,
		PermissionSet requiredPermissions,
		PermissionSet optionalPermissions,
		PermissionSet refusedPersmissions);
	AssemblyBuilder DefineDynamicAssembly (
		AssemblyName name,
		AssemblyBuilderAccess access,
		string dir,
		Evidence evidence,
		PermissionSet requiredPermissions,
		PermissionSet optionalPermissions,
		PermissionSet refusedPersmissions,
		bool isSynchronized);

	void DoCallBack (CrossAppDomainDelegate theDelegate);
	bool Equals (object other);

	int ExecuteAssembly (string assemblyFile);
	int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity);
	int ExecuteAssembly (
		string assemblyFile,
		Evidence assemblySecurity,
		string[] args);

	Assembly[] GetAssemblies ();
	object GetData (string name);
	int GetHashCode();
	object GetLifetimeService ();
	Type GetType ();
	object InitializeLifetimeService ();

	Assembly Load (AssemblyName assemblyRef);
	Assembly Load (byte[] rawAssembly);
	Assembly Load (string assemblyString);
	Assembly Load (AssemblyName assemblyRef, Evidence assemblySecurity);
	Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore);
	Assembly Load (string assemblyString, Evidence assemblySecurity);
	Assembly Load (
		byte[] rawAssembly,
		byte[] rawSymbolStore,
		Evidence securityEvidence);

	void SetAppDomainPolicy (PolicyLevel domainPolicy);
	void SetCachePath (string s);
	void SetData (string name, object data);
	void SetPrincipalPolicy (PrincipalPolicy policy);
	void SetShadowCopyPath (string s);
	void SetThreadPrincipal (IPrincipal principal);
	string ToString ();

	event AssemblyLoadEventHandler AssemblyLoad;
	event ResolveEventHandler AssemblyResolve;
	event EventHandler DomainUnload;
	event EventHandler ProcessExit;
	event ResolveEventHandler ResourceResolve;
	event ResolveEventHandler TypeResolve;
	event UnhandledExceptionEventHandler UnhandledException;
}

}
