using System;

using System.Linq.Expressions;

public enum Code
{
	Opened = 0,
	Closed = 1,
	ReckonedUp = 2
}

public struct Status
{
	Code value;

	public Status (Code value)
	{
		this.value = value;
	}

	public static implicit operator Status (Code x)
	{
		return new Status (x);
	}

	public static implicit operator Code (Status x)
	{
		return x.value;
	}
}

public class Test
{
	Status status;

	public static void Main ()
	{
		Test test = new Test ();

		if (test.status == Code.ReckonedUp) {
			return;
		}
	}
}
