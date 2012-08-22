using System;
using Mono.CodeContracts.Inference.Interface;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.Proving;
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.Inference
{
	public class OverflowVisitor : OverflowVisitorBase
	{
		public override void Unary (UnaryOperator unaryOperator, BoxedExpression argument, BoxedExpression parent)
		{
			argument.Dispatch ((IBoxedExpressionController)this);
		}
		
		public override void Binary (BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression parent)
		{
			left.Dispatch ((IBoxedExpressionController)this);
			if (this.CanOverflow)
				return;
			right.Dispatch ((IBoxedExpressionController)this);
			if (this.CanOverflow)
				return;
			switch (binaryOperator) {
			case BinaryOperator.Add:
			case BinaryOperator.Add_Ovf:
			case BinaryOperator.Add_Ovf_Un:
				int sign1;
				int sign2;
				if (IFactQueryExtensions.TrySign<BoxedExpression, Variable> (
					this.facts,
					this.pc,
					left,
					out sign1
				) && IFactQueryExtensions.TrySign<BoxedExpression, Variable> (
					this.facts,
					this.pc,
					right,
					out sign2
				) && (sign1 <= 0 && sign2 <= 0 || sign1 * sign2 == -1)) {
					this.Overflow = false;
					break;
				} else {
					this.Overflow = true;
					break;
				}
			case BinaryOperator.And:
			case BinaryOperator.Ceq:
			case BinaryOperator.Cobjeq:
			case BinaryOperator.Cne_Un:
			case BinaryOperator.Cge:
			case BinaryOperator.Cge_Un:
			case BinaryOperator.Cgt:
			case BinaryOperator.Cgt_Un:
			case BinaryOperator.Cle:
			case BinaryOperator.Cle_Un:
			case BinaryOperator.Clt:
			case BinaryOperator.Clt_Un:
			case BinaryOperator.LogicalAnd:
			case BinaryOperator.LogicalOr:
			case BinaryOperator.Or:
			case BinaryOperator.Shl:
			case BinaryOperator.Shr:
			case BinaryOperator.Shr_Un:
			case BinaryOperator.Xor:
				this.Overflow = false;
				break;
			case BinaryOperator.Div:
			case BinaryOperator.Div_Un:
			case BinaryOperator.Rem:
			case BinaryOperator.Rem_Un:
				if (this.Facts.IsNonZero (this.pc, right) == ProofOutcome.True)
					this.Overflow = false;
				this.Overflow = true;
				break;
			case BinaryOperator.Mul:
			case BinaryOperator.Mul_Ovf:
			case BinaryOperator.Mul_Ovf_Un:
				this.Overflow = true;
				break;
			case BinaryOperator.Sub:
			case BinaryOperator.Sub_Ovf:
			case BinaryOperator.Sub_Ovf_Un:
				int sign3;
				if (IFactQueryExtensions.TrySign<BoxedExpression, Variable> (
					this.facts,
					this.pc,
					right,
					out sign3
				) && sign3 >= 0) {
					this.Overflow = false;
					break;
				} else {
					this.Overflow = true;
					break;
				}
			default:
				this.Overflow = true;
				break;
			}
		}
		
		public bool CanOverflow 
		{
			get {return this.Overflow;}
		}
		
		public OverflowVisitor (APC pc, IFactQuery<BoxedExpression, Variable> facts): base(pc, facts)
		{
		}	
	}
}

