// cs0023-3.cs: The `.' operator can not be applied to operands of type 'void'
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
