// 
// EqualityPair.cs
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
	struct EqualityPair<TFunc, TAbstractDomain> : IEquatable<EqualityPair<TFunc, TAbstractDomain>> 
		where TFunc : IEquatable<TFunc>, IConstantInfo 
		where TAbstractDomain : IAbstractDomainForEGraph<TAbstractDomain>, IEquatable<TAbstractDomain> {

		public readonly SymValue Sv1;
		public readonly SymValue Sv2;

		public EqualityPair (SymValue v1, SymValue v2)
		{
			this.Sv1 = v1;
			this.Sv2 = v2;
		}

		#region Implementation of IEquatable<SymGraph<Constant,AbstractValue>.EqualityPair>
		public bool Equals (EqualityPair<TFunc, TAbstractDomain> other)
		{
			return (this.Sv1 == other.Sv1 && this.Sv2 == other.Sv2);
		}
		#endregion

		public override bool Equals (object obj)
		{
			if (obj is EqualityPair<TFunc, TAbstractDomain>)
				return Equals ((EqualityPair<TFunc, TAbstractDomain>) obj);
			return false;
		}

		public override int GetHashCode ()
		{
			return (this.Sv1 == null ? 1 : this.Sv1.GlobalId) + this.Sv2.GlobalId;
		}
	}
}