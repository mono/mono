//
// System.Runtime.Remoting.Channels.CORBA.CORBAServerFormatterSinkProvider.cs
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

namespace System.Runtime.Remoting.Channels.CORBA
{
	public class CORBAServerFormatterSinkProvider :
		IServerFormatterSinkProvider, IServerChannelSinkProvider
	{
		IServerChannelSinkProvider next = null;

		public CORBAServerFormatterSinkProvider ()
		{
		}

		[MonoTODO]
		public CORBAServerFormatterSinkProvider (IDictionary properties,
							 ICollection providerData)
	        {
			throw new NotImplementedException ();
		}

		public IServerChannelSinkProvider Next
		{
			get {
				return next;
			}

			set {
				next = value;
			}
		}

		public IServerChannelSink CreateSink (IChannelReceiver channel)
		{
			IServerChannelSink next_sink = null;
			CORBAServerFormatterSink result;
			
			if (next != null)
				next_sink = next.CreateSink (channel);
			
			result = new CORBAServerFormatterSink (next_sink);

			// set properties on result
			
			return result;
		}

		public void GetChannelData (IChannelDataStore channelData)
		{
			// no idea why we need this
		}
	}
}
