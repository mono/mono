using System;
using System.Linq.Expressions;

namespace FieldInfoBug
{
	public class MonoRuntime
	{
		public static int Main ()
		{
			// This constructor throws ArgumentException: 
			// The field handle and the type handle are incompatible.
			new GenericClass<object> ("value");
			return 0;
		}
	}

	public class GenericClass<T>
	{
		public GenericClass (string argument)
		{
			Expression<Func<string>> expression = () => argument;
		}
	}
}