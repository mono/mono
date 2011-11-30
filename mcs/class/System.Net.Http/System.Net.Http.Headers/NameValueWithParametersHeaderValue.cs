//
// NameValueWithParametersHeaderValue.cs
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
	public class NameValueWithParametersHeaderValue : NameValueHeaderValue, ICloneable
	{
		List<NameValueHeaderValue> parameters;

		public NameValueWithParametersHeaderValue (string name)
			: base (name)
		{
		}

		public NameValueWithParametersHeaderValue (string name, string value)
			: base (name, value)
		{
		}

		protected NameValueWithParametersHeaderValue (NameValueWithParametersHeaderValue source)
			: base (source)
		{
			if (source.parameters != null) {
				foreach (var item in source.parameters)
					Parameters.Add (item);
			}
		}

		public ICollection<NameValueHeaderValue> Parameters {
			get {
				return parameters ?? (parameters = new List<NameValueHeaderValue> ());
			}
		}

		object ICloneable.Clone ()
		{
			return new NameValueWithParametersHeaderValue (this);
		}

		public override bool Equals (object obj)
		{
			var source = obj as NameValueWithParametersHeaderValue;
			if (source == null)
				return false;

			return base.Equals (obj) && Enumerable.SequenceEqual (source.parameters, parameters);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode () ^ HashCodeCalculator.Calculate (parameters);
		}

		public static new NameValueWithParametersHeaderValue Parse (string input)
		{
			NameValueWithParametersHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}

		public static bool TryParse (string input, out NameValueWithParametersHeaderValue parsedValue)
		{
			throw new NotImplementedException ();
		}
	}
}
