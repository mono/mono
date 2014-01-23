// CS0229: Ambiguity between `Program.I1.Id' and `Program.I2.Id'
// Line: 18

static class Program
{
	public interface I1
	{
		string Id { get; }
	}

	public interface I2
	{
		int Id { get; }
	}

	static void Generic<T> (T item) where T : I1, I2
	{
		var a = item.Id;
	}
}
