// CS1525: Unexpected symbol `='
// Line: 10

public class Test
{
	private string name;

	public string Name
	{
		get { return name ?? name = string.Empty; }
	}
}
