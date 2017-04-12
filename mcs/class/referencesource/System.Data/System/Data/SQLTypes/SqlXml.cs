//------------------------------------------------------------------------------
// <copyright file="SqlXmlReader.cs" company="Microsoft">
//	   Copyright (c) Microsoft Corporation.  All rights reserved.
//	</copyright>																
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

//**************************************************************************
//				Copyright (c) 1988-2000 Microsoft Corporation.
//
// @File: SqlXml.cs
//
// @Owner: junfung
// @Test: jstowe
//
// Purpose: Implementation of SqlXml which is equivalent to 
//			  data type "xml" in SQL Server
//
// Notes: 
//	  
// History:
//
// @EndHeader@
//**************************************************************************

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Reflection;

namespace System.Data.SqlTypes
{
	[Serializable, XmlSchemaProvider("GetXsdType")]
	public sealed class SqlXml: System.Data.SqlTypes.INullable, IXmlSerializable {
		private bool m_fNotNull; // false if null, the default ctor (plain 0) will make it Null
		private Stream m_stream;
		private bool firstCreateReader;
		private MethodInfo createSqlReaderMethodInfo;
        private readonly static Func<Stream, XmlReaderSettings, XmlParserContext, XmlReader> sqlReaderDelegate = CreateSqlReaderDelegate();

        private readonly static XmlReaderSettings DefaultXmlReaderSettings = new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment };
        private readonly static XmlReaderSettings DefaultXmlReaderSettingsCloseInput = new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment, CloseInput = true };
        private static MethodInfo s_createSqlReaderMethodInfo;
		
		public SqlXml() {
			SetNull();
		}

		// constructor
		// construct a Null
		private SqlXml(bool fNull) {
			SetNull();
		}

		public SqlXml(XmlReader value) {
			// whoever pass in the XmlReader is responsible for closing it			  
			if (value == null) {
				SetNull();
			}
			else {
				m_fNotNull = true;			  
				firstCreateReader = true;
				m_stream = CreateMemoryStreamFromXmlReader(value);
			}
		}

		public SqlXml(Stream value) {
			// whoever pass in the stream is responsible for closing it
			// similar to SqlBytes implementation
			if (value == null) {
				SetNull();
			}
			else  {
				firstCreateReader = true;
				m_fNotNull = true;
				m_stream = value;
			}
		}

        public XmlReader CreateReader() {
            if (IsNull) {
				throw new SqlNullValueException();
            }
			
			SqlXmlStreamWrapper stream = new SqlXmlStreamWrapper(m_stream);
			
			// if it is the first time we create reader and stream does not support CanSeek, no need to reset position
			if ((!firstCreateReader || stream.CanSeek) && stream.Position != 0) {
				stream.Seek(0, SeekOrigin.Begin);
            }

            // NOTE: Maintaining createSqlReaderMethodInfo private field member to preserve the serialization of the class
            if (createSqlReaderMethodInfo == null) {
                createSqlReaderMethodInfo = CreateSqlReaderMethodInfo;
            }
            Debug.Assert(createSqlReaderMethodInfo != null, "MethodInfo reference for XmlReader.CreateSqlReader should not be null.");

            XmlReader r = CreateSqlXmlReader(stream);
			firstCreateReader = false;
			return r;
        }

		internal static XmlReader CreateSqlXmlReader(Stream stream, bool closeInput = false, bool throwTargetInvocationExceptions = false) {
            // Call the internal delegate
            XmlReaderSettings settingsToUse = closeInput ? DefaultXmlReaderSettingsCloseInput : DefaultXmlReaderSettings;
            try {
                return sqlReaderDelegate(stream, settingsToUse, null);
            }
            // Dev11 Bug #315513: Exception type breaking change from 4.0 RTM when calling GetChars on null xml
            // For particular callers, we need to wrap all exceptions inside a TargetInvocationException to simulate calling CreateSqlReader via MethodInfo.Invoke
            catch (Exception ex) {
                if ((!throwTargetInvocationExceptions) || (!ADP.IsCatchableExceptionType(ex))) {
                    throw;
                }
                else {
                    throw new TargetInvocationException(ex);
                }
            }
		}

        // NOTE: ReflectionPermission required here for accessing the non-public internal method CreateSqlReader() of System.Xml regardless of its grant set.
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
        private static Func<Stream, XmlReaderSettings, XmlParserContext, XmlReader> CreateSqlReaderDelegate()
        {
            Debug.Assert(CreateSqlReaderMethodInfo != null, "MethodInfo reference for XmlReader.CreateSqlReader should not be null.");
            return (Func<Stream, XmlReaderSettings, XmlParserContext, XmlReader>)Delegate.CreateDelegate(typeof(Func<Stream, XmlReaderSettings, XmlParserContext, XmlReader>), CreateSqlReaderMethodInfo);
        }

        private static MethodInfo CreateSqlReaderMethodInfo {
            get {
                if (s_createSqlReaderMethodInfo == null) {
                    s_createSqlReaderMethodInfo = typeof(System.Xml.XmlReader).GetMethod("CreateSqlReader", BindingFlags.Static | BindingFlags.NonPublic);
                }

                return s_createSqlReaderMethodInfo;
            }
        }
		
		// INullable
		public bool IsNull {
			get { return !m_fNotNull;}
		}

		public string Value {
			get {
				if (IsNull)
					throw new SqlNullValueException();

				StringWriter sw = new StringWriter((System.IFormatProvider)null);
				XmlWriterSettings writerSettings = new XmlWriterSettings();
				writerSettings.CloseOutput = false;		// don't close the memory stream
				writerSettings.ConformanceLevel = ConformanceLevel.Fragment;
				XmlWriter ww = XmlWriter.Create(sw, writerSettings);				
				
				XmlReader reader = this.CreateReader();

				if (reader.ReadState == ReadState.Initial)
					reader.Read();

				while (!reader.EOF) {
					ww.WriteNode(reader, true);
				}	  
				ww.Flush();
				
				return sw.ToString();			
			}
		}

		public static SqlXml Null {
			get {
				return new SqlXml(true);
			}
		}

		private void SetNull() {
			m_fNotNull = false;
			m_stream = null;
			firstCreateReader = true;
		}

		private Stream CreateMemoryStreamFromXmlReader(XmlReader reader) {
			XmlWriterSettings writerSettings = new XmlWriterSettings();
			writerSettings.CloseOutput = false;		// don't close the memory stream
			writerSettings.ConformanceLevel = ConformanceLevel.Fragment;
			writerSettings.Encoding = Encoding.GetEncoding("utf-16");
			writerSettings.OmitXmlDeclaration = true;
			MemoryStream writerStream = new MemoryStream();
			XmlWriter ww = XmlWriter.Create(writerStream, writerSettings);		   

			if (reader.ReadState == ReadState.Closed)
				throw new InvalidOperationException(SQLResource.ClosedXmlReaderMessage);

			if (reader.ReadState == ReadState.Initial)
				reader.Read();
			
			while (!reader.EOF) {
				ww.WriteNode(reader, true);
			}	  
			ww.Flush();
			// set the stream to the beginning			
			writerStream.Seek(0, SeekOrigin.Begin);
			return writerStream;
		}

		XmlSchema IXmlSerializable.GetSchema() { 
			return null; 
		}
			
		void IXmlSerializable.ReadXml(XmlReader r) {
				string isNull = r.GetAttribute("nil", XmlSchema.InstanceNamespace);
				
			if (isNull != null && XmlConvert.ToBoolean(isNull)) {
                // VSTFDevDiv# 479603 - SqlTypes read null value infinitely and never read the next value. Fix - Read the next value.
                r.ReadInnerXml();
                SetNull();
			}
			else {
				m_fNotNull = true;  
				firstCreateReader = true;

				m_stream = new MemoryStream();
				StreamWriter sw = new StreamWriter(m_stream);
				sw.Write(r.ReadInnerXml());
				sw.Flush();

				if (m_stream.CanSeek)
					m_stream.Seek(0, SeekOrigin.Begin);
			}
		}

		void IXmlSerializable.WriteXml(XmlWriter writer) 
        {
			if (IsNull) 
            {
				writer.WriteAttributeString("xsi", "nil", XmlSchema.InstanceNamespace, "true");
			}
			else 
            {
                // VSTFDevDiv Bug 197567 - [SqlXml Column Read from SQL Server 2005 Fails to XML Serialize (writes raw binary)]
                // Instead of the WriteRaw use the WriteNode. As Tds sends a binary stream - Create a XmlReader to convert 
                // get the Xml string value from the binary and call WriteNode to pass that out to the XmlWriter.
                XmlReader reader = this.CreateReader();
                if (reader.ReadState == ReadState.Initial)
                    reader.Read();

                while (!reader.EOF)
                {
                    writer.WriteNode(reader, true);
                }
			}
            writer.Flush();
        }

		public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet) {
			return new XmlQualifiedName("anyType", XmlSchema.Namespace);
		}
	} // SqlXml 		

	// two purposes for this class
	// 1) keep its internal position so one reader positions on the orginial stream 
	//	  will not interface with the other
	// 2) when xmlreader calls close, do not close the orginial stream
	//
	internal sealed class SqlXmlStreamWrapper : Stream	{
		// --------------------------------------------------------------
	 //	  Data members
	 // --------------------------------------------------------------

		private Stream	m_stream;		
		private long	m_lPosition;
		bool m_isClosed;
		
		// --------------------------------------------------------------
		//	  Constructor(s)
		// --------------------------------------------------------------

		internal SqlXmlStreamWrapper(Stream stream) {
			m_stream  = stream;
			Debug.Assert(m_stream != null, "stream can not be null");			
			m_lPosition = 0;
			m_isClosed = false;
		}

		// --------------------------------------------------------------
		//	  Public properties
		// --------------------------------------------------------------

		// Always can read/write/seek, unless stream is null, 
		// which means the stream has been closed.

		public override bool CanRead {
			get
			{
				if (IsStreamClosed())			
					return false;
				return m_stream.CanRead;
			}
		}

		public override bool CanSeek {
			get {
				if (IsStreamClosed())			
					return false;
				return m_stream.CanSeek;
			}
		}

		public override bool CanWrite {
			get {
				if (IsStreamClosed())			
					return false;
				return m_stream.CanWrite;
			}
		}

		public override long Length {
			get {
				ThrowIfStreamClosed("get_Length");
				ThrowIfStreamCannotSeek("get_Length");
				return m_stream.Length;
			}
		}

		public override long Position {
			get {
				ThrowIfStreamClosed("get_Position");
				ThrowIfStreamCannotSeek("get_Position");
				return m_lPosition;
			}
			set {
				ThrowIfStreamClosed("set_Position");
				ThrowIfStreamCannotSeek("set_Position");
				if (value < 0 || value > m_stream.Length)
					throw new ArgumentOutOfRangeException("value");
				else
					m_lPosition = value;
			}
		}

		// --------------------------------------------------------------
		//	  Public methods
		// --------------------------------------------------------------

		public override long Seek(long offset, SeekOrigin origin) {
			long lPosition = 0;

			ThrowIfStreamClosed("Seek");
			ThrowIfStreamCannotSeek("Seek");
			switch(origin)	{
				
				case SeekOrigin.Begin:
					if (offset < 0 || offset > m_stream.Length)
						throw new ArgumentOutOfRangeException("offset");
					m_lPosition = offset;
					break;
					
				case SeekOrigin.Current:
					lPosition = m_lPosition + offset;
					if (lPosition < 0 || lPosition > m_stream.Length)
						throw new ArgumentOutOfRangeException("offset");
					m_lPosition = lPosition;
					break;
					
				case SeekOrigin.End:
					lPosition = m_stream.Length + offset;
					if (lPosition < 0 || lPosition > m_stream.Length)
						throw new ArgumentOutOfRangeException("offset");
					m_lPosition = lPosition;
					break;
					
				default:
					throw ADP.InvalidSeekOrigin("offset");
			}

			return m_lPosition;
		}

		public override int Read(byte[] buffer, int offset, int count) {
			ThrowIfStreamClosed("Read");
			ThrowIfStreamCannotRead("Read");
			
			if (buffer==null)
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || count > buffer.Length - offset)
				throw new ArgumentOutOfRangeException("count");
			
			if (m_stream.CanSeek && m_stream.Position != m_lPosition)
				m_stream.Seek(m_lPosition, SeekOrigin.Begin);

			int iBytesRead = (int) m_stream.Read(buffer, offset, count);
			m_lPosition += iBytesRead;

			return iBytesRead;
		}

		public override void Write(byte[] buffer, int offset, int count) {
			ThrowIfStreamClosed("Write");
			ThrowIfStreamCannotWrite("Write");
			if (buffer==null)
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || count > buffer.Length - offset)
				throw new ArgumentOutOfRangeException("count");

			if (m_stream.CanSeek && m_stream.Position != m_lPosition)
				m_stream.Seek(m_lPosition, SeekOrigin.Begin);

			m_stream.Write(buffer, offset, count);
			m_lPosition += count;
		}

		public override int ReadByte() {
			ThrowIfStreamClosed("ReadByte");
			ThrowIfStreamCannotRead("ReadByte");
			// If at the end of stream, return -1, rather than call ReadByte,
			// which will throw exception. This is the behavior for Stream.
			//
			if (m_stream.CanSeek && m_lPosition >= m_stream.Length)
				return -1;
			
			if (m_stream.CanSeek && m_stream.Position != m_lPosition)
				m_stream.Seek(m_lPosition, SeekOrigin.Begin);

			int ret = m_stream.ReadByte();
			m_lPosition ++;
			return ret;
		}

		public override void WriteByte(byte value) {
			ThrowIfStreamClosed("WriteByte");
			ThrowIfStreamCannotWrite("WriteByte");
			if (m_stream.CanSeek && m_stream.Position != m_lPosition)
				m_stream.Seek(m_lPosition, SeekOrigin.Begin);
			m_stream.WriteByte(value);
			m_lPosition ++;
		}

		public override void SetLength(long value) {
			ThrowIfStreamClosed("SetLength");
			ThrowIfStreamCannotSeek("SetLength");

			m_stream.SetLength(value);
			if (m_lPosition > value)
				m_lPosition = value;
		}

		public override void Flush() {			
			if (m_stream != null)
				m_stream.Flush();
		}
					
        protected override void Dispose(bool disposing) {
            try {
                // does not close the underline stream but mark itself as closed
                m_isClosed = true;
            }
            finally {
                base.Dispose(disposing);
            }
        }

		private void ThrowIfStreamCannotSeek(string method) {
			if (!m_stream.CanSeek)			
				throw new NotSupportedException(SQLResource.InvalidOpStreamNonSeekable(method));
		}			

		private void ThrowIfStreamCannotRead(string method) {
			if (!m_stream.CanRead)			
				throw new NotSupportedException(SQLResource.InvalidOpStreamNonReadable(method));
		}			

		private void ThrowIfStreamCannotWrite(string method) {
			if (!m_stream.CanWrite)			
				throw new NotSupportedException(SQLResource.InvalidOpStreamNonWritable(method));
			}			

		private void ThrowIfStreamClosed(string method) {
			if (IsStreamClosed())
				throw new ObjectDisposedException(SQLResource.InvalidOpStreamClosed(method));
		}			

		private bool IsStreamClosed() {
			// Check the .CanRead and .CanWrite and .CanSeek properties to make sure stream is really closed
			
			if (m_isClosed || m_stream == null || (!m_stream.CanRead && !m_stream.CanWrite && !m_stream.CanSeek))
				return true;
			else
				return false;				
		}			
	} // class SqlXmlStreamWrapper

}

