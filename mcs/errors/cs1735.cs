// CS1735: XML comment on `S<T1,T2>' has a typeparamref name `T' that could not be resolved
// Line: 9
// Compiler options: -doc:dummy.xml /warnaserror /warn:2

/// <summary>
///  Test
///  <typeparamref name="T" />
/// </summary>
public struct S<T1, T2>
{
}
