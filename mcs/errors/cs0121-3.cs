// cs0121-3.cs: The call is ambigious between `IInteger.Add (int)' and `IDouble.Add (double)'
// line 28

// (note, this is taken from `13.2.5 Interface member access')
interface IInteger {
	void Add(int i);
}

interface IDouble {
	void Add(double d);
}

interface INumber: IInteger, IDouble {}

class Number : INumber {
	void IDouble.Add (double d)
	{
		System.Console.WriteLine ("IDouble.Add (double d)");
	}
	void IInteger.Add (int d)
	{
		System.Console.WriteLine ("IInteger.Add (int d)");
	}
	
	static void Main ()
	{
		INumber n = new Number ();
		n.Add(1);               // Error, both Add methods are applicable
		n.Add(1.0);               // Ok, only IDouble.Add is applicable
		((IInteger)n).Add(1);   // Ok, only IInteger.Add is a candidate
		((IDouble)n).Add(1);      // Ok, only IDouble.Add is a candidate
	}
}