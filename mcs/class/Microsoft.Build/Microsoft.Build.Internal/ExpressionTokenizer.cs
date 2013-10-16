using System;
using Microsoft.Build.Evaluation;

namespace Microsoft.Build.Internal
{
	class ExpressionTokenizer : yyParser.yyInput
	{
		public ExpressionTokenizer (string source)
		{
			this.source = source;
			current_token_position = -1;
		}
		
		string source;
		
		int current_token;
		string error;
		int pos, current_token_position;
		object token_value;

		public bool advance ()
		{
			if (pos == source.Length)
				return false;

			error = null;
			token_value = null;
			current_token_position = pos;

			switch (source [pos++]) {
			case '.':
				current_token = Token.DOT;
				break;
			case ',':
				current_token = Token.COMMA;
				break;
			case '(':
				current_token = Token.PAREN_OPEN;
				break;
			case ')':
				current_token = Token.PAREN_CLOSE;
				break;
			case '-':
				if (pos < source.Length && source [pos] == '>') {
					current_token = Token.ARROW;
					pos++;
				} else {
					current_token = Token.ERROR;
					error = "'-' is not followed by '>'.";
				}
				break;
			case '=':
				if (pos < source.Length && source [pos] == '=') {
					current_token = Token.EQ;
					pos++;
				} else {
					current_token = Token.ERROR;
					error = "'=' is not followed by '='.";
				}
				break;
			case ':':
				if (pos < source.Length && source [pos] == ':') {
					current_token = Token.COLON2;
					pos++;
				} else {
					current_token = Token.ERROR;
					error = "':' is not followed by ':'.";
				}
				break;
			case '!':
				if (pos < source.Length && source [pos] == '=') {
					pos++;
					current_token = Token.NE;
				}
				else
					current_token = Token.NOT;
				break;
			case '>':
				if (pos < source.Length && source [pos] == '=') {
					pos++;
					current_token = Token.GE;
				}
				else
					current_token = Token.GT;
				break;
			case '<':
				if (pos < source.Length && source [pos] == '=') {
					pos++;
					current_token = Token.LE;
				}
				else
					current_token = Token.LT;
				break;
			case '$':
				if (pos < source.Length && source [pos] == '(') {
					current_token = Token.PROP_OPEN;
					pos++;
				} else {
					current_token = Token.ERROR;
					error = "property reference '$' is not followed by '('.";
				}
				break;
			case '@':
				if (pos < source.Length && source [pos] == '(') {
					current_token = Token.ITEM_OPEN;
					pos++;
				} else {
					current_token = Token.ERROR;
					error = "item reference '@' is not followed by '('.";
				}
				break;
			case '%':
				if (pos < source.Length && source [pos] == '(') {
					current_token = Token.METADATA_OPEN;
					pos++;
				} else {
					current_token = Token.ERROR;
					error = "metadata reference '%' is not followed by '('.";
				}
				break;
			case '"':
				ReadStringLiteral (source, '"');
				break;
			case '\'':
				ReadStringLiteral (source, '\'');
				break;
			default:
				pos = source.IndexOfAny (token_starter_chars, pos);
				if (pos < 0)
					pos = source.Length;
				var val = source.Substring (current_token_position, pos - current_token_position - 1);
				if (val.Equals ("AND", StringComparison.OrdinalIgnoreCase))
					current_token = Token.AND;
				else if (val.Equals ("OR", StringComparison.OrdinalIgnoreCase))
					current_token = Token.OR;
				else if (val.Equals ("TRUE", StringComparison.OrdinalIgnoreCase))
					current_token = Token.TRUE_LITERAL;
				else if (val.Equals ("FALSE", StringComparison.OrdinalIgnoreCase))
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

		static readonly char [] token_starter_chars = ".,)-=:!><$@%\"' ".ToCharArray ();
		
		void ReadStringLiteral (string source, char c)
		{
			while (pos < source.Length && source [pos] != c)
				pos++;
			if (source [pos - 1] != c) {
				current_token = Token.ERROR;
				error = string.Format ("missing string literal terminator [{0}]", c);
			} else {
				current_token = Token.NAME;
				token_value = source.Substring (current_token_position + 1, pos - current_token_position - 2);
				token_value = ProjectCollection.Unescape ((string) token_value);
			}
		}
		
		public int token ()
		{
			return current_token;
		}
		
		public object value ()
		{
			if (current_token == Token.NAME)
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
	}

	class ErrorToken : Location
	{
		public string Message { get; set; }
	}

	interface ILocation
	{
		//int Line { get; }
		int Column { get; }
		//string File { get; }
	}

	class Location : ILocation
	{
		//public int Line { get; set; }
		public int Column { get; set; }
		//public string File { get; set; }
	}
}
