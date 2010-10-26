using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;

public class Main2
{
	// Use this for initialization
	static void Main () 
    {
       for (int i = 0; i < 10; i++)
            ThreadPool.QueueUserWorkItem(ThreadProc);
	}
	
    static void ThreadProc(object o)
	{
		try{
		var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		socket.BeginConnect(new IPEndPoint(IPAddress.Parse("66.102.13.106"), 80), new AsyncCallback(ConnectCallback1), null);
		} finally {
			Console.WriteLine(GetCurrentWin32ThreadId()+" is Outta here");
		}
		//Thread.Sleep(300);
	}
	
	[System.Runtime.InteropServices.DllImport("Kernel32", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
    public static extern Int32 GetCurrentWin32ThreadId();
	
	static void ConnectCallback1(IAsyncResult ar)
	{
	}

}


