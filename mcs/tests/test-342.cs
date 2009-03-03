using System;

class A {
	public virtual void Foo (int i) { }
	
	public virtual void Foo (double d) {
		throw new Exception ("Shouldn't be invoked");
	}
	
	public virtual bool this [int i] {
		get { return true; }
	}
	
	public virtual bool this [double d] {
		get { throw new Exception ("Shouldn't be invoked"); }
	}

}

class B : A {
	public override void Foo (double d) {
		throw new Exception ("Overload resolution failed");
	}
	
	public override bool this [double d] {
		get { throw new Exception ("Overload resolution failed"); }
	}
	
	public static void Main () {
		new B ().Foo (1);
		bool b = new B () [1];
	}
}
