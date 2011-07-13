// 
// SymGraphTerm.cs
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
using System.Linq;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.SymbolicGraph {
	struct SymGraphTerm<TFunc> : IEquatable<SymGraphTerm<TFunc>>
		where TFunc : IEquatable<TFunc> {
		public readonly SymValue[] Args;
		public readonly TFunc Function;

		public SymGraphTerm (TFunc function, params SymValue[] args)
		{
			this.Function = function;
			this.Args = args;
		}

		public bool Equals (SymGraphTerm<TFunc> that)
		{
			if (!this.Function.Equals (that.Function) || this.Args.Length != that.Args.Length)
				return false;

			for (int i = 0; i < this.Args.Length; i++) {
				if (!this.Args [i].Equals (that.Args [i]))
					return false;
			}
			return true;
		}

		public override string ToString ()
		{
			var args = this.Args == null ? "<no args>" : string.Join (", ", this.Args.Select (a => a.ToString ()));
			return string.Format ("Term({0}, {{{1}}})", this.Function, args);
		}
	}
}
