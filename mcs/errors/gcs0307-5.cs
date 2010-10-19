// CS0307: The property `Test<T,U>.Value' cannot be used with type arguments
// Line: 16

class Test<T, U>
{
	public object Value {
		get { return null; }
	}

	public class B
	{
		public B (object arg)
		{
		}
		
		public static B Default = new B (Value<U>.Default);
	}
}
