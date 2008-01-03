// CS0019: Operator `??' cannot be applied to operands of type `T' and `T' 
// Line: 8

class F
{
	T Bar<T> (T t)
	{
		return t ?? default(T);
	}
}
