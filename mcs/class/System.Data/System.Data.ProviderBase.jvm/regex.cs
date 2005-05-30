using System.Data;
using System.Data.Common;
using System.Data.OleDb;

namespace System.Data.ProviderBase {
	public abstract class SimpleRegex {
		abstract protected internal SimpleMatch Match(string input, int beginning, int length);
		protected internal SimpleMatch Match(string input) {
			return Match(input, 0, input.Length);
		}

		protected bool IsWordChar(char c) { //regexp w ([a-zA-Z_0-9]) + #@$
			if (c < '@') {
				return (c >= '0' && c <= '9') ||
						(c == '#' || c == '$');
			}
			else {
				return (c <= 'Z') ||
					(c <= 'z' && c >= '_' && c != '`');
			}
		}
	}
	public class SimpleCapture {
		int _index;
		int _length;
		string _input;

		protected SimpleCapture(int index, int length, string input) {
			_index = index;
			_length = length;
			_input = input;
		}

		protected internal int Index {
			get {
				return _index;
			}
		}

		protected internal int Length {
			get {
				return _length;
			}
		}

		protected internal string Value {
			get {
				return Input.Substring(Index, Length);
			}
		}

		protected string Input {
			get {
				return _input;
			}
		}
	}

	public class SimpleMatch : SimpleCapture {
		bool _success;
		SimpleRegex _regex;
		int _total;
		int _skip;

		protected internal SimpleMatch(SimpleRegex regex, int total, bool success, int index, int length, string input)
			: this(regex, total, success, index, length, 0, input) {}

		protected internal SimpleMatch(SimpleRegex regex, int total, bool success, int index, int length, int skip, string input)
			: base(index, length, input) {
			_success = success;
			_regex = regex;
			_total = total;
			_skip = skip;
		}

		protected internal SimpleMatch NextMatch() {
			return _regex.Match(Input, Index+Length+_skip, _total);
		}

		protected internal bool Success {
			get {
				return _success;
			}
		}
	}

	internal class OleDbParamsRegex : SimpleRegex {
		protected internal override SimpleMatch Match(string input, int beginning, int length) {

			for (int i = beginning; i < length; i++) {
				char ch = input[i];
				switch(ch) {
					case '\'': {
						int end = input.IndexOf('\'', i+1);
						if (end < 0)
							break;

						i = end;
						break;
					}
					case '"': {
						int end = input.IndexOf('"', i+1);
						if (end < 0)
							break;

						i = end;
						break;
					}
					case '[': {
						int end = input.IndexOf(']', i+1);
						if (end < 0)
							break;

						i = end;
						break;
					}
					case '?': {
						return new SimpleMatch(this, length, true, i, 1, input);
					}
				}
			}

			return new SimpleMatch(this, length, false, length, 0, input);
		}
	}

	internal class SqlParamsRegex : SimpleRegex {
		//static readonly char[] charsEnd = new char[] {' ', '\f', '\t', '\v', '\r', '\n',  ',', ';', '(', ')', '[', ']','='};
		protected internal override SimpleMatch Match(string input, int beginning, int length) {

			int actualLen = length-1; //there must be something after @
			for (int i = beginning; i < actualLen; i++) {
				char ch = input[i];
				switch(ch) {
					case '\'': {
						int end = input.IndexOf('\'', i+1);
						if (end < 0)
							break;

						i = end;
						break;
					}
					case '"': {
						int end = input.IndexOf('"', i+1);
						if (end < 0)
							break;

						i = end;
						break;
					}
					case '[': {
						int end = input.IndexOf(']', i+1);
						if (end < 0)
							break;

						i = end;
						break;
					}
					case '@': {
						int start = i;

						do {
							i++;
						}while (i < length && input[i] == '@');

						if (i - start > 1)
							break;

						while (i < length && IsWordChar(input[i]))
							i++;
	
						return new SimpleMatch(this, length, true, start, i-start, input);
					}
				}
			}

			return new SimpleMatch(this, length, false, length, 0, input);
		}
	}

	internal class CharacterSplitterRegex : SimpleRegex {
		char _delimiter;
		internal CharacterSplitterRegex(char delimiter) {
			_delimiter = delimiter;
		}
		protected internal override SimpleMatch Match(string input, int beginning, int length) {

			for (int i = beginning; i < length; i++) {
				char ch = input[i];
				switch(ch) {
					case '\'': {
						int end = input.IndexOf('\'', i+1);
						if (end < 0)
							break;

						i = end;
						break;
					}
					case '"': {
						int end = input.IndexOf('"', i+1);
						if (end < 0)
							break;

						i = end;
						break;
					}
					case '[': {
						int end = input.IndexOf(']', i+1);
						if (end < 0)
							break;

						i = end;
						break;
					}
					default: {
						if (ch != _delimiter)
							break;

						return new SimpleMatch(this, length, true, beginning, i-beginning, 1, input);
					}
				}
			}

			int matchLength = length-beginning;
			return new SimpleMatch(this, length, matchLength > 0, beginning, matchLength, input);
		}
	}
}