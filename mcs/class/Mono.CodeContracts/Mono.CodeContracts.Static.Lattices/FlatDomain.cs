// 
// FlatDomain.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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
using System.IO;

namespace Mono.CodeContracts.Static.Lattices {
	/// <summary>
	/// Represents domain, where abstract values are disjunct, and their join/meet turns into Top/Bottom respectively
	/// </summary>
	/// <example>
	///       Top
	///      / | \
	///    v1 v2 v3
	///      \ | /
	///       Bot
	/// </example>
	/// <typeparam name="T"></typeparam>
	struct FlatDomain<T> : IAbstractDomain<FlatDomain<T>>, IEquatable<FlatDomain<T>>
		where T : IEquatable<T> {
		public static readonly FlatDomain<T> BottomValue = new FlatDomain<T> (Kind.Bottom);
		public static readonly FlatDomain<T> TopValue = new FlatDomain<T> (Kind.Top);

		public readonly T Concrete;
		private readonly Kind _kind;

		private FlatDomain (Kind kind)
		{
			this._kind = kind;
			this.Concrete = default(T);
		}

		public FlatDomain (T value)
		{
			this._kind = Kind.Concrete;
			this.Concrete = value;
		}

		public static implicit operator FlatDomain<T> (T value)
		{
			return new FlatDomain<T> (value);
		}

		#region Implementation of IAbstractDomain<FlatDomain<T>>
		public bool IsNormal
		{
			get { return this._kind == Kind.Concrete; }
		}

		public FlatDomain<T> Top
		{
			get { return TopValue; }
		}

		public FlatDomain<T> Bottom
		{
			get { return BottomValue; }
		}

		public bool IsTop
		{
			get { return this._kind == Kind.Top; }
		}

		public bool IsBottom
		{
			get { return this._kind == Kind.Bottom; }
		}

		public FlatDomain<T> Join (FlatDomain<T> that, bool widening, out bool weaker)
		{
			if (IsTop || that.IsBottom) {
				weaker = false;
				return this;
			}
			if (that.IsTop) {
				weaker = !IsTop;
				return that;
			}
			if (IsBottom) {
				weaker = !that.IsBottom;
				return that;
			}
			if (this.Concrete.Equals (that.Concrete)) {
				weaker = false;
				return that;
			}

			weaker = true;
			return TopValue;
		}

		public FlatDomain<T> Meet (FlatDomain<T> that)
		{
			if (IsTop || that.IsBottom)
				return that;
			if (that.IsTop || IsBottom)
				return this;
			if (this.Concrete.Equals (that.Concrete))
				return that;

			return BottomValue;
		}

		public bool LessEqual (FlatDomain<T> that)
		{
			if (that.IsTop || IsBottom)
				return true;
			if (IsTop || that.IsBottom)
				return false;

			//less equal means equality of values
			return this.Concrete.Equals (that.Concrete);
		}

		public FlatDomain<T> ImmutableVersion ()
		{
			return this;
		}

		public FlatDomain<T> Clone ()
		{
			return this;
		}

		public void Dump (TextWriter tw)
		{
			if (IsTop)
				tw.WriteLine ("Top");
			else if (IsBottom)
				tw.WriteLine ("Bot");
			else
				tw.WriteLine ("<{0}>", this.Concrete);
		}

		public override string ToString ()
		{
			if (IsTop)
				return "Top";
			if (IsBottom)
				return "Bot";

			return string.Format ("<{0}>", this.Concrete);
		}
		#endregion

		#region Implementation of IEquatable<FlatDomain<T>>
		public bool Equals (FlatDomain<T> that)
		{
			if (!this.IsNormal)
				return this._kind == that._kind;

			if (!that.IsNormal)
				return false;

			if (this.Concrete.Equals (that.Concrete))
				return true;

			return false;
		}
		#endregion

		#region Nested type: Kind
		private enum Kind : byte {
			Top = 0,
			Bottom,
			Concrete
		}
		#endregion
	}
}
