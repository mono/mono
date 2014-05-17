using System;
using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;

namespace Mono.Debugger.Soft
{
	internal class ILInterpreter
	{
		MethodMirror method;

		public ILInterpreter (MethodMirror method) {
			this.method = method;
		}

		public Value Evaluate (Value this_val, Value[] args) {
			var body = method.GetMethodBody ();

			// Implement only the IL opcodes required to evaluate mcs compiled property accessors:
			// IL_0000:  nop
			// IL_0001:  ldarg.0
			// IL_0002:  ldfld      int32 Tests::field_i
			// IL_0007:  stloc.0
			// IL_0008:  br         IL_000d
			// IL_000d:  ldloc.0
			// IL_000e:  ret
			if (args != null && args.Length != 0)
				throw new NotSupportedException ();				
			if (method.IsStatic || method.DeclaringType.IsValueType || this_val == null || !(this_val is ObjectMirror))
				throw new NotSupportedException ();

			var instructions = body.Instructions;
			if (instructions.Count > 16)
				throw new NotSupportedException ();

			Value[] stack = new Value [16];
			Value locals_0 = null;
			Value res = null;

			int sp = 0;
			int ins_count = 0;
			var ins = instructions [0];
			while (ins != null) {
				if (ins_count > 16)
					throw new NotImplementedException ();
				ins_count ++;
				var next = ins.Next;

				var op = ins.OpCode;
				if (op == OpCodes.Nop) {
				} else if (op == OpCodes.Ldarg_0) {
					if (sp > 0)
						throw new NotSupportedException ();
					stack [sp++] = this_val;
				} else if (op == OpCodes.Ldfld) {
					if (sp != 1)
						throw new NotSupportedException ();
					var obj = (ObjectMirror)stack [--sp];
					var field = (FieldInfoMirror)ins.Operand;
					try {
						stack [sp++] = obj.GetValue (field);
					} catch (ArgumentException) {
						throw new NotSupportedException ();
					}
				} else if (op == OpCodes.Stloc_0) {
					if (sp != 1)
						throw new NotSupportedException ();
					locals_0 = stack [--sp];
				} else if (op == OpCodes.Br) {
					next = (ILInstruction)ins.Operand;
				} else if (op == OpCodes.Ldloc_0) {
					if (sp != 0)
						throw new NotSupportedException ();
					stack [sp++] = locals_0;
				} else if (op == OpCodes.Ret) {
					if (sp == 0)
						res = null;
					else
						res = stack [--sp];
					break;
				} else {
					throw new NotSupportedException ();
				}
				ins = next;
			}

			return res;
		}
	}
}
