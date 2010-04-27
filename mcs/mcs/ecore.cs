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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using SLE = System.Linq.Expressions;
using System.Linq;

namespace Mono.CSharp {

	/// <remarks>
	///   The ExprClass class contains the is used to pass the 
	///   classification of an expression (value, variable, namespace,
	///   type, method group, property access, event access, indexer access,
	///   nothing).
	/// </remarks>
	public enum ExprClass : byte {
		Unresolved	= 0,
		
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

		// Set if this is resolving the first part of a MemberAccess.
		Intermediate		= 1 << 11,
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
		protected TypeSpec type;
		protected Location loc;
		
		public TypeSpec Type {
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

		public virtual string GetSignatureForError ()
		{
			return type.GetDefinition ().GetSignatureForError ();
		}

		public static bool IsMemberAccessible (TypeSpec invocation_type, MemberSpec mi, out bool must_do_cs1540_check)
		{
			var ma = mi.Modifiers & Modifiers.AccessibilityMask;

			must_do_cs1540_check = false; // by default we do not check for this

			if (ma == Modifiers.PUBLIC)
				return true;
		
			//
			// If only accessible to the current class or children
			//
			if (ma == Modifiers.PRIVATE)
				return invocation_type.MemberDefinition == mi.DeclaringType.MemberDefinition ||
					TypeManager.IsNestedChildOf (invocation_type, mi.DeclaringType);

			if ((ma & Modifiers.INTERNAL) != 0) {
				var b = TypeManager.IsThisOrFriendAssembly (invocation_type == InternalType.FakeInternalType ?
					 CodeGen.Assembly.Builder : invocation_type.Assembly, mi.DeclaringType.Assembly);
				if (b || ma == Modifiers.INTERNAL)
					return b;
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
		protected abstract Expression DoResolve (ResolveContext rc);

		public virtual Expression DoResolveLValue (ResolveContext rc, Expression right_side)
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
		public virtual TypeExpr ResolveAsTypeTerminal (IMemberContext ec , bool silent)
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
			}

			te.loc = loc;

			//
			// Obsolete checks cannot be done when resolving base context as they
			// require type dependecies to be set but we are just resolving them
			//
			if (!silent && !(ec is TypeContainer.BaseContext)) {
				ObsoleteAttribute obsolete_attr = te.Type.GetAttributeObsolete ();
				if (obsolete_attr != null && !ec.IsObsolete) {
					AttributeTester.Report_ObsoleteMessage (obsolete_attr, te.GetSignatureForError (), Location, ec.Compiler.Report);
				}
			}

			return te;
		}
	
		public static void ErrorIsInaccesible (Location loc, string name, Report Report)
		{
			Report.Error (122, loc, "`{0}' is inaccessible due to its protection level", name);
		}

		protected static void Error_CannotAccessProtected (ResolveContext ec, Location loc, MemberSpec m, TypeSpec qualifier, TypeSpec container)
		{
			ec.Report.Error (1540, loc, "Cannot access protected member `{0}' via a qualifier of type `{1}'."
				+ " The qualifier must be of type `{2}' or derived from it", 
				m.GetSignatureForError (),
				TypeManager.CSharpName (qualifier),
				TypeManager.CSharpName (container));

		}

		public void Error_ExpressionMustBeConstant (ResolveContext rc, Location loc, string e_name)
		{
			rc.Report.Error (133, loc, "The expression being assigned to `{0}' must be constant", e_name);
		}

		public void Error_ConstantCanBeInitializedWithNullOnly (ResolveContext rc, TypeSpec type, Location loc, string name)
		{
			rc.Report.Error (134, loc, "A constant `{0}' of reference type `{1}' can only be initialized with null",
				name, TypeManager.CSharpName (type));
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

		public virtual void Error_ValueCannotBeConverted (ResolveContext ec, Location loc, TypeSpec target, bool expl)
		{
			Error_ValueCannotBeConvertedCore (ec, loc, target, expl);
		}

		protected void Error_ValueCannotBeConvertedCore (ResolveContext ec, Location loc, TypeSpec target, bool expl)
		{
			// The error was already reported as CS1660
			if (type == InternalType.AnonymousMethod)
				return;

/*
			if (TypeManager.IsGenericParameter (Type) && TypeManager.IsGenericParameter (target) && type.Name == target.Name) {
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
			} else if (Type.MetaInfo.FullName == target.MetaInfo.FullName) {
				ec.Report.ExtraInformation (loc,
					String.Format (
					"The type `{0}' has two conflicting definitions, one comes from `{1}' and the other from `{2}' (in the previous ",
					Type.MetaInfo.FullName, Type.Assembly.FullName, target.Assembly.FullName));
			}
*/
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
				type.GetSignatureForError (), target.GetSignatureForError ());
		}

		public virtual void Error_VariableIsUsedBeforeItIsDeclared (Report Report, string name)
		{
			Report.Error (841, loc, "A local variable `{0}' cannot be used before it is declared", name);
		}

		public void Error_TypeArgumentsCannotBeUsed (Report report, Location loc, MemberSpec member, int arity)
		{
			// Better message for possible generic expressions
			if (member != null && (member.Kind & MemberKind.GenericMask) != 0) {
				report.SymbolRelatedToPreviousError (member);
				if (member is TypeSpec)
					member = ((TypeSpec) member).GetDefinition ();
				else
					member = ((MethodSpec) member).GetGenericMethodDefinition ();

				string name = member.Kind == MemberKind.Method ? "method" : "type";
				if (member.IsGeneric) {
					report.Error (305, loc, "Using the generic {0} `{1}' requires `{2}' type argument(s)",
						name, member.GetSignatureForError (), member.Arity.ToString ());
				} else {
					report.Error (308, loc, "The non-generic {0} `{1}' cannot be used with the type arguments",
						name, member.GetSignatureForError ());
				}
			} else {
				report.Error (307, loc, "The {0} `{1}' cannot be used with type arguments",
					ExprClassName, GetSignatureForError ());
			}
		}

		protected virtual void Error_TypeDoesNotContainDefinition (ResolveContext ec, TypeSpec type, string name)
		{
			Error_TypeDoesNotContainDefinition (ec, loc, type, name);
		}

		public static void Error_TypeDoesNotContainDefinition (ResolveContext ec, Location loc, TypeSpec type, string name)
		{
			ec.Report.SymbolRelatedToPreviousError (type);
			ec.Report.Error (117, loc, "`{0}' does not contain a definition for `{1}'",
				TypeManager.CSharpName (type), name);
		}

		protected static void Error_ValueAssignment (ResolveContext ec, Location loc)
		{
			ec.Report.Error (131, loc, "The left-hand side of an assignment must be a variable, a property or an indexer");
		}

		ResolveFlags ExprClassToResolveFlags {
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
			if (eclass != ExprClass.Unresolved)
				return this;

			Expression e;
			if (this is SimpleName) {
				e = ((SimpleName) this).DoResolve (ec, (flags & ResolveFlags.Intermediate) != 0);
			} else {
				e = DoResolve (ec);
			}

			if (e == null)
				return null;

			if ((flags & e.ExprClassToResolveFlags) == 0) {
				e.Error_UnexpectedKind (ec, flags, loc);
				return null;
			}

			if (e.type == null)
				throw new InternalErrorException ("Expression `{0}' didn't set its type in DoResolve", e.GetType ());

			return e;
		}

		/// <summary>
		///   Resolves an expression and performs semantic analysis on it.
		/// </summary>
		public Expression Resolve (ResolveContext rc)
		{
			return Resolve (rc, ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);
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
			bool out_access = right_side == EmptyExpression.OutAccess.Instance;

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

			if (e.eclass == ExprClass.Unresolved)
				throw new Exception ("Expression " + e + " ExprClass is Invalid after resolve");

			if ((e.type == null) && !(e is GenericTypeExpr))
				throw new Exception ("Expression " + e + " did not set its type after Resolve");

			return e;
		}

		public virtual void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			Attribute.Error_AttributeArgumentNotValid (rc, loc);
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
			ec.Emit (on_true ? OpCodes.Brtrue : OpCodes.Brfalse, target);
		}

		// Emit this expression for its side effects, not for its value.
		// The default implementation is to emit the value, and then throw it away.
		// Subclasses can provide more efficient implementations, but those MUST be equivalent
		public virtual void EmitSideEffect (EmitContext ec)
		{
			Emit (ec);
			ec.Emit (OpCodes.Pop);
		}

		/// <summary>
		///   Protected constructor.  Only derivate types should
		///   be able to be created
		/// </summary>

		protected Expression ()
		{
		}

		/// <summary>
		///   Returns a fully formed expression after a MemberLookup
		/// </summary>
		/// 
		public static Expression ExprClassFromMemberInfo (TypeSpec container_type, MemberSpec spec, Location loc)
		{
			if (spec is EventSpec)
				return new EventExpr ((EventSpec) spec, loc);
			if (spec is ConstSpec)
				return new ConstantExpr ((ConstSpec) spec, loc);
			if (spec is FieldSpec)
				return new FieldExpr ((FieldSpec) spec, loc);
			if (spec is PropertySpec)
				return new PropertyExpr (container_type, (PropertySpec) spec, loc);
			if (spec is TypeSpec)
				return new TypeExpression (((TypeSpec) spec), loc);

			return null;
		}

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

		public static Expression MemberLookup (CompilerContext ctx, TypeSpec container_type, TypeSpec queried_type, string name, int arity,
						       MemberKind mt, BindingRestriction bf, Location loc)
		{
			return MemberLookup (ctx, container_type, null, queried_type, name, arity, mt, bf, loc);
		}

		//
		// Lookup type `queried_type' for code in class `container_type' with a qualifier of
		// `qualifier_type' or null to lookup members in the current class.
		//
		public static Expression MemberLookup (CompilerContext ctx, TypeSpec container_type,
						       TypeSpec qualifier_type, TypeSpec queried_type,
						       string name, int arity, MemberKind mt,
						       BindingRestriction binding, Location loc)
		{
			var mi = TypeManager.MemberLookup (container_type, qualifier_type,
								     queried_type, mt, binding, name, arity, null);
			if (mi == null)
				return null;

			var first = mi [0];
			if (mi.Count > 1) {
				foreach (var mc in mi) {
					if (mc is MethodSpec)
						return new MethodGroupExpr (mi, queried_type, loc);
				}

				ctx.Report.SymbolRelatedToPreviousError (mi [1]);
				ctx.Report.SymbolRelatedToPreviousError (first);
				ctx.Report.Error (229, loc, "Ambiguity between `{0}' and `{1}'",
					first.GetSignatureForError (), mi [1].GetSignatureForError ());
			}

			if (first is MethodSpec)
				return new MethodGroupExpr (mi, queried_type, loc);

			return ExprClassFromMemberInfo (container_type, first, loc);
		}

		public static Expression MemberLookup (CompilerContext ctx, TypeSpec container_type, TypeSpec queried_type,
							   string name, int arity, BindingRestriction binding, Location loc)
		{
			return MemberLookup (ctx, container_type, null, queried_type, name, arity,
					     MemberKind.All, binding | BindingRestriction.AccessibleOnly, loc);
		}

		public static Expression MemberLookup (CompilerContext ctx, TypeSpec container_type, TypeSpec qualifier_type,
							   TypeSpec queried_type, string name, int arity, BindingRestriction binding, Location loc)
		{
			return MemberLookup (ctx, container_type, qualifier_type, queried_type,
						 name, arity, MemberKind.All, binding | BindingRestriction.AccessibleOnly, loc);
		}

		public static MethodGroupExpr MethodLookup (CompilerContext ctx, TypeSpec container_type, TypeSpec queried_type,
						       MemberKind kind, string name, int arity, Location loc)
		{
			return (MethodGroupExpr)MemberLookup (ctx, container_type, null, queried_type, name, arity,
					     kind, BindingRestriction.AccessibleOnly, loc);
		}

		/// <summary>
		///   This is a wrapper for MemberLookup that is not used to "probe", but
		///   to find a final definition.  If the final definition is not found, we
		///   look for private members and display a useful debugging message if we
		///   find it.
		/// </summary>
		protected Expression MemberLookupFinal (ResolveContext ec, TypeSpec qualifier_type,
							    TypeSpec queried_type, string name, int arity,
							    MemberKind mt, BindingRestriction bf,
							    Location loc)
		{
			Expression e;

			int errors = ec.Report.Errors;
			e = MemberLookup (ec.Compiler, ec.CurrentType, qualifier_type, queried_type, name, arity, mt, bf, loc);

			if (e != null || errors != ec.Report.Errors)
				return e;

			// No errors were reported by MemberLookup, but there was an error.
			return Error_MemberLookupFailed (ec, ec.CurrentType, qualifier_type, queried_type,
					name, arity, null, mt, bf);
		}

		protected virtual Expression Error_MemberLookupFailed (ResolveContext ec, TypeSpec container_type, TypeSpec qualifier_type,
						       TypeSpec queried_type, string name, int arity, string class_name,
							   MemberKind mt, BindingRestriction bf)
		{
			IList<MemberSpec> lookup = null;
			if (queried_type == null) {
				class_name = "global::";
			} else {
				BindingRestriction restriction = bf & BindingRestriction.DeclaredOnly;

				lookup = TypeManager.MemberLookup (queried_type, null, queried_type,
					mt, restriction, name, arity, null);

				if (lookup != null) {
					Expression e = Error_MemberLookupFailed (ec, queried_type, lookup);

					//
					// FIXME: This is still very wrong, it should be done inside
					// OverloadResolve to do correct arguments matching.
					// Requires MemberLookup accessiblity check removal
					//
					if (e == null || (mt & (MemberKind.Method | MemberKind.Constructor)) == 0) {
						var mi = lookup.First ();
						ec.Report.SymbolRelatedToPreviousError (mi);
						if ((mi.Modifiers & Modifiers.PROTECTED) != 0 && qualifier_type != null && container_type != null && qualifier_type != container_type &&
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
					MemberKind.All, BindingRestriction.None, name, -System.Math.Max (1, arity), null);
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

			var mge = Error_MemberLookupFailed (ec, queried_type, lookup);
			if (arity > 0 && mge != null) {
				mge.SetTypeArguments (ec, new TypeArguments (new FullNamedExpression [arity]));
			}

			return mge;
		}

		protected virtual MemberExpr Error_MemberLookupFailed (ResolveContext ec, TypeSpec type, IList<MemberSpec> members)
		{
			if (members.Any ((m) => !(m is MethodSpec)))
				return (MemberExpr) ExprClassFromMemberInfo (type, members.First (), loc);

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
		protected static Expression GetOperatorTrue (ResolveContext ec, Expression e, Location loc)
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
			operator_group = MethodLookup (ec.Compiler, ec.CurrentType, e.Type, MemberKind.Operator, mname, 0, loc) as MethodGroupExpr;
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

		public virtual string ExprClassName
		{
			get {
				switch (eclass){
				case ExprClass.Unresolved:
					return "Unresolved";
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
		// Returns the size of type `t' if known, otherwise, 0
		//
		public static int GetTypeSize (TypeSpec t)
		{
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
			if (source.type == InternalType.Dynamic) {
				Arguments args = new Arguments (1);
				args.Add (new Argument (source));
				return new DynamicConversion (TypeManager.int32_type, CSharpBinderFlags.ConvertArrayIndex, args, loc).Resolve (ec);
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
				TypeSpec t = TypeManager.CoreLookupType (ec.Compiler, "System.Linq.Expressions", "Expression", MemberKind.Class, true);
				if (t == null)
					return null;

				TypeManager.expression_type_expr = texpr = new TypeExpression (t, Location.Null);
			}

			return texpr;
		}

		//
		// Implemented by all expressions which support conversion from
		// compiler expression to invokable runtime expression. Used by
		// dynamic C# binder.
		//
		public virtual SLE.Expression MakeExpression (BuilderContext ctx)
		{
			throw new NotImplementedException ("MakeExpression for " + GetType ());
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

		protected TypeCast (Expression child, TypeSpec return_type)
		{
			eclass = child.eclass;
			loc = child.Location;
			type = return_type;
			this.child = child;
		}

		public Expression Child {
			get {
				return child;
			}
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

		protected override Expression DoResolve (ResolveContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);
		}

		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			return ctx.HasSet (BuilderContext.Options.CheckedScope) ?
				SLE.Expression.ConvertChecked (child.MakeExpression (ctx), type.GetMetaInfo ()) :
				SLE.Expression.Convert (child.MakeExpression (ctx), type.GetMetaInfo ());
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
		EmptyCast (Expression child, TypeSpec target_type)
			: base (child, target_type)
		{
		}

		public static Expression Create (Expression child, TypeSpec type)
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
		MethodSpec conversion_operator;
			
		public OperatorCast (Expression child, TypeSpec target_type) 
			: this (child, target_type, false)
		{
		}

		public OperatorCast (Expression child, TypeSpec target_type, bool find_explicit)
			: base (child, target_type)
		{
			conversion_operator = GetConversionOperator (find_explicit);
			if (conversion_operator == null)
				throw new InternalErrorException ("Outer conversion routine is out of sync");
		}

		// Returns the implicit operator that converts from
		// 'child.Type' to our target type (type)
		MethodSpec GetConversionOperator (bool find_explicit)
		{
			string operator_name = find_explicit ? "op_Explicit" : "op_Implicit";

			// Operators are always public
			var mi = TypeManager.MemberLookup (child.Type, child.Type, child.Type, MemberKind.Operator,
				BindingRestriction.None, operator_name, 0, null);

			if (mi == null){
				mi = TypeManager.MemberLookup (type, type, type, MemberKind.Operator,
					BindingRestriction.None, operator_name, 0, null);
			}
			
			foreach (MethodSpec oper in mi) {
				AParametersCollection pd = oper.Parameters;

				if (pd.Types [0] == child.Type && oper.ReturnType == type)
					return oper;
			}

			return null;
		}

		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);
			ec.Emit (OpCodes.Call, conversion_operator);
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
		static Dictionary<TypeSpec, MethodSpec> operators;

		public CastFromDecimal (Expression child, TypeSpec return_type)
			: base (child, return_type)
		{
			if (child.Type != TypeManager.decimal_type)
				throw new ArgumentException ("Expected decimal child " + child.Type.GetSignatureForError ());
		}

		// Returns the explicit operator that converts from an
		// express of type System.Decimal to 'type'.
		public Expression Resolve ()
		{
			if (operators == null) {
				var all_oper = TypeManager.MemberLookup (TypeManager.decimal_type,
				   TypeManager.decimal_type, TypeManager.decimal_type, MemberKind.Operator,
				   BindingRestriction.None, "op_Explicit", 0, null);

				operators = new Dictionary<TypeSpec, MethodSpec> ();
				foreach (MethodSpec oper in all_oper) {
					AParametersCollection pd = oper.Parameters;
					if (pd.Types [0] == TypeManager.decimal_type)
						operators.Add (oper.ReturnType, oper);
				}
			}

			return operators.ContainsKey (type) ? this : null;
		}

		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);

			ec.Emit (OpCodes.Call, operators [type]);
		}

		public static void Reset ()
		{
			operators = null;
		}
	}

	
	//
	// Constant specialization of EmptyCast.
	// We need to special case this since an empty cast of
	// a constant is still a constant. 
	//
	public class EmptyConstantCast : Constant
	{
		public Constant child;

		public EmptyConstantCast (Constant child, TypeSpec type)
			: base (child.Location)
		{
			if (child == null)
				throw new ArgumentNullException ("child");

			this.child = child;
			this.eclass = child.eclass;
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

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			if (child.Type == target_type)
				return child;

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

		public override bool IsDefaultValue {
			get { return child.IsDefaultValue; }
		}

		public override bool IsNegative {
			get { return child.IsNegative; }
		}

		public override bool IsNull {
			get { return child.IsNull; }
		}
		
		public override bool IsOneInteger {
			get { return child.IsOneInteger; }
		}

		public override bool IsZeroInteger {
			get { return child.IsZeroInteger; }
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			return this;
		}
		
		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);			
		}

		public override void EmitBranchable (EmitContext ec, Label label, bool on_true)
		{
			child.EmitBranchable (ec, label, on_true);

			// Only to make verifier happy
			if (TypeManager.IsGenericParameter (type) && child.IsNull)
				ec.Emit (OpCodes.Unbox_Any, type);
		}

		public override void EmitSideEffect (EmitContext ec)
		{
			child.EmitSideEffect (ec);
		}

		public override Constant ConvertImplicitly (ResolveContext rc, TypeSpec target_type)
		{
			// FIXME: Do we need to check user conversions?
			if (!Convert.ImplicitStandardConversionExists (this, target_type))
				return null;
			return child.ConvertImplicitly (rc, target_type);
		}
	}

	/// <summary>
	///  This class is used to wrap literals which belong inside Enums
	/// </summary>
	public class EnumConstant : Constant
	{
		public Constant Child;

		public EnumConstant (Constant child, TypeSpec enum_type)
			: base (child.Location)
		{
			this.Child = child;
			this.type = enum_type;
		}

		protected EnumConstant (Location loc)
			: base (loc)
		{
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			Child = Child.Resolve (rc);
			this.eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Child.Emit (ec);
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			Child.EncodeAttributeValue (rc, enc, Child.Type);
		}

		public override void EmitBranchable (EmitContext ec, Label label, bool on_true)
		{
			Child.EmitBranchable (ec, label, on_true);
		}

		public override void EmitSideEffect (EmitContext ec)
		{
			Child.EmitSideEffect (ec);
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
			if (type.MemberDefinition is TypeContainer)
				return Child.GetValue ();
#endif

			return System.Enum.ToObject (type.GetMetaInfo (), Child.GetValue ());
		}
		
		public override string AsString ()
		{
			return Child.AsString ();
		}

		public EnumConstant Increment()
		{
			return new EnumConstant (((IntegralConstant) Child).Increment (), type);
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

		public override Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type)
		{
			if (Child.Type == target_type)
				return Child;

			return Child.ConvertExplicitly (in_checked_context, target_type);
		}

		public override Constant ConvertImplicitly (ResolveContext rc, TypeSpec type)
		{
			if (this.type == type) {
				return this;
			}

			if (!Convert.ImplicitStandardConversionExists (this, type)){
				return null;
			}

			return Child.ConvertImplicitly (rc, type);
		}
	}

	/// <summary>
	///   This kind of cast is used to encapsulate Value Types in objects.
	///
	///   The effect of it is to box the value type emitted by the previous
	///   operation.
	/// </summary>
	public class BoxedCast : TypeCast {

		public BoxedCast (Expression expr, TypeSpec target_type)
			: base (expr, target_type)
		{
			eclass = ExprClass.Value;
		}
		
		protected override Expression DoResolve (ResolveContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode (child.Type);
			child.EncodeAttributeValue (rc, enc, child.Type);
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			
			ec.Emit (OpCodes.Box, child.Type);
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
		public UnboxCast (Expression expr, TypeSpec return_type)
			: base (expr, return_type)
		{
		}

		protected override Expression DoResolve (ResolveContext ec)
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

			ec.Emit (OpCodes.Unbox_Any, type);
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
		
		public ConvCast (Expression child, TypeSpec return_type, Mode m)
			: base (child, return_type)
		{
			mode = m;
		}

		protected override Expression DoResolve (ResolveContext ec)
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
			base.Emit (ec);

			if (ec.HasSet (EmitContext.Options.CheckedScope)) {
				switch (mode){
				case Mode.I1_U1: ec.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I1_U2: ec.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I1_U4: ec.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I1_U8: ec.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I1_CH: ec.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U1_I1: ec.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U1_CH: /* nothing */ break;

				case Mode.I2_I1: ec.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.I2_U1: ec.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I2_U2: ec.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I2_U4: ec.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I2_U8: ec.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I2_CH: ec.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U2_I1: ec.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U2_U1: ec.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U2_I2: ec.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U2_CH: /* nothing */ break;

				case Mode.I4_I1: ec.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.I4_U1: ec.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I4_I2: ec.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.I4_U4: ec.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I4_U2: ec.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I4_U8: ec.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I4_CH: ec.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U4_I1: ec.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U4_U1: ec.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U4_I2: ec.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U4_U2: ec.Emit (OpCodes.Conv_Ovf_U2_Un); break;
				case Mode.U4_I4: ec.Emit (OpCodes.Conv_Ovf_I4_Un); break;
				case Mode.U4_CH: ec.Emit (OpCodes.Conv_Ovf_U2_Un); break;

				case Mode.I8_I1: ec.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.I8_U1: ec.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I8_I2: ec.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.I8_U2: ec.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I8_I4: ec.Emit (OpCodes.Conv_Ovf_I4); break;
				case Mode.I8_U4: ec.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I8_U8: ec.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I8_CH: ec.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I8_I: ec.Emit (OpCodes.Conv_Ovf_U); break;

				case Mode.U8_I1: ec.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U8_U1: ec.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U8_I2: ec.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U8_U2: ec.Emit (OpCodes.Conv_Ovf_U2_Un); break;
				case Mode.U8_I4: ec.Emit (OpCodes.Conv_Ovf_I4_Un); break;
				case Mode.U8_U4: ec.Emit (OpCodes.Conv_Ovf_U4_Un); break;
				case Mode.U8_I8: ec.Emit (OpCodes.Conv_Ovf_I8_Un); break;
				case Mode.U8_CH: ec.Emit (OpCodes.Conv_Ovf_U2_Un); break;
				case Mode.U8_I: ec.Emit (OpCodes.Conv_Ovf_U_Un); break;

				case Mode.CH_I1: ec.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.CH_U1: ec.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.CH_I2: ec.Emit (OpCodes.Conv_Ovf_I2_Un); break;

				case Mode.R4_I1: ec.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.R4_U1: ec.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.R4_I2: ec.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.R4_U2: ec.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.R4_I4: ec.Emit (OpCodes.Conv_Ovf_I4); break;
				case Mode.R4_U4: ec.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.R4_I8: ec.Emit (OpCodes.Conv_Ovf_I8); break;
				case Mode.R4_U8: ec.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.R4_CH: ec.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.R8_I1: ec.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.R8_U1: ec.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.R8_I2: ec.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.R8_U2: ec.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.R8_I4: ec.Emit (OpCodes.Conv_Ovf_I4); break;
				case Mode.R8_U4: ec.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.R8_I8: ec.Emit (OpCodes.Conv_Ovf_I8); break;
				case Mode.R8_U8: ec.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.R8_CH: ec.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.R8_R4: ec.Emit (OpCodes.Conv_R4); break;

				case Mode.I_I8: ec.Emit (OpCodes.Conv_Ovf_I8_Un); break;
				}
			} else {
				switch (mode){
				case Mode.I1_U1: ec.Emit (OpCodes.Conv_U1); break;
				case Mode.I1_U2: ec.Emit (OpCodes.Conv_U2); break;
				case Mode.I1_U4: ec.Emit (OpCodes.Conv_U4); break;
				case Mode.I1_U8: ec.Emit (OpCodes.Conv_I8); break;
				case Mode.I1_CH: ec.Emit (OpCodes.Conv_U2); break;

				case Mode.U1_I1: ec.Emit (OpCodes.Conv_I1); break;
				case Mode.U1_CH: ec.Emit (OpCodes.Conv_U2); break;

				case Mode.I2_I1: ec.Emit (OpCodes.Conv_I1); break;
				case Mode.I2_U1: ec.Emit (OpCodes.Conv_U1); break;
				case Mode.I2_U2: ec.Emit (OpCodes.Conv_U2); break;
				case Mode.I2_U4: ec.Emit (OpCodes.Conv_U4); break;
				case Mode.I2_U8: ec.Emit (OpCodes.Conv_I8); break;
				case Mode.I2_CH: ec.Emit (OpCodes.Conv_U2); break;

				case Mode.U2_I1: ec.Emit (OpCodes.Conv_I1); break;
				case Mode.U2_U1: ec.Emit (OpCodes.Conv_U1); break;
				case Mode.U2_I2: ec.Emit (OpCodes.Conv_I2); break;
				case Mode.U2_CH: /* nothing */ break;

				case Mode.I4_I1: ec.Emit (OpCodes.Conv_I1); break;
				case Mode.I4_U1: ec.Emit (OpCodes.Conv_U1); break;
				case Mode.I4_I2: ec.Emit (OpCodes.Conv_I2); break;
				case Mode.I4_U4: /* nothing */ break;
				case Mode.I4_U2: ec.Emit (OpCodes.Conv_U2); break;
				case Mode.I4_U8: ec.Emit (OpCodes.Conv_I8); break;
				case Mode.I4_CH: ec.Emit (OpCodes.Conv_U2); break;

				case Mode.U4_I1: ec.Emit (OpCodes.Conv_I1); break;
				case Mode.U4_U1: ec.Emit (OpCodes.Conv_U1); break;
				case Mode.U4_I2: ec.Emit (OpCodes.Conv_I2); break;
				case Mode.U4_U2: ec.Emit (OpCodes.Conv_U2); break;
				case Mode.U4_I4: /* nothing */ break;
				case Mode.U4_CH: ec.Emit (OpCodes.Conv_U2); break;

				case Mode.I8_I1: ec.Emit (OpCodes.Conv_I1); break;
				case Mode.I8_U1: ec.Emit (OpCodes.Conv_U1); break;
				case Mode.I8_I2: ec.Emit (OpCodes.Conv_I2); break;
				case Mode.I8_U2: ec.Emit (OpCodes.Conv_U2); break;
				case Mode.I8_I4: ec.Emit (OpCodes.Conv_I4); break;
				case Mode.I8_U4: ec.Emit (OpCodes.Conv_U4); break;
				case Mode.I8_U8: /* nothing */ break;
				case Mode.I8_CH: ec.Emit (OpCodes.Conv_U2); break;
				case Mode.I8_I: ec.Emit (OpCodes.Conv_U); break;

				case Mode.U8_I1: ec.Emit (OpCodes.Conv_I1); break;
				case Mode.U8_U1: ec.Emit (OpCodes.Conv_U1); break;
				case Mode.U8_I2: ec.Emit (OpCodes.Conv_I2); break;
				case Mode.U8_U2: ec.Emit (OpCodes.Conv_U2); break;
				case Mode.U8_I4: ec.Emit (OpCodes.Conv_I4); break;
				case Mode.U8_U4: ec.Emit (OpCodes.Conv_U4); break;
				case Mode.U8_I8: /* nothing */ break;
				case Mode.U8_CH: ec.Emit (OpCodes.Conv_U2); break;
				case Mode.U8_I: ec.Emit (OpCodes.Conv_U); break;

				case Mode.CH_I1: ec.Emit (OpCodes.Conv_I1); break;
				case Mode.CH_U1: ec.Emit (OpCodes.Conv_U1); break;
				case Mode.CH_I2: ec.Emit (OpCodes.Conv_I2); break;

				case Mode.R4_I1: ec.Emit (OpCodes.Conv_I1); break;
				case Mode.R4_U1: ec.Emit (OpCodes.Conv_U1); break;
				case Mode.R4_I2: ec.Emit (OpCodes.Conv_I2); break;
				case Mode.R4_U2: ec.Emit (OpCodes.Conv_U2); break;
				case Mode.R4_I4: ec.Emit (OpCodes.Conv_I4); break;
				case Mode.R4_U4: ec.Emit (OpCodes.Conv_U4); break;
				case Mode.R4_I8: ec.Emit (OpCodes.Conv_I8); break;
				case Mode.R4_U8: ec.Emit (OpCodes.Conv_U8); break;
				case Mode.R4_CH: ec.Emit (OpCodes.Conv_U2); break;

				case Mode.R8_I1: ec.Emit (OpCodes.Conv_I1); break;
				case Mode.R8_U1: ec.Emit (OpCodes.Conv_U1); break;
				case Mode.R8_I2: ec.Emit (OpCodes.Conv_I2); break;
				case Mode.R8_U2: ec.Emit (OpCodes.Conv_U2); break;
				case Mode.R8_I4: ec.Emit (OpCodes.Conv_I4); break;
				case Mode.R8_U4: ec.Emit (OpCodes.Conv_U4); break;
				case Mode.R8_I8: ec.Emit (OpCodes.Conv_I8); break;
				case Mode.R8_U8: ec.Emit (OpCodes.Conv_U8); break;
				case Mode.R8_CH: ec.Emit (OpCodes.Conv_U2); break;
				case Mode.R8_R4: ec.Emit (OpCodes.Conv_R4); break;

				case Mode.I_I8: ec.Emit (OpCodes.Conv_U8); break;
				}
			}
		}
	}
	
	public class OpcodeCast : TypeCast {
		readonly OpCode op;
		
		public OpcodeCast (Expression child, TypeSpec return_type, OpCode op)
			: base (child, return_type)
		{
			this.op = op;
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			ec.Emit (op);
		}

		public TypeSpec UnderlyingType {
			get { return child.Type; }
		}
	}

	/// <summary>
	///   This kind of cast is used to encapsulate a child and cast it
	///   to the class requested
	/// </summary>
	public sealed class ClassCast : TypeCast {
		readonly bool forced;
		
		public ClassCast (Expression child, TypeSpec return_type)
			: base (child, return_type)
		{
		}
		
		public ClassCast (Expression child, TypeSpec return_type, bool forced)
			: base (child, return_type)
		{
			this.forced = forced;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);

			bool gen = TypeManager.IsGenericParameter (child.Type);
			if (gen)
				ec.Emit (OpCodes.Box, child.Type);
			
			if (type.IsGenericParameter) {
				ec.Emit (OpCodes.Unbox_Any, type);
				return;
			}
			
			if (gen && !forced)
				return;
			
			ec.Emit (OpCodes.Castclass, type);
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

			public override Constant ConvertImplicitly (ResolveContext rc, TypeSpec target_type)
			{
				Constant c = base.ConvertImplicitly (rc, target_type);
				if (c != null)
					c = new ReducedConstantExpression (c, orig_expr);

				return c;
			}

			public override Expression CreateExpressionTree (ResolveContext ec)
			{
				return orig_expr.CreateExpressionTree (ec);
			}

			public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
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

			protected override Expression DoResolve (ResolveContext ec)
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
		}

		readonly Expression expr, orig_expr;

		private ReducedExpression (Expression expr, Expression orig_expr)
		{
			this.expr = expr;
			this.eclass = expr.eclass;
			this.type = expr.Type;
			this.orig_expr = orig_expr;
			this.loc = orig_expr.Location;
		}

		//
		// Creates fully resolved expression switcher
		//
		public static Constant Create (Constant expr, Expression original_expr)
		{
			if (expr.eclass == ExprClass.Unresolved)
				throw new ArgumentException ("Unresolved expression");

			return new ReducedConstantExpression (expr, original_expr);
		}

		public static ExpressionStatement Create (ExpressionStatement s, Expression orig)
		{
			return new ReducedExpressionStatement (s, orig);
		}

		//
		// Creates unresolved reduce expression. The original expression has to be
		// already resolved
		//
		public static Expression Create (Expression expr, Expression original_expr)
		{
			Constant c = expr as Constant;
			if (c != null)
				return Create (c, original_expr);

			ExpressionStatement s = expr as ExpressionStatement;
			if (s != null)
				return Create (s, original_expr);

			if (expr.eclass == ExprClass.Unresolved)
				throw new ArgumentException ("Unresolved expression");

			return new ReducedExpression (expr, original_expr);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return orig_expr.CreateExpressionTree (ec);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
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

		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			return orig_expr.MakeExpression (ctx);
		}
	}

	//
	// Standard composite pattern
	//
	public abstract class CompositeExpression : Expression
	{
		Expression expr;

		protected CompositeExpression (Expression expr)
		{
			this.expr = expr;
			this.loc = expr.Location;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return expr.CreateExpressionTree (ec);
		}

		public Expression Child {
			get { return expr; }
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr != null) {
				type = expr.Type;
				eclass = expr.eclass;
			}

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			expr.Emit (ec);
		}

		public override bool IsNull {
			get { return expr.IsNull; }
		}
	}

	//
	// Base of expressions used only to narrow resolve flow
	//
	public abstract class ShimExpression : Expression
	{
		protected Expression expr;

		protected ShimExpression (Expression expr)
		{
			this.expr = expr;
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			if (expr == null)
				return;

			ShimExpression target = (ShimExpression) t;
			target.expr = expr.Clone (clonectx);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override void Emit (EmitContext ec)
		{
			throw new InternalErrorException ("Missing Resolve call");
		}

		public Expression Expr {
			get { return expr; }
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

		protected ATypeNameExpression (string name, int arity, Location l)
			: this (name, new UnboundTypeArguments (arity), l)
		{
		}

		#region Properties

		protected int Arity {
			get {
				return targs == null ? 0 : targs.Count;
			}
		}

		public bool HasTypeArguments {
			get {
				return targs != null && !targs.IsEmpty;
			}
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

		#endregion

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

		// TODO: Move it to MemberCore
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

		public override string GetSignatureForError ()
		{
			if (targs != null) {
				return Name + "<" + targs.GetSignatureForError () + ">";
			}

			return Name;
		}
	}
	
	/// <summary>
	///   SimpleName expressions are formed of a single word and only happen at the beginning 
	///   of a dotted-name.
	/// </summary>
	public class SimpleName : ATypeNameExpression
	{
		public SimpleName (string name, Location l)
			: base (name, l)
		{
		}

		public SimpleName (string name, TypeArguments args, Location l)
			: base (name, args, l)
		{
		}

		public SimpleName (string name, int arity, Location l)
			: base (name, arity, l)
		{
		}

		public SimpleName GetMethodGroup ()
		{
			return new SimpleName (Name, targs, loc);
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

		protected virtual void Error_TypeOrNamespaceNotFound (IMemberContext ec)
		{
			if (ec.CurrentType != null) {
				if (ec.CurrentMemberDefinition != null) {
					MemberCore mc = ec.CurrentMemberDefinition.Parent.GetDefinition (Name);
					if (mc != null) {
						Error_UnexpectedKind (ec.Compiler.Report, mc, "type", GetMemberType (mc), loc);
						return;
					}
				}

				/*
								// TODO MemberCache: Implement
 
								string ns = ec.CurrentType.Namespace;
								string fullname = (ns.Length > 0) ? ns + "." + Name : Name;
								foreach (Assembly a in GlobalRootNamespace.Instance.Assemblies) {
									var type = a.GetType (fullname);
									if (type != null) {
										ec.Compiler.Report.SymbolRelatedToPreviousError (type);
										Expression.ErrorIsInaccesible (loc, TypeManager.CSharpName (type), ec.Compiler.Report);
										return;
									}
								}

								if (ec.CurrentTypeDefinition != null) {
									TypeSpec t = ec.CurrentTypeDefinition.LookupAnyGeneric (Name);
									if (t != null) {
										Namespace.Error_InvalidNumberOfTypeArguments (ec.Compiler.Report, t, loc);
										return;
									}
								}
				*/
			}

			FullNamedExpression retval = ec.LookupNamespaceOrType (Name, -System.Math.Max (1, Arity), loc, true);
			if (retval != null) {
				Error_TypeArgumentsCannotBeUsed (ec.Compiler.Report, loc, retval.Type, Arity);
/*
				var te = retval as TypeExpr;
				if (HasTypeArguments && te != null && !te.Type.IsGeneric)
					retval.Error_TypeArgumentsCannotBeUsed (ec.Compiler.Report, loc);
				else
					Namespace.Error_InvalidNumberOfTypeArguments (ec.Compiler.Report, retval.Type, loc);
*/
				return;
			}

			NamespaceEntry.Error_NamespaceNotFound (loc, Name, ec.Compiler.Report);
		}

		public bool IdenticalNameAndTypeName (IMemberContext mc, Expression resolved_to, Location loc)
		{
			if (resolved_to == null || resolved_to.Type == null)
				return false;

			if (resolved_to.Type is ElementTypeSpec || resolved_to.Type is InternalType)
				return false;

			return resolved_to.Type.Name == Name &&
				(mc.LookupNamespaceOrType (Name, Arity, loc, /* ignore_cs0104 = */ true) != null);
		}

		protected override Expression DoResolve (ResolveContext ec)
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

		static bool IsNestedChild (TypeSpec t, TypeSpec parent)
		{
			while (parent != null) {
				if (TypeManager.IsNestedChildOf (t, parent))
					return true;

				parent = parent.BaseType;
			}

			return false;
		}

		public override FullNamedExpression ResolveAsTypeStep (IMemberContext ec, bool silent)
		{
			int errors = ec.Compiler.Report.Errors;
			FullNamedExpression fne = ec.LookupNamespaceOrType (Name, Arity, loc, /*ignore_cs0104=*/ false);

			if (fne != null) {
				if (HasTypeArguments && fne.Type != null && TypeManager.IsGenericType (fne.Type)) {
					GenericTypeExpr ct = new GenericTypeExpr (fne.Type, targs, loc);
					return ct.ResolveAsTypeStep (ec, false);
				}

				return fne;
			}

			if (!HasTypeArguments && Name == "dynamic" &&
				RootContext.Version > LanguageVersion.V_3 &&
				RootContext.MetadataCompatibilityVersion > MetadataVersion.v2) {

				if (!PredefinedAttributes.Get.Dynamic.IsDefined) {
					ec.Compiler.Report.Error (1980, Location,
						"Dynamic keyword requires `{0}' to be defined. Are you missing System.Core.dll assembly reference?",
						PredefinedAttributes.Get.Dynamic.GetSignatureForError ());
				}

				return new DynamicTypeExpr (loc);
			}

			if (silent || errors != ec.Compiler.Report.Errors)
				return null;

			Error_TypeOrNamespaceNotFound (ec);
			return null;
		}

		Expression SimpleNameResolve (ResolveContext ec, Expression right_side, bool intermediate)
		{
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
					e = new LocalVariableReference (ec.CurrentBlock, Name, loc);

					if (right_side != null) {
						e = e.ResolveLValue (ec, right_side);
					} else {
						if (intermediate) {
							using (ec.With (ResolveContext.Options.DoFlowAnalysis, false)) {
								e = e.Resolve (ec, ResolveFlags.VariableOrValue);
							}
						} else {
							e = e.Resolve (ec, ResolveFlags.VariableOrValue);
						}
					}

					if (HasTypeArguments && e != null)
						e.Error_TypeArgumentsCannotBeUsed (ec.Report, loc, null, 0);

					return e;
				}

				e = current_block.Toplevel.GetParameterReference (Name, loc);
				if (e != null) {
					if (right_side != null)
						e = e.ResolveLValue (ec, right_side);
					else
						e = e.Resolve (ec);

					if (HasTypeArguments && e != null)
						e.Error_TypeArgumentsCannotBeUsed (ec.Report, loc, null, 0);

					return e;
				}
			}
			
			//
			// Stage 2: Lookup members 
			//
			int arity = HasTypeArguments ? Arity : -1;
//			TypeSpec almost_matched_type = null;
//			IList<MemberSpec> almost_matched = null;
			for (TypeSpec lookup_ds = ec.CurrentType; lookup_ds != null; lookup_ds = lookup_ds.DeclaringType) {
				e = MemberLookup (ec.Compiler, ec.CurrentType, lookup_ds, Name, arity, BindingRestriction.NoOverrides, loc);
				if (e != null) {
					PropertyExpr pe = e as PropertyExpr;
					if (pe != null) {
						// since TypeManager.MemberLookup doesn't know if we're doing a lvalue access or not,
						// it doesn't know which accessor to check permissions against
						if (pe.PropertyInfo.Kind == MemberKind.Property && pe.IsAccessibleFrom (ec.CurrentType, right_side != null))
							break;
					} else if (e is EventExpr) {
						if (((EventExpr) e).IsAccessibleFrom (ec.CurrentType))
							break;
					} else if (HasTypeArguments && e is TypeExpression) {
						e = new GenericTypeExpr (e.Type, targs, loc).ResolveAsTypeStep (ec, false);
						break;
					} else {
						break;
					}
					e = null;
				}
/*
				if (almost_matched == null && almost_matched_members.Count > 0) {
					almost_matched_type = lookup_ds;
					almost_matched = new List<MemberSpec>(almost_matched_members);
				}
*/ 
			}

			if (e == null) {
/*
				if (almost_matched == null && almost_matched_members.Count > 0) {
					almost_matched_type = ec.CurrentType;
					almost_matched = new List<MemberSpec> (almost_matched_members);
				}
*/ 
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
						return new FieldExpr (Import.CreateField (fi, null), loc).Resolve (ec);
				}
/*
				if (almost_matched != null)
					almost_matched_members = almost_matched;
				if (almost_matched_type == null)
					almost_matched_type = ec.CurrentType;
*/
				string type_name = ec.MemberContext.CurrentType == null ? null : ec.MemberContext.CurrentType.Name;
				return Error_MemberLookupFailed (ec, ec.CurrentType, null, ec.CurrentType, Name, arity,
					type_name, MemberKind.All, BindingRestriction.AccessibleOnly);
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
/*
						if (!me.IsStatic &&
						    (!intermediate || !IdenticalNameAndTypeName (ec, me, loc))) {
							Error_ObjectRefRequired (ec, loc, me.GetSignatureForError ());
							return null;
						}
*/
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

				if (HasTypeArguments) {
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
					: me.Resolve (ec);
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

		protected override Expression DoResolve (ResolveContext ec)
		{
			return ResolveAsTypeTerminal (ec, false);
		}

		public virtual bool CheckAccessLevel (IMemberContext mc)
		{
			DeclSpace c = mc.CurrentMemberDefinition as DeclSpace;
			if (c == null)
				c = mc.CurrentMemberDefinition.Parent;

			return c.CheckAccessLevel (Type);
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
	}

	/// <summary>
	///   Fully resolved Expression that already evaluated to a type
	/// </summary>
	public class TypeExpression : TypeExpr {
		public TypeExpression (TypeSpec t, Location l)
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
		
		public TypeLookupExpression (TypeSpec type)
		{
			eclass = ExprClass.Type;
			this.type = type;
		}

		protected override TypeExpr DoResolveAsTypeStep (IMemberContext ec)
		{
			return this;
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
		public abstract TypeSpec DeclaringType {
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

		public virtual MemberExpr ResolveMemberAccess (ResolveContext ec, Expression left, Location loc,
							       SimpleName original)
		{
			//
			// Precondition:
			//   original == null || original.Resolve (...) ==> left
			//

			if (left is TypeExpr) {
				left = ((TypeExpr) left).ResolveAsTypeTerminal (ec, false);
				if (left == null)
					return null;

				// TODO: Same problem as in class.cs, TypeTerminal does not
				// always do all necessary checks
				ObsoleteAttribute oa = left.Type.GetAttributeObsolete ();
				if (oa != null && !ec.IsObsolete) {
					AttributeTester.Report_ObsoleteMessage (oa, left.GetSignatureForError (), loc, ec.Report);
				}

//				GenericTypeExpr ct = left as GenericTypeExpr;
//				if (ct != null && !ct.CheckConstraints (ec))
//					return null;
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

			if (TypeManager.IsValueType (InstanceExpression.Type)) {
				if (InstanceExpression is IMemoryLocation) {
					((IMemoryLocation) InstanceExpression).AddressOf (ec, AddressOp.LoadStore);
				} else {
					LocalTemporary t = new LocalTemporary (InstanceExpression.Type);
					InstanceExpression.Emit (ec);
					t.Store (ec);
					t.AddressOf (ec, AddressOp.Store);
				}
			} else
				InstanceExpression.Emit (ec);

			if (prepare_for_load)
				ec.Emit (OpCodes.Dup);
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

		public ExtensionMethodGroupExpr (List<MethodSpec> list, NamespaceEntry n, TypeSpec extensionType, Location l)
			: base (list.Cast<MemberSpec>().ToList (), extensionType, l)
		{
			this.namespace_entry = n;
		}

		public override bool IsStatic {
			get { return true; }
		}

		public bool IsTopLevel {
			get { return namespace_entry == null; }
		}

		public override MethodGroupExpr OverloadResolve (ResolveContext ec, ref Arguments arguments, bool may_fail, Location loc)
		{
			if (arguments == null)
				arguments = new Arguments (1);

			arguments.Insert (0, new Argument (ExtensionExpression));
			MethodGroupExpr mg = ResolveOverloadExtensions (ec, ref arguments, namespace_entry, loc);

			// Store resolved argument and restore original arguments
			if (mg == null)
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
			int arity = type_arguments == null ? -1 : type_arguments.Count;
			ExtensionMethodGroupExpr e = ns.LookupExtensionMethod (type, Name, arity, loc);
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
			bool AmbiguousCall (ResolveContext ec, MethodSpec ambiguous);
			bool NoExactMatch (ResolveContext ec, MethodSpec method);
		}

		public IErrorHandler CustomErrorHandler;
		public IList<MemberSpec> Methods;
		MethodSpec best_candidate;
		// TODO: make private
		public TypeArguments type_arguments;
 		bool identical_type_name;
		bool has_inaccessible_candidates_only;
		TypeSpec delegate_type;
		TypeSpec queried_type;

		public MethodGroupExpr (IList<MemberSpec> mi, TypeSpec type, Location l)
			: this (type, l)
		{
			Methods = mi;
		}

		public MethodGroupExpr (MethodSpec m, TypeSpec type, Location l)
			: this (type, l)
		{
			Methods = new List<MemberSpec> (1) { m };
		}

		public MethodGroupExpr (IList<MemberSpec> mi, TypeSpec type, Location l, bool inacessibleCandidatesOnly)
			: this (mi, type, l)
		{
			has_inaccessible_candidates_only = inacessibleCandidatesOnly;
		}
		
		protected MethodGroupExpr (TypeSpec type, Location loc)
		{
			this.loc = loc;
			eclass = ExprClass.MethodGroup;
			this.type = InternalType.MethodGroup;
			queried_type = type;
		}

		public override TypeSpec DeclaringType {
			get {
				return queried_type;
			}
		}

		public MethodSpec BestCandidate {
			get {
				return best_candidate;
			}
		}

		public TypeSpec DelegateType {
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
				return best_candidate.GetSignatureForError ();

			return Methods.First ().GetSignatureForError ();
		}

		public override string Name {
			get {
				return Methods.First ().Name;
			}
		}

		public override bool IsInstance {
			get {
				if (best_candidate != null)
					return !best_candidate.IsStatic;

				foreach (var mb in Methods)
					if (!mb.IsStatic)
						return true;

				return false;
			}
		}

		public override bool IsStatic {
			get {
				if (best_candidate != null)
					return best_candidate.IsStatic;

				foreach (var mb in Methods)
					if (mb.IsStatic)
						return true;

				return false;
			}
		}

		public static explicit operator MethodSpec (MethodGroupExpr mg)
		{
			return mg.best_candidate;
		}

		//
		//  7.4.3.3  Better conversion from expression
		//  Returns :   1    if a->p is better,
		//              2    if a->q is better,
		//              0 if neither is better
		//
		static int BetterExpressionConversion (ResolveContext ec, Argument a, TypeSpec p, TypeSpec q)
		{
			TypeSpec argument_type = a.Type;
			if (argument_type == InternalType.AnonymousMethod && RootContext.Version > LanguageVersion.ISO_2) {
				//
				// Uwrap delegate from Expression<T>
				//
				if (p.GetDefinition () == TypeManager.expression_type) {
					p = TypeManager.GetTypeArguments (p) [0];
				}
				if (q.GetDefinition () == TypeManager.expression_type) {
					q = TypeManager.GetTypeArguments (q) [0];
				}
				
				p = Delegate.GetInvokeMethod (ec.Compiler, p).ReturnType;
				q = Delegate.GetInvokeMethod (ec.Compiler, q).ReturnType;
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
		public static int BetterTypeConversion (ResolveContext ec, TypeSpec p, TypeSpec q)
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
			} else if (p == InternalType.Dynamic) {
				if (q == TypeManager.object_type)
					return 2;
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
			} else if (q == InternalType.Dynamic) {
				if (p == TypeManager.object_type)
					return 1;
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
			MethodSpec candidate, bool candidate_params,
			MethodSpec best, bool best_params)
		{
			AParametersCollection candidate_pd = candidate.Parameters;
			AParametersCollection best_pd = best.Parameters;
		
			bool better_at_least_one = false;
			bool same = true;
			for (int j = 0, c_idx = 0, b_idx = 0; j < argument_count; ++j, ++c_idx, ++b_idx) 
			{
				Argument a = args [j];

				// Provided default argument value is never better
				if (a.IsDefaultArgument && candidate_params == best_params)
					return false;

				TypeSpec ct = candidate_pd.Types [c_idx];
				TypeSpec bt = best_pd.Types [b_idx];

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
				
				if (TypeManager.IsEqual (ct, bt))
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
			if (best.IsGeneric) {
				if (!candidate.IsGeneric)
					return true;
			} else if (candidate.IsGeneric) {
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
			// Both methods have the same number of parameters, and the parameters have equal types
			// Pick the "more specific" signature using rules over original (non-inflated) types
			//
			var candidate_def_pd = ((IParametersMember) candidate.MemberDefinition).Parameters;
			var best_def_pd = ((IParametersMember) best.MemberDefinition).Parameters;

			bool specific_at_least_once = false;
			for (int j = 0; j < candidate_param_count; ++j) 
			{
				var ct = candidate_def_pd.Types [j];
				var bt = best_def_pd.Types [j];
				if (ct == bt)
					continue;
				TypeSpec specific = MoreSpecific (ct, bt);
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

			if (best_candidate.IsConditionallyExcluded (loc))
				ec.Report.Error (765, loc,
					"Partial methods with only a defining declaration or removed conditional methods cannot be used in an expression tree");
			
			return new TypeOfMethod (best_candidate, loc);
		}
		
		protected override Expression DoResolve (ResolveContext ec)
		{
			this.eclass = ExprClass.MethodGroup;

			if (InstanceExpression != null) {
				InstanceExpression = InstanceExpression.Resolve (ec);
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

		void Error_AmbiguousCall (ResolveContext ec, MethodSpec ambiguous)
		{
			if (CustomErrorHandler != null && CustomErrorHandler.AmbiguousCall (ec, ambiguous))
				return;

			ec.Report.SymbolRelatedToPreviousError (best_candidate);
			ec.Report.Error (121, loc, "The call is ambiguous between the following methods or properties: `{0}' and `{1}'",
				best_candidate.GetSignatureForError (), ambiguous.GetSignatureForError ());
		}

		protected virtual void Error_InvalidArguments (ResolveContext ec, Location loc, int idx, MethodSpec method,
													Argument a, AParametersCollection expected_par, TypeSpec paramType)
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

		public override void Error_ValueCannotBeConverted (ResolveContext ec, Location loc, TypeSpec target, bool expl)
		{
			ec.Report.Error (428, loc, "Cannot convert method group `{0}' to non-delegate type `{1}'. Consider using parentheses to invoke the method",
				Name, TypeManager.CSharpName (target));
		}

		void Error_ArgumentCountWrong (ResolveContext ec, int arg_count)
		{
			ec.Report.Error (1501, loc, "No overload for method `{0}' takes `{1}' arguments",
				      Name, arg_count.ToString ());
		}
		
		protected virtual int GetApplicableParametersCount (MethodSpec method, AParametersCollection parameters)
		{
			return parameters.Count;
		}

		protected virtual IList<MemberSpec> GetBaseTypeMethods (ResolveContext rc, TypeSpec type)
		{
			return TypeManager.MemberLookup (rc.CurrentType, null, type,
				MemberKind.Method, BindingRestriction.AccessibleOnly | BindingRestriction.NoOverrides,
				Name, 0, null);	// TODO MemberCache: Arity !
		}

		bool GetBaseTypeMethods (ResolveContext rc)
		{
			var base_type = Methods.First ().DeclaringType.BaseType;
			if (base_type == null)
				return false;

			var methods = GetBaseTypeMethods (rc, base_type);
			if (methods == null)
				return false;

			Methods = methods;
			return true;
		}

		///
		/// Determines if the candidate method is applicable (section 14.4.2.1)
		/// to the given set of arguments
		/// A return value rates candidate method compatibility,
		/// 0 = the best, int.MaxValue = the worst
		///
		public int IsApplicable (ResolveContext ec,
						ref Arguments arguments, int arg_count, ref MethodSpec method, ref bool params_expanded_form)
		{
			var candidate = method;

			AParametersCollection pd = candidate.Parameters;
			int param_count = GetApplicableParametersCount (candidate, pd);
			int optional_count = 0;

			if (arg_count != param_count) {
				for (int i = 0; i < pd.Count; ++i) {
					if (pd.FixedParameters [i].HasDefaultValue) {
						optional_count = pd.Count - i;
						break;
					}
				}

				int args_gap = System.Math.Abs (arg_count - param_count);
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

							int index = pd.GetParameterIndexByName (na.Name);

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

			//
			// 1. Handle generic method using type arguments when specified or type inference
			//
			if (candidate.IsGeneric) {
				if (type_arguments != null) {
					var g_args_count = candidate.Arity;
					if (g_args_count != type_arguments.Count)
						return int.MaxValue - 20000 + System.Math.Abs (type_arguments.Count - g_args_count);

					method = candidate.MakeGenericMethod (type_arguments.Arguments);
					candidate = method;
					pd = candidate.Parameters;
				} else {
					int score = TypeManager.InferTypeArguments (ec, arguments, ref candidate);
					if (score != 0)
						return score - 20000;

					pd = candidate.Parameters;
				}
			} else {
				if (type_arguments != null)
					return int.MaxValue - 15000;
			}

			//
			// 2. Each argument has to be implicitly convertible to method parameter
			//
			method = candidate;
			Parameter.Modifier p_mod = 0;
			TypeSpec pt = null;
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

		int IsArgumentCompatible (ResolveContext ec, Parameter.Modifier arg_mod, Argument argument, Parameter.Modifier param_mod, TypeSpec parameter)
		{
			//
			// Types have to be identical when ref or out modifer is used 
			//
			if (arg_mod != 0 || param_mod != 0) {
				if (TypeManager.HasElementType (parameter))
					parameter = TypeManager.GetElementType (parameter);

				TypeSpec a_type = argument.Type;
				if (TypeManager.HasElementType (a_type))
					a_type = TypeManager.GetElementType (a_type);

				if (a_type != parameter) {
					if (a_type == InternalType.Dynamic)
						return 0;

					return 2;
				}
			} else {
				if (!Convert.ImplicitConversionExists (ec, argument.Expr, parameter)) {
					if (argument.Type == InternalType.Dynamic)
						return 0;

					return 2;
				}
			}

			if (arg_mod != param_mod)
				return 1;

			return 0;
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

			var all = new List<MemberSpec> (mg1.Methods);
			foreach (MethodSpec m in mg2.Methods){
				if (!TypeManager.ArrayContainsMethod (all, m, false))
					all.Add (m);
			}

			return new MethodGroupExpr (all, null, loc);
		}		

		static TypeSpec MoreSpecific (TypeSpec p, TypeSpec q)
		{
			if (TypeManager.IsGenericParameter (p) && !TypeManager.IsGenericParameter (q))
				return q;
			if (!TypeManager.IsGenericParameter (p) && TypeManager.IsGenericParameter (q))
				return p;

			var ac_p = p as ArrayContainer;
			if (ac_p != null) {
				var ac_q = ((ArrayContainer) q);
				TypeSpec specific = MoreSpecific (ac_p.Element, (ac_q.Element));
				if (specific == ac_p.Element)
					return p;
				if (specific == ac_q.Element)
					return q;
			} else if (TypeManager.IsGenericType (p)) {
				var pargs = TypeManager.GetTypeArguments (p);
				var qargs = TypeManager.GetTypeArguments (q);

				bool p_specific_at_least_once = false;
				bool q_specific_at_least_once = false;

				for (int i = 0; i < pargs.Length; i++) {
					TypeSpec specific = MoreSpecific (pargs[i], qargs[i]);
					if (specific == pargs[i])
						p_specific_at_least_once = true;
					if (specific == qargs[i])
						q_specific_at_least_once = true;
				}

				if (p_specific_at_least_once && !q_specific_at_least_once)
					return p;
				if (!p_specific_at_least_once && q_specific_at_least_once)
					return q;
			}

			return null;
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
			var candidates = new List<MethodSpec> (2);
			List<MethodSpec> params_candidates = null;

			int arg_count = Arguments != null ? Arguments.Count : 0;
			Dictionary<MethodSpec, Arguments> candidates_expanded = null;
			Arguments candidate_args = Arguments;

			if (RootContext.Version == LanguageVersion.ISO_1 && Name == "Invoke" && TypeManager.IsDelegateType (DeclaringType)) {
				if (!may_fail)
					ec.Report.Error (1533, loc, "Invoke cannot be called directly on a delegate");
				return null;
			}

			//
			// Enable message recording, it's used mainly by lambda expressions
			//
			var msg_recorder = new SessionReportPrinter ();
			var prev_recorder = ec.Report.SetPrinter (msg_recorder);

			do {
				//
				// Methods in a base class are not candidates if any method in a derived
				// class is applicable
				//
				int best_candidate_rate = int.MaxValue;

				foreach (var member in Methods) {
					var m = member as MethodSpec;
					if (m == null) {
						// TODO: It's wrong when non-member is before applicable method
						// TODO: Should report only when at least 1 from the batch is applicable
						if (candidates.Count != 0) {
							ec.Report.SymbolRelatedToPreviousError (candidates [0]);
							ec.Report.SymbolRelatedToPreviousError (member);
							ec.Report.Warning (467, 2, loc, "Ambiguity between method `{0}' and non-method `{1}'. Using method `{0}'",
								candidates[0].GetSignatureForError (), member.GetSignatureForError ());
						}
						continue;
					}

					//
					// Check if candidate is applicable (section 14.4.2.1)
					//
					bool params_expanded_form = false;
					int candidate_rate = IsApplicable (ec, ref candidate_args, arg_count, ref m, ref params_expanded_form);

					if (candidate_rate < best_candidate_rate) {
						best_candidate_rate = candidate_rate;
						best_candidate = m;
					}

					if (params_expanded_form) {
						if (params_candidates == null)
							params_candidates = new List<MethodSpec> (2);
						params_candidates.Add (m);
					}

					if (candidate_args != Arguments) {
						if (candidates_expanded == null)
							candidates_expanded = new Dictionary<MethodSpec, Arguments> (2);

						candidates_expanded.Add (m, candidate_args);
						candidate_args = Arguments;
					}

					if (candidate_rate != 0 || has_inaccessible_candidates_only) {
						if (msg_recorder != null)
							msg_recorder.EndSession ();
						continue;
					}

					msg_recorder = null;
					candidates.Add (m);
				}
			} while (candidates.Count == 0 && GetBaseTypeMethods (ec));

			ec.Report.SetPrinter (prev_recorder);
			if (msg_recorder != null && !msg_recorder.IsEmpty) {
				if (!may_fail)
					msg_recorder.Merge (prev_recorder);

				return null;
			}

			int candidate_top = candidates.Count;
			if (candidate_top == 0) {
				//
				// When we found a top level method which does not match and it's 
				// not an extension method. We start extension methods lookup from here
				//
				if (InstanceExpression != null) {
					var first = Methods.First ();
					var arity = type_arguments == null ? -1 : type_arguments.Count;
					ExtensionMethodGroupExpr ex_method_lookup = ec.LookupExtensionMethod (type, first.Name, arity, loc);
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

					bool params_expanded = params_candidates != null && params_candidates.Contains (best_candidate);
					if (NoExactMatch (ec, ref Arguments, params_expanded))
						return null;
				}

				//
				// We failed to find any method with correct argument count
				//
				if (Methods.First ().Kind == MemberKind.Constructor) {
					ec.Report.SymbolRelatedToPreviousError (queried_type);
					ec.Report.Error (1729, loc,
						"The type `{0}' does not contain a constructor that takes `{1}' arguments",
						TypeManager.CSharpName (queried_type), arg_count.ToString ());
				} else {
					Error_ArgumentCountWrong (ec, arg_count);
				}
                                
				return null;
			}

			if (arg_count != 0 && Arguments.HasDynamic) {
				best_candidate = null;
				return this;
			}

			//
			// Now we actually find the best method
			//
			best_candidate = candidates [0];
			bool method_params = params_candidates != null && params_candidates.Contains (best_candidate);

			for (int ix = 1; ix < candidate_top; ix++) {
				var candidate = candidates [ix];

				if (candidate == best_candidate)
					continue;

				bool cand_params = params_candidates != null && params_candidates.Contains (candidate);

				if (candidates_expanded != null && candidates_expanded.ContainsKey (candidate)) {
					candidate_args = candidates_expanded[candidate];
					arg_count = candidate_args.Count;
				}

				if (BetterFunction (ec, candidate_args, arg_count, 
					candidate, cand_params,
					best_candidate, method_params)) {
					best_candidate = candidate;
					method_params = cand_params;
				}

				if (candidate_args != Arguments) {
					candidate_args = Arguments;
					arg_count = candidate_args != null ? candidate_args.Count : 0;
				}
			}

			if (candidates_expanded != null && candidates_expanded.ContainsKey (best_candidate)) {
				candidate_args = candidates_expanded[best_candidate];
				arg_count = candidate_args.Count;
			}

			//
			// Now check that there are no ambiguities i.e the selected method
			// should be better than all the others
			//
			MethodSpec ambiguous = null;
			for (int ix = 1; ix < candidate_top; ix++) {
				var candidate = candidates [ix];

				if (candidate == best_candidate)
					continue;

				bool cand_params = params_candidates != null && params_candidates.Contains (candidate);
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

			if (best_candidate.IsGeneric) {
				ConstraintChecker.CheckAll (best_candidate.GetGenericMethodDefinition (), best_candidate.TypeArguments,
					best_candidate.Constraints, loc, ec.Report);
			}

			//
			// Check ObsoleteAttribute on the best method
			//
			ObsoleteAttribute oa = best_candidate.GetAttributeObsolete ();
			if (oa != null && !ec.IsObsolete)
				AttributeTester.Report_ObsoleteMessage (oa, GetSignatureForError (), loc, ec.Report);

			best_candidate.MemberDefinition.SetIsUsed ();

			Arguments = candidate_args;
			return this;
		}

		bool NoExactMatch (ResolveContext ec, ref Arguments Arguments, bool params_expanded)
		{
			AParametersCollection pd = best_candidate.Parameters;
			int arg_count = Arguments == null ? 0 : Arguments.Count;

			if (arg_count == pd.Count || pd.HasParams) {
				if (best_candidate.IsGeneric) {
					if (type_arguments == null) {
						ec.Report.Error (411, loc,
							"The type arguments for method `{0}' cannot be inferred from the usage. Try specifying the type arguments explicitly",
							best_candidate.GetGenericMethodDefinition().GetSignatureForError ());
						return true;
					}
				}

				var ta = type_arguments == null ? 0 : type_arguments.Count;
				if (ta != best_candidate.Arity) {
					Error_TypeArgumentsCannotBeUsed (ec.Report, loc, best_candidate, type_arguments.Count);
					return true;
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

				if (!VerifyArgumentsCompat (ec, ref Arguments, arg_count, best_candidate, params_expanded, false, loc))
					return true;

				if (has_inaccessible_candidates_only)
					return true;
			}

			return false;
		}
		
		public override void SetTypeArguments (ResolveContext ec, TypeArguments ta)
		{
			type_arguments = ta;
		}

		public bool VerifyArgumentsCompat (ResolveContext ec, ref Arguments arguments,
							  int arg_count, MethodSpec method,
							  bool chose_params_expanded,
							  bool may_fail, Location loc)
		{
			AParametersCollection pd = method.Parameters;
			int param_count = GetApplicableParametersCount (method, pd);

			int errors = ec.Report.Errors;
			Parameter.Modifier p_mod = 0;
			TypeSpec pt = null;
			int a_idx = 0, a_pos = 0;
			Argument a = null;
			ArrayInitializer params_initializers = null;
			bool has_unsafe_arg = method.ReturnType.IsPointer;

			for (; a_idx < arg_count; a_idx++, ++a_pos) {
				a = arguments [a_idx];
				if (p_mod != Parameter.Modifier.PARAMS) {
					p_mod = pd.FixedParameters [a_idx].ModFlags;
					pt = pd.Types [a_idx];
					has_unsafe_arg |= pt.IsPointer;

					if (p_mod == Parameter.Modifier.PARAMS) {
						if (chose_params_expanded) {
							params_initializers = new ArrayInitializer (arg_count - a_idx, a.Expr.Location);
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
						int name_index = pd.GetParameterIndexByName (na.Name);
						if (name_index < 0 || name_index >= param_count) {
							if (DeclaringType != null && TypeManager.IsDelegateType (DeclaringType)) {
								ec.Report.SymbolRelatedToPreviousError (DeclaringType);
								ec.Report.Error (1746, na.Location,
									"The delegate `{0}' does not contain a parameter named `{1}'",
									TypeManager.CSharpName (DeclaringType), na.Name);
							} else {
								ec.Report.SymbolRelatedToPreviousError (best_candidate);
								ec.Report.Error (1739, na.Location,
									"The best overloaded method match for `{0}' does not contain a parameter named `{1}'",
									TypeManager.CSharpSignature (method), na.Name);
							}
						} else if (arguments[name_index] != a) {
							if (DeclaringType != null && TypeManager.IsDelegateType (DeclaringType))
								ec.Report.SymbolRelatedToPreviousError (DeclaringType);
							else
								ec.Report.SymbolRelatedToPreviousError (best_candidate);

							ec.Report.Error (1744, na.Location,
								"Named argument `{0}' cannot be used for a parameter which has positional argument specified",
								na.Name);
						}
					}
				}

				if (a.Expr.Type == InternalType.Dynamic)
					continue;

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
				params_initializers = new ArrayInitializer (0, loc);
			}

			//
			// Append an array argument with all params arguments
			//
			if (params_initializers != null) {
				arguments.Add (new Argument (
					new ArrayCreation (new TypeExpression (pt, loc), "[]", params_initializers, loc).Resolve (ec)));
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
		ConstSpec constant;

		public ConstantExpr (ConstSpec constant, Location loc)
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
			get { return true; }
		}

		public override TypeSpec DeclaringType {
			get { return constant.DeclaringType; }
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			constant.MemberDefinition.SetIsUsed ();

			if (!rc.IsObsolete) {
				var oa = constant.GetAttributeObsolete ();
				if (oa != null)
					AttributeTester.Report_ObsoleteMessage (oa, TypeManager.GetFullNameSignature (constant), loc, rc.Report);
			}

			// Constants are resolved on-demand
			var c = constant.Value.Resolve (rc) as Constant;

			// Creates reference expression to the constant value
			return Constant.CreateConstant (rc, constant.MemberType, c.GetValue (), loc);
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
	public class FieldExpr : MemberExpr, IDynamicAssign, IMemoryLocation, IVariableReference {
		protected FieldSpec spec;
		VariableInfo variable_info;
		
		LocalTemporary temp;
		bool prepared;
		
		protected FieldExpr (Location l)
		{
			loc = l;
		}

		public FieldExpr (FieldSpec spec, Location loc)
		{
			this.spec = spec;
			this.loc = loc;

			type = spec.MemberType;
		}
		
		public FieldExpr (FieldBase fi, Location l)
			: this (fi.Spec, l)
		{
			loc = l;
		}

		public override string Name {
			get {
				return spec.Name;
			}
		}

		public override bool IsInstance {
			get {
				return !spec.IsStatic;
			}
		}

		public override bool IsStatic {
			get {
				return spec.IsStatic;
			}
		}

		public FieldSpec Spec {
			get {
				return spec;
			}
		}

		public override TypeSpec DeclaringType {
			get {
				return spec.DeclaringType;
			}
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.GetFullNameSignature (spec);
		}

		public VariableInfo VariableInfo {
			get {
				return variable_info;
			}
		}

		public override MemberExpr ResolveMemberAccess (ResolveContext ec, Expression left, Location loc,
								SimpleName original)
		{
			if (spec.MemberType.IsPointer && !ec.IsUnsafe) {
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
			return new TypeOfField (spec, loc);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			return DoResolve (ec, false, false);
		}

		Expression DoResolve (ResolveContext ec, bool lvalue_instance, bool out_access)
		{
			if (!IsStatic){
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
					if (InstanceExpression != EmptyExpression.Null) {
						using (ec.With (ResolveContext.Options.DoFlowAnalysis, false)) {
							InstanceExpression = InstanceExpression.Resolve (ec, ResolveFlags.VariableOrValue);
						}
					}
				}

				if (InstanceExpression == null)
					return null;

				using (ec.Set (ResolveContext.Options.OmitStructFlowAnalysis)) {
					InstanceExpression.CheckMarshalByRefAccess (ec);
				}
			}

			if (!ec.IsObsolete) {
				ObsoleteAttribute oa = spec.GetAttributeObsolete ();
				if (oa != null)
					AttributeTester.Report_ObsoleteMessage (oa, TypeManager.GetFullNameSignature (spec), loc, ec.Report);
			}

			var fb = spec as FixedFieldSpec;
			IVariableReference var = InstanceExpression as IVariableReference;
			
			if (fb != null) {
				IFixedExpression fe = InstanceExpression as IFixedExpression;
				if (!ec.HasSet (ResolveContext.Options.FixedInitializerScope) && (fe == null || !fe.IsFixed)) {
					ec.Report.Error (1666, loc, "You cannot use fixed size buffers contained in unfixed expressions. Try using the fixed statement");
				}

				if (InstanceExpression.eclass != ExprClass.Variable) {
					ec.Report.SymbolRelatedToPreviousError (spec);
					ec.Report.Error (1708, loc, "`{0}': Fixed size buffers can only be accessed through locals or fields",
						TypeManager.GetFullNameSignature (spec));
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
			if (!vi.IsFieldAssigned (ec, Name, loc))
				return null;

			variable_info = vi.GetSubStruct (Name);
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
			if (right_side == EmptyExpression.OutAccess.Instance || right_side == EmptyExpression.LValueMemberOutAccess)
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
				var.VariableInfo.SetFieldAssigned (ec, Name);

			bool lvalue_instance = !spec.IsStatic && TypeManager.IsValueType (spec.DeclaringType);
			bool out_access = right_side == EmptyExpression.OutAccess.Instance || right_side == EmptyExpression.LValueMemberOutAccess;

			Expression e = DoResolve (ec, lvalue_instance, out_access);

			if (e == null)
				return null;

			spec.MemberDefinition.SetIsAssigned ();

			if ((right_side == EmptyExpression.UnaryAddress || right_side == EmptyExpression.OutAccess.Instance) &&
					(spec.Modifiers & Modifiers.VOLATILE) != 0) {
				ec.Report.Warning (420, 1, loc,
					"`{0}': A volatile field references will not be treated as volatile",
					spec.GetSignatureForError ());
			}

			if (spec.IsReadOnly) {
				// InitOnly fields can only be assigned in constructors or initializers
				if (!ec.HasAny (ResolveContext.Options.FieldInitializerScope | ResolveContext.Options.ConstructorScope))
					return Report_AssignToReadonly (ec, right_side);

				if (ec.HasSet (ResolveContext.Options.ConstructorScope)) {

					// InitOnly fields cannot be assigned-to in a different constructor from their declaring type
					if (!TypeManager.IsEqual (ec.CurrentMemberDefinition.Parent.Definition, DeclaringType.GetDefinition ()))
						return Report_AssignToReadonly (ec, right_side);
					// static InitOnly fields cannot be assigned-to in an instance constructor
					if (IsStatic && !ec.IsStatic)
						return Report_AssignToReadonly (ec, right_side);
					// instance constructors can't modify InitOnly fields of other instances of the same type
					if (!IsStatic && !(InstanceExpression is This))
						return Report_AssignToReadonly (ec, right_side);
				}
			}

			if (right_side == EmptyExpression.OutAccess.Instance &&
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
			return spec.GetHashCode ();
		}
		
		public bool IsFixed {
			get {
				//
				// A variable of the form V.I is fixed when V is a fixed variable of a struct type
				//
				IVariableReference variable = InstanceExpression as IVariableReference;
				if (variable != null)
					return InstanceExpression.Type.IsStruct && variable.IsFixed;

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

			if (spec != fe.spec)
				return false;

			if (InstanceExpression == null || fe.InstanceExpression == null)
				return true;

			return InstanceExpression.Equals (fe.InstanceExpression);
		}
		
		public void Emit (EmitContext ec, bool leave_copy)
		{
			bool is_volatile = false;

			if ((spec.Modifiers & Modifiers.VOLATILE) != 0)
				is_volatile = true;

			spec.MemberDefinition.SetIsUsed ();
			
			if (IsStatic){
				if (is_volatile)
					ec.Emit (OpCodes.Volatile);

				ec.Emit (OpCodes.Ldsfld, spec);
			} else {
				if (!prepared)
					EmitInstance (ec, false);

				// Optimization for build-in types
				if (TypeManager.IsStruct (type) && TypeManager.IsEqual (type, ec.MemberContext.CurrentType) && TypeManager.IsEqual (InstanceExpression.Type, type)) {
					ec.EmitLoadFromPtr (type);
				} else {
					var ff = spec as FixedFieldSpec;
					if (ff != null) {
						ec.Emit (OpCodes.Ldflda, spec);
						ec.Emit (OpCodes.Ldflda, ff.Element);
					} else {
						if (is_volatile)
							ec.Emit (OpCodes.Volatile);

						ec.Emit (OpCodes.Ldfld, spec);
					}
				}
			}

			if (leave_copy) {
				ec.Emit (OpCodes.Dup);
				if (!IsStatic) {
					temp = new LocalTemporary (this.Type);
					temp.Store (ec);
				}
			}
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			prepared = prepare_for_load;
			EmitInstance (ec, prepared);

			source.Emit (ec);
			if (leave_copy) {
				ec.Emit (OpCodes.Dup);
				if (!IsStatic) {
					temp = new LocalTemporary (this.Type);
					temp.Store (ec);
				}
			}

			if ((spec.Modifiers & Modifiers.VOLATILE) != 0)
				ec.Emit (OpCodes.Volatile);
					
			spec.MemberDefinition.SetIsAssigned ();

			if (IsStatic)
				ec.Emit (OpCodes.Stsfld, spec);
			else
				ec.Emit (OpCodes.Stfld, spec);
			
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
			bool is_volatile = (spec.Modifiers & Modifiers.VOLATILE) != 0;

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
			if ((mode & AddressOp.Store) != 0)
				spec.MemberDefinition.SetIsAssigned ();
			if ((mode & AddressOp.Load) != 0)
				spec.MemberDefinition.SetIsUsed ();

			//
			// Handle initonly fields specially: make a copy and then
			// get the address of the copy.
			//
			bool need_copy;
			if (spec.IsReadOnly){
				need_copy = true;
				if (ec.HasSet (EmitContext.Options.ConstructorScope)){
					if (IsStatic){
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
				local = ec.DeclareLocal (type, false);
				ec.Emit (OpCodes.Stloc, local);
				ec.Emit (OpCodes.Ldloca, local);
				return;
			}


			if (IsStatic){
				ec.Emit (OpCodes.Ldsflda, spec);
			} else {
				if (!prepared)
					EmitInstance (ec, false);
				ec.Emit (OpCodes.Ldflda, spec);
			}
		}

		public SLE.Expression MakeAssignExpression (BuilderContext ctx)
		{
			return MakeExpression (ctx);
		}

		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			return SLE.Expression.Field (InstanceExpression.MakeExpression (ctx), spec.GetMetaInfo ());
		}
	}

	
	/// <summary>
	///   Expression that evaluates to a Property.  The Assign class
	///   might set the `Value' expression if we are in an assignment.
	///
	///   This is not an LValue because we need to re-write the expression, we
	///   can not take data from the stack and store it.  
	/// </summary>
	public class PropertyExpr : MemberExpr, IDynamicAssign
	{
		PropertySpec spec;
		TypeArguments targs;
		
		LocalTemporary temp;
		bool prepared;

		public PropertyExpr (TypeSpec container_type, PropertySpec spec, Location l)
		{
			this.spec = spec;
			loc = l;

			type = spec.MemberType;
		}

		#region Properties

		public override string Name {
			get {
				return spec.Name;
			}
		}

		public override bool IsInstance {
			get {
				return !IsStatic;
			}
		}

		public override bool IsStatic {
			get {
				return spec.IsStatic;
			}
		}

		#endregion

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
			args.Add (new Argument (new TypeOfMethod (spec.Get, loc)));
			return CreateExpressionFactoryCall (ec, "Property", args);
		}

		public Expression CreateSetterTypeOfExpression ()
		{
			return new TypeOfMethod (spec.Set, loc);
		}

		public override TypeSpec DeclaringType {
			get {
				return spec.DeclaringType;
			}
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.GetFullNameSignature (spec);
		}

		public SLE.Expression MakeAssignExpression (BuilderContext ctx)
		{
			return SLE.Expression.Property (InstanceExpression.MakeExpression (ctx), (MethodInfo) spec.Set.GetMetaInfo ());
		}

		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			return SLE.Expression.Property (InstanceExpression.MakeExpression (ctx), (MethodInfo) spec.Get.GetMetaInfo ());
		}

		public PropertySpec PropertyInfo {
			get {
				return spec;
			}
		}

		bool InstanceResolve (ResolveContext ec, bool lvalue_instance, bool must_do_cs1540_check)
		{
			if (IsStatic) {
				InstanceExpression = null;
				return true;
			}

			if (InstanceExpression == null) {
				SimpleName.Error_ObjectRefRequired (ec, loc, GetSignatureForError ());
				return false;
			}

			InstanceExpression = InstanceExpression.Resolve (ec);
			if (lvalue_instance && InstanceExpression != null)
				InstanceExpression = InstanceExpression.ResolveLValue (ec, EmptyExpression.LValueMemberAccess);

			if (InstanceExpression == null)
				return false;

			InstanceExpression.CheckMarshalByRefAccess (ec);

			if (must_do_cs1540_check && (InstanceExpression != EmptyExpression.Null) &&
			    !TypeManager.IsInstantiationOfSameGenericType (InstanceExpression.Type, ec.CurrentType) &&
			    !TypeManager.IsNestedChildOf (ec.CurrentType, InstanceExpression.Type) &&
			    !TypeManager.IsSubclassOf (InstanceExpression.Type, ec.CurrentType)) {
				ec.Report.SymbolRelatedToPreviousError (spec);
				Error_CannotAccessProtected (ec, loc, spec, InstanceExpression.Type, ec.CurrentType);
				return false;
			}

			return true;
		}

		void Error_PropertyNotValid (ResolveContext ec)
		{
			ec.Report.SymbolRelatedToPreviousError (spec);
			ec.Report.Error (1546, loc, "Property or event `{0}' is not supported by the C# language",
				GetSignatureForError ());
		}

		public bool IsAccessibleFrom (TypeSpec invocation_type, bool lvalue)
		{
			bool dummy;
			var accessor = lvalue ? spec.Set : spec.Get;
			if (accessor == null && lvalue)
				accessor = spec.Get;
			return accessor != null && IsMemberAccessible (invocation_type, accessor, out dummy);
		}

		bool IsSingleDimensionalArrayLength ()
		{
			if (DeclaringType != TypeManager.array_type || !spec.HasGet || Name != "Length")
				return false;

			ArrayContainer ac = InstanceExpression.Type as ArrayContainer;
			return ac != null && ac.Rank == 1;
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			eclass = ExprClass.PropertyAccess;

			bool must_do_cs1540_check = false;
			ec.Report.DisableReporting ();
			bool res = ResolveGetter (ec, ref must_do_cs1540_check);
			ec.Report.EnableReporting ();

			if (!res) {
				if (InstanceExpression != null) {
					TypeSpec expr_type = InstanceExpression.Type;
					ExtensionMethodGroupExpr ex_method_lookup = ec.LookupExtensionMethod (expr_type, Name, 0, loc);
					if (ex_method_lookup != null) {
						ex_method_lookup.ExtensionExpression = InstanceExpression;
						ex_method_lookup.SetTypeArguments (ec, targs);
						return ex_method_lookup.Resolve (ec);
					}
				}

				ResolveGetter (ec, ref must_do_cs1540_check);
				return null;
			}

			if (!InstanceResolve (ec, false, must_do_cs1540_check))
				return null;

			//
			// Only base will allow this invocation to happen.
			//
			if (IsBase && spec.IsAbstract) {
				Error_CannotCallAbstractBase (ec, TypeManager.GetFullNameSignature (spec));
			}

			if (spec.MemberType.IsPointer && !ec.IsUnsafe){
				UnsafeError (ec, loc);
			}

			if (!ec.IsObsolete) {
				ObsoleteAttribute oa = spec.GetAttributeObsolete ();
				if (oa != null)
					AttributeTester.Report_ObsoleteMessage (oa, GetSignatureForError (), loc, ec.Report);
			}

			return this;
		}

		override public Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			eclass = ExprClass.PropertyAccess;

			if (right_side == EmptyExpression.OutAccess.Instance) {
				if (ec.CurrentBlock.Toplevel.GetParameterReference (spec.Name, loc) is MemberAccess) {
					ec.Report.Error (1939, loc, "A range variable `{0}' may not be passes as `ref' or `out' parameter",
					    spec.Name);
				} else {
					right_side.DoResolveLValue (ec, this);
				}
				return null;
			}

			if (right_side == EmptyExpression.LValueMemberAccess || right_side == EmptyExpression.LValueMemberOutAccess) {
				Error_CannotModifyIntermediateExpressionValue (ec);
			}

			if (spec.IsNotRealProperty) {
				Error_PropertyNotValid (ec);
				return null;
			}

			if (!spec.HasSet){
				if (ec.CurrentBlock.Toplevel.GetParameterReference (spec.Name, loc) is MemberAccess) {
					ec.Report.Error (1947, loc, "A range variable `{0}' cannot be assigned to. Consider using `let' clause to store the value",
						spec.Name);
				} else {
					ec.Report.Error (200, loc, "Property or indexer `{0}' cannot be assigned to (it is read only)",
						GetSignatureForError ());
				}
				return null;
			}

			if (targs != null) {
				base.SetTypeArguments (ec, targs);
				return null;
			}

			bool must_do_cs1540_check;
			if (!IsMemberAccessible (ec.CurrentType, spec.Set, out must_do_cs1540_check)) {
				if (spec.HasDifferentAccessibility) {
					ec.Report.SymbolRelatedToPreviousError (spec.Set);
					ec.Report.Error (272, loc, "The property or indexer `{0}' cannot be used in this context because the set accessor is inaccessible",
						TypeManager.CSharpSignature (spec));
				} else {
					ec.Report.SymbolRelatedToPreviousError (spec.Set);
					ErrorIsInaccesible (loc, TypeManager.CSharpSignature (spec.Set), ec.Report);
				}
				return null;
			}
			
			if (!InstanceResolve (ec, TypeManager.IsStruct (spec.DeclaringType), must_do_cs1540_check))
				return null;
			
			//
			// Only base will allow this invocation to happen.
			//
			if (IsBase && spec.IsAbstract){
				Error_CannotCallAbstractBase (ec, TypeManager.GetFullNameSignature (spec));
			}

			if (spec.MemberType.IsPointer && !ec.IsUnsafe) {
				UnsafeError (ec, loc);
			}

			if (!ec.IsObsolete) {
				ObsoleteAttribute oa = spec.GetAttributeObsolete ();
				if (oa != null)
					AttributeTester.Report_ObsoleteMessage (oa, GetSignatureForError (), loc, ec.Report);
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
				ec.Emit (OpCodes.Ldlen);
				ec.Emit (OpCodes.Conv_I4);
				return;
			}

			Invocation.EmitCall (ec, IsBase, InstanceExpression, spec.Get, null, loc, prepared, false);
			
			if (leave_copy) {
				ec.Emit (OpCodes.Dup);
				if (!IsStatic) {
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
					ec.Emit (OpCodes.Dup);
					if (!IsStatic) {
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
			
			Invocation.EmitCall (ec, IsBase, InstanceExpression, spec.Set, args, loc, false, prepared);
			
			if (temp != null) {
				temp.Emit (ec);
				temp.Release (ec);
			}
		}

		bool ResolveGetter (ResolveContext ec, ref bool must_do_cs1540_check)
		{
			if (targs != null) {
				base.SetTypeArguments (ec, targs);
				return false;
			}

			if (spec.IsNotRealProperty) {
				Error_PropertyNotValid (ec);
				return false;
			}

			if (!spec.HasGet) {
				if (InstanceExpression != EmptyExpression.Null) {
					ec.Report.SymbolRelatedToPreviousError (spec);
					ec.Report.Error (154, loc, "The property or indexer `{0}' cannot be used in this context because it lacks the `get' accessor",
						spec.GetSignatureForError ());
					return false;
				}
			}

			if (spec.HasGet && !IsMemberAccessible (ec.CurrentType, spec.Get, out must_do_cs1540_check)) {
				if (spec.HasDifferentAccessibility) {
					ec.Report.SymbolRelatedToPreviousError (spec.Get);
					ec.Report.Error (271, loc, "The property or indexer `{0}' cannot be used in this context because the get accessor is inaccessible",
						TypeManager.CSharpSignature (spec));
				} else {
					ec.Report.SymbolRelatedToPreviousError (spec.Get);
					ErrorIsInaccesible (loc, TypeManager.CSharpSignature (spec.Get), ec.Report);
				}

				return false;
			}

			return true;
		}

		public override void SetTypeArguments (ResolveContext ec, TypeArguments ta)
		{
			targs = ta;
		}
	}

	/// <summary>
	///   Fully resolved expression that evaluates to an Event
	/// </summary>
	public class EventExpr : MemberExpr
	{
		readonly EventSpec spec;

		public EventExpr (EventSpec spec, Location loc)
		{
			this.spec = spec;
			this.loc = loc;
		}

		public override string Name {
			get {
				return spec.Name;
			}
		}

		public override bool IsInstance {
			get {
				return !spec.IsStatic;
			}
		}

		public override bool IsStatic {
			get {
				return spec.IsStatic;
			}
		}

		public override TypeSpec DeclaringType {
			get {
				return spec.DeclaringType;
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

			if (spec.DeclaringType == ec.CurrentType ||
			    TypeManager.IsNestedChildOf(ec.CurrentType, spec.DeclaringType)) {
					
				// TODO: Breaks dynamic binder as currect context fields are imported and not compiled
				EventField mi = spec.MemberDefinition as EventField;

				if (mi != null && mi.HasBackingField) {
					mi.SetIsUsed ();
					if (!ec.IsObsolete)
						mi.CheckObsoleteness (loc);

					if ((mi.ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0 && !ec.HasSet (ResolveContext.Options.CompoundAssignmentScope))
						Error_AssignmentEventOnly (ec);
					
					FieldExpr ml = new FieldExpr (mi.BackingField, loc);

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
			if (IsStatic) {
				InstanceExpression = null;
				return true;
			}

			if (InstanceExpression == null) {
				SimpleName.Error_ObjectRefRequired (ec, loc, GetSignatureForError ());
				return false;
			}

			InstanceExpression = InstanceExpression.Resolve (ec);
			if (InstanceExpression == null)
				return false;

			if (IsBase && spec.IsAbstract) {
				Error_CannotCallAbstractBase (ec, TypeManager.CSharpSignature(spec));
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
				ec.Report.SymbolRelatedToPreviousError (spec);
				ErrorIsInaccesible (loc, TypeManager.CSharpSignature (spec), ec.Report);
				return false;
			}

			return true;
		}

		public bool IsAccessibleFrom (TypeSpec invocation_type)
		{
			bool dummy;
			return IsMemberAccessible (invocation_type, spec.AccessorAdd, out dummy) &&
				IsMemberAccessible (invocation_type, spec.AccessorRemove, out dummy);
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

		protected override Expression DoResolve (ResolveContext ec)
		{
			eclass = ExprClass.EventAccess;

			bool must_do_cs1540_check;
			if (!(IsMemberAccessible (ec.CurrentType, spec.AccessorAdd, out must_do_cs1540_check) &&
			      IsMemberAccessible (ec.CurrentType, spec.AccessorRemove, out must_do_cs1540_check))) {
				ec.Report.SymbolRelatedToPreviousError (spec);
				ErrorIsInaccesible (loc, TypeManager.CSharpSignature (spec), ec.Report);
				return null;
			}

			if (!InstanceResolve (ec, must_do_cs1540_check))
				return null;

			if (!ec.HasSet (ResolveContext.Options.CompoundAssignmentScope)) {
				Error_CannotAssign (ec);
				return null;
			}

			if (!ec.IsObsolete) {
				var oa = spec.GetAttributeObsolete ();
				if (oa != null)
					AttributeTester.Report_ObsoleteMessage (oa, GetSignatureForError (), loc, ec.Report);
			}

			spec.MemberDefinition.SetIsUsed ();
			type = spec.MemberType;
			
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
				GetSignatureForError (), TypeManager.CSharpName (spec.DeclaringType));
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpSignature (spec);
		}

		public void EmitAddOrRemove (EmitContext ec, bool is_add, Expression source)
		{
			Arguments args = new Arguments (1);
			args.Add (new Argument (source));
			Invocation.EmitCall (ec, IsBase, InstanceExpression,
				is_add ? spec.AccessorAdd : spec.AccessorRemove,
				args, loc);
		}
	}

	public class TemporaryVariable : VariableReference
	{
		LocalInfo li;

		public TemporaryVariable (TypeSpec type, Location loc)
		{
			this.type = type;
			this.loc = loc;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			eclass = ExprClass.Variable;

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
			return Resolve (ec);
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
			return li.HoistedVariant;
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
		int initializers_count;

		public VarExpr (Location loc)
			: base ("var", loc)
		{
			initializers_count = 1;
		}

		public int VariableInitializersCount {
			set {
				this.initializers_count = value;
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

			if (initializers_count == 1)
				return null;

			if (initializers_count > 1) {
				rc.Compiler.Report.Error (819, loc, "An implicitly typed local variable declaration cannot include multiple declarators");
				initializers_count = 1;
				return null;
			}

			if (initializers_count == 0) {
				initializers_count = 1;
				rc.Compiler.Report.Error (818, loc, "An implicitly typed local variable declarator must include an initializer");
				return null;
			}

			return null;
		}
	}
}	
