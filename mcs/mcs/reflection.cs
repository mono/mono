//
// reflection.cs: System.Reflection and System.Reflection.Emit specific implementations
//
// Author: Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2009-2010 Novell, Inc. 
//
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Security;

namespace Mono.CSharp
{
#if STATIC
	public class ReflectionImporter
	{
		public ReflectionImporter (BuildinTypes buildin)
		{
			throw new NotSupportedException ();
		}

		public void ImportAssembly (Assembly assembly, RootNamespace targetNamespace)
		{
			throw new NotSupportedException ();
		}

		public ImportedModuleDefinition ImportModule (Module module, RootNamespace targetNamespace)
		{
			throw new NotSupportedException ();
		}

		public TypeSpec ImportType (Type type)
		{
			throw new NotSupportedException ();
		}
	}
#else
	public sealed class ReflectionImporter : MetadataImporter
	{
		public ReflectionImporter (BuildinTypes buildin)
		{
			Initialize (buildin);
		}

		protected override MemberKind DetermineKindFromBaseType (Type baseType)
		{
			if (baseType == typeof (ValueType))
				return MemberKind.Struct;

			if (baseType == typeof (System.Enum))
				return MemberKind.Enum;

			if (baseType == typeof (MulticastDelegate))
				return MemberKind.Delegate;

			return MemberKind.Class;
		}

		public override void GetCustomAttributeTypeName (CustomAttributeData cad, out string typeNamespace, out string typeName)
		{
			var dt = cad.Constructor.DeclaringType;
			typeNamespace = dt.Namespace;
			typeName = dt.Name;
		}

		protected override bool HasVolatileModifier (Type[] modifiers)
		{
			foreach (var t in modifiers) {
				if (t == typeof (IsVolatile))
					return true;
			}

			return false;
		}

		public void ImportAssembly (Assembly assembly, RootNamespace targetNamespace)
		{
			// It can be used more than once when importing same assembly
			// into 2 or more global aliases
			var definition = GetAssemblyDefinition (assembly);

			//
			// This part tries to simulate loading of top-level
			// types only, any missing dependencies are ignores here.
			// Full error report is reported later when the type is
			// actually used
			//
			Type[] all_types;
			try {
				all_types = assembly.GetTypes ();
			} catch (ReflectionTypeLoadException e) {
				all_types = e.Types;
			}

			ImportTypes (all_types, targetNamespace, definition.HasExtensionMethod);
		}

		public ImportedModuleDefinition ImportModule (Module module, RootNamespace targetNamespace)
		{
			var module_definition = new ImportedModuleDefinition (module, this);
			module_definition.ReadAttributes ();

			Type[] all_types;
			try {
				all_types = module.GetTypes ();
			} catch (ReflectionTypeLoadException e) {
				all_types = e.Types;
			}

			ImportTypes (all_types, targetNamespace, false);

			return module_definition;
		}

		void Initialize (BuildinTypes buildin)
		{
			//
			// Setup mapping for build-in types to avoid duplication of their definition
			//
			buildin_types.Add (typeof (object), buildin.Object);
			buildin_types.Add (typeof (System.ValueType), buildin.ValueType);
			buildin_types.Add (typeof (System.Attribute), buildin.Attribute);

			buildin_types.Add (typeof (int), buildin.Int);
			buildin_types.Add (typeof (long), buildin.Long);
			buildin_types.Add (typeof (uint), buildin.UInt);
			buildin_types.Add (typeof (ulong), buildin.ULong);
			buildin_types.Add (typeof (byte), buildin.Byte);
			buildin_types.Add (typeof (sbyte), buildin.SByte);
			buildin_types.Add (typeof (short), buildin.Short);
			buildin_types.Add (typeof (ushort), buildin.UShort);

			buildin_types.Add (typeof (System.Collections.IEnumerator), buildin.IEnumerator);
			buildin_types.Add (typeof (System.Collections.IEnumerable), buildin.IEnumerable);
			buildin_types.Add (typeof (System.IDisposable), buildin.IDisposable);

			buildin_types.Add (typeof (char), buildin.Char);
			buildin_types.Add (typeof (string), buildin.String);
			buildin_types.Add (typeof (float), buildin.Float);
			buildin_types.Add (typeof (double), buildin.Double);
			buildin_types.Add (typeof (decimal), buildin.Decimal);
			buildin_types.Add (typeof (bool), buildin.Bool);
			buildin_types.Add (typeof (System.IntPtr), buildin.IntPtr);
			buildin_types.Add (typeof (System.UIntPtr), buildin.UIntPtr);

			buildin_types.Add (typeof (System.MulticastDelegate), buildin.MulticastDelegate);
			buildin_types.Add (typeof (System.Delegate), buildin.Delegate);
			buildin_types.Add (typeof (System.Enum), buildin.Enum);
			buildin_types.Add (typeof (System.Array), buildin.Array);
			buildin_types.Add (typeof (void), buildin.Void);
			buildin_types.Add (typeof (System.Type), buildin.Type);
			buildin_types.Add (typeof (System.Exception), buildin.Exception);
			buildin_types.Add (typeof (System.RuntimeFieldHandle), buildin.RuntimeFieldHandle);
			buildin_types.Add (typeof (System.RuntimeTypeHandle), buildin.RuntimeTypeHandle);
		}
	}
#endif

	public class AssemblyDefinitionDynamic : AssemblyDefinition
	{
		//
		// In-memory only assembly container
		//
		public AssemblyDefinitionDynamic (ModuleContainer module, string name)
			: base (module, name)
		{
		}

		//
		// Assembly container with file output
		//
		public AssemblyDefinitionDynamic (ModuleContainer module, string name, string fileName)
			: base (module, name, fileName)
		{
		}

		public Module IncludeModule (string moduleFile)
		{
			return builder_extra.AddModule (moduleFile);
		}

#if !STATIC
		public override ModuleBuilder CreateModuleBuilder ()
		{
			if (file_name == null)
				return Builder.DefineDynamicModule (Name, false);

			return base.CreateModuleBuilder ();
		}
#endif
		//
		// Initializes the code generator
		//
		public bool Create (AppDomain domain, AssemblyBuilderAccess access)
		{
#if STATIC
			throw new NotSupportedException ();
#else
			ResolveAssemblySecurityAttributes ();
			var an = CreateAssemblyName ();

			try {
				Builder = file_name == null ?
					domain.DefineDynamicAssembly (an, access) :
					domain.DefineDynamicAssembly (an, access, Dirname (file_name));
			} catch (ArgumentException) {
				// specified key may not be exportable outside it's container
				if (RootContext.StrongNameKeyContainer != null) {
					Report.Error (1548, "Could not access the key inside the container `" +
						RootContext.StrongNameKeyContainer + "'.");
				}
				throw;
			}

			builder_extra = new AssemblyBuilderMonoSpecific (Builder, Compiler);
			return true;
#endif
		}

		static string Dirname (string name)
		{
			int pos = name.LastIndexOf ('/');

			if (pos != -1)
				return name.Substring (0, pos);

			pos = name.LastIndexOf ('\\');
			if (pos != -1)
				return name.Substring (0, pos);

			return ".";
		}

#if !STATIC
		protected override void SaveModule (PortableExecutableKinds pekind, ImageFileMachine machine)
		{
			try {
				var module_only = typeof (AssemblyBuilder).GetProperty ("IsModuleOnly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				var set_module_only = module_only.GetSetMethod (true);

				set_module_only.Invoke (Builder, new object[] { true });
			} catch {
				base.SaveModule (pekind, machine);
			}

			Builder.Save (file_name, pekind, machine);
		}
#endif
	}

	//
	// Extension to System.Reflection.Emit.AssemblyBuilder to have fully compatible
	// compiler
	//
	class AssemblyBuilderMonoSpecific : AssemblyBuilderExtension
	{
		static MethodInfo adder_method;
		static MethodInfo add_permission;
		static MethodInfo add_type_forwarder;
		static MethodInfo win32_icon_define;
		static FieldInfo assembly_version;
		static FieldInfo assembly_algorithm;
		static FieldInfo assembly_culture;
		static FieldInfo assembly_flags;

		AssemblyBuilder builder;

		public AssemblyBuilderMonoSpecific (AssemblyBuilder ab, CompilerContext ctx)
			: base (ctx)
		{
			this.builder = ab;
		}

		public override Module AddModule (string module)
		{
			try {
				if (adder_method == null)
					adder_method = typeof (AssemblyBuilder).GetMethod ("AddModule", BindingFlags.Instance | BindingFlags.NonPublic);

				return (Module) adder_method.Invoke (builder, new object[] { module });
			} catch {
				return base.AddModule (module);
			}
		}

		public override void AddPermissionRequests (PermissionSet[] permissions)
		{
			try {
				if (add_permission == null)
					add_permission = typeof (AssemblyBuilder).GetMethod ("AddPermissionRequests", BindingFlags.Instance | BindingFlags.NonPublic);

				add_permission.Invoke (builder, permissions);
			} catch {
				base.AddPermissionRequests (permissions);
			}
		}

		public override void AddTypeForwarder (TypeSpec type, Location loc)
		{
			try {
				if (add_type_forwarder == null) {
					add_type_forwarder = typeof (AssemblyBuilder).GetMethod ("AddTypeForwarder", BindingFlags.NonPublic | BindingFlags.Instance);
				}

				add_type_forwarder.Invoke (builder, new object[] { type.GetMetaInfo () });
			} catch {
				base.AddTypeForwarder (type, loc);
			}
		}

		public override void DefineWin32IconResource (string fileName)
		{
			try {
				if (win32_icon_define == null)
					win32_icon_define = typeof (AssemblyBuilder).GetMethod ("DefineIconResource", BindingFlags.Instance | BindingFlags.NonPublic);

				win32_icon_define.Invoke (builder, new object[] { fileName });
			} catch {
				base.DefineWin32IconResource (fileName);
			}
		}

		public override void SetAlgorithmId (uint value, Location loc)
		{
			try {
				if (assembly_algorithm == null)
					assembly_algorithm = typeof (AssemblyBuilder).GetField ("algid", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);

				assembly_algorithm.SetValue (builder, value);
			} catch {
				base.SetAlgorithmId (value, loc);
			}
		}

		public override void SetCulture (string culture, Location loc)
		{
			try {
				if (assembly_culture == null)
					assembly_culture = typeof (AssemblyBuilder).GetField ("culture", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);

				assembly_culture.SetValue (builder, culture);
			} catch {
				base.SetCulture (culture, loc);
			}
		}

		public override void SetFlags (uint flags, Location loc)
		{
			try {
				if (assembly_flags == null)
					assembly_flags = typeof (AssemblyBuilder).GetField ("flags", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);

				assembly_flags.SetValue (builder, flags);
			} catch {
				base.SetFlags (flags, loc);
			}
		}

		public override void SetVersion (Version version, Location loc)
		{
			try {
				if (assembly_version == null)
					assembly_version = typeof (AssemblyBuilder).GetField ("version", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);

				assembly_version.SetValue (builder, version.ToString (4));
			} catch {
				base.SetVersion (version, loc);
			}
		}
	}

	//
	// Reflection based references loader
	//
	class DynamicLoader : AssemblyReferencesLoader<Assembly>
	{
		readonly ReflectionImporter importer;

		public DynamicLoader (ReflectionImporter importer, CompilerContext compiler)
			: base (compiler)
		{
			this.importer = importer;
		}

		public ReflectionImporter Importer {
			get {
				return importer;
			}
		}

		public override bool HasObjectType (Assembly assembly)
		{
			return assembly.GetType (compiler.BuildinTypes.Object.FullName) != null;
		}

		protected override string[] GetDefaultReferences ()
		{
			//
			// For now the "default config" is harcoded into the compiler
			// we can move this outside later
			//
			var default_references = new List<string> (8);

			default_references.Add ("System");
			default_references.Add ("System.Xml");
#if NET_2_1
			default_references.Add ("System.Net");
			default_references.Add ("System.Windows");
			default_references.Add ("System.Windows.Browser");
#endif

			if (RootContext.Version > LanguageVersion.ISO_2)
				default_references.Add ("System.Core");
			if (RootContext.Version > LanguageVersion.V_3)
				default_references.Add ("Microsoft.CSharp");

			return default_references.ToArray ();
		}

		public override Assembly LoadAssemblyFile (string fileName)
		{
			return LoadAssemblyFile (fileName, false);
		}

		Assembly LoadAssemblyFile (string assembly, bool soft)
		{
			Assembly a = null;
			string total_log = "";

			try {
				try {
					char[] path_chars = { '/', '\\' };

					if (assembly.IndexOfAny (path_chars) != -1) {
						a = Assembly.LoadFrom (assembly);
					} else {
						string ass = assembly;
						if (ass.EndsWith (".dll") || ass.EndsWith (".exe"))
							ass = assembly.Substring (0, assembly.Length - 4);
						a = Assembly.Load (ass);
					}
				} catch (FileNotFoundException) {
					bool err = !soft;
					foreach (string dir in paths) {
						string full_path = Path.Combine (dir, assembly);
						if (!assembly.EndsWith (".dll") && !assembly.EndsWith (".exe"))
							full_path += ".dll";

						try {
							a = Assembly.LoadFrom (full_path);
							err = false;
							break;
						} catch (FileNotFoundException ff) {
							if (soft)
								return a;
							total_log += ff.FusionLog;
						}
					}
					if (err) {
						Error_FileNotFound (assembly);
						return a;
					}
				}
			} catch (BadImageFormatException) {
				Error_FileCorrupted (assembly);
			}

			return a;
		}

		public override Assembly LoadAssemblyDefault (string fileName)
		{
			return LoadAssemblyFile (fileName, true);
		}

		Module LoadModuleFile (AssemblyDefinitionDynamic assembly, string module)
		{
			string total_log = "";

			try {
				try {
					return assembly.IncludeModule (module);
				} catch (FileNotFoundException) {
					bool err = true;
					foreach (string dir in paths) {
						string full_path = Path.Combine (dir, module);
						if (!module.EndsWith (".netmodule"))
							full_path += ".netmodule";

						try {
							return assembly.IncludeModule (full_path);
						} catch (FileNotFoundException ff) {
							total_log += ff.FusionLog;
						}
					}
					if (err) {
						Error_FileNotFound (module);
						return null;
					}
				}
			} catch (BadImageFormatException) {
				Error_FileCorrupted (module);
			}

			return null;
		}

		public void LoadModules (AssemblyDefinitionDynamic assembly, RootNamespace targetNamespace)
		{
			if (RootContext.Modules.Count == 0)
				return;

			foreach (var moduleName in RootContext.Modules) {
				var m = LoadModuleFile (assembly, moduleName);
				if (m == null)
					continue;

				var md = importer.ImportModule (m, targetNamespace);
				assembly.AddModule (md);
			}
		}

		public override void LoadReferences (ModuleContainer module)
		{
			Assembly corlib;
			List<Tuple<RootNamespace, Assembly>> loaded;
			base.LoadReferencesCore (module, out corlib, out loaded);

			if (corlib == null)
				return;

			importer.ImportAssembly (corlib, module.GlobalRootNamespace);
			foreach (var entry in loaded) {
				importer.ImportAssembly (entry.Item2, entry.Item1);
			}
		}
	}
}