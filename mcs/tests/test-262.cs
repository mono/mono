namespace n1 {
	class Attribute {}
}

namespace n3 {
	using n1;
	using System;
	class A {
		void Attribute () {
		}
		void X ()
		{
			Attribute ();
		}
		static void Main () {
			new A ().X ();
		}
	}
}