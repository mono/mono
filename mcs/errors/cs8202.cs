//
// The type parameters introduce the names into the class namespace, so it is
// not possible to have members with the same name as a parameter
//

// First error.
class X <T> {
	int T;
}

// Second error: type parameter is the same name as the class

class Y <Y> {
}

class W {
	static void Main () {}
}
