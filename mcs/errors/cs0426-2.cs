// CS0426: The nested type `M' does not exist in the type `N'
// Line: 6

class A
{
	class B : N.M
	{
	}
}

class N
{
	public const string S = "1";
}
