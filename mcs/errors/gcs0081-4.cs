// gcs0081.cs: Type parameter declaration must be an identifier not a type
// Line: 4

interface IFoo<T>
{
}

public class Bar {
	public void GetItemCommand<IFoo<int>>()
	{
	}
}
