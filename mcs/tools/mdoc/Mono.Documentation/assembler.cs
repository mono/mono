//
// The assembler: Help compiler.
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// (C) 2003 Ximian, Inc.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Monodoc;

namespace Mono.Documentation {
	
public class MDocAssembler {
	public static void Run (string output, Dictionary<string, List<string>> formatToFileMap)
	{
		HelpSource hs;
		List<Provider> list = new List<Provider> ();
		EcmaProvider ecma = null;
		bool sort = false;
		
		foreach (string format in formatToFileMap.Keys) {
			switch (format) {
			case "ecma":
				if (ecma == null) {
					ecma = new EcmaProvider ();
					list.Add (ecma);
					sort = true;
				}
				foreach (string dir in formatToFileMap [format])
					ecma.AddDirectory (dir);
				break;

			case "xhtml":
			case "hb":
				list.AddRange (formatToFileMap [format].Select (d => (Provider) new XhtmlProvider (d)));
				break;

			case "man":
				list.Add (new ManProvider (formatToFileMap [format].ToArray ()));
				break;

			case "simple":
				list.AddRange (formatToFileMap [format].Select (d => (Provider) new SimpleProvider (d)));
				break;

			case "error":
				list.AddRange (formatToFileMap [format].Select (d => (Provider) new ErrorProvider (d)));
				break;

			case "ecmaspec":
				list.AddRange (formatToFileMap [format].Select (d => (Provider) new EcmaSpecProvider (d)));
				break;

			case "addins":
				list.AddRange (formatToFileMap [format].Select (d => (Provider) new AddinsProvider (d)));
				break;
			}
		}

		hs = new HelpSource (output, true);

		foreach (Provider p in list) {
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
	}
}

}
