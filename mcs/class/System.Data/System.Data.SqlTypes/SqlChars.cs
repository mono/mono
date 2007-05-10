//
// System.Data.SqlTypes.SqlChars
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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace System.Data.SqlTypes
{
	[SerializableAttribute]
	[XmlSchemaProvider ("GetSchema")]
	public sealed class SqlChars : INullable, IXmlSerializable, ISerializable
	{
		#region Fields

		bool notNull;
		char [] buffer;
		StorageState storage = StorageState.UnmanagedBuffer;

		#endregion

		#region Constructors

		public SqlChars ()
		{
			notNull = false;
			buffer = null;
		}

		public SqlChars (char[] buffer)
		{
			if (buffer == null) {
				notNull = false;
				this.buffer = null;
			} else {
				notNull = true;
				this.buffer = buffer;
				storage = StorageState.Buffer;
			}
		}

		public SqlChars (SqlString value)
		{
			if (value == null) {
				notNull = false;
				buffer = null;
			} else {
				notNull = true;
				buffer = value.Value.ToCharArray ();
				storage = StorageState.Buffer;
			}
		}

		#endregion

		#region Properties

                public char [] Buffer {
                        get { return buffer; }
                }

                public bool IsNull {
                        get { return !notNull; }
                }

                public char this [long offset] {
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

                public static SqlChars Null {
                        get {
				return new SqlChars ();
			}
                }

                public StorageState Storage {
                        get {
				if (storage == StorageState.UnmanagedBuffer)
					throw new SqlNullValueException ("Data is Null");
				return storage;
			}
                }

                public char [] Value {
                        get {
				if (buffer == null)
					return buffer;
				return (char []) buffer.Clone ();
			}
                }

		#endregion

		#region Methods

		public void SetLength (long value)
		{
			if (buffer == null)
				throw new SqlTypeException ("There is no buffer");
			if (value < 0 || value > buffer.Length)
				throw new ArgumentOutOfRangeException ("Specified argument was out of the range of valid values.");
			Array.Resize (ref buffer, (int) value);
		}
                                                                                
		public void SetNull ()
		{
			buffer = null;
			notNull = false;
		}

		public SqlString ToSqlString ()
		{
			if (buffer == null) {
				return SqlString.Null;
			}
			else {
				return new SqlString (buffer.ToString ());
			}
		}
                                                              
		[MonoTODO]
		public long Read (long offset, char [] buffer, int offsetInBuffer, int count)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported("")]
		public void Write (long offset, char [] buffer, int offsetInBuffer, int count)
		{
			throw new NotImplementedException ();
		}

		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			XmlQualifiedName qualifiedName = new XmlQualifiedName ("string", "http://www.w3.org/2001/XMLSchema");
			return qualifiedName;
		}
		
		[MonoTODO]
		XmlSchema IXmlSerializable.GetSchema ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		void IXmlSerializable.ReadXml (XmlReader reader)
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
                                                                                
		#endregion
	}
}

#endif
