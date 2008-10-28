using System;
using System.Collections;

namespace Monodoc {
class Dump {

	static void Usage ()
	{
		Console.WriteLine ("Usage is: dump file.tree");
	}
	
	static int Main (string [] args)
	{
		int argc = args.Length;
		Tree t = null;
		
		for (int i = 0; i < argc; i++){
			string arg = args [i];
			
			switch (arg){
				
			default:
				if (t == null)
					t = new Tree (null, arg);
				break;
			}
		}

		if (t != null)
			Node.PrintTree (t);

		return 0;
	}
}
}
