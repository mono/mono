//
// System.Data.SqlTypes.SqlBytes
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2003
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

#if NET_2_0

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace System.Data.SqlTypes
{
	[SerializableAttribute]
	[XmlSchemaProvider ("GetXsdType")]
	public sealed class SqlBytes : INullable, IXmlSerializable, ISerializable
	{
		#region Fields

		bool notNull;
		byte [] buffer;
		StorageState storage = StorageState.UnmanagedBuffer;
		Stream stream = null;

		#endregion

		#region Constructors

		public SqlBytes ()
		{
			buffer = null;
			notNull = false;
		}

		public SqlBytes (byte[] buffer)
		{
			if (buffer == null) {
				notNull = false;
				buffer = null;
			}
			else {
				notNull = true;
				this.buffer = buffer;
				storage = StorageState.Buffer;
			}
		}

		public SqlBytes (SqlBinary value)
		{
			if (value.IsNull) {
				notNull = false;
				buffer = null;
			}
			else {
				notNull = true;
				buffer = value.Value;
				storage = StorageState.Buffer;
			}
		}

		public SqlBytes (Stream s)
		{
			if (s == null) {
				notNull = false;
				buffer = null;
			} else {
				notNull = true;
				int len = (int) s.Length;
				buffer = new byte [len];
				s.Read (buffer, 0, len);
				storage = StorageState.Stream;
				stream = s;
			}
		}

		#endregion

		#region Properties

		public byte [] Buffer {
			get { return buffer; }
		}

		public bool IsNull {
			get { return !notNull; }
		}

		public byte this [long offset] {
			set {
				if (notNull && offset >= 0 && offset < buffer.Length)
					buffer [offset] = value;
			}
			get {
				if (buffer == null)
					throw new SqlNullValueException ("Data is Null");
				if (offset < 0 || offset >= buffer.Length)
					throw new ArgumentOutOfRangeException ("Parameter name: offset");
				return buffer [offset];
			}
		}

		public long Length {
			get {
				if (!notNull || buffer == null)
					throw new SqlNullValueException ("Data is Null");
				if (buffer.Length < 0)
					return -1;
				return buffer.Length;
			}
		}

		public long MaxLength {
			get {
				if (!notNull || buffer == null || storage == StorageState.Stream)
					return -1;
				return buffer.Length;
			}
		}

		public static SqlBytes Null {
			get {
				return new SqlBytes ();
			}
		}

		public StorageState Storage {
			get {
				if (storage == StorageState.UnmanagedBuffer)
					throw new SqlNullValueException ("Data is Null");
				return storage;
			}
		}

		public Stream Stream {
			set {
				stream = value;
			}
			get {
				return stream;
			}
		}

		public byte [] Value {
			get {
				if (buffer == null)
					return buffer;
				return (byte []) buffer.Clone ();
			}
		}

		#endregion

		#region Methods

		public void SetLength (long value)
		{
			if (buffer == null)
				throw new SqlTypeException ("There is no buffer. Read or write operation failed.");
			if (value < 0 || value > buffer.Length)
				throw new ArgumentOutOfRangeException ("Specified argument was out of the range of valid values.");
			Array.Resize (ref buffer, (int) value);
		}

		public void SetNull ()
		{
			buffer = null;
			notNull = false;
		}

		public static explicit operator SqlBytes (SqlBinary value)
		{
			if (value.IsNull)
				return Null;
			else
				return new SqlBytes (value.Value);
		}

		public static explicit operator SqlBinary (SqlBytes value)
		{
			if (value.IsNull)
				return SqlBinary.Null;
			else
				return new SqlBinary (value.Value);
		}

		public SqlBinary ToSqlBinary ()
		{
			return new SqlBinary (buffer);
		}

		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			XmlQualifiedName qualifiedName = new XmlQualifiedName ("base64Binary", "http://www.w3.org/2001/XMLSchema");
			return qualifiedName;
		}
		
		[MonoTODO]
		XmlSchema IXmlSerializable.GetSchema ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IXmlSerializable.ReadXml (XmlReader r)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public long Read (long offset, byte [] buffer, int offsetInBuffer, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (IsNull)
				throw new SqlNullValueException ("There is no buffer. Read or write failed");
			
			if (count > MaxLength || count > buffer.Length || 
			    count < 0 || ((offsetInBuffer + count) > buffer.Length))
				throw new ArgumentOutOfRangeException ("count");
			
			if (offset < 0 || offset > MaxLength)
				throw new ArgumentOutOfRangeException ("offset");
			
			if (offsetInBuffer < 0 || offsetInBuffer > buffer.Length)
				throw new ArgumentOutOfRangeException ("offsetInBuffer");
			
			/* Final count of what will be copied */
			long actualCount = count;
			if (count + offset > Length )
				actualCount = Length - offset;
			
			Array.Copy (this.buffer, offset, buffer, offsetInBuffer, actualCount);
			
			return actualCount;
		}

		public void Write (long offset, byte [] buffer, int offsetInBuffer, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (IsNull)
				throw new SqlTypeException ("There is no buffer. Read or write operation failed.");
							
			if (offset < 0) 
				throw new ArgumentOutOfRangeException ("offset");
			
			if (offsetInBuffer < 0 || offsetInBuffer > buffer.Length 
			    || offsetInBuffer > Length 
			    || offsetInBuffer + count > Length
			    || offsetInBuffer + count > buffer.Length)
				throw new ArgumentOutOfRangeException ("offsetInBuffer");
			
			if (count < 0 || count > MaxLength)
				throw new ArgumentOutOfRangeException ("count");
			
			if (offset > MaxLength || offset+count > MaxLength)
				throw new SqlTypeException ("The buffer is insufficient. Read or write operation failed.");
			
			if (count + offset > Length && 
			    count + offset <= MaxLength)
				SetLength (count);
			
			Array.Copy (buffer, offsetInBuffer, this.buffer, offset, count);
		}

		#endregion
	}
}

#endif
