//
// System.Data.SqlClient.SqlXmlTextReader.cs
//
// Author:
//   Konstantin Triger (kostat@mainsoft.com)
//
// Copyright (C) 2006 Mainsoft, corp. (http://www.mainsoft.com)
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
using System.IO;
using System.Text;
using System.Xml;

namespace System.Data.SqlClient {
	internal sealed class SqlXmlTextReader : TextReader {

		#region FragmentXmlTextReader

		sealed class FragmentXmlTextReader : XmlTextReader {
			public FragmentXmlTextReader(System.IO.TextReader reader) : base(reader) {}

			public override bool Read() {
				do {
					if (!base.Read())
						return false;
				}while(base.Depth == 0);

				return true;
			}

			public override int Depth {
				get {
					int depth = base.Depth;
					if (depth >= 1)
						depth --;

					return depth;
				}
			}
		}

		#endregion

		#region Fields

		bool _hasPeekedChar;
		readonly char[] _peekedChar = new char[1];
		readonly SqlDataReader _reader;

		string _data;
		int _rootPosition;
		int _position = -1;
		bool _eof;

		static readonly char[] OpenRoot = new char[] {'<', 'X', '>'};
		const int OpenRootLength = 3;
		static readonly char[] CloseRoot = new char[] {'<', '/', 'X', '>'};
		const int CloseRootLength = 4;

		#endregion // Fields

		#region Constructors

		private SqlXmlTextReader (SqlDataReader reader) {
			_reader = reader;
		}

		#endregion

		#region Methods

		public static XmlReader Create(SqlDataReader dataReader) {
			return new FragmentXmlTextReader(new SqlXmlTextReader(dataReader));
		}

		public override void Close() {
			_reader.Close ();	
		}

		public override int Peek () {
			if (!_hasPeekedChar) {

				int consumed = Read(_peekedChar, 0, 1);
				if (consumed < 0)
					return -1;

				_hasPeekedChar = true;
			}

			return _peekedChar[0];
		}
			
		public override int Read () {
			int c = Peek();
			_hasPeekedChar = false;
			return c;
		}	

		public override int Read (char[] buffer, int index, int count) {
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			if (index < 0)
				throw new ArgumentOutOfRangeException("index");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			if (count == 0)
				return 0;

			int got = 0;

			if (_hasPeekedChar) {
				buffer[index++] = _peekedChar[0];
				count--;
				_hasPeekedChar = false;
				got ++;
			}

			if (!_eof) {
				while (count > 0) {

					if (_rootPosition < OpenRootLength) {
						buffer[index++] = OpenRoot[_rootPosition++];
						count --;
						got ++;
						continue;
					}

					if (_position < 0) {
						if (_reader.Read()) {
							_position = 0;
							_data = _reader.GetString(0);
						}
						else {
							if(_reader.NextResult())
								continue;
							else {
								_rootPosition = 0;
								_eof = true;
								break;
							}
						}
					}

					int consumed = ((_position + count) > _data.Length) ? (_data.Length - (int)_position) : count;
					_data.CopyTo(_position, buffer, index, consumed);
						
					if (consumed > 0) {
						_position += consumed;
						got += consumed;
						index += consumed;
						count -= consumed;
					}
					else
						_position = -1;
				}
			}

			while (count > 0 && _rootPosition < CloseRootLength) {
				buffer[index++] = CloseRoot[_rootPosition++];
				count --;
				got ++;
			}


			return got;
		}

		#endregion // Methods
	}
}	