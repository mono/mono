//
// OracleBFile.cs 
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
using System.Data.SqlTypes;

namespace System.Data.OracleClient
{
	public sealed class OracleBFile : Stream, ICloneable, IDisposable, INullable
	{
		#region Fields

		public static readonly new OracleBFile Null = new OracleBFile ();

		//OracleConnection connection;
		//bool isOpen;
		//bool notNull;

		#endregion // Fields

		#region Constructors

		internal OracleBFile ()
		{
		}

		#endregion // Constructors

		#region Properties

		public override bool CanRead {
			get { 
				//return (IsNull || isOpen); 
				throw new NotImplementedException ();
			}
		}

		public override bool CanSeek {
			get { 
				//return (IsNull || isOpen);
				throw new NotImplementedException ();
			}
		}

		public override bool CanWrite {
			get { 
				//return false; 
				throw new NotImplementedException ();				
			}
		}

		public OracleConnection Connection {
			get { 				
				//return connection; 
				throw new NotImplementedException ();
			}
		}

		public string DirectoryName {
			[MonoTODO]
			get { 
				//if (!isOpen)
				//	throw new ObjectDisposedException ("OracleBFile");
				throw new NotImplementedException ();
			}
		}

		public bool FileExists {
			[MonoTODO]
			get { 
				//if (!isOpen)
				//	throw new ObjectDisposedException ("OracleBFile");
				//if (Connection.State == ConnectionState.Closed)
				//	throw new InvalidOperationException ();
				throw new NotImplementedException ();
			}
		}

		public string FileName {
			[MonoTODO]
			get {
				//if (!isOpen)
				//	throw new ObjectDisposedException ("OracleBFile");
				//if (IsNull)
				//	return String.Empty;
				throw new NotImplementedException ();
			}
		}

		public bool IsNull {
			get { 
				//return !notNull; 
				throw new NotImplementedException ();				
			}
		}

		public override long Length {
			[MonoTODO]
			get { 
				//if (!isOpen)
				//	throw new ObjectDisposedException ("OracleBFile");
				throw new NotImplementedException ();
			}
		}

		public override long Position {
			[MonoTODO]
			get { 
				//if (!isOpen)
				//	throw new ObjectDisposedException ("OracleBFile");
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				//if (!isOpen)
				//	throw new ObjectDisposedException ("OracleBFile");
				//if (value > Length) 
				//	throw new ArgumentOutOfRangeException ();
				throw new NotImplementedException ();
			}
		}

		public object Value {
			[MonoTODO]
			get { 
				throw new NotImplementedException ();
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public object Clone ()
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

#if NET_2_0
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}
#else
		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
#endif

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
		public void SetFileName (string directory, string file)
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


