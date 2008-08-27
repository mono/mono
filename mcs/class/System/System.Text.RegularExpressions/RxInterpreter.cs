
using System;
using System.Collections;
using System.Globalization;

namespace System.Text.RegularExpressions {

	internal delegate bool EvalDelegate (RxInterpreter interp, int strpos, ref int strpos_result);

	sealed class RxInterpreter: BaseMachine {
		byte[] program;
		string str;
		int string_start;
		int string_end;
		int group_count;
//		int match_start;
		int[] groups;
		EvalDelegate eval_del; // optimized EvalByteCode method created by the CILCompiler

		Mark[] marks = null; // mark stack
		int mark_start; // start of current checkpoint
		int mark_end; // end of checkpoint/next free mark

		static readonly bool trace_rx = Environment.GetEnvironmentVariable ("RXD") != null;

		static int ReadInt (byte[] code, int pc)
		{
			int val = code [pc];
			val |= code [pc + 1] << 8;
			val |= code [pc + 2] << 16;
			val |= code [pc + 3] << 24;
			return val;
		}

		public RxInterpreter (byte[] program, EvalDelegate eval_del)
		{
			this.program = program;
			this.eval_del = eval_del;
			group_count = 1 + (program [1] | (program [2] << 8));
			groups = new int [group_count];

			ResetGroups ();
		}

		public override Match Scan (Regex regex, string text, int start, int end) {
			str = text;
			string_start = start;
			string_end = end;
			int res = 0;

			bool match;
			if (eval_del != null) {
				match = eval_del (this, start, ref res);
			} else {
				match = EvalByteCode (11, start, ref res);
			}
			marks [groups [0]].End = res;
			if (match) {
				return GenerateMatch (regex);
				//Match m = new Match (regex, this, text, end, 0, match_start, res - match_start);
				//return m;
			}
			return Match.Empty;
		}

		// capture management
		private void Open (int gid, int ptr) {
			int m = groups [gid];
			if (m < mark_start || marks [m].IsDefined) {
				m = CreateMark (m);
				groups [gid] = m;
			}

			marks [m].Start = ptr;
		}

		private void Close (int gid, int ptr) {
	       		marks [groups [gid]].End = ptr;
		}
/*
		private bool Balance (int gid, int balance_gid, bool capture, int ptr) {
			int b = groups [balance_gid];

			if (b == -1 || marks [b].Index < 0) {
				//Group not previously matched
				return false;
			}
			if (gid > 0 && capture){ 
				Open (gid, marks [b].Index + marks [b].Length);
				Close (gid, ptr);
			}

			groups [balance_gid] = marks[b].Previous;
			return true;
		}
*/
		private int Checkpoint () {
			mark_start = mark_end;
			return mark_start;
		}

		private void Backtrack (int cp) {
			for (int i = 0; i < groups.Length; ++ i) {
				int m = groups [i];
				while (cp <= m)
					m = marks [m].Previous;
				groups [i] = m;
			}
		}

		private void ResetGroups () {
			int n = groups.Length;
			if (marks == null)
				marks = new Mark [n * 10];

			for (int i = 0; i < n; ++ i) {
				groups [i] = i;

				marks [i].Start = -1;
				marks [i].End = -1;
				marks [i].Previous = -1;
			}
			mark_start = 0;
			mark_end = n;
		}

		private int GetLastDefined (int gid) {
			int m = groups [gid];
			while (m >= 0 && !marks [m].IsDefined)
				m = marks [m].Previous;

			return m;
		}

		private int CreateMark (int previous) {
			if (mark_end == marks.Length) {
				Mark [] dest = new Mark [marks.Length * 2];
				marks.CopyTo (dest, 0);
				marks = dest;
			}

			int m = mark_end ++;
			marks [m].Start = marks [m].End = -1;
			marks [m].Previous = previous;

			return m;
		}

		private void GetGroupInfo (int gid, out int first_mark_index, out int n_caps)
		{
			first_mark_index = -1;
			n_caps = 0;
			for (int m = groups [gid]; m >= 0; m = marks [m].Previous) {
				if (!marks [m].IsDefined)
					continue;
				if (first_mark_index < 0)
					first_mark_index = m;
				++n_caps;
			}
		}

		private void PopulateGroup (Group g, int first_mark_index, int n_caps)
		{
			int i = 1;
			for (int m = marks [first_mark_index].Previous; m >= 0; m = marks [m].Previous) {
				if (!marks [m].IsDefined)
					continue;
				Capture cap = new Capture (str, marks [m].Index, marks [m].Length);
				g.Captures.SetValue (cap, n_caps - 1 - i);
				++i;
			}
		}

		private Match GenerateMatch (Regex regex)
		{
			int n_caps, first_mark_index;
			Group g;
			GetGroupInfo (0, out first_mark_index, out n_caps);

			// Avoid fully populating the Match instance if not needed
			if (!needs_groups_or_captures)
				return new Match (regex, this, str, string_end, 0, marks [first_mark_index].Index, marks [first_mark_index].Length);

			Match retval = new Match (regex, this, str, string_end, groups.Length, 
						  marks [first_mark_index].Index, marks [first_mark_index].Length, n_caps);
			PopulateGroup (retval, first_mark_index, n_caps);

			for (int gid = 1; gid < groups.Length; ++ gid) {
				GetGroupInfo (gid, out first_mark_index, out n_caps);
				if (first_mark_index < 0) {
					g = Group.Fail;
				} else {
					g = new Group (str, marks [first_mark_index].Index, marks [first_mark_index].Length, n_caps);
					PopulateGroup (g, first_mark_index, n_caps);
				}
				retval.Groups.SetValue (g, gid);
			}
			return retval;
		}

		// used by the IL backend
		void SetStartOfMatch (int pos)
		{
			marks [groups [0]].Start = pos;
		}

		static bool IsWordChar (char c)
		{
			return Char.IsLetterOrDigit (c) || Char.GetUnicodeCategory (c) == UnicodeCategory.ConnectorPunctuation;
		}

		bool EvalByteCode (int pc, int strpos, ref int strpos_result)
		{
			// luckily the IL engine can deal with char_group_end at compile time
			// this code offset needs to be checked only in opcodes that handle
			// a single char and that are included in a TestCharGroup expression:
			// the engine is supposed to jump to this offset as soons as the
			// first opcode in the expression matches
			// The code pattern becomes:
			// on successfull match: check if char_group_end is nonzero and jump to
			// test_char_group_passed after adjusting strpos
			// on failure: try the next expression by simply advancing pc
			int char_group_end = 0;
			int length, start, end;
			while (true) {
				if (trace_rx)
					Console.WriteLine ("evaluating: {0} at pc: {1}, strpos: {2}, cge: {3}", (RxOp)program [pc], pc, strpos, char_group_end);
				switch ((RxOp)program [pc]) {
				case RxOp.True:
					if (char_group_end != 0) {
						pc = char_group_end;
						char_group_end = 0;
						continue;
					}
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
				case RxOp.WordBoundary:
					if (string_end == 0)
						return false;
					if (strpos == 0) {
						if (IsWordChar (str [strpos])) {
							pc++;
							continue;
						}
					} else if (strpos == string_end) {
						if (IsWordChar (str [strpos - 1])) {
							pc++;
							continue;
						}
					} else {
						if (IsWordChar (str [strpos]) != IsWordChar (str [strpos - 1])) {
							pc++;
							continue;
						}
					}
					return false;
				case RxOp.NoWordBoundary:
					if (string_end == 0)
						return false;
					if (strpos == 0) {
						if (!IsWordChar (str [strpos])) {
							pc++;
							continue;
						}
					} else if (strpos == string_end) {
						if (!IsWordChar (str [strpos - 1])) {
							pc++;
							continue;
						}
					} else {
						if (IsWordChar (str [strpos]) == IsWordChar (str [strpos - 1])) {
							pc++;
							continue;
						}
					}
					return false;
				case RxOp.Anchor:
					// FIXME: test anchor
					length = program [pc + 3] | (program [pc + 4] << 8);
					pc += program [pc + 1] | (program [pc + 2] << 8);
					// it's important to test also the end of the string
					// position for things like: "" =~ /$/
					end = string_end + 1;
					while (strpos < end) {
						int res = strpos;
						if (groups.Length > 1) {
							ResetGroups ();
							marks [groups [0]].Start = strpos;
						}
						if (EvalByteCode (pc, strpos, ref res)) {
//							match_start = strpos;
							marks [groups [0]].Start = strpos;
							if (groups.Length > 1)
								marks [groups [0]].End = res;
							strpos_result = res;
							return true;
						}
						strpos++;
					}
					return false;
				case RxOp.Reference:
					length = GetLastDefined (program [pc + 1] | (program [pc + 2] << 8));
					if (length < 0)
						return false;
					start = marks [length].Index;
					length = marks [length].Length;
					if (strpos + length > string_end)
						return false;
					for (end = start + length; start < end; ++start) {
						if (str [strpos] != str [start])
							return false;
						strpos++;
					}
					pc += 3;
					continue;
				case RxOp.ReferenceIgnoreCase:
					length = GetLastDefined (program [pc + 1] | (program [pc + 2] << 8));
					if (length < 0)
						return false;
					start = marks [length].Index;
					length = marks [length].Length;
					if (strpos + length > string_end)
						return false;
					for (end = start + length; start < end; ++start) {
						if (str [strpos] != str [start] && Char.ToLower (str [strpos]) != Char.ToLower (str [start]))
							return false;
						strpos++;
					}
					pc += 3;
					continue;
				case RxOp.IfDefined:
					if (GetLastDefined (program [pc + 3] | (program [pc + 4] << 8)) < 0)
						pc += 5;
					else
						pc += program [pc + 1] | (program [pc + 2] << 8);
					continue;
				case RxOp.SubExpression: {
					int res = 0;
					if (EvalByteCode (pc + 3, strpos, ref res)) {
						pc += program [pc + 1] | (program [pc + 2] << 8);
						continue;
					}
					return false;
				}
				case RxOp.Test: {
					int res = 0;
					// FIXME: checkpoint
					if (EvalByteCode (pc + 5, strpos, ref res)) {
						pc += program [pc + 1] | (program [pc + 2] << 8);
					} else {
						pc += program [pc + 3] | (program [pc + 4] << 8);
					}
					continue;
				}
				case RxOp.OpenGroup:
					Open (program [pc + 1] | (program [pc + 2] << 8), strpos);
					pc += 3;
					continue;
				case RxOp.CloseGroup:
					Close (program [pc + 1] | (program [pc + 2] << 8), strpos);
					pc += 3;
					continue;
				case RxOp.Jump:
					pc += program [pc + 1] | (program [pc + 2] << 8);
					continue;
				case RxOp.TestCharGroup:
					char_group_end = pc + program [pc + 1] | (program [pc + 2] << 8);
					pc += 3;
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
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc += 2;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				case RxOp.NoChar:
					if (strpos < string_end && (str [strpos] != program [pc + 1])) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc += 2;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				case RxOp.CharIgnoreCase:
					if (strpos < string_end && (Char.ToLower (str [strpos]) == program [pc + 1])) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc += 2;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				case RxOp.NoCharIgnoreCase:
					if (strpos < string_end && (Char.ToLower (str [strpos]) != program [pc + 1])) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc += 2;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				case RxOp.Range:
					if (strpos < string_end) {
						int c = str [strpos];
						if (c >= program [pc + 1] && c <= program [pc + 2]) {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 3;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 3;
					continue;
				case RxOp.NoRange:
					if (strpos < string_end) {
						int c = str [strpos];
						if (c >= program [pc + 1] && c <= program [pc + 2]) {
							if (char_group_end == 0)
								return false;
							pc += 3;
							continue;
						}
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc += 3;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 3;
					continue;
				case RxOp.RangeIgnoreCase:
					if (strpos < string_end) {
						int c = Char.ToLower (str [strpos]);
						if (c >= program [pc + 1] && c <= program [pc + 2]) {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 3;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 3;
					continue;
				case RxOp.NoRangeIgnoreCase:
					if (strpos < string_end) {
						int c = Char.ToLower (str [strpos]);
						if (c >= program [pc + 1] && c <= program [pc + 2]) {
							if (char_group_end == 0)
								return false;
							pc += 3;
							continue;
						}
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc += 3;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 3;
					continue;
				case RxOp.Bitmap:
					if (strpos < string_end) {
						int c = str [strpos];
						c -= program [pc + 1];
						length = program [pc + 2];
						if (c < 0 || c >= (length << 3)) {
							if (char_group_end == 0)
								return false;
							pc += length + 3;
							continue;
						}
						pc += 3;
						if ((program [pc + (c >> 3)] & (1 << (c & 0x7))) != 0) {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += length;
							continue;
						}
						if (char_group_end == 0)
							return false;
						pc += length;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 3 + program [pc + 2];
					continue;
				case RxOp.NoBitmap:
					if (strpos < string_end) {
						int c = str [strpos];
						c -= program [pc + 1];
						length = program [pc + 2];
						if (c < 0 || c >= (length << 3)) {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 3 + length;
							continue;
						}
						pc += 3;
						if ((program [pc + (c >> 3)] & (1 << (c & 0x7))) != 0) {
							if (char_group_end == 0)
								return false;
							pc += length;
							continue;
						} else {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += length;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 3 + program [pc + 2];
					continue;
				case RxOp.BitmapIgnoreCase:
					if (strpos < string_end) {
						int c = Char.ToLower (str [strpos]);
						c -= program [pc + 1];
						length = program [pc + 2];
						if (c < 0 || c >= (length << 3)) {
							if (char_group_end == 0)
								return false;
							pc += length + 3;
							continue;
						}
						pc += 3;
						if ((program [pc + (c >> 3)] & (1 << (c & 0x7))) != 0) {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += length;
							continue;
						}
						if (char_group_end == 0)
							return false;
						pc += length;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 3 + program [pc + 2];
					continue;
				case RxOp.NoBitmapIgnoreCase:
					if (strpos < string_end) {
						int c = str [strpos];
						c -= program [pc + 1];
						length =  program [pc + 2];
						pc += 3;
						if (c < 0 || c >= (length << 3)) {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += length;
							continue;
						}
						if ((program [pc + (c >> 3)] & (1 << (c & 0x7))) != 0) {
							if (char_group_end == 0)
								return false;
							pc += length;
							continue;
						} else {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += length;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 3 + program [pc + 2];
					continue;
				case RxOp.UnicodeChar:
					if (strpos < string_end && (str [strpos] == (program [pc + 1] | (program [pc + 2] << 8)))) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc += 3;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 3;
					continue;
				case RxOp.NoUnicodeChar:
					if (strpos < string_end && (str [strpos] != (program [pc + 1] | (program [pc + 2] << 8)))) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc += 3;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 3;
					continue;
				case RxOp.UnicodeCharIgnoreCase:
					if (strpos < string_end && (Char.ToLower (str [strpos]) == (program [pc + 1] | (program [pc + 2] << 8)))) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc += 3;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 3;
					continue;
				case RxOp.NoUnicodeCharIgnoreCase:
					if (strpos < string_end && (Char.ToLower (str [strpos]) != (program [pc + 1] | (program [pc + 2] << 8)))) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc += 3;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 3;
					continue;
				case RxOp.CategoryAny:
					if (strpos < string_end && str [strpos] != '\n') {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc++;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.CategoryAnySingleline:
					if (strpos < string_end) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc++;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.CategoryWord:
					if (strpos < string_end) {
						char c = str [strpos];
						if (Char.IsLetterOrDigit (c) || Char.GetUnicodeCategory (c) == UnicodeCategory.ConnectorPunctuation) {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc++;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.NoCategoryWord:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!Char.IsLetterOrDigit (c) && Char.GetUnicodeCategory (c) != UnicodeCategory.ConnectorPunctuation) {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc++;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.CategoryDigit:
					if (strpos < string_end && Char.IsDigit (str [strpos])) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc++;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.NoCategoryDigit:
					if (strpos < string_end && !Char.IsDigit (str [strpos])) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc++;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.CategoryWhiteSpace:
					if (strpos < string_end && Char.IsWhiteSpace (str [strpos])) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc++;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.NoCategoryWhiteSpace:
					if (strpos < string_end && !Char.IsWhiteSpace (str [strpos])) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc++;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.CategoryEcmaWord:
					if (strpos < string_end) {
						int c = str [strpos];
						if ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || '0' <= c && c <= '9' || c == '_') {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc++;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.NoCategoryEcmaWord:
					if (strpos < string_end) {
						int c = str [strpos];
						if ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || '0' <= c && c <= '9' || c == '_') {
							if (char_group_end == 0)
								return false;
							pc++;
							continue;
						}
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc++;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.CategoryEcmaWhiteSpace:
					if (strpos < string_end) {
						int c = str [strpos];
						if (c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v') {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc++;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.NoCategoryEcmaWhiteSpace:
					if (strpos < string_end) {
						int c = str [strpos];
						if (c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v') {
							if (char_group_end == 0)
								return false;
							pc++;
							continue;
						}
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc++;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.CategoryUnicodeSpecials:
					if (strpos < string_end) {
						int c = str [strpos];
						if ('\uFEFF' <= c && c <= '\uFEFF' || '\uFFF0' <= c && c <= '\uFFFD') {
							strpos++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc++;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.NoCategoryUnicodeSpecials:
					if (strpos < string_end) {
						int c = str [strpos];
						if ('\uFEFF' <= c && c <= '\uFEFF' || '\uFFF0' <= c && c <= '\uFFFD') {
							if (char_group_end == 0)
								return false;
							pc++;
							continue;
						}
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc++;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc++;
					continue;
				case RxOp.CategoryUnicode:
					if (strpos < string_end && Char.GetUnicodeCategory (str [strpos]) == (UnicodeCategory)program [pc + 1]) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc += 2;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				case RxOp.NoCategoryUnicode:
					if (strpos < string_end && Char.GetUnicodeCategory (str [strpos]) != (UnicodeCategory)program [pc + 1]) {
						strpos++;
						if (char_group_end != 0)
							goto test_char_group_passed;
						pc += 2;
						continue;
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
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
					int cp = Checkpoint ();
					while (length < end) {
						if (!EvalByteCode (pc + 11, strpos, ref res)) {
							if (length >= start) {
								goto repeat_success;
							}
							return false;
						}
						strpos = res;
						length++;
						Backtrack (cp);
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
				continue;
			test_char_group_passed:
				pc = char_group_end;
				char_group_end = 0;
				continue;
			} // end of while (true)
		}
	}
}

