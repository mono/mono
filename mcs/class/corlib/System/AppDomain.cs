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

namespace System
{
	[ClassInterface(ClassInterfaceType.None)]
	public sealed class AppDomain : MarshalByRefObject , _AppDomain , IEvidenceFactory
	{
		IntPtr _mono_app_domain;
		static string _process_guid;

		[ThreadStatic]
		static Hashtable type_resolve_in_progress;

		[ThreadStatic]
		static Hashtable assembly_resolve_in_progress;

		private PrincipalPolicy _principalPolicy;

		[ThreadStatic]
		private static IPrincipal _principal;

		private AppDomain ()
		{
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern AppDomainSetup getSetup ();

		AppDomainSetup SetupInformationNoCopy {
			get { return getSetup (); }
		}

		public AppDomainSetup SetupInformation {
			get {
				AppDomainSetup setup = getSetup ();
				return new AppDomainSetup (setup);
			}
		}

		public string BaseDirectory {
			get {
				return SetupInformationNoCopy.ApplicationBase;
			}
		}

		public string RelativeSearchPath {
			get {
				return SetupInformationNoCopy.PrivateBinPath;
			}
		}

		public string DynamicDirectory {
			get {
				AppDomainSetup setup = SetupInformationNoCopy;
				if (setup.DynamicBase == null) return null;
				return Path.Combine (setup.DynamicBase, setup.ApplicationName);
			}
		}

		public bool ShadowCopyFiles {
			get {
				return (SetupInformationNoCopy.ShadowCopyFiles == "true");
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string getFriendlyName ();

		public string FriendlyName {
			get {
				return getFriendlyName ();
			}
		}

		[MonoTODO ("Return evidence")]
		public Evidence Evidence {
			get {
				return null;
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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain getCurDomain ();
		
		public static AppDomain CurrentDomain {
			get {
				return getCurDomain ();
			}
		}

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

		public void ClearPrivatePath ()
		{
			SetupInformationNoCopy.PrivateBinPath = String.Empty;
		}

		public void ClearShadowCopyPath ()
		{
			SetupInformationNoCopy.ShadowCopyDirectories = String.Empty;
		}

		public ObjectHandle CreateComInstanceFrom (string assemblyName, string typeName)
		{
			return Activator.CreateComInstanceFrom (assemblyName, typeName);
		}

#if NET_1_1
		public static ObjectHandle CreateComInstanceFrom (string assemblyName, string typeName,
		                                                  byte [] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			return Activator.CreateComInstanceFrom (assemblyName, typeName, hashValue ,hashAlgorithm);
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

		public object CreateInstanceAndUnwrap (string assemblyName, string typeName, bool ignoreCase,
		                                       BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture,
		                                       object[] activationAttributes, Evidence securityAttributes)
		{
			ObjectHandle oh = CreateInstance (assemblyName, typeName, ignoreCase, bindingAttr, binder, args,
				culture, activationAttributes, securityAttributes);
			return (oh != null) ? oh.Unwrap () : null;
		}

		public ObjectHandle CreateInstanceFrom (string assemblyName, string typeName)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstanceFrom (assemblyName, typeName);
		}

		public ObjectHandle CreateInstanceFrom (string assemblyName, string typeName, object[] activationAttributes)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstanceFrom (assemblyName, typeName, activationAttributes);
		}

		public ObjectHandle CreateInstanceFrom (string assemblyName, string typeName, bool ignoreCase,
		                                        BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture,
		                                        object[] activationAttributes, Evidence securityAttributes)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstanceFrom (assemblyName, typeName, ignoreCase, bindingAttr, binder, args,
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

		public object CreateInstanceFromAndUnwrap (string assemblyName, string typeName, bool ignoreCase,
		                                           BindingFlags bindingAttr, Binder binder, object[] args,
		                                           CultureInfo culture, object[] activationAttributes,
		                                           Evidence securityAttributes)
		{
			ObjectHandle oh = CreateInstanceFrom (assemblyName, typeName, ignoreCase, bindingAttr, binder, args,
				culture, activationAttributes, securityAttributes);

			return (oh != null) ? oh.Unwrap () : null;
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access)
		{
			return DefineDynamicAssembly (name, access, null, null, null, null, null, false);
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, Evidence evidence)
		{
			return DefineDynamicAssembly (name, access, null, evidence, null, null, null, false);
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir)
		{
			return DefineDynamicAssembly (name, access, dir, null, null, null, null, false);
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir,
		                                              Evidence evidence)
		{
			return DefineDynamicAssembly (name, access, dir, evidence, null, null, null, false);
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access,
		                                              PermissionSet requiredPermissions,
		                                              PermissionSet optionalPermissions,
		                                              PermissionSet refusedPermissions)
		{
			return DefineDynamicAssembly (name, access, null, null, requiredPermissions, optionalPermissions,
				refusedPermissions, false);
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, Evidence evidence,
		                                              PermissionSet requiredPermissions,
		                                              PermissionSet optionalPermissions,
		                                              PermissionSet refusedPermissions)
		{
			return DefineDynamicAssembly (name, access, null, evidence, requiredPermissions, optionalPermissions,
				refusedPermissions, false);
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir,
		                                              PermissionSet requiredPermissions,
		                                              PermissionSet optionalPermissions,
		                                              PermissionSet refusedPermissions)
		{
			return DefineDynamicAssembly (name, access, dir, null, requiredPermissions, optionalPermissions,
				refusedPermissions, false);
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir,
		                                              Evidence evidence,
		                                              PermissionSet requiredPermissions,
		                                              PermissionSet optionalPermissions,
		                                              PermissionSet refusedPermissions)
		{
			return DefineDynamicAssembly (name, access, dir, evidence, requiredPermissions, optionalPermissions,
				refusedPermissions, false);
		}

		[MonoTODO ("FIXME: examine all other parameters")]
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, string dir,
		                                              Evidence evidence,
		                                              PermissionSet requiredPermissions,
		                                              PermissionSet optionalPermissions,
		                                              PermissionSet refusedPermissions, bool isSynchronized)
		{
			// FIXME: examine all other parameters
			
			AssemblyBuilder ab = new AssemblyBuilder (name, dir, access);
			return ab;
		}

		public void DoCallBack (CrossAppDomainDelegate theDelegate)
		{
			if (theDelegate != null)
				theDelegate ();
		}

		public int ExecuteAssembly (string assemblyFile)
		{
			return ExecuteAssembly (assemblyFile, new Evidence (), null);
		}

		public int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity)
		{
			return ExecuteAssembly (assemblyFile, new Evidence (), null);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity, string[] args);

		[MonoTODO]
		public int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity, string[] args, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			throw new NotImplementedException ();
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern Assembly [] GetAssemblies ();

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
		private extern Assembly LoadAssembly (string assemblyRef, Evidence securityEvidence);

		public Assembly Load (AssemblyName assemblyRef)
		{
			return Load (assemblyRef, new Evidence ());
		}

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

			return LoadAssembly (assemblyRef.FullName, assemblySecurity);
		}

		public Assembly Load (string assemblyString)
		{
			if (assemblyString == null)
				throw new ArgumentNullException ("assemblyString");

			return LoadAssembly (assemblyString, new Evidence ());
		}

		public Assembly Load (string assemblyString, Evidence assemblySecurity)
		{
			if (assemblyString == null)
				throw new ArgumentNullException ("assemblyString");

			return LoadAssembly (assemblyString, assemblySecurity);
		}

		public Assembly Load (byte[] rawAssembly)
		{
			return Load (rawAssembly, null, new Evidence ());
		}

		public Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore)
		{
			return Load (rawAssembly, rawSymbolStore, new Evidence ());
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern Assembly LoadAssemblyRaw (byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence);

		public Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence)
		{
			if (rawAssembly == null)
				throw new ArgumentNullException ("rawAssembly");
				
			return LoadAssemblyRaw (rawAssembly, rawSymbolStore, securityEvidence);
		}

		[MonoTODO]
		public void SetAppDomainPolicy (PolicyLevel domainPolicy)
		{
			throw new NotImplementedException ();
		}

		public void SetCachePath (string path)
		{
			SetupInformationNoCopy.CachePath = path;
		}

		public void SetPrincipalPolicy (PrincipalPolicy policy)
		{
			new SecurityPermission (SecurityPermissionFlag.ControlPrincipal).Demand ();
			if (IsFinalizingForUnload ())
				throw new AppDomainUnloadedException ();

			_principalPolicy = policy;
			_principal = null;
		}

		public void SetShadowCopyFiles()
		{
			SetupInformationNoCopy.ShadowCopyFiles = "true";
		}

		public void SetShadowCopyPath (string path)
		{
			SetupInformationNoCopy.ShadowCopyDirectories = path;
		}

		public void SetThreadPrincipal (IPrincipal principal)
		{
			new SecurityPermission (SecurityPermissionFlag.ControlPrincipal).Demand ();
			if (principal == null)
				throw new ArgumentNullException ("principal");
			if (_principal != null)
				throw new PolicyException (Locale.GetText ("principal already present."));
			if (IsFinalizingForUnload ())
				throw new AppDomainUnloadedException ();

			_principal = principal;
		}

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
				InternalPushDomainRef (domain);
				pushed = true;
				InternalSetDomain (domain);
				return ((MonoMethod) method).InternalInvoke (obj, args);
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
				InternalPushDomainRefByID (domain_id);
				pushed = true;
				InternalSetDomainByID (domain_id);
				return ((MonoMethod) method).InternalInvoke (obj, args);
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

		[MonoTODO ("allow setup in the other domain")]
		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo, AppDomainSetup info)
		{
			if (friendlyName == null)
				throw new System.ArgumentNullException ("friendlyName");

			if (info == null)
				throw new ArgumentNullException ("info");

			// todo: allow setup in the other domain

			return (AppDomain) RemotingServices.GetDomainProxy (createDomain (friendlyName, info));
		}

		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo,string appBasePath,
		                                      string appRelativeSearchPath, bool shadowCopyFiles)
		{
			AppDomainSetup info = new AppDomainSetup ();

			info.ApplicationBase = appBasePath;
			info.PrivateBinPath = appRelativeSearchPath;

			if (shadowCopyFiles)
				info.ShadowCopyFiles = "true";
			else
				info.ShadowCopyFiles = "false";

			return CreateDomain (friendlyName, securityInfo, info);
		}

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

		public static void Unload (AppDomain domain)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");

			InternalUnload (domain.getDomainID());
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern void SetData (string name, object data);

		public void SetDynamicBase (string path)
		{
			SetupInformationNoCopy.DynamicBase = path;
		}

#if NET_2_0
		[Obsolete ("")]
#endif
		public static int GetCurrentThreadId ()
		{
			return Thread.CurrentThreadId;
		}

		public override string ToString ()
		{
			return getFriendlyName ();
		}

		// The following methods are called from the runtime. Don't change signatures.
		private void DoAssemblyLoad (Assembly assembly)
		{
			if (AssemblyLoad == null)
				return;

			AssemblyLoad (this, new AssemblyLoadEventArgs (assembly));
		}

		private Assembly DoAssemblyResolve (string name)
		{
			if (AssemblyResolve == null)
				return null;
			
			/* Prevent infinite recursion */
			Hashtable ht = assembly_resolve_in_progress;
			if (ht == null) {
				ht = new Hashtable ();
				assembly_resolve_in_progress = ht;
			}

			if (ht.Contains (name))
				return null;
			else
				ht [name] = name;

			try {
				foreach (Delegate eh in AssemblyResolve.GetInvocationList ()) {
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

		internal byte[] GetMarshalledDomainObjRef ()
		{
			ObjRef oref = RemotingServices.Marshal (AppDomain.CurrentDomain, null, typeof (AppDomain));
			return CADSerializer.SerializeObject (oref).GetBuffer();
		}

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

		// End of methods called from the runtime
		
		public event AssemblyLoadEventHandler AssemblyLoad;

		public event ResolveEventHandler AssemblyResolve;

		public event EventHandler DomainUnload;

		public event EventHandler ProcessExit;

		public event ResolveEventHandler ResourceResolve;

		public event ResolveEventHandler TypeResolve;

		public event UnhandledExceptionEventHandler UnhandledException;
	}
}
