// CS0023: The `.' operator cannot be applied to operand of type `void'
// Line: 12

using System; 
 
public class Testing 
{ 
	public static void DoNothing() {} 
	 
	public static void Main() 
	{ 
	 	Console.WriteLine(DoNothing().ToString()); 
	} 
} 
