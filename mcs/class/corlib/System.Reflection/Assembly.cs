//
// System.Reflection/Assembly.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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
using System.Security.Policy;
using System.Security.Permissions;
using System.Runtime.Serialization;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Threading;
using System.Text;
using System.Diagnostics.Contracts;

using Mono.Security;

namespace System.Reflection {

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_Assembly))]
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	[StructLayout (LayoutKind.Sequential)]
#if MOBILE
	public partial class Assembly : ICustomAttributeProvider {
#else
	public abstract class Assembly : ICustomAttributeProvider, _Assembly, IEvidenceFactory, ISerializable {
#endif
		internal class ResolveEventHolder {
			public event ModuleResolveEventHandler ModuleResolve;
		}

		internal class UnmanagedMemoryStreamForModule : UnmanagedMemoryStream
		{
#pragma warning disable 414
			Module module;
#pragma warning restore

			public unsafe UnmanagedMemoryStreamForModule (byte* pointer, long length, Module module)
				: base (pointer, length)
			{
				this.module = module;
			}

			protected override void Dispose (bool disposing)
			{
				if (_isOpen) {
					/* 
					 * The returned pointer points inside metadata, so
					 * we have to increase the refcount of the module, and decrease
					 * it when the stream is finalized.
					 */
					module = null;
				}

				base.Dispose (disposing);
			}
		}

		// Note: changes to fields must be reflected in _MonoReflectionAssembly struct (object-internals.h)
#pragma warning disable 649
		internal IntPtr _mono_assembly;
#pragma warning restore 649

		private ResolveEventHolder resolve_event_holder;
#if !MOBILE
		private Evidence _evidence;
		internal PermissionSet _minimum;	// for SecurityAction.RequestMinimum
		internal PermissionSet _optional;	// for SecurityAction.RequestOptional
		internal PermissionSet _refuse;		// for SecurityAction.RequestRefuse
		private PermissionSet _granted;		// for the resolved assembly granted permissions
		private PermissionSet _denied;		// for the resolved assembly denied permissions
#else
		object _evidence, _minimum, _optional, _refuse, _granted, _denied;
#endif
		private bool fromByteArray;
		private string assemblyName;

		protected
		Assembly () 
		{
			resolve_event_holder = new ResolveEventHolder ();
		}

		//
		// We can't store the event directly in this class, since the
		// compiler would silently insert the fields before _mono_assembly
		//
		public event ModuleResolveEventHandler ModuleResolve {
			[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
			add {
				resolve_event_holder.ModuleResolve += value;
			}
			[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static internal extern string GetAotId ();

		// SECURITY: this should be the only caller to icall get_code_base
		private string GetCodeBase (bool escaped)
		{
			string cb = get_code_base (escaped);
#if !MOBILE
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

		public virtual Evidence Evidence {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence = true)]
			get { return UnprotectedGetEvidence (); }
		}

		// note: the security runtime requires evidences but may be unable to do so...
		internal Evidence UnprotectedGetEvidence ()
		{
#if MOBILE
			return null;
#else
			// if the host (runtime) hasn't provided it's own evidence...
			if (_evidence == null) {
				// ... we will provide our own
				lock (this) {
					_evidence = Evidence.GetDefaultHostEvidence (this);
				}
			}
			return _evidence;
#endif
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern bool get_global_assembly_cache ();

		internal bool FromByteArray {
			set { fromByteArray = value; }
		}

		public virtual String Location {
			get {
				if (fromByteArray)
					return String.Empty;

				string loc = get_location ();
#if !MOBILE
				if ((loc != String.Empty) && SecurityManager.SecurityEnabled) {
					// we cannot divulge local file informations
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, loc).Demand ();
				}
#endif
				return loc;
			}
		}

		[ComVisible (false)]
		public virtual string ImageRuntimeVersion {
			get {
				return InternalImageRuntimeVersion ();
			}
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
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
		private extern object GetFilesInternal (String name, bool getResourceModules);

		public virtual FileStream[] GetFiles ()
		{
			return GetFiles (false);
		}

		public virtual FileStream [] GetFiles (bool getResourceModules)
		{
			string[] names = (string[]) GetFilesInternal (null, getResourceModules);
			if (names == null)
				return EmptyArray<FileStream>.Value;

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
			if (info == null) {
				Assembly a = AppDomain.CurrentDomain.DoResourceResolve (name, this);
				if (a != null && a != this)
					return a.GetManifestResourceStream (name);
				else
					return null;
			}

			if (info.ReferencedAssembly != null)
				return info.ReferencedAssembly.GetManifestResourceStream (name);
			if ((info.FileName != null) && (info.ResourceLocation == 0)) {
				if (fromByteArray)
					throw new FileNotFoundException (info.FileName);

				string location = Path.GetDirectoryName (Location);
				string filename = Path.Combine (location, info.FileName);
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
					stream = new UnmanagedMemoryStreamForModule ((byte*) data, size, module);
				}
				return stream;
			}
		}

		public virtual Stream GetManifestResourceStream (Type type, String name)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return GetManifestResourceStream(type, name, false, ref stackMark);
		}

		internal Stream GetManifestResourceStream (Type type, String name, bool skipSecurityCheck, ref StackCrawlMark stackMark)
		{
			StringBuilder sb = new StringBuilder ();
			if (type == null) {
				if (name == null)
						throw new ArgumentNullException ("type");
			} else {
				String nameSpace = type.Namespace;
				if (nameSpace != null) {
					sb.Append (nameSpace);
					if (name != null)
						sb.Append (Type.Delimiter);
				}
			}

			if (name != null)
				sb.Append(name);

			return GetManifestResourceStream (sb.ToString());
		}

		internal unsafe Stream GetManifestResourceStream(String name, ref StackCrawlMark stackMark, bool skipSecurityCheck)
		{
			return GetManifestResourceStream (null, name, skipSecurityCheck, ref stackMark);
		}

		internal String GetSimpleName()
		{
			AssemblyName aname = GetName (true);
			return aname.Name;
		}

		internal byte[] GetPublicKey()
		{
			AssemblyName aname = GetName (true);
			return aname.GetPublicKey ();
		}

		internal Version GetVersion()
		{
			AssemblyName aname = GetName (true);
			return aname.Version;
		}

		private AssemblyNameFlags GetFlags()
		{
			AssemblyName aname = GetName (true);
			return aname.Flags;
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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void InternalGetAssemblyName (string assemblyFile, AssemblyName aname);

		public virtual AssemblyName GetName (Boolean copiedName)
		{
			throw new NotImplementedException ();
		}

		public virtual AssemblyName GetName ()
		{
			return GetName (false);
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

		internal Assembly GetSatelliteAssembly (CultureInfo culture, Version version, bool throwOnError)
		{
			if (culture == null)
				throw new ArgumentNullException("culture");
			Contract.EndContractBlock();

			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			String name = GetSimpleName() + ".resources";
			return InternalGetSatelliteAssembly(name, culture, version, true, ref stackMark);
		}

		internal RuntimeAssembly InternalGetSatelliteAssembly (String name, CultureInfo culture, Version version, bool throwOnFileNotFound, ref StackCrawlMark stackMark)
		{
			AssemblyName an = new AssemblyName ();

			an.SetPublicKey (GetPublicKey ());
			an.Flags = GetFlags () | AssemblyNameFlags.PublicKey;

			if (version == null)
				an.Version = GetVersion ();
			else
				an.Version = version;

			an.CultureInfo = culture;
			an.Name = name;

			Assembly assembly;

			try {
				assembly = AppDomain.CurrentDomain.LoadSatellite (an, false);
				if (assembly != null)
					return (RuntimeAssembly)assembly;
			} catch (FileNotFoundException) {
				assembly = null;
				// ignore
			}

			if (String.IsNullOrEmpty (Location))
				return null;

			// Try the assembly directory
			string location = Path.GetDirectoryName (Location);
			string fullName = Path.Combine (location, Path.Combine (culture.Name, an.Name + ".dll"));
			if (!throwOnFileNotFound && !File.Exists (fullName))
				return null;

			return (RuntimeAssembly)LoadFrom (fullName);
		}

#if !MOBILE
		Type _Assembly.GetType ()
		{
			// Required or object::GetType becomes virtual final
			return base.GetType ();
		}		
#endif

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static Assembly LoadFrom (String assemblyFile, bool refonly);

		public static Assembly LoadFrom (String assemblyFile)
		{
			return LoadFrom (assemblyFile, false);
		}

		[Obsolete]
		public static Assembly LoadFrom (String assemblyFile, Evidence securityEvidence)
		{
			Assembly a = LoadFrom (assemblyFile, false);
#if !MOBILE
			if ((a != null) && (securityEvidence != null)) {
				// merge evidence (i.e. replace defaults with provided evidences)
				a.Evidence.Merge (securityEvidence);
			}
#endif
			return a;
		}

		[Obsolete]
		[MonoTODO("This overload is not currently implemented")]
		// FIXME: What are we missing?
		public static Assembly LoadFrom (String assemblyFile, Evidence securityEvidence, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Assembly LoadFrom (String assemblyFile, byte [] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			throw new NotImplementedException ();
		}

		public static Assembly UnsafeLoadFrom (String assemblyFile)
		{
			return LoadFrom (assemblyFile);
		}

		[Obsolete]
		public static Assembly LoadFile (String path, Evidence securityEvidence)
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

		public static Assembly Load (String assemblyString)
		{
			return AppDomain.CurrentDomain.Load (assemblyString);
		}

		[Obsolete]
		public static Assembly Load (String assemblyString, Evidence assemblySecurity)
		{
			return AppDomain.CurrentDomain.Load (assemblyString, assemblySecurity);
		}

		public static Assembly Load (AssemblyName assemblyRef)
		{
			return AppDomain.CurrentDomain.Load (assemblyRef);
		}

		[Obsolete]
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

		[Obsolete]
		public static Assembly Load (Byte[] rawAssembly, Byte[] rawSymbolStore,
					     Evidence securityEvidence)
		{
			return AppDomain.CurrentDomain.Load (rawAssembly, rawSymbolStore, securityEvidence);
		}

		[MonoLimitation ("Argument securityContextSource is ignored")]
		public static Assembly Load (byte [] rawAssembly, byte [] rawSymbolStore, SecurityContextSource securityContextSource)
		{
			return AppDomain.CurrentDomain.Load (rawAssembly, rawSymbolStore);
		}

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

		[Obsolete]
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
		public
		virtual
		Module LoadModule (string moduleName, byte [] rawModule, byte [] rawSymbolStore)
		{
			throw new NotImplementedException ();
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern Assembly load_with_partial_name (string name, Evidence e);

		[Obsolete]
		public static Assembly LoadWithPartialName (string partialName, Evidence securityEvidence)
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
		internal static Assembly LoadWithPartialName (string partialName, Evidence securityEvidence, bool oldBehavior)
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

		public
		virtual
		Object CreateInstance (String typeName, Boolean ignoreCase,
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

		public Module[] GetModules ()
		{
			return GetModules (false);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal virtual extern Module[] GetModulesInternal ();
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern virtual String[] GetManifestResourceNames ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static Assembly GetExecutingAssembly ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static Assembly GetCallingAssembly ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern AssemblyName[] GetReferencedAssemblies (Assembly module);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool GetManifestResourceInfoInternal (String name, ManifestResourceInfo info);

		public virtual ManifestResourceInfo GetManifestResourceInfo (String resourceName)
		{
			if (resourceName == null)
				throw new ArgumentNullException ("resourceName");
			if (resourceName.Length == 0)
				throw new ArgumentException ("String cannot have zero length.");
			ManifestResourceInfo result = new ManifestResourceInfo (null, null, 0);
			bool found = GetManifestResourceInfoInternal (resourceName, result);
			if (found)
				return result;
			else
				return null;
		}

		[MonoTODO ("Currently it always returns zero")]
		[ComVisible (false)]
		public
		virtual
		long HostContext {
			get { return 0; }
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
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override bool Equals (object o)
		{
			if (((object) this) == o)
				return true;

			if (o == null)
				return false;
			
			Assembly other = (Assembly) o;
			return other._mono_assembly == _mono_assembly;
		}

#if !MOBILE
		// Code Access Security

		internal void Resolve () 
		{
			lock (this) {
				// FIXME: As we (currently) delay the resolution until the first CAS
				// Demand it's too late to evaluate the Minimum permission set as a 
				// condition to load the assembly into the AppDomain
				LoadAssemblyPermissions ();
				Evidence e = new Evidence (UnprotectedGetEvidence ()); // we need a copy to add PRE
				e.AddHost (new PermissionRequestEvidence (_minimum, _optional, _refuse));
				_granted = SecurityManager.ResolvePolicy (e,
					_minimum, _optional, _refuse, out _denied);
			}
		}

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
		
		public virtual PermissionSet PermissionSet {
			get { return this.GrantedPermissionSet; }
		}
		
		public virtual SecurityRuleSet SecurityRuleSet {
			get { throw CreateNIE (); }
		}

#endif

		static Exception CreateNIE ()
		{
			return new NotImplementedException ("Derived classes must implement it");
		}
		
		public virtual IList<CustomAttributeData> GetCustomAttributesData ()
		{
			return CustomAttributeData.GetCustomAttributes (this);
		}

		[MonoTODO]
		public bool IsFullyTrusted {
			get { return true; }
		}

		public virtual Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			throw CreateNIE ();
		}

		public virtual Module GetModule (String name)
		{
			throw CreateNIE ();
		}

		public virtual AssemblyName[] GetReferencedAssemblies ()
		{
			throw CreateNIE ();
		}

		public virtual Module[] GetModules (bool getResourceModules)
		{
			throw CreateNIE ();
		}

		[MonoTODO ("Always returns the same as GetModules")]
		public virtual Module[] GetLoadedModules (bool getResourceModules)
		{
			throw CreateNIE ();
		}

		public virtual Assembly GetSatelliteAssembly (CultureInfo culture)
		{
			throw CreateNIE ();
		}

		public virtual Assembly GetSatelliteAssembly (CultureInfo culture, Version version)
		{
			throw CreateNIE ();
		}

		public virtual Module ManifestModule {
			get { throw CreateNIE (); }
		}

		public virtual bool GlobalAssemblyCache {
			get { throw CreateNIE (); }
		}

		public virtual bool IsDynamic {
			get { return false; }
		}

		public static bool operator == (Assembly left, Assembly right)
		{
			if ((object)left == (object)right)
				return true;
			if ((object)left == null ^ (object)right == null)
				return false;
			return left.Equals (right);
		}

		public static bool operator != (Assembly left, Assembly right)
		{
			if ((object)left == (object)right)
				return false;
			if ((object)left == null ^ (object)right == null)
				return true;
			return !left.Equals (right);
		}

		public virtual IEnumerable<TypeInfo> DefinedTypes {
			get {
				foreach (var type in GetTypes ()) {
					yield return type.GetTypeInfo ();
				}
			}
		}

		public virtual IEnumerable<Type> ExportedTypes {
			get { return GetExportedTypes (); }
		}

		public virtual IEnumerable<Module> Modules {
			get { return GetModules (); }
		}

		public virtual IEnumerable<CustomAttributeData> CustomAttributes {
			get { return GetCustomAttributesData (); }
		}
	}
}
