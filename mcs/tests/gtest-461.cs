using System;

namespace Test
{
	public class BaseContext
	{
	}

	public class MyDataContext : BaseContext
	{
	}

	public abstract class Entity<T>
	{
	}

	public class Person : Entity<MyDataContext>
	{
	}

	public sealed class TheBox<T> where T : BaseContext
	{
		public U GetById<U> (Guid entityId) where U : Entity<T>
		{
			return null;
		}
	}

	public class Program
	{
		public static void Main ()
		{
			TheBox<MyDataContext> dc = new TheBox<MyDataContext> ();
			dc.GetById<Person> (Guid.NewGuid ());
		}
	}
}

