//
// ConsoleOutput.cs: 
//   Finds types and (optionally) shows reflection information about 
//   the types.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//
// Permission is hereby granted, free of charge, to any           
// person obtaining a copy of this software and associated        
// documentation files (the "Software"), to deal in the           
// Software without restriction, including without limitation     
// the rights to use, copy, modify, merge, publish,               
// distribute, sublicense, and/or sell copies of the Software,    
// and to permit persons to whom the Software is furnished to     
// do so, subject to the following conditions:                    
//                                                                 
// The above copyright notice and this permission notice          
// shall be included in all copies or substantial portions        
// of the Software.                                               
//                                                                 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY      
// KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO         
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A               
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL      
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,      
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,  
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION       
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

// #define TRACE

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class ConsoleOutput {

		private static void TraceStringArray (string message, IEnumerable contents)
		{
			Trace.WriteLine (message);
			foreach (string s in contents) {
				Trace.WriteLine ("  " + s);
			}
		}

		private static void PrintVersion ()
		{
			Console.WriteLine ("type-reflector 0.2");
			Console.WriteLine ("Written by Jonathan Pryor.");
			Console.WriteLine ();
			Console.WriteLine ("Copyright (C) 2002 Jonathan Pryor.");
		}

		private static void InitFactory ()
		{
			TypeDisplayerFactory.Add ("reflection", typeof(ReflectionTypeDisplayer));
			TypeDisplayerFactory.Add ("explicit", typeof(ExplicitTypeDisplayer));
			TypeDisplayerFactory.Add ("c#", typeof(CSharpTypeDisplayer));
		}

		public static void Main (string[] args)
		{
			InitFactory ();

			TypeReflectorOptions options = new TypeReflectorOptions ();

			try {
				options.ParseOptions (args);
			} catch (Exception e) {
				Console.WriteLine (e.Message);
				Console.WriteLine ("See `{0} --help' for more information", ProgramOptions.ProgramName);
				// Console.WriteLine ("** Full Message continues:\n" + e);
				return;
			}

			if (options.FoundHelp) {
				Console.WriteLine (options.OptionsHelp);
				return;
			}

			if (options.DefaultAssemblies) {
				Console.WriteLine ("The default search assemblies are:");
				foreach (string s in TypeReflectorOptions.GetDefaultAssemblies ()) {
					Console.WriteLine ("  {0}", s);
				}
				return;
			}

			if (options.Version) {
				PrintVersion ();
				return;
			}

			if (options.Types.Count == 0) {
				Console.WriteLine ("No types specified.");
				Console.WriteLine ("See `{0} --help' for more information", ProgramOptions.ProgramName);
				return;
			}

			TraceStringArray ("Search Assemblies: ", options.Assemblies);
			TraceStringArray ("Search for Types: ", options.Types);

			TypeLoader loader = new TypeLoader (options.Assemblies);
			loader.MatchBase = options.MatchBase;
			loader.MatchFullName = options.MatchFullName;
			loader.MatchClassName = options.MatchClassName;
			loader.MatchNamespace = options.MatchNamespace;
			loader.MatchMethodReturnType = options.MatchReturnType;

			TypeDisplayer p = TypeDisplayerFactory.Create (
					options.Output,
					Console.Out);

			// ((IndentingTypeDisplayer)p).SetWriter (Console.Out);
			p.VerboseOutput = options.VerboseOutput;
			p.ShowBase = options.ShowBase;
			p.ShowConstructors = options.ShowConstructors;
			p.ShowEvents = options.ShowEvents;
			p.ShowFields = options.ShowFields;
			p.ShowInterfaces = options.ShowInterfaces;
			p.ShowMethods = options.ShowMethods;
			p.ShowProperties = options.ShowProperties;
			p.ShowTypeProperties = options.ShowTypeProperties;
			p.ShowInheritedMembers = options.ShowInheritedMembers;
			p.ShowNonPublic = options.ShowNonPublic;
			p.ShowMonoBroken = options.ShowMonoBroken;
			p.FlattenHierarchy = options.FlattenHierarchy;
			p.MaxDepth = options.MaxDepth;

			foreach (string t in options.Types) {
				try {
					ICollection typesFound = loader.LoadTypes (t);
					if (typesFound.Count > 0)
						foreach (Type type in loader.LoadTypes(t))
							p.Parse (type);
					else
						Console.WriteLine ("Unable to find type `{0}'.", t);
				} catch (Exception e) {
					Console.WriteLine ("Unable to display type `{0}': {1}.", t, e.ToString());
				}
			}
		}
	}
}

