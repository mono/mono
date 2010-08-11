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
			return this.LoadArg (parameterDefinition.Index);
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
