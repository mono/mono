
//
// System.Reflection.Emit/ILGenerator.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;
using Mono.CSharp.Debugger;

namespace System.Reflection.Emit {

	internal struct ILExceptionBlock {
		public const int CATCH = 0;
		public const int FILTER = 1;
		public const int FINALLY = 2;
		public const int FAULT = 4;

		internal Type extype;
		internal int type;
		internal int start;
		internal int len;
		internal int filter_offset;
		
		internal void Debug () {
#if NO
			System.Console.Write ("\ttype="+type.ToString()+" start="+start.ToString()+" len="+len.ToString());
			if (extype != null)
				System.Console.WriteLine (" extype="+extype.ToString());
			else
				System.Console.WriteLine ("");
#endif
		}
	}
	internal struct ILExceptionInfo {
		ILExceptionBlock[] handlers;
		internal int start;
		int len;
		internal Label end;

		internal int NumHandlers () {
			return handlers.Length;
		}
		
		internal void AddCatch (Type extype, int offset) {
			int i;
			End (offset);
			add_block (offset);
			i = handlers.Length - 1;
			handlers [i].type = ILExceptionBlock.CATCH;
			handlers [i].start = offset;
			handlers [i].extype = extype;
		}

		internal void AddFinally (int offset) {
			int i;
			End (offset);
			add_block (offset);
			i = handlers.Length - 1;
			handlers [i].type = ILExceptionBlock.FINALLY;
			handlers [i].start = offset;
			handlers [i].extype = null;
		}

		internal void End (int offset) {
			if (handlers == null)
				return;
			int i = handlers.Length - 1;
			if (i >= 0)
				handlers [i].len = offset - handlers [i].start;
		}

		internal int LastClauseType () {
			if (handlers != null)
				return handlers [handlers.Length-1].type;
			else
				return ILExceptionBlock.CATCH;
		}

		internal void Debug (int b) {
#if NO
			System.Console.WriteLine ("Handler {0} at {1}, len: {2}", b, start, len);
			for (int i = 0; i < handlers.Length; ++i)
				handlers [i].Debug ();
#endif
		}

		void add_block (int offset) {
			if (handlers != null) {
				int i = handlers.Length;
				ILExceptionBlock[] new_b = new ILExceptionBlock [i + 1];
				System.Array.Copy (handlers, new_b, i);
				handlers = new_b;
				handlers [i].len = offset - handlers [i].start;
			} else {
				handlers = new ILExceptionBlock [1];
				len = offset - start;
			}
		}
	}
	
	internal struct ILTokenInfo {
		public MemberInfo member;
		public int code_pos;
	}

	public class ILGenerator: Object {
		private struct LabelFixup {
			public int size;
			public int pos;			// the location of the fixup
			public int label_base;	// the base address for this fixup
			public int label_idx;
		};
		static Type void_type = typeof (void);
		private byte[] code;
		private MethodBase mbuilder; /* a MethodBuilder or ConstructorBuilder */
		private int code_len;
		private int max_stack;
		private int cur_stack;
		private LocalBuilder[] locals;
		private ILExceptionInfo[] ex_handlers;
		private int num_token_fixups;
		private ILTokenInfo[] token_fixups;
		private int[] label_to_addr;
		private int num_labels;
		private LabelFixup[] fixups;
		private int num_fixups;
		private ModuleBuilder module;
		private AssemblyBuilder abuilder;
		private IMonoSymbolWriter sym_writer;
		private Stack scopes;
		private int cur_block;
		private Stack open_blocks;

		internal ILGenerator (MethodBase mb, int size) {
			if (size < 0)
				size = 128;
			code_len = 0;
			code = new byte [size];
			mbuilder = mb;
			cur_stack = max_stack = 0;
			num_fixups = num_labels = 0;
			label_to_addr = new int [8];
			fixups = new LabelFixup [8];
			token_fixups = new ILTokenInfo [8];
			num_token_fixups = 0;
			if (mb is MethodBuilder) {
				module = (ModuleBuilder)((MethodBuilder)mb).TypeBuilder.Module;
			} else if (mb is ConstructorBuilder) {
				module = (ModuleBuilder)((ConstructorBuilder)mb).TypeBuilder.Module;
			}
			abuilder = (AssemblyBuilder)module.Assembly;
			sym_writer = module.symbol_writer;
			open_blocks = new Stack ();
		}

		private void add_token_fixup (MemberInfo mi) {
			if (num_token_fixups == token_fixups.Length) {
				ILTokenInfo[] ntf = new ILTokenInfo [num_token_fixups * 2];
				token_fixups.CopyTo (ntf, 0);
				token_fixups = ntf;
			}
			token_fixups [num_token_fixups].member = mi;
			token_fixups [num_token_fixups++].code_pos = code_len;
		}

		private void make_room (int nbytes) {
			if (code_len + nbytes < code.Length)
				return;
			byte[] new_code = new byte [(code_len + nbytes) * 2 + 128];
			System.Array.Copy (code, 0, new_code, 0, code.Length);
			code = new_code;
		}
		private void emit_int (int val) {
			code [code_len++] = (byte) (val & 0xFF);
			code [code_len++] = (byte) ((val >> 8) & 0xFF);
			code [code_len++] = (byte) ((val >> 16) & 0xFF);
			code [code_len++] = (byte) ((val >> 24) & 0xFF);
		}
		/* change to pass by ref to avoid copy */
		private void ll_emit (OpCode opcode) {
			/* 
			 * there is already enough room allocated in code.
			 */
			// access op1 and op2 directly since the Value property is useless
			if (opcode.Size == 2)
				code [code_len++] = opcode.op1;
			code [code_len++] = opcode.op2;
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

		private void InternalEndClause () {
			switch (ex_handlers [cur_block].LastClauseType ()) {
			case ILExceptionBlock.CATCH:
				// how could we optimize code size here?
				Emit (OpCodes.Leave, ex_handlers [cur_block].end);
				break;
			case ILExceptionBlock.FAULT:
			case ILExceptionBlock.FINALLY:
				Emit (OpCodes.Endfinally);
				break;
			case ILExceptionBlock.FILTER:
				Emit (OpCodes.Endfilter);
				break;
			}
		}

		public virtual void BeginCatchBlock (Type exceptionType) {
			if (open_blocks.Count <= 0)
				throw new NotSupportedException ("Not in an exception block");
			InternalEndClause ();
			ex_handlers [cur_block].AddCatch (exceptionType, code_len);
			cur_stack = 1; // the exception object is on the stack by default
			if (max_stack < cur_stack)
				max_stack = cur_stack;
			//System.Console.WriteLine ("Begin catch Block: {0} {1}",exceptionType.ToString(), max_stack);
			//throw new NotImplementedException ();
		}
		public virtual void BeginExceptFilterBlock () {
			throw new NotImplementedException ();
		}
		public virtual Label BeginExceptionBlock () {
			//System.Console.WriteLine ("Begin Block");
			
			if (ex_handlers != null) {
				cur_block = ex_handlers.Length;
				ILExceptionInfo[] new_ex = new ILExceptionInfo [cur_block + 1];
				System.Array.Copy (ex_handlers, new_ex, cur_block);
				ex_handlers = new_ex;
			} else {
				ex_handlers = new ILExceptionInfo [1];
				cur_block = 0;
			}
			open_blocks.Push (cur_block);
			ex_handlers [cur_block].start = code_len;
			return ex_handlers [cur_block].end = DefineLabel ();
		}
		public virtual void BeginFaultBlock() {
			if (open_blocks.Count <= 0)
				throw new NotSupportedException ("Not in an exception block");
			//System.Console.WriteLine ("Begin fault Block");
			//throw new NotImplementedException ();
		}
		public virtual void BeginFinallyBlock() {
			if (open_blocks.Count <= 0)
				throw new NotSupportedException ("Not in an exception block");
			InternalEndClause ();
			//System.Console.WriteLine ("Begin finally Block");
			ex_handlers [cur_block].AddFinally (code_len);
		}
		public virtual void BeginScope () {
			if (sym_writer != null) {
				if (scopes == null)
					scopes = new Stack ();
				scopes.Push (sym_writer.OpenScope (code_len));
			}
		}
		public LocalBuilder DeclareLocal (Type localType) {
			LocalBuilder res = new LocalBuilder (module, localType, this);
			if (locals != null) {
				LocalBuilder[] new_l = new LocalBuilder [locals.Length + 1];
				System.Array.Copy (locals, new_l, locals.Length);
				new_l [locals.Length] = res;
				locals = new_l;
			} else {
				locals = new LocalBuilder [1];
				locals [0] = res;
			}
			res.position = (uint)(locals.Length - 1);
			return res;
		}
		public virtual Label DefineLabel () {
			if (num_labels >= label_to_addr.Length) {
				int[] new_l = new int [label_to_addr.Length * 2];
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
			if (constructor is ConstructorBuilder)
				add_token_fixup (constructor);
			emit_int (token);
			ParameterInfo[] mparams = constructor.GetParameters();
			if (mparams != null)
				cur_stack -= mparams.Length;
		}
		public virtual void Emit (OpCode opcode, double val) {
			byte[] s = System.BitConverter.GetBytes (val);
			make_room (10);
			ll_emit (opcode);
			if (BitConverter.IsLittleEndian){
				System.Array.Copy (s, 0, code, code_len, 8);
				code_len += 8;
			} else {
				code [code_len++] = s [7];
				code [code_len++] = s [6];
				code [code_len++] = s [5];
				code [code_len++] = s [4];
				code [code_len++] = s [3];
				code [code_len++] = s [2];
				code [code_len++] = s [1];
				code [code_len++] = s [0];				
			}
		}
		public virtual void Emit (OpCode opcode, FieldInfo field) {
			int token = abuilder.GetToken (field);
			make_room (6);
			ll_emit (opcode);
			if (field is FieldBuilder)
				add_token_fixup (field);
			emit_int (token);
		}
		public virtual void Emit (OpCode opcode, Int16 val) {
			make_room (4);
			ll_emit (opcode);
			code [code_len++] = (byte) (val & 0xFF);
			code [code_len++] = (byte) ((val >> 8) & 0xFF);
		}
		public virtual void Emit (OpCode opcode, int val) {
			make_room (6);
			ll_emit (opcode);
			emit_int (val);
		}
		public virtual void Emit (OpCode opcode, long val) {
			make_room (10);
			ll_emit (opcode);
			code [code_len++] = (byte) (val & 0xFF);
			code [code_len++] = (byte) ((val >> 8) & 0xFF);
			code [code_len++] = (byte) ((val >> 16) & 0xFF);
			code [code_len++] = (byte) ((val >> 24) & 0xFF);
			code [code_len++] = (byte) ((val >> 32) & 0xFF);
			code [code_len++] = (byte) ((val >> 40) & 0xFF);
			code [code_len++] = (byte) ((val >> 48) & 0xFF);
			code [code_len++] = (byte) ((val >> 56) & 0xFF);
		}
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
			fixups [num_fixups].label_base = code_len;
			fixups [num_fixups].label_idx = label.label;
			num_fixups++;
			code_len += tlen;

		}
		public virtual void Emit (OpCode opcode, Label[] labels) {
			/* opcode needs to be switch. */
			int count = labels.Length;
			make_room (6 + count * 4);
			ll_emit (opcode);
			int switch_base = code_len + count*4;
			emit_int (count);
			if (num_fixups + count >= fixups.Length) {
				LabelFixup[] newf = new LabelFixup [fixups.Length + count + 16];
				System.Array.Copy (fixups, newf, fixups.Length);
				fixups = newf;
			}
			for (int i = 0; i < count; ++i) {
				fixups [num_fixups].size = 4;
				fixups [num_fixups].pos = code_len;
				fixups [num_fixups].label_base = switch_base;
				fixups [num_fixups].label_idx = labels [i].label;
				num_fixups++;
				code_len += 4;
			}
		}
		public virtual void Emit (OpCode opcode, LocalBuilder lbuilder) {
			uint pos = lbuilder.position;
			bool load_addr = false;
			bool is_store = false;
			make_room (6);

			if (lbuilder.ilgen != this)
				throw new Exception ("Trying to emit a local from a different ILGenerator.");

			/* inline the code from ll_emit () to optimize il code size */
			if (opcode.StackBehaviourPop == StackBehaviour.Pop1) {
				cur_stack --;
				is_store = true;
			} else {
				cur_stack++;
				if (cur_stack > max_stack)
					max_stack = cur_stack;
				load_addr = opcode.StackBehaviourPush == StackBehaviour.Pushi;
			}
			if (load_addr) {
				if (pos < 256) {
					code [code_len++] = (byte)0x12;
					code [code_len++] = (byte)pos;
				} else {
					code [code_len++] = (byte)0xfe;
					code [code_len++] = (byte)0x0d;
					code [code_len++] = (byte)(pos & 0xff);
					code [code_len++] = (byte)((pos >> 8) & 0xff);
				}
			} else {
				if (is_store) {
					if (pos < 4) {
						code [code_len++] = (byte)(0x0a + pos);
					} else if (pos < 256) {
						code [code_len++] = (byte)0x13;
						code [code_len++] = (byte)pos;
					} else {
						code [code_len++] = (byte)0xfe;
						code [code_len++] = (byte)0x0e;
						code [code_len++] = (byte)(pos & 0xff);
						code [code_len++] = (byte)((pos >> 8) & 0xff);
					}
				} else {
					if (pos < 4) {
						code [code_len++] = (byte)(0x06 + pos);
					} else if (pos < 256) {
						code [code_len++] = (byte)0x11;
						code [code_len++] = (byte)pos;
					} else {
						code [code_len++] = (byte)0xfe;
						code [code_len++] = (byte)0x0c;
						code [code_len++] = (byte)(pos & 0xff);
						code [code_len++] = (byte)((pos >> 8) & 0xff);
					}
				}
			}
		}
		public virtual void Emit (OpCode opcode, MethodInfo method) {
			if (method == null)
				throw new ArgumentNullException ("method");

			int token = abuilder.GetToken (method);
			make_room (6);
			ll_emit (opcode);
			if (method is MethodBuilder)
				add_token_fixup (method);
			emit_int (token);
			if (method.ReturnType != void_type)
				cur_stack ++;
			ParameterInfo[] mparams = method.GetParameters();
			if (mparams != null)
				cur_stack -= mparams.Length;
		}
		[CLSCompliant(false)]
		public void Emit (OpCode opcode, sbyte val) {
			make_room (3);
			ll_emit (opcode);
			code [code_len++] = (byte)val;
		}

		public virtual void Emit (OpCode opcode, SignatureHelper shelper) {
			int token = abuilder.GetToken (shelper);
			make_room (6);
			ll_emit (opcode);
			emit_int (token);
		}
		public virtual void Emit (OpCode opcode, float val) {
			byte[] s = System.BitConverter.GetBytes (val);
			make_room (6);
			ll_emit (opcode);
			if (BitConverter.IsLittleEndian){
				System.Array.Copy (s, 0, code, code_len, 4);
				code_len += 4;
			} else {
				code [code_len++] = s [3];
				code [code_len++] = s [2];
				code [code_len++] = s [1];
				code [code_len++] = s [0];				
			}
		}
		public virtual void Emit (OpCode opcode, string val) {
			int token = abuilder.GetToken (val);
			make_room (6);
			ll_emit (opcode);
			emit_int (token);
		}
		public virtual void Emit (OpCode opcode, Type type) {
			make_room (6);
			ll_emit (opcode);
			if (type is TypeBuilder)
				add_token_fixup (type);
			emit_int (abuilder.GetToken (type));
		}

		public void EmitCall (OpCode opcode, MethodInfo methodinfo, Type[] optionalParamTypes) {
			throw new NotImplementedException ();
		}

		public void EmitCalli (OpCode opcode, CallingConvention unmanagedCallConv, Type returnType, Type[] paramTypes) {
			SignatureHelper helper 
				= SignatureHelper.GetMethodSigHelper (module, 0, unmanagedCallConv, returnType, paramTypes);
			Emit (opcode, helper);
		}

		public void EmitCalli (OpCode opcode, CallingConventions callConv, Type returnType, Type[] paramTypes, Type[] optionalParamTypes) {
			if (optionalParamTypes != null)
				throw new NotImplementedException ();

			SignatureHelper helper 
				= SignatureHelper.GetMethodSigHelper (module, callConv, 0, returnType, paramTypes);
			Emit (opcode, helper);
		}
		
		public virtual void EmitWriteLine (FieldInfo field) {
			throw new NotImplementedException ();
		}
		public virtual void EmitWriteLine (LocalBuilder lbuilder) {
			throw new NotImplementedException ();
		}
		public virtual void EmitWriteLine (string val) {
			Emit (OpCodes.Ldstr, val);
			Emit (OpCodes.Call, 
				  typeof (Console).GetMethod ("WriteLine",
											  new Type[1] { typeof(string)}));
		}

		public virtual void EndExceptionBlock () {
			if (open_blocks.Count <= 0)
				throw new NotSupportedException ("Not in an exception block");
			InternalEndClause ();
			MarkLabel (ex_handlers [cur_block].end);
			ex_handlers [cur_block].End (code_len);
			ex_handlers [cur_block].Debug (cur_block);
			//System.Console.WriteLine ("End Block {0} (handlers: {1})", cur_block, ex_handlers [cur_block].NumHandlers ());
			open_blocks.Pop ();
			if (open_blocks.Count > 0)
				cur_block = (int)open_blocks.Peek ();
			//Console.WriteLine ("curblock restored to {0}", cur_block);
			//throw new NotImplementedException ();
		}
		public virtual void EndScope () {
			if (sym_writer != null) {
				sym_writer.CloseScope (code_len);
				if (scopes == null)
					throw new InvalidOperationException ();
				scopes.Pop ();
			}
		}
		public virtual void MarkLabel (Label loc) {
			if (loc.label < 0 || loc.label >= num_labels)
				throw new System.ArgumentException ("The label is not valid");
			if (label_to_addr [loc.label] >= 0)
				throw new System.ArgumentException ("The label was already defined");
			label_to_addr [loc.label] = code_len;
		}
		public virtual void MarkSequencePoint (ISymbolDocumentWriter document, int startLine,
						       int startColumn, int endLine, int endColumn) {
			if (sym_writer == null)
				return;

			sym_writer.MarkSequencePoint (code_len, startLine, startColumn);
		}
		public virtual void ThrowException (Type exceptionType) {
			throw new NotImplementedException ();
		}
		public void UsingNamespace (String usingNamespace) {
			throw new NotImplementedException ();
		}

		internal void label_fixup () {
			int i;
			for (i = 0; i < num_fixups; ++i) {
				int diff = label_to_addr [fixups [i].label_idx] - fixups [i].label_base;
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
