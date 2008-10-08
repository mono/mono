// CS1002: Expecting `,', or `>', got <
// Line: 9

interface IFoo<T>
{
}

public class Bar {
	public void GetItemCommand<IFoo<int>>()
	{
	}
}
