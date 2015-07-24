// Compiler options: -t:library

namespace Reference
{
	public interface IB
	{
	}

	public interface IA : IHide
	{
		new IB Equals { get; }
	}
	
	public interface IHide
	{
		bool Equals(object o);
	}
}