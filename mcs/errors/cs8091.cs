// CS8091: `Test.Test()': Contructors cannot be extern and have a constructor initializer
// Line: 16

public class A
{
    public A (int arg)
    {
    }
}

public class Test : A
{
	int prop = 1;

	public extern Test ()
		: base (1);
}