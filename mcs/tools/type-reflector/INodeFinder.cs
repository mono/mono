//
// INodeFinder.cs: Policy interface to find nodes for a type.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public interface INodeFinder {

		NodeInfoCollection GetChildren (NodeInfo root);
	}
}

