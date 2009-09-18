//
// dynamic.cs: support for dynamic expressions
//
// Authors: Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2009 Novell, Inc
//

using System;
using System.Collections;

#if NET_4_0
using System.Dynamic;
#endif

namespace Mono.CSharp
{
	class DynamicTypeExpr : TypeExpr
	{
		public DynamicTypeExpr (Location loc)
		{
			this.loc = loc;

			type = InternalType.Dynamic;
			eclass = ExprClass.Type;
		}

		public override bool CheckAccessLevel (IMemberContext ds)
		{
			return true;
		}

		protected override TypeExpr DoResolveAsTypeStep (IMemberContext ec)
		{
			return this;
		}
	}

	//
	// Expression created from runtime dynamic object value
	//
	public class RuntimeValueExpression : Expression, IAssignMethod
	{
#if !NET_4_0
		public class DynamicMetaObject { public Type RuntimeType; }
#endif

		readonly DynamicMetaObject obj;

		// When strongly typed expression is required
		readonly bool typed;

		public RuntimeValueExpression (DynamicMetaObject obj, bool typed)
		{
			this.obj = obj;
			this.typed = typed;
			this.type = obj.RuntimeType;
			this.eclass = ExprClass.Value;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}

		#region IAssignMethod Members

		public void Emit (EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException ();
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			throw new NotImplementedException ();
		}

		#endregion

#if NET_4_0
		public override System.Linq.Expressions.Expression MakeExpression (BuilderContext ctx)
		{
			if (typed && obj.Expression.Type != type)
				return System.Linq.Expressions.Expression.Convert (obj.Expression, type);

			return obj.Expression;
		}
#endif
	}

	interface IDynamicBinder
	{
		Expression CreateCallSiteBinder (ResolveContext ec, Arguments args);
	}

	class DynamicExpressionStatement : ExpressionStatement
	{
		class StaticDataClass : CompilerGeneratedClass
		{
			public StaticDataClass ()
				: base (new RootDeclSpace (new NamespaceEntry (null, null, null)),
					new MemberName (CompilerGeneratedClass.MakeName (null, "c", "DynamicSites", 0)),
					Modifiers.INTERNAL | Modifiers.STATIC)
			{
				ModFlags &= ~Modifiers.SEALED;
			}
		}

		static StaticDataClass global_site_container;
		static int field_counter;
		static int container_counter;

		readonly Arguments arguments;
		protected IDynamicBinder binder;
		Expression binder_expr;

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

		static TypeContainer CreateSiteContainer ()
		{
			if (global_site_container == null) {
				global_site_container = new StaticDataClass ();
				RootContext.ToplevelTypes.AddCompilerGeneratedClass (global_site_container);
				global_site_container.DefineType ();
				global_site_container.Define ();
//				global_site_container.EmitType ();

				RootContext.RegisterCompilerGeneratedType (global_site_container.TypeBuilder);
			}

			return global_site_container;
		}

		static Field CreateSiteField (FullNamedExpression type)
		{
			TypeContainer site_container = CreateSiteContainer ();
			Field f = new Field (site_container, type, Modifiers.PUBLIC | Modifiers.STATIC,
				new MemberName ("Site" +  field_counter++), null);
			f.Define ();

			site_container.AddField (f);
			return f;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotImplementedException ();
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (eclass != ExprClass.Invalid)
				return this;

			if (TypeManager.call_site_type == null)
				TypeManager.call_site_type = TypeManager.CoreLookupType (ec.Compiler,
					"System.Runtime.CompilerServices", "CallSite", Kind.Class, true);

			if (TypeManager.generic_call_site_type == null)
				TypeManager.generic_call_site_type = TypeManager.CoreLookupType (ec.Compiler,
					"System.Runtime.CompilerServices", "CallSite`1", Kind.Class, true);

			eclass = ExprClass.Value;

			if (type == null)
				type = InternalType.Dynamic;

			binder_expr = binder.CreateCallSiteBinder (ec, arguments);
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			EmitCall (ec, false);
		}

		public override void EmitStatement (EmitContext ec)
		{
			EmitCall (ec, true);
		}

		void EmitCall (EmitContext ec, bool isStatement)
		{
			int dyn_args_count = arguments == null ? 0 : arguments.Count;
			TypeExpr site_type = CreateSiteType (RootContext.ToplevelTypes.Compiler, isStatement, dyn_args_count);
			FieldExpr site_field_expr = new FieldExpr (CreateSiteField (site_type).FieldBuilder, loc);

			SymbolWriter.OpenCompilerGeneratedBlock (ec.ig);

			Arguments args = new Arguments (1);
			args.Add (new Argument (binder_expr));
			StatementExpression s = new StatementExpression (new SimpleAssign (site_field_expr, new Invocation (new MemberAccess (site_type, "Create"), args)));
			
			BlockContext bc = new BlockContext (ec.MemberContext, null, TypeManager.void_type);		
			if (s.Resolve (bc)) {
				Statement init = new If (new Binary (Binary.Operator.Equality, site_field_expr, new NullLiteral (loc)), s, loc);
				init.Emit (ec);
			}

			args = new Arguments (1 + dyn_args_count);
			args.Add (new Argument (site_field_expr));
			if (arguments != null) {
				foreach (Argument a in arguments) {
					if (a is NamedArgument) {
						// Name is not valid in this context
						args.Add (new Argument (a.Expr, a.ArgType));
						continue;
					}

					args.Add (a);
				}
			}

			ResolveContext rc = new ResolveContext (ec.MemberContext);
			Expression target = new DelegateInvocation (new MemberAccess (site_field_expr, "Target", loc).Resolve (rc), args, loc).Resolve (rc);
			if (target != null)
				target.Emit (ec);

			SymbolWriter.CloseCompilerGeneratedBlock (ec.ig);
		}

		public static MemberAccess GetBinderNamespace (Location loc)
		{
			return new MemberAccess (new MemberAccess (
				new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "Microsoft", loc), "CSharp", loc), "RuntimeBinder", loc);
		}

		TypeExpr CreateSiteType (CompilerContext ctx, bool isStatement, int dyn_args_count)
		{
			int default_args = isStatement ? 1 : 2;

			bool has_ref_out_argument = false;
			FullNamedExpression[] targs = new FullNamedExpression[dyn_args_count + default_args];
			targs [0] = new TypeExpression (TypeManager.call_site_type, loc);
			for (int i = 0; i < dyn_args_count; ++i) {
				Type arg_type;
				Argument a = arguments [i];
				if (a.Type == TypeManager.null_type)
					arg_type = TypeManager.object_type;
				else
					arg_type = TypeManager.TypeToReflectionType (a.Type);

				if (a.ArgType == Argument.AType.Out || a.ArgType == Argument.AType.Ref)
					has_ref_out_argument = true;

				targs [i + 1] = new TypeExpression (arg_type, loc);
			}

			TypeExpr del_type = null;
			if (!has_ref_out_argument) {
				string d_name = isStatement ? "Action`" : "Func`";

				Type t = TypeManager.CoreLookupType (ctx, "System", d_name + (dyn_args_count + default_args), Kind.Delegate, false);
				if (t != null) {
					if (!isStatement)
						targs[targs.Length - 1] = new TypeExpression (TypeManager.TypeToReflectionType (type), loc);

					del_type = new GenericTypeExpr (t, new TypeArguments (targs), loc);
				}
			}

			// No appropriate predefined delegate found
			if (del_type == null) {
				Type rt = isStatement ? TypeManager.void_type : type;
				Parameter[] p = new Parameter [dyn_args_count + 1];
				p[0] = new Parameter (targs [0], "p0", Parameter.Modifier.NONE, null, loc);

				for (int i = 1; i < dyn_args_count + 1; ++i)
					p[i] = new Parameter (targs[i], "p" + i.ToString ("X"), arguments[i - 1].Modifier, null, loc);

				TypeContainer parent = CreateSiteContainer ();
				Delegate d = new Delegate (parent.NamespaceEntry, parent, new TypeExpression (rt, loc),
					Modifiers.INTERNAL | Modifiers.COMPILER_GENERATED,
					new MemberName ("Container" + container_counter++.ToString ("X")),
					new ParametersCompiled (p), null);

				d.DefineType ();
				d.Define ();

				parent.AddDelegate (d);
				del_type = new TypeExpression (d.TypeBuilder, loc);
			}

			TypeExpr site_type = new GenericTypeExpr (TypeManager.generic_call_site_type, new TypeArguments (del_type), loc);
			return site_type;
		}

		public static void Reset ()
		{
			global_site_container = null;
			field_counter = container_counter = 0;
		}
	}

	//
	// Dynamic member access compound assignment for events
	//
	class DynamicEventCompoundAssign : DynamicExpressionStatement, IDynamicBinder
	{
		string name;
		ExpressionStatement assignment;
		ExpressionStatement invoke;

		public DynamicEventCompoundAssign (string name, Arguments args, ExpressionStatement assignment, ExpressionStatement invoke, Location loc)
			: base (null, args, loc)
		{
			this.name = name;
			this.assignment = assignment;
			this.invoke = invoke;
			base.binder = this;

			// Used by += or -= only
			type = TypeManager.bool_type;
		}

		public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (2);
			MemberAccess binder = GetBinderNamespace (loc);

			binder_args.Add (new Argument (new StringLiteral (name, loc)));
			binder_args.Add (new Argument (new TypeOf (new TypeExpression (ec.CurrentType, loc), loc)));

			return new New (new MemberAccess (binder, "CSharpIsEventBinder", loc), binder_args, loc);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Statement cond = new If (
				new Binary (Binary.Operator.Equality, this, new BoolLiteral (true, loc)),
				new StatementExpression (invoke),
				new StatementExpression (assignment),
				loc);
			cond.Emit (ec);
		}
	}

	class DynamicConversion : DynamicExpressionStatement, IDynamicBinder
	{
		bool is_explicit;

		public DynamicConversion (Type targetType, bool isExplicit, Arguments args, Location loc)
			: base (null, args, loc)
		{
			type = targetType;
			is_explicit = isExplicit;
			base.binder = this;
		}

		public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (2);
			MemberAccess binder = GetBinderNamespace (loc);

			binder_args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));
			binder_args.Add (new Argument (new MemberAccess (new MemberAccess (binder, "CSharpConversionKind", loc),
				is_explicit ? "ExplicitConversion" : "ImplicitConversion", loc)));
			binder_args.Add (new Argument (new BoolLiteral (ec.HasSet (ResolveContext.Options.CheckedScope), loc)));
				
			return new New (new MemberAccess (binder, "CSharpConvertBinder", loc), binder_args, loc);
		}
	}

	class DynamicIndexBinder : DynamicExpressionStatement, IDynamicBinder, IAssignMethod
	{
		readonly bool isSet;

		public DynamicIndexBinder (bool isSet, Arguments args, Location loc)
			: base (null, args, loc)
		{
			base.binder = this;
			this.isSet = isSet;
		}

		public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (2);
			MemberAccess binder = GetBinderNamespace (loc);

			binder_args.Add (new Argument (new TypeOf (new TypeExpression (ec.CurrentType, loc), loc)));
			binder_args.Add (new Argument (new ImplicitlyTypedArrayCreation ("[]", args.CreateDynamicBinderArguments (), loc)));

			return new New (new MemberAccess (binder, isSet ? "CSharpSetIndexBinder" : "CSharpGetIndexBinder", loc), binder_args, loc);
		}

		#region IAssignMethod Members

		public void Emit (EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException ();
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			EmitStatement (ec);
		}

		#endregion
	}

	class DynamicInvocation : DynamicExpressionStatement, IDynamicBinder
	{
		ATypeNameExpression member;

		public DynamicInvocation (ATypeNameExpression member, Arguments args, Location loc)
			: base (null, args, loc)
		{
			base.binder = this;
			this.member = member;
		}

		public DynamicInvocation (ATypeNameExpression member, Arguments args, Type type, Location loc)
			: this (member, args, loc)
		{
			// When a return type is known not to be dynamic
			this.type = type;
		}

		public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (member != null ? 5 : 3);
			MemberAccess binder = GetBinderNamespace (loc);
			bool is_member_access = member is MemberAccess;

			string call_flags;
			if (!is_member_access && member is SimpleName) {
				call_flags = "SimpleNameCall";
				is_member_access = true;
			} else {
				call_flags = "None";
			}

			binder_args.Add (new Argument (new MemberAccess (new MemberAccess (binder, "CSharpCallFlags", loc), call_flags, loc)));

			if (is_member_access)
				binder_args.Add (new Argument (new StringLiteral (member.Name, member.Location)));

			binder_args.Add (new Argument (new TypeOf (new TypeExpression (ec.CurrentType, loc), loc)));

			if (member != null && member.HasTypeArguments) {
				TypeArguments ta = member.TypeArguments;
				if (ta.Resolve (ec)) {
					ArrayList targs = new ArrayList (ta.Count);
					foreach (Type t in ta.Arguments)
						targs.Add (new TypeOf (new TypeExpression (t, loc), loc));

					binder_args.Add (new Argument (new ImplicitlyTypedArrayCreation ("[]", targs, loc)));
				}
			} else if (is_member_access) {
				binder_args.Add (new Argument (new NullLiteral (loc)));
			}

			Expression real_args;
			if (args == null) {
				// Cannot be null because .NET trips over
				real_args = new ArrayCreation (new MemberAccess (binder, "CSharpArgumentInfo", loc), "[]", new ArrayList (0), loc);
			} else {
				real_args = new ImplicitlyTypedArrayCreation ("[]", args.CreateDynamicBinderArguments (), loc);
			}

			binder_args.Add (new Argument (real_args));

			return new New (new MemberAccess (binder,
				is_member_access ? "CSharpInvokeMemberBinder" : "CSharpInvokeBinder", loc), binder_args, loc);
		}
	}

	class DynamicMemberBinder : DynamicExpressionStatement, IDynamicBinder, IAssignMethod
	{
		readonly bool isSet;
		readonly string name;

		public DynamicMemberBinder (bool isSet, string name, Arguments args, Location loc)
			: base (null, args, loc)
		{
			base.binder = this;
			this.isSet = isSet;
			this.name = name;
		}

		public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (3);
			MemberAccess binder = GetBinderNamespace (loc);

			binder_args.Add (new Argument (new StringLiteral (name, loc)));
			binder_args.Add (new Argument (new TypeOf (new TypeExpression (ec.CurrentType, loc), loc)));
			binder_args.Add (new Argument (new ImplicitlyTypedArrayCreation ("[]", args.CreateDynamicBinderArguments (), loc)));

			return new New (new MemberAccess (binder, isSet ? "CSharpSetMemberBinder" : "CSharpGetMemberBinder", loc), binder_args, loc);
		}

		#region IAssignMethod Members

		public void Emit (EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException ();
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			EmitStatement (ec);
		}

		#endregion
	}

	class DynamicUnaryConversion : DynamicExpressionStatement, IDynamicBinder
	{
		string name;

		public DynamicUnaryConversion (string name, Arguments args, Location loc)
			: base (null, args, loc)
		{
			this.name = name;
			base.binder = this;
			if (name == "IsTrue" || name == "IsFalse")
				type = TypeManager.bool_type;
		}

		public Expression CreateCallSiteBinder (ResolveContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (3);

			MemberAccess sle = new MemberAccess (new MemberAccess (
				new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "System", loc), "Linq", loc), "Expressions", loc);

			MemberAccess binder = GetBinderNamespace (loc);

			binder_args.Add (new Argument (new MemberAccess (new MemberAccess (sle, "ExpressionType", loc), name, loc)));
			binder_args.Add (new Argument (new BoolLiteral (ec.HasSet (ResolveContext.Options.CheckedScope), loc)));
			binder_args.Add (new Argument (new ImplicitlyTypedArrayCreation ("[]", args.CreateDynamicBinderArguments (), loc)));

			return new New (new MemberAccess (binder, "CSharpUnaryOperationBinder", loc), binder_args, loc);
		}
	}
}
