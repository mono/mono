// 
// System.Web.HttpValueCollection
//
// Author:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
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
			FillFromQueryString (sData, WebEncoding.Encoding);
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
			FillFromHeaders (sData, WebEncoding.Encoding);
		}

		// String = test=aaa&kalle=nisse
		internal void FillFromQueryString (string sData, Encoding encoding)
		{
			_bHeaders = false;

			string k, v;
			int eq;
			string [] arrValues = sData.Split (new char [] {'&'});
			foreach (string sValue in arrValues) {
				eq = sValue.IndexOf ('=');
				if (eq == 0)
					throw new InvalidOperationException ("Data is malformed");

				if (eq == -1) {
					k = HttpUtility.UrlDecode (sValue.Trim (), encoding);
					Add (k, String.Empty);
					continue;
				}

				k = sValue.Substring (0, eq).Trim ();
				if (k.Length == 0)
					throw new InvalidOperationException ("Data is malformed");

				v = String.Empty;
				if (eq + 1 < sValue.Length) {
					v = sValue.Substring (eq + 1).Trim ();
					if (v.Length == 0)
						v = String.Empty;
				}

				k = HttpUtility.UrlDecode (k, encoding);
				if (v.Length > 0)
					v = HttpUtility.UrlDecode (v, encoding);

				Add (k, v); 
			}		
		}

		internal void FillFromQueryString (string sData)
		{
			FillFromQueryString (sData, WebEncoding.Encoding);
		}

		internal void FillFromCookieString (string sData)
		{
			FillFromQueryString (sData, WebEncoding.Encoding);
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

		[MonoTODO("string ToString(bool UrlEncode)")]
		internal string ToString (bool UrlEncode)
		{
			if (_bHeaders) {
			}

			// TODO: Should return a correctly formated string (different depending on header flag)
			throw new NotImplementedException ();
		}

		virtual new public string ToString ()
		{
			return ToString (false);
		}
	}
}

