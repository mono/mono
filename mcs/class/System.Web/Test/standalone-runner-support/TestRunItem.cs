//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2010 Novell, Inc http://novell.com/
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
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Hosting;

namespace StandAloneRunnerSupport
{
	public sealed class TestRunItem
	{
		public TestCallback Callback {
			get; set;
		}

		public object TestRunData {
			get; set;
		}
		
		public string Url {
			get; set;
		}

		public string PathInfo {
			get; set;
		}
		
		public string UrlDescription {
			get; set;
		}
#if BUG_IN_THE_RUNTIME_IS_FIXED
		public SerializedDictionary <string, string> PostValues {
			get; set;
		}
#else
		public string[] PostValues {
			get; set;
		}
#endif
		public TestRunItem ()
		: this (null, null, null)
		{}
		
		public TestRunItem (string url, TestCallback callback)
		: this (url, null, callback)
		{}

		public TestRunItem (string url, string urlDescription, TestCallback callback)
		{
			Url = url;
			Callback = callback;
			UrlDescription = urlDescription;
		}
	}
}
