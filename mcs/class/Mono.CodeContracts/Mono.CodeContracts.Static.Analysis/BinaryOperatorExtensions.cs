using System;

namespace Mono.CodeContracts.Static.Analysis
{
	public class BinaryOperatorExtensions
	{
		public static bool IsComparisonBinaryOperator (BinaryOperator binOp)
		{
			switch (binOp) {
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
				return true;
			default:
				return false;
			}
		}

		public static bool TryInvert (this BinaryOperator binOp, out BinaryOperator inverted)
		{
			switch (binOp) {
			case BinaryOperator.Ceq:
				inverted = BinaryOperator.Ceq;
				return true;
			case BinaryOperator.Cne_Un:
				inverted = BinaryOperator.Cne_Un;
				return true;
			case BinaryOperator.Cge:
				inverted = BinaryOperator.Cle;
				return true;
			case BinaryOperator.Cge_Un:
				inverted = BinaryOperator.Cle_Un;
				return true;
			case BinaryOperator.Cgt:
				inverted = BinaryOperator.Clt;
				return true;
			case BinaryOperator.Cgt_Un:
				inverted = BinaryOperator.Clt_Un;
				return true;
			case BinaryOperator.Cle:
				inverted = BinaryOperator.Cge;
				return true;
			case BinaryOperator.Cle_Un:
				inverted = BinaryOperator.Cge_Un;
				return true;
			case BinaryOperator.Clt:
				inverted = BinaryOperator.Cgt;
				return true;
			case BinaryOperator.Clt_Un:
				inverted = BinaryOperator.Cgt_Un;
				return true;
			default:
				inverted = BinaryOperator.Add;
				return false;
			}
		}
	}
}

