using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

namespace Monodoc
{
	public partial class Tree
	{
		[Obsolete ("Proxy to RootNode")]
		public List<Node> Nodes {
			get {
				return RootNode.Nodes;
			}
		}

		[Obsolete ("Proxy to RootNode")]
		public void Sort ()
		{
			RootNode.Sort ();
		}
	}
}
