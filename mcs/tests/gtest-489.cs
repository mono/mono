abstract class sample
{
	public abstract TValue Value<TKey, TValue> ();

	class nested<T> : sample
	{
		struct holder<TKey, TValue>
		{
			public static TValue Val;
		}

		public override sealed TValue Value<TKey, TValue> ()
		{
			return holder<TKey, TValue>.Val;
		}
	}

	public static void Main ()
	{
	}
}
