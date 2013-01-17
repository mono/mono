// Compiler options: -langversion:future
using System;
using System.Threading.Tasks;
using System.Threading;

struct Struct
{
	object m_member;
	public Struct (object member)
	{
		m_member = member;
	}

	public async Task<bool> AsyncMethod ()
	{
		bool b = (string)m_member == "1";
		await Task.Factory.StartNew (() => -3);
		b &= (string)m_member == "1";
		return b;
	}
}

class C
{
	public static int Main ()
	{
		Struct s = new Struct ("1");
		var t = s.AsyncMethod ();
		if (!Task.WaitAll (new[] { t }, 2000))
			return 1;
		
		if (!t.Result)
			return 2;
		
		return 0;
	}
}
