// CorCompare
//
// Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak

using System;
using System.Reflection;
using System.Collections;
using System.Text;
using System.IO;

namespace Mono.Util
{
	class CorCompare {
		// these types are in mono corlib, but not in the dll we are going to examine.
		static string[] ghostTypes = {"System.Object", "System.ValueType", "System.Delegate"};

		public static void Main(string[] args) {
			if (args.Length < 1) {
				Console.WriteLine ("Usage: CorCompare assembly_to_compare");
				return;
			}
			Assembly monoAsmbl = null;
			try{
				monoAsmbl = Assembly.LoadFrom(args[0]);
			}
			catch(FileNotFoundException)
			{
				Console.WriteLine("Could not find corlib file: {0}", args[0]);
				return;
			}

			Assembly msAsmbl = Assembly.GetAssembly(typeof (System.Object));
			Type[] mscorlibTypes = msAsmbl.GetTypes();
			Type[] monocorlibTypes;
			ArrayList TypesList = new ArrayList();

			// load the classes we know should exist
			foreach (string name in ghostTypes){
				TypesList.Add(name);
			}

			// GetTypes() doesn't seem to like loading our dll, so use jujitsu
			try {
				monocorlibTypes = monoAsmbl.GetTypes();
			}
			catch(ReflectionTypeLoadException e) {
				// the exception holds all the types in the dll anyway
				// some are in Types and some are in the LoaderExceptions array
				 monocorlibTypes = e.Types;
				foreach(TypeLoadException loadException in e.LoaderExceptions){
					TypesList.Add(loadException.TypeName);
				}
				//Console.WriteLine(e.ToString());
				//return;
			}

			// whether GetTypes() worked or not, we will have _some_ types here
			foreach(Type subt in monocorlibTypes){
				if (null != subt) {
					TypesList.Add(subt.FullName);
				}
			}

			// going to use BinarySearch, so sort first
			TypesList.Sort();
			
			ArrayList MissingTypes = new ArrayList();
			bool foundit = false;

			// make list of ms types not in mono
			foreach(Type t in mscorlibTypes) {
				if (t.IsPublic) {
					foundit = (TypesList.BinarySearch(t.FullName) >= 0);
					if (!foundit){
						MissingTypes.Add(t.FullName);
					}
				}
			}

			// sort for easy reading
			MissingTypes.Sort();
			Console.WriteLine(XMLUtil.ToXML(MissingTypes, "Type", "MissingTypes"));
		}
	}

}
