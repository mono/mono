// 
// MultiEdge.cs
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

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.SymbolicGraph {
	struct MultiEdge<TFunc, TAbstractDomain> : IEquatable<MultiEdge<TFunc, TAbstractDomain>> 
		where TFunc : IEquatable<TFunc>, IConstantInfo 
		where TAbstractDomain : IAbstractDomainForEGraph<TAbstractDomain>, IEquatable<TAbstractDomain> {
		
		public readonly int Arity;
		public readonly int Index;
		public readonly TFunc Function;

		public MultiEdge (TFunc function, int index, int arity)
		{
			this.Function = function;
			this.Index = index;
			this.Arity = arity;
		}

		#region Implementation of IEquatable<MultiEdge>
		public bool Equals (MultiEdge<TFunc, TAbstractDomain> other)
		{
			return (this.Index == other.Index && this.Arity == other.Arity && this.Function.Equals (other.Function));
		}
		#endregion

		public override bool Equals (object obj)
		{
			if (obj is MultiEdge<TFunc, TAbstractDomain>)
				return Equals ((MultiEdge<TFunc, TAbstractDomain>) obj);
			return false;
		}

		public override int GetHashCode ()
		{
			return this.Arity*13 + this.Index;
		}

		public override string ToString ()
		{
			return String.Format ("[{0}:{1}]", this.Function, this.Index);
		}
	}
}