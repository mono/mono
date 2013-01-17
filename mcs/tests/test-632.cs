using System;
class Bug379822 {
	static void Assert (bool expected, bool value)
	{
		if (value != expected)
			throw new Exception ("unexpected value");
	}

	static void TestAnd (bool var)
	{
		Assert (false, false && var);
		Assert (false, var && false);
		Assert (false, false & var);
		Assert (false, var & false);

		Assert (var, true && var);
		Assert (var, var && true);
		Assert (var, true & var);
		Assert (var, var & true);
	}

	static void TestOr (bool var)
	{
		Assert (var, false || var);
		Assert (var, var || false);
		Assert (var, false | var);
		Assert (var, var | false);

		Assert (true, true || var);
		Assert (true, var || true);
		Assert (true, true | var);
		Assert (true, var | true);
	}

	static void Test (bool var)
	{
		TestAnd (var);
		TestOr (var);
	}

	public static void Main ()
	{
		Test (false);
		Test (true);
	}
}