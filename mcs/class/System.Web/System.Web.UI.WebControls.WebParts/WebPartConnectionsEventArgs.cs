//
// System.Web.UI.WebControls.WebParts.WebPartConnectionsEventArgs.cs
//
// Authors:
//   Sanjay Gupta (gsanjay@novell.com)
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


namespace System.Web.UI.WebControls.WebParts
{
	public class WebPartConnectionsEventArgs : EventArgs
	{
		WebPart providerPart;
		WebPart consumerPart;
		ProviderConnectionPoint providerPoint;
		ConsumerConnectionPoint consumerPoint;
		WebPartConnection connection;
		
		public WebPartConnectionsEventArgs (WebPart providerPart, ProviderConnectionPoint providerPoint, 
						WebPart consumerPart, ConsumerConnectionPoint consumerPoint) : this (providerPart, 
						providerPoint, consumerPart, consumerPoint, null)
		{}
		
		public WebPartConnectionsEventArgs (WebPart providerPart, ProviderConnectionPoint providerPoint, 
						WebPart consumerPart, ConsumerConnectionPoint consumerPoint, WebPartConnection connection)
		{
			this.providerPart = providerPart;
			this.providerPoint = providerPoint;
			this.consumerPart = consumerPart;
			this.consumerPoint = consumerPoint;
			this.connection = connection;
		}
		
		public WebPartConnection Connection {
			get { return connection; }
		}
		
		public ConsumerConnectionPoint ConsumerConnectionPoint {
			get { return consumerPoint; }
		}
		
		public WebPart Consumer {
			get { return consumerPart; }
		}
		
		public ProviderConnectionPoint ProviderConnectionPoint {
			get { return providerPoint; }
		}
		
		public WebPart Provider {
			get { return providerPart; }
		}
	}
}

