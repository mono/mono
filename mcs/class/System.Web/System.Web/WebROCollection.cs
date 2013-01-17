//
// System.Web.WebROCollection
//
// Authors:
//   	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// (c) 2005-2009 Novell, Inc. (http://www.novell.com)
// Copyright 2012 Xamarin, Inc (http://xamarin.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web.Util;

namespace System.Web
{
	class WebROCollection : NameValueCollection
	{
		bool got_id;
		int id;

		public WebROCollection () : base (SecureHashCodeProvider.DefaultInvariant, CaseInsensitiveComparer.DefaultInvariant) { }
		public bool GotID {
			get { return got_id; }
		}

		public int ID {
			get { return id; }
			set {
				got_id = true;
				id = value;
			}
		}
		public void Protect ()
		{
			IsReadOnly = true;
		}

		public void Unprotect ()
		{
			IsReadOnly = false;
		}

		public override string ToString ()
		{
			StringBuilder result = new StringBuilder ();
			foreach (string key in AllKeys) {
				if (result.Length > 0)
					result.Append ('&');

				if (key != null && key.Length > 0){
					result.Append (key);
					result.Append ('=');
				}
				result.Append (Get (key));
			}

			return result.ToString ();
		}
	}
}
