﻿//
// dynamic.cs: support for dynamic expressions
//
// Authors: Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2009 Novell, Inc
// Copyright 2011 Xamarin Inc.
//

using System;
using System.Linq;
using SLE = System.Linq.Expressions;
using System.Dynamic;
#if STATIC
using IKVM.Reflection.Emit;
#else
using System.Reflection.Emit;
#endif

namespace Mono.CSharp
{
	//
	// A copy of Microsoft.CSharp/Microsoft.CSharp.RuntimeBinder/CSharpBinderFlags.cs
	// has to be kept in sync
	//
	[Flags]
	public enum CSharpBinderFlags
	{
		None = 0,
		CheckedContext = 1,
		InvokeSimpleName = 1 << 1,
		InvokeSpecialName = 1 << 2,
		BinaryOperationLogical = 1 << 3,
		ConvertExplicit = 1 << 4,
		ConvertArrayIndex = 1 << 5,
		ResultIndexed = 1 << 6,
		ValueFromCompoundAssignment = 1 << 7,
		ResultDiscarded = 1 << 8
	}

	//
	// Type expression with internal dynamic type symbol
	//
	class DynamicTypeExpr : TypeExpr
	{
		public DynamicTypeExpr (Location loc)
		{
			this.loc = loc;
		}

		public override TypeSpec ResolveAsType (IMemberContext ec, bool allowUnboundTypeArguments)
		{
			eclass = ExprClass.Type;
			type = ec.Module.Compiler.BuiltinTypes.Dynamic;
			return type;
		}
	}

	#region Dynamic runtime binder expressions

	//
	// Expression created from runtime dynamic object value by dynamic binder
	//
	public class RuntimeValueExpression : Expression, IDynamicAssign, IMemoryLocation
	{

		readonly DynamicMetaObject obj;

		public RuntimeValueExpression (DynamicMetaObject obj, TypeSpec type)
		{
			this.obj = obj;
			this.type = type;
			this.eclass = ExprClass.Variable;
		}

		#region Properties

		public bool IsSuggestionOnly { get; set; }

		public DynamicMetaObject MetaObject {
			get { return obj; }
		}

		#endregion

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			throw new NotImplementedException ();
		}

		public override bool ContainsEmitWithAwait ()
		{
			throw new NotSupportedException ();
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ();
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			return this;
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}

		#region IAssignMethod Members

		public void Emit (EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException ();
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			throw new NotImplementedException ();
		}

		#endregion

		public SLE.Expression MakeAssignExpression (BuilderContext ctx, Expression source)
		{
			return obj.Expression;
		}

		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
#if STATIC
			return base.MakeExpression (ctx);
#else

				if (type.IsStruct && !obj.Expression.Type.IsValueType)
					return SLE.Expression.Unbox (obj.Expression, type.GetMetaInfo ());

				if (obj.Expression.NodeType == SLE.ExpressionType.Parameter) {
					if (((SLE.ParameterExpression) obj.Expression).IsByRef)
						return obj.Expression;
				}

				return SLE.Expression.Convert (obj.Expression, type.GetMetaInfo ());
#endif
		}
	}

	//
	// Wraps runtime dynamic expression into expected type. Needed
	// to satify expected type check by dynamic binder and no conversion
	// is required (ResultDiscarded).
	//
	public class DynamicResultCast : ShimExpression
	{
		public DynamicResultCast (TypeSpec type, Expression expr)
			: base (expr)
		{
			this.type = type;
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			expr = expr.Resolve (ec);
			eclass = ExprClass.Value;
			return this;
		}

		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
#if STATIC
			return base.MakeExpression (ctx);
#else
			return SLE.Expression.Block (expr.MakeExpression (ctx), SLE.Expression.Default (type.GetMetaInfo ()));
#endif
		}
	}

	#endregion

	//
	// Creates dynamic binder expression
	//
	interface IDynamicBinder
	{
		Expression CreateCallSiteBinder (ResolveContext ec, Arguments args);
	}

	//
	// Extends standard assignment interface for expressions
	// supported by dynamic resolver
	//
	interface IDynamicAssign : IAssignMethod
	{
		SLE.Expression MakeAssignExpression (BuilderContext ctx, Expression source);
	}

	//
	// Base dynamic expression statement creator
	//
	class DynamicExpressionStatement : ExpressionStatement
	{
		//
		// Binder flag dynamic constant, the value is combination of
		// flags known at resolve stage and flags known only at emit
		// stage
		//
		protected class BinderFlags : EnumConstant
		{
			readonly DynamicExpressionStatement statement;
			readonly CSharpBinderFlags flags;

			public BinderFlags (CSharpBinderFlags flags, DynamicExpressionStatement statement)
				: base (statement.loc)
			{
				this.flags = flags;
				this.statement = statement;
				eclass = 0;
			}

			protected override Expression DoResolve (ResolveContext ec)
			{
				Child = new IntConstant (ec.BuiltinTypes, (int) (flags | statement.flags), statement.loc);

				type = ec.Module.PredefinedTypes.BinderFlags.Resolve ();
				eclass = Child.eclass;
				return this;
			}
		}

		readonly Arguments arguments;
		protected IDynamicBinder binder;
		protected Expression binder_expr;

		// Used by BinderFlags
		protected CSharpBinderFlags flags;

		TypeSpec binder_type;
		TypeParameters context_mvars;

		public DynamicExpressionStatement (IDynamicBinder binder, Arguments args, Location loc)
		{
			this.binder = binder;
			this.arguments = args;
			this.loc = loc;
		}

		public Arguments Arguments {
			get {
				return arguments;
			}
		}

		public override bool ContainsEmitWithAwait ()
		{
			return arguments.ContainsEmitWithAwait ();
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			ec.Report.Error (1963, loc, "An expression tree cannot contain a dynamic operation");
			return null;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			if (DoResolveCore (rc))
				binder_expr = binder.CreateCallSiteBinder (rc, arguments);

			return this;
		}

		protected bool DoResolveCore (ResolveContext rc)
		{
			int i = 0;
			foreach (var arg in arguments) {
				if (arg.Type == InternalType.VarOutType) {
					// Should be special error message about dynamic dispatch
					rc.Report.Error (8197, arg.Expr.Location, "Cannot infer the type of implicitly-typed out variable `{0}'", ((DeclarationExpression) arg.Expr).Variable.Name);
				} else if (arg.Type == InternalType.DefaultType) {
					rc.Report.Error (8311, arg.Expr.Location, "Cannot use a default literal as an argument to a dynamically dispatched operation");
				}

				// Forced limitation because Microsoft.CSharp needs to catch up
				if (i > 0 && arguments [i - 1] is NamedArgument && !(arguments [i] is NamedArgument))
					rc.Report.Error (8324, loc, "Named argument specifications must appear after all fixed arguments have been specified in a dynamic invocation");
				++i;
			}

			if (rc.CurrentTypeParameters != null && rc.CurrentTypeParameters[0].IsMethodTypeParameter)
				context_mvars = rc.CurrentTypeParameters;

			int errors = rc.Report.Errors;
			var pt = rc.Module.PredefinedTypes;

			binder_type = pt.Binder.Resolve ();
			pt.CallSite.Resolve ();
			pt.CallSiteGeneric.Resolve ();

			eclass = ExprClass.Value;

			if (type == null)
				type = rc.BuiltinTypes.Dynamic;

			if (rc.Report.Errors == errors)
				return true;

			rc.Report.Error (1969, loc,
				"Dynamic operation cannot be compiled without `Microsoft.CSharp.dll' assembly reference");
			return false;
		}

		public override void Emit (EmitContext ec)
		{
			EmitCall (ec, binder_expr, arguments,  false);
		}

		public override void EmitStatement (EmitContext ec)
		{
			EmitCall (ec, binder_expr, arguments, true);
		}

		protected void EmitConditionalAccess (EmitContext ec)
		{
			var a_expr = arguments [0].Expr;

			var des = a_expr as DynamicExpressionStatement;
			if (des != null) {
				des.EmitConditionalAccess (ec);
			}

			if (HasConditionalAccess ()) {
				var NullOperatorLabel = ec.DefineLabel ();

				if (ExpressionAnalyzer.IsInexpensiveLoad (a_expr)) {
					a_expr.Emit (ec);
				} else {
					var lt = new LocalTemporary (a_expr.Type);
					lt.EmitAssign (ec, a_expr, true, false);

					Arguments [0].Expr = lt;
				}

				ec.Emit (OpCodes.Brtrue_S, NullOperatorLabel);

				if (!ec.ConditionalAccess.Statement) {
					if (ec.ConditionalAccess.Type.IsNullableType)
						Nullable.LiftedNull.Create (ec.ConditionalAccess.Type, Location.Null).Emit (ec);
					else
						ec.EmitNull ();
				}

				ec.Emit (OpCodes.Br, ec.ConditionalAccess.EndLabel);
				ec.MarkLabel (NullOperatorLabel);

				return;
			}

			if (a_expr.HasConditionalAccess ()) {
				var lt = new LocalTemporary (a_expr.Type);
				lt.EmitAssign (ec, a_expr, false, false);

				Arguments [0].Expr = lt;
			}
		}

		protected void EmitCall (EmitContext ec, Expression binder, Arguments arguments, bool isStatement)
		{
			//
			// This method generates all internal infrastructure for a dynamic call. The
			// reason why it's quite complicated is the mixture of dynamic and anonymous
			// methods. Dynamic itself requires a temporary class (ContainerX) and anonymous
			// methods can generate temporary storey as well (AnonStorey). Handling MVAR
			// type parameters rewrite is non-trivial in such case as there are various
			// combinations possible therefore the mutator is not straightforward. Secondly
			// we need to keep both MVAR(possibly VAR for anon storey) and type VAR to emit
			// correct Site field type and its access from EmitContext.
			//

			int dyn_args_count = arguments == null ? 0 : arguments.Count;
			int default_args = isStatement ? 1 : 2;
			var module = ec.Module;

			bool has_ref_out_argument = false;
			var targs = new TypeExpression[dyn_args_count + default_args];
			targs[0] = new TypeExpression (module.PredefinedTypes.CallSite.TypeSpec, loc);

			TypeExpression[] targs_for_instance = null;
			TypeParameterMutator mutator;

			var site_container = ec.CreateDynamicSite ();

			if (context_mvars != null) {
				TypeParameters tparam;
				TypeContainer sc = site_container;
				do {
					tparam = sc.CurrentTypeParameters;
					sc = sc.Parent;
				} while (tparam == null);

				mutator = new TypeParameterMutator (context_mvars, tparam);

				if (!ec.IsAnonymousStoreyMutateRequired) {
					targs_for_instance = new TypeExpression[targs.Length];
					targs_for_instance[0] = targs[0];
				}
			} else {
				mutator = null;
			}

			for (int i = 0; i < dyn_args_count; ++i) {
				Argument a = arguments[i];
				if (a.ArgType == Argument.AType.Out || a.ArgType == Argument.AType.Ref)
					has_ref_out_argument = true;

				var t = a.Type;

				// Convert any internal type like dynamic or null to object
				if (t.Kind == MemberKind.InternalCompilerType)
					t = ec.BuiltinTypes.Object;

				if (targs_for_instance != null)
					targs_for_instance[i + 1] = new TypeExpression (t, loc);

				if (mutator != null)
					t = t.Mutate (mutator);

				targs[i + 1] = new TypeExpression (t, loc);
			}

			TypeExpr del_type = null;
			TypeExpr del_type_instance_access = null;
			if (!has_ref_out_argument) {
				string d_name = isStatement ? "Action" : "Func";

				TypeSpec te = null;
				Namespace type_ns = module.GlobalRootNamespace.GetNamespace ("System", true);
				if (type_ns != null) {
					te = type_ns.LookupType (module, d_name, dyn_args_count + default_args, LookupMode.Normal, loc);
				}

				if (te != null) {
					if (!isStatement) {
						var t = type;
						if (t.Kind == MemberKind.InternalCompilerType)
							t = ec.BuiltinTypes.Object;

						if (targs_for_instance != null)
							targs_for_instance[targs_for_instance.Length - 1] = new TypeExpression (t, loc);

						if (mutator != null)
							t = t.Mutate (mutator);

						targs[targs.Length - 1] = new TypeExpression (t, loc);
					}

					del_type = new GenericTypeExpr (te, new TypeArguments (targs), loc);
					if (targs_for_instance != null)
						del_type_instance_access = new GenericTypeExpr (te, new TypeArguments (targs_for_instance), loc);
					else
						del_type_instance_access = del_type;
				}
			}

			//
			// Create custom delegate when no appropriate predefined delegate has been found
			//
			Delegate d;
			if (del_type == null) {
				TypeSpec rt = isStatement ? ec.BuiltinTypes.Void : type;
				Parameter[] p = new Parameter[dyn_args_count + 1];
				p[0] = new Parameter (targs[0], "p0", Parameter.Modifier.NONE, null, loc);

				var site = ec.CreateDynamicSite ();
				int index = site.Containers == null ? 0 : site.Containers.Count;

				if (mutator != null)
					rt = mutator.Mutate (rt);

				for (int i = 1; i < dyn_args_count + 1; ++i) {
					p[i] = new Parameter (targs[i], "p" + i.ToString ("X"), arguments[i - 1].Modifier, null, loc);
				}

				d = new Delegate (site, new TypeExpression (rt, loc),
					Modifiers.INTERNAL | Modifiers.COMPILER_GENERATED,
					new MemberName ("Container" + index.ToString ("X")),
					new ParametersCompiled (p), null);

				d.CreateContainer ();
				d.DefineContainer ();
				d.Define ();
				d.PrepareEmit ();

				site.AddTypeContainer (d);

				//
				// Add new container to inflated site container when the
				// member cache already exists
				//
				if (site.CurrentType is InflatedTypeSpec && index > 0)
					site.CurrentType.MemberCache.AddMember (d.CurrentType);

				del_type = new TypeExpression (d.CurrentType, loc);
				if (targs_for_instance != null) {
					del_type_instance_access = null;
				} else {
					del_type_instance_access = del_type;
				}
			} else {
				d = null;
			}

			var site_type_decl = new GenericTypeExpr (module.PredefinedTypes.CallSiteGeneric.TypeSpec, new TypeArguments (del_type), loc);
			var field = site_container.CreateCallSiteField (site_type_decl, loc);
			if (field == null)
				return;

			if (del_type_instance_access == null) {
				var dt = d.CurrentType.DeclaringType.MakeGenericType (module, context_mvars.Types);
				del_type_instance_access = new TypeExpression (MemberCache.GetMember (dt, d.CurrentType), loc);
			}

			var instanceAccessExprType = new GenericTypeExpr (module.PredefinedTypes.CallSiteGeneric.TypeSpec,
				new TypeArguments (del_type_instance_access), loc);

			if (instanceAccessExprType.ResolveAsType (ec.MemberContext) == null)
				return;

			bool inflate_using_mvar = context_mvars != null && ec.IsAnonymousStoreyMutateRequired;

			TypeSpec gt;
			if (inflate_using_mvar || context_mvars == null) {
				gt = site_container.CurrentType;
			} else {
				gt = site_container.CurrentType.MakeGenericType (module, context_mvars.Types);
			}

			// When site container already exists the inflated version has to be
			// updated manually to contain newly created field
			if (gt is InflatedTypeSpec && site_container.AnonymousMethodsCounter > 1) {
				var tparams = gt.MemberDefinition.TypeParametersCount > 0 ? gt.MemberDefinition.TypeParameters : TypeParameterSpec.EmptyTypes;
				var inflator = new TypeParameterInflator (module, gt, tparams, gt.TypeArguments);
				gt.MemberCache.AddMember (field.InflateMember (inflator));
			}

			FieldExpr site_field_expr = new FieldExpr (MemberCache.GetMember (gt, field), loc);

			BlockContext bc = new BlockContext (ec.MemberContext, null, ec.BuiltinTypes.Void);

			Arguments args = new Arguments (1);
			args.Add (new Argument (binder));
			StatementExpression s = new StatementExpression (new SimpleAssign (site_field_expr, new Invocation (new MemberAccess (instanceAccessExprType, "Create"), args)));

			using (ec.With (BuilderContext.Options.OmitDebugInfo, true)) {

				var conditionalAccessReceiver = IsConditionalAccessReceiver;
				var ca = ec.ConditionalAccess;

				if (conditionalAccessReceiver) {
					ec.ConditionalAccess = new ConditionalAccessContext (type, ec.DefineLabel ()) {
						Statement = isStatement
					};

					//
					// Emit conditional access expressions before dynamic call
					// is initialized. It pushes site_field_expr on stack before
					// the actual instance argument is emited which would cause
					// jump from non-empty stack.
					//
					EmitConditionalAccess (ec);
				}

				if (s.Resolve (bc)) {
					Statement init = new If (new Binary (Binary.Operator.Equality, site_field_expr, new NullLiteral (loc)), s, loc);
					init.Emit (ec);
				}

				args = new Arguments (1 + dyn_args_count);
				args.Add (new Argument (site_field_expr));
				if (arguments != null) {
					int arg_pos = 1;
					foreach (Argument a in arguments) {
						if (a is NamedArgument) {
							// Name is not valid in this context
							args.Add (new Argument (a.Expr, a.ArgType));
						} else {
							args.Add (a);
						}

						if (inflate_using_mvar && a.Type != targs[arg_pos].Type)
							a.Expr.Type = targs[arg_pos].Type;

						++arg_pos;
					}
				}

				var target = new DelegateInvocation (new MemberAccess (site_field_expr, "Target", loc).Resolve (bc), args, false, loc).Resolve (bc);
				if (target != null) {
					target.Emit (ec);
				}

				if (conditionalAccessReceiver) {
					ec.CloseConditionalAccess (!isStatement && type.IsNullableType ? type : null);
					ec.ConditionalAccess = ca;
				}
			}
		}

		public override void FlowAnalysis (FlowAnalysisContext fc)
		{
			arguments.FlowAnalysis (fc);
		}

		public static MemberAccess GetBinderNamespace (Location loc)
		{
			return new MemberAccess (new MemberAccess (
				new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "Microsoft", loc), "CSharp", loc), "RuntimeBinder", loc);
		}

		protected MemberAccess GetBinder (string name, Location loc)
		{
			return new MemberAccess (new TypeExpression (binder_type, loc), name, loc);
		}

		protected virtual bool IsConditionalAccessReceiver {
			get {
				return false;
			}
		}
	}

	//
	// Dynamic member access compound assignment for events
	//
	class DynamicEventCompoundAssign : ExpressionStatement
	{
		class IsEvent : DynamicExpressionStatement, IDynamicBinder
		{
			string name;

			public IsEvent (string name, Arguments args, Location loc)
				: base (null, args, loc)
			{
				this.name = name;
				binder = this;
			}

			public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
			{
				type = ec.BuiltinTypes.Bool;

				Arguments binder_args = new Arguments (3);

				binder_args.Add (new Argument (new BinderFlags (0, this)));
				binder_args.Add (new Argument (new StringLiteral (ec.BuiltinTypes, name, loc)));
				binder_args.Add (new Argument (new TypeOf (ec.CurrentType, loc)));

				return new Invocation (GetBinder ("IsEvent", loc), binder_args);
			}
		}

		Expression condition;
		ExpressionStatement invoke, assign;

		public DynamicEventCompoundAssign (string name, Arguments args, ExpressionStatement assignment, ExpressionStatement invoke, Location loc)
		{
			condition = new IsEvent (name, args, loc);
			this.invoke = invoke;
			this.assign = assignment;
			this.loc = loc;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return condition.CreateExpressionTree (ec);
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = rc.BuiltinTypes.Dynamic;
			eclass = ExprClass.Value;
			condition = condition.Resolve (rc);
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			var rc = new ResolveContext (ec.MemberContext);
			var expr = new Conditional (new BooleanExpression (condition), invoke, assign, loc).Resolve (rc);
			expr.Emit (ec);
		}

		public override void EmitStatement (EmitContext ec)
		{
			var stmt = new If (condition, new StatementExpression (invoke), new StatementExpression (assign), loc);
			using (ec.With (BuilderContext.Options.OmitDebugInfo, true)) {
				stmt.Emit (ec);
			}
		}

		public override void FlowAnalysis (FlowAnalysisContext fc)
		{
			invoke.FlowAnalysis (fc);
		}
	}

	class DynamicConversion : DynamicExpressionStatement, IDynamicBinder
	{
		public DynamicConversion (TypeSpec targetType, CSharpBinderFlags flags, Arguments args, Location loc)
			: base (null, args, loc)
		{
			type = targetType;
			base.flags = flags;
			base.binder = this;
		}

		public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (3);

			flags |= ec.HasSet (ResolveContext.Options.CheckedScope) ? CSharpBinderFlags.CheckedContext : 0;

			binder_args.Add (new Argument (new BinderFlags (flags, this)));
			binder_args.Add (new Argument (new TypeOf (type, loc)));
			binder_args.Add (new Argument (new TypeOf (ec.CurrentType, loc)));
			return new Invocation (GetBinder ("Convert", loc), binder_args);
		}
	}

	class DynamicConstructorBinder : DynamicExpressionStatement, IDynamicBinder
	{
		public DynamicConstructorBinder (TypeSpec type, Arguments args, Location loc)
			: base (null, args, loc)
		{
			this.type = type;
			base.binder = this;
		}

		public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (3);

			binder_args.Add (new Argument (new BinderFlags (0, this)));
			binder_args.Add (new Argument (new TypeOf (ec.CurrentType, loc)));
			binder_args.Add (new Argument (new ImplicitlyTypedArrayCreation (args.CreateDynamicBinderArguments (ec), loc)));

			return new Invocation (GetBinder ("InvokeConstructor", loc), binder_args);
		}
	}

	class DynamicIndexBinder : DynamicMemberAssignable
	{
		bool can_be_mutator;
		readonly bool conditional_access_receiver;
		readonly bool conditional_access;

		public DynamicIndexBinder (Arguments args, bool conditionalAccessReceiver, bool conditionalAccess, Location loc)
			: base (args, loc)
		{
			this.conditional_access_receiver = conditionalAccessReceiver;
			this.conditional_access = conditionalAccess;
		}

		public DynamicIndexBinder (CSharpBinderFlags flags, Arguments args, Location loc)
			: this (args, false, false, loc)
		{
			base.flags = flags;
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			can_be_mutator = true;
			return base.DoResolve (ec);
		}

		protected override Expression CreateCallSiteBinder (ResolveContext ec, Arguments args, bool isSet)
		{
			Arguments binder_args = new Arguments (3);

			binder_args.Add (new Argument (new BinderFlags (flags, this)));
			binder_args.Add (new Argument (new TypeOf (ec.CurrentType, loc)));
			binder_args.Add (new Argument (new ImplicitlyTypedArrayCreation (args.CreateDynamicBinderArguments (ec), loc)));

			isSet |= (flags & CSharpBinderFlags.ValueFromCompoundAssignment) != 0;
			return new Invocation (GetBinder (isSet ? "SetIndex" : "GetIndex", loc), binder_args);
		}

		protected override Arguments CreateSetterArguments (ResolveContext rc, Expression rhs)
		{
			//
			// Indexer has arguments which complicates things as the setter and getter
			// are called in two steps when unary mutator is used. We have to make a
			// copy of all variable arguments to not duplicate any side effect.
			//
			// ++d[++arg, Foo ()]
			//

			if (!can_be_mutator)
				return base.CreateSetterArguments (rc, rhs);

			var setter_args = new Arguments (Arguments.Count + 1);
			for (int i = 0; i < Arguments.Count; ++i) {
				var expr = Arguments[i].Expr;

				if (expr is Constant || expr is VariableReference || expr is This) {
					setter_args.Add (Arguments [i]);
					continue;
				}

				LocalVariable temp = LocalVariable.CreateCompilerGenerated (expr.Type, rc.CurrentBlock, loc);
				expr = new SimpleAssign (temp.CreateReferenceExpression (rc, expr.Location), expr).Resolve (rc);
				Arguments[i].Expr = temp.CreateReferenceExpression (rc, expr.Location).Resolve (rc);
				setter_args.Add (Arguments [i].Clone (expr));
			}

			setter_args.Add (new Argument (rhs));
			return setter_args;
		}

		protected override bool IsConditionalAccessReceiver {
			get {
				return conditional_access_receiver;
			}
		}

		public override bool HasConditionalAccess ()
		{
			return conditional_access;
		}
	}

	class DynamicInvocation : DynamicExpressionStatement, IDynamicBinder
	{
		readonly ATypeNameExpression member;
		readonly bool conditional_access_receiver;

		public DynamicInvocation (ATypeNameExpression member, Arguments args, bool conditionalAccessReceiver, Location loc)
			: base (null, args, loc)
		{
			base.binder = this;
			this.member = member;
			this.conditional_access_receiver = conditionalAccessReceiver;
		}

		public static DynamicInvocation CreateSpecialNameInvoke (ATypeNameExpression member, Arguments args, Location loc)
		{
			return new DynamicInvocation (member, args, false, loc) {
				flags = CSharpBinderFlags.InvokeSpecialName
			};
		}

		public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (member != null ? 5 : 3);
			bool is_member_access = member is MemberAccess;

			CSharpBinderFlags call_flags;
			if (!is_member_access && member is SimpleName) {
				call_flags = CSharpBinderFlags.InvokeSimpleName;
				is_member_access = true;
			} else {
				call_flags = 0;
			}

			binder_args.Add (new Argument (new BinderFlags (call_flags, this)));

			if (is_member_access)
				binder_args.Add (new Argument (new StringLiteral (ec.BuiltinTypes, member.Name, member.Location)));

			if (member != null && member.HasTypeArguments) {
				TypeArguments ta = member.TypeArguments;
				if (ta.Resolve (ec, false)) {
					var targs = new ArrayInitializer (ta.Count, loc);
					foreach (TypeSpec t in ta.Arguments)
						targs.Add (new TypeOf (t, loc));

					binder_args.Add (new Argument (new ImplicitlyTypedArrayCreation (targs, loc)));
				}
			} else if (is_member_access) {
				binder_args.Add (new Argument (new NullLiteral (loc)));
			}

			binder_args.Add (new Argument (new TypeOf (ec.CurrentType, loc)));

			Expression real_args;
			if (args == null) {
				// Cannot be null because .NET trips over
				real_args = new ArrayCreation (
					new MemberAccess (GetBinderNamespace (loc), "CSharpArgumentInfo", loc),
					new ArrayInitializer (0, loc), loc);
			} else {
				real_args = new ImplicitlyTypedArrayCreation (args.CreateDynamicBinderArguments (ec), loc);
			}

			binder_args.Add (new Argument (real_args));

			return new Invocation (GetBinder (is_member_access ? "InvokeMember" : "Invoke", loc), binder_args);
		}

		public override void EmitStatement (EmitContext ec)
		{
			flags |= CSharpBinderFlags.ResultDiscarded;
			base.EmitStatement (ec);
		}

		protected override bool IsConditionalAccessReceiver {
			get {
				return conditional_access_receiver;
			}
		}

		public override bool HasConditionalAccess ()
		{
			return member is ConditionalMemberAccess;
		}
	}

	class DynamicConditionalMemberBinder : DynamicMemberBinder
	{
		public DynamicConditionalMemberBinder (string name, Arguments args, Location loc)
			: base (name, args, loc)
		{
		}

		public override bool HasConditionalAccess ()
		{
			return true;
		}
	}

	class DynamicMemberBinder : DynamicMemberAssignable
	{
		readonly string name;
		bool conditionalAccessReceiver;

		public DynamicMemberBinder (string name, Arguments args, Location loc)
			: base (args, loc)
		{
			this.name = name;
		}

		public DynamicMemberBinder (string name, CSharpBinderFlags flags, Arguments args, Location loc)
			: this (name, args, loc)
		{
			base.flags = flags;
		}

		protected override Expression CreateCallSiteBinder (ResolveContext ec, Arguments args, bool isSet)
		{
			Arguments binder_args = new Arguments (4);

			binder_args.Add (new Argument (new BinderFlags (flags, this)));
			binder_args.Add (new Argument (new StringLiteral (ec.BuiltinTypes, name, loc)));
			binder_args.Add (new Argument (new TypeOf (ec.CurrentType, loc)));
			binder_args.Add (new Argument (new ImplicitlyTypedArrayCreation (args.CreateDynamicBinderArguments (ec), loc)));

			isSet |= (flags & CSharpBinderFlags.ValueFromCompoundAssignment) != 0;
			return new Invocation (GetBinder (isSet ? "SetMember" : "GetMember", loc), binder_args);
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			if (!rc.HasSet (ResolveContext.Options.DontSetConditionalAccessReceiver))
				conditionalAccessReceiver = HasConditionalAccess () || Arguments [0].Expr.HasConditionalAccess ();

			return base.DoResolve (rc);
		}

		protected override bool IsConditionalAccessReceiver {
			get {
				return conditionalAccessReceiver;
			}
		}
	}

	//
	// Any member binder which can be source and target of assignment
	//
	abstract class DynamicMemberAssignable : DynamicExpressionStatement, IDynamicBinder, IAssignMethod
	{
		Expression setter;
		Arguments setter_args;

		protected DynamicMemberAssignable (Arguments args, Location loc)
			: base (null, args, loc)
		{
			base.binder = this;
		}

		public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
		{
			//
			// DoResolve always uses getter
			//
			return CreateCallSiteBinder (ec, args, false);
		}

		protected abstract Expression CreateCallSiteBinder (ResolveContext ec, Arguments args, bool isSet);

		protected virtual Arguments CreateSetterArguments (ResolveContext rc, Expression rhs)
		{
			var setter_args = new Arguments (Arguments.Count + 1);
			setter_args.AddRange (Arguments);
			setter_args.Add (new Argument (rhs));
			return setter_args;
		}

		public override Expression DoResolveLValue (ResolveContext rc, Expression right_side)
		{
			if (right_side == EmptyExpression.OutAccess) {
				right_side.DoResolveLValue (rc, this);
				return null;
			}

			if (DoResolveCore (rc)) {
				setter_args = CreateSetterArguments (rc, right_side);
				setter = CreateCallSiteBinder (rc, setter_args, true);
			}

			eclass = ExprClass.Variable;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			// It's null for ResolveLValue used without assignment
			if (binder_expr == null)
				EmitCall (ec, setter, Arguments, false);
			else
				base.Emit (ec);
		}

		public override void EmitStatement (EmitContext ec)
		{
			// It's null for ResolveLValue used without assignment
			if (binder_expr == null)
				EmitCall (ec, setter, Arguments, true);
			else
				base.EmitStatement (ec);
		}

		#region IAssignMethod Members

		public void Emit (EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException ();
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			EmitCall (ec, setter, setter_args, !leave_copy);
		}

		#endregion
	}

	class DynamicUnaryConversion : DynamicExpressionStatement, IDynamicBinder
	{
		readonly string name;

		public DynamicUnaryConversion (string name, Arguments args, Location loc)
			: base (null, args, loc)
		{
			this.name = name;
			base.binder = this;
		}

		public static DynamicUnaryConversion CreateIsTrue (ResolveContext rc, Arguments args, Location loc)
		{
			return new DynamicUnaryConversion ("IsTrue", args, loc) { type = rc.BuiltinTypes.Bool };
		}

		public static DynamicUnaryConversion CreateIsFalse (ResolveContext rc, Arguments args, Location loc)
		{
			return new DynamicUnaryConversion ("IsFalse", args, loc) { type = rc.BuiltinTypes.Bool };
		}

		public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (4);

			MemberAccess sle = new MemberAccess (new MemberAccess (
				new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "System", loc), "Linq", loc), "Expressions", loc);

			var flags = ec.HasSet (ResolveContext.Options.CheckedScope) ? CSharpBinderFlags.CheckedContext : 0;

			binder_args.Add (new Argument (new BinderFlags (flags, this)));
			binder_args.Add (new Argument (new MemberAccess (new MemberAccess (sle, "ExpressionType", loc), name, loc)));
			binder_args.Add (new Argument (new TypeOf (ec.CurrentType, loc)));
			binder_args.Add (new Argument (new ImplicitlyTypedArrayCreation (args.CreateDynamicBinderArguments (ec), loc)));

			return new Invocation (GetBinder ("UnaryOperation", loc), binder_args);
		}
	}

	sealed class DynamicSiteClass : HoistedStoreyClass
	{
		public DynamicSiteClass (TypeDefinition parent, MemberBase host, TypeParameters tparams)
			: base (parent, MakeMemberName (host, "DynamicSite", parent.DynamicSitesCounter, tparams, Location.Null), tparams, Modifiers.STATIC, MemberKind.Class)
		{
			parent.DynamicSitesCounter++;
		}

		public FieldSpec CreateCallSiteField (FullNamedExpression type, Location loc)
		{
			int index = AnonymousMethodsCounter++;
			Field f = new HoistedField (this, type, Modifiers.PUBLIC | Modifiers.STATIC, "Site" + index.ToString ("X"), null, loc);
			f.Define ();

			AddField (f);
			return f.Spec;
		}
	}
}
