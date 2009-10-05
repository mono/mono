// Compiler options: -target:library

public interface InterfaceWithGenericMethod
{
	void GenericMethod_1<T>() where T : struct, II;
	void GenericMethod_2<T>() where T : class, II;
	void GenericMethod_3<T>() where T : II, new ();
}

public interface II
{
}
