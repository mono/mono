//
// OracleLob.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Data.OracleClient.Oci;
using System.Data.SqlTypes;
using System.IO;
using System.Text;

namespace System.Data.OracleClient
{
	public sealed class OracleLob : Stream, ICloneable, IDisposable, INullable
	{
		#region Fields

		public static readonly new OracleLob Null = new OracleLob ();

		internal OracleConnection connection;
		bool isBatched;
		bool isOpen = true;
		bool notNull;
		OciLobLocator locator;
		OracleType type;

		long length = -1;
		long position;

		#endregion // Fields

		#region Constructors

		internal OracleLob ()
		{
		}

		internal OracleLob (OciLobLocator locator, OciDataType ociType)
		{
			notNull = true;
			this.locator = locator;

			switch (ociType) {
			case OciDataType.Blob:
				type = OracleType.Blob;
				break;
			case OciDataType.Clob:
				type = OracleType.Clob;
				break;
			}
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
				return locator.GetChunkSize ();
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
				if (length >= 0)
					return length;
				return locator.GetLength (LobType == OracleType.Blob);
			}
		}

		public OracleType LobType {
			get { return type; }
		}

		internal OciLobLocator Locator {
			get { return locator; }
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
						return string.Empty;
					else // OracleType.Blob
						return new byte[0];
				}

				if (LobType == OracleType.Clob) {
					buffer = new byte [len];
					Read (buffer, 0, len);
					UnicodeEncoding encoding = new UnicodeEncoding ();
					return encoding.GetString (buffer);
				} else {
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
		public void Append (OracleLob source)
		{
			if (source.IsNull)
				throw new ArgumentNullException ();
			if (Connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ();
			throw new NotImplementedException ();
		}

		void AssertAmountIsEven (long amount, string argName)
		{
			if (amount % 2 == 1)
				throw new ArgumentOutOfRangeException ("CLOB and NCLOB parameters require even number of bytes for this argument.");
		}

		void AssertAmountIsValid (long amount, string argName)
		{
			if (amount > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException ("Argument too big.");
			if (LobType == OracleType.Clob || LobType == OracleType.NClob)
				AssertAmountIsEven (amount, argName);
		}

		void AssertConnectionIsOpen ()
		{
			if (connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ("Invalid operation. The connection is closed.");
		}

		void AssertObjectNotDisposed ()
		{
			if (!isOpen)
				throw new ObjectDisposedException ("OracleLob");
		}

		void AssertTransactionExists ()
		{
			if (connection.Transaction == null)
				throw new InvalidOperationException ("Modifying a LOB requires that the connection be transacted.");
		}

		public void BeginBatch ()
		{
			BeginBatch (OracleLobOpenMode.ReadOnly);
		}

		public void BeginBatch (OracleLobOpenMode mode)
		{
			AssertConnectionIsOpen ();
			AssertObjectNotDisposed ();

			locator.BeginBatch (mode);
			isBatched = true;
		}

		[MonoTODO]
		public object Clone ()
		{
			throw new NotImplementedException ();
		}

#if !NET_2_0
		[MonoTODO]
		public override void Close ()
		{
			Dispose (true);
		}

		[MonoTODO]
		public void Dispose ()
		{
			this.Dispose (true);
			GC.SuppressFinalize (this);
		}
#endif

#if NET_2_0
		protected override
#endif
		void Dispose (bool disposing)
		{
			if (disposing) {
				if (locator != null)
					locator.Dispose ();
			}
			locator = null;
			isOpen = false;
		}

		public long CopyTo (OracleLob destination)
		{
			return CopyTo (0, destination, 0, Length);
		}

		public long CopyTo (OracleLob destination, long destinationOffset)
		{
			return CopyTo (0, destination, destinationOffset, Length);
		}

		public long CopyTo (long sourceOffset, OracleLob destination, long destinationOffset, long amount)
		{
			if (destination.IsNull)
				throw new ArgumentNullException ();

			AssertAmountIsValid (sourceOffset, "sourceOffset");
			AssertAmountIsValid (destinationOffset, "destinationOffset");
			AssertAmountIsValid (amount, "amount");
			AssertTransactionExists ();
			AssertConnectionIsOpen ();

			return (long) locator.Copy (destination.Locator, (uint) amount, (uint) destinationOffset + 1, (uint) sourceOffset + 1);
		}

		public void EndBatch ()
		{
			AssertConnectionIsOpen ();
			AssertObjectNotDisposed ();

			locator.EndBatch ();
			isBatched = false;
		}

		public long Erase ()
		{
			return Erase (0, Length);
		}

		public long Erase (long offset, long amount)
		{
			if (offset < 0 || amount < 0)
				throw new ArgumentOutOfRangeException ("Must be a positive value.");
			if (offset + amount > Length)
				throw new ArgumentOutOfRangeException ();

			AssertAmountIsValid (offset, "offset");
			AssertAmountIsValid (amount, "amount");

			return (long) locator.Erase ((uint) offset + 1, (uint) amount);
		}

		public override void Flush ()
		{
			// No-op
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ();

			AssertAmountIsValid (offset, "offset");
			AssertAmountIsValid (count, "count");
			AssertConnectionIsOpen ();
			AssertObjectNotDisposed ();

			int bytesRead;
			byte[] output = new byte [count];

			position = offset;
			bytesRead = locator.Read (output, (uint) Position + 1, (uint) count, LobType == OracleType.Blob);
			output.CopyTo (buffer, offset);
			position += bytesRead;
			return bytesRead;
		}

		[MonoTODO]
		public override long Seek (long offset, SeekOrigin origin)
		{
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
		public override void SetLength (long value)
		{
			AssertAmountIsValid (value, "value");
			AssertTransactionExists ();
			AssertConnectionIsOpen ();
			AssertObjectNotDisposed ();

			locator.Trim ((uint) value);
			length = value;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
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
			position += locator.Write (value, (uint) Position + 1, (uint) value.Length, LobType);
		}

		#endregion // Methods
	}
}
