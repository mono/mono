
using System;
using System.Data.SqlTypes;
using System.IO;
using System.Text;

namespace System.Data.OracleClient {
	public sealed class OracleLob : Stream, ICloneable, INullable {
		#region Fields

		public static readonly new OracleLob Null = new OracleLob ();

		internal OracleConnection connection;
		bool isBatched = false;
		bool isOpen = true;
		bool notNull = false;
		OracleType type;

		long length = -1;
		long position = 0;

		#endregion // Fields

		#region Constructors

		internal OracleLob () {
		}


		#endregion // Constructors

		#region Properties

		public override bool CanRead {
			get { return (IsNull || isOpen); }
		}

		public override bool CanSeek {
			get { return (IsNull || isOpen); }
		}

		public override bool CanWrite {
			get { return isOpen; }
		}

		public int ChunkSize {
			[MonoTODO]
			get { 
				AssertConnectionIsOpen ();
				AssertObjectNotDisposed ();
				throw new NotImplementedException ();
			}
		}

		public OracleConnection Connection {
			get { return connection; }
		}

		public bool IsBatched {
			get { return isBatched; }
		}

		public bool IsNull {
			get { return !notNull; }
		}

		public bool IsTemporary {
			get { 
				AssertConnectionIsOpen ();
				AssertObjectNotDisposed ();
				throw new NotImplementedException ();
			}
		}

		public override long Length {
			get { 
				AssertConnectionIsOpen ();
				AssertObjectNotDisposed ();
				throw new NotImplementedException ();
			}
		}

		public OracleType LobType {
			get { return type; }
		}

		public override long Position {
			get { 
				AssertConnectionIsOpen ();
				AssertObjectNotDisposed ();
				return position;
			}
			set {
				AssertConnectionIsOpen ();
				AssertObjectNotDisposed ();
				position = value;
			}
		}

		public object Value {
			get { 
				AssertObjectNotDisposed ();
				if (IsNull)
					return DBNull.Value;
				
				byte[] buffer = null;

				int len = (int) Length;
				if (len == 0) {
					// LOB is not Null, but it is Empty
					if (LobType == OracleType.Clob)
						return "";
					else // OracleType.Blob
						return new byte[0];
				}

				if (LobType == OracleType.Clob) {
					buffer = new byte [len];
					Read (buffer, 0, len);
					UnicodeEncoding encoding = new UnicodeEncoding ();
					return encoding.GetString (buffer);
				}
				else {
					// OracleType.Blob
					buffer = new byte [len];
					Read (buffer, 0, len);
					return buffer;
				}
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Append (OracleLob source) {
			if (source.IsNull)
				throw new ArgumentNullException ();
			if (Connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ();
			throw new NotImplementedException ();
		}

		void AssertAmountIsEven (long amount, string argName) {
			if (amount % 2 == 1)
				throw new ArgumentOutOfRangeException ("CLOB and NCLOB parameters require even number of bytes for this argument.");
		}

		void AssertAmountIsValid (long amount, string argName) {
			if (amount > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException ("Argument too big.");
			if (LobType == OracleType.Clob || LobType == OracleType.NClob)
				AssertAmountIsEven (amount, argName);
		}

		void AssertConnectionIsOpen () {
			if (connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ("Invalid operation. The connection is closed.");
		}

		void AssertObjectNotDisposed () {
			if (!isOpen)
				throw new ObjectDisposedException ("OracleLob");
		}

		void AssertTransactionExists () {
//			if (connection.Transaction == null)
//				throw new InvalidOperationException ("Modifying a LOB requires that the connection be transacted.");
			throw new NotImplementedException ();
		}

		public void BeginBatch () {
			BeginBatch (OracleLobOpenMode.ReadOnly);
		}

		public void BeginBatch (OracleLobOpenMode mode) {
			AssertConnectionIsOpen ();
			AssertObjectNotDisposed ();

			isBatched = true;
		}

		[MonoTODO]
		public object Clone () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Close () {
			isOpen = false;
		}

		public long CopyTo (OracleLob destination) {
			return CopyTo (0, destination, 0, Length);
		}

		public long CopyTo (OracleLob destination, long destinationOffset) {
			return CopyTo (0, destination, destinationOffset, Length);
		}

		public long CopyTo (long sourceOffset, OracleLob destination, long destinationOffset, long amount) {
			if (destination.IsNull)
				throw new ArgumentNullException ();

			AssertAmountIsValid (sourceOffset, "sourceOffset");
			AssertAmountIsValid (destinationOffset, "destinationOffset");
			AssertAmountIsValid (amount, "amount");
			AssertTransactionExists ();
			AssertConnectionIsOpen ();

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Dispose () {
			throw new NotImplementedException ();
		}

		public void EndBatch () {
			AssertConnectionIsOpen ();
			AssertObjectNotDisposed ();

			isBatched = false;
		}

		public long Erase () {
			return Erase (0, Length);
		}

		public long Erase (long offset, long amount) {
			if (offset < 0 || amount < 0)
				throw new ArgumentOutOfRangeException ("Must be a positive value.");
			if (offset + amount > Length)
				throw new ArgumentOutOfRangeException ();

			AssertAmountIsValid (offset, "offset");
			AssertAmountIsValid (amount, "amount");

			throw new NotImplementedException ();
		}

		public override void Flush () {
			// No-op
		}

		public override int Read (byte[] buffer, int offset, int count) {
			if (buffer == null)
				throw new ArgumentNullException ();

			AssertAmountIsValid (offset, "offset");
			AssertAmountIsValid (count, "count");
			AssertConnectionIsOpen ();
			AssertObjectNotDisposed ();

			throw new NotImplementedException ();

			int bytesRead;
			byte[] output = new byte[count];

			output.CopyTo (buffer, offset);
			position += bytesRead;
			return bytesRead;
		}

		[MonoTODO]
		public override long Seek (long offset, SeekOrigin origin) {
			long newPosition = position;

			switch (origin) {
				case SeekOrigin.Begin:
					newPosition = offset;
					break;
				case SeekOrigin.Current:
					newPosition += offset;
					break;
				case SeekOrigin.End:
					newPosition = Length + offset;
					break;
			}

			if (newPosition > Length)
				throw new ArgumentOutOfRangeException ();

			position = newPosition;
			return position;
		}

		[MonoTODO]
		public override void SetLength (long value) {
			AssertAmountIsValid (value, "value");
			AssertTransactionExists ();
			AssertConnectionIsOpen ();
			AssertObjectNotDisposed ();

			throw new NotImplementedException ();

			length = value;
		}

		public override void Write (byte[] buffer, int offset, int count) {
			if (buffer == null)
				throw new ArgumentNullException ("Buffer is null.");
			if (offset < 0 || count < 0)
				throw new ArgumentOutOfRangeException ("Must be a positive value.");
			if (offset + count > buffer.Length)
				throw new ArgumentOutOfRangeException ("The offset and count values specified exceed the buffer provided.");
			AssertAmountIsValid (offset, "offset");
			AssertAmountIsValid (count, "count");
			AssertTransactionExists ();
			AssertConnectionIsOpen ();
			AssertObjectNotDisposed ();

			byte[] value = null;
			if (offset + count == buffer.Length && offset == 0)
				value = buffer;
			else {
				value = new byte[count];
				Array.Copy (buffer, offset, value, 0, count);
			}

			throw new NotImplementedException ();

//			position += locator.Write (value, (uint) Position, (uint) value.Length, LobType);
		}

		#endregion // Methods
	}
}
