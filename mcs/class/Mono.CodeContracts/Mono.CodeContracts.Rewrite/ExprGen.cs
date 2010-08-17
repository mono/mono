//
// ExprGen.cs
//
// Authors:
//	Chris Bacon (chrisbacon76@gmail.com)
//
// Copyright (C) 2010 Chris Bacon
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
using System.Linq;
using System.Text;
using Mono.CodeContracts.Rewrite.Ast;
using Mono.Cecil;

namespace Mono.CodeContracts.Rewrite {
	class ExprGen {

		public ExprGen (MethodInfo methodInfo)
		{
			this.methodInfo = methodInfo;
		}

		private MethodInfo methodInfo;

		public ExprBlock Block (IEnumerable<Expr> exprs)
		{
			return new ExprBlock (this.methodInfo, exprs);
		}

		public ExprReturn Return ()
		{
			return new ExprReturn (this.methodInfo);
		}

		public ExprBox Box (Expr exprToBox)
		{
			return new ExprBox (this.methodInfo, exprToBox);
		}

		public ExprNop Nop ()
		{
			return new ExprNop (this.methodInfo);
		}

		public ExprLoadArg LoadArg (int index)
		{
			return new ExprLoadArg (this.methodInfo, index);
		}

		public ExprLoadArg LoadArg (ParameterDefinition parameterDefinition)
		{
			return this.LoadArg (parameterDefinition.Sequence);
		}

		public ExprLoadConstant LoadConstant (object value)
		{
			return new ExprLoadConstant (this.methodInfo, value);
		}

		public ExprCall Call (MethodReference method, IEnumerable<Expr> parameters)
		{
			return new ExprCall (this.methodInfo, method, parameters);
		}

		public ExprCompareEqual CompareEqual (Expr left, Expr right)
		{
			return new ExprCompareEqual (this.methodInfo, left, right);
		}

		public ExprCompareLessThan CompareLessThan (Expr left, Expr right, Sn signage)
		{
			return new ExprCompareLessThan (this.methodInfo, left, right, signage);
		}

		public ExprCompareGreaterThan CompareGreaterThan (Expr left, Expr right, Sn signage)
		{
			return new ExprCompareGreaterThan (this.methodInfo, left, right, signage);
		}

		public ExprConv Conv (Expr exprToConvert, TypeCode convToType)
		{
			return new ExprConv (this.methodInfo, exprToConvert, convToType);
		}

		public ExprAdd Add (Expr left, Expr right, Sn signage, bool overflow)
		{
			return new ExprAdd (this.methodInfo, left, right, signage, overflow);
		}

		public ExprSub Sub (Expr left, Expr right, Sn signage, bool overflow)
		{
			return new ExprSub (this.methodInfo, left, right, signage, overflow);
		}

	}
}
