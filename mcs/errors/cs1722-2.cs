// cs1722: type in interface list is not a interface, base classes must be listed first
//
class A1 {
}

interface I {
}

class B : I, A1 {
	static void Main () {}
}
