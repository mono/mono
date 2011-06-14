// CS1735: XML comment on `S.Test<U>()' has a typeparamref name `T' that could not be resolved
// Line: 9
// Compiler options: -doc:dummy.xml /warnaserror /warn:2

struct S
{
	/// <summary>
	///  Test
	///  <typeparamref name="T" />
	/// </summary>
	public void Test<U> ()
	{
	}
}
