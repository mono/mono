// 
// SymbolicValue.cs
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

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	struct SymbolicValue : IEquatable<SymbolicValue>, IComparable<SymbolicValue>, IComparable {
		public readonly SymValue Symbol;

		public SymbolicValue (SymValue symbol)
		{
			this.Symbol = symbol;
		}

		public bool IsNull
		{
			get { return this.Symbol == null; }
		}

		public int MethodLocalId
		{
			get { return this.Symbol.UniqueId; }
		}

		#region Implementation of IEquatable<SymbolicValue>
		public bool Equals (SymbolicValue other)
		{
			return this.Symbol == other.Symbol;
		}
		#endregion

		#region Implementation of IComparable<in SymbolicValue>
		public int CompareTo (SymbolicValue other)
		{
			return this.Symbol.CompareTo (other.Symbol);
		}
		#endregion

		#region Implementation of IComparable
		public int CompareTo (object obj)
		{
			if (!(obj is SymbolicValue))
				return 1;

			return CompareTo ((SymbolicValue) obj);
		}
		#endregion

		public override bool Equals (object obj)
		{
			if (obj is SymbolicValue)
				return Equals ((SymbolicValue) obj);

			return false;
		}

		public override int GetHashCode ()
		{
			return this.Symbol == null ? 0 : this.Symbol.GetHashCode ();
		}

		public override string ToString ()
		{
			if (this.Symbol == null)
				return "<!null!>";
			return this.Symbol.ToString ();
		}

		public static int GetUniqueKey (SymbolicValue arg)
		{
			return SymValue.GetUniqueKey (arg.Symbol);
		}
	}
}
