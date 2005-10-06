// 
// System.Web.HttpValueCollection
//
// Author:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
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
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Util;

namespace System.Web
{
	[Serializable]
	class HttpValueCollection : NameValueCollection
	{
		bool _bHeaders;

		internal HttpValueCollection ()
		{
			_bHeaders = false;
		}

		internal HttpValueCollection (string sData)
		{
			FillFromQueryString (sData, Encoding.UTF8);
			IsReadOnly = true;
		}

		internal HttpValueCollection(string sData, bool ReadOnly, Encoding encoding)
		{
			FillFromQueryString (sData, encoding);
			IsReadOnly = ReadOnly;
		}

		protected HttpValueCollection (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		// string = header1: value1\r\nheader2: value2
		internal void FillFromHeaders (string sHeaders, Encoding encoding)
		{
			_bHeaders = true;
			char [] arrSplitValue = new char [] {':'};
			string sKey, sValue;

			sKey = "";
			sValue = "";

			string [] arrValues = sHeaders.Split (new char [] {'\r', '\n'});
			foreach (string sLine in arrValues) {
				string [] arrKeyValue = sLine.Split (arrSplitValue);
				if (arrKeyValue.Length == 1 && arrKeyValue [0].Length == 0) {
					// Empty \r or \n is ignored
					continue;
				}

				if (arrKeyValue[0] != sKey && sKey.Length > 0) {
					Add (HttpUtility.UrlDecode (sKey, encoding),
					     HttpUtility.UrlDecode (sValue, encoding));
				}

				if (arrKeyValue.Length == 1) {
					sValue += "\r\n" + arrKeyValue [0].Trim();
					continue;
				} else if (arrKeyValue.Length == 2) {
					if (arrKeyValue[0].Length == 0) {
						sValue += arrKeyValue [1].Trim();
						continue;
					}

					sKey = arrKeyValue [0].Trim();
					sValue = arrKeyValue [1].Trim();
				} 
			}

			if (sKey.Length > 0) {
				Add (HttpUtility.UrlDecode (sKey, encoding),
				     HttpUtility.UrlDecode (sValue, encoding));
			}
		}

		internal void FillFromHeaders (string sData)
		{
			FillFromHeaders (sData, Encoding.UTF8);
		}

		// String = test=aaa&kalle=nisse
		internal void FillFromQueryString (string sData, Encoding encoding)
		{
			FillFromQueryString (sData, encoding, true);
		}
		
		void FillFromQueryString (string sData, Encoding encoding, bool decode)
		{
			_bHeaders = false;
			if (sData == null || sData == "")
				return;

			string k, v;
			int eq;
			string [] arrValues = sData.Split (new char [] {'&'});
			foreach (string sValue in arrValues) {
				eq = sValue.IndexOf ('=');

				if (eq == -1) {
					k = sValue.Trim ();
					if (decode)
						k = HttpUtility.UrlDecode (k, encoding);

					Add (k, String.Empty);
					continue;
				}

				k = sValue.Substring (0, eq).Trim ();
				v = String.Empty;
				if (eq + 1 < sValue.Length) {
					v = sValue.Substring (eq + 1).Trim ();
					if (v.Length == 0)
						v = String.Empty;
				}

				if (decode) {
					k = HttpUtility.UrlDecode (k, encoding);
					if (v.Length > 0)
						v = HttpUtility.UrlDecode (v, encoding);
				}

				Add (k, v); 
			}		
		}

		internal void FillFromQueryString (string sData)
		{
			FillFromQueryString (sData, Encoding.UTF8);
		}

		internal void FillFromCookieString (string sData)
		{
			FillFromQueryString (sData, Encoding.UTF8, false);
		}

		internal void MakeReadOnly ()
		{
			IsReadOnly = true;
		}

		internal void MakeReadWrite ()
		{
			IsReadOnly = false;
		}

		internal void Merge (NameValueCollection oData)
		{
			foreach (string sKey in oData)
				Add (sKey, oData [sKey]);
		}

		internal void Reset ()
		{
			Clear ();
		}

		internal string ToString (bool UrlEncode)
		{
			StringBuilder result = new StringBuilder ();
			string eq = (_bHeaders ? ":" : "=");
			string separator = (_bHeaders ? "\r\n" : "&");

			foreach (string strKey in AllKeys) {

				if (result.Length > 0)
					result.Append (separator);

				if (UrlEncode) {
					// use encoding
					result.Append (HttpUtility.UrlEncode (strKey, Encoding.UTF8));
					result.Append (eq);
					result.Append (HttpUtility.UrlEncode (Get (strKey), Encoding.UTF8));
				} else {
					result.Append (strKey);
					result.Append (eq);
					result.Append (Get (strKey));
				}                               
			}

			return result.ToString ();
		}

		public override string ToString ()
		{
			return ToString (false);
		}
	}
}

