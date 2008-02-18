

using System;

static class AExtensions
{
	public static int Round (this double d)
	{
		return (int) Math.Round (d);
	}
}

static class BExtensions
{
	public static T GetBy<T> (this T [] a, double p)
	{
		return a [p.Round ()];
	}
}

public class C
{
	public static void Main ()
	{
	}
}
