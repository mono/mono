//
// ecore.cs: Core of the Expression representation for the intermediate tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
//

namespace Mono.MonoBASIC {
	using System;
	using System.Collections;
	using System.Diagnostics;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;

	/// <remarks>
	///   The ExprClass class contains the is used to pass the 
	///   classification of an expression (value, variable, namespace,
	///   type, method group, property access, event access, indexer access,
	///   nothing).
	/// </remarks>
	public enum ExprClass : byte {
		Invalid,
		
		Value,
		Variable,
		Namespace,
		Type,
		MethodGroup,
		PropertyAccess,
		EventAccess,
		IndexerAccess,
		Nothing, 
	}

	/// <remarks>
	///   This is used to tell Resolve in which types of expressions we're
	///   interested.
	/// </remarks>
	[Flags]
	public enum ResolveFlags {
		// Returns Value, Variable, PropertyAccess, EventAccess or IndexerAccess.
		VariableOrValue		= 1,

		// Returns a type expression.
		Type			= 2,

		// Returns a method group.
		MethodGroup		= 4,

		// Allows SimpleNames to be returned.
		// This is used by MemberAccess to construct long names that can not be
		// partially resolved (namespace-qualified names for example).
		SimpleName		= 8,

		// Mask of all the expression class flags.
		MaskExprClass		= 15,

		// Disable control flow analysis while resolving the expression.
		// This is used when resolving the instance expression of a field expression.
		DisableFlowAnalysis	= 16
	}

	//
	// This is just as a hint to AddressOf of what will be done with the
	// address.
	[Flags]
	public enum AddressOp {
		Store = 1,
		Load  = 2,
		LoadStore = 3
	};
	
	/// <summary>
	///   This interface is implemented by variables
	/// </summary>
	public interface IMemoryLocation {
		/// <summary>
		///   The AddressOf method should generate code that loads
		///   the address of the object and leaves it on the stack.
		///
		///   The 'mode' argument is used to notify the expression
		///   of whether this will be used to read from the address or
		///   write to the address.
		///
		///   This is just a hint that can be used to provide good error
		///   reporting, and should have no other side effects. 
		/// </summary>
		void AddressOf (EmitContext ec, AddressOp mode);
	}

	/// <summary>
	///   This interface is implemented by variables
	/// </summary>
	public interface IVariable {
		/// <summary>
		///   Checks whether the variable has already been assigned at
		///   the current position of the method's control flow and
		///   reports an appropriate error message if not.
		///
		///   If the variable is a struct, then this call checks whether
		///   all of its fields (including all private ones) have been
		///   assigned.
		/// </summary>
		bool IsAssigned (EmitContext ec, Location loc);

		/// <summary>
		///   Checks whether field 'name' in this struct has been assigned.
		/// </summary>
		bool IsFieldAssigned (EmitContext ec, string name, Location loc);

		/// <summary>
		///   Tells the flow analysis code that the variable has already
		///   been assigned at the current code position.
		///
		///   If the variable is a struct, this call marks all its fields
		///   (including private fields) as being assigned.
		/// </summary>
		void SetAssigned (EmitContext ec);

		/// <summary>
		///   Tells the flow analysis code that field 'name' in this struct
		///   has already been assigned atthe current code position.
		/// </summary>
		void SetFieldAssigned (EmitContext ec, string name);
	}

	/// <summary>
	///   This interface denotes an expression which evaluates to a member
	///   of a struct or a class.
	/// </summary>
	public interface IMemberExpr
	{
		/// <summary>
		///   The name of this member.
		/// </summary>
		string Name {
			get;
		}

		/// <summary>
		///   Whether this is an instance member.
		/// </summary>
		bool IsInstance {
			get;
		}

		/// <summary>
		///   Whether this is a static member.
		/// </summary>
		bool IsStatic {
			get;
		}

		/// <summary>
		///   The type which declares this member.
		/// </summary>
		Type DeclaringType {
			get;
		}

		/// <summary>
		///   The instance expression associated with this member, if it's a
		///   non-static member.
		/// </summary>
		Expression InstanceExpression {
			get; set;
		}
	}

	/// <summary>
	///   Expression which resolves to a type.
	/// </summary>
	public interface ITypeExpression
	{
		/// <summary>
		///   Resolve the expression, but only lookup types.
		/// </summary>
		Expression DoResolveType (EmitContext ec);
	}

	/// <remarks>
	///   Base class for expressions
	/// </remarks>
	public abstract class Expression {
		public ExprClass eclass;
		protected Type type;
		protected Location loc;
		
		public Type Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}

		public Location Location {
			get {
				return loc;
			}
		}

		/// <summary>
		///   Utility wrapper routine for Error, just to beautify the code
		/// </summary>
		public void Error (int error, string s)
		{
			if (!Location.IsNull (loc))
				Report.Error (error, loc, s);
			else
				Report.Error (error, s);
		}

		/// <summary>
		///   Utility wrapper routine for Warning, just to beautify the code
		/// </summary>
		public void Warning (int warning, string s)
		{
			if (!Location.IsNull (loc))
				Report.Warning (warning, loc, s);
			else
				Report.Warning (warning, s);
		}

		/// <summary>
		///   Utility wrapper routine for Warning, only prints the warning if
		///   warnings of level 'level' are enabled.
		/// </summary>
		public void Warning (int warning, int level, string s)
		{
			if (level <= RootContext.WarningLevel)
				Warning (warning, s);
		}

		static public void Error_CannotConvertType (Location loc, Type source, Type target)
		{
			Report.Error (30, loc, "Cannot convert type '" +
				      TypeManager.MonoBASIC_Name (source) + "' to '" +
				      TypeManager.MonoBASIC_Name (target) + "'");
		}

		/// <summary>
		///   Performs semantic analysis on the Expression
		/// </summary>
		///
		/// <remarks>
		///   The Resolve method is invoked to perform the semantic analysis
		///   on the node.
		///
		///   The return value is an expression (it can be the
		///   same expression in some cases) or a new
		///   expression that better represents this node.
		///   
		///   For example, optimizations of Unary (LiteralInt)
		///   would return a new LiteralInt with a negated
		///   value.
		///   
		///   If there is an error during semantic analysis,
		///   then an error should be reported (using Report)
		///   and a null value should be returned.
		///   
		///   There are two side effects expected from calling
		///   Resolve(): the the field variable "eclass" should
		///   be set to any value of the enumeration
		///   'ExprClass' and the type variable should be set
		///   to a valid type (this is the type of the
		///   expression).
		/// </remarks>
		public abstract Expression DoResolve (EmitContext ec);

		public virtual Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			return DoResolve (ec);
		}
		
		/// <summary>
		///   Resolves an expression and performs semantic analysis on it.
		/// </summary>
		///
		/// <remarks>
		///   Currently Resolve wraps DoResolve to perform sanity
		///   checking and assertion checking on what we expect from Resolve.
		/// </remarks>
		public Expression Resolve (EmitContext ec, ResolveFlags flags)
		{
			// Are we doing a types-only search ?
			if ((flags & ResolveFlags.MaskExprClass) == ResolveFlags.Type) {
				ITypeExpression type_expr = this as ITypeExpression;

				if (type_expr == null)
					return null;

				return type_expr.DoResolveType (ec);
			}

			bool old_do_flow_analysis = ec.DoFlowAnalysis;
			if ((flags & ResolveFlags.DisableFlowAnalysis) != 0)
				ec.DoFlowAnalysis = false;
			
			Expression e;
			try {
				if (this is SimpleName)
					e = ((SimpleName) this).DoResolveAllowStatic (ec);
				else 
					e = DoResolve (ec);
			} finally {
				ec.DoFlowAnalysis = old_do_flow_analysis;
			}

			if (e == null)
				return null;

			if (e is SimpleName){
				SimpleName s = (SimpleName) e;

				if ((flags & ResolveFlags.SimpleName) == 0) {

					object lookup = TypeManager.MemberLookup (
						ec.ContainerType, ec.ContainerType, AllMemberTypes,
						AllBindingFlags | BindingFlags.NonPublic, s.Name);
					if (lookup != null)
						Error (30390, "'" + s.Name + "' " +
						       "is inaccessible because of its protection level");
					else
						Error (30451, "The name '" + s.Name + "' could not be " +
						       "found in '" + ec.DeclSpace.Name + "'");
					return null;
				}

				return s;
			}
			
			if ((e is TypeExpr) || (e is ComposedCast)) {
				if ((flags & ResolveFlags.Type) == 0) {
					e.Error118 (flags);
					return null;
				}

				return e;
			}

			switch (e.eclass) {
			case ExprClass.Type:
				if ((flags & ResolveFlags.VariableOrValue) == 0) {
					e.Error118 (flags);
					return null;
				}
				break;

			case ExprClass.MethodGroup:
				if ((flags & ResolveFlags.MethodGroup) == 0) {
					MethodGroupExpr mg = (MethodGroupExpr) e;
					Invocation i = new Invocation (mg, new ArrayList(), Location.Null);
					Expression te = i.Resolve(ec);
					//((MethodGroupExpr) e).ReportUsageError ();
					//return null;
					return te;
				}
				break;

			case ExprClass.Value:
			case ExprClass.Variable:
			case ExprClass.PropertyAccess:
			case ExprClass.EventAccess:
			case ExprClass.IndexerAccess:
				if ((flags & ResolveFlags.VariableOrValue) == 0) {
					e.Error118 (flags);
					return null;
				}
				break;

			default:
				throw new Exception ("Expression " + e.GetType () +
						     " ExprClass is Invalid after resolve");
			}

			if (e.type == null)
				throw new Exception (
					"Expression " + e.GetType () +
					" did not set its type after Resolve\n" +
					"called from: " + this.GetType ());

			return e;
		}

		/// <summary>
		///   Resolves an expression and performs semantic analysis on it.
		/// </summary>
		public Expression Resolve (EmitContext ec)
		{
			return Resolve (ec, ResolveFlags.VariableOrValue);
		}

		/// <summary>
		///   Resolves an expression for LValue assignment
		/// </summary>
		///
		/// <remarks>
		///   Currently ResolveLValue wraps DoResolveLValue to perform sanity
		///   checking and assertion checking on what we expect from Resolve
		/// </remarks>
		public Expression ResolveLValue (EmitContext ec, Expression right_side)
		{
			Expression e = DoResolveLValue (ec, right_side);

			if (e != null){
				if (e is SimpleName){
					SimpleName s = (SimpleName) e;

					Report.Error (
						30451, loc,
						"The name '" + s.Name + "' could not be found in '" +
						ec.DeclSpace.Name + "'");
					return null;
				}

				if (e.eclass == ExprClass.Invalid)
					throw new Exception ("Expression " + e +
							     " ExprClass is Invalid after resolve");

				if (e.eclass == ExprClass.MethodGroup) {
					MethodGroupExpr mg = (MethodGroupExpr) e;
					Invocation i = new Invocation (mg, new ArrayList(), Location.Null);
					Expression te = i.Resolve(ec);
					return te;
					//((MethodGroupExpr) e).ReportUsageError ();
					//return null;
				}

				if (e.type == null)
					throw new Exception ("Expression " + e +
							     " did not set its type after Resolve");
			}

			return e;
		}
		
		/// <summary>
		///   Emits the code for the expression
		/// </summary>
		///
		/// <remarks>
		///   The Emit method is invoked to generate the code
		///   for the expression.  
		/// </remarks>
		public abstract void Emit (EmitContext ec);

		/// <summary>
		///   Protected constructor.  Only derivate types should
		///   be able to be created
		/// </summary>

		protected Expression ()
		{
			eclass = ExprClass.Invalid;
			type = null;
		}

		/// <summary>
		///   Returns a literalized version of a literal FieldInfo
		/// </summary>
		///
		/// <remarks>
		///   The possible return values are:
		///      IntConstant, UIntConstant
		///      LongLiteral, ULongConstant
		///      FloatConstant, DoubleConstant
		///      StringConstant
		///
		///   The value returned is already resolved.
		/// </remarks>
		public static Constant Constantify (object v, Type t)
		{
			if (t == TypeManager.int32_type)
				return new IntConstant ((int) v);
			else if (t == TypeManager.uint32_type)
				return new UIntConstant ((uint) v);
			else if (t == TypeManager.int64_type)
				return new LongConstant ((long) v);
			else if (t == TypeManager.uint64_type)
				return new ULongConstant ((ulong) v);
			else if (t == TypeManager.float_type)
				return new FloatConstant ((float) v);
			else if (t == TypeManager.double_type)
				return new DoubleConstant ((double) v);
			else if (t == TypeManager.string_type)
				return new StringConstant ((string) v);
			else if (t == TypeManager.short_type)
				return new ShortConstant ((short)v);
			else if (t == TypeManager.ushort_type)
				return new UShortConstant ((ushort)v);
			else if (t == TypeManager.sbyte_type)
				return new SByteConstant (((sbyte)v));
			else if (t == TypeManager.byte_type)
				return new ByteConstant ((byte)v);
			else if (t == TypeManager.char_type)
				return new CharConstant ((char)v);
			else if (t == TypeManager.bool_type)
				return new BoolConstant ((bool) v);
			else if (TypeManager.IsEnumType (t)){
				Constant e = Constantify (v, TypeManager.TypeToCoreType (v.GetType ()));

				return new EnumConstant (e, t);
			} else
				throw new Exception ("Unknown type for constant (" + t +
						     "), details: " + v);
		}

		/// <summary>
		///   Returns a fully formed expression after a MemberLookup
		/// </summary>
		public static Expression ExprClassFromMemberInfo (EmitContext ec, MemberInfo mi, Location loc)
		{
			if (mi is EventInfo)
				return new EventExpr ((EventInfo) mi, loc);
			else if (mi is FieldInfo)
				return new FieldExpr ((FieldInfo) mi, loc);
			else if (mi is PropertyInfo)
				return new PropertyExpr (ec, (PropertyInfo) mi, loc);
		        else if (mi is Type){
				return new TypeExpr ((System.Type) mi, loc);
			}

			return null;
		}

		//
		// FIXME: Probably implement a cache for (t,name,current_access_set)?
		//
		// This code could use some optimizations, but we need to do some
		// measurements.  For example, we could use a delegate to 'flag' when
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
		// FIXME: When calling MemberLookup inside an 'Invocation', we should pass
		// the arguments here and have MemberLookup return only the methods that
		// match the argument count/type, unlike we are doing now (we delay this
		// decision).
		//
		// This is so we can catch correctly attempts to invoke instance methods
		// from a static body (scan for error 120 in ResolveSimpleName).
		//
		//
		// FIXME: Potential optimization, have a static ArrayList
		//

		public static Expression MemberLookup (EmitContext ec, Type t, string name,
						       MemberTypes mt, BindingFlags bf, Location loc)
		{
			return MemberLookup (ec, ec.ContainerType, t, name, mt, bf, loc);
		}

		//
		// Lookup type 't' for code in class 'invocation_type'.  Note that it's important
		// to set 'invocation_type' correctly since this method also checks whether the
		// invoking class is allowed to access the member in class 't'.  When you want to
		// explicitly do a lookup in the base class, you must set both 't' and 'invocation_type'
		// to the base class (although a derived class can access protected members of its base
		// class it cannot do so through an instance of the base class (error CS1540)).
		// 

		public static Expression MemberLookup (EmitContext ec, Type invocation_type, Type t,
						       string name, MemberTypes mt, BindingFlags bf,
						       Location loc)
		{
			MemberInfo [] mi = TypeManager.MemberLookup (invocation_type, t, mt, bf, name);

			if (mi == null)
				return null;

			int count = mi.Length;

			if (count > 1)
				return new MethodGroupExpr (mi, loc);

			if (mi [0] is MethodBase)
				return new MethodGroupExpr (mi, loc);

			return ExprClassFromMemberInfo (ec, mi [0], loc);
		}

		public const MemberTypes AllMemberTypes =
			MemberTypes.Constructor |
			MemberTypes.Event       |
			MemberTypes.Field       |
			MemberTypes.Method      |
			MemberTypes.NestedType  |
			MemberTypes.Property;
		
		public const BindingFlags AllBindingFlags =
			BindingFlags.Public |
			BindingFlags.Static |
			BindingFlags.Instance |
			BindingFlags.IgnoreCase;

		public static Expression MemberLookup (EmitContext ec, Type t, string name, Location loc)
		{
			return MemberLookup (ec, ec.ContainerType, t, name, AllMemberTypes, AllBindingFlags, loc);
		}

		public static Expression MethodLookup (EmitContext ec, Type t, string name, Location loc)
		{
			return MemberLookup (ec, ec.ContainerType, t, name,
					     MemberTypes.Method, AllBindingFlags, loc);
		}

		/// <summary>
		///   This is a wrapper for MemberLookup that is not used to "probe", but
		///   to find a final definition.  If the final definition is not found, we
		///   look for private members and display a useful debugging message if we
		///   find it.
		/// </summary>
		public static Expression MemberLookupFinal (EmitContext ec, Type t, string name, 
							    Location loc)
		{
			return MemberLookupFinal (ec, t, name, MemberTypes.Method, AllBindingFlags, loc);
		}

		public static Expression MemberLookupFinal (EmitContext ec, Type t, string name,
							    MemberTypes mt, BindingFlags bf, Location loc)
		{
			Expression e;

			int errors = Report.Errors;

			e = MemberLookup (ec, ec.ContainerType, t, name, mt, bf, loc);

			if (e != null)
				return e;

			// Error has already been reported.
			if (errors < Report.Errors)
				return null;
			
			e = MemberLookup (ec, t, name, AllMemberTypes,
					  AllBindingFlags | BindingFlags.NonPublic, loc);
			if (e == null){
				Report.Error (
					30456, loc, "'" + t + "' does not contain a definition " +
					"for '" + name + "'");
			} else {
					Report.Error (
						30390, loc, "'" + t + "." + name +
						"' is inaccessible due to its protection level");
			}
			
			return null;
		}

		static public MemberInfo GetFieldFromEvent (EventExpr event_expr)
		{
			EventInfo ei = event_expr.EventInfo;

			return TypeManager.GetPrivateFieldOfEvent (ei);
		}
		
		static EmptyExpression MyEmptyExpr;
		static public Expression ImplicitReferenceConversion (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (expr_type == null && expr.eclass == ExprClass.MethodGroup){
				// if we are a method group, emit a warning

				expr.Emit (null);
			}

			//
			// notice that it is possible to write "ValueType v = 1", the ValueType here
			// is an abstract class, and not really a value type, so we apply the same rules.
			//
			if (target_type == TypeManager.object_type || target_type == TypeManager.value_type) {
				//
				// A pointer type cannot be converted to object
				// 
				if (expr_type.IsPointer)
					return null;

				if (expr_type.IsValueType)
					return new BoxedCast (expr);
				if (expr_type.IsClass || expr_type.IsInterface)
					return new EmptyCast (expr, target_type);
			} else if (expr_type.IsSubclassOf (target_type)) {
				//
				// Special case: enumeration to System.Enum.
				// System.Enum is not a value type, it is a class, so we need
				// a boxing conversion
				//
				if (expr_type.IsEnum)
					return new BoxedCast (expr);
			
				return new EmptyCast (expr, target_type);
			} else {

				// This code is kind of mirrored inside StandardConversionExists
				// with the small distinction that we only probe there
				//
				// Always ensure that the code here and there is in sync
				
				// from the null type to any reference-type.
				if (expr is NullLiteral && !target_type.IsValueType)
					return new EmptyCast (expr, target_type);

				// from any class-type S to any interface-type T.
				if (target_type.IsInterface) {
					if (TypeManager.ImplementsInterface (expr_type, target_type)){
						if (expr_type.IsClass)
							return new EmptyCast (expr, target_type);
						else if (expr_type.IsValueType)
							return new BoxedCast (expr);
					}
				}

				// from any interface type S to interface-type T.
				if (expr_type.IsInterface && target_type.IsInterface) {
					if (TypeManager.ImplementsInterface (expr_type, target_type))
						return new EmptyCast (expr, target_type);
					else
						return null;
				}
				
				// from an array-type S to an array-type of type T
				if (expr_type.IsArray && target_type.IsArray) {
					if (expr_type.GetArrayRank () == target_type.GetArrayRank ()) {

						Type expr_element_type = expr_type.GetElementType ();

						if (MyEmptyExpr == null)
							MyEmptyExpr = new EmptyExpression ();
						
						MyEmptyExpr.SetType (expr_element_type);
						Type target_element_type = target_type.GetElementType ();

						if (!expr_element_type.IsValueType && !target_element_type.IsValueType)
							if (StandardConversionExists (MyEmptyExpr,
										      target_element_type))
								return new EmptyCast (expr, target_type);
					}
				}
				
				
				// from an array-type to System.Array
				if (expr_type.IsArray && target_type == TypeManager.array_type)
					return new EmptyCast (expr, target_type);
				
				// from any delegate type to System.Delegate
				if (expr_type.IsSubclassOf (TypeManager.delegate_type) &&
				    target_type == TypeManager.delegate_type)
					return new EmptyCast (expr, target_type);
					
				// from any array-type or delegate type into System.ICloneable.
				if (expr_type.IsArray || expr_type.IsSubclassOf (TypeManager.delegate_type))
					if (target_type == TypeManager.icloneable_type)
						return new EmptyCast (expr, target_type);
				
				return null;

			}
			
			return null;
		}

		/// <summary>
		///   Implicit Numeric Conversions.
		///
		///   expr is the expression to convert, returns a new expression of type
		///   target_type or null if an implicit conversion is not possible.
		/// </summary>
		static public Expression ImplicitNumericConversion (EmitContext ec, Expression expr,
								    Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			
			//
			// Attempt to do the implicit constant expression conversions

			if (expr is IntConstant){
				Expression e;
				
				e = TryImplicitIntConversion (target_type, (IntConstant) expr);

				if (e != null)
					return e;
			} else if (expr is LongConstant && target_type == TypeManager.uint64_type){
				//
				// Try the implicit constant expression conversion
				// from long to ulong, instead of a nice routine,
				// we just inline it
				//
				long v = ((LongConstant) expr).Value;
				if (v > 0)
					return new ULongConstant ((ulong) v);
			}

 			Type real_target_type = target_type;

			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double.
				//
				if (real_target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double
				// 
				if ((real_target_type == TypeManager.short_type) ||
				    (real_target_type == TypeManager.ushort_type) ||
				    (real_target_type == TypeManager.int32_type) ||
				    (real_target_type == TypeManager.uint32_type))
					return new EmptyCast (expr, target_type);

				if (real_target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, float, double
				// 
				if (real_target_type == TypeManager.int32_type)
					return new EmptyCast (expr, target_type);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double
				//
				if (real_target_type == TypeManager.uint32_type)
					return new EmptyCast (expr, target_type);

				if (real_target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, float, double
				//
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double
				//
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R4);
			} else if (expr_type == TypeManager.int64_type){
				//
				// From long/ulong to float, double
				//
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);	
			} else if (expr_type == TypeManager.uint64_type){
				//
				// From ulong to float, double
				//
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R4);	
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, long, ulong, float, double
				// 
				if ((real_target_type == TypeManager.ushort_type) ||
				    (real_target_type == TypeManager.int32_type) ||
				    (real_target_type == TypeManager.uint32_type))
					return new EmptyCast (expr, target_type);
				if (real_target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
			}

			return null;
		}

		//
		// Tests whether an implicit reference conversion exists between expr_type
		// and target_type
		//
		public static bool ImplicitReferenceConversionExists (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;
			
			//
			// This is the boxed case.
			//
			if (target_type == TypeManager.object_type) {
				if ((expr_type.IsClass) ||
				    (expr_type.IsValueType) ||
				    (expr_type.IsInterface))
					return true;
				
			} else if (expr_type.IsSubclassOf (target_type)) {
				return true;
			} else {
				// Please remember that all code below actually comes
				// from ImplicitReferenceConversion so make sure code remains in sync
				
				// from any class-type S to any interface-type T.
				if (target_type.IsInterface) {
					if (TypeManager.ImplementsInterface (expr_type, target_type))
						return true;
				}
				
				// from any interface type S to interface-type T.
				if (expr_type.IsInterface && target_type.IsInterface)
					if (TypeManager.ImplementsInterface (expr_type, target_type))
						return true;
				
				// from an array-type S to an array-type of type T
				if (expr_type.IsArray && target_type.IsArray) {
					if (expr_type.GetArrayRank () == target_type.GetArrayRank ()) {
						
						Type expr_element_type = expr_type.GetElementType ();

						if (MyEmptyExpr == null)
							MyEmptyExpr = new EmptyExpression ();
						
						MyEmptyExpr.SetType (expr_element_type);
						Type target_element_type = target_type.GetElementType ();
						
						if (!expr_element_type.IsValueType && !target_element_type.IsValueType)
							if (StandardConversionExists (MyEmptyExpr,
										      target_element_type))
								return true;
					}
				}
				
				// from an array-type to System.Array
				if (expr_type.IsArray && (target_type == TypeManager.array_type))
					return true;
				
				// from any delegate type to System.Delegate
				if (expr_type.IsSubclassOf (TypeManager.delegate_type) &&
				    target_type == TypeManager.delegate_type)
					if (target_type.IsAssignableFrom (expr_type))
						return true;
					
				// from any array-type or delegate type into System.ICloneable.
				if (expr_type.IsArray || expr_type.IsSubclassOf (TypeManager.delegate_type))
					if (target_type == TypeManager.icloneable_type)
						return true;
				
				// from the null type to any reference-type.
				if (expr is NullLiteral && !target_type.IsValueType &&
				    !TypeManager.IsEnumType (target_type))
					return true;
				
			}

			return false;
		}

		/// <summary>
		///  Same as StandardConversionExists except that it also looks at
		///  implicit user defined conversions - needed for overload resolution
		/// </summary>
		public static bool ImplicitConversionExists (EmitContext ec, Expression expr, Type target_type)
		{
			if (StandardConversionExists (expr, target_type) == true)
				return true;

			Expression dummy = ImplicitUserConversion (ec, expr, target_type, Location.Null);

			if (dummy != null)
				return true;

			return false;
		}

		/// <summary>
		///  Determines if a standard implicit conversion exists from
		///  expr_type to target_type
		/// </summary>
		public static bool StandardConversionExists (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (expr_type == null || expr_type == TypeManager.void_type)
				return false;
			
			if (expr_type == target_type)
				return true;

			// Conversions from enum to underlying type are widening.
			if (expr_type.IsSubclassOf (TypeManager.enum_type))
				expr_type = TypeManager.EnumToUnderlying (expr_type);

			if (expr_type == target_type)
				return true;

			// First numeric conversions 

			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double.
				//
				if ((target_type == TypeManager.int32_type) || 
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type)  ||
				    (target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
	
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, float, double
				// 
				if ((target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double
				//
				if ((target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, float, double
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if ((expr_type == TypeManager.uint64_type) ||
				   (expr_type == TypeManager.int64_type)) {
				//
				// From long/ulong to float, double
				//
				if ((target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;

			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (target_type == TypeManager.double_type)
					return true;
			}	
			
			if (ImplicitReferenceConversionExists (expr, target_type))
				return true;
			
			if (expr is IntConstant){
				int value = ((IntConstant) expr).Value;

				if (target_type == TypeManager.sbyte_type){
					if (value >= SByte.MinValue && value <= SByte.MaxValue)
						return true;
				} else if (target_type == TypeManager.byte_type){
					if (Byte.MinValue >= 0 && value <= Byte.MaxValue)
						return true;
				} else if (target_type == TypeManager.short_type){
					if (value >= Int16.MinValue && value <= Int16.MaxValue)
						return true;
				} else if (target_type == TypeManager.ushort_type){
					if (value >= UInt16.MinValue && value <= UInt16.MaxValue)
						return true;
				} else if (target_type == TypeManager.uint32_type){
					if (value >= 0)
						return true;
				} else if (target_type == TypeManager.uint64_type){
					 //
					 // we can optimize this case: a positive int32
					 // always fits on a uint64.  But we need an opcode
					 // to do it.
					 //
					if (value >= 0)
						return true;
				}
				
				if (value == 0 && expr is IntLiteral && TypeManager.IsEnumType (target_type))
					return true;
			}

			if (expr is LongConstant && target_type == TypeManager.uint64_type){
				//
				// Try the implicit constant expression conversion
				// from long to ulong, instead of a nice routine,
				// we just inline it
				//
				long v = ((LongConstant) expr).Value;
				if (v > 0)
					return true;
			}
			
			if (target_type.IsSubclassOf (TypeManager.enum_type) && expr is IntLiteral){
				IntLiteral i = (IntLiteral) expr;

				if (i.Value == 0)
					return true;
			}

			if (target_type == TypeManager.void_ptr_type && expr_type.IsPointer)
				return true;

			return false;
		}

		//
		// Used internally by FindMostEncompassedType, this is used
		// to avoid creating lots of objects in the tight loop inside
		// FindMostEncompassedType
		//
		static EmptyExpression priv_fmet_param;
		
		/// <summary>
		///  Finds "most encompassed type" according to the spec (13.4.2)
		///  amongst the methods in the MethodGroupExpr
		/// </summary>
		static Type FindMostEncompassedType (ArrayList types)
		{
			Type best = null;

			if (priv_fmet_param == null)
				priv_fmet_param = new EmptyExpression ();

			foreach (Type t in types){
				priv_fmet_param.SetType (t);
				
				if (best == null) {
					best = t;
					continue;
				}
				
				if (StandardConversionExists (priv_fmet_param, best))
					best = t;
			}

			return best;
		}

		//
		// Used internally by FindMostEncompassingType, this is used
		// to avoid creating lots of objects in the tight loop inside
		// FindMostEncompassingType
		//
		static EmptyExpression priv_fmee_ret;
		
		/// <summary>
		///  Finds "most encompassing type" according to the spec (13.4.2)
		///  amongst the types in the given set
		/// </summary>
		static Type FindMostEncompassingType (ArrayList types)
		{
			Type best = null;

			if (priv_fmee_ret == null)
				priv_fmee_ret = new EmptyExpression ();

			foreach (Type t in types){
				priv_fmee_ret.SetType (best);

				if (best == null) {
					best = t;
					continue;
				}

				if (StandardConversionExists (priv_fmee_ret, t))
					best = t;
			}
			
			return best;
		}

		//
		// Used to avoid creating too many objects
		//
		static EmptyExpression priv_fms_expr;
		
		/// <summary>
		///   Finds the most specific source Sx according to the rules of the spec (13.4.4)
		///   by making use of FindMostEncomp* methods. Applies the correct rules separately
		///   for explicit and implicit conversion operators.
		/// </summary>
		static public Type FindMostSpecificSource (MethodGroupExpr me, Expression source,
							   bool apply_explicit_conv_rules,
							   Location loc)
		{
			ArrayList src_types_set = new ArrayList ();
			
			if (priv_fms_expr == null)
				priv_fms_expr = new EmptyExpression ();

			//
			// If any operator converts from S then Sx = S
			//
			Type source_type = source.Type;
			foreach (MethodBase mb in me.Methods){
				ParameterData pd = Invocation.GetParameterData (mb);
				Type param_type = pd.ParameterType (0);

				if (param_type == source_type)
					return param_type;

				if (apply_explicit_conv_rules) {
					//
					// From the spec :
					// Find the set of applicable user-defined conversion operators, U.  This set
					// consists of the
					// user-defined implicit or explicit conversion operators declared by
					// the classes or structs in D that convert from a type encompassing
					// or encompassed by S to a type encompassing or encompassed by T
					//
					priv_fms_expr.SetType (param_type);
					if (StandardConversionExists (priv_fms_expr, source_type))
						src_types_set.Add (param_type);
					else {
						if (StandardConversionExists (source, param_type))
							src_types_set.Add (param_type);
					}
				} else {
					//
					// Only if S is encompassed by param_type
					//
					if (StandardConversionExists (source, param_type))
						src_types_set.Add (param_type);
				}
			}
			
			//
			// Explicit Conv rules
			//
			if (apply_explicit_conv_rules) {
				ArrayList candidate_set = new ArrayList ();

				foreach (Type param_type in src_types_set){
					if (StandardConversionExists (source, param_type))
						candidate_set.Add (param_type);
				}

				if (candidate_set.Count != 0)
					return FindMostEncompassedType (candidate_set);
			}

			//
			// Final case
			//
			if (apply_explicit_conv_rules)
				return FindMostEncompassingType (src_types_set);
			else
				return FindMostEncompassedType (src_types_set);
		}

		//
		// Useful in avoiding proliferation of objects
		//
		static EmptyExpression priv_fmt_expr;
		
		/// <summary>
		///  Finds the most specific target Tx according to section 13.4.4
		/// </summary>
		static public Type FindMostSpecificTarget (MethodGroupExpr me, Type target,
							   bool apply_explicit_conv_rules,
							   Location loc)
		{
			ArrayList tgt_types_set = new ArrayList ();
			
			if (priv_fmt_expr == null)
				priv_fmt_expr = new EmptyExpression ();
			
			//
			// If any operator converts to T then Tx = T
			//
			foreach (MethodInfo mi in me.Methods){
				Type ret_type = mi.ReturnType;

				if (ret_type == target)
					return ret_type;

				if (apply_explicit_conv_rules) {
					//
					// From the spec :
					// Find the set of applicable user-defined conversion operators, U.
					//
					// This set consists of the
					// user-defined implicit or explicit conversion operators declared by
					// the classes or structs in D that convert from a type encompassing
					// or encompassed by S to a type encompassing or encompassed by T
					//
					priv_fms_expr.SetType (ret_type);
					if (StandardConversionExists (priv_fms_expr, target))
						tgt_types_set.Add (ret_type);
					else {
						priv_fms_expr.SetType (target);
						if (StandardConversionExists (priv_fms_expr, ret_type))
							tgt_types_set.Add (ret_type);
					}
				} else {
					//
					// Only if T is encompassed by param_type
					//
					priv_fms_expr.SetType (ret_type);
					if (StandardConversionExists (priv_fms_expr, target))
						tgt_types_set.Add (ret_type);
				}
			}

			//
			// Explicit conv rules
			//
			if (apply_explicit_conv_rules) {
				ArrayList candidate_set = new ArrayList ();

				foreach (Type ret_type in tgt_types_set){
					priv_fmt_expr.SetType (ret_type);
					
					if (StandardConversionExists (priv_fmt_expr, target))
						candidate_set.Add (ret_type);
				}

				if (candidate_set.Count != 0)
					return FindMostEncompassingType (candidate_set);
			}
			
			//
			// Okay, final case !
			//
			if (apply_explicit_conv_rules)
				return FindMostEncompassedType (tgt_types_set);
			else 
				return FindMostEncompassingType (tgt_types_set);
		}
		
		/// <summary>
		///  User-defined Implicit conversions
		/// </summary>
		static public Expression ImplicitUserConversion (EmitContext ec, Expression source,
								 Type target, Location loc)
		{
			return UserDefinedConversion (ec, source, target, loc, false);
		}

		/// <summary>
		///  User-defined Explicit conversions
		/// </summary>
		static public Expression ExplicitUserConversion (EmitContext ec, Expression source,
								 Type target, Location loc)
		{
			return UserDefinedConversion (ec, source, target, loc, true);
		}

		/// <summary>
		///   Computes the MethodGroup for the user-defined conversion
		///   operators from source_type to target_type.  'look_for_explicit'
		///   controls whether we should also include the list of explicit
		///   operators
		/// </summary>
		static MethodGroupExpr GetConversionOperators (EmitContext ec,
							       Type source_type, Type target_type,
							       Location loc, bool look_for_explicit)
		{
			Expression mg1 = null, mg2 = null;
			Expression mg5 = null, mg6 = null, mg7 = null, mg8 = null;
			string op_name;

			//
			// FIXME : How does the False operator come into the picture ?
			// This doesn't look complete and very correct !
			//
			if (target_type == TypeManager.bool_type && !look_for_explicit)
				op_name = "op_True";
			else
				op_name = "op_Implicit";

			MethodGroupExpr union3;
			
			mg1 = MethodLookup (ec, source_type, op_name, loc);
			if (source_type.BaseType != null)
				mg2 = MethodLookup (ec, source_type.BaseType, op_name, loc);

			if (mg1 == null)
				union3 = (MethodGroupExpr) mg2;
			else if (mg2 == null)
				union3 = (MethodGroupExpr) mg1;
			else
				union3 = Invocation.MakeUnionSet (mg1, mg2, loc);

			mg1 = MethodLookup (ec, target_type, op_name, loc);
			if (mg1 != null){
				if (union3 != null)
					union3 = Invocation.MakeUnionSet (union3, mg1, loc);
				else
					union3 = (MethodGroupExpr) mg1;
			}

			if (target_type.BaseType != null)
				mg1 = MethodLookup (ec, target_type.BaseType, op_name, loc);
			
			if (mg1 != null){
				if (union3 != null)
					union3 = Invocation.MakeUnionSet (union3, mg1, loc);
				else
					union3 = (MethodGroupExpr) mg1;
			}

			MethodGroupExpr union4 = null;

			if (look_for_explicit) {
				op_name = "op_Explicit";

				mg5 = MemberLookup (ec, source_type, op_name, loc);
				if (source_type.BaseType != null)
					mg6 = MethodLookup (ec, source_type.BaseType, op_name, loc);
				
				mg7 = MemberLookup (ec, target_type, op_name, loc);
				if (target_type.BaseType != null)
					mg8 = MethodLookup (ec, target_type.BaseType, op_name, loc);
				
				MethodGroupExpr union5 = Invocation.MakeUnionSet (mg5, mg6, loc);
				MethodGroupExpr union6 = Invocation.MakeUnionSet (mg7, mg8, loc);

				union4 = Invocation.MakeUnionSet (union5, union6, loc);
			}
			
			return Invocation.MakeUnionSet (union3, union4, loc);
		}
		
		/// <summary>
		///   User-defined conversions
		/// </summary>
		static public Expression UserDefinedConversion (EmitContext ec, Expression source,
								Type target, Location loc,
								bool look_for_explicit)
		{
			MethodGroupExpr union;
			Type source_type = source.Type;
			MethodBase method = null;
			
			union = GetConversionOperators (ec, source_type, target, loc, look_for_explicit);
			if (union == null)
				return null;
			
			Type most_specific_source, most_specific_target;

#if BLAH
			foreach (MethodBase m in union.Methods){
				Console.WriteLine ("Name: " + m.Name);
				Console.WriteLine ("    : " + ((MethodInfo)m).ReturnType);
			}
#endif
			
			most_specific_source = FindMostSpecificSource (union, source, look_for_explicit, loc);
			if (most_specific_source == null)
				return null;

			most_specific_target = FindMostSpecificTarget (union, target, look_for_explicit, loc);
			if (most_specific_target == null) 
				return null;

			int count = 0;

			foreach (MethodBase mb in union.Methods){
				ParameterData pd = Invocation.GetParameterData (mb);
				MethodInfo mi = (MethodInfo) mb;
				
				if (pd.ParameterType (0) == most_specific_source &&
				    mi.ReturnType == most_specific_target) {
					method = mb;
					count++;
				}
			}
			
			if (method == null || count > 1)
				return null;
			
			
			//
			// This will do the conversion to the best match that we
			// found.  Now we need to perform an implict standard conversion
			// if the best match was not the type that we were requested
			// by target.
			//
			if (look_for_explicit)
				source = ConvertExplicitStandard (ec, source, most_specific_source, loc);
			else
				source = ConvertImplicitStandard (ec, source, most_specific_source, loc);

			if (source == null)
				return null;

			Expression e;
			e =  new UserCast ((MethodInfo) method, source, loc);
			if (e.Type != target){
				if (!look_for_explicit)
					e = ConvertImplicitStandard (ec, e, target, loc);
				else
					e = ConvertExplicitStandard (ec, e, target, loc);
			} 
			return e;
		}
		
		/// <summary>
		///   Converts implicitly the resolved expression 'expr' into the
		///   'target_type'.  It returns a new expression that can be used
		///   in a context that expects a 'target_type'. 
		/// </summary>
		static public Expression ConvertImplicit (EmitContext ec, Expression expr,
							  Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			Expression e;

			if (expr_type == target_type)
				return expr;

			if (target_type == null)
				throw new Exception ("Target type is null");

			e = ConvertImplicitStandard (ec, expr, target_type, loc);
			if (e != null)
				return e;

			e = ImplicitUserConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;
				
			e = RuntimeConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;				

			return null;
		}

		/// <summary>
		///   Converts the resolved expression 'expr' into the
		///   'target_type' using the Microsoft.VisualBasic runtime.
		///   It returns a new expression that can be used
		///   in a context that expects a 'target_type'. 
		/// </summary>
		static private Expression RTConversionExpression (EmitContext ec, string s, Expression expr, Location loc)
		{
			Expression etmp, e;
			ArrayList args;
			Argument arg;
			
		 	etmp = Mono.MonoBASIC.Parser.DecomposeQI("Microsoft.VisualBasic.CompilerServices." + s, loc);
		 	args = new ArrayList();
		 	arg = new Argument (expr, Argument.AType.Expression);
		 	args.Add (arg);
			e = (Expression) new Invocation (etmp, args, loc);
			e = e.Resolve(ec);	
			return (e);		
		}
		
		static public bool RuntimeConversionExists (EmitContext ec, Expression expr, Type target_type)
		{
			return (RuntimeConversion (ec, expr, target_type,Location.Null)) != null;	
		}
		
		static public Expression RuntimeConversion (EmitContext ec, Expression expr,
								Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			TypeCode dest_type = Type.GetTypeCode (target_type);
			TypeCode src_type = Type.GetTypeCode (expr_type);
			Expression e = null;

			// VB.NET Objects can be converted to anything by default
			// unless, that is, an exception at runtime blows it all
			if (src_type == TypeCode.Object) {
				Expression cast_type = Mono.MonoBASIC.Parser.DecomposeQI(target_type.ToString(), loc);
				Cast ce = new Cast (cast_type, expr, loc);
				ce.IsRuntimeCast = true;
				return ce.Resolve (ec);
			}

			switch (dest_type) {
				case TypeCode.String:
					switch (src_type) {
						case TypeCode.SByte:						
						case TypeCode.Byte:
							e = RTConversionExpression(ec, "StringType.FromByte", expr, loc);
							break;	
						case TypeCode.UInt16:
						case TypeCode.Int16:
							e = RTConversionExpression(ec, "StringType.FromShort", expr, loc);
							break;		
						case TypeCode.UInt32:					
						case TypeCode.Int32:
							e = RTConversionExpression(ec, "StringType.FromInteger", expr, loc);
							break;							
						case TypeCode.UInt64:	
						case TypeCode.Int64:
							e = RTConversionExpression(ec, "StringType.FromLong", expr, loc);
							break;							
						case TypeCode.Char:
							e = RTConversionExpression(ec, "StringType.FromChar", expr, loc);
							break;								
						case TypeCode.Single:
							e = RTConversionExpression(ec, "StringType.FromSingle", expr, loc);
							break;		
						case TypeCode.Double:
							e = RTConversionExpression(ec, "StringType.FromDouble", expr, loc);
							break;																			
						case TypeCode.Boolean:
							e = RTConversionExpression(ec, "StringType.FromBoolean", expr, loc);
							break;	
						case TypeCode.DateTime:
							e = RTConversionExpression(ec, "StringType.FromDate", expr, loc);
							break;		
						case TypeCode.Decimal:
							e = RTConversionExpression(ec, "StringType.FromDecimal", expr, loc);
							break;		
						case TypeCode.Object:
							e = RTConversionExpression(ec, "StringType.FromObject", expr, loc);
							break;																												
					}
					break;
					
				case TypeCode.Int32:
				case TypeCode.UInt32:	
					switch (src_type) {						
						case TypeCode.String:				
							e = RTConversionExpression(ec, "IntegerType.FromString", expr, loc);
							break;		
						case TypeCode.Object:				
							e = RTConversionExpression(ec, "IntegerType.FromObject", expr, loc);
							break;											
					}
					break;	

				case TypeCode.Int16:
				case TypeCode.UInt16:	
					switch (src_type) {						
						case TypeCode.String:				
							e = RTConversionExpression(ec, "ShortType.FromString", expr, loc);
							break;		
						case TypeCode.Object:				
							e = RTConversionExpression(ec, "ShortType.FromObject", expr, loc);
							break;											
					}
					break;	
				case TypeCode.Byte:
					// Ok, this *is* broken
					e = RTConversionExpression(ec, "ByteType.FromObject", expr, loc);
					break;																			
			}
			
			// We must examine separately some types that
			// don't have a TypeCode but are supported 
			// in the runtime
			if (expr_type == typeof(System.String) && target_type == typeof (System.Char[])) {
				e = RTConversionExpression(ec, "CharArrayType.FromString", expr, loc);
			}
			
			return e;
		}
										  		
		/// <summary>
		///   Attempts to apply the 'Standard Implicit
		///   Conversion' rules to the expression 'expr' into
		///   the 'target_type'.  It returns a new expression
		///   that can be used in a context that expects a
		///   'target_type'.
		///
		///   This is different from 'ConvertImplicit' in that the
		///   user defined implicit conversions are excluded. 
		/// </summary>
		static public Expression ConvertImplicitStandard (EmitContext ec, Expression expr,
								  Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			Expression e;

			if (expr_type == target_type)
				return expr;

			e = ImplicitNumericConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;

			e = ImplicitReferenceConversion (expr, target_type);
			if (e != null)
				return e;

			if (expr.Type.IsSubclassOf (TypeManager.enum_type)) {
				expr_type = TypeManager.EnumToUnderlying (expr.Type);
				expr = new EmptyCast (expr, expr_type);
				if (expr_type == target_type)
					return expr;
				e = ImplicitNumericConversion (ec, expr, target_type, loc);
				if (e != null)
					return e;
			}

			if (ec.InUnsafe) {
				if (expr_type.IsPointer){
					if (target_type == TypeManager.void_ptr_type)
						return new EmptyCast (expr, target_type);

					//
					// yep, comparing pointer types cant be done with
					// t1 == t2, we have to compare their element types.
					//
					if (target_type.IsPointer){
						if (target_type.GetElementType()==expr_type.GetElementType())
							return expr;
					}
				}
				
				if (target_type.IsPointer){
					if (expr is NullLiteral)
						return new EmptyCast (expr, target_type);
				}
			}

			return null;
		}

		/// <summary>
		///   Attemps to perform an implict constant conversion of the IntConstant
		///   into a different data type using casts (See Implicit Constant
		///   Expression Conversions)
		/// </summary>
		static protected Expression TryImplicitIntConversion (Type target_type, IntConstant ic)
		{
			int value = ic.Value;

			//
			// FIXME: This could return constants instead of EmptyCasts
			//
			if (target_type == TypeManager.sbyte_type){
				if (value >= SByte.MinValue && value <= SByte.MaxValue)
					return new SByteConstant ((sbyte) value);
			} else if (target_type == TypeManager.byte_type){
				if (Byte.MinValue >= 0 && value <= Byte.MaxValue)
					return new ByteConstant ((byte) value);
			} else if (target_type == TypeManager.short_type){
				if (value >= Int16.MinValue && value <= Int16.MaxValue)
					return new ShortConstant ((short) value);
			} else if (target_type == TypeManager.ushort_type){
				if (value >= UInt16.MinValue && value <= UInt16.MaxValue)
					return new UShortConstant ((ushort) value);
			} else if (target_type == TypeManager.uint32_type){
				if (value >= 0)
					return new UIntConstant ((uint) value);
			} else if (target_type == TypeManager.uint64_type){
				//
				// we can optimize this case: a positive int32
				// always fits on a uint64.  But we need an opcode
				// to do it.
				//
				if (value >= 0)
					return new ULongConstant ((ulong) value);
			}
			
			if (value == 0 && ic is IntLiteral && TypeManager.IsEnumType (target_type)){
				Type underlying = TypeManager.EnumToUnderlying (target_type);
				Constant e = (Constant) ic;
				
				//
				// Possibly, we need to create a different 0 literal before passing
				// to EnumConstant
				//n
				if (underlying == TypeManager.int64_type)
					e = new LongLiteral (0);
				else if (underlying == TypeManager.uint64_type)
					e = new ULongLiteral (0);

				return new EnumConstant (e, target_type);
			}
			return null;
		}

		static public void Error_CannotConvertImplicit (Location loc, Type source, Type target)
		{
			string msg = "Cannot convert implicitly from '"+
				TypeManager.MonoBASIC_Name (source) + "' to '" +
				TypeManager.MonoBASIC_Name (target) + "'";

			Report.Error (29, loc, msg);
		}

		/// <summary>
		///   Attemptes to implicityly convert 'target' into 'type', using
		///   ConvertImplicit.  If there is no implicit conversion, then
		///   an error is signaled
		/// </summary>
		static public Expression ConvertImplicitRequired (EmitContext ec, Expression source,
								  Type target_type, Location loc)
		{
			Expression e;
			
			e = ConvertImplicit (ec, source, target_type, loc);
			if (e != null)
				return e;

			if (source is DoubleLiteral && target_type == TypeManager.float_type){
				Report.Error (664, loc,
					      "Double literal cannot be implicitly converted to " +
					      "float type, use F suffix to create a float literal");
			}

			Error_CannotConvertImplicit (loc, source.Type, target_type);

			return null;
		}

		/// <summary>
		///   Performs the explicit numeric conversions
		/// </summary>
		static Expression ConvertNumericExplicit (EmitContext ec, Expression expr, Type target_type, Location loc)
		{
			Type expr_type = expr.Type;

			//
			// If we have an enumeration, extract the underlying type,
			// use this during the comparison, but wrap around the original
			// target_type
			//
			Type real_target_type = target_type;

			if (TypeManager.IsEnumType (real_target_type))
				real_target_type = TypeManager.EnumToUnderlying (real_target_type);

			if (StandardConversionExists (expr, real_target_type)){
 				Expression ce = ConvertImplicitStandard (ec, expr, real_target_type, loc);

				if (real_target_type != target_type)
					return new EmptyCast (ce, target_type);
				return ce;
			}
			
			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to byte, ushort, uint, ulong, char
				//
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_U1);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_U2);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_CH);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to sbyte and char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U1_I1);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U1_CH);
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to sbyte, byte, ushort, uint, ulong, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_U1);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_U2);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_CH);
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to sbyte, byte, short, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U2_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U2_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U2_I2);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U2_CH);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to sbyte, byte, short, ushort, uint, ulong, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_U2);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_CH);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to sbyte, byte, short, ushort, int, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_I4);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_CH);
			} else if (expr_type == TypeManager.int64_type){
				//
				// From long to sbyte, byte, short, ushort, int, uint, ulong, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_CH);
			} else if (expr_type == TypeManager.uint64_type){
				//
				// From ulong to sbyte, byte, short, ushort, int, uint, long, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_U4);
				if (real_target_type == TypeManager.int64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_I8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_CH);
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to sbyte, byte, short
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.CH_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.CH_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.CH_I2);
			} else if (expr_type == TypeManager.float_type){
				//
				// From float to sbyte, byte, short,
				// ushort, int, uint, long, ulong, char
				// or decimal
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_U4);
				if (real_target_type == TypeManager.int64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_I8);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_CH);
			} else if (expr_type == TypeManager.double_type){
				//
				// From double to byte, byte, short,
				// ushort, int, uint, long, ulong,
				// char, float or decimal
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_U4);
				if (real_target_type == TypeManager.int64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_I8);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_CH);
				if (real_target_type == TypeManager.float_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_R4);
			} 

			// decimal is taken care of by the op_Explicit methods.

			return null;
		}

		/// <summary>
		///  Returns whether an explicit reference conversion can be performed
		///  from source_type to target_type
		/// </summary>
		public static bool ExplicitReferenceConversionExists (Type source_type, Type target_type)
		{
			bool target_is_value_type = target_type.IsValueType;
			
			if (source_type == target_type)
				return true;
			
			//
			// From object to any reference type
			//
			if (source_type == TypeManager.object_type && !target_is_value_type)
				return true;
					
			//
			// From any class S to any class-type T, provided S is a base class of T
			//
			if (target_type.IsSubclassOf (source_type))
				return true;

			//
			// From any interface type S to any interface T provided S is not derived from T
			//
			if (source_type.IsInterface && target_type.IsInterface){
				if (!target_type.IsSubclassOf (source_type))
					return true;
			}
			    
			//
			// From any class type S to any interface T, provided S is not sealed
			// and provided S does not implement T.
			//
			if (target_type.IsInterface && !source_type.IsSealed &&
			    !TypeManager.ImplementsInterface (source_type, target_type))
				return true;

			//
			// From any interface-type S to to any class type T, provided T is not
			// sealed, or provided T implements S.
			//
			if (source_type.IsInterface &&
			    (!target_type.IsSealed || TypeManager.ImplementsInterface (target_type, source_type)))
				return true;
			
			
			// From an array type S with an element type Se to an array type T with an 
			// element type Te provided all the following are true:
			//     * S and T differe only in element type, in other words, S and T
			//       have the same number of dimensions.
			//     * Both Se and Te are reference types
			//     * An explicit referenc conversions exist from Se to Te
			//
			if (source_type.IsArray && target_type.IsArray) {
				if (source_type.GetArrayRank () == target_type.GetArrayRank ()) {
					
					Type source_element_type = source_type.GetElementType ();
					Type target_element_type = target_type.GetElementType ();
					
					if (!source_element_type.IsValueType && !target_element_type.IsValueType)
						if (ExplicitReferenceConversionExists (source_element_type,
										       target_element_type))
							return true;
				}
			}
			

			// From System.Array to any array-type
			if (source_type == TypeManager.array_type &&
			    target_type.IsArray){
				return true;
			}

			//
			// From System delegate to any delegate-type
			//
			if (source_type == TypeManager.delegate_type &&
			    target_type.IsSubclassOf (TypeManager.delegate_type))
				return true;

			//
			// From ICloneable to Array or Delegate types
			//
			if (source_type == TypeManager.icloneable_type &&
			    (target_type == TypeManager.array_type ||
			     target_type == TypeManager.delegate_type))
				return true;
			
			return false;
		}

		/// <summary>
		///   Implements Explicit Reference conversions
		/// </summary>
		static Expression ConvertReferenceExplicit (Expression source, Type target_type)
		{
			Type source_type = source.Type;
			bool target_is_value_type = target_type.IsValueType;

			//
			// From object to any reference type
			//
			if (source_type == TypeManager.object_type && !target_is_value_type)
				return new ClassCast (source, target_type);


			//
			// From any class S to any class-type T, provided S is a base class of T
			//
			if (target_type.IsSubclassOf (source_type))
				return new ClassCast (source, target_type);

			//
			// From any interface type S to any interface T provided S is not derived from T
			//
			if (source_type.IsInterface && target_type.IsInterface){
				if (TypeManager.ImplementsInterface (source_type, target_type))
					return null;
				else
					return new ClassCast (source, target_type);
			}
			    
			//
			// From any class type S to any interface T, provides S is not sealed
			// and provided S does not implement T.
			//
			if (target_type.IsInterface && !source_type.IsSealed) {
				if (TypeManager.ImplementsInterface (source_type, target_type))
					return null;
				else
					return new ClassCast (source, target_type);
				
			}

			//
			// From any interface-type S to to any class type T, provided T is not
			// sealed, or provided T implements S.
			//
			if (source_type.IsInterface) {
				if (!target_type.IsSealed || TypeManager.ImplementsInterface (target_type, source_type))
					return new ClassCast (source, target_type);
				else
					return null;
			}
			
			// From an array type S with an element type Se to an array type T with an 
			// element type Te provided all the following are true:
			//     * S and T differe only in element type, in other words, S and T
			//       have the same number of dimensions.
			//     * Both Se and Te are reference types
			//     * An explicit referenc conversions exist from Se to Te
			//
			if (source_type.IsArray && target_type.IsArray) {
				if (source_type.GetArrayRank () == target_type.GetArrayRank ()) {
					
					Type source_element_type = source_type.GetElementType ();
					Type target_element_type = target_type.GetElementType ();
					
					if (!source_element_type.IsValueType && !target_element_type.IsValueType)
						if (ExplicitReferenceConversionExists (source_element_type,
										       target_element_type))
							return new ClassCast (source, target_type);
				}
			}
			

			// From System.Array to any array-type
			if (source_type == TypeManager.array_type &&
			    target_type.IsArray) {
				return new ClassCast (source, target_type);
			}

			//
			// From System delegate to any delegate-type
			//
			if (source_type == TypeManager.delegate_type &&
			    target_type.IsSubclassOf (TypeManager.delegate_type))
				return new ClassCast (source, target_type);

			//
			// From ICloneable to Array or Delegate types
			//
			if (source_type == TypeManager.icloneable_type &&
			    (target_type == TypeManager.array_type ||
			     target_type == TypeManager.delegate_type))
				return new ClassCast (source, target_type);
			
			return null;
		}
		
		/// <summary>
		///   Performs an explicit conversion of the expression 'expr' whose
		///   type is expr.Type to 'target_type'.
		/// </summary>
		static public Expression ConvertExplicit (EmitContext ec, Expression expr,
							  Type target_type, bool runtimeconv, Location loc)
		{
			Type expr_type = expr.Type;
			Expression ne = ConvertImplicitStandard (ec, expr, target_type, loc);

			if (ne != null)
				return ne;

			ne = ConvertNumericExplicit (ec, expr, target_type, loc);
			if (ne != null)
				return ne;

			//
			// Unboxing conversion.
			//
			if (expr_type == TypeManager.object_type && target_type.IsValueType)
				return new UnboxCast (expr, target_type);

			//
			// Enum types
			//
			if (expr_type.IsSubclassOf (TypeManager.enum_type)) {
				Expression e;

				//
				// FIXME: Is there any reason we should have EnumConstant
				// dealt with here instead of just using always the
				// UnderlyingSystemType to wrap the type?
				//
				if (expr is EnumConstant)
					e = ((EnumConstant) expr).Child;
				else {
					e = new EmptyCast (expr, TypeManager.EnumToUnderlying (expr_type));
				}
				
				Expression t = ConvertImplicit (ec, e, target_type, loc);
				if (t != null)
					return t;
				
				t = ConvertNumericExplicit (ec, e, target_type, loc);
				if (t != null)
					return t;
				
				t = RuntimeConversion (ec, e, target_type, loc);
				if (t != null)
					return t;	
								
				Error_CannotConvertType (loc, expr_type, target_type);
				return null;
			}
			
			ne = ConvertReferenceExplicit (expr, target_type);
			if (ne != null)
				return ne;

			if (ec.InUnsafe){
				if (target_type.IsPointer){
					if (expr_type.IsPointer)
						return new EmptyCast (expr, target_type);
					
					if (expr_type == TypeManager.sbyte_type ||
					    expr_type == TypeManager.byte_type ||
					    expr_type == TypeManager.short_type ||
					    expr_type == TypeManager.ushort_type ||
					    expr_type == TypeManager.int32_type ||
					    expr_type == TypeManager.uint32_type ||
					    expr_type == TypeManager.uint64_type ||
					    expr_type == TypeManager.int64_type)
						return new OpcodeCast (expr, target_type, OpCodes.Conv_U);
				}
				if (expr_type.IsPointer){
					if (target_type == TypeManager.sbyte_type ||
					    target_type == TypeManager.byte_type ||
					    target_type == TypeManager.short_type ||
					    target_type == TypeManager.ushort_type ||
					    target_type == TypeManager.int32_type ||
					    target_type == TypeManager.uint32_type ||
					    target_type == TypeManager.uint64_type ||
					    target_type == TypeManager.int64_type){
						Expression e = new EmptyCast (expr, TypeManager.uint32_type);
						Expression ci, ce;

						ci = ConvertImplicitStandard (ec, e, target_type, loc);

						if (ci != null)
							return ci;

						ce = ConvertNumericExplicit (ec, e, target_type, loc);
						if (ce != null)
							return ce;
						//
						// We should always be able to go from an uint32
						// implicitly or explicitly to the other integral
						// types
						//
						throw new Exception ("Internal compiler error");
					}
				}
			}
			
			ne = ExplicitUserConversion (ec, expr, target_type, loc);
			if (ne != null)
				return ne;

			if (!(runtimeconv))	{
				ne = RuntimeConversion (ec, expr, target_type, loc);
				if (ne != null)
					return ne;
				
				Error_CannotConvertType (loc, expr_type, target_type);
			}
			return null;
		}

		/// <summary>
		///   Same as ConvertExplicit, only it doesn't include user defined conversions
		/// </summary>
		static public Expression ConvertExplicitStandard (EmitContext ec, Expression expr,
								  Type target_type, Location l)
		{
			Expression ne = ConvertImplicitStandard (ec, expr, target_type, l);

			if (ne != null)
				return ne;

			ne = ConvertNumericExplicit (ec, expr, target_type, l);
			if (ne != null)
				return ne;

			ne = ConvertReferenceExplicit (expr, target_type);
			if (ne != null)
				return ne;

			ne = RuntimeConversion (ec, expr, target_type, l);
			if (ne != null)
				return ne;				

			Error_CannotConvertType (l, expr.Type, target_type);
			return null;
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
		
		/// <summary>
		///   Reports that we were expecting 'expr' to be of class 'expected'
		/// </summary>
		public void Error118 (string expected)
		{
			string kind = "Unknown";
			
			kind = ExprClassName (eclass);

			Error (118, "Expression denotes a '" + kind +
			       "' where a '" + expected + "' was expected");
		}

		public void Error118 (ResolveFlags flags)
		{
			ArrayList valid = new ArrayList (10);

			if ((flags & ResolveFlags.VariableOrValue) != 0) {
				valid.Add ("variable");
				valid.Add ("value");
			}

			if ((flags & ResolveFlags.Type) != 0)
				valid.Add ("type");

			if ((flags & ResolveFlags.MethodGroup) != 0)
				valid.Add ("method group");

			if ((flags & ResolveFlags.SimpleName) != 0)
				valid.Add ("simple name");

			if (valid.Count == 0)
				valid.Add ("unknown");

			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < valid.Count; i++) {
				if (i > 0)
					sb.Append (", ");
				else if (i == valid.Count)
					sb.Append (" or ");
				sb.Append (valid [i]);
			}

			string kind = ExprClassName (eclass);

			Error (119, "Expression denotes a '" + kind + "' where " +
			       "a '" + sb.ToString () + "' was expected");
		}
		
		static void Error_ConstantValueCannotBeConverted (Location l, string val, Type t)
		{
			Report.Error (31, l, "Constant value '" + val + "' cannot be converted to " +
				      TypeManager.MonoBASIC_Name (t));
		}

		public static void UnsafeError (Location loc)
		{
			Report.Error (214, loc, "Pointers may only be used in an unsafe context");
		}
		
		/// <summary>
		///   Converts the IntConstant, UIntConstant, LongConstant or
		///   ULongConstant into the integral target_type.   Notice
		///   that we do not return an 'Expression' we do return
		///   a boxed integral type.
		///
		///   FIXME: Since I added the new constants, we need to
		///   also support conversions from CharConstant, ByteConstant,
		///   SByteConstant, UShortConstant, ShortConstant
		///
		///   This is used by the switch statement, so the domain
		///   of work is restricted to the literals above, and the
		///   targets are int32, uint32, char, byte, sbyte, ushort,
		///   short, uint64 and int64
		/// </summary>
		public static object ConvertIntLiteral (Constant c, Type target_type, Location loc)
		{
			string s = "";

			if (c.Type == target_type)
				return ((Constant) c).GetValue ();

			//
			// Make into one of the literals we handle, we dont really care
			// about this value as we will just return a few limited types
			// 
			if (c is EnumConstant)
				c = ((EnumConstant)c).WidenToCompilerConstant ();

			if (c is IntConstant){
				int v = ((IntConstant) c).Value;
				
				if (target_type == TypeManager.uint32_type){
					if (v >= 0)
						return (uint) v;
				} else if (target_type == TypeManager.char_type){
					if (v >= Char.MinValue && v <= Char.MaxValue)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v >= Byte.MinValue && v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v >= SByte.MinValue && v <= SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.short_type){
					if (v >= Int16.MinValue && v <= UInt16.MaxValue)
						return (short) v;
				} else if (target_type == TypeManager.ushort_type){
					if (v >= UInt16.MinValue && v <= UInt16.MaxValue)
						return (ushort) v;
				} else if (target_type == TypeManager.int64_type)
					return (long) v;
			        else if (target_type == TypeManager.uint64_type){
					if (v > 0)
						return (ulong) v;
				}

				s = v.ToString ();
			} else if (c is	UIntConstant){
				uint v = ((UIntConstant) c).Value;

				if (target_type == TypeManager.int32_type){
					if (v <= Int32.MaxValue)
						return (int) v;
				} else if (target_type == TypeManager.char_type){
					if (v >= Char.MinValue && v <= Char.MaxValue)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v <= SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.short_type){
					if (v <= UInt16.MaxValue)
						return (short) v;
				} else if (target_type == TypeManager.ushort_type){
					if (v <= UInt16.MaxValue)
						return (ushort) v;
				} else if (target_type == TypeManager.int64_type)
					return (long) v;
			        else if (target_type == TypeManager.uint64_type)
					return (ulong) v;
				s = v.ToString ();
			} else if (c is	LongConstant){ 
				long v = ((LongConstant) c).Value;

				if (target_type == TypeManager.int32_type){
					if (v >= UInt32.MinValue && v <= UInt32.MaxValue)
						return (int) v;
				} else if (target_type == TypeManager.uint32_type){
					if (v >= 0 && v <= UInt32.MaxValue)
						return (uint) v;
				} else if (target_type == TypeManager.char_type){
					if (v >= Char.MinValue && v <= Char.MaxValue)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v >= Byte.MinValue && v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v >= SByte.MinValue && v <= SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.short_type){
					if (v >= Int16.MinValue && v <= UInt16.MaxValue)
						return (short) v;
				} else if (target_type == TypeManager.ushort_type){
					if (v >= UInt16.MinValue && v <= UInt16.MaxValue)
						return (ushort) v;
			        } else if (target_type == TypeManager.uint64_type){
					if (v > 0)
						return (ulong) v;
				}
				s = v.ToString ();
			} else if (c is	ULongConstant){
				ulong v = ((ULongConstant) c).Value;

				if (target_type == TypeManager.int32_type){
					if (v <= Int32.MaxValue)
						return (int) v;
				} else if (target_type == TypeManager.uint32_type){
					if (v <= UInt32.MaxValue)
						return (uint) v;
				} else if (target_type == TypeManager.char_type){
					if (v >= Char.MinValue && v <= Char.MaxValue)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v >= Byte.MinValue && v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v <= (int) SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.short_type){
					if (v <= UInt16.MaxValue)
						return (short) v;
				} else if (target_type == TypeManager.ushort_type){
					if (v <= UInt16.MaxValue)
						return (ushort) v;
			        } else if (target_type == TypeManager.int64_type){
					if (v <= Int64.MaxValue)
						return (long) v;
				}
				s = v.ToString ();
			} else if (c is ByteConstant){
				byte v = ((ByteConstant) c).Value;
				
				if (target_type == TypeManager.int32_type)
					return (int) v;
				else if (target_type == TypeManager.uint32_type)
					return (uint) v;
				else if (target_type == TypeManager.char_type)
					return (char) v;
				else if (target_type == TypeManager.sbyte_type){
					if (v <= SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.short_type)
					return (short) v;
				else if (target_type == TypeManager.ushort_type)
					return (ushort) v;
			        else if (target_type == TypeManager.int64_type)
					return (long) v;
				else if (target_type == TypeManager.uint64_type)
					return (ulong) v;
				s = v.ToString ();
			} else if (c is SByteConstant){
				sbyte v = ((SByteConstant) c).Value;
				
				if (target_type == TypeManager.int32_type)
					return (int) v;
				else if (target_type == TypeManager.uint32_type){
					if (v >= 0)
						return (uint) v;
				} else if (target_type == TypeManager.char_type){
					if (v >= 0)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v >= 0)
						return (byte) v;
				} else if (target_type == TypeManager.short_type)
					return (short) v;
				else if (target_type == TypeManager.ushort_type){
					if (v >= 0)
						return (ushort) v;
			        } else if (target_type == TypeManager.int64_type)
					return (long) v;
				else if (target_type == TypeManager.uint64_type){
					if (v >= 0)
						return (ulong) v;
				}
				s = v.ToString ();
			} else if (c is ShortConstant){
				short v = ((ShortConstant) c).Value;
				
				if (target_type == TypeManager.int32_type){
					return (int) v;
				} else if (target_type == TypeManager.uint32_type){
					if (v >= 0)
						return (uint) v;
				} else if (target_type == TypeManager.char_type){
					if (v >= 0)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v >= Byte.MinValue && v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v >= SByte.MinValue && v <= SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.ushort_type){
					if (v >= 0)
						return (ushort) v;
			        } else if (target_type == TypeManager.int64_type)
					return (long) v;
				else if (target_type == TypeManager.uint64_type)
					return (ulong) v;

				s = v.ToString ();
			} else if (c is UShortConstant){
				ushort v = ((UShortConstant) c).Value;
				
				if (target_type == TypeManager.int32_type)
					return (int) v;
				else if (target_type == TypeManager.uint32_type)
					return (uint) v;
				else if (target_type == TypeManager.char_type){
					if (v >= Char.MinValue && v <= Char.MaxValue)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v >= Byte.MinValue && v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v <= SByte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.short_type){
					if (v <= Int16.MaxValue)
						return (short) v;
			        } else if (target_type == TypeManager.int64_type)
					return (long) v;
				else if (target_type == TypeManager.uint64_type)
					return (ulong) v;

				s = v.ToString ();
			} else if (c is CharConstant){
				char v = ((CharConstant) c).Value;
				
				if (target_type == TypeManager.int32_type)
					return (int) v;
				else if (target_type == TypeManager.uint32_type)
					return (uint) v;
				else if (target_type == TypeManager.byte_type){
					if (v >= Byte.MinValue && v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v <= SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.short_type){
					if (v <= Int16.MaxValue)
						return (short) v;
				} else if (target_type == TypeManager.ushort_type)
					return (short) v;
			        else if (target_type == TypeManager.int64_type)
					return (long) v;
				else if (target_type == TypeManager.uint64_type)
					return (ulong) v;

				s = v.ToString ();
			}
			Error_ConstantValueCannotBeConverted (loc, s, target_type);
			return null;
		}

		//
		// Load the object from the pointer.  
		//
		public static void LoadFromPtr (ILGenerator ig, Type t)
		{
			if (t == TypeManager.int32_type)
				ig.Emit (OpCodes.Ldind_I4);
			else if (t == TypeManager.uint32_type)
				ig.Emit (OpCodes.Ldind_U4);
			else if (t == TypeManager.short_type)
				ig.Emit (OpCodes.Ldind_I2);
			else if (t == TypeManager.ushort_type)
				ig.Emit (OpCodes.Ldind_U2);
			else if (t == TypeManager.char_type)
				ig.Emit (OpCodes.Ldind_U2);
			else if (t == TypeManager.byte_type)
				ig.Emit (OpCodes.Ldind_U1);
			else if (t == TypeManager.sbyte_type)
				ig.Emit (OpCodes.Ldind_I1);
			else if (t == TypeManager.uint64_type)
				ig.Emit (OpCodes.Ldind_I8);
			else if (t == TypeManager.int64_type)
				ig.Emit (OpCodes.Ldind_I8);
			else if (t == TypeManager.float_type)
				ig.Emit (OpCodes.Ldind_R4);
			else if (t == TypeManager.double_type)
				ig.Emit (OpCodes.Ldind_R8);
			else if (t == TypeManager.bool_type)
				ig.Emit (OpCodes.Ldind_I1);
			else if (t == TypeManager.intptr_type)
				ig.Emit (OpCodes.Ldind_I);
			else if (TypeManager.IsEnumType (t)) {
				if (t == TypeManager.enum_type)
					ig.Emit (OpCodes.Ldind_Ref);
				else
					LoadFromPtr (ig, TypeManager.EnumToUnderlying (t));
			} else if (t.IsValueType)
				ig.Emit (OpCodes.Ldobj, t);
			else
				ig.Emit (OpCodes.Ldind_Ref);
		}

		//
		// The stack contains the pointer and the value of type 'type'
		//
		public static void StoreFromPtr (ILGenerator ig, Type type)
		{
			if (TypeManager.IsEnumType (type))
				type = TypeManager.EnumToUnderlying (type);
			if (type == TypeManager.int32_type || type == TypeManager.uint32_type)
				ig.Emit (OpCodes.Stind_I4);
			else if (type == TypeManager.int64_type || type == TypeManager.uint64_type)
				ig.Emit (OpCodes.Stind_I8);
			else if (type == TypeManager.char_type || type == TypeManager.short_type ||
				 type == TypeManager.ushort_type)
				ig.Emit (OpCodes.Stind_I2);
			else if (type == TypeManager.float_type)
				ig.Emit (OpCodes.Stind_R4);
			else if (type == TypeManager.double_type)
				ig.Emit (OpCodes.Stind_R8);
			else if (type == TypeManager.byte_type || type == TypeManager.sbyte_type ||
				 type == TypeManager.bool_type)
				ig.Emit (OpCodes.Stind_I1);
			else if (type == TypeManager.intptr_type)
				ig.Emit (OpCodes.Stind_I);
			else if (type.IsValueType)
				ig.Emit (OpCodes.Stobj, type);
			else
				ig.Emit (OpCodes.Stind_Ref);
		}
		
		//
		// Returns the size of type 't' if known, otherwise, 0
		//
		public static int GetTypeSize (Type t)
		{
			t = TypeManager.TypeToCoreType (t);
			if (t == TypeManager.int32_type ||
			    t == TypeManager.uint32_type ||
			    t == TypeManager.float_type)
			        return 4;
			else if (t == TypeManager.int64_type ||
				 t == TypeManager.uint64_type ||
				 t == TypeManager.double_type)
			        return 8;
			else if (t == TypeManager.byte_type ||
				 t == TypeManager.sbyte_type ||
				 t == TypeManager.bool_type) 	
			        return 1;
			else if (t == TypeManager.short_type ||
				 t == TypeManager.char_type ||
				 t == TypeManager.ushort_type)
				return 2;
			else if (t == TypeManager.decimal_type)
				return 16;
			else
				return 0;
		}

		//
		// Default implementation of IAssignMethod.CacheTemporaries
		//
		public void CacheTemporaries (EmitContext ec)
		{
		}

		static void Error_NegativeArrayIndex (Location loc)
		{
			Report.Error (284, loc, "Can not create array with a negative size");
		}
		
		//
		// Converts 'source' to an int, uint, long or ulong.
		//
		public Expression ExpressionToArrayArgument (EmitContext ec, Expression source, Location loc)
		{
			Expression target;
			
			bool old_checked = ec.CheckState;
			ec.CheckState = true;
			
			target = ConvertImplicit (ec, source, TypeManager.int32_type, loc);
			if (target == null){
				target = ConvertImplicit (ec, source, TypeManager.uint32_type, loc);
				if (target == null){
					target = ConvertImplicit (ec, source, TypeManager.int64_type, loc);
					if (target == null){
						target = ConvertImplicit (ec, source, TypeManager.uint64_type, loc);
						if (target == null)
							Expression.Error_CannotConvertImplicit (loc, source.Type, TypeManager.int32_type);
					}
				}
			} 
			ec.CheckState = old_checked;

			//
			// Only positive constants are allowed at compile time
			//
			if (target is Constant){
				if (target is IntConstant){
					if (((IntConstant) target).Value < 0){
						Error_NegativeArrayIndex (loc);
						return null;
					}
				}

				if (target is LongConstant){
					if (((LongConstant) target).Value < 0){
						Error_NegativeArrayIndex (loc);
						return null;
					}
				}
				
			}

			return target;
		}
		
	}

	/// <summary>
	///   This is just a base class for expressions that can
	///   appear on statements (invocations, object creation,
	///   assignments, post/pre increment and decrement).  The idea
	///   being that they would support an extra Emition interface that
	///   does not leave a result on the stack.
	/// </summary>
	public abstract class ExpressionStatement : Expression {

		/// <summary>
		///   Requests the expression to be emitted in a 'statement'
		///   context.  This means that no new value is left on the
		///   stack after invoking this method (constrasted with
		///   Emit that will always leave a value on the stack).
		/// </summary>
		public abstract void EmitStatement (EmitContext ec);
	}

	/// <summary>
	///   This kind of cast is used to encapsulate the child
	///   whose type is child.Type into an expression that is
	///   reported to return "return_type".  This is used to encapsulate
	///   expressions which have compatible types, but need to be dealt
	///   at higher levels with.
	///
	///   For example, a "byte" expression could be encapsulated in one
	///   of these as an "unsigned int".  The type for the expression
	///   would be "unsigned int".
	///
	/// </summary>
	public class EmptyCast : Expression {
		protected Expression child;

		public EmptyCast (Expression child, Type return_type)
		{
			eclass = child.eclass;
			type = return_type;
			this.child = child;
		}

		public override Expression DoResolve (EmitContext ec)
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

	/// <summary>
	///  This class is used to wrap literals which belong inside Enums
	/// </summary>
	public class EnumConstant : Constant {
		public Constant Child;

		public EnumConstant (Constant child, Type enum_type)
		{
			eclass = child.eclass;
			this.Child = child;
			type = enum_type;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Child.Emit (ec);
		}

		public override object GetValue ()
		{
			return Child.GetValue ();
		}

		//
		// Converts from one of the valid underlying types for an enumeration
		// (int32, uint32, int64, uint64, short, ushort, byte, sbyte) to
		// one of the internal compiler literals: Int/UInt/Long/ULong Literals.
		//
		public Constant WidenToCompilerConstant ()
		{
			Type t = TypeManager.EnumToUnderlying (Child.Type);
			object v = ((Constant) Child).GetValue ();;
			
			if (t == TypeManager.int32_type)
				return new IntConstant ((int) v);
			if (t == TypeManager.uint32_type)
				return new UIntConstant ((uint) v);
			if (t == TypeManager.int64_type)
				return new LongConstant ((long) v);
			if (t == TypeManager.uint64_type)
				return new ULongConstant ((ulong) v);
			if (t == TypeManager.short_type)
				return new ShortConstant ((short) v);
			if (t == TypeManager.ushort_type)
				return new UShortConstant ((ushort) v);
			if (t == TypeManager.byte_type)
				return new ByteConstant ((byte) v);
			if (t == TypeManager.sbyte_type)
				return new SByteConstant ((sbyte) v);

			throw new Exception ("Invalid enumeration underlying type: " + t);
		}

		//
		// Extracts the value in the enumeration on its native representation
		//
		public object GetPlainValue ()
		{
			Type t = TypeManager.EnumToUnderlying (Child.Type);
			object v = ((Constant) Child).GetValue ();;
			
			if (t == TypeManager.int32_type)
				return (int) v;
			if (t == TypeManager.uint32_type)
				return (uint) v;
			if (t == TypeManager.int64_type)
				return (long) v;
			if (t == TypeManager.uint64_type)
				return (ulong) v;
			if (t == TypeManager.short_type)
				return (short) v;
			if (t == TypeManager.ushort_type)
				return (ushort) v;
			if (t == TypeManager.byte_type)
				return (byte) v;
			if (t == TypeManager.sbyte_type)
				return (sbyte) v;

			return null;
		}
		
		public override string AsString ()
		{
			return Child.AsString ();
		}

		public override DoubleConstant ConvertToDouble ()
		{
			return Child.ConvertToDouble ();
		}

		public override FloatConstant ConvertToFloat ()
		{
			return Child.ConvertToFloat ();
		}

		public override ULongConstant ConvertToULong ()
		{
			return Child.ConvertToULong ();
		}

		public override LongConstant ConvertToLong ()
		{
			return Child.ConvertToLong ();
		}

		public override UIntConstant ConvertToUInt ()
		{
			return Child.ConvertToUInt ();
		}

		public override IntConstant ConvertToInt ()
		{
			return Child.ConvertToInt ();
		}
	}

	/// <summary>
	///   This kind of cast is used to encapsulate Value Types in objects.
	///
	///   The effect of it is to box the value type emitted by the previous
	///   operation.
	/// </summary>
	public class BoxedCast : EmptyCast {

		public BoxedCast (Expression expr)
			: base (expr, TypeManager.object_type)
		{
		}

		public override Expression DoResolve (EmitContext ec)
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

	public class UnboxCast : EmptyCast {
		public UnboxCast (Expression expr, Type return_type)
			: base (expr, return_type)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Type t = type;
			ILGenerator ig = ec.ig;
			
			base.Emit (ec);
			ig.Emit (OpCodes.Unbox, t);

			LoadFromPtr (ig, t);
		}
	}
	
	/// <summary>
	///   This is used to perform explicit numeric conversions.
	///
	///   Explicit numeric conversions might trigger exceptions in a checked
	///   context, so they should generate the conv.ovf opcodes instead of
	///   conv opcodes.
	/// </summary>
	public class ConvCast : EmptyCast {
		public enum Mode : byte {
			I1_U1, I1_U2, I1_U4, I1_U8, I1_CH,
			U1_I1, U1_CH,
			I2_I1, I2_U1, I2_U2, I2_U4, I2_U8, I2_CH,
			U2_I1, U2_U1, U2_I2, U2_CH,
			I4_I1, I4_U1, I4_I2, I4_U2, I4_U4, I4_U8, I4_CH,
			U4_I1, U4_U1, U4_I2, U4_U2, U4_I4, U4_CH,
			I8_I1, I8_U1, I8_I2, I8_U2, I8_I4, I8_U4, I8_U8, I8_CH,
			U8_I1, U8_U1, U8_I2, U8_U2, U8_I4, U8_U4, U8_I8, U8_CH,
			CH_I1, CH_U1, CH_I2,
			R4_I1, R4_U1, R4_I2, R4_U2, R4_I4, R4_U4, R4_I8, R4_U8, R4_CH,
			R8_I1, R8_U1, R8_I2, R8_U2, R8_I4, R8_U4, R8_I8, R8_U8, R8_CH, R8_R4
		}

		Mode mode;
		bool checked_state;
		
		public ConvCast (EmitContext ec, Expression child, Type return_type, Mode m)
			: base (child, return_type)
		{
			checked_state = ec.CheckState;
			mode = m;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			base.Emit (ec);

			if (checked_state){
				switch (mode){
				case Mode.I1_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I1_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I1_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I1_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I1_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U1_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U1_CH: /* nothing */ break;

				case Mode.I2_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.I2_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I2_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I2_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I2_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I2_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U2_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U2_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U2_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U2_CH: /* nothing */ break;

				case Mode.I4_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.I4_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I4_I2: ig.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.I4_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I4_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I4_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I4_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U4_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U4_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U4_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U4_U2: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;
				case Mode.U4_I4: ig.Emit (OpCodes.Conv_Ovf_I4_Un); break;
				case Mode.U4_CH: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;

				case Mode.I8_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.I8_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I8_I2: ig.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.I8_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I8_I4: ig.Emit (OpCodes.Conv_Ovf_I4); break;
				case Mode.I8_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I8_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I8_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U8_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U8_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U8_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U8_U2: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;
				case Mode.U8_I4: ig.Emit (OpCodes.Conv_Ovf_I4_Un); break;
				case Mode.U8_U4: ig.Emit (OpCodes.Conv_Ovf_U4_Un); break;
				case Mode.U8_I8: ig.Emit (OpCodes.Conv_Ovf_I8_Un); break;
				case Mode.U8_CH: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;

				case Mode.CH_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.CH_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.CH_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;

				case Mode.R4_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.R4_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.R4_I2: ig.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.R4_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.R4_I4: ig.Emit (OpCodes.Conv_Ovf_I4); break;
				case Mode.R4_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.R4_I8: ig.Emit (OpCodes.Conv_Ovf_I8); break;
				case Mode.R4_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.R4_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.R8_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.R8_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.R8_I2: ig.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.R8_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.R8_I4: ig.Emit (OpCodes.Conv_Ovf_I4); break;
				case Mode.R8_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.R8_I8: ig.Emit (OpCodes.Conv_Ovf_I8); break;
				case Mode.R8_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.R8_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.R8_R4: ig.Emit (OpCodes.Conv_R4); break;
				}
			} else {
				switch (mode){
				case Mode.I1_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.I1_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.I1_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.I1_U8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.I1_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.U1_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U1_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.I2_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.I2_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.I2_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.I2_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.I2_U8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.I2_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.U2_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U2_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.U2_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.U2_CH: /* nothing */ break;

				case Mode.I4_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.I4_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.I4_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.I4_U4: /* nothing */ break;
				case Mode.I4_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.I4_U8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.I4_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.U4_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U4_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.U4_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.U4_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.U4_I4: /* nothing */ break;
				case Mode.U4_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.I8_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.I8_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.I8_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.I8_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.I8_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.I8_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.I8_U8: /* nothing */ break;
				case Mode.I8_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.U8_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U8_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.U8_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.U8_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.U8_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.U8_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.U8_I8: /* nothing */ break;
				case Mode.U8_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.CH_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.CH_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.CH_I2: ig.Emit (OpCodes.Conv_I2); break;

				case Mode.R4_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.R4_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.R4_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.R4_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.R4_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.R4_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.R4_I8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.R4_U8: ig.Emit (OpCodes.Conv_U8); break;
				case Mode.R4_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.R8_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.R8_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.R8_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.R8_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.R8_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.R8_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.R8_I8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.R8_U8: ig.Emit (OpCodes.Conv_U8); break;
				case Mode.R8_CH: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.R8_R4: ig.Emit (OpCodes.Conv_R4); break;
				}
			}
		}
	}
	
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

		public override Expression DoResolve (EmitContext ec)
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

	/// <summary>
	///   This kind of cast is used to encapsulate a child and cast it
	///   to the class requested
	/// </summary>
	public class ClassCast : EmptyCast {
		public ClassCast (Expression child, Type return_type)
			: base (child, return_type)
			
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);

			ec.ig.Emit (OpCodes.Castclass, type);
		}			
		
	}
	
	/// <summary>
	///   SimpleName expressions are initially formed of a single
	///   word and it only happens at the beginning of the expression.
	/// </summary>
	///
	/// <remarks>
	///   The expression will try to be bound to a Field, a Method
	///   group or a Property.  If those fail we pass the name to our
	///   caller and the SimpleName is compounded to perform a type
	///   lookup.  The idea behind this process is that we want to avoid
	///   creating a namespace map from the assemblies, as that requires
	///   the GetExportedTypes function to be called and a hashtable to
	///   be constructed which reduces startup time.  If later we find
	///   that this is slower, we should create a 'NamespaceExpr' expression
	///   that fully participates in the resolution process. 
	///   
	///   For example 'System.Console.WriteLine' is decomposed into
	///   MemberAccess (MemberAccess (SimpleName ("System"), "Console"), "WriteLine")
	///   
	///   The first SimpleName wont produce a match on its own, so it will
	///   be turned into:
	///   MemberAccess (SimpleName ("System.Console"), "WriteLine").
	///   
	///   System.Console will produce a TypeExpr match.
	///   
	///   The downside of this is that we might be hitting 'LookupType' too many
	///   times with this scheme.
	/// </remarks>
	public class SimpleName : Expression, ITypeExpression {
		public readonly string Name;
		
		public SimpleName (string name, Location l)
		{
			Name = name;
			loc = l;
		}

		public static void Error_ObjectRefRequired (EmitContext ec, Location l, string name)
		{
			if (ec.IsFieldInitializer)
				Report.Error (
					236, l,
					"A field initializer cannot reference the non-static field, " +
					"method or property '"+name+"'");
			else
				Report.Error (
					120, l,
					"An object reference is required " +
					"for the non-static field '"+name+"'");
		}
		
		//
		// Checks whether we are trying to access an instance
		// property, method or field from a static body.
		//
		Expression MemberStaticCheck (EmitContext ec, Expression e)
		{
			if (e is IMemberExpr){
				IMemberExpr member = (IMemberExpr) e;
				
				if (!member.IsStatic){
					Error_ObjectRefRequired (ec, loc, Name);
					return null;
				}
			}

			return e;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			return SimpleNameResolve (ec, null, false);
		}

		public override Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			return SimpleNameResolve (ec, right_side, false);
		}
		

		public Expression DoResolveAllowStatic (EmitContext ec)
		{
			return SimpleNameResolve (ec, null, true);
		}

		public Expression DoResolveType (EmitContext ec)
		{
			//
			// Stage 3: Lookup symbol in the various namespaces. 
			//
			DeclSpace ds = ec.DeclSpace;
			Type t;
			string alias_value;

			if (ec.ResolvingTypeTree){
				int errors = Report.Errors;
				Type dt = ec.DeclSpace.FindType (loc, Name);
				if (Report.Errors != errors)
					return null;
				
				if (dt != null)
					return new TypeExpr (dt, loc);
			}

			if ((t = RootContext.LookupType (ds, Name, true, loc)) != null)
				return new TypeExpr (t, loc);
				

			//
			// Stage 2 part b: Lookup up if we are an alias to a type
			// or a namespace.
			//
			// Since we are cheating: we only do the Alias lookup for
			// namespaces if the name does not include any dots in it
			//
				
			alias_value = ec.DeclSpace.LookupAlias (Name);
				
			if (Name.IndexOf ('.') == -1 && alias_value != null) {
				if ((t = RootContext.LookupType (ds, alias_value, true, loc)) != null)
					return new TypeExpr (t, loc);
					
				// we have alias value, but it isn't Type, so try if it's namespace
				return new SimpleName (alias_value, loc);
			}
				
			// No match, maybe our parent can compose us
			// into something meaningful.
			return this;
		}

		/// <remarks>
		///   7.5.2: Simple Names. 
		///
		///   Local Variables and Parameters are handled at
		///   parse time, so they never occur as SimpleNames.
		///
		///   The 'allow_static' flag is used by MemberAccess only
		///   and it is used to inform us that it is ok for us to 
		///   avoid the static check, because MemberAccess might end
		///   up resolving the Name as a Type name and the access as
		///   a static type access.
		///
		///   ie: Type Type; .... { Type.GetType (""); }
		///
		///   Type is both an instance variable and a Type;  Type.GetType
		///   is the static method not an instance method of type.
		/// </remarks>
		Expression SimpleNameResolve (EmitContext ec, Expression right_side, bool allow_static)
		{
			Expression e = null;
			
			//
			// Stage 1: Performed by the parser (binding to locals or parameters).
			//
			Block current_block = ec.CurrentBlock;
			if (ec.InvokingOwnOverload == false && current_block != null && current_block.IsVariableDefined (Name)){
				LocalVariableReference var;

				var = new LocalVariableReference (ec.CurrentBlock, Name, loc);

				if (right_side != null)
					return var.ResolveLValue (ec, right_side);
				else
					return var.Resolve (ec);
			}

			if (current_block != null){
				int idx = -1;
				Parameter par = null;
				Parameters pars = current_block.Parameters;
				if (pars != null)
					par = pars.GetParameterByName (Name, out idx);

				if (par != null) {
					ParameterReference param;
					
					param = new ParameterReference (pars, idx, Name, loc);

					if (right_side != null)
						return param.ResolveLValue (ec, right_side);
					else
						return param.Resolve (ec);
				}
			}

			//
			// Stage 2: Lookup members 
			//

			//
			// For enums, the TypeBuilder is not ec.DeclSpace.TypeBuilder
			// Hence we have two different cases
			//

			DeclSpace lookup_ds = ec.DeclSpace;
			do {
				if (lookup_ds.TypeBuilder == null)
					break;

				e = MemberLookup (ec, lookup_ds.TypeBuilder, Name, loc);
				if (e != null)
					break;

				//
				// Classes/structs keep looking, enums break
				//
				if (lookup_ds is TypeContainer)
					lookup_ds = ((TypeContainer) lookup_ds).Parent;
				else
					break;
			} while (lookup_ds != null);
				
			if (e == null && ec.ContainerType != null)
				e = MemberLookup (ec, ec.ContainerType, Name, loc);

// #52067 - Start - Trying to solve

			if (e == null) {
				
				ArrayList lookups = new ArrayList();
				ArrayList typelookups = new ArrayList();
				
				int split = Name.LastIndexOf('.');
				if (split != -1) {
					String nameSpacePart = Name.Substring(0, split);
					String memberNamePart = Name.Substring(split + 1);
					foreach(Type type in TypeManager.GetPertinentStandardModules(nameSpacePart)) {
						e = MemberLookup(ec, type, memberNamePart, loc);
						if (e != null) {
							lookups.Add(e);
							typelookups.Add(type);
						}
					}
				}
				
				string[] NamespacesInScope = RootContext.SourceBeingCompiled.GetNamespacesInScope(ec.DeclSpace.Namespace.Name);
				foreach(Type type in TypeManager.GetPertinentStandardModules(NamespacesInScope)) {
					e = MemberLookup(ec, type, Name, loc);
					if (e != null) {
						lookups.Add(e);
						typelookups.Add(type);
					}
				}
				if (lookups.Count == 1) { 
					e = (Expression)lookups[0];
				} else {
					if (lookups.Count > 1) {
						StringBuilder sb = new StringBuilder();
						foreach(Type type in typelookups)
							sb.Append("'" + type.FullName + "'");						
						Error (-1, "The name '" + Name + "' can be resolved to a member of more than one standard module: " + sb.ToString() + ". Please fully qualify it.");
						return null;
					}
				}
			}

// #52067 - End

			if (e == null)
				return DoResolveType (ec);

			if (e is TypeExpr)
				return e;

			if (e is IMemberExpr) {
				e = MemberAccess.ResolveMemberAccess (ec, e, null, loc, this);
				if (e == null)
					return null;

				IMemberExpr me = e as IMemberExpr;
				if (me == null)
					return e;

				// This fails if ResolveMemberAccess() was unable to decide whether
				// it's a field or a type of the same name.
				if (!me.IsStatic && (me.InstanceExpression == null))
					return e;

/* FIXME    If this is not commented out, it seems that it's not possible to reach class members in mBas.
            Maybe a grammar-related problem?

				if (!me.IsStatic &&
				    TypeManager.IsNestedChildOf (me.InstanceExpression.Type, me.DeclaringType)) {
					Error (38, "Cannot access nonstatic member '" + me.Name + "' of " +
					       "outer type '" + me.DeclaringType + "' via nested type '" +
					       me.InstanceExpression.Type + "'");
					return null;
				}
*/
				if (right_side != null)
					e = e.DoResolveLValue (ec, right_side);
				else
					e = e.DoResolve (ec);

				return e;
			}

			if (ec.IsStatic || ec.IsFieldInitializer){
				if (allow_static)
					return e;

				return MemberStaticCheck (ec, e);
			} else
				return e;
		}
		
		public override void Emit (EmitContext ec)
		{
			//
			// If this is ever reached, then we failed to
			// find the name as a namespace
			//

			Error (30451, "The name '" + Name +
			       "' does not exist in the class '" +
			       ec.DeclSpace.Name + "'");
		}

		public override string ToString ()
		{
			return Name;
		}
	}
	
	/// <summary>
	///   Fully resolved expression that evaluates to a type
	/// </summary>
	public class TypeExpr : Expression, ITypeExpression {
		public TypeExpr (Type t, Location l)
		{
			Type = t;
			eclass = ExprClass.Type;
			loc = l;
		}

		public virtual Expression DoResolveType (EmitContext ec)
		{
			return this;
		}

		override public Expression DoResolve (EmitContext ec)
		{
			return this;
		}

		override public void Emit (EmitContext ec)
		{
			throw new Exception ("Should never be called");
		}

		public override string ToString ()
		{
			return Type.ToString ();
		}
	}

	/// <summary>
	///   Used to create types from a fully qualified name.  These are just used
	///   by the parser to setup the core types.  A TypeLookupExpression is always
	///   classified as a type.
	/// </summary>
	public class TypeLookupExpression : TypeExpr {
		string name;
		
		public TypeLookupExpression (string name) : base (null, Location.Null)
		{
			this.name = name;
		}

		public override Expression DoResolveType (EmitContext ec)
		{
			if (type == null)
				type = RootContext.LookupType (ec.DeclSpace, name, false, Location.Null);
			return this;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			return DoResolveType (ec);
		}

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Should never be called");
		}

		public override string ToString ()
		{
			return name;
		}
	}

	/// <summary>
	///   MethodGroup Expression.
	///  
	///   This is a fully resolved expression that evaluates to a type
	/// </summary>
	public class MethodGroupExpr : Expression, IMemberExpr {
		public MethodBase [] Methods;
		Expression instance_expression = null;
		bool is_explicit_impl = false;
		
		public MethodGroupExpr (MemberInfo [] mi, Location l)
		{
			Methods = new MethodBase [mi.Length];
			mi.CopyTo (Methods, 0);
			eclass = ExprClass.MethodGroup;
			type = TypeManager.object_type;
			loc = l;
		}

		public MethodGroupExpr (ArrayList list, Location l)
		{
			Methods = new MethodBase [list.Count];

			try {
				list.CopyTo (Methods, 0);
			} catch {
				foreach (MemberInfo m in list){
					if (!(m is MethodBase)){
						Console.WriteLine ("Name " + m.Name);
						Console.WriteLine ("Found a: " + m.GetType ().FullName);
					}
				}
				throw;
			}
			loc = l;
			eclass = ExprClass.MethodGroup;
			type = TypeManager.object_type;
		}

		public Type DeclaringType {
			get {
				return Methods [0].DeclaringType;
			}
		}
		
		//
		// 'A method group may have associated an instance expression' 
		// 
		public Expression InstanceExpression {
			get {
				return instance_expression;
			}

			set {
				instance_expression = value;
			}
		}

		public bool IsExplicitImpl {
			get {
				return is_explicit_impl;
			}

			set {
				is_explicit_impl = value;
			}
		}

		public string Name {
			get {
				return Methods [0].Name;
			}
		}

		public bool IsInstance {
			get {
				foreach (MethodBase mb in Methods)
					if (!mb.IsStatic)
						return true;

				return false;
			}
		}

		public bool IsStatic {
			get {
				foreach (MethodBase mb in Methods)
					if (mb.IsStatic)
						return true;

				return false;
			}
		}
		
		override public Expression DoResolve (EmitContext ec)
		{
			if (instance_expression != null) {
				instance_expression = instance_expression.DoResolve (ec);
				if (instance_expression == null)
					return null;
			}

			return this;
		}

		public void ReportUsageError ()
		{
			Report.Error (654, loc, "Method '" + Methods [0].DeclaringType + "." +
				      Methods [0].Name + "()' is referenced without parentheses");
		}

		override public void Emit (EmitContext ec)
		{
			ReportUsageError ();
		}

		bool RemoveMethods (bool keep_static)
		{
			ArrayList smethods = new ArrayList ();

			foreach (MethodBase mb in Methods){
				if (mb.IsStatic == keep_static)
					smethods.Add (mb);
			}

			if (smethods.Count == 0)
				return false;

			Methods = new MethodBase [smethods.Count];
			smethods.CopyTo (Methods, 0);

			return true;
		}
		
		/// <summary>
		///   Removes any instance methods from the MethodGroup, returns
		///   false if the resulting set is empty.
		/// </summary>
		public bool RemoveInstanceMethods ()
		{
			return RemoveMethods (true);
		}

		/// <summary>
		///   Removes any static methods from the MethodGroup, returns
		///   false if the resulting set is empty.
		/// </summary>
		public bool RemoveStaticMethods ()
		{
			return RemoveMethods (false);
		}
	}

	/// <summary>
	///   Fully resolved expression that evaluates to a Field
	/// </summary>
	public class FieldExpr : Expression, IAssignMethod, IMemoryLocation, IMemberExpr {
		public readonly FieldInfo FieldInfo;
		Expression instance_expr;
		
		public FieldExpr (FieldInfo fi, Location l)
		{
			FieldInfo = fi;
			eclass = ExprClass.Variable;
			type = fi.FieldType;
			loc = l;
		}

		public string Name {
			get {
				return FieldInfo.Name;
			}
		}

		public bool IsInstance {
			get {
				return !FieldInfo.IsStatic;
			}
		}

		public bool IsStatic {
			get {
				return FieldInfo.IsStatic;
			}
		}

		public Type DeclaringType {
			get {
				return FieldInfo.DeclaringType;
			}
		}

		public Expression InstanceExpression {
			get {
				return instance_expr;
			}

			set {
				instance_expr = value;
			}
		}

		override public Expression DoResolve (EmitContext ec)
		{
			if (!FieldInfo.IsStatic){
				if (instance_expr == null){
					throw new Exception ("non-static FieldExpr without instance var\n" +
							     "You have to assign the Instance variable\n" +
							     "Of the FieldExpr to set this\n");
				}

				// Resolve the field's instance expression while flow analysis is turned
				// off: when accessing a field "a.b", we must check whether the field
				// "a.b" is initialized, not whether the whole struct "a" is initialized.
				instance_expr = instance_expr.Resolve (ec, ResolveFlags.VariableOrValue |
								       ResolveFlags.DisableFlowAnalysis);
				if (instance_expr == null)
					return null;
			}

			// If the instance expression is a local variable or parameter.
			IVariable var = instance_expr as IVariable;
			if ((var != null) && !var.IsFieldAssigned (ec, FieldInfo.Name, loc))
				return null;

			return this;
		}

		void Report_AssignToReadonly (bool is_instance)
		{
			string msg;
			
			if (is_instance)
				msg = "Readonly field can not be assigned outside " +
				"of constructor or variable initializer";
			else
				msg = "A static readonly field can only be assigned in " +
				"a static constructor";

			Report.Error (is_instance ? 191 : 198, loc, msg);
		}
		
		override public Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			IVariable var = instance_expr as IVariable;
			if (var != null)
				var.SetFieldAssigned (ec, FieldInfo.Name);

			Expression e = DoResolve (ec);

			if (e == null)
				return null;
			
			if (!FieldInfo.IsInitOnly)
				return this;

			//
			// InitOnly fields can only be assigned in constructors
			//

			if (ec.IsConstructor)
				return this;

			Report_AssignToReadonly (true);
			
			return null;
		}

		override public void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			bool is_volatile = false;

			if (FieldInfo is FieldBuilder){
				FieldBase f = TypeManager.GetField (FieldInfo);

				if ((f.ModFlags & Modifiers.VOLATILE) != 0)
					is_volatile = true;
				
				f.status |= Field.Status.USED;
			}
			
			if (FieldInfo.IsStatic){
				if (is_volatile)
					ig.Emit (OpCodes.Volatile);
				
				ig.Emit (OpCodes.Ldsfld, FieldInfo);
			} else {
				if (instance_expr.Type.IsValueType){
					IMemoryLocation ml;
					LocalTemporary tempo = null;
					
					if (!(instance_expr is IMemoryLocation)){
						tempo = new LocalTemporary (
							ec, instance_expr.Type);

						InstanceExpression.Emit (ec);
						tempo.Store (ec);
						ml = tempo;
					} else
						ml = (IMemoryLocation) instance_expr;

					ml.AddressOf (ec, AddressOp.Load);
				} else 
					instance_expr.Emit (ec);

				if (is_volatile)
					ig.Emit (OpCodes.Volatile);
				
				ig.Emit (OpCodes.Ldfld, FieldInfo);
			}
		}

		public void EmitAssign (EmitContext ec, Expression source)
		{
			FieldAttributes fa = FieldInfo.Attributes;
			bool is_static = (fa & FieldAttributes.Static) != 0;
			bool is_readonly = (fa & FieldAttributes.InitOnly) != 0;
			ILGenerator ig = ec.ig;

			if (is_readonly && !ec.IsConstructor){
				Report_AssignToReadonly (!is_static);
				return;
			}
			
			if (!is_static){
				Expression instance = instance_expr;

				if (instance.Type.IsValueType){
					if (instance is IMemoryLocation){
						IMemoryLocation ml = (IMemoryLocation) instance;

						ml.AddressOf (ec, AddressOp.Store);
					} else
						throw new Exception ("The " + instance + " of type " +
								     instance.Type +
								     " represents a ValueType and does " +
								     "not implement IMemoryLocation");
				} else
					instance.Emit (ec);
			}
			source.Emit (ec);

			if (FieldInfo is FieldBuilder){
				FieldBase f = TypeManager.GetField (FieldInfo);
				
				if ((f.ModFlags & Modifiers.VOLATILE) != 0)
					ig.Emit (OpCodes.Volatile);
			}
			
			if (is_static)
				ig.Emit (OpCodes.Stsfld, FieldInfo);
			else 
				ig.Emit (OpCodes.Stfld, FieldInfo);

			if (FieldInfo is FieldBuilder){
				FieldBase f = TypeManager.GetField (FieldInfo);

				f.status |= Field.Status.ASSIGNED;
			}
		}
		
		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			ILGenerator ig = ec.ig;
			
			if (FieldInfo is FieldBuilder){
				FieldBase f = TypeManager.GetField (FieldInfo);
				if ((f.ModFlags & Modifiers.VOLATILE) != 0)
					ig.Emit (OpCodes.Volatile);
			}

			if (FieldInfo is FieldBuilder){
				FieldBase f = TypeManager.GetField (FieldInfo);

				if ((mode & AddressOp.Store) != 0)
					f.status |= Field.Status.ASSIGNED;
				if ((mode & AddressOp.Load) != 0)
					f.status |= Field.Status.USED;
			}

			//
			// Handle initonly fields specially: make a copy and then
			// get the address of the copy.
			//
			if (FieldInfo.IsInitOnly && !ec.IsConstructor){
				LocalBuilder local;
				
				Emit (ec);
				local = ig.DeclareLocal (type);
				ig.Emit (OpCodes.Stloc, local);
				ig.Emit (OpCodes.Ldloca, local);
				return;
			}

			if (FieldInfo.IsStatic)
				ig.Emit (OpCodes.Ldsflda, FieldInfo);
			else {
				if (instance_expr is IMemoryLocation)
					((IMemoryLocation)instance_expr).AddressOf (ec, AddressOp.LoadStore);
				else
					instance_expr.Emit (ec);
				ig.Emit (OpCodes.Ldflda, FieldInfo);
			}
		}
	}
	
	/// <summary>
	///   Expression that evaluates to a Property.  The Assign class
	///   might set the 'Value' expression if we are in an assignment.
	///
	///   This is not an LValue because we need to re-write the expression, we
	///   can not take data from the stack and store it.  
	/// </summary>
	public class PropertyExpr : ExpressionStatement, IAssignMethod, IMemberExpr {
		public readonly PropertyInfo PropertyInfo;
		public bool IsBase;
		MethodInfo getter, setter;
		bool is_static;
		public ArrayList PropertyArgs;

		Expression instance_expr;

		public PropertyExpr (EmitContext ec, PropertyInfo pi, Location l)
		{
			PropertyInfo = pi;
			eclass = ExprClass.PropertyAccess;
			PropertyArgs = new ArrayList();
			is_static = false;
			loc = l;

			type = TypeManager.TypeToCoreType (pi.PropertyType);

			ResolveAccessors (ec);
		}

		public string Name {
			get {
				return PropertyInfo.Name;
			}
		}

		public bool IsInstance {
			get {
				return !is_static;
			}
		}

		public bool IsStatic {
			get {
				return is_static;
			}
		}
		
		public Type DeclaringType {
			get {
				return PropertyInfo.DeclaringType;
			}
		}

		//
		// The instance expression associated with this expression
		//
		public Expression InstanceExpression {
			set {
				instance_expr = value;
			}

			get {
				return instance_expr;
			}
		}

		public bool VerifyAssignable ()
		{
			if (!PropertyInfo.CanWrite){
				Report.Error (200, loc, 
					      "The property '" + PropertyInfo.Name +
					      "' can not be assigned to, as it has not set accessor");
				return false;
			}

			return true;
		}

		void ResolveAccessors (EmitContext ec)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
			MemberInfo [] group;
			
			group = TypeManager.MemberLookup (ec.ContainerType, PropertyInfo.DeclaringType,
							      MemberTypes.Method, flags, "get_" + PropertyInfo.Name);

			//
			// The first method is the closest to us
			//
			if (group != null && group.Length > 0){
				getter = (MethodInfo) group [0];

				if (getter.IsStatic)
					is_static = true;
			} 			

			//
			// The first method is the closest to us
			//
			group = TypeManager.MemberLookup (ec.ContainerType, PropertyInfo.DeclaringType,
							  MemberTypes.Method, flags, "set_" + PropertyInfo.Name);
			if (group != null && group.Length > 0){
				setter = (MethodInfo) group [0];
				if (setter.IsStatic)
					is_static = true;
			}
		}

		override public Expression DoResolve (EmitContext ec)
		{
			if (getter == null){
				Report.Error (154, loc, 
					      "The property '" + PropertyInfo.Name +
					      "' can not be used in " +
					      "this context because it lacks a get accessor");
				return null;
			}

			if ((instance_expr == null) && ec.IsStatic && !is_static) {
				SimpleName.Error_ObjectRefRequired (ec, loc, PropertyInfo.Name);
				return null;
			}

			if (instance_expr != null) {
				instance_expr = instance_expr.DoResolve (ec);
				if (instance_expr == null)
					return null;
			}

			return this;
		}

		override public Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			if (setter == null){
				Report.Error (154, loc, 
					      "The property '" + PropertyInfo.Name +
					      "' can not be used in " +
					      "this context because it lacks a set accessor");
				return null;
			}

			if (instance_expr != null) {
				instance_expr = instance_expr.DoResolve (ec);
				if (instance_expr == null)
					return null;
			}

			return this;
		}

		override public void Emit (EmitContext ec)
		{
			//
			// Special case: length of single dimension array property is turned into ldlen
			//
			if ((getter == TypeManager.system_int_array_get_length) ||
			    (getter == TypeManager.int_array_get_length)){
				Type iet = instance_expr.Type;

				//
				// System.Array.Length can be called, but the Type does not
				// support invoking GetArrayRank, so test for that case first
				//
				if (iet != TypeManager.array_type && (iet.GetArrayRank () == 1)){
					instance_expr.Emit (ec);
					ec.ig.Emit (OpCodes.Ldlen);
					return;
				}
			}
			Invocation.EmitCall (ec, IsBase, IsStatic, instance_expr, getter, null, PropertyArgs, loc);
		}

		//
		// Implements the IAssignMethod interface for assignments
		//
		public void EmitAssign (EmitContext ec, Expression source)
		{
			Argument arg = new Argument (source, Argument.AType.Expression);
			ArrayList args = new ArrayList ();
//HERE
			args.Add (arg);
			Invocation.EmitCall (ec, IsBase, IsStatic, instance_expr, setter, args, PropertyArgs,loc);
		}

		override public void EmitStatement (EmitContext ec)
		{
			Emit (ec);
			ec.ig.Emit (OpCodes.Pop);
		}
	}

	/// <summary>
	///   Fully resolved expression that evaluates to an Event
	/// </summary>
	public class EventExpr : Expression, IMemberExpr {
		public readonly EventInfo EventInfo;
		public Expression instance_expr;

		bool is_static;
		MethodInfo add_accessor, remove_accessor;
		
		public EventExpr (EventInfo ei, Location loc)
		{
			EventInfo = ei;
			this.loc = loc;
			eclass = ExprClass.EventAccess;

			add_accessor = TypeManager.GetAddMethod (ei);
			remove_accessor = TypeManager.GetRemoveMethod (ei);
			
			if (add_accessor.IsStatic || remove_accessor.IsStatic)
				is_static = true;

			if (EventInfo is MyEventBuilder)
				type = ((MyEventBuilder) EventInfo).EventType;
			else
				type = EventInfo.EventHandlerType;
		}

		public string Name {
			get {
				return EventInfo.Name;
			}
		}

		public bool IsInstance {
			get {
				return !is_static;
			}
		}

		public bool IsStatic {
			get {
				return is_static;
			}
		}

		public Type DeclaringType {
			get {
				return EventInfo.DeclaringType;
			}
		}

		public Expression InstanceExpression {
			get {
				return instance_expr;
			}

			set {
				instance_expr = value;
			}
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (instance_expr != null) {
				instance_expr = instance_expr.DoResolve (ec);
				if (instance_expr == null)
					return null;
			}

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Report.Error (70, loc, "The event '" + Name + "' can only appear on the left hand side of += or -= (except on the defining type)");
		}

		public void EmitAddOrRemove (EmitContext ec, Expression source)
		{
			Expression handler = ((Binary) source).Right;
			
			Argument arg = new Argument (handler, Argument.AType.Expression);
			ArrayList args = new ArrayList ();
				
			args.Add (arg);
			
			if (((Binary) source).Oper == Binary.Operator.Addition)
				Invocation.EmitCall (
					ec, false, IsStatic, instance_expr, add_accessor, args, loc);
			else
				Invocation.EmitCall (
					ec, false, IsStatic, instance_expr, remove_accessor, args, loc);
		}
	}
}
