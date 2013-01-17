//
// System.Runtime.Remoting.Channels.SoapServerFormatterSinkProvider.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
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
using System.Runtime.Serialization.Formatters;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Channels
{
	public class SoapServerFormatterSinkProvider :
		IServerFormatterSinkProvider, IServerChannelSinkProvider
	{
		private IServerChannelSinkProvider _next;
		SoapCore _soapCore;

		internal static string[] AllowedProperties = new string [] { "includeVersions", "strictBinding", "typeFilterLevel" };

		public SoapServerFormatterSinkProvider ()
		{
			_soapCore = SoapCore.DefaultInstance;
		}

		public SoapServerFormatterSinkProvider (IDictionary properties,
							ICollection providerData)
		{
			_soapCore = new SoapCore (this, properties, AllowedProperties);
		}

		public IServerChannelSinkProvider Next
		{
			get { return _next;	}

			set { _next = value; }
		}

		[ComVisible(false)]
		public TypeFilterLevel TypeFilterLevel
		{
			get { return _soapCore.TypeFilterLevel; }
			set 
			{
				IDictionary props = (IDictionary) ((ICloneable)_soapCore.Properties).Clone ();
				props ["typeFilterLevel"] = value;
				_soapCore = new SoapCore (this, props, AllowedProperties);
			}
		}

		public IServerChannelSink CreateSink (IChannelReceiver channel)
		{
			IServerChannelSink chain = _next.CreateSink(channel);
			SoapServerFormatterSink sinkFormatter = new SoapServerFormatterSink(SoapServerFormatterSink.Protocol.Http, chain, channel);
			sinkFormatter.SoapCore = _soapCore;
			
			return sinkFormatter;
		}

		public void GetChannelData (IChannelDataStore channelData)
		{
			if(_next != null)
				_next.GetChannelData(channelData);
		}
	}
}
