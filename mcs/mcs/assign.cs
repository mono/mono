//
// assign.cs: Assignments.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Martin Baulig (martin@ximian.com)
//   Marek Safar (marek.safar@gmail.com)	
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc.
// Copyright 2004-2008 Novell, Inc
//
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	/// <summary>
	///   This interface is implemented by expressions that can be assigned to.
	/// </summary>
	/// <remarks>
	///   This interface is implemented by Expressions whose values can not
	///   store the result on the top of the stack.
	///
	///   Expressions implementing this (Properties, Indexers and Arrays) would
	///   perform an assignment of the Expression "source" into its final
	///   location.
	///
	///   No values on the top of the stack are expected to be left by
	///   invoking this method.
	/// </remarks>
	public interface IAssignMethod {
		//
		// This is an extra version of Emit. If leave_copy is `true'
		// A copy of the expression will be left on the stack at the
		// end of the code generated for EmitAssign
		//
		void Emit (EmitContext ec, bool leave_copy);

		//
		// This method does the assignment
		// `source' will be stored into the location specified by `this'
		// if `leave_copy' is true, a copy of `source' will be left on the stack
		// if `prepare_for_load' is true, when `source' is emitted, there will
		// be data on the stack that it can use to compuatate its value. This is
		// for expressions like a [f ()] ++, where you can't call `f ()' twice.
		//
		void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load);

		/*
		For simple assignments, this interface is very simple, EmitAssign is called with source
		as the source expression and leave_copy and prepare_for_load false.

		For compound assignments it gets complicated.

		EmitAssign will be called as before, however, prepare_for_load will be
		true. The @source expression will contain an expression
		which calls Emit. So, the calls look like:

		this.EmitAssign (ec, source, false, true) ->
			source.Emit (ec); ->
				[...] ->
					this.Emit (ec, false); ->
					end this.Emit (ec, false); ->
				end [...]
			end source.Emit (ec);
		end this.EmitAssign (ec, source, false, true)


		When prepare_for_load is true, EmitAssign emits a `token' on the stack that
		Emit will use for its state.

		Let's take FieldExpr as an example. assume we are emitting f ().y += 1;

		Here is the call tree again. This time, each call is annotated with the IL
		it produces:

		this.EmitAssign (ec, source, false, true)
			call f
			dup

			Binary.Emit ()
				this.Emit (ec, false);
				ldfld y
				end this.Emit (ec, false);

				IntConstant.Emit ()
				ldc.i4.1
				end IntConstant.Emit

				add
			end Binary.Emit ()

			stfld
		end this.EmitAssign (ec, source, false, true)

		Observe two things:
			1) EmitAssign left a token on the stack. It was the result of f ().
			2) This token was used by Emit

		leave_copy (in both EmitAssign and Emit) tells the compiler to leave a copy
		of the expression at that point in evaluation. This is used for pre/post inc/dec
		and for a = x += y. Let's do the above example with leave_copy true in EmitAssign

		this.EmitAssign (ec, source, true, true)
			call f
			dup

			Binary.Emit ()
				this.Emit (ec, false);
				ldfld y
				end this.Emit (ec, false);

				IntConstant.Emit ()
				ldc.i4.1
				end IntConstant.Emit

				add
			end Binary.Emit ()

			dup
			stloc temp
			stfld
			ldloc temp
		end this.EmitAssign (ec, source, true, true)

		And with it true in Emit

		this.EmitAssign (ec, source, false, true)
			call f
			dup

			Binary.Emit ()
				this.Emit (ec, true);
				ldfld y
				dup
				stloc temp
				end this.Emit (ec, true);

				IntConstant.Emit ()
				ldc.i4.1
				end IntConstant.Emit

				add
			end Binary.Emit ()

			stfld
			ldloc temp
		end this.EmitAssign (ec, source, false, true)

		Note that these two examples are what happens for ++x and x++, respectively.
		*/
	}

	/// <summary>
	///   An Expression to hold a temporary value.
	/// </summary>
	/// <remarks>
	///   The LocalTemporary class is used to hold temporary values of a given
	///   type to "simulate" the expression semantics. The local variable is
	///   never captured.
	///
	///   The local temporary is used to alter the normal flow of code generation
	///   basically it creates a local variable, and its emit instruction generates
	///   code to access this value, return its address or save its value.
	///
	///   If `is_address' is true, then the value that we store is the address to the
	///   real value, and not the value itself.
	///
	///   This is needed for a value type, because otherwise you just end up making a
	///   copy of the value on the stack and modifying it. You really need a pointer
	///   to the origional value so that you can modify it in that location. This
	///   Does not happen with a class because a class is a pointer -- so you always
	///   get the indirection.
	///
	/// </remarks>
	public class LocalTemporary : Expression, IMemoryLocation, IAssignMethod {
		LocalBuilder builder;

		public LocalTemporary (TypeSpec t)
		{
			type = t;
			eclass = ExprClass.Value;
		}

		public LocalTemporary (LocalBuilder b, TypeSpec t)
			: this (t)
		{
			builder = b;
		}

		public void Release (EmitContext ec)
		{
			ec.FreeTemporaryLocal (builder, type);
			builder = null;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (1);
			args.Add (new Argument (this));
			return CreateExpressionFactoryCall (ec, "Constant", args);
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
			if (builder == null)
				throw new InternalErrorException ("Emit without Store, or after Release");

			ec.Emit (OpCodes.Ldloc, builder);
		}

		#region IAssignMethod Members

		public void Emit (EmitContext ec, bool leave_copy)
		{
			Emit (ec);

			if (leave_copy)
				Emit (ec);
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			if (prepare_for_load)
				throw new NotImplementedException ();

			source.Emit (ec);

			Store (ec);

			if (leave_copy)
				Emit (ec);
		}

		#endregion

		public LocalBuilder Builder {
			get { return builder; }
		}

		public void Store (EmitContext ec)
		{
			if (builder == null)
				builder = ec.GetTemporaryLocal (type);

			ec.Emit (OpCodes.Stloc, builder);
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			if (builder == null)
				builder = ec.GetTemporaryLocal (type);

			if (builder.LocalType.IsByRef) {
				//
				// if is_address, than this is just the address anyways,
				// so we just return this.
				//
				ec.Emit (OpCodes.Ldloc, builder);
			} else {
				ec.Emit (OpCodes.Ldloca, builder);
			}
		}
	}

	/// <summary>
	///   The Assign node takes care of assigning the value of source into
	///   the expression represented by target.
	/// </summary>
	public abstract class Assign : ExpressionStatement {
		protected Expression target, source;

		protected Assign (Expression target, Expression source, Location loc)
		{
			this.target = target;
			this.source = source;
			this.loc = loc;
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			ec.Report.Error (832, loc, "An expression tree cannot contain an assignment operator");
			return null;
		}

		public Expression Target {
			get { return target; }
		}

		public Expression Source {
			get {
				return source;
			}
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			bool ok = true;
			source = source.Resolve (ec);
						
			if (source == null) {
				ok = false;
				source = EmptyExpression.Null;
			}

			target = target.ResolveLValue (ec, source);

			if (target == null || !ok)
				return null;

			TypeSpec target_type = target.Type;
			TypeSpec source_type = source.Type;

			eclass = ExprClass.Value;
			type = target_type;

			if (!(target is IAssignMethod)) {
				Error_ValueAssignment (ec, loc);
				return null;
			}

			if (target_type != source_type) {
				Expression resolved = ResolveConversions (ec);

				if (resolved != this)
					return resolved;
			}

			return this;
		}

#if NET_4_0
		public override System.Linq.Expressions.Expression MakeExpression (BuilderContext ctx)
		{
			var tassign = target as IDynamicAssign;
			if (tassign == null)
				throw new InternalErrorException (target.GetType () + " does not support dynamic assignment");

			var target_object = tassign.MakeAssignExpression (ctx, source);

			//
			// Some hacking is needed as DLR does not support void type and requires
			// always have object convertible return type to support caching and chaining
			//
			// We do this by introducing an explicit block which returns RHS value when
			// available or null
			//
			if (target_object.NodeType == System.Linq.Expressions.ExpressionType.Block)
				return target_object;

			var source_object = System.Linq.Expressions.Expression.Convert (source.MakeExpression (ctx), target_object.Type);
			return System.Linq.Expressions.Expression.Assign (target_object, source_object);
		}
#endif
		protected virtual Expression ResolveConversions (ResolveContext ec)
		{
			source = Convert.ImplicitConversionRequired (ec, source, target.Type, loc);
			if (source == null)
				return null;

			return this;
		}

		void Emit (EmitContext ec, bool is_statement)
		{
			IAssignMethod t = (IAssignMethod) target;
			t.EmitAssign (ec, source, !is_statement, this is CompoundAssign);
		}

		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec, true);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			Assign _target = (Assign) t;

			_target.target = target.Clone (clonectx);
			_target.source = source.Clone (clonectx);
		}
	}

	public class SimpleAssign : Assign
	{
		public SimpleAssign (Expression target, Expression source)
			: this (target, source, target.Location)
		{
		}

		public SimpleAssign (Expression target, Expression source, Location loc)
			: base (target, source, loc)
		{
		}

		bool CheckEqualAssign (Expression t)
		{
			if (source is Assign) {
				Assign a = (Assign) source;
				if (t.Equals (a.Target))
					return true;
				return a is SimpleAssign && ((SimpleAssign) a).CheckEqualAssign (t);
			}
			return t.Equals (source);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			Expression e = base.DoResolve (ec);
			if (e == null || e != this)
				return e;

			if (CheckEqualAssign (target))
				ec.Report.Warning (1717, 3, loc, "Assignment made to same variable; did you mean to assign something else?");

			return this;
		}
	}

	public class RuntimeExplicitAssign : Assign
	{
		public RuntimeExplicitAssign (Expression target, Expression source)
			: base (target, source, target.Location)
		{
		}

		protected override Expression ResolveConversions (ResolveContext ec)
		{
			source = Convert.ExplicitConversion (ec, source, target.Type, loc);
			if (source != null)
				return this;

			return base.ResolveConversions (ec);
		}
	}

	//
	// Compiler generated assign
	//
	class CompilerAssign : Assign
	{
		public CompilerAssign (Expression target, Expression source, Location loc)
			: base (target, source, loc)
		{
		}

		public void UpdateSource (Expression source)
		{
			base.source = source;
		}
	}

	//
	// Implements fields and events class initializers
	//
	public class FieldInitializer : Assign
	{
		//
		// Field initializers are tricky for partial classes. They have to
		// share same constructor (block) for expression trees resolve but
		// they have they own resolve scope
		//
		sealed class FieldInitializerContext : ResolveContext
		{
			ExplicitBlock ctor_block;

			public FieldInitializerContext (IMemberContext mc, ResolveContext constructorContext)
				: base (mc, Options.FieldInitializerScope | Options.ConstructorScope)
			{
				this.ctor_block = constructorContext.CurrentBlock.Explicit;
			}

			public override ExplicitBlock ConstructorBlock {
				get {
					return ctor_block;
				}
			}
		}

		//
		// Keep resolved value because field initializers have their own rules
		//
		ExpressionStatement resolved;
		IMemberContext mc;

		public FieldInitializer (FieldSpec spec, Expression expression, IMemberContext mc)
			: base (new FieldExpr (spec, expression.Location), expression, expression.Location)
		{
			this.mc = mc;
			if (!spec.IsStatic)
				((FieldExpr)target).InstanceExpression = CompilerGeneratedThis.Instance;
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			// Field initializer can be resolved (fail) many times
			if (source == null)
				return null;

			if (resolved == null) {
				var ctx = new FieldInitializerContext (mc, ec);
				resolved = base.DoResolve (ctx) as ExpressionStatement;
			}

			return resolved;
		}

		public override void EmitStatement (EmitContext ec)
		{
			if (resolved == null)
				return;
			
			if (resolved != this)
				resolved.EmitStatement (ec);
			else
				base.EmitStatement (ec);
		}
		
		public bool IsComplexInitializer {
			get { return !(source is Constant); }
		}

		public bool IsDefaultInitializer {
			get {
				Constant c = source as Constant;
				if (c == null)
					return false;
				
				FieldExpr fe = (FieldExpr)target;
				return c.IsDefaultInitializer (fe.Type);
			}
		}
	}

	//
	// This class is used for compound assignments.
	//
	public class CompoundAssign : Assign
	{
		// This is just a hack implemented for arrays only
		public sealed class TargetExpression : Expression
		{
			Expression child;
			public TargetExpression (Expression child)
			{
				this.child = child;
				this.loc = child.Location;
			}

			public override Expression CreateExpressionTree (ResolveContext ec)
			{
				throw new NotSupportedException ("ET");
			}

			protected override Expression DoResolve (ResolveContext ec)
			{
				type = child.Type;
				eclass = ExprClass.Value;
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				child.Emit (ec);
			}
		}

		// Used for underlying binary operator
		readonly Binary.Operator op;
		Expression right;
		Expression left;

		public CompoundAssign (Binary.Operator op, Expression target, Expression source, Location loc)
			: base (target, source, loc)
		{
			right = source;
			this.op = op;
		}

		public CompoundAssign (Binary.Operator op, Expression target, Expression source, Expression left, Location loc)
			: this (op, target, source, loc)
		{
			this.left = left;
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			right = right.Resolve (ec);
			if (right == null)
				return null;

			MemberAccess ma = target as MemberAccess;
			using (ec.Set (ResolveContext.Options.CompoundAssignmentScope)) {
				target = target.Resolve (ec);
			}
			
			if (target == null)
				return null;

			if (target is MethodGroupExpr){
				ec.Report.Error (1656, loc,
					"Cannot assign to `{0}' because it is a `{1}'",
					((MethodGroupExpr)target).Name, target.ExprClassName);
				return null;
			}

			var event_expr = target as EventExpr;
			if (event_expr != null) {
				source = Convert.ImplicitConversionRequired (ec, right, target.Type, loc);
				if (source == null)
					return null;

				Expression rside;
				if (op == Binary.Operator.Addition)
					rside = EmptyExpression.EventAddition;
				else if (op == Binary.Operator.Subtraction)
					rside = EmptyExpression.EventSubtraction;
				else
					rside = null;

				target = target.ResolveLValue (ec, rside);
				if (target == null)
					return null;

				eclass = ExprClass.Value;
				type = event_expr.Operator.ReturnType;
				return this;
			}

			//
			// Only now we can decouple the original source/target
			// into a tree, to guarantee that we do not have side
			// effects.
			//
			if (left == null)
				left = new TargetExpression (target);

			source = new Binary (op, left, right, true, loc);

			if (target is DynamicMemberAssignable) {
				Arguments targs = ((DynamicMemberAssignable) target).Arguments;
				source = source.Resolve (ec);

				Arguments args = new Arguments (targs.Count + 1);
				args.AddRange (targs);
				args.Add (new Argument (source));
				if (target is DynamicMemberBinder) {
					source = new DynamicMemberBinder (ma.Name, CSharpBinderFlags.ValueFromCompoundAssignment, args, loc).Resolve (ec);

					// Handles possible event addition/subtraction
					if (op == Binary.Operator.Addition || op == Binary.Operator.Subtraction) {
						args = new Arguments (targs.Count + 1);
						args.AddRange (targs);
						args.Add (new Argument (right));
						string method_prefix = op == Binary.Operator.Addition ?
							Event.AEventAccessor.AddPrefix : Event.AEventAccessor.RemovePrefix;

						var invoke = DynamicInvocation.CreateSpecialNameInvoke (
							new MemberAccess (right, method_prefix + ma.Name, loc), args, loc).Resolve (ec);

						args = new Arguments (targs.Count);
						args.AddRange (targs);
						source = new DynamicEventCompoundAssign (ma.Name, args,
							(ExpressionStatement) source, (ExpressionStatement) invoke, loc).Resolve (ec);
					}
				} else {
					source = new DynamicIndexBinder (CSharpBinderFlags.ValueFromCompoundAssignment, args, loc).Resolve (ec);
				}

				return source;
			}

			return base.DoResolve (ec);
		}

		protected override Expression ResolveConversions (ResolveContext ec)
		{
			//
			// LAMESPEC: Under dynamic context no target conversion is happening
			// This allows more natual dynamic behaviour but breaks compatibility
			// with static binding
			//
			if (target is RuntimeValueExpression)
				return this;

			TypeSpec target_type = target.Type;

			//
			// 1. the return type is implicitly convertible to the type of target
			//
			if (Convert.ImplicitConversionExists (ec, source, target_type)) {
				source = Convert.ImplicitConversion (ec, source, target_type, loc);
				return this;
			}

			//
			// Otherwise, if the selected operator is a predefined operator
			//
			Binary b = source as Binary;
			if (b == null && source is ReducedExpression)
				b = ((ReducedExpression) source).OriginalExpression as Binary;

			if (b != null) {
				//
				// 2a. the operator is a shift operator
				//
				// 2b. the return type is explicitly convertible to the type of x, and
				// y is implicitly convertible to the type of x
				//
				if ((b.Oper & Binary.Operator.ShiftMask) != 0 ||
					Convert.ImplicitConversionExists (ec, right, target_type)) {
					source = Convert.ExplicitConversion (ec, source, target_type, loc);
					return this;
				}
			}

			if (source.Type == InternalType.Dynamic) {
				Arguments arg = new Arguments (1);
				arg.Add (new Argument (source));
				return new SimpleAssign (target, new DynamicConversion (target_type, CSharpBinderFlags.ConvertExplicit, arg, loc), loc).Resolve (ec);
			}

			right.Error_ValueCannotBeConverted (ec, loc, target_type, false);
			return null;
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			CompoundAssign ctarget = (CompoundAssign) t;

			ctarget.right = ctarget.source = source.Clone (clonectx);
			ctarget.target = target.Clone (clonectx);
		}
	}
}
