// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `S'
// Line: 9
// Compiler options: /unsafe

unsafe public class C
{
	S* i;
}

public struct S
{
	AC ac;
}

abstract class AC
{
}
