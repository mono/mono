using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.Inference.Interface;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public class AssumeExpression : ContractExpression
		{
			public AssumeExpression (BoxedExpression condition, EdgeTag tag, APC pc)
				: base (condition, tag, pc)
			{
			}

			#region Overrides of ContractExpression
			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.Assume (pc, this.Tag, Dummy.Value, data);
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression cond = this.Condition.Substitute (map);
				if (cond == this.Condition)
					return this;
				if (cond == null)
					return null;

				return new AssumeExpression (cond, this.Tag, this.Apc);
			}
			#endregion
		}
}

