//
// ConsoleTypeDisplayer.cs: 
//   Display types on the console.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

// #define TRACE

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class ConsoleTypeDisplayer : TypeDisplayer
	{
		private IndentingTextWriter writer = new IndentingTextWriter (Console.Out);

		// `ReflectionTypeDisplayer.PrintTypeProperties' is recursive, but refrains 
    // from printing duplicates.  Despite duplicate removal, the output for 
    // printing the Properties of System.Type is > 800K of text.
		//
		// 3 levels permits viewing Attribute values, but not the attributes of
		// those attribute values.
		//
		// For example, 3 levels permits:
		// 		class		System.Type                           {depth 0}
		// 			Properties:                                 {depth 1}
		// 				System.Reflection.MemberTypes MemberType  {depth 2}
		// 					- CanRead=True                          {depth 3}
		// 					- CanWrite=False                        {depth 3}
		// 					...
		private int maxDepth = 3;

		public override int MaxDepth {
			set {maxDepth = value;}
		}

		public override bool RequireTypes {
			get {return true;}
		}

		public ConsoleTypeDisplayer ()
		{
		}

		public override void Run ()
		{
			foreach (Assembly a in Assemblies) {
				writer.WriteLine ("Assembly: FullName='{0}'; Location='{1}'", 
						a.FullName, a.Location);
				using (Indenter i = Indent()) {
					foreach (string ns in Namespaces(a)) {
						writer.WriteLine ("Namespace: {0}", ns);
						using (Indenter i2 = Indent()) {
							foreach (Type type in Types (a, ns)) {
								Node root = new Node (Formatter, Finder);
										// new GroupingNodeFinder (f));
										// new ExplicitNodeFinder());
								// root.Extra = new NodeInfo (null, type, NodeTypes.Type);
								root.NodeInfo = new NodeInfo (null, type);
								ShowNode (root, writer, maxDepth);
								writer.WriteLine ();
							}
						}
					}
				}
			}
		}

		private static void ShowNode (Node root, IndentingTextWriter writer, int maxDepth)
		{
			writer.WriteLine (root.Description);
			if (maxDepth > 0) {
				using (Indenter i = new Indenter (writer)) {
					foreach (Node child in root.GetChildren()) {
						ShowNode (child, writer, maxDepth-1);
					}
				}
			}
		}

		private Indenter Indent ()
		{
			return new Indenter (writer);
		}
	}
}

