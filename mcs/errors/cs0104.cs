// cs0104.cs: Ambiguous type reference
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
