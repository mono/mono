using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.CodeContracts.Rewrite.Ast {
	class ExprCompareLessThan : ExprBinaryOpComparison {

		public ExprCompareLessThan (MethodInfo methodInfo, Expr left, Expr right, Sn signage)
			: base (methodInfo, left, right, signage)
		{
		}

		public override ExprType ExprType
		{
			get { return ExprType.CompareLessThan; }
		}

	}
}
