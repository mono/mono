//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	interpreter.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace System.Text.RegularExpressions {

	class Interpreter : IMachine {
		public Interpreter (ushort[] program) {
			this.program = program;
			this.qs = null;

			// process info block

			Debug.Assert ((OpCode)program[0] == OpCode.Info, "Regex", "Cant' find info block");

			this.group_count = program[1] + 1;
			this.match_min = program[2];
			this.match_max = program[3];

			// setup

			this.program_start = 4;
			this.groups = new int [group_count];
		}

		// IMachine implementation

		public Match Scan (Regex regex, string text, int start, int end) {
			this.text = text;
			this.text_end = end;
			this.scan_ptr = start;

			if (Eval (Mode.Match, ref scan_ptr, program_start))
				return GenerateMatch (regex);

			return Match.Empty;
		}

		// private methods

		private void Reset () {
			ResetGroups ();
			fast = repeat = null;
		}

		private bool Eval (Mode mode, ref int ref_ptr, int pc) {
			int ptr = ref_ptr;
		Begin:
			for (;;) {
				ushort word = program[pc];
				OpCode op = (OpCode)(word & 0x00ff);
				OpFlags flags = (OpFlags)(word & 0xff00);

				switch (op) {
				case OpCode.Anchor: {
					int skip = program[pc + 1];

					int anch_offset = program[pc + 2];
					int anch_ptr = ptr + anch_offset;
					int anch_end = text_end - match_min + anch_offset;	// maximum anchor position

					// the general case for an anchoring expression is at the bottom, however we
					// do some checks for the common cases before to save processing time. the current
					// optimizer only outputs three types of anchoring expressions: fixed position,
					// fixed substring, and no anchor.

					OpCode anch_op = (OpCode)(program[pc + 3] & 0x00ff);
					if (anch_op == OpCode.Position && skip == 6) {				// position anchor
						// Anchor
						// 	Position
						//	True

						switch ((Position)program[pc + 4]) {
						case Position.StartOfString:
							if (anch_ptr == 0) {
								ptr = 0;
								if (TryMatch (ref ptr, pc + skip))
									goto Pass;
							}
							break;
						
						case Position.StartOfLine:
							if (anch_ptr == 0) {
								ptr = 0;
								if (TryMatch (ref ptr, pc + skip))
									goto Pass;

								++ anch_ptr;
							}

							while (anch_ptr <= anch_end) {
								if (text[anch_ptr - 1] == '\n') {
									ptr = anch_ptr - anch_offset;
									if (TryMatch (ref ptr, pc + skip))
										goto Pass;
								}

								++ anch_ptr;
							}
							break;
						
						case Position.StartOfScan:
							if (anch_ptr == scan_ptr) {
								ptr = scan_ptr - anch_offset;
								if (TryMatch (ref ptr, pc + skip))
									goto Pass;
							}
							break;

						default:
							// FIXME
							break;
						}
					}
					else if (qs != null ||
						(anch_op == OpCode.String && skip == 6 + program[pc + 4])) {	// substring anchor
						// Anchor
						//	String
						//	True

						if (qs == null) {
							bool ignore = ((OpFlags)program[pc + 3] & OpFlags.IgnoreCase) != 0;
							string substring = GetString (pc + 3);

							qs = new QuickSearch (substring, ignore);
						}

						while (anch_ptr <= anch_end) {
							anch_ptr = qs.Search (text, anch_ptr, anch_end);
							if (anch_ptr < 0)
								break;

							ptr = anch_ptr - anch_offset;
							if (TryMatch (ref ptr, pc + skip))
								goto Pass;

							++ anch_ptr;
						}
					}
					else if (anch_op == OpCode.True) {					// no anchor
						// Anchor
						//	True

						while (anch_ptr <= anch_end) {
							ptr = anch_ptr;
							if (TryMatch (ref ptr, pc + skip))
								goto Pass;

							++ anch_ptr;
						}
					}
					else {									// general case
						// Anchor
						//	<expr>
						//	True

						while (anch_ptr <= anch_end) {
							ptr = anch_ptr;
							if (Eval (Mode.Match, ref ptr, pc + 3)) {
								// anchor expression passed: try real expression at the correct offset

								ptr = anch_ptr - anch_offset;
								if (TryMatch (ref ptr, pc + skip))
									goto Pass;
							}

							++ anch_ptr;
						}
					}

					goto Fail;
				}
				
				case OpCode.False: {
					goto Fail;
				}

				case OpCode.True: {
					goto Pass;
				}

				case OpCode.Position: {
					if (!IsPosition ((Position)program[pc + 1], ptr))
						goto Fail;
					pc += 2;
					break;
				}

				case OpCode.String: {
					bool reverse = (flags & OpFlags.RightToLeft) != 0;
					bool ignore = (flags & OpFlags.IgnoreCase) != 0;
					int len = program[pc + 1];

					if (reverse) {
						ptr -= len;
						if (ptr < 0)
							goto Fail;
					}
					else if (ptr + len > text_end)
						goto Fail;

					pc += 2;
					for (int i = 0; i < len; ++ i) {
						char c = text[ptr + i];
						if (ignore)
							c = Char.ToLower (c);

						if (c != (char)program[pc ++])
							goto Fail;
					}

					if (!reverse)
						ptr += len;
					break;
				}

				case OpCode.Reference: {
					bool reverse = (flags & OpFlags.RightToLeft) != 0;
					bool ignore = (flags & OpFlags.IgnoreCase) != 0;
					int m = GetLastDefined (program [pc + 1]);
					if (m < 0)
						goto Fail;

					int str = marks [m].Index;
					int len = marks [m].Length;

					if (reverse) {
						ptr -= len;
						if (ptr < 0)
							goto Fail;
					}
					else if (ptr + len > text_end)
						goto Fail;

					pc += 2;
					for (int i = 0; i < len; ++ i) {
						if (ignore) {
							if (Char.ToLower (text[ptr + i]) != Char.ToLower (text[str + i]))
								goto Fail;
						}
						else {
							if (text[ptr + i] != text[str + i])
								goto Fail;
						}
					}

					if (!reverse)
						ptr += len;
					break;
				}

				case OpCode.Character: case OpCode.Category:
				case OpCode.Range: case OpCode.Set: {
					if (!EvalChar (mode, ref ptr, ref pc, false))
						goto Fail;
					break;
				}

				case OpCode.In: {
					int target = pc + program[pc + 1];
					pc += 2;
					if (!EvalChar (mode, ref ptr, ref pc, true))
						goto Fail;

					pc = target;
					break;
				}

				case OpCode.Open: {
					Open (program[pc + 1], ptr);
					pc += 2;
					break;
				}

				case OpCode.Close: {
					Close (program[pc + 1], ptr);
					pc += 2;
					break;
				}

				case OpCode.Balance: {
					Balance (program[pc + 1], program[pc + 2], ptr);
					break;
				}

				case OpCode.IfDefined: {
					int m = GetLastDefined (program [pc + 2]);
					if (m < 0)
						pc += program[pc + 1];
					else
						pc += 3;
					break;
				}

				case OpCode.Sub: {
					if (!Eval (Mode.Match, ref ptr, pc + 2))
						goto Fail;

					pc += program[pc + 1];
					break;
				}

				case OpCode.Test: {
					int cp = Checkpoint ();
					int test_ptr = ptr;
					if (Eval (Mode.Match, ref test_ptr, pc + 3))
						pc += program[pc + 1];
					else {
						Backtrack (cp);
						pc += program[pc + 2];
					}
					break;
				}

				case OpCode.Branch: {
					OpCode branch_op;
					do {
						int cp = Checkpoint ();
						if (Eval (Mode.Match, ref ptr, pc + 2))
							goto Pass;
						
						Backtrack (cp);
						
						pc += program[pc + 1];
						branch_op = (OpCode)(program[pc] & 0xff);
					} while (branch_op != OpCode.False);

					goto Fail;
				}

				case OpCode.Jump: {
					pc += program[pc + 1];
					break;
				}

				case OpCode.Repeat: {
					this.repeat = new RepeatContext (
						this.repeat,			// previous context
						program[pc + 2],		// minimum
						program[pc + 3],		// maximum
						(flags & OpFlags.Lazy) != 0,	// lazy
						pc + 4				// subexpression
					);

					if (Eval (Mode.Match, ref ptr, pc + program[pc + 1]))
						goto Pass;
					else {
						this.repeat = this.repeat.Previous;
						goto Fail;
					}
				}

				case OpCode.Until: {
					RepeatContext current = this.repeat;
					int start = current.Start;

					if (!current.IsMinimum) {
						++ current.Count;
						current.Start = ptr;
						if (Eval (Mode.Match, ref ptr, repeat.Expression))
							goto Pass;

						current.Start = start;
						-- current.Count;
						goto Fail;
					}

					if (ptr == current.Start) {
						// degenerate match ... match tail or fail

						this.repeat = current.Previous;
						if (Eval (Mode.Match, ref ptr, pc + 1))
							goto Pass;
					
						this.repeat = current;
						goto Fail;
					}

					if (current.IsLazy) {
						// match tail first ...

						this.repeat = current.Previous;
						int cp = Checkpoint ();
						if (Eval (Mode.Match, ref ptr, pc + 1))
							goto Pass;

						Backtrack (cp);

						// ... then match more

						this.repeat = current;
						if (!current.IsMaximum) {
							++ current.Count;
							current.Start = ptr;
							if (Eval (Mode.Match, ref ptr, current.Expression))
								goto Pass;

							current.Start = start;
							-- current.Count;
							goto Fail;
						}

						return false;
					}
					else {
						// match more first ...

						if (!current.IsMaximum) {
							int cp = Checkpoint ();
							++ current.Count;
							current.Start = ptr;
							if (Eval (Mode.Match, ref ptr, current.Expression))
								goto Pass;

							current.Start = start;
							-- current.Count;
							Backtrack (cp);
						}

						// ... then match tail

						this.repeat = current.Previous;
						if (Eval (Mode.Match, ref ptr, pc + 1))
							goto Pass;

						this.repeat = current;
						goto Fail;
					}
				}

				case OpCode.FastRepeat: {
					this.fast = new RepeatContext (
						fast,
						program[pc + 2],		// minimum
						program[pc + 3],		// maximum
						(flags & OpFlags.Lazy) != 0,	// lazy
						pc + 4				// subexpression
					);
					fast.Start = ptr;

					int cp = Checkpoint ();

					pc += program[pc + 1];		// tail expression
					ushort tail_word = program[pc];

					int c1, c2;			// first character of tail operator
					int coff;			// 0 or -1 depending on direction

					OpCode tail_op = (OpCode)(tail_word & 0xff);
					if (tail_op == OpCode.Character || tail_op == OpCode.String) {
						OpFlags tail_flags = (OpFlags)(tail_word & 0xff00);

						if (tail_op == OpCode.String)
							c1 = program[pc + 2];				// first char of string
						else
							c1 = program[pc + 1];				// character
						
						if ((tail_flags & OpFlags.IgnoreCase) != 0)
							c2 = Char.ToUpper ((char)c1);			// ignore case
						else
							c2 = c1;

						if ((tail_flags & OpFlags.RightToLeft) != 0)
							coff = -1;					// reverse
						else
							coff = 0;
					}
					else {
						c1 = c2 = -1;
						coff = 0;
					}

					if (fast.IsLazy) {
						if (!fast.IsMinimum && !Eval (Mode.Count, ref ptr, fast.Expression)) {
							//Console.WriteLine ("lazy fast: failed mininum.");
							fast = fast.Previous;
							goto Fail;
						}
						
						while (true) {
							int p = ptr + coff;
							if ((c1 < 0 || (p >= 0 && p < text_end && (c1 == text[p] || c2 == text[p]))) &&
							    Eval (Mode.Match, ref ptr, pc))
								break;

							if (fast.IsMaximum) {
								//Console.WriteLine ("lazy fast: failed with maximum.");
								fast = fast.Previous;
								goto Fail;
							}

							Backtrack (cp);
							if (!Eval (Mode.Count, ref ptr, fast.Expression)) {
								//Console.WriteLine ("lazy fast: no more.");
								fast = fast.Previous;
								goto Fail;
							}
						}
						fast = fast.Previous;
						goto Pass;
					}
					else {
						if (!Eval (Mode.Count, ref ptr, fast.Expression)) {
							fast = fast.Previous;
							goto Fail;
						}
					
						int width;
						if (fast.Count > 0)
							width = (ptr - fast.Start) / fast.Count;
						else
							width = 0;

						while (true) {
							int p = ptr + coff;
							if ((c1 < 0 || (p >= 0 && p < text_end && (c1 == text[p] || c2 == text[p]))) &&
							    Eval (Mode.Match, ref ptr, pc))
								break;

							-- fast.Count;
							if (!fast.IsMinimum) {
								fast = fast.Previous;
								goto Fail;
							}

							ptr -= width;
							Backtrack (cp);
						}
						fast = fast.Previous;
						goto Pass;
					}
				}

				case OpCode.Info: {
					Debug.Assert (false, "Regex", "Info block found in pattern");
					goto Fail;
				}
				}
			}
		Pass:
			ref_ptr = ptr;

			switch (mode) {
			case Mode.Match:
				return true;

			case Mode.Count: {
				++ fast.Count;
				if (fast.IsMaximum || (fast.IsLazy && fast.IsMinimum))
					return true;

				pc = fast.Expression;
				goto Begin;
			}
			}

		Fail:
			switch (mode) {
			case Mode.Match:
				return false;

			case Mode.Count: {
				if (!fast.IsLazy && fast.IsMinimum)
					return true;

				ref_ptr = fast.Start;
				return false;
			}
			}

			return false;
		}

		private bool EvalChar (Mode mode, ref int ptr, ref int pc, bool multi) {
			bool consumed = false;
			char c = '\0';
			bool negate;
			bool ignore;
			do {
				ushort word = program[pc];
				OpCode op = (OpCode)(word & 0x00ff);
				OpFlags flags = (OpFlags)(word & 0xff00);

				++ pc;

				ignore = (flags & OpFlags.IgnoreCase) != 0;
				
				// consume character: the direction of an In construct is
				// determined by the direction of its first op

				if (!consumed) {
					if ((flags & OpFlags.RightToLeft) != 0) {
						if (ptr <= 0)
							return false;

						c = text[-- ptr];
					}
					else {
						if (ptr >= text_end)
							return false;

						c = text[ptr ++];
					}

					if (ignore)
						c = Char.ToLower (c);

					consumed = true;
				}

				// negate flag

				negate = (flags & OpFlags.Negate) != 0;

				// execute op
				
				switch (op) {
				case OpCode.True:
					return true;

				case OpCode.False:
					return false;
				
				case OpCode.Character: {
					if (c == (char)program[pc ++])
						return !negate;
					break;
				}

				case OpCode.Category: {
					if (CategoryUtils.IsCategory ((Category)program[pc ++], c))
						return !negate;

					break;
				}
				
				case OpCode.Range: {
					int lo = (char)program[pc ++];
					int hi = (char)program[pc ++];
					if (lo <= c && c <= hi)
						return !negate;
					break;
				}

				case OpCode.Set: {
					int lo = (char)program[pc ++];
					int len = (char)program[pc ++];
					int bits = pc;
					pc += len;

					int i = (int)c - lo;
					if (i < 0 || i >= len << 4)
						break;

					if ((program[bits + (i >> 4)] & (1 << (i & 0xf))) != 0)
						return !negate;
					break;
				}
				}
			} while (multi);

			return negate;
		}

		private bool TryMatch (ref int ref_ptr, int pc) {
			Reset ();
			
			int ptr = ref_ptr;
			marks [groups [0]].Start = ptr;
			if (Eval (Mode.Match, ref ptr, pc)) {
				marks [groups [0]].End = ptr;
				ref_ptr = ptr;
				return true;
			}

			return false;
		}
		
		private bool IsPosition (Position pos, int ptr) {
			switch (pos) {
			case Position.Start: case Position.StartOfString:
				return ptr == 0;

			case Position.StartOfLine:
				return ptr == 0 || text[ptr - 1] == '\n';
				
			case Position.StartOfScan:
				return ptr == scan_ptr;
			
			case Position.End:
				return ptr == text_end ||
					(ptr == text_end - 1 && text[ptr] == '\n');

			case Position.EndOfLine:
				return ptr == text_end || text[ptr] == '\n';
				
			case Position.EndOfString:
				return ptr == text_end;
				
			case Position.Boundary:
				if (text_end == 0)
					return false;

				if (ptr == 0)
					return IsWordChar (text[ptr]);
				else if (ptr == text_end)
					return IsWordChar (text[ptr - 1]);
				else
					return IsWordChar (text[ptr]) != IsWordChar (text[ptr - 1]);

			case Position.NonBoundary:
				if (text_end == 0)
					return false;

				if (ptr == 0)
					return !IsWordChar (text[ptr]);
				else if (ptr == text_end)
					return !IsWordChar (text[ptr - 1]);
				else
					return IsWordChar (text[ptr]) == IsWordChar (text[ptr - 1]);
			
			default:
				return false;
			}
		}

		private bool IsWordChar (char c) {
			return CategoryUtils.IsCategory (Category.Word, c);
		}

		private string GetString (int pc) {
			int len = program[pc + 1];
			int str = pc + 2;

			char[] cs = new char[len];
			for (int i = 0; i < len; ++ i)
				cs[i] = (char)program[str ++];

			return new string (cs);
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

		private void Balance (int gid, int balance_gid, int ptr) {
			int b = groups [balance_gid];
			Debug.Assert (marks [b].IsDefined, "Regex", "Balancing group not closed");

			if (gid > 0) {
				Open (gid, marks [b].Index + marks [b].Length);
				Close (gid, ptr);
			}

			groups [balance_gid] = marks [b].Previous;
		}

		private int Checkpoint () {
			mark_start = mark_end;
			return mark_start;
		}

		private void Backtrack (int cp) {
			Debug.Assert (cp > mark_start, "Regex", "Attempt to backtrack forwards");

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

		private Match GenerateMatch (Regex regex) {
			int[][] grps = new int[groups.Length][];
			ArrayList caps = new ArrayList ();

			for (int gid = 0; gid < groups.Length; ++ gid) {
				caps.Clear ();
				for (int m = groups[gid]; m >= 0; m = marks[m].Previous) {
					if (!marks[m].IsDefined)
						continue;
					
					caps.Add (marks[m].Index);
					caps.Add (marks[m].Length);
				}

				grps[gid] = (int[])caps.ToArray (typeof (int));
			}

			return new Match (regex, this, text, text_end, grps);
		}

		// interpreter attributes

		private ushort[] program;		// regex program
		private int program_start;		// first instruction after info block
		private string text;			// input text
		private int text_end;			// end of input text (last character + 1)
		private int group_count;		// number of capturing groups
		private int match_min, match_max;	// match width information
		private QuickSearch qs;			// fast substring matcher

		// match state
		
		private int scan_ptr;			// start of scan

		private RepeatContext repeat;		// current repeat context
		private RepeatContext fast;		// fast repeat context

		private Mark[] marks = null;		// mark stack
		private int mark_start;			// start of current checkpoint
		private int mark_end;			// end of checkpoint/next free mark

		private int[] groups;			// current group definitions

		// private classes

		private struct Mark {
			public int Start, End;
			public int Previous;

			public bool IsDefined {
				get { return Start >= 0 && End >= 0; }
			}

			public int Index {
				get { return Start < End ? Start : End; }
			}

			public int Length {
				get { return Start < End ? End - Start : Start - End; }
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

		private enum Mode {
			Search,
			Match,
			Count
		}
	}
}
