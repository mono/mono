using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Mono.CodeContracts.Rewrite.Ast {
	class ExprLoadArg : Expr {

		public ExprLoadArg (MethodInfo methodInfo, int index)
			: base (methodInfo)
		{
			this.Index = index;
		}

		public int Index { get; private set; }

		public override ExprType ExprType
		{
			get { return ExprType.LoadArg; }
		}

		public override TypeReference ReturnType
		{
			get
			{
				return base.MethodInfo.Method.Parameters [this.Index].ParameterType;
			}
		}

	}
}
