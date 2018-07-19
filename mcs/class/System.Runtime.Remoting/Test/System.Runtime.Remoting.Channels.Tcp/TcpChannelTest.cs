//
// TcpChanneltest.cs
//
// Author:
//	Gert Driesen <drieseng@users.sourceforge.net>
//
// Copyright (C) 2008 Gert Driesen
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.Remoting
{
	[TestFixture]
	public class TcpChannelTest
	{
		[Test] // TcpChannel (IDictionary, IClientChannelSinkProvider, IServerChannelSinkProvider)
		public void Constructor3 ()
		{
			const string SERVICE_URI = "MarshalSvc";
			string [] urls;
			ChannelDataStore ds;
			TcpChannel chn;

			MarshalObject marshal = new MarshalObject ();

			var port = NetworkHelpers.FindFreePort ();
			IDictionary props = new Hashtable ();
			props ["name"] = "marshal channel";
			props ["port"] = port;
			props ["bindTo"] = IPAddress.Loopback.ToString ();
			chn = new TcpChannel (props, null, null);

			ChannelServices.RegisterChannel (chn);

			Assert.AreEqual ("marshal channel", chn.ChannelName, "#A1");
			urls = chn.GetUrlsForUri (SERVICE_URI);
			Assert.IsNotNull (urls, "#A2");
			Assert.AreEqual (1, urls.Length, "#A3");
			Assert.AreEqual ($"tcp://{IPAddress.Loopback.ToString ()}:{port}/{SERVICE_URI}", urls [0], "#A6");
			ds = chn.ChannelData as ChannelDataStore;
			Assert.IsNotNull (ds, "#A4");
			Assert.AreEqual (1, ds.ChannelUris.Length, "#A5");
			Assert.AreEqual ($"tcp://{IPAddress.Loopback.ToString ()}:{port}", ds.ChannelUris [0], "#A6");

			ChannelServices.UnregisterChannel (chn);
			
			chn = new TcpChannel ((IDictionary) null, null, null);

			ChannelServices.RegisterChannel (chn);

			Assert.AreEqual ("tcp", chn.ChannelName, "#B1");
			urls = chn.GetUrlsForUri (SERVICE_URI);
			Assert.IsNull (urls, "#B1");
			ds = chn.ChannelData as ChannelDataStore;
			Assert.IsNull (ds, "#B2");

			ChannelServices.UnregisterChannel (chn);
		}
		
		struct ParseURLTestCase {
			public readonly string input;
			public readonly string retval;
			public readonly string objectURI;
			
			public ParseURLTestCase (string s0, string s1, string s2)
			{
				input = s0;
				retval = s1;
				objectURI = s2;
			}
		};
		
		ParseURLTestCase[] ParseURLTests = new ParseURLTestCase[] {
			new ParseURLTestCase ("tcp:", "tcp:", null),
			new ParseURLTestCase ("tcp://", "tcp://", null),
			new ParseURLTestCase ("tcp:localhost", null, null),
			new ParseURLTestCase ("ftp://localhost", null, null),
			new ParseURLTestCase ("tcp://localhost", "tcp://localhost", null),
			new ParseURLTestCase ("tCp://localhost", "tCp://localhost", null),
			new ParseURLTestCase ("tcp://localhost:/", "tcp://localhost:", "/"),
			new ParseURLTestCase ("tcp://localhost:9090", "tcp://localhost:9090", null),
			new ParseURLTestCase ("tcp://localhost:9090/", "tcp://localhost:9090", "/"),
			new ParseURLTestCase ("tcp://localhost:9090/RemoteObject.rem", "tcp://localhost:9090", "/RemoteObject.rem"),
			new ParseURLTestCase ("tcp://localhost:q24691247abc1297/RemoteObject.rem", "tcp://localhost:q24691247abc1297", "/RemoteObject.rem"),
		};
		
		[Test] // TcpChannel.Parse ()
		public void ParseURL ()
		{
			TcpChannel channel;
			int i;
			
			channel = new TcpChannel ();
			
			for (i = 0; i < ParseURLTests.Length; i++) {
				string retval, objectURI;
				
				retval = channel.Parse (ParseURLTests[i].input, out objectURI);
				
				Assert.AreEqual (ParseURLTests[i].retval, retval, "#C1");
				Assert.AreEqual (ParseURLTests[i].objectURI, objectURI, "#C2");
			}
		}
		
		public class MarshalObject : ContextBoundObject
		{
			public MarshalObject ()
			{
			}
		}
		
		TcpServerChannel GetServerChannel (string name, int port)
		{
			TcpServerChannel serverChannel = new TcpServerChannel (name + "Server", port);
			ChannelServices.RegisterChannel (serverChannel);
			
			RemotingConfiguration.RegisterWellKnownServiceType (
				typeof (RemoteObject), "RemoteObject.rem", 
				WellKnownObjectMode.Singleton);
			
			return serverChannel;
		}
		
		TcpClientChannel GetClientChannel (string name, string uri)
		{
			TcpClientChannel clientChannel = new TcpClientChannel (name + "Client", null);
			ChannelServices.RegisterChannel (clientChannel);
			
			WellKnownClientTypeEntry remoteType = new WellKnownClientTypeEntry (
				typeof (RemoteObject), uri + "/RemoteObject.rem");
			RemotingConfiguration.RegisterWellKnownClientType (remoteType);
			
			return clientChannel;
		}
		
		[Test]
		[Category ("NotWorking")]  // seems to hang - "too many open files" ???
		[Ignore ("Fails on MS")]
		public void TestTcpRemoting ()
		{
			TcpServerChannel serverChannel = GetServerChannel ("TcpRemotingTest", 9090);
			string uri = serverChannel.GetChannelUri ();
			
			Assert.IsNotNull (uri, "Server channel URI is null");
			
			TcpClientChannel clientChannel = GetClientChannel ("TcpRemotingTest", uri);
			
			RemoteObject remoteObject = new RemoteObject (); 
			
			Assert.IsTrue (remoteObject.ReturnOne () == 1, "Invoking RemoteObject.ReturnOne() failed");
		}
	}
}
