//
// const.cs: Constant declarations.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
//

namespace Mono.CSharp {

	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Collections;


	public class Const : Expression {

		public readonly string     Name;
		public readonly string     ConstantType;
		public Expression Expr;
		public Attributes  OptAttributes;
		
		int mod_flags;
		Location Location;
		public FieldBuilder FieldBuilder;

		object ConstantValue = null;

		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Const (string constant_type, string name, Expression expr, int mod_flags,
			      Attributes attrs, Location loc)
		{
			this.ConstantType = constant_type;
			this.Name = name;
			this.Expr = expr;
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PRIVATE);
			this.Location = loc;
			OptAttributes = attrs;
		}

		public FieldAttributes FieldAttr {
			get {
				return FieldAttributes.Literal | FieldAttributes.Static |
					Modifiers.FieldAttr (mod_flags) ;
			}
		}

		public int ModFlags {
			get {
				return mod_flags;
			}
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// FIXME: implement
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Unimplemented");
		}

		void WarningNotHiding (TypeContainer parent)
		{
			Report.Warning (
				109, Location,
				"The member `" + parent.Name + "." + Name + "' does not hide an " +
				"inherited member.  The keyword new is not required");
		}
		
		/// <summary>
		///   Defines the constant in the @parent
		/// </summary>
		public void Define (TypeContainer parent)
		{
			type = parent.LookupType (ConstantType, true);

			if (type == null)
				return;
			
			if (!TypeManager.IsBuiltinType (type) && (!type.IsSubclassOf (TypeManager.enum_type))) {
				Report.Error (-3, Location, "Constant type is not valid (only system types are allowed)");
				return;
			}

			Type ptype = parent.TypeBuilder.BaseType;

			if (ptype != null) {
				MemberInfo [] mi = TypeContainer.FindMembers (ptype, MemberTypes.Field, BindingFlags.Public,
									      Type.FilterName, Name);
				
				if (mi == null || mi.Length == 0)
					if ((ModFlags & Modifiers.NEW) != 0)
						WarningNotHiding (parent);

			} else if ((ModFlags & Modifiers.NEW) != 0)
				WarningNotHiding (parent);

			FieldBuilder = parent.TypeBuilder.DefineField (Name, type, FieldAttr);

			TypeManager.RegisterConstant (FieldBuilder, this);
		}

		/// <summary>
		///  Looks up the value of a constant field. Defines it if it hasn't
		///  already been. Similar to LookupEnumValue in spirit.
		/// </summary>
		public object LookupConstantValue (EmitContext ec)
		{
			if (ConstantValue != null)
				return ConstantValue;
			
			Expr = Expr.Resolve (ec);

			if (Expr == null) {
				Report.Error (150, Location, "A constant value is expected");
				return null;
			}
			
			Expr = Expression.Reduce (ec, Expr);

			if (!(Expr is Literal)) {
				Report.Error (150, Location, "A constant value is expected");
				return null;
			}

			ConstantValue = ((Literal) Expr).GetValue ();
			
			FieldBuilder.SetConstant (ConstantValue);

			if (!TypeManager.RegisterField (FieldBuilder, ConstantValue))
				return null;

			return ConstantValue;
		}
		
		
		/// <summary>
		///  Emits the field value by evaluating the expression
		/// </summary>
		public void EmitConstant (TypeContainer parent)
		{
			if (FieldBuilder == null)
				return;
			
			EmitContext ec = new EmitContext (parent, Location, null, type, ModFlags);
			LookupConstantValue (ec);
			
			return;
		}
	}
}


