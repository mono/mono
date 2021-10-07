using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class Tests
{
    public static int Main (String[] args) {
        test_async_debug_generics ();
		return 1;
    }
    [MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void test_async_debug_generics () {
		ExecuteAsync_Broken<object>().Wait ();
	}

	async static Task<T> ExecuteAsync_Broken<T>()
	{
		await Task.Delay(2);
		return default;
	}
}
