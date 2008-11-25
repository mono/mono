using System;

namespace OverloadTest
{
	public interface MyInterface<T>
	{
		void Invoke (T target);
	}

	public class MyClass<T>
	{

		public bool Method (MyInterface<T> obj)
		{
			return Method (obj.Invoke);
		}

		public bool Method (Action<T> myAction)
		{
			return true;
		}
	}

	class C
	{
		public static void Main ()
		{
		}
	}
}
