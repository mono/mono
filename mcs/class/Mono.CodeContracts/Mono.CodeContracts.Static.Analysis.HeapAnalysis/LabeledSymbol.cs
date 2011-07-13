// 
// LabeledSymbol.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
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

using System;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	struct LabeledSymbol<Label, TSymValue> : IEquatable<LabeledSymbol<Label, TSymValue>>
		where TSymValue : IEquatable<TSymValue> {
		public readonly Label ReadAt;
		public readonly TSymValue Symbol;

		public LabeledSymbol (Label readAt, TSymValue symbol)
		{
			this.ReadAt = readAt;
			this.Symbol = symbol;
		}

		#region Implementation of IEquatable<ExternalExpression<Label,SymbolicValue>>
		public bool Equals (LabeledSymbol<Label, TSymValue> other)
		{
			return this.Symbol.Equals (other.Symbol);
		}
		#endregion

		public override bool Equals (object obj)
		{
			return obj is LabeledSymbol<Label, TSymValue> && Equals ((LabeledSymbol<Label, TSymValue>) obj);
		}

		public override int GetHashCode ()
		{
			return this.Symbol.GetHashCode ();
		}

		public override string ToString ()
		{
			return string.Format ("{0}@{1}", this.Symbol, this.ReadAt);
		}
	}
}
