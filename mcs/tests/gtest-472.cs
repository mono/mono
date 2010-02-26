class C<T>
{
	public virtual void CopyUnsafe(T[] value, params long[] fromIdx){}
	public virtual bool CopyUnsafe(T[] value, long fromIdx) { return true; }

	public virtual void CopyUnsafe(T[] value)
	{
		bool b = CopyUnsafe(value, 0);
	}
}

class A
{
	public static void Main ()
	{
	}
}
