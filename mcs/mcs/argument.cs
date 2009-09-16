//
// argument.cs: Argument expressions
//
// Author:
//   Miguel de Icaza (miguel@ximain.com)
//   Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
// Copyright 2003-2008 Novell, Inc.
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	//
	// Argument expression used for invocation
	//
	public class Argument
	{
		public enum AType : byte
		{
			Ref = 1,			// ref modifier used
			Out = 2,			// out modifier used
			Default = 3,		// argument created from default parameter value
			DynamicStatic = 4	// static argument for dynamic binding
		}

		public readonly AType ArgType;
		public Expression Expr;

		public Argument (Expression expr, AType type)
		{
			this.Expr = expr;
			this.ArgType = type;
		}

		public Argument (Expression expr)
		{
			if (expr == null)
				throw new ArgumentNullException ();

			this.Expr = expr;
		}

		public Type Type {
			get { return Expr.Type; }
		}

		public Parameter.Modifier Modifier {
			get {
				switch (ArgType) {
				case AType.Out:
					return Parameter.Modifier.OUT;

				case AType.Ref:
					return Parameter.Modifier.REF;

				default:
					return Parameter.Modifier.NONE;
				}
			}
		}

		public virtual Expression CreateExpressionTree (ResolveContext ec)
		{
			if (ArgType == AType.Default)
				ec.Report.Error (854, Expr.Location, "An expression tree cannot contain an invocation which uses optional parameter");

			return Expr.CreateExpressionTree (ec);
		}

		public string GetSignatureForError ()
		{
			if (Expr.eclass == ExprClass.MethodGroup)
				return Expr.ExprClassName;

			return TypeManager.CSharpName (Expr.Type);
		}

		public bool IsByRef {
			get { return ArgType == AType.Ref || ArgType == AType.Out; }
		}

		public bool IsDefaultArgument {
			get { return ArgType == AType.Default; }
		}

		public bool ResolveMethodGroup (ResolveContext ec)
		{
			SimpleName sn = Expr as SimpleName;
			if (sn != null)
				Expr = sn.GetMethodGroup ();

			// FIXME: csc doesn't report any error if you try to use `ref' or
			//        `out' in a delegate creation expression.
			Expr = Expr.Resolve (ec, ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);
			if (Expr == null)
				return false;

			return true;
		}

		public void Resolve (ResolveContext ec)
		{
			if (Expr == EmptyExpression.Null)
				return;

			using (ec.With (ResolveContext.Options.DoFlowAnalysis, true)) {
				// Verify that the argument is readable
				if (ArgType != AType.Out)
					Expr = Expr.Resolve (ec);

				// Verify that the argument is writeable
				if (Expr != null && IsByRef)
					Expr = Expr.ResolveLValue (ec, EmptyExpression.OutAccess);

				if (Expr == null)
					Expr = EmptyExpression.Null;
			}
		}

		public virtual void Emit (EmitContext ec)
		{
			if (!IsByRef) {
				Expr.Emit (ec);
				return;
			}

			AddressOp mode = AddressOp.Store;
			if (ArgType == AType.Ref)
				mode |= AddressOp.Load;

			IMemoryLocation ml = (IMemoryLocation) Expr;
			ParameterReference pr = ml as ParameterReference;

			//
			// ParameterReferences might already be references, so we want
			// to pass just the value
			//
			if (pr != null && pr.IsRef)
				pr.EmitLoad (ec);
			else
				ml.AddressOf (ec, mode);
		}

		public Argument Clone (CloneContext clonectx)
		{
			Argument a = (Argument) MemberwiseClone ();
			a.Expr = Expr.Clone (clonectx);
			return a;
		}
	}

	public class NamedArgument : Argument
	{
		public readonly LocatedToken Name;
		LocalTemporary variable;

		public NamedArgument (LocatedToken name, Expression expr)
			: base (expr)
		{
			Name = name;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			ec.Report.Error (853, Name.Location, "An expression tree cannot contain named argument");
			return base.CreateExpressionTree (ec);
		}

		public override void Emit (EmitContext ec)
		{
			// TODO: Should guard against multiple emits
			base.Emit (ec);

			// Release temporary variable when used
			if (variable != null)
				variable.Release (ec);
		}

		public void EmitAssign (EmitContext ec)
		{
			Expr.Emit (ec);
			variable = new LocalTemporary (Expr.Type);
			variable.Store (ec);

			Expr = variable;
		}
	}

	public class Arguments
	{
		ArrayList args;			// TODO: This should really be linked list
		ArrayList reordered;	// TODO: LinkedList

		public Arguments (int capacity)
		{
			args = new ArrayList (capacity);
		}

		public int Add (Argument arg)
		{
			return args.Add (arg);
		}

		public void AddRange (Arguments args)
		{
			this.args.AddRange (args.args);
		}

		public ArrayList CreateDynamicBinderArguments ()
		{
			ArrayList all = new ArrayList (args.Count);
			Location loc = Location.Null;

			MemberAccess binder = DynamicExpressionStatement.GetBinderNamespace (loc);

			foreach (Argument a in args) {
				Arguments dargs = new Arguments (2);

				// CSharpArgumentInfoFlags.None = 0
				const string info_flags_enum = "CSharpArgumentInfoFlags";
				Expression info_flags = new IntLiteral (0, loc);

				if (a.Expr is Constant) {
					// Any constant is emitted as a literal
					info_flags = new Binary (Binary.Operator.BitwiseOr, info_flags,
						new MemberAccess (new MemberAccess (binder, info_flags_enum, loc), "LiteralConstant", loc));
				} else if (a.ArgType == Argument.AType.Ref) {
					info_flags = new Binary (Binary.Operator.BitwiseOr, info_flags,
						new MemberAccess (new MemberAccess (binder, info_flags_enum, loc), "IsRef", loc));
				} else if (a.ArgType == Argument.AType.Out) {
					info_flags = new Binary (Binary.Operator.BitwiseOr, info_flags,
						new MemberAccess (new MemberAccess (binder, info_flags_enum, loc), "IsOut", loc));
				} else if (a.ArgType == Argument.AType.DynamicStatic) {
					info_flags = new Binary (Binary.Operator.BitwiseOr, info_flags,
						new MemberAccess (new MemberAccess (binder, info_flags_enum, loc), "IsStaticType", loc));
				}

				if (!TypeManager.IsDynamicType (a.Expr.Type)) {
					info_flags = new Binary (Binary.Operator.BitwiseOr, info_flags,
						new MemberAccess (new MemberAccess (binder, info_flags_enum, loc), "UseCompileTimeType", loc));
				}

				string named_value;
				NamedArgument na = a as NamedArgument;
				if (na != null) {
					info_flags = new Binary (Binary.Operator.BitwiseOr, info_flags,
						new MemberAccess (new MemberAccess (binder, info_flags_enum, loc), "NamedArgument", loc));

					named_value = na.Name.Value;
				} else {
					named_value = null;
				}

				dargs.Add (new Argument (info_flags));
				dargs.Add (new Argument (new StringLiteral (named_value, loc)));
				all.Add (new New (new MemberAccess (binder, "CSharpArgumentInfo", loc), dargs, loc));
			}

			return all;
		}

		public static Arguments CreateForExpressionTree (ResolveContext ec, Arguments args, params Expression[] e)
		{
			Arguments all = new Arguments ((args == null ? 0 : args.Count) + e.Length);
			for (int i = 0; i < e.Length; ++i) {
				if (e [i] != null)
					all.Add (new Argument (e[i]));
			}

			if (args != null) {
				foreach (Argument a in args.args) {
					Expression tree_arg = a.CreateExpressionTree (ec);
					if (tree_arg != null)
						all.Add (new Argument (tree_arg));
				}
			}

			return all;
		}

		public void CheckArrayAsAttribute (CompilerContext ctx)
		{
			foreach (Argument arg in args) {
				// Type is undefined (was error 246)
				if (arg.Type == null)
					continue;

				if (arg.Type.IsArray)
					ctx.Report.Warning (3016, 1, arg.Expr.Location, "Arrays as attribute arguments are not CLS-compliant");
			}
		}

		public Arguments Clone (CloneContext ctx)
		{
			Arguments cloned = new Arguments (args.Count);
			foreach (Argument a in args)
				cloned.Add (a.Clone (ctx));

			return cloned;
		}

		public int Count {
			get { return args.Count; }
		}

		//
		// Emits a list of resolved Arguments
		// 
		public void Emit (EmitContext ec)
		{
			Emit (ec, false, null);
		}

		//
		// if `dup_args' is true, a copy of the arguments will be left
		// on the stack. If `dup_args' is true, you can specify `this_arg'
		// which will be duplicated before any other args. Only EmitCall
		// should be using this interface.
		//
		public void Emit (EmitContext ec, bool dup_args, LocalTemporary this_arg)
		{
			LocalTemporary[] temps = null;

			if (dup_args && Count != 0)
				temps = new LocalTemporary [Count];

			if (reordered != null && Count > 1) {
				foreach (NamedArgument na in reordered)
					na.EmitAssign (ec);
			}

			int i = 0;
			foreach (Argument a in args) {
				a.Emit (ec);
				if (dup_args) {
					ec.ig.Emit (OpCodes.Dup);
					(temps [i++] = new LocalTemporary (a.Type)).Store (ec);
				}
			}

			if (dup_args) {
				if (this_arg != null)
					this_arg.Emit (ec);

				for (i = 0; i < temps.Length; i++) {
					temps[i].Emit (ec);
					temps[i].Release (ec);
				}
			}
		}

		public bool GetAttributableValue (ResolveContext ec, out object[] values)
		{
			values = new object [args.Count];
			for (int j = 0; j < values.Length; ++j) {
				Argument a = this [j];
				if (!a.Expr.GetAttributableValue (ec, a.Type, out values[j]))
					return false;
			}

			return true;
		}

		public IEnumerator GetEnumerator ()
		{
			return args.GetEnumerator ();
		}

		public void Insert (int index, Argument arg)
		{
			args.Insert (index, arg);
		}

#if NET_4_0
		public static System.Linq.Expressions.Expression[] MakeExpression (Arguments args, BuilderContext ctx)
		{
			if (args == null || args.Count == 0)
				return null;

			// TODO: implement
			if (args.reordered != null)
				throw new NotImplementedException ();

			var exprs = new System.Linq.Expressions.Expression [args.Count];
			for (int i = 0; i < exprs.Length; ++i) {
				Argument a = (Argument) args.args [i];
				exprs[i] = a.Expr.MakeExpression (ctx);
			}

			return exprs;
		}
#endif

		public void MarkReorderedArgument (NamedArgument a)
		{
			//
			// Constant expression can have no effect on left-to-right execution
			//
			if (a.Expr is Constant)
				return;

			if (reordered == null)
				reordered = new ArrayList ();

			reordered.Add (a);
		}

		//
		// Returns dynamic when at least one argument is of dynamic type
		//
		public void Resolve (ResolveContext ec, out bool dynamic)
		{
			dynamic = false;
			foreach (Argument a in args) {
				a.Resolve (ec);
				dynamic |= TypeManager.IsDynamicType (a.Type);
			}
		}

		public void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			foreach (Argument a in args)
				a.Expr.MutateHoistedGenericType (storey);
		}

		public void RemoveAt (int index)
		{
			args.RemoveAt (index);
		}

		public Argument this [int index] {
			get { return (Argument) args [index]; }
			set { args [index] = value; }
		}
	}
}
