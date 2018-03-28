// InstrToken.cs
// Author: Sergey Chaban (serge@wildwestsoftware.com)

#if !MOBILE

using System;
using System.Reflection.Emit;

namespace Mono.ILASM {

	public class InstrToken : ILToken {


		/// <summary>
		/// </summary>
		public InstrToken (OpCode opcode)
		{
			this.val = opcode;
			token = GetInstrType (opcode);
		}


		/// <summary>
		/// </summary>
		/// <param name="opcode"></param>
		/// <returns></returns>
		public static int GetInstrType (OpCode opcode)
		{
			OperandType t = opcode.OperandType;
			int token = Token.UNKNOWN;

			switch (t) {

				case OperandType.InlineBrTarget:
				case OperandType.ShortInlineBrTarget:
					token = Token.INSTR_BRTARGET;
					break;

				case OperandType.InlineField:
					token = Token.INSTR_FIELD;
					break;

				case OperandType.InlineI:
				case OperandType.ShortInlineI:
					token = Token.INSTR_I;
					break;

				case OperandType.InlineI8:
					token = Token.INSTR_I8;
					break;

				case OperandType.InlineMethod:
					token = Token.INSTR_METHOD;
					break;

				case OperandType.InlineNone:
					token = Token.INSTR_NONE;
					break;
#pragma warning disable 618
				case OperandType.InlinePhi:
					token = Token.INSTR_PHI;
					break;
#pragma warning restore 618

				case OperandType.InlineR:
				case OperandType.ShortInlineR:
					token = Token.INSTR_R;
					break;

				/*
				case OperandType.InlineRVA:
					token = Token.INSTR_RVA;
					break;
				*/

				case OperandType.InlineSig:
					token = Token.INSTR_SIG;
					break;

				case OperandType.InlineString:
					token = Token.INSTR_STRING;
					break;

				case OperandType.InlineSwitch:
					token = Token.INSTR_SWITCH;
					break;

				case OperandType.InlineTok:
					token = Token.INSTR_TOK;
					break;

				case OperandType.InlineType:
					token = Token.INSTR_TYPE;
					break;

				case OperandType.InlineVar:
				case OperandType.ShortInlineVar:
					token = Token.INSTR_VAR;
					break;
			}

			return token;
		}


	}

}

#endif
