//
// expression.cs: Expression representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright 2001, 2002, 2003 Ximian, Inc.
// Copyright 2003-2008 Novell, Inc.
//
#define USE_OLD

namespace Mono.CSharp {
	using System;
	using System.Collections;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;

#if NET_4_0
	using SLE = System.Linq.Expressions;
#endif

	//
	// This is an user operator expression, automatically created during
	// resolve phase
	//
	public class UserOperatorCall : Expression {
		public delegate Expression ExpressionTreeExpression (ResolveContext ec, MethodGroupExpr mg);

		protected readonly Arguments arguments;
		protected readonly MethodGroupExpr mg;
		readonly ExpressionTreeExpression expr_tree;

		public UserOperatorCall (MethodGroupExpr mg, Arguments args, ExpressionTreeExpression expr_tree, Location loc)
		{
			this.mg = mg;
			this.arguments = args;
			this.expr_tree = expr_tree;

			type = TypeManager.TypeToCoreType (((MethodInfo) mg).ReturnType);
			eclass = ExprClass.Value;
			this.loc = loc;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			if (expr_tree != null)
				return expr_tree (ec, mg);

			Arguments args = Arguments.CreateForExpressionTree (ec, arguments,
				new NullLiteral (loc),
				mg.CreateExpressionTree (ec));

			return CreateExpressionFactoryCall (ec, "Call", args);
		}

		protected override void CloneTo (CloneContext context, Expression target)
		{
			// Nothing to clone
		}
		
		public override Expression DoResolve (ResolveContext ec)
		{
			//
			// We are born fully resolved
			//
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			mg.EmitCall (ec, arguments);
		}

#if NET_4_0
		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			return SLE.Expression.Call ((MethodInfo) mg, Arguments.MakeExpression (arguments, ctx));
		}
#endif

		public MethodGroupExpr Method {
			get { return mg; }
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			arguments.MutateHoistedGenericType (storey);
			mg.MutateHoistedGenericType (storey);
		}
	}

	public class ParenthesizedExpression : Expression
	{
		public Expression Expr;

		public ParenthesizedExpression (Expression expr)
		{
			Expr = expr;
			loc = expr.Location;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			Expr = Expr.Resolve (ec);
			return Expr;
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			return Expr.DoResolveLValue (ec, right_side);
		}

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Should not happen");
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			ParenthesizedExpression target = (ParenthesizedExpression) t;

			target.Expr = Expr.Clone (clonectx);
		}
	}
	
	//
	//   Unary implements unary expressions.
	//
	public class Unary : Expression
	{
		public enum Operator : byte {
			UnaryPlus, UnaryNegation, LogicalNot, OnesComplement,
			AddressOf,  TOP
		}

		static Type [] [] predefined_operators;

		public readonly Operator Oper;
		public Expression Expr;
		Expression enum_conversion;

		public Unary (Operator op, Expression expr)
		{
			Oper = op;
			Expr = expr;
			loc = expr.Location;
		}

		// <summary>
		//   This routine will attempt to simplify the unary expression when the
		//   argument is a constant.
		// </summary>
		Constant TryReduceConstant (ResolveContext ec, Constant e)
		{
			if (e is EmptyConstantCast)
				return TryReduceConstant (ec, ((EmptyConstantCast) e).child);
			
			if (e is SideEffectConstant) {
				Constant r = TryReduceConstant (ec, ((SideEffectConstant) e).value);
				return r == null ? null : new SideEffectConstant (r, e, r.Location);
			}

			Type expr_type = e.Type;
			
			switch (Oper){
			case Operator.UnaryPlus:
				// Unary numeric promotions
				if (expr_type == TypeManager.byte_type)
					return new IntConstant (((ByteConstant)e).Value, e.Location);
				if (expr_type == TypeManager.sbyte_type)
					return new IntConstant (((SByteConstant)e).Value, e.Location);
				if (expr_type == TypeManager.short_type)
					return new IntConstant (((ShortConstant)e).Value, e.Location);
				if (expr_type == TypeManager.ushort_type)
					return new IntConstant (((UShortConstant)e).Value, e.Location);
				if (expr_type == TypeManager.char_type)
					return new IntConstant (((CharConstant)e).Value, e.Location);
				
				// Predefined operators
				if (expr_type == TypeManager.int32_type || expr_type == TypeManager.uint32_type ||
				    expr_type == TypeManager.int64_type || expr_type == TypeManager.uint64_type ||
				    expr_type == TypeManager.float_type || expr_type == TypeManager.double_type ||
				    expr_type == TypeManager.decimal_type) {
					return e;
				}
				
				return null;
				
			case Operator.UnaryNegation:
				// Unary numeric promotions
				if (expr_type == TypeManager.byte_type)
					return new IntConstant (-((ByteConstant)e).Value, e.Location);
				if (expr_type == TypeManager.sbyte_type)
					return new IntConstant (-((SByteConstant)e).Value, e.Location);
				if (expr_type == TypeManager.short_type)
					return new IntConstant (-((ShortConstant)e).Value, e.Location);
				if (expr_type == TypeManager.ushort_type)
					return new IntConstant (-((UShortConstant)e).Value, e.Location);
				if (expr_type == TypeManager.char_type)
					return new IntConstant (-((CharConstant)e).Value, e.Location);
				
				// Predefined operators
				if (expr_type == TypeManager.int32_type) {
					int value = ((IntConstant)e).Value;
					if (value == int.MinValue) {
						if (ec.ConstantCheckState) {
							ConstantFold.Error_CompileTimeOverflow (ec, loc);
							return null;
						}
						return e;
					}
					return new IntConstant (-value, e.Location);
				}
				if (expr_type == TypeManager.int64_type) {
					long value = ((LongConstant)e).Value;
					if (value == long.MinValue) {
						if (ec.ConstantCheckState) {
							ConstantFold.Error_CompileTimeOverflow (ec, loc);
							return null;
						}
						return e;
					}
					return new LongConstant (-value, e.Location);
				}
				
				if (expr_type == TypeManager.uint32_type) {
					UIntLiteral uil = e as UIntLiteral;
					if (uil != null) {
						if (uil.Value == 2147483648)
							return new IntLiteral (int.MinValue, e.Location);
						return new LongLiteral (-uil.Value, e.Location);
					}
					return new LongConstant (-((UIntConstant)e).Value, e.Location);
				}
				
				if (expr_type == TypeManager.uint64_type) {
					ULongLiteral ull = e as ULongLiteral;
					if (ull != null && ull.Value == 9223372036854775808)
						return new LongLiteral (long.MinValue, e.Location);
					return null;
				}
				
				if (expr_type == TypeManager.float_type) {
					FloatLiteral fl = e as FloatLiteral;
					// For better error reporting
					if (fl != null)
						return new FloatLiteral (-fl.Value, e.Location);

					return new FloatConstant (-((FloatConstant)e).Value, e.Location);
				}
				if (expr_type == TypeManager.double_type) {
					DoubleLiteral dl = e as DoubleLiteral;
					// For better error reporting
					if (dl != null)
						return new DoubleLiteral (-dl.Value, e.Location);

					return new DoubleConstant (-((DoubleConstant)e).Value, e.Location);
				}
				if (expr_type == TypeManager.decimal_type)
					return new DecimalConstant (-((DecimalConstant)e).Value, e.Location);
				
				return null;
				
			case Operator.LogicalNot:
				if (expr_type != TypeManager.bool_type)
					return null;
				
				bool b = (bool)e.GetValue ();
				return new BoolConstant (!b, e.Location);
				
			case Operator.OnesComplement:
				// Unary numeric promotions
				if (expr_type == TypeManager.byte_type)
					return new IntConstant (~((ByteConstant)e).Value, e.Location);
				if (expr_type == TypeManager.sbyte_type)
					return new IntConstant (~((SByteConstant)e).Value, e.Location);
				if (expr_type == TypeManager.short_type)
					return new IntConstant (~((ShortConstant)e).Value, e.Location);
				if (expr_type == TypeManager.ushort_type)
					return new IntConstant (~((UShortConstant)e).Value, e.Location);
				if (expr_type == TypeManager.char_type)
					return new IntConstant (~((CharConstant)e).Value, e.Location);
				
				// Predefined operators
				if (expr_type == TypeManager.int32_type)
					return new IntConstant (~((IntConstant)e).Value, e.Location);
				if (expr_type == TypeManager.uint32_type)
					return new UIntConstant (~((UIntConstant)e).Value, e.Location);
				if (expr_type == TypeManager.int64_type)
					return new LongConstant (~((LongConstant)e).Value, e.Location);
				if (expr_type == TypeManager.uint64_type){
					return new ULongConstant (~((ULongConstant)e).Value, e.Location);
				}
				if (e is EnumConstant) {
					e = TryReduceConstant (ec, ((EnumConstant)e).Child);
					if (e != null)
						e = new EnumConstant (e, expr_type);
					return e;
				}
				return null;
			}
			throw new Exception ("Can not constant fold: " + Oper.ToString());
		}
		
		protected Expression ResolveOperator (ResolveContext ec, Expression expr)
		{
			eclass = ExprClass.Value;

			if (predefined_operators == null)
				CreatePredefinedOperatorsTable ();

			Type expr_type = expr.Type;
			Expression best_expr;

			//
			// Primitive types first
			//
			if (TypeManager.IsPrimitiveType (expr_type)) {
				best_expr = ResolvePrimitivePredefinedType (expr);
				if (best_expr == null)
					return null;

				type = best_expr.Type;
				Expr = best_expr;
				return this;
			}

			//
			// E operator ~(E x);
			//
			if (Oper == Operator.OnesComplement && TypeManager.IsEnumType (expr_type))
				return ResolveEnumOperator (ec, expr);

			return ResolveUserType (ec, expr);
		}

		protected virtual Expression ResolveEnumOperator (ResolveContext ec, Expression expr)
		{
			Type underlying_type = TypeManager.GetEnumUnderlyingType (expr.Type);
			Expression best_expr = ResolvePrimitivePredefinedType (EmptyCast.Create (expr, underlying_type));
			if (best_expr == null)
				return null;

			Expr = best_expr;
			enum_conversion = Convert.ExplicitNumericConversion (new EmptyExpression (best_expr.Type), underlying_type);
			type = expr.Type;
			return EmptyCast.Create (this, type);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return CreateExpressionTree (ec, null);
		}

		Expression CreateExpressionTree (ResolveContext ec, MethodGroupExpr user_op)
		{
			string method_name;
			switch (Oper) {
			case Operator.AddressOf:
				Error_PointerInsideExpressionTree (ec);
				return null;
			case Operator.UnaryNegation:
				if (ec.HasSet (ResolveContext.Options.CheckedScope) && user_op == null && !IsFloat (type))
					method_name = "NegateChecked";
				else
					method_name = "Negate";
				break;
			case Operator.OnesComplement:
			case Operator.LogicalNot:
				method_name = "Not";
				break;
			case Operator.UnaryPlus:
				method_name = "UnaryPlus";
				break;
			default:
				throw new InternalErrorException ("Unknown unary operator " + Oper.ToString ());
			}

			Arguments args = new Arguments (2);
			args.Add (new Argument (Expr.CreateExpressionTree (ec)));
			if (user_op != null)
				args.Add (new Argument (user_op.CreateExpressionTree (ec)));
			return CreateExpressionFactoryCall (ec, method_name, args);
		}

		static void CreatePredefinedOperatorsTable ()
		{
			predefined_operators = new Type [(int) Operator.TOP] [];

			//
			// 7.6.1 Unary plus operator
			//
			predefined_operators [(int) Operator.UnaryPlus] = new Type [] {
				TypeManager.int32_type, TypeManager.uint32_type,
				TypeManager.int64_type, TypeManager.uint64_type,
				TypeManager.float_type, TypeManager.double_type,
				TypeManager.decimal_type
			};

			//
			// 7.6.2 Unary minus operator
			//
			predefined_operators [(int) Operator.UnaryNegation] = new Type [] {
				TypeManager.int32_type, 
				TypeManager.int64_type,
				TypeManager.float_type, TypeManager.double_type,
				TypeManager.decimal_type
			};

			//
			// 7.6.3 Logical negation operator
			//
			predefined_operators [(int) Operator.LogicalNot] = new Type [] {
				TypeManager.bool_type
			};

			//
			// 7.6.4 Bitwise complement operator
			//
			predefined_operators [(int) Operator.OnesComplement] = new Type [] {
				TypeManager.int32_type, TypeManager.uint32_type,
				TypeManager.int64_type, TypeManager.uint64_type
			};
		}

		//
		// Unary numeric promotions
		//
		static Expression DoNumericPromotion (Operator op, Expression expr)
		{
			Type expr_type = expr.Type;
			if ((op == Operator.UnaryPlus || op == Operator.UnaryNegation || op == Operator.OnesComplement) &&
				expr_type == TypeManager.byte_type || expr_type == TypeManager.sbyte_type ||
				expr_type == TypeManager.short_type || expr_type == TypeManager.ushort_type ||
				expr_type == TypeManager.char_type)
				return Convert.ImplicitNumericConversion (expr, TypeManager.int32_type);

			if (op == Operator.UnaryNegation && expr_type == TypeManager.uint32_type)
				return Convert.ImplicitNumericConversion (expr, TypeManager.int64_type);

			return expr;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (Oper == Operator.AddressOf) {
				return ResolveAddressOf (ec);
			}

			Expr = Expr.Resolve (ec);
			if (Expr == null)
				return null;

			if (TypeManager.IsDynamicType (Expr.Type)) {
				Arguments args = new Arguments (1);
				args.Add (new Argument (Expr));
				return new DynamicUnaryConversion (GetOperatorExpressionTypeName (), args, loc).Resolve (ec);
			}

			if (TypeManager.IsNullableType (Expr.Type))
				return new Nullable.LiftedUnaryOperator (Oper, Expr).Resolve (ec);

			//
			// Attempt to use a constant folding operation.
			//
			Constant cexpr = Expr as Constant;
			if (cexpr != null) {
				cexpr = TryReduceConstant (ec, cexpr);
				if (cexpr != null)
					return cexpr;
			}

			Expression expr = ResolveOperator (ec, Expr);
			if (expr == null)
				Error_OperatorCannotBeApplied (ec, loc, OperName (Oper), Expr.Type);
			
			//
			// Reduce unary operator on predefined types
			//
			if (expr == this && Oper == Operator.UnaryPlus)
				return Expr;

			return expr;
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right)
		{
			return null;
		}

		public override void Emit (EmitContext ec)
		{
			EmitOperator (ec, type);
		}

		protected void EmitOperator (EmitContext ec, Type type)
		{
			ILGenerator ig = ec.ig;

			switch (Oper) {
			case Operator.UnaryPlus:
				Expr.Emit (ec);
				break;
				
			case Operator.UnaryNegation:
				if (ec.HasSet (EmitContext.Options.CheckedScope) && !IsFloat (type)) {
					ig.Emit (OpCodes.Ldc_I4_0);
					if (type == TypeManager.int64_type)
						ig.Emit (OpCodes.Conv_U8);
					Expr.Emit (ec);
					ig.Emit (OpCodes.Sub_Ovf);
				} else {
					Expr.Emit (ec);
					ig.Emit (OpCodes.Neg);
				}
				
				break;
				
			case Operator.LogicalNot:
				Expr.Emit (ec);
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Ceq);
				break;
				
			case Operator.OnesComplement:
				Expr.Emit (ec);
				ig.Emit (OpCodes.Not);
				break;
				
			case Operator.AddressOf:
				((IMemoryLocation)Expr).AddressOf (ec, AddressOp.LoadStore);
				break;
				
			default:
				throw new Exception ("This should not happen: Operator = "
						     + Oper.ToString ());
			}

			//
			// Same trick as in Binary expression
			//
			if (enum_conversion != null)
				enum_conversion.Emit (ec);
		}

		public override void EmitBranchable (EmitContext ec, Label target, bool on_true)
		{
			if (Oper == Operator.LogicalNot)
				Expr.EmitBranchable (ec, target, !on_true);
			else
				base.EmitBranchable (ec, target, on_true);
		}

		public override void EmitSideEffect (EmitContext ec)
		{
			Expr.EmitSideEffect (ec);
		}

		public static void Error_OperatorCannotBeApplied (ResolveContext ec, Location loc, string oper, Type t)
		{
			ec.Report.Error (23, loc, "The `{0}' operator cannot be applied to operand of type `{1}'",
				oper, TypeManager.CSharpName (t));
		}

		//
		// Converts operator to System.Linq.Expressions.ExpressionType enum name
		//
		string GetOperatorExpressionTypeName ()
		{
			switch (Oper) {
			case Operator.OnesComplement:
				return "OnesComplement";
			case Operator.LogicalNot:
				return "Not";
			case Operator.UnaryNegation:
				return "Negate";
			case Operator.UnaryPlus:
				return "UnaryPlus";
			default:
				throw new NotImplementedException ("Unknown express type operator " + Oper.ToString ());
			}
		}

		static bool IsFloat (Type t)
		{
			return t == TypeManager.float_type || t == TypeManager.double_type;
		}

		//
		// Returns a stringified representation of the Operator
		//
		public static string OperName (Operator oper)
		{
			switch (oper) {
			case Operator.UnaryPlus:
				return "+";
			case Operator.UnaryNegation:
				return "-";
			case Operator.LogicalNot:
				return "!";
			case Operator.OnesComplement:
				return "~";
			case Operator.AddressOf:
				return "&";
			}

			throw new NotImplementedException (oper.ToString ());
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			type = storey.MutateType (type);
			Expr.MutateHoistedGenericType (storey);
		}

		Expression ResolveAddressOf (ResolveContext ec)
		{
			if (!ec.IsUnsafe)
				UnsafeError (ec, loc);

			Expr = Expr.DoResolveLValue (ec, EmptyExpression.UnaryAddress);
			if (Expr == null || Expr.eclass != ExprClass.Variable) {
				ec.Report.Error (211, loc, "Cannot take the address of the given expression");
				return null;
			}

			if (!TypeManager.VerifyUnManaged (Expr.Type, loc)) {
				return null;
			}

			IVariableReference vr = Expr as IVariableReference;
			bool is_fixed;
			if (vr != null) {
				VariableInfo vi = vr.VariableInfo;
				if (vi != null) {
					if (vi.LocalInfo != null)
						vi.LocalInfo.Used = true;

					//
					// A variable is considered definitely assigned if you take its address.
					//
					vi.SetAssigned (ec);
				}

				is_fixed = vr.IsFixed;
				vr.SetHasAddressTaken ();

				if (vr.IsHoisted) {
					AnonymousMethodExpression.Error_AddressOfCapturedVar (ec, vr, loc);
				}
			} else {
				IFixedExpression fe = Expr as IFixedExpression;
				is_fixed = fe != null && fe.IsFixed;
			}

			if (!is_fixed && !ec.HasSet (ResolveContext.Options.FixedInitializerScope)) {
				ec.Report.Error (212, loc, "You can only take the address of unfixed expression inside of a fixed statement initializer");
			}

			type = TypeManager.GetPointerType (Expr.Type);
			eclass = ExprClass.Value;
			return this;
		}

		Expression ResolvePrimitivePredefinedType (Expression expr)
		{
			expr = DoNumericPromotion (Oper, expr);
			Type expr_type = expr.Type;
			Type[] predefined = predefined_operators [(int) Oper];
			foreach (Type t in predefined) {
				if (t == expr_type)
					return expr;
			}
			return null;
		}

		//
		// Perform user-operator overload resolution
		//
		protected virtual Expression ResolveUserOperator (ResolveContext ec, Expression expr)
		{
			CSharp.Operator.OpType op_type;
			switch (Oper) {
			case Operator.LogicalNot:
				op_type = CSharp.Operator.OpType.LogicalNot; break;
			case Operator.OnesComplement:
				op_type = CSharp.Operator.OpType.OnesComplement; break;
			case Operator.UnaryNegation:
				op_type = CSharp.Operator.OpType.UnaryNegation; break;
			case Operator.UnaryPlus:
				op_type = CSharp.Operator.OpType.UnaryPlus; break;
			default:
				throw new InternalErrorException (Oper.ToString ());
			}

			string op_name = CSharp.Operator.GetMetadataName (op_type);
			MethodGroupExpr user_op = MemberLookup (ec.Compiler, ec.CurrentType, expr.Type, op_name, MemberTypes.Method, AllBindingFlags, expr.Location) as MethodGroupExpr;
			if (user_op == null)
				return null;

			Arguments args = new Arguments (1);
			args.Add (new Argument (expr));
			user_op = user_op.OverloadResolve (ec, ref args, false, expr.Location);

			if (user_op == null)
				return null;

			Expr = args [0].Expr;
			return new UserOperatorCall (user_op, args, CreateExpressionTree, expr.Location);
		}

		//
		// Unary user type overload resolution
		//
		Expression ResolveUserType (ResolveContext ec, Expression expr)
		{
			Expression best_expr = ResolveUserOperator (ec, expr);
			if (best_expr != null)
				return best_expr;

			Type[] predefined = predefined_operators [(int) Oper];
			foreach (Type t in predefined) {
				Expression oper_expr = Convert.UserDefinedConversion (ec, expr, t, expr.Location, false, false);
				if (oper_expr == null)
					continue;

				//
				// decimal type is predefined but has user-operators
				//
				if (oper_expr.Type == TypeManager.decimal_type)
					oper_expr = ResolveUserType (ec, oper_expr);
				else
					oper_expr = ResolvePrimitivePredefinedType (oper_expr);

				if (oper_expr == null)
					continue;

				if (best_expr == null) {
					best_expr = oper_expr;
					continue;
				}

				int result = MethodGroupExpr.BetterTypeConversion (ec, best_expr.Type, t);
				if (result == 0) {
					ec.Report.Error (35, loc, "Operator `{0}' is ambiguous on an operand of type `{1}'",
						OperName (Oper), TypeManager.CSharpName (expr.Type));
					break;
				}

				if (result == 2)
					best_expr = oper_expr;
			}
			
			if (best_expr == null)
				return null;
			
			//
			// HACK: Decimal user-operator is included in standard operators
			//
			if (best_expr.Type == TypeManager.decimal_type)
				return best_expr;			

			Expr = best_expr;
			type = best_expr.Type;
			return this;			
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			Unary target = (Unary) t;

			target.Expr = Expr.Clone (clonectx);
		}
	}

	//
	// Unary operators are turned into Indirection expressions
	// after semantic analysis (this is so we can take the address
	// of an indirection).
	//
	public class Indirection : Expression, IMemoryLocation, IAssignMethod, IFixedExpression {
		Expression expr;
		LocalTemporary temporary;
		bool prepared;
		
		public Indirection (Expression expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Error_PointerInsideExpressionTree (ec);
			return null;
		}
		
		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			Indirection target = (Indirection) t;
			target.expr = expr.Clone (clonectx);
		}		
		
		public override void Emit (EmitContext ec)
		{
			if (!prepared)
				expr.Emit (ec);
			
			LoadFromPtr (ec.ig, Type);
		}

		public void Emit (EmitContext ec, bool leave_copy)
		{
			Emit (ec);
			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				temporary = new LocalTemporary (expr.Type);
				temporary.Store (ec);
			}
		}
		
		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			prepared = prepare_for_load;
			
			expr.Emit (ec);

			if (prepare_for_load)
				ec.ig.Emit (OpCodes.Dup);
			
			source.Emit (ec);
			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				temporary = new LocalTemporary (expr.Type);
				temporary.Store (ec);
			}
			
			StoreFromPtr (ec.ig, type);
			
			if (temporary != null) {
				temporary.Emit (ec);
				temporary.Release (ec);
			}
		}
		
		public void AddressOf (EmitContext ec, AddressOp Mode)
		{
			expr.Emit (ec);
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			return DoResolve (ec);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return null;

			if (!ec.IsUnsafe)
				UnsafeError (ec, loc);

			if (!expr.Type.IsPointer) {
				ec.Report.Error (193, loc, "The * or -> operator must be applied to a pointer");
				return null;
			}

			if (expr.Type == TypeManager.void_ptr_type) {
				ec.Report.Error (242, loc, "The operation in question is undefined on void pointers");
				return null;
			}

			type = TypeManager.GetElementType (expr.Type);
			eclass = ExprClass.Variable;
			return this;
		}

		public bool IsFixed {
			get { return true; }
		}

		public override string ToString ()
		{
			return "*(" + expr + ")";
		}
	}
	
	/// <summary>
	///   Unary Mutator expressions (pre and post ++ and --)
	/// </summary>
	///
	/// <remarks>
	///   UnaryMutator implements ++ and -- expressions.   It derives from
	///   ExpressionStatement becuase the pre/post increment/decrement
	///   operators can be used in a statement context.
	///
	/// FIXME: Idea, we could split this up in two classes, one simpler
	/// for the common case, and one with the extra fields for more complex
	/// classes (indexers require temporary access;  overloaded require method)
	///
	/// </remarks>
	public class UnaryMutator : ExpressionStatement {
		[Flags]
		public enum Mode : byte {
			IsIncrement    = 0,
			IsDecrement    = 1,
			IsPre          = 0,
			IsPost         = 2,
			
			PreIncrement   = 0,
			PreDecrement   = IsDecrement,
			PostIncrement  = IsPost,
			PostDecrement  = IsPost | IsDecrement
		}

		Mode mode;
		bool is_expr = false;
		bool recurse = false;

		Expression expr;

		//
		// This is expensive for the simplest case.
		//
		UserOperatorCall method;

		public UnaryMutator (Mode m, Expression e)
		{
			mode = m;
			loc = e.Location;
			expr = e;
		}

		static string OperName (Mode mode)
		{
			return (mode == Mode.PreIncrement || mode == Mode.PostIncrement) ?
				"++" : "--";
		}

		/// <summary>
		///   Returns whether an object of type `t' can be incremented
		///   or decremented with add/sub (ie, basically whether we can
		///   use pre-post incr-decr operations on it, but it is not a
		///   System.Decimal, which we require operator overloading to catch)
		/// </summary>
		static bool IsIncrementableNumber (Type t)
		{
			return (t == TypeManager.sbyte_type) ||
				(t == TypeManager.byte_type) ||
				(t == TypeManager.short_type) ||
				(t == TypeManager.ushort_type) ||
				(t == TypeManager.int32_type) ||
				(t == TypeManager.uint32_type) ||
				(t == TypeManager.int64_type) ||
				(t == TypeManager.uint64_type) ||
				(t == TypeManager.char_type) ||
				(TypeManager.IsSubclassOf (t, TypeManager.enum_type)) ||
				(t == TypeManager.float_type) ||
				(t == TypeManager.double_type) ||
				(t.IsPointer && t != TypeManager.void_ptr_type);
		}

		Expression ResolveOperator (ResolveContext ec)
		{
			type = expr.Type;
			
			//
			// The operand of the prefix/postfix increment decrement operators
			// should be an expression that is classified as a variable,
			// a property access or an indexer access
			//
			if (expr.eclass == ExprClass.Variable || expr.eclass == ExprClass.IndexerAccess || expr.eclass == ExprClass.PropertyAccess) {
				expr = expr.ResolveLValue (ec, expr);
			} else {
				ec.Report.Error (1059, loc, "The operand of an increment or decrement operator must be a variable, property or indexer");
			}

			//
			// Step 1: Perform Operator Overload location
			//
			MethodGroupExpr mg;
			string op_name;
			
			if (mode == Mode.PreIncrement || mode == Mode.PostIncrement)
				op_name = Operator.GetMetadataName (Operator.OpType.Increment);
			else
				op_name = Operator.GetMetadataName (Operator.OpType.Decrement);

			mg = MemberLookup (ec.Compiler, ec.CurrentType, type, op_name, MemberTypes.Method, AllBindingFlags, loc) as MethodGroupExpr;

			if (mg != null) {
				Arguments args = new Arguments (1);
				args.Add (new Argument (expr));
				mg = mg.OverloadResolve (ec, ref args, false, loc);
				if (mg == null)
					return null;

				method = new UserOperatorCall (mg, args, null, loc);
				Convert.ImplicitConversionRequired (ec, method, type, loc);
				return this;
			}

			if (!IsIncrementableNumber (type)) {
				ec.Report.Error (187, loc, "No such operator '" + OperName (mode) + "' defined for type '" +
					   TypeManager.CSharpName (type) + "'");
				return null;
			}

			return this;
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

			if (TypeManager.IsDynamicType (expr.Type)) {
				Arguments args = new Arguments (1);
				args.Add (new Argument (expr));
				return new DynamicUnaryConversion (GetOperatorExpressionTypeName (), args, loc).Resolve (ec);
			}

			eclass = ExprClass.Value;

			if (TypeManager.IsNullableType (expr.Type))
				return new Nullable.LiftedUnaryMutator (mode, expr, loc).Resolve (ec);

			return ResolveOperator (ec);
		}

		//
		// Loads the proper "1" into the stack based on the type, then it emits the
		// opcode for the operation requested
		//
		void LoadOneAndEmitOp (EmitContext ec, Type t)
		{
			//
			// Measure if getting the typecode and using that is more/less efficient
			// that comparing types.  t.GetTypeCode() is an internal call.
			//
			ILGenerator ig = ec.ig;
						     
			if (t == TypeManager.uint64_type || t == TypeManager.int64_type)
				LongConstant.EmitLong (ig, 1);
			else if (t == TypeManager.double_type)
				ig.Emit (OpCodes.Ldc_R8, 1.0);
			else if (t == TypeManager.float_type)
				ig.Emit (OpCodes.Ldc_R4, 1.0F);
			else if (t.IsPointer){
				Type et = TypeManager.GetElementType (t);
				int n = GetTypeSize (et);
				
				if (n == 0)
					ig.Emit (OpCodes.Sizeof, et);
				else {
					IntConstant.EmitInt (ig, n);
					ig.Emit (OpCodes.Conv_I);
				}
			} else 
				ig.Emit (OpCodes.Ldc_I4_1);

			//
			// Now emit the operation
			//

			Binary.Operator op = (mode & Mode.IsDecrement) != 0 ? Binary.Operator.Subtraction : Binary.Operator.Addition;
			Binary.EmitOperatorOpcode (ec, op, t);

			if (t == TypeManager.sbyte_type){
				if (ec.HasSet (EmitContext.Options.CheckedScope))
					ig.Emit (OpCodes.Conv_Ovf_I1);
				else
					ig.Emit (OpCodes.Conv_I1);
			} else if (t == TypeManager.byte_type){
				if (ec.HasSet (EmitContext.Options.CheckedScope))
					ig.Emit (OpCodes.Conv_Ovf_U1);
				else
					ig.Emit (OpCodes.Conv_U1);
			} else if (t == TypeManager.short_type){
				if (ec.HasSet (EmitContext.Options.CheckedScope))
					ig.Emit (OpCodes.Conv_Ovf_I2);
				else
					ig.Emit (OpCodes.Conv_I2);
			} else if (t == TypeManager.ushort_type || t == TypeManager.char_type){
				if (ec.HasSet (EmitContext.Options.CheckedScope))
					ig.Emit (OpCodes.Conv_Ovf_U2);
				else
					ig.Emit (OpCodes.Conv_U2);
			}
			
		}

		void EmitCode (EmitContext ec, bool is_expr)
		{
			recurse = true;
			this.is_expr = is_expr;
			((IAssignMethod) expr).EmitAssign (ec, this, is_expr && (mode == Mode.PreIncrement || mode == Mode.PreDecrement), true);
		}

		public override void Emit (EmitContext ec)
		{
			//
			// We use recurse to allow ourselfs to be the source
			// of an assignment. This little hack prevents us from
			// having to allocate another expression
			//
			if (recurse) {
				((IAssignMethod) expr).Emit (ec, is_expr && (mode == Mode.PostIncrement || mode == Mode.PostDecrement));
				if (method == null)
					LoadOneAndEmitOp (ec, expr.Type);
				else
					ec.ig.Emit (OpCodes.Call, (MethodInfo)method.Method);
				recurse = false;
				return;
			}

			EmitCode (ec, true);
		}

		public override void EmitStatement (EmitContext ec)
		{
			EmitCode (ec, false);
		}

		//
		// Converts operator to System.Linq.Expressions.ExpressionType enum name
		//
		string GetOperatorExpressionTypeName ()
		{
			if ((mode & Mode.IsDecrement) != 0)
				return "Decrement";

			return "Increment";
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			UnaryMutator target = (UnaryMutator) t;

			target.expr = expr.Clone (clonectx);
		}
	}

	/// <summary>
	///   Base class for the `Is' and `As' classes. 
	/// </summary>
	///
	/// <remarks>
	///   FIXME: Split this in two, and we get to save the `Operator' Oper
	///   size. 
	/// </remarks>
	public abstract class Probe : Expression {
		public Expression ProbeType;
		protected Expression expr;
		protected TypeExpr probe_type_expr;
		
		public Probe (Expression expr, Expression probe_type, Location l)
		{
			ProbeType = probe_type;
			loc = l;
			this.expr = expr;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			probe_type_expr = ProbeType.ResolveAsTypeTerminal (ec, false);
			if (probe_type_expr == null)
				return null;

			expr = expr.Resolve (ec);
			if (expr == null)
				return null;

			if ((probe_type_expr.Type.Attributes & Class.StaticClassAttribute) == Class.StaticClassAttribute) {
				ec.Report.Error (-244, loc, "The `{0}' operator cannot be applied to an operand of a static type",
					OperatorName);
			}
			
			if (expr.Type.IsPointer || probe_type_expr.Type.IsPointer) {
				ec.Report.Error (244, loc, "The `{0}' operator cannot be applied to an operand of pointer type",
					OperatorName);
				return null;
			}

			if (expr.Type == InternalType.AnonymousMethod) {
				ec.Report.Error (837, loc, "The `{0}' operator cannot be applied to a lambda expression or anonymous method",
					OperatorName);
				return null;
			}

			return this;
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			expr.MutateHoistedGenericType (storey);
			probe_type_expr.MutateHoistedGenericType (storey);
		}

		protected abstract string OperatorName { get; }

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			Probe target = (Probe) t;

			target.expr = expr.Clone (clonectx);
			target.ProbeType = ProbeType.Clone (clonectx);
		}

	}

	/// <summary>
	///   Implementation of the `is' operator.
	/// </summary>
	public class Is : Probe {
		Nullable.Unwrap expr_unwrap;

		public Is (Expression expr, Expression probe_type, Location l)
			: base (expr, probe_type, l)
		{
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree (ec, null,
				expr.CreateExpressionTree (ec),
				new TypeOf (probe_type_expr, loc));

			return CreateExpressionFactoryCall (ec, "TypeIs", args);
		}
		
		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			if (expr_unwrap != null) {
				expr_unwrap.EmitCheck (ec);
				return;
			}

			expr.Emit (ec);
			ig.Emit (OpCodes.Isinst, probe_type_expr.Type);
			ig.Emit (OpCodes.Ldnull);
			ig.Emit (OpCodes.Cgt_Un);
		}

		public override void EmitBranchable (EmitContext ec, Label target, bool on_true)
		{
			ILGenerator ig = ec.ig;
			if (expr_unwrap != null) {
				expr_unwrap.EmitCheck (ec);
			} else {
				expr.Emit (ec);
				ig.Emit (OpCodes.Isinst, probe_type_expr.Type);
			}			
			ig.Emit (on_true ? OpCodes.Brtrue : OpCodes.Brfalse, target);
		}
		
		Expression CreateConstantResult (ResolveContext ec, bool result)
		{
			if (result)
				ec.Report.Warning (183, 1, loc, "The given expression is always of the provided (`{0}') type",
					TypeManager.CSharpName (probe_type_expr.Type));
			else
				ec.Report.Warning (184, 1, loc, "The given expression is never of the provided (`{0}') type",
					TypeManager.CSharpName (probe_type_expr.Type));

			return ReducedExpression.Create (new BoolConstant (result, loc), this);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (base.DoResolve (ec) == null)
				return null;

			Type d = expr.Type;
			bool d_is_nullable = false;

			//
			// If E is a method group or the null literal, or if the type of E is a reference
			// type or a nullable type and the value of E is null, the result is false
			//
			if (expr.IsNull || expr.eclass == ExprClass.MethodGroup)
				return CreateConstantResult (ec, false);

			if (TypeManager.IsNullableType (d) && !TypeManager.ContainsGenericParameters (d)) {
				d = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (d) [0]);
				d_is_nullable = true;
			}

			type = TypeManager.bool_type;
			eclass = ExprClass.Value;
			Type t = probe_type_expr.Type;
			bool t_is_nullable = false;
			if (TypeManager.IsNullableType (t) && !TypeManager.ContainsGenericParameters (t)) {
				t = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (t) [0]);
				t_is_nullable = true;
			}

			if (TypeManager.IsStruct (t)) {
				if (d == t) {
					//
					// D and T are the same value types but D can be null
					//
					if (d_is_nullable && !t_is_nullable) {
						expr_unwrap = Nullable.Unwrap.Create (expr, false);
						return this;
					}
					
					//
					// The result is true if D and T are the same value types
					//
					return CreateConstantResult (ec, true);
				}

				if (TypeManager.IsGenericParameter (d))
					return ResolveGenericParameter (ec, t, d);

				//
				// An unboxing conversion exists
				//
				if (Convert.ExplicitReferenceConversionExists (d, t))
					return this;
			} else {
				if (TypeManager.IsGenericParameter (t))
					return ResolveGenericParameter (ec, d, t);

				if (TypeManager.IsStruct (d)) {
					bool temp;
					if (Convert.ImplicitBoxingConversionExists (expr, t, out temp))
						return CreateConstantResult (ec, true);
				} else {
					if (TypeManager.IsGenericParameter (d))
						return ResolveGenericParameter (ec, t, d);

					if (TypeManager.ContainsGenericParameters (d))
						return this;

					if (Convert.ImplicitReferenceConversionExists (expr, t) ||
						Convert.ExplicitReferenceConversionExists (d, t)) {
						return this;
					}
				}
			}

			return CreateConstantResult (ec, false);
		}

		Expression ResolveGenericParameter (ResolveContext ec, Type d, Type t)
		{
			GenericConstraints constraints = TypeManager.GetTypeParameterConstraints (t);
			if (constraints != null) {
				if (constraints.IsReferenceType && TypeManager.IsStruct (d))
					return CreateConstantResult (ec, false);
			}

			if (TypeManager.IsGenericParameter (expr.Type)) {
				if (constraints != null && constraints.IsValueType && expr.Type == t)
					return CreateConstantResult (ec, true);

				expr = new BoxedCast (expr, d);
			}

			return this;
		}
		
		protected override string OperatorName {
			get { return "is"; }
		}
	}

	/// <summary>
	///   Implementation of the `as' operator.
	/// </summary>
	public class As : Probe {
		bool do_isinst;
		Expression resolved_type;
		
		public As (Expression expr, Expression probe_type, Location l)
			: base (expr, probe_type, l)
		{
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree (ec, null,
				expr.CreateExpressionTree (ec),
				new TypeOf (probe_type_expr, loc));

			return CreateExpressionFactoryCall (ec, "TypeAs", args);
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			expr.Emit (ec);

			if (do_isinst)
				ig.Emit (OpCodes.Isinst, type);

#if GMCS_SOURCE
			if (TypeManager.IsGenericParameter (type) || TypeManager.IsNullableType (type))
				ig.Emit (OpCodes.Unbox_Any, type);
#endif
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			// Because expr is modified
			if (eclass != ExprClass.Invalid)
				return this;

			if (resolved_type == null) {
				resolved_type = base.DoResolve (ec);

				if (resolved_type == null)
					return null;
			}

			type = probe_type_expr.Type;
			eclass = ExprClass.Value;
			Type etype = expr.Type;

			if (!TypeManager.IsReferenceType (type) && !TypeManager.IsNullableType (type)) {
				if (TypeManager.IsGenericParameter (type)) {
					ec.Report.Error (413, loc,
						"The `as' operator cannot be used with a non-reference type parameter `{0}'. Consider adding `class' or a reference type constraint",
						probe_type_expr.GetSignatureForError ());
				} else {
					ec.Report.Error (77, loc,
						"The `as' operator cannot be used with a non-nullable value type `{0}'",
						TypeManager.CSharpName (type));
				}
				return null;
			}

			if (expr.IsNull && TypeManager.IsNullableType (type)) {
				return Nullable.LiftedNull.CreateFromExpression (ec, this);
			}
			
			Expression e = Convert.ImplicitConversion (ec, expr, type, loc);
			if (e != null){
				expr = e;
				do_isinst = false;
				return this;
			}

			if (Convert.ExplicitReferenceConversionExists (etype, type)){
				if (TypeManager.IsGenericParameter (etype))
					expr = new BoxedCast (expr, etype);

				do_isinst = true;
				return this;
			}

			if (TypeManager.ContainsGenericParameters (etype) ||
			    TypeManager.ContainsGenericParameters (type)) {
				expr = new BoxedCast (expr, etype);
				do_isinst = true;
				return this;
			}

			ec.Report.Error (39, loc, "Cannot convert type `{0}' to `{1}' via a built-in conversion",
				TypeManager.CSharpName (etype), TypeManager.CSharpName (type));

			return null;
		}

		protected override string OperatorName {
			get { return "as"; }
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			type = storey.MutateType (type);
			base.MutateHoistedGenericType (storey);
		}
	
		public override bool GetAttributableValue (ResolveContext ec, Type value_type, out object value)
		{
			return expr.GetAttributableValue (ec, value_type, out value);
		}
	}
	
	/// <summary>
	///   This represents a typecast in the source language.
	///
	///   FIXME: Cast expressions have an unusual set of parsing
	///   rules, we need to figure those out.
	/// </summary>
	public class Cast : Expression {
		Expression target_type;
		Expression expr;
			
		public Cast (Expression cast_type, Expression expr)
			: this (cast_type, expr, cast_type.Location)
		{
		}

		public Cast (Expression cast_type, Expression expr, Location loc)
		{
			this.target_type = cast_type;
			this.expr = expr;
			this.loc = loc;
		}

		public Expression TargetType {
			get { return target_type; }
		}

		public Expression Expr {
			get { return expr; }
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return null;

			TypeExpr target = target_type.ResolveAsTypeTerminal (ec, false);
			if (target == null)
				return null;

			type = target.Type;

			if (type.IsAbstract && type.IsSealed) {
				ec.Report.Error (716, loc, "Cannot convert to static type `{0}'", TypeManager.CSharpName (type));
				return null;
			}

			eclass = ExprClass.Value;

			Constant c = expr as Constant;
			if (c != null) {
				c = c.TryReduce (ec, type, loc);
				if (c != null)
					return c;
			}

			if (type.IsPointer && !ec.IsUnsafe) {
				UnsafeError (ec, loc);
			} else if (TypeManager.IsDynamicType (expr.Type)) {
				Arguments arg = new Arguments (1);
				arg.Add (new Argument (expr));
				return new DynamicConversion (type, true, arg, loc).Resolve (ec);
			}

			expr = Convert.ExplicitConversion (ec, expr, type, loc);
			return expr;
		}
		
		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Should not happen");
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			Cast target = (Cast) t;

			target.target_type = target_type.Clone (clonectx);
			target.expr = expr.Clone (clonectx);
		}
	}
	
	//
	// C# 2.0 Default value expression
	//
	public class DefaultValueExpression : Expression
	{
		sealed class DefaultValueNullLiteral : NullLiteral
		{
			public DefaultValueNullLiteral (DefaultValueExpression expr)
				: base (expr.type, expr.loc)
			{
			}

			public override void Error_ValueCannotBeConverted (ResolveContext ec, Location loc, Type t, bool expl)
			{
				Error_ValueCannotBeConvertedCore (ec, loc, t, expl);
			}
		}


		Expression expr;

		public DefaultValueExpression (Expression expr, Location loc)
		{
			this.expr = expr;
			this.loc = loc;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (2);
			args.Add (new Argument (this));
			args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));
			return CreateExpressionFactoryCall (ec, "Constant", args);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			TypeExpr texpr = expr.ResolveAsTypeTerminal (ec, false);
			if (texpr == null)
				return null;

			type = texpr.Type;

			if ((type.Attributes & Class.StaticClassAttribute) == Class.StaticClassAttribute) {
				ec.Report.Error (-244, loc, "The `default value' operator cannot be applied to an operand of a static type");
			}

			if (type.IsPointer)
				return new NullLiteral (Location).ConvertImplicitly (type);

			if (TypeManager.IsReferenceType (type))
				return new DefaultValueNullLiteral (this);

			Constant c = New.Constantify (type);
			if (c != null)
				return c;

			eclass = ExprClass.Variable;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			LocalTemporary temp_storage = new LocalTemporary(type);

			temp_storage.AddressOf(ec, AddressOp.LoadStore);
			ec.ig.Emit(OpCodes.Initobj, type);
			temp_storage.Emit(ec);
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			type = storey.MutateType (type);
		}
		
		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			DefaultValueExpression target = (DefaultValueExpression) t;
			
			target.expr = expr.Clone (clonectx);
		}
	}

	/// <summary>
	///   Binary operators
	/// </summary>
	public class Binary : Expression, IDynamicBinder
	{

		protected class PredefinedOperator {
			protected readonly Type left;
			protected readonly Type right;
			public readonly Operator OperatorsMask;
			public Type ReturnType;

			public PredefinedOperator (Type ltype, Type rtype, Operator op_mask)
				: this (ltype, rtype, op_mask, ltype)
			{
			}

			public PredefinedOperator (Type type, Operator op_mask, Type return_type)
				: this (type, type, op_mask, return_type)
			{
			}

			public PredefinedOperator (Type type, Operator op_mask)
				: this (type, type, op_mask, type)
			{
			}

			public PredefinedOperator (Type ltype, Type rtype, Operator op_mask, Type return_type)
			{
				if ((op_mask & Operator.ValuesOnlyMask) != 0)
					throw new InternalErrorException ("Only masked values can be used");

				this.left = ltype;
				this.right = rtype;
				this.OperatorsMask = op_mask;
				this.ReturnType = return_type;
			}

			public virtual Expression ConvertResult (ResolveContext ec, Binary b)
			{
				b.type = ReturnType;

				b.left = Convert.ImplicitConversion (ec, b.left, left, b.left.Location);
				b.right = Convert.ImplicitConversion (ec, b.right, right, b.right.Location);

				//
				// A user operators does not support multiple user conversions, but decimal type
				// is considered to be predefined type therefore we apply predefined operators rules
				// and then look for decimal user-operator implementation
				//
				if (left == TypeManager.decimal_type)
					return b.ResolveUserOperator (ec, b.left.Type, b.right.Type);

				return b;
			}

			public bool IsPrimitiveApplicable (Type ltype, Type rtype)
			{
				//
				// We are dealing with primitive types only
				//
				return left == ltype && ltype == rtype;
			}

			public virtual bool IsApplicable (ResolveContext ec, Expression lexpr, Expression rexpr)
			{
				if (TypeManager.IsEqual (left, lexpr.Type) &&
					TypeManager.IsEqual (right, rexpr.Type))
					return true;

				return Convert.ImplicitConversionExists (ec, lexpr, left) &&
					Convert.ImplicitConversionExists (ec, rexpr, right);
			}

			public PredefinedOperator ResolveBetterOperator (ResolveContext ec, PredefinedOperator best_operator)
			{
				int result = 0;
				if (left != null && best_operator.left != null) {
					result = MethodGroupExpr.BetterTypeConversion (ec, best_operator.left, left);
				}

				//
				// When second arguments are same as the first one, the result is same
				//
				if (right != null && (left != right || best_operator.left != best_operator.right)) {
					result |= MethodGroupExpr.BetterTypeConversion (ec, best_operator.right, right);
				}

				if (result == 0 || result > 2)
					return null;

				return result == 1 ? best_operator : this;
			}
		}

		class PredefinedStringOperator : PredefinedOperator {
			public PredefinedStringOperator (Type type, Operator op_mask)
				: base (type, op_mask, type)
			{
				ReturnType = TypeManager.string_type;
			}

			public PredefinedStringOperator (Type ltype, Type rtype, Operator op_mask)
				: base (ltype, rtype, op_mask)
			{
				ReturnType = TypeManager.string_type;
			}

			public override Expression ConvertResult (ResolveContext ec, Binary b)
			{
				//
				// Use original expression for nullable arguments
				//
				Nullable.Unwrap unwrap = b.left as Nullable.Unwrap;
				if (unwrap != null)
					b.left = unwrap.Original;

				unwrap = b.right as Nullable.Unwrap;
				if (unwrap != null)
					b.right = unwrap.Original;

				b.left = Convert.ImplicitConversion (ec, b.left, left, b.left.Location);
				b.right = Convert.ImplicitConversion (ec, b.right, right, b.right.Location);

				//
				// Start a new concat expression using converted expression
				//
				return new StringConcat (b.loc, b.left, b.right).Resolve (ec);
			}
		}

		class PredefinedShiftOperator : PredefinedOperator {
			public PredefinedShiftOperator (Type ltype, Operator op_mask) :
				base (ltype, TypeManager.int32_type, op_mask)
			{
			}

			public override Expression ConvertResult (ResolveContext ec, Binary b)
			{
				b.left = Convert.ImplicitConversion (ec, b.left, left, b.left.Location);

				Expression expr_tree_expr = EmptyCast.Create (b.right, TypeManager.int32_type);

				int right_mask = left == TypeManager.int32_type || left == TypeManager.uint32_type ? 0x1f : 0x3f;

				//
				// b = b.left >> b.right & (0x1f|0x3f)
				//
				b.right = new Binary (Operator.BitwiseAnd,
					b.right, new IntConstant (right_mask, b.right.Location)).Resolve (ec);

				//
				// Expression tree representation does not use & mask
				//
				b.right = ReducedExpression.Create (b.right, expr_tree_expr).Resolve (ec);
				b.type = ReturnType;
				return b;
			}
		}

		class PredefinedPointerOperator : PredefinedOperator {
			public PredefinedPointerOperator (Type ltype, Type rtype, Operator op_mask)
				: base (ltype, rtype, op_mask)
			{
			}

			public PredefinedPointerOperator (Type ltype, Type rtype, Operator op_mask, Type retType)
				: base (ltype, rtype, op_mask, retType)
			{
			}

			public PredefinedPointerOperator (Type type, Operator op_mask, Type return_type)
				: base (type, op_mask, return_type)
			{
			}

			public override bool IsApplicable (ResolveContext ec, Expression lexpr, Expression rexpr)
			{
				if (left == null) {
					if (!lexpr.Type.IsPointer)
						return false;
				} else {
					if (!Convert.ImplicitConversionExists (ec, lexpr, left))
						return false;
				}

				if (right == null) {
					if (!rexpr.Type.IsPointer)
						return false;
				} else {
					if (!Convert.ImplicitConversionExists (ec, rexpr, right))
						return false;
				}

				return true;
			}

			public override Expression ConvertResult (ResolveContext ec, Binary b)
			{
				if (left != null) {
					b.left = EmptyCast.Create (b.left, left);
				} else if (right != null) {
					b.right = EmptyCast.Create (b.right, right);
				}

				Type r_type = ReturnType;
				Expression left_arg, right_arg;
				if (r_type == null) {
					if (left == null) {
						left_arg = b.left;
						right_arg = b.right;
						r_type = b.left.Type;
					} else {
						left_arg = b.right;
						right_arg = b.left;
						r_type = b.right.Type;
					}
				} else {
					left_arg = b.left;
					right_arg = b.right;
				}

				return new PointerArithmetic (b.oper, left_arg, right_arg, r_type, b.loc).Resolve (ec);
			}
		}

		[Flags]
		public enum Operator {
			Multiply	= 0 | ArithmeticMask,
			Division	= 1 | ArithmeticMask,
			Modulus		= 2 | ArithmeticMask,
			Addition	= 3 | ArithmeticMask | AdditionMask,
			Subtraction = 4 | ArithmeticMask | SubtractionMask,

			LeftShift	= 5 | ShiftMask,
			RightShift	= 6 | ShiftMask,

			LessThan	= 7 | ComparisonMask | RelationalMask,
			GreaterThan	= 8 | ComparisonMask | RelationalMask,
			LessThanOrEqual		= 9 | ComparisonMask | RelationalMask,
			GreaterThanOrEqual	= 10 | ComparisonMask | RelationalMask,
			Equality	= 11 | ComparisonMask | EqualityMask,
			Inequality	= 12 | ComparisonMask | EqualityMask,

			BitwiseAnd	= 13 | BitwiseMask,
			ExclusiveOr	= 14 | BitwiseMask,
			BitwiseOr	= 15 | BitwiseMask,

			LogicalAnd	= 16 | LogicalMask,
			LogicalOr	= 17 | LogicalMask,

			//
			// Operator masks
			//
			ValuesOnlyMask	= ArithmeticMask - 1,
			ArithmeticMask	= 1 << 5,
			ShiftMask		= 1 << 6,
			ComparisonMask	= 1 << 7,
			EqualityMask	= 1 << 8,
			BitwiseMask		= 1 << 9,
			LogicalMask		= 1 << 10,
			AdditionMask	= 1 << 11,
			SubtractionMask	= 1 << 12,
			RelationalMask	= 1 << 13
		}

		readonly Operator oper;
		protected Expression left, right;
		readonly bool is_compound;
		Expression enum_conversion;

		static PredefinedOperator [] standard_operators;
		static PredefinedOperator [] pointer_operators;
		
		public Binary (Operator oper, Expression left, Expression right, bool isCompound)
			: this (oper, left, right)
		{
			this.is_compound = isCompound;
		}

		public Binary (Operator oper, Expression left, Expression right)
		{
			this.oper = oper;
			this.left = left;
			this.right = right;
			this.loc = left.Location;
		}

		public Operator Oper {
			get {
				return oper;
			}
		}
		
		/// <summary>
		///   Returns a stringified representation of the Operator
		/// </summary>
		string OperName (Operator oper)
		{
			string s;
			switch (oper){
			case Operator.Multiply:
				s = "*";
				break;
			case Operator.Division:
				s = "/";
				break;
			case Operator.Modulus:
				s = "%";
				break;
			case Operator.Addition:
				s = "+";
				break;
			case Operator.Subtraction:
				s = "-";
				break;
			case Operator.LeftShift:
				s = "<<";
				break;
			case Operator.RightShift:
				s = ">>";
				break;
			case Operator.LessThan:
				s = "<";
				break;
			case Operator.GreaterThan:
				s = ">";
				break;
			case Operator.LessThanOrEqual:
				s = "<=";
				break;
			case Operator.GreaterThanOrEqual:
				s = ">=";
				break;
			case Operator.Equality:
				s = "==";
				break;
			case Operator.Inequality:
				s = "!=";
				break;
			case Operator.BitwiseAnd:
				s = "&";
				break;
			case Operator.BitwiseOr:
				s = "|";
				break;
			case Operator.ExclusiveOr:
				s = "^";
				break;
			case Operator.LogicalOr:
				s = "||";
				break;
			case Operator.LogicalAnd:
				s = "&&";
				break;
			default:
				s = oper.ToString ();
				break;
			}

			if (is_compound)
				return s + "=";

			return s;
		}

		public static void Error_OperatorCannotBeApplied (ResolveContext ec, Expression left, Expression right, Operator oper, Location loc)
		{
			new Binary (oper, left, right).Error_OperatorCannotBeApplied (ec, left, right);
		}

		public static void Error_OperatorCannotBeApplied (ResolveContext ec, Expression left, Expression right, string oper, Location loc)
		{
			string l, r;
			l = TypeManager.CSharpName (left.Type);
			r = TypeManager.CSharpName (right.Type);

			ec.Report.Error (19, loc, "Operator `{0}' cannot be applied to operands of type `{1}' and `{2}'",
				oper, l, r);
		}
		
		protected void Error_OperatorCannotBeApplied (ResolveContext ec, Expression left, Expression right)
		{
			Error_OperatorCannotBeApplied (ec, left, right, OperName (oper), loc);
		}

		//
		// Converts operator to System.Linq.Expressions.ExpressionType enum name
		//
		string GetOperatorExpressionTypeName ()
		{
			switch (oper) {
			case Operator.Addition:
				return is_compound ? "AddAssign" : "Add";
			case Operator.BitwiseAnd:
				return is_compound ? "AndAssign" : "And";
			case Operator.BitwiseOr:
				return is_compound ? "OrAssign" : "Or";
			case Operator.Division:
				return is_compound ? "DivideAssign" : "Divide";
			case Operator.ExclusiveOr:
				return is_compound ? "ExclusiveOrAssign" : "ExclusiveOr";
			case Operator.Equality:
				return "Equal";
			case Operator.GreaterThan:
				return "GreaterThan";
			case Operator.GreaterThanOrEqual:
				return "GreaterThanOrEqual";
			case Operator.Inequality:
				return "NotEqual";
			case Operator.LeftShift:
				return is_compound ? "LeftShiftAssign" : "LeftShift";
			case Operator.LessThan:
				return "LessThan";
			case Operator.LessThanOrEqual:
				return "LessThanOrEqual";
			case Operator.LogicalAnd:
				return "And";
			case Operator.LogicalOr:
				return "Or";
			case Operator.Modulus:
				return is_compound ? "ModuloAssign" : "Modulo";
			case Operator.Multiply:
				return is_compound ? "MultiplyAssign" : "Multiply";
			case Operator.RightShift:
				return is_compound ? "RightShiftAssign" : "RightShift";
			case Operator.Subtraction:
				return is_compound ? "SubtractAssign" : "Subtract";
			default:
				throw new NotImplementedException ("Unknown expression type operator " + oper.ToString ());
			}
		}

		static string GetOperatorMetadataName (Operator op)
		{
			CSharp.Operator.OpType op_type;
			switch (op) {
			case Operator.Addition:
				op_type = CSharp.Operator.OpType.Addition; break;
			case Operator.BitwiseAnd:
				op_type = CSharp.Operator.OpType.BitwiseAnd; break;
			case Operator.BitwiseOr:
				op_type = CSharp.Operator.OpType.BitwiseOr; break;
			case Operator.Division:
				op_type = CSharp.Operator.OpType.Division; break;
			case Operator.Equality:
				op_type = CSharp.Operator.OpType.Equality; break;
			case Operator.ExclusiveOr:
				op_type = CSharp.Operator.OpType.ExclusiveOr; break;
			case Operator.GreaterThan:
				op_type = CSharp.Operator.OpType.GreaterThan; break;
			case Operator.GreaterThanOrEqual:
				op_type = CSharp.Operator.OpType.GreaterThanOrEqual; break;
			case Operator.Inequality:
				op_type = CSharp.Operator.OpType.Inequality; break;
			case Operator.LeftShift:
				op_type = CSharp.Operator.OpType.LeftShift; break;
			case Operator.LessThan:
				op_type = CSharp.Operator.OpType.LessThan; break;
			case Operator.LessThanOrEqual:
				op_type = CSharp.Operator.OpType.LessThanOrEqual; break;
			case Operator.Modulus:
				op_type = CSharp.Operator.OpType.Modulus; break;
			case Operator.Multiply:
				op_type = CSharp.Operator.OpType.Multiply; break;
			case Operator.RightShift:
				op_type = CSharp.Operator.OpType.RightShift; break;
			case Operator.Subtraction:
				op_type = CSharp.Operator.OpType.Subtraction; break;
			default:
				throw new InternalErrorException (op.ToString ());
			}

			return CSharp.Operator.GetMetadataName (op_type);
		}

		public static void EmitOperatorOpcode (EmitContext ec, Operator oper, Type l)
		{
			OpCode opcode;
			ILGenerator ig = ec.ig;

			switch (oper){
			case Operator.Multiply:
				if (ec.HasSet (EmitContext.Options.CheckedScope)) {
					if (l == TypeManager.int32_type || l == TypeManager.int64_type)
						opcode = OpCodes.Mul_Ovf;
					else if (!IsFloat (l))
						opcode = OpCodes.Mul_Ovf_Un;
					else
						opcode = OpCodes.Mul;
				} else
					opcode = OpCodes.Mul;
				
				break;
				
			case Operator.Division:
				if (IsUnsigned (l))
					opcode = OpCodes.Div_Un;
				else
					opcode = OpCodes.Div;
				break;
				
			case Operator.Modulus:
				if (IsUnsigned (l))
					opcode = OpCodes.Rem_Un;
				else
					opcode = OpCodes.Rem;
				break;

			case Operator.Addition:
				if (ec.HasSet (EmitContext.Options.CheckedScope)) {
					if (l == TypeManager.int32_type || l == TypeManager.int64_type)
						opcode = OpCodes.Add_Ovf;
					else if (!IsFloat (l))
						opcode = OpCodes.Add_Ovf_Un;
					else
						opcode = OpCodes.Add;
				} else
					opcode = OpCodes.Add;
				break;

			case Operator.Subtraction:
				if (ec.HasSet (EmitContext.Options.CheckedScope)) {
					if (l == TypeManager.int32_type || l == TypeManager.int64_type)
						opcode = OpCodes.Sub_Ovf;
					else if (!IsFloat (l))
						opcode = OpCodes.Sub_Ovf_Un;
					else
						opcode = OpCodes.Sub;
				} else
					opcode = OpCodes.Sub;
				break;

			case Operator.RightShift:
				if (IsUnsigned (l))
					opcode = OpCodes.Shr_Un;
				else
					opcode = OpCodes.Shr;
				break;
				
			case Operator.LeftShift:
				opcode = OpCodes.Shl;
				break;

			case Operator.Equality:
				opcode = OpCodes.Ceq;
				break;

			case Operator.Inequality:
				ig.Emit (OpCodes.Ceq);
				ig.Emit (OpCodes.Ldc_I4_0);
				
				opcode = OpCodes.Ceq;
				break;

			case Operator.LessThan:
				if (IsUnsigned (l))
					opcode = OpCodes.Clt_Un;
				else
					opcode = OpCodes.Clt;
				break;

			case Operator.GreaterThan:
				if (IsUnsigned (l))
					opcode = OpCodes.Cgt_Un;
				else
					opcode = OpCodes.Cgt;
				break;

			case Operator.LessThanOrEqual:
				if (IsUnsigned (l) || IsFloat (l))
					ig.Emit (OpCodes.Cgt_Un);
				else
					ig.Emit (OpCodes.Cgt);
				ig.Emit (OpCodes.Ldc_I4_0);
				
				opcode = OpCodes.Ceq;
				break;

			case Operator.GreaterThanOrEqual:
				if (IsUnsigned (l) || IsFloat (l))
					ig.Emit (OpCodes.Clt_Un);
				else
					ig.Emit (OpCodes.Clt);
				
				ig.Emit (OpCodes.Ldc_I4_0);
				
				opcode = OpCodes.Ceq;
				break;

			case Operator.BitwiseOr:
				opcode = OpCodes.Or;
				break;

			case Operator.BitwiseAnd:
				opcode = OpCodes.And;
				break;

			case Operator.ExclusiveOr:
				opcode = OpCodes.Xor;
				break;

			default:
				throw new InternalErrorException (oper.ToString ());
			}

			ig.Emit (opcode);
		}

		static bool IsUnsigned (Type t)
		{
			if (t.IsPointer)
				return true;

			return (t == TypeManager.uint32_type || t == TypeManager.uint64_type ||
				t == TypeManager.ushort_type || t == TypeManager.byte_type);
		}

		static bool IsFloat (Type t)
		{
			return t == TypeManager.float_type || t == TypeManager.double_type;
		}

		Expression ResolveOperator (ResolveContext ec)
		{
			Type l = left.Type;
			Type r = right.Type;
			Expression expr;
			bool primitives_only = false;

			if (standard_operators == null)
				CreateStandardOperatorsTable ();

			//
			// Handles predefined primitive types
			//
			if (TypeManager.IsPrimitiveType (l) && TypeManager.IsPrimitiveType (r)) {
				if ((oper & Operator.ShiftMask) == 0) {
					if (l != TypeManager.bool_type && !DoBinaryOperatorPromotion (ec))
						return null;

					primitives_only = true;
				}
			} else {
				// Pointers
				if (l.IsPointer || r.IsPointer)
					return ResolveOperatorPointer (ec, l, r);

				// Enums
				bool lenum = TypeManager.IsEnumType (l);
				bool renum = TypeManager.IsEnumType (r);
				if (lenum || renum) {
					expr = ResolveOperatorEnum (ec, lenum, renum, l, r);

					// TODO: Can this be ambiguous
					if (expr != null)
						return expr;
				}

				// Delegates
				if ((oper == Operator.Addition || oper == Operator.Subtraction || (oper & Operator.EqualityMask) != 0) &&
					 (TypeManager.IsDelegateType (l) || TypeManager.IsDelegateType (r))) {
						
					expr = ResolveOperatorDelegate (ec, l, r);

					// TODO: Can this be ambiguous
					if (expr != null)
						return expr;
				}

				// User operators
				expr = ResolveUserOperator (ec, l, r);
				if (expr != null)
					return expr;

				// Predefined reference types equality
				if ((oper & Operator.EqualityMask) != 0) {
					expr = ResolveOperatorEqualityRerefence (ec, l, r);
					if (expr != null)
						return expr;
				}
			}

			return ResolveOperatorPredefined (ec, standard_operators, primitives_only, null);
		}

		// at least one of 'left' or 'right' is an enumeration constant (EnumConstant or SideEffectConstant or ...)
		// if 'left' is not an enumeration constant, create one from the type of 'right'
		Constant EnumLiftUp (ResolveContext ec, Constant left, Constant right, Location loc)
		{
			switch (oper) {
			case Operator.BitwiseOr:
			case Operator.BitwiseAnd:
			case Operator.ExclusiveOr:
			case Operator.Equality:
			case Operator.Inequality:
			case Operator.LessThan:
			case Operator.LessThanOrEqual:
			case Operator.GreaterThan:
			case Operator.GreaterThanOrEqual:
				if (TypeManager.IsEnumType (left.Type))
					return left;
				
				if (left.IsZeroInteger)
					return left.TryReduce (ec, right.Type, loc);
				
				break;
				
			case Operator.Addition:
			case Operator.Subtraction:
				return left;
				
			case Operator.Multiply:
			case Operator.Division:
			case Operator.Modulus:
			case Operator.LeftShift:
			case Operator.RightShift:
				if (TypeManager.IsEnumType (right.Type) || TypeManager.IsEnumType (left.Type))
					break;
				return left;
			}
			Error_OperatorCannotBeApplied (ec, this.left, this.right);
			return null;
		}

		//
		// The `|' operator used on types which were extended is dangerous
		//
		void CheckBitwiseOrOnSignExtended (ResolveContext ec)
		{
			OpcodeCast lcast = left as OpcodeCast;
			if (lcast != null) {
				if (IsUnsigned (lcast.UnderlyingType))
					lcast = null;
			}

			OpcodeCast rcast = right as OpcodeCast;
			if (rcast != null) {
				if (IsUnsigned (rcast.UnderlyingType))
					rcast = null;
			}

			if (lcast == null && rcast == null)
				return;

			// FIXME: consider constants

			ec.Report.Warning (675, 3, loc,
				"The operator `|' used on the sign-extended type `{0}'. Consider casting to a smaller unsigned type first",
				TypeManager.CSharpName (lcast != null ? lcast.UnderlyingType : rcast.UnderlyingType));
		}

		static void CreatePointerOperatorsTable ()
		{
			ArrayList temp = new ArrayList ();

			//
			// Pointer arithmetic:
			//
			// T* operator + (T* x, int y);		T* operator - (T* x, int y);
			// T* operator + (T* x, uint y);	T* operator - (T* x, uint y);
			// T* operator + (T* x, long y);	T* operator - (T* x, long y);
			// T* operator + (T* x, ulong y);	T* operator - (T* x, ulong y);
			//
			temp.Add (new PredefinedPointerOperator (null, TypeManager.int32_type, Operator.AdditionMask | Operator.SubtractionMask));
			temp.Add (new PredefinedPointerOperator (null, TypeManager.uint32_type, Operator.AdditionMask | Operator.SubtractionMask));
			temp.Add (new PredefinedPointerOperator (null, TypeManager.int64_type, Operator.AdditionMask | Operator.SubtractionMask));
			temp.Add (new PredefinedPointerOperator (null, TypeManager.uint64_type, Operator.AdditionMask | Operator.SubtractionMask));

			//
			// T* operator + (int y,   T* x);
			// T* operator + (uint y,  T *x);
			// T* operator + (long y,  T *x);
			// T* operator + (ulong y, T *x);
			//
			temp.Add (new PredefinedPointerOperator (TypeManager.int32_type, null, Operator.AdditionMask, null));
			temp.Add (new PredefinedPointerOperator (TypeManager.uint32_type, null, Operator.AdditionMask, null));
			temp.Add (new PredefinedPointerOperator (TypeManager.int64_type, null, Operator.AdditionMask, null));
			temp.Add (new PredefinedPointerOperator (TypeManager.uint64_type, null, Operator.AdditionMask, null));

			//
			// long operator - (T* x, T *y)
			//
			temp.Add (new PredefinedPointerOperator (null, Operator.SubtractionMask, TypeManager.int64_type));

			pointer_operators = (PredefinedOperator []) temp.ToArray (typeof (PredefinedOperator));
		}

		static void CreateStandardOperatorsTable ()
		{
			ArrayList temp = new ArrayList ();
			Type bool_type = TypeManager.bool_type;

			temp.Add (new PredefinedOperator (TypeManager.int32_type, Operator.ArithmeticMask | Operator.BitwiseMask));
			temp.Add (new PredefinedOperator (TypeManager.uint32_type, Operator.ArithmeticMask | Operator.BitwiseMask));
			temp.Add (new PredefinedOperator (TypeManager.int64_type, Operator.ArithmeticMask | Operator.BitwiseMask));
			temp.Add (new PredefinedOperator (TypeManager.uint64_type, Operator.ArithmeticMask | Operator.BitwiseMask));
			temp.Add (new PredefinedOperator (TypeManager.float_type, Operator.ArithmeticMask));
			temp.Add (new PredefinedOperator (TypeManager.double_type, Operator.ArithmeticMask));
			temp.Add (new PredefinedOperator (TypeManager.decimal_type, Operator.ArithmeticMask));

			temp.Add (new PredefinedOperator (TypeManager.int32_type, Operator.ComparisonMask, bool_type));
			temp.Add (new PredefinedOperator (TypeManager.uint32_type, Operator.ComparisonMask, bool_type));
			temp.Add (new PredefinedOperator (TypeManager.int64_type, Operator.ComparisonMask, bool_type));
			temp.Add (new PredefinedOperator (TypeManager.uint64_type, Operator.ComparisonMask, bool_type));
			temp.Add (new PredefinedOperator (TypeManager.float_type, Operator.ComparisonMask, bool_type));
			temp.Add (new PredefinedOperator (TypeManager.double_type, Operator.ComparisonMask, bool_type));
			temp.Add (new PredefinedOperator (TypeManager.decimal_type, Operator.ComparisonMask, bool_type));

			temp.Add (new PredefinedOperator (TypeManager.string_type, Operator.EqualityMask, bool_type));

			temp.Add (new PredefinedStringOperator (TypeManager.string_type, Operator.AdditionMask));
			temp.Add (new PredefinedStringOperator (TypeManager.string_type, TypeManager.object_type, Operator.AdditionMask));
			temp.Add (new PredefinedStringOperator (TypeManager.object_type, TypeManager.string_type, Operator.AdditionMask));

			temp.Add (new PredefinedOperator (bool_type,
				Operator.BitwiseMask | Operator.LogicalMask | Operator.EqualityMask, bool_type));

			temp.Add (new PredefinedShiftOperator (TypeManager.int32_type, Operator.ShiftMask));
			temp.Add (new PredefinedShiftOperator (TypeManager.uint32_type, Operator.ShiftMask));
			temp.Add (new PredefinedShiftOperator (TypeManager.int64_type, Operator.ShiftMask));
			temp.Add (new PredefinedShiftOperator (TypeManager.uint64_type, Operator.ShiftMask));

			standard_operators = (PredefinedOperator []) temp.ToArray (typeof (PredefinedOperator));
		}

		//
		// Rules used during binary numeric promotion
		//
		static bool DoNumericPromotion (ref Expression prim_expr, ref Expression second_expr, Type type)
		{
			Expression temp;
			Type etype;

			Constant c = prim_expr as Constant;
			if (c != null) {
				temp = c.ConvertImplicitly (type);
				if (temp != null) {
					prim_expr = temp;
					return true;
				}
			}

			if (type == TypeManager.uint32_type) {
				etype = prim_expr.Type;
				if (etype == TypeManager.int32_type || etype == TypeManager.short_type || etype == TypeManager.sbyte_type) {
					type = TypeManager.int64_type;

					if (type != second_expr.Type) {
						c = second_expr as Constant;
						if (c != null)
							temp = c.ConvertImplicitly (type);
						else
							temp = Convert.ImplicitNumericConversion (second_expr, type);
						if (temp == null)
							return false;
						second_expr = temp;
					}
				}
			} else if (type == TypeManager.uint64_type) {
				//
				// A compile-time error occurs if the other operand is of type sbyte, short, int, or long
				//
				if (type == TypeManager.int32_type || type == TypeManager.int64_type ||
					type == TypeManager.sbyte_type || type == TypeManager.sbyte_type)
					return false;
			}

			temp = Convert.ImplicitNumericConversion (prim_expr, type);
			if (temp == null)
				return false;

			prim_expr = temp;
			return true;
		}

		//
		// 7.2.6.2 Binary numeric promotions
		//
		public bool DoBinaryOperatorPromotion (ResolveContext ec)
		{
			Type ltype = left.Type;
			Type rtype = right.Type;
			Expression temp;

			foreach (Type t in ConstantFold.binary_promotions) {
				if (t == ltype)
					return t == rtype || DoNumericPromotion (ref right, ref left, t);

				if (t == rtype)
					return t == ltype || DoNumericPromotion (ref left, ref right, t);
			}

			Type int32 = TypeManager.int32_type;
			if (ltype != int32) {
				Constant c = left as Constant;
				if (c != null)
					temp = c.ConvertImplicitly (int32);
				else
					temp = Convert.ImplicitNumericConversion (left, int32);

				if (temp == null)
					return false;
				left = temp;
			}

			if (rtype != int32) {
				Constant c = right as Constant;
				if (c != null)
					temp = c.ConvertImplicitly (int32);
				else
					temp = Convert.ImplicitNumericConversion (right, int32);

				if (temp == null)
					return false;
				right = temp;
			}

			return true;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (left == null)
				return null;

			if ((oper == Operator.Subtraction) && (left is ParenthesizedExpression)) {
				left = ((ParenthesizedExpression) left).Expr;
				left = left.Resolve (ec, ResolveFlags.VariableOrValue | ResolveFlags.Type);
				if (left == null)
					return null;

				if (left.eclass == ExprClass.Type) {
					ec.Report.Error (75, loc, "To cast a negative value, you must enclose the value in parentheses");
					return null;
				}
			} else
				left = left.Resolve (ec);

			if (left == null)
				return null;

			Constant lc = left as Constant;

			if (lc != null && lc.Type == TypeManager.bool_type &&
				((oper == Operator.LogicalAnd && lc.IsDefaultValue) ||
				 (oper == Operator.LogicalOr && !lc.IsDefaultValue))) {

				// FIXME: resolve right expression as unreachable
				// right.Resolve (ec);

				ec.Report.Warning (429, 4, loc, "Unreachable expression code detected");
				return left;
			}

			right = right.Resolve (ec);
			if (right == null)
				return null;

			eclass = ExprClass.Value;
			Constant rc = right as Constant;

			// The conversion rules are ignored in enum context but why
			if (!ec.HasSet (ResolveContext.Options.EnumScope) && lc != null && rc != null && (TypeManager.IsEnumType (left.Type) || TypeManager.IsEnumType (right.Type))) {
				lc = EnumLiftUp (ec, lc, rc, loc);
				if (lc != null)
					rc = EnumLiftUp (ec, rc, lc, loc);
			}

			if (rc != null && lc != null) {
				int prev_e = ec.Report.Errors;
				Expression e = ConstantFold.BinaryFold (
					ec, oper, lc, rc, loc);
				if (e != null || ec.Report.Errors != prev_e)
					return e;
			} else if ((oper == Operator.BitwiseAnd || oper == Operator.LogicalAnd) && !TypeManager.IsDynamicType (left.Type) &&
					((lc != null && lc.IsDefaultValue && !(lc is NullLiteral)) || (rc != null && rc.IsDefaultValue && !(rc is NullLiteral)))) {

				if ((ResolveOperator (ec)) == null) {
					Error_OperatorCannotBeApplied (ec, left, right);
					return null;
				}

				//
				// The result is a constant with side-effect
				//
				Constant side_effect = rc == null ?
					new SideEffectConstant (lc, right, loc) :
					new SideEffectConstant (rc, left, loc);

				return ReducedExpression.Create (side_effect, this);
			}

			// Comparison warnings
			if ((oper & Operator.ComparisonMask) != 0) {
				if (left.Equals (right)) {
					ec.Report.Warning (1718, 3, loc, "A comparison made to same variable. Did you mean to compare something else?");
				}
				CheckUselessComparison (ec, lc, right.Type);
				CheckUselessComparison (ec, rc, left.Type);
			}

			if (TypeManager.IsDynamicType (left.Type) || TypeManager.IsDynamicType (right.Type)) {
				Arguments args = new Arguments (2);
				args.Add (new Argument (left));
				args.Add (new Argument (right));
				return new DynamicExpressionStatement (this, args, loc).Resolve (ec);
			}

			if (RootContext.Version >= LanguageVersion.ISO_2 &&
				((TypeManager.IsNullableType (left.Type) && (right is NullLiteral || TypeManager.IsNullableType (right.Type) || TypeManager.IsValueType (right.Type))) ||
				(TypeManager.IsValueType (left.Type) && right is NullLiteral) ||
				(TypeManager.IsNullableType (right.Type) && (left is NullLiteral || TypeManager.IsNullableType (left.Type) || TypeManager.IsValueType (left.Type))) ||
				(TypeManager.IsValueType (right.Type) && left is NullLiteral)))
				return new Nullable.LiftedBinaryOperator (oper, left, right, loc).Resolve (ec);

			return DoResolveCore (ec, left, right);
		}

		protected Expression DoResolveCore (ResolveContext ec, Expression left_orig, Expression right_orig)
		{
			Expression expr = ResolveOperator (ec);
			if (expr == null)
				Error_OperatorCannotBeApplied (ec, left_orig, right_orig);

			if (left == null || right == null)
				throw new InternalErrorException ("Invalid conversion");

			if (oper == Operator.BitwiseOr)
				CheckBitwiseOrOnSignExtended (ec);

			return expr;
		}

#if NET_4_0
		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			var le = left.MakeExpression (ctx);
			var re = right.MakeExpression (ctx);
			bool is_checked = ctx.HasSet (BuilderContext.Options.CheckedScope);

			switch (oper) {
			case Operator.Addition:
				return is_checked ? SLE.Expression.AddChecked (le, re) : SLE.Expression.Add (le, re);
			case Operator.BitwiseAnd:
				return SLE.Expression.And (le, re);
			case Operator.BitwiseOr:
				return SLE.Expression.Or (le, re);
			case Operator.Division:
				return SLE.Expression.Divide (le, re);
			case Operator.Equality:
				return SLE.Expression.Equal (le, re);
			case Operator.ExclusiveOr:
				return SLE.Expression.ExclusiveOr (le, re);
			case Operator.GreaterThan:
				return SLE.Expression.GreaterThan (le, re);
			case Operator.GreaterThanOrEqual:
				return SLE.Expression.GreaterThanOrEqual (le, re);
			case Operator.Inequality:
				return SLE.Expression.NotEqual (le, re);
			case Operator.LeftShift:
				return SLE.Expression.LeftShift (le, re);
			case Operator.LessThan:
				return SLE.Expression.LessThan (le, re);
			case Operator.LessThanOrEqual:
				return SLE.Expression.LessThanOrEqual (le, re);
			case Operator.LogicalAnd:
				return SLE.Expression.AndAlso (le, re);
			case Operator.LogicalOr:
				return SLE.Expression.OrElse (le, re);
			case Operator.Modulus:
				return SLE.Expression.Modulo (le, re);
			case Operator.Multiply:
				return is_checked ? SLE.Expression.MultiplyChecked (le, re) : SLE.Expression.Multiply (le, re);
			case Operator.RightShift:
				return SLE.Expression.RightShift (le, re);
			case Operator.Subtraction:
				return is_checked ? SLE.Expression.SubtractChecked (le, re) : SLE.Expression.Subtract (le, re);
			default:
				throw new NotImplementedException (oper.ToString ());
			}
		}
#endif

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			left.MutateHoistedGenericType (storey);
			right.MutateHoistedGenericType (storey);
		}

		//
		// D operator + (D x, D y)
		// D operator - (D x, D y)
		// bool operator == (D x, D y)
		// bool operator != (D x, D y)
		//
		Expression ResolveOperatorDelegate (ResolveContext ec, Type l, Type r)
		{
			bool is_equality = (oper & Operator.EqualityMask) != 0;
			if (!TypeManager.IsEqual (l, r) && !TypeManager.IsVariantOf (r, l)) {
				Expression tmp;
				if (right.eclass == ExprClass.MethodGroup || (r == InternalType.AnonymousMethod && !is_equality)) {
					tmp = Convert.ImplicitConversionRequired (ec, right, l, loc);
					if (tmp == null)
						return null;
					right = tmp;
					r = right.Type;
				} else if (left.eclass == ExprClass.MethodGroup || (l == InternalType.AnonymousMethod && !is_equality)) {
					tmp = Convert.ImplicitConversionRequired (ec, left, r, loc);
					if (tmp == null)
						return null;
					left = tmp;
					l = left.Type;
				} else {
					return null;
				}
			}

			//
			// Resolve delegate equality as a user operator
			//
			if (is_equality)
				return ResolveUserOperator (ec, l, r);

			MethodInfo method;
			Arguments args = new Arguments (2);
			args.Add (new Argument (left));
			args.Add (new Argument (right));

			if (oper == Operator.Addition) {
				if (TypeManager.delegate_combine_delegate_delegate == null) {
					TypeManager.delegate_combine_delegate_delegate = TypeManager.GetPredefinedMethod (
						TypeManager.delegate_type, "Combine", loc, TypeManager.delegate_type, TypeManager.delegate_type);
				}

				method = TypeManager.delegate_combine_delegate_delegate;
			} else {
				if (TypeManager.delegate_remove_delegate_delegate == null) {
					TypeManager.delegate_remove_delegate_delegate = TypeManager.GetPredefinedMethod (
						TypeManager.delegate_type, "Remove", loc, TypeManager.delegate_type, TypeManager.delegate_type);
				}

				method = TypeManager.delegate_remove_delegate_delegate;
			}

			MethodGroupExpr mg = new MethodGroupExpr (new MemberInfo [] { method }, TypeManager.delegate_type, loc);
			mg = mg.OverloadResolve (ec, ref args, false, loc);

			return new ClassCast (new UserOperatorCall (mg, args, CreateExpressionTree, loc), l);
		}

		//
		// Enumeration operators
		//
		Expression ResolveOperatorEnum (ResolveContext ec, bool lenum, bool renum, Type ltype, Type rtype)
		{
			//
			// bool operator == (E x, E y);
			// bool operator != (E x, E y);
			// bool operator < (E x, E y);
			// bool operator > (E x, E y);
			// bool operator <= (E x, E y);
			// bool operator >= (E x, E y);
			//
			// E operator & (E x, E y);
			// E operator | (E x, E y);
			// E operator ^ (E x, E y);
			//
			// U operator - (E e, E f)
			// E operator - (E e, U x)
			//
			// E operator + (U x, E e)
			// E operator + (E e, U x)
			//
			if (!((oper & (Operator.ComparisonMask | Operator.BitwiseMask)) != 0 ||
				(oper == Operator.Subtraction && lenum) ||
				(oper == Operator.Addition && (lenum != renum || type != null))))	// type != null for lifted null
				return null;

			Expression ltemp = left;
			Expression rtemp = right;
			Type underlying_type;
			Expression expr;
			
			if ((oper & (Operator.ComparisonMask | Operator.BitwiseMask)) != 0) {
				if (renum) {
					expr = Convert.ImplicitConversion (ec, left, rtype, loc);
					if (expr != null) {
						left = expr;
						ltype = expr.Type;
					}
				} else if (lenum) {
					expr = Convert.ImplicitConversion (ec, right, ltype, loc);
					if (expr != null) {
						right = expr;
						rtype = expr.Type;
					}
				}
			}			

			if (TypeManager.IsEqual (ltype, rtype)) {
				underlying_type = TypeManager.GetEnumUnderlyingType (ltype);

				if (left is Constant)
					left = ((Constant) left).ConvertExplicitly (false, underlying_type);
				else
					left = EmptyCast.Create (left, underlying_type);

				if (right is Constant)
					right = ((Constant) right).ConvertExplicitly (false, underlying_type);
				else
					right = EmptyCast.Create (right, underlying_type);
			} else if (lenum) {
				underlying_type = TypeManager.GetEnumUnderlyingType (ltype);

				if (oper != Operator.Subtraction && oper != Operator.Addition) {
					Constant c = right as Constant;
					if (c == null || !c.IsDefaultValue)
						return null;
				} else {
					if (!Convert.ImplicitStandardConversionExists (right, underlying_type))
						return null;

					right = Convert.ImplicitConversionStandard (ec, right, underlying_type, right.Location);
				}

				if (left is Constant)
					left = ((Constant) left).ConvertExplicitly (false, underlying_type);
				else
					left = EmptyCast.Create (left, underlying_type);

			} else if (renum) {
				underlying_type = TypeManager.GetEnumUnderlyingType (rtype);

				if (oper != Operator.Addition) {
					Constant c = left as Constant;
					if (c == null || !c.IsDefaultValue)
						return null;
				} else {
					if (!Convert.ImplicitStandardConversionExists (left, underlying_type))
						return null;

					left = Convert.ImplicitConversionStandard (ec, left, underlying_type, left.Location);
				}

				if (right is Constant)
					right = ((Constant) right).ConvertExplicitly (false, underlying_type);
				else
					right = EmptyCast.Create (right, underlying_type);

			} else {
				return null;
			}

			//
			// C# specification uses explicit cast syntax which means binary promotion
			// should happen, however it seems that csc does not do that
			//
			if (!DoBinaryOperatorPromotion (ec)) {
				left = ltemp;
				right = rtemp;
				return null;
			}

			Type res_type = null;
			if ((oper & Operator.BitwiseMask) != 0 || oper == Operator.Subtraction || oper == Operator.Addition) {
				Type promoted_type = lenum ? left.Type : right.Type;
				enum_conversion = Convert.ExplicitNumericConversion (
					new EmptyExpression (promoted_type), underlying_type);

				if (oper == Operator.Subtraction && renum && lenum)
					res_type = underlying_type;
				else if (oper == Operator.Addition && renum)
					res_type = rtype;
				else
					res_type = ltype;
			}
			
			expr = ResolveOperatorPredefined (ec, standard_operators, true, res_type);
			if (!is_compound || expr == null)
				return expr;

			//
			// Section: 7.16.2
			//

			//
			// If the return type of the selected operator is implicitly convertible to the type of x
			//
			if (Convert.ImplicitConversionExists (ec, expr, ltype))
				return expr;

			//
			// Otherwise, if the selected operator is a predefined operator, if the return type of the
			// selected operator is explicitly convertible to the type of x, and if y is implicitly
			// convertible to the type of x or the operator is a shift operator, then the operation
			// is evaluated as x = (T)(x op y), where T is the type of x
			//
			expr = Convert.ExplicitConversion (ec, expr, ltype, loc);
			if (expr == null)
				return null;

			if (Convert.ImplicitConversionExists (ec, ltemp, ltype))
				return expr;

			return null;
		}

		//
		// 7.9.6 Reference type equality operators
		//
		Binary ResolveOperatorEqualityRerefence (ResolveContext ec, Type l, Type r)
		{
			//
			// operator != (object a, object b)
			// operator == (object a, object b)
			//

			// TODO: this method is almost equivalent to Convert.ImplicitReferenceConversion

			if (left.eclass == ExprClass.MethodGroup || right.eclass == ExprClass.MethodGroup)
				return null;

			type = TypeManager.bool_type;
			GenericConstraints constraints;

			bool lgen = TypeManager.IsGenericParameter (l);

			if (TypeManager.IsEqual (l, r)) {
				if (lgen) {
					//
					// Only allow to compare same reference type parameter
					//
					if (TypeManager.IsReferenceType (l)) {
						left = new BoxedCast (left, TypeManager.object_type);
						right = new BoxedCast (right, TypeManager.object_type);
						return this;
					}

					return null;
				}

				if (l == InternalType.AnonymousMethod)
					return null;

				if (TypeManager.IsValueType (l))
					return null;

				return this;
			}

			bool rgen = TypeManager.IsGenericParameter (r);

			//
			// a, Both operands are reference-type values or the value null
			// b, One operand is a value of type T where T is a type-parameter and
			// the other operand is the value null. Furthermore T does not have the
			// value type constrain
			//
			if (left is NullLiteral || right is NullLiteral) {
				if (lgen) {
					constraints = TypeManager.GetTypeParameterConstraints (l);
					if (constraints != null && constraints.HasValueTypeConstraint)
						return null;

					left = new BoxedCast (left, TypeManager.object_type);
					return this;
				}

				if (rgen) {
					constraints = TypeManager.GetTypeParameterConstraints (r);
					if (constraints != null && constraints.HasValueTypeConstraint)
						return null;

					right = new BoxedCast (right, TypeManager.object_type);
					return this;
				}
			}

			//
			// An interface is converted to the object before the
			// standard conversion is applied. It's not clear from the
			// standard but it looks like it works like that.
			//
			if (lgen) {
				if (!TypeManager.IsReferenceType (l))
					return null;

				l = TypeManager.object_type;
				left = new BoxedCast (left, l);
			} else if (l.IsInterface) {
				l = TypeManager.object_type;
			} else if (TypeManager.IsStruct (l)) {
				return null;
			}

			if (rgen) {
				if (!TypeManager.IsReferenceType (r))
					return null;

				r = TypeManager.object_type;
				right = new BoxedCast (right, r);
			} else if (r.IsInterface) {
				r = TypeManager.object_type;
			} else if (TypeManager.IsStruct (r)) {
				return null;
			}


			const string ref_comparison = "Possible unintended reference comparison. " +
				"Consider casting the {0} side of the expression to `string' to compare the values";

			//
			// A standard implicit conversion exists from the type of either
			// operand to the type of the other operand
			//
			if (Convert.ImplicitReferenceConversionExists (left, r)) {
				if (l == TypeManager.string_type)
					ec.Report.Warning (253, 2, loc, ref_comparison, "right");

				return this;
			}

			if (Convert.ImplicitReferenceConversionExists (right, l)) {
				if (r == TypeManager.string_type)
					ec.Report.Warning (252, 2, loc, ref_comparison, "left");

				return this;
			}

			return null;
		}


		Expression ResolveOperatorPointer (ResolveContext ec, Type l, Type r)
		{
			//
			// bool operator == (void* x, void* y);
			// bool operator != (void* x, void* y);
			// bool operator < (void* x, void* y);
			// bool operator > (void* x, void* y);
			// bool operator <= (void* x, void* y);
			// bool operator >= (void* x, void* y);
			//
			if ((oper & Operator.ComparisonMask) != 0) {
				Expression temp;
				if (!l.IsPointer) {
					temp = Convert.ImplicitConversion (ec, left, r, left.Location);
					if (temp == null)
						return null;
					left = temp;
				}

				if (!r.IsPointer) {
					temp = Convert.ImplicitConversion (ec, right, l, right.Location);
					if (temp == null)
						return null;
					right = temp;
				}

				type = TypeManager.bool_type;
				return this;
			}

			if (pointer_operators == null)
				CreatePointerOperatorsTable ();

			return ResolveOperatorPredefined (ec, pointer_operators, false, null);
		}

		//
		// Build-in operators method overloading
		//
		protected virtual Expression ResolveOperatorPredefined (ResolveContext ec, PredefinedOperator [] operators, bool primitives_only, Type enum_type)
		{
			PredefinedOperator best_operator = null;
			Type l = left.Type;
			Type r = right.Type;
			Operator oper_mask = oper & ~Operator.ValuesOnlyMask;

			foreach (PredefinedOperator po in operators) {
				if ((po.OperatorsMask & oper_mask) == 0)
					continue;

				if (primitives_only) {
					if (!po.IsPrimitiveApplicable (l, r))
						continue;
				} else {
					if (!po.IsApplicable (ec, left, right))
						continue;
				}

				if (best_operator == null) {
					best_operator = po;
					if (primitives_only)
						break;

					continue;
				}

				best_operator = po.ResolveBetterOperator (ec, best_operator);

				if (best_operator == null) {
					ec.Report.Error (34, loc, "Operator `{0}' is ambiguous on operands of type `{1}' and `{2}'",
						OperName (oper), left.GetSignatureForError (), right.GetSignatureForError ());

					best_operator = po;
					break;
				}
			}

			if (best_operator == null)
				return null;

			Expression expr = best_operator.ConvertResult (ec, this);
			if (enum_type == null)
				return expr;

			//
			// HACK: required by enum_conversion
			//
			expr.Type = enum_type;
			return EmptyCast.Create (expr, enum_type);
		}

		//
		// Performs user-operator overloading
		//
		protected virtual Expression ResolveUserOperator (ResolveContext ec, Type l, Type r)
		{
			Operator user_oper;
			if (oper == Operator.LogicalAnd)
				user_oper = Operator.BitwiseAnd;
			else if (oper == Operator.LogicalOr)
				user_oper = Operator.BitwiseOr;
			else
				user_oper = oper;

			string op = GetOperatorMetadataName (user_oper);

			MethodGroupExpr left_operators = MemberLookup (ec.Compiler, ec.CurrentType, l, op, MemberTypes.Method, AllBindingFlags, loc) as MethodGroupExpr;
			MethodGroupExpr right_operators = null;

			if (!TypeManager.IsEqual (r, l)) {
				right_operators = MemberLookup (ec.Compiler, ec.CurrentType, r, op, MemberTypes.Method, AllBindingFlags, loc) as MethodGroupExpr;
				if (right_operators == null && left_operators == null)
					return null;
			} else if (left_operators == null) {
				return null;
			}

			Arguments args = new Arguments (2);
			Argument larg = new Argument (left);
			args.Add (larg);
			Argument rarg = new Argument (right);
			args.Add (rarg);

			MethodGroupExpr union;

			//
			// User-defined operator implementations always take precedence
			// over predefined operator implementations
			//
			if (left_operators != null && right_operators != null) {
				if (IsPredefinedUserOperator (l, user_oper)) {
					union = right_operators.OverloadResolve (ec, ref args, true, loc);
					if (union == null)
						union = left_operators;
				} else if (IsPredefinedUserOperator (r, user_oper)) {
					union = left_operators.OverloadResolve (ec, ref args, true, loc);
					if (union == null)
						union = right_operators;
				} else {
					union = MethodGroupExpr.MakeUnionSet (left_operators, right_operators, loc);
				}
			} else if (left_operators != null) {
				union = left_operators;
			} else {
				union = right_operators;
			}

			union = union.OverloadResolve (ec, ref args, true, loc);
			if (union == null)
				return null;

			Expression oper_expr;

			// TODO: CreateExpressionTree is allocated every time
			if (user_oper != oper) {
				oper_expr = new ConditionalLogicalOperator (union, args, CreateExpressionTree,
					oper == Operator.LogicalAnd, loc).Resolve (ec);
			} else {
				oper_expr = new UserOperatorCall (union, args, CreateExpressionTree, loc);

				//
				// This is used to check if a test 'x == null' can be optimized to a reference equals,
				// and not invoke user operator
				//
				if ((oper & Operator.EqualityMask) != 0) {
					if ((left is NullLiteral && IsBuildInEqualityOperator (r)) ||
						(right is NullLiteral && IsBuildInEqualityOperator (l))) {
						type = TypeManager.bool_type;
						if (left is NullLiteral || right is NullLiteral)
							oper_expr = ReducedExpression.Create (this, oper_expr).Resolve (ec);
					} else if (l != r) {
						MethodInfo mi = (MethodInfo) union;
						
						//
						// Two System.Delegate(s) are never equal
						//
						if (mi.DeclaringType == TypeManager.multicast_delegate_type)
							return null;
					}
				}
			}

			left = larg.Expr;
			right = rarg.Expr;
			return oper_expr;
		}

		public override TypeExpr ResolveAsTypeTerminal (IMemberContext ec, bool silent)
		{
			return null;
		}

		private void CheckUselessComparison (ResolveContext ec, Constant c, Type type)
		{
			if (c == null || !IsTypeIntegral (type)
				|| c is StringConstant
				|| c is BoolConstant
				|| c is FloatConstant
				|| c is DoubleConstant
				|| c is DecimalConstant
				)
				return;

			long value = 0;

			if (c is ULongConstant) {
				ulong uvalue = ((ULongConstant) c).Value;
				if (uvalue > long.MaxValue) {
					if (type == TypeManager.byte_type ||
					    type == TypeManager.sbyte_type ||
					    type == TypeManager.short_type ||
					    type == TypeManager.ushort_type ||
					    type == TypeManager.int32_type ||
					    type == TypeManager.uint32_type ||
					    type == TypeManager.int64_type ||
						type == TypeManager.char_type)
						WarnUselessComparison (ec, type);
					return;
				}
				value = (long) uvalue;
			}
			else if (c is ByteConstant)
				value = ((ByteConstant) c).Value;
			else if (c is SByteConstant)
				value = ((SByteConstant) c).Value;
			else if (c is ShortConstant)
				value = ((ShortConstant) c).Value;
			else if (c is UShortConstant)
				value = ((UShortConstant) c).Value;
			else if (c is IntConstant)
				value = ((IntConstant) c).Value;
			else if (c is UIntConstant)
				value = ((UIntConstant) c).Value;
			else if (c is LongConstant)
				value = ((LongConstant) c).Value;
			else if (c is CharConstant)
				value = ((CharConstant)c).Value;

			if (value == 0)
				return;

			if (IsValueOutOfRange (value, type))
				WarnUselessComparison (ec, type);
		}

		static bool IsValueOutOfRange (long value, Type type)
		{
			if (IsTypeUnsigned (type) && value < 0)
				return true;
			return type == TypeManager.sbyte_type && (value >= 0x80 || value < -0x80) ||
				type == TypeManager.byte_type && value >= 0x100 ||
				type == TypeManager.short_type && (value >= 0x8000 || value < -0x8000) ||
				type == TypeManager.ushort_type && value >= 0x10000 ||
				type == TypeManager.int32_type && (value >= 0x80000000 || value < -0x80000000) ||
				type == TypeManager.uint32_type && value >= 0x100000000;
		}

		static bool IsBuildInEqualityOperator (Type t)
		{
			return t == TypeManager.object_type || t == TypeManager.string_type ||
				t == TypeManager.delegate_type || TypeManager.IsDelegateType (t);
		}

		static bool IsPredefinedUserOperator (Type t, Operator op)
		{
			//
			// Some predefined types have user operators
			//
			return (op & Operator.EqualityMask) != 0 && (t == TypeManager.string_type || t == TypeManager.decimal_type);
		}

		private static bool IsTypeIntegral (Type type)
		{
			return type == TypeManager.uint64_type ||
				type == TypeManager.int64_type ||
				type == TypeManager.uint32_type ||
				type == TypeManager.int32_type ||
				type == TypeManager.ushort_type ||
				type == TypeManager.short_type ||
				type == TypeManager.sbyte_type ||
				type == TypeManager.byte_type ||
				type == TypeManager.char_type;
		}

		private static bool IsTypeUnsigned (Type type)
		{
			return type == TypeManager.uint64_type ||
				type == TypeManager.uint32_type ||
				type == TypeManager.ushort_type ||
				type == TypeManager.byte_type ||
				type == TypeManager.char_type;
		}

		private void WarnUselessComparison (ResolveContext ec, Type type)
		{
			ec.Report.Warning (652, 2, loc, "A comparison between a constant and a variable is useless. The constant is out of the range of the variable type `{0}'",
				TypeManager.CSharpName (type));
		}

		/// <remarks>
		///   EmitBranchable is called from Statement.EmitBoolExpression in the
		///   context of a conditional bool expression.  This function will return
		///   false if it is was possible to use EmitBranchable, or true if it was.
		///
		///   The expression's code is generated, and we will generate a branch to `target'
		///   if the resulting expression value is equal to isTrue
		/// </remarks>
		public override void EmitBranchable (EmitContext ec, Label target, bool on_true)
		{
			ILGenerator ig = ec.ig;

			//
			// This is more complicated than it looks, but its just to avoid
			// duplicated tests: basically, we allow ==, !=, >, <, >= and <=
			// but on top of that we want for == and != to use a special path
			// if we are comparing against null
			//
			if ((oper == Operator.Equality || oper == Operator.Inequality) && (left is Constant || right is Constant)) {
				bool my_on_true = oper == Operator.Inequality ? on_true : !on_true;
				
				//
				// put the constant on the rhs, for simplicity
				//
				if (left is Constant) {
					Expression swap = right;
					right = left;
					left = swap;
				}
				
				if (((Constant) right).IsZeroInteger) {
					left.EmitBranchable (ec, target, my_on_true);
					return;
				}
				if (right.Type == TypeManager.bool_type) {
					// right is a boolean, and it's not 'false' => it is 'true'
					left.EmitBranchable (ec, target, !my_on_true);
					return;
				}

			} else if (oper == Operator.LogicalAnd) {

				if (on_true) {
					Label tests_end = ig.DefineLabel ();
					
					left.EmitBranchable (ec, tests_end, false);
					right.EmitBranchable (ec, target, true);
					ig.MarkLabel (tests_end);					
				} else {
					//
					// This optimizes code like this 
					// if (true && i > 4)
					//
					if (!(left is Constant))
						left.EmitBranchable (ec, target, false);

					if (!(right is Constant)) 
						right.EmitBranchable (ec, target, false);
				}
				
				return;
				
			} else if (oper == Operator.LogicalOr){
				if (on_true) {
					left.EmitBranchable (ec, target, true);
					right.EmitBranchable (ec, target, true);
					
				} else {
					Label tests_end = ig.DefineLabel ();
					left.EmitBranchable (ec, tests_end, true);
					right.EmitBranchable (ec, target, false);
					ig.MarkLabel (tests_end);
				}
				
				return;
				
			} else if (!(oper == Operator.LessThan        || oper == Operator.GreaterThan ||
			             oper == Operator.LessThanOrEqual || oper == Operator.GreaterThanOrEqual ||
			             oper == Operator.Equality        || oper == Operator.Inequality)) {
				base.EmitBranchable (ec, target, on_true);
				return;
			}
			
			left.Emit (ec);
			right.Emit (ec);

			Type t = left.Type;
			bool is_float = IsFloat (t);
			bool is_unsigned = is_float || IsUnsigned (t);
			
			switch (oper){
			case Operator.Equality:
				if (on_true)
					ig.Emit (OpCodes.Beq, target);
				else
					ig.Emit (OpCodes.Bne_Un, target);
				break;

			case Operator.Inequality:
				if (on_true)
					ig.Emit (OpCodes.Bne_Un, target);
				else
					ig.Emit (OpCodes.Beq, target);
				break;

			case Operator.LessThan:
				if (on_true)
					if (is_unsigned && !is_float)
						ig.Emit (OpCodes.Blt_Un, target);
					else
						ig.Emit (OpCodes.Blt, target);
				else
					if (is_unsigned)
						ig.Emit (OpCodes.Bge_Un, target);
					else
						ig.Emit (OpCodes.Bge, target);
				break;

			case Operator.GreaterThan:
				if (on_true)
					if (is_unsigned && !is_float)
						ig.Emit (OpCodes.Bgt_Un, target);
					else
						ig.Emit (OpCodes.Bgt, target);
				else
					if (is_unsigned)
						ig.Emit (OpCodes.Ble_Un, target);
					else
						ig.Emit (OpCodes.Ble, target);
				break;

			case Operator.LessThanOrEqual:
				if (on_true)
					if (is_unsigned && !is_float)
						ig.Emit (OpCodes.Ble_Un, target);
					else
						ig.Emit (OpCodes.Ble, target);
				else
					if (is_unsigned)
						ig.Emit (OpCodes.Bgt_Un, target);
					else
						ig.Emit (OpCodes.Bgt, target);
				break;


			case Operator.GreaterThanOrEqual:
				if (on_true)
					if (is_unsigned && !is_float)
						ig.Emit (OpCodes.Bge_Un, target);
					else
						ig.Emit (OpCodes.Bge, target);
				else
					if (is_unsigned)
						ig.Emit (OpCodes.Blt_Un, target);
					else
						ig.Emit (OpCodes.Blt, target);
				break;
			default:
				throw new InternalErrorException (oper.ToString ());
			}
		}
		
		public override void Emit (EmitContext ec)
		{
			EmitOperator (ec, left.Type);
		}

		protected virtual void EmitOperator (EmitContext ec, Type l)
		{
			ILGenerator ig = ec.ig;

			//
			// Handle short-circuit operators differently
			// than the rest
			//
			if ((oper & Operator.LogicalMask) != 0) {
				Label load_result = ig.DefineLabel ();
				Label end = ig.DefineLabel ();

				bool is_or = oper == Operator.LogicalOr;
				left.EmitBranchable (ec, load_result, is_or);
				right.Emit (ec);
				ig.Emit (OpCodes.Br_S, end);
				
				ig.MarkLabel (load_result);
				ig.Emit (is_or ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
				ig.MarkLabel (end);
				return;
			}

			left.Emit (ec);

			//
			// Optimize zero-based operations
			//
			// TODO: Implement more optimizations, but it should probably go to PredefinedOperators
			//
			if ((oper & Operator.ShiftMask) != 0 || oper == Operator.Addition || oper == Operator.Subtraction) {
				Constant rc = right as Constant;
				if (rc != null && rc.IsDefaultValue) {
					return;
				}
			}

			right.Emit (ec);
			EmitOperatorOpcode (ec, oper, l);

			//
			// Nullable enum could require underlying type cast and we cannot simply wrap binary
			// expression because that would wrap lifted binary operation
			//
			if (enum_conversion != null)
				enum_conversion.Emit (ec);
		}

		public override void EmitSideEffect (EmitContext ec)
		{
			if ((oper & Operator.LogicalMask) != 0 ||
				(ec.HasSet (EmitContext.Options.CheckedScope) && (oper == Operator.Multiply || oper == Operator.Addition || oper == Operator.Subtraction))) {
				base.EmitSideEffect (ec);
			} else {
				left.EmitSideEffect (ec);
				right.EmitSideEffect (ec);
			}
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			Binary target = (Binary) t;

			target.left = left.Clone (clonectx);
			target.right = right.Clone (clonectx);
		}

		public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (4);

			MemberAccess sle = new MemberAccess (new MemberAccess (
				new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "System", loc), "Linq", loc), "Expressions", loc);

			MemberAccess binder = DynamicExpressionStatement.GetBinderNamespace (loc);

			binder_args.Add (new Argument (new MemberAccess (new MemberAccess (sle, "ExpressionType", loc), GetOperatorExpressionTypeName (), loc)));
			binder_args.Add (new Argument (new BoolLiteral (ec.HasSet (ResolveContext.Options.CheckedScope), loc)));

			bool member_access = left is DynamicMemberBinder || right is DynamicMemberBinder;
			binder_args.Add (new Argument (new BoolLiteral (member_access, loc)));
			binder_args.Add (new Argument (new ImplicitlyTypedArrayCreation ("[]", args.CreateDynamicBinderArguments (), loc)));

			return new New (new MemberAccess (binder, "CSharpBinaryOperationBinder", loc), binder_args, loc);
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return CreateExpressionTree (ec, null);
		}

		Expression CreateExpressionTree (ResolveContext ec, MethodGroupExpr method)		
		{
			string method_name;
			bool lift_arg = false;
			
			switch (oper) {
			case Operator.Addition:
				if (method == null && ec.HasSet (ResolveContext.Options.CheckedScope) && !IsFloat (type))
					method_name = "AddChecked";
				else
					method_name = "Add";
				break;
			case Operator.BitwiseAnd:
				method_name = "And";
				break;
			case Operator.BitwiseOr:
				method_name = "Or";
				break;
			case Operator.Division:
				method_name = "Divide";
				break;
			case Operator.Equality:
				method_name = "Equal";
				lift_arg = true;
				break;
			case Operator.ExclusiveOr:
				method_name = "ExclusiveOr";
				break;				
			case Operator.GreaterThan:
				method_name = "GreaterThan";
				lift_arg = true;
				break;
			case Operator.GreaterThanOrEqual:
				method_name = "GreaterThanOrEqual";
				lift_arg = true;
				break;
			case Operator.Inequality:
				method_name = "NotEqual";
				lift_arg = true;
				break;
			case Operator.LeftShift:
				method_name = "LeftShift";
				break;
			case Operator.LessThan:
				method_name = "LessThan";
				lift_arg = true;
				break;
			case Operator.LessThanOrEqual:
				method_name = "LessThanOrEqual";
				lift_arg = true;
				break;
			case Operator.LogicalAnd:
				method_name = "AndAlso";
				break;
			case Operator.LogicalOr:
				method_name = "OrElse";
				break;
			case Operator.Modulus:
				method_name = "Modulo";
				break;
			case Operator.Multiply:
				if (method == null && ec.HasSet (ResolveContext.Options.CheckedScope) && !IsFloat (type))
					method_name = "MultiplyChecked";
				else
					method_name = "Multiply";
				break;
			case Operator.RightShift:
				method_name = "RightShift";
				break;
			case Operator.Subtraction:
				if (method == null && ec.HasSet (ResolveContext.Options.CheckedScope) && !IsFloat (type))
					method_name = "SubtractChecked";
				else
					method_name = "Subtract";
				break;

			default:
				throw new InternalErrorException ("Unknown expression tree binary operator " + oper);
			}

			Arguments args = new Arguments (2);
			args.Add (new Argument (left.CreateExpressionTree (ec)));
			args.Add (new Argument (right.CreateExpressionTree (ec)));
			if (method != null) {
				if (lift_arg)
					args.Add (new Argument (new BoolConstant (false, loc)));
				
				args.Add (new Argument (method.CreateExpressionTree (ec)));
			}
			
			return CreateExpressionFactoryCall (ec, method_name, args);
		}
	}
	
	//
	// Represents the operation a + b [+ c [+ d [+ ...]]], where a is a string
	// b, c, d... may be strings or objects.
	//
	public class StringConcat : Expression {
		Arguments arguments;
		
		public StringConcat (Location loc, Expression left, Expression right)
		{
			this.loc = loc;
			type = TypeManager.string_type;
			eclass = ExprClass.Value;

			arguments = new Arguments (2);
			Append (left);
			Append (right);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Argument arg = arguments [0];
			return CreateExpressionAddCall (ec, arg, arg.CreateExpressionTree (ec), 1);
		}

		//
		// Creates nested calls tree from an array of arguments used for IL emit
		//
		Expression CreateExpressionAddCall (ResolveContext ec, Argument left, Expression left_etree, int pos)
		{
			Arguments concat_args = new Arguments (2);
			Arguments add_args = new Arguments (3);

			concat_args.Add (left);
			add_args.Add (new Argument (left_etree));

			concat_args.Add (arguments [pos]);
			add_args.Add (new Argument (arguments [pos].CreateExpressionTree (ec)));

			MethodGroupExpr method = CreateConcatMemberExpression ().Resolve (ec) as MethodGroupExpr;
			if (method == null)
				return null;

			method = method.OverloadResolve (ec, ref concat_args, false, loc);
			if (method == null)
				return null;

			add_args.Add (new Argument (method.CreateExpressionTree (ec)));

			Expression expr = CreateExpressionFactoryCall (ec, "Add", add_args);
			if (++pos == arguments.Count)
				return expr;

			left = new Argument (new EmptyExpression (((MethodInfo)method).ReturnType));
			return CreateExpressionAddCall (ec, left, expr, pos);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			return this;
		}
		
		public void Append (Expression operand)
		{
			//
			// Constant folding
			//
			StringConstant sc = operand as StringConstant;
			if (sc != null) {
				if (arguments.Count != 0) {
					Argument last_argument = arguments [arguments.Count - 1];
					StringConstant last_expr_constant = last_argument.Expr as StringConstant;
					if (last_expr_constant != null) {
						last_argument.Expr = new StringConstant (
							last_expr_constant.Value + sc.Value, sc.Location);
						return;
					}
				}
			} else {
				//
				// Multiple (3+) concatenation are resolved as multiple StringConcat instances
				//
				StringConcat concat_oper = operand as StringConcat;
				if (concat_oper != null) {
					arguments.AddRange (concat_oper.arguments);
					return;
				}
			}

			arguments.Add (new Argument (operand));
		}

		Expression CreateConcatMemberExpression ()
		{
			return new MemberAccess (new MemberAccess (new QualifiedAliasMember ("global", "System", loc), "String", loc), "Concat", loc);
		}

		public override void Emit (EmitContext ec)
		{
			Expression concat = new Invocation (CreateConcatMemberExpression (), arguments, true);
			concat = concat.Resolve (new ResolveContext (ec.MemberContext));
			if (concat != null)
				concat.Emit (ec);
		}

#if NET_4_0
		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			if (arguments.Count != 2)
				throw new NotImplementedException ("arguments.Count != 2");

			var concat = TypeManager.string_type.GetMethod ("Concat", new[] { typeof (object), typeof (object) });
			return SLE.Expression.Add (arguments[0].Expr.MakeExpression (ctx), arguments[1].Expr.MakeExpression (ctx), concat);
		}
#endif
		
		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			arguments.MutateHoistedGenericType (storey);
		}		
	}

	//
	// User-defined conditional logical operator
	//
	public class ConditionalLogicalOperator : UserOperatorCall {
		readonly bool is_and;
		Expression oper;

		public ConditionalLogicalOperator (MethodGroupExpr oper_method, Arguments arguments,
			ExpressionTreeExpression expr_tree, bool is_and, Location loc)
			: base (oper_method, arguments, expr_tree, loc)
		{
			this.is_and = is_and;
		}
		
		public override Expression DoResolve (ResolveContext ec)
		{
			MethodInfo method = (MethodInfo)mg;
			type = TypeManager.TypeToCoreType (method.ReturnType);
			AParametersCollection pd = TypeManager.GetParameterData (method);
			if (!TypeManager.IsEqual (type, type) || !TypeManager.IsEqual (type, pd.Types [0]) || !TypeManager.IsEqual (type, pd.Types [1])) {
				ec.Report.Error (217, loc,
					"A user-defined operator `{0}' must have parameters and return values of the same type in order to be applicable as a short circuit operator",
					TypeManager.CSharpSignature (method));
				return null;
			}

			Expression left_dup = new EmptyExpression (type);
			Expression op_true = GetOperatorTrue (ec, left_dup, loc);
			Expression op_false = GetOperatorFalse (ec, left_dup, loc);
			if (op_true == null || op_false == null) {
				ec.Report.Error (218, loc,
					"The type `{0}' must have operator `true' and operator `false' defined when `{1}' is used as a short circuit operator",
					TypeManager.CSharpName (type), TypeManager.CSharpSignature (method));
				return null;
			}

			oper = is_and ? op_false : op_true;
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label end_target = ig.DefineLabel ();

			//
			// Emit and duplicate left argument
			//
			arguments [0].Expr.Emit (ec);
			ig.Emit (OpCodes.Dup);
			arguments.RemoveAt (0);

			oper.EmitBranchable (ec, end_target, true);
			base.Emit (ec);
			ig.MarkLabel (end_target);
		}
	}

	public class PointerArithmetic : Expression {
		Expression left, right;
		Binary.Operator op;

		//
		// We assume that `l' is always a pointer
		//
		public PointerArithmetic (Binary.Operator op, Expression l, Expression r, Type t, Location loc)
		{
			type = t;
			this.loc = loc;
			left = l;
			right = r;
			this.op = op;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Error_PointerInsideExpressionTree (ec);
			return null;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			eclass = ExprClass.Variable;
			
			if (left.Type == TypeManager.void_ptr_type) {
				ec.Report.Error (242, loc, "The operation in question is undefined on void pointers");
				return null;
			}
			
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Type op_type = left.Type;
			ILGenerator ig = ec.ig;
			
			// It must be either array or fixed buffer
			Type element;
			if (TypeManager.HasElementType (op_type)) {
				element = TypeManager.GetElementType (op_type);
			} else {
				FieldExpr fe = left as FieldExpr;
				if (fe != null)
					element = AttributeTester.GetFixedBuffer (fe.FieldInfo).ElementType;
				else
					element = op_type;
			}

			int size = GetTypeSize (element);
			Type rtype = right.Type;
			
			if ((op & Binary.Operator.SubtractionMask) != 0 && rtype.IsPointer){
				//
				// handle (pointer - pointer)
				//
				left.Emit (ec);
				right.Emit (ec);
				ig.Emit (OpCodes.Sub);

				if (size != 1){
					if (size == 0)
						ig.Emit (OpCodes.Sizeof, element);
					else 
						IntLiteral.EmitInt (ig, size);
					ig.Emit (OpCodes.Div);
				}
				ig.Emit (OpCodes.Conv_I8);
			} else {
				//
				// handle + and - on (pointer op int)
				//
				Constant left_const = left as Constant;
				if (left_const != null) {
					//
					// Optimize ((T*)null) pointer operations
					//
					if (left_const.IsDefaultValue) {
						left = EmptyExpression.Null;
					} else {
						left_const = null;
					}
				}

				left.Emit (ec);

				Constant right_const = right as Constant;
				if (right_const != null) {
					//
					// Optimize 0-based arithmetic
					//
					if (right_const.IsDefaultValue)
						return;

					if (size != 0) {
						// TODO: Should be the checks resolve context sensitive?
						ResolveContext rc = new ResolveContext (ec.MemberContext);
						right = ConstantFold.BinaryFold (rc, Binary.Operator.Multiply, new IntConstant (size, right.Location), right_const, loc);
						if (right == null)
							return;
					} else {
						ig.Emit (OpCodes.Sizeof, element);
						right = EmptyExpression.Null;
					}
				}

				right.Emit (ec);
				if (rtype == TypeManager.sbyte_type || rtype == TypeManager.byte_type ||
					rtype == TypeManager.short_type || rtype == TypeManager.ushort_type) {
					ig.Emit (OpCodes.Conv_I);
				} else if (rtype == TypeManager.uint32_type) {
					ig.Emit (OpCodes.Conv_U);
				}

				if (right_const == null && size != 1){
					if (size == 0)
						ig.Emit (OpCodes.Sizeof, element);
					else 
						IntLiteral.EmitInt (ig, size);
					if (rtype == TypeManager.int64_type || rtype == TypeManager.uint64_type)
						ig.Emit (OpCodes.Conv_I8);

					Binary.EmitOperatorOpcode (ec, Binary.Operator.Multiply, rtype);
				}

				if (left_const == null) {
					if (rtype == TypeManager.int64_type)
						ig.Emit (OpCodes.Conv_I);
					else if (rtype == TypeManager.uint64_type)
						ig.Emit (OpCodes.Conv_U);

					Binary.EmitOperatorOpcode (ec, op, op_type);
				}
			}
		}
	}
	
	/// <summary>
	///   Implements the ternary conditional operator (?:)
	/// </summary>
	public class Conditional : Expression {
		Expression expr, true_expr, false_expr;
		
		public Conditional (Expression expr, Expression true_expr, Expression false_expr)
		{
			this.expr = expr;
			this.true_expr = true_expr;
			this.false_expr = false_expr;
			this.loc = expr.Location;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		public Expression TrueExpr {
			get {
				return true_expr;
			}
		}

		public Expression FalseExpr {
			get {
				return false_expr;
			}
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (3);
			args.Add (new Argument (expr.CreateExpressionTree (ec)));
			args.Add (new Argument (true_expr.CreateExpressionTree (ec)));
			args.Add (new Argument (false_expr.CreateExpressionTree (ec)));
			return CreateExpressionFactoryCall (ec, "Condition", args);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			expr = Expression.ResolveBoolean (ec, expr, loc);
			
			Assign ass = expr as Assign;
			if (ass != null && ass.Source is Constant) {
				ec.Report.Warning (665, 3, loc, "Assignment in conditional expression is always constant; did you mean to use == instead of = ?");
			}

			true_expr = true_expr.Resolve (ec);
			false_expr = false_expr.Resolve (ec);

			if (true_expr == null || false_expr == null || expr == null)
				return null;

			eclass = ExprClass.Value;
			Type true_type = true_expr.Type;
			Type false_type = false_expr.Type;
			type = true_type;

			//
			// First, if an implicit conversion exists from true_expr
			// to false_expr, then the result type is of type false_expr.Type
			//
			if (!TypeManager.IsEqual (true_type, false_type)) {
				Expression conv = Convert.ImplicitConversion (ec, true_expr, false_type, loc);
				if (conv != null) {
					//
					// Check if both can convert implicitl to each other's type
					//
					if (Convert.ImplicitConversion (ec, false_expr, true_type, loc) != null) {
						ec.Report.Error (172, loc,
							   "Can not compute type of conditional expression " +
							   "as `" + TypeManager.CSharpName (true_expr.Type) +
							   "' and `" + TypeManager.CSharpName (false_expr.Type) +
							   "' convert implicitly to each other");
						return null;
					}
					type = false_type;
					true_expr = conv;
				} else if ((conv = Convert.ImplicitConversion (ec, false_expr, true_type, loc)) != null) {
					false_expr = conv;
				} else {
					ec.Report.Error (173, loc,
						"Type of conditional expression cannot be determined because there is no implicit conversion between `{0}' and `{1}'",
						true_expr.GetSignatureForError (), false_expr.GetSignatureForError ());
					return null;
				}
			}			

			// Dead code optimalization
			Constant c = expr as Constant;
			if (c != null){
				bool is_false = c.IsDefaultValue;
				ec.Report.Warning (429, 4, is_false ? true_expr.Location : false_expr.Location, "Unreachable expression code detected");
				return ReducedExpression.Create (is_false ? false_expr : true_expr, this).Resolve (ec);
			}

			return this;
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			expr.MutateHoistedGenericType (storey);
			true_expr.MutateHoistedGenericType (storey);
			false_expr.MutateHoistedGenericType (storey);
			type = storey.MutateType (type);
		}

		public override TypeExpr ResolveAsTypeTerminal (IMemberContext ec, bool silent)
		{
			return null;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label false_target = ig.DefineLabel ();
			Label end_target = ig.DefineLabel ();

			expr.EmitBranchable (ec, false_target, false);
			true_expr.Emit (ec);

			if (type.IsInterface) {
				LocalBuilder temp = ec.GetTemporaryLocal (type);
				ig.Emit (OpCodes.Stloc, temp);
				ig.Emit (OpCodes.Ldloc, temp);
				ec.FreeTemporaryLocal (temp, type);
			}

			ig.Emit (OpCodes.Br, end_target);
			ig.MarkLabel (false_target);
			false_expr.Emit (ec);
			ig.MarkLabel (end_target);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			Conditional target = (Conditional) t;

			target.expr = expr.Clone (clonectx);
			target.true_expr = true_expr.Clone (clonectx);
			target.false_expr = false_expr.Clone (clonectx);
		}
	}

	public abstract class VariableReference : Expression, IAssignMethod, IMemoryLocation, IVariableReference {
		LocalTemporary temp;

		#region Abstract
		public abstract HoistedVariable GetHoistedVariable (AnonymousExpression ae);
		public abstract bool IsFixed { get; }
		public abstract bool IsRef { get; }
		public abstract string Name { get; }
		public abstract void SetHasAddressTaken ();

		//
		// Variable IL data, it has to be protected to encapsulate hoisted variables
		//
		protected abstract ILocalVariable Variable { get; }
		
		//
		// Variable flow-analysis data
		//
		public abstract VariableInfo VariableInfo { get; }
		#endregion

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			HoistedVariable hv = GetHoistedVariable (ec);
			if (hv != null) {
				hv.AddressOf (ec, mode);
				return;
			}

			Variable.EmitAddressOf (ec);
		}

		public HoistedVariable GetHoistedVariable (ResolveContext rc)
		{
			return GetHoistedVariable (rc.CurrentAnonymousMethod);
		}

		public HoistedVariable GetHoistedVariable (EmitContext ec)
		{
			return GetHoistedVariable (ec.CurrentAnonymousMethod);
		}

		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}

		public override void EmitSideEffect (EmitContext ec)
		{
			// do nothing
		}

		//
		// This method is used by parameters that are references, that are
		// being passed as references:  we only want to pass the pointer (that
		// is already stored in the parameter, not the address of the pointer,
		// and not the value of the variable).
		//
		public void EmitLoad (EmitContext ec)
		{
			Variable.Emit (ec);
		}

		public void Emit (EmitContext ec, bool leave_copy)
		{
			Report.Debug (64, "VARIABLE EMIT", this, Variable, type, IsRef, loc);

			HoistedVariable hv = GetHoistedVariable (ec);
			if (hv != null) {
				hv.Emit (ec, leave_copy);
				return;
			}

			EmitLoad (ec);

			if (IsRef) {
				//
				// If we are a reference, we loaded on the stack a pointer
				// Now lets load the real value
				//
				LoadFromPtr (ec.ig, type);
			}

			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);

				if (IsRef) {
					temp = new LocalTemporary (Type);
					temp.Store (ec);
				}
			}
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy,
					bool prepare_for_load)
		{
			HoistedVariable hv = GetHoistedVariable (ec);
			if (hv != null) {
				hv.EmitAssign (ec, source, leave_copy, prepare_for_load);
				return;
			}

			New n_source = source as New;
			if (n_source != null) {
				if (!n_source.Emit (ec, this)) {
					if (leave_copy)
						EmitLoad (ec);
					return;
				}
			} else {
				if (IsRef)
					EmitLoad (ec);

				source.Emit (ec);
			}

			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				if (IsRef) {
					temp = new LocalTemporary (Type);
					temp.Store (ec);
				}
			}

			if (IsRef)
				StoreFromPtr (ec.ig, type);
			else
				Variable.EmitAssign (ec);

			if (temp != null) {
				temp.Emit (ec);
				temp.Release (ec);
			}
		}

		public bool IsHoisted {
			get { return GetHoistedVariable ((AnonymousExpression) null) != null; }
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			type = storey.MutateType (type);
		}
	}

	/// <summary>
	///   Local variables
	/// </summary>
	public class LocalVariableReference : VariableReference {
		readonly string name;
		public Block Block;
		public LocalInfo local_info;
		bool is_readonly;
		bool resolved;	// TODO: merge with eclass

		public LocalVariableReference (Block block, string name, Location l)
		{
			Block = block;
			this.name = name;
			loc = l;
		}

		//
		// Setting `is_readonly' to false will allow you to create a writable
		// reference to a read-only variable.  This is used by foreach and using.
		//
		public LocalVariableReference (Block block, string name, Location l,
					       LocalInfo local_info, bool is_readonly)
			: this (block, name, l)
		{
			this.local_info = local_info;
			this.is_readonly = is_readonly;
		}

		public override VariableInfo VariableInfo {
			get { return local_info.VariableInfo; }
		}

		public override HoistedVariable GetHoistedVariable (AnonymousExpression ae)
		{
			return local_info.HoistedVariableReference;
		}

		//		
		// A local variable is always fixed
		//
		public override bool IsFixed {
			get { return true; }
		}

		public override bool IsRef {
			get { return false; }
		}

		public bool IsReadOnly {
			get { return is_readonly; }
		}

		public override string Name {
			get { return name; }
		}

		public bool VerifyAssigned (ResolveContext ec)
		{
			VariableInfo variable_info = local_info.VariableInfo;
			return variable_info == null || variable_info.IsAssigned (ec, loc);
		}

		void ResolveLocalInfo ()
		{
			if (local_info == null) {
				local_info = Block.GetLocalInfo (Name);
				type = local_info.VariableType;
				is_readonly = local_info.ReadOnly;
			}
		}

		public override void SetHasAddressTaken ()
		{
			local_info.AddressTaken = true;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			HoistedVariable hv = GetHoistedVariable (ec);
			if (hv != null)
				return hv.CreateExpressionTree (ec);

			Arguments arg = new Arguments (1);
			arg.Add (new Argument (this));
			return CreateExpressionFactoryCall (ec, "Constant", arg);
		}

		Expression DoResolveBase (ResolveContext ec)
		{
			type = local_info.VariableType;

			Expression e = Block.GetConstantExpression (Name);
			if (e != null)
				return e.Resolve (ec);

			VerifyAssigned (ec);

			//
			// If we are referencing a variable from the external block
			// flag it for capturing
			//
			if (ec.MustCaptureVariable (local_info)) {
				if (local_info.AddressTaken)
					AnonymousMethodExpression.Error_AddressOfCapturedVar (ec, this, loc);

				if (ec.IsVariableCapturingRequired) {
					AnonymousMethodStorey storey = local_info.Block.Explicit.CreateAnonymousMethodStorey (ec);
					storey.CaptureLocalVariable (ec, local_info);
				}
			}

			resolved |= ec.DoFlowAnalysis;
			eclass = ExprClass.Variable;
			return this;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (resolved)
				return this;

			ResolveLocalInfo ();
			local_info.Used = true;

			if (type == null && local_info.Type is VarExpr) {
			    local_info.VariableType = TypeManager.object_type;
				Error_VariableIsUsedBeforeItIsDeclared (ec.Report, Name);
			    return null;
			}
			
			return DoResolveBase (ec);
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			ResolveLocalInfo ();

			// is out param
			if (right_side == EmptyExpression.OutAccess)
				local_info.Used = true;

			// Infer implicitly typed local variable
			if (type == null) {
				VarExpr ve = local_info.Type as VarExpr;
				if (ve != null) {
					if (!ve.InferType (ec, right_side))
						return null;
					type = local_info.VariableType = ve.Type;
				}
			}
						
			if (is_readonly) {
				int code;
				string msg;
				if (right_side == EmptyExpression.OutAccess) {
					code = 1657; msg = "Cannot pass `{0}' as a ref or out argument because it is a `{1}'";
				} else if (right_side == EmptyExpression.LValueMemberAccess) {
					code = 1654; msg = "Cannot assign to members of `{0}' because it is a `{1}'";
				} else if (right_side == EmptyExpression.LValueMemberOutAccess) {
					code = 1655; msg = "Cannot pass members of `{0}' as ref or out arguments because it is a `{1}'";
				} else if (right_side == EmptyExpression.UnaryAddress) {
					code = 459; msg = "Cannot take the address of {1} `{0}'";
				} else {
					code = 1656; msg = "Cannot assign to `{0}' because it is a `{1}'";
				}
				ec.Report.Error (code, loc, msg, Name, local_info.GetReadOnlyContext ());
			} else if (VariableInfo != null) {
				VariableInfo.SetAssigned (ec);
			}

			return DoResolveBase (ec);
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			LocalVariableReference lvr = obj as LocalVariableReference;
			if (lvr == null)
				return false;

			return Name == lvr.Name && Block == lvr.Block;
		}

		protected override ILocalVariable Variable {
			get { return local_info; }
		}

		public override string ToString ()
		{
			return String.Format ("{0} ({1}:{2})", GetType (), Name, loc);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			LocalVariableReference target = (LocalVariableReference) t;
			
			target.Block = clonectx.LookupBlock (Block);
			if (local_info != null)
				target.local_info = clonectx.LookupVariable (local_info);
		}
	}

	/// <summary>
	///   This represents a reference to a parameter in the intermediate
	///   representation.
	/// </summary>
	public class ParameterReference : VariableReference {
		readonly ToplevelParameterInfo pi;

		public ParameterReference (ToplevelParameterInfo pi, Location loc)
		{
			this.pi = pi;
			this.loc = loc;
		}

		public override bool IsRef {
			get { return (pi.Parameter.ModFlags & Parameter.Modifier.ISBYREF) != 0; }
		}

		bool HasOutModifier {
			get { return pi.Parameter.ModFlags == Parameter.Modifier.OUT; }
		}

		public override HoistedVariable GetHoistedVariable (AnonymousExpression ae)
		{
			return pi.Parameter.HoistedVariableReference;
		}

		//
		// A ref or out parameter is classified as a moveable variable, even 
		// if the argument given for the parameter is a fixed variable
		//		
		public override bool IsFixed {
			get { return !IsRef; }
		}

		public override string Name {
			get { return Parameter.Name; }
		}

		public Parameter Parameter {
			get { return pi.Parameter; }
		}

		public override VariableInfo VariableInfo {
			get { return pi.VariableInfo; }
		}

		protected override ILocalVariable Variable {
			get { return Parameter; }
		}

		public bool IsAssigned (ResolveContext ec, Location loc)
		{
			// HACK: Variables are not captured in probing mode
			if (ec.IsInProbingMode)
				return true;
			
			if (!ec.DoFlowAnalysis || !HasOutModifier || ec.CurrentBranching.IsAssigned (VariableInfo))
				return true;

			ec.Report.Error (269, loc, "Use of unassigned out parameter `{0}'", Name);
			return false;
		}

		public override void SetHasAddressTaken ()
		{
			Parameter.HasAddressTaken = true;
		}

		void SetAssigned (ResolveContext ec)
		{
			if (HasOutModifier && ec.DoFlowAnalysis)
				ec.CurrentBranching.SetAssigned (VariableInfo);
		}

		bool DoResolveBase (ResolveContext ec)
		{
			type = pi.ParameterType;
			eclass = ExprClass.Variable;

			AnonymousExpression am = ec.CurrentAnonymousMethod;
			if (am == null)
				return true;

			Block b = ec.CurrentBlock;
			while (b != null) {
				IParameterData[] p = b.Toplevel.Parameters.FixedParameters;
				for (int i = 0; i < p.Length; ++i) {
					if (p [i] != Parameter)
						continue;

					//
					// Skip closest anonymous method parameters
					//
					if (b == ec.CurrentBlock && !am.IsIterator)
						return true;

					if (IsRef) {
						ec.Report.Error (1628, loc,
							"Parameter `{0}' cannot be used inside `{1}' when using `ref' or `out' modifier",
							Name, am.ContainerType);
					}

					b = null;
					break;
				}

				if (b != null)
					b = b.Toplevel.Parent;
			}

			if (pi.Parameter.HasAddressTaken)
				AnonymousMethodExpression.Error_AddressOfCapturedVar (ec, this, loc);

			if (ec.IsVariableCapturingRequired) {
				AnonymousMethodStorey storey = pi.Block.CreateAnonymousMethodStorey (ec);
				storey.CaptureParameter (ec, this);
			}

			return true;
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			ParameterReference pr = obj as ParameterReference;
			if (pr == null)
				return false;

			return Name == pr.Name;
		}
		
		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			// Nothing to clone
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			HoistedVariable hv = GetHoistedVariable (ec);
			if (hv != null)
				return hv.CreateExpressionTree (ec);

			return Parameter.ExpressionTreeVariableReference ();
		}

		//
		// Notice that for ref/out parameters, the type exposed is not the
		// same type exposed externally.
		//
		// for "ref int a":
		//   externally we expose "int&"
		//   here we expose       "int".
		//
		// We record this in "is_ref".  This means that the type system can treat
		// the type as it is expected, but when we generate the code, we generate
		// the alternate kind of code.
		//
		public override Expression DoResolve (ResolveContext ec)
		{
			if (!DoResolveBase (ec))
				return null;

			// HACK: Variables are not captured in probing mode
			if (ec.IsInProbingMode)
				return this;

			if (HasOutModifier && ec.DoFlowAnalysis &&
			    (!ec.OmitStructFlowAnalysis || !VariableInfo.TypeInfo.IsStruct) && !IsAssigned (ec, loc))
				return null;

			return this;
		}

		override public Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			if (!DoResolveBase (ec))
				return null;

			// HACK: parameters are not captured when probing is on
			if (!ec.IsInProbingMode)
				SetAssigned (ec);

			return this;
		}

		static public void EmitLdArg (ILGenerator ig, int x)
		{
			switch (x) {
			case 0: ig.Emit (OpCodes.Ldarg_0); break;
			case 1: ig.Emit (OpCodes.Ldarg_1); break;
			case 2: ig.Emit (OpCodes.Ldarg_2); break;
			case 3: ig.Emit (OpCodes.Ldarg_3); break;
			default:
				if (x > byte.MaxValue)
					ig.Emit (OpCodes.Ldarg, x);
				else
					ig.Emit (OpCodes.Ldarg_S, (byte) x);
				break;
			}
		}
	}
	
	/// <summary>
	///   Invocation of methods or delegates.
	/// </summary>
	public class Invocation : ExpressionStatement
	{
		protected Arguments arguments;
		protected Expression expr;
		protected MethodGroupExpr mg;
		bool arguments_resolved;
		
		//
		// arguments is an ArrayList, but we do not want to typecast,
		// as it might be null.
		//
		public Invocation (Expression expr, Arguments arguments)
		{
			SimpleName sn = expr as SimpleName;
			if (sn != null)
				this.expr = sn.GetMethodGroup ();
			else
				this.expr = expr;
			
			this.arguments = arguments;
			if (expr != null)
				loc = expr.Location;
		}

		public Invocation (Expression expr, Arguments arguments, bool arguments_resolved)
			: this (expr, arguments)
		{
			this.arguments_resolved = arguments_resolved;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args;

			//
			// Special conversion for nested expression trees
			//
			if (TypeManager.DropGenericTypeArguments (type) == TypeManager.expression_type) {
				args = new Arguments (1);
				args.Add (new Argument (this));
				return CreateExpressionFactoryCall (ec, "Quote", args);
			}

			Expression instance = mg.IsInstance ?
				mg.InstanceExpression.CreateExpressionTree (ec) :
				new NullLiteral (loc);

			args = Arguments.CreateForExpressionTree (ec, arguments,
				instance,
				mg.CreateExpressionTree (ec));

			if (mg.IsBase)
				MemberExpr.Error_BaseAccessInExpressionTree (ec, loc);

			return CreateExpressionFactoryCall (ec, "Call", args);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			// Don't resolve already resolved expression
			if (eclass != ExprClass.Invalid)
				return this;
			
			Expression expr_resolved = expr.Resolve (ec, ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);
			if (expr_resolved == null)
				return null;

			//
			// Next, evaluate all the expressions in the argument list
			//
			bool dynamic_arg = false;
			if (arguments != null && !arguments_resolved)
				arguments.Resolve (ec, out dynamic_arg);

			Type expr_type = expr_resolved.Type;
			mg = expr_resolved as MethodGroupExpr;

			if (dynamic_arg || TypeManager.IsDynamicType (expr_type)) {
				Arguments args;
				DynamicMemberBinder dmb = expr_resolved as DynamicMemberBinder;
				if (dmb != null) {
					args = dmb.Arguments;
					if (arguments != null)
						args.AddRange (arguments);
				} else if (mg == null) {
					if (arguments == null)
						args = new Arguments (1);
					else
						args = arguments;

					args.Insert (0, new Argument (expr_resolved));
					expr = null;
				} else {
					if (mg.IsBase) {
						ec.Report.Error (1971, loc,
							"The base call to method `{0}' cannot be dynamically dispatched. Consider casting the dynamic arguments or eliminating the base access",
							mg.Name);
						return null;
					}

					args = arguments;

					if (mg.IsStatic != mg.IsInstance) {
						if (args == null)
							args = new Arguments (1);

						if (mg.IsStatic) {
							args.Insert (0, new Argument (new TypeOf (new TypeExpression (mg.DeclaringType, loc), loc).Resolve (ec), Argument.AType.DynamicStatic));
						} else {
							MemberAccess ma = expr as MemberAccess;
							if (ma != null)
								args.Insert (0, new Argument (ma.Left.Resolve (ec)));
							else
								args.Insert (0, new Argument (new This (loc).Resolve (ec)));
						}
					}
				}

				return new DynamicInvocation (expr as ATypeNameExpression, args, loc).Resolve (ec);
			}

			if (mg == null) {
				if (expr_type != null && TypeManager.IsDelegateType (expr_type)){
					return (new DelegateInvocation (
						expr_resolved, arguments, loc)).Resolve (ec);
				}

				MemberExpr me = expr_resolved as MemberExpr;
				if (me == null) {
					expr_resolved.Error_UnexpectedKind (ec, ResolveFlags.MethodGroup, loc);
					return null;
				}
				
				mg = ec.LookupExtensionMethod (me.Type, me.Name, loc);
				if (mg == null) {
					ec.Report.Error (1955, loc, "The member `{0}' cannot be used as method or delegate",
						expr_resolved.GetSignatureForError ());
					return null;
				}

				((ExtensionMethodGroupExpr)mg).ExtensionExpression = me.InstanceExpression;
			}

			mg = DoResolveOverload (ec);
			if (mg == null)
				return null;

			MethodInfo method = (MethodInfo)mg;
			if (method != null) {
				type = TypeManager.TypeToCoreType (method.ReturnType);

				// TODO: this is a copy of mg.ResolveMemberAccess method
				Expression iexpr = mg.InstanceExpression;
				if (method.IsStatic) {
					if (iexpr == null ||
						iexpr is This || iexpr is EmptyExpression ||
						mg.IdenticalTypeName) {
						mg.InstanceExpression = null;
					} else {
						MemberExpr.error176 (ec, loc, mg.GetSignatureForError ());
						return null;
					}
				} else {
					if (iexpr == null || iexpr == EmptyExpression.Null) {
						SimpleName.Error_ObjectRefRequired (ec, loc, mg.GetSignatureForError ());
					}
				}
			}

			if (type.IsPointer){
				if (!ec.IsUnsafe){
					UnsafeError (ec, loc);
					return null;
				}
			}
			
			//
			// Only base will allow this invocation to happen.
			//
			if (mg.IsBase && method.IsAbstract){
				Error_CannotCallAbstractBase (ec, TypeManager.CSharpSignature (method));
				return null;
			}

			if (arguments == null && method.DeclaringType == TypeManager.object_type && method.Name == Destructor.MetadataName) {
				if (mg.IsBase)
					ec.Report.Error (250, loc, "Do not directly call your base class Finalize method. It is called automatically from your destructor");
				else
					ec.Report.Error (245, loc, "Destructors and object.Finalize cannot be called directly. Consider calling IDisposable.Dispose if available");
				return null;
			}

			IsSpecialMethodInvocation (ec, method, loc);
			
			if (mg.InstanceExpression != null)
				mg.InstanceExpression.CheckMarshalByRefAccess (ec);

			eclass = ExprClass.Value;
			return this;
		}

		protected virtual MethodGroupExpr DoResolveOverload (ResolveContext ec)
		{
			return mg.OverloadResolve (ec, ref arguments, false, loc);
		}

		public static bool IsSpecialMethodInvocation (ResolveContext ec, MethodBase method, Location loc)
		{
			if (!TypeManager.IsSpecialMethod (method))
				return false;

			ec.Report.SymbolRelatedToPreviousError (method);
			ec.Report.Error (571, loc, "`{0}': cannot explicitly call operator or accessor",
				TypeManager.CSharpSignature (method, true));
	
			return true;
		}

		static Type[] GetVarargsTypes (MethodBase mb, Arguments arguments)
		{
			AParametersCollection pd = TypeManager.GetParameterData (mb);
			
			Argument a = arguments [pd.Count - 1];
			Arglist list = (Arglist) a.Expr;

			return list.ArgumentTypes;
		}

		/// <summary>
		/// This checks the ConditionalAttribute on the method 
		/// </summary>
		public static bool IsMethodExcluded (MethodBase method, Location loc)
		{
			if (method.IsConstructor)
				return false;

			method = TypeManager.DropGenericMethodArguments (method);
			if (method.DeclaringType.Module == RootContext.ToplevelTypes.Builder) {
				IMethodData md = TypeManager.GetMethod (method);
				if (md != null)
					return md.IsExcluded ();

				// For some methods (generated by delegate class) GetMethod returns null
				// because they are not included in builder_to_method table
				return false;
			}

			return AttributeTester.IsConditionalMethodExcluded (method, loc);
		}

		/// <remarks>
		///   is_base tells whether we want to force the use of the `call'
		///   opcode instead of using callvirt.  Call is required to call
		///   a specific method, while callvirt will always use the most
		///   recent method in the vtable.
		///
		///   is_static tells whether this is an invocation on a static method
		///
		///   instance_expr is an expression that represents the instance
		///   it must be non-null if is_static is false.
		///
		///   method is the method to invoke.
		///
		///   Arguments is the list of arguments to pass to the method or constructor.
		/// </remarks>
		public static void EmitCall (EmitContext ec, bool is_base,
					     Expression instance_expr,
					     MethodBase method, Arguments Arguments, Location loc)
		{
			EmitCall (ec, is_base, instance_expr, method, Arguments, loc, false, false);
		}
		
		// `dup_args' leaves an extra copy of the arguments on the stack
		// `omit_args' does not leave any arguments at all.
		// So, basically, you could make one call with `dup_args' set to true,
		// and then another with `omit_args' set to true, and the two calls
		// would have the same set of arguments. However, each argument would
		// only have been evaluated once.
		public static void EmitCall (EmitContext ec, bool is_base,
					     Expression instance_expr,
					     MethodBase method, Arguments Arguments, Location loc,
		                             bool dup_args, bool omit_args)
		{
			ILGenerator ig = ec.ig;
			bool struct_call = false;
			bool this_call = false;
			LocalTemporary this_arg = null;

			Type decl_type = method.DeclaringType;

			if (IsMethodExcluded (method, loc))
				return;
			
			bool is_static = method.IsStatic;
			if (!is_static){
				this_call = instance_expr is This;
				if (TypeManager.IsStruct (decl_type) || TypeManager.IsEnumType (decl_type))
					struct_call = true;

				//
				// If this is ourselves, push "this"
				//
				if (!omit_args) {
					Type t = null;
					Type iexpr_type = instance_expr.Type;

					//
					// Push the instance expression
					//
					if (TypeManager.IsValueType (iexpr_type) || TypeManager.IsGenericParameter (iexpr_type)) {
						//
						// Special case: calls to a function declared in a 
						// reference-type with a value-type argument need
						// to have their value boxed.
						if (TypeManager.IsStruct (decl_type) ||
						    TypeManager.IsGenericParameter (iexpr_type)) {
							//
							// If the expression implements IMemoryLocation, then
							// we can optimize and use AddressOf on the
							// return.
							//
							// If not we have to use some temporary storage for
							// it.
							if (instance_expr is IMemoryLocation) {
								((IMemoryLocation)instance_expr).
									AddressOf (ec, AddressOp.LoadStore);
							} else {
								LocalTemporary temp = new LocalTemporary (iexpr_type);
								instance_expr.Emit (ec);
								temp.Store (ec);
								temp.AddressOf (ec, AddressOp.Load);
							}

							// avoid the overhead of doing this all the time.
							if (dup_args)
								t = TypeManager.GetReferenceType (iexpr_type);
						} else {
							instance_expr.Emit (ec);
							
							// FIXME: should use instance_expr is IMemoryLocation + constraint.
							// to help JIT to produce better code
							ig.Emit (OpCodes.Box, instance_expr.Type);
							t = TypeManager.object_type;
						}
					} else {
						instance_expr.Emit (ec);
						t = instance_expr.Type;
					}

					if (dup_args) {
						ig.Emit (OpCodes.Dup);
						if (Arguments != null && Arguments.Count != 0) {
							this_arg = new LocalTemporary (t);
							this_arg.Store (ec);
						}
					}
				}
			}

			if (!omit_args && Arguments != null)
				Arguments.Emit (ec, dup_args, this_arg);

			OpCode call_op;
			if (is_static || struct_call || is_base || (this_call && !method.IsVirtual)) {
				call_op = OpCodes.Call;
			} else {
				call_op = OpCodes.Callvirt;
				
#if GMCS_SOURCE
				if ((instance_expr != null) && (instance_expr.Type.IsGenericParameter))
					ig.Emit (OpCodes.Constrained, instance_expr.Type);
#endif
			}

			if ((method.CallingConvention & CallingConventions.VarArgs) != 0) {
				Type[] varargs_types = GetVarargsTypes (method, Arguments);
				ig.EmitCall (call_op, (MethodInfo) method, varargs_types);
				return;
			}

			//
			// If you have:
			// this.DoFoo ();
			// and DoFoo is not virtual, you can omit the callvirt,
			// because you don't need the null checking behavior.
			//
			if (method is MethodInfo)
				ig.Emit (call_op, (MethodInfo) method);
			else
				ig.Emit (call_op, (ConstructorInfo) method);
		}

		public override void Emit (EmitContext ec)
		{
			mg.EmitCall (ec, arguments);
		}
		
		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec);

			// 
			// Pop the return value if there is one
			//
			if (TypeManager.TypeToCoreType (type) != TypeManager.void_type)
				ec.ig.Emit (OpCodes.Pop);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			Invocation target = (Invocation) t;

			if (arguments != null)
				target.arguments = arguments.Clone (clonectx);

			target.expr = expr.Clone (clonectx);
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			mg.MutateHoistedGenericType (storey);
			type = storey.MutateType (type);
			if (arguments != null) {
				arguments.MutateHoistedGenericType (storey);
			}
		}
	}
/*
	//
	// It's either a cast or delegate invocation
	//
	public class InvocationOrCast : ExpressionStatement
	{
		Expression expr;
		Expression argument;

		public InvocationOrCast (Expression expr, Expression argument)
		{
			this.expr = expr;
			this.argument = argument;
			this.loc = expr.Location;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			Expression e = ResolveCore (ec);
			if (e == null)
				return null;

			return e.Resolve (ec);
		}

		Expression ResolveCore (EmitContext ec)
		{
			//
			// First try to resolve it as a cast.
			//
			TypeExpr te = expr.ResolveAsBaseTerminal (ec, true);
			if (te != null) {
				return new Cast (te, argument, loc);
			}

			//
			// This can either be a type or a delegate invocation.
			// Let's just resolve it and see what we'll get.
			//
			expr = expr.Resolve (ec, ResolveFlags.Type | ResolveFlags.VariableOrValue);
			if (expr == null)
				return null;

			//
			// Ok, so it's a Cast.
			//
			if (expr.eclass == ExprClass.Type || expr.eclass == ExprClass.TypeParameter) {
				return new Cast (expr, argument, loc);
			}

			if (expr.eclass == ExprClass.Namespace) {
				expr.Error_UnexpectedKind (null, "type", loc);
				return null;
			}			

			//
			// It's a delegate invocation.
			//
			if (!TypeManager.IsDelegateType (expr.Type)) {
				Error (149, "Method name expected");
				return null;
			}

			ArrayList args = new ArrayList (1);
			args.Add (new Argument (argument, Argument.AType.Expression));
			return new DelegateInvocation (expr, args, loc);
		}

		public override ExpressionStatement ResolveStatement (EmitContext ec)
		{
			Expression e = ResolveCore (ec);
			if (e == null)
				return null;

			ExpressionStatement s = e as ExpressionStatement;
			if (s == null) {
				Error_InvalidExpressionStatement ();
				return null;
			}

			return s.ResolveStatement (ec);
		}

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Cannot happen");
		}

		public override void EmitStatement (EmitContext ec)
		{
			throw new Exception ("Cannot happen");
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			InvocationOrCast target = (InvocationOrCast) t;

			target.expr = expr.Clone (clonectx);
			target.argument = argument.Clone (clonectx);
		}
	}
*/

	/// <summary>
	///    Implements the new expression 
	/// </summary>
	public class New : ExpressionStatement, IMemoryLocation {
		protected Arguments Arguments;

		//
		// During bootstrap, it contains the RequestedType,
		// but if `type' is not null, it *might* contain a NewDelegate
		// (because of field multi-initialization)
		//
		protected Expression RequestedType;

		protected MethodGroupExpr method;

		bool is_type_parameter;

		public New (Expression requested_type, Arguments arguments, Location l)
		{
			RequestedType = requested_type;
			Arguments = arguments;
			loc = l;
		}

		/// <summary>
		/// Converts complex core type syntax like 'new int ()' to simple constant
		/// </summary>
		public static Constant Constantify (Type t)
		{
			if (t == TypeManager.int32_type)
				return new IntConstant (0, Location.Null);
			if (t == TypeManager.uint32_type)
				return new UIntConstant (0, Location.Null);
			if (t == TypeManager.int64_type)
				return new LongConstant (0, Location.Null);
			if (t == TypeManager.uint64_type)
				return new ULongConstant (0, Location.Null);
			if (t == TypeManager.float_type)
				return new FloatConstant (0, Location.Null);
			if (t == TypeManager.double_type)
				return new DoubleConstant (0, Location.Null);
			if (t == TypeManager.short_type)
				return new ShortConstant (0, Location.Null);
			if (t == TypeManager.ushort_type)
				return new UShortConstant (0, Location.Null);
			if (t == TypeManager.sbyte_type)
				return new SByteConstant (0, Location.Null);
			if (t == TypeManager.byte_type)
				return new ByteConstant (0, Location.Null);
			if (t == TypeManager.char_type)
				return new CharConstant ('\0', Location.Null);
			if (t == TypeManager.bool_type)
				return new BoolConstant (false, Location.Null);
			if (t == TypeManager.decimal_type)
				return new DecimalConstant (0, Location.Null);
			if (TypeManager.IsEnumType (t))
				return new EnumConstant (Constantify (TypeManager.GetEnumUnderlyingType (t)), t);
			if (TypeManager.IsNullableType (t))
				return Nullable.LiftedNull.Create (t, Location.Null);

			return null;
		}

		//
		// Checks whether the type is an interface that has the
		// [ComImport, CoClass] attributes and must be treated
		// specially
		//
		public Expression CheckComImport (ResolveContext ec)
		{
			if (!type.IsInterface)
				return null;

			//
			// Turn the call into:
			// (the-interface-stated) (new class-referenced-in-coclassattribute ())
			//
			Type real_class = AttributeTester.GetCoClassAttribute (type);
			if (real_class == null)
				return null;

			New proxy = new New (new TypeExpression (real_class, loc), Arguments, loc);
			Cast cast = new Cast (new TypeExpression (type, loc), proxy, loc);
			return cast.Resolve (ec);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args;
			if (method == null) {
				args = new Arguments (1);
				args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));
			} else {
				args = Arguments.CreateForExpressionTree (ec,
					Arguments,
					method.CreateExpressionTree (ec));
			}

			return CreateExpressionFactoryCall (ec, "New", args);
		}
		
		public override Expression DoResolve (ResolveContext ec)
		{
			//
			// The New DoResolve might be called twice when initializing field
			// expressions (see EmitFieldInitializers, the call to
			// GetInitializerExpression will perform a resolve on the expression,
			// and later the assign will trigger another resolution
			//
			// This leads to bugs (#37014)
			//
			if (type != null){
				if (RequestedType is NewDelegate)
					return RequestedType;
				return this;
			}

			TypeExpr texpr = RequestedType.ResolveAsTypeTerminal (ec, false);
			if (texpr == null)
				return null;

			type = texpr.Type;

			if (type.IsPointer) {
				ec.Report.Error (1919, loc, "Unsafe type `{0}' cannot be used in an object creation expression",
					TypeManager.CSharpName (type));
				return null;
			}

			if (Arguments == null) {
				Constant c = Constantify (type);
				if (c != null)
					return ReducedExpression.Create (c, this);
			}

			if (TypeManager.IsDelegateType (type)) {
				return (new NewDelegate (type, Arguments, loc)).Resolve (ec);
			}

			if (TypeManager.IsGenericParameter (type)) {
				GenericConstraints gc = TypeManager.GetTypeParameterConstraints (type);

				if ((gc == null) || (!gc.HasConstructorConstraint && !gc.IsValueType)) {
					ec.Report.Error (304, loc,
						"Cannot create an instance of the variable type '{0}' because it doesn't have the new() constraint",
						TypeManager.CSharpName (type));
					return null;
				}

				if ((Arguments != null) && (Arguments.Count != 0)) {
					ec.Report.Error (417, loc,
						"`{0}': cannot provide arguments when creating an instance of a variable type",
						TypeManager.CSharpName (type));
					return null;
				}

				if (TypeManager.activator_create_instance == null) {
					Type activator_type = TypeManager.CoreLookupType (ec.Compiler, "System", "Activator", Kind.Class, true);
					if (activator_type != null) {
						TypeManager.activator_create_instance = TypeManager.GetPredefinedMethod (
							activator_type, "CreateInstance", loc, Type.EmptyTypes);
					}
				}

				is_type_parameter = true;
				eclass = ExprClass.Value;
				return this;
			}

			if (type.IsAbstract && type.IsSealed) {
				ec.Report.SymbolRelatedToPreviousError (type);
				ec.Report.Error (712, loc, "Cannot create an instance of the static class `{0}'", TypeManager.CSharpName (type));
				return null;
			}

			if (type.IsInterface || type.IsAbstract){
				if (!TypeManager.IsGenericType (type)) {
					RequestedType = CheckComImport (ec);
					if (RequestedType != null)
						return RequestedType;
				}
				
				ec.Report.SymbolRelatedToPreviousError (type);
				ec.Report.Error (144, loc, "Cannot create an instance of the abstract class or interface `{0}'", TypeManager.CSharpName (type));
				return null;
			}

			bool is_struct = TypeManager.IsStruct (type);
			eclass = ExprClass.Value;

			//
			// SRE returns a match for .ctor () on structs (the object constructor), 
			// so we have to manually ignore it.
			//
			if (is_struct && Arguments == null)
				return this;

			// For member-lookup, treat 'new Foo (bar)' as call to 'foo.ctor (bar)', where 'foo' is of type 'Foo'.
			Expression ml = MemberLookupFinal (ec, type, type, ConstructorInfo.ConstructorName,
				MemberTypes.Constructor, AllBindingFlags | BindingFlags.DeclaredOnly, loc);

			if (Arguments != null) {
				bool dynamic;
				Arguments.Resolve (ec, out dynamic);

				if (dynamic) {
					Arguments.Insert (0, new Argument (new TypeOf (texpr, loc).Resolve (ec)));
					return new DynamicInvocation (new SimpleName (ConstructorInfo.ConstructorName, loc), Arguments, type, loc).Resolve (ec);
				}
			}

			if (ml == null)
				return null;

			method = ml as MethodGroupExpr;
			if (method == null) {
				ml.Error_UnexpectedKind (ec, ResolveFlags.MethodGroup, loc);
				return null;
			}

			method = method.OverloadResolve (ec, ref Arguments, false, loc);
			if (method == null)
				return null;

			return this;
		}

		bool DoEmitTypeParameter (EmitContext ec)
		{
#if GMCS_SOURCE
			ILGenerator ig = ec.ig;
//			IMemoryLocation ml;

			MethodInfo ci = TypeManager.activator_create_instance.MakeGenericMethod (
				new Type [] { type });

			GenericConstraints gc = TypeManager.GetTypeParameterConstraints (type);
			if (gc.HasReferenceTypeConstraint || gc.HasClassConstraint) {
				ig.Emit (OpCodes.Call, ci);
				return true;
			}

			// Allow DoEmit() to be called multiple times.
			// We need to create a new LocalTemporary each time since
			// you can't share LocalBuilders among ILGeneators.
			LocalTemporary temp = new LocalTemporary (type);

			Label label_activator = ig.DefineLabel ();
			Label label_end = ig.DefineLabel ();

			temp.AddressOf (ec, AddressOp.Store);
			ig.Emit (OpCodes.Initobj, type);

			temp.Emit (ec);
			ig.Emit (OpCodes.Box, type);
			ig.Emit (OpCodes.Brfalse, label_activator);

			temp.AddressOf (ec, AddressOp.Store);
			ig.Emit (OpCodes.Initobj, type);
			temp.Emit (ec);
			ig.Emit (OpCodes.Br_S, label_end);

			ig.MarkLabel (label_activator);

			ig.Emit (OpCodes.Call, ci);
			ig.MarkLabel (label_end);
			return true;
#else
			throw new InternalErrorException ();
#endif
		}

		//
		// This Emit can be invoked in two contexts:
		//    * As a mechanism that will leave a value on the stack (new object)
		//    * As one that wont (init struct)
		//
		// If we are dealing with a ValueType, we have a few
		// situations to deal with:
		//
		//    * The target is a ValueType, and we have been provided
		//      the instance (this is easy, we are being assigned).
		//
		//    * The target of New is being passed as an argument,
		//      to a boxing operation or a function that takes a
		//      ValueType.
		//
		//      In this case, we need to create a temporary variable
		//      that is the argument of New.
		//
		// Returns whether a value is left on the stack
		//
		// *** Implementation note ***
		//
		// To benefit from this optimization, each assignable expression
		// has to manually cast to New and call this Emit.
		//
		// TODO: It's worth to implement it for arrays and fields
		//
		public virtual bool Emit (EmitContext ec, IMemoryLocation target)
		{
			bool is_value_type = TypeManager.IsValueType (type);
			ILGenerator ig = ec.ig;
			VariableReference vr = target as VariableReference;

			if (target != null && is_value_type && (vr != null || method == null)) {
				target.AddressOf (ec, AddressOp.Store);
			} else if (vr != null && vr.IsRef) {
				vr.EmitLoad (ec);
			}
			
			if (Arguments != null)
				Arguments.Emit (ec);

			if (is_value_type) {
				if (method == null) {
					ig.Emit (OpCodes.Initobj, type);
					return false;
				}

				if (vr != null) {
					ig.Emit (OpCodes.Call, (ConstructorInfo) method);
					return false;
				}
			}
			
			if (is_type_parameter)
				return DoEmitTypeParameter (ec);			

			ConstructorInfo ci = (ConstructorInfo) method;
#if MS_COMPATIBLE
			if (TypeManager.IsGenericType (type) && type.IsGenericTypeDefinition)
				ci = TypeBuilder.GetConstructor (type, ci);
#endif

			ig.Emit (OpCodes.Newobj, ci);
			return true;
		}

		public override void Emit (EmitContext ec)
		{
			LocalTemporary v = null;
			if (method == null && TypeManager.IsValueType (type)) {
				// TODO: Use temporary variable from pool
				v = new LocalTemporary (type);
			}

			if (!Emit (ec, v))
				v.Emit (ec);
		}
		
		public override void EmitStatement (EmitContext ec)
		{
			LocalTemporary v = null;
			if (method == null && TypeManager.IsValueType (type)) {
				// TODO: Use temporary variable from pool
				v = new LocalTemporary (type);
			}

			if (Emit (ec, v))
				ec.ig.Emit (OpCodes.Pop);
		}

		public bool IsDefaultValueType {
			get {
				return TypeManager.IsValueType (type) && !HasInitializer && Arguments == null;
			}
		}

		public virtual bool HasInitializer {
			get {
				return false;
			}
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			EmitAddressOf (ec, mode);
		}

		protected virtual IMemoryLocation EmitAddressOf (EmitContext ec, AddressOp mode)
		{
			LocalTemporary value_target = new LocalTemporary (type);

			if (is_type_parameter) {
				DoEmitTypeParameter (ec);
				value_target.Store (ec);
				value_target.AddressOf (ec, mode);
				return value_target;
			}

			if (!TypeManager.IsStruct (type)){
				//
				// We throw an exception.  So far, I believe we only need to support
				// value types:
				// foreach (int j in new StructType ())
				// see bug 42390
				//
				throw new Exception ("AddressOf should not be used for classes");
			}

			value_target.AddressOf (ec, AddressOp.Store);

			if (method == null) {
				ec.ig.Emit (OpCodes.Initobj, type);
			} else {
				if (Arguments != null)
					Arguments.Emit (ec);

				ec.ig.Emit (OpCodes.Call, (ConstructorInfo) method);
			}
			
			value_target.AddressOf (ec, mode);
			return value_target;
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			New target = (New) t;

			target.RequestedType = RequestedType.Clone (clonectx);
			if (Arguments != null){
				target.Arguments = Arguments.Clone (clonectx);
			}
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			if (method != null) {
				method.MutateHoistedGenericType (storey);
				if (Arguments != null) {
					Arguments.MutateHoistedGenericType (storey);
				}
			}

			type = storey.MutateType (type);
		}
	}

	/// <summary>
	///   14.5.10.2: Represents an array creation expression.
	/// </summary>
	///
	/// <remarks>
	///   There are two possible scenarios here: one is an array creation
	///   expression that specifies the dimensions and optionally the
	///   initialization data and the other which does not need dimensions
	///   specified but where initialization data is mandatory.
	/// </remarks>
	public class ArrayCreation : Expression {
		FullNamedExpression requested_base_type;
		ArrayList initializers;

		//
		// The list of Argument types.
		// This is used to construct the `newarray' or constructor signature
		//
		protected ArrayList arguments;
		
		protected Type array_element_type;
		bool expect_initializers = false;
		int num_arguments = 0;
		protected int dimensions;
		protected readonly string rank;

		protected ArrayList array_data;

		IDictionary bounds;

		// The number of constants in array initializers
		int const_initializers_count;
		bool only_constant_initializers;
		
		public ArrayCreation (FullNamedExpression requested_base_type, ArrayList exprs, string rank, ArrayList initializers, Location l)
		{
			this.requested_base_type = requested_base_type;
			this.initializers = initializers;
			this.rank = rank;
			loc = l;

			arguments = new ArrayList (exprs.Count);

			foreach (Expression e in exprs) {
				arguments.Add (e);
				num_arguments++;
			}
		}

		public ArrayCreation (FullNamedExpression requested_base_type, string rank, ArrayList initializers, Location l)
		{
			this.requested_base_type = requested_base_type;
			this.initializers = initializers;
			this.rank = rank;
			loc = l;

			//this.rank = rank.Substring (0, rank.LastIndexOf ('['));
			//
			//string tmp = rank.Substring (rank.LastIndexOf ('['));
			//
			//dimensions = tmp.Length - 1;
			expect_initializers = true;
		}

		protected override void Error_NegativeArrayIndex (ResolveContext ec, Location loc)
		{
			ec.Report.Error (248, loc, "Cannot create an array with a negative size");
		}

		bool CheckIndices (ResolveContext ec, ArrayList probe, int idx, bool specified_dims, int child_bounds)
		{
			if (specified_dims) { 
				Expression a = (Expression) arguments [idx];
				a = a.Resolve (ec);
				if (a == null)
					return false;

				Constant c = a as Constant;
				if (c != null) {
					c = c.ImplicitConversionRequired (ec, TypeManager.int32_type, a.Location);
				}

				if (c == null) {
					ec.Report.Error (150, a.Location, "A constant value is expected");
					return false;
				}

				int value = (int) c.GetValue ();
				
				if (value != probe.Count) {
					ec.Report.Error (847, loc, "An array initializer of length `{0}' was expected", value);
					return false;
				}
				
				bounds [idx] = value;
			}

			only_constant_initializers = true;
			for (int i = 0; i < probe.Count; ++i) {
				object o = probe [i];
				if (o is ArrayList) {
					ArrayList sub_probe = o as ArrayList;
					if (idx + 1 >= dimensions){
						ec.Report.Error (623, loc, "Array initializers can only be used in a variable or field initializer. Try using a new expression instead");
						return false;
					}
					
					bool ret = CheckIndices (ec, sub_probe, idx + 1, specified_dims, child_bounds - 1);
					if (!ret)
						return false;
				} else if (child_bounds > 1) {
					ec.Report.Error (846, ((Expression) o).Location, "A nested array initializer was expected");
				} else {
					Expression element = ResolveArrayElement (ec, (Expression) o);
					if (element == null)
						continue;

					// Initializers with the default values can be ignored
					Constant c = element as Constant;
					if (c != null) {
						if (c.IsDefaultInitializer (array_element_type)) {
							element = null;
						}
						else {
							++const_initializers_count;
						}
					} else {
						only_constant_initializers = false;
					}
					
					array_data.Add (element);
				}
			}

			return true;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args;

			if (array_data == null) {
				args = new Arguments (arguments.Count + 1);
				args.Add (new Argument (new TypeOf (new TypeExpression (array_element_type, loc), loc)));
				foreach (Expression a in arguments)
					args.Add (new Argument (a.CreateExpressionTree (ec)));

				return CreateExpressionFactoryCall (ec, "NewArrayBounds", args);
			}

			if (dimensions > 1) {
				ec.Report.Error (838, loc, "An expression tree cannot contain a multidimensional array initializer");
				return null;
			}

			args = new Arguments (array_data == null ? 1 : array_data.Count + 1);
			args.Add (new Argument (new TypeOf (new TypeExpression (array_element_type, loc), loc)));
			if (array_data != null) {
				for (int i = 0; i < array_data.Count; ++i) {
					Expression e = (Expression) array_data [i];
					if (e == null)
						e = Convert.ImplicitConversion (ec, (Expression) initializers [i], array_element_type, loc);

					args.Add (new Argument (e.CreateExpressionTree (ec)));
				}
			}

			return CreateExpressionFactoryCall (ec, "NewArrayInit", args);
		}		
		
		public void UpdateIndices ()
		{
			int i = 0;
			for (ArrayList probe = initializers; probe != null;) {
				if (probe.Count > 0 && probe [0] is ArrayList) {
					Expression e = new IntConstant (probe.Count, Location.Null);
					arguments.Add (e);

					bounds [i++] =  probe.Count;
					
					probe = (ArrayList) probe [0];
					
				} else {
					Expression e = new IntConstant (probe.Count, Location.Null);
					arguments.Add (e);

					bounds [i++] = probe.Count;
					return;
				}
			}

		}

		Expression first_emit;
		LocalTemporary first_emit_temp;

		protected virtual Expression ResolveArrayElement (ResolveContext ec, Expression element)
		{
			element = element.Resolve (ec);
			if (element == null)
				return null;

			if (element is CompoundAssign.TargetExpression) {
				if (first_emit != null)
					throw new InternalErrorException ("Can only handle one mutator at a time");
				first_emit = element;
				element = first_emit_temp = new LocalTemporary (element.Type);
			}

			return Convert.ImplicitConversionRequired (
				ec, element, array_element_type, loc);
		}

		protected bool ResolveInitializers (ResolveContext ec)
		{
			if (initializers == null) {
				return !expect_initializers;
			}
						
			//
			// We use this to store all the date values in the order in which we
			// will need to store them in the byte blob later
			//
			array_data = new ArrayList ();
			bounds = new System.Collections.Specialized.HybridDictionary ();
			
			if (arguments != null)
				return CheckIndices (ec, initializers, 0, true, dimensions);

			arguments = new ArrayList ();

			if (!CheckIndices (ec, initializers, 0, false, dimensions))
				return false;
				
			UpdateIndices ();
				
			return true;
		}

		//
		// Resolved the type of the array
		//
		bool ResolveArrayType (ResolveContext ec)
		{
			if (requested_base_type == null) {
				ec.Report.Error (622, loc, "Can only use array initializer expressions to assign to array types. Try using a new expression instead");
				return false;
			}

			if (requested_base_type is VarExpr) {
				ec.Report.Error (820, loc, "An implicitly typed local variable declarator cannot use an array initializer");
				return false;
			}
			
			StringBuilder array_qualifier = new StringBuilder (rank);

			//
			// `In the first form allocates an array instace of the type that results
			// from deleting each of the individual expression from the expression list'
			//
			if (num_arguments > 0) {
				array_qualifier.Append ("[");
				for (int i = num_arguments-1; i > 0; i--)
					array_qualifier.Append (",");
				array_qualifier.Append ("]");
			}

			//
			// Lookup the type
			//
			TypeExpr array_type_expr;
			array_type_expr = new ComposedCast (requested_base_type, array_qualifier.ToString (), loc);
			array_type_expr = array_type_expr.ResolveAsTypeTerminal (ec, false);
			if (array_type_expr == null)
				return false;

			type = array_type_expr.Type;
			array_element_type = TypeManager.GetElementType (type);
			dimensions = type.GetArrayRank ();

			return true;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (type != null)
				return this;

			if (!ResolveArrayType (ec))
				return null;

			//
			// First step is to validate the initializers and fill
			// in any missing bits
			//
			if (!ResolveInitializers (ec))
				return null;

			for (int i = 0; i < arguments.Count; ++i) {
				Expression e = ((Expression) arguments[i]).Resolve (ec);
				if (e == null)
					continue;

				arguments [i] = ConvertExpressionToArrayIndex (ec, e);
			}
							
			eclass = ExprClass.Value;
			return this;
		}

		MethodInfo GetArrayMethod (int arguments)
		{
			ModuleBuilder mb = RootContext.ToplevelTypes.Builder;

			Type[] arg_types = new Type[arguments];
			for (int i = 0; i < arguments; i++)
				arg_types[i] = TypeManager.int32_type;

			MethodInfo mi = mb.GetArrayMethod (type, ".ctor", CallingConventions.HasThis, null,
							arg_types);

			if (mi == null) {
				RootContext.ToplevelTypes.Compiler.Report.Error (-6, "New invocation: Can not find a constructor for " +
						  "this argument list");
				return null;
			}

			return mi; 
		}

		byte [] MakeByteBlob ()
		{
			int factor;
			byte [] data;
			byte [] element;
			int count = array_data.Count;

			if (TypeManager.IsEnumType (array_element_type))
				array_element_type = TypeManager.GetEnumUnderlyingType (array_element_type);
			
			factor = GetTypeSize (array_element_type);
			if (factor == 0)
				throw new Exception ("unrecognized type in MakeByteBlob: " + array_element_type);

			data = new byte [(count * factor + 3) & ~3];
			int idx = 0;

			for (int i = 0; i < count; ++i) {
				object v = array_data [i];

				if (v is EnumConstant)
					v = ((EnumConstant) v).Child;
				
				if (v is Constant && !(v is StringConstant))
					v = ((Constant) v).GetValue ();
				else {
					idx += factor;
					continue;
				}
				
				if (array_element_type == TypeManager.int64_type){
					if (!(v is Expression)){
						long val = (long) v;
						
						for (int j = 0; j < factor; ++j) {
							data [idx + j] = (byte) (val & 0xFF);
							val = (val >> 8);
						}
					}
				} else if (array_element_type == TypeManager.uint64_type){
					if (!(v is Expression)){
						ulong val = (ulong) v;

						for (int j = 0; j < factor; ++j) {
							data [idx + j] = (byte) (val & 0xFF);
							val = (val >> 8);
						}
					}
				} else if (array_element_type == TypeManager.float_type) {
					if (!(v is Expression)){
						element = BitConverter.GetBytes ((float) v);
							
						for (int j = 0; j < factor; ++j)
							data [idx + j] = element [j];
						if (!BitConverter.IsLittleEndian)
							System.Array.Reverse (data, idx, 4);
					}
				} else if (array_element_type == TypeManager.double_type) {
					if (!(v is Expression)){
						element = BitConverter.GetBytes ((double) v);

						for (int j = 0; j < factor; ++j)
							data [idx + j] = element [j];

						// FIXME: Handle the ARM float format.
						if (!BitConverter.IsLittleEndian)
							System.Array.Reverse (data, idx, 8);
					}
				} else if (array_element_type == TypeManager.char_type){
					if (!(v is Expression)){
						int val = (int) ((char) v);
						
						data [idx] = (byte) (val & 0xff);
						data [idx+1] = (byte) (val >> 8);
					}
				} else if (array_element_type == TypeManager.short_type){
					if (!(v is Expression)){
						int val = (int) ((short) v);
					
						data [idx] = (byte) (val & 0xff);
						data [idx+1] = (byte) (val >> 8);
					}
				} else if (array_element_type == TypeManager.ushort_type){
					if (!(v is Expression)){
						int val = (int) ((ushort) v);
					
						data [idx] = (byte) (val & 0xff);
						data [idx+1] = (byte) (val >> 8);
					}
				} else if (array_element_type == TypeManager.int32_type) {
					if (!(v is Expression)){
						int val = (int) v;
					
						data [idx]   = (byte) (val & 0xff);
						data [idx+1] = (byte) ((val >> 8) & 0xff);
						data [idx+2] = (byte) ((val >> 16) & 0xff);
						data [idx+3] = (byte) (val >> 24);
					}
				} else if (array_element_type == TypeManager.uint32_type) {
					if (!(v is Expression)){
						uint val = (uint) v;
					
						data [idx]   = (byte) (val & 0xff);
						data [idx+1] = (byte) ((val >> 8) & 0xff);
						data [idx+2] = (byte) ((val >> 16) & 0xff);
						data [idx+3] = (byte) (val >> 24);
					}
				} else if (array_element_type == TypeManager.sbyte_type) {
					if (!(v is Expression)){
						sbyte val = (sbyte) v;
						data [idx] = (byte) val;
					}
				} else if (array_element_type == TypeManager.byte_type) {
					if (!(v is Expression)){
						byte val = (byte) v;
						data [idx] = (byte) val;
					}
				} else if (array_element_type == TypeManager.bool_type) {
					if (!(v is Expression)){
						bool val = (bool) v;
						data [idx] = (byte) (val ? 1 : 0);
					}
				} else if (array_element_type == TypeManager.decimal_type){
					if (!(v is Expression)){
						int [] bits = Decimal.GetBits ((decimal) v);
						int p = idx;

						// FIXME: For some reason, this doesn't work on the MS runtime.
						int [] nbits = new int [4];
						nbits [0] = bits [3];
						nbits [1] = bits [2];
						nbits [2] = bits [0];
						nbits [3] = bits [1];
						
						for (int j = 0; j < 4; j++){
							data [p++] = (byte) (nbits [j] & 0xff);
							data [p++] = (byte) ((nbits [j] >> 8) & 0xff);
							data [p++] = (byte) ((nbits [j] >> 16) & 0xff);
							data [p++] = (byte) (nbits [j] >> 24);
						}
					}
				} else
					throw new Exception ("Unrecognized type in MakeByteBlob: " + array_element_type);

                                idx += factor;
			}

			return data;
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			array_element_type = storey.MutateType (array_element_type);
			type = storey.MutateType (type);
			if (arguments != null) {
				foreach (Expression e in arguments)
					e.MutateHoistedGenericType (storey);
			}
			
			if (array_data != null) {
				foreach (Expression e in array_data) {
					// Don't mutate values optimized away
					if (e == null)
						continue;

					e.MutateHoistedGenericType (storey);
				}
			}
		}

		//
		// Emits the initializers for the array
		//
		void EmitStaticInitializers (EmitContext ec)
		{
			// FIXME: This should go to Resolve !
			if (TypeManager.void_initializearray_array_fieldhandle == null) {
				TypeManager.void_initializearray_array_fieldhandle = TypeManager.GetPredefinedMethod (
					TypeManager.runtime_helpers_type, "InitializeArray", loc,
					TypeManager.array_type, TypeManager.runtime_field_handle_type);
				if (TypeManager.void_initializearray_array_fieldhandle == null)
					return;
			}

			//
			// First, the static data
			//
			FieldBuilder fb;
			ILGenerator ig = ec.ig;
			
			byte [] data = MakeByteBlob ();

			fb = RootContext.MakeStaticData (data);

			ig.Emit (OpCodes.Dup);
			ig.Emit (OpCodes.Ldtoken, fb);
			ig.Emit (OpCodes.Call,
				 TypeManager.void_initializearray_array_fieldhandle);
		}

		//
		// Emits pieces of the array that can not be computed at compile
		// time (variables and string locations).
		//
		// This always expect the top value on the stack to be the array
		//
		void EmitDynamicInitializers (EmitContext ec, bool emitConstants)
		{
			ILGenerator ig = ec.ig;
			int dims = bounds.Count;
			int [] current_pos = new int [dims];

			MethodInfo set = null;

			if (dims != 1){
				Type [] args = new Type [dims + 1];

				for (int j = 0; j < dims; j++)
					args [j] = TypeManager.int32_type;
				args [dims] = array_element_type;

				set = RootContext.ToplevelTypes.Builder.GetArrayMethod (
					type, "Set",
					CallingConventions.HasThis | CallingConventions.Standard,
					TypeManager.void_type, args);
			}

			for (int i = 0; i < array_data.Count; i++){

				Expression e = (Expression)array_data [i];

				// Constant can be initialized via StaticInitializer
				if (e != null && !(!emitConstants && e is Constant)) {
					Type etype = e.Type;

					ig.Emit (OpCodes.Dup);

					for (int idx = 0; idx < dims; idx++) 
						IntConstant.EmitInt (ig, current_pos [idx]);

					//
					// If we are dealing with a struct, get the
					// address of it, so we can store it.
					//
					if ((dims == 1) && TypeManager.IsStruct (etype) &&
					    (!TypeManager.IsBuiltinOrEnum (etype) ||
					     etype == TypeManager.decimal_type)) {

						ig.Emit (OpCodes.Ldelema, etype);
					}

					e.Emit (ec);

					if (dims == 1) {
						bool is_stobj, has_type_arg;
						OpCode op = ArrayAccess.GetStoreOpcode (etype, out is_stobj, out has_type_arg);
						if (is_stobj)
							ig.Emit (OpCodes.Stobj, etype);
						else if (has_type_arg)
							ig.Emit (op, etype);
						else
							ig.Emit (op);
					} else 
						ig.Emit (OpCodes.Call, set);

				}
				
				//
				// Advance counter
				//
				for (int j = dims - 1; j >= 0; j--){
					current_pos [j]++;
					if (current_pos [j] < (int) bounds [j])
						break;
					current_pos [j] = 0;
				}
			}
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (first_emit != null) {
				first_emit.Emit (ec);
				first_emit_temp.Store (ec);
			}

			foreach (Expression e in arguments)
				e.Emit (ec);

			if (arguments.Count == 1)
				ig.Emit (OpCodes.Newarr, array_element_type);
			else {
				ig.Emit (OpCodes.Newobj, GetArrayMethod (arguments.Count));
			}
			
			if (initializers == null)
				return;

			// Emit static initializer for arrays which have contain more than 4 items and
			// the static initializer will initialize at least 25% of array values.
			// NOTE: const_initializers_count does not contain default constant values.
			if (const_initializers_count >= 4 && const_initializers_count * 4 > (array_data.Count) &&
				TypeManager.IsPrimitiveType (array_element_type)) {
				EmitStaticInitializers (ec);

				if (!only_constant_initializers)
					EmitDynamicInitializers (ec, false);
			} else {
				EmitDynamicInitializers (ec, true);
			}

			if (first_emit_temp != null)
				first_emit_temp.Release (ec);
		}

		public override bool GetAttributableValue (ResolveContext ec, Type value_type, out object value)
		{
			if (arguments.Count != 1) {
				// ec.Report.Error (-211, Location, "attribute can not encode multi-dimensional arrays");
				return base.GetAttributableValue (ec, null, out value);
			}

			if (array_data == null) {
				Expression arg = (Expression) arguments[0];
				object arg_value;
				if (arg.GetAttributableValue (ec, arg.Type, out arg_value) && arg_value is int && (int)arg_value == 0) {
					value = Array.CreateInstance (array_element_type, 0);
					return true;
				}

				// ec.Report.Error (-212, Location, "array should be initialized when passing it to an attribute");
				return base.GetAttributableValue (ec, null, out value);
			}
			
			Array ret = Array.CreateInstance (array_element_type, array_data.Count);
			object element_value;
			for (int i = 0; i < ret.Length; ++i)
			{
				Expression e = (Expression)array_data [i];

				// Is null when an initializer is optimized (value == predefined value)
				if (e == null) 
					continue;

				if (!e.GetAttributableValue (ec, array_element_type, out element_value)) {
					value = null;
					return false;
				}
				ret.SetValue (element_value, i);
			}
			value = ret;
			return true;
		}
		
		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			ArrayCreation target = (ArrayCreation) t;

			if (requested_base_type != null)
				target.requested_base_type = (FullNamedExpression)requested_base_type.Clone (clonectx);

			if (arguments != null){
				target.arguments = new ArrayList (arguments.Count);
				foreach (Expression e in arguments)
					target.arguments.Add (e.Clone (clonectx));
			}

			if (initializers != null){
				target.initializers = new ArrayList (initializers.Count);
				foreach (object initializer in initializers)
					if (initializer is ArrayList) {
						ArrayList this_al = (ArrayList)initializer;
						ArrayList al = new ArrayList (this_al.Count);
						target.initializers.Add (al);
						foreach (Expression e in this_al)
							al.Add (e.Clone (clonectx));
					} else {
						target.initializers.Add (((Expression)initializer).Clone (clonectx));
					}
			}
		}
	}
	
	//
	// Represents an implicitly typed array epxression
	//
	public class ImplicitlyTypedArrayCreation : ArrayCreation
	{
		public ImplicitlyTypedArrayCreation (string rank, ArrayList initializers, Location loc)
			: base (null, rank, initializers, loc)
		{			
			if (rank.Length > 2) {
				while (rank [++dimensions] == ',');
			} else {
				dimensions = 1;
			}
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (type != null)
				return this;

			if (!ResolveInitializers (ec))
				return null;

			if (array_element_type == null || array_element_type == TypeManager.null_type ||
				array_element_type == TypeManager.void_type || array_element_type == InternalType.AnonymousMethod ||
				array_element_type == InternalType.MethodGroup ||
				arguments.Count != dimensions) {
				Error_NoBestType (ec);
				return null;
			}

			//
			// At this point we found common base type for all initializer elements
			// but we have to be sure that all static initializer elements are of
			// same type
			//
			UnifyInitializerElement (ec);

			type = TypeManager.GetConstructedType (array_element_type, rank);
			eclass = ExprClass.Value;
			return this;
		}

		void Error_NoBestType (ResolveContext ec)
		{
			ec.Report.Error (826, loc,
				"The type of an implicitly typed array cannot be inferred from the initializer. Try specifying array type explicitly");
		}

		//
		// Converts static initializer only
		//
		void UnifyInitializerElement (ResolveContext ec)
		{
			for (int i = 0; i < array_data.Count; ++i) {
				Expression e = (Expression)array_data[i];
				if (e != null)
					array_data [i] = Convert.ImplicitConversion (ec, e, array_element_type, Location.Null);
			}
		}

		protected override Expression ResolveArrayElement (ResolveContext ec, Expression element)
		{
			element = element.Resolve (ec);
			if (element == null)
				return null;
			
			if (array_element_type == null) {
				if (element.Type != TypeManager.null_type)
					array_element_type = element.Type;

				return element;
			}

			if (Convert.ImplicitConversionExists (ec, element, array_element_type)) {
				return element;
			}

			if (Convert.ImplicitConversionExists (ec, new TypeExpression (array_element_type, loc), element.Type)) {
				array_element_type = element.Type;
				return element;
			}

			Error_NoBestType (ec);
			return null;
		}
	}	
	
	public sealed class CompilerGeneratedThis : This
	{
		public static This Instance = new CompilerGeneratedThis ();

		private CompilerGeneratedThis ()
			: base (Location.Null)
		{
		}

		public CompilerGeneratedThis (Type type, Location loc)
			: base (loc)
		{
			this.type = type;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			eclass = ExprClass.Variable;
			if (type == null)
				type = ec.CurrentType;

			is_struct = type.IsValueType;
			return this;
		}

		public override HoistedVariable GetHoistedVariable (AnonymousExpression ae)
		{
			return null;
		}
	}
	
	/// <summary>
	///   Represents the `this' construct
	/// </summary>

	public class This : VariableReference
	{
		sealed class ThisVariable : ILocalVariable
		{
			public static readonly ILocalVariable Instance = new ThisVariable ();

			public void Emit (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldarg_0);
			}

			public void EmitAssign (EmitContext ec)
			{
				throw new InvalidOperationException ();
			}

			public void EmitAddressOf (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldarg_0);
			}
		}

		Block block;
		VariableInfo variable_info;
		protected bool is_struct;

		public This (Block block, Location loc)
		{
			this.loc = loc;
			this.block = block;
		}

		public This (Location loc)
		{
			this.loc = loc;
		}

		public override VariableInfo VariableInfo {
			get { return variable_info; }
		}

		public override bool IsFixed {
			get { return false; }
		}

		public override HoistedVariable GetHoistedVariable (AnonymousExpression ae)
		{
			if (ae == null)
				return null;

			AnonymousMethodStorey storey = ae.Storey;
			while (storey != null) {
				AnonymousMethodStorey temp = storey.Parent as AnonymousMethodStorey;
				if (temp == null)
					return storey.HoistedThis;

				storey = temp;
			}

			return null;
		}

		public override bool IsRef {
			get { return is_struct; }
		}

		protected override ILocalVariable Variable {
			get { return ThisVariable.Instance; }
		}

		public static bool IsThisAvailable (ResolveContext ec)
		{
			if (ec.IsStatic || ec.HasAny (ResolveContext.Options.FieldInitializerScope | ResolveContext.Options.BaseInitializer | ResolveContext.Options.ConstantScope))
				return false;

			if (ec.CurrentAnonymousMethod == null)
				return true;

			if (ec.CurrentType.IsValueType && ec.CurrentIterator == null)
				return false;

			return true;
		}

		public bool ResolveBase (ResolveContext ec)
		{
			if (eclass != ExprClass.Invalid)
				return true;

			eclass = ExprClass.Variable;
			type = ec.CurrentType;

			if (!IsThisAvailable (ec)) {
				if (ec.IsStatic && !ec.HasSet (ResolveContext.Options.ConstantScope)) {
					ec.Report.Error (26, loc, "Keyword `this' is not valid in a static property, static method, or static field initializer");
				} else if (ec.CurrentAnonymousMethod != null) {
					ec.Report.Error (1673, loc,
						"Anonymous methods inside structs cannot access instance members of `this'. " +
						"Consider copying `this' to a local variable outside the anonymous method and using the local instead");
				} else {
					ec.Report.Error (27, loc, "Keyword `this' is not available in the current context");
				}
			}

			is_struct = type.IsValueType;

			if (block != null) {
				if (block.Toplevel.ThisVariable != null)
					variable_info = block.Toplevel.ThisVariable.VariableInfo;

				AnonymousExpression am = ec.CurrentAnonymousMethod;
				if (am != null && ec.IsVariableCapturingRequired) {
					am.SetHasThisAccess ();
				}
			}
			
			return true;
		}

		//
		// Called from Invocation to check if the invocation is correct
		//
		public override void CheckMarshalByRefAccess (ResolveContext ec)
		{
			if ((variable_info != null) && !(TypeManager.IsStruct (type) && ec.OmitStructFlowAnalysis) &&
			    !variable_info.IsAssigned (ec)) {
				ec.Report.Error (188, loc,
					"The `this' object cannot be used before all of its fields are assigned to");
				variable_info.SetAssigned (ec);
			}
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (1);
			args.Add (new Argument (this));
			
			// Use typeless constant for ldarg.0 to save some
			// space and avoid problems with anonymous stories
			return CreateExpressionFactoryCall (ec, "Constant", args);
		}
		
		public override Expression DoResolve (ResolveContext ec)
		{
			ResolveBase (ec);
			return this;
		}

		override public Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			if (!ResolveBase (ec))
				return null;

			if (variable_info != null)
				variable_info.SetAssigned (ec);

			if (ec.CurrentType.IsClass){
				if (right_side == EmptyExpression.UnaryAddress)
					ec.Report.Error (459, loc, "Cannot take the address of `this' because it is read-only");
				else if (right_side == EmptyExpression.OutAccess)
					ec.Report.Error (1605, loc, "Cannot pass `this' as a ref or out argument because it is read-only");
				else
					ec.Report.Error (1604, loc, "Cannot assign to `this' because it is read-only");
			}

			return this;
		}

		public override int GetHashCode()
		{
			return block.GetHashCode ();
		}

		public override string Name {
			get { return "this"; }
		}

		public override bool Equals (object obj)
		{
			This t = obj as This;
			if (t == null)
				return false;

			return block == t.block;
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			This target = (This) t;

			target.block = clonectx.LookupBlock (block);
		}

		public override void SetHasAddressTaken ()
		{
			// Nothing
		}
	}

	/// <summary>
	///   Represents the `__arglist' construct
	/// </summary>
	public class ArglistAccess : Expression
	{
		public ArglistAccess (Location loc)
		{
			this.loc = loc;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			eclass = ExprClass.Variable;
			type = TypeManager.runtime_argument_handle_type;

			if (ec.HasSet (ResolveContext.Options.FieldInitializerScope) || !ec.CurrentBlock.Toplevel.Parameters.HasArglist) {
				ec.Report.Error (190, loc,
					"The __arglist construct is valid only within a variable argument method");
			}

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Arglist);
		}

		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			// nothing.
		}
	}

	/// <summary>
	///   Represents the `__arglist (....)' construct
	/// </summary>
	class Arglist : Expression
	{
		Arguments Arguments;

		public Arglist (Location loc)
			: this (null, loc)
		{
		}

		public Arglist (Arguments args, Location l)
		{
			Arguments = args;
			loc = l;
		}

		public Type[] ArgumentTypes {
		    get {
				if (Arguments == null)
					return Type.EmptyTypes;

		        Type[] retval = new Type [Arguments.Count];
		        for (int i = 0; i < retval.Length; i++)
		            retval [i] = Arguments [i].Expr.Type;

		        return retval;
		    }
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			ec.Report.Error (1952, loc, "An expression tree cannot contain a method with variable arguments");
			return null;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			eclass = ExprClass.Variable;
			type = InternalType.Arglist;
			if (Arguments != null) {
				bool dynamic;	// Can be ignored as there is always only 1 overload
				Arguments.Resolve (ec, out dynamic);
			}

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			if (Arguments != null)
				Arguments.Emit (ec);
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			if (Arguments != null)
				Arguments.MutateHoistedGenericType (storey);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			Arglist target = (Arglist) t;

			if (Arguments != null)
				target.Arguments = Arguments.Clone (clonectx);
		}
	}

	/// <summary>
	///   Implements the typeof operator
	/// </summary>
	public class TypeOf : Expression {
		Expression QueriedType;
		protected Type typearg;
		
		public TypeOf (Expression queried_type, Location l)
		{
			QueriedType = queried_type;
			loc = l;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (2);
			args.Add (new Argument (this));
			args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));
			return CreateExpressionFactoryCall (ec, "Constant", args);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (eclass != ExprClass.Invalid)
				return this;

			TypeExpr texpr = QueriedType.ResolveAsTypeTerminal (ec, false);
			if (texpr == null)
				return null;

			typearg = texpr.Type;

			if (typearg == TypeManager.void_type) {
				ec.Report.Error (673, loc, "System.Void cannot be used from C#. Use typeof (void) to get the void type object");
			} else if (typearg.IsPointer && !ec.IsUnsafe){
				UnsafeError (ec, loc);
			} else if (texpr is DynamicTypeExpr) {
				ec.Report.Error (1962, QueriedType.Location,
					"The typeof operator cannot be used on the dynamic type");
			}

			type = TypeManager.type_type;

			return DoResolveBase ();
		}

		protected Expression DoResolveBase ()
		{
			if (TypeManager.system_type_get_type_from_handle == null) {
				TypeManager.system_type_get_type_from_handle = TypeManager.GetPredefinedMethod (
					TypeManager.type_type, "GetTypeFromHandle", loc, TypeManager.runtime_handle_type);
			}

			// Even though what is returned is a type object, it's treated as a value by the compiler.
			// In particular, 'typeof (Foo).X' is something totally different from 'Foo.X'.
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldtoken, TypeManager.TypeToReflectionType (typearg));
			ec.ig.Emit (OpCodes.Call, TypeManager.system_type_get_type_from_handle);
		}

		public override bool GetAttributableValue (ResolveContext ec, Type value_type, out object value)
		{
			if (TypeManager.ContainsGenericParameters (typearg) &&
				!TypeManager.IsGenericTypeDefinition (typearg)) {
				ec.Report.SymbolRelatedToPreviousError (typearg);
				ec.Report.Error (416, loc, "`{0}': an attribute argument cannot use type parameters",
					     TypeManager.CSharpName (typearg));
				value = null;
				return false;
			}

			if (value_type == TypeManager.object_type) {
				value = (object)typearg;
				return true;
			}
			value = typearg;
			return true;
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			if (!TypeManager.IsGenericTypeDefinition (typearg))
				typearg = storey.MutateType (typearg);
		}

		public Type TypeArgument {
			get {
				return typearg;
			}
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			TypeOf target = (TypeOf) t;
			if (QueriedType != null)
				target.QueriedType = QueriedType.Clone (clonectx);
		}
	}

	/// <summary>
	///   Implements the `typeof (void)' operator
	/// </summary>
	public class TypeOfVoid : TypeOf {
		public TypeOfVoid (Location l) : base (null, l)
		{
			loc = l;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.type_type;
			typearg = TypeManager.void_type;

			return DoResolveBase ();
		}
	}

	class TypeOfMethod : TypeOfMember
	{
		public TypeOfMethod (MethodBase method, Location loc)
			: base (method, loc)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (member is MethodInfo) {
				type = TypeManager.methodinfo_type;
				if (type == null)
					type = TypeManager.methodinfo_type = TypeManager.CoreLookupType (ec.Compiler, "System.Reflection", "MethodInfo", Kind.Class, true);
			} else {
				type = TypeManager.ctorinfo_type;
				if (type == null)
					type = TypeManager.ctorinfo_type = TypeManager.CoreLookupType (ec.Compiler, "System.Reflection", "ConstructorInfo", Kind.Class, true);
			}

			return base.DoResolve (ec);
		}

		public override void Emit (EmitContext ec)
		{
			if (member is ConstructorInfo)
				ec.ig.Emit (OpCodes.Ldtoken, (ConstructorInfo) member);
			else
				ec.ig.Emit (OpCodes.Ldtoken, (MethodInfo) member);

			base.Emit (ec);
			ec.ig.Emit (OpCodes.Castclass, type);
		}

		protected override string GetMethodName {
			get { return "GetMethodFromHandle"; }
		}

		protected override string RuntimeHandleName {
			get { return "RuntimeMethodHandle"; }
		}

		protected override MethodInfo TypeFromHandle {
			get {
				return TypeManager.methodbase_get_type_from_handle;
			}
			set {
				TypeManager.methodbase_get_type_from_handle = value;
			}
		}

		protected override MethodInfo TypeFromHandleGeneric {
			get {
				return TypeManager.methodbase_get_type_from_handle_generic;
			}
			set {
				TypeManager.methodbase_get_type_from_handle_generic = value;
			}
		}

		protected override string TypeName {
			get { return "MethodBase"; }
		}
	}

	abstract class TypeOfMember : Expression
	{
		protected readonly MemberInfo member;

		protected TypeOfMember (MemberInfo member, Location loc)
		{
			this.member = member;
			this.loc = loc;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (2);
			args.Add (new Argument (this));
			args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));
			return CreateExpressionFactoryCall (ec, "Constant", args);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			bool is_generic = TypeManager.IsGenericType (member.DeclaringType);
			MethodInfo mi = is_generic ? TypeFromHandleGeneric : TypeFromHandle;

			if (mi == null) {
				Type t = TypeManager.CoreLookupType (ec.Compiler, "System.Reflection", TypeName, Kind.Class, true);
				Type handle_type = TypeManager.CoreLookupType (ec.Compiler, "System", RuntimeHandleName, Kind.Class, true);

				if (t == null || handle_type == null)
					return null;

				mi = TypeManager.GetPredefinedMethod (t, GetMethodName, loc,
					is_generic ?
					new Type[] { handle_type, TypeManager.runtime_handle_type } :
					new Type[] { handle_type } );

				if (is_generic)
					TypeFromHandleGeneric = mi;
				else
					TypeFromHandle = mi;
			}

			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			bool is_generic = TypeManager.IsGenericType (member.DeclaringType);
			MethodInfo mi;
			if (is_generic) {
				mi = TypeFromHandleGeneric;
				ec.ig.Emit (OpCodes.Ldtoken, member.DeclaringType);
			} else {
				mi = TypeFromHandle;
			}

			ec.ig.Emit (OpCodes.Call, mi);
		}

		protected abstract string GetMethodName { get; }
		protected abstract string RuntimeHandleName { get; }
		protected abstract MethodInfo TypeFromHandle { get; set; }
		protected abstract MethodInfo TypeFromHandleGeneric { get; set; }
		protected abstract string TypeName { get; }
	}

	class TypeOfField : TypeOfMember
	{
		public TypeOfField (FieldInfo field, Location loc)
			: base (field, loc)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (TypeManager.fieldinfo_type == null)
				TypeManager.fieldinfo_type = TypeManager.CoreLookupType (ec.Compiler, "System.Reflection", TypeName, Kind.Class, true);

			type = TypeManager.fieldinfo_type;
			return base.DoResolve (ec);
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldtoken, (FieldInfo) member);
			base.Emit (ec);
		}

		protected override string GetMethodName {
			get { return "GetFieldFromHandle"; }
		}

		protected override string RuntimeHandleName {
			get { return "RuntimeFieldHandle"; }
		}

		protected override MethodInfo TypeFromHandle {
			get {
				return TypeManager.fieldinfo_get_field_from_handle;
			}
			set {
				TypeManager.fieldinfo_get_field_from_handle = value;
			}
		}

		protected override MethodInfo TypeFromHandleGeneric {
			get {
				return TypeManager.fieldinfo_get_field_from_handle_generic;
			}
			set {
				TypeManager.fieldinfo_get_field_from_handle_generic = value;
			}
		}

		protected override string TypeName {
			get { return "FieldInfo"; }
		}
	}

	/// <summary>
	///   Implements the sizeof expression
	/// </summary>
	public class SizeOf : Expression {
		readonly Expression QueriedType;
		Type type_queried;
		
		public SizeOf (Expression queried_type, Location l)
		{
			this.QueriedType = queried_type;
			loc = l;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Error_PointerInsideExpressionTree (ec);
			return null;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			TypeExpr texpr = QueriedType.ResolveAsTypeTerminal (ec, false);
			if (texpr == null)
				return null;

			type_queried = texpr.Type;
			if (TypeManager.IsEnumType (type_queried))
				type_queried = TypeManager.GetEnumUnderlyingType (type_queried);

			int size_of = GetTypeSize (type_queried);
			if (size_of > 0) {
				return new IntConstant (size_of, loc);
			}

			if (!TypeManager.VerifyUnManaged (type_queried, loc)){
				return null;
			}

			if (!ec.IsUnsafe) {
				ec.Report.Error (233, loc,
					"`{0}' does not have a predefined size, therefore sizeof can only be used in an unsafe context (consider using System.Runtime.InteropServices.Marshal.SizeOf)",
					TypeManager.CSharpName (type_queried));
			}
			
			type = TypeManager.int32_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			int size = GetTypeSize (type_queried);

			if (size == 0)
				ec.ig.Emit (OpCodes.Sizeof, type_queried);
			else
				IntConstant.EmitInt (ec.ig, size);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
		}
	}

	/// <summary>
	///   Implements the qualified-alias-member (::) expression.
	/// </summary>
	public class QualifiedAliasMember : MemberAccess
	{
		readonly string alias;
		public static readonly string GlobalAlias = "global";

		public QualifiedAliasMember (string alias, string identifier, TypeArguments targs, Location l)
			: base (null, identifier, targs, l)
		{
			this.alias = alias;
		}

		public QualifiedAliasMember (string alias, string identifier, Location l)
			: base (null, identifier, l)
		{
			this.alias = alias;
		}

		public override FullNamedExpression ResolveAsTypeStep (IMemberContext ec, bool silent)
		{
			if (alias == GlobalAlias) {
				expr = GlobalRootNamespace.Instance;
				return base.ResolveAsTypeStep (ec, silent);
			}

			int errors = ec.Compiler.Report.Errors;
			expr = ec.LookupNamespaceAlias (alias);
			if (expr == null) {
				if (errors == ec.Compiler.Report.Errors)
					ec.Compiler.Report.Error (432, loc, "Alias `{0}' not found", alias);
				return null;
			}

			FullNamedExpression fne = base.ResolveAsTypeStep (ec, silent);
			if (fne == null)
				return null;

			if (expr.eclass == ExprClass.Type) {
				if (!silent) {
					ec.Compiler.Report.Error (431, loc,
						"Alias `{0}' cannot be used with '::' since it denotes a type. Consider replacing '::' with '.'", alias);
				}
				return null;
			}

			return fne;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			return ResolveAsTypeStep (ec, false);
		}

		protected override void Error_IdentifierNotFound (IMemberContext rc, FullNamedExpression expr_type, string identifier)
		{
			rc.Compiler.Report.Error (687, loc,
				"A namespace alias qualifier `{0}' did not resolve to a namespace or a type",
				GetSignatureForError ());
		}

		public override string GetSignatureForError ()
		{
			string name = Name;
			if (targs != null) {
				name = TypeManager.RemoveGenericArity (Name) + "<" +
					targs.GetSignatureForError () + ">";
			}

			return alias + "::" + name;
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			// Nothing 
		}
	}

	/// <summary>
	///   Implements the member access expression
	/// </summary>
	public class MemberAccess : ATypeNameExpression {
		protected Expression expr;

		public MemberAccess (Expression expr, string id)
			: base (id, expr.Location)
		{
			this.expr = expr;
		}

		public MemberAccess (Expression expr, string identifier, Location loc)
			: base (identifier, loc)
		{
			this.expr = expr;
		}

		public MemberAccess (Expression expr, string identifier, TypeArguments args, Location loc)
			: base (identifier, args, loc)
		{
			this.expr = expr;
		}

		Expression DoResolve (ResolveContext ec, Expression right_side)
		{
			if (type != null)
				throw new Exception ();

			//
			// Resolve the expression with flow analysis turned off, we'll do the definite
			// assignment checks later.  This is because we don't know yet what the expression
			// will resolve to - it may resolve to a FieldExpr and in this case we must do the
			// definite assignment check on the actual field and not on the whole struct.
			//

			SimpleName original = expr as SimpleName;
			Expression expr_resolved = expr.Resolve (ec,
				ResolveFlags.VariableOrValue | ResolveFlags.Type |
				ResolveFlags.Intermediate | ResolveFlags.DisableStructFlowAnalysis);

			if (expr_resolved == null)
				return null;

			string LookupIdentifier = MemberName.MakeName (Name, targs);

			Namespace ns = expr_resolved as Namespace;
			if (ns != null) {
				FullNamedExpression retval = ns.Lookup (ec.Compiler, LookupIdentifier, loc);

				if (retval == null)
					ns.Error_NamespaceDoesNotExist (loc, LookupIdentifier, ec.Report);
				else if (targs != null)
					retval = new GenericTypeExpr (retval.Type, targs, loc).ResolveAsTypeStep (ec, false);

				return retval;
			}

			Type expr_type = expr_resolved.Type;
			if (TypeManager.IsDynamicType (expr_type)) {
				Arguments args = new Arguments (2);
				args.Add (new Argument (expr_resolved.Resolve (ec)));
				if (right_side != null)
					args.Add (new Argument (right_side));

				return new DynamicMemberBinder (right_side != null, Name, args, loc).Resolve (ec);
			}

			if (expr_type.IsPointer || expr_type == TypeManager.void_type ||
				expr_type == TypeManager.null_type || expr_type == InternalType.AnonymousMethod) {
				Unary.Error_OperatorCannotBeApplied (ec, loc, ".", expr_type);
				return null;
			}

			Constant c = expr_resolved as Constant;
			if (c != null && c.GetValue () == null) {
				ec.Report.Warning (1720, 1, loc, "Expression will always cause a `{0}'",
					"System.NullReferenceException");
			}

			if (targs != null) {
				if (!targs.Resolve (ec))
					return null;
			}

			Expression member_lookup;
			member_lookup = MemberLookup (ec.Compiler,
				ec.CurrentType, expr_type, expr_type, Name, loc);

			if (member_lookup == null && targs != null) {
				member_lookup = MemberLookup (ec.Compiler,
					ec.CurrentType, expr_type, expr_type, LookupIdentifier, loc);
			}

			if (member_lookup == null) {
				ExprClass expr_eclass = expr_resolved.eclass;

				//
				// Extension methods are not allowed on all expression types
				//
				if (expr_eclass == ExprClass.Value || expr_eclass == ExprClass.Variable ||
					expr_eclass == ExprClass.IndexerAccess || expr_eclass == ExprClass.PropertyAccess ||
					expr_eclass == ExprClass.EventAccess) {
					ExtensionMethodGroupExpr ex_method_lookup = ec.LookupExtensionMethod (expr_type, Name, loc);
					if (ex_method_lookup != null) {
						ex_method_lookup.ExtensionExpression = expr_resolved;

						if (targs != null) {
							ex_method_lookup.SetTypeArguments (ec, targs);
						}

						return ex_method_lookup.DoResolve (ec);
					}
				}

				expr = expr_resolved;
				member_lookup = Error_MemberLookupFailed (ec,
					ec.CurrentType, expr_type, expr_type, Name, null,
					AllMemberTypes, AllBindingFlags);
				if (member_lookup == null)
					return null;
			}

			TypeExpr texpr = member_lookup as TypeExpr;
			if (texpr != null) {
				if (!(expr_resolved is TypeExpr) && 
				    (original == null || !original.IdenticalNameAndTypeName (ec, expr_resolved, loc))) {
					ec.Report.Error (572, loc, "`{0}': cannot reference a type through an expression; try `{1}' instead",
						Name, member_lookup.GetSignatureForError ());
					return null;
				}

				if (!texpr.CheckAccessLevel (ec.MemberContext)) {
					ec.Report.SymbolRelatedToPreviousError (member_lookup.Type);
					ErrorIsInaccesible (loc, TypeManager.CSharpName (member_lookup.Type), ec.Report);
					return null;
				}

				GenericTypeExpr ct = expr_resolved as GenericTypeExpr;
				if (ct != null) {
					//
					// When looking up a nested type in a generic instance
					// via reflection, we always get a generic type definition
					// and not a generic instance - so we have to do this here.
					//
					// See gtest-172-lib.cs and gtest-172.cs for an example.
					//

					TypeArguments nested_targs;
					if (HasTypeArguments) {
						nested_targs = ct.TypeArguments.Clone ();
						nested_targs.Add (targs);
					} else {
						nested_targs = ct.TypeArguments;
					}

					ct = new GenericTypeExpr (member_lookup.Type, nested_targs, loc);

					return ct.ResolveAsTypeStep (ec, false);
				}

				return member_lookup;
			}

			MemberExpr me = (MemberExpr) member_lookup;
			me = me.ResolveMemberAccess (ec, expr_resolved, loc, original);
			if (me == null)
				return null;

			if (targs != null) {
				me.SetTypeArguments (ec, targs);
			}

			if (original != null && !TypeManager.IsValueType (expr_type)) {
				if (me.IsInstance) {
					LocalVariableReference var = expr_resolved as LocalVariableReference;
					if (var != null && !var.VerifyAssigned (ec))
						return null;
				}
			}

			// The following DoResolve/DoResolveLValue will do the definite assignment
			// check.

			if (right_side != null)
				return me.DoResolveLValue (ec, right_side);
			else
				return me.DoResolve (ec);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			return DoResolve (ec, null);
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			return DoResolve (ec, right_side);
		}

		public override FullNamedExpression ResolveAsTypeStep (IMemberContext ec, bool silent)
		{
			return ResolveNamespaceOrType (ec, silent);
		}

		public FullNamedExpression ResolveNamespaceOrType (IMemberContext rc, bool silent)
		{
			FullNamedExpression expr_resolved = expr.ResolveAsTypeStep (rc, silent);

			if (expr_resolved == null)
				return null;

			string LookupIdentifier = MemberName.MakeName (Name, targs);

			Namespace ns = expr_resolved as Namespace;
			if (ns != null) {
				FullNamedExpression retval = ns.Lookup (rc.Compiler, LookupIdentifier, loc);

				if (retval == null && !silent)
					ns.Error_NamespaceDoesNotExist (loc, LookupIdentifier, rc.Compiler.Report);
				else if (targs != null)
					retval = new GenericTypeExpr (retval.Type, targs, loc).ResolveAsTypeStep (rc, silent);

				return retval;
			}

			TypeExpr tnew_expr = expr_resolved.ResolveAsTypeTerminal (rc, false);
			if (tnew_expr == null)
				return null;

			Type expr_type = tnew_expr.Type;
			if (TypeManager.IsGenericParameter (expr_type)) {
				rc.Compiler.Report.Error (704, loc, "A nested type cannot be specified through a type parameter `{0}'",
					tnew_expr.GetSignatureForError ());
				return null;
			}

			Expression member_lookup = MemberLookup (rc.Compiler,
				rc.CurrentType, expr_type, expr_type, LookupIdentifier,
				MemberTypes.NestedType, BindingFlags.Public | BindingFlags.NonPublic, loc);
			if (member_lookup == null) {
				if (silent)
					return null;

				Error_IdentifierNotFound (rc, expr_resolved, LookupIdentifier);
				return null;
			}

			TypeExpr texpr = member_lookup.ResolveAsTypeTerminal (rc, false);
			if (texpr == null)
				return null;

			TypeArguments the_args = targs;
			Type declaring_type = texpr.Type.DeclaringType;
			if (TypeManager.HasGenericArguments (declaring_type) && !TypeManager.IsGenericTypeDefinition (expr_type)) {
				while (!TypeManager.IsEqual (TypeManager.DropGenericTypeArguments (expr_type), declaring_type)) {
					expr_type = expr_type.BaseType;
				}
				
				TypeArguments new_args = new TypeArguments ();
				foreach (Type decl in TypeManager.GetTypeArguments (expr_type))
					new_args.Add (new TypeExpression (TypeManager.TypeToCoreType (decl), loc));

				if (targs != null)
					new_args.Add (targs);

				the_args = new_args;
			}

			if (the_args != null) {
				GenericTypeExpr ctype = new GenericTypeExpr (texpr.Type, the_args, loc);
				return ctype.ResolveAsTypeStep (rc, false);
			}

			return texpr;
		}

		protected virtual void Error_IdentifierNotFound (IMemberContext rc, FullNamedExpression expr_type, string identifier)
		{
			Expression member_lookup = MemberLookup (rc.Compiler,
				rc.CurrentType, expr_type.Type, expr_type.Type, SimpleName.RemoveGenericArity (identifier),
				MemberTypes.NestedType, BindingFlags.Public | BindingFlags.NonPublic, loc);

			if (member_lookup != null) {
				expr_type = member_lookup.ResolveAsTypeTerminal (rc, false);
				if (expr_type == null)
					return;

				Namespace.Error_TypeArgumentsCannotBeUsed (expr_type, loc);
				return;
			}

			member_lookup = MemberLookup (rc.Compiler,
				rc.CurrentType, expr_type.Type, expr_type.Type, identifier,
					MemberTypes.All, BindingFlags.Public | BindingFlags.NonPublic, loc);

			if (member_lookup == null) {
				rc.Compiler.Report.Error (426, loc, "The nested type `{0}' does not exist in the type `{1}'",
						  Name, expr_type.GetSignatureForError ());
			} else {
				// TODO: Report.SymbolRelatedToPreviousError
				member_lookup.Error_UnexpectedKind (rc.Compiler.Report, null, "type", loc);
			}
		}

		protected override void Error_TypeDoesNotContainDefinition (ResolveContext ec, Type type, string name)
		{
			if (RootContext.Version > LanguageVersion.ISO_2 &&
				((expr.eclass & (ExprClass.Value | ExprClass.Variable)) != 0)) {
				ec.Report.Error (1061, loc, "Type `{0}' does not contain a definition for `{1}' and no " +
					"extension method `{1}' of type `{0}' could be found " +
					"(are you missing a using directive or an assembly reference?)",
					TypeManager.CSharpName (type), name);
				return;
			}

			base.Error_TypeDoesNotContainDefinition (ec, type, name);
		}

		public override string GetSignatureForError ()
		{
			return expr.GetSignatureForError () + "." + base.GetSignatureForError ();
		}

		public Expression Left {
			get {
				return expr;
			}
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			MemberAccess target = (MemberAccess) t;

			target.expr = expr.Clone (clonectx);
		}
	}

	/// <summary>
	///   Implements checked expressions
	/// </summary>
	public class CheckedExpr : Expression {

		public Expression Expr;

		public CheckedExpr (Expression e, Location l)
		{
			Expr = e;
			loc = l;
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			using (ec.With (ResolveContext.Options.AllCheckStateFlags, true))
				return Expr.CreateExpressionTree (ec);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			using (ec.With (ResolveContext.Options.AllCheckStateFlags, true))
				Expr = Expr.Resolve (ec);
			
			if (Expr == null)
				return null;

			if (Expr is Constant || Expr is MethodGroupExpr || Expr is AnonymousMethodExpression || Expr is DefaultValueExpression)
				return Expr;
			
			eclass = Expr.eclass;
			type = Expr.Type;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			using (ec.With (EmitContext.Options.AllCheckStateFlags, true))
				Expr.Emit (ec);
		}

		public override void EmitBranchable (EmitContext ec, Label target, bool on_true)
		{
			using (ec.With (EmitContext.Options.AllCheckStateFlags, true))
				Expr.EmitBranchable (ec, target, on_true);
		}

#if NET_4_0
		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			using (ctx.With (BuilderContext.Options.AllCheckStateFlags, true)) {
				return Expr.MakeExpression (ctx);
			}
		}
#endif

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			Expr.MutateHoistedGenericType (storey);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			CheckedExpr target = (CheckedExpr) t;

			target.Expr = Expr.Clone (clonectx);
		}
	}

	/// <summary>
	///   Implements the unchecked expression
	/// </summary>
	public class UnCheckedExpr : Expression {

		public Expression Expr;

		public UnCheckedExpr (Expression e, Location l)
		{
			Expr = e;
			loc = l;
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			using (ec.With (ResolveContext.Options.AllCheckStateFlags, false))
				return Expr.CreateExpressionTree (ec);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			using (ec.With (ResolveContext.Options.AllCheckStateFlags, false))
				Expr = Expr.Resolve (ec);

			if (Expr == null)
				return null;

			if (Expr is Constant || Expr is MethodGroupExpr || Expr is AnonymousMethodExpression || Expr is DefaultValueExpression)
				return Expr;
			
			eclass = Expr.eclass;
			type = Expr.Type;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			using (ec.With (EmitContext.Options.AllCheckStateFlags, false))
				Expr.Emit (ec);
		}
		
		public override void EmitBranchable (EmitContext ec, Label target, bool on_true)
		{
			using (ec.With (EmitContext.Options.AllCheckStateFlags, false))
				Expr.EmitBranchable (ec, target, on_true);
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			Expr.MutateHoistedGenericType (storey);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			UnCheckedExpr target = (UnCheckedExpr) t;

			target.Expr = Expr.Clone (clonectx);
		}
	}

	/// <summary>
	///   An Element Access expression.
	///
	///   During semantic analysis these are transformed into 
	///   IndexerAccess, ArrayAccess or a PointerArithmetic.
	/// </summary>
	public class ElementAccess : Expression {
		public Arguments Arguments;
		public Expression Expr;

		public ElementAccess (Expression e, Arguments args)
		{
			Expr = e;
			loc  = e.Location;
			this.Arguments = args;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree (ec, Arguments,
				Expr.CreateExpressionTree (ec));

			return CreateExpressionFactoryCall (ec, "ArrayIndex", args);
		}

		Expression MakePointerAccess (ResolveContext ec, Type t)
		{
			if (Arguments.Count != 1){
				ec.Report.Error (196, loc, "A pointer must be indexed by only one value");
				return null;
			}

			if (Arguments [0] is NamedArgument)
				Error_NamedArgument ((NamedArgument) Arguments[0], ec.Report);

			Expression p = new PointerArithmetic (Binary.Operator.Addition, Expr, Arguments [0].Expr.Resolve (ec), t, loc);
			return new Indirection (p, loc).Resolve (ec);
		}
		
		public override Expression DoResolve (ResolveContext ec)
		{
			Expr = Expr.Resolve (ec);
			if (Expr == null)
				return null;

			//
			// We perform some simple tests, and then to "split" the emit and store
			// code we create an instance of a different class, and return that.
			//
			// I am experimenting with this pattern.
			//
			Type t = Expr.Type;

			if (t == TypeManager.array_type){
				ec.Report.Error (21, loc, "Cannot apply indexing with [] to an expression of type `System.Array'");
				return null;
			}
			
			if (t.IsArray)
				return (new ArrayAccess (this, loc)).Resolve (ec);
			if (t.IsPointer)
				return MakePointerAccess (ec, t);

			FieldExpr fe = Expr as FieldExpr;
			if (fe != null) {
				IFixedBuffer ff = AttributeTester.GetFixedBuffer (fe.FieldInfo);
				if (ff != null) {
					return MakePointerAccess (ec, ff.ElementType);
				}
			}
			return (new IndexerAccess (this, loc)).Resolve (ec);
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			Expr = Expr.Resolve (ec);
			if (Expr == null)
				return null;

			type = Expr.Type;
			if (type.IsArray)
				return (new ArrayAccess (this, loc)).DoResolveLValue (ec, right_side);

			if (type.IsPointer)
				return MakePointerAccess (ec, type);

			if (Expr.eclass != ExprClass.Variable && TypeManager.IsStruct (type))
				Error_CannotModifyIntermediateExpressionValue (ec);

			return (new IndexerAccess (this, loc)).DoResolveLValue (ec, right_side);
		}
		
		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Should never be reached");
		}

		public static void Error_NamedArgument (NamedArgument na, Report Report)
		{
			Report.Error (1742, na.Name.Location, "An element access expression cannot use named argument");
		}

		public override string GetSignatureForError ()
		{
			return Expr.GetSignatureForError ();
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			ElementAccess target = (ElementAccess) t;

			target.Expr = Expr.Clone (clonectx);
			if (Arguments != null)
				target.Arguments = Arguments.Clone (clonectx);
		}
	}

	/// <summary>
	///   Implements array access 
	/// </summary>
	public class ArrayAccess : Expression, IAssignMethod, IMemoryLocation {
		//
		// Points to our "data" repository
		//
		ElementAccess ea;

		LocalTemporary temp;

		bool prepared;
		
		public ArrayAccess (ElementAccess ea_data, Location l)
		{
			ea = ea_data;
			loc = l;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return ea.CreateExpressionTree (ec);
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			return DoResolve (ec);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
#if false
			ExprClass eclass = ea.Expr.eclass;

			// As long as the type is valid
			if (!(eclass == ExprClass.Variable || eclass == ExprClass.PropertyAccess ||
			      eclass == ExprClass.Value)) {
				ea.Expr.Error_UnexpectedKind ("variable or value");
				return null;
			}
#endif

			if (eclass != ExprClass.Invalid)
				return this;

			// dynamic is used per argument in ConvertExpressionToArrayIndex case
			bool dynamic;
			ea.Arguments.Resolve (ec, out dynamic);

			Type t = ea.Expr.Type;
			int rank = ea.Arguments.Count;
			if (t.GetArrayRank () != rank) {
				ec.Report.Error (22, ea.Location, "Wrong number of indexes `{0}' inside [], expected `{1}'",
					  ea.Arguments.Count.ToString (), t.GetArrayRank ().ToString ());
				return null;
			}

			type = TypeManager.GetElementType (t);
			if (type.IsPointer && !ec.IsUnsafe) {
				UnsafeError (ec, ea.Location);
			}

			foreach (Argument a in ea.Arguments) {
				if (a is NamedArgument)
					ElementAccess.Error_NamedArgument ((NamedArgument) a, ec.Report);

				a.Expr = ConvertExpressionToArrayIndex (ec, a.Expr);
			}
			
			eclass = ExprClass.Variable;

			return this;
		}

		/// <summary>
		///    Emits the right opcode to load an object of Type `t'
		///    from an array of T
		/// </summary>
		void EmitLoadOpcode (ILGenerator ig, Type type, int rank)
		{
			if (rank > 1) {
				MethodInfo get = FetchGetMethod ();
				ig.Emit (OpCodes.Call, get);
				return;
			}

			if (type == TypeManager.byte_type || type == TypeManager.bool_type)
				ig.Emit (OpCodes.Ldelem_U1);
			else if (type == TypeManager.sbyte_type)
				ig.Emit (OpCodes.Ldelem_I1);
			else if (type == TypeManager.short_type)
				ig.Emit (OpCodes.Ldelem_I2);
			else if (type == TypeManager.ushort_type || type == TypeManager.char_type)
				ig.Emit (OpCodes.Ldelem_U2);
			else if (type == TypeManager.int32_type)
				ig.Emit (OpCodes.Ldelem_I4);
			else if (type == TypeManager.uint32_type)
				ig.Emit (OpCodes.Ldelem_U4);
			else if (type == TypeManager.uint64_type)
				ig.Emit (OpCodes.Ldelem_I8);
			else if (type == TypeManager.int64_type)
				ig.Emit (OpCodes.Ldelem_I8);
			else if (type == TypeManager.float_type)
				ig.Emit (OpCodes.Ldelem_R4);
			else if (type == TypeManager.double_type)
				ig.Emit (OpCodes.Ldelem_R8);
			else if (type == TypeManager.intptr_type)
				ig.Emit (OpCodes.Ldelem_I);
			else if (TypeManager.IsEnumType (type)){
				EmitLoadOpcode (ig, TypeManager.GetEnumUnderlyingType (type), rank);
			} else if (TypeManager.IsStruct (type)){
				ig.Emit (OpCodes.Ldelema, type);
				ig.Emit (OpCodes.Ldobj, type);
#if GMCS_SOURCE
			} else if (type.IsGenericParameter) {
				ig.Emit (OpCodes.Ldelem, type);
#endif
			} else if (type.IsPointer)
				ig.Emit (OpCodes.Ldelem_I);
			else
				ig.Emit (OpCodes.Ldelem_Ref);
		}

		protected override void Error_NegativeArrayIndex (ResolveContext ec, Location loc)
		{
			ec.Report.Warning (251, 2, loc, "Indexing an array with a negative index (array indices always start at zero)");
		}

		/// <summary>
		///    Returns the right opcode to store an object of Type `t'
		///    from an array of T.  
		/// </summary>
		static public OpCode GetStoreOpcode (Type t, out bool is_stobj, out bool has_type_arg)
		{
			has_type_arg = false; is_stobj = false;
			t = TypeManager.TypeToCoreType (t);
			if (TypeManager.IsEnumType (t))
				t = TypeManager.GetEnumUnderlyingType (t);
			if (t == TypeManager.byte_type || t == TypeManager.sbyte_type ||
			    t == TypeManager.bool_type)
				return OpCodes.Stelem_I1;
			else if (t == TypeManager.short_type || t == TypeManager.ushort_type ||
				 t == TypeManager.char_type)
				return OpCodes.Stelem_I2;
			else if (t == TypeManager.int32_type || t == TypeManager.uint32_type)
				return OpCodes.Stelem_I4;
			else if (t == TypeManager.int64_type || t == TypeManager.uint64_type)
				return OpCodes.Stelem_I8;
			else if (t == TypeManager.float_type)
				return OpCodes.Stelem_R4;
			else if (t == TypeManager.double_type)
				return OpCodes.Stelem_R8;
			else if (t == TypeManager.intptr_type) {
                                has_type_arg = true;
				is_stobj = true;
                                return OpCodes.Stobj;
			} else if (TypeManager.IsStruct (t)) {
				has_type_arg = true;
				is_stobj = true;
				return OpCodes.Stobj;
#if GMCS_SOURCE
			} else if (t.IsGenericParameter) {
				has_type_arg = true;
				return OpCodes.Stelem;
#endif

			} else if (t.IsPointer)
				return OpCodes.Stelem_I;
			else
				return OpCodes.Stelem_Ref;
		}

		MethodInfo FetchGetMethod ()
		{
			ModuleBuilder mb = RootContext.ToplevelTypes.Builder;
			int arg_count = ea.Arguments.Count;
			Type [] args = new Type [arg_count];
			MethodInfo get;
			
			for (int i = 0; i < arg_count; i++){
				//args [i++] = a.Type;
				args [i] = TypeManager.int32_type;
			}
			
			get = mb.GetArrayMethod (
				ea.Expr.Type, "Get",
				CallingConventions.HasThis |
				CallingConventions.Standard,
				type, args);
			return get;
		}
				

		MethodInfo FetchAddressMethod ()
		{
			ModuleBuilder mb = RootContext.ToplevelTypes.Builder;
			int arg_count = ea.Arguments.Count;
			Type [] args = new Type [arg_count];
			MethodInfo address;
			Type ret_type;
			
			ret_type = TypeManager.GetReferenceType (type);
			
			for (int i = 0; i < arg_count; i++){
				//args [i++] = a.Type;
				args [i] = TypeManager.int32_type;
			}
			
			address = mb.GetArrayMethod (
				ea.Expr.Type, "Address",
				CallingConventions.HasThis |
				CallingConventions.Standard,
				ret_type, args);

			return address;
		}

		//
		// Load the array arguments into the stack.
		//
		void LoadArrayAndArguments (EmitContext ec)
		{
			ea.Expr.Emit (ec);

			for (int i = 0; i < ea.Arguments.Count; ++i) {
				ea.Arguments [i].Emit (ec);
			}
		}

		public void Emit (EmitContext ec, bool leave_copy)
		{
			int rank = ea.Expr.Type.GetArrayRank ();
			ILGenerator ig = ec.ig;

			if (prepared) {
				LoadFromPtr (ig, this.type);
			} else {
				LoadArrayAndArguments (ec);
				EmitLoadOpcode (ig, type, rank);
			}	

			if (leave_copy) {
				ig.Emit (OpCodes.Dup);
				temp = new LocalTemporary (this.type);
				temp.Store (ec);
			}
		}
		
		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			int rank = ea.Expr.Type.GetArrayRank ();
			ILGenerator ig = ec.ig;
			Type t = source.Type;
			prepared = prepare_for_load;

			if (prepared) {
				AddressOf (ec, AddressOp.LoadStore);
				ec.ig.Emit (OpCodes.Dup);
			} else {
				LoadArrayAndArguments (ec);
			}

			if (rank == 1) {
				bool is_stobj, has_type_arg;
				OpCode op = GetStoreOpcode (t, out is_stobj, out has_type_arg);

				if (!prepared) {
					//
					// The stobj opcode used by value types will need
					// an address on the stack, not really an array/array
					// pair
					//
					if (is_stobj)
						ig.Emit (OpCodes.Ldelema, t);
				}
				
				source.Emit (ec);
				if (leave_copy) {
					ec.ig.Emit (OpCodes.Dup);
					temp = new LocalTemporary (this.type);
					temp.Store (ec);
				}
				
				if (prepared)
					StoreFromPtr (ig, t);
				else if (is_stobj)
					ig.Emit (OpCodes.Stobj, t);
				else if (has_type_arg)
					ig.Emit (op, t);
				else
					ig.Emit (op);
			} else {
				source.Emit (ec);
				if (leave_copy) {
					ec.ig.Emit (OpCodes.Dup);
					temp = new LocalTemporary (this.type);
					temp.Store (ec);
				}

				if (prepared) {
					StoreFromPtr (ig, t);
				} else {
					int arg_count = ea.Arguments.Count;
					Type [] args = new Type [arg_count + 1];
					for (int i = 0; i < arg_count; i++) {
						//args [i++] = a.Type;
						args [i] = TypeManager.int32_type;
					}
					args [arg_count] = type;

					MethodInfo set = RootContext.ToplevelTypes.Builder.GetArrayMethod (
						ea.Expr.Type, "Set",
						CallingConventions.HasThis |
						CallingConventions.Standard,
						TypeManager.void_type, args);

					ig.Emit (OpCodes.Call, set);
				}
			}
			
			if (temp != null) {
				temp.Emit (ec);
				temp.Release (ec);
			}
		}

		public void EmitNew (EmitContext ec, New source, bool leave_copy)
		{
			if (!source.Emit (ec, this)) {
				if (leave_copy)
					throw new NotImplementedException ();

				return;
			}

			throw new NotImplementedException ();
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			int rank = ea.Expr.Type.GetArrayRank ();
			ILGenerator ig = ec.ig;

			LoadArrayAndArguments (ec);

			if (rank == 1){
				ig.Emit (OpCodes.Ldelema, type);
			} else {
				MethodInfo address = FetchAddressMethod ();
				ig.Emit (OpCodes.Call, address);
			}
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			type = storey.MutateType (type);
			ea.Expr.Type = storey.MutateType (ea.Expr.Type);
		}
	}

	/// <summary>
	///   Expressions that represent an indexer call.
	/// </summary>
	public class IndexerAccess : Expression, IAssignMethod
	{
		class IndexerMethodGroupExpr : MethodGroupExpr
		{
			public IndexerMethodGroupExpr (Indexers indexers, Location loc)
				: base (null, loc)
			{
				Methods = (MethodBase []) indexers.Methods.ToArray (typeof (MethodBase));
			}

			public override string Name {
				get {
					return "this";
				}
			}

			protected override int GetApplicableParametersCount (MethodBase method, AParametersCollection parameters)
			{
				//
				// Here is the trick, decrease number of arguments by 1 when only
				// available property method is setter. This makes overload resolution
				// work correctly for indexers.
				//
				
				if (method.Name [0] == 'g')
					return parameters.Count;

				return parameters.Count - 1;
			}
		}

		class Indexers
		{
			// Contains either property getter or setter
			public ArrayList Methods;
			public ArrayList Properties;

			Indexers ()
			{
			}

			void Append (Type caller_type, MemberInfo [] mi)
			{
				if (mi == null)
					return;

				foreach (PropertyInfo property in mi) {
					MethodInfo accessor = property.GetGetMethod (true);
					if (accessor == null)
						accessor = property.GetSetMethod (true);

					if (Methods == null) {
						Methods = new ArrayList ();
						Properties = new ArrayList ();
					}

					Methods.Add (accessor);
					Properties.Add (property);
				}
			}

			static MemberInfo [] GetIndexersForTypeOrInterface (Type caller_type, Type lookup_type)
			{
				string p_name = TypeManager.IndexerPropertyName (lookup_type);

				return TypeManager.MemberLookup (
					caller_type, caller_type, lookup_type, MemberTypes.Property,
					BindingFlags.Public | BindingFlags.Instance |
					BindingFlags.DeclaredOnly, p_name, null);
			}
			
			public static Indexers GetIndexersForType (Type caller_type, Type lookup_type) 
			{
				Indexers ix = new Indexers ();

				if (TypeManager.IsGenericParameter (lookup_type)) {
					GenericConstraints gc = TypeManager.GetTypeParameterConstraints (lookup_type);
					if (gc == null)
						return ix;

					if (gc.HasClassConstraint) {
						Type class_contraint = gc.ClassConstraint;
						while (class_contraint != TypeManager.object_type && class_contraint != null) {
							ix.Append (caller_type, GetIndexersForTypeOrInterface (caller_type, class_contraint));
							class_contraint = class_contraint.BaseType;
						}
					}

					Type[] ifaces = gc.InterfaceConstraints;
					foreach (Type itype in ifaces)
						ix.Append (caller_type, GetIndexersForTypeOrInterface (caller_type, itype));

					return ix;
				}

				Type copy = lookup_type;
				while (copy != TypeManager.object_type && copy != null){
					ix.Append (caller_type, GetIndexersForTypeOrInterface (caller_type, copy));
					copy = copy.BaseType;
				}

				if (lookup_type.IsInterface) {
					Type [] ifaces = TypeManager.GetInterfaces (lookup_type);
					if (ifaces != null) {
						foreach (Type itype in ifaces)
							ix.Append (caller_type, GetIndexersForTypeOrInterface (caller_type, itype));
					}
				}

				return ix;
			}
		}

		//
		// Points to our "data" repository
		//
		MethodInfo get, set;
		bool is_base_indexer;
		bool prepared;
		LocalTemporary temp;
		LocalTemporary prepared_value;
		Expression set_expr;

		protected Type indexer_type;
		protected Type current_type;
		protected Expression instance_expr;
		protected Arguments arguments;
		
		public IndexerAccess (ElementAccess ea, Location loc)
			: this (ea.Expr, false, loc)
		{
			this.arguments = ea.Arguments;
		}

		protected IndexerAccess (Expression instance_expr, bool is_base_indexer,
					 Location loc)
		{
			this.instance_expr = instance_expr;
			this.is_base_indexer = is_base_indexer;
			this.eclass = ExprClass.Value;
			this.loc = loc;
		}

		static string GetAccessorName (bool isSet)
		{
			return isSet ? "set" : "get";
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree (ec, arguments,
				instance_expr.CreateExpressionTree (ec),
				new TypeOfMethod (get, loc));

			return CreateExpressionFactoryCall (ec, "Call", args);
		}

		protected virtual void CommonResolve (ResolveContext ec)
		{
			indexer_type = instance_expr.Type;
			current_type = ec.CurrentType;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			return ResolveAccessor (ec, null);
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			if (right_side == EmptyExpression.OutAccess) {
				ec.Report.Error (206, loc,
					"A property or indexer may not be passed as an out or ref parameter");
				return null;
			}

			// if the indexer returns a value type, and we try to set a field in it
			if (right_side == EmptyExpression.LValueMemberAccess || right_side == EmptyExpression.LValueMemberOutAccess) {
				Error_CannotModifyIntermediateExpressionValue (ec);
			}

			return ResolveAccessor (ec, right_side);
		}

		Expression ResolveAccessor (ResolveContext ec, Expression right_side)
		{
			CommonResolve (ec);

			bool dynamic;
			arguments.Resolve (ec, out dynamic);
			if (dynamic || TypeManager.IsDynamicType (indexer_type)) {
				int additional = right_side == null ? 1 : 2;
				Arguments args = new Arguments (arguments.Count + additional);
				if (is_base_indexer) {
					ec.Report.Error (1972, loc, "The indexer base access cannot be dynamically dispatched. Consider casting the dynamic arguments or eliminating the base access");
				} else {
					args.Add (new Argument (instance_expr));
				}
				args.AddRange (arguments);
				if (right_side != null)
					args.Add (new Argument (right_side));

				return new DynamicIndexBinder (right_side != null, args, loc).Resolve (ec);
			}

			Indexers ilist = Indexers.GetIndexersForType (current_type, indexer_type);
			if (ilist.Methods == null) {
				ec.Report.Error (21, loc, "Cannot apply indexing with [] to an expression of type `{0}'",
						  TypeManager.CSharpName (indexer_type));
				return null;
			}

			MethodGroupExpr mg = new IndexerMethodGroupExpr (ilist, loc);
			mg = mg.OverloadResolve (ec, ref arguments, false, loc);
			if (mg == null)
				return null;

			MethodInfo mi = (MethodInfo) mg;
			PropertyInfo pi = null;
			for (int i = 0; i < ilist.Methods.Count; ++i) {
				if (ilist.Methods [i] == mi) {
					pi = (PropertyInfo) ilist.Properties [i];
					break;
				}
			}

			type = TypeManager.TypeToCoreType (pi.PropertyType);
			if (type.IsPointer && !ec.IsUnsafe)
				UnsafeError (ec, loc);

			MethodInfo accessor;
			if (right_side == null) {
				accessor = get = pi.GetGetMethod (true);
			} else {
				accessor = set = pi.GetSetMethod (true);
				if (accessor == null && pi.GetGetMethod (true) != null) {
					ec.Report.SymbolRelatedToPreviousError (pi);
					ec.Report.Error (200, loc, "The read only property or indexer `{0}' cannot be assigned to",
						TypeManager.GetFullNameSignature (pi));
					return null;
				}

				set_expr = Convert.ImplicitConversion (ec, right_side, type, loc);
			}

			if (accessor == null) {
				ec.Report.SymbolRelatedToPreviousError (pi);
				ec.Report.Error (154, loc, "The property or indexer `{0}' cannot be used in this context because it lacks a `{1}' accessor",
					TypeManager.GetFullNameSignature (pi), GetAccessorName (right_side != null));
				return null;
			}

			//
			// Only base will allow this invocation to happen.
			//
			if (accessor.IsAbstract && this is BaseIndexerAccess) {
				Error_CannotCallAbstractBase (ec, TypeManager.GetFullNameSignature (pi));
			}

			bool must_do_cs1540_check;
			if (!IsAccessorAccessible (ec.CurrentType, accessor, out must_do_cs1540_check)) {
				if (set == null)
					set = pi.GetSetMethod (true);
				else
					get = pi.GetGetMethod (true);

				if (set != null && get != null &&
					(set.Attributes & MethodAttributes.MemberAccessMask) != (get.Attributes & MethodAttributes.MemberAccessMask)) {
					ec.Report.SymbolRelatedToPreviousError (accessor);
					ec.Report.Error (271, loc, "The property or indexer `{0}' cannot be used in this context because a `{1}' accessor is inaccessible",
						TypeManager.GetFullNameSignature (pi), GetAccessorName (right_side != null));
				} else {
					ec.Report.SymbolRelatedToPreviousError (pi);
					ErrorIsInaccesible (loc, TypeManager.GetFullNameSignature (pi), ec.Report);
				}
			}

			instance_expr.CheckMarshalByRefAccess (ec);
			eclass = ExprClass.IndexerAccess;
			return this;
		}
		
		public void Emit (EmitContext ec, bool leave_copy)
		{
			if (prepared) {
				prepared_value.Emit (ec);
			} else {
				Invocation.EmitCall (ec, is_base_indexer, instance_expr, get,
					arguments, loc, false, false);
			}

			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				temp = new LocalTemporary (Type);
				temp.Store (ec);
			}
		}
		
		//
		// source is ignored, because we already have a copy of it from the
		// LValue resolution and we have already constructed a pre-cached
		// version of the arguments (ea.set_arguments);
		//
		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			prepared = prepare_for_load;
			Expression value = set_expr;

			if (prepared) {
				Invocation.EmitCall (ec, is_base_indexer, instance_expr, get,
					arguments, loc, true, false);

				prepared_value = new LocalTemporary (type);
				prepared_value.Store (ec);
				source.Emit (ec);
				prepared_value.Release (ec);

				if (leave_copy) {
					ec.ig.Emit (OpCodes.Dup);
					temp = new LocalTemporary (Type);
					temp.Store (ec);
				}
			} else if (leave_copy) {
				temp = new LocalTemporary (Type);
				source.Emit (ec);
				temp.Store (ec);
				value = temp;
			}
			
			if (!prepared)
				arguments.Add (new Argument (value));

			Invocation.EmitCall (ec, is_base_indexer, instance_expr, set, arguments, loc, false, prepared);
			
			if (temp != null) {
				temp.Emit (ec);
				temp.Release (ec);
			}
		}
		
		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpSignature (get != null ? get : set, false);
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			if (get != null)
				get = storey.MutateGenericMethod (get);
			if (set != null)
				set = storey.MutateGenericMethod (set);

			instance_expr.MutateHoistedGenericType (storey);
			if (arguments != null)
				arguments.MutateHoistedGenericType (storey);

			type = storey.MutateType (type);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			IndexerAccess target = (IndexerAccess) t;

			if (arguments != null)
				target.arguments = arguments.Clone (clonectx);

			if (instance_expr != null)
				target.instance_expr = instance_expr.Clone (clonectx);
		}
	}

	/// <summary>
	///   The base operator for method names
	/// </summary>
	public class BaseAccess : Expression {
		public readonly string Identifier;
		TypeArguments args;

		public BaseAccess (string member, Location l)
		{
			this.Identifier = member;
			loc = l;
		}

		public BaseAccess (string member, TypeArguments args, Location l)
			: this (member, l)
		{
			this.args = args;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			Expression c = CommonResolve (ec);

			if (c == null)
				return null;

			//
			// MethodGroups use this opportunity to flag an error on lacking ()
			//
			if (!(c is MethodGroupExpr))
				return c.Resolve (ec);
			return c;
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			Expression c = CommonResolve (ec);

			if (c == null)
				return null;

			//
			// MethodGroups use this opportunity to flag an error on lacking ()
			//
			if (! (c is MethodGroupExpr))
				return c.DoResolveLValue (ec, right_side);

			return c;
		}

		Expression CommonResolve (ResolveContext ec)
		{
			Expression member_lookup;
			Type current_type = ec.CurrentType;
			Type base_type = current_type.BaseType;

			if (!This.IsThisAvailable (ec)) {
				if (ec.IsStatic) {
					ec.Report.Error (1511, loc, "Keyword `base' is not available in a static method");
				} else {
					ec.Report.Error (1512, loc, "Keyword `base' is not available in the current context");
				}
				return null;
			}
			
			member_lookup = MemberLookup (ec.Compiler, ec.CurrentType, null, base_type, Identifier,
						      AllMemberTypes, AllBindingFlags, loc);
			if (member_lookup == null) {
				Error_MemberLookupFailed (ec, ec.CurrentType, base_type, base_type, Identifier,
					null, AllMemberTypes, AllBindingFlags);
				return null;
			}

			Expression left;
			
			if (ec.IsStatic)
				left = new TypeExpression (base_type, loc);
			else
				left = ec.GetThis (loc);

			MemberExpr me = member_lookup as MemberExpr;
			if (me == null){
				if (member_lookup is TypeExpression){
					ec.Report.Error (582, loc, "{0}: Can not reference a type through an expression, try `{1}' instead",
							 Identifier, member_lookup.GetSignatureForError ());
				} else {
					ec.Report.Error (582, loc, "{0}: Can not reference a {1} through an expression", 
							 Identifier, member_lookup.ExprClassName);
				}
				
				return null;
			}
			
			me = me.ResolveMemberAccess (ec, left, loc, null);
			if (me == null)
				return null;

			me.IsBase = true;
			if (args != null) {
				args.Resolve (ec);
				me.SetTypeArguments (ec, args);
			}

			return me;
		}

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Should never be called"); 
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			BaseAccess target = (BaseAccess) t;

			if (args != null)
				target.args = args.Clone ();
		}
	}

	/// <summary>
	///   The base indexer operator
	/// </summary>
	public class BaseIndexerAccess : IndexerAccess {
		public BaseIndexerAccess (Arguments args, Location loc)
			: base (null, true, loc)
		{
			this.arguments = args;
		}

		protected override void CommonResolve (ResolveContext ec)
		{
			instance_expr = ec.GetThis (loc);

			current_type = ec.CurrentType.BaseType;
			indexer_type = current_type;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			MemberExpr.Error_BaseAccessInExpressionTree (ec, loc);
			return base.CreateExpressionTree (ec);
		}
	}
	
	/// <summary>
	///   This class exists solely to pass the Type around and to be a dummy
	///   that can be passed to the conversion functions (this is used by
	///   foreach implementation to typecast the object return value from
	///   get_Current into the proper type.  All code has been generated and
	///   we only care about the side effect conversions to be performed
	///
	///   This is also now used as a placeholder where a no-action expression
	///   is needed (the `New' class).
	/// </summary>
	public class EmptyExpression : Expression {
		public static readonly Expression Null = new EmptyExpression ();

		public static readonly EmptyExpression OutAccess = new EmptyExpression ();
		public static readonly EmptyExpression LValueMemberAccess = new EmptyExpression ();
		public static readonly EmptyExpression LValueMemberOutAccess = new EmptyExpression ();
		public static readonly EmptyExpression UnaryAddress = new EmptyExpression ();

		static EmptyExpression temp = new EmptyExpression ();
		public static EmptyExpression Grab ()
		{
			EmptyExpression retval = temp == null ? new EmptyExpression () : temp;
			temp = null;
			return retval;
		}

		public static void Release (EmptyExpression e)
		{
			temp = e;
		}

		EmptyExpression ()
		{
			// FIXME: Don't set to object
			type = TypeManager.object_type;
			eclass = ExprClass.Value;
			loc = Location.Null;
		}

		public EmptyExpression (Type t)
		{
			type = t;
			eclass = ExprClass.Value;
			loc = Location.Null;
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
			// nothing, as we only exist to not do anything.
		}

		public override void EmitSideEffect (EmitContext ec)
		{
		}

		//
		// This is just because we might want to reuse this bad boy
		// instead of creating gazillions of EmptyExpressions.
		// (CanImplicitConversion uses it)
		//
		public void SetType (Type t)
		{
			type = t;
		}
	}
	
	//
	// Empty statement expression
	//
	public sealed class EmptyExpressionStatement : ExpressionStatement
	{
		public static readonly EmptyExpressionStatement Instance = new EmptyExpressionStatement ();

		private EmptyExpressionStatement ()
		{
			eclass = ExprClass.Value;
			loc = Location.Null;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return null;
		}

		public override void EmitStatement (EmitContext ec)
		{
			// Do nothing
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.object_type;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			// Do nothing
		}
	}	

	public class UserCast : Expression {
		MethodInfo method;
		Expression source;
		
		public UserCast (MethodInfo method, Expression source, Location l)
		{
			this.method = method;
			this.source = source;
			type = TypeManager.TypeToCoreType (method.ReturnType);
			loc = l;
		}

		public Expression Source {
			get {
				return source;
			}
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (3);
			args.Add (new Argument (source.CreateExpressionTree (ec)));
			args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));
			args.Add (new Argument (new TypeOfMethod (method, loc)));
			return CreateExpressionFactoryCall (ec, "Convert", args);
		}
			
		public override Expression DoResolve (ResolveContext ec)
		{
			ObsoleteAttribute oa = AttributeTester.GetMethodObsoleteAttribute (method);
			if (oa != null)
				AttributeTester.Report_ObsoleteMessage (oa, GetSignatureForError (), loc, ec.Report);

			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			source.Emit (ec);
			ec.ig.Emit (OpCodes.Call, method);
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpSignature (method);
		}

#if NET_4_0
		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			return SLE.Expression.Convert (source.MakeExpression (ctx), type, method);
		}
#endif

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			source.MutateHoistedGenericType (storey);
			method = storey.MutateGenericMethod (method);
		}
	}

	// <summary>
	//   This class is used to "construct" the type during a typecast
	//   operation.  Since the Type.GetType class in .NET can parse
	//   the type specification, we just use this to construct the type
	//   one bit at a time.
	// </summary>
	public class ComposedCast : TypeExpr {
		FullNamedExpression left;
		string dim;
		
		public ComposedCast (FullNamedExpression left, string dim)
			: this (left, dim, left.Location)
		{
		}

		public ComposedCast (FullNamedExpression left, string dim, Location l)
		{
			this.left = left;
			this.dim = dim;
			loc = l;
		}

		protected override TypeExpr DoResolveAsTypeStep (IMemberContext ec)
		{
			TypeExpr lexpr = left.ResolveAsTypeTerminal (ec, false);
			if (lexpr == null)
				return null;

			Type ltype = lexpr.Type;
			if ((dim.Length > 0) && (dim [0] == '?')) {
				TypeExpr nullable = new Nullable.NullableType (lexpr, loc);
				if (dim.Length > 1)
					nullable = new ComposedCast (nullable, dim.Substring (1), loc);
				return nullable.ResolveAsTypeTerminal (ec, false);
			}

			if (dim == "*" && !TypeManager.VerifyUnManaged (ltype, loc))
				return null;

			if (dim.Length != 0 && dim [0] == '[') {
				if (TypeManager.IsSpecialType (ltype)) {
					ec.Compiler.Report.Error (611, loc, "Array elements cannot be of type `{0}'", TypeManager.CSharpName (ltype));
					return null;
				}

				if ((ltype.Attributes & Class.StaticClassAttribute) == Class.StaticClassAttribute) {
					ec.Compiler.Report.SymbolRelatedToPreviousError (ltype);
					ec.Compiler.Report.Error (719, loc, "Array elements cannot be of static type `{0}'", 
						TypeManager.CSharpName (ltype));
				}
			}

			if (dim != "")
				type = TypeManager.GetConstructedType (ltype, dim);
			else
				type = ltype;

			if (type == null)
				throw new InternalErrorException ("Couldn't create computed type " + ltype + dim);

			if (type.IsPointer && !ec.IsUnsafe){
				UnsafeError (ec.Compiler.Report, loc);
			}

			eclass = ExprClass.Type;
			return this;
		}

		public override string GetSignatureForError ()
		{
			return left.GetSignatureForError () + dim;
		}

		public override TypeExpr ResolveAsTypeTerminal (IMemberContext ec, bool silent)
		{
			return ResolveAsBaseTerminal (ec, silent);
		}		
	}

	public class FixedBufferPtr : Expression {
		Expression array;

		public FixedBufferPtr (Expression array, Type array_type, Location l)
		{
			this.array = array;
			this.loc = l;

			type = TypeManager.GetPointerType (array_type);
			eclass = ExprClass.Value;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Error_PointerInsideExpressionTree (ec);
			return null;
		}

		public override void Emit(EmitContext ec)
		{
			array.Emit (ec);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			//
			// We are born fully resolved
			//
			return this;
		}
	}


	//
	// This class is used to represent the address of an array, used
	// only by the Fixed statement, this generates "&a [0]" construct
	// for fixed (char *pa = a)
	//
	public class ArrayPtr : FixedBufferPtr {
		Type array_type;
		
		public ArrayPtr (Expression array, Type array_type, Location l):
			base (array, array_type, l)
		{
			this.array_type = array_type;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			
			ILGenerator ig = ec.ig;
			IntLiteral.EmitInt (ig, 0);
			ig.Emit (OpCodes.Ldelema, array_type);
		}
	}

	//
	// Encapsulates a conversion rules required for array indexes
	//
	public class ArrayIndexCast : TypeCast
	{
		public ArrayIndexCast (Expression expr)
			: base (expr, expr.Type)
		{
			if (type == TypeManager.int32_type)
				throw new ArgumentException ("unnecessary conversion");
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (2);
			args.Add (new Argument (child.CreateExpressionTree (ec)));
			args.Add (new Argument (new TypeOf (new TypeExpression (TypeManager.int32_type, loc), loc)));
			return CreateExpressionFactoryCall (ec, "ConvertChecked", args);
		}

		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);

			if (type == TypeManager.uint32_type)
				ec.ig.Emit (OpCodes.Conv_U);
			else if (type == TypeManager.int64_type)
				ec.ig.Emit (OpCodes.Conv_Ovf_I);
			else if (type == TypeManager.uint64_type)
				ec.ig.Emit (OpCodes.Conv_Ovf_I_Un);
			else
				throw new InternalErrorException ("Cannot emit cast to unknown array element type", type);
		}

		public override bool GetAttributableValue (ResolveContext ec, Type value_type, out object value)
		{
			return child.GetAttributableValue (ec, value_type, out value);
		}
	}

	//
	// Implements the `stackalloc' keyword
	//
	public class StackAlloc : Expression {
		Type otype;
		Expression t;
		Expression count;
		
		public StackAlloc (Expression type, Expression count, Location l)
		{
			t = type;
			this.count = count;
			loc = l;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			count = count.Resolve (ec);
			if (count == null)
				return null;
			
			if (count.Type != TypeManager.uint32_type){
				count = Convert.ImplicitConversionRequired (ec, count, TypeManager.int32_type, loc);
				if (count == null)
					return null;
			}

			Constant c = count as Constant;
			if (c != null && c.IsNegative) {
				ec.Report.Error (247, loc, "Cannot use a negative size with stackalloc");
				return null;
			}

			if (ec.HasAny (ResolveContext.Options.CatchScope | ResolveContext.Options.FinallyScope)) {
				ec.Report.Error (255, loc, "Cannot use stackalloc in finally or catch");
			}

			TypeExpr texpr = t.ResolveAsTypeTerminal (ec, false);
			if (texpr == null)
				return null;

			otype = texpr.Type;

			if (!TypeManager.VerifyUnManaged (otype, loc))
				return null;

			type = TypeManager.GetPointerType (otype);
			eclass = ExprClass.Value;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			int size = GetTypeSize (otype);
			ILGenerator ig = ec.ig;

			count.Emit (ec);

			if (size == 0)
				ig.Emit (OpCodes.Sizeof, otype);
			else
				IntConstant.EmitInt (ig, size);

			ig.Emit (OpCodes.Mul_Ovf_Un);
			ig.Emit (OpCodes.Localloc);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			StackAlloc target = (StackAlloc) t;
			target.count = count.Clone (clonectx);
			target.t = t.Clone (clonectx);
		}
	}

	//
	// An object initializer expression
	//
	public class ElementInitializer : Assign
	{
		public readonly string Name;

		public ElementInitializer (string name, Expression initializer, Location loc)
			: base (null, initializer, loc)
		{
			this.Name = name;
		}
		
		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			ElementInitializer target = (ElementInitializer) t;
			target.source = source.Clone (clonectx);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (2);
			FieldExpr fe = target as FieldExpr;
			if (fe != null)
				args.Add (new Argument (fe.CreateTypeOfExpression ()));
			else
				args.Add (new Argument (((PropertyExpr)target).CreateSetterTypeOfExpression ()));

			args.Add (new Argument (source.CreateExpressionTree (ec)));
			return CreateExpressionFactoryCall (ec,
				source is CollectionOrObjectInitializers ? "ListBind" : "Bind",
				args);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (source == null)
				return EmptyExpressionStatement.Instance;
			
			MemberExpr me = MemberLookupFinal (ec, ec.CurrentInitializerVariable.Type, ec.CurrentInitializerVariable.Type,
				Name, MemberTypes.Field | MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, loc) as MemberExpr;

			if (me == null)
				return null;

			target = me;
			me.InstanceExpression = ec.CurrentInitializerVariable;

			if (source is CollectionOrObjectInitializers) {
				Expression previous = ec.CurrentInitializerVariable;
				ec.CurrentInitializerVariable = target;
				source = source.Resolve (ec);
				ec.CurrentInitializerVariable = previous;
				if (source == null)
					return null;
					
				eclass = source.eclass;
				type = source.Type;
				return this;
			}

			Expression expr = base.DoResolve (ec);
			if (expr == null)
				return null;

			//
			// Ignore field initializers with default value
			//
			Constant c = source as Constant;
			if (c != null && c.IsDefaultInitializer (type) && target.eclass == ExprClass.Variable)
				return EmptyExpressionStatement.Instance.DoResolve (ec);

			return expr;
		}

		protected override Expression Error_MemberLookupFailed (ResolveContext ec, Type type, MemberInfo[] members)
		{
			MemberInfo member = members [0];
			if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field)
				ec.Report.Error (1913, loc, "Member `{0}' cannot be initialized. An object " +
					"initializer may only be used for fields, or properties", TypeManager.GetFullNameSignature (member));
			else
				ec.Report.Error (1914, loc, " Static field or property `{0}' cannot be assigned in an object initializer",
					TypeManager.GetFullNameSignature (member));

			return null;
		}
		
		public override void EmitStatement (EmitContext ec)
		{
			if (source is CollectionOrObjectInitializers)
				source.Emit (ec);
			else
				base.EmitStatement (ec);
		}
	}
	
	//
	// A collection initializer expression
	//
	class CollectionElementInitializer : Invocation
	{
		public class ElementInitializerArgument : Argument
		{
			public ElementInitializerArgument (Expression e)
				: base (e)
			{
			}
		}

		sealed class AddMemberAccess : MemberAccess
		{
			public AddMemberAccess (Expression expr, Location loc)
				: base (expr, "Add", loc)
			{
			}

			protected override void Error_TypeDoesNotContainDefinition (ResolveContext ec, Type type, string name)
			{
				if (TypeManager.HasElementType (type))
					return;

				base.Error_TypeDoesNotContainDefinition (ec, type, name);
			}
		}

		public CollectionElementInitializer (Expression argument)
			: base (null, new Arguments (1))
		{
			base.arguments.Add (new ElementInitializerArgument (argument));
			this.loc = argument.Location;
		}

		public CollectionElementInitializer (ArrayList arguments, Location loc)
			: base (null, new Arguments (arguments.Count))
		{
			foreach (Expression e in arguments)
				base.arguments.Add (new ElementInitializerArgument (e));

			this.loc = loc;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (2);
			args.Add (new Argument (mg.CreateExpressionTree (ec)));

			ArrayList expr_initializers = new ArrayList (arguments.Count);
			foreach (Argument a in arguments)
				expr_initializers.Add (a.CreateExpressionTree (ec));

			args.Add (new Argument (new ArrayCreation (
				CreateExpressionTypeExpression (ec, loc), "[]", expr_initializers, loc)));
			return CreateExpressionFactoryCall (ec, "ElementInit", args);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			CollectionElementInitializer target = (CollectionElementInitializer) t;
			if (arguments != null)
				target.arguments = arguments.Clone (clonectx);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (eclass != ExprClass.Invalid)
				return this;

			base.expr = new AddMemberAccess (ec.CurrentInitializerVariable, loc);

			return base.DoResolve (ec);
		}
	}
	
	//
	// A block of object or collection initializers
	//
	public class CollectionOrObjectInitializers : ExpressionStatement
	{
		ArrayList initializers;
		bool is_collection_initialization;
		
		public static readonly CollectionOrObjectInitializers Empty = 
			new CollectionOrObjectInitializers (new ArrayList (0), Location.Null);

		public CollectionOrObjectInitializers (ArrayList initializers, Location loc)
		{
			this.initializers = initializers;
			this.loc = loc;
		}
		
		public bool IsEmpty {
			get {
				return initializers.Count == 0;
			}
		}

		public bool IsCollectionInitializer {
			get {
				return is_collection_initialization;
			}
		}

		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			CollectionOrObjectInitializers t = (CollectionOrObjectInitializers) target;

			t.initializers = new ArrayList (initializers.Count);
			foreach (Expression e in initializers)
				t.initializers.Add (e.Clone (clonectx));
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			ArrayList expr_initializers = new ArrayList (initializers.Count);
			foreach (Expression e in initializers) {
				Expression expr = e.CreateExpressionTree (ec);
				if (expr != null)
					expr_initializers.Add (expr);
			}

			return new ImplicitlyTypedArrayCreation ("[]", expr_initializers, loc);
		}
		
		public override Expression DoResolve (ResolveContext ec)
		{
			if (eclass != ExprClass.Invalid)
				return this;

			ArrayList element_names = null;
			for (int i = 0; i < initializers.Count; ++i) {
				Expression initializer = (Expression) initializers [i];
				ElementInitializer element_initializer = initializer as ElementInitializer;

				if (i == 0) {
					if (element_initializer != null) {
						element_names = new ArrayList (initializers.Count);
						element_names.Add (element_initializer.Name);
					} else if (initializer is CompletingExpression){
						initializer.Resolve (ec);
						throw new InternalErrorException ("This line should never be reached");
					} else {
						if (!TypeManager.ImplementsInterface (ec.CurrentInitializerVariable.Type, TypeManager.ienumerable_type)) {
							ec.Report.Error (1922, loc, "A field or property `{0}' cannot be initialized with a collection " +
								"object initializer because type `{1}' does not implement `{2}' interface",
								ec.CurrentInitializerVariable.GetSignatureForError (),
								TypeManager.CSharpName (ec.CurrentInitializerVariable.Type),
								TypeManager.CSharpName (TypeManager.ienumerable_type));
							return null;
						}
						is_collection_initialization = true;
					}
				} else {
					if (is_collection_initialization != (element_initializer == null)) {
						ec.Report.Error (747, initializer.Location, "Inconsistent `{0}' member declaration",
							is_collection_initialization ? "collection initializer" : "object initializer");
						continue;
					}

					if (!is_collection_initialization) {
						if (element_names.Contains (element_initializer.Name)) {
							ec.Report.Error (1912, element_initializer.Location,
								"An object initializer includes more than one member `{0}' initialization",
								element_initializer.Name);
						} else {
							element_names.Add (element_initializer.Name);
						}
					}
				}

				Expression e = initializer.Resolve (ec);
				if (e == EmptyExpressionStatement.Instance)
					initializers.RemoveAt (i--);
				else
					initializers [i] = e;
			}

			type = ec.CurrentInitializerVariable.Type;
			if (is_collection_initialization) {
				if (TypeManager.HasElementType (type)) {
					ec.Report.Error (1925, loc, "Cannot initialize object of type `{0}' with a collection initializer",
						TypeManager.CSharpName (type));
				}
			}

			eclass = ExprClass.Variable;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			EmitStatement (ec);
		}

		public override void EmitStatement (EmitContext ec)
		{
			foreach (ExpressionStatement e in initializers)
				e.EmitStatement (ec);
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			foreach (Expression e in initializers)
				e.MutateHoistedGenericType (storey);
		}
	}
	
	//
	// New expression with element/object initializers
	//
	public class NewInitialize : New
	{
		//
		// This class serves as a proxy for variable initializer target instances.
		// A real variable is assigned later when we resolve left side of an
		// assignment
		//
		sealed class InitializerTargetExpression : Expression, IMemoryLocation
		{
			NewInitialize new_instance;

			public InitializerTargetExpression (NewInitialize newInstance)
			{
				this.type = newInstance.type;
				this.loc = newInstance.loc;
				this.eclass = newInstance.eclass;
				this.new_instance = newInstance;
			}

			public override Expression CreateExpressionTree (ResolveContext ec)
			{
				// Should not be reached
				throw new NotSupportedException ("ET");
			}

			public override Expression DoResolve (ResolveContext ec)
			{
				return this;
			}

			public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
			{
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				Expression e = (Expression) new_instance.instance;
				e.Emit (ec);
			}

			#region IMemoryLocation Members

			public void AddressOf (EmitContext ec, AddressOp mode)
			{
				new_instance.instance.AddressOf (ec, mode);
			}

			#endregion
		}

		CollectionOrObjectInitializers initializers;
		IMemoryLocation instance;

		public NewInitialize (Expression requested_type, Arguments arguments, CollectionOrObjectInitializers initializers, Location l)
			: base (requested_type, arguments, l)
		{
			this.initializers = initializers;
		}

		protected override IMemoryLocation EmitAddressOf (EmitContext ec, AddressOp Mode)
		{
			instance = base.EmitAddressOf (ec, Mode);

			if (!initializers.IsEmpty)
				initializers.Emit (ec);

			return instance;
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			base.CloneTo (clonectx, t);

			NewInitialize target = (NewInitialize) t;
			target.initializers = (CollectionOrObjectInitializers) initializers.Clone (clonectx);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (2);
			args.Add (new Argument (base.CreateExpressionTree (ec)));
			if (!initializers.IsEmpty)
				args.Add (new Argument (initializers.CreateExpressionTree (ec)));

			return CreateExpressionFactoryCall (ec,
				initializers.IsCollectionInitializer ? "ListInit" : "MemberInit",
				args);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (eclass != ExprClass.Invalid)
				return this;
			
			Expression e = base.DoResolve (ec);
			if (type == null)
				return null;

			Expression previous = ec.CurrentInitializerVariable;
			ec.CurrentInitializerVariable = new InitializerTargetExpression (this);
			initializers.Resolve (ec);
			ec.CurrentInitializerVariable = previous;
			return e;
		}

		public override bool Emit (EmitContext ec, IMemoryLocation target)
		{
			bool left_on_stack = base.Emit (ec, target);

			if (initializers.IsEmpty)
				return left_on_stack;

			LocalTemporary temp = target as LocalTemporary;
			if (temp == null) {
				if (!left_on_stack) {
					VariableReference vr = target as VariableReference;
					
					// FIXME: This still does not work correctly for pre-set variables
					if (vr != null && vr.IsRef)
						target.AddressOf (ec, AddressOp.Load);

					((Expression) target).Emit (ec);
					left_on_stack = true;
				}

				temp = new LocalTemporary (type);
			}

			instance = temp;
			if (left_on_stack)
				temp.Store (ec);

			initializers.Emit (ec);

			if (left_on_stack) {
				temp.Emit (ec);
				temp.Release (ec);
			}

			return left_on_stack;
		}

		public override bool HasInitializer {
			get {
				return !initializers.IsEmpty;
			}
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			base.MutateHoistedGenericType (storey);
			initializers.MutateHoistedGenericType (storey);
		}
	}

	public class NewAnonymousType : New
	{
		static readonly ArrayList EmptyParameters = new ArrayList (0);

		ArrayList parameters;
		readonly TypeContainer parent;
		AnonymousTypeClass anonymous_type;

		public NewAnonymousType (ArrayList parameters, TypeContainer parent, Location loc)
			 : base (null, null, loc)
		{
			this.parameters = parameters;
			this.parent = parent;
		}

		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			if (parameters == null)
				return;

			NewAnonymousType t = (NewAnonymousType) target;
			t.parameters = new ArrayList (parameters.Count);
			foreach (AnonymousTypeParameter atp in parameters)
				t.parameters.Add (atp.Clone (clonectx));
		}

		AnonymousTypeClass CreateAnonymousType (ResolveContext ec, ArrayList parameters)
		{
			AnonymousTypeClass type = parent.Module.GetAnonymousType (parameters);
			if (type != null)
				return type;

			type = AnonymousTypeClass.Create (ec.Compiler, parent, parameters, loc);
			if (type == null)
				return null;

			type.DefineType ();
			type.Define ();
			type.EmitType ();
			if (ec.Report.Errors == 0)
				type.CloseType ();

			parent.Module.AddAnonymousType (type);
			return type;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
#if GMCS_SOURCE			
			if (parameters == null)
				return base.CreateExpressionTree (ec);

			ArrayList init = new ArrayList (parameters.Count);
			foreach (Property p in anonymous_type.Properties)
				init.Add (new TypeOfMethod (TypeBuilder.GetMethod (type, p.GetBuilder), loc));

			ArrayList ctor_args = new ArrayList (Arguments.Count);
			foreach (Argument a in Arguments)
				ctor_args.Add (a.CreateExpressionTree (ec));

			Arguments args = new Arguments (3);
			args.Add (new Argument (method.CreateExpressionTree (ec)));
			args.Add (new Argument (new ArrayCreation (TypeManager.expression_type_expr, "[]", ctor_args, loc)));
			args.Add (new Argument (new ImplicitlyTypedArrayCreation ("[]", init, loc)));

			return CreateExpressionFactoryCall (ec, "New", args);
#else
			throw new NotSupportedException ();
#endif
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (ec.HasSet (ResolveContext.Options.ConstantScope)) {
				ec.Report.Error (836, loc, "Anonymous types cannot be used in this expression");
				return null;
			}

			if (parameters == null) {
				anonymous_type = CreateAnonymousType (ec, EmptyParameters);
				RequestedType = new TypeExpression (anonymous_type.TypeBuilder, loc);
				return base.DoResolve (ec);
			}

			bool error = false;
			Arguments = new Arguments (parameters.Count);
			TypeExpression [] t_args = new TypeExpression [parameters.Count];
			for (int i = 0; i < parameters.Count; ++i) {
				Expression e = ((AnonymousTypeParameter) parameters [i]).Resolve (ec);
				if (e == null) {
					error = true;
					continue;
				}

				Arguments.Add (new Argument (e));
				t_args [i] = new TypeExpression (e.Type, e.Location);
			}

			if (error)
				return null;

			anonymous_type = CreateAnonymousType (ec, parameters);
			if (anonymous_type == null)
				return null;

			RequestedType = new GenericTypeExpr (anonymous_type.TypeBuilder, new TypeArguments (t_args), loc);
			return base.DoResolve (ec);
		}
	}

	public class AnonymousTypeParameter : Expression
	{
		public readonly string Name;
		Expression initializer;

		public AnonymousTypeParameter (Expression initializer, string name, Location loc)
		{
			this.Name = name;
			this.loc = loc;
			this.initializer = initializer;
		}
		
		public AnonymousTypeParameter (Parameter parameter)
		{
			this.Name = parameter.Name;
			this.loc = parameter.Location;
			this.initializer = new SimpleName (Name, loc);
		}		

		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			AnonymousTypeParameter t = (AnonymousTypeParameter) target;
			t.initializer = initializer.Clone (clonectx);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override bool Equals (object o)
		{
			AnonymousTypeParameter other = o as AnonymousTypeParameter;
			return other != null && Name == other.Name;
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			Expression e = initializer.Resolve (ec);
			if (e == null)
				return null;

			if (e.eclass == ExprClass.MethodGroup) {
				Error_InvalidInitializer (ec, e.ExprClassName);
				return null;
			}

			type = e.Type;
			if (type == TypeManager.void_type || type == TypeManager.null_type ||
				type == InternalType.AnonymousMethod || type.IsPointer) {
				Error_InvalidInitializer (ec, e.GetSignatureForError ());
				return null;
			}

			return e;
		}

		protected virtual void Error_InvalidInitializer (ResolveContext ec, string initializer)
		{
			ec.Report.Error (828, loc, "An anonymous type property `{0}' cannot be initialized with `{1}'",
				Name, initializer);
		}

		public override void Emit (EmitContext ec)
		{
			throw new InternalErrorException ("Should not be reached");
		}
	}
}
