//
// mono-find-provides.cs - Prints out an assembly's name and version
//
// Author: Duncan Mak (duncan@ximian.com)
// 
// 2004 Copyright Novell Inc.
//

using System;
using System.Reflection;

namespace Mono {
class FindProvides {

        static void Main (string [] args)
        {
                if (args.Length == 0) {
                        string s = Console.ReadLine ();

                        while (s != null) {
                                PrintProvides (s);
                                s = Console.ReadLine ();
                        }

                } else {
                        foreach (string s in args)
                                PrintProvides (s);
                }
        }

        static void PrintProvides (string s)
        {
                try {
                        Assembly a = Assembly.LoadFrom (s);
                        AssemblyName an = a.GetName ();

                        // hack to work around the issue with a 2.0 corlib
			if (s.Trim ().EndsWith ("2.0/mscorlib.dll"))
                                Console.WriteLine  ("mono({0}) = {1}", "mscorlib", "2.0.3600.0");
                        else
                                Console.WriteLine ("mono({0}) = {1}", an.Name, an.Version);

                } catch {}
        }
}
}
