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

namespace System.Data.OracleClient {
	public sealed class OracleLob : Stream, ICloneable, INullable
	{
		#region Fields

		public static readonly new OracleLob Null = new OracleLob ();

		internal OracleConnection connection;
		bool isBatched = false;
		bool isOpen = true;
		bool notNull = false;
		OciLobLocator locator;
		OracleType type;

		long length = -1;
		long position = 1;

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
				if (Connection.State == ConnectionState.Closed)
					throw new InvalidOperationException ();
				if (!isOpen)
					throw new ObjectDisposedException ("OracleLob");
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
				if (Connection.State == ConnectionState.Closed)
					throw new InvalidOperationException ();
				if (!isOpen)
					throw new ObjectDisposedException ("OracleLob");
				throw new NotImplementedException ();
			}
		}

		public override long Length {
			get { 
				if (Connection.State == ConnectionState.Closed)
					throw new InvalidOperationException ();
				if (!isOpen)
					throw new ObjectDisposedException ("OracleLob");
				if (length >= 0)
					return length;
				return locator.GetLength (LobType == OracleType.Blob);
			}
		}

		public OracleType LobType {
			get { return type; }
		}


		public override long Position {
			get { 
				if (Connection.State == ConnectionState.Closed)
					throw new InvalidOperationException ();
				if (!isOpen)
					throw new ObjectDisposedException ("OracleLob");
				return position;
			}
			set {
				if (!isOpen)
					throw new ObjectDisposedException ("OracleLob");
				if (value > Length) 
					throw new ArgumentOutOfRangeException ();
				position = value;
			}
		}

		public object Value {
			get { 
				if (!isOpen)
					throw new ObjectDisposedException ("OracleLob");
				if (IsNull)
					return DBNull.Value;
				
				byte[] buffer = new byte [Length];
				Read (buffer, 1, (int) Length);

				if (LobType == OracleType.Clob)
					return (new UnicodeEncoding ()).GetString (buffer);
				return buffer;
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

		public void BeginBatch ()
		{
			BeginBatch (OracleLobOpenMode.ReadOnly);
		}

		public void BeginBatch (OracleLobOpenMode mode)
		{
			isBatched = true;
			locator.BeginBatch (mode);
		}

		[MonoTODO]
		public object Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Close ()
		{
			locator.Dispose ();
			isOpen = false;
		}

		[MonoTODO]
		public long CopyTo (OracleLob destination)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long CopyTo (OracleLob destination, long destinationOffset)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long CopyTo (long sourceOffset, OracleLob destination, long destinationOffset, long amount)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		public void EndBatch ()
		{
			locator.EndBatch ();
			isBatched = false;
		}

		public long Erase ()
		{
			return Erase (1, Length);
		}

		public long Erase (long offset, long amount)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException ();
			if (amount < 0)
				throw new ArgumentOutOfRangeException ();
			if (offset + amount > Length)
				throw new ArgumentOutOfRangeException ();
			if (offset > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException ();
			if (amount > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException ();
			return (long) locator.Erase ((uint) offset + 1, (uint) amount);
		}

		public override void Flush ()
		{
			// No-op
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			int bytesRead;
			byte[] output = new byte[count];

			bytesRead = locator.Read (output, (uint) Position, (uint) count, LobType == OracleType.Blob);
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
				newPosition = Length - offset;
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
			if ((LobType == OracleType.Clob || LobType == OracleType.NClob) && (value % 2) == 1)
				throw new ArgumentOutOfRangeException ();
			if (value > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException ();
			if (connection.Transaction == null)
				throw new InvalidOperationException ();
			if (IsNull)
				throw new InvalidOperationException ();
			if (Connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ();
			if (!isOpen)
				throw new ObjectDisposedException ("OracleLob");

			locator.Trim ((uint) value);
			length = value;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("Buffer is null.");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("Offset parameter must be positive.");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("Count parameter must be positive.");
			if (offset + count > buffer.Length)
				throw new ArgumentOutOfRangeException ("Offset + Count > buffer Length.");
			if (offset > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException ("Offset too big.");
			if (count > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException ("Count too big.");
			if (LobType == OracleType.Clob || LobType == OracleType.NClob) {
				if (offset % 2 == 1)
					throw new ArgumentOutOfRangeException ("Offset must be even.");
				if (count % 2 == 1)
					throw new ArgumentOutOfRangeException ("Count must be even.");
			}
			if (connection.Transaction == null)
				throw new InvalidOperationException ("Transaction is null.");
			if (IsNull)
				throw new InvalidOperationException ("LOB is null.");
			if (connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ("Connection is closed.");
			if (!isOpen)
				throw new ObjectDisposedException ("OracleLob");

			if (offset + count == buffer.Length && offset == 0)
				position += locator.Write (buffer, (uint) Position, (uint) buffer.Length, LobType);
			else {
				byte[] copy = new byte [count];
				Array.Copy (buffer, offset, copy, 0, count);
				position += locator.Write (copy, (uint) Position, (uint) copy.Length, LobType);
			}
		}

		#endregion // Methods
	}
}
