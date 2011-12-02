//
// HttpHeaders.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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
using System.Collections.Generic;
using System.Linq;

namespace System.Net.Http.Headers
{
	public abstract class HttpHeaders : IEnumerable<KeyValuePair<string, IEnumerable<string>>>
	{
		class HeaderBucket
		{
			//
			// headers can hold an object of 3 kinds
			// - simple type for parsed single values (e.g. DateTime)
			// - CollectionHeader for multi-value headers
			// - List<string> for not checked single values
			//
			public object Parsed;
			List<string> values;

			public HeaderBucket (object parsed)
			{
				this.Parsed = parsed;
			}

			public bool HasStringValues {
				get {
					return values != null && values.Count > 0;
				}
			}

			public List<string> Values {
				get {
					return values ?? (values = new List<string> ());
				}
			}
		}

		static readonly Dictionary<string, HeaderInfo> known_headers;

		static HttpHeaders ()
		{
			var headers = new[] {
				HeaderInfo.CreateMulti<MediaTypeWithQualityHeaderValue> ("Accept", MediaTypeWithQualityHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<StringWithQualityHeaderValue> ("Accept-Charset", StringWithQualityHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<StringWithQualityHeaderValue> ("Accept-Encoding", StringWithQualityHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<StringWithQualityHeaderValue> ("Accept-Language", StringWithQualityHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<TimeSpan> ("Age", Parser.TimeSpanSeconds.TryParse, HttpHeaderKind.Response),
				HeaderInfo.CreateSingle<AuthenticationHeaderValue> ("Authorization", AuthenticationHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<CacheControlHeaderValue> ("Cache-Control", CacheControlHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<string> ("Connection", Parser.Token.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<DateTimeOffset> ("Date", Parser.DateTime.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<NameValueWithParametersHeaderValue> ("Expect", NameValueWithParametersHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<string> ("From", Parser.EmailAddress.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<Uri> ("Host", Parser.Uri.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<EntityTagHeaderValue> ("If-Match", EntityTagHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<DateTimeOffset> ("If-Modified-Since", Parser.DateTime.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<EntityTagHeaderValue> ("If-None-Match", EntityTagHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<RangeConditionHeaderValue> ("If-Range", RangeConditionHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<DateTimeOffset> ("If-Unmodified-Since", Parser.DateTime.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<int> ("Max-Forwards", Parser.Int.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<NameValueHeaderValue> ("Pragma", NameValueHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<AuthenticationHeaderValue> ("Proxy-Authorization", AuthenticationHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<RangeHeaderValue> ("Range", RangeHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateSingle<Uri> ("Referer", Parser.Uri.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<TransferCodingWithQualityHeaderValue> ("TE", TransferCodingWithQualityHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<string> ("Trailer", Parser.Token.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<TransferCodingHeaderValue> ("Transfer-Encoding", TransferCodingHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<ProductHeaderValue> ("Upgrade", ProductHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<ProductInfoHeaderValue> ("User-Agent", ProductInfoHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<ViaHeaderValue> ("Via", ViaHeaderValue.TryParse, HttpHeaderKind.Request),
				HeaderInfo.CreateMulti<WarningHeaderValue> ("Warning", WarningHeaderValue.TryParse, HttpHeaderKind.Request)
			};

			known_headers = new Dictionary<string, HeaderInfo> (StringComparer.OrdinalIgnoreCase);
			foreach (var header in headers) {
				known_headers.Add (header.Name, header);
			}
		}

		readonly Dictionary<string, HeaderBucket> headers;
		readonly HttpHeaderKind HeaderKind;

		protected HttpHeaders ()
		{
			headers = new Dictionary<string, HeaderBucket> (StringComparer.OrdinalIgnoreCase);
		}

		internal HttpHeaders (HttpHeaderKind headerKind)
			: this ()
		{
			this.HeaderKind = headerKind;
		}

		public void Add (string name, string value)
		{
			Add (name, new[] { value });
		}

		public void Add (string name, IEnumerable<string> values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");

			AddInternal (name, values, CheckName (name), false);
		}

		internal bool AddValue (string value, HeaderInfo headerInfo, bool ignoreInvalid)
		{
			return AddInternal (headerInfo.Name, new [] { value }, headerInfo, ignoreInvalid);
		}

		bool AddInternal (string name, IEnumerable<string> values, HeaderInfo headerInfo, bool ignoreInvalid)
		{
			HeaderBucket bucket;
			headers.TryGetValue (name, out bucket);
			bool ok = true;

			foreach (var value in values) {
				bool first_entry = bucket == null;

				if (headerInfo != null) {
					object parsed_value;
					if (!headerInfo.TryParse (value, out parsed_value)) {
						if (ignoreInvalid) {
							ok = false;
							continue;
						}

						throw new FormatException ();
					}

					if (headerInfo.AllowsMany) {
						if (bucket == null)
							bucket = new HeaderBucket (headerInfo.CreateCollection (this));

						headerInfo.AddToCollection (bucket.Parsed, parsed_value);
					} else {
						if (bucket != null)
							throw new FormatException ();

						bucket = new HeaderBucket (parsed_value);
					}
				} else {
					if (bucket == null)
						bucket = new HeaderBucket (null);

					bucket.Values.Add (value ?? string.Empty);
				}

				if (first_entry) {
					headers.Add (name, bucket);
				}
			}

			return ok;
		}

		public void AddWithoutValidation (string name, string value)
		{
			AddWithoutValidation (name, new[] { value });
		}

		public void AddWithoutValidation (string name, IEnumerable<string> values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");

			CheckName (name);
			AddInternal (name, values, null, true);
		}

		HeaderInfo CheckName (string name)
		{
			if (string.IsNullOrEmpty (name))
				throw new ArgumentException ("name");

			if (!Parser.Token.IsValid (name))
				throw new FormatException ();

			HeaderInfo headerInfo;
			if (known_headers.TryGetValue (name, out headerInfo) && (headerInfo.HeaderKind & HeaderKind) == 0)
				throw new InvalidOperationException (name);

			return headerInfo;
		}

		public void Clear ()
		{
			headers.Clear ();
		}

		public bool Contains (string name)
		{
			CheckName (name);

			return headers.ContainsKey (name);
		}

		public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator ()
		{
			foreach (var entry in headers) {
				IEnumerable<string> values = null; // TODO:
				yield return new KeyValuePair<string, IEnumerable<string>> (entry.Key, values);
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public IEnumerable<string> GetValues (string name)
		{
			IEnumerable<string> values;
			if (!TryGetValues (name, out values))
				throw new InvalidOperationException ();

			return values;
		}

		public bool Remove (string name)
		{
			CheckName (name);
			return headers.Remove (name);
		}

		public bool TryGetValues (string name, out IEnumerable<string> values)
		{
			var header_info = CheckName (name);

			HeaderBucket bucket;
			if (!headers.TryGetValue (name, out bucket)) {
				values = null;
				return false;
			}

			List<string> string_values = new List<string> ();
			if (header_info != null && header_info.AllowsMany) {
				header_info.AddToStringCollection (string_values, bucket.Parsed);
			} else {
				if (bucket.Parsed != null)
					string_values.Add (bucket.Parsed.ToString ());
			}

			if (bucket.HasStringValues)
				string_values.AddRange (bucket.Values);

			values = string_values;
			return true;
		}

		internal void AddOrRemove (string name, string value)
		{
			if (string.IsNullOrEmpty (value))
				Remove (name);
			else
				SetValue (name, value);
		}

		internal void AddOrRemove<T> (string name, T value) where T : class
		{
			if (value == null)
				Remove (name);
			else
				SetValue (name, value);
		}

		internal void AddOrRemove<T> (string name, T? value) where T : struct
		{
			if (!value.HasValue)
				Remove (name);
			else
				SetValue (name, value);
		}

		internal T GetValue<T> (string name)
		{
			HeaderBucket value;

			if (!headers.TryGetValue (name, out value))
				return default (T);

			var res = (T) value.Parsed;
			if (value.HasStringValues && typeof (T) == typeof (string) && (object) res == null)
				res = (T) (object) value.Values[0];

			return res;
		}

		internal HttpHeaderValueCollection<T> GetValues<T> (string name) where T : class
		{
			HeaderBucket value;

			if (!headers.TryGetValue (name, out value)) {
				value = new HeaderBucket (new HttpHeaderValueCollection<T> (this, known_headers [name]));
				headers.Add (name, value);
			}

			return (HttpHeaderValueCollection<T>) value.Parsed;
		}

		void SetValue<T> (string name, T value)
		{
			headers[name] = new HeaderBucket (value);
		}
	}
}
