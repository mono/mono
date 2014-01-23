using System;
using System.Threading;
using System.Threading.Tasks;

class A
{
	public virtual Task<int> Foo (int value)
	{
		return Task.FromResult (value);
	}
	
	public virtual Task<int> Prop { get; set; }
}

class B : A
{
	public override Task<int> Prop {
		get {
			throw new ApplicationException ();
		}
		set {
			throw new ApplicationException ();
		}
	}
	
	public override async Task<int> Foo (int value)
	{
		return await base.Foo (value) + 1;
	}
	
	public async Task<int> Foo2 (int value)
	{
		base.Prop = Task.FromResult (value);
		return await base.Prop;
	}
}

class Program
{
	public static int Main()
	{
		var b = new B ();
		if (b.Foo (3).Result != 4)
			return 1;
		
		if (b.Foo2 (5).Result != 5)
			return 2;
		
		Console.WriteLine("ok");
		return 0;
	}
}
