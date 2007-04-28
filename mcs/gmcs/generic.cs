//
// generic.cs: Generics support
//
// Authors: Martin Baulig (martin@ximian.com)
//          Miguel de Icaza (miguel@ximian.com)
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

				// TODO: It's aleady done in ResolveAsBaseTerminal
				if (!ec.GenericDeclContainer.AsAccessible (fn.Type, ec.GenericDeclContainer.ModFlags)) {
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
				    (class_constraint_type == TypeManager.object_type)) {
					Report.Error (702, loc,
						      "Bound cannot be special class `{0}'",
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

			foreach (Type iface in gc.InterfaceConstraints) {
				bool ok = false;
				foreach (Type check in InterfaceConstraints) {
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
	public class TypeParameter : MemberCore, IMemberContainer {
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

		public void EmitAttributes ()
		{
			if (OptAttributes != null)
				OptAttributes.Emit ();
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
				return (AttributeTargets) AttributeTargets.GenericParameter;
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return new string [] { "type parameter" };
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
			return FindMembers (mt, bf, null, null);
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

				Report.Error (81, Location, "Type parameter declaration " +
					      "must be an identifier not a type");
				return null;
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

		public override bool AsAccessible (DeclSpace ds, int flags)
		{
			foreach (Type t in atypes) {
				if (!ds.AsAccessible (t, flags))
					return false;
			}

			return ds.AsAccessible (gt, flags);
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
				else
					return tparam.GenericConstraints.HasConstructorConstraint;
			}

			MemberList list = TypeManager.FindMembers (
				atype, MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance |
				BindingFlags.DeclaredOnly, null, null);

			if (atype.IsAbstract || (list == null))
				return false;

			foreach (MethodBase mb in list) {
				ParameterData pd = TypeManager.GetParameterData (mb);
				if ((pd.Count == 0) && mb.IsPublic && !mb.IsStatic)
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
				if (block != null) {
					LocalInfo li = (LocalInfo)block.Variables[type_argument_name];
					if (li != null) {
						Error_ParameterNameCollision (li.Location, type_argument_name, "local variable");
						return false;
					}
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

		static void Error_ParameterNameCollision (Location loc, string name, string collisionWith)
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
				TypeParameters [i].EmitAttributes ();

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

	public class DefaultValueExpression : Expression
	{
		Expression expr;

		public DefaultValueExpression (Expression expr, Location loc)
		{
			this.expr = expr;
			this.loc = loc;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			TypeExpr texpr = expr.ResolveAsTypeTerminal (ec, false);
			if (texpr == null)
				return null;

			type = texpr.Type;

			if (type == TypeManager.void_type) {
				Error_VoidInvalidInTheContext (loc);
				return null;
			}

			if (type.IsGenericParameter)
			{
				GenericConstraints constraints = TypeManager.GetTypeParameterConstraints(type);
				if (constraints != null && constraints.IsReferenceType)
					return new NullDefault (new NullLiteral (Location), type);
			}
			else
			{
				Constant c = New.Constantify(type);
				if (c != null)
					return new NullDefault (c, type);

				if (!TypeManager.IsValueType (type))
					return new NullDefault (new NullLiteral (Location), type);
			}
			eclass = ExprClass.Variable;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			LocalTemporary temp_storage = new LocalTemporary(type);

			temp_storage.AddressOf(ec, AddressOp.LoadStore);
			ec.ig.Emit(OpCodes.Initobj, type);
			temp_storage.Emit(ec);
		}
	}

	public class NullableType : TypeExpr
	{
		Expression underlying;

		public NullableType (Expression underlying, Location l)
		{
			this.underlying = underlying;
			loc = l;

			eclass = ExprClass.Type;
		}

		public NullableType (Type type, Location loc)
			: this (new TypeExpression (type, loc), loc)
		{ }

		public override string Name {
			get { return underlying.ToString () + "?"; }
		}

		public override string FullName {
			get { return underlying.ToString () + "?"; }
		}

		protected override TypeExpr DoResolveAsTypeStep (IResolveContext ec)
		{
			TypeArguments args = new TypeArguments (loc);
			args.Add (underlying);

			ConstructedType ctype = new ConstructedType (TypeManager.generic_nullable_type, args, loc);
			return ctype.ResolveAsTypeTerminal (ec, false);
		}
	}

	public partial class TypeManager
	{
		//
		// A list of core types that the compiler requires or uses
		//
		static public Type activator_type;
		static public Type generic_ilist_type;
		static public Type generic_icollection_type;
		static public Type generic_ienumerator_type;
		static public Type generic_ienumerable_type;
		static public Type generic_nullable_type;

		//
		// These methods are called by code generated by the compiler
		//
		static public MethodInfo activator_create_instance;

		static void InitGenericCoreTypes ()
		{
			activator_type = CoreLookupType ("System", "Activator");

			generic_ilist_type = CoreLookupType (
				"System.Collections.Generic", "IList", 1);
			generic_icollection_type = CoreLookupType (
				"System.Collections.Generic", "ICollection", 1);
			generic_ienumerator_type = CoreLookupType (
				"System.Collections.Generic", "IEnumerator", 1);
			generic_ienumerable_type = CoreLookupType (
				"System.Collections.Generic", "IEnumerable", 1);
			generic_nullable_type = CoreLookupType (
				"System", "Nullable", 1);
		}

		static void InitGenericCodeHelpers ()
		{
			// Activator
			activator_create_instance = GetMethod (
				activator_type, "CreateInstance", Type.EmptyTypes);
		}

		static Type CoreLookupType (string ns, string name, int arity)
		{
			return CoreLookupType (ns, MemberName.MakeName (name, arity));
		}

		public static TypeContainer LookupGenericTypeContainer (Type t)
		{
			t = DropGenericTypeArguments (t);
			return LookupTypeContainer (t);
		}

		public static GenericConstraints GetTypeParameterConstraints (Type t)
		{
			if (!t.IsGenericParameter)
				throw new InvalidOperationException ();

			TypeParameter tparam = LookupTypeParameter (t);
			if (tparam != null)
				return tparam.GenericConstraints;

			return ReflectionConstraints.GetConstraints (t);
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

		static bool UnifyType (Type pt, Type at, Type[] inferred)
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
				if ((gt != generic_ilist_type) && (gt != generic_icollection_type) &&
				    (gt != generic_ienumerable_type))
					return false;

				Type[] args = GetTypeArguments (pt);
				return UnifyType (args [0], at.GetElementType (), inferred);
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

				if (DropGenericTypeArguments (pt) != DropGenericTypeArguments (type))
					continue;

				if (!UnifyTypes (pt.GetGenericArguments (), type.GetGenericArguments (), inferred))
					return false;
			}

			return true;
		}

		static bool UnifyTypes (Type[] pts, Type [] ats, Type [] inferred)
		{
			for (int i = 0; i < ats.Length; i++) {
				if (!UnifyType (pts [i], ats [i], inferred))
					return false;
			}
			return true;
		}

		/// <summary>
		///   Type inference.  Try to infer the type arguments from the params method
		///   `method', which is invoked with the arguments `arguments'.  This is used
		///   when resolving an Invocation or a DelegateInvocation and the user
		///   did not explicitly specify type arguments.
		/// </summary>
		public static bool InferParamsTypeArguments (EmitContext ec, ArrayList arguments,
							     ref MethodBase method)
		{
			if (!TypeManager.IsGenericMethod (method))
				return true;

			// if there are no arguments, there's no way to infer the type-arguments
			if (arguments == null || arguments.Count == 0)
				return false;

			ParameterData pd = TypeManager.GetParameterData (method);
			int pd_count = pd.Count;
			int arg_count = arguments.Count;

			if (pd_count == 0)
				return false;

			if (pd.ParameterModifier (pd_count - 1) != Parameter.Modifier.PARAMS)
				return false;

			if (pd_count - 1 > arg_count)
				return false;

			Type[] method_args = method.GetGenericArguments ();
			Type[] inferred_types = new Type [method_args.Length];

			//
			// If we have come this far, the case which
			// remains is when the number of parameters is
			// less than or equal to the argument count.
			//
			for (int i = 0; i < pd_count - 1; ++i) {
				Argument a = (Argument) arguments [i];

				if ((a.Expr is NullLiteral) || (a.Expr is MethodGroupExpr))
					continue;

				Type pt = pd.ParameterType (i);
				Type at = a.Type;

				if (!UnifyType (pt, at, inferred_types))
					return false;
			}

			Type element_type = TypeManager.GetElementType (pd.ParameterType (pd_count - 1));

			for (int i = pd_count - 1; i < arg_count; i++) {
				Argument a = (Argument) arguments [i];

				if ((a.Expr is NullLiteral) || (a.Expr is MethodGroupExpr))
					continue;

				if (!UnifyType (element_type, a.Type, inferred_types))
					return false;
			}

			for (int i = 0; i < inferred_types.Length; i++)
				if (inferred_types [i] == null)
					return false;

			method = ((MethodInfo)method).MakeGenericMethod (inferred_types);
			return true;
		}

		static bool InferTypeArguments (Type[] param_types, Type[] arg_types,
						Type[] inferred_types)
		{
			for (int i = 0; i < arg_types.Length; i++) {
				if (arg_types [i] == null)
					continue;

				if (!UnifyType (param_types [i], arg_types [i], inferred_types))
					return false;
			}

			for (int i = 0; i < inferred_types.Length; ++i)
				if (inferred_types [i] == null)
					return false;

			return true;
		}

		//
		// Infers the type of a single LambdaExpression in the invocation call and
		// stores the infered type in the inferred_types array.
		//
		// The index of the arguments that contain lambdas is passed in
		//
		// @lambdas.  Merely to avoid rescanning the list.
		//
		// The method being called:
		//   @method_generic_args: The generic type arguments for the method being called
		//   @method_pd: The ParameterData for the method being called.
		//
		// The call site:
		//   @arguments: Arraylist of Argument()s.  The arguments being passed.
		//
		// Returns:
		//   @inferred_types: the array that is populated with our results.
		//
		// true if the code was able to do one inference.
		//
		static bool LambdaInfer (EmitContext ec,
					 Type [] method_generic_args,
					 ParameterData method_pd,
					 ArrayList arguments,
					 Type[] inferred_types,
					 ArrayList lambdas)
		{
			int last_count = lambdas.Count;

			for (int i = 0; i < last_count; i++){
				int argn = (int) lambdas [i];

				Argument a = (Argument) arguments [argn];

				LambdaExpression le = a.Expr as LambdaExpression;

				if (le == null)
					throw new Exception (
					     String.Format ("Internal Compiler error: argument {0} should be a Lambda Expression",
							    argn));
							     
				//
				// "The corresponding parameter’s type, in the
				// following called P, is a delegate type with a
				// return type that involves one or more method type
				// parameters."
				//
				// 
				if (!TypeManager.IsDelegateType (method_pd.ParameterType (argn)))
					goto useless_lambda;
				
				Type p_type = method_pd.ParameterType (argn);
				MethodGroupExpr method_group = Expression.MemberLookup (
					ec.ContainerType, p_type, "Invoke", MemberTypes.Method,
					Expression.AllBindingFlags, Location.Null) as MethodGroupExpr;
				
				if (method_group == null){
					// This we report elsewhere as -200, but here we can ignore
					goto useless_lambda;
				}
				MethodInfo p_delegate_method = method_group.Methods [0] as MethodInfo;
				if (p_delegate_method == null){
					// This should not happen.
					goto useless_lambda;
				}
				
				Type p_return_type = p_delegate_method.ReturnType;
				if (!p_return_type.IsGenericParameter)
					goto useless_lambda;
				
				//
				// P and L have the same number of parameters, and
				// each parameter in P has the same modifiers as the
				// corresponding parameter in L, or no modifiers if
				// L has an implicitly typed parameter list.
				//
				ParameterData p_delegate_parameters = TypeManager.GetParameterData (p_delegate_method);
				int p_delegate_parameter_count = p_delegate_parameters.Count;
				if (p_delegate_parameter_count != le.Parameters.Count)
					goto useless_lambda;

				if (le.HasExplicitParameters){
					for (int j = 0; j < p_delegate_parameter_count; j++){
						if (p_delegate_parameters.ParameterModifier (j) != 
						    le.Parameters.ParameterModifier (j))
							goto useless_lambda;
					}
				} else { 
					for (int j = 0; j < p_delegate_parameter_count; j++)
						if (le.Parameters.ParameterModifier (j) != Parameter.Modifier.NONE)
							goto useless_lambda;
				}
				
				//
				// TODO: P’s parameter types involve no method type
				// parameters or involve only method type parameters
				// for which a consistent set of inferences have
				// already been made.
				//
				//Console.WriteLine ("Method: {0}", p_delegate_method);
				//for (int j = 0; j < p_delegate_parameter_count; j++){
				//Console.WriteLine ("PType [{2}, {0}] = {1}", j, p_delegate_parameters.ParameterType (j), argn);
				//}
				
				//
				// At this point we know that P has method type parameters
				// that involve only type parameters that have a consistent
				// set of inferences made.
				//
				if (le.HasExplicitParameters){
					//
					// TODO: If L has an explicitly typed parameter
					// list, when inferred types are substituted for
					// method type parameters in P, each parameter in P
					// has the same type as the corresponding parameter
					// in L.
					//
				} else {
					//
					// TODO: If L has an implicitly typed parameter
					// list, when inferred types are substituted for
					// method type parameters in P and the resulting
					// parameter types are given to the parameters of L,
					// the body of L is a valid expression or statement
					// block.

					Type [] types = new Type [p_delegate_parameter_count];

					bool failure = false;
					for (int j = 0; j < p_delegate_parameter_count; j++){
						Type p_pt = p_delegate_parameters.ParameterType (j);

						if (!p_pt.IsGenericParameter){
							types [j] = p_pt;
							continue;
						}

						bool found = false;
						for (int k = 0; k < method_generic_args.Length; k++){
							if (method_generic_args [k] == p_pt){
								types [j] = inferred_types [k];
								break;
							}
						}
						//
						// If we could not infer just yet, continue
						//
						if (types [j] == null)
							goto do_continue;
					}

					//
					// If it results in a valid expression or statement block
					//
					Type lambda_inferred_type = le.TryBuild (ec, types);

					if (lambda_inferred_type != null){
						//
						// Success, set the proper inferred_type value to the new type.
						// return true
						//
						for (int k = 0; k < method_generic_args.Length; k++){
							if (method_generic_args [k] == p_return_type){
								inferred_types [k] = lambda_inferred_type;

								lambdas.RemoveAt (i);
								return true;
							}
						}
					}
				}

			useless_lambda:
				lambdas.RemoveAt (i);
				
			do_continue:
				;
			}

#if false
			Console.WriteLine ("Inferred types");
			foreach (Type it in inferred_types){
				Console.WriteLine ("  IT: {0}", it);
				if (it == null)
					return false;
			}
#endif

			// No inference was made in any of the elements.
			return false;
		}
	
		/// <summary>
		///   Type inference.  Try to infer the type arguments from `method',
		///   which is invoked with the arguments `arguments'.  This is used
		///   when resolving an Invocation or a DelegateInvocation and the user
		///   did not explicitly specify type arguments.
		/// </summary>
		public static bool InferTypeArguments (EmitContext ec,
						       ArrayList arguments,
						       ref MethodBase method)
		{
			if (!TypeManager.IsGenericMethod (method))
				return true;

			int arg_count;
			if (arguments != null)
				arg_count = arguments.Count;
			else
				arg_count = 0;

			ParameterData pd = TypeManager.GetParameterData (method);
			if (arg_count != pd.Count)
				return false;

			Type[] method_generic_args = method.GetGenericArguments ();

			bool is_open = false;
			
			for (int i = 0; i < method_generic_args.Length; i++) {
				if (method_generic_args [i].IsGenericParameter) {
					is_open = true;
					break;
				}
			}

			// If none of the method parameters mention a generic parameter, we can't infer the generic parameters
			if (!is_open)
				return !TypeManager.IsGenericMethodDefinition (method);

			Type[] inferred_types = new Type [method_generic_args.Length];

			Type[] param_types = new Type [pd.Count];
			Type[] arg_types = new Type [pd.Count];
			ArrayList lambdas = null;
			
			for (int i = 0; i < arg_count; i++) {
				param_types [i] = pd.ParameterType (i);

				Argument a = (Argument) arguments [i];
				if (a.Expr is NullLiteral || a.Expr is MethodGroupExpr)
					continue;
								
				if (a.Expr is LambdaExpression){
					if (lambdas == null)
						lambdas = new ArrayList ();
					lambdas.Add (i);
				}
				else if (a.Expr is AnonymousMethodExpression) {
					if (RootContext.Version != LanguageVersion.LINQ)
						continue;

					Type dtype = param_types[i];
					if (!TypeManager.IsDelegateType (dtype))
						continue;

					AnonymousMethodExpression am = (AnonymousMethodExpression)a.Expr;

					Expression e = am.Compatible (ec, dtype);
					if (e == null)
						return false;

					arg_types[i] = e.Type;
					continue;
				}

				arg_types [i] = a.Type;
			}

			if (!InferTypeArguments (param_types, arg_types, inferred_types)){
				//Console.WriteLine ("InferTypeArgument found {0} lambdas ", lambdas);
				if (lambdas == null)
					return false;

				//
				// While the lambda expressions lead to a valid inference
				// 
				int lambda_count;
				do {
					lambda_count = lambdas.Count;
					if (!LambdaInfer (ec, method_generic_args, pd, arguments, inferred_types, lambdas))
						return false;
				} while (lambdas.Count != 0 && lambdas.Count != lambda_count);
			} 

			method = ((MethodInfo)method).MakeGenericMethod (inferred_types);

#if MS_COMPATIBLE
			// MS implementation throws NotSupportedException for GetParameters
			// on unbaked generic method
			ParameterData p = TypeManager.GetParameterData (method);
			p.InflateTypes (param_types, inferred_types);
#endif

			return true;
		}

		/// <summary>
		///   Type inference.
		/// </summary>
		public static bool InferTypeArguments (ParameterData apd,
						       ref MethodBase method)
		{
			if (!TypeManager.IsGenericMethod (method))
				return true;

			ParameterData pd = TypeManager.GetParameterData (method);
			if (apd.Count != pd.Count)
				return false;

			Type[] method_args = method.GetGenericArguments ();
			Type[] inferred_types = new Type [method_args.Length];

			Type[] param_types = new Type [pd.Count];
			Type[] arg_types = new Type [pd.Count];

			for (int i = 0; i < apd.Count; i++) {
				param_types [i] = pd.ParameterType (i);
				arg_types [i] = apd.ParameterType (i);
			}

			if (!InferTypeArguments (param_types, arg_types, inferred_types))
				return false;

			method = ((MethodInfo)method).MakeGenericMethod (inferred_types);
			return true;
		}
	}

	public abstract class Nullable
	{
		public sealed class NullableInfo
		{
			public readonly Type Type;
			public readonly Type UnderlyingType;
			public readonly MethodInfo HasValue;
			public readonly MethodInfo Value;
			public readonly ConstructorInfo Constructor;

			public NullableInfo (Type type)
			{
				Type = type;
				UnderlyingType = TypeManager.GetTypeArguments (type) [0];

				PropertyInfo has_value_pi = TypeManager.GetProperty (type, "HasValue");
				PropertyInfo value_pi = TypeManager.GetProperty (type, "Value");

				HasValue = has_value_pi.GetGetMethod (false);
				Value = value_pi.GetGetMethod (false);
				Constructor = type.GetConstructor (new Type[] { UnderlyingType });
			}
		}

		public class Unwrap : Expression, IMemoryLocation, IAssignMethod
		{
			Expression expr;
			NullableInfo info;

			LocalTemporary temp;
			bool has_temp;

			protected Unwrap (Expression expr)
			{
				this.expr = expr;
				this.loc = expr.Location;
			}

			public static Unwrap Create (Expression expr, EmitContext ec)
			{
				return new Unwrap (expr).Resolve (ec) as Unwrap;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				expr = expr.Resolve (ec);
				if (expr == null)
					return null;

				temp = new LocalTemporary (expr.Type);

				info = new NullableInfo (expr.Type);
				type = info.UnderlyingType;
				eclass = expr.eclass;
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				AddressOf (ec, AddressOp.LoadStore);
				ec.ig.EmitCall (OpCodes.Call, info.Value, null);
			}

			public void EmitCheck (EmitContext ec)
			{
				AddressOf (ec, AddressOp.LoadStore);
				ec.ig.EmitCall (OpCodes.Call, info.HasValue, null);
			}

			public void Store (EmitContext ec)
			{
				create_temp (ec);
			}

			void create_temp (EmitContext ec)
			{
				if ((temp != null) && !has_temp) {
					expr.Emit (ec);
					temp.Store (ec);
					has_temp = true;
				}
			}

			public void AddressOf (EmitContext ec, AddressOp mode)
			{
				create_temp (ec);
				if (temp != null)
					temp.AddressOf (ec, AddressOp.LoadStore);
				else
					((IMemoryLocation) expr).AddressOf (ec, AddressOp.LoadStore);
			}

			public void Emit (EmitContext ec, bool leave_copy)
			{
				create_temp (ec);
				if (leave_copy) {
					if (temp != null)
						temp.Emit (ec);
					else
						expr.Emit (ec);
				}

				Emit (ec);
			}

			public void EmitAssign (EmitContext ec, Expression source,
						bool leave_copy, bool prepare_for_load)
			{
				InternalWrap wrap = new InternalWrap (source, info, loc);
				((IAssignMethod) expr).EmitAssign (ec, wrap, leave_copy, false);
			}

			protected class InternalWrap : Expression
			{
				public Expression expr;
				public NullableInfo info;

				public InternalWrap (Expression expr, NullableInfo info, Location loc)
				{
					this.expr = expr;
					this.info = info;
					this.loc = loc;

					type = info.Type;
					eclass = ExprClass.Value;
				}

				public override Expression DoResolve (EmitContext ec)
				{
					return this;
				}

				public override void Emit (EmitContext ec)
				{
					expr.Emit (ec);
					ec.ig.Emit (OpCodes.Newobj, info.Constructor);
				}
			}
		}

		public class Wrap : Expression
		{
			Expression expr;
			NullableInfo info;

			protected Wrap (Expression expr)
			{
				this.expr = expr;
				this.loc = expr.Location;
			}

			public static Wrap Create (Expression expr, EmitContext ec)
			{
				return new Wrap (expr).Resolve (ec) as Wrap;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				expr = expr.Resolve (ec);
				if (expr == null)
					return null;

				TypeExpr target_type = new NullableType (expr.Type, loc);
				target_type = target_type.ResolveAsTypeTerminal (ec, false);
				if (target_type == null)
					return null;

				type = target_type.Type;
				info = new NullableInfo (type);
				eclass = ExprClass.Value;
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				expr.Emit (ec);
				ec.ig.Emit (OpCodes.Newobj, info.Constructor);
			}
		}

		public class NullableLiteral : NullLiteral, IMemoryLocation {
			public NullableLiteral (Type target_type, Location loc)
				: base (loc)
			{
				this.type = target_type;

				eclass = ExprClass.Value;
			}
		
			public override Expression DoResolve (EmitContext ec)
			{
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				LocalTemporary value_target = new LocalTemporary (type);

				value_target.AddressOf (ec, AddressOp.Store);
				ec.ig.Emit (OpCodes.Initobj, type);
				value_target.Emit (ec);
			}

			public void AddressOf (EmitContext ec, AddressOp Mode)
			{
				LocalTemporary value_target = new LocalTemporary (type);
					
				value_target.AddressOf (ec, AddressOp.Store);
				ec.ig.Emit (OpCodes.Initobj, type);
				((IMemoryLocation) value_target).AddressOf (ec, Mode);
			}
		}

		public abstract class Lifted : Expression, IMemoryLocation
		{
			Expression expr, underlying, wrap, null_value;
			Unwrap unwrap;

			protected Lifted (Expression expr, Location loc)
			{
				this.expr = expr;
				this.loc = loc;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				expr = expr.Resolve (ec);
				if (expr == null)
					return null;

				unwrap = Unwrap.Create (expr, ec);
				if (unwrap == null)
					return null;

				underlying = ResolveUnderlying (unwrap, ec);
				if (underlying == null)
					return null;

				wrap = Wrap.Create (underlying, ec);
				if (wrap == null)
					return null;

				null_value = new NullableLiteral (wrap.Type, loc).Resolve (ec);
				if (null_value == null)
					return null;

				type = wrap.Type;
				eclass = ExprClass.Value;
				return this;
			}

			protected abstract Expression ResolveUnderlying (Expression unwrap, EmitContext ec);

			public override void Emit (EmitContext ec)
			{
				ILGenerator ig = ec.ig;
				Label is_null_label = ig.DefineLabel ();
				Label end_label = ig.DefineLabel ();

				unwrap.EmitCheck (ec);
				ig.Emit (OpCodes.Brfalse, is_null_label);

				wrap.Emit (ec);
				ig.Emit (OpCodes.Br, end_label);

				ig.MarkLabel (is_null_label);
				null_value.Emit (ec);

				ig.MarkLabel (end_label);
			}

			public void AddressOf (EmitContext ec, AddressOp mode)
			{
				unwrap.AddressOf (ec, mode);
			}
		}

		public class LiftedConversion : Lifted
		{
			public readonly bool IsUser;
			public readonly bool IsExplicit;
			public readonly Type TargetType;

			public LiftedConversion (Expression expr, Type target_type, bool is_user,
						 bool is_explicit, Location loc)
				: base (expr, loc)
			{
				this.IsUser = is_user;
				this.IsExplicit = is_explicit;
				this.TargetType = target_type;
			}

			protected override Expression ResolveUnderlying (Expression unwrap, EmitContext ec)
			{
				Type type = TypeManager.GetTypeArguments (TargetType) [0];

				if (IsUser) {
					return Convert.UserDefinedConversion (ec, unwrap, type, loc, IsExplicit);
				} else {
					if (IsExplicit)
						return Convert.ExplicitConversion (ec, unwrap, type, loc);
					else
						return Convert.ImplicitConversion (ec, unwrap, type, loc);
				}
			}
		}

		public class LiftedUnaryOperator : Lifted
		{
			public readonly Unary.Operator Oper;

			public LiftedUnaryOperator (Unary.Operator op, Expression expr, Location loc)
				: base (expr, loc)
			{
				this.Oper = op;
			}

			protected override Expression ResolveUnderlying (Expression unwrap, EmitContext ec)
			{
				return new Unary (Oper, unwrap, loc);
			}
		}

		public class LiftedConditional : Lifted
		{
			Expression true_expr, false_expr;

			public LiftedConditional (Expression expr, Expression true_expr, Expression false_expr,
						  Location loc)
				: base (expr, loc)
			{
				this.true_expr = true_expr;
				this.false_expr = false_expr;
			}

			protected override Expression ResolveUnderlying (Expression unwrap, EmitContext ec)
			{
				return new Conditional (unwrap, true_expr, false_expr);
			}
		}

		public class LiftedBinaryOperator : Expression
		{
			public readonly Binary.Operator Oper;

			Expression left, right, original_left, original_right;
			Expression underlying, null_value, bool_wrap;
			Unwrap left_unwrap, right_unwrap;
			bool is_equality, is_comparision, is_boolean;

			public LiftedBinaryOperator (Binary.Operator op, Expression left, Expression right,
						     Location loc)
			{
				this.Oper = op;
				this.left = original_left = left;
				this.right = original_right = right;
				this.loc = loc;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				if (TypeManager.IsNullableType (left.Type)) {
					left = left_unwrap = Unwrap.Create (left, ec);
					if (left == null)
						return null;
				}

				if (TypeManager.IsNullableType (right.Type)) {
					right = right_unwrap = Unwrap.Create (right, ec);
					if (right == null)
						return null;
				}

				if ((Oper == Binary.Operator.LogicalAnd) ||
				    (Oper == Binary.Operator.LogicalOr)) {
					Binary.Error_OperatorCannotBeApplied (
						loc, Binary.OperName (Oper),
						original_left.GetSignatureForError (),
						original_right.GetSignatureForError ());
					return null;
				}

				if (((Oper == Binary.Operator.BitwiseAnd) || (Oper == Binary.Operator.BitwiseOr)) &&
				    ((left.Type == TypeManager.bool_type) && (right.Type == TypeManager.bool_type))) {
					Expression empty = new EmptyExpression (TypeManager.bool_type);
					bool_wrap = Wrap.Create (empty, ec);
					null_value = new NullableLiteral (bool_wrap.Type, loc).Resolve (ec);

					type = bool_wrap.Type;
					is_boolean = true;
				} else if ((Oper == Binary.Operator.Equality) || (Oper == Binary.Operator.Inequality)) {
					if (!(left is NullLiteral) && !(right is NullLiteral)) {
						underlying = new Binary (Oper, left, right).Resolve (ec);
						if (underlying == null)
							return null;
					}

					type = TypeManager.bool_type;
					is_equality = true;
				} else if ((Oper == Binary.Operator.LessThan) ||
					   (Oper == Binary.Operator.GreaterThan) ||
					   (Oper == Binary.Operator.LessThanOrEqual) ||
					   (Oper == Binary.Operator.GreaterThanOrEqual)) {
					underlying = new Binary (Oper, left, right).Resolve (ec);
					if (underlying == null)
						return null;

					type = TypeManager.bool_type;
					is_comparision = true;
				} else {
					underlying = new Binary (Oper, left, right).Resolve (ec);
					if (underlying == null)
						return null;

					underlying = Wrap.Create (underlying, ec);
					if (underlying == null)
						return null;

					type = underlying.Type;
					null_value = new NullableLiteral (type, loc).Resolve (ec);
				}

				eclass = ExprClass.Value;
				return this;
			}

			void EmitBoolean (EmitContext ec)
			{
				ILGenerator ig = ec.ig;

				Label left_is_null_label = ig.DefineLabel ();
				Label right_is_null_label = ig.DefineLabel ();
				Label is_null_label = ig.DefineLabel ();
				Label wrap_label = ig.DefineLabel ();
				Label end_label = ig.DefineLabel ();

				if (left_unwrap != null) {
					left_unwrap.EmitCheck (ec);
					ig.Emit (OpCodes.Brfalse, left_is_null_label);
				}

				left.Emit (ec);
				ig.Emit (OpCodes.Dup);
				if ((Oper == Binary.Operator.BitwiseOr) || (Oper == Binary.Operator.LogicalOr))
					ig.Emit (OpCodes.Brtrue, wrap_label);
				else
					ig.Emit (OpCodes.Brfalse, wrap_label);

				if (right_unwrap != null) {
					right_unwrap.EmitCheck (ec);
					ig.Emit (OpCodes.Brfalse, right_is_null_label);
				}

				if ((Oper == Binary.Operator.LogicalAnd) || (Oper == Binary.Operator.LogicalOr))
					ig.Emit (OpCodes.Pop);

				right.Emit (ec);
				if (Oper == Binary.Operator.BitwiseOr)
					ig.Emit (OpCodes.Or);
				else if (Oper == Binary.Operator.BitwiseAnd)
					ig.Emit (OpCodes.And);
				ig.Emit (OpCodes.Br, wrap_label);

				ig.MarkLabel (left_is_null_label);
				if (right_unwrap != null) {
					right_unwrap.EmitCheck (ec);
					ig.Emit (OpCodes.Brfalse, is_null_label);
				}

				right.Emit (ec);
				ig.Emit (OpCodes.Dup);
				if ((Oper == Binary.Operator.BitwiseOr) || (Oper == Binary.Operator.LogicalOr))
					ig.Emit (OpCodes.Brtrue, wrap_label);
				else
					ig.Emit (OpCodes.Brfalse, wrap_label);

				ig.MarkLabel (right_is_null_label);
				ig.Emit (OpCodes.Pop);
				ig.MarkLabel (is_null_label);
				null_value.Emit (ec);
				ig.Emit (OpCodes.Br, end_label);

				ig.MarkLabel (wrap_label);
				ig.Emit (OpCodes.Nop);
				bool_wrap.Emit (ec);
				ig.Emit (OpCodes.Nop);

				ig.MarkLabel (end_label);
			}

			void EmitEquality (EmitContext ec)
			{
				ILGenerator ig = ec.ig;

				// Given 'X? x;' for any value type X: 'x != null' is the same as 'x.HasValue'
				if (left is NullLiteral) {
					if (right_unwrap == null)
						throw new InternalErrorException ();
					right_unwrap.EmitCheck (ec);
					if (Oper == Binary.Operator.Equality) {
						ig.Emit (OpCodes.Ldc_I4_0);
						ig.Emit (OpCodes.Ceq);
					}
					return;
				}

				if (right is NullLiteral) {
					if (left_unwrap == null)
						throw new InternalErrorException ();
					left_unwrap.EmitCheck (ec);
					if (Oper == Binary.Operator.Equality) {
						ig.Emit (OpCodes.Ldc_I4_0);
						ig.Emit (OpCodes.Ceq);
					}
					return;
				}

				Label both_have_value_label = ig.DefineLabel ();
				Label end_label = ig.DefineLabel ();

				if (left_unwrap != null && right_unwrap != null) {
					Label dissimilar_label = ig.DefineLabel ();

					left_unwrap.EmitCheck (ec);
					ig.Emit (OpCodes.Dup);
					right_unwrap.EmitCheck (ec);
					ig.Emit (OpCodes.Bne_Un, dissimilar_label);

					ig.Emit (OpCodes.Brtrue, both_have_value_label);

					// both are null
					if (Oper == Binary.Operator.Equality)
						ig.Emit (OpCodes.Ldc_I4_1);
					else
						ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Br, end_label);

					ig.MarkLabel (dissimilar_label);
					ig.Emit (OpCodes.Pop);
				} else if (left_unwrap != null) {
					left_unwrap.EmitCheck (ec);
					ig.Emit (OpCodes.Brtrue, both_have_value_label);
				} else if (right_unwrap != null) {
					right_unwrap.EmitCheck (ec);
					ig.Emit (OpCodes.Brtrue, both_have_value_label);
				} else {
					throw new InternalErrorException ("shouldn't get here");
				}

				// one is null while the other isn't
				if (Oper == Binary.Operator.Equality)
					ig.Emit (OpCodes.Ldc_I4_0);
				else
					ig.Emit (OpCodes.Ldc_I4_1);
				ig.Emit (OpCodes.Br, end_label);

				ig.MarkLabel (both_have_value_label);
				underlying.Emit (ec);

				ig.MarkLabel (end_label);
			}

			void EmitComparision (EmitContext ec)
			{
				ILGenerator ig = ec.ig;

				Label is_null_label = ig.DefineLabel ();
				Label end_label = ig.DefineLabel ();

				if (left_unwrap != null) {
					left_unwrap.EmitCheck (ec);
					ig.Emit (OpCodes.Brfalse, is_null_label);
				}

				if (right_unwrap != null) {
					right_unwrap.EmitCheck (ec);
					ig.Emit (OpCodes.Brfalse, is_null_label);
				}

				underlying.Emit (ec);
				ig.Emit (OpCodes.Br, end_label);

				ig.MarkLabel (is_null_label);
				ig.Emit (OpCodes.Ldc_I4_0);

				ig.MarkLabel (end_label);
			}

			public override void Emit (EmitContext ec)
			{
				if (left_unwrap != null)
					left_unwrap.Store (ec);
				if (right_unwrap != null)
					right_unwrap.Store (ec);

				if (is_boolean) {
					EmitBoolean (ec);
					return;
				} else if (is_equality) {
					EmitEquality (ec);
					return;
				} else if (is_comparision) {
					EmitComparision (ec);
					return;
				}

				ILGenerator ig = ec.ig;

				Label is_null_label = ig.DefineLabel ();
				Label end_label = ig.DefineLabel ();

				if (left_unwrap != null) {
					left_unwrap.EmitCheck (ec);
					ig.Emit (OpCodes.Brfalse, is_null_label);
				}

				if (right_unwrap != null) {
					right_unwrap.EmitCheck (ec);
					ig.Emit (OpCodes.Brfalse, is_null_label);
				}

				underlying.Emit (ec);
				ig.Emit (OpCodes.Br, end_label);

				ig.MarkLabel (is_null_label);
				null_value.Emit (ec);

				ig.MarkLabel (end_label);
			}
		}

		public class OperatorTrueOrFalse : Expression
		{
			public readonly bool IsTrue;

			Expression expr;
			Unwrap unwrap;

			public OperatorTrueOrFalse (Expression expr, bool is_true, Location loc)
			{
				this.IsTrue = is_true;
				this.expr = expr;
				this.loc = loc;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				unwrap = Unwrap.Create (expr, ec);
				if (unwrap == null)
					return null;

				if (unwrap.Type != TypeManager.bool_type)
					return null;

				type = TypeManager.bool_type;
				eclass = ExprClass.Value;
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				ILGenerator ig = ec.ig;

				Label is_null_label = ig.DefineLabel ();
				Label end_label = ig.DefineLabel ();

				unwrap.EmitCheck (ec);
				ig.Emit (OpCodes.Brfalse, is_null_label);

				unwrap.Emit (ec);
				if (!IsTrue) {
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Ceq);
				}
				ig.Emit (OpCodes.Br, end_label);

				ig.MarkLabel (is_null_label);
				ig.Emit (OpCodes.Ldc_I4_0);

				ig.MarkLabel (end_label);
			}
		}

		public class NullCoalescingOperator : Expression
		{
			Expression left, right;
			Expression expr;
			Unwrap unwrap;

			public NullCoalescingOperator (Expression left, Expression right, Location loc)
			{
				this.left = left;
				this.right = right;
				this.loc = loc;

				eclass = ExprClass.Value;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				if (type != null)
					return this;

				left = left.Resolve (ec);
				if (left == null)
					return null;

				right = right.Resolve (ec);
				if (right == null)
					return null;

				Type ltype = left.Type, rtype = right.Type;

				if (!TypeManager.IsNullableType (ltype) && ltype.IsValueType) {
					Binary.Error_OperatorCannotBeApplied (loc, "??", ltype, rtype);
					return null;
				}

				if (TypeManager.IsNullableType (ltype)) {
					NullableInfo info = new NullableInfo (ltype);

					unwrap = Unwrap.Create (left, ec);
					if (unwrap == null)
						return null;

					expr = Convert.ImplicitConversion (ec, right, info.UnderlyingType, loc);
					if (expr != null) {
						left = unwrap;
						type = expr.Type;
						return this;
					}
				}

				expr = Convert.ImplicitConversion (ec, right, ltype, loc);
				if (expr != null) {
					type = expr.Type;
					return this;
				}

				Expression left_null = unwrap != null ? unwrap : left;
				expr = Convert.ImplicitConversion (ec, left_null, rtype, loc);
				if (expr != null) {
					left = expr;
					expr = right;
					type = rtype;
					return this;
				}

				Binary.Error_OperatorCannotBeApplied (loc, "??", ltype, rtype);
				return null;
			}

			public override void Emit (EmitContext ec)
			{
				ILGenerator ig = ec.ig;

				Label is_null_label = ig.DefineLabel ();
				Label end_label = ig.DefineLabel ();

				if (unwrap != null) {
					unwrap.EmitCheck (ec);
					ig.Emit (OpCodes.Brfalse, is_null_label);

					left.Emit (ec);
					ig.Emit (OpCodes.Br, end_label);

					ig.MarkLabel (is_null_label);
					expr.Emit (ec);

					ig.MarkLabel (end_label);
				} else {
					left.Emit (ec);
					ig.Emit (OpCodes.Dup);
					ig.Emit (OpCodes.Brtrue, end_label);

					ig.MarkLabel (is_null_label);

					ig.Emit (OpCodes.Pop);
					expr.Emit (ec);

					ig.MarkLabel (end_label);
				}
			}
		}

		public class LiftedUnaryMutator : ExpressionStatement
		{
			public readonly UnaryMutator.Mode Mode;
			Expression expr, null_value;
			UnaryMutator underlying;
			Unwrap unwrap;

			public LiftedUnaryMutator (UnaryMutator.Mode mode, Expression expr, Location loc)
			{
				this.expr = expr;
				this.Mode = mode;
				this.loc = loc;

				eclass = ExprClass.Value;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				expr = expr.Resolve (ec);
				if (expr == null)
					return null;

				unwrap = Unwrap.Create (expr, ec);
				if (unwrap == null)
					return null;

				underlying = (UnaryMutator) new UnaryMutator (Mode, unwrap, loc).Resolve (ec);
				if (underlying == null)
					return null;

				null_value = new NullableLiteral (expr.Type, loc).Resolve (ec);
				if (null_value == null)
					return null;

				type = expr.Type;
				return this;
			}

			void DoEmit (EmitContext ec, bool is_expr)
			{
				ILGenerator ig = ec.ig;
				Label is_null_label = ig.DefineLabel ();
				Label end_label = ig.DefineLabel ();

				unwrap.EmitCheck (ec);
				ig.Emit (OpCodes.Brfalse, is_null_label);

				if (is_expr)
					underlying.Emit (ec);
				else
					underlying.EmitStatement (ec);
				ig.Emit (OpCodes.Br, end_label);

				ig.MarkLabel (is_null_label);
				if (is_expr)
					null_value.Emit (ec);

				ig.MarkLabel (end_label);
			}

			public override void Emit (EmitContext ec)
			{
				DoEmit (ec, true);
			}

			public override void EmitStatement (EmitContext ec)
			{
				DoEmit (ec, false);
			}
		}
	}
}
