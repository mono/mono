// 
// IFullExpressionDecoder.cs
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

using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis.Paths;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding {
	interface IFullExpressionDecoder<Variable, Expression> {
		bool IsVariable (Expression expr, out object variable);
		Variable UnderlyingVariable (Expression expr);
		bool IsNull (Expression expr);
		bool IsConstant (Expression expr, out object value, out TypeNode type);
		bool IsSizeof (Expression expr, out TypeNode type);
		bool IsIsinst (Expression expr, out Expression arg, out TypeNode type);
		bool IsUnaryExpression (Expression expr, out UnaryOperator op, out Expression arg);
		bool IsBinaryExpression (Expression expr, out BinaryOperator op, out Expression left, out Expression right);
		void AddFreeVariables (Expression expr, ISet<Expression> set);
		LispList<PathElement> GetVariableAccessPath (Expression expr);
		bool TryGetType (Expression expr, out TypeNode type);
		bool TrySizeOfAsConstant (Expression expr, out int sizeAsConstant);
	}
}
