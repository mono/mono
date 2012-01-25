// CS0216: The operator `Test.operator ==(Test, bool)' requires a matching operator `!=' to also be defined
// Line: 10

partial class Test
{
}

partial class Test
{
	public static bool operator == (Test lhs, bool rhs)
	{
		return false;
	}
}

