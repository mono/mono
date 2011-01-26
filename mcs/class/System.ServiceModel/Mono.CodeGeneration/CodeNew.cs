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
// Copyright (C) Lluis Sanchez Gual, 2004
//

#if !MONOTOUCH
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeNew: CodeExpression
	{
		object value;
		Type type;
		ConstructorInfo ctor;
		CodeExpression[] parameters;
		
		public CodeNew (Type type, params CodeExpression[] parameters)
		{
			this.type = type;
			Type[] ptypes = new Type [parameters.Length];
			for (int n=0; n<parameters.Length; n++)
				ptypes [n] = parameters[n].GetResultType ();
			ctor = type.GetConstructor (ptypes);
			if (ctor == null)
				throw new InvalidOperationException ("Constructor not found");
			this.parameters = parameters;
		}

		public override void Generate (ILGenerator gen)
		{
			foreach (CodeExpression exp in parameters)
				exp.Generate (gen);
			gen.Emit (OpCodes.Newobj, ctor);
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			cp.Write ("new " + type.Name + " (");
			for (int n=0; n<parameters.Length; n++) {
				if (n > 0) cp.Write (", ");
				parameters[n].PrintCode (cp);
			}
			cp.Write (")");
		}
		
		public override Type GetResultType ()
		{
			return type;
		}
	}
}
#endif
