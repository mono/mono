using System.Threading.Tasks;

public class A
{
	public async Task<ValueType> Test1 (int input2)
	{
		return new ValueType (await Task.FromResult (12345));
	}

	public static void Main ()
	{
		var a = new A ();
		a.Test1 (1).Wait ();
	}
}

public struct ValueType
{
	public ValueType (int field2)
	{
	}
}
