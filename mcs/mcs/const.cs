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


	public class Const : MemberCore {
		public readonly string ConstantType;
		public Expression Expr;
		public Attributes  OptAttributes;
		public FieldBuilder FieldBuilder;

		object ConstantValue = null;
		Type type;

		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Const (string constant_type, string name, Expression expr, int mod_flags,
			      Attributes attrs, Location loc)
			: base (name, loc)
		{
			ConstantType = constant_type;
			Name = name;
			Expr = expr;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PRIVATE);
			OptAttributes = attrs;
		}

		public FieldAttributes FieldAttr {
			get {
				return FieldAttributes.Literal | FieldAttributes.Static |
					Modifiers.FieldAttr (ModFlags) ;
			}
		}

		/// <summary>
		///   Defines the constant in the @parent
		/// </summary>
		public override bool Define (TypeContainer parent)
		{
			type = RootContext.LookupType (parent, ConstantType, true, Location);

			if (type == null)
				return false;
			
			if (!TypeManager.IsBuiltinType (type) &&
			    (!type.IsSubclassOf (TypeManager.enum_type))) {
				Report.Error (
					-3, Location,
					"Constant type is not valid (only system types are allowed)");
				return false;
			}

			Type ptype = parent.TypeBuilder.BaseType;

			if (ptype != null) {
				MemberInfo [] mi = TypeContainer.FindMembers (
					ptype, MemberTypes.Field, BindingFlags.Public,
					Type.FilterName, Name);
				
				if (mi == null || mi.Length == 0)
					if ((ModFlags & Modifiers.NEW) != 0)
						WarningNotHiding (parent);

			} else if ((ModFlags & Modifiers.NEW) != 0)
				WarningNotHiding (parent);

			FieldBuilder = parent.TypeBuilder.DefineField (Name, type, FieldAttr);

			TypeManager.RegisterConstant (FieldBuilder, this);

			return true;
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


