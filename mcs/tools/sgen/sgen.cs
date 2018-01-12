// 
// sgen.cs
//
// Authors:
//   Lluis Sanchez Gual (lluis@ximian.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
// Copyright (C) 2006 Novell, Inc.
//

using System;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.CodeDom.Compiler;

public class Driver
{
	static int Main (string[] args)
	{
		Driver d = new Driver();
		return d.Run (args);
	}
		
	string assembly;
	ArrayList references = new ArrayList ();
	ArrayList types;
	string compilerOptions;
	bool proxyTypes;
	bool debug;
	bool keep;
	bool force;
	string outDir;
	bool help;
	bool silent;
	bool nologo;
	bool verbose;
	string unknownArg;
	

	public int Run (string[] args)
	{
		ParseArgs (args);
		
		if (!nologo)
		{
			Console.WriteLine ("Mono Xml Serializer Generator Tool");
			Console.WriteLine ("Mono version " + Environment.Version);
			Console.WriteLine ();
		}
		
		if (unknownArg != null)
		{
			Console.WriteLine ("Unknown option: " + unknownArg);
			Console.WriteLine ();
			return 1;
		}
		
		if (help)
		{
			Console.WriteLine ("Usage: sgen [options]");
			Console.WriteLine ();
			return 0;
		}
		
		if (assembly == null) {
			Console.WriteLine ("Assembly name not provided");
			Console.WriteLine ();
			return 1;
		}
		
		Assembly asm = null;
		
		try {
			asm = Assembly.Load (assembly);
		}
		catch {}
		
		if (asm == null) {
			try {
				asm = Assembly.LoadFrom (assembly);
			} catch {
				Console.WriteLine ("Specified assembly cannot be loaded.");
				Console.WriteLine ();
				return 1;
			}
		}
		ArrayList userTypes = new ArrayList ();
		ArrayList maps = new ArrayList ();
		XmlReflectionImporter imp = new XmlReflectionImporter ();
		
		if (verbose)
			Console.WriteLine ("Generating serializer for the following types:");

		if (types == null) {
			Type [] types;

			try {
				types = asm.GetTypes ();
			} catch (ReflectionTypeLoadException tle){
				Console.WriteLine ($"There was an error loading one or more of the types from the referenced assembly {assembly}");
				foreach (var le in tle.LoaderExceptions){
					Console.WriteLine (le);
				}
				return 1;
			}
			
			foreach (Type t in types){
				try {
					maps.Add (imp.ImportTypeMapping (t));
					userTypes.Add (t);
					if (verbose)
						Console.WriteLine( " - " + t );
				} catch (InvalidOperationException ex) {
					if (verbose)
						Console.WriteLine (" - Warning: " + ex.Message);
				} catch (NotImplementedException ex) {
					if (verbose) {
						Console.WriteLine (" - Warning: ignoring '" + t + "'");
						Console.WriteLine ("   " + ex.Message);
					}
				} catch (NotSupportedException ex) {
					if (verbose)
						Console.WriteLine (" - Warning: " + ex.Message);
				}
			}
		} else {
			foreach (string type in types) {
				try {
					Type t = asm.GetType (type);
					maps.Add (imp.ImportTypeMapping (t));
					userTypes.Add (t);
					if (verbose)
						Console.WriteLine (" - " + t);
				} catch (InvalidOperationException ex) {
					if (verbose)
						Console.WriteLine (" - Warning: " + ex.Message);
				} catch (NotImplementedException ex) {
					if (verbose) {
						Console.WriteLine (" - Warning: ignoring '" + type + "'");
						Console.WriteLine ("   " + ex.Message);
					}
				} catch (NotSupportedException ex) {
					if (verbose)
						Console.WriteLine (" - Warning: " + ex.Message);
				}
			}
		}

		if (verbose)
			Console.WriteLine ();
			
		CompilerParameters parameters = new CompilerParameters ();
		parameters.GenerateInMemory = false;
		parameters.IncludeDebugInformation = debug;
		parameters.ReferencedAssemblies.AddRange ((string[])references.ToArray(typeof(string)));
		parameters.TempFiles = new TempFileCollection (Environment.CurrentDirectory, keep);
		parameters.CompilerOptions = compilerOptions;
		
		string file = Path.GetFileNameWithoutExtension (asm.Location) + ".XmlSerializers.dll";
		if (outDir == null) outDir = Path.GetDirectoryName (asm.Location);
		parameters.OutputAssembly = Path.Combine (outDir, file);
		
		if (File.Exists (parameters.OutputAssembly) && !force) {
			Console.WriteLine ("Cannot generate assembly '" + parameters.OutputAssembly + "' because it already exist. Use /force option to overwrite the existing assembly");
			Console.WriteLine ();
			return 1;
		}
		
		XmlSerializer.GenerateSerializer (
				(Type[]) userTypes.ToArray (typeof(Type)), 
				(XmlTypeMapping[]) maps.ToArray (typeof(XmlTypeMapping)), 
				parameters);
				
		if (!silent) {
			Console.WriteLine ("Generated assembly: " + file);
			Console.WriteLine ();
		}
		
		return 0;
	}

	void ParseArgs (string[] args)
	{
		foreach (string arg in args)
		{
			int index = arg.Length > 2 && arg [0] == '-' && arg [1] == '-' ? 2 : -1;
			index = index >= 0 ? index : arg.Length > 0 && arg [0] == '/' ? 1 : -1;
			if (index < 0)
			{
				assembly = arg;
				continue;
			}
			
			int i = arg.IndexOf (':', index);
			if (i == -1) i = arg.Length;
			string op = arg.Substring (index, i - index).ToLowerInvariant ();
			string param = (i < arg.Length - index) ? arg.Substring (i + 1) : "";
			if (op == "assembly" || op == "a") {
				assembly = param;
			}
			else if (op == "type" || op == "t") {
				if (types == null) types = new ArrayList ();
				types.Add (param);
			}
			else if (op == "reference" || op == "r") {
				references.Add (param);
			}
			else if (op == "compiler" || op == "c") {
				compilerOptions = param;
			}
			else if (op == "proxytypes" || op == "p") {
				proxyTypes = true;
			}
			else if (op == "debug" || op == "d") {
				debug = true;
			}
			else if (op == "keep" || op == "k") {
				keep = true;
			}
			else if (op == "force" || op == "f") {
				force = true;
			}
			else if (op == "out" || op == "o") {
				outDir = param;
			}
			else if (op == "?" || op == "help") {
				help = true;
			}
			else if (op == "nologo" || op == "n") {
				nologo = true;
			}
			else if (op == "silent" || op == "s") {
				silent = true;
			}
			else if (op == "verbose" || op == "v") {
				verbose = true;
			}
			else if (arg.StartsWith ("/") && (arg.EndsWith (".dll") || arg.EndsWith (".exe")) && arg.IndexOfAny (Path.InvalidPathChars) == -1)
			{
				assembly = arg;
				continue;
			}
			else {
				unknownArg = arg;
				return;
			}
		}
	}
}

