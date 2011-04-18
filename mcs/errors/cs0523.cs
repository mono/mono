// CS0523: Struct member `B.a' of type `A' causes a cycle in the struct layout
// Line: 9

struct A {
	B b;
}

struct B {
	A a;
}

class Y { static void Main () {} }
