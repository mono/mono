//
// tree.cs: keeps a tree representation of the generated code
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

namespace Mono.CSharp
{

	public interface ITreeDump {
		int  Dump (Tree tree, StreamWriter output);
		void ParseOptions (string options);
	}

	// <summary>
	//   
	//   We store here all the toplevel types that we have parsed,
	//   this is the root of all information we have parsed.
	// 
	// </summary>
	
	public class Tree {
		TypeContainer root_types;

		public Tree ()
		{
			root_types = new RootTypes ();
		}

		public void RecordDecl (Namespace ns, MemberName name, DeclSpace ds)
		{
			if (ds.Parent == root_types)
				ns.AddDeclSpace (name.Basename, ds);
		}
		
		//
		// FIXME: Why are we using Types?
		//
                public TypeContainer Types {
                        get { return root_types; }
                }
	}

	public sealed class RootTypes : TypeContainer
	{
		public RootTypes ()
			: base (null, null, MemberName.Null, null, Kind.Root)
		{
			types = new ArrayList ();
		}

		public override bool IsClsComplianceRequired ()
		{
			return true;
		}

		public override bool GetClsCompliantAttributeValue ()
		{
			return CodeGen.Assembly.IsClsCompliant;
		}

		public override string GetSignatureForError ()
		{
			return "";
		}

		protected override bool AddToTypeContainer (DeclSpace ds)
		{
			return AddToContainer (ds, ds.Name);
		}

		public override TypeContainer AddPartial (TypeContainer nextPart)
		{
			return AddPartial (nextPart, nextPart.Name);
		}

	}
}
