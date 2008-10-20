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
		NameValueCollection _queryString;
		NameValueCollection _form;
		NameValueCollection _serverVariables;
		HttpCookieCollection _cookies;
		bool _merged;

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
			Protect ();
		}

		public override string Get (string name)
		{
			MergeCollections ();
			return base.Get (name);
		}

		void MergeCollections ()
		{
			if (_merged)
				return;			

			Unprotect ();

			Add (_queryString);
			Add (_form);
			Add (_serverVariables);

			/* special handling for Cookies since
			 * it isn't a NameValueCollection. */
			for (int i = 0; i < _cookies.Count; i++) {
				HttpCookie cookie = _cookies [i];
				Add (cookie.Name, cookie.Value);
			}

			_merged = true;

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

		public override string[] GetValues (int index)
                {
			MergeCollections ();
			return base.GetValues (index);
                }
                
                public override string[] GetValues (string name)
                {
			MergeCollections ();
			return base.GetValues (name);
                }
		
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new SerializationException ();
		}

		public override string [] AllKeys
		{
			get {
				MergeCollections ();
				return base.AllKeys;
			}
		}

		public override int Count
		{
			get {
				MergeCollections ();
				return base.Count;
			}
		}
	}
}
