//
// System.Web.Configuation.TraceConfigurationHandler
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Web;
using System.Xml;
using System.Configuration;

namespace System.Web.Configuration {

	internal class TraceConfigurationHandler : IConfigurationSectionHandler {

		public object Create (object parent, object context, XmlNode section)
		{
			TraceConfig config = new TraceConfig ();

			string enabled_str = AttValue ("enabled", section);
			if (enabled_str != null) {
				try {
					config.Enabled = Boolean.Parse (enabled_str);
				} catch {
					ThrowException ("The 'enabled' attribute is case sensitive" +
							" and must be set to 'true' or 'false'.", section);
				}
			}

			string local_str = AttValue ("localOnly", section);
			if (local_str != null) {
				try {
					config.LocalOnly = Boolean.Parse (local_str);
				} catch {
					ThrowException ("The 'localOnly' attribute is case sensitive" +
							" and must be set to 'true' or 'false'.", section);
				}
			}

			string page_str = AttValue ("pageOutput", section);
			if (page_str != null) {
				try {
					config.PageOutput = Boolean.Parse (page_str);
				} catch {
					ThrowException ("The 'pageOutput' attribute is case sensitive" +
							" and must be set to 'true' or 'false'.", section);
				}
			}

			string limit_str = AttValue ("requestLimit", section);
			if (limit_str != null) {
				try {
					config.RequestLimit = Int32.Parse (limit_str);
				} catch {
					ThrowException ("The 'requestLimit' attribute must be an integer value.",
							section);
				}
			}

			string trace_str = AttValue ("traceMode", section);
			if (trace_str != null) {
				try {
					config.TraceMode = (TraceMode) Enum.Parse (typeof (TraceMode), trace_str);
				} catch {
					ThrowException ("The 'traceMode' attribute is case sensitive and must be" +
							" one of the following values: SortByTime, SortByCategory.",
							section);
				}
			}
			
			return config;
		}

		// A few methods to save some typing
		static string AttValue (string name, XmlNode node)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, true);
		}

		static void ThrowException (string message, XmlNode node)
		{
			HandlersUtil.ThrowException (message, node);
		}
	}
}

