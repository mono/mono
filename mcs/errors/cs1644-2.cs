// CS1644: Feature `access modifiers on properties' cannot be used because it is not part of the C# 1.0 language specification
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
