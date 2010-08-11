using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Mono.CodeContracts.Rewrite.Ast {
	class ExprBlock : Expr {

		public ExprBlock (MethodInfo methodInfo, IEnumerable<Expr> exprs)
			: base (methodInfo)
		{
			this.Exprs = exprs;
		}

		public override ExprType ExprType
		{
			get { return ExprType.Block; }
		}

		public override TypeReference ReturnType
		{
			get { return base.MethodInfo.TypeVoid; }
		}

		public IEnumerable<Expr> Exprs { get; private set; }

	}
}
