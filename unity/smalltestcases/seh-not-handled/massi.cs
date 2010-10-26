using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;

public class Test
{
	static void SIGSEGV () 
	{
		Console.WriteLine ("Before SIGSEGV");
		string s = null;
		Console.WriteLine (s.Length);
		Console.WriteLine ("After SIGSEGV");
	}
	static void Main () {
		SIGSEGV ();
	}
}
