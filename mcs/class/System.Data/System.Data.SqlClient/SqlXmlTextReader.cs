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

using Mono.Data.TdsClient.Internal;
using System;
using System.IO;
using System.Text;

namespace System.Data.SqlClient {
	internal sealed class SqlXmlTextReader : TextReader, IDisposable
	{
		#region Fields

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
			reader.Close ();
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
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
				return false;
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
			return (int) localBuffer[position];
		}
			
		public override int Read ()
		{
			int result = Peek ();
			position += 1;
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
