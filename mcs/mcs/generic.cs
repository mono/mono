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
using System.Collections.Generic;
using System.Text;
using System.Linq;
	
namespace Mono.CSharp {
	public enum Variance
	{
		//
		// Don't add or modify internal values, they are used as -/+ calculation signs
		//
		None			= 0,
		Covariant		= 1,
		Contravariant	= -1
	}

	[Flags]
	public enum SpecialConstraint
	{
		None		= 0,
		Constructor = 1 << 2,
		Class		= 1 << 3,
		Struct		= 1 << 4
	}

	public class SpecialContraintExpr : FullNamedExpression
	{
		public SpecialContraintExpr (SpecialConstraint constraint, Location loc)
		{
			this.loc = loc;
			this.Constraint = constraint;
		}

		public SpecialConstraint Constraint { get; private set; }

		protected override Expression DoResolve (ResolveContext rc)
		{
			throw new NotImplementedException ();
		}
	}

	//
	// A set of parsed constraints for a type parameter
	//
	public class Constraints
	{
		SimpleMemberName tparam;
		List<FullNamedExpression> constraints;
		Location loc;
		bool resolved;
		bool resolving;
		
		public Constraints (SimpleMemberName tparam, List<FullNamedExpression> constraints, Location loc)
		{
			this.tparam = tparam;
			this.constraints = constraints;
			this.loc = loc;
		}

		#region Properties

		public Location Location {
			get {
				return loc;
			}
		}

		public SimpleMemberName TypeParameter {
			get {
				return tparam;
			}
		}

		#endregion

		bool CheckConflictingInheritedConstraint (TypeSpec ba, TypeSpec bb, IMemberContext context, Location loc)
		{
			if (!TypeSpec.IsBaseClass (ba, bb, false) && !TypeSpec.IsBaseClass (bb, ba, false)) {
				context.Compiler.Report.Error (455, loc,
					"Type parameter `{0}' inherits conflicting constraints `{1}' and `{2}'",
					tparam.Value,
					ba.GetSignatureForError (), bb.GetSignatureForError ());
				return false;
			}

			return true;
		}

		public void CheckGenericConstraints (IMemberContext context)
		{
			foreach (var c in constraints) {
				var ge = c as GenericTypeExpr;
				if (ge != null)
					ge.CheckConstraints (context);
			}
		}

		//
		// Resolve the constraints types with only possible early checks, return
		// value `false' is reserved for recursive failure
		//
		public bool Resolve (IMemberContext context, TypeParameter tp)
		{
			if (resolved)
				return true;

			if (resolving)
				return false;

			resolving = true;
			var spec = tp.Type;
			List<TypeParameterSpec> tparam_types = null;
			bool iface_found = false;

			spec.BaseType = TypeManager.object_type;

			for (int i = 0; i < constraints.Count; ++i) {
				var constraint = constraints[i];

				if (constraint is SpecialContraintExpr) {
					spec.SpecialConstraint |= ((SpecialContraintExpr) constraint).Constraint;
					if (spec.HasSpecialStruct)
						spec.BaseType = TypeManager.value_type;

					// Set to null as it does not have a type
					constraints[i] = null;
					continue;
				}

				var type_expr = constraints[i] = constraint.ResolveAsTypeTerminal (context, false);
				if (type_expr == null)
					continue;

				var gexpr = type_expr as GenericTypeExpr;
				if (gexpr != null && gexpr.HasDynamicArguments ()) {
					context.Compiler.Report.Error (1968, constraint.Location,
						"A constraint cannot be the dynamic type `{0}'", gexpr.GetSignatureForError ());
					continue;
				}

				var type = type_expr.Type;

				if (!context.CurrentMemberDefinition.IsAccessibleAs (type)) {
					context.Compiler.Report.SymbolRelatedToPreviousError (type);
					context.Compiler.Report.Error (703, loc,
						"Inconsistent accessibility: constraint type `{0}' is less accessible than `{1}'",
						type.GetSignatureForError (), context.GetSignatureForError ());
				}

				if (type.IsInterface) {
					if (!spec.AddInterface (type)) {
						context.Compiler.Report.Error (405, constraint.Location,
							"Duplicate constraint `{0}' for type parameter `{1}'", type.GetSignatureForError (), tparam.Value);
					}

					iface_found = true;
					continue;
				}


				var constraint_tp = type as TypeParameterSpec;
				if (constraint_tp != null) {
					if (tparam_types == null) {
						tparam_types = new List<TypeParameterSpec> (2);
					} else if (tparam_types.Contains (constraint_tp)) {
						context.Compiler.Report.Error (405, constraint.Location,
							"Duplicate constraint `{0}' for type parameter `{1}'", type.GetSignatureForError (), tparam.Value);
						continue;
					}

					//
					// Checks whether each generic method parameter constraint type
					// is valid with respect to T
					//
					if (tp.IsMethodTypeParameter) {
						TypeManager.CheckTypeVariance (type, Variance.Contravariant, context);
					}

					var tp_def = constraint_tp.MemberDefinition as TypeParameter;
					if (tp_def != null && !tp_def.ResolveConstraints (context)) {
						context.Compiler.Report.Error (454, constraint.Location,
							"Circular constraint dependency involving `{0}' and `{1}'",
							constraint_tp.GetSignatureForError (), tp.GetSignatureForError ());
						continue;
					}

					//
					// Checks whether there are no conflicts between type parameter constraints
					//
					// class Foo<T, U>
					//      where T : A
					//      where U : B, T
					//
					// A and B are not convertible and only 1 class constraint is allowed
					//
					if (constraint_tp.HasTypeConstraint) {
						if (spec.HasTypeConstraint || spec.HasSpecialStruct) {
							if (!CheckConflictingInheritedConstraint (spec.BaseType, constraint_tp.BaseType, context, constraint.Location))
								continue;
						} else {
							for (int ii = 0; ii < tparam_types.Count; ++ii) {
								if (!tparam_types[ii].HasTypeConstraint)
									continue;

								if (!CheckConflictingInheritedConstraint (tparam_types[ii].BaseType, constraint_tp.BaseType, context, constraint.Location))
									break;
							}
						}
					}

					if (constraint_tp.HasSpecialStruct) {
						context.Compiler.Report.Error (456, constraint.Location,
							"Type parameter `{0}' has the `struct' constraint, so it cannot be used as a constraint for `{1}'",
							constraint_tp.GetSignatureForError (), tp.GetSignatureForError ());
						continue;
					}

					tparam_types.Add (constraint_tp);
					continue;
				}

				if (iface_found || spec.HasTypeConstraint) {
					context.Compiler.Report.Error (406, constraint.Location,
						"The class type constraint `{0}' must be listed before any other constraints. Consider moving type constraint to the beginning of the constraint list",
						type.GetSignatureForError ());
				}

				if (spec.HasSpecialStruct || spec.HasSpecialClass) {
					context.Compiler.Report.Error (450, type_expr.Location,
						"`{0}': cannot specify both a constraint class and the `class' or `struct' constraint",
						type.GetSignatureForError ());
				}

				if (type == InternalType.Dynamic) {
					context.Compiler.Report.Error (1967, constraint.Location, "A constraint cannot be the dynamic type");
					continue;
				}

				if (type.IsSealed || !type.IsClass) {
					context.Compiler.Report.Error (701, loc,
						"`{0}' is not a valid constraint. A constraint must be an interface, a non-sealed class or a type parameter",
						TypeManager.CSharpName (type));
					continue;
				}

				if (type.IsStatic) {
					context.Compiler.Report.Error (717, constraint.Location,
						"`{0}' is not a valid constraint. Static classes cannot be used as constraints",
						type.GetSignatureForError ());
				} else if (type == TypeManager.array_type || type == TypeManager.delegate_type ||
							type == TypeManager.enum_type || type == TypeManager.value_type ||
							type == TypeManager.object_type || type == TypeManager.multicast_delegate_type) {
					context.Compiler.Report.Error (702, constraint.Location,
						"A constraint cannot be special class `{0}'", type.GetSignatureForError ());
					continue;
				}

				spec.BaseType = type;
			}

			if (tparam_types != null)
				spec.TypeArguments = tparam_types.ToArray ();

			resolving = false;
			resolved = true;
			return true;
		}

		public void VerifyClsCompliance (Report report)
		{
			foreach (var c in constraints)
			{
				if (c == null)
					continue;

				if (!c.Type.IsCLSCompliant ()) {
					report.SymbolRelatedToPreviousError (c.Type);
					report.Warning (3024, 1, loc, "Constraint type `{0}' is not CLS-compliant",
						c.Type.GetSignatureForError ());
				}
			}
		}
	}

	//
	// A type parameter for a generic type or generic method definition
	//
	public class TypeParameter : MemberCore, ITypeDefinition
	{
		static readonly string[] attribute_target = new string [] { "type parameter" };
		
		Constraints constraints;
		GenericTypeParameterBuilder builder;
		TypeParameterSpec spec;

		public TypeParameter (DeclSpace parent, int index, MemberName name, Constraints constraints, Attributes attrs, Variance variance)
			: base (parent, name, attrs)
		{
			this.constraints = constraints;
			this.spec = new TypeParameterSpec (null, index, this, SpecialConstraint.None, variance, null);
		}

		public TypeParameter (TypeParameterSpec spec, DeclSpace parent, TypeSpec parentSpec, MemberName name, Attributes attrs)
			: base (parent, name, attrs)
		{
			this.spec = new TypeParameterSpec (parentSpec, spec.DeclaredPosition, spec.MemberDefinition, spec.SpecialConstraint, spec.Variance, null) {
				BaseType = spec.BaseType,
				InterfacesDefined = spec.InterfacesDefined,
				TypeArguments = spec.TypeArguments
			};
		}

		#region Properties

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.GenericParameter;
			}
		}

		public override string DocCommentHeader {
			get {
				throw new InvalidOperationException (
					"Unexpected attempt to get doc comment from " + this.GetType ());
			}
		}

		public bool IsMethodTypeParameter {
			get {
				return spec.IsMethodOwned;
			}
		}

		public string Namespace {
			get {
				return null;
			}
		}

		public TypeParameterSpec Type {
			get {
				return spec;
			}
		}

		public int TypeParametersCount {
			get {
				return 0;
			}
		}

		public TypeParameterSpec[] TypeParameters {
			get {
				return null;
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_target;
			}
		}

		public Variance Variance {
			get {
				return spec.Variance;
			}
		}

		#endregion

		//
		// This is called for each part of a partial generic type definition.
		//
		// If partial type parameters constraints are not null and we don't
		// already have constraints they become our constraints. If we already
		// have constraints, we must check that they're the same.
		//
		public bool AddPartialConstraints (TypeContainer part, TypeParameter tp)
		{
			if (builder == null)
				throw new InvalidOperationException ();

			var new_constraints = tp.constraints;
			if (new_constraints == null)
				return true;

			// TODO: could create spec only
			//tp.Define (null, -1, part.Definition);
			tp.spec.DeclaringType = part.Definition;
			if (!tp.ResolveConstraints (part))
				return false;

			if (constraints != null)
				return spec.HasSameConstraintsDefinition (tp.Type);

			// Copy constraint from resolved part to partial container
			spec.SpecialConstraint = tp.spec.SpecialConstraint;
			spec.InterfacesDefined = tp.spec.InterfacesDefined;
			spec.TypeArguments = tp.spec.TypeArguments;
			spec.BaseType = tp.spec.BaseType;
			
			return true;
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			builder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), cdata);
		}

		public void CheckGenericConstraints ()
		{
			if (constraints != null)
				constraints.CheckGenericConstraints (this);
		}

		public TypeParameter CreateHoistedCopy (TypeContainer declaringType, TypeSpec declaringSpec)
		{
			return new TypeParameter (spec, declaringType, declaringSpec, MemberName, null);
		}

		public override bool Define ()
		{
			return true;
		}

		//
		// This is the first method which is called during the resolving
		// process; we're called immediately after creating the type parameters
		// with SRE (by calling `DefineGenericParameters()' on the TypeBuilder /
		// MethodBuilder).
		//
		public void Define (GenericTypeParameterBuilder type, TypeSpec declaringType)
		{
			if (builder != null)
				throw new InternalErrorException ();

			this.builder = type;
			spec.DeclaringType = declaringType;
			spec.SetMetaInfo (type);
		}

		public void EmitConstraints (GenericTypeParameterBuilder builder)
		{
			var attr = GenericParameterAttributes.None;
			if (spec.Variance == Variance.Contravariant)
				attr |= GenericParameterAttributes.Contravariant;
			else if (spec.Variance == Variance.Covariant)
				attr |= GenericParameterAttributes.Covariant;

			if (spec.HasSpecialClass)
				attr |= GenericParameterAttributes.ReferenceTypeConstraint;
			else if (spec.HasSpecialStruct)
				attr |= GenericParameterAttributes.NotNullableValueTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint;

			if (spec.HasSpecialConstructor)
				attr |= GenericParameterAttributes.DefaultConstructorConstraint;

			if (spec.BaseType != TypeManager.object_type)
				builder.SetBaseTypeConstraint (spec.BaseType.GetMetaInfo ());

			if (spec.InterfacesDefined != null)
				builder.SetInterfaceConstraints (spec.InterfacesDefined.Select (l => l.GetMetaInfo ()).ToArray ());

			if (spec.TypeArguments != null)
				builder.SetInterfaceConstraints (spec.TypeArguments.Select (l => l.GetMetaInfo ()).ToArray ());

			builder.SetGenericParameterAttributes (attr);
		}

		public override void Emit ()
		{
			EmitConstraints (builder);

			if (OptAttributes != null)
				OptAttributes.Emit ();

			base.Emit ();
		}

		public void ErrorInvalidVariance (IMemberContext mc, Variance expected)
		{
			Report.SymbolRelatedToPreviousError (mc.CurrentMemberDefinition);
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

		public TypeSpec GetAttributeCoClass ()
		{
			return null;
		}

		public string GetAttributeDefaultMember ()
		{
			throw new NotSupportedException ();
		}

		public AttributeUsageAttribute GetAttributeUsage (PredefinedAttribute pa)
		{
			throw new NotSupportedException ();
		}

		public override string GetSignatureForError ()
		{
			return MemberName.Name;
		}

		public void LoadMembers (TypeSpec declaringType, bool onlyTypes, ref MemberCache cache)
		{
			throw new NotSupportedException ("Not supported for compiled definition");
		}

		//
		// Resolves all type parameter constraints
		//
		public bool ResolveConstraints (IMemberContext context)
		{
			if (constraints != null)
				return constraints.Resolve (context, this);

			if (spec.BaseType == null)
				spec.BaseType = TypeManager.object_type;

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

		public override bool IsClsComplianceRequired ()
		{
			return false;
		}

		public new void VerifyClsCompliance ()
		{
			if (constraints != null)
				constraints.VerifyClsCompliance (Report);
		}
	}

	[System.Diagnostics.DebuggerDisplay ("{DisplayDebugInfo()}")]
	public class TypeParameterSpec : TypeSpec
	{
		public static readonly new TypeParameterSpec[] EmptyTypes = new TypeParameterSpec[0];

		Variance variance;
		SpecialConstraint spec;
		readonly int tp_pos;
		TypeSpec[] targs;
		TypeSpec[] ifaces_defined;

		//
		// Creates type owned type parameter
		//
		public TypeParameterSpec (TypeSpec declaringType, int index, ITypeDefinition definition, SpecialConstraint spec, Variance variance, Type info)
			: base (MemberKind.TypeParameter, declaringType, definition, info, Modifiers.PUBLIC)
		{
			this.variance = variance;
			this.spec = spec;
			state &= ~StateFlags.Obsolete_Undetected;
			tp_pos = index;
		}

		//
		// Creates method owned type parameter
		//
		public TypeParameterSpec (int index, ITypeDefinition definition, SpecialConstraint spec, Variance variance, Type info)
			: this (null, index, definition, spec, variance, info)
		{
		}

		#region Properties

		public int DeclaredPosition {
			get {
				return tp_pos;
			}
		}

		public bool HasSpecialConstructor {
			get {
				return (spec & SpecialConstraint.Constructor) != 0;
			}
		}

		public bool HasSpecialClass {
			get {
				return (spec & SpecialConstraint.Class) != 0;
			}
		}

		public bool HasSpecialStruct {
			get {
				return (spec & SpecialConstraint.Struct) != 0;
			}
		}

		public bool HasTypeConstraint {
			get {
				return BaseType != TypeManager.object_type && BaseType != TypeManager.value_type;
			}
		}

		public override IList<TypeSpec> Interfaces {
			get {
				if ((state & StateFlags.InterfacesExpanded) == 0) {
					if (ifaces != null) {
						for (int i = 0; i < ifaces.Count; ++i ) {
							var iface_type = ifaces[i];
							if (iface_type.Interfaces != null) {
								if (ifaces_defined == null)
									ifaces_defined = ifaces.ToArray ();

								for (int ii = 0; ii < iface_type.Interfaces.Count; ++ii) {
									var ii_iface_type = iface_type.Interfaces [ii];

									AddInterface (ii_iface_type);
								}
							}
						}
					}

					if (ifaces_defined == null && ifaces != null)
						ifaces_defined = ifaces.ToArray ();

					state |= StateFlags.InterfacesExpanded;
				}

				return ifaces;
			}
		}

		//
		// Unexpanded interfaces list
		//
		public TypeSpec[] InterfacesDefined {
			get {
				if (ifaces_defined == null && ifaces != null)
					ifaces_defined = ifaces.ToArray ();

				return ifaces_defined;
			}
			set {
				ifaces_defined = value;
			}
		}

		public bool IsConstrained {
			get {
				return spec != SpecialConstraint.None || ifaces != null || targs != null || HasTypeConstraint;
			}
		}

		//
		// Returns whether the type parameter is "known to be a reference type"
		//
		public bool IsReferenceType {
			get {
				return (spec & SpecialConstraint.Class) != 0 || HasTypeConstraint;
			}
		}

		public bool IsValueType {	// TODO: Do I need this ?
			get {
				// TODO MemberCache: probably wrong
				return HasSpecialStruct;
			}
		}

		public override string Name {
			get {
				return definition.Name;
			}
		}

		public bool IsMethodOwned {
			get {
				return DeclaringType == null;
			}
		}

		public SpecialConstraint SpecialConstraint {
			get {
				return spec;
			}
			set {
				spec = value;
			}
		}

		//
		// Types used to inflate the generic type
		//
		public new TypeSpec[] TypeArguments {
			get {
				return targs;
			}
			set {
				targs = value;
			}
		}

		public Variance Variance {
			get {
				return variance;
			}
		}

		#endregion

		public string DisplayDebugInfo ()
		{
			var s = GetSignatureForError ();
			return IsMethodOwned ? s + "!!" : s + "!";
		}

		//
		// Finds effective base class
		//
		public TypeSpec GetEffectiveBase ()
		{
			if (HasSpecialStruct) {
				return TypeManager.value_type;
			}

			if (BaseType != null && targs == null)
				return BaseType;

			var types = targs;
			if (HasTypeConstraint) {
				Array.Resize (ref types, types.Length + 1);
				types[types.Length - 1] = BaseType;
			}

			if (types != null)
				return Convert.FindMostEncompassedType (types.Select (l => l.BaseType));

			return TypeManager.object_type;
		}

		public override string GetSignatureForError ()
		{
			return Name;
		}

		//
		// Constraints have to match by definition but not position, used by
		// partial classes or methods
		//
		public bool HasSameConstraintsDefinition (TypeParameterSpec other)
		{
			if (spec != other.spec)
				return false;

			if (BaseType != other.BaseType)
				return false;

			if (!TypeSpecComparer.Override.IsSame (InterfacesDefined, other.InterfacesDefined))
				return false;

			if (!TypeSpecComparer.Override.IsSame (targs, other.targs))
				return false;

			return true;
		}

		//
		// Constraints have to match by using same set of types, used by
		// implicit interface implementation
		//
		public bool HasSameConstraintsImplementation (TypeParameterSpec other)
		{
			if (spec != other.spec)
				return false;

			//
			// It can be same base type or inflated type parameter
			//
			// interface I<T> { void Foo<U> where U : T; }
			// class A : I<int> { void Foo<X> where X : int {} }
			//
			bool found;
			if (!TypeSpecComparer.Override.IsEqual (BaseType, other.BaseType)) {
				if (other.targs == null)
					return false;

				found = false;
				foreach (var otarg in other.targs) {
					if (TypeSpecComparer.Override.IsEqual (BaseType, otarg)) {
						found = true;
						break;
					}
				}

				if (!found)
					return false;
			}

			// Check interfaces implementation -> definition
			if (InterfacesDefined != null) {
				foreach (var iface in InterfacesDefined) {
					found = false;
					if (other.InterfacesDefined != null) {
						foreach (var oiface in other.InterfacesDefined) {
							if (TypeSpecComparer.Override.IsEqual (iface, oiface)) {
								found = true;
								break;
							}
						}
					}

					if (found)
						continue;

					if (other.targs != null) {
						foreach (var otarg in other.targs) {
							if (TypeSpecComparer.Override.IsEqual (BaseType, otarg)) {
								found = true;
								break;
							}
						}
					}

					if (!found)
						return false;
				}
			}

			// Check interfaces implementation <- definition
			if (other.InterfacesDefined != null) {
				if (InterfacesDefined == null)
					return false;

				foreach (var oiface in other.InterfacesDefined) {
					found = false;
					foreach (var iface in InterfacesDefined) {
						if (TypeSpecComparer.Override.IsEqual (iface, oiface)) {
							found = true;
							break;
						}
					}

					if (!found)
						return false;
				}
			}

			// Check type parameters implementation -> definition
			if (targs != null) {
				if (other.targs == null)
					return false;

				foreach (var targ in targs) {
					found = false;
					foreach (var otarg in other.targs) {
						if (TypeSpecComparer.Override.IsEqual (targ, otarg)) {
							found = true;
							break;
						}
					}

					if (!found)
						return false;
				}
			}

			// Check type parameters implementation <- definition
			if (other.targs != null) {
				foreach (var otarg in other.targs) {
					// Ignore inflated type arguments, were checked above
					if (!otarg.IsGenericParameter)
						continue;

					if (targs == null)
						return false;

					found = false;
					foreach (var targ in targs) {
						if (TypeSpecComparer.Override.IsEqual (targ, otarg)) {
							found = true;
							break;
						}
					}

					if (!found)
						return false;
				}				
			}

			return true;
		}

		public static TypeParameterSpec[] InflateConstraints (TypeParameterInflator inflator, TypeParameterSpec[] tparams)
		{
			TypeParameterSpec[] constraints = null;

			for (int i = 0; i < tparams.Length; ++i) {
				var tp = tparams[i];
				if (tp.HasTypeConstraint || tp.Interfaces != null || tp.TypeArguments != null) {
					if (constraints == null) {
						constraints = new TypeParameterSpec[tparams.Length];
						Array.Copy (tparams, constraints, constraints.Length);
					}

					constraints[i] = (TypeParameterSpec) constraints[i].InflateMember (inflator);
				}
			}

			if (constraints == null)
				constraints = tparams;

			return constraints;
		}

		public void InflateConstraints (TypeParameterInflator inflator, TypeParameterSpec tps)
		{
			tps.BaseType = inflator.Inflate (BaseType);
			if (ifaces != null) {
				tps.ifaces = new List<TypeSpec> (ifaces.Count);
				for (int i = 0; i < ifaces.Count; ++i)
					tps.ifaces.Add (inflator.Inflate (ifaces[i]));
			}
			if (targs != null) {
				tps.targs = new TypeSpec[targs.Length];
				for (int i = 0; i < targs.Length; ++i)
					tps.targs[i] = inflator.Inflate (targs[i]);
			}
		}

		public override MemberSpec InflateMember (TypeParameterInflator inflator)
		{
			var tps = (TypeParameterSpec) MemberwiseClone ();
			InflateConstraints (inflator, tps);
			return tps;
		}

		//
		// Populates type parameter members using type parameter constraints
		// The trick here is to be called late enough but not too late to
		// populate member cache with all members from other types
		//
		protected override void InitializeMemberCache (bool onlyTypes)
		{
			cache = new MemberCache ();
			if (ifaces != null) {
				foreach (var iface_type in Interfaces) {
					cache.AddInterface (iface_type);
				}
			}
		}

		public bool IsConvertibleToInterface (TypeSpec iface)
		{
			if (Interfaces != null) {
				foreach (var t in Interfaces) {
					if (t == iface)
						return true;
				}
			}

			if (TypeArguments != null) {
				foreach (var t in TypeArguments) {
					if (((TypeParameterSpec) t).IsConvertibleToInterface (iface))
						return true;
				}
			}

			return false;
		}

		public override TypeSpec Mutate (TypeParameterMutator mutator)
		{
			return mutator.Mutate (this);
		}
	}

	public struct TypeParameterInflator
	{
		readonly TypeSpec type;
		readonly TypeParameterSpec[] tparams;
		readonly TypeSpec[] targs;

		public TypeParameterInflator (TypeParameterInflator nested, TypeSpec type)
			: this (type, nested.tparams, nested.targs)
		{
		}

		public TypeParameterInflator (TypeSpec type, TypeParameterSpec[] tparams, TypeSpec[] targs)
		{
			if (tparams.Length != targs.Length)
				throw new ArgumentException ("Invalid arguments");

			this.tparams = tparams;
			this.targs = targs;
			this.type = type;
		}

		//
		// Type parameters to inflate
		//
		public TypeParameterSpec[] TypeParameters {
			get {
				return tparams;
			}
		}

		public TypeSpec Inflate (TypeSpec ts)
		{
			var tp = ts as TypeParameterSpec;
			if (tp != null)
				return Inflate (tp);

			var ac = ts as ArrayContainer;
			if (ac != null) {
				var et = Inflate (ac.Element);
				if (et != ac.Element)
					return ArrayContainer.MakeType (et, ac.Rank);

				return ac;
			}

			//
			// When inflating a nested type, inflate its parent first
			// in case it's using same type parameters (was inflated within the type)
			//
			if (ts.IsNested) {
				var parent = Inflate (ts.DeclaringType);
				if (ts.DeclaringType != parent) {
					//
					// Keep the inflated type arguments
					// 
					var targs = ts.TypeArguments;

					//
					// Parent was inflated, find the same type on inflated type
					// to use same cache for nested types on same generic parent
					//
					// TODO: Should use BindingRestriction.DeclaredOnly or GetMember
					ts = MemberCache.FindNestedType (parent, ts.Name, ts.Arity);

					//
					// Handle the tricky case where parent shares local type arguments
					// which means inflating inflated type
					//
					// class Test<T> {
					//		public static Nested<T> Foo () { return null; }
					//
					//		public class Nested<U> {}
					//	}
					//
					//  return type of Test<string>.Foo() has to be Test<string>.Nested<string> 
					//
					if (targs.Length > 0) {
						var inflated_targs = new TypeSpec [targs.Length];
						for (var i = 0; i < targs.Length; ++i)
							inflated_targs[i] = Inflate (targs[i]);

						ts = ts.MakeGenericType (inflated_targs);
					}

					return ts;
				}
			}

			// Inflate generic type
			if (ts.Arity > 0)
				return InflateTypeParameters (ts);

			return ts;
		}

		public TypeSpec Inflate (TypeParameterSpec tp)
		{
			for (int i = 0; i < tparams.Length; ++i)
				if (tparams [i] == tp)
					return targs[i];

			// This can happen when inflating nested types
			// without type arguments specified
			return tp;
		}

		//
		// Inflates generic types
		//
		TypeSpec InflateTypeParameters (TypeSpec type)
		{
			var targs = new TypeSpec[type.Arity];
			var i = 0;

			var gti = type as InflatedTypeSpec;

			//
			// Inflating using outside type arguments, var v = new Foo<int> (), class Foo<T> {}
			//
			if (gti != null) {
				for (; i < targs.Length; ++i)
					targs[i] = Inflate (gti.TypeArguments[i]);

				return gti.GetDefinition ().MakeGenericType (targs);
			}

			//
			// Inflating parent using inside type arguments, class Foo<T> { ITest<T> foo; }
			//
			var args = type.MemberDefinition.TypeParameters;
			foreach (var ds_tp in args)
				targs[i++] = Inflate (ds_tp);

			return type.MakeGenericType (targs);
		}

		public TypeSpec TypeInstance {
			get { return type; }
		}
	}

	//
	// Before emitting any code we have to change all MVAR references to VAR
	// when the method is of generic type and has hoisted variables
	//
	public class TypeParameterMutator
	{
		TypeParameter[] mvar;
		TypeParameter[] var;
		Dictionary<TypeSpec, TypeSpec> mutated_typespec = new Dictionary<TypeSpec, TypeSpec> ();

		public TypeParameterMutator (TypeParameter[] mvar, TypeParameter[] var)
		{
			if (mvar.Length != var.Length)
				throw new ArgumentException ();

			this.mvar = mvar;
			this.var = var;
		}

		#region Properties

		public TypeParameter[] MethodTypeParameters {
			get {
				return mvar;
			}
		}

		#endregion

		public static TypeSpec GetMemberDeclaringType (TypeSpec type)
		{
			if (type is InflatedTypeSpec) {
				if (type.DeclaringType == null)
					return type.GetDefinition ();

				var parent = GetMemberDeclaringType (type.DeclaringType);
				type = MemberCache.GetMember<TypeSpec> (parent, type);
			}

			return type;
		}

		public TypeSpec Mutate (TypeSpec ts)
		{
			TypeSpec value;
			if (mutated_typespec.TryGetValue (ts, out value))
				return value;

			value = ts.Mutate (this);
			mutated_typespec.Add (ts, value);
			return value;
		}

		public TypeParameterSpec Mutate (TypeParameterSpec tp)
		{
			for (int i = 0; i < mvar.Length; ++i) {
				if (mvar[i].Type == tp)
					return var[i].Type;
			}

			return tp;
		}

		public TypeSpec[] Mutate (TypeSpec[] targs)
		{
			TypeSpec[] mutated = new TypeSpec[targs.Length];
			bool changed = false;
			for (int i = 0; i < targs.Length; ++i) {
				mutated[i] = Mutate (targs[i]);
				changed |= targs[i] != mutated[i];
			}

			return changed ? mutated : targs;
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

		public override bool CheckAccessLevel (IMemberContext ds)
		{
			return true;
		}
	}

	public class InflatedTypeSpec : TypeSpec
	{
		TypeSpec[] targs;
		TypeParameterSpec[] constraints;
		readonly TypeSpec open_type;

		public InflatedTypeSpec (TypeSpec openType, TypeSpec declaringType, TypeSpec[] targs)
			: base (openType.Kind, declaringType, openType.MemberDefinition, null, openType.Modifiers)
		{
			if (targs == null)
				throw new ArgumentNullException ("targs");

//			this.state = openType.state;
			this.open_type = openType;
			this.targs = targs;

			foreach (var arg in targs) {
				if (arg.HasDynamicElement || arg == InternalType.Dynamic) {
					state |= StateFlags.HasDynamicElement;
					break;
				}
			}
		}

		#region Properties

		public override TypeSpec BaseType {
			get {
				if (cache == null || (state & StateFlags.PendingBaseTypeInflate) != 0)
					InitializeMemberCache (true);

				return base.BaseType;
			}
		}

		//
		// Inflated type parameters with constraints array, mapping with type arguments is based on index
		//
		public TypeParameterSpec[] Constraints {
			get {
				if (constraints == null) {
					var inflator = CreateLocalInflator ();
					constraints = TypeParameterSpec.InflateConstraints (inflator, MemberDefinition.TypeParameters);
				}

				return constraints;
			}
		}

		public override IList<TypeSpec> Interfaces {
			get {
				if (cache == null)
					InitializeMemberCache (true);

				return base.Interfaces;
			}
		}

		//
		// Types used to inflate the generic  type
		//
		public override TypeSpec[] TypeArguments {
			get {
				return targs;
			}
		}

		#endregion

		TypeParameterInflator CreateLocalInflator ()
		{
			TypeParameterSpec[] tparams_full;
			TypeSpec[] targs_full = targs;
			if (IsNested) {
				//
				// Special case is needed when we are inflating an open type (nested type definition)
				// on inflated parent. Consider following case
				//
				// Foo<T>.Bar<U> => Foo<string>.Bar<U>
				//
				// Any later inflation of Foo<string>.Bar<U> has to also inflate T if used inside Bar<U>
				//
				List<TypeSpec> merged_targs = null;
				List<TypeParameterSpec> merged_tparams = null;

				var type = DeclaringType;

				do {
					if (type.TypeArguments.Length > 0) {
						if (merged_targs == null) {
							merged_targs = new List<TypeSpec> ();
							merged_tparams = new List<TypeParameterSpec> ();
							if (targs.Length > 0) {
								merged_targs.AddRange (targs);
								merged_tparams.AddRange (open_type.MemberDefinition.TypeParameters);
							}
						}
						merged_tparams.AddRange (type.MemberDefinition.TypeParameters);
						merged_targs.AddRange (type.TypeArguments);
					}
					type = type.DeclaringType;
				} while (type != null);

				if (merged_targs != null) {
					// Type arguments are not in the right order but it should not matter in this case
					targs_full = merged_targs.ToArray ();
					tparams_full = merged_tparams.ToArray ();
				} else if (targs.Length == 0) {
					tparams_full = TypeParameterSpec.EmptyTypes;
				} else {
					tparams_full = open_type.MemberDefinition.TypeParameters;
				}
			} else if (targs.Length == 0) {
				tparams_full = TypeParameterSpec.EmptyTypes;
			} else {
				tparams_full = open_type.MemberDefinition.TypeParameters;
			}

			return new TypeParameterInflator (this, tparams_full, targs_full);
		}

		Type CreateMetaInfo (TypeParameterMutator mutator)
		{
			//
			// Converts nested type arguments into right order
			// Foo<string, bool>.Bar<int> => string, bool, int
			//
			var all = new List<Type> ();
			TypeSpec type = this;
			TypeSpec definition = type;
			do {
				if (type.GetDefinition().IsGeneric) {
					all.InsertRange (0,
						type.TypeArguments != TypeSpec.EmptyTypes ?
						type.TypeArguments.Select (l => l.GetMetaInfo ()) :
						type.MemberDefinition.TypeParameters.Select (l => l.GetMetaInfo ()));
				}

				definition = definition.GetDefinition ();
				type = type.DeclaringType;
			} while (type != null);

			return definition.GetMetaInfo ().MakeGenericType (all.ToArray ());
		}

		public override ObsoleteAttribute GetAttributeObsolete ()
		{
			return open_type.GetAttributeObsolete ();
		}

		protected override bool IsNotCLSCompliant ()
		{
			if (base.IsNotCLSCompliant ())
				return true;

			foreach (var ta in TypeArguments) {
				if (ta.MemberDefinition.IsNotCLSCompliant ())
					return true;
			}

			return false;
		}

		public override TypeSpec GetDefinition ()
		{
			return open_type;
		}

		public override Type GetMetaInfo ()
		{
			if (info == null)
				info = CreateMetaInfo (null);

			return info;
		}

		public override string GetSignatureForError ()
		{
			if (TypeManager.IsNullableType (open_type))
				return targs[0].GetSignatureForError () + "?";

			return base.GetSignatureForError ();
		}

		protected override string GetTypeNameSignature ()
		{
			if (targs.Length == 0 || MemberDefinition is AnonymousTypeClass)
				return null;

			return "<" + TypeManager.CSharpName (targs) + ">";
		}

		protected override void InitializeMemberCache (bool onlyTypes)
		{
			if (cache == null)
				cache = new MemberCache (onlyTypes ? open_type.MemberCacheTypes : open_type.MemberCache);

			var inflator = CreateLocalInflator ();

			//
			// Two stage inflate due to possible nested types recursive
			// references
			//
			// class A<T> {
			//    B b;
			//    class B {
			//      T Value;
			//    }
			// }
			//
			// When resolving type of `b' members of `B' cannot be 
			// inflated because are not yet available in membercache
			//
			if ((state & StateFlags.PendingMemberCacheMembers) == 0) {
				open_type.MemberCacheTypes.InflateTypes (cache, inflator);

				//
				// Inflate any implemented interfaces
				//
				if (open_type.Interfaces != null) {
					ifaces = new List<TypeSpec> (open_type.Interfaces.Count);
					foreach (var iface in open_type.Interfaces) {
						var iface_inflated = inflator.Inflate (iface);
						AddInterface (iface_inflated);
					}
				}

				//
				// Handles the tricky case of recursive nested base generic type
				//
				// class A<T> : Base<A<T>.Nested> {
				//    class Nested {}
				// }
				//
				// When inflating A<T>. base type is not yet known, secondary
				// inflation is required (not common case) once base scope
				// is known
				//
				if (open_type.BaseType == null) {
					if (IsClass)
						state |= StateFlags.PendingBaseTypeInflate;
				} else {
					BaseType = inflator.Inflate (open_type.BaseType);
				}
			} else if ((state & StateFlags.PendingBaseTypeInflate) != 0) {
				BaseType = inflator.Inflate (open_type.BaseType);
				state &= ~StateFlags.PendingBaseTypeInflate;
			}

			if (onlyTypes) {
				state |= StateFlags.PendingMemberCacheMembers;
				return;
			}

			var tc = open_type.MemberDefinition as TypeContainer;
			if (tc != null && !tc.HasMembersDefined)
				throw new InternalErrorException ("Inflating MemberCache with undefined members");

			if ((state & StateFlags.PendingBaseTypeInflate) != 0) {
				BaseType = inflator.Inflate (open_type.BaseType);
				state &= ~StateFlags.PendingBaseTypeInflate;
			}

			state &= ~StateFlags.PendingMemberCacheMembers;
			open_type.MemberCache.InflateMembers (cache, open_type, inflator);
		}

		public override TypeSpec Mutate (TypeParameterMutator mutator)
		{
			var targs = TypeArguments;
			if (targs != null)
				targs = mutator.Mutate (targs);

			var decl = DeclaringType;
			if (IsNested && DeclaringType.IsGenericOrParentIsGeneric)
				decl = mutator.Mutate (decl);

			if (targs == TypeArguments && decl == DeclaringType)
				return this;

			var mutated = (InflatedTypeSpec) MemberwiseClone ();
			if (decl != DeclaringType) {
				// Gets back MethodInfo in case of metaInfo was inflated
				//mutated.info = MemberCache.GetMember<TypeSpec> (DeclaringType.GetDefinition (), this).info;

				mutated.declaringType = decl;
				mutated.state |= StateFlags.PendingMetaInflate;
			}

			if (targs != null) {
				mutated.targs = targs;
				mutated.info = null;
			}

			return mutated;
		}
	}


	//
	// Tracks the type arguments when instantiating a generic type. It's used
	// by both type arguments and type parameters
	//
	public class TypeArguments
	{
		List<FullNamedExpression> args;
		TypeSpec[] atypes;

		public TypeArguments (params FullNamedExpression[] types)
		{
			this.args = new List<FullNamedExpression> (types);
		}

		public void Add (FullNamedExpression type)
		{
			args.Add (type);
		}

		// TODO: Kill this monster
		public TypeParameterName[] GetDeclarations ()
		{
			return args.ConvertAll (i => (TypeParameterName) i).ToArray ();
		}

		/// <summary>
		///   We may only be used after Resolve() is called and return the fully
		///   resolved types.
		/// </summary>
		// TODO: Not needed, just return type from resolve
		public TypeSpec[] Arguments {
			get {
				return atypes;
			}
		}

		public int Count {
			get {
				return args.Count;
			}
		}

		public virtual bool IsEmpty {
			get {
				return false;
			}
		}

		public string GetSignatureForError()
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < Count; ++i) {
				var expr = args[i];
				if (expr != null)
					sb.Append (expr.GetSignatureForError ());

				if (i + 1 < Count)
					sb.Append (',');
			}

			return sb.ToString ();
		}

		/// <summary>
		///   Resolve the type arguments.
		/// </summary>
		public virtual bool Resolve (IMemberContext ec)
		{
			if (atypes != null)
			    return atypes.Length != 0;

			int count = args.Count;
			bool ok = true;

			atypes = new TypeSpec [count];

			for (int i = 0; i < count; i++){
				TypeExpr te = args[i].ResolveAsTypeTerminal (ec, false);
				if (te == null) {
					ok = false;
					continue;
				}

				atypes[i] = te.Type;

				if (te.Type.IsStatic) {
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
				atypes = TypeSpec.EmptyTypes;

			return ok;
		}

		public TypeArguments Clone ()
		{
			TypeArguments copy = new TypeArguments ();
			foreach (var ta in args)
				copy.args.Add (ta);

			return copy;
		}
	}

	public class UnboundTypeArguments : TypeArguments
	{
		public UnboundTypeArguments (int arity)
			: base (new FullNamedExpression[arity])
		{
		}

		public override bool IsEmpty {
			get {
				return true;
			}
		}

		public override bool Resolve (IMemberContext ec)
		{
			// Nothing to be resolved
			return true;
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

	//
	// A type expression of generic type with type arguments
	//
	class GenericTypeExpr : TypeExpr
	{
		TypeArguments args;
		TypeSpec open_type;
		bool constraints_checked;

		/// <summary>
		///   Instantiate the generic type `t' with the type arguments `args'.
		///   Use this constructor if you already know the fully resolved
		///   generic type.
		/// </summary>		
		public GenericTypeExpr (TypeSpec open_type, TypeArguments args, Location l)
		{
			this.open_type = open_type;
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
			if (!args.Resolve (ec))
				return null;

			TypeSpec[] atypes = args.Arguments;

			//
			// Now bind the parameters
			//
			type = open_type.MakeGenericType (atypes);

			//
			// Check constraints when context is not method/base type
			//
			if (!ec.HasUnresolvedConstraints)
				CheckConstraints (ec);

			return this;
		}

		//
		// Checks the constraints of open generic type against type
		// arguments. Has to be called after all members have been defined
		//
		public bool CheckConstraints (IMemberContext ec)
		{
			if (constraints_checked)
				return true;

			constraints_checked = true;

			var gtype = (InflatedTypeSpec) type;
			var constraints = gtype.Constraints;
			if (constraints == null)
				return true;

			return new ConstraintChecker(ec).CheckAll (open_type, args.Arguments, constraints, loc);
		}
	
		public override bool CheckAccessLevel (IMemberContext mc)
		{
			DeclSpace c = mc.CurrentMemberDefinition as DeclSpace;
			if (c == null)
				c = mc.CurrentMemberDefinition.Parent;

			return c.CheckAccessLevel (open_type);
		}

		public bool HasDynamicArguments ()
		{
			return HasDynamicArguments (args.Arguments);
		}

		static bool HasDynamicArguments (TypeSpec[] args)
		{
			for (int i = 0; i < args.Length; ++i) {
				var item = args[i];

				if (item == InternalType.Dynamic)
					return true;

				if (TypeManager.IsGenericType (item))
					return HasDynamicArguments (TypeManager.GetTypeArguments (item));

				if (item.IsArray) {
					while (item.IsArray) {
						item = ((ArrayContainer) item).Element;
					}

					if (item == InternalType.Dynamic)
						return true;
				}
			}

			return false;
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

	//
	// Generic type with unbound type arguments, used for typeof (G<,,>)
	//
	class GenericOpenTypeExpr : TypeExpr
	{
		public GenericOpenTypeExpr (TypeSpec type, /*UnboundTypeArguments args,*/ Location loc)
		{
			this.type = type.GetDefinition ();
			this.loc = loc;
		}

		protected override TypeExpr DoResolveAsTypeStep (IMemberContext ec)
		{
			return this;
		}
	}

	struct ConstraintChecker
	{
		IMemberContext mc;
		bool ignore_inferred_dynamic;

		public ConstraintChecker (IMemberContext ctx)
		{
			this.mc = ctx;
			ignore_inferred_dynamic = false;
		}

		#region Properties

		public bool IgnoreInferredDynamic {
			get {
				return ignore_inferred_dynamic;
			}
			set {
				ignore_inferred_dynamic = value;
			}
		}

		#endregion

		//
		// Checks all type arguments againts type parameters constraints
		// NOTE: It can run in probing mode when `mc' is null
		//
		public bool CheckAll (MemberSpec context, TypeSpec[] targs, TypeParameterSpec[] tparams, Location loc)
		{
			for (int i = 0; i < tparams.Length; i++) {
				if (ignore_inferred_dynamic && targs[i] == InternalType.Dynamic)
					continue;

				if (!CheckConstraint (context, targs [i], tparams [i], loc))
					return false;
			}

			return true;
		}

		bool CheckConstraint (MemberSpec context, TypeSpec atype, TypeParameterSpec tparam, Location loc)
		{
			//
			// First, check the `class' and `struct' constraints.
			//
			if (tparam.HasSpecialClass && !TypeManager.IsReferenceType (atype)) {
				if (mc != null) {
					mc.Compiler.Report.Error (452, loc,
						"The type `{0}' must be a reference type in order to use it as type parameter `{1}' in the generic type or method `{2}'",
						TypeManager.CSharpName (atype), tparam.GetSignatureForError (), context.GetSignatureForError ());
				}

				return false;
			}

			if (tparam.HasSpecialStruct && (!TypeManager.IsValueType (atype) || TypeManager.IsNullableType (atype))) {
				if (mc != null) {
					mc.Compiler.Report.Error (453, loc,
						"The type `{0}' must be a non-nullable value type in order to use it as type parameter `{1}' in the generic type or method `{2}'",
						TypeManager.CSharpName (atype), tparam.GetSignatureForError (), context.GetSignatureForError ());
				}

				return false;
			}

			bool ok = true;

			//
			// Check the class constraint
			//
			if (tparam.HasTypeConstraint) {
				if (!CheckConversion (mc, context, atype, tparam, tparam.BaseType, loc)) {
					if (mc == null)
						return false;

					ok = false;
				}
			}

			//
			// Check the interfaces constraints
			//
			if (tparam.Interfaces != null) {
				if (TypeManager.IsNullableType (atype)) {
					if (mc == null)
						return false;

					mc.Compiler.Report.Error (313, loc,
						"The type `{0}' cannot be used as type parameter `{1}' in the generic type or method `{2}'. The nullable type `{0}' never satisfies interface constraint",
						atype.GetSignatureForError (), tparam.GetSignatureForError (), context.GetSignatureForError ());
					ok = false;
				} else {
					foreach (TypeSpec iface in tparam.Interfaces) {
						if (!CheckConversion (mc, context, atype, tparam, iface, loc)) {
							if (mc == null)
								return false;

							ok = false;
						}
					}
				}
			}

			//
			// Check the type parameter constraint
			//
			if (tparam.TypeArguments != null) {
				foreach (var ta in tparam.TypeArguments) {
					if (!CheckConversion (mc, context, atype, tparam, ta, loc)) {
						if (mc == null)
							return false;

						ok = false;
					}
				}
			}

			//
			// Finally, check the constructor constraint.
			//
			if (!tparam.HasSpecialConstructor)
				return ok;

			if (!HasDefaultConstructor (atype)) {
				if (mc != null) {
					mc.Compiler.Report.SymbolRelatedToPreviousError (atype);
					mc.Compiler.Report.Error (310, loc,
						"The type `{0}' must have a public parameterless constructor in order to use it as parameter `{1}' in the generic type or method `{2}'",
						TypeManager.CSharpName (atype), tparam.GetSignatureForError (), context.GetSignatureForError ());
				}
				return false;
			}

			return ok;
		}

		static bool HasDynamicTypeArgument (TypeSpec[] targs)
		{
			for (int i = 0; i < targs.Length; ++i) {
				var targ = targs [i];
				if (targ == InternalType.Dynamic)
					return true;

				if (HasDynamicTypeArgument (targ.TypeArguments))
					return true;
			}

			return false;
		}

		bool CheckConversion (IMemberContext mc, MemberSpec context, TypeSpec atype, TypeParameterSpec tparam, TypeSpec ttype, Location loc)
		{
			var expr = new EmptyExpression (atype);
			if (Convert.ImplicitStandardConversionExists (expr, ttype))
				return true;

			//
			// When partial/full type inference finds a dynamic type argument delay
			// the constraint check to runtime, it can succeed for real underlying
			// dynamic type
			//
			if (ignore_inferred_dynamic && HasDynamicTypeArgument (ttype.TypeArguments))
				return true;

			if (mc != null) {
				mc.Compiler.Report.SymbolRelatedToPreviousError (tparam);
				if (TypeManager.IsValueType (atype)) {
					mc.Compiler.Report.Error (315, loc,
						"The type `{0}' cannot be used as type parameter `{1}' in the generic type or method `{2}'. There is no boxing conversion from `{0}' to `{3}'",
						atype.GetSignatureForError (), tparam.GetSignatureForError (), context.GetSignatureForError (), ttype.GetSignatureForError ());
				} else if (atype.IsGenericParameter) {
					mc.Compiler.Report.Error (314, loc,
						"The type `{0}' cannot be used as type parameter `{1}' in the generic type or method `{2}'. There is no boxing or type parameter conversion from `{0}' to `{3}'",
						atype.GetSignatureForError (), tparam.GetSignatureForError (), context.GetSignatureForError (), ttype.GetSignatureForError ());
				} else {
					mc.Compiler.Report.Error (311, loc,
						"The type `{0}' cannot be used as type parameter `{1}' in the generic type or method `{2}'. There is no implicit reference conversion from `{0}' to `{3}'",
						atype.GetSignatureForError (), tparam.GetSignatureForError (), context.GetSignatureForError (), ttype.GetSignatureForError ());
				}
			}

			return false;
		}

		bool HasDefaultConstructor (TypeSpec atype)
		{
			var tp = atype as TypeParameterSpec;
			if (tp != null) {
				return tp.HasSpecialConstructor || tp.HasSpecialStruct;
			}

			if (atype.IsStruct || atype.IsEnum)
				return true;

			if (atype.IsAbstract)
				return false;

			var tdef = atype.GetDefinition ();

			//
			// In some circumstances MemberCache is not yet populated and members
			// cannot be defined yet (recursive type new constraints)
			//
			// class A<T> where T : B<T>, new () {}
			// class B<T> where T : A<T>, new () {}
			//
			var tc = tdef.MemberDefinition as Class;
			if (tc != null) {
				if (tc.InstanceConstructors == null) {
					// Default ctor will be generated later
					return true;
				}

				foreach (var c in tc.InstanceConstructors) {
					if (c.ParameterInfo.IsEmpty) {
						if ((c.ModFlags & Modifiers.PUBLIC) != 0)
							return true;
					}
				}

				return false;
			}

			var found = MemberCache.FindMember (tdef,
				MemberFilter.Constructor (ParametersCompiled.EmptyReadOnlyParameters),
				BindingRestriction.DeclaredOnly | BindingRestriction.InstanceOnly);

			return found != null && (found.Modifiers & Modifiers.PUBLIC) != 0;
		}
	}

	/// <summary>
	///   A generic method definition.
	/// </summary>
	public class GenericMethod : DeclSpace
	{
		ParametersCompiled parameters;

		public GenericMethod (NamespaceEntry ns, DeclSpace parent, MemberName name,
				      FullNamedExpression return_type, ParametersCompiled parameters)
			: base (ns, parent, name, null)
		{
			this.parameters = parameters;
		}

		public GenericMethod (NamespaceEntry ns, DeclSpace parent, MemberName name, TypeParameter[] tparams,
					  FullNamedExpression return_type, ParametersCompiled parameters)
			: this (ns, parent, name, return_type, parameters)
		{
			this.type_params = tparams;
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

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			throw new NotSupportedException ();
		}

		public override bool Define ()
		{
			throw new NotSupportedException ();
		}

		/// <summary>
		///   Define and resolve the type parameters.
		///   We're called from Method.Define().
		/// </summary>
		public bool Define (MethodOrOperator m)
		{
			TypeParameterName[] names = MemberName.TypeArguments.GetDeclarations ();
			string[] snames = new string [names.Length];
			var block = m.Block;
			for (int i = 0; i < names.Length; i++) {
				string type_argument_name = names[i].Name;

				if (block == null) {
					int idx = parameters.GetParameterIndexByName (type_argument_name);
					if (idx >= 0) {
						var b = m.Block;
						if (b == null)
							b = new ToplevelBlock (Compiler, Location);

						b.Error_AlreadyDeclaredTypeParameter (type_argument_name, parameters[i].Location);
					}
				} else {
					INamedBlockVariable variable = null;
					block.GetLocalName (type_argument_name, m.Block, ref variable);
					if (variable != null)
						variable.Block.Error_AlreadyDeclaredTypeParameter (type_argument_name, variable.Location);
				}

				snames[i] = type_argument_name;
			}

			GenericTypeParameterBuilder[] gen_params = m.MethodBuilder.DefineGenericParameters (snames);
			for (int i = 0; i < TypeParameters.Length; i++)
				TypeParameters [i].Define (gen_params [i], null);

			return true;
		}

		public void EmitAttributes ()
		{
			if (OptAttributes != null)
				OptAttributes.Emit ();
		}

		public override string GetSignatureForError ()
		{
			return base.GetSignatureForError () + parameters.GetSignatureForError ();
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
				tp.VerifyClsCompliance ();
			}
		}
	}

	public partial class TypeManager
	{
		public static Variance CheckTypeVariance (TypeSpec t, Variance expected, IMemberContext member)
		{
			var tp = t as TypeParameterSpec;
			if (tp != null) {
				Variance v = tp.Variance;
				if (expected == Variance.None && v != expected ||
					expected == Variance.Covariant && v == Variance.Contravariant ||
					expected == Variance.Contravariant && v == Variance.Covariant) {
					((TypeParameter)tp.MemberDefinition).ErrorInvalidVariance (member, expected);
				}

				return expected;
			}

			if (t.TypeArguments.Length > 0) {
				var targs_definition = t.MemberDefinition.TypeParameters;
				TypeSpec[] targs = GetTypeArguments (t);
				for (int i = 0; i < targs.Length; ++i) {
					Variance v = targs_definition[i].Variance;
					CheckTypeVariance (targs[i], (Variance) ((int)v * (int)expected), member);
				}

				return expected;
			}

			if (t.IsArray)
				return CheckTypeVariance (GetElementType (t), expected, member);

			return Variance.None;
		}
	}

	//
	// Implements C# type inference
	//
	class TypeInference
	{
		//
		// Tracks successful rate of type inference
		//
		int score = int.MaxValue;
		readonly Arguments arguments;
		readonly int arg_count;

		public TypeInference (Arguments arguments)
		{
			this.arguments = arguments;
			if (arguments != null)
				arg_count = arguments.Count;
		}

		public int InferenceScore {
			get {
				return score;
			}
		}

		public TypeSpec[] InferMethodArguments (ResolveContext ec, MethodSpec method)
		{
			var method_generic_args = method.GenericDefinition.TypeParameters;
			TypeInferenceContext context = new TypeInferenceContext (method_generic_args);
			if (!context.UnfixedVariableExists)
				return TypeSpec.EmptyTypes;

			AParametersCollection pd = method.Parameters;
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

			TypeSpec [] ptypes = methodParameters.Types;
			
			//
			// The first inference phase
			//
			TypeSpec method_parameter = null;
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

					ptypes = (TypeSpec[]) ptypes.Clone ();
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

				if (a.Expr.Type == InternalType.Null)
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

		bool DoSecondPhase (ResolveContext ec, TypeInferenceContext tic, TypeSpec[] methodParameters, bool fixDependent)
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
				TypeSpec t_i = methodParameters [i >= methodParameters.Length ? methodParameters.Length - 1: i];
				
				if (!TypeManager.IsDelegateType (t_i)) {
					if (t_i.GetDefinition () != TypeManager.expression_type)
						continue;

					t_i = TypeManager.GetTypeArguments (t_i) [0];
				}

				var mi = Delegate.GetInvokeMethod (ec.Compiler, t_i);
				TypeSpec rtype = mi.ReturnType;

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

		class BoundInfo : IEquatable<BoundInfo>
		{
			public readonly TypeSpec Type;
			public readonly BoundKind Kind;

			public BoundInfo (TypeSpec type, BoundKind kind)
			{
				this.Type = type;
				this.Kind = kind;
			}
			
			public override int GetHashCode ()
			{
				return Type.GetHashCode ();
			}

			#region IEquatable<BoundInfo> Members

			public bool Equals (BoundInfo other)
			{
				return Type == other.Type && Kind == other.Kind;
			}

			#endregion
		}

		readonly TypeSpec[] unfixed_types;
		readonly TypeSpec[] fixed_types;
		readonly List<BoundInfo>[] bounds;
		bool failed;

		// TODO MemberCache: Could it be TypeParameterSpec[] ??
		public TypeInferenceContext (TypeSpec[] typeArguments)
		{
			if (typeArguments.Length == 0)
				throw new ArgumentException ("Empty generic arguments");

			fixed_types = new TypeSpec [typeArguments.Length];
			for (int i = 0; i < typeArguments.Length; ++i) {
				if (typeArguments [i].IsGenericParameter) {
					if (bounds == null) {
						bounds = new List<BoundInfo> [typeArguments.Length];
						unfixed_types = new TypeSpec [typeArguments.Length];
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
			fixed_types = new TypeSpec [1];
			unfixed_types = new TypeSpec [1];
			unfixed_types[0] = InternalType.Arglist; // it can be any internal type
			bounds = new List<BoundInfo> [1];
		}

		public TypeSpec[] InferredTypeArguments {
			get {
				return fixed_types;
			}
		}

		public void AddCommonTypeBound (TypeSpec type)
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

			var a = bounds [index];
			if (a == null) {
				a = new List<BoundInfo> (2);
				a.Add (bound);
				bounds [index] = a;
				return;
			}

			if (a.Contains (bound))
				return;

			a.Add (bound);
		}
		
		bool AllTypesAreFixed (TypeSpec[] types)
		{
			foreach (TypeSpec t in types) {
				if (t.IsGenericParameter) {
					if (!IsFixed (t))
						return false;
					continue;
				}

				if (TypeManager.IsGenericType (t))
					return AllTypesAreFixed (TypeManager.GetTypeArguments (t));
			}
			
			return true;
		}		

		//
		// 26.3.3.8 Exact Inference
		//
		public int ExactInference (TypeSpec u, TypeSpec v)
		{
			// If V is an array type
			if (v.IsArray) {
				if (!u.IsArray)
					return 0;

				// TODO MemberCache: GetMetaInfo ()
				if (u.GetMetaInfo ().GetArrayRank () != v.GetMetaInfo ().GetArrayRank ())
					return 0;

				return ExactInference (TypeManager.GetElementType (u), TypeManager.GetElementType (v));
			}

			// If V is constructed type and U is constructed type
			if (TypeManager.IsGenericType (v)) {
				if (!TypeManager.IsGenericType (u))
					return 0;

				TypeSpec [] ga_u = TypeManager.GetTypeArguments (u);
				TypeSpec [] ga_v = TypeManager.GetTypeArguments (v);
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
		public bool FixIndependentTypeArguments (ResolveContext ec, TypeSpec[] methodParameters, ref bool fixed_any)
		{
			var types_to_fix = new List<TypeSpec> (unfixed_types);
			for (int i = 0; i < methodParameters.Length; ++i) {
				TypeSpec t = methodParameters[i];

				if (!TypeManager.IsDelegateType (t)) {
					if (TypeManager.expression_type == null || t.MemberDefinition != TypeManager.expression_type.MemberDefinition)
						continue;

					t =  TypeManager.GetTypeArguments (t) [0];
				}

				if (t.IsGenericParameter)
					continue;

				var invoke = Delegate.GetInvokeMethod (ec.Compiler, t);
				TypeSpec rtype = invoke.ReturnType;
				if (!rtype.IsGenericParameter && !TypeManager.IsGenericType (rtype))
					continue;

				// Remove dependent types, they cannot be fixed yet
				RemoveDependentTypes (types_to_fix, rtype);
			}

			foreach (TypeSpec t in types_to_fix) {
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

			var candidates = bounds [i];
			if (candidates == null)
				return false;

			if (candidates.Count == 1) {
				unfixed_types[i] = null;
				TypeSpec t = candidates[0].Type;
				if (t == InternalType.Null)
					return false;

				fixed_types [i] = t;
				return true;
			}

			//
			// Determines a unique type from which there is
			// a standard implicit conversion to all the other
			// candidate types.
			//
			TypeSpec best_candidate = null;
			int cii;
			int candidates_count = candidates.Count;
			for (int ci = 0; ci < candidates_count; ++ci) {
				BoundInfo bound = candidates [ci];
				for (cii = 0; cii < candidates_count; ++cii) {
					if (cii == ci)
						continue;

					BoundInfo cbound = candidates[cii];
					
					// Same type parameters with different bounds
					if (cbound.Type == bound.Type) {
						if (bound.Kind != BoundKind.Exact)
							bound = cbound;

						continue;
					}

					if (bound.Kind == BoundKind.Exact || cbound.Kind == BoundKind.Exact) {
						if (cbound.Kind == BoundKind.Lower) {
							if (!Convert.ImplicitConversionExists (ec, new TypeExpression (cbound.Type, Location.Null), bound.Type)) {
								break;
							}

							continue;
						}
						if (cbound.Kind == BoundKind.Upper) {
							if (!Convert.ImplicitConversionExists (ec, new TypeExpression (bound.Type, Location.Null), cbound.Type)) {
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
						if (cbound.Kind == BoundKind.Lower) {
							if (!Convert.ImplicitConversionExists (ec, new TypeExpression (cbound.Type, Location.Null), bound.Type)) {
								break;
							}
						} else {
							if (!Convert.ImplicitConversionExists (ec, new TypeExpression (bound.Type, Location.Null), cbound.Type)) {
								break;
							}

							bound = cbound;
						}

						continue;
					}

					if (bound.Kind == BoundKind.Upper) {
						if (!Convert.ImplicitConversionExists (ec, new TypeExpression (bound.Type, Location.Null), cbound.Type)) {
							break;
						}
					} else {
						throw new NotImplementedException ("variance conversion");
					}
				}

				if (cii != candidates_count)
					continue;

				//
				// We already have the best candidate, break if thet are different
				//
				// Dynamic is never ambiguous as we prefer dynamic over other best candidate types
				//
				if (best_candidate != null) {

					if (best_candidate == InternalType.Dynamic)
						continue;

					if (bound.Type != InternalType.Dynamic && best_candidate != bound.Type)
						return false;
				}

				best_candidate = bound.Type;
			}

			if (best_candidate == null)
				return false;

			unfixed_types[i] = null;
			fixed_types[i] = best_candidate;
			return true;
		}
		
		//
		// Uses inferred or partially infered types to inflate delegate type argument. Returns
		// null when type parameter was not yet inferres
		//
		public TypeSpec InflateGenericArgument (TypeSpec parameter)
		{
			var tp = parameter as TypeParameterSpec;
			if (tp != null) {
				//
				// Type inference work on generic arguments (MVAR) only
				//
				if (!tp.IsMethodOwned)
					return parameter;

				return fixed_types [tp.DeclaredPosition] ?? parameter;
			}

			var gt = parameter as InflatedTypeSpec;
			if (gt != null) {
				var inflated_targs = new TypeSpec [gt.TypeArguments.Length];
				for (int ii = 0; ii < inflated_targs.Length; ++ii) {
					var inflated = InflateGenericArgument (gt.TypeArguments [ii]);
					if (inflated == null)
						return null;

					inflated_targs[ii] = inflated;
				}

				return gt.GetDefinition ().MakeGenericType (inflated_targs);
			}

			return parameter;
		}
		
		//
		// Tests whether all delegate input arguments are fixed and generic output type
		// requires output type inference 
		//
		public bool IsReturnTypeNonDependent (ResolveContext ec, MethodSpec invoke, TypeSpec returnType)
		{
			if (returnType.IsGenericParameter) {
				if (IsFixed (returnType))
				    return false;
			} else if (TypeManager.IsGenericType (returnType)) {
				if (TypeManager.IsDelegateType (returnType)) {
					invoke = Delegate.GetInvokeMethod (ec.Compiler, returnType);
					return IsReturnTypeNonDependent (ec, invoke, invoke.ReturnType);
				}
					
				TypeSpec[] g_args = TypeManager.GetTypeArguments (returnType);
				
				// At least one unfixed return type has to exist 
				if (AllTypesAreFixed (g_args))
					return false;
			} else {
				return false;
			}

			// All generic input arguments have to be fixed
			AParametersCollection d_parameters = invoke.Parameters;
			return AllTypesAreFixed (d_parameters.Types);
		}
		
		bool IsFixed (TypeSpec type)
		{
			return IsUnfixed (type) == -1;
		}		

		int IsUnfixed (TypeSpec type)
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
		public int LowerBoundInference (TypeSpec u, TypeSpec v)
		{
			return LowerBoundInference (u, v, false);
		}

		//
		// Lower-bound (false) or Upper-bound (true) inference based on inversed argument
		//
		int LowerBoundInference (TypeSpec u, TypeSpec v, bool inversed)
		{
			// If V is one of the unfixed type arguments
			int pos = IsUnfixed (v);
			if (pos != -1) {
				AddToBounds (new BoundInfo (u, inversed ? BoundKind.Upper : BoundKind.Lower), pos);
				return 1;
			}			

			// If U is an array type
			var u_ac = u as ArrayContainer;
			if (u_ac != null) {
				var v_ac = v as ArrayContainer;
				if (v_ac != null) {
					if (u_ac.Rank != v_ac.Rank)
						return 0;

					if (TypeManager.IsValueType (u_ac.Element))
						return ExactInference (u_ac.Element, v_ac.Element);

					return LowerBoundInference (u_ac.Element, v_ac.Element, inversed);
				}

				if (u_ac.Rank != 1)
					return 0;

				if (TypeManager.IsGenericType (v)) {
					TypeSpec g_v = v.GetDefinition ();
					if (g_v != TypeManager.generic_ilist_type &&
						g_v != TypeManager.generic_icollection_type &&
						g_v != TypeManager.generic_ienumerable_type)
						return 0;

					var v_i = TypeManager.GetTypeArguments (v) [0];
					if (TypeManager.IsValueType (u_ac.Element))
						return ExactInference (u_ac.Element, v_i);

					return LowerBoundInference (u_ac.Element, v_i);
				}
			} else if (TypeManager.IsGenericType (v)) {
				//
				// if V is a constructed type C<V1..Vk> and there is a unique type C<U1..Uk>
				// such that U is identical to, inherits from (directly or indirectly),
				// or implements (directly or indirectly) C<U1..Uk>
				//
				var u_candidates = new List<TypeSpec> ();
				var open_v = v.MemberDefinition;

				for (TypeSpec t = u; t != null; t = t.BaseType) {
					if (open_v == t.MemberDefinition)
						u_candidates.Add (t);

					if (t.Interfaces != null) {
						foreach (var iface in t.Interfaces) {
							if (open_v == iface.MemberDefinition)
								u_candidates.Add (iface);
						}
					}
				}

				TypeSpec [] unique_candidate_targs = null;
				TypeSpec[] ga_v = TypeManager.GetTypeArguments (v);
				foreach (TypeSpec u_candidate in u_candidates) {
					//
					// The unique set of types U1..Uk means that if we have an interface I<T>,
					// class U : I<int>, I<long> then no type inference is made when inferring
					// type I<T> by applying type U because T could be int or long
					//
					if (unique_candidate_targs != null) {
						TypeSpec[] second_unique_candidate_targs = TypeManager.GetTypeArguments (u_candidate);
						if (TypeSpecComparer.Equals (unique_candidate_targs, second_unique_candidate_targs)) {
							unique_candidate_targs = second_unique_candidate_targs;
							continue;
						}

						//
						// This should always cause type inference failure
						//
						failed = true;
						return 1;
					}

					unique_candidate_targs = TypeManager.GetTypeArguments (u_candidate);
				}

				if (unique_candidate_targs != null) {
					var ga_open_v = open_v.TypeParameters;
					int score = 0;
					for (int i = 0; i < unique_candidate_targs.Length; ++i) {
						Variance variance = ga_open_v [i].Variance;

						TypeSpec u_i = unique_candidate_targs [i];
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
		public int OutputTypeInference (ResolveContext ec, Expression e, TypeSpec t)
		{
			// If e is a lambda or anonymous method with inferred return type
			AnonymousMethodExpression ame = e as AnonymousMethodExpression;
			if (ame != null) {
				TypeSpec rt = ame.InferReturnType (ec, this, t);
				var invoke = Delegate.GetInvokeMethod (ec.Compiler, t);

				if (rt == null) {
					AParametersCollection pd = invoke.Parameters;
					return ame.Parameters.Count == pd.Count ? 1 : 0;
				}

				TypeSpec rtype = invoke.ReturnType;
				return LowerBoundInference (rt, rtype) + 1;
			}

			//
			// if E is a method group and T is a delegate type or expression tree type
			// return type Tb with parameter types T1..Tk and return type Tb, and overload
			// resolution of E with the types T1..Tk yields a single method with return type U,
			// then a lower-bound inference is made from U for Tb.
			//
			if (e is MethodGroupExpr) {
				if (!TypeManager.IsDelegateType (t)) {
					if (TypeManager.expression_type == null || t.MemberDefinition != TypeManager.expression_type.MemberDefinition)
						return 0;

					t = TypeManager.GetTypeArguments (t)[0];
				}

				var invoke = Delegate.GetInvokeMethod (ec.Compiler, t);
				TypeSpec rtype = invoke.ReturnType;

				if (!rtype.IsGenericParameter && !TypeManager.IsGenericType (rtype))
					return 0;

				// LAMESPEC: Standard does not specify that all methodgroup arguments
				// has to be fixed but it does not specify how to do recursive type inference
				// either. We choose the simple option and infer return type only
				// if all delegate generic arguments are fixed.
				TypeSpec[] param_types = new TypeSpec [invoke.Parameters.Count];
				for (int i = 0; i < param_types.Length; ++i) {
					var inflated = InflateGenericArgument (invoke.Parameters.Types[i]);
					if (inflated == null)
						return 0;

					param_types[i] = inflated;
				}

				MethodGroupExpr mg = (MethodGroupExpr) e;
				Arguments args = DelegateCreation.CreateDelegateMethodArguments (invoke.Parameters, param_types, e.Location);
				mg = mg.OverloadResolve (ec, ref args, null, OverloadResolver.Restrictions.CovariantDelegate | OverloadResolver.Restrictions.ProbingOnly);
				if (mg == null)
					return 0;

				return LowerBoundInference (mg.BestCandidate.ReturnType, rtype) + 1;
			}

			//
			// if e is an expression with type U, then
			// a lower-bound inference is made from U for T
			//
			return LowerBoundInference (e.Type, t) * 2;
		}

		void RemoveDependentTypes (List<TypeSpec> types, TypeSpec returnType)
		{
			int idx = IsUnfixed (returnType);
			if (idx >= 0) {
				types [idx] = null;
				return;
			}

			if (TypeManager.IsGenericType (returnType)) {
				foreach (TypeSpec t in TypeManager.GetTypeArguments (returnType)) {
					RemoveDependentTypes (types, t);
				}
			}
		}

		public bool UnfixedVariableExists {
			get {
				if (unfixed_types == null)
					return false;

				foreach (TypeSpec ut in unfixed_types)
					if (ut != null)
						return true;
				return false;
			}
		}
	}
}
