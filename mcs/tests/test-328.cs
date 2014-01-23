namespace X {
	enum Z { x };
}
namespace A {
	using Y = X;
	namespace B {
		using Y;
		class Tester {
			internal static Z z = Z.x;
			public static void Main() { }
		}
	}
}
