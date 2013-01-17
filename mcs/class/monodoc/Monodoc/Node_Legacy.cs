using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

#if LEGACY_MODE

namespace Monodoc
{
	public partial class Node
	{
		[Obsolete ("Use `Tree' instead of 'tree'")]
		public Tree tree {
			get {
				return this.Tree;
			}
		}

		[Obsolete ("Use TreeDumper")]
		public static void PrintTree (Tree t)
		{
			TreeDumper.PrintTree (t.RootNode);
		}

		
	}
}

#endif
