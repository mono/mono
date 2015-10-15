//------------------------------------------------------------------------------
// <copyright file="SQLBytes.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>																
// <owner current="true" primary="true">junfang</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

//**************************************************************************
// @File: SqlBytes.cs
// @Owner: junfang
//
// Created by:	JunFang
//
// Description: Class SqlBytes is used to represent a binary/varbinary/image
//		data from SQL Server. It contains a byte array buffer, which can
//		be refilled. For example, in data access, user could use one instance
//		of sqlbytes to bind to a binary column, and we will just keep copying
//		the data into the same instance, and avoid allocation per row.
//		It is also used to construct a UDT from on-disk binary value.
//
// Notes: 
//	
// History:
//
//     @Version: Yukon
//     120214 JXF  09/23/02 SqlBytes/SqlChars class indexer
//     112296 AZA  07/06/02 Seal SqlAccess classes.
//     107151 AZA  04/18/02 Track byte array buffer as well as SqlBytes in 
//                          sqlaccess.
//     107216 JXF  04/17/02 











namespace System.Data.SqlTypes {
    using System;
	using System.Diagnostics;
    using System.Data.Common;
    using System.Data.Sql;
    using System.Data.SqlClient;
	using System.Data.SqlTypes;
	using System.IO;
	using System.Runtime.InteropServices;
	using System.Runtime.Serialization;
	using System.Security.Permissions;
	using System.Xml;
	using System.Xml.Schema;
	using System.Xml.Serialization;

	[Serializable]
	internal enum SqlBytesCharsState {
		Null = 0,
		Buffer = 1,
		//IntPtr = 2,
		Stream = 3,
	}

	[Serializable,XmlSchemaProvider("GetXsdType")]
    public sealed class SqlBytes : System.Data.SqlTypes.INullable, IXmlSerializable, ISerializable {
		// --------------------------------------------------------------
		//	  Data members
		// --------------------------------------------------------------

		// SqlBytes has five possible states
		// 1) SqlBytes is Null
		//		- m_stream must be null, m_lCuLen must be x_lNull.
		// 2) SqlBytes contains a valid buffer, 
		//		- m_rgbBuf must not be null,m_stream must be null
		// 3) SqlBytes contains a valid pointer
		//		- m_rgbBuf could be null or not,
		//			if not null, content is garbage, should never look into it.
		//      - m_stream must be null.
		// 4) SqlBytes contains a Stream
		//      - m_stream must not be null
		//      - m_rgbBuf could be null or not. if not null, content is garbage, should never look into it.
		//		- m_lCurLen must be x_lNull.
		// 5) SqlBytes contains a Lazy Materialized Blob (ie, StorageState.Delayed)
		//
		internal byte[]	            m_rgbBuf;	// Data buffer
		private  long	            m_lCurLen;	// Current data length
		internal Stream             m_stream;
		private  SqlBytesCharsState m_state;

		private  byte[]	            m_rgbWorkBuf;	// A 1-byte work buffer.

		// The max data length that we support at this time.
		private  const long x_lMaxLen = (long)System.Int32.MaxValue;

		private  const long x_lNull = -1L;

		// --------------------------------------------------------------
		//	  Constructor(s)
		// --------------------------------------------------------------

		// Public default constructor used for XML serialization
		public SqlBytes() {
			SetNull();
		}

		// Create a SqlBytes with an in-memory buffer
		public SqlBytes(byte[] buffer) {
			m_rgbBuf = buffer;
			m_stream = null;
			if (m_rgbBuf == null) {
				m_state = SqlBytesCharsState.Null;
				m_lCurLen = x_lNull;
            }
			else {
				m_state = SqlBytesCharsState.Buffer;
				m_lCurLen = (long)m_rgbBuf.Length;
            }

			m_rgbWorkBuf = null;

			AssertValid();
		}

		// Create a SqlBytes from a SqlBinary
		public SqlBytes(SqlBinary value) : this(value.IsNull ? (byte[])null : value.Value)	{
        }

		public SqlBytes(Stream s) {
    		// Create a SqlBytes from a Stream
			m_rgbBuf = null;
			m_lCurLen = x_lNull;
			m_stream = s;
			m_state = (s == null) ? SqlBytesCharsState.Null : SqlBytesCharsState.Stream;

			m_rgbWorkBuf = null;

			AssertValid();
        }

		// Constructor required for serialization. Deserializes as a Buffer. If the bits have been tampered with
		// then this will throw a SerializationException or a InvalidCastException.
		private SqlBytes(SerializationInfo info, StreamingContext context)
			{
			m_stream = null;
			m_rgbWorkBuf = null;

			if (info.GetBoolean("IsNull"))
				{
				m_state = SqlBytesCharsState.Null;
				m_rgbBuf = null;
				}
			else
				{
				m_state = SqlBytesCharsState.Buffer;
				m_rgbBuf = (byte[]) info.GetValue("data", typeof(byte[]));
				m_lCurLen = m_rgbBuf.Length;
				}

			AssertValid();
			}


		// --------------------------------------------------------------
		//	  Public properties
		// --------------------------------------------------------------

		// INullable
		public bool IsNull {
			get {
				return m_state == SqlBytesCharsState.Null; 
			}
		}

		// Property: the in-memory buffer of SqlBytes
		//		Return Buffer even if SqlBytes is Null.
		public byte[] Buffer {
			get {
				if (FStream())	{
					CopyStreamToBuffer();
				}
				return m_rgbBuf;
			}
		}

		// Property: the actual length of the data
		public long Length {
			get {
				switch (m_state) {
					case SqlBytesCharsState.Null: 
                        throw new SqlNullValueException();

					case SqlBytesCharsState.Stream:
						return m_stream.Length;

					default:
						return m_lCurLen;
				}
			}
		}

		// Property: the max length of the data
		//		Return MaxLength even if SqlBytes is Null.
		//		When the buffer is also null, return -1.
		//		If containing a Stream, return -1.
		public long MaxLength {
			get {
				switch (m_state) {
					case SqlBytesCharsState.Stream:
						return -1L;

					default:
						return (m_rgbBuf == null) ? -1L : (long)m_rgbBuf.Length;
                }
            }
        }

		// Property: get a copy of the data in a new byte[] array.
		public byte[] Value {
			get {
				byte[] buffer;

				switch (m_state) {
					case SqlBytesCharsState.Null: 
						throw new SqlNullValueException();

					case SqlBytesCharsState.Stream:
						if (m_stream.Length > x_lMaxLen)
                                            throw new SqlTypeException(Res.GetString(Res.SqlMisc_BufferInsufficientMessage));       
						buffer = new byte[m_stream.Length];
						if (m_stream.Position != 0)
							m_stream.Seek(0, SeekOrigin.Begin);
						m_stream.Read(buffer, 0, checked((int)m_stream.Length));
						break;

					default:
						buffer = new byte[m_lCurLen];
						Array.Copy(m_rgbBuf, buffer, (int)m_lCurLen);
						break;
                }

				return buffer;
            }
        }

		// class indexer
		public byte this[long offset] {
			get {
                if (offset < 0 || offset >= this.Length)
                    throw new ArgumentOutOfRangeException("offset");

                if (m_rgbWorkBuf == null)
					m_rgbWorkBuf = new byte[1];

                Read(offset, m_rgbWorkBuf, 0, 1);
				return m_rgbWorkBuf[0];
            }
			set {
				if (m_rgbWorkBuf == null)
					m_rgbWorkBuf = new byte[1];
				m_rgbWorkBuf[0] = value;
				Write(offset, m_rgbWorkBuf, 0, 1);
            }
        }

		public StorageState Storage {
			get {
				switch (m_state) {
					case SqlBytesCharsState.Null: 
						throw new SqlNullValueException();
					case SqlBytesCharsState.Stream:
					    return StorageState.Stream;

					case SqlBytesCharsState.Buffer:
					    return StorageState.Buffer;

					default:
					    return StorageState.UnmanagedBuffer;
				}
			}
		}

        public Stream Stream {
            get {
    			return FStream() ? m_stream : new StreamOnSqlBytes(this);
            }
            set {
    			m_lCurLen = x_lNull;
    			m_stream = value;
    			m_state = (value == null) ? SqlBytesCharsState.Null : SqlBytesCharsState.Stream;
    			AssertValid();
            }
        }

		// --------------------------------------------------------------
		//	  Public methods
		// --------------------------------------------------------------

		public void SetNull() {
    		m_lCurLen = x_lNull;
    		m_stream = null;
    		m_state = SqlBytesCharsState.Null;

    		AssertValid();
		}

		// Set the current length of the data
		// If the SqlBytes is Null, setLength will make it non-Null.
		public void SetLength(long value) {
			if (value < 0)
				throw new ArgumentOutOfRangeException("value");

			if (FStream()) { 
				m_stream.SetLength(value);
			}
			else {
				// If there is a buffer, even the value of SqlBytes is Null,
				// still allow setting length to zero, which will make it not Null.
				// If the buffer is null, raise exception
				//
				if (null == m_rgbBuf)
                    throw new SqlTypeException(Res.GetString(Res.SqlMisc_NoBufferMessage));

				if (value > (long)m_rgbBuf.Length)
					throw new ArgumentOutOfRangeException("value");

				else if (IsNull)
					// At this point we know that value is small enough
					// Go back in buffer mode
					m_state = SqlBytesCharsState.Buffer;

				m_lCurLen = value;
            }

			AssertValid();
        }

		// Read data of specified length from specified offset into a buffer
		public long Read(long offset, byte[] buffer, int offsetInBuffer, int count) {
			if (IsNull)
                throw new SqlNullValueException();

			// Validate the arguments
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			if (offset > this.Length || offset < 0)
				throw new ArgumentOutOfRangeException("offset");

			if (offsetInBuffer > buffer.Length || offsetInBuffer < 0)
				throw new ArgumentOutOfRangeException("offsetInBuffer");

			if (count < 0 || count > buffer.Length - offsetInBuffer)
				throw new ArgumentOutOfRangeException("count");

			// Adjust count based on data length
			if (count > this.Length - offset)
				count = (int)(this.Length - offset);

			if (count != 0)	{
				switch (m_state) {
                    case SqlBytesCharsState.Stream:
						if (m_stream.Position != offset)
							m_stream.Seek(offset, SeekOrigin.Begin);
						m_stream.Read(buffer, offsetInBuffer, count);
						break;

					default:
						Array.Copy(m_rgbBuf, offset, buffer, offsetInBuffer, count);
						break;
    			}
			}
			return count;
		}

		// Write data of specified length into the SqlBytes from specified offset
		public void Write(long offset, byte[] buffer, int offsetInBuffer, int count) {
			if (FStream()) {
				if (m_stream.Position != offset)
					m_stream.Seek(offset, SeekOrigin.Begin);
				m_stream.Write(buffer, offsetInBuffer, count);
            }
			else {
				// Validate the arguments
				if (buffer == null)
					throw new ArgumentNullException("buffer");

				if (m_rgbBuf == null)
                    throw new SqlTypeException(Res.GetString(Res.SqlMisc_NoBufferMessage));

				if (offset < 0)
					throw new ArgumentOutOfRangeException("offset");
				if (offset > m_rgbBuf.Length)
                                    throw new SqlTypeException(Res.GetString(Res.SqlMisc_BufferInsufficientMessage));

				if (offsetInBuffer < 0 || offsetInBuffer > buffer.Length)
					throw new ArgumentOutOfRangeException("offsetInBuffer");

				if (count < 0 || count > buffer.Length - offsetInBuffer)
					throw new ArgumentOutOfRangeException("count");

				if (count > m_rgbBuf.Length - offset)
                    throw new SqlTypeException(Res.GetString(Res.SqlMisc_BufferInsufficientMessage));

				if (IsNull) {
					// If NULL and there is buffer inside, we only allow writing from 
					// offset zero.
					//
					if (offset != 0)
                        throw new SqlTypeException(Res.GetString(Res.SqlMisc_WriteNonZeroOffsetOnNullMessage)); 

					// treat as if our current length is zero.
					// Note this has to be done after all inputs are validated, so that
					// we won't throw exception after this point.
					//
					m_lCurLen = 0;
                    m_state = SqlBytesCharsState.Buffer;
				}
				else if (offset > m_lCurLen) {
					// Don't allow writing from an offset that this larger than current length.
					// It would leave uninitialized data in the buffer.
					//
                    throw new SqlTypeException(Res.GetString(Res.SqlMisc_WriteOffsetLargerThanLenMessage));
				}

				if (count != 0)	{
					Array.Copy(buffer, offsetInBuffer, m_rgbBuf, offset, count);

					// If the last position that has been written is after
					// the current data length, reset the length
					if (m_lCurLen < offset + count)
						m_lCurLen = offset + count;
				}
			}

    		AssertValid();
		}

		public SqlBinary ToSqlBinary() {
			return IsNull ? SqlBinary.Null : new SqlBinary(Value);
		}

		// --------------------------------------------------------------
		//	  Conversion operators
		// --------------------------------------------------------------

		// Alternative method: ToSqlBinary()
		public static explicit operator SqlBinary(SqlBytes value) {
			return value.ToSqlBinary();
		}

		// Alternative method: constructor SqlBytes(SqlBinary)
		public static explicit operator SqlBytes(SqlBinary value) {
			return new SqlBytes(value);
		}

		// --------------------------------------------------------------
		//	  Private utility functions
		// --------------------------------------------------------------

		[System.Diagnostics.Conditional("DEBUG")] 
		private void AssertValid() {
    		Debug.Assert(m_state >= SqlBytesCharsState.Null && m_state <= SqlBytesCharsState.Stream);

			if (IsNull) {
			}
			else {
				Debug.Assert((m_lCurLen >= 0 && m_lCurLen <= x_lMaxLen) || FStream());
				Debug.Assert(FStream() || (m_rgbBuf != null && m_lCurLen <= m_rgbBuf.Length));
				Debug.Assert(!FStream() || (m_lCurLen == x_lNull));
			}
			Debug.Assert(m_rgbWorkBuf == null || m_rgbWorkBuf.Length == 1);
		}

		// Copy the data from the Stream to the array buffer.
		// If the SqlBytes doesn't hold a buffer or the buffer
		// is not big enough, allocate new byte array.
		private void CopyStreamToBuffer() {
			Debug.Assert(FStream());

			long lStreamLen = m_stream.Length;
			if (lStreamLen >= x_lMaxLen)
                           throw new SqlTypeException(Res.GetString(Res.SqlMisc_WriteOffsetLargerThanLenMessage));

			if (m_rgbBuf == null || m_rgbBuf.Length < lStreamLen)
				m_rgbBuf = new byte[lStreamLen];

			if (m_stream.Position != 0)
				m_stream.Seek(0, SeekOrigin.Begin);

			m_stream.Read(m_rgbBuf, 0, (int)lStreamLen);
			m_stream = null;
			m_lCurLen = lStreamLen;
			m_state = SqlBytesCharsState.Buffer;

			AssertValid();
        }

		// whether the SqlBytes contains a pointer
		// whether the SqlBytes contains a Stream
		internal bool FStream()	{
			return m_state == SqlBytesCharsState.Stream;
        }

		private void SetBuffer(byte[] buffer) {
			m_rgbBuf = buffer;
			m_lCurLen = (m_rgbBuf == null) ? x_lNull : (long)m_rgbBuf.Length;
			m_stream = null;
			m_state = (m_rgbBuf == null) ? SqlBytesCharsState.Null : SqlBytesCharsState.Buffer;

			AssertValid();
        }

		// --------------------------------------------------------------
		// 		XML Serialization
		// --------------------------------------------------------------

		XmlSchema IXmlSerializable.GetSchema() { 
			return null; 
        }
		
		void IXmlSerializable.ReadXml(XmlReader r) {
			byte[] value = null;
			
 			string isNull = r.GetAttribute("nil", XmlSchema.InstanceNamespace);
 			
			if (isNull != null && XmlConvert.ToBoolean(isNull)) {
                // VSTFDevDiv# 479603 - SqlTypes read null value infinitely and never read the next value. Fix - Read the next value.
                r.ReadElementString();
                SetNull();
			}
			else {
				string base64 = r.ReadElementString();
				if (base64 == null) {
					value = new byte[0];
				}
				else {
					base64 = base64.Trim();
					if (base64.Length == 0) 
						value = new byte[0];
					else 
						value = Convert.FromBase64String(base64);
				}
			}
			
            SetBuffer(value);
		}

		void IXmlSerializable.WriteXml(XmlWriter writer) {
			if (IsNull) {
				writer.WriteAttributeString("xsi", "nil", XmlSchema.InstanceNamespace, "true");
			}
			else {
				byte[] value = this.Buffer;
				writer.WriteString(Convert.ToBase64String(value, 0, (int)(this.Length)));
            }
        }

		public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet) {
			return new XmlQualifiedName("base64Binary", XmlSchema.Namespace);
		}


		// --------------------------------------------------------------
		// 		Serialization using ISerializable
		// --------------------------------------------------------------

		// State information is not saved. The current state is converted to Buffer and only the underlying
		// array is serialized, except for Null, in which case this state is kept.
		[SecurityPermissionAttribute(SecurityAction.LinkDemand,SerializationFormatter=true)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) 
		{
			switch (m_state)
			{
				case SqlBytesCharsState.Null:
					info.AddValue("IsNull", true);
					break;

				case SqlBytesCharsState.Buffer:
					info.AddValue("IsNull", false);
					info.AddValue("data", m_rgbBuf);
					break;

				case SqlBytesCharsState.Stream:
					CopyStreamToBuffer();
					goto case SqlBytesCharsState.Buffer;

				default:
					Debug.Assert(false);
					goto case SqlBytesCharsState.Null;
					
			}
		}

		// --------------------------------------------------------------
		//	  Static fields, properties
		// --------------------------------------------------------------

		// Get a Null instance. 
		// Since SqlBytes is mutable, have to be property and create a new one each time.
		public static SqlBytes Null {
			get	{
				return new SqlBytes((byte[])null);
			}
		}
	} // class SqlBytes

	// StreamOnSqlBytes is a stream build on top of SqlBytes, and
	// provides the Stream interface. The purpose is to help users
	// to read/write SqlBytes object. After getting the stream from
	// SqlBytes, users could create a BinaryReader/BinaryWriter object
	// to easily read and write primitive types.
	internal sealed class StreamOnSqlBytes : Stream
		{
		// --------------------------------------------------------------
		//	  Data members
		// --------------------------------------------------------------

		private SqlBytes	m_sb;		// the SqlBytes object 
		private long		m_lPosition;

		// --------------------------------------------------------------
		//	  Constructor(s)
		// --------------------------------------------------------------

		internal StreamOnSqlBytes(SqlBytes sb) {
			m_sb = sb;
			m_lPosition = 0;
		}

		// --------------------------------------------------------------
		//	  Public properties
		// --------------------------------------------------------------

		// Always can read/write/seek, unless sb is null, 
		// which means the stream has been closed.

		public override bool CanRead {
			get	{
				return m_sb != null && !m_sb.IsNull;
			}
		}

		public override bool CanSeek {
			get {
				return m_sb != null;
			}
		}

		public override bool CanWrite {
			get	{
				return m_sb != null && (!m_sb.IsNull || m_sb.m_rgbBuf != null);
			}
		}

		public override long Length	{
			get	{
    			CheckIfStreamClosed("get_Length");
	    		return m_sb.Length;
            }
        }

		public override long Position {
			get	{
				CheckIfStreamClosed("get_Position");
				return m_lPosition;
			}
			set	{
				CheckIfStreamClosed("set_Position");
				if (value < 0 || value > m_sb.Length)
					throw new ArgumentOutOfRangeException("value");
				else
					m_lPosition = value;
			}
		}

		// --------------------------------------------------------------
		//	  Public methods
		// --------------------------------------------------------------

		public override long Seek(long offset, SeekOrigin origin) {
			CheckIfStreamClosed("Seek");

			long lPosition = 0;

			switch(origin) {
				case SeekOrigin.Begin:
					if (offset < 0 || offset > m_sb.Length)
						throw new ArgumentOutOfRangeException("offset");
					m_lPosition = offset;
					break;
					
				case SeekOrigin.Current:
					lPosition = m_lPosition + offset;
					if (lPosition < 0 || lPosition > m_sb.Length)
						throw new ArgumentOutOfRangeException("offset");
					m_lPosition = lPosition;
					break;
					
				case SeekOrigin.End:
					lPosition = m_sb.Length + offset;
					if (lPosition < 0 || lPosition > m_sb.Length)
						throw new ArgumentOutOfRangeException("offset");
					m_lPosition = lPosition;
					break;
					
				default:
                                throw ADP.InvalidSeekOrigin("offset");
			}

			return m_lPosition;
		}

		// The Read/Write/ReadByte/WriteByte simply delegates to SqlBytes
		public override int Read(byte[] buffer, int offset, int count) {
			CheckIfStreamClosed("Read");

			if (buffer==null)
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || count > buffer.Length - offset)
				throw new ArgumentOutOfRangeException("count");

			int iBytesRead = (int)m_sb.Read(m_lPosition, buffer, offset, count);
			m_lPosition += iBytesRead;

			return iBytesRead;
		}

		public override void Write(byte[] buffer, int offset, int count) {
			CheckIfStreamClosed("Write");

			if (buffer==null)
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || count > buffer.Length - offset)
				throw new ArgumentOutOfRangeException("count");

			m_sb.Write(m_lPosition, buffer, offset, count);
			m_lPosition += count;
		}

		public override int ReadByte() {
			CheckIfStreamClosed("ReadByte");

			// If at the end of stream, return -1, rather than call SqlBytes.ReadByte,
			// which will throw exception. This is the behavior for Stream.
			//
			if (m_lPosition >= m_sb.Length)
				return -1;

			int ret = m_sb[m_lPosition];
			m_lPosition ++;
			return ret;
		}

		public override void WriteByte(byte value) {
			CheckIfStreamClosed("WriteByte");

			m_sb[m_lPosition] = value;
			m_lPosition ++;
		}

		public override void SetLength(long value) {
			CheckIfStreamClosed("SetLength");

			m_sb.SetLength(value);
			if (m_lPosition > value)
				m_lPosition = value;
		}

		// Flush is a no-op for stream on SqlBytes, because they are all in memory
		public override void Flush() {
			if (m_sb.FStream())
				m_sb.m_stream.Flush();
		}

		protected override void Dispose(bool disposing) {
			// When m_sb is null, it means the stream has been closed, and
			// any opearation in the future should fail.
			// This is the only case that m_sb is null.
            try {
			m_sb = null;
		}
            finally {
                base.Dispose(disposing);
            }
		}

		// --------------------------------------------------------------
		//	  Private utility functions
		// --------------------------------------------------------------

		private bool FClosed() {
			return m_sb == null;
		}

        private void CheckIfStreamClosed(string methodname) {
			if (FClosed())
                throw ADP.StreamClosed(methodname);
        }
    } // class StreamOnSqlBytes
} // namespace System.Data.SqlTypes
