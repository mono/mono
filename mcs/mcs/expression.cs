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
	
	public abstract class Expression {
		Type type;
		bool is_lvalue;

		public Type Type {
			get {
				return type;
			}
		}

		public bool IsLValue {
			get {
				return is_lvalue;
			}

			set {
				is_lvalue = value;
			}
		}

		public virtual bool IsLiteral {
			get {
				return false;
			}
		}
		
		// <summary>
		//   Protected constructor.  Only derivate types should
		//   be able to be created
		// </summary>
		// protected Type (

		protected Expression ()
		{
			type = null;
		}
		
		static bool EvaluateType (Expression expr, out Type setType)
		{
			setType = null;
			return true;
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
	}

	public class Probe : Expression {
		TypeRef typeref;
		Expression expr;
		Operator oper;

		public enum Operator {
			Is, As
		}
		
		public Probe (Operator oper, Expression expr, TypeRef typeref)
		{
			this.oper = oper;
			this.typeref = typeref;
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

		public Type ProbeType {
			get {
				return typeref.Type;
			}
		}
			       
	}
	
	public class Cast : Expression {
		TypeRef typeref;
		Expression expr;
		
		public Cast (TypeRef typeref, Expression expr)
		{
			this.typeref = typeref;
			this.expr = expr;
		}

		public Type TargetType {
			get {
				return typeref.Type;
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

		AType type;
		Expression expr;

		public Argument (Expression expr, AType type)
		{
			this.expr = expr;
			this.type = type;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		public AType Type {
			get {
				return type;
			}
		}
	}

	// <summary>
	//   Invocation of methods or delegates.
	// </summary>
	public class Invocation : Expression {
		ArrayList arguments;
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
			this.arguments = arguments;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		public ArrayList Arguments {
			get {
				return arguments;
			}
		}
	}

	public class New : Expression {
		ArrayList arguments;
		TypeRef requested_ref;

		public New (TypeRef requested_ref, ArrayList arguments)
		{
			this.requested_ref = requested_ref;
			this.arguments = arguments;
		}
		
		public ArrayList Arguments{
			get {
				return arguments;
			}
		}
		
		public Type RequestedType {
			get {
				return requested_ref.Type;
			}
		}
	}

	public class This : Expression {
	}

	public class TypeOf : Expression {
		TypeRef queried_ref;
		
		public TypeOf (TypeRef queried_ref)
		{
			this.queried_ref = queried_ref;
		}

		public Type QueriedType {
			get {
				return queried_ref.Type;
			}
		}
	}

	public class SizeOf : Expression {
		TypeRef queried_ref;
		
		public SizeOf (TypeRef queried_ref)
		{
			this.queried_ref = queried_ref;
		}

		public Type QueriedType {
			get {
				return queried_ref.Type;
			}
		}
	}

	public class MemberAccess : Expression {
		Expression expr;
		string id;
		
		public MemberAccess (Expression expr, string id)
		{
			this.expr = expr;
			this.id = id;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		public string Identifier {
			get {
				return id;
			}
		}
	}

	public class BuiltinTypeAccess : Expression {
		TypeRef access_base;
		string method;
		
		public BuiltinTypeAccess (TypeRef typeref, string method)
		{
			System.Console.WriteLine ("DUDE!  This TypeRef should be fully resolved!");
			this.access_base = access_base;
			this.method = method;
		}

		public Type AccessBase {
			get {
				return access_base.Type;
			}
		}

		public string Method {
			get {
				return method;
			}
		}
	}

	public class MethodGroup : Expression {
		Hashtable signatures;
		string name;
		
		public MethodGroup (string name)
		{
			signatures = new Hashtable ();
			this.name = name;
		}

		public bool Add (Method method)
		{
			string sig = method.ArgumentSignature;

			if (signatures.Contains (sig))
				return false;

			signatures.Add (sig, method);
			return true;
		}

		public string Name {
			get {
				return name;
			}
		}

		public Hashtable Methods {
			get {
				return signatures;
			}
		}
	}
}





