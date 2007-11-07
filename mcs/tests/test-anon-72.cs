public class Test
{
	public delegate bool UnaryOperator(object self, out object res);
	public void AddOperator(UnaryOperator target) {}
	public bool TryGetValue(object self, out object value)
	{
		value = null;
		return false;
	}
    
	public static void Main ()
	{
	}
	
	void Foo ()
	{
		AddOperator (delegate(object self, out object res) {
			object value;
			if (TryGetValue(self, out value)) {
				res = value;
				if (res != null) return true;
			}
			res = null;
			return false;
		});
	}
}
