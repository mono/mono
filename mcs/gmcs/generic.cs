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
		int num_constraints, first_constraint;
		Type class_constraint_type;
		Type[] iface_constraint_types;

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

			return true;
		}

		public TypeExpr[] InterfaceConstraints {
			get {
				TypeExpr[] ifaces = new TypeExpr [iface_constraints.Count];
				iface_constraints.CopyTo (ifaces, 0);
				return ifaces;
			}
		}

		public bool ResolveTypes (EmitContext ec)
		{
			iface_constraint_types = new Type [iface_constraints.Count];

			for (int i = 0; i < iface_constraints.Count; i++) {
				TypeExpr iface_constraint = (TypeExpr) iface_constraints [i];
				Type resolved = iface_constraint.ResolveType (ec);
				if (resolved == null)
					return false;

				for (int j = 0; j < i; j++) {
					if (!iface_constraint_types [j].Equals (resolved))
						continue;

					Report.Error (405, loc,
						      "Duplicate constraint `{0}' for type " +
						      "parameter `{1}'.", resolved, name);
					return false;
				}

				iface_constraint_types [i] = resolved;
			}

			if (class_constraint != null) {
				class_constraint_type = class_constraint.ResolveType (ec);
				if (class_constraint_type == null)
					return false;

				if (class_constraint_type.IsSealed) {
					Report.Error (701, loc,
						      "`{0}' is not a valid bound.  Bounds " +
						      "must be interfaces or non sealed " +
						      "classes", class_constraint_type);
					return false;
				}

				if ((class_constraint_type == TypeManager.array_type) ||
				    (class_constraint_type == TypeManager.delegate_type) ||
				    (class_constraint_type == TypeManager.enum_type) ||
				    (class_constraint_type == TypeManager.value_type) ||
				    (class_constraint_type == TypeManager.object_type)) {
					Report.Error (702, loc,
						      "Bound cannot be special class `{0}'",
						      class_constraint_type);
					return false;
				}
			}

			return true;
		}

		public void Define (GenericTypeParameterBuilder type)
		{
			if (has_ctor_constraint)
				type.Mono_SetConstructorConstraint ();
		}

		bool GenericConstraints.HasConstructor {
			get { return has_ctor_constraint; }
		}

		bool GenericConstraints.HasClassConstraint {
			get { return class_constraint_type != null; }
		}

		Type GenericConstraints.ClassConstraint {
			get { return class_constraint_type; }
		}

		Type[] GenericConstraints.InterfaceConstraints {
			get { return iface_constraint_types; }
		}
	}

	//
	// This type represents a generic type parameter
	//
	public class TypeParameter : IMemberContainer {
		string name;
		Constraints constraints;
		Location loc;
		GenericTypeParameterBuilder type;

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

		public void Define (GenericTypeParameterBuilder type)
		{
			this.type = type;
			TypeExpr[] ifaces = null;
			if (constraints != null) {
				ifaces = constraints.InterfaceConstraints;
				constraints.Define (type);
			}
			TypeManager.AddTypeParameter (type, this, ifaces);
		}

		public bool DefineType (EmitContext ec)
		{
			if (constraints != null) {
				if (!constraints.ResolveTypes (ec))
					return false;

				GenericConstraints gc = (GenericConstraints) constraints;

				if (gc.HasClassConstraint)
					type.SetBaseTypeConstraint (gc.ClassConstraint);

				type.SetInterfaceConstraints (gc.InterfaceConstraints);
			}

			return true;
		}

		//
		// IMemberContainer
		//

		IMemberContainer IMemberContainer.Parent {
			get { return null; }
		}

		bool IMemberContainer.IsInterface {
			get { return true; }
		}

		MemberList IMemberContainer.GetMembers (MemberTypes mt, BindingFlags bf)
		{
			return FindMembers (mt, bf, null, null);
		}

		MemberCache IMemberContainer.MemberCache {
			get { return null; }
		}

		public MemberList FindMembers (MemberTypes mt, BindingFlags bf,
					       MemberFilter filter, object criteria)
		{
			if (constraints == null)
				return MemberList.Empty;

			ArrayList members = new ArrayList ();

			GenericConstraints gc = (GenericConstraints) constraints;

			if (gc.HasClassConstraint) {
				MemberList list = TypeManager.FindMembers (
					gc.ClassConstraint, mt, bf, filter, criteria);

				members.AddRange (list);
			}

			foreach (Type t in gc.InterfaceConstraints) {
				MemberList list = TypeManager.FindMembers (
					t, mt, bf, filter, criteria);

				members.AddRange (list);
			}

			return new MemberList (members);
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
			this.name = name + "!" + args.Count;
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
				if (!Convert.ImplicitStandardConversionExists (aexpr, parent)) {
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
				if (!Convert.ImplicitStandardConversionExists (aexpr, itype)) {
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

			Type t;
			int num_args;

			SimpleName sn = new SimpleName (name, loc);
			TypeExpr resolved = sn.ResolveAsTypeTerminal (ec);
			if ((resolved == null) || (resolved.Type == null)) {
				Report.Error (246, loc,
					      "The type or namespace name `{0}<...>' "+
					      "could not be found", Basename);
				return null;
			}

			t = resolved.Type;
			if (t == null) {
				Report.Error (246, loc, "Cannot find type `{0}'<...>",
					      Basename);
				return null;
			}

			num_args = TypeManager.GetNumberOfTypeArguments (t);
			if (num_args == 0) {
				Report.Error (308, loc,
					      "The non-generic type `{0}' cannot " +
					      "be used with type arguments.",
					      TypeManager.CSharpName (t));
				return null;
			}

			gt = t.GetGenericTypeDefinition ();
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
				Report.Error (305, loc,
					      "Using the generic type `{0}' " +
					      "requires {1} type arguments",
					      TypeManager.GetFullName (gt),
					      gen_params.Length);
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

			return new MemberAccess (current, Basename, args, loc);
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

		public string Basename {
			get {
				int pos = name.LastIndexOf ('!');
				if (pos >= 0)
					return name.Substring (0, pos);
				else
					return name;
			}
		}

		public override string Name {
			get {
				return full_name;
			}
		}
	}

	public class GenericMethod : DeclSpace
	{
		public GenericMethod (NamespaceEntry ns, TypeContainer parent,
				      MemberName name, Attributes attrs, Location l)
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

			GenericTypeParameterBuilder[] gen_params;
			gen_params = mb.DefineGenericParameters (MemberName.TypeParameters);
			for (int i = 0; i < TypeParameters.Length; i++)
				TypeParameters [i].Define (gen_params [i]);

			return true;
		}

		public bool DefineType (EmitContext ec, MethodBuilder mb)
		{
			for (int i = 0; i < TypeParameters.Length; i++)
				if (!TypeParameters [i].DefineType (ec))
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
