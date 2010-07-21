//
// System.AppDomain.cs
//
// Authors:
//   Paolo Molaro (lupus@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Gonzalo Paniagua (gonzalo@ximian.com)
//   Patrik Torstensson
//   Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.  http://www.ximian.com
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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Principal;
using System.Configuration.Assemblies;

using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace System {

	[ComVisible (true)]
#if !NET_2_1
	[ComDefaultInterface (typeof (_AppDomain))]
#endif
	[ClassInterface(ClassInterfaceType.None)]
#if NET_2_1
	public sealed class AppDomain : MarshalByRefObject {
#else
	public sealed class AppDomain : MarshalByRefObject, _AppDomain, IEvidenceFactory {
#endif
        #pragma warning disable 169
        #region Sync with object-internals.h
		IntPtr _mono_app_domain;
		#endregion
        #pragma warning restore 169
		static string _process_guid;

		[ThreadStatic]
		static Hashtable type_resolve_in_progress;

		[ThreadStatic]
		static Hashtable assembly_resolve_in_progress;

		[ThreadStatic]
		static Hashtable assembly_resolve_in_progress_refonly;
#if !MOONLIGHT
		// CAS
		private Evidence _evidence;
		private PermissionSet _granted;

		// non-CAS
		private PrincipalPolicy _principalPolicy;

		[ThreadStatic]
		private static IPrincipal _principal;
#endif
		static AppDomain default_domain;

		private AppDomain ()
		{
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern AppDomainSetup getSetup ();

#if NET_2_1
		internal
#endif
		AppDomainSetup SetupInformationNoCopy {
			get { return getSetup (); }
		}

		public AppDomainSetup SetupInformation {
			get {
				AppDomainSetup setup = getSetup ();
				return new AppDomainSetup (setup);
			}
		}

#if !NET_2_1
		[MonoTODO]
		public ApplicationTrust ApplicationTrust {
			get { throw new NotImplementedException (); }
		}
#endif
#if !MOONLIGHT
		public string BaseDirectory {
			get {
				string path = SetupInformationNoCopy.ApplicationBase;
				if (SecurityManager.SecurityEnabled && (path != null) && (path.Length > 0)) {
					// we cannot divulge local file informations
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, path).Demand ();
				}
				return path;
			}
		}

		public string RelativeSearchPath {
			get {
				string path = SetupInformationNoCopy.PrivateBinPath;
				if (SecurityManager.SecurityEnabled && (path != null) && (path.Length > 0)) {
					// we cannot divulge local file informations
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, path).Demand ();
				}
				return path;
			}
		}

		public string DynamicDirectory {
			get {
				AppDomainSetup setup = SetupInformationNoCopy;
				if (setup.DynamicBase == null)
					return null;

				string path = Path.Combine (setup.DynamicBase, setup.ApplicationName);
				if (SecurityManager.SecurityEnabled && (path != null) && (path.Length > 0)) {
					// we cannot divulge local file informations
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, path).Demand ();
				}
				return path;
			}
		}

		public bool ShadowCopyFiles {
			get {
				return (SetupInformationNoCopy.ShadowCopyFiles == "true");
			}
		}
#endif

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string getFriendlyName ();

		public string FriendlyName {
			get {
				return getFriendlyName ();
			}
		}
#if !MOONLIGHT
		public Evidence Evidence {
			get {
				// if the host (runtime) hasn't provided it's own evidence...
				if (_evidence == null) {
					// ... we will provide our own
					lock (this) {
						// the executed assembly from the "default" appdomain
						// or null if we're not in the default appdomain or
						// if there is no entry assembly (embedded mono)
						Assembly a = Assembly.GetEntryAssembly ();
						if (a == null) {
							if (this == DefaultDomain)
								// mono is embedded
								return new Evidence ();
							else
								_evidence = AppDomain.DefaultDomain.Evidence;
						} else {
							_evidence = Evidence.GetDefaultHostEvidence (a);
						}
					}
				}
				return new Evidence (_evidence);	// return a copy
			}
		}

		internal IPrincipal DefaultPrincipal {
			get {
				if (_principal == null) {
					switch (_principalPolicy) {
						case PrincipalPolicy.UnauthenticatedPrincipal:
							_principal = new GenericPrincipal (
								new GenericIdentity (String.Empty, String.Empty), null);
							break;
						case PrincipalPolicy.WindowsPrincipal:
							_principal = new WindowsPrincipal (WindowsIdentity.GetCurrent ());
							break;
					}
				}
				return _principal; 
			}
		}

		// for AppDomain there is only an allowed (i.e. granted) set
		// http://msdn.microsoft.com/library/en-us/cpguide/html/cpcondetermininggrantedpermissions.asp
		internal PermissionSet GrantedPermissionSet {
			get { return _granted; }
		}
#endif

#if NET_4_0
		public PermissionSet PermissionSet {
			get { return this.GrantedPermissionSet; }
		}
#endif

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain getCurDomain ();
		
		public static AppDomain CurrentDomain {
			get {
				return getCurDomain ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain getRootDomain ();

		internal static AppDomain DefaultDomain {
			get {
				if (default_domain == null) {
					AppDomain rd = getRootDomain ();
					if (rd == CurrentDomain)
						default_domain = rd;
					else
						default_domain = (AppDomain) RemotingServices.GetDomainProxy (rd);
				}
				return default_domain;
			}
		}

#if !MOONLIGHT

		[Obsolete ("AppDomain.AppendPrivatePath has been deprecated. Please investigate the use of AppDomainSetup.PrivateBinPath instead.")]
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void AppendPrivatePath (string path)
		{
			if (path == null || path.Length == 0)
				return;

			AppDomainSetup setup = SetupInformationNoCopy;

			string pp = setup.PrivateBinPath;
			if (pp == null || pp.Length == 0) {
				setup.PrivateBinPath = path;
				return;
			}

			pp = pp.Trim ();
			if (pp [pp.Length - 1] != Path.PathSeparator)
				pp += Path.PathSeparator;

			setup.PrivateBinPath = pp + path;
		}

		[Obsolete ("AppDomain.ClearPrivatePath has been deprecated. Please investigate the use of AppDomainSetup.PrivateBinPath instead.")]
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void ClearPrivatePath ()
		{
			SetupInformationNoCopy.PrivateBinPath = String.Empty;
		}

		[Obsolete ("Use AppDomainSetup.ShadowCopyDirectories")]
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void ClearShadowCopyPath ()
		{
			SetupInformationNoCopy.ShadowCopyDirectories = String.Empty;
		}

#if !NET_2_1
		public ObjectHandle CreateComInstanceFrom (string assemblyName, string typeName)
		{
			return Activator.CreateComInstanceFrom (assemblyName, typeName);
		}

		public ObjectHandle CreateComInstanceFrom (string assemblyFile, string typeName,
			byte [] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			return Activator.CreateComInstanceFrom (assemblyFile, typeName, hashValue ,hashAlgorithm);
		}
#endif

		public ObjectHandle CreateInstance (string assemblyName, string typeName)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstance (assemblyName, typeName);
		}

		public ObjectHandle CreateInstance (string assemblyName, string typeName, object[] activationAttributes)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstance (assemblyName, typeName, activationAttributes);
		}

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public ObjectHandle CreateInstance (string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr,
		                                    Binder binder, object[] args, CultureInfo culture, object[] activationAttributes,
		                                    Evidence securityAttributes)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstance (assemblyName, typeName, ignoreCase, bindingAttr, binder, args,
				culture, activationAttributes, securityAttributes);
		}

		public object CreateInstanceAndUnwrap (string assemblyName, string typeName)
		{
			ObjectHandle oh = CreateInstance (assemblyName, typeName);
			return (oh != null) ? oh.Unwrap () : null;
		}

		public object CreateInstanceAndUnwrap (string assemblyName, string typeName, object [] activationAttributes)
		{
			ObjectHandle oh = CreateInstance (assemblyName, typeName, activationAttributes);
			return (oh != null) ? oh.Unwrap () : null;
		}

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public object CreateInstanceAndUnwrap (string assemblyName, string typeName, bool ignoreCase,
		                                       BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture,
		                                       object[] activationAttributes, Evidence securityAttributes)
		{
			ObjectHandle oh = CreateInstance (assemblyName, typeName, ignoreCase, bindingAttr, binder, args,
				culture, activationAttributes, securityAttributes);
			return (oh != null) ? oh.Unwrap () : null;
		}

#if NET_4_0
		public ObjectHandle CreateInstance (string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr,
		                                    Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstance (assemblyName, typeName, ignoreCase, bindingAttr, binder, args,
				culture, activationAttributes, null);
		}
		public object CreateInstanceAndUnwrap (string assemblyName, string typeName, bool ignoreCase,
		                                       BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture,
		                                       object[] activationAttributes)
		{
			ObjectHandle oh = CreateInstance (assemblyName, typeName, ignoreCase, bindingAttr, binder, args,
				culture, activationAttributes);
			return (oh != null) ? oh.Unwrap () : null;
		}

		public ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, bool ignoreCase,
		                                        BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture,
		                                        object[] activationAttributes)
		{
			if (assemblyFile == null)
				throw new ArgumentNullException ("assemblyFile");

			return Activator.CreateInstanceFrom (assemblyFile, typeName, ignoreCase, bindingAttr, binder, args,
			                                     culture, activationAttributes, null);
		}

		public object CreateInstanceFromAndUnwrap (string assemblyName, string typeName, bool ignoreCase,
		                                           BindingFlags bindingAttr, Binder binder, object[] args,
		                                           CultureInfo culture, object[] activationAttributes)
		{
			ObjectHandle oh = CreateInstanceFrom (assemblyName, typeName, ignoreCase, bindingAttr, binder, args,
				culture, activationAttributes);

			return (oh != null) ? oh.Unwrap () : null;
		}
#endif

		public ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName)
		{
			if (assemblyFile == null)
				throw new ArgumentNullException ("assemblyFile");

			return Activator.CreateInstanceFrom (assemblyFile, typeName);
		}

		public ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, object[] activationAttributes)
		{
			if (assemblyFile == null)
				throw new ArgumentNullException ("assemblyFile");

			return Activator.CreateInstanceFrom (assemblyFile, typeName, activationAttributes);
		}

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, bool ignoreCase,
		                                        BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture,
		                                        object[] activationAttributes, Evidence securityAttributes)
		{
			if (assemblyFile == null)
				throw new ArgumentNullException ("assemblyFile");

			return Activator.CreateInstanceFrom (assemblyFile, typeName, ignoreCase, bindingAttr, binder, args,
			                                     culture, activationAttributes, securityAttributes);
		}

		public object CreateInstanceFromAndUnwrap (string assemblyName, string typeName)
		{
			ObjectHandle oh = CreateInstanceFrom (assemblyName, typeName);
			return (oh != null) ? oh.Unwrap () : null;
		}

		public object CreateInstanceFromAndUnwrap (string assemblyName, string typeName, object [] activationAttributes)
		{
			ObjectHandle oh = CreateInstanceFrom (assemblyName, typeName, activationAttributes);
			return (oh != null) ? oh.Unwrap () : null;
		}

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public object CreateInstanceFromAndUnwrap (string assemblyName, string typeName, bool ignoreCase,
		                                           BindingFlags bindingAttr, Binder binder, object[] args,
		                                           CultureInfo culture, object[] activationAttributes,
		                                           Evidence securityAttributes)
		{
			ObjectHandle oh = CreateInstanceFrom (assemblyName, typeName, ignoreCase, bindingAttr, binder, args,
				culture, activationAttributes, securityAttributes);

			return (oh != null) ? oh.Unwrap () : null;
		}

#endif // !NET_2_1

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access)
		{
			return DefineDynamicAssembly (name, access, null, null, null, null, null, false);
		}

#if NET_4_0
		[Obsolete ("Declarative security for assembly level is no longer enforced")]
#endif
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, Evidence evidence)
		{
			return DefineDynamicAssembly (name, access, null, evidence, null, null, null, false);
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir)
		{
			return DefineDynamicAssembly (name, access, dir, null, null, null, null, false);
		}

#if NET_4_0
		[Obsolete ("Declarative security for assembly level is no longer enforced")]
#endif
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir,
		                                              Evidence evidence)
		{
			return DefineDynamicAssembly (name, access, dir, evidence, null, null, null, false);
		}

#if NET_4_0
		[Obsolete ("Declarative security for assembly level is no longer enforced")]
#endif
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access,
		                                              PermissionSet requiredPermissions,
		                                              PermissionSet optionalPermissions,
		                                              PermissionSet refusedPermissions)
		{
			return DefineDynamicAssembly (name, access, null, null, requiredPermissions, optionalPermissions,
				refusedPermissions, false);
		}

#if NET_4_0
		[Obsolete ("Declarative security for assembly level is no longer enforced")]
#endif
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, Evidence evidence,
		                                              PermissionSet requiredPermissions,
		                                              PermissionSet optionalPermissions,
		                                              PermissionSet refusedPermissions)
		{
			return DefineDynamicAssembly (name, access, null, evidence, requiredPermissions, optionalPermissions,
				refusedPermissions, false);
		}

#if NET_4_0
		[Obsolete ("Declarative security for assembly level is no longer enforced")]
#endif
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir,
		                                              PermissionSet requiredPermissions,
		                                              PermissionSet optionalPermissions,
		                                              PermissionSet refusedPermissions)
		{
			return DefineDynamicAssembly (name, access, dir, null, requiredPermissions, optionalPermissions,
				refusedPermissions, false);
		}

#if NET_4_0
		[Obsolete ("Declarative security for assembly level is no longer enforced")]
#endif
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir,
		                                              Evidence evidence,
		                                              PermissionSet requiredPermissions,
		                                              PermissionSet optionalPermissions,
		                                              PermissionSet refusedPermissions)
		{
			return DefineDynamicAssembly (name, access, dir, evidence, requiredPermissions, optionalPermissions,
				refusedPermissions, false);
		}

#if NET_4_0
		[Obsolete ("Declarative security for assembly level is no longer enforced")]
#endif
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir,
		                                              Evidence evidence,
		                                              PermissionSet requiredPermissions,
		                                              PermissionSet optionalPermissions,
		                                              PermissionSet refusedPermissions, bool isSynchronized)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			ValidateAssemblyName (name.Name);

			// FIXME: examine all other parameters
			
			AssemblyBuilder ab = new AssemblyBuilder (name, dir, access, false);
			ab.AddPermissionRequests (requiredPermissions, optionalPermissions, refusedPermissions);
			return ab;
		}

		// NET 3.5 method
#if NET_4_0
		[Obsolete ("Declarative security for assembly level is no longer enforced")]
#endif
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir,
		                                              Evidence evidence,
		                                              PermissionSet requiredPermissions,
		                                              PermissionSet optionalPermissions,
		                                              PermissionSet refusedPermissions, bool isSynchronized, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			AssemblyBuilder ab = DefineDynamicAssembly (name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, isSynchronized);
			if (assemblyAttributes != null)
				foreach (CustomAttributeBuilder cb in assemblyAttributes) {
					ab.SetCustomAttribute (cb);
				}
			return ab;
		}

		// NET 3.5 method
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			return DefineDynamicAssembly (name, access, null, null, null, null, null, false, assemblyAttributes);
		}

#if NET_4_0
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir, bool isSynchronized, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			return DefineDynamicAssembly (name, access, dir, null, null, null, null, isSynchronized, assemblyAttributes);
		}

		[MonoLimitation ("The argument securityContextSource is ignored")]
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, IEnumerable<CustomAttributeBuilder> assemblyAttributes, SecurityContextSource securityContextSource)
		{
			return DefineDynamicAssembly (name, access, assemblyAttributes);
		}
#endif

		internal AssemblyBuilder DefineInternalDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access)
		{
			return new AssemblyBuilder (name, null, access, true);
		}

		//
		// AppDomain.DoCallBack works because AppDomain is a MarshalByRefObject
		// so, when you call AppDomain.DoCallBack, that's a remote call
		//
		public void DoCallBack (CrossAppDomainDelegate callBackDelegate)
		{
			if (callBackDelegate != null)
				callBackDelegate ();
		}

		public int ExecuteAssembly (string assemblyFile)
		{
			return ExecuteAssembly (assemblyFile, (Evidence)null, null);
		}

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity)
		{
			return ExecuteAssembly (assemblyFile, assemblySecurity, null);
		}

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity, string[] args)
		{
			Assembly a = Assembly.LoadFrom (assemblyFile, assemblySecurity);
			return ExecuteAssemblyInternal (a, args);
		}

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity, string[] args, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			Assembly a = Assembly.LoadFrom (assemblyFile, assemblySecurity, hashValue, hashAlgorithm);
			return ExecuteAssemblyInternal (a, args);
		}


#if NET_4_0
		public int ExecuteAssembly (string assemblyFile, string[] args)
		{
			Assembly a = Assembly.LoadFrom (assemblyFile, null);
			return ExecuteAssemblyInternal (a, args);
		}

		public int ExecuteAssembly (string assemblyFile, string[] args, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			Assembly a = Assembly.LoadFrom (assemblyFile, null, hashValue, hashAlgorithm);
			return ExecuteAssemblyInternal (a, args);
		}
#endif

		int ExecuteAssemblyInternal (Assembly a, string[] args)
		{
			if (a.EntryPoint == null)
				throw new MissingMethodException ("Entry point not found in assembly '" + a.FullName + "'.");
			return ExecuteAssembly (a, args);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int ExecuteAssembly (Assembly a, string[] args);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern Assembly [] GetAssemblies (bool refOnly);

		public Assembly [] GetAssemblies ()
		{
			return GetAssemblies (false);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern object GetData (string name);

		public new Type GetType()
		{
			return base.GetType ();
		}

		public override object InitializeLifetimeService ()
		{
			return null;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Assembly LoadAssembly (string assemblyRef, Evidence securityEvidence, bool refOnly);

		public Assembly Load (AssemblyName assemblyRef)
		{
			return Load (assemblyRef, null);
		}

		internal Assembly LoadSatellite (AssemblyName assemblyRef, bool throwOnError)
		{
			if (assemblyRef == null)
				throw new ArgumentNullException ("assemblyRef");

			Assembly result = LoadAssembly (assemblyRef.FullName, null, false);
			if (result == null && throwOnError)
				throw new FileNotFoundException (null, assemblyRef.Name);
			return result;
		}

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public Assembly Load (AssemblyName assemblyRef, Evidence assemblySecurity)
		{
			if (assemblyRef == null)
				throw new ArgumentNullException ("assemblyRef");

			if (assemblyRef.Name == null || assemblyRef.Name.Length == 0) {
				if (assemblyRef.CodeBase != null)
					return Assembly.LoadFrom (assemblyRef.CodeBase, assemblySecurity);
				else
					throw new ArgumentException (Locale.GetText ("assemblyRef.Name cannot be empty."), "assemblyRef");
			}

			Assembly assembly = LoadAssembly (assemblyRef.FullName, assemblySecurity, false);
			if (assembly != null)
				return assembly;

			if (assemblyRef.CodeBase == null)
				throw new FileNotFoundException (null, assemblyRef.Name);

			string cb = assemblyRef.CodeBase;
			if (cb.ToLower (CultureInfo.InvariantCulture).StartsWith ("file://"))
				cb = new Mono.Security.Uri (cb).LocalPath;

			try {
				assembly = Assembly.LoadFrom (cb, assemblySecurity);
			} catch {
				throw new FileNotFoundException (null, assemblyRef.Name);
			}
			AssemblyName aname = assembly.GetName ();
			// Name, version, culture, publickeytoken. Anything else?
			if (assemblyRef.Name != aname.Name)
				throw new FileNotFoundException (null, assemblyRef.Name);

			if (assemblyRef.Version != new Version () && assemblyRef.Version != aname.Version)
				throw new FileNotFoundException (null, assemblyRef.Name);

			if (assemblyRef.CultureInfo != null && assemblyRef.CultureInfo.Equals (aname))
				throw new FileNotFoundException (null, assemblyRef.Name);

			byte [] pt = assemblyRef.GetPublicKeyToken ();
			if (pt != null) {
				byte [] loaded_pt = aname.GetPublicKeyToken ();
				if (loaded_pt == null || (pt.Length != loaded_pt.Length))
					throw new FileNotFoundException (null, assemblyRef.Name);
				for (int i = pt.Length - 1; i >= 0; i--)
					if (loaded_pt [i] != pt [i])
						throw new FileNotFoundException (null, assemblyRef.Name);
			}
			return assembly;
		}

		public Assembly Load (string assemblyString)
		{
			return Load (assemblyString, null, false);
		}

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public Assembly Load (string assemblyString, Evidence assemblySecurity)
		{
			return Load (assemblyString, assemblySecurity, false);
		}
		
		internal Assembly Load (string assemblyString, Evidence assemblySecurity, bool refonly)
		{
			if (assemblyString == null)
				throw new ArgumentNullException ("assemblyString");
				
			if (assemblyString.Length == 0)
				throw new ArgumentException ("assemblyString cannot have zero length");

			Assembly assembly = LoadAssembly (assemblyString, assemblySecurity, refonly);
			if (assembly == null)
				throw new FileNotFoundException (null, assemblyString);
			return assembly;
		}

		public Assembly Load (byte[] rawAssembly)
		{
			return Load (rawAssembly, null, null);
		}

		public Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore)
		{
			return Load (rawAssembly, rawSymbolStore, null);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Assembly LoadAssemblyRaw (byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence, bool refonly);

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence)
		{
			return Load (rawAssembly, rawSymbolStore, securityEvidence, false);
		}

		internal Assembly Load (byte [] rawAssembly, byte [] rawSymbolStore, Evidence securityEvidence, bool refonly)
		{
			if (rawAssembly == null)
				throw new ArgumentNullException ("rawAssembly");

			Assembly assembly = LoadAssemblyRaw (rawAssembly, rawSymbolStore, securityEvidence, refonly);
			assembly.FromByteArray = true;
			return assembly;
		}
#if !MOONLIGHT
#if NET_4_0
		[Obsolete ("AppDomain policy levels are obsolete")]
#endif
		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
		public void SetAppDomainPolicy (PolicyLevel domainPolicy)
		{
			if (domainPolicy == null)
				throw new ArgumentNullException ("domainPolicy");
			if (_granted != null) {
				throw new PolicyException (Locale.GetText (
					"An AppDomain policy is already specified."));
			}
			if (IsFinalizingForUnload ())
				throw new AppDomainUnloadedException ();

			PolicyStatement ps = domainPolicy.Resolve (_evidence);
			_granted = ps.PermissionSet;
		}

		[Obsolete ("Use AppDomainSetup.SetCachePath")]
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void SetCachePath (string path)
		{
			SetupInformationNoCopy.CachePath = path;
		}

		[SecurityPermission (SecurityAction.Demand, ControlPrincipal = true)]
		public void SetPrincipalPolicy (PrincipalPolicy policy)
		{
			if (IsFinalizingForUnload ())
				throw new AppDomainUnloadedException ();

			_principalPolicy = policy;
			_principal = null;
		}

		[Obsolete ("Use AppDomainSetup.ShadowCopyFiles")]
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void SetShadowCopyFiles()
		{
			SetupInformationNoCopy.ShadowCopyFiles = "true";
		}

		[Obsolete ("Use AppDomainSetup.ShadowCopyDirectories")]
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void SetShadowCopyPath (string path)
		{
			SetupInformationNoCopy.ShadowCopyDirectories = path;
		}

		[SecurityPermission (SecurityAction.Demand, ControlPrincipal = true)]
		public void SetThreadPrincipal (IPrincipal principal)
		{
			if (principal == null)
				throw new ArgumentNullException ("principal");
			if (_principal != null)
				throw new PolicyException (Locale.GetText ("principal already present."));
			if (IsFinalizingForUnload ())
				throw new AppDomainUnloadedException ();

			_principal = principal;
		}
#endif
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain InternalSetDomainByID (int domain_id);
 
		// Changes the active domain and returns the old domain
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain InternalSetDomain (AppDomain context);

		// Notifies the runtime that this thread references 'domain'.
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern void InternalPushDomainRef (AppDomain domain);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern void InternalPushDomainRefByID (int domain_id);

		// Undoes the effect of the last PushDomainRef call
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern void InternalPopDomainRef ();

		// Changes the active context and returns the old context
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern Context InternalSetContext (Context context);

		// Returns the current context
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern Context InternalGetContext ();

		// Returns the current context
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern Context InternalGetDefaultContext ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern string InternalGetProcessGuid (string newguid);

		// This method is handled specially by the runtime
		// It is the only managed method which is allowed to set the current
		// appdomain
		internal static object InvokeInDomain (AppDomain domain, MethodInfo method, object obj, object [] args)
		{
			AppDomain current = CurrentDomain;
			bool pushed = false;

			try {
				Exception exc;
				InternalPushDomainRef (domain);
				pushed = true;
				InternalSetDomain (domain);
				object o = ((MonoMethod) method).InternalInvoke (obj, args, out exc);
				if (exc != null)
					throw exc;
				return o;
			}
			finally {
				InternalSetDomain (current);
				if (pushed)
					InternalPopDomainRef ();
			}
		}

		internal static object InvokeInDomainByID (int domain_id, MethodInfo method, object obj, object [] args)
		{
			AppDomain current = CurrentDomain;
			bool pushed = false;

			try {
				Exception exc;
				InternalPushDomainRefByID (domain_id);
				pushed = true;
				InternalSetDomainByID (domain_id);
				object o = ((MonoMethod) method).InternalInvoke (obj, args, out exc);
				if (exc != null)
					throw exc;
				return o;
			}
			finally {
				InternalSetDomain (current);
				if (pushed)
					InternalPopDomainRef ();
			}
		}

		internal static String GetProcessGuid ()
		{
			if (_process_guid == null) {
				_process_guid = InternalGetProcessGuid (Guid.NewGuid().ToString ());
			}
			return _process_guid;
		}

#if !MOONLIGHT

		public static AppDomain CreateDomain (string friendlyName)
		{
			return CreateDomain (friendlyName, null, null);
		}
		
		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo)
		{
			return CreateDomain (friendlyName, securityInfo, null);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain createDomain (string friendlyName, AppDomainSetup info);

		[MonoLimitationAttribute ("Currently it does not allow the setup in the other domain")]
		[SecurityPermission (SecurityAction.Demand, ControlAppDomain = true)]
		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo, AppDomainSetup info)
		{
			if (friendlyName == null)
				throw new System.ArgumentNullException ("friendlyName");

			AppDomain def = AppDomain.DefaultDomain;
			if (info == null) {
				// if null, get default domain's SetupInformation	
				if (def == null)
					info = new AppDomainSetup ();	// we're default!
				else
					info = def.SetupInformation;
			}
			else
				info = new AppDomainSetup (info);	// copy

			// todo: allow setup in the other domain
			if (def != null) {
				if (!info.Equals (def.SetupInformation)) {
					// If not specified use default domain's app base.
					if (info.ApplicationBase == null)
						info.ApplicationBase = def.SetupInformation.ApplicationBase;
					if (info.ConfigurationFile == null)
						info.ConfigurationFile = Path.GetFileName (def.SetupInformation.ConfigurationFile);
				}
			} else if (info.ConfigurationFile == null)
				info.ConfigurationFile = "[I don't have a config file]";

#if !NET_2_1
			if (info.AppDomainInitializer != null) {
				if (!info.AppDomainInitializer.Method.IsStatic)
					throw new ArgumentException ("Non-static methods cannot be invoked as an appdomain initializer");
			}
#endif

			info.SerializeNonPrimitives ();

			AppDomain ad = (AppDomain) RemotingServices.GetDomainProxy (createDomain (friendlyName, info));
			if (securityInfo == null) {
				// get default domain's Evidence (unless we're are the default!)
				if (def == null)
					ad._evidence = null;		// we'll get them later (GetEntryAssembly)
				else
					ad._evidence = def.Evidence;	// new (shallow) copy
			}
			else
				ad._evidence = new Evidence (securityInfo);	// copy

#if !NET_2_1
			if (info.AppDomainInitializer != null) {
				Loader loader = new Loader (
					info.AppDomainInitializer.Method.DeclaringType.Assembly.Location);
				ad.DoCallBack (loader.Load);

				Initializer initializer = new Initializer (
					info.AppDomainInitializer,
					info.AppDomainInitializerArguments);
				ad.DoCallBack (initializer.Initialize);
			}
#endif

			return ad;
		}

#if !NET_2_1
		[Serializable]
		class Loader {

			string assembly;

			public Loader (string assembly)
			{
				this.assembly = assembly;
			}

			public void Load ()
			{
				Assembly.LoadFrom (assembly);
			}
		}

		[Serializable]
		class Initializer {

			AppDomainInitializer initializer;
			string [] arguments;

			public Initializer (AppDomainInitializer initializer, string [] arguments)
			{
				this.initializer = initializer;
				this.arguments = arguments;
			}

			public void Initialize ()
			{
				initializer (arguments);
			}
		}
#endif

		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo,string appBasePath,
		                                      string appRelativeSearchPath, bool shadowCopyFiles)
		{
			return CreateDomain (friendlyName, securityInfo, CreateDomainSetup (appBasePath, appRelativeSearchPath, shadowCopyFiles));
		}
		
#if !NET_2_1
		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo, AppDomainSetup info,
		                                      PermissionSet grantSet, params StrongName [] fullTrustAssemblies)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.ApplicationTrust = new ApplicationTrust (grantSet, fullTrustAssemblies ?? new StrongName [0]);
			return CreateDomain (friendlyName, securityInfo, info);		
		}
#endif

		static AppDomainSetup CreateDomainSetup (string appBasePath, string appRelativeSearchPath, bool shadowCopyFiles)
		{
			AppDomainSetup info = new AppDomainSetup ();

			info.ApplicationBase = appBasePath;
			info.PrivateBinPath = appRelativeSearchPath;

			if (shadowCopyFiles)
				info.ShadowCopyFiles = "true";
			else
				info.ShadowCopyFiles = "false";

			return info;
		}
#endif // !NET_2_1

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern bool InternalIsFinalizingForUnload (int domain_id);

		public bool IsFinalizingForUnload()
		{
			return InternalIsFinalizingForUnload (getDomainID ());
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void InternalUnload (int domain_id);

		// We do this because if the domain is a transparant proxy this
		// will still return the correct domain id.
		private int getDomainID ()
		{
			return Thread.GetDomainID ();
		}

		[SecurityPermission (SecurityAction.Demand, ControlAppDomain = true)]
		[ReliabilityContractAttribute (Consistency.MayCorruptAppDomain, Cer.MayFail)]
		public static void Unload (AppDomain domain)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");

			InternalUnload (domain.getDomainID());
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public extern void SetData (string name, object data);

		[MonoLimitation ("The permission field is ignored")]
		public void SetData (string name, object data, IPermission permission)
		{
			SetData (name, data);
		}

#if !NET_2_1
		[Obsolete ("Use AppDomainSetup.DynamicBase")]
		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void SetDynamicBase (string path)
		{
			SetupInformationNoCopy.DynamicBase = path;
		}
#endif // !NET_2_1

		[Obsolete ("AppDomain.GetCurrentThreadId has been deprecated"
			+ " because it does not provide a stable Id when managed"
			+ " threads are running on fibers (aka lightweight"
			+ " threads). To get a stable identifier for a managed"
			+ " thread, use the ManagedThreadId property on Thread.'")]
		public static int GetCurrentThreadId ()
		{
			return Thread.CurrentThreadId;
		}

		public override string ToString ()
		{
#if !MOONLIGHT
			return getFriendlyName ();
#else
			StringBuilder sb = new StringBuilder ("Name:");
			sb.AppendLine (FriendlyName);
			sb.AppendLine ("There are no context policies.");
			return sb.ToString ();
#endif
		}

		private static void ValidateAssemblyName (string name)
		{
			if (name == null || name.Length == 0)
				throw new ArgumentException ("The Name of " +
					"AssemblyName cannot be null or a " +
					"zero-length string.");

			bool isValid = true;

			for (int i = 0; i < name.Length; i++) {
				char c = name [i];

				// do not allow leading whitespace
				if (i == 0 && char.IsWhiteSpace (c)) {
					isValid = false;
					break;
				}

				// do not allow /,\ or : in name
				if (c == '/' || c == '\\' || c == ':') {
					isValid = false;
					break;
				}
			}

			if (!isValid)
				throw new ArgumentException ("The Name of " +
					"AssemblyName cannot start with " +
					"whitespace, or contain '/', '\\' " +
					" or ':'.");
		}

		// The following methods are called from the runtime. Don't change signatures.
#pragma warning disable 169		
		private void DoAssemblyLoad (Assembly assembly)
		{
			if (AssemblyLoad == null)
				return;

			AssemblyLoad (this, new AssemblyLoadEventArgs (assembly));
		}

		private Assembly DoAssemblyResolve (string name, bool refonly)
		{
			ResolveEventHandler del;
#if !NET_2_1
			if (refonly)
				del = ReflectionOnlyAssemblyResolve;
			else
				del = AssemblyResolve;
#else
			del = AssemblyResolve;
#endif
			if (del == null)
				return null;
			
			/* Prevent infinite recursion */
			Hashtable ht;
			if (refonly) {
				ht = assembly_resolve_in_progress_refonly;
				if (ht == null) {
					ht = new Hashtable ();
					assembly_resolve_in_progress_refonly = ht;
				}
			} else {
				ht = assembly_resolve_in_progress;
				if (ht == null) {
					ht = new Hashtable ();
					assembly_resolve_in_progress = ht;
				}
			}

			string s = (string) ht [name];
			if (s != null)
				return null;
			ht [name] = name;
			try {
				Delegate[] invocation_list = del.GetInvocationList ();

				foreach (Delegate eh in invocation_list) {
					ResolveEventHandler handler = (ResolveEventHandler) eh;
					Assembly assembly = handler (this, new ResolveEventArgs (name));
					if (assembly != null)
						return assembly;
				}
				return null;
			}
			finally {
				ht.Remove (name);
			}
		}

		internal Assembly DoTypeResolve (Object name_or_tb)
		{
			if (TypeResolve == null)
				return null;

			string name;

			if (name_or_tb is TypeBuilder)
				name = ((TypeBuilder) name_or_tb).FullName;
			else
				name = (string) name_or_tb;

			/* Prevent infinite recursion */
			Hashtable ht = type_resolve_in_progress;
			if (ht == null) {
				ht = new Hashtable ();
				type_resolve_in_progress = ht;
			}

			if (ht.Contains (name))
				return null;
			else
				ht [name] = name;

			try {
				foreach (Delegate d in TypeResolve.GetInvocationList ()) {
					ResolveEventHandler eh = (ResolveEventHandler) d;
					Assembly assembly = eh (this, new ResolveEventArgs (name));
					if (assembly != null)
						return assembly;
				}
				return null;
			}
			finally {
				ht.Remove (name);
			}
		}

		private void DoDomainUnload ()
		{
			if (DomainUnload != null)
				DomainUnload(this, null);
		}

#if !NET_2_1
		internal byte[] GetMarshalledDomainObjRef ()
		{
			ObjRef oref = RemotingServices.Marshal (AppDomain.CurrentDomain, null, typeof (AppDomain));
			return CADSerializer.SerializeObject (oref).GetBuffer();
		}
#endif
		internal void ProcessMessageInDomain (byte[] arrRequest, CADMethodCallMessage cadMsg,
		                                      out byte[] arrResponse, out CADMethodReturnMessage cadMrm)
		{
			IMessage reqDomMsg;

			if (null != arrRequest)
				reqDomMsg = CADSerializer.DeserializeMessage (new MemoryStream(arrRequest), null);
			else
				reqDomMsg = new MethodCall (cadMsg);

			IMessage retDomMsg = ChannelServices.SyncDispatchMessage (reqDomMsg);

			cadMrm = CADMethodReturnMessage.Create (retDomMsg);
			if (null == cadMrm) {
				arrResponse = CADSerializer.SerializeMessage (retDomMsg).GetBuffer();
			} 
			else
				arrResponse = null;
		}

#pragma warning restore 169

		// End of methods called from the runtime
		
		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public event AssemblyLoadEventHandler AssemblyLoad;

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public event ResolveEventHandler AssemblyResolve;

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public event EventHandler DomainUnload;

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public event EventHandler ProcessExit;

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public event ResolveEventHandler ResourceResolve;

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public event ResolveEventHandler TypeResolve;

		[method: SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		public event UnhandledExceptionEventHandler UnhandledException;

#if NET_4_0 || BOOTSTRAP_NET_4_0
		[MonoTODO]
		public bool IsHomogenous {
			get { return true; }
		}

		[MonoTODO]
		public bool IsFullyTrusted {
			get { return true; }
		}
#endif

        #pragma warning disable 649
		private AppDomainManager _domain_manager;
        #pragma warning restore 649

		// default is null
		public AppDomainManager DomainManager {
			get { return _domain_manager; }
		}

#if (!MOONLIGHT)

		public event ResolveEventHandler ReflectionOnlyAssemblyResolve;

        #pragma warning disable 649
		private ActivationContext _activation;
		private ApplicationIdentity _applicationIdentity;
        #pragma warning restore 649

		// properties

		public ActivationContext ActivationContext {
			get { return _activation; }
		}

		public ApplicationIdentity ApplicationIdentity {
			get { return _applicationIdentity; }
		}

		public int Id {
			[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
			get { return getDomainID (); }
		}

		// methods

		[MonoTODO ("This routine only returns the parameter currently")]
		[ComVisible (false)]
		public string ApplyPolicy (string assemblyName)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");
			if (assemblyName.Length == 0) // String.Empty
				throw new ArgumentException ("assemblyName");
			return assemblyName;
		}

		// static methods

		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo, string appBasePath,
			string appRelativeSearchPath, bool shadowCopyFiles, AppDomainInitializer adInit, string[] adInitArgs)
		{
			AppDomainSetup info = CreateDomainSetup (appBasePath, appRelativeSearchPath, shadowCopyFiles);

			info.AppDomainInitializerArguments = adInitArgs;
			info.AppDomainInitializer = adInit;

			return CreateDomain (friendlyName, securityInfo, info);
		}

		public int ExecuteAssemblyByName (string assemblyName)
		{
			return ExecuteAssemblyByName (assemblyName, (Evidence)null, null);
		}

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public int ExecuteAssemblyByName (string assemblyName, Evidence assemblySecurity)
		{
			return ExecuteAssemblyByName (assemblyName, assemblySecurity, null);
		}

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public int ExecuteAssemblyByName (string assemblyName, Evidence assemblySecurity, params string[] args)
		{
			Assembly a = Assembly.Load (assemblyName, assemblySecurity);

			return ExecuteAssemblyInternal (a, args);
		}

#if NET_4_0
		[Obsolete ("Use an overload that does not take an Evidence parameter")]
#endif
		public int ExecuteAssemblyByName (AssemblyName assemblyName, Evidence assemblySecurity, params string[] args)
		{
			Assembly a = Assembly.Load (assemblyName, assemblySecurity);

			return ExecuteAssemblyInternal (a, args);
		}

#if NET_4_0
		public int ExecuteAssemblyByName (string assemblyName, params string[] args)
		{
			Assembly a = Assembly.Load (assemblyName, null);

			return ExecuteAssemblyInternal (a, args);
		}

		public int ExecuteAssemblyByName (AssemblyName assemblyName, params string[] args)
		{
			Assembly a = Assembly.Load (assemblyName, null);

			return ExecuteAssemblyInternal (a, args);
		}
#endif

		public bool IsDefaultAppDomain ()
		{
			return Object.ReferenceEquals (this, DefaultDomain);
		}

		public Assembly[] ReflectionOnlyGetAssemblies ()
		{
			return GetAssemblies (true);
		}

#else // MOONLIGHT

		public int ExecuteAssemblyByName (string assemblyName)
		{
			// critical code in SL that we're not calling in ML
			throw new NotImplementedException ();
		}
#endif

#if !NET_2_1
		void _AppDomain.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _AppDomain.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _AppDomain.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _AppDomain.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
#endif

#if NET_4_0 || MOONLIGHT
		[MonoTODO ("Currently always returns false")]
		public bool? IsCompatibilitySwitchSet (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			// default (at least for SL4) is to return false for unknown values (can't get a null out of it)
			return false;
		}

		[MonoTODO ("Currently always returns false")]
		public static bool MonitoringIsEnabled { 
			get { return false; }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public long MonitoringSurvivedMemorySize {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public static long MonitoringSurvivedProcessMemorySize {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public long MonitoringTotalAllocatedMemorySize {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public TimeSpan MonitoringTotalProcessorTime {
			get { throw new NotImplementedException (); }
		}
#endif
	}
}
