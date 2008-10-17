// Author:  John Luke  <jluke@cfl.rr.com>

using System;
using Monodoc;

namespace Monodoc
{
	public class Mod
	{
		static void Main (string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine ("Usage: mod.exe Url");
				return;
			}

			RootTree help_tree = RootTree.LoadTree ();
			Node n;
			Console.WriteLine (help_tree.RenderUrl (args[0], out n));
		}
	}
}
