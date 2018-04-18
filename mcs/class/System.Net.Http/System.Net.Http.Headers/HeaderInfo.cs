//
// HeaderInfo.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011, 2014 Xamarin Inc (http://www.xamarin.com)
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

using System.Diagnostics;
using System.Collections.Generic;

namespace System.Net.Http.Headers
{
	delegate bool TryParseDelegate<T> (string value, out T result);
	delegate bool TryParseListDelegate<T> (string value, int minimalCount, out List<T> result);

	abstract class HeaderInfo
	{
		class HeaderTypeInfo<T, U> : HeaderInfo where U : class
		{
			readonly TryParseDelegate<T> parser;

			public HeaderTypeInfo (string name, TryParseDelegate<T> parser, HttpHeaderKind headerKind)
				: base (name, headerKind)
			{
				this.parser = parser;
			}

			public override void AddToCollection (object collection, object value)
			{
				Debug.Assert (AllowsMany);

				var c = (HttpHeaderValueCollection<U>) collection;

				var list = value as List<U>;
				if (list != null)
					c.AddRange (list);
				else
					c.Add ((U) value);
			}

			protected override object CreateCollection (HttpHeaders headers, HeaderInfo headerInfo)
			{
				return new HttpHeaderValueCollection<U> (headers, headerInfo);
			}

			public override List<string> ToStringCollection (object collection)
			{
				if (collection == null)
					return null;

				var c = (HttpHeaderValueCollection<U>) collection;
				if (c.Count == 0) {
					if (c.InvalidValues == null)
						return null;

					return new List<string> (c.InvalidValues);
				}

				var list = new List<string> ();
				foreach (var item in c) {
					list.Add (item.ToString ());
				}

				if (c.InvalidValues != null)
					list.AddRange (c.InvalidValues);

				return list;
			}

			public override bool TryParse (string value, out object result)
			{
				T tresult;
				bool b = parser (value, out tresult);
				result = tresult;
				return b;
			}
		}

		class CollectionHeaderTypeInfo<T, U> : HeaderTypeInfo<T, U> where U : class
		{
			readonly int minimalCount;
			readonly string separator;
			readonly TryParseListDelegate<T> parser;

			public CollectionHeaderTypeInfo (string name, TryParseListDelegate<T> parser, HttpHeaderKind headerKind, int minimalCount, string separator)
				: base (name, null, headerKind)
			{
				this.parser = parser;
				this.minimalCount = minimalCount;
				AllowsMany = true;
				this.separator = separator;
			}

			public override string Separator {
				get {
					return separator;
				}
			}

			public override bool TryParse (string value, out object result)
			{
				List<T> tresult;
				if (!parser (value, minimalCount, out tresult)) {
					result = null;
					return false;
				}

				result = tresult;
				return true;
			}
		}

		public bool AllowsMany;
		public readonly HttpHeaderKind HeaderKind;
		public readonly string Name;

		protected HeaderInfo (string name, HttpHeaderKind headerKind)
		{
			this.Name = name;
			this.HeaderKind = headerKind;
		}

		public static HeaderInfo CreateSingle<T> (string name, TryParseDelegate<T> parser, HttpHeaderKind headerKind, Func<object, string> toString = null)
		{
			return new HeaderTypeInfo<T, object> (name, parser, headerKind) {
				CustomToString = toString
			};
		}

		//
		// Headers with #rule for defining lists of elements or *rule for defining occurences of elements
		//
		public static HeaderInfo CreateMulti<T> (string name, TryParseListDelegate<T> elementParser, HttpHeaderKind headerKind, int minimalCount = 1, string separator = ", ") where T : class
		{
			return new CollectionHeaderTypeInfo<T, T> (name, elementParser, headerKind, minimalCount, separator);
		}

		public object CreateCollection (HttpHeaders headers)
		{
			return CreateCollection (headers, this);
		}

		public Func<object, string> CustomToString {
			get; private set;
		}

		public virtual string Separator {
			get {
				// Needed for AllowsMany only
				throw new NotSupportedException ();
			}
		}

		public abstract void AddToCollection (object collection, object value);
		protected abstract object CreateCollection (HttpHeaders headers, HeaderInfo headerInfo);
		public abstract List<string> ToStringCollection (object collection);
		public abstract bool TryParse (string value, out object result);
	}
}
