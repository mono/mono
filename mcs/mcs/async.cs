//
// async.cs: Asynchronous functions
//
// Author:
//   Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2011 Novell, Inc.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

#if STATIC
using IKVM.Reflection.Emit;
#else
using System.Reflection.Emit;
#endif

namespace Mono.CSharp
{
	class Await : ExpressionStatement
	{
		Expression expr;
		AwaitStatement stmt;

		public Await (Expression expr, Location loc)
		{
			this.expr = expr;
			this.loc = loc;
		}

		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			var t = (Await) target;

			t.expr = expr.Clone (clonectx);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotImplementedException ("ET");
		}

		public override bool ContainsEmitWithAwait ()
		{
			return true;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			if (rc.HasSet (ResolveContext.Options.FinallyScope)) {
				rc.Report.Error (1984, loc,
					"The `await' operator cannot be used in the body of a finally clause");
			}

			if (rc.HasSet (ResolveContext.Options.CatchScope)) {
				rc.Report.Error (1985, loc,
					"The `await' operator cannot be used in the body of a catch clause");
			}

			if (rc.HasSet (ResolveContext.Options.LockScope)) {
				rc.Report.Error (1996, loc,
					"The `await' operator cannot be used in the body of a lock statement");
			}

			if (rc.HasSet (ResolveContext.Options.ExpressionTreeConversion)) {
				rc.Report.Error (1989, loc, "An expression tree cannot contain an await operator");
				return null;
			}

			if (rc.IsUnsafe) {
				// TODO: New error code
				rc.Report.Error (-1900, loc,
					"The `await' operator cannot be used in an unsafe context");
			}

			var bc = (BlockContext) rc;

			if (!bc.CurrentBlock.ParametersBlock.IsAsync) {
				// TODO: Should check for existence of await type but
				// what to do with it
			}

			stmt = new AwaitStatement (expr, loc);
			if (!stmt.Resolve (bc))
				return null;

			type = stmt.ResultType;
			eclass = ExprClass.Variable;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			stmt.EmitPrologue (ec);
			stmt.Emit (ec);
		}
		
		public override Expression EmitToField (EmitContext ec)
		{
			stmt.EmitPrologue (ec);
			return stmt.GetResultExpression (ec);
		}
		
		public void EmitAssign (EmitContext ec, FieldExpr field)
		{
			stmt.EmitPrologue (ec);
			field.InstanceExpression.Emit (ec);
			stmt.Emit (ec);
		}

		public override void EmitStatement (EmitContext ec)
		{
			stmt.EmitStatement (ec);
		}
	}

	class AwaitStatement : YieldStatement<AsyncInitializer>
	{
		sealed class AwaitableMemberAccess : MemberAccess
		{
			public AwaitableMemberAccess (Expression expr)
				: base (expr, "GetAwaiter")
			{
			}

			protected override void Error_TypeDoesNotContainDefinition (ResolveContext rc, TypeSpec type, string name)
			{
				Error_WrongGetAwaiter (rc, loc, type);
			}

			protected override void Error_OperatorCannotBeApplied (ResolveContext rc, TypeSpec type)
			{
				rc.Report.Error (1991, loc, "Cannot await `{0}' expression", type.GetSignatureForError ());
			}
		}

		Field awaiter;
		PropertyExpr is_completed;
		MethodSpec on_completed;
		MethodSpec get_result;
		TypeSpec type;

		public AwaitStatement (Expression expr, Location loc)
			: base (expr, loc)
		{
		}

		#region Properties

		public TypeSpec Type {
			get {
				return type;
			}
		}

		public TypeSpec ResultType {
			get {
				return get_result.ReturnType;
			}
		}

		#endregion

		protected override void DoEmit (EmitContext ec)
		{
			GetResultExpression (ec).Emit (ec);
		}

		public Invocation GetResultExpression (EmitContext ec)
		{
			var fe_awaiter = new FieldExpr (awaiter, loc);
			fe_awaiter.InstanceExpression = new CompilerGeneratedThis (ec.CurrentType, loc);

			//
			// result = awaiter.GetResult ();
			//
			var mg_result = MethodGroupExpr.CreatePredefined (get_result, fe_awaiter.Type, loc);
			mg_result.InstanceExpression = fe_awaiter;

			return Invocation.CreatePredefined (mg_result, new Arguments (0));
		}

		public void EmitPrologue (EmitContext ec)
		{
			var fe_awaiter = new FieldExpr (awaiter, loc);
			fe_awaiter.InstanceExpression = new CompilerGeneratedThis (ec.CurrentType, loc);

			//
			// awaiter = expr.GetAwaiter ();
			//
			fe_awaiter.EmitAssign (ec, expr, false, false);

			Label skip_continuation = ec.DefineLabel ();

			is_completed.InstanceExpression = fe_awaiter;
			is_completed.EmitBranchable (ec, skip_continuation, true);

			base.DoEmit (ec);

			FieldSpec[] stack_fields = null;
			TypeSpec[] stack = null;
			//
			// Here is the clever bit. We know that await statement has to yield the control
			// back but it can appear inside almost any expression. This means the stack can
			// contain any depth of values and same values have to be present when the continuation
			// handles control back.
			//
			// For example: await a + await b
			//
			// In this case we fabricate a static stack forwarding method which moves the values
			// from the stack to async storey fields. On re-entry point we restore exactly same
			// stack using these fields.
			//
			// We fabricate a static method because we don't want to touch original stack and
			// the instance method would require `this' as the first stack value on the stack
			//
			if (ec.StackHeight > 0) {
				var async_storey = (AsyncTaskStorey) machine_initializer.Storey;

				stack = ec.GetStackTypes ();
				bool explicit_this;
				var method = async_storey.GetStackForwarder (stack, out stack_fields, out explicit_this);
				if (explicit_this)
					ec.EmitThis ();

				ec.Emit (OpCodes.Call, method);
			}

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

			// Return ok
			machine_initializer.EmitLeave (ec, unwind_protect);

			ec.MarkLabel (resume_point);

			if (stack_fields != null) {
				for (int i = 0; i < stack_fields.Length; ++i) {
					ec.EmitThis ();

					var field = stack_fields[i];

					//
					// We don't store `this' because it can be easily re-created
					//
					if (field == null)
						continue;

					if (stack[i] is ReferenceContainer)
						ec.Emit (OpCodes.Ldflda, field);
					else
						ec.Emit (OpCodes.Ldfld, field);
				}
			}

			ec.MarkLabel (skip_continuation);
		}

		public void EmitStatement (EmitContext ec)
		{
			EmitPrologue (ec);
			Emit (ec);

			if (ResultType.Kind != MemberKind.Void) {
				var storey = (AsyncTaskStorey) machine_initializer.Storey;

			    if (storey.HoistedReturn != null)
			        storey.HoistedReturn.EmitAssign (ec);
				else
					ec.Emit (OpCodes.Pop);
			}
		}

		static void Error_WrongGetAwaiter (ResolveContext rc, Location loc, TypeSpec type)
		{
			rc.Report.Error (1986, loc,
				"The `await' operand type `{0}' must have suitable GetAwaiter method",
				type.GetSignatureForError ());
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

			type = expr.Type;

			//
			// The task result is of dynamic type
			//
			if (expr.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				throw new NotImplementedException ("dynamic await");

			//
			// Check whether the expression is awaitable
			//
			Expression ama = new AwaitableMemberAccess (expr).Resolve (bc);
			if (ama == null)
				return false;

			Arguments args = new Arguments (0);

			var errors_printer = new SessionReportPrinter ();
			var old = bc.Report.SetPrinter (errors_printer);
			ama = new Invocation (ama, args).Resolve (bc);

			if (errors_printer.ErrorsCount > 0 || !MemberAccess.IsValidDotExpression (ama.Type)) {
				bc.Report.SetPrinter (old);
				Error_WrongGetAwaiter (bc, loc, expr.Type);
				return false;
			}

			var awaiter_type = ama.Type;
			awaiter = ((AsyncTaskStorey) machine_initializer.Storey).AddAwaiter (awaiter_type, loc);
			expr = ama;

			//
			// Predefined: bool IsCompleted { get; } 
			//
			var is_completed_ma = new MemberAccess (expr, "IsCompleted").Resolve (bc);
			if (is_completed_ma != null) {
				is_completed = is_completed_ma as PropertyExpr;
				if (is_completed != null && is_completed.Type.BuiltinType == BuiltinTypeSpec.Type.Bool && is_completed.IsInstance && is_completed.Getter != null) {
					// valid
				} else {
					bc.Report.SetPrinter (old);
					Error_WrongAwaiterPattern (bc, awaiter_type);
					return false;
				}
			}

			bc.Report.SetPrinter (old);

			if (errors_printer.ErrorsCount > 0) {
				Error_WrongAwaiterPattern (bc, awaiter_type);
				return false;
			}

			//
			// Predefined: OnCompleted (Action)
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

			//
			// Predefined: GetResult ()
			//
			// The method return type is also result type of await expression
			//
			get_result = MemberCache.FindMember (awaiter_type, MemberFilter.Method ("GetResult", 0,
				ParametersCompiled.EmptyReadOnlyParameters, null),
				BindingRestriction.InstanceOnly) as MethodSpec;

			if (get_result == null) {
				Error_WrongAwaiterPattern (bc, awaiter_type);
				return false;
			}

			return true;
		}
	}

	public class AsyncInitializer : StateMachineInitializer
	{
		TypeInferenceContext return_inference;

		public AsyncInitializer (ParametersBlock block, TypeContainer host, TypeSpec returnType)
			: base (block, host, returnType)
		{
		}

		#region Properties

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

		public Block OriginalBlock {
			get {
				return block.Parent;
			}
		}

		public TypeInferenceContext ReturnTypeInference {
			get {
				return return_inference;
			}
		}

		#endregion

		public static void Create (ParametersBlock block, ParametersCompiled parameters, TypeContainer host, TypeSpec returnType, Location loc)
		{
			if (returnType != null && returnType.Kind != MemberKind.Void &&
				returnType != host.Module.PredefinedTypes.Task.TypeSpec &&
				!returnType.IsGenericTask) {
				host.Compiler.Report.Error (1983, loc, "The return type of an async method must be void, Task, or Task<T>");
			}

			for (int i = 0; i < parameters.Count; i++) {
				Parameter p = parameters[i];
				Parameter.Modifier mod = p.ModFlags;
				if ((mod & Parameter.Modifier.ISBYREF) != 0) {
					host.Compiler.Report.Error (1988, p.Location,
						"Async methods cannot have ref or out parameters");
					return;
				}

				// TODO:
				if (p is ArglistParameter) {
					host.Compiler.Report.Error (1636, p.Location,
						"__arglist is not allowed in parameter list of iterators");
					return;
				}

				// TODO:
				if (parameters.Types[i].IsPointer) {
					host.Compiler.Report.Error (1637, p.Location,
						"Iterators cannot have unsafe parameters or yield types");
					return;
				}
			}

			// TODO: Warning
			//if (!block.HasAwait) {
			//}

			block.WrapIntoAsyncTask (host, returnType);
		}

		protected override BlockContext CreateBlockContext (ResolveContext rc)
		{
			var ctx = base.CreateBlockContext (rc);
			var lambda = rc.CurrentAnonymousMethod as LambdaMethod;
			if (lambda != null)
				return_inference = lambda.ReturnTypeInference;

			return ctx;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return base.CreateExpressionTree (ec);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}

		protected override void EmitMoveNextEpilogue (EmitContext ec)
		{
			var storey = (AsyncTaskStorey) Storey;
			storey.EmitSetResult (ec);
		}

		public override void EmitStatement (EmitContext ec)
		{
			var storey = (AsyncTaskStorey) Storey;
			storey.Instance.Emit (ec);
			ec.Emit (OpCodes.Call, storey.StateMachineMethod.Spec);

			if (storey.Task != null) {
				//
				// async.$builder.Task;
				//
				var pe_task = new PropertyExpr (storey.Task, loc) {
					InstanceExpression = new FieldExpr (storey.Builder, loc) {
						InstanceExpression = storey.Instance
					},
					Getter = storey.Task.Get
				};

				pe_task.Emit (ec);
			}

			ec.Emit (OpCodes.Ret);
		}
	}

	class AsyncTaskStorey : StateMachine
	{
		sealed class ParametersLoadStatement : Statement
		{
			readonly FieldSpec[] fields;
			readonly TypeSpec[] parametersTypes;
			readonly int thisParameterIndex;

			public ParametersLoadStatement (FieldSpec[] fields, TypeSpec[] parametersTypes, int thisParameterIndex)
			{
				this.fields = fields;
				this.parametersTypes = parametersTypes;
				this.thisParameterIndex = thisParameterIndex;
			}

			protected override void CloneTo (CloneContext clonectx, Statement target)
			{
				throw new NotImplementedException ();
			}

			protected override void DoEmit (EmitContext ec)
			{
				for (int i = 0; i < fields.Length; ++i) {
					var field = fields[i];
					if (field == null)
						continue;

					ec.EmitArgumentLoad (thisParameterIndex);
					ec.EmitArgumentLoad (i);
					if (parametersTypes[i] is ReferenceContainer)
						ec.EmitLoadFromPtr (field.MemberType);

					ec.Emit (OpCodes.Stfld, field);
				}
			}
		}

		int awaiters;
		Field builder, continuation;
		readonly TypeSpec return_type;
		MethodSpec set_result;
		PropertySpec task;
		LocalVariable hoisted_return;
		Dictionary<TypeSpec[], Tuple<MethodSpec, FieldSpec[], bool>> stack_forwarders;
		List<FieldSpec> hoisted_stack_slots;

		public AsyncTaskStorey (AsyncInitializer initializer, TypeSpec type)
			: base (initializer.OriginalBlock, initializer.Host, null, null, "async")
		{
			return_type = type;
		}

		#region Properties

		public Field Builder {
			get {
				return builder;
			}
		}

		public Field Continuation {
			get {
				return continuation;
			}
		}

		public LocalVariable HoistedReturn {
			get {
				return hoisted_return;
			}
		}

		public TypeSpec ReturnType {
			get {
				return return_type;
			}
		}

		public PropertySpec Task {
			get {
				return task;
			}
		}

		#endregion

		public Field AddAwaiter (TypeSpec type, Location loc)
		{
			return AddCompilerGeneratedField ("$awaiter" + awaiters++.ToString ("X"), new TypeExpression (type, loc));
		}

		int locals_captured;

		public Field AddCapturedLocalVariable (TypeSpec type)
		{
			var field = AddCompilerGeneratedField ("<s>$" + locals_captured++.ToString ("X"), new TypeExpression (type, Location));
			field.Define ();

			return field;
		}

		FieldSpec CreateStackValueField (TypeSpec type, BitArray usedFields)
		{
			if (hoisted_stack_slots == null) {
				hoisted_stack_slots = new List<FieldSpec> ();
			} else {
				for (int i = 0; i < usedFields.Count; ++i) {
					if (hoisted_stack_slots[i].MemberType == type && !usedFields[i]) {
						usedFields.Set (i, true);
						return hoisted_stack_slots[i];
					}
				}
			}

			var field = AddCompilerGeneratedField ("<s>$" + hoisted_stack_slots.Count.ToString ("X"), new TypeExpression (type, Location));
			field.Define ();

			hoisted_stack_slots.Add (field.Spec);
			return field.Spec;
		}

		protected override bool DoDefineMembers ()
		{
			var action = Module.PredefinedTypes.Action.Resolve ();
			if (action != null) {
				continuation = AddCompilerGeneratedField ("$continuation", new TypeExpression (action, Location));
				continuation.ModFlags |= Modifiers.READONLY;
			}

			PredefinedType builder_type;
			PredefinedMember<MethodSpec> bf;
			PredefinedMember<MethodSpec> sr;
			bool has_task_return_type = false;
			var pred_members = Module.PredefinedMembers;

			if (return_type.Kind == MemberKind.Void) {
				builder_type = Module.PredefinedTypes.AsyncVoidMethodBuilder;
				bf = pred_members.AsyncVoidMethodBuilderCreate;
				sr = pred_members.AsyncVoidMethodBuilderSetResult;
			} else if (return_type == Module.PredefinedTypes.Task.TypeSpec) {
				builder_type = Module.PredefinedTypes.AsyncTaskMethodBuilder;
				bf = pred_members.AsyncTaskMethodBuilderCreate;
				sr = pred_members.AsyncTaskMethodBuilderSetResult;
				task = pred_members.AsyncTaskMethodBuilderTask.Resolve (Location);
			} else {
				builder_type = Module.PredefinedTypes.AsyncTaskMethodBuilderGeneric;
				bf = pred_members.AsyncTaskMethodBuilderGenericCreate;
				sr = pred_members.AsyncTaskMethodBuilderGenericSetResult;
				task = pred_members.AsyncTaskMethodBuilderGenericTask.Resolve (Location);
				has_task_return_type = true;
			}

			set_result = sr.Resolve (Location);
			var builder_factory = bf.Resolve (Location);
			var bt = builder_type.Resolve ();
			if (bt == null || set_result == null || builder_factory == null)
				return false;

			//
			// Inflate generic Task types
			//
			if (has_task_return_type) {
				bt = bt.MakeGenericType (Module, return_type.TypeArguments);
				builder_factory = MemberCache.GetMember<MethodSpec> (bt, builder_factory);
				set_result = MemberCache.GetMember<MethodSpec> (bt, set_result);

				if (task != null)
					task = MemberCache.GetMember<PropertySpec> (bt, task);
			}

			builder = AddCompilerGeneratedField ("$builder", new TypeExpression (bt, Location));
			builder.ModFlags |= Modifiers.READONLY;

			if (!base.DoDefineMembers ())
				return false;

			MethodGroupExpr mg;
			var block = instance_constructors[0].Block;

			//
			// Initialize continuation with state machine method
			//
			if (continuation != null) {
				var args = new Arguments (1);
				mg = MethodGroupExpr.CreatePredefined (StateMachineMethod.Spec, spec, Location);
				args.Add (new Argument (mg));

				block.AddStatement (
					new StatementExpression (new SimpleAssign (
						new FieldExpr (continuation, Location),
						new NewDelegate (action, args, Location),
						Location
				)));
			}

			mg = MethodGroupExpr.CreatePredefined (builder_factory, bt, Location);
			block.AddStatement (
				new StatementExpression (new SimpleAssign (
				new FieldExpr (builder, Location),
				new Invocation (mg, new Arguments (0)),
				Location)));

			if (has_task_return_type) {
				hoisted_return = LocalVariable.CreateCompilerGenerated (bt.TypeArguments[0], block, Location);
			}

			return true;
		}

		public void EmitSetResult (EmitContext ec)
		{
			//
			// $builder.SetResult ();
			// $builder.SetResult<return-type> (value);
			//
			var mg = MethodGroupExpr.CreatePredefined (set_result, set_result.DeclaringType, Location);
			mg.InstanceExpression = new FieldExpr (Builder, Location) {
				InstanceExpression = new CompilerGeneratedThis (ec.CurrentType, Location)
			};

			Arguments args;
			if (hoisted_return == null) {
				args = new Arguments (0);
			} else {
				args = new Arguments (1);
				args.Add (new Argument (new LocalVariableReference (hoisted_return, Location)));
			}

			mg.EmitCall (ec, args);
		}

		//
		// Fabricates stack forwarder based on stack types which copies all
		// parameters to type fields
		//
		public MethodSpec GetStackForwarder (TypeSpec[] types, out FieldSpec[] fields, out bool explicitThisNeeded)
		{
			if (stack_forwarders == null) {
				stack_forwarders = new Dictionary<TypeSpec[], Tuple<MethodSpec, FieldSpec[], bool>> (TypeSpecComparer.Default);
			} else {
				//
				// Does same forwarder method with same types already exist
				//
				Tuple<MethodSpec, FieldSpec[], bool> method;
				if (stack_forwarders.TryGetValue (types, out method)) {
					fields = method.Item2;
					explicitThisNeeded = method.Item3;
					return method.Item1;
				}
			}

			Parameter[] p = new Parameter[types.Length + 1];
			TypeSpec[] ptypes = new TypeSpec[p.Length];
			fields = new FieldSpec[types.Length];
			int this_argument_index = -1;
			BitArray used_fields_map = null;

			for (int i = 0; i < types.Length; ++i) {
				var t = types[i];

				TypeSpec parameter_type = t;
				if (parameter_type == InternalType.CurrentTypeOnStack) {
					parameter_type = CurrentType;
				}

				p[i] = new Parameter (new TypeExpression (parameter_type, Location), null, 0, null, Location);
				ptypes[i] = parameter_type;

				if (t == InternalType.CurrentTypeOnStack) {
					if (this_argument_index < 0)
						this_argument_index = i;

					// Null means the type is `this' we can optimize by ignoring
					continue;
				}

				var reference = t as ReferenceContainer;
				if (reference != null)
					t = reference.Element;

				if (used_fields_map == null)
					used_fields_map = new BitArray (hoisted_stack_slots == null ? 0 : hoisted_stack_slots.Count);

				fields[i] = CreateStackValueField (t, used_fields_map);
			}

			//
			// None of the arguments is `this' need to add an extra parameter to pass it
			// to static forwarder method
			//
			if (this_argument_index < 0) {
				explicitThisNeeded = true;
				this_argument_index = types.Length;
				var this_parameter = new Parameter (new TypeExpression (CurrentType, Location), null, 0, null, Location);
				p[types.Length] = this_parameter;
				ptypes[types.Length] = CurrentType;
			} else {
				explicitThisNeeded = false;
				Array.Resize (ref p, p.Length - 1);
				Array.Resize (ref ptypes, ptypes.Length - 1);
			}

			var parameters = ParametersCompiled.CreateFullyResolved (p, ptypes);

			var m = new Method (this, null, new TypeExpression (Compiler.BuiltinTypes.Void, Location),
				Modifiers.STATIC | Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED | Modifiers.DEBUGGER_HIDDEN,
				new MemberName ("<>s__" + stack_forwarders.Count.ToString ("X")), parameters, null);

			m.Block = new ToplevelBlock (Compiler, parameters, Location);
			m.Block.AddScopeStatement (new ParametersLoadStatement (fields, ptypes, this_argument_index));

			m.Define ();
			Methods.Add (m);

			stack_forwarders.Add (types, Tuple.Create (m.Spec, fields, explicitThisNeeded));

			return m.Spec;
		}
	}
}
