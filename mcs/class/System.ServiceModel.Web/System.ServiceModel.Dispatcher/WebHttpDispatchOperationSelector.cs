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
using System.Collections.Generic;
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

		Dictionary<string,UriTemplateTable> tables = new Dictionary<string,UriTemplateTable> ();

		protected WebHttpDispatchOperationSelector ()
		{
		}

		public WebHttpDispatchOperationSelector (ServiceEndpoint endpoint)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
			if (endpoint.Address == null)
				throw new InvalidOperationException ("EndpointAddress must be set in the argument ServiceEndpoint");

			foreach (OperationDescription od in endpoint.Contract.Operations) {
				WebAttributeInfo info = od.GetWebAttributeInfo ();
				if (info != null) {
					UriTemplateTable table;
					if (!tables.TryGetValue (info.Method, out table)) {
						 table = new UriTemplateTable (endpoint.Address.Uri);
						 tables.Add (info.Method, table);
					}
					table.KeyValuePairs.Add (new TemplateTablePair (info.BuildUriTemplate (od, null), od));
				}
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
			// FIXME: something like this check is required here or somewhere else. See WebHttpDispatchOperationSelectorTest.SelectOperationCheckExistingProperty()
//			if (message.Properties.ContainsKey (WebBodyFormatMessageProperty.Name))
//				throw new ArgumentException ("There is already message property for Web body format");
			uriMatched = false;
			Uri to = message.Headers.To;
			if (to == null)
				return String.Empty;

			// First, UriTemplateTable with corresponding HTTP method is tested. Then other tables can be tested for match.
			UriTemplateTable table;
			var hp = (HttpRequestMessageProperty) message.Properties [HttpRequestMessageProperty.Name];
			tables.TryGetValue (hp == null ? null : hp.Method, out table);
			UriTemplateMatch match = table == null ? null : table.MatchSingle (to);
			if (match == null)
				foreach (var tab in tables.Values)
					if ((match = tab.MatchSingle (to)) != null) {
						table = tab;
						break;
					}

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
