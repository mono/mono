// Compiler options: -target:library

namespace testcase
{
	public interface IInitializationExpression
	{
		void AddRegistry<T> (int i);
	}

	public class ConfigurationExpression
	{
		public void AddRegistry<T> (int i)
		{
		}
	}
}