// cs0197.cs: You cant pass by ref or out a member or field of a MarshalByRefObjectClass.
// Line: 14

namespace cs0197
{
	public class A: MarshalByRefObject
	{
		public string s;
	}
		
	public class B
	{
		public class ConCat (ref string s)
		{
			s += ' Error';
		}
		
		static void Main()
		{
			A Foo = new A ();
			Foo.s = 'cs0197';
			this.ConCat (ref Foo.s);
			Console.WriteLine (Foo.s);			
		}
	}
}
