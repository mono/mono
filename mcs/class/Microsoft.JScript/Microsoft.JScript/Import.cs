//
// Import.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
// Copyright (C) 2005 , Novell Inc (http://novell.com)
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
using Microsoft.JScript.Vsa;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class Import : AST {

		string name;

		internal Import (AST parent, string name, Location location)
			: base (parent, location)
		{
			this.name = name;
		}

		public static void JScriptImport (string name, VsaEngine engine)
		{
		}

		internal override bool Resolve (Environment env)
		{
			if (InFunction) {
				string err = location.SourceName + "(" + location.LineNumber + ",0) : " +
				"error JS1229: The import statement is not valid in this context";
				throw new Exception (err);
			}
			return Namespace.IsNamespace (name);
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			ig.Emit (OpCodes.Ldstr, name);
			CodeGenerator.load_engine (false, ig);
			ig.Emit (OpCodes.Call, GetType ().GetMethod ("JScriptImport"));
		}
	}
}
