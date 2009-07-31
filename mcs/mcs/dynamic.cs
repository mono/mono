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

		public override bool CheckAccessLevel (DeclSpace ds)
		{
			return true;
		}

		protected override TypeExpr DoResolveAsTypeStep (IResolveContext ec)
		{
			return this;
		}
	}

	interface IDynamicBinder
	{
		Expression CreateCallSiteBinder (EmitContext ec, Arguments args);
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

		static StaticDataClass site_container;
		static int field_counter;

		readonly Arguments arguments;
		protected IDynamicBinder binder;

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

		protected static Field CreateSiteField (FullNamedExpression type)
		{
			if (site_container == null) {
				site_container = new StaticDataClass ();
				RootContext.ToplevelTypes.AddCompilerGeneratedClass (site_container);
				site_container.DefineType ();
				site_container.Define ();
//				site_container.EmitType ();

				RootContext.RegisterCompilerGeneratedType (site_container.TypeBuilder);
			}

			Field f = new Field (site_container, type, Modifiers.PUBLIC | Modifiers.STATIC,
				new MemberName ("Site" +  field_counter++), null);
			f.Define ();

			site_container.AddField (f);
			return f;
		}

		public override Expression CreateExpressionTree (EmitContext ec)
		{
			throw new NotImplementedException ();
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (TypeManager.call_site_type == null)
				TypeManager.call_site_type = TypeManager.CoreLookupType (
					"System.Runtime.CompilerServices", "CallSite", Kind.Class, true);

			if (TypeManager.generic_call_site_type == null)
				TypeManager.generic_call_site_type = TypeManager.CoreLookupType (
					"System.Runtime.CompilerServices", "CallSite`1", Kind.Class, true);

			eclass = ExprClass.Value;

			if (type == null)
				type = InternalType.Dynamic;

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
			int default_args = isStatement ? 1 : 2;

			string d_name = isStatement ? "Action`" : "Func`";
			Type t = TypeManager.CoreLookupType ("System", d_name + (dyn_args_count + default_args), Kind.Delegate, false);
			if (t == null)
				throw new NotImplementedException ("Create compiler generated delegate");

			FullNamedExpression[] targs = new FullNamedExpression [dyn_args_count + default_args];
			targs [0] = new TypeExpression (TypeManager.call_site_type, loc);
			for (int i = 0; i < dyn_args_count; ++i)
				targs[i + 1] = new TypeExpression (TypeManager.TypeToReflectionType (arguments [i].Type), loc);

			if (!isStatement)
				targs [targs.Length - 1] = new TypeExpression (TypeManager.TypeToReflectionType (type), loc);

			TypeExpr site_type = new GenericTypeExpr (TypeManager.generic_call_site_type, new TypeArguments (new GenericTypeExpr (t, new TypeArguments (targs), loc)), loc);
			FieldExpr site_field_expr = new FieldExpr (CreateSiteField (site_type).FieldBuilder, loc);

			SymbolWriter.OpenCompilerGeneratedBlock (ec.ig);

			Arguments args = new Arguments (1);
			args.Add (new Argument (binder.CreateCallSiteBinder (ec, arguments)));
			StatementExpression s = new StatementExpression (new SimpleAssign (site_field_expr, new Invocation (new MemberAccess (site_type, "Create"), args)));
			if (s.Resolve (ec)) {
				Statement init = new If (new Binary (Binary.Operator.Equality, site_field_expr, new NullLiteral (loc)), s, loc);
				init.Emit (ec);
			}

			args = new Arguments (1 + dyn_args_count);
			args.Add (new Argument (site_field_expr));
			if (arguments != null)
				args.AddRange (arguments);

			Expression target = new DelegateInvocation (new MemberAccess (site_field_expr, "Target", loc).Resolve (ec), args, loc).Resolve (ec);
			if (target != null)
				target.Emit (ec);

			SymbolWriter.CloseCompilerGeneratedBlock (ec.ig);
		}

		public static MemberAccess GetBinderNamespace (Location loc)
		{
			return new MemberAccess (new MemberAccess (
				new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "Microsoft", loc), "CSharp", loc), "RuntimeBinder", loc);
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

		public Expression CreateCallSiteBinder (EmitContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (2);
			MemberAccess binder = GetBinderNamespace (loc);

			binder_args.Add (new Argument (new StringLiteral (name, loc)));
			binder_args.Add (new Argument (new TypeOf (new TypeExpression (ec.ContainerType, loc), loc)));

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
		Type target_type;
		bool is_explicit;

		public DynamicConversion (Type targetType, bool isExplicit, Arguments args, Location loc)
			: base (null, args, loc)
		{
			this.target_type = targetType;
			is_explicit = isExplicit;
			base.binder = this;
		}

		public Expression CreateCallSiteBinder (EmitContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (2);
			MemberAccess binder = GetBinderNamespace (loc);

			binder_args.Add (new Argument (new TypeOf (new TypeExpression (target_type, loc), loc)));
			binder_args.Add (new Argument (new MemberAccess (new MemberAccess (binder, "CSharpConversionKind", loc),
				is_explicit ? "ExplicitConversion" : "ImplicitConversion", loc)));
			binder_args.Add (new Argument (new BoolLiteral (ec.CheckState, loc)));
				
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

		public Expression CreateCallSiteBinder (EmitContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (2);
			MemberAccess binder = GetBinderNamespace (loc);

			binder_args.Add (new Argument (new TypeOf (new TypeExpression (ec.ContainerType, loc), loc)));
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

		public Expression CreateCallSiteBinder (EmitContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (member != null ? 5 : 3);
			MemberAccess binder = GetBinderNamespace (loc);

			// TODO: It's SimpleName for base.ctor ()
			binder_args.Add (new Argument (new MemberAccess (new MemberAccess (binder, "CSharpCallFlags", loc), "None", loc)));
			if (member != null)
				binder_args.Add (new Argument (new StringLiteral (member.Name, member.Location)));

			binder_args.Add (new Argument (new TypeOf (new TypeExpression (ec.ContainerType, loc), loc)));

			// TODO: member_access.TypeArguments
			if (member != null)
				binder_args.Add (new Argument (new NullLiteral (loc)));

			binder_args.Add (new Argument (new ImplicitlyTypedArrayCreation ("[]", args.CreateDynamicBinderArguments (), loc)));

			return new New (new MemberAccess (binder,
				member != null ? "CSharpInvokeMemberBinder" : "CSharpInvokeBinder", loc), binder_args, loc);
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

		public Expression CreateCallSiteBinder (EmitContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (3);
			MemberAccess binder = GetBinderNamespace (loc);

			binder_args.Add (new Argument (new StringLiteral (name, loc)));
			binder_args.Add (new Argument (new TypeOf (new TypeExpression (ec.ContainerType, loc), loc)));
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

		public Expression CreateCallSiteBinder (EmitContext ec, Arguments args)
		{
			Arguments binder_args = new Arguments (3);

			MemberAccess sle = new MemberAccess (new MemberAccess (
				new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "System", loc), "Linq", loc), "Expressions", loc);

			MemberAccess binder = GetBinderNamespace (loc);

			binder_args.Add (new Argument (new MemberAccess (new MemberAccess (sle, "ExpressionType", loc), name, loc)));
			binder_args.Add (new Argument (new BoolLiteral (ec.CheckState, loc)));
			binder_args.Add (new Argument (new ImplicitlyTypedArrayCreation ("[]", args.CreateDynamicBinderArguments (), loc)));

			return new New (new MemberAccess (binder, "CSharpUnaryOperationBinder", loc), binder_args, loc);
		}
	}
}
