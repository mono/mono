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

		public string TypeParameter {
			get {
				return name;
			}
		}

		bool has_ctor_constraint;
		TypeExpr class_constraint;
		ArrayList iface_constraints;
		TypeExpr[] constraint_types;
		int num_constraints, first_constraint;
		Type[] types;

		public bool HasConstructorConstraint {
			get { return has_ctor_constraint; }
		}

		public bool Resolve (DeclSpace ds)
		{
			iface_constraints = new ArrayList ();

			foreach (object obj in constraints) {
				if (has_ctor_constraint) {
					Report.Error (401, loc,
						      "The new() constraint must be last.");
					return false;
				}

				if (obj is bool) {
					has_ctor_constraint = true;
					continue;
				}

				TypeExpr expr = ds.ResolveTypeExpr ((Expression) obj, false, loc);
				if (expr == null)
					return false;

				if (expr is TypeParameterExpr) {
					Report.Error (700, loc,
						      "`{0}': naked type parameters cannot " +
						      "be used as bounds", expr.Name);
					return false;
				}

				if (expr.IsInterface)
					iface_constraints.Add (expr);
				else if (class_constraint != null) {
					Report.Error (406, loc,
						      "`{0}': the class constraint for `{1}' " +
						      "must come before any other constraints.",
						      expr.Name, name);
					return false;
				} else
					class_constraint = expr;

				num_constraints++;
			}

			constraint_types = new TypeExpr [num_constraints];
			if (class_constraint != null)
				constraint_types [first_constraint++] = class_constraint;
			iface_constraints.CopyTo (constraint_types, first_constraint);

			return true;
		}

		public Type[] ResolveTypes (EmitContext ec)
		{
			types = new Type [constraint_types.Length];

			for (int i = 0; i < constraint_types.Length; i++) {
				types [i] = constraint_types [i].ResolveType (ec);
				if (types [i] == null)
					return null;

				for (int j = first_constraint; j < i; j++) {
					if (!types [j].Equals (types [i]))
						continue;

					Report.Error (405, loc,
						      "Duplicate constraint `{0}' for type " +
						      "parameter `{1}'.", types [i], name);
					return null;
				}
			}

			if (class_constraint != null) {
				if (types [0].IsSealed) {
					Report.Error (701, loc,
						      "`{0}' is not a valid bound.  Bounds " +
						      "must be interfaces or non sealed " +
						      "classes", types [0]);
					return null;
				}

				if ((types [0] == TypeManager.array_type) ||
				    (types [0] == TypeManager.delegate_type) ||
				    (types [0] == TypeManager.enum_type) ||
				    (types [0] == TypeManager.value_type) ||
				    (types [0] == TypeManager.object_type)) {
					Report.Error (702, loc,
						      "Bound cannot be special class `{0}'",
						      types [0]);
					return null;
				}
			}

			return types;
		}

		bool GenericConstraints.HasConstructor {
			get {
				return has_ctor_constraint;
			}
		}

		Type[] GenericConstraints.Types {
			get {
				if (types == null)
					throw new InvalidOperationException ();

				return types;
			}
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

		public bool HasConstructorConstraint {
			get {
				if (constraints != null)
					return constraints.HasConstructorConstraint;

				return false;
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
			TypeManager.AddTypeParameter (type, this);
			return type;
		}

		public Type DefineMethod (MethodBuilder mb)
		{
			type = mb.DefineGenericParameter (name);
			TypeManager.AddTypeParameter (type, this);
			return type;
		}

		public bool DefineType (EmitContext ec, TypeBuilder tb)
		{
			int index = type.GenericParameterPosition;
			if (constraints == null)
				tb.SetGenericParameterConstraints (index, new Type [0], false);
			else {
				Type[] types = constraints.ResolveTypes (ec);
				if (types == null)
					return false;

				tb.SetGenericParameterConstraints (
					index, types, constraints.HasConstructorConstraint);
			}

			return true;
		}

		public bool DefineType (EmitContext ec, MethodBuilder mb)
		{
			int index = type.GenericParameterPosition;
			if (constraints == null)
				mb.SetGenericParameterConstraints (index, new Type [0], false);
			else {
				Type[] types = constraints.ResolveTypes (ec);
				if (types == null)
					return false;

				mb.SetGenericParameterConstraints (
					index, types, constraints.HasConstructorConstraint);
			}

			return true;
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
			this.loc = loc;
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

		public string[] GetDeclarations ()
		{
			string[] ret = new string [args.Count];
			for (int i = 0; i < args.Count; i++) {
				SimpleName sn = args [i] as SimpleName;
				if (sn != null) {
					ret [i] = sn.Name;
					continue;
				}

				Report.Error (81, Location, "Type parameter declaration " +
					      "must be an identifier not a type");
				return null;
			}
			return ret;
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
					s.Append (",");
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
			: this (type_params, l)
		{
			loc = l;

			this.name = name;
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

		public ConstructedType (Type t, TypeParameter[] type_params, Location l)
			: this (type_params, l)
		{
			gt = t.GetGenericTypeDefinition ();

			this.name = gt.FullName;
			full_name = gt.FullName + "<" + args.ToString () + ">";
		}

		public ConstructedType (Type t, TypeArguments args, Location l)
			: this (args, l)
		{
			gt = t.GetGenericTypeDefinition ();

			this.name = gt.FullName;
			full_name = gt.FullName + "<" + args.ToString () + ">";
		}

		public TypeArguments TypeArguments {
			get { return args; }
		}

		protected string DeclarationName {
			get {
				StringBuilder sb = new StringBuilder ();
				sb.Append (gt.FullName);
				sb.Append ("<");
				for (int i = 0; i < gen_params.Length; i++) {
					if (i > 0)
						sb.Append (",");
					sb.Append (gen_params [i]);
				}
				sb.Append (">");
				return sb.ToString ();
			}
		}

		protected bool CheckConstraints (EmitContext ec, int index)
		{
			Type atype = atypes [index];
			Type ptype = gen_params [index];

			if (atype == ptype)
				return true;

			Expression aexpr = new EmptyExpression (atype);

			//
			// First, check the class constraint.
			//

			Type parent = ptype.BaseType;
			if ((parent != null) && (parent != TypeManager.object_type)) {
				if (!Convert.ImplicitConversionExists (ec, aexpr, parent)) {
					Report.Error (309, loc, "The type `{0}' must be " +
						      "convertible to `{1}' in order to " +
						      "use it as parameter `{2}' in the " +
						      "generic type or method `{3}'",
						      atype, parent, ptype, DeclarationName);
					return false;
				}
			}

			//
			// Now, check the interface constraints.
			//
			foreach (Type itype in ptype.GetInterfaces ()) {
				if (!Convert.ImplicitConversionExists (ec, aexpr, itype)) {
					Report.Error (309, loc, "The type `{0}' must be " +
						      "convertible to `{1}' in order to " +
						      "use it as parameter `{2}' in the " +
						      "generic type or method `{3}'",
						      atype, itype, ptype, DeclarationName);
					return false;
				}
			}

			//
			// Finally, check the constructor constraint.
			//

			if (!TypeManager.HasConstructorConstraint (ptype))
				return true;

			MethodGroupExpr mg = Expression.MemberLookup (
				ec, atype, ".ctor", MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance |
				BindingFlags.DeclaredOnly, loc)
				as MethodGroupExpr;

			if (atype.IsAbstract || (mg == null) || !mg.IsInstance) {
				Report.Error (310, loc, "The type `{0}' must have a public " +
					      "parameterless constructor in order to use it " +
					      "as parameter `{1}' in the generic type or " +
					      "method `{2}'", atype, ptype, DeclarationName);
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
			if (resolved == null) {
				sn = new SimpleName (name, -1, loc);
				resolved = sn.ResolveAsTypeTerminal (ec);
				if ((resolved == null) || (resolved.Type == null)) {
					Report.Error (246, loc,
						      "The type or namespace name `{0}<...>' "+
						      "could not be found", name);
					return null;
				}

				Type t = resolved.Type;
				int num_args = TypeManager.GetNumberOfTypeArguments (t);

				if (num_args == 0) {
					Report.Error (308, loc,
						      "The non-generic type `{0}' cannot " +
						      "be used with type arguments.",
						      TypeManager.CSharpName (t));
					return null;
				}

				Report.Error (305, loc,
					      "Using the generic type `{0}' " +
					      "requires {1} type arguments",
					      TypeManager.GetFullName (t), num_args);
				return null;
			}

			if (resolved.Type == null)
				throw new InternalErrorException (
					"Failed to resolve constructed type `{0}'",
					full_name);

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
				if (!CheckConstraints (ec, i))
					return null;
			}

			//
			// Now bind the parameters.
			//
			type = gt.BindGenericParameters (atypes);
			return type;
		}

		public Expression GetMemberAccess (EmitContext ec)
		{
			TypeExpr current;
			if (ec.TypeContainer.CurrentType != null)
				current = ec.TypeContainer.CurrentType;
			else
				current = new TypeExpression (ec.ContainerType, loc);

			return new GenericMemberAccess (current, name, args, loc);
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

		public override bool IsAttribute {
			get { return false; }
		}

		public override TypeExpr[] GetInterfaces ()
		{
			TypeExpr[] ifaces = TypeManager.GetInterfaces (gt);
			return ifaces;
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
			for (int i = 0; i < TypeParameters.Length; i++)
				if (!TypeParameters [i].Resolve (parent))
					return false;

			return true;
		}

		public bool Define (TypeContainer parent, MethodBuilder mb)
		{
			if (!Define (parent))
				return false;

			Type[] gen_params = new Type [TypeParameters.Length];
			for (int i = 0; i < TypeParameters.Length; i++)
				gen_params [i] = TypeParameters [i].DefineMethod (mb);

			return true;
		}

		public bool DefineType (EmitContext ec, MethodBuilder mb)
		{
			for (int i = 0; i < TypeParameters.Length; i++)
				if (!TypeParameters [i].DefineType (ec, mb))
					return false;

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

		private bool DoResolveBase (EmitContext ec)
		{
			ConstructedType cexpr = expr as ConstructedType;
			if (cexpr != null) {
				TypeArguments new_args = new TypeArguments (loc);
				new_args.Add (cexpr.TypeArguments);
				new_args.Add (args);

				args = new_args;
			}

			return true;
		}

		public override Expression DoResolve (EmitContext ec, Expression right_side,
						      ResolveFlags flags)
		{
			if (!DoResolveBase (ec))
				return null;

			Expression expr = base.DoResolve (ec, right_side, flags);
			if (expr == null)
				return null;

			TypeExpr texpr = expr as TypeExpr;
			if (texpr != null) {
				Type t = texpr.ResolveType (ec);
				if (t == null)
					return null;

				ConstructedType ctype = new ConstructedType (t, args, loc);
				return ctype.DoResolve (ec);
			}

			MethodGroupExpr mg = expr as MethodGroupExpr;
			if (mg == null) {
				return expr;
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

				Type[] gen_params = mi.GetGenericParameters ();
			
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
			new_mg.HasTypeArguments = true;
			return new_mg;
		}

		public override Expression ResolveAsTypeStep (EmitContext ec)
		{
			if (!DoResolveBase (ec))
				return null;

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
