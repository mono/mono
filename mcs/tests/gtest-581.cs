// Compiler options: -r:gtest-581-lib.dll

class Program
{
	static void Main()
	{
		var f1 = (IA<A>) new C();
		var f2 = (IA<B>) new C();
		var f3 = (IB<A>) new C();
		var f4 = (IB<C>) new C();
	}
}