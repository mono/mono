// CS0171: Field `S<TKey>.key' must be fully assigned before control leaves the constructor
// Line: 13

public struct S<TKey> {
	private TKey key;

	public TKey Key {
		get { return key; }
		private set { key = value; }
	}
		
	public S (TKey key)
	{
		Key = key;
	}
}
