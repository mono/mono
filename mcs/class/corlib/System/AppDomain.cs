//
// System/AppDomain.cs
//
// Authors:
//   Paolo Molaro (lupus@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Gonzalo Paniagua (gonzalo@ximian.com)
//   Patrik Torstensson
//
// (C) 2001, 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
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
using System.Security.Principal;
using System.Security.Policy;
using System.Security;

namespace System {

	[ClassInterface(ClassInterfaceType.None)]
	public sealed class AppDomain : MarshalByRefObject , _AppDomain , IEvidenceFactory {

		IntPtr _mono_app_domain;

		// Evidence evidence;

		AppDomain () {}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern AppDomainSetup getSetup ();

		public AppDomainSetup SetupInformation {

			get {
				return getSetup ();
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
				// fixme: dont know if this is right?
				return SetupInformation.DynamicBase;
			}
		}

		public bool ShadowCopyFiles {

			get {
				return (SetupInformation.ShadowCopyFiles == "true");
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern string getFriendlyName ();

		public string FriendlyName {

			get {
				return getFriendlyName ();
			}
		}

		public Evidence Evidence {

			get {
				return null;
				//return evidence;
			}
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain getCurDomain ();
		
		public static AppDomain CurrentDomain
		{
			get {
				return getCurDomain ();
			}
		}

		public void AppendPrivatePath (string path)
		{
			if (path == null || path == "")
				return;

			AppDomainSetup setup = SetupInformation;

			string pp = setup.PrivateBinPath;
			if (pp == null || pp == "") {
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
			SetupInformation.PrivateBinPath = "";
		}
		
		public void ClearShadowCopyPath ()
		{
			SetupInformation.ShadowCopyDirectories = "";
		}

		[MonoTODO]
		public ObjectHandle CreateComInstanceFrom (string assemblyName,
							   string typeName)
		{
			if(assemblyName==null) {
				throw new ArgumentNullException("assemblyName is null");
			}
			if(typeName==null) {
				throw new ArgumentNullException("typeName is null");
			}
			
			throw new NotImplementedException();
		}
		

		public ObjectHandle CreateInstance (string assemblyName, string typeName)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstance (assemblyName, typeName);
		}

		public ObjectHandle CreateInstance (string assemblyName, string typeName,
						    object[] activationAttributes)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstance (assemblyName, typeName, activationAttributes);
		}
		
		public ObjectHandle CreateInstance (string assemblyName,
						    string typeName,
						    bool ignoreCase,
						    BindingFlags bindingAttr,
						    Binder binder,
						    object[] args,
						    CultureInfo culture,
						    object[] activationAttributes,
						    Evidence securityAttributes)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstance (assemblyName,
							 typeName,
							 ignoreCase,
							 bindingAttr,
							 binder,
							 args,
							 culture,
							 activationAttributes,
							 securityAttributes);
		}

		public object CreateInstanceAndUnwrap (string assemblyName, string typeName)
		{
			ObjectHandle oh = CreateInstance (assemblyName, typeName);
			return (oh != null) ? oh.Unwrap () : null;
		}
		
		public object CreateInstanceAndUnwrap (string assemblyName,
						       string typeName,
						       object [] activationAttributes)
		{
			ObjectHandle oh = CreateInstance (assemblyName, typeName, activationAttributes);
			return (oh != null) ? oh.Unwrap () : null;
		}

		public object CreateInstanceAndUnwrap (string assemblyName,
						       string typeName,
						       bool ignoreCase,
						       BindingFlags bindingAttr,
						       Binder binder,
						       object[] args,
						       CultureInfo culture,
						       object[] activationAttributes,
						       Evidence securityAttributes)
		{
			ObjectHandle oh = CreateInstance (assemblyName,
							  typeName,
							  ignoreCase,
							  bindingAttr,
							  binder,
							  args,
							  culture,
							  activationAttributes,
							  securityAttributes);
			return (oh != null) ? oh.Unwrap () : null;
		}

		public ObjectHandle CreateInstanceFrom (string assemblyName, string typeName)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstanceFrom (assemblyName, typeName);
		}
		
		public ObjectHandle CreateInstanceFrom (string assemblyName, string typeName,
							object[] activationAttributes)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstanceFrom (assemblyName, typeName, activationAttributes);
		}
		
		public ObjectHandle CreateInstanceFrom (string assemblyName,
							string typeName,
							bool ignoreCase,
							BindingFlags bindingAttr,
							Binder binder,
							object[] args,
							CultureInfo culture,
							object[] activationAttributes,
							Evidence securityAttributes)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			return Activator.CreateInstanceFrom (assemblyName,
							     typeName,
							     ignoreCase,
							     bindingAttr,
							     binder,
							     args,
							     culture,
							     activationAttributes,
							     securityAttributes);
		}

		public object CreateInstanceFromAndUnwrap (string assemblyName, string typeName)
		{
			ObjectHandle oh = CreateInstanceFrom (assemblyName, typeName);
			return (oh != null) ? oh.Unwrap () : null;
		}
		
		public object CreateInstanceFromAndUnwrap (string assemblyName,
							   string typeName,
							   object [] activationAttributes)
		{
			ObjectHandle oh = CreateInstanceFrom (assemblyName, typeName, activationAttributes);
			return (oh != null) ? oh.Unwrap () : null;
		}

		public object CreateInstanceFromAndUnwrap (string assemblyName,
							   string typeName,
							   bool ignoreCase,
							   BindingFlags bindingAttr,
							   Binder binder,
							   object[] args,
							   CultureInfo culture,
							   object[] activationAttributes,
							   Evidence securityAttributes)
		{
			ObjectHandle oh = CreateInstanceFrom (assemblyName,
							      typeName,
							      ignoreCase,
							      bindingAttr,
							      binder,
							      args,
							      culture,
							      activationAttributes,
							      securityAttributes);
			return (oh != null) ? oh.Unwrap () : null;
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access)
		{
			return DefineDynamicAssembly (name, access, null, null,
						      null, null, null, false);
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      Evidence evidence)
		{
			return DefineDynamicAssembly (name, access, null, evidence,
						      null, null, null, false);
		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      string dir)
		{
			return DefineDynamicAssembly (name, access, dir, null,
						      null, null, null, false);
		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      string dir,
							      Evidence evidence)
		{
			return DefineDynamicAssembly (name, access, dir, evidence,
						      null, null, null, false);
		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      PermissionSet requiredPermissions,
							      PermissionSet optionalPermissions,
							      PermissionSet refusedPersmissions)
		{
			return DefineDynamicAssembly (name, access, null, null,
						      requiredPermissions, optionalPermissions,
						      refusedPersmissions, false);
		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      Evidence evidence,
							      PermissionSet requiredPermissions,
							      PermissionSet optionalPermissions,
							      PermissionSet refusedPersmissions)
		{
			return DefineDynamicAssembly (name, access, null, evidence,
						      requiredPermissions, optionalPermissions,
						      refusedPersmissions, false);
		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      string dir,
							      PermissionSet requiredPermissions,
							      PermissionSet optionalPermissions,
							      PermissionSet refusedPersmissions)
		{
			return DefineDynamicAssembly (name, access, dir, null,
						      requiredPermissions, optionalPermissions,
						      refusedPersmissions, false);
		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      string dir,
							      Evidence evidence,
							      PermissionSet requiredPermissions,
							      PermissionSet optionalPermissions,
							      PermissionSet refusedPersmissions)
		{
			return DefineDynamicAssembly (name, access, dir, evidence,
						      requiredPermissions, optionalPermissions,
						      refusedPersmissions, false);

		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      string dir,
							      Evidence evidence,
							      PermissionSet requiredPermissions,
							      PermissionSet optionalPermissions,
							      PermissionSet refusedPersmissions,
							      bool isSynchronized)
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
		
		public override bool Equals (object other)
		{
			if (!(other is AppDomain))
				return false;

			return this._mono_app_domain == ((AppDomain)other)._mono_app_domain;
		}

		public int ExecuteAssembly (string assemblyFile)
		{
			return ExecuteAssembly (assemblyFile, new Evidence (), null);
		}
		
		public int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity)
		{
			return ExecuteAssembly (assemblyFile, new Evidence (), null);
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity, string[] args);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern Assembly [] GetAssemblies ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern object GetData (string name);
		
		public override int GetHashCode ()
		{
			return (int)_mono_app_domain;
		}

		public new Type GetType()
		{
			return base.GetType ();
		}
		
		public override object InitializeLifetimeService ()
		{
			return null;
		}

		[MonoTODO]
		public bool IsFinalizingForUnload()
		{
			throw new NotImplementedException();
		}
	
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern Assembly LoadAssembly (AssemblyName assemblyRef, Evidence securityEvidence);

		public Assembly Load (AssemblyName assemblyRef)
		{
			return Load (assemblyRef, new Evidence ());
		}

		public Assembly Load (AssemblyName assemblyRef, Evidence assemblySecurity)
		{
			return LoadAssembly (assemblyRef, assemblySecurity);
		}

		public Assembly Load (string assemblyString)
		{
			AssemblyName an = new AssemblyName ();
			an.Name = assemblyString;
			
			return Load (an, new Evidence ());			
		}

		public Assembly Load (string assemblyString, Evidence assemblySecurity)
		{
			AssemblyName an = new AssemblyName ();
			an.Name = assemblyString;
			
			return Load (an, assemblySecurity);			
		}

		public Assembly Load (byte[] rawAssembly)
		{
			return Load (rawAssembly, null, new Evidence ());
		}

		public Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore)
		{
			return Load (rawAssembly, rawSymbolStore, new Evidence ());
		}

		[MonoTODO]
		public Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence)
		{
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public void SetAppDomainPolicy (PolicyLevel domainPolicy)
		{
			throw new NotImplementedException ();
		}
		
		public void SetCachePath (string s)
		{
			SetupInformation.CachePath = s;
		}
		
		[MonoTODO]
		public void SetPrincipalPolicy (PrincipalPolicy policy)
		{
			throw new NotImplementedException ();
		}

		public void SetShadowCopyFiles()
		{
			SetupInformation.ShadowCopyFiles = "true";
		}
						
		public void SetShadowCopyPath (string s)
		{
			SetupInformation.ShadowCopyDirectories = s;
		}
		
		[MonoTODO]
		public void SetThreadPrincipal (IPrincipal principal)
		{
			throw new NotImplementedException ();
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern AppDomain InternalSetDomainByID (int domain_id);
 
		// Changes the active domain and returns the old domain
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern AppDomain InternalSetDomain (AppDomain context);
			
		// Changes the active context and returns the old context
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern Context InternalSetContext (Context context);

		// Returns the current context
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern Context InternalGetContext ();

		// Returns the current context
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern Context InternalGetDefaultContext ();

		public static AppDomain CreateDomain (string friendlyName)
		{
			return CreateDomain (friendlyName, new Evidence (), new AppDomainSetup ());
		}
		
		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo)
		{
			return CreateDomain (friendlyName, securityInfo, new AppDomainSetup ());
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain createDomain (string friendlyName, AppDomainSetup info);

		public static AppDomain CreateDomain (string friendlyName,
						      Evidence securityInfo,
						      AppDomainSetup info)
		{
			//TODO: treat securityInfo (can be null)
			if (friendlyName == null)
				throw new System.ArgumentNullException ("friendlyName");

			if (info == null)
				throw new System.ArgumentNullException ("info");

			// todo: allow setup in the other domain
			return (AppDomain) RemotingServices.GetDomainProxy( createDomain (friendlyName, info));
		}

		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo,
						      string appBasePath, string appRelativeSearchPath,
						      bool shadowCopyFiles)
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


		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void InternalUnload (int domain_id);
		
		// We do this because if the domain is a transparant proxy this
		// will still return the correct domain id.
		private int getDomainID () {
			return Thread.GetDomainID ();
		}

		public static void Unload (AppDomain domain)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");

			// fire the event(s) that we are unload the domain
			domain.OnDomainUnload ();
			
			// FIX: We need to unload the stuff in another thread
			// and throw abort exceptions on all threads involved
			// in any operations in the unloading domain.
			//
			// We need to clean the remoting references and cross app
			// domain sinks.
			//
			// This is just a hack to make unload work (almost anyway)
		
			InternalUnload (domain.getDomainID());
		}

		private void OnDomainUnload () {
			if (DomainUnload != null)
				DomainUnload(this, null);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern void SetData (string name, object data);

		[MonoTODO]
		public void SetDynamicBase(string path)
		{
			throw new NotImplementedException();
		}
				
		public static int GetCurrentThreadId ()
		{
			return Thread.CurrentThreadId;
		}

		public override string ToString () {
			return getFriendlyName ();
		}

		// This methods called from the runtime. Don't change signature.
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
			
			ResolveEventHandler [] list = (ResolveEventHandler []) AssemblyResolve.GetInvocationList ();
			foreach (ResolveEventHandler eh in list) {
				Assembly assembly = eh (this, new ResolveEventArgs (name));
				if (assembly != null)
					return assembly;
			}

			return null;
		}

		private Assembly DoTypeResolve (Object name_or_tb)
		{
			if (TypeResolve == null)
				return null;

			string name;

			if (name_or_tb is TypeBuilder)
				name = ((TypeBuilder)name_or_tb).FullName;
			else
				name = (string)name_or_tb;

			foreach (Delegate d in TypeResolve.GetInvocationList ()) {
				ResolveEventHandler eh = (ResolveEventHandler)d;
				Assembly assembly = eh (this, new ResolveEventArgs (name));
				if (assembly != null)
					return assembly;
			}

			return null;
		}

		internal byte[] GetMarshalledDomainObjRef ()
		{
			ObjRef oref = RemotingServices.Marshal(AppDomain.CurrentDomain, null, typeof(AppDomain));
			return CADSerializer.SerializeObject (oref).GetBuffer();
		}

		internal void ProcessMessageInDomain (byte[] arrRequest, CADMethodCallMessage cadMsg, out byte[] arrResponse, out CADMethodReturnMessage cadMrm)
		{
			IMessage reqDomMsg;

			if (null != arrRequest)
				reqDomMsg = CADSerializer.DeserializeMessage (new MemoryStream(arrRequest), null);
			else
				reqDomMsg = new MethodCall (cadMsg);

			IMessage retDomMsg = ChannelServices.SyncDispatchMessage (reqDomMsg);

			cadMrm = CADMethodReturnMessage.Create (retDomMsg);
			if (null == cadMrm) 
			{
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
