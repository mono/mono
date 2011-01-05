// CS0019: Operator `==' cannot be applied to operands of type `object' and `V'
// Line: 9

public class C<V>
{
	public bool TryGet (V v)
	{
		object tmp = null;
		return tmp == v;
	}
}
