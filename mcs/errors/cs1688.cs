// CS1688: Cannot convert anonymous method block without a parameter list to delegate type `C.WithOutParam' because it has one or more `out' parameters
// Line: 10

class C
{
	delegate void WithOutParam (out string value);

    static void Main() 
    {
        WithOutParam o = delegate
			{ 
				return;
			};
    }
}