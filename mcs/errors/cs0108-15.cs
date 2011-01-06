// CS0118: `B.Factory' hides inherited member `A.Factory(object)'. Use the new keyword if hiding was intended
// Line: 12
// Compiler options: -warnaserror -warn:2

public abstract class A
{
	public void Factory (object data) { }
}

public class B : A
{
	public delegate void Factory (object data, object fail);
}
