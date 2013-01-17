// 
// VisitorForIsBinaryExpression.cs
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
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding {
	class VisitorForIsBinaryExpression<V, E> : QueryVisitor<V, E> 
		where V : IEquatable<V> where E : IEquatable<E> {
		
		private BinaryOperator op;
		private E left;
		private E right;

		public static bool IsBinary (E expr, out BinaryOperator bop, out E left, out E right, FullExpressionDecoder<V,E> decoder)
		{
			VisitorForIsBinaryExpression<V, E> v = decoder.BinaryExpressionVisitor;
			bool res = Decode (expr, v, decoder);

			bop = v.op;
			left = v.left;
			right = v.right;
			return res;
		}

		public override bool Binary (E pc, BinaryOperator op, V dest, E operand1, E operand2, Dummy data)
		{
			this.op = op;
			this.left = operand1;
			this.right = operand2;

			return true;
		}
	}
}