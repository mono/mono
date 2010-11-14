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

		public Location Location {
			get { return loc; }
		}

		public virtual string GetSignatureForError ()
		{
			return type.GetDefinition ().GetSignatureForError ();
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
				ErrorIsInaccesible (ec, te.Type.GetSignatureForError (), loc);
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
	
		public static void ErrorIsInaccesible (IMemberContext rc, string member, Location loc)
		{
			rc.Compiler.Report.Error (122, loc, "`{0}' is inaccessible due to its protection level", member);
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
				Error_TypeArgumentsCannotBeUsed (report, ExprClassName, GetSignatureForError (), loc);
			}
		}

		public void Error_TypeArgumentsCannotBeUsed (Report report, string exprType, string name, Location loc)
		{
			report.Error (307, loc, "The {0} `{1}' cannot be used with type arguments",
				exprType, name);
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

		protected void Error_VoidPointerOperation (ResolveContext rc)
		{
			rc.Report.Error (242, loc, "The operation in question is undefined on void pointers");
		}

		public ResolveFlags ExprClassToResolveFlags {
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
			try {
				e = DoResolve (ec);

				if (e == null)
					return null;

				if ((flags & e.ExprClassToResolveFlags) == 0) {
					e.Error_UnexpectedKind (ec, flags, loc);
					return null;
				}

				if (e.type == null)
					throw new InternalErrorException ("Expression `{0}' didn't set its type in DoResolve", e.GetType ());

				return e;
			} catch (Exception ex) {
				if (loc.IsNull || Report.DebugFlags > 0 || ex is CompletionResult || ec.Report.IsDisabled)
					throw;

				ec.Report.Error (584, loc, "Internal compiler error: {0}", ex.Message);
				return EmptyExpression.Null;	// TODO: Add location
			}
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
			rc.Compiler.Report.Error (182, loc,
				"An attribute argument must be a constant expression, typeof expression or array creation expression");
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
		static Expression ExprClassFromMemberInfo (MemberSpec spec, Location loc)
		{
			if (spec is EventSpec)
				return new EventExpr ((EventSpec) spec, loc);
			if (spec is ConstSpec)
				return new ConstantExpr ((ConstSpec) spec, loc);
			if (spec is FieldSpec)
				return new FieldExpr ((FieldSpec) spec, loc);
			if (spec is PropertySpec)
				return new PropertyExpr ((PropertySpec) spec, loc);
			if (spec is TypeSpec)
				return new TypeExpression (((TypeSpec) spec), loc);

			return null;
		}

		protected static MethodSpec ConstructorLookup (ResolveContext rc, TypeSpec type, ref Arguments args, Location loc)
		{
			var ctors = MemberCache.FindMembers (type, Constructor.ConstructorName, true);
			if (ctors == null) {
				rc.Report.SymbolRelatedToPreviousError (type);
				if (type.IsStruct) {
					// Report meaningful error for struct as they always have default ctor in C# context
					OverloadResolver.Error_ConstructorMismatch (rc, type, args == null ? 0 : args.Count, loc);
				} else {
					rc.Report.Error (143, loc, "The class `{0}' has no constructors defined",
						type.GetSignatureForError ());
				}

				return null;
			}

			var r = new OverloadResolver (ctors, OverloadResolver.Restrictions.NoBaseMembers, loc);
			return r.ResolveMember<MethodSpec> (rc, ref args);
		}

		[Flags]
		public enum MemberLookupRestrictions
		{
			None = 0,
			InvocableOnly = 1,
			ExactArity = 1 << 2,
			ReadAccess = 1 << 3
		}

		//
		// Lookup type `queried_type' for code in class `container_type' with a qualifier of
		// `qualifier_type' or null to lookup members in the current class.
		//
		public static Expression MemberLookup (ResolveContext rc, TypeSpec currentType, TypeSpec queried_type, string name, int arity, MemberLookupRestrictions restrictions, Location loc)
		{
			var members = MemberCache.FindMembers (queried_type, name, false);
			if (members == null)
				return null;

			MemberSpec non_method = null;
			MemberSpec ambig_non_method = null;
			currentType = currentType ?? InternalType.FakeInternalType;
			do {
				for (int i = 0; i < members.Count; ++i) {
					var member = members[i];

					// HACK: for events because +=/-= can appear at same class only, should use OverrideToBase there
					if ((member.Modifiers & Modifiers.OVERRIDE) != 0 && member.Kind != MemberKind.Event)
						continue;

					if ((arity > 0 || (restrictions & MemberLookupRestrictions.ExactArity) != 0) && member.Arity != arity)
						continue;

					if (rc != null) {
						if (!member.IsAccessible (currentType))
							continue;

						//
						// With runtime binder we can have a situation where queried type is inaccessible
						// because it came via dynamic object, the check about inconsisted accessibility
						// had no effect as the type was unknown during compilation
						//
						// class A {
						//		private class N { }
						//
						//		public dynamic Foo ()
						//		{
						//			return new N ();
						//		}
						//	}
						//
						if (rc.Compiler.IsRuntimeBinder && !member.DeclaringType.IsAccessible (currentType))
							continue;
					}

					if ((restrictions & MemberLookupRestrictions.InvocableOnly) != 0) {
						if (member is MethodSpec)
							return new MethodGroupExpr (members, queried_type, loc);

						if (!Invocation.IsMemberInvocable (member))
							continue;
					}

					if (non_method == null || member is MethodSpec) {
						non_method = member;
					} else if (currentType != null) {
						ambig_non_method = member;
					}
				}

				if (non_method != null) {
					if (ambig_non_method != null && rc != null) {
						rc.Report.SymbolRelatedToPreviousError (non_method);
						rc.Report.SymbolRelatedToPreviousError (ambig_non_method);
						rc.Report.Error (229, loc, "Ambiguity between `{0}' and `{1}'",
							non_method.GetSignatureForError (), ambig_non_method.GetSignatureForError ());
					}

					if (non_method is MethodSpec)
						return new MethodGroupExpr (members, queried_type, loc);

					return ExprClassFromMemberInfo (non_method, loc);
				}

				if (members[0].DeclaringType.BaseType == null)
					members = null;
				else
					members = MemberCache.FindMembers (members[0].DeclaringType.BaseType, name, false);

			} while (members != null);

			return null;
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
		protected static Expression GetOperatorFalse (ResolveContext ec, Expression e, Location loc)
		{
			return GetOperatorTrueOrFalse (ec, e, false, loc);
		}

		static Expression GetOperatorTrueOrFalse (ResolveContext ec, Expression e, bool is_true, Location loc)
		{
			var op = is_true ? Operator.OpType.True : Operator.OpType.False;
			var methods = MemberCache.GetUserOperator (e.type, op, false);
			if (methods == null)
				return null;

			Arguments arguments = new Arguments (1);
			arguments.Add (new Argument (e));

			var res = new OverloadResolver (methods, OverloadResolver.Restrictions.BaseMembersIncluded | OverloadResolver.Restrictions.NoBaseMembers, loc);
			var oper = res.ResolveOperator (ec, ref arguments);

			if (oper == null)
				return null;

			return new UserOperatorCall (oper, arguments, null, loc);
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
		public virtual Expression Clone (CloneContext clonectx)
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

		public ExpressionStatement ResolveStatement (BlockContext ec)
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
			var op = find_explicit ? Operator.OpType.Explicit : Operator.OpType.Implicit;

			var mi = MemberCache.GetUserOperator (child.Type, op, true);
			if (mi == null){
				mi = MemberCache.GetUserOperator (type, op, true);
			}
			
			foreach (MethodSpec oper in mi) {
				if (oper.ReturnType != type)
					continue;

				if (oper.Parameters.Types [0] == child.Type)
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
				var all_oper = MemberCache.GetUserOperator (TypeManager.decimal_type, Operator.OpType.Explicit, true);

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
	
	class OpcodeCast : TypeCast
	{
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

	//
	// Opcode casts expression with 2 opcodes but only
	// single expression tree node
	//
	class OpcodeCastDuplex : OpcodeCast
	{
		readonly OpCode second;

		public OpcodeCastDuplex (Expression child, TypeSpec returnType, OpCode first, OpCode second)
			: base (child, returnType, first)
		{
			this.second = second;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			ec.Emit (second);
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

		#region Properties

		public Expression OriginalExpression {
			get {
				return orig_expr;
			}
		}

		#endregion

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

		public abstract Expression LookupNameExpression (ResolveContext rc, MemberLookupRestrictions restriction);
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

		protected override Expression DoResolve (ResolveContext ec)
		{
			return SimpleNameResolve (ec, null, false);
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			return SimpleNameResolve (ec, right_side, false);
		}

		public override FullNamedExpression ResolveAsTypeStep (IMemberContext ec, bool silent)
		{
			int errors = ec.Compiler.Report.Errors;
			FullNamedExpression fne = ec.LookupNamespaceOrType (Name, Arity, loc, /*ignore_cs0104=*/ false);

			if (fne != null) {
				if (fne.Type != null && Arity > 0) {
					if (HasTypeArguments) {
						GenericTypeExpr ct = new GenericTypeExpr (fne.Type, targs, loc);
						return ct.ResolveAsTypeStep (ec, false);
					}

					return new GenericOpenTypeExpr (fne.Type, loc);
				}

				//
				// dynamic namespace is ignored when dynamic is allowed (does not apply to types)
				//
				if (!(fne is Namespace))
					return fne;
			}

			if (Arity == 0 && Name == "dynamic" && RootContext.Version > LanguageVersion.V_3) {
				if (!ec.Compiler.PredefinedAttributes.Dynamic.IsDefined) {
					ec.Compiler.Report.Error (1980, Location,
						"Dynamic keyword requires `{0}' to be defined. Are you missing System.Core.dll assembly reference?",
						ec.Compiler.PredefinedAttributes.Dynamic.GetSignatureForError ());
				}

				return new DynamicTypeExpr (loc);
			}

			if (fne != null)
				return fne;

			if (silent || errors != ec.Compiler.Report.Errors)
				return null;

			Error_TypeOrNamespaceNotFound (ec);
			return null;
		}

		public override Expression LookupNameExpression (ResolveContext rc, MemberLookupRestrictions restrictions)
		{
			int lookup_arity = Arity;
			bool errorMode = false;
			Expression e;
			Block current_block = rc.CurrentBlock;
			INamedBlockVariable variable = null;
			bool variable_found = false;

			while (true) {
				//
				// Stage 1: binding to local variables or parameters
				//
				// LAMESPEC: It should take invocableOnly into account but that would break csc compatibility
				//
				if (current_block != null && lookup_arity == 0) {
					if (current_block.ParametersBlock.TopBlock.GetLocalName (Name, current_block.Original, ref variable)) {
						if (!variable.IsDeclared) {
							// We found local name in accessible block but it's not
							// initialized yet, maybe the user wanted to bind to something else
							errorMode = true;
							variable_found = true;
						} else {
							e = variable.CreateReferenceExpression (rc, loc);
							if (e != null) {
								if (Arity > 0)
									Error_TypeArgumentsCannotBeUsed (rc.Report, "variable", Name, loc);

								return e;
							}
						}
					}
				}

				//
				// Stage 2: Lookup members if we are inside a type up to top level type for nested types
				//
				TypeSpec member_type = rc.CurrentType;
				TypeSpec current_type = member_type;
				for (; member_type != null; member_type = member_type.DeclaringType) {
					e = MemberLookup (errorMode ? null : rc, current_type, member_type, Name, lookup_arity, restrictions, loc);
					if (e == null)
						continue;

					var me = e as MemberExpr;
					if (me == null) {
						// The name matches a type, defer to ResolveAsTypeStep
						if (e is TypeExpr)
							break;

						continue;
					}

					if (errorMode) {
						if (variable != null) {
							if (me is FieldExpr || me is ConstantExpr || me is EventExpr || me is PropertyExpr) {
								rc.Report.Error (844, loc,
									"A local variable `{0}' cannot be used before it is declared. Consider renaming the local variable when it hides the member `{1}'",
									Name, me.GetSignatureForError ());
							} else {
								break;
							}
						} else if (me is MethodGroupExpr) {
							// Leave it to overload resolution to report correct error
						} else {
							// TODO: rc.Report.SymbolRelatedToPreviousError ()
							ErrorIsInaccesible (rc, me.GetSignatureForError (), loc);
						}
					} else {
						// LAMESPEC: again, ignores InvocableOnly
						if (variable != null) {
							rc.Report.SymbolRelatedToPreviousError (variable.Location, Name);
							rc.Report.Error (135, loc, "`{0}' conflicts with a declaration in a child block", Name);
						}

						//
						// MemberLookup does not check accessors availability, this is actually needed for properties only
						//
						var pe = me as PropertyExpr;
						if (pe != null) {

							// Break as there is no other overload available anyway
							if ((restrictions & MemberLookupRestrictions.ReadAccess) != 0) {
								if (!pe.PropertyInfo.HasGet || !pe.PropertyInfo.Get.IsAccessible (current_type))
									break;

								pe.Getter = pe.PropertyInfo.Get;
							} else {
								if (!pe.PropertyInfo.HasSet || !pe.PropertyInfo.Set.IsAccessible (current_type))
									break;

								pe.Setter = pe.PropertyInfo.Set;
							}
						}
					}

					// TODO: It's used by EventExpr -> FieldExpr transformation only
					// TODO: Should go to MemberAccess
					me = me.ResolveMemberAccess (rc, null, null);

					if (Arity > 0) {
						targs.Resolve (rc);
						me.SetTypeArguments (rc, targs);
					}

					return me;
				}

				//
				// Stage 3: Lookup nested types, namespaces and type parameters in the context
				//
				if ((restrictions & MemberLookupRestrictions.InvocableOnly) == 0 && !variable_found) {
					e = ResolveAsTypeStep (rc, lookup_arity == 0 || !errorMode);
					if (e != null) {
						if (variable != null) {
							rc.Report.SymbolRelatedToPreviousError (variable.Location, Name);
							rc.Report.Error (135, loc, "`{0}' conflicts with a declaration in a child block", Name);
						}

						return e;
					}
				}

				if (errorMode) {
					if (variable_found) {
						rc.Report.Error (841, loc, "A local variable `{0}' cannot be used before it is declared", Name);
					} else {
						rc.Report.Error (103, loc, "The name `{0}' does not exist in the current context", Name);
					}

					return null;
				}

				if (RootContext.EvalMode) {
					var fi = Evaluator.LookupField (Name);
					if (fi != null)
						return new FieldExpr (fi.Item1, loc);
				}

				lookup_arity = 0;
				restrictions &= ~MemberLookupRestrictions.InvocableOnly;
				errorMode = true;
			}
		}
		
		Expression SimpleNameResolve (ResolveContext ec, Expression right_side, bool intermediate)
		{
			Expression e = LookupNameExpression (ec, right_side == null ? MemberLookupRestrictions.ReadAccess : MemberLookupRestrictions.None);

			if (e == null)
				return null;

			if (right_side != null) {
				if (e is TypeExpr) {
				    e.Error_UnexpectedKind (ec, ResolveFlags.VariableOrValue, loc);
				    return null;
				}

				e = e.ResolveLValue (ec, right_side);
			} else {
				e = e.Resolve (ec);
			}

			//if (ec.CurrentBlock == null || ec.CurrentBlock.CheckInvariantMeaningInBlock (Name, e, Location))
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

	/// <summary>
	///   This class denotes an expression which evaluates to a member
	///   of a struct or a class.
	/// </summary>
	public abstract class MemberExpr : Expression
	{
		//
		// An instance expression associated with this member, if it's a
		// non-static member
		//
		public Expression InstanceExpression;

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
			get { return InstanceExpression is BaseThis; }
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

		// TODO: Not needed
		protected abstract TypeSpec DeclaringType {
			get;
		}

		//
		// Converts best base candidate for virtual method starting from QueriedBaseType
		//
		protected MethodSpec CandidateToBaseOverride (ResolveContext rc, MethodSpec method)
		{
			//
			// Only when base.member is used and method is virtual
			//
			if (!IsBase)
				return method;

			//
			// Overload resulution works on virtual or non-virtual members only (no overrides). That
			// means for base.member access we have to find the closest match after we found best candidate
			//
			if ((method.Modifiers & (Modifiers.ABSTRACT | Modifiers.VIRTUAL | Modifiers.STATIC)) != Modifiers.STATIC) {
				//
				// The method could already be what we are looking for
				//
				TypeSpec[] targs = null;
				if (method.DeclaringType != InstanceExpression.Type) {
					var base_override = MemberCache.FindMember (InstanceExpression.Type, new MemberFilter (method), BindingRestriction.InstanceOnly) as MethodSpec;
					if (base_override != null && base_override.DeclaringType != method.DeclaringType) {
						if (base_override.IsGeneric)
							targs = method.TypeArguments;

						method = base_override;
					}
				}

				// TODO: For now we do it for any hoisted call even if it's needed for
				// hoisted stories only but that requires a new expression wrapper
				if (rc.CurrentAnonymousMethod != null) {
					if (targs == null && method.IsGeneric) {
						targs = method.TypeArguments;
						method = method.GetGenericMethodDefinition ();
					}

					if (method.Parameters.HasArglist)
						throw new NotImplementedException ("__arglist base call proxy");

					method = rc.CurrentMemberDefinition.Parent.PartialContainer.CreateHoistedBaseCallProxy (rc, method);

					// Ideally this should apply to any proxy rewrite but in the case of unary mutators on
					// get/set member expressions second call would fail to proxy because left expression
					// would be of 'this' and not 'base'
					if (rc.CurrentType.IsStruct)
						InstanceExpression = new This (loc).Resolve (rc);
				}

				if (targs != null)
					method = method.MakeGenericMethod (targs);
			}

			//
			// Only base will allow this invocation to happen.
			//
			if (method.IsAbstract) {
				Error_CannotCallAbstractBase (rc, method.GetSignatureForError ());
			}

			return method;
		}

		protected void CheckProtectedMemberAccess<T> (ResolveContext rc, T member) where T : MemberSpec
		{
			if (InstanceExpression == null)
				return;

			if ((member.Modifiers & Modifiers.AccessibilityMask) == Modifiers.PROTECTED && !(InstanceExpression is This)) {
				var ct = rc.CurrentType;
				var expr_type = InstanceExpression.Type;
				if (ct != expr_type) {
					expr_type = expr_type.GetDefinition ();
					if (ct != expr_type && !IsSameOrBaseQualifier (ct, expr_type)) {
						rc.Report.SymbolRelatedToPreviousError (member);
						rc.Report.Error (1540, loc,
							"Cannot access protected member `{0}' via a qualifier of type `{1}'. The qualifier must be of type `{2}' or derived from it",
							member.GetSignatureForError (), expr_type.GetSignatureForError (), ct.GetSignatureForError ());
					}
				}
			}
		}

		static bool IsSameOrBaseQualifier (TypeSpec type, TypeSpec qtype)
		{
			do {
				type = type.GetDefinition ();

				if (type == qtype || TypeManager.IsFamilyAccessible (qtype, type))
					return true;

				type = type.DeclaringType;
			} while (type != null);

			return false;
		}

		protected void DoBestMemberChecks<T> (ResolveContext rc, T member) where T : MemberSpec, IInterfaceMemberSpec
		{
			if (InstanceExpression != null) {
				InstanceExpression = InstanceExpression.Resolve (rc);
				CheckProtectedMemberAccess (rc, member);
			}

			if (member.MemberType.IsPointer && !rc.IsUnsafe) {
				UnsafeError (rc, loc);
			}

			if (!rc.IsObsolete) {
				ObsoleteAttribute oa = member.GetAttributeObsolete ();
				if (oa != null)
					AttributeTester.Report_ObsoleteMessage (oa, member.GetSignatureForError (), loc, rc.Report);
			}

			if (!(member is FieldSpec))
				member.MemberDefinition.SetIsUsed ();
		}

		protected virtual void Error_CannotCallAbstractBase (ResolveContext rc, string name)
		{
			rc.Report.Error (205, loc, "Cannot call an abstract base member `{0}'", name);
		}

		//
		// Implements identicial simple name and type-name
		//
		public Expression ProbeIdenticalTypeName (ResolveContext rc, Expression left, SimpleName name)
		{
			var t = left.Type;
			if (t.Kind == MemberKind.InternalCompilerType || t is ElementTypeSpec || t.Arity > 0)
				return left;

			// In a member access of the form E.I, if E is a single identifier, and if the meaning of E as a simple-name is
			// a constant, field, property, local variable, or parameter with the same type as the meaning of E as a type-name

			if (left is MemberExpr || left is VariableReference) {
				rc.Report.DisableReporting ();
				Expression identical_type = rc.LookupNamespaceOrType (name.Name, 0, loc, true) as TypeExpr;
				rc.Report.EnableReporting ();
				if (identical_type != null && identical_type.Type == left.Type)
					return identical_type;
			}

			return left;
		}

		public bool ResolveInstanceExpression (ResolveContext rc, Expression rhs)
		{
			if (IsStatic) {
				if (InstanceExpression != null) {
					if (InstanceExpression is TypeExpr) {
						ObsoleteAttribute oa = InstanceExpression.Type.GetAttributeObsolete ();
						if (oa != null && !rc.IsObsolete) {
							AttributeTester.Report_ObsoleteMessage (oa, InstanceExpression.GetSignatureForError (), loc, rc.Report);
						}
					} else {
						var runtime_expr = InstanceExpression as RuntimeValueExpression;
						if (runtime_expr == null || !runtime_expr.IsSuggestionOnly) {
							rc.Report.Error (176, loc,
								"Static member `{0}' cannot be accessed with an instance reference, qualify it with a type name instead",
								GetSignatureForError ());
						}
					}

					InstanceExpression = null;
				}

				return false;
			}

			if (InstanceExpression == null || InstanceExpression is TypeExpr) {
				if (InstanceExpression != null || !This.IsThisAvailable (rc, true)) {
					if (rc.HasSet (ResolveContext.Options.FieldInitializerScope))
						rc.Report.Error (236, loc,
							"A field initializer cannot reference the nonstatic field, method, or property `{0}'",
							GetSignatureForError ());
					else
						rc.Report.Error (120, loc,
							"An object reference is required to access non-static member `{0}'",
							GetSignatureForError ());

					return false;
				}

				if (!TypeManager.IsFamilyAccessible (rc.CurrentType, DeclaringType)) {
					rc.Report.Error (38, loc,
						"Cannot access a nonstatic member of outer type `{0}' via nested type `{1}'",
						DeclaringType.GetSignatureForError (), rc.CurrentType.GetSignatureForError ());
				}

				InstanceExpression = new This (loc);
				if (this is FieldExpr && rc.CurrentType.IsStruct) {
					using (rc.Set (ResolveContext.Options.OmitStructFlowAnalysis)) {
						InstanceExpression = InstanceExpression.Resolve (rc);
					}
				} else {
					InstanceExpression = InstanceExpression.Resolve (rc);
				}

				return false;
			}

			var me = InstanceExpression as MemberExpr;
			if (me != null) {
				me.ResolveInstanceExpression (rc, rhs);

				var fe = me as FieldExpr;
				if (fe != null && fe.IsMarshalByRefAccess ()) {
					rc.Report.SymbolRelatedToPreviousError (me.DeclaringType);
					rc.Report.Warning (1690, 1, loc,
						"Cannot call methods, properties, or indexers on `{0}' because it is a value type member of a marshal-by-reference class",
						me.GetSignatureForError ());
				}

				return true;
			}

			//
			// Run member-access postponed check once we know that
			// the expression is not field expression which is the only
			// expression which can use uninitialized this
			//
			if (InstanceExpression is This && !(this is FieldExpr) && rc.CurrentType.IsStruct) {
				((This)InstanceExpression).CheckStructThisDefiniteAssignment (rc);
			}

			//
			// Additional checks for l-value member access
			//
			if (rhs != null) {
				//
				// TODO: It should be recursive but that would break csc compatibility
				//
				if (InstanceExpression is UnboxCast) {
					rc.Report.Error (445, InstanceExpression.Location, "Cannot modify the result of an unboxing conversion");
				}
			}

			return true;
		}

		public virtual MemberExpr ResolveMemberAccess (ResolveContext ec, Expression left, SimpleName original)
		{
			if (left != null && left.IsNull && TypeManager.IsReferenceType (left.Type)) {
				ec.Report.Warning (1720, 1, left.Location,
					"Expression will always cause a `{0}'", "System.NullReferenceException");
			}

			InstanceExpression = left;
			return this;
		}

		protected void EmitInstance (EmitContext ec, bool prepare_for_load)
		{
			TypeSpec instance_type = InstanceExpression.Type;
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

				// Only to make verifier happy
				if (instance_type.IsGenericParameter && !(InstanceExpression is This) && TypeManager.IsReferenceType (instance_type))
					ec.Emit (OpCodes.Box, instance_type);
			}

			if (prepare_for_load)
				ec.Emit (OpCodes.Dup);
		}

		public abstract void SetTypeArguments (ResolveContext ec, TypeArguments ta);
	}

	// 
	// Represents a group of extension method candidates for whole namespace
	// 
	class ExtensionMethodGroupExpr : MethodGroupExpr, OverloadResolver.IErrorHandler
	{
		NamespaceEntry namespace_entry;
		public readonly Expression ExtensionExpression;

		public ExtensionMethodGroupExpr (IList<MethodSpec> list, NamespaceEntry n, Expression extensionExpr, Location l)
			: base (list.Cast<MemberSpec>().ToList (), extensionExpr.Type, l)
		{
			this.namespace_entry = n;
			this.ExtensionExpression = extensionExpr;
		}

		public override bool IsStatic {
			get { return true; }
		}

		public override IList<MemberSpec> GetBaseMembers (TypeSpec baseType)
		{
			if (namespace_entry == null)
				return null;

			//
			// For extension methodgroup we are not looking for base members but parent
			// namespace extension methods
			//
			int arity = type_arguments == null ? 0 : type_arguments.Count;
			var found = namespace_entry.LookupExtensionMethod (DeclaringType, Name, arity, ref namespace_entry);
			if (found == null)
				return null;

			return found.Cast<MemberSpec> ().ToList ();
		}

		public override MethodGroupExpr LookupExtensionMethod (ResolveContext rc)
		{
			// We are already here
			return null;
		}

		public override MethodGroupExpr OverloadResolve (ResolveContext ec, ref Arguments arguments, OverloadResolver.IErrorHandler ehandler, OverloadResolver.Restrictions restr)
		{
			if (arguments == null)
				arguments = new Arguments (1);

			arguments.Insert (0, new Argument (ExtensionExpression, Argument.AType.ExtensionType));
			var res = base.OverloadResolve (ec, ref arguments, ehandler ?? this, restr);

			// Store resolved argument and restore original arguments
			if (res == null) {
				// Clean-up modified arguments for error reporting
				arguments.RemoveAt (0);
				return null;
			}

			var me = ExtensionExpression as MemberExpr;
			if (me != null)
				me.ResolveInstanceExpression (ec, null);

			InstanceExpression = null;
			return this;
		}

		#region IErrorHandler Members

		bool OverloadResolver.IErrorHandler.AmbiguousCandidates (ResolveContext rc, MemberSpec best, MemberSpec ambiguous)
		{
			return false;
		}

		bool OverloadResolver.IErrorHandler.ArgumentMismatch (ResolveContext rc, MemberSpec best, Argument arg, int index)
		{
			rc.Report.SymbolRelatedToPreviousError (best);
			rc.Report.Error (1928, loc,
				"Type `{0}' does not contain a member `{1}' and the best extension method overload `{2}' has some invalid arguments",
				queried_type.GetSignatureForError (), Name, best.GetSignatureForError ());

			if (index == 0) {
				rc.Report.Error (1929, loc,
					"Extension method instance type `{0}' cannot be converted to `{1}'",
					arg.Type.GetSignatureForError (), ((MethodSpec)best).Parameters.ExtensionMethodType.GetSignatureForError ());
			}

			return true;
		}

		bool OverloadResolver.IErrorHandler.NoArgumentMatch (ResolveContext rc, MemberSpec best)
		{
			return false;
		}

		bool OverloadResolver.IErrorHandler.TypeInferenceFailed (ResolveContext rc, MemberSpec best)
		{
			return false;
		}

		#endregion
	}

	/// <summary>
	///   MethodGroupExpr represents a group of method candidates which
	///   can be resolved to the best method overload
	/// </summary>
	public class MethodGroupExpr : MemberExpr, OverloadResolver.IBaseMembersProvider
	{
		protected IList<MemberSpec> Methods;
		MethodSpec best_candidate;
		TypeSpec best_candidate_return;
		protected TypeArguments type_arguments;

 		SimpleName simple_name;
		protected TypeSpec queried_type;

		public MethodGroupExpr (IList<MemberSpec> mi, TypeSpec type, Location loc)
		{
			Methods = mi;
			this.loc = loc;
			this.type = InternalType.MethodGroup;

			eclass = ExprClass.MethodGroup;
			queried_type = type;
		}

		public MethodGroupExpr (MethodSpec m, TypeSpec type, Location loc)
			: this (new MemberSpec[] { m }, type, loc)
		{
		}

		#region Properties

		public MethodSpec BestCandidate {
			get {
				return best_candidate;
			}
		}

		public TypeSpec BestCandidateReturnType {
			get {
				return best_candidate_return;
			}
		}

		protected override TypeSpec DeclaringType {
			get {
				return queried_type;
			}
		}

		public override bool IsInstance {
			get {
				if (best_candidate != null)
					return !best_candidate.IsStatic;

				return false;
			}
		}

		public override bool IsStatic {
			get {
				if (best_candidate != null)
					return best_candidate.IsStatic;

				return false;
			}
		}

		public override string Name {
			get {
				if (best_candidate != null)
					return best_candidate.Name;

				// TODO: throw ?
				return Methods.First ().Name;
			}
		}

		#endregion

		//
		// When best candidate is already know this factory can be used
		// to avoid expensive overload resolution to be called
		//
		// NOTE: InstanceExpression has to be set manually
		//
		public static MethodGroupExpr CreatePredefined (MethodSpec best, TypeSpec queriedType, Location loc)
		{
			return new MethodGroupExpr (best, queriedType, loc) {
				best_candidate = best,
				best_candidate_return = best.ReturnType
			};
		}

		public override string GetSignatureForError ()
		{
			if (best_candidate != null)
				return best_candidate.GetSignatureForError ();

			return Methods.First ().GetSignatureForError ();
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

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
		
		public void EmitCall (EmitContext ec, Arguments arguments)
		{
			Invocation.EmitCall (ec, InstanceExpression, best_candidate, arguments, loc);			
		}

		public override void Error_ValueCannotBeConverted (ResolveContext ec, Location loc, TypeSpec target, bool expl)
		{
			ec.Report.Error (428, loc, "Cannot convert method group `{0}' to non-delegate type `{1}'. Consider using parentheses to invoke the method",
				Name, TypeManager.CSharpName (target));
		}

		public static bool IsExtensionMethodArgument (Expression expr)
		{
			//
			// LAMESPEC: No details about which expressions are not allowed
			//
			return !(expr is TypeExpr) && !(expr is BaseThis);
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
		public virtual MethodGroupExpr OverloadResolve (ResolveContext ec, ref Arguments args, OverloadResolver.IErrorHandler cerrors, OverloadResolver.Restrictions restr)
		{
			// TODO: causes issues with probing mode, remove explicit Kind check
			if (best_candidate != null && best_candidate.Kind == MemberKind.Destructor)
				return this;

			var r = new OverloadResolver (Methods, type_arguments, restr, loc);
			if ((restr & OverloadResolver.Restrictions.NoBaseMembers) == 0) {
				r.BaseMembersProvider = this;
			}

			if (cerrors != null)
				r.CustomErrors = cerrors;

			// TODO: When in probing mode do IsApplicable only and when called again do VerifyArguments for full error reporting
			best_candidate = r.ResolveMember<MethodSpec> (ec, ref args);
			if (best_candidate == null)
				return r.BestCandidateIsDynamic ? this : null;

			// Overload resolver had to create a new method group, all checks bellow have already been executed
			if (r.BestCandidateNewMethodGroup != null)
				return r.BestCandidateNewMethodGroup;

			if (best_candidate.Kind == MemberKind.Method) {
				if (InstanceExpression != null) {
					if (best_candidate.IsExtensionMethod && args[0].Expr == InstanceExpression) {
						InstanceExpression = null;
					} else {
						if (best_candidate.IsStatic && simple_name != null) {
							InstanceExpression = ProbeIdenticalTypeName (ec, InstanceExpression, simple_name);
						}

						InstanceExpression.Resolve (ec);
					}
				}

				ResolveInstanceExpression (ec, null);
				if (InstanceExpression != null)
					CheckProtectedMemberAccess (ec, best_candidate);
			}

			var base_override = CandidateToBaseOverride (ec, best_candidate);
			if (base_override == best_candidate) {
				best_candidate_return = r.BestCandidateReturnType;
			} else {
				best_candidate = base_override;
				best_candidate_return = best_candidate.ReturnType;
			}

			return this;
		}

		public override MemberExpr ResolveMemberAccess (ResolveContext ec, Expression left, SimpleName original)
		{
			simple_name = original;
			return base.ResolveMemberAccess (ec, left, original);
		}

		public override void SetTypeArguments (ResolveContext ec, TypeArguments ta)
		{
			type_arguments = ta;
		}

		#region IBaseMembersProvider Members

		public virtual IList<MemberSpec> GetBaseMembers (TypeSpec baseType)
		{
			return baseType == null ? null : MemberCache.FindMembers (baseType, Methods [0].Name, false);
		}

		public IParametersMember GetOverrideMemberParameters (MemberSpec member)
		{
			if (queried_type == member.DeclaringType)
				return null;

			return MemberCache.FindMember (queried_type, new MemberFilter ((MethodSpec) member),
				BindingRestriction.InstanceOnly | BindingRestriction.OverrideOnly) as IParametersMember;
		}

		//
		// Extension methods lookup after ordinary methods candidates failed to apply
		//
		public virtual MethodGroupExpr LookupExtensionMethod (ResolveContext rc)
		{
			if (InstanceExpression == null)
				return null;

			InstanceExpression = InstanceExpression.Resolve (rc);
			if (!IsExtensionMethodArgument (InstanceExpression))
				return null;

			int arity = type_arguments == null ? 0 : type_arguments.Count;
			NamespaceEntry methods_scope = null;
			var methods = rc.LookupExtensionMethod (InstanceExpression.Type, Methods[0].Name, arity, ref methods_scope);
			if (methods == null)
				return null;

			var emg = new ExtensionMethodGroupExpr (methods, methods_scope, InstanceExpression, loc);
			emg.SetTypeArguments (rc, type_arguments);
			return emg;
		}

		#endregion
	}

	public struct OverloadResolver
	{
		[Flags]
		public enum Restrictions
		{
			None = 0,
			DelegateInvoke = 1,
			ProbingOnly	= 1 << 1,
			CovariantDelegate = 1 << 2,
			NoBaseMembers = 1 << 3,
			BaseMembersIncluded = 1 << 4
		}

		public interface IBaseMembersProvider
		{
			IList<MemberSpec> GetBaseMembers (TypeSpec baseType);
			IParametersMember GetOverrideMemberParameters (MemberSpec member);
			MethodGroupExpr LookupExtensionMethod (ResolveContext rc);
		}

		public interface IErrorHandler
		{
			bool AmbiguousCandidates (ResolveContext rc, MemberSpec best, MemberSpec ambiguous);
			bool ArgumentMismatch (ResolveContext rc, MemberSpec best, Argument a, int index);
			bool NoArgumentMatch (ResolveContext rc, MemberSpec best);
			bool TypeInferenceFailed (ResolveContext rc, MemberSpec best);
		}

		sealed class NoBaseMembers : IBaseMembersProvider
		{
			public static readonly IBaseMembersProvider Instance = new NoBaseMembers ();

			public IList<MemberSpec> GetBaseMembers (TypeSpec baseType)
			{
				return null;
			}

			public IParametersMember GetOverrideMemberParameters (MemberSpec member)
			{
				return null;
			}

			public MethodGroupExpr LookupExtensionMethod (ResolveContext rc)
			{
				return null;
			}
		}

		struct AmbiguousCandidate
		{
			public readonly MemberSpec Member;
			public readonly bool Expanded;
			public readonly AParametersCollection Parameters;

			public AmbiguousCandidate (MemberSpec member, AParametersCollection parameters, bool expanded)
			{
				Member = member;
				Parameters = parameters;
				Expanded = expanded;
			}
		}

		Location loc;
		IList<MemberSpec> members;
		TypeArguments type_arguments;
		IBaseMembersProvider base_provider;
		IErrorHandler custom_errors;
		Restrictions restrictions;
		MethodGroupExpr best_candidate_extension_group;
		TypeSpec best_candidate_return_type;

		SessionReportPrinter lambda_conv_msgs;
		ReportPrinter prev_recorder;

		public OverloadResolver (IList<MemberSpec> members, Restrictions restrictions, Location loc)
			: this (members, null, restrictions, loc)
		{
		}

		public OverloadResolver (IList<MemberSpec> members, TypeArguments targs, Restrictions restrictions, Location loc)
			: this ()
		{
			if (members == null || members.Count == 0)
				throw new ArgumentException ("empty members set");

			this.members = members;
			this.loc = loc;
			type_arguments = targs;
			this.restrictions = restrictions;
			if (IsDelegateInvoke)
				this.restrictions |= Restrictions.NoBaseMembers;

			base_provider = NoBaseMembers.Instance;
		}

		#region Properties

		public IBaseMembersProvider BaseMembersProvider {
			get {
				return base_provider;
			}
			set {
				base_provider = value;
			}
		}

		public bool BestCandidateIsDynamic { get; set; }

		//
		// Best candidate was found in newly created MethodGroupExpr, used by extension methods
		//
		public MethodGroupExpr BestCandidateNewMethodGroup {
			get {
				return best_candidate_extension_group;
			}
		}

		//
		// Return type can be different between best candidate and closest override
		//
		public TypeSpec BestCandidateReturnType {
			get {
				return best_candidate_return_type;
			}
		}

		public IErrorHandler CustomErrors {
			get {
				return custom_errors;
			}
			set {
				custom_errors = value;
			}
		}

		TypeSpec DelegateType {
			get {
				if ((restrictions & Restrictions.DelegateInvoke) == 0)
					throw new InternalErrorException ("Not running in delegate mode", loc);

				return members [0].DeclaringType;
			}
		}

		bool IsProbingOnly {
			get {
				return (restrictions & Restrictions.ProbingOnly) != 0;
			}
		}

		bool IsDelegateInvoke {
			get {
				return (restrictions & Restrictions.DelegateInvoke) != 0;
			}
		}

		#endregion

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
					p = TypeManager.GetTypeArguments (p)[0];
				}
				if (q.GetDefinition () == TypeManager.expression_type) {
					q = TypeManager.GetTypeArguments (q)[0];
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
				// Dynamic is never better
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
				// Dynamic is never better
				return 1;
			}

			// FIXME: handle lifted operators

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
		static bool BetterFunction (ResolveContext ec, Arguments args, MemberSpec candidate, AParametersCollection cparam, bool candidate_params,
			MemberSpec best, AParametersCollection bparam, bool best_params)
		{
			AParametersCollection candidate_pd = ((IParametersMember) candidate).Parameters;
			AParametersCollection best_pd = ((IParametersMember) best).Parameters;

			bool better_at_least_one = false;
			bool same = true;
			int args_count = args == null ? 0 : args.Count;
			int j = 0;
			Argument a = null;
			TypeSpec ct, bt;
			for (int c_idx = 0, b_idx = 0; j < args_count; ++j, ++c_idx, ++b_idx) {
				a = args[j];

				// Default arguments are ignored for better decision
				if (a.IsDefaultArgument)
					break;

				//
				// When comparing named argument the parameter type index has to be looked up
				// in original parameter set (override version for virtual members)
				//
				NamedArgument na = a as NamedArgument;
				if (na != null) {
					int idx = cparam.GetParameterIndexByName (na.Name);
					ct = candidate_pd.Types[idx];
					if (candidate_params && candidate_pd.FixedParameters[idx].ModFlags == Parameter.Modifier.PARAMS)
						ct = TypeManager.GetElementType (ct);

					idx = bparam.GetParameterIndexByName (na.Name);
					bt = best_pd.Types[idx];
					if (best_params && best_pd.FixedParameters[idx].ModFlags == Parameter.Modifier.PARAMS)
						bt = TypeManager.GetElementType (bt);
				} else {
					ct = candidate_pd.Types[c_idx];
					bt = best_pd.Types[b_idx];

					if (candidate_params && candidate_pd.FixedParameters[c_idx].ModFlags == Parameter.Modifier.PARAMS) {
						ct = TypeManager.GetElementType (ct);
						--c_idx;
					}

					if (best_params && best_pd.FixedParameters[b_idx].ModFlags == Parameter.Modifier.PARAMS) {
						bt = TypeManager.GetElementType (bt);
						--b_idx;
					}
				}

				if (TypeSpecComparer.IsEqual (ct, bt))
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
			if (!same && !a.IsDefaultArgument)
				return false;

			//
			// The two methods have equal non-optional parameter types, apply tie-breaking rules
			//

			//
			// This handles the following cases:
			//
			//  Foo (int i) is better than Foo (int i, long l = 0)
			//  Foo (params int[] args) is better than Foo (int i = 0, params int[] args)
			//
			// Prefer non-optional version
			//
			// LAMESPEC: Specification claims this should be done at last but the opposite is true
			//
			if (candidate_params == best_params && candidate_pd.Count != best_pd.Count) {
				if (candidate_pd.Count >= best_pd.Count)
					return false;

				if (j < candidate_pd.Count && candidate_pd.FixedParameters[j].HasDefaultValue)
					return false;

				return true;
			}

			//
			// One is a non-generic method and second is a generic method, then non-generic is better
			//
			if (best.IsGeneric != candidate.IsGeneric)
				return best.IsGeneric;

			//
			// This handles the following cases:
			//
			//   Trim () is better than Trim (params char[] chars)
			//   Concat (string s1, string s2, string s3) is better than
			//     Concat (string s1, params string [] srest)
			//   Foo (int, params int [] rest) is better than Foo (params int [] rest)
			//
			// Prefer non-expanded version
			//
			if (candidate_params != best_params)
				return best_params;

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
			for (j = 0; j < candidate_param_count; ++j) {
				NamedArgument na = args_count == 0 ? null : args [j] as NamedArgument;
				if (na != null) {
					ct = candidate_def_pd.Types[cparam.GetParameterIndexByName (na.Name)];
					bt = best_def_pd.Types[bparam.GetParameterIndexByName (na.Name)];
				} else {
					ct = candidate_def_pd.Types[j];
					bt = best_def_pd.Types[j];
				}

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

			return false;
		}

		public static void Error_ConstructorMismatch (ResolveContext rc, TypeSpec type, int argCount, Location loc)
		{
			rc.Report.Error (1729, loc,
				"The type `{0}' does not contain a constructor that takes `{1}' arguments",
				type.GetSignatureForError (), argCount.ToString ());
		}

		//
		// Determines if the candidate method is applicable to the given set of arguments
		// There could be two different set of parameters for same candidate where one
		// is the closest override for default values and named arguments checks and second
		// one being the virtual base for the parameter types and modifiers.
		//
		// A return value rates candidate method compatibility,
		// 0 = the best, int.MaxValue = the worst
		//
		int IsApplicable (ResolveContext ec, ref Arguments arguments, int arg_count, ref MemberSpec candidate, IParametersMember pm, ref bool params_expanded_form, ref bool dynamicArgument, ref TypeSpec returnType)
		{
			var pd = pm.Parameters;
			int param_count = pd.Count;
			int optional_count = 0;
			int score;
			Arguments orig_args = arguments;

			if (arg_count != param_count) {
				for (int i = 0; i < pd.Count; ++i) {
					if (pd.FixedParameters[i].HasDefaultValue) {
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

				// Resize to fit optional arguments
				if (optional_count != 0) {
					if (arguments == null) {
						arguments = new Arguments (optional_count);
					} else {
						// Have to create a new container, so the next run can do same
						var resized = new Arguments (param_count);
						resized.AddRange (arguments);
						arguments = resized;
					}

					for (int i = arg_count; i < param_count; ++i)
						arguments.Add (null);
				}
			}

			if (arg_count > 0) {
				//
				// Shuffle named arguments to the right positions if there are any
				//
				if (arguments[arg_count - 1] is NamedArgument) {
					arg_count = arguments.Count;

					for (int i = 0; i < arg_count; ++i) {
						bool arg_moved = false;
						while (true) {
							NamedArgument na = arguments[i] as NamedArgument;
							if (na == null)
								break;

							int index = pd.GetParameterIndexByName (na.Name);

							// Named parameter not found
							if (index < 0)
								return (i + 1) * 3;

							// already reordered
							if (index == i)
								break;

							Argument temp;
							if (index >= param_count) {
								// When using parameters which should not be available to the user
								if ((pd.FixedParameters[index].ModFlags & Parameter.Modifier.PARAMS) == 0)
									break;

								arguments.Add (null);
								++arg_count;
								temp = null;
							} else {
								temp = arguments[index];

								// The slot has been taken by positional argument
								if (temp != null && !(temp is NamedArgument))
									break;
							}

							if (!arg_moved) {
								arguments = arguments.MarkOrderedArgument (na);
								arg_moved = true;
							}

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
			var ms = candidate as MethodSpec;
			if (ms != null && ms.IsGeneric) {
				// Setup constraint checker for probing only
				ConstraintChecker cc = new ConstraintChecker (null);

				if (type_arguments != null) {
					var g_args_count = ms.Arity;
					if (g_args_count != type_arguments.Count)
						return int.MaxValue - 20000 + System.Math.Abs (type_arguments.Count - g_args_count);

					ms = ms.MakeGenericMethod (type_arguments.Arguments);
				} else {
					// TODO: It should not be here (we don't know yet whether any argument is lambda) but
					// for now it simplifies things. I should probably add a callback to ResolveContext
					if (lambda_conv_msgs == null) {
						lambda_conv_msgs = new SessionReportPrinter ();
						prev_recorder = ec.Report.SetPrinter (lambda_conv_msgs);
					}

					var ti = new TypeInference (arguments);
					TypeSpec[] i_args = ti.InferMethodArguments (ec, ms);
					lambda_conv_msgs.EndSession ();

					if (i_args == null)
						return ti.InferenceScore - 20000;

					if (i_args.Length != 0) {
						ms = ms.MakeGenericMethod (i_args);
					}

					cc.IgnoreInferredDynamic = true;
				}

				//
				// Type arguments constraints have to match for the method to be applicable
				//
				if (!cc.CheckAll (ms.GetGenericMethodDefinition (), ms.TypeArguments, ms.Constraints, loc)) {
					candidate = ms;
					return int.MaxValue - 25000;
				}

				//
				// We have a generic return type and at same time the method is override which
				// means we have to also inflate override return type in case the candidate is
				// best candidate and override return type is different to base return type.
				// 
				// virtual Foo<T, object> with override Foo<T, dynamic>
				//
				if (candidate != pm) {
					MethodSpec override_ms = (MethodSpec) pm;
					var inflator = new TypeParameterInflator (ms.DeclaringType, override_ms.GenericDefinition.TypeParameters, ms.TypeArguments);
					returnType = inflator.Inflate (returnType);
				} else {
					returnType = ms.ReturnType;
				}

				candidate = ms;

			} else {
				if (type_arguments != null)
					return int.MaxValue - 15000;
			}

			//
			// 2. Each argument has to be implicitly convertible to method parameter
			//
			Parameter.Modifier p_mod = 0;
			TypeSpec pt = null;
			TypeSpec[] ptypes = ((IParametersMember) candidate).Parameters.Types;

			for (int i = 0; i < arg_count; i++) {
				Argument a = arguments[i];
				if (a == null) {
					if (!pd.FixedParameters[i].HasDefaultValue) {
						arguments = orig_args;
						return arg_count * 2 + 2;
					}

					//
					// Get the default value expression, we can use the same expression
					// if the type matches
					//
					Expression e = pd.FixedParameters[i].DefaultValue;
					if (!(e is Constant) || e.Type.IsGenericOrParentIsGeneric) {
						//
						// LAMESPEC: No idea what the exact rules are for System.Reflection.Missing.Value instead of null
						//
						if (e == EmptyExpression.MissingValue && ptypes[i] == TypeManager.object_type || ptypes[i] == InternalType.Dynamic) {
							e = new MemberAccess (new MemberAccess (new MemberAccess (
								new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "System", loc), "Reflection", loc), "Missing", loc), "Value", loc);
						} else {
							e = new DefaultValueExpression (new TypeExpression (ptypes [i], loc), loc);
						}

						e = e.Resolve (ec);
					}

					arguments[i] = new Argument (e, Argument.AType.Default);
					continue;
				}

				if (p_mod != Parameter.Modifier.PARAMS) {
					p_mod = pd.FixedParameters[i].ModFlags;
					pt = ptypes [i];
				} else if (!params_expanded_form) {
					params_expanded_form = true;
					pt = ((ElementTypeSpec) pt).Element;
					i -= 2;
					continue;
				}

				score = 1;
				if (!params_expanded_form) {
					if (a.ArgType == Argument.AType.ExtensionType) {
						//
						// Indentity, implicit reference or boxing conversion must exist for the extension parameter
						//
						var at = a.Type;
						if (at == pt || TypeSpecComparer.IsEqual (at, pt) ||
							Convert.ImplicitReferenceConversionExists (a.Expr, pt) ||
							Convert.ImplicitBoxingConversion (EmptyExpression.Null, at, pt) != null) {
							score = 0;
							continue;
						}
					} else {
						score = IsArgumentCompatible (ec, a, p_mod & ~Parameter.Modifier.PARAMS, pt);

						if (score < 0)
							dynamicArgument = true;
					}
				}

				//
				// It can be applicable in expanded form (when not doing exact match like for delegates)
				//
				if (score != 0 && (p_mod & Parameter.Modifier.PARAMS) != 0 && (restrictions & Restrictions.CovariantDelegate) == 0) {
					if (!params_expanded_form)
						pt = ((ElementTypeSpec) pt).Element;

					if (score > 0)
						score = IsArgumentCompatible (ec, a, Parameter.Modifier.NONE, pt);

					if (score == 0) {
						params_expanded_form = true;
					} else if (score < 0) {
						params_expanded_form = true;
						dynamicArgument = true;
					}
				}

				if (score > 0) {
					if (params_expanded_form)
						++score;
					return (arg_count - i) * 2 + score;
				}
			}

			//
			// When params parameter has no argument it will be provided later if the method is the best candidate
			//
			if (arg_count + 1 == pd.Count && (pd.FixedParameters [arg_count].ModFlags & Parameter.Modifier.PARAMS) != 0)
				params_expanded_form = true;

			//
			// Restore original arguments for dynamic binder to keep the intention of original source code
			//
			if (dynamicArgument)
				arguments = orig_args;

			return 0;
		}

		//
		// Tests argument compatibility with the parameter
		// The possible return values are
		// 0 - success
		// 1 - modifier mismatch
		// 2 - type mismatch
		// -1 - dynamic binding required
		//
		int IsArgumentCompatible (ResolveContext ec, Argument argument, Parameter.Modifier param_mod, TypeSpec parameter)
		{
			//
			// Types have to be identical when ref or out modifer
			// is used and argument is not of dynamic type
			//
			if ((argument.Modifier | param_mod) != 0) {
				if (argument.Type != parameter) {
					//
					// Do full equality check after quick path
					//
					if (!TypeSpecComparer.IsEqual (argument.Type, parameter)) {
						//
						// Using dynamic for ref/out parameter can still succeed at runtime
						//
						if (argument.Type == InternalType.Dynamic && argument.Modifier == 0 && (restrictions & Restrictions.CovariantDelegate) == 0)
							return -1;

						return 2;
					}
				}

				if (argument.Modifier != param_mod) {
					//
					// Using dynamic for ref/out parameter can still succeed at runtime
					//
					if (argument.Type == InternalType.Dynamic && argument.Modifier == 0 && (restrictions & Restrictions.CovariantDelegate) == 0)
						return -1;

					return 1;
				}

			} else {
				if (argument.Type == InternalType.Dynamic && (restrictions & Restrictions.CovariantDelegate) == 0)
					return -1;

				//
				// Deploy custom error reporting for lambda methods. When probing lambda methods
				// keep all errors reported in separate set and once we are done and no best
				// candidate found, this set is used to report more details about what was wrong
				// with lambda body
				//
				if (argument.Expr.Type == InternalType.AnonymousMethod) {
					if (lambda_conv_msgs == null) {
						lambda_conv_msgs = new SessionReportPrinter ();
						prev_recorder = ec.Report.SetPrinter (lambda_conv_msgs);
					}
				}

				if (!Convert.ImplicitConversionExists (ec, argument.Expr, parameter)) {
					if (lambda_conv_msgs != null) {
						lambda_conv_msgs.EndSession ();
					}

					return 2;
				}
			}

			return 0;
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
				TypeSpec specific = MoreSpecific (ac_p.Element, ac_q.Element);
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

		//
		// Find the best method from candidate list
		//
		public T ResolveMember<T> (ResolveContext rc, ref Arguments args) where T : MemberSpec, IParametersMember
		{
			List<AmbiguousCandidate> ambiguous_candidates = null;

			MemberSpec best_candidate;
			Arguments best_candidate_args = null;
			bool best_candidate_params = false;
			bool best_candidate_dynamic = false;
			int best_candidate_rate;
			IParametersMember best_parameter_member = null;

			int args_count = args != null ? args.Count : 0;

			Arguments candidate_args = args;
			bool error_mode = false;
			var current_type = rc.CurrentType;
			MemberSpec invocable_member = null;

			// Be careful, cannot return until error reporter is restored
			while (true) {
				best_candidate = null;
				best_candidate_rate = int.MaxValue;

				var type_members = members;
				try {

					do {
						for (int i = 0; i < type_members.Count; ++i) {
							var member = type_members[i];

							//
							// Methods in a base class are not candidates if any method in a derived
							// class is applicable
							//
							if ((member.Modifiers & Modifiers.OVERRIDE) != 0)
								continue;

							if (!error_mode) {
								if (!member.IsAccessible (current_type))
									continue;

								if (rc.Compiler.IsRuntimeBinder && !member.DeclaringType.IsAccessible (current_type))
									continue;
							}

							IParametersMember pm = member as IParametersMember;
							if (pm == null) {
								//
								// Will use it later to report ambiguity between best method and invocable member
								//
								if (Invocation.IsMemberInvocable (member))
									invocable_member = member;

								continue;
							}

							//
							// Overload resolution is looking for base member but using parameter names
							// and default values from the closest member. That means to do expensive lookup
							// for the closest override for virtual or abstract members
							//
							if ((member.Modifiers & (Modifiers.VIRTUAL | Modifiers.ABSTRACT)) != 0) {
								var override_params = base_provider.GetOverrideMemberParameters (member);
								if (override_params != null)
									pm = override_params;
							}

							//
							// Check if the member candidate is applicable
							//
							bool params_expanded_form = false;
							bool dynamic_argument = false;
							TypeSpec rt = pm.MemberType;
							int candidate_rate = IsApplicable (rc, ref candidate_args, args_count, ref member, pm, ref params_expanded_form, ref dynamic_argument, ref rt);

							//
							// How does it score compare to others
							//
							if (candidate_rate < best_candidate_rate) {
								best_candidate_rate = candidate_rate;
								best_candidate = member;
								best_candidate_args = candidate_args;
								best_candidate_params = params_expanded_form;
								best_candidate_dynamic = dynamic_argument;
								best_parameter_member = pm;
								best_candidate_return_type = rt;
							} else if (candidate_rate == 0) {
								//
								// The member look is done per type for most operations but sometimes
								// it's not possible like for binary operators overload because they
								// are unioned between 2 sides
								//
								if ((restrictions & Restrictions.BaseMembersIncluded) != 0) {
									if (TypeSpec.IsBaseClass (best_candidate.DeclaringType, member.DeclaringType, true))
										continue;
								}

								// Is the new candidate better
								if (BetterFunction (rc, candidate_args, member, pm.Parameters, params_expanded_form, best_candidate, best_parameter_member.Parameters, best_candidate_params)) {
									best_candidate = member;
									best_candidate_args = candidate_args;
									best_candidate_params = params_expanded_form;
									best_candidate_dynamic = dynamic_argument;
									best_parameter_member = pm;
									best_candidate_return_type = rt;
								} else {
									// It's not better but any other found later could be but we are not sure yet
									if (ambiguous_candidates == null)
										ambiguous_candidates = new List<AmbiguousCandidate> ();

									ambiguous_candidates.Add (new AmbiguousCandidate (member, pm.Parameters, params_expanded_form));
								}
							}

							// Restore expanded arguments
							if (candidate_args != args)
								candidate_args = args;
						}
					} while (best_candidate_rate != 0 && (type_members = base_provider.GetBaseMembers (type_members[0].DeclaringType.BaseType)) != null);
				} finally {
					if (prev_recorder != null)
						rc.Report.SetPrinter (prev_recorder);
				}

				//
				// We've found exact match
				//
				if (best_candidate_rate == 0)
					break;

				//
				// Try extension methods lookup when no ordinary method match was found and provider enables it
				//
				if (!error_mode) {
					var emg = base_provider.LookupExtensionMethod (rc);
					if (emg != null) {
						emg = emg.OverloadResolve (rc, ref args, null, restrictions);
						if (emg != null) {
							best_candidate_extension_group = emg;
							return (T) (MemberSpec) emg.BestCandidate;
						}
					}
				}

				// Don't run expensive error reporting mode for probing
				if (IsProbingOnly)
					return null;

				if (error_mode)
					break;

				lambda_conv_msgs = null;
				error_mode = true;
			}

			//
			// No best member match found, report an error
			//
			if (best_candidate_rate != 0 || error_mode) {
				ReportOverloadError (rc, best_candidate, best_parameter_member, best_candidate_args, best_candidate_params);
				return null;
			}

			if (best_candidate_dynamic) {
				if (args[0].ArgType == Argument.AType.ExtensionType) {
					rc.Report.Error (1973, loc,
						"Type `{0}' does not contain a member `{1}' and the best extension method overload `{2}' cannot be dynamically dispatched. Consider calling the method without the extension method syntax",
						args [0].Type.GetSignatureForError (), best_candidate.Name, best_candidate.GetSignatureForError ());
				}

				BestCandidateIsDynamic = true;
				return null;
			}

			if (ambiguous_candidates != null) {
				//
				// Now check that there are no ambiguities i.e the selected method
				// should be better than all the others
				//
				for (int ix = 0; ix < ambiguous_candidates.Count; ix++) {
					var candidate = ambiguous_candidates [ix];

					if (!BetterFunction (rc, best_candidate_args, best_candidate, best_parameter_member.Parameters, best_candidate_params, candidate.Member, candidate.Parameters, candidate.Expanded)) {
						var ambiguous = candidate.Member;
						if (custom_errors == null || !custom_errors.AmbiguousCandidates (rc, best_candidate, ambiguous)) {
							rc.Report.SymbolRelatedToPreviousError (best_candidate);
							rc.Report.SymbolRelatedToPreviousError (ambiguous);
							rc.Report.Error (121, loc, "The call is ambiguous between the following methods or properties: `{0}' and `{1}'",
								best_candidate.GetSignatureForError (), ambiguous.GetSignatureForError ());
						}

						return (T) best_candidate;
					}
				}
			}

			if (invocable_member != null) {
				rc.Report.SymbolRelatedToPreviousError (best_candidate);
				rc.Report.SymbolRelatedToPreviousError (invocable_member);
				rc.Report.Warning (467, 2, loc, "Ambiguity between method `{0}' and invocable non-method `{1}'. Using method group",
					best_candidate.GetSignatureForError (), invocable_member.GetSignatureForError ());
			}

			//
			// And now check if the arguments are all
			// compatible, perform conversions if
			// necessary etc. and return if everything is
			// all right
			//
			if (!VerifyArguments (rc, ref best_candidate_args, best_candidate, best_parameter_member, best_candidate_params))
				return null;

			if (best_candidate == null)
				return null;

			//
			// Check ObsoleteAttribute on the best method
			//
			ObsoleteAttribute oa = best_candidate.GetAttributeObsolete ();
			if (oa != null && !rc.IsObsolete)
				AttributeTester.Report_ObsoleteMessage (oa, best_candidate.GetSignatureForError (), loc, rc.Report);

			best_candidate.MemberDefinition.SetIsUsed ();

			args = best_candidate_args;
			return (T) best_candidate;
		}

		public MethodSpec ResolveOperator (ResolveContext rc, ref Arguments args)
		{
			return ResolveMember<MethodSpec> (rc, ref args);
		}

		void ReportArgumentMismatch (ResolveContext ec, int idx, MemberSpec method,
													Argument a, AParametersCollection expected_par, TypeSpec paramType)
		{
			if (custom_errors != null && custom_errors.ArgumentMismatch (ec, method, a, idx))
				return;

			if (a is CollectionElementInitializer.ElementInitializerArgument) {
				ec.Report.SymbolRelatedToPreviousError (method);
				if ((expected_par.FixedParameters[idx].ModFlags & Parameter.Modifier.ISBYREF) != 0) {
					ec.Report.Error (1954, loc, "The best overloaded collection initalizer method `{0}' cannot have 'ref', or `out' modifier",
						TypeManager.CSharpSignature (method));
					return;
				}
				ec.Report.Error (1950, loc, "The best overloaded collection initalizer method `{0}' has some invalid arguments",
					  TypeManager.CSharpSignature (method));
			} else if (IsDelegateInvoke) {
				ec.Report.Error (1594, loc, "Delegate `{0}' has some invalid arguments",
					DelegateType.GetSignatureForError ());
			} else {
				ec.Report.SymbolRelatedToPreviousError (method);
				ec.Report.Error (1502, loc, "The best overloaded method match for `{0}' has some invalid arguments",
					method.GetSignatureForError ());
			}

			Parameter.Modifier mod = idx >= expected_par.Count ? 0 : expected_par.FixedParameters[idx].ModFlags;

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

				ec.Report.Error (1503, loc,
					"Argument `#{0}' cannot convert `{1}' expression to type `{2}'", index, p1, p2);
			}
		}

		//
		// We have failed to find exact match so we return error info about the closest match
		//
		void ReportOverloadError (ResolveContext rc, MemberSpec best_candidate, IParametersMember pm, Arguments args, bool params_expanded)
		{
			int ta_count = type_arguments == null ? 0 : type_arguments.Count;
			int arg_count = args == null ? 0 : args.Count;

			if (ta_count != best_candidate.Arity && (ta_count > 0 || ((IParametersMember) best_candidate).Parameters.IsEmpty)) {
				var mg = new MethodGroupExpr (new [] { best_candidate }, best_candidate.DeclaringType, loc);
				mg.Error_TypeArgumentsCannotBeUsed (rc.Report, loc, best_candidate, ta_count);
				return;
			}

			if (lambda_conv_msgs != null) {
				if (lambda_conv_msgs.Merge (rc.Report.Printer))
					return;
			}

			//
			// For candidates which match on parameters count report more details about incorrect arguments
			//
			if (pm != null) {
				int unexpanded_count = pm.Parameters.HasParams ? pm.Parameters.Count - 1 : pm.Parameters.Count;
				if (pm.Parameters.Count == arg_count || params_expanded || unexpanded_count == arg_count) {
					// Reject any inaccessible member
					if (!best_candidate.IsAccessible (rc.CurrentType) || !best_candidate.DeclaringType.IsAccessible (rc.CurrentType)) {
						rc.Report.SymbolRelatedToPreviousError (best_candidate);
						Expression.ErrorIsInaccesible (rc, best_candidate.GetSignatureForError (), loc);
						return;
					}

					var ms = best_candidate as MethodSpec;
					if (ms != null && ms.IsGeneric) {
						bool constr_ok = true;
						if (ms.TypeArguments != null)
							constr_ok = new ConstraintChecker (rc.MemberContext).CheckAll (ms.GetGenericMethodDefinition (), ms.TypeArguments, ms.Constraints, loc);

						if (ta_count == 0) {
							if (custom_errors != null && custom_errors.TypeInferenceFailed (rc, best_candidate))
								return;

							if (constr_ok) {
								rc.Report.Error (411, loc,
									"The type arguments for method `{0}' cannot be inferred from the usage. Try specifying the type arguments explicitly",
									ms.GetGenericMethodDefinition ().GetSignatureForError ());
							}

							return;
						}
					}

					VerifyArguments (rc, ref args, best_candidate, pm, params_expanded);
					return;
				}
			}

			//
			// We failed to find any method with correct argument count, report best candidate
			//
			if (custom_errors != null && custom_errors.NoArgumentMatch (rc, best_candidate))
				return;

			if (best_candidate.Kind == MemberKind.Constructor) {
				rc.Report.SymbolRelatedToPreviousError (best_candidate);
				Error_ConstructorMismatch (rc, best_candidate.DeclaringType, arg_count, loc);
			} else if (IsDelegateInvoke) {
				rc.Report.SymbolRelatedToPreviousError (DelegateType);
				rc.Report.Error (1593, loc, "Delegate `{0}' does not take `{1}' arguments",
					DelegateType.GetSignatureForError (), arg_count.ToString ());
			} else {
				string name = best_candidate.Kind == MemberKind.Indexer ? "this" : best_candidate.Name;
				rc.Report.SymbolRelatedToPreviousError (best_candidate);
				rc.Report.Error (1501, loc, "No overload for method `{0}' takes `{1}' arguments",
					name, arg_count.ToString ());
			}
		}

		bool VerifyArguments (ResolveContext ec, ref Arguments args, MemberSpec member, IParametersMember pm, bool chose_params_expanded)
		{
			var pd = pm.Parameters;
			TypeSpec[] ptypes = ((IParametersMember) member).Parameters.Types;

			Parameter.Modifier p_mod = 0;
			TypeSpec pt = null;
			int a_idx = 0, a_pos = 0;
			Argument a = null;
			ArrayInitializer params_initializers = null;
			bool has_unsafe_arg = pm.MemberType.IsPointer;
			int arg_count = args == null ? 0 : args.Count;

			for (; a_idx < arg_count; a_idx++, ++a_pos) {
				a = args[a_idx];
				if (p_mod != Parameter.Modifier.PARAMS) {
					p_mod = pd.FixedParameters[a_idx].ModFlags;
					pt = ptypes[a_idx];
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

					if (a.Expr.Type == pt || TypeSpecComparer.IsEqual (a.Expr.Type, pt))
						continue;

					break;
				}

				NamedArgument na = a as NamedArgument;
				if (na != null) {
					int name_index = pd.GetParameterIndexByName (na.Name);
					if (name_index < 0 || name_index >= pd.Count) {
						if (IsDelegateInvoke) {
							ec.Report.SymbolRelatedToPreviousError (DelegateType);
							ec.Report.Error (1746, na.Location,
								"The delegate `{0}' does not contain a parameter named `{1}'",
								DelegateType.GetSignatureForError (), na.Name);
						} else {
							ec.Report.SymbolRelatedToPreviousError (member);
							ec.Report.Error (1739, na.Location,
								"The best overloaded method match for `{0}' does not contain a parameter named `{1}'",
								TypeManager.CSharpSignature (member), na.Name);
						}
					} else if (args[name_index] != a) {
						if (IsDelegateInvoke)
							ec.Report.SymbolRelatedToPreviousError (DelegateType);
						else
							ec.Report.SymbolRelatedToPreviousError (member);

						ec.Report.Error (1744, na.Location,
							"Named argument `{0}' cannot be used for a parameter which has positional argument specified",
							na.Name);
					}
				}
				
				if (a.Expr.Type == InternalType.Dynamic)
					continue;

				if ((restrictions & Restrictions.CovariantDelegate) != 0 && !Delegate.IsTypeCovariant (a.Expr, pt)) {
					custom_errors.NoArgumentMatch (ec, member);
					return false;
				}

				Expression conv = null;
				if (a.ArgType == Argument.AType.ExtensionType) {
					if (a.Expr.Type == pt || TypeSpecComparer.IsEqual (a.Expr.Type, pt)) {
						conv = a.Expr;
					} else {
						conv = Convert.ImplicitReferenceConversion (a.Expr, pt, false);
						if (conv == null)
							conv = Convert.ImplicitBoxingConversion (a.Expr, a.Expr.Type, pt);
					}
				} else {
					conv = Convert.ImplicitConversion (ec, a.Expr, pt, loc);
				}

				if (conv == null)
					break;

				//
				// Convert params arguments to an array initializer
				//
				if (params_initializers != null) {
					// we choose to use 'a.Expr' rather than 'conv' so that
					// we don't hide the kind of expression we have (esp. CompoundAssign.Helper)
					params_initializers.Add (a.Expr);
					args.RemoveAt (a_idx--);
					--arg_count;
					continue;
				}

				// Update the argument with the implicit conversion
				a.Expr = conv;
			}

			if (a_idx != arg_count) {
				ReportArgumentMismatch (ec, a_pos, member, a, pd, pt);
				return false;
			}

			//
			// Fill not provided arguments required by params modifier
			//
			if (params_initializers == null && pd.HasParams && arg_count + 1 == pd.Count) {
				if (args == null)
					args = new Arguments (1);

				pt = ptypes[pd.Count - 1];
				pt = TypeManager.GetElementType (pt);
				has_unsafe_arg |= pt.IsPointer;
				params_initializers = new ArrayInitializer (0, loc);
			}

			//
			// Append an array argument with all params arguments
			//
			if (params_initializers != null) {
				args.Add (new Argument (
					new ArrayCreation (new TypeExpression (pt, loc), params_initializers, loc).Resolve (ec)));
				arg_count++;
			}

			if (has_unsafe_arg && !ec.IsUnsafe) {
				Expression.UnsafeError (ec, loc);
			}

			//
			// We could infer inaccesible type arguments
			//
			if (type_arguments == null && member.IsGeneric) {
				var ms = (MethodSpec) member;
				foreach (var ta in ms.TypeArguments) {
					if (!ta.IsAccessible (ec.CurrentType)) {
						ec.Report.SymbolRelatedToPreviousError (ta);
						Expression.ErrorIsInaccesible (ec, member.GetSignatureForError (), loc);
						break;
					}
				}
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

		protected override TypeSpec DeclaringType {
			get { return constant.DeclaringType; }
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			ResolveInstanceExpression (rc, null);
			DoBestMemberChecks (rc, constant);

			var c = constant.GetConstant (rc);

			// Creates reference expression to the constant value
			return Constant.CreateConstant (rc, constant.MemberType, c.GetValue (), loc);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}

		public override string GetSignatureForError ()
		{
			return constant.GetSignatureForError ();
		}

		public override void SetTypeArguments (ResolveContext ec, TypeArguments ta)
		{
			Error_TypeArgumentsCannotBeUsed (ec.Report, "constant", GetSignatureForError (), loc);
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
		}

		#region Properties

		public override string Name {
			get {
				return spec.Name;
			}
		}

		public bool IsHoisted {
			get {
				IVariableReference hv = InstanceExpression as IVariableReference;
				return hv != null && hv.IsHoisted;
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

		protected override TypeSpec DeclaringType {
			get {
				return spec.DeclaringType;
			}
		}

		public VariableInfo VariableInfo {
			get {
				return variable_info;
			}
		}

#endregion

		public override string GetSignatureForError ()
		{
			return TypeManager.GetFullNameSignature (spec);
		}

		public bool IsMarshalByRefAccess ()
		{
			// Checks possible ldflda of field access expression
			return !spec.IsStatic && TypeManager.IsValueType (spec.MemberType) &&
				TypeSpec.IsBaseClass (spec.DeclaringType, TypeManager.mbr_type, false) &&
				!(InstanceExpression is This);
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
			return DoResolve (ec, null);
		}

		Expression DoResolve (ResolveContext ec, Expression rhs)
		{
			bool lvalue_instance = rhs != null && IsInstance && spec.DeclaringType.IsStruct;

			if (ResolveInstanceExpression (ec, rhs)) {
				// Resolve the field's instance expression while flow analysis is turned
				// off: when accessing a field "a.b", we must check whether the field
				// "a.b" is initialized, not whether the whole struct "a" is initialized.

				if (lvalue_instance) {
					using (ec.With (ResolveContext.Options.DoFlowAnalysis, false)) {
						bool out_access = rhs == EmptyExpression.OutAccess.Instance || rhs == EmptyExpression.LValueMemberOutAccess;

						Expression right_side =
							out_access ? EmptyExpression.LValueMemberOutAccess : EmptyExpression.LValueMemberAccess;

						InstanceExpression = InstanceExpression.ResolveLValue (ec, right_side);
					}
				} else {
					using (ec.With (ResolveContext.Options.DoFlowAnalysis, false)) {
						InstanceExpression = InstanceExpression.Resolve (ec, ResolveFlags.VariableOrValue);
					}
				}

				if (InstanceExpression == null)
					return null;
			}

			DoBestMemberChecks (ec, spec);

			var fb = spec as FixedFieldSpec;
			IVariableReference var = InstanceExpression as IVariableReference;

			if (lvalue_instance && var != null && var.VariableInfo != null) {
				var.VariableInfo.SetFieldAssigned (ec, Name);
			}
			
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
			Expression e = DoResolve (ec, right_side);

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
					if (ec.CurrentMemberDefinition.Parent.Definition != spec.DeclaringType.GetDefinition ())
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
				!IsStatic && !(InstanceExpression is This) && TypeManager.mbr_type != null && TypeSpec.IsBaseClass (spec.DeclaringType, TypeManager.mbr_type, false)) {
				ec.Report.SymbolRelatedToPreviousError (spec.DeclaringType);
				ec.Report.Warning (197, 1, loc,
						"Passing `{0}' as ref or out or taking its address may cause a runtime exception because it is a field of a marshal-by-reference class",
						GetSignatureForError ());
			}

			eclass = ExprClass.Variable;
			return this;
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
				if (TypeManager.IsStruct (type) && type == ec.MemberContext.CurrentType && InstanceExpression.Type == type) {
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
			prepared = prepare_for_load && !(source is DynamicExpressionStatement);
			if (IsInstance)
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

			if (is_volatile) // || is_marshal_by_ref ())
				base.EmitSideEffect (ec);
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

		public SLE.Expression MakeAssignExpression (BuilderContext ctx, Expression source)
		{
			return MakeExpression (ctx);
		}

		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			return SLE.Expression.Field (
				IsStatic ? null : InstanceExpression.MakeExpression (ctx),
				spec.GetMetaInfo ());
		}

		public override void SetTypeArguments (ResolveContext ec, TypeArguments ta)
		{
			Error_TypeArgumentsCannotBeUsed (ec.Report, "field", GetSignatureForError (), loc);
		}
	}

	
	/// <summary>
	///   Expression that evaluates to a Property.  The Assign class
	///   might set the `Value' expression if we are in an assignment.
	///
	///   This is not an LValue because we need to re-write the expression, we
	///   can not take data from the stack and store it.  
	/// </summary>
	class PropertyExpr : PropertyOrIndexerExpr<PropertySpec>
	{
		public PropertyExpr (PropertySpec spec, Location l)
			: base (l)
		{
			best_candidate = spec;
			type = spec.MemberType;
		}

		#region Properties

		protected override TypeSpec DeclaringType {
			get {
				return best_candidate.DeclaringType;
			}
		}

		public override string Name {
			get {
				return best_candidate.Name;
			}
		}

		public override bool IsInstance {
			get {
				return !IsStatic;
			}
		}

		public override bool IsStatic {
			get {
				return best_candidate.IsStatic;
			}
		}

		public PropertySpec PropertyInfo {
			get {
				return best_candidate;
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

			args = new Arguments (2);
			if (InstanceExpression == null)
				args.Add (new Argument (new NullLiteral (loc)));
			else
				args.Add (new Argument (InstanceExpression.CreateExpressionTree (ec)));
			args.Add (new Argument (new TypeOfMethod (Getter, loc)));
			return CreateExpressionFactoryCall (ec, "Property", args);
		}

		public Expression CreateSetterTypeOfExpression ()
		{
			return new TypeOfMethod (Setter, loc);
		}

		public override string GetSignatureForError ()
		{
			return best_candidate.GetSignatureForError ();
		}

		public override SLE.Expression MakeAssignExpression (BuilderContext ctx, Expression source)
		{
			return SLE.Expression.Property (InstanceExpression.MakeExpression (ctx), (MethodInfo) Setter.GetMetaInfo ());
		}

		public override SLE.Expression MakeExpression (BuilderContext ctx)
		{
			return SLE.Expression.Property (InstanceExpression.MakeExpression (ctx), (MethodInfo) Getter.GetMetaInfo ());
		}

		void Error_PropertyNotValid (ResolveContext ec)
		{
			ec.Report.SymbolRelatedToPreviousError (best_candidate);
			ec.Report.Error (1546, loc, "Property or event `{0}' is not supported by the C# language",
				GetSignatureForError ());
		}

		bool IsSingleDimensionalArrayLength ()
		{
			if (best_candidate.DeclaringType != TypeManager.array_type || !best_candidate.HasGet || Name != "Length")
				return false;

			ArrayContainer ac = InstanceExpression.Type as ArrayContainer;
			return ac != null && ac.Rank == 1;
		}

		public override void Emit (EmitContext ec, bool leave_copy)
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

			Invocation.EmitCall (ec, InstanceExpression, Getter, null, loc, prepared, false);
			
			if (leave_copy) {
				ec.Emit (OpCodes.Dup);
				if (!IsStatic) {
					temp = new LocalTemporary (this.Type);
					temp.Store (ec);
				}
			}
		}

		public override void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			Arguments args;

			if (prepare_for_load && !(source is DynamicExpressionStatement)) {
				args = new Arguments (0);
				prepared = true;
				source.Emit (ec);
				
				if (leave_copy) {
					ec.Emit (OpCodes.Dup);
					if (!IsStatic) {
						temp = new LocalTemporary (this.Type);
						temp.Store (ec);
					}
				}
			} else {
				args = new Arguments (1);

				if (leave_copy) {
					source.Emit (ec);
					temp = new LocalTemporary (this.Type);
					temp.Store (ec);
					args.Add (new Argument (temp));
				} else {
					args.Add (new Argument (source));
				}
			}

			Invocation.EmitCall (ec, InstanceExpression, Setter, args, loc, false, prepared);
			
			if (temp != null) {
				temp.Emit (ec);
				temp.Release (ec);
			}
		}

		protected override Expression OverloadResolve (ResolveContext rc, Expression right_side)
		{
			eclass = ExprClass.PropertyAccess;

			if (best_candidate.IsNotRealProperty) {
				Error_PropertyNotValid (rc);
			}

			ResolveInstanceExpression (rc, right_side);

			if ((best_candidate.Modifiers & (Modifiers.ABSTRACT | Modifiers.VIRTUAL)) != 0 && best_candidate.DeclaringType != InstanceExpression.Type) {
				var filter = new MemberFilter (best_candidate.Name, 0, MemberKind.Property, null, null);
				var p = MemberCache.FindMember (InstanceExpression.Type, filter, BindingRestriction.InstanceOnly | BindingRestriction.OverrideOnly) as PropertySpec;
				if (p != null) {
					type = p.MemberType;
				}
			}

			DoBestMemberChecks (rc, best_candidate);
			return this;
		}

		public override void SetTypeArguments (ResolveContext ec, TypeArguments ta)
		{
			Error_TypeArgumentsCannotBeUsed (ec.Report, "property", GetSignatureForError (), loc);
		}
	}

	abstract class PropertyOrIndexerExpr<T> : MemberExpr, IDynamicAssign where T : PropertySpec
	{
		// getter and setter can be different for base calls
		MethodSpec getter, setter;
		protected T best_candidate;

		protected LocalTemporary temp;
		protected bool prepared;

		protected PropertyOrIndexerExpr (Location l)
		{
			loc = l;
		}

		#region Properties

		public MethodSpec Getter {
			get {
				return getter;
			}
			set {
				getter = value;
			}
		}

		public MethodSpec Setter {
			get {
				return setter;
			}
			set {
				setter = value;
			}
		}

		#endregion

		protected override Expression DoResolve (ResolveContext ec)
		{
			if (eclass == ExprClass.Unresolved) {
				var expr = OverloadResolve (ec, null);
				if (expr == null)
					return null;

				if (expr != this)
					return expr.Resolve (ec);
			}

			if (!ResolveGetter (ec))
				return null;

			return this;
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			if (right_side == EmptyExpression.OutAccess.Instance) {
				// TODO: best_candidate can be null at this point
				INamedBlockVariable variable = null;
				if (best_candidate != null && ec.CurrentBlock.ParametersBlock.TopBlock.GetLocalName (best_candidate.Name, ec.CurrentBlock, ref variable) && variable is Linq.RangeVariable) {
					ec.Report.Error (1939, loc, "A range variable `{0}' may not be passes as `ref' or `out' parameter",
						best_candidate.Name);
				} else {
					right_side.DoResolveLValue (ec, this);
				}
				return null;
			}

			// if the property/indexer returns a value type, and we try to set a field in it
			if (right_side == EmptyExpression.LValueMemberAccess || right_side == EmptyExpression.LValueMemberOutAccess) {
				Error_CannotModifyIntermediateExpressionValue (ec);
			}

			if (eclass == ExprClass.Unresolved) {
				var expr = OverloadResolve (ec, right_side);
				if (expr == null)
					return null;

				if (expr != this)
					return expr.ResolveLValue (ec, right_side);
			}

			if (!ResolveSetter (ec))
				return null;

			return this;
		}

		//
		// Implements the IAssignMethod interface for assignments
		//
		public abstract void Emit (EmitContext ec, bool leave_copy);
		public abstract void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load);

		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}

		public abstract SLE.Expression MakeAssignExpression (BuilderContext ctx, Expression source);

		protected abstract Expression OverloadResolve (ResolveContext rc, Expression right_side);

		bool ResolveGetter (ResolveContext rc)
		{
			if (!best_candidate.HasGet) {
				if (InstanceExpression != EmptyExpression.Null) {
					rc.Report.SymbolRelatedToPreviousError (best_candidate);
					rc.Report.Error (154, loc, "The property or indexer `{0}' cannot be used in this context because it lacks the `get' accessor",
						best_candidate.GetSignatureForError ());
					return false;
				}
			} else if (!best_candidate.Get.IsAccessible (rc.CurrentType)) {
				if (best_candidate.HasDifferentAccessibility) {
					rc.Report.SymbolRelatedToPreviousError (best_candidate.Get);
					rc.Report.Error (271, loc, "The property or indexer `{0}' cannot be used in this context because the get accessor is inaccessible",
						TypeManager.CSharpSignature (best_candidate));
				} else {
					rc.Report.SymbolRelatedToPreviousError (best_candidate.Get);
					ErrorIsInaccesible (rc, best_candidate.Get.GetSignatureForError (), loc);
				}
			}

			if (best_candidate.HasDifferentAccessibility) {
				CheckProtectedMemberAccess (rc, best_candidate.Get);
			}

			getter = CandidateToBaseOverride (rc, best_candidate.Get);
			return true;
		}

		bool ResolveSetter (ResolveContext rc)
		{
			if (!best_candidate.HasSet) {
				rc.Report.Error (200, loc, "Property or indexer `{0}' cannot be assigned to (it is read-only)",
					GetSignatureForError ());
				return false;
			}

			if (!best_candidate.Set.IsAccessible (rc.CurrentType)) {
				if (best_candidate.HasDifferentAccessibility) {
					rc.Report.SymbolRelatedToPreviousError (best_candidate.Set);
					rc.Report.Error (272, loc, "The property or indexer `{0}' cannot be used in this context because the set accessor is inaccessible",
						GetSignatureForError ());
				} else {
					rc.Report.SymbolRelatedToPreviousError (best_candidate.Set);
					ErrorIsInaccesible (rc, best_candidate.Set.GetSignatureForError (), loc);
				}
			}

			if (best_candidate.HasDifferentAccessibility)
				CheckProtectedMemberAccess (rc, best_candidate.Set);

			setter = CandidateToBaseOverride (rc, best_candidate.Set);
			return true;
		}
	}

	/// <summary>
	///   Fully resolved expression that evaluates to an Event
	/// </summary>
	public class EventExpr : MemberExpr, IAssignMethod
	{
		readonly EventSpec spec;
		MethodSpec op;

		public EventExpr (EventSpec spec, Location loc)
		{
			this.spec = spec;
			this.loc = loc;
		}

		#region Properties

		protected override TypeSpec DeclaringType {
			get {
				return spec.DeclaringType;
			}
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

		public MethodSpec Operator {
			get {
				return op;
			}
		}

		#endregion

		public override MemberExpr ResolveMemberAccess (ResolveContext ec, Expression left, SimpleName original)
		{
			//
			// If the event is local to this class and we are not lhs of +=/-= we transform ourselves into a FieldExpr
			//
			if (!ec.HasSet (ResolveContext.Options.CompoundAssignmentScope)) {
				if (spec.BackingField != null &&
					(spec.DeclaringType == ec.CurrentType || TypeManager.IsNestedChildOf (ec.CurrentType, spec.DeclaringType))) {

					spec.MemberDefinition.SetIsUsed ();

					if (!ec.IsObsolete) {
						ObsoleteAttribute oa = spec.GetAttributeObsolete ();
						if (oa != null)
							AttributeTester.Report_ObsoleteMessage (oa, spec.GetSignatureForError (), loc, ec.Report);
					}

					if ((spec.Modifiers & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
						Error_AssignmentEventOnly (ec);

					FieldExpr ml = new FieldExpr (spec.BackingField, loc);

					InstanceExpression = null;

					return ml.ResolveMemberAccess (ec, left, original);
				}
			}

			return base.ResolveMemberAccess (ec, left, original);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override Expression DoResolveLValue (ResolveContext ec, Expression right_side)
		{
			if (right_side == EmptyExpression.EventAddition) {
				op = spec.AccessorAdd;
			} else if (right_side == EmptyExpression.EventSubtraction) {
				op = spec.AccessorRemove;
			}

			if (op == null) {
				Error_AssignmentEventOnly (ec);
				return null;
			}

			op = CandidateToBaseOverride (ec, op);
			return this;
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			eclass = ExprClass.EventAccess;
			type = spec.MemberType;

			ResolveInstanceExpression (ec, null);

			if (!ec.HasSet (ResolveContext.Options.CompoundAssignmentScope)) {
				Error_AssignmentEventOnly (ec);
			}

			DoBestMemberChecks (ec, spec);
			return this;
		}		

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
			//Error_CannotAssign ();
		}

		#region IAssignMethod Members

		public void Emit (EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException ();
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			if (leave_copy || !prepare_for_load)
				throw new NotImplementedException ("EventExpr::EmitAssign");

			Arguments args = new Arguments (1);
			args.Add (new Argument (source));
			Invocation.EmitCall (ec, InstanceExpression, op, args, loc);
		}

		#endregion

		void Error_AssignmentEventOnly (ResolveContext ec)
		{
			if (spec.DeclaringType == ec.CurrentType || TypeManager.IsNestedChildOf (ec.CurrentType, spec.DeclaringType)) {
				ec.Report.Error (79, loc,
					"The event `{0}' can only appear on the left hand side of `+=' or `-=' operator",
					GetSignatureForError ());
			} else {
				ec.Report.Error (70, loc,
					"The event `{0}' can only appear on the left hand side of += or -= when used outside of the type `{1}'",
					GetSignatureForError (), spec.DeclaringType.GetSignatureForError ());
			}
		}

		protected override void Error_CannotCallAbstractBase (ResolveContext rc, string name)
		{
			name = name.Substring (0, name.LastIndexOf ('.'));
			base.Error_CannotCallAbstractBase (rc, name);
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpSignature (spec);
		}

		public override void SetTypeArguments (ResolveContext ec, TypeArguments ta)
		{
			Error_TypeArgumentsCannotBeUsed (ec.Report, "event", GetSignatureForError (), loc);
		}
	}

	public class TemporaryVariableReference : VariableReference
	{
		public class Declarator : Statement
		{
			TemporaryVariableReference variable;

			public Declarator (TemporaryVariableReference variable)
			{
				this.variable = variable;
				loc = variable.loc;
			}

			protected override void DoEmit (EmitContext ec)
			{
				variable.li.CreateBuilder (ec);
			}

			protected override void CloneTo (CloneContext clonectx, Statement target)
			{
				// Nothing
			}
		}

		LocalVariable li;

		public TemporaryVariableReference (LocalVariable li, Location loc)
		{
			this.li = li;
			this.type = li.Type;
			this.loc = loc;
		}

		public override bool IsLockedByStatement {
			get {
				return false;
			}
			set {
			}
		}

		public LocalVariable LocalInfo {
		    get {
		        return li;
		    }
		}

		public static TemporaryVariableReference Create (TypeSpec type, Block block, Location loc)
		{
			var li = LocalVariable.CreateCompilerGenerated (type, block, loc);
			return new TemporaryVariableReference (li, loc);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			eclass = ExprClass.Variable;

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
			li.CreateBuilder (ec);

			Emit (ec, false);
		}

		public void EmitAssign (EmitContext ec, Expression source)
		{
			li.CreateBuilder (ec);

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
		public VarExpr (Location loc)
			: base ("var", loc)
		{
		}

		public bool InferType (ResolveContext ec, Expression right_side)
		{
			if (type != null)
				throw new InternalErrorException ("An implicitly typed local variable could not be redefined");
			
			type = right_side.Type;
			if (type == InternalType.Null || type == TypeManager.void_type || type == InternalType.AnonymousMethod || type == InternalType.MethodGroup) {
				ec.Report.Error (815, loc,
					"An implicitly typed local variable declaration cannot be initialized with `{0}'",
					type.GetSignatureForError ());
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
	}
}	
