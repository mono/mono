// cs0038-2.cs: Cannot access a nonstatic member of outer type `X' via nested type `X.Nested'
// Line: 9
public enum MyEnum { V = 1 }

class X {
	public MyEnum MyEnum;	
	class Nested {
		internal MyEnum D () { 
			return MyEnum; 
		}
	}
	
	static int Main () {
		Nested n = new Nested ();
		return n.D() == MyEnum.V ? 0 : 1;
	}
}
