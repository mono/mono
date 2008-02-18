// CS0752: `C.Foo(out int)': A partial method parameters cannot use `out' modifier
// Line: 7


public partial class C
{
	partial void Foo (out int i)
	{
		i = 8;
	}
}
