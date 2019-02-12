using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Runtime.Serialization;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection
{
	[StructLayout (LayoutKind.Sequential)]
	class RuntimeAssembly : Assembly
	{
		//
		// KEEP IN SYNC WITH mcs/class/corlib/System.Reflection/RuntimeAssembly.cs
		//
		#region
#pragma warning disable 649
		internal IntPtr _mono_assembly;
#pragma warning restore 649
		// Unused in netcore, kept for layout compatibility
		object _evidence;
		#endregion

		bool fromByteArray;
		string assemblyName;
		ResolveEventHolder resolve_event_holder;

		internal class ResolveEventHolder {
#pragma warning disable 67
			public event ModuleResolveEventHandler ModuleResolve;
#pragma warning restore
		}

		// FIXME: Merge some of these

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string get_location ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static string get_code_base (Assembly a, bool escaped);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string get_fullname (Assembly a);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string InternalImageRuntimeVersion (Assembly a);

		public override extern MethodInfo EntryPoint {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public override extern bool ReflectionOnly {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public override extern String[] GetManifestResourceNames ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool GetManifestResourceInfoInternal (string name, ManifestResourceInfo info);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern IntPtr GetManifestResourceInternal (String name, out int size, out Module module);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Module GetManifestModuleInternal ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Module[] GetModulesInternal ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern IntPtr InternalGetReferencedAssemblies (Assembly module);

		public override Type[] GetExportedTypes() {
			throw new NotImplementedException ();
		}

		public override Type[] GetForwardedTypes() {
			throw new NotImplementedException ();
		}

		public override string CodeBase {
			get {
				return get_code_base (this, false);
			}
		}

		public override string FullName {
			get {
				return get_fullname (this);
			}
		}

		public override string ImageRuntimeVersion {
			get {
				return InternalImageRuntimeVersion (this);
			}
		}

		public override string Location {
			get {
				if (fromByteArray)
					return String.Empty;

				return get_location ();
			}
		}

		public override bool IsCollectible {
			get {
				return false;
			}
		}

		public override ManifestResourceInfo GetManifestResourceInfo (string resourceName) {
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

		public override Stream GetManifestResourceStream (string name) {
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException ("String cannot have zero length.",
					"name");

			ManifestResourceInfo info = GetManifestResourceInfo (name);
			if (info == null) {
				// FIXME: resource resolve
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

			if (IsCollectible)
				throw new NotImplementedException ();

			int size;
			Module module;
			IntPtr data = GetManifestResourceInternal (name, out size, out module);
			if (data == (IntPtr) 0)
				return null;
			else {
				var buffer = new ResourceSafeBuffer (data, (ulong)size);
				return new UnmanagedMemoryStream (buffer, 0, size);
			}
		}

		public override Stream GetManifestResourceStream (Type type, string name) {
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

		public override AssemblyName GetName(bool copiedName) {
			return AssemblyName.Create (this, true);
		}

		public override Type GetType(string name, bool throwOnError, bool ignoreCase) {
			Type res;
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException ("name", "Name cannot be empty");

			return InternalGetType (null, name, throwOnError, ignoreCase);
		}

		public override bool IsDefined(Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override IList<CustomAttributeData> GetCustomAttributesData() {
			return CustomAttributeData.GetCustomAttributes (this);
		}

		public override object[] GetCustomAttributes(bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override object CreateInstance(string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
		{
		    Type t = GetType(typeName, throwOnError: false, ignoreCase: ignoreCase);
		    if (t == null)
				return null;

		    return Activator.CreateInstance(t, bindingAttr, binder, args, culture, activationAttributes);
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

		public override Module ManifestModule {
			get {
				return GetManifestModuleInternal ();
			}
		}

		public override Module GetModule (string name) {
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

		public override Module[] GetModules (bool getResourceModules) {
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

		public override Module[] GetLoadedModules (bool getResourceModules) {
			return GetModules (getResourceModules);
		}

		public override AssemblyName[] GetReferencedAssemblies() {
			using (var nativeNames = new Mono.SafeGPtrArrayHandle (InternalGetReferencedAssemblies (this))) {
				var numAssemblies = nativeNames.Length;
				try {
					AssemblyName [] result = new AssemblyName[numAssemblies];
					const bool addVersion = true;
					const bool addPublicKey = false;
					const bool defaultToken = true;
					const bool assemblyRef = true;
					for (int i = 0; i < numAssemblies; i++) {
						AssemblyName name = new AssemblyName ();
						unsafe {
							Mono.MonoAssemblyName *nativeName = (Mono.MonoAssemblyName*) nativeNames[i];
							name.FillName (nativeName, null, addVersion, addPublicKey, defaultToken, assemblyRef);
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

		public override Assembly GetSatelliteAssembly(CultureInfo culture) {
			throw new NotImplementedException ();
		}

		public override Assembly GetSatelliteAssembly(CultureInfo culture, Version version) {
			throw new NotImplementedException ();
		}

		public override FileStream GetFile(string name) {
			throw new NotImplementedException ();
		}

		public override FileStream[] GetFiles(bool getResourceModules) {
			throw new NotImplementedException ();
		}

		public override bool GlobalAssemblyCache {
			get {
				throw new NotImplementedException ();
			}
		}

		public override long HostContext {
			get {
				throw new NotImplementedException ();
			}
		}

		public override Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore) {
			throw new NotImplementedException ();
		}

		internal override IntPtr MonoAssembly {
			get {
				return _mono_assembly;
			}
		}

		internal class ResourceSafeBuffer : SafeBuffer
		{
			public ResourceSafeBuffer (IntPtr data, ulong size) : base (true) {
				SetHandle (data);
				Initialize (size);
			}

            protected override unsafe bool ReleaseHandle () {
                return true;
            }
		}
	}
}
