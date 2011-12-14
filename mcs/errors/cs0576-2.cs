// CS0576: Namespace `Top' contains a definition with same name as alias `T'
// Line: 12

namespace Top
{
	using T = System.Exception;

	namespace T
	{
		class C
		{
			T t;
		}
	}
}
