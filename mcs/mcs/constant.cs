namespace CIR {

	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Collections;
	
	public class Constant : Expression {
		string     name;
		Expression expr;
		string     constant_type;
		int        mod_flags;
		public readonly ArrayList  OptAttributes;

		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Constant (string constant_type, string name, Expression expr, int mod_flags, ArrayList attrs)
		{
			this.constant_type = constant_type;
			this.name = name;
			this.expr = expr;
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PRIVATE);
			OptAttributes = attrs;
		}

		public string Name {
			get {
				return name;
			}
		}

		public string ConstantType {
			get {
				return constant_type;
			}
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		public FieldAttributes FieldAttr {
			get {
				return FieldAttributes.Literal | Modifiers.FieldAttr (mod_flags) ;
			}
		}

		public override Expression Resolve (TypeContainer tc)
		{
			// FIXME: implement
			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
		       
		// <summary>
		//   Defines the constant in the @parent
		// </summary>
		public void EmitConstant (RootContext rc, TypeContainer parent)
		{
			FieldBuilder fb;
			TypeCode tc;
			Type t;
			
			t = rc.LookupType (parent, constant_type);
			if (t == null)
				return;

			tc = System.Type.GetTypeCode (t);
			
			if ((tc == TypeCode.SByte)  || (tc == TypeCode.Byte)   ||
			    (tc == TypeCode.Int16)  || (tc == TypeCode.UInt16) ||
			    (tc == TypeCode.Int32)  || (tc == TypeCode.Int64)  ||
			    (tc == TypeCode.UInt32) || (tc == TypeCode.UInt64)) {
				
			} else if ((tc == TypeCode.Double) || (tc == TypeCode.Single)) {

			} else if (tc == TypeCode.Char) {
			} else if (tc == TypeCode.Decimal) {

			} else if (t.IsSubclassOf (typeof (System.String))) {

			} else if (t.IsSubclassOf (typeof (System.Enum))) {

			} else {
				rc.Report.Error (-3, "Constant type is not valid (only system types are allowed");
				return;
			}

			fb = parent.TypeBuilder.DefineField (name, t, FieldAttr);
			
		}
	}
}


