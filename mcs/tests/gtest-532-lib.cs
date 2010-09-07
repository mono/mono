// Compiler options: -t:library

public interface IServicesContainer
{
	void Register<I, T> () where T : I;
	void Register<I> (object instance);
}
