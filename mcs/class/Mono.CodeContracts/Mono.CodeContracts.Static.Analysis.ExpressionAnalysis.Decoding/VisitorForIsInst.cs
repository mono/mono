// 
// VisitorForIsInst.cs
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
	class VisitorForIsInst<V, E> : QueryVisitor<V, E> 
		where V : IEquatable<V> 
		where E : IEquatable<E> {
		
		private E argument;
		private TypeNode type;

		public static bool IsIsInst (E expr, out TypeNode type, out E arg, FullExpressionDecoder<V,E> decoder)
		{
			VisitorForIsInst<V, E> v = decoder.IsInstVisitor;
			bool res = Decode (expr, v, decoder);

			arg = v.argument;
			type = v.type;
			return res;
		}

		public override bool Isinst (E pc, TypeNode type, V dest, E obj, Dummy data)
		{
			this.type = type;
			this.argument = obj;
			return true;
		}
	}
}