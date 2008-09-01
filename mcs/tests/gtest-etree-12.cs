using System;
using System.Reflection;
using System.Linq.Expressions;

public static class TestCase
{
	// This causes all the trouble:
	public static bool DUMMY = StaticMethodTakingAnExpression ((i) => true);

	public static bool StaticMethodTakingAnExpression (
	  Expression<Func<object, bool>> expression)
	{
		// I don't execute the expression here!!!
		return false;
	}

	public static void DummyToMakeTheStaticsInitialize ()
	{
		// Just a dummy method to make this static class get initialized
	}
}

public class Program
{
	public static int Main ()
	{
		TestCase.DummyToMakeTheStaticsInitialize ();
		return 0;
	}
}
