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
		TypeExpr class_constraint;
		ArrayList iface_constraints;
		TypeExpr[] constraint_types;
		int num_constraints;

		public bool HasConstructorConstraint {
			get { return has_ctor_constraint; }
		}

		public bool Resolve (DeclSpace ds)
		{
			iface_constraints = new ArrayList ();

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

				TypeExpr expr = ds.ResolveTypeExpr ((Expression) obj, false, loc);
				if (expr == null)
					return false;

				if (expr.IsInterface)
					iface_constraints.Add (expr);
				else if (class_constraint != null) {
					Error ("can have at most one class constraint.");
					return false;
				} else
					class_constraint = expr;

				num_constraints++;
			}

			constraint_types = new TypeExpr [num_constraints];
			int pos = 0;
			if (class_constraint != null)
				constraint_types [pos++] = class_constraint;
			iface_constraints.CopyTo (constraint_types, pos);

			return true;
		}

		public Type[] ResolveTypes (EmitContext ec)
		{
			Type [] types = new Type [constraint_types.Length];

			for (int i = 0; i < constraint_types.Length; i++)
				types [i] = constraint_types [i].ResolveType (ec);

			return types;
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

		public bool Resolve (DeclSpace ds)
		{
			if (constraints != null)
				return constraints.Resolve (ds);

			return true;
		}

		public Type Define (TypeBuilder tb)
		{
			type = tb.DefineGenericParameter (name);
			return type;
		}

		public Type DefineMethod (MethodBuilder mb)
		{
			type = mb.DefineGenericParameter (name);
			return type;
		}

		public void DefineType (EmitContext ec, TypeBuilder tb)
		{
			int index = type.GenericParameterPosition;
			if (constraints == null)
				tb.SetGenericParameterConstraints (index, new Type [0]);
			else
				tb.SetGenericParameterConstraints (index, constraints.ResolveTypes (ec));
		}

		public void DefineType (EmitContext ec, MethodBuilder mb)
		{
			int index = type.GenericParameterPosition;
			if (constraints == null)
				mb.SetGenericParameterConstraints (index, new Type [0]);
			else
				mb.SetGenericParameterConstraints (index, constraints.ResolveTypes (ec));
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

		public override string Name {
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
		}

		public override TypeExpr DoResolveAsTypeStep (EmitContext ec)
		{
			type = type_parameter.Type;

			return this;
		}

		public void Error_CannotUseAsUnmanagedType (Location loc)
		{
			Report.Error (-203, loc, "Can not use type parameter as unamanged type");
		}
	}

	public class TypeArguments {
		public readonly Location Location;
		ArrayList args;
		Type[] atypes;
		bool has_type_args;
		bool created;
		
		public TypeArguments (Location loc)
		{
			args = new ArrayList ();
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
				return args.Count;
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
			DeclSpace ds = ec.DeclSpace;
			int count = args.Count;
			bool ok = true;

			atypes = new Type [count];
			
			for (int i = 0; i < count; i++){
				TypeExpr te = ds.ResolveTypeExpr (
					(Expression) args [i], false, Location);
				if (te == null) {
					ok = false;
					continue;
				}
				if (te is TypeParameterExpr)
					has_type_args = true;
				atypes [i] = te.ResolveType (ec);

				if (atypes [i] == null) {
					Report.Error (246, Location, "Cannot find type `{0}'",
						      te.Name);
					ok = false;
				}
			}
			return ok;
		}
	}
	
	public class ConstructedType : TypeExpr {
		string name, full_name;
		TypeArguments args;
		Type[] gen_params, atypes;
		Type gt;
		
		public ConstructedType (string name, TypeArguments args, Location l)
		{
			loc = l;
			this.name = name;
			this.args = args;

			eclass = ExprClass.Type;
			full_name = name + "<" + args.ToString () + ">";
		}

		public ConstructedType (string name, TypeParameter[] type_params, Location l)
		{
			loc = l;
			this.name = name;

			args = new TypeArguments (l);
			foreach (TypeParameter type_param in type_params)
				args.Add (new TypeParameterExpr (type_param, l));

			eclass = ExprClass.Type;
			full_name = name + "<" + args.ToString () + ">";
		}

		public ConstructedType (Type t, TypeParameter[] type_params, Location l)
			: this (t.Name, type_params, l)
		{
			gt = t.GetGenericTypeDefinition ();
		}

		public ConstructedType (Type t, TypeArguments args, Location l)
			: this (t.Name, args, l)
		{
			gt = t.GetGenericTypeDefinition ();
		}

		public TypeArguments TypeArguments {
			get { return args; }
		}

		protected bool CheckConstraints (int index)
		{
			Type atype = atypes [index];
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

		public override TypeExpr DoResolveAsTypeStep (EmitContext ec)
		{
			if (gt != null)
				return this;

			//
			// First, resolve the generic type.
			//
			DeclSpace ds;
			Type nested = ec.DeclSpace.FindNestedType (loc, name, out ds);
			if (nested != null) {
				gt = nested.GetGenericTypeDefinition ();

				TypeArguments new_args = new TypeArguments (loc);
				foreach (TypeParameter param in ds.TypeParameters)
					new_args.Add (new TypeParameterExpr (param, loc));
				new_args.Add (args);

				args = new_args;
				return this;
			}

			SimpleName sn = new SimpleName (name, args.Count, loc);
			TypeExpr resolved = sn.ResolveAsTypeTerminal (ec);
			if (resolved == null)
				return null;

			if (resolved.Type == null) {
				Report.Error (-220, loc,
					      "Failed to resolve constructed type `{0}'",
					      full_name);
				return null;
			}

			gt = resolved.Type.GetGenericTypeDefinition ();
			return this;
		}

		public override Type ResolveType (EmitContext ec)
		{
			if (type != null)
				return type;
			if (DoResolveAsTypeStep (ec) == null)
				return null;

			//
			// Resolve the arguments.
			//
			if (args.Resolve (ec) == false)
				return null;

			gen_params = gt.GetGenericArguments ();
			atypes = args.Arguments;

			if (atypes.Length != gen_params.Length) {
				Report.Error (-217, loc, "Generic type `{0}' takes {1} " +
					      "type parameters, but specified {2}.", gt.Name,
					      gen_params.Length, atypes.Length);
				return null;
			}

			for (int i = 0; i < gen_params.Length; i++) {
				if (!CheckConstraints (i))
					return null;
			}

			//
			// Now bind the parameters.
			//
			type = gt.BindGenericParameters (atypes);
			return type;
		}

		public Expression GetMemberAccess (TypeExpr current_type)
		{
			return new GenericMemberAccess (current_type, name, args, loc);
		}

		public override bool CheckAccessLevel (DeclSpace ds)
		{
			return ds.CheckAccessLevel (gt);
		}

		public override bool AsAccessible (DeclSpace ds, int flags)
		{
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

		public override TypeExpr[] GetInterfaces ()
		{
			TypeExpr[] ifaces = TypeManager.GetInterfaces (gt);
			return ifaces;
		}

		public override string Name {
			get {
				return full_name;
			}
		}
	}

	public class GenericMethod : DeclSpace
	{
		public GenericMethod (NamespaceEntry ns, TypeContainer parent, string name,
				      Attributes attrs, Location l)
			: base (ns, parent, name, attrs, l)
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

		public bool DefineType (EmitContext ec, MethodBuilder mb)
		{
			for (int i = 0; i < TypeParameters.Length; i++)
				TypeParameters [i].DefineType (ec, mb);

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
			: base (expr, id, args.Count, loc)
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

			if (args.Resolve (ec) == false)
				return null;

			Type[] atypes = args.Arguments;

			ArrayList list = new ArrayList ();

			foreach (MethodBase method in mg.Methods) {
				MethodInfo mi = method as MethodInfo;
				if (mi == null)
					continue;

				Type[] gen_params = mi.GetGenericArguments ();
			
				if (atypes.Length != gen_params.Length) {
					Report.Error (-217, loc, "Generic method `{0}' takes {1} " +
						      "type parameters, but specified {2}.", mi.Name,
						      gen_params.Length, atypes.Length);
					continue;
				}

				list.Add (mi.BindGenericParameters (args.Arguments));
			}

			MethodInfo[] methods = new MethodInfo [list.Count];
			list.CopyTo (methods, 0);

			MethodGroupExpr new_mg = new MethodGroupExpr (methods, mg.Location);
			new_mg.InstanceExpression = mg.InstanceExpression;
			return new_mg;
		}

		public override Expression ResolveAsTypeStep (EmitContext ec)
		{
			ConstructedType cexpr = expr as ConstructedType;
			if (cexpr != null) {
				TypeArguments new_args = new TypeArguments (loc);
				new_args.Add (cexpr.TypeArguments);
				new_args.Add (args);

				args = new_args;
			}

			expr = base.ResolveAsTypeStep (ec);
			if (expr == null)
				return null;

			Type t = ((TypeExpr) expr).ResolveType (ec);
			if (t == null)
				return null;

			ConstructedType ctype = new ConstructedType (t, args, loc);
			return ctype.ResolveAsTypeStep (ec);
		}
	}

	public class DefaultValueExpression : Expression
	{
		Expression expr;
		LocalTemporary temp_storage;

		public DefaultValueExpression (Expression expr, Location loc)
		{
			this.expr = expr;
			this.loc = loc;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = ec.DeclSpace.ResolveType (expr, false, loc);
			if (type == null)
				return null;

			if (type.IsGenericParameter || TypeManager.IsValueType (type))
				temp_storage = new LocalTemporary (ec, type);

			eclass = ExprClass.Variable;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			if (temp_storage != null) {
				temp_storage.AddressOf (ec, AddressOp.LoadStore);
				ec.ig.Emit (OpCodes.Initobj, type);
				temp_storage.Emit (ec);
			} else
				ec.ig.Emit (OpCodes.Ldnull);
		}
	}
}
