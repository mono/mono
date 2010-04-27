// CS0693: Type parameter `T' has the same name as the type parameter from outer type `R<U>.A<T>'
// Line: 9
// Compiler options: -warnaserror -warn:3

class R<U>
{
	class A<T>
	{
		struct I<T>
		{
		}
	}
}
