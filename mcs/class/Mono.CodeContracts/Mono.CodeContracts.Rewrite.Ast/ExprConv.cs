using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Mono.CodeContracts.Rewrite.Ast {
	class ExprConv : Expr {

		public ExprConv (MethodInfo methodInfo, Expr exprToConvert, TypeCode convToType)
			: base (methodInfo)
		{
			this.ExprToConvert = exprToConvert;
			this.ConvToType = convToType;
		}

		public override ExprType ExprType
		{
			get { return ExprType.Conv; }
		}

		public Expr ExprToConvert { get; private set; }
		public TypeCode ConvToType { get; private set; }

		public override TypeReference ReturnType
		{
			get
			{
				switch (this.ConvToType) {
				case TypeCode.Int64:
					return base.MethodInfo.TypeInt64;
				default:
					throw new NotSupportedException ("Cannot conv to: " + this.ConvToType);
				}
			}
		}

	}
}
