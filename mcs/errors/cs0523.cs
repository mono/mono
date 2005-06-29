// cs0523.cs: Struct member `B.a' of type `A' causes a cycle in the struct layout
//

struct A {
	B b;
}

struct B {
	A a;
}

class Y { static void Main () {} }
