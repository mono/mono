// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Moonlight.SecurityModel;

/// <summary>
/// Detect all methods calling [SecurityCritical] methods. In order to work (does not mean
/// they have too) they need to be decorated as [SecuritySafeCritical]. Otherwise the 
/// CoreCLR security model will throw an exception at runtime.
/// 
/// Note: [SecuritySafeCritical] code can be called by anyone, including transparent code,
/// so there is no compatibility issues to consider to add/remove them. However the less
/// we have the easier (shorter) the audit will be.
/// </summary>
class Program {

	static Dictionary<AssemblyDefinition, List<string>> methods = new Dictionary<AssemblyDefinition, List<string>> ();

	static void ProcessMethod (MethodDefinition method)
	{
		if (!method.HasBody)
			return;

		// SC code can call SC code
		// note: here we use the 'rock' because the [SecurityCritical] attribute 
		// could be located on the type (or nested type)
		if (method.IsSecurityCritical ())
			return;

		foreach (Instruction ins in method.Body.Instructions) {
			MethodReference mr = (ins.Operand as MethodReference);
			if (mr == null)
				continue;

			MethodDefinition md = mr.Resolve ();
			if (md == null) {
				// this can occurs for some generated types, like Int[,] where the compiler generates a few methods
				continue;
			}

			// again we use the rock here as we want the final result (not the local attribute)
			if (md.IsSecurityCritical ()) {
				AssemblyDefinition ad = method.DeclaringType.Module.Assembly;
				List<string> list = methods [ad];
				string m = method.ToString ();
				if (!list.Contains (m))
					list.Add (m);
				// TODO: is it worth adding comments (e.g. callers) ? if there's not too many
				// and/or just a count of them (to see if they would be easy to remove)
				// OTOH that will mess up diffs so it should be an option (if even)
			}
		}
	}

	static void ProcessType (TypeDefinition type)
	{
		if (type.HasConstructors) {
			foreach (MethodDefinition ctor in type.Constructors) {
				ProcessMethod (ctor);
			}
		}
		if (type.HasMethods) {
			foreach (MethodDefinition method in type.Methods) {
				ProcessMethod (method);
			}
		}
	}

	static void ProcessAssembly (AssemblyDefinition assembly)
	{
		foreach (ModuleDefinition module in assembly.Modules) {
			foreach (TypeDefinition type in module.Types) {
				ProcessType (type);
			}
		}
	}

	static int Main (string [] args)
	{
		int input;
		string output;

		switch (args.Length) {
		case 0:
			Console.WriteLine ("Usage: detect-ssc input-dir [input-dir [...]] [output-dir]");
			return 1;
		case 1:
			output = args [0];
			input = 1;
			break;
		default:
			input = args.Length - 1;
			output = args [input];
			break;
		}

		// load everything first (to ease resolving later)
		Console.WriteLine ("Loading assemblies...");
		foreach (string assembly in PlatformCode.Assemblies) {
			string fullpath = null;
			for (int i = 0; i < input; i++) {
				fullpath = Path.Combine (args [i], assembly) + ".dll";
				if (File.Exists (fullpath))
					break;

				fullpath = null;
			}

			if (fullpath == null) {
				Console.WriteLine ("{0} NOT FOUND!", assembly);
				continue;
			}

			AssemblyDefinition ad = AssemblyFactory.GetAssembly (fullpath);
			for (int i = 0; i < input; i++)
				(ad.Resolver as BaseAssemblyResolver).AddSearchDirectory (args [i]);
			methods.Add (ad, new List<string> ());
		}

		// then process
		Console.WriteLine ("Processing...");
		foreach (AssemblyDefinition ad in methods.Keys) {
			ProcessAssembly (ad);
		}

		// and report
		foreach (KeyValuePair<AssemblyDefinition,List<string>> kpv in methods) {
			List<string> list = kpv.Value;
			string assembly = kpv.Key.Name.Name;
			string outfile = Path.Combine (output, assembly) + ".auto.ssc";
			using (StreamWriter sw = new StreamWriter (outfile)) {
				sw.WriteLine ("# [SecuritySafeCritical] needed inside {0} to call all [SecurityCritical] methods", assembly);
				sw.WriteLine ("# {0} methods", list.Count);
				sw.WriteLine ();
				foreach (string method in list) {
					sw.WriteLine ("+SSC-M: {0}", method);
				}
				Console.WriteLine ("{0}.dll: {1} methods.", assembly, list.Count);
			}
		}
		return 0;
	}
}
