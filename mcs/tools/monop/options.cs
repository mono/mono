//
// options.cs: Processes command line args
//
// Author:
//	John Luke  <john.luke@gmail.com>
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

public class Options
{
	public bool DeclaredOnly = false;
	public bool FilterObsolete = false;
	public bool PrintRefs = false;
	public bool Search = false;
	public bool ShowPrivate = false;
	public string AssemblyReference = null;
	public string Type = null;

	public Options ()
	{
	}

	// returning true means processed ok
	// and execution should continue
	internal bool ProcessArgs (string[] args)
	{
		if (args.Length < 1) {
			PrintHelp ();
			return false;
		}

		for (int i = 0; i < args.Length; i++) {
			switch (args[i]) {
				case "-h":
				case "--help":
					PrintHelp ();
					return false;
				case "--runtime-version":
					PrintRuntimeVersion ();
					return false;
				case "-d":
				case "--declared-only":
					DeclaredOnly = true;
					break;
				case "--filter-obsolete":
				case "-f":
					FilterObsolete = true;
					break;
				case "-p":
				case "--private":
					ShowPrivate = true;
					break;
				case "--refs":
					PrintRefs = true;
					break;
				case "-s":
				case "-k":
				case "--search":
					Search = true;
					break;
				case "-c":
					i++;
					if (i < args.Length)
						MonoP.Completion (args[i]);
					return false;
				case "-r":
					i++;
					if (i < args.Length)
						AssemblyReference = args[i];
					break;
				default:
					if (args[i].StartsWith ("-r:") || args[i].StartsWith ("/r:")) {
						AssemblyReference = args [i].Substring (3);
						break;
					}

					// The first unrecognizable option becomes
					// the type to look up
					if (Type == null) {
						Type = args[i];
						break;
					}

					// others are ignored
					Console.WriteLine ("ignored: {0}", args[i]);
					break;
			}
		}

		// must specify at least one of these
		if (Type == null && AssemblyReference == null)
			return false;

		return true;
	}

	void PrintRuntimeVersion ()
	{
		Console.WriteLine ("runtime version: {0}", Environment.Version);
	}

	void PrintHelp ()
	{
		Console.WriteLine ("Usage is: monop [option] [-c] [-r:Assembly] [class-name]");
		Console.WriteLine ("");
		Console.WriteLine ("options:");
		Console.WriteLine ("\t--declared-only,-d\tOnly show members declared in the Type");
		Console.WriteLine ("\t--help,-h\t\tShow this information");
		Console.WriteLine ("\t--filter-obsolete,-f\tDo not show obsolete types and members");
		Console.WriteLine ("\t--private,-p\t\tShow private members");
		Console.WriteLine ("\t--refs\t\t\tPrint a list of the referenced assemblies for an assembly");
		Console.WriteLine ("\t--runtime-version\tPrint runtime version");
		Console.WriteLine ("\t--search,-s,-k\t\tSearch through all known namespaces");
	}
}

