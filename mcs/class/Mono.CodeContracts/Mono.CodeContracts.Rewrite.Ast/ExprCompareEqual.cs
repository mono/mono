using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.CodeContracts.Rewrite.Ast {
	class ExprCompareEqual : ExprBinaryOpComparison {

		public ExprCompareEqual (MethodInfo methodInfo, Expr left, Expr right)
			: base (methodInfo, left, right, Sn.None)
		{
		}

		public override ExprType ExprType
		{
			get { return ExprType.CompareEqual; }
		}

	}
}
