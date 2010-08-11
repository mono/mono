using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Mono.CodeContracts.Rewrite.Ast {
	abstract class ExprBinaryOpArithmetic : ExprBinaryOp {

		public ExprBinaryOpArithmetic (MethodInfo methodInfo, Expr left, Expr right, Sn signage, bool overflow)
			: base (methodInfo, left, right, signage)
		{
			this.Overflow = overflow;
		}

		public bool Overflow { get; private set; }

		public override TypeReference ReturnType
		{
			get {
				if (base.Left.ReturnType.FullName == "System.Int8" ||
					base.Left.ReturnType.FullName == "System.Int16") {
					return base.MethodInfo.TypeInt32;
				}
				if (base.Left.ReturnType.FullName == "System.Uint8" ||
					base.Left.ReturnType.FullName == "System.Uint16") {
					return base.MethodInfo.TypeUInt32;
				}
				return base.Left.ReturnType;
			}
		}

	}
}
