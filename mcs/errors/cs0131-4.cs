// CS0131: The left-hand side of an assignment must be a variable, a property or an indexer
// Line: 17

public class Person
{
	string _name;

	public string Name
	{
		get { return _name; }
		set { _name = value; }
	}

	public static void Main ()
	{
		Person johnDoe = new Person ();
		(string) johnDoe.Name = "John Doe";
	}
}
