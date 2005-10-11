//
// Enum.cs: AST representation of a enum_statement.
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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

using System;
using System.Text;
using System.Collections;

namespace Microsoft.JScript {

	internal class Enum : AST {

		private ArrayList modifiers;
		private string name;
		private string type;		
		private ArrayList pairs;

		internal Enum (AST parent, Location location)
			: base (parent, location)
		{
			Modifiers = new ArrayList ();
			Pairs = new ArrayList ();
		}


		internal ArrayList Modifiers {
			get { return modifiers; }
			set { modifiers = value; }
		}

		internal string Name {
			get { return name; }
			set { name = value; }
		}


		internal string Type {
			get { return type; }
			set { type = value; }
		}


		internal ArrayList Pairs {
			get { return pairs; }
			set { pairs = value; }
		}


		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			foreach (string modifier in Modifiers)
				sb.Append (modifier + "\n");

			sb.Append (name + "\n");
			sb.Append (type + "\n");
			
			foreach (BinaryOp bop in Pairs)
				sb.Append (bop.ToString () + "\n");

			return sb.ToString ();
		}

		internal override bool Resolve (Environment env)
		{
			throw new NotImplementedException ();
		}					

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
