//
// ikvm.cs: IKVM.Reflection and IKVM.Reflection.Emit specific implementations
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
using MetaType = IKVM.Reflection.Type;
using IKVM.Reflection;
using IKVM.Reflection.Emit;
using System.IO;
using System.Configuration.Assemblies;

namespace Mono.CSharp
{
#if !STATIC
	public class StaticImporter
	{
		public StaticImporter (BuildinTypes buildin)
		{
			throw new NotSupportedException ();
		}

		public void ImportAssembly (Assembly assembly, RootNamespace targetNamespace)
		{
			throw new NotSupportedException ();
		}

		public void ImportModule (Module module, RootNamespace targetNamespace)
		{
			throw new NotSupportedException ();
		}

		public TypeSpec ImportType (System.Type type)
		{
			throw new NotSupportedException ();
		}
	}

#else

	class StaticImporter : MetadataImporter
	{
		public StaticImporter ()
		{
		}

		protected override MemberKind DetermineKindFromBaseType (MetaType baseType)
		{
			string name = baseType.Name;

			// TODO: namespace check
			if (name == "ValueType")
				return MemberKind.Struct;

			if (name == "Enum")
				return MemberKind.Enum;

			if (name == "MulticastDelegate")
				return MemberKind.Delegate;

			return MemberKind.Class;
		}

		protected override bool HasVolatileModifier (FieldInfo field)
		{
			var reqs = field.GetRequiredCustomModifiers ();
			if (reqs.Length > 0) {
				foreach (var t in reqs) {
					if (t.Name == "IsVolatile" && t.Namespace == CompilerServicesNamespace)
						return true;
				}
			}

			return false;
		}

		public void ImportAssembly (Assembly assembly, RootNamespace targetNamespace)
		{
			// It can be used more than once when importing same assembly
			// into 2 or more global aliases
			var definition = GetAssemblyDefinition (assembly);

			var all_types = assembly.GetTypes ();
			ImportTypes (all_types, targetNamespace, definition.HasExtensionMethod);
		}

		public ImportedModuleDefinition ImportModule (Module module, RootNamespace targetNamespace)
		{
			var module_definition = new ImportedModuleDefinition (module, this);
			module_definition.ReadAttributes ();

			var all_types = module.GetTypes ();
			ImportTypes (all_types, targetNamespace, false);

			return module_definition;
		}

		public void InitializeBuildinTypes (BuildinTypes buildin, Assembly corlib)
		{
			//
			// Setup mapping for build-in types to avoid duplication of their definition
			//
			foreach (var type in buildin.AllTypes) {
				buildin_types.Add (corlib.GetType (type.FullName), type);
			}
		}
	}
#endif

	class AssemblyDefinitionStatic : AssemblyDefinition
	{
		//
		// Assembly container with file output
		//
		public AssemblyDefinitionStatic (ModuleContainer module, string name, string fileName)
			: base (module, name, fileName)
		{
		}

		//
		// Initializes the code generator
		//
		public bool Create (Universe domain)
		{
			ResolveAssemblySecurityAttributes ();
			var an = CreateAssemblyName ();

			Builder = domain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Save, Path.GetDirectoryName (file_name));

			builder_extra = new AssemblyBuilderIKVM (Builder, Compiler);
			return true;
		}

		public Module IncludeModule (RawModule moduleFile)
		{
			return Builder.__AddModule (moduleFile);
		}

		protected override void SaveModule (PortableExecutableKinds pekind, ImageFileMachine machine)
		{
			module.Builder.__Save (pekind, machine);
		}
	}

	class StaticLoader : AssemblyReferencesLoader<Assembly>, IDisposable
	{
		readonly StaticImporter importer;
		readonly Universe domain;

		public StaticLoader (StaticImporter importer, CompilerContext compiler)
			: base (compiler)
		{
			this.importer = importer;
			domain = new Universe ();
		}

		public Universe Domain {
			get {
				return domain;
			}
		}

		public void Dispose ()
		{
			domain.Dispose ();
		}

		protected override string[] GetDefaultReferences ()
		{
			//
			// For now the "default config" is harcoded into the compiler
			// we can move this outside later
			//
			var default_references = new List<string> (8);

			default_references.Add ("System.dll");
			default_references.Add ("System.Xml.dll");
#if NET_2_1
			default_references.Add ("System.Net.dll");
			default_references.Add ("System.Windows.dll");
			default_references.Add ("System.Windows.Browser.dll");
#endif

			// TODO: Will have to do it based on mscorlib version or something like that

			if (RootContext.Version > LanguageVersion.ISO_2)
				default_references.Add ("System.Core.dll");
			if (RootContext.Version > LanguageVersion.V_3)
				default_references.Add ("Microsoft.CSharp.dll");

			return default_references.ToArray ();
		}

		public override bool HasObjectType (Assembly assembly)
		{
			return assembly.GetType (compiler.BuildinTypes.Object.FullName) != null;
		}

		public override Assembly LoadAssemblyFile (string fileName)
		{
			bool? has_extension;
			foreach (var path in paths) {
				var file = Path.Combine (path, fileName);
				if (!File.Exists (file)) {
					if (!has_extension.HasValue)
						has_extension = fileName.EndsWith (".dll", StringComparison.Ordinal) || fileName.EndsWith (".exe", StringComparison.Ordinal);

					if (has_extension.Value)
						continue;

					file += ".dll";
					if (!File.Exists (file))
						continue;
				}

				try {
					using (RawModule module = domain.OpenRawModule (file)) {
						if (!module.IsManifestModule) {
							Error_AssemblyIsModule (fileName);
							return null;
						}

						return domain.LoadAssembly (module);
					}
				} catch {
					Error_FileCorrupted (file);
					return null;
				}
			}

			Error_FileNotFound (fileName);
			return null;
		}

		public RawModule LoadModuleFile (string moduleName)
		{
			foreach (var path in paths) {
				var file = Path.Combine (path, moduleName);
				if (!File.Exists (file)) {
					if (moduleName.EndsWith (".netmodule", StringComparison.Ordinal))
						continue;

					file += ".netmodule";
					if (!File.Exists (file))
						continue;
				}

				try {
					return domain.OpenRawModule (file);
				} catch {
					Error_FileCorrupted (file);
					return null;
				}
			}

			Error_FileNotFound (moduleName);
			return null;				
		}

		//
		// Optimized default assembly loader version
		//
		public override Assembly LoadAssemblyDefault (string assembly)
		{
			foreach (var path in paths) {
				var file = Path.Combine (path, assembly);
				if (!File.Exists (file))
					continue;

				try {
					return domain.LoadFile (file);
				} catch {
					// Default assemblies can fail to load without error
					return null;
				}
			}

			return null;
		}

		public override void LoadReferences (ModuleContainer module)
		{
			Assembly corlib;
			List<Tuple<RootNamespace, Assembly>> loaded;
			base.LoadReferencesCore (module, out corlib, out loaded);

			if (corlib == null)
				return;

			importer.InitializeBuildinTypes (compiler.BuildinTypes, corlib);
			importer.ImportAssembly (corlib, module.GlobalRootNamespace);

			foreach (var entry in loaded) {
				importer.ImportAssembly (entry.Item2, entry.Item1);
			}
		}

		public void LoadModules (AssemblyDefinitionStatic assembly, RootNamespace targetNamespace)
		{
			if (RootContext.Modules.Count == 0)
				return;

			foreach (var moduleName in RootContext.Modules) {
				var m = LoadModuleFile (moduleName);
				if (m == null)
					continue;

				if (m.IsManifestModule) {
					Error_FileCorrupted (moduleName);
					continue;
				}

				var md = importer.ImportModule (assembly.IncludeModule (m), targetNamespace);
				assembly.AddModule (md);
			}
		}
	}

	class AssemblyBuilderIKVM : AssemblyBuilderExtension
	{
		readonly AssemblyBuilder builder;

		public AssemblyBuilderIKVM (AssemblyBuilder builder, CompilerContext ctx)
			: base (ctx)
		{
			this.builder = builder;
		}

		public override void AddTypeForwarder (TypeSpec type, Location loc)
		{
			builder.__AddTypeForwarder (type.GetMetaInfo ());
		}

		public override void DefineWin32IconResource (string fileName)
		{
			builder.__DefineIconResource (File.ReadAllBytes (fileName));
		}

		public override void SetAlgorithmId (uint value, Location loc)
		{
			builder.__SetAssemblyAlgorithmId ((AssemblyHashAlgorithm) value);
		}

		public override void SetCulture (string culture, Location loc)
		{
			builder.__SetAssemblyCulture (culture);
		}

		public override void SetFlags (uint flags, Location loc)
		{
			builder.__SetAssemblyFlags ((AssemblyNameFlags) flags);
		}

		public override void SetVersion (Version version, Location loc)
		{
			builder.__SetAssemblyVersion (version);
		}
	}
}