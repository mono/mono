//
// System.Reflection/Assembly.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
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

using System;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Runtime.Serialization;
using System.Reflection.Emit;
using System.IO;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections;
using System.Configuration.Assemblies;

namespace System.Reflection {

	internal class ResolveEventHolder {
		public event ModuleResolveEventHandler ModuleResolve;
	}

	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class Assembly : System.Reflection.ICustomAttributeProvider,
		System.Security.IEvidenceFactory, System.Runtime.Serialization.ISerializable {

		// Note: changes to fields must be reflected in _MonoReflectionAssembly struct (object-internals.h)
		private IntPtr _mono_assembly;

		private ResolveEventHolder resolve_event_holder;
		private Evidence _evidence;
		internal PermissionSet _minimum;		// for SecurityAction.RequestMinimum
		internal PermissionSet _optional;	// for SecurityAction.RequestOptional
		internal PermissionSet _refuse;		// for SecurityAction.RequestRefuse
		private PermissionSet _granted;		// for the resolved assembly granted permissions
		private PermissionSet _denied;		// for the resolved assembly denied permissions
		
		internal Assembly () 
		{
			resolve_event_holder = new ResolveEventHolder ();
		}

		//
		// We can't store the event directly in this class, since the
		// compile would silently insert the fields before _mono_assembly
		//
		public event ModuleResolveEventHandler ModuleResolve {
			add {
				resolve_event_holder.ModuleResolve -= value;
			}
			remove {
				resolve_event_holder.ModuleResolve -= value;
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string get_code_base ();
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string get_location ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string InternalImageRuntimeVersion ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool get_global_assembly_cache ();

		public virtual string CodeBase {
			get {
				return get_code_base ();
			}
		}

		internal virtual string CopiedCodeBase {
			get {
				return get_code_base ();
			}
		} 

		[MonoTODO]
		public virtual string EscapedCodeBase {
			get {
				//FIXME: escape characters -> Uri
				return get_code_base ();
			}
		}

		public virtual string FullName {
			get {
				//
				// FIXME: This is wrong, but it gets us going
				// in the compiler for now
				//
				return GetName (false).ToString ();
			}
		}

		public virtual extern MethodInfo EntryPoint {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public virtual Evidence Evidence {
			get {
				// if the host (runtime) hasn't provided it's own evidence...
				if (_evidence == null) {
					// ... we will provide our own
					lock (this) {
						_evidence = Evidence.GetDefaultHostEvidence (this);
					}
				}
				return _evidence;
			}
		}

		public bool GlobalAssemblyCache {
			get {
				return get_global_assembly_cache ();
			}
		}
		
		public virtual String Location {
			get {
				return get_location ();
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

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			UnitySerializationHolder.GetAssemblyData (this, info, context);
		}

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
		private extern object GetFilesInternal (String name);

		public virtual FileStream[] GetFiles ()
		{
			string[] names = (string[]) GetFilesInternal (null);
			if (names == null)
				return new FileStream [0];

			FileStream[] res = new FileStream [names.Length];
			for (int i = 0; i < names.Length; ++i)
				res [i] = new FileStream (names [i], FileMode.Open, FileAccess.Read);
			return res;
		}

		[MonoTODO ("true == not implemented")]
		public virtual FileStream [] GetFiles (bool getResourceModules)
		{
			if (!getResourceModules)
				return GetFiles ();
			throw new NotImplementedException ();
		}

		public virtual FileStream GetFile (String name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException ("name");

			string filename = (string)GetFilesInternal (name);
			if (filename != null)
				return new FileStream (filename, FileMode.Open, FileAccess.Read);
			else
				return null;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern IntPtr GetManifestResourceInternal (String name, out int size, out Module module);

		public virtual Stream GetManifestResourceStream (String name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name == "")
				throw new ArgumentException ("name cannot have zero length.");

			ManifestResourceInfo info = GetManifestResourceInfo (name);
			if (info == null)
				return null;

			if (info.ReferencedAssembly != null)
				return info.ReferencedAssembly.GetManifestResourceStream (name);
			if ((info.FileName != null) && (info.ResourceLocation == 0)) {
				string filename = Path.Combine (Path.GetDirectoryName (Location),
											info.FileName);
				return new FileStream (filename, FileMode.Open, FileAccess.Read);
			}

			int size;
			Module module;
			IntPtr data = GetManifestResourceInternal (name, out size, out module);
			if (data == (IntPtr) 0)
				return null;
			else {
				IntPtrStream stream = new IntPtrStream (data, size);
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
			if (type != null)
				ns = type.Namespace;
			else 
				ns = null;

			if ((ns == null) || (ns == ""))
				return GetManifestResourceStream (name);
			else
				return GetManifestResourceStream (ns + "." + name);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern Type[] GetTypes (bool exportedOnly);
		
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

			return InternalGetType (null, name, throwOnError, ignoreCase);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void InternalGetAssemblyName (string assemblyFile, AssemblyName aname);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void FillName (Assembly ass, AssemblyName aname);

		[MonoTODO ("true == not supported")]
		public virtual AssemblyName GetName (Boolean copiedName)
		{
			AssemblyName aname = new AssemblyName ();
			FillName (this, aname);
			return aname;
		}

		public virtual AssemblyName GetName ()
		{
			return GetName (false);
		}

		public override String ToString ()
		{
			return GetName ().ToString ();
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
			return GetSatelliteAssembly (culture, null);
		}

		public Assembly GetSatelliteAssembly (CultureInfo culture, Version version)
		{
			if (culture == null)
				throw new ArgumentException ("culture");

			AssemblyName aname = GetName (true);
			if (version != null)
				aname.Version = version;

			aname.CultureInfo = culture;
			aname.Name = aname.Name + ".resources";
			return Load (aname);
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static Assembly LoadFrom (String assemblyFile);

		public static Assembly LoadFrom (String assemblyFile, Evidence securityEvidence)
		{
			Assembly a = LoadFrom (assemblyFile);
			if ((a != null) && (securityEvidence != null)) {
				// merge evidence (i.e. replace defaults with provided evidences)
				a.Evidence.Merge (securityEvidence);
			}
			return a;
		}

#if NET_1_1

		[MonoTODO]
		public static Assembly LoadFrom (String assemblyFile, Evidence securityEvidence, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			if (assemblyFile == null)
				throw new ArgumentNullException ("assemblyFile");
			if (assemblyFile == String.Empty)
				throw new ArgumentException ("Name can't be the empty string", "assemblyFile");
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Assembly LoadFile (String path, Evidence securityEvidence) {
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path == String.Empty)
				throw new ArgumentException ("Path can't be empty", "path");
			// FIXME: Make this do the right thing
			return LoadFrom (path, securityEvidence);
		}

		public static Assembly LoadFile (String path) {
			return LoadFile (path, null);
		}
#endif

		public static Assembly Load (String assemblyString)
		{
			return AppDomain.CurrentDomain.Load (assemblyString);
		}
		
		public static Assembly Load (String assemblyString, Evidence assemblySecurity)
		{
			return AppDomain.CurrentDomain.Load (assemblyString, assemblySecurity);
		}

		public static Assembly Load (AssemblyName assemblyRef)
		{
			return AppDomain.CurrentDomain.Load (assemblyRef);
		}

		public static Assembly Load (AssemblyName assemblyRef, Evidence assemblySecurity)
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
					     Evidence securityEvidence)
		{
			return AppDomain.CurrentDomain.Load (rawAssembly, rawSymbolStore, securityEvidence);
		}

#if NET_2_0
		[Obsolete ("")]
#endif
		public static Assembly LoadWithPartialName (string partialName)
		{
			return LoadWithPartialName (partialName, null);
		}

		[MonoTODO]
		public Module LoadModule (string moduleName, byte [] rawModule)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Module LoadModule (string moduleName, byte [] rawModule, byte [] rawSymbolStore)
		{
			throw new NotImplementedException ();
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern Assembly load_with_partial_name (string name, Evidence e);

		/**
		 * LAMESPEC: It is possible for this method to throw exceptions IF the name supplied
		 * is a valid gac name and contains filesystem entry charachters at the end of the name
		 * ie System/// will throw an exception. However ////System will not as that is canocolized
		 * out of the name.
		 */
#if NET_2_0
		[Obsolete ("")]
#endif
		public static Assembly LoadWithPartialName (string partialName, Evidence securityEvidence)
		{
			if (partialName == null)
				throw new NullReferenceException ();

			int ci = partialName.IndexOf (',');
			if (ci > 0)
				partialName = partialName.Substring (0, ci);

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

			return Activator.CreateInstance (t);
		}

		public Object CreateInstance (String typeName, Boolean ignoreCase,
					      BindingFlags bindingAttr, Binder binder,
					      Object[] args, CultureInfo culture,
					      Object[] activationAttributes)
		{
			Type t = GetType (typeName, false, ignoreCase);
			if (t == null)
				return null;

			return Activator.CreateInstance (t, bindingAttr, binder, args, culture, activationAttributes);
		}

		public Module[] GetLoadedModules ()
		{
			return GetLoadedModules (false);
		}

		[MonoTODO]
		public Module[] GetLoadedModules (bool getResourceModules)
		{
			// Currently, the two sets of modules are equal
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
			if (name == "")
				throw new ArgumentException ("Name can't be empty");

			Module[] modules = GetModules (true);
			foreach (Module module in modules) {
				if (module.ScopeName == name)
					return module;
			}

			return null;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Module[] GetModulesInternal ();

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
			if (resourceName == "")
				throw new ArgumentException ("String cannot have zero length.");
			ManifestResourceInfo result = new ManifestResourceInfo ();
			bool found = GetManifestResourceInfoInternal (resourceName, result);
			if (found)
				return result;
			else
				return null;
		}

		private class ResourceCloseHandler {

			Module module;

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
		internal static extern MethodBase MonoDebugger_GetMethod (Assembly assembly, int token);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern int MonoDebugger_GetMethodToken (Assembly assembly, MethodBase method);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern Type MonoDebugger_GetLocalTypeFromSignature (Assembly assembly, byte[] signature);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern Type MonoDebugger_GetType (Assembly assembly, int token);

#if NET_2_0
		[MonoTODO]
		[ComVisible (false)]
		public long HostContext {
			get { return 0; }
		}

		[MonoTODO ("choice is rather limited")]
		[ComVisible (false)]
		public ImageFileMachine ImageFileMachine {
			get { return ImageFileMachine.I386; }
		}

		[MonoTODO]
		[ComVisible (false)]
		public Module ManifestModule {
			get { return null; }
		}

		[MonoTODO]
		[ComVisible (false)]
		public int MetadataToken {
			get { return 0; }
		}

		[MonoTODO]
		[ComVisible (false)]
		public PortableExecutableKind PortableExecutableKind {
			get { return PortableExecutableKind.ILOnly; }
		}

		[MonoTODO ("see ReflectionOnlyLoad")]
		[ComVisible (false)]
		public virtual bool ReflectionOnly {
			get { return false; }
		}
#endif

		// Code Access Security

		internal void Resolve () 
		{
			lock (this) {
				_granted = SecurityManager.ResolvePolicy (Evidence, _minimum, _optional,
					_refuse, out _denied);
			}
#if false
			Console.WriteLine ("Granted: {0}", _granted);
			if (_denied != null)
				Console.WriteLine ("Denied: {0}", _denied);
#endif
		}

		internal PermissionSet GrantedPermissionSet {
			get {
				if (_granted == null) {
					Resolve ();
				}
				return _granted;
			}
		}

		internal PermissionSet DeniedPermissionSet {
			get {
				// yes we look for granted, as denied may be null
				if (_granted == null) {
					Resolve ();
				}
				return _denied;
			}
		}

		// Result isn't affected by overrides (like Assert, Deny and PermitOnly)
		internal bool Demand (IPermission p) 
		{
			Type t = p.GetType ();

			// have we been explicitely denied this permission ?
			if (_denied != null) {
				IPermission denied = _denied.GetPermission (t);
				if (denied != null) {
					if (p.IsSubsetOf (denied))
						return false;
				}
			}

			// is it part of the optional permissions requested by the assembly ?
			if (_optional != null) {
				IPermission optional = _optional.GetPermission (t);
				if (optional != null) {
					// there is! so we can only request a subset of it
					if (!p.IsSubsetOf (optional))
						return false;
				}
			}

			// don't check IUnrestrictedPermission if we have "Full Trust"
			// note: that won't work for code identity permissions (e.g. Zone)
			if ((p is IUnrestrictedPermission) && GrantedPermissionSet.IsUnrestricted ())
				return true;

			// finally does the resolved policy allow this requested permission ?
			IPermission granted = GrantedPermissionSet.GetPermission (t);
			if (granted != null) {
				if (!p.IsSubsetOf (granted))
					return false;
			}
			return true;
		}
	}
}
