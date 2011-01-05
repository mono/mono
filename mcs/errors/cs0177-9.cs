// CS0177: The out parameter `output' must be assigned to before control leaves the current method
// Line: 10

class Test
{
	delegate T Creator<T> ();

	static bool TryAction<T> (Creator<T> creator, out T output) where T : struct
	{
		return false;
	}
}
