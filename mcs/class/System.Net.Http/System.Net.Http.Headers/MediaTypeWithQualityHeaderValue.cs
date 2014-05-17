//
// MediaTypeWithQualityHeaderValue.cs
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

namespace System.Net.Http.Headers
{
	public sealed class MediaTypeWithQualityHeaderValue : MediaTypeHeaderValue
	{
		public MediaTypeWithQualityHeaderValue (string mediaType)
			: base (mediaType)
		{
		}

		public MediaTypeWithQualityHeaderValue (string mediaType, double quality)
			: this (mediaType)
		{
			Quality = quality;
		}

		private MediaTypeWithQualityHeaderValue ()
		{
		}

		public double? Quality {
			get {
				return QualityValue.GetValue (parameters);
			}
			set {
				QualityValue.SetValue (ref parameters, value);
			}
		}

		public new static MediaTypeWithQualityHeaderValue Parse (string input)
		{
			MediaTypeWithQualityHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException ();
		}

		public static bool TryParse (string input, out MediaTypeWithQualityHeaderValue parsedValue)
		{
			var lexer = new Lexer (input);
			Token token;
			if (TryParseElement (lexer, out parsedValue, out token) && token == Token.Type.End)
				return true;

			parsedValue = null;
			return false;
		}

		static bool TryParseElement (Lexer lexer, out MediaTypeWithQualityHeaderValue parsedValue, out Token t)
		{
			parsedValue = null;

			string media;
			List<NameValueHeaderValue> parameters = null;
			var token = TryParseMediaType (lexer, out media);
			if (token == null) {
				t = Token.Empty;
				return false;
			}

			t = token.Value;
			if (t == Token.Type.SeparatorSemicolon && (!NameValueHeaderValue.TryParseParameters (lexer, out parameters, out t) || t != Token.Type.End))
				return false;

			parsedValue = new MediaTypeWithQualityHeaderValue ();
			parsedValue.media_type = media;
			parsedValue.parameters = parameters;
			return true;
		}

		internal static bool TryParse (string input, int minimalCount, out List<MediaTypeWithQualityHeaderValue> result)
		{
			return CollectionParser.TryParse (input, minimalCount, TryParseElement, out result);
		}
	}
}
