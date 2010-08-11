using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Mono.CodeContracts.Rewrite.Ast {
	class ExprLoadConstant : Expr {

		public ExprLoadConstant (MethodInfo methodInfo, object value)
			: base (methodInfo)
		{
			this.Value = value;

			if (value == null) {
				this.returnType = methodInfo.TypeObject;
			} else {
				Type type = value.GetType();
				this.returnType = methodInfo.Module.Import (type);
			}
		}

		private TypeReference returnType;

		public object Value { get; private set; }

		public override ExprType ExprType
		{
			get { return ExprType.LoadConstant; }
		}

		public override TypeReference ReturnType
		{
			get { return this.returnType; }
		}

	}
}
