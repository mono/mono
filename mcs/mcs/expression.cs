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
	using System.Text;
	
	// <remarks>
	//   The ExprClass class contains the is used to pass the 
	//   classification of an expression (value, variable, namespace,
	//   type, method group, property access, event access, indexer access,
	//   nothing).
	// </remarks>
	public enum ExprClass {
		Invalid,
		
		Value,
		Variable,   // Every Variable should implement LValue
		Namespace,
		Type,
		MethodGroup,
		PropertyAccess,
		EventAccess,
		IndexerAccess,
		Nothing, 
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

		//
		// Return value indicates whether a value is left on the stack or not
		//
		public abstract bool Emit (EmitContext ec);
		
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
				return new EventExpr ((EventInfo) mi);
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
		//     MethodBases
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
		public static Expression MemberLookup (RootContext rc, Type t, string name,
							  bool same_type, MemberTypes mt, BindingFlags bf)
		{
			if (same_type)
				bf |= BindingFlags.NonPublic;

			MemberInfo [] mi = rc.TypeManager.FindMembers (t, mt, bf, Type.FilterName, name);

			if (mi == null)
				return null;
			
			if (mi.Length == 1 && !(mi [0] is MethodBase))
				return Expression.ExprClassFromMemberInfo (mi [0]);
			
			for (int i = 0; i < mi.Length; i++)
				if (!(mi [i] is MethodBase)){
					rc.Report.Error (-5, "Do not know how to reproduce this case: " + 
							 "Methods and non-Method with the same name, report this please");

					for (i = 0; i < mi.Length; i++){
						Type tt = mi [i].GetType ();

						Console.WriteLine (i + ": " + mi [i]);
						while (tt != TypeManager.object_type){
							Console.WriteLine (tt);
							tt = tt.BaseType;
						}
					}
				}

			return new MethodGroupExpr (mi);
		}

		public const MemberTypes AllMemberTypes =
			MemberTypes.Constructor |
			MemberTypes.Event       |
			MemberTypes.Field       |
			MemberTypes.Method      |
			MemberTypes.NestedType  |
			MemberTypes.Property;
		
		public const BindingFlags AllBindingsFlags =
			BindingFlags.Public |
			BindingFlags.Static |
			BindingFlags.Instance;

		public static Expression MemberLookup (RootContext rc, Type t, string name,
						       bool same_type)
		{
			return MemberLookup (rc, t, name, same_type, AllMemberTypes, AllBindingsFlags);
		}
		
		// <summary>
		//   Resolves the E in `E.I' side for a member_access
		//
		//   This is suboptimal and should be merged with ResolveMemberAccess
		// </summary>
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
				return  MemberLookup (tc.RootContext,
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

		static public Expression ImplicitReferenceConversion (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;
			
			if (target_type == TypeManager.object_type) {
				if (expr_type.IsClass)
					return new EmptyCast (expr, target_type);
				if (expr_type.IsValueType)
					return new BoxedCast (expr, target_type);
			} else if (expr_type.IsSubclassOf (target_type))
				return new EmptyCast (expr, target_type);
			else 
				// FIXME: missing implicit reference conversions:
				// 
				// from any class-type S to any interface-type T.
				// from any interface type S to interface-type T.
				// from an array-type S to an array-type of type T
				// from an array-type to System.Array
				// from any delegate type to System.Delegate
				// from any array-type or delegate type into System.ICloneable.
				// from the null type to any reference-type.
				     
				return null;

			return null;
		}
		       
		// <summary>
		//   Converts implicitly the resolved expression `expr' into the
		//   `target_type'.  It returns a new expression that can be used
		//   in a context that expects a `target_type'. 
		// </summary>
		static public Expression ConvertImplicit (Expression expr, Type target_type, TypeContainer tc)
		{
			Type expr_type = expr.Type;

			if (expr_type == target_type)
				return expr;
			
			//
			// Step 1: Perform implicit conversions as found on expr.Type
			//
			Expression imp;

			imp = new UserImplicitCast (expr, target_type);

			imp = imp.Resolve (tc);

			if (imp != null)
				return imp;

			//
			// Step 2: Built-in conversions.
			//
			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double.
				//
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type))
					return new EmptyCast (expr, target_type);

				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, float, double
				// 
				if (target_type == TypeManager.int32_type)
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double
				//
				if ((target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type))
					return new EmptyCast (expr, target_type);
					
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, float, double
				//
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double
				//
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R4);
			} else if ((expr_type == TypeManager.uint64_type) ||
				   (expr_type == TypeManager.int64_type)){
				//
				// From long to float, double
				//
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R4);	
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type))
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
			} else
				return ImplicitReferenceConversion (expr, target_type);

			

			//
			//  Could not find an implicit cast.
			//
			return null;
		}

		// <summary>
		//   Attemptes to implicityly convert `target' into `type', using
		//   ConvertImplicit.  If there is no implicit conversion, then
		//   an error is signaled
		// </summary>
		static public Expression ConvertImplicitRequired (TypeContainer tc, Expression target,
								  Type type, Location l)
		{
			Expression e;
			
			e = ConvertImplicit (target, type, tc);
			if (e == null){
				string msg = "Can not convert implicitly from `"+
					TypeManager.CSharpName (target.Type) + "' to `" +
					TypeManager.CSharpName (type) + "'";

				tc.RootContext.Report.Error (29, l, msg);
			}
			return e;
		}
		
		// <summary>
		//   Performs an explicit conversion of the expression `expr' whose
		//   type is expr.Type to `target_type'.
		// </summary>
		static public Expression ConvertExplicit (Expression expr, Type target_type)
		{
			return expr;
		}

		void report (TypeContainer tc, int error, string s)
		{
			tc.RootContext.Report.Error (error, s);
		}

		static string ExprClassName (ExprClass c)
		{
			switch (c){
			case ExprClass.Invalid:
				return "Invalid";
			case ExprClass.Value:
				return "value";
			case ExprClass.Variable:
				return "variable";
			case ExprClass.Namespace:
				return "namespace";
			case ExprClass.Type:
				return "type";
			case ExprClass.MethodGroup:
				return "method group";
			case ExprClass.PropertyAccess:
				return "property access";
			case ExprClass.EventAccess:
				return "event access";
			case ExprClass.IndexerAccess:
				return "indexer access";
			case ExprClass.Nothing:
				return "null";
			}
			throw new Exception ("Should not happen");
		}
		
		// <summary>
		//   Reports that we were expecting `expr' to be of class `expected'
		// </summary>
		protected void report118 (TypeContainer tc, Expression expr, string expected)
		{
			report (tc, 118, "Expression denotes a '" + ExprClassName (expr.ExprClass) +
				"' where an " + expected + " was expected");
		}
	}

	// <summary>
	//   This kind of cast is used to encapsulate the child
	//   whose type is child.Type into an expression that is
	//   reported to return "return_type".  This is used to encapsulate
	//   expressions which have compatible types, but need to be dealt
	//   at higher levels with.
	//
	//   For example, a "byte" expression could be encapsulated in one
	//   of these as an "unsigned int".  The type for the expression
	//   would be "unsigned int".
	//
	// </summary>
	
	public class EmptyCast : Expression {
		protected Expression child;

		public EmptyCast (Expression child, Type return_type)
		{
			ExprClass = child.ExprClass;
			type = return_type;
			this.child = child;
		}

		public override Expression Resolve (TypeContainer tc)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override bool Emit (EmitContext ec)
		{
			return child.Emit (ec);
		}			
	}

	// <summary>
	//   This kind of cast is used to encapsulate Value Types in objects.
	//
	//   The effect of it is to box the value type emitted by the previous
	//   operation.
	// </summary>
	public class BoxedCast : EmptyCast {

		public BoxedCast (Expression expr, Type target_type)
			: base (expr, target_type)
		{
		}

		public override Expression Resolve (TypeContainer tc)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override bool Emit (EmitContext ec)
		{
			base.Emit (ec);
			ec.ig.Emit (OpCodes.Box, child.Type);
			return true;
		}
	}
	

	// <summary>
	//   This kind of cast is used to encapsulate a child expression
	//   that can be trivially converted to a target type using one or 
	//   two opcodes.  The opcodes are passed as arguments.
	// </summary>
	public class OpcodeCast : EmptyCast {
		OpCode op, op2;
		bool second_valid;
		
		public OpcodeCast (Expression child, Type return_type, OpCode op)
			: base (child, return_type)
			
		{
			this.op = op;
			second_valid = false;
		}

		public OpcodeCast (Expression child, Type return_type, OpCode op, OpCode op2)
			: base (child, return_type)
			
		{
			this.op = op;
			this.op2 = op2;
			second_valid = true;
		}

		public override Expression Resolve (TypeContainer tc)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override bool Emit (EmitContext ec)
		{
			base.Emit (ec);
			ec.ig.Emit (op);

			if (second_valid)
				ec.ig.Emit (op2);

			return true;
		}			
		
	}
	
	public class Unary : Expression {
		public enum Operator {
			Add, Subtract, Negate, BitComplement,
			Indirection, AddressOf, PreIncrement,
			PreDecrement, PostIncrement, PostDecrement
		}

		Operator   oper;
		Expression expr;
		ArrayList  Arguments;
		MethodBase method;
		Location   location;
		
		public Unary (Operator op, Expression expr, Location loc)
		{
			this.oper = op;
			this.expr = expr;
			this.location = loc;
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

		// <summary>
		//   Returns a stringified representation of the Operator
		// </summary>
		string OperName ()
		{
			switch (oper){
			case Operator.Add:
				return "+";
			case Operator.Subtract:
				return "-";
			case Operator.Negate:
				return "!";
			case Operator.BitComplement:
				return "~";
			case Operator.AddressOf:
				return "&";
			case Operator.Indirection:
				return "*";
			case Operator.PreIncrement : case Operator.PostIncrement :
				return "++";
			case Operator.PreDecrement : case Operator.PostDecrement :
				return "--";
			}

			return oper.ToString ();
		}

		Expression ForceConversion (Expression expr, Type target_type, TypeContainer tc)
		{
			if (expr.Type == target_type)
				return expr;

			return ConvertImplicit (expr, target_type, tc);
		}

		void report23 (Report r, Type t)
		{
			r.Error (23, "Operator " + OperName () + " cannot be applied to operand of type `" +
				 TypeManager.CSharpName (t) + "'");
		}

		// <summary>
		//   Returns whether an object of type `t' can be incremented
		//   or decremented with add/sub (ie, basically whether we can
		//   use pre-post incr-decr operations on it, but it is not a
		//   System.Decimal, which we test elsewhere)
		// </summary>
		static bool IsIncrementableNumber (Type t)
		{
			return (t == TypeManager.sbyte_type) ||
				(t == TypeManager.byte_type) ||
				(t == TypeManager.short_type) ||
				(t == TypeManager.ushort_type) ||
				(t == TypeManager.int32_type) ||
				(t == TypeManager.uint32_type) ||
				(t == TypeManager.int64_type) ||
				(t == TypeManager.uint64_type) ||
				(t == TypeManager.char_type) ||
				(t.IsSubclassOf (TypeManager.enum_type)) ||
				(t == TypeManager.float_type) ||
				(t == TypeManager.double_type);
		}
			
		Expression ResolveOperator (TypeContainer tc)
		{
			Type expr_type = expr.Type;

			//
			// Step 1: Perform Operator Overload location
			//
			Expression mg;
			string op_name;
				
			if (oper == Operator.PostIncrement || oper == Operator.PreIncrement)
				op_name = "op_Increment";
			else if (oper == Operator.PostDecrement || oper == Operator.PreDecrement)
				op_name = "op_Decrement";
			else
				op_name = "op_" + oper;

			mg = MemberLookup (tc.RootContext, expr_type, op_name, false);
			
			if (mg != null) {
				Arguments = new ArrayList ();
				Arguments.Add (new Argument (expr, Argument.AType.Expression));
				
				method = Invocation.OverloadResolve ((MethodGroupExpr) mg, Arguments, tc, location);
				if (method != null) {
					Method m = (Method) TypeContainer.LookupMethodByBuilder (method);
					type = m.GetReturnType (tc);
					return this;
				}
			}

			//
			// Step 2: Default operations on CLI native types.
			//

			// Only perform numeric promotions on:
			// +, -, ++, --

			if (expr_type == null)
				return null;
			
			if (oper == Operator.Negate){
				if (expr_type != TypeManager.bool_type) {
					report23 (tc.RootContext.Report, expr.Type);
					return null;
				} else
					type = TypeManager.bool_type;
			}

			if (oper == Operator.BitComplement) {
				if (!((expr_type == TypeManager.int32_type) ||
				      (expr_type == TypeManager.uint32_type) ||
				      (expr_type == TypeManager.int64_type) ||
				      (expr_type == TypeManager.uint64_type) ||
				      (expr_type.IsSubclassOf (TypeManager.enum_type)))){
					report23 (tc.RootContext.Report, expr.Type);
					return null;
				}
				type = expr_type;
				return this;
			}

			if (oper == Operator.Add) {
				//
				// A plus in front of something is just a no-op, so return the child.
				//
				return expr;
			}

			//
			// Deals with -literals
			// int     operator- (int x)
			// long    operator- (long x)
			// float   operator- (float f)
			// double  operator- (double d)
			// decimal operator- (decimal d)
			//
			if (oper == Operator.Subtract){
				//
				// Fold a "- Constant" into a negative constant
				//
			
				Expression e = null;

				//
				// Is this a constant? 
				//
				if (expr is IntLiteral)
					e = new IntLiteral (-((IntLiteral) expr).Value);
				else if (expr is LongLiteral)
					e = new LongLiteral (-((LongLiteral) expr).Value);
				else if (expr is FloatLiteral)
					e = new FloatLiteral (-((FloatLiteral) expr).Value);
				else if (expr is DoubleLiteral)
					e = new DoubleLiteral (-((DoubleLiteral) expr).Value);
				else if (expr is DecimalLiteral)
					e = new DecimalLiteral (-((DecimalLiteral) expr).Value);
				
				if (e != null){
					e = e.Resolve (tc);
					return e;
				}

				//
				// Not a constant we can optimize, perform numeric 
				// promotions to int, long, double.
				//
				//
				// The following is inneficient, because we call
				// ConvertImplicit too many times.
				//
				// It is also not clear if we should convert to Float
				// or Double initially.
				//
				if (expr_type == TypeManager.uint32_type){
					//
					// FIXME: handle exception to this rule that
					// permits the int value -2147483648 (-2^31) to
					// bt written as a decimal interger literal
					//
					type = TypeManager.int64_type;
					expr = ConvertImplicit (expr, type, tc);
					return this;
				}

				if (expr_type == TypeManager.uint64_type){
					//
					// FIXME: Handle exception of `long value'
					// -92233720368547758087 (-2^63) to be written as
					// decimal integer literal.
					//
					report23 (tc.RootContext.Report, expr_type);
					return null;
				}

				e = ConvertImplicit (expr, TypeManager.int32_type, tc);
				if (e != null){
					expr = e;
					type = e.Type;
					return this;
				} 

				e = ConvertImplicit (expr, TypeManager.int64_type, tc);
				if (e != null){
					expr = e;
					type = e.Type;
					return this;
				}

				e = ConvertImplicit (expr, TypeManager.double_type, tc);
				if (e != null){
					expr = e;
					type = e.Type;
					return this;
				}

				report23 (tc.RootContext.Report, expr_type);
				return null;
			}

			//
			// The operand of the prefix/postfix increment decrement operators
			// should be an expression that is classified as a variable,
			// a property access or an indexer access
			//
			if (oper == Operator.PreDecrement || oper == Operator.PreIncrement ||
			    oper == Operator.PostDecrement || oper == Operator.PostIncrement){
				if (expr.ExprClass == ExprClass.Variable){
					if (IsIncrementableNumber (expr_type) ||
					    expr_type == TypeManager.decimal_type){
						type = expr_type;
						return this;
					}
				} else if (expr.ExprClass == ExprClass.IndexerAccess){
					//
					// FIXME: Verify that we have both get and set methods
					//
					throw new Exception ("Implement me");
				} else if (expr.ExprClass == ExprClass.PropertyAccess){
					//
					// FIXME: Verify that we have both get and set methods
					//
					throw new Exception ("Implement me");
				} else {
					report118 (tc, expr, "variable, indexer or property access");
				}
			}

			tc.RootContext.Report.Error (187, "No such operator '" + OperName () +
						     "' defined for type '" +
						     TypeManager.CSharpName (expr_type) + "'");
			return null;

		}

		public override Expression Resolve (TypeContainer tc)
		{
			expr = expr.Resolve (tc);

			if (expr == null)
				return null;
			
			return ResolveOperator (tc);
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Type expr_type = expr.Type;

			if (method != null) {

				// Note that operators are static anyway
				
				if (Arguments != null) {
					Invocation.EmitArguments (ec, method, Arguments);
				}
				
				if (method is MethodInfo)
					ig.Emit (OpCodes.Call, (MethodInfo) method);
				else
					ig.Emit (OpCodes.Call, (ConstructorInfo) method);
				
				return true;
			}
			
			switch (oper){
			case Operator.Add:
				throw new Exception ("This should be caught by Resolve");

			case Operator.Subtract:
			        expr.Emit (ec);
				ig.Emit (OpCodes.Neg);
				break;
				
			case Operator.Negate:
				expr.Emit (ec);
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Ceq);
				break;

			case Operator.BitComplement:
				expr.Emit (ec);
				ig.Emit (OpCodes.Not);
				break;

			case Operator.AddressOf:
				throw new Exception ("Not implemented yet");

			case Operator.Indirection:
				throw new Exception ("Not implemented yet");

			case Operator.PreIncrement:
			case Operator.PreDecrement:
				if (expr.ExprClass == ExprClass.Variable){
					if (expr_type == TypeManager.decimal_type){
						throw new Exception ("FIXME: Add pre inc/dec for decimals");
					} else {
						//
						// Resolve already verified that it is an "incrementable"
						// 
						expr.Emit (ec);
						ig.Emit (OpCodes.Ldc_I4_1);

						if (oper == Operator.PreDecrement)
							ig.Emit (OpCodes.Sub);
						else
							ig.Emit (OpCodes.Add);
						((LValue) expr).Store (ig);
						ig.Emit (OpCodes.Dup);
					} 
				} else {
					throw new Exception ("Handle Indexers and Properties here");
				}
				break;
				
			case Operator.PostIncrement:
			case Operator.PostDecrement:
				if (expr.ExprClass == ExprClass.Variable){
					if (expr_type == TypeManager.decimal_type){
						throw new Exception ("FIXME: Add pre inc/dec for decimals");
					} else {
						//
						// Resolve already verified that it is an "incrementable"
						// 
						expr.Emit (ec);
						ig.Emit (OpCodes.Dup);
						ig.Emit (OpCodes.Ldc_I4_1);

						if (oper == Operator.PostDecrement)
							ig.Emit (OpCodes.Sub);
						else
							ig.Emit (OpCodes.Add);
						((LValue) expr).Store (ig);
					} 
				} else {
					throw new Exception ("Handle Indexers and Properties here");
				}
				break;
				
			default:
				throw new Exception ("This should not happen: Operator = "
						     + oper.ToString ());
			}

			//
			// yes, we leave a value on the stack
			//
			return true;
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
			throw new Exception ("Unimplemented");
			// return this;
		}

		public override bool Emit (EmitContext ec)
		{
			return true;
		}
	}
	
	public class Cast : Expression {
		string target_type;
		Expression expr;
		
		public Cast (string cast_type, Expression expr)
		{
			this.target_type = cast_type;
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
			type = tc.LookupType (target_type, false);
			eclass = ExprClass.Value;
			
			if (type == null)
				return null;

			//
			// FIXME: Unimplemented
			//
			throw new Exception ("FINISH ME");
		}

		public override bool Emit (EmitContext ec)
		{
			return true;
		}
	}

	public class Binary : Expression {
		public enum Operator {
			Multiply, Divide, Modulo,
			Add, Subtract,
			ShiftLeft, ShiftRight,
			LessThan, GreaterThan, LessOrEqual, GreaterOrEqual, 
			Equal, NotEqual,
			BitwiseAnd,
			ExclusiveOr,
			BitwiseOr,
			LogicalAnd,
			LogicalOr
		}

		Operator oper;
		Expression left, right;
		MethodBase method;
		ArrayList  Arguments;
		Location   location;
		

		public Binary (Operator oper, Expression left, Expression right, Location loc)
		{
			this.oper = oper;
			this.left = left;
			this.right = right;
			this.location = loc;
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


		// <summary>
		//   Returns a stringified representation of the Operator
		// </summary>
		string OperName ()
		{
			switch (oper){
			case Operator.Multiply:
				return "*";
			case Operator.Divide:
				return "/";
			case Operator.Modulo:
				return "%";
			case Operator.Add:
				return "+";
			case Operator.Subtract:
				return "-";
			case Operator.ShiftLeft:
				return "<<";
			case Operator.ShiftRight:
				return ">>";
			case Operator.LessThan:
				return "<";
			case Operator.GreaterThan:
				return ">";
			case Operator.LessOrEqual:
				return "<=";
			case Operator.GreaterOrEqual:
				return ">=";
			case Operator.Equal:
				return "==";
			case Operator.NotEqual:
				return "!=";
			case Operator.BitwiseAnd:
				return "&";
			case Operator.BitwiseOr:
				return "|";
			case Operator.ExclusiveOr:
				return "^";
			case Operator.LogicalOr:
				return "||";
			case Operator.LogicalAnd:
				return "&&";
			}

			return oper.ToString ();
		}

		Expression ForceConversion (Expression expr, Type target_type, TypeContainer tc)
		{
			if (expr.Type == target_type)
				return expr;

			return ConvertImplicit (expr, target_type, tc);
		}
		
		//
		// Note that handling the case l == Decimal || r == Decimal
		// is taken care of by the Step 1 Operator Overload resolution.
		//
		void DoNumericPromotions (TypeContainer tc, Type l, Type r)
		{
			if (l == TypeManager.double_type || r == TypeManager.double_type){
				//
				// If either operand is of type double, the other operand is
				// conveted to type double.
				//
				if (r != TypeManager.double_type)
					right = ConvertImplicit (right, TypeManager.double_type, tc);
				if (l != TypeManager.double_type)
					left = ConvertImplicit (left, TypeManager.double_type, tc);
				
				type = TypeManager.double_type;
			} else if (l == TypeManager.float_type || r == TypeManager.float_type){
				//
				// if either operand is of type float, th eother operand is
				// converd to type float.
				//
				if (r != TypeManager.double_type)
					right = ConvertImplicit (right, TypeManager.float_type, tc);
				if (l != TypeManager.double_type)
					left = ConvertImplicit (left, TypeManager.float_type, tc);
				type = TypeManager.float_type;
			} else if (l == TypeManager.uint64_type || r == TypeManager.uint64_type){
				//
				// If either operand is of type ulong, the other operand is
				// converted to type ulong.  or an error ocurrs if the other
				// operand is of type sbyte, short, int or long
				//
				Type other = null;
				
				if (l == TypeManager.uint64_type)
					other = r;
				else if (r == TypeManager.uint64_type)
					other = l;

				if ((other == TypeManager.sbyte_type) ||
				    (other == TypeManager.short_type) ||
				    (other == TypeManager.int32_type) ||
				    (other == TypeManager.int64_type)){
					string oper = OperName ();
					
					tc.RootContext.Report.Error (34, "Operator `" + OperName ()
								     + "' is ambiguous on operands of type `"
								     + TypeManager.CSharpName (l) + "' "
								     + "and `" + TypeManager.CSharpName (r)
								     + "'");
				}
				type = TypeManager.uint64_type;
			} else if (l == TypeManager.int64_type || r == TypeManager.int64_type){
				//
				// If either operand is of type long, the other operand is converted
				// to type long.
				//
				if (l != TypeManager.int64_type)
					left = ConvertImplicit (left, TypeManager.int64_type, tc);
				if (r != TypeManager.int64_type)
					right = ConvertImplicit (right, TypeManager.int64_type, tc);

				type = TypeManager.int64_type;
			} else if (l == TypeManager.uint32_type || r == TypeManager.uint32_type){
				//
				// If either operand is of type uint, and the other
				// operand is of type sbyte, short or int, othe operands are
				// converted to type long.
				//
				Type other = null;
				
				if (l == TypeManager.uint32_type)
					other = r;
				else if (r == TypeManager.uint32_type)
					other = l;

				if ((other == TypeManager.sbyte_type) ||
				    (other == TypeManager.short_type) ||
				    (other == TypeManager.int32_type)){
					left = ForceConversion (left, TypeManager.int64_type, tc);
					right = ForceConversion (right, TypeManager.int64_type, tc);
					type = TypeManager.int64_type;
				} else {
					//
					// if either operand is of type uint, the other
					// operand is converd to type uint
					//
					left = ForceConversion (left, TypeManager.uint32_type, tc);
					right = ForceConversion (left, TypeManager.uint32_type, tc);
					type = TypeManager.uint32_type;
				} 
			} else {
				left = ForceConversion (left, TypeManager.int32_type, tc);
				right = ForceConversion (right, TypeManager.int32_type, tc);
				type = TypeManager.int32_type;
			}
		}

		void error19 (TypeContainer tc)
		{
			tc.RootContext.Report.Error (
				19,
				"Operator " + OperName () + " cannot be applied to operands of type `" +
				TypeManager.CSharpName (left.Type) + "' and `" +
				TypeManager.CSharpName (right.Type) + "'");
						     
		}
		
		Expression CheckShiftArguments (TypeContainer tc)
		{
			Expression e;
			Type l = left.Type;
			Type r = right.Type;

			e = ForceConversion (right, TypeManager.int32_type, tc);
			if (e == null){
				error19 (tc);
				return null;
			}
			right = e;

			if (((e = ConvertImplicit (left, TypeManager.int32_type, tc)) != null) ||
			    ((e = ConvertImplicit (left, TypeManager.uint32_type, tc)) != null) ||
			    ((e = ConvertImplicit (left, TypeManager.int64_type, tc)) != null) ||
			    ((e = ConvertImplicit (left, TypeManager.uint64_type, tc)) != null)){
				left = e;

				return this;
			}
			error19 (tc);
			return null;
		}
		
		Expression ResolveOperator (TypeContainer tc)
		{
			Type l = left.Type;
			Type r = right.Type;

			//
			// Step 1: Perform Operator Overload location
			//
			Expression left_expr, right_expr;
			
			string op = "op_" + oper;

			left_expr = MemberLookup (tc.RootContext, l, op, false);

			right_expr = MemberLookup (tc.RootContext, r, op, false);

			if (left_expr != null || right_expr != null) {
				//
				// Now we need to form the union of these two sets and
				// then call OverloadResolve on that.
				//
				MethodGroupExpr left_set = null, right_set = null;
				int length1 = 0, length2 = 0;

				if (left_expr != null) {
					left_set = (MethodGroupExpr) left_expr;
					length1 = left_set.Methods.Length;
				}

				if (right_expr != null) {
					right_set = (MethodGroupExpr) right_expr;
					length2 = right_set.Methods.Length;
				}

				MemberInfo [] mi = new MemberInfo [length1 + length2];
				if (left_set != null)
					left_set.Methods.CopyTo (mi, 0);
				if (right_set != null)
					right_set.Methods.CopyTo (mi, length1);
				
				MethodGroupExpr union = new MethodGroupExpr (mi);
				
				Arguments = new ArrayList ();
				Arguments.Add (new Argument (left, Argument.AType.Expression));
				Arguments.Add (new Argument (right, Argument.AType.Expression));

			
				method = Invocation.OverloadResolve (union, Arguments, tc, location);
				if (method != null) {
					Method m = (Method) TypeContainer.LookupMethodByBuilder (method);
					type = m.GetReturnType (tc);
					return this;
				}
			}

			//
			// Step 2: Default operations on CLI native types.
			//
			
			// Only perform numeric promotions on:
			// +, -, *, /, %, &, |, ^, ==, !=, <, >, <=, >=
			//
			if (oper == Operator.ShiftLeft || oper == Operator.ShiftRight){
				return CheckShiftArguments (tc);
			} else if (oper == Operator.LogicalOr || oper == Operator.LogicalAnd){

				if (l != TypeManager.bool_type || r != TypeManager.bool_type)
					error19 (tc);
			} else
				DoNumericPromotions (tc, l, r);

			if (left == null || right == null)
				return null;

			if (oper == Operator.BitwiseAnd ||
			    oper == Operator.BitwiseOr ||
			    oper == Operator.ExclusiveOr){
				if (!((l == TypeManager.int32_type) ||
				      (l == TypeManager.uint32_type) ||
				      (l == TypeManager.int64_type) ||
				      (l == TypeManager.uint64_type))){
					error19 (tc);
					return null;
				}
			}

			if (oper == Operator.Equal ||
			    oper == Operator.NotEqual ||
			    oper == Operator.LessOrEqual ||
			    oper == Operator.LessThan ||
			    oper == Operator.GreaterOrEqual ||
			    oper == Operator.GreaterThan){
				type = TypeManager.bool_type;
			}
			
			return this;
		}
		
		public override Expression Resolve (TypeContainer tc)
		{
			left = left.Resolve (tc);
			right = right.Resolve (tc);

			if (left == null || right == null)
				return null;

			return ResolveOperator (tc);
		}

		public bool IsBranchable ()
		{
			if (oper == Operator.Equal ||
			    oper == Operator.NotEqual ||
			    oper == Operator.LessThan ||
			    oper == Operator.GreaterThan ||
			    oper == Operator.LessOrEqual ||
			    oper == Operator.GreaterOrEqual){
				return true;
			} else
				return false;
		}

		// <summary>
		//   This entry point is used by routines that might want
		//   to emit a brfalse/brtrue after an expression, and instead
		//   they could use a more compact notation.
		//
		//   Typically the code would generate l.emit/r.emit, followed
		//   by the comparission and then a brtrue/brfalse.  The comparissions
		//   are sometimes inneficient (there are not as complete as the branches
		//   look for the hacks in Emit using double ceqs).
		//
		//   So for those cases we provide EmitBranchable that can emit the
		//   branch with the test
		// </summary>
		public void EmitBranchable (EmitContext ec, int target)
		{
			OpCode opcode;
			bool close_target = false;
			
			left.Emit (ec);
			right.Emit (ec);
			
			switch (oper){
			case Operator.Equal:
				if (close_target)
					opcode = OpCodes.Beq_S;
				else
					opcode = OpCodes.Beq;
				break;

			case Operator.NotEqual:
				if (close_target)
					opcode = OpCodes.Bne_Un_S;
				else
					opcode = OpCodes.Bne_Un;
				break;

			case Operator.LessThan:
				if (close_target)
					opcode = OpCodes.Blt_S;
				else
					opcode = OpCodes.Blt;
				break;

			case Operator.GreaterThan:
				if (close_target)
					opcode = OpCodes.Bgt_S;
				else
					opcode = OpCodes.Bgt;
				break;

			case Operator.LessOrEqual:
				if (close_target)
					opcode = OpCodes.Ble_S;
				else
					opcode = OpCodes.Ble;
				break;

			case Operator.GreaterOrEqual:
				if (close_target)
					opcode = OpCodes.Bge_S;
				else
					opcode = OpCodes.Ble;
				break;

			default:
				throw new Exception ("EmitBranchable called on non-EmitBranchable operator: "
						     + oper.ToString ());
			}

			ec.ig.Emit (opcode, target);
		}
		
		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Type l = left.Type;
			Type r = right.Type;
			OpCode opcode;

			if (method != null) {

				// Note that operators are static anyway
				
				if (Arguments != null) 
					Invocation.EmitArguments (ec, method, Arguments);
				
				if (method is MethodInfo)
					ig.Emit (OpCodes.Call, (MethodInfo) method);
				else
					ig.Emit (OpCodes.Call, (ConstructorInfo) method);
				
				return true;
			}
			
			left.Emit (ec);
			right.Emit (ec);

			switch (oper){
			case Operator.Multiply:
				if (ec.CheckState){
					if (l == TypeManager.int32_type || l == TypeManager.int64_type)
						opcode = OpCodes.Mul_Ovf;
					else if (l==TypeManager.uint32_type || l==TypeManager.uint64_type)
						opcode = OpCodes.Mul_Ovf_Un;
					else
						opcode = OpCodes.Mul;
				} else
					opcode = OpCodes.Mul;

				break;

			case Operator.Divide:
				if (l == TypeManager.uint32_type || l == TypeManager.uint64_type)
					opcode = OpCodes.Div_Un;
				else
					opcode = OpCodes.Div;
				break;

			case Operator.Modulo:
				if (l == TypeManager.uint32_type || l == TypeManager.uint64_type)
					opcode = OpCodes.Rem_Un;
				else
					opcode = OpCodes.Rem;
				break;

			case Operator.Add:
				if (ec.CheckState){
					if (l == TypeManager.int32_type || l == TypeManager.int64_type)
						opcode = OpCodes.Add_Ovf;
					else if (l==TypeManager.uint32_type || l==TypeManager.uint64_type)
						opcode = OpCodes.Add_Ovf_Un;
					else
						opcode = OpCodes.Mul;
				} else
					opcode = OpCodes.Add;
				break;

			case Operator.Subtract:
				if (ec.CheckState){
					if (l == TypeManager.int32_type || l == TypeManager.int64_type)
						opcode = OpCodes.Sub_Ovf;
					else if (l==TypeManager.uint32_type || l==TypeManager.uint64_type)
						opcode = OpCodes.Sub_Ovf_Un;
					else
						opcode = OpCodes.Sub;
				} else
					opcode = OpCodes.Sub;
				break;

			case Operator.ShiftRight:
				opcode = OpCodes.Shr;
				break;
				
			case Operator.ShiftLeft:
				opcode = OpCodes.Shl;
				break;

			case Operator.Equal:
				opcode = OpCodes.Ceq;
				break;

			case Operator.NotEqual:
				ec.ig.Emit (OpCodes.Ceq);
				ec.ig.Emit (OpCodes.Ldc_I4_0);
				
				opcode = OpCodes.Ceq;
				break;

			case Operator.LessThan:
				opcode = OpCodes.Clt;
				break;

			case Operator.GreaterThan:
				opcode = OpCodes.Cgt;
				break;

			case Operator.LessOrEqual:
				ec.ig.Emit (OpCodes.Cgt);
				ec.ig.Emit (OpCodes.Ldc_I4_0);
				
				opcode = OpCodes.Ceq;
				break;

			case Operator.GreaterOrEqual:
				ec.ig.Emit (OpCodes.Clt);
				ec.ig.Emit (OpCodes.Ldc_I4_1);
				
				opcode = OpCodes.Sub;
				break;

			case Operator.LogicalOr:
			case Operator.BitwiseOr:
				opcode = OpCodes.Or;
				break;

			case Operator.LogicalAnd:
			case Operator.BitwiseAnd:
				opcode = OpCodes.And;
				break;

			case Operator.ExclusiveOr:
				opcode = OpCodes.Xor;
				break;

			default:
				throw new Exception ("This should not happen: Operator = "
						     + oper.ToString ());
			}

			ig.Emit (opcode);

			return true;
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
			throw new Exception ("Unimplemented");
			// return this;
		}

		public override bool Emit (EmitContext ec)
		{
			return true;
		}
	}

	public class SimpleName : Expression {
		public readonly string Name;
		public readonly Location Location;
		
		public SimpleName (string name, Location l)
		{
			Name = name;
			Location = l;
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
						 "for the non-static field `"+Name+"'");
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
						 Name+"'");
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

			e = MemberLookup (tc.RootContext, tc.TypeBuilder, Name, true);
			if (e != null){
				if (e is TypeExpr)
					return e;
				else if (e is FieldExpr){
					FieldExpr fe = (FieldExpr) e;

					if (!fe.FieldInfo.IsStatic)
						fe.Instance = new This ();
				}
				
				if ((tc.ModFlags & Modifiers.STATIC) != 0)
					return MemberStaticCheck (r, e);
				else
					return e;
			}

			//
			// Do step 3 of the Simple Name resolution.
			//
			// FIXME: implement me.

			r.Error (103, Location, "The name `" + Name + "' does not exist in the class `" +
				 tc.Name + "'");

			return null;
		}
		
		//
		// SimpleName needs to handle a multitude of cases:
		//
		// simple_names and qualified_identifiers are placed on
		// the tree equally.
		//
		public override Expression Resolve (TypeContainer tc)
		{
			if (Name.IndexOf (".") != -1)
				return ResolveMemberAccess (tc, Name);
			else
				return ResolveSimpleName (tc);
		}

		public override bool Emit (EmitContext ec)
		{
			throw new Exception ("SimpleNames should be gone from the tree");
		}
	}

	// <summary>
	//   A simple interface that should be implemeneted by LValues
	// </summary>
	public interface LValue {
		void Store (ILGenerator ig);
	}
	
	public class LocalVariableReference : Expression, LValue {
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
				return Block.GetVariableInfo (Name);
			}
		}
		
		public override Expression Resolve (TypeContainer tc)
		{
			VariableInfo vi = Block.GetVariableInfo (Name);

			type = vi.VariableType;
			return this;
		}

		public override bool Emit (EmitContext ec)
		{
			VariableInfo vi = VariableInfo;
			ILGenerator ig = ec.ig;
			int idx = vi.Idx;

			switch (idx){
			case 0:
				ig.Emit (OpCodes.Ldloc_0);
				break;
				
			case 1:
				ig.Emit (OpCodes.Ldloc_1);
				break;

			case 2:
				ig.Emit (OpCodes.Ldloc_2);
				break;

			case 3:
				ig.Emit (OpCodes.Ldloc_3);
				break;

			default:
				if (idx < 255)
					ig.Emit (OpCodes.Ldloc_S, idx);
				else
					ig.Emit (OpCodes.Ldloc, idx);
				break;
			}

			return true;
		}

		public void Store (ILGenerator ig)
		{
			VariableInfo vi = VariableInfo;
			int idx = vi.Idx;
					
			switch (idx){
			case 0:
				ig.Emit (OpCodes.Stloc_0);
				break;
				
			case 1:
				ig.Emit (OpCodes.Stloc_1);
				break;
				
			case 2:
				ig.Emit (OpCodes.Stloc_2);
				break;
				
			case 3:
				ig.Emit (OpCodes.Stloc_3);
				break;
				
			default:
				if (idx < 255)
					ig.Emit (OpCodes.Stloc_S, idx);
				else
					ig.Emit (OpCodes.Stloc, idx);
				break;
			}
		}
	}

	public class ParameterReference : Expression, LValue {
		public readonly Parameters Pars;
		public readonly String Name;
		public readonly int Idx;
		
		public ParameterReference (Parameters pars, int idx, string name)
		{
			Pars = pars;
			Idx  = idx;
			Name = name;
			eclass = ExprClass.Variable;
		}

		public override Expression Resolve (TypeContainer tc)
		{
			Type [] types = Pars.GetParameterInfo (tc);

			type = types [Idx];

			return this;
		}

		public override bool Emit (EmitContext ec)
		{
			if (Idx < 255)
				ec.ig.Emit (OpCodes.Ldarg_S, Idx);
			else
				ec.ig.Emit (OpCodes.Ldarg, Idx);

			return true;
		}

		public void Store (ILGenerator ig)
		{
			if (Idx < 255)
				ig.Emit (OpCodes.Starg_S, Idx);
			else
				ig.Emit (OpCodes.Starg, Idx);
			
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

			set {
				expr = value;
			}
		}

		public bool Resolve (TypeContainer tc)
		{
			expr = expr.Resolve (tc);

			return expr != null;
		}

		public bool Emit (EmitContext ec)
		{
			return expr.Emit (ec);
		}
	}

	// <summary>
	//   Invocation of methods or delegates.
	// </summary>
	public class Invocation : Expression {
		public readonly ArrayList Arguments;
		public readonly Location Location;
		
		Expression expr;
		MethodBase method = null;
			
		static Hashtable method_parameter_cache;

		static Invocation ()
		{
			method_parameter_cache = new Hashtable ();
		}
			
		//
		// arguments is an ArrayList, but we do not want to typecast,
		// as it might be null.
		//
		// FIXME: only allow expr to be a method invocation or a
		// delegate invocation (7.5.5)
		//
		public Invocation (Expression expr, ArrayList arguments, Location l)
		{
			this.expr = expr;
			Arguments = arguments;
			Location = l;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		/// <summary>
		///   Computes whether Argument `a' and the Type t of the  ParameterInfo `pi' are
		///   compatible, and if so, how good is the match (in terms of
		///   "better conversions" (7.4.2.3).
		///
		///   0   is the best possible match.
		///   -1  represents a type mismatch.
		///   -2  represents a ref/out mismatch.
		/// </summary>
		static int Badness (Argument a, Type t)
		{
			Expression argument_expr = a.Expr;
			Type argument_type = argument_expr.Type;

			if (argument_type == null){
				throw new Exception ("Expression of type " + a.Expr + " does not resolve its type");
			}
			
			if (t == argument_type) 
				return 0;

			//
			// Now probe whether an implicit constant expression conversion
			// can be used.
			//
			// An implicit constant expression conversion permits the following
			// conversions:
			//
			//    * A constant-expression of type `int' can be converted to type
			//      sbyte, byute, short, ushort, uint, ulong provided the value of
			//      of the expression is withing the range of the destination type.
			//
			//    * A constant-expression of type long can be converted to type
			//      ulong, provided the value of the constant expression is not negative
			//
			// FIXME: Note that this assumes that constant folding has
			// taken place.  We dont do constant folding yet.
			//

			if (argument_type == TypeManager.int32_type && argument_expr is IntLiteral){
				IntLiteral ei = (IntLiteral) argument_expr;
				int value = ei.Value;
				
				if (t == TypeManager.sbyte_type){
					if (value >= SByte.MinValue && value <= SByte.MaxValue)
						return 1;
				} else if (t == TypeManager.byte_type){
					if (Byte.MinValue >= 0 && value <= Byte.MaxValue)
						return 1;
				} else if (t == TypeManager.short_type){
					if (value >= Int16.MinValue && value <= Int16.MaxValue)
						return 1;
				} else if (t == TypeManager.ushort_type){
					if (value >= UInt16.MinValue && value <= UInt16.MaxValue)
						return 1;
				} else if (t == TypeManager.uint32_type){
					//
					// we can optimize this case: a positive int32
					// always fits on a uint32
					//
					if (value >= 0)
						return 1;
				} else if (t == TypeManager.uint64_type){
					//
					// we can optimize this case: a positive int32
					// always fits on a uint64
					//
					if (value >= 0)
						return 1;
				}
			} else if (argument_type == TypeManager.int64_type && argument_expr is LongLiteral){
				LongLiteral ll = (LongLiteral) argument_expr;
				
				if (t == TypeManager.uint64_type){
					if (ll.Value > 0)
						return 1;
				}
			}
			
			// FIXME: Implement user-defined implicit conversions here.
			// FIXME: Implement better conversion here.
			
			return -1;
		}

		// <summary>
		//   Returns the Parameters (a ParameterData interface) for the
		//   Method `mb'
		// </summary>
		static ParameterData GetParameterData (MethodBase mb)
		{
			object pd = method_parameter_cache [mb];

			if (pd != null)
				return (ParameterData) pd;

			if (mb is MethodBuilder || mb is ConstructorBuilder){
				MethodCore mc = TypeContainer.LookupMethodByBuilder (mb);

				InternalParameters ip = mc.ParameterInfo;
				method_parameter_cache [mb] = ip;

				return (ParameterData) ip;
			} else {
				ParameterInfo [] pi = mb.GetParameters ();
				ReflectionParameters rp = new ReflectionParameters (pi);
				method_parameter_cache [mb] = rp;

				return (ParameterData) rp;
			}
		}

		static bool ConversionExists (Type from, Type to, TypeContainer tc)
		{
			// Locate user-defined implicit operators

			Expression mg;
			
			mg = MemberLookup (tc.RootContext, to, "op_Implicit", false);

			if (mg != null) {
				MethodGroupExpr me = (MethodGroupExpr) mg;
				
				for (int i = me.Methods.Length; i > 0;) {
					i--;
					MethodBase mb = me.Methods [i];
					ParameterData pd = GetParameterData (mb);
					
					if (from == pd.ParameterType (0))
						return true;
				}
			}

			mg = MemberLookup (tc.RootContext, from, "op_Implicit", false);

			if (mg != null) {
				MethodGroupExpr me = (MethodGroupExpr) mg;

				for (int i = me.Methods.Length; i > 0;) {
					i--;
					MethodBase mb = me.Methods [i];
					Method method = (Method) TypeContainer.LookupMethodByBuilder (mb);
					
					if (method.GetReturnType (tc) == to)
						return true;
				}
			}
			
			return false;
		}
		
		// <summary>
		//  Determines "better conversion" as specified in 7.4.2.3
		//  Returns : 1 if a->p is better
		//            0 if a->q or neither is better 
		// </summary>
		static int BetterConversion (Argument a, Type p, Type q, TypeContainer tc)
		{
			
			Type argument_type = a.Expr.Type;
			Expression argument_expr = a.Expr;

			if (argument_type == null)
				throw new Exception ("Expression of type " + a.Expr + " does not resolve its type");
			
			
			if (p == q)
				return 0;

			if (argument_type == p)
				return 1;

			if (argument_type == q)
				return 0;

			//
			// Now probe whether an implicit constant expression conversion
			// can be used.
			//
			// An implicit constant expression conversion permits the following
			// conversions:
			//
			//    * A constant-expression of type `int' can be converted to type
			//      sbyte, byute, short, ushort, uint, ulong provided the value of
			//      of the expression is withing the range of the destination type.
			//
			//    * A constant-expression of type long can be converted to type
			//      ulong, provided the value of the constant expression is not negative
			//
			// FIXME: Note that this assumes that constant folding has
			// taken place.  We dont do constant folding yet.
			//

			if (argument_type == TypeManager.int32_type && argument_expr is IntLiteral){
				IntLiteral ei = (IntLiteral) argument_expr;
				int value = ei.Value;
				
				if (p == TypeManager.sbyte_type){
					if (value >= SByte.MinValue && value <= SByte.MaxValue)
						return 1;
				} else if (p == TypeManager.byte_type){
					if (Byte.MinValue >= 0 && value <= Byte.MaxValue)
						return 1;
				} else if (p == TypeManager.short_type){
					if (value >= Int16.MinValue && value <= Int16.MaxValue)
						return 1;
				} else if (p == TypeManager.ushort_type){
					if (value >= UInt16.MinValue && value <= UInt16.MaxValue)
						return 1;
				} else if (p == TypeManager.uint32_type){
					//
					// we can optimize this case: a positive int32
					// always fits on a uint32
					//
					if (value >= 0)
						return 1;
				} else if (p == TypeManager.uint64_type){
					//
					// we can optimize this case: a positive int32
					// always fits on a uint64
					//
					if (value >= 0)
						return 1;
				}
			} else if (argument_type == TypeManager.int64_type && argument_expr is LongLiteral){
				LongLiteral ll = (LongLiteral) argument_expr;
				
				if (p == TypeManager.uint64_type){
					if (ll.Value > 0)
						return 1;
				}
			}

			// User-defined Implicit conversions come here
			
			if (q != null)
				if (ConversionExists (p, q, tc) == true &&
				    ConversionExists (q, p, tc) == false)
					return 1;
			
			if (p == TypeManager.sbyte_type)
				if (q == TypeManager.byte_type || q == TypeManager.ushort_type ||
				    q == TypeManager.uint32_type || q == TypeManager.uint64_type) 
					return 1;

			if (p == TypeManager.short_type)
				if (q == TypeManager.ushort_type || q == TypeManager.uint32_type ||
				    q == TypeManager.uint64_type)
					return 1;

			if (p == TypeManager.int32_type)
				if (q == TypeManager.uint32_type || q == TypeManager.uint64_type)
					return 1;

			if (p == TypeManager.int64_type)
				if (q == TypeManager.uint64_type)
					return 1;

			return 0;
		}
		
		// <summary>
		//  Determines "Better function" and returns an integer indicating :
		//  0 if candidate ain't better
		//  1 if candidate is better than the current best match
		// </summary>
		static int BetterFunction (ArrayList args, MethodBase candidate, MethodBase best, TypeContainer tc)
		{
			ParameterData candidate_pd = GetParameterData (candidate);
			ParameterData best_pd;
			int argument_count;

			if (args == null)
				argument_count = 0;
			else
				argument_count = args.Count;

			if (candidate_pd.Count == 0 && argument_count == 0)
				return 1;

			if (best == null) {
				if (candidate_pd.Count == argument_count) {
					int x = 0;
					for (int j = argument_count; j > 0;) {
						j--;
						
						Argument a = (Argument) args [j];
						
						x = BetterConversion (a, candidate_pd.ParameterType (j), null, tc);
						
						if (x > 0)
							continue;
						else 
							break;
					}
					
					if (x > 0)
						return 1;
					else
						return 0;
					
				} else
					return 0;
			}

			best_pd = GetParameterData (best);

			if (candidate_pd.Count == argument_count && best_pd.Count == argument_count) {
				int rating1 = 0, rating2 = 0;
				
				for (int j = argument_count; j > 0;) {
					j--;
					int x, y;
					
					Argument a = (Argument) args [j];

					x = BetterConversion (a, candidate_pd.ParameterType (j),
							      best_pd.ParameterType (j), tc);
					y = BetterConversion (a, best_pd.ParameterType (j),
							      candidate_pd.ParameterType (j), tc);

					rating1 += x;
					rating2 += y;
				}

				if (rating1 > rating2)
					return 1;
				else
					return 0;
			} else
				return 0;
			
		}

		public static string FullMethodDesc (MethodBase mb)
		{
			StringBuilder sb = new StringBuilder (mb.Name);
			ParameterData pd = GetParameterData (mb);
			
			sb.Append (" (");
			for (int i = pd.Count; i > 0;) {
				i--;
				sb.Append (TypeManager.CSharpName (pd.ParameterType (i)));
				if (i != 0)
					sb.Append (",");
			}
			
			sb.Append (")");
			return sb.ToString ();
		}
		
		// <summary>
		//   Find the Applicable Function Members (7.4.2.1)
		//
		//   me: Method Group expression with the members to select.
		//       it might contain constructors or methods (or anything
		//       that maps to a method).
		//
		//   Arguments: ArrayList containing resolved Argument objects.
		//
		//   Returns: The MethodBase (either a ConstructorInfo or a MethodInfo)
		//            that is the best match of me on Arguments.
		//
		// </summary>
		public static MethodBase OverloadResolve (MethodGroupExpr me, ArrayList Arguments,
							  TypeContainer tc, Location loc)
		{
			ArrayList afm = new ArrayList ();
			int best_match_idx = -1;
			MethodBase method = null;
			int argument_count;
			
			for (int i = me.Methods.Length; i > 0; ){
				i--;
				MethodBase candidate  = me.Methods [i];
				int x;
				
				x = BetterFunction (Arguments, candidate, method, tc);
				
				if (x == 0)
					continue;
				else {
					best_match_idx = i;
					method = me.Methods [best_match_idx];
				}
			}
			
			if (best_match_idx != -1)
				return method;

			// Now we see if we can at least find a method with the same number of arguments
			// and then try doing implicit conversion on the arguments

			if (Arguments == null)
				argument_count = 0;
			else
				argument_count = Arguments.Count;

			ParameterData pd = null;
			
			for (int i = me.Methods.Length; i > 0;) {
				i--;
				MethodBase mb = me.Methods [i];
				pd = GetParameterData (mb);

				if (pd.Count == argument_count) {
					best_match_idx = i;
					method = me.Methods [best_match_idx];
					break;
				} else
					continue;
			}

			if (best_match_idx == -1)
				return null;

			// And now convert implicitly, each argument to the required type
			
			pd = GetParameterData (method);

			for (int j = argument_count; j > 0;) {
				j--;
				Argument a = (Argument) Arguments [j];
				Expression a_expr = a.Expr;
				
				Expression conv = ConvertImplicit (a_expr, pd.ParameterType (j), tc);

				if (conv == null) {
					tc.RootContext.Report.Error (1502, loc,
					       "The best overloaded match for method '" + FullMethodDesc (method) +
					       "' has some invalid arguments");
					tc.RootContext.Report.Error (1503, loc,
					       "Argument " + (j+1) +
					       " : Cannot convert from '" + TypeManager.CSharpName (a_expr.Type)
					       + "' to '" + TypeManager.CSharpName (pd.ParameterType (j)) + "'");
					return null;
				}

				//
				// Update the argument with the implicit conversion
				//
				if (a_expr != conv)
					a.Expr = conv;
			}
			
			return method;
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
				report118 (tc, this.expr, "method group");
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

			method = OverloadResolve ((MethodGroupExpr) this.expr, Arguments, tc, Location);

			if (method == null){
				tc.RootContext.Report.Error (-6, Location,
				       "Could not find any applicable function for this argument list");
				return null;
			}

			if (method is MethodInfo)
				type = ((MethodInfo)method).ReturnType;

			return this;
		}

		public static void EmitArguments (EmitContext ec, MethodBase method, ArrayList Arguments)
		{
			int top;

			if (Arguments != null)
				top = Arguments.Count;
			else
				top = 0;

			for (int i = 0; i < top; i++){
				Argument a = (Argument) Arguments [i];

				Console.WriteLine ("Perform the actual type widening of arguments here for things like: void fn (sbyte s);  ... fn (1)");
				
				a.Emit (ec);
			}
		}
		
		public override bool Emit (EmitContext ec)
		{
			bool is_static = method.IsStatic;

			if (!is_static){
				MethodGroupExpr mg = (MethodGroupExpr) this.expr;

				if (mg.InstanceExpression == null){
					throw new Exception ("Internal compiler error.  Should check in the method groups for static/instance");
				}

				mg.InstanceExpression.Emit (ec);
			}

			if (Arguments != null)
				EmitArguments (ec, method, Arguments);

			if (is_static){
				if (method is MethodInfo)
					ec.ig.Emit (OpCodes.Call, (MethodInfo) method);
				else
					ec.ig.Emit (OpCodes.Call, (ConstructorInfo) method);
			} else {
				if (method is MethodInfo)
					ec.ig.Emit (OpCodes.Callvirt, (MethodInfo) method);
				else
					ec.ig.Emit (OpCodes.Callvirt, (ConstructorInfo) method);
			}

			if (method is MethodInfo){
				return ((MethodInfo)method).ReturnType != TypeManager.void_type;
			} else {
				//
				// Constructors do not leave any values on the stack
				//
				return false;
			}
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

		Location Location;
		MethodBase method = null;

		public New (string requested_type, ArrayList arguments, Location loc)
		{
			RequestedType = requested_type;
			Arguments = arguments;
			NewType = NType.Object;
			Location = loc;
		}

		public New (string requested_type, ArrayList exprs, string rank, ArrayList initializers, Location loc)
		{
			RequestedType = requested_type;
			Indices       = exprs;
			Rank          = rank;
			Initializers  = initializers;
			NewType       = NType.Array;
			Location      = loc;
		}
		
		public override Expression Resolve (TypeContainer tc)
		{
			type = tc.LookupType (RequestedType, false);

			if (type == null)
				return null;

			Expression ml;

			ml = MemberLookup (tc.RootContext, type, ".ctor", false,
					   MemberTypes.Constructor, AllBindingsFlags);

			if (! (ml is MethodGroupExpr)){
				//
				// FIXME: Find proper error
				//
				report118 (tc, ml, "method group");
				return null;
			}
			
			if (Arguments != null){
				for (int i = Arguments.Count; i > 0;){
					--i;
					Argument a = (Argument) Arguments [i];

					if (!a.Resolve (tc))
						return null;
				}
			}

			method = Invocation.OverloadResolve ((MethodGroupExpr) ml, Arguments, tc, Location);

			if (method == null) {
				tc.RootContext.Report.Error (-6, Location,
				"New invocation: Can not find a constructor for this argument list");
				return null;
			}
			
			return this;
		}

		public override bool Emit (EmitContext ec)
		{
			Invocation.EmitArguments (ec, method, Arguments);
			ec.ig.Emit (OpCodes.Newobj, (ConstructorInfo) method);
			return true;
		}
	}

	//
	// Represents the `this' construct
	//
	public class This : Expression, LValue {
		public override Expression Resolve (TypeContainer tc)
		{
			eclass = ExprClass.Variable;
			type = tc.TypeBuilder;

			//
			// FIXME: Verify that this is only used in instance contexts.
			//
			return this;
		}

		public override bool Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldarg_0);
			return true;
		}

		public void Store (ILGenerator ig)
		{
			//
			// Assignment to the "this" variable
			//
			ig.Emit (OpCodes.Starg, 0);
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
			type = tc.LookupType (QueriedType, false);

			if (type == null)
				return null;
			
			eclass = ExprClass.Type;
			return this;
		}

		public override bool Emit (EmitContext ec)
		{
			throw new Exception ("Implement me");
			// FIXME: Implement.
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
			throw new Exception ("Unimplemented");
			// return this;
		}

		public override bool Emit (EmitContext ec)
		{
			throw new Exception ("Implement me");
		}
	}

	public class MemberAccess : Expression {
		public readonly string Identifier;
		Expression expr;
		Expression member_lookup;
		
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
			Expression new_expression = expr.Resolve (tc);

			if (new_expression == null)
				return null;

			member_lookup = MemberLookup (tc.RootContext, expr.Type, Identifier, false);

			if (member_lookup is MethodGroupExpr){
				MethodGroupExpr mg = (MethodGroupExpr) member_lookup;

				//
				// Bind the instance expression to it
				//
				// FIXME: This is a horrible way of detecting if it is
				// an instance expression.  Figure out how to fix this.
				//

				if (expr is LocalVariableReference ||
				    expr is ParameterReference ||
				    expr is FieldExpr)
					mg.InstanceExpression = expr;
					
				return member_lookup;
			} else if (member_lookup is FieldExpr){
				FieldExpr fe = (FieldExpr) member_lookup;

				fe.Instance = expr;

				return member_lookup;
			} else
				//
				// FIXME: This should generate the proper node
				// ie, for a Property Access, it should like call it
				// and stuff.

				return member_lookup;
		}

		public override bool Emit (EmitContext ec)
		{
			throw new Exception ("Implement me");
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

		public override bool Emit (EmitContext ec)
		{
			throw new Exception ("Namespace expressions should never be emitted");
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

		override public bool Emit (EmitContext ec)
		{
			throw new Exception ("Implement me");
		}
	}

	// <summary>
	//   MethodGroup Expression.
	//  
	//   This is a fully resolved expression that evaluates to a type
	// </summary>
	public class MethodGroupExpr : Expression {
		public readonly MethodBase [] Methods;
		Expression instance_expression = null;
		
		public MethodGroupExpr (MemberInfo [] mi)
		{
			Methods = new MethodBase [mi.Length];
			mi.CopyTo (Methods, 0);
			eclass = ExprClass.MethodGroup;
		}

		//
		// `A method group may have associated an instance expression' 
		// 
		public Expression InstanceExpression {
			get {
				return instance_expression;
			}

			set {
				instance_expression = value;
			}
		}
		
		override public Expression Resolve (TypeContainer tc)
		{
			return this;
		}

		override public bool Emit (EmitContext ec)
		{
			throw new Exception ("This should never be reached");
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
			throw new Exception ("Unimplemented");
			// return this;
		}

		public override bool Emit (EmitContext ec)
		{
			throw new Exception ("Unimplemented");
		}
	}


	//   Fully resolved expression that evaluates to a Field
	// </summary>
	public class FieldExpr : Expression, LValue {
		public readonly FieldInfo FieldInfo;
		public Expression Instance;
			
		public FieldExpr (FieldInfo fi)
		{
			FieldInfo = fi;
			eclass = ExprClass.Variable;
			type = fi.FieldType;
		}

		override public Expression Resolve (TypeContainer tc)
		{
			if (!FieldInfo.IsStatic){
				if (Instance == null){
					throw new Exception ("non-static FieldExpr without instance var\n" +
							     "You have to assign the Instance variable\n" +
							     "Of the FieldExpr to set this\n");
				}

				Instance = Instance.Resolve (tc);
				if (Instance == null)
					return null;
				
			}
			return this;
		}

		override public bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (FieldInfo.IsStatic)
				ig.Emit (OpCodes.Ldsfld, FieldInfo);
			else {
				Instance.Emit (ec);
				
				ig.Emit (OpCodes.Ldfld, FieldInfo);
			}

			return true;
		}

		public void Store (ILGenerator ig)
		{
			if (FieldInfo.IsStatic)
				ig.Emit (OpCodes.Stsfld, FieldInfo);
			else
				ig.Emit (OpCodes.Stfld, FieldInfo);
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
				
			MethodBase [] acc = pi.GetAccessors ();

			for (int i = 0; i < acc.Length; i++)
				if (acc [i].IsStatic)
					IsStatic = true;

			type = pi.PropertyType;
		}

		override public Expression Resolve (TypeContainer tc)
		{
			// We are born in resolved state. 
			return this;
		}

		override public bool Emit (EmitContext ec)
		{
			// FIXME: Implement;
			throw new Exception ("Unimplemented");
		}
	}

	// <summary>
	//   Fully resolved expression that evaluates to a Property
	// </summary>
	public class EventExpr : Expression {
		public readonly EventInfo EventInfo;
		
		public EventExpr (EventInfo ei)
		{
			EventInfo = ei;
			eclass = ExprClass.EventAccess;
		}

		override public Expression Resolve (TypeContainer tc)
		{
			// We are born in resolved state. 
			return this;
		}

		override public bool Emit (EmitContext ec)
		{
			throw new Exception ("Implement me");
			// FIXME: Implement.
		}
	}
	
	public class CheckedExpr : Expression {

		public Expression Expr;

		public CheckedExpr (Expression e)
		{
			Expr = e;
		}

		public override Expression Resolve (TypeContainer tc)
		{
			Expr = Expr.Resolve (tc);

			if (Expr == null)
				return null;

			eclass = Expr.ExprClass;
			type = Expr.Type;
			return this;
		}

		public override bool Emit (EmitContext ec)
		{
			bool last_check = ec.CheckState;
			bool v;
			
			ec.CheckState = true;
			
			v = Expr.Emit (ec);

			ec.CheckState = last_check;

			return v;
		}
		
	}

	public class UnCheckedExpr : Expression {

		public Expression Expr;

		public UnCheckedExpr (Expression e)
		{
			Expr = e;
		}

		public override Expression Resolve (TypeContainer tc)
		{
			Expr = Expr.Resolve (tc);

			if (Expr == null)
				return null;

			eclass = Expr.ExprClass;
			type = Expr.Type;
			return this;
		}

		public override bool Emit (EmitContext ec)
		{
			bool last_check = ec.CheckState;
			bool v;
			
			ec.CheckState = false;
			
			v = Expr.Emit (ec);

			ec.CheckState = last_check;

			return v;
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
			// FIXME: Implement;
			throw new Exception ("Unimplemented");
			// return this;
		}
		
		public override bool Emit (EmitContext ec)
		{
			// FIXME : Implement !
			throw new Exception ("Unimplemented");
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
			// FIXME: Implement;
			throw new Exception ("Unimplemented");
			// return this;
		}

		public override bool Emit (EmitContext ec)
		{
			throw new Exception ("Unimplemented");
		}
	}

	public class UserImplicitCast : Expression {

		Expression source;
		Type       target; 
		MethodBase method;
		ArrayList  arguments;
		
		public UserImplicitCast (Expression source, Type target)
		{
			this.source = source;
			this.target = target;
		}

		public override Expression Resolve (TypeContainer tc)
		{
			source = source.Resolve (tc);

			if (source == null)
				return null;

			Expression mg;

			mg = MemberLookup (tc.RootContext, source.Type, "op_Implicit", false);

			if (mg != null) {
				
				MethodGroupExpr me = (MethodGroupExpr) mg;

				arguments = new ArrayList ();
				arguments.Add (new Argument (source, Argument.AType.Expression));

				method = Invocation.OverloadResolve (me, arguments, tc, new Location ("", 0,0));
			
				if (method != null) {
					Method m = (Method) TypeContainer.LookupMethodByBuilder (method);
					type = m.GetReturnType (tc);

					if (type != target)
						return null;
					
					return this;
				} else
					return null;

			} else
				return null;
		}
		
		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			if (method != null) {

				// Note that operators are static anyway
				
				if (arguments != null) 
					Invocation.EmitArguments (ec, method, arguments);
				
				if (method is MethodInfo)
					ig.Emit (OpCodes.Call, (MethodInfo) method);
				else
					ig.Emit (OpCodes.Call, (ConstructorInfo) method);
				
				return true;
			}

			return false;
			

		}

	}
}
