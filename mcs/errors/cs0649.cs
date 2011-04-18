// CS0649: Field `X.s' is never assigned to, and will always have its default value `null'
// Line: 4
// Compiler options: -warnaserror -warn:4

class X {
	string s;

	string Value {
		get {
			return s;
		}
	}
}
