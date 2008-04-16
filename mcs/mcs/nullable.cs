//
// nullable.cs: Nullable types support
//
// Authors: Martin Baulig (martin@ximian.com)
//          Miguel de Icaza (miguel@ximian.com)
//          Marek Safar (marek.safar@gmail.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2004 Novell, Inc
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
	
namespace Mono.CSharp.Nullable
{
	public class NullableType : TypeExpr
	{
		Expression underlying;

		public NullableType (Expression underlying, Location l)
		{
			this.underlying = underlying;
			loc = l;

			eclass = ExprClass.Type;
		}

		public NullableType (Type type, Location loc)
			: this (new TypeExpression (type, loc), loc)
		{ }

		public override string Name {
			get { return underlying.ToString () + "?"; }
		}

		public override string FullName {
			get { return underlying.ToString () + "?"; }
		}

		protected override TypeExpr DoResolveAsTypeStep (IResolveContext ec)
		{
			TypeArguments args = new TypeArguments (loc);
			args.Add (underlying);

			if (TypeManager.generic_nullable_type == null) {
				TypeManager.generic_nullable_type = TypeManager.CoreLookupType (
					"System", "Nullable`1", Kind.Struct, true);
			}

			ConstructedType ctype = new ConstructedType (TypeManager.generic_nullable_type, args, loc);
			return ctype.ResolveAsTypeTerminal (ec, false);
		}
	}

	public sealed class NullableInfo
	{
		public readonly Type Type;
		public readonly Type UnderlyingType;
		public readonly MethodInfo HasValue;
		public readonly MethodInfo Value;
		public readonly MethodInfo GetValueOrDefault;
		public readonly ConstructorInfo Constructor;

		public NullableInfo (Type type)
		{
			Type = type;
			UnderlyingType = TypeManager.GetTypeArguments (type) [0];

			PropertyInfo has_value_pi = TypeManager.GetPredefinedProperty (type, "HasValue", Location.Null);
			PropertyInfo value_pi = TypeManager.GetPredefinedProperty (type, "Value", Location.Null);
			GetValueOrDefault = TypeManager.GetPredefinedMethod (type, "GetValueOrDefault", Location.Null, Type.EmptyTypes);

			HasValue = has_value_pi.GetGetMethod (false);
			Value = value_pi.GetGetMethod (false);
#if MS_COMPATIBLE
			if (UnderlyingType.Module == CodeGen.Module.Builder) {
				Type o_type = TypeManager.DropGenericTypeArguments (type);
				Constructor = TypeBuilder.GetConstructor (type,
					TypeManager.GetPredefinedConstructor (o_type, Location.Null, o_type.GetGenericArguments ()));
				return;
			}
#endif
			Constructor = type.GetConstructor (new Type[] { UnderlyingType });
		}
	}
	
	public class HasValue : Expression
	{
		Expression expr;
		NullableInfo info;

		private HasValue (Expression expr)
		{
			this.expr = expr;
		}
		
		public static Expression Create (Expression expr, EmitContext ec)
		{
			return new HasValue (expr).Resolve (ec);
		}

		public override void Emit (EmitContext ec)
		{
			IMemoryLocation memory_loc = expr as IMemoryLocation;
			if (memory_loc == null) {
				LocalTemporary temp = new LocalTemporary (expr.Type);
				expr.Emit (ec);
				temp.Store (ec);
				memory_loc = temp;
			}
			memory_loc.AddressOf (ec, AddressOp.LoadStore);
			ec.ig.EmitCall (OpCodes.Call, info.HasValue, null);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			this.info = new NullableInfo (expr.Type);

			type = TypeManager.bool_type;
			eclass = expr.eclass;
			return this;
		}
	}		

	public class Unwrap : Expression, IMemoryLocation, IAssignMethod
	{
		Expression expr;
		NullableInfo info;

		LocalTemporary temp;
		bool has_temp;

		protected Unwrap (Expression expr)
		{
			this.expr = expr;
			this.loc = expr.Location;
		}

		public static Unwrap Create (Expression expr, EmitContext ec)
		{
			return new Unwrap (expr).Resolve (ec) as Unwrap;
		}
		
		public override Expression CreateExpressionTree (EmitContext ec)
		{
			return expr.CreateExpressionTree (ec);
		}			

		public override Expression DoResolve (EmitContext ec)
		{
			if (expr == null)
				return null;

			temp = new LocalTemporary (expr.Type);

			info = new NullableInfo (expr.Type);
			type = info.UnderlyingType;
			eclass = expr.eclass;
			return this;
		}
		
		public override Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			return DoResolve (ec);
		}			

		public override void Emit (EmitContext ec)
		{
			AddressOf (ec, AddressOp.LoadStore);
			ec.ig.EmitCall (OpCodes.Call, info.Value, null);
		}

		public void EmitCheck (EmitContext ec)
		{
			AddressOf (ec, AddressOp.LoadStore);
			ec.ig.EmitCall (OpCodes.Call, info.HasValue, null);
		}

		public void EmitGetValueOrDefault (EmitContext ec)
		{
			AddressOf (ec, AddressOp.LoadStore);
			ec.ig.EmitCall (OpCodes.Call, info.GetValueOrDefault, null);
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

		public void Store (EmitContext ec)
		{
			create_temp (ec);
		}

		void create_temp (EmitContext ec)
		{
			if ((temp != null) && !has_temp) {
				expr.Emit (ec);
				temp.Store (ec);
				has_temp = true;
			}
		}

		public void LoadTemporary (EmitContext ec)
		{
			temp.Emit (ec);
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			create_temp (ec);
			if (temp != null)
				temp.AddressOf (ec, AddressOp.LoadStore);
			else
				((IMemoryLocation) expr).AddressOf (ec, AddressOp.LoadStore);
		}

		public void Emit (EmitContext ec, bool leave_copy)
		{
			create_temp (ec);
			if (leave_copy) {
				if (temp != null)
					temp.Emit (ec);
				else
					expr.Emit (ec);
			}

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

			public override Expression DoResolve (EmitContext ec)
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

		public static new Expression Create (Expression expr, Type type)
		{
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

		public static Expression CreateFromExpression (Expression e)
		{
			Report.Warning (458, 2, e.Location, "The result of the expression is always `null' of type `{0}'",
				TypeManager.CSharpName (e.Type));

			return ReducedExpression.Create (Create (e.Type, e.Location), e);
		}

		public override Expression CreateExpressionTree (EmitContext ec)
		{
			ArrayList args = new ArrayList (2);
			args.Add (new Argument (this));
			args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));

			return CreateExpressionFactoryCall ("Constant", args);
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

	public abstract class Lifted : Expression, IMemoryLocation
	{
		Expression expr, underlying, wrap, null_value;
		Unwrap unwrap;

		protected Lifted (Expression expr, Location loc)
		{
			this.expr = expr;
			this.loc = loc;
		}
		
		public override Expression CreateExpressionTree (EmitContext ec)
		{
			return underlying.CreateExpressionTree (ec);
		}			

		public override Expression DoResolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return null;

			unwrap = Unwrap.Create (expr, ec);
			if (unwrap == null)
				return null;

			underlying = ResolveUnderlying (unwrap, ec);
			if (underlying == null)
				return null;

			TypeExpr target_type = new NullableType (underlying.Type, loc);
			target_type = target_type.ResolveAsTypeTerminal (ec, false);
			if (target_type == null)
				return null;

			wrap = Wrap.Create (underlying, target_type.Type);
			if (wrap == null)
				return null;

			null_value = LiftedNull.Create (wrap.Type, loc);

			type = wrap.Type;
			eclass = ExprClass.Value;
			return this;
		}

		protected abstract Expression ResolveUnderlying (Expression unwrap, EmitContext ec);

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label is_null_label = ig.DefineLabel ();
			Label end_label = ig.DefineLabel ();

			unwrap.EmitCheck (ec);
			ig.Emit (OpCodes.Brfalse, is_null_label);

			wrap.Emit (ec);
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

	public class LiftedConversion : Lifted
	{
		public readonly bool IsUser;
		public readonly bool IsExplicit;
		public readonly Type TargetType;

		public LiftedConversion (Expression expr, Type target_type, bool is_user,
					 bool is_explicit, Location loc)
			: base (expr, loc)
		{
			this.IsUser = is_user;
			this.IsExplicit = is_explicit;
			this.TargetType = target_type;
		}

		protected override Expression ResolveUnderlying (Expression unwrap, EmitContext ec)
		{
			Type type = TypeManager.GetTypeArguments (TargetType) [0];

			if (IsUser) {
				if (IsExplicit)
					return Convert.ExplicitUserConversion (ec, unwrap, type, loc);
				else
					return Convert.ImplicitUserConversion (ec, unwrap, type, loc);
			} else {
				if (IsExplicit)
					return Convert.ExplicitConversion (ec, unwrap, type, loc);
				else
					return Convert.ImplicitConversion (ec, unwrap, type, loc);
			}
		}
	}

	public class LiftedUnaryOperator : Lifted
	{
		public readonly Unary.Operator Oper;

		public LiftedUnaryOperator (Unary.Operator op, Expression expr, Location loc)
			: base (expr, loc)
		{
			this.Oper = op;
		}

		protected override Expression ResolveUnderlying (Expression unwrap, EmitContext ec)
		{
			return new Unary (Oper, unwrap, loc).Resolve (ec);
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

		//
		// CSC 2 has this behavior, it allows structs to be compared
		// with the null literal *outside* of a generics context and
		// inlines that as true or false.
		//
		Expression CreateNullConstant (Expression expr)
		{
			// FIXME: Handle side effect constants
			Constant c = new BoolConstant (Oper == Operator.Inequality, loc);

			if ((Oper & Operator.EqualityMask) != 0) {
				Report.Warning (472, 2, loc, "The result of comparing `{0}' against null is always `{1}'. " +
						"This operation is undocumented and it is temporary supported for compatibility reasons only",
						expr.GetSignatureForError (), c.AsString ());
			} else {
				Report.Warning (464, 2, loc, "The result of comparing type `{0}' against null is always `{1}'",
						expr.GetSignatureForError (), c.AsString ());
			}

			return ReducedExpression.Create (c, this);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (eclass != ExprClass.Invalid)
				return this;

			if ((Oper & Operator.LogicalMask) != 0) {
				Error_OperatorCannotBeApplied (left, right);
				return null;
			}

			left_orig = left;
			if (TypeManager.IsNullableType (left.Type)) {
				left = left_unwrap = Unwrap.Create (left, ec);
				if (left == null)
					return null;
			}

			right_orig = right;
			if (TypeManager.IsNullableType (right.Type)) {
				right = right_unwrap = Unwrap.Create (right, ec);
				if (right == null)
					return null;
			}

			//
			// Some details are in 6.4.2, 7.2.7
			// Arguments can be lifted for equal operators when the return type is bool and both
			// arguments are of same type
			//	
			if (left is NullLiteral) {
				left = right;
				left_null_lifted = true;
				type = TypeManager.bool_type;
			}

			if (right is NullLiteral) {
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

			left_unwrap.EmitGetValueOrDefault (ec);
			ig.Emit (OpCodes.Brtrue_S, load_right);

			right_unwrap.EmitGetValueOrDefault (ec);
			ig.Emit (OpCodes.Brtrue_S, load_left);

			left_unwrap.EmitCheck (ec);
			ig.Emit (OpCodes.Brfalse_S, load_right);

			// load left
			ig.MarkLabel (load_left);

			if (Oper == Operator.BitwiseAnd) {
				left_unwrap.LoadTemporary (ec);
			} else {
				right_unwrap.LoadTemporary (ec);
				right_unwrap = left_unwrap;
			}
			ig.Emit (OpCodes.Br_S, end_label);

			// load right
			ig.MarkLabel (load_right);
			right_unwrap.LoadTemporary (ec);

			ig.MarkLabel (end_label);
		}

		//
		// Emits optimized equality or inequality operator when possible
		//
		bool EmitEquality (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			//
			// Either left or right is null
			//
			if (left_unwrap != null && right.IsNull) {
				left_unwrap.EmitCheck (ec);
				if (Oper == Binary.Operator.Equality) {
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Ceq);
				}
				return true;
			}

			if (right_unwrap != null && left.IsNull) {
				right_unwrap.EmitCheck (ec);
				if (Oper == Binary.Operator.Equality) {
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Ceq);
				}
				return true;
			}

			if (user_operator != null)
				return false;

			Label dissimilar_label = ig.DefineLabel ();
			Label end_label = ig.DefineLabel ();

			if (left_unwrap != null)
				left_unwrap.EmitGetValueOrDefault (ec);
			else
				left.Emit (ec);

			if (right_unwrap != null)
				right_unwrap.EmitGetValueOrDefault (ec);
			else
				right.Emit (ec);

			ig.Emit (OpCodes.Bne_Un_S, dissimilar_label);

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
			return true;
		}
		
		public override void EmitBranchable (EmitContext ec, Label target, bool onTrue)
		{
			Emit (ec);
			ec.ig.Emit (onTrue ? OpCodes.Brtrue : OpCodes.Brfalse, target);
		}			

		public override void Emit (EmitContext ec)
		{
			if (left_unwrap != null)
				left_unwrap.Store (ec);

			if (right_unwrap != null) {
				if (right.Equals (left))
					right_unwrap = left_unwrap;
				else
					right_unwrap.Store (ec);
			}

			if (user_operator == null && IsBitwiseBoolean) {
				EmitBitwiseBoolean (ec);
				return;
			}

			if ((Oper & Operator.EqualityMask) != 0) {
				if (EmitEquality (ec))
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
				ec.ig.Emit (OpCodes.Newobj, wrap_ctor);

			ig.Emit (OpCodes.Br_S, end_label);
			ig.MarkLabel (is_null_label);

			if ((Oper & Operator.ComparisonMask) != 0) {
				if (Oper == Operator.Equality)
					ig.Emit (OpCodes.Ldc_I4_1);
				else
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
				l = TypeManager.GetTypeArguments (l) [0];

			base.EmitOperator (ec, l);
		}

		bool IsBitwiseBoolean {
			get {
				return (Oper & Operator.BitwiseMask) != 0 && left_unwrap != null && right_unwrap != null &&
				left_unwrap.Type == TypeManager.bool_type && right_unwrap.Type == TypeManager.bool_type;
			}
		}

		Expression LiftResult (EmitContext ec, Expression res_expr)
		{
			TypeExpr lifted_type;

			//
			// Avoid double conversion
			//
			if (left_unwrap == null || left_null_lifted || !TypeManager.IsEqual (left_unwrap.Type, left.Type)) {
				lifted_type = new NullableType (left.Type, loc);
				lifted_type = lifted_type.ResolveAsTypeTerminal (ec, false);
				if (lifted_type == null)
					return null;

				if (left is UserCast || left is TypeCast)
					left.Type = lifted_type.Type;
				else
					left = EmptyCast.Create (left, lifted_type.Type);
			}

			if (right_unwrap == null || right_null_lifted || !TypeManager.IsEqual (right_unwrap.Type, right.Type)) {
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
				res_expr.Type = lifted_type.Type;
			}

			if (left_null_lifted) {
				left = LiftedNull.Create (right.Type, left.Location);

				if ((Oper & (Operator.ArithmeticMask | Operator.ShiftMask)) != 0)
					return LiftedNull.CreateFromExpression (res_expr);

				//
				// Value types and null comparison
				//
				if (right_unwrap == null || (Oper & Operator.RelationalMask) != 0)
					return CreateNullConstant (right_orig).Resolve (ec);
			}

			if (right_null_lifted) {
				right = LiftedNull.Create (left.Type, right.Location);

				if ((Oper & (Operator.ArithmeticMask | Operator.ShiftMask)) != 0)
					return LiftedNull.CreateFromExpression (res_expr);

				//
				// Value types and null comparison
				//
				if (left_unwrap == null || (Oper & Operator.RelationalMask) != 0)
					return CreateNullConstant (left_orig).Resolve (ec);
			}

			return res_expr;
		}

		protected override Expression ResolveOperatorPredefined (EmitContext ec, Binary.PredefinedOperator [] operators, bool primitives_only)
		{
			Expression e = base.ResolveOperatorPredefined (ec, operators, primitives_only);

			if (e == this)
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

		protected override Expression ResolveUserOperator (EmitContext ec, Type l, Type r)
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
		
		public override Expression CreateExpressionTree (EmitContext ec)
		{
			UserCast uc = left as UserCast;
			Expression conversion = null;
			if (uc != null) {
				left = uc.Source;

				ArrayList c_args = new ArrayList (2);
				c_args.Add (new Argument (uc.CreateExpressionTree (ec)));
				c_args.Add (new Argument (left.CreateExpressionTree (ec)));
				conversion = CreateExpressionFactoryCall ("Lambda", c_args);
			}

			ArrayList args = new ArrayList (3);
			args.Add (new Argument (left.CreateExpressionTree (ec)));
			args.Add (new Argument (right.CreateExpressionTree (ec)));
			if (conversion != null)
				args.Add (new Argument (conversion));
			
			return CreateExpressionFactoryCall ("Coalesce", args);
		}			

		public override Expression DoResolve (EmitContext ec)
		{
			if (type != null)
				return this;

			left = left.Resolve (ec);
			right = right.Resolve (ec);

			if (left == null || right == null)
				return null;

			eclass = ExprClass.Value;
			Type ltype = left.Type, rtype = right.Type;
			Expression expr;

			if (TypeManager.IsNullableType (ltype)) {
				NullableInfo info = new NullableInfo (ltype);

				unwrap = Unwrap.Create (left, ec);
				if (unwrap == null)
					return null;

				expr = Convert.ImplicitConversion (ec, right, info.UnderlyingType, loc);
				if (expr != null) {
					left = unwrap;
					right = expr;
					type = expr.Type;
					return this;
				}
			} else if (!TypeManager.IsReferenceType (ltype)) {
				Binary.Error_OperatorCannotBeApplied (loc, "??", ltype, rtype);
				return null;
			}

			expr = Convert.ImplicitConversion (ec, right, ltype, loc);
			if (expr != null) {
				type = expr.Type;
				right = expr;
				return this;
			}

			Expression left_null = unwrap != null ? unwrap : left;
			expr = Convert.ImplicitConversion (ec, left_null, rtype, loc);
			if (expr != null) {
				left = expr;
				type = rtype;
				return this;
			}

			Binary.Error_OperatorCannotBeApplied (loc, "??", ltype, rtype);
			return null;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			Label is_null_label = ig.DefineLabel ();
			Label end_label = ig.DefineLabel ();

			if (unwrap != null) {
				unwrap.EmitCheck (ec);
				ig.Emit (OpCodes.Brfalse, is_null_label);

				left.Emit (ec);
				ig.Emit (OpCodes.Br, end_label);

				ig.MarkLabel (is_null_label);
				right.Emit (ec);

				ig.MarkLabel (end_label);
			} else {
				left.Emit (ec);
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Brtrue, end_label);

				ig.MarkLabel (is_null_label);

				ig.Emit (OpCodes.Pop);
				right.Emit (ec);

				ig.MarkLabel (end_label);
			}
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
		Expression expr, null_value;
		UnaryMutator underlying;
		Unwrap unwrap;

		public LiftedUnaryMutator (UnaryMutator.Mode mode, Expression expr, Location loc)
		{
			this.expr = expr;
			this.Mode = mode;
			this.loc = loc;

			eclass = ExprClass.Value;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return null;

			unwrap = Unwrap.Create (expr, ec);
			if (unwrap == null)
				return null;

			underlying = (UnaryMutator) new UnaryMutator (Mode, unwrap, loc).Resolve (ec);
			if (underlying == null)
				return null;

			type = expr.Type;
			null_value = LiftedNull.Create (type, loc);
			return this;
		}

		void DoEmit (EmitContext ec, bool is_expr)
		{
			ILGenerator ig = ec.ig;
			Label is_null_label = ig.DefineLabel ();
			Label end_label = ig.DefineLabel ();

			unwrap.EmitCheck (ec);
			ig.Emit (OpCodes.Brfalse, is_null_label);

			if (is_expr)
				underlying.Emit (ec);
			else
				underlying.EmitStatement (ec);
			ig.Emit (OpCodes.Br, end_label);

			ig.MarkLabel (is_null_label);
			if (is_expr)
				null_value.Emit (ec);

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

