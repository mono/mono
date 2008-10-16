//
// The assembler: Help compiler.
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// (C) 2003 Ximian, Inc.
//
using System;
using System.Xml;
using System.Collections;
using Monodoc;

namespace Mono.Documentation {
	
public class Assembler {
	static void Usage ()
	{
		Console.WriteLine ("Usage is: assembler [--out] [--ecma|xhtml|hb|man|simple|error|ecmaspec] [dir1 dir2");
		Console.WriteLine ("   --ecma   To process an tree containing ECMA XML documents");
		Console.WriteLine ("   --xhtml  To process an tree containing XHTML documents");
	}
	
	public static int Main (string [] args)
	{
		string output = "tree";
		HelpSource hs;
		ArrayList list = new ArrayList ();
		EcmaProvider ecma = null;
		bool sort = false;
		
		int argc = args.Length;
		for (int i = 0; i < argc; i++){
			string arg = args [i];
			
			switch (arg){
			case "-o": case "--out":
				if (i < argc)
					output = args [++i];
				else {
					Usage ();
					return 1;
				} 
				break;

			case "--ecma":
				if (i < argc){
					if (ecma == null) {
						ecma = new EcmaProvider ();
						list.Add (ecma);
						sort = true;
					}
					ecma.AddDirectory (args [++i]);
				} else {
					Usage ();
					return 1;
				}
				break;

			case "--xhtml":
			case "--hb":
				if (i < argc){
					Provider populator = new XhtmlProvider (args [++i]);

					list.Add (populator);
				} else {
					Usage ();
					return 1;
				}
				break;

			case "--man":
				if (i < argc){
					int countfiles = args.Length - ++i;
					string[] xmlfiles = new String[countfiles];
					for (int a = 0;a< countfiles;a++) {
						xmlfiles[a] = args [i];
						i++;
					}
					Provider populator = new ManProvider (xmlfiles);

					list.Add (populator);
				} else {
					Usage ();
					return 1;
				}
				break;

		case "--simple":
				if (i < argc){
					Provider populator = new SimpleProvider (args [++i]);

					list.Add (populator);
				} else {
					Usage ();
					return 1;
				}
				break;
			case "--error":
				if (i < argc){
					Provider populator = new ErrorProvider (args [++i]);

					list.Add (populator);
				} else {
					Usage ();
					return 1;
				}
				break;
			case "--ecmaspec":
				if (i < argc){
					Provider populator = new EcmaSpecProvider (args [++i]);

					list.Add (populator);
				} else {
					Usage ();
					return 1;
				}
				break;

			case "--addins":
				if (i < argc){
					Provider populator = new AddinsProvider (args [++i]);
					list.Add (populator);
				} else {
					Usage ();
					return 1;
				}
				break;
			
			default:
				Usage ();
				return 1;
			}
		}

		hs = new HelpSource (output, true);

		foreach (Provider p in list){
			p.PopulateTree (hs.Tree);
		}

		if (sort)
			hs.Tree.Sort ();
			      
		//
		// Flushes the EcmaProvider
		//
		foreach (Provider p in list)
			p.CloseTree (hs, hs.Tree);

		hs.Save ();
		return 0;
	}
}

}
