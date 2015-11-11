using System.Threading.Tasks;

class X
{
	public async Task ReturnsTaskAsync (Task task)
	{
		await task;
	}

	public async Task<Task> ReturnsTaskOfTaskAsync ()
	{
		var t1 = Task.FromResult (ReturnsTaskAsync (null));
		await t1;
		Task<Task> t2 = Task.FromResult (ReturnsTaskAsync (null));
		return t2;
	}

	public static void Main ()
	{
		new X ().ReturnsTaskOfTaskAsync ().Wait ();
	}
}