//
// generic.cs: Generics support
//
// Authors: Martin Baulig (martin@ximian.com)
//          Miguel de Icaza (miguel@ximian.com)
//          Marek Safar (marek.safar@gmail.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2004 Novell, Inc
//
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
	
namespace Mono.CSharp {

	/// <summary>
	///   Abstract base class for type parameter constraints.
	///   The type parameter can come from a generic type definition or from reflection.
	/// </summary>
	public abstract class GenericConstraints {
		public abstract string TypeParameter {
			get;
		}

		public abstract GenericParameterAttributes Attributes {
			get;
		}

		public bool HasConstructorConstraint {
			get { return (Attributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0; }
		}

		public bool HasReferenceTypeConstraint {
			get { return (Attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0; }
		}

		public bool HasValueTypeConstraint {
			get { return (Attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0; }
		}

		public virtual bool HasClassConstraint {
			get { return ClassConstraint != null; }
		}

		public abstract Type ClassConstraint {
			get;
		}

		public abstract Type[] InterfaceConstraints {
			get;
		}

		public abstract Type EffectiveBaseClass {
			get;
		}

		// <summary>
		//   Returns whether the type parameter is "known to be a reference type".
		// </summary>
		public virtual bool IsReferenceType {
			get {
				if (HasReferenceTypeConstraint)
					return true;
				if (HasValueTypeConstraint)
					return false;

				if (ClassConstraint != null) {
					if (ClassConstraint.IsValueType)
						return false;

					if (ClassConstraint != TypeManager.object_type)
						return true;
				}

				foreach (Type t in InterfaceConstraints) {
					if (!t.IsGenericParameter)
						continue;

					GenericConstraints gc = TypeManager.GetTypeParameterConstraints (t);
					if ((gc != null) && gc.IsReferenceType)
						return true;
				}

				return false;
			}
		}

		// <summary>
		//   Returns whether the type parameter is "known to be a value type".
		// </summary>
		public virtual bool IsValueType {
			get {
				if (HasValueTypeConstraint)
					return true;
				if (HasReferenceTypeConstraint)
					return false;

				if (ClassConstraint != null) {
					if (!ClassConstraint.IsValueType)
						return false;

					if (ClassConstraint != TypeManager.value_type)
						return true;
				}

				foreach (Type t in InterfaceConstraints) {
					if (!t.IsGenericParameter)
						continue;

					GenericConstraints gc = TypeManager.GetTypeParameterConstraints (t);
					if ((gc != null) && gc.IsValueType)
						return true;
				}

				return false;
			}
		}
	}

	public enum SpecialConstraint
	{
		Constructor,
		ReferenceType,
		ValueType
	}

	/// <summary>
	///   Tracks the constraints for a type parameter from a generic type definition.
	/// </summary>
	public class Constraints : GenericConstraints {
		string name;
		ArrayList constraints;
		Location loc;
		
		//
		// name is the identifier, constraints is an arraylist of
		// Expressions (with types) or `true' for the constructor constraint.
		// 
		public Constraints (string name, ArrayList constraints,
				    Location loc)
		{
			this.name = name;
			this.constraints = constraints;
			this.loc = loc;
		}

		public override string TypeParameter {
			get {
				return name;
			}
		}

		public Constraints Clone ()
		{
			return new Constraints (name, constraints, loc);
		}

		GenericParameterAttributes attrs;
		TypeExpr class_constraint;
		ArrayList iface_constraints;
		ArrayList type_param_constraints;
		int num_constraints;
		Type class_constraint_type;
		Type[] iface_constraint_types;
		Type effective_base_type;
		bool resolved;
		bool resolved_types;

		/// <summary>
		///   Resolve the constraints - but only resolve things into Expression's, not
		///   into actual types.
		/// </summary>
		public bool Resolve (IResolveContext ec)
		{
			if (resolved)
				return true;

			iface_constraints = new ArrayList ();
			type_param_constraints = new ArrayList ();

			foreach (object obj in constraints) {
				if (HasConstructorConstraint) {
					Report.Error (401, loc,
						      "The new() constraint must be the last constraint specified");
					return false;
				}

				if (obj is SpecialConstraint) {
					SpecialConstraint sc = (SpecialConstraint) obj;

					if (sc == SpecialConstraint.Constructor) {
						if (!HasValueTypeConstraint) {
							attrs |= GenericParameterAttributes.DefaultConstructorConstraint;
							continue;
						}

						Report.Error (451, loc, "The `new()' constraint " +
							"cannot be used with the `struct' constraint");
						return false;
					}

					if ((num_constraints > 0) || HasReferenceTypeConstraint || HasValueTypeConstraint) {
						Report.Error (449, loc, "The `class' or `struct' " +
							      "constraint must be the first constraint specified");
						return false;
					}

					if (sc == SpecialConstraint.ReferenceType)
						attrs |= GenericParameterAttributes.ReferenceTypeConstraint;
					else
						attrs |= GenericParameterAttributes.NotNullableValueTypeConstraint;
					continue;
				}

				int errors = Report.Errors;
				FullNamedExpression fn = ((Expression) obj).ResolveAsTypeStep (ec, false);

				if (fn == null) {
					if (errors != Report.Errors)
						return false;

					NamespaceEntry.Error_NamespaceNotFound (loc, ((Expression)obj).GetSignatureForError ());
					return false;
				}

				TypeExpr expr;
				ConstructedType cexpr = fn as ConstructedType;
				if (cexpr != null) {
					if (!cexpr.ResolveConstructedType (ec))
						return false;

					expr = cexpr;
				} else
					expr = ((Expression) obj).ResolveAsTypeTerminal (ec, false);

				if ((expr == null) || (expr.Type == null))
					return false;

				if (!ec.GenericDeclContainer.IsAccessibleAs (fn.Type)) {
					Report.SymbolRelatedToPreviousError (fn.Type);
					Report.Error (703, loc,
						"Inconsistent accessibility: constraint type `{0}' is less accessible than `{1}'",
						fn.GetSignatureForError (), ec.GenericDeclContainer.GetSignatureForError ());
					return false;
				}

				TypeParameterExpr texpr = expr as TypeParameterExpr;
				if (texpr != null)
					type_param_constraints.Add (expr);
				else if (expr.IsInterface)
					iface_constraints.Add (expr);
				else if (class_constraint != null) {
					Report.Error (406, loc,
						      "`{0}': the class constraint for `{1}' " +
						      "must come before any other constraints.",
						      expr.Name, name);
					return false;
				} else if (HasReferenceTypeConstraint || HasValueTypeConstraint) {
					Report.Error (450, loc, "`{0}': cannot specify both " +
						      "a constraint class and the `class' " +
						      "or `struct' constraint", expr.GetSignatureForError ());
					return false;
				} else
					class_constraint = expr;

				num_constraints++;
			}

			ArrayList list = new ArrayList ();
			foreach (TypeExpr iface_constraint in iface_constraints) {
				foreach (Type type in list) {
					if (!type.Equals (iface_constraint.Type))
						continue;

					Report.Error (405, loc,
						      "Duplicate constraint `{0}' for type " +
						      "parameter `{1}'.", iface_constraint.GetSignatureForError (),
						      name);
					return false;
				}

				list.Add (iface_constraint.Type);
			}

			foreach (TypeParameterExpr expr in type_param_constraints) {
				foreach (Type type in list) {
					if (!type.Equals (expr.Type))
						continue;

					Report.Error (405, loc,
						      "Duplicate constraint `{0}' for type " +
						      "parameter `{1}'.", expr.GetSignatureForError (), name);
					return false;
				}

				list.Add (expr.Type);
			}

			iface_constraint_types = new Type [list.Count];
			list.CopyTo (iface_constraint_types, 0);

			if (class_constraint != null) {
				class_constraint_type = class_constraint.Type;
				if (class_constraint_type == null)
					return false;

				if (class_constraint_type.IsSealed) {
					if (class_constraint_type.IsAbstract)
					{
						Report.Error (717, loc, "`{0}' is not a valid constraint. Static classes cannot be used as constraints",
							TypeManager.CSharpName (class_constraint_type));
					}
					else
					{
						Report.Error (701, loc, "`{0}' is not a valid constraint. A constraint must be an interface, " +
							"a non-sealed class or a type parameter", TypeManager.CSharpName(class_constraint_type));
					}
					return false;
				}

				if ((class_constraint_type == TypeManager.array_type) ||
				    (class_constraint_type == TypeManager.delegate_type) ||
				    (class_constraint_type == TypeManager.enum_type) ||
				    (class_constraint_type == TypeManager.value_type) ||
				    (class_constraint_type == TypeManager.object_type) ||
					class_constraint_type == TypeManager.multicast_delegate_type) {
					Report.Error (702, loc,
							  "A constraint cannot be special class `{0}'",
						      TypeManager.CSharpName (class_constraint_type));
					return false;
				}
			}

			if (class_constraint_type != null)
				effective_base_type = class_constraint_type;
			else if (HasValueTypeConstraint)
				effective_base_type = TypeManager.value_type;
			else
				effective_base_type = TypeManager.object_type;

			resolved = true;
			return true;
		}

		bool CheckTypeParameterConstraints (TypeParameter tparam, Hashtable seen)
		{
			seen.Add (tparam, true);

			Constraints constraints = tparam.Constraints;
			if (constraints == null)
				return true;

			if (constraints.HasValueTypeConstraint) {
				Report.Error (456, loc, "Type parameter `{0}' has " +
					      "the `struct' constraint, so it cannot " +
					      "be used as a constraint for `{1}'",
					      tparam.Name, name);
				return false;
			}

			if (constraints.type_param_constraints == null)
				return true;

			foreach (TypeParameterExpr expr in constraints.type_param_constraints) {
				if (seen.Contains (expr.TypeParameter)) {
					Report.Error (454, loc, "Circular constraint " +
						      "dependency involving `{0}' and `{1}'",
						      tparam.Name, expr.Name);
					return false;
				}

				if (!CheckTypeParameterConstraints (expr.TypeParameter, seen))
					return false;
			}

			return true;
		}

		/// <summary>
		///   Resolve the constraints into actual types.
		/// </summary>
		public bool ResolveTypes (IResolveContext ec)
		{
			if (resolved_types)
				return true;

			resolved_types = true;

			foreach (object obj in constraints) {
				ConstructedType cexpr = obj as ConstructedType;
				if (cexpr == null)
					continue;

				if (!cexpr.CheckConstraints (ec))
					return false;
			}

			foreach (TypeParameterExpr expr in type_param_constraints) {
				Hashtable seen = new Hashtable ();
				if (!CheckTypeParameterConstraints (expr.TypeParameter, seen))
					return false;
			}

			for (int i = 0; i < iface_constraints.Count; ++i) {
				TypeExpr iface_constraint = (TypeExpr) iface_constraints [i];
				iface_constraint = iface_constraint.ResolveAsTypeTerminal (ec, false);
				if (iface_constraint == null)
					return false;
				iface_constraints [i] = iface_constraint;
			}

			if (class_constraint != null) {
				class_constraint = class_constraint.ResolveAsTypeTerminal (ec, false);
				if (class_constraint == null)
					return false;
			}

			return true;
		}

		/// <summary>
		///   Check whether there are no conflicts in our type parameter constraints.
		///
		///   This is an example:
		///
		///   class Foo<T,U>
		///      where T : class
		///      where U : T, struct
		/// </summary>
		public bool CheckDependencies ()
		{
			foreach (TypeParameterExpr expr in type_param_constraints) {
				if (!CheckDependencies (expr.TypeParameter))
					return false;
			}

			return true;
		}

		bool CheckDependencies (TypeParameter tparam)
		{
			Constraints constraints = tparam.Constraints;
			if (constraints == null)
				return true;

			if (HasValueTypeConstraint && constraints.HasClassConstraint) {
				Report.Error (455, loc, "Type parameter `{0}' inherits " +
					      "conflicting constraints `{1}' and `{2}'",
					      name, TypeManager.CSharpName (constraints.ClassConstraint),
					      "System.ValueType");
				return false;
			}

			if (HasClassConstraint && constraints.HasClassConstraint) {
				Type t1 = ClassConstraint;
				TypeExpr e1 = class_constraint;
				Type t2 = constraints.ClassConstraint;
				TypeExpr e2 = constraints.class_constraint;

				if (!Convert.ImplicitReferenceConversionExists (e1, t2) &&
				    !Convert.ImplicitReferenceConversionExists (e2, t1)) {
					Report.Error (455, loc,
						      "Type parameter `{0}' inherits " +
						      "conflicting constraints `{1}' and `{2}'",
						      name, TypeManager.CSharpName (t1), TypeManager.CSharpName (t2));
					return false;
				}
			}

			if (constraints.type_param_constraints == null)
				return true;

			foreach (TypeParameterExpr expr in constraints.type_param_constraints) {
				if (!CheckDependencies (expr.TypeParameter))
					return false;
			}

			return true;
		}

		public override GenericParameterAttributes Attributes {
			get { return attrs; }
		}

		public override bool HasClassConstraint {
			get { return class_constraint != null; }
		}

		public override Type ClassConstraint {
			get { return class_constraint_type; }
		}

		public override Type[] InterfaceConstraints {
			get { return iface_constraint_types; }
		}

		public override Type EffectiveBaseClass {
			get { return effective_base_type; }
		}

		public bool IsSubclassOf (Type t)
		{
			if ((class_constraint_type != null) &&
			    class_constraint_type.IsSubclassOf (t))
				return true;

			if (iface_constraint_types == null)
				return false;

			foreach (Type iface in iface_constraint_types) {
				if (TypeManager.IsSubclassOf (iface, t))
					return true;
			}

			return false;
		}

		public Location Location {
			get {
				return loc;
			}
		}

		/// <summary>
		///   This is used when we're implementing a generic interface method.
		///   Each method type parameter in implementing method must have the same
		///   constraints than the corresponding type parameter in the interface
		///   method.  To do that, we're called on each of the implementing method's
		///   type parameters.
		/// </summary>
		public bool CheckInterfaceMethod (GenericConstraints gc)
		{
			if (gc.Attributes != attrs)
				return false;

			if (HasClassConstraint != gc.HasClassConstraint)
				return false;
			if (HasClassConstraint && !TypeManager.IsEqual (gc.ClassConstraint, ClassConstraint))
				return false;

			int gc_icount = gc.InterfaceConstraints != null ?
				gc.InterfaceConstraints.Length : 0;
			int icount = InterfaceConstraints != null ?
				InterfaceConstraints.Length : 0;

			if (gc_icount != icount)
				return false;

			for (int i = 0; i < gc.InterfaceConstraints.Length; ++i) {
				Type iface = gc.InterfaceConstraints [i];
				if (iface.IsGenericType)
					iface = iface.GetGenericTypeDefinition ();
				
				bool ok = false;
				for (int ii = 0; i < InterfaceConstraints.Length; ++ii) {
					Type check = InterfaceConstraints [ii];
					if (check.IsGenericType)
						check = check.GetGenericTypeDefinition ();
					
					if (TypeManager.IsEqual (iface, check)) {
						ok = true;
						break;
					}
				}

				if (!ok)
					return false;
			}

			return true;
		}

		public void VerifyClsCompliance ()
		{
			if (class_constraint_type != null && !AttributeTester.IsClsCompliant (class_constraint_type))
				Warning_ConstrainIsNotClsCompliant (class_constraint_type, class_constraint.Location);

			if (iface_constraint_types != null) {
				for (int i = 0; i < iface_constraint_types.Length; ++i) {
					if (!AttributeTester.IsClsCompliant (iface_constraint_types [i]))
						Warning_ConstrainIsNotClsCompliant (iface_constraint_types [i],
							((TypeExpr)iface_constraints [i]).Location);
				}
			}
		}

		void Warning_ConstrainIsNotClsCompliant (Type t, Location loc)
		{
			Report.SymbolRelatedToPreviousError (t);
			Report.Warning (3024, 1, loc, "Constraint type `{0}' is not CLS-compliant",
				TypeManager.CSharpName (t));
		}
	}

	/// <summary>
	///   A type parameter from a generic type definition.
	/// </summary>
	public class TypeParameter : MemberCore, IMemberContainer
	{
		static readonly string[] attribute_target = new string [] { "type parameter" };
		
		string name;
		DeclSpace decl;
		GenericConstraints gc;
		Constraints constraints;
		Location loc;
		GenericTypeParameterBuilder type;
		MemberCache member_cache;

		public TypeParameter (DeclSpace parent, DeclSpace decl, string name,
				      Constraints constraints, Attributes attrs, Location loc)
			: base (parent, new MemberName (name, loc), attrs)
		{
			this.name = name;
			this.decl = decl;
			this.constraints = constraints;
			this.loc = loc;
		}

		public GenericConstraints GenericConstraints {
			get { return gc != null ? gc : constraints; }
		}

		public Constraints Constraints {
			get { return constraints; }
		}

		public DeclSpace DeclSpace {
			get { return decl; }
		}

		public Type Type {
			get { return type; }
		}

		/// <summary>
		///   This is the first method which is called during the resolving
		///   process; we're called immediately after creating the type parameters
		///   with SRE (by calling `DefineGenericParameters()' on the TypeBuilder /
		///   MethodBuilder).
		///
		///   We're either called from TypeContainer.DefineType() or from
		///   GenericMethod.Define() (called from Method.Define()).
		/// </summary>
		public void Define (GenericTypeParameterBuilder type)
		{
			if (this.type != null)
				throw new InvalidOperationException ();

			this.type = type;
			TypeManager.AddTypeParameter (type, this);
		}

		/// <summary>
		///   This is the second method which is called during the resolving
		///   process - in case of class type parameters, we're called from
		///   TypeContainer.ResolveType() - after it resolved the class'es
		///   base class and interfaces. For method type parameters, we're
		///   called immediately after Define().
		///
		///   We're just resolving the constraints into expressions here, we
		///   don't resolve them into actual types.
		///
		///   Note that in the special case of partial generic classes, we may be
		///   called _before_ Define() and we may also be called multiple types.
		/// </summary>
		public bool Resolve (DeclSpace ds)
		{
			if (constraints != null) {
				if (!constraints.Resolve (ds)) {
					constraints = null;
					return false;
				}
			}

			return true;
		}

		/// <summary>
		///   This is the third method which is called during the resolving
		///   process.  We're called immediately after calling DefineConstraints()
		///   on all of the current class'es type parameters.
		///
		///   Our job is to resolve the constraints to actual types.
		///
		///   Note that we may have circular dependencies on type parameters - this
		///   is why Resolve() and ResolveType() are separate.
		/// </summary>
		public bool ResolveType (IResolveContext ec)
		{
			if (constraints != null) {
				if (!constraints.ResolveTypes (ec)) {
					constraints = null;
					return false;
				}
			}

			return true;
		}

		/// <summary>
		///   This is the fourth and last method which is called during the resolving
		///   process.  We're called after everything is fully resolved and actually
		///   register the constraints with SRE and the TypeManager.
		/// </summary>
		public bool DefineType (IResolveContext ec)
		{
			return DefineType (ec, null, null, false);
		}

		/// <summary>
		///   This is the fith and last method which is called during the resolving
		///   process.  We're called after everything is fully resolved and actually
		///   register the constraints with SRE and the TypeManager.
		///
		///   The `builder', `implementing' and `is_override' arguments are only
		///   applicable to method type parameters.
		/// </summary>
		public bool DefineType (IResolveContext ec, MethodBuilder builder,
					MethodInfo implementing, bool is_override)
		{
			if (!ResolveType (ec))
				return false;

			if (implementing != null) {
				if (is_override && (constraints != null)) {
					Report.Error (460, loc,
						"`{0}': Cannot specify constraints for overrides or explicit interface implementation methods",
						TypeManager.CSharpSignature (builder));
					return false;
				}

				MethodBase mb = TypeManager.DropGenericMethodArguments (implementing);

				int pos = type.GenericParameterPosition;
				Type mparam = mb.GetGenericArguments () [pos];
				GenericConstraints temp_gc = ReflectionConstraints.GetConstraints (mparam);

				if (temp_gc != null)
					gc = new InflatedConstraints (temp_gc, implementing.DeclaringType);
				else if (constraints != null)
					gc = new InflatedConstraints (constraints, implementing.DeclaringType);

				bool ok = true;
				if (constraints != null) {
					if (temp_gc == null)
						ok = false;
					else if (!constraints.CheckInterfaceMethod (gc))
						ok = false;
				} else {
					if (!is_override && (temp_gc != null))
						ok = false;
				}

				if (!ok) {
					Report.SymbolRelatedToPreviousError (implementing);

					Report.Error (
						425, loc, "The constraints for type " +
						"parameter `{0}' of method `{1}' must match " +
						"the constraints for type parameter `{2}' " +
						"of interface method `{3}'. Consider using " +
						"an explicit interface implementation instead",
						Name, TypeManager.CSharpSignature (builder),
						TypeManager.CSharpName (mparam), TypeManager.CSharpSignature (mb));
					return false;
				}
			} else if (DeclSpace is CompilerGeneratedClass) {
				TypeParameter[] tparams = DeclSpace.TypeParameters;
				Type[] types = new Type [tparams.Length];
				for (int i = 0; i < tparams.Length; i++)
					types [i] = tparams [i].Type;

				if (constraints != null)
					gc = new InflatedConstraints (constraints, types);
			} else {
				gc = (GenericConstraints) constraints;
			}

			if (gc == null)
				return true;

			if (gc.HasClassConstraint)
				type.SetBaseTypeConstraint (gc.ClassConstraint);

			type.SetInterfaceConstraints (gc.InterfaceConstraints);
			type.SetGenericParameterAttributes (gc.Attributes);
			TypeManager.RegisterBuilder (type, gc.InterfaceConstraints);

			return true;
		}

		/// <summary>
		///   Check whether there are no conflicts in our type parameter constraints.
		///
		///   This is an example:
		///
		///   class Foo<T,U>
		///      where T : class
		///      where U : T, struct
		/// </summary>
		public bool CheckDependencies ()
		{
			if (constraints != null)
				return constraints.CheckDependencies ();

			return true;
		}

		/// <summary>
		///   This is called for each part of a partial generic type definition.
		///
		///   If `new_constraints' is not null and we don't already have constraints,
		///   they become our constraints.  If we already have constraints, we must
		///   check that they're the same.
		///   con
		/// </summary>
		public bool UpdateConstraints (IResolveContext ec, Constraints new_constraints)
		{
			if (type == null)
				throw new InvalidOperationException ();

			if (new_constraints == null)
				return true;

			if (!new_constraints.Resolve (ec))
				return false;
			if (!new_constraints.ResolveTypes (ec))
				return false;

			if (constraints != null) 
				return constraints.CheckInterfaceMethod (new_constraints);

			constraints = new_constraints;
			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null)
				OptAttributes.Emit ();

			base.Emit ();
		}

		public override string DocCommentHeader {
			get {
				throw new InvalidOperationException (
					"Unexpected attempt to get doc comment from " + this.GetType () + ".");
			}
		}

		//
		// MemberContainer
		//

		public override bool Define ()
		{
			return true;
		}

		public override void ApplyAttributeBuilder (Attribute a,
							    CustomAttributeBuilder cb)
		{
			type.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.GenericParameter;
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_target;
			}
		}

		//
		// IMemberContainer
		//

		string IMemberContainer.Name {
			get { return Name; }
		}

		MemberCache IMemberContainer.BaseCache {
			get {
				if (gc == null)
					return null;

				if (gc.EffectiveBaseClass.BaseType == null)
					return null;

				return TypeManager.LookupMemberCache (gc.EffectiveBaseClass.BaseType);
			}
		}

		bool IMemberContainer.IsInterface {
			get { return false; }
		}

		MemberList IMemberContainer.GetMembers (MemberTypes mt, BindingFlags bf)
		{
			throw new NotSupportedException ();
		}

		public MemberCache MemberCache {
			get {
				if (member_cache != null)
					return member_cache;

				if (gc == null)
					return null;

				Type[] ifaces = TypeManager.ExpandInterfaces (gc.InterfaceConstraints);
				member_cache = new MemberCache (this, gc.EffectiveBaseClass, ifaces);

				return member_cache;
			}
		}

		public MemberList FindMembers (MemberTypes mt, BindingFlags bf,
					       MemberFilter filter, object criteria)
		{
			if (gc == null)
				return MemberList.Empty;

			ArrayList members = new ArrayList ();

			if (gc.HasClassConstraint) {
				MemberList list = TypeManager.FindMembers (
					gc.ClassConstraint, mt, bf, filter, criteria);

				members.AddRange (list);
			}

			Type[] ifaces = TypeManager.ExpandInterfaces (gc.InterfaceConstraints);
			foreach (Type t in ifaces) {
				MemberList list = TypeManager.FindMembers (
					t, mt, bf, filter, criteria);

				members.AddRange (list);
			}

			return new MemberList (members);
		}

		public bool IsSubclassOf (Type t)
		{
			if (type.Equals (t))
				return true;

			if (constraints != null)
				return constraints.IsSubclassOf (t);

			return false;
		}

		public override string ToString ()
		{
			return "TypeParameter[" + name + "]";
		}

		public static string GetSignatureForError (TypeParameter[] tp)
		{
			if (tp == null || tp.Length == 0)
				return "";

			StringBuilder sb = new StringBuilder ("<");
			for (int i = 0; i < tp.Length; ++i) {
				if (i > 0)
					sb.Append (",");
				sb.Append (tp[i].GetSignatureForError ());
			}
			sb.Append ('>');
			return sb.ToString ();
		}

		public void InflateConstraints (Type declaring)
		{
			if (constraints != null)
				gc = new InflatedConstraints (constraints, declaring);
		}
		
		public override bool IsClsComplianceRequired ()
		{
			return false;
		}

		protected class InflatedConstraints : GenericConstraints
		{
			GenericConstraints gc;
			Type base_type;
			Type class_constraint;
			Type[] iface_constraints;
			Type[] dargs;

			public InflatedConstraints (GenericConstraints gc, Type declaring)
				: this (gc, TypeManager.GetTypeArguments (declaring))
			{ }

			public InflatedConstraints (GenericConstraints gc, Type[] dargs)
			{
				this.gc = gc;
				this.dargs = dargs;

				ArrayList list = new ArrayList ();
				if (gc.HasClassConstraint)
					list.Add (inflate (gc.ClassConstraint));
				foreach (Type iface in gc.InterfaceConstraints)
					list.Add (inflate (iface));

				bool has_class_constr = false;
				if (list.Count > 0) {
					Type first = (Type) list [0];
					has_class_constr = !first.IsGenericParameter && !first.IsInterface;
				}

				if ((list.Count > 0) && has_class_constr) {
					class_constraint = (Type) list [0];
					iface_constraints = new Type [list.Count - 1];
					list.CopyTo (1, iface_constraints, 0, list.Count - 1);
				} else {
					iface_constraints = new Type [list.Count];
					list.CopyTo (iface_constraints, 0);
				}

				if (HasValueTypeConstraint)
					base_type = TypeManager.value_type;
				else if (class_constraint != null)
					base_type = class_constraint;
				else
					base_type = TypeManager.object_type;
			}

			Type inflate (Type t)
			{
				if (t == null)
					return null;
				if (t.IsGenericParameter)
					return dargs [t.GenericParameterPosition];
				if (t.IsGenericType) {
					Type[] args = t.GetGenericArguments ();
					Type[] inflated = new Type [args.Length];

					for (int i = 0; i < args.Length; i++)
						inflated [i] = inflate (args [i]);

					t = t.GetGenericTypeDefinition ();
					t = t.MakeGenericType (inflated);
				}

				return t;
			}

			public override string TypeParameter {
				get { return gc.TypeParameter; }
			}

			public override GenericParameterAttributes Attributes {
				get { return gc.Attributes; }
			}

			public override Type ClassConstraint {
				get { return class_constraint; }
			}

			public override Type EffectiveBaseClass {
				get { return base_type; }
			}

			public override Type[] InterfaceConstraints {
				get { return iface_constraints; }
			}
		}
	}

	/// <summary>
	///   A TypeExpr which already resolved to a type parameter.
	/// </summary>
	public class TypeParameterExpr : TypeExpr {
		TypeParameter type_parameter;

		public override string Name {
			get {
				return type_parameter.Name;
			}
		}

		public override string FullName {
			get {
				return type_parameter.Name;
			}
		}

		public TypeParameter TypeParameter {
			get {
				return type_parameter;
			}
		}
		
		public TypeParameterExpr (TypeParameter type_parameter, Location loc)
		{
			this.type_parameter = type_parameter;
			this.loc = loc;
		}

		protected override TypeExpr DoResolveAsTypeStep (IResolveContext ec)
		{
			type = type_parameter.Type;

			return this;
		}

		public override bool IsInterface {
			get { return false; }
		}

		public override bool CheckAccessLevel (DeclSpace ds)
		{
			return true;
		}

		public void Error_CannotUseAsUnmanagedType (Location loc)
		{
			Report.Error (-203, loc, "Can not use type parameter as unmanaged type");
		}
	}

	/// <summary>
	///   Tracks the type arguments when instantiating a generic type.  We're used in
	///   ConstructedType.
	/// </summary>
	public class TypeArguments {
		public readonly Location Location;
		ArrayList args;
		Type[] atypes;
		int dimension;
		bool has_type_args;
		bool created;
		
		public TypeArguments (Location loc)
		{
			args = new ArrayList ();
			this.Location = loc;
		}

		public TypeArguments (Location loc, params Expression[] types)
		{
			this.Location = loc;
			this.args = new ArrayList (types);
		}
		
		public TypeArguments (int dimension, Location loc)
		{
			this.dimension = dimension;
			this.Location = loc;
		}

		public void Add (Expression type)
		{
			if (created)
				throw new InvalidOperationException ();

			args.Add (type);
		}

		public void Add (TypeArguments new_args)
		{
			if (created)
				throw new InvalidOperationException ();

			args.AddRange (new_args.args);
		}

		/// <summary>
		///   We're used during the parsing process: the parser can't distinguish
		///   between type parameters and type arguments.  Because of that, the
		///   parser creates a `MemberName' with `TypeArguments' for both cases and
		///   in case of a generic type definition, we call GetDeclarations().
		/// </summary>
		public TypeParameterName[] GetDeclarations ()
		{
			TypeParameterName[] ret = new TypeParameterName [args.Count];
			for (int i = 0; i < args.Count; i++) {
				TypeParameterName name = args [i] as TypeParameterName;
				if (name != null) {
					ret [i] = name;
					continue;
				}
				SimpleName sn = args [i] as SimpleName;
				if (sn != null) {
					ret [i] = new TypeParameterName (sn.Name, null, sn.Location);
					continue;
				}

				Expression expr = (Expression) args [i];
				// TODO: Wrong location
				Report.Error (81, Location, "Type parameter declaration must be an identifier not a type");
				ret [i] = new TypeParameterName (expr.GetSignatureForError (), null, expr.Location);
			}
			return ret;
		}

		/// <summary>
		///   We may only be used after Resolve() is called and return the fully
		///   resolved types.
		/// </summary>
		public Type[] Arguments {
			get {
				return atypes;
			}
		}

		public bool HasTypeArguments {
			get {
				return has_type_args;
			}
		}

		public int Count {
			get {
				if (dimension > 0)
					return dimension;
				else
					return args.Count;
			}
		}

		public bool IsUnbound {
			get {
				return dimension > 0;
			}
		}

		public override string ToString ()
		{
			StringBuilder s = new StringBuilder ();

			int count = Count;
			for (int i = 0; i < count; i++){
				//
				// FIXME: Use TypeManager.CSharpname once we have the type
				//
				if (args != null)
					s.Append (args [i].ToString ());
				if (i+1 < count)
					s.Append (",");
			}
			return s.ToString ();
		}

		public string GetSignatureForError()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < Count; ++i)
			{
				Expression expr = (Expression)args [i];
				sb.Append(expr.GetSignatureForError());
				if (i + 1 < Count)
					sb.Append(',');
			}
			return sb.ToString();
		}

		/// <summary>
		///   Resolve the type arguments.
		/// </summary>
		public bool Resolve (IResolveContext ec)
		{
			int count = args.Count;
			bool ok = true;

			atypes = new Type [count];

			for (int i = 0; i < count; i++){
				TypeExpr te = ((Expression) args [i]).ResolveAsTypeTerminal (ec, false);
				if (te == null) {
					ok = false;
					continue;
				}

				atypes[i] = te.Type;
				if (te.Type.IsGenericParameter) {
					if (te is TypeParameterExpr)
						has_type_args = true;
					continue;
				}

				if (te.Type.IsSealed && te.Type.IsAbstract) {
					Report.Error (718, Location, "`{0}': static classes cannot be used as generic arguments",
						te.GetSignatureForError ());
					return false;
				}

				if (te.Type.IsPointer) {
					Report.Error (306, Location, "The type `{0}' may not be used " +
							  "as a type argument", TypeManager.CSharpName (te.Type));
					return false;
				}

				if (te.Type == TypeManager.void_type) {
					Expression.Error_VoidInvalidInTheContext (Location);
					return false;
				}
			}
			return ok;
		}

		public TypeArguments Clone ()
		{
			TypeArguments copy = new TypeArguments (Location);
			foreach (Expression ta in args)
				copy.args.Add (ta);

			return copy;
		}
	}

	public class TypeParameterName : SimpleName
	{
		Attributes attributes;

		public TypeParameterName (string name, Attributes attrs, Location loc)
			: base (name, loc)
		{
			attributes = attrs;
		}

		public Attributes OptAttributes {
			get {
				return attributes;
			}
		}
	}

	/// <summary>
	///   An instantiation of a generic type.
	/// </summary>	
	public class ConstructedType : TypeExpr {
		string full_name;
		FullNamedExpression name;
		TypeArguments args;
		Type[] gen_params, atypes;
		Type gt;

		/// <summary>
		///   Instantiate the generic type `fname' with the type arguments `args'.
		/// </summary>		
		public ConstructedType (FullNamedExpression fname, TypeArguments args, Location l)
		{
			loc = l;
			this.name = fname;
			this.args = args;

			eclass = ExprClass.Type;
			full_name = name + "<" + args.ToString () + ">";
		}

		protected ConstructedType (TypeArguments args, Location l)
		{
			loc = l;
			this.args = args;

			eclass = ExprClass.Type;
		}

		protected ConstructedType (TypeParameter[] type_params, Location l)
		{
			loc = l;

			args = new TypeArguments (l);
			foreach (TypeParameter type_param in type_params)
				args.Add (new TypeParameterExpr (type_param, l));

			eclass = ExprClass.Type;
		}

		/// <summary>
		///   This is used to construct the `this' type inside a generic type definition.
		/// </summary>
		public ConstructedType (Type t, TypeParameter[] type_params, Location l)
			: this (type_params, l)
		{
			gt = t.GetGenericTypeDefinition ();

			this.name = new TypeExpression (gt, l);
			full_name = gt.FullName + "<" + args.ToString () + ">";
		}

		/// <summary>
		///   Instantiate the generic type `t' with the type arguments `args'.
		///   Use this constructor if you already know the fully resolved
		///   generic type.
		/// </summary>		
		public ConstructedType (Type t, TypeArguments args, Location l)
			: this (args, l)
		{
			gt = t.GetGenericTypeDefinition ();

			this.name = new TypeExpression (gt, l);
			full_name = gt.FullName + "<" + args.ToString () + ">";
		}

		public TypeArguments TypeArguments {
			get { return args; }
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.RemoveGenericArity (gt.FullName) + "<" + args.GetSignatureForError () + ">";
		}

		protected override TypeExpr DoResolveAsTypeStep (IResolveContext ec)
		{
			if (!ResolveConstructedType (ec))
				return null;

			return this;
		}

		/// <summary>
		///   Check the constraints; we're called from ResolveAsTypeTerminal()
		///   after fully resolving the constructed type.
		/// </summary>
		public bool CheckConstraints (IResolveContext ec)
		{
			return ConstraintChecker.CheckConstraints (ec, gt, gen_params, atypes, loc);
		}

		/// <summary>
		///   Resolve the constructed type, but don't check the constraints.
		/// </summary>
		public bool ResolveConstructedType (IResolveContext ec)
		{
			if (type != null)
				return true;
			// If we already know the fully resolved generic type.
			if (gt != null)
				return DoResolveType (ec);

			int num_args;
			Type t = name.Type;

			if (t == null) {
				Report.Error (246, loc, "Cannot find type `{0}'<...>", Name);
				return false;
			}

			num_args = TypeManager.GetNumberOfTypeArguments (t);
			if (num_args == 0) {
				Report.Error (308, loc,
					      "The non-generic type `{0}' cannot " +
					      "be used with type arguments.",
					      TypeManager.CSharpName (t));
				return false;
			}

			gt = t.GetGenericTypeDefinition ();
			return DoResolveType (ec);
		}

		bool DoResolveType (IResolveContext ec)
		{
			//
			// Resolve the arguments.
			//
			if (args.Resolve (ec) == false)
				return false;

			gen_params = gt.GetGenericArguments ();
			atypes = args.Arguments;

			if (atypes.Length != gen_params.Length) {
				Report.Error (305, loc,
					      "Using the generic type `{0}' " +
					      "requires {1} type arguments",
					      TypeManager.CSharpName (gt),
					      gen_params.Length.ToString ());
				return false;
			}

			//
			// Now bind the parameters.
			//
			type = gt.MakeGenericType (atypes);
			return true;
		}

		public Expression GetSimpleName (EmitContext ec)
		{
			return this;
		}

		public override bool CheckAccessLevel (DeclSpace ds)
		{
			return ds.CheckAccessLevel (gt);
		}

		public override bool AsAccessible (DeclSpace ds)
		{
			foreach (Type t in atypes) {
				if (!ds.IsAccessibleAs (t))
					return false;
			}

			return ds.IsAccessibleAs (gt);
		}

		public override bool IsClass {
			get { return gt.IsClass; }
		}

		public override bool IsValueType {
			get { return gt.IsValueType; }
		}

		public override bool IsInterface {
			get { return gt.IsInterface; }
		}

		public override bool IsSealed {
			get { return gt.IsSealed; }
		}

		public override bool Equals (object obj)
		{
			ConstructedType cobj = obj as ConstructedType;
			if (cobj == null)
				return false;

			if ((type == null) || (cobj.type == null))
				return false;

			return type == cobj.type;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override string Name {
			get {
				return full_name;
			}
		}

		public override string FullName {
			get {
				return full_name;
			}
		}
	}

	public abstract class ConstraintChecker
	{
		protected readonly Type[] gen_params;
		protected readonly Type[] atypes;
		protected readonly Location loc;

		protected ConstraintChecker (Type[] gen_params, Type[] atypes, Location loc)
		{
			this.gen_params = gen_params;
			this.atypes = atypes;
			this.loc = loc;
		}

		/// <summary>
		///   Check the constraints; we're called from ResolveAsTypeTerminal()
		///   after fully resolving the constructed type.
		/// </summary>
		public bool CheckConstraints (IResolveContext ec)
		{
			for (int i = 0; i < gen_params.Length; i++) {
				if (!CheckConstraints (ec, i))
					return false;
			}

			return true;
		}

		protected bool CheckConstraints (IResolveContext ec, int index)
		{
			Type atype = atypes [index];
			Type ptype = gen_params [index];

			if (atype == ptype)
				return true;

			Expression aexpr = new EmptyExpression (atype);

			GenericConstraints gc = TypeManager.GetTypeParameterConstraints (ptype);
			if (gc == null)
				return true;

			bool is_class, is_struct;
			if (atype.IsGenericParameter) {
				GenericConstraints agc = TypeManager.GetTypeParameterConstraints (atype);
				if (agc != null) {
					if (agc is Constraints)
						((Constraints) agc).Resolve (ec);
					is_class = agc.IsReferenceType;
					is_struct = agc.IsValueType;
				} else {
					is_class = is_struct = false;
				}
			} else {
#if MS_COMPATIBLE
				is_class = false;
				if (!atype.IsGenericType)
#endif
				is_class = atype.IsClass || atype.IsInterface;
				is_struct = atype.IsValueType && !TypeManager.IsNullableType (atype);
			}

			//
			// First, check the `class' and `struct' constraints.
			//
			if (gc.HasReferenceTypeConstraint && !is_class) {
				Report.Error (452, loc, "The type `{0}' must be " +
					      "a reference type in order to use it " +
					      "as type parameter `{1}' in the " +
					      "generic type or method `{2}'.",
					      TypeManager.CSharpName (atype),
					      TypeManager.CSharpName (ptype),
					      GetSignatureForError ());
				return false;
			} else if (gc.HasValueTypeConstraint && !is_struct) {
				Report.Error (453, loc, "The type `{0}' must be a " +
					      "non-nullable value type in order to use it " +
					      "as type parameter `{1}' in the " +
					      "generic type or method `{2}'.",
					      TypeManager.CSharpName (atype),
					      TypeManager.CSharpName (ptype),
					      GetSignatureForError ());
				return false;
			}

			//
			// The class constraint comes next.
			//
			if (gc.HasClassConstraint) {
				if (!CheckConstraint (ec, ptype, aexpr, gc.ClassConstraint))
					return false;
			}

			//
			// Now, check the interface constraints.
			//
			if (gc.InterfaceConstraints != null) {
				foreach (Type it in gc.InterfaceConstraints) {
					if (!CheckConstraint (ec, ptype, aexpr, it))
						return false;
				}
			}

			//
			// Finally, check the constructor constraint.
			//

			if (!gc.HasConstructorConstraint)
				return true;

			if (TypeManager.IsBuiltinType (atype) || atype.IsValueType)
				return true;

			if (HasDefaultConstructor (atype))
				return true;

			Report_SymbolRelatedToPreviousError ();
			Report.SymbolRelatedToPreviousError (atype);
			Report.Error (310, loc, "The type `{0}' must have a public " +
				      "parameterless constructor in order to use it " +
				      "as parameter `{1}' in the generic type or " +
				      "method `{2}'",
				      TypeManager.CSharpName (atype),
				      TypeManager.CSharpName (ptype),
				      GetSignatureForError ());
			return false;
		}

		protected bool CheckConstraint (IResolveContext ec, Type ptype, Expression expr,
						Type ctype)
		{
			if (TypeManager.HasGenericArguments (ctype)) {
				Type[] types = TypeManager.GetTypeArguments (ctype);

				TypeArguments new_args = new TypeArguments (loc);

				for (int i = 0; i < types.Length; i++) {
					Type t = types [i];

					if (t.IsGenericParameter) {
						int pos = t.GenericParameterPosition;
						t = atypes [pos];
					}
					new_args.Add (new TypeExpression (t, loc));
				}

				TypeExpr ct = new ConstructedType (ctype, new_args, loc);
				if (ct.ResolveAsTypeStep (ec, false) == null)
					return false;
				ctype = ct.Type;
			} else if (ctype.IsGenericParameter) {
				int pos = ctype.GenericParameterPosition;
				ctype = atypes [pos];
			}

			if (Convert.ImplicitStandardConversionExists (expr, ctype))
				return true;

			Error_TypeMustBeConvertible (expr.Type, ctype, ptype);
			return false;
		}

		bool HasDefaultConstructor (Type atype)
		{
			if (atype.IsAbstract)
				return false;

		again:
			atype = TypeManager.DropGenericTypeArguments (atype);
			if (atype is TypeBuilder) {
				TypeContainer tc = TypeManager.LookupTypeContainer (atype);
				if (tc.InstanceConstructors == null) {
					atype = atype.BaseType;
					goto again;
				}

				foreach (Constructor c in tc.InstanceConstructors) {
					if ((c.ModFlags & Modifiers.PUBLIC) == 0)
						continue;
					if ((c.Parameters.FixedParameters != null) &&
					    (c.Parameters.FixedParameters.Length != 0))
						continue;
					if (c.Parameters.HasArglist || c.Parameters.HasParams)
						continue;

					return true;
				}
			}

			TypeParameter tparam = TypeManager.LookupTypeParameter (atype);
			if (tparam != null) {
				if (tparam.GenericConstraints == null)
					return false;
						
				return tparam.GenericConstraints.HasConstructorConstraint || 
					tparam.GenericConstraints.HasValueTypeConstraint;
			}

			MemberInfo [] list = TypeManager.MemberLookup (null, null, atype, MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				ConstructorInfo.ConstructorName, null);

			if (list == null)
				return false;

			foreach (MethodBase mb in list) {
				ParameterData pd = TypeManager.GetParameterData (mb);
				if (pd.Count == 0)
					return true;
			}

			return false;
		}

		protected abstract string GetSignatureForError ();
		protected abstract void Report_SymbolRelatedToPreviousError ();

		void Error_TypeMustBeConvertible (Type atype, Type gc, Type ptype)
		{
			Report_SymbolRelatedToPreviousError ();
			Report.SymbolRelatedToPreviousError (atype);
			Report.Error (309, loc, 
				      "The type `{0}' must be convertible to `{1}' in order to " +
				      "use it as parameter `{2}' in the generic type or method `{3}'",
				      TypeManager.CSharpName (atype), TypeManager.CSharpName (gc),
				      TypeManager.CSharpName (ptype), GetSignatureForError ());
		}

		public static bool CheckConstraints (EmitContext ec, MethodBase definition,
						     MethodBase instantiated, Location loc)
		{
			MethodConstraintChecker checker = new MethodConstraintChecker (
				definition, definition.GetGenericArguments (),
				instantiated.GetGenericArguments (), loc);

			return checker.CheckConstraints (ec);
		}

		public static bool CheckConstraints (IResolveContext ec, Type gt, Type[] gen_params,
						     Type[] atypes, Location loc)
		{
			TypeConstraintChecker checker = new TypeConstraintChecker (
				gt, gen_params, atypes, loc);

			return checker.CheckConstraints (ec);
		}

		protected class MethodConstraintChecker : ConstraintChecker
		{
			MethodBase definition;

			public MethodConstraintChecker (MethodBase definition, Type[] gen_params,
							Type[] atypes, Location loc)
				: base (gen_params, atypes, loc)
			{
				this.definition = definition;
			}

			protected override string GetSignatureForError ()
			{
				return TypeManager.CSharpSignature (definition);
			}

			protected override void Report_SymbolRelatedToPreviousError ()
			{
				Report.SymbolRelatedToPreviousError (definition);
			}
		}

		protected class TypeConstraintChecker : ConstraintChecker
		{
			Type gt;

			public TypeConstraintChecker (Type gt, Type[] gen_params, Type[] atypes,
						      Location loc)
				: base (gen_params, atypes, loc)
			{
				this.gt = gt;
			}

			protected override string GetSignatureForError ()
			{
				return TypeManager.CSharpName (gt);
			}

			protected override void Report_SymbolRelatedToPreviousError ()
			{
				Report.SymbolRelatedToPreviousError (gt);
			}
		}
	}

	/// <summary>
	///   A generic method definition.
	/// </summary>
	public class GenericMethod : DeclSpace
	{
		Expression return_type;
		Parameters parameters;

		public GenericMethod (NamespaceEntry ns, DeclSpace parent, MemberName name,
				      Expression return_type, Parameters parameters)
			: base (ns, parent, name, null)
		{
			this.return_type = return_type;
			this.parameters = parameters;
		}

		public override TypeBuilder DefineType ()
		{
			throw new Exception ();
		}

		public override bool Define ()
		{
			for (int i = 0; i < TypeParameters.Length; i++)
				if (!TypeParameters [i].Resolve (this))
					return false;

			return true;
		}

		/// <summary>
		///   Define and resolve the type parameters.
		///   We're called from Method.Define().
		/// </summary>
		public bool Define (MethodBuilder mb, ToplevelBlock block)
		{
			TypeParameterName[] names = MemberName.TypeArguments.GetDeclarations ();
			string[] snames = new string [names.Length];
			for (int i = 0; i < names.Length; i++) {
				string type_argument_name = names[i].Name;
				Parameter p = parameters.GetParameterByName (type_argument_name);
				if (p != null) {
					Error_ParameterNameCollision (p.Location, type_argument_name, "method parameter");
					return false;
				}
				
				snames[i] = type_argument_name;
			}

			GenericTypeParameterBuilder[] gen_params = mb.DefineGenericParameters (snames);
			for (int i = 0; i < TypeParameters.Length; i++)
				TypeParameters [i].Define (gen_params [i]);

			if (!Define ())
				return false;

			for (int i = 0; i < TypeParameters.Length; i++) {
				if (!TypeParameters [i].ResolveType (this))
					return false;
			}

			return true;
		}

		internal static void Error_ParameterNameCollision (Location loc, string name, string collisionWith)
		{
			Report.Error (412, loc, "The type parameter name `{0}' is the same as `{1}'",
				name, collisionWith);
		}

		/// <summary>
		///   We're called from MethodData.Define() after creating the MethodBuilder.
		/// </summary>
		public bool DefineType (EmitContext ec, MethodBuilder mb,
					MethodInfo implementing, bool is_override)
		{
			for (int i = 0; i < TypeParameters.Length; i++)
				if (!TypeParameters [i].DefineType (
					    ec, mb, implementing, is_override))
					return false;

			bool ok = true;
			foreach (Parameter p in parameters.FixedParameters){
				if (!p.Resolve (ec))
					ok = false;
			}
			if ((return_type != null) && (return_type.ResolveAsTypeTerminal (ec, false) == null))
				ok = false;

			return ok;
		}

		public void EmitAttributes ()
		{
			for (int i = 0; i < TypeParameters.Length; i++)
				TypeParameters [i].Emit ();

			if (OptAttributes != null)
				OptAttributes.Emit ();
		}

		public override bool DefineMembers ()
		{
			return true;
		}

		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria)
		{
			throw new Exception ();
		}		

		public override MemberCache MemberCache {
			get {
				return null;
			}
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Method | AttributeTargets.ReturnValue;
			}
		}

		public override string DocCommentHeader {
			get { return "M:"; }
		}

		public new void VerifyClsCompliance ()
		{
			foreach (TypeParameter tp in TypeParameters) {
				if (tp.Constraints == null)
					continue;

				tp.Constraints.VerifyClsCompliance ();
			}
		}
	}

	public partial class TypeManager
	{
		public static TypeContainer LookupGenericTypeContainer (Type t)
		{
			t = DropGenericTypeArguments (t);
			return LookupTypeContainer (t);
		}

		/// <summary>
		///   Check whether `a' and `b' may become equal generic types.
		///   The algorithm to do that is a little bit complicated.
		/// </summary>
		public static bool MayBecomeEqualGenericTypes (Type a, Type b, Type[] class_inferred,
							       Type[] method_inferred)
		{
			if (a.IsGenericParameter) {
				//
				// If a is an array of a's type, they may never
				// become equal.
				//
				while (b.IsArray) {
					b = b.GetElementType ();
					if (a.Equals (b))
						return false;
				}

				//
				// If b is a generic parameter or an actual type,
				// they may become equal:
				//
				//    class X<T,U> : I<T>, I<U>
				//    class X<T> : I<T>, I<float>
				// 
				if (b.IsGenericParameter || !b.IsGenericType) {
					int pos = a.GenericParameterPosition;
					Type[] args = a.DeclaringMethod != null ? method_inferred : class_inferred;
					if (args [pos] == null) {
						args [pos] = b;
						return true;
					}

					return args [pos] == a;
				}

				//
				// We're now comparing a type parameter with a
				// generic instance.  They may become equal unless
				// the type parameter appears anywhere in the
				// generic instance:
				//
				//    class X<T,U> : I<T>, I<X<U>>
				//        -> error because you could instanciate it as
				//           X<X<int>,int>
				//
				//    class X<T> : I<T>, I<X<T>> -> ok
				//

				Type[] bargs = GetTypeArguments (b);
				for (int i = 0; i < bargs.Length; i++) {
					if (a.Equals (bargs [i]))
						return false;
				}

				return true;
			}

			if (b.IsGenericParameter)
				return MayBecomeEqualGenericTypes (b, a, class_inferred, method_inferred);

			//
			// At this point, neither a nor b are a type parameter.
			//
			// If one of them is a generic instance, let
			// MayBecomeEqualGenericInstances() compare them (if the
			// other one is not a generic instance, they can never
			// become equal).
			//

			if (a.IsGenericType || b.IsGenericType)
				return MayBecomeEqualGenericInstances (a, b, class_inferred, method_inferred);

			//
			// If both of them are arrays.
			//

			if (a.IsArray && b.IsArray) {
				if (a.GetArrayRank () != b.GetArrayRank ())
					return false;
			
				a = a.GetElementType ();
				b = b.GetElementType ();

				return MayBecomeEqualGenericTypes (a, b, class_inferred, method_inferred);
			}

			//
			// Ok, two ordinary types.
			//

			return a.Equals (b);
		}

		//
		// Checks whether two generic instances may become equal for some
		// particular instantiation (26.3.1).
		//
		public static bool MayBecomeEqualGenericInstances (Type a, Type b,
								   Type[] class_inferred,
								   Type[] method_inferred)
		{
			if (!a.IsGenericType || !b.IsGenericType)
				return false;
			if (a.GetGenericTypeDefinition () != b.GetGenericTypeDefinition ())
				return false;

			return MayBecomeEqualGenericInstances (
				GetTypeArguments (a), GetTypeArguments (b), class_inferred, method_inferred);
		}

		public static bool MayBecomeEqualGenericInstances (Type[] aargs, Type[] bargs,
								   Type[] class_inferred,
								   Type[] method_inferred)
		{
			if (aargs.Length != bargs.Length)
				return false;

			for (int i = 0; i < aargs.Length; i++) {
				if (!MayBecomeEqualGenericTypes (aargs [i], bargs [i], class_inferred, method_inferred))
					return false;
			}

			return true;
		}

		/// <summary>
		///   Type inference.  Try to infer the type arguments from `method',
		///   which is invoked with the arguments `arguments'.  This is used
		///   when resolving an Invocation or a DelegateInvocation and the user
		///   did not explicitly specify type arguments.
		/// </summary>
		public static int InferTypeArguments (EmitContext ec,
						       ArrayList arguments,
						       ref MethodBase method)
		{
			ATypeInference ti = ATypeInference.CreateInstance (arguments);
			Type[] i_args = ti.InferMethodArguments (ec, method);
			if (i_args == null)
				return ti.InferenceScore;

			if (i_args.Length == 0)
				return 0;

			method = ((MethodInfo) method).MakeGenericMethod (i_args);
			return 0;
		}

		/// <summary>
		///   Type inference.
		/// </summary>
		public static bool InferTypeArguments (ParameterData apd,
						       ref MethodBase method)
		{
			if (!TypeManager.IsGenericMethod (method))
				return true;

			ATypeInference ti = ATypeInference.CreateInstance (ArrayList.Adapter (apd.Types));
			Type[] i_args = ti.InferDelegateArguments (method);
			if (i_args == null)
				return false;

			method = ((MethodInfo) method).MakeGenericMethod (i_args);
			return true;
		}
	}

	abstract class ATypeInference
	{
		protected readonly ArrayList arguments;
		protected readonly int arg_count;

		protected ATypeInference (ArrayList arguments)
		{
			this.arguments = arguments;
			if (arguments != null)
				arg_count = arguments.Count;
		}

		public static ATypeInference CreateInstance (ArrayList arguments)
		{
			if (RootContext.Version == LanguageVersion.ISO_2)
				return new TypeInferenceV2 (arguments);

			return new TypeInferenceV3 (arguments);
		}

		public virtual int InferenceScore {
			get {
				return int.MaxValue;
			}
		}

		public abstract Type[] InferMethodArguments (EmitContext ec, MethodBase method);
		public abstract Type[] InferDelegateArguments (MethodBase method);
	}

	//
	// Implements C# 2.0 type inference
	//
	class TypeInferenceV2 : ATypeInference
	{
		public TypeInferenceV2 (ArrayList arguments)
			: base (arguments)
		{
		}

		public override Type[] InferDelegateArguments (MethodBase method)
		{
			ParameterData pd = TypeManager.GetParameterData (method);
			if (arg_count != pd.Count)
				return null;

			Type[] method_args = method.GetGenericArguments ();
			Type[] inferred_types = new Type[method_args.Length];

			Type[] param_types = new Type[pd.Count];
			Type[] arg_types = (Type[])arguments.ToArray (typeof (Type));

			for (int i = 0; i < arg_count; i++) {
				param_types[i] = pd.ParameterType (i);
			}

			if (!InferTypeArguments (param_types, arg_types, inferred_types))
				return null;

			return inferred_types;
		}

		public override Type[] InferMethodArguments (EmitContext ec, MethodBase method)
		{
			ParameterData pd = TypeManager.GetParameterData (method);
			Type[] method_generic_args = method.GetGenericArguments ();
			Type [] inferred_types = new Type [method_generic_args.Length];
			Type[] arg_types = new Type [pd.Count];

			int a_count = arg_types.Length;
			if (pd.HasParams)
				--a_count;

			for (int i = 0; i < a_count; i++) {
				Argument a = (Argument) arguments[i];
				if (a.Expr is NullLiteral || a.Expr is MethodGroupExpr || a.Expr is AnonymousMethodExpression)
					continue;

				if (!TypeInferenceV2.UnifyType (pd.ParameterType (i), a.Type, inferred_types))
					return null;
			}

			if (pd.HasParams) {
				Type element_type = TypeManager.GetElementType (pd.ParameterType (a_count));
				for (int i = a_count; i < arg_count; i++) {
					Argument a = (Argument) arguments [i];
					if (a.Expr is NullLiteral || a.Expr is MethodGroupExpr || a.Expr is AnonymousMethodExpression)
						continue;

					if (!TypeInferenceV2.UnifyType (element_type, a.Type, inferred_types))
						return null;
				}
			}

			for (int i = 0; i < inferred_types.Length; i++)
				if (inferred_types [i] == null)
					return null;

			return inferred_types;
		}

		static bool InferTypeArguments (Type[] param_types, Type[] arg_types,
				Type[] inferred_types)
		{
			for (int i = 0; i < arg_types.Length; i++) {
				if (arg_types[i] == null)
					continue;

				if (!UnifyType (param_types[i], arg_types[i], inferred_types))
					return false;
			}

			for (int i = 0; i < inferred_types.Length; ++i)
				if (inferred_types[i] == null)
					return false;

			return true;
		}

		public static bool UnifyType (Type pt, Type at, Type[] inferred)
		{
			if (pt.IsGenericParameter) {
				if (pt.DeclaringMethod == null)
					return pt == at;

				int pos = pt.GenericParameterPosition;

				if (inferred [pos] == null)
					inferred [pos] = at;

				return inferred [pos] == at;
			}

			if (!pt.ContainsGenericParameters) {
				if (at.ContainsGenericParameters)
					return UnifyType (at, pt, inferred);
				else
					return true;
			}

			if (at.IsArray) {
				if (pt.IsArray) {
					if (at.GetArrayRank () != pt.GetArrayRank ())
						return false;

					return UnifyType (pt.GetElementType (), at.GetElementType (), inferred);
				}

				if (!pt.IsGenericType)
					return false;

				Type gt = pt.GetGenericTypeDefinition ();
				if ((gt != TypeManager.generic_ilist_type) && (gt != TypeManager.generic_icollection_type) &&
					(gt != TypeManager.generic_ienumerable_type))
					return false;

				Type[] args = TypeManager.GetTypeArguments (pt);
				return UnifyType (args[0], at.GetElementType (), inferred);
			}

			if (pt.IsArray) {
				if (!at.IsArray ||
					(pt.GetArrayRank () != at.GetArrayRank ()))
					return false;

				return UnifyType (pt.GetElementType (), at.GetElementType (), inferred);
			}

			if (pt.IsByRef && at.IsByRef)
				return UnifyType (pt.GetElementType (), at.GetElementType (), inferred);
			ArrayList list = new ArrayList ();
			if (at.IsGenericType)
				list.Add (at);
			for (Type bt = at.BaseType; bt != null; bt = bt.BaseType)
				list.Add (bt);

			list.AddRange (TypeManager.GetInterfaces (at));

			foreach (Type type in list) {
				if (!type.IsGenericType)
					continue;

				if (TypeManager.DropGenericTypeArguments (pt) != TypeManager.DropGenericTypeArguments (type))
					continue;

				if (!UnifyTypes (pt.GetGenericArguments (), type.GetGenericArguments (), inferred))
					return false;
			}

			return true;
		}

		static bool UnifyTypes (Type[] pts, Type[] ats, Type[] inferred)
		{
			for (int i = 0; i < ats.Length; i++) {
				if (!UnifyType (pts [i], ats [i], inferred))
					return false;
			}
			return true;
		}
	}

	//
	// Implements C# 3.0 type inference
	//
	class TypeInferenceV3 : ATypeInference
	{
		//
		// Tracks successful rate of type inference
		//
		int score = int.MaxValue;

		public TypeInferenceV3 (ArrayList arguments)
			: base (arguments)
		{
		}

		public override int InferenceScore {
			get {
				return score;
			}
		}

		public override Type[] InferDelegateArguments (MethodBase method)
		{
			ParameterData pd = TypeManager.GetParameterData (method);
			if (arg_count != pd.Count)
				return null;

			Type[] d_gargs = method.GetGenericArguments ();
			TypeInferenceContext context = new TypeInferenceContext (d_gargs);

			// A lower-bound inference is made from each argument type Uj of D
			// to the corresponding parameter type Tj of M
			for (int i = 0; i < arg_count; ++i) {
				Type t = pd.Types [i];
				if (!t.IsGenericParameter)
					continue;

				context.LowerBoundInference ((Type)arguments[i], t);
			}

			if (!context.FixAllTypes ())
				return null;

			return context.InferredTypeArguments;
		}

		public override Type[] InferMethodArguments (EmitContext ec, MethodBase method)
		{
			Type[] method_generic_args = method.GetGenericArguments ();
			TypeInferenceContext context = new TypeInferenceContext (method_generic_args);
			if (!context.UnfixedVariableExists)
				return Type.EmptyTypes;

			ParameterData pd = TypeManager.GetParameterData (method);
			if (!InferInPhases (ec, context, pd))
				return null;

			return context.InferredTypeArguments;
		}

		//
		// Implements method type arguments inference
		//
		bool InferInPhases (EmitContext ec, TypeInferenceContext tic, ParameterData methodParameters)
		{
			int params_arguments_start;
			if (methodParameters.HasParams) {
				params_arguments_start = methodParameters.Count - 1;
			} else {
				params_arguments_start = arg_count;
			}
			
			//
			// The first inference phase
			//
			Type method_parameter = null;
			for (int i = 0; i < arg_count; i++) {
				Argument a = (Argument) arguments [i];
				
				if (i < params_arguments_start) {
					method_parameter = methodParameters.Types [i];
				} else if (i == params_arguments_start) {
					if (arg_count == params_arguments_start + 1 && TypeManager.HasElementType (a.Type))
						method_parameter = methodParameters.Types [params_arguments_start];
					else
						method_parameter = TypeManager.GetElementType (methodParameters.Types [params_arguments_start]);
				}

				//
				// When a lambda expression, an anonymous method
				// is used an explicit argument type inference takes a place
				//
				AnonymousMethodExpression am = a.Expr as AnonymousMethodExpression;
				if (am != null) {
					if (am.ExplicitTypeInference (tic, method_parameter))
						--score; 
					continue;
				}

				if (a.Expr.Type == TypeManager.null_type)
					continue;

				//
				// Otherwise an output type inference is made
				//
				score -= tic.OutputTypeInference (ec, a.Expr, method_parameter);
			}

			//
			// Part of the second phase but because it happens only once
			// we don't need to call it in cycle
			//
			bool fixed_any = false;
			if (!tic.FixIndependentTypeArguments (methodParameters, ref fixed_any))
				return false;

			return DoSecondPhase (ec, tic, methodParameters, !fixed_any);
		}

		bool DoSecondPhase (EmitContext ec, TypeInferenceContext tic, ParameterData methodParameters, bool fixDependent)
		{
			bool fixed_any = false;
			if (fixDependent && !tic.FixDependentTypes (methodParameters, ref fixed_any))
				return false;

			// If no further unfixed type variables exist, type inference succeeds
			if (!tic.UnfixedVariableExists)
				return true;

			if (!fixed_any && fixDependent)
				return false;
			
			// For all arguments where the corresponding argument output types
			// contain unfixed type variables but the input types do not,
			// an output type inference is made
			for (int i = 0; i < arg_count; i++) {
				Type t_i = methodParameters.ParameterType (i);
				if (!TypeManager.IsDelegateType (t_i)) {
					if (TypeManager.DropGenericTypeArguments (t_i) != TypeManager.expression_type)
						continue;

					t_i = t_i.GetGenericArguments () [0];
				}

				MethodInfo mi = Delegate.GetInvokeMethod (t_i, t_i);
				Type rtype = mi.ReturnType;

#if MS_COMPATIBLE
				// Blablabla, because reflection does not work with dynamic types
				Type[] g_args = t_i.GetGenericArguments ();
				rtype = g_args[rtype.GenericParameterPosition];
#endif

				if (tic.IsReturnTypeNonDependent (mi, rtype))
					score -= tic.OutputTypeInference (ec, ((Argument) arguments [i]).Expr, t_i);
			}


			return DoSecondPhase (ec, tic, methodParameters, true);
		}
	}

	public class TypeInferenceContext
	{
		readonly Type[] unfixed_types;
		readonly Type[] fixed_types;
		readonly ArrayList[] bounds;
		
		public TypeInferenceContext (Type[] typeArguments)
		{
			if (typeArguments.Length == 0)
				throw new ArgumentException ("Empty generic arguments");

			fixed_types = new Type [typeArguments.Length];
			for (int i = 0; i < typeArguments.Length; ++i) {
				if (typeArguments [i].IsGenericParameter) {
					if (bounds == null) {
						bounds = new ArrayList [typeArguments.Length];
						unfixed_types = new Type [typeArguments.Length];
					}
					unfixed_types [i] = typeArguments [i];
				} else {
					fixed_types [i] = typeArguments [i];
				}
			}
		}

		public Type[] InferredTypeArguments {
			get {
				return fixed_types;
			}
		}

		void AddToBounds (Type t, int index)
		{
			ArrayList a = bounds [index];
			if (a == null) {
				a = new ArrayList ();
				bounds [index] = a;
			} else {
				if (a.Contains (t))
					return;
			}

			//
			// SPEC: does not cover type inference using constraints
			//
			if (TypeManager.IsGenericParameter (t)) {
				GenericConstraints constraints = TypeManager.GetTypeParameterConstraints (t);
				if (constraints != null) {
					//if (constraints.EffectiveBaseClass != null)
					//	t = constraints.EffectiveBaseClass;
				}
			}
			a.Add (t);
		}
		
		bool AllTypesAreFixed (Type[] types)
		{
			foreach (Type t in types) {
				if (t.IsGenericParameter) {
					if (!IsFixed (t))
						return false;
					continue;
				}

				if (t.IsGenericType)
					return AllTypesAreFixed (t.GetGenericArguments ());
			}
			
			return true;
		}		

		//
		// 26.3.3.8 Exact Inference
		//
		public int ExactInference (Type u, Type v)
		{
			// If V is an array type
			if (v.IsArray) {
				if (!u.IsArray)
					return 0;

				if (u.GetArrayRank () != v.GetArrayRank ())
					return 0;

				return ExactInference (TypeManager.GetElementType (u), TypeManager.GetElementType (v));
			}

			// If V is constructed type and U is constructed type
			if (v.IsGenericType && !v.IsGenericTypeDefinition) {
				if (!u.IsGenericType)
					return 0;

				Type [] ga_u = u.GetGenericArguments ();
				Type [] ga_v = v.GetGenericArguments ();
				if (ga_u.Length != ga_v.Length)
					return 0;

				int score = 0;
				for (int i = 0; i < ga_u.Length; ++i)
					score += ExactInference (ga_u [i], ga_v [i]);

				return score > 0 ? 1 : 0;
			}

			// If V is one of the unfixed type arguments
			int pos = IsUnfixed (v);
			if (pos == -1)
				return 0;

			AddToBounds (u, pos);
			return 1;
		}

		public bool FixAllTypes ()
		{
			for (int i = 0; i < unfixed_types.Length; ++i) {
				if (!FixType (i))
					return false;
			}
			return true;
		}

		//
		// All unfixed type variables Xi are fixed for which all of the following hold:
		// a, There is at least one type variable Xj that depends on Xi
		// b, Xi has a non-empty set of bounds
		// 
		public bool FixDependentTypes (ParameterData methodParameters, ref bool fixed_any)
		{
			for (int i = 0; i < unfixed_types.Length; ++i) {
				if (unfixed_types[i] == null)
					continue;

				if (bounds[i] == null)
					continue;

				if (!FixType (i))
					return false;
				
				fixed_any = true;
			}

			return true;
		}

		//
		// All unfixed type variables Xi which depend on no Xj are fixed
		//
		public bool FixIndependentTypeArguments (ParameterData methodParameters, ref bool fixed_any)
		{
			ArrayList types_to_fix = new ArrayList (unfixed_types);
			for (int i = 0; i < methodParameters.Types.Length; ++i) {
				Type t = methodParameters.Types [i];
				if (t.IsGenericParameter)
					continue;

				if (!TypeManager.IsDelegateType (t)) {
					if (TypeManager.DropGenericTypeArguments (t) != TypeManager.expression_type)
						continue;

					t = t.GetGenericArguments () [0];
				}

				MethodInfo invoke = Delegate.GetInvokeMethod (t, t);
				Type rtype = invoke.ReturnType;
				if (!rtype.IsGenericParameter && !rtype.IsGenericType)
					continue;

#if MS_COMPATIBLE
				// Blablabla, because reflection does not work with dynamic types
				if (rtype.IsGenericParameter) {
					Type [] g_args = t.GetGenericArguments ();
					rtype = g_args [rtype.GenericParameterPosition];
				}
#endif
				// Remove dependent types, they cannot be fixed yet
				RemoveDependentTypes (types_to_fix, rtype);
			}

			foreach (Type t in types_to_fix) {
				if (t == null)
					continue;

				int idx = IsUnfixed (t);
				if (idx >= 0 && !FixType (idx)) {
					return false;
				}
			}

			fixed_any = types_to_fix.Count > 0;
			return true;
		}

		//
		// 26.3.3.10 Fixing
		//
		public bool FixType (int i)
		{
			// It's already fixed
			if (unfixed_types[i] == null)
				throw new InternalErrorException ("Type argument has been already fixed");

			ArrayList candidates = (ArrayList)bounds [i];
			if (candidates == null)
				return false;

			if (candidates.Count == 1) {
				unfixed_types[i] = null;
				fixed_types[i] = (Type)candidates[0];
				return true;
			}

			//
			// Determines a unique type from which there is
			// a standard implicit conversion to all the other
			// candidate types.
			//
			Type best_candidate = null;
			int cii;
			int candidates_count = candidates.Count;
			for (int ci = 0; ci < candidates_count; ++ci) {
				Type candidate = (Type)candidates [ci];
				for (cii = 0; cii < candidates_count; ++cii) {
					if (cii == ci)
						continue;

					if (!Convert.ImplicitConversionExists (null,
						new TypeExpression ((Type)candidates [cii], Location.Null), candidate)) {
						break;
					}
				}

				if (cii != candidates_count)
					continue;

				if (best_candidate != null)
					return false;

				best_candidate = candidate;
			}

			if (best_candidate == null)
				return false;

			unfixed_types[i] = null;
			fixed_types[i] = best_candidate;
			return true;
		}
		
		//
		// Uses inferred types to inflate delegate type argument
		//
		public Type InflateGenericArgument (Type parameter)
		{
			if (parameter.IsGenericParameter)
				return fixed_types [parameter.GenericParameterPosition];

			if (parameter.IsGenericType) {
				Type [] parameter_targs = parameter.GetGenericArguments ();
				for (int ii = 0; ii < parameter_targs.Length; ++ii) {
					parameter_targs [ii] = InflateGenericArgument (parameter_targs [ii]);
				}
				return parameter.GetGenericTypeDefinition ().MakeGenericType (parameter_targs);
			}

			return parameter;
		}
		
		//
		// Tests whether all delegate input arguments are fixed and generic output type
		// requires output type inference 
		//
		public bool IsReturnTypeNonDependent (MethodInfo invoke, Type returnType)
		{
			if (returnType.IsGenericParameter) {
				if (IsFixed (returnType))
				    return false;
			} else if (returnType.IsGenericType) {
				if (TypeManager.IsDelegateType (returnType)) {
					invoke = Delegate.GetInvokeMethod (returnType, returnType);
					return IsReturnTypeNonDependent (invoke, invoke.ReturnType);
				}
					
				Type[] g_args = returnType.GetGenericArguments ();
				
				// At least one unfixed return type has to exist 
				if (AllTypesAreFixed (g_args))
					return false;
			} else {
				return false;
			}

			// All generic input arguments have to be fixed
			ParameterData d_parameters = TypeManager.GetParameterData (invoke);
			return AllTypesAreFixed (d_parameters.Types);
		}
		
		bool IsFixed (Type type)
		{
			return IsUnfixed (type) == -1;
		}		

		int IsUnfixed (Type type)
		{
			if (!type.IsGenericParameter)
				return -1;

			//return unfixed_types[type.GenericParameterPosition] != null;
			for (int i = 0; i < unfixed_types.Length; ++i) {
				if (unfixed_types [i] == type)
					return i;
			}

			return -1;
		}

		//
		// 26.3.3.9 Lower-bound Inference
		//
		public int LowerBoundInference (Type u, Type v)
		{
			// Remove ref, out modifiers
			if (v.IsByRef)
				v = v.GetElementType ();
			
			// If V is one of the unfixed type arguments
			int pos = IsUnfixed (v);
			if (pos != -1) {
				AddToBounds (u, pos);
				return 1;
			}			

			// If U is an array type
			if (u.IsArray) {
				int u_dim = u.GetArrayRank ();
				Type v_e;
				Type u_e = TypeManager.GetElementType (u);

				if (v.IsArray) {
					if (u_dim != v.GetArrayRank ())
						return 0;

					v_e = TypeManager.GetElementType (v);

					if (u.IsByRef) {
						return LowerBoundInference (u_e, v_e);
					}

					return ExactInference (u_e, v_e);
				}

				if (u_dim != 1)
					return 0;

				if (v.IsGenericType) {
					Type g_v = v.GetGenericTypeDefinition ();
					if ((g_v != TypeManager.generic_ilist_type) && (g_v != TypeManager.generic_icollection_type) &&
						(g_v != TypeManager.generic_ienumerable_type))
						return 0;

					v_e = TypeManager.GetTypeArguments (v)[0];

					if (u.IsByRef) {
						return LowerBoundInference (u_e, v_e);
					}

					return ExactInference (u_e, v_e);
				}
			} else if (v.IsGenericType && !v.IsGenericTypeDefinition) {
				//
				// if V is a constructed type C<V1..Vk> and there is a unique set of types U1..Uk
				// such that a standard implicit conversion exists from U to C<U1..Uk> then an exact
				// inference is made from each Ui for the corresponding Vi
				//
				ArrayList u_candidates = new ArrayList ();
				if (u.IsGenericType)
					u_candidates.Add (u);

				for (Type t = u.BaseType; t != null; t = t.BaseType) {
					if (t.IsGenericType && !t.IsGenericTypeDefinition)
						u_candidates.Add (t);
				}

				// TODO: Implement GetGenericInterfaces only and remove
				// the if from foreach
				u_candidates.AddRange (TypeManager.GetInterfaces (u));

				Type open_v = v.GetGenericTypeDefinition ();
				int score = 0;
				foreach (Type u_candidate in u_candidates) {
					if (!u_candidate.IsGenericType || u_candidate.IsGenericTypeDefinition)
						continue;

					if (TypeManager.DropGenericTypeArguments (u_candidate) != open_v)
						continue;

					Type [] ga_u = u_candidate.GetGenericArguments ();
					Type [] ga_v = v.GetGenericArguments ();
					bool all_exact = true;
					for (int i = 0; i < ga_u.Length; ++i)
						if (ExactInference (ga_u [i], ga_v [i]) == 0)
							all_exact = false;

					if (all_exact && score == 0)
						++score;
				}
				return score;
			}

			return 0;
		}

		//
		// 26.3.3.6 Output Type Inference
		//
		public int OutputTypeInference (EmitContext ec, Expression e, Type t)
		{
			// If e is a lambda or anonymous method with inferred return type
			AnonymousMethodExpression ame = e as AnonymousMethodExpression;
			if (ame != null) {
				Type rt = ame.InferReturnType (ec, this, t);
				MethodInfo invoke = Delegate.GetInvokeMethod (t, t);

				if (rt == null) {
					ParameterData pd = TypeManager.GetParameterData (invoke);
					return ame.Parameters.Count == pd.Count ? 1 : 0;
				}

				Type rtype = invoke.ReturnType;
#if MS_COMPATIBLE
				// Blablabla, because reflection does not work with dynamic types
				Type [] g_args = t.GetGenericArguments ();
				rtype = g_args [rtype.GenericParameterPosition];
#endif
				return LowerBoundInference (rt, rtype) + 1;
			}

			if (e is MethodGroupExpr) {
				if (!TypeManager.IsDelegateType (t))
					return 0;

				MethodInfo invoke = Delegate.GetInvokeMethod (t, t);
				Type rtype = invoke.ReturnType;
				if (!TypeManager.IsGenericType (rtype))
					return 0;
				
				throw new NotImplementedException ();
			}

			//
			// if e is an expression with type U, then
			// a lower-bound inference is made from U for T
			//
			return LowerBoundInference (e.Type, t) * 2;
		}

		static void RemoveDependentTypes (ArrayList types, Type returnType)
		{
			if (returnType.IsGenericParameter) {
				types [returnType.GenericParameterPosition] = null;
				return;
			}

			if (returnType.IsGenericType) {
				foreach (Type t in returnType.GetGenericArguments ()) {
					RemoveDependentTypes (types, t);
				}
			}
		}

		public bool UnfixedVariableExists {
			get {
				if (unfixed_types == null)
					return false;

				foreach (Type ut in unfixed_types)
					if (ut != null)
						return true;
				return false;
			}
		}
	}
}
