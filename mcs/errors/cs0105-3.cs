// CS0105: The using directive for `N.M' appeared previously in this namespace
// Line: 8
// Compiler options: -warnaserror -warn:3

namespace N
{
	using M;
	using N.M;
	
	namespace M
	{
	}
}
