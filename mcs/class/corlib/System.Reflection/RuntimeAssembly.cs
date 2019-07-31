//
// System.Reflection/RuntimeAssembly.cs
//
// Author:
//   Rodrigo Kumpera (rkumpera@novell.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
#if !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Diagnostics.Contracts;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;

using Mono;

namespace System.Reflection {

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_Assembly))]
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	[StructLayout (LayoutKind.Sequential)]
	class RuntimeAssembly : Assembly
	{
		#region Sync with AssemblyBuilder.cs and ReflectionAssembly in object-internals.h
#pragma warning disable 649
		internal IntPtr _mono_assembly;
#pragma warning restore 649
#if !MOBILE
		internal Evidence _evidence;
#else
		object _evidence;
#endif
		#endregion

		internal ResolveEventHolder resolve_event_holder;
#if !MOBILE
		internal PermissionSet _minimum;	// for SecurityAction.RequestMinimum
		internal PermissionSet _optional;	// for SecurityAction.RequestOptional
		internal PermissionSet _refuse;		// for SecurityAction.RequestRefuse
		internal PermissionSet _granted;		// for the resolved assembly granted permissions
		internal PermissionSet _denied;		// for the resolved assembly denied permissions
#else
		object _minimum, _optional, _refuse, _granted, _denied;
#endif
		internal bool fromByteArray;
		internal string assemblyName;

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

		protected RuntimeAssembly ()
		{
			resolve_event_holder = new ResolveEventHolder ();
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			UnitySerializationHolder.GetUnitySerializationInfo (info,
                                                               UnitySerializationHolder.AssemblyUnity,
                                                               this.FullName,
                                                               this);
		}

		internal static RuntimeAssembly GetExecutingAssembly (ref StackCrawlMark stackMark)
		{
			// Mono runtime does not support StackCrawlMark, The easiest workaround is to replace use
			// of StackCrawlMark.LookForMyCaller with GetCallingAssembly
			throw new NotSupportedException ();
		}

        // Creates AssemblyName. Fills assembly if AssemblyResolve event has been raised.
        [System.Security.SecurityCritical]  // auto-generated
        internal static AssemblyName CreateAssemblyName(
            String assemblyString, 
            bool forIntrospection, 
            out RuntimeAssembly assemblyFromResolveEvent)
        {
            if (assemblyString == null)
                throw new ArgumentNullException("assemblyString");
            Contract.EndContractBlock();

            if ((assemblyString.Length == 0) ||
                (assemblyString[0] == '\0'))
                throw new ArgumentException(Environment.GetResourceString("Format_StringZeroLength"));

            if (forIntrospection)
                AppDomain.CheckReflectionOnlyLoadSupported();

            AssemblyName an = new AssemblyName();

            an.Name = assemblyString;
            assemblyFromResolveEvent = null; // instead of an.nInit(out assemblyFromResolveEvent, forIntrospection, true);
            return an;
        }

        internal static RuntimeAssembly InternalLoadAssemblyName(
            AssemblyName assemblyRef, 
            Evidence assemblySecurity,
            RuntimeAssembly reqAssembly,
            ref StackCrawlMark stackMark,
#if FEATURE_HOSTED_BINDER
            IntPtr pPrivHostBinder,
#endif
            bool throwOnFileNotFound, 
            bool forIntrospection,
            bool suppressSecurityChecks)
        {
            if (assemblyRef == null)
                throw new ArgumentNullException("assemblyRef");
            Contract.EndContractBlock();

            if (assemblyRef.CodeBase != null)
            {
                AppDomain.CheckLoadFromSupported();
            }

            assemblyRef = (AssemblyName)assemblyRef.Clone();
#if FEATURE_VERSIONING
            if (!forIntrospection &&
                (assemblyRef.ProcessorArchitecture != ProcessorArchitecture.None)) {
                // PA does not have a semantics for by-name binds for execution
                assemblyRef.ProcessorArchitecture = ProcessorArchitecture.None;
            }
#endif

            if (assemblySecurity != null)
            {
#if FEATURE_CAS_POLICY
                if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
                }
#endif // FEATURE_CAS_POLICY

                if (!suppressSecurityChecks)
                {
#if MONO_FEATURE_CAS
#pragma warning disable 618
                    new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
#pragma warning restore 618
#endif
                }
            }

			return (RuntimeAssembly) Assembly.Load (assemblyRef);
		}

		internal static RuntimeAssembly LoadWithPartialNameInternal (String partialName, Evidence securityEvidence, ref StackCrawlMark stackMark)
		{
			// Mono runtime does not support StackCrawlMark
			//FIXME stackMark should probably change method behavior in some cases.
			return (RuntimeAssembly) Assembly.LoadWithPartialName (partialName, securityEvidence);
		}

		internal static RuntimeAssembly LoadWithPartialNameInternal (AssemblyName an, Evidence securityEvidence, ref StackCrawlMark stackMark)
		{
			return LoadWithPartialNameInternal (an.ToString (), securityEvidence, ref stackMark);
		}

		// the security runtime requires access to the assemblyname (e.g. to get the strongname)
		public override AssemblyName GetName (bool copiedName)
		{
#if !MOBILE
			// CodeBase, which is restricted, will be copied into the AssemblyName object so...
			if (SecurityManager.SecurityEnabled) {
				var _ = CodeBase; // this will ensure the Demand is made
			}
#endif
			return AssemblyName.Create (this, true);
		}

		public
		override
		Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			Type res;
			if (name == null)
				throw new ArgumentNullException (name);
			if (name.Length == 0)
			throw new ArgumentException ("name", "Name cannot be empty");

			res = InternalGetType (null, name, throwOnError, ignoreCase);
			return res;
		}

		public
		override
		Module GetModule (String name)
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

		public
		override
		AssemblyName[] GetReferencedAssemblies () {
			return GetReferencedAssemblies (this);
		}

		public
		override
		Module[] GetModules (bool getResourceModules) {
			Module[] modules = GetModulesInternal ();

			if (!getResourceModules) {
				var result = new List<Module> (modules.Length);
				foreach (Module m in modules)
					if (!m.IsResource ())
						result.Add (m);
				return result.ToArray ();
			}
			else
				return modules;
		}

		[MonoTODO ("Always returns the same as GetModules")]
		public
		override
		Module[] GetLoadedModules (bool getResourceModules)
		{
			return GetModules (getResourceModules);
		}

        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public
		override
		Assembly GetSatelliteAssembly (CultureInfo culture)
		{
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return GetSatelliteAssembly (culture, null, true, ref stackMark);
		}

        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public
		override
		Assembly GetSatelliteAssembly (CultureInfo culture, Version version)
		{
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return GetSatelliteAssembly (culture, version, true, ref stackMark);
		}

		//FIXME remove GetManifestModule under v4, it's a v2 artifact
		[ComVisible (false)]
		public
		override
		Module ManifestModule {
			get {
				return GetManifestModule ();
			}
		}

		public
		override
		bool GlobalAssemblyCache {
			get {
				return get_global_assembly_cache ();
			}
		}

		public override Type[] GetExportedTypes ()
		{
			return GetTypes (true);
		}

		internal static byte[] GetAotId () {
			var guid = new byte [16];
			var res = GetAotIdInternal (guid);
			if (res)
				return guid;
			else
				return null;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static string get_code_base (Assembly a, bool escaped);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string get_location ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string get_fullname (Assembly a);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static bool GetAotIdInternal (byte[] aotid);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string InternalImageRuntimeVersion (Assembly a);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern bool get_global_assembly_cache ();

		public override extern MethodInfo EntryPoint {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		[ComVisible (false)]
		public override extern bool ReflectionOnly {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		// SECURITY: this should be the only caller to icall get_code_base
		internal static string GetCodeBase (Assembly a, bool escaped)
		{
			string cb = get_code_base (a, escaped);
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

		public override string CodeBase {
			get { return GetCodeBase (this, false); }
		}

		public override string EscapedCodeBase {
			get { return GetCodeBase (this, true); }
		}

		public override string FullName {
			get {
				return get_fullname (this);
			}
		}

		[ComVisible (false)]
		public override string ImageRuntimeVersion {
			get {
				return InternalImageRuntimeVersion (this);
			}
		}

		internal override IntPtr MonoAssembly {
			get {
				return _mono_assembly;
			}
		}

		internal override bool FromByteArray {
			set { fromByteArray = value; }
		}

		public override String Location {
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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool GetManifestResourceInfoInternal (String name, ManifestResourceInfo info);

		public override ManifestResourceInfo GetManifestResourceInfo (String resourceName)
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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public override extern String[] GetManifestResourceNames ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern IntPtr GetManifestResourceInternal (String name, out int size, out Module module);

		public override Stream GetManifestResourceStream (String name)
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

		[MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public override Stream GetManifestResourceStream (Type type, String name)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return GetManifestResourceStream(type, name, false, ref stackMark);
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object [] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public override object [] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override IList<CustomAttributeData> GetCustomAttributesData ()
		{
			return CustomAttributeData.GetCustomAttributes (this);
		}

		//
		// We can't store the event directly in this class, since the
		// compiler would silently insert the fields before _mono_assembly
		//
		public override event ModuleResolveEventHandler ModuleResolve {
			[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
			add {
				resolve_event_holder.ModuleResolve += value;
			}
			[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
			remove {
				resolve_event_holder.ModuleResolve -= value;
			}
		}

		internal override Module GetManifestModule () {
			return GetManifestModuleInternal ();
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Module GetManifestModuleInternal ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal override extern Module[] GetModulesInternal ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern object GetFilesInternal (String name, bool getResourceModules);

		public override FileStream [] GetFiles (bool getResourceModules)
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

		public override FileStream GetFile (String name)
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
			
			if (!(o is RuntimeAssembly))
				return false;
			var other = (RuntimeAssembly) o;
			return other._mono_assembly == _mono_assembly;
		}

		public override string ToString ()
		{
			// note: ToString work without requiring CodeBase (so no checks are needed)

			if (assemblyName != null)
				return assemblyName;

			assemblyName = FullName;
			return assemblyName;
		}

		public override Evidence Evidence {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence = true)]
			get { return UnprotectedGetEvidence (); }
		}

		// note: the security runtime requires evidences but may be unable to do so...
		internal override Evidence UnprotectedGetEvidence ()
		{
#if MOBILE || DISABLE_SECURITY
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

		internal override PermissionSet GrantedPermissionSet {
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

		internal override PermissionSet DeniedPermissionSet {
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
		
		public override PermissionSet PermissionSet {
			get { return this.GrantedPermissionSet; }
		}
#endif
	}
}
