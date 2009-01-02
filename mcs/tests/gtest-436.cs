public enum AnEnum
{
	value1,
	value2
}

class SomeClass
{
	public static int Main ()
	{
		if (AnEnum.value1 != null) {
			return 0;
		}

		return 1;
	}
}
