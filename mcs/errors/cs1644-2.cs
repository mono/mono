// syntax error, got token `PROTECTED', expecting SET CLOSE_BRACE OPEN_BRACKET
// Line: 13
// Compiler options: -langversion:ISO-1

class Class {

	public int Count {

		get {
			return 0;
		}

		protected set {
		}
	}
}
