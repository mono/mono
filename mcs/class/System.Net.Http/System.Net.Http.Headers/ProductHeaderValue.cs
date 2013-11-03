//
// ProductHeaderValue.cs
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

namespace System.Net.Http.Headers
{
	public class ProductHeaderValue : ICloneable
	{
		public ProductHeaderValue (string name)
		{
			Parser.Token.Check (name);
			Name = name;
		}

		public ProductHeaderValue (string name, string version)
			: this (name)
		{
			if (version != null)
				Parser.Token.Check (version);

			Version = version;
		}

		internal ProductHeaderValue ()
		{
		}

		public string Name { get; internal set; }
		public string Version { get; internal set; }

		object ICloneable.Clone ()
		{
			return MemberwiseClone ();
		}

		public override bool Equals (object obj)
		{
			var source = obj as ProductHeaderValue;
			if (source == null)
				return false;

			return string.Equals (source.Name, Name, StringComparison.OrdinalIgnoreCase) &&
				string.Equals (source.Version, Version, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode ()
		{
			var hc = Name.ToLowerInvariant ().GetHashCode ();
			if (Version != null)
				hc ^= Version.ToLowerInvariant ().GetHashCode ();

			return hc;
		}

		public static ProductHeaderValue Parse (string input)
		{
			ProductHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}

		public static bool TryParse (string input, out ProductHeaderValue parsedValue)
		{
			parsedValue = null;

			var lexer = new Lexer (input);
			var t = lexer.Scan ();
			if (t != Token.Type.Token)
				return false;

			var value = new ProductHeaderValue ();
			value.Name = lexer.GetStringValue (t);

			t = lexer.Scan ();
			if (t == Token.Type.SeparatorSlash) {
				t = lexer.Scan ();
				if (t != Token.Type.Token)
					return false;

				value.Version = lexer.GetStringValue (t);
				t = lexer.Scan ();
			}

			if (t != Token.Type.End)
				return false;

			parsedValue = value;
			return true;
		}

		public override string ToString ()
		{
			return Version == null ? Name : Name + "/" + Version;
		}
	}
}
