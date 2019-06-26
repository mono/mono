using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;

#if ENABLE_CECIL
using Mono.Cecil.Cil;
#else
using System.Reflection.Emit;
#endif

namespace Mono.Debugger.Soft
{
	public class MethodBodyMirror : Mirror
	{
		MethodMirror method;
		MethodBodyInfo info;

		internal MethodBodyMirror (VirtualMachine vm, MethodMirror method, MethodBodyInfo info) : base (vm, 0) {
			this.method = method;
			this.info = info;
		}

		public MethodMirror Method {
			get {
				return method;
			}
		}

		public List<ILExceptionHandler> ExceptionHandlers {
			get {
				vm.CheckProtocolVersion (2, 18);
				return info.clauses.Select (c =>
				{
					var handler = new ILExceptionHandler (c.try_offset, c.try_length, (ILExceptionHandlerType) c.flags, c.handler_offset, c.handler_length);
					if (c.flags == ExceptionClauseFlags.None)
						handler.CatchType = vm.GetType (c.catch_type_id);
					else if (c.flags == ExceptionClauseFlags.Filter)
						handler.FilterOffset = c.filter_offset;

					return handler;
				}).ToList ();
			}
		}

		public byte[] GetILAsByteArray () {
			return info.il;
		}

		public List<ILInstruction> Instructions {
			get {
				return ReadCilBody (new BinaryReader (new MemoryStream (info.il)), info.il.Length);
			}
		}

		static bool opcodes_inited;

		static OpCode [] OneByteOpCode = new OpCode [0xe0 + 1];
		static OpCode [] TwoBytesOpCode = new OpCode [0x1e + 1];

		Dictionary<int, ResolvedToken> tokensCache = new Dictionary<int, ResolvedToken> ();

		ResolvedToken ResolveToken (int token)
		{
			lock (tokensCache) {
				ResolvedToken resolvedToken;
				if (!tokensCache.TryGetValue (token, out resolvedToken)) {
					resolvedToken = vm.conn.Method_ResolveToken (Method.Id, token);
					tokensCache.Add (token, resolvedToken);
				}
				return resolvedToken;
			}
		}

		// Adapted from Cecil
		List<ILInstruction> ReadCilBody (BinaryReader br, int code_size)
		{
			long start = br.BaseStream.Position;
			ILInstruction last = null;
			//GenericContext context = new GenericContext (body.Method);
			List<ILInstruction> code = new List<ILInstruction> ();

			var by_offset = new Dictionary<int, ILInstruction> ();

			if (!opcodes_inited) {
				foreach (FieldInfo fi in typeof (OpCodes).GetFields (BindingFlags.Static|BindingFlags.Public)) {
					var val = (OpCode)fi.GetValue (null);
					bool isOneByteOpCode;
					uint index;

#if ENABLE_CECIL
					isOneByteOpCode = val.Op1 == 0xff;
					index = val.Op2;
#else
					uint value = (uint)val.Value;
					isOneByteOpCode = value <= 0xff;
					index = isOneByteOpCode ? value : value & 0xff;
#endif
					if (isOneByteOpCode)
						OneByteOpCode [index] = val;
					else
						TwoBytesOpCode [index] = val;
				}
				opcodes_inited = true;
			}

			while (br.BaseStream.Position < start + code_size) {
				OpCode op;
				long offset = br.BaseStream.Position - start;
				int cursor = br.ReadByte ();
				int token;
				ResolvedToken t;

				if (cursor == 0xfe)
					op = TwoBytesOpCode [br.ReadByte ()];
				else
					op = OneByteOpCode [cursor];

				ILInstruction instr = new ILInstruction ((int)offset, op, null);

				by_offset [instr.Offset] = instr;

				switch (op.OperandType) {
				case OperandType.InlineNone :
					break;
				case OperandType.InlineSwitch :
					uint length = br.ReadUInt32 ();
					int [] branches = new int [length];
					int [] buf = new int [length];
					for (int i = 0; i < length; i++)
						buf [i] = br.ReadInt32 ();
					for (int i = 0; i < length; i++)
						branches [i] = Convert.ToInt32 (br.BaseStream.Position - start + buf [i]);
					instr.Operand = branches;
					break;
				case OperandType.ShortInlineBrTarget :
					sbyte sbrtgt = br.ReadSByte ();
					instr.Operand = Convert.ToInt32 (br.BaseStream.Position - start + sbrtgt);
					break;
				case OperandType.InlineBrTarget :
					int brtgt = br.ReadInt32 ();
					instr.Operand = Convert.ToInt32 (br.BaseStream.Position - start + brtgt);
					break;
				case OperandType.ShortInlineI :
					if (op == OpCodes.Ldc_I4_S)
						instr.Operand = br.ReadSByte ();
					else
						instr.Operand = br.ReadByte ();
					break;
				case OperandType.ShortInlineVar :
					instr.Operand = br.ReadByte ();
					break;
#if ENABLE_CECIL
				case OperandType.ShortInlineArg :
					instr.Operand = br.ReadByte ();
					break;
#endif
				case OperandType.InlineSig :
					br.ReadInt32 ();
					//instr.Operand = GetCallSiteAt (br.ReadInt32 (), context);
					break;
				case OperandType.InlineI :
					instr.Operand = br.ReadInt32 ();
					break;
				case OperandType.InlineVar :
					instr.Operand = br.ReadInt16 ();
					break;
#if ENABLE_CECIL
				case OperandType.InlineArg :
					instr.Operand = br.ReadInt16 ();
					break;
#endif
				case OperandType.InlineI8 :
					instr.Operand = br.ReadInt64 ();
					break;
				case OperandType.ShortInlineR :
					instr.Operand = br.ReadSingle ();
					break;
				case OperandType.InlineR :
					instr.Operand = br.ReadDouble ();
					break;
				case OperandType.InlineString :
					token = br.ReadInt32 ();
					t = ResolveToken (token);
					if (t.Type == TokenType.STRING)
						instr.Operand = t.Str;
					break;
				case OperandType.InlineField :
				case OperandType.InlineMethod :
				case OperandType.InlineType :
				case OperandType.InlineTok :
					token = br.ReadInt32 ();

					t = ResolveToken (token);

					switch (t.Type) {
					case TokenType.TYPE:
						instr.Operand = vm.GetType (t.Id);
						break;
					case TokenType.FIELD:
						instr.Operand = vm.GetField (t.Id);
						break;
					case TokenType.METHOD:
						instr.Operand = vm.GetMethod (t.Id);
						break;
					case TokenType.UNKNOWN:
						break;
					default:
						throw new NotImplementedException ("Unknown token type: " + t.Type);
					}
					break;
				}

				if (last != null) {
					last.Next = instr;
					instr.Previous = last;
				}

				last = instr;

				code.Add (instr);
			}

			// resolve branches
			foreach (ILInstruction i in code) {
				switch (i.OpCode.OperandType) {
				case OperandType.ShortInlineBrTarget:
				case OperandType.InlineBrTarget:
					i.Operand = by_offset [(int)i.Operand];
					break;
				case OperandType.InlineSwitch:
					int [] lbls = (int []) i.Operand;
					ILInstruction [] instrs = new ILInstruction [lbls.Length];
					for (int j = 0; j < lbls.Length; j++)
						instrs [j] = by_offset [lbls [j]];
					i.Operand = instrs;
					break;
				}
			}

			return code;
		}
	}
}
