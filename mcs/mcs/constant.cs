namespace CIR {

	using System;
	
	public class Constant : Expression {
		public string     name;
		public Expression expr;
		public string     type;

		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Constant (string type, string name, Expression expr)
		{
			this.type = type;
			this.name = name;
			this.expr = expr;
		}

		public void Reduce ()
		{
			
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
	}
}

