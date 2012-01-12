// Author:  John Luke  <jluke@cfl.rr.com>

using System;
using Monodoc;

namespace Monodoc
{
	public class Mod
	{
		static void Main (string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine ("Usage: mod.exe Url");
				return;
			}
			bool index =  (args.Length == 2);
			

			RootTree help_tree = RootTree.LoadTree ();
			if (index){
				Console.WriteLine ("Building index");
				RootTree.MakeIndex ();
				return;
			}
			Node n;
			Console.WriteLine (help_tree.RenderUrl (args[0], out n));
		}
	}
}
