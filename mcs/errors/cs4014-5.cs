// CS4014: The statement is not awaited and execution of current method continues before the call is completed. Consider using `await' operator or calling `Wait' method
// Line: 17
// Compiler options: -warnaserror

using System;
using System.Threading.Tasks;

class C
{
    public static async Task<T> Test<T> ()
    {
        return await Task.FromResult (default (T));
    }

    static void Main ()
    {
        Test<object> ();
    }
}
