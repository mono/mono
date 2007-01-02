// CS0649: Field `X.y' is never assigned to, and will always have its default value `null'
// Line: 10
// Compiler options: -warnaserror -warn:4

class X {
	Y y;

	Y Value {
		get {
			return y;
		}
	}
}

struct Y
{
}
