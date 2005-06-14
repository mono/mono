using System;
using System.Reflection;

namespace FLMID.Bugs.ParametersOne
{
	public class Class<T>
	{
		public void Add(T x)
		{
			System.Console.WriteLine("OK");
		}
	}
	public class Test
	{
	
		public static void Main(string [] args)
		{
			Class<string> instance = new Class<string>();
			
			MethodInfo _method = null;
			
			foreach(MethodInfo method in
typeof(Class<string>).GetMethods(BindingFlags.Instance | BindingFlags.Public))
			{
				if(method.Name.Equals("Add") && method.GetParameters().Length==1)
				{
					_method = method;
					break;
				}
			}
			_method.Invoke(instance , new object[]{"1"});
		}
	}
}
