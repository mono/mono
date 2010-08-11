using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.CodeContracts.Rewrite.Ast {
	class ExprNop : Expr {

		public ExprNop (MethodInfo methodInfo)
			: base (methodInfo)
		{
		}

		public override ExprType ExprType
		{
			get { return ExprType.Nop; }
		}

		public override Mono.Cecil.TypeReference ReturnType
		{
			get { return base.MethodInfo.TypeVoid; }
		}

	}
}
