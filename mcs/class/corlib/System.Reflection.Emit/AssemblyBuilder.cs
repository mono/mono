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

namespace System.Reflection.Emit {

	public sealed class AssemblyBuilder : Assembly {
		private IntPtr dynamic_assembly;
		private MethodInfo entry_point;
		private ModuleBuilder[] modules;
		private string name;
		private string dir;
		private CustomAttributeBuilder[] cattrs;
		internal Type corlib_object_type = typeof (System.Object);
		internal Type corlib_value_type = typeof (System.ValueType);
		private int[] table_indexes;
		internal ArrayList methods;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void basic_init (AssemblyBuilder ab);
		
		internal AssemblyBuilder (AssemblyName n, string directory, AssemblyBuilderAccess access) {
			name = n.Name;
			dir = directory;
			basic_init (this);
		}

		internal int get_next_table_index (object obj, int table, bool inc) {
			if (table_indexes == null) {
				table_indexes = new int [64];
				for (int i=0; i < 64; ++i)
					table_indexes [i] = 1;
				/* allow room for .<Module> in TypeDef table */
				table_indexes [0x02] = 2;
			}
			// Console.WriteLine ("getindex for table "+table.ToString()+" got "+table_indexes [table].ToString());
			if (inc) {
				if ((table == 0x06) && (methods != null))
					methods.Add (obj);
				return table_indexes [table]++;
			}
			return table_indexes [table];
		}

		public override string CodeBase {
			get {
				return null;
			}
		}
		
		public override MethodInfo EntryPoint {
			get {
				return entry_point;
			}
		}

		public override string Location {
			get {
				return null;
			}
		}

		public void AddResourceFile (string name, string fileName)
		{
		}

		public void AddResourceFile (string name, string fileName, ResourceAttributes attribute)
		{
		}

		public ModuleBuilder DefineDynamicModule (string name)
		{
			return DefineDynamicModule (name, name, false);
		}

		public ModuleBuilder DefineDynamicModule (string name, bool emitSymbolInfo)
		{
			return DefineDynamicModule (name, name, emitSymbolInfo);
		}

		public ModuleBuilder DefineDynamicModule(string name, string fileName)
		{
			return DefineDynamicModule (name, fileName, false);
		}

		public ModuleBuilder DefineDynamicModule (string name, string fileName,
							  bool emitSymbolInfo)
		{
			ModuleBuilder r = new ModuleBuilder (this, name, fileName, emitSymbolInfo);

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
			return null;
		}

		public IResourceWriter DefineResource (string name, string description,
						       string fileName, ResourceAttributes attribute)
		{
			return null;
		}

		public void DefineUnmanagedResource (byte[] resource)
		{
		}

		public void DefineUnmanagedResource (string resourceFileName)
		{
		}

		public void DefineVersionInfoResource ()
		{
		}

		public void DefineVersionInfoResource (string product, string productVersion,
						       string company, string copyright, string trademark)
		{
		}

		public ModuleBuilder GetDynamicModule (string name)
		{
			return null;
		}

		public override Type[] GetExportedTypes ()
		{
			return null;
		}

		public override FileStream GetFile (string name)
		{
			return null;
		}

		/*public virtual FileStream[] GetFiles() {
			return null;
		}
		public override FileStream[] GetFiles(bool getResourceModules) {
			return null;
		}*/

		/*public virtual ManifestResourceInfo GetManifestResourceInfo(string resourceName)
		  {
			return null;
		}
		public virtual string[] GetManifestResourceNames() {
			return null;
		}
		public virtual Stream GetManifestResourceStream(string name) {
			return null;
		}
		public virtual Stream GetManifestResourceStream(Type type, string name) {
			return null;
		}*/

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int getUSIndex (AssemblyBuilder ab, string str);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int getToken (AssemblyBuilder ab, MemberInfo member);

		internal int GetToken (string str) {
			return getUSIndex (this, str);
		}
		
		internal int GetToken (MemberInfo member) {
			return getToken (this, member);
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int getDataChunk (AssemblyBuilder ab, byte[] buf, int offset);

		public void Save (string assemblyFileName)
		{
			byte[] buf = new byte [65536];
			FileStream file;
			int count, offset;

			if (dir != null) {
				assemblyFileName = String.Format ("{0}{1}{2}", dir, System.IO.Path.DirectorySeparatorChar, assemblyFileName);
			}

			file = new FileStream (assemblyFileName, FileMode.Create, FileAccess.Write);

			offset = 0;
			while ((count = getDataChunk (this, buf, offset)) != 0) {
				file.Write (buf, 0, count);
				offset += count;
			}
			file.Close ();
		}

		public void SetEntryPoint (MethodInfo entryMethod)
		{
			entry_point = entryMethod;
		}

		public void SetEntryPoint (MethodInfo entryMethod, PEFileKinds fileKind)
		{
			entry_point = entryMethod;
		}

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
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
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}

		public void SetCorlibTypeBuilders (Type corlib_object_type, Type corlib_value_type) {
			this.corlib_object_type = corlib_object_type;
			this.corlib_value_type = corlib_value_type;
		}
	}
}
