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
	using System.Reflection;
	using System.Reflection.Emit;

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

		public abstract Expression Resolve (TypeContainer tc);
		public abstract void Emit (EmitContext ec);
		
		// <summary>
		//   Protected constructor.  Only derivate types should
		//   be able to be created
		// </summary>

		protected Expression ()
		{
			eclass = ExprClass.Invalid;
			type = null;
		}

		// 
		// Returns a fully formed expression after a MemberLookup
		//
		static Expression ExprClassFromMemberInfo (MemberInfo mi)
		{
			if (mi is EventInfo){
				// FIXME: Implement
				return null;
			} else if (mi is FieldInfo){
				return new FieldExpr ((FieldInfo) mi);
			} else if (mi is PropertyInfo){
				return new PropertyExpr ((PropertyInfo) mi);
			} else if (mi is Type)
				return new TypeExpr ((Type) mi);

			return null;
		}
		
		//
		// FIXME: Probably implement a cache for (t,name,current_access_set)?
		//
		// FIXME: We need to cope with access permissions here, or this wont
		// work!
		//
		// This code could use some optimizations, but we need to do some
		// measurements.  For example, we could use a delegate to `flag' when
		// something can not any longer be a method-group (because it is something
		// else).
		//
		// Return values:
		//     If the return value is an Array, then it is an array of
		//     MethodInfos
		//   
		//     If the return value is an MemberInfo, it is anything, but a Method
		//
		//     null on error.
		//
		// FIXME: When calling MemberLookup inside an `Invocation', we should pass
		// the arguments here and have MemberLookup return only the methods that
		// match the argument count/type, unlike we are doing now (we delay this
		// decision).
		//
		// This is so we can catch correctly attempts to invoke instance methods
		// from a static body (scan for error 120 in ResolveSimpleName).
		// 
		protected static Expression MemberLookup (Report r, Type t, string name, bool same_type)
		{
			MemberTypes mt =
				// MemberTypes.Constructor |
				MemberTypes.Event       |
				MemberTypes.Field       |
				MemberTypes.Method      |
				MemberTypes.NestedType  |
				MemberTypes.Property;
			
			BindingFlags bf =
				BindingFlags.Public |
				BindingFlags.Static |
				BindingFlags.Instance;
			
			if (same_type)
				bf |= BindingFlags.NonPublic;

			
			MemberInfo [] mi = t.FindMembers (mt, bf, Type.FilterName, name);

			if (mi.Length == 1 && !(mi [0] is MethodInfo))
				return Expression.ExprClassFromMemberInfo (mi [0]);

			for (int i = 0; i < mi.Length; i++)
				if (!(mi [i] is MethodInfo)){
					r.Error (-5, "Do not know how to reproduce this case: Methods and non-Method with the same name, report this please");
					
				}

			return new MethodGroupExpr (mi);
		}
		
		// <summary>
		//   Resolves the E in `E.I' side for a member_access
		//
		// This is suboptimal and should be merged with ResolveMemberAccess
		static Expression ResolvePrimary (TypeContainer tc, string name)
		{
			int dot_pos = name.LastIndexOf (".");

			if (tc.RootContext.IsNamespace (name))
				return new NamespaceExpr (name);

			if (dot_pos != -1){
			} else {
				Type t = tc.LookupType (name, false);

				if (t != null)
					return new TypeExpr (t);
			}

			return null;
		}
			
		static public Expression ResolveMemberAccess (TypeContainer tc, string name)
		{
			Expression left_e;
			int dot_pos = name.LastIndexOf (".");
			string left = name.Substring (0, dot_pos);
			string right = name.Substring (dot_pos + 1);

			left_e = ResolvePrimary (tc, left);
			if (left_e == null)
				return null;

			switch (left_e.ExprClass){
			case ExprClass.Type:
				return  MemberLookup (tc.RootContext.Report,
						      left_e.Type, right,
						      left_e.Type == tc.TypeBuilder);
				
			case ExprClass.Namespace:
			case ExprClass.PropertyAccess:
			case ExprClass.IndexerAccess:
			case ExprClass.Variable:
			case ExprClass.Value:
			case ExprClass.Nothing:
			case ExprClass.EventAccess:
			case ExprClass.MethodGroup:
			case ExprClass.Invalid:
				tc.RootContext.Report.Error (-1000,
							     "Internal compiler error, should have " +
							     "got these handled before");
				break;
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

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
			return this;
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

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
			return this;
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
		
		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
			return this;
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

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: implement me
			return this;
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

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
			return this;
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
		// Checks whether we are trying to access an instance
		// property, method or field from a static body.
		//
		Expression MemberStaticCheck (Report r, Expression e)
		{
			if (e is FieldExpr){
				FieldInfo fi = ((FieldExpr) e).FieldInfo;
				
				if (!fi.IsStatic){
					r.Error (120,
						 "An object reference is required " +
						 "for the non-static field `"+name+"'");
					return null;
				}
			} else if (e is MethodGroupExpr){
				// FIXME: Pending reorganization of MemberLookup
				// Basically at this point we should have the
				// best match already selected for us, and
				// we should only have to check a *single*
				// Method for its static on/off bit.
				return e;
			} else if (e is PropertyExpr){
				if (!((PropertyExpr) e).IsStatic){
					r.Error (120,
						 "An object reference is required " +
						 "for the non-static property access `"+
						 name+"'");
					return null;
				}
			}

			return e;
		}
		
		//
		// 7.5.2: Simple Names. 
		//
		// Local Variables and Parameters are handled at
		// parse time, so they never occur as SimpleNames.
		//
		Expression ResolveSimpleName (TypeContainer tc)
		{
			Expression e;
			Report r = tc.RootContext.Report;

			e = MemberLookup (tc.RootContext.Report, tc.TypeBuilder, name, true);
			if (e != null){
				if (e is TypeExpr)
					return e;
				if ((tc.ModFlags & Modifiers.STATIC) != 0)
					return MemberStaticCheck (r, e);
				else
					return e;
			}

			//
			// Do step 3 of the Simple Name resolution.
			//
			// FIXME: implement me.
			
			return this;
		}
		
		//
		// SimpleName needs to handle a multitude of cases:
		//
		// simple_names and qualified_identifiers are placed on
		// the tree equally.
		//
		public override Expression Resolve (TypeContainer tc)
		{
			if (name.IndexOf (".") != -1)
				return ResolveMemberAccess (tc, name);
			else
				return ResolveSimpleName (tc);
		}

		public override void Emit (EmitContext ec)
		{
		}
	}
	
	public class LocalVariableReference : Expression {
		public readonly string Name;
		public readonly Block Block;
		
		public LocalVariableReference (Block block, string name)
		{
			Block = block;
			Name = name;
			eclass = ExprClass.Variable;
		}

		public VariableInfo VariableInfo {
			get {
				return (VariableInfo) Block.GetVariableInfo (Name);
			}
		}
		
		public override Expression Resolve (TypeContainer tc)
		{
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Console.WriteLine ("Internal compiler error, LocalVariableReference should not be emitted");
		}
	}

	public class ParameterReference : Expression {
		public readonly Parameters Pars;
		public readonly String Name;
		public readonly int Idx;
		
		public ParameterReference (Parameters pars, int idx, string name)
		{
			Pars = pars;
			Idx  = idx;
			Name = name;
		}

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
			return this;
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

		public bool Resolve (TypeContainer tc)
		{
			expr = expr.Resolve (tc);
			return expr != null;
		}

		public void Emit (EmitContext ec)
		{
			expr.Emit (ec);
		}
	}

	// <summary>
	//   Invocation of methods or delegates.
	// </summary>
	public class Invocation : Expression {
		public readonly ArrayList Arguments;
		Expression expr;
		MethodInfo method = null;
		
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

		/// <summary>
		///   Computes whether Argument `a' and the ParameterInfo `pi' are
		///   compatible, and if so, how good is the match (in terms of
		///   "better conversions" (7.4.2.3).
		///
		///   0   is the best possible match.
		///   -1  represents a type mismatch.
		///   -2  represents a ref/out mismatch.
		/// </summary>
		static int Badness (Argument a, ParameterInfo pi)
		{
			if (pi.ParameterType == a.Expr.Type)
				return 0;

			// FIXME: Implement implicit conversions here.
			// FIXME: Implement better conversion here.
			
			return -1;
		}
		
		public override Expression Resolve (TypeContainer tc)
		{
			//
			// First, resolve the expression that is used to
			// trigger the invocation
			//
			this.expr = expr.Resolve (tc);
			if (this.expr == null)
				return null;

			if (!(this.expr is MethodGroupExpr)){
				tc.RootContext.Report.Error (118,
				       "Denotes an " + this.expr.ExprClass + " while a method was expected");
				return null;
			}

			//
			// Next, evaluate all the expressions in the argument list
			//
			if (Arguments != null){
				for (int i = Arguments.Count; i > 0;){
					--i;
					Argument a = (Argument) Arguments [i];

					if (!a.Resolve (tc))
						return null;
				}
			}

			//
			// Find the Applicable Function Members (7.4.2.1)
			//
			MethodGroupExpr me = (MethodGroupExpr) this.expr;
			ArrayList afm = new ArrayList ();
			int best_match = 10000;
			int best_match_idx = -1;
			
			for (int i = me.Methods.Length; i > 0; ){
				i--;
				ParameterInfo [] pi = me.Methods [i].GetParameters ();

				//
				// Compute how good this is
				//
				if (pi.Length == Arguments.Count){
					int badness = 0;
					
					for (int j = Arguments.Count; j > 0;){
						int x;
						j--;

						Argument a = (Argument) Arguments [j];

						x = Badness (a, pi [j]);

						if (x < 0){
							// FIXME: report nice error.
						} else
							badness += x;
					}

					if (badness < best_match){
						best_match = badness;
						method = me.Methods [i];
						best_match_idx = i;
					}
				}
			}

			if (method == null){
				tc.RootContext.Report.Error (-6,
				"Figure out error: Can not find a good function for this argument list");
				return null;
			}

			Console.WriteLine ("Found a method! " + method);

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			int top = Arguments.Count;

			for (int i = 0; i < top; i++){
				Argument a = (Argument) Arguments [i];

				a.Emit (ec);
			}

			ec.ig.Emit (OpCodes.Call, (MethodInfo) method);
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
		
		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class This : Expression {
		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
			return this;
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

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
			return this;
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

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
			return this;
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
		
		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
			return this;
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

		public override Expression Resolve (TypeContainer tc)
		{
			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	// <summary>
	//   Fully resolved expression that evaluates to a type
	// </summary>
	public class TypeExpr : Expression {
		public TypeExpr (Type t)
		{
			Type = t;
			eclass = ExprClass.Type;
		}

		override public Expression Resolve (TypeContainer tc)
		{
			return this;
		}

		override public void Emit (EmitContext ec)
		{
			
		}
	}

	// <summary>
	//   Fully resolved expression that evaluates to a type
	// </summary>
	public class MethodGroupExpr : Expression {
		public readonly MethodInfo [] Methods;
		
		public MethodGroupExpr (MemberInfo [] mi)
		{
			Methods = new MethodInfo [mi.Length];
			mi.CopyTo (Methods, 0);
			eclass = ExprClass.MethodGroup;
		}

		override public Expression Resolve (TypeContainer tc)
		{
			return this;
		}

		override public void Emit (EmitContext ec)
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

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: Implement;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}


	//   Fully resolved expression that evaluates to a Field
	// </summary>
	public class FieldExpr : Expression {
		public readonly FieldInfo FieldInfo;

		public FieldExpr (FieldInfo fi)
		{
			FieldInfo = fi;
			eclass = ExprClass.Variable;
		}

		override public Expression Resolve (TypeContainer tc)
		{
			// We are born in resolved state. 
			return this;
		}

		override public void Emit (EmitContext ec)
		{
			// FIXME: Assert that this should not be reached?
		}
	}
	
	// <summary>
	//   Fully resolved expression that evaluates to a Property
	// </summary>
	public class PropertyExpr : Expression {
		public readonly PropertyInfo PropertyInfo;
		public readonly bool IsStatic;
		
		public PropertyExpr (PropertyInfo pi)
		{
			PropertyInfo = pi;
			eclass = ExprClass.PropertyAccess;
			IsStatic = false;
				
			MethodInfo [] acc = pi.GetAccessors ();

			for (int i = 0; i < acc.Length; i++)
				if (acc [i].IsStatic)
					IsStatic = true;
		}

		override public Expression Resolve (TypeContainer tc)
		{
			// We are born in resolved state. 
			return this;
		}

		override public void Emit (EmitContext ec)
		{
			// FIXME: Implement.
		}
	}

	public class CheckedExpr : Expression {

		public readonly Expression Expr;

		public CheckedExpr (Expression e)
		{
			Expr = e;
		}

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME : Implement !
			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
		
	}

	public class UnCheckedExpr : Expression {

		public readonly Expression Expr;

		public UnCheckedExpr (Expression e)
		{
			Expr = e;
		}

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME : Implement !
			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
		
	}

		public class ElementAccess : Expression {

		public readonly ArrayList  Arguments;
		public readonly Expression Expr;

		public ElementAccess (Expression e, ArrayList e_list)
		{
			Expr = e;
			Arguments = e_list;
		}

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME : Implement
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			// FIXME : Implement !
		}

	}

	public class BaseAccess : Expression {

		public enum BaseAccessType {
			Member,
			Indexer
		};
		
		public readonly BaseAccessType BAType;
		public readonly string         Member;
		public readonly ArrayList      Arguments;

		public BaseAccess (BaseAccessType t, string member, ArrayList args)
		{
			BAType = t;
			Member = member;
			Arguments = args;
			
		}

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME : Implement !
			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}
}
