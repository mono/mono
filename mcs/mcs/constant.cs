namespace CIR {

	using System;
	
	public class Constant : Expression {
		public string     name;
		public Expression expr;
		public TypeRef    typeref;

		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Constant (TypeRef typeref, string name, Expression expr)
		{
			this.typeref = typeref;
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

		public Type ConstantType {
			get {
				return typeref.Type;
			}
		}

		public Expression Expr {
			get {
				return expr;
			}
		}
	}
}

