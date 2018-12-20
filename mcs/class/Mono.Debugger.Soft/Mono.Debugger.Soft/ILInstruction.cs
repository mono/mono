using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

#if ENABLE_CECIL
using Mono.Cecil.Cil;
#else
using System.Reflection.Emit;
#endif

namespace Mono.Debugger.Soft
{
	/*
	 * This is similar to the Instruction class in Cecil, we can't use that
	 * as its constructor is internal.
	 */
	public class ILInstruction
	{
		int offset;
		OpCode opcode;
		object operand;
		ILInstruction prev, next;

		internal ILInstruction (int offset, OpCode opcode, object operand) {
			this.offset = offset;
			this.opcode = opcode;
			this.operand = operand;
		}

		public int Offset {
			get {
				return offset;
			}
		}

		public OpCode OpCode {
			get {
				return opcode;
			}
		}

		public Object Operand {
			get {
				return operand;
			}
			set {
				operand = value;
			}
		}

		public ILInstruction Next {
			get {
				return next;
			}
			set {
				next = value;
			}
		}

		public ILInstruction Previous {
			get {
				return prev;
			}
			set {
				prev = value;
			}
		}
	}
}
