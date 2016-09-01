//
// ikvm.cs: IKVM.Reflection and IKVM.Reflection.Emit specific implementations
//
// Author: Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2009-2010 Novell, Inc. 
// Copyright 2011 Xamarin Inc
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
		public StaticImporter (BuiltinTypes builtin)
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

	sealed class StaticImporter : MetadataImporter
	{
		public StaticImporter (ModuleContainer module)
			: base (module)
		{
		}

		public void AddCompiledAssembly (AssemblyDefinitionStatic assembly)
		{
			assembly_2_definition.Add (assembly.Builder, assembly);
		}

		public override void AddCompiledType (TypeBuilder type, TypeSpec spec)
		{
			compiled_types.Add (type, spec);
		}

		protected override MemberKind DetermineKindFromBaseType (MetaType baseType)
		{
			string name = baseType.Name;

			if (name == "ValueType" && baseType.Namespace == "System")
				return MemberKind.Struct;

			if (name == "Enum" && baseType.Namespace == "System")
				return MemberKind.Enum;

			if (name == "MulticastDelegate" && baseType.Namespace == "System")
				return MemberKind.Delegate;

			return MemberKind.Class;
		}

		protected override bool HasVolatileModifier (MetaType[] modifiers)
		{
			foreach (var t in modifiers) {
				if (t.Name == "IsVolatile" && t.Namespace == CompilerServicesNamespace)
					return true;
			}

			return false;
		}

		public void ImportAssembly (Assembly assembly, RootNamespace targetNamespace)
		{
			try {
				// It can be used more than once when importing same assembly
				// into 2 or more global aliases
				// TODO: Should be just Add
				GetAssemblyDefinition (assembly);

				var all_types = assembly.GetTypes ();
				ImportTypes (all_types, targetNamespace, true);

				all_types = assembly.ManifestModule.__GetExportedTypes ();
				if (all_types.Length != 0)
					ImportForwardedTypes (all_types, targetNamespace);
			} catch (Exception e) {
				throw new InternalErrorException (e, "Failed to import assembly `{0}'", assembly.FullName);
			}
		}

		public ImportedModuleDefinition ImportModule (Module module, RootNamespace targetNamespace)
		{
			var module_definition = new ImportedModuleDefinition (module);
			module_definition.ReadAttributes ();

			var all_types = module.GetTypes ();
			ImportTypes (all_types, targetNamespace, false);

			return module_definition;
		}

		void ImportForwardedTypes (MetaType[] types, Namespace targetNamespace)
		{
			Namespace ns = targetNamespace;
			string prev_namespace = null;
			foreach (var t in types) {
				if (!t.__IsTypeForwarder)
					continue;

				// IsMissing tells us the type has been forwarded and target assembly is missing 
				if (!t.__IsMissing)
					continue;

				if (t.Name[0] == '<')
					continue;

				var it = CreateType (t, null, new DynamicTypeReader (t), true);
				if (it == null)
					continue;

				if (prev_namespace != t.Namespace) {
					ns = t.Namespace == null ? targetNamespace : targetNamespace.GetNamespace (t.Namespace, true);
					prev_namespace = t.Namespace;
				}

				ns.AddType (module, it);
			}
		}

		public void InitializeBuiltinTypes (BuiltinTypes builtin, Assembly corlib)
		{
			//
			// Setup mapping for build-in types to avoid duplication of their definition
			//
			foreach (var type in builtin.AllTypes) {
				compiled_types.Add (corlib.GetType (type.FullName), type);
			}
		}
	}
#endif

	class AssemblyDefinitionStatic : AssemblyDefinition
	{
		readonly StaticLoader loader;

		//
		// Assembly container with file output
		//
		public AssemblyDefinitionStatic (ModuleContainer module, StaticLoader loader, string name, string fileName)
			: base (module, name, fileName)
		{
			this.loader = loader;
			Importer = loader.MetadataImporter;
		}

		//
		// Initializes the assembly SRE domain
		//
		public void Create (Universe domain)
		{
			ResolveAssemblySecurityAttributes ();
			var an = CreateAssemblyName ();

			Builder = domain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Save, Path.GetDirectoryName (file_name));
			module.Create (this, CreateModuleBuilder ());
		}

		public override void Emit ()
		{
			if (loader.Corlib != null && !(loader.Corlib is AssemblyBuilder)) {
				Builder.__SetImageRuntimeVersion (loader.Corlib.ImageRuntimeVersion, 0x20000);
			} else if (module.Compiler.Settings.RuntimeMetadataVersion != null) {
				Builder.__SetImageRuntimeVersion (module.Compiler.Settings.RuntimeMetadataVersion, 0x20000);
			} else {
				// Sets output file metadata version when there is no mscorlib
				switch (module.Compiler.Settings.StdLibRuntimeVersion) {
				case RuntimeVersion.v4:
					Builder.__SetImageRuntimeVersion ("v4.0.30319", 0x20000);
					break;
				case RuntimeVersion.v2:
					Builder.__SetImageRuntimeVersion ("v2.0.50727", 0x20000);
					break;
				case RuntimeVersion.v1:
					// Compiler does not do any checks whether the produced metadata
					// are valid in the context of 1.0 stream version
					Builder.__SetImageRuntimeVersion ("v1.1.4322", 0x10000);
					break;
				default:
					throw new NotImplementedException ();
				}
			}

			builder_extra = new AssemblyBuilderIKVM (Builder, Compiler);

			base.Emit ();
		}

		public Module IncludeModule (RawModule moduleFile)
		{
			return Builder.__AddModule (moduleFile);
		}

		protected override List<AssemblyReferenceMessageInfo> GetNotUnifiedReferences (AssemblyName assemblyName)
		{
			return loader.GetNotUnifiedReferences (assemblyName);
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
		Assembly corlib;
		readonly List<Tuple<AssemblyName, string, Assembly>> loaded_names;
		static readonly Dictionary<string, string[]> sdk_directory;
		Dictionary<AssemblyName, List<AssemblyReferenceMessageInfo>> resolved_version_mismatches;
		static readonly TypeName objectTypeName = new TypeName ("System", "Object");

		static StaticLoader ()
		{
			sdk_directory = new Dictionary<string, string[]> ();
			sdk_directory.Add ("2", new string[] { "2.0-api", "v2.0.50727" });
			sdk_directory.Add ("2.0", new string[] { "2.0-api", "v2.0.50727" });
			sdk_directory.Add ("4", new string[] { "4.0-api", "v4.0.30319" });
			sdk_directory.Add ("4.0", new string[] { "4.0-api", "v4.0.30319" });
			sdk_directory.Add ("4.5", new string[] { "4.5-api", "v4.0.30319" });
			sdk_directory.Add ("4.6", new string [] { "4.5", "net_4_x", "v4.0.30319" });
		}

		public StaticLoader (StaticImporter importer, CompilerContext compiler)
			: base (compiler)
		{
			this.importer = importer;
			domain = new Universe (UniverseOptions.MetadataOnly | UniverseOptions.ResolveMissingMembers | 
				UniverseOptions.DisableFusion | UniverseOptions.DecodeVersionInfoAttributeBlobs |
				UniverseOptions.DeterministicOutput | UniverseOptions.DisableDefaultAssembliesLookup);
			
			domain.AssemblyResolve += AssemblyReferenceResolver;
			loaded_names = new List<Tuple<AssemblyName, string, Assembly>> ();

			if (compiler.Settings.StdLib) {
				var corlib_path = Path.GetDirectoryName (typeof (object).Assembly.Location);
				string fx_path = corlib_path.Substring (0, corlib_path.LastIndexOf (Path.DirectorySeparatorChar));

				string sdk_path = null;

				string sdk_version = compiler.Settings.SdkVersion ?? "4.6";
				string[] sdk_sub_dirs;

				if (!sdk_directory.TryGetValue (sdk_version, out sdk_sub_dirs))
					sdk_sub_dirs = new string[] { sdk_version };

				foreach (var dir in sdk_sub_dirs) {
					sdk_path = Path.Combine (fx_path, dir);
					if (File.Exists (Path.Combine (sdk_path, "mscorlib.dll")))
						break;

					sdk_path = null;
				}

				if (sdk_path == null) {
					compiler.Report.Warning (-1, 1, "SDK path could not be resolved");
					sdk_path = corlib_path;
				}

				paths.Add (sdk_path);
			}
		}

		#region Properties

		public Assembly Corlib {
			get {
				return corlib;
			}
		}

		public AssemblyDefinitionStatic CompiledAssembly {  get; set; }

		public Universe Domain {
			get {
				return domain;
			}
		}

		public StaticImporter MetadataImporter {
			get {
				return importer;
			}
		}

		#endregion

		Assembly AssemblyReferenceResolver (object sender, IKVM.Reflection.ResolveEventArgs args)
		{
			var refname = args.Name;
			if (refname == "mscorlib")
				return corlib;

			Assembly version_mismatch = null;
			bool is_fx_assembly = false;

			foreach (var assembly in domain.GetAssemblies ()) {
				AssemblyComparisonResult result;
				if (!domain.CompareAssemblyIdentity (refname, false, assembly.FullName, false, out result)) {
					if ((result == AssemblyComparisonResult.NonEquivalentVersion || result == AssemblyComparisonResult.NonEquivalentPartialVersion) &&
						(version_mismatch == null || version_mismatch.GetName ().Version < assembly.GetName ().Version) &&
						!is_fx_assembly) {
						version_mismatch = assembly;
					}

					continue;
				}

				if (result == AssemblyComparisonResult.EquivalentFullMatch ||
					result == AssemblyComparisonResult.EquivalentWeakNamed ||
					result == AssemblyComparisonResult.EquivalentPartialMatch) {
					return assembly;
				}

				if (result == AssemblyComparisonResult.EquivalentFXUnified) {
					is_fx_assembly = true;

					if (version_mismatch == null || version_mismatch.GetName ().Version < assembly.GetName ().Version)
						version_mismatch = assembly;

					continue;
				}

				throw new NotImplementedException ("Assembly equality = " + result.ToString ());
			}

			if (version_mismatch != null) {
				if (version_mismatch is AssemblyBuilder)
					return version_mismatch;

				var ref_an = new AssemblyName (refname);
				var v1 = ref_an.Version;
				var v2 = version_mismatch.GetName ().Version;

				if (v1 > v2) {
					var messageInfo = new AssemblyReferenceMessageInfo (ref_an, report => {
						report.SymbolRelatedToPreviousError (args.RequestingAssembly.Location);
						report.Error (1705, string.Format ("Assembly `{0}' depends on `{1}' which has a higher version number than referenced assembly `{2}'",
														   args.RequestingAssembly.FullName, refname, version_mismatch.GetName ().FullName));
					});

					AddReferenceVersionMismatch (args.RequestingAssembly.GetName (), messageInfo);
					return version_mismatch;
				}

				if (!is_fx_assembly) {
					var messageInfo = new AssemblyReferenceMessageInfo (ref_an, report => {
						if (v1.Major != v2.Major || v1.Minor != v2.Minor) {
							report.Warning (1701, 2,
								"Assuming assembly reference `{0}' matches assembly `{1}'. You may need to supply runtime policy",
								refname, version_mismatch.GetName ().FullName);
						} else {
							report.Warning (1702, 3,
								"Assuming assembly reference `{0}' matches assembly `{1}'. You may need to supply runtime policy",
								refname, version_mismatch.GetName ().FullName);
						}
					});

					AddReferenceVersionMismatch (args.RequestingAssembly.GetName (), messageInfo);
				}

				return version_mismatch;
			}

			//
			// Recursive reference to compiled assembly checks name only. Any other
			// details (PublicKey, Version, etc) are not yet known hence cannot be checked
			//
			ParsedAssemblyName referenced_assembly;
			if (Fusion.ParseAssemblyName (args.Name, out referenced_assembly) == ParseAssemblyResult.OK && CompiledAssembly.Name == referenced_assembly.Name)
				return CompiledAssembly.Builder;

			// AssemblyReference has not been found in the domain
			// create missing reference and continue
			return domain.CreateMissingAssembly (args.Name);
		}

		void AddReferenceVersionMismatch (AssemblyName an, AssemblyReferenceMessageInfo errorInfo)
		{
			if (resolved_version_mismatches == null)
				resolved_version_mismatches = new Dictionary<AssemblyName, List<AssemblyReferenceMessageInfo>> ();

			List<AssemblyReferenceMessageInfo> names;
			if (!resolved_version_mismatches.TryGetValue (an, out names)) {
				names = new List<AssemblyReferenceMessageInfo> ();
				resolved_version_mismatches.Add (an, names);
			}

			names.Add (errorInfo);
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
			var default_references = new List<string> (4);

			default_references.Add ("System.dll");
			default_references.Add ("System.Xml.dll");
			default_references.Add ("System.Core.dll");

			if (corlib != null && corlib.GetName ().Version.Major >= 4) {
				default_references.Add ("Microsoft.CSharp.dll");
			}

			return default_references.ToArray ();
		}

		public List<AssemblyReferenceMessageInfo> GetNotUnifiedReferences (AssemblyName assemblyName)
		{
			List<AssemblyReferenceMessageInfo> list = null;
			if (resolved_version_mismatches != null)
				resolved_version_mismatches.TryGetValue (assemblyName, out list);

			return list;
		}

		public override Assembly HasObjectType (Assembly assembly)
		{
			try {
				// System.Object can be forwarded and ikvm
				// transparently finds it in target assembly therefore
				// need to return actual obj assembly becauase in such
				// case it's different to assembly parameter
				var obj = assembly.FindType (objectTypeName);
				return obj == null ? null : obj.Assembly;
			} catch (Exception e) {
				throw new InternalErrorException (e, "Failed to load assembly `{0}'", assembly.FullName);
			}
		}

		public override Assembly LoadAssemblyFile (string fileName, bool isImplicitReference)
		{
			bool? has_extension = null;
			foreach (var path in paths) {
				var file = Path.Combine (path, fileName);
				if (compiler.Settings.DebugFlags > 0)
					Console.WriteLine ("Probing assembly location `{0}'", file);

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
					using (var stream = new FileStream (file, FileMode.Open, FileAccess.Read, FileShare.Read)) {
						using (RawModule module = domain.OpenRawModule (stream, file)) {
							if (!module.IsManifestModule) {
								Error_AssemblyIsModule (fileName);
								return null;
							}

							//
							// check whether the assembly can be actually imported without
							// collision
							//
							var an = module.GetAssemblyName ();
							foreach (var entry in loaded_names) {
								var loaded_name = entry.Item1;
								if (an.Name != loaded_name.Name)
									continue;

								if (module.ModuleVersionId == entry.Item3.ManifestModule.ModuleVersionId)
									return entry.Item3;
							
								if (((an.Flags | loaded_name.Flags) & AssemblyNameFlags.PublicKey) == 0) {
									compiler.Report.SymbolRelatedToPreviousError (entry.Item2);
									compiler.Report.SymbolRelatedToPreviousError (fileName);
									compiler.Report.Error (1704,
										"An assembly with the same name `{0}' has already been imported. Consider removing one of the references or sign the assembly",
										an.Name);
									return null;
								}

								if ((an.Flags & AssemblyNameFlags.PublicKey) == (loaded_name.Flags & AssemblyNameFlags.PublicKey)) {
									compiler.Report.SymbolRelatedToPreviousError (entry.Item2);
									compiler.Report.SymbolRelatedToPreviousError (fileName);
									compiler.Report.Error (1703,
										"An assembly `{0}' with the same identity has already been imported. Consider removing one of the references",
										an.Name);
									return null;
								}
							}

							if (compiler.Settings.DebugFlags > 0)
								Console.WriteLine ("Loading assembly `{0}'", fileName);

							var assembly = domain.LoadAssembly (module);
							if (assembly != null)
								loaded_names.Add (Tuple.Create (an, fileName, assembly));

							return assembly;
						}
					}
				} catch (Exception e) {
					if (compiler.Settings.DebugFlags > 0)
						Console.WriteLine ("Exception during loading: {0}'", e.ToString ());

					if (!isImplicitReference)
						Error_FileCorrupted (file);

					return null;
				}
			}

			if (!isImplicitReference)
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

		public override void LoadReferences (ModuleContainer module)
		{
			List<Tuple<RootNamespace, Assembly>> loaded;
			base.LoadReferencesCore (module, out corlib, out loaded);

			compiler.TimeReporter.Start (TimeReporter.TimerType.ReferencesImporting);

			if (corlib == null) {
				// System.Object was not found in any referenced assembly, use compiled assembly as corlib
				corlib = module.DeclaringAssembly.Builder;
			} else {
				importer.InitializeBuiltinTypes (compiler.BuiltinTypes, corlib);
				importer.ImportAssembly (corlib, module.GlobalRootNamespace);
			}

			foreach (var entry in loaded) {
				importer.ImportAssembly (entry.Item2, entry.Item1);
			}

			compiler.TimeReporter.Stop (TimeReporter.TimerType.ReferencesImporting);
		}

		public void LoadModules (AssemblyDefinitionStatic assembly, RootNamespace targetNamespace)
		{
			foreach (var moduleName in compiler.Settings.Modules) {
				var m = LoadModuleFile (moduleName);
				if (m == null)
					continue;

				if (m.IsManifestModule) {
					Error_ModuleIsAssembly (moduleName);
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
			builder.__AddTypeForwarder (type.GetMetaInfo (), false);
		}

		public override void DefineWin32IconResource (string fileName)
		{
			byte[] bytes;
			try {
				bytes = File.ReadAllBytes (fileName);
			} catch (Exception e) {
				ctx.Report.Error (7064, Location.Null, "Error opening icon file `{0}'. {1}", fileName, e.Message);
				return;
			}

			builder.__DefineIconResource (bytes);
		}

		public override AssemblyName[] GetReferencedAssemblies ()
		{
			foreach (var m in builder.Modules) {
				if (m is ModuleBuilder)
					return m.__GetReferencedAssemblies ();
			}

			return new AssemblyName [0];
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
			builder.__AssemblyFlags = (AssemblyNameFlags) flags;
		}

		public override void SetVersion (Version version, Location loc)
		{
			builder.__SetAssemblyVersion (version);
		}
	}
}
