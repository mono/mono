//
// constant.cs: Constant expressions and constant folding.
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


	public class Constant : Expression {

		public readonly string     Name;
		public Expression Expr;
		public readonly string     ConstantType;
		public Attributes  OptAttributes;
		
		int        mod_flags;

		Location Location;
		FieldBuilder FieldBuilder;

		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Constant (string constant_type, string name, Expression expr, int mod_flags,
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
		       
		/// <summary>
		///   Defines the constant in the @parent
		/// </summary>
		public void Define (TypeContainer parent)
		{
			type = parent.LookupType (ConstantType, true);

			if (type == null)
				return;
			
			if (!TypeManager.IsBuiltinType (type) && (!type.IsSubclassOf (TypeManager.enum_type))) {
				Report.Error (-3, "Constant type is not valid (only system types are allowed)");
				return;
			}
			
			FieldBuilder = parent.TypeBuilder.DefineField (Name, type, FieldAttr);
			
		}

		/// <summary>
		///  Emits the field value by evaluating the expression
		/// </summary>
		public void EmitConstant (TypeContainer parent)
		{
			if (FieldBuilder == null)
				return;
			
			EmitContext ec = new EmitContext (parent, null, type, ModFlags);

			Expr = Expression.Reduce (ec, Expr);

			if (!(Expr is Literal)) {
				Report.Error (150, Location, "A constant value is expected");
				return;
			}

			object val = ((Literal) Expr).GetValue ();

			FieldBuilder.SetConstant (val);

			TypeManager.RegisterField (FieldBuilder, val);
			
			return;
		}
	}
}


