//
// generic.cs: Support classes for generics
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2003 Ximian, Inc.
//
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Collections;
using System.Text;
	
namespace Mono.CSharp {

	//
	// Tracks the constraints for a type parameter
	//
	public class Constraints {
		string type_parameter;
		ArrayList constraints;
		Location loc;
		
		//
		// type_parameter is the identifier, constraints is an arraylist of
		// Expressions (with types) or `true' for the constructor constraint.
		// 
		public Constraints (string type_parameter, ArrayList constraints,
				    Location loc)
		{
			this.type_parameter = type_parameter;
			this.constraints = constraints;
			this.loc = loc;
		}

		public string TypeParameter {
			get {
				return type_parameter;
			}
		}

		protected void Error (string message)
		{
			Report.Error (-218, "Invalid constraints clause for type " +
				      "parameter `{0}': {1}", type_parameter, message);
		}

		bool has_ctor_constraint;
		Type class_constraint;
		ArrayList iface_constraints;
		Type[] constraint_types;
		int num_constraints;

		public bool HasConstructorConstraint {
			get { return has_ctor_constraint; }
		}

		public Type[] Types {
			get { return constraint_types; }
		}

		public bool Resolve (TypeContainer tc)
		{
			iface_constraints = new ArrayList ();

			if (constraints == null) {
				constraint_types = new Type [0];
				return true;
			}

			foreach (object obj in constraints) {
				if (has_ctor_constraint) {
					Error ("can only use one constructor constraint and " +
					       "it must be the last constraint in the list.");
					return false;
				}

				if (obj is bool) {
					has_ctor_constraint = true;
					continue;
				}

				Expression expr = tc.ResolveTypeExpr ((Expression) obj, false, loc);
				if (expr == null)
					return false;

				Type etype = expr.Type;
				if (etype.IsInterface)
					iface_constraints.Add (etype);
				else if (class_constraint != null) {
					Error ("can have at most one class constraint.");
					return false;
				} else
					class_constraint = etype;

				num_constraints++;
			}

			constraint_types = new Type [num_constraints];
			int pos = 0;
			if (class_constraint != null)
				constraint_types [pos++] = class_constraint;
			iface_constraints.CopyTo (constraint_types, pos);

			return true;
		}
	}

	//
	// This type represents a generic type parameter
	//
	public class TypeParameter {
		string name;
		Constraints constraints;
		Location loc;
		Type type;

		public TypeParameter (string name, Constraints constraints, Location loc)
		{
			this.name = name;
			this.constraints = constraints;
			this.loc = loc;
		}

		public string Name {
			get {
				return name;
			}
		}

		public Location Location {
			get {
				return loc;
			}
		}

		public Constraints Constraints {
			get {
				return constraints;
			}
		}

		public Type Type {
			get {
				return type;
			}
		}

		public bool Resolve (TypeContainer tc)
		{
			if (constraints != null)
				return constraints.Resolve (tc);

			return true;
		}

		public Type Define (TypeBuilder tb)
		{
			if (constraints != null)
				type = tb.DefineGenericParameter (name, constraints.Types);
			else
				type = tb.DefineGenericParameter (name, new Type [0]);

			return type;
		}

		public Type DefineMethod (MethodBuilder mb)
		{
			if (constraints != null)
				type = mb.DefineGenericParameter (name, constraints.Types);
			else
				type = mb.DefineGenericParameter (name, new Type [0]);

			return type;
		}

		public override string ToString ()
		{
			return "TypeParameter[" + name + "]";
		}
	}

	//
	// This type represents a generic type parameter reference.
	//
	// These expressions are born in a fully resolved state.
	//
	public class TypeParameterExpr : TypeExpr {
		TypeParameter type_parameter;

		public string Name {
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
			: base (type_parameter.Type, loc)
		{
			this.type_parameter = type_parameter;
		}

		public override Expression ResolveAsTypeStep (EmitContext ec)
		{
			type = type_parameter.Type;

			return this;
		}

		public override string ToString ()
		{
			return "TypeParameterExpr[" + type_parameter.Name + "]";
		}

		public void Error_CannotUseAsUnmanagedType (Location loc)
		{
			Report.Error (-203, loc, "Can not use type parameter as unamanged type");
		}
	}

	public class TypeArguments {
		ArrayList args;
		Type[] atypes;
		bool has_type_args;
		
		public TypeArguments ()
		{
			args = new ArrayList ();
		}

		public void Add (Expression type)
		{
			args.Add (type);
		}

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

		public override string ToString ()
		{
			StringBuilder s = new StringBuilder ();

			int count = args.Count;
			for (int i = 0; i < count; i++){
				//
				// FIXME: Use TypeManager.CSharpname once we have the type
				//
				s.Append (args [i].ToString ());
				if (i+1 < count)
					s.Append (", ");
			}
			return s.ToString ();
		}

		public bool Resolve (EmitContext ec)
		{
			int count = args.Count;
			bool ok = true;

			atypes = new Type [count];
			
			for (int i = 0; i < count; i++){
				Expression e = ((Expression)args [i]).ResolveAsTypeTerminal (ec);
				if (e == null)
					ok = false;
				if (e is TypeParameterExpr)
					has_type_args = true;
				args [i] = e;
				atypes [i] = e.Type;
			}
			return ok;
		}
	}
	
	public class ConstructedType : Expression {
		Expression container_type;
		string name, full_name;
		TypeArguments args;
		Type[] gen_params, atypes;
		
		public ConstructedType (string name, TypeArguments args, Location l)
		{
			loc = l;
			this.container_type = container_type;
			this.name = name;
			this.args = args;
			eclass = ExprClass.Type;

			full_name = name + "<" + args.ToString () + ">";
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (args.Resolve (ec) == false)
				return null;

			//
			// Pretend there are not type parameters, until we get GetType support
			//
			return new SimpleName (name, loc).DoResolve (ec);
		}

		protected bool CheckConstraints (int index)
		{
			Type atype = args.Arguments [index];
			Type ptype = gen_params [index];

			//// FIXME
			return true;

			//
			// First, check parent class.
			//
			if ((ptype.BaseType != atype.BaseType) &&
			    !atype.BaseType.IsSubclassOf (ptype.BaseType)) {
				Report.Error (-219, loc, "Cannot create constructed type `{0}': " +
					      "type argument `{1}' must derive from `{2}'.",
					      full_name, atype, ptype.BaseType);
				return false;
			}

			//
			// Now, check the interfaces.
			//
			foreach (Type itype in ptype.GetInterfaces ()) {
				if (TypeManager.ImplementsInterface (atype, itype))
					continue;

				Report.Error (-219, loc, "Cannot create constructed type `{0}: " +
					      "type argument `{1}' must implement interface `{2}'.",
					      full_name, atype, itype);
				return false;
			}

			return true;
		}

		public override Expression ResolveAsTypeStep (EmitContext ec)
		{
			if (args.Resolve (ec) == false)
				return null;

			//
			// First, resolve the generic type.
			//
			SimpleName sn = new SimpleName (name, loc);
			Expression resolved = sn.ResolveAsTypeStep (ec);
			if (resolved == null)
				return null;

			Type gt = resolved.Type.GetGenericTypeDefinition ();
			gen_params = gt.GetGenericParameters ();
			atypes = args.Arguments;

			if (atypes.Length != gen_params.Length) {
				Report.Error (-217, loc, "Generic type `{0}' takes {1} " +
					      "type parameters, but specified {2}.", gt.Name,
					      gen_params.Length, atypes.Length);
				return null;
			}

			if (args.HasTypeArguments)
				return new TypeExpr (gt, loc);

			for (int i = 0; i < gen_params.Length; i++) {
				if (!CheckConstraints (i))
					return null;
			}

			//
			// Now bind the parameters.
			//
			Type ntype = gt.BindGenericParameters (args.Arguments);
			return new TypeExpr (ntype, loc);
		}
		
		public override void Emit (EmitContext ec)
		{
			//
			// Never reached for now
			//
			throw new Exception ("IMPLEMENT ME");
		}

		public override string ToString ()
		{
			return full_name;
		}
	}

	public class GenericMethod : DeclSpace
	{
		public GenericMethod (NamespaceEntry ns, TypeContainer parent, string name, Location l)
			: base (ns, parent, name, l)
		{ }

		public override TypeBuilder DefineType ()
		{
			throw new Exception ();
		}

		public override bool Define (TypeContainer parent)
		{
			return true;
		}

		public bool Define (MethodBuilder mb)
		{
			Type[] gen_params = new Type [TypeParameters.Length];
			for (int i = 0; i < TypeParameters.Length; i++)
				gen_params [i] = TypeParameters [i].DefineMethod (mb);

			return true;
		}

		public override bool DefineMembers (TypeContainer parent)
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
				throw new Exception ();
			}
		}
	}

	public class GenericMemberAccess : MemberAccess
	{
		TypeArguments args;

		public GenericMemberAccess (Expression expr, string id, TypeArguments args, Location loc)
			: base (expr, id, loc)
		{
			this.args = args;
		}

		public override Expression DoResolve (EmitContext ec, Expression right_side,
						      ResolveFlags flags)
		{
			Expression expr = base.DoResolve (ec, right_side, flags);
			if (expr == null)
				return null;

			MethodGroupExpr mg = expr as MethodGroupExpr;
			if (mg == null) {
				Report.Error (-220, loc, "Member `{0}' has type arguments, but did " +
					      "not resolve as a method group.", Identifier);
				return null;
			}

			Report.Debug (64, "RESOLVE GENERIC MEMBER ACCESS", expr, expr.GetType ());

			return expr;
		}
	}
}
