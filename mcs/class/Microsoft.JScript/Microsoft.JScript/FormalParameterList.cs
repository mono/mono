//
// FormalParameterList.cs: A list of identifiers.
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System.Reflection.Emit;
using System.Collections;
using System.Text;
using System;

namespace Microsoft.JScript {

	internal class FormalParam : AST {
		internal string id;
		internal string type_annot;
		internal byte pos;

		//
		// FIXME: 
		//	Must perform semantic analysis on type_annot,
		//	and assign that type value to 'type' if valid.
		//
		internal Type type = typeof (Object);

		internal FormalParam (string id, string type_annot)
		{
			this.id = id;
			this.type_annot = type_annot;
		}

		public override string ToString ()
		{
			return id + " " + type_annot;
		}

		internal override void Emit (EmitContext ec)
		{
		}
	
		internal override bool Resolve (IdentificationTable context)
		{
			context.Enter (id, this);
			return true;
		}
	}
			
	public class FormalParameterList : AST {

		internal ArrayList ids;

		internal FormalParameterList ()
		{
			ids = new ArrayList ();
		}

		internal void Add (string id, string type_annot)
		{
			FormalParam p = new FormalParam (id, type_annot);	
			ids.Add (p);	
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
		
			foreach (FormalParam f in ids)
				sb.Append (f.ToString () + " ");
		
			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			FormalParam f;
			int i, n = ids.Count;

			for (i = 0; i < n; i++) {
				f = (FormalParam) ids [i];
				f.pos = (byte) (i + 2);
				f.Resolve (context);
			}
			return true;
		} 

		internal override void Emit (EmitContext ec)
		{
			int n = ids.Count;
			ILGenerator ig = ec.ig;

			ig.Emit (OpCodes.Ldc_I4, n);
			ig.Emit (OpCodes.Newarr, typeof (string));

			for (int i = 0; i < n; i++) {
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Ldc_I4, i);
				ig.Emit (OpCodes.Ldstr, ((FormalParam) ids [i]).id);
				ig.Emit (OpCodes.Stelem_Ref);
			}
		}

		internal int size {
			get { return ids.Count; }
		}
	}
}
