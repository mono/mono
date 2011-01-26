// created on 28/08/2004 at 17:07

#if !MONOTOUCH
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeArgument: CodeExpression
	{
		int argument;
		
		public CodeArgument (int arg, Type type)
		{
			argument = arg;		
		}
		
		public int Argument
		{
			get { return argument; }
		}
		
		public override void Generate (ILGenerator gen)
		{
			gen.Emit (OpCodes.Ldloc, var.LocalBuilder);
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			cp.Write ("arg" + argument);
		}
		
		public override Type GetResultType ()
		{
			return var.Type;
		}
	}
}
#endif
