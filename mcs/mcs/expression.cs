//
// expression.cs: Expression representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
namespace CIR {
	using System.Collections;
	using System.Diagnostics;
	using System;

	// <remarks>
	//   The ExprClass class contains the is used to pass the 
	//   classification of an expression (value, variable, namespace,
	//   type, method group, property access, event access, indexer access,
	//   nothing).
	// </remarks>
	public enum ExprClass {
		Invalid,
		
		Value, Variable, Namespace, Type,
		MethodGroup, PropertyAccess,
		EventAccess, IndexerAccess, Nothing, 
	}

	// <remarks>
	//   Base class for expressions
	// </remarks>
	public abstract class Expression {
		protected ExprClass eclass;
		protected Type      type;
		
		public Type Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}

		public ExprClass ExprClass {
			get {
				return eclass;
			}

			set {
				eclass = value;
			}
		}

		public abstract void Resolve (TypeContainer tc);
		public abstract void Emit    (EmitContext ec);
		
		// <summary>
		//   Protected constructor.  Only derivate types should
		//   be able to be created
		// </summary>

		protected Expression ()
		{
			eclass = ExprClass.Invalid;
			type = null;
		}

		static public Expression ResolveSimpleName (TypeContainer tc, string name)
		{
			return null;
		}

		// <summary>
		//   Resolves the E in `E.I' side for a member_access
		static Expression ResolvePrimary (TypeContainer tc, string name)
		{
			int dot_pos = name.LastIndexOf (".");

			if (tc.RootContext.IsNamespace (name))
				return new NamespaceExpr (name);

			if (dot_pos != -1){
			} else {
			}

			return null;
		}
			
		static public Expression ResolveName (TypeContainer tc, string name)
		{
			int dot_pos = name.LastIndexOf (".");
			
			if (dot_pos == -1){
				return ResolveSimpleName (tc, name);
			} else {
				Expression left_e;
				string left = name.Substring (0, dot_pos);
				string right = name.Substring (dot_pos + 1);

				left_e = ResolvePrimary (tc, left);
			}

			return null;
		}
		
	}

	public class Unary : Expression {
		public enum Operator {
			Plus, Minus, Negate, BitComplement,
			Indirection, AddressOf, PreIncrement,
			PreDecrement, PostIncrement, PostDecrement
		}

		Operator   oper;
		Expression expr;
		
		public Unary (Operator op, Expression expr)
		{
			this.oper = op;
			this.expr = expr;
		}

		public Expression Expr {
			get {
				return expr;
			}

			set {
				expr = value;
			}
		}

		public Operator Oper {
			get {
				return oper;
			}

			set {
				oper = value;
			}
		}

		public override void Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class Probe : Expression {
		string probe_type;
		Expression expr;
		Operator oper;

		public enum Operator {
			Is, As
		}
		
		public Probe (Operator oper, Expression expr, string probe_type)
		{
			this.oper = oper;
			this.probe_type = probe_type;
			this.expr = expr;
		}

		public Operator Oper {
			get {
				return oper;
			}
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		public string ProbeType {
			get {
				return probe_type;
			}
		}

		public override void Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}
	
	public class Cast : Expression {
		string target_type;
		Expression expr;
		
		public Cast (string cast_type, Expression expr)
		{
			this.target_type = target_type;
			this.expr = expr;
		}

		public string TargetType {
			get {
				return target_type;
			}
		}

		public Expression Expr {
			get {
				return expr;
			}
			set {
				expr = value;
			}
		}
		
		public override void Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class Binary : Expression {
		public enum Operator {
			Multiply, Divide, Modulo,
			Add, Substract,
			ShiftLeft, ShiftRight,
			LessThan, GreatherThan, LessOrEqual, GreatherOrEqual, 
			Equal, NotEqual,
			BitwiseAnd,
			ExclusiveOr,
			BitwiseOr,
			LogicalAnd,
			LogicalOr
		}

		Operator oper;
		Expression left, right;
		
		public Binary (Operator oper, Expression left, Expression right)
		{
			this.oper = oper;
			this.left = left;
			this.right = right;
		}

		public Operator Oper {
			get {
				return oper;
			}
			set {
				oper = value;
			}
		}
		
		public Expression Left {
			get {
				return left;
			}
			set {
				left = value;
			}
		}

		public Expression Right {
			get {
				return right;
			}
			set {
				right = value;
			}
		}

		public override void Resolve (TypeContainer tc)
		{
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class Conditional : Expression {
		Expression expr, trueExpr, falseExpr;
		
		public Conditional (Expression expr, Expression trueExpr, Expression falseExpr)
		{
			this.expr = expr;
			this.trueExpr = trueExpr;
			this.falseExpr = falseExpr;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		public Expression TrueExpr {
			get {
				return trueExpr;
			}
		}

		public Expression FalseExpr {
			get {
				return falseExpr;
			}
		}

		public override void Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class SimpleName : Expression {
		string name;
		
		public SimpleName (string name)
		{
			this.name = name;
		}

		public string Name {
			get {
				return name;
			}
		}

		//
		// SimpleName needs to handle a multitude of cases:
		//
		// simple_names and qualified_identifiers are placed on
		// the tree equally.
		//
		public override void Resolve (TypeContainer tc)
		{
			ResolveName (tc, name);
		}

		public override void Emit (EmitContext ec)
		{
		}
	}
	
	public class LocalVariableReference : Expression {
		string name;
		Block block;
		
		public LocalVariableReference (Block block, string name)
		{
			this.block = block;
			this.name = name;
		}

		public Block Block {
			get {
				return block;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public override void Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class ParameterReference : Expression {
		Parameters pars;
		string name;
		
		public ParameterReference (Parameters pars, string name)
		{
			this.pars = pars;
			this.name = name;
		}

		public string Name {
			get {
				return name;
			}
		}

		public override void Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}
	
	// <summary>
	//   Used for arguments to New(), Invocation()
	// </summary>
	public class Argument {
		public enum AType {
			Expression,
			Ref,
			Out
		};

		public readonly AType Type;
		Expression expr;

		public Argument (Expression expr, AType type)
		{
			this.expr = expr;
			this.Type = type;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}
	}

	// <summary>
	//   Invocation of methods or delegates.
	// </summary>
	public class Invocation : Expression {
		public readonly ArrayList Arguments;
		Expression expr;

		//
		// arguments is an ArrayList, but we do not want to typecast,
		// as it might be null.
		//
		// FIXME: only allow expr to be a method invocation or a
		// delegate invocation (7.5.5)
		//
		public Invocation (Expression expr, ArrayList arguments)
		{
			this.expr = expr;
			Arguments = arguments;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		public override void Resolve (TypeContainer tc)
		{
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class New : Expression {

		public enum NType {
			Object,
			Array
		};

		public readonly NType     NewType;
		public readonly ArrayList Arguments;
		public readonly string    RequestedType;
		// These are for the case when we have an array
		public readonly string    Rank;
		public readonly ArrayList Indices;
		public readonly ArrayList Initializers;
		

		public New (string requested_type, ArrayList arguments)
		{
			RequestedType = requested_type;
			Arguments = arguments;
			NewType = NType.Object;
		}

		public New (string requested_type, ArrayList exprs, string rank, ArrayList initializers)
		{
			RequestedType = requested_type;
			Indices       = exprs;
			Rank          = rank;
			Initializers  = initializers;
			NewType       = NType.Array;
		}
		
		public override void Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class This : Expression {
		public override void Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class TypeOf : Expression {
		public readonly string QueriedType;
		
		public TypeOf (string queried_type)
		{
			QueriedType = queried_type;
		}

		public override void Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class SizeOf : Expression {
		public readonly string QueriedType;
		
		public SizeOf (string queried_type)
		{
			this.QueriedType = queried_type;
		}

		public override void Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class MemberAccess : Expression {
		public readonly string Identifier;
		Expression expr;
		
		public MemberAccess (Expression expr, string id)
		{
			this.expr = expr;
			Identifier = id;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}
		
		public override void Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
		}

		public override void Emit (EmitContext ec)
		{
		}

	}

	// <summary>
	//   Nodes of type Namespace are created during the semantic
	//   analysis to resolve member_access/qualified_identifier/simple_name
	//   accesses.
	//
	//   They are born `resolved'. 
	// </summary>
	public class NamespaceExpr : Expression {
		public readonly string Name;
		
		public NamespaceExpr (string name)
		{
			Name = name;
			eclass = ExprClass.Namespace;
		}

		public override void Resolve (TypeContainer tc)
		{
		}

		public override void Emit (EmitContext ec)
		{
		}
	}
	
	public class BuiltinTypeAccess : Expression {
		public readonly string AccessBase;
		public readonly string Method;
		
		public BuiltinTypeAccess (string type, string method)
		{
			System.Console.WriteLine ("DUDE! This type should be fully resolved!");
			AccessBase = type;
			Method = method;
		}

		public override void Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}
}





