#!/usr/bin/env bash

cat >$0.cs <<EOF

using System;
using System.Collections.Generic;
using System.IO;

class GenSource {
	static List<string> filelist;
	static List<string> list;
	static List<string> excludelist;

	static int Main (string [] args)
	{
		string includefile = null;
		string excludefile = null;

		if (args.Length == 0) {
			Console.Error.WriteLine ("No arguments");
			return 1;
		}

		if (args.Length > 0)
			includefile = args [0];

		if (args.Length > 1)
			excludefile = args [1];

		list = new List<string> ();
		filelist = new List<string> ();
		Parse (excludefile);
		if (list.Count != 0) {
			excludelist = list;
			list = new List<string> ();
		}

		Parse (includefile);
		foreach (string line in list)
			Console.WriteLine (line);
		return 0;
	}

	static void Parse (string onelist)
	{
		if (string.IsNullOrEmpty (onelist))
			return;

		if (!File.Exists (onelist))
			return;

		if (filelist.Contains (onelist))
			return;

		filelist.Add (onelist);

		foreach (string l in File.ReadAllLines (onelist)) {
			string line = l.Replace (" ", "").Replace ("\t", "").Replace ('/', Path.DirectorySeparatorChar);
			if (!line.StartsWith ("#")) {
				if (excludelist != null && excludelist.Contains (line))
					continue;

				if (string.IsNullOrEmpty (line))
					continue;
				
				string dir = Path.GetDirectoryName (line);
				string name = Path.GetFileName (line);
				string [] expanded = Directory.GetFiles (Path.Combine (Environment.CurrentDirectory, dir), name);
				foreach (string file in expanded) {
					if (!list.Contains (file)) {
						list.Add (file);
					}
				}
			} else {
				Parse (line.Substring ("#include".Length));
			}
		}
	}
}

EOF

# check if $0.exe exists, or if $0 is newer than $0.exe
if test ! -e $0.exe -o $0 -nt $0.exe; then
	# echo compiling...
	gmcs $0.cs -target:exe -out:$0.exe -debug+
fi
mono --debug $0.exe "$@" | sort
# leave exe for next time the build requires us to avoid recompiling
