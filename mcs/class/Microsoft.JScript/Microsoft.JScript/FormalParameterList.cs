//
// FormalParameterList.cs: A list of identifiers.
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System.Collections;
using System.Text;
using System;

namespace Microsoft.JScript {

	internal class FormalParam {
		internal string id;
		internal string type_annot;

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
	}
			
	public class FormalParameterList : AST {

		internal ArrayList ids;

		public FormalParameterList ()
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
			int i, size = ids.Count;

			for (i = 0; i < size; i++) {
				f = (FormalParam) ids [i];
				context.Enter (f.id, f);
			}

			return true;
		} 

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
