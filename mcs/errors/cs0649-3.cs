// CS0649: Field `X.e' is never assigned to, and will always have its default value `0'
// Line: 10
// Compiler options: -warnaserror -warn:4

using System;
class X {
	E e;

	E Value {
		get {
			return e;
		}
	}
}

enum E
{
	Foo
}
