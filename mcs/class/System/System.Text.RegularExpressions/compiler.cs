//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	compiler.cs
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
using System.Diagnostics;
using System.Collections;

namespace System.Text.RegularExpressions {
	abstract class LinkRef {
#if TRACE_REGEX
		static int next_label = 1;
		int label = next_label++;

		public override string ToString ()
		{
			return "L" + label.ToString ();
		}
#endif
	}
		
	interface ICompiler {
		void Reset ();
		IMachineFactory GetMachineFactory ();

		// instruction emission

		void EmitFalse ();
		void EmitTrue ();

		// character matching

		void EmitCharacter (char c, bool negate, bool ignore, bool reverse);
		void EmitCategory (Category cat, bool negate, bool reverse);
		void EmitNotCategory (Category cat, bool negate, bool reverse);
		void EmitRange (char lo, char hi, bool negate, bool ignore, bool reverse);
		void EmitSet (char lo, BitArray set, bool negate, bool ignore, bool reverse);

		// other operators

		void EmitString (string str, bool ignore, bool reverse);
		void EmitPosition (Position pos);
		void EmitOpen (int gid);
		void EmitClose (int gid);
		void EmitBalanceStart(int gid, int balance, bool capture,  LinkRef tail);
		void EmitBalance ();
		void EmitReference (int gid, bool ignore, bool reverse);

		// constructs

		void EmitIfDefined (int gid, LinkRef tail);
		void EmitSub (LinkRef tail);
		void EmitTest (LinkRef yes, LinkRef tail);
		void EmitBranch (LinkRef next);
		void EmitJump (LinkRef target);
		void EmitRepeat (int min, int max, bool lazy, LinkRef until);
		void EmitUntil (LinkRef repeat);
		void EmitIn (LinkRef tail);
		void EmitInfo (int count, int min, int max);
		void EmitFastRepeat (int min, int max, bool lazy, LinkRef tail);
		void EmitAnchor (bool reverse, int offset, LinkRef tail);

		// event for the CILCompiler
		void EmitBranchEnd();
		void EmitAlternationEnd();

		LinkRef NewLink ();
		void ResolveLink (LinkRef link);
	}

	class InterpreterFactory : IMachineFactory {
		public InterpreterFactory (ushort[] pattern) {
			this.pattern = pattern;
		}
		
		public IMachine NewInstance () {
			return new Interpreter (pattern);
		}

		public int GroupCount {
			get { return pattern[1]; }
		}

		public int Gap {
			get { return gap; }
			set { gap = value; }
		}

		public IDictionary Mapping {
			get { return mapping; }
			set { mapping = value; }
		}

		public string [] NamesMapping {
			get { return namesMapping; }
			set { namesMapping = value; }
		}

		private IDictionary mapping;
		private ushort[] pattern;
		private string [] namesMapping;
		private int gap;
	}

	class PatternCompiler : ICompiler {
		public static ushort EncodeOp (OpCode op, OpFlags flags) {
			return (ushort)((int)op | ((int)flags & 0xff00));
		}

		[Conditional ("TRACE_REGEX")]
		static void TraceRegexp (string fmt, params object[] args)
		{
			Console.Write ("\t");
			Console.WriteLine (fmt, args);
		}

		[Conditional ("TRACE_REGEX")]
		static void TraceRegexpLabel (LinkRef lref)
		{
			Console.Write ("{0}:", lref);
		}

		public static void DecodeOp (ushort word, out OpCode op, out OpFlags flags) {
			op = (OpCode)(word & 0x00ff);
			flags = (OpFlags)(word & 0xff00);
		}

		public PatternCompiler () {
			pgm = new ArrayList ();
		}

		// ICompiler implementation

		public void Reset () {
			pgm.Clear ();
		}

		public IMachineFactory GetMachineFactory () {
			ushort[] image = new ushort[pgm.Count];
			pgm.CopyTo (image);

			return new InterpreterFactory (image);
		}

		public void EmitFalse () {
			Emit (OpCode.False);
			TraceRegexp ("false");
		}

		public void EmitTrue () {
			Emit (OpCode.True);

			TraceRegexp ("true");
		}

		void EmitCount (int count)
		{
			uint ucount = (uint) count;
			Emit ((ushort) (ucount & 0xFFFF)); // lo 16bits
			Emit ((ushort) (ucount >> 16));	   // hi
		}

		public void EmitCharacter (char c, bool negate, bool ignore, bool reverse) {
			Emit (OpCode.Character, MakeFlags (negate, ignore, reverse, false));

			if (ignore)
				c = Char.ToLower (c);

			Emit ((ushort)c);

			TraceRegexp ("character {0} negate {1} ignore {2} reverse {3}", c, negate, ignore, reverse);
		}

		public void EmitCategory (Category cat, bool negate, bool reverse) {
			Emit (OpCode.Category, MakeFlags (negate, false, reverse, false));
			Emit ((ushort)cat);

			TraceRegexp ("category {0} negate {1} reverse {2}", cat, negate, reverse);
		}

		public void EmitNotCategory (Category cat, bool negate, bool reverse) {
			Emit (OpCode.NotCategory, MakeFlags (negate, false, reverse, false));
			Emit ((ushort)cat);

			TraceRegexp ("not category {0} negate {1} reverse {2}", cat, negate, reverse);
		}

		public void EmitRange (char lo, char hi, bool negate, bool ignore, bool reverse) {
			Emit (OpCode.Range, MakeFlags (negate, ignore, reverse, false));
			Emit ((ushort)lo);
			Emit ((ushort)hi);

			TraceRegexp ("char range '{0}' - '{1}' negate {2} ignore {3} reverse {4}", lo, hi, negate, ignore, reverse);
		}

		public void EmitSet (char lo, BitArray set, bool negate, bool ignore, bool reverse) {
			Emit (OpCode.Set, MakeFlags (negate, ignore, reverse, false));
			Emit ((ushort)lo);

			int len = (set.Length + 0xf) >> 4;
			Emit ((ushort)len);

			int b = 0;
			while (len -- != 0) {
				ushort word = 0;
				for (int i = 0; i < 16; ++ i) {
					if (b >= set.Length)
						break;
				
					if (set[b ++])
						word |= (ushort)(1 << i);
				}

				Emit (word);
			}

			TraceRegexp ("set lo '{0}' - '{1}' negate {2} ignore {3} reverse {4}", lo, set, negate, ignore, reverse);
		}

		public void EmitString (string str, bool ignore, bool reverse) {
			Emit (OpCode.String, MakeFlags (false, ignore, reverse, false));
			int len = str.Length;
			Emit ((ushort)len);

			if (ignore)
				str = str.ToLower ();
			
			for (int i = 0; i < len; ++ i)
				Emit ((ushort)str[i]);
			TraceRegexp ("string '{0}' ignore {1} reverse {2}", str, ignore, reverse);
		}

		public void EmitPosition (Position pos) {
			Emit (OpCode.Position, 0);
			Emit ((ushort)pos);

			TraceRegexp ("position {0}", pos);
		}

		public void EmitOpen (int gid) {
			Emit (OpCode.Open);
			Emit ((ushort)gid);

			TraceRegexp ("open {0}", gid);
		}

		public void EmitClose (int gid) {
			Emit (OpCode.Close);
			Emit ((ushort)gid);

			TraceRegexp ("close {0}", gid);
		}

	       

		public void EmitBalanceStart (int gid, int balance, bool capture, LinkRef tail) {
			BeginLink (tail);
			Emit (OpCode.BalanceStart);
			Emit ((ushort)gid);
			Emit ((ushort)balance);
			Emit ((ushort)(capture ? 1 : 0));
			EmitLink (tail);

			TraceRegexp ("balance start gid {0} balance {1} capture {2} tail {3}", gid, balance, capture, tail);
		}

		public void EmitBalance () {
			Emit (OpCode.Balance);

			TraceRegexp ("balance");
		}

		public void EmitReference (int gid, bool ignore, bool reverse) {
			Emit (OpCode.Reference, MakeFlags (false, ignore, reverse, false));
			Emit ((ushort)gid);

			TraceRegexp ("reference gid {0} ignore {1} reverse {2}", gid, ignore, reverse);
		}

		public void EmitIfDefined (int gid, LinkRef tail) {
			BeginLink (tail);
			Emit (OpCode.IfDefined);
			EmitLink (tail);
			Emit ((ushort)gid);

			TraceRegexp ("if defined gid {1} tail {2}", gid, tail);
		}

		public void EmitSub (LinkRef tail) {
			BeginLink (tail);
			Emit (OpCode.Sub);
			EmitLink (tail);
	
			TraceRegexp ("sub {0}", tail);
		}

		public void EmitTest (LinkRef yes, LinkRef tail) {
			BeginLink (yes);
			BeginLink (tail);
			Emit (OpCode.Test);
			EmitLink (yes);
			EmitLink (tail);

			TraceRegexp ("test yes {0} tail {1}", yes, tail);
		}

		public void EmitBranch (LinkRef next) {
			BeginLink (next);
			Emit (OpCode.Branch, 0);
			EmitLink (next);

			TraceRegexp ("branch next {0}", next);
		}

		public void EmitJump (LinkRef target) {
			BeginLink (target);
			Emit (OpCode.Jump, 0);
			EmitLink (target);

			TraceRegexp ("jmp target {0}", target);
		}

		public void EmitRepeat (int min, int max, bool lazy, LinkRef until) {
			BeginLink (until);
			Emit (OpCode.Repeat, MakeFlags (false, false, false, lazy));
			EmitLink (until);
			EmitCount (min);
			EmitCount (max);

			TraceRegexp ("repeat min {0} max {1} lazy {2} until {3}", min, max, lazy, until);
		}

		public void EmitUntil (LinkRef repeat) {
			ResolveLink (repeat);
			Emit (OpCode.Until);

			TraceRegexp ("end until {0}", repeat);
		}

		public void EmitFastRepeat (int min, int max, bool lazy, LinkRef tail) {
			BeginLink (tail);
			Emit (OpCode.FastRepeat, MakeFlags (false, false, false, lazy));
			EmitLink (tail);
			EmitCount (min);
			EmitCount (max);

			TraceRegexp ("repeat-fast min {0} max {1} lazy {2} tail {3}", min, max, lazy, tail);
		}

		public void EmitIn (LinkRef tail) {
			BeginLink (tail);
			Emit (OpCode.In);
			EmitLink (tail);
	
			TraceRegexp ("in tail {0}", tail);
		}

		public void EmitAnchor (bool reverse, int offset, LinkRef tail) {
			BeginLink (tail);
			Emit (OpCode.Anchor, MakeFlags(false, false, reverse, false));
			EmitLink (tail);
			Emit ((ushort)offset);

			TraceRegexp ("anchor reverse {0} offset {1} tail {2}", reverse, offset, tail);
		}

		public void EmitInfo (int count, int min, int max) {
			Emit (OpCode.Info);
			EmitCount (count);
			EmitCount (min);
			EmitCount (max);

			TraceRegexp ("info group count {0} match_min {1} match_max {2}", count, min, max);
		}

		public LinkRef NewLink () {
			return new PatternLinkStack ();
		}
		
		public void ResolveLink (LinkRef lref) {
			PatternLinkStack stack = (PatternLinkStack)lref;
		
			while (stack.Pop ())
				pgm[stack.OffsetAddress] = (ushort)stack.GetOffset (CurrentAddress);

			TraceRegexpLabel (lref);
		}

		public void EmitBranchEnd(){}
		public void EmitAlternationEnd(){}

		// private members

		private static OpFlags MakeFlags (bool negate, bool ignore, bool reverse, bool lazy) {
			OpFlags flags = 0;
			if (negate) flags |= OpFlags.Negate;
			if (ignore) flags |= OpFlags.IgnoreCase;
			if (reverse) flags |= OpFlags.RightToLeft;
			if (lazy) flags |= OpFlags.Lazy;

			return flags;
		}
		
		private void Emit (OpCode op) {
			Emit (op, (OpFlags)0);
		}

		private void Emit (OpCode op, OpFlags flags) {
			Emit (EncodeOp (op, flags));
		}

		private void Emit (ushort word) {
			pgm.Add (word);
		}

		private int CurrentAddress {
			get { return pgm.Count; }
		}

		private void BeginLink (LinkRef lref) {
			PatternLinkStack stack = (PatternLinkStack)lref;
			stack.BaseAddress = CurrentAddress;
		}

		private void EmitLink (LinkRef lref) {
			PatternLinkStack stack = (PatternLinkStack)lref;
			stack.OffsetAddress = CurrentAddress;
			Emit ((ushort)0);	// placeholder
			stack.Push ();
		}


		private class PatternLinkStack : LinkStack {
			public PatternLinkStack () {
			}
		
			public int BaseAddress {
				set { link.base_addr = value; }
			}

			public int OffsetAddress {
				get { return link.offset_addr; }
				set { link.offset_addr = value; }
			}

			public int GetOffset (int target_addr) {
				return target_addr - link.base_addr;
			}

			// LinkStack implementation

			protected override object GetCurrent () { return link; }
			protected override void SetCurrent (object l) { link = (Link)l; }

			private struct Link {
				public int base_addr;
				public int offset_addr;
			}

			Link link;
		}

		private ArrayList pgm;
	}

	abstract class LinkStack : LinkRef {
		public LinkStack () {
			stack = new Stack ();
		}

		public void Push () {
			stack.Push (GetCurrent ());
		}

		public bool Pop () {
			if (stack.Count > 0) {
				SetCurrent (stack.Pop ());
				return true;
			}

			return false;
		}

		protected abstract object GetCurrent ();
		protected abstract void SetCurrent (object l);

		private Stack stack;
	}

	//Used by CILCompiler and Interpreter
	internal struct Mark {
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

}
