using System.Threading.Tasks;

class C
{
	public async Task SynchronousCall (int arg)
	{
		AnotherTask (arg);
	}
	
	Task AnotherTask (int arg)
	{
		return Task.FromResult (arg);
	}
	
	public static void Main ()
	{
		new C ().SynchronousCall (1);
	}
}