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

		// Mask of all the expression class flags.
		MaskExprClass		= 7,

		// Disable control flow analysis while resolving the expression.
		// This is used when resolving the instance expression of a field expression.
		DisableFlowAnalysis	= 8,

		// Set if this is resolving the first part of a MemberAccess.
		Intermediate		= 16,

		// Disable control flow analysis _of struct_ while resolving the expression.
		// This is used when resolving the instance expression of a field expression.
		DisableStructFlowAnalysis	= 32,

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

		bool VerifyFixed ();
	}

	/// <remarks>
	///   Base class for expressions
	/// </remarks>
	public abstract class Expression {
		public ExprClass eclass;
		protected Type type;
		protected Location loc;
		
		public Type Type {
			get { return type; }
			set { type = value; }
		}

		public virtual Location Location {
			get { return loc; }
		}

		/// <summary>
		///   Utility wrapper routine for Error, just to beautify the code
		/// </summary>
		public void Error (int error, string s)
		{
			if (loc.IsNull)
				Report.Error (error, s);
			else
				Report.Error (error, loc, s);
		}

		// Not nice but we have broken hierarchy
		public virtual void CheckMarshallByRefAccess (Type container) {}

		public virtual string GetSignatureForError ()
		{
			return TypeManager.CSharpName (type);
		}

		public static bool IsAccessorAccessible (Type invocation_type, MethodInfo mi, out bool must_do_cs1540_check)
		{
			MethodAttributes ma = mi.Attributes & MethodAttributes.MemberAccessMask;

			must_do_cs1540_check = false; // by default we do not check for this

			if (ma == MethodAttributes.Public)
				return true;
			
			//
			// If only accessible to the current class or children
			//
			if (ma == MethodAttributes.Private)
				return TypeManager.IsPrivateAccessible (invocation_type, mi.DeclaringType) ||
					TypeManager.IsNestedChildOf (invocation_type, mi.DeclaringType);

			if (mi.DeclaringType.Assembly == invocation_type.Assembly ||
					TypeManager.IsFriendAssembly (mi.DeclaringType.Assembly)) {
				if (ma == MethodAttributes.Assembly || ma == MethodAttributes.FamORAssem)
					return true;
			} else {
				if (ma == MethodAttributes.Assembly || ma == MethodAttributes.FamANDAssem)
					return false;
			}

			// Family and FamANDAssem require that we derive.
			// FamORAssem requires that we derive if in different assemblies.
			if (!TypeManager.IsNestedFamilyAccessible (invocation_type, mi.DeclaringType))
				return false;

			if (!TypeManager.IsNestedChildOf (invocation_type, mi.DeclaringType))
				must_do_cs1540_check = true;

			return true;
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
			return null;
		}

		//
		// This is used if the expression should be resolved as a type or namespace name.
		// the default implementation fails.   
		//
		public FullNamedExpression ResolveAsTypeStep (EmitContext ec)
		{
			return ResolveAsTypeStep (ec, false);
		}

		public virtual FullNamedExpression ResolveAsTypeStep (EmitContext ec,  bool silent)
		{
			return null;
		}

		//
		// This is used to resolve the expression as a type, a null
		// value will be returned if the expression is not a type
		// reference
		//
		public TypeExpr ResolveAsTypeTerminal (EmitContext ec)
		{
			return ResolveAsTypeTerminal (ec, false);
		}

		public virtual TypeExpr ResolveAsTypeTerminal (EmitContext ec, bool silent)
		{
			int errors = Report.Errors;

			FullNamedExpression fne = ResolveAsTypeStep (ec, silent);

			if (fne == null){
				if (!silent && errors == Report.Errors)
					Report.Error (118, loc, "Expecting a type.");
				return null;
			}

			if (fne.eclass != ExprClass.Type) {
				if (!silent && (errors == Report.Errors))
					fne.Error_UnexpectedKind (null, "type", loc);
				return null;
			}

			TypeExpr te = fne as TypeExpr;

			if (!te.CheckAccessLevel (ec.DeclSpace)) {
				ErrorIsInaccesible (loc, TypeManager.CSharpName (te.Type));
				return null;
			}

			ConstructedType ct = te as ConstructedType;
			if ((ct != null) && !ec.ResolvingTypeTree && !ec.ResolvingGenericMethod &&
			    !ct.CheckConstraints (ec))
				return null;

			te.loc = loc;
			return te;
		}

		public static void ErrorIsInaccesible (Location loc, string name)
		{
			Report.Error (122, loc, "`{0}' is inaccessible due to its protection level", name);
		}

		protected static void Error_CannotAccessProtected (Location loc, MemberInfo m, Type qualifier, Type container)
		{
			Report.Error (1540, loc, "Cannot access protected member `{0}' via a qualifier of type `{1}';"
				+ " the qualifier must be of type `{2}' (or derived from it)", 
				TypeManager.GetFullNameSignature (m),
				TypeManager.CSharpName (qualifier),
				TypeManager.CSharpName (container));

		}

		public virtual void Error_ValueCannotBeConverted (Location loc, Type target, bool expl)
		{
			if (Type.Name == target.Name){
				Report.ExtraInformation (loc,
					String.Format (
					"The type {0} has two conflicting definitions, one comes from {1} and the other from {2}",
					Type.Name, Type.Assembly.FullName, target.Assembly.FullName));
							 
			}

			if (expl) {
				Report.Error (30, loc, "Cannot convert type `{0}' to `{1}'",
					GetSignatureForError (), TypeManager.CSharpName (target));
				return;
			}
			
			Expression e = (this is EnumConstant) ? ((EnumConstant)this).Child : this;
			bool b = Convert.ExplicitNumericConversion (e, target) != null;

			if (b || Convert.ExplicitReferenceConversionExists (Type, target) || Convert.ExplicitUnsafe (e, target) != null) {
				Report.Error (266, loc, "Cannot implicitly convert type `{0}' to `{1}'. An explicit conversion exists (are you missing a cast?)",
					TypeManager.CSharpName (Type), TypeManager.CSharpName (target));
				return;
			}

			if (Type != TypeManager.string_type && this is Constant && !(this is NullCast)) {
				Report.Error (31, loc, "Constant value `{0}' cannot be converted to a `{1}'",
					GetSignatureForError (), TypeManager.CSharpName (target));
				return;
			}

			Report.Error (29, loc, "Cannot implicitly convert type {0} to `{1}'",
				Type == TypeManager.anonymous_method_type ?
				"anonymous method" : "`" + GetSignatureForError () + "'",
				TypeManager.CSharpName (target));
		}

		protected static void Error_TypeDoesNotContainDefinition (Location loc, Type type, string name)
		{
			Report.Error (117, loc, "`{0}' does not contain a definition for `{1}'",
				TypeManager.CSharpName (type), name);
		}

		ResolveFlags ExprClassToResolveFlags ()
		{
			switch (eclass) {
			case ExprClass.Type:
			case ExprClass.Namespace:
				return ResolveFlags.Type;

			case ExprClass.MethodGroup:
				return ResolveFlags.MethodGroup;

			case ExprClass.Value:
			case ExprClass.Variable:
			case ExprClass.PropertyAccess:
			case ExprClass.EventAccess:
			case ExprClass.IndexerAccess:
				return ResolveFlags.VariableOrValue;

			default:
				throw new Exception ("Expression " + GetType () +
						     " ExprClass is Invalid after resolve");
			}

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
				return ResolveAsTypeStep (ec, false);

			bool old_do_flow_analysis = ec.DoFlowAnalysis;
			bool old_omit_struct_analysis = ec.OmitStructFlowAnalysis;
			if ((flags & ResolveFlags.DisableFlowAnalysis) != 0)
				ec.DoFlowAnalysis = false;
			if ((flags & ResolveFlags.DisableStructFlowAnalysis) != 0)
				ec.OmitStructFlowAnalysis = true;

			Expression e;
			bool intermediate = (flags & ResolveFlags.Intermediate) == ResolveFlags.Intermediate;
			if (this is SimpleName)
				e = ((SimpleName) this).DoResolve (ec, intermediate);

			else 
				e = DoResolve (ec);

			ec.DoFlowAnalysis = old_do_flow_analysis;
			ec.OmitStructFlowAnalysis = old_omit_struct_analysis;

			if (e == null)
				return null;

			if ((flags & e.ExprClassToResolveFlags ()) == 0) {
				e.Error_UnexpectedKind (flags, loc);
				return null;
			}

			if (e.type == null && !(e is Namespace)) {
				throw new Exception (
					"Expression " + e.GetType () +
					" did not set its type after Resolve\n" +
					"called from: " + this.GetType ());
			}

			return e;
		}

		/// <summary>
		///   Resolves an expression and performs semantic analysis on it.
		/// </summary>
		public Expression Resolve (EmitContext ec)
		{
			Expression e = Resolve (ec, ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);

			if (e != null && e.eclass == ExprClass.MethodGroup && RootContext.Version == LanguageVersion.ISO_1) {
				((MethodGroupExpr) e).ReportUsageError ();
				return null;
			}
			return e;
		}

		public Constant ResolveAsConstant (EmitContext ec, MemberCore mc)
		{
			Expression e = Resolve (ec);
			if (e == null)
				return null;

			Constant c = e as Constant;
			if (c != null)
				return c;

			EmptyCast empty = e as EmptyCast;
			if (empty != null) {
				c = empty.Child as Constant;
				if (c != null) {
					// TODO: not sure about this maybe there is easier way how to use EmptyCast
					if (e.Type.IsEnum)
						c.Type = e.Type;

					return c;
				}
			}
			Const.Error_ExpressionMustBeConstant (loc, mc.GetSignatureForError ());
			return null;
		}

		/// <summary>
		///   Resolves an expression for LValue assignment
		/// </summary>
		///
		/// <remarks>
		///   Currently ResolveLValue wraps DoResolveLValue to perform sanity
		///   checking and assertion checking on what we expect from Resolve
		/// </remarks>
		public Expression ResolveLValue (EmitContext ec, Expression right_side, Location loc)
		{
			int errors = Report.Errors;
			Expression e = DoResolveLValue (ec, right_side);

			if (e == null) {
				if (errors == Report.Errors)
					Report.Error (131, loc, "The left-hand side of an assignment or mutating operation must be a variable, property or indexer");
				return null;
			}

			if (e != null){
				if (e.eclass == ExprClass.Invalid)
					throw new Exception ("Expression " + e +
							     " ExprClass is Invalid after resolve");

				if (e.eclass == ExprClass.MethodGroup) {
					((MethodGroupExpr) e).ReportUsageError ();
					return null;
				}

				if ((e.type == null) && !(e is ConstructedType))
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
				return new IntConstant ((int) v, Location.Null);
			else if (t == TypeManager.uint32_type)
				return new UIntConstant ((uint) v, Location.Null);
			else if (t == TypeManager.int64_type)
				return new LongConstant ((long) v, Location.Null);
			else if (t == TypeManager.uint64_type)
				return new ULongConstant ((ulong) v, Location.Null);
			else if (t == TypeManager.float_type)
				return new FloatConstant ((float) v, Location.Null);
			else if (t == TypeManager.double_type)
				return new DoubleConstant ((double) v, Location.Null);
			else if (t == TypeManager.string_type)
				return new StringConstant ((string) v, Location.Null);
			else if (t == TypeManager.short_type)
				return new ShortConstant ((short)v, Location.Null);
			else if (t == TypeManager.ushort_type)
				return new UShortConstant ((ushort)v, Location.Null);
			else if (t == TypeManager.sbyte_type)
				return new SByteConstant ((sbyte)v, Location.Null);
			else if (t == TypeManager.byte_type)
				return new ByteConstant ((byte)v, Location.Null);
			else if (t == TypeManager.char_type)
				return new CharConstant ((char)v, Location.Null);
			else if (t == TypeManager.bool_type)
				return new BoolConstant ((bool) v, Location.Null);
			else if (t == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) v, Location.Null);
			else if (TypeManager.IsEnumType (t)){
				Type real_type = TypeManager.TypeToCoreType (v.GetType ());
				if (real_type == t)
					real_type = System.Enum.GetUnderlyingType (real_type);

				Constant e = Constantify (v, real_type);

				return new EnumConstant (e, t);
			} else if (v == null && !TypeManager.IsValueType (t))
				return new NullLiteral (Location.Null);
			else
				throw new Exception ("Unknown type for constant (" + t +
						     "), details: " + v);
		}

		/// <summary>
		///   Returns a fully formed expression after a MemberLookup
		/// </summary>
		/// 
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

		protected static ArrayList almostMatchedMembers = new ArrayList (4);

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

			MemberInfo [] mi = TypeManager.MemberLookup (
				container_type, qualifier_type, queried_type, mt, bf, name,
				almostMatchedMembers);

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
			if (ec.ResolvingTypeTree)
				return MemberLookup (ec, ec.ContainerType, qualifier_type,
						     queried_type, name, MemberTypes.NestedType,
						     AllBindingFlags, loc);
			else
				return MemberLookup (ec, ec.ContainerType, qualifier_type,
						     queried_type, name, AllMemberTypes,
						     AllBindingFlags, loc);
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
							    Type queried_type, string name,
							    Location loc)
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

			e = MemberLookup (ec, ec.ContainerType, qualifier_type, queried_type,
					  name, mt, bf, loc);

			if (e == null && errors == Report.Errors)
				// No errors were reported by MemberLookup, but there was an error.
				MemberLookupFailed (ec, qualifier_type, queried_type, name, null, true, loc);

			return e;
		}

		public static void MemberLookupFailed (EmitContext ec, Type qualifier_type,
						       Type queried_type, string name,
						       string class_name, bool complain_if_none_found, 
						       Location loc)
		{
			if (almostMatchedMembers.Count != 0) {
				for (int i = 0; i < almostMatchedMembers.Count; ++i) {
					MemberInfo m = (MemberInfo) almostMatchedMembers [i];
					for (int j = 0; j < i; ++j) {
						if (m == almostMatchedMembers [j]) {
							m = null;
							break;
						}
					}
					if (m == null)
						continue;
					
					Type declaring_type = m.DeclaringType;
					
					Report.SymbolRelatedToPreviousError (m);
					if (qualifier_type == null) {
						Report.Error (38, loc, "Cannot access a nonstatic member of outer type `{0}' via nested type `{1}'",
							      TypeManager.CSharpName (m.DeclaringType),
							      TypeManager.CSharpName (ec.ContainerType));
					} else if (qualifier_type != ec.ContainerType &&
						   TypeManager.IsNestedFamilyAccessible (ec.ContainerType, declaring_type)) {
						// Although a derived class can access protected members of
						// its base class it cannot do so through an instance of the
						// base class (CS1540).  If the qualifier_type is a base of the
						// ec.ContainerType and the lookup succeeds with the latter one,
						// then we are in this situation.
						Error_CannotAccessProtected (loc, m, qualifier_type, ec.ContainerType);
					} else {
						ErrorIsInaccesible (loc, TypeManager.GetFullNameSignature (m));
					}
				}
				almostMatchedMembers.Clear ();
				return;
			}

			MemberInfo[] lookup = TypeManager.MemberLookup (queried_type, null, queried_type,
									AllMemberTypes, AllBindingFlags |
									BindingFlags.NonPublic, name, null);

			if (lookup == null) {
				if (!complain_if_none_found)
					return;

				if (class_name != null)
					Report.Error (103, loc, "The name `{0}' does not exist in the context of `{1}'",
						name, class_name);
				else
					Error_TypeDoesNotContainDefinition (loc, queried_type, name);
				return;
			}

			if (TypeManager.MemberLookup (queried_type, null, queried_type,
						      AllMemberTypes, AllBindingFlags |
						      BindingFlags.NonPublic, name, null) == null) {
				if ((lookup.Length == 1) && (lookup [0] is Type)) {
					Type t = (Type) lookup [0];

					Report.Error (305, loc,
						      "Using the generic type `{0}' " +
						      "requires {1} type arguments",
						      TypeManager.CSharpName (t),
						      TypeManager.GetNumberOfTypeArguments (t).ToString ());
					return;
				}
			}

			MemberList ml = TypeManager.FindMembers (queried_type, MemberTypes.Constructor,
								 BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly, null, null);
			if (name == ".ctor" && ml.Count == 0)
			{
				Report.Error (143, loc, "The type `{0}' has no constructors defined", TypeManager.CSharpName (queried_type));
				return;
			}

			ErrorIsInaccesible (loc, TypeManager.GetFullNameSignature (lookup [0]));
		}

		/// <summary>
		///   Returns an expression that can be used to invoke operator true
		///   on the expression if it exists.
		/// </summary>
		static public Expression GetOperatorTrue (EmitContext ec, Expression e, Location loc)
		{
			return GetOperatorTrueOrFalse (ec, e, true, loc);
		}

		/// <summary>
		///   Returns an expression that can be used to invoke operator false
		///   on the expression if it exists.
		/// </summary>
		static public Expression GetOperatorFalse (EmitContext ec, Expression e, Location loc)
		{
			return GetOperatorTrueOrFalse (ec, e, false, loc);
		}

		static Expression GetOperatorTrueOrFalse (EmitContext ec, Expression e, bool is_true, Location loc)
		{
			MethodBase method;
			Expression operator_group;

			if (TypeManager.IsNullableType (e.Type))
				return new Nullable.OperatorTrueOrFalse (e, is_true, loc).Resolve (ec);

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

			if (e.Type == TypeManager.bool_type)
				return e;

			Expression converted = Convert.ImplicitConversion (ec, e, TypeManager.bool_type, Location.Null);

			if (converted != null)
				return converted;

			//
			// If no implicit conversion to bool exists, try using `operator true'
			//
			converted = Expression.GetOperatorTrue (ec, e, loc);
			if (converted == null){
				e.Error_ValueCannotBeConverted (loc, TypeManager.bool_type, false);
				return null;
			}
			return converted;
		}
		
		public virtual string ExprClassName
		{
			get {
				switch (eclass){
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
		}
		
		/// <summary>
		///   Reports that we were expecting `expr' to be of class `expected'
		/// </summary>
		public void Error_UnexpectedKind (EmitContext ec, string expected, Location loc)
		{
			Error_UnexpectedKind (ec, expected, ExprClassName, loc);
		}

		public void Error_UnexpectedKind (EmitContext ec, string expected, string was, Location loc)
		{
			string name = GetSignatureForError ();
			if (ec != null)
				name = ec.DeclSpace.GetSignatureForError () + '.' + name;

			Report.Error (118, loc, "`{0}' is a `{1}' but a `{2}' was expected",
			      name, was, expected);
		}

		public void Error_UnexpectedKind (ResolveFlags flags, Location loc)
		{
			string [] valid = new string [4];
			int count = 0;

			if ((flags & ResolveFlags.VariableOrValue) != 0) {
				valid [count++] = "variable";
				valid [count++] = "value";
			}

			if ((flags & ResolveFlags.Type) != 0)
				valid [count++] = "type";

			if ((flags & ResolveFlags.MethodGroup) != 0)
				valid [count++] = "method group";

			if (count == 0)
				valid [count++] = "unknown";

			StringBuilder sb = new StringBuilder (valid [0]);
			for (int i = 1; i < count - 1; i++) {
				sb.Append ("', `");
				sb.Append (valid [i]);
			}
			if (count > 1) {
				sb.Append ("' or `");
				sb.Append (valid [count - 1]);
			}

			Report.Error (119, loc, 
				"Expression denotes a `{0}', where a `{1}' was expected", ExprClassName, sb.ToString ());
		}
		
		public static void UnsafeError (Location loc)
		{
			Report.Error (214, loc, "Pointers and fixed size buffers may only be used in an unsafe context");
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
			} else if (t.IsValueType || t.IsGenericParameter)
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
			else if (type.IsValueType || type.IsGenericParameter)
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

		protected void Error_CannotCallAbstractBase (string name)
		{
			Report.Error (205, loc, "Cannot call an abstract base member `{0}'", name);
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
							source.Error_ValueCannotBeConverted (loc, TypeManager.int32_type, false);
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
			loc = child.Location;
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
	/// 	This is a numeric cast to a Decimal
	/// </summary>
	public class CastToDecimal : EmptyCast {

		MethodInfo conversion_operator;

		public CastToDecimal (Expression child)
			: this (child, false)
		{
		}

		public CastToDecimal (Expression child, bool find_explicit)
			: base (child, TypeManager.decimal_type)
		{
			conversion_operator = GetConversionOperator (find_explicit);

			if (conversion_operator == null)
				throw new InternalErrorException ("Outer conversion routine is out of sync");
		}

		// Returns the implicit operator that converts from
		// 'child.Type' to System.Decimal.
		MethodInfo GetConversionOperator (bool find_explicit)
		{
			string operator_name = find_explicit ? "op_Explicit" : "op_Implicit";
			
			MemberInfo [] mi = TypeManager.MemberLookup (type, type, type, MemberTypes.Method,
				BindingFlags.Static | BindingFlags.Public, operator_name, null);

			foreach (MethodInfo oper in mi) {
				ParameterData pd = TypeManager.GetParameterData (oper);

				if (pd.ParameterType (0) == child.Type && oper.ReturnType == type)
					return oper;
			}

			return null;
		}
		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			child.Emit (ec);

			ig.Emit (OpCodes.Call, conversion_operator);
		}
	}

	/// <summary>
	/// 	This is an explicit numeric cast from a Decimal
	/// </summary>
	public class CastFromDecimal : EmptyCast
	{
		static IDictionary operators;

		public CastFromDecimal (Expression child, Type return_type)
			: base (child, return_type)
		{
			if (child.Type != TypeManager.decimal_type)
				throw new InternalErrorException (
					"The expected type is Decimal, instead it is " + child.Type.FullName);
		}

		// Returns the explicit operator that converts from an
		// express of type System.Decimal to 'type'.
		public Expression Resolve ()
		{
			if (operators == null) {
				 MemberInfo[] all_oper = TypeManager.MemberLookup (TypeManager.decimal_type,
					TypeManager.decimal_type, TypeManager.decimal_type, MemberTypes.Method,
					BindingFlags.Static | BindingFlags.Public, "op_Explicit", null);

				operators = new System.Collections.Specialized.HybridDictionary ();
				foreach (MethodInfo oper in all_oper) {
					ParameterData pd = TypeManager.GetParameterData (oper);
					if (pd.ParameterType (0) == TypeManager.decimal_type)
						operators.Add (oper.ReturnType, oper);
				}
			}

			return operators.Contains (type) ? this : null;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			child.Emit (ec);

			ig.Emit (OpCodes.Call, (MethodInfo)operators [type]);
		}
	}

        //
	// We need to special case this since an empty cast of
	// a NullLiteral is still a Constant
	//
	public class NullCast : Constant {
		public Constant child;
				
		public NullCast (Constant child, Type return_type):
			base (Location.Null)
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

		public override Constant Increment ()
		{
			throw new NotSupportedException ();
		}

		public override bool IsDefaultValue {
			get {
				return true;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			if (type == target_type)
				return child.Reduce (ec, target_type);

			return null;
		}

	}


	/// <summary>
	///  This class is used to wrap literals which belong inside Enums
	/// </summary>
	public class EnumConstant : Constant {
		public Constant Child;

		public EnumConstant (Constant child, Type enum_type):
			base (child.Location)
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

		public override string GetSignatureForError()
		{
			return TypeManager.CSharpName (Type);
		}

		public override object GetValue ()
		{
			return Child.GetValue ();
		}

		public override object GetTypedValue ()
		{
			// FIXME: runtime is not ready to work with just emited enums
			if (!RootContext.StdLib) {
				return Child.GetValue ();
			}

			return System.Enum.ToObject (type, Child.GetValue ());
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

		public override Constant Increment()
		{
			return new EnumConstant (Child.Increment (), type);
		}

		public override bool IsDefaultValue {
			get {
				return Child.IsDefaultValue;
			}
		}

		public override bool IsZeroInteger {
			get { return Child.IsZeroInteger; }
		}

		public override bool IsNegative {
			get {
				return Child.IsNegative;
			}
		}

		public override Constant Reduce(EmitContext ec, Type target_type)
		{
			if (Child.Type == target_type)
				return Child;

			return Child.Reduce (ec, target_type);
		}

		public override Constant ToType (Type type, Location loc)
		{
			if (Type == type) {
				// This is workaround of mono bug. It can be removed when the latest corlib spreads enough
				if (TypeManager.IsEnumType (type.UnderlyingSystemType))
					return this;

				if (type.UnderlyingSystemType != Child.Type)
					Child = Child.ToType (type.UnderlyingSystemType, loc);
				return this;
			}

			if (!Convert.ImplicitStandardConversionExists (Convert.ConstantEC, this, type)){
				Error_ValueCannotBeConverted (loc, type, false);
				return null;
			}

			return Child.ToType (type, loc);
		}

	}

	/// <summary>
	///   This kind of cast is used to encapsulate Value Types in objects.
	///
	///   The effect of it is to box the value type emitted by the previous
	///   operation.
	/// </summary>
	public class BoxedCast : EmptyCast {

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

		public override Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			if (right_side == EmptyExpression.LValueMemberAccess)
				Report.Error (445, loc, "Cannot modify the result of an unboxing conversion");
			return base.DoResolveLValue (ec, right_side);
		}

		public override void Emit (EmitContext ec)
		{
			Type t = type;
			ILGenerator ig = ec.ig;
			
			base.Emit (ec);
			if (t.IsGenericParameter)
				ig.Emit (OpCodes.Unbox_Any, t);
			else {
				ig.Emit (OpCodes.Unbox, t);

				LoadFromPtr (ig, t);
			}
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
		
		public ConvCast (Expression child, Type return_type, Mode m)
			: base (child, return_type)
		{
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

			if (ec.CheckState){
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

			if (child.Type.IsGenericParameter)
				ec.ig.Emit (OpCodes.Box, child.Type);

			if (type.IsGenericParameter)
				ec.ig.Emit (OpCodes.Unbox_Any, type);
			else
				ec.ig.Emit (OpCodes.Castclass, type);
		}
	}
	
	/// <summary>
	///   SimpleName expressions are formed of a single word and only happen at the beginning 
	///   of a dotted-name.
	/// </summary>
	public class SimpleName : Expression {
		public string Name;
		public readonly TypeArguments Arguments;
		bool in_transit;

		public SimpleName (string name, Location l)
		{
			Name = name;
			loc = l;
		}

		public SimpleName (string name, TypeArguments args, Location l)
		{
			Name = name;
			Arguments = args;
			loc = l;
		}

		public SimpleName (string name, TypeParameter[] type_params, Location l)
		{
			Name = name;
			loc = l;

			Arguments = new TypeArguments (l);
			foreach (TypeParameter type_param in type_params)
				Arguments.Add (new TypeParameterExpr (type_param, l));
		}

		public static string RemoveGenericArity (string name)
		{
			int start = 0;
			StringBuilder sb = new StringBuilder ();
			while (start < name.Length) {
				int pos = name.IndexOf ('`', start);
				if (pos < 0) {
					sb.Append (name.Substring (start));
					break;
				}

				sb.Append (name.Substring (start, pos-start));

				pos++;
				while ((pos < name.Length) && Char.IsNumber (name [pos]))
					pos++;

				start = pos;
			}

			return sb.ToString ();
		}

		public SimpleName GetMethodGroup ()
		{
			return new SimpleName (RemoveGenericArity (Name), Arguments, loc);
		}

		public static void Error_ObjectRefRequired (EmitContext ec, Location l, string name)
		{
			if (ec.IsFieldInitializer)
				Report.Error (236, l,
					"A field initializer cannot reference the nonstatic field, method, or property `{0}'",
					name);
			else {
				if (name.LastIndexOf ('.') > 0)
					name = name.Substring (name.LastIndexOf ('.') + 1);

				Report.Error (
					120, l, "`{0}': An object reference is required for the nonstatic field, method or property",
					name);
			}
		}

		public bool IdenticalNameAndTypeName (EmitContext ec, Expression resolved_to, Location loc)
		{
			return resolved_to != null && resolved_to.Type != null && 
				resolved_to.Type.Name == Name &&
				(ec.DeclSpace.LookupType (Name, loc, /* ignore_cs0104 = */ true) != null);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			return SimpleNameResolve (ec, null, false);
		}

		public override Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			return SimpleNameResolve (ec, right_side, false);
		}
		

		public Expression DoResolve (EmitContext ec, bool intermediate)
		{
			return SimpleNameResolve (ec, null, intermediate);
		}

		private bool IsNestedChild (Type t, Type parent)
		{
			if (parent == null)
				return false;

			while (parent != null) {
				parent = TypeManager.DropGenericTypeArguments (parent);
				if (TypeManager.IsNestedChildOf (t, parent))
					return true;

				parent = parent.BaseType;
			}

			return false;
		}

		FullNamedExpression ResolveNested (EmitContext ec, Type t)
		{
			if (!t.IsGenericTypeDefinition)
				return null;

			DeclSpace ds = ec.DeclSpace;
			while (ds != null) {
				if (IsNestedChild (t, ds.TypeBuilder))
					break;

				ds = ds.Parent;
			}

			if (ds == null)
				return null;

			Type[] gen_params = t.GetGenericArguments ();

			int arg_count = Arguments != null ? Arguments.Count : 0;

			for (; (ds != null) && ds.IsGeneric; ds = ds.Parent) {
				if (arg_count + ds.CountTypeParameters == gen_params.Length) {
					TypeArguments new_args = new TypeArguments (loc);
					foreach (TypeParameter param in ds.TypeParameters)
						new_args.Add (new TypeParameterExpr (param, loc));

					if (Arguments != null)
						new_args.Add (Arguments);

					return new ConstructedType (t, new_args, loc);
				}
			}

			return null;
		}

		public override FullNamedExpression ResolveAsTypeStep (EmitContext ec, bool silent)
		{
			FullNamedExpression fne = ec.DeclSpace.LookupGeneric (Name, loc);
			if (fne != null)
				return fne.ResolveAsTypeStep (ec, silent);

			int errors = Report.Errors;
			fne = ec.DeclSpace.LookupType (Name, loc, /*ignore_cs0104=*/ false);

			if (fne != null) {
				if (fne.Type == null)
					return fne;

				FullNamedExpression nested = ResolveNested (ec, fne.Type);
				if (nested != null)
					return nested.ResolveAsTypeStep (ec);

				if (Arguments != null) {
					ConstructedType ct = new ConstructedType (fne, Arguments, loc);
					return ct.ResolveAsTypeStep (ec);
				}

				return fne;
			}

			if (silent || errors != Report.Errors)
				return null;

			MemberCore mc = ec.DeclSpace.GetDefinition (Name);
			if (mc != null) {
				Error_UnexpectedKind (ec, "type", GetMemberType (mc), loc);
			} else {
				NamespaceEntry.Error_NamespaceNotFound (loc, Name);
			}

			return null;
		}

		// TODO: I am still not convinced about this. If someone else will need it
		// implement this as virtual property in MemberCore hierarchy
		string GetMemberType (MemberCore mc)
		{
			if (mc is PropertyBase)
				return "property";
			if (mc is Indexer)
				return "indexer";
			if (mc is FieldBase)
				return "field";
			if (mc is MethodCore)
				return "method";
			if (mc is EnumMember)
				return "enum";

			return "type";
		}

		Expression SimpleNameResolve (EmitContext ec, Expression right_side, bool intermediate)
		{
			if (in_transit)
				return null;
			in_transit = true;

			Expression e = DoSimpleNameResolve (ec, right_side, intermediate);
			if (e == null)
				return null;

			if (ec.CurrentBlock == null || ec.CurrentBlock.CheckInvariantMeaningInBlock (Name, e, Location))
				return e;

			return null;
		}

		/// <remarks>
		///   7.5.2: Simple Names. 
		///
		///   Local Variables and Parameters are handled at
		///   parse time, so they never occur as SimpleNames.
		///
		///   The `intermediate' flag is used by MemberAccess only
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
		Expression DoSimpleNameResolve (EmitContext ec, Expression right_side, bool intermediate)
		{
			Expression e = null;

			//
			// Stage 1: Performed by the parser (binding to locals or parameters).
			//
			Block current_block = ec.CurrentBlock;
			if (current_block != null){
				LocalInfo vi = current_block.GetLocalInfo (Name);
				if (vi != null){
					LocalVariableReference var = new LocalVariableReference (ec.CurrentBlock, Name, loc);
					if (right_side != null) {
						return var.ResolveLValue (ec, right_side, loc);
					} else {
						ResolveFlags rf = ResolveFlags.VariableOrValue;
						if (intermediate)
							rf |= ResolveFlags.DisableFlowAnalysis;
						return var.Resolve (ec, rf);
					}
				}

				ParameterReference pref = current_block.Toplevel.GetParameterReference (Name, loc);
				if (pref != null) {
					if (right_side != null)
						return pref.ResolveLValue (ec, right_side, loc);
					else
						return pref.Resolve (ec);
				}
			}
			
			//
			// Stage 2: Lookup members 
			//

			DeclSpace lookup_ds = ec.DeclSpace;
			Type almost_matched_type = null;
			ArrayList almost_matched = null;
			do {
				if (lookup_ds.TypeBuilder == null)
					break;

				e = MemberLookup (ec, lookup_ds.TypeBuilder, Name, loc);
				if (e != null)
					break;

				if (almost_matched == null && almostMatchedMembers.Count > 0) {
					almost_matched_type = lookup_ds.TypeBuilder;
					almost_matched = (ArrayList) almostMatchedMembers.Clone ();
				}

				lookup_ds =lookup_ds.Parent;
			} while (lookup_ds != null);

			if (e == null && ec.ContainerType != null)
				e = MemberLookup (ec, ec.ContainerType, Name, loc);

			if (e == null) {
				if (almost_matched == null && almostMatchedMembers.Count > 0) {
					almost_matched_type = ec.ContainerType;
					almost_matched = (ArrayList) almostMatchedMembers.Clone ();
				}
				e = ResolveAsTypeStep (ec, true);
			}

			if (e == null) {
				if (almost_matched != null)
					almostMatchedMembers = almost_matched;
				if (almost_matched_type == null)
					almost_matched_type = ec.ContainerType;
				MemberLookupFailed (ec, null, almost_matched_type, ((SimpleName) this).Name, ec.DeclSpace.Name, true, loc);
				return null;
			}

			if (e is TypeExpr)
				return e;

			if (e is MemberExpr) {
				MemberExpr me = (MemberExpr) e;

				Expression left;
				if (me.IsInstance) {
					if (ec.IsStatic || ec.IsFieldInitializer) {
						//
						// Note that an MemberExpr can be both IsInstance and IsStatic.
						// An unresolved MethodGroupExpr can contain both kinds of methods
						// and each predicate is true if the MethodGroupExpr contains
						// at least one of that kind of method.
						//

						if (!me.IsStatic &&
						    (!intermediate || !IdenticalNameAndTypeName (ec, me, loc))) {
							Error_ObjectRefRequired (ec, loc, me.GetSignatureForError ());
							return EmptyExpression.Null;
						}

						//
						// Pass the buck to MemberAccess and Invocation.
						//
						left = EmptyExpression.Null;
					} else {
						left = ec.GetThis (loc);
					}
				} else {
					left = new TypeExpression (ec.ContainerType, loc);
				}

				e = me.ResolveMemberAccess (ec, left, loc, null);
				if (e == null)
					return null;

				me = e as MemberExpr;
				if (me == null)
					return e;

				if (Arguments != null) {
					MethodGroupExpr mg = me as MethodGroupExpr;
					if (mg == null)
						return null;

					return mg.ResolveGeneric (ec, Arguments);
				}

				if (!me.IsStatic &&
				    TypeManager.IsNestedFamilyAccessible (me.InstanceExpression.Type, me.DeclaringType) &&
				    me.InstanceExpression.Type != me.DeclaringType &&
				    !TypeManager.IsFamilyAccessible (me.InstanceExpression.Type, me.DeclaringType) &&
				    (!intermediate || !IdenticalNameAndTypeName (ec, e, loc))) {
					Report.Error (38, loc, "Cannot access a nonstatic member of outer type `{0}' via nested type `{1}'",
						TypeManager.CSharpName (me.DeclaringType), TypeManager.CSharpName (me.InstanceExpression.Type));
					return null;
				}

				return (right_side != null)
					? me.DoResolveLValue (ec, right_side)
					: me.DoResolve (ec);
			}

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

		public override string GetSignatureForError ()
		{
			return Name;
		}
	}

	/// <summary>
	///   Represents a namespace or a type.  The name of the class was inspired by
	///   section 10.8.1 (Fully Qualified Names).
	/// </summary>
	public abstract class FullNamedExpression : Expression {
		public override FullNamedExpression ResolveAsTypeStep (EmitContext ec, bool silent)
		{
			return this;
		}

		public abstract string FullName {
			get;
		}
	}
	
	/// <summary>
	///   Expression that evaluates to a type
	/// </summary>
	public abstract class TypeExpr : FullNamedExpression {
		override public FullNamedExpression ResolveAsTypeStep (EmitContext ec, bool silent)
		{
			TypeExpr t = DoResolveAsTypeStep (ec);
			if (t == null)
				return null;

			eclass = ExprClass.Type;
			return t;
		}

		override public Expression DoResolve (EmitContext ec)
		{
			return ResolveAsTypeTerminal (ec);
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

		protected abstract TypeExpr DoResolveAsTypeStep (EmitContext ec);

		public Type ResolveType (EmitContext ec)
		{
			TypeExpr t = ResolveAsTypeTerminal (ec);
			if (t == null)
				return null;

			if (ec.TestObsoleteMethodUsage) {
				ObsoleteAttribute obsolete_attr = AttributeTester.GetObsoleteAttribute (t.Type);
				if (obsolete_attr != null) {
					AttributeTester.Report_ObsoleteMessage (obsolete_attr, Name, Location);
				}
			}

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

	/// <summary>
	///   Fully resolved Expression that already evaluated to a type
	/// </summary>
	public class TypeExpression : TypeExpr {
		public TypeExpression (Type t, Location l)
		{
			Type = t;
			eclass = ExprClass.Type;
			loc = l;
		}

		protected override TypeExpr DoResolveAsTypeStep (EmitContext ec)
		{
			return this;
		}

		public override string Name {
			get { return Type.ToString (); }
		}

		public override string FullName {
			get { return Type.FullName; }
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

		static readonly char [] dot_array = { '.' };
		protected override TypeExpr DoResolveAsTypeStep (EmitContext ec)
		{
			if (type != null)
				return this;

			// If name is of the form `N.I', first lookup `N', then search a member `I' in it.
			string rest = null;
			string lookup_name = name;
			int pos = name.IndexOf ('.');
			if (pos >= 0) {
				rest = name.Substring (pos + 1);
				lookup_name = name.Substring (0, pos);
			}

			FullNamedExpression resolved = RootNamespace.Global.Lookup (ec.DeclSpace, lookup_name, Location.Null);

			if (resolved != null && rest != null) {
				// Now handle the rest of the the name.
				string [] elements = rest.Split (dot_array);
				string element;
				int count = elements.Length;
				int i = 0;
				while (i < count && resolved != null && resolved is Namespace) {
					Namespace ns = resolved as Namespace;
					element = elements [i++];
					lookup_name += "." + element;
					resolved = ns.Lookup (ec.DeclSpace, element, Location.Null);
				}

				if (resolved != null && resolved is TypeExpr) {
					Type t = ((TypeExpr) resolved).Type;
					while (t != null) {
						if (!ec.DeclSpace.CheckAccessLevel (t)) {
							resolved = null;
							lookup_name = t.FullName;
							break;
						}
						if (i == count) {
							type = t;
							return this;
						}
						t = TypeManager.GetNestedType (t, elements [i++]);
					}
				}
			}

			if (resolved == null) {
				NamespaceEntry.Error_NamespaceNotFound (loc, lookup_name);
				return null;
			}

			if (!(resolved is TypeExpr)) {
				resolved.Error_UnexpectedKind (ec, "type", loc);
				return null;
			}

			type = ((TypeExpr) resolved).ResolveType (ec);
			return this;
		}

		public override string Name {
			get { return name; }
		}

		public override string FullName {
			get { return name; }
		}
	}

	/// <summary>
	///   Represents an "unbound generic type", ie. typeof (Foo<>).
	///   See 14.5.11.
	/// </summary>
	public class UnboundTypeExpression : TypeExpr
	{
		MemberName name;

		public UnboundTypeExpression (MemberName name, Location l)
		{
			this.name = name;
			loc = l;
		}

		protected override TypeExpr DoResolveAsTypeStep (EmitContext ec)
		{
			Expression expr;
			if (name.Left != null) {
				Expression lexpr = name.Left.GetTypeExpression ();
				expr = new MemberAccess (lexpr, name.Basename, loc);
			} else {
				expr = new SimpleName (name.Basename, loc);
			}

			FullNamedExpression fne = expr.ResolveAsTypeStep (ec);
			if (fne == null)
				return null;

			type = fne.Type;
			return new TypeExpression (type, loc);
		}

		public override string Name {
			get { return name.FullName; }
		}

		public override string FullName {
			get { return name.FullName; }
		}
	}

	public class TypeAliasExpression : TypeExpr {
		FullNamedExpression alias;
		TypeExpr texpr;
		TypeArguments args;
		string name;

		public TypeAliasExpression (FullNamedExpression alias, TypeArguments args, Location l)
		{
			this.alias = alias;
			this.args = args;
			loc = l;

			eclass = ExprClass.Type;
			if (args != null)
				name = alias.FullName + "<" + args.ToString () + ">";
			else
				name = alias.FullName;
		}

		public override string Name {
			get { return alias.FullName; }
		}

		public override string FullName {
			get { return name; }
		}

		protected override TypeExpr DoResolveAsTypeStep (EmitContext ec)
		{
			texpr = alias.ResolveAsTypeTerminal (ec);
			if (texpr == null)
				return null;

			Type type = texpr.ResolveType (ec);
			int num_args = TypeManager.GetNumberOfTypeArguments (type);

			if (args != null) {
				if (num_args == 0) {
					Report.Error (308, loc,
						      "The non-generic type `{0}' cannot " +
						      "be used with type arguments.",
						      TypeManager.CSharpName (type));
					return null;
				}

				ConstructedType ctype = new ConstructedType (type, args, loc);
				return ctype.ResolveAsTypeTerminal (ec);
			} else if (num_args > 0) {
				Report.Error (305, loc,
					      "Using the generic type `{0}' " +
					      "requires {1} type arguments",
					      TypeManager.CSharpName (type), num_args.ToString ());
				return null;
			}

			return new TypeExpression (type, loc);
		}

		public override bool CheckAccessLevel (DeclSpace ds)
		{
			return texpr.CheckAccessLevel (ds);
		}

		public override bool AsAccessible (DeclSpace ds, int flags)
		{
			return texpr.AsAccessible (ds, flags);
		}

		public override bool IsClass {
			get { return texpr.IsClass; }
		}

		public override bool IsValueType {
			get { return texpr.IsValueType; }
		}

		public override bool IsInterface {
			get { return texpr.IsInterface; }
		}

		public override bool IsSealed {
			get { return texpr.IsSealed; }
		}
	}

	/// <summary>
	///   This class denotes an expression which evaluates to a member
	///   of a struct or a class.
	/// </summary>
	public abstract class MemberExpr : Expression
	{
		/// <summary>
		///   The name of this member.
		/// </summary>
		public abstract string Name {
			get;
		}

		/// <summary>
		///   Whether this is an instance member.
		/// </summary>
		public abstract bool IsInstance {
			get;
		}

		/// <summary>
		///   Whether this is a static member.
		/// </summary>
		public abstract bool IsStatic {
			get;
		}

		/// <summary>
		///   The type which declares this member.
		/// </summary>
		public abstract Type DeclaringType {
			get;
		}

		/// <summary>
		///   The instance expression associated with this member, if it's a
		///   non-static member.
		/// </summary>
		public Expression InstanceExpression;

		public static void error176 (Location loc, string name)
		{
			Report.Error (176, loc, "Static member `{0}' cannot be accessed " +
				      "with an instance reference, qualify it with a type name instead", name);
		}

		// TODO: possible optimalization
		// Cache resolved constant result in FieldBuilder <-> expression map
		public virtual Expression ResolveMemberAccess (EmitContext ec, Expression left, Location loc,
							       SimpleName original)
		{
			//
			// Precondition:
			//   original == null || original.Resolve (...) ==> left
			//

			if (left is TypeExpr) {
				if (!IsStatic) {
					SimpleName.Error_ObjectRefRequired (ec, loc, Name);
					return null;
				}

				return this;
			}
				
			if (!IsInstance) {
				if (original != null && original.IdenticalNameAndTypeName (ec, left, loc))
					return this;

				error176 (loc, GetSignatureForError ());
				return null;
			}

			InstanceExpression = left;

			return this;
		}

		protected void EmitInstance (EmitContext ec, bool prepare_for_load)
		{
			if (IsStatic)
				return;

			if (InstanceExpression == EmptyExpression.Null) {
				SimpleName.Error_ObjectRefRequired (ec, loc, Name);
				return;
			}
				
			if (InstanceExpression.Type.IsValueType) {
				if (InstanceExpression is IMemoryLocation) {
					((IMemoryLocation) InstanceExpression).AddressOf (ec, AddressOp.LoadStore);
				} else {
					LocalTemporary t = new LocalTemporary (ec, InstanceExpression.Type);
					InstanceExpression.Emit (ec);
					t.Store (ec);
					t.AddressOf (ec, AddressOp.Store);
				}
			} else
				InstanceExpression.Emit (ec);

			if (prepare_for_load)
				ec.ig.Emit (OpCodes.Dup);
		}
	}

	/// <summary>
	///   MethodGroup Expression.
	///  
	///   This is a fully resolved expression that evaluates to a type
	/// </summary>
	public class MethodGroupExpr : MemberExpr {
		public MethodBase [] Methods;
		bool has_type_arguments = false;
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

		public override Type DeclaringType {
			get {
                                //
                                // We assume that the top-level type is in the end
                                //
				return Methods [Methods.Length - 1].DeclaringType;
                                //return Methods [0].DeclaringType;
			}
		}

		public bool HasTypeArguments {
			get {
				return has_type_arguments;
			}

			set {
				has_type_arguments = value;
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

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpSignature (Methods [0]);
		}

		public override string Name {
			get {
				return Methods [0].Name;
			}
		}

		public override bool IsInstance {
			get {
				foreach (MethodBase mb in Methods)
					if (!mb.IsStatic)
						return true;

				return false;
			}
		}

		public override bool IsStatic {
			get {
				foreach (MethodBase mb in Methods)
					if (mb.IsStatic)
						return true;

				return false;
			}
		}

		public override Expression ResolveMemberAccess (EmitContext ec, Expression left, Location loc,
								SimpleName original)
		{
			if (!(left is TypeExpr) &&
			    original != null && original.IdenticalNameAndTypeName (ec, left, loc))
				IdenticalTypeName = true;

			return base.ResolveMemberAccess (ec, left, loc, original);
		}
		
		override public Expression DoResolve (EmitContext ec)
		{
			if (!IsInstance)
				InstanceExpression = null;

			if (InstanceExpression != null) {
				InstanceExpression = InstanceExpression.DoResolve (ec);
				if (InstanceExpression == null)
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

		public Expression ResolveGeneric (EmitContext ec, TypeArguments args)
		{
			if (args.Resolve (ec) == false)
				return null;

			Type[] atypes = args.Arguments;

			int first_count = 0;
			MethodInfo first = null;

			ArrayList list = new ArrayList ();
			foreach (MethodBase mb in Methods) {
				MethodInfo mi = mb as MethodInfo;
				if ((mi == null) || !mi.HasGenericParameters)
					continue;

				Type[] gen_params = mi.GetGenericArguments ();

				if (first == null) {
					first = mi;
					first_count = gen_params.Length;
				}

				if (gen_params.Length != atypes.Length)
					continue;

				list.Add (mi.MakeGenericMethod (atypes));
			}

			if (list.Count > 0) {
				MethodGroupExpr new_mg = new MethodGroupExpr (list, Location);
				new_mg.InstanceExpression = InstanceExpression;
				new_mg.HasTypeArguments = true;
				new_mg.IsBase = IsBase;
				return new_mg;
			}

			if (first != null)
				Report.Error (
					305, loc, "Using the generic method `{0}' " +
					"requires {1} type arguments", Name,
					first_count.ToString ());
			else
				Report.Error (
					308, loc, "The non-generic method `{0}' " +
					"cannot be used with type arguments", Name);

			return null;
		}
	}

	/// <summary>
	///   Fully resolved expression that evaluates to a Field
	/// </summary>
	public class FieldExpr : MemberExpr, IAssignMethod, IMemoryLocation, IVariable {
		public readonly FieldInfo FieldInfo;
		VariableInfo variable_info;
		
		LocalTemporary temp;
		bool prepared;
		bool in_initializer;

		public FieldExpr (FieldInfo fi, Location l, bool in_initializer):
			this (fi, l)
		{
			this.in_initializer = in_initializer;
		}
		
		public FieldExpr (FieldInfo fi, Location l)
		{
			FieldInfo = fi;
			eclass = ExprClass.Variable;
			type = TypeManager.TypeToCoreType (fi.FieldType);
			loc = l;
		}

		public override string Name {
			get {
				return FieldInfo.Name;
			}
		}

		public override bool IsInstance {
			get {
				return !FieldInfo.IsStatic;
			}
		}

		public override bool IsStatic {
			get {
				return FieldInfo.IsStatic;
			}
		}

		public override Type DeclaringType {
			get {
				return FieldInfo.DeclaringType;
			}
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.GetFullNameSignature (FieldInfo);
		}

		public VariableInfo VariableInfo {
			get {
				return variable_info;
			}
		}

		public override Expression ResolveMemberAccess (EmitContext ec, Expression left, Location loc,
								SimpleName original)
		{
			FieldInfo fi = FieldInfo.Mono_GetGenericFieldDefinition ();

			Type t = fi.FieldType;

			if (fi.IsLiteral || (fi.IsInitOnly && t == TypeManager.decimal_type)) {
				IConstant ic = TypeManager.GetConstant (fi);
				if (ic == null) {
					if (fi.IsLiteral) {
						ic = new ExternalConstant (fi);
					} else {
						ic = ExternalConstant.CreateDecimal (fi);
						if (ic == null) {
							return base.ResolveMemberAccess (ec, left, loc, original);
						}
					}
					TypeManager.RegisterConstant (fi, ic);
				}

				bool left_is_type = left is TypeExpr;
				if (!left_is_type && (original == null || !original.IdenticalNameAndTypeName (ec, left, loc))) {
					Report.SymbolRelatedToPreviousError (FieldInfo);
					error176 (loc, TypeManager.GetFullNameSignature (FieldInfo));
					return null;
				}

				if (ic.ResolveValue ()) {
					if (ec.TestObsoleteMethodUsage)
						ic.CheckObsoleteness (loc);
				}

				return ic.Value;
			}
			
			if (t.IsPointer && !ec.InUnsafe) {
				UnsafeError (loc);
				return null;
			}

			return base.ResolveMemberAccess (ec, left, loc, original);
		}

		override public Expression DoResolve (EmitContext ec)
		{
			return DoResolve (ec, false);
		}

		Expression DoResolve (EmitContext ec, bool lvalue_instance)
		{
			if (ec.InRefOutArgumentResolving && FieldInfo.IsInitOnly && !ec.IsConstructor && FieldInfo.FieldType.IsValueType) {
				if (FieldInfo.FieldType is TypeBuilder) {
					if (FieldInfo.IsStatic)
						Report.Error (1651, loc, "Fields of static readonly field `{0}' cannot be passed ref or out (except in a static constructor)",
							GetSignatureForError ());
					else
						Report.Error (1649, loc, "Members of readonly field `{0}.{1}' cannot be passed ref or out (except in a constructor)",
							TypeManager.CSharpName (DeclaringType), Name);
				} else {
					if (FieldInfo.IsStatic)
						Report.Error (199, loc, "A static readonly field `{0}' cannot be passed ref or out (except in a static constructor)",
							Name);
					else
						Report.Error (192, loc, "A readonly field `{0}' cannot be passed ref or out (except in a constructor)",
							Name);
				}
				return null;
			}

			if (!FieldInfo.IsStatic){
				if (InstanceExpression == null){
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

				if (lvalue_instance) {
					bool old_do_flow_analysis = ec.DoFlowAnalysis;
					ec.DoFlowAnalysis = false;
					InstanceExpression = InstanceExpression.ResolveLValue (ec, EmptyExpression.LValueMemberAccess, loc);
					ec.DoFlowAnalysis = old_do_flow_analysis;
				} else {
					ResolveFlags rf = ResolveFlags.VariableOrValue | ResolveFlags.DisableFlowAnalysis;
					InstanceExpression = InstanceExpression.Resolve (ec, rf);
				}

				if (InstanceExpression == null)
					return null;
			}

			if (!in_initializer && !ec.IsFieldInitializer) {
				ObsoleteAttribute oa;
				FieldBase f = TypeManager.GetField (FieldInfo);
				if (f != null) {
					if (ec.TestObsoleteMethodUsage)
						f.CheckObsoleteness (loc);
					// To be sure that type is external because we do not register generated fields
				} else if (!(FieldInfo.DeclaringType is TypeBuilder)) {                                
					oa = AttributeTester.GetMemberObsoleteAttribute (FieldInfo);
					if (oa != null)
						AttributeTester.Report_ObsoleteMessage (oa, TypeManager.GetFullNameSignature (FieldInfo), loc);
				}
			}

			AnonymousContainer am = ec.CurrentAnonymousMethod;
			if (am != null){
				if (!FieldInfo.IsStatic){
					if (!am.IsIterator && (ec.TypeContainer is Struct)){
 						Report.Error (1673, loc,
 						"Anonymous methods inside structs cannot access instance members of `{0}'. Consider copying `{0}' to a local variable outside the anonymous method and using the local instead",
 							"this");
						return null;
					}
					if ((am.ContainerAnonymousMethod == null) && (InstanceExpression is This))
						ec.CaptureField (this);
				}
			}
			
			// If the instance expression is a local variable or parameter.
			IVariable var = InstanceExpression as IVariable;
			if ((var == null) || (var.VariableInfo == null))
				return this;

			VariableInfo vi = var.VariableInfo;
			if (!vi.IsFieldAssigned (ec, FieldInfo.Name, loc))
				return null;

			variable_info = vi.GetSubStruct (FieldInfo.Name);
			return this;
		}

		void Report_AssignToReadonly (Expression right_side)
		{
			int code;
			string msg;
			bool need_error_sig = false;
			if (right_side == EmptyExpression.LValueMemberAccess) {
				if (IsStatic) {
					code = 1650;
					msg = "Fields of static readonly field `{0}' cannot be assigned to (except in a static constructor or a variable initializer)";
				} else {
					code = 1648;
					msg = "Members of readonly field `{0}' cannot be modified (except in a constructor or a variable initializer)";
				}
				need_error_sig = true;
			} else if (IsStatic) {
				code = 198;
				msg = "A static readonly field cannot be assigned to (except in a static constructor or a variable initializer)";
			} else {
				code = 191;
				msg = "A readonly field cannot be assigned to (except in a constructor or a variable initializer)";
			}

			if (need_error_sig)
				Report.Error (code, loc, msg, GetSignatureForError ());
			else
				Report.Error (code, loc, msg);
		}
		
		override public Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			IVariable var = InstanceExpression as IVariable;
			if ((var != null) && (var.VariableInfo != null))
				var.VariableInfo.SetFieldAssigned (ec, FieldInfo.Name);

			bool lvalue_instance = !FieldInfo.IsStatic && FieldInfo.DeclaringType.IsValueType;

			Expression e = DoResolve (ec, lvalue_instance);

			if (e == null)
				return null;

			FieldBase fb = TypeManager.GetField (FieldInfo);
			if (fb != null)
				fb.SetAssigned ();

			if (!FieldInfo.IsInitOnly)
				return this;

			//
			// InitOnly fields can only be assigned in constructors
			//

			if (ec.IsConstructor){
				if (IsStatic && !ec.IsStatic)
					Report_AssignToReadonly (right_side);

				Type ctype;
				if (ec.TypeContainer.CurrentType != null)
					ctype = ec.TypeContainer.CurrentType;
				else
					ctype = ec.ContainerType;

				if (TypeManager.IsEqual (ctype, FieldInfo.DeclaringType))
					return this;
			}

			Report_AssignToReadonly (right_side);
			
			return null;
		}

		public override void CheckMarshallByRefAccess (Type container)
		{
			if (!IsStatic && Type.IsValueType && !container.IsSubclassOf (TypeManager.mbr_type) && DeclaringType.IsSubclassOf (TypeManager.mbr_type)) {
				Report.SymbolRelatedToPreviousError (DeclaringType);
				Report.Error (1690, loc, "Cannot call methods, properties, or indexers on `{0}' because it is a value type member of a marshal-by-reference class",
					GetSignatureForError ());
			}
		}

		public bool VerifyFixed ()
		{
			IVariable variable = InstanceExpression as IVariable;
			// A variable of the form V.I is fixed when V is a fixed variable of a struct type.
			// We defer the InstanceExpression check after the variable check to avoid a 
			// separate null check on InstanceExpression.
			return variable != null && InstanceExpression.Type.IsValueType && variable.VerifyFixed ();
		}

		public override int GetHashCode()
		{
			return FieldInfo.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			FieldExpr fe = obj as FieldExpr;
			if (fe == null)
				return false;

			if (FieldInfo != fe.FieldInfo)
				return false;

			if (InstanceExpression == null || fe.InstanceExpression == null)
				return true;

			return InstanceExpression.Equals (fe.InstanceExpression);
		}
		
		public void Emit (EmitContext ec, bool leave_copy)
		{
			ILGenerator ig = ec.ig;
			bool is_volatile = false;

			FieldInfo the_fi = FieldInfo.Mono_GetGenericFieldDefinition ();
			if (the_fi is FieldBuilder){
				FieldBase f = TypeManager.GetField (the_fi);
				if (f != null){
					if ((f.ModFlags & Modifiers.VOLATILE) != 0)
						is_volatile = true;
					
					f.SetMemberIsUsed ();
				}
			} 
			
			if (FieldInfo.IsStatic){
				if (is_volatile)
					ig.Emit (OpCodes.Volatile);
				
				ig.Emit (OpCodes.Ldsfld, FieldInfo);
			} else {
				if (!prepared)
					EmitInstance (ec, false);

				if (is_volatile)
					ig.Emit (OpCodes.Volatile);

				IFixedBuffer ff = AttributeTester.GetFixedBuffer (FieldInfo);
				if (ff != null)
				{
					ig.Emit (OpCodes.Ldflda, FieldInfo);
					ig.Emit (OpCodes.Ldflda, ff.Element);
				}
				else {
					ig.Emit (OpCodes.Ldfld, FieldInfo);
				}
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
				Report_AssignToReadonly (source);
				return;
			}

			EmitInstance (ec, prepare_for_load);

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
					
					f.SetAssigned ();
				}
			} 

			if (is_static)
				ig.Emit (OpCodes.Stsfld, FieldInfo);
			else 
				ig.Emit (OpCodes.Stfld, FieldInfo);
			
			if (temp != null)
				temp.Emit (ec);
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
						Report.Warning (420, 1, loc, "`{0}': A volatile fields cannot be passed using a ref or out parameter",
							f.GetSignatureForError ());
						return;
					}
					
					if ((mode & AddressOp.Store) != 0)
						f.SetAssigned ();
					if ((mode & AddressOp.Load) != 0)
						f.SetMemberIsUsed ();
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
				EmitInstance (ec, false);
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
	public class PropertyExpr : MemberExpr, IAssignMethod {
		public readonly PropertyInfo PropertyInfo;

		//
		// This is set externally by the  `BaseAccess' class
		//
		public bool IsBase;
		MethodInfo getter, setter;
		bool is_static;

		bool resolved;
		
		LocalTemporary temp;
		bool prepared;

		internal static PtrHashtable AccessorTable = new PtrHashtable (); 

		public PropertyExpr (EmitContext ec, PropertyInfo pi, Location l)
		{
			PropertyInfo = pi;
			eclass = ExprClass.PropertyAccess;
			is_static = false;
			loc = l;

			type = TypeManager.TypeToCoreType (pi.PropertyType);

			ResolveAccessors (ec);
		}

		public override string Name {
			get {
				return PropertyInfo.Name;
			}
		}

		public override bool IsInstance {
			get {
				return !is_static;
			}
		}

		public override bool IsStatic {
			get {
				return is_static;
			}
		}
		
		public override Type DeclaringType {
			get {
				return PropertyInfo.DeclaringType;
			}
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.GetFullNameSignature (PropertyInfo);
		}

		void FindAccessors (Type invocation_type)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
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
					getter = pi.GetGetMethod (true);

				if (setter == null)
					setter = pi.GetSetMethod (true);

				MethodInfo accessor = getter != null ? getter : setter;

				if (!accessor.IsVirtual)
					return;
			}
		}

		//
		// We also perform the permission checking here, as the PropertyInfo does not
		// hold the information for the accessibility of its setter/getter
		//
		void ResolveAccessors (EmitContext ec)
		{
			FindAccessors (ec.ContainerType);

			if (getter != null) {
				MethodInfo the_getter = getter;
				if (the_getter.Mono_IsInflatedMethod)
					the_getter = the_getter.GetGenericMethodDefinition ();
				IMethodData md = TypeManager.GetMethod (the_getter);
				if (md != null)
					md.SetMemberIsUsed ();

				AccessorTable [getter] = PropertyInfo;
				is_static = getter.IsStatic;
			}

			if (setter != null) {
				MethodInfo the_setter = setter;
				if (the_setter.Mono_IsInflatedMethod)
					the_setter = the_setter.GetGenericMethodDefinition ();
				IMethodData md = TypeManager.GetMethod (the_setter);
				if (md != null)
					md.SetMemberIsUsed ();

				AccessorTable [setter] = PropertyInfo;
				is_static = setter.IsStatic;
			}
		}

		bool InstanceResolve (EmitContext ec, bool lvalue_instance, bool must_do_cs1540_check)
		{
			if (is_static) {
				InstanceExpression = null;
				return true;
			}

			if (InstanceExpression == null) {
				SimpleName.Error_ObjectRefRequired (ec, loc, PropertyInfo.Name);
				return false;
			}

			if (lvalue_instance)
				InstanceExpression = InstanceExpression.ResolveLValue (ec, EmptyExpression.LValueMemberAccess, loc);
			else
				InstanceExpression = InstanceExpression.DoResolve (ec);
			if (InstanceExpression == null)
				return false;
			
			InstanceExpression.CheckMarshallByRefAccess (ec.ContainerType);

			if (must_do_cs1540_check && InstanceExpression != EmptyExpression.Null &&
				InstanceExpression.Type != ec.ContainerType && 
				ec.ContainerType.IsSubclassOf (PropertyInfo.DeclaringType) &&
				InstanceExpression.Type.IsSubclassOf (PropertyInfo.DeclaringType)) {
					Error_CannotAccessProtected (loc, PropertyInfo, InstanceExpression.Type, ec.ContainerType);
					return false;
			}

			return true;
		}

		void Error_PropertyNotFound (MethodInfo mi, bool getter)
		{
			// TODO: correctly we should compare arguments but it will lead to bigger changes
			if (mi is MethodBuilder) {
				Error_TypeDoesNotContainDefinition (loc, PropertyInfo.DeclaringType, Name);
				return;
			}

			StringBuilder sig = new StringBuilder (TypeManager.CSharpName (mi.DeclaringType));
			sig.Append ('.');
			ParameterData iparams = TypeManager.GetParameterData (mi);
			sig.Append (getter ? "get_" : "set_");
			sig.Append (Name);
			sig.Append (iparams.GetSignatureForError ());

			Report.SymbolRelatedToPreviousError (mi);
			Report.Error (1546, loc, "Property `{0}' is not supported by the C# language. Try to call the accessor method `{1}' directly",
				Name, sig.ToString ());
		}
		
		override public Expression DoResolve (EmitContext ec)
		{
			if (resolved)
				return this;

			if (getter != null){
				if (TypeManager.GetParameterData (getter).Count != 0){
					Error_PropertyNotFound (getter, true);
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

				if (InstanceExpression != EmptyExpression.Null) {
					Report.Error (154, loc, "The property or indexer `{0}' cannot be used in this context because it lacks the `get' accessor",
						TypeManager.GetFullNameSignature (PropertyInfo));
					return null;
				}
			} 

			bool must_do_cs1540_check = false;
			if (getter != null &&
			    !IsAccessorAccessible (ec.ContainerType, getter, out must_do_cs1540_check)) {
				PropertyBase.PropertyMethod pm = TypeManager.GetMethod (getter) as PropertyBase.PropertyMethod;
				if (pm != null && pm.HasCustomAccessModifier) {
					Report.SymbolRelatedToPreviousError (pm);
					Report.Error (271, loc, "The property or indexer `{0}' cannot be used in this context because the get accessor is inaccessible",
						TypeManager.CSharpSignature (getter));
				}
				else
					ErrorIsInaccesible (loc, TypeManager.CSharpSignature (getter));
				return null;
			}

			if (!InstanceResolve (ec, false, must_do_cs1540_check))
				return null;

			//
			// Only base will allow this invocation to happen.
			//
			if (IsBase && getter.IsAbstract) {
				Error_CannotCallAbstractBase (TypeManager.GetFullNameSignature (PropertyInfo));
				return null;
			}

			if (PropertyInfo.PropertyType.IsPointer && !ec.InUnsafe){
				UnsafeError (loc);
				return null;
			}

			resolved = true;

			return this;
		}

		override public Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			if (right_side == EmptyExpression.OutAccess) {
				Report.Error (206, loc, "A property or indexer `{0}' may not be passed as an out or ref parameter",
					      GetSignatureForError ());
				return null;
			}

			if (right_side == EmptyExpression.LValueMemberAccess) {
				Report.Error (1612, loc, "Cannot modify the return value of `{0}' because it is not a variable",
					      GetSignatureForError ());
				return null;
			}

			if (setter == null){
				//
				// The following condition happens if the PropertyExpr was
				// created, but is invalid (ie, the property is inaccessible),
				// and we did not want to embed the knowledge about this in
				// the caller routine.  This only avoids double error reporting.
				//
				if (getter == null)
					return null;
				Report.Error (200, loc, "Property or indexer `{0}' cannot be assigned to (it is read only)",
					      GetSignatureForError ());
				return null;
			}

			if (TypeManager.GetParameterData (setter).Count != 1){
				Error_PropertyNotFound (setter, false);
				return null;
			}

			bool must_do_cs1540_check;
			if (!IsAccessorAccessible (ec.ContainerType, setter, out must_do_cs1540_check)) {
				PropertyBase.PropertyMethod pm = TypeManager.GetMethod (setter) as PropertyBase.PropertyMethod;
				if (pm != null && pm.HasCustomAccessModifier) {
					Report.SymbolRelatedToPreviousError (pm);
					Report.Error (272, loc, "The property or indexer `{0}' cannot be used in this context because the set accessor is inaccessible",
						TypeManager.CSharpSignature (setter));
				}
				else
					ErrorIsInaccesible (loc, TypeManager.CSharpSignature (setter));
				return null;
			}

			if (!InstanceResolve (ec, PropertyInfo.DeclaringType.IsValueType, must_do_cs1540_check))
				return null;
			
			//
			// Only base will allow this invocation to happen.
			//
			if (IsBase && setter.IsAbstract){
				Error_CannotCallAbstractBase (TypeManager.GetFullNameSignature (PropertyInfo));
				return null;
			}

			return this;
		}
		
		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}
		
		public void Emit (EmitContext ec, bool leave_copy)
		{
			//
			// Special case: length of single dimension array property is turned into ldlen
			//
			if ((getter == TypeManager.system_int_array_get_length) ||
			    (getter == TypeManager.int_array_get_length)){
				Type iet = InstanceExpression.Type;

				//
				// System.Array.Length can be called, but the Type does not
				// support invoking GetArrayRank, so test for that case first
				//
				if (iet != TypeManager.array_type && (iet.GetArrayRank () == 1)) {
					if (!prepared)
						EmitInstance (ec, false);
					ec.ig.Emit (OpCodes.Ldlen);
					ec.ig.Emit (OpCodes.Conv_I4);
					return;
				}
			}

			Invocation.EmitCall (ec, IsBase, IsStatic, InstanceExpression, getter, null, loc, prepared, false);
			
			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				if (!is_static) {
					temp = new LocalTemporary (ec, this.Type);
					temp.Store (ec);
				}
			}
		}

		//
		// Implements the IAssignMethod interface for assignments
		//
		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			Expression my_source = source;

			prepared = prepare_for_load;
			
			if (prepared) {
				source.Emit (ec);
				if (leave_copy) {
					ec.ig.Emit (OpCodes.Dup);
					if (!is_static) {
						temp = new LocalTemporary (ec, this.Type);
						temp.Store (ec);
					}
				}
			} else if (leave_copy) {
				source.Emit (ec);
				if (!is_static) {
					temp = new LocalTemporary (ec, this.Type);
					temp.Store (ec);
				}
				my_source = temp;
			}
			
			ArrayList args = new ArrayList (1);
			args.Add (new Argument (my_source, Argument.AType.Expression));
			
			Invocation.EmitCall (ec, IsBase, IsStatic, InstanceExpression, setter, args, loc, false, prepared);
			
			if (temp != null)
				temp.Emit (ec);
		}
	}

	/// <summary>
	///   Fully resolved expression that evaluates to an Event
	/// </summary>
	public class EventExpr : MemberExpr {
		public readonly EventInfo EventInfo;

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

		public override string Name {
			get {
				return EventInfo.Name;
			}
		}

		public override bool IsInstance {
			get {
				return !is_static;
			}
		}

		public override bool IsStatic {
			get {
				return is_static;
			}
		}

		public override Type DeclaringType {
			get {
				return EventInfo.DeclaringType;
			}
		}

		public override Expression ResolveMemberAccess (EmitContext ec, Expression left, Location loc,
								SimpleName original)
		{
			//
			// If the event is local to this class, we transform ourselves into a FieldExpr
			//

			if (EventInfo.DeclaringType == ec.ContainerType ||
			    TypeManager.IsNestedChildOf(ec.ContainerType, EventInfo.DeclaringType)) {
				MemberInfo mi = TypeManager.GetPrivateFieldOfEvent (EventInfo);

				if (mi != null) {
					MemberExpr ml = (MemberExpr) ExprClassFromMemberInfo (ec, mi, loc);

					if (ml == null) {
						Report.Error (-200, loc, "Internal error!!");
						return null;
					}

					InstanceExpression = null;
				
					return ml.ResolveMemberAccess (ec, left, loc, original);
				}
			}

			return base.ResolveMemberAccess (ec, left, loc, original);
		}


		bool InstanceResolve (EmitContext ec, bool must_do_cs1540_check)
		{
			if (is_static) {
				InstanceExpression = null;
				return true;
			}

			if (InstanceExpression == null) {
				SimpleName.Error_ObjectRefRequired (ec, loc, EventInfo.Name);
				return false;
			}

			InstanceExpression = InstanceExpression.DoResolve (ec);
			if (InstanceExpression == null)
				return false;

			//
			// This is using the same mechanism as the CS1540 check in PropertyExpr.
			// However, in the Event case, we reported a CS0122 instead.
			//
			if (must_do_cs1540_check && InstanceExpression != EmptyExpression.Null) {
				if ((InstanceExpression.Type != ec.ContainerType) &&
					ec.ContainerType.IsSubclassOf (InstanceExpression.Type)) {
					ErrorIsInaccesible (loc, TypeManager.CSharpSignature (EventInfo));
					return false;
				}
			}

			return true;
		}

		public override Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			return DoResolve (ec);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			bool must_do_cs1540_check;
			if (!(IsAccessorAccessible (ec.ContainerType, add_accessor, out must_do_cs1540_check) &&
			      IsAccessorAccessible (ec.ContainerType, remove_accessor, out must_do_cs1540_check))) {
				ErrorIsInaccesible (loc, TypeManager.CSharpSignature (EventInfo));
				return null;
			}

			if (!InstanceResolve (ec, must_do_cs1540_check))
				return null;
			
			return this;
		}		

		public override void Emit (EmitContext ec)
		{
			if (InstanceExpression is This)
				Report.Error (79, loc, "The event `{0}' can only appear on the left hand side of += or -=", GetSignatureForError ());
			else
				Report.Error (70, loc, "The event `{0}' can only appear on the left hand side of += or -= "+
					      "(except on the defining type)", Name);
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpSignature (EventInfo);
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
					ec, false, IsStatic, InstanceExpression, add_accessor, args, loc);
			else
				Invocation.EmitCall (
					ec, false, IsStatic, InstanceExpression, remove_accessor, args, loc);
		}
	}

	
	public class TemporaryVariable : Expression, IMemoryLocation
	{
		LocalInfo li;
		
		public TemporaryVariable (Type type, Location loc)
		{
			this.type = type;
			this.loc = loc;
			eclass = ExprClass.Value;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			if (li != null)
				return this;
			
			TypeExpr te = new TypeExpression (type, loc);
			li = ec.CurrentBlock.AddTemporaryVariable (te, loc);
			if (!li.Resolve (ec))
				return null;
			
			AnonymousContainer am = ec.CurrentAnonymousMethod;
			if ((am != null) && am.IsIterator)
				ec.CaptureVariable (li);
			
			return this;
		}
		
		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			if (li.FieldBuilder != null) {
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, li.FieldBuilder);
			} else {
				ig.Emit (OpCodes.Ldloc, li.LocalBuilder);
			}
		}
		
		public void EmitLoadAddress (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			if (li.FieldBuilder != null) {
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldflda, li.FieldBuilder);
			} else {
				ig.Emit (OpCodes.Ldloca, li.LocalBuilder);
			}
		}
		
		public void Store (EmitContext ec, Expression right_side)
		{
			if (li.FieldBuilder != null)
				ec.ig.Emit (OpCodes.Ldarg_0);
			
			right_side.Emit (ec);
			if (li.FieldBuilder != null) {
				ec.ig.Emit (OpCodes.Stfld, li.FieldBuilder);
			} else {
				ec.ig.Emit (OpCodes.Stloc, li.LocalBuilder);
			}
		}
		
		public void EmitThis (EmitContext ec)
		{
			if (li.FieldBuilder != null) {
				ec.ig.Emit (OpCodes.Ldarg_0);
			}
		}
		
		public void EmitStore (ILGenerator ig)
		{
			if (li.FieldBuilder != null)
				ig.Emit (OpCodes.Stfld, li.FieldBuilder);
			else
				ig.Emit (OpCodes.Stloc, li.LocalBuilder);
		}
		
		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			EmitLoadAddress (ec);
		}
	}
	
}	
