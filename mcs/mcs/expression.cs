//
// expression.cs: Expression representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
//
// Ideas:
//   Maybe we should make Resolve be an instance method that just calls
//   the virtual DoResolve function and checks conditions like the eclass
//   and type being set if a non-null value is returned.  For robustness
//   purposes.
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

		// <summary>
		//   Utility wrapper routine for Error, just to beautify the code
		// </summary>
		static protected void Error (TypeContainer tc, int error, string s)
		{
			tc.RootContext.Report.Error (error, s);
		}

		static protected void Error (TypeContainer tc, int error, Location l, string s)
		{
			tc.RootContext.Report.Error (error, l, s);
		}
		
		// <summary>
		//   Utility wrapper routine for Warning, just to beautify the code
		// </summary>
		static protected void Warning (TypeContainer tc, int warning, string s)
		{
			tc.RootContext.Report.Warning (warning, s);
		}

		// <summary>
		//   Performs semantic analysis on the Expression
		// </summary>
		//
		// <remarks>
		//   The Resolve method is invoked to perform the semantic analysis
		//   on the node.
		//
		//   The return value is an expression (it can be the
		//   same expression in some cases) or a new
		//   expression that better represents this node.
		//   
		//   For example, optimizations of Unary (LiteralInt)
		//   would return a new LiteralInt with a negated
		//   value.
		//   
		//   If there is an error during semantic analysis,
		//   then an error should
	        //   be reported (using TypeContainer.RootContext.Report) and a null
		//   value should be returned.
		//   
		//   There are two side effects expected from calling
		//   Resolve(): the the field variable "eclass" should
		//   be set to any value of the enumeration
		//   `ExprClass' and the type variable should be set
		//   to a valid type (this is the type of the
		//   expression).
		// </remarks>
		
		public abstract Expression Resolve (TypeContainer tc);

		// <summary>
		//   Emits the code for the expression
		// </summary>
		//
		// <remarks>
		// 
		//   The Emit method is invoked to generate the code
		//   for the expression.  
		//
		// </remarks>
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
		public static Expression MemberLookup (TypeContainer tc, Type t, string name,
						       bool same_type, MemberTypes mt, BindingFlags bf)
		{
			if (same_type)
				bf |= BindingFlags.NonPublic;

			MemberInfo [] mi = tc.RootContext.TypeManager.FindMembers (
				t, mt, bf, Type.FilterName, name);

			if (mi == null)
				return null;
			
			if (mi.Length == 1 && !(mi [0] is MethodBase))
				return Expression.ExprClassFromMemberInfo (mi [0]);
			
			for (int i = 0; i < mi.Length; i++)
				if (!(mi [i] is MethodBase)){
					Error (tc,
					       -5, "Do not know how to reproduce this case: " + 
					       "Methods and non-Method with the same name, " +
					       "report this please");

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

		public static Expression MemberLookup (TypeContainer tc, Type t, string name,
						       bool same_type)
		{
			return MemberLookup (tc, t, name, same_type, AllMemberTypes, AllBindingsFlags);
		}

		//
		// I am in general unhappy with this implementation.
		//
		// I need to revise this.
		//
		static public Expression ResolveMemberAccess (TypeContainer tc, string name)
		{
			Expression left_e = null;
			int dot_pos = name.LastIndexOf (".");
			string left = name.Substring (0, dot_pos);
			string right = name.Substring (dot_pos + 1);
			Type t;

			if ((t = tc.LookupType (left, false)) != null)
				left_e = new TypeExpr (t);
			else {
				//
				// FIXME: IMplement:
				
				// Handle here:
				//    T.P  Static property access (P) on Type T.
				//    e.P  instance property access on instance e for P.
				//    p
				//
			}

			if (left_e == null){
				Error (tc, 246, "Can not find type or namespace `"+left+"'");
				return null;
			}

			switch (left_e.ExprClass){
			case ExprClass.Type:
				return  MemberLookup (tc,
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
				throw new Exception ("Should have got the " + left_e.ExprClass +
						     " handled before");
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
		//   Handles expressions like this: decimal d; d = 1;
		//   and changes them into: decimal d; d = new System.Decimal (1);
		// </summary>
		static Expression InternalTypeConstructor (TypeContainer tc, Expression expr, Type target)
		{
			ArrayList args = new ArrayList ();

			args.Add (new Argument (expr, Argument.AType.Expression));
			
			Expression ne = new New (target.FullName, args,
						 new Location ("FIXME", 1, 1));

			return ne.Resolve (tc);
		}
						    
		// <summary>
		//   Converts implicitly the resolved expression `expr' into the
		//   `target_type'.  It returns a new expression that can be used
		//   in a context that expects a `target_type'. 
		// </summary>
		static public Expression ConvertImplicit (TypeContainer tc, Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (expr_type == target_type)
				return expr;
			
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
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (tc, expr, target_type);
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
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (tc, expr, target_type);
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
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (tc, expr, target_type);
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
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (tc, expr, target_type);
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
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (tc, expr, target_type);
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
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (tc, expr, target_type);
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
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (tc, expr, target_type);
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
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (tc, expr, target_type);
			} else
				return ImplicitReferenceConversion (expr, target_type);


			if (UserImplicitCast.CanConvert (tc, expr, target_type) == true) {
				Expression imp = new UserImplicitCast (expr, target_type);
				imp.Resolve (tc);
				return imp;
			}

			//
			//  Could not find an implicit cast.
			//
			return null;
		}

		// <summary>
		//   Attemps to perform an implict constant conversion of the IntLiteral
		//   into a different data type using casts (See Implicit Constant
		//   Expression Conversions)
		// </summary>
		static protected Expression TryImplicitIntConversion (Type target_type, IntLiteral il)
		{
			int value = il.Value;
			
			if (target_type == TypeManager.sbyte_type){
				if (value >= SByte.MinValue && value <= SByte.MaxValue)
					return il;
			} else if (target_type == TypeManager.byte_type){
				if (Byte.MinValue >= 0 && value <= Byte.MaxValue)
					return il;
			} else if (target_type == TypeManager.short_type){
				if (value >= Int16.MinValue && value <= Int16.MaxValue)
					return il;
			} else if (target_type == TypeManager.ushort_type){
				if (value >= UInt16.MinValue && value <= UInt16.MaxValue)
					return il;
			} else if (target_type == TypeManager.uint32_type){
				//
				// we can optimize this case: a positive int32
				// always fits on a uint32
				//
				if (value >= 0)
					return il;
			} else if (target_type == TypeManager.uint64_type){
				//
				// we can optimize this case: a positive int32
				// always fits on a uint64.  But we need an opcode
				// to do it.
				//
				if (value >= 0)
					return new OpcodeCast (il, target_type, OpCodes.Conv_I8);
			}

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
			
			e = ConvertImplicit (tc, target, type);
			if (e != null)
				return e;
			
			//
			// Attempt to do the implicit constant expression conversions

			if (target is IntLiteral){
				e = TryImplicitIntConversion (type, (IntLiteral) target);
				if (e != null)
					return e;
			} else if (target is LongLiteral){
				//
				// Try the implicit constant expression conversion
				// from long to ulong, instead of a nice routine,
				// we just inline it
				//
				if (((LongLiteral) target).Value > 0)
					return target;
			}
			
			string msg = "Can not convert implicitly from `"+
				TypeManager.CSharpName (target.Type) + "' to `" +
				TypeManager.CSharpName (type) + "'";

			Error (tc, 29, l, msg);

			return null;
		}

		// <summary>
		//   Performs the explicit numeric conversions
		// </summary>
		static Expression ConvertNumericExplicit (TypeContainer tc, Expression expr,
							  Type target_type)
		{
			Type expr_type = expr.Type;
			
			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to byte, ushort, uint, ulong, char
				//
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to sbyte and char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to sbyte, byte, ushort, uint, ulong, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to sbyte, byte, short, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to sbyte, byte, short, ushort, uint, ulong, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.uint32_type)
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to sbyte, byte, short, ushort, int, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.int32_type)
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.int64_type){
				//
				// From long to sbyte, byte, short, ushort, int, uint, ulong, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.uint64_type)
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.uint64_type){
				//
				// From ulong to sbyte, byte, short, ushort, int, uint, long, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.int64_type)
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to sbyte, byte, short
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
			} else if (expr_type == TypeManager.float_type){
				//
				// From float to sbyte, byte, short,
				// ushort, int, uint, long, ulong, char
				// or decimal
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (tc, expr, target_type);
			} else if (expr_type == TypeManager.double_type){
				//
				// From double to byte, byte, short,
				// ushort, int, uint, long, ulong,
				// char, float or decimal
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (tc, expr, target_type);
			} 

			// decimal is taken care of by the op_Explicit methods.

			return null;
		}
		
		// <summary>
		//   Performs an explicit conversion of the expression `expr' whose
		//   type is expr.Type to `target_type'.
		// </summary>
		static public Expression ConvertExplicit (TypeContainer tc, Expression expr,
							  Type target_type)
		{
			Expression ne = ConvertImplicit (tc, expr, target_type);

			if (ne != null)
				return ne;

			ne = ConvertNumericExplicit (tc, expr, target_type);
			if (ne != null)
				return ne;

			
			return expr;
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
			Error (tc, 118, "Expression denotes a '" + ExprClassName (expr.ExprClass) +
			       "' where an " + expected + " was expected");
		}
	}

	// <summary>
	//   This is just a base class for expressions that can
	//   appear on statements (invocations, object creation,
	//   assignments, post/pre increment and decrement).  The idea
	//   being that they would support an extra Emition interface that
	//   does not leave a result on the stack.
	// </summary>

	public abstract class ExpressionStatement : Expression {

		// <summary>
		//   Requests the expression to be emitted in a `statement'
		//   context.  This means that no new value is left on the
		//   stack after invoking this method (constrasted with
		//   Emit that will always leave a value on the stack).
		// </summary>
		public abstract void EmitStatement (EmitContext ec);
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

		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);
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

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			ec.ig.Emit (OpCodes.Box, child.Type);
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

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			ec.ig.Emit (op);

			if (second_valid)
				ec.ig.Emit (op2);
		}			
		
	}

	// <summary>
	//   Unary expressions.  
	// </summary>
	//
	// <remarks>
	//   Unary implements unary expressions.   It derives from
	//   ExpressionStatement becuase the pre/post increment/decrement
	//   operators can be used in a statement context.
	// </remarks>
	public class Unary : ExpressionStatement {
		public enum Operator {
			Addition, Subtraction, Negate, BitComplement,
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
			case Operator.Addition:
				return "+";
			case Operator.Subtraction:
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

		Expression ForceConversion (TypeContainer tc, Expression expr, Type target_type)
		{
			if (expr.Type == target_type)
				return expr;

			return ConvertImplicit (tc, expr, target_type);
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

			mg = MemberLookup (tc, expr_type, op_name, false);
			
			if (mg != null) {
				Arguments = new ArrayList ();
				Arguments.Add (new Argument (expr, Argument.AType.Expression));
				
				method = Invocation.OverloadResolve (tc, (MethodGroupExpr) mg, Arguments, location);
				if (method != null) {
					MethodInfo mi = (MethodInfo) method;

					type = mi.ReturnType;
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
				}
				
				type = TypeManager.bool_type;
				return this;
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

			if (oper == Operator.Addition) {
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
			if (oper == Operator.Subtraction){
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
					expr = ConvertImplicit (tc, expr, type);
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

				e = ConvertImplicit (tc, expr, TypeManager.int32_type);
				if (e != null){
					expr = e;
					type = e.Type;
					return this;
				} 

				e = ConvertImplicit (tc, expr, TypeManager.int64_type);
				if (e != null){
					expr = e;
					type = e.Type;
					return this;
				}

				e = ConvertImplicit (tc, expr, TypeManager.double_type);
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

			if (oper == Operator.AddressOf){
				if (expr.ExprClass != ExprClass.Variable){
					Error (tc, 211, "Cannot take the address of non-variables");
					return null;
				}
				type = Type.GetType (expr.Type.ToString () + "*");
			}
			
			Error (tc, 187, "No such operator '" + OperName () + "' defined for type '" +
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

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Type expr_type = expr.Type;

			if (method != null) {

				// Note that operators are static anyway
				
				if (Arguments != null) 
					Invocation.EmitArguments (ec, method, Arguments);

				//
				// Post increment/decrement operations need a copy at this
				// point.
				//
				if (oper == Operator.PostDecrement || oper == Operator.PostIncrement)
					ig.Emit (OpCodes.Dup);
				

				ig.Emit (OpCodes.Call, (MethodInfo) method);

				//
				// Pre Increment and Decrement operators
				//
				if (oper == Operator.PreIncrement || oper == Operator.PreDecrement){
					ig.Emit (OpCodes.Dup);
				}
				
				//
				// Increment and Decrement should store the result
				//
				if (oper == Operator.PreDecrement || oper == Operator.PreIncrement ||
				    oper == Operator.PostDecrement || oper == Operator.PostIncrement){
					((LValue) expr).Store (ec);
				}
				return;
			}
			
			switch (oper) {
			case Operator.Addition:
				throw new Exception ("This should be caught by Resolve");
				
			case Operator.Subtraction:
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
				((LValue)expr).AddressOf (ec);
				break;
				
			case Operator.Indirection:
				throw new Exception ("Not implemented yet");
				
			case Operator.PreIncrement:
			case Operator.PreDecrement:
				if (expr.ExprClass == ExprClass.Variable){
					//
					// Resolve already verified that it is an "incrementable"
					// 
					expr.Emit (ec);
					ig.Emit (OpCodes.Ldc_I4_1);
					
					if (oper == Operator.PreDecrement)
						ig.Emit (OpCodes.Sub);
					else
						ig.Emit (OpCodes.Add);
					ig.Emit (OpCodes.Dup);
					((LValue) expr).Store (ec);
				} else {
					throw new Exception ("Handle Indexers and Properties here");
				}
				break;
				
			case Operator.PostIncrement:
			case Operator.PostDecrement:
				if (expr.ExprClass == ExprClass.Variable){
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
					((LValue) expr).Store (ec);
				} else {
					throw new Exception ("Handle Indexers and Properties here");
				}
				break;
				
			default:
				throw new Exception ("This should not happen: Operator = "
						     + oper.ToString ());
			}
		}
		

		public override void EmitStatement (EmitContext ec)
		{
			//
			// FIXME: we should rewrite this code to generate
			// better code for ++ and -- as we know we wont need
			// the values on the stack
			//
			Emit (ec);
			ec.ig.Emit (OpCodes.Pop);
		}
	}
	
	public class Probe : Expression {
		public readonly string ProbeType;
		public readonly Operator Oper;
		Expression expr;
		Type probe_type;
		
		public enum Operator {
			Is, As
		}
		
		public Probe (Operator oper, Expression expr, string probe_type)
		{
			Oper = oper;
			ProbeType = probe_type;
			this.expr = expr;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}
		
		public override Expression Resolve (TypeContainer tc)
		{
			probe_type = tc.LookupType (ProbeType, false);

			if (probe_type == null)
				return null;

			expr = expr.Resolve (tc);
			
			type = TypeManager.bool_type;
			eclass = ExprClass.Value;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			expr.Emit (ec);
			
			if (Oper == Operator.Is){
				ec.ig.Emit (OpCodes.Isinst, probe_type);
			} else {
				throw new Exception ("Implement as");
			}
		}
	}

	// <summary>
	//   This represents a typecast in the source language.
	//
	//   FIXME: Cast expressions have an unusual set of parsing
	//   rules, we need to figure those out.
	// </summary>
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

			expr = ConvertExplicit (tc, expr, type);
			
			return expr;
		}

		public override void Emit (EmitContext ec)
		{
			//
			// This one will never happen
			//
			throw new Exception ("Should not happen");
		}
	}

	public class Binary : Expression {
		public enum Operator {
			Multiply, Division, Modulus,
			Addition, Subtraction,
			LeftShift, RightShift,
			LessThan, GreaterThan, LessThanOrEqual, GreaterThanOrEqual, 
			Equality, Inequality,
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
			case Operator.Division:
				return "/";
			case Operator.Modulus:
				return "%";
			case Operator.Addition:
				return "+";
			case Operator.Subtraction:
				return "-";
			case Operator.LeftShift:
				return "<<";
			case Operator.RightShift:
				return ">>";
			case Operator.LessThan:
				return "<";
			case Operator.GreaterThan:
				return ">";
			case Operator.LessThanOrEqual:
				return "<=";
			case Operator.GreaterThanOrEqual:
				return ">=";
			case Operator.Equality:
				return "==";
			case Operator.Inequality:
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

		Expression ForceConversion (TypeContainer tc, Expression expr, Type target_type)
		{
			if (expr.Type == target_type)
				return expr;

			return ConvertImplicit (tc, expr, target_type);
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
					right = ConvertImplicit (tc, right, TypeManager.double_type);
				if (l != TypeManager.double_type)
					left = ConvertImplicit (tc, left, TypeManager.double_type);
				
				type = TypeManager.double_type;
			} else if (l == TypeManager.float_type || r == TypeManager.float_type){
				//
				// if either operand is of type float, th eother operand is
				// converd to type float.
				//
				if (r != TypeManager.double_type)
					right = ConvertImplicit (tc, right, TypeManager.float_type);
				if (l != TypeManager.double_type)
					left = ConvertImplicit (tc, left, TypeManager.float_type);
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
					
					Error (tc, 34, "Operator `" + OperName ()
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
					left = ConvertImplicit (tc, left, TypeManager.int64_type);
				if (r != TypeManager.int64_type)
					right = ConvertImplicit (tc, right, TypeManager.int64_type);

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
					left = ForceConversion (tc, left, TypeManager.int64_type);
					right = ForceConversion (tc, right, TypeManager.int64_type);
					type = TypeManager.int64_type;
				} else {
					//
					// if either operand is of type uint, the other
					// operand is converd to type uint
					//
					left = ForceConversion (tc, left, TypeManager.uint32_type);
					right = ForceConversion (tc, left, TypeManager.uint32_type);
					type = TypeManager.uint32_type;
				} 
			} else if (l == TypeManager.decimal_type || r == TypeManager.decimal_type){
				if (l != TypeManager.decimal_type)
					left = ConvertImplicit (tc, left, TypeManager.decimal_type);
				if (r != TypeManager.decimal_type)
					right = ConvertImplicit (tc, right, TypeManager.decimal_type);

				type = TypeManager.decimal_type;
			} else {
				left = ForceConversion (tc, left, TypeManager.int32_type);
				right = ForceConversion (tc, right, TypeManager.int32_type);
				type = TypeManager.int32_type;
			}
		}

		void error19 (TypeContainer tc)
		{
			Error (tc, 19,
			       "Operator " + OperName () + " cannot be applied to operands of type `" +
			       TypeManager.CSharpName (left.Type) + "' and `" +
			       TypeManager.CSharpName (right.Type) + "'");
						     
		}
		
		Expression CheckShiftArguments (TypeContainer tc)
		{
			Expression e;
			Type l = left.Type;
			Type r = right.Type;

			e = ForceConversion (tc, right, TypeManager.int32_type);
			if (e == null){
				error19 (tc);
				return null;
			}
			right = e;

			if (((e = ConvertImplicit (tc, left, TypeManager.int32_type)) != null) ||
			    ((e = ConvertImplicit (tc, left, TypeManager.uint32_type)) != null) ||
			    ((e = ConvertImplicit (tc, left, TypeManager.int64_type)) != null) ||
			    ((e = ConvertImplicit (tc, left, TypeManager.uint64_type)) != null)){
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

			left_expr = MemberLookup (tc, l, op, false);

			right_expr = MemberLookup (tc, r, op, false);

			MethodGroupExpr union = Invocation.MakeUnionSet (left_expr, right_expr);

			Arguments = new ArrayList ();
			Arguments.Add (new Argument (left, Argument.AType.Expression));
			Arguments.Add (new Argument (right, Argument.AType.Expression));
			
			if (union != null) {
				method = Invocation.OverloadResolve (tc, union, Arguments, location);
				if (method != null) {
					MethodInfo mi = (MethodInfo) method;
					
					type = mi.ReturnType;
					return this;
				}
			}	
			
			//
			// Step 2: Default operations on CLI native types.
			//
			
			// Only perform numeric promotions on:
			// +, -, *, /, %, &, |, ^, ==, !=, <, >, <=, >=
			//
			if (oper == Operator.LeftShift || oper == Operator.RightShift){
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

			if (oper == Operator.Equality ||
			    oper == Operator.Inequality ||
			    oper == Operator.LessThanOrEqual ||
			    oper == Operator.LessThan ||
			    oper == Operator.GreaterThanOrEqual ||
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
			if (oper == Operator.Equality ||
			    oper == Operator.Inequality ||
			    oper == Operator.LessThan ||
			    oper == Operator.GreaterThan ||
			    oper == Operator.LessThanOrEqual ||
			    oper == Operator.GreaterThanOrEqual){
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
			case Operator.Equality:
				if (close_target)
					opcode = OpCodes.Beq_S;
				else
					opcode = OpCodes.Beq;
				break;

			case Operator.Inequality:
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

			case Operator.LessThanOrEqual:
				if (close_target)
					opcode = OpCodes.Ble_S;
				else
					opcode = OpCodes.Ble;
				break;

			case Operator.GreaterThanOrEqual:
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
		
		public override void Emit (EmitContext ec)
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

				return;
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

			case Operator.Division:
				if (l == TypeManager.uint32_type || l == TypeManager.uint64_type)
					opcode = OpCodes.Div_Un;
				else
					opcode = OpCodes.Div;
				break;

			case Operator.Modulus:
				if (l == TypeManager.uint32_type || l == TypeManager.uint64_type)
					opcode = OpCodes.Rem_Un;
				else
					opcode = OpCodes.Rem;
				break;

			case Operator.Addition:
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

			case Operator.Subtraction:
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

			case Operator.RightShift:
				opcode = OpCodes.Shr;
				break;
				
			case Operator.LeftShift:
				opcode = OpCodes.Shl;
				break;

			case Operator.Equality:
				opcode = OpCodes.Ceq;
				break;

			case Operator.Inequality:
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

			case Operator.LessThanOrEqual:
				ec.ig.Emit (OpCodes.Cgt);
				ec.ig.Emit (OpCodes.Ldc_I4_0);
				
				opcode = OpCodes.Ceq;
				break;

			case Operator.GreaterThanOrEqual:
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

		public override void Emit (EmitContext ec)
		{
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

			e = MemberLookup (tc, tc.TypeBuilder, Name, true);
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

			Error (tc, 103, Location, "The name `" + Name + "' does not exist in the class `" +
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

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("SimpleNames should be gone from the tree");
		}
	}

	// <summary>
	//   A simple interface that should be implemeneted by LValues
	// </summary>
	public interface LValue {

		// <summary>
		//   The Store method should store the contents of the top
		//   of the stack into the storage that is implemented by
		//   the particular implementation of LValue
		// </summary>
		void Store     (EmitContext ec);

		// <summary>
		//   The AddressOf method should generate code that loads
		//   the address of the LValue and leaves it on the stack
		// </summary>
		void AddressOf (EmitContext ec);
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

		public override void Emit (EmitContext ec)
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
				if (idx <= 255)
					ig.Emit (OpCodes.Ldloc_S, (byte) idx);
				else
					ig.Emit (OpCodes.Ldloc, idx);
				break;
			}
		}

		public void Store (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
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
				if (idx <= 255)
					ig.Emit (OpCodes.Stloc_S, (byte) idx);
				else
					ig.Emit (OpCodes.Stloc, idx);
				break;
			}
		}

		public void AddressOf (EmitContext ec)
		{
			VariableInfo vi = VariableInfo;
			int idx = vi.Idx;
			
			if (idx <= 255)
				ec.ig.Emit (OpCodes.Ldloca_S, (byte) idx);
			else
				ec.ig.Emit (OpCodes.Ldloca, idx);
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

		public override void Emit (EmitContext ec)
		{
			if (Idx <= 255)
				ec.ig.Emit (OpCodes.Ldarg_S, (byte) Idx);
			else
				ec.ig.Emit (OpCodes.Ldarg, Idx);
		}

		public void Store (EmitContext ec)
		{
			if (Idx <= 255)
				ec.ig.Emit (OpCodes.Starg_S, (byte) Idx);
			else
				ec.ig.Emit (OpCodes.Starg, Idx);
			
		}

		public void AddressOf (EmitContext ec)
		{
			if (Idx <= 255)
				ec.ig.Emit (OpCodes.Ldarga_S, (byte) Idx);
			else
				ec.ig.Emit (OpCodes.Ldarga, Idx);
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

		public void Emit (EmitContext ec)
		{
			expr.Emit (ec);
		}
	}

	// <summary>
	//   Invocation of methods or delegates.
	// </summary>
	public class Invocation : ExpressionStatement {
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

				if (t == TypeManager.uint64_type)
					if (ll.Value > 0)
						return 1;
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

		static bool ConversionExists (TypeContainer tc, Type from, Type to)
		{
			// Locate user-defined implicit operators

			Expression mg;
			
			mg = MemberLookup (tc, to, "op_Implicit", false);

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

			mg = MemberLookup (tc, from, "op_Implicit", false);

			if (mg != null) {
				MethodGroupExpr me = (MethodGroupExpr) mg;

				for (int i = me.Methods.Length; i > 0;) {
					i--;
					MethodBase mb = me.Methods [i];
					MethodInfo mi = (MethodInfo) mb;
					
					if (mi.ReturnType == to)
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
		static int BetterConversion (TypeContainer tc, Argument a, Type p, Type q)
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

			if (q == null) {

				Expression tmp;

				tmp = ConvertImplicit (tc, argument_expr, p);

				if (tmp != null)
					return 1;
				else
					return 0;

			}
			
			if (ConversionExists (tc, p, q) == true &&
			    ConversionExists (tc, q, p) == false)
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
		static int BetterFunction (TypeContainer tc, ArrayList args, MethodBase candidate, MethodBase best)
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
						
						x = BetterConversion (tc, a, candidate_pd.ParameterType (j), null);
						
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

					x = BetterConversion (tc, a, candidate_pd.ParameterType (j),
							      best_pd.ParameterType (j));
					y = BetterConversion (tc, a, best_pd.ParameterType (j),
							      candidate_pd.ParameterType (j));
					
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

		public static MethodGroupExpr MakeUnionSet (Expression mg1, Expression mg2)
		{

			if (mg1 != null || mg2 != null) {
					
				MethodGroupExpr left_set = null, right_set = null;
				int length1 = 0, length2 = 0;
				
				if (mg1 != null) {
					left_set = (MethodGroupExpr) mg1;
					length1 = left_set.Methods.Length;
				}
				
				if (mg2 != null) {
					right_set = (MethodGroupExpr) mg2;
					length2 = right_set.Methods.Length;
				}
				
				MemberInfo [] miset = new MemberInfo [length1 + length2];
				if (left_set != null)
					left_set.Methods.CopyTo (miset, 0);
				if (right_set != null)
					right_set.Methods.CopyTo (miset, length1);
				
				MethodGroupExpr union = new MethodGroupExpr (miset);

				return union;
				
			}

			return null;

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
		public static MethodBase OverloadResolve (TypeContainer tc, MethodGroupExpr me,
							  ArrayList Arguments, Location loc)
		{
			ArrayList afm = new ArrayList ();
			int best_match_idx = -1;
			MethodBase method = null;
			int argument_count;
			
			for (int i = me.Methods.Length; i > 0; ){
				i--;
				MethodBase candidate  = me.Methods [i];
				int x;

				x = BetterFunction (tc, Arguments, candidate, method);
				
				if (x == 0)
					continue;
				else {
					best_match_idx = i;
					method = me.Methods [best_match_idx];
				}
			}

			if (Arguments == null)
				argument_count = 0;
			else
				argument_count = Arguments.Count;
			
			ParameterData pd;
			
			// Now we see if we can at least find a method with the same number of arguments
			// and then try doing implicit conversion on the arguments
			if (best_match_idx == -1) {
				
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

			}

			if (method == null)
				return null;

			// And now convert implicitly, each argument to the required type
			
			pd = GetParameterData (method);

			for (int j = argument_count; j > 0;) {
				j--;
				Argument a = (Argument) Arguments [j];
				Expression a_expr = a.Expr;
				
				Expression conv = ConvertImplicit (tc, a_expr, pd.ParameterType (j));

				if (conv == null) {
					Error (tc, 1502, loc,
					       "The best overloaded match for method '" + FullMethodDesc (method) +
					       "' has some invalid arguments");
					Error (tc, 1503, loc,
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

			method = OverloadResolve (tc, (MethodGroupExpr) this.expr, Arguments, Location);

			if (method == null){
				Error (tc, -6, Location,
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

				a.Emit (ec);
			}
		}
		
		public override void Emit (EmitContext ec)
		{
			bool is_static = method.IsStatic;

			if (!is_static){
				MethodGroupExpr mg = (MethodGroupExpr) this.expr;

				//
				// If this is ourselves, push "this"
				//
				if (mg.InstanceExpression == null){
					ec.ig.Emit (OpCodes.Ldarg_0);
				} else {
					//
					// Push the instance expression
					//
					mg.InstanceExpression.Emit (ec);
				}
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
		}

		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec);

			// 
			// Pop the return value if there is one
			//
			if (method is MethodInfo){
				if (((MethodInfo)method).ReturnType != TypeManager.void_type)
					ec.ig.Emit (OpCodes.Pop);
			}
		}
	}

	public class New : ExpressionStatement {

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

			ml = MemberLookup (tc, type, ".ctor", false,
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

			method = Invocation.OverloadResolve (tc, (MethodGroupExpr) ml, Arguments, Location);

			if (method == null) {
				Error (tc, -6, Location,
				       "New invocation: Can not find a constructor for this argument list");
				return null;
			}
			
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Invocation.EmitArguments (ec, method, Arguments);
			ec.ig.Emit (OpCodes.Newobj, (ConstructorInfo) method);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec);
			ec.ig.Emit (OpCodes.Pop);
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

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldarg_0);
		}

		public void Store (EmitContext ec)
		{
			//
			// Assignment to the "this" variable.
			//
			// FIXME: Apparently this is a bug that we
			// must catch as `this' seems to be readonly ;-)
			//
			ec.ig.Emit (OpCodes.Starg, 0);
		}

		public void AddressOf (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldarga_S, (byte) 0);
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

		public override void Emit (EmitContext ec)
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

		public override void Emit (EmitContext ec)
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

			member_lookup = MemberLookup (tc, expr.Type, Identifier, false);

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

		public override void Emit (EmitContext ec)
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

		public override void Emit (EmitContext ec)
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

		override public void Emit (EmitContext ec)
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

		override public void Emit (EmitContext ec)
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

		public override void Emit (EmitContext ec)
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

		override public void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (FieldInfo.IsStatic)
				ig.Emit (OpCodes.Ldsfld, FieldInfo);
			else {
				Instance.Emit (ec);
				
				ig.Emit (OpCodes.Ldfld, FieldInfo);
			}
		}

		public void Store (EmitContext ec)
		{
			if (FieldInfo.IsStatic)
				ec.ig.Emit (OpCodes.Stsfld, FieldInfo);
			else
				ec.ig.Emit (OpCodes.Stfld, FieldInfo);
		}

		public void AddressOf (EmitContext ec)
		{
			if (FieldInfo.IsStatic)
				ec.ig.Emit (OpCodes.Ldsflda, FieldInfo);
			else {
				Instance.Emit (ec);
				ec.ig.Emit (OpCodes.Ldflda, FieldInfo);
			}
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

		override public void Emit (EmitContext ec)
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

		override public void Emit (EmitContext ec)
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

		public override void Emit (EmitContext ec)
		{
			bool last_check = ec.CheckState;
			
			ec.CheckState = true;
			Expr.Emit (ec);
			ec.CheckState = last_check;
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

		public override void Emit (EmitContext ec)
		{
			bool last_check = ec.CheckState;
			
			ec.CheckState = false;
			Expr.Emit (ec);
			ec.CheckState = last_check;
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
		
		public override void Emit (EmitContext ec)
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

		public override void Emit (EmitContext ec)
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

			Expression mg1, mg2;
			MethodGroupExpr union;
			MethodInfo mi;
			
			mg1 = MemberLookup (tc, source.Type, "op_Implicit", false);
			mg2 = MemberLookup (tc, target, "op_Implicit", false);
			
			union = Invocation.MakeUnionSet (mg1, mg2);

			arguments = new ArrayList ();
			arguments.Add (new Argument (source, Argument.AType.Expression));
			
			if (union != null) {
				method = Invocation.OverloadResolve (tc, union, arguments,
								     new Location ("FIXME", 1, 1));
				
				if (method != null) {
					mi = (MethodInfo) method;

					if (mi.ReturnType == target) {
						type = mi.ReturnType;
						return this;
					}
				}
			}
			
			if (target == TypeManager.bool_type) {

				mg1 = MemberLookup (tc, source.Type, "op_True", false);
				mg2 = MemberLookup (tc, target, "op_True", false);
				
				union = Invocation.MakeUnionSet (mg1, mg2);

				if (union == null)
					return null;
				
				method = Invocation.OverloadResolve (tc, union, arguments,
								     new Location ("FIXME", 1, 1));
				
				if (method != null) {
					mi = (MethodInfo) method;

					if (mi.ReturnType == target) {
						type = mi.ReturnType;
						return this;
					}
				}
			}

			return null;
		}

		public static bool CanConvert (TypeContainer tc, Expression source, Type target)
		{
			source = source.Resolve (tc);
			
			if (source == null)
				return false;

			Expression mg1, mg2;
			MethodBase method;
			ArrayList arguments;
			
			mg1 = MemberLookup (tc, source.Type, "op_Implicit", false);
			mg2 = MemberLookup (tc, target, "op_Implicit", false);
			
			MethodGroupExpr union = Invocation.MakeUnionSet (mg1, mg2);

			arguments = new ArrayList ();
			arguments.Add (new Argument (source, Argument.AType.Expression));
			
			if (union != null) {
				
				method = Invocation.OverloadResolve (tc, union, arguments,
								     new Location ("FIXME", 1, 1));

				if (method != null) {
					MethodInfo mi = (MethodInfo) method;
					
					if (mi.ReturnType == target)
						return true;
				}
			}
			
			// If we have a boolean type, we need to check for the True
			// and False operators too.
			
			if (target == TypeManager.bool_type) {

				mg1 = MemberLookup (tc, source.Type, "op_True", false);
				mg2 = MemberLookup (tc, target, "op_True", false);
				
				union = Invocation.MakeUnionSet (mg1, mg2);

				if (union == null)
					return false;

				method = Invocation.OverloadResolve (tc, union, arguments,
								     new Location ("FIXME", 1, 1));
				if (method != null) {
					MethodInfo mi = (MethodInfo) method;

					if (mi.ReturnType == target) 
						return true;
				}
			}
			
			return false;
			
		}
		
		public override void Emit (EmitContext ec)
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

				return;
			}

			throw new Exception ("Implement me");
		}

	}
}
