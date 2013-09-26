using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

class TodoItem
{
}

internal class MobileServiceTable2<T>
{
	public Task<List<T>> ToListAsync ()
	{
		var r = new List<T> ();
		r.Add (default (T));
		return Task.FromResult<List<T>> (r);
	}
}

public class Tests
{
	int foo (Action t)
	{
		t ();
		return 0;
	}

	private void OnTap (TodoItem task)
	{
	}

	private async Task RefreshAsync ()
	{
		var ta = new MobileServiceTable2<TodoItem> ();
		var r = await ta.ToListAsync ();

		r.Select<TodoItem, int> (t => foo (() => OnTap (t))).ToList ();
	}

	public static void Main (String[] args)
	{
		var t = new Tests ();
		t.RefreshAsync ().Wait ();
	}
}