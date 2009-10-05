//
// generic.cs: Generics support
//
// Authors: Martin Baulig (martin@ximian.com)
//          Miguel de Icaza (miguel@ximian.com)
//          Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004-2008 Novell, Inc
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
					if (!TypeManager.IsValueType (ClassConstraint))
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

	public class ReflectionConstraints : GenericConstraints
	{
		GenericParameterAttributes attrs;
		Type base_type;
		Type class_constraint;
		Type[] iface_constraints;
		string name;

		public static GenericConstraints GetConstraints (Type t)
		{
			Type[] constraints = t.GetGenericParameterConstraints ();
			GenericParameterAttributes attrs = t.GenericParameterAttributes;
			if (constraints.Length == 0 && attrs == GenericParameterAttributes.None)
				return null;
			return new ReflectionConstraints (t.Name, constraints, attrs);
		}

		private ReflectionConstraints (string name, Type[] constraints, GenericParameterAttributes attrs)
		{
			this.name = name;
			this.attrs = attrs;

			int interface_constraints_pos = 0;
			if ((attrs & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0) {
				base_type = TypeManager.value_type;
				interface_constraints_pos = 1;
			} else if ((attrs & GenericParameterAttributes.ReferenceTypeConstraint) != 0) {
				if (constraints.Length > 0 && constraints[0].IsClass) {
					class_constraint = base_type = constraints[0];
					interface_constraints_pos = 1;
				} else {
					base_type = TypeManager.object_type;
				}
			} else {
				base_type = TypeManager.object_type;
			}

			if (constraints.Length > interface_constraints_pos) {
				if (interface_constraints_pos == 0) {
					iface_constraints = constraints;
				} else {
					iface_constraints = new Type[constraints.Length - interface_constraints_pos];
					Array.Copy (constraints, interface_constraints_pos, iface_constraints, 0, iface_constraints.Length);
				}
			} else {
				iface_constraints = Type.EmptyTypes;
			}
		}

		public override string TypeParameter
		{
			get { return name; }
		}

		public override GenericParameterAttributes Attributes
		{
			get { return attrs; }
		}

		public override Type ClassConstraint
		{
			get { return class_constraint; }
		}

		public override Type EffectiveBaseClass
		{
			get { return base_type; }
		}

		public override Type[] InterfaceConstraints
		{
			get { return iface_constraints; }
		}
	}

	public enum Variance
	{
		//
		// Don't add or modify internal values, they are used as -/+ calculation signs
		//
		None			= 0,
		Covariant		= 1,
		Contravariant	= -1
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
		public bool Resolve (MemberCore ec, TypeParameter tp, Report Report)
		{
			if (resolved)
				return true;

			if (ec == null)
				return false;

			iface_constraints = new ArrayList (2);	// TODO: Too expensive allocation
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

					NamespaceEntry.Error_NamespaceNotFound (loc, ((Expression)obj).GetSignatureForError (), Report);
					return false;
				}

				TypeExpr expr;
				GenericTypeExpr cexpr = fn as GenericTypeExpr;
				if (cexpr != null) {
					expr = cexpr.ResolveAsBaseTerminal (ec, false);
				} else
					expr = ((Expression) obj).ResolveAsTypeTerminal (ec, false);

				if ((expr == null) || (expr.Type == null))
					return false;

				if (!ec.IsAccessibleAs (fn.Type)) {
					Report.SymbolRelatedToPreviousError (fn.Type);
					Report.Error (703, loc,
						"Inconsistent accessibility: constraint type `{0}' is less accessible than `{1}'",
						fn.GetSignatureForError (), ec.GetSignatureForError ());
					return false;
				}

				if (TypeManager.IsGenericParameter (expr.Type))
					type_param_constraints.Add (expr);
				else if (expr.IsInterface)
					iface_constraints.Add (expr);
				else if (class_constraint != null || iface_constraints.Count != 0) {
					Report.Error (406, loc,
						"The class type constraint `{0}' must be listed before any other constraints. Consider moving type constraint to the beginning of the constraint list",
						expr.GetSignatureForError ());
					return false;
				} else if (HasReferenceTypeConstraint || HasValueTypeConstraint) {
					Report.Error (450, loc, "`{0}': cannot specify both " +
						      "a constraint class and the `class' " +
						      "or `struct' constraint", expr.GetSignatureForError ());
					return false;
				} else
					class_constraint = expr;


				//
				// Checks whether each generic method parameter constraint type
				// is valid with respect to T
				//
				if (tp != null && tp.Type.DeclaringMethod != null) {
					TypeManager.CheckTypeVariance (expr.Type, Variance.Contravariant, ec as MemberCore);
				}

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

			foreach (TypeExpr expr in type_param_constraints) {
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

				if (TypeManager.IsDynamicType (class_constraint_type)) {
					Report.Error (1967, loc, "A constraint cannot be the dynamic type");
					return false;
				}
			}

			if (class_constraint_type != null)
				effective_base_type = class_constraint_type;
			else if (HasValueTypeConstraint)
				effective_base_type = TypeManager.value_type;
			else
				effective_base_type = TypeManager.object_type;

			if ((attrs & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
				attrs |= GenericParameterAttributes.DefaultConstructorConstraint;

			resolved = true;
			return true;
		}

		bool CheckTypeParameterConstraints (Type tparam, ref TypeExpr prevConstraint, ArrayList seen, Report Report)
		{
			seen.Add (tparam);

			Constraints constraints = TypeManager.LookupTypeParameter (tparam).Constraints;
			if (constraints == null)
				return true;

			if (constraints.HasValueTypeConstraint) {
				Report.Error (456, loc,
					"Type parameter `{0}' has the `struct' constraint, so it cannot be used as a constraint for `{1}'",
					tparam.Name, name);
				return false;
			}

			//
			//  Checks whether there are no conflicts between type parameter constraints
			//
			//   class Foo<T, U>
			//      where T : A
			//      where U : A, B	// A and B are not convertible
			//
			if (constraints.HasClassConstraint) {
				if (prevConstraint != null) {
					Type t2 = constraints.ClassConstraint;
					TypeExpr e2 = constraints.class_constraint;

					if (!Convert.ImplicitReferenceConversionExists (prevConstraint, t2) &&
						!Convert.ImplicitReferenceConversionExists (e2, prevConstraint.Type)) {
						Report.Error (455, loc,
							"Type parameter `{0}' inherits conflicting constraints `{1}' and `{2}'",
							name, TypeManager.CSharpName (prevConstraint.Type), TypeManager.CSharpName (t2));
						return false;
					}
				}

				prevConstraint = constraints.class_constraint;
			}

			if (constraints.type_param_constraints == null)
				return true;

			foreach (TypeExpr expr in constraints.type_param_constraints) {
				if (seen.Contains (expr.Type)) {
					Report.Error (454, loc, "Circular constraint " +
						      "dependency involving `{0}' and `{1}'",
						      tparam.Name, expr.GetSignatureForError ());
					return false;
				}

				if (!CheckTypeParameterConstraints (expr.Type, ref prevConstraint, seen, Report))
					return false;
			}

			return true;
		}

		/// <summary>
		///   Resolve the constraints into actual types.
		/// </summary>
		public bool ResolveTypes (IMemberContext ec, Report r)
		{
			if (resolved_types)
				return true;

			resolved_types = true;

			foreach (object obj in constraints) {
				GenericTypeExpr cexpr = obj as GenericTypeExpr;
				if (cexpr == null)
					continue;

				if (!cexpr.CheckConstraints (ec))
					return false;
			}

			if (type_param_constraints.Count != 0) {
				ArrayList seen = new ArrayList ();
				TypeExpr prev_constraint = class_constraint;
				foreach (TypeExpr expr in type_param_constraints) {
					if (!CheckTypeParameterConstraints (expr.Type, ref prev_constraint, seen, r))
						return false;
					seen.Clear ();
				}
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
		public bool AreEqual (GenericConstraints gc)
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
				for (int ii = 0; ii < InterfaceConstraints.Length; ii++) {
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

		public void VerifyClsCompliance (Report r)
		{
			if (class_constraint_type != null && !AttributeTester.IsClsCompliant (class_constraint_type))
				Warning_ConstrainIsNotClsCompliant (class_constraint_type, class_constraint.Location, r);

			if (iface_constraint_types != null) {
				for (int i = 0; i < iface_constraint_types.Length; ++i) {
					if (!AttributeTester.IsClsCompliant (iface_constraint_types [i]))
						Warning_ConstrainIsNotClsCompliant (iface_constraint_types [i],
							((TypeExpr)iface_constraints [i]).Location, r);
				}
			}
		}

		void Warning_ConstrainIsNotClsCompliant (Type t, Location loc, Report Report)
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
		
		DeclSpace decl;
		GenericConstraints gc;
		Constraints constraints;
		GenericTypeParameterBuilder type;
		MemberCache member_cache;
		Variance variance;

		public TypeParameter (DeclSpace parent, DeclSpace decl, string name,
				      Constraints constraints, Attributes attrs, Variance variance, Location loc)
			: base (parent, new MemberName (name, loc), attrs)
		{
			this.decl = decl;
			this.constraints = constraints;
			this.variance = variance;
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

		public Variance Variance {
			get { return variance; }
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

		public void ErrorInvalidVariance (IMemberContext mc, Variance expected)
		{
// TODO:	Report.SymbolRelatedToPreviousError (mc);
			string input_variance = Variance == Variance.Contravariant ? "contravariant" : "covariant";
			string gtype_variance;
			switch (expected) {
			case Variance.Contravariant: gtype_variance = "contravariantly"; break;
			case Variance.Covariant: gtype_variance = "covariantly"; break;
			default: gtype_variance = "invariantly"; break;
			}

			Delegate d = mc as Delegate;
			string parameters = d != null ? d.Parameters.GetSignatureForError () : "";

			Report.Error (1961, Location,
				"The {2} type parameter `{0}' must be {3} valid on `{1}{4}'",
					GetSignatureForError (), mc.GetSignatureForError (), input_variance, gtype_variance, parameters);
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
				if (!constraints.Resolve (ds, this, Report)) {
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
		public bool ResolveType (IMemberContext ec)
		{
			if (constraints != null) {
				if (!constraints.ResolveTypes (ec, Report)) {
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
		public bool DefineType (IMemberContext ec)
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
		public bool DefineType (IMemberContext ec, MethodBuilder builder,
					MethodInfo implementing, bool is_override)
		{
			if (!ResolveType (ec))
				return false;

			if (implementing != null) {
				if (is_override && (constraints != null)) {
					Report.Error (460, Location,
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
					else if (!constraints.AreEqual (gc))
						ok = false;
				} else {
					if (!is_override && (temp_gc != null))
						ok = false;
				}

				if (!ok) {
					Report.SymbolRelatedToPreviousError (implementing);

					Report.Error (
						425, Location, "The constraints for type " +
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

			SetConstraints (type);
			return true;
		}

		public static TypeParameter FindTypeParameter (TypeParameter[] tparams, string name)
		{
			foreach (var tp in tparams) {
				if (tp.Name == name)
					return tp;
			}

			return null;
		}

		public void SetConstraints (GenericTypeParameterBuilder type)
		{
			GenericParameterAttributes attr = GenericParameterAttributes.None;
			if (variance == Variance.Contravariant)
				attr |= GenericParameterAttributes.Contravariant;
			else if (variance == Variance.Covariant)
				attr |= GenericParameterAttributes.Covariant;

			if (gc != null) {
				if (gc.HasClassConstraint || gc.HasValueTypeConstraint)
					type.SetBaseTypeConstraint (gc.EffectiveBaseClass);

				attr |= gc.Attributes;
				type.SetInterfaceConstraints (gc.InterfaceConstraints);
				TypeManager.RegisterBuilder (type, gc.InterfaceConstraints);
			}
			
			type.SetGenericParameterAttributes (attr);
		}

		/// <summary>
		///   This is called for each part of a partial generic type definition.
		///
		///   If `new_constraints' is not null and we don't already have constraints,
		///   they become our constraints.  If we already have constraints, we must
		///   check that they're the same.
		///   con
		/// </summary>
		public bool UpdateConstraints (MemberCore ec, Constraints new_constraints)
		{
			if (type == null)
				throw new InvalidOperationException ();

			if (new_constraints == null)
				return true;

			if (!new_constraints.Resolve (ec, this, Report))
				return false;
			if (!new_constraints.ResolveTypes (ec, Report))
				return false;

			if (constraints != null) 
				return constraints.AreEqual (new_constraints);

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

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
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
					return t.GenericParameterPosition < dargs.Length ? dargs [t.GenericParameterPosition] : t;
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
		
		public TypeParameterExpr (TypeParameter type_parameter, Location loc)
		{
			this.type = type_parameter.Type;
			this.eclass = ExprClass.TypeParameter;
			this.loc = loc;
		}

		protected override TypeExpr DoResolveAsTypeStep (IMemberContext ec)
		{
			throw new NotSupportedException ();
		}

		public override FullNamedExpression ResolveAsTypeStep (IMemberContext ec, bool silent)
		{
			return this;
		}

		public override bool IsInterface {
			get { return false; }
		}

		public override bool CheckAccessLevel (IMemberContext ds)
		{
			return true;
		}
	}

	//
	// Tracks the type arguments when instantiating a generic type. It's used
	// by both type arguments and type parameters
	//
	public class TypeArguments {
		ArrayList args;
		Type[] atypes;
		
		public TypeArguments ()
		{
			args = new ArrayList ();
		}

		public TypeArguments (params FullNamedExpression[] types)
		{
			this.args = new ArrayList (types);
		}

		public void Add (FullNamedExpression type)
		{
			args.Add (type);
		}

		public void Add (TypeArguments new_args)
		{
			args.AddRange (new_args.args);
		}

		// TODO: Should be deleted
		public TypeParameterName[] GetDeclarations ()
		{
			return (TypeParameterName[]) args.ToArray (typeof (TypeParameterName));
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

		public int Count {
			get {
				return args.Count;
			}
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
		public bool Resolve (IMemberContext ec)
		{
			if (atypes != null)
				return atypes.Length != 0;

			int count = args.Count;
			bool ok = true;

			atypes = new Type [count];

			for (int i = 0; i < count; i++){
				TypeExpr te = ((FullNamedExpression) args[i]).ResolveAsTypeTerminal (ec, false);
				if (te == null) {
					ok = false;
					continue;
				}

				atypes[i] = te.Type;

				if (te.Type.IsSealed && te.Type.IsAbstract) {
					ec.Compiler.Report.Error (718, te.Location, "`{0}': static classes cannot be used as generic arguments",
						te.GetSignatureForError ());
					ok = false;
				}

				if (te.Type.IsPointer || TypeManager.IsSpecialType (te.Type)) {
					ec.Compiler.Report.Error (306, te.Location,
						"The type `{0}' may not be used as a type argument",
						te.GetSignatureForError ());
					ok = false;
				}
			}

			if (!ok)
				atypes = Type.EmptyTypes;

			return ok;
		}

		public TypeArguments Clone ()
		{
			TypeArguments copy = new TypeArguments ();
			foreach (Expression ta in args)
				copy.args.Add (ta);

			return copy;
		}
	}

	public class TypeParameterName : SimpleName
	{
		Attributes attributes;
		Variance variance;

		public TypeParameterName (string name, Attributes attrs, Location loc)
			: this (name, attrs, Variance.None, loc)
		{
		}

		public TypeParameterName (string name, Attributes attrs, Variance variance, Location loc)
			: base (name, loc)
		{
			attributes = attrs;
			this.variance = variance;
		}

		public Attributes OptAttributes {
			get {
				return attributes;
			}
		}

		public Variance Variance {
			get {
				return variance;
			}
		}
	}

	/// <summary>
	///   A reference expression to generic type
	/// </summary>	
	class GenericTypeExpr : TypeExpr
	{
		TypeArguments args;
		Type[] gen_params;	// TODO: Waiting for constrains check cleanup
		Type open_type;

		//
		// Should be carefully used only with defined generic containers. Type parameters
		// can be used as type arguments in this case.
		//
		// TODO: This could be GenericTypeExpr specialization
		//
		public GenericTypeExpr (DeclSpace gType, Location l)
		{
			open_type = gType.TypeBuilder.GetGenericTypeDefinition ();

			args = new TypeArguments ();
			foreach (TypeParameter type_param in gType.TypeParameters)
				args.Add (new TypeParameterExpr (type_param, l));

			this.loc = l;
		}

		/// <summary>
		///   Instantiate the generic type `t' with the type arguments `args'.
		///   Use this constructor if you already know the fully resolved
		///   generic type.
		/// </summary>		
		public GenericTypeExpr (Type t, TypeArguments args, Location l)
		{
			open_type = t.GetGenericTypeDefinition ();

			loc = l;
			this.args = args;
		}

		public TypeArguments TypeArguments {
			get { return args; }
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpName (type);
		}

		protected override TypeExpr DoResolveAsTypeStep (IMemberContext ec)
		{
			if (eclass != ExprClass.Invalid)
				return this;

			eclass = ExprClass.Type;

			if (!args.Resolve (ec))
				return null;

			gen_params = open_type.GetGenericArguments ();
			Type[] atypes = args.Arguments;
			
			if (atypes.Length != gen_params.Length) {
				Namespace.Error_InvalidNumberOfTypeArguments (open_type, loc);
				return null;
			}

			//
			// Now bind the parameters
			//
			type = open_type.MakeGenericType (atypes);
			return this;
		}

		/// <summary>
		///   Check the constraints; we're called from ResolveAsTypeTerminal()
		///   after fully resolving the constructed type.
		/// </summary>
		public bool CheckConstraints (IMemberContext ec)
		{
			return ConstraintChecker.CheckConstraints (ec, open_type, gen_params, args.Arguments, loc);
		}
	
		public override bool CheckAccessLevel (IMemberContext mc)
		{
			return mc.CurrentTypeDefinition.CheckAccessLevel (open_type);
		}

		public override bool IsClass {
			get { return open_type.IsClass; }
		}

		public override bool IsValueType {
			get { return TypeManager.IsStruct (open_type); }
		}

		public override bool IsInterface {
			get { return open_type.IsInterface; }
		}

		public override bool IsSealed {
			get { return open_type.IsSealed; }
		}

		public override bool Equals (object obj)
		{
			GenericTypeExpr cobj = obj as GenericTypeExpr;
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
	}

	public abstract class ConstraintChecker
	{
		protected readonly Type[] gen_params;
		protected readonly Type[] atypes;
		protected readonly Location loc;
		protected Report Report;

		protected ConstraintChecker (Type[] gen_params, Type[] atypes, Location loc, Report r)
		{
			this.gen_params = gen_params;
			this.atypes = atypes;
			this.loc = loc;
			this.Report = r;
		}

		/// <summary>
		///   Check the constraints; we're called from ResolveAsTypeTerminal()
		///   after fully resolving the constructed type.
		/// </summary>
		public bool CheckConstraints (IMemberContext ec)
		{
			for (int i = 0; i < gen_params.Length; i++) {
				if (!CheckConstraints (ec, i))
					return false;
			}

			return true;
		}

		protected bool CheckConstraints (IMemberContext ec, int index)
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
					if (agc is Constraints) {
						// FIXME: No constraints can be resolved here, we are in
						// completely wrong/different context. This path is hit
						// when resolving base type of unresolved generic type
						// with constraints. We are waiting with CheckConsttraints
						// after type-definition but not in this case
						if (!((Constraints) agc).Resolve (null, null, Report))
							return true;
					}
					is_class = agc.IsReferenceType;
					is_struct = agc.IsValueType;
				} else {
					is_class = is_struct = false;
				}
			} else {
				is_class = TypeManager.IsReferenceType (atype);
				is_struct = TypeManager.IsValueType (atype) && !TypeManager.IsNullableType (atype);
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

			if (TypeManager.IsValueType (atype))
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

		protected bool CheckConstraint (IMemberContext ec, Type ptype, Expression expr,
						Type ctype)
		{
			//
			// All this is needed because we don't have
			// real inflated type hierarchy
			//
			if (TypeManager.HasGenericArguments (ctype)) {
				Type[] types = TypeManager.GetTypeArguments (ctype);

				TypeArguments new_args = new TypeArguments ();

				for (int i = 0; i < types.Length; i++) {
					Type t = TypeManager.TypeToCoreType (types [i]);

					if (t.IsGenericParameter) {
						int pos = t.GenericParameterPosition;
						if (t.DeclaringMethod == null && this is MethodConstraintChecker) {
							Type parent = ((MethodConstraintChecker) this).declaring_type;
							t = parent.GetGenericArguments ()[pos];
						} else {
							t = atypes [pos];
						}
					}
					new_args.Add (new TypeExpression (t, loc));
				}

				TypeExpr ct = new GenericTypeExpr (ctype, new_args, loc);
				if (ct.ResolveAsTypeStep (ec, false) == null)
					return false;
				ctype = ct.Type;
			} else if (ctype.IsGenericParameter) {
				int pos = ctype.GenericParameterPosition;
				if (ctype.DeclaringMethod == null) {
					// FIXME: Implement
					return true;
				} else {				
					ctype = atypes [pos];
				}
			}

			if (Convert.ImplicitStandardConversionExists (expr, ctype))
				return true;

			Report_SymbolRelatedToPreviousError ();
			Report.SymbolRelatedToPreviousError (expr.Type);

			if (TypeManager.IsNullableType (expr.Type) && ctype.IsInterface) {
				Report.Error (313, loc,
					"The type `{0}' cannot be used as type parameter `{1}' in the generic type or method `{2}'. " +
					"The nullable type `{0}' never satisfies interface constraint of type `{3}'",
					TypeManager.CSharpName (expr.Type), TypeManager.CSharpName (ptype),
					GetSignatureForError (), TypeManager.CSharpName (ctype));
			} else {
				Report.Error (309, loc,
					"The type `{0}' must be convertible to `{1}' in order to " +
					"use it as parameter `{2}' in the generic type or method `{3}'",
					TypeManager.CSharpName (expr.Type), TypeManager.CSharpName (ctype),
					TypeManager.CSharpName (ptype), GetSignatureForError ());
			}
			return false;
		}

		static bool HasDefaultConstructor (Type atype)
		{
			TypeParameter tparam = TypeManager.LookupTypeParameter (atype);
			if (tparam != null) {
				if (tparam.GenericConstraints == null)
					return false;
						
				return tparam.GenericConstraints.HasConstructorConstraint || 
					tparam.GenericConstraints.HasValueTypeConstraint;
			}
		
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

			MemberInfo [] list = TypeManager.MemberLookup (null, null, atype, MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				ConstructorInfo.ConstructorName, null);

			if (list == null)
				return false;

			foreach (MethodBase mb in list) {
				AParametersCollection pd = TypeManager.GetParameterData (mb);
				if (pd.Count == 0)
					return true;
			}

			return false;
		}

		protected abstract string GetSignatureForError ();
		protected abstract void Report_SymbolRelatedToPreviousError ();

		public static bool CheckConstraints (IMemberContext ec, MethodBase definition,
						     MethodBase instantiated, Location loc)
		{
			MethodConstraintChecker checker = new MethodConstraintChecker (
				definition, instantiated.DeclaringType, definition.GetGenericArguments (),
				instantiated.GetGenericArguments (), loc, ec.Compiler.Report);

			return checker.CheckConstraints (ec);
		}

		public static bool CheckConstraints (IMemberContext ec, Type gt, Type[] gen_params,
						     Type[] atypes, Location loc)
		{
			TypeConstraintChecker checker = new TypeConstraintChecker (
				gt, gen_params, atypes, loc, ec.Compiler.Report);

			return checker.CheckConstraints (ec);
		}

		protected class MethodConstraintChecker : ConstraintChecker
		{
			MethodBase definition;
			public Type declaring_type;

			public MethodConstraintChecker (MethodBase definition, Type declaringType, Type[] gen_params,
							Type[] atypes, Location loc, Report r)
				: base (gen_params, atypes, loc, r)
			{
				this.declaring_type = declaringType;
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
						      Location loc, Report r)
				: base (gen_params, atypes, loc, r)
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
		FullNamedExpression return_type;
		ParametersCompiled parameters;

		public GenericMethod (NamespaceEntry ns, DeclSpace parent, MemberName name,
				      FullNamedExpression return_type, ParametersCompiled parameters)
			: base (ns, parent, name, null)
		{
			this.return_type = return_type;
			this.parameters = parameters;
		}

		public override TypeContainer CurrentTypeDefinition {
			get {
				return Parent.CurrentTypeDefinition;
			}
		}

		public override TypeParameter[] CurrentTypeParameters {
			get {
				return base.type_params;
			}
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
		public bool Define (MethodOrOperator m)
		{
			TypeParameterName[] names = MemberName.TypeArguments.GetDeclarations ();
			string[] snames = new string [names.Length];
			for (int i = 0; i < names.Length; i++) {
				string type_argument_name = names[i].Name;
				int idx = parameters.GetParameterIndexByName (type_argument_name);
				if (idx >= 0) {
					Block b = m.Block;
					if (b == null)
						b = new Block (null);

					b.Error_AlreadyDeclaredTypeParameter (Report, parameters [i].Location,
						type_argument_name, "method parameter");
				}
				
				snames[i] = type_argument_name;
			}

			GenericTypeParameterBuilder[] gen_params = m.MethodBuilder.DefineGenericParameters (snames);
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

		/// <summary>
		///   We're called from MethodData.Define() after creating the MethodBuilder.
		/// </summary>
		public bool DefineType (IMemberContext ec, MethodBuilder mb,
					MethodInfo implementing, bool is_override)
		{
			for (int i = 0; i < TypeParameters.Length; i++)
				if (!TypeParameters [i].DefineType (
					    ec, mb, implementing, is_override))
					return false;

			bool ok = parameters.Resolve (ec);

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

		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria)
		{
			throw new Exception ();
		}

		public override string GetSignatureForError ()
		{
			return base.GetSignatureForError () + parameters.GetSignatureForError ();
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

				tp.Constraints.VerifyClsCompliance (Report);
			}
		}
	}

	public partial class TypeManager
	{
		static public Type activator_type;
	
		public static TypeContainer LookupGenericTypeContainer (Type t)
		{
			t = DropGenericTypeArguments (t);
			return LookupTypeContainer (t);
		}

		public static Variance GetTypeParameterVariance (Type type)
		{
			TypeParameter tparam = LookupTypeParameter (type);
			if (tparam != null)
				return tparam.Variance;

			switch (type.GenericParameterAttributes & GenericParameterAttributes.VarianceMask) {
			case GenericParameterAttributes.Covariant:
				return Variance.Covariant;
			case GenericParameterAttributes.Contravariant:
				return Variance.Contravariant;
			default:
				return Variance.None;
			}
		}

		public static Variance CheckTypeVariance (Type t, Variance expected, IMemberContext member)
		{
			TypeParameter tp = LookupTypeParameter (t);
			if (tp != null) {
				Variance v = tp.Variance;
				if (expected == Variance.None && v != expected ||
					expected == Variance.Covariant && v == Variance.Contravariant ||
					expected == Variance.Contravariant && v == Variance.Covariant)
					tp.ErrorInvalidVariance (member, expected);

				return expected;
			}

			if (t.IsGenericType) {
				Type[] targs_definition = GetTypeArguments (DropGenericTypeArguments (t));
				Type[] targs = GetTypeArguments (t);
				for (int i = 0; i < targs_definition.Length; ++i) {
					Variance v = GetTypeParameterVariance (targs_definition[i]);
					CheckTypeVariance (targs[i], (Variance) ((int)v * (int)expected), member);
				}

				return expected;
			}

			if (t.IsArray)
				return CheckTypeVariance (GetElementType (t), expected, member);

			return Variance.None;
		}

		public static bool IsVariantOf (Type type1, Type type2)
		{
			if (!type1.IsGenericType || !type2.IsGenericType)
				return false;

			Type generic_target_type = DropGenericTypeArguments (type2);
			if (DropGenericTypeArguments (type1) != generic_target_type)
				return false;

			Type[] t1 = GetTypeArguments (type1);
			Type[] t2 = GetTypeArguments (type2);
			Type[] targs_definition = GetTypeArguments (generic_target_type);
			for (int i = 0; i < targs_definition.Length; ++i) {
				Variance v = GetTypeParameterVariance (targs_definition [i]);
				if (v == Variance.None) {
					if (t1[i] == t2[i])
						continue;
					return false;
				}

				if (v == Variance.Covariant) {
					if (!Convert.ImplicitReferenceConversionExists (new EmptyExpression (t1 [i]), t2 [i]))
						return false;
				} else if (!Convert.ImplicitReferenceConversionExists (new EmptyExpression (t2[i]), t1[i])) {
					return false;
				}
			}

			return true;
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
					b = GetElementType (b);
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
			
				a = GetElementType (a);
				b = GetElementType (b);

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
		public static int InferTypeArguments (ResolveContext ec, Arguments arguments, ref MethodBase method)
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

/*
		public static bool InferTypeArguments (ResolveContext ec, AParametersCollection param, ref MethodBase method)
		{
			if (!TypeManager.IsGenericMethod (method))
				return true;

			ATypeInference ti = ATypeInference.CreateInstance (DelegateCreation.CreateDelegateMethodArguments (param, Location.Null));
			Type[] i_args = ti.InferDelegateArguments (ec, method);
			if (i_args == null)
				return false;

			method = ((MethodInfo) method).MakeGenericMethod (i_args);
			return true;
		}
*/
	}

	abstract class ATypeInference
	{
		protected readonly Arguments arguments;
		protected readonly int arg_count;

		protected ATypeInference (Arguments arguments)
		{
			this.arguments = arguments;
			if (arguments != null)
				arg_count = arguments.Count;
		}

		public static ATypeInference CreateInstance (Arguments arguments)
		{
			return new TypeInference (arguments);
		}

		public virtual int InferenceScore {
			get {
				return int.MaxValue;
			}
		}

		public abstract Type[] InferMethodArguments (ResolveContext ec, MethodBase method);
//		public abstract Type[] InferDelegateArguments (ResolveContext ec, MethodBase method);
	}

	//
	// Implements C# type inference
	//
	class TypeInference : ATypeInference
	{
		//
		// Tracks successful rate of type inference
		//
		int score = int.MaxValue;

		public TypeInference (Arguments arguments)
			: base (arguments)
		{
		}

		public override int InferenceScore {
			get {
				return score;
			}
		}

/*
		public override Type[] InferDelegateArguments (ResolveContext ec, MethodBase method)
		{
			AParametersCollection pd = TypeManager.GetParameterData (method);
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

				context.LowerBoundInference (arguments [i].Expr.Type, t);
			}

			if (!context.FixAllTypes (ec))
				return null;

			return context.InferredTypeArguments;
		}
*/
		public override Type[] InferMethodArguments (ResolveContext ec, MethodBase method)
		{
			Type[] method_generic_args = method.GetGenericArguments ();
			TypeInferenceContext context = new TypeInferenceContext (method_generic_args);
			if (!context.UnfixedVariableExists)
				return Type.EmptyTypes;

			AParametersCollection pd = TypeManager.GetParameterData (method);
			if (!InferInPhases (ec, context, pd))
				return null;

			return context.InferredTypeArguments;
		}

		//
		// Implements method type arguments inference
		//
		bool InferInPhases (ResolveContext ec, TypeInferenceContext tic, AParametersCollection methodParameters)
		{
			int params_arguments_start;
			if (methodParameters.HasParams) {
				params_arguments_start = methodParameters.Count - 1;
			} else {
				params_arguments_start = arg_count;
			}

			Type [] ptypes = methodParameters.Types;
			
			//
			// The first inference phase
			//
			Type method_parameter = null;
			for (int i = 0; i < arg_count; i++) {
				Argument a = arguments [i];
				if (a == null)
					continue;
				
				if (i < params_arguments_start) {
					method_parameter = methodParameters.Types [i];
				} else if (i == params_arguments_start) {
					if (arg_count == params_arguments_start + 1 && TypeManager.HasElementType (a.Type))
						method_parameter = methodParameters.Types [params_arguments_start];
					else
						method_parameter = TypeManager.GetElementType (methodParameters.Types [params_arguments_start]);

					ptypes = (Type[]) ptypes.Clone ();
					ptypes [i] = method_parameter;
				}

				//
				// When a lambda expression, an anonymous method
				// is used an explicit argument type inference takes a place
				//
				AnonymousMethodExpression am = a.Expr as AnonymousMethodExpression;
				if (am != null) {
					if (am.ExplicitTypeInference (ec, tic, method_parameter))
						--score; 
					continue;
				}

				if (a.IsByRef) {
					score -= tic.ExactInference (a.Type, method_parameter);
					continue;
				}

				if (a.Expr.Type == TypeManager.null_type)
					continue;

				if (TypeManager.IsValueType (method_parameter)) {
					score -= tic.LowerBoundInference (a.Type, method_parameter);
					continue;
				}

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
			if (!tic.FixIndependentTypeArguments (ec, ptypes, ref fixed_any))
				return false;

			return DoSecondPhase (ec, tic, ptypes, !fixed_any);
		}

		bool DoSecondPhase (ResolveContext ec, TypeInferenceContext tic, Type[] methodParameters, bool fixDependent)
		{
			bool fixed_any = false;
			if (fixDependent && !tic.FixDependentTypes (ec, ref fixed_any))
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
				
				// Align params arguments
				Type t_i = methodParameters [i >= methodParameters.Length ? methodParameters.Length - 1: i];
				
				if (!TypeManager.IsDelegateType (t_i)) {
					if (TypeManager.DropGenericTypeArguments (t_i) != TypeManager.expression_type)
						continue;

					t_i = t_i.GetGenericArguments () [0];
				}

				MethodInfo mi = Delegate.GetInvokeMethod (ec.Compiler, t_i, t_i);
				Type rtype = mi.ReturnType;

#if MS_COMPATIBLE
				// Blablabla, because reflection does not work with dynamic types
				Type[] g_args = t_i.GetGenericArguments ();
				rtype = g_args[rtype.GenericParameterPosition];
#endif

				if (tic.IsReturnTypeNonDependent (ec, mi, rtype))
					score -= tic.OutputTypeInference (ec, arguments [i].Expr, t_i);
			}


			return DoSecondPhase (ec, tic, methodParameters, true);
		}
	}

	public class TypeInferenceContext
	{
		enum BoundKind
		{
			Exact	= 0,
			Lower	= 1,
			Upper	= 2
		}

		class BoundInfo
		{
			public readonly Type Type;
			public readonly BoundKind Kind;

			public BoundInfo (Type type, BoundKind kind)
			{
				this.Type = type;
				this.Kind = kind;
			}
			
			public override int GetHashCode ()
			{
				return Type.GetHashCode ();
			}

			public override bool Equals (object obj)
			{
				BoundInfo a = (BoundInfo) obj;
				return Type == a.Type && Kind == a.Kind;
			}
		}

		readonly Type[] unfixed_types;
		readonly Type[] fixed_types;
		readonly ArrayList[] bounds;
		bool failed;
		
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

		// 
		// Used together with AddCommonTypeBound fo implement
		// 7.4.2.13 Finding the best common type of a set of expressions
		//
		public TypeInferenceContext ()
		{
			fixed_types = new Type [1];
			unfixed_types = new Type [1];
			unfixed_types[0] = InternalType.Arglist; // it can be any internal type
			bounds = new ArrayList [1];
		}

		public Type[] InferredTypeArguments {
			get {
				return fixed_types;
			}
		}

		public void AddCommonTypeBound (Type type)
		{
			AddToBounds (new BoundInfo (type, BoundKind.Lower), 0);
		}

		void AddToBounds (BoundInfo bound, int index)
		{
			//
			// Some types cannot be used as type arguments
			//
			if (bound.Type == TypeManager.void_type || bound.Type.IsPointer)
				return;

			ArrayList a = bounds [index];
			if (a == null) {
				a = new ArrayList ();
				bounds [index] = a;
			} else {
				if (a.Contains (bound))
					return;
			}

			//
			// SPEC: does not cover type inference using constraints
			//
			//if (TypeManager.IsGenericParameter (t)) {
			//    GenericConstraints constraints = TypeManager.GetTypeParameterConstraints (t);
			//    if (constraints != null) {
			//        //if (constraints.EffectiveBaseClass != null)
			//        //	t = constraints.EffectiveBaseClass;
			//    }
			//}
			a.Add (bound);
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

			AddToBounds (new BoundInfo (u, BoundKind.Exact), pos);
			return 1;
		}

		public bool FixAllTypes (ResolveContext ec)
		{
			for (int i = 0; i < unfixed_types.Length; ++i) {
				if (!FixType (ec, i))
					return false;
			}
			return true;
		}

		//
		// All unfixed type variables Xi are fixed for which all of the following hold:
		// a, There is at least one type variable Xj that depends on Xi
		// b, Xi has a non-empty set of bounds
		// 
		public bool FixDependentTypes (ResolveContext ec, ref bool fixed_any)
		{
			for (int i = 0; i < unfixed_types.Length; ++i) {
				if (unfixed_types[i] == null)
					continue;

				if (bounds[i] == null)
					continue;

				if (!FixType (ec, i))
					return false;
				
				fixed_any = true;
			}

			return true;
		}

		//
		// All unfixed type variables Xi which depend on no Xj are fixed
		//
		public bool FixIndependentTypeArguments (ResolveContext ec, Type[] methodParameters, ref bool fixed_any)
		{
			ArrayList types_to_fix = new ArrayList (unfixed_types);
			for (int i = 0; i < methodParameters.Length; ++i) {
				Type t = methodParameters[i];

				if (!TypeManager.IsDelegateType (t)) {
					if (TypeManager.DropGenericTypeArguments (t) != TypeManager.expression_type)
						continue;

					t = t.GetGenericArguments () [0];
				}

				if (t.IsGenericParameter)
					continue;

				MethodInfo invoke = Delegate.GetInvokeMethod (ec.Compiler, t, t);
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
				if (idx >= 0 && !FixType (ec, idx)) {
					return false;
				}
			}

			fixed_any = types_to_fix.Count > 0;
			return true;
		}

		//
		// 26.3.3.10 Fixing
		//
		public bool FixType (ResolveContext ec, int i)
		{
			// It's already fixed
			if (unfixed_types[i] == null)
				throw new InternalErrorException ("Type argument has been already fixed");

			if (failed)
				return false;

			ArrayList candidates = (ArrayList)bounds [i];
			if (candidates == null)
				return false;

			if (candidates.Count == 1) {
				unfixed_types[i] = null;
				Type t = ((BoundInfo) candidates[0]).Type;
				if (t == TypeManager.null_type)
					return false;

				fixed_types [i] = t;
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
				BoundInfo bound = (BoundInfo)candidates [ci];
				for (cii = 0; cii < candidates_count; ++cii) {
					if (cii == ci)
						continue;

					BoundInfo cbound = (BoundInfo) candidates[cii];
					
					// Same type parameters with different bounds
					if (cbound.Type == bound.Type) {
						if (bound.Kind != BoundKind.Exact)
							bound = cbound;

						continue;
					}

					if (bound.Kind == BoundKind.Exact || cbound.Kind == BoundKind.Exact) {
						if (cbound.Kind != BoundKind.Exact) {
							if (!Convert.ImplicitConversionExists (ec, new TypeExpression (cbound.Type, Location.Null), bound.Type)) {
								break;
							}

							continue;
						}
						
						if (bound.Kind != BoundKind.Exact) {
							if (!Convert.ImplicitConversionExists (ec, new TypeExpression (bound.Type, Location.Null), cbound.Type)) {
								break;
							}

							bound = cbound;
							continue;
						}
						
						break;
					}

					if (bound.Kind == BoundKind.Lower) {
						if (!Convert.ImplicitConversionExists (ec, new TypeExpression (cbound.Type, Location.Null), bound.Type)) {
							break;
						}
					} else {
						if (!Convert.ImplicitConversionExists (ec, new TypeExpression (bound.Type, Location.Null), cbound.Type)) {
							break;
						}
					}
				}

				if (cii != candidates_count)
					continue;

				if (best_candidate != null && best_candidate != bound.Type)
					return false;

				best_candidate = bound.Type;
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
			if (parameter.IsGenericParameter) {
				//
				// Inflate method generic argument (MVAR) only
				//
				if (parameter.DeclaringMethod == null)
					return parameter;

				return fixed_types [parameter.GenericParameterPosition];
			}

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
		public bool IsReturnTypeNonDependent (ResolveContext ec, MethodInfo invoke, Type returnType)
		{
			if (returnType.IsGenericParameter) {
				if (IsFixed (returnType))
				    return false;
			} else if (returnType.IsGenericType) {
				if (TypeManager.IsDelegateType (returnType)) {
					invoke = Delegate.GetInvokeMethod (ec.Compiler, returnType, returnType);
					return IsReturnTypeNonDependent (ec, invoke, invoke.ReturnType);
				}
					
				Type[] g_args = returnType.GetGenericArguments ();
				
				// At least one unfixed return type has to exist 
				if (AllTypesAreFixed (g_args))
					return false;
			} else {
				return false;
			}

			// All generic input arguments have to be fixed
			AParametersCollection d_parameters = TypeManager.GetParameterData (invoke);
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
			return LowerBoundInference (u, v, false);
		}

		//
		// Lower-bound (false) or Upper-bound (true) inference based on inversed argument
		//
		int LowerBoundInference (Type u, Type v, bool inversed)
		{
			// If V is one of the unfixed type arguments
			int pos = IsUnfixed (v);
			if (pos != -1) {
				AddToBounds (new BoundInfo (u, inversed ? BoundKind.Upper : BoundKind.Lower), pos);
				return 1;
			}			

			// If U is an array type
			if (u.IsArray) {
				int u_dim = u.GetArrayRank ();
				Type v_i;
				Type u_i = TypeManager.GetElementType (u);

				if (v.IsArray) {
					if (u_dim != v.GetArrayRank ())
						return 0;

					v_i = TypeManager.GetElementType (v);

					if (TypeManager.IsValueType (u_i))
						return ExactInference (u_i, v_i);

					return LowerBoundInference (u_i, v_i, inversed);
				}

				if (u_dim != 1)
					return 0;

				if (v.IsGenericType) {
					Type g_v = v.GetGenericTypeDefinition ();
					if ((g_v != TypeManager.generic_ilist_type) && (g_v != TypeManager.generic_icollection_type) &&
						(g_v != TypeManager.generic_ienumerable_type))
						return 0;

					v_i = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (v) [0]);
					if (TypeManager.IsValueType (u_i))
						return ExactInference (u_i, v_i);

					return LowerBoundInference (u_i, v_i);
				}
			} else if (v.IsGenericType && !v.IsGenericTypeDefinition) {
				//
				// if V is a constructed type C<V1..Vk> and there is a unique type C<U1..Uk>
				// such that U is identical to, inherits from (directly or indirectly),
				// or implements (directly or indirectly) C<U1..Uk>
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
				Type [] unique_candidate_targs = null;
				Type [] ga_v = v.GetGenericArguments ();			
				foreach (Type u_candidate in u_candidates) {
					if (!u_candidate.IsGenericType || u_candidate.IsGenericTypeDefinition)
						continue;

					if (TypeManager.DropGenericTypeArguments (u_candidate) != open_v)
						continue;

					//
					// The unique set of types U1..Uk means that if we have an interface I<T>,
					// class U : I<int>, I<long> then no type inference is made when inferring
					// type I<T> by applying type U because T could be int or long
					//
					if (unique_candidate_targs != null) {
						Type[] second_unique_candidate_targs = u_candidate.GetGenericArguments ();
						if (TypeManager.IsEqual (unique_candidate_targs, second_unique_candidate_targs)) {
							unique_candidate_targs = second_unique_candidate_targs;
							continue;
						}

						//
						// This should always cause type inference failure
						//
						failed = true;
						return 1;
					}

					unique_candidate_targs = u_candidate.GetGenericArguments ();
				}

				if (unique_candidate_targs != null) {
					Type[] ga_open_v = open_v.GetGenericArguments ();
					int score = 0;
					for (int i = 0; i < unique_candidate_targs.Length; ++i) {
						Variance variance = TypeManager.GetTypeParameterVariance (ga_open_v [i]);

						Type u_i = unique_candidate_targs [i];
						if (variance == Variance.None || TypeManager.IsValueType (u_i)) {
							if (ExactInference (u_i, ga_v [i]) == 0)
								++score;
						} else {
							bool upper_bound = (variance == Variance.Contravariant && !inversed) ||
								(variance == Variance.Covariant && inversed);

							if (LowerBoundInference (u_i, ga_v [i], upper_bound) == 0)
								++score;
						}
					}
					return score;
				}
			}

			return 0;
		}

		//
		// 26.3.3.6 Output Type Inference
		//
		public int OutputTypeInference (ResolveContext ec, Expression e, Type t)
		{
			// If e is a lambda or anonymous method with inferred return type
			AnonymousMethodExpression ame = e as AnonymousMethodExpression;
			if (ame != null) {
				Type rt = ame.InferReturnType (ec, this, t);
				MethodInfo invoke = Delegate.GetInvokeMethod (ec.Compiler, t, t);

				if (rt == null) {
					AParametersCollection pd = TypeManager.GetParameterData (invoke);
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

			//
			// if E is a method group and T is a delegate type or expression tree type
			// return type Tb with parameter types T1..Tk and return type Tb, and overload
			// resolution of E with the types T1..Tk yields a single method with return type U,
			// then a lower-bound inference is made from U for Tb.
			//
			if (e is MethodGroupExpr) {
				// TODO: Or expression tree
				if (!TypeManager.IsDelegateType (t))
					return 0;

				MethodInfo invoke = Delegate.GetInvokeMethod (ec.Compiler, t, t);
				Type rtype = invoke.ReturnType;
#if MS_COMPATIBLE
				// Blablabla, because reflection does not work with dynamic types
				Type [] g_args = t.GetGenericArguments ();
				rtype = g_args [rtype.GenericParameterPosition];
#endif

				if (!TypeManager.IsGenericType (rtype))
					return 0;

				MethodGroupExpr mg = (MethodGroupExpr) e;
				Arguments args = DelegateCreation.CreateDelegateMethodArguments (TypeManager.GetParameterData (invoke), e.Location);
				mg = mg.OverloadResolve (ec, ref args, true, e.Location);
				if (mg == null)
					return 0;

				// TODO: What should happen when return type is of generic type ?
				throw new NotImplementedException ();
//				return LowerBoundInference (null, rtype) + 1;
			}

			//
			// if e is an expression with type U, then
			// a lower-bound inference is made from U for T
			//
			return LowerBoundInference (e.Type, t) * 2;
		}

		void RemoveDependentTypes (ArrayList types, Type returnType)
		{
			int idx = IsUnfixed (returnType);
			if (idx >= 0) {
				types [idx] = null;
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
