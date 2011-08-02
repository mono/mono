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
using Mono.CodeGeneration;

namespace Mono.CodeGeneration
{
	public class CodeCast: CodeExpression
	{
		Type type; 
		CodeExpression exp;
		
		public CodeCast (Type type, CodeExpression exp)
		{
			this.type = type;		
			this.exp = exp;
		}
		
		public override void Generate (ILGenerator gen)
		{
			exp.Generate (gen);
			
			Type typeObj = exp.GetResultType ();

			if (type.IsAssignableFrom (typeObj)) {
				if (typeObj.IsValueType)
					gen.Emit (OpCodes.Box, typeObj);
				return;
			}
			else if (type.IsValueType && typeObj == typeof(object)) {
				// Unbox
				gen.Emit (OpCodes.Unbox, type);
				CodeGenerationHelper.LoadFromPtr (gen, type);
				return;
			}
			else if (typeObj.IsAssignableFrom (type)) {
				// Sub s = (Sub)base
				gen.Emit (OpCodes.Castclass, type);
				return;
			}
			else if (CodeGenerationHelper.IsNumber (type) && CodeGenerationHelper.IsNumber (typeObj)) {
				switch (Type.GetTypeCode (type))
				{
					case TypeCode.Byte:
						gen.Emit (OpCodes.Conv_U1);
						return;
					case TypeCode.Double:
						gen.Emit (OpCodes.Conv_R8);
						return;
					case TypeCode.Int16:
						gen.Emit (OpCodes.Conv_I2);
						return;
					case TypeCode.Int32:
						gen.Emit (OpCodes.Conv_I4);
						return;
					case TypeCode.Int64:
						gen.Emit (OpCodes.Conv_I8);
						return;
					case TypeCode.SByte:
						gen.Emit (OpCodes.Conv_I1);
						return;
					case TypeCode.Single:
						gen.Emit (OpCodes.Conv_R4);
						return;
					case TypeCode.UInt16:
						gen.Emit (OpCodes.Conv_U2);
						return;
					case TypeCode.UInt32:
						gen.Emit (OpCodes.Conv_U4);
						return;
					case TypeCode.UInt64:
						gen.Emit (OpCodes.Conv_U8);
						return;
				}
			}
			
			MethodInfo imp = type.GetMethod ("op_Implicit", new Type[] { typeObj });
			if (imp != null) {
				gen.Emit (OpCodes.Call, imp);
				return;
			}
			
			foreach (MethodInfo m in typeObj.GetMember ("op_Explicit"))
				if (m.ReturnType == type) {
					gen.Emit (OpCodes.Call, m);
					return;
				}

			throw new InvalidOperationException ("Can't cast from " + typeObj + " to " + type);
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			Type typeObj = exp.GetResultType ();
			if (type.IsAssignableFrom (typeObj)) {
				exp.PrintCode (cp);
				return;
			}
			
			cp.Write ("((" + type.FullName + ") ");
			exp.PrintCode (cp);
			cp.Write (")");
		}
		
		public override Type GetResultType ()
		{
			return type;
		}
	}
}
#endif
