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
	
	public static int Main ()
	{
		INumber n = new Number ();
		n.Add(1);               
		n.Add(1.0);             
		((IInteger)n).Add(1);   
		((IDouble)n).Add(1);    
		return 0;
	}
}
