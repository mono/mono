// cs0236-2.cs: A field initializer cannot reference the nonstatic field, method, or property `C1.CC'
// Line: 11

class C1
{
    public double CC = 0;
}

class C2
{
	public static readonly double X_Small2 = C1.CC;
}
