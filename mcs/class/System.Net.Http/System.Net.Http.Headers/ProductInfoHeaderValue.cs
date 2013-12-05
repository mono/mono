//
// ProductInfoHeaderValue.cs
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
	public class ProductInfoHeaderValue : ICloneable
	{
		public ProductInfoHeaderValue (ProductHeaderValue product)
		{
			if (product == null)
				throw new ArgumentNullException ();

			Product = product;
		}

		public ProductInfoHeaderValue (string comment)
		{
			Parser.Token.CheckComment (comment);
			Comment = comment;
		}

		public ProductInfoHeaderValue (string productName, string productVersion)
		{
			Product = new ProductHeaderValue (productName, productVersion);
		}

		private ProductInfoHeaderValue ()
		{
		}

		public string Comment { get; private set; }
		public ProductHeaderValue Product { get; private set; }

		object ICloneable.Clone ()
		{
			return MemberwiseClone ();
		}

		public override bool Equals (object obj)
		{
			var source = obj as ProductInfoHeaderValue;
			if (source == null)
				return false;

			return Product != null ?
				Product.Equals (source.Product) :
				source.Comment == Comment;
		}

		public override int GetHashCode ()
		{
			return Product != null ?
				Product.GetHashCode () :
				Comment.GetHashCode ();
		}

		public static ProductInfoHeaderValue Parse (string input)
		{
			ProductInfoHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}
		
		public static bool TryParse (string input, out ProductInfoHeaderValue parsedValue)
		{
			parsedValue = null;

			var lexer = new Lexer (input);
			if (!TryParseElement (lexer, out parsedValue) || parsedValue == null)
				return false;

			if (lexer.Scan () != Token.Type.End) {
				parsedValue = null;
				return false;
			}	

			return true;
		}

		internal static bool TryParse (string input, out List<ProductInfoHeaderValue> result)
		{
			var list = new List<ProductInfoHeaderValue> ();
			var lexer = new Lexer (input);
			result = null;

			while (true) {
				ProductInfoHeaderValue element;
				if (!TryParseElement (lexer, out element))
					return false;

				if (element == null) {
					result = list;
					return true;
				}

				list.Add (element);
			}
		}

		static bool TryParseElement (Lexer lexer, out ProductInfoHeaderValue parsedValue)
		{
			string comment;
			parsedValue = null;
			Token t;

			if (lexer.ScanCommentOptional (out comment, out t)) {
				if (comment == null)
					return false;

				parsedValue = new ProductInfoHeaderValue ();
				parsedValue.Comment = comment;
				return true;
			}

			if (t == Token.Type.End)
				return true;

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
			}

			parsedValue = new ProductInfoHeaderValue (value);
			return true;
		}

		public override string ToString ()
		{
			if (Product == null)
				return Comment;

			return Product.ToString ();
		}
	}
}
