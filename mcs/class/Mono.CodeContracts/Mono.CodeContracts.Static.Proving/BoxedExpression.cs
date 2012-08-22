using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.Inference.Interface;
using Mono.CodeContracts.Static.Proving.BoxedExpressions;

namespace Mono.CodeContracts.Static.Proving
{
	internal abstract class BoxedExpression
	{
		private  static IEqualityComparer<KeyValuePair<BoxedExpression, IEnumerable<MinimalProofObligation>>> equalityPairComparerCache; 
		public virtual bool IsVariable {
			get { return false; }
		}

		public virtual bool IsBooleanTyped {
			get { return false; }
		}

		public virtual object UnderlyingVariable {
			get { return false; }
		}

		public virtual PathElement[] AccessPath {
			get { return null; }
		}

		public virtual bool IsConstant {
			get { return false; }
		}

		public virtual object Constant {
			get { throw new InvalidOperationException (); }
		}

		public virtual object ConstantType {
			get { throw new InvalidOperationException (); }
		}

		public virtual bool IsSizeof {
			get { return false; }
		}

		public virtual bool IsUnary {
			get { return false; }
		}

		public virtual UnaryOperator UnaryOperator {
			get { throw new InvalidOperationException (); }
		}

		public virtual BoxedExpression UnaryArgument {
			get { throw new InvalidOperationException (); }
		}

		public virtual bool IsBinary {
			get { return false; }
		}

		public virtual BinaryOperator BinaryOperator {
			get { throw new InvalidOperationException (); }
		}

		public virtual BoxedExpression BinaryLeftArgument {
			get { throw new InvalidOperationException (); }
		}

		public virtual BoxedExpression BinaryRightArgument {
			get { throw new InvalidOperationException (); }
		}

		public virtual bool IsIsinst {
			get { return false; }
		}

		public virtual bool IsNull {
			get { return false; }
		}

		public virtual bool IsCast {
			get { return false; }
		}

		public virtual bool IsResult {
			get { return false; }
		}

		public virtual bool TryGetType (out object type)
		{
			type = null;
			return false;
		}
		        
		public static IEqualityComparer<KeyValuePair<BoxedExpression, IEnumerable<MinimalProofObligation>>> EqualityPairComparer
		{
			get
			{
				if(BoxedExpression.equalityPairComparerCache == null)
				{
					//BoxedExpression.equalityPairComparerCache = (IEqualityComparer<KeyValuePair<BoxedExpression, IEnumerable<MinimalProofObligation>>>) new BoxedExpression
				}
			}
		}

		public virtual bool IsBinaryExpression (out BinaryOperator op, out BoxedExpression left, out BoxedExpression right)
		{
			op = BinaryOperator.Add;
			left = null;
			right = null;
			return false;
		}

		public virtual bool IsUnaryExpression (out UnaryOperator op, out BoxedExpression argument)
		{
			op = UnaryOperator.Conv_i;
			argument = null;
			return false;
		}

		public virtual bool IsIsinstExpression (out BoxedExpression expr, out TypeNode type)
		{
			expr = null;
			type = null;
			return false;
		}

		public abstract void AddFreeVariables (HashSet<BoxedExpression> set);

		public virtual BoxedExpression Substitute (BoxedExpression what, BoxedExpression replace)
		{
			if (this == what || Equals (what))
				return replace;

			return RecursiveSubstitute (what, replace);
		}

		public abstract BoxedExpression Substitute<Variable> (Func<Variable, BoxedExpression, BoxedExpression> map);

		protected internal virtual BoxedExpression RecursiveSubstitute (BoxedExpression what, BoxedExpression replace)
		{
			return this;
		}

		public abstract Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			where Visitor : IILVisitor<PC, Dummy, Dummy, Data, Result>;

		public static BoxedExpression Var (object var)
		{
			return new VariableExpression (var);
		}

		public static BoxedExpression For<Variable, Expression> (Expression external, IFullExpressionDecoder<Variable, Expression> decoder)
			where Expression : IEquatable<Expression>
		{
			return new ExternalBox<Variable, Expression> (external, decoder);
		}

		public static BoxedExpression MakeIsinst (TypeNode type, BoxedExpression arg)
		{
			return new IsinstExpression (arg, type);
		}

		public static BoxedExpression Convert<Variable, ExternalExpression> (ExternalExpression expr, IFullExpressionDecoder<Variable, ExternalExpression> decoder)
		{
			TypeNode type;
			object value;
			if (decoder.IsConstant (expr, out value, out type))
				return new ConstantExpression (value, type);
			if (decoder.IsNull (expr))
				return new ConstantExpression (null, null);

			object variable;
			if (decoder.IsVariable (expr, out variable)) {
				LispList<PathElement> variableAccessPath = decoder.GetVariableAccessPath (expr);
				return new VariableExpression (variable, variableAccessPath);
			}

			if (decoder.IsSizeof (expr, out type)) {
				int sizeAsConstant;
				return decoder.TrySizeOfAsConstant (expr, out sizeAsConstant) ? new SizeOfExpression (type, sizeAsConstant) : new SizeOfExpression (type);
			}

			ExternalExpression arg;
			if (decoder.IsIsinst (expr, out arg, out type))
				return new IsinstExpression (Convert (arg, decoder), type);

			UnaryOperator op;
			if (decoder.IsUnaryExpression (expr, out op, out arg))
				return new Mono.CodeContracts.Static.Proving.BoxedExpressions.UnaryExpression (op, Convert (arg, decoder));

			BinaryOperator bop;
			ExternalExpression left;
			ExternalExpression right;
			if (!decoder.IsBinaryExpression (expr, out bop, out left, out right))
				throw new InvalidOperationException ();

			return new BinaryExpression (bop, Convert (left, decoder), Convert (right, decoder));
		}

		public static BoxedExpression Binary (BinaryOperator op, BoxedExpression left, BoxedExpression right, object frameworkVar = null)
		{
			return (BoxedExpression)new BinaryExpression (op, left, right, frameworkVar);
		}

		public static bool SimpleSyntacticEquality (BoxedExpression left, BoxedExpression right)
		{
			if (left == null || right == null)
				return left == right;
			if (left.Equals ((object)right))
				return true;

			BinaryOperator binaryOp_1, binaryOp_2, inverted;
			BoxedExpression left_1, left_2, right_1, right_2;

			if (left.IsBinaryExpression (out  binaryOp_1, out left_1, out right_1) && BinaryOperatorExtensions.IsComparisonBinaryOperator (binaryOp_1) && (right.IsBinaryExpression (out binaryOp_2, out left_2, out right_2) && BinaryOperatorExtensions.IsComparisonBinaryOperator (binaryOp_2)) && (BinaryOperatorExtensions.TryInvert (binaryOp_2, out inverted) && binaryOp_1 == inverted && BoxedExpression.SimpleSyntacticEquality (left_1, right_2)))
				return BoxedExpression.SimpleSyntacticEquality (right_1, left_2);
			else
				return false;
		}

		public abstract void Dispatch (IBoxedExpressionController controller);
	}
}
