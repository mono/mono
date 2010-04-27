public interface ITestBase<TInterface>
{ }

public interface ITestSub1 : ITestBase<ITestSub2>
{ }

public interface ITestSub2 : ITestSub1
{ }

class C
{
	public static void Main ()
	{
	}
}