//
// System.Reflection.Emit/AssemblyBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Security.Policy;
using System.Runtime.Serialization;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Permissions;

using Mono.Security;
using Mono.Security.Cryptography;

namespace System.Reflection.Emit {

	internal struct RefEmitPermissionSet {
		public SecurityAction action;
		public string pset;

		public RefEmitPermissionSet (SecurityAction action, string pset) {
			this.action = action;
			this.pset = pset;
		}
	}

	internal struct MonoResource {
		public byte[] data;
		public string name;
		public string filename;
		public ResourceAttributes attrs;
	}

	public sealed class AssemblyBuilder : Assembly {
		private IntPtr dynamic_assembly;
		private MethodInfo entry_point;
		private ModuleBuilder[] modules;
		private string name;
		private string dir;
		private CustomAttributeBuilder[] cattrs;
		private MonoResource[] resources;
		byte[] public_key;
		string version;
		string culture;
		uint algid;
		uint flags;
		PEFileKinds pekind = PEFileKinds.Dll;
		bool delay_sign;
		uint access;
		internal Type corlib_object_type = typeof (System.Object);
		internal Type corlib_value_type = typeof (System.ValueType);
		internal Type corlib_enum_type = typeof (System.Enum);
		internal Type corlib_void_type = typeof (void);
		ArrayList resource_writers = null;
		bool created;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void basic_init (AssemblyBuilder ab);
		
		internal AssemblyBuilder (AssemblyName n, string directory, AssemblyBuilderAccess access) {
			name = n.Name;
			if (directory == null || directory == String.Empty)
				dir = Directory.GetCurrentDirectory ();
			else
				dir = directory;
			this.access = (uint)access;

			/* Set defaults from n */
			if (n.CultureInfo != null) {
				culture = n.CultureInfo.Name;
			}
			Version v = n.Version;
			if (v != null) {
				version = v.ToString ();
			}

			basic_init (this);
		}

		public override string CodeBase {
			get {
				throw not_supported ();
			}
		}
		
		public override MethodInfo EntryPoint {
			get {
				return entry_point;
			}
		}

		public override string Location {
			get {
				throw not_supported ();
			}
		}

#if NET_1_1
		/* This is to keep signature compatibility with MS.NET */
		public override string ImageRuntimeVersion {
			get {
				return base.ImageRuntimeVersion;
			}
		}
#endif

		public void AddResourceFile (string name, string fileName)
		{
			AddResourceFile (name, fileName, ResourceAttributes.Public);
		}

		public void AddResourceFile (string name, string fileName, ResourceAttributes attribute)
		{
			AddResourceFile (name, fileName, attribute, true);
		}

		private void AddResourceFile (string name, string fileName, ResourceAttributes attribute, bool fileNeedsToExists)
		{
			check_name_and_filename (name, fileName, fileNeedsToExists);

			// Resource files are created/searched under the assembly storage
			// directory
			if (dir != null)
				fileName = Path.Combine (dir, fileName);

			if (resources != null) {
				MonoResource[] new_r = new MonoResource [resources.Length + 1];
				System.Array.Copy(resources, new_r, resources.Length);
				resources = new_r;
			} else {
				resources = new MonoResource [1];
			}
			int p = resources.Length - 1;
			resources [p].name = name;
			resources [p].filename = fileName;
			resources [p].attrs = attribute;
		}

		public void EmbedResourceFile (string name, string fileName)
		{
			EmbedResourceFile (name, fileName, ResourceAttributes.Public);
		}

		public void EmbedResourceFile (string name, string fileName, ResourceAttributes attribute)
		{
			if (resources != null) {
				MonoResource[] new_r = new MonoResource [resources.Length + 1];
				System.Array.Copy(resources, new_r, resources.Length);
				resources = new_r;
			} else {
				resources = new MonoResource [1];
			}
			int p = resources.Length - 1;
			resources [p].name = name;
			resources [p].attrs = attribute;
			try {
				FileStream s = new FileStream (fileName, FileMode.Open, FileAccess.Read);
				long len = s.Length;
				resources [p].data = new byte [len];
				s.Read (resources [p].data, 0, (int)len);
				s.Close ();
			} catch {
				/* do something */
			}
		}

		internal void EmbedResource (string name, byte[] blob, ResourceAttributes attribute)
		{
			if (resources != null) {
				MonoResource[] new_r = new MonoResource [resources.Length + 1];
				System.Array.Copy(resources, new_r, resources.Length);
				resources = new_r;
			} else {
				resources = new MonoResource [1];
			}
			int p = resources.Length - 1;
			resources [p].name = name;
			resources [p].attrs = attribute;
			resources [p].data = blob;
		}

		public ModuleBuilder DefineDynamicModule (string name)
		{
			return DefineDynamicModule (name, name, false, true);
		}

		public ModuleBuilder DefineDynamicModule (string name, bool emitSymbolInfo)
		{
			return DefineDynamicModule (name, name, emitSymbolInfo, true);
		}

		public ModuleBuilder DefineDynamicModule(string name, string fileName)
		{
			return DefineDynamicModule (name, fileName, false, false);
		}

		public ModuleBuilder DefineDynamicModule (string name, string fileName,
							  bool emitSymbolInfo)
		{
			return DefineDynamicModule (name, fileName, emitSymbolInfo, false);
		}

		private ModuleBuilder DefineDynamicModule (string name, string fileName,
												   bool emitSymbolInfo, bool transient)
		{
			check_name_and_filename (name, fileName, false);

			if (!transient) {
				if (Path.GetExtension (fileName) == String.Empty)
					throw new ArgumentException ("Module file name '" + fileName + "' must have file extension.");
				if (!IsSave)
					throw new NotSupportedException ("Persistable modules are not supported in a dynamic assembly created with AssemblyBuilderAccess.Run");
				if (created)
					throw new InvalidOperationException ("Assembly was already saved.");
			}

			ModuleBuilder r = new ModuleBuilder (this, name, fileName, emitSymbolInfo, transient);

			if (modules != null) {
				ModuleBuilder[] new_modules = new ModuleBuilder [modules.Length + 1];
				System.Array.Copy(modules, new_modules, modules.Length);
				new_modules [modules.Length] = r;
				modules = new_modules;
			} else {
				modules = new ModuleBuilder [1];
				modules [0] = r;
			}
			return r;
		}

		public IResourceWriter DefineResource (string name, string description, string fileName)
		{
			return DefineResource (name, description, fileName, ResourceAttributes.Public);
		}

		public IResourceWriter DefineResource (string name, string description,
						       string fileName, ResourceAttributes attribute)
		{
			IResourceWriter writer;

			// description seems to be ignored
			AddResourceFile (name, fileName, attribute, false);
			writer = new ResourceWriter (fileName);
			if (resource_writers == null)
				resource_writers = new ArrayList ();
			resource_writers.Add (writer);
			return writer;
		}

		[MonoTODO]
		public void DefineUnmanagedResource (byte[] resource)
		{
			if (resource == null)
				throw new ArgumentNullException ("resource");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DefineUnmanagedResource (string resourceFileName)
		{
			if (resourceFileName == null)
				throw new ArgumentNullException ("resourceFileName");
			if (resourceFileName == String.Empty)
				throw new ArgumentException ("resourceFileName");
			if (!File.Exists (resourceFileName) || Directory.Exists (resourceFileName))
				throw new FileNotFoundException ("File '" + resourceFileName + "' does not exists or is a directory.");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DefineVersionInfoResource ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DefineVersionInfoResource (string product, string productVersion,
						       string company, string copyright, string trademark)
		{
			throw new NotImplementedException ();
		}

		public ModuleBuilder GetDynamicModule (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name == "")
				throw new ArgumentException ("Name can't be null");

			if (modules != null)
				for (int i = 0; i < modules.Length; ++i)
					if (modules [i].name == name)
						return modules [i];
			return null;
		}

		public override Type[] GetExportedTypes ()
		{
			throw not_supported ();
		}

		public override FileStream GetFile (string name)
		{
			throw not_supported ();
		}

		public override FileStream[] GetFiles(bool getResourceModules) {
			throw not_supported ();
		}

		public override ManifestResourceInfo GetManifestResourceInfo(string resourceName) {
			throw not_supported ();
		}

		public override string[] GetManifestResourceNames() {
			throw not_supported ();
		}

		public override Stream GetManifestResourceStream(string name) {
			throw not_supported ();
		}
		public override Stream GetManifestResourceStream(Type type, string name) {
			throw not_supported ();
		}

		internal bool IsSave {
			get {
				return access != (uint)AssemblyBuilderAccess.Run;
			}
		}

		internal string AssemblyDir {
			get {
				return dir;
			}
		}

		public void Save (string assemblyFileName)
		{
			if (resource_writers != null) {
				foreach (IResourceWriter writer in resource_writers) {
					writer.Generate ();
					writer.Close ();
				}
			}

			// Create a main module if not already created
			ModuleBuilder mainModule = null;
			foreach (ModuleBuilder module in modules)
				if (module.FullyQualifiedName == assemblyFileName)
					mainModule = module;
			if (mainModule == null)
				mainModule = DefineDynamicModule ("RefEmit_OnDiskManifestModule", assemblyFileName);

			mainModule.IsMain = true;

			foreach (ModuleBuilder module in modules)
				if (module != mainModule)
					module.Save ();

			// Write out the main module at the end, because it needs to
			// contain the hash of the other modules
			mainModule.Save ();

			created = true;
		}

		public void SetEntryPoint (MethodInfo entryMethod)
		{
			SetEntryPoint (entryMethod, PEFileKinds.ConsoleApplication);
		}

		public void SetEntryPoint (MethodInfo entryMethod, PEFileKinds fileKind)
		{
			if (entryMethod == null)
				throw new ArgumentNullException ("entryMethod");
			if (entryMethod.DeclaringType.Assembly != this)
				throw new InvalidOperationException ("Entry method is not defined in the same assembly.");

			entry_point = entryMethod;
			pekind = fileKind;
		}

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) 
		{
			if (customBuilder == null)
				throw new ArgumentNullException ("customBuilder");

			string attrname = customBuilder.Ctor.ReflectedType.FullName;
			byte[] data;
			int len, pos;
			Mono.Security.StrongName sn;
			if (attrname == "System.Reflection.AssemblyVersionAttribute") {
				data = customBuilder.Data;
				pos = 2;
				len = CustomAttributeBuilder.decode_len (data, pos, out pos);
				version = create_assembly_version (CustomAttributeBuilder.string_from_bytes (data, pos, len));
				return;
			} else if (attrname == "System.Reflection.AssemblyKeyFileAttribute") {
				data = customBuilder.Data;
				pos = 2;
				len = CustomAttributeBuilder.decode_len (data, pos, out pos);
				string keyfile_name = CustomAttributeBuilder.string_from_bytes (data, pos, len);
				if (keyfile_name == String.Empty)
					return;
				using (FileStream fs = new FileStream (keyfile_name, FileMode.Open)) {
					byte[] snkeypair = new byte [fs.Length];
					fs.Read (snkeypair, 0, snkeypair.Length);

					// this will import public or private/public keys
					RSA rsa = CryptoConvert.FromCapiKeyBlob (snkeypair);
					// and export only the public part
					sn = new Mono.Security.StrongName (rsa);
					public_key = sn.PublicKey;
				}
				return;
			} else if (attrname == "System.Reflection.AssemblyKeyNameAttribute") {
				data = customBuilder.Data;
				pos = 2;
				len = CustomAttributeBuilder.decode_len (data, pos, out pos);
				string key_name = CustomAttributeBuilder.string_from_bytes (data, pos, len);
				if (key_name == String.Empty)
					return;
				CspParameters csparam = new CspParameters ();
				csparam.KeyContainerName = key_name;
				RSA rsacsp = new RSACryptoServiceProvider (csparam);
				sn = new Mono.Security.StrongName (rsacsp);
				public_key = sn.PublicKey;
				return;
			} else if (attrname == "System.Reflection.AssemblyCultureAttribute") {
				data = customBuilder.Data;
				pos = 2;
				len = CustomAttributeBuilder.decode_len (data, pos, out pos);
				culture = CustomAttributeBuilder.string_from_bytes (data, pos, len);
			} else if (attrname == "System.Reflection.AssemblyAlgorithmIdAttribute") {
				data = customBuilder.Data;
				pos = 2;
				algid = (uint)data [pos];
				algid |= ((uint)data [pos + 1]) << 8;
				algid |= ((uint)data [pos + 2]) << 16;
				algid |= ((uint)data [pos + 3]) << 24;
			} else if (attrname == "System.Reflection.AssemblyFlagsAttribute") {
				data = customBuilder.Data;
				pos = 2;
				flags = (uint)data [pos];
				flags |= ((uint)data [pos + 1]) << 8;
				flags |= ((uint)data [pos + 2]) << 16;
				flags |= ((uint)data [pos + 3]) << 24;
				return;
			} else if (attrname == "System.Reflection.AssemblyDelaySignAttribute") {
				data = customBuilder.Data;
				pos = 2;
				delay_sign = data [2] != 0;
			}
			if (cattrs != null) {
				CustomAttributeBuilder[] new_array = new CustomAttributeBuilder [cattrs.Length + 1];
				cattrs.CopyTo (new_array, 0);
				new_array [cattrs.Length] = customBuilder;
				cattrs = new_array;
			} else {
				cattrs = new CustomAttributeBuilder [1];
				cattrs [0] = customBuilder;
			}
		}
		public void SetCustomAttribute ( ConstructorInfo con, byte[] binaryAttribute) {
			if (con == null)
				throw new ArgumentNullException ("con");
			if (binaryAttribute == null)
				throw new ArgumentNullException ("binaryAttribute");

			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}

		public void SetCorlibTypeBuilders (Type corlib_object_type, Type corlib_value_type, Type corlib_enum_type) {
			this.corlib_object_type = corlib_object_type;
			this.corlib_value_type = corlib_value_type;
			this.corlib_enum_type = corlib_enum_type;
		}

		public void SetCorlibTypeBuilders (Type corlib_object_type, Type corlib_value_type, Type corlib_enum_type, Type corlib_void_type)
		{
			SetCorlibTypeBuilders (corlib_object_type, corlib_value_type, corlib_enum_type);
			this.corlib_void_type = corlib_void_type;
		}

		private Exception not_supported () {
			// Strange message but this is what MS.NET prints...
			return new NotSupportedException ("The invoked member is not supported in a dynamic module.");
		}

		private void check_name_and_filename (string name, string fileName,
											  bool fileNeedsToExists) {
			if (name == null)
				throw new ArgumentNullException ("name");
			if (fileName == null)
				throw new ArgumentNullException ("fileName");
			if (name == "")
				throw new ArgumentException ("name cannot be empty", "name");
			if (fileName == "")
				throw new ArgumentException ("fileName cannot be empty", "fileName");
			if (Path.GetFileName (fileName) != fileName)
				throw new ArgumentException ("fileName '" + fileName + "' must not include a path.");

			// Resource files are created/searched under the assembly storage
			// directory
			string fullFileName = fileName;
			if (dir != null)
				fullFileName = Path.Combine (dir, fileName);

			if (fileNeedsToExists && !File.Exists (fullFileName))
				throw new FileNotFoundException ("Could not find file '" + fileName + "'");

			if (resources != null) {
				for (int i = 0; i < resources.Length; ++i) {
					if (resources [i].filename == fullFileName)
						throw new ArgumentException ("Duplicate file name '" + fileName + "'");
					if (resources [i].name == name)
						throw new ArgumentException ("Duplicate name '" + name + "'");
				}
			}

			if (modules != null) {
				for (int i = 0; i < modules.Length; ++i) {
					// Use fileName instead of fullFileName here
					if (!modules [i].IsTransient () && (modules [i].FileName == fileName))
						throw new ArgumentException ("Duplicate file name '" + fileName + "'");
					if (modules [i].Name == name)
						throw new ArgumentException ("Duplicate name '" + name + "'");
				}
			}
		}

		private String create_assembly_version (String version) {
			String[] parts = version.Split ('.');
			int[] ver = new int [4] { 0, 0, 0, 0 };

			if ((parts.Length < 0) || (parts.Length > 4))
				throw new ArgumentException ("The version specified '" + version + "' is invalid");

			for (int i = 0; i < parts.Length; ++i) {
				if (parts [i] == "*") {
					DateTime now = DateTime.Now;

					if (i == 2) {
						ver [2] = (now - new DateTime (2000, 1, 1)).Days;
						if (parts.Length == 3)
							ver [3] = (now.Second + (now.Minute * 60) + (now.Hour * 3600)) / 2;
					}
					else
						if (i == 3)
							ver [3] = (now.Second + (now.Minute * 60) + (now.Hour * 3600)) / 2;
					else
						throw new ArgumentException ("The version specified '" + version + "' is invalid");
				}
				else {
					try {
						ver [i] = Int32.Parse (parts [i]);
					}
					catch (FormatException) {
						throw new ArgumentException ("The version specified '" + version + "' is invalid");
					}
				}
			}

			return ver [0] + "." + ver [1] + "." + ver [2] + "." + ver [3];
		}
	}
}
