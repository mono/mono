using c = System.Console;
using s = System;
using System2 = System;

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

namespace Foo {

  // Trick: this class is called System.  but we are going to use the using alias to
  // reference the real system.
  class System {
	static void X() {
	  System2.Console.WriteLine("FOO");
	}
  }
}

class App {
	public static int Main () {
		A.B.C.Hola ();
		X.Y.Z.W.Ahoj ();

		// Array declaration
		System2.Net.IPAddress[] addresses2;

		return 0;
	}
}
