using System;

class A {
	public virtual void Foo (int i) { }
	
	public virtual void Foo (double d) {
		throw new Exception ("Shouldn't be invoked");
	}
}

class B : A {
	public override void Foo (double d) {
		throw new Exception ("Overload resolution failed");
	}
	
	public static void Main () {
		new B ().Foo (1);
	}
}
