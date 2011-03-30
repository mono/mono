// CS0506: `C.Run()': cannot override inherited member `A.Run()' because it is not marked virtual, abstract or override
// Line: 7
// Compiler options: -r:CS0506-3-lib.dll

public class C : B
{
	public override void Run ()
	{
	}
}
