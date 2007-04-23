//
// System.Web.HttpParamsCollection
//
// Authors:
//   Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2006 Mainsoft Co. (http://www.mainsoft.com)
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
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace System.Web
{
	internal class HttpParamsCollection : WebROCollection
	{
		private NameValueCollection _queryString;
		private NameValueCollection _form;
		private NameValueCollection _serverVariables;
		private HttpCookieCollection _cookies;
		private bool _merged;

		public HttpParamsCollection (NameValueCollection queryString,
									NameValueCollection form,
									NameValueCollection serverVariables,
									HttpCookieCollection cookies)
		{
			_queryString = queryString;
			_form = form;
			_serverVariables = serverVariables;
			_cookies = cookies;
			_merged = false;
			IsReadOnly = true;
		}

		public override string Get (string name)
		{
			if (_merged)
				return base.Get (name);

			string values = null;

			string query_value = _queryString [name];
			if (query_value != null)
				values += query_value;

			string form_value = _form [name];
			if (form_value != null)
				values += "," + form_value;

			string servar_value = _serverVariables [name];
			if (servar_value != null)
				values += "," + servar_value;

			HttpCookie answer = _cookies [name];
			string cookie_value = ((answer == null) ? null : answer.Value);
			if (cookie_value != null)
				values += "," + cookie_value;

			if (values == null)
				return null;

			if (values.Length > 0 && values [0] == ',')
				return values.Substring (1);

			return values;
		}

		private void MergeCollections ()
		{
			if (_merged)
				return;

			Unprotect ();

			Add (_queryString);
			Add (_form);
			Add (_serverVariables);

			/* special handling for Cookies since
			 * it isn't a NameValueCollection. */
			string [] cookies = _cookies.AllKeys;
			foreach (string k in cookies)
				Add (k, _cookies [k].Value);

			Protect ();
		}

		public override string Get (int index)
		{
			MergeCollections ();
			return base.Get (index);
		}

		public override string GetKey (int index)
		{
			MergeCollections ();
			return base.GetKey (index);
		}

		public override string [] GetValues (int index)
		{
			string value = Get (index);
			if (value == null)
				return null;

			return value.Split (',');
		}

		public override string [] GetValues (string name)
		{
			string value = Get (name);
			if (value == null)
				return null;

			return value.Split (',');
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new SerializationException ();
		}

		public override string [] AllKeys
		{
			get
			{
				MergeCollections ();
				return base.AllKeys;
			}
		}

		public override int Count
		{
			get
			{
				MergeCollections ();
				return base.Count;
			}
		}
	}
}
