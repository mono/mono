// Compiler options: -doc:xml-063.xml

/// Test A
public class A
{
}

/// <seealso cref="explicit operator long (Test)"/>
/// <seealso cref="explicit operator A"/>
/// <seealso cref="implicit operator Test"/>
/// <seealso cref="implicit operator Test(bool)"/>
/// <seealso cref="operator !(Test)"/>
/// <seealso cref="operator =="/>
/// <seealso cref="operator !="/>
/// <seealso cref="Test()"/>
public class Test
{
	/// Start
	Test ()
	{
	}

	/// Comment
	public static explicit operator A (Test test)
	{
		return new A ();
	}

	/// Comment 2
	public static explicit operator long (Test test)
	{
		return 2;
	}

	/// Comment 3
	public static implicit operator Test (int test)
	{
		return new Test ();
	}

	/// Comment 4
	public static implicit operator Test (bool test)
	{
		return new Test ();
	}

	/// Comment 5
	public static bool operator !(Test test)
	{
		return false;
	}

	/// Comment 6
	public static bool operator == (Test a, int b)
	{
		return true;
	}

	/// Comment 7
	public static bool operator != (Test a, int b)
	{
		return false;
	}

	/// Comment 61
	public static bool operator == (Test a, long b)
	{
		return true;
	}

	/// Comment 72
	public static bool operator != (Test a, long b)
	{
		return false;
	}

	static void Main ()
	{
	}
}
