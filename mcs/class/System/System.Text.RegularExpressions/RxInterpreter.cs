
using System;
using System.Collections;

namespace System.Text.RegularExpressions {

	class RxInterpreter: BaseMachine {
		byte[] program;
		string str;
		int string_start;
		int string_end;
		int group_count;
		int match_start;
		int[] groups;

		static int ReadInt (byte[] code, int pc)
		{
			int val = code [pc];
			val |= code [pc + 1] << 8;
			val |= code [pc + 2] << 16;
			val |= code [pc + 3] << 24;
			return val;
		}

		public RxInterpreter (byte[] program)
		{
			this.program = program;
			group_count = 1 + (program [1] | (program [2] << 8));
			groups = new int [group_count];
		}

		public override Match Scan (Regex regex, string text, int start, int end) {
			str = text;
			string_start = start;
			string_end = end;
			int res = 0;
			if (EvalByteCode (11, start, ref res)) {
				Match m = new Match (regex, this, text, end, 0, match_start, res - match_start);
				return m;
			}
			return Match.Empty;
		}

		bool EvalByteCode (int pc, int strpos, ref int strpos_result)
		{
			int length, start, end;
			while (true) {
				//Console.WriteLine ("evaluating: {0} at pc: {1}, strpos: {2}", (RxOp)program [pc], pc, strpos);
				switch ((RxOp)program [pc]) {
				case RxOp.True:
					strpos_result = strpos;
					return true;
				case RxOp.False:
					return false;
				case RxOp.AnyPosition:
					pc++;
					continue;
				case RxOp.StartOfString:
					if (strpos != 0)
						return false;
					pc++;
					continue;
				case RxOp.StartOfLine:
					if (strpos == 0 || str [strpos - 1] == '\n') {
						pc++;
						continue;
					}
					return false;
				case RxOp.StartOfScan:
					if (strpos != string_start)
						return false;
					pc++;
					continue;
				case RxOp.End:
					if (strpos == string_end || (strpos == string_end - 1 && str [strpos] == '\n')) {
						pc++;
						continue;
					}
					return false;
				case RxOp.EndOfString:
					if (strpos != string_end)
						return false;
					pc++;
					continue;
				case RxOp.EndOfLine:
					if (strpos == string_end || str [strpos] == '\n') {
						pc++;
						continue;
					}
					return false;
				case RxOp.Anchor:
					// FIXME: test anchor
					length = program [pc + 3] | (program [pc + 4] << 8);
					pc += program [pc + 1] | (program [pc + 2] << 8);
					while (strpos < string_end) {
						int res = strpos;
						if (EvalByteCode (pc, strpos, ref res)) {
							match_start = strpos;
							strpos_result = res;
							return true;
						}
						strpos++;
					}
					return false;
				case RxOp.Jump:
					pc += program [pc + 1] | (program [pc + 2] << 8);
					continue;
				case RxOp.String:
					start = pc + 2;
					length = program [pc + 1];
					if (strpos + length > string_end)
						return false;
					end = start + length;
					for (; start < end; ++start) {
						if (str [strpos] != program [start])
							return false;
						strpos++;
					}
					pc = end;
					continue;
				case RxOp.StringIgnoreCase:
					start = pc + 2;
					length = program [pc + 1];
					if (strpos + length > string_end)
						return false;
					end = start + length;
					for (; start < end; ++start) {
						if (str [strpos] != program [start] && Char.ToLower (str [strpos]) != program [start])
							return false;
						strpos++;
					}
					pc = end;
					continue;
				case RxOp.UnicodeString:
					start = pc + 3;
					length = program [pc + 1] | (program [pc + 2] << 8);
					if (strpos + length > string_end)
						return false;
					end = start + length * 2;
					for (; start < end; start += 2) {
						int c = program [start] | (program [start + 1] << 8);
						if (str [strpos] != c)
							return false;
						strpos++;
					}
					pc = end;
					continue;
				case RxOp.UnicodeStringIgnoreCase:
					start = pc + 3;
					length = program [pc + 1] | (program [pc + 2] << 8);
					if (strpos + length > string_end)
						return false;
					end = start + length * 2;
					for (; start < end; start += 2) {
						int c = program [start] | (program [start + 1] << 8);
						if (str [strpos] != c && Char.ToLower (str [strpos]) != c)
							return false;
						strpos++;
					}
					pc = end;
					continue;
				case RxOp.Char:
					if (strpos < string_end && (str [strpos] == program [pc + 1])) {
						strpos++;
						pc += 2;
						continue;
					}
					return false;
				case RxOp.NoChar:
					if (strpos < string_end && (str [strpos] != program [pc + 1])) {
						strpos++;
						pc += 2;
						continue;
					}
					return false;
				case RxOp.CharIgnoreCase:
					if (strpos < string_end && (Char.ToLower (str [strpos]) == program [pc + 1])) {
						strpos++;
						pc += 2;
						continue;
					}
					return false;
				case RxOp.NoCharIgnoreCase:
					if (strpos < string_end && (Char.ToLower (str [strpos]) != program [pc + 1])) {
						strpos++;
						pc += 2;
						continue;
					}
					return false;
				case RxOp.Range:
					if (strpos < string_end) {
						int c = str [strpos];
						if (c >= program [pc + 1] && c <= program [pc + 2]) {
							strpos++;
							pc += 3;
							continue;
						}
					}
					return false;
				case RxOp.NoRange:
					if (strpos < string_end) {
						int c = str [strpos];
						if (c >= program [pc + 1] && c <= program [pc + 2])
							return false;
						strpos++;
						pc += 3;
						continue;
					}
					return false;
				case RxOp.RangeIgnoreCase:
					if (strpos < string_end) {
						int c = Char.ToLower (str [strpos]);
						if (c >= program [pc + 1] && c <= program [pc + 2]) {
							strpos++;
							pc += 3;
							continue;
						}
					}
					return false;
				case RxOp.NoRangeIgnoreCase:
					if (strpos < string_end) {
						int c = Char.ToLower (str [strpos]);
						if (c >= program [pc + 1] && c <= program [pc + 2])
							return false;
						strpos++;
						pc += 3;
						continue;
					}
					return false;
				case RxOp.Bitmap:
					if (strpos < string_end) {
						int c = str [strpos];
						c -= program [pc + 1];
						length =  program [pc + 2];
						if (c < 0 || c >= (length << 3))
							return false;
						pc += 3;
						if ((program [pc + (c >> 3)] & (1 << (c & 0x7))) != 0) {
							strpos++;
							pc += length;
							continue;
						}
					}
					return false;
				case RxOp.BitmapIgnoreCase:
					if (strpos < string_end) {
						int c = Char.ToLower (str [strpos]);
						c -= program [pc + 1];
						length =  program [pc + 2];
						if (c < 0 || c >= (length << 3))
							return false;
						pc += 3;
						if ((program [pc + (c >> 3)] & (1 << (c & 0x7))) != 0) {
							strpos++;
							pc += length;
							continue;
						}
					}
					return false;
				case RxOp.CategoryAny:
					if (strpos < string_end && str [strpos] != '\n') {
						strpos++;
						pc++;
						continue;
					}
					return false;
				case RxOp.Branch: {
					int res = 0;
					if (EvalByteCode (pc + 3, strpos, ref res)) {
						strpos_result = res;
						return true;
					}
					//Console.WriteLine ("branch offset: {0}", program [pc + 1] | (program [pc + 2] << 8));
					pc += program [pc + 1] | (program [pc + 2] << 8);
					continue;
				}
				case RxOp.Repeat:
				case RxOp.RepeatLazy: {
					int res = 0;
					start = ReadInt (program, pc + 3);
					end = ReadInt (program, pc + 7);
					//Console.WriteLine ("min: {0}, max: {1}", start, end);
					length = 0;
					while (length < end) {
						if (!EvalByteCode (pc + 11, strpos, ref res)) {
							if (length >= start) {
								goto repeat_success;
							}
							return false;
						}
						strpos = res;
						length++;
					}
					if (length != end)
						return false;
				repeat_success:
					pc += program [pc + 1] | (program [pc + 2] << 8);
					continue;
				}
				default:
					Console.WriteLine ("evaluating: {0} at pc: {1}, strpos: {2}", (RxOp)program [pc], pc, strpos);
					throw new NotSupportedException ();
				}
			}
		}
	}
}

