using System;

namespace TestNamespace
{
    /// <summary>
    /// <see cref="FunctionWithParameter" />
    /// </summary>
    class TestClass
    {
	public static void Main () {}
	/// <summary>
	/// Function with wrong generated parameter list in XML documentation. There is missing @ after System.Int32
	/// </summary>
	public void FunctionWithParameter( ref int number, out int num2)
	{
	    num2 = 0;
	    number = 1;
	}
    }
}
