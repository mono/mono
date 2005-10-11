//
// TypeOf.cs:
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

namespace Microsoft.JScript {

	public sealed class Typeof : UnaryOp {

		internal Typeof ()
			: base (null, null)
		{
		}

		public static string JScriptTypeof (object value)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);

			if (Convert.IsNumberTypeCode (tc))
				return "number";
			
			switch (tc) {
			case TypeCode.String:
				return "string";

			case TypeCode.Object:
			case TypeCode.DBNull:
				if (value is ScriptFunction || value is RegExpObject)
					return "function";

				return "object";

			case TypeCode.Empty:
				return "undefined";

			case TypeCode.Boolean:
				return "boolean";
				
			default:
				Console.WriteLine ("TypeOf, tc = {0}", tc);
				break;
			}
			throw new NotImplementedException ();
		}

		internal override bool Resolve (Environment env)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
