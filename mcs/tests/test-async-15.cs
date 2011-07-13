// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;
using System.Threading;

interface IFoo
{
	int Value { get; set; }
}

struct S : IFoo
{
	public S (int a1, string a2)
	 	: this ()
	{
		Value = a1;
	}
	
	public int Value { get; set; }
	
	public void SetValue (int value)
	{
		Value = value;
	}
}

class Tester
{
	async Task<T> NewInitTestGen<T> () where T : struct, IFoo
	{
		int value = 9;
		
		var s = new T () {
			Value = await Task.Factory.StartNew (() => 13)
		};
		
		if (s.Value != 13)
			return new T ();
		
		return s;
	}
	
	public static int Main ()
	{
		var t = new Tester().NewInitTestGen<S> ();
		
		if (!Task.WaitAll (new[] { t }, 1000)) {
			return 1;
		}
		
		if (t.Result.Value != 13)
			return 2;
		
		return 0;
	}
}
