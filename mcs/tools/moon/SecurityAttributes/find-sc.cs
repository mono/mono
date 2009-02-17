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
using Moonlight.SecurityModel;

/// <summary>
/// Find which SL2 visible API are decorated with [SecurityCritical] attributes.
/// They are specially important since they are required for compatibility between
/// Moonlight and Silverlight.
/// </summary>
class Program {

	private const string SecurityCritical = "System.Security.SecurityCriticalAttribute";
	private const string SecuritySafeCritical = "System.Security.SecuritySafeCriticalAttribute";

	static List<string> types = new List<string> ();
	static List<string> methods = new List<string> ();

	static void ProcessMethod (MethodDefinition method)
	{
		// only visible API is important for compatibility
		if (!method.IsVisible ())
			return;

		// note: IsSecurityCritical rock is not what we want to use here, since
		// we're only interested in the presence (or absence) of the attribute
		if (method.HasAttribute (SecurityCritical))
			methods.Add (method.ToString ());
	}

	static void ProcessType (TypeDefinition type)
	{
		// only visible API is important for compatibility
		if (!type.IsVisible ())
			return;

		// note: IsSecurityCritical rock is not what we want to use here, since
		// we're only interested in the presence (or absence) of the attribute
		if (type.HasAttribute (SecurityCritical))
			types.Add (type.FullName);

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
		if (args.Length < 1) {
			Console.WriteLine ("Usage: find-sc input-dir [output-dir]");
			return 1;
		}

		string input = args [0];
		string output = (args.Length < 2) ? input : args [1];

		foreach (string assembly in PlatformCode.Assemblies) {
			Console.Write ("{0}.dll:", assembly);

			string fullpath = Path.Combine (input, assembly) + ".dll";
			if (!File.Exists (fullpath)) {
				Console.WriteLine (" NOT FOUND!");
				continue;
			}

			AssemblyDefinition ad = AssemblyFactory.GetAssembly (fullpath);
			ProcessAssembly (ad);

			// this will make it easier to diff in the future
			types.Sort ();
			methods.Sort ();

			string outfile = Path.Combine (output, assembly) + ".compat.sc";
			using (StreamWriter sw = new StreamWriter (outfile)) {
				sw.WriteLine ("# [SecurityCritical] present inside the visible API of '{0}'.", ad.Name.FullName);
				sw.WriteLine ("# {0} types and {1} methods were decorated.", types.Count, methods.Count);
				sw.WriteLine ();
				Console.Write (" {0} types", types.Count);
				foreach (string s in types)
					sw.WriteLine ("!SC-T: {0}", s);
				sw.WriteLine ();
				Console.Write (" {0} methods", methods.Count);
				foreach (string s in methods)
					sw.WriteLine ("!SC-M: {0}", s);
			}

			types.Clear ();
			methods.Clear ();
			Console.WriteLine (".");
		}
		return 0;
	}
}
