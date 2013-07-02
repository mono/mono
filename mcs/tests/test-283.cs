class X {
	public virtual int Foo () {            
		return 1;
	}
}

class Y : X {

	delegate int D();
	
	
	D GetIt () {
		return new D (base.Foo);
	}
	
	D GetIt2 () {
		return base.Foo;
	}
	
	public override int Foo () {
		return 0;
	}
	
	public static int Main ()
	{
		if (new Y ().GetIt () () == 1 && new Y ().GetIt2 () () == 1) {
			System.Console.WriteLine ("good");
			return 0;
		}
		
		return 1;
	}
}