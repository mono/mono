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
		Hashtable type_resolve_in_progress;

		[ThreadStatic]
		Hashtable assembly_resolve_in_progress;

		// CAS
		private Evidence _evidence;
		private PermissionSet _granted;
		internal PermissionSet _refused;

		// non-CAS
		private PrincipalPolicy _principalPolicy;

		[ThreadStatic]
		private static IPrincipal _principal;

		private AppDomain ()
		{
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern AppDomainSetup getSetup ();

		public AppDomainSetup SetupInformation {
			get {
				AppDomainSetup setup = getSetup ();
				if (setup == null)
					return null;
				return new AppDomainSetup (setup);
			}
		}

		public string BaseDirectory {
			get {
				return SetupInformation.ApplicationBase;
			}
		}

		public string RelativeSearchPath {
			get {
				return SetupInformation.PrivateBinPath;
			}
		}

		public string DynamicDirectory {
			get {
				if (SetupInformation.DynamicBase == null) return null;
				return Path.Combine (SetupInformation.DynamicBase, SetupInformation.ApplicationName);
			}
		}

		public bool ShadowCopyFiles {
			get {
				return (SetupInformation.ShadowCopyFiles == "true");
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string getFriendlyName ();

		public string FriendlyName {
			get {
				return getFriendlyName ();
			}
		}

		public Evidence Evidence {
			get {
				// if the host (runtime) hasn't provided it's own evidence...
				if (_evidence == null) {
					// ... we will provide our own
					lock (this) {
						// the executed assembly from the "default" appdomain
						// or null if we're not in the default appdomain
						Assembly a = Assembly.GetEntryAssembly ();
						if (a == null)
							_evidence = AppDomain.DefaultDomain.Evidence;
						else
							_evidence = Evidence.GetDefaultHostEvidence (a);
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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain getCurDomain ();
		
		public static AppDomain CurrentDomain {
			get {
				return getCurDomain ();
			}
		}

		// Get an AppDomain by it's ID (required to find the "default" app domain)
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain getDomainByID (int domain_id);

		internal static AppDomain DefaultDomain {
			get {
				return getDomainByID (0);
			}
		}

#if NET_2_0
		[Obsolete ("")]
#endif
		public void AppendPrivatePath (string path)
		{
			if (path == null || path.Length == 0)
				return;

			AppDomainSetup setup = SetupInformation;

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

#if NET_2_0
		[Obsolete ("")]
#endif
		public void ClearPrivatePath ()
		{
			SetupInformation.PrivateBinPath = String.Empty;
		}

		public void ClearShadowCopyPath ()
		{
			SetupInformation.ShadowCopyDirectories = String.Empty;
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
			ab.AddPermissionRequests (requiredPermissions, optionalPermissions, refusedPermissions);
			return ab;
		}

		public void DoCallBack (CrossAppDomainDelegate theDelegate)
		{
			if (theDelegate != null)
				theDelegate ();
		}

		public int ExecuteAssembly (string assemblyFile)
		{
			return ExecuteAssembly (assemblyFile, null, null);
		}

		public int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity)
		{
			return ExecuteAssembly (assemblyFile, assemblySecurity, null);
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
			return Load (assemblyRef, null);
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

			return LoadAssembly (assemblyString, null);
		}

		public Assembly Load (string assemblyString, Evidence assemblySecurity)
		{
			if (assemblyString == null)
				throw new ArgumentNullException ("assemblyString");

			return LoadAssembly (assemblyString, assemblySecurity);
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
		private extern Assembly LoadAssemblyRaw (byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence);

		public Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence)
		{
			if (rawAssembly == null)
				throw new ArgumentNullException ("rawAssembly");
				
			return LoadAssemblyRaw (rawAssembly, rawSymbolStore, securityEvidence);
		}

		[SecurityPermission (SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
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

			_granted = SecurityManager.ResolvePolicy (_evidence);
		}

		public void SetCachePath (string path)
		{
			SetupInformation.CachePath = path;
		}

		[SecurityPermission (SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
		public void SetPrincipalPolicy (PrincipalPolicy policy)
		{
			if (IsFinalizingForUnload ())
				throw new AppDomainUnloadedException ();

			_principalPolicy = policy;
			_principal = null;
		}

		public void SetShadowCopyFiles()
		{
			SetupInformation.ShadowCopyFiles = "true";
		}

		public void SetShadowCopyPath (string path)
		{
			SetupInformation.ShadowCopyDirectories = path;
		}

		[SecurityPermission (SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
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

			if (info == null) {
				// if null, get default domain's SetupInformation
				AppDomain def = AppDomain.DefaultDomain;
				if (def == null)
					info = new AppDomainSetup ();	// we're default!
				else
					info = def.SetupInformation;
			}
			else
				info = new AppDomainSetup (info);	// copy

			// todo: allow setup in the other domain

			AppDomain ad = (AppDomain) RemotingServices.GetDomainProxy (createDomain (friendlyName, info));
			if (securityInfo == null) {
				// get default domain's Evidence (unless we're are the default!)
				AppDomain def = AppDomain.DefaultDomain; 
				if (def == null)
					ad._evidence = null;		// we'll get them later (GetEntryAssembly)
				else
					ad._evidence = def.Evidence;	// new (shallow) copy
			}
			else
				ad._evidence = new Evidence (securityInfo);	// copy

			return ad;
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
			SetupInformation.DynamicBase = path;
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

#if NET_2_0
		private ActivationContext _activation;
		private AppDomainManager _domain_manager;

		// properties

		public ActivationContext ActivationContext {
			get { return _activation; }
		}

		// default is null
		public AppDomainManager DomainManager {
			get { return _domain_manager; }
		}

		public int Id {
			get { return getDomainID (); }
		}

		// methods

		[MonoTODO ("what's the policy affecting names ?")]
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

		[MonoTODO]
		// LAMESPEC: Only the fist argument (full path to application) is documented
		public static bool Activate (string[] args)
		{
			if (args == null)
				throw new ArgumentNullException ("args");
			// TODO - what class implements IApplicationDescription ?
			return ActivateNewProcess (null);
		}

		[MonoTODO]
		public static bool ActivateNewProcess (IApplicationDescription appDescription)
		{
			if (appDescription == null)
				throw new ArgumentNullException ("appDescription");
			return false;
		}

		[MonoTODO ("add support for new delegate")]
		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo, string appBasePath,
			string appRelativeSearchPath, bool shadowCopy, AppDomainInitializer adInit, string[] adInitArgs)
		{
			return CreateDomain (friendlyName, securityInfo, appBasePath, appRelativeSearchPath, shadowCopy);
		}

		[MonoTODO ("resolve assemblyName to location")]
		public int ExecuteAssemblyByName (string assemblyName)
		{
			return ExecuteAssemblyByName (assemblyName, null, null);
		}

		[MonoTODO ("resolve assemblyName to location")]
		public int ExecuteAssemblyByName (string assemblyName, Evidence assemblySecurity)
		{
			return ExecuteAssemblyByName (assemblyName, assemblySecurity, null);
		}

		[MonoTODO ("resolve assemblyName to location")]
		public int ExecuteAssemblyByName (string assemblyName, Evidence assemblySecurity, string[] args)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			AssemblyName an = new AssemblyName (assemblyName);
			return ExecuteAssemblyByName (an, assemblySecurity, args);
		}

		[MonoTODO ("assemblyName may not have a codebase")]
		public int ExecuteAssemblyByName (AssemblyName assemblyName, Evidence assemblySecurity, string[] args)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return ExecuteAssembly (assemblyName.CodeBase, assemblySecurity, args);
		}

		public bool IsDefaultAppDomain ()
		{
			return (Id == 0);
		}

		[MonoTODO ("see Assembly.ReflectionOnlyLoad")]
		public Assembly[] ReflectionOnlyGetAssemblies ()
		{
			return new Assembly [0];
		}
#endif
	}
}
