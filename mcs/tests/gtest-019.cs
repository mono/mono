// A very simple generic interface

public interface IEnumerator<T> {
	T Current { get; } 
	bool MoveNext();
	void Reset();
}

class X
{
	static void Main ()
	{ }
}
