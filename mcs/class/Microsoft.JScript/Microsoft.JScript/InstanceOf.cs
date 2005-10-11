//
// InstanceOf.cs:
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

	public sealed class Instanceof : BinaryOp {

		internal Instanceof ()
			: base (null, null, null, (JSToken) 0, null)
		{
		}

		public static bool JScriptInstanceof (object v1, object v2)
		{
			if (v2 is ArrayConstructor)
				return v1 is ArrayObject;
			else if (v2 is BooleanConstructor)
				return v1 is BooleanObject;
			else if (v2 is DateConstructor)
				return v1 is DateObject;
			else if (v2 is EnumeratorConstructor)
				return v1 is EnumeratorObject;
			else if (v2 is ErrorConstructor)
				return v1 is ErrorObject;
			else if (v2 is FunctionConstructor)
				return v1 is FunctionObject;
			else if (v2 is NumberConstructor)
				return v1 is NumberObject;
			else if (v2 is ObjectConstructor)
				return v1 is JSObject;
			else if (v2 is RegExpConstructor)
				return v1 is RegExpObject;
			else if (v2 is StringConstructor)
				return v1 is StringObject;
			else if (v2 is ScriptFunction)
				return false;
			else
				throw new JScriptException (JSError.TypeMismatch);
		}

		internal override bool Resolve (Environment env)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
