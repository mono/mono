
// Based upon interpreter.cs, written by Dan Lewis (dlewis@gmx.co.uk)
//
// There are a couple of bits flagged with DEAD_CODE which are bits that do
// not seem to have been completed
//
using System;
using System.Collections;
using System.Globalization;
using System.Diagnostics;

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

		IntStack stack; // utility stack

		RepeatContext repeat;	// current repeat context
		RepeatContext deep;		// points to the most-nested repeat context

		/* The readonly ensures the JIT can optimize out if (trace_rx) statements */
		public static readonly bool trace_rx =
#if !NET_2_1
			Environment.GetEnvironmentVariable ("MONO_TRACE_RX") != null;
#else
			false;
#endif

		// private classes

		internal struct IntStack {
			int [] values;
			int count;
			public int Pop ()
			{
				return values [--count];
			}
			public void Push (int value)
			{
				if (values == null) {
					values = new int [8];
				} else if (count == values.Length) {
					int new_size = values.Length;
					new_size += new_size >> 1;
					int [] new_values = new int [new_size];
					for (int i = 0; i < count; ++i)
						new_values [i] = values [i];
					values = new_values;
				}
				values [count++] = value;
			}
			public int Top {
				get { return values [count - 1]; }
			}
			public int Count {
				get { return count; }
				set {
					if (value > count)
						throw new SystemException ("can only truncate the stack");
					count = value;
				}
			}
		}

		private class RepeatContext {
			public RepeatContext (RepeatContext previous, int min, int max, bool lazy, int expr_pc) {
				this.previous = previous;
				this.min = min;
				this.max = max;
				this.lazy = lazy;
				this.expr_pc = expr_pc;
				
				this.start = -1;
				this.count = 0;
			}

			public int Count {
				get { return count; }
				set { count = value; }
			}

			public int Start {
				get { return start; }
				set { start = value; }
			}

			public bool IsMinimum {
				get { return min <= count; }
			}

			public bool IsMaximum {
				get { return max <= count; }
			}

			public bool IsLazy {
				get { return lazy; }
			}

			public int Expression {
				get { return expr_pc; }
			}

			public RepeatContext Previous {
				get { return previous; }
			}
		
			private int start;
			private int min, max;
			private bool lazy;
			private int expr_pc;
			private RepeatContext previous;

			private int count;
		}

		static int ReadInt (byte[] code, int pc)
		{
			int val = code [pc];
			val |= (int)code [pc + 1] << 8;
			val |= (int)code [pc + 2] << 16;
			val |= (int)code [pc + 3] << 24;
			return val;
		}

		public RxInterpreter (byte[] program, EvalDelegate eval_del)
		{
			this.program = program;
			this.eval_del = eval_del;
			group_count = 1 + (program [1] | ((int)program [2] << 8));
			groups = new int [group_count];
			stack = new IntStack ();

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

		private bool Balance (int gid, int balance_gid, bool capture, int ptr) {
			int b = groups [balance_gid];

			if(b == -1 || marks[b].Index < 0) {
				//Group not previously matched
				return false;
			}
			Debug.Assert (marks [b].IsDefined, "Regex", "Balancng group not closed");
			if (gid > 0 && capture){ 
				Open (gid, marks [b].Index + marks [b].Length);
				Close (gid, ptr);
			}

			groups [balance_gid] = marks[b].Previous;

			return true;
		}

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
				marks = new Mark [n];

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
	    internal void SetStartOfMatch (int pos)
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
				if (trace_rx) {
					Console.WriteLine ("evaluating: {0} at pc: {1}, strpos: {2}, cge: {3}", (RxOp)program [pc], pc, strpos, char_group_end);
					//Console.WriteLine ("deep: " + (deep == null ? 0 : deep.GetHashCode ()) + " repeat: " + (this.repeat == null ? 0 : this.repeat.GetHashCode ()));
				}
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
					int skip = program [pc + 1] | ((int)program [pc + 2] << 8);
					int anch_offset = program [pc + 3] | ((int)program [pc + 4] << 8);

					/*
					 * In the general case, we have to evaluate the bytecode
					 * starting at pc + skip, however the optimizer emits some
					 * special cases, whose bytecode begins at pc + 5.
					 */
					int anch_pc = pc + 5;
					RxOp anch_op = (RxOp)(program[anch_pc] & 0x00ff);

					bool spec_anch = false;

					// FIXME: Add more special cases from interpreter.cs
					if (anch_op == RxOp.String || anch_op == RxOp.StringIgnoreCase) {
						if (pc + skip == anch_pc + 2 + program [anch_pc + 1] + 1) {
							// Anchor
							//	String
							//	True
							spec_anch = true;
							if (trace_rx)
								Console.WriteLine ("  string anchor at {0}, offset {1}", anch_pc, anch_offset);
						}
					}

					pc += skip;

					if ((RxOp)program [pc] == RxOp.StartOfString) {
						if (strpos == 0) {
							int res = strpos;
							if (groups.Length > 1) {
								ResetGroups ();
								marks [groups [0]].Start = strpos;
							}
							if (EvalByteCode (pc + 1, strpos, ref res)) {
								marks [groups [0]].Start = strpos;
								if (groups.Length > 1)
									marks [groups [0]].End = res;
								strpos_result = res;
								return true;
							}
						}
						return false;
					}

					// it's important to test also the end of the string
					// position for things like: "" =~ /$/
					end = string_end + 1;
					while (strpos < end) {
						if (spec_anch) {
							if (anch_op == RxOp.String || anch_op == RxOp.StringIgnoreCase) {
								/* 
								 * This means the match must contain a given
								 * string at a constant position, so we can skip 
								 * forward until the string matches. This is a win if
								 * the rest of the regex 
								 * has a complex positive lookbehind for example.
								 */
								int tmp_res = strpos;
								if (!EvalByteCode (anch_pc, strpos + anch_offset, ref tmp_res)) {
									strpos ++;
									continue;
								}
							}
						}
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
				case RxOp.AnchorReverse:
					length = program [pc + 3] | ((int)program [pc + 4] << 8);
					pc += program [pc + 1] | ((int)program [pc + 2] << 8);
					// it's important to test also the end of the string
					// position for things like: "" =~ /$/
					end = 0;
					while (strpos >= 0) {
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
						strpos--;
					}
					return false;
				case RxOp.Reference:
					length = GetLastDefined (program [pc + 1] | ((int)program [pc + 2] << 8));
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
					length = GetLastDefined (program [pc + 1] | ((int)program [pc + 2] << 8));
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
				case RxOp.ReferenceReverse: {
					length = GetLastDefined (program [pc + 1] | ((int)program [pc + 2] << 8));
					if (length < 0)
						return false;
					start = marks [length].Index;
					length = marks [length].Length;
					if (strpos - length < 0)
						return false;
					int p = strpos - length;
					for (end = start + length; start < end; ++start, ++p) {
						if (str [p] != str [start])
							return false;
					}
					strpos -= length;
					pc += 3;
					continue;
				}
				case RxOp.IfDefined:
					if (GetLastDefined (program [pc + 3] | ((int)program [pc + 4] << 8)) >= 0)
						pc += 5;
					else
						pc += program [pc + 1] | ((int)program [pc + 2] << 8);
					continue;
				case RxOp.SubExpression: {
					int res = 0;
					if (EvalByteCode (pc + 3, strpos, ref res)) {
						pc += program [pc + 1] | ((int)program [pc + 2] << 8);
						strpos = res;
						continue;
					}
					return false;
				}
				case RxOp.Test: {
					int res = 0;
					// FIXME: checkpoint
					if (EvalByteCode (pc + 5, strpos, ref res)) {
						pc += program [pc + 1] | ((int)program [pc + 2] << 8);
					} else {
						pc += program [pc + 3] | ((int)program [pc + 4] << 8);
					}
					continue;
				}
				case RxOp.OpenGroup:
					Open (program [pc + 1] | ((int)program [pc + 2] << 8), strpos);
					pc += 3;
					continue;
				case RxOp.CloseGroup:
					Close (program [pc + 1] | ((int)program [pc + 2] << 8), strpos);
					pc += 3;
					continue;
				case RxOp.BalanceStart: {
					int res = 0;

					if (!EvalByteCode (pc + 8, strpos, ref res))
						goto Fail;

					int gid = program [pc + 1] | ((int)program [pc + 2] << 8);
					int balance_gid = program [pc + 3] | ((int)program [pc + 4] << 8);
					bool capture = program [pc + 5] > 0;
					if (!Balance (gid, balance_gid, capture, strpos))
						goto Fail;

					strpos = res;					
					pc += program[pc + 6] | ((int)program [pc + 7] << 8);
					break;
				}
				case RxOp.Balance: {
					goto Pass;
				}

				case RxOp.Jump:
					pc += program [pc + 1] | ((int)program [pc + 2] << 8);
					continue;
				case RxOp.TestCharGroup:
					char_group_end = pc + (program [pc + 1] | ((int)program [pc + 2] << 8));
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
				case RxOp.StringReverse: {
					start = pc + 2;
					length = program [pc + 1];
					if (strpos < length)
						return false;
					int p = strpos - length;
					end = start + length;
					for (; start < end; ++start, ++p) {
						if (str [p] != program [start])
							return false;
					}
					strpos -= length;
					pc = end;
					continue;
				}
				case RxOp.StringIgnoreCaseReverse: {
					start = pc + 2;
					length = program [pc + 1];
					if (strpos < length)
						return false;
					int p = strpos - length;
					end = start + length;
					for (; start < end; ++start, ++p) {
						if (str [p] != program [start] && Char.ToLower (str [p]) != program [start])
							return false;
					}
					strpos -= length;
					pc = end;
					continue;
				}
				case RxOp.UnicodeString: {
					start = pc + 3;
					length = program [pc + 1] | ((int)program [pc + 2] << 8);
					if (strpos + length > string_end)
						return false;
					end = start + length * 2;
					for (; start < end; start += 2) {
						int c = program [start] | ((int)program [start + 1] << 8);
						if (str [strpos] != c)
							return false;
						strpos++;
					}
					pc = end;
					continue;
				}
				case RxOp.UnicodeStringIgnoreCase: {
					start = pc + 3;
					length = program [pc + 1] | ((int)program [pc + 2] << 8);
					if (strpos + length > string_end)
						return false;
					end = start + length * 2;
					for (; start < end; start += 2) {
						int c = program [start] | ((int)program [start + 1] << 8);
						if (str [strpos] != c && Char.ToLower (str [strpos]) != c)
							return false;
						strpos++;
					}
					pc = end;
					continue;
				}
				case RxOp.UnicodeStringReverse: {
					start = pc + 3;
					length = program [pc + 1] | ((int)program [pc + 2] << 8);
					if (strpos < length)
						return false;
					int p = strpos - length;
					end = start + length * 2;
					for (; start < end; start += 2, p += 2) {
						int c = program [start] | ((int)program [start + 1] << 8);
						if (str [p] != c)
							return false;
					}
					strpos -= length;
					pc = end;
					continue;
				}
				case RxOp.UnicodeStringIgnoreCaseReverse: {
					start = pc + 3;
					length = program [pc + 1] | ((int)program [pc + 2] << 8);
					if (strpos < length)
						return false;
					int p = strpos - length;
					end = start + length * 2;
					for (; start < end; start += 2, p += 2) {
						int c = program [start] | ((int)program [start + 1] << 8);
						if (str [p] != c && Char.ToLower (str [p]) != c)
							return false;
					}
					strpos -= length;
					pc = end;
					continue;
				}

					/*
					 * The opcodes below are basically specialized versions of one 
					 * generic opcode, which has three parameters:
					 * - reverse (Reverse), revert (No), ignore-case (IgnoreCase)
					 * Thus each opcode has 8 variants.
					 * FIXME: Maybe move all unusual variations 
					 * (Reverse+IgnoreCase+Unicode) into a generic GenericChar opcode 
					 * like in the old interpreter.
					 * FIXME: Move all the Reverse opcodes to a separate method.
					 */
#if FALSE
					if (!reverse) {
						if (strpos < string_end && (COND (str [strpos]))) {
							if (!revert) {
								strpos ++;
								if (char_group_end != 0)
									goto test_char_group_passed;
								pc += ins_len;
								continue;
							} else {
								/*
								 * If we are inside a char group, the cases are ANDed 
								 * together, so we have to continue checking the
								 * other cases, and we need to increase strpos after 
								 * the final check.
								 * The char group is termined by a True, hence the
								 * + 1 below.
								 * FIXME: Optimize this.
								 */
								pc += ins_len;
								if (char_group_end == 0 || (pc + 1 == char_group_end))
									strpos ++;
								if (pc + 1 == char_group_end)
									goto test_char_group_passed;
								continue;
							}
						} else {
							if (!revert) {
								if (char_group_end == 0)
									return false;
								pc += ins_len;
								continue;
							} else {
								/* Fail both inside and outside a char group */
								return false;
							}
						}
					} else {
						// Same as above, but use:
						// - strpos > 0 instead of strpos < string_len
						// - COND (str [strpos - 1]) instead of COND (str [strpos])
						// - strpos -- instead of strpos ++
					}
#endif
				// GENERATED BY gen-interp.cs, DO NOT MODIFY
				
				/* Char */
				
				case RxOp.Char:
					if (strpos < string_end) {
						char c = str [strpos];
						if (((c == program [pc + 1]))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 2;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				
				/* Range */
				
				case RxOp.Range:
					if (strpos < string_end) {
						char c = str [strpos];
						if (((c >= program [pc + 1] && c <= program [pc + 2]))) {
							strpos ++;
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
				
				/* UnicodeRange */
				
				case RxOp.UnicodeRange:
					if (strpos < string_end) {
						char c = str [strpos];
						if (((c >= (program [pc + 1] | ((int)program [pc + 2] << 8))) && (c <= (program [pc + 3] | ((int)program [pc + 4] << 8))))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 5;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 5;
					continue;
				
				/* UnicodeChar */
				
				case RxOp.UnicodeChar:
					if (strpos < string_end) {
						char c = str [strpos];
						if (((c == (program [pc + 1] | ((int)program [pc + 2] << 8))))) {
							strpos ++;
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
				
				/* CategoryAny */
				
				case RxOp.CategoryAny:
					if (strpos < string_end) {
						char c = str [strpos];
						if (((c != '\n'))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				
				/* CategoryAnySingleline */
				
				case RxOp.CategoryAnySingleline:
					if (strpos < string_end) {
						// char c = str [strpos];
						if ((true)) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				
				/* CategoryWord */
				
				case RxOp.CategoryWord:
					if (strpos < string_end) {
						char c = str [strpos];
						if (((Char.IsLetterOrDigit (c) || Char.GetUnicodeCategory (c) == UnicodeCategory.ConnectorPunctuation))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				
				/* CategoryDigit */
				
				case RxOp.CategoryDigit:
					if (strpos < string_end) {
						char c = str [strpos];
						if (((Char.IsDigit (c)))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				
				/* CategoryWhiteSpace */
				
				case RxOp.CategoryWhiteSpace:
					if (strpos < string_end) {
						char c = str [strpos];
						if (((Char.IsWhiteSpace (c)))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				
				/* CategoryEcmaWord */
				
				case RxOp.CategoryEcmaWord:
					if (strpos < string_end) {
						char c = str [strpos];
						if ((('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || '0' <= c && c <= '9' || c == '_'))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				
				/* CategoryEcmaWhiteSpace */
				
				case RxOp.CategoryEcmaWhiteSpace:
					if (strpos < string_end) {
						char c = str [strpos];
						if (((c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v'))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				
				/* CategoryUnicodeSpecials */
				
				case RxOp.CategoryUnicodeSpecials:
					if (strpos < string_end) {
						char c = str [strpos];
						if ((('\uFEFF' <= c && c <= '\uFEFF' || '\uFFF0' <= c && c <= '\uFFFD'))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				
				/* CategoryUnicode */
				
				case RxOp.CategoryUnicode:
					if (strpos < string_end) {
						char c = str [strpos];
						if (((Char.GetUnicodeCategory (c) == (UnicodeCategory)program [pc + 1]))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 2;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				
				/* CategoryGeneral */
				
				case RxOp.CategoryGeneral:
					if (strpos < string_end) {
						char c = str [strpos];
						if (((CategoryUtils.IsCategory ((Category)program [pc + 1], c)))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 2;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				
				/* Bitmap */
				
				case RxOp.Bitmap:
					if (strpos < string_end) {
						char c = str [strpos];
						int c2 = (int)c; c2 -= program [pc + 1]; length = program [pc + 2];
						if (((c2 >= 0 && c2 < (length << 3) && (program [pc + 3 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 3 + program [pc + 2];
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 3 + program [pc + 2];
					continue;
				
				/* UnicodeBitmap */
				
				case RxOp.UnicodeBitmap:
					if (strpos < string_end) {
						char c = str [strpos];
						int c2 = (int)c; c2 -= (program [pc + 1] | ((int)program [pc + 2] << 8)); length = (program [pc + 3] | ((int)program [pc + 4] << 8));
						if (((c2 >= 0 && c2 < (length << 3) && (program [pc + 5 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 5 + (program [pc + 3] | ((int)program [pc + 4] << 8));
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 5 + (program [pc + 3] | ((int)program [pc + 4] << 8));
					continue;
				case RxOp.CharIgnoreCase:
					if (strpos < string_end) {
						char c = Char.ToLower (str [strpos]);
						if (((c == program [pc + 1]))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 2;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				case RxOp.RangeIgnoreCase:
					if (strpos < string_end) {
						char c = Char.ToLower (str [strpos]);
						if (((c >= program [pc + 1] && c <= program [pc + 2]))) {
							strpos ++;
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
				case RxOp.UnicodeRangeIgnoreCase:
					if (strpos < string_end) {
						char c = Char.ToLower (str [strpos]);
						if (((c >= (program [pc + 1] | ((int)program [pc + 2] << 8))) && (c <= (program [pc + 3] | ((int)program [pc + 4] << 8))))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 5;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 5;
					continue;
				case RxOp.UnicodeCharIgnoreCase:
					if (strpos < string_end) {
						char c = Char.ToLower (str [strpos]);
						if (((c == (program [pc + 1] | ((int)program [pc + 2] << 8))))) {
							strpos ++;
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
				case RxOp.BitmapIgnoreCase:
					if (strpos < string_end) {
						char c = Char.ToLower (str [strpos]);
						int c2 = (int)c; c2 -= program [pc + 1]; length = program [pc + 2];
						if (((c2 >= 0 && c2 < (length << 3) && (program [pc + 3 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 3 + program [pc + 2];
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 3 + program [pc + 2];
					continue;
				case RxOp.UnicodeBitmapIgnoreCase:
					if (strpos < string_end) {
						char c = Char.ToLower (str [strpos]);
						int c2 = (int)c; c2 -= (program [pc + 1] | ((int)program [pc + 2] << 8)); length = (program [pc + 3] | ((int)program [pc + 4] << 8));
						if (((c2 >= 0 && c2 < (length << 3) && (program [pc + 5 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							strpos ++;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 5 + (program [pc + 3] | ((int)program [pc + 4] << 8));
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 5 + (program [pc + 3] | ((int)program [pc + 4] << 8));
					continue;
				case RxOp.NoChar:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!((c == program [pc + 1]))) {
							pc += 2;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoRange:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!((c >= program [pc + 1] && c <= program [pc + 2]))) {
							pc += 3;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoUnicodeRange:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!((c >= (program [pc + 1] | ((int)program [pc + 2] << 8))) && (c <= (program [pc + 3] | ((int)program [pc + 4] << 8))))) {
							pc += 5;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoUnicodeChar:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!((c == (program [pc + 1] | ((int)program [pc + 2] << 8))))) {
							pc += 3;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryAny:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!((c != '\n'))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryAnySingleline:
					if (strpos < string_end) {
#if DEAD_CODE
						char c = str [strpos];
						if (!(true)) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
#endif
					}
					return false;
				case RxOp.NoCategoryWord:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!((Char.IsLetterOrDigit (c) || Char.GetUnicodeCategory (c) == UnicodeCategory.ConnectorPunctuation))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryDigit:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!((Char.IsDigit (c)))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryWhiteSpace:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!((Char.IsWhiteSpace (c)))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryEcmaWord:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!(('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || '0' <= c && c <= '9' || c == '_'))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryEcmaWhiteSpace:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!((c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v'))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryUnicodeSpecials:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!(('\uFEFF' <= c && c <= '\uFEFF' || '\uFFF0' <= c && c <= '\uFFFD'))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryUnicode:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!((Char.GetUnicodeCategory (c) == (UnicodeCategory)program [pc + 1]))) {
							pc += 2;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryGeneral:
					if (strpos < string_end) {
						char c = str [strpos];
						if (!((CategoryUtils.IsCategory ((Category)program [pc + 1], c)))) {
							pc += 2;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoBitmap:
					if (strpos < string_end) {
						char c = str [strpos];
						int c2 = (int)c; c2 -= program [pc + 1]; length = program [pc + 2];
						if (!((c2 >= 0 && c2 < (length << 3) && (program [pc + 3 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							pc += 3 + program [pc + 2];
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoUnicodeBitmap:
					if (strpos < string_end) {
						char c = str [strpos];
						int c2 = (int)c; c2 -= (program [pc + 1] | ((int)program [pc + 2] << 8)); length = (program [pc + 3] | ((int)program [pc + 4] << 8));
						if (!((c2 >= 0 && c2 < (length << 3) && (program [pc + 5 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							pc += 5 + (program [pc + 3] | ((int)program [pc + 4] << 8));
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCharIgnoreCase:
					if (strpos < string_end) {
						char c = Char.ToLower (str [strpos]);
						if (!((c == program [pc + 1]))) {
							pc += 2;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoRangeIgnoreCase:
					if (strpos < string_end) {
						char c = Char.ToLower (str [strpos]);
						if (!((c >= program [pc + 1] && c <= program [pc + 2]))) {
							pc += 3;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoUnicodeRangeIgnoreCase:
					if (strpos < string_end) {
						char c = Char.ToLower (str [strpos]);
						if (!((c >= (program [pc + 1] | ((int)program [pc + 2] << 8))) && (c <= (program [pc + 3] | ((int)program [pc + 4] << 8))))) {
							pc += 5;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoUnicodeCharIgnoreCase:
					if (strpos < string_end) {
						char c = Char.ToLower (str [strpos]);
						if (!((c == (program [pc + 1] | ((int)program [pc + 2] << 8))))) {
							pc += 3;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoBitmapIgnoreCase:
					if (strpos < string_end) {
						char c = Char.ToLower (str [strpos]);
						int c2 = (int)c; c2 -= program [pc + 1]; length = program [pc + 2];
						if (!((c2 >= 0 && c2 < (length << 3) && (program [pc + 3 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							pc += 3 + program [pc + 2];
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoUnicodeBitmapIgnoreCase:
					if (strpos < string_end) {
						char c = Char.ToLower (str [strpos]);
						int c2 = (int)c; c2 -= (program [pc + 1] | ((int)program [pc + 2] << 8)); length = (program [pc + 3] | ((int)program [pc + 4] << 8));
						if (!((c2 >= 0 && c2 < (length << 3) && (program [pc + 5 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							pc += 5 + (program [pc + 3] | ((int)program [pc + 4] << 8));
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos ++;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.CharReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (((c == program [pc + 1]))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 2;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				case RxOp.RangeReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (((c >= program [pc + 1] && c <= program [pc + 2]))) {
							strpos --;
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
				case RxOp.UnicodeRangeReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (((c >= (program [pc + 1] | ((int)program [pc + 2] << 8))) && (c <= (program [pc + 3] | ((int)program [pc + 4] << 8))))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 5;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 5;
					continue;
				case RxOp.UnicodeCharReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (((c == (program [pc + 1] | ((int)program [pc + 2] << 8))))) {
							strpos --;
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
				case RxOp.CategoryAnyReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (((c != '\n'))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				case RxOp.CategoryAnySinglelineReverse:
					if (strpos > 0) {
						//char c = str [strpos - 1];
						if ((true)) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				case RxOp.CategoryWordReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (((Char.IsLetterOrDigit (c) || Char.GetUnicodeCategory (c) == UnicodeCategory.ConnectorPunctuation))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				case RxOp.CategoryDigitReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (((Char.IsDigit (c)))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				case RxOp.CategoryWhiteSpaceReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (((Char.IsWhiteSpace (c)))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				case RxOp.CategoryEcmaWordReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if ((('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || '0' <= c && c <= '9' || c == '_'))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				case RxOp.CategoryEcmaWhiteSpaceReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (((c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v'))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				case RxOp.CategoryUnicodeSpecialsReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if ((('\uFEFF' <= c && c <= '\uFEFF' || '\uFFF0' <= c && c <= '\uFFFD'))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 1;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 1;
					continue;
				case RxOp.CategoryUnicodeReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (((Char.GetUnicodeCategory (c) == (UnicodeCategory)program [pc + 1]))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 2;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				case RxOp.CategoryGeneralReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (((CategoryUtils.IsCategory ((Category)program [pc + 1], c)))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 2;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				case RxOp.BitmapReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						int c2 = (int)c; c2 -= program [pc + 1]; length = program [pc + 2];
						if (((c2 >= 0 && c2 < (length << 3) && (program [pc + 3 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 3 + program [pc + 2];
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 3 + program [pc + 2];
					continue;
				case RxOp.UnicodeBitmapReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						int c2 = (int)c; c2 -= (program [pc + 1] | ((int)program [pc + 2] << 8)); length = (program [pc + 3] | ((int)program [pc + 4] << 8));
						if (((c2 >= 0 && c2 < (length << 3) && (program [pc + 5 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 5 + (program [pc + 3] | ((int)program [pc + 4] << 8));
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 5 + (program [pc + 3] | ((int)program [pc + 4] << 8));
					continue;
				case RxOp.CharIgnoreCaseReverse:
					if (strpos > 0) {
						char c = Char.ToLower (str [strpos - 1]);
						if (((c == program [pc + 1]))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 2;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 2;
					continue;
				case RxOp.RangeIgnoreCaseReverse:
					if (strpos > 0) {
						char c = Char.ToLower (str [strpos - 1]);
						if (((c >= program [pc + 1] && c <= program [pc + 2]))) {
							strpos --;
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
				case RxOp.UnicodeRangeIgnoreCaseReverse:
					if (strpos > 0) {
						char c = Char.ToLower (str [strpos - 1]);
						if (((c >= (program [pc + 1] | ((int)program [pc + 2] << 8))) && (c <= (program [pc + 3] | ((int)program [pc + 4] << 8))))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 5;
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 5;
					continue;
				case RxOp.UnicodeCharIgnoreCaseReverse:
					if (strpos > 0) {
						char c = Char.ToLower (str [strpos - 1]);
						if (((c == (program [pc + 1] | ((int)program [pc + 2] << 8))))) {
							strpos --;
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
				case RxOp.BitmapIgnoreCaseReverse:
					if (strpos > 0) {
						char c = Char.ToLower (str [strpos - 1]);
						int c2 = (int)c; c2 -= program [pc + 1]; length = program [pc + 2];
						if (((c2 >= 0 && c2 < (length << 3) && (program [pc + 3 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 3 + program [pc + 2];
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 3 + program [pc + 2];
					continue;
				case RxOp.UnicodeBitmapIgnoreCaseReverse:
					if (strpos > 0) {
						char c = Char.ToLower (str [strpos - 1]);
						int c2 = (int)c; c2 -= (program [pc + 1] | ((int)program [pc + 2] << 8)); length = (program [pc + 3] | ((int)program [pc + 4] << 8));
						if (((c2 >= 0 && c2 < (length << 3) && (program [pc + 5 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							strpos --;
							if (char_group_end != 0)
								goto test_char_group_passed;
							pc += 5 + (program [pc + 3] | ((int)program [pc + 4] << 8));
							continue;
						}
					}
					if (char_group_end == 0)
						return false;
					pc += 5 + (program [pc + 3] | ((int)program [pc + 4] << 8));
					continue;
				case RxOp.NoCharReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!((c == program [pc + 1]))) {
							pc += 2;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoRangeReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!((c >= program [pc + 1] && c <= program [pc + 2]))) {
							pc += 3;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoUnicodeRangeReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!((c >= (program [pc + 1] | ((int)program [pc + 2] << 8))) && (c <= (program [pc + 3] | ((int)program [pc + 4] << 8))))) {
							pc += 5;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoUnicodeCharReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!((c == (program [pc + 1] | ((int)program [pc + 2] << 8))))) {
							pc += 3;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryAnyReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!((c != '\n'))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryAnySinglelineReverse:
					if (strpos > 0) {
#if DEAD_CODe
						char c = str [strpos - 1];
						if (!(true)) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
#endif
					}
					return false;
				case RxOp.NoCategoryWordReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!((Char.IsLetterOrDigit (c) || Char.GetUnicodeCategory (c) == UnicodeCategory.ConnectorPunctuation))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryDigitReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!((Char.IsDigit (c)))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryWhiteSpaceReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!((Char.IsWhiteSpace (c)))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryEcmaWordReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!(('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || '0' <= c && c <= '9' || c == '_'))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryEcmaWhiteSpaceReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!((c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v'))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryUnicodeSpecialsReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!(('\uFEFF' <= c && c <= '\uFEFF' || '\uFFF0' <= c && c <= '\uFFFD'))) {
							pc += 1;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryUnicodeReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!((Char.GetUnicodeCategory (c) == (UnicodeCategory)program [pc + 1]))) {
							pc += 2;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCategoryGeneralReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						if (!((CategoryUtils.IsCategory ((Category)program [pc + 1], c)))) {
							pc += 2;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoBitmapReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						int c2 = (int)c; c2 -= program [pc + 1]; length = program [pc + 2];
						if (!((c2 >= 0 && c2 < (length << 3) && (program [pc + 3 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							pc += 3 + program [pc + 2];
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoUnicodeBitmapReverse:
					if (strpos > 0) {
						char c = str [strpos - 1];
						int c2 = (int)c; c2 -= (program [pc + 1] | ((int)program [pc + 2] << 8)); length = (program [pc + 3] | ((int)program [pc + 4] << 8));
						if (!((c2 >= 0 && c2 < (length << 3) && (program [pc + 5 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							pc += 5 + (program [pc + 3] | ((int)program [pc + 4] << 8));
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoCharIgnoreCaseReverse:
					if (strpos > 0) {
						char c = Char.ToLower (str [strpos - 1]);
						if (!((c == program [pc + 1]))) {
							pc += 2;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoRangeIgnoreCaseReverse:
					if (strpos > 0) {
						char c = Char.ToLower (str [strpos - 1]);
						if (!((c >= program [pc + 1] && c <= program [pc + 2]))) {
							pc += 3;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoUnicodeRangeIgnoreCaseReverse:
					if (strpos > 0) {
						char c = Char.ToLower (str [strpos - 1]);
						if (!((c >= (program [pc + 1] | ((int)program [pc + 2] << 8))) && (c <= (program [pc + 3] | ((int)program [pc + 4] << 8))))) {
							pc += 5;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoUnicodeCharIgnoreCaseReverse:
					if (strpos > 0) {
						char c = Char.ToLower (str [strpos - 1]);
						if (!((c == (program [pc + 1] | ((int)program [pc + 2] << 8))))) {
							pc += 3;
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoBitmapIgnoreCaseReverse:
					if (strpos > 0) {
						char c = Char.ToLower (str [strpos - 1]);
						int c2 = (int)c; c2 -= program [pc + 1]; length = program [pc + 2];
						if (!((c2 >= 0 && c2 < (length << 3) && (program [pc + 3 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							pc += 3 + program [pc + 2];
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				case RxOp.NoUnicodeBitmapIgnoreCaseReverse:
					if (strpos > 0) {
						char c = Char.ToLower (str [strpos - 1]);
						int c2 = (int)c; c2 -= (program [pc + 1] | ((int)program [pc + 2] << 8)); length = (program [pc + 3] | ((int)program [pc + 4] << 8));
						if (!((c2 >= 0 && c2 < (length << 3) && (program [pc + 5 + (c2 >> 3)] & (1 << (c2 & 0x7))) != 0))) {
							pc += 5 + (program [pc + 3] | ((int)program [pc + 4] << 8));
							if (char_group_end == 0 || (pc + 1 == char_group_end)) {
								strpos --;
							if (pc + 1 == char_group_end)
								goto test_char_group_passed;
							}
							continue;
						}
					}
					return false;
				
				// END OF GENERATED CODE

				case RxOp.Branch: {
					int res = 0;
					if (EvalByteCode (pc + 3, strpos, ref res)) {
						strpos_result = res;
						return true;
					}
					//Console.WriteLine ("branch offset: {0}", program [pc + 1] | ((int)program [pc + 2] << 8));
					pc += program [pc + 1] | ((int)program [pc + 2] << 8);
					continue;
				}
				case RxOp.Repeat:
				case RxOp.RepeatLazy: {
					/*
					 * Repetation is modelled by two opcodes: Repeat and Until which
					 * contain the the qualified regex between them, i.e.:
					 * Repeat, <bytecode for the inner regex>, Until, <Tail expr>
					 * It is processed as follows: 
					 * Repeat, [Until, <inner expr>]*, <Tail>
					 * This means that nested quantifiers are processed a bit
					 * strangely: when the inner quantifier fails to match, its
					 * tail is processed which includes the outer Until.
					 *
					 * This code is from the old interpreter.cs.
					 *
					 * FIXME: Rethink this.
					 */

					int res = 0;

					this.repeat = new RepeatContext (
						this.repeat,			// previous context
						ReadInt (program, pc + 3),		// minimum
						ReadInt (program, pc + 7),		// maximum
						(RxOp)program [pc] == RxOp.RepeatLazy, // lazy
						pc + 11				// subexpression
					);

					int until = pc + (program [pc + 1] | ((int)program [pc + 2] << 8));
					if (!EvalByteCode (until, strpos, ref res)) {
						this.repeat = this.repeat.Previous;
						return false;
					}

					strpos = res;
					strpos_result = strpos;
					return true;
				}
				case RxOp.Until: {
					RepeatContext current = this.repeat;
					int res = 0;

					//
					// Can we avoid recursion?
					//
					// Backtracking can be forced in nested quantifiers from the tail of this quantifier.
					// Thus, we cannot, in general, use a simple loop on repeat.Expression to handle
					// quantifiers.
					//
					// If 'deep' was unmolested, that implies that there was no nested quantifiers.
					// Thus, we can safely avoid recursion.
					//
					if (deep == current)
						goto Pass;

					start = current.Start;
					int start_count = current.Count;

					// First match at least 'start' items without backtracking
					while (!current.IsMinimum) {
						++ current.Count;
						current.Start = strpos;
						deep = current;
						if (!EvalByteCode (current.Expression, strpos, ref res)) {
							current.Start = start;
							current.Count = start_count;
							goto Fail;
						}
						strpos = res;
						if (deep != current)	// recursive mode
							goto Pass;
					}

					if (strpos == current.Start) {
						// degenerate match ... match tail or fail
						this.repeat = current.Previous;
						deep = null;
						if (EvalByteCode (pc + 1, strpos, ref res)) {
							strpos = res;
							goto Pass;
						}
						this.repeat = current;
						goto Fail;
					}

					if (current.IsLazy) {
						for (;;) {
							// match tail first ...
							this.repeat = current.Previous;
							deep = null;
							int cp = Checkpoint ();
							if (EvalByteCode (pc + 1, strpos, ref res)) {
								strpos = res;
								goto Pass;
							}

							Backtrack (cp);

							// ... then match more
							this.repeat = current;
							if (current.IsMaximum)
								goto Fail;
							++ current.Count;
							current.Start = strpos;
							deep = current;
							if (!EvalByteCode (current.Expression, strpos, ref res)) {
								current.Start = start;
								current.Count = start_count;
								goto Fail;
							}
							strpos = res;
							if (deep != current)	// recursive mode
								goto Pass;
							// Degenerate match: ptr has not moved since the last (failed) tail match.
							// So, next and subsequent tail matches will fail.
							if (strpos == current.Start)
								goto Fail;
						}
					} else {
						int stack_size = stack.Count;

						// match greedily as much as possible
						while (!current.IsMaximum) {
							int cp = Checkpoint ();
							int old_ptr = strpos;
							int old_start = current.Start;

							++ current.Count;
							if (trace_rx)
								Console.WriteLine ("recurse with count {0}.", current.Count);
							current.Start = strpos;
							deep = current;
							if (!EvalByteCode (current.Expression, strpos, ref res)) {
								-- current.Count;
								current.Start = old_start;
								Backtrack (cp);
								break;
							}
							strpos = res;
							if (deep != current) {
								// recursive mode: no more backtracking, truncate the stack
								stack.Count = stack_size;
								goto Pass;
							}
							stack.Push (cp);
							stack.Push (old_ptr);

							// Degenerate match: no point going on
							if (strpos == current.Start)
								break;
						}

						if (trace_rx)
							Console.WriteLine ("matching tail: {0} pc={1}", strpos, pc + 1);
						// then, match the tail, backtracking as necessary.
						this.repeat = current.Previous;
						for (;;) {
							deep = null;
							if (EvalByteCode (pc + 1, strpos, ref res)) {
								strpos = res;
								stack.Count = stack_size;
								goto Pass;
							}
							if (stack.Count == stack_size) {
								this.repeat = current;
								goto Fail;
							}

							--current.Count;
							strpos = stack.Pop ();
							Backtrack (stack.Pop ());
							if (trace_rx)
								Console.WriteLine ("backtracking to {0} expr={1} pc={2}", strpos, current.Expression, pc);
						}
					}
				}

				case RxOp.FastRepeat:
				case RxOp.FastRepeatLazy: {
					/*
					 * A FastRepeat is a simplified version of Repeat which does
					 * not contain another repeat inside, so backtracking is 
					 * easier.
					 */
					bool lazy = program [pc] == (byte)RxOp.FastRepeatLazy;
					int res = 0;
					int tail = pc + (program [pc + 1] | ((int)program [pc + 2] << 8));
 					start = ReadInt (program, pc + 3);
 					end = ReadInt (program, pc + 7);
					//Console.WriteLine ("min: {0}, max: {1} tail: {2}", start, end, tail);
 					length = 0;

					deep = null;

					// First match at least 'start' items
					while (length < start) {
						if (!EvalByteCode (pc + 11, strpos, ref res))
 							return false;
 						strpos = res;
 						length++;
 					}
					
					if (lazy) {
						while (true) {
							// Match the tail
							int cp = Checkpoint ();
							if (EvalByteCode (tail, strpos, ref res)) {
								strpos = res;
								goto repeat_success;
							}
							Backtrack (cp);

							if (length >= end)
								return false;

							// Match an item
							if (!EvalByteCode (pc + 11, strpos, ref res))
								return false;
							strpos = res;
							length ++;
						}
					} else {
						// Then match as many items as possible, recording
						// backtracking information
						int old_stack_size = stack.Count;
						while (length < end) {
							int cp = Checkpoint ();
							if (!EvalByteCode (pc + 11, strpos, ref res)) {
								Backtrack (cp);
								break;
							}
							stack.Push (cp);
							stack.Push (strpos);
							strpos = res;
							length++;
						}	

						if (tail <= pc)
							throw new Exception ();

						// Then, match the tail, backtracking as necessary.
						while (true) {
							if (EvalByteCode (tail, strpos, ref res)) {
								strpos = res;
								stack.Count = old_stack_size;
								goto repeat_success;
							}
							if (stack.Count == old_stack_size)
								return false;

							// Backtrack
							strpos = stack.Pop ();
							Backtrack (stack.Pop ());
							if (trace_rx)
								Console.WriteLine ("backtracking to: {0}", strpos);
						}
					}

 				repeat_success:
					// We matched the tail too so just return
					goto Pass;
				}

				default:
					Console.WriteLine ("evaluating: {0} at pc: {1}, strpos: {2}", (RxOp)program [pc], pc, strpos);
					throw new NotSupportedException ();
				}
				continue;

			Pass:
				strpos_result = strpos;
				return true;
			Fail:
				return false;
			test_char_group_passed:
				pc = char_group_end;
				char_group_end = 0;
				continue;
			} // end of while (true)
		}
	}
}
