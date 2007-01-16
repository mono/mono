//
// Test case from bug 80518
//

using System;

namespace test
{
    public class BaseClass
    {
	public BaseClass()
	{
	}
	public string Hello { get { return "Hello"; } }
    }

    public abstract class Printer
    {
	public abstract void Print<T>(T obj) where T: BaseClass;
    } 
    
    public class PrinterImpl : Printer
    {
	public override void Print<T>(T obj) 
	{
	    Console.WriteLine(obj.Hello);
	}
    }

    public class Starter
    {
	public static void Main( string[] args )
	{
	    BaseClass bc = new BaseClass();
	    Printer p = new PrinterImpl();
	    p.Print<BaseClass>(bc);
	}	
    }
}
