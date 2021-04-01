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

//
// System.Reflection.Emit/ModuleBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

#if MONO_FEATURE_SRE
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Resources;
using System.Globalization;

namespace System.Reflection.Emit {

#if !MOBILE
	[ComVisible (true)]
	[ComDefaultInterface (typeof (_ModuleBuilder))]
	[ClassInterface (ClassInterfaceType.None)]
	partial class ModuleBuilder : _ModuleBuilder
	{
		void _ModuleBuilder.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _ModuleBuilder.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _ModuleBuilder.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _ModuleBuilder.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
	}
#endif

	[StructLayout (LayoutKind.Sequential)]
	public partial class ModuleBuilder : Module {

#pragma warning disable 169, 414
		#region Sync with object-internals.h
		// This class inherits from Module, but the runtime expects it to have the same layout as MonoModule
		#region Sync with MonoModule
		internal IntPtr _impl; /* a pointer to a MonoImage */
		internal Assembly assembly;
		internal string fqname;
		internal string name;
		internal string scopename;
		internal bool is_resource;
		internal int token;
		#endregion
		private UIntPtr dynamic_image; /* GC-tracked */
		private int num_types;
		private TypeBuilder[] types;
		private CustomAttributeBuilder[] cattrs;
		private byte[] guid;
		private int table_idx;
		internal AssemblyBuilder assemblyb;
		private MethodBuilder[] global_methods;
		private FieldBuilder[] global_fields;
		bool is_main;
		private MonoResource[] resources;
		private IntPtr unparented_classes;
		private int[] table_indexes;
		#endregion
#pragma warning restore 169, 414
		
		private TypeBuilder global_type;
		private Type global_type_created;
		// name_cache keys are display names
		Dictionary<TypeName, TypeBuilder> name_cache;
		Dictionary<string, int> us_string_cache;
		bool transient;
		ModuleBuilderTokenGenerator token_gen;
		Hashtable resource_writers;
		ISymbolWriter symbolWriter;

		static bool has_warned_about_symbolWriter;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void basic_init (ModuleBuilder ab);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void set_wrappers_type (ModuleBuilder mb, Type ab);

		internal ModuleBuilder (AssemblyBuilder assb, string name, string fullyqname, bool emitSymbolInfo, bool transient) {
			this.name = this.scopename = name;
			this.fqname = fullyqname;
			this.assembly = this.assemblyb = assb;
			this.transient = transient;
			// to keep mcs fast we do not want CryptoConfig wo be involved to create the RNG
			guid = Guid.FastNewGuidArray ();
			// guid = Guid.NewGuid().ToByteArray ();
			table_idx = get_next_table_index (this, 0x00, 1);
			name_cache = new Dictionary<TypeName, TypeBuilder> ();
			us_string_cache = new Dictionary<string, int> (512);

			basic_init (this);

			CreateGlobalType ();

			if (assb.IsRun) {
				TypeBuilder tb = new TypeBuilder (this, TypeAttributes.Abstract, 0xFFFFFF); /*last valid token*/
				Type type = tb.CreateType ();
				set_wrappers_type (this, type);
			}

			if (emitSymbolInfo) {
				Assembly asm = Assembly.LoadWithPartialName ("Mono.CompilerServices.SymbolWriter");

				Type t = null;
				if (asm != null)
					t = asm.GetType ("Mono.CompilerServices.SymbolWriter.SymbolWriterImpl");

				if (t == null) {
					WarnAboutSymbolWriter ("Failed to load the default Mono.CompilerServices.SymbolWriter assembly");
				} else {
					try {
						symbolWriter = (ISymbolWriter) Activator.CreateInstance (t, new object[] { this });
					} catch (System.MissingMethodException) {
						WarnAboutSymbolWriter ("The default Mono.CompilerServices.SymbolWriter is not available on this platform");					
						return;
					}
				}
				
				string fileName = fqname;
				if (assemblyb.AssemblyDir != null)
					fileName = Path.Combine (assemblyb.AssemblyDir, fileName);
				symbolWriter.Initialize (IntPtr.Zero, fileName, true);
			}
		}

		static void WarnAboutSymbolWriter (string message) 
		{
			if (has_warned_about_symbolWriter)
				return;

			has_warned_about_symbolWriter = true;
			Console.Error.WriteLine ("WARNING: {0}", message);
		}

		public override string FullyQualifiedName {
			get { 
				string fullyQualifiedName = fqname;
				if (fullyQualifiedName == null)
					return null;
				if (assemblyb.AssemblyDir != null) {
					fullyQualifiedName = Path.Combine (assemblyb.AssemblyDir, fullyQualifiedName);
					fullyQualifiedName = Path.GetFullPath (fullyQualifiedName);
				}

				return fullyQualifiedName;
			}
		}

		public bool IsTransient () {
			return transient;
		}

		public void CreateGlobalFunctions () 
		{
			if (global_type_created != null)
				throw new InvalidOperationException ("global methods already created");
			if (global_type != null)
				global_type_created = global_type.CreateType ();
		}

		public FieldBuilder DefineInitializedData( string name, byte[] data, FieldAttributes attributes) {
			if (data == null)
				throw new ArgumentNullException ("data");

			var maskedAttributes = attributes & ~FieldAttributes.ReservedMask;
			FieldBuilder fb = DefineDataImpl (name, data.Length, maskedAttributes | FieldAttributes.HasFieldRVA);
			fb.SetRVAData (data);

			return fb;
		}

		public FieldBuilder DefineUninitializedData (string name, int size, FieldAttributes attributes)
		{
			return DefineDataImpl (name, size, attributes & ~FieldAttributes.ReservedMask);
		}

		private FieldBuilder DefineDataImpl (string name, int size, FieldAttributes attributes)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name == String.Empty)
				throw new ArgumentException ("name cannot be empty", "name");
			if (global_type_created != null)
				throw new InvalidOperationException ("global fields already created");
			if ((size <= 0) || (size >= 0x3f0000))
				throw new ArgumentException ("Data size must be > 0 and < 0x3f0000", null as string);

			CreateGlobalType ();

			string typeName = "$ArrayType$" + size;
			Type datablobtype = GetType (typeName, false, false);
			if (datablobtype == null) {
				TypeBuilder tb = DefineType (typeName, 
				    TypeAttributes.Public|TypeAttributes.ExplicitLayout|TypeAttributes.Sealed,
					assemblyb.corlib_value_type, null, PackingSize.Size1, size);
				tb.CreateType ();
				datablobtype = tb;
			}
			FieldBuilder fb = global_type.DefineField (name, datablobtype, attributes|FieldAttributes.Static);

			if (global_fields != null) {
				FieldBuilder[] new_fields = new FieldBuilder [global_fields.Length+1];
				System.Array.Copy (global_fields, new_fields, global_fields.Length);
				new_fields [global_fields.Length] = fb;
				global_fields = new_fields;
			} else {
				global_fields = new FieldBuilder [1];
				global_fields [0] = fb;
			}
			return fb;
		}

		private void addGlobalMethod (MethodBuilder mb) {
			if (global_methods != null) {
				MethodBuilder[] new_methods = new MethodBuilder [global_methods.Length+1];
				System.Array.Copy (global_methods, new_methods, global_methods.Length);
				new_methods [global_methods.Length] = mb;
				global_methods = new_methods;
			} else {
				global_methods = new MethodBuilder [1];
				global_methods [0] = mb;
			}
		}

		public MethodBuilder DefineGlobalMethod (string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
		{
			return DefineGlobalMethod (name, attributes, CallingConventions.Standard, returnType, parameterTypes);
		}

		public MethodBuilder DefineGlobalMethod (string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes) {
			return DefineGlobalMethod (name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
		}

		public MethodBuilder DefineGlobalMethod (string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if ((attributes & MethodAttributes.Static) == 0)
				throw new ArgumentException ("global methods must be static");
			if (global_type_created != null)
				throw new InvalidOperationException ("global methods already created");
			CreateGlobalType ();
			MethodBuilder mb = global_type.DefineMethod (name, attributes, callingConvention, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);

			addGlobalMethod (mb);
			return mb;
		}

		public MethodBuilder DefinePInvokeMethod (string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet) {
			return DefinePInvokeMethod (name, dllName, name, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
		}

		public MethodBuilder DefinePInvokeMethod (string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet) {
			if (name == null)
				throw new ArgumentNullException ("name");
			if ((attributes & MethodAttributes.Static) == 0)
				throw new ArgumentException ("global methods must be static");
			if (global_type_created != null)
				throw new InvalidOperationException ("global methods already created");
			CreateGlobalType ();
			MethodBuilder mb = global_type.DefinePInvokeMethod (name, dllName, entryName, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);

			addGlobalMethod (mb);
			return mb;
		}			

		public TypeBuilder DefineType (string name) {
			return DefineType (name, 0);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr) {
			if ((attr & TypeAttributes.Interface) != 0)
				return DefineType (name, attr, null, null);
			return DefineType (name, attr, typeof(object), null);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent) {
			return DefineType (name, attr, parent, null);
		}

		private void AddType (TypeBuilder tb)
		{
			if (types != null) {
				if (types.Length == num_types) {
					TypeBuilder[] new_types = new TypeBuilder [types.Length * 2];
					System.Array.Copy (types, new_types, num_types);
					types = new_types;
				}
			} else {
				types = new TypeBuilder [1];
			}
			types [num_types] = tb;
			num_types ++;
		}

		private TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, Type[] interfaces, PackingSize packingSize, int typesize) {
			if (name == null)
				throw new ArgumentNullException ("fullname");
			TypeIdentifier ident = TypeIdentifiers.FromInternal (name);
			if (name_cache.ContainsKey (ident))
				throw new ArgumentException ("Duplicate type name within an assembly.");
			TypeBuilder res = new TypeBuilder (this, name, attr, parent, interfaces, packingSize, typesize, null);
			AddType (res);

			name_cache.Add (ident, res);
			
			return res;
		}

		internal void RegisterTypeName (TypeBuilder tb, TypeName name)
		{
			name_cache.Add (name, tb);
		}
		
		internal TypeBuilder GetRegisteredType (TypeName name)
		{
			TypeBuilder result = null;
			name_cache.TryGetValue (name, out result);
			return result;
		}

		[ComVisible (true)]
		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, Type[] interfaces) {
			return DefineType (name, attr, parent, interfaces, PackingSize.Unspecified, TypeBuilder.UnspecifiedTypeSize);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, int typesize) {
			return DefineType (name, attr, parent, null, PackingSize.Unspecified, typesize);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, PackingSize packsize) {
			return DefineType (name, attr, parent, null, packsize, TypeBuilder.UnspecifiedTypeSize);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, PackingSize packingSize, int typesize) {
			return DefineType (name, attr, parent, null, packingSize, typesize);
		}

		public MethodInfo GetArrayMethod( Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes) {
			return new MonoArrayMethod (arrayClass, methodName, callingConvention, returnType, parameterTypes);
		}

		public EnumBuilder DefineEnum( string name, TypeAttributes visibility, Type underlyingType) {
			TypeIdentifier ident = TypeIdentifiers.FromInternal (name);
			if (name_cache.ContainsKey (ident))
				throw new ArgumentException ("Duplicate type name within an assembly.");

			EnumBuilder eb = new EnumBuilder (this, name, visibility, underlyingType);
			TypeBuilder res = eb.GetTypeBuilder ();
			AddType (res);
			name_cache.Add (ident, res);
			return eb;
		}

		[ComVisible (true)]
		public override Type GetType( string className) {
			return GetType (className, false, false);
		}
		
		[ComVisible (true)]
		public override Type GetType( string className, bool ignoreCase) {
			return GetType (className, false, ignoreCase);
		}

		private TypeBuilder search_in_array (TypeBuilder[] arr, int validElementsInArray, TypeName className) {
			int i;
			for (i = 0; i < validElementsInArray; ++i) {
				if (String.Compare (className.DisplayName, arr [i].FullName, true, CultureInfo.InvariantCulture) == 0) {
					return arr [i];
				}
			}
			return null;
		}

		private TypeBuilder search_nested_in_array (TypeBuilder[] arr, int validElementsInArray, TypeName className) {
			int i;
			for (i = 0; i < validElementsInArray; ++i) {
				if (String.Compare (className.DisplayName, arr [i].Name, true, CultureInfo.InvariantCulture) == 0)
					return arr [i];
			}
			return null;
		}

		private TypeBuilder GetMaybeNested (TypeBuilder t, IEnumerable<TypeName> nested) {
			TypeBuilder result = t;

			foreach (TypeName pname in nested) {
				if (result.subtypes == null)
					return null;
				result = search_nested_in_array(result.subtypes, result.subtypes.Length, pname);
				if (result == null)
					return null;
			}
			return result;
		}

		[ComVisible (true)]
		public override Type GetType (string className, bool throwOnError, bool ignoreCase)
		{
			if (className == null)
				throw new ArgumentNullException ("className");
			if (className.Length == 0)
				throw new ArgumentException ("className");

			TypeBuilder result = null;

			if (types == null && throwOnError)
				throw new TypeLoadException (className);

			TypeSpec ts = TypeSpec.Parse (className);

			if (!ignoreCase) {
				var displayNestedName = ts.TypeNameWithoutModifiers();
				name_cache.TryGetValue (displayNestedName, out result);
			} else {
				if (types != null)
					result = search_in_array (types, num_types,  ts.Name);
				if (!ts.IsNested && result != null) {
					result = GetMaybeNested (result, ts.Nested);
				}
			}
			if ((result == null) && throwOnError)
				throw new TypeLoadException (className);
			if (result != null && (ts.HasModifiers || ts.IsByRef)) {
				Type mt = result;
				if (result is TypeBuilder) {
					var tb = result as TypeBuilder;
					if (tb.is_created)
						mt = tb.CreateType ();
				}
				foreach (var mod in ts.Modifiers) {
					if (mod is PointerSpec)
						mt = mt.MakePointerType ();
					else if (mod is ArraySpec) {
						var spec = mod as ArraySpec;
						if (spec.IsBound)
							return null;
						if (spec.Rank == 1)
							mt = mt.MakeArrayType ();
						else
							mt = mt.MakeArrayType (spec.Rank);
					}
				}
				if (ts.IsByRef)
					mt = mt.MakeByRefType ();
				result = mt as TypeBuilder;
				if (result == null)
					return mt;
			}
			if (result != null && result.is_created)
				return result.CreateType ();
			else
				return result;
		}

		internal int get_next_table_index (object obj, int table, int count) {
			if (table_indexes == null) {
				table_indexes = new int [64];
				for (int i=0; i < 64; ++i)
					table_indexes [i] = 1;
				/* allow room for .<Module> in TypeDef table */
				table_indexes [0x02] = 2;
			}
			// Console.WriteLine ("getindex for table "+table.ToString()+" got "+table_indexes [table].ToString());
			var index = table_indexes [table];
			table_indexes [table] += count;
			return index;
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

		[ComVisible (true)]
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}

		public ISymbolWriter GetSymWriter () {
			return symbolWriter;
		}

		public ISymbolDocumentWriter DefineDocument (string url, Guid language, Guid languageVendor, Guid documentType)
		{
			if (symbolWriter != null)
				return symbolWriter.DefineDocument (url, language, languageVendor, documentType);
			else
				return null;
		}

		public override Type [] GetTypes ()
		{
			if (types == null)
				return Type.EmptyTypes;

			int n = num_types;
			Type [] copy = new Type [n];
			Array.Copy (types, copy, n);

			// MS replaces the typebuilders with their created types
			for (int i = 0; i < copy.Length; ++i)
				if (types [i].is_created)
					copy [i] = types [i].CreateType ();

			return copy;
		}

		public IResourceWriter DefineResource (string name, string description, ResourceAttributes attribute)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name == String.Empty)
				throw new ArgumentException ("name cannot be empty");
			if (transient)
				throw new InvalidOperationException ("The module is transient");
			if (!assemblyb.IsSave)
				throw new InvalidOperationException ("The assembly is transient");
			ResourceWriter writer = new ResourceWriter (new MemoryStream ());
			if (resource_writers == null)
				resource_writers = new Hashtable ();
			resource_writers [name] = writer;

			// The data is filled out later
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

			return writer;
		}

		public IResourceWriter DefineResource (string name, string description)
		{
			return DefineResource (name, description, ResourceAttributes.Public);
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
				throw new FileNotFoundException ("File '" + resourceFileName + "' does not exist or is a directory.");

			throw new NotImplementedException ();
		}

		public void DefineManifestResource (string name, Stream stream, ResourceAttributes attribute) {
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name == String.Empty)
				throw new ArgumentException ("name cannot be empty");
			if (stream == null)
				throw new ArgumentNullException ("stream");
			if (transient)
				throw new InvalidOperationException ("The module is transient");
			if (!assemblyb.IsSave)
				throw new InvalidOperationException ("The assembly is transient");

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
			resources [p].stream = stream;
		}

		[MonoTODO]
		public void SetSymCustomAttribute (string name, byte[] data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetUserEntryPoint (MethodInfo entryPoint)
		{
			if (entryPoint == null)
				throw new ArgumentNullException ("entryPoint");
			if (entryPoint.DeclaringType.Module != this)
				throw new InvalidOperationException ("entryPoint is not contained in this module");
			throw new NotImplementedException ();
		}

		public MethodToken GetMethodToken (MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException ("method");

			return new MethodToken (GetToken (method));
		}
		
		public MethodToken GetMethodToken (MethodInfo method, IEnumerable<Type> optionalParameterTypes)
		{
			if (method == null)
				throw new ArgumentNullException ("method");

			return new MethodToken (GetToken (method, optionalParameterTypes));
		}

		public MethodToken GetArrayMethodToken (Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return GetMethodToken (GetArrayMethod (arrayClass, methodName, callingConvention, returnType, parameterTypes));
		}

		[ComVisible (true)]
		public MethodToken GetConstructorToken (ConstructorInfo con)
		{
			if (con == null)
				throw new ArgumentNullException ("con");

			return new MethodToken (GetToken (con));
		}
		
		public MethodToken GetConstructorToken (ConstructorInfo constructor, IEnumerable<Type> optionalParameterTypes)
		{
			if (constructor == null)
				throw new ArgumentNullException ("constructor");

			return new MethodToken (GetToken (constructor, optionalParameterTypes));
		}

		public FieldToken GetFieldToken (FieldInfo field)
		{
			if (field == null)
				throw new ArgumentNullException ("field");

			return new FieldToken (GetToken (field));
		}

		[MonoTODO]
		public SignatureToken GetSignatureToken (byte[] sigBytes, int sigLength)
		{
			throw new NotImplementedException ();
		}

		public SignatureToken GetSignatureToken (SignatureHelper sigHelper)
		{
			if (sigHelper == null)
				throw new ArgumentNullException ("sigHelper");
			return new SignatureToken (GetToken (sigHelper));
		}

		public StringToken GetStringConstant (string str)
		{
			if (str == null)
				throw new ArgumentNullException ("str");
			return new StringToken (GetToken (str));
		}

		public TypeToken GetTypeToken (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (type.IsByRef)
				throw new ArgumentException ("type can't be a byref type", "type");
			if (!IsTransient () && (type.Module is ModuleBuilder) && ((ModuleBuilder)type.Module).IsTransient ())
				throw new InvalidOperationException ("a non-transient module can't reference a transient module");
			return new TypeToken (GetToken (type));
		}

		public TypeToken GetTypeToken (string name)
		{
			return GetTypeToken (GetType (name));
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int getUSIndex (ModuleBuilder mb, string str);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int getToken (ModuleBuilder mb, object obj, bool create_open_instance);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int getMethodToken (ModuleBuilder mb, MethodBase method,
							  Type[] opt_param_types);

		internal int GetToken (string str)
		{
			int result;
			if (!us_string_cache.TryGetValue (str, out result)) {
				result = getUSIndex (this, str);
				us_string_cache [str] = result;
			}
			
			return result;
		}

		static int typeref_tokengen =  0x01ffffff;
		static int typedef_tokengen =  0x02ffffff;
		static int typespec_tokengen =  0x1bffffff;
		static int memberref_tokengen =  0x0affffff;
		static int methoddef_tokengen =  0x06ffffff;
		Dictionary<MemberInfo, int> inst_tokens, inst_tokens_open;

		//
		// Assign a pseudo token to the various TypeBuilderInst objects, so the runtime
		// doesn't have to deal with them.
		// For Save assemblies, the tokens will be fixed up later during Save ().
		// For Run assemblies, the tokens will not be fixed up, so the runtime will
		// still encounter these objects, it will resolve them by calling their
		// RuntimeResolve () methods.
		//
		int GetPseudoToken (MemberInfo member, bool create_open_instance)
		{
			int token;
			var dict = create_open_instance ? inst_tokens_open : inst_tokens;
			if (dict == null) {
				dict = new Dictionary<MemberInfo, int> (ReferenceEqualityComparer<MemberInfo>.Instance);
				if (create_open_instance)
					inst_tokens_open = dict;
				else
					inst_tokens = dict;
			} else if (dict.TryGetValue (member, out token)) {
				return token;
			}

			// Count backwards to avoid collisions with the tokens
			// allocated by the runtime
			if (member is TypeBuilderInstantiation || member is SymbolType)
				token = typespec_tokengen --;
			else if (member is FieldOnTypeBuilderInst)
				token = memberref_tokengen --;
			else if (member is ConstructorOnTypeBuilderInst)
				token = memberref_tokengen --;
			else if (member is MethodOnTypeBuilderInst)
				token = memberref_tokengen --;
			else if (member is FieldBuilder)
				token = memberref_tokengen --;
			else if (member is TypeBuilder) {
				if (create_open_instance && (member as TypeBuilder).ContainsGenericParameters)
					token = typespec_tokengen --;
				else if (member.Module == this)
					token = typedef_tokengen --;
				else
					token = typeref_tokengen --;
			} else if (member is EnumBuilder) {
				token = GetPseudoToken ((member as  EnumBuilder).GetTypeBuilder(), create_open_instance);
				dict[member] = token;
				// n.b. don't register with the runtime, the TypeBuilder already did it.
				return token;
			} else if (member is ConstructorBuilder) {
				if (member.Module == this && !(member as ConstructorBuilder).TypeBuilder.ContainsGenericParameters)
					token = methoddef_tokengen --;
				else
					token = memberref_tokengen --;
			} else if (member is MethodBuilder) {
				var mb = member as MethodBuilder;
				if (member.Module == this && !mb.TypeBuilder.ContainsGenericParameters && !mb.IsGenericMethodDefinition)
					token = methoddef_tokengen --;
				else
					token = memberref_tokengen --;
			} else if (member is GenericTypeParameterBuilder) {
				token = typespec_tokengen --;
			} else
				throw new NotImplementedException ();

			dict [member] = token;
			RegisterToken (member, token);
			return token;
		}

		internal int GetToken (MemberInfo member) {
			if (member is ConstructorBuilder || member is MethodBuilder || member is FieldBuilder)
				return GetPseudoToken (member, false);
			return getToken (this, member, true);
		}

		internal int GetToken (MemberInfo member, bool create_open_instance) {
			if (member is TypeBuilderInstantiation || member is FieldOnTypeBuilderInst || member is ConstructorOnTypeBuilderInst || member is MethodOnTypeBuilderInst || member is SymbolType || member is FieldBuilder || member is TypeBuilder || member is ConstructorBuilder || member is MethodBuilder || member is GenericTypeParameterBuilder ||
			    member is EnumBuilder)
				return GetPseudoToken (member, create_open_instance);
			return getToken (this, member, create_open_instance);
		}

		internal int GetToken (MethodBase method, IEnumerable<Type> opt_param_types) {
			if (method is ConstructorBuilder || method is MethodBuilder)
				return GetPseudoToken (method, false);

			if (opt_param_types == null)
				return getToken (this, method, true);

			var optParamTypes = new List<Type> (opt_param_types);
			return  getMethodToken (this, method, optParamTypes.ToArray ());
		}
		
		internal int GetToken (MethodBase method, Type[] opt_param_types) {
			if (method is ConstructorBuilder || method is MethodBuilder)
				return GetPseudoToken (method, false);
			return getMethodToken (this, method, opt_param_types);
		}

		internal int GetToken (SignatureHelper helper) {
			return getToken (this, helper, true);
		}

		/*
		 * Register the token->obj mapping with the runtime so the Module.Resolve... 
		 * methods will work for obj.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern void RegisterToken (object obj, int token);

		/*
		 * Returns MemberInfo registered with the given token.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern object GetRegisteredToken (int token);

		internal TokenGenerator GetTokenGenerator () {
			if (token_gen == null)
				token_gen = new ModuleBuilderTokenGenerator (this);
			return token_gen;
		}

		// Called from the runtime to return the corresponding finished reflection object
		internal static object RuntimeResolve (object obj) {
			if (obj is MethodBuilder)
				return (obj as MethodBuilder).RuntimeResolve ();
			if (obj is ConstructorBuilder)
				return (obj as ConstructorBuilder).RuntimeResolve ();
			if (obj is FieldBuilder)
				return (obj as FieldBuilder).RuntimeResolve ();
			if (obj is GenericTypeParameterBuilder)
				return (obj as GenericTypeParameterBuilder).RuntimeResolve ();
			if (obj is FieldOnTypeBuilderInst)
				return (obj as FieldOnTypeBuilderInst).RuntimeResolve ();
			if (obj is MethodOnTypeBuilderInst)
				return (obj as MethodOnTypeBuilderInst).RuntimeResolve ();
			if (obj is ConstructorOnTypeBuilderInst)
				return (obj as ConstructorOnTypeBuilderInst).RuntimeResolve ();
			if (obj is Type)
				return (obj as Type).RuntimeResolve ();
			throw new NotImplementedException (obj.GetType ().FullName);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void build_metadata (ModuleBuilder mb);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void WriteToFile (IntPtr handle);

		void FixupTokens (Dictionary<int, int> token_map, Dictionary<int, MemberInfo> member_map, Dictionary<MemberInfo, int> inst_tokens,
						  bool open) {
			foreach (var v in inst_tokens) {
				var member = v.Key;
				var old_token = v.Value;
				MemberInfo finished = null;

				// Construct the concrete reflection object corresponding to the
				// TypeBuilderInst object, and request a token for it instead.
				if (member is TypeBuilderInstantiation || member is SymbolType) {
					finished = (member as Type).RuntimeResolve ();
				} else if (member is FieldOnTypeBuilderInst) {
					finished = (member as FieldOnTypeBuilderInst).RuntimeResolve ();
				} else if (member is ConstructorOnTypeBuilderInst) {
					finished = (member as ConstructorOnTypeBuilderInst).RuntimeResolve ();
				} else if (member is MethodOnTypeBuilderInst) {
					finished = (member as MethodOnTypeBuilderInst).RuntimeResolve ();
				} else if (member is FieldBuilder) {
					finished = (member as FieldBuilder).RuntimeResolve ();
				} else if (member is TypeBuilder) {
					finished = (member as TypeBuilder).RuntimeResolve ();
				} else if (member is EnumBuilder) {
					finished = (member as EnumBuilder).RuntimeResolve ();
				} else if (member is ConstructorBuilder) {
					finished = (member as ConstructorBuilder).RuntimeResolve ();
				} else if (member is MethodBuilder) {
					finished = (member as MethodBuilder).RuntimeResolve ();
				} else if (member is GenericTypeParameterBuilder) {
					finished = (member as GenericTypeParameterBuilder).RuntimeResolve ();
				} else {
					throw new NotImplementedException ();
				}

				int new_token = GetToken (finished, open);
				token_map [old_token] = new_token;
				member_map [old_token] = finished;
				// Replace the token mapping in the runtime so it points to the new object
				RegisterToken (finished, old_token);
			}
		}

		//
		// Fixup the pseudo tokens assigned to the various SRE objects
		//
		void FixupTokens ()
		{
			var token_map = new Dictionary<int, int> ();
			var member_map = new Dictionary<int, MemberInfo> ();
			if (inst_tokens != null)
				FixupTokens (token_map, member_map, inst_tokens, false);
			if (inst_tokens_open != null)
				FixupTokens (token_map, member_map, inst_tokens_open, true);

			// Replace the tokens in the IL stream
			if (types != null) {
				for (int i = 0; i < num_types; ++i)
					types [i].FixupTokens (token_map, member_map);
			}
		}

		internal void Save ()
		{
			if (transient && !is_main)
				return;

			if (types != null) {
				for (int i = 0; i < num_types; ++i)
					if (!types [i].is_created)
						throw new NotSupportedException ("Type '" + types [i].FullName + "' was not completed.");
			}

			FixupTokens ();

			if ((global_type != null) && (global_type_created == null))
				global_type_created = global_type.CreateType ();

			if (resources != null) {
				for (int i = 0; i < resources.Length; ++i) {
					IResourceWriter rwriter;
					if (resource_writers != null && (rwriter = resource_writers [resources [i].name] as IResourceWriter) != null) {
						ResourceWriter writer = (ResourceWriter)rwriter;
						writer.Generate ();
						MemoryStream mstream = (MemoryStream)writer._output;
						resources [i].data = new byte [mstream.Length];
						mstream.Seek (0, SeekOrigin.Begin);
						mstream.Read (resources [i].data, 0, (int)mstream.Length);
						continue;
					}
					Stream stream = resources [i].stream;

					// According to MSDN docs, the stream is read during assembly save, not earlier
					if (stream != null) {
						try {
							long len = stream.Length;
							resources [i].data = new byte [len];
							stream.Seek (0, SeekOrigin.Begin);
							stream.Read (resources [i].data, 0, (int)len);
						} catch {
							/* do something */
						}
					}
				}
			}

			build_metadata (this);

			string fileName = fqname;
			if (assemblyb.AssemblyDir != null)
				fileName = Path.Combine (assemblyb.AssemblyDir, fileName);

			try {
				// We mmap the file, so unlink the previous version since it may be in use
				File.Delete (fileName);
			} catch {
				// We can safely ignore
			}
			using (FileStream file = new FileStream (fileName, FileMode.Create, FileAccess.Write))
				WriteToFile (file.Handle);
			
			//
			// The constant 0x80000000 is internal to Mono, it means `make executable'
			//
			File.SetAttributes (fileName, (FileAttributes) (unchecked ((int) 0x80000000)));
			
			if (types != null && symbolWriter != null) {
				for (int i = 0; i < num_types; ++i)
					types [i].GenerateDebugInfo (symbolWriter);
				symbolWriter.Close ();
			}
		}

		internal string FileName {
			get {
				return fqname;
			}
		}

		internal bool IsMain {
			set {
				is_main = value;
			}
		}

		internal void CreateGlobalType () {
			if (global_type == null)
				global_type = new TypeBuilder (this, 0, 1);
		}

		internal override Guid GetModuleVersionId ()
		{
			return new Guid (guid);
		}

		public override	Assembly Assembly {
			get { return assemblyb; }
		}

		public override string Name {
			get { return name; }
		}

		public override string ScopeName {
			get { return name; }
		}

		public override Guid ModuleVersionId {
			get {
				return GetModuleVersionId ();
			}
		}

		//XXX resource modules can't be defined with ModuleBuilder
		public override bool IsResource ()
		{
			return false;
		}

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) 
		{
			if (global_type_created == null)
				return null;
			if (types == null)
				return global_type_created.GetMethod (name);
			return global_type_created.GetMethod (name, bindingAttr, binder, callConvention, types, modifiers);
		}

		public override FieldInfo ResolveField (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments) {
			return RuntimeModule.ResolveField (this, _impl, metadataToken, genericTypeArguments, genericMethodArguments);
		}

		public override MemberInfo ResolveMember (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments) {
			return RuntimeModule.ResolveMember (this, _impl, metadataToken, genericTypeArguments, genericMethodArguments);
		}

		internal MemberInfo ResolveOrGetRegisteredToken (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments)
		{
			ResolveTokenError error;
			MemberInfo m = RuntimeModule.ResolveMemberToken (_impl, metadataToken, RuntimeModule.ptrs_from_types (genericTypeArguments), RuntimeModule.ptrs_from_types (genericMethodArguments), out error);
			if (m != null)
				return m;

			m = GetRegisteredToken (metadataToken) as MemberInfo;
			if (m == null)
				throw RuntimeModule.resolve_token_exception (Name, metadataToken, error, "MemberInfo");
			else
				return m;
		}

		public override MethodBase ResolveMethod (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments) {
			return RuntimeModule.ResolveMethod (this, _impl, metadataToken, genericTypeArguments, genericMethodArguments);
		}

		public override string ResolveString (int metadataToken) {
			return RuntimeModule.ResolveString (this, _impl, metadataToken);
		}

		public override byte[] ResolveSignature (int metadataToken) {
			return RuntimeModule.ResolveSignature (this, _impl, metadataToken);
		}

		public override Type ResolveType (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments) {
			return RuntimeModule.ResolveType (this, _impl, metadataToken, genericTypeArguments, genericMethodArguments);
		}

		public override bool Equals (object obj)
		{
			return base.Equals (obj);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return base.IsDefined (attributeType, inherit);
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			return GetCustomAttributes (null, inherit);
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			if (cattrs == null || cattrs.Length == 0)
				return Array.Empty<object> ();

			if (attributeType is TypeBuilder)
				throw new InvalidOperationException ("First argument to GetCustomAttributes can't be a TypeBuilder");

			List<object> results = new List<object> ();
			for (int i=0; i < cattrs.Length; i++) {
				Type t = cattrs [i].Ctor.GetType ();

				if (t is TypeBuilder)
					throw new InvalidOperationException ("Can't construct custom attribute for TypeBuilder type");

				if (attributeType == null || attributeType.IsAssignableFrom (t))
					results.Add (cattrs [i].Invoke ());
			}

			return results.ToArray ();
		}

		public override FieldInfo GetField (string name, BindingFlags bindingAttr)
		{
			if (global_type_created == null)
				throw new InvalidOperationException ("Module-level fields cannot be retrieved until after the CreateGlobalFunctions method has been called for the module.");
			return global_type_created.GetField (name, bindingAttr);
		}

		public override FieldInfo[] GetFields (BindingFlags bindingFlags)
		{
			if (global_type_created == null)
				throw new InvalidOperationException ("Module-level fields cannot be retrieved until after the CreateGlobalFunctions method has been called for the module.");
			return global_type_created.GetFields (bindingFlags);
		}

		public override MethodInfo[] GetMethods (BindingFlags bindingFlags)
		{
			if (global_type_created == null)
				throw new InvalidOperationException ("Module-level methods cannot be retrieved until after the CreateGlobalFunctions method has been called for the module.");
			return global_type_created.GetMethods (bindingFlags);
		}

		public override int MetadataToken {
			get {
				return RuntimeModule.get_MetadataToken (this);
			}
		}
	}

	internal class ModuleBuilderTokenGenerator : TokenGenerator {

		private ModuleBuilder mb;

		public ModuleBuilderTokenGenerator (ModuleBuilder mb) {
			this.mb = mb;
		}

		public int GetToken (string str) {
			return mb.GetToken (str);
		}

		public int GetToken (MemberInfo member, bool create_open_instance) {
			return mb.GetToken (member, create_open_instance);
		}

		public int GetToken (MethodBase method, Type[] opt_param_types) {
			return mb.GetToken (method, opt_param_types);
		}

		public int GetToken (SignatureHelper helper) {
			return mb.GetToken (helper);
		}
	}
}

#endif
