// 
// BoxedExpression.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis.Paths;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Proving {
	internal abstract class BoxedExpression {
		public virtual bool IsVariable
		{
			get { return false; }
		}

		public virtual bool IsBooleanTyped
		{
			get { return false; }
		}

		public virtual object UnderlyingVariable
		{
			get { return false; }
		}

		public virtual PathElement[] AccessPath
		{
			get { return null; }
		}

		public virtual bool IsConstant
		{
			get { return false; }
		}

		public virtual object Constant
		{
			get { throw new InvalidOperationException (); }
		}

		public virtual object ConstantType
		{
			get { throw new InvalidOperationException (); }
		}

                public virtual bool IsSizeof
		{
			get { return false; }
		}

		public virtual bool IsUnary
		{
			get { return false; }
		}

		public virtual UnaryOperator UnaryOperator
		{
			get { throw new InvalidOperationException (); }
		}

		public virtual BoxedExpression UnaryArgument
		{
			get { throw new InvalidOperationException (); }
		}

		public virtual bool IsBinary
		{
			get { return false; }
		}

		public virtual BinaryOperator BinaryOperator
		{
			get { throw new InvalidOperationException (); }
		}

		public virtual BoxedExpression BinaryLeftArgument
		{
			get { throw new InvalidOperationException (); }
		}

		public virtual BoxedExpression BinaryRightArgument
		{
			get { throw new InvalidOperationException (); }
		}

		public virtual bool IsIsinst
		{
			get { return false; }
		}

		public virtual bool IsNull
		{
			get { return false; }
		}

		public virtual bool IsCast
		{
			get { return false; }
		}

		public virtual bool IsResult
		{
			get { return false; }
		}

		public virtual bool TryGetType (out object type)
		{
			type = null;
			return false;
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

                public virtual bool Sizeof (out int size)
                {
                        return false.Without (out size);
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

                public static BoxedExpression Const (object value, TypeNode type)
                {
                        return new ConstantExpression (value, type);
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
				Sequence<PathElement> variableAccessPath = decoder.GetVariableAccessPath (expr);
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
				return new UnaryExpression (op, Convert (arg, decoder));

			BinaryOperator bop;
			ExternalExpression left;
			ExternalExpression right;
			if (!decoder.IsBinaryExpression (expr, out bop, out left, out right))
				throw new InvalidOperationException ();

			return new BinaryExpression (bop, Convert (left, decoder), Convert (right, decoder));
		}

                public static BoxedExpression Binary (BinaryOperator bop, BoxedExpression left, BoxedExpression right)
                {
                        return new BinaryExpression (bop, left, right);
                }

                public static BoxedExpression Unary (UnaryOperator op, BoxedExpression arg)
                {
                        return new UnaryExpression (op, arg);
                }

		#region Nested type: AssertExpression
		public class AssertExpression : ContractExpression {
			public AssertExpression (BoxedExpression condition, EdgeTag tag, APC pc) : base (condition, tag, pc)
			{
			}

			#region Overrides of ContractExpression
			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.Assert (pc, this.Tag, Dummy.Value, data);
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression cond = this.Condition.Substitute (map);
				if (cond == this.Condition)
					return this;
				if (cond == null)
					return null;

				return new AssertExpression (cond, this.Tag, this.Apc);
			}
			#endregion
		}
		#endregion

		#region Nested type: AssumeExpression
		public class AssumeExpression : ContractExpression {
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
		#endregion

		#region Nested type: BinaryExpression
		public class BinaryExpression : BoxedExpression {
			public readonly BoxedExpression Left;
			public readonly BinaryOperator Op;
			public readonly BoxedExpression Right;

			public BinaryExpression (BinaryOperator op, BoxedExpression left, BoxedExpression right)
			{
				this.Op = op;
				this.Left = left;
				this.Right = right;
			}

			public override bool IsBinary
			{
				get { return true; }
			}

			public override BoxedExpression BinaryLeftArgument
			{
				get { return this.Left; }
			}

			public override BoxedExpression BinaryRightArgument
			{
				get { return this.Right; }
			}

			public override BinaryOperator BinaryOperator
			{
				get { return this.Op; }
			}

			public override bool IsBinaryExpression (out BinaryOperator op, out BoxedExpression left, out BoxedExpression right)
			{
				op = this.Op;
				left = this.Left;
				right = this.Right;
				return true;
			}

			#region Overrides of BoxedExpression
			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.Left.AddFreeVariables (set);
				this.Right.AddFreeVariables (set);
			}

			protected internal override BoxedExpression RecursiveSubstitute (BoxedExpression what, BoxedExpression replace)
			{
				BoxedExpression left = this.Left.Substitute (what, replace);
				BoxedExpression right = this.Right.Substitute (what, replace);
				if (left == this.Left && right == this.Right)
					return this;

				return new BinaryExpression (this.Op, left, right);
			}

			public override BoxedExpression Substitute<Variable> (Func<Variable, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression left = this.Left.Substitute (map);
				if (left == null)
					return null;

				BoxedExpression right = this.Right.Substitute (map);
				if (right == null)
					return null;

				if (this.Left == left && this.Right == right)
					return this;
				return new BinaryExpression (this.Op, left, right);
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.Binary (pc, this.Op, Dummy.Value, Dummy.Value, Dummy.Value, data);
			}
			#endregion
		}
		#endregion

		#region Nested type: CastExpression
		public class CastExpression : BoxedExpression {
			public readonly TypeNode CastToType;
			public readonly BoxedExpression Expr;

			public CastExpression (TypeNode castToType, BoxedExpression expr)
			{
				this.CastToType = castToType;
				this.Expr = expr;
			}

			public override bool IsCast
			{
				get { return true; }
			}

			public override PathElement[] AccessPath
			{
				get { return this.Expr.AccessPath; }
			}

			public override BoxedExpression BinaryLeftArgument
			{
				get { return this.Expr.BinaryLeftArgument; }
			}

			public override BoxedExpression BinaryRightArgument
			{
				get { return this.Expr.BinaryRightArgument; }
			}

			public override BinaryOperator BinaryOperator
			{
				get { return this.Expr.BinaryOperator; }
			}

			public override object Constant
			{
				get { return this.Expr.Constant; }
			}

			public override object ConstantType
			{
				get { return this.Expr.ConstantType; }
			}

			public override bool IsBinary
			{
				get { return this.Expr.IsBinary; }
			}

			public override bool IsBooleanTyped
			{
				get { return this.Expr.IsBooleanTyped; }
			}

			public override bool IsConstant
			{
				get { return this.Expr.IsConstant; }
			}


			public override bool IsSizeof
			{
				get { return this.Expr.IsSizeof; }
			}

			public override bool IsNull
			{
				get { return this.Expr.IsNull; }
			}

			public override bool IsIsinst
			{
				get { return this.Expr.IsIsinst; }
			}

			public override bool IsResult
			{
				get { return this.Expr.IsResult; }
			}

			public override bool IsUnary
			{
				get { return this.Expr.IsUnary; }
			}

			public override bool IsVariable
			{
				get { return this.Expr.IsVariable; }
			}

			public override BoxedExpression UnaryArgument
			{
				get { return this.Expr.UnaryArgument; }
			}

			public override UnaryOperator UnaryOperator
			{
				get { return this.Expr.UnaryOperator; }
			}

			public override object UnderlyingVariable
			{
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
		#endregion

		#region Nested type: ConstantExpression
		public class ConstantExpression : BoxedExpression {
			public readonly TypeNode Type;
			public readonly object Value;
			private readonly bool is_boolean;

			public ConstantExpression (object value, TypeNode type)
				: this (value, type, false)
			{
			}

			public ConstantExpression (object value, TypeNode type, bool isBoolean)
			{
				this.Value = value;
				this.Type = type;
				this.is_boolean = isBoolean;
			}

			public override bool IsBooleanTyped
			{
				get { return this.is_boolean; }
			}

			public override bool IsConstant
			{
				get { return true; }
			}

			public override object Constant
			{
				get { return this.Value; }
			}

			public override object ConstantType
			{
				get { return this.Type; }
			}

			public override bool IsNull
			{
				get
				{
					if (this.Value == null)
						return true;

					var conv = this.Value as IConvertible;
					if (conv != null) {
						try {
							if (conv.ToInt32 (null) == 0)
								return true;
						} catch {
							return false;
						}
					}

					return false;
				}
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				return this;
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				if (this.Value == null)
					return visitor.LoadNull (pc, Dummy.Value, data);

				return visitor.LoadConst (pc, this.Type, this.Value, Dummy.Value, data);
			}
		}
		#endregion

		#region Nested type: ContractExpression
		public abstract class ContractExpression : BoxedExpression {
			public readonly APC Apc;
			public readonly BoxedExpression Condition;
			public readonly EdgeTag Tag;

			public ContractExpression (BoxedExpression condition, EdgeTag tag, APC pc)
			{
				this.Tag = tag;
				this.Condition = condition;
				this.Apc = pc;
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.Condition.AddFreeVariables (set);
			}

			public abstract override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data);
			public abstract override BoxedExpression Substitute<Variable> (Func<Variable, BoxedExpression, BoxedExpression> map);
		}
		#endregion

		#region Nested type: ExternalBox
		public class ExternalBox<Variable, LabeledSymbol> : BoxedExpression
			where LabeledSymbol : IEquatable<LabeledSymbol> {
			private readonly IFullExpressionDecoder<Variable, LabeledSymbol> decoder;
			private readonly LabeledSymbol expr;
			private Optional<Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression>> binary;

			private Optional<Tuple<bool, object, TypeNode>> constant;
			private Optional<Tuple<bool, BoxedExpression, TypeNode>> isInst;
			private Optional<Pair<bool, object>> isVar;
			private Optional<Pair<bool, TypeNode>> type;
			private Optional<Tuple<bool, UnaryOperator, BoxedExpression>> unary;
			private Optional<object> var;

			public ExternalBox (LabeledSymbol external, IFullExpressionDecoder<Variable, LabeledSymbol> decoder)
			{
				this.expr = external;
				this.decoder = decoder;
			}

			public override bool IsBinary
			{
				get
				{
					Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> binary;
					TryGetBinaryFromCache (out binary);
					return binary.Item1;
				}
			}

			public override BinaryOperator BinaryOperator
			{
				get
				{
					Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> binary;
					bool res = TryGetBinaryFromCache (out binary);
					if (!res)
						throw new InvalidOperationException ();

					return binary.Item2;
				}
			}

			public override BoxedExpression BinaryLeftArgument
			{
				get
				{
					Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> binary;
					bool res = TryGetBinaryFromCache (out binary);
					if (!res)
						throw new InvalidOperationException ();

					return binary.Item3;
				}
			}

			public override BoxedExpression BinaryRightArgument
			{
				get
				{
					Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> binary;
					bool res = TryGetBinaryFromCache (out binary);
					if (!res)
						throw new InvalidOperationException ();

					return binary.Item4;
				}
			}

			public override bool IsConstant
			{
				get
				{
					Tuple<bool, object, TypeNode> consta;
					TryGetConstantFromCache (out consta);

					return consta.Item1;
				}
			}

			public override object Constant
			{
				get
				{
					Tuple<bool, object, TypeNode> consta;
					if (!TryGetConstantFromCache (out consta))
						throw new InvalidOperationException ();

					return consta.Item2;
				}
			}

			public override object ConstantType
			{
				get
				{
					Tuple<bool, object, TypeNode> consta;
					if (!TryGetConstantFromCache (out consta))
						throw new InvalidOperationException();

					return consta.Item3;
				}
			}

			public override bool IsIsinst
			{
				get
				{
					Tuple<bool, BoxedExpression, TypeNode> isinst;
					TryGetIsInstFromCache (out isinst);
					return isinst.Item1;
				}
			}

			public override bool IsNull
			{
				get { return this.decoder.IsNull (this.expr); }
			}

			public override bool IsUnary
			{
				get
				{
					Tuple<bool, UnaryOperator, BoxedExpression> unary;
					TryGetUnaryFromCache (out unary);
					return unary.Item1;
				}
			}

			public override UnaryOperator UnaryOperator
			{
				get
				{
					Tuple<bool, UnaryOperator, BoxedExpression> unary;
					if (!TryGetUnaryFromCache (out unary))
						throw new InvalidOperationException();
					return unary.Item2;
				}
			}

			public override BoxedExpression UnaryArgument
			{
				get
				{
					Tuple<bool, UnaryOperator, BoxedExpression> unary;
					if (!TryGetUnaryFromCache (out unary))
						throw new InvalidOperationException ();
					return unary.Item3;
				}
			}

			public override bool IsSizeof
			{
				get
				{
					TypeNode type;
					return this.decoder.IsSizeof (this.expr, out type);
				}
			}

			public override bool IsVariable
			{
				get
				{
					Pair<bool, object> var1;
					TryGetIsVariableFromCache (out var1);
					return var1.Key;
				}
			}

			public override object UnderlyingVariable
			{
				get
				{
					if (!this.var.IsValid)
						this.var = this.decoder.UnderlyingVariable (this.expr);

					return this.var.Value;
				}
			}

			private bool TryGetBinaryFromCache (out Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> binary)
			{
				if (this.binary.IsValid) {
					binary = this.binary.Value;
					return true;
				}
				BinaryOperator op;
				LabeledSymbol left;
				LabeledSymbol right;
				bool res = this.decoder.IsBinaryExpression (this.expr, out op, out left, out right);
				this.binary = binary = new Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> (res, op, For (left, this.decoder), For (right, this.decoder));

				return res;
			}

			private bool TryGetUnaryFromCache (out Tuple<bool, UnaryOperator, BoxedExpression> unary)
			{
				if (this.unary.IsValid) {
					unary = this.unary.Value;
					return true;
				}
				UnaryOperator op;
				LabeledSymbol arg;
				bool res = this.decoder.IsUnaryExpression (this.expr, out op, out arg);
				this.unary = unary = new Tuple<bool, UnaryOperator, BoxedExpression> (res, op, For (arg, this.decoder));

				return res;
			}

			private bool TryGetIsInstFromCache (out Tuple<bool, BoxedExpression, TypeNode> isinst)
			{
				if (this.isInst.IsValid) {
					isinst = this.isInst.Value;
					return true;
				}

				LabeledSymbol arg;
				TypeNode type;
				bool res = this.decoder.IsIsinst (this.expr, out arg, out type);
				this.isInst = isinst = new Tuple<bool, BoxedExpression, TypeNode> (res, For (arg, this.decoder), type);

				return res;
			}

			private bool TryGetConstantFromCache (out Tuple<bool, object, TypeNode> result)
			{
				if (this.constant.IsValid) {
					result = this.constant.Value;
					return true;
				}
				object value;
				TypeNode type;
				bool res = this.decoder.IsConstant (this.expr, out value, out type);
				this.constant = result = new Tuple<bool, object, TypeNode> (res, value, type);

				return res;
			}

			private bool TryGetTypeFromCache (out Pair<bool, TypeNode> result)
			{
				if (this.type.IsValid) {
					result = this.type.Value;
					return true;
				}

				TypeNode type;
				bool res = this.decoder.TryGetType (this.expr, out type);
				this.type = result = new Pair<bool, TypeNode> (res, type);

				return res;
			}

			private bool TryGetIsVariableFromCache (out Pair<bool, object> result)
			{
				if (this.isVar.IsValid) {
					result = this.isVar.Value;
					return true;
				}

				object value;
				bool res = this.decoder.IsVariable (this.expr, out value);
				this.isVar = result = new Pair<bool, object> (res, value);

				return res;
			}

			public override bool IsBinaryExpression (out BinaryOperator op, out BoxedExpression left, out BoxedExpression right)
			{
				Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> bin;
				if (!TryGetBinaryFromCache (out bin) || !bin.Item1) {
					op = BinaryOperator.Add;
					left = null;
					right = null;
					return false;
				}

				op = bin.Item2;
				left = bin.Item3;
				right = bin.Item4;
				return true;
			}

			public override bool IsIsinstExpression (out BoxedExpression expr, out TypeNode type)
			{
				Tuple<bool, BoxedExpression, TypeNode> isinst;
				if (!TryGetIsInstFromCache (out isinst) || !isinst.Item1) {
					expr = null;
					type = null;
					return false;
				}

				expr = isinst.Item2;
				type = isinst.Item3;
				return true;
			}

			public override bool IsUnaryExpression (out UnaryOperator op, out BoxedExpression argument)
			{
				Tuple<bool, UnaryOperator, BoxedExpression> unary;
				if (!TryGetUnaryFromCache (out unary) || !unary.Item1) {
					op = UnaryOperator.Conv_i;
					argument = null;
					return false;
				}

				op = unary.Item2;
				argument = unary.Item3;
				return true;
			}

			protected internal override BoxedExpression RecursiveSubstitute (BoxedExpression what, BoxedExpression replace)
			{
				return Convert (this.expr, this.decoder).Substitute (what, replace);
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.decoder.AddFreeVariables (this.expr, new SetWrapper (set, this.decoder));
			}

			public override BoxedExpression Substitute<V> (Func<V, BoxedExpression, BoxedExpression> map)
			{
				return Convert (this.expr, this.decoder).Substitute (map);
			}

			public override bool TryGetType (out object type)
			{
				Pair<bool, TypeNode> result;
				if (!TryGetTypeFromCache (out result) || !result.Key) {
					type = null;
					return false;
				}

				type = result.Value;
				return true;
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				Tuple<bool, object, TypeNode> constant;
				if (TryGetConstantFromCache (out constant)) {
					if (constant.Item2 != null)
						return visitor.LoadConst (pc, constant.Item3, constant, Dummy.Value, data);

					return visitor.LoadNull (pc, Dummy.Value, data);
				}

				UnaryOperator op;
				LabeledSymbol arg;
				if (this.decoder.IsUnaryExpression (this.expr, out op, out arg))
					return visitor.Unary (pc, op, false, Dummy.Value, Dummy.Value, data);

				BinaryOperator bop;
				LabeledSymbol left;
				LabeledSymbol right;
				if (this.decoder.IsBinaryExpression (this.expr, out bop, out left, out right))
					return visitor.Binary (pc, bop, Dummy.Value, Dummy.Value, Dummy.Value, data);
				TypeNode type;
				if (this.decoder.IsIsinst (this.expr, out arg, out type))
					return visitor.Isinst (pc, type, Dummy.Value, Dummy.Value, data);
				if (this.decoder.IsNull (this.expr))
					return visitor.LoadNull (pc, Dummy.Value, data);
				if (this.decoder.IsSizeof (this.expr, out type))
					return visitor.Sizeof (pc, type, Dummy.Value, data);

				return visitor.Nop (pc, data);
			}

			#region Nested type: SetWrapper
			private struct SetWrapper : ISet<LabeledSymbol>, IEnumerable<LabeledSymbol> {
				private readonly IFullExpressionDecoder<Variable, LabeledSymbol> decoder;
				private readonly HashSet<BoxedExpression> set;

				#region Implementation of IEnumerable
				public IEnumerator<LabeledSymbol> GetEnumerator ()
				{
					throw new NotImplementedException ();
				}

				IEnumerator IEnumerable.GetEnumerator ()
				{
					return GetEnumerator ();
				}
				#endregion

				public SetWrapper (HashSet<BoxedExpression> set, IFullExpressionDecoder<Variable, LabeledSymbol> decoder)
				{
					this.set = set;
					this.decoder = decoder;
				}

				#region Implementation of ICollection<ExternalExpression>
				public void Add (LabeledSymbol item)
				{
					this.set.Add (For (item, this.decoder));
				}

				bool ISet<LabeledSymbol>.Add (LabeledSymbol item)
				{
					Add (item);
					return true;
				}

				public void UnionWith (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public void IntersectWith (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public void ExceptWith (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public void SymmetricExceptWith (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public bool IsSubsetOf (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public bool IsSupersetOf (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public bool IsProperSupersetOf (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public bool IsProperSubsetOf (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public bool Overlaps (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public bool SetEquals (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}


				public void Clear ()
				{
					throw new NotImplementedException ();
				}

				public bool Contains (LabeledSymbol item)
				{
					throw new NotImplementedException ();
				}

				public void CopyTo (LabeledSymbol[] array, int arrayIndex)
				{
					throw new NotImplementedException ();
				}

				public bool Remove (LabeledSymbol item)
				{
					throw new NotImplementedException ();
				}

				public int Count
				{
					get { throw new NotImplementedException (); }
				}

				public bool IsReadOnly
				{
					get { throw new NotImplementedException (); }
				}
				#endregion
			}
			#endregion
			}
		#endregion

		#region Nested type: IsinstExpression
		public class IsinstExpression : BoxedExpression {
			private readonly BoxedExpression arg;
			private readonly TypeNode type;

			public IsinstExpression (BoxedExpression boxedExpression, TypeNode type)
			{
				this.arg = boxedExpression;
				this.type = type;
			}

			public override bool IsIsinst
			{
				get { return true; }
			}

			public override BoxedExpression UnaryArgument
			{
				get { return this.arg; }
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.Isinst (pc, this.type, Dummy.Value, Dummy.Value, data);
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.arg.AddFreeVariables (set);
			}

			public override BoxedExpression Substitute<Variable> (Func<Variable, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression arg = this.arg.Substitute (map);
				if (arg == this.arg)
					return this;
				if (arg == null)
					return null;

				return new IsinstExpression (arg, this.type);
			}
		}
		#endregion

		#region Nested type: OldExpression
		public class OldExpression : BoxedExpression {
			private const string ContractOldValueTemplate = "Contract.OldValue({0})";

			public readonly BoxedExpression Old;
			public readonly TypeNode Type;

			public OldExpression (BoxedExpression old, TypeNode type)
			{
				this.Old = old;
				this.Type = type;
			}

			#region Overrides of BoxedExpression
			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.Old.AddFreeVariables (set);
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression old = this.Old.Substitute (map);
				if (old == this.Old)
					return this;
				if (old == null)
					return null;

				return new OldExpression (old, this.Type);
			}

			public override bool IsBinaryExpression (out BinaryOperator op, out BoxedExpression left, out BoxedExpression right)
			{
				return this.Old.IsBinaryExpression (out op, out left, out right);
			}

			public override bool IsUnaryExpression (out UnaryOperator op, out BoxedExpression argument)
			{
				return this.Old.IsUnaryExpression (out op, out argument);
			}

			public override bool IsIsinstExpression (out BoxedExpression expr, out TypeNode type)
			{
				return this.Old.IsIsinstExpression (out expr, out type);
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.EndOld (pc, new PC (pc.Node, 0), this.Type, Dummy.Value, Dummy.Value, data);
			}
			#endregion

			public override PathElement[] AccessPath
			{
				get { return this.Old.AccessPath; }
			}

			public override BoxedExpression BinaryLeftArgument
			{
				get { return this.Old.BinaryLeftArgument; }
			}

			public override BoxedExpression BinaryRightArgument
			{
				get { return this.Old.BinaryRightArgument; }
			}

			public override BinaryOperator BinaryOperator
			{
				get { return this.Old.BinaryOperator; }
			}

			public override object Constant
			{
				get { return this.Old.Constant; }
			}

			public override object ConstantType
			{
				get { return this.Old.ConstantType; }
			}

			public override bool IsBinary
			{
				get { return this.Old.IsBinary; }
			}

			public override bool IsConstant
			{
				get { return this.Old.IsConstant; }
			}

			public override bool IsSizeof
			{
				get { return this.Old.IsSizeof; }
			}

			public override bool IsNull
			{
				get { return this.Old.IsNull; }
			}

			public override bool IsIsinst
			{
				get { return this.Old.IsIsinst; }
			}

			public override bool IsUnary
			{
				get { return this.Old.IsUnary; }
			}

			public override bool IsVariable
			{
				get { return this.Old.IsVariable; }
			}

			public override BoxedExpression UnaryArgument
			{
				get { return this.Old.UnaryArgument; }
			}

			public override UnaryOperator UnaryOperator
			{
				get { return this.Old.UnaryOperator; }
			}

			public override object UnderlyingVariable
			{
				get { return this.Old.UnderlyingVariable; }
			}
		}
		#endregion

		#region Nested type: PC
		public struct PC {
			public readonly int Index;
			public readonly BoxedExpression Node;

			public PC (BoxedExpression expr, int index)
			{
				this.Node = expr;
				this.Index = index;
			}
		}
		#endregion

		#region Nested type: ResultExpression
		public class ResultExpression : BoxedExpression {
			private const string ContractResultTemplate = "Contract.Result<{0}>()";

			public readonly TypeNode Type;

			public ResultExpression (TypeNode type)
			{
				this.Type = type;
			}

			public override bool IsResult
			{
				get { return true; }
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				return this;
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.LoadResult (pc, this.Type, Dummy.Value, Dummy.Value, data);
			}
		}
		#endregion

		#region Nested type: SizeOfExpression
		public class SizeOfExpression : BoxedExpression {
			public readonly int SizeAsConstant;
			public readonly TypeNode Type;

			public SizeOfExpression (TypeNode type, int sizeAsConstant)
			{
				this.Type = type;
				this.SizeAsConstant = sizeAsConstant;
			}

			public SizeOfExpression (TypeNode type)
				: this (type, -1)
			{
			}

			public override bool IsSizeof
			{
				get { return true; }
                        }

                        public override bool Sizeof (out int size)
                        {
                                size = SizeAsConstant;
                                return size >= 0;
                        }

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				return this;
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.Sizeof (pc, this.Type, Dummy.Value, data);
			}
		}
		#endregion

		#region Nested type: UnaryExpression
		public class UnaryExpression : BoxedExpression {
			public readonly BoxedExpression Argument;
			public readonly UnaryOperator Op;

			public UnaryExpression (UnaryOperator op, BoxedExpression argument)
			{
				this.Op = op;
				this.Argument = argument;
			}

			public override bool IsUnary
			{
				get { return true; }
			}

			public override BoxedExpression UnaryArgument
			{
				get { return this.Argument; }
			}

			public override UnaryOperator UnaryOperator
			{
				get { return this.Op; }
			}

			public override bool IsUnaryExpression (out UnaryOperator op, out BoxedExpression argument)
			{
				op = this.Op;
				argument = this.Argument;
				return true;
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.Argument.AddFreeVariables (set);
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression argument = this.Argument.Substitute (map);
				if (argument == this.Argument)
					return this;
				if (argument == null)
					return null;

				return new UnaryExpression (this.Op, argument);
			}

			protected internal override BoxedExpression RecursiveSubstitute (BoxedExpression what, BoxedExpression replace)
			{
				BoxedExpression argument = this.Argument.Substitute (what, replace);

				if (argument == this.Argument)
					return this;

				return new UnaryExpression (this.Op, argument);
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.Unary (pc, this.Op, false, Dummy.Value, Dummy.Value, data);
			}

			public override bool Equals (object obj)
			{
				if (this == obj)
					return true;

				var unary = obj as UnaryExpression;
				return unary != null && this.Op == unary.Op && this.Argument.Equals (unary.Argument);
			}

			public override int GetHashCode ()
			{
				return this.Op.GetHashCode () * 13 + (this.Argument == null ? 0 : this.Argument.GetHashCode ());
			}

		}
		#endregion

		#region Nested type: ValueAtReturnExpression
		public class ValueAtReturnExpression : BoxedExpression {
			private const string ContractValueAtReturnTemplate = "Contract.ValueAtReturn({0})";

			public readonly TypeNode Type;
			public readonly BoxedExpression Value;

			public ValueAtReturnExpression (BoxedExpression old, TypeNode type)
			{
				this.Value = old;
				this.Type = type;
			}

			#region Overrides of BoxedExpression
			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.Value.AddFreeVariables (set);
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression value = this.Value.Substitute (map);
				if (value == this.Value)
					return this;
				if (value == null)
					return null;

				return new ValueAtReturnExpression (value, this.Type);
			}

			public override bool IsBinaryExpression (out BinaryOperator op, out BoxedExpression left, out BoxedExpression right)
			{
				return this.Value.IsBinaryExpression (out op, out left, out right);
			}

			public override bool IsUnaryExpression (out UnaryOperator op, out BoxedExpression argument)
			{
				return this.Value.IsUnaryExpression (out op, out argument);
			}

			public override bool IsIsinstExpression (out BoxedExpression expr, out TypeNode type)
			{
				return this.Value.IsIsinstExpression (out expr, out type);
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				throw new NotImplementedException ();
			}
			#endregion

			public override PathElement[] AccessPath
			{
				get { return this.Value.AccessPath; }
			}

			public override BoxedExpression BinaryLeftArgument
			{
				get { return this.Value.BinaryLeftArgument; }
			}

			public override BoxedExpression BinaryRightArgument
			{
				get { return this.Value.BinaryRightArgument; }
			}

			public override BinaryOperator BinaryOperator
			{
				get { return this.Value.BinaryOperator; }
			}

			public override object Constant
			{
				get { return this.Value.Constant; }
			}

			public override object ConstantType
			{
				get { return this.Value.ConstantType; }
			}

			public override bool IsBinary
			{
				get { return this.Value.IsBinary; }
			}

			public override bool IsConstant
			{
				get { return this.Value.IsConstant; }
			}

			public override bool IsSizeof
			{
				get { return this.Value.IsSizeof; }
			}

			public override bool IsNull
			{
				get { return this.Value.IsNull; }
			}

			public override bool IsIsinst
			{
				get { return this.Value.IsIsinst; }
			}

			public override bool IsUnary
			{
				get { return this.Value.IsUnary; }
			}

			public override bool IsVariable
			{
				get { return this.Value.IsVariable; }
			}

			public override BoxedExpression UnaryArgument
			{
				get { return this.Value.UnaryArgument; }
			}

			public override UnaryOperator UnaryOperator
			{
				get { return this.Value.UnaryOperator; }
			}

			public override object UnderlyingVariable
			{
				get { return this.Value.UnderlyingVariable; }
			}
		}
		#endregion

		#region Nested type: VariableExpression
		public class VariableExpression : BoxedExpression {
			private readonly PathElement[] Path;
			private readonly object UnderlyingVar;
			public readonly object VarType;

			public VariableExpression (object var)
				: this (var, (Sequence<PathElement>) null)
			{
			}

			public VariableExpression (object var, Sequence<PathElement> path)
			{
				this.UnderlyingVar = var;
				this.Path = path != null ? path.AsEnumerable ().ToArray () : null;
			}

			public VariableExpression (object var, Sequence<PathElement> path, object type)
				: this (var, path)
			{
				this.VarType = type;
			}

			public VariableExpression (object var, PathElement[] path)
			{
				this.UnderlyingVar = var;
				this.Path = path;
			}

			public VariableExpression (object var, PathElement[] path, object type)
				: this (var, path)
			{
				this.VarType = type;
			}

			public override bool IsVariable
			{
				get { return true; }
			}

			public override object UnderlyingVariable
			{
				get { return this.UnderlyingVar; }
			}

			public override PathElement[] AccessPath
			{
				get { return this.Path; }
			}

			public override bool IsBooleanTyped
			{
				get { return this.Path != null && this.Path [this.Path.Length - 1].IsBooleanTyped; }
			}

			public override bool TryGetType (out object type)
			{
				type = this.VarType;
				return type != null;
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				set.Add (this);
			}

			protected internal override BoxedExpression RecursiveSubstitute (BoxedExpression what, BoxedExpression replace)
			{
				var varExpr = what as VariableExpression;
				if (varExpr != null && varExpr.UnderlyingVar.Equals (this.UnderlyingVar))
					return replace;

				return this;
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.Nop (pc, data);
			}

			public override bool Equals (object obj)
			{
				if (this == obj)
					return true;
				var boxedExpression = obj as BoxedExpression;
				if (boxedExpression != null && boxedExpression.IsVariable)
					return this.UnderlyingVar.Equals (boxedExpression.UnderlyingVariable);

				return false;
			}

			public override int GetHashCode ()
			{
				return this.UnderlyingVariable != null ? 0 : this.UnderlyingVariable.GetHashCode ();
			}

			public override BoxedExpression Substitute<Variable> (Func<Variable, BoxedExpression, BoxedExpression> map)
			{
				if (!(this.UnderlyingVar is Variable))
					return this;
				var variable = ((Variable) this.UnderlyingVar);
				return map (variable, this);
			}
		}
		#endregion
	}
}
