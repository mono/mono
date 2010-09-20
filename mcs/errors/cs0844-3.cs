// CS0844: A local variable `top' cannot be used before it is declared. Consider renaming the local variable when it hides the member `X.top'
// Line: 17

class Symbol
{
}

class X
{
	Symbol top;

	internal int Enter (Symbol key, object value)
	{
		if (key != null) {
			top = key;
		}
		object top = null;
		return top.Count;
	}
}
