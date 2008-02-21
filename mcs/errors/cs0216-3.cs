// CS0216: The operator `MyType.operator >(MyType, MyType)' requires a matching operator `<' to also be defined
// Line: 23

public struct MyType
{
	int value;

	public MyType (int value)
	{
		this.value = value;
	}

	public static bool operator == (MyType a, MyType b)
	{
		return a.value == b.value;
	}

	public static bool operator != (MyType a, MyType b)
	{
		return a.value != b.value;
	}
	
	public static bool operator > (MyType a, MyType b)
	{
		return a.value > b.value;
	}

	public static bool operator >= (MyType a, MyType b)
	{
		return a.value >= b.value;
	}	

	public override string ToString ()
	{
		return value.ToString ();
	}
}
