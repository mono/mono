// 
// ValueContextProvider.cs
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
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis.Paths;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	class ValueContextProvider<TContext> : IValueContextProvider<SymbolicValue>, IValueContext<SymbolicValue>
		where TContext : IStackContextProvider {
		private readonly HeapAnalysis parent;
		private readonly TContext underlying;

		public ValueContextProvider (HeapAnalysis parent, TContext underlying)
		{
			this.parent = parent;
			this.underlying = underlying;
		}

		#region Implementation of IMethodContextProvider
		public IMethodContext MethodContext
		{
			get { return this.underlying.MethodContext; }
		}
		#endregion

		#region Implementation of IStackContextProvider
		public IStackContext StackContext
		{
			get { return this.underlying.StackContext; }
		}
		#endregion

		#region Implementation of IValueContextProvider<SymbolicValue>
		IValueContext<SymbolicValue> IValueContextProvider<SymbolicValue>.ValueContext
		{
			get { return this; }
		}
		#endregion

		#region Implementation of IValueContext<SymbolicValue>
		public FlatDomain<TypeNode> GetType (APC pc, SymbolicValue value)
		{
			Domain domain;
			if (!this.parent.PreStateLookup (pc, out domain) || domain.IsBottom)
				return FlatDomain<TypeNode>.TopValue;

			return domain.GetType (value.Symbol).Type;
		}

		public bool IsZero (APC at, SymbolicValue value)
		{
			Domain domain;
			if (!this.parent.PreStateLookup (at, out domain))
				return true;

			return domain.IsZero (value.Symbol);
		}

		public bool TryLocalValue (APC at, Local local, out SymbolicValue sv)
		{
			Domain domain;
			if (this.parent.PreStateLookup (at, out domain))
				return domain.TryGetCorrespondingValueAbstraction (local, out sv);

			sv = new SymbolicValue ();
			return false;
		}

		public bool TryParameterValue (APC at, Parameter p, out SymbolicValue sv)
		{
			Domain domain;
			if (this.parent.PreStateLookup (at, out domain))
				return domain.TryGetCorrespondingValueAbstraction (p, out sv);

			sv = new SymbolicValue ();
			return false;
		}

		public bool TryGetArrayLength (APC at, SymbolicValue array, out SymbolicValue length)
		{
			Domain domain;
			if (!this.parent.PreStateLookup (at, out domain))
				throw new ArgumentException ("pc wasn't visited");
			SymValue lengthValue;
			bool arrayLength = domain.TryGetArrayLength (array.Symbol, out lengthValue);

			length = new SymbolicValue (lengthValue);
			return arrayLength;
		}

		public Sequence<PathElement> AccessPathList (APC at, SymbolicValue sv, bool allowLocal, bool preferLocal)
		{
			Domain domain;
			if (!this.parent.PreStateLookup (at, out domain))
				throw new ArgumentException ("pc wasn't visited");

			AccessPathFilter<Method> filter = AccessPathFilter<Method>.IsVisibleFrom (MethodContext.CurrentMethod);
			return domain.GetAccessPathList (sv.Symbol, filter, allowLocal, preferLocal);
		}

		public bool IsConstant (APC at, SymbolicValue symbol, out TypeNode type, out object constant)
		{
			Domain domain;
			if (!this.parent.PreStateLookup (at, out domain))
				throw new ArgumentException ("pc wasn't visited");

			return domain.IsConstant (symbol.Symbol, out type, out constant);
		}

		public bool TryStackValue (APC at, int stackIndex, out SymbolicValue sv)
		{
			Domain domain;
			if (this.parent.PreStateLookup (at, out domain))
				return domain.TryGetCorrespondingValueAbstraction (stackIndex, out sv);

			sv = new SymbolicValue ();
			return false;
		}

		public bool IsValid (SymbolicValue sv)
		{
			return sv.Symbol != null;
		}

		public string AccessPath (APC at, SymbolicValue sv)
		{
			Domain domain;
			if (!this.parent.PreStateLookup (at, out domain))
				throw new ArgumentException ("pc wasn't visited");

			return domain.GetAccessPath (sv.Symbol);
		}

		public IEnumerable<Sequence<PathElement>> AccessPaths (APC at, SymValue value, AccessPathFilter<Method> filter)
		{
			Domain domain;
			if (!this.parent.PreStateLookup (at, out domain))
				throw new ArgumentException ("pc wasn't visited");

			return domain.GetAccessPathsFiltered (value, filter, true).Select (path => path.Coerce<PathElementBase, PathElement> ());
		}

		public Sequence<PathElement> VisibleAccessPathList (APC at, SymbolicValue value)
		{
			Domain domain;
			if (!this.parent.PreStateLookup (at, out domain))
				throw new ArgumentException ("pc wasn't visited");

			AccessPathFilter<Method> filter = AccessPathFilter<Method>.FromPrecondition (MethodContext.CurrentMethod);
			return domain.GetAccessPathList (value.Symbol, filter, false, false);
		}
		#endregion
	}
}
