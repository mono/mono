// IFaceDisco.cs
//
// Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak

using System;
using System.Reflection;
using System.Collections;
using System.IO;

namespace Mono.Util
{
	class IFaceDisco {
		public static void Main(string[] args) {
			Assembly					asm;
			Type[]						asmTypes;
			InterfaceMapping		map;
			Type[]						interfaces;
			ArrayList					TypesList					= new ArrayList();
			ArrayList					implementingTypes	= new ArrayList();
			string						asmFullPath				= null;
			string						ifaceToDiscover		= null;
			
			if (args.Length < 1 || args.Length > 3) {
				Usage();
				return;
			}

			for (int i = 0; i < args.Length; i++) {
				string arg = args[i];

				if (arg.StartsWith("-") && ((i + 1) < args.Length)) {
					if (arg == "--asm") {
						asmFullPath = args[++i];
					} else {
						Usage();
						return;
					}
				} else {
					// allow only one interface to discover
					if (ifaceToDiscover != null){
						Usage();
						return;
					}
					ifaceToDiscover = arg;
				}
			}

			// find the assembly
			if (null == asmFullPath){
				asm = Assembly.GetAssembly(typeof (System.Object));
			}
			else {
				try{
					asm = Assembly.LoadFrom(asmFullPath);
				}
				catch(Exception e){
					Console.WriteLine("Could not open assembly '{0}' for discovery. Error is: "+e.Message, asmFullPath);
					return;
				}
			}
			asmTypes = asm.GetTypes();

			// examine all the public types
			foreach(Type t in asmTypes) {
				if (t.IsPublic) {
					// find out which, if any, interfaces are "in" the type
					interfaces= t.GetInterfaces();
					if (null != interfaces){
						// look for the interface we want to discover
						foreach (Type iface in interfaces) {
							// this area seems to throw an exception sometimes, just ignore it
							try{
								if (iface.FullName.ToLower() == args[0].ToLower()) {
									// find out if this type is the one which "declares" the interface
									map = t.GetInterfaceMap(iface);
									if (map.TargetMethods[0].DeclaringType.FullName == t.FullName){
										// if so, then we found a class to report
										implementingTypes.Add(t.FullName);
									} // if
								}  // if
							}catch{}
						} // foreach
					} // if
				} // if
			} // foreach

			// sort the list to make it easier to find what you are looking for
			implementingTypes.Sort();
			Console.WriteLine(XMLUtil.ToXML(implementingTypes, "Type", "ImplementingTypes"));
		} // Main()

		private static void Usage() {
			Console.WriteLine (
				"Mono Interface Discovery Tool\n" +
				"usage: ifacedisco [--asm assembly] interface\n\n" +
				"  The full path to 'assembly' should be specified when using --asm.\n" +
				"  If 'assembly' is not specified, the assembly that contains System.Object will be used.\n" +
				"  Use the fully qualified form for 'interface', e.g. System.Runtime.Serialization.ISerializable\n"
				);
		} // Usage()

	} // class IFaceDisco
}  // namespace Mono.Util
