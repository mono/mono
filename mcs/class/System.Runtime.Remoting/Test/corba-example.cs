using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.CORBA;
using System.IO;


// compile with:
// csc -r:../../lib/System.Runtime.Remoting.dll corba-example.cs

class Test : MarshalByRefObject {

	public int test_function (int a, bool b)
	{
		Console.WriteLine ("test function called: " + b);
		return a + 1;
	}
	
	static int Main () {

		Test t1 = new Test ();
		ObjRef myref = RemotingServices.Marshal (t1, "/test");
		Console.WriteLine ("OBJREF: " + myref.URI);
		
		string url = "corba://localhost:8000/test";
		string uri;
		
		CORBAChannel chnl = new CORBAChannel (8000);
		ChannelServices.RegisterChannel (chnl);

		Console.WriteLine ("Channel name: " + chnl.ChannelName);
		Console.WriteLine ("Channel priority: " + chnl.ChannelPriority);
		Console.WriteLine ("URI: " + chnl.Parse (url, out uri));
		Console.WriteLine ("URI: " + uri);
		
		Test tp = (Test)RemotingServices.Connect (typeof (Test), url);

		int res = tp.test_function (4, true);

		Console.WriteLine ("RESULT: " + res);
		
		chnl.StopListening (null);
		
		return 0;
	}
}
