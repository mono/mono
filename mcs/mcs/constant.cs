namespace CIR {

	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	
	public class Constant : Expression {
		string     name;
		Expression expr;
		string     type;
		int        mod_flags;

		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Constant (string type, string name, Expression expr, int mod_flags)
		{
			this.type = type;
			this.name = name;
			this.expr = expr;
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PRIVATE);
		}

		public string Name {
			get {
				return name;
			}
		}

		public string ConstantType {
			get {
				return type;
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

		// <summary>
		//   Defines the constant in the @parent
		// </summary>
		public void EmitConstant (RootContext rc, TypeContainer parent)
		{
			FieldBuilder fb;
			TypeCode tc;
			Type t;
			
			t = rc.LookupType (parent, type);
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


