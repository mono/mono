using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[AsyncMethodBuilder (typeof(MyTaskMethodBuilder<>))]
class MyTask<T>
{
}

[AsyncMethodBuilder (typeof(MyTaskMethodBuilder))]
class MyTask
{
}

class MyTaskMethodBuilder
{
	public static MyTaskMethodBuilder Create()
	{
		return null;
	}

	public MyTask Task {
		get {
			return null;
		}
	}

	public void SetException (Exception exception)
	{

	}

	public void SetResult ()
	{

	}

	public void AwaitOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
	{

	}

	public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
	{

	}

	public void Start<TStateMachine> (ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
	{

	}

	public void SetStateMachine (IAsyncStateMachine stateMachine)
	{

	}	
}

class MyTaskMethodBuilder<T>
{
	public static MyTaskMethodBuilder<T> Create()
	{
		return null;
	}

	public MyTask<T> Task {
		get {
			return null;
		}
	}

	public void SetException (Exception exception)
	{

	}

	public void SetResult (T result)
	{

	}

	public void AwaitOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
	{

	}

	public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
	{

	}

	public void Start<TStateMachine> (ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
	{

	}

	public void SetStateMachine (IAsyncStateMachine stateMachine)
	{

	}
}

class X
{
	public async MyTask Test ()
	{
		await Task.Delay (1);
	}

	public async MyTask<int> Test2 ()
	{
		await Task.Delay (1);
		return 2;
	}

	public async ValueTask<string> Test3 ()
	{
		await Task.Delay (1);
		return "as";
	}	

	public static void Main ()
	{
		var x = new X ();
		var r1 = x.Test3 ().Result;
	}
}