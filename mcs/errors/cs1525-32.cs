// CS1525: Unexpected symbol `<', expecting `,' or `>'
// Line: 9

interface IFoo<T>
{
}

public class Bar {
	public void GetItemCommand<IFoo<int>>()
	{
	}
}
