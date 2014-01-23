class X2 {}
namespace A {
	enum X1 { x1 };
	enum X2 { x2 };
}
namespace A.B {
	using Y1 = X1;
	using Y2 = X2;
	class Tester {
		internal static Y1 y1 = Y1.x1;
		internal static Y2 y2 = Y2.x2;
		public static void Main() { }
	}
}
