//
// System.Runtime.Remoting.Channels.BinaryClientFormatterSinkProvider.cs
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

namespace System.Runtime.Remoting.Channels
{
	public class BinaryClientFormatterSinkProvider :
		IClientFormatterSinkProvider, IClientChannelSinkProvider
	{
		IClientChannelSinkProvider next = null;
		BinaryCore _binaryCore;

		static string[] allowedProperties = new string [] { "includeVersions", "strictBinding", "typeFilterLevel" };

		public BinaryClientFormatterSinkProvider ()
		{
			_binaryCore = BinaryCore.DefaultInstance;
		}

		public BinaryClientFormatterSinkProvider (IDictionary properties,
							  ICollection providerData)
		{
			_binaryCore = new BinaryCore (this, properties, allowedProperties);
		}

		public IClientChannelSinkProvider Next
		{
			get {
				return next;
			}
			
			set {
				next = value;
			}
		}

		public IClientChannelSink CreateSink (IChannelSender channel,
						      string url,
						      object remoteChannelData)
		{
			IClientChannelSink next_sink = null;
			BinaryClientFormatterSink result;
			
			if (next != null)
				next_sink = next.CreateSink (channel, url, remoteChannelData);
			
			result = new BinaryClientFormatterSink (next_sink);
			result.BinaryCore = _binaryCore;

			return result;
		}		
	}
}
