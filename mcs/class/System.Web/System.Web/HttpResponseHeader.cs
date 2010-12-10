//
// System.Web.HttpResponseHeader.cs 
//
// Author:
//	Chris Toshok (toshok@novell.com)
//

//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System.Text;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web
{
	abstract class BaseResponseHeader
	{
		string headerValue;
		
		public string Value {
			get { return headerValue; }
			set {
				string hname, hvalue;
#if NET_4_0
				HttpEncoder.Current.HeaderNameValueEncode (null, value, out hname, out hvalue);
#else
				HttpEncoder.HeaderNameValueEncode (null, value, out hname, out hvalue);
#endif
				headerValue = hvalue;
			}
		}
/*	  
		static bool headerCheckingEnabled;
		
		static BaseResponseHeader ()
		{
			HttpRuntimeSection section = HttpRuntime.Section;
			headerCheckingEnabled = section == null || section.EnableHeaderChecking;
		}
*/

		internal BaseResponseHeader (string val)
		{
			Value = val;
		}
		
		internal abstract void SendContent (HttpWorkerRequest wr);
	}

	internal sealed class KnownResponseHeader : BaseResponseHeader
	{
		public int ID;

		internal KnownResponseHeader (int ID, string val) : base (val)
		{
			this.ID = ID;
		}

		internal override void SendContent (HttpWorkerRequest wr)
		{
			wr.SendKnownResponseHeader (ID, Value);
		}
	}

	internal sealed class UnknownResponseHeader : BaseResponseHeader
	{
		string headerName;
		
		public string Name {
			get { return headerName; }
			set {
				string hname, hvalue;
#if NET_4_0
				HttpEncoder.Current.HeaderNameValueEncode (value, null, out hname, out hvalue);
#else
				HttpEncoder.HeaderNameValueEncode (value, null, out hname, out hvalue);
#endif
				headerName = hname;
			}
		}
		

		public UnknownResponseHeader (string name, string val) : base (val)
		{
			Name = name;
		}
	
		internal override void SendContent (HttpWorkerRequest wr)
		{
			wr.SendUnknownResponseHeader (Name, Value);
		}
		
	}
}

