// CS1599: The return type of `System.TypedReference' is not allowed
// Line: 8

using System;

public class Program
{
    public static TypedReference operator + (int a, Program b)
    {
    	throw new ApplicationException ();
    }
}