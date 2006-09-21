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
	public abstract class GenericConstraints
	{
		public abstract string TypeParameter {
			get;
		}
	}

	public abstract class Constraints : GenericConstraints
	{
		public abstract Location Location {
			get;
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

		public override string DocCommentHeader {
			get { throw new NotImplementedException (); }
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

		MemberCache IMemberContainer.MemberCache {
			get { throw new NotImplementedException (); }
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

	public abstract class GenericMethod : DeclSpace
	{
		public GenericMethod (NamespaceEntry ns, DeclSpace parent, MemberName name,
				      Expression return_type, Parameters parameters)
			: base (ns, parent, name, null)
		{
			throw new NotImplementedException ();
		}
	}

	public abstract class TypeArguments
	{
		public int Count {
			get { throw new NotImplementedException (); }
		}

		public bool IsUnbound {
			get { throw new NotImplementedException (); }
		}

		public TypeParameterName[] GetDeclarations ()
		{
			throw new NotImplementedException ();
		}
	}
}

namespace System.Reflection.Emit {
	// GRIEVOUS HACK
	abstract class GenericTypeParameterBuilder : Type {
	}
}
