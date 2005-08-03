// cs0277.cs: Accessor `C.Prop.set' must be declared public to implement interface member `I.Prop.set'
// Line: 10

interface I {
	decimal Prop { set; }
}

class C: I {
	public decimal Prop {
		internal set {}
		get {
			return 0;
		}
	}
}