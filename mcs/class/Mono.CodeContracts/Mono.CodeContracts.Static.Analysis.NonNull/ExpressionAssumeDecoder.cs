// 
// ExpressionAssumeDecoder.cs
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
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.NonNull {
	struct ExpressionAssumeDecoder<E, V>
		: ISymbolicExpressionVisitor<E, E, V, Pair<bool, Domain<E, V>>, Domain<E, V>>
		where V : IEquatable<V>
		where E : IEquatable<E> {
		private readonly IExpressionContextProvider<E, V> context_provider;

		public ExpressionAssumeDecoder (IExpressionContextProvider<E, V> contextProvider)
		{
			this.context_provider = contextProvider;
		}

		#region ISymbolicExpressionVisitor<E,E,V,Pair<bool,Domain<E,V>>,Domain<E,V>> Members
		public Domain<E, V> Binary (E pc, BinaryOperator op, V dest, E operand1, E operand2, Pair<bool, Domain<E, V>> data)
		{
			IExpressionContext<E, V> exprCtx = this.context_provider.ExpressionContext;
			switch (op) {
			case BinaryOperator.Ceq:
			case BinaryOperator.Cobjeq:
				if (data.Value.IsNull (exprCtx.Unrefine (operand2)) || exprCtx.IsZero (operand2)
				    || data.Value.IsNull (exprCtx.Unrefine (operand1)) || exprCtx.IsZero (operand1))
					return Recurse (new Pair<bool, Domain<E, V>> (!data.Key, data.Value), operand1);
				if (data.Value.IsNonNull (exprCtx.Unrefine (operand1)) || data.Value.IsNonNull (exprCtx.Unrefine (operand2)))
					return Analysis<E, V>.AssumeNonNull (exprCtx.Unrefine (operand2), data.Value);
				return data.Value;
			case BinaryOperator.Cne_Un:
				if (data.Value.IsNull (exprCtx.Unrefine (operand2)) || exprCtx.IsZero (operand2)
				    || data.Value.IsNull (exprCtx.Unrefine (operand1)) || exprCtx.IsZero (operand1))
					return Recurse (data, operand1);

				return data.Value;
			default:
				return data.Value;
			}
		}

		public Domain<E, V> Isinst (E pc, TypeNode type, V dest, E obj, Pair<bool, Domain<E, V>> data)
		{
			if (data.Key)
				return Recurse (new Pair<bool, Domain<E, V>> (true, Analysis<E, V>.AssumeNonNull (dest, data.Value)), obj);
			return data.Value;
		}

		public Domain<E, V> LoadNull (E pc, V dest, Pair<bool, Domain<E, V>> data)
		{
			if (data.Key)
				return Domain<E, V>.BottomValue;
			return data.Value;
		}

		public Domain<E, V> LoadConst (E pc, TypeNode type, object constant, V dest, Pair<bool, Domain<E, V>> data)
		{
			if (constant is string)
				return data.Value;

			var convertible = constant as IConvertible;
			bool isZero = false;
			if (convertible != null) {
				try {
					isZero = convertible.ToInt32 (null) == 0;
				} catch {
					return data.Value;
				}
			}

			if (data.Key && isZero || !data.Key && !isZero)
				return Domain<E, V>.BottomValue;

			return data.Value;
		}

		public Domain<E, V> Sizeof (E pc, TypeNode type, V dest, Pair<bool, Domain<E, V>> data)
		{
			return data.Value;
		}

		public Domain<E, V> Unary (E pc, UnaryOperator op, bool unsigned, V dest, E source, Pair<bool, Domain<E, V>> data)
		{
			switch (op) {
			case UnaryOperator.Conv_i:
			case UnaryOperator.Conv_i1:
			case UnaryOperator.Conv_i2:
			case UnaryOperator.Conv_i4:
			case UnaryOperator.Conv_i8:
			case UnaryOperator.Conv_u:
			case UnaryOperator.Conv_u1:
			case UnaryOperator.Conv_u2:
			case UnaryOperator.Conv_u4:
			case UnaryOperator.Conv_u8:
				return Recurse (data, source);
			case UnaryOperator.Neg:
				return Recurse (data, source);
			case UnaryOperator.Not:
				return Recurse (new Pair<bool, Domain<E, V>> (!data.Key, data.Value), source);
			default:
				return data.Value;
			}
		}

		public Domain<E, V> SymbolicConstant (E orig, V variable, Pair<bool, Domain<E, V>> data)
		{
			if (data.Key) {
				return !this.context_provider.ExpressionContext.IsZero (orig)
				       	? Domain<E, V>.BottomValue
				       	: Analysis<E, V>.AssumeNonNull (variable, data.Value);
			}

			if (data.Value.NonNulls.Contains (variable))
				return Domain<E, V>.BottomValue;

			return Analysis<E, V>.AssumeNull (variable, data.Value);
		}
		#endregion

		private Domain<E, V> Recurse (Pair<bool, Domain<E, V>> pair, E expr)
		{
			return this.context_provider.ExpressionContext.Decode<Pair<bool, Domain<E, V>>, Domain<E, V>, ExpressionAssumeDecoder<E, V>> (expr, this, pair);
		}
		}
}
