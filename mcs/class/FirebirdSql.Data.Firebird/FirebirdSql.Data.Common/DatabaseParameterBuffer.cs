/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.IO;
using System.Text;
using System.Net;

namespace FirebirdSql.Data.Common
{
	internal sealed class DatabaseParameterBuffer : ParameterBuffer
	{
		#region Constructors

		public DatabaseParameterBuffer() : base()
		{
		}

		public DatabaseParameterBuffer(bool isLittleEndian) : base(isLittleEndian)
		{
		}

		#endregion

		#region Methods

		public void Append(int type, byte value)
		{
			this.WriteByte(type);
			this.WriteByte(1);
			this.Write(value);
		}

		public void Append(int type, short value)
		{
			this.WriteByte(type);
			this.WriteByte(2);
			this.Write(value);
		}

		public void Append(int type, int value)
		{
			this.WriteByte(type);
			this.WriteByte((byte)4);
			this.Write(value);
		}

		public void Append(int type, string content)
		{
			this.Append(type, Encoding.Default.GetBytes(content));
		}

		public void Append(int type, byte[] buffer)
		{
			this.WriteByte(type);
			this.WriteByte(buffer.Length);
			this.Write(buffer);
		}

		#endregion
	}
}
