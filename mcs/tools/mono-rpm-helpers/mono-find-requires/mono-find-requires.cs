//
// mono-find-requires.cs - Prints out referenced assembles
//
// Author: Duncan Mak (duncan@ximian.com)
// 
// 2004 Copyright Novell Inc.
//

using System;
using System.Reflection;

namespace Mono {
class FindRequires {

        static void Main (string [] args)
        {
                if (args.Length == 0) {
                        string s = Console.ReadLine ();

                        while (s != null) {
                                PrintRequires (s);
                                s = Console.ReadLine ();
                        }

                } else {
                        foreach (string s in args)
                                PrintRequires (s);
                }
        }

        static void PrintRequires (string s)
        { 
                try {
                        Assembly a = Assembly.LoadFrom (s);
                
			foreach (AssemblyName an in a.GetReferencedAssemblies ())
				Console.WriteLine ("mono({0}) = {1}", an.Name, an.Version);

                } catch {}
        }
}
}
