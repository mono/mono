namespace CIR {

	using System;
	using System.Reflection;
	
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
				return FieldAttributes.Literal | Modifiers.Map (mod_flags) ;
			}
		}
	}
}


