// CS0104: `X' is an ambiguous reference between `A.X' and `B.X'
// Line: 16
namespace A {
	class X {
	}
}

namespace B {
	class X {
	}
}

namespace C {
	using A;
	using B;
	class D : X {

	static void Main () {}
	}
}
