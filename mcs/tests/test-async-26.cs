// Compiler options: -langversion:future

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
	class Program
	{
		public static Task<TResult> Run<TResult> (Func<Task<TResult>> function)
		{
			var t = Task<Task<TResult>>.Factory.StartNew (function);
			return GetTaskResult (t);
		}

		async static Task<TResult> GetTaskResult<TResult> (Task<Task<TResult>> task)
		{
			return await task.Result;
		}

		public static int Main ()
		{
			var t2 = Run (() => Task<int>.Factory.StartNew (() => 5));

			if (!t2.Wait (1000)) {
				Console.WriteLine (t2.Status);
				return 1;
			}
			
			Console.WriteLine ("ok");
			return 0;
		}
	}
}
