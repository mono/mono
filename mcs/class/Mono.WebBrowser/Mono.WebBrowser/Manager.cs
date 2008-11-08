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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Reflection;

namespace Mono.WebBrowser
{
	public sealed class Manager
	{
		public static IWebBrowser GetNewInstance ()
		{
			return Manager.GetNewInstance (Platform.Winforms);
		}

		public static IWebBrowser GetNewInstance (Platform platform)
		{
			string browserEngine = Environment.GetEnvironmentVariable ("MONO_BROWSER_ENGINE");

#if NET_2_0			
			if (browserEngine == "webkit") {
				Assembly ass;
				try {
					ass = Assembly.LoadWithPartialName ("mono-webkit");
					IWebBrowser ret = (IWebBrowser) ass.CreateInstance ("Mono.WebKit.WebBrowser");
					return ret;
				} catch {
					//throw new Exception (Mono.WebBrowser.Exception.ErrorCodes.EngineNotSupported, browserEngine);
					browserEngine = null;
				}
			}
#endif
			if (browserEngine == null || browserEngine == "mozilla")
				return new Mono.Mozilla.WebBrowser (platform);
			throw new Exception (Mono.WebBrowser.Exception.ErrorCodes.EngineNotSupported, browserEngine);
		}

		
	}
}
