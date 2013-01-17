//
// CacheControlHeaderValue.cs
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

using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace System.Net.Http.Headers
{
	public class CacheControlHeaderValue : ICloneable
	{
		List<NameValueHeaderValue> extensions;
		List<string> no_cache_headers, private_headers;

		public ICollection<NameValueHeaderValue> Extensions {
			get {
				return extensions ?? (extensions = new List<NameValueHeaderValue> ());
			}
		}

		public TimeSpan? MaxAge { get; set; }

		public bool MaxStale { get; set; }

		public TimeSpan? MaxStaleLimit { get; set; }

		public TimeSpan? MinFresh { get; set; }

		public bool MustRevalidate { get; set; }

		public bool NoCache { get; set; }

		public ICollection<string> NoCacheHeaders {
			get {
				return no_cache_headers ?? (no_cache_headers = new List<string> ());
			}
		}

		public bool NoStore { get; set; }

		public bool NoTransform { get; set; }

		public bool OnlyIfCached { get; set; }

		public bool Private { get; set; }

		public ICollection<string> PrivateHeaders {
			get {
				return private_headers ?? (private_headers = new List<string> ());
			}
		}

		public bool ProxyRevalidate { get; set; }

		public bool Public { get; set; }

		public TimeSpan? SharedMaxAge { get; set; }

		object ICloneable.Clone ()
		{
			var copy = (CacheControlHeaderValue) MemberwiseClone ();
			if (extensions != null) {
				copy.extensions = new List<NameValueHeaderValue> ();
				foreach (var entry in extensions) {
					copy.extensions.Add (entry);
				}
			}

			if (no_cache_headers != null) {
				copy.no_cache_headers = new List<string> ();
				foreach (var entry in no_cache_headers) {
					copy.no_cache_headers.Add (entry);
				}
			}

			if (private_headers != null) {
				copy.private_headers = new List<string> ();
				foreach (var entry in private_headers) {
					copy.private_headers.Add (entry);
				}
			}

			return copy;
		}

		public override bool Equals (object obj)
		{
			var source = obj as CacheControlHeaderValue;
			if (source == null)
				return false;

			if (MaxAge != source.MaxAge || MaxStale != source.MaxStale || MaxStaleLimit != source.MaxStaleLimit ||
				MinFresh != source.MinFresh || MustRevalidate != source.MustRevalidate || NoCache != source.NoCache ||
				NoStore != source.NoStore || NoTransform != source.NoTransform || OnlyIfCached != source.OnlyIfCached ||
				Private != source.Private || ProxyRevalidate != source.ProxyRevalidate || Public != source.Public ||
				SharedMaxAge != source.SharedMaxAge)
				return false;

			return extensions.SequenceEqual (source.extensions) &&
				no_cache_headers.SequenceEqual (source.no_cache_headers) &&
				private_headers.SequenceEqual (source.private_headers);
		}

		public override int GetHashCode ()
		{
			int hc = 29;
			unchecked {
				hc = hc * 29 + HashCodeCalculator.Calculate (extensions);
				hc = hc * 29 + MaxAge.GetHashCode ();
				hc = hc * 29 + MaxStale.GetHashCode ();
				hc = hc * 29 + MaxStaleLimit.GetHashCode ();
				hc = hc * 29 + MinFresh.GetHashCode ();
				hc = hc * 29 + MustRevalidate.GetHashCode ();
				hc = hc * 29 + HashCodeCalculator.Calculate (no_cache_headers);
				hc = hc * 29 + NoCache.GetHashCode ();
				hc = hc * 29 + NoStore.GetHashCode ();
				hc = hc * 29 + NoTransform.GetHashCode ();
				hc = hc * 29 + OnlyIfCached.GetHashCode ();
				hc = hc * 29 + Private.GetHashCode ();
				hc = hc * 29 + HashCodeCalculator.Calculate (private_headers);
				hc = hc * 29 + ProxyRevalidate.GetHashCode ();
				hc = hc * 29 + Public.GetHashCode ();
				hc = hc * 29 + SharedMaxAge.GetHashCode ();
			}

			return hc;
		}

		public static CacheControlHeaderValue Parse (string input)
		{
			CacheControlHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}

		public static bool TryParse (string input, out CacheControlHeaderValue parsedValue)
		{
			parsedValue = null;
			if (input == null)
				return true;

			var value = new CacheControlHeaderValue ();

			var lexer = new Lexer (input);
			Token t;
			do {
				t = lexer.Scan ();
				if (t != Token.Type.Token)
					return false;

				string s = lexer.GetStringValue (t);
				bool token_read = false;
				TimeSpan? ts;
				switch (s) {
				case "no-store":
					value.NoStore = true;
					break;
				case "no-transform":
					value.NoTransform = true;
					break;
				case "only-if-cached":
					value.OnlyIfCached = true;
					break;
				case "public":
					value.Public = true;
					break;
				case "must-revalidate":
					value.MustRevalidate = true;
					break;
				case "proxy-revalidate":
					value.ProxyRevalidate = true;
					break;
				case "max-stale":
					value.MaxStale = true;
					t = lexer.Scan ();
					if (t != Token.Type.SeparatorEqual) {
						token_read = true;
						break;
					}

					t = lexer.Scan ();
					if (t != Token.Type.Token)
						return false;

					ts = lexer.TryGetTimeSpanValue (t);
					if (ts == null)
						return false;

					value.MaxStaleLimit = ts;
					break;
				case "max-age":
				case "s-maxage":
				case "min-fresh":
					t = lexer.Scan ();
					if (t != Token.Type.SeparatorEqual) {
						return false;
					}

					t = lexer.Scan ();
					if (t != Token.Type.Token)
						return false;

					ts = lexer.TryGetTimeSpanValue (t);
					if (ts == null)
						return false;

					switch (s.Length) {
					case 7:
						value.MaxAge = ts;
						break;
					case 8:
						value.SharedMaxAge = ts;
						break;
					default:
						value.MinFresh = ts;
						break;
					}

					break;
				case "private":
				case "no-cache":
					if (s.Length == 7) {
						value.Private = true;
					} else {
						value.NoCache = true;
					}

					t = lexer.Scan ();
					if (t != Token.Type.SeparatorEqual) {
						token_read = true;
						break;
					}

					t = lexer.Scan ();
					if (t != Token.Type.QuotedString)
						return false;

					foreach (var entry in lexer.GetQuotedStringValue (t).Split (',')) {
						var qs = entry.Trim ('\t', ' ');

						if (s.Length == 7) {
							value.PrivateHeaders.Add (qs);
						} else {
							value.NoCache = true;
							value.NoCacheHeaders.Add (qs);
						}
					}
					break;
				default:
					string name = lexer.GetStringValue (t);
					string svalue = null;

					t = lexer.Scan ();
					if (t == Token.Type.SeparatorEqual) {
						t = lexer.Scan ();
						switch (t.Kind) {
						case Token.Type.Token:
						case Token.Type.QuotedString:
							svalue = lexer.GetStringValue (t);
							break;
						default:
							return false;
						}
					} else {
						token_read = true;
					}

					value.Extensions.Add (NameValueHeaderValue.Create (name, svalue));
					break;
				}

				if (!token_read)
					t = lexer.Scan ();
			} while (t == Token.Type.SeparatorComma);

			if (t != Token.Type.End)
				return false;

			parsedValue = value;
			return true;
		}

		public override string ToString ()
		{
			const string separator = ", ";

			var sb = new StringBuilder ();
			if (NoStore) {
				sb.Append ("no-store");
				sb.Append (separator);
			}

			if (NoTransform) {
				sb.Append ("no-transform");
				sb.Append (separator);
			}

			if (OnlyIfCached) {
				sb.Append ("only-if-cached");
				sb.Append (separator);
			}

			if (Public) {
				sb.Append ("public");
				sb.Append (separator);
			}

			if (MustRevalidate) {
				sb.Append ("must-revalidate");
				sb.Append (separator);
			}

			if (ProxyRevalidate) {
				sb.Append ("proxy-revalidate");
				sb.Append (separator);
			}

			if (NoCache) {
				sb.Append ("no-cache");
				if (no_cache_headers != null) {
					sb.Append ("=\"");
					no_cache_headers.ToStringBuilder (sb);
					sb.Append ("\"");
				}

				sb.Append (separator);
			}

			if (MaxAge != null) {
				sb.Append ("max-age=");
				sb.Append (MaxAge.Value.TotalSeconds.ToString (CultureInfo.InvariantCulture));
				sb.Append (separator);
			}

			if (SharedMaxAge != null) {
				sb.Append ("s-maxage=");
				sb.Append (SharedMaxAge.Value.TotalSeconds.ToString (CultureInfo.InvariantCulture));
				sb.Append (separator);
			}

			if (MaxStale) {
				sb.Append ("max-stale");
				if (MaxStaleLimit != null) {
					sb.Append ("=");
					sb.Append (MaxStaleLimit.Value.TotalSeconds.ToString (CultureInfo.InvariantCulture));
				}

				sb.Append (separator);
			}

			if (MinFresh != null) {
				sb.Append ("min-fresh=");
				sb.Append (MinFresh.Value.TotalSeconds.ToString (CultureInfo.InvariantCulture));
				sb.Append (separator);
			}

			if (Private) {
				sb.Append ("private");
				if (private_headers != null) {
					sb.Append ("=\"");
					private_headers.ToStringBuilder (sb);
					sb.Append ("\"");
				}

				sb.Append (separator);
			}

			CollectionExtensions.ToStringBuilder (extensions, sb);

			if (sb.Length > 2 && sb[sb.Length - 2] == ',' && sb[sb.Length - 1] == ' ')
				sb.Remove (sb.Length - 2, 2);

			return sb.ToString ();
		}
	}
}
