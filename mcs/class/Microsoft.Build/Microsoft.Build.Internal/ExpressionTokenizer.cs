//
// ExpressionTokenizer.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System.Collections.Generic;
using Microsoft.Build.Evaluation;

namespace Microsoft.Build.Internal.Expressions
{
	enum ExpressionValidationType
	{
		LaxString,
		StrictBoolean,
	}
	
	enum TokenizerMode
	{
		Default,
		InsideItemOrProperty,
	}
	
	class ExpressionTokenizer : yyParser.yyInput
	{
		public ExpressionTokenizer (string source, ExpressionValidationType validationType)
		{
			this.source = source;
			current_token_position = -1;
			validation_type = validationType;
			modes.Push (TokenizerMode.Default);
		}
		
		string source;
		ExpressionValidationType validation_type;
		
		int current_token;
		string error;
		int pos, current_token_position;
		object token_value;
		Stack<TokenizerMode> modes = new Stack<TokenizerMode> ();

		TokenizerMode CurrentTokenizerMode {
			get { return modes.Peek (); }
		}

		public bool advance ()
		{
			if (pos == source.Length)
				return false;

			error = null;
			token_value = null;

			while (pos < source.Length) {
				if (spaces.IndexOf (source [pos]) >= 0)
					pos++;
				else
					break;
			}
			if (pos == source.Length)
				return false;
			current_token_position = pos;

			switch (source [pos++]) {
			case '.':
				TokenForItemPropertyValue (".", Token.DOT);
				break;
			case ',':
				TokenForItemPropertyValue (",", Token.COMMA);
				break;
			case '[':
				TokenForItemPropertyValue ("[", Token.BRACE_OPEN);
				break;
			case ']':
				TokenForItemPropertyValue ("]", Token.BRACE_CLOSE);
				break;
			case '(':
				modes.Push (TokenizerMode.Default);
				TokenForItemPropertyValue ("(", Token.PAREN_OPEN);
				break;
			case ')':
				if (modes.Count > 0) {
					modes.Pop ();
					current_token = Token.PAREN_CLOSE;
				} else {
					token_value = ")";
					current_token = Token.NAME;
				}
				break;
			case '-':
				if (pos < source.Length && source [pos] == '>') {
					current_token = Token.ARROW;
					pos++;
				} else
					ErrorOnStrictBoolean ("-", "'-' is not followed by '>'.");
				break;
			case '=':
				if (pos < source.Length && source [pos] == '=') {
					current_token = Token.EQ;
					pos++;
				} else
					ErrorOnStrictBoolean ("=", "'=' is not followed by '='.");
				break;
			case ':':
				if (pos < source.Length && source [pos] == ':') {
					current_token = Token.COLON2;
				} else
					ErrorOnStrictBoolean (":", "':' is not followed by ':'.");
				pos++;
				break;
			case '!':
				if (pos < source.Length && source [pos] == '=') {
					pos++;
					current_token = Token.NE;
				} else
					TokenForItemPropertyValue ("!", Token.NOT);
				break;
			case '>':
				if (pos < source.Length && source [pos] == '=') {
					pos++;
					current_token = Token.GE;
				} else
					current_token = Token.GT;
				break;
			case '<':
				if (pos < source.Length && source [pos] == '=') {
					pos++;
					current_token = Token.LE;
				} else
					current_token = Token.LT;
				break;
			case '$':
				if (pos < source.Length && source [pos] == '(') {
					modes.Push (TokenizerMode.InsideItemOrProperty);
					current_token = Token.PROP_OPEN;
					pos++;
				} else
					ErrorOnStrictBoolean ("$", "property reference '$' is not followed by '('.");
				break;
			case '@':
				if (pos < source.Length && source [pos] == '(') {
					modes.Push (TokenizerMode.InsideItemOrProperty);
					current_token = Token.ITEM_OPEN;
					pos++;
				} else
					ErrorOnStrictBoolean ("@", "item reference '@' is not followed by '('.");
				break;
			case '%':
				if (pos < source.Length && source [pos] == '(') {
					modes.Push (TokenizerMode.InsideItemOrProperty);
					current_token = Token.METADATA_OPEN;
					pos++;
				} else
					ErrorOnStrictBoolean ("%", "metadata reference '%' is not followed by '('.");
				break;
			case '"':
			case '\'':
				pos = source.IndexOf (source [pos - 1], pos);
				if (pos < 0) {
					ErrorOnStrictBoolean ("'", "unterminated string literal");
					pos = source.Length;
				}
				token_value = source.Substring (current_token_position + 1, pos - current_token_position - 1);
				current_token = Token.STRING_LITERAL;
				pos++;
				break;
			default:
				pos = source.IndexOfAny (token_starter_chars, pos);
				if (pos < 0)
					pos = source.Length;
				var val = source.Substring (current_token_position, pos - current_token_position);
				if (val.Equals ("AND", StringComparison.OrdinalIgnoreCase))
					current_token = Token.AND;
				else if (val.Equals ("OR", StringComparison.OrdinalIgnoreCase))
					current_token = Token.OR;
				else if (val.Equals ("TRUE", StringComparison.OrdinalIgnoreCase))
					current_token = Token.TRUE_LITERAL;
				else if (val.Equals ("FALSE", StringComparison.OrdinalIgnoreCase))
					current_token = Token.FALSE_LITERAL;
				else if (val.Equals ("YES", StringComparison.OrdinalIgnoreCase))
					current_token = Token.TRUE_LITERAL;
				else if (val.Equals ("NO", StringComparison.OrdinalIgnoreCase))
					current_token = Token.FALSE_LITERAL;
				else if (val.Equals ("ON", StringComparison.OrdinalIgnoreCase))
					current_token = Token.TRUE_LITERAL;
				else if (val.Equals ("OFF", StringComparison.OrdinalIgnoreCase))
					current_token = Token.FALSE_LITERAL;
				else {
					current_token = Token.NAME;
					token_value = ProjectCollection.Unescape (val);
					break;
				}
				break;
			}
			return true;
		}
		string spaces = " \t\r\n";

		static readonly char [] token_starter_chars = ".,[]()-=:!><$@%\"' ".ToCharArray ();
		
		void ReadStringLiteral (string source, char c)
		{
			while (pos < source.Length && source [pos] != c)
				pos++;
			if (source [pos - 1] != c)
				ErrorOnStrictBoolean (c.ToString (), string.Format ("missing string literal terminator [{0}]", c));
			else {
				current_token = Token.NAME;
				token_value = source.Substring (current_token_position + 1, pos - current_token_position - 2);
				token_value = ProjectCollection.Unescape ((string) token_value);
			}
		}
		
		void TokenForItemPropertyValue (string value, int token)
		{
			if (true)//CurrentTokenizerMode == TokenizerMode.InsideItemOrProperty)
				current_token = token;
			else {
				current_token = Token.NAME;
				token_value = value;
			}
		}
		
		void ErrorOnStrictBoolean (string value, string message)
		{
			if (validation_type == ExpressionValidationType.StrictBoolean) {
				current_token = Token.ERROR;
				error = message;
			} else {
				current_token = Token.NAME;
				token_value = value;
			}
		}
		
		public int token ()
		{
			return current_token;
		}
		
		public object value ()
		{
			if (current_token == Token.NAME || current_token == Token.STRING_LITERAL)
				return new NameToken () { Name = (string) token_value, Column = current_token_position };
			else if (error != null)
				return new ErrorToken () { Message = error, Column = current_token_position };
			else
				return new Location () { Column = current_token_position };
		}
	}	

	class NameToken : Location
	{
		public string Name { get; set; }
		
		public override string ToString ()
		{
			return string.Format ("[NameToken: Value={0}]", Name);
		}
	}

	class ErrorToken : Location
	{
		public string Message { get; set; }
	}

	interface ILocation
	{
		//int Line { get; }
		int Column { get; }
		string File { get; }
		
		string ToLocationString ();
	}

	class Location : ILocation
	{
		//public int Line { get; set; }
		public int Column { get; set; }
		public string File { get; set; }
		
		public string ToLocationString ()
		{
			return "at " + Column;
		}
	}
}
