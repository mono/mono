// CS0758: A partial method declaration and partial method implementation cannot differ on use of `params' modifier
// Line: 9


public partial class C
{
	partial void Foo (int[] args);
	
	partial void Foo (params int[] args)
	{
	}
}
