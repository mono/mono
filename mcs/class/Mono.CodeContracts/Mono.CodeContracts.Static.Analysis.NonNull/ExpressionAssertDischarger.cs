// 
// ExpressionAssertDischarger.cs
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
using Mono.CodeContracts.Static.ControlFlow;

namespace Mono.CodeContracts.Static.Analysis.NonNull {
	struct ExpressionAssertDischarger<E, V> 
		: ISymbolicExpressionVisitor<E, E, V, bool, ProofOutcome> 
		where E : IEquatable<E> 
		where V : IEquatable<V> {
		private readonly Analysis<E, V> analysis;
		private readonly APC pc;

		public ExpressionAssertDischarger(Analysis<E, V> analysis, APC pc)
		{
			this.analysis = analysis;
			this.pc = pc;
		}

		private IExpressionContextProvider<E, V> ContextProvider
		{
			get { return this.analysis.ContextProvider; }
		} 

		#region Implementation of IExpressionILVisitor<Expression,Expression,Variable,bool,ProofOutcome>
		private ProofOutcome Recurse(bool polarity, E expr)
		{
			return this.ContextProvider.ExpressionContext.Decode<bool, ProofOutcome, ExpressionAssertDischarger<E, V>> (expr, this, polarity);
		}

		public ProofOutcome Binary(E orig, BinaryOperator op, V dest, E operand1, E operand2, bool polarity)
		{
			switch (op) {
			case BinaryOperator.Ceq:
			case BinaryOperator.Cobjeq:
				if (this.ContextProvider.ExpressionContext.IsZero (operand2) || this.ContextProvider.ExpressionContext.IsZero (operand1))
					return this.Recurse (!polarity, operand1);
				return ProofOutcome.Top;
			case BinaryOperator.Cne_Un:
				if (this.ContextProvider.ExpressionContext.IsZero(operand2) || this.ContextProvider.ExpressionContext.IsZero(operand1))
					return this.Recurse (polarity, operand1);
				return ProofOutcome.Top;
			default:
				return this.SymbolicConstant (orig, this.ContextProvider.ExpressionContext.Unrefine (orig), polarity);
			}
		}

		public ProofOutcome Isinst(E orig, TypeNode type, V dest, E obj, bool polarity)
		{
			if (!polarity)
				return this.analysis.IsNull (this.pc, dest);
			ProofOutcome outcome = this.analysis.IsNonNull (this.pc, dest);

			return outcome != ProofOutcome.True ? outcome : this.Recurse (true, obj);
		}

		public ProofOutcome LoadNull(E orig, V dest, bool polarity)
		{
			return polarity ? ProofOutcome.False : ProofOutcome.True;
		}

		public ProofOutcome LoadConst(E orig, TypeNode type, object constant, V dest, bool polarity)
		{
			var isConstantEqualZero = constant is int && (int) constant == 0;
			
			return (isConstantEqualZero != polarity) ? ProofOutcome.True : ProofOutcome.False;
		}

		public ProofOutcome Sizeof(E pc, TypeNode type, V dest, bool polarity)
		{
			return polarity ? ProofOutcome.True : ProofOutcome.False;
		}

		public ProofOutcome Unary(E orig, UnaryOperator op, bool unsigned, V dest, E source, bool polarity)
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
				return this.Recurse (polarity, source);
			case UnaryOperator.Neg:
				return this.Recurse(polarity, source);
			case UnaryOperator.Not:
				return this.Recurse(!polarity, source);
			default:
				return this.SymbolicConstant (orig, this.ContextProvider.ExpressionContext.Unrefine (orig), polarity);
			}
		}
		#endregion

		#region Implementation of ISymbolicExpressionVisitor<Expression,Expression,Variable,bool,ProofOutcome>
		public ProofOutcome SymbolicConstant(E pc, V variable, bool polarity)
		{
			return polarity ? this.analysis.IsNonNull (this.pc, variable) : this.analysis.IsNull(this.pc, variable);
		}
		#endregion
	}
}