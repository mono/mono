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
using System.IO;
using System.Data.OracleClient.Oci;
using System.Data.SqlTypes;

namespace System.Data.OracleClient {
	public sealed class OracleLob : Stream, ICloneable, INullable
	{
		#region Fields

		public static readonly new OracleLob Null = new OracleLob ();

		OracleConnection connection;
		bool isBatched = false;
		bool isOpen = true;
		bool notNull = false;
		OciLobLocator locator;

		#endregion // Fields

		#region Constructors

		internal OracleLob ()
		{
		}

		internal OracleLob (OciLobLocator locator)
		{
			notNull = true;
			this.locator = locator;
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
				throw new InvalidOperationException ();
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
			[MonoTODO]
			get { 
				if (Connection.State == ConnectionState.Closed)
					throw new InvalidOperationException ();
				if (!isOpen)
					throw new ObjectDisposedException ("OracleLob");
				throw new NotImplementedException ();
			}
		}

		public OracleType LobType {
			[MonoTODO]
			get { 
				throw new NotImplementedException (); 
			}
		}


		public override long Position {
			[MonoTODO]
			get { 
				if (Connection.State == ConnectionState.Closed)
					throw new InvalidOperationException ();
				if (!isOpen)
					throw new ObjectDisposedException ("OracleLob");
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				if (!isOpen)
					throw new ObjectDisposedException ("OracleLob");
				if (value > Length) 
					throw new ArgumentOutOfRangeException ();
				throw new NotImplementedException ();
			}
		}

		public object Value {
			[MonoTODO]
			get { 
				if (!isOpen)
					throw new ObjectDisposedException ("OracleLob");
				if (IsNull)
					return DBNull.Value;
				throw new NotImplementedException ();
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

		[MonoTODO]
		public void BeginBatch (OracleLobOpenMode mode)
		{
			isBatched = true;
		}

		[MonoTODO]
		public object Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Close ()
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public void EndBatch ()
		{
			isBatched = false;
		}

		[MonoTODO]
		public long Erase ()
		{
			throw new NotImplementedException ();
		}

		public long Erase (long offset, long amount)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Flush ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int Read (byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void SetLength (long value)
		{
			throw new InvalidOperationException ();
		}

		[MonoTODO]
		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}

		#endregion // Methods
	}
}
