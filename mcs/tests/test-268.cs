public enum MyEnum { V = 1 }

class X {
	public MyEnum MyEnum;	
	class Nested {
		internal MyEnum D () { 
			return MyEnum.V; 
		}
	}
	
	static int Main () {
		Nested n = new Nested ();
		return n.D() == MyEnum.V ? 0 : 1;
	}
}
