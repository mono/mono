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
		public class CastExpression : BoxedExpression
		{
			public readonly TypeNode CastToType;
			public readonly BoxedExpression Expr;

			public CastExpression (TypeNode castToType, BoxedExpression expr)
			{
				this.CastToType = castToType;
				this.Expr = expr;
			}

			public override bool IsCast {
				get { return true; }
			}

			public override PathElement[] AccessPath {
				get { return this.Expr.AccessPath; }
			}

			public override BoxedExpression BinaryLeftArgument {
				get { return this.Expr.BinaryLeftArgument; }
			}

			public override BoxedExpression BinaryRightArgument {
				get { return this.Expr.BinaryRightArgument; }
			}

			public override BinaryOperator BinaryOperator {
				get { return this.Expr.BinaryOperator; }
			}

			public override object Constant {
				get { return this.Expr.Constant; }
			}

			public override object ConstantType {
				get { return this.Expr.ConstantType; }
			}

			public override bool IsBinary {
				get { return this.Expr.IsBinary; }
			}

			public override bool IsBooleanTyped {
				get { return this.Expr.IsBooleanTyped; }
			}

			public override bool IsConstant {
				get { return this.Expr.IsConstant; }
			}

			public override bool IsSizeof {
				get { return this.Expr.IsSizeof; }
			}

			public override bool IsNull {
				get { return this.Expr.IsNull; }
			}

			public override bool IsIsinst {
				get { return this.Expr.IsIsinst; }
			}

			public override bool IsResult {
				get { return this.Expr.IsResult; }
			}

			public override bool IsUnary {
				get { return this.Expr.IsUnary; }
			}

			public override bool IsVariable {
				get { return this.Expr.IsVariable; }
			}

			public override BoxedExpression UnaryArgument {
				get { return this.Expr.UnaryArgument; }
			}

			public override UnaryOperator UnaryOperator {
				get { return this.Expr.UnaryOperator; }
			}

			public override object UnderlyingVariable {
				get { return this.Expr.UnderlyingVariable; }
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.Expr.AddFreeVariables (set);
			}

			public override BoxedExpression Substitute<Variable> (Func<Variable, BoxedExpression, BoxedExpression> map)
			{
				return this.Expr.Substitute (map);
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return this.Expr.ForwardDecode<Data, Result, Visitor> (pc, visitor, data);
			}

			public override bool IsBinaryExpression (out BinaryOperator op, out BoxedExpression left, out BoxedExpression right)
			{
				return this.Expr.IsBinaryExpression (out op, out left, out right);
			}

			protected internal override BoxedExpression RecursiveSubstitute (BoxedExpression what, BoxedExpression replace)
			{
				return this.Expr.RecursiveSubstitute (what, replace);
			}

			public override BoxedExpression Substitute (BoxedExpression what, BoxedExpression replace)
			{
				return this.Expr.Substitute (what, replace);
			}

			public override bool IsUnaryExpression (out UnaryOperator op, out BoxedExpression argument)
			{
				return this.Expr.IsUnaryExpression (out op, out argument);
			}
		}
}

