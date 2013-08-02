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
	public class CodeArrayItem: CodeValueReference
	{
		CodeExpression array;
		CodeExpression index;
		
		public CodeArrayItem (CodeExpression array, CodeExpression index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index == null)
				throw new ArgumentNullException ("index");
			this.array = array;
			this.index = index;		
		}
		
		public override void Generate (ILGenerator gen)
		{
			array.Generate (gen);
			index.Generate (gen);
			
			Type t = array.GetResultType().GetElementType();
			if (t.IsEnum && t != typeof(Enum)) t = t.UnderlyingSystemType;
			
			switch (Type.GetTypeCode (t))
			{
				case TypeCode.Byte:
					gen.Emit (OpCodes.Ldelem_U1);
					break;

				case TypeCode.Double:
					gen.Emit (OpCodes.Ldelem_R8);
					break;
					
				case TypeCode.Int16:
					gen.Emit (OpCodes.Ldelem_I2);
					break;
					
				case TypeCode.UInt32:
					gen.Emit (OpCodes.Ldelem_U4);
					break;
					
				case TypeCode.Int32:
					gen.Emit (OpCodes.Ldelem_I4);
					break;
					
				case TypeCode.UInt64:
				case TypeCode.Int64:
					gen.Emit (OpCodes.Ldelem_I8);
					break;
					
				case TypeCode.SByte:
					gen.Emit (OpCodes.Ldelem_I1);
					break;
					
				case TypeCode.Single:
					gen.Emit (OpCodes.Ldelem_R4);
					break;
					
				case TypeCode.UInt16:
					gen.Emit (OpCodes.Ldelem_U2);
					break;
					
				default:
					if (t.IsValueType) {
						gen.Emit (OpCodes.Ldelema, t);
						CodeGenerationHelper.LoadFromPtr (gen, t);
					}
					else
						gen.Emit (OpCodes.Ldelem_Ref);
					break;
			}		
		}
		
		public override void GenerateSet (ILGenerator gen, CodeExpression value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			Type t = array.GetResultType().GetElementType();
			if (t.IsEnum && t != typeof(Enum)) t = t.UnderlyingSystemType;
			
			array.Generate (gen);
			index.Generate (gen);
			
			switch (Type.GetTypeCode (t))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
					value.Generate (gen);
					gen.Emit (OpCodes.Stelem_I1);
					break;

				case TypeCode.Double:
					value.Generate (gen);
					gen.Emit (OpCodes.Stelem_R8);
					break;
					
				case TypeCode.UInt16:
				case TypeCode.Int16:
					value.Generate (gen);
					gen.Emit (OpCodes.Stelem_I2);
					break;
					
				case TypeCode.UInt32:
				case TypeCode.Int32:
					value.Generate (gen);
					gen.Emit (OpCodes.Stelem_I4);
					break;
					
				case TypeCode.UInt64:
				case TypeCode.Int64:
					value.Generate (gen);
					gen.Emit (OpCodes.Stelem_I8);
					break;
					
				case TypeCode.Single:
					value.Generate (gen);
					gen.Emit (OpCodes.Stelem_R4);
					break;
					
				default:
					if (t.IsValueType) {
						gen.Emit (OpCodes.Ldelema, t);
						value.Generate (gen);
						gen.Emit (OpCodes.Stobj, t);
					}
					else {
						value.Generate (gen);
						gen.Emit (OpCodes.Stelem_Ref);
					}
					break;
			}				
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			array.PrintCode (cp);
			cp.Write ("[");
			index.PrintCode (cp);
			cp.Write ("]");
		}
		
		public override Type GetResultType ()
		{
			return array.GetResultType().GetElementType();
		}
	}
}
#endif
