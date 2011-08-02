// CS1540: Cannot access protected member `A.f' via a qualifier of type `A'. The qualifier must be of type `B' or derived from it
// Line: 9

class A {
	protected int f { get { return 1; } }
}

class B : A {
         int baz () { return new A().f; }
   }
 
