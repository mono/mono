//
// System.Net.NetworkInformation.UdpStatistics
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (c) 2006-2007 Novell, Inc. (http://www.novell.com)
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
using System.Collections.Specialized;
using System.Globalization;

namespace System.Net.NetworkInformation {
	class MibUdpStatistics : UdpStatistics
	{
		StringDictionary dic;

		public MibUdpStatistics (StringDictionary dic)
		{
			this.dic = dic;
		}

		long Get (string name)
		{
			return dic [name] != null ? long.Parse (dic [name], NumberFormatInfo.InvariantInfo) : 0;
		}

		public override long DatagramsReceived {
			get { return Get ("InDatagrams"); }
		}
		public override long DatagramsSent {
			get { return Get ("OutDatagrams"); }
		}
		public override long IncomingDatagramsDiscarded {
			get { return Get ("NoPorts"); }
		}
		public override long IncomingDatagramsWithErrors {
			get { return Get ("InErrors"); }
		}
		public override int UdpListeners {
			get { return (int) Get ("NumAddrs"); }
		}
	}
}
