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

namespace Mono.CSharp
{
	public class ReflectionImporter : MetadataImporter
	{
		public ReflectionImporter (BuildinTypes buildin)
			: base ()
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

		protected override bool HasVolatileModifier (FieldInfo field)
		{
			var reqs = field.GetRequiredCustomModifiers ();
			if (reqs.Length > 0) {
				foreach (var t in reqs) {
					if (t == typeof (IsVolatile))
						return true;
				}
			}

			return false;
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

	public class DynamicLoader
	{
		readonly ReflectionImporter importer;
		readonly Report reporter;

		// A list of default references, they can fail to load as the user didn't not specify them
		string[] default_references;

		List<string> paths;

		public DynamicLoader (ReflectionImporter importer, CompilerContext compiler)
		{
			this.importer = importer;
			this.reporter = compiler.Report;

			default_references = GetDefaultReferences ();

			paths = new List<string> ();
			paths.AddRange (RootContext.ReferencesLookupPaths);
			paths.Add (GetSystemDir ());
			paths.Add (Directory.GetCurrentDirectory ());
		}

		public ReflectionImporter Importer {
			get {
				return importer;
			}
		}

		void Error6 (string name, string log)
		{
			if (log != null && log.Length > 0)
				reporter.ExtraInformation (Location.Null, "Log:\n" + log + "\n(log related to previous ");
			reporter.Error (6, "cannot find metadata file `{0}'", name);
		}

		void Error9 (string type, string filename, string log)
		{
			if (log != null && log.Length > 0)
				reporter.ExtraInformation (Location.Null, "Log:\n" + log + "\n(log related to previous ");
			reporter.Error (9, "file `{0}' has invalid `{1}' metadata", filename, type);
		}

		void BadAssembly (string filename, string log)
		{
/*
			MethodInfo adder_method = null; // AssemblyDefinition.AddModule_Method;

			if (adder_method != null) {
				AssemblyName an = new AssemblyName ();
				an.Name = ".temp";
				var ab = AppDomain.CurrentDomain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Run);
				try {
					object m = null;
					try {
						m = adder_method.Invoke (ab, new object [] { filename });
					} catch (TargetInvocationException ex) {
						throw ex.InnerException;
					}

					if (m != null) {
						Report.Error (1509, "Referenced file `{0}' is not an assembly. Consider using `-addmodule' option instead",
										Path.GetFileName (filename));
						return;
					}
				} catch (FileNotFoundException) {
					// did the file get deleted during compilation? who cares? swallow the exception
				} catch (BadImageFormatException) {
					// swallow exception
				} catch (FileLoadException) {
					// swallow exception
				}
			}
*/
			Error9 ("assembly", filename, log);
		}

		//
		// Returns the directory where the system assemblies are installed
		//
		static string GetSystemDir ()
		{
			return Path.GetDirectoryName (typeof (object).Assembly.Location);
		}

		string[] GetDefaultReferences ()
		{
			if (!RootContext.LoadDefaultReferences)
				return new string [0];

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

		public Assembly LoadAssemblyFile (string assembly, bool soft)
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
						Error6 (assembly, total_log);
						return a;
					}
				}
			} catch (BadImageFormatException f) {
				// .NET 2.0 throws this if we try to load a module without an assembly manifest ...
				BadAssembly (f.FileName, f.FusionLog);
			} catch (FileLoadException f) {
				// ... while .NET 1.1 throws this
				BadAssembly (f.FileName, f.FusionLog);
			}

			return a;
		}

		void LoadModule (AssemblyDefinition assembly, string module)
		{
			string total_log = "";

			try {
				try {
					assembly.AddModule (module);
				} catch (FileNotFoundException) {
					bool err = true;
					foreach (string dir in paths) {
						string full_path = Path.Combine (dir, module);
						if (!module.EndsWith (".netmodule"))
							full_path += ".netmodule";

						try {
							assembly.AddModule (full_path);
							err = false;
							break;
						} catch (FileNotFoundException ff) {
							total_log += ff.FusionLog;
						}
					}
					if (err) {
						Error6 (module, total_log);
						return;
					}
				}
			} catch (BadImageFormatException f) {
				Error9 ("module", f.FileName, f.FusionLog);
			} catch (FileLoadException f) {
				Error9 ("module", f.FileName, f.FusionLog);
			}
		}

		/// <summary>
		///   Loads all assemblies referenced on the command line
		/// </summary>
		public void LoadReferences (ModuleContainer module)
		{
			Assembly a;
			var loaded = new List<Tuple<RootNamespace, Assembly>> ();

			//
			// Load Core Library for default compilation
			//
			if (RootContext.StdLib) {
				a = LoadAssemblyFile ("mscorlib", false);
				if (a != null)
					loaded.Add (Tuple.Create (module.GlobalRootNamespace, a));
			}

			foreach (string r in default_references) {
				a = LoadAssemblyFile (r, true);
				if (a != null)
					loaded.Add (Tuple.Create (module.GlobalRootNamespace, a));
			}

			foreach (string r in RootContext.AssemblyReferences) {
				a = LoadAssemblyFile (r, false);
				if (a == null)
					continue;

				var key = Tuple.Create (module.GlobalRootNamespace, a);
				if (loaded.Contains (key))
					continue;

				loaded.Add (key);
			}

			foreach (var entry in RootContext.AssemblyReferencesAliases) {
				a = LoadAssemblyFile (entry.Item2, false);
				if (a == null)
					continue;

				var key = Tuple.Create (module.CreateRootNamespace (entry.Item1), a);
				if (loaded.Contains (key))
					continue;

				loaded.Add (key);
			}

			foreach (var entry in loaded) {
				importer.ImportAssembly (entry.Item2, entry.Item1);
			}
		}

		public void LoadModules (AssemblyDefinition assembly)
		{
			if (RootContext.Modules.Count == 0)
				return;

			foreach (var module in RootContext.Modules) {
				LoadModule (assembly, module);
			}
		}
	}
}