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

namespace FirebirdSql.Data.Common
{
	internal sealed class EventParameterBuffer : ParameterBuffer
	{
		#region Constructors

		public EventParameterBuffer() : base(true)
		{
		}

		#endregion

		#region Methods

		public void Append(string content, int actualCount)
		{
			this.Append(Encoding.Default.GetBytes(content), actualCount);
		}

		public void Append(byte[] content, int actualCount)
		{
			this.WriteByte(content.Length);
			this.Write(content);
			this.Write(actualCount);
		}

		#endregion
	}
}