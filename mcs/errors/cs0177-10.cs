// CS0177: The out parameter `output' must be assigned to before control leaves the current method
// Line: 8

class Test
{
	static bool TryAction<T> (out T output)
	{
		return false;
	}

	static void Main ()
	{
		Test value;
		TryAction<Test> (out value);
	}
}
