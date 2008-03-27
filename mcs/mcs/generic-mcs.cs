//
// generic.cs: Support classes for generics to reduce differences from GMCS
//
// Author:
//   Raja R Harinath <rharinath@novell.com>
//
// (C) 2006 Novell, Inc.
//
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;

namespace Mono.CSharp
{
	public enum SpecialConstraint
	{
		Constructor,
		ReferenceType,
		ValueType
	}
	
	public abstract class GenericConstraints
	{
		public bool HasValueTypeConstraint {
			get {
				throw new NotImplementedException ();
			}
		}
			
		public abstract string TypeParameter {
			get;
		}

		public bool IsReferenceType { 
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

		public void VerifyClsCompliance ()
		{
		}
	}

	public class TypeParameter : MemberCore, IMemberContainer
	{
		public TypeParameter (DeclSpace parent, DeclSpace decl, string name,
				      Constraints constraints, Attributes attrs, Location loc)
			: base (parent, new MemberName (name, loc), attrs)
		{
			throw new NotImplementedException ();
		}

		public static string GetSignatureForError (TypeParameter[] tp)
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

		public override void ApplyAttributeBuilder (Attribute a,
							    CustomAttributeBuilder cb)
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

		public bool DefineType (IResolveContext ec)
		{
			throw new NotImplementedException ();
		}

		public bool DefineType (IResolveContext ec, MethodBuilder builder,
					MethodInfo implementing, bool is_override)
		{
			throw new NotImplementedException ();
		}

		public bool CheckDependencies ()
		{
			throw new NotImplementedException ();
		}

		public bool UpdateConstraints (IResolveContext ec, Constraints new_constraints)
		{
			throw new NotImplementedException ();
		}

		//
		// IMemberContainer
		//

		Type IMemberContainer.Type {
			get { throw new NotImplementedException (); }
		}

		string IMemberContainer.Name {
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
	}

	public class TypeParameterExpr : TypeExpr
	{
		public override string Name {
			get { throw new NotImplementedException (); }
		}

		public override string FullName {
			get { throw new NotImplementedException (); }
		}

		public TypeParameterExpr (TypeParameter type_parameter, Location loc)
		{
			throw new NotImplementedException ();
		}

		protected override TypeExpr DoResolveAsTypeStep (IResolveContext ec)
		{
			return null;
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

	public class ConstructedType : TypeExpr
	{
		public ConstructedType (FullNamedExpression fname, TypeArguments args, Location l)
		{
			throw new NotImplementedException ();
		}

		public ConstructedType (Type t, TypeParameter[] type_params, Location l)
		{
			throw new NotImplementedException ();
		}

		public ConstructedType (Type t, TypeArguments args, Location l)
		{
			throw new NotImplementedException ();
		}

		public override string Name {
			get { throw new NotImplementedException (); }
		}

		public override string FullName {
			get { throw new NotImplementedException (); }
		}

		protected override TypeExpr DoResolveAsTypeStep (IResolveContext ec)
		{
			throw new NotImplementedException ();
		}

		public bool CheckConstraints (IResolveContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	public class GenericMethod : DeclSpace
	{
		public GenericMethod (NamespaceEntry ns, DeclSpace parent, MemberName name,
				      Expression return_type, Parameters parameters)
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

		public bool DefineType (EmitContext ec, MethodBuilder mb,
					MethodInfo implementing, bool is_override)
		{
			throw new NotImplementedException ();
		}

		public void EmitAttributes ()
		{
			throw new NotImplementedException ();
		}

		public override bool DefineMembers ()
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
		public readonly Location Location;
		ArrayList args;
		//Type[] atypes;
		int dimension;
		//bool has_type_args;
		//bool created;
		
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
		}

		public void Add (TypeArguments new_args)
		{
		}

		public bool Resolve (IResolveContext ec)
		{
			throw new NotImplementedException ();
		}

		public Type[] Arguments {
			get { throw new NotImplementedException (); }
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
			get { throw new NotImplementedException (); }
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
		public void ExactInference (Type u, Type v)
		{
			throw new NotImplementedException ();
		}
		
		public Type InflateGenericArgument (Type t)
		{
			throw new NotImplementedException ();		
		}
	}
}
