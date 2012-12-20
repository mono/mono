using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

#if LEGACY_MODE

namespace MonkeyDoc
{
	public partial class Node
	{
		[Obsolete ("Use `Tree' instead of 'tree'")]
		public Tree tree {
			get {
				return this.Tree;
			}
		}
	}
}

#endif
