/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.IO;
using System.Text;
using System.Net;

namespace FirebirdSql.Data.Common
{
	internal abstract class ParameterBuffer
	{
		#region Fields

		private MemoryStream stream;
		private bool isLittleEndian;

		#endregion

		#region Properties

		public short Length
		{
			get { return (short)this.ToArray().Length; }
		}

		#endregion

		#region Protected properties

		protected bool IsLittleEndian
		{
			get { return this.isLittleEndian; }
		}

		#endregion

		#region Constructors

		protected ParameterBuffer() : this(false)
		{
		}

		protected ParameterBuffer(bool isLittleEndian)
		{
			this.stream = new MemoryStream();
			this.isLittleEndian = isLittleEndian;
		}

		#endregion

		#region Protected Methods

		protected void WriteByte(int value)
		{
			this.WriteByte((byte)value);
		}

		protected void WriteByte(byte value)
		{
			this.stream.WriteByte(value);
		}

		protected void Write(short value)
		{
			if (!this.IsLittleEndian)
			{
				value = (short)IPAddress.NetworkToHostOrder(value);
			}

			byte[] buffer = BitConverter.GetBytes(value);

			this.stream.Write(buffer, 0, buffer.Length);
		}

		protected void Write(int value)
		{
			if (!this.IsLittleEndian)
			{
				value = (int)IPAddress.NetworkToHostOrder(value);
			}

			byte[] buffer = BitConverter.GetBytes(value);

			this.stream.Write(buffer, 0, buffer.Length);
		}

		protected void Write(byte[] buffer)
		{
			this.Write(buffer, 0, buffer.Length);
		}

		protected void Write(byte[] buffer, int offset, int count)
		{
			this.stream.Write(buffer, offset, count);
		}

		#endregion

		#region Methods

		public virtual void Append(int type)
		{
			this.WriteByte(type);
		}

		public byte[] ToArray()
		{
			return stream.ToArray();
		}

		#endregion
	}
}
