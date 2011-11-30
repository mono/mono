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
using System.Linq;

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

			return Enumerable.SequenceEqual (extensions, source.extensions) &&
				Enumerable.SequenceEqual (no_cache_headers, source.no_cache_headers) &&
				Enumerable.SequenceEqual (private_headers, source.private_headers);
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
			throw new NotImplementedException ();
		}
	}
}
