#if NET_2_0
/*
Used to determine Browser Capabilities by the Browsers UserAgent String and related
Browser supplied Headers.
Copyright (C) 2002-Present  Owen Brady (Ocean at owenbrady dot net) 
and Dean Brettle (dean at brettle dot com)

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace System.Web.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	internal abstract class CapabilitiesBuild : ICapabilitiesProcess
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		protected abstract System.Collections.ObjectModel.Collection<string> HeaderNames(System.Collections.ObjectModel.Collection<string> list);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="userAgent"></param>
		/// <param name="initialCapabilities"></param>
		/// <returns></returns>
		public System.Web.Configuration.CapabilitiesResult Process(string userAgent, System.Collections.IDictionary initialCapabilities)
		{
			System.Collections.Specialized.NameValueCollection header;
			header = new System.Collections.Specialized.NameValueCollection(1);
			header.Add("User-Agent", userAgent);
			return Process(header, initialCapabilities);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="initialCapabilities"></param>
		/// <returns></returns>
		public System.Web.Configuration.CapabilitiesResult Process(System.Web.HttpRequest request, System.Collections.IDictionary initialCapabilities)
		{
			if (request != null)
			{
				return Process(request.Headers, initialCapabilities);
			}
			else
			{
				return Process("", initialCapabilities);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="header"></param>
		/// <param name="initialCapabilities"></param>
		/// <returns></returns>
		public abstract System.Web.Configuration.CapabilitiesResult Process(System.Collections.Specialized.NameValueCollection header, System.Collections.IDictionary initialCapabilities);
	}
}
#endif
