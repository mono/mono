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
	public class CodeDecrement: CodeValueReference
	{
		CodeValueReference exp;
		
		public CodeDecrement (CodeValueReference exp)
		{
			this.exp = exp;
		}
		
		public override void Generate (ILGenerator gen)
		{
			exp.GenerateSet (gen, new CodeSubstractOne (exp));
			exp.Generate (gen);
		}
		
		public override void GenerateSet (ILGenerator gen, CodeExpression value)
		{
			exp.GenerateSet (gen, value);
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			exp.PrintCode (cp);
			cp.Write ("--");
		}
		
		public override Type GetResultType ()
		{
			return exp.GetResultType();
		}
	}
	
	public class CodeSubstractOne: CodeExpression
	{
		CodeExpression exp;
		MethodInfo decMet;
		
		public CodeSubstractOne (CodeExpression exp)
		{
			this.exp = exp;
			if (!exp.IsNumber) {
				decMet = exp.GetResultType ().GetMethod ("op_Decrement");
				if (decMet == null)
					throw new InvalidOperationException ("Operator '--' cannot be applied to operand of type '" + exp.GetResultType().FullName + "'");
			}
		}
		
		public override void Generate (ILGenerator gen)
		{
			if (decMet != null) {
				CodeGenerationHelper.GenerateMethodCall (gen, null, decMet, exp);
				return;
			}
			
			exp.Generate (gen);
			Type t = exp.GetResultType ();
			switch (Type.GetTypeCode (t))
			{
				case TypeCode.Byte:
					gen.Emit (OpCodes.Ldc_I4_1);
					gen.Emit (OpCodes.Sub);
					gen.Emit (OpCodes.Conv_U1);
					break;

				case TypeCode.Double:
					gen.Emit (OpCodes.Ldc_R8, 1);
					gen.Emit (OpCodes.Sub);
					break;
					
				case TypeCode.Int16:
					gen.Emit (OpCodes.Ldc_I4_1);
					gen.Emit (OpCodes.Sub);
					gen.Emit (OpCodes.Conv_I2);
					break;
					
				case TypeCode.UInt32:
				case TypeCode.Int32:
					gen.Emit (OpCodes.Ldc_I4_1);
					gen.Emit (OpCodes.Sub);
					break;
					
				case TypeCode.UInt64:
				case TypeCode.Int64:
					gen.Emit (OpCodes.Ldc_I4_1);
					gen.Emit (OpCodes.Conv_U8);
					gen.Emit (OpCodes.Sub);
					break;
					
				case TypeCode.SByte:
					gen.Emit (OpCodes.Ldc_I4_1);
					gen.Emit (OpCodes.Sub);
					gen.Emit (OpCodes.Conv_I1);
					break;
					
				case TypeCode.Single:
					gen.Emit (OpCodes.Ldc_R4, 1);
					gen.Emit (OpCodes.Sub);
					break;
					
				case TypeCode.UInt16:
					gen.Emit (OpCodes.Ldc_I4_1);
					gen.Emit (OpCodes.Sub);
					gen.Emit (OpCodes.Conv_U2);
					break;
			}
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			exp.PrintCode (cp);
			cp.Write ("--");
		}
		
		public override Type GetResultType ()
		{
			return exp.GetResultType();
		}
	}
}
#endif
