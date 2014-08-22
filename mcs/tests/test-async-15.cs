using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

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

struct S2 : IEnumerable
{
	public List<int> Values;

	public void Add (int x)
	{
		if (Values == null)
			Values = new List<int> ();

		Values.Add(x);
	}

	public IEnumerator GetEnumerator()
	{
		return Values as IEnumerator;
	}
}

class Tester
{
	async Task<T> NewInitTestGen<T> () where T : struct, IFoo
	{
		var s = new T () {
			Value = await Task.Factory.StartNew (() => 13).ConfigureAwait (false)
		};
		
		if (s.Value != 13)
			return new T ();
		
		return s;
	}

	static async Task<int> NewInitCol ()
	{
		var s = new S2 { 
			await Task.FromResult (1),
			await Task.Factory.StartNew (() => 2)
		};

		return s.Values [0] + s.Values [1];
	}
	
	public static int Main ()
	{
		var t = new Tester().NewInitTestGen<S> ();
		
		if (!Task.WaitAll (new[] { t }, 1000)) {
			return 1;
		}
		
		if (t.Result.Value != 13)
			return 2;

		var v = NewInitCol ().Result;
		if (v != 3)
			return 3;
		
		return 0;
	}
}
