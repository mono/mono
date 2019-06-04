struct rigidbody { public float x; }

class Program
{
	static rigidbody a;
	static ref rigidbody property_returning_struct_by_ref => ref a;

	static void Main()
	{
		System.Console.WriteLine (property_returning_struct_by_ref.x);
	}
}