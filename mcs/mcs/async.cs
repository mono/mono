//
// async.cs: Asynchronous functions
//
// Author:
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright 2011 Novell, Inc.
//

using System;
using System.Collections.Generic;
using System.Linq;
#if STATIC
using IKVM.Reflection.Emit;
#else
using System.Reflection.Emit;
#endif

namespace Mono.CSharp
{
	class Await : YieldStatement<AsyncInitializer>
	{
		sealed class AwaitableMemberAccess : MemberAccess
		{
			public AwaitableMemberAccess (Expression expr)
				: base (expr, "GetAwaiter")
			{
			}

			protected override void Error_TypeDoesNotContainDefinition (ResolveContext rc, TypeSpec type, string name)
			{
				rc.Report.Error (1986, loc,
					"The `await' operand type `{0}' must have suitable GetAwaiter method",
					type.GetSignatureForError ());
			}
		}

		Field awaiter;
		PropertyExpr is_completed;
		MethodSpec on_completed;

		public Await (Expression expr, Location loc)
			: base (new AwaitableMemberAccess (expr), loc)
		{
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotImplementedException ();
		}

		protected override void DoEmit (EmitContext ec)
		{
			var fe_awaiter = new FieldExpr (awaiter, loc);
			fe_awaiter.InstanceExpression = new CompilerGeneratedThis (ec.CurrentType, loc);

			//
			// awaiter = expr.GetAwaiter ();
			//
			fe_awaiter.EmitAssign (ec, expr, false, false);

			is_completed.InstanceExpression = fe_awaiter;
			is_completed.EmitBranchable (ec, resume_point, true);

			base.DoEmit (ec);

			var mg_completed = MethodGroupExpr.CreatePredefined (on_completed, fe_awaiter.Type, loc);
			mg_completed.InstanceExpression = fe_awaiter;

			var args = new Arguments (1);
			var storey = (AsyncTaskStorey) machine_initializer.Storey;
			var fe_cont = new FieldExpr (storey.Continuation, loc);
			fe_cont.InstanceExpression = new CompilerGeneratedThis (ec.CurrentType, loc);

			args.Add (new Argument (fe_cont));

			//
			// awaiter.OnCompleted (continuation);
			//
			mg_completed.EmitCall (ec, args);
		}

		void Error_WrongAwaiterPattern (ResolveContext rc, TypeSpec awaiter)
		{
			rc.Report.Error (1999, loc, "The awaiter type `{0}' must have suitable IsCompleted, OnCompleted, and GetResult members",
				awaiter.GetSignatureForError ());
		}

		public override bool Resolve (BlockContext bc)
		{
			if (!base.Resolve (bc))
				return false;

			//
			// Check whether the expression is awaitable
			//
			var t = expr.Type;

			//
			// The task t is of type dynamic
			//
			if (t.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				throw new NotImplementedException ("dynamic await");

			var mg = expr as MethodGroupExpr;
			if (mg == null)
				throw new NotImplementedException ("wrong expression kind");

			Arguments args = new Arguments (0);

			//expr = mg.OverloadResolve (bc, ref args, null, OverloadResolver.Restrictions.NoBaseMembers);
			expr = new Invocation (expr, args).Resolve (bc);

			var awaiter_type = expr.Type;
			awaiter = ((AsyncTaskStorey) machine_initializer.Storey).AddAwaiter (awaiter_type, loc);

			//
			// bool IsCompleted { get; } 
			//
			var is_completed_ma = new MemberAccess (expr, "IsCompleted").Resolve (bc);
			if (is_completed_ma != null) {
				is_completed = is_completed_ma as PropertyExpr;
				if (is_completed != null && is_completed.Type.BuiltinType == BuiltinTypeSpec.Type.Bool && is_completed.IsInstance && is_completed.Getter != null) {
					// valid
				} else {
					Error_WrongAwaiterPattern (bc, awaiter_type);
					return false;
				}
			}

			//
			// void OnCompleted (Action)
			//
			if (bc.Module.PredefinedTypes.Action.Define ()) {
				on_completed = MemberCache.FindMember (awaiter_type, MemberFilter.Method ("OnCompleted", 0,
					ParametersCompiled.CreateFullyResolved (bc.Module.PredefinedTypes.Action.TypeSpec), bc.Module.Compiler.BuiltinTypes.Void),
					BindingRestriction.InstanceOnly) as MethodSpec;

				if (on_completed == null) {
					Error_WrongAwaiterPattern (bc, awaiter_type);
					return false;
				}
			}

			return true;
		}
	}

	public class AsyncInitializer : StateMachineInitializer
	{
		public AsyncInitializer (ParametersBlock block, TypeContainer host)
			: base (block, host, host.Compiler.BuiltinTypes.Void)
		{
		}

		public override string ContainerType {
			get {
				return "async state machine block";
			}
		}

		public override bool IsIterator {
			get {
				return false;
			}
		}

		public static void Create (ParametersBlock block, TypeContainer host)
		{
			var init = block.WrapIntoAsyncTask (host);
			init.type = host.Compiler.BuiltinTypes.Void;
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}

		public override void EmitStatement (EmitContext ec)
		{
			storey.Instance.Emit (ec);
			ec.Emit (OpCodes.Call, storey.StateMachineMethod.Spec);
		}
	}

	class AsyncTaskStorey : StateMachine
	{
		int awaiters;
		Field continuation;

		public AsyncTaskStorey (AsyncInitializer initializer)
			: base (initializer.Block, initializer.Host, null, null, "async")
		{
		}

		public Field AddAwaiter (TypeSpec type, Location loc)
		{
			var field = AddCompilerGeneratedField ("$awaiter" + awaiters++.ToString ("X"), new TypeExpression (type, loc));
			return field;
		}

		public Field Continuation {
			get {
				return continuation;
			}
		}

		protected override bool DoDefineMembers ()
		{
			var action = Module.PredefinedTypes.Action.Resolve ();
			if (action != null) {
				continuation = AddCompilerGeneratedField ("$continuation", new TypeExpression (action, Location));
				continuation.ModFlags |= Modifiers.READONLY;
			}

			if (!base.DoDefineMembers ())
				return false;

			//
			// Initialize continuation with state machine method
			//
			if (continuation != null) {
				var args = new Arguments (1);
				var mg = MethodGroupExpr.CreatePredefined (StateMachineMethod.Spec, spec, Location);
				args.Add (new Argument (mg));

				instance_constructors[0].Block.AddStatement (
					new StatementExpression (new SimpleAssign (
						new FieldExpr (continuation, Location),
						new NewDelegate (action, args, Location),
						Location
				)));
			}

			return true;
		}
	}
}
