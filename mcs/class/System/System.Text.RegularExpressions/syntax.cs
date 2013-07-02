//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	syntax.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
//		(c) 2002

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

namespace System.Text.RegularExpressions.Syntax {
	// collection classes

	class ExpressionCollection : CollectionBase {
		public void Add (Expression e) {
			List.Add (e);
		}

		public Expression this[int i] {
			get { return (Expression)List[i]; }
			set { List[i] = value; }
		}

		protected override void OnValidate (object o) {
			// allow null elements
		}
	}

	// abstract classes

	abstract class Expression {
		public abstract void Compile (ICompiler cmp, bool reverse);
		public abstract void GetWidth (out int min, out int max);

		public int GetFixedWidth () {
			int min, max;
			GetWidth (out min, out max);

			if (min == max)
				return min;

			return -1;
		}

		public virtual AnchorInfo GetAnchorInfo (bool reverse) {
			return new AnchorInfo (this, GetFixedWidth ());
		}

		public abstract bool IsComplex ();
	}

	// composite expressions

	abstract class CompositeExpression : Expression {
		public CompositeExpression () {
			expressions = new ExpressionCollection ();
		}

		protected ExpressionCollection Expressions {
			get { return expressions; }
		}

		protected void GetWidth (out int min, out int max, int count) {
			min = Int32.MaxValue;
			max = 0;
			bool empty = true;

			for (int i = 0; i < count; ++ i) {
				Expression e = Expressions[i];
				if (e == null)
					continue;

				empty = false;
				int a, b;
				e.GetWidth (out a, out b);
				if (a < min) min = a;
				if (b > max) max = b;
			}

			if (empty)
				min = max = 0;
		}


		public override bool IsComplex ()
		{
			foreach (Expression e in Expressions) {
				if (e.IsComplex ())
					return true;
			}
			return GetFixedWidth () <= 0;
		}

		private ExpressionCollection expressions;
	}

	// groups

	class Group : CompositeExpression {
		public Group () {
		}

		public Expression Expression {
			get { return Expressions[0]; }
			set { Expressions[0] = value; }
		}

		public void AppendExpression (Expression e) {
			Expressions.Add (e);
		}

		public override void Compile (ICompiler cmp, bool reverse) {
			int count = Expressions.Count;
			for (int i = 0; i < count; ++ i) {
				Expression e;
				if (reverse)
					e = Expressions[count - i - 1];
				else
					e = Expressions[i];

				e.Compile (cmp, reverse);
			}
		}

		public override void GetWidth (out int min, out int max) {
			min = 0;
			max = 0;

			foreach (Expression e in Expressions) {
				int a, b;
				e.GetWidth (out a, out b);
				min += a;
				if (max == Int32.MaxValue || b == Int32.MaxValue)
					max = Int32.MaxValue;
				else
					max += b;
			}
		}

		public override AnchorInfo GetAnchorInfo (bool reverse)
		{
			int ptr;
			int width = GetFixedWidth ();

			ArrayList infos = new ArrayList ();
			IntervalCollection segments = new IntervalCollection ();

			// accumulate segments
			ptr = 0;
			int count = Expressions.Count;
			for (int i = 0; i < count; ++ i) {
				Expression e;
				if (reverse)
					e = Expressions [count - i - 1];
				else
					e = Expressions [i];

				AnchorInfo info = e.GetAnchorInfo (reverse);
				infos.Add (info);

				if (info.IsPosition)
					return new AnchorInfo (this, ptr + info.Offset, width, info.Position);

				if (info.IsSubstring)
					segments.Add (info.GetInterval (ptr));

				if (info.IsUnknownWidth)
					break;

				ptr += info.Width;
			}

			// normalize and find the longest segment
			segments.Normalize ();

			Interval longest = Interval.Empty;
			foreach (Interval segment in segments) {
				if (segment.Size > longest.Size)
					longest = segment;
			}

			if (longest.IsEmpty)
				return new AnchorInfo (this, width);

			// now chain the substrings that made this segment together
			bool ignore = false;
			int n_strings = 0;

			ptr = 0;
			for (int i = 0; i < infos.Count; ++i) {
				AnchorInfo info = (AnchorInfo) infos [i];

				if (info.IsSubstring && longest.Contains (info.GetInterval (ptr))) {
					ignore |= info.IgnoreCase;
					infos [n_strings ++] = info;
				}

				if (info.IsUnknownWidth)
					break;

				ptr += info.Width;
			}

			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < n_strings; ++i) {
				AnchorInfo info;
				if (reverse)
					info = (AnchorInfo) infos [n_strings - i - 1];
				else
					info = (AnchorInfo) infos [i];
				sb.Append (info.Substring);
			}

			if (sb.Length == longest.Size)
				return new AnchorInfo (this, longest.low, width, sb.ToString (), ignore);
			// were the string segments overlapping?
			if (sb.Length > longest.Size) {
				Console.Error.WriteLine ("overlapping?");
				return new AnchorInfo (this, width);
			}
			throw new SystemException ("Shouldn't happen");
		}
	}

	class RegularExpression : Group {
		public RegularExpression () {
			group_count = 0;
		}

		public int GroupCount {
			get { return group_count; }
			set { group_count = value; }
		}

		public override void Compile (ICompiler cmp, bool reverse) {
			// info block

			int min, max;
			GetWidth (out min, out max);
			cmp.EmitInfo (group_count, min, max);

			// anchoring expression

			AnchorInfo info = GetAnchorInfo (reverse);
			//if (reverse)
			//	info = new AnchorInfo (this, GetFixedWidth ());	// FIXME

			LinkRef pattern = cmp.NewLink ();
			cmp.EmitAnchor (reverse, info.Offset, pattern);

			if (info.IsPosition)
				cmp.EmitPosition (info.Position);
			else if (info.IsSubstring)
				cmp.EmitString (info.Substring, info.IgnoreCase, reverse);

			cmp.EmitTrue ();

			// pattern

			cmp.ResolveLink (pattern);
			base.Compile (cmp, reverse);
			cmp.EmitTrue ();
		}

		private int group_count;
	}

	class CapturingGroup : Group, IComparable {
		public CapturingGroup () {
			this.gid = 0;
			this.name = null;
		}

		public int Index {
			get { return gid; }
			set { gid = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public bool IsNamed {
			get { return name != null; }
		}

		public override void Compile (ICompiler cmp, bool reverse) {
			cmp.EmitOpen (gid);
			base.Compile (cmp, reverse);
			cmp.EmitClose (gid);
		}

		public override bool IsComplex () {
			return true;
		}

		public int CompareTo (object other)
		{
			return gid - ((CapturingGroup) other).gid;
		}

		private int gid;
		private string name;
	}

	class BalancingGroup : CapturingGroup {
		public BalancingGroup () {
			this.balance = null;
		}

		public CapturingGroup Balance {
			get { return balance; }
			set { balance = value; }
		}

		public override void Compile (ICompiler cmp, bool reverse) {
			// can't invoke Group.Compile from here :(
			// so I'll just repeat the code

			LinkRef tail = cmp.NewLink ();

			cmp.EmitBalanceStart (this.Index, balance.Index, this.IsNamed,  tail);

			int count = Expressions.Count;
			for (int i = 0; i < count; ++ i) {
				Expression e;
				if (reverse)
					e = Expressions[count - i - 1];
				else
					e = Expressions[i];

				e.Compile (cmp, reverse);
			}

			cmp.EmitBalance ();
			cmp.ResolveLink(tail);
		}

		private CapturingGroup balance;
	}

	class NonBacktrackingGroup : Group {
		public NonBacktrackingGroup () {
		}

		public override void Compile (ICompiler cmp, bool reverse) {
			LinkRef tail = cmp.NewLink ();

			cmp.EmitSub (tail);
			base.Compile (cmp, reverse);
			cmp.EmitTrue ();
			cmp.ResolveLink (tail);
		}

		public override bool IsComplex () {
			return true;
		}
	}

	// repetition

	class Repetition : CompositeExpression {
		public Repetition (int min, int max, bool lazy) {
			Expressions.Add (null);

			this.min = min;
			this.max = max;
			this.lazy = lazy;
		}

		public Expression Expression {
			get { return Expressions[0]; }
			set { Expressions[0] = value; }
		}

		public int Minimum {
			get { return min; }
			set { min = value; }
		}

		public int Maximum {
			get { return max; }
			set { max = value; }
		}

		public bool Lazy {
			get { return lazy; }
			set { lazy = value; }
		}

		public override void Compile (ICompiler cmp, bool reverse) {
			if (Expression.IsComplex ()) {
				LinkRef until = cmp.NewLink ();

				cmp.EmitRepeat (min, max, lazy, until);
				Expression.Compile (cmp, reverse);
				cmp.EmitUntil (until);
			}
			else {
				LinkRef tail = cmp.NewLink ();

				cmp.EmitFastRepeat (min, max, lazy, tail);
				Expression.Compile (cmp, reverse);
				cmp.EmitTrue ();
				cmp.ResolveLink (tail);
			}
		}

		public override void GetWidth (out int min, out int max) {
			Expression.GetWidth (out min, out max);
			min = min * this.min;
			if (max == Int32.MaxValue || this.max == 0xffff)
				max = Int32.MaxValue;
			else
				max = max * this.max;
		}

		public override AnchorInfo GetAnchorInfo (bool reverse) {
			int width = GetFixedWidth ();
			if (Minimum == 0)
				return new AnchorInfo (this, width);

			AnchorInfo info = Expression.GetAnchorInfo (reverse);
			if (info.IsPosition)
				return new AnchorInfo (this, info.Offset, width, info.Position);

			if (info.IsSubstring) {
				if (info.IsComplete) {
					// Minimum > 0
					string str = info.Substring;
					StringBuilder sb = new StringBuilder (str);
					for (int i = 1; i < Minimum; ++ i)
						sb.Append (str);

					return new AnchorInfo (this, 0, width, sb.ToString (), info.IgnoreCase);
				}

				return new AnchorInfo (this, info.Offset, width, info.Substring, info.IgnoreCase);
			}

			return new AnchorInfo (this, width);
		}

		private int min, max;
		private bool lazy;
	}

	// assertions

	abstract class Assertion : CompositeExpression {
		public Assertion () {
			Expressions.Add (null);		// true expression
			Expressions.Add (null);		// false expression
		}

		public Expression TrueExpression {
			get { return Expressions[0]; }
			set { Expressions[0] = value; }
		}

		public Expression FalseExpression {
			get { return Expressions[1]; }
			set { Expressions[1] = value; }
		}

		public override void GetWidth (out int min, out int max) {
			GetWidth (out min, out max, 2);

			if (TrueExpression == null || FalseExpression == null)
				min = 0;
		}
	}

	class CaptureAssertion : Assertion {
		public CaptureAssertion (Literal l) {
			literal = l;
		}

		public CapturingGroup CapturingGroup {
			get { return group; }
			set { group = value; }
		}

		public override void Compile (ICompiler cmp, bool reverse) {
			if (group == null) {
				Alternate.Compile (cmp, reverse);
				return;
			}

			int gid = group.Index;
			LinkRef tail = cmp.NewLink ();

			if (FalseExpression == null) {
				//    IfDefined :1
				//      <yes_exp>
				// 1: <tail>

				cmp.EmitIfDefined (gid, tail);
				TrueExpression.Compile (cmp, reverse);
			}
			else {
				//    IfDefined :1
				//      <yes_expr>
				//      Jump :2
				// 1:   <no_expr>
				// 2: <tail>

				LinkRef false_expr = cmp.NewLink ();
				cmp.EmitIfDefined (gid, false_expr);
				TrueExpression.Compile (cmp, reverse);
				cmp.EmitJump (tail);
				cmp.ResolveLink (false_expr);
				FalseExpression.Compile (cmp, reverse);
			}

			cmp.ResolveLink (tail);
		}

		public override bool IsComplex () {
			if (group == null)
				return Alternate.IsComplex ();
			if (TrueExpression != null && TrueExpression.IsComplex ())
				return true;
			if (FalseExpression != null && FalseExpression.IsComplex ())
				return true;
			return GetFixedWidth () <= 0;
		}

		ExpressionAssertion Alternate {
			get {
				if (alternate == null) {
					alternate = new ExpressionAssertion ();
					alternate.TrueExpression = TrueExpression;
					alternate.FalseExpression = FalseExpression;
					alternate.TestExpression = literal;
				}
				return alternate;
			}
		}

		private ExpressionAssertion alternate;
		private CapturingGroup group;
		private Literal literal;
	}

	class ExpressionAssertion : Assertion {
		public ExpressionAssertion () {
			Expressions.Add (null);		// test expression
		}

		public bool Reverse {
			get { return reverse; }
			set { reverse = value; }
		}

		public bool Negate {
			get { return negate; }
			set { negate = value; }
		}

		public Expression TestExpression {
			get { return Expressions[2]; }
			set { Expressions[2] = value; }
		}

		public override void Compile (ICompiler cmp, bool reverse) {
			LinkRef true_expr = cmp.NewLink ();
			LinkRef false_expr = cmp.NewLink ();

			// test op: positive / negative

			if (!negate)
				cmp.EmitTest (true_expr, false_expr);
			else
				cmp.EmitTest (false_expr, true_expr);

			// test expression: lookahead / lookbehind

			TestExpression.Compile (cmp, this.reverse);
			cmp.EmitTrue ();

			// target expressions

			if (TrueExpression == null) {			// (?= ...)
				//    Test :1, :2
				//      <test_expr>
				// :2   False
				// :1   <tail>

				cmp.ResolveLink (false_expr);
				cmp.EmitFalse ();
				cmp.ResolveLink (true_expr);
			}
			else {
				cmp.ResolveLink (true_expr);
				TrueExpression.Compile (cmp, reverse);

				if (FalseExpression == null) {		// (?(...) ...)
					//    Test :1, :2
					//      <test_expr>
					// :1   <yes_expr>
					// :2   <tail>

					cmp.ResolveLink (false_expr);
				}
				else {					// (?(...) ... | ...)
					//    Test :1, :2
					//      <test_expr>
					// :1   <yes_expr>
					//      Jump :3
					// :2   <no_expr>
					// :3   <tail>

					LinkRef tail = cmp.NewLink ();

					cmp.EmitJump (tail);
					cmp.ResolveLink (false_expr);
					FalseExpression.Compile (cmp, reverse);
					cmp.ResolveLink (tail);
				}
			}
		}

		public override bool IsComplex ()
		{
			return true;
		}

		private bool reverse, negate;
	}

	// alternation

	class Alternation : CompositeExpression {
		public Alternation () {
		}

		public ExpressionCollection Alternatives {
			get { return Expressions; }
		}

		public void AddAlternative (Expression e) {
			Alternatives.Add (e);
		}

		public override void Compile (ICompiler cmp, bool reverse) {
			//			LinkRef next = cmp.NewLink ();
			LinkRef tail = cmp.NewLink ();

			foreach (Expression e in Alternatives) {
				LinkRef next = cmp.NewLink ();
				cmp.EmitBranch (next);
				e.Compile (cmp, reverse);
				cmp.EmitJump (tail);
				cmp.ResolveLink (next);
				cmp.EmitBranchEnd();
			}

			cmp.EmitFalse ();
			cmp.ResolveLink (tail);
			cmp.EmitAlternationEnd();
		}

		public override void GetWidth (out int min, out int max) {
			GetWidth (out min, out max, Alternatives.Count);
		}
	}

	// terminal expressions

	class Literal : Expression {
		public Literal (string str, bool ignore) {
			this.str = str;
			this.ignore = ignore;
		}

		public string String {
			get { return str; }
			set { str = value; }
		}

		public bool IgnoreCase {
			get { return ignore; }
			set { ignore = value; }
		}

		public static void CompileLiteral (string str, ICompiler cmp, bool ignore, bool reverse)
		{
			if (str.Length == 0)
				return;

			if (str.Length == 1)
				cmp.EmitCharacter (str[0], false, ignore, reverse);
			else
				cmp.EmitString (str, ignore, reverse);
		}

		public override void Compile (ICompiler cmp, bool reverse)
		{
			CompileLiteral (str, cmp, ignore, reverse);
		}

		public override void GetWidth (out int min, out int max) {
			min = max = str.Length;
		}

		public override AnchorInfo GetAnchorInfo (bool reverse) {
			return new AnchorInfo (this, 0, str.Length, str, ignore);
		}

		public override bool IsComplex () {
			return false;
		}

		private string str;
		private bool ignore;
	}

	class PositionAssertion : Expression {
		public PositionAssertion (Position pos) {
			this.pos = pos;
		}

		public Position Position {
			get { return pos; }
			set { pos = value; }
		}

		public override void Compile (ICompiler cmp, bool reverse) {
			cmp.EmitPosition (pos);
		}

		public override void GetWidth (out int min, out int max) {
			min = max = 0;
		}

		public override bool IsComplex () {
			return false;
		}

		public override AnchorInfo GetAnchorInfo (bool revers) {
			switch (pos) {
			case Position.StartOfString: case Position.StartOfLine: case Position.StartOfScan:
				return new AnchorInfo (this, 0, 0, pos);

			default:
				return new AnchorInfo (this, 0);
			}
		}

		private Position pos;
	}

	class Reference : Expression {
		public Reference (bool ignore) {
			this.ignore = ignore;
		}

		public CapturingGroup CapturingGroup {
			get { return group; }
			set { group = value; }
		}

		public bool IgnoreCase {
			get { return ignore; }
			set { ignore = value; }
		}

		public override void Compile (ICompiler cmp, bool reverse) {
			cmp.EmitReference (group.Index, ignore, reverse);
		}

		public override void GetWidth (out int min, out int max) {
			//group.GetWidth (out min, out max);
			// TODO set width to referenced group for non-cyclical references
			min = 0;
			max = Int32.MaxValue;
		}

		public override bool IsComplex () {
			return true;	// FIXME incorporate cyclic check
		}

		private CapturingGroup group;
		private bool ignore;
	}

	class BackslashNumber : Reference {
		string literal;
		bool ecma;

		public BackslashNumber (bool ignore, bool ecma)
			: base (ignore)
		{
			this.ecma = ecma;
		}

		// Precondition: groups [num_str] == null
		public bool ResolveReference (string num_str, Hashtable groups)
		{
			if (ecma) {
				int last_i = 0;
				for (int i = 1; i < num_str.Length; ++i) {
					if (groups [num_str.Substring (0, i)] != null)
						last_i = i;
				}
				if (last_i != 0) {
					CapturingGroup = (CapturingGroup) groups [num_str.Substring (0, last_i)];
					literal = num_str.Substring (last_i);
					return true;
				}
			} else {
				if (num_str.Length == 1)
					return false;
			}

			int ptr = 0;
			int as_octal = Parser.ParseOctal (num_str, ref ptr);
			// Since ParseOctal reads at most 3 digits, as_octal <= octal 0777
			if (as_octal == -1)
				return false;
			if (as_octal > 0xff && ecma) {
				as_octal /= 8;
				--ptr;
			}
			as_octal &= 0xff;
			literal = ((char) as_octal) + num_str.Substring (ptr);
			return true;
		}

		public override void Compile (ICompiler cmp, bool reverse)
		{
			if (CapturingGroup != null)
				base.Compile (cmp, reverse);
			if (literal != null)
				Literal.CompileLiteral (literal, cmp, IgnoreCase, reverse);
		}
	}

	class CharacterClass : Expression {
		public CharacterClass (bool negate, bool ignore) {
			this.negate = negate;
			this.ignore = ignore;

			intervals = new IntervalCollection ();

			// initialize pos/neg category arrays

			int cat_size = (int) Category.LastValue;
			pos_cats = new BitArray (cat_size);
			neg_cats = new BitArray (cat_size);
		}

		public CharacterClass (Category cat, bool negate) : this (false, false) {
			this.AddCategory (cat, negate);
		}

		public bool Negate {
			get { return negate; }
			set { negate = value; }
		}

		public bool IgnoreCase {
			get { return ignore; }
			set { ignore = value; }
		}

		public void AddCategory (Category cat, bool negate) {
			int n = (int)cat;

			if (negate) {
				neg_cats[n] = true;
			} else {
				pos_cats[n] = true;
			}
		}

		public void AddCharacter (char c) {
			// TODO: this is certainly not the most efficient way of doing things
			// TODO: but at least it produces correct results.
			AddRange (c, c);
		}

		public void AddRange (char lo, char hi) {
			Interval new_interval = new Interval (lo, hi);

			// ignore case is on. we must make sure our interval does not
			// use upper case. if it does, we must normalize the upper case
			// characters into lower case.
			if (ignore) {
				if (upper_case_characters.Intersects (new_interval)) {
					Interval partial_new_interval;

					if (new_interval.low < upper_case_characters.low) {
						partial_new_interval = new Interval (upper_case_characters.low + distance_between_upper_and_lower_case,
										     new_interval.high +  distance_between_upper_and_lower_case);
						new_interval.high = upper_case_characters.low - 1;
					}
					else {
						partial_new_interval = new Interval (new_interval.low + distance_between_upper_and_lower_case,
										     upper_case_characters.high + distance_between_upper_and_lower_case);
						new_interval.low = upper_case_characters.high + 1;
					}
					intervals.Add (partial_new_interval);
				}
				else if (upper_case_characters.Contains (new_interval)) {
					new_interval.high += distance_between_upper_and_lower_case;
					new_interval.low += distance_between_upper_and_lower_case;
				}
			}
			intervals.Add (new_interval);
		}

		public override void Compile (ICompiler cmp, bool reverse) {
			// create the meta-collection
			IntervalCollection meta =
				intervals.GetMetaCollection (new IntervalCollection.CostDelegate (GetIntervalCost));

			// count ops
			int count = meta.Count;
			for (int i = 0; i < pos_cats.Length; ++ i) {
				if (pos_cats[i] || neg_cats [i])
					++ count;
			}

			if (count == 0)
				return;

			// emit in op for |meta| > 1
			LinkRef tail = cmp.NewLink ();
			if (count > 1)
				cmp.EmitIn (tail);

			// emit character/range/sets from meta-collection
			// we emit these first so that any 'ignore' flags will be noticed by the evaluator
			foreach (Interval a in meta) {
				if (a.IsDiscontiguous) {			// Set
					BitArray bits = new BitArray (a.Size);
					foreach (Interval b in intervals) {
						if (a.Contains (b)) {
							for (int i = b.low; i <= b.high; ++ i)
								bits[i - a.low] = true;
						}
					}

					cmp.EmitSet ((char)a.low, bits, negate, ignore, reverse);
				}
				else if (a.IsSingleton)				// Character
					cmp.EmitCharacter ((char)a.low, negate, ignore, reverse);
				else						// Range
					cmp.EmitRange ((char)a.low, (char)a.high, negate, ignore, reverse);
			}

			// emit categories
			for (int i = 0; i < pos_cats.Length; ++ i) {
				if (pos_cats[i]) {
					if (neg_cats [i])
						cmp.EmitCategory (Category.AnySingleline, negate, reverse);
					else
						cmp.EmitCategory ((Category)i, negate, reverse);
				} else if (neg_cats[i]) {
					cmp.EmitNotCategory ((Category)i, negate, reverse);
				}
			}

			// finish up
			if (count > 1) {
				if (negate)
					cmp.EmitTrue ();
				else
					cmp.EmitFalse ();

				cmp.ResolveLink (tail);
			}
		}

		public override void GetWidth (out int min, out int max) {
			min = max = 1;
		}

		public override bool IsComplex () {
			return false;
		}

		// private

		private static double GetIntervalCost (Interval i) {
			// use op length as cost metric (=> optimize for space)

			if (i.IsDiscontiguous)
				return 3 + ((i.Size + 0xf) >> 4);		// Set
			else if (i.IsSingleton)
				return 2;					// Character
			else
				return 3;					// Range
		}

		private static Interval upper_case_characters = new Interval ((char)65, (char)90);
		private const int distance_between_upper_and_lower_case = 32;
		private bool negate, ignore;
		private BitArray pos_cats, neg_cats;
		private IntervalCollection intervals;
	}

	class AnchorInfo {
		private Expression expr;

		private Position pos;
		private int offset;

		private string str;
		private int width;
		private bool ignore;

		public AnchorInfo (Expression expr, int width) {
			this.expr = expr;
			this.offset = 0;
			this.width = width;

			this.str = null;
			this.ignore = false;
			this.pos = Position.Any;
		}

		public AnchorInfo (Expression expr, int offset, int width, string str, bool ignore) {
			this.expr = expr;
			this.offset = offset;
			this.width = width;

			this.str = ignore ? str.ToLower () : str;

			this.ignore = ignore;
			this.pos = Position.Any;
		}

		public AnchorInfo (Expression expr, int offset, int width, Position pos) {
			this.expr = expr;
			this.offset = offset;
			this.width = width;

			this.pos = pos;

			this.str = null;
			this.ignore = false;
		}

		public Expression Expression {
			get { return expr; }
		}

		public int Offset {
			get { return offset; }
		}

		public int Width {
			get { return width; }
		}

		public int Length {
			get { return (str != null) ? str.Length : 0; }
		}

		public bool IsUnknownWidth {
			get { return width < 0; }
		}

		public bool IsComplete {
			get { return Length == Width; }
		}

		public string Substring {
			get { return str; }
		}

		public bool IgnoreCase {
			get { return ignore; }
		}

		public Position Position {
			get { return pos; }
		}

		public bool IsSubstring {
			get { return str != null; }
		}

		public bool IsPosition {
			get { return pos != Position.Any; }
		}

		public Interval GetInterval () {
			return GetInterval (0);
		}

		public Interval GetInterval (int start) {
			if (!IsSubstring)
				return Interval.Empty;

			return new Interval (start + Offset, start + Offset + Length - 1);
		}
	}
}
