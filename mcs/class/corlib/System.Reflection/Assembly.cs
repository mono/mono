//
// System.Reflection/Assembly.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
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
using System.Security.Policy;
using System.Security.Permissions;
#endif
using System.Runtime.Serialization;
#if !MICRO_LIB
using System.Reflection;
using System.Reflection.Emit;
#endif
using System.IO;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections;
using System.Configuration.Assemblies;

using Mono.Security;

namespace System.Reflection {

#if NET_2_0
	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_Assembly))]
#endif
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
#if NET_2_1 || DISABLE_SECURITY
	public class Assembly : ICustomAttributeProvider, _Assembly {
#else
	public class Assembly : ICustomAttributeProvider, _Assembly, IEvidenceFactory, ISerializable {
#endif

		internal class ResolveEventHolder {
			public event ModuleResolveEventHandler ModuleResolve;
		}

		// Note: changes to fields must be reflected in _MonoReflectionAssembly struct (object-internals.h)
#pragma warning disable 169
		private IntPtr _mono_assembly;
#pragma warning restore 169

		private ResolveEventHolder resolve_event_holder;
		#if !DISABLE_SECURITY
		private Evidence _evidence;
		internal PermissionSet _minimum;	// for SecurityAction.RequestMinimum
		internal PermissionSet _optional;	// for SecurityAction.RequestOptional
		internal PermissionSet _refuse;		// for SecurityAction.RequestRefuse
		private PermissionSet _granted;		// for the resolved assembly granted permissions
		private PermissionSet _denied;		// for the resolved assembly denied permissions
		#endif
		private bool fromByteArray;
		private string assemblyName;

		internal Assembly () 
		{
			resolve_event_holder = new ResolveEventHolder ();
		}

		//
		// We can't store the event directly in this class, since the
		// compiler would silently insert the fields before _mono_assembly
		//
		public event ModuleResolveEventHandler ModuleResolve {
			#if !DISABLE_SECURITY
			[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
			#endif
			add {
				resolve_event_holder.ModuleResolve += value;
			}
			#if !DISABLE_SECURITY
			[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
			#endif
			remove {
				resolve_event_holder.ModuleResolve -= value;
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string get_code_base (bool escaped);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string get_fullname ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string get_location ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string InternalImageRuntimeVersion ();

		// SECURITY: this should be the only caller to icall get_code_base
		private string GetCodeBase (bool escaped)
		{
			string cb = get_code_base (escaped);
#if !NET_2_1 && !DISABLE_SECURITY
			if (SecurityManager.SecurityEnabled) {
				// we cannot divulge local file informations
				if (String.Compare ("FILE://", 0, cb, 0, 7, true, CultureInfo.InvariantCulture) == 0) {
					string file = cb.Substring (7);
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, file).Demand ();
				}
			}
#endif
			return cb;
		}

		public virtual string CodeBase {
			get { return GetCodeBase (false); }
		}

		public virtual string EscapedCodeBase {
			get { return GetCodeBase (true); }
		}

		public virtual string FullName {
			get {
				//
				// FIXME: This is wrong, but it gets us going
				// in the compiler for now
				//
				return ToString ();
			}
		}

		public virtual extern MethodInfo EntryPoint {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}
#if !NET_2_1 || MONOTOUCH
#if !DISABLE_SECURITY
		public virtual Evidence Evidence {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence = true)]
			get { return UnprotectedGetEvidence (); }
		}

		// note: the security runtime requires evidences but may be unable to do so...
		internal Evidence UnprotectedGetEvidence ()
		{
			// if the host (runtime) hasn't provided it's own evidence...
			if (_evidence == null) {
				// ... we will provide our own
				lock (this) {
					_evidence = Evidence.GetDefaultHostEvidence (this);
				}
			}
			return _evidence;
		}
#endif
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool get_global_assembly_cache ();

		public bool GlobalAssemblyCache {
			get {
				return get_global_assembly_cache ();
			}
		}
#endif
		internal bool FromByteArray {
			set { fromByteArray = value; }
		}

		public virtual String Location {
			get {
				if (fromByteArray)
					return String.Empty;

				string loc = get_location ();
#if !NET_2_1 && !DISABLE_SECURITY
				if ((loc != String.Empty) && SecurityManager.SecurityEnabled) {
					// we cannot divulge local file informations
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, loc).Demand ();
				}
#endif
				return loc;
			}
		}

#if NET_1_1
		[ComVisible (false)]
		public virtual string ImageRuntimeVersion {
			get {
				return InternalImageRuntimeVersion ();
			}
		}
#endif
#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
#endif
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			UnitySerializationHolder.GetAssemblyData (this, info, context);
		}

#if ONLY_1_1
		public new Type GetType ()
		{
			return base.GetType ();
		}
#endif

		public virtual bool IsDefined (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public virtual object [] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public virtual object [] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern object GetFilesInternal (String name, bool getResourceModules);

		public virtual FileStream[] GetFiles ()
		{
			return GetFiles (false);
		}

		public virtual FileStream [] GetFiles (bool getResourceModules)
		{
			string[] names = (string[]) GetFilesInternal (null, getResourceModules);
			if (names == null)
				return new FileStream [0];

			string location = Location;

			FileStream[] res;
			if (location != String.Empty) {
				res = new FileStream [names.Length + 1];
				res [0] = new FileStream (location, FileMode.Open, FileAccess.Read);
				for (int i = 0; i < names.Length; ++i)
					res [i + 1] = new FileStream (names [i], FileMode.Open, FileAccess.Read);
			} else {
				res = new FileStream [names.Length];
				for (int i = 0; i < names.Length; ++i)
					res [i] = new FileStream (names [i], FileMode.Open, FileAccess.Read);
			}
			return res;
		}

		public virtual FileStream GetFile (String name)
		{
			if (name == null)
				throw new ArgumentNullException (null, "Name cannot be null.");
			if (name.Length == 0)
				throw new ArgumentException ("Empty name is not valid");

			string filename = (string)GetFilesInternal (name, true);
			if (filename != null)
				return new FileStream (filename, FileMode.Open, FileAccess.Read);
			else
				return null;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern IntPtr GetManifestResourceInternal (String name, out int size, out Module module);

		public virtual Stream GetManifestResourceStream (String name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException ("String cannot have zero length.",
					"name");

			ManifestResourceInfo info = GetManifestResourceInfo (name);
			if (info == null)
				return null;

			if (info.ReferencedAssembly != null)
				return info.ReferencedAssembly.GetManifestResourceStream (name);
			if ((info.FileName != null) && (info.ResourceLocation == 0)) {
				if (fromByteArray)
#if NET_2_0
					throw new FileNotFoundException (info.FileName);
#else
					return null;
#endif

				string location = Path.GetDirectoryName (Location);
				string filename = Path.Combine (location, info.FileName);
#if NET_2_1 && !MONOTOUCH
				// we don't control the content of 'info.FileName' so we want to make sure we keep to ourselves
				filename = Path.GetFullPath (filename);
				if (!filename.StartsWith (location))
					throw new SecurityException ("non-rooted access to manifest resource");
#endif
				return new FileStream (filename, FileMode.Open, FileAccess.Read);
			}

			int size;
			Module module;
			IntPtr data = GetManifestResourceInternal (name, out size, out module);
			if (data == (IntPtr) 0)
				return null;
			else {
				UnmanagedMemoryStream stream;
				unsafe {
					stream = new UnmanagedMemoryStream ((byte*) data, size);
				}

				/* 
				 * The returned pointer points inside metadata, so
				 * we have to increase the refcount of the module, and decrease
				 * it when the stream is finalized.
				 */
				stream.Closed += new EventHandler (new ResourceCloseHandler (module).OnClose);
				return stream;
			}
		}

		public virtual Stream GetManifestResourceStream (Type type, String name)
		{
			string ns;
			if (type != null) {
				ns = type.Namespace;
			} else {
				if (name == null)
					throw new ArgumentNullException ("type");
				ns = null;
			}

			if (ns == null || ns.Length == 0)
				return GetManifestResourceStream (name);
			else
				return GetManifestResourceStream (ns + "." + name);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal virtual extern Type[] GetTypes (bool exportedOnly);
		
		public virtual Type[] GetTypes ()
		{
			return GetTypes (false);
		}

		public virtual Type[] GetExportedTypes ()
		{
			return GetTypes (true);
		}

		public virtual Type GetType (String name, Boolean throwOnError)
		{
			return GetType (name, throwOnError, false);
		}

		public virtual Type GetType (String name) {
			return GetType (name, false, false);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Type InternalGetType (Module module, String name, Boolean throwOnError, Boolean ignoreCase);

		public Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException (name);
			if (name.Length == 0)
			throw new ArgumentException ("name", "Name cannot be empty");

			return InternalGetType (null, name, throwOnError, ignoreCase);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void InternalGetAssemblyName (string assemblyFile, AssemblyName aname);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void FillName (Assembly ass, AssemblyName aname);

		[MonoTODO ("copiedName == true is not supported")]
		public virtual AssemblyName GetName (Boolean copiedName)
		{
			#if !DISABLE_SECURITY
			// CodeBase, which is restricted, will be copied into the AssemblyName object so...
			if (SecurityManager.SecurityEnabled) {
				GetCodeBase (true); // this will ensure the Demand is made
			}
			#endif
			
			return UnprotectedGetName ();
		}

		public virtual AssemblyName GetName ()
		{
			return GetName (false);
		}

		// the security runtime requires access to the assemblyname (e.g. to get the strongname)
		internal virtual AssemblyName UnprotectedGetName ()
		{
			AssemblyName aname = new AssemblyName ();
			FillName (this, aname);
			return aname;
		}

		public override string ToString ()
		{
			// note: ToString work without requiring CodeBase (so no checks are needed)

			if (assemblyName != null)
				return assemblyName;

			assemblyName = get_fullname ();
			return assemblyName;
		}

		public static String CreateQualifiedName (String assemblyName, String typeName) 
		{
			return typeName + ", " + assemblyName;
		}

		public static Assembly GetAssembly (Type type)
		{
			if (type != null)
				return type.Assembly;
			throw new ArgumentNullException ("type");
		}


		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern Assembly GetEntryAssembly();

		public Assembly GetSatelliteAssembly (CultureInfo culture)
		{
			return GetSatelliteAssembly (culture, null, true);
		}

		public Assembly GetSatelliteAssembly (CultureInfo culture, Version version)
		{
			return GetSatelliteAssembly (culture, version, true);
		}

		internal Assembly GetSatelliteAssemblyNoThrow (CultureInfo culture, Version version)
		{
			return GetSatelliteAssembly (culture, version, false);
		}

		private Assembly GetSatelliteAssembly (CultureInfo culture, Version version, bool throwOnError)
		{
			if (culture == null)
				throw new ArgumentException ("culture");

			AssemblyName aname = GetName (true);
			if (version != null)
				aname.Version = version;

			aname.CultureInfo = culture;
			aname.Name = aname.Name + ".resources";
			Assembly assembly;

			try {
				assembly = AppDomain.CurrentDomain.LoadSatellite (aname, false);
			if (assembly != null)
				return assembly;
			} catch (FileNotFoundException) {
				assembly = null;
				// ignore
			}

			// Try the assembly directory
			string location = Path.GetDirectoryName (Location);
			string fullName = Path.Combine (location, Path.Combine (culture.Name, aname.Name + ".dll"));
#if NET_2_1 && !MONOTOUCH
			// it's unlikely that culture.Name or aname.Name could contain stuff like ".." but...
			fullName = Path.GetFullPath (fullName);
			if (!fullName.StartsWith (location)) {
				if (throwOnError)
					throw new SecurityException ("non-rooted access to satellite assembly");
				return null;
			}
#endif
			if (!throwOnError && !File.Exists (fullName))
				return null;

			return LoadFrom (fullName);
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static Assembly LoadFrom (String assemblyFile, bool refonly);

		public static Assembly LoadFrom (String assemblyFile)
		{
			return LoadFrom (assemblyFile, false);
		}
#if !DISABLE_SECURITY
		public static Assembly LoadFrom (String assemblyFile, Evidence securityEvidence)
#else
		public static Assembly LoadFrom (String assemblyFile, object securityEvidence)
#endif
		{
			Assembly a = LoadFrom (assemblyFile, false);
#if !NET_2_1 && !DISABLE_SECURITY
			if ((a != null) && (securityEvidence != null)) {
				// merge evidence (i.e. replace defaults with provided evidences)
				a.Evidence.Merge (securityEvidence);
			}
#endif
			return a;
		}

#if NET_1_1

		[MonoTODO("This overload is not currently implemented")]
		// FIXME: What are we missing?
		#if !DISABLE_SECURITY
		public static Assembly LoadFrom (String assemblyFile, Evidence securityEvidence, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		#else
		public static Assembly LoadFrom (String assemblyFile, object securityEvidence, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		#endif
		{
			if (assemblyFile == null)
				throw new ArgumentNullException ("assemblyFile");
			if (assemblyFile == String.Empty)
				throw new ArgumentException ("Name can't be the empty string", "assemblyFile");
			throw new NotImplementedException ();
		}
#if !DISABLE_SECURITY
		public static Assembly LoadFile (String path, Evidence securityEvidence)
#else
		public static Assembly LoadFile (String path, object securityEvidence)
#endif
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path == String.Empty)
				throw new ArgumentException ("Path can't be empty", "path");
			// FIXME: Make this do the right thing
			return LoadFrom (path, securityEvidence);
		}

		public static Assembly LoadFile (String path)
		{
			return LoadFile (path, null);
		}
#endif

		public static Assembly Load (String assemblyString)
		{
			return AppDomain.CurrentDomain.Load (assemblyString);
		}
		#if !DISABLE_SECURITY
		public static Assembly Load (String assemblyString, Evidence assemblySecurity)
		#else
		public static Assembly Load (String assemblyString, object assemblySecurity)
		#endif
		{
			return AppDomain.CurrentDomain.Load (assemblyString, assemblySecurity);
		}

		public static Assembly Load (AssemblyName assemblyRef)
		{
			return AppDomain.CurrentDomain.Load (assemblyRef);
		}
		#if !DISABLE_SECURITY
		public static Assembly Load (AssemblyName assemblyRef, Evidence assemblySecurity)
		#else
		public static Assembly Load (AssemblyName assemblyRef, object assemblySecurity)
		#endif
		{
			return AppDomain.CurrentDomain.Load (assemblyRef, assemblySecurity);
		}

		public static Assembly Load (Byte[] rawAssembly)
		{
			return AppDomain.CurrentDomain.Load (rawAssembly);
		}

		public static Assembly Load (Byte[] rawAssembly, Byte[] rawSymbolStore)
		{
			return AppDomain.CurrentDomain.Load (rawAssembly, rawSymbolStore);
		}

		public static Assembly Load (Byte[] rawAssembly, Byte[] rawSymbolStore,
		#if !DISABLE_SECURITY
					     Evidence securityEvidence)
		#else
						 object securityEvidence)
		#endif
		{
			return AppDomain.CurrentDomain.Load (rawAssembly, rawSymbolStore, securityEvidence);
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		public static Assembly ReflectionOnlyLoad (byte[] rawAssembly)
		{
			return AppDomain.CurrentDomain.Load (rawAssembly, null, null, true);
		}

		public static Assembly ReflectionOnlyLoad (string assemblyString) 
		{
			return AppDomain.CurrentDomain.Load (assemblyString, null, true);
		}

		public static Assembly ReflectionOnlyLoadFrom (string assemblyFile) 
		{
			if (assemblyFile == null)
				throw new ArgumentNullException ("assemblyFile");
			
			return LoadFrom (assemblyFile, true);
		}
#endif

#if NET_2_0
		[Obsolete ("")]
#endif
		public static Assembly LoadWithPartialName (string partialName)
		{
			return LoadWithPartialName (partialName, null);
		}

		[MonoTODO ("Not implemented")]
		public Module LoadModule (string moduleName, byte [] rawModule)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public Module LoadModule (string moduleName, byte [] rawModule, byte [] rawSymbolStore)
		{
			throw new NotImplementedException ();
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		#if !DISABLE_SECURITY
		private static extern Assembly load_with_partial_name (string name, Evidence e);
		#else
		private static extern Assembly load_with_partial_name (string name, object e);
		#endif

#if NET_2_0
		[Obsolete ("")]
#endif
#if !DISABLE_SECURITY
		public static Assembly LoadWithPartialName (string partialName, Evidence securityEvidence)
#else
		public static Assembly LoadWithPartialName (string partialName, object securityEvidence)
#endif
		{
			return LoadWithPartialName (partialName, securityEvidence, true);
		}

		/**
		 * LAMESPEC: It is possible for this method to throw exceptions IF the name supplied
		 * is a valid gac name and contains filesystem entry charachters at the end of the name
		 * ie System/// will throw an exception. However ////System will not as that is canocolized
		 * out of the name.
		 */

		// FIXME: LoadWithPartialName must look cache (no CAS) or read from disk (CAS)
		#if !DISABLE_SECURITY
		internal static Assembly LoadWithPartialName (string partialName, Evidence securityEvidence, bool oldBehavior)
		#else
		internal static Assembly LoadWithPartialName (string partialName, object securityEvidence, bool oldBehavior)
		#endif
		{
			if (!oldBehavior)
				throw new NotImplementedException ();

			if (partialName == null)
				throw new NullReferenceException ();

			return load_with_partial_name (partialName, securityEvidence);
		}

		public Object CreateInstance (String typeName) 
		{
			return CreateInstance (typeName, false);
		}

		public Object CreateInstance (String typeName, Boolean ignoreCase)
		{
			Type t = GetType (typeName, false, ignoreCase);
			if (t == null)
				return null;

			try {
				return Activator.CreateInstance (t);
			} catch (InvalidOperationException) {
				throw new ArgumentException ("It is illegal to invoke a method on a Type loaded via ReflectionOnly methods.");
			}
		}

		public Object CreateInstance (String typeName, Boolean ignoreCase,
					      BindingFlags bindingAttr, Binder binder,
					      Object[] args, CultureInfo culture,
					      Object[] activationAttributes)
		{
			Type t = GetType (typeName, false, ignoreCase);
			if (t == null)
				return null;

			try {
				return Activator.CreateInstance (t, bindingAttr, binder, args, culture, activationAttributes);
			} catch (InvalidOperationException) {
				throw new ArgumentException ("It is illegal to invoke a method on a Type loaded via ReflectionOnly methods.");
			}
		}

		public Module[] GetLoadedModules ()
		{
			return GetLoadedModules (false);
		}

		// FIXME: Currently, the two sets of modules are equal
		public Module[] GetLoadedModules (bool getResourceModules)
		{
			return GetModules (getResourceModules);
		}

		public Module[] GetModules ()
		{
			return GetModules (false);
		}

		public Module GetModule (String name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException ("Name can't be empty");

			Module[] modules = GetModules (true);
			foreach (Module module in modules) {
				if (module.ScopeName == name)
					return module;
			}

			return null;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal virtual extern Module[] GetModulesInternal ();

		public Module[] GetModules (bool getResourceModules) {
			Module[] modules = GetModulesInternal ();

			if (!getResourceModules) {
				ArrayList result = new ArrayList (modules.Length);
				foreach (Module m in modules)
					if (!m.IsResource ())
						result.Add (m);
				return (Module[])result.ToArray (typeof (Module));
			}
			else
				return modules;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern string[] GetNamespaces ();
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern virtual String[] GetManifestResourceNames ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static Assembly GetExecutingAssembly ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static Assembly GetCallingAssembly ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern AssemblyName[] GetReferencedAssemblies ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool GetManifestResourceInfoInternal (String name, ManifestResourceInfo info);

		public virtual ManifestResourceInfo GetManifestResourceInfo (String resourceName)
		{
			if (resourceName == null)
				throw new ArgumentNullException ("resourceName");
			if (resourceName.Length == 0)
				throw new ArgumentException ("String cannot have zero length.");
			ManifestResourceInfo result = new ManifestResourceInfo ();
			bool found = GetManifestResourceInfoInternal (resourceName, result);
			if (found)
				return result;
			else
				return null;
		}

		private class ResourceCloseHandler {
#pragma warning disable 169, 414
			Module module;
#pragma warning restore 169, 414			

			public ResourceCloseHandler (Module module) {
				this.module = module;
			}

			public void OnClose (object sender, EventArgs e) {
				// The module dtor will take care of things
				module = null;
			}
		}

		//
		// The following functions are only for the Mono Debugger.
		//

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern int MonoDebugger_GetMethodToken (MethodBase method);

#if NET_2_0 || BOOTSTRAP_NET_2_0
		[MonoTODO ("Always returns zero")]
		[ComVisible (false)]
		public long HostContext {
			get { return 0; }
		}

		[ComVisible (false)]
		public Module ManifestModule {
			get {
				return GetManifestModule ();
			}
		}

		internal virtual Module GetManifestModule () {
			return GetManifestModuleInternal ();
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Module GetManifestModuleInternal ();

		[ComVisible (false)]
		public virtual extern bool ReflectionOnly {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}
#endif

#if !NET_2_1 || MONOTOUCH
		// Code Access Security

		internal void Resolve () 
		{
			lock (this) {
				// FIXME: As we (currently) delay the resolution until the first CAS
				// Demand it's too late to evaluate the Minimum permission set as a 
				// condition to load the assembly into the AppDomain
				#if !DISABLE_SECURITY
				LoadAssemblyPermissions ();
				Evidence e = new Evidence (UnprotectedGetEvidence ()); // we need a copy to add PRE
				e.AddHost (new PermissionRequestEvidence (_minimum, _optional, _refuse));
				_granted = SecurityManager.ResolvePolicy (e,
					_minimum, _optional, _refuse, out _denied);
				#endif
			}
		}
#if !DISABLE_SECURITY
		internal PermissionSet GrantedPermissionSet {
			get {
				if (_granted == null) {
					if (SecurityManager.ResolvingPolicyLevel != null) {
						if (SecurityManager.ResolvingPolicyLevel.IsFullTrustAssembly (this))
							return DefaultPolicies.FullTrust;
						else
							return null; // we can't resolve during resolution
					}
					Resolve ();
				}
				return _granted;
			}
		}

		internal PermissionSet DeniedPermissionSet {
			get {
				// yes we look for granted, as denied may be null
				if (_granted == null) {
					if (SecurityManager.ResolvingPolicyLevel != null) {
						if (SecurityManager.ResolvingPolicyLevel.IsFullTrustAssembly (this))
							return null;
						else
							return DefaultPolicies.FullTrust; // deny unrestricted
					}
					Resolve ();
				}
				return _denied;
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern internal static bool LoadPermissions (Assembly a, 
			ref IntPtr minimum, ref int minLength,
			ref IntPtr optional, ref int optLength,
			ref IntPtr refused, ref int refLength);

		// Support for SecurityAction.RequestMinimum, RequestOptional and RequestRefuse
		private void LoadAssemblyPermissions ()
		{
			IntPtr minimum = IntPtr.Zero, optional = IntPtr.Zero, refused = IntPtr.Zero;
			int minLength = 0, optLength = 0, refLength = 0;
			if (LoadPermissions (this, ref minimum, ref minLength, ref optional,
				ref optLength, ref refused, ref refLength)) {

				// Note: no need to cache these permission sets as they will only be created once
				// at assembly resolution time.
				if (minLength > 0) {
					byte[] data = new byte [minLength];
					Marshal.Copy (minimum, data, 0, minLength);
					_minimum = SecurityManager.Decode (data);
				}
				if (optLength > 0) {
					byte[] data = new byte [optLength];
					Marshal.Copy (optional, data, 0, optLength);
					_optional = SecurityManager.Decode (data);
				}
				if (refLength > 0) {
					byte[] data = new byte [refLength];
					Marshal.Copy (refused, data, 0, refLength);
					_refuse = SecurityManager.Decode (data);
				}
			}
		}
#endif
#endif
	}
}
