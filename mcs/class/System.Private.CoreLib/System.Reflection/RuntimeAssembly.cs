#nullable disable
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading;

namespace System.Reflection
{
	[StructLayout (LayoutKind.Sequential)]
	sealed class RuntimeAssembly : Assembly
	{
		private sealed class ResolveEventHolder
		{
			public event ModuleResolveEventHandler ModuleResolve;
		}

		private sealed class UnmanagedMemoryStreamForModule : UnmanagedMemoryStream
		{
			Module module;

			public unsafe UnmanagedMemoryStreamForModule (byte* pointer, long length, Module module)
				: base (pointer, length)
			{
				this.module = module;
			}
		}

		//
		// KEEP IN SYNC WITH mcs/class/corlib/System.Reflection/RuntimeAssembly.cs
		//
		#region VM dependency
		IntPtr _mono_assembly;
		object _evidence; 		// Unused, kept for layout compatibility
		#endregion

		ResolveEventHolder resolve_event_holder;

		public override extern MethodInfo? EntryPoint {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public override bool ReflectionOnly => false;

		public override string? CodeBase {
			get {
				return get_code_base (this, false);
			}
		}

		public override string? FullName {
			get {
				return get_fullname (this);
			}
		}

		//
		// We can't store the event directly in this class, since the
		// compiler would silently insert the fields before _mono_assembly
		//
		public override event ModuleResolveEventHandler ModuleResolve {
			add {
				resolve_event_holder.ModuleResolve += value;
			}
			remove {
				resolve_event_holder.ModuleResolve -= value;
			}
		}

		public override Module? ManifestModule => GetManifestModuleInternal ();

		public override bool GlobalAssemblyCache => false;

		public override long HostContext => 0;

		public override string ImageRuntimeVersion => InternalImageRuntimeVersion (this);

		public override string Location {
			get {
				return get_location ();
			}
		}

		// TODO:
		public override bool IsCollectible => false;

		internal static AssemblyName CreateAssemblyName (string assemblyString, out RuntimeAssembly assemblyFromResolveEvent)
		{
           if (assemblyString == null)
                throw new ArgumentNullException (nameof (assemblyString));

            if ((assemblyString.Length == 0) ||
                (assemblyString[0] == '\0'))
                throw new ArgumentException (SR.Format_StringZeroLength);

			assemblyFromResolveEvent = null;
			try {
				return new AssemblyName (assemblyString);
			} catch (Exception) {
				assemblyFromResolveEvent = (RuntimeAssembly)AssemblyLoadContext.DoAssemblyResolve (assemblyString);
				if (assemblyFromResolveEvent == null)
					throw new FileLoadException (assemblyString);
				return null;
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public override extern String[] GetManifestResourceNames ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public override extern Type[] GetExportedTypes ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public override extern Type[] GetForwardedTypes ();

		public override ManifestResourceInfo GetManifestResourceInfo (string resourceName)
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

		public override Stream GetManifestResourceStream (string name)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));

			if (name.Length == 0)
				throw new ArgumentException ("String cannot have zero length.",
					"name");

			unsafe {
				byte* data = (byte*) GetManifestResourceInternal (name, out int length, out Module resourceModule);
				if (data == null)
					return null;

				// It cannot use SafeBuffer mode because not all methods are supported in this
				// mode (e.g. UnmanagedMemoryStream.get_PositionPointer)
				return new UnmanagedMemoryStreamForModule (data, length, resourceModule);
			}
		}

		public override Stream GetManifestResourceStream (Type type, string name)
		{
			if (type == null && name == null)
				throw new ArgumentNullException (nameof (type));

			string nameSpace = type?.Namespace;

			string resourceName = nameSpace != null && name != null ?
				nameSpace + Type.Delimiter + name :
				nameSpace + name;

			return GetManifestResourceStream (resourceName);
		}

		public override AssemblyName GetName(bool copiedName)
		{
			return AssemblyName.Create (this, true);
		}

		public override Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));

			if (name.Length == 0)
				throw new ArgumentException ("name", "Name cannot be empty");

			return InternalGetType (null, name, throwOnError, ignoreCase);
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override IList<CustomAttributeData> GetCustomAttributesData ()
		{
			return CustomAttributeData.GetCustomAttributes (this);
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override Module GetModule (string name)
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

		public override Module[] GetModules (bool getResourceModules)
		{
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

		public override Module[] GetLoadedModules (bool getResourceModules)
		{
			return GetModules (getResourceModules);
		}

		public override AssemblyName[] GetReferencedAssemblies ()
		{
			using (var nativeNames = new Mono.SafeGPtrArrayHandle (InternalGetReferencedAssemblies (this))) {
				var numAssemblies = nativeNames.Length;
				try {
					AssemblyName [] result = new AssemblyName[numAssemblies];
					const bool addVersion = true;
					const bool addPublicKey = false;
					const bool defaultToken = true;
					for (int i = 0; i < numAssemblies; i++) {
						AssemblyName name = new AssemblyName ();
						unsafe {
							Mono.MonoAssemblyName *nativeName = (Mono.MonoAssemblyName*) nativeNames[i];
							name.FillName (nativeName, null, addVersion, addPublicKey, defaultToken);
							result[i] = name;
						}
					}
					return result;
				} finally {
					for (int i = 0; i < numAssemblies; i++) {
						unsafe {
							Mono.MonoAssemblyName* nativeName = (Mono.MonoAssemblyName*) nativeNames[i];
							Mono.RuntimeMarshal.FreeAssemblyName (ref *nativeName, true);
						}
					}
				}
			}
		}

		public override Assembly GetSatelliteAssembly (CultureInfo culture)
		{
			return GetSatelliteAssembly (culture, null);
		}

		public override Assembly GetSatelliteAssembly (CultureInfo culture, Version version)
		{
			if (culture == null)
				throw new ArgumentNullException (nameof (culture));

			return InternalGetSatelliteAssembly (culture, version, true);
		}

		[System.Security.DynamicSecurityMethod] // Methods containing StackCrawlMark local var has to be marked DynamicSecurityMethod
		internal Assembly InternalGetSatelliteAssembly (CultureInfo culture, Version version, bool throwOnFileNotFound)
		{
			var aname = GetName ();

			var an = new AssemblyName ();
			if (version == null)
				an.Version = aname.Version;
			else
				an.Version = version;

			an.CultureInfo = culture;
			an.Name = aname.Name + ".resources";

			Assembly res = null;
			try {
				StackCrawlMark unused = default;
				res = Assembly.Load (an, ref unused, null);
			} catch {
			}

			if (res == this)
				res = null;
			if (res == null && throwOnFileNotFound)
				throw new FileNotFoundException (String.Format (culture, SR.IO_FileNotFound_FileName, an.Name));
			return res;
		}

		public override FileStream GetFile (string name)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name), SR.ArgumentNull_FileName);
			if (name.Length == 0)
				throw new ArgumentException (SR.Argument_EmptyFileName);

			string location = Location;
			if (location != null && Path.GetFileName (location) == name)
				return new FileStream (location, FileMode.Open, FileAccess.Read);
			string filename = (string)GetFilesInternal (name, true);
			if (filename != null)
				return new FileStream (filename, FileMode.Open, FileAccess.Read);
			else
				return null;
		}

		public override FileStream[] GetFiles (bool getResourceModules)
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

		internal static RuntimeAssembly InternalLoadAssemblyName (AssemblyName assemblyRef, ref StackCrawlMark stackMark, AssemblyLoadContext assemblyLoadContext)
		{
			// TODO: Use assemblyLoadContext
			return (RuntimeAssembly) InternalLoad (assemblyRef.FullName, ref stackMark, IntPtr.Zero);
		}

		public override Module LoadModule (string moduleName, byte[] rawModule, byte[] rawSymbolStore)
		{
			throw new NotImplementedException ();
		}

		internal override IntPtr MonoAssembly {
			get {
				return _mono_assembly;
			}
		}

		// FIXME: Merge some of these

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern string get_location ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static string get_code_base (Assembly a, bool escaped);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static string get_fullname (Assembly a);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static string InternalImageRuntimeVersion (Assembly a);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern bool GetManifestResourceInfoInternal (string name, ManifestResourceInfo info);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern IntPtr /* byte* */ GetManifestResourceInternal (string name, out int size, out Module module);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern Module GetManifestModuleInternal ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern Module[] GetModulesInternal ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern IntPtr InternalGetReferencedAssemblies (Assembly module);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern object GetFilesInternal (String name, bool getResourceModules);
	}
}
