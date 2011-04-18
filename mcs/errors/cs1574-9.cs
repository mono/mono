// CS1574: XML comment on `Foo' has cref attribute `Dispatch()' that could not be resolved
// Line: 8
// Compiler options: -doc:dummy.xml -warnaserror

/// I am delegate, without parens
public delegate void Dispatch ();

/// <see cref="Dispatch()" />
public class Foo
{
}
