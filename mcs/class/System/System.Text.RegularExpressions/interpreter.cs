//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	interpreter.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace System.Text.RegularExpressions {

	partial class Interpreter : BaseMachine {
		private int ReadProgramCount (int ptr)
		{
			int ret = program [ptr + 1];
			ret <<= 16;
			ret += program [ptr];
			return ret;
		}

		public Interpreter (ushort[] program) {
			this.program = program;
			this.qs = null;

			// process info block
			Debug.Assert ((OpCode)program[0] == OpCode.Info, "Regex", "Cant' find info block");
			this.group_count = ReadProgramCount (1) + 1;
			this.match_min = ReadProgramCount (3);
			//this.match_max = ReadProgramCount (5);

			// setup

			this.program_start = 7;
			this.groups = new int [group_count];
		}

		// IMachine implementation

		public override Match Scan (Regex regex, string text, int start, int end) {
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
					bool anch_reverse = (flags & OpFlags.RightToLeft) != 0;	
					int anch_ptr = anch_reverse ?  ptr - anch_offset  : ptr + anch_offset;
					int anch_end = text_end - match_min + anch_offset;	// maximum anchor position  
					
					
					int anch_begin =  0;


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
							if (anch_reverse || anch_offset == 0) {
								if (anch_reverse)
									ptr = anch_offset;
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

							while ((anch_reverse && anch_ptr >= 0) || (!anch_reverse && anch_ptr <= anch_end)) {  
								if (anch_ptr == 0 || text[anch_ptr - 1] == '\n') {
									if (anch_reverse)
										ptr = anch_ptr == anch_end ? anch_ptr : anch_ptr + anch_offset;
									else
										ptr = anch_ptr == 0 ? anch_ptr : anch_ptr - anch_offset;
									if (TryMatch (ref ptr, pc + skip))
										goto Pass;
								}
							
								if (anch_reverse)
									-- anch_ptr;
								else
									++ anch_ptr;
							}
							break;
						
						case Position.StartOfScan:
							if (anch_ptr == scan_ptr) {							
								ptr = anch_reverse ? scan_ptr + anch_offset : scan_ptr - anch_offset;
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
				 
						bool reverse = ((OpFlags)program[pc + 3] & OpFlags.RightToLeft) != 0;

						if (qs == null) {
							bool ignore = ((OpFlags)program[pc + 3] & OpFlags.IgnoreCase) != 0;
							string substring = GetString (pc + 3);
							qs = new QuickSearch (substring, ignore, reverse);
						}
						while ((anch_reverse && anch_ptr >= anch_begin) 
						       || (!anch_reverse && anch_ptr <= anch_end)) {

							if (reverse) 	
							{
								anch_ptr = qs.Search (text, anch_ptr, anch_begin);
								if (anch_ptr != -1)
									anch_ptr += qs.Length ;
								
							}
							else
								anch_ptr = qs.Search (text, anch_ptr, anch_end);
							if (anch_ptr < 0)
								break;

							ptr = reverse ? anch_ptr + anch_offset : anch_ptr - anch_offset;
							if (TryMatch (ref ptr, pc + skip))
								goto Pass;

							if (reverse)
								anch_ptr -= 2;
							else 
								++ anch_ptr;
						}
					}
					else if (anch_op == OpCode.True) {					// no anchor
						// Anchor
						//	True

					
						while ((anch_reverse && anch_ptr >= anch_begin) 
						       || (!anch_reverse && anch_ptr <= anch_end)) {

							ptr = anch_ptr;
							if (TryMatch (ref ptr, pc + skip))
								goto Pass;
							if (anch_reverse)
								-- anch_ptr;
							else 
								++ anch_ptr;
						}
					}
					else {									// general case
						// Anchor
						//	<expr>
						//	True

						while ((anch_reverse && anch_ptr >= anch_begin) 
						       || (!anch_reverse && anch_ptr <= anch_end)) {

							ptr = anch_ptr;
							if (Eval (Mode.Match, ref ptr, pc + 3)) {
								// anchor expression passed: try real expression at the correct offset

								ptr = anch_reverse ? anch_ptr + anch_offset : anch_ptr - anch_offset;
								if (TryMatch (ref ptr, pc + skip))
									goto Pass;
							}

						    if (anch_reverse)
								-- anch_ptr;
							else 
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
					else 
					if (ptr + len > text_end)
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
					if (ignore) {
						for (int i = 0; i < len; ++ i) {
							if (Char.ToLower (text[ptr + i]) != Char.ToLower (text[str + i]))
								goto Fail;
						}
					} else {
						for (int i = 0; i < len; ++ i) {
							if (text[ptr + i] != text[str + i])
								goto Fail;
						}
					}

					if (!reverse)
						ptr += len;
					break;
				}

				case OpCode.Character: case OpCode.Category: case OpCode.NotCategory:
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

			        case OpCode.BalanceStart: {

					int start = ptr; //point before the balancing group
					
					if (!Eval (Mode.Match, ref ptr, pc + 5))
						goto Fail;
					
					
					
					if(!Balance (program[pc + 1], program[pc + 2], (program[pc + 3] == 1 ? true : false) , start)) {
						goto Fail;
					}

					
					pc += program[pc + 4];
					break;
				}

				case OpCode.Balance: {
					goto Pass;
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
						ReadProgramCount (pc + 2),		// minimum
						ReadProgramCount (pc + 4),		// maximum
						(flags & OpFlags.Lazy) != 0,	// lazy
						pc + 6				// subexpression
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

					int start = current.Start;
					int start_count = current.Count;

					while (!current.IsMinimum) {
						++ current.Count;
						current.Start = ptr;
						deep = current;
						if (!Eval (Mode.Match, ref ptr, current.Expression)) {
							current.Start = start;
							current.Count = start_count;
							goto Fail;
						}
						if (deep != current)	// recursive mode
							goto Pass;
					}

					if (ptr == current.Start) {
						// degenerate match ... match tail or fail
						this.repeat = current.Previous;
						deep = null;
						if (Eval (Mode.Match, ref ptr, pc + 1))
							goto Pass;
					
						this.repeat = current;
						goto Fail;
					}

					if (current.IsLazy) {
						for (;;) {
							// match tail first ...
							this.repeat = current.Previous;
							deep = null;
							int cp = Checkpoint ();
							if (Eval (Mode.Match, ref ptr, pc + 1))
								goto Pass;

							Backtrack (cp);

							// ... then match more
							this.repeat = current;
							if (current.IsMaximum)
								goto Fail;
							++ current.Count;
							current.Start = ptr;
							deep = current;
							if (!Eval (Mode.Match, ref ptr, current.Expression)) {
								current.Start = start;
								current.Count = start_count;
								goto Fail;
							}
							if (deep != current)	// recursive mode
								goto Pass;
							// Degenerate match: ptr has not moved since the last (failed) tail match.
							// So, next and subsequent tail matches will fail.
							if (ptr == current.Start)
								goto Fail;
						}
					} else {
						int stack_size = stack.Count;

						// match greedily as much as possible
						while (!current.IsMaximum) {
							int cp = Checkpoint ();
							int old_ptr = ptr;
							int old_start = current.Start;

							++ current.Count;
							current.Start = ptr;
							deep = current;
							if (!Eval (Mode.Match, ref ptr, current.Expression)) {
								-- current.Count;
								current.Start = old_start;
								Backtrack (cp);
								break;
							}
							if (deep != current) {
								// recursive mode: no more backtracking, truncate the stack
								stack.Count = stack_size;
								goto Pass;
							}
							stack.Push (cp);
							stack.Push (old_ptr);

							// Degenerate match: no point going on
							if (ptr == current.Start)
								break;
						}

						// then, match the tail, backtracking as necessary.
						this.repeat = current.Previous;
						for (;;) {
							deep = null;
							if (Eval (Mode.Match, ref ptr, pc + 1)) {
								stack.Count = stack_size;
								goto Pass;
							}
							if (stack.Count == stack_size) {
								this.repeat = current;
								goto Fail;
							}

							--current.Count;
							ptr = stack.Pop ();
							Backtrack (stack.Pop ());
						}
					}
				}

				case OpCode.FastRepeat: {
					this.fast = new RepeatContext (
						fast,
						ReadProgramCount (pc + 2),		// minimum
						ReadProgramCount (pc + 4),		// maximum
						(flags & OpFlags.Lazy) != 0,	// lazy
						pc + 6				// subexpression
					);

					fast.Start = ptr;

					int cp = Checkpoint ();

					pc += program[pc + 1];		// tail expression
					ushort tail_word = program[pc];

					int c1 = -1;		// first character of tail operator
					int c2 = -1;		// ... and the same character, in upper case if ignoring case
					int coff = 0;		// 0 or -1 depending on direction

					OpCode tail_op = (OpCode)(tail_word & 0xff);
					if (tail_op == OpCode.Character || tail_op == OpCode.String) {
						OpFlags tail_flags = (OpFlags)(tail_word & 0xff00);

						if ((tail_flags & OpFlags.Negate) != 0)
							goto skip;

						if (tail_op == OpCode.String)
						{
							int offset = 0;
						
							if ((tail_flags & OpFlags.RightToLeft) != 0)
							{
								offset = program[pc + 1] - 1 ;
							}
							  
							c1 = program[pc + 2 + offset];				// first char of string
						}
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

				skip:
					if (fast.IsLazy) {
						if (!fast.IsMinimum && !Eval (Mode.Count, ref ptr, fast.Expression)) {
							//Console.WriteLine ("lazy fast: failed mininum.");
							fast = fast.Previous;
							goto Fail;
						}
						
						while (true) {
							int p = ptr + coff;
							if (c1 < 0 || (p >= 0 && p < text_end && (c1 == text[p] || c2 == text[p]))) {
								deep = null;
								if (Eval (Mode.Match, ref ptr, pc))
									break;
							}

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
							if (c1 < 0 || (p >= 0 && p < text_end && (c1 == text[p] || c2 == text[p]))) {
								deep = null;
								if (Eval (Mode.Match, ref ptr, pc))
									break;
							}

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

				case OpCode.NotCategory: {
					if (!CategoryUtils.IsCategory ((Category)program[pc ++], c))
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
			Debug.Assert (cp > mark_start, "Regex", "Attempt to backtrack forwards");
			for (int i = 0; i < groups.Length; ++ i) {
				int m = groups [i];
				while (cp <= m) {
					marks [m].Start = -1;
					m = marks [m].Previous;
				}

				groups [i] = m;
			}
			mark_start = cp;
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
				Capture cap = new Capture (text, marks [m].Index, marks [m].Length);
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
				return new Match (regex, this, text, text_end, 0, marks [first_mark_index].Index, marks [first_mark_index].Length);

			Match retval = new Match (regex, this, text, text_end, groups.Length, 
						  marks [first_mark_index].Index, marks [first_mark_index].Length, n_caps);
			PopulateGroup (retval, first_mark_index, n_caps);

			for (int gid = 1; gid < groups.Length; ++ gid) {
				GetGroupInfo (gid, out first_mark_index, out n_caps);
				if (first_mark_index < 0) {
					g = Group.Fail;
				} else {
					g = new Group (text, marks [first_mark_index].Index, marks [first_mark_index].Length, n_caps);
					PopulateGroup (g, first_mark_index, n_caps);
				}
				retval.Groups.SetValue (g, gid);
			}
			return retval;
		}

		// interpreter attributes

		private ushort[] program;		// regex program
		private int program_start;		// first instruction after info block
		private string text;			// input text
		private int text_end;			// end of input text (last character + 1)
		private int group_count;		// number of capturing groups
		private int match_min;//, match_max;	// match width information
		private QuickSearch qs;			// fast substring matcher

		// match state
		
		private int scan_ptr;			// start of scan

		private RepeatContext repeat;		// current repeat context
		private RepeatContext fast;		// fast repeat context

		// Repeat/Until handling
		private IntStack stack = new IntStack (); // utility stack
		private RepeatContext deep;		// points to the most-nested repeat context

		private Mark[] marks = null;		// mark stack
		private int mark_start;			// start of current checkpoint
		private int mark_end;			// end of checkpoint/next free mark

		private int[] groups;			// current group definitions

		// private classes

		private struct IntStack {
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

		private enum Mode {
			Search,
			Match,
			Count
		}
	}
}
