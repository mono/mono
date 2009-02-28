//
// System.Web.HttpResponseHeader.cs 
//
// Author:
//	Chris Toshok (toshok@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Web {

	internal abstract class BaseResponseHeader {
		string headerValue;
		
		public string Value {
			get { return headerValue; }
			set { headerValue = EncodeHeader (value); }
		}
	  
		static bool headerCheckingEnabled;
		
		static BaseResponseHeader () {
#if NET_2_0
			HttpRuntimeSection section = WebConfigurationManager.GetWebApplicationSection ("system.web/httpRuntime") as HttpRuntimeSection;
#else
			HttpRuntimeConfig section = HttpContext.GetAppConfig ("system.web/httpRuntime") as HttpRuntimeConfig;
#endif
			headerCheckingEnabled = section == null || section.EnableHeaderChecking;
		}


		internal BaseResponseHeader (string val)
		{
			Value = val;
		}

		string EncodeHeader (string value)
		{
			if (value == null || value.Length == 0)
				return value;
			
			if (headerCheckingEnabled) {
				StringBuilder ret = new StringBuilder ();
				int len = value.Length;

				for (int i = 0; i < len; i++) {
					switch (value [i]) {
						case '\r':
							ret.Append ("%0d");
							break;

						case '\n':
							ret.Append ("%0a");
							break;

						default:
							ret.Append (value [i]);
							break;
					}
				}

				return ret.ToString ();
			} else
				return value;
		}
		
		internal abstract void SendContent (HttpWorkerRequest wr);
	}

	internal sealed class KnownResponseHeader : BaseResponseHeader {
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

	internal  class UnknownResponseHeader : BaseResponseHeader {
		public string Name;

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

