using System;

public struct MoneyValue
{
	private readonly decimal _amount;

	public MoneyValue (decimal amount)
	{
		_amount = amount;
	}

	public static implicit operator decimal (MoneyValue moneyValue)
	{
		return moneyValue._amount;
	}
}

public class Program
{
	static void Main ()
	{
		var nullMoneyValue = (MoneyValue?) null;
		var moneyValue = new MoneyValue (123);

		var crashApplication = nullMoneyValue < moneyValue; 

		Console.WriteLine("All OK");
	}
}
