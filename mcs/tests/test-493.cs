class A {
	protected int f { get { return 1; } }
}

class B : A {
         int bar () { return new C().f; } 
   }
   
class C : B {
	public static void Main () {}
} 
