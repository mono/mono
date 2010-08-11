using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.CodeContracts.Rewrite.Ast;

namespace Mono.CodeContracts.Rewrite {
	class ContractRequiresInfo {

		public ContractRequiresInfo (Expr originalExpr, Expr rewrittenExpr)
		{
			this.OriginalExpr = originalExpr;
			this.RewrittenExpr = rewrittenExpr;
		}

		public Expr OriginalExpr { get; private set; }
		public Expr RewrittenExpr { get; private set; }

	}
}
