using System;

public abstract class A
{
	public abstract event EventHandler Finished;
	public int count;

	public A ()
	{
	}

	public void test (A a)
	{
		a.Finished += TestA;
	}

	public void TestA (object sender, EventArgs e)
	{
		Console.WriteLine ("A test method.");
		count += 3;
	}
}

public class B : A
{
	public override event EventHandler Finished;

	public B ()
	{
		Finished += this.TestB;
		this.test (this);
		Finished (this, EventArgs.Empty);
	}

	public void TestB (object sender, EventArgs e)
	{
		Console.WriteLine ("B test method.");
		count += 7;
	}

	public static int Main ()
	{
		return new B ().count - 10;
	}
}
