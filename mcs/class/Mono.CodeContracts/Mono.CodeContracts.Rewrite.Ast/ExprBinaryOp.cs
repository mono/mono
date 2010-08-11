using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.CodeContracts.Rewrite.Ast {
	abstract class ExprBinaryOp : Expr {

		public ExprBinaryOp (MethodInfo methodInfo, Expr left, Expr right, Sn signage)
			: base (methodInfo)
		{
			this.Left = left;
			this.Right = right;
			this.Signage = signage;
		}

		public Expr Left { get; private set; }
		public Expr Right { get; private set; }
		public Sn Signage { get; private set; }

		public bool IsSigned
		{
			get { return this.Signage == Sn.Signed; }
		}

		public bool IsUnsigned
		{
			get { return this.Signage == Sn.Unsigned; }
		}

	}
}
