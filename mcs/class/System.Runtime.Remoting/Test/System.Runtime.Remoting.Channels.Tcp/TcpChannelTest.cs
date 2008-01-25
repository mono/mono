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

namespace MonoTests.System.Runtime.Remoting.Channels.Tcp
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

			IDictionary props = new Hashtable ();
			props ["name"] = "marshal channel";
			props ["port"] = 1236;
			props ["bindTo"] = IPAddress.Loopback.ToString ();
			chn = new TcpChannel (props, null, null);

			ChannelServices.RegisterChannel (chn);

			Assert.AreEqual ("marshal channel", chn.ChannelName, "#A1");
			urls = chn.GetUrlsForUri (SERVICE_URI);
			Assert.IsNotNull (urls, "#A2");
			Assert.AreEqual (1, urls.Length, "#A3");
			Assert.AreEqual ("tcp://" + IPAddress.Loopback.ToString () + ":1236/" + SERVICE_URI, urls [0], "#A6");
			ds = chn.ChannelData as ChannelDataStore;
			Assert.IsNotNull (ds, "#A4");
			Assert.AreEqual (1, ds.ChannelUris.Length, "#A5");
			Assert.AreEqual ("tcp://" + IPAddress.Loopback.ToString () + ":1236", ds.ChannelUris [0], "#A6");

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

		public class MarshalObject : ContextBoundObject
		{
			public MarshalObject ()
			{
			}
		}
	}
}
