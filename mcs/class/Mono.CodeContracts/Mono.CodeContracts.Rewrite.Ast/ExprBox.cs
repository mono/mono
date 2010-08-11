using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.CodeContracts.Rewrite.Ast {
	class ExprBox : Expr {

		public ExprBox (MethodInfo methodInfo, Expr exprToBox)
			: base (methodInfo)
		{
			this.ExprToBox = exprToBox;
		}

		public override ExprType ExprType
		{
			get { return ExprType.Box; }
		}

		public override Mono.Cecil.TypeReference ReturnType
		{
			get { return this.ExprToBox.ReturnType; }
		}

		public Expr ExprToBox { get; private set; }

	}
}
