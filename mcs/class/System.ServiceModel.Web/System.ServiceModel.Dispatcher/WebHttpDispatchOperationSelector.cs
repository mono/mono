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
using System.Linq;
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
			// FIXME: uriMatched should be used to differentiate 404 and 405 (method not allowed)
			bool uriMatched;
			return SelectOperation (ref message, out uriMatched);
		}

		protected virtual string SelectOperation (ref Message message, out bool uriMatched)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (message.Properties.ContainsKey (UriMatchedProperty.Name))
				throw new ArgumentException (String.Format ("There is already a message property for template-matched URI '{0}'", UriMatchedProperty.Name));
			uriMatched = false;
			var hp = (HttpRequestMessageProperty) message.Properties [HttpRequestMessageProperty.Name];

			OperationDescription od = null;
			Message dummy = message; // since lambda expression prohibits ref variable...

			Uri to = message.Headers.To;
			// First, UriTemplateTable with corresponding HTTP method is tested. Then other tables can be tested for match.
			UriTemplateTable table = null;
			if (hp != null && hp.Method != null)
				tables.TryGetValue (hp.Method, out table);
			// FIXME: looks like To header does not matter on .NET. (But how to match then? I have no idea)
			UriTemplateMatch match = to == null || table == null ? null : table.MatchSingle (to);
			if (to != null && match == null) {
				foreach (var tab in tables.Values)
					if ((match = tab.MatchSingle (to)) != null) {
						table = tab;
						break;
					}
			}

			if (match != null) {
				uriMatched = true;
				foreach (TemplateTablePair p in table.KeyValuePairs)
					if (p.Key == match.Template)
						od = p.Value as OperationDescription;
			}
			if (od != null)
				message.Properties.Add (UriMatchedProperty.Name, new UriMatchedProperty ());

			return uriMatched && od != null ? od.Name : String.Empty;
		}

		internal class UriMatchedProperty
		{
			public static string Name = "UriMatched"; // this is what .NET uses for MessageProperty that represents a matched URI.
		}
	}
}
