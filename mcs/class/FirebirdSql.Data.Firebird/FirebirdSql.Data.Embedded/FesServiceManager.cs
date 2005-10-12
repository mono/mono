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
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Embedded
{
	internal sealed class FesServiceManager : IServiceManager
	{
		#region Fields

		private int handle;

		#endregion

		#region Properties

		public int Handle
		{
			get { return this.handle; }
		}

		public bool IsLittleEndian
		{
			get { return BitConverter.IsLittleEndian; }
		}

		#endregion

		#region Constructors

		public FesServiceManager()
		{
		}

		#endregion

		#region Methods

		public void Attach(ServiceParameterBuffer spb, string dataSource, int port, string service)
		{
			int[] statusVector = FesConnection.GetNewStatusVector();
			int svcHandle = this.Handle;

			FbClient.isc_service_attach(
				statusVector,
				(short)service.Length,
				service,
				ref	svcHandle,
				(short)spb.Length,
				spb.ToArray());

			// Parse status	vector
			this.ParseStatusVector(statusVector);

			// Update status vector
			this.handle = svcHandle;
		}

		public void Detach()
		{
			int[] statusVector = FesConnection.GetNewStatusVector();
			int svcHandle = this.Handle;

			FbClient.isc_service_detach(statusVector, ref svcHandle);

			// Parse status	vector
			this.ParseStatusVector(statusVector);

			// Update status vector
			this.handle = svcHandle;
		}

		public void Start(ServiceParameterBuffer spb)
		{
			int[] statusVector = FesConnection.GetNewStatusVector();
			int svcHandle = this.Handle;
			int reserved = 0;

			FbClient.isc_service_start(
				statusVector,
				ref	svcHandle,
				ref	reserved,
				(short)spb.Length,
				spb.ToArray());

			// Parse status	vector
			this.ParseStatusVector(statusVector);
		}

		public void Query(
			ServiceParameterBuffer spb,
			int requestLength,
			byte[] requestBuffer,
			int bufferLength,
			byte[] buffer)
		{
			int[] statusVector = FesConnection.GetNewStatusVector();
			int svcHandle = this.Handle;
			int reserved = 0;

			FbClient.isc_service_query(
				statusVector,
				ref	svcHandle,
				ref	reserved,
				(short)spb.Length,
				spb.ToArray(),
				(short)requestLength,
				requestBuffer,
				(short)buffer.Length,
				buffer);

			// Parse status	vector
			this.ParseStatusVector(statusVector);
		}

		#endregion

		#region Buffer Creation	methods

		public ServiceParameterBuffer CreateParameterBuffer()
		{
			return new ServiceParameterBuffer();
		}

		#endregion

		#region Private	Methods

		private void ParseStatusVector(int[] statusVector)
		{
			IscException ex = FesConnection.ParseStatusVector(statusVector);

			if (ex != null && !ex.IsWarning)
			{
				throw ex;
			}
		}

		#endregion
	}
}
