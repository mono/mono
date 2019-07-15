using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Policy;

#if !MONO_FEATURE_SRE

namespace System {

	public partial class AppDomain {

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access)
		{
			throw new PlatformNotSupportedException ();
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			throw new PlatformNotSupportedException ();
		}


		[Obsolete ("Declarative security for assembly level is no longer enforced")]
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, Evidence evidence)
		{
			throw new PlatformNotSupportedException ();
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir)
		{
			throw new PlatformNotSupportedException ();
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, IEnumerable<CustomAttributeBuilder> assemblyAttributes, SecurityContextSource securityContextSource)
		{
			throw new PlatformNotSupportedException ();
		}

		[Obsolete ("Declarative security for assembly level is no longer enforced")]
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence)
		{
			throw new PlatformNotSupportedException ();
		}

		[Obsolete ("Declarative security for assembly level is no longer enforced")]
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			throw new PlatformNotSupportedException ();
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir, bool isSynchronized, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			throw new PlatformNotSupportedException ();
		}

		[Obsolete ("Declarative security for assembly level is no longer enforced")]
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			throw new PlatformNotSupportedException ();
		}

		[Obsolete ("Declarative security for assembly level is no longer enforced")]
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			throw new PlatformNotSupportedException ();
		}

		[Obsolete ("Declarative security for assembly level is no longer enforced")]
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			throw new PlatformNotSupportedException ();
		}

		[Obsolete ("Declarative security for assembly level is no longer enforced")]
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions, bool isSynchronized)
		{
			throw new PlatformNotSupportedException ();
		}

		[Obsolete ("Declarative security for assembly level is no longer enforced")]
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions, bool isSynchronized, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
