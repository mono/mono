using c = System.Console;
using s = System;

namespace A {
	namespace B {
		class C {
			public static void Hola () {
				c.WriteLine ("Hola!");
			}
		}
	}
}

namespace X {
	namespace Y {
		namespace Z {
			class W {
				public static void Ahoj () {
					s.Console.WriteLine ("Ahoj!");
				}
			}
		}
	}
}

class App {
	public static int Main () {
		A.B.C.Hola ();
		X.Y.Z.W.Ahoj ();

		return 0;
	}
}
