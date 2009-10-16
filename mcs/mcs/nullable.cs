//
// nullable.cs: Nullable types support
//
// Authors: Martin Baulig (martin@ximian.com)
//          Miguel de Icaza (miguel@ximian.com)
//          Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004-2008 Novell, Inc
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
	
namespace Mono.CSharp.Nullable
{
	public class NullableType : TypeExpr
	{
		TypeExpr underlying;

		public NullableType (TypeExpr underlying, Location l)
		{
			this.underlying = underlying;
			loc = l;

			eclass = ExprClass.Type;
		}

		public NullableType (Type type, Location loc)
			: this (new TypeExpression (type, loc), loc)
		{ }

		protected override TypeExpr DoResolveAsTypeStep (IMemberContext ec)
		{
			if (TypeManager.generic_nullable_type == null) {
				TypeManager.generic_nullable_type = TypeManager.CoreLookupType (ec.Compiler,
					"System", "Nullable`1", Kind.Struct, true);
			}

			TypeArguments args = new TypeArguments (underlying);
			GenericTypeExpr ctype = new GenericTypeExpr (TypeManager.generic_nullable_type, args, loc);
			return ctype.ResolveAsTypeTerminal (ec, false);
		}

		public override TypeExpr ResolveAsTypeTerminal (IMemberContext ec, bool silent)
		{
			return ResolveAsBaseTerminal (ec, silent);
		}		
	}

	public sealed class NullableInfo
	{
		public readonly Type Type;
		public readonly Type UnderlyingType;
		public MethodInfo HasValue;
		public MethodInfo Value;
		public MethodInfo GetValueOrDefault;
		public ConstructorInfo Constructor;

		public NullableInfo (Type type)
		{
			Type = type;
			UnderlyingType = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (type) [0]);

			PropertyInfo has_value_pi = TypeManager.GetPredefinedProperty (type, "HasValue", Location.Null, Type.EmptyTypes);
			PropertyInfo value_pi = TypeManager.GetPredefinedProperty (type, "Value", Location.Null, Type.EmptyTypes);
			GetValueOrDefault = TypeManager.GetPredefinedMethod (type, "GetValueOrDefault", Location.Null, Type.EmptyTypes);

			HasValue = has_value_pi.GetGetMethod (false);
			Value = value_pi.GetGetMethod (false);

			// When compiling corlib
			if (type.Module == RootContext.ToplevelTypes.Builder) {
				TypeContainer tc = TypeManager.LookupGenericTypeContainer (type);
				
				// TODO: check for correct overload
				Constructor c = ((Constructor) tc.InstanceConstructors [0]);

#if GMCS_SOURCE
				Constructor = TypeBuilder.GetConstructor (type, c.ConstructorBuilder);
#endif
				return;
			}

#if MS_COMPATIBLE
			if (UnderlyingType.Module == RootContext.ToplevelTypes.Builder) {
				ConstructorInfo cinfo = TypeManager.DropGenericTypeArguments (type).GetConstructors ()[0];
				Constructor = TypeBuilder.GetConstructor (type, cinfo);
				return;
			}
#endif

			Constructor = type.GetConstructor (new Type[] { UnderlyingType });
		}
	}

	public class Unwrap : Expression, IMemoryLocation, IAssignMethod
	{
		Expression expr;
		NullableInfo info;

		LocalTemporary temp;
		readonly bool useDefaultValue;

		Unwrap (Expression expr, bool useDefaultValue)
		{
			this.expr = expr;
			this.loc = expr.Location;
			this.useDefaultValue = useDefaultValue;

			info = new NullableInfo (expr.Type);
			type = info.UnderlyingType;
			eclass = expr.eclass;
		}

		public static Expression Create (Expression expr)
		{
			//
			// Avoid unwraping and wraping of same type
			//
			Wrap wrap = expr as Wrap;
			if (wrap != null)
				return wrap.Child;

			return Create (expr, false);
		}

		public static Unwrap Create (Expression expr, bool useDefaultValue)
		{
			return new Unwrap (expr, useDefaultValue);
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return expr.CreateExpressionTree (ec);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			return this;
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			return DoResolve (ec);
		}

		public override void Emit (EmitContext ec)
		{
			Store (ec);
			if (useDefaultValue)
				Invocation.EmitCall (ec, false, this, info.GetValueOrDefault, null, loc);
			else
				Invocation.EmitCall (ec, false, this, info.Value, null, loc);
		}

		public void EmitCheck (EmitContext ec)
		{
			Store (ec);
			Invocation.EmitCall (ec, false, this, info.HasValue, null, loc);
		}

		public override bool Equals (object obj)
		{
			Unwrap uw = obj as Unwrap;
			return uw != null && expr.Equals (uw.expr);
		}

		public Expression Original {
			get {
				return expr;
			}
		}
		
		public override int GetHashCode ()
		{
			return expr.GetHashCode ();
		}

		public override bool IsNull {
			get {
				return expr.IsNull;
			}
		}

		void Store (EmitContext ec)
		{
			if (expr is VariableReference)
				return;

			if (temp != null)
				return;

			expr.Emit (ec);
			LocalVariable.Store (ec);
		}

		public void Load (EmitContext ec)
		{
			if (expr is VariableReference)
				expr.Emit (ec);
			else
				LocalVariable.Emit (ec);
		}

#if NET_4_0
		public override System.Linq.Expressions.Expression MakeExpression (BuilderContext ctx)
		{
			return expr.MakeExpression (ctx);
		}
#endif

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			type = storey.MutateType (type);
			info.Constructor = storey.MutateConstructor (info.Constructor);
			info.HasValue = storey.MutateGenericMethod (info.HasValue);
			info.GetValueOrDefault = storey.MutateGenericMethod (info.GetValueOrDefault);
			info.Value = storey.MutateGenericMethod (info.Value);
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			IMemoryLocation ml = expr as VariableReference;
			if (ml != null) 
				ml.AddressOf (ec, mode);
			else
				LocalVariable.AddressOf (ec, mode);
		}

		//
		// Keeps result of non-variable expression
		//
		LocalTemporary LocalVariable {
			get {
				if (temp == null)
					temp = new LocalTemporary (info.Type);
				return temp;
			}
		}

		public void Emit (EmitContext ec, bool leave_copy)
		{
			if (leave_copy)
				Load (ec);

			Emit (ec);
		}

		public void EmitAssign (EmitContext ec, Expression source,
					bool leave_copy, bool prepare_for_load)
		{
			InternalWrap wrap = new InternalWrap (source, info, loc);
			((IAssignMethod) expr).EmitAssign (ec, wrap, leave_copy, false);
		}

		protected class InternalWrap : Expression
		{
			public Expression expr;
			public NullableInfo info;

			public InternalWrap (Expression expr, NullableInfo info, Location loc)
			{
				this.expr = expr;
				this.info = info;
				this.loc = loc;

				type = info.Type;
				eclass = ExprClass.Value;
			}

			public override Expression CreateExpressionTree (ResolveContext ec)
			{
				throw new NotSupportedException ("ET");
			}

			public override Expression DoResolve (ResolveContext ec)
			{
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				expr.Emit (ec);
				ec.ig.Emit (OpCodes.Newobj, info.Constructor);
			}
		}
	}

	public class Wrap : TypeCast
	{
		readonly NullableInfo info;

		protected Wrap (Expression expr, Type type)
			: base (expr, type)
		{
			info = new NullableInfo (type);
			eclass = ExprClass.Value;
		}

		public Expression Child {
			get { return child; }
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			TypeCast child_cast = child as TypeCast;
			if (child_cast != null) {
				child.Type = type;
				return child_cast.CreateExpressionTree (ec);
			}

			return base.CreateExpressionTree (ec);
		}

		public static Expression Create (Expression expr, Type type)
		{
			//
			// Avoid unwraping and wraping of the same type
			//
			Unwrap unwrap = expr as Unwrap;
			if (unwrap != null && TypeManager.IsEqual (expr.Type, TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (type) [0])))
				return unwrap.Original;
		
			return new Wrap (expr, type);
		}
		
		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);
			ec.ig.Emit (OpCodes.Newobj, info.Constructor);
		}
	}

	//
	// Represents null literal lifted to nullable type
	//
	public class LiftedNull : EmptyConstantCast, IMemoryLocation
	{
		private LiftedNull (Type nullable_type, Location loc)
			: base (new NullLiteral (loc), nullable_type)
		{
			eclass = ExprClass.Value;
		}

		public static Constant Create (Type nullable, Location loc)
		{
			return new LiftedNull (nullable, loc);
		}

		public static Expression CreateFromExpression (ResolveContext ec, Expression e)
		{
			ec.Report.Warning (458, 2, e.Location, "The result of the expression is always `null' of type `{0}'",
				TypeManager.CSharpName (e.Type));

			return ReducedExpression.Create (Create (e.Type, e.Location), e);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (2);
			args.Add (new Argument (this));
			args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));

			return CreateExpressionFactoryCall (ec, "Constant", args);
		}

		public override void Emit (EmitContext ec)
		{
			// TODO: generate less temporary variables
			LocalTemporary value_target = new LocalTemporary (type);

			value_target.AddressOf (ec, AddressOp.Store);
			ec.ig.Emit (OpCodes.Initobj, type);
			value_target.Emit (ec);
		}

		public void AddressOf (EmitContext ec, AddressOp Mode)
		{
			LocalTemporary value_target = new LocalTemporary (type);
				
			value_target.AddressOf (ec, AddressOp.Store);
			ec.ig.Emit (OpCodes.Initobj, type);
			((IMemoryLocation) value_target).AddressOf (ec, Mode);
		}
	}

	//
	// Generic lifting expression, supports all S/S? -> T/T? cases
	//
	public class Lifted : Expression, IMemoryLocation
	{
		Expression expr, null_value;
		Unwrap unwrap;

		public Lifted (Expression expr, Unwrap unwrap, Type type)
		{
			this.expr = expr;
			this.unwrap = unwrap;
			this.loc = expr.Location;
			this.type = type;
		}

		public Lifted (Expression expr, Expression unwrap, Type type)
			: this (expr, unwrap as Unwrap, type)
		{
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return expr.CreateExpressionTree (ec);
		}			

		public override Expression DoResolve (ResolveContext ec)
		{
			//
			// It's null when lifting non-nullable type
			//
			if (unwrap == null) {
				// S -> T? is wrap only
				if (TypeManager.IsNullableType (type))
					return Wrap.Create (expr, type);

				// S -> T can be simplified
				return expr;
			}

			// Wrap target for T?
			if (TypeManager.IsNullableType (type)) {
				expr = Wrap.Create (expr, type);
				if (expr == null)
					return null;

				null_value = LiftedNull.Create (type, loc);
			} else {
				null_value = new NullLiteral (type, loc);
			}

			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label is_null_label = ig.DefineLabel ();
			Label end_label = ig.DefineLabel ();

			unwrap.EmitCheck (ec);
			ig.Emit (OpCodes.Brfalse, is_null_label);

			expr.Emit (ec);

			ig.Emit (OpCodes.Br, end_label);
			ig.MarkLabel (is_null_label);

			null_value.Emit (ec);
			ig.MarkLabel (end_label);
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			unwrap.AddressOf (ec, mode);
		}
	}

	public class LiftedUnaryOperator : Unary, IMemoryLocation
	{
		Unwrap unwrap;
		Expression user_operator;

		public LiftedUnaryOperator (Unary.Operator op, Expression expr)
			: base (op, expr)
		{
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			unwrap.AddressOf (ec, mode);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			if (user_operator != null)
				return user_operator.CreateExpressionTree (ec);

			if (Oper == Operator.UnaryPlus)
				return Expr.CreateExpressionTree (ec);

			return base.CreateExpressionTree (ec);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (eclass != ExprClass.Invalid)
				return this;

			unwrap = Unwrap.Create (Expr, false);
			if (unwrap == null)
				return null;

			Expression res = base.ResolveOperator (ec, unwrap);
			if (res != this) {
				if (user_operator == null)
					return res;
			} else {
				res = Expr = LiftExpression (ec, Expr);
			}

			if (res == null)
				return null;

			eclass = ExprClass.Value;
			type = res.Type;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label is_null_label = ig.DefineLabel ();
			Label end_label = ig.DefineLabel ();

			unwrap.EmitCheck (ec);
			ig.Emit (OpCodes.Brfalse, is_null_label);

			NullableInfo ni = new NullableInfo (type);

			if (user_operator != null) {
				user_operator.Emit (ec);
			} else {
				EmitOperator (ec, ni.UnderlyingType);
			}

			ig.Emit (OpCodes.Newobj, ni.Constructor);
			ig.Emit (OpCodes.Br_S, end_label);

			ig.MarkLabel (is_null_label);
			LiftedNull.Create (type, loc).Emit (ec);

			ig.MarkLabel (end_label);
		}

		Expression LiftExpression (ResolveContext ec, Expression expr)
		{
			TypeExpr lifted_type = new NullableType (expr.Type, expr.Location);
			lifted_type = lifted_type.ResolveAsTypeTerminal (ec, false);
			if (lifted_type == null)
				return null;

			expr.Type = lifted_type.Type;
			return expr;
		}

		protected override Expression ResolveEnumOperator (ResolveContext ec, Expression expr)
		{
			expr = base.ResolveEnumOperator (ec, expr);
			if (expr == null)
				return null;

			Expr = LiftExpression (ec, Expr);
			return LiftExpression (ec, expr);
		}

		protected override Expression ResolveUserOperator (ResolveContext ec, Expression expr)
		{
			expr = base.ResolveUserOperator (ec, expr);
			if (expr == null)
				return null;

			//
			// When a user operator is of non-nullable type
			//
			if (Expr is Unwrap) {
				user_operator = LiftExpression (ec, expr);
				return user_operator;
			}

			return expr;
		}
	}

	public class LiftedBinaryOperator : Binary
	{
		Unwrap left_unwrap, right_unwrap;
		bool left_null_lifted, right_null_lifted;
		Expression left_orig, right_orig;
		Expression user_operator;
		ConstructorInfo wrap_ctor;

		public LiftedBinaryOperator (Binary.Operator op, Expression left, Expression right,
					     Location loc)
			: base (op, left, right)
		{
			this.loc = loc;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			if (user_operator != null)
				return user_operator.CreateExpressionTree (ec);

			return base.CreateExpressionTree (ec);
		}

		//
		// CSC 2 has this behavior, it allows structs to be compared
		// with the null literal *outside* of a generics context and
		// inlines that as true or false.
		//
		Expression CreateNullConstant (ResolveContext ec, Expression expr)
		{
			// FIXME: Handle side effect constants
			Constant c = new BoolConstant (Oper == Operator.Inequality, loc);

			if ((Oper & Operator.EqualityMask) != 0) {
				ec.Report.Warning (472, 2, loc, "The result of comparing value type `{0}' with null is `{1}'",
						expr.GetSignatureForError (), c.AsString ());
			} else {
				ec.Report.Warning (464, 2, loc, "The result of comparing type `{0}' with null is always `{1}'",
						expr.GetSignatureForError (), c.AsString ());
			}

			return ReducedExpression.Create (c, this);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (eclass != ExprClass.Invalid)
				return this;

			if ((Oper & Operator.LogicalMask) != 0) {
				Error_OperatorCannotBeApplied (ec, left, right);
				return null;
			}

			bool use_default_call = (Oper & (Operator.BitwiseMask | Operator.EqualityMask)) != 0;
			left_orig = left;
			if (TypeManager.IsNullableType (left.Type)) {
				left = left_unwrap = Unwrap.Create (left, use_default_call);
				if (left == null)
					return null;
			}

			right_orig = right;
			if (TypeManager.IsNullableType (right.Type)) {
				right = right_unwrap = Unwrap.Create (right, use_default_call);
				if (right == null)
					return null;
			}

			//
			// Some details are in 6.4.2, 7.2.7
			// Arguments can be lifted for equal operators when the return type is bool and both
			// arguments are of same type
			//	
			if (left_orig.IsNull) {
				left = right;
				left_null_lifted = true;
				type = TypeManager.bool_type;
			}

			if (right_orig.IsNull) {
				right = left;
				right_null_lifted = true;
				type = TypeManager.bool_type;
			}

			eclass = ExprClass.Value;
			return DoResolveCore (ec, left_orig, right_orig);
		}

		void EmitBitwiseBoolean (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			Label load_left = ig.DefineLabel ();
			Label load_right = ig.DefineLabel ();
			Label end_label = ig.DefineLabel ();

			left_unwrap.Emit (ec);
			ig.Emit (OpCodes.Brtrue_S, load_right);

			right_unwrap.Emit (ec);
			ig.Emit (OpCodes.Brtrue_S, load_left);

			left_unwrap.EmitCheck (ec);
			ig.Emit (OpCodes.Brfalse_S, load_right);

			// load left
			ig.MarkLabel (load_left);

			if (Oper == Operator.BitwiseAnd) {
				left_unwrap.Load (ec);
			} else {
				right_unwrap.Load (ec);
				right_unwrap = left_unwrap;
			}
			ig.Emit (OpCodes.Br_S, end_label);

			// load right
			ig.MarkLabel (load_right);
			right_unwrap.Load (ec);

			ig.MarkLabel (end_label);
		}

		//
		// Emits optimized equality or inequality operator when possible
		//
		void EmitEquality (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			//
			// Either left or right is null
			//
			if (left_unwrap != null && (right_null_lifted || right.IsNull)) {
				left_unwrap.EmitCheck (ec);
				if (Oper == Binary.Operator.Equality) {
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Ceq);
				}
				return;
			}

			if (right_unwrap != null && (left_null_lifted || left.IsNull)) {
				right_unwrap.EmitCheck (ec);
				if (Oper == Binary.Operator.Equality) {
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Ceq);
				}
				return;
			}

			Label dissimilar_label = ig.DefineLabel ();
			Label end_label = ig.DefineLabel ();

			if (user_operator != null) {
				user_operator.Emit (ec);
				ig.Emit (Oper == Operator.Equality ? OpCodes.Brfalse_S : OpCodes.Brtrue_S, dissimilar_label);
			} else {
				left.Emit (ec);
				right.Emit (ec);

				ig.Emit (OpCodes.Bne_Un_S, dissimilar_label);
			}

			if (left_unwrap != null)
				left_unwrap.EmitCheck (ec);

			if (right_unwrap != null)
				right_unwrap.EmitCheck (ec);

			if (left_unwrap != null && right_unwrap != null) {
				if (Oper == Operator.Inequality)
					ig.Emit (OpCodes.Xor);
				else
					ig.Emit (OpCodes.Ceq);
			} else {
				if (Oper == Operator.Inequality) {
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Ceq);
				}
			}

			ig.Emit (OpCodes.Br_S, end_label);

			ig.MarkLabel (dissimilar_label);
			if (Oper == Operator.Inequality)
				ig.Emit (OpCodes.Ldc_I4_1);
			else
				ig.Emit (OpCodes.Ldc_I4_0);

			ig.MarkLabel (end_label);
		}
		
		public override void EmitBranchable (EmitContext ec, Label target, bool onTrue)
		{
			Emit (ec);
			ec.ig.Emit (onTrue ? OpCodes.Brtrue : OpCodes.Brfalse, target);
		}			

		public override void Emit (EmitContext ec)
		{
			//
			// Optimize same expression operation
			//
			if (right_unwrap != null && right.Equals (left))
				right_unwrap = left_unwrap;

			if (user_operator == null && IsBitwiseBoolean) {
				EmitBitwiseBoolean (ec);
				return;
			}

			if ((Oper & Operator.EqualityMask) != 0) {
				EmitEquality (ec);
				return;
			}

			ILGenerator ig = ec.ig;

			Label is_null_label = ig.DefineLabel ();
			Label end_label = ig.DefineLabel ();

			if (left_unwrap != null) {
				left_unwrap.EmitCheck (ec);
				ig.Emit (OpCodes.Brfalse, is_null_label);
			}

			//
			// Don't emit HasValue check when left and right expressions are same
			//
			if (right_unwrap != null && !left.Equals (right)) {
				right_unwrap.EmitCheck (ec);
				ig.Emit (OpCodes.Brfalse, is_null_label);
			}

			EmitOperator (ec, left.Type);

			if (wrap_ctor != null)
				ig.Emit (OpCodes.Newobj, wrap_ctor);

			ig.Emit (OpCodes.Br_S, end_label);
			ig.MarkLabel (is_null_label);

			if ((Oper & Operator.ComparisonMask) != 0) {
				ig.Emit (OpCodes.Ldc_I4_0);
			} else {
				LiftedNull.Create (type, loc).Emit (ec);
			}

			ig.MarkLabel (end_label);
		}

		protected override void EmitOperator (EmitContext ec, Type l)
		{
			if (user_operator != null) {
				user_operator.Emit (ec);
				return;
			}

			if (TypeManager.IsNullableType (l))
				l = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (l) [0]);

			base.EmitOperator (ec, l);
		}

		bool IsBitwiseBoolean {
			get {
				return (Oper & Operator.BitwiseMask) != 0 && left_unwrap != null && right_unwrap != null &&
				left_unwrap.Type == TypeManager.bool_type && right_unwrap.Type == TypeManager.bool_type;
			}
		}

		Expression LiftResult (ResolveContext ec, Expression res_expr)
		{
			TypeExpr lifted_type;

			//
			// Avoid double conversion
			//
			if (left_unwrap == null || left_null_lifted || !TypeManager.IsEqual (left_unwrap.Type, left.Type) || (left_unwrap != null && right_null_lifted)) {
				lifted_type = new NullableType (left.Type, loc);
				lifted_type = lifted_type.ResolveAsTypeTerminal (ec, false);
				if (lifted_type == null)
					return null;

				if (left is UserCast || left is TypeCast)
					left.Type = lifted_type.Type;
				else
					left = EmptyCast.Create (left, lifted_type.Type);
			}

			if (right_unwrap == null || right_null_lifted || !TypeManager.IsEqual (right_unwrap.Type, right.Type) || (right_unwrap != null && left_null_lifted)) {
				lifted_type = new NullableType (right.Type, loc);
				lifted_type = lifted_type.ResolveAsTypeTerminal (ec, false);
				if (lifted_type == null)
					return null;

				if (right is UserCast || right is TypeCast)
					right.Type = lifted_type.Type;
				else
					right = EmptyCast.Create (right, lifted_type.Type);
			}

			if ((Oper & Operator.ComparisonMask) == 0) {
				lifted_type = new NullableType (res_expr.Type, loc);
				lifted_type = lifted_type.ResolveAsTypeTerminal (ec, false);
				if (lifted_type == null)
					return null;

				wrap_ctor = new NullableInfo (lifted_type.Type).Constructor;
				type = res_expr.Type = lifted_type.Type;
			}

			if (left_null_lifted) {
				left = LiftedNull.Create (right.Type, left.Location);

				if ((Oper & (Operator.ArithmeticMask | Operator.ShiftMask | Operator.BitwiseMask)) != 0)
					return LiftedNull.CreateFromExpression (ec, res_expr);

				//
				// Value types and null comparison
				//
				if (right_unwrap == null || (Oper & Operator.RelationalMask) != 0)
					return CreateNullConstant (ec, right_orig).Resolve (ec);
			}

			if (right_null_lifted) {
				right = LiftedNull.Create (left.Type, right.Location);

				if ((Oper & (Operator.ArithmeticMask | Operator.ShiftMask | Operator.BitwiseMask)) != 0)
					return LiftedNull.CreateFromExpression (ec, res_expr);

				//
				// Value types and null comparison
				//
				if (left_unwrap == null || (Oper & Operator.RelationalMask) != 0)
					return CreateNullConstant (ec, left_orig).Resolve (ec);
			}

			return res_expr;
		}

		protected override Expression ResolveOperatorPredefined (ResolveContext ec, Binary.PredefinedOperator [] operators, bool primitives_only, Type enum_type)
		{
			Expression e = base.ResolveOperatorPredefined (ec, operators, primitives_only, enum_type);

			if (e == this || enum_type != null)
				return LiftResult (ec, e);

			//
			// 7.9.9 Equality operators and null
			//
			// The == and != operators permit one operand to be a value of a nullable type and
			// the other to be the null literal, even if no predefined or user-defined operator
			// (in unlifted or lifted form) exists for the operation.
			//
			if (e == null && (Oper & Operator.EqualityMask) != 0) {
				if ((left_null_lifted && right_unwrap != null) || (right_null_lifted && left_unwrap != null))
					return LiftResult (ec, this);
			}

			return e;
		}

		protected override Expression ResolveUserOperator (ResolveContext ec, Type l, Type r)
		{
			Expression expr = base.ResolveUserOperator (ec, l, r);
			if (expr == null)
				return null;

			expr = LiftResult (ec, expr);
			if (expr is Constant)
				return expr;

			type = expr.Type;
			user_operator = expr;
			return this;
		}
	}

	public class NullCoalescingOperator : Expression
	{
		Expression left, right;
		Unwrap unwrap;

		public NullCoalescingOperator (Expression left, Expression right, Location loc)
		{
			this.left = left;
			this.right = right;
			this.loc = loc;
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			if (left.Type == TypeManager.null_type)
				ec.Report.Error (845, loc, "An expression tree cannot contain a coalescing operator with null left side");

			UserCast uc = left as UserCast;
			Expression conversion = null;
			if (uc != null) {
				left = uc.Source;

				Arguments c_args = new Arguments (2);
				c_args.Add (new Argument (uc.CreateExpressionTree (ec)));
				c_args.Add (new Argument (left.CreateExpressionTree (ec)));
				conversion = CreateExpressionFactoryCall (ec, "Lambda", c_args);
			}

			Arguments args = new Arguments (3);
			args.Add (new Argument (left.CreateExpressionTree (ec)));
			args.Add (new Argument (right.CreateExpressionTree (ec)));
			if (conversion != null)
				args.Add (new Argument (conversion));
			
			return CreateExpressionFactoryCall (ec, "Coalesce", args);
		}

		Expression ConvertExpression (ResolveContext ec)
		{
			// TODO: ImplicitConversionExists should take care of this
			if (left.eclass == ExprClass.MethodGroup)
				return null;

			Type ltype = left.Type;

			//
			// If left is a nullable type and an implicit conversion exists from right to underlying type of left,
			// the result is underlying type of left
			//
			if (TypeManager.IsNullableType (ltype)) {
				unwrap = Unwrap.Create (left, false);
				if (unwrap == null)
					return null;

				if (Convert.ImplicitConversionExists (ec, right, unwrap.Type)) {
					left = unwrap;
					type = left.Type;
					right = Convert.ImplicitConversion (ec, right, type, loc);
					return this;
				}
			} else if (TypeManager.IsReferenceType (ltype)) {
				if (Convert.ImplicitConversionExists (ec, right, ltype)) {
					//
					// Reduce (constant ?? expr) to constant
					//
					Constant lc = left as Constant;
					if (lc != null && !lc.IsDefaultValue)
						return new SideEffectConstant (lc, right, loc).Resolve (ec);

					//
					// Reduce (left ?? null) to left OR (null-constant ?? right) to right
					//
					if (right.IsNull || lc != null)
						return ReducedExpression.Create (lc != null ? right : left, this).Resolve (ec);

					right = Convert.ImplicitConversion (ec, right, ltype, loc);
					type = left.Type;
					return this;
				}
			} else {
				return null;
			}

			Type rtype = right.Type;
			if (!Convert.ImplicitConversionExists (ec, unwrap != null ? unwrap : left, rtype) || right.eclass == ExprClass.MethodGroup)
				return null;

			//
			// Reduce (null ?? right) to right
			//
			if (left.IsNull)
				return ReducedExpression.Create (right, this).Resolve (ec);

			left = Convert.ImplicitConversion (ec, unwrap != null ? unwrap : left, rtype, loc);
			type = rtype;
			return this;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (eclass != ExprClass.Invalid)
				return this;

			left = left.Resolve (ec);
			right = right.Resolve (ec);

			if (left == null || right == null)
				return null;

			eclass = ExprClass.Value;

			Expression e = ConvertExpression (ec);
			if (e == null) {
				Binary.Error_OperatorCannotBeApplied (ec, left, right, "??", loc);
				return null;
			}

			return e;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			Label end_label = ig.DefineLabel ();

			if (unwrap != null) {
				Label is_null_label = ig.DefineLabel ();

				unwrap.EmitCheck (ec);
				ig.Emit (OpCodes.Brfalse, is_null_label);

				left.Emit (ec);
				ig.Emit (OpCodes.Br, end_label);

				ig.MarkLabel (is_null_label);
				right.Emit (ec);

				ig.MarkLabel (end_label);
				return;
			}

			left.Emit (ec);

			ig.Emit (OpCodes.Dup);
			ig.Emit (OpCodes.Brtrue, end_label);

			ig.Emit (OpCodes.Pop);
			right.Emit (ec);

			ig.MarkLabel (end_label);
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			left.MutateHoistedGenericType (storey);
			right.MutateHoistedGenericType (storey);
			type = storey.MutateType (type);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			NullCoalescingOperator target = (NullCoalescingOperator) t;

			target.left = left.Clone (clonectx);
			target.right = right.Clone (clonectx);
		}
	}

	public class LiftedUnaryMutator : ExpressionStatement
	{
		public readonly UnaryMutator.Mode Mode;
		Expression expr;
		UnaryMutator underlying;
		Unwrap unwrap;

		public LiftedUnaryMutator (UnaryMutator.Mode mode, Expression expr, Location loc)
		{
			this.expr = expr;
			this.Mode = mode;
			this.loc = loc;

			eclass = ExprClass.Value;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return new SimpleAssign (this, this).CreateExpressionTree (ec);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return null;

			unwrap = Unwrap.Create (expr, false);
			if (unwrap == null)
				return null;

			underlying = (UnaryMutator) new UnaryMutator (Mode, unwrap).Resolve (ec);
			if (underlying == null)
				return null;

			type = expr.Type;
			return this;
		}

		void DoEmit (EmitContext ec, bool is_expr)
		{
			ILGenerator ig = ec.ig;
			Label is_null_label = ig.DefineLabel ();
			Label end_label = ig.DefineLabel ();

			unwrap.EmitCheck (ec);
			ig.Emit (OpCodes.Brfalse, is_null_label);

			if (is_expr) {
				underlying.Emit (ec);
				ig.Emit (OpCodes.Br_S, end_label);
			} else {
				underlying.EmitStatement (ec);
			}

			ig.MarkLabel (is_null_label);
			if (is_expr)
				LiftedNull.Create (type, loc).Emit (ec);

			ig.MarkLabel (end_label);
		}

		public override void Emit (EmitContext ec)
		{
			DoEmit (ec, true);
		}

		public override void EmitStatement (EmitContext ec)
		{
			DoEmit (ec, false);
		}
	}
}

