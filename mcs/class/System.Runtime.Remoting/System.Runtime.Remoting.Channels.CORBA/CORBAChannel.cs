//
// System.Runtime.Remoting.Channels.CORBA.CORBAChannel.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

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

using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;

namespace System.Runtime.Remoting.Channels.CORBA
{
	public class CORBAChannel : IChannelReceiver, IChannel,
		IChannelSender
	{
		CORBAServerChannel svr_chnl;
		CORBAClientChannel cnt_chnl;
		
		string name = "corba";

		public CORBAChannel ()
	        {
			svr_chnl = new CORBAServerChannel (0);
			cnt_chnl = new CORBAClientChannel ();
		}

		public CORBAChannel (int port)
		{
			svr_chnl = new CORBAServerChannel (port);
			cnt_chnl = new CORBAClientChannel ();
		}

		[MonoTODO]
		public CORBAChannel (IDictionary properties,
				      IClientChannelSinkProvider clientSinkProvider,
				      IServerChannelSinkProvider serverSinkProvider)
		{
			throw new NotImplementedException ();
		}

		public object ChannelData
		{
			get {
				return svr_chnl.ChannelData;
			}
		}

		public string ChannelName
		{
			get {
				return name;
			}
		}

		public int ChannelPriority
		{
			get {
				return svr_chnl.ChannelPriority;
			}
		}

		public IMessageSink CreateMessageSink (string url,
						       object remoteChannelData,
						       out string objectURI)
	        {
			return cnt_chnl.CreateMessageSink (url, remoteChannelData, out objectURI);
		}

		public string[] GetUrlsForUri (string objectURI)
		{
			return svr_chnl.GetUrlsForUri (objectURI);
		}

		public string Parse (string url, out string objectURI)
		{
			return svr_chnl.Parse (url, out objectURI);
		}

		public void StartListening (object data)
		{
			svr_chnl.StartListening (data);
		}

		public void StopListening (object data)
		{
			svr_chnl.StopListening (data);
		}

		internal static string ParseCORBAURL (string url, out string objectURI, out int port)
		{
			// format: "corba://host:port/path/to/object"
			
			objectURI = null;
			port = 0;
			
			Match m = Regex.Match (url, "corba://([^:]+):([0-9]+)(/.*)");

			if (!m.Success)
				return null;
			
			string host = m.Groups[1].Value;
			string port_str = m.Groups[2].Value;
			objectURI = m.Groups[3].Value;
			port = Convert.ToInt32 (port_str);
				
			return host;
		}
	}
}
