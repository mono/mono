//
// WebHttpDispatchOperationSelector.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

using TemplateTablePair = System.Collections.Generic.KeyValuePair<System.UriTemplate, object>;

namespace System.ServiceModel.Dispatcher
{
	public class WebHttpDispatchOperationSelector : IDispatchOperationSelector
	{
		public const string HttpOperationSelectorUriMatchedPropertyName = "UriMatched";

		UriTemplateTable table;

		protected WebHttpDispatchOperationSelector ()
		{
		}

		public WebHttpDispatchOperationSelector (ServiceEndpoint endpoint)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
			if (endpoint.Address == null)
				throw new InvalidOperationException ("EndpointAddress must be set in the argument ServiceEndpoint");

			table = new UriTemplateTable (endpoint.Address.Uri);

			foreach (OperationDescription od in endpoint.Contract.Operations) {
				WebAttributeInfo info = od.GetWebAttributeInfo ();
				if (info != null)
					table.KeyValuePairs.Add (new TemplateTablePair (info.BuildUriTemplate (od, null), od));
			}
		}

		public string SelectOperation (ref Message message)
		{
			bool dummy;
			return SelectOperation (ref message, out dummy);
		}

		protected virtual string SelectOperation (ref Message message, out bool uriMatched)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (message.Properties.ContainsKey (WebBodyFormatMessageProperty.Name))
				throw new ArgumentException ("There is already message property for Web body format");
			uriMatched = false;
			Uri to = message.Headers.To;
			if (to == null)
				return String.Empty;

			UriTemplateMatch match = table.MatchSingle (to);
			OperationDescription od = null;
			if (match != null) {
				uriMatched = true;
				foreach (TemplateTablePair p in table.KeyValuePairs)
					if (p.Key == match.Template)
						od = p.Value as OperationDescription;
			}
			return od != null ? od.Name : String.Empty;
		}
	}
}
