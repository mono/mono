//
// generic.cs: Support classes for generics to reduce differences from GMCS
//
// Author:
//   Raja R Harinath <rharinath@novell.com>
//
// Copyright 2006 Novell, Inc.
//
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;

namespace Mono.CSharp
{
	public enum Variance
	{
		None,
		Covariant,
		Contravariant
	}

	public enum SpecialConstraint
	{
		Constructor,
		ReferenceType,
		ValueType
	}

	public abstract class GenericTypeParameterBuilder : Type
	{
	}

	public class InternalsVisibleToAttribute
	{
		public string AssemblyName;
	}

	class ConstraintChecker
	{
		public static bool CheckConstraints (ResolveContext ec, MethodBase a, MethodBase b, Location loc)
		{
			throw new NotSupportedException ();
		}
	}
	
	public abstract class GenericConstraints
	{
		public bool HasConstructorConstraint {
			get { throw new NotImplementedException (); }
		}

		public bool HasValueTypeConstraint {
			get { throw new NotImplementedException (); }
		}

		public bool HasClassConstraint {
			get { throw new NotImplementedException (); }
		}

		public bool HasReferenceTypeConstraint {
			get { throw new NotImplementedException (); }
		}
			
		public abstract string TypeParameter {
			get;
		}

		public bool IsReferenceType { 
			get { throw new NotSupportedException (); }
		}
		
		public bool IsValueType { 
			get { throw new NotSupportedException (); }
		}

		public Type[] InterfaceConstraints {
			get { throw new NotSupportedException (); }
		}

		public Type ClassConstraint {
			get { throw new NotSupportedException (); }
		}

		public Type EffectiveBaseClass {
			get { throw new NotSupportedException (); }
		}
	}

	public class Constraints : GenericConstraints
	{
		public Constraints (string name, ArrayList constraints, Location loc)
		{
		}
		
		public Constraints Clone ()
		{
			throw new NotImplementedException ();
		}
		
		public Location Location {
			get { return Location.Null; } 
		}
		
		public override string TypeParameter {
			get { throw new NotImplementedException (); }
		}		

		public void VerifyClsCompliance (Report r)
		{
		}
	}

	public class TypeParameter : MemberCore, IMemberContainer
	{
		public TypeParameter (DeclSpace parent, DeclSpace decl, string name,
				      Constraints constraints, Attributes attrs, Variance variance, Location loc)
			: base (parent, new MemberName (name, loc), attrs)
		{
			throw new NotImplementedException ();
		}

		public static string GetSignatureForError (TypeParameter[] tp)
		{
			throw new NotImplementedException ();
		}
		
		public void ErrorInvalidVariance (MemberCore mc, Variance v)
		{
		}
		
		public static TypeParameter FindTypeParameter (TypeParameter[] all, string name)
		{
			throw new NotImplementedException ();
		}

		//
		// MemberContainer
		//

		public override bool Define ()
		{
			return true;
		}

		public void Define (Type t)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			throw new NotImplementedException ();
		}

		public override AttributeTargets AttributeTargets {
			get { throw new NotImplementedException (); }
		}

		public override string[] ValidAttributeTargets {
			get {
				return new string [] { "type parameter" };
			}
		}

		public Constraints Constraints {
			get {
				return null;
			}
		}

		public override string DocCommentHeader {
			get { throw new NotImplementedException (); }
		}

		public bool Resolve (DeclSpace ds)
		{
			throw new NotImplementedException ();
		}

		public bool DefineType (IMemberContext ec)
		{
			throw new NotImplementedException ();
		}

		public bool DefineType (IMemberContext ec, MethodBuilder builder,
					MethodInfo implementing, bool is_override)
		{
			throw new NotImplementedException ();
		}

		public bool CheckDependencies ()
		{
			throw new NotImplementedException ();
		}

		public bool UpdateConstraints (IMemberContext ec, Constraints new_constraints)
		{
			throw new NotImplementedException ();
		}

		//
		// IMemberContainer
		//

		public Type Type {
			get { throw new NotImplementedException (); }
		}

		string IMemberContainer.Name {
			get { throw new NotImplementedException (); }
		}

		public Variance Variance {
			get { throw new NotImplementedException (); }
		}

		MemberCache IMemberContainer.BaseCache {
			get { throw new NotImplementedException (); }
		}

		bool IMemberContainer.IsInterface {
			get { throw new NotImplementedException (); }
		}

		MemberList IMemberContainer.GetMembers (MemberTypes mt, BindingFlags bf)
		{
			throw new NotImplementedException ();
		}

		public bool IsSubclassOf (Type t)
		{
			throw new NotImplementedException ();
		}

		public MemberList FindMembers (MemberTypes mt, BindingFlags bf,
					       MemberFilter filter, object criteria)
		{
			throw new NotImplementedException ();
		}

		public void SetConstraints (GenericTypeParameterBuilder type)
		{
			throw new NotImplementedException ();
		}
	}

	public class TypeParameterExpr : TypeExpr
	{
		public TypeParameterExpr (TypeParameter type_parameter, Location loc)
		{
			throw new NotImplementedException ();
		}

		protected override TypeExpr DoResolveAsTypeStep (IMemberContext ec)
		{
			return null;
		}

		public TypeParameter TypeParameter {
			get {
				throw new NotImplementedException ();
			}
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

	public class GenericTypeExpr : TypeExpr
	{
		public GenericTypeExpr (DeclSpace t, Location l)
		{
			throw new NotImplementedException ();
		}

		public GenericTypeExpr (Type t, TypeArguments args, Location l)
		{
			throw new NotImplementedException ();
		}

		protected override TypeExpr DoResolveAsTypeStep (IMemberContext ec)
		{
			throw new NotImplementedException ();
		}

		public bool CheckConstraints (IMemberContext ec)
		{
			throw new NotImplementedException ();
		}

		public TypeArguments TypeArguments {
			get { throw new NotImplementedException (); }
		}

		public bool VerifyVariantTypeParameters (IMemberContext rc)
		{
			throw new NotImplementedException ();
		}
	}

	public class GenericMethod : DeclSpace
	{
		public GenericMethod (NamespaceEntry ns, DeclSpace parent, MemberName name,
				      Expression return_type, ParametersCompiled parameters)
			: base (ns, parent, name, null)
		{
			throw new NotImplementedException ();
		}

		public override TypeBuilder DefineType ()
		{
			throw new NotImplementedException ();
		}

		public override bool Define ()
		{
			throw new NotImplementedException ();
		}

		public bool DefineType (IMemberContext ec, MethodBuilder mb,
					MethodInfo implementing, bool is_override)
		{
			throw new NotImplementedException ();
		}

		public void EmitAttributes ()
		{
			throw new NotImplementedException ();
		}

		internal static void Error_ParameterNameCollision (Location loc, string name, string collisionWith)
		{
		}

		public override MemberCache MemberCache {
			get { throw new NotImplementedException (); }
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
			throw new NotImplementedException ();
		}
	}

	public class TypeArguments
	{
		ArrayList args;
		
		public TypeArguments ()
		{
			args = new ArrayList ();
		}

		public TypeArguments (params Expression[] types)
		{
			args = new ArrayList (types);
		}
		
		public void Add (Expression type)
		{
		}

		public void Add (TypeArguments new_args)
		{
		}

		public bool Resolve (IMemberContext ec)
		{
			throw new NotImplementedException ();
		}

		public Type[] Arguments {
			get { throw new NotImplementedException (); }
		}

		public int Count {
			get {
				return args.Count;
			}
		}

		public TypeParameterName[] GetDeclarations ()
		{
			throw new NotImplementedException ();
		}
		
		public string GetSignatureForError ()
		{
			throw new NotImplementedException ();
		}

		public TypeArguments Clone ()
		{
			throw new NotImplementedException ();
		}
	}

	public class TypeInferenceContext
	{
		public Type[] InferredTypeArguments;
		
		public void AddCommonTypeBound (Type type)
		{
			throw new NotImplementedException ();
		}
		
		public void ExactInference (Type u, Type v)
		{
			throw new NotImplementedException ();
		}
		
		public Type InflateGenericArgument (Type t)
		{
			throw new NotImplementedException ();		
		}
		
		public bool FixAllTypes (ResolveContext ec)
		{
			return false;
		}
	}
	
	partial class TypeManager
	{
		public static Variance CheckTypeVariance (Type type, Variance v, IMemberContext mc)
		{
			return v;
		}
		
		public static bool IsVariantOf (Type a, Type b)
		{
			return false;
		}
		
		public static TypeContainer LookupGenericTypeContainer (Type t)
		{
			throw new NotImplementedException ();
		}
	}
}
