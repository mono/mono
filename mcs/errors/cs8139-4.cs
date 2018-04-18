// CS8139: `D.Prop': cannot change return type tuple element names when overriding inherited member `C.Prop'
// Line: 14

class C
{
	public virtual (int a, int b) Prop {
		get {
			throw null;
		}
	}
}

class D : C
{
	public override (int c, int d) Prop {
		get {
			throw null;
		}
	}	
}