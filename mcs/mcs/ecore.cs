//
// ecore.cs: Core of the Expression representation for the intermediate tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001, 2002, 2003 Ximian, Inc.
//
//

namespace Mono.CSharp {
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
		DisableFlowAnalysis	= 16,

		// Set if this is resolving the first part of a MemberAccess.
		Intermediate		= 32
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
		///   The `mode' argument is used to notify the expression
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
		VariableInfo VariableInfo {
			get;
		}

		bool VerifyFixed (bool is_expression);
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
		public void Warning (int code, string format, params object[] args)
		{
			Report.Warning (code, loc, format, args);
		}

		/// <summary>
		/// Tests presence of ObsoleteAttribute and report proper error
		/// </summary>
		protected void CheckObsoleteAttribute (Type type)
		{
			ObsoleteAttribute obsolete_attr = AttributeTester.GetObsoleteAttribute (type);
			if (obsolete_attr == null)
				return;

			AttributeTester.Report_ObsoleteMessage (obsolete_attr, type.FullName, loc);
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
		///   `ExprClass' and the type variable should be set
		///   to a valid type (this is the type of the
		///   expression).
		/// </remarks>
		public abstract Expression DoResolve (EmitContext ec);

		public virtual Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			return DoResolve (ec);
		}

		//
		// This is used if the expression should be resolved as a type.
		// the default implementation fails.   Use this method in
		// those participants in the SimpleName chain system.
		//
		public virtual Expression ResolveAsTypeStep (EmitContext ec)
		{
			return null;
		}

		//
		// This is used to resolve the expression as a type, a null
		// value will be returned if the expression is not a type
		// reference
		//
		public TypeExpr ResolveAsTypeTerminal (EmitContext ec, bool silent)
		{
			int errors = Report.Errors;

			TypeExpr te = ResolveAsTypeStep (ec) as TypeExpr;

			if (te == null || te.eclass != ExprClass.Type) {
				if (!silent && errors == Report.Errors)
					Report.Error (246, Location, "Cannot find type '{0}'", ToString ());
				return null;
			}

			if (!te.CheckAccessLevel (ec.DeclSpace)) {
				Report.Error (122, Location, "'{0}' is inaccessible due to its protection level", te.Name);
				return null;
			}

			return te;
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
			if ((flags & ResolveFlags.MaskExprClass) == ResolveFlags.Type) 
				return ResolveAsTypeStep (ec);

			bool old_do_flow_analysis = ec.DoFlowAnalysis;
			if ((flags & ResolveFlags.DisableFlowAnalysis) != 0)
				ec.DoFlowAnalysis = false;

			Expression e;
			bool intermediate = (flags & ResolveFlags.Intermediate) == ResolveFlags.Intermediate;
			if (this is SimpleName)
				e = ((SimpleName) this).DoResolveAllowStatic (ec, intermediate);

			else 
				e = DoResolve (ec);

			ec.DoFlowAnalysis = old_do_flow_analysis;

			if (e == null)
				return null;

			if (e is SimpleName){
				SimpleName s = (SimpleName) e;

				if ((flags & ResolveFlags.SimpleName) == 0) {
					MemberLookupFailed (ec, null, ec.ContainerType, s.Name,
							    ec.DeclSpace.Name, loc);
					return null;
				}

				return s;
			}

			if ((e is TypeExpr) || (e is ComposedCast)) {
				if ((flags & ResolveFlags.Type) == 0) {
					e.Error_UnexpectedKind (flags, loc);
					return null;
				}

				return e;
			}

			switch (e.eclass) {
			case ExprClass.Type:
				if ((flags & ResolveFlags.VariableOrValue) == 0) {
					e.Error_UnexpectedKind (flags, loc);
					return null;
				}
				break;

			case ExprClass.MethodGroup:
				if (RootContext.Version == LanguageVersion.ISO_1){
					if ((flags & ResolveFlags.MethodGroup) == 0) {
						((MethodGroupExpr) e).ReportUsageError ();
						return null;
					}
				}
				break;

			case ExprClass.Value:
			case ExprClass.Variable:
			case ExprClass.PropertyAccess:
			case ExprClass.EventAccess:
			case ExprClass.IndexerAccess:
				if ((flags & ResolveFlags.VariableOrValue) == 0) {
					Console.WriteLine ("I got: {0} and {1}", e.GetType (), e);
					Console.WriteLine ("I am {0} and {1}", this.GetType (), this);
					FieldInfo fi = ((FieldExpr) e).FieldInfo;
					
					Console.WriteLine ("{0} and {1}", fi.DeclaringType, fi.Name);
					e.Error_UnexpectedKind (flags, loc);
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
					MemberLookupFailed (ec, null, ec.ContainerType, s.Name,
							    ec.DeclSpace.Name, loc);
					return null;
				}

				if (e.eclass == ExprClass.Invalid)
					throw new Exception ("Expression " + e +
							     " ExprClass is Invalid after resolve");

				if (e.eclass == ExprClass.MethodGroup) {
					((MethodGroupExpr) e).ReportUsageError ();
					return null;
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

		public virtual void EmitBranchable (EmitContext ec, Label target, bool onTrue)
		{
			Emit (ec);
			ec.ig.Emit (onTrue ? OpCodes.Brtrue : OpCodes.Brfalse, target);
		}

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
				Type real_type = TypeManager.TypeToCoreType (v.GetType ());
				if (real_type == t)
					real_type = System.Enum.GetUnderlyingType (real_type);

				Constant e = Constantify (v, real_type);

				return new EnumConstant (e, t);
			} else if (v == null && !TypeManager.IsValueType (t)){
				return NullLiteral.Null;
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
				return new TypeExpression ((System.Type) mi, loc);
			}

			return null;
		}


		private static ArrayList almostMatchedMembers = new ArrayList (4);

		//
		// FIXME: Probably implement a cache for (t,name,current_access_set)?
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
		//
		// FIXME: Potential optimization, have a static ArrayList
		//

		public static Expression MemberLookup (EmitContext ec, Type queried_type, string name,
						       MemberTypes mt, BindingFlags bf, Location loc)
		{
			return MemberLookup (ec, ec.ContainerType, null, queried_type, name, mt, bf, loc);
		}

		//
		// Lookup type `queried_type' for code in class `container_type' with a qualifier of
		// `qualifier_type' or null to lookup members in the current class.
		//

		public static Expression MemberLookup (EmitContext ec, Type container_type,
						       Type qualifier_type, Type queried_type,
						       string name, MemberTypes mt,
						       BindingFlags bf, Location loc)
		{
			almostMatchedMembers.Clear ();

			MemberInfo [] mi = TypeManager.MemberLookup (container_type, qualifier_type,
								     queried_type, mt, bf, name, almostMatchedMembers);

			if (mi == null)
				return null;

			int count = mi.Length;

			if (mi [0] is MethodBase)
				return new MethodGroupExpr (mi, loc);

			if (count > 1)
				return null;

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
			BindingFlags.Instance;

		public static Expression MemberLookup (EmitContext ec, Type queried_type,
						       string name, Location loc)
		{
			return MemberLookup (ec, ec.ContainerType, null, queried_type, name,
					     AllMemberTypes, AllBindingFlags, loc);
		}

		public static Expression MemberLookup (EmitContext ec, Type qualifier_type,
						       Type queried_type, string name, Location loc)
		{
			return MemberLookup (ec, ec.ContainerType, qualifier_type, queried_type,
					     name, AllMemberTypes, AllBindingFlags, loc);
		}

		public static Expression MethodLookup (EmitContext ec, Type queried_type,
						       string name, Location loc)
		{
			return MemberLookup (ec, ec.ContainerType, null, queried_type, name,
					     MemberTypes.Method, AllBindingFlags, loc);
		}

		/// <summary>
		///   This is a wrapper for MemberLookup that is not used to "probe", but
		///   to find a final definition.  If the final definition is not found, we
		///   look for private members and display a useful debugging message if we
		///   find it.
		/// </summary>
		public static Expression MemberLookupFinal (EmitContext ec, Type qualifier_type,
							    Type queried_type, string name, Location loc)
		{
			return MemberLookupFinal (ec, qualifier_type, queried_type, name,
						  AllMemberTypes, AllBindingFlags, loc);
		}

		public static Expression MemberLookupFinal (EmitContext ec, Type qualifier_type,
							    Type queried_type, string name,
							    MemberTypes mt, BindingFlags bf,
							    Location loc)
		{
			Expression e;

			int errors = Report.Errors;

			e = MemberLookup (ec, ec.ContainerType, qualifier_type, queried_type, name, mt, bf, loc);

			if (e == null && errors == Report.Errors)
				// No errors were reported by MemberLookup, but there was an error.
				MemberLookupFailed (ec, qualifier_type, queried_type, name, null, loc);

			return e;
		}

		public static void MemberLookupFailed (EmitContext ec, Type qualifier_type,
						       Type queried_type, string name,
						       string class_name, Location loc)
		{
			if (almostMatchedMembers.Count != 0) {
				if (qualifier_type == null) {
					foreach (MemberInfo m in almostMatchedMembers)
						Report.Error (38, loc, 
							      "Cannot access non-static member `{0}' via nested type `{1}'", 
							      TypeManager.GetFullNameSignature (m),
							      TypeManager.CSharpName (ec.ContainerType));
					return;
				}

				if (qualifier_type != ec.ContainerType) {
					// Although a derived class can access protected members of
					// its base class it cannot do so through an instance of the
					// base class (CS1540).  If the qualifier_type is a parent of the
					// ec.ContainerType and the lookup succeeds with the latter one,
					// then we are in this situation.
					foreach (MemberInfo m in almostMatchedMembers)
						Report.Error (1540, loc, 
							      "Cannot access protected member `{0}' via a qualifier of type `{1}';"
							      + " the qualifier must be of type `{2}' (or derived from it)", 
							      TypeManager.GetFullNameSignature (m),
							      TypeManager.CSharpName (qualifier_type),
							      TypeManager.CSharpName (ec.ContainerType));
					return;
				}
				almostMatchedMembers.Clear ();
			}

			object lookup = TypeManager.MemberLookup (queried_type, null, queried_type,
								  AllMemberTypes, AllBindingFlags |
								  BindingFlags.NonPublic, name, null);

			if (lookup == null) {
				if (class_name != null)
					Report.Error (103, loc, "The name `" + name + "' could not be " +
						      "found in `" + class_name + "'");
				else
					Report.Error (
						117, loc, "`" + queried_type + "' does not contain a " +
						"definition for `" + name + "'");
				return;
			}

			if (qualifier_type != null)
				Report.Error (122, loc, "'{0}' is inaccessible due to its protection level", TypeManager.CSharpName (qualifier_type) + "." + name);
			else if (name == ".ctor") {
				Report.Error (143, loc, String.Format ("The type {0} has no constructors defined",
								       TypeManager.CSharpName (queried_type)));
			} else {
				Report.Error (122, loc, "'{0}' is inaccessible due to its protection level", name);
			}
		}

		static public MemberInfo GetFieldFromEvent (EventExpr event_expr)
		{
			EventInfo ei = event_expr.EventInfo;

			return TypeManager.GetPrivateFieldOfEvent (ei);
		}
		
		/// <summary>
		///   Returns an expression that can be used to invoke operator true
		///   on the expression if it exists.
		/// </summary>
		static public StaticCallExpr GetOperatorTrue (EmitContext ec, Expression e, Location loc)
		{
			return GetOperatorTrueOrFalse (ec, e, true, loc);
		}

		/// <summary>
		///   Returns an expression that can be used to invoke operator false
		///   on the expression if it exists.
		/// </summary>
		static public StaticCallExpr GetOperatorFalse (EmitContext ec, Expression e, Location loc)
		{
			return GetOperatorTrueOrFalse (ec, e, false, loc);
		}

		static StaticCallExpr GetOperatorTrueOrFalse (EmitContext ec, Expression e, bool is_true, Location loc)
		{
			MethodBase method;
			Expression operator_group;

			operator_group = MethodLookup (ec, e.Type, is_true ? "op_True" : "op_False", loc);
			if (operator_group == null)
				return null;

			ArrayList arguments = new ArrayList ();
			arguments.Add (new Argument (e, Argument.AType.Expression));
			method = Invocation.OverloadResolve (
				ec, (MethodGroupExpr) operator_group, arguments, false, loc);

			if (method == null)
				return null;

			return new StaticCallExpr ((MethodInfo) method, arguments, loc);
		}

		/// <summary>
		///   Resolves the expression `e' into a boolean expression: either through
		///   an implicit conversion, or through an `operator true' invocation
		/// </summary>
		public static Expression ResolveBoolean (EmitContext ec, Expression e, Location loc)
		{
			e = e.Resolve (ec);
			if (e == null)
				return null;

			Expression converted = e;
			if (e.Type != TypeManager.bool_type)
				converted = Convert.ImplicitConversion (ec, e, TypeManager.bool_type, new Location (-1));

			//
			// If no implicit conversion to bool exists, try using `operator true'
			//
			if (converted == null){
				Expression operator_true = Expression.GetOperatorTrue (ec, e, loc);
				if (operator_true == null){
					Report.Error (
						31, loc, "Can not convert the expression to a boolean");
					return null;
				}
				e = operator_true;
			} else
				e = converted;

			return e;
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
		///   Reports that we were expecting `expr' to be of class `expected'
		/// </summary>
		public void Error_UnexpectedKind (string expected, Location loc)
		{
			string kind = "Unknown";
			
			kind = ExprClassName (eclass);

			Report.Error (118, loc, "Expression denotes a `" + kind +
			       "' where a `" + expected + "' was expected");
		}

		public void Error_UnexpectedKind (ResolveFlags flags, Location loc)
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

			Error (119, "Expression denotes a `" + kind + "' where " +
			       "a `" + sb.ToString () + "' was expected");
		}
		
		static public void Error_ConstantValueCannotBeConverted (Location l, string val, Type t)
		{
			Report.Error (31, l, "Constant value `" + val + "' cannot be converted to " +
				      TypeManager.CSharpName (t));
		}

		public static void UnsafeError (Location loc)
		{
			Report.Error (214, loc, "Pointers may only be used in an unsafe context");
		}
		
		/// <summary>
		///   Converts the IntConstant, UIntConstant, LongConstant or
		///   ULongConstant into the integral target_type.   Notice
		///   that we do not return an `Expression' we do return
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
			if (TypeManager.IsEnumType (target_type))
				target_type = TypeManager.EnumToUnderlying (target_type);

			//
			// Make into one of the literals we handle, we dont really care
			// about this value as we will just return a few limited types
			// 
			if (c is EnumConstant)
				c = ((EnumConstant)c).WidenToCompilerConstant ();
			
			if (!Convert.ImplicitStandardConversionExists (c, target_type)){
				Convert.Error_CannotImplicitConversion (loc, c.Type, target_type);
				return null;
			}
			
			string s = "";

			if (c.Type == target_type)
				return ((Constant) c).GetValue ();


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
			else if (t.IsPointer)
				ig.Emit (OpCodes.Ldind_I);
			else 
				ig.Emit (OpCodes.Ldind_Ref);
		}

		//
		// The stack contains the pointer and the value of type `type'
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
		// Returns the size of type `t' if known, otherwise, 0
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

		public static void Error_NegativeArrayIndex (Location loc)
		{
			Report.Error (248, loc, "Cannot create an array with a negative size");
		}
		
		//
		// Converts `source' to an int, uint, long or ulong.
		//
		public Expression ExpressionToArrayArgument (EmitContext ec, Expression source, Location loc)
		{
			Expression target;
			
			bool old_checked = ec.CheckState;
			ec.CheckState = true;
			
			target = Convert.ImplicitConversion (ec, source, TypeManager.int32_type, loc);
			if (target == null){
				target = Convert.ImplicitConversion (ec, source, TypeManager.uint32_type, loc);
				if (target == null){
					target = Convert.ImplicitConversion (ec, source, TypeManager.int64_type, loc);
					if (target == null){
						target = Convert.ImplicitConversion (ec, source, TypeManager.uint64_type, loc);
						if (target == null)
							Convert.Error_CannotImplicitConversion (loc, source.Type, TypeManager.int32_type);
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

		public virtual ExpressionStatement ResolveStatement (EmitContext ec)
		{
			Expression e = Resolve (ec);
			if (e == null)
				return null;

			ExpressionStatement es = e as ExpressionStatement;
			if (es == null)
				Error (201, "Only assignment, call, increment, decrement and new object " +
				       "expressions can be used as a statement");

			return es;
		}

		/// <summary>
		///   Requests the expression to be emitted in a `statement'
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

		public Expression Child {
			get {
				return child;
			}
		}		

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

        //
	// We need to special case this since an empty cast of
	// a NullLiteral is still a Constant
	//
	public class NullCast : Constant {
		protected Expression child;
				
		public NullCast (Expression child, Type return_type)
		{
			eclass = child.eclass;
			type = return_type;
			this.child = child;
		}

		override public string AsString ()
		{
			return "null";
		}

		public override object GetValue ()
		{
			return null;
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

		public override bool IsNegative {
			get {
				return false;
			}
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

		public object GetValueAsEnumType ()
		{
			return System.Enum.ToObject (type, Child.GetValue ());
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
		
		public override bool IsZeroInteger {
			get { return Child.IsZeroInteger; }
		}

		public override bool IsNegative {
			get {
				return Child.IsNegative;
			}
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
			eclass = ExprClass.Value;
		}

		public BoxedCast (Expression expr, Type target_type)
			: base (expr, target_type)
		{
			eclass = ExprClass.Value;
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

		public override string ToString ()
		{
			return String.Format ("ConvCast ({0}, {1})", mode, child);
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
	///   that this is slower, we should create a `NamespaceExpr' expression
	///   that fully participates in the resolution process. 
	///   
	///   For example `System.Console.WriteLine' is decomposed into
	///   MemberAccess (MemberAccess (SimpleName ("System"), "Console"), "WriteLine")
	///   
	///   The first SimpleName wont produce a match on its own, so it will
	///   be turned into:
	///   MemberAccess (SimpleName ("System.Console"), "WriteLine").
	///   
	///   System.Console will produce a TypeExpr match.
	///   
	///   The downside of this is that we might be hitting `LookupType' too many
	///   times with this scheme.
	/// </remarks>
	public class SimpleName : Expression {
		public string Name;

		//
		// If true, then we are a simple name, not composed with a ".
		//
		bool is_base;

		public SimpleName (string a, string b, Location l)
		{
			Name = String.Concat (a, ".", b);
			loc = l;
			is_base = false;
		}
		
		public SimpleName (string name, Location l)
		{
			Name = name;
			loc = l;
			is_base = true;
		}

		public static void Error_ObjectRefRequired (EmitContext ec, Location l, string name)
		{
			if (ec.IsFieldInitializer)
				Report.Error (
					236, l,
					"A field initializer cannot reference the non-static field, " +
					"method or property `"+name+"'");
			else
				Report.Error (
					120, l,
					"An object reference is required " +
					"for the non-static field `"+name+"'");
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
			return SimpleNameResolve (ec, null, false, false);
		}

		public override Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			return SimpleNameResolve (ec, right_side, false, false);
		}
		

		public Expression DoResolveAllowStatic (EmitContext ec, bool intermediate)
		{
			return SimpleNameResolve (ec, null, true, intermediate);
		}

		public override Expression ResolveAsTypeStep (EmitContext ec)
		{
			DeclSpace ds = ec.DeclSpace;
			NamespaceEntry ns = ds.NamespaceEntry;
			Type t;
			string alias_value;

			//
			// Since we are cheating: we only do the Alias lookup for
			// namespaces if the name does not include any dots in it
			//
			if (ns != null && is_base)
				alias_value = ns.LookupAlias (Name);
			else
				alias_value = null;

			if (ec.ResolvingTypeTree){
				int errors = Report.Errors;
				Type dt = ds.FindType (loc, Name);

				if (Report.Errors != errors)
					return null;
				
				if (dt != null)
					return new TypeExpression (dt, loc);

				if (alias_value != null){
					if ((t = RootContext.LookupType (ds, alias_value, true, loc)) != null)
						return new TypeExpression (t, loc);
				}
			}

			if ((t = RootContext.LookupType (ds, Name, true, loc)) != null)
				return new TypeExpression (t, loc);

			if (alias_value != null) {
				if ((t = RootContext.LookupType (ds, alias_value, true, loc)) != null)
					return new TypeExpression (t, loc);
				
				// we have alias value, but it isn't Type, so try if it's namespace
				return new SimpleName (alias_value, loc);
			}

			// No match, maybe our parent can compose us
			// into something meaningful.
			return this;
		}

		Expression SimpleNameResolve (EmitContext ec, Expression right_side,
					      bool allow_static, bool intermediate)
		{
			Expression e = DoSimpleNameResolve (ec, right_side, allow_static, intermediate);
			if (e == null)
				return null;

			Block current_block = ec.CurrentBlock;
			if (current_block != null){
				//LocalInfo vi = current_block.GetLocalInfo (Name);
				if (is_base &&
				    current_block.IsVariableNameUsedInChildBlock(Name)) {
					Report.Error (135, Location,
						      "'{0}' has a different meaning in a " +
						      "child block", Name);
					return null;
				}
			}

			return e;
		}

		/// <remarks>
		///   7.5.2: Simple Names. 
		///
		///   Local Variables and Parameters are handled at
		///   parse time, so they never occur as SimpleNames.
		///
		///   The `allow_static' flag is used by MemberAccess only
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
		Expression DoSimpleNameResolve (EmitContext ec, Expression right_side, bool allow_static, bool intermediate)
		{
			Expression e = null;

			//
			// Stage 1: Performed by the parser (binding to locals or parameters).
			//
			Block current_block = ec.CurrentBlock;
			if (current_block != null){
				LocalInfo vi = current_block.GetLocalInfo (Name);
				if (vi != null){
					Expression var;
					
					var = new LocalVariableReference (ec.CurrentBlock, Name, loc);
					
					if (right_side != null)
						return var.ResolveLValue (ec, right_side);
					else
						return var.Resolve (ec);
				}

				int idx = -1;
				Parameter par = null;
				Parameters pars = current_block.Parameters;
				if (pars != null)
					par = pars.GetParameterByName (Name, out idx);

				if (par != null) {
					ParameterReference param;
					
					param = new ParameterReference (pars, current_block, idx, Name, loc);

					if (right_side != null)
						return param.ResolveLValue (ec, right_side);
					else
						return param.Resolve (ec);
				}
			}
			
			//
			// Stage 2: Lookup members 
			//

			DeclSpace lookup_ds = ec.DeclSpace;
			do {
				if (lookup_ds.TypeBuilder == null)
					break;

				e = MemberLookup (ec, lookup_ds.TypeBuilder, Name, loc);
				if (e != null)
					break;

				lookup_ds =lookup_ds.Parent;
			} while (lookup_ds != null);
				
			if (e == null && ec.ContainerType != null)
				e = MemberLookup (ec, ec.ContainerType, Name, loc);

			if (e == null) {
				//
				// Since we are cheating (is_base is our hint
				// that we are the beginning of the name): we
				// only do the Alias lookup for namespaces if
				// the name does not include any dots in it
				//
				NamespaceEntry ns = ec.DeclSpace.NamespaceEntry;
				if (is_base && ns != null){
					string alias_value = ns.LookupAlias (Name);
					if (alias_value != null){
						Name = alias_value;
						Type t;

						if ((t = TypeManager.LookupType (Name)) != null)
							return new TypeExpression (t, loc);
					
						// No match, maybe our parent can compose us
						// into something meaningful.
						return this;
					}
				}

				return ResolveAsTypeStep (ec);
			}

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
				
				if (!me.IsStatic &&
				    TypeManager.IsSubclassOrNestedChildOf (me.InstanceExpression.Type, me.DeclaringType) &&
				    me.InstanceExpression.Type != me.DeclaringType &&
				    !me.InstanceExpression.Type.IsSubclassOf (me.DeclaringType) &&
				    (!intermediate || !MemberAccess.IdenticalNameAndTypeName (ec, this, e, loc))) {
					Error (38, "Cannot access nonstatic member `" + me.Name + "' of " +
					       "outer type `" + me.DeclaringType + "' via nested type `" +
					       me.InstanceExpression.Type + "'");
					return null;
				}

				return (right_side != null)
					? e.DoResolveLValue (ec, right_side)
					: e.DoResolve (ec);
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

			Error (103, "The name `" + Name +
			       "' does not exist in the class `" +
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
	public abstract class TypeExpr : Expression {
		override public Expression ResolveAsTypeStep (EmitContext ec)
		{
			TypeExpr t = DoResolveAsTypeStep (ec);
			if (t == null)
				return null;

			eclass = ExprClass.Type;
			return t;
		}

		override public Expression DoResolve (EmitContext ec)
		{
			return ResolveAsTypeTerminal (ec, false);
		}

		override public void Emit (EmitContext ec)
		{
			throw new Exception ("Should never be called");
		}

		public virtual bool CheckAccessLevel (DeclSpace ds)
		{
			return ds.CheckAccessLevel (Type);
		}

		public virtual bool AsAccessible (DeclSpace ds, int flags)
		{
			return ds.AsAccessible (Type, flags);
		}

		public virtual bool IsClass {
			get { return Type.IsClass; }
		}

		public virtual bool IsValueType {
			get { return Type.IsValueType; }
		}

		public virtual bool IsInterface {
			get { return Type.IsInterface; }
		}

		public virtual bool IsSealed {
			get { return Type.IsSealed; }
		}

		public virtual bool CanInheritFrom ()
		{
			if (Type == TypeManager.enum_type ||
			    (Type == TypeManager.value_type && RootContext.StdLib) ||
			    Type == TypeManager.multicast_delegate_type ||
			    Type == TypeManager.delegate_type ||
			    Type == TypeManager.array_type)
				return false;

			return true;
		}

		public virtual bool IsAttribute {
			get {
				return Type == TypeManager.attribute_type ||
					Type.IsSubclassOf (TypeManager.attribute_type);
			}
		}

		public virtual TypeExpr[] GetInterfaces ()
		{
			return TypeManager.GetInterfaces (Type);
		}

		public abstract TypeExpr DoResolveAsTypeStep (EmitContext ec);

		public virtual Type ResolveType (EmitContext ec)
		{
			TypeExpr t = ResolveAsTypeTerminal (ec, false);
			if (t == null)
				return null;

			return t.Type;
		}

		public abstract string Name {
			get;
		}

		public override bool Equals (object obj)
		{
			TypeExpr tobj = obj as TypeExpr;
			if (tobj == null)
				return false;

			return Type == tobj.Type;
		}

		public override int GetHashCode ()
		{
			return Type.GetHashCode ();
		}
		
		public override string ToString ()
		{
			return Name;
		}
	}

	public class TypeExpression : TypeExpr {
		public TypeExpression (Type t, Location l)
		{
			Type = t;
			eclass = ExprClass.Type;
			loc = l;
		}

		public override TypeExpr DoResolveAsTypeStep (EmitContext ec)
		{
			return this;
		}

		public override string Name {
			get {
				return Type.ToString ();
			}
		}
	}

	/// <summary>
	///   Used to create types from a fully qualified name.  These are just used
	///   by the parser to setup the core types.  A TypeLookupExpression is always
	///   classified as a type.
	/// </summary>
	public class TypeLookupExpression : TypeExpr {
		string name;
		
		public TypeLookupExpression (string name)
		{
			this.name = name;
		}

		public override TypeExpr DoResolveAsTypeStep (EmitContext ec)
		{
			if (type == null)
				type = RootContext.LookupType (ec.DeclSpace, name, false, Location.Null);
			return this;
		}

		public override string Name {
			get {
				return name;
			}
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
		bool identical_type_name = false;
		bool is_base;
		
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
                                //
                                // The methods are arranged in this order:
                                // derived type -> base type
                                //
				return Methods [0].DeclaringType;
			}
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

		public bool IsExplicitImpl {
			get {
				return is_explicit_impl;
			}

			set {
				is_explicit_impl = value;
			}
		}

		public bool IdenticalTypeName {
			get {
				return identical_type_name;
			}

			set {
				identical_type_name = value;
			}
		}
		
		public bool IsBase {
			get {
				return is_base;
			}
			set {
				is_base = value;
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
			if (!IsInstance)
				instance_expression = null;

			if (instance_expression != null) {
				instance_expression = instance_expression.DoResolve (ec);
				if (instance_expression == null)
					return null;
			}

			return this;
		}

		public void ReportUsageError ()
		{
			Report.Error (654, loc, "Method `" + DeclaringType + "." +
				      Name + "()' is referenced without parentheses");
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
	public class FieldExpr : Expression, IAssignMethod, IMemoryLocation, IMemberExpr, IVariable {
		public readonly FieldInfo FieldInfo;
		Expression instance_expr;
		VariableInfo variable_info;

		LocalTemporary temp;
		bool prepared;
		
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

		public VariableInfo VariableInfo {
			get {
				return variable_info;
			}
		}

		override public Expression DoResolve (EmitContext ec)
		{
			if (!FieldInfo.IsStatic){
				if (instance_expr == null){
					//
					// This can happen when referencing an instance field using
					// a fully qualified type expression: TypeName.InstanceField = xxx
					// 
					SimpleName.Error_ObjectRefRequired (ec, loc, FieldInfo.Name);
					return null;
				}

				// Resolve the field's instance expression while flow analysis is turned
				// off: when accessing a field "a.b", we must check whether the field
				// "a.b" is initialized, not whether the whole struct "a" is initialized.
				instance_expr = instance_expr.Resolve (ec, ResolveFlags.VariableOrValue |
								       ResolveFlags.DisableFlowAnalysis);
				if (instance_expr == null)
					return null;
			}

			ObsoleteAttribute oa;
			FieldBase f = TypeManager.GetField (FieldInfo);
			if (f != null) {
				oa = f.GetObsoleteAttribute (f.Parent);
				if (oa != null)
					AttributeTester.Report_ObsoleteMessage (oa, f.GetSignatureForError (), loc);
                                
                        // To be sure that type is external because we do not register generated fields
                        } else if (!(FieldInfo.DeclaringType is TypeBuilder)) {                                
				oa = AttributeTester.GetMemberObsoleteAttribute (FieldInfo);
				if (oa != null)
					AttributeTester.Report_ObsoleteMessage (oa, TypeManager.GetFullNameSignature (FieldInfo), loc);
			}

			// If the instance expression is a local variable or parameter.
			IVariable var = instance_expr as IVariable;
			if ((var == null) || (var.VariableInfo == null))
				return this;

			VariableInfo vi = var.VariableInfo;
			if (!vi.IsFieldAssigned (ec, FieldInfo.Name, loc))
				return null;

			variable_info = vi.GetSubStruct (FieldInfo.Name);
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
			if ((var != null) && (var.VariableInfo != null))
				var.VariableInfo.SetFieldAssigned (ec, FieldInfo.Name);

			Expression e = DoResolve (ec);

			if (e == null)
				return null;

			if (!FieldInfo.IsStatic && (instance_expr.Type.IsValueType && !(instance_expr is IMemoryLocation))) {
				// FIXME: Provide better error reporting.
				Error (1612, "Cannot modify expression because it is not a variable.");
				return null;
			}

			if (!FieldInfo.IsInitOnly)
				return this;

			FieldBase fb = TypeManager.GetField (FieldInfo);
			if (fb != null)
				fb.SetAssigned ();

			//
			// InitOnly fields can only be assigned in constructors
			//

			if (ec.IsConstructor){
				if (IsStatic && !ec.IsStatic)
					Report_AssignToReadonly (false);

				if (ec.ContainerType == FieldInfo.DeclaringType)
					return this;
			}

			Report_AssignToReadonly (!IsStatic);
			
			return null;
		}

		public bool VerifyFixed (bool is_expression)
		{
			IVariable variable = instance_expr as IVariable;
			if ((variable == null) || !variable.VerifyFixed (true))
				return false;

			return true;
		}
		
		public void Emit (EmitContext ec, bool leave_copy)
		{
			ILGenerator ig = ec.ig;
			bool is_volatile = false;

			if (FieldInfo is FieldBuilder){
				FieldBase f = TypeManager.GetField (FieldInfo);
				if (f != null){
					if ((f.ModFlags & Modifiers.VOLATILE) != 0)
						is_volatile = true;
					
					f.status |= Field.Status.USED;
				}
			} 
			
			if (FieldInfo.IsStatic){
				if (is_volatile)
					ig.Emit (OpCodes.Volatile);
				
				ig.Emit (OpCodes.Ldsfld, FieldInfo);
			} else {
				if (!prepared)
					EmitInstance (ec);
				
				if (is_volatile)
					ig.Emit (OpCodes.Volatile);
				
				ig.Emit (OpCodes.Ldfld, FieldInfo);
			}
			
			if (leave_copy) {	
				ec.ig.Emit (OpCodes.Dup);
				if (!FieldInfo.IsStatic) {
					temp = new LocalTemporary (ec, this.Type);
					temp.Store (ec);
				}
			}
		}
		
		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			FieldAttributes fa = FieldInfo.Attributes;
			bool is_static = (fa & FieldAttributes.Static) != 0;
			bool is_readonly = (fa & FieldAttributes.InitOnly) != 0;
			ILGenerator ig = ec.ig;
			prepared = prepare_for_load;

			if (is_readonly && !ec.IsConstructor){
				Report_AssignToReadonly (!is_static);
				return;
			}

			if (!is_static) {
				EmitInstance (ec);
				if (prepare_for_load)
					ig.Emit (OpCodes.Dup);
			}

			source.Emit (ec);
			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				if (!FieldInfo.IsStatic) {
					temp = new LocalTemporary (ec, this.Type);
					temp.Store (ec);
				}
			}

			if (FieldInfo is FieldBuilder){
				FieldBase f = TypeManager.GetField (FieldInfo);
				if (f != null){
					if ((f.ModFlags & Modifiers.VOLATILE) != 0)
						ig.Emit (OpCodes.Volatile);
					
					f.status |= Field.Status.ASSIGNED;
				}
			} 

			if (is_static)
				ig.Emit (OpCodes.Stsfld, FieldInfo);
			else 
				ig.Emit (OpCodes.Stfld, FieldInfo);
			
			if (temp != null)
				temp.Emit (ec);
		}

		void EmitInstance (EmitContext ec)
		{
			if (instance_expr.Type.IsValueType) {
				if (instance_expr is IMemoryLocation) {
					((IMemoryLocation) instance_expr).AddressOf (ec, AddressOp.LoadStore);
				} else {
					LocalTemporary t = new LocalTemporary (ec, instance_expr.Type);
					instance_expr.Emit (ec);
					t.Store (ec);
					t.AddressOf (ec, AddressOp.Store);
				}
			} else
				instance_expr.Emit (ec);
		}

		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}
		
		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			ILGenerator ig = ec.ig;
			
			if (FieldInfo is FieldBuilder){
				FieldBase f = TypeManager.GetField (FieldInfo);
				if (f != null){
					if ((f.ModFlags & Modifiers.VOLATILE) != 0){
						Error (676, "volatile variable: can not take its address, or pass as ref/out parameter");
						return;
					}
					
					if ((mode & AddressOp.Store) != 0)
						f.status |= Field.Status.ASSIGNED;
					if ((mode & AddressOp.Load) != 0)
						f.status |= Field.Status.USED;
				}
			} 

			//
			// Handle initonly fields specially: make a copy and then
			// get the address of the copy.
			//
			bool need_copy;
			if (FieldInfo.IsInitOnly){
				need_copy = true;
				if (ec.IsConstructor){
					if (FieldInfo.IsStatic){
						if (ec.IsStatic)
							need_copy = false;
					} else
						need_copy = false;
				}
			} else
				need_copy = false;
			
			if (need_copy){
				LocalBuilder local;
				Emit (ec);
				local = ig.DeclareLocal (type);
				ig.Emit (OpCodes.Stloc, local);
				ig.Emit (OpCodes.Ldloca, local);
				return;
			}


			if (FieldInfo.IsStatic){
				ig.Emit (OpCodes.Ldsflda, FieldInfo);
			} else {
				EmitInstance (ec);
				ig.Emit (OpCodes.Ldflda, FieldInfo);
			}
		}
	}

	//
	// A FieldExpr whose address can not be taken
	//
	public class FieldExprNoAddress : FieldExpr, IMemoryLocation {
		public FieldExprNoAddress (FieldInfo fi, Location loc) : base (fi, loc)
		{
		}
		
		public new void AddressOf (EmitContext ec, AddressOp mode)
		{
			Report.Error (-215, "Report this: Taking the address of a remapped parameter not supported");
		}
	}
	
	/// <summary>
	///   Expression that evaluates to a Property.  The Assign class
	///   might set the `Value' expression if we are in an assignment.
	///
	///   This is not an LValue because we need to re-write the expression, we
	///   can not take data from the stack and store it.  
	/// </summary>
	public class PropertyExpr : ExpressionStatement, IAssignMethod, IMemberExpr {
		public readonly PropertyInfo PropertyInfo;

		//
		// This is set externally by the  `BaseAccess' class
		//
		public bool IsBase;
		MethodInfo getter, setter;
		bool is_static;
		bool must_do_cs1540_check;
		
		Expression instance_expr;
		LocalTemporary temp;
		bool prepared;

		public PropertyExpr (EmitContext ec, PropertyInfo pi, Location l)
		{
			PropertyInfo = pi;
			eclass = ExprClass.PropertyAccess;
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
			if (setter == null) {
				Report.Error (200, loc, 
					      "The property `" + PropertyInfo.Name +
					      "' can not be assigned to, as it has not set accessor");
				return false;
			}

			return true;
		}

		void FindAccessors (Type invocation_type)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.Static | BindingFlags.Instance |
				BindingFlags.DeclaredOnly;

			Type current = PropertyInfo.DeclaringType;
			for (; current != null; current = current.BaseType) {
				MemberInfo[] group = TypeManager.MemberLookup (
					invocation_type, invocation_type, current,
					MemberTypes.Property, flags, PropertyInfo.Name, null);

				if (group == null)
					continue;

				if (group.Length != 1)
					// Oooops, can this ever happen ?
					return;

				PropertyInfo pi = (PropertyInfo) group [0];

				if (getter == null)
					getter = pi.GetGetMethod (true);;

				if (setter == null)
					setter = pi.GetSetMethod (true);;

				MethodInfo accessor = getter != null ? getter : setter;

				if (!accessor.IsVirtual)
					return;
			}
		}

		bool IsAccessorAccessible (Type invocation_type, MethodInfo mi)
		{
			MethodAttributes ma = mi.Attributes & MethodAttributes.MemberAccessMask;

			//
			// If only accessible to the current class or children
			//
			if (ma == MethodAttributes.Private) {
				Type declaring_type = mi.DeclaringType;
					
				if (invocation_type != declaring_type)
					return TypeManager.IsSubclassOrNestedChildOf (invocation_type, declaring_type);

				return true;
			}
			//
			// FamAndAssem requires that we not only derivate, but we are on the
			// same assembly.  
			//
			if (ma == MethodAttributes.FamANDAssem){
				return (mi.DeclaringType.Assembly != invocation_type.Assembly);
			}

			// Assembly and FamORAssem succeed if we're in the same assembly.
			if ((ma == MethodAttributes.Assembly) || (ma == MethodAttributes.FamORAssem)){
				if (mi.DeclaringType.Assembly == invocation_type.Assembly)
					return true;
			}

			// We already know that we aren't in the same assembly.
			if (ma == MethodAttributes.Assembly)
				return false;

			// Family and FamANDAssem require that we derive.
			if ((ma == MethodAttributes.Family) || (ma == MethodAttributes.FamANDAssem) || (ma == MethodAttributes.FamORAssem)){
				if (!TypeManager.IsSubclassOrNestedChildOf (invocation_type, mi.DeclaringType))
					return false;

				if (!TypeManager.IsNestedChildOf (invocation_type, mi.DeclaringType))
					must_do_cs1540_check = true;

				return true;
			}

			return true;
		}

		//
		// We also perform the permission checking here, as the PropertyInfo does not
		// hold the information for the accessibility of its setter/getter
		//
		void ResolveAccessors (EmitContext ec)
		{
			FindAccessors (ec.ContainerType);

			if (setter != null && !IsAccessorAccessible (ec.ContainerType, setter) ||
				getter != null && !IsAccessorAccessible (ec.ContainerType, getter)) {
				Report.Error (122, loc, "'{0}' is inaccessible due to its protection level", PropertyInfo.Name);
			}

			is_static = getter != null ? getter.IsStatic : setter.IsStatic;
		}

		bool InstanceResolve (EmitContext ec)
		{
			if ((instance_expr == null) && ec.IsStatic && !is_static) {
				SimpleName.Error_ObjectRefRequired (ec, loc, PropertyInfo.Name);
				return false;
			}

			if (instance_expr != null) {
				instance_expr = instance_expr.DoResolve (ec);
				if (instance_expr == null)
					return false;
			}

			if (must_do_cs1540_check && (instance_expr != null)) {
				if ((instance_expr.Type != ec.ContainerType) &&
				    ec.ContainerType.IsSubclassOf (instance_expr.Type)) {
					Report.Error (1540, loc, "Cannot access protected member `" +
						      PropertyInfo.DeclaringType + "." + PropertyInfo.Name + 
						      "' via a qualifier of type `" +
						      TypeManager.CSharpName (instance_expr.Type) +
						      "'; the qualifier must be of type `" +
						      TypeManager.CSharpName (ec.ContainerType) +
						      "' (or derived from it)");
					return false;
				}
			}

			return true;
		}
		
		override public Expression DoResolve (EmitContext ec)
		{
			if (getter != null){
				if (TypeManager.GetArgumentTypes (getter).Length != 0){
					Report.Error (
						117, loc, "`{0}' does not contain a " +
						"definition for `{1}'.", getter.DeclaringType,
						Name);
					return null;
				}
			}

			if (getter == null){
				//
				// The following condition happens if the PropertyExpr was
				// created, but is invalid (ie, the property is inaccessible),
				// and we did not want to embed the knowledge about this in
				// the caller routine.  This only avoids double error reporting.
				//
				if (setter == null)
					return null;
				
				Report.Error (154, loc, 
					      "The property `" + PropertyInfo.Name +
					      "' can not be used in " +
					      "this context because it lacks a get accessor");
				return null;
			} 

			if (!InstanceResolve (ec))
				return null;

			//
			// Only base will allow this invocation to happen.
			//
			if (IsBase && getter.IsAbstract){
				Report.Error (205, loc, "Cannot call an abstract base property: " +
					      PropertyInfo.DeclaringType + "." +PropertyInfo.Name);
				return null;
			}

			return this;
		}

		override public Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			if (setter == null){
				//
				// The following condition happens if the PropertyExpr was
				// created, but is invalid (ie, the property is inaccessible),
				// and we did not want to embed the knowledge about this in
				// the caller routine.  This only avoids double error reporting.
				//
				if (getter == null)
					return null;
				
				Report.Error (154, loc, 
					      "The property `" + PropertyInfo.Name +
					      "' can not be used in " +
					      "this context because it lacks a set accessor");
				return null;
			}

			if (TypeManager.GetArgumentTypes (setter).Length != 1){
				Report.Error (
					117, loc, "`{0}' does not contain a " +
					"definition for `{1}'.", getter.DeclaringType,
					Name);
				return null;
			}

			if (!InstanceResolve (ec))
				return null;
			
			//
			// Only base will allow this invocation to happen.
			//
			if (IsBase && setter.IsAbstract){
				Report.Error (205, loc, "Cannot call an abstract base property: " +
					      PropertyInfo.DeclaringType + "." +PropertyInfo.Name);
				return null;
			}

			//
			// Check that we are not making changes to a temporary memory location
			//
			if (instance_expr != null && instance_expr.Type.IsValueType && !(instance_expr is IMemoryLocation)) {
				// FIXME: Provide better error reporting.
				Error (1612, "Cannot modify expression because it is not a variable.");
				return null;
			}

			return this;
		}


		
		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}
		
		void EmitInstance (EmitContext ec)
		{
			if (is_static)
				return;

			if (instance_expr.Type.IsValueType) {
				if (instance_expr is IMemoryLocation) {
					((IMemoryLocation) instance_expr).AddressOf (ec, AddressOp.LoadStore);
				} else {
					LocalTemporary t = new LocalTemporary (ec, instance_expr.Type);
					instance_expr.Emit (ec);
					t.Store (ec);
					t.AddressOf (ec, AddressOp.Store);
				}
			} else
				instance_expr.Emit (ec);
			
			if (prepared)
				ec.ig.Emit (OpCodes.Dup);
		}

		
		public void Emit (EmitContext ec, bool leave_copy)
		{
			if (!prepared)
				EmitInstance (ec);
			
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
				if (iet != TypeManager.array_type && (iet.GetArrayRank () == 1)) {
					ec.ig.Emit (OpCodes.Ldlen);
					ec.ig.Emit (OpCodes.Conv_I4);
					return;
				}
			}

			Invocation.EmitCall (ec, IsBase, IsStatic, new EmptyAddressOf (), getter, null, loc);
			
			if (!leave_copy)
				return;
			
			ec.ig.Emit (OpCodes.Dup);
			if (!is_static) {
				temp = new LocalTemporary (ec, this.Type);
				temp.Store (ec);
			}
		}

		//
		// Implements the IAssignMethod interface for assignments
		//
		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			prepared = prepare_for_load;
			
			EmitInstance (ec);

			source.Emit (ec);
			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				if (!is_static) {
					temp = new LocalTemporary (ec, this.Type);
					temp.Store (ec);
				}
			}
			
			ArrayList args = new ArrayList (1);
			args.Add (new Argument (new EmptyAddressOf (), Argument.AType.Expression));
			
			Invocation.EmitCall (ec, IsBase, IsStatic, new EmptyAddressOf (), setter, args, loc);
			
			if (temp != null)
				temp.Emit (ec);
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
		Expression instance_expr;

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

			if (EventInfo is MyEventBuilder){
				MyEventBuilder eb = (MyEventBuilder) EventInfo;
				type = eb.EventType;
				eb.SetUsed ();
			} else
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
			Report.Error (70, loc, "The event `" + Name + "' can only appear on the left hand side of += or -= (except on the defining type)");
		}

		public void EmitAddOrRemove (EmitContext ec, Expression source)
		{
			BinaryDelegate source_del = (BinaryDelegate) source;
			Expression handler = source_del.Right;
			
			Argument arg = new Argument (handler, Argument.AType.Expression);
			ArrayList args = new ArrayList ();
				
			args.Add (arg);
			
			if (source_del.IsAddition)
				Invocation.EmitCall (
					ec, false, IsStatic, instance_expr, add_accessor, args, loc);
			else
				Invocation.EmitCall (
					ec, false, IsStatic, instance_expr, remove_accessor, args, loc);
		}
	}
}	
