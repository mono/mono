//
// FormalParameterList.cs: A list of identifiers.
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

		internal FormalParam (string id, string type_annot, Location location)
			: base (null, location)
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

		internal override bool Resolve (Environment env)
		{
			env.Enter (String.Empty, Symbol.CreateSymbol (id), this);
			return true;
		}
	}
			
	internal class FormalParameterList : AST {

		internal ArrayList ids;

		internal FormalParameterList (Location location)
			: base (null, location)
		{
			ids = new ArrayList ();
		}

		internal void Add (string id, string type_annot, Location location)
		{
			FormalParam p = new FormalParam (id, type_annot, location);
			ids.Add (p);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
		
			foreach (FormalParam f in ids)
				sb.Append (f.ToString () + " ");
		
			return sb.ToString ();
		}

		internal override bool Resolve (Environment env)
		{
			FormalParam f;
			int i, n = ids.Count;

			for (i = 0; i < n; i++) {
				f = (FormalParam) ids [i];
				f.pos = (byte) (i + 2);
				f.Resolve (env);
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

		internal FormalParam get_element (int i)
		{
			if (i >= 0 && i < size)
				return (FormalParam) ids [i];
			else
				throw new IndexOutOfRangeException ();
		}
	}
}
