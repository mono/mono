//
// argument.cs: Arguments representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
//
#define USE_OLD

namespace Mono.CSharp {
	using System;
	using System.Collections;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;

	
	/// <summary>
	///   Used for arguments to New(), Invocation()
	/// </summary>
	public class Argument {
		public enum AType : byte {
			Expression,
			Ref,
			Out,
			NoArg
		};

		public AType ArgType;
		public Expression Expr;
		
		public Argument (Expression expr, AType type)
		{
			this.Expr = expr;
			this.ArgType = type;
		}

		public Type Type {
			get {
				if (ArgType == AType.Ref || ArgType == AType.Out)
					return TypeManager.LookupType (Expr.Type.ToString () + "&");
				else
					return Expr.Type;
			}
		}

		public Parameter.Modifier GetParameterModifier ()
		{
			switch (ArgType) {
			case AType.Out:
				return Parameter.Modifier.OUT | Parameter.Modifier.ISBYREF;

			case AType.Ref:
				return Parameter.Modifier.REF | Parameter.Modifier.ISBYREF;

			default:
				return Parameter.Modifier.NONE;
			}
		}

	        public static string FullDesc (Argument a)
		{
			return (a.ArgType == AType.Ref ? "ref " :
				(a.ArgType == AType.Out ? "out " : "")) +
				TypeManager.CSharpName (a.Expr.Type);
		}

		public bool ResolveMethodGroup (EmitContext ec, Location loc)
		{
			// FIXME: csc doesn't report any error if you try to use `ref' or
			//        `out' in a delegate creation expression.
			Expr = Expr.Resolve (ec, ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);
			if (Expr == null)
				return false;

			return true;
		}

		public bool Resolve (EmitContext ec, Location loc)
		{
			// Optional void arguments - MyCall (1,,2) - are resolved later
			// in VerifyArgsCompat
			if (ArgType == AType.NoArg || ArgType == AType.Ref) 
			{
				return true;				
			}
/*
			if (ArgType == AType.Ref) {
				Expr = Expr.Resolve (ec);
				if (Expr == null)
					return false;

				Expr = Expr.ResolveLValue (ec, Expr);
			} else */if (ArgType == AType.Out)
				Expr = Expr.ResolveLValue (ec, new EmptyExpression ());
			else
				Expr = Expr.Resolve (ec);


			if (Expr == null)
				return false;

			if (ArgType == AType.Expression)
				return true;

			if (Expr.eclass != ExprClass.Variable){
				//
				// We just probe to match the CSC output
				//
				if (Expr.eclass == ExprClass.PropertyAccess ||
				    Expr.eclass == ExprClass.IndexerAccess){
					Report.Error (
						206, loc,
						"A property or indexer can not be passed as an out or ref " +
						"parameter");
				} else {
					Report.Error (
						1510, loc,
						"An lvalue is required as an argument to out or ref");
				}
				return false;
			}
				
			return true;
		}

		public void Emit (EmitContext ec)
		{
			//
			// Ref and Out parameters need to have their addresses taken.
			//
			// ParameterReferences might already be references, so we want
			// to pass just the value
			//
			if (ArgType == AType.Ref || ArgType == AType.Out){
				AddressOp mode = AddressOp.Store;

				if (ArgType == AType.Ref)
					mode |= AddressOp.Load;
				
				if (Expr is ParameterReference){
					ParameterReference pr = (ParameterReference) Expr;

					if (pr.is_ref)
						pr.EmitLoad (ec);
					else {
						
						pr.AddressOf (ec, mode);
					}
				} else
					((IMemoryLocation)Expr).AddressOf (ec, mode);
			} else
				Expr.Emit (ec);
		}
	}
}
