// CS0442: `C.Prop.get': abstract properties cannot have private accessors
// Line: 7

abstract class C {
    protected abstract int Prop
    {
	private get;
	set;
    }
}

