//
// Second test from bug 80518
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
	public abstract void Print<T>(object x) where T: BaseClass;
    } 
    
    public class PrinterImpl: Printer
    {
	public override void Print<T>(object x)
	{
	    Console.WriteLine((x as T).Hello);
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
