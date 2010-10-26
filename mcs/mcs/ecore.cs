//
// ecore.cs: Core of the Expression representation for the intermediate tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// Copyright 2001, 2002, 2003 Ximian, Inc.
// Copyright 2003-2008 Novell, Inc.
//
//

namespace Mono.CSharp {
	using System;
	using System.Collections;
	using System.Diagnostics;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;

#if NET_4_0
	using SLE = System.Linq.Expressions;
#endif

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
		TypeParameter,
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
		Type			= 1 << 1,

		// Returns a method group.
		MethodGroup		= 1 << 2,

		TypeParameter	= 1 << 3,

		// Mask of all the expression class flags.
		MaskExprClass = VariableOrValue | Type | MethodGroup | TypeParameter,

		// Disable control flow analysis while resolving the expression.
		// This is used when resolving the instance expression of a field expression.
		DisableFlowAnalysis	= 1 << 10,

		// Set if this is resolving the first part of a MemberAccess.
		Intermediate		= 1 << 11,

		// Disable control flow analysis _of struct_ while resolving the expression.
		// This is used when resolving the instance expression of a field expression.
		DisableStructFlowAnalysis	= 1 << 12,

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

	//
	// An expressions resolved as a direct variable reference
	//
	public interface IVariableReference : IFixedExpression
	{
		bool IsHoisted { get; }
		string Name { get; }
		VariableInfo VariableInfo { get; }

		void SetHasAddressTaken ();
	}

	//
	// Implemented by an expression which could be or is always
	// fixed
	//
	public interface IFixedExpression
	{
		bool IsFixed { get; }
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

		// Not nice but we have broken hierarchy.
		public virtual void CheckMarshalByRefAccess (ResolveContext ec)
		{
		}

		public virtual bool GetAttributableValue (ResolveContext ec, Type value_type, out object value)
		{
			Attribute.Error_AttributeArgumentNotValid (ec, loc);
			value = null;
			return false;
		}

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

			if (TypeManager.IsThisOrFriendAssembly (mi.DeclaringType.Assembly)) {
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

		public virtual bool IsNull {
			get {
				return false;
			}
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
		public abstract Expression DoResolve (ResolveContext ec);

		public virtual Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			return null;
		}

		//
		// This is used if the expression should be resolved as a type or namespace name.
		// the default implementation fails.   
		//
		public virtual FullNamedExpression ResolveAsTypeStep (IMemberContext rc,  bool silent)
		{
			if (!silent) {
				ResolveContext ec = new ResolveContext (rc);
				Expression e = Resolve (ec);
				if (e != null)
					e.Error_UnexpectedKind (ec, ResolveFlags.Type, loc);
			}

			return null;
		}

		//
		// C# 3.0 introduced contextual keywords (var) which behaves like a type if type with
		// same name exists or as a keyword when no type was found
		// 
		public virtual TypeExpr ResolveAsContextualType (IMemberContext rc, bool silent)
		{
			return ResolveAsTypeTerminal (rc, silent);
		}		
		
		//
		// This is used to resolve the expression as a type, a null
		// value will be returned if the expression is not a type
		// reference
		//
		public virtual TypeExpr ResolveAsTypeTerminal (IMemberContext ec, bool silent)
		{
			TypeExpr te = ResolveAsBaseTerminal (ec, silent);
			if (te == null)
				return null;

			if (!silent) { // && !(te is TypeParameterExpr)) {
				ObsoleteAttribute obsolete_attr = AttributeTester.GetObsoleteAttribute (te.Type);
				if (obsolete_attr != null && !ec.IsObsolete) {
					AttributeTester.Report_ObsoleteMessage (obsolete_attr, te.GetSignatureForError (), Location, ec.Compiler.Report);
				}
			}

			GenericTypeExpr ct = te as GenericTypeExpr;
			if (ct != null) {
				//
				// TODO: Constrained type parameters check for parameters of generic method overrides is broken
				// There are 2 solutions.
				// 1, Skip this check completely when we are in override/explicit impl scope
				// 2, Copy type parameters constraints from base implementation and pass (they have to be emitted anyway)
				//
				MemberCore gm = ec as GenericMethod;
				if (gm == null)
					gm = ec as Method;
				if (gm != null && ((gm.ModFlags & Modifiers.OVERRIDE) != 0 || gm.MemberName.Left != null)) {
					te.loc = loc;
					return te;
				}

				// TODO: silent flag is ignored
				ct.CheckConstraints (ec);
			}

			return te;
		}
	
		public TypeExpr ResolveAsBaseTerminal (IMemberContext ec, bool silent)
		{
			int errors = ec.Compiler.Report.Errors;

			FullNamedExpression fne = ResolveAsTypeStep (ec, silent);

			if (fne == null)
				return null;
				
			TypeExpr te = fne as TypeExpr;				
			if (te == null) {
				if (!silent && errors == ec.Compiler.Report.Errors)
					fne.Error_UnexpectedKind (ec.Compiler.Report, null, "type", loc);
				return null;
			}

			if (!te.CheckAccessLevel (ec)) {
				ec.Compiler.Report.SymbolRelatedToPreviousError (te.Type);
				ErrorIsInaccesible (loc, TypeManager.CSharpName (te.Type), ec.Compiler.Report);
				return null;
			}

			te.loc = loc;
			return te;
		}

		public static void ErrorIsInaccesible (Location loc, string name, Report Report)
		{
			Report.Error (122, loc, "`{0}' is inaccessible due to its protection level", name);
		}

		protected static void Error_CannotAccessProtected (ResolveContext ec, Location loc, MemberInfo m, Type qualifier, Type container)
		{
			ec.Report.Error (1540, loc, "Cannot access protected member `{0}' via a qualifier of type `{1}'."
				+ " The qualifier must be of type `{2}' or derived from it", 
				TypeManager.GetFullNameSignature (m),
				TypeManager.CSharpName (qualifier),
				TypeManager.CSharpName (container));

		}

		public static void Error_InvalidExpressionStatement (Report Report, Location loc)
		{
			Report.Error (201, loc, "Only assignment, call, increment, decrement, and new object " +
				       "expressions can be used as a statement");
		}
		
		public void Error_InvalidExpressionStatement (BlockContext ec)
		{
			Error_InvalidExpressionStatement (ec.Report, loc);
		}

		public static void Error_VoidInvalidInTheContext (Location loc, Report Report)
		{
			Report.Error (1547, loc, "Keyword `void' cannot be used in this context");
		}

		public virtual void Error_ValueCannotBeConverted (ResolveContext ec, Location loc, Type target, bool expl)
		{
			Error_ValueCannotBeConvertedCore (ec, loc, target, expl);
		}

		protected void Error_ValueCannotBeConvertedCore (ResolveContext ec, Location loc, Type target, bool expl)
		{
			// The error was already reported as CS1660
			if (type == InternalType.AnonymousMethod)
				return;

			if (TypeManager.IsGenericParameter (Type) && TypeManager.IsGenericParameter (target) && type.Name == target.Name) {
#if GMCS_SOURCE
				string sig1 = type.DeclaringMethod == null ?
					TypeManager.CSharpName (type.DeclaringType) :
					TypeManager.CSharpSignature (type.DeclaringMethod);
				string sig2 = target.DeclaringMethod == null ?
					TypeManager.CSharpName (target.DeclaringType) :
					TypeManager.CSharpSignature (target.DeclaringMethod);
				ec.Report.ExtraInformation (loc,
					String.Format (
						"The generic parameter `{0}' of `{1}' cannot be converted to the generic parameter `{0}' of `{2}' (in the previous ",
						Type.Name, sig1, sig2));
#endif
			} else if (Type.FullName == target.FullName){
				ec.Report.ExtraInformation (loc,
					String.Format (
					"The type `{0}' has two conflicting definitions, one comes from `{1}' and the other from `{2}' (in the previous ",
					Type.FullName, Type.Assembly.FullName, target.Assembly.FullName));
			}

			if (expl) {
				ec.Report.Error (30, loc, "Cannot convert type `{0}' to `{1}'",
					TypeManager.CSharpName (type), TypeManager.CSharpName (target));
				return;
			}

			ec.Report.DisableReporting ();
			bool expl_exists = Convert.ExplicitConversion (ec, this, target, Location.Null) != null;
			ec.Report.EnableReporting ();

			if (expl_exists) {
				ec.Report.Error (266, loc, "Cannot implicitly convert type `{0}' to `{1}'. " +
					      "An explicit conversion exists (are you missing a cast?)",
					TypeManager.CSharpName (Type), TypeManager.CSharpName (target));
				return;
			}

			ec.Report.Error (29, loc, "Cannot implicitly convert type `{0}' to `{1}'",
				TypeManager.CSharpName (type),
				TypeManager.CSharpName (target));
		}

		public virtual void Error_VariableIsUsedBeforeItIsDeclared (Report Report, string name)
		{
			Report.Error (841, loc, "A local variable `{0}' cannot be used before it is declared", name);
		}

		protected virtual void Error_TypeDoesNotContainDefinition (ResolveContext ec, Type type, string name)
		{
			Error_TypeDoesNotContainDefinition (ec, loc, type, name);
		}

		public static void Error_TypeDoesNotContainDefinition (ResolveContext ec, Location loc, Type type, string name)
		{
			ec.Report.SymbolRelatedToPreviousError (type);
			ec.Report.Error (117, loc, "`{0}' does not contain a definition for `{1}'",
				TypeManager.CSharpName (type), name);
		}

		protected static void Error_ValueAssignment (ResolveContext ec, Location loc)
		{
			ec.Report.Error (131, loc, "The left-hand side of an assignment must be a variable, a property or an indexer");
		}

		ResolveFlags ExprClassToResolveFlags
		{
			get {
				switch (eclass) {
				case ExprClass.Type:
				case ExprClass.Namespace:
					return ResolveFlags.Type;
					
				case ExprClass.MethodGroup:
					return ResolveFlags.MethodGroup;
					
				case ExprClass.TypeParameter:
					return ResolveFlags.TypeParameter;
					
				case ExprClass.Value:
				case ExprClass.Variable:
				case ExprClass.PropertyAccess:
				case ExprClass.EventAccess:
				case ExprClass.IndexerAccess:
					return ResolveFlags.VariableOrValue;
					
				default:
					throw new InternalErrorException (loc.ToString () + " " +  GetType () + " ExprClass is Invalid after resolve");
				}
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
		public Expression Resolve (ResolveContext ec, ResolveFlags flags)
		{
			if ((flags & ResolveFlags.MaskExprClass) == ResolveFlags.Type) 
				return ResolveAsTypeStep (ec, false);

			bool do_flow_analysis = ec.DoFlowAnalysis;
			bool omit_struct_analysis = ec.OmitStructFlowAnalysis;
			if ((flags & ResolveFlags.DisableFlowAnalysis) != 0)
				do_flow_analysis = false;
			if ((flags & ResolveFlags.DisableStructFlowAnalysis) != 0)
				omit_struct_analysis = true;

			Expression e;
			using (ec.WithFlowAnalysis (do_flow_analysis, omit_struct_analysis)) {
				if (this is SimpleName) {
					bool intermediate = (flags & ResolveFlags.Intermediate) == ResolveFlags.Intermediate;
					e = ((SimpleName) this).DoResolve (ec, intermediate);
				} else {
					e = DoResolve (ec);
				}
			}

			if (e == null)
				return null;

			if ((flags & e.ExprClassToResolveFlags) == 0) {
				e.Error_UnexpectedKind (ec, flags, loc);
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
		public Expression Resolve (ResolveContext ec)
		{
			Expression e = Resolve (ec, ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);

			if (e != null && e.eclass == ExprClass.MethodGroup && RootContext.Version == LanguageVersion.ISO_1) {
				((MethodGroupExpr) e).ReportUsageError (ec);
				return null;
			}
			return e;
		}

		public Constant ResolveAsConstant (ResolveContext ec, MemberCore mc)
		{
			Expression e = Resolve (ec);
			if (e == null)
				return null;

			Constant c = e as Constant;
			if (c != null)
				return c;

			if (type != null && TypeManager.IsReferenceType (type))
				Const.Error_ConstantCanBeInitializedWithNullOnly (type, loc, mc.GetSignatureForError (), ec.Report);
			else
				Const.Error_ExpressionMustBeConstant (loc, mc.GetSignatureForError (), ec.Report);

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
		public Expression ResolveLValue (ResolveContext ec, Expression right_side)
		{
			int errors = ec.Report.Errors;
			bool out_access = right_side == EmptyExpression.OutAccess;

			Expression e = DoResolveLValue (ec, right_side);

			if (e != null && out_access && !(e is IMemoryLocation)) {
				// FIXME: There's no problem with correctness, the 'Expr = null' handles that.
				//        Enabling this 'throw' will "only" result in deleting useless code elsewhere,

				//throw new InternalErrorException ("ResolveLValue didn't return an IMemoryLocation: " +
				//				  e.GetType () + " " + e.GetSignatureForError ());
				e = null;
			}

			if (e == null) {
				if (errors == ec.Report.Errors) {
					if (out_access)
						ec.Report.Error (1510, loc, "A ref or out argument must be an assignable variable");
					else
						Error_ValueAssignment (ec, loc);
				}
				return null;
			}

			if (e.eclass == ExprClass.Invalid)
				throw new Exception ("Expression " + e + " ExprClass is Invalid after resolve");

			if ((e.type == null) && !(e is GenericTypeExpr))
				throw new Exception ("Expression " + e + " did not set its type after Resolve");

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

		// Emit code to branch to @target if this expression is equivalent to @on_true.
		// The default implementation is to emit the value, and then emit a brtrue or brfalse.
		// Subclasses can provide more efficient implementations, but those MUST be equivalent,
		// including the use of conditional branches.  Note also that a branch MUST be emitted
		public virtual void EmitBranchable (EmitContext ec, Label target, bool on_true)
		{
			Emit (ec);
			ec.ig.Emit (on_true ? OpCodes.Brtrue : OpCodes.Brfalse, target);
		}

		// Emit this expression for its side effects, not for its value.
		// The default implementation is to emit the value, and then throw it away.
		// Subclasses can provide more efficient implementations, but those MUST be equivalent
		public virtual void EmitSideEffect (EmitContext ec)
		{
			Emit (ec);
			ec.ig.Emit (OpCodes.Pop);
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
		///   Returns a fully formed expression after a MemberLookup
		/// </summary>
		/// 
		public static Expression ExprClassFromMemberInfo (Type container_type, MemberInfo mi, Location loc)
		{
			if (mi is EventInfo)
				return new EventExpr ((EventInfo) mi, loc);
			else if (mi is FieldInfo) {
				FieldInfo fi = (FieldInfo) mi;
				if (fi.IsLiteral || (fi.IsInitOnly && fi.FieldType == TypeManager.decimal_type))
					return new ConstantExpr (fi, loc);
				return new FieldExpr (fi, loc);
			} else if (mi is PropertyInfo)
				return new PropertyExpr (container_type, (PropertyInfo) mi, loc);
			else if (mi is Type) {
				return new TypeExpression ((System.Type) mi, loc);
			}

			return null;
		}

		// TODO: [Obsolete ("Can be removed")]
		protected static ArrayList almost_matched_members = new ArrayList (4);

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

		public static Expression MemberLookup (CompilerContext ctx, Type container_type, Type queried_type, string name,
						       MemberTypes mt, BindingFlags bf, Location loc)
		{
			return MemberLookup (ctx, container_type, null, queried_type, name, mt, bf, loc);
		}

		//
		// Lookup type `queried_type' for code in class `container_type' with a qualifier of
		// `qualifier_type' or null to lookup members in the current class.
		//

		public static Expression MemberLookup (CompilerContext ctx, Type container_type,
						       Type qualifier_type, Type queried_type,
						       string name, MemberTypes mt,
						       BindingFlags bf, Location loc)
		{
			almost_matched_members.Clear ();

			MemberInfo [] mi = TypeManager.MemberLookup (container_type, qualifier_type,
								     queried_type, mt, bf, name, almost_matched_members);

			if (mi == null)
				return null;

			if (mi.Length > 1) {
				bool is_interface = qualifier_type != null && qualifier_type.IsInterface;
				ArrayList methods = new ArrayList (2);
				ArrayList non_methods = null;

				foreach (MemberInfo m in mi) {
					if (m is MethodBase) {
						methods.Add (m);
						continue;
					}

					if (non_methods == null)
						non_methods = new ArrayList (2);

					bool is_candidate = true;
					for (int i = 0; i < non_methods.Count; ++i) {
						MemberInfo n_m = (MemberInfo) non_methods [i];
						if (n_m.DeclaringType.IsInterface && TypeManager.ImplementsInterface (m.DeclaringType, n_m.DeclaringType)) {
							non_methods.Remove (n_m);
							--i;
						} else if (m.DeclaringType.IsInterface && TypeManager.ImplementsInterface (n_m.DeclaringType, m.DeclaringType)) {
							is_candidate = false;
							break;
						}
					}
					
					if (is_candidate) {
						non_methods.Add (m);
					}
				}
				
				if (methods.Count == 0 && non_methods != null && non_methods.Count > 1) {
					ctx.Report.SymbolRelatedToPreviousError ((MemberInfo)non_methods [1]);
					ctx.Report.SymbolRelatedToPreviousError ((MemberInfo)non_methods [0]);
					ctx.Report.Error (229, loc, "Ambiguity between `{0}' and `{1}'",
						TypeManager.GetFullNameSignature ((MemberInfo)non_methods [1]),
						TypeManager.GetFullNameSignature ((MemberInfo)non_methods [0]));
					return null;
				}

				if (methods.Count == 0)
					return ExprClassFromMemberInfo (container_type, (MemberInfo)non_methods [0], loc);

				if (non_methods != null && non_methods.Count > 0) {
					MethodBase method = (MethodBase) methods [0];
					MemberInfo non_method = (MemberInfo) non_methods [0];
					if (method.DeclaringType == non_method.DeclaringType) {
						// Cannot happen with C# code, but is valid in IL
						ctx.Report.SymbolRelatedToPreviousError (method);
						ctx.Report.SymbolRelatedToPreviousError (non_method);
						ctx.Report.Error (229, loc, "Ambiguity between `{0}' and `{1}'",
							      TypeManager.GetFullNameSignature (non_method),
							      TypeManager.CSharpSignature (method));
						return null;
					}

					if (is_interface) {
						ctx.Report.SymbolRelatedToPreviousError (method);
						ctx.Report.SymbolRelatedToPreviousError (non_method);
						ctx.Report.Warning (467, 2, loc, "Ambiguity between method `{0}' and non-method `{1}'. Using method `{0}'",
								TypeManager.CSharpSignature (method), TypeManager.GetFullNameSignature (non_method));
					}
				}

				return new MethodGroupExpr (methods, queried_type, loc);
			}

			if (mi [0] is MethodBase)
				return new MethodGroupExpr (mi, queried_type, loc);

			return ExprClassFromMemberInfo (container_type, mi [0], loc);
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

		public static Expression MemberLookup (CompilerContext ctx, Type container_type, Type queried_type,
						       string name, Location loc)
		{
			return MemberLookup (ctx, container_type, null, queried_type, name,
					     AllMemberTypes, AllBindingFlags, loc);
		}

		public static Expression MemberLookup (CompilerContext ctx, Type container_type, Type qualifier_type,
						       Type queried_type, string name, Location loc)
		{
			return MemberLookup (ctx, container_type, qualifier_type, queried_type,
					     name, AllMemberTypes, AllBindingFlags, loc);
		}

		public static MethodGroupExpr MethodLookup (CompilerContext ctx, Type container_type, Type queried_type,
						       string name, Location loc)
		{
			return (MethodGroupExpr)MemberLookup (ctx, container_type, null, queried_type, name,
					     MemberTypes.Method, AllBindingFlags, loc);
		}

		/// <summary>
		///   This is a wrapper for MemberLookup that is not used to "probe", but
		///   to find a final definition.  If the final definition is not found, we
		///   look for private members and display a useful debugging message if we
		///   find it.
		/// </summary>
		protected Expression MemberLookupFinal (ResolveContext ec, Type qualifier_type,
							    Type queried_type, string name,
							    MemberTypes mt, BindingFlags bf,
							    Location loc)
		{
			Expression e;

			int errors = ec.Report.Errors;
			e = MemberLookup (ec.Compiler, ec.CurrentType, qualifier_type, queried_type, name, mt, bf, loc);

			if (e != null || errors != ec.Report.Errors)
				return e;

			// No errors were reported by MemberLookup, but there was an error.
			return Error_MemberLookupFailed (ec, ec.CurrentType, qualifier_type, queried_type,
					name, null, mt, bf);
		}

		protected virtual Expression Error_MemberLookupFailed (ResolveContext ec, Type container_type, Type qualifier_type,
						       Type queried_type, string name, string class_name,
							   MemberTypes mt, BindingFlags bf)
		{
			MemberInfo[] lookup = null;
			if (queried_type == null) {
				class_name = "global::";
			} else {
				lookup = TypeManager.MemberLookup (queried_type, null, queried_type,
					mt, (bf & ~BindingFlags.Public) | BindingFlags.NonPublic,
					name, null);

				if (lookup != null) {
					Expression e = Error_MemberLookupFailed (ec, queried_type, lookup);

					//
					// FIXME: This is still very wrong, it should be done inside
					// OverloadResolve to do correct arguments matching.
					// Requires MemberLookup accessiblity check removal
					//
					if (e == null || (mt & (MemberTypes.Method | MemberTypes.Constructor)) == 0) {
						MemberInfo mi = lookup[0];
						ec.Report.SymbolRelatedToPreviousError (mi);
						if (qualifier_type != null && container_type != null && qualifier_type != container_type &&
							TypeManager.IsNestedFamilyAccessible (container_type, mi.DeclaringType)) {
							// Although a derived class can access protected members of
							// its base class it cannot do so through an instance of the
							// base class (CS1540).  If the qualifier_type is a base of the
							// ec.CurrentType and the lookup succeeds with the latter one,
							// then we are in this situation.
							Error_CannotAccessProtected (ec, loc, mi, qualifier_type, container_type);
						} else {
							ErrorIsInaccesible (loc, TypeManager.GetFullNameSignature (mi), ec.Report);
						}
					}

					return e;
				}

				lookup = TypeManager.MemberLookup (queried_type, null, queried_type,
					AllMemberTypes, AllBindingFlags | BindingFlags.NonPublic,
					name, null);
			}

			if (lookup == null) {
				if (class_name != null) {
					ec.Report.Error (103, loc, "The name `{0}' does not exist in the current context",
						name);
				} else {
					Error_TypeDoesNotContainDefinition (ec, queried_type, name);
				}
				return null;
			}

			if (TypeManager.MemberLookup (queried_type, null, queried_type,
						      AllMemberTypes, AllBindingFlags |
						      BindingFlags.NonPublic, name, null) == null) {
				if ((lookup.Length == 1) && (lookup [0] is Type)) {
					Type t = (Type) lookup [0];

					ec.Report.Error (305, loc,
						      "Using the generic type `{0}' " +
						      "requires {1} type arguments",
						      TypeManager.CSharpName (t),
						      TypeManager.GetNumberOfTypeArguments (t).ToString ());
					return null;
				}
			}

			return Error_MemberLookupFailed (ec, queried_type, lookup);
		}

		protected virtual Expression Error_MemberLookupFailed (ResolveContext ec, Type type, MemberInfo[] members)
		{
			for (int i = 0; i < members.Length; ++i) {
				if (!(members [i] is MethodBase))
					return null;
			}

			// By default propagate the closest candidates upwards
			return new MethodGroupExpr (members, type, loc, true);
		}

		protected virtual void Error_NegativeArrayIndex (ResolveContext ec, Location loc)
		{
			throw new NotImplementedException ();
		}

		protected void Error_PointerInsideExpressionTree (ResolveContext ec)
		{
			ec.Report.Error (1944, loc, "An expression tree cannot contain an unsafe pointer operation");
		}

		/// <summary>
		///   Returns an expression that can be used to invoke operator true
		///   on the expression if it exists.
		/// </summary>
		static public Expression GetOperatorTrue (ResolveContext ec, Expression e, Location loc)
		{
			return GetOperatorTrueOrFalse (ec, e, true, loc);
		}

		/// <summary>
		///   Returns an expression that can be used to invoke operator false
		///   on the expression if it exists.
		/// </summary>
		static public Expression GetOperatorFalse (ResolveContext ec, Expression e, Location loc)
		{
			return GetOperatorTrueOrFalse (ec, e, false, loc);
		}

		static Expression GetOperatorTrueOrFalse (ResolveContext ec, Expression e, bool is_true, Location loc)
		{
			MethodGroupExpr operator_group;
			string mname = Operator.GetMetadataName (is_true ? Operator.OpType.True : Operator.OpType.False);
			operator_group = MethodLookup (ec.Compiler, ec.CurrentType, e.Type, mname, loc) as MethodGroupExpr;
			if (operator_group == null)
				return null;

			Arguments arguments = new Arguments (1);
			arguments.Add (new Argument (e));
			operator_group = operator_group.OverloadResolve (
				ec, ref arguments, false, loc);

			if (operator_group == null)
				return null;

			return new UserOperatorCall (operator_group, arguments, null, loc);
		}

		/// <summary>
		///   Resolves the expression `e' into a boolean expression: either through
		///   an implicit conversion, or through an `operator true' invocation
		/// </summary>
		public static Expression ResolveBoolean (ResolveContext ec, Expression e, Location loc)
		{
			e = e.Resolve (ec);
			if (e == null)
				return null;

			if (e.Type == TypeManager.bool_type)
				return e;

			if (TypeManager.IsDynamicType (e.Type)) {
				Arguments args = new Arguments (1);
				args.Add (new Argument (e));
				return new DynamicUnaryConversion ("IsTrue", args, loc).Resolve (ec);
			}

			Expression converted = Convert.ImplicitConversion (ec, e, TypeManager.bool_type, Location.Null);

			if (converted != null)
				return converted;

			//
			// If no implicit conversion to bool exists, try using `operator true'
			//
			converted = Expression.GetOperatorTrue (ec, e, loc);
			if (converted == null){
				e.Error_ValueCannotBeConverted (ec, loc, TypeManager.bool_type, false);
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
				case ExprClass.TypeParameter:
					return "type parameter";
				}
				throw new Exception ("Should not happen");
			}
		}
		
		/// <summary>
		///   Reports that we were expecting `expr' to be of class `expected'
		/// </summary>
		public void Error_UnexpectedKind (Report r, MemberCore mc, string expected, Location loc)
		{
			Error_UnexpectedKind (r, mc, expected, ExprClassName, loc);
		}

		public void Error_UnexpectedKind (Report r, MemberCore mc, string expected, string was, Location loc)
		{
			string name;
			if (mc != null)
				name = mc.GetSignatureForError ();
			else
				name = GetSignatureForError ();

			r.Error (118, loc, "`{0}' is a `{1}' but a `{2}' was expected",
			      name, was, expected);
		}

		public void Error_UnexpectedKind (ResolveContext ec, ResolveFlags flags, Location loc)
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

			ec.Report.Error (119, loc, 
				"Expression denotes a `{0}', where a `{1}' was expected", ExprClassName, sb.ToString ());
		}
		
		public static void UnsafeError (ResolveContext ec, Location loc)
		{
			UnsafeError (ec.Report, loc);
		}

		public static void UnsafeError (Report Report, Location loc)
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
					LoadFromPtr (ig, TypeManager.GetEnumUnderlyingType (t));
			} else if (TypeManager.IsStruct (t) || TypeManager.IsGenericParameter (t))
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
				type = TypeManager.GetEnumUnderlyingType (type);
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
			else if (TypeManager.IsStruct (type) || TypeManager.IsGenericParameter (type))
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

		protected void Error_CannotCallAbstractBase (ResolveContext ec, string name)
		{
			ec.Report.Error (205, loc, "Cannot call an abstract base member `{0}'", name);
		}
		
		protected void Error_CannotModifyIntermediateExpressionValue (ResolveContext ec)
		{
			ec.Report.SymbolRelatedToPreviousError (type);
			if (ec.CurrentInitializerVariable != null) {
				ec.Report.Error (1918, loc, "Members of value type `{0}' cannot be assigned using a property `{1}' object initializer",
					TypeManager.CSharpName (type), GetSignatureForError ());
			} else {
				ec.Report.Error (1612, loc, "Cannot modify a value type return value of `{0}'. Consider storing the value in a temporary variable",
					GetSignatureForError ());
			}
		}

		//
		// Converts `source' to an int, uint, long or ulong.
		//
		protected Expression ConvertExpressionToArrayIndex (ResolveContext ec, Expression source)
		{
			if (TypeManager.IsDynamicType (source.type)) {
				Arguments args = new Arguments (1);
				args.Add (new Argument (source));
				return new DynamicConversion (TypeManager.int32_type, false, args, loc).Resolve (ec);
			}

			Expression converted;
			
			using (ec.Set (ResolveContext.Options.CheckedScope)) {
				converted = Convert.ImplicitConversion (ec, source, TypeManager.int32_type, source.loc);
				if (converted == null)
					converted = Convert.ImplicitConversion (ec, source, TypeManager.uint32_type, source.loc);
				if (converted == null)
					converted = Convert.ImplicitConversion (ec, source, TypeManager.int64_type, source.loc);
				if (converted == null)
					converted = Convert.ImplicitConversion (ec, source, TypeManager.uint64_type, source.loc);

				if (converted == null) {
					source.Error_ValueCannotBeConverted (ec, source.loc, TypeManager.int32_type, false);
					return null;
				}
			}

			//
			// Only positive constants are allowed at compile time
			//
			Constant c = converted as Constant;
			if (c != null && c.IsNegative)
				Error_NegativeArrayIndex (ec, source.loc);

			// No conversion needed to array index
			if (converted.Type == TypeManager.int32_type)
				return converted;

			return new ArrayIndexCast (converted).Resolve (ec);
		}

		//
		// Derived classes implement this method by cloning the fields that
		// could become altered during the Resolve stage
		//
		// Only expressions that are created for the parser need to implement
		// this.
		//
		protected virtual void CloneTo (CloneContext clonectx, Expression target)
		{
			throw new NotImplementedException (
				String.Format (
					"CloneTo not implemented for expression {0}", this.GetType ()));
		}

		//
		// Clones an expression created by the parser.
		//
		// We only support expressions created by the parser so far, not
		// expressions that have been resolved (many more classes would need
		// to implement CloneTo).
		//
		// This infrastructure is here merely for Lambda expressions which
		// compile the same code using different type values for the same
		// arguments to find the correct overload
		//
		public Expression Clone (CloneContext clonectx)
		{
			Expression cloned = (Expression) MemberwiseClone ();
			CloneTo (clonectx, cloned);

			return cloned;
		}

		//
		// Implementation of expression to expression tree conversion
		//
		public abstract Expression CreateExpressionTree (ResolveContext ec);

		protected Expression CreateExpressionFactoryCall (ResolveContext ec, string name, Arguments args)
		{
			return CreateExpressionFactoryCall (ec, name, null, args, loc);
		}

		protected Expression CreateExpressionFactoryCall (ResolveContext ec, string name, TypeArguments typeArguments, Arguments args)
		{
			return CreateExpressionFactoryCall (ec, name, typeArguments, args, loc);
		}

		public static Expression CreateExpressionFactoryCall (ResolveContext ec, string name, TypeArguments typeArguments, Arguments args, Location loc)
		{
			return new Invocation (new MemberAccess (CreateExpressionTypeExpression (ec, loc), name, typeArguments, loc), args);
		}

		protected static TypeExpr CreateExpressionTypeExpression (ResolveContext ec, Location loc)
		{
			TypeExpr texpr = TypeManager.expression_type_expr;
			if (texpr == null) {
				Type t = TypeManager.CoreLookupType (ec.Compiler, "System.Linq.Expressions", "Expression", Kind.Class, true);
				if (t == null)
					return null;

				TypeManager.expression_type_expr = texpr = new TypeExpression (t, Location.Null);
			}

			return texpr;
		}

#if NET_4_0
		//
		// Implemented by all expressions which support conversion from
		// compiler expression to invokable runtime expression. Used by
		// dynamic C# binder.
		//
		public virtual SLE.Expression MakeExpression (BuilderContext ctx)
		{
			throw new NotImplementedException ("MakeExpression for " + GetType ());
		}
#endif

		public virtual void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			// TODO: It should probably be type = storey.MutateType (type);
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

		public virtual ExpressionStatement ResolveStatement (BlockContext ec)
		{
			Expression e = Resolve (ec);
			if (e == null)
				return null;

			ExpressionStatement es = e as ExpressionStatement;
			if (es == null)
				Error_InvalidExpressionStatement (ec);

			return es;
		}

		/// <summary>
		///   Requests the expression to be emitted in a `statement'
		///   context.  This means that no new value is left on the
		///   stack after invoking this method (constrasted with
		///   Emit that will always leave a value on the stack).
		/// </summary>
		public abstract void EmitStatement (EmitContext ec);

		public override void EmitSideEffect (EmitContext ec)
		{
			EmitStatement (ec);
		}
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
	public abstract class TypeCast : Expression
	{
		protected readonly Expression child;

		protected TypeCast (Expression child, Type return_type)
		{
			eclass = child.eclass;
			loc = child.Location;
			type = return_type;
			this.child = child;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (2);
			args.Add (new Argument (child.CreateExpressionTree (ec)));
			args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));

			if (type.IsPointer || child.Type.IsPointer)
				Error_PointerInsideExpressionTree (ec);

			return CreateExpressionFactoryCall (ec, ec.HasSet (ResolveContext.Options.CheckedScope) ? "ConvertChecked" : "Convert", args);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);
		}

		public override bool GetAttributableValue (ResolveContext ec, Type value_type, out object value)
		{
			return child.GetAttributableValue (ec, value_type, out value);
		}

#if NET_4_0
		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			return ctx.HasSet (BuilderContext.Options.CheckedScope) ?
				SLE.Expression.ConvertChecked (child.MakeExpression (ctx), type) :
				SLE.Expression.Convert (child.MakeExpression (ctx), type);
		}
#endif

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			type = storey.MutateType (type);
			child.MutateHoistedGenericType (storey);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			// Nothing to clone
		}

		public override bool IsNull {
			get { return child.IsNull; }
		}
	}

	public class EmptyCast : TypeCast {
		EmptyCast (Expression child, Type target_type)
			: base (child, target_type)
		{
		}

		public static Expression Create (Expression child, Type type)
		{
			Constant c = child as Constant;
			if (c != null)
				return new EmptyConstantCast (c, type);

			EmptyCast e = child as EmptyCast;
			if (e != null)
				return new EmptyCast (e.child, type);

			return new EmptyCast (child, type);
		}

		public override void EmitBranchable (EmitContext ec, Label label, bool on_true)
		{
			child.EmitBranchable (ec, label, on_true);
		}

		public override void EmitSideEffect (EmitContext ec)
		{
			child.EmitSideEffect (ec);
		}
	}

	//
	// Used for predefined class library user casts (no obsolete check, etc.)
	//
	public class OperatorCast : TypeCast {
		MethodInfo conversion_operator;
			
		public OperatorCast (Expression child, Type target_type) 
			: this (child, target_type, false)
		{
		}

		public OperatorCast (Expression child, Type target_type, bool find_explicit)
			: base (child, target_type)
		{
			conversion_operator = GetConversionOperator (find_explicit);
			if (conversion_operator == null)
				throw new InternalErrorException ("Outer conversion routine is out of sync");
		}

		// Returns the implicit operator that converts from
		// 'child.Type' to our target type (type)
		MethodInfo GetConversionOperator (bool find_explicit)
		{
			string operator_name = find_explicit ? "op_Explicit" : "op_Implicit";

			MemberInfo [] mi;

			mi = TypeManager.MemberLookup (child.Type, child.Type, child.Type, MemberTypes.Method,
				BindingFlags.Static | BindingFlags.Public, operator_name, null);

			if (mi == null){
				mi = TypeManager.MemberLookup (type, type, type, MemberTypes.Method,
							       BindingFlags.Static | BindingFlags.Public, operator_name, null);
			}
			
			foreach (MethodInfo oper in mi) {
				AParametersCollection pd = TypeManager.GetParameterData (oper);

				if (pd.Types [0] == child.Type && TypeManager.TypeToCoreType (oper.ReturnType) == type)
					return oper;
			}

			return null;
		}

		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);
			ec.ig.Emit (OpCodes.Call, conversion_operator);
		}
	}
	
	/// <summary>
	/// 	This is a numeric cast to a Decimal
	/// </summary>
	public class CastToDecimal : OperatorCast {
		public CastToDecimal (Expression child)
			: this (child, false)
		{
		}

		public CastToDecimal (Expression child, bool find_explicit)
			: base (child, TypeManager.decimal_type, find_explicit)
		{
		}
	}

	/// <summary>
	/// 	This is an explicit numeric cast from a Decimal
	/// </summary>
	public class CastFromDecimal : TypeCast
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
					AParametersCollection pd = TypeManager.GetParameterData (oper);
					if (pd.Types [0] == TypeManager.decimal_type)
						operators.Add (TypeManager.TypeToCoreType (oper.ReturnType), oper);
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
	// Constant specialization of EmptyCast.
	// We need to special case this since an empty cast of
	// a constant is still a constant. 
	//
	public class EmptyConstantCast : Constant
	{
		public readonly Constant child;

		public EmptyConstantCast(Constant child, Type type)
			: base (child.Location)
		{
			eclass = child.eclass;
			this.child = child;
			this.type = type;
		}

		public override string AsString ()
		{
			return child.AsString ();
		}

		public override object GetValue ()
		{
			return child.GetValue ();
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			// FIXME: check that 'type' can be converted to 'target_type' first
			return child.ConvertExplicitly (in_checked_context, target_type);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree (ec, null,
				child.CreateExpressionTree (ec),
				new TypeOf (new TypeExpression (type, loc), loc));

			if (type.IsPointer)
				Error_PointerInsideExpressionTree (ec);

			return CreateExpressionFactoryCall (ec, "Convert", args);
		}

		public override Constant Increment ()
		{
			return child.Increment ();
		}

		public override bool IsDefaultValue {
			get { return child.IsDefaultValue; }
		}

		public override bool IsNegative {
			get { return child.IsNegative; }
		}

		public override bool IsNull {
			get { return child.IsNull; }
		}

		public override bool IsZeroInteger {
			get { return child.IsZeroInteger; }
		}
		
		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);			
		}

		public override void EmitBranchable (EmitContext ec, Label label, bool on_true)
		{
			child.EmitBranchable (ec, label, on_true);

#if GMCS_SOURCE
			// Only to make verifier happy
			if (TypeManager.IsGenericParameter (type) && child.IsNull)
				ec.ig.Emit (OpCodes.Unbox_Any, type);
#endif
		}

		public override void EmitSideEffect (EmitContext ec)
		{
			child.EmitSideEffect (ec);
		}

		public override Constant ConvertImplicitly (Type target_type)
		{
			// FIXME: Do we need to check user conversions?
			if (!Convert.ImplicitStandardConversionExists (this, target_type))
				return null;
			return child.ConvertImplicitly (target_type);
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			child.MutateHoistedGenericType (storey);
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
		
		public override Expression DoResolve (ResolveContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Child.Emit (ec);
		}

		public override void EmitBranchable (EmitContext ec, Label label, bool on_true)
		{
			Child.EmitBranchable (ec, label, on_true);
		}

		public override void EmitSideEffect (EmitContext ec)
		{
			Child.EmitSideEffect (ec);
		}

		public override bool GetAttributableValue (ResolveContext ec, Type value_type, out object value)
		{
			value = GetTypedValue ();
			return true;
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

#if MS_COMPATIBLE
			// Small workaround for big problem
			// System.Enum.ToObject cannot be called on dynamic types
			// EnumBuilder has to be used, but we cannot use EnumBuilder
			// because it does not properly support generics
			//
			// This works only sometimes
			//
			if (type.Module == RootContext.ToplevelTypes.Builder)
				return Child.GetValue ();
#endif

			return System.Enum.ToObject (type, Child.GetValue ());
		}
		
		public override string AsString ()
		{
			return Child.AsString ();
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

		public override Constant ConvertExplicitly(bool in_checked_context, Type target_type)
		{
			if (Child.Type == target_type)
				return Child;

			return Child.ConvertExplicitly (in_checked_context, target_type);
		}

		public override Constant ConvertImplicitly (Type type)
		{
			Type this_type = TypeManager.DropGenericTypeArguments (Type);
			type = TypeManager.DropGenericTypeArguments (type);

			if (this_type == type) {
				// This is workaround of mono bug. It can be removed when the latest corlib spreads enough
				if (TypeManager.IsEnumType (type.UnderlyingSystemType))
					return this;

				Type child_type = TypeManager.DropGenericTypeArguments (Child.Type);
				if (type.UnderlyingSystemType != child_type)
					Child = Child.ConvertImplicitly (type.UnderlyingSystemType);
				return this;
			}

			if (!Convert.ImplicitStandardConversionExists (this, type)){
				return null;
			}

			return Child.ConvertImplicitly(type);
		}

	}

	/// <summary>
	///   This kind of cast is used to encapsulate Value Types in objects.
	///
	///   The effect of it is to box the value type emitted by the previous
	///   operation.
	/// </summary>
	public class BoxedCast : TypeCast {

		public BoxedCast (Expression expr, Type target_type)
			: base (expr, target_type)
		{
			eclass = ExprClass.Value;
		}
		
		public override Expression DoResolve (ResolveContext ec)
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

		public override void EmitSideEffect (EmitContext ec)
		{
			// boxing is side-effectful, since it involves runtime checks, except when boxing to Object or ValueType
			// so, we need to emit the box+pop instructions in most cases
			if (TypeManager.IsStruct (child.Type) &&
			    (type == TypeManager.object_type || type == TypeManager.value_type))
				child.EmitSideEffect (ec);
			else
				base.EmitSideEffect (ec);
		}
	}

	public class UnboxCast : TypeCast {
		public UnboxCast (Expression expr, Type return_type)
			: base (expr, return_type)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			if (right_side == EmptyExpression.LValueMemberAccess || right_side == EmptyExpression.LValueMemberOutAccess)
				ec.Report.Error (445, loc, "Cannot modify the result of an unboxing conversion");
			return base.DoResolveLValue (ec, right_side);
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);

			ILGenerator ig = ec.ig;

#if GMCS_SOURCE
			ig.Emit (OpCodes.Unbox_Any, type);
#else			
			ig.Emit (OpCodes.Unbox, type);
			LoadFromPtr (ig, type);
#endif			
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			type = storey.MutateType (type);
			base.MutateHoistedGenericType (storey);			
		}
	}
	
	/// <summary>
	///   This is used to perform explicit numeric conversions.
	///
	///   Explicit numeric conversions might trigger exceptions in a checked
	///   context, so they should generate the conv.ovf opcodes instead of
	///   conv opcodes.
	/// </summary>
	public class ConvCast : TypeCast {
		public enum Mode : byte {
			I1_U1, I1_U2, I1_U4, I1_U8, I1_CH,
			U1_I1, U1_CH,
			I2_I1, I2_U1, I2_U2, I2_U4, I2_U8, I2_CH,
			U2_I1, U2_U1, U2_I2, U2_CH,
			I4_I1, I4_U1, I4_I2, I4_U2, I4_U4, I4_U8, I4_CH,
			U4_I1, U4_U1, U4_I2, U4_U2, U4_I4, U4_CH,
			I8_I1, I8_U1, I8_I2, I8_U2, I8_I4, I8_U4, I8_U8, I8_CH, I8_I,
			U8_I1, U8_U1, U8_I2, U8_U2, U8_I4, U8_U4, U8_I8, U8_CH, U8_I,
			CH_I1, CH_U1, CH_I2,
			R4_I1, R4_U1, R4_I2, R4_U2, R4_I4, R4_U4, R4_I8, R4_U8, R4_CH,
			R8_I1, R8_U1, R8_I2, R8_U2, R8_I4, R8_U4, R8_I8, R8_U8, R8_CH, R8_R4,
			I_I8,
		}

		Mode mode;
		
		public ConvCast (Expression child, Type return_type, Mode m)
			: base (child, return_type)
		{
			mode = m;
		}

		public override Expression DoResolve (ResolveContext ec)
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

			if (ec.HasSet (EmitContext.Options.CheckedScope)) {
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
				case Mode.I8_I: ig.Emit (OpCodes.Conv_Ovf_U); break;

				case Mode.U8_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U8_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U8_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U8_U2: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;
				case Mode.U8_I4: ig.Emit (OpCodes.Conv_Ovf_I4_Un); break;
				case Mode.U8_U4: ig.Emit (OpCodes.Conv_Ovf_U4_Un); break;
				case Mode.U8_I8: ig.Emit (OpCodes.Conv_Ovf_I8_Un); break;
				case Mode.U8_CH: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;
				case Mode.U8_I: ig.Emit (OpCodes.Conv_Ovf_U_Un); break;

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

				case Mode.I_I8: ig.Emit (OpCodes.Conv_Ovf_I8_Un); break;
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
				case Mode.I8_I: ig.Emit (OpCodes.Conv_U); break;

				case Mode.U8_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U8_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.U8_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.U8_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.U8_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.U8_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.U8_I8: /* nothing */ break;
				case Mode.U8_CH: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.U8_I: ig.Emit (OpCodes.Conv_U); break;

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

				case Mode.I_I8: ig.Emit (OpCodes.Conv_U8); break;
				}
			}
		}
	}
	
	public class OpcodeCast : TypeCast {
		readonly OpCode op;
		
		public OpcodeCast (Expression child, Type return_type, OpCode op)
			: base (child, return_type)
		{
			this.op = op;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			ec.ig.Emit (op);
		}

		public Type UnderlyingType {
			get { return child.Type; }
		}
	}

	/// <summary>
	///   This kind of cast is used to encapsulate a child and cast it
	///   to the class requested
	/// </summary>
	public sealed class ClassCast : TypeCast {
		readonly bool forced;
		
		public ClassCast (Expression child, Type return_type)
			: base (child, return_type)
		{
		}
		
		public ClassCast (Expression child, Type return_type, bool forced)
			: base (child, return_type)
		{
			this.forced = forced;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);

#if GMCS_SOURCE
			bool gen = TypeManager.IsGenericParameter (child.Type);
			if (gen)
				ec.ig.Emit (OpCodes.Box, child.Type);
			
			if (type.IsGenericParameter) {
				ec.ig.Emit (OpCodes.Unbox_Any, type);
				return;
			}
			
			if (gen && !forced)
				return;
#endif
			
			ec.ig.Emit (OpCodes.Castclass, type);
		}
	}

	//
	// Created during resolving pahse when an expression is wrapped or constantified
	// and original expression can be used later (e.g. for expression trees)
	//
	public class ReducedExpression : Expression
	{
		sealed class ReducedConstantExpression : EmptyConstantCast
		{
			readonly Expression orig_expr;

			public ReducedConstantExpression (Constant expr, Expression orig_expr)
				: base (expr, expr.Type)
			{
				this.orig_expr = orig_expr;
			}

			public override Constant ConvertImplicitly (Type target_type)
			{
				Constant c = base.ConvertImplicitly (target_type);
				if (c != null)
					c = new ReducedConstantExpression (c, orig_expr);
				return c;
			}

			public override Expression CreateExpressionTree (ResolveContext ec)
			{
				return orig_expr.CreateExpressionTree (ec);
			}

			public override bool GetAttributableValue (ResolveContext ec, Type value_type, out object value)
			{
				//
				// Even if resolved result is a constant original expression was not
				// and attribute accepts constants only
				//
				Attribute.Error_AttributeArgumentNotValid (ec, orig_expr.Location);
				value = null;
				return false;
			}

			public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
			{
				Constant c = base.ConvertExplicitly (in_checked_context, target_type);
				if (c != null)
					c = new ReducedConstantExpression (c, orig_expr);
				return c;
			}
		}

		sealed class ReducedExpressionStatement : ExpressionStatement
		{
			readonly Expression orig_expr;
			readonly ExpressionStatement stm;

			public ReducedExpressionStatement (ExpressionStatement stm, Expression orig)
			{
				this.orig_expr = orig;
				this.stm = stm;
				this.loc = orig.Location;
			}

			public override Expression CreateExpressionTree (ResolveContext ec)
			{
				return orig_expr.CreateExpressionTree (ec);
			}

			public override Expression DoResolve (ResolveContext ec)
			{
				eclass = stm.eclass;
				type = stm.Type;
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				stm.Emit (ec);
			}

			public override void EmitStatement (EmitContext ec)
			{
				stm.EmitStatement (ec);
			}

			public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
			{
				stm.MutateHoistedGenericType (storey);
			}
		}

		readonly Expression expr, orig_expr;

		private ReducedExpression (Expression expr, Expression orig_expr)
		{
			this.expr = expr;
			this.orig_expr = orig_expr;
			this.loc = orig_expr.Location;
		}

		public static Constant Create (Constant expr, Expression original_expr)
		{
			return new ReducedConstantExpression (expr, original_expr);
		}

		public static ExpressionStatement Create (ExpressionStatement s, Expression orig)
		{
			return new ReducedExpressionStatement (s, orig);
		}

		public static Expression Create (Expression expr, Expression original_expr)
		{
			Constant c = expr as Constant;
			if (c != null)
				return Create (c, original_expr);

			ExpressionStatement s = expr as ExpressionStatement;
			if (s != null)
				return Create (s, original_expr);

			return new ReducedExpression (expr, original_expr);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return orig_expr.CreateExpressionTree (ec);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			eclass = expr.eclass;
			type = expr.Type;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			expr.Emit (ec);
		}

		public override void EmitBranchable (EmitContext ec, Label target, bool on_true)
		{
			expr.EmitBranchable (ec, target, on_true);
		}

#if NET_4_0
		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			return orig_expr.MakeExpression (ctx);
		}
#endif

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			expr.MutateHoistedGenericType (storey);
		}
	}

	//
	// Unresolved type name expressions
	//
	public abstract class ATypeNameExpression : FullNamedExpression
	{
		string name;
		protected TypeArguments targs;

		protected ATypeNameExpression (string name, Location l)
		{
			this.name = name;
			loc = l;
		}

		protected ATypeNameExpression (string name, TypeArguments targs, Location l)
		{
			this.name = name;
			this.targs = targs;
			loc = l;
		}

		public bool HasTypeArguments {
			get {
				return targs != null;
			}
		}

		public override bool Equals (object obj)
		{
			ATypeNameExpression atne = obj as ATypeNameExpression;
			return atne != null && atne.Name == Name &&
				(targs == null || targs.Equals (atne.targs));
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public override string GetSignatureForError ()
		{
			if (targs != null) {
				return TypeManager.RemoveGenericArity (Name) + "<" +
					targs.GetSignatureForError () + ">";
			}

			return Name;
		}

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public TypeArguments TypeArguments {
			get {
				return targs;
			}
		}
	}
	
	/// <summary>
	///   SimpleName expressions are formed of a single word and only happen at the beginning 
	///   of a dotted-name.
	/// </summary>
	public class SimpleName : ATypeNameExpression {
		bool in_transit;

		public SimpleName (string name, Location l)
			: base (name, l)
		{
		}

		public SimpleName (string name, TypeArguments args, Location l)
			: base (name, args, l)
		{
		}

		public SimpleName (string name, TypeParameter[] type_params, Location l)
			: base (name, l)
		{
			targs = new TypeArguments ();
			foreach (TypeParameter type_param in type_params)
				targs.Add (new TypeParameterExpr (type_param, l));
		}

		public static string RemoveGenericArity (string name)
		{
			int start = 0;
			StringBuilder sb = null;
			do {
				int pos = name.IndexOf ('`', start);
				if (pos < 0) {
					if (start == 0)
						return name;

					sb.Append (name.Substring (start));
					break;
				}

				if (sb == null)
					sb = new StringBuilder ();
				sb.Append (name.Substring (start, pos-start));

				pos++;
				while ((pos < name.Length) && Char.IsNumber (name [pos]))
					pos++;

				start = pos;
			} while (start < name.Length);

			return sb.ToString ();
		}

		public SimpleName GetMethodGroup ()
		{
			return new SimpleName (RemoveGenericArity (Name), targs, loc);
		}

		public static void Error_ObjectRefRequired (ResolveContext ec, Location l, string name)
		{
			if (ec.HasSet (ResolveContext.Options.FieldInitializerScope))
				ec.Report.Error (236, l,
					"A field initializer cannot reference the nonstatic field, method, or property `{0}'",
					name);
			else
				ec.Report.Error (120, l,
					"An object reference is required to access non-static member `{0}'",
					name);
		}

		public bool IdenticalNameAndTypeName (IMemberContext mc, Expression resolved_to, Location loc)
		{
			return resolved_to != null && resolved_to.Type != null && 
				resolved_to.Type.Name == Name &&
				(mc.LookupNamespaceOrType (Name, loc, /* ignore_cs0104 = */ true) != null);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			return SimpleNameResolve (ec, null, false);
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			return SimpleNameResolve (ec, right_side, false);
		}
		

		public Expression DoResolve (ResolveContext ec, bool intermediate)
		{
			return SimpleNameResolve (ec, null, intermediate);
		}

		static bool IsNestedChild (Type t, Type parent)
		{
			while (parent != null) {
				if (TypeManager.IsNestedChildOf (t, TypeManager.DropGenericTypeArguments (parent)))
					return true;

				parent = parent.BaseType;
			}

			return false;
		}

		FullNamedExpression ResolveNested (Type t)
		{
			if (!TypeManager.IsGenericTypeDefinition (t) && !TypeManager.IsGenericType (t))
				return null;

			Type ds = t;
			while (ds != null && !IsNestedChild (t, ds))
				ds = ds.DeclaringType;

			if (ds == null)
				return null;

			Type[] gen_params = TypeManager.GetTypeArguments (t);

			int arg_count = targs != null ? targs.Count : 0;

			for (; (ds != null) && TypeManager.IsGenericType (ds); ds = ds.DeclaringType) {
				Type[] gargs = TypeManager.GetTypeArguments (ds);
				if (arg_count + gargs.Length == gen_params.Length) {
					TypeArguments new_args = new TypeArguments ();
					foreach (Type param in gargs)
						new_args.Add (new TypeExpression (param, loc));

					if (targs != null)
						new_args.Add (targs);

					return new GenericTypeExpr (t, new_args, loc);
				}
			}

			return null;
		}

		public override FullNamedExpression ResolveAsTypeStep (IMemberContext ec, bool silent)
		{
			int errors = ec.Compiler.Report.Errors;
			FullNamedExpression fne = ec.LookupNamespaceOrType (Name, loc, /*ignore_cs0104=*/ false);

			if (fne != null) {
				if (fne.Type == null)
					return fne;

				FullNamedExpression nested = ResolveNested (fne.Type);
				if (nested != null)
					return nested.ResolveAsTypeStep (ec, false);

				if (targs != null) {
					if (TypeManager.IsGenericType (fne.Type)) {
						GenericTypeExpr ct = new GenericTypeExpr (fne.Type, targs, loc);
						return ct.ResolveAsTypeStep (ec, false);
					}

					Namespace.Error_TypeArgumentsCannotBeUsed (fne, loc);
				}

				return fne;
			}

			if (!HasTypeArguments && Name == "dynamic" && RootContext.Version > LanguageVersion.V_3)
				return new DynamicTypeExpr (loc);

			if (silent || errors != ec.Compiler.Report.Errors)
				return null;

			Error_TypeOrNamespaceNotFound (ec);
			return null;
		}

		protected virtual void Error_TypeOrNamespaceNotFound (IMemberContext ec)
		{
			if (ec.CurrentType != null) {
				if (ec.CurrentTypeDefinition != null) {
					MemberCore mc = ec.CurrentTypeDefinition.GetDefinition (Name);
					if (mc != null) {
						Error_UnexpectedKind (ec.Compiler.Report, mc, "type", GetMemberType (mc), loc);
						return;
					}
				}

				string ns = ec.CurrentType.Namespace;
				string fullname = (ns.Length > 0) ? ns + "." + Name : Name;
				foreach (Assembly a in GlobalRootNamespace.Instance.Assemblies) {
					Type type = a.GetType (fullname);
					if (type != null) {
						ec.Compiler.Report.SymbolRelatedToPreviousError (type);
						Expression.ErrorIsInaccesible (loc, TypeManager.CSharpName (type), ec.Compiler.Report);
						return;
					}
				}

				if (ec.CurrentTypeDefinition != null) {
					Type t = ec.CurrentTypeDefinition.LookupAnyGeneric (Name);
					if (t != null) {
						Namespace.Error_InvalidNumberOfTypeArguments (t, loc);
						return;
					}
				}
			}

			if (targs != null) {
				FullNamedExpression retval = ec.LookupNamespaceOrType (SimpleName.RemoveGenericArity (Name), loc, true);
				if (retval != null) {
					Namespace.Error_TypeArgumentsCannotBeUsed (retval, loc);
					return;
				}
			}
						
			NamespaceEntry.Error_NamespaceNotFound (loc, Name, ec.Compiler.Report);
		}

		// TODO: I am still not convinced about this. If someone else will need it
		// implement this as virtual property in MemberCore hierarchy
		public static string GetMemberType (MemberCore mc)
		{
			if (mc is Property)
				return "property";
			if (mc is Indexer)
				return "indexer";
			if (mc is FieldBase)
				return "field";
			if (mc is MethodCore)
				return "method";
			if (mc is EnumMember)
				return "enum";
			if (mc is Event)
				return "event";

			return "type";
		}

		Expression SimpleNameResolve (ResolveContext ec, Expression right_side, bool intermediate)
		{
			if (in_transit)
				return null;

			in_transit = true;
			Expression e = DoSimpleNameResolve (ec, right_side, intermediate);
			in_transit = false;

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
		Expression DoSimpleNameResolve (ResolveContext ec, Expression right_side, bool intermediate)
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
						return var.ResolveLValue (ec, right_side);
					} else {
						ResolveFlags rf = ResolveFlags.VariableOrValue;
						if (intermediate)
							rf |= ResolveFlags.DisableFlowAnalysis;
						return var.Resolve (ec, rf);
					}
				}

				Expression expr = current_block.Toplevel.GetParameterReference (Name, loc);
				if (expr != null) {
					if (right_side != null)
						return expr.ResolveLValue (ec, right_side);

					return expr.Resolve (ec);
				}
			}
			
			//
			// Stage 2: Lookup members 
			//

			Type almost_matched_type = null;
			ArrayList almost_matched = null;
			for (Type lookup_ds = ec.CurrentType; lookup_ds != null; lookup_ds = lookup_ds.DeclaringType) {
				e = MemberLookup (ec.Compiler, ec.CurrentType, lookup_ds, Name, loc);
				if (e != null) {
					PropertyExpr pe = e as PropertyExpr;
					if (pe != null) {
						AParametersCollection param = TypeManager.GetParameterData (pe.PropertyInfo);

						// since TypeManager.MemberLookup doesn't know if we're doing a lvalue access or not,
						// it doesn't know which accessor to check permissions against
						if (param.IsEmpty && pe.IsAccessibleFrom (ec.CurrentType, right_side != null))
							break;
					} else if (e is EventExpr) {
						if (((EventExpr) e).IsAccessibleFrom (ec.CurrentType))
							break;
					} else if (targs != null && e is TypeExpression) {
						e = new GenericTypeExpr (e.Type, targs, loc).ResolveAsTypeStep (ec, false);
						break;
					} else {
						break;
					}
					e = null;
				}

				if (almost_matched == null && almost_matched_members.Count > 0) {
					almost_matched_type = lookup_ds;
					almost_matched = (ArrayList) almost_matched_members.Clone ();
				}
			}

			if (e == null) {
				if (almost_matched == null && almost_matched_members.Count > 0) {
					almost_matched_type = ec.CurrentType;
					almost_matched = (ArrayList) almost_matched_members.Clone ();
				}
				e = ResolveAsTypeStep (ec, true);
			}

			if (e == null) {
				if (current_block != null) {
					IKnownVariable ikv = current_block.Explicit.GetKnownVariable (Name);
					if (ikv != null) {
						LocalInfo li = ikv as LocalInfo;
						// Supress CS0219 warning
						if (li != null)
							li.Used = true;

						Error_VariableIsUsedBeforeItIsDeclared (ec.Report, Name);
						return null;
					}
				}

				if (RootContext.EvalMode){
					FieldInfo fi = Evaluator.LookupField (Name);
					if (fi != null)
						return new FieldExpr (fi, loc).Resolve (ec);
				}

				if (almost_matched != null)
					almost_matched_members = almost_matched;
				if (almost_matched_type == null)
					almost_matched_type = ec.CurrentType;

				string type_name = ec.MemberContext.CurrentType == null ? null : ec.MemberContext.CurrentType.Name;
				return Error_MemberLookupFailed (ec, ec.CurrentType, null, almost_matched_type, Name,
					type_name, AllMemberTypes, AllBindingFlags);
			}

			if (e is MemberExpr) {
				MemberExpr me = (MemberExpr) e;

				Expression left;
				if (me.IsInstance) {
					if (ec.IsStatic || ec.HasAny (ResolveContext.Options.FieldInitializerScope | ResolveContext.Options.BaseInitializer | ResolveContext.Options.ConstantScope)) {
						//
						// Note that an MemberExpr can be both IsInstance and IsStatic.
						// An unresolved MethodGroupExpr can contain both kinds of methods
						// and each predicate is true if the MethodGroupExpr contains
						// at least one of that kind of method.
						//

						if (!me.IsStatic &&
						    (!intermediate || !IdenticalNameAndTypeName (ec, me, loc))) {
							Error_ObjectRefRequired (ec, loc, me.GetSignatureForError ());
							return null;
						}

						//
						// Pass the buck to MemberAccess and Invocation.
						//
						left = EmptyExpression.Null;
					} else {
						left = ec.GetThis (loc);
					}
				} else {
					left = new TypeExpression (ec.CurrentType, loc);
				}

				me = me.ResolveMemberAccess (ec, left, loc, null);
				if (me == null)
					return null;

				if (targs != null) {
					if (!targs.Resolve (ec))
						return null;

					me.SetTypeArguments (ec, targs);
				}

				if (!me.IsStatic && (me.InstanceExpression != null && me.InstanceExpression != EmptyExpression.Null) &&
				    TypeManager.IsNestedFamilyAccessible (me.InstanceExpression.Type, me.DeclaringType) &&
				    me.InstanceExpression.Type != me.DeclaringType &&
				    !TypeManager.IsFamilyAccessible (me.InstanceExpression.Type, me.DeclaringType) &&
				    (!intermediate || !IdenticalNameAndTypeName (ec, e, loc))) {
					ec.Report.Error (38, loc, "Cannot access a nonstatic member of outer type `{0}' via nested type `{1}'",
						TypeManager.CSharpName (me.DeclaringType), TypeManager.CSharpName (me.InstanceExpression.Type));
					return null;
				}

				return (right_side != null)
					? me.DoResolveLValue (ec, right_side)
					: me.DoResolve (ec);
			}

			return e;
		}
	}

	/// <summary>
	///   Represents a namespace or a type.  The name of the class was inspired by
	///   section 10.8.1 (Fully Qualified Names).
	/// </summary>
	public abstract class FullNamedExpression : Expression
	{
		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			// Do nothing, most unresolved type expressions cannot be
			// resolved to different type
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			throw new NotSupportedException ();
		}

		public override FullNamedExpression ResolveAsTypeStep (IMemberContext ec, bool silent)
		{
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			throw new InternalErrorException ("FullNamedExpression `{0}' found in resolved tree",
				GetSignatureForError ());
		}
	}
	
	/// <summary>
	///   Expression that evaluates to a type
	/// </summary>
	public abstract class TypeExpr : FullNamedExpression {
		public override FullNamedExpression ResolveAsTypeStep (IMemberContext ec, bool silent)
		{
			TypeExpr t = DoResolveAsTypeStep (ec);
			if (t == null)
				return null;

			eclass = ExprClass.Type;
			return t;
		}

		override public Expression DoResolve (ResolveContext ec)
		{
			return ResolveAsTypeTerminal (ec, false);
		}

		public virtual bool CheckAccessLevel (IMemberContext mc)
		{
			return mc.CurrentTypeDefinition.CheckAccessLevel (Type);
		}

		public virtual bool IsClass {
			get { return Type.IsClass; }
		}

		public virtual bool IsValueType {
			get { return TypeManager.IsStruct (Type); }
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

		protected abstract TypeExpr DoResolveAsTypeStep (IMemberContext ec);

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

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			type = storey.MutateType (type);
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

		protected override TypeExpr DoResolveAsTypeStep (IMemberContext ec)
		{
			return this;
		}

		public override TypeExpr ResolveAsTypeTerminal (IMemberContext ec, bool silent)
		{
			return this;
		}
	}

	//
	// Used to create types from a fully qualified name.  These are just used
	// by the parser to setup the core types.
	//
	public sealed class TypeLookupExpression : TypeExpr {
		readonly string ns_name;
		readonly string name;
		
		public TypeLookupExpression (string ns, string name)
		{
			this.name = name;
			this.ns_name = ns;
			eclass = ExprClass.Type;
		}

		public override TypeExpr ResolveAsTypeTerminal (IMemberContext ec, bool silent)
		{
			//
			// It's null only during mscorlib bootstrap when DefineType
			// nees to resolve base type of same type
			//
			// For instance struct Char : IComparable<char>
			//
			// TODO: it could be removed when Resolve starts to use 
			// DeclSpace instead of Type
			//
			if (type == null) {
				Namespace ns = GlobalRootNamespace.Instance.GetNamespace (ns_name, false);
				FullNamedExpression fne = ns.Lookup (ec.Compiler, name, loc);
				if (fne != null)
					type = fne.Type;
			}

			return this;
		}

		protected override TypeExpr DoResolveAsTypeStep (IMemberContext ec)
		{
			return this;
		}

		public override string GetSignatureForError ()
		{
			if (type == null)
				return TypeManager.CSharpName (ns_name + "." + name, null);

			return base.GetSignatureForError ();
		}
	}

	/// <summary>
	///   This class denotes an expression which evaluates to a member
	///   of a struct or a class.
	/// </summary>
	public abstract class MemberExpr : Expression
	{
		protected bool is_base;

		/// <summary>
		///   The name of this member.
		/// </summary>
		public abstract string Name {
			get;
		}

		//
		// When base.member is used
		//
		public bool IsBase {
			get { return is_base; }
			set { is_base = value; }
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

		public static void error176 (ResolveContext ec, Location loc, string name)
		{
			ec.Report.Error (176, loc, "Static member `{0}' cannot be accessed " +
				      "with an instance reference, qualify it with a type name instead", name);
		}

		public static void Error_BaseAccessInExpressionTree (ResolveContext ec, Location loc)
		{
			ec.Report.Error (831, loc, "An expression tree may not contain a base access");
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			if (InstanceExpression != null)
				InstanceExpression.MutateHoistedGenericType (storey);
		}

		// TODO: possible optimalization
		// Cache resolved constant result in FieldBuilder <-> expression map
		public virtual MemberExpr ResolveMemberAccess (ResolveContext ec, Expression left, Location loc,
							       SimpleName original)
		{
			//
			// Precondition:
			//   original == null || original.Resolve (...) ==> left
			//

			if (left is TypeExpr) {
				left = left.ResolveAsBaseTerminal (ec, false);
				if (left == null)
					return null;

				// TODO: Same problem as in class.cs, TypeTerminal does not
				// always do all necessary checks
				ObsoleteAttribute oa = AttributeTester.GetObsoleteAttribute (left.Type);
				if (oa != null && !ec.IsObsolete) {
					AttributeTester.Report_ObsoleteMessage (oa, left.GetSignatureForError (), loc, ec.Report);
				}

				GenericTypeExpr ct = left as GenericTypeExpr;
				if (ct != null && !ct.CheckConstraints (ec))
					return null;
				//

				if (!IsStatic) {
					SimpleName.Error_ObjectRefRequired (ec, loc, GetSignatureForError ());
					return null;
				}

				return this;
			}
				
			if (!IsInstance) {
				if (original != null && original.IdenticalNameAndTypeName (ec, left, loc))
					return this;

				return ResolveExtensionMemberAccess (ec, left);
			}

			InstanceExpression = left;
			return this;
		}

		protected virtual MemberExpr ResolveExtensionMemberAccess (ResolveContext ec, Expression left)
		{
			error176 (ec, loc, GetSignatureForError ());
			return this;
		}

		protected void EmitInstance (EmitContext ec, bool prepare_for_load)
		{
			if (IsStatic)
				return;

			if (InstanceExpression == EmptyExpression.Null) {
				// FIXME: This should not be here at all
				SimpleName.Error_ObjectRefRequired (new ResolveContext (ec.MemberContext), loc, GetSignatureForError ());
				return;
			}

			Type instance_type = InstanceExpression.Type;
			if (TypeManager.IsValueType (instance_type)) {
				if (InstanceExpression is IMemoryLocation) {
					((IMemoryLocation) InstanceExpression).AddressOf (ec, AddressOp.LoadStore);
				} else {
					LocalTemporary t = new LocalTemporary (instance_type);
					InstanceExpression.Emit (ec);
					t.Store (ec);
					t.AddressOf (ec, AddressOp.Store);
				}
			} else {
				InstanceExpression.Emit (ec);
				#if GMCS_SOURCE
				// Only to make verifier happy
				if (instance_type.IsGenericParameter && !(InstanceExpression is This) && TypeManager.IsReferenceType (instance_type))
					ec.ig.Emit (OpCodes.Box, instance_type);
				#endif
			}

			if (prepare_for_load)
				ec.ig.Emit (OpCodes.Dup);
		}

		public virtual void SetTypeArguments (ResolveContext ec, TypeArguments ta)
		{
			// TODO: need to get correct member type
			ec.Report.Error (307, loc, "The property `{0}' cannot be used with type arguments",
				GetSignatureForError ());
		}
	}

	/// 
	/// Represents group of extension methods
	/// 
	public class ExtensionMethodGroupExpr : MethodGroupExpr
	{
		readonly NamespaceEntry namespace_entry;
		public Expression ExtensionExpression;
		Argument extension_argument;

		public ExtensionMethodGroupExpr (ArrayList list, NamespaceEntry n, Type extensionType, Location l)
			: base (list, extensionType, l)
		{
			this.namespace_entry = n;
		}

		public override bool IsStatic {
			get { return true; }
		}

		public bool IsTopLevel {
			get { return namespace_entry == null; }
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			extension_argument.Expr.MutateHoistedGenericType (storey);
			base.MutateHoistedGenericType (storey);
		}

		public override MethodGroupExpr OverloadResolve (ResolveContext ec, ref Arguments arguments, bool may_fail, Location loc)
		{
			if (arguments == null)
				arguments = new Arguments (1);

			arguments.Insert (0, new Argument (ExtensionExpression));
			MethodGroupExpr mg = ResolveOverloadExtensions (ec, ref arguments, namespace_entry, loc);

			// Store resolved argument and restore original arguments
			if (mg != null)
				((ExtensionMethodGroupExpr)mg).extension_argument = arguments [0];
			else
				arguments.RemoveAt (0);	// Clean-up modified arguments for error reporting

			return mg;
		}

		MethodGroupExpr ResolveOverloadExtensions (ResolveContext ec, ref Arguments arguments, NamespaceEntry ns, Location loc)
		{
			// Use normal resolve rules
			MethodGroupExpr mg = base.OverloadResolve (ec, ref arguments, ns != null, loc);
			if (mg != null)
				return mg;

			if (ns == null)
				return null;

			// Search continues
			ExtensionMethodGroupExpr e = ns.LookupExtensionMethod (type, Name, loc);
			if (e == null)
				return base.OverloadResolve (ec, ref arguments, false, loc);

			e.ExtensionExpression = ExtensionExpression;
			e.SetTypeArguments (ec, type_arguments);			
			return e.ResolveOverloadExtensions (ec, ref arguments, e.namespace_entry, loc);
		}		
	}

	/// <summary>
	///   MethodGroupExpr represents a group of method candidates which
	///   can be resolved to the best method overload
	/// </summary>
	public class MethodGroupExpr : MemberExpr
	{
		public interface IErrorHandler
		{
			bool AmbiguousCall (ResolveContext ec, MethodBase ambiguous);
			bool NoExactMatch (ResolveContext ec, MethodBase method);
		}

		public IErrorHandler CustomErrorHandler;		
		public MethodBase [] Methods;
		MethodBase best_candidate;
		// TODO: make private
		public TypeArguments type_arguments;
 		bool identical_type_name;
		bool has_inaccessible_candidates_only;
		Type delegate_type;
		Type queried_type;
		
		public MethodGroupExpr (MemberInfo [] mi, Type type, Location l)
			: this (type, l)
		{
			Methods = new MethodBase [mi.Length];
			mi.CopyTo (Methods, 0);
		}

		public MethodGroupExpr (MemberInfo[] mi, Type type, Location l, bool inacessibleCandidatesOnly)
			: this (mi, type, l)
		{
			has_inaccessible_candidates_only = inacessibleCandidatesOnly;
		}

		public MethodGroupExpr (ArrayList list, Type type, Location l)
			: this (type, l)
		{
			try {
				Methods = (MethodBase[])list.ToArray (typeof (MethodBase));
			} catch {
				foreach (MemberInfo m in list){
					if (!(m is MethodBase)){
						Console.WriteLine ("Name " + m.Name);
						Console.WriteLine ("Found a: " + m.GetType ().FullName);
					}
				}
				throw;
			}


		}

		protected MethodGroupExpr (Type type, Location loc)
		{
			this.loc = loc;
			eclass = ExprClass.MethodGroup;
			this.type = InternalType.MethodGroup;
			queried_type = type;
		}

		public override Type DeclaringType {
			get {
				return queried_type;
			}
		}

		public Type DelegateType {
			set {
				delegate_type = value;
			}
		}

		public bool IdenticalTypeName {
			get {
				return identical_type_name;
			}
		}

		public override string GetSignatureForError ()
		{
			if (best_candidate != null)
				return TypeManager.CSharpSignature (best_candidate);
			
			return TypeManager.CSharpSignature (Methods [0]);
		}

		public override string Name {
			get {
				return Methods [0].Name;
			}
		}

		public override bool IsInstance {
			get {
				if (best_candidate != null)
					return !best_candidate.IsStatic;

				foreach (MethodBase mb in Methods)
					if (!mb.IsStatic)
						return true;

				return false;
			}
		}

		public override bool IsStatic {
			get {
				if (best_candidate != null)
					return best_candidate.IsStatic;

				foreach (MethodBase mb in Methods)
					if (mb.IsStatic)
						return true;

				return false;
			}
		}
		
		public static explicit operator ConstructorInfo (MethodGroupExpr mg)
		{
			return (ConstructorInfo)mg.best_candidate;
		}

		public static explicit operator MethodInfo (MethodGroupExpr mg)
		{
			return (MethodInfo)mg.best_candidate;
		}

		//
		//  7.4.3.3  Better conversion from expression
		//  Returns :   1    if a->p is better,
		//              2    if a->q is better,
		//              0 if neither is better
		//
		static int BetterExpressionConversion (ResolveContext ec, Argument a, Type p, Type q)
		{
			Type argument_type = TypeManager.TypeToCoreType (a.Type);
			if (argument_type == InternalType.AnonymousMethod && RootContext.Version > LanguageVersion.ISO_2) {
				//
				// Uwrap delegate from Expression<T>
				//
				if (TypeManager.DropGenericTypeArguments (p) == TypeManager.expression_type) {
					p = TypeManager.GetTypeArguments (p) [0];
				}
				if (TypeManager.DropGenericTypeArguments (q) == TypeManager.expression_type) {
					q = TypeManager.GetTypeArguments (q) [0];
				}
				
				p = Delegate.GetInvokeMethod (ec.Compiler, null, p).ReturnType;
				q = Delegate.GetInvokeMethod (ec.Compiler, null, q).ReturnType;
				if (p == TypeManager.void_type && q != TypeManager.void_type)
					return 2;
				if (q == TypeManager.void_type && p != TypeManager.void_type)
					return 1;
			} else {
				if (argument_type == p)
					return 1;

				if (argument_type == q)
					return 2;
			}

			return BetterTypeConversion (ec, p, q);
		}

		//
		// 7.4.3.4  Better conversion from type
		//
		public static int BetterTypeConversion (ResolveContext ec, Type p, Type q)
		{
			if (p == null || q == null)
				throw new InternalErrorException ("BetterTypeConversion got a null conversion");

			if (p == TypeManager.int32_type) {
				if (q == TypeManager.uint32_type || q == TypeManager.uint64_type)
					return 1;
			} else if (p == TypeManager.int64_type) {
				if (q == TypeManager.uint64_type)
					return 1;
			} else if (p == TypeManager.sbyte_type) {
				if (q == TypeManager.byte_type || q == TypeManager.ushort_type ||
					q == TypeManager.uint32_type || q == TypeManager.uint64_type)
					return 1;
			} else if (p == TypeManager.short_type) {
				if (q == TypeManager.ushort_type || q == TypeManager.uint32_type ||
					q == TypeManager.uint64_type)
					return 1;
			}

			if (q == TypeManager.int32_type) {
				if (p == TypeManager.uint32_type || p == TypeManager.uint64_type)
					return 2;
			} if (q == TypeManager.int64_type) {
				if (p == TypeManager.uint64_type)
					return 2;
			} else if (q == TypeManager.sbyte_type) {
				if (p == TypeManager.byte_type || p == TypeManager.ushort_type ||
					p == TypeManager.uint32_type || p == TypeManager.uint64_type)
					return 2;
			} if (q == TypeManager.short_type) {
				if (p == TypeManager.ushort_type || p == TypeManager.uint32_type ||
					p == TypeManager.uint64_type)
					return 2;
			}

			// TODO: this is expensive
			Expression p_tmp = new EmptyExpression (p);
			Expression q_tmp = new EmptyExpression (q);

			bool p_to_q = Convert.ImplicitConversionExists (ec, p_tmp, q);
			bool q_to_p = Convert.ImplicitConversionExists (ec, q_tmp, p);

			if (p_to_q && !q_to_p)
				return 1;

			if (q_to_p && !p_to_q)
				return 2;

			return 0;
		}

		/// <summary>
		///   Determines "Better function" between candidate
		///   and the current best match
		/// </summary>
		/// <remarks>
		///    Returns a boolean indicating :
		///     false if candidate ain't better
		///     true  if candidate is better than the current best match
		/// </remarks>
		static bool BetterFunction (ResolveContext ec, Arguments args, int argument_count,
			MethodBase candidate, bool candidate_params,
			MethodBase best, bool best_params)
		{
			AParametersCollection candidate_pd = TypeManager.GetParameterData (candidate);
			AParametersCollection best_pd = TypeManager.GetParameterData (best);
		
			bool better_at_least_one = false;
			bool same = true;
			for (int j = 0, c_idx = 0, b_idx = 0; j < argument_count; ++j, ++c_idx, ++b_idx) 
			{
				Argument a = args [j];

				// Provided default argument value is never better
				if (a.IsDefaultArgument && candidate_params == best_params)
					return false;

				Type ct = candidate_pd.Types [c_idx];
				Type bt = best_pd.Types [b_idx];

				if (candidate_params && candidate_pd.FixedParameters [c_idx].ModFlags == Parameter.Modifier.PARAMS) 
				{
					ct = TypeManager.GetElementType (ct);
					--c_idx;
				}

				if (best_params && best_pd.FixedParameters [b_idx].ModFlags == Parameter.Modifier.PARAMS) 
				{
					bt = TypeManager.GetElementType (bt);
					--b_idx;
				}

				if (ct.Equals (bt))
					continue;

				same = false;
				int result = BetterExpressionConversion (ec, a, ct, bt);

				// for each argument, the conversion to 'ct' should be no worse than 
				// the conversion to 'bt'.
				if (result == 2)
					return false;

				// for at least one argument, the conversion to 'ct' should be better than 
				// the conversion to 'bt'.
				if (result != 0)
					better_at_least_one = true;
			}

			if (better_at_least_one)
				return true;

			//
			// This handles the case
			//
			//   Add (float f1, float f2, float f3);
			//   Add (params decimal [] foo);
			//
			// The call Add (3, 4, 5) should be ambiguous.  Without this check, the
			// first candidate would've chosen as better.
			//
			if (!same)
				return false;

			//
			// The two methods have equal parameter types.  Now apply tie-breaking rules
			//
			if (TypeManager.IsGenericMethod (best)) {
				if (!TypeManager.IsGenericMethod (candidate))
					return true;
			} else if (TypeManager.IsGenericMethod (candidate)) {
				return false;
			}

			//
			// This handles the following cases:
			//
			//   Trim () is better than Trim (params char[] chars)
			//   Concat (string s1, string s2, string s3) is better than
			//     Concat (string s1, params string [] srest)
			//   Foo (int, params int [] rest) is better than Foo (params int [] rest)
			//
			if (!candidate_params && best_params)
				return true;
			if (candidate_params && !best_params)
				return false;

			int candidate_param_count = candidate_pd.Count;
			int best_param_count = best_pd.Count;

			if (candidate_param_count != best_param_count)
				// can only happen if (candidate_params && best_params)
				return candidate_param_count > best_param_count && best_pd.HasParams;

			//
			// now, both methods have the same number of parameters, and the parameters have the same types
			// Pick the "more specific" signature
			//

			MethodBase orig_candidate = TypeManager.DropGenericMethodArguments (candidate);
			MethodBase orig_best = TypeManager.DropGenericMethodArguments (best);

			AParametersCollection orig_candidate_pd = TypeManager.GetParameterData (orig_candidate);
			AParametersCollection orig_best_pd = TypeManager.GetParameterData (orig_best);

			bool specific_at_least_once = false;
			for (int j = 0; j < candidate_param_count; ++j) 
			{
				Type ct = orig_candidate_pd.Types [j];
				Type bt = orig_best_pd.Types [j];
				if (ct.Equals (bt))
					continue;
				Type specific = MoreSpecific (ct, bt);
				if (specific == bt)
					return false;
				if (specific == ct)
					specific_at_least_once = true;
			}

			if (specific_at_least_once)
				return true;

			// FIXME: handle lifted operators
			// ...

			return false;
		}

		protected override MemberExpr ResolveExtensionMemberAccess (ResolveContext ec, Expression left)
		{
			if (!IsStatic)
				return base.ResolveExtensionMemberAccess (ec, left);

			//
			// When left side is an expression and at least one candidate method is 
			// static, it can be extension method
			//
			InstanceExpression = left;
			return this;
		}

		public override MemberExpr ResolveMemberAccess (ResolveContext ec, Expression left, Location loc,
								SimpleName original)
		{
			if (!(left is TypeExpr) &&
			    original != null && original.IdenticalNameAndTypeName (ec, left, loc))
				identical_type_name = true;

			return base.ResolveMemberAccess (ec, left, loc, original);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			if (best_candidate == null) {
				ec.Report.Error (1953, loc, "An expression tree cannot contain an expression with method group");
				return null;
			}

			IMethodData md = TypeManager.GetMethod (best_candidate);
			if (md != null && md.IsExcluded ())
				ec.Report.Error (765, loc,
					"Partial methods with only a defining declaration or removed conditional methods cannot be used in an expression tree");
			
			return new TypeOfMethod (best_candidate, loc);
		}
		
		override public Expression DoResolve (ResolveContext ec)
		{
			if (InstanceExpression != null) {
				InstanceExpression = InstanceExpression.DoResolve (ec);
				if (InstanceExpression == null)
					return null;
			}

			return this;
		}

		public void ReportUsageError (ResolveContext ec)
		{
			ec.Report.Error (654, loc, "Method `" + DeclaringType + "." +
				      Name + "()' is referenced without parentheses");
		}

		override public void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
			// ReportUsageError ();
		}
		
		public void EmitCall (EmitContext ec, Arguments arguments)
		{
			Invocation.EmitCall (ec, IsBase, InstanceExpression, best_candidate, arguments, loc);			
		}

		void Error_AmbiguousCall (ResolveContext ec, MethodBase ambiguous)
		{
			if (CustomErrorHandler != null && CustomErrorHandler.AmbiguousCall (ec, ambiguous))
				return;

			ec.Report.SymbolRelatedToPreviousError (best_candidate);
			ec.Report.Error (121, loc, "The call is ambiguous between the following methods or properties: `{0}' and `{1}'",
				TypeManager.CSharpSignature (ambiguous), TypeManager.CSharpSignature (best_candidate));
		}

		protected virtual void Error_InvalidArguments (ResolveContext ec, Location loc, int idx, MethodBase method,
													Argument a, AParametersCollection expected_par, Type paramType)
		{
			ExtensionMethodGroupExpr emg = this as ExtensionMethodGroupExpr;

			if (a is CollectionElementInitializer.ElementInitializerArgument) {
				ec.Report.SymbolRelatedToPreviousError (method);
				if ((expected_par.FixedParameters [idx].ModFlags & Parameter.Modifier.ISBYREF) != 0) {
					ec.Report.Error (1954, loc, "The best overloaded collection initalizer method `{0}' cannot have 'ref', or `out' modifier",
						TypeManager.CSharpSignature (method));
					return;
				}
				ec.Report.Error (1950, loc, "The best overloaded collection initalizer method `{0}' has some invalid arguments",
					  TypeManager.CSharpSignature (method));
			} else if (TypeManager.IsDelegateType (method.DeclaringType)) {
				ec.Report.Error (1594, loc, "Delegate `{0}' has some invalid arguments",
					TypeManager.CSharpName (method.DeclaringType));
			} else {
				ec.Report.SymbolRelatedToPreviousError (method);
				if (emg != null) {
					ec.Report.Error (1928, loc,
						"Type `{0}' does not contain a member `{1}' and the best extension method overload `{2}' has some invalid arguments",
						emg.ExtensionExpression.GetSignatureForError (),
						emg.Name, TypeManager.CSharpSignature (method));
				} else {
					ec.Report.Error (1502, loc, "The best overloaded method match for `{0}' has some invalid arguments",
						TypeManager.CSharpSignature (method));
				}
			}

			Parameter.Modifier mod = idx >= expected_par.Count ? 0 : expected_par.FixedParameters [idx].ModFlags;

			string index = (idx + 1).ToString ();
			if (((mod & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) ^
				(a.Modifier & (Parameter.Modifier.REF | Parameter.Modifier.OUT))) != 0) {
				if ((mod & Parameter.Modifier.ISBYREF) == 0)
					ec.Report.Error (1615, loc, "Argument `#{0}' does not require `{1}' modifier. Consider removing `{1}' modifier",
						index, Parameter.GetModifierSignature (a.Modifier));
				else
					ec.Report.Error (1620, loc, "Argument `#{0}' is missing `{1}' modifier",
						index, Parameter.GetModifierSignature (mod));
			} else {
				string p1 = a.GetSignatureForError ();
				string p2 = TypeManager.CSharpName (paramType);

				if (p1 == p2) {
					ec.Report.ExtraInformation (loc, "(equally named types possibly from different assemblies in previous ");
					ec.Report.SymbolRelatedToPreviousError (a.Expr.Type);
					ec.Report.SymbolRelatedToPreviousError (paramType);
				}

				if (idx == 0 && emg != null) {
					ec.Report.Error (1929, loc,
						"Extension method instance type `{0}' cannot be converted to `{1}'", p1, p2);
				} else {
					ec.Report.Error (1503, loc,
						"Argument `#{0}' cannot convert `{1}' expression to type `{2}'", index, p1, p2);
				}
			}
		}

		public override void Error_ValueCannotBeConverted (ResolveContext ec, Location loc, Type target, bool expl)
		{
			ec.Report.Error (428, loc, "Cannot convert method group `{0}' to non-delegate type `{1}'. Consider using parentheses to invoke the method",
				Name, TypeManager.CSharpName (target));
		}

		void Error_ArgumentCountWrong (ResolveContext ec, int arg_count)
		{
			ec.Report.Error (1501, loc, "No overload for method `{0}' takes `{1}' arguments",
				      Name, arg_count.ToString ());
		}
		
		protected virtual int GetApplicableParametersCount (MethodBase method, AParametersCollection parameters)
		{
			return parameters.Count;
		}		

		public static bool IsAncestralType (Type first_type, Type second_type)
		{
			return first_type != second_type &&
				(TypeManager.IsSubclassOf (second_type, first_type) ||
				TypeManager.ImplementsInterface (second_type, first_type));
		}

		///
		/// Determines if the candidate method is applicable (section 14.4.2.1)
		/// to the given set of arguments
		/// A return value rates candidate method compatibility,
		/// 0 = the best, int.MaxValue = the worst
		///
		public int IsApplicable (ResolveContext ec,
						ref Arguments arguments, int arg_count, ref MethodBase method, ref bool params_expanded_form)
		{
			MethodBase candidate = method;

			AParametersCollection pd = TypeManager.GetParameterData (candidate);
			int param_count = GetApplicableParametersCount (candidate, pd);
			int optional_count = 0;

			if (arg_count != param_count) {
				for (int i = 0; i < pd.Count; ++i) {
					if (pd.FixedParameters [i].HasDefaultValue) {
						optional_count = pd.Count - i;
						break;
					}
				}

				int args_gap = Math.Abs (arg_count - param_count);
				if (optional_count != 0) {
					if (args_gap > optional_count)
						return int.MaxValue - 10000 + args_gap - optional_count;

					// Readjust expected number when params used
					if (pd.HasParams) {
						optional_count--;
						if (arg_count < param_count)
							param_count--;
					} else if (arg_count > param_count) {
						return int.MaxValue - 10000 + args_gap;
					}
				} else if (arg_count != param_count) {
					if (!pd.HasParams)
						return int.MaxValue - 10000 + args_gap;
					if (arg_count < param_count - 1)
						return int.MaxValue - 10000 + args_gap;
				}

				// Initialize expanded form of a method with 1 params parameter
				params_expanded_form = param_count == 1 && pd.HasParams;

				// Resize to fit optional arguments
				if (optional_count != 0) {
					Arguments resized;
					if (arguments == null) {
						resized = new Arguments (optional_count);
					} else {
						resized = new Arguments (param_count);
						resized.AddRange (arguments);
					}

					for (int i = arg_count; i < param_count; ++i)
						resized.Add (null);
					arguments = resized;
				}
			}

			if (arg_count > 0) {
				//
				// Shuffle named arguments to the right positions if there are any
				//
				if (arguments [arg_count - 1] is NamedArgument) {
					arg_count = arguments.Count;

					for (int i = 0; i < arg_count; ++i) {
						bool arg_moved = false;
						while (true) {
							NamedArgument na = arguments[i] as NamedArgument;
							if (na == null)
								break;

							int index = pd.GetParameterIndexByName (na.Name.Value);

							// Named parameter not found or already reordered
							if (index <= i)
								break;

							// When using parameters which should not be available to the user
							if (index >= param_count)
								break;

							if (!arg_moved) {
								arguments.MarkReorderedArgument (na);
								arg_moved = true;
							}

							Argument temp = arguments[index];
							arguments[index] = arguments[i];
							arguments[i] = temp;

							if (temp == null)
								break;
						}
					}
				} else {
					arg_count = arguments.Count;
				}
			} else if (arguments != null) {
				arg_count = arguments.Count;
			}

#if GMCS_SOURCE
			//
			// 1. Handle generic method using type arguments when specified or type inference
			//
			if (TypeManager.IsGenericMethod (candidate)) {
				if (type_arguments != null) {
					Type [] g_args = candidate.GetGenericArguments ();
					if (g_args.Length != type_arguments.Count)
						return int.MaxValue - 20000 + Math.Abs (type_arguments.Count - g_args.Length);

					// TODO: Don't create new method, create Parameters only
					method = ((MethodInfo) candidate).MakeGenericMethod (type_arguments.Arguments);
					candidate = method;
					pd = TypeManager.GetParameterData (candidate);
				} else {
					int score = TypeManager.InferTypeArguments (ec, arguments, ref candidate);
					if (score != 0)
						return score - 20000;

					if (TypeManager.IsGenericMethodDefinition (candidate))
						throw new InternalErrorException ("A generic method `{0}' definition took part in overload resolution",
							TypeManager.CSharpSignature (candidate));

					pd = TypeManager.GetParameterData (candidate);
				}
			} else {
				if (type_arguments != null)
					return int.MaxValue - 15000;
			}
#endif

			//
			// 2. Each argument has to be implicitly convertible to method parameter
			//
			method = candidate;
			Parameter.Modifier p_mod = 0;
			Type pt = null;
			for (int i = 0; i < arg_count; i++) {
				Argument a = arguments [i];
				if (a == null) {
					if (!pd.FixedParameters [i].HasDefaultValue)
						throw new InternalErrorException ();

					Expression e = pd.FixedParameters [i].DefaultValue as Constant;
					if (e == null)
						e = new DefaultValueExpression (new TypeExpression (pd.Types [i], loc), loc).Resolve (ec);

					arguments [i] = new Argument (e, Argument.AType.Default);
					continue;
				}

				if (p_mod != Parameter.Modifier.PARAMS) {
					p_mod = pd.FixedParameters [i].ModFlags & ~(Parameter.Modifier.OUTMASK | Parameter.Modifier.REFMASK);
					pt = pd.Types [i];
				} else {
					params_expanded_form = true;
				}

				Parameter.Modifier a_mod = a.Modifier & ~(Parameter.Modifier.OUTMASK | Parameter.Modifier.REFMASK);
				int score = 1;
				if (!params_expanded_form)
					score = IsArgumentCompatible (ec, a_mod, a, p_mod & ~Parameter.Modifier.PARAMS, pt);

				if (score != 0 && (p_mod & Parameter.Modifier.PARAMS) != 0 && delegate_type == null) {
					// It can be applicable in expanded form
					score = IsArgumentCompatible (ec, a_mod, a, 0, TypeManager.GetElementType (pt));
					if (score == 0)
						params_expanded_form = true;
				}

				if (score != 0) {
					if (params_expanded_form)
						++score;
					return (arg_count - i) * 2 + score;
				}
			}
			
			if (arg_count != param_count)
				params_expanded_form = true;	
			
			return 0;
		}

		int IsArgumentCompatible (ResolveContext ec, Parameter.Modifier arg_mod, Argument argument, Parameter.Modifier param_mod, Type parameter)
		{
			//
			// Types have to be identical when ref or out modifer is used 
			//
			if (arg_mod != 0 || param_mod != 0) {
				if (TypeManager.HasElementType (parameter))
					parameter = TypeManager.GetElementType (parameter);

				Type a_type = argument.Type;
				if (TypeManager.HasElementType (a_type))
					a_type = TypeManager.GetElementType (a_type);

				if (a_type != parameter)
					return 2;
			} else {
				if (!Convert.ImplicitConversionExists (ec, argument.Expr, parameter))
					return 2;
			}

			if (arg_mod != param_mod)
				return 1;

			return 0;
		}

		public static bool IsOverride (MethodBase cand_method, MethodBase base_method)
		{
			if (!IsAncestralType (base_method.DeclaringType, cand_method.DeclaringType))
				return false;

			AParametersCollection cand_pd = TypeManager.GetParameterData (cand_method);
			AParametersCollection base_pd = TypeManager.GetParameterData (base_method);
		
			if (cand_pd.Count != base_pd.Count)
				return false;

			for (int j = 0; j < cand_pd.Count; ++j) 
			{
				Parameter.Modifier cm = cand_pd.FixedParameters [j].ModFlags;
				Parameter.Modifier bm = base_pd.FixedParameters [j].ModFlags;
				Type ct = cand_pd.Types [j];
				Type bt = base_pd.Types [j];

				if (cm != bm || ct != bt)
					return false;
			}

			return true;
		}

		public static MethodGroupExpr MakeUnionSet (MethodGroupExpr mg1, MethodGroupExpr mg2, Location loc)
		{
			if (mg1 == null) {
				if (mg2 == null)
					return null;
				return mg2;
			}

			if (mg2 == null)
				return mg1;
			
			ArrayList all = new ArrayList (mg1.Methods);
			foreach (MethodBase m in mg2.Methods){
				if (!TypeManager.ArrayContainsMethod (mg1.Methods, m, false))
					all.Add (m);
			}

			return new MethodGroupExpr (all, null, loc);
		}		

		static Type MoreSpecific (Type p, Type q)
		{
			if (TypeManager.IsGenericParameter (p) && !TypeManager.IsGenericParameter (q))
				return q;
			if (!TypeManager.IsGenericParameter (p) && TypeManager.IsGenericParameter (q))
				return p;

			if (TypeManager.HasElementType (p)) 
			{
				Type pe = TypeManager.GetElementType (p);
				Type qe = TypeManager.GetElementType (q);
				Type specific = MoreSpecific (pe, qe);
				if (specific == pe)
					return p;
				if (specific == qe)
					return q;
			} 
			else if (TypeManager.IsGenericType (p)) 
			{
				Type[] pargs = TypeManager.GetTypeArguments (p);
				Type[] qargs = TypeManager.GetTypeArguments (q);

				bool p_specific_at_least_once = false;
				bool q_specific_at_least_once = false;

				for (int i = 0; i < pargs.Length; i++) 
				{
					Type specific = MoreSpecific (TypeManager.TypeToCoreType (pargs [i]), TypeManager.TypeToCoreType (qargs [i]));
					if (specific == pargs [i])
						p_specific_at_least_once = true;
					if (specific == qargs [i])
						q_specific_at_least_once = true;
				}

				if (p_specific_at_least_once && !q_specific_at_least_once)
					return p;
				if (!p_specific_at_least_once && q_specific_at_least_once)
					return q;
			}

			return null;
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			base.MutateHoistedGenericType (storey);

			MethodInfo mi = best_candidate as MethodInfo;
			if (mi != null) {
				best_candidate = storey.MutateGenericMethod (mi);
				return;
			}

			best_candidate = storey.MutateConstructor ((ConstructorInfo) this);
		}

		/// <summary>
		///   Find the Applicable Function Members (7.4.2.1)
		///
		///   me: Method Group expression with the members to select.
		///       it might contain constructors or methods (or anything
		///       that maps to a method).
		///
		///   Arguments: ArrayList containing resolved Argument objects.
		///
		///   loc: The location if we want an error to be reported, or a Null
		///        location for "probing" purposes.
		///
		///   Returns: The MethodBase (either a ConstructorInfo or a MethodInfo)
		///            that is the best match of me on Arguments.
		///
		/// </summary>
		public virtual MethodGroupExpr OverloadResolve (ResolveContext ec, ref Arguments Arguments,
			bool may_fail, Location loc)
		{
			bool method_params = false;
			Type applicable_type = null;
			ArrayList candidates = new ArrayList (2);
			ArrayList candidate_overrides = null;

			//
			// Used to keep a map between the candidate
			// and whether it is being considered in its
			// normal or expanded form
			//
			// false is normal form, true is expanded form
			//
			Hashtable candidate_to_form = null;
			Hashtable candidates_expanded = null;
			Arguments candidate_args = Arguments;

			int arg_count = Arguments != null ? Arguments.Count : 0;

			if (RootContext.Version == LanguageVersion.ISO_1 && Name == "Invoke" && TypeManager.IsDelegateType (DeclaringType)) {
				if (!may_fail)
					ec.Report.Error (1533, loc, "Invoke cannot be called directly on a delegate");
				return null;
			}

			int nmethods = Methods.Length;

			if (!IsBase) {
				//
				// Methods marked 'override' don't take part in 'applicable_type'
				// computation, nor in the actual overload resolution.
				// However, they still need to be emitted instead of a base virtual method.
				// So, we salt them away into the 'candidate_overrides' array.
				//
				// In case of reflected methods, we replace each overriding method with
				// its corresponding base virtual method.  This is to improve compatibility
				// with non-C# libraries which change the visibility of overrides (#75636)
				//
				int j = 0;
				for (int i = 0; i < Methods.Length; ++i) {
					MethodBase m = Methods [i];
					if (TypeManager.IsOverride (m)) {
						if (candidate_overrides == null)
							candidate_overrides = new ArrayList ();
						candidate_overrides.Add (m);
						m = TypeManager.TryGetBaseDefinition (m);
					}
					if (m != null)
						Methods [j++] = m;
				}
				nmethods = j;
			}

			//
			// Enable message recording, it's used mainly by lambda expressions
			//
			SessionReportPrinter msg_recorder = new SessionReportPrinter ();
			ReportPrinter prev_recorder = ec.Report.SetPrinter (msg_recorder);

			//
			// First we construct the set of applicable methods
			//
			bool is_sorted = true;
			int best_candidate_rate = int.MaxValue;
			for (int i = 0; i < nmethods; i++) {
				Type decl_type = Methods [i].DeclaringType;

				//
				// If we have already found an applicable method
				// we eliminate all base types (Section 14.5.5.1)
				//
				if (applicable_type != null && IsAncestralType (decl_type, applicable_type))
					continue;

				//
				// Check if candidate is applicable (section 14.4.2.1)
				//
				bool params_expanded_form = false;
				int candidate_rate = IsApplicable (ec, ref candidate_args, arg_count, ref Methods [i], ref params_expanded_form);

				if (candidate_rate < best_candidate_rate) {
					best_candidate_rate = candidate_rate;
					best_candidate = Methods [i];
				}
				
				if (params_expanded_form) {
					if (candidate_to_form == null)
						candidate_to_form = new PtrHashtable ();
					MethodBase candidate = Methods [i];
					candidate_to_form [candidate] = candidate;
				}
				
				if (candidate_args != Arguments) {
					if (candidates_expanded == null)
						candidates_expanded = new Hashtable (2);

					candidates_expanded.Add (Methods [i], candidate_args);
					candidate_args = Arguments;
				}

				if (candidate_rate != 0 || has_inaccessible_candidates_only) {
					if (msg_recorder != null)
						msg_recorder.EndSession ();
					continue;
				}

				msg_recorder = null;
				candidates.Add (Methods [i]);

				if (applicable_type == null)
					applicable_type = decl_type;
				else if (applicable_type != decl_type) {
					is_sorted = false;
					if (IsAncestralType (applicable_type, decl_type))
						applicable_type = decl_type;
				}
			}

			ec.Report.SetPrinter (prev_recorder);
			if (msg_recorder != null && !msg_recorder.IsEmpty) {
				if (!may_fail)
					msg_recorder.Merge (prev_recorder);

				return null;
			}
			
			int candidate_top = candidates.Count;

			if (applicable_type == null) {
				//
				// When we found a top level method which does not match and it's 
				// not an extension method. We start extension methods lookup from here
				//
				if (InstanceExpression != null) {
					ExtensionMethodGroupExpr ex_method_lookup = ec.LookupExtensionMethod (type, Name, loc);
					if (ex_method_lookup != null) {
						ex_method_lookup.ExtensionExpression = InstanceExpression;
						ex_method_lookup.SetTypeArguments (ec, type_arguments);
						return ex_method_lookup.OverloadResolve (ec, ref Arguments, may_fail, loc);
					}
				}
				
				if (may_fail)
					return null;

				//
				// Okay so we have failed to find exact match so we
				// return error info about the closest match
				//
				if (best_candidate != null) {
					if (CustomErrorHandler != null && !has_inaccessible_candidates_only && CustomErrorHandler.NoExactMatch (ec, best_candidate))
						return null;

					AParametersCollection pd = TypeManager.GetParameterData (best_candidate);
					bool cand_params = candidate_to_form != null && candidate_to_form.Contains (best_candidate);
					if (arg_count == pd.Count || pd.HasParams) {
						if (TypeManager.IsGenericMethodDefinition (best_candidate)) {
							if (type_arguments == null) {
								ec.Report.Error (411, loc,
									"The type arguments for method `{0}' cannot be inferred from " +
									"the usage. Try specifying the type arguments explicitly",
									TypeManager.CSharpSignature (best_candidate));
								return null;
							}

							Type[] g_args = TypeManager.GetGenericArguments (best_candidate);
							if (type_arguments.Count != g_args.Length) {
								ec.Report.SymbolRelatedToPreviousError (best_candidate);
								ec.Report.Error (305, loc, "Using the generic method `{0}' requires `{1}' type argument(s)",
									TypeManager.CSharpSignature (best_candidate),
									g_args.Length.ToString ());
								return null;
							}
						} else {
							if (type_arguments != null && !TypeManager.IsGenericMethod (best_candidate)) {
								Namespace.Error_TypeArgumentsCannotBeUsed (best_candidate, loc);
								return null;
							}
						}

						if (has_inaccessible_candidates_only) {
							if (InstanceExpression != null && type != ec.CurrentType && TypeManager.IsNestedFamilyAccessible (ec.CurrentType, best_candidate.DeclaringType)) {
								// Although a derived class can access protected members of
								// its base class it cannot do so through an instance of the
								// base class (CS1540).  If the qualifier_type is a base of the
								// ec.CurrentType and the lookup succeeds with the latter one,
								// then we are in this situation.
								Error_CannotAccessProtected (ec, loc, best_candidate, queried_type, ec.CurrentType);
							} else {
								ec.Report.SymbolRelatedToPreviousError (best_candidate);
								ErrorIsInaccesible (loc, GetSignatureForError (), ec.Report);
							}
						}

						if (!VerifyArgumentsCompat (ec, ref Arguments, arg_count, best_candidate, cand_params, may_fail, loc))
							return null;

						if (has_inaccessible_candidates_only)
							return null;

						throw new InternalErrorException ("VerifyArgumentsCompat didn't find any problem with rejected candidate " + best_candidate);
					}
				}

				//
				// We failed to find any method with correct argument count
				//
				if (Name == ConstructorInfo.ConstructorName) {
					ec.Report.SymbolRelatedToPreviousError (queried_type);
					ec.Report.Error (1729, loc,
						"The type `{0}' does not contain a constructor that takes `{1}' arguments",
						TypeManager.CSharpName (queried_type), arg_count);
				} else {
					Error_ArgumentCountWrong (ec, arg_count);
				}
                                
				return null;
			}

			if (!is_sorted) {
				//
				// At this point, applicable_type is _one_ of the most derived types
				// in the set of types containing the methods in this MethodGroup.
				// Filter the candidates so that they only contain methods from the
				// most derived types.
				//

				int finalized = 0; // Number of finalized candidates

				do {
					// Invariant: applicable_type is a most derived type
					
					// We'll try to complete Section 14.5.5.1 for 'applicable_type' by 
					// eliminating all it's base types.  At the same time, we'll also move
					// every unrelated type to the end of the array, and pick the next
					// 'applicable_type'.

					Type next_applicable_type = null;
					int j = finalized; // where to put the next finalized candidate
					int k = finalized; // where to put the next undiscarded candidate
					for (int i = finalized; i < candidate_top; ++i) {
						MethodBase candidate = (MethodBase) candidates [i];
						Type decl_type = candidate.DeclaringType;

						if (decl_type == applicable_type) {
							candidates [k++] = candidates [j];
							candidates [j++] = candidates [i];
							continue;
						}

						if (IsAncestralType (decl_type, applicable_type))
							continue;

						if (next_applicable_type != null &&
							IsAncestralType (decl_type, next_applicable_type))
							continue;

						candidates [k++] = candidates [i];

						if (next_applicable_type == null ||
							IsAncestralType (next_applicable_type, decl_type))
							next_applicable_type = decl_type;
					}

					applicable_type = next_applicable_type;
					finalized = j;
					candidate_top = k;
				} while (applicable_type != null);
			}

			//
			// Now we actually find the best method
			//

			best_candidate = (MethodBase) candidates [0];
			method_params = candidate_to_form != null && candidate_to_form.Contains (best_candidate);

			//
			// TODO: Broken inverse order of candidates logic does not work with optional
			// parameters used for method overrides and I am not going to fix it for SRE
			//
			if (candidates_expanded != null && candidates_expanded.Contains (best_candidate)) {
				candidate_args = (Arguments) candidates_expanded [best_candidate];
				arg_count = candidate_args.Count;
			}

			for (int ix = 1; ix < candidate_top; ix++) {
				MethodBase candidate = (MethodBase) candidates [ix];

				if (candidate == best_candidate)
					continue;

				bool cand_params = candidate_to_form != null && candidate_to_form.Contains (candidate);

				if (BetterFunction (ec, candidate_args, arg_count, 
					candidate, cand_params,
					best_candidate, method_params)) {
					best_candidate = candidate;
					method_params = cand_params;
				}
			}
			//
			// Now check that there are no ambiguities i.e the selected method
			// should be better than all the others
			//
			MethodBase ambiguous = null;
			for (int ix = 1; ix < candidate_top; ix++) {
				MethodBase candidate = (MethodBase) candidates [ix];

				if (candidate == best_candidate)
					continue;

				bool cand_params = candidate_to_form != null && candidate_to_form.Contains (candidate);
				if (!BetterFunction (ec, candidate_args, arg_count,
					best_candidate, method_params,
					candidate, cand_params)) 
				{
					if (!may_fail)
						ec.Report.SymbolRelatedToPreviousError (candidate);
					ambiguous = candidate;
				}
			}

			if (ambiguous != null) {
				Error_AmbiguousCall (ec, ambiguous);
				return this;
			}

			//
			// If the method is a virtual function, pick an override closer to the LHS type.
			//
			if (!IsBase && best_candidate.IsVirtual) {
				if (TypeManager.IsOverride (best_candidate))
					throw new InternalErrorException (
						"Should not happen.  An 'override' method took part in overload resolution: " + best_candidate);

				if (candidate_overrides != null) {
					Type[] gen_args = null;
					bool gen_override = false;
					if (TypeManager.IsGenericMethod (best_candidate))
						gen_args = TypeManager.GetGenericArguments (best_candidate);

					foreach (MethodBase candidate in candidate_overrides) {
						if (TypeManager.IsGenericMethod (candidate)) {
							if (gen_args == null)
								continue;

							if (gen_args.Length != TypeManager.GetGenericArguments (candidate).Length)
								continue;
						} else {
							if (gen_args != null)
								continue;
						}
						
						if (IsOverride (candidate, best_candidate)) {
							gen_override = true;
							best_candidate = candidate;
						}
					}

					if (gen_override && gen_args != null) {
#if GMCS_SOURCE
						best_candidate = ((MethodInfo) best_candidate).MakeGenericMethod (gen_args);
#endif						
					}
				}
			}

			//
			// And now check if the arguments are all
			// compatible, perform conversions if
			// necessary etc. and return if everything is
			// all right
			//
			if (!VerifyArgumentsCompat (ec, ref candidate_args, arg_count, best_candidate,
				method_params, may_fail, loc))
				return null;

			if (best_candidate == null)
				return null;

			MethodBase the_method = TypeManager.DropGenericMethodArguments (best_candidate);
			if (TypeManager.IsGenericMethodDefinition (the_method) &&
			    !ConstraintChecker.CheckConstraints (ec, the_method, best_candidate, loc))
				return null;

			//
			// Check ObsoleteAttribute on the best method
			//
			ObsoleteAttribute oa = AttributeTester.GetMethodObsoleteAttribute (the_method);
			if (oa != null && !ec.IsObsolete)
				AttributeTester.Report_ObsoleteMessage (oa, GetSignatureForError (), loc, ec.Report);

			IMethodData data = TypeManager.GetMethod (the_method);
			if (data != null)
				data.SetMemberIsUsed ();

			Arguments = candidate_args;
			return this;
		}
		
		public override void SetTypeArguments (ResolveContext ec, TypeArguments ta)
		{
			type_arguments = ta;
		}

		public bool VerifyArgumentsCompat (ResolveContext ec, ref Arguments arguments,
							  int arg_count, MethodBase method,
							  bool chose_params_expanded,
							  bool may_fail, Location loc)
		{
			AParametersCollection pd = TypeManager.GetParameterData (method);
			int param_count = GetApplicableParametersCount (method, pd);

			int errors = ec.Report.Errors;
			Parameter.Modifier p_mod = 0;
			Type pt = null;
			int a_idx = 0, a_pos = 0;
			Argument a = null;
			ArrayList params_initializers = null;
			bool has_unsafe_arg = false;

			for (; a_idx < arg_count; a_idx++, ++a_pos) {
				a = arguments [a_idx];
				if (p_mod != Parameter.Modifier.PARAMS) {
					p_mod = pd.FixedParameters [a_idx].ModFlags;
					pt = pd.Types [a_idx];
					has_unsafe_arg |= pt.IsPointer;

					if (p_mod == Parameter.Modifier.PARAMS) {
						if (chose_params_expanded) {
							params_initializers = new ArrayList (arg_count - a_idx);
							pt = TypeManager.GetElementType (pt);
						}
					}
				}

				//
				// Types have to be identical when ref or out modifer is used 
				//
				if (a.Modifier != 0 || (p_mod & ~Parameter.Modifier.PARAMS) != 0) {
					if ((p_mod & ~Parameter.Modifier.PARAMS) != a.Modifier)
						break;

					if (!TypeManager.IsEqual (a.Expr.Type, pt))
						break;

					continue;
				} else {
					NamedArgument na = a as NamedArgument;
					if (na != null) {
						int name_index = pd.GetParameterIndexByName (na.Name.Value);
						if (name_index < 0 || name_index >= param_count) {
							if (DeclaringType != null && TypeManager.IsDelegateType (DeclaringType)) {
								ec.Report.SymbolRelatedToPreviousError (DeclaringType);
								ec.Report.Error (1746, na.Name.Location,
									"The delegate `{0}' does not contain a parameter named `{1}'",
									TypeManager.CSharpName (DeclaringType), na.Name.Value);
							} else {
								ec.Report.SymbolRelatedToPreviousError (best_candidate);
								ec.Report.Error (1739, na.Name.Location,
									"The best overloaded method match for `{0}' does not contain a parameter named `{1}'",
									TypeManager.CSharpSignature (method), na.Name.Value);
							}
						} else if (arguments[name_index] != a) {
							if (DeclaringType != null && TypeManager.IsDelegateType (DeclaringType))
								ec.Report.SymbolRelatedToPreviousError (DeclaringType);
							else
								ec.Report.SymbolRelatedToPreviousError (best_candidate);

							ec.Report.Error (1744, na.Name.Location,
								"Named argument `{0}' cannot be used for a parameter which has positional argument specified",
								na.Name.Value);
						}
					}
				}

				if (delegate_type != null && !Delegate.IsTypeCovariant (a.Expr, pt))
					break;

				Expression conv = Convert.ImplicitConversion (ec, a.Expr, pt, loc);
				if (conv == null)
					break;

				//
				// Convert params arguments to an array initializer
				//
				if (params_initializers != null) {
					// we choose to use 'a.Expr' rather than 'conv' so that
					// we don't hide the kind of expression we have (esp. CompoundAssign.Helper)
					params_initializers.Add (a.Expr);
					arguments.RemoveAt (a_idx--);
					--arg_count;
					continue;
				}

				// Update the argument with the implicit conversion
				a.Expr = conv;
			}

			if (a_idx != arg_count) {
				if (!may_fail && ec.Report.Errors == errors) {
					if (CustomErrorHandler != null)
						CustomErrorHandler.NoExactMatch (ec, best_candidate);
					else
						Error_InvalidArguments (ec, loc, a_pos, method, a, pd, pt);
				}
				return false;
			}

			//
			// Fill not provided arguments required by params modifier
			//
			if (params_initializers == null && pd.HasParams && arg_count + 1 == param_count) {
				if (arguments == null)
					arguments = new Arguments (1);

				pt = pd.Types [param_count - 1];
				pt = TypeManager.GetElementType (pt);
				has_unsafe_arg |= pt.IsPointer;
				params_initializers = new ArrayList (0);
			}

			//
			// Append an array argument with all params arguments
			//
			if (params_initializers != null) {
				arguments.Add (new Argument (
						       new ArrayCreation (new TypeExpression (pt, loc), "[]",
									  params_initializers, loc).Resolve (ec)));
				arg_count++;
			}

			if (arg_count < param_count) {
				if (!may_fail)
					Error_ArgumentCountWrong (ec, arg_count);
				return false;
			}

			if (has_unsafe_arg && !ec.IsUnsafe) {
				if (!may_fail)
					UnsafeError (ec, loc);
				return false;
			}

			return true;
		}
	}

	public class ConstantExpr : MemberExpr
	{
		FieldInfo constant;

		public ConstantExpr (FieldInfo constant, Location loc)
		{
			this.constant = constant;
			this.loc = loc;
		}

		public override string Name {
			get { throw new NotImplementedException (); }
		}

		public override bool IsInstance {
			get { return !IsStatic; }
		}

		public override bool IsStatic {
			get { return constant.IsStatic; }
		}

		public override Type DeclaringType {
			get { return constant.DeclaringType; }
		}

		public override MemberExpr ResolveMemberAccess (ResolveContext ec, Expression left, Location loc, SimpleName original)
		{
			constant = TypeManager.GetGenericFieldDefinition (constant);

			IConstant ic = TypeManager.GetConstant (constant);
			if (ic == null) {
				if (constant.IsLiteral) {
					ic = new ExternalConstant (constant);
				} else {
					ic = ExternalConstant.CreateDecimal (constant);
					// HACK: decimal field was not resolved as constant
					if (ic == null)
						return new FieldExpr (constant, loc).ResolveMemberAccess (ec, left, loc, original);
				}
				TypeManager.RegisterConstant (constant, ic);
			}

			return base.ResolveMemberAccess (ec, left, loc, original);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			IConstant ic = TypeManager.GetConstant (constant);
			if (ic.ResolveValue ()) {
				if (!ec.IsObsolete)
					ic.CheckObsoleteness (loc);
			}

			return ic.CreateConstantReference (loc);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.GetFullNameSignature (constant);
		}
	}

	/// <summary>
	///   Fully resolved expression that evaluates to a Field
	/// </summary>
	public class FieldExpr : MemberExpr, IAssignMethod, IMemoryLocation, IVariableReference {
		public FieldInfo FieldInfo;
		readonly Type constructed_generic_type;
		VariableInfo variable_info;
		
		LocalTemporary temp;
		bool prepared;
		
		protected FieldExpr (Location l)
		{
			loc = l;
		}
		
		public FieldExpr (FieldInfo fi, Location l)
		{
			FieldInfo = fi;
			type = TypeManager.TypeToCoreType (fi.FieldType);
			loc = l;
		}

		public FieldExpr (FieldInfo fi, Type genericType, Location l)
			: this (fi, l)
		{
			if (TypeManager.IsGenericTypeDefinition (genericType))
				return;
			this.constructed_generic_type = genericType;
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

		public override MemberExpr ResolveMemberAccess (ResolveContext ec, Expression left, Location loc,
								SimpleName original)
		{
			FieldInfo fi = TypeManager.GetGenericFieldDefinition (FieldInfo);
			Type t = fi.FieldType;

			if (t.IsPointer && !ec.IsUnsafe) {
				UnsafeError (ec, loc);
			}

			return base.ResolveMemberAccess (ec, left, loc, original);
		}

		public void SetHasAddressTaken ()
		{
			IVariableReference vr = InstanceExpression as IVariableReference;
			if (vr != null)
				vr.SetHasAddressTaken ();
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Expression instance;
			if (InstanceExpression == null) {
				instance = new NullLiteral (loc);
			} else {
				instance = InstanceExpression.CreateExpressionTree (ec);
			}

			Arguments args = Arguments.CreateForExpressionTree (ec, null,
				instance,
				CreateTypeOfExpression ());

			return CreateExpressionFactoryCall (ec, "Field", args);
		}

		public Expression CreateTypeOfExpression ()
		{
			return new TypeOfField (GetConstructedFieldInfo (), loc);
		}

		override public Expression DoResolve (ResolveContext ec)
		{
			return DoResolve (ec, false, false);
		}

		Expression DoResolve (ResolveContext ec, bool lvalue_instance, bool out_access)
		{
			if (!FieldInfo.IsStatic){
				if (InstanceExpression == null){
					//
					// This can happen when referencing an instance field using
					// a fully qualified type expression: TypeName.InstanceField = xxx
					// 
					SimpleName.Error_ObjectRefRequired (ec, loc, GetSignatureForError ());
					return null;
				}

				// Resolve the field's instance expression while flow analysis is turned
				// off: when accessing a field "a.b", we must check whether the field
				// "a.b" is initialized, not whether the whole struct "a" is initialized.

				if (lvalue_instance) {
					using (ec.With (ResolveContext.Options.DoFlowAnalysis, false)) {
						Expression right_side =
							out_access ? EmptyExpression.LValueMemberOutAccess : EmptyExpression.LValueMemberAccess;

						if (InstanceExpression != EmptyExpression.Null)
							InstanceExpression = InstanceExpression.ResolveLValue (ec, right_side);
					}
				} else {
					ResolveFlags rf = ResolveFlags.VariableOrValue | ResolveFlags.DisableFlowAnalysis;

					if (InstanceExpression != EmptyExpression.Null)
						InstanceExpression = InstanceExpression.Resolve (ec, rf);
				}

				if (InstanceExpression == null)
					return null;

				using (ec.Set (ResolveContext.Options.OmitStructFlowAnalysis)) {
					InstanceExpression.CheckMarshalByRefAccess (ec);
				}
			}

			// TODO: the code above uses some non-standard multi-resolve rules
			if (eclass != ExprClass.Invalid)
				return this;

			if (!ec.IsObsolete) {
				FieldBase f = TypeManager.GetField (FieldInfo);
				if (f != null) {
					f.CheckObsoleteness (loc);
				} else {
					ObsoleteAttribute oa = AttributeTester.GetMemberObsoleteAttribute (FieldInfo);
					if (oa != null)
						AttributeTester.Report_ObsoleteMessage (oa, TypeManager.GetFullNameSignature (FieldInfo), loc, ec.Report);
				}
			}

			IFixedBuffer fb = AttributeTester.GetFixedBuffer (FieldInfo);
			IVariableReference var = InstanceExpression as IVariableReference;
			
			if (fb != null) {
				IFixedExpression fe = InstanceExpression as IFixedExpression;
				if (!ec.HasSet (ResolveContext.Options.FixedInitializerScope) && (fe == null || !fe.IsFixed)) {
					ec.Report.Error (1666, loc, "You cannot use fixed size buffers contained in unfixed expressions. Try using the fixed statement");
				}

				if (InstanceExpression.eclass != ExprClass.Variable) {
					ec.Report.SymbolRelatedToPreviousError (FieldInfo);
					ec.Report.Error (1708, loc, "`{0}': Fixed size buffers can only be accessed through locals or fields",
						TypeManager.GetFullNameSignature (FieldInfo));
				} else if (var != null && var.IsHoisted) {
					AnonymousMethodExpression.Error_AddressOfCapturedVar (ec, var, loc);
				}
				
				return new FixedBufferPtr (this, fb.ElementType, loc).Resolve (ec);
			}

			eclass = ExprClass.Variable;

			// If the instance expression is a local variable or parameter.
			if (var == null || var.VariableInfo == null)
				return this;

			VariableInfo vi = var.VariableInfo;
			if (!vi.IsFieldAssigned (ec, FieldInfo.Name, loc))
				return null;

			variable_info = vi.GetSubStruct (FieldInfo.Name);
			eclass = ExprClass.Variable;
			return this;
		}

		static readonly int [] codes = {
			191,	// instance, write access
			192,	// instance, out access
			198,	// static, write access
			199,	// static, out access
			1648,	// member of value instance, write access
			1649,	// member of value instance, out access
			1650,	// member of value static, write access
			1651	// member of value static, out access
		};

		static readonly string [] msgs = {
			/*0191*/ "A readonly field `{0}' cannot be assigned to (except in a constructor or a variable initializer)",
			/*0192*/ "A readonly field `{0}' cannot be passed ref or out (except in a constructor)",
			/*0198*/ "A static readonly field `{0}' cannot be assigned to (except in a static constructor or a variable initializer)",
			/*0199*/ "A static readonly field `{0}' cannot be passed ref or out (except in a static constructor)",
			/*1648*/ "Members of readonly field `{0}' cannot be modified (except in a constructor or a variable initializer)",
			/*1649*/ "Members of readonly field `{0}' cannot be passed ref or out (except in a constructor)",
			/*1650*/ "Fields of static readonly field `{0}' cannot be assigned to (except in a static constructor or a variable initializer)",
			/*1651*/ "Fields of static readonly field `{0}' cannot be passed ref or out (except in a static constructor)"
		};

		// The return value is always null.  Returning a value simplifies calling code.
		Expression Report_AssignToReadonly (ResolveContext ec, Expression right_side)
		{
			int i = 0;
			if (right_side == EmptyExpression.OutAccess || right_side == EmptyExpression.LValueMemberOutAccess)
				i += 1;
			if (IsStatic)
				i += 2;
			if (right_side == EmptyExpression.LValueMemberAccess || right_side == EmptyExpression.LValueMemberOutAccess)
				i += 4;
			ec.Report.Error (codes [i], loc, msgs [i], GetSignatureForError ());

			return null;
		}
		
		override public Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			IVariableReference var = InstanceExpression as IVariableReference;
			if (var != null && var.VariableInfo != null)
				var.VariableInfo.SetFieldAssigned (ec, FieldInfo.Name);

			bool lvalue_instance = !FieldInfo.IsStatic && TypeManager.IsValueType (FieldInfo.DeclaringType);
			bool out_access = right_side == EmptyExpression.OutAccess || right_side == EmptyExpression.LValueMemberOutAccess;

			Expression e = DoResolve (ec, lvalue_instance, out_access);

			if (e == null)
				return null;

			FieldBase fb = TypeManager.GetField (FieldInfo);
			if (fb != null) {
				fb.SetAssigned ();

				if ((right_side == EmptyExpression.UnaryAddress || right_side == EmptyExpression.OutAccess) &&
					(fb.ModFlags & Modifiers.VOLATILE) != 0) {
					ec.Report.Warning (420, 1, loc,
						"`{0}': A volatile field references will not be treated as volatile",
						fb.GetSignatureForError ());
				}
			}

			if (FieldInfo.IsInitOnly) {
				// InitOnly fields can only be assigned in constructors or initializers
				if (!ec.HasAny (ResolveContext.Options.FieldInitializerScope | ResolveContext.Options.ConstructorScope))
					return Report_AssignToReadonly (ec, right_side);

				if (ec.HasSet (ResolveContext.Options.ConstructorScope)) {
					Type ctype = ec.CurrentType;

					// InitOnly fields cannot be assigned-to in a different constructor from their declaring type
					if (!TypeManager.IsEqual (ctype, FieldInfo.DeclaringType))
						return Report_AssignToReadonly (ec, right_side);
					// static InitOnly fields cannot be assigned-to in an instance constructor
					if (IsStatic && !ec.IsStatic)
						return Report_AssignToReadonly (ec, right_side);
					// instance constructors can't modify InitOnly fields of other instances of the same type
					if (!IsStatic && !(InstanceExpression is This))
						return Report_AssignToReadonly (ec, right_side);
				}
			}

			if (right_side == EmptyExpression.OutAccess &&
			    !IsStatic && !(InstanceExpression is This) && TypeManager.mbr_type != null && TypeManager.IsSubclassOf (DeclaringType, TypeManager.mbr_type)) {
				ec.Report.SymbolRelatedToPreviousError (DeclaringType);
				ec.Report.Warning (197, 1, loc,
						"Passing `{0}' as ref or out or taking its address may cause a runtime exception because it is a field of a marshal-by-reference class",
						GetSignatureForError ());
			}

			eclass = ExprClass.Variable;
			return this;
		}

		bool is_marshal_by_ref ()
		{
			return !IsStatic && TypeManager.IsStruct (Type) && TypeManager.mbr_type != null && TypeManager.IsSubclassOf (DeclaringType, TypeManager.mbr_type);
		}

		public override void CheckMarshalByRefAccess (ResolveContext ec)
		{
			if (is_marshal_by_ref () && !(InstanceExpression is This)) {
				ec.Report.SymbolRelatedToPreviousError (DeclaringType);
				ec.Report.Warning (1690, 1, loc, "Cannot call methods, properties, or indexers on `{0}' because it is a value type member of a marshal-by-reference class",
						GetSignatureForError ());
			}
		}

		public override int GetHashCode ()
		{
			return FieldInfo.GetHashCode ();
		}
		
		public bool IsFixed {
			get {
				//
				// A variable of the form V.I is fixed when V is a fixed variable of a struct type
				//
				IVariableReference variable = InstanceExpression as IVariableReference;
				if (variable != null)
					return TypeManager.IsStruct (InstanceExpression.Type) && variable.IsFixed;

				IFixedExpression fe = InstanceExpression as IFixedExpression;
				return fe != null && fe.IsFixed;
			}
		}

		public bool IsHoisted {
			get {
				IVariableReference hv = InstanceExpression as IVariableReference;
				return hv != null && hv.IsHoisted;
			}
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

			FieldBase f = TypeManager.GetField (FieldInfo);
			if (f != null){
				if ((f.ModFlags & Modifiers.VOLATILE) != 0)
					is_volatile = true;

				f.SetMemberIsUsed ();
			}
			
			if (FieldInfo.IsStatic){
				if (is_volatile)
					ig.Emit (OpCodes.Volatile);

				ig.Emit (OpCodes.Ldsfld, GetConstructedFieldInfo ());
			} else {
				if (!prepared)
					EmitInstance (ec, false);

				// Optimization for build-in types
				if (TypeManager.IsStruct (type) && TypeManager.IsEqual (type, ec.MemberContext.CurrentType)) {
					LoadFromPtr (ig, type);
				} else {
					IFixedBuffer ff = AttributeTester.GetFixedBuffer (FieldInfo);
					if (ff != null) {
						ig.Emit (OpCodes.Ldflda, GetConstructedFieldInfo ());
						ig.Emit (OpCodes.Ldflda, ff.Element);
					} else {
						if (is_volatile)
							ig.Emit (OpCodes.Volatile);

						ig.Emit (OpCodes.Ldfld, GetConstructedFieldInfo ());
					}
				}
			}

			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				if (!FieldInfo.IsStatic) {
					temp = new LocalTemporary (this.Type);
					temp.Store (ec);
				}
			}
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			FieldAttributes fa = FieldInfo.Attributes;
			bool is_static = (fa & FieldAttributes.Static) != 0;
			ILGenerator ig = ec.ig;

			prepared = prepare_for_load;
			EmitInstance (ec, prepared);

			source.Emit (ec);
			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				if (!FieldInfo.IsStatic) {
					temp = new LocalTemporary (this.Type);
					temp.Store (ec);
				}
			}

			FieldBase f = TypeManager.GetField (FieldInfo);
			if (f != null){
				if ((f.ModFlags & Modifiers.VOLATILE) != 0)
					ig.Emit (OpCodes.Volatile);
					
				f.SetAssigned ();
			}

			if (is_static)
				ig.Emit (OpCodes.Stsfld, GetConstructedFieldInfo ());
			else
				ig.Emit (OpCodes.Stfld, GetConstructedFieldInfo ());
			
			if (temp != null) {
				temp.Emit (ec);
				temp.Release (ec);
				temp = null;
			}
		}

		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}

		public override void EmitSideEffect (EmitContext ec)
		{
			FieldBase f = TypeManager.GetField (FieldInfo);
			bool is_volatile = f != null && (f.ModFlags & Modifiers.VOLATILE) != 0;

			if (is_volatile || is_marshal_by_ref ())
				base.EmitSideEffect (ec);
		}

		public override void Error_VariableIsUsedBeforeItIsDeclared (Report r, string name)
		{
			r.Error (844, loc,
				"A local variable `{0}' cannot be used before it is declared. Consider renaming the local variable when it hides the field `{1}'",
				name, GetSignatureForError ());
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			ILGenerator ig = ec.ig;

			FieldBase f = TypeManager.GetField (FieldInfo);
			if (f != null){				
				if ((mode & AddressOp.Store) != 0)
					f.SetAssigned ();
				if ((mode & AddressOp.Load) != 0)
					f.SetMemberIsUsed ();
			}

			//
			// Handle initonly fields specially: make a copy and then
			// get the address of the copy.
			//
			bool need_copy;
			if (FieldInfo.IsInitOnly){
				need_copy = true;
				if (ec.HasSet (EmitContext.Options.ConstructorScope)){
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
				ig.Emit (OpCodes.Ldsflda, GetConstructedFieldInfo ());
			} else {
				if (!prepared)
					EmitInstance (ec, false);
				ig.Emit (OpCodes.Ldflda, GetConstructedFieldInfo ());
			}
		}

		FieldInfo GetConstructedFieldInfo ()
		{
			if (constructed_generic_type == null)
				return FieldInfo;
#if GMCS_SOURCE
			return TypeBuilder.GetField (constructed_generic_type, FieldInfo);
#else
			throw new NotSupportedException ();
#endif			
		}
		
		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			FieldInfo = storey.MutateField (FieldInfo);
			base.MutateHoistedGenericType (storey);
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
		MethodInfo getter, setter;
		bool is_static;

		bool resolved;
		
		LocalTemporary temp;
		bool prepared;

		public PropertyExpr (Type container_type, PropertyInfo pi, Location l)
		{
			PropertyInfo = pi;
			eclass = ExprClass.PropertyAccess;
			is_static = false;
			loc = l;

			type = TypeManager.TypeToCoreType (pi.PropertyType);

			ResolveAccessors (container_type);
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

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args;
			if (IsSingleDimensionalArrayLength ()) {
				args = new Arguments (1);
				args.Add (new Argument (InstanceExpression.CreateExpressionTree (ec)));
				return CreateExpressionFactoryCall (ec, "ArrayLength", args);
			}

			if (is_base) {
				Error_BaseAccessInExpressionTree (ec, loc);
				return null;
			}

			args = new Arguments (2);
			if (InstanceExpression == null)
				args.Add (new Argument (new NullLiteral (loc)));
			else
				args.Add (new Argument (InstanceExpression.CreateExpressionTree (ec)));
			args.Add (new Argument (new TypeOfMethod (getter, loc)));
			return CreateExpressionFactoryCall (ec, "Property", args);
		}

		public Expression CreateSetterTypeOfExpression ()
		{
			return new TypeOfMethod (setter, loc);
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
		// TODO: Refactor to use some kind of cache together with GetPropertyFromAccessor
		void ResolveAccessors (Type container_type)
		{
			FindAccessors (container_type);

			if (getter != null) {
				MethodBase the_getter = TypeManager.DropGenericMethodArguments (getter);
				IMethodData md = TypeManager.GetMethod (the_getter);
				if (md != null)
					md.SetMemberIsUsed ();

				is_static = getter.IsStatic;
			}

			if (setter != null) {
				MethodBase the_setter = TypeManager.DropGenericMethodArguments (setter);
				IMethodData md = TypeManager.GetMethod (the_setter);
				if (md != null)
					md.SetMemberIsUsed ();

				is_static = setter.IsStatic;
			}
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			if (InstanceExpression != null)
				InstanceExpression.MutateHoistedGenericType (storey);

			type = storey.MutateType (type);
			if (getter != null)
				getter = storey.MutateGenericMethod (getter);
			if (setter != null)
				setter = storey.MutateGenericMethod (setter);
		}

		bool InstanceResolve (ResolveContext ec, bool lvalue_instance, bool must_do_cs1540_check)
		{
			if (is_static) {
				InstanceExpression = null;
				return true;
			}

			if (InstanceExpression == null) {
				SimpleName.Error_ObjectRefRequired (ec, loc, GetSignatureForError ());
				return false;
			}

			InstanceExpression = InstanceExpression.DoResolve (ec);
			if (lvalue_instance && InstanceExpression != null)
				InstanceExpression = InstanceExpression.ResolveLValue (ec, EmptyExpression.LValueMemberAccess);

			if (InstanceExpression == null)
				return false;

			InstanceExpression.CheckMarshalByRefAccess (ec);

			if (must_do_cs1540_check && (InstanceExpression != EmptyExpression.Null) &&
			    !TypeManager.IsInstantiationOfSameGenericType (InstanceExpression.Type, ec.CurrentType) &&
			    !TypeManager.IsNestedChildOf (ec.CurrentType, InstanceExpression.Type) &&
			    !TypeManager.IsSubclassOf (InstanceExpression.Type, ec.CurrentType)) {
				ec.Report.SymbolRelatedToPreviousError (PropertyInfo);
				Error_CannotAccessProtected (ec, loc, PropertyInfo, InstanceExpression.Type, ec.CurrentType);
				return false;
			}

			return true;
		}

		void Error_PropertyNotFound (ResolveContext ec, MethodInfo mi, bool getter)
		{
			// TODO: correctly we should compare arguments but it will lead to bigger changes
			if (mi is MethodBuilder) {
				Error_TypeDoesNotContainDefinition (ec, loc, PropertyInfo.DeclaringType, Name);
				return;
			}
			
			StringBuilder sig = new StringBuilder (TypeManager.CSharpName (mi.DeclaringType));
			sig.Append ('.');
			AParametersCollection iparams = TypeManager.GetParameterData (mi);
			sig.Append (getter ? "get_" : "set_");
			sig.Append (Name);
			sig.Append (iparams.GetSignatureForError ());

			ec.Report.SymbolRelatedToPreviousError (mi);
			ec.Report.Error (1546, loc, "Property `{0}' is not supported by the C# language. Try to call the accessor method `{1}' directly",
				Name, sig.ToString ());
		}

		public bool IsAccessibleFrom (Type invocation_type, bool lvalue)
		{
			bool dummy;
			MethodInfo accessor = lvalue ? setter : getter;
			if (accessor == null && lvalue)
				accessor = getter;
			return accessor != null && IsAccessorAccessible (invocation_type, accessor, out dummy);
		}

		bool IsSingleDimensionalArrayLength ()
		{
			if (DeclaringType != TypeManager.array_type || getter == null || Name != "Length")
				return false;

			string t_name = InstanceExpression.Type.Name;
			int t_name_len = t_name.Length;
			return t_name_len > 2 && t_name [t_name_len - 2] == '[';
		}

		override public Expression DoResolve (ResolveContext ec)
		{
			if (resolved)
				return this;

			if (getter != null){
				if (TypeManager.GetParameterData (getter).Count != 0){
					Error_PropertyNotFound (ec, getter, true);
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
					ec.Report.Error (154, loc, "The property or indexer `{0}' cannot be used in this context because it lacks the `get' accessor",
						TypeManager.GetFullNameSignature (PropertyInfo));
					return null;
				}
			} 

			bool must_do_cs1540_check = false;
			if (getter != null &&
			    !IsAccessorAccessible (ec.CurrentType, getter, out must_do_cs1540_check)) {
				PropertyBase.PropertyMethod pm = TypeManager.GetMethod (getter) as PropertyBase.PropertyMethod;
				if (pm != null && pm.HasCustomAccessModifier) {
					ec.Report.SymbolRelatedToPreviousError (pm);
					ec.Report.Error (271, loc, "The property or indexer `{0}' cannot be used in this context because the get accessor is inaccessible",
						TypeManager.CSharpSignature (getter));
				}
				else {
					ec.Report.SymbolRelatedToPreviousError (getter);
					ErrorIsInaccesible (loc, TypeManager.CSharpSignature (getter), ec.Report);
				}
				return null;
			}
			
			if (!InstanceResolve (ec, false, must_do_cs1540_check))
				return null;

			//
			// Only base will allow this invocation to happen.
			//
			if (IsBase && getter.IsAbstract) {
				Error_CannotCallAbstractBase (ec, TypeManager.GetFullNameSignature (PropertyInfo));
			}

			if (PropertyInfo.PropertyType.IsPointer && !ec.IsUnsafe){
				UnsafeError (ec, loc);
			}

			if (!ec.IsObsolete) {
				PropertyBase pb = TypeManager.GetProperty (PropertyInfo);
				if (pb != null) {
					pb.CheckObsoleteness (loc);
				} else {
					ObsoleteAttribute oa = AttributeTester.GetMemberObsoleteAttribute (PropertyInfo);
					if (oa != null)
						AttributeTester.Report_ObsoleteMessage (oa, GetSignatureForError (), loc, ec.Report);
				}
			}

			resolved = true;

			return this;
		}

		override public Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			if (right_side == EmptyExpression.OutAccess) {
				if (ec.CurrentBlock.Toplevel.GetParameterReference (PropertyInfo.Name, loc) is MemberAccess) {
					ec.Report.Error (1939, loc, "A range variable `{0}' may not be passes as `ref' or `out' parameter",
					    PropertyInfo.Name);
				} else {
					ec.Report.Error (206, loc, "A property or indexer `{0}' may not be passed as `ref' or `out' parameter",
					      GetSignatureForError ());
				}
				return null;
			}

			if (right_side == EmptyExpression.LValueMemberAccess || right_side == EmptyExpression.LValueMemberOutAccess) {
				Error_CannotModifyIntermediateExpressionValue (ec);
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

				if (ec.CurrentBlock.Toplevel.GetParameterReference (PropertyInfo.Name, loc) is MemberAccess) {
					ec.Report.Error (1947, loc, "A range variable `{0}' cannot be assigned to. Consider using `let' clause to store the value",
						PropertyInfo.Name);
				} else {
					ec.Report.Error (200, loc, "Property or indexer `{0}' cannot be assigned to (it is read only)",
						GetSignatureForError ());
				}
				return null;
			}

			if (TypeManager.GetParameterData (setter).Count != 1){
				Error_PropertyNotFound (ec, setter, false);
				return null;
			}

			bool must_do_cs1540_check;
			if (!IsAccessorAccessible (ec.CurrentType, setter, out must_do_cs1540_check)) {
				PropertyBase.PropertyMethod pm = TypeManager.GetMethod (setter) as PropertyBase.PropertyMethod;
				if (pm != null && pm.HasCustomAccessModifier) {
					ec.Report.SymbolRelatedToPreviousError (pm);
					ec.Report.Error (272, loc, "The property or indexer `{0}' cannot be used in this context because the set accessor is inaccessible",
						TypeManager.CSharpSignature (setter));
				}
				else {
					ec.Report.SymbolRelatedToPreviousError (setter);
					ErrorIsInaccesible (loc, TypeManager.CSharpSignature (setter), ec.Report);
				}
				return null;
			}
			
			if (!InstanceResolve (ec, TypeManager.IsStruct (PropertyInfo.DeclaringType), must_do_cs1540_check))
				return null;
			
			//
			// Only base will allow this invocation to happen.
			//
			if (IsBase && setter.IsAbstract){
				Error_CannotCallAbstractBase (ec, TypeManager.GetFullNameSignature (PropertyInfo));
			}

			if (PropertyInfo.PropertyType.IsPointer && !ec.IsUnsafe) {
				UnsafeError (ec, loc);
			}

			if (!ec.IsObsolete) {
				PropertyBase pb = TypeManager.GetProperty (PropertyInfo);
				if (pb != null) {
					pb.CheckObsoleteness (loc);
				} else {
					ObsoleteAttribute oa = AttributeTester.GetMemberObsoleteAttribute (PropertyInfo);
					if (oa != null)
						AttributeTester.Report_ObsoleteMessage (oa, GetSignatureForError (), loc, ec.Report);
				}
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
			if (IsSingleDimensionalArrayLength ()) {
				if (!prepared)
					EmitInstance (ec, false);
				ec.ig.Emit (OpCodes.Ldlen);
				ec.ig.Emit (OpCodes.Conv_I4);
				return;
			}

			Invocation.EmitCall (ec, IsBase, InstanceExpression, getter, null, loc, prepared, false);
			
			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				if (!is_static) {
					temp = new LocalTemporary (this.Type);
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

			if (prepare_for_load) {
				prepared = true;
				source.Emit (ec);
				
				if (leave_copy) {
					ec.ig.Emit (OpCodes.Dup);
					if (!is_static) {
						temp = new LocalTemporary (this.Type);
						temp.Store (ec);
					}
				}
			} else if (leave_copy) {
				source.Emit (ec);
				temp = new LocalTemporary (this.Type);
				temp.Store (ec);
				my_source = temp;
			}

			Arguments args = new Arguments (1);
			args.Add (new Argument (my_source));
			
			Invocation.EmitCall (ec, IsBase, InstanceExpression, setter, args, loc, false, prepared);
			
			if (temp != null) {
				temp.Emit (ec);
				temp.Release (ec);
			}
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
		
		public void Error_AssignmentEventOnly (ResolveContext ec)
		{
			ec.Report.Error (79, loc, "The event `{0}' can only appear on the left hand side of `+=' or `-=' operator",
				GetSignatureForError ());
		}

		public override MemberExpr ResolveMemberAccess (ResolveContext ec, Expression left, Location loc,
								SimpleName original)
		{
			//
			// If the event is local to this class, we transform ourselves into a FieldExpr
			//

			if (EventInfo.DeclaringType == ec.CurrentType ||
			    TypeManager.IsNestedChildOf(ec.CurrentType, EventInfo.DeclaringType)) {
				EventField mi = TypeManager.GetEventField (EventInfo);

				if (mi != null) {
					if (!ec.IsObsolete)
						mi.CheckObsoleteness (loc);

					if ((mi.ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0 && !ec.HasSet (ResolveContext.Options.CompoundAssignmentScope))
						Error_AssignmentEventOnly (ec);
					
					FieldExpr ml = new FieldExpr (mi.BackingField.FieldBuilder, loc);

					InstanceExpression = null;
				
					return ml.ResolveMemberAccess (ec, left, loc, original);
				}
			}

			if (left is This && !ec.HasSet (ResolveContext.Options.CompoundAssignmentScope))			
				Error_AssignmentEventOnly (ec);

			return base.ResolveMemberAccess (ec, left, loc, original);
		}

		bool InstanceResolve (ResolveContext ec, bool must_do_cs1540_check)
		{
			if (is_static) {
				InstanceExpression = null;
				return true;
			}

			if (InstanceExpression == null) {
				SimpleName.Error_ObjectRefRequired (ec, loc, GetSignatureForError ());
				return false;
			}

			InstanceExpression = InstanceExpression.DoResolve (ec);
			if (InstanceExpression == null)
				return false;

			if (IsBase && add_accessor.IsAbstract) {
				Error_CannotCallAbstractBase (ec, TypeManager.CSharpSignature(add_accessor));
				return false;
			}

			//
			// This is using the same mechanism as the CS1540 check in PropertyExpr.
			// However, in the Event case, we reported a CS0122 instead.
			//
			// TODO: Exact copy from PropertyExpr
			//
			if (must_do_cs1540_check && InstanceExpression != EmptyExpression.Null &&
			    !TypeManager.IsInstantiationOfSameGenericType (InstanceExpression.Type, ec.CurrentType) &&
			    !TypeManager.IsNestedChildOf (ec.CurrentType, InstanceExpression.Type) &&
			    !TypeManager.IsSubclassOf (InstanceExpression.Type, ec.CurrentType)) {
				ec.Report.SymbolRelatedToPreviousError (EventInfo);
				ErrorIsInaccesible (loc, TypeManager.CSharpSignature (EventInfo), ec.Report);
				return false;
			}

			return true;
		}

		public bool IsAccessibleFrom (Type invocation_type)
		{
			bool dummy;
			return IsAccessorAccessible (invocation_type, add_accessor, out dummy) &&
				IsAccessorAccessible (invocation_type, remove_accessor, out dummy);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			// contexts where an LValue is valid have already devolved to FieldExprs
			Error_CannotAssign (ec);
			return null;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			bool must_do_cs1540_check;
			if (!(IsAccessorAccessible (ec.CurrentType, add_accessor, out must_do_cs1540_check) &&
			      IsAccessorAccessible (ec.CurrentType, remove_accessor, out must_do_cs1540_check))) {
				ec.Report.SymbolRelatedToPreviousError (EventInfo);
				ErrorIsInaccesible (loc, TypeManager.CSharpSignature (EventInfo), ec.Report);
				return null;
			}

			if (!InstanceResolve (ec, must_do_cs1540_check))
				return null;

			if (!ec.HasSet (ResolveContext.Options.CompoundAssignmentScope)) {
				Error_CannotAssign (ec);
				return null;
			}

			if (!ec.IsObsolete) {
				EventField ev = TypeManager.GetEventField (EventInfo);
				if (ev != null) {
					ev.CheckObsoleteness (loc);
				} else {
					ObsoleteAttribute oa = AttributeTester.GetMemberObsoleteAttribute (EventInfo);
					if (oa != null)
						AttributeTester.Report_ObsoleteMessage (oa, GetSignatureForError (), loc, ec.Report);
				}
			}
			
			return this;
		}		

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
			//Error_CannotAssign ();
		}

		public void Error_CannotAssign (ResolveContext ec)
		{
			ec.Report.Error (70, loc,
				"The event `{0}' can only appear on the left hand side of += or -= when used outside of the type `{1}'",
				GetSignatureForError (), TypeManager.CSharpName (EventInfo.DeclaringType));
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpSignature (EventInfo);
		}

		public void EmitAddOrRemove (EmitContext ec, bool is_add, Expression source)
		{
			Arguments args = new Arguments (1);
			args.Add (new Argument (source));
			Invocation.EmitCall (ec, IsBase, InstanceExpression, is_add ? add_accessor : remove_accessor, args, loc);
		}
	}

	public class TemporaryVariable : VariableReference
	{
		LocalInfo li;

		public TemporaryVariable (Type type, Location loc)
		{
			this.type = type;
			this.loc = loc;
			eclass = ExprClass.Variable;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (li != null)
				return this;

			TypeExpr te = new TypeExpression (type, loc);
			li = ec.CurrentBlock.AddTemporaryVariable (te, loc);
			if (!li.Resolve (ec))
				return null;

			//
			// Don't capture temporary variables except when using
			// iterator redirection
			//
			if (ec.CurrentAnonymousMethod != null && ec.CurrentAnonymousMethod.IsIterator && ec.IsVariableCapturingRequired) {
				AnonymousMethodStorey storey = li.Block.Explicit.CreateAnonymousMethodStorey (ec);
				storey.CaptureLocalVariable (ec, li);
			}

			return this;
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			return DoResolve (ec);
		}
		
		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}

		public void EmitAssign (EmitContext ec, Expression source)
		{
			EmitAssign (ec, source, false, false);
		}

		public override HoistedVariable GetHoistedVariable (AnonymousExpression ae)
		{
			return li.HoistedVariableReference;
		}

		public override bool IsFixed {
			get { return true; }
		}

		public override bool IsRef {
			get { return false; }
		}

		public override string Name {
			get { throw new NotImplementedException (); }
		}

		public override void SetHasAddressTaken ()
		{
			throw new NotImplementedException ();
		}

		protected override ILocalVariable Variable {
			get { return li; }
		}

		public override VariableInfo VariableInfo {
			get { throw new NotImplementedException (); }
		}
	}

	/// 
	/// Handles `var' contextual keyword; var becomes a keyword only
	/// if no type called var exists in a variable scope
	/// 
	class VarExpr : SimpleName
	{
		// Used for error reporting only
		ArrayList initializer;

		public VarExpr (Location loc)
			: base ("var", loc)
		{
		}

		public ArrayList VariableInitializer {
			set {
				this.initializer = value;
			}
		}

		public bool InferType (ResolveContext ec, Expression right_side)
		{
			if (type != null)
				throw new InternalErrorException ("An implicitly typed local variable could not be redefined");
			
			type = right_side.Type;
			if (type == TypeManager.null_type || type == TypeManager.void_type || type == InternalType.AnonymousMethod || type == InternalType.MethodGroup) {
				ec.Report.Error (815, loc, "An implicitly typed local variable declaration cannot be initialized with `{0}'",
				              right_side.GetSignatureForError ());
				return false;
			}

			eclass = ExprClass.Variable;
			return true;
		}

		protected override void Error_TypeOrNamespaceNotFound (IMemberContext ec)
		{
			if (RootContext.Version < LanguageVersion.V_3)
				base.Error_TypeOrNamespaceNotFound (ec);
			else
				ec.Compiler.Report.Error (825, loc, "The contextual keyword `var' may only appear within a local variable declaration");
		}

		public override TypeExpr ResolveAsContextualType (IMemberContext rc, bool silent)
		{
			TypeExpr te = base.ResolveAsContextualType (rc, true);
			if (te != null)
				return te;

			if (RootContext.Version < LanguageVersion.V_3)
				rc.Compiler.Report.FeatureIsNotAvailable (loc, "implicitly typed local variable");

			if (initializer == null)
				return null;

			if (initializer.Count > 1) {
				Location loc_init = ((CSharpParser.VariableDeclaration) initializer[1]).Location;
				rc.Compiler.Report.Error (819, loc_init, "An implicitly typed local variable declaration cannot include multiple declarators");
				initializer = null;
				return null;
			}

			Expression variable_initializer = ((CSharpParser.VariableDeclaration) initializer[0]).expression_or_array_initializer;
			if (variable_initializer == null) {
				rc.Compiler.Report.Error (818, loc, "An implicitly typed local variable declarator must include an initializer");
				return null;
			}

			return null;
		}
	}
}	
