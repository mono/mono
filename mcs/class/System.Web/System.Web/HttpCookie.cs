// 
// System.Web.HttpCookie
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
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
using System.Globalization;
using System.Text;
using System.Web;
using System.Collections.Specialized;

namespace System.Web
{
	public sealed class HttpCookie
	{
		string _Name;
		string _Value;
		string _Domain;
		DateTime _Expires;
		bool _ExpiresSet;
		string _Path;  
		bool _Secure = false;

		HttpValueCollection _Values;

		internal HttpCookie ()
		{
			_Path = "/";
		}

		public HttpCookie (string name)
		{
			_Path = "/";
			_Name = name;
		}

		public HttpCookie (string name, string value)
		{
			_Name = name;
			_Value = value;
			_Path = "/";
		}

		internal HttpCookie (string name, string value, string path, DateTime expires)
		{
			_Name = name;
			_Value = value;
			_Path = path;
			if (expires != DateTime.MinValue)
				Expires = expires;
		}
		
		internal HttpResponseHeader GetCookieHeader ()
		{
			StringBuilder oSetCookie = new StringBuilder ();

			if (null != _Name && _Name.Length > 0) {
				oSetCookie.Append (_Name);
				oSetCookie.Append ("=");
			}

			if (null != _Values) {
				oSetCookie.Append (_Values.ToString (false));
			} else if (null != _Value) {
				oSetCookie.Append (_Value);
			}

			if (null != _Domain && _Domain.Length > 0) {
				oSetCookie.Append ("; domain=");
				oSetCookie.Append (_Domain);
			}

			if (null != _Path && Path.Length > 0) {
				oSetCookie.Append ("; path=");
				oSetCookie.Append (_Path);
			}

			if (_ExpiresSet && _Expires != DateTime.MinValue) {
				oSetCookie.Append ("; expires=");
				DateTime ut = _Expires.ToUniversalTime ();
				oSetCookie.Append (ut.ToString ("ddd, dd-MMM-yyyy HH':'mm':'ss 'GMT'",
								DateTimeFormatInfo.InvariantInfo));
			}

			if (_Secure)
				oSetCookie.Append ("; secure");

			return new HttpResponseHeader (HttpWorkerRequest.HeaderSetCookie, oSetCookie.ToString());
		}

		public string Domain
		{
			get { return _Domain; }
			set { _Domain = value; }
		}

		public DateTime Expires
		{
			get {
				if (!_ExpiresSet)
					return DateTime.MinValue;

				return _Expires;
			}

			set {
				_ExpiresSet = true;
				_Expires = value;
			}
		}

		public bool HasKeys
		{
			get {
				return Values.HasKeys ();
			}
		}

		public string this [string key]
		{
			get { return Values [key]; }
			set { Values [key] = value; }
		}

		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		public string Path
		{
			get { return _Path; }
			set { _Path = value; }
		}

		public bool Secure
		{
			get { return _Secure; }
			set { _Secure = value; }
		}

		public string Value
		{
			get {
				if (null != _Values)
					return _Values.ToString (false);

				return _Value;
			}

			set {
				if (null != _Values) {
					_Values.Reset ();
					_Values.Add (null, value);
					return;
				}

				_Value = value;
			}
		}

		public NameValueCollection Values
		{
			get {
				if (null == _Values) {
					_Values = new HttpValueCollection ();
					if (null != _Value) {
						// Do we have multiple keys
						if (_Value.IndexOf ("&") >= 0 || _Value.IndexOf ("=") >= 0) {
							_Values.FillFromCookieString (_Value);
						} else {
							_Values.Add (null, _Value);
						}

						_Value = null;
					}
				}

				return _Values;
			}
		}
	}
}

