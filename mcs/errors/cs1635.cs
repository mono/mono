// CS1635: Cannot restore warning `CS0219' because it was disabled globally
// Line: 11
// Compiler options: -nowarn:219 -warnaserror

class C
{
    public static void Main ()
    {
#pragma warning disable 219
	int o = 4;
#pragma warning restore 219
    }
}
