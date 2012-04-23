//
// DelegatingHandler.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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

using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
{
	public abstract class DelegatingHandler : HttpMessageHandler
	{
		bool disposed;
		
		protected DelegatingHandler ()
		{
		}
		
		protected DelegatingHandler(HttpMessageHandler innerHandler)
		{
			if (innerHandler == null)
				throw new ArgumentNullException ("innerHandler");
			
			InnerHandler = innerHandler;
		}
		
		public HttpMessageHandler InnerHandler { get; set; }
		
		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				disposed = true;
				InnerHandler.Dispose ();
			}
			
			base.Dispose (disposing);
		}

		protected internal override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return InnerHandler.SendAsync (request, cancellationToken);
		}
	}
}
