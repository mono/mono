//
// System.Reflection.Emit/AssemblyBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

//
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
using System.Reflection;
using System.Resources;
using System.IO;
using System.Security.Policy;
using System.Runtime.Serialization;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security;
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
		public int offset;
	}

	internal struct MonoWin32Resource {
		public int res_type;
		public int res_id;
		public int lang_id;
		public byte[] data;

		public MonoWin32Resource (int res_type, int res_id, int lang_id, byte[] data) {
			this.res_type = res_type;
			this.res_id = res_id;
			this.lang_id = lang_id;
			this.data = data;
		}
	}

	public sealed class AssemblyBuilder : Assembly {
		#region Sync with reflection.h
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
		Module[] loaded_modules;
		MonoWin32Resource[] win32_resources;
		#endregion
		internal Type corlib_object_type = typeof (System.Object);
		internal Type corlib_value_type = typeof (System.ValueType);
		internal Type corlib_enum_type = typeof (System.Enum);
		internal Type corlib_void_type = typeof (void);
		ArrayList resource_writers = null;
		Win32VersionResource version_res;
		bool created;
		bool is_module_only;
		private Mono.Security.StrongName sn;
		PermissionSet required_perm, optional_perm, refused_perm;

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

			if (n.KeyPair != null) {
				// full keypair is available (for signing)
				sn = n.KeyPair.StrongName ();
			}
			else {
				// public key is available (for delay-signing)
				byte[] pk = n.GetPublicKey ();
				if ((pk != null) && (pk.Length > 0)) {
					sn = new Mono.Security.StrongName (pk);
				}
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

		/// <summary>
		/// Don't change the method name and parameters order. It is used by mcs 
		/// </summary>
		[MonoTODO ("Missing support in runtime for parameter applying")]
		internal void AddPermissionRequests (PermissionSet required, PermissionSet optional, PermissionSet refused)
		{
			if (created)
				throw new InvalidOperationException ("Assembly was already saved.");

			required_perm = required;
			optional_perm = optional;
			refused_perm = refused;
		}

		internal void EmbedResourceFile (string name, string fileName)
		{
			EmbedResourceFile (name, fileName, ResourceAttributes.Public);
		}

		internal void EmbedResourceFile (string name, string fileName, ResourceAttributes attribute)
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

			if ((modules != null) && is_module_only)
				throw new InvalidOperationException ("A module-only assembly can only contain one module.");

			if (modules != null) {
				ModuleBuilder[] new_modules = new ModuleBuilder [modules.Length + 1];
				System.Array.Copy(modules, new_modules, modules.Length);
				modules = new_modules;
			} else {
				modules = new ModuleBuilder [1];
			}
			modules [modules.Length - 1] = r;
			return r;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern Module InternalAddModule (string fileName);

		/*
		 * Mono extension to support /addmodule in mcs.
		 */
		internal Module AddModule (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException (fileName);

			Module m = InternalAddModule (fileName);

			if (loaded_modules != null) {
				Module[] new_modules = new Module [loaded_modules.Length + 1];
				System.Array.Copy (loaded_modules, new_modules, loaded_modules.Length);
				loaded_modules = new_modules;
			} else {
				loaded_modules = new Module [1];
			}
			loaded_modules [loaded_modules.Length - 1] = m;

			return m;
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

		private void AddUnmanagedResource (Win32Resource res) {
			MemoryStream ms = new MemoryStream ();
			res.WriteTo (ms);

			if (win32_resources != null) {
				MonoWin32Resource[] new_res = new MonoWin32Resource [win32_resources.Length + 1];
				System.Array.Copy (win32_resources, new_res, win32_resources.Length);
				win32_resources = new_res;
			}
			else
				win32_resources = new MonoWin32Resource [1];

			win32_resources [win32_resources.Length - 1] = new MonoWin32Resource (res.Type.Id, res.Name.Id, res.Language, ms.ToArray ());
		}

		[MonoTODO]
		public void DefineUnmanagedResource (byte[] resource)
		{
			if (resource == null)
				throw new ArgumentNullException ("resource");

			/*
			 * The format of the argument byte array is not documented
			 * so this method is impossible to implement.
			 */

			throw new NotImplementedException ();
		}

		public void DefineUnmanagedResource (string resourceFileName)
		{
			if (resourceFileName == null)
				throw new ArgumentNullException ("resourceFileName");
			if (resourceFileName == String.Empty)
				throw new ArgumentException ("resourceFileName");
			if (!File.Exists (resourceFileName) || Directory.Exists (resourceFileName))
				throw new FileNotFoundException ("File '" + resourceFileName + "' does not exists or is a directory.");

			using (FileStream fs = new FileStream (resourceFileName, FileMode.Open)) {
				Win32ResFileReader reader = new Win32ResFileReader (fs);

				foreach (Win32EncodedResource res in reader.ReadResources ()) {
					if (res.Name.IsName || res.Type.IsName)
						throw new InvalidOperationException ("resource files with named resources or non-default resource types are not supported.");

					AddUnmanagedResource (res);
				}
			}
		}

		public void DefineVersionInfoResource ()
		{
			if (version_res != null)
				throw new ArgumentException ("Native resource has already been defined.");			

			version_res = new Win32VersionResource (1, 0);

			if (cattrs != null) {
				foreach (CustomAttributeBuilder cb in cattrs) {
					string attrname = cb.Ctor.ReflectedType.FullName;

					if (attrname == "System.Reflection.AssemblyProductAttribute")
						version_res.ProductName = cb.string_arg ();
					else if (attrname == "System.Reflection.AssemblyCompanyAttribute")
						version_res.CompanyName = cb.string_arg ();
					else if (attrname == "System.Reflection.AssemblyCopyrightAttribute")
						version_res.LegalCopyright = cb.string_arg ();
					else if (attrname == "System.Reflection.AssemblyTrademarkAttribute")
						version_res.LegalTrademarks = cb.string_arg ();
					else if (attrname == "System.Reflection.AssemblyCultureAttribute")
						version_res.FileLanguage = new CultureInfo (GetCultureString (cb.string_arg ())).LCID;
					else if (attrname == "System.Reflection.AssemblyFileVersionAttribute")
						version_res.FileVersion = cb.string_arg ();
					else if (attrname == "System.Reflection.AssemblyInformationalVersionAttribute")
						version_res.ProductVersion = cb.string_arg ();
					else if (attrname == "System.Reflection.AssemblyTitleAttribute")
						version_res.FileDescription = cb.string_arg ();
					else if (attrname == "System.Reflection.AssemblyDescriptionAttribute")
						version_res.Comments = cb.string_arg ();
				}
			}
		}

		public void DefineVersionInfoResource (string product, string productVersion,
						       string company, string copyright, string trademark)
		{
			if (version_res != null)
				throw new ArgumentException ("Native resource has already been defined.");

			/*
			 * We can only create the resource later, when the file name and
			 * the binary version is known.
			 */

			version_res = new Win32VersionResource (1, 0);
			version_res.ProductName = product;
			version_res.ProductVersion = productVersion;
			version_res.CompanyName = company;
			version_res.LegalCopyright = copyright;
			version_res.LegalTrademarks = trademark;
		}

		/* 
		 * Mono extension to support /win32icon in mcs
		 */
		internal void DefineIconResource (string iconFileName)
		{
			if (iconFileName == null)
				throw new ArgumentNullException ("iconFileName");
			if (iconFileName == String.Empty)
				throw new ArgumentException ("iconFileName");
			if (!File.Exists (iconFileName) || Directory.Exists (iconFileName))
				throw new FileNotFoundException ("File '" + iconFileName + "' does not exists or is a directory.");

			using (FileStream fs = new FileStream (iconFileName, FileMode.Open)) {
				Win32IconFileReader reader = new Win32IconFileReader (fs);
				
				ICONDIRENTRY[] entries = reader.ReadIcons ();

				Win32IconResource[] icons = new Win32IconResource [entries.Length];
				for (int i = 0; i < entries.Length; ++i) {
					icons [i] = new Win32IconResource (i + 1, 0, entries [i]);
					AddUnmanagedResource (icons [i]);
				}

				Win32GroupIconResource group = new Win32GroupIconResource (1, 0, icons);
				AddUnmanagedResource (group);
			}
		}

		private void DefineVersionInfoResourceImpl (string fileName) {
			// Add missing info
			if (version_res.FileVersion == "0.0.0.0")
				version_res.FileVersion = version;
			version_res.InternalName = Path.GetFileNameWithoutExtension (fileName);
			version_res.OriginalFilename = fileName;

			AddUnmanagedResource (version_res);
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

		/*
		 * Mono extension. If this is set, the assembly can only contain one
		 * module, access should be Save, and the saved image will not contain an
		 * assembly manifest.
		 */
		internal bool IsModuleOnly {
			get {
				return is_module_only;
			}
			set {
				is_module_only = value;
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
			if (modules != null) {
				foreach (ModuleBuilder module in modules)
					if (module.FullyQualifiedName == assemblyFileName)
						mainModule = module;
			}
			if (mainModule == null)
				mainModule = DefineDynamicModule ("RefEmit_OnDiskManifestModule", assemblyFileName);

			if (!is_module_only)
				mainModule.IsMain = true;

			/* 
			 * Create a new entry point if the one specified
			 * by the user is in another module.
			 */
			if ((entry_point != null) && entry_point.DeclaringType.Module != mainModule) {
				Type[] paramTypes;
				if (entry_point.GetParameters ().Length == 1)
					paramTypes = new Type [] { typeof (string) };
				else
					paramTypes = new Type [0];

				MethodBuilder mb = mainModule.DefineGlobalMethod ("__EntryPoint$", MethodAttributes.Static|MethodAttributes.PrivateScope, entry_point.ReturnType, paramTypes);
				ILGenerator ilgen = mb.GetILGenerator ();
				if (paramTypes.Length == 1)
					ilgen.Emit (OpCodes.Ldarg_0);
				ilgen.Emit (OpCodes.Tailcall);
				ilgen.Emit (OpCodes.Call, entry_point);
				ilgen.Emit (OpCodes.Ret);

				entry_point = mb;
			}

			if (version_res != null)
				DefineVersionInfoResourceImpl (assemblyFileName);

			if (sn != null) {
				// runtime needs to value to embed it into the assembly
				public_key = sn.PublicKey;
			}

			foreach (ModuleBuilder module in modules)
				if (module != mainModule)
					module.Save ();

			// Write out the main module at the end, because it needs to
			// contain the hash of the other modules
			mainModule.Save ();

			if ((sn != null) && (sn.CanSign)) {
				sn.Sign (System.IO.Path.Combine (this.AssemblyDir, assemblyFileName));
			}

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
			int pos;

			if (attrname == "System.Reflection.AssemblyVersionAttribute") {
				version = create_assembly_version (customBuilder.string_arg ());
				return;
			} else if (attrname == "System.Reflection.AssemblyCultureAttribute") {
				culture = GetCultureString (customBuilder.string_arg ());
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

		internal void SetCorlibTypeBuilders (Type corlib_object_type, Type corlib_value_type, Type corlib_enum_type) {
			this.corlib_object_type = corlib_object_type;
			this.corlib_value_type = corlib_value_type;
			this.corlib_enum_type = corlib_enum_type;
		}

		internal void SetCorlibTypeBuilders (Type corlib_object_type, Type corlib_value_type, Type corlib_enum_type, Type corlib_void_type)
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

		private string GetCultureString (string str)
		{
			return (str == "neutral" ? String.Empty : str);
		}
	}
}
