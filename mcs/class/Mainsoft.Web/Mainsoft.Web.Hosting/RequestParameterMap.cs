//
// Mainsoft.Web.Hosting.RequestParameterMap.cs
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//
// (C) 2008 Mainsoft Co. (http://www.mainsoft.com)
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

using System;
using java.util;
using System.Collections.Specialized;
using System.Collections;
namespace Mainsoft.Web.Hosting
{
	partial class BaseExternalContext
	{
		abstract class BaseRequestParameterMap : AbstractAttributeMap
		{
			readonly NameValueCollection _form;

			protected BaseRequestParameterMap (NameValueCollection form) {
				_form = form;
			}

			protected NameValueCollection Form {
				get { return _form; }
			}

			protected override void setAttribute (String key, Object value) {
				throw new NotSupportedException (
					"Cannot set Request Parameter");
			}

			protected override void removeAttribute (String key) {
				throw new NotSupportedException (
					"Cannot remove Request Parameter");
			}

			protected override Enumeration getAttributeNames () {
				return new EnumerationImpl (_form.Keys.GetEnumerator ());
			}

			sealed class EnumerationImpl : Enumeration
			{
				readonly IEnumerator _en;
				public EnumerationImpl (IEnumerator en) {
					_en = en;
				}

				#region Enumeration Members

				public bool hasMoreElements () {
					return _en.MoveNext ();
				}

				public object nextElement () {
					return _en.Current;
				}

				#endregion
			}
		}

		sealed class RequestParameterMap : BaseRequestParameterMap
		{
			public RequestParameterMap (NameValueCollection form)
				: base (form) {
			}

			protected override Object getAttribute (String key) {
				return Form [key];
			}
		}

		sealed class IEnumeratorIteratorImpl : Iterator
		{
			readonly private IEnumerator _en;

			public IEnumeratorIteratorImpl (IEnumerator en) {
				_en = en;
			}

			public bool hasNext () {
				return _en.MoveNext ();
			}

			public Object next () {
				return _en.Current;
			}

			public void remove () {
				throw new NotSupportedException (GetType ().Name + " UnsupportedOperationException");
			}
		}
	}
}