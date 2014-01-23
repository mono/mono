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

#if !FULL_AOT_RUNTIME
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeVariableReference: CodeValueReference
	{
		LocalBuilder localBuilder;
		Type type;
		string name;
		
		public CodeVariableReference (Type type, string name)
		{
			this.type = type;
			this.name = name;		
		}
		
		public Type Type
		{
			get { return type; }
		}
		
		public string Name
		{
			get { return name; }
		}
		
		internal LocalBuilder LocalBuilder
		{
			get { return localBuilder; }
			set { localBuilder = value; }
		}
		
		public override void Generate (ILGenerator gen)
		{
			gen.Emit (OpCodes.Ldloc, localBuilder);
		}
		
		public override void GenerateSet (ILGenerator gen, CodeExpression value)
		{
			value.Generate (gen);
			CodeGenerationHelper.GenerateSafeConversion (gen, type, value.GetResultType ());
			gen.Emit (OpCodes.Stloc, localBuilder);
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			cp.Write (name);
		}
		
		public override Type GetResultType ()
		{
			return type;
		}
	}
}
#endif
