// CS0216: The operator `Test.operator ==(Test, bool)' requires a matching operator `!=' to also be defined
// Line: 11

public abstract class Test
{
	public static bool operator == (Test lhs, bool rhs)
	{
		return false;
	}

	public static bool operator != (Test lhs, IDoNotExist rhs)
	{
		return !(lhs == rhs);
	}
}

