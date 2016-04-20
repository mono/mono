// CS0023: The `?' operator cannot be applied to operand of type `T'
// Line: 11

class C2<T>
{
	C2<T> i;
	T field;

	public void Foo ()
	{
		var x = i?.field;
	}
}