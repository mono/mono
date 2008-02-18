// CS1661: Cannot convert `lambda expression' to delegate type `C.WithOutParam' since there is a parameter mismatch
// Line: 10


class C
{
	delegate void WithOutParam (out string value);

	static void Main() 
	{
		WithOutParam o = (string l) => { return; };
    }
}
