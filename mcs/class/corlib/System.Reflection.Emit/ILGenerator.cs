
//
// System.Reflection.Emit/ILGenerator.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Diagnostics.SymbolStore;

namespace System.Reflection.Emit {
	public class ILGenerator: Object {
		private struct LabelFixup {
			public int size;
			public int pos;
			public int label_idx;
		};
		private byte[] code;
		private MethodBase mbuilder; /* a MethodBuilder or ConstructorBuilder */
		private int code_len;
		private int max_stack;
		private int cur_stack;
		private LocalBuilder[] locals;
		private int[] label_to_addr;
		private int num_labels;
		private LabelFixup[] fixups;
		private int num_fixups;
		private AssemblyBuilder abuilder;

		internal ILGenerator (MethodBase mb, int size) {
			if (size < 0)
				size = 256;
			code_len = 0;
			code = new byte [size];
			mbuilder = mb;
			cur_stack = max_stack = 0;
			num_fixups = num_labels = 0;
			label_to_addr = new int [16];
			fixups = new LabelFixup [16];
			if (mb is MethodBuilder) {
				abuilder = (AssemblyBuilder)((MethodBuilder)mb).TypeBuilder.Module.Assembly;
			} else if (mb is ConstructorBuilder) {
				abuilder = (AssemblyBuilder)((ConstructorBuilder)mb).TypeBuilder.Module.Assembly;
			}
		}

		private void make_room (int nbytes) {
			if (code_len + nbytes < code.Length)
				return;
			byte[] new_code = new byte [code.Length + 128];
			System.Array.Copy (code, new_code, code.Length);
			code = new_code;
		}
		private void emit_int (int val) {
			code [code_len++] = (byte) (val & 0xFF);
			code [code_len++] = (byte) ((val >> 8) & 0xFF);
			code [code_len++] = (byte) ((val >> 16) & 0xFF);
			code [code_len++] = (byte) ((val << 24) & 0xFF);
		}
		/* change to pass by ref to avoid copy */
		private void ll_emit (OpCode opcode) {
			/* 
			 * there is already enough room allocated in code.
			 */
			if (opcode.Size == 1) {
				code [code_len++] = (byte)opcode.Value;
			} else {
				code [code_len++] = (byte)(opcode.Value & 0xFF);
				code [code_len++] = (byte)(opcode.Value >> 8);
			}
			/*
			 * We should probably keep track of stack needs here.
			 * Or we may want to run the verifier on the code before saving it
			 * (this may be needed anyway when the ILGenerator is not used...).
			 */
			switch (opcode.StackBehaviourPush) {
			case StackBehaviour.Push1:
			case StackBehaviour.Pushi:
			case StackBehaviour.Pushi8:
			case StackBehaviour.Pushr4:
			case StackBehaviour.Pushr8:
			case StackBehaviour.Pushref:
			case StackBehaviour.Varpush: /* again we are conservative and assume it pushes 1 */
				cur_stack ++;
				break;
			case StackBehaviour.Push1_push1:
				cur_stack += 2;
				break;
			}
			if (max_stack < cur_stack)
				max_stack = cur_stack;
			/* 
			 * Note that we adjust for the pop behaviour _after_ setting max_stack.
			 */
			switch (opcode.StackBehaviourPop) {
			case StackBehaviour.Varpop:
				break; /* we are conservative and assume it doesn't decrease the stack needs */
			case StackBehaviour.Pop1:
			case StackBehaviour.Popi:
			case StackBehaviour.Popref:
				cur_stack --;
				break;
			case StackBehaviour.Pop1_pop1:
			case StackBehaviour.Popi_pop1:
			case StackBehaviour.Popi_popi:
			case StackBehaviour.Popi_popi8:
			case StackBehaviour.Popi_popr4:
			case StackBehaviour.Popi_popr8:
			case StackBehaviour.Popref_pop1:
			case StackBehaviour.Popref_popi:
				cur_stack -= 2;
				break;
			case StackBehaviour.Popi_popi_popi:
			case StackBehaviour.Popref_popi_popi:
			case StackBehaviour.Popref_popi_popi8:
			case StackBehaviour.Popref_popi_popr4:
			case StackBehaviour.Popref_popi_popr8:
			case StackBehaviour.Popref_popi_popref:
				cur_stack -= 3;
				break;
			}
		}

		private static int target_len (OpCode opcode) {
			if (opcode.operandType == OperandType.InlineBrTarget)
				return 4;
			return 1;
		}

		public virtual void BeginCatchBlock (Type exceptionType) {}
		public virtual void BeginExceptFilterBlock () {}
		public virtual void BeginExceptionBlock () {}
		public virtual void BeginFaultBlock() {}
		public virtual void BeginFinallyBlock() {}
		public virtual void BeginScope () {}
		public virtual LocalBuilder DeclareLocal (Type localType) {
			LocalBuilder res = new LocalBuilder (localType);
			if (locals != null) {
				LocalBuilder[] new_l = new LocalBuilder [locals.Length];
				System.Array.Copy (locals, new_l, locals.Length);
				new_l [locals.Length] = res;
				locals = new_l;
			} else {
				locals = new LocalBuilder [1];
				locals [0] = res;
			}
			res.position = locals.Length - 1;
			return res;
		}
		public virtual Label DefineLabel () {
			if (num_labels >= label_to_addr.Length) {
				int[] new_l = new int [label_to_addr.Length + 16];
				System.Array.Copy (label_to_addr, new_l, label_to_addr.Length);
				label_to_addr = new_l;
			}
			label_to_addr [num_labels] = -1;
			return new Label (num_labels++);
		}
		public virtual void Emit (OpCode opcode) {
			make_room (2);
			ll_emit (opcode);
		}
		public virtual void Emit (OpCode opcode, Byte val) {
			make_room (3);
			ll_emit (opcode);
			code [code_len++] = val;
		}
		public virtual void Emit (OpCode opcode, ConstructorInfo constructor) {
			int token = abuilder.GetToken (constructor);
			make_room (6);
			ll_emit (opcode);
			emit_int (token);
		}
		public virtual void Emit (OpCode opcode, Double val) {}
		public virtual void Emit (OpCode opcode, FieldInfo field) {
			int token = abuilder.GetToken (field);
			make_room (6);
			ll_emit (opcode);
			emit_int (token);
		}
		public virtual void Emit (OpCode opcode, Int16 val) {
			make_room (4);
			ll_emit (opcode);
			code [code_len++] = (byte) (val & 0xFF);
			code [code_len++] = (byte) ((val >> 8) & 0xFF);
		}
		public virtual void Emit (OpCode opcode, Int32 val) {
			make_room (6);
			ll_emit (opcode);
			emit_int (val);
		}
		public virtual void Emit (OpCode opcode, Int64 val) {}
		public virtual void Emit (OpCode opcode, Label label) {
			int tlen = target_len (opcode);
			make_room (6);
			ll_emit (opcode);
			if (num_fixups >= fixups.Length) {
				LabelFixup[] newf = new LabelFixup [fixups.Length + 16];
				System.Array.Copy (fixups, newf, fixups.Length);
				fixups = newf;
			}
			fixups [num_fixups].size = tlen;
			fixups [num_fixups].pos = code_len;
			fixups [num_fixups].label_idx = label.label;
			num_fixups++;
			code_len += tlen;

		}
		public virtual void Emit (OpCode opcode, Label[] labels) {
			/* opcode needs to be switch. */
			int count = labels.Length;
			make_room (6 + count * 4);
			ll_emit (opcode);
			emit_int (count);
			if (num_fixups + count >= fixups.Length) {
				LabelFixup[] newf = new LabelFixup [fixups.Length + count + 16];
				System.Array.Copy (fixups, newf, fixups.Length);
				fixups = newf;
			}
			for (int i = 0; i < count; ++i) {
				fixups [num_fixups].size = 4;
				fixups [num_fixups].pos = code_len;
				fixups [num_fixups].label_idx = labels [i].label;
				num_fixups++;
				code_len += 4;
			}
		}
		public virtual void Emit (OpCode opcode, LocalBuilder lbuilder) {
			make_room (6);
			ll_emit (opcode);
			code [code_len++] = (byte) (lbuilder.position & 0xFF);
			if (opcode.operandType == OperandType.InlineVar) {
				code [code_len++] = (byte) ((lbuilder.position >> 8) & 0xFF);
			}
		}
		public virtual void Emit (OpCode opcode, MethodInfo method) {
			int token = abuilder.GetToken (method);
			make_room (6);
			ll_emit (opcode);
			emit_int (token);
		}
		[CLSCompliant(false)]
		public virtual void Emit (OpCode opcode, sbyte val) {
			make_room (3);
			ll_emit (opcode);
			code [code_len++] = (byte)val;
		}
		public virtual void Emit (OpCode opcode, SignatureHelper shelper) {
			int token = 0; // FIXME: request a token from the modulebuilder
			make_room (6);
			ll_emit (opcode);
			emit_int (token);
		}
		public virtual void Emit (OpCode opcode, float val) {}
		public virtual void Emit (OpCode opcode, string val) {
			int token = abuilder.GetToken (val);
			make_room (3);
			ll_emit (opcode);
			emit_int (token);
		}
		public virtual void Emit (OpCode opcode, Type type) {}

		public void EmitCall (OpCode opcode, MethodInfo methodinfo, Type[] optionalParamTypes) {}
		public void EmitCalli (OpCode opcode, CallingConventions call_conv, Type returnType, Type[] paramTypes, Type[] optionalParamTypes) {}

		public virtual void EmitWriteLine (FieldInfo field) {}
		public virtual void EmitWriteLine (LocalBuilder lbuilder) {}
		public virtual void EmitWriteLine (string val) {}

		public virtual void EndExceptionBlock () {}
		public virtual void EndScope () {}
		public virtual void MarkLabel (Label loc) {
			if (loc.label < 0 || loc.label >= num_labels)
				throw new System.ArgumentException ("The label is not valid");
			if (label_to_addr [loc.label] >= 0)
				throw new System.ArgumentException ("The label was already defined");
			label_to_addr [loc.label] = code_len;
		}
		public virtual void MarkSequencePoint (ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int EndColumn) {}
		public virtual void ThrowException (Type exceptionType) {}
		public virtual void UsingNamespace (String usingNamespace) {}

		internal void label_fixup () {
			int i;
			for (i = 0; i < num_fixups; ++i) {
				int diff = label_to_addr [fixups [i].label_idx] - fixups [i].pos;
				if (fixups [i].size == 1) {
					code [fixups [i].pos] = (byte)((sbyte) diff - 1);
				} else {
					int old_cl = code_len;
					code_len = fixups [i].pos;
					emit_int (diff - 4);
					code_len = old_cl;
				}
			}
		}
	}
}
