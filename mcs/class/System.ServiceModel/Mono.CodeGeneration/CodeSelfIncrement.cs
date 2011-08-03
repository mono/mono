// created on 28/08/2004 at 17:30

#if !MONOTOUCH
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeSelfIncrement: CodeExpression
	{
		CodeValueReference exp;
		
		public CodeSelfIncrement (CodeValueReference exp)
		{
			this.exp = exp;
			if (!exp.IsNumber)
				throw new InvalidOperationException ("Operator '++' cannot be applied to operand of type '" + exp.GetResultType().FullName + "'");
		}
		
		public override void Generate (ILGenerator gen)
		{
			exp.Generate (gen);
			Type t = exp.GetResultType ();
			switch (Type.GetTypeCode (t))
			{
				case TypeCode.Byte:
					gen.Emit (OpCodes.Ldc_I4_1);
					gen.Emit (OpCodes.Add);
					gen.Emit (OpCodes.Conv_U1);
					break;

				case TypeCode.Decimal:
					MethodInfo met = typeof(Decimal).GetMethod ("op_Increment");
					CodeGenerationHelper.GenerateMethodCall (gen, null, met, exp);
					break;
					
				case TypeCode.Double:
					gen.Emit (OpCodes.Ldc_R8, 1);
					gen.Emit (OpCodes.Add);
					break;
					
				case TypeCode.Int16:
					gen.Emit (OpCodes.Ldc_I4_1);
					gen.Emit (OpCodes.Add);
					gen.Emit (OpCodes.Conv_I2);
					break;
					
				case TypeCode.UInt32:
				case TypeCode.Int32:
					gen.Emit (OpCodes.Ldc_I4_1);
					gen.Emit (OpCodes.Add);
					break;
					
				case TypeCode.UInt64:
				case TypeCode.Int64:
					gen.Emit (OpCodes.Ldc_I4_1);
					gen.Emit (OpCodes.Add);
					gen.Emit (OpCodes.Conv_U8);
					break;
					
				case TypeCode.SByte:
					gen.Emit (OpCodes.Ldc_I4_1);
					gen.Emit (OpCodes.Add);
					gen.Emit (OpCodes.Conv_I1);
					break;
					
				case TypeCode.Single:
					gen.Emit (OpCodes.Ldc_R4, 1);
					gen.Emit (OpCodes.Add);
					break;
					
				case TypeCode.UInt16:
					gen.Emit (OpCodes.Ldc_I4_1);
					gen.Emit (OpCodes.Add);
					gen.Emit (OpCodes.Conv_U2);
					break;
			}
			exp.GenerateSet (gen, exp);
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			exp.PrintCode (cp);
			cp.Write ("++");
		}
		
		public override Type GetResultType ()
		{
			return exp.GetResultType();
		}
	}
}
#endif
