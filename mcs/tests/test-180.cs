using System;

public class Testing
{
	public enum Fruit { Apple, Orange };	

	public static void Main()
	{
		Console.WriteLine(Convert.ToInt64( Fruit.Orange as Enum ) );
	}
}
