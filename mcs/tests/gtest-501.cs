public class UndoableDictionary<TKey, TValue> : UpdateableCollection<TValue>
{
	public override void TestFunc ()
	{
	}
}

public abstract class UpdateableCollection<T>
{
	internal void AddReferences ()
	{
	}

	public virtual void TestFunc ()
	{
	}

	class X : UndoableDictionary<int, int>
	{
	}

}

public class C
{
	public static int Main ()
	{
		new UndoableDictionary<string, decimal> ();
		return 0;
	}
}
