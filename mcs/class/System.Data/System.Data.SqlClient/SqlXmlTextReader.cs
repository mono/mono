//
// System.Data.SqlClient.SqlXmlTextReader.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002
// Copyright (C) Tim Coleman, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using Mono.Data.Tds.Protocol;
using System;
using System.IO;
using System.Text;

namespace System.Data.SqlClient {
	internal sealed class SqlXmlTextReader : TextReader, IDisposable
	{
		#region Fields

		bool disposed = false;
		bool eof = false;
		SqlDataReader reader;
		string localBuffer = "<results>";
		int position;

		#endregion // Fields

		#region Constructors

		internal SqlXmlTextReader (SqlDataReader reader)
			: base ()
		{
			this.reader = reader;
		}

		#endregion

		#region Methods

		public override void Close()
		{
			reader.Close ();	
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					Close ();
					((IDisposable) reader).Dispose ();
				}
				disposed = true;
			}
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private bool GetNextBuffer ()
		{
			if (eof) {
				localBuffer = null;
				return false;
			}

			position = 0;
			if (reader.Read ()) 
				localBuffer = reader.GetString (0);
			else if (reader.NextResult () && reader.Read ()) 
				localBuffer = reader.GetString (0);
			else {
				eof = true;
				localBuffer = "</results>";
			}
			return true;
		}

		public override int Peek ()
		{
			bool moreResults;
			if (localBuffer == null || localBuffer.Length == 0) {
				moreResults = GetNextBuffer ();
				if (!moreResults)
					return -1;
			}
			if (eof && position >= localBuffer.Length)
				return -1;
			return (int) localBuffer[position];
		}
			
		public override int Read ()
		{
			int result = Peek ();
			position += 1;
			if (!eof && position >= localBuffer.Length)
				GetNextBuffer ();
			return result;
		}	

		public override int Read (char[] buffer, int index, int count)
		{
			bool moreResults = true;
			int countRead = 0;

			if (localBuffer == null)
				moreResults = GetNextBuffer ();

			while (moreResults && count - countRead > localBuffer.Length - position) {
				localBuffer.CopyTo (position, buffer, index + countRead, localBuffer.Length);
				countRead += localBuffer.Length;
				moreResults = GetNextBuffer ();
			}
			if (moreResults && countRead < count) {
				localBuffer.CopyTo (position, buffer, index + countRead, count - countRead);
				position += count - countRead;
			}

			return countRead;
		}

		public override int ReadBlock (char[] buffer, int index, int count)
		{
			return Read (buffer, index, count);
		}

		public override string ReadLine ()
		{
			bool moreResults = true;
			string outBuffer;
			if (localBuffer == null)
				moreResults = GetNextBuffer ();
			if (!moreResults)
				return null;
			outBuffer = localBuffer;
			GetNextBuffer ();
			return outBuffer;
		}

		public override string ReadToEnd ()
		{
			string outBuffer = String.Empty;

			bool moreResults = true;
			if (localBuffer == null)
				moreResults = GetNextBuffer ();
			while (moreResults) {
				outBuffer += localBuffer;
				moreResults = GetNextBuffer ();
			}
			return outBuffer;
		}

		#endregion // Methods
	}
}	
