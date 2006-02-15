// HtmlAgilityPack V1.0 - Simon Mourier <simonm@microsoft.com>
using System;
using System.IO;
using System.Text;
using System.Collections;

namespace HtmlAgilityPack
{
	/// <summary>
	/// Represents the type of fragement in a mixed code document.
	/// </summary>
	public enum MixedCodeDocumentFragmentType
	{
		/// <summary>
		/// The fragment contains code.
		/// </summary>
		Code,

		/// <summary>
		/// The fragment contains text.
		/// </summary>
		Text,
	}

	/// <summary>
	/// Represents a fragment of code in a mixed code document.
	/// </summary>
	public class MixedCodeDocumentCodeFragment: MixedCodeDocumentFragment
	{
		internal string _code;

		internal MixedCodeDocumentCodeFragment(MixedCodeDocument doc):
			base(doc, MixedCodeDocumentFragmentType.Code)
		{
		}

		/// <summary>
		/// Gets the fragment code text.
		/// </summary>
		public string Code
		{
			get
			{
				if (_code == null)
				{
					_code = FragmentText.Substring(_doc.TokenCodeStart.Length,
						FragmentText.Length - _doc.TokenCodeEnd.Length - _doc.TokenCodeStart.Length -1).Trim();
					if (_code.StartsWith("="))
					{
						_code = _doc.TokenResponseWrite + _code.Substring(1, _code.Length-1);
					}
				}
				return _code;
			}
			set
			{
				_code = value;
			}
		}
	}

	/// <summary>
	/// Represents a fragment of text in a mixed code document.
	/// </summary>
	public class MixedCodeDocumentTextFragment: MixedCodeDocumentFragment
	{
		internal MixedCodeDocumentTextFragment(MixedCodeDocument doc):
			base(doc, MixedCodeDocumentFragmentType.Text)
		{
		}

		/// <summary>
		/// Gets the fragment text.
		/// </summary>
		public string Text
		{
			get
			{
				return FragmentText;
			}
			set
			{
				base._fragmenttext = value;
			}
		}
	}

	/// <summary>
	/// Represents a base class for fragments in a mixed code document.
	/// </summary>
	public abstract class MixedCodeDocumentFragment
	{
		internal MixedCodeDocumentFragmentType _type;
		internal MixedCodeDocument _doc;
		internal int _index;
		internal int _length;
		internal int _line;
		internal int _lineposition;
		internal string _fragmenttext;

		internal MixedCodeDocumentFragment(MixedCodeDocument doc, MixedCodeDocumentFragmentType type)
		{
			_doc = doc;
			_type = type;
			switch(type)
			{
				case MixedCodeDocumentFragmentType.Text:
					_doc._textfragments.Append(this);
					break;

				case MixedCodeDocumentFragmentType.Code:
					_doc._codefragments.Append(this);
					break;
			}
			_doc._fragments.Append(this);
		}

		/// <summary>
		/// Gets the type of fragment.
		/// </summary>
		public MixedCodeDocumentFragmentType FragmentType
		{
			get
			{
				return _type;
			}
		}

		/// <summary>
		/// Gets the fragment position in the document's stream.
		/// </summary>
		public int StreamPosition
		{
			get
			{
				return _index;
			}
		}

		/// <summary>
		/// Gets the line number of the fragment.
		/// </summary>
		public int Line
		{
			get
			{
				return _line;
			}
		}

		/// <summary>
		/// Gets the line position (column) of the fragment.
		/// </summary>
		public int LinePosition
		{
			get
			{
				return _lineposition;
			}
		}

		/// <summary>
		/// Gets the fragement text.
		/// </summary>
		public string FragmentText
		{
			get
			{
				if (_fragmenttext == null)
				{
					_fragmenttext = _doc._text.Substring(_index, _length);
				}
				return _fragmenttext;
			}
		}
	}

	/// <summary>
	/// Represents a list of mixed code fragments.
	/// </summary>
	public class MixedCodeDocumentFragmentList: IEnumerable
	{
		private MixedCodeDocument _doc;
		private ArrayList _items = new ArrayList();

		internal MixedCodeDocumentFragmentList(MixedCodeDocument doc)
		{
			_doc = doc;
		}

		/// <summary>
		/// Appends a fragment to the list of fragments.
		/// </summary>
		/// <param name="newFragment">The fragment to append. May not be null.</param>
		public void Append(MixedCodeDocumentFragment newFragment)
		{
			if (newFragment == null)
			{
				throw new ArgumentNullException("newFragment");
			}
			_items.Add(newFragment);
		}

		/// <summary>
		/// Prepends a fragment to the list of fragments.
		/// </summary>
		/// <param name="newFragment">The fragment to append. May not be null.</param>
		public void Prepend(MixedCodeDocumentFragment newFragment)
		{
			if (newFragment == null)
			{
				throw new ArgumentNullException("newFragment");
			}
			_items.Insert(0, newFragment);
		}

		/// <summary>
		/// Remove a fragment from the list of fragments. If this fragment was not in the list, an exception will be raised.
		/// </summary>
		/// <param name="fragment">The fragment to remove. May not be null.</param>
		public void Remove(MixedCodeDocumentFragment fragment)
		{
			if (fragment == null)
			{
				throw new ArgumentNullException("fragment");
			}
			int index = GetFragmentIndex(fragment);
			if (index == -1)
			{
				throw new IndexOutOfRangeException();
			}
			RemoveAt(index);
		}

		/// <summary>
		/// Remove a fragment from the list of fragments, using its index in the list.
		/// </summary>
		/// <param name="index">The index of the fragment to remove.</param>
		public void RemoveAt(int index)
		{
			MixedCodeDocumentFragment frag = (MixedCodeDocumentFragment)_items[index];
			_items.RemoveAt(index);
		}

		/// <summary>
		/// Remove all fragments from the list.
		/// </summary>
		public void RemoveAll()
		{
			_items.Clear();
		}

		/// <summary>
		/// Gets the number of fragments contained in the list.
		/// </summary>
		public int Count
		{
			get
			{
				return _items.Count;
			}
		}

		internal int GetFragmentIndex(MixedCodeDocumentFragment fragment)
		{
			if (fragment == null)
			{
				throw new ArgumentNullException("fragment");
			}
			for(int i=0;i<_items.Count;i++)
			{
				if (((MixedCodeDocumentFragment)_items[i])==fragment)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Gets a fragment from the list using its index.
		/// </summary>
		public MixedCodeDocumentFragment this[int index]
		{
			get
			{
				return _items[index] as MixedCodeDocumentFragment;
			}
		}

		internal void Clear()
		{
			_items.Clear();
		}

		/// <summary>
		/// Gets an enumerator that can iterate through the fragment list.
		/// </summary>
		public MixedCodeDocumentFragmentEnumerator GetEnumerator() 
		{
			return new MixedCodeDocumentFragmentEnumerator(_items);
		}

		/// <summary>
		/// Gets an enumerator that can iterate through the fragment list.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator() 
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Represents a fragment enumerator.
		/// </summary>
		public class MixedCodeDocumentFragmentEnumerator: IEnumerator 
		{
			int _index;
			ArrayList _items;

			internal MixedCodeDocumentFragmentEnumerator(ArrayList items) 
			{
				_items = items;
				_index = -1;
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the collection.
			/// </summary>
			public void Reset() 
			{
				_index = -1;
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
			public bool MoveNext() 
			{
				_index++;
				return (_index<_items.Count);
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public MixedCodeDocumentFragment Current 
			{
				get 
				{
					return (MixedCodeDocumentFragment)(_items[_index]);
				}
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			object IEnumerator.Current 
			{
				get 
				{
					return (Current);
				}
			}
		}
	}

	/// <summary>
	/// Represents a document with mixed code and text. ASP, ASPX, JSP, are good example of such documents.
	/// </summary>
	public class MixedCodeDocument
	{
		private System.Text.Encoding _streamencoding = null;
		internal string _text;
		internal MixedCodeDocumentFragmentList _fragments;
		internal MixedCodeDocumentFragmentList _codefragments;
		internal MixedCodeDocumentFragmentList _textfragments;
		private ParseState _state;
		private int _index;
		private int _c;
		private int _line;
		private int _lineposition;
		private MixedCodeDocumentFragment _currentfragment;

		/// <summary>
		/// Gets or sets the token representing code start.
		/// </summary>
		public string TokenCodeStart = "<%";

		/// <summary>
		/// Gets or sets the token representing code end.
		/// </summary>
		public string TokenCodeEnd = "%>";

		/// <summary>
		/// Gets or sets the token representing code directive.
		/// </summary>
		public string TokenDirective = "@";

		/// <summary>
		/// Gets or sets the token representing response write directive.
		/// </summary>
		public string TokenResponseWrite = "Response.Write ";


		private string TokenTextBlock = "TextBlock({0})";

		/// <summary>
		/// Creates a mixed code document instance.
		/// </summary>
		public MixedCodeDocument()
		{
			_codefragments = new MixedCodeDocumentFragmentList(this);
			_textfragments = new MixedCodeDocumentFragmentList(this);
			_fragments = new MixedCodeDocumentFragmentList(this);
		}

		/// <summary>
		/// Loads a mixed code document from a stream.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		public void Load(Stream stream)
		{
			Load(new StreamReader(stream));
		}

		/// <summary>
		/// Loads a mixed code document from a stream.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
		public void Load(Stream stream, bool detectEncodingFromByteOrderMarks)
		{
			Load(new StreamReader(stream, detectEncodingFromByteOrderMarks));
		}

		/// <summary>
		/// Loads a mixed code document from a stream.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="encoding">The character encoding to use.</param>
		public void Load(Stream stream, Encoding encoding)
		{
			Load(new StreamReader(stream, encoding));
		}

		/// <summary>
		/// Loads a mixed code document from a stream.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
		public void Load(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
		{
			Load(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks));
		}

		/// <summary>
		/// Loads a mixed code document from a stream.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
		/// <param name="buffersize">The minimum buffer size.</param>
		public void Load(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
		{
			Load(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks, buffersize));
		}

		/// <summary>
		/// Loads a mixed code document from a file.
		/// </summary>
		/// <param name="path">The complete file path to be read.</param>
		public void Load(string path)
		{
			Load(new StreamReader(path));
		}

		/// <summary>
		/// Loads a mixed code document from a file.
		/// </summary>
		/// <param name="path">The complete file path to be read.</param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
		public void Load(string path, bool detectEncodingFromByteOrderMarks)
		{
			Load(new StreamReader(path, detectEncodingFromByteOrderMarks));
		}

		/// <summary>
		/// Loads a mixed code document from a file.
		/// </summary>
		/// <param name="path">The complete file path to be read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		public void Load(string path, Encoding encoding)
		{
			Load(new StreamReader(path, encoding));
		}

		/// <summary>
		/// Loads a mixed code document from a file.
		/// </summary>
		/// <param name="path">The complete file path to be read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
		public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
		{
			Load(new StreamReader(path, encoding, detectEncodingFromByteOrderMarks));
		}

		/// <summary>
		/// Loads a mixed code document from a file.
		/// </summary>
		/// <param name="path">The complete file path to be read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
		/// <param name="buffersize">The minimum buffer size.</param>
		public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
		{
			Load(new StreamReader(path, encoding, detectEncodingFromByteOrderMarks, buffersize));
		}

		/// <summary>
		/// Loads a mixed document from a text
		/// </summary>
		/// <param name="html">The text to load.</param>
		public void LoadHtml(string html)
		{
			Load(new StringReader(html));
		}

		/// <summary>
		/// Loads the mixed code document from the specified TextReader.
		/// </summary>
		/// <param name="reader">The TextReader used to feed the HTML data into the document.</param>
		public void Load(TextReader reader)
		{
			_codefragments.Clear();
			_textfragments.Clear();

			// all pseudo constructors get down to this one
			StreamReader sr = reader as StreamReader;
			if (sr != null)
			{
				_streamencoding = sr.CurrentEncoding;
			}

			_text = reader.ReadToEnd();
			reader.Close();
			Parse();
		}

		internal System.Text.Encoding GetOutEncoding()
		{
			if (_streamencoding != null)
				return _streamencoding;
			return System.Text.Encoding.Default;
		}

		/// <summary>
		/// Gets the encoding of the stream used to read the document.
		/// </summary>
		public System.Text.Encoding StreamEncoding
		{
			get
			{
				return _streamencoding;
			}
		}

		/// <summary>
		/// Gets the list of code fragments in the document.
		/// </summary>
		public MixedCodeDocumentFragmentList CodeFragments
		{
			get
			{
				return _codefragments;
			}
		}

		/// <summary>
		/// Gets the list of text fragments in the document.
		/// </summary>
		public MixedCodeDocumentFragmentList TextFragments
		{
			get
			{
				return _textfragments;
			}
		}

		/// <summary>
		/// Gets the list of all fragments in the document.
		/// </summary>
		public MixedCodeDocumentFragmentList Fragments
		{
			get
			{
				return _fragments;
			}
		}

		/// <summary>
		/// Saves the mixed document to the specified stream.
		/// </summary>
		/// <param name="outStream">The stream to which you want to save.</param>
		public void Save(Stream outStream)
		{
			StreamWriter sw = new StreamWriter(outStream, GetOutEncoding());
			Save(sw);
		}

		/// <summary>
		/// Saves the mixed document to the specified stream.
		/// </summary>
		/// <param name="outStream">The stream to which you want to save.</param>
		/// <param name="encoding">The character encoding to use.</param>
		public void Save(Stream outStream, System.Text.Encoding encoding)
		{
			StreamWriter sw = new StreamWriter(outStream, encoding);
			Save(sw);
		}

		/// <summary>
		/// Saves the mixed document to the specified file.
		/// </summary>
		/// <param name="filename">The location of the file where you want to save the document.</param>
		public void Save(string filename)
		{
			StreamWriter sw = new StreamWriter(filename, false, GetOutEncoding());
			Save(sw);
		}

		/// <summary>
		/// Saves the mixed document to the specified file.
		/// </summary>
		/// <param name="filename">The location of the file where you want to save the document.</param>
		/// <param name="encoding">The character encoding to use.</param>
		public void Save(string filename, System.Text.Encoding encoding)
		{
			StreamWriter sw = new StreamWriter(filename, false, encoding);
			Save(sw);
		}

		/// <summary>
		/// Saves the mixed document to the specified StreamWriter.
		/// </summary>
		/// <param name="writer">The StreamWriter to which you want to save.</param>
		public void Save(StreamWriter writer)
		{
			Save((TextWriter)writer);
		}

		/// <summary>
		/// Saves the mixed document to the specified TextWriter.
		/// </summary>
		/// <param name="writer">The TextWriter to which you want to save.</param>
		public void Save(TextWriter writer)
		{

			writer.Flush();
		}

		/// <summary>
		/// Gets the code represented by the mixed code document seen as a template.
		/// </summary>
		public string Code
		{
			get
			{
				string s = "";
				int i = 0;
				foreach(MixedCodeDocumentFragment frag in _fragments)
				{
					switch(frag._type)
					{
						case MixedCodeDocumentFragmentType.Text:
							s += TokenResponseWrite + string.Format(TokenTextBlock, i) + "\n";
							i++;
							break;

						case MixedCodeDocumentFragmentType.Code:
							s += ((MixedCodeDocumentCodeFragment)frag).Code + "\n";
							break;
					}
				}
				return s;
			}
		}

		/// <summary>
		/// Create a text fragment instances.
		/// </summary>
		/// <returns>The newly created text fragment instance.</returns>
		public MixedCodeDocumentTextFragment CreateTextFragment()
		{
			return (MixedCodeDocumentTextFragment)CreateFragment(MixedCodeDocumentFragmentType.Text);
		}

		/// <summary>
		/// Create a code fragment instances.
		/// </summary>
		/// <returns>The newly created code fragment instance.</returns>
		public MixedCodeDocumentCodeFragment CreateCodeFragment()
		{
			return (MixedCodeDocumentCodeFragment)CreateFragment(MixedCodeDocumentFragmentType.Code);
		}

		internal MixedCodeDocumentFragment CreateFragment(MixedCodeDocumentFragmentType type)
		{
			switch(type)
			{
				case MixedCodeDocumentFragmentType.Text:
					return new MixedCodeDocumentTextFragment(this);

				case MixedCodeDocumentFragmentType.Code:
					return new MixedCodeDocumentCodeFragment(this);

				default:
					throw new NotSupportedException();
			}
		}

		private void SetPosition()
		{
			_currentfragment._line = _line;
			_currentfragment._lineposition = _lineposition;
			_currentfragment._index = _index - 1;
			_currentfragment._length = 0;
		}

		private void IncrementPosition()
		{
			_index++;
			if (_c == 10)
			{
				_lineposition = 1;
				_line++;
			}
			else
				_lineposition++;
		}

		private enum ParseState
		{
			Text,
			Code
		}

		private void Parse()
		{
			_state = ParseState.Text;
			_index = 0;
			_currentfragment = CreateFragment(MixedCodeDocumentFragmentType.Text);

			while (_index<_text.Length)
			{
				_c = _text[_index];
				IncrementPosition();

				switch(_state)
				{
					case ParseState.Text:
						if (_index+TokenCodeStart.Length<_text.Length)
						{
							if (_text.Substring(_index-1, TokenCodeStart.Length) == TokenCodeStart)
							{
								_state = ParseState.Code;
								_currentfragment._length = _index -1 - _currentfragment._index;
								_currentfragment = CreateFragment(MixedCodeDocumentFragmentType.Code);
								SetPosition();
								continue;
							}
						}
						break;

					case ParseState.Code:
						if (_index+TokenCodeEnd.Length<_text.Length)
						{
							if (_text.Substring(_index-1, TokenCodeEnd.Length) == TokenCodeEnd)
							{
								_state = ParseState.Text;
								_currentfragment._length = _index + TokenCodeEnd.Length - _currentfragment._index;
								_index += TokenCodeEnd.Length;
								_lineposition += TokenCodeEnd.Length;
								_currentfragment = CreateFragment(MixedCodeDocumentFragmentType.Text);
								SetPosition();
								continue;
							}
						}
						break;
				}
			}

			_currentfragment._length = _index - _currentfragment._index;
		}

	}
}
