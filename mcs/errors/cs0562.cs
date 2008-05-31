// CS0562: The parameter type of a unary operator must be the containing type
// Line: 7

class SampleClass {
	public static SampleClass operator - (int value)
	{
		return new SampleClass();
	}
}
