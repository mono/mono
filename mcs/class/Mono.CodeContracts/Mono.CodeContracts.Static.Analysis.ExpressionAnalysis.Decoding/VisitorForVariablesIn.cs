// 
// VisitorForVariablesIn.cs
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
// 

using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding {
	class VisitorForVariablesIn<V, E> : ISymbolicExpressionVisitor<E, E, V, ISet<E>, Dummy>
		where V : IEquatable<V>
		where E : IEquatable<E> {
		private readonly IExpressionContextProvider<E, V> contextProvider;

		public VisitorForVariablesIn (IExpressionContextProvider<E, V> contextProvider)
		{
			this.contextProvider = contextProvider;
		}

		public static void AddFreeVariables (E expr, ISet<E> set, FullExpressionDecoder<V, E> decoder)
		{
			decoder.VariablesInVisitor.Recurse (expr, set);
		}

		private void Recurse (E expr, ISet<E> set)
		{
			this.contextProvider.ExpressionContext.Decode<ISet<E>, Dummy, VisitorForVariablesIn<V, E>> (expr, this, set);
		}

		#region Implementation of IExpressionILVisitor<E,E,V,ISet<E>,Dummy>
		public Dummy Binary (E pc, BinaryOperator op, V dest, E operand1, E operand2, ISet<E> data)
		{
			Recurse (operand1, data);
			Recurse (operand2, data);
			return Dummy.Value;
		}

		public Dummy Isinst (E pc, TypeNode type, V dest, E obj, ISet<E> data)
		{
			data.Add (pc);
			return Dummy.Value;
		}

		public Dummy LoadNull (E pc, V dest, ISet<E> polarity)
		{
			return Dummy.Value;
		}

		public Dummy LoadConst (E pc, TypeNode type, object constant, V dest, ISet<E> data)
		{
			return Dummy.Value;
		}

		public Dummy Sizeof (E pc, TypeNode type, V dest, ISet<E> data)
		{
			data.Add (pc);
			return Dummy.Value;
		}

		public Dummy Unary (E pc, UnaryOperator op, bool unsigned, V dest, E source, ISet<E> data)
		{
			Recurse (source, data);
			return Dummy.Value;
		}
		#endregion

		#region Implementation of ISymbolicExpressionVisitor<E,E,V,ISet<E>,Dummy>
		public Dummy SymbolicConstant (E pc, V variable, ISet<E> data)
		{
			return Dummy.Value;
		}
		#endregion
	}
}
