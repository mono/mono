// Compiler options: -target:library

namespace SeparateAssembly
{
	public interface IGenericAction<T1, T2>
	{
		void AddAction(IGenericAction<T1, T2> action);
		void AddAction(IGenericAction<T2, T1> action);
	}
}