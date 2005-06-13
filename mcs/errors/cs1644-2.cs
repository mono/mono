// cs1644.cs: Feature 'access modifiers on properties' cannot be used because it is not part of the standardized ISO C# language specification
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
