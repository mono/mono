using System;
using System.Threading.Tasks;

public class Class1
{
	protected void InvokeAction (Action action)
	{
		action ();
	}

	public void Bar()
	{
	}

	async Task Test ()
	{
		Task.Run(async () =>
			{
				var implementor = ServiceLocator.GetImplementor<IInterface1> ();
				string message = null;
				bool result = await implementor.Foo ((s) => message = s);

				InvokeAction (() => Bar ());
			}).Wait ();
	}

	interface IInterface1
	{
		Task<bool> Foo(Action<string> action);
	}

	class CIInterface1 : IInterface1
	{
		public Task<bool> Foo (Action<string> action)
		{
			action ("msg");
			return Task.FromResult (false);
		}
	}

	static class ServiceLocator
	{
		public static TService GetImplementor<TService>() where TService : class
		{
			return (TService) (object) new CIInterface1 ();
		}
	}

	public static void Main ()
	{
		new Class1 ().Test ().Wait ();
	}
}

