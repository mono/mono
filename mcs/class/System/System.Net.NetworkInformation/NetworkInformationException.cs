//
// System.Net.NetworkInformation.NetworkInformationException
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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
using System.ComponentModel;
using System.Runtime.Serialization;

namespace System.Net.NetworkInformation {
	[Serializable]
	public class NetworkInformationException
#if !NET_2_1
		: Win32Exception
#else
		: Exception
#endif
	{
		int error_code;
		
		public NetworkInformationException ()
		{
		}

#if !NET_2_1
		public NetworkInformationException (int errorCode) : base (errorCode)
		{
		}
#else
		public NetworkInformationException (int errorCode)
		{
			error_code = errorCode;
		}
#endif

#if !NET_2_1
		protected NetworkInformationException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			error_code = info.GetInt32 ("ErrorCode");
		}
#endif

		public
#if !NET_2_1
		override
#endif
		int ErrorCode {
			get { return error_code; }
		}
	}
}

